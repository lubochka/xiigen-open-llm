// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

// Marker type for the L5 dynamic-graph learning-machine boundary.
public sealed class DynamicGraphDomain
{
    public const string DomainTag = "DynamicGraphDomain";
}

// Creates the existing baseline orchestrator without adding another runtime hierarchy level.
public delegate Task<OperationResult<ILogicalOrchestrator>> DynamicGraphOrchestratorFactory(
    CancellationToken cancellationToken);

// L5 ownership scaffold. Training lifecycle methods deliberately fail until a real trained path exists.
public sealed class DynamicGraphMachine : ILearningMachine<DynamicGraphDomain>
{
    public const string DomainTag = DynamicGraphDomain.DomainTag;
    public const string RuntimeMode = "honest_untrained";
    public const bool SemanticQualityClaimed = false;

    public const string TrainingNotActivatedCode = "DYNAMIC_GRAPH_TRAINING_NOT_ACTIVATED";
    public const string EvaluationNotActivatedCode = "DYNAMIC_GRAPH_EVALUATION_NOT_ACTIVATED";
    public const string ExportNotActivatedCode = "DYNAMIC_GRAPH_EXPORT_NOT_ACTIVATED";
    public const string ImportNotActivatedCode = "DYNAMIC_GRAPH_IMPORT_NOT_ACTIVATED";
    public const string FactoryResultRequiredCode = "DYNAMIC_GRAPH_ORCHESTRATOR_FACTORY_RESULT_REQUIRED";
    public const string OrchestratorRequiredCode = "DYNAMIC_GRAPH_ORCHESTRATOR_REQUIRED";
    public const string FactoryFailedCode = "DYNAMIC_GRAPH_ORCHESTRATOR_FACTORY_FAILED";

    private const string CancelledMessage = "Dynamic graph machine operation was cancelled.";
    private readonly DynamicGraphOrchestratorFactory _orchestratorFactory;

    public DynamicGraphMachine(DynamicGraphOrchestratorFactory orchestratorFactory)
    {
        _orchestratorFactory = orchestratorFactory
            ?? throw new ArgumentNullException(nameof(orchestratorFactory));
    }

    public Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<DynamicGraphDomain> batch,
        CancellationToken ct = default)
        => Task.FromResult(ct.IsCancellationRequested
            ? OperationResult<LearningBatchResult>.Fail(CancelledMessage, "CANCELLED")
            : OperationResult<LearningBatchResult>.Fail(
                "Dynamic graph training is not activated.",
                TrainingNotActivatedCode));

    public Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<DynamicGraphDomain> evalSet,
        CancellationToken ct = default)
        => Task.FromResult(ct.IsCancellationRequested
            ? OperationResult<LearningEvaluationResult>.Fail(CancelledMessage, "CANCELLED")
            : OperationResult<LearningEvaluationResult>.Fail(
                "Dynamic graph evaluation is not activated.",
                EvaluationNotActivatedCode));

    public Task<OperationResult<LearningMachineExport>> ExportAsync(CancellationToken ct = default)
        => Task.FromResult(ct.IsCancellationRequested
            ? OperationResult<LearningMachineExport>.Fail(CancelledMessage, "CANCELLED")
            : OperationResult<LearningMachineExport>.Fail(
                "Dynamic graph export is not activated.",
                ExportNotActivatedCode));

    public Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export,
        CancellationToken ct = default)
        => Task.FromResult(ct.IsCancellationRequested
            ? OperationResult<bool>.Fail(CancelledMessage, "CANCELLED")
            : OperationResult<bool>.Fail(
                "Dynamic graph import is not activated.",
                ImportNotActivatedCode));

    public async Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ILogicalOrchestrator>.Fail(CancelledMessage, "CANCELLED");

        OperationResult<ILogicalOrchestrator>? result;
        try
        {
            var pendingResult = _orchestratorFactory(ct);
            if (pendingResult is null)
            {
                if (ct.IsCancellationRequested)
                    return OperationResult<ILogicalOrchestrator>.Fail(CancelledMessage, "CANCELLED");

                return OperationResult<ILogicalOrchestrator>.Fail(
                    "Dynamic graph orchestrator factory must return a result.",
                    FactoryResultRequiredCode);
            }

            result = await pendingResult.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<ILogicalOrchestrator>.Fail(CancelledMessage, "CANCELLED");
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (ct.IsCancellationRequested)
                return OperationResult<ILogicalOrchestrator>.Fail(CancelledMessage, "CANCELLED");

            return OperationResult<ILogicalOrchestrator>.Fail(
                "Dynamic graph orchestrator factory failed.",
                FactoryFailedCode);
        }

        if (ct.IsCancellationRequested)
            return OperationResult<ILogicalOrchestrator>.Fail(CancelledMessage, "CANCELLED");

        if (result is null)
        {
            return OperationResult<ILogicalOrchestrator>.Fail(
                "Dynamic graph orchestrator factory must return a result.",
                FactoryResultRequiredCode);
        }

        if (!result.IsSuccess)
            return result;

        if (result.Value is null)
        {
            return OperationResult<ILogicalOrchestrator>.Fail(
                "Successful dynamic graph orchestrator factory result must contain an orchestrator.",
                OrchestratorRequiredCode);
        }

        return result;
    }
}
