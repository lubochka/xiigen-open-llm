// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;

// FeedbackRouter: converts user feedback signals into training data.
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
public sealed class FeedbackRouter : BaseService, IFeedbackRouter
{
    // Cache where positive examples are accumulated.
    private readonly IDomainDatasetCache _cache;

    // DPO pairs accumulated since last IncrementalTrainer run.
    private readonly List<DpoPair> _dpoPairs = new();

    public FeedbackRouter(ISessionStorage storage, IDomainDatasetCache cache)
        : base(storage)
        => _cache = cache;

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

    // Return all DPO pairs accumulated since the last training run.
    public OperationResult<IReadOnlyList<DpoPair>> GetDpoPairs()
        => OperationResult<IReadOnlyList<DpoPair>>.Ok(_dpoPairs.AsReadOnly());

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

        _dpoPairs.Add(new DpoPair(
            feedback.Prompt,
            feedback.UserCorrection,
            feedback.ModelResponse,
            feedback.Domain,
            DateTimeOffset.UtcNow));

        var example = new TrainingExample(
            feedback.Prompt, feedback.UserCorrection, feedback.Domain, 1.0, DateTimeOffset.UtcNow);
        return _cache.Add(example);
    }

    // REJECTED: model response was wrong → DPO pair with empty chosen.
    private OperationResult<bool> RouteRejected(FeedbackEvent feedback)
    {
        _dpoPairs.Add(new DpoPair(
            feedback.Prompt,
            "[no preferred response]",
            feedback.ModelResponse,
            feedback.Domain,
            DateTimeOffset.UtcNow));
        return OperationResult<bool>.Ok(true);
    }
}
