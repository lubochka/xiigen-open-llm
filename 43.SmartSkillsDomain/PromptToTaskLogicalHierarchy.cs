// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class PromptToTaskLearningMachineAdapter : ILearningMachine<PromptToTaskDomain>
{
    private readonly PromptToTaskMachine _machine;
    private readonly string? _checkpointPath;
    private readonly string? _manifestPath;
    private readonly string _sourceManifestRef;
    private readonly IReadOnlyList<string> _acceptedRefIds;

    public PromptToTaskLearningMachineAdapter(
        PromptToTaskMachine? machine = null,
        string? checkpointPath = null,
        string? manifestPath = null,
        string sourceManifestRef = "artifacts/prompt-to-task-corpus-summary.json",
        IReadOnlyList<string>? acceptedRefIds = null)
    {
        _machine = machine ?? new PromptToTaskMachine();
        _checkpointPath = checkpointPath;
        _manifestPath = manifestPath;
        _sourceManifestRef = sourceManifestRef;
        _acceptedRefIds = acceptedRefIds ?? [];
    }

    public Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<PromptToTaskDomain> batch,
        CancellationToken ct = default)
        => Task.FromResult(OperationResult<LearningBatchResult>.Fail(
            "PromptToTask logical hierarchy adapter does not train semantic behavior in CM-26A1.",
            "PROMPT_TO_TASK_TRAINING_NOT_IMPLEMENTED_IN_LOGICAL_ADAPTER"));

    public Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<PromptToTaskDomain> evalSet,
        CancellationToken ct = default)
        => Task.FromResult(OperationResult<LearningEvaluationResult>.Ok(new LearningEvaluationResult(
            EvaluationId: evalSet.EvaluationId,
            RowsEvaluated: evalSet.EvaluationItems.Count,
            Accuracy: 0.0,
            RewardMargin: 0.0,
            PassesGate: false,
            GateNote: "PromptToTask logical hierarchy adapter exposes source hierarchy only; semantic quality remains unclaimed.")));

    public async Task<OperationResult<LearningMachineExport>> ExportAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_checkpointPath) || string.IsNullOrWhiteSpace(_manifestPath))
        {
            return OperationResult<LearningMachineExport>.Fail(
                "PromptToTask logical adapter export requires checkpoint and manifest paths.",
                "PROMPT_TO_TASK_LOGICAL_EXPORT_PATHS_REQUIRED");
        }

        var export = await _machine.SaveCheckpointAsync(
            _checkpointPath,
            _manifestPath,
            _sourceManifestRef,
            _acceptedRefIds,
            ct);
        if (!export.IsSuccess)
            return OperationResult<LearningMachineExport>.Fail(export.ErrorMessage, export.ErrorCode);

        return OperationResult<LearningMachineExport>.Ok(new LearningMachineExport(
            MachineId: export.Value!.MachineId,
            DomainTag: export.Value.DomainTag,
            Version: export.Value.Version,
            CheckpointRef: export.Value.CheckpointRef,
            ManifestRef: export.Value.ManifestRef,
            AccuracyAtExport: 0.0,
            ExportedAtUtc: export.Value.ExportedAtUtc));
    }

    public async Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<bool>.Fail("Import cancelled.", "CANCELLED");
        if (export is null)
            return OperationResult<bool>.Fail("Export is required.", "PROMPT_TO_TASK_EXPORT_REQUIRED");
        if (!string.Equals(export.MachineId, PromptToTaskMachine.MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Machine id mismatch.", "PROMPT_TO_TASK_MACHINE_ID_MISMATCH");
        if (!string.Equals(export.DomainTag, PromptToTaskMachine.DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Domain tag mismatch.", "PROMPT_TO_TASK_DOMAIN_TAG_MISMATCH");

        var loaded = await PromptToTaskMachine.LoadCheckpointAsync(export.CheckpointRef, export.ManifestRef, ct);
        if (!loaded.IsSuccess)
            return OperationResult<bool>.Fail(loaded.ErrorMessage, loaded.ErrorCode);

        return _machine.ImportLearnedState(loaded.Value!.Checkpoint);
    }

    public Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(CancellationToken ct = default)
        => Task.FromResult(OperationResult<ILogicalOrchestrator>.Ok(
            new PromptToTaskLogicalOrchestrator(new PromptToTaskLogicalFlow(_machine))));
}

public sealed class PromptToTaskLogicalOrchestrator(PromptToTaskLogicalFlow flow) : ILogicalOrchestrator
{
    public const string RequiredMetadataKey = "prompt_to_task_request";

    public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Metadata.TryGetValue(RequiredMetadataKey, out var value) || value is not PromptToTaskRequest promptRequest)
        {
            return OperationResult<LogicalOrchestratorResult>.Fail(
                $"PromptToTask logical orchestrator requires metadata '{RequiredMetadataKey}' containing a PromptToTaskRequest.",
                "PROMPT_TO_TASK_REQUEST_METADATA_REQUIRED");
        }

        var flowRequest = new LogicalFlowRequest<PromptToTaskRequest>(
            FlowId: PromptToTaskLogicalFlow.FlowId,
            ScenarioId: request.ScenarioId,
            RunId: request.RunId,
            Input: promptRequest,
            Flags: request.Flags ?? new LogicalExecutionFlags(),
            Metadata: request.Metadata);
        var flowResult = await flow.ExecuteAsync(flowRequest, cancellationToken);
        if (!flowResult.IsSuccess)
            return flowResult.MapError<LogicalOrchestratorResult>();

        var result = new LogicalOrchestratorResult(
            ScenarioId: request.ScenarioId,
            SelectedFlowId: flowResult.Value!.FlowId,
            OverallVerdict: flowResult.Value.OverallVerdict,
            FinalHumanResult: flowResult.Value.FinalHumanResult,
            AppliedFlags: request.Flags ?? new LogicalExecutionFlags(),
            FlowResult: flowResult.Value)
        {
            Warnings = flowResult.Value.Warnings
        };

        return OperationResult<LogicalOrchestratorResult>.Ok(result);
    }
}

public sealed class PromptToTaskLogicalFlow(PromptToTaskMachine machine) : ILogicalFlow<PromptToTaskRequest>
{
    public const string FlowId = "prompt-to-task-logical-hierarchy";

    private readonly IReadOnlyList<ILogicalUnit<PromptToTaskRequest, PromptToTaskResponse>> _units =
    [
        new PromptToTaskIntentUnit(machine),
        new PromptToTaskConstraintUnit(machine),
        new PromptToTaskDeliverableUnit(machine),
        new PromptToTaskAuthorityBoundaryUnit(machine)
    ];

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<PromptToTaskRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Flow cancelled.", "CANCELLED");

        var evidence = new List<string>
        {
            "PROMPT_TO_TASK_LOGICAL_FLOW_SOURCE_BACKED",
            "PROMPT_TO_TASK_TRANSLATE_DELEGATED_TO_MACHINE",
            "NO_ADDITIONAL_DETERMINISTIC_SEMANTIC_SHORTCUT",
            "SEMANTIC_QUALITY_CLAIMED_FALSE"
        };
        var warnings = new List<string>
        {
            "CM26A1_LOGICAL_HIERARCHY_ADAPTER_ONLY",
            "NO_TRAINING_OR_SEMANTIC_QUALITY_GATE_RUN"
        };

        PromptToTaskResponse? finalResponse = null;
        foreach (var unit in _units)
        {
            var unitResult = await unit.ExecuteAsync(new LogicalUnitRequest<PromptToTaskRequest>(
                RunId: request.RunId,
                UnitId: unit switch
                {
                    PromptToTaskIntentUnit => PromptToTaskIntentUnit.UnitId,
                    PromptToTaskConstraintUnit => PromptToTaskConstraintUnit.UnitId,
                    PromptToTaskDeliverableUnit => PromptToTaskDeliverableUnit.UnitId,
                    PromptToTaskAuthorityBoundaryUnit => PromptToTaskAuthorityBoundaryUnit.UnitId,
                    _ => "prompt_to_task_unknown"
                },
                UnitRole: unit switch
                {
                    PromptToTaskIntentUnit => PromptToTaskIntentUnit.Role,
                    PromptToTaskConstraintUnit => PromptToTaskConstraintUnit.Role,
                    PromptToTaskDeliverableUnit => PromptToTaskDeliverableUnit.Role,
                    PromptToTaskAuthorityBoundaryUnit => PromptToTaskAuthorityBoundaryUnit.Role,
                    _ => "unknown"
                },
                DomainTag: PromptToTaskMachine.DomainTag,
                Input: request.Input,
                Flags: request.Flags,
                Metadata: request.Metadata), cancellationToken);
            if (!unitResult.IsSuccess)
                return unitResult.MapError<LogicalFlowResult>();

            finalResponse = unitResult.Value!.Output;
            evidence.AddRange(unitResult.Value.EvidenceIds);
            warnings.AddRange(unitResult.Value.Warnings);
        }

        var summary = new LogicalFlowSummary(
            SummaryId: $"{request.RunId}-prompt-to-task-logical-summary",
            Tier: 0,
            BaselinePrimary: 0.0,
            EnhancedPrimary: 0.0,
            PrimaryMetricName: "semantic_quality_claimed",
            LowerIsBetter: false,
            Passed: false,
            ShouldScaleUp: false,
            NextAction: "Run PromptToTask trainable semantic quality gates before any semantic completion claim.",
            ScalingSignal: 0.0,
            ScalingSignalNote: "CM-26A1 source hierarchy evidence only.",
            Notes: "Logical hierarchy delegates to PromptToTaskMachine.Translate and does not add semantic authority.",
            BaselineTimeMs: 0.0,
            EnhancedTimeMs: 0.0);

        var result = new LogicalFlowResult(
            FlowId: FlowId,
            ScenarioId: request.ScenarioId,
            OverallVerdict: finalResponse?.ContractStatus ?? PromptToTaskMachine.NotTrainedStatus,
            FinalHumanResult: finalResponse?.StructuredTask.TaskType ?? PromptToTaskMachine.NotReadyTaskType,
            Summary: summary,
            Metrics: new Dictionary<string, string>
            {
                ["logical_unit_count"] = _units.Count.ToString(),
                ["semantic_quality_claimed"] = bool.FalseString,
                ["training_run"] = bool.FalseString
            },
            EvidenceIds: evidence.Distinct(StringComparer.Ordinal).ToArray(),
            Warnings: warnings.Distinct(StringComparer.Ordinal).ToArray());

        return OperationResult<LogicalFlowResult>.Ok(result);
    }
}

public sealed class PromptToTaskIntentUnit(PromptToTaskMachine machine)
    : PromptToTaskLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "prompt_to_task_intent_unit";
    public const string Role = "intent_extraction";
}

public sealed class PromptToTaskConstraintUnit(PromptToTaskMachine machine)
    : PromptToTaskLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "prompt_to_task_constraint_unit";
    public const string Role = "constraint_extraction";
}

public sealed class PromptToTaskDeliverableUnit(PromptToTaskMachine machine)
    : PromptToTaskLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "prompt_to_task_deliverable_unit";
    public const string Role = "deliverable_extraction";
}

public sealed class PromptToTaskAuthorityBoundaryUnit(PromptToTaskMachine machine)
    : PromptToTaskLogicalUnitBase(machine, UnitId, Role)
{
    public const string UnitId = "prompt_to_task_authority_boundary_unit";
    public const string Role = "authority_boundary_check";
}

public abstract class PromptToTaskLogicalUnitBase(
    PromptToTaskMachine machine,
    string unitId,
    string unitRole) : ILogicalUnit<PromptToTaskRequest, PromptToTaskResponse>
{
    public Task<OperationResult<LogicalUnitResult<PromptToTaskResponse>>> ExecuteAsync(
        LogicalUnitRequest<PromptToTaskRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<PromptToTaskResponse>>.Fail("Unit cancelled.", "CANCELLED"));

        var translated = machine.Translate(request.Input);
        if (!translated.IsSuccess)
            return Task.FromResult(translated.MapError<LogicalUnitResult<PromptToTaskResponse>>());

        var result = new LogicalUnitResult<PromptToTaskResponse>(
            UnitId: unitId,
            UnitRole: unitRole,
            DomainTag: PromptToTaskMachine.DomainTag,
            Output: translated.Value!,
            Confidence: 0.0,
            PredicateIds: [],
            DomainTags: [PromptToTaskMachine.DomainTag],
            EvidenceIds:
            [
                "PROMPT_TO_TASK_LOGICAL_UNIT_SOURCE_BACKED",
                $"PROMPT_TO_TASK_UNIT_ROLE:{unitRole}",
                translated.Value!.ContractStatus,
                .. translated.Value.ContractEvidence
            ],
            Warnings:
            [
                "PROMPT_TO_TASK_LOGICAL_UNIT_DELEGATES_TRANSLATE",
                "NO_ADDITIONAL_SEMANTIC_CLASSIFICATION"
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<PromptToTaskResponse>>.Ok(result));
    }
}
