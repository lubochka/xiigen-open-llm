// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using SmartSkillsDomain;

public sealed class PromptToTaskMachineSkeletonTests
{
    [Fact]
    public void PromptToTaskMachineSkeleton_ConstructAndCallReturnsNotTrainedResult()
    {
        var machine = new PromptToTaskMachine();
        var request = CreateRequest("prompt-skeleton-001", "Create exactly one bounded work order.");

        var result = machine.Translate(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(PromptToTaskMachine.NotTrainedStatus, result.Value!.ContractStatus);
        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, result.Value.StructuredTask.TaskType);
        Assert.Contains("NO_TRAINED_STATE_LOADED", result.Value.ContractEvidence);
        Assert.DoesNotContain(result.Value.StructuredTask.EvidenceNeeds, need => need.Contains("semantic translation", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Value.StructuredTask.EvidenceNeeds, need => need.Contains("prompt-to-task inference can be claimed", StringComparison.Ordinal));
    }

    [Fact]
    public void PromptToTaskMachineSkeleton_PreservesContractIdentityFields()
    {
        var machine = new PromptToTaskMachine();
        var request = CreateRequest("prompt-skeleton-002", "Do not run broad tests.");

        var result = machine.Translate(request);
        var response = result.Value!;

        Assert.Equal(request, response.Request);
        Assert.Equal(request.PromptId, response.StructuredTask.PromptId);
        Assert.Equal(request.RawPromptSha256, response.StructuredTask.RawPromptSha256);
        Assert.Equal(request.UserLanguage, response.StructuredTask.UserLanguage);
        Assert.Equal(request.ExplicitConstraints, response.StructuredTask.Constraints);
        Assert.Equal(request.PromptProvenance, response.StructuredTask.SourceProvenance["prompt_provenance"]);
    }

    [Fact]
    public void PromptToTaskMachineSkeleton_DoesNotClassifyPromptSemantically()
    {
        var machine = new PromptToTaskMachine();
        var first = machine.Translate(CreateRequest("prompt-skeleton-003", "Implement a Claude extension."));
        var second = machine.Translate(CreateRequest("prompt-skeleton-004", "Plan a training asset lifecycle."));

        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, first.Value!.StructuredTask.TaskType);
        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, second.Value!.StructuredTask.TaskType);
        Assert.Empty(first.Value.StructuredTask.ExplicitUserRequirements);
        Assert.Empty(second.Value.StructuredTask.Deliverables);
        Assert.Contains("NO_SEMANTIC_CLASSIFICATION_PERFORMED", first.Value.ContractEvidence);
        Assert.Contains("NO_SEMANTIC_CLASSIFICATION_PERFORMED", second.Value.ContractEvidence);
    }

    [Fact]
    public void PromptToTaskMachineSkeleton_UsesOperationResultEnvelope()
    {
        var method = typeof(PromptToTaskMachine).GetMethod(nameof(PromptToTaskMachine.Translate));

        Assert.NotNull(method);
        Assert.Equal(typeof(OperationResult<PromptToTaskResponse>), method!.ReturnType);
    }

    [Fact]
    public void PromptToTaskMachineSkeleton_HasNoDeterministicClassifierTrainerOrLookupState()
    {
        var machineType = typeof(PromptToTaskMachine);
        var fieldTypes = machineType
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Select(field => field.FieldType)
            .ToArray();
        var memberNames = machineType
            .GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(member => member.Name)
            .ToArray();

        Assert.DoesNotContain(typeof(Regex), fieldTypes);
        Assert.DoesNotContain(fieldTypes, type => type.Name.Contains("Dictionary", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Classifier", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Trainer", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Lookup", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Bm25", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("ExactPrompt", StringComparison.OrdinalIgnoreCase));
    }

    private static PromptToTaskRequest CreateRequest(string promptId, string rawPrompt)
        => new(
            PromptId: promptId,
            RawPrompt: rawPrompt,
            RawPromptSha256: $"sha256:{promptId}",
            PromptProvenance: "conversation:wo-ptt-impl-01",
            UserLanguage: "en",
            ExplicitConstraints: ["<=15 minutes", "no broad tests", "no host repos"],
            ContextFields: new Dictionary<string, string>
            {
                ["source_root_id"] = "claude-user-prompts",
                ["work_order"] = "WO-PTT-IMPL-01"
            });
}
