// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

public sealed record SmartSkillsSemanticUnitDefinition(
    string DomainId,
    string MachineName,
    string TrainingAdapterName,
    string DpoRowShapeName,
    string CheckpointSectionName,
    string HeldOutFixtureId,
    bool Trainable,
    bool DeterministicFinalAuthority,
    bool RuntimeSemanticQualityClaimed);

public static class SmartSkillsSemanticUnitCatalog
{
    public static IReadOnlyList<string> ExpectedDomainIds { get; } =
    [
        "prompt_understanding",
        "task_understanding",
        "task_type",
        "routing",
        "skill_candidates",
        "skill_selection",
        "prompt_context",
        "result_gap",
        "output_validation",
        "task_completion",
        "intermediate_checks"
    ];

    public static IReadOnlyList<SmartSkillsSemanticUnitDefinition> All { get; } =
    [
        Unit(
            "prompt_understanding",
            "PromptUnderstandingMachine",
            "IPromptUnderstandingTraining",
            "PromptUnderstandingDpoRow",
            "semantic_units.prompt_understanding",
            "heldout.prompt_understanding.v1"),
        Unit(
            "task_understanding",
            "TaskUnderstandingMachine",
            "ITaskUnderstandingTraining",
            "TaskUnderstandingDpoRow",
            "semantic_units.task_understanding",
            "heldout.task_understanding.v1"),
        Unit(
            "task_type",
            "TaskTypeMachine",
            "ITaskTypeTraining",
            "TaskTypeDpoRow",
            "semantic_units.task_type",
            "heldout.task_type.v1"),
        Unit(
            "routing",
            "RoutingMachine",
            "IRoutingTraining",
            "RoutingDpoRow",
            "semantic_units.routing",
            "heldout.routing.v1"),
        Unit(
            "skill_candidates",
            "SkillCandidateMachine",
            "ISkillCandidateTraining",
            "SkillCandidateDpoRow",
            "semantic_units.skill_candidates",
            "heldout.skill_candidates.v1"),
        Unit(
            "skill_selection",
            "SkillSelectionMachine",
            "ISkillSelectionTraining;IMachineTraining<SkillSelectionDomain>",
            "SkillSelectionMachinePair",
            "semantic_units.skill_selection",
            "heldout.skill_selection.v1"),
        Unit(
            "prompt_context",
            "PromptContextMachine",
            "IPromptContextTraining",
            "PromptContextDpoRow",
            "semantic_units.prompt_context",
            "heldout.prompt_context.v1"),
        Unit(
            "result_gap",
            "ResultGapMachine",
            "IResultGapTraining",
            "ResultGapDpoRow",
            "semantic_units.result_gap",
            "heldout.result_gap.v1"),
        Unit(
            "output_validation",
            "OutputValidationMachine",
            "IOutputValidationTraining",
            "OutputValidationDpoRow",
            "semantic_units.output_validation",
            "heldout.output_validation.v1"),
        Unit(
            "task_completion",
            "TaskCompletionMachine",
            "ITaskCompletionTraining",
            "TaskCompletionDpoRow",
            "semantic_units.task_completion",
            "heldout.task_completion.v1"),
        Unit(
            "intermediate_checks",
            "IntermediateCheckMachine",
            "IIntermediateCheckTraining",
            "IntermediateCheckDpoRow",
            "semantic_units.intermediate_checks",
            "heldout.intermediate_checks.v1")
    ];

    private static SmartSkillsSemanticUnitDefinition Unit(
        string domainId,
        string machineName,
        string trainingAdapterName,
        string dpoRowShapeName,
        string checkpointSectionName,
        string heldOutFixtureId)
        => new(
            DomainId: domainId,
            MachineName: machineName,
            TrainingAdapterName: trainingAdapterName,
            DpoRowShapeName: dpoRowShapeName,
            CheckpointSectionName: checkpointSectionName,
            HeldOutFixtureId: heldOutFixtureId,
            Trainable: true,
            DeterministicFinalAuthority: false,
            RuntimeSemanticQualityClaimed: false);
}
