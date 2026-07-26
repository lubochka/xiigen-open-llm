// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using SmartSkillsDomain;

public sealed class PromptToTaskToOrchestrationReasoningHandoffTests
{
    [Fact]
    public void Handoff_PreservesStructuredTaskIdentityAndProvenance()
    {
        var response = CreatePromptToTaskResponse();
        var request = CreateOrchestrationRequest(response.StructuredTask);

        Assert.Equal(response.StructuredTask.PromptId, request.Task.PromptId);
        Assert.Equal(response.StructuredTask.RawPromptSha256, request.Task.RawPromptSha256);
        Assert.Equal(response.StructuredTask.SplitId, request.Task.SplitId);
        Assert.Equal("claude-user-prompts", request.Task.SourceProvenance["source_root_id"]);
        Assert.Equal("prompt-handoff-001", request.Task.SourceProvenance["prompt_id"]);
    }

    [Fact]
    public void Handoff_PreservesRequirementsConstraintsAndEvidenceNeeds()
    {
        var response = CreatePromptToTaskResponse();
        var request = CreateOrchestrationRequest(response.StructuredTask);

        Assert.Contains("exactly one bounded work order", request.Task.ExplicitUserRequirements);
        Assert.Contains("parent REVIEW_INTERNAL review required", request.Task.ExplicitUserRequirements);
        Assert.Contains("no broad tests", request.Task.Constraints);
        Assert.Contains("no machine implementation", request.Task.Constraints);
        Assert.Contains("training code", request.Task.ForbiddenActions);
        Assert.Equal(["claude", "cursor", "codex"], request.Task.RequiredHosts);
        Assert.Contains("filtered handoff contract test", request.Task.EvidenceNeeds);
    }

    [Fact]
    public void Handoff_DoesNotPassRawPromptTextIntoOrchestrationReasoningRequest()
    {
        var response = CreatePromptToTaskResponse();
        var request = CreateOrchestrationRequest(response.StructuredTask);
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var requestPropertyNames = typeof(OrchestrationReasoningRequest)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("RawPrompt", requestPropertyNames);
        Assert.DoesNotContain("PromptText", requestPropertyNames);
        Assert.DoesNotContain(response.Request.RawPrompt, json);
        Assert.Contains(response.StructuredTask.RawPromptSha256, json);
    }

    [Fact]
    public void Handoff_UsesContractsOnlyWithoutClassifierSelectorMachineOrTrainingTypes()
    {
        var involvedTypeNames = new[]
        {
            nameof(PromptToTaskRequest),
            nameof(PromptToTaskResponse),
            nameof(StructuredTaskContract),
            nameof(OrchestrationReasoningRequest),
            nameof(OrchestrationScenarioPlan),
            nameof(OrchestrationReasoningResponse)
        };

        Assert.DoesNotContain(involvedTypeNames, name => name.Contains("Classifier", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(involvedTypeNames, name => name.Contains("Selector", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(involvedTypeNames, name => name.Contains("Machine", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(involvedTypeNames, name => name.Contains("Trainer", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(involvedTypeNames, name => name.Contains("Replay", StringComparison.OrdinalIgnoreCase));
    }

    private static PromptToTaskResponse CreatePromptToTaskResponse()
    {
        var request = new PromptToTaskRequest(
            PromptId: "prompt-handoff-001",
            RawPrompt: "Run exactly one bounded handoff work order and do not run broad tests.",
            RawPromptSha256: "sha256:prompt-handoff-001",
            PromptProvenance: "conversation:visible-current",
            UserLanguage: "en",
            ExplicitConstraints: ["no broad tests", "no machine implementation"],
            ContextFields: new Dictionary<string, string>
            {
                ["source_root_id"] = "claude-user-prompts",
                ["host"] = "codex"
            });
        var task = new StructuredTaskContract(
            PromptId: request.PromptId,
            RawPromptSha256: request.RawPromptSha256,
            UserLanguage: request.UserLanguage,
            TaskType: "handoff_contract_test",
            ExplicitUserRequirements:
            [
                "exactly one bounded work order",
                "parent REVIEW_INTERNAL review required"
            ],
            Constraints: request.ExplicitConstraints,
            Deliverables: ["handoff fixture test"],
            ForbiddenActions: ["training code", "extension repos", "broad tests"],
            RequiredHosts: ["claude", "cursor", "codex"],
            EvidenceNeeds: ["filtered handoff contract test"],
            SourceProvenance: new Dictionary<string, string>
            {
                ["source_root_id"] = request.ContextFields["source_root_id"],
                ["prompt_id"] = request.PromptId,
                ["prompt_provenance"] = request.PromptProvenance
            },
            SplitId: "split-handoff-001");

        return new PromptToTaskResponse(
            ResponseId: "ptt-response-handoff-001",
            Request: request,
            StructuredTask: task,
            ContractEvidence: ["prompt-to-task contract fixture"],
            ContractStatus: "contract_only_no_training");
    }

    private static OrchestrationReasoningRequest CreateOrchestrationRequest(StructuredTaskContract task)
        => new(
            RequestId: "orch-request-handoff-001",
            Task: task,
            GuideDerivedStateRefs: ["guide-state:common:v1"],
            CorpusBundleIds: ["corpus:sonnet-local-analyzes"],
            AdaptiveContextRefs: new Dictionary<string, string>
            {
                ["project_local_state"] = "adaptive:project-redacted"
            },
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["external_ai"] = "approval-required",
                ["deterministic_shortcut"] = "forbidden"
            });
}
