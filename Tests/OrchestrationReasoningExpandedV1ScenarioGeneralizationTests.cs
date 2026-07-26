// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningExpandedV1ScenarioGeneralizationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> TokenStopWords = new(StringComparer.OrdinalIgnoreCase)
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
        "ignore",
        "input",
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

    [Fact]
    public void ExpandedV1HeldOutEval_UsesTransferableDecisionStructureInsteadOfExactScenarioReplay()
    {
        var rows = ReadExpandedRows();
        var trainRows = rows.Where(row => string.Equals(row.SplitRole, "train", StringComparison.Ordinal)).ToArray();
        var heldOutRows = rows.Where(row => string.Equals(row.SplitRole, "held_out", StringComparison.Ordinal)).ToArray();
        var unit = new OrchestrationReasoningScenarioUnit(CreateState(trainRows));
        var evaluationRows = heldOutRows
            .Select(row => new OrchestrationReasoningScenarioEvaluationRow(
                RowId: row.TrainingPairId,
                Request: CreateExpandedRequest(row),
                ExpectedPatternId: "",
                ExpectedScenarioRef: row.ChosenScenarioRef,
                ExpectedDecisionStructureRefs: BuildExpectedDecisionStructureRefs(row)))
            .ToArray();

        var report = unit.EvaluateHeldOutRows(evaluationRows);
        var rowResults = heldOutRows
            .Select(row => BuildRowResult(unit, row))
            .ToArray();
        var sourceOnlyReport = EvaluateSourceOnlyControls(unit, heldOutRows);
        var boilerplateOnlyReport = EvaluateBoilerplateOnlyControls(unit, heldOutRows);

        Assert.Equal("transferable_decision_structure_baseline", report.EvaluationMode);
        Assert.Equal(4, report.HeldOutRowCount);
        Assert.Equal(4, report.StructureCorrectCount);
        Assert.Equal(1.0, report.StructureAccuracy);
        Assert.Equal(4, report.CorrectCount);
        Assert.True(report.PassesThresholds);
        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.HostRuntimeClaimed);
        Assert.All(rowResults, result => Assert.True(result.Passed));
        Assert.All(rowResults, result => Assert.False(result.ExactScenarioReplay));
        Assert.All(rowResults, result => Assert.NotEmpty(result.ObservedDecisionStructureRefs));
        Assert.All(rowResults, result => Assert.DoesNotContain("decision-token:chosen", result.ObservedDecisionStructureRefs));
        Assert.All(rowResults, result => Assert.DoesNotContain("decision-token:plan", result.ObservedDecisionStructureRefs));
        Assert.All(rowResults, result => Assert.DoesNotContain("decision-token:chosen", result.MatchedDecisionStructureRefs));
        Assert.All(rowResults, result => Assert.DoesNotContain("decision-token:plan", result.MatchedDecisionStructureRefs));
        Assert.Equal(0, sourceOnlyReport.StructureCorrectCount);
        Assert.Equal(0, boilerplateOnlyReport.StructureCorrectCount);
        Assert.Contains(report.DeterministicControlNotes, note => note.Contains("transparent baseline", StringComparison.OrdinalIgnoreCase));

        var learnedStateText = JsonSerializer.Serialize(CreateState(trainRows).Patterns, JsonOptions);
        foreach (var heldOutRow in heldOutRows)
        {
            Assert.DoesNotContain(heldOutRow.TrainingPairId, learnedStateText);
            Assert.DoesNotContain(heldOutRow.SourceRowId, learnedStateText);
            Assert.DoesNotContain(heldOutRow.SourceManifestRef, learnedStateText);
            Assert.DoesNotContain(heldOutRow.ChosenScenarioRef, learnedStateText);
        }

        WriteEvidenceArtifactsIfRequested(report, rowResults, sourceOnlyReport, boilerplateOnlyReport);
    }

    [Fact]
    public void ExpandedV1HeldOutEval_DoesNotPassFromSourceIdsOrExpectedScenarioRefsOnly()
    {
        var rows = ReadExpandedRows();
        var trainRows = rows.Where(row => string.Equals(row.SplitRole, "train", StringComparison.Ordinal)).ToArray();
        var heldOutRows = rows.Where(row => string.Equals(row.SplitRole, "held_out", StringComparison.Ordinal)).ToArray();
        var unit = new OrchestrationReasoningScenarioUnit(CreateState(trainRows));
        var report = EvaluateSourceOnlyControls(unit, heldOutRows);

        Assert.Equal("transferable_decision_structure_baseline", report.EvaluationMode);
        Assert.Equal(4, report.HeldOutRowCount);
        Assert.Equal(0, report.StructureCorrectCount);
        Assert.Equal(0.0, report.StructureAccuracy);
        Assert.False(report.PassesThresholds);
        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.HostRuntimeClaimed);
        Assert.Contains(report.DeterministicControlNotes, note => note.Contains("exact scenario ids are not final authority", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExpandedV1HeldOutEval_DoesNotPassFromLabelBoilerplateOnly()
    {
        var rows = ReadExpandedRows();
        var trainRows = rows.Where(row => string.Equals(row.SplitRole, "train", StringComparison.Ordinal)).ToArray();
        var heldOutRows = rows.Where(row => string.Equals(row.SplitRole, "held_out", StringComparison.Ordinal)).ToArray();
        var unit = new OrchestrationReasoningScenarioUnit(CreateState(trainRows));

        var report = EvaluateBoilerplateOnlyControls(unit, heldOutRows);

        Assert.Equal("transferable_decision_structure_baseline", report.EvaluationMode);
        Assert.Equal(4, report.HeldOutRowCount);
        Assert.Equal(0, report.StructureCorrectCount);
        Assert.Equal(0.0, report.StructureAccuracy);
        Assert.False(report.PassesThresholds);
        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.HostRuntimeClaimed);
    }

    [Fact]
    public void ExpandedV1Decide_BlocksOneAnchorTransferableMatchEvenWhenEvalCountsIt()
    {
        var rows = ReadExpandedRows();
        var authorityTrainRow = rows.Single(row => string.Equals(row.TrainingPairId, "or-expanded-v1-pair-001", StringComparison.Ordinal));
        var unit = new OrchestrationReasoningScenarioUnit(CreateState([authorityTrainRow]));
        var oneAnchorRequest = CreateRequest(
            requestId: "or-expanded-v1-one-anchor-transferable-control",
            constraints: ["authority"],
            requirements: [],
            deliverables: [],
            evidence: [],
            sourceProvenance: new Dictionary<string, string>
            {
                ["control"] = "one-anchor-transferable-match"
            },
            runtimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });
        var evaluationReport = unit.EvaluateHeldOutRows(
        [
            new OrchestrationReasoningScenarioEvaluationRow(
                RowId: "one-anchor-transferable-eval-counts",
                Request: oneAnchorRequest,
                ExpectedPatternId: "",
                ExpectedScenarioRef: "not-the-authority-train-scenario",
                ExpectedDecisionStructureRefs: ["decision-token:authority"])
        ]);

        var nearest = unit.FindNearestOrchestrationGuidePatterns(
            new OrchestrationReasoningNearestQuery(
                RequestId: oneAnchorRequest.RequestId,
                TaskType: oneAnchorRequest.Task.TaskType,
                ConstraintSignals: oneAnchorRequest.Task.Constraints,
                RequirementSignals: oneAnchorRequest.Task.ExplicitUserRequirements,
                DeliverableSignals: oneAnchorRequest.Task.Deliverables,
                EvidenceSignals: oneAnchorRequest.Task.EvidenceNeeds,
                RuntimePolicyRefs: oneAnchorRequest.RuntimePolicyRefs),
            k: 1);
        var decision = unit.Decide(oneAnchorRequest).Value!;

        Assert.Equal("transferable_decision_structure_baseline", evaluationReport.EvaluationMode);
        Assert.Equal(1, evaluationReport.StructureCorrectCount);
        Assert.True(evaluationReport.PassesThresholds);
        Assert.Single(nearest);
        Assert.Equal("transferable_decision_structure_overlap_baseline", nearest[0].MatchKind);
        Assert.Single(nearest[0].DecisionStructureRefs!, item => item.StartsWith("decision-token:", StringComparison.Ordinal));
        Assert.Contains("decision-token:authority", nearest[0].DecisionStructureRefs!);
        Assert.False(decision.IsReady);
        Assert.Contains("at least two decision-token anchors", decision.BlockReason, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(decision.DecisionStructureRefs!);
        Assert.Contains(decision.NearestGuides, guide => guide.DecisionStructureRefs?.Contains("decision-token:authority", StringComparer.Ordinal) == true);
    }

    private static OrchestrationReasoningScenarioUnitState CreateState(IReadOnlyList<ExpandedRow> trainRows)
    {
        var learnedState = new OrchestrationReasoningLearnedGuideStateDescriptor(
            StateId: "or-expanded-v1-generalization-repair-train-only-state",
            GuideStateRef: "leaf-local:or-expanded-v1-generalization-repair",
            ManifestId: "manifest:or-expanded-v1-generalization-repair",
            ManifestSha256: "sha256:leaf-local-generalization-repair",
            MachineType: nameof(OrchestrationReasoningMachine),
            SchemaVersion: "1",
            SourceBaseCommit: "not-recorded-git-forbidden",
            StateSha256: "sha256:leaf-local-state",
            LearnedPatterns: trainRows.Select(BuildPattern).ToArray());

        return OrchestrationReasoningScenarioUnit.FromLearnedState(
            learnedState,
            acceptedPairRefs: trainRows.Select(row => row.ApprovedPairRef).ToArray(),
            quarantinedPairRefs: [],
            minimumConfidence: 0.01,
            minimumTopTwoMargin: 0.01);
    }

    private static OrchestrationReasoningLearnedPattern BuildPattern(ExpandedRow row, int index)
        => new(
            PatternId: $"or-expanded-v1-train-pattern-{index + 1:D2}",
            ScenarioRef: row.ChosenScenarioRef,
            RequiredTaskTypes: ["universal_reasoning"],
            RequiredConstraintSignals: BuildUniversalDecisionSignals(row),
            RequiredRequirementSignals: [],
            RequiredDeliverableSignals: [],
            RequiredEvidenceSignals: [],
            PhaseFamilies: ["orchestration-reasoning-expanded-v1"],
            GateFamilies: ["source-disjoint", row.ErrorCode.ToLowerInvariant()],
            AssetFamilies: ["common-guide-state-baseline"],
            HostCheckSets: ["core-local-eval"],
            RiskFlags:
            [
                "baseline_not_semantic_quality",
                "host_runtime_claimed_false",
                "exact_label_replay_forbidden"
            ],
            NextGranularizationActions:
            [
                "measure transferable decision structure before any readiness claim"
            ],
            Confidence: 0.8);

    private static OrchestrationReasoningRequest CreateExpandedRequest(ExpandedRow row)
        => CreateRequest(
            requestId: $"or-expanded-v1-heldout-{row.TrainingPairId}",
            constraints: BuildUniversalDecisionSignals(row),
            requirements: ["source disjoint evaluation", "no exact held-out scenario label replay"],
            deliverables: ["transferable decision structure evidence"],
            evidence: ["leakage ablation evidence", "no semantic quality claim"],
            sourceProvenance: new Dictionary<string, string>
            {
                ["split_role"] = row.SplitRole,
                ["raw_prompt_text"] = "not_stored",
                ["source_manifest_ref"] = row.SourceManifestRef
            },
            runtimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });

    private static OrchestrationReasoningRequest CreateSourceOnlyControlRequest(ExpandedRow row)
        => CreateRequest(
            requestId: $"or-expanded-v1-control-{row.TrainingPairId}",
            constraints: [],
            requirements: [],
            deliverables: [],
            evidence: [],
            sourceProvenance: new Dictionary<string, string>
            {
                ["source_row_id"] = row.SourceRowId,
                ["source_manifest_ref"] = row.SourceManifestRef,
                ["raw_prompt_text"] = "not_stored"
            },
            runtimePolicyRefs: new Dictionary<string, string>
            {
                ["expected_scenario_ref"] = row.ChosenScenarioRef,
                ["expected_pair_id"] = row.TrainingPairId,
                ["deterministic_shortcut"] = "forbidden"
            });

    private static OrchestrationReasoningRequest CreateBoilerplateOnlyControlRequest(ExpandedRow row)
        => CreateRequest(
            requestId: $"or-expanded-v1-boilerplate-control-{row.TrainingPairId}",
            constraints:
            [
                "chosen_quality_reason rejected_quality_reason",
                "Chosen plan.",
                "Rejected plan.",
                "decision-token:chosen decision-token:plan"
            ],
            requirements: [],
            deliverables: [],
            evidence: [],
            sourceProvenance: new Dictionary<string, string>
            {
                ["source_row_id"] = row.SourceRowId,
                ["raw_prompt_text"] = "not_stored",
                ["control"] = "boilerplate_only"
            },
            runtimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });

    private static OrchestrationReasoningRequest CreateRequest(
        string requestId,
        IReadOnlyList<string> constraints,
        IReadOnlyList<string> requirements,
        IReadOnlyList<string> deliverables,
        IReadOnlyList<string> evidence,
        IReadOnlyDictionary<string, string> sourceProvenance,
        IReadOnlyDictionary<string, string> runtimePolicyRefs)
        => new(
            RequestId: requestId,
            Task: new StructuredTaskContract(
                PromptId: requestId,
                RawPromptSha256: $"sha256:{requestId}",
                UserLanguage: "en",
                TaskType: "universal_reasoning",
                ExplicitUserRequirements: requirements,
                Constraints: constraints,
                Deliverables: deliverables,
                ForbiddenActions:
                [
                    "held_out_training",
                    "exact_label_replay",
                    "semantic_quality_claim",
                    "hidden_deterministic_shortcut"
                ],
                RequiredHosts: ["core_only"],
                EvidenceNeeds: evidence,
                SourceProvenance: sourceProvenance,
                SplitId: "orchestration-reasoning-expanded-v1-heldout"),
            GuideDerivedStateRefs: ["leaf-local:or-expanded-v1-generalization-repair"],
            CorpusBundleIds: ["orchestration-reasoning-expanded-v1"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: runtimePolicyRefs);

    private static IReadOnlyList<string> BuildUniversalDecisionSignals(ExpandedRow row)
        =>
        [
            row.ProvenanceRefs.GetValueOrDefault("teaches_boundary") ?? "",
            StripQualityReasonBoilerplate(row.ProvenanceRefs.GetValueOrDefault("chosen_quality_reason") ?? ""),
            StripQualityReasonBoilerplate(row.ProvenanceRefs.GetValueOrDefault("rejected_quality_reason") ?? "")
        ];

    private static IReadOnlyList<string> BuildExpectedDecisionStructureRefs(ExpandedRow row)
        => BuildUniversalDecisionSignals(row)
            .SelectMany(SplitTransferableSignalTokens)
            .Select(token => $"decision-token:{token}")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static OrchestrationReasoningScenarioEvaluationReport EvaluateSourceOnlyControls(
        OrchestrationReasoningScenarioUnit unit,
        IReadOnlyList<ExpandedRow> heldOutRows)
        => unit.EvaluateHeldOutRows(heldOutRows
            .Select(row => new OrchestrationReasoningScenarioEvaluationRow(
                RowId: $"{row.TrainingPairId}:source-only-control",
                Request: CreateSourceOnlyControlRequest(row),
                ExpectedPatternId: "",
                ExpectedScenarioRef: row.ChosenScenarioRef,
                ExpectedDecisionStructureRefs: BuildExpectedDecisionStructureRefs(row)))
            .ToArray());

    private static OrchestrationReasoningScenarioEvaluationReport EvaluateBoilerplateOnlyControls(
        OrchestrationReasoningScenarioUnit unit,
        IReadOnlyList<ExpandedRow> heldOutRows)
        => unit.EvaluateHeldOutRows(heldOutRows
            .Select(row => new OrchestrationReasoningScenarioEvaluationRow(
                RowId: $"{row.TrainingPairId}:boilerplate-only-control",
                Request: CreateBoilerplateOnlyControlRequest(row),
                ExpectedPatternId: "",
                ExpectedScenarioRef: row.ChosenScenarioRef,
                ExpectedDecisionStructureRefs: BuildExpectedDecisionStructureRefs(row)))
            .ToArray());

    private static string StripQualityReasonBoilerplate(string value)
    {
        var trimmed = value.Trim();
        foreach (var prefix in new[] { "Chosen plan ", "Rejected plan " })
        {
            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return trimmed[prefix.Length..].TrimStart();
        }

        return trimmed;
    }

    private static GeneralizationRowResult BuildRowResult(
        OrchestrationReasoningScenarioUnit unit,
        ExpandedRow row)
    {
        var expectedRefs = BuildExpectedDecisionStructureRefs(row);
        var nearest = unit.FindNearestOrchestrationGuidePatterns(
            new OrchestrationReasoningNearestQuery(
                RequestId: $"or-expanded-v1-row-result-{row.TrainingPairId}",
                TaskType: "universal_reasoning",
                ConstraintSignals: BuildUniversalDecisionSignals(row),
                RequirementSignals: ["source disjoint evaluation", "no exact held-out scenario label replay"],
                DeliverableSignals: ["transferable decision structure evidence"],
                EvidenceSignals: ["leakage ablation evidence", "no semantic quality claim"],
                RuntimePolicyRefs: new Dictionary<string, string>
                {
                    ["deterministic_shortcut"] = "forbidden"
                }),
            k: 8);
        var observedRefs = nearest
            .SelectMany(guide => guide.DecisionStructureRefs ?? [])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var exactScenarioReplay = nearest.Any(guide => string.Equals(
            guide.ScenarioRef,
            row.ChosenScenarioRef,
            StringComparison.Ordinal));
        var matchedRefs = expectedRefs
            .Where(expected => observedRefs.Contains(expected, StringComparer.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new GeneralizationRowResult(
            RowId: row.TrainingPairId,
            SourceRowId: row.SourceRowId,
            ExpectedScenarioRef: row.ChosenScenarioRef,
            ExpectedDecisionStructureRefs: expectedRefs,
            ObservedDecisionStructureRefs: observedRefs,
            MatchedDecisionStructureRefs: matchedRefs,
            TopPatternId: nearest.FirstOrDefault()?.PatternId ?? "",
            TopScenarioRef: nearest.FirstOrDefault()?.ScenarioRef ?? "",
            ExactScenarioReplay: exactScenarioReplay,
            Passed: matchedRefs.Length > 0 && !exactScenarioReplay,
            MatchKind: nearest.FirstOrDefault()?.MatchKind ?? "no_match");
    }

    private static IReadOnlyList<ExpandedRow> ReadExpandedRows()
        => File.ReadAllLines(RepoPath(
                "training-data",
                "smart-skills",
                "semantic-quality",
                "orchestration-reasoning-expanded-v1",
                "orchestration-reasoning-expanded-dpo.jsonl"), Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<ExpandedRow>(line, JsonOptions)!)
            .ToArray();

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
        if (TokenStopWords.Contains(token))
            return false;
        if (token.Count(char.IsDigit) > token.Length / 2)
            return false;
        if (token.StartsWith("sha", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    private static void WriteEvidenceArtifactsIfRequested(
        OrchestrationReasoningScenarioEvaluationReport report,
        IReadOnlyList<GeneralizationRowResult> rowResults,
        OrchestrationReasoningScenarioEvaluationReport sourceOnlyReport,
        OrchestrationReasoningScenarioEvaluationReport boilerplateOnlyReport)
    {
        var evidenceDir = Environment.GetEnvironmentVariable("OR_EXPANDED_V1_GENERALIZATION_EVIDENCE_DIR");
        if (string.IsNullOrWhiteSpace(evidenceDir))
            return;

        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(
            Path.Combine(evidenceDir, "expanded-v1-generalization-eval-summary.json"),
            JsonSerializer.Serialize(report, JsonOptions),
            Encoding.UTF8);
        File.WriteAllText(
            Path.Combine(evidenceDir, "expanded-v1-generalization-eval-results.jsonl"),
            string.Join(Environment.NewLine, rowResults.Select(row => JsonSerializer.Serialize(row, JsonLineOptions))) + Environment.NewLine,
            Encoding.UTF8);
        File.WriteAllText(
            Path.Combine(evidenceDir, "leakage-ablation-report.json"),
            JsonSerializer.Serialize(
                new
                {
                    schema_version = 1,
                    source_only_controls_total = sourceOnlyReport.HeldOutRowCount,
                    source_only_controls_passed = sourceOnlyReport.StructureCorrectCount,
                    boilerplate_only_controls_total = boilerplateOnlyReport.HeldOutRowCount,
                    boilerplate_only_controls_passed = boilerplateOnlyReport.StructureCorrectCount,
                    exact_scenario_replay_rows = rowResults.Count(row => row.ExactScenarioReplay),
                    filtered_label_like_decision_tokens = new[] { "decision-token:chosen", "decision-token:plan" },
                    label_like_decision_tokens_observed = rowResults
                        .SelectMany(row => row.ObservedDecisionStructureRefs)
                        .Where(item => string.Equals(item, "decision-token:chosen", StringComparison.Ordinal)
                            || string.Equals(item, "decision-token:plan", StringComparison.Ordinal))
                        .Distinct(StringComparer.Ordinal)
                        .Order(StringComparer.Ordinal)
                        .ToArray(),
                    ablation_rows = new[]
                    {
                        new
                        {
                            control_id = "source-id-and-exact-scenario-ref-only-control",
                            controls_total = sourceOnlyReport.HeldOutRowCount,
                            controls_passed = sourceOnlyReport.StructureCorrectCount,
                            passes_thresholds = sourceOnlyReport.PassesThresholds,
                            proves = "Source ids, pair ids, manifest refs, and expected scenario refs do not pass the transferable structure gate."
                        },
                        new
                        {
                            control_id = "boilerplate-only-label-token-control",
                            controls_total = boilerplateOnlyReport.HeldOutRowCount,
                            controls_passed = boilerplateOnlyReport.StructureCorrectCount,
                            passes_thresholds = boilerplateOnlyReport.PassesThresholds,
                            proves = "chosen_quality_reason/rejected_quality_reason labels plus chosen/plan boilerplate tokens do not pass."
                        }
                    },
                    hidden_deterministic_shortcut_used_as_semantic_quality = false,
                    semantic_quality_claimed = false,
                    host_runtime_claimed = false,
                    notes = new[]
                    {
                        "Structure mode uses sanitized universal decision signals and filters label-like decision-token refs.",
                        "Source row ids, source manifest refs, pair ids, and exact held-out scenario refs are not accepted as final authority.",
                        "Boilerplate-only chosen/rejected quality labels and chosen/plan tokens are not accepted as transferable structure.",
                        "This remains a transparent deterministic baseline, not semantic quality evidence."
                    }
                },
                JsonOptions),
            Encoding.UTF8);
    }

    private static string RepoPath(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return Path.Combine(new[] { directory!.FullName }.Concat(parts).ToArray());
    }

    private sealed class ExpandedRow
    {
        public string TrainingPairId { get; init; } = "";

        public string SourceRowId { get; init; } = "";

        public string ApprovedPairRef { get; init; } = "";

        public string ChosenScenarioRef { get; init; } = "";

        public string RejectedScenarioRef { get; init; } = "";

        public string ErrorCode { get; init; } = "";

        public string ErrorTaxonomyRef { get; init; } = "";

        public string SourceGuideSha256 { get; init; } = "";

        public string SourceManifestRef { get; init; } = "";

        public string SplitId { get; init; } = "";

        public string SplitRole { get; init; } = "";

        public string HeldOutSetId { get; init; } = "";

        public string NearMissStrategyRef { get; init; } = "";

        public bool ContainsRawPrivateText { get; init; }

        public Dictionary<string, string> ProvenanceRefs { get; init; } = new(StringComparer.Ordinal);
    }

    private sealed record GeneralizationRowResult(
        string RowId,
        string SourceRowId,
        string ExpectedScenarioRef,
        IReadOnlyList<string> ExpectedDecisionStructureRefs,
        IReadOnlyList<string> ObservedDecisionStructureRefs,
        IReadOnlyList<string> MatchedDecisionStructureRefs,
        string TopPatternId,
        string TopScenarioRef,
        bool ExactScenarioReplay,
        bool Passed,
        string MatchKind);
}
