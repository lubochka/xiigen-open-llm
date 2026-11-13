// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

// Exposes the typed DesignConversion routing flow through the string-input orchestrator registry.
public sealed class DesignConversionOrchestrationFlowAdapter : ILogicalFlow<string>
{
    public const string RuntimeFlowInputMetadataKey = "runtime_flow_input";
    public const string DesignConversionInputMetadataAlias = "design_conversion_input";
    public const string InputRequiredErrorCode = "DESIGN_CONVERSION_INPUT_REQUIRED";
    public const string InputWrongTypeErrorCode = "DESIGN_CONVERSION_INPUT_WRONG_TYPE";

    private readonly DesignConversionRoutingFlow _innerFlow;

    public DesignConversionOrchestrationFlowAdapter(DesignConversionRoutingFlow? innerFlow = null)
    {
        _innerFlow = innerFlow ?? new DesignConversionRoutingFlow();
    }

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail(
                "DesignConversion orchestration adapter cancelled.",
                "CANCELLED");

        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail(
                "Logical flow request is required.",
                "REQUEST_REQUIRED");

        if (!string.Equals(request.FlowId, DesignConversionRoutingFlow.FlowId, StringComparison.Ordinal))
            return OperationResult<LogicalFlowResult>.Fail(
                $"Unsupported flow id '{request.FlowId}'. Expected '{DesignConversionRoutingFlow.FlowId}'.",
                "UNSUPPORTED_FLOW_ID");

        if (!TryReadTypedInput(request.Metadata, out var input, out var errorCode, out var errorMessage))
            return OperationResult<LogicalFlowResult>.Fail(errorMessage, errorCode);

        var typedRequest = new LogicalFlowRequest<DesignConversionRuntimeFlowInput>(
            RunId: request.RunId,
            FlowId: DesignConversionRoutingFlow.FlowId,
            ScenarioId: DesignConversionRoutingFlow.ScenarioId,
            Input: input,
            Flags: request.Flags,
            Metadata: request.Metadata);

        return await _innerFlow.ExecuteAsync(typedRequest, cancellationToken);
    }

    private static bool TryReadTypedInput(
        IReadOnlyDictionary<string, object>? metadata,
        out DesignConversionRuntimeFlowInput input,
        out string errorCode,
        out string errorMessage)
    {
        input = null!;
        errorCode = string.Empty;
        errorMessage = string.Empty;

        if (metadata is null)
        {
            errorCode = InputRequiredErrorCode;
            errorMessage = $"DesignConversionRuntimeFlowInput must be supplied in metadata under '{RuntimeFlowInputMetadataKey}'.";
            return false;
        }

        if (metadata.TryGetValue(RuntimeFlowInputMetadataKey, out var runtimeInput))
            return TryAcceptValue(RuntimeFlowInputMetadataKey, runtimeInput, out input, out errorCode, out errorMessage);

        if (metadata.TryGetValue(DesignConversionInputMetadataAlias, out var aliasInput))
            return TryAcceptValue(DesignConversionInputMetadataAlias, aliasInput, out input, out errorCode, out errorMessage);

        errorCode = InputRequiredErrorCode;
        errorMessage = $"DesignConversionRuntimeFlowInput must be supplied in metadata under '{RuntimeFlowInputMetadataKey}'.";
        return false;
    }

    private static bool TryAcceptValue(
        string key,
        object value,
        out DesignConversionRuntimeFlowInput input,
        out string errorCode,
        out string errorMessage)
    {
        if (value is DesignConversionRuntimeFlowInput typedInput)
        {
            input = typedInput;
            errorCode = string.Empty;
            errorMessage = string.Empty;
            return true;
        }

        input = null!;
        errorCode = InputWrongTypeErrorCode;
        errorMessage = $"Metadata key '{key}' must contain a DesignConversionRuntimeFlowInput typed object.";
        return false;
    }
}
