// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DesignConversion.ModelActivation.Tests")]

namespace ContinuousLearning;

using AgentInfrastructure;
using DPO.Models;

// FeedbackRouter: durably converts user feedback signals into training data.
//
// Three routing rules:
//   ACCEPTED  → add response as positive TrainingExample (weight 1.0)
//   EDITED    → DpoPair(chosen=UserCorrection, rejected=ModelResponse)
//               + add UserCorrection as positive TrainingExample
//   REJECTED  → DpoPair(chosen="[no preferred response]", rejected=ModelResponse)
//               — rejected signal without correction still trains "avoid this"
//
// Weight formula (D-09 — dynamic, never a constant ×10):
//   maxWeight = totalExamples / (numDomains × rareExamples)
//   Example: 1000 examples, 5 domains, 50 security examples → maxWeight = 4.0
// FC-12: ISessionStorage first parameter → base(storage).
public sealed class FeedbackRouter : BaseService, IFeedbackRouter,
    IDpoPairTransactionCoordinator
{
    private const string NoPreferredResponse = "[no preferred response]";

    // Cache where positive examples are accumulated.
    private readonly IDomainDatasetCache _cache;

    // Durable pair storage rooted in server-owned configuration.
    private readonly FileDpoPairStore _dpoPairStore;

    [Obsolete(
        "Supply an explicit server-owned DPO pair storage root. "
        + "Edited/rejected feedback and pair reads fail closed without it.")]
    public FeedbackRouter(
        ISessionStorage storage,
        IDomainDatasetCache cache)
        : this(storage, cache, dpoPairStorageRoot: null)
    {
    }

    public FeedbackRouter(
        ISessionStorage storage,
        IDomainDatasetCache cache,
        string? dpoPairStorageRoot)
        : this(
            storage,
            cache,
            dpoPairStorageRoot,
            FileDpoPairStore.DefaultMaximumRecordCount,
            afterMutationLockAcquiredForTesting: null,
            mutationLockTimeoutForTesting: null)
    {
    }

    internal FeedbackRouter(
        ISessionStorage storage,
        IDomainDatasetCache cache,
        string? dpoPairStorageRoot,
        int maximumRecordCount,
        Action? afterMutationLockAcquiredForTesting = null,
        TimeSpan? mutationLockTimeoutForTesting = null,
        Func<IOException?>? mutationRegionLockFailureForTesting = null,
        bool corruptMutationLockLengthAfterAcquisitionForTesting = false,
        bool unsupportedPlatformForTesting = false,
        int? maximumTransactionPairCountForTesting = null)
        : base(storage)
    {
        _cache = cache;
        _dpoPairStore = new FileDpoPairStore(
            dpoPairStorageRoot,
            maximumRecordCount,
            afterMutationLockAcquiredForTesting,
            mutationLockTimeoutForTesting,
            mutationRegionLockFailureForTesting,
            corruptMutationLockLengthAfterAcquisitionForTesting,
            unsupportedPlatformForTesting,
            maximumTransactionPairCountForTesting:
                maximumTransactionPairCountForTesting);
    }

    // Route one feedback event to the appropriate training signal.
    public OperationResult<bool> Route(FeedbackEvent feedback)
    {
        if (feedback is null)
            return OperationResult<bool>.Fail("Feedback cannot be null.", "NULL_FEEDBACK");

        return feedback.Signal switch
        {
            FeedbackSignal.Accepted => RouteAccepted(feedback),
            FeedbackSignal.Edited   => RouteEdited(feedback),
            FeedbackSignal.Rejected => RouteRejected(feedback),
            _                       => OperationResult<bool>.Fail(
                                           "Unknown feedback signal.", "UNKNOWN_SIGNAL"),
        };
    }

    // Return only durable pairs which are still pending and not reserved by an
    // active or committed transaction.
    public OperationResult<IReadOnlyList<DpoPair>> GetDpoPairs()
        => _dpoPairStore.GetAll();

    OperationResult<FileDpoPairStore.TransactionHandle?>
        IDpoPairTransactionCoordinator.PrepareDpoTransaction(
            IReadOnlyList<string> domains)
        => _dpoPairStore.PrepareTransaction(domains);

    // D-09: dynamic max_weight — never a constant multiplier.
    // totalExamples / (numDomains × rareExamples) guards against catastrophic forgetting.
    public static double ComputeMaxWeight(int totalExamples, int numDomains, int rareExamples)
    {
        if (rareExamples <= 0) return 1.0;
        return totalExamples / ((double)numDomains * rareExamples);
    }

    // ACCEPTED: model response was good → positive example with standard weight.
    private OperationResult<bool> RouteAccepted(FeedbackEvent feedback)
    {
        var example = new TrainingExample(
            feedback.Prompt, feedback.ModelResponse, feedback.Domain, 1.0, DateTimeOffset.UtcNow);
        return _cache.Add(example);
    }

    // EDITED: user improved the response → DPO pair + positive example from correction.
    private OperationResult<bool> RouteEdited(FeedbackEvent feedback)
    {
        if (string.IsNullOrWhiteSpace(feedback.UserCorrection))
            return OperationResult<bool>.Fail(
                "Edited feedback requires UserCorrection.", "MISSING_CORRECTION");
        if (StringComparer.Ordinal.Equals(
                feedback.UserCorrection,
                NoPreferredResponse))
        {
            return OperationResult<bool>.Fail(
                "Edited feedback cannot use the no-preferred-response sentinel.",
                DpoPairSideAuthorshipPolicy.InvalidErrorCode);
        }

        var rejectedAuthorship = ResolveModelResponseAuthorship(feedback);
        if (!rejectedAuthorship.IsSuccess)
            return rejectedAuthorship.MapError<bool>();

        var pair = new DpoPair(
            feedback.Prompt,
            feedback.UserCorrection,
            feedback.ModelResponse,
            feedback.Domain,
            DateTimeOffset.UtcNow)
        {
            TeacherProvenance = feedback.TeacherProvenance,
            GovernanceDecision = feedback.GovernanceDecision,
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.User),
            RejectedAuthorship = rejectedAuthorship.Value,
        };
        var persisted = PersistTrainingReadyPair(pair);
        if (!persisted.IsSuccess)
            return persisted;

        var example = new TrainingExample(
            feedback.Prompt, feedback.UserCorrection, feedback.Domain, 1.0, DateTimeOffset.UtcNow);
        return _cache.Add(example);
    }

    // REJECTED: model response was wrong → DPO pair with empty chosen.
    private OperationResult<bool> RouteRejected(FeedbackEvent feedback)
    {
        if (!string.IsNullOrWhiteSpace(feedback.UserCorrection))
        {
            return OperationResult<bool>.Fail(
                "Rejected feedback cannot also carry a user correction.",
                DpoPairSideAuthorshipPolicy.InvalidErrorCode);
        }
        var rejectedAuthorship = ResolveModelResponseAuthorship(feedback);
        if (!rejectedAuthorship.IsSuccess)
            return rejectedAuthorship.MapError<bool>();

        var pair = new DpoPair(
            feedback.Prompt,
            NoPreferredResponse,
            feedback.ModelResponse,
            feedback.Domain,
            DateTimeOffset.UtcNow)
        {
            TeacherProvenance = feedback.TeacherProvenance,
            GovernanceDecision = feedback.GovernanceDecision,
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.DeterministicGenerator),
            RejectedAuthorship = rejectedAuthorship.Value,
        };
        return PersistTrainingReadyPair(pair);
    }

    private static OperationResult<DpoPairSideAuthorship>
        ResolveModelResponseAuthorship(FeedbackEvent feedback)
    {
        var provenance = feedback.TeacherProvenance;
        if (provenance is null)
        {
            return OperationResult<DpoPairSideAuthorship>.Ok(
                new DpoPairSideAuthorship(DpoPairSideAuthorKind.LocalModel));
        }

        if (!Enum.IsDefined(provenance.SourceKind))
        {
            return InvalidAuthorship<DpoPairSideAuthorship>();
        }

        if (provenance.SourceKind ==
            TrainingTeacherSourceKind.DeterministicNoExternalTeacher)
        {
            return OperationResult<DpoPairSideAuthorship>.Ok(
                new DpoPairSideAuthorship(DpoPairSideAuthorKind.LocalModel));
        }

        if (provenance.SourceKind ==
            TrainingTeacherSourceKind.ExternalTeacherOutput &&
            DpoPairSideAuthorshipPolicy.IsPermittedOpenTeacherPassport(
                provenance,
                requireTrainingContentSha256: false))
        {
            // OpenTeacher ставится только когда event уже несет валидный
            // passport; иначе router не имеет права делать "teacher" по ярлыку.
            return OperationResult<DpoPairSideAuthorship>.Ok(
                new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.OpenTeacher,
                    provenance));
        }

        return InvalidAuthorship<DpoPairSideAuthorship>();
    }

    private OperationResult<bool> PersistTrainingReadyPair(DpoPair pair)
    {
        var boundPair = DpoPairTrainingContentAuthority.Bind(pair);
        if (!boundPair.IsSuccess || boundPair.Value is null)
            return boundPair.MapError<bool>();
        var governance = TrainingDataGovernanceAdmission.Validate(
            Array.AsReadOnly([boundPair.Value]));
        if (!governance.IsSuccess)
            return governance;
        var persisted = _dpoPairStore.Add(boundPair.Value);
        if (!persisted.IsSuccess)
            return persisted.MapError<bool>();
        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<T> InvalidAuthorship<T>()
        => OperationResult<T>.Fail(
            "DPO pair side authorship is unknown or contradictory.",
            DpoPairSideAuthorshipPolicy.InvalidErrorCode);
}
