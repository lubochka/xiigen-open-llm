// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using SmartSkillsDomain;

using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SmartSkillsSemanticQualityP11FreshLoadTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string P11Dir = Path.Combine(
        RepoRoot,
        "artifacts",
        "smart-skills-semantic-quality-completion",
        "p11-fresh-load");

    [Fact]
    public async Task P11FreshLoadsComponentCheckpointsThroughProjectPaths()
    {
        var reportPath = Path.Combine(P11Dir, "smart-skills-p11-fresh-load-report.json");
        var reportMarkdownPath = Path.Combine(P11Dir, "smart-skills-p11-fresh-load-report.md");
        var resultRowsPath = Path.Combine(P11Dir, "smart-skills-p11-per-component-results.jsonl");
        var selfCheckPath = Path.Combine(P11Dir, "p11-self-check.json");
        var parentReviewPath = Path.Combine(P11Dir, "p11-parent-review.json");
        var parentReviewMarkdownPath = Path.Combine(P11Dir, "p11-parent-review.md");
        var evidenceCardPath = Path.Combine(P11Dir, "p11-evidence-card.md");
        var reviewCyclesPath = Path.Combine(P11Dir, "p11-review-cycles.jsonl");
        var p10ManifestPath = Path.Combine(
            RepoRoot,
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p10-checkpoint-rewrite",
            "smart-skills-p10-component-manifests.json");

        using var reportDoc = JsonDocument.Parse(File.ReadAllText(reportPath, Encoding.UTF8));
        using var selfCheckDoc = JsonDocument.Parse(File.ReadAllText(selfCheckPath, Encoding.UTF8));
        using var parentReviewDoc = JsonDocument.Parse(File.ReadAllText(parentReviewPath, Encoding.UTF8));
        using var p10ManifestDoc = JsonDocument.Parse(File.ReadAllText(p10ManifestPath, Encoding.UTF8));

        Assert.Equal(
            "PARTIAL_PASS_WITH_PROVENANCE_REBUILD_REQUIRED",
            reportDoc.RootElement.GetProperty("verdict").GetString());
        Assert.Equal(1, reportDoc.RootElement.GetProperty("component_pass_count").GetInt32());
        Assert.Equal(0, reportDoc.RootElement.GetProperty("component_fail_count").GetInt32());
        Assert.Equal(
            1,
            reportDoc.RootElement.GetProperty("component_provenance_rebuild_required_count").GetInt32());
        var focusedTestResult = reportDoc.RootElement
            .GetProperty("focused_test")
            .GetProperty("result")
            .GetString();
        Assert.NotEqual("pending_final_run", focusedTestResult);
        Assert.Contains("passed", focusedTestResult, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Failed=0", focusedTestResult, StringComparison.Ordinal);
        Assert.Contains("Passed=1", focusedTestResult, StringComparison.Ordinal);
        Assert.True(selfCheckDoc.RootElement.GetProperty("project_load_path_exercised").GetBoolean());
        Assert.True(selfCheckDoc.RootElement.GetProperty("supported_component_project_load_path_exercised").GetBoolean());
        Assert.False(selfCheckDoc.RootElement.GetProperty("import_async_paths_exist_and_exercised").GetBoolean());
        Assert.True(selfCheckDoc.RootElement.GetProperty("supported_component_import_async_paths_exist_and_exercised").GetBoolean());
        Assert.True(selfCheckDoc.RootElement.GetProperty("provenance_rebuild_required_components_classified").GetBoolean());
        Assert.False(selfCheckDoc.RootElement.GetProperty("held_out_component_samples_passed_after_fresh_load").GetBoolean());
        Assert.True(selfCheckDoc.RootElement.GetProperty("supported_component_held_out_sample_passed_after_fresh_load").GetBoolean());
        Assert.False(GetSafetyFlag(reportDoc.RootElement, "semanticQualityClaimed"));
        Assert.False(GetSafetyFlag(reportDoc.RootElement, "fullApiSemanticQualityClaimed"));
        Assert.False(GetSafetyFlag(reportDoc.RootElement, "skillSelectionReadinessClaimed"));
        Assert.False(GetSafetyFlag(reportDoc.RootElement, "extensionRuntimeReadinessClaimed"));
        Assert.False(GetSafetyFlag(reportDoc.RootElement, "finalModelReadyClaimed"));
        Assert.Equal(
            "PARTIAL_PASS_WITH_PROVENANCE_REBUILD_REQUIRED",
            parentReviewDoc.RootElement.GetProperty("verdict").GetString());
        Assert.False(parentReviewDoc.RootElement.GetProperty("accepted_as_two_component_fresh_load_lifecycle_evidence").GetBoolean());
        Assert.True(parentReviewDoc.RootElement.GetProperty("accepted_as_supported_component_fresh_load_lifecycle_evidence").GetBoolean());
        var parentVerifiedFacts = parentReviewDoc.RootElement.GetProperty("verified_facts");
        Assert.Equal(1, parentVerifiedFacts.GetProperty("component_pass_count").GetInt32());
        Assert.Equal(1, parentVerifiedFacts.GetProperty("component_provenance_rebuild_required_count").GetInt32());
        Assert.True(parentVerifiedFacts.GetProperty("orchestration_reasoning_provenance_rebuild_required").GetBoolean());
        Assert.False(parentVerifiedFacts.TryGetProperty("git_diff_check_passed", out _));
        Assert.Contains(
            "bounded P11/P12/P7 artifact and test paths only",
            parentVerifiedFacts.GetProperty("diff_check_scope").GetString(),
            StringComparison.Ordinal);
        AssertPartialBoundaryMarkdown(reportMarkdownPath);
        AssertPartialBoundaryMarkdown(parentReviewMarkdownPath);
        AssertPartialBoundaryMarkdown(evidenceCardPath);
        var reviewCycleLines = File.ReadAllLines(reviewCyclesPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
        Assert.Equal(4, reviewCycleLines.Length);
        Assert.Contains(reviewCycleLines, line => line.Contains("provenance_rebuild_required", StringComparison.Ordinal));
        Assert.Contains(reviewCycleLines, line => line.Contains("\"result\":\"partial_pass\"", StringComparison.Ordinal)
            && line.Contains("PromptToTask", StringComparison.Ordinal));

        var resultRows = File.ReadAllLines(resultRowsPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();
        Assert.Equal(2, resultRows.Length);

        var promptToTaskComponent = FindComponent(p10ManifestDoc.RootElement, "PromptToTask");
        var orchestrationComponent = FindComponent(p10ManifestDoc.RootElement, "OrchestrationReasoning");

        await VerifyPromptToTaskFreshLoadAsync(promptToTaskComponent, resultRows.Single(row =>
            row.RootElement.GetProperty("component_id").GetString() == "PromptToTask"));
        await VerifyOrchestrationProvenanceRebuildRequiredAsync(
            orchestrationComponent,
            resultRows.Single(row =>
                row.RootElement.GetProperty("component_id").GetString() == "OrchestrationReasoning"),
            FindComponent(reportDoc.RootElement, "OrchestrationReasoning"));

        AssertNoMojibake(P11Dir);
    }

    private static async Task VerifyPromptToTaskFreshLoadAsync(JsonElement component, JsonDocument resultRow)
    {
        var checkpointPath = ResolveRepoPath(component.GetProperty("candidate_checkpoint_ref").GetString()!);
        var manifestPath = ResolveRepoPath(component.GetProperty("candidate_manifest_ref").GetString()!);
        var expectedHash = component.GetProperty("candidate_checkpoint_sha256").GetString()!;
        Assert.Equal(expectedHash, Sha256File(checkpointPath));
        Assert.True(resultRow.RootElement.GetProperty("hash_match").GetBoolean());

        var request = BuildPromptToTaskHeldOutRequest();
        var freshMachine = new PromptToTaskMachine();
        var preLoad = freshMachine.Translate(request);
        Assert.True(preLoad.IsSuccess, preLoad.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.NotTrainedStatus, preLoad.Value!.ContractStatus);

        var load = await PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        var loadedResponse = load.Value!.Machine.Translate(request);
        Assert.True(loadedResponse.IsSuccess, loadedResponse.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, loadedResponse.Value!.ContractStatus);
        Assert.Equal("p7f-pattern-task-contract-planning", loadedResponse.Value.StructuredTask.SourceProvenance["learned_pattern_id"]);
        Assert.Equal("PromptToTask.ContractFromUserPrompt", loadedResponse.Value.StructuredTask.TaskType);

        var importedMachine = new PromptToTaskMachine();
        var export = new PromptToTaskLifecycleExport(
            PromptToTaskMachine.MachineId,
            PromptToTaskMachine.DomainTag,
            PromptToTaskMachine.Version,
            checkpointPath,
            manifestPath,
            expectedHash,
            DateTimeOffset.UtcNow);
        var import = await importedMachine.ImportAsync(export);
        Assert.True(import.IsSuccess, import.ErrorMessage);
        var importedResponse = importedMachine.Translate(request);
        Assert.True(importedResponse.IsSuccess, importedResponse.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, importedResponse.Value!.ContractStatus);
        Assert.Equal("component_task_contract_produced_after_fresh_load", resultRow.RootElement.GetProperty("observed_component_output").GetString());
        Assert.True(resultRow.RootElement.GetProperty("expected_component_output_observed").GetBoolean());
    }

    private static async Task VerifyOrchestrationProvenanceRebuildRequiredAsync(
        JsonElement component,
        JsonDocument resultRow,
        JsonElement reportComponent)
    {
        var checkpointPath = ResolveRepoPath(component.GetProperty("candidate_checkpoint_ref").GetString()!);
        var manifestPath = ResolveRepoPath(component.GetProperty("candidate_manifest_ref").GetString()!);
        var expectedHash = component.GetProperty("candidate_checkpoint_sha256").GetString()!;
        Assert.Equal(expectedHash, Sha256File(checkpointPath));
        Assert.True(resultRow.RootElement.GetProperty("hash_match").GetBoolean());

        var request = BuildOrchestrationHeldOutRequest();
        var freshMachine = new OrchestrationReasoningMachine();
        var preLoad = freshMachine.Plan(request);
        Assert.True(preLoad.IsSuccess, preLoad.ErrorMessage);
        Assert.Equal(OrchestrationReasoningMachine.NotTrainedStatus, preLoad.Value!.ContractStatus);

        var load = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.False(load.IsSuccess);
        Assert.Equal(
            OrchestrationReasoningSourceAdmission.ProvenanceRebuildRequiredCode,
            load.ErrorCode);
        Assert.Contains(
            "provenance rebuild",
            load.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            "provenance_rebuild_required",
            resultRow.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            OrchestrationReasoningSourceAdmission.ProvenanceRebuildRequiredCode,
            resultRow.RootElement.GetProperty("load_error_code").GetString());
        Assert.Equal(
            "legacy_schema_v1_common_checkpoint_requires_source_admission_rebuild",
            resultRow.RootElement.GetProperty("observed_component_output").GetString());
        Assert.False(resultRow.RootElement.GetProperty("expected_component_output_observed").GetBoolean());
        Assert.False(resultRow.RootElement.GetProperty("component_lifecycle_pass").GetBoolean());
        Assert.True(resultRow.RootElement.GetProperty("provenance_rebuild_required").GetBoolean());

        Assert.Equal(
            "provenance_rebuild_required",
            reportComponent.GetProperty("status").GetString());
        Assert.False(reportComponent.GetProperty("component_lifecycle_pass").GetBoolean());
        Assert.True(reportComponent.GetProperty("provenance_rebuild_required").GetBoolean());
    }

    private static PromptToTaskRequest BuildPromptToTaskHeldOutRequest()
    {
        var prompt = "redacted held-out request asks for a task contract with constraints and validation boundaries";
        var signals = new[]
        {
            "intent",
            "task_contract",
            "constraint_boundary",
            "expected_result_contract",
            "answer_contract",
            "validation_boundary",
            "positive_negative_examples",
            "input_summary"
        };
        return new PromptToTaskRequest(
            PromptId: "p11-heldout-prompt-to-task-contract",
            RawPrompt: prompt,
            RawPromptSha256: Sha256Text(prompt),
            PromptProvenance: "p7g_reserved_positive_redacted_surrogate",
            UserLanguage: "ru",
            ExplicitConstraints: signals,
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
            PromptId: "p11-heldout-orchestration-branch-review",
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
            SplitId: "p11-heldout-component-sample");

        return new OrchestrationReasoningRequest(
            RequestId: "p11-heldout-orchestration-branch-review",
            Task: task,
            GuideDerivedStateRefs: [],
            CorpusBundleIds: [],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["runtime_boundary"] = "component_fresh_load_only"
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

    private static void AssertPartialBoundaryMarkdown(string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        Assert.Contains("PARTIAL_PASS_WITH_PROVENANCE_REBUILD_REQUIRED", text, StringComparison.Ordinal);
        Assert.Contains("ORCHESTRATION_REASONING_CHECKPOINT_SOURCE_PROVENANCE_REBUILD_REQUIRED", text, StringComparison.Ordinal);
        Assert.DoesNotContain("PASS_AFTER_PARENT_REVIEW", text, StringComparison.Ordinal);
        Assert.DoesNotContain("PASS_CANDIDATE_READY_FOR_PARENT_REVIEW", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Two source-disjoint held-out component samples pass", text, StringComparison.Ordinal);
        Assert.DoesNotContain("P11 is accepted as component fresh-load lifecycle evidence only", text, StringComparison.Ordinal);
        Assert.DoesNotContain("two-component lifecycle evidence only", text, StringComparison.OrdinalIgnoreCase);
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
