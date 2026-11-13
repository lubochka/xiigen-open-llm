// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Resolves and executes exactly one admitted L3 logical flow by its selected target identity.
/// The adapter does not retry, use registry route fallbacks, inspect selection evidence, mutate
/// topology, or make aggregate quality, training, DPO, promotion, or lifecycle claims.
/// </summary>
public sealed class DynamicGraphRegistryL3SelectedTargetExecutor
    : IDynamicGraphL3SelectedTargetExecutor
{
    private const string RegistryRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_REGISTRY_REQUIRED";
    private const string RegistryRequiredMessage =
        "Dynamic graph L3 selected-target flow registry is required.";
    private const string RequestInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_REQUEST_INVALID";
    private const string RequestInvalidMessage =
        "Dynamic graph L3 selected-target flow request is invalid.";
    private const string AlreadyStartedCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_EXECUTION_ALREADY_STARTED";
    private const string AlreadyStartedMessage =
        "Dynamic graph L3 selected-target flow execution has already started.";
    private const string AssociationInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_ASSOCIATION_INVALID";
    private const string AssociationInvalidMessage =
        "Dynamic graph L3 selected-target association is invalid.";
    private const string ResolutionFailedCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_RESOLUTION_FAILED";
    private const string ResolutionFailedMessage =
        "Dynamic graph L3 selected-target flow resolution failed.";
    private const string ExecutionFailedCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_EXECUTION_FAILED";
    private const string ExecutionFailedMessage =
        "Dynamic graph L3 selected-target flow execution failed.";
    private const string ResultRequiredCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_RESULT_REQUIRED";
    private const string ResultRequiredMessage =
        "Dynamic graph L3 selected-target flow execution result is required.";
    private const string ResultInvalidCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_RESULT_INVALID";
    private const string ResultInvalidMessage =
        "Dynamic graph L3 selected-target flow execution result is invalid.";
    private const string IdentityMismatchCode =
        "DYNAMIC_GRAPH_L3_SELECTED_TARGET_FLOW_RESULT_IDENTITY_MISMATCH";
    private const string IdentityMismatchMessage =
        "Dynamic graph L3 selected-target flow execution result does not match the request.";
    private const string CancelledCode = "CANCELLED";
    private const string CancelledMessage = "Cancelled.";

    private readonly InMemoryLogicalFlowRegistry _registry;
    private readonly string _runId;
    private readonly string _scenarioId;
    private readonly string _input;
    private readonly LogicalExecutionFlags _flags;
    private readonly IReadOnlyDictionary<string, object> _metadata;
    private readonly LogicalFlowFactoryRequest? _factoryRequest;
    private int _started;

    private DynamicGraphRegistryL3SelectedTargetExecutor(
        InMemoryLogicalFlowRegistry registry,
        string runId,
        string scenarioId,
        string input,
        LogicalExecutionFlags flags,
        IReadOnlyDictionary<string, object> metadata,
        LogicalFlowFactoryRequest? factoryRequest)
    {
        _registry = registry;
        _runId = runId;
        _scenarioId = scenarioId;
        _input = input;
        _flags = flags;
        _metadata = metadata;
        _factoryRequest = factoryRequest;
    }

    public static OperationResult<DynamicGraphRegistryL3SelectedTargetExecutor> Create(
        InMemoryLogicalFlowRegistry? registry,
        string? runId,
        string? scenarioId,
        string? input,
        LogicalExecutionFlags? flags,
        IReadOnlyDictionary<string, object>? metadata)
    {
        if (registry is null)
        {
            return Failure<DynamicGraphRegistryL3SelectedTargetExecutor>(
                RegistryRequiredMessage,
                RegistryRequiredCode);
        }

        if (!IsCanonicalIdentity(runId)
            || !IsCanonicalIdentity(scenarioId)
            || string.IsNullOrWhiteSpace(input)
            || flags is null
            || !LogicalMetadataSnapshot.TryCreate(metadata, out var ownedMetadata))
        {
            return Failure<DynamicGraphRegistryL3SelectedTargetExecutor>(
                RequestInvalidMessage,
                RequestInvalidCode);
        }

        return OperationResult<DynamicGraphRegistryL3SelectedTargetExecutor>.Ok(
            new DynamicGraphRegistryL3SelectedTargetExecutor(
                registry,
                runId!,
                scenarioId!,
                input!,
                flags with { },
                ownedMetadata,
                factoryRequest: null));
    }

    public static OperationResult<DynamicGraphRegistryL3SelectedTargetExecutor> CreateFactoryAware(
        InMemoryLogicalFlowRegistry? registry,
        string? scenarioId,
        string? input,
        LogicalFlowFactoryRequest? factoryRequest)
    {
        if (registry is null)
        {
            return Failure<DynamicGraphRegistryL3SelectedTargetExecutor>(
                RegistryRequiredMessage,
                RegistryRequiredCode);
        }

        if (factoryRequest is null
            || !IsCanonicalIdentity(factoryRequest.RunId)
            || !IsCanonicalIdentity(scenarioId)
            || string.IsNullOrWhiteSpace(input)
            || factoryRequest.Flags is null
            || factoryRequest.Units is null
            || !Enum.IsDefined(factoryRequest.MinimumTrustState)
            || !LogicalMetadataSnapshot.TryCreate(
                factoryRequest.Metadata,
                out var ownedMetadata))
        {
            return Failure<DynamicGraphRegistryL3SelectedTargetExecutor>(
                RequestInvalidMessage,
                RequestInvalidCode);
        }

        var ownedFlags = factoryRequest.Flags with { };
        var ownedFactoryRequest = new LogicalFlowFactoryRequest(
            factoryRequest.RunId,
            ownedFlags with { },
            factoryRequest.Units,
            ownedMetadata,
            factoryRequest.Topology)
        {
            MinimumTrustState = factoryRequest.MinimumTrustState
        };

        return OperationResult<DynamicGraphRegistryL3SelectedTargetExecutor>.Ok(
            new DynamicGraphRegistryL3SelectedTargetExecutor(
                registry,
                factoryRequest.RunId,
                scenarioId!,
                input!,
                ownedFlags,
                ownedMetadata,
                ownedFactoryRequest));
    }

    public async Task<OperationResult<DynamicGraphL3SelectedTargetExecutionResult>> ExecuteAsync(
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
        {
            return Failure<DynamicGraphL3SelectedTargetExecutionResult>(
                AlreadyStartedMessage,
                AlreadyStartedCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        DynamicGraphMeasurementTarget target;
        string authorityEdgeId;
        try
        {
            if (association is null)
                return AssociationInvalid();

            (target, authorityEdgeId) = association.SnapshotTargetAuthorityEdgeBinding();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return AssociationInvalid();
        }

        if (target.Level != DynamicGraphMeasurementTargetLevel.L3LogicalFlow
            || target.MechanismKind != DynamicGraphMeasurementMechanismKind.LearningSubject
            || target.DeterministicFinalAuthority
            || !IsCanonicalIdentity(target.TargetId)
            || !IsCanonicalIdentity(authorityEdgeId)
            || !StringComparer.Ordinal.Equals(target.TargetId, authorityEdgeId))
        {
            return AssociationInvalid();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        ILogicalFlow<string> flow;
        LogicalFlowRequest<string> request;
        if (_factoryRequest is null)
        {
            OperationResult<ILogicalFlow<string>>? resolution;
            try
            {
                resolution = _registry.Resolve<string>(target.TargetId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Cancelled();
            }
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return ResolutionFailed();
            }

            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            if (resolution is null || !resolution.IsSuccess || resolution.Value is null)
                return ResolutionFailed();

            if (!LogicalMetadataSnapshot.TryCreate(_metadata, out var requestMetadata))
            {
                return Failure<DynamicGraphL3SelectedTargetExecutionResult>(
                    RequestInvalidMessage,
                    RequestInvalidCode);
            }

            flow = resolution.Value;
            request = new LogicalFlowRequest<string>(
                _runId,
                target.TargetId,
                _scenarioId,
                _input,
                _flags with { },
                requestMetadata);
        }
        else
        {
            if (!LogicalMetadataSnapshot.TryCreate(
                    _factoryRequest.Metadata,
                    out var factoryMetadata)
                || !LogicalMetadataSnapshot.TryCreate(
                    _metadata,
                    out var requestMetadata))
            {
                return Failure<DynamicGraphL3SelectedTargetExecutionResult>(
                    RequestInvalidMessage,
                    RequestInvalidCode);
            }

            var freshFactoryRequest = new LogicalFlowFactoryRequest(
                _factoryRequest.RunId,
                _factoryRequest.Flags with { },
                _factoryRequest.Units,
                factoryMetadata,
                _factoryRequest.Topology)
            {
                MinimumTrustState = _factoryRequest.MinimumTrustState
            };
            request = new LogicalFlowRequest<string>(
                _runId,
                target.TargetId,
                _scenarioId,
                _input,
                _flags with { },
                requestMetadata);

            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            OperationResult<LogicalFlowBinding>? binding;
            try
            {
                binding = _registry.ResolveFactoryBindingByFlowId(
                    target.TargetId,
                    _scenarioId,
                    freshFactoryRequest);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Cancelled();
            }
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return ResolutionFailed();
            }

            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            if (binding is null
                || !binding.IsSuccess
                || binding.Value?.Flow is null
                || !StringComparer.Ordinal.Equals(binding.Value.FlowId, target.TargetId)
                || !StringComparer.Ordinal.Equals(binding.Value.ScenarioId, _scenarioId))
            {
                return ResolutionFailed();
            }

            flow = binding.Value.Flow;
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<LogicalFlowResult>? execution;
        try
        {
            var executionTask = flow.ExecuteAsync(request, cancellationToken);
            if (executionTask is null)
            {
                return Failure<DynamicGraphL3SelectedTargetExecutionResult>(
                    ResultRequiredMessage,
                    ResultRequiredCode);
            }

            execution = await executionTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure<DynamicGraphL3SelectedTargetExecutionResult>(
                ExecutionFailedMessage,
                ExecutionFailedCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (execution is null || (execution.IsSuccess && execution.Value is null))
        {
            return Failure<DynamicGraphL3SelectedTargetExecutionResult>(
                ResultRequiredMessage,
                ResultRequiredCode);
        }

        if (!execution.IsSuccess)
            return execution.MapError<DynamicGraphL3SelectedTargetExecutionResult>();

        LogicalFlowResult stableResult;
        try
        {
            if (!LogicalMetadataSnapshot.TryCreate(execution.Value, out stableResult)
                || !IsValidResultStructure(stableResult))
            {
                return Failure<DynamicGraphL3SelectedTargetExecutionResult>(
                    ResultInvalidMessage,
                    ResultInvalidCode);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure<DynamicGraphL3SelectedTargetExecutionResult>(
                ResultInvalidMessage,
                ResultInvalidCode);
        }

        if (!StringComparer.Ordinal.Equals(stableResult.FlowId, request.FlowId)
            || !StringComparer.Ordinal.Equals(stableResult.ScenarioId, request.ScenarioId))
        {
            return Failure<DynamicGraphL3SelectedTargetExecutionResult>(
                IdentityMismatchMessage,
                IdentityMismatchCode);
        }

        var result = new DynamicGraphL3SelectedTargetExecutionResult(
            target.TargetId,
            _runId,
            stableResult.EvidenceIds);

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        return OperationResult<DynamicGraphL3SelectedTargetExecutionResult>.Ok(result);
    }

    private static bool IsValidResultStructure(LogicalFlowResult result)
    {
        var summary = result.Summary;
        return IsCanonicalIdentity(result.FlowId)
            && IsCanonicalIdentity(result.ScenarioId)
            && IsCanonicalIdentity(result.OverallVerdict)
            && IsSafeFreeText(result.FinalHumanResult, requireNonEmpty: true)
            && summary is not null
            && IsCanonicalIdentity(summary.SummaryId)
            && summary.Tier >= 0
            && double.IsFinite(summary.BaselinePrimary)
            && double.IsFinite(summary.EnhancedPrimary)
            && IsCanonicalIdentity(summary.PrimaryMetricName)
            && IsCanonicalIdentity(summary.NextAction)
            && double.IsFinite(summary.ScalingSignal)
            && IsSafeFreeText(summary.ScalingSignalNote, requireNonEmpty: false)
            && IsSafeFreeText(summary.Notes, requireNonEmpty: false)
            && double.IsFinite(summary.BaselineTimeMs)
            && double.IsFinite(summary.EnhancedTimeMs)
            && result.Metrics is not null
            && result.Metrics.All(metric =>
                IsCanonicalIdentity(metric.Key)
                && IsSafeFreeText(metric.Value, requireNonEmpty: false))
            && AreCanonicalOrdinalUnique(result.EvidenceIds, requireNonEmpty: true)
            && result.Warnings is not null
            && result.Warnings.All(value => IsSafeFreeText(value, requireNonEmpty: false))
            && result.TransitionSignals is not null
            && result.TransitionSignals.All(IsValidTransitionSignal)
            && (result.EdgeTargetHandoff is null
                || IsValidEdgeTargetHandoff(result.EdgeTargetHandoff));
    }

    private static bool IsValidTransitionSignal(PhaseTransitionSignal signal)
        => signal is not null
           && IsSafeFreeText(signal.TransitionPrompt, requireNonEmpty: true)
           && IsCanonicalIdentity(signal.SourceNodeId)
           && IsCanonicalIdentity(signal.SourceSubPhaseId)
           && AreCanonicalOrdinalUnique(signal.EvidenceIds, requireNonEmpty: false);

    private static bool IsValidEdgeTargetHandoff(DynamicGraphEdgeTargetHandoff handoff)
    {
        if (!IsCanonicalIdentity(handoff.RunId)
            || !IsCanonicalIdentity(handoff.EdgeId)
            || !IsCanonicalIdentity(handoff.FromNodeId)
            || !IsCanonicalIdentity(handoff.DomainTag)
            || !Enum.IsDefined(handoff.Kind)
            || !IsCanonicalIdentity(handoff.AuthoritySource)
            || !IsCanonicalIdentity(handoff.UpstreamOutputHash)
            || !IsCanonicalIdentity(handoff.ProjectionRule)
            || handoff.OrderedTargets is null
            || !AreCanonicalOrdinalUnique(handoff.UpstreamCandidateRefs, requireNonEmpty: true))
        {
            return false;
        }

        var targetIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < handoff.OrderedTargets.Count; index++)
        {
            var target = handoff.OrderedTargets[index];
            if (target is null
                || !IsCanonicalIdentity(target.TargetNodeId)
                || !targetIds.Add(target.TargetNodeId)
                || !double.IsFinite(target.Confidence)
                || target.Confidence is < 0d or > 1d
                || target.Ordinal != index)
            {
                return false;
            }
        }

        return handoff.KnowledgeEdge == (handoff.OrderedTargets.Count == 0);
    }

    private static bool AreCanonicalOrdinalUnique(
        IReadOnlyList<string>? values,
        bool requireNonEmpty)
    {
        if (values is null || (requireNonEmpty && values.Count == 0))
            return false;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < values.Count; index++)
        {
            if (!IsCanonicalIdentity(values[index]) || !seen.Add(values[index]))
                return false;
        }

        return true;
    }

    private static bool IsSafeFreeText(string? value, bool requireNonEmpty)
    {
        if (value is null || (requireNonEmpty && string.IsNullOrWhiteSpace(value)))
            return false;

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character is '\r' or '\n' or '\t')
                continue;

            if (char.IsControl(character) || character is '\u0085' or '\u2028' or '\u2029')
                return false;

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                    return false;

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    private static bool IsCanonicalIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim()))
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character)
                || character is '\u0085' or '\u2028' or '\u2029')
            {
                return false;
            }

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                    return false;

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    private static OperationResult<DynamicGraphL3SelectedTargetExecutionResult>
        AssociationInvalid()
        => Failure<DynamicGraphL3SelectedTargetExecutionResult>(
            AssociationInvalidMessage,
            AssociationInvalidCode);

    private static OperationResult<DynamicGraphL3SelectedTargetExecutionResult>
        ResolutionFailed()
        => Failure<DynamicGraphL3SelectedTargetExecutionResult>(
            ResolutionFailedMessage,
            ResolutionFailedCode);

    private static OperationResult<DynamicGraphL3SelectedTargetExecutionResult> Cancelled()
        => Failure<DynamicGraphL3SelectedTargetExecutionResult>(
            CancelledMessage,
            CancelledCode);

    private static OperationResult<T> Failure<T>(string message, string code)
        => OperationResult<T>.Fail(message, code);
}
