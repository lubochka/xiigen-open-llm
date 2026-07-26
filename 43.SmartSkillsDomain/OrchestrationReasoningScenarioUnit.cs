// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed record OrchestrationReasoningNearestQuery(
    string RequestId,
    string TaskType,
    IReadOnlyList<string> ConstraintSignals,
    IReadOnlyList<string> RequirementSignals,
    IReadOnlyList<string> DeliverableSignals,
    IReadOnlyList<string> EvidenceSignals,
    IReadOnlyDictionary<string, string> RuntimePolicyRefs);

public sealed record OrchestrationReasoningScenarioUnitPattern(
    string PatternId,
    string ScenarioRef,
    IReadOnlyList<string> RequiredTaskTypes,
    IReadOnlyList<string> RequiredConstraintSignals,
    IReadOnlyList<string> RequiredRequirementSignals,
    IReadOnlyList<string> RequiredDeliverableSignals,
    IReadOnlyList<string> RequiredEvidenceSignals,
    IReadOnlyList<string> PhaseFamilies,
    IReadOnlyList<string> GateFamilies,
    IReadOnlyList<string> AssetFamilies,
    IReadOnlyList<string> HostCheckSets,
    IReadOnlyList<string> RiskFlags,
    IReadOnlyList<string> NextGranularizationActions,
    double Confidence,
    string? AcceptedPairRef = null);

public sealed record OrchestrationReasoningNearestGuide(
    string PatternId,
    string ScenarioRef,
    int MatchedSignalCount,
    int TotalPatternSignalCount,
    double BaselineConfidence,
    double BaselineScore,
    IReadOnlyList<string> MatchedSignals,
    bool HostRuntimeClaimed,
    bool SemanticQualityClaimed,
    IReadOnlyList<string>? DecisionStructureRefs = null,
    string MatchKind = "exact_signal_contract");

public sealed record OrchestrationReasoningScenarioUnitState(
    int SchemaVersion,
    string StateId,
    string CheckpointRef,
    string ManifestRef,
    IReadOnlyList<OrchestrationReasoningScenarioUnitPattern> Patterns,
    IReadOnlyList<string> AcceptedPairRefs,
    IReadOnlyList<string> QuarantinedPairRefs,
    double MinimumConfidence,
    double MinimumTopTwoMargin,
    bool ContinueTrainingRunClaimed,
    bool CheckpointRewritten,
    bool HostRuntimeClaimed,
    bool SemanticQualityClaimed);

public sealed record OrchestrationReasoningScenarioDecision(
    bool IsReady,
    string? PatternId,
    string? ScenarioRef,
    OrchestrationReasoningScenarioUnitPattern? Pattern,
    int MatchedSignalCount,
    double Confidence,
    double TopTwoMargin,
    bool HostRuntimeClaimed,
    bool SemanticQualityClaimed,
    string BlockReason,
    IReadOnlyList<string> EvidenceSignals,
    IReadOnlyList<OrchestrationReasoningNearestGuide> NearestGuides,
    IReadOnlyList<string>? DecisionStructureRefs = null,
    string MatchKind = "not_ready");

public sealed record OrchestrationReasoningScenarioEvaluationRow(
    string RowId,
    OrchestrationReasoningRequest Request,
    string ExpectedPatternId,
    string ExpectedScenarioRef,
    IReadOnlyList<string>? ExpectedDecisionStructureRefs = null);

public sealed record OrchestrationReasoningScenarioEvaluationReport(
    int SchemaVersion,
    string StateId,
    int HeldOutRowCount,
    int CorrectCount,
    double Accuracy,
    bool PassesThresholds,
    double MinimumConfidence,
    double MinimumTopTwoMargin,
    bool HostRuntimeClaimed,
    bool SemanticQualityClaimed,
    string BlockReason,
    IReadOnlyList<string> DeterministicControlNotes,
    int StructureCorrectCount = 0,
    double StructureAccuracy = 0.0,
    string EvaluationMode = "exact_pattern_and_scenario_ref");

public sealed class OrchestrationReasoningScenarioUnit(
    Func<OrchestrationReasoningScenarioUnitState?> loadUserState,
    Func<OrchestrationReasoningScenarioUnitState?> loadCommonState)
    : ILogicalUnit<OrchestrationReasoningRequest, OrchestrationReasoningScenarioDecision>
{
    public const string UnitId = "orchestration_reasoning_scenario_unit";
    public const string UnitRole = "scenario_decision";

    private const double MinimumEvaluationAccuracy = 0.8;
    private const string ExactSignalMatchKind = "exact_signal_contract";
    private const string TransferableStructureMatchKind = "transferable_decision_structure_overlap_baseline";

    private static readonly HashSet<string> TransferableTokenStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "able",
        "about",
        "after",
        "agent",
        "allowed",
        "appears",
        "artifact",
        "artifacts",
        "because",
        "before",
        "being",
        "candidate",
        "choose",
        "chooses",
        "choosing",
        "chosen",
        "claim",
        "claimed",
        "claims",
        "code",
        "common",
        "copied",
        "current",
        "decision",
        "decisions",
        "deliverable",
        "deliverables",
        "deterministic",
        "dpo",
        "evidence",
        "exact",
        "external",
        "file",
        "files",
        "final",
        "forbidden",
        "from",
        "guide",
        "heldout",
        "host",
        "input",
        "ignore",
        "label",
        "learned",
        "local",
        "marker",
        "material",
        "model",
        "nearest",
        "only",
        "order",
        "output",
        "pair",
        "pairs",
        "path",
        "pattern",
        "patterns",
        "plausible",
        "plan",
        "plans",
        "prerequisite",
        "prerequisites",
        "project",
        "prompt",
        "quality",
        "reason",
        "reasons",
        "ready",
        "readiness",
        "ref",
        "refs",
        "rejected",
        "repair",
        "required",
        "requirement",
        "requirements",
        "route",
        "row",
        "rows",
        "runtime",
        "scenario",
        "semantic",
        "source",
        "scope",
        "state",
        "surface",
        "task",
        "title",
        "token",
        "tokens",
        "train",
        "trained",
        "training",
        "user",
        "using",
        "with",
        "without",
        "work"
    };

    public OrchestrationReasoningScenarioUnit(OrchestrationReasoningScenarioUnitState state)
        : this(() => state, () => null)
    {
    }

    public static OrchestrationReasoningScenarioUnitState FromLearnedState(
        OrchestrationReasoningLearnedGuideStateDescriptor learnedState,
        IReadOnlyList<string>? acceptedPairRefs = null,
        IReadOnlyList<string>? quarantinedPairRefs = null,
        double minimumConfidence = 0.01,
        double minimumTopTwoMargin = 0.01)
        => new(
            SchemaVersion: 1,
            StateId: learnedState.StateId,
            CheckpointRef: learnedState.GuideStateRef,
            ManifestRef: learnedState.ManifestId,
            Patterns: (learnedState.LearnedPatterns ?? [])
                .Select(pattern => new OrchestrationReasoningScenarioUnitPattern(
                    PatternId: pattern.PatternId,
                    ScenarioRef: pattern.ScenarioRef,
                    RequiredTaskTypes: pattern.RequiredTaskTypes,
                    RequiredConstraintSignals: pattern.RequiredConstraintSignals,
                    RequiredRequirementSignals: pattern.RequiredRequirementSignals,
                    RequiredDeliverableSignals: pattern.RequiredDeliverableSignals,
                    RequiredEvidenceSignals: pattern.RequiredEvidenceSignals,
                    PhaseFamilies: pattern.PhaseFamilies,
                    GateFamilies: pattern.GateFamilies,
                    AssetFamilies: pattern.AssetFamilies,
                    HostCheckSets: pattern.HostCheckSets,
                    RiskFlags: pattern.RiskFlags,
                    NextGranularizationActions: pattern.NextGranularizationActions,
                    Confidence: pattern.Confidence))
                .ToArray(),
            AcceptedPairRefs: acceptedPairRefs ?? [],
            QuarantinedPairRefs: quarantinedPairRefs ?? [],
            MinimumConfidence: minimumConfidence,
            MinimumTopTwoMargin: minimumTopTwoMargin,
            ContinueTrainingRunClaimed: false,
            CheckpointRewritten: false,
            HostRuntimeClaimed: false,
            SemanticQualityClaimed: false);

    public Task<OperationResult<LogicalUnitResult<OrchestrationReasoningScenarioDecision>>> ExecuteAsync(
        LogicalUnitRequest<OrchestrationReasoningRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<OrchestrationReasoningScenarioDecision>>.Fail("Unit cancelled.", "CANCELLED"));

        var decision = Decide(request.Input);
        if (!decision.IsSuccess)
            return Task.FromResult(decision.MapError<LogicalUnitResult<OrchestrationReasoningScenarioDecision>>());

        var value = decision.Value!;
        var result = new LogicalUnitResult<OrchestrationReasoningScenarioDecision>(
            UnitId: UnitId,
            UnitRole: UnitRole,
            DomainTag: OrchestrationReasoningMachine.DomainTag,
            Output: value,
            Confidence: value.Confidence,
            PredicateIds: value.PatternId is null ? [] : [value.PatternId],
            DomainTags: [OrchestrationReasoningMachine.DomainTag],
            EvidenceIds: value.EvidenceSignals.ToArray(),
            Warnings: value.IsReady
                ? ["BASELINE_NEAREST_SIGNAL_OVERLAP_NOT_SEMANTIC_QUALITY"]
                : [value.BlockReason]);

        return Task.FromResult(OperationResult<LogicalUnitResult<OrchestrationReasoningScenarioDecision>>.Ok(result));
    }

    public OperationResult<OrchestrationReasoningScenarioDecision> Decide(OrchestrationReasoningRequest request)
    {
        var state = LoadState();
        if (state is null)
            return OperationResult<OrchestrationReasoningScenarioDecision>.Ok(NotReady(
                "OrchestrationReasoningScenarioUnit state is not loaded.",
                [],
                0.0,
                0.0));

        if (state.SemanticQualityClaimed || state.HostRuntimeClaimed)
            return OperationResult<OrchestrationReasoningScenarioDecision>.Fail(
                "OrchestrationReasoningScenarioUnit does not accept semantic or host runtime claims in this slice.",
                "ORCHESTRATION_REASONING_SCENARIO_UNIT_CLAIM_UNSUPPORTED");

        var query = BuildQuery(request);
        var nearest = FindNearestOrchestrationGuidePatterns(query, state.Patterns.Count);
        var top = nearest.FirstOrDefault();
        var second = nearest.Skip(1).FirstOrDefault();
        var topTwoMargin = top is null
            ? 0.0
            : second is null
                ? top.BaselineConfidence
                : top.BaselineConfidence - second.BaselineConfidence;

        if (top is null || top.MatchedSignalCount <= 0)
            return OperationResult<OrchestrationReasoningScenarioDecision>.Ok(NotReady(
                "No OrchestrationReasoning baseline nearest guide matched structured task signals.",
                nearest,
                0.0,
                topTwoMargin));

        if (top.BaselineConfidence < state.MinimumConfidence)
            return OperationResult<OrchestrationReasoningScenarioDecision>.Ok(NotReady(
                "OrchestrationReasoning baseline confidence is below the unit threshold.",
                nearest,
                top.BaselineConfidence,
                topTwoMargin));

        if (topTwoMargin < state.MinimumTopTwoMargin)
            return OperationResult<OrchestrationReasoningScenarioDecision>.Ok(NotReady(
                "OrchestrationReasoning baseline top-two margin is below the unit threshold.",
                nearest,
                top.BaselineConfidence,
                topTwoMargin));

        var pattern = state.Patterns.First(pattern => string.Equals(pattern.PatternId, top.PatternId, StringComparison.Ordinal));
        if (string.Equals(top.MatchKind, TransferableStructureMatchKind, StringComparison.Ordinal)
            && CountDecisionTokenRefs(top.DecisionStructureRefs) < 2)
        {
            return OperationResult<OrchestrationReasoningScenarioDecision>.Ok(NotReady(
                "OrchestrationReasoning transferable structure overlap needs at least two decision-token anchors before plan output.",
                nearest,
                top.BaselineConfidence,
                topTwoMargin));
        }
        if (!string.Equals(top.MatchKind, ExactSignalMatchKind, StringComparison.Ordinal))
        {
            return OperationResult<OrchestrationReasoningScenarioDecision>.Ok(NotReady(
                "OrchestrationReasoning runtime planning requires an exact learned signal contract; transferable structure overlap remains eval-only baseline evidence.",
                nearest,
                top.BaselineConfidence,
                topTwoMargin));
        }

        return OperationResult<OrchestrationReasoningScenarioDecision>.Ok(new OrchestrationReasoningScenarioDecision(
            IsReady: true,
            PatternId: pattern.PatternId,
            ScenarioRef: pattern.ScenarioRef,
            Pattern: pattern,
            MatchedSignalCount: top.MatchedSignalCount,
            Confidence: top.BaselineConfidence,
            TopTwoMargin: topTwoMargin,
            HostRuntimeClaimed: false,
            SemanticQualityClaimed: false,
            BlockReason: "",
            EvidenceSignals: new[]
            {
                "ORCHESTRATION_REASONING_SCENARIO_UNIT_USED",
                "ORCHESTRATION_REASONING_BASELINE_NEAREST_SIGNAL_OVERLAP",
                "ORCHESTRATION_REASONING_TRANSFERABLE_STRUCTURE_BASELINE_NOT_SEMANTIC_QUALITY",
                "BASELINE_NOT_SEMANTIC_QUALITY",
                "HOST_RUNTIME_CLAIMED_FALSE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                $"ORCHESTRATION_REASONING_SCENARIO_UNIT_STATE:{state.StateId}",
                $"ORCHESTRATION_REASONING_SCENARIO_UNIT_MATCHED:{pattern.PatternId}",
                $"ORCHESTRATION_REASONING_SCENARIO_UNIT_MATCH_KIND:{top.MatchKind}",
                $"ORCHESTRATION_REASONING_SCENARIO_UNIT_MARGIN:{topTwoMargin:0.###}"
            }
            .Concat(top.DecisionStructureRefs ?? [])
            .Distinct(StringComparer.Ordinal)
            .ToArray(),
            NearestGuides: nearest,
            DecisionStructureRefs: top.DecisionStructureRefs ?? [],
            MatchKind: top.MatchKind));
    }

    public IReadOnlyList<OrchestrationReasoningNearestGuide> FindNearestOrchestrationGuidePatterns(
        OrchestrationReasoningNearestQuery query,
        int k)
    {
        var state = LoadState();
        if (state is null || k <= 0)
            return [];

        var requestSignals = BuildSignalSet(query);
        var requestContentTokens = BuildTransferableSignalTokenSet(
            query.ConstraintSignals
                .Concat(query.RequirementSignals)
                .Concat(query.DeliverableSignals)
                .Concat(query.EvidenceSignals));
        return state.Patterns
            .Select(pattern => BuildNearestGuide(pattern, requestSignals, requestContentTokens))
            .Where(guide => guide.MatchedSignalCount > 0)
            .OrderByDescending(guide => guide.BaselineConfidence)
            .ThenByDescending(guide => guide.MatchedSignalCount)
            .ThenBy(guide => guide.PatternId, StringComparer.Ordinal)
            .Take(k)
            .ToArray();
    }

    public OrchestrationReasoningScenarioEvaluationReport EvaluateHeldOutRows(
        IReadOnlyList<OrchestrationReasoningScenarioEvaluationRow> heldOutRows)
    {
        var state = LoadState();
        var stateId = state?.StateId ?? "no-state";
        var structureMode = heldOutRows.Any(row => row.ExpectedDecisionStructureRefs?.Count > 0);
        var correct = 0;
        var structureCorrect = 0;
        foreach (var row in heldOutRows)
        {
            if (structureMode)
            {
                var expectedStructureRefs = row.ExpectedDecisionStructureRefs ?? [];
                var nearest = FindNearestOrchestrationGuidePatterns(
                    BuildQuery(row.Request),
                    Math.Max(1, state?.Patterns.Count ?? 1));
                var observedStructureRefs = nearest
                    .SelectMany(guide => guide.DecisionStructureRefs ?? [])
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                var exactScenarioReplay = nearest.Any(guide => string.Equals(
                    guide.ScenarioRef,
                    row.ExpectedScenarioRef,
                    StringComparison.Ordinal));
                if (expectedStructureRefs.Count > 0
                    && !exactScenarioReplay
                    && expectedStructureRefs.Any(expected => observedStructureRefs.Contains(expected, StringComparer.Ordinal)))
                {
                    structureCorrect++;
                    correct++;
                }

                continue;
            }

            var decision = Decide(row.Request).Value!;
            if (decision.IsReady
                && string.Equals(decision.PatternId, row.ExpectedPatternId, StringComparison.Ordinal)
                && string.Equals(decision.ScenarioRef, row.ExpectedScenarioRef, StringComparison.Ordinal))
            {
                correct++;
            }
        }

        var accuracy = heldOutRows.Count == 0 ? 0.0 : (double)correct / heldOutRows.Count;
        var structureAccuracy = heldOutRows.Count == 0 ? 0.0 : (double)structureCorrect / heldOutRows.Count;
        var passesThresholds = heldOutRows.Count > 0 && accuracy >= MinimumEvaluationAccuracy;
        var blockReason = passesThresholds
            ? structureMode
                ? "Transferable decision-structure baseline passed; semantic quality and host runtime remain unclaimed until lifecycle and host gates pass."
                : "Semantic quality and host runtime remain unclaimed until checkpoint rewrite, fresh-load, continue-training, and host gates pass."
            : structureMode
                ? "Held-out transferable decision-structure baseline or row count is below the OrchestrationReasoningScenarioUnit threshold."
                : "Held-out baseline accuracy or row count is below the OrchestrationReasoningScenarioUnit threshold.";

        return new OrchestrationReasoningScenarioEvaluationReport(
            SchemaVersion: 1,
            StateId: stateId,
            HeldOutRowCount: heldOutRows.Count,
            CorrectCount: correct,
            Accuracy: accuracy,
            PassesThresholds: passesThresholds,
            MinimumConfidence: state?.MinimumConfidence ?? 0.0,
            MinimumTopTwoMargin: state?.MinimumTopTwoMargin ?? 0.0,
            HostRuntimeClaimed: false,
            SemanticQualityClaimed: false,
            BlockReason: blockReason,
            DeterministicControlNotes:
            [
                "Uses structured task signals and runtime policy fields only.",
                "Raw prompt text, source ids, pair ids, file paths, guide titles, and exact scenario ids are not final authority.",
                "Nearest/KNN-style overlap is a transparent baseline, not semantic quality evidence.",
                "Transferable decision-structure mode measures non-label structure refs and forbids exact held-out scenario replay."
            ],
            StructureCorrectCount: structureCorrect,
            StructureAccuracy: structureAccuracy,
            EvaluationMode: structureMode
                ? "transferable_decision_structure_baseline"
                : "exact_pattern_and_scenario_ref");
    }

    private OrchestrationReasoningScenarioUnitState? LoadState()
        => loadUserState() ?? loadCommonState();

    private static OrchestrationReasoningScenarioDecision NotReady(
        string blockReason,
        IReadOnlyList<OrchestrationReasoningNearestGuide> nearest,
        double confidence,
        double topTwoMargin)
        => new(
            IsReady: false,
            PatternId: null,
            ScenarioRef: null,
            Pattern: null,
            MatchedSignalCount: 0,
            Confidence: confidence,
            TopTwoMargin: topTwoMargin,
            HostRuntimeClaimed: false,
            SemanticQualityClaimed: false,
            BlockReason: blockReason,
            EvidenceSignals:
            [
                "ORCHESTRATION_REASONING_SCENARIO_UNIT_USED",
                "ORCHESTRATION_REASONING_SCENARIO_UNIT_NOT_READY",
                "BASELINE_NOT_SEMANTIC_QUALITY",
                "HOST_RUNTIME_CLAIMED_FALSE",
                "SEMANTIC_QUALITY_CLAIMED_FALSE"
            ],
            NearestGuides: nearest,
            DecisionStructureRefs: [],
            MatchKind: "not_ready");

    private static OrchestrationReasoningNearestQuery BuildQuery(OrchestrationReasoningRequest request)
        => new(
            RequestId: request.RequestId,
            TaskType: request.Task.TaskType,
            ConstraintSignals: request.Task.Constraints,
            RequirementSignals: request.Task.ExplicitUserRequirements,
            DeliverableSignals: request.Task.Deliverables,
            EvidenceSignals: request.Task.EvidenceNeeds,
            RuntimePolicyRefs: request.RuntimePolicyRefs);

    private static OrchestrationReasoningNearestGuide BuildNearestGuide(
        OrchestrationReasoningScenarioUnitPattern pattern,
        HashSet<string> requestSignals,
        IReadOnlyCollection<string> requestContentTokens)
    {
        var requiredTaskSignals = pattern.RequiredTaskTypes
            .Select(NormalizeSignal)
            .Where(signal => signal.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var requiredContentSignals = pattern.RequiredConstraintSignals
            .Concat(pattern.RequiredRequirementSignals)
            .Concat(pattern.RequiredDeliverableSignals)
            .Concat(pattern.RequiredEvidenceSignals)
            .Select(NormalizeSignal)
            .Where(signal => signal.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var requiredSignals = requiredTaskSignals
            .Concat(requiredContentSignals)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var matchedSignals = requiredSignals
            .Where(requestSignals.Contains)
            .OrderBy(signal => signal, StringComparer.Ordinal)
            .ToArray();
        var requiredSatisfied = requiredSignals.Length > 0
            && requiredSignals.All(requestSignals.Contains);
        var requiredContentTokens = BuildTransferableSignalTokenSet(requiredContentSignals);
        var matchedContentTokens = requiredContentTokens
            .Where(requestContentTokens.Contains)
            .OrderBy(token => token, StringComparer.Ordinal)
            .ToArray();
        var taskTypeSatisfied = requiredTaskSignals.Length == 0
            || requiredTaskSignals.Any(requestSignals.Contains);
        var coverage = requiredSignals.Length == 0
            ? 0.0
            : (double)matchedSignals.Length / requiredSignals.Length;
        var tokenCoverage = requiredContentTokens.Count == 0
            ? 0.0
            : (double)matchedContentTokens.Length / requiredContentTokens.Count;
        var hasTransferableStructure = taskTypeSatisfied && matchedContentTokens.Length > 0;
        var matchKind = requiredSatisfied
            ? ExactSignalMatchKind
            : hasTransferableStructure
                ? TransferableStructureMatchKind
                : "no_transferable_signal_overlap";
        var confidence = requiredSatisfied
            ? Math.Clamp(pattern.Confidence * coverage, 0.0, 1.0)
            : hasTransferableStructure
                ? Math.Clamp(pattern.Confidence * (0.05 + tokenCoverage * 0.55 + Math.Min(matchedContentTokens.Length, 4) * 0.03), 0.0, 1.0)
                : 0.0;
        var matchedSignalCount = requiredSatisfied
            ? matchedSignals.Length
            : matchedContentTokens.Length;
        var totalSignalCount = requiredSatisfied
            ? requiredSignals.Length
            : Math.Max(requiredContentTokens.Count, requiredSignals.Length);
        var matchedSignalRefs = requiredSatisfied
            ? matchedSignals
            : matchedContentTokens.Select(token => $"decision-token:{token}").ToArray();
        var decisionStructureRefs = BuildDecisionStructureRefs(pattern, matchedContentTokens);

        return new OrchestrationReasoningNearestGuide(
            PatternId: pattern.PatternId,
            ScenarioRef: pattern.ScenarioRef,
            MatchedSignalCount: matchedSignalCount,
            TotalPatternSignalCount: totalSignalCount,
            BaselineConfidence: confidence,
            BaselineScore: matchedSignalCount,
            MatchedSignals: matchedSignalRefs,
            HostRuntimeClaimed: false,
            SemanticQualityClaimed: false,
            DecisionStructureRefs: decisionStructureRefs,
            MatchKind: matchKind);
    }

    private static HashSet<string> BuildSignalSet(OrchestrationReasoningNearestQuery query)
    {
        var signals = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            NormalizeSignal(query.TaskType)
        };

        foreach (var signal in query.ConstraintSignals
            .Concat(query.RequirementSignals)
            .Concat(query.DeliverableSignals)
            .Concat(query.EvidenceSignals))
        {
            signals.Add(NormalizeSignal(signal));
        }

        foreach (var field in query.RuntimePolicyRefs)
        {
            signals.Add(NormalizeSignal(field.Key));
            signals.Add(NormalizeSignal(field.Value));
            signals.Add(NormalizeSignal($"{field.Key}:{field.Value}"));
        }

        return signals;
    }

    private static string NormalizeSignal(string value)
        => value.Trim().ToLowerInvariant();

    private static IReadOnlyList<string> BuildTransferableSignalTokenSet(IEnumerable<string> signals)
    {
        var tokens = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var signal in signals)
        {
            foreach (var token in SplitTransferableSignalTokens(signal))
                tokens.Add(token);
        }

        return tokens.ToArray();
    }

    private static IEnumerable<string> SplitTransferableSignalTokens(string value)
    {
        var token = new List<char>();
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                token.Add(ch);
                continue;
            }

            foreach (var emitted in FlushToken(token))
                yield return emitted;
        }

        foreach (var emitted in FlushToken(token))
            yield return emitted;
    }

    private static IEnumerable<string> FlushToken(List<char> token)
    {
        if (token.Count == 0)
            yield break;

        var value = new string(token.ToArray());
        token.Clear();
        if (IsTransferableSignalToken(value))
            yield return value;
    }

    private static bool IsTransferableSignalToken(string token)
    {
        if (token.Length < 4)
            return false;
        if (TransferableTokenStopWords.Contains(token))
            return false;
        if (token.Count(char.IsDigit) > token.Length / 2)
            return false;
        if (token.StartsWith("sha", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    private static IReadOnlyList<string> BuildDecisionStructureRefs(
        OrchestrationReasoningScenarioUnitPattern pattern,
        IReadOnlyList<string> matchedContentTokens)
    {
        var refs = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var family in pattern.PhaseFamilies)
            AddFamilyRef(refs, "phase", family);
        foreach (var family in pattern.GateFamilies)
            AddFamilyRef(refs, "gate", family);
        foreach (var family in pattern.AssetFamilies)
            AddFamilyRef(refs, "asset", family);
        foreach (var family in pattern.HostCheckSets)
            AddFamilyRef(refs, "host-check", family);
        foreach (var flag in pattern.RiskFlags)
            AddFamilyRef(refs, "risk", flag);
        foreach (var action in pattern.NextGranularizationActions)
            AddFamilyRef(refs, "next-action", action);
        foreach (var token in matchedContentTokens)
            refs.Add($"decision-token:{token}");

        return refs.ToArray();
    }

    private static void AddFamilyRef(SortedSet<string> refs, string prefix, string value)
    {
        var family = string.Join("-", SplitTransferableSignalTokens(value));
        if (family.Length > 0)
            refs.Add($"{prefix}:{family}");
    }

    private static int CountDecisionTokenRefs(IReadOnlyList<string>? decisionStructureRefs)
        => decisionStructureRefs?.Count(item => item.StartsWith("decision-token:", StringComparison.Ordinal)) ?? 0;
}
