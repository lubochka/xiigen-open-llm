// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using SmartSkillsDomain;

public sealed class PromptToTaskMachineStateLoadingTests
{
    [Fact]
    public void PromptToTaskMachineStateLoading_UnloadedStateReturnsNotTrainedResult()
    {
        var machine = new PromptToTaskMachine();
        var result = machine.Translate(CreateRequest("prompt-state-001", "Create a bounded work order."));

        Assert.True(result.IsSuccess);
        Assert.Equal(PromptToTaskMachine.NotTrainedStatus, result.Value!.ContractStatus);
        Assert.Equal("none_loaded", result.Value.StructuredTask.SourceProvenance["machine_state"]);
        Assert.Contains("NO_TRAINED_STATE_LOADED", result.Value.ContractEvidence);
    }

    [Fact]
    public void PromptToTaskMachineStateLoading_LoadedDescriptorIsDetected()
    {
        var descriptor = CreateDescriptor();
        var machine = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(descriptor));

        var result = machine.Translate(CreateRequest("prompt-state-002", "Implement the prompt machine."));

        Assert.True(result.IsSuccess);
        Assert.Equal(PromptToTaskMachine.LoadedStateNoPatternsStatus, result.Value!.ContractStatus);
        Assert.Equal("descriptor_loaded", result.Value.StructuredTask.SourceProvenance["machine_state"]);
        Assert.Contains("LEARNED_STATE_DESCRIPTOR_LOADED", result.Value.ContractEvidence);
        Assert.Contains("NO_LEARNED_PATTERNS_AVAILABLE", result.Value.ContractEvidence);
    }

    [Fact]
    public void PromptToTaskMachineStateLoading_PreservesManifestIdentity()
    {
        var descriptor = CreateDescriptor();
        var machine = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(descriptor));

        var response = machine.Translate(CreateRequest("prompt-state-003", "Preserve state metadata.")).Value!;
        var provenance = response.StructuredTask.SourceProvenance;

        Assert.Equal(descriptor.StateId, provenance["state_id"]);
        Assert.Equal(descriptor.ManifestId, provenance["manifest_id"]);
        Assert.Equal(descriptor.ManifestSha256, provenance["manifest_sha256"]);
        Assert.Equal(descriptor.MachineType, provenance["machine_type"]);
        Assert.Equal(descriptor.SchemaVersion, provenance["schema_version"]);
        Assert.Equal(descriptor.SourceBaseCommit, provenance["source_base_commit"]);
        Assert.Equal(descriptor.StateSha256, provenance["state_sha256"]);
    }

    [Fact]
    public void PromptToTaskMachineStateLoading_LoadedStateStillDoesNotGeneratePromptSemantics()
    {
        var machine = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(CreateDescriptor()));
        var first = machine.Translate(CreateRequest("prompt-state-004", "Run Claude extension debug."));
        var second = machine.Translate(CreateRequest("prompt-state-005", "Train the prompt machine later."));

        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, first.Value!.StructuredTask.TaskType);
        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, second.Value!.StructuredTask.TaskType);
        Assert.Empty(first.Value.StructuredTask.ExplicitUserRequirements);
        Assert.Empty(second.Value.StructuredTask.Deliverables);
        Assert.Contains("NO_SEMANTIC_CLASSIFICATION_PERFORMED", first.Value.ContractEvidence);
        Assert.Contains("NO_SEMANTIC_CLASSIFICATION_PERFORMED", second.Value.ContractEvidence);
    }

    [Fact]
    public void PromptToTaskMachineStateLoading_LoadedLearnedPatternProducesStructuredTask()
    {
        var pattern = CreatePattern();
        var machine = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(CreateDescriptor([pattern])));

        var result = machine.Translate(CreateLearnedRequest("prompt-state-006", "One private wording."));
        var task = result.Value!.StructuredTask;

        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, result.Value.ContractStatus);
        Assert.Equal("bounded_implementation_slice", task.TaskType);
        Assert.Contains("implement scoped production behavior", task.ExplicitUserRequirements);
        Assert.Contains("no host repos", task.Constraints);
        Assert.Contains("focused test evidence", task.Deliverables);
        Assert.Contains("training runs", task.ForbiddenActions);
        Assert.Contains("llm_mvp_core", task.RequiredHosts);
        Assert.Equal("ptt-03-learned-split", task.SplitId);
        Assert.Equal(pattern.PatternId, task.SourceProvenance["learned_pattern_id"]);
        Assert.Equal("2", task.SourceProvenance["matched_signal_count"]);
        Assert.Contains("PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED", result.Value.ContractEvidence);
        Assert.Contains("BASELINE_NOT_SEMANTIC_QUALITY", result.Value.ContractEvidence);
        Assert.Contains("SEMANTIC_QUALITY_CLAIMED_FALSE", result.Value.ContractEvidence);
        Assert.Contains("LEARNED_PATTERN_MATCHED", result.Value.ContractEvidence);
    }

    [Fact]
    public void PromptToTaskMachineStateLoading_TiedTopTwoMarginReturnsNotReady()
    {
        var firstPattern = CreatePattern();
        var secondPattern = CreatePattern() with
        {
            PatternId = "ptt-pattern-bounded-implementation-tie",
            TaskType = "competing_bounded_implementation_slice"
        };
        var machine = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(CreateDescriptor([firstPattern, secondPattern])));

        var result = machine.Translate(CreateLearnedRequest("prompt-state-006-margin", "Private wording with same signals."));

        Assert.Equal(PromptToTaskMachine.NoMatchingPatternStatus, result.Value!.ContractStatus);
        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, result.Value.StructuredTask.TaskType);
        Assert.Contains("PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED", result.Value.ContractEvidence);
        Assert.Contains("PROMPT_TO_TASK_TASK_PATTERN_UNIT_NOT_READY", result.Value.ContractEvidence);
        Assert.Contains("BASELINE_NOT_SEMANTIC_QUALITY", result.Value.ContractEvidence);
        Assert.DoesNotContain("LEARNED_PATTERN_MATCHED", result.Value.ContractEvidence);
    }

    [Fact]
    public void PromptToTaskMachineStateLoading_LearnedStateWithoutSignalMatchReturnsNoSemanticTask()
    {
        var machine = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(CreateDescriptor([CreatePattern()])));

        var result = machine.Translate(CreateRequest("prompt-state-007", "Different private wording."));

        Assert.Equal(PromptToTaskMachine.NoMatchingPatternStatus, result.Value!.ContractStatus);
        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, result.Value.StructuredTask.TaskType);
        Assert.Empty(result.Value.StructuredTask.Deliverables);
        Assert.Contains("NO_MATCHING_LEARNED_PATTERN", result.Value.ContractEvidence);
    }

    [Fact]
    public void PromptToTaskMachineStateLoading_DifferentRawPromptsWithSameSignalsUseSameLearnedTask()
    {
        var machine = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(CreateDescriptor([CreatePattern()])));
        var first = machine.Translate(CreateLearnedRequest("prompt-state-008", "First private prompt text."));
        var second = machine.Translate(CreateLearnedRequest("prompt-state-009", "Completely different private prompt text."));

        Assert.NotEqual(first.Value!.Request.RawPrompt, second.Value!.Request.RawPrompt);
        Assert.NotEqual(first.Value.StructuredTask.RawPromptSha256, second.Value.StructuredTask.RawPromptSha256);
        Assert.Equal(first.Value.StructuredTask.TaskType, second.Value.StructuredTask.TaskType);
        Assert.Equal(first.Value.StructuredTask.Deliverables, second.Value.StructuredTask.Deliverables);
        Assert.Equal(first.Value.StructuredTask.SourceProvenance["learned_pattern_id"], second.Value.StructuredTask.SourceProvenance["learned_pattern_id"]);
        Assert.Contains("PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED", first.Value.ContractEvidence);
        Assert.Contains("PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED", second.Value.ContractEvidence);
    }

    [Fact]
    public void PromptToTaskMachineStateLoading_AddsNoClassifierTrainerLookupOrDeterministicFinalAuthority()
    {
        var inspectedTypes = new[]
        {
            typeof(PromptToTaskMachine),
            typeof(PromptToTaskLearnedStateDescriptor),
            typeof(IPromptToTaskLearnedStateLoader)
        };
        var memberNames = inspectedTypes
            .SelectMany(type => type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Select(member => member.Name)
            .ToArray();
        var fieldTypes = typeof(PromptToTaskMachine)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Select(field => field.FieldType)
            .ToArray();

        Assert.DoesNotContain(typeof(Regex), fieldTypes);
        Assert.DoesNotContain(fieldTypes, type => type.Name.Contains("Dictionary", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Classifier", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Trainer", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Lookup", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Bm25", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("ExactPrompt", StringComparison.OrdinalIgnoreCase));
    }

    private static PromptToTaskLearnedStateDescriptor CreateDescriptor(
        IReadOnlyList<PromptToTaskLearnedPattern>? learnedPatterns = null)
        => new(
            StateId: "prompt-to-task-state:v1:test-descriptor",
            ManifestId: "models/prompt-to-task/prompt-to-task-v1/manifest",
            ManifestSha256: "sha256:manifest-test",
            MachineType: "PromptToTaskMachine",
            SchemaVersion: "prompt-to-task-state-v1",
            SourceBaseCommit: "commit:test",
            StateSha256: "sha256:state-test",
            LearnedPatterns: learnedPatterns);

    private static PromptToTaskLearnedPattern CreatePattern()
        => new(
            PatternId: "ptt-pattern-bounded-implementation",
            TaskType: "bounded_implementation_slice",
            LearnedSignals: ["bounded_subphase", "work_order:PTT-03"],
            ExplicitUserRequirements: ["implement scoped production behavior"],
            Constraints: ["no host repos", "no external AI"],
            Deliverables: ["focused test evidence", "implementation integrity artifact"],
            ForbiddenActions: ["training runs", "commit/push"],
            RequiredHosts: ["llm_mvp_core"],
            EvidenceNeeds: ["ablation evidence", "no hidden deterministic shortcut sweep"],
            SplitId: "ptt-03-learned-split",
            Confidence: 0.91);

    private static PromptToTaskRequest CreateRequest(string promptId, string rawPrompt)
        => new(
            PromptId: promptId,
            RawPrompt: rawPrompt,
            RawPromptSha256: $"sha256:{promptId}",
            PromptProvenance: "conversation:wo-ptt-impl-02",
            UserLanguage: "en",
            ExplicitConstraints: ["<=15 minutes", "no broad tests", "no host repos"],
            ContextFields: new Dictionary<string, string>
            {
                ["source_root_id"] = "claude-user-prompts",
                ["work_order"] = "WO-PTT-IMPL-02"
            });

    private static PromptToTaskRequest CreateLearnedRequest(string promptId, string rawPrompt)
        => new(
            PromptId: promptId,
            RawPrompt: rawPrompt,
            RawPromptSha256: $"sha256:{promptId}",
            PromptProvenance: "conversation:ptt-03",
            UserLanguage: "en",
            ExplicitConstraints: ["bounded_subphase", "no host repos"],
            ContextFields: new Dictionary<string, string>
            {
                ["source_root_id"] = "claude-user-prompts",
                ["work_order"] = "PTT-03"
            });

    private sealed class StaticPromptToTaskLearnedStateLoader(PromptToTaskLearnedStateDescriptor descriptor)
        : IPromptToTaskLearnedStateLoader
    {
        public OperationResult<PromptToTaskLearnedStateDescriptor> Load()
            => OperationResult<PromptToTaskLearnedStateDescriptor>.Ok(descriptor);
    }
}
