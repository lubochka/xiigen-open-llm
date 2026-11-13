// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed record PromptToTaskNearestQuery(
    string PromptId,
    string RawPromptSha256,
    IReadOnlyList<string> PromptSignals,
    IReadOnlyDictionary<string, string> ContextFields);

public sealed record PromptToTaskTaskPatternUnitPattern(
    string PatternId,
    string TaskType,
    IReadOnlyList<string> LearnedSignals,
    IReadOnlyList<string> ExplicitUserRequirements,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<string> Deliverables,
    IReadOnlyList<string> ForbiddenActions,
    IReadOnlyList<string> RequiredHosts,
    IReadOnlyList<string> EvidenceNeeds,
    string SplitId,
    double Confidence,
    string? AcceptedPairId = null);

public sealed record PromptToTaskNearestPattern(
    string PatternId,
    string TaskType,
    int MatchedSignalCount,
    int TotalPatternSignalCount,
    double BaselineConfidence,
    double BaselineScore,
    IReadOnlyList<string> MatchedSignals,
    bool SemanticQualityClaimed);

public sealed record PromptToTaskTaskPatternUnitState(
    int SchemaVersion,
    string StateId,
    string CheckpointRef,
    string ManifestRef,
    IReadOnlyList<PromptToTaskTaskPatternUnitPattern> Patterns,
    IReadOnlyList<string> AcceptedPairIds,
    IReadOnlyList<string> QuarantinedPairIds,
    double MinimumConfidence,
    double MinimumTopTwoMargin,
    bool SemanticQualityClaimed);

public sealed record PromptToTaskTaskPatternUnitDecision(
    bool IsReady,
    string? PatternId,
    string? TaskType,
    PromptToTaskTaskPatternUnitPattern? Pattern,
    int MatchedSignalCount,
    double Confidence,
    double TopTwoMargin,
    bool SemanticQualityClaimed,
    string BlockReason,
    IReadOnlyList<string> EvidenceSignals,
    IReadOnlyList<PromptToTaskNearestPattern> NearestPatterns);

public sealed record PromptToTaskTaskPatternEvaluationRow(
    string RowId,
    PromptToTaskRequest Request,
    string ExpectedPatternId,
    string ExpectedTaskType);

public sealed record PromptToTaskTaskPatternEvaluationReport(
    int SchemaVersion,
    string StateId,
    int HeldOutRowCount,
    int CorrectCount,
    double Accuracy,
    bool PassesThresholds,
    double MinimumConfidence,
    double MinimumTopTwoMargin,
    bool SemanticQualityClaimed,
    string BlockReason,
    IReadOnlyList<string> DeterministicControlNotes);

public sealed class PromptToTaskTaskPatternUnit(
    Func<PromptToTaskTaskPatternUnitState?> loadUserState,
    Func<PromptToTaskTaskPatternUnitState?> loadCommonState)
    : ILogicalUnit<PromptToTaskRequest, PromptToTaskTaskPatternUnitDecision>
{
    public const string UnitId = "prompt_to_task_task_pattern_unit";
    public const string UnitRole = "task_pattern_decision";

    private const double MinimumEvaluationAccuracy = 0.8;

    public PromptToTaskTaskPatternUnit(PromptToTaskTaskPatternUnitState state)
        : this(() => state, () => null)
    {
    }

    public static PromptToTaskTaskPatternUnitState FromLearnedState(
        PromptToTaskLearnedStateDescriptor learnedState,
        IReadOnlyList<string>? acceptedPairIds = null,
        IReadOnlyList<string>? quarantinedPairIds = null,
        double minimumConfidence = 0.01,
        double minimumTopTwoMargin = 0.01)
        => new(
            SchemaVersion: 1,
            StateId: learnedState.StateId,
            CheckpointRef: learnedState.StateSha256,
            ManifestRef: learnedState.ManifestId,
            Patterns: (learnedState.LearnedPatterns ?? [])
                .Select(pattern => new PromptToTaskTaskPatternUnitPattern(
                    PatternId: pattern.PatternId,
                    TaskType: pattern.TaskType,
                    LearnedSignals: pattern.LearnedSignals,
                    ExplicitUserRequirements: pattern.ExplicitUserRequirements,
                    Constraints: pattern.Constraints,
                    Deliverables: pattern.Deliverables,
                    ForbiddenActions: pattern.ForbiddenActions,
                    RequiredHosts: pattern.RequiredHosts,
                    EvidenceNeeds: pattern.EvidenceNeeds,
                    SplitId: pattern.SplitId,
                    Confidence: pattern.Confidence))
                .ToArray(),
            AcceptedPairIds: acceptedPairIds ?? [],
            QuarantinedPairIds: quarantinedPairIds ?? [],
            MinimumConfidence: minimumConfidence,
            MinimumTopTwoMargin: minimumTopTwoMargin,
            SemanticQualityClaimed: false);

    public Task<OperationResult<LogicalUnitResult<PromptToTaskTaskPatternUnitDecision>>> ExecuteAsync(
        LogicalUnitRequest<PromptToTaskRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<PromptToTaskTaskPatternUnitDecision>>.Fail("Unit cancelled.", "CANCELLED"));

        var decision = Decide(request.Input);
        if (!decision.IsSuccess)
            return Task.FromResult(decision.MapError<LogicalUnitResult<PromptToTaskTaskPatternUnitDecision>>());

        var value = decision.Value!;
        var result = new LogicalUnitResult<PromptToTaskTaskPatternUnitDecision>(
            UnitId: UnitId,
            UnitRole: UnitRole,
            DomainTag: PromptToTaskMachine.DomainTag,
            Output: value,
            Confidence: value.Confidence,
            PredicateIds: value.PatternId is null ? [] : [value.PatternId],
            DomainTags: [PromptToTaskMachine.DomainTag],
            EvidenceIds: value.EvidenceSignals.ToArray(),
            Warnings: value.IsReady
                ? ["BASELINE_NEAREST_SIGNAL_OVERLAP_NOT_SEMANTIC_QUALITY"]
                : [value.BlockReason]);

        return Task.FromResult(OperationResult<LogicalUnitResult<PromptToTaskTaskPatternUnitDecision>>.Ok(result));
    }

    public OperationResult<PromptToTaskTaskPatternUnitDecision> Decide(PromptToTaskRequest request)
    {
        var state = LoadState();
        if (state is null)
            return OperationResult<PromptToTaskTaskPatternUnitDecision>.Ok(NotReady(
                "PromptToTaskTaskPatternUnit state is not loaded.",
                [],
                0.0,
                0.0));

        if (state.SemanticQualityClaimed)
            return OperationResult<PromptToTaskTaskPatternUnitDecision>.Fail(
                "PromptToTaskTaskPatternUnit does not accept semantic quality claims in this slice.",
                "PROMPT_TO_TASK_TASK_PATTERN_UNIT_SEMANTIC_QUALITY_UNSUPPORTED");

        var query = BuildQuery(request);
        var nearest = FindNearestPromptToTaskPatterns(query.Query, state.Patterns.Count);
        var top = nearest.FirstOrDefault();
        var second = nearest.Skip(1).FirstOrDefault();
        var topTwoMargin = top is null
            ? 0.0
            : second is null
                ? top.BaselineConfidence
                : top.BaselineConfidence - second.BaselineConfidence;

        if (top is null || top.MatchedSignalCount <= 0)
            return OperationResult<PromptToTaskTaskPatternUnitDecision>.Ok(NotReady(
                "No PromptToTask baseline nearest pattern matched request signals.",
                nearest,
                0.0,
                topTwoMargin,
                query.AuthorityEvidenceSignals));

        if (top.BaselineConfidence < state.MinimumConfidence)
            return OperationResult<PromptToTaskTaskPatternUnitDecision>.Ok(NotReady(
                "PromptToTask baseline confidence is below the unit threshold.",
                nearest,
                top.BaselineConfidence,
                topTwoMargin,
                query.AuthorityEvidenceSignals));

        if (topTwoMargin < state.MinimumTopTwoMargin)
            return OperationResult<PromptToTaskTaskPatternUnitDecision>.Ok(NotReady(
                "PromptToTask baseline top-two margin is below the unit threshold.",
                nearest,
                top.BaselineConfidence,
                topTwoMargin,
                query.AuthorityEvidenceSignals));

        var pattern = state.Patterns.First(pattern => string.Equals(pattern.PatternId, top.PatternId, StringComparison.Ordinal));
        return OperationResult<PromptToTaskTaskPatternUnitDecision>.Ok(new PromptToTaskTaskPatternUnitDecision(
            IsReady: true,
            PatternId: pattern.PatternId,
            TaskType: pattern.TaskType,
            Pattern: pattern,
            MatchedSignalCount: top.MatchedSignalCount,
            Confidence: top.BaselineConfidence,
            TopTwoMargin: topTwoMargin,
            SemanticQualityClaimed: false,
            BlockReason: "",
            EvidenceSignals:
            [
                "PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED",
                "PROMPT_TO_TASK_BASELINE_NEAREST_SIGNAL_OVERLAP",
                "BASELINE_NOT_SEMANTIC_QUALITY",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                $"PROMPT_TO_TASK_TASK_PATTERN_UNIT_STATE:{state.StateId}",
                $"PROMPT_TO_TASK_TASK_PATTERN_UNIT_MATCHED:{pattern.PatternId}",
                $"PROMPT_TO_TASK_TASK_PATTERN_UNIT_MARGIN:{topTwoMargin:0.###}",
                .. query.AuthorityEvidenceSignals
            ],
            NearestPatterns: nearest));
    }

    public IReadOnlyList<PromptToTaskNearestPattern> FindNearestPromptToTaskPatterns(
        PromptToTaskNearestQuery query,
        int k)
    {
        var state = LoadState();
        if (state is null || k <= 0)
            return [];

        var requestSignals = BuildSignalSet(query.PromptSignals, query.ContextFields);
        return state.Patterns
            .Select(pattern => BuildNearestPattern(pattern, requestSignals))
            .Where(pattern => pattern.MatchedSignalCount > 0)
            .OrderByDescending(pattern => pattern.BaselineConfidence)
            .ThenByDescending(pattern => pattern.MatchedSignalCount)
            .ThenBy(pattern => pattern.PatternId, StringComparer.Ordinal)
            .Take(k)
            .ToArray();
    }

    public PromptToTaskTaskPatternEvaluationReport EvaluateHeldOutRows(
        IReadOnlyList<PromptToTaskTaskPatternEvaluationRow> heldOutRows)
    {
        var state = LoadState();
        var stateId = state?.StateId ?? "no-state";
        var correct = 0;
        foreach (var row in heldOutRows)
        {
            var decision = Decide(row.Request).Value!;
            if (decision.IsReady
                && string.Equals(decision.PatternId, row.ExpectedPatternId, StringComparison.Ordinal)
                && string.Equals(decision.TaskType, row.ExpectedTaskType, StringComparison.Ordinal))
            {
                correct++;
            }
        }

        var accuracy = heldOutRows.Count == 0 ? 0.0 : (double)correct / heldOutRows.Count;
        var passesThresholds = heldOutRows.Count > 0 && accuracy >= MinimumEvaluationAccuracy;
        var blockReason = passesThresholds
            ? "Semantic quality remains unclaimed until checkpoint asset, fresh-load, and continue-training gates pass."
            : "Held-out baseline accuracy or row count is below the PromptToTaskTaskPatternUnit threshold.";

        return new PromptToTaskTaskPatternEvaluationReport(
            SchemaVersion: 1,
            StateId: stateId,
            HeldOutRowCount: heldOutRows.Count,
            CorrectCount: correct,
            Accuracy: accuracy,
            PassesThresholds: passesThresholds,
            MinimumConfidence: state?.MinimumConfidence ?? 0.0,
            MinimumTopTwoMargin: state?.MinimumTopTwoMargin ?? 0.0,
            SemanticQualityClaimed: false,
            BlockReason: blockReason,
            DeterministicControlNotes:
            [
                "Uses explicit constraints, normalized prompt-token signals, and context fields only.",
                "Raw prompt text, file paths, source ids, pair ids, and exact prompt lookup are not final authority.",
                "Nearest/KNN-style overlap is a transparent baseline, not semantic quality evidence."
            ]);
    }

    private PromptToTaskTaskPatternUnitState? LoadState()
        => loadUserState() ?? loadCommonState();

    private static PromptToTaskTaskPatternUnitDecision NotReady(
        string blockReason,
        IReadOnlyList<PromptToTaskNearestPattern> nearest,
        double confidence,
        double topTwoMargin,
        IReadOnlyList<string>? authorityEvidenceSignals = null)
    {
        var evidenceSignals = new List<string>
        {
            "PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED",
            "PROMPT_TO_TASK_TASK_PATTERN_UNIT_NOT_READY",
            "BASELINE_NOT_SEMANTIC_QUALITY",
            "SEMANTIC_QUALITY_CLAIMED_FALSE"
        };
        evidenceSignals.AddRange(authorityEvidenceSignals ?? []);

        return new PromptToTaskTaskPatternUnitDecision(
            IsReady: false,
            PatternId: null,
            TaskType: null,
            Pattern: null,
            MatchedSignalCount: 0,
            Confidence: confidence,
            TopTwoMargin: topTwoMargin,
            SemanticQualityClaimed: false,
            BlockReason: blockReason,
            EvidenceSignals: evidenceSignals.Distinct(StringComparer.Ordinal).ToArray(),
            NearestPatterns: nearest);
    }

    private static PromptToTaskAuthorityQuery BuildQuery(PromptToTaskRequest request)
    {
        var authority = ExtractAuthorityText(request);
        var promptSignals = request.ExplicitConstraints
            .Concat(authority.AuthorityTexts.SelectMany(ExtractPromptSignals))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new PromptToTaskAuthorityQuery(
            Query: new PromptToTaskNearestQuery(
            PromptId: request.PromptId,
            RawPromptSha256: request.RawPromptSha256,
            PromptSignals: promptSignals,
            ContextFields: request.ContextFields),
            AuthorityEvidenceSignals: authority.EvidenceSignals);
    }

    private static IReadOnlyList<string> ExtractPromptSignals(string rawPrompt)
    {
        var tokenSignals = rawPrompt
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => new string(token
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray()))
            .Where(token => token.Length is >= 4 and <= 40)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(token => $"prompt:{token}")
            .ToArray();
        var bareSignals = tokenSignals
            .Select(signal => signal["prompt:".Length..])
            .ToArray();
        return tokenSignals
            .Concat(bareSignals)
            .Concat(BuildUniversalSignalAliases(rawPrompt))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static PromptToTaskAuthorityText ExtractAuthorityText(PromptToTaskRequest request)
    {
        var evidenceSignals = new List<string>();
        var authorityTexts = new List<string>();
        var rawPromptHasEvidenceMarker = ContainsEvidenceOnlyMarker(request.RawPrompt);
        var contextHasEvidenceMarker = request.ContextFields.Keys.Any(IsEvidenceOnlyContextKey);

        if (TryGetContextValue(request.ContextFields, "current_unquoted_user_instruction", out var currentInstruction))
        {
            authorityTexts.Add(currentInstruction);
            evidenceSignals.Add("CURRENT_UNQUOTED_AUTHORITY_USED");
            if (rawPromptHasEvidenceMarker || contextHasEvidenceMarker)
                evidenceSignals.Add("EVIDENCE_ONLY_AUTHORITY_QUARANTINE_APPLIED");
            if (TryGetContextValue(request.ContextFields, "current_unquoted_reactivates_quote", out var reactivation)
                && string.Equals(reactivation, "true", StringComparison.OrdinalIgnoreCase)
                && TryGetContextValue(request.ContextFields, "reactivated_quote_text", out var reactivatedText))
            {
                authorityTexts.Add(reactivatedText);
                evidenceSignals.Add("QUOTE_REACTIVATED_BY_CURRENT_UNQUOTED_INSTRUCTION");
            }

            return new PromptToTaskAuthorityText(
                AuthorityTexts: authorityTexts,
                EvidenceSignals: evidenceSignals.Distinct(StringComparer.Ordinal).ToArray());
        }

        var liveText = StripEvidenceOnlyText(request.RawPrompt, out var evidenceOnlyRemoved);
        if (!string.IsNullOrWhiteSpace(liveText))
        {
            authorityTexts.Add(liveText);
            evidenceSignals.Add("RAW_PROMPT_AUTHORITY_USED");
        }

        if (evidenceOnlyRemoved || contextHasEvidenceMarker)
            evidenceSignals.Add("EVIDENCE_ONLY_AUTHORITY_QUARANTINE_APPLIED");
        if (authorityTexts.Count == 0)
            evidenceSignals.Add("NO_CURRENT_UNQUOTED_AUTHORITY_TEXT");

        return new PromptToTaskAuthorityText(
            AuthorityTexts: authorityTexts,
            EvidenceSignals: evidenceSignals.Distinct(StringComparer.Ordinal).ToArray());
    }

    private static string StripEvidenceOnlyText(string rawPrompt, out bool evidenceOnlyRemoved)
    {
        evidenceOnlyRemoved = false;
        var liveLines = new List<string>();
        var inEvidenceBlock = false;
        foreach (var line in rawPrompt.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            var trimmed = line.Trim();
            if (IsLiveTaskMarkerLine(trimmed, out var liveTaskLine))
            {
                inEvidenceBlock = false;
                if (!string.IsNullOrWhiteSpace(liveTaskLine))
                    liveLines.Add(liveTaskLine);
                continue;
            }

            if (inEvidenceBlock)
            {
                if (IsEvidenceBlockEnd(trimmed))
                    inEvidenceBlock = false;
                evidenceOnlyRemoved = true;
                continue;
            }

            if (IsEvidenceOnlyMarkerLine(trimmed))
            {
                evidenceOnlyRemoved = true;
                inEvidenceBlock = true;
                continue;
            }

            liveLines.Add(line);
        }

        return string.Join(Environment.NewLine, liveLines).Trim();
    }

    private static IReadOnlyList<string> BuildUniversalSignalAliases(string rawPrompt)
    {
        var normalized = rawPrompt.ToLowerInvariant();
        var aliases = new List<string>();

        AddAliasIf(aliases, normalized, "plan", "plan");
        AddAliasIf(aliases, normalized, "planning", "planning");
        AddAliasIf(aliases, normalized, "phase", "phase");
        AddAliasIf(aliases, normalized, "approval", "approval");
        AddAliasIf(aliases, normalized, "approve", "approval");
        AddAliasIf(aliases, normalized, "do not implement", "implementation_authorized_false");
        AddAliasIf(aliases, normalized, "not implement", "implementation_authorized_false");
        AddAliasIf(aliases, normalized, "implementation is not authorized", "implementation_authorized_false");
        AddAliasIf(aliases, normalized, "approval gate", "implementation_authorized_false");
        AddAliasIf(aliases, normalized, "missing gate", "missing_gates");
        AddAliasIf(aliases, normalized, "missing structure", "missing_structure");
        AddAliasIf(aliases, normalized, "gate", "missing_gates");
        AddAliasIf(aliases, normalized, "scope", "scope");
        AddAliasIf(aliases, normalized, "scope creep", "scope_creep");
        AddAliasIf(aliases, normalized, "deliverable", "deliverable");
        AddAliasIf(aliases, normalized, "artifact", "deliverable");
        AddAliasIf(aliases, normalized, "documentation", "documentation");
        AddAliasIf(aliases, normalized, "missing documentation sync", "missing_documentation_sync");
        AddAliasIf(aliases, normalized, "document", "documentation");
        AddAliasIf(aliases, normalized, "skill", "skill");
        AddAliasIf(aliases, normalized, "audit", "audit");
        AddAliasIf(aliases, normalized, "review", "review");
        AddAliasIf(aliases, normalized, "consult", "consultation");
        AddAliasIf(aliases, normalized, "feature", "feature");
        AddAliasIf(aliases, normalized, "parity", "parity");
        AddAliasIf(aliases, normalized, "product decision", "product_decision_masquerade");
        AddAliasIf(aliases, normalized, "source", "source");
        AddAliasIf(aliases, normalized, "reference", "source");
        AddAliasIf(aliases, normalized, "context", "context");
        AddAliasIf(aliases, normalized, "needs source", "source_resolution_required");
        AddAliasIf(aliases, normalized, "source required", "source_resolution_required");
        AddAliasIf(aliases, normalized, "context dependency", "context_dependency");
        AddAliasIf(aliases, normalized, "flow", "flow");
        AddAliasIf(aliases, normalized, "roadmap", "roadmap");
        AddAliasIf(aliases, normalized, "validation", "validation");
        AddAliasIf(aliases, normalized, "validate", "validation");
        AddAliasIf(aliases, normalized, "insufficient verification", "insufficient_verification");
        AddAliasIf(aliases, normalized, "deterministic shortcut", "deterministic_shortcut");
        AddAliasIf(aliases, normalized, "weak goal", "weak_goal_decomposition");
        AddAliasIf(aliases, normalized, "vague deliverable", "vague_deliverables");

        return aliases.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void AddAliasIf(
        List<string> aliases,
        string normalizedPrompt,
        string phrase,
        string signal)
    {
        if (normalizedPrompt.Contains(phrase, StringComparison.Ordinal))
            aliases.Add(signal);
    }

    private static PromptToTaskNearestPattern BuildNearestPattern(
        PromptToTaskTaskPatternUnitPattern pattern,
        HashSet<string> requestSignals)
    {
        var patternSignals = pattern.LearnedSignals
            .Select(NormalizeSignal)
            .Where(signal => signal.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var matchedSignals = patternSignals
            .Where(requestSignals.Contains)
            .OrderBy(signal => signal, StringComparer.Ordinal)
            .ToArray();
        var coverage = patternSignals.Length == 0
            ? 0.0
            : (double)matchedSignals.Length / patternSignals.Length;
        var confidence = Math.Clamp(pattern.Confidence * coverage, 0.0, 1.0);

        return new PromptToTaskNearestPattern(
            PatternId: pattern.PatternId,
            TaskType: pattern.TaskType,
            MatchedSignalCount: matchedSignals.Length,
            TotalPatternSignalCount: patternSignals.Length,
            BaselineConfidence: confidence,
            BaselineScore: matchedSignals.Length,
            MatchedSignals: matchedSignals,
            SemanticQualityClaimed: false);
    }

    private static HashSet<string> BuildSignalSet(
        IReadOnlyList<string> promptSignals,
        IReadOnlyDictionary<string, string> contextFields)
    {
        var signals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var signal in promptSignals)
            signals.Add(NormalizeSignal(signal));

        foreach (var field in contextFields)
        {
            if (IsAuthorityContextKey(field.Key) || IsEvidenceOnlyContextKey(field.Key) || IsProvenanceOnlyContextKey(field.Key))
                continue;

            signals.Add(NormalizeSignal(field.Key));
            signals.Add(NormalizeSignal(field.Value));
            signals.Add(NormalizeSignal($"{field.Key}:{field.Value}"));
        }

        return signals;
    }

    private static bool TryGetContextValue(
        IReadOnlyDictionary<string, string> contextFields,
        string key,
        out string value)
    {
        foreach (var field in contextFields)
        {
            if (string.Equals(field.Key, key, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(field.Value))
            {
                value = field.Value;
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static bool ContainsEvidenceOnlyMarker(string value)
        => value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(token => IsEvidenceOnlyMarkerLine(token));

    private static bool IsEvidenceOnlyMarkerLine(string line)
    {
        var normalized = line.Trim().ToLowerInvariant();
        return normalized.StartsWith("quote:", StringComparison.Ordinal)
            || normalized.StartsWith("quoted:", StringComparison.Ordinal)
            || normalized.StartsWith("quoted old plan", StringComparison.Ordinal)
            || normalized.StartsWith("old plan:", StringComparison.Ordinal)
            || normalized.StartsWith("attachment:", StringComparison.Ordinal)
            || normalized.StartsWith("attached:", StringComparison.Ordinal)
            || normalized.StartsWith("prior assistant", StringComparison.Ordinal)
            || normalized.StartsWith("assistant answer:", StringComparison.Ordinal)
            || normalized.StartsWith("state next_action", StringComparison.Ordinal)
            || normalized.StartsWith("state.next_action", StringComparison.Ordinal)
            || normalized.StartsWith("old approval", StringComparison.Ordinal)
            || normalized.StartsWith("transcript:", StringComparison.Ordinal)
            || normalized.StartsWith("evidence-only:", StringComparison.Ordinal)
            || normalized.StartsWith("evidence_only:", StringComparison.Ordinal);
    }

    private static bool IsEvidenceOnlyContextKey(string key)
    {
        if (IsAuthorityContextKey(key))
            return false;

        var normalized = key.Trim().ToLowerInvariant();
        return normalized.Contains("quote", StringComparison.Ordinal)
            || normalized.Contains("attachment", StringComparison.Ordinal)
            || normalized.Contains("prior_assistant", StringComparison.Ordinal)
            || normalized.Contains("assistant_answer", StringComparison.Ordinal)
            || normalized.Contains("state_next_action", StringComparison.Ordinal)
            || normalized.Contains("old_approval", StringComparison.Ordinal)
            || normalized.Contains("old_plan", StringComparison.Ordinal)
            || normalized.Contains("transcript", StringComparison.Ordinal);
    }

    private static bool IsAuthorityContextKey(string key)
    {
        var normalized = key.Trim().ToLowerInvariant();
        return string.Equals(normalized, "current_unquoted_user_instruction", StringComparison.Ordinal)
            || string.Equals(normalized, "current_unquoted_reactivates_quote", StringComparison.Ordinal)
            || string.Equals(normalized, "reactivated_quote_text", StringComparison.Ordinal);
    }

    private static bool IsProvenanceOnlyContextKey(string key)
    {
        var normalized = key.Trim().ToLowerInvariant();
        return normalized.Contains("source_row_id", StringComparison.Ordinal)
            || normalized.Contains("source_family", StringComparison.Ordinal)
            || normalized.Contains("heldout_source_family", StringComparison.Ordinal)
            || normalized.Contains("fixture_id", StringComparison.Ordinal)
            || normalized.Contains("pair_id", StringComparison.Ordinal)
            || normalized.Contains("prompt_id", StringComparison.Ordinal)
            || normalized.Contains("scenario_id", StringComparison.Ordinal)
            || normalized.Contains("split_role", StringComparison.Ordinal)
            || normalized.Contains("provenance", StringComparison.Ordinal);
    }

    private static bool IsLiveTaskMarkerLine(string line, out string liveTaskLine)
    {
        liveTaskLine = string.Empty;
        var markers = new[] { "current task:", "real_task:", "real task:" };
        foreach (var marker in markers)
        {
            if (line.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
            {
                liveTaskLine = line[marker.Length..].Trim();
                return true;
            }
        }

        return false;
    }

    private static bool IsEvidenceBlockEnd(string line)
        => line.Equals("end quote", StringComparison.OrdinalIgnoreCase)
            || line.Equals("end evidence", StringComparison.OrdinalIgnoreCase)
            || line.Equals("end transcript", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSignal(string value)
        => value.Trim().ToLowerInvariant();

    private sealed record PromptToTaskAuthorityQuery(
        PromptToTaskNearestQuery Query,
        IReadOnlyList<string> AuthorityEvidenceSignals);

    private sealed record PromptToTaskAuthorityText(
        IReadOnlyList<string> AuthorityTexts,
        IReadOnlyList<string> EvidenceSignals);
}
