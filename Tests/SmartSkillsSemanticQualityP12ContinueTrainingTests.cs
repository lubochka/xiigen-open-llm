// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using SmartSkillsDomain;

using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SmartSkillsSemanticQualityP12ContinueTrainingTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string P12Dir = Path.Combine(
        RepoRoot,
        "artifacts",
        "smart-skills-semantic-quality-completion",
        "p12-continue-training");

    [Fact]
    public async Task P12RecordsNoOpContinueAndPreservesHeldOutBehavior()
    {
        var reportPath = Path.Combine(P12Dir, "smart-skills-p12-continue-training-report.json");
        var resultRowsPath = Path.Combine(P12Dir, "smart-skills-p12-per-component-results.jsonl");
        var selfCheckPath = Path.Combine(P12Dir, "p12-self-check.json");
        var p10ManifestPath = Path.Combine(
            RepoRoot,
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p10-checkpoint-rewrite",
            "smart-skills-p10-component-manifests.json");

        using var reportDoc = JsonDocument.Parse(File.ReadAllText(reportPath, Encoding.UTF8));
        using var selfCheckDoc = JsonDocument.Parse(File.ReadAllText(selfCheckPath, Encoding.UTF8));
        using var p10ManifestDoc = JsonDocument.Parse(File.ReadAllText(p10ManifestPath, Encoding.UTF8));

        Assert.Equal("PASS_CANDIDATE_READY_FOR_PARENT_REVIEW", reportDoc.RootElement.GetProperty("verdict").GetString());
        Assert.Equal(0, reportDoc.RootElement.GetProperty("delta_path_component_count").GetInt32());
        Assert.Equal(2, reportDoc.RootElement.GetProperty("no_op_path_component_count").GetInt32());
        Assert.True(reportDoc.RootElement.GetProperty("no_op_continue_marker").GetBoolean());
        var focusedTestResult = reportDoc.RootElement
            .GetProperty("focused_test")
            .GetProperty("result")
            .GetString();
        Assert.NotEqual("pending_final_run", focusedTestResult);
        Assert.Contains("passed", focusedTestResult, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Failed=0", focusedTestResult, StringComparison.Ordinal);
        Assert.Contains("Passed=1", focusedTestResult, StringComparison.Ordinal);
        Assert.True(selfCheckDoc.RootElement.GetProperty("explicit_no_op_marker_recorded").GetBoolean());
        Assert.True(selfCheckDoc.RootElement.GetProperty("no_new_checkpoint_created").GetBoolean());
        Assert.False(GetSafetyFlag(reportDoc.RootElement, "semanticQualityClaimed"));
        Assert.False(GetSafetyFlag(reportDoc.RootElement, "fullApiSemanticQualityClaimed"));
        Assert.False(GetSafetyFlag(reportDoc.RootElement, "skillSelectionReadinessClaimed"));
        Assert.False(GetSafetyFlag(reportDoc.RootElement, "extensionRuntimeReadinessClaimed"));
        Assert.False(GetSafetyFlag(reportDoc.RootElement, "finalModelReadyClaimed"));

        var resultRows = File.ReadAllLines(resultRowsPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();
        Assert.Equal(2, resultRows.Length);

        var promptToTaskComponent = FindComponent(p10ManifestDoc.RootElement, "PromptToTask");
        var orchestrationComponent = FindComponent(p10ManifestDoc.RootElement, "OrchestrationReasoning");

        await VerifyPromptToTaskNoOpContinueAsync(promptToTaskComponent, resultRows.Single(row =>
            row.RootElement.GetProperty("component_id").GetString() == "PromptToTask"));
        await VerifyOrchestrationNoOpContinueAsync(orchestrationComponent, resultRows.Single(row =>
            row.RootElement.GetProperty("component_id").GetString() == "OrchestrationReasoning"));

        Assert.Empty(Directory.EnumerateFiles(P12Dir, "*continued*.*", SearchOption.AllDirectories));
        AssertNoMojibake(P12Dir);
    }

    private static async Task VerifyPromptToTaskNoOpContinueAsync(JsonElement component, JsonDocument resultRow)
    {
        Assert.NotNull(typeof(PromptToTaskMachine).GetMethod(
            "ContinueWithAcceptedPattern",
            BindingFlags.Instance | BindingFlags.Public));
        Assert.Equal("explicit_no_op", resultRow.RootElement.GetProperty("continue_path").GetString());
        Assert.False(resultRow.RootElement.GetProperty("approved_delta_material_found").GetBoolean());
        Assert.False(resultRow.RootElement.GetProperty("continue_api_invoked").GetBoolean());
        Assert.True(resultRow.RootElement.GetProperty("no_op_marker_recorded").GetBoolean());
        Assert.True(resultRow.RootElement.GetProperty("checkpoint_sha_stable_with_no_op_marker").GetBoolean());
        Assert.False(resultRow.RootElement.GetProperty("continued_checkpoint_created").GetBoolean());
        Assert.Equal(JsonValueKind.Null, resultRow.RootElement.GetProperty("continued_checkpoint_ref").ValueKind);

        var checkpointPath = ResolveRepoPath(component.GetProperty("candidate_checkpoint_ref").GetString()!);
        var manifestPath = ResolveRepoPath(component.GetProperty("candidate_manifest_ref").GetString()!);
        var expectedHash = component.GetProperty("candidate_checkpoint_sha256").GetString()!;
        Assert.Equal(expectedHash, Sha256File(checkpointPath));
        Assert.Equal(expectedHash, resultRow.RootElement.GetProperty("actual_previous_checkpoint_sha256").GetString());

        var load = await PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.Equal(resultRow.RootElement.GetProperty("previous_accepted_ref_count").GetInt32(), load.Value!.Checkpoint.AcceptedRefIds.Count);
        Assert.Equal(resultRow.RootElement.GetProperty("learned_pattern_count_before").GetInt32(), load.Value.Checkpoint.LearnedState.LearnedPatterns!.Count);
        Assert.Equal(resultRow.RootElement.GetProperty("learned_pattern_count_after").GetInt32(), load.Value.Checkpoint.LearnedState.LearnedPatterns!.Count);

        var request = BuildPromptToTaskHeldOutRequest();
        var response = load.Value.Machine.Translate(request);
        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, response.Value!.ContractStatus);
        Assert.Equal("p7f-pattern-task-contract-planning", response.Value.StructuredTask.SourceProvenance["learned_pattern_id"]);
        Assert.True(resultRow.RootElement.GetProperty("held_out_non_regression_passed").GetBoolean());
    }

    private static async Task VerifyOrchestrationNoOpContinueAsync(JsonElement component, JsonDocument resultRow)
    {
        Assert.NotNull(typeof(OrchestrationReasoningMachine).GetMethod(
            "ContinueTrainingFromLearnedState",
            BindingFlags.Instance | BindingFlags.Public));
        Assert.Equal("explicit_no_op", resultRow.RootElement.GetProperty("continue_path").GetString());
        Assert.False(resultRow.RootElement.GetProperty("approved_delta_material_found").GetBoolean());
        Assert.False(resultRow.RootElement.GetProperty("continue_api_invoked").GetBoolean());
        Assert.True(resultRow.RootElement.GetProperty("no_op_marker_recorded").GetBoolean());
        Assert.True(resultRow.RootElement.GetProperty("checkpoint_sha_stable_with_no_op_marker").GetBoolean());
        Assert.False(resultRow.RootElement.GetProperty("continued_checkpoint_created").GetBoolean());
        Assert.Equal(JsonValueKind.Null, resultRow.RootElement.GetProperty("continued_checkpoint_ref").ValueKind);

        var checkpointPath = ResolveRepoPath(component.GetProperty("candidate_checkpoint_ref").GetString()!);
        var manifestPath = ResolveRepoPath(component.GetProperty("candidate_manifest_ref").GetString()!);
        var expectedHash = component.GetProperty("candidate_checkpoint_sha256").GetString()!;
        Assert.Equal(expectedHash, Sha256File(checkpointPath));
        Assert.Equal(expectedHash, resultRow.RootElement.GetProperty("actual_previous_checkpoint_sha256").GetString());

        var load = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.Equal(resultRow.RootElement.GetProperty("previous_accepted_ref_count").GetInt32(), load.Value!.Checkpoint.AcceptedRefIds.Count);
        Assert.Equal(resultRow.RootElement.GetProperty("learned_pattern_count_before").GetInt32(), load.Value.Checkpoint.LearnedState.LearnedPatterns!.Count);
        Assert.Equal(resultRow.RootElement.GetProperty("learned_pattern_count_after").GetInt32(), load.Value.Checkpoint.LearnedState.LearnedPatterns!.Count);

        var request = BuildOrchestrationHeldOutRequest();
        var response = load.Value.Machine.Plan(request);
        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, response.Value!.ContractStatus);
        Assert.Equal("or-p9a-pattern-branch-review-targeted-repair", response.Value.ScenarioPlan.SelectedLearnedPatterns.Single());
        Assert.Equal("scenario:branch-review-targeted-repair", response.Value.ScenarioPlan.ScenarioId);
        Assert.True(resultRow.RootElement.GetProperty("held_out_non_regression_passed").GetBoolean());
    }

    private static PromptToTaskRequest BuildPromptToTaskHeldOutRequest()
    {
        var prompt = "redacted held-out request asks for a task contract with constraints and validation boundaries";
        return new PromptToTaskRequest(
            PromptId: "p12-heldout-prompt-to-task-contract",
            RawPrompt: prompt,
            RawPromptSha256: Sha256Text(prompt),
            PromptProvenance: "p7g_reserved_positive_redacted_surrogate",
            UserLanguage: "ru",
            ExplicitConstraints:
            [
                "intent",
                "task_contract",
                "constraint_boundary",
                "expected_result_contract",
                "answer_contract",
                "validation_boundary",
                "positive_negative_examples",
                "input_summary"
            ],
            ContextFields: new Dictionary<string, string>
            {
                ["redaction"] = "no_raw_private_prompt_text",
                ["source_disjoint_role"] = "held_out_component_sample"
            });
    }

    private static OrchestrationReasoningRequest BuildOrchestrationHeldOutRequest()
    {
        var prompt = "redacted structured task asks for diagnosis before a bounded repair strategy";
        var task = new StructuredTaskContract(
            PromptId: "p12-heldout-orchestration-branch-review",
            RawPromptSha256: Sha256Text(prompt),
            UserLanguage: "ru",
            TaskType: "orchestration_reasoning",
            ExplicitUserRequirements:
            [
                "compare_actual_to_expected",
                "preserve_unaffected_work"
            ],
            Constraints:
            [
                "diagnose_failure_before_repair",
                "choose_one_repair_strategy",
                "targeted_repair_only"
            ],
            Deliverables:
            [
                "repair_strategy_decision",
                "bounded_repair_plan"
            ],
            ForbiddenActions:
            [
                "no_training",
                "no_final_model_overwrite"
            ],
            RequiredHosts: [],
            EvidenceNeeds:
            [
                "failure_evidence",
                "done_criteria_evidence"
            ],
            SourceProvenance: new Dictionary<string, string>
            {
                ["redaction"] = "no_raw_private_prompt_text",
                ["source_disjoint_role"] = "held_out_component_sample"
            },
            SplitId: "p12-heldout-component-sample");

        return new OrchestrationReasoningRequest(
            RequestId: "p12-heldout-orchestration-branch-review",
            Task: task,
            GuideDerivedStateRefs: [],
            CorpusBundleIds: [],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["runtime_boundary"] = "component_no_op_continue_only"
            });
    }

    private static JsonElement FindComponent(JsonElement root, string componentId)
        => root.GetProperty("components")
            .EnumerateArray()
            .Single(component => component.GetProperty("component_id").GetString() == componentId);

    private static string ResolveRepoPath(string repoRelativeOrAbsolute)
    {
        if (Path.IsPathRooted(repoRelativeOrAbsolute))
        {
            return repoRelativeOrAbsolute;
        }

        return Path.GetFullPath(Path.Combine(RepoRoot, repoRelativeOrAbsolute.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static bool GetSafetyFlag(JsonElement root, string propertyName)
        => root.GetProperty("safety_flags").GetProperty(propertyName).GetBoolean();

    private static void AssertNoMojibake(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file, Encoding.UTF8);
            Assert.DoesNotContain('\u00D0', text);
            Assert.DoesNotContain('\u00D1', text);
            Assert.DoesNotContain('\uFFFD', text);
            Assert.DoesNotContain('\u00E2', text);
            Assert.DoesNotContain('\u00C3', text);
            Assert.DoesNotContain(new string('?', 4), text);
        }
    }

    private static string FindRepoRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "AGENTS.md"))
                && Directory.Exists(Path.Combine(current, "artifacts")))
            {
                return current;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
