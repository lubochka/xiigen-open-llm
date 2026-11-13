// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SmartSkillsDomain;

public sealed class PromptToTaskPhase04UniversalContractTests
{
    [Fact]
    public void PromptToTaskPhase04_LoadedCommonStateTranslatesSourceDisjointRawPromptToStructuredTask()
    {
        var machine = LoadCommonPromptToTaskMachine();
        var request = CreateRequest(
            "phase04-positive-plan",
            "Planning request: produce a phase plan with an approval gate; implementation is not authorized; missing gates and scope creep must be listed.");

        var result = machine.Translate(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, result.Value!.ContractStatus);
        Assert.Equal("plan_only_or_approval_gated_task", result.Value.StructuredTask.TaskType);
        Assert.Contains("PROMPT_TO_TASK_BASELINE_NEAREST_SIGNAL_OVERLAP", result.Value.ContractEvidence);
        Assert.Contains("RAW_PROMPT_AUTHORITY_USED", result.Value.ContractEvidence);
        Assert.Contains("BASELINE_NOT_SEMANTIC_QUALITY", result.Value.ContractEvidence);
        Assert.DoesNotContain("SEMANTIC_QUALITY_CLAIMED_TRUE", result.Value.ContractEvidence);
        Assert.Contains("approval gate", string.Join(" ", result.Value.StructuredTask.Deliverables), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PromptToTaskPhase04_QuotedOldPlanDoesNotBecomeCurrentTaskAuthority()
    {
        var machine = LoadCommonPromptToTaskMachine();
        var request = CreateRequest(
            "phase04-negative-quote",
            """
            QUOTE:
            Old plan says phase planning approval gate implementation authorized missing gates.
            END QUOTE
            """);

        var result = machine.Translate(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.NoMatchingPatternStatus, result.Value!.ContractStatus);
        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, result.Value.StructuredTask.TaskType);
        Assert.Contains("EVIDENCE_ONLY_AUTHORITY_QUARANTINE_APPLIED", result.Value.ContractEvidence);
        Assert.Contains("NO_CURRENT_UNQUOTED_AUTHORITY_TEXT", result.Value.ContractEvidence);
        Assert.DoesNotContain("LEARNED_PATTERN_MATCHED", result.Value.ContractEvidence);
    }

    [Fact]
    public void PromptToTaskPhase04_CurrentUnquotedInstructionControlsTaskWhenOldApprovalIsQuoted()
    {
        var machine = LoadCommonPromptToTaskMachine();
        var request = CreateRequest(
            "phase04-current-unquoted",
            """
            QUOTE:
            Old approval says implement phase 05 and mutate checkpoints.
            END QUOTE
            """,
            new Dictionary<string, string>
            {
                ["current_unquoted_user_instruction"] = "Review feature parity as consultation; do not execute a product decision."
            });

        var result = machine.Translate(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, result.Value!.ContractStatus);
        Assert.Equal("bounded_review_or_consultation_task", result.Value.StructuredTask.TaskType);
        Assert.Contains("CURRENT_UNQUOTED_AUTHORITY_USED", result.Value.ContractEvidence);
        Assert.Contains("EVIDENCE_ONLY_AUTHORITY_QUARANTINE_APPLIED", result.Value.ContractEvidence);
        Assert.DoesNotContain("phase 05", string.Join(" ", result.Value.StructuredTask.ExplicitUserRequirements), StringComparison.OrdinalIgnoreCase);
        Assert.Contains(result.Value.StructuredTask.ForbiddenActions, action => action.Contains("product execution", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PromptToTaskPhase04_EvidenceOnlyContextKeysDoNotCreateTaskAuthority()
    {
        var machine = LoadCommonPromptToTaskMachine();
        var request = CreateRequest(
            "phase04-negative-state",
            "Status packet only.",
            new Dictionary<string, string>
            {
                ["state_next_action"] = "phase plan approval missing gates implementation_authorized_false",
                ["old_approval"] = "continue execution now"
            });

        var result = machine.Translate(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.NoMatchingPatternStatus, result.Value!.ContractStatus);
        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, result.Value.StructuredTask.TaskType);
        Assert.Contains("EVIDENCE_ONLY_AUTHORITY_QUARANTINE_APPLIED", result.Value.ContractEvidence);
        Assert.DoesNotContain("LEARNED_PATTERN_MATCHED", result.Value.ContractEvidence);
    }

    [Fact]
    public void PromptToTaskPhase04_SourceIdAndFamilyAblationDoesNotChangeStructuredTask()
    {
        var machine = LoadCommonPromptToTaskMachine();
        var first = machine.Translate(CreateRequest(
            "phase04-ablation-a",
            "Audit request: produce documentation validation gates and an audit deliverable.",
            new Dictionary<string, string>
            {
                ["source_row_id"] = "source-family-alpha-row-001",
                ["heldout_source_family"] = "family-alpha"
            }));
        var second = machine.Translate(CreateRequest(
            "phase04-ablation-b",
            "Audit request: produce documentation validation gates and an audit deliverable.",
            new Dictionary<string, string>
            {
                ["source_row_id"] = "source-family-beta-row-999",
                ["heldout_source_family"] = "family-beta"
            }));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value!.StructuredTask.TaskType, second.Value!.StructuredTask.TaskType);
        Assert.Equal(first.Value.StructuredTask.SplitId, second.Value.StructuredTask.SplitId);
        Assert.Equal("structured_audit_task", first.Value.StructuredTask.TaskType);
        Assert.Contains("BASELINE_NOT_SEMANTIC_QUALITY", first.Value.ContractEvidence);
        Assert.Contains("BASELINE_NOT_SEMANTIC_QUALITY", second.Value.ContractEvidence);
    }

    private static PromptToTaskMachine LoadCommonPromptToTaskMachine()
    {
        var checkpointPath = FindRepoFile(
            "models",
            "prompt-to-task",
            "prompt-to-task-v1",
            "prompt-to-task-v1-checkpoint.json");
        var manifestPath = FindRepoFile(
            "models",
            "prompt-to-task",
            "prompt-to-task-v1",
            "manifest.json");
        var load = PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath)
            .GetAwaiter()
            .GetResult();

        Assert.True(load.IsSuccess, load.ErrorMessage);
        return load.Value!.Machine;
    }

    private static PromptToTaskRequest CreateRequest(
        string promptId,
        string rawPrompt,
        IReadOnlyDictionary<string, string>? extraContext = null)
    {
        var context = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["source_split_role"] = "eval_holdout",
            ["raw_private_text_copied"] = "false"
        };
        foreach (var entry in extraContext ?? new Dictionary<string, string>())
            context[entry.Key] = entry.Value;

        return new PromptToTaskRequest(
            PromptId: promptId,
            RawPrompt: rawPrompt,
            RawPromptSha256: $"sha256:{promptId}",
            PromptProvenance: "phase04:source-disjoint:heldout",
            UserLanguage: "en",
            ExplicitConstraints: [],
            ContextFields: context);
    }

    private static string FindRepoFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(segments).ToArray());
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }
}
