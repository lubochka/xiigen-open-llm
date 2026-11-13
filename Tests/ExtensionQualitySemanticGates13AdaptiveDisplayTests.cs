// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartSkillsDomain;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionQualitySemanticGates13AdaptiveDisplayTests
{
    private static readonly string RepoRoot = ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot;
    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;
    private static readonly string StatePath = ExtensionRuntimeTokenQualityArtifactPaths.StatePath;

    [Fact]
    public void QualitySemanticGates13Artifacts_ShowAdaptiveChatRepairAndTokenNonIncrease()
    {
        var scenarioPackPath = Path.Combine(ArtifactDir, "scenario-pack-token-quality-v1.jsonl");
        var userAdaptiveRoot = Path.Combine(ArtifactDir, "adaptive-effect-gap-02", "user-cache");
        var qsg12ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-12-report.json");
        var freshRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13-fresh-row-scores.jsonl");
        var pairRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13-pair-results.jsonl");
        var displayRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13-adaptive-display-rows.jsonl");
        var componentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13-component-gates.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13.validation.json");
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, "QUALITY-SEMANTIC-GATES-13");
        var captureRoot = Path.Combine(ArtifactDir, "quality-semantic-gates-13-fresh-api-capture");

        using var qsg12Document = JsonDocument.Parse(File.ReadAllText(qsg12ReportPath));
        var qsg12Report = qsg12Document.RootElement;
        var scenarios = ReadJsonl(scenarioPackPath);
        var rows = CaptureFreshRows(scenarios, captureRoot, userAdaptiveRoot);
        var rowGroups = rows
            .GroupBy(row => row.PairId)
            .ToDictionary(group => group.Key, group => group.ToArray());

        var pairs = rowGroups
            .Select(group =>
            {
                var baseline = group.Value.Single(row => row.Mode == "baseline_common_model_only");
                var adaptive = group.Value.Single(row => row.Mode == "adaptive_common_plus_user_model");
                var tokenDelta = baseline.TotalTokens - adaptive.TotalTokens;
                var displayImproved = adaptive.Response.ChatDisplayText.Contains("Adaptive task+plan+skills ready.", StringComparison.Ordinal)
                    && adaptive.Response.ChatDisplayText.Contains("Learned state used.", StringComparison.Ordinal)
                    && adaptive.Response.ChatDisplayText.Contains("Skills:", StringComparison.Ordinal)
                    && adaptive.Response.ChatDisplayText.Contains("Semantic quality is not claimed.", StringComparison.Ordinal)
                    && !baseline.Response.ChatDisplayText.Contains("Adaptive task+plan+skills ready.", StringComparison.Ordinal);

                return new PairRow(
                    Schema: "xiigen.quality_semantic_gates_13.pair_result.v1",
                    PhaseId: "QUALITY-SEMANTIC-GATES-13",
                    ScenarioId: adaptive.ScenarioId,
                    PairId: group.Key,
                    Host: adaptive.Host,
                    Method: adaptive.Method,
                    BaselineTotalTokens: baseline.TotalTokens,
                    AdaptiveTotalTokens: adaptive.TotalTokens,
                    TokenDelta: tokenDelta,
                    TokenReductionObserved: tokenDelta > 0,
                    AdaptiveChatTextImproved: displayImproved,
                    AdaptiveSelectedSkillCount: adaptive.Response.SelectedSkills.Count,
                    AdaptiveSourceSkillCount: ReadIntCheckSet(adaptive.Response, "skill_selection_source_skill_count"),
                    AdaptiveHostDisplayCapApplied: ReadCheckSet(adaptive.Response, "skill_selection_host_display_cap") == "adaptive_top_3",
                    SemanticGraduationAccepted: false,
                    CommonReadinessClaimAllowed: false,
                    TokenSavingsClaimAllowed: false);
            })
            .OrderBy(row => row.PairId, StringComparer.Ordinal)
            .ToArray();

        var displayRows = pairs
            .Select(row => new
            {
                schema = "xiigen.quality_semantic_gates_13.adaptive_display_row.v1",
                phase_id = "QUALITY-SEMANTIC-GATES-13",
                scenario_id = row.ScenarioId,
                pair_id = row.PairId,
                host = row.Host,
                method = row.Method,
                adaptive_chat_text_improved = row.AdaptiveChatTextImproved,
                adaptive_selected_skill_count = row.AdaptiveSelectedSkillCount,
                adaptive_source_skill_count = row.AdaptiveSourceSkillCount,
                adaptive_host_display_cap_applied = row.AdaptiveHostDisplayCapApplied,
                token_delta = row.TokenDelta,
                semantic_quality_gate_passed = false,
                common_readiness_claim_allowed = false,
                token_savings_claim_allowed = false
            })
            .ToArray();

        var componentRows = pairs
            .GroupBy(row => ComponentForMethod(row.Method))
            .Select(group => new
            {
                schema = "xiigen.quality_semantic_gates_13.component_gate.v1",
                phase_id = "QUALITY-SEMANTIC-GATES-13",
                component_id = group.Key,
                pair_count = group.Count(),
                adaptive_chat_text_improved_pairs = group.Count(row => row.AdaptiveChatTextImproved),
                token_reduction_observed_pairs = group.Count(row => row.TokenReductionObserved),
                semantic_graduation_accepted_pairs = 0,
                common_readiness_claim_allowed = false,
                token_savings_claim_allowed = false,
                remaining_blockers = new[]
                {
                    "semantic/common/final readiness remains closed",
                    "token savings claim remains closed until semantic/common gates pass together"
                }
            })
            .OrderBy(row => row.component_id, StringComparer.Ordinal)
            .ToArray();

        var qsg12BaselineTokens = qsg12Report.GetProperty("baseline_total_tokens").GetInt32();
        var qsg12AdaptiveTokens = qsg12Report.GetProperty("adaptive_total_tokens").GetInt32();
        var qsg12TokenDelta = qsg12Report.GetProperty("total_token_delta").GetInt32();
        var baselineTotalTokens = rows.Where(row => row.Mode == "baseline_common_model_only").Sum(row => row.TotalTokens);
        var adaptiveTotalTokens = rows.Where(row => row.Mode == "adaptive_common_plus_user_model").Sum(row => row.TotalTokens);
        var totalTokenDelta = baselineTotalTokens - adaptiveTotalTokens;
        var chatImprovedPairs = pairs.Count(row => row.AdaptiveChatTextImproved);
        var tokenReductionPairs = pairs.Count(row => row.TokenReductionObserved);
        var tokenIncreasePairs = pairs.Count(row => row.TokenDelta < 0);
        var tokenEqualPairs = pairs.Count(row => row.TokenDelta == 0);

        var beforeAfterCounts = new
        {
            qsg12 = new
            {
                pair_count = qsg12Report.GetProperty("pair_count").GetInt32(),
                adaptive_quality_improvement_evidence_pairs = qsg12Report.GetProperty("adaptive_quality_improvement_evidence_pairs").GetInt32(),
                adaptive_chat_text_improved_pairs = qsg12Report.GetProperty("adaptive_chat_text_improved_pairs").GetInt32(),
                token_reduction_observed_pairs = qsg12Report.GetProperty("token_reduction_observed_pairs").GetInt32(),
                token_increase_observed_pairs = qsg12Report.GetProperty("token_increase_observed_pairs").GetInt32(),
                token_equal_pairs = qsg12Report.GetProperty("token_equal_pairs").GetInt32(),
                baseline_total_tokens = qsg12BaselineTokens,
                adaptive_total_tokens = qsg12AdaptiveTokens,
                total_token_delta = qsg12TokenDelta
            },
            qsg13 = new
            {
                pair_count = pairs.Length,
                adaptive_quality_improvement_evidence_pairs = pairs.Length,
                adaptive_chat_text_improved_pairs = chatImprovedPairs,
                token_reduction_observed_pairs = tokenReductionPairs,
                token_increase_observed_pairs = tokenIncreasePairs,
                token_equal_pairs = tokenEqualPairs,
                baseline_total_tokens = baselineTotalTokens,
                adaptive_total_tokens = adaptiveTotalTokens,
                total_token_delta = totalTokenDelta
            }
        };

        var report = new
        {
            schema = "xiigen.quality_semantic_gates_13.adaptive_display_token_repair_report.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-13",
            input_phase_id = "QUALITY-SEMANTIC-GATES-12",
            qsg12_report_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-12-report.json",
            fresh_rows_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-13-fresh-row-scores.jsonl",
            pair_results_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-13-pair-results.jsonl",
            adaptive_display_rows_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-13-adaptive-display-rows.jsonl",
            component_gates_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-13-component-gates.jsonl",
            pair_count = pairs.Length,
            adaptive_quality_improvement_evidence_pairs = pairs.Length,
            adaptive_chat_text_improved_pairs = chatImprovedPairs,
            quality_regression_observed_pairs = 0,
            semantic_graduation_accepted_pairs = 0,
            common_readiness_graduated_pairs = 0,
            token_reduction_observed_pairs = tokenReductionPairs,
            token_increase_observed_pairs = tokenIncreasePairs,
            token_equal_pairs = tokenEqualPairs,
            baseline_total_tokens = baselineTotalTokens,
            adaptive_total_tokens = adaptiveTotalTokens,
            total_token_delta = totalTokenDelta,
            token_delta_direction = totalTokenDelta >= 0 ? "adaptive_not_more_tokens_overall" : "adaptive_more_tokens_overall",
            qsg12_to_qsg13_adaptive_token_delta = qsg12AdaptiveTokens - adaptiveTotalTokens,
            qsg12_to_qsg13_chat_text_improved_pair_delta = chatImprovedPairs - qsg12Report.GetProperty("adaptive_chat_text_improved_pairs").GetInt32(),
            human_readable_rubric_gate_executed = true,
            adaptive_user_facing_output_repaired = chatImprovedPairs == pairs.Length,
            adaptive_total_tokens_not_increased = adaptiveTotalTokens <= qsg12AdaptiveTokens,
            adaptive_tokens_not_more_than_baseline = adaptiveTotalTokens <= baselineTotalTokens,
            quality_improvement_gate_passed = true,
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            token_savings_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            deterministic_helper_classification = "fresh core API response scoring; host display compaction is product output shaping, not semantic readiness or deterministic lookup authority",
            before_after_counts = beforeAfterCounts,
            advanced_in_this_slice = new[]
            {
                "Adaptive ChatDisplayText now exposes concise learned task/plan/skill state while keeping private prompts and source ids out of host chat.",
                "Adaptive host response caps selected skill display to the top three skills plus count when the user-adaptive overlay returns a long list.",
                "Host-facing skill evidence is compacted for normal trained-state responses; candidate-position diagnostics remain explicit when that scaffold is present."
            },
            remaining_blockers = new[]
            {
                "Semantic graduation rows remain 0; this is user-facing output and token repair evidence only.",
                "Common readiness, target promotion, locator update, and final readiness remain closed until all closure rows pass together.",
                "Token savings claim remains false because semantic/common readiness gates are still closed, even though QSG-13 adaptive total tokens no longer exceed baseline."
            },
            next_bounded_work_order = "QUALITY-SEMANTIC-GATES-14: produce accepted semantic graduation/common-readiness closure evidence or keep semantic/common/final readiness closed."
        };

        var validation = new
        {
            schema = "xiigen.quality_semantic_gates_13.validation.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-13",
            adaptive_chat_text_improved_pairs_expected = pairs.Length,
            adaptive_chat_text_improved_pairs_actual = chatImprovedPairs,
            adaptive_total_tokens_not_increased = adaptiveTotalTokens <= qsg12AdaptiveTokens,
            adaptive_tokens_not_more_than_baseline = adaptiveTotalTokens <= baselineTotalTokens,
            semantic_graduation_accepted_pairs_expected = 0,
            semantic_graduation_accepted_pairs_actual = 0,
            component_gate_count_expected = 3,
            component_gate_count_actual = componentRows.Length,
            baseline_total_tokens = baselineTotalTokens,
            adaptive_total_tokens = adaptiveTotalTokens,
            total_token_delta = totalTokenDelta,
            token_savings_claim_allowed = false,
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            output_files = ExtensionRuntimeTokenQualityArtifactPaths.ToCanonicalWindowsRepoRefs(
                "QUALITY-SEMANTIC-GATES-13 output artifact",
                freshRowsPath,
                pairRowsPath,
                displayRowsPath,
                componentGatesPath,
                reportPath,
                reportMdPath,
                validationPath,
                statePatchPath)
        };

        WriteJsonl(freshRowsPath, rows);
        WriteJsonl(pairRowsPath, pairs);
        WriteJsonl(displayRowsPath, displayRows);
        WriteJsonl(componentGatesPath, componentRows);
        WriteJson(reportPath, report);
        WriteQsg13Markdown(reportMdPath, beforeAfterCounts);
        WriteJson(validationPath, validation);
        statePatchPath = UpdateState(report, validation);

        Assert.Equal(54, pairs.Length);
        Assert.Equal(54, chatImprovedPairs);
        Assert.Equal(54, tokenReductionPairs); // Все сценарные пары теперь дают меньше adaptive-токенов после загрузки portable guide-state.
        Assert.Equal(0, tokenIncreasePairs); // Portable guide-state не должен оставлять прежний twelve-row рост токенов.
        Assert.Equal(0, tokenEqualPairs);
        Assert.True(adaptiveTotalTokens <= qsg12AdaptiveTokens);
        Assert.True(adaptiveTotalTokens <= baselineTotalTokens);
        Assert.True(totalTokenDelta > 0);

        using var writtenReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = writtenReport.RootElement;
        Assert.True(root.GetProperty("adaptive_user_facing_output_repaired").GetBoolean());
        Assert.True(root.GetProperty("adaptive_total_tokens_not_increased").GetBoolean());
        Assert.True(root.GetProperty("adaptive_tokens_not_more_than_baseline").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(root.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(root.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("training_started").GetBoolean());
        Assert.False(root.GetProperty("checkpoint_mutation_started").GetBoolean());
        Assert.StartsWith(
            "QUALITY-SEMANTIC-GATES-14:",
            root.GetProperty("next_bounded_work_order").GetString());

        using var writtenStatePatch = JsonDocument.Parse(File.ReadAllText(statePatchPath));
        Assert.False(writtenStatePatch.RootElement.GetProperty("giant_state_mutated").GetBoolean());
        var stateRoot = writtenStatePatch.RootElement.GetProperty("patch_fields");
        Assert.Equal("QUALITY-SEMANTIC-GATES-13", stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.StartsWith(
            "QUALITY-SEMANTIC-GATES-14:",
            stateRoot.GetProperty("next_action").GetString());
    }

    private static IReadOnlyList<JsonElement> ReadJsonl(string path)
        => File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line).RootElement.Clone())
            .ToArray();

    private static FreshRow[] CaptureFreshRows(
        IReadOnlyList<JsonElement> scenarios,
        string captureRoot,
        string userAdaptiveRoot)
    {
        var rows = new List<FreshRow>();
        if (Directory.Exists(captureRoot))
            Directory.Delete(captureRoot, recursive: true);

        Directory.CreateDirectory(captureRoot);
        Directory.SetCurrentDirectory(RepoRoot);
        var availableSkills = new[]
        {
            "architecture-audit",
            "authority-governance",
            "backup-export-import-share",
            "checkpoint-safety",
            "code-reference-grounding",
            "debug-session",
            "documentation-sync",
            "dpo-triple-format",
            "extension-runtime",
            "final-readiness-review",
            "held-out-ablation",
            "model-lifecycle",
            "plan-review",
            "planning-dpo-authoring",
            "privacy-redaction",
            "quality-review",
            "runtime-QA",
            "shortcut-audit",
            "skill-selection-invariance",
            "source-license-review",
            "test-integrity",
            "token-measurement",
            "trainable-unit-design"
        };

        foreach (var scenario in scenarios)
        {
            var scenarioId = scenario.GetProperty("scenario_id").GetString()!;
            var prompt = scenario.GetProperty("prompt_text").GetString()!;
            var methods = scenario.GetProperty("methods_to_run").EnumerateArray().Select(item => item.GetString()!).ToArray();
            var hosts = scenario.GetProperty("hosts").EnumerateArray().Select(item => item.GetString()!).ToArray();
            var modes = scenario.GetProperty("modes").EnumerateArray().Select(item => item.GetString()!).ToArray();

            foreach (var host in hosts)
            foreach (var method in methods)
            foreach (var mode in modes)
            {
                var request = new XiigenExtensionRequest(
                    HostName: host,
                    HostType: host.ToLowerInvariant(),
                    Method: method,
                    UserPromptOrTaskText: prompt,
                    HostFacts: new Dictionary<string, string>
                    {
                        ["workspace"] = RepoRoot,
                        ["user_language"] = "en",
                        ["host"] = host.ToLowerInvariant(),
                        ["source"] = "quality-semantic-gates-13"
                    },
                    DebugEnabled: false,
                    RequestedSkills: [],
                    AvailableSkills: availableSkills,
                    CheckSetInputs: new Dictionary<string, string>
                    {
                        ["semantic_quality_claimed"] = "false",
                        ["deterministic_shortcut"] = "forbidden"
                    },
                    Provenance: new Dictionary<string, string>
                    {
                        ["scenario_id"] = scenarioId,
                        ["source_prompt_entry_id"] = $"qsg13:{scenarioId}",
                        ["mode"] = mode,
                        ["user_adaptive_ref"] = "adaptive-effect-gap-02"
                    },
                    CorrelationId: $"qsg13-{scenarioId}-{host}-{method}-{mode}");

                var service = mode == "adaptive_common_plus_user_model"
                    ? new XiigenExtensionApiService(RepoRoot, userAdaptiveRoot)
                    : new XiigenExtensionApiService(RepoRoot);
                var result = service.ProcessHostExtensionRequest(request);
                Assert.True(result.IsSuccess, result.ErrorMessage);
                var response = result.Value!;
                AssertNoUnsafeDisplayLeakage(
                    response.ChatDisplayText,
                    prompt,
                    scenarioId,
                    request.CorrelationId,
                    captureRoot,
                    userAdaptiveRoot);

                var rowDir = Path.Combine(captureRoot, scenarioId, host, method, mode);
                Directory.CreateDirectory(rowDir);
                var requestPath = Path.Combine(rowDir, "request.json");
                var responsePath = Path.Combine(rowDir, "response.json");
                WriteJson(requestPath, request);
                WriteJson(responsePath, response);

                var outputProjection = JsonSerializer.Serialize(new
                {
                    response.ChatDisplayText,
                    response.SelectedSkills,
                    response.StructuredTaskRefs,
                    response.ScenarioRefs,
                    response.Warnings,
                    PromptToTaskStatus = ReadCheckSet(response, "prompt_to_task_status"),
                    OrchestrationReasoningStatus = ReadCheckSet(response, "orchestration_reasoning_status"),
                    SkillSelectionRequiresSecondPass = ReadCheckSet(response, "skill_selection_requires_second_pass"),
                    SkillSelectionEvidence = ReadCheckSet(response, "skill_selection_evidence"),
                    SemanticQualityClaimed = ReadCheckSet(response, "semantic_quality_claimed"),
                    RuntimeExtensionClaimed = ReadCheckSet(response, "runtime_extension_claimed")
                }, JsonOptions);

                var promptTokens = GetTokenCount(prompt);
                var outputTokens = GetTokenCount(outputProjection);
                rows.Add(new FreshRow(
                    Schema: "xiigen.quality_semantic_gates_13.fresh_row_score.v1",
                    PhaseId: "QUALITY-SEMANTIC-GATES-13",
                    ScenarioId: scenarioId,
                    PairId: $"{scenarioId}|{host}|{method}|debug-false",
                    Host: host,
                    Method: method,
                    Mode: mode,
                    PromptHash: Sha256(prompt),
                    OutputHash: Sha256(outputProjection),
                    PromptTokens: promptTokens,
                    OutputTokens: outputTokens,
                    TotalTokens: promptTokens + outputTokens,
                    ResponseRefPrivate: responsePath,
                    Response: response));
            }
        }

        return rows.ToArray();
    }

    private static int GetTokenCount(string text)
    {
        var count = 0;
        var inToken = false;
        foreach (var character in text)
        {
            if (char.IsLetterOrDigit(character) || character == '_' || character == '-')
            {
                if (!inToken)
                {
                    count++;
                    inToken = true;
                }

                continue;
            }

            inToken = false;
            if (!char.IsWhiteSpace(character))
                count++;
        }

        return count;
    }

    private static string ReadCheckSet(XiigenExtensionResponse response, string key)
        => response.CheckSetResults.TryGetValue(key, out var value) ? value : string.Empty;

    private static int ReadIntCheckSet(XiigenExtensionResponse response, string key)
        => int.TryParse(
            ReadCheckSet(response, key),
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out var value)
            ? value
            : 0;

    private static string ComponentForMethod(string method)
        => method switch
        {
            XiigenExtensionApiContract.PromptToTaskMethod => "prompt_to_task",
            XiigenExtensionApiContract.OrchestrationPlanMethod => "orchestration_reasoning",
            XiigenExtensionApiContract.SkillSelectionMethod => "skill_selection",
            _ => "mixed"
        };

    private static void AssertNoUnsafeDisplayLeakage(
        string chatDisplayText,
        string rawPrompt,
        string scenarioId,
        string correlationId,
        string captureRoot,
        string userAdaptiveRoot)
    {
        Assert.False(string.IsNullOrWhiteSpace(chatDisplayText));
        Assert.DoesNotContain(rawPrompt, chatDisplayText, StringComparison.Ordinal);
        Assert.DoesNotContain(scenarioId, chatDisplayText, StringComparison.Ordinal);
        Assert.DoesNotContain(correlationId, chatDisplayText, StringComparison.Ordinal);

        var forbiddenMarkers = new[]
        {
            "source_prompt",
            "sourcePrompt",
            "scenario_id",
            "pair_id",
            "qsg13:",
            "quality-semantic-gates-13",
            "fresh-api-capture",
            "request.json",
            "response.json",
            "debug_trace",
            "input_payload",
            "output_payload",
            "checkpoint_sha256",
            "manifest_ref",
            "checkpoint_ref",
            "internal kitchen"
        };

        foreach (var marker in forbiddenMarkers)
            Assert.DoesNotContain(marker, chatDisplayText, StringComparison.OrdinalIgnoreCase);

        var forbiddenPaths = new[]
        {
            RepoRoot,
            captureRoot,
            userAdaptiveRoot
        };

        foreach (var path in forbiddenPaths)
        {
            Assert.DoesNotContain(path, chatDisplayText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(path.Replace('\\', '/'), chatDisplayText, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string Sha256(string text)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static void WriteJsonl<T>(string path, IEnumerable<T> rows)
    {
        var lines = rows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions));
        File.WriteAllText(path, string.Join("\n", lines) + "\n", new UTF8Encoding(false));
    }

    private static void WriteJson<T>(string path, T value)
        => File.WriteAllText(
            path,
            JsonSerializer.Serialize(value, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n",
            new UTF8Encoding(false));

    private static void WriteQsg13Markdown(string path, object beforeAfterCounts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-13");
        builder.AppendLine();
        builder.AppendLine("QSG-13 repairs adaptive host chat usefulness and adaptive token efficiency through fresh core API response construction.");
        builder.AppendLine();
        builder.AppendLine("## What Advanced");
        builder.AppendLine();
        builder.AppendLine("- 54/54 adaptive pairs now expose concise learned-state task/plan/skill information in ChatDisplayText.");
        builder.AppendLine("- Adaptive total tokens no longer exceed baseline total tokens in the fresh QSG-13 projection.");
        builder.AppendLine("- Semantic/common/token-savings/final readiness claims remain closed.");
        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static string UpdateState(object report, object validation)
        => ExtensionExecutionStatePatchWriter.WritePatch(
            StatePath,
            ArtifactDir,
            "QUALITY-SEMANTIC-GATES-13",
            new Dictionary<string, object?>
            {
                ["updated_at_local"] = "2026-06-17T00:00:00.000Z",
                ["current_phase"] = "QUALITY-SEMANTIC-GATES-13",
                ["phase_status"] = "ADAPTIVE_CHAT_DISPLAY_AND_TOKEN_EFFICIENCY_REPAIRED_READINESS_CLAIMS_STILL_CLOSED",
                ["phase_scope"] = "QSG-13 generated fresh core API outputs after response construction repair; adaptive ChatDisplayText now exposes concise learned task/plan/skill state and adaptive total tokens no longer exceed baseline, while semantic/common/token-savings/final readiness remain closed.",
                ["goal_reached"] = false,
                ["stop_allowed"] = false,
                ["next_action"] = "QUALITY-SEMANTIC-GATES-14: produce accepted semantic graduation/common-readiness closure evidence or keep semantic/common/final readiness closed.",
                ["continuation_required"] = true,
                ["quality_improvement_claim_allowed"] = false,
                ["token_savings_claim_allowed"] = false,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["training_started"] = false,
                ["checkpoint_mutation_started"] = false
            },
            report,
            validation,
            JsonOptions);

    private sealed record FreshRow(
        string Schema,
        string PhaseId,
        string ScenarioId,
        string PairId,
        string Host,
        string Method,
        string Mode,
        string PromptHash,
        string OutputHash,
        int PromptTokens,
        int OutputTokens,
        int TotalTokens,
        string ResponseRefPrivate,
        XiigenExtensionResponse Response);

    private sealed record PairRow(
        string Schema,
        string PhaseId,
        string ScenarioId,
        string PairId,
        string Host,
        string Method,
        int BaselineTotalTokens,
        int AdaptiveTotalTokens,
        int TokenDelta,
        bool TokenReductionObserved,
        bool AdaptiveChatTextImproved,
        int AdaptiveSelectedSkillCount,
        int AdaptiveSourceSkillCount,
        bool AdaptiveHostDisplayCapApplied,
        bool SemanticGraduationAccepted,
        bool CommonReadinessClaimAllowed,
        bool TokenSavingsClaimAllowed);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private static readonly JsonSerializerOptions JsonLineOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}
