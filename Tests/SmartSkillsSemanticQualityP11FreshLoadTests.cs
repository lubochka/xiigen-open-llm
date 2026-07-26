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
        var resultRowsPath = Path.Combine(P11Dir, "smart-skills-p11-per-component-results.jsonl");
        var selfCheckPath = Path.Combine(P11Dir, "p11-self-check.json");
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
        var focusedTestResult = reportDoc.RootElement
            .GetProperty("focused_test")
            .GetProperty("result")
            .GetString();
        Assert.NotEqual("pending_final_run", focusedTestResult);
        Assert.Contains("passed", focusedTestResult, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Failed=0", focusedTestResult, StringComparison.Ordinal);
        Assert.Contains("Passed=1", focusedTestResult, StringComparison.Ordinal);
        Assert.True(selfCheckDoc.RootElement.GetProperty("project_load_path_exercised").GetBoolean());
        Assert.True(selfCheckDoc.RootElement.GetProperty("import_async_paths_exist_and_exercised").GetBoolean());
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

        await VerifyPromptToTaskFreshLoadAsync(promptToTaskComponent, resultRows.Single(row =>
            row.RootElement.GetProperty("component_id").GetString() == "PromptToTask"));
        await VerifyOrchestrationFreshLoadAsync(orchestrationComponent, resultRows.Single(row =>
            row.RootElement.GetProperty("component_id").GetString() == "OrchestrationReasoning"));

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

    private static async Task VerifyOrchestrationFreshLoadAsync(JsonElement component, JsonDocument resultRow)
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
        Assert.True(load.IsSuccess, load.ErrorMessage);
        var loadedResponse = load.Value!.Machine.Plan(request);
        Assert.True(loadedResponse.IsSuccess, loadedResponse.ErrorMessage);
        Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, loadedResponse.Value!.ContractStatus);
        Assert.Equal("or-p9a-pattern-branch-review-targeted-repair", loadedResponse.Value.ScenarioPlan.SelectedLearnedPatterns.Single());
        Assert.Equal("scenario:branch-review-targeted-repair", loadedResponse.Value.ScenarioPlan.ScenarioId);

        var importedMachine = new OrchestrationReasoningMachine();
        var export = new OrchestrationReasoningLifecycleExport(
            OrchestrationReasoningMachine.MachineId,
            OrchestrationReasoningMachine.DomainTag,
            OrchestrationReasoningMachine.Version,
            checkpointPath,
            manifestPath,
            expectedHash,
            DateTimeOffset.UtcNow);
        var import = await importedMachine.ImportAsync(export);
        Assert.True(import.IsSuccess, import.ErrorMessage);
        var importedResponse = importedMachine.Plan(request);
        Assert.True(importedResponse.IsSuccess, importedResponse.ErrorMessage);
        Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, importedResponse.Value!.ContractStatus);
        Assert.Equal("component_orchestration_plan_produced_after_fresh_load", resultRow.RootElement.GetProperty("observed_component_output").GetString());
        Assert.True(resultRow.RootElement.GetProperty("expected_component_output_observed").GetBoolean());
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
