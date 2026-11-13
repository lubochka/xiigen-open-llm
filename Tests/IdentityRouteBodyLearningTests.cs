// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using ProjectWikiDomain;

// FIX-IM transfer, plan 48-learned-route-body-identity.md, from IDENTITY-M1-01.
//
// Two steps decide a learned orchestration answer: the learned weights pick the route,
// then a nearest-neighbour search fetches the body to print under it. The search used
// to range over every learned pair, so a short pair from another route could win on
// token overlap and supply its plan under the chosen route's heading. The transfer
// confines that last search to the route already chosen.
//
// The two learned pairs below are verbatim from the experiment's own published state,
// docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/IDENTITY-M1-01/A/states.json
// (sha256 e74255093298c06374dc1a1e7d52aee57bf06da3d9eabf3ceb9915cd94397144), entry
// phase="after-lessons", flow_grounding_knn. The long prompt is kept at its full length
// on purpose: that length is half the reason the old search went wrong.
public sealed class IdentityRouteBodyLearningTests
{
    private const string ShortRouteId = "LAB-B";
    private const string LongRouteId = "LAB-A";

    // IQ1 of the experiment: the query whose route and body disagreed before the fix.
    private const string ApprovalQuery = "validate payment approval ledger";

    // IQ2 of the experiment: a query identical to the short pair's own prompt.
    private const string ReleaseQuery = "validate payment ledger release";

    private const string ShortPlanText =
        "Validate payment, persist the ledger, then release the receipt.";

    private const string LongPlanText =
        "Validate approval before writing the ledger; preserve the approval record.";

    [Fact]
    public void ChosenRouteDecidesTheBody_InsteadOfTheGloballyNearestPair()
    {
        var store = BuildStore();

        // What the old code did: the global nearest neighbour for this query is the
        // SHORT pair, because the long pair's 45 words dilute the overlap. Pinning it
        // here keeps the test honest - it fails if this premise ever stops holding.
        var global = store.FindNearest(ApprovalQuery, PairMode.Mode1LearnedFlowGrounding);
        Assert.NotNull(global);
        Assert.Equal("00-b", global!.PairId);
        Assert.Equal(ShortRouteId, global.FlowRef);

        // What the transfer does: inside the already-chosen route the answer is that
        // route's own pair, with its own plan text.
        var restricted = store.FindNearest(
            ApprovalQuery, PairMode.Mode1LearnedFlowGrounding, LongRouteId);
        Assert.NotNull(restricted);
        Assert.Equal("10-a", restricted!.PairId);
        Assert.Equal(LongRouteId, restricted.FlowRef);
        Assert.Equal(LongPlanText, restricted.Chosen.PlanText);

        // The two differ: that difference is the whole measured effect.
        Assert.NotEqual(global.PairId, restricted.PairId);
    }

    [Fact]
    public void GroundedRouteAndDeliveredBodyAgree_OnTheExperimentsOwnQuery()
    {
        var model = new PromptFlowGroundingModel();
        foreach (var pair in LearnedPairs())
            model.Train(pair);

        Assert.Equal(2, model.KnnSize);
        Assert.Equal(2, model.WeightsSize);

        // The learned weights choose the long route for this query.
        var grounding = model.TryGround(ApprovalQuery, HumanIntentContract.Empty);
        Assert.NotNull(grounding);
        Assert.Equal(LongRouteId, grounding!.FlowRef);

        // The body handed back must belong to that same route, not to whichever pair
        // happens to sit nearest overall.
        var learned = model.FindLearnedPair(ApprovalQuery, HumanIntentContract.Empty, grounding);
        Assert.NotNull(learned);
        Assert.Equal(grounding.FlowRef, learned!.FlowRef);
        Assert.Equal("10-a", learned.PairId);
        Assert.Equal(LongPlanText, learned.Chosen.PlanText);
    }

    [Fact]
    public void UnchangedGlobalSearch_StillAnswersTheQueryThatMatchesItsOwnPair()
    {
        var store = BuildStore();

        // The historical two-argument search is untouched: same pair, same score.
        var nearest = store.FindNearestWithScore(ReleaseQuery, PairMode.Mode1LearnedFlowGrounding);
        Assert.NotNull(nearest);
        Assert.Equal("00-b", nearest!.Pair.PairId);
        Assert.Equal(1.0, nearest.Score);
        Assert.Equal(ShortPlanText, nearest.Pair.Chosen.PlanText);

        // A null route means exactly the historical behaviour, spelled out.
        var explicitlyGlobal = store.FindNearestWithScore(
            ReleaseQuery, PairMode.Mode1LearnedFlowGrounding, flowRef: null);
        Assert.NotNull(explicitlyGlobal);
        Assert.Equal(nearest.Pair.PairId, explicitlyGlobal!.Pair.PairId);
        Assert.Equal(nearest.Score, explicitlyGlobal.Score);
    }

    [Fact]
    public void UnknownOrEmptyRoute_ReturnsNothingInsteadOfAnotherRoutesPair()
    {
        var store = BuildStore();
        var model = BuildImportedModel();

        // A route with no learned pair yields nothing. It must not quietly widen back
        // to the global search - that is the failure this transfer removes.
        Assert.Null(store.FindNearest(
            ApprovalQuery, PairMode.Mode1LearnedFlowGrounding, "LAB-MISSING"));
        Assert.Null(model.FindLearnedPair(
            ApprovalQuery, HumanIntentContract.Empty, GroundingFor("LAB-MISSING")));

        // The empty string is a route name that no pair carries, not a wildcard.
        Assert.Null(store.FindNearest(
            ApprovalQuery, PairMode.Mode1LearnedFlowGrounding, string.Empty));

        // An empty query is still refused before the search, as before.
        Assert.Null(model.FindLearnedPair(
            string.Empty, HumanIntentContract.Empty, GroundingFor(LongRouteId)));
    }

    [Fact]
    public void RestrictionKeepsTheModeFilter_AndEachRouteAnswersFromItself()
    {
        var store = BuildStore();

        // Both predicates still apply: a wrong mode finds nothing even on a real route.
        Assert.Null(store.FindNearest(ReleaseQuery, PairMode.Mode2Generalization, ShortRouteId));

        // Each route returns its own pair for the same query text.
        var fromShort = store.FindNearest(
            ApprovalQuery, PairMode.Mode1LearnedFlowGrounding, ShortRouteId);
        var fromLong = store.FindNearest(
            ApprovalQuery, PairMode.Mode1LearnedFlowGrounding, LongRouteId);
        Assert.Equal("00-b", fromShort!.PairId);
        Assert.Equal("10-a", fromLong!.PairId);
        Assert.Equal(ShortPlanText, fromShort.Chosen.PlanText);
        Assert.Equal(LongPlanText, fromLong.Chosen.PlanText);
    }

    private static OrchestrationKnnStore BuildStore()
    {
        var store = new OrchestrationKnnStore();
        store.ImportPairs(LearnedPairs());
        return store;
    }

    // Loads the learned pairs without training, the way the plan's contract check asks:
    // the retrieval boundary is what is under test, not the weight building.
    private static PromptFlowGroundingModel BuildImportedModel()
    {
        var model = new PromptFlowGroundingModel();
        model.ImportState(
            LearnedPairs(),
            new Dictionary<string, OrchestrationFlowGroundingWeights>(StringComparer.Ordinal));
        return model;
    }

    // A grounding carrying only the chosen route: no evidence features, so the search
    // text stays exactly the prompt and the route acts purely as a filter.
    private static FlowGroundingResult GroundingFor(string flowRef)
        => new(flowRef, 0.95, [], [], [], []);

    private static IReadOnlyList<OrchestrationTrainingPair> LearnedPairs() =>
    [
        new OrchestrationTrainingPair(
            PairId: "00-b",
            Mode: PairMode.Mode1LearnedFlowGrounding,
            Prompt: ReleaseQuery,
            Chosen: new OrchestrationPlanChosen(
                ShortRouteId,
                "Receipt plan",
                "simulation:IDENTITY/00-b",
                ShortPlanText),
            RejectedText: "Release the receipt before validating payment.",
            ExpectedRouteMode: GenerationRouteMode.LearnedFlowGrounding,
            ExpectedBoundarySource: BoundarySource.Classifier,
            ExpectedMlGenerated: true,
            ExpectedIntentKind: IntentKind.ImplementationExecutionRequest,
            FlowRef: ShortRouteId,
            SourceBundleId: "simulation:IDENTITY/00-b",
            IsHeldOut: false),
        new OrchestrationTrainingPair(
            PairId: "10-a",
            Mode: PairMode.Mode1LearnedFlowGrounding,
            Prompt: "validate payment approval ledger archive audit compliance evidence journal "
                + "record retention reconciliation balance account transaction customer invoice "
                + "consent signature security trace backup version timestamp report settlement "
                + "currency receipt notification queue pending review deadline exception checksum "
                + "authorization persist recovery replica durability log history status monitoring "
                + "integrity",
            Chosen: new OrchestrationPlanChosen(
                LongRouteId,
                "Approval ledger plan",
                "simulation:IDENTITY/10-a",
                LongPlanText),
            RejectedText: "Write the ledger without validating approval.",
            ExpectedRouteMode: GenerationRouteMode.LearnedFlowGrounding,
            ExpectedBoundarySource: BoundarySource.Classifier,
            ExpectedMlGenerated: true,
            ExpectedIntentKind: IntentKind.ImplementationExecutionRequest,
            FlowRef: LongRouteId,
            SourceBundleId: "simulation:IDENTITY/10-a",
            IsHeldOut: false)
    ];
}
