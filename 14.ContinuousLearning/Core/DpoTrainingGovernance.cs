// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Collections.ObjectModel;
using System.Runtime.ExceptionServices;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Immutable capture-time decision for training-data licensing and quarantine.
/// Evidence references are snapshotted so caller mutation cannot alter admission.
/// </summary>
public sealed class TrainingDataGovernanceDecision
    : IEquatable<TrainingDataGovernanceDecision>
{
    private readonly ReadOnlyCollection<string> _evidenceRefs;

    [System.Text.Json.Serialization.JsonConstructor]
    public TrainingDataGovernanceDecision(
        bool licenseAccepted,
        bool isQuarantined,
        IReadOnlyList<string>? evidenceRefs)
    {
        LicenseAccepted = licenseAccepted;
        IsQuarantined = isQuarantined;
        _evidenceRefs = Array.AsReadOnly(evidenceRefs?.Take(33).ToArray() ?? []);
    }

    public TrainingDataGovernanceDecision(
        bool licenseAccepted,
        bool isQuarantined,
        IEnumerable<string>? evidenceRefs)
        : this(licenseAccepted, isQuarantined, evidenceRefs?.Take(33).ToArray())
    {
    }

    public bool LicenseAccepted { get; }
    public bool IsQuarantined { get; }
    public IReadOnlyList<string> EvidenceRefs => _evidenceRefs;

    public bool Equals(TrainingDataGovernanceDecision? other)
        => other is not null
           && LicenseAccepted == other.LicenseAccepted
           && IsQuarantined == other.IsQuarantined
           && _evidenceRefs.SequenceEqual(other._evidenceRefs, StringComparer.Ordinal);

    public override bool Equals(object? obj)
        => obj is TrainingDataGovernanceDecision other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(LicenseAccepted);
        hash.Add(IsQuarantined);
        foreach (var evidenceRef in _evidenceRefs)
            hash.Add(evidenceRef, StringComparer.Ordinal);
        return hash.ToHashCode();
    }
}

internal static class DpoPairBatchSnapshot
{
    internal static OperationResult<IReadOnlyList<DpoPair>> Capture(
        IEnumerable<DpoPair>? source)
    {
        if (source is null)
        {
            return OperationResult<IReadOnlyList<DpoPair>>.Fail(
                "DPO pair input is required.",
                "DPO_PAIR_INPUT_REQUIRED");
        }

        IEnumerator<DpoPair> enumerator;
        try
        {
            enumerator = source.GetEnumerator();
            if (enumerator is null)
                throw new InvalidOperationException("DPO_PAIR_ENUMERATOR_REQUIRED");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return EnumerationFailure();
        }

        OperationResult<IReadOnlyList<DpoPair>>? result = null;
        ExceptionDispatchInfo? primaryFatalException = null;
        var snapshot = new List<DpoPair>(
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows);
        try
        {
            while (enumerator.MoveNext())
            {
                if (snapshot.Count == LegalTrainingTeacherAdmissionAuthority.MaxBatchRows)
                {
                    result = OperationResult<IReadOnlyList<DpoPair>>.Fail(
                        "The DPO batch exceeds the legal admission bound.",
                        LegalTrainingTeacherAdmissionAuthority.BatchLimitExceededErrorCode);
                    break;
                }

                snapshot.Add(enumerator.Current);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            result = EnumerationFailure();
        }
        catch (Exception exception)
        {
            primaryFatalException = ExceptionDispatchInfo.Capture(exception);
        }

        ExceptionDispatchInfo? disposeFatalException = null;
        try
        {
            enumerator.Dispose();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (primaryFatalException is null)
                result = EnumerationFailure();
        }
        catch (Exception exception)
        {
            disposeFatalException = ExceptionDispatchInfo.Capture(exception);
        }

        if (primaryFatalException is not null && disposeFatalException is not null)
        {
            throw new AggregateException(
                "DPO pair enumeration and enumerator cleanup both failed fatally.",
                primaryFatalException.SourceException,
                disposeFatalException.SourceException);
        }

        disposeFatalException?.Throw();
        primaryFatalException?.Throw();
        return result
               ?? OperationResult<IReadOnlyList<DpoPair>>.Ok(
                   Array.AsReadOnly(snapshot.ToArray()));
    }

    private static OperationResult<IReadOnlyList<DpoPair>> EnumerationFailure()
        => OperationResult<IReadOnlyList<DpoPair>>.Fail(
            "DPO pairs could not be enumerated safely.",
            LegalTrainingTeacherAdmissionAuthority.EnumerationFailedErrorCode);
}

public static class DpoPairTrainingContentAuthority
{
    internal static OperationResult<DpoTrainingContentBatch> Project(
        IReadOnlyList<DpoPair>? pairs)
    {
        if (pairs is null || pairs.Count == 0 ||
            pairs.Count > BoundedDpoTrainingContentProjector.MaximumRowCount)
        {
            return OperationResult<DpoTrainingContentBatch>.Fail(
                "A bounded DPO pair batch is required.",
                BoundedDpoTrainingContentProjector.LimitExceededErrorCode);
        }

        try
        {
            var inputs = new DpoTrainingContentRowInput[pairs.Count];
            for (var index = 0; index < pairs.Count; index++)
            {
                var pair = pairs[index];
                if (pair is null || pair.Domain is null ||
                    System.Text.Encoding.UTF8.GetByteCount(pair.Domain) >
                        BoundedDpoTrainingContentProjector
                            .MaximumContextUtf8Bytes)
                {
                    return OperationResult<DpoTrainingContentBatch>.Fail(
                        "A bounded DPO pair domain is required.",
                        BoundedDpoTrainingContentProjector
                            .LimitExceededErrorCode);
                }
                inputs[index] = new DpoTrainingContentRowInput(
                    pair.Prompt,
                    pair.Chosen,
                    pair.Rejected,
                    TeachingVector: null,
                    Context: string.Concat(
                        "continuous-learning-dpo-v1|domain=",
                        pair.Domain));
            }
            return BoundedDpoTrainingContentProjector.Project(inputs);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DpoTrainingContentBatch>.Fail(
                "DPO pair content could not be projected safely.",
                BoundedDpoTrainingContentProjector.InvalidErrorCode);
        }
    }

    /// <summary>
    /// Public doorway for production authoring flows that create a rich DPO pair
    /// outside this assembly and must persist it through the repository-owned
    /// dataset sealer. It binds the teacher passport to the exact trainable
    /// content hash so later training can prove which bytes were used.
    /// </summary>
    public static OperationResult<DpoPair> Bind(DpoPair? pair)
    {
        if (pair is null)
        {
            return OperationResult<DpoPair>.Fail(
                "DPO pair is required.",
                "NULL_DPO_PAIR");
        }

        var content = Project([pair]);
        if (!content.IsSuccess || content.Value is null)
        {
            return OperationResult<DpoPair>.Fail(
                "DPO pair fields are invalid.",
                "INVALID_DPO_PAIR");
        }
        if (pair.TeacherProvenance is null)
        {
            return OperationResult<DpoPair>.Fail(
                "Typed teacher provenance is required for DPO content binding.",
                LegalTrainingTeacherAdmissionAuthority
                    .ProvenanceRequiredErrorCode);
        }
        var boundProvenance = pair.TeacherProvenance with
        {
            TrainingContentSha256 = content.Value.Rows[0].Sha256
        };
        var chosenAuthorship = BindSideAuthorship(
            pair.ChosenAuthorship,
            pair.TeacherProvenance,
            boundProvenance);
        if (!chosenAuthorship.IsSuccess)
            return chosenAuthorship.MapError<DpoPair>();
        var rejectedAuthorship = BindSideAuthorship(
            pair.RejectedAuthorship,
            pair.TeacherProvenance,
            boundProvenance);
        if (!rejectedAuthorship.IsSuccess)
            return rejectedAuthorship.MapError<DpoPair>();
        return OperationResult<DpoPair>.Ok(pair with
        {
            TeacherProvenance = boundProvenance,
            ChosenAuthorship = chosenAuthorship.Value,
            RejectedAuthorship = rejectedAuthorship.Value,
        });
    }

    private static OperationResult<DpoPairSideAuthorship?> BindSideAuthorship(
        DpoPairSideAuthorship? authorship,
        TrainingTeacherProvenance unboundProvenance,
        TrainingTeacherProvenance boundProvenance)
    {
        if (authorship is null ||
            authorship.AuthorKind is not DPO.Models.DpoPairSideAuthorKind.OpenTeacher)
        {
            return OperationResult<DpoPairSideAuthorship?>.Ok(authorship);
        }

        if (authorship.OpenTeacherPassport is null ||
            !DpoPairSideAuthorshipPolicy.SamePassportIdentity(
                authorship.OpenTeacherPassport,
                unboundProvenance,
                includeTrainingContentSha256: false))
        {
            return OperationResult<DpoPairSideAuthorship?>.Fail(
                "DPO pair open-teacher authorship does not match the pair passport.",
                "INVALID_DPO_PAIR");
        }

        // Привязка контента вписывает финальный digest в обе ссылки: сторона
        // OpenTeacher не должна указывать на старый metadata-only паспорт.
        return OperationResult<DpoPairSideAuthorship?>.Ok(
            new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                boundProvenance));
    }
}

internal static class TrainingDataGovernanceAdmission
{
    internal const string RequiredErrorCode = "TRAINING_DATA_GOVERNANCE_REQUIRED";
    internal const string LicenseDeniedErrorCode = "TRAINING_DATA_LICENSE_NOT_ACCEPTED";
    internal const string QuarantinedErrorCode = "TRAINING_DATA_QUARANTINED";
    internal const string EvidenceInvalidErrorCode = "TRAINING_DATA_GOVERNANCE_EVIDENCE_INVALID";

    internal static OperationResult<bool> Validate(
        IReadOnlyList<DpoPair> pairs)
    {
        foreach (var pair in pairs)
        {
            var admitted = ValidateDecision(pair.GovernanceDecision); // Preserve the existing per-pair order through the shared decision owner.
            if (!admitted.IsSuccess) return admitted; // The first original governance refusal remains the batch outcome.
        }

        return OperationResult<bool>.Ok(true);
    }

    internal static OperationResult<bool> ValidateDecision(TrainingDataGovernanceDecision? governance) // Material eligibility reuses the actual capture-time policy, without manufacturing a DPO pair.
    { // Keep the original required/license/quarantine/evidence order and messages.
        if (governance is null) // Absence cannot be replaced by a descriptive material digest.
        { // Report the existing required-decision refusal.
            return OperationResult<bool>.Fail( // Preserve the public batch policy's error wording.
                "Training-data governance is required for every DPO pair.", // Existing message intentionally retained for compatibility.
                RequiredErrorCode); // Existing required-decision code.
        } // End missing-decision handling.
        if (!governance.LicenseAccepted) // Capture-time licensing remains mandatory.
        { // Report the existing license refusal before quarantine/evidence.
            return OperationResult<bool>.Fail( // Preserve the original licensing outcome.
                "Training-data licensing was not accepted.", // Existing licensing message.
                LicenseDeniedErrorCode); // Existing licensing code.
        } // End licensing handling.
        if (governance.IsQuarantined) // A license flag does not release quarantine.
        { // Report the existing quarantine refusal.
            return OperationResult<bool>.Fail( // Preserve the original quarantine outcome.
                "Quarantined training data cannot be admitted.", // Existing quarantine message.
                QuarantinedErrorCode); // Existing quarantine code.
        } // End quarantine handling.
        if (governance.EvidenceRefs.Count == 0 // Preserve the nonempty evidence bound.
            || governance.EvidenceRefs.Count > 32 // Preserve the existing maximum, including material consumers.
            || governance.EvidenceRefs.Any(static evidenceRef => // Validate every ordered reference.
                !IsSha256Reference(evidenceRef)) // Reuse the existing strict lowercase sha256 reference syntax.
            || governance.EvidenceRefs.Distinct(StringComparer.Ordinal).Count() // Duplicates still refuse.
                != governance.EvidenceRefs.Count) // Keep the original ordinal uniqueness rule.
        { // Report the existing evidence refusal.
            return OperationResult<bool>.Fail( // Preserve the original evidence outcome.
                "Training-data governance evidence is invalid.", // Existing evidence message.
                EvidenceInvalidErrorCode); // Existing evidence code.
        } // End evidence handling.
        return OperationResult<bool>.Ok(true); // This validates supplied governance policy, not independent real-rights certification.
    } // End the shared single-decision policy.

    internal static bool IsSha256Reference(string? value)
    {
        const string prefix = "sha256:";
        if (value is null
            || value.Length != prefix.Length + 64
            || !value.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        return value.AsSpan(prefix.Length).ToString().All(static character =>
            character is >= '0' and <= '9' or >= 'a' and <= 'f');
    }
}
