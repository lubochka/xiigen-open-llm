// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Dashboard;

using AgentInfrastructure;
using SmartSkillsDomain;

public interface IXiigenExtensionApiGateway
{
    OperationResult<XiigenExtensionResponse> ProcessHostExtensionRequest(
        XiigenExtensionRequest request);
}

public sealed class XiigenExtensionApiServiceGateway : IXiigenExtensionApiGateway
{
    private readonly XiigenExtensionApiService _service;

    public XiigenExtensionApiServiceGateway()
        : this(new XiigenExtensionApiService())
    {
    }

    public XiigenExtensionApiServiceGateway(XiigenExtensionApiService? service)
        => _service = service ?? new XiigenExtensionApiService();

    public OperationResult<XiigenExtensionResponse> ProcessHostExtensionRequest(
        XiigenExtensionRequest request)
        => _service.ProcessHostExtensionRequest(request);
}

public sealed record ApiDashboardRunResult(
    XiigenExtensionRequest? Request,
    XiigenExtensionResponse Response,
    ApiDashboardResultViewModel ViewModel,
    bool CoreBoundaryCalled,
    bool ExternalAiUsed,
    bool ScriptParityUsed,
    bool HostRuntimeProofClaimed,
    bool RuntimeExtensionProofClaimed,
    bool SemanticQualityOrTrainedModelProofClaimed,
    string EvidenceScope);

public sealed class ApiDashboardRunner
{
    public const string DashboardValidationFailedStatus = "dashboard_validation_failed";
    public const string DashboardCoreOperationFailedStatus = "dashboard_core_operation_failed";
    public const string DashboardEvidenceScope = "dashboard-to-core invocation only; not extension host runtime proof";

    private readonly IXiigenExtensionApiGateway _gateway;

    public ApiDashboardRunner()
        : this(new XiigenExtensionApiServiceGateway())
    {
    }

    public ApiDashboardRunner(IXiigenExtensionApiGateway? gateway)
        => _gateway = gateway ?? new XiigenExtensionApiServiceGateway();

    public OperationResult<ApiDashboardRunResult> Run(
        ApiDashboardCatalogRow selectedRow,
        ApiDashboardFixtureExampleRow selectedFixture,
        bool debugEnabled,
        bool showRawJson = false,
        string? correlationId = null)
        => RunRequestBuildResult(
            ApiDashboardRequestBuilder.BuildRequest(
                selectedRow,
                selectedFixture,
                debugEnabled,
                correlationId),
            debugEnabled,
            showRawJson);

    public OperationResult<ApiDashboardRunResult> Run(
        ApiDashboardCatalogRow selectedRow,
        string textInput,
        bool debugEnabled,
        bool showRawJson = false,
        string? correlationId = null,
        IReadOnlyDictionary<string, string>? hostFacts = null,
        IReadOnlyList<string>? requestedSkills = null,
        IReadOnlyList<string>? availableSkills = null,
        IReadOnlyDictionary<string, string>? checkSetInputs = null,
        IReadOnlyDictionary<string, string>? provenance = null)
        => RunRequestBuildResult(
            ApiDashboardRequestBuilder.BuildRequest(
                selectedRow,
                textInput,
                debugEnabled,
                correlationId,
                hostFacts,
                requestedSkills,
                availableSkills,
                checkSetInputs,
                provenance),
            debugEnabled,
            showRawJson);

    public OperationResult<ApiDashboardRunResult> Run(
        string methodId,
        string textInput,
        bool debugEnabled,
        bool showRawJson = false,
        string? correlationId = null,
        IReadOnlyDictionary<string, string>? hostFacts = null,
        IReadOnlyList<string>? requestedSkills = null,
        IReadOnlyList<string>? availableSkills = null,
        IReadOnlyDictionary<string, string>? checkSetInputs = null,
        IReadOnlyDictionary<string, string>? provenance = null)
        => RunRequestBuildResult(
            ApiDashboardRequestBuilder.BuildRequest(
                methodId,
                textInput,
                debugEnabled,
                correlationId,
                hostFacts,
                requestedSkills,
                availableSkills,
                checkSetInputs,
                provenance),
            debugEnabled,
            showRawJson);

    public OperationResult<ApiDashboardRunResult> Run(
        XiigenExtensionRequest request,
        bool showRawJson = false)
    {
        if (request is null)
        {
            return BuildVisibleErrorResult(
                request: null,
                status: DashboardValidationFailedStatus,
                displayText: "Dashboard request validation failed.",
                errorCode: "API_DASHBOARD_REQUEST_REQUIRED",
                errorMessage: "Dashboard runner requires a xiigen extension request.",
                debugRequested: false,
                showRawJson,
                coreBoundaryCalled: false);
        }

        var coreResult = _gateway.ProcessHostExtensionRequest(request);
        if (!coreResult.IsSuccess || coreResult.Value is null)
        {
            return BuildVisibleErrorResult(
                request,
                DashboardCoreOperationFailedStatus,
                "Dashboard core call failed before a xiigen response was returned.",
                string.IsNullOrWhiteSpace(coreResult.ErrorCode) ? "API_DASHBOARD_CORE_OPERATION_FAILED" : coreResult.ErrorCode,
                string.IsNullOrWhiteSpace(coreResult.ErrorMessage) ? "Core boundary returned no response." : coreResult.ErrorMessage,
                request.DebugEnabled,
                showRawJson,
                coreBoundaryCalled: true);
        }

        return BuildPresentedResult(
            request,
            coreResult.Value,
            request.DebugEnabled,
            showRawJson,
            coreBoundaryCalled: true);
    }

    private OperationResult<ApiDashboardRunResult> RunRequestBuildResult(
        OperationResult<XiigenExtensionRequest> requestResult,
        bool debugRequested,
        bool showRawJson)
    {
        if (!requestResult.IsSuccess || requestResult.Value is null)
        {
            return BuildVisibleErrorResult(
                request: null,
                status: DashboardValidationFailedStatus,
                displayText: "Dashboard request validation failed.",
                errorCode: string.IsNullOrWhiteSpace(requestResult.ErrorCode) ? "API_DASHBOARD_REQUEST_BUILD_FAILED" : requestResult.ErrorCode,
                errorMessage: string.IsNullOrWhiteSpace(requestResult.ErrorMessage) ? "Dashboard request builder returned no request." : requestResult.ErrorMessage,
                debugRequested,
                showRawJson,
                coreBoundaryCalled: false);
        }

        return Run(requestResult.Value, showRawJson);
    }

    private static OperationResult<ApiDashboardRunResult> BuildVisibleErrorResult(
        XiigenExtensionRequest? request,
        string status,
        string displayText,
        string errorCode,
        string errorMessage,
        bool debugRequested,
        bool showRawJson,
        bool coreBoundaryCalled)
    {
        var response = new XiigenExtensionResponse(
            Success: false,
            Status: status,
            ChatDisplayText: displayText,
            SelectedSkills: [],
            CheckSetResults: BuildBoundaryCheckSet(coreBoundaryCalled),
            StructuredTaskRefs: [],
            ScenarioRefs: [],
            Warnings:
            [
                "semantic_quality_claimed=false",
                "core_boundary_only_no_host_runtime_claim",
                "script_parity_used=false",
                "external_ai=false"
            ],
            Errors: BuildErrorList(errorCode, errorMessage),
            DebugTrace: null);

        return BuildPresentedResult(
            request,
            response,
            debugRequested,
            showRawJson,
            coreBoundaryCalled);
    }

    private static OperationResult<ApiDashboardRunResult> BuildPresentedResult(
        XiigenExtensionRequest? request,
        XiigenExtensionResponse response,
        bool debugRequested,
        bool showRawJson,
        bool coreBoundaryCalled)
    {
        var presenterResult = ApiDashboardResultPresenter.BuildViewModel(
            response,
            debugRequested,
            showRawJson);
        if (!presenterResult.IsSuccess || presenterResult.Value is null)
        {
            return presenterResult.MapError<ApiDashboardRunResult>();
        }

        var viewModel = presenterResult.Value;
        var result = new ApiDashboardRunResult(
            Request: request,
            Response: response,
            ViewModel: viewModel,
            CoreBoundaryCalled: coreBoundaryCalled,
            ExternalAiUsed: IsExplicitTrue(response.CheckSetResults, "external_ai"),
            ScriptParityUsed: IsExplicitTrue(response.CheckSetResults, "script_parity_used"),
            HostRuntimeProofClaimed: IsExplicitTrue(response.CheckSetResults, "host_runtime_claimed"),
            RuntimeExtensionProofClaimed: viewModel.RuntimeExtensionProofClaimed,
            SemanticQualityOrTrainedModelProofClaimed: viewModel.SemanticQualityOrTrainedModelProofClaimed,
            EvidenceScope: DashboardEvidenceScope);

        return OperationResult<ApiDashboardRunResult>.Ok(result);
    }

    private static IReadOnlyDictionary<string, string> BuildBoundaryCheckSet(
        bool coreBoundaryCalled)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["core_boundary_called"] = coreBoundaryCalled.ToString().ToLowerInvariant(),
            ["core_boundary_executed"] = "false",
            ["runtime_extension_claimed"] = "false",
            ["semantic_quality_claimed"] = "false",
            ["trained_model_proof_claimed"] = "false",
            ["host_runtime_claimed"] = "false",
            ["script_parity_used"] = "false",
            ["external_ai"] = "false"
        };

    private static IReadOnlyList<string> BuildErrorList(
        string errorCode,
        string errorMessage)
    {
        var errors = new List<string>();
        if (!string.IsNullOrWhiteSpace(errorCode))
            errors.Add(errorCode);
        if (!string.IsNullOrWhiteSpace(errorMessage))
            errors.Add(errorMessage);

        return errors;
    }

    private static bool IsExplicitTrue(
        IReadOnlyDictionary<string, string>? values,
        string key)
        => values is not null
            && values.TryGetValue(key, out var value)
            && bool.TryParse(value, out var parsed)
            && parsed;
}
