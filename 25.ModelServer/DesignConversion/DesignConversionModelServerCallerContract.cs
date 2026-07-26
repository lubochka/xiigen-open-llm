// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using DesignConversionDomain;

public sealed record DesignConversionModelServerCallerContractField(
    string JsonName,
    string DotNetType,
    bool Required,
    string Requirement,
    bool MayCarryPrivateInput,
    bool ResponseMayEchoRawValue);

public sealed record DesignConversionModelServerCallerContractValidationFailure(
    string ErrorCode,
    int StatusCode,
    string Trigger);

public sealed record DesignConversionModelServerCallerContractResponseField(
    string JsonName,
    string DotNetType,
    bool Required,
    string Description);

public sealed record DesignConversionModelServerCallerContractExample(
    string Name,
    DesignConversionSourceKind SourceKind,
    string RequestId,
    string Description);

public sealed record DesignConversionModelServerCallerContractDescriptor(
    string SchemaVersion,
    string Boundary,
    string EndpointMethod,
    string EndpointPath,
    string RequestContentType,
    string ResponseContentType,
    IReadOnlyList<string> SupportedSourceKinds,
    IReadOnlyList<DesignConversionModelServerCallerContractField> RequestFields,
    IReadOnlyList<DesignConversionModelServerCallerContractResponseField> ResponseFields,
    IReadOnlyList<DesignConversionModelServerCallerContractValidationFailure> ValidationFailures,
    IReadOnlyList<DesignConversionModelServerCallerContractExample> Examples,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerLocalCallerResponseEnvelopeDescriptor(
    string SchemaVersion,
    string Boundary,
    IReadOnlyList<string> EnvelopeKinds,
    IReadOnlyList<DesignConversionModelServerCallerContractResponseField> Fields,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerImprovementLoopContractDescriptor(
    string SchemaVersion,
    string Boundary,
    string EndpointMethod,
    string EndpointPath,
    string RequestContentType,
    string ResponseContentType,
    IReadOnlyList<DesignConversionModelServerCallerContractField> RequestFields,
    IReadOnlyList<DesignConversionModelServerCallerContractResponseField> ResponseFields,
    IReadOnlyList<DesignConversionModelServerCallerContractResponseField> LocalCallerEnvelopeFields,
    IReadOnlyList<DesignConversionModelServerCallerContractValidationFailure> ValidationFailures,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerStatusReadbackContractDescriptor(
    string SchemaVersion,
    string Boundary,
    string EndpointMethod,
    string EndpointPath,
    string RequestContentType,
    string ResponseContentType,
    IReadOnlyList<DesignConversionModelServerCallerContractField> RequestFields,
    IReadOnlyList<DesignConversionModelServerCallerContractResponseField> ResponseFields,
    IReadOnlyList<DesignConversionModelServerCallerContractResponseField> LocalCallerEnvelopeFields,
    IReadOnlyList<DesignConversionModelServerCallerContractValidationFailure> ValidationFailures,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerDpoUsefulnessContractDescriptor(
    string SchemaVersion,
    string Boundary,
    string EndpointMethod,
    string EndpointPath,
    string RequestContentType,
    string ResponseContentType,
    IReadOnlyList<DesignConversionModelServerCallerContractField> RequestFields,
    IReadOnlyList<DesignConversionModelServerCallerContractResponseField> ResponseFields,
    IReadOnlyList<DesignConversionModelServerCallerContractResponseField> LocalCallerEnvelopeFields,
    IReadOnlyList<DesignConversionModelServerCallerContractValidationFailure> ValidationFailures,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerTrainingIntakeContractDescriptor(
    string SchemaVersion,
    string Boundary,
    string EndpointMethod,
    string EndpointPath,
    string RequestContentType,
    string ResponseContentType,
    IReadOnlyList<DesignConversionModelServerCallerContractField> RequestFields,
    IReadOnlyList<DesignConversionModelServerCallerContractResponseField> ResponseFields,
    IReadOnlyList<DesignConversionModelServerCallerContractResponseField> LocalCallerEnvelopeFields,
    IReadOnlyList<DesignConversionModelServerCallerContractValidationFailure> ValidationFailures,
    IReadOnlyList<string> SupportedActions,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public static class DesignConversionModelServerCallerContract
{
    public const string SchemaVersion = "design_conversion_model_server_caller_contract_v1";
    public const string Boundary = "local_caller_contract_for_figma_canva_like_clients_no_external_repo_wiring";
    public const string LocalCallerResponseEnvelopeSchemaVersion = "design_conversion_local_caller_response_envelope_v1";
    public const string LocalCallerResponseEnvelopeBoundary =
        "stable_success_error_envelope_for_local_modelserver_caller_no_external_repo_wiring";
    public const string ImprovementLoopRouteSchemaVersion =
        "design_conversion_model_server_improvement_loop_route_contract_v1";
    public const string ImprovementLoopRouteBoundary =
        "local_improvement_loop_route_contract_refs_only_no_public_host_no_external_caller";
    public const string StatusReadbackRouteSchemaVersion =
        "design_conversion_model_server_status_readback_route_contract_v1";
    public const string StatusReadbackRouteBoundary =
        "local_status_readback_route_contract_refs_only_no_public_host_no_external_caller";
    public const string DpoUsefulnessRouteSchemaVersion =
        "design_conversion_model_server_dpo_usefulness_route_contract_v1";
    public const string DpoUsefulnessRouteBoundary =
        "local_dpo_usefulness_route_contract_refs_only_no_public_host_no_external_caller";
    public const string TrainingIntakeRouteSchemaVersion =
        "design_conversion_model_server_training_intake_route_contract_v1";
    public const string TrainingIntakeRouteBoundary =
        "local_training_intake_route_contract_refs_only_no_public_host_no_external_caller_no_product_ready_claim";
    public const string DefaultSubmitBaseUrlMissingStatus = "bounded_non_ready_base_url_missing";
    public const string DefaultSubmitBaseUrlNextAction =
        "start_design_conversion_live_local_server_and_use_ready_file_baseUrl_or_call_in_process_registry_adapter";
    public const string Flow47VerificationAuthority = "host_boundary";
    public const string Flow47VerificationStateNotEvaluatedInLocalCaller = "not_evaluated_in_local_caller";

    public static DesignConversionModelServerCallerContractDescriptor Describe()
        => new(
            SchemaVersion: SchemaVersion,
            Boundary: Boundary,
            EndpointMethod: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            EndpointPath: DesignConversionModelServerEndpointRegistry.EndpointPath,
            RequestContentType: "application/json",
            ResponseContentType: "application/json; charset=utf-8",
            SupportedSourceKinds:
            [
                DesignConversionSourceKind.Figma.ToString(),
                DesignConversionSourceKind.Canva.ToString(),
                DesignConversionSourceKind.Other.ToString()
            ],
            RequestFields:
            [
                new("requestId", "string", true, "Stable caller-generated request id.", false, true),
                new("sourceKind", nameof(DesignConversionSourceKind), true, "Figma, Canva, or Other.", false, true),
                new("designTree", nameof(DesignConversionModelServerPayloadRef), true, "Generated design tree ref/hash or payload.", true, false),
                new("originalTree", nameof(DesignConversionModelServerPayloadRef), true, "Original source tree ref/hash or payload.", true, false),
                new("html", "string", true, "Generated HTML candidate.", true, false),
                new("css", "string", true, "Generated CSS candidate.", true, false),
                new("screenshots", "DesignConversionModelServerArtifactRef[]", false, "At least one screenshot or design render artifact ref is required.", false, false),
                new("designRenders", "DesignConversionModelServerArtifactRef[]", false, "At least one screenshot or design render artifact ref is required.", false, false),
                new("screenshots|designRenders", "DesignConversionModelServerArtifactRef[]", true, "Composite requirement: at least one screenshot or design render artifact ref is required.", false, false),
                new("budget", nameof(DesignConversionModelServerBudget), false, "Optional caller budget and tier limits; server defaults are local and conservative.", true, false),
                new("metadata", "object", false, "Optional caller metadata; private values are sanitized before response metadata.", true, false)
            ],
            ResponseFields:
            [
                new("endpoint", "string", true, "Endpoint path that handled the request."),
                new("method", "string", true, "HTTP method shape."),
                new("httpSeam", "string", true, "Local socketless HTTP seam boundary."),
                new("apiShape", "string", true, "In-process API shape identifier."),
                new("runtimeSurface", "string", true, "In-process runtime surface identifier."),
                new("statusCode", "number", true, "HTTP-like status code."),
                new("contractStatus", "string", false, "Runtime contract status on success."),
                new("jobId", "string", false, "Local runtime job id on success."),
                new("requestId", "string", false, "Caller request id on success."),
                new("runtimeStatus", nameof(DesignConversionThinClientStatusResult), false, "Runtime status object on success."),
                new("trainingReadiness", nameof(DesignConversionModelServerTrainingReadiness), false, "Training/upload/deployment readiness boundary object on success."),
                new("evidenceRefs", "string[]", false, "Evidence references and hashes only."),
                new("warnings", "string[]", false, "Boundary and readiness warnings."),
                new("boundaryMetadata", "object", true, "Boundary flags and counts; no raw HTML/CSS/design tree/private payloads.")
            ],
            ValidationFailures:
            [
                new("REQUEST_REQUIRED", 400, "Null local HTTP request."),
                new("REQUEST_BODY_REQUIRED", 400, "Missing or empty JSON request body."),
                new("REQUEST_JSON_INVALID", 400, "Invalid JSON body."),
                new("METHOD_NOT_ALLOWED", 405, "Endpoint invoked with a non-POST method."),
                new("UNSUPPORTED_MEDIA_TYPE", 415, "Content type is not application/json."),
                new("DESIGN_CONVERSION_SOURCE_TREE_REQUIRED", 400, "Missing designTree payload, ref, or hash."),
                new("DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED", 400, "Missing originalTree payload, ref, or hash."),
                new("DESIGN_CONVERSION_HTML_REQUIRED", 400, "Missing generated HTML."),
                new("DESIGN_CONVERSION_CSS_REQUIRED", 400, "Missing generated CSS."),
                new("DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED", 400, "Missing both screenshots and designRenders refs."),
                new("DESIGN_CONVERSION_VIEWPORT_REQUIRED", 400, "Artifact refs do not include a positive viewport."),
                new("DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED", 400, "Screenshot/render artifact refs are missing required refs-only proof data or use unsupported proof-ref syntax.")
            ],
            Examples:
            [
                new("figma-local-caller-fixture", DesignConversionSourceKind.Figma, "pass124-figma-caller-contract-fixture", "Figma-like request DTO that can be serialized directly to the endpoint JSON shape."),
                new("canva-local-caller-fixture", DesignConversionSourceKind.Canva, "pass124-canva-caller-contract-fixture", "Canva-like request DTO that can be serialized directly to the endpoint JSON shape.")
            ],
            BoundaryMetadata: BoundaryMetadata());

    public static DesignConversionModelServerLocalCallerResponseEnvelopeDescriptor DescribeLocalCallerResponseEnvelope()
        => new(
            SchemaVersion: LocalCallerResponseEnvelopeSchemaVersion,
            Boundary: LocalCallerResponseEnvelopeBoundary,
            EnvelopeKinds: ["success", "error"],
            Fields: LocalCallerResponseEnvelopeFields(),
            BoundaryMetadata: BoundaryMetadata());

    public static DesignConversionModelServerImprovementLoopContractDescriptor DescribeImprovementLoopRoute()
        => new(
            SchemaVersion: ImprovementLoopRouteSchemaVersion,
            Boundary: ImprovementLoopRouteBoundary,
            EndpointMethod: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            EndpointPath: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            RequestContentType: "application/json",
            ResponseContentType: "application/json; charset=utf-8",
            RequestFields: ImprovementLoopRequestFields(),
            ResponseFields: ImprovementLoopResponseFields(),
            LocalCallerEnvelopeFields: LocalCallerResponseEnvelopeFields(),
            ValidationFailures: ImprovementLoopValidationFailures(),
            BoundaryMetadata: ImprovementLoopBoundaryMetadata());

    public static DesignConversionModelServerStatusReadbackContractDescriptor DescribeStatusReadbackRoute()
        => new(
            SchemaVersion: StatusReadbackRouteSchemaVersion,
            Boundary: StatusReadbackRouteBoundary,
            EndpointMethod: DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointMethod,
            EndpointPath: DesignConversionModelServerStatusHandler.EndpointTemplatePath,
            RequestContentType: "none",
            ResponseContentType: "application/json; charset=utf-8",
            RequestFields: StatusReadbackRequestFields(),
            ResponseFields: StatusReadbackResponseFields(),
            LocalCallerEnvelopeFields: LocalCallerResponseEnvelopeFields(),
            ValidationFailures: StatusReadbackValidationFailures(),
            BoundaryMetadata: StatusReadbackBoundaryMetadata());

    public static DesignConversionModelServerDpoUsefulnessContractDescriptor DescribeDpoUsefulnessRoute()
        => new(
            SchemaVersion: DpoUsefulnessRouteSchemaVersion,
            Boundary: DpoUsefulnessRouteBoundary,
            EndpointMethod: DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointMethod,
            EndpointPath: DesignConversionModelServerDpoUsefulnessHandler.EndpointPath,
            RequestContentType: "none",
            ResponseContentType: "application/json; charset=utf-8",
            RequestFields: DpoUsefulnessRequestFields(),
            ResponseFields: DpoUsefulnessResponseFields(),
            LocalCallerEnvelopeFields: LocalCallerResponseEnvelopeFields(),
            ValidationFailures: DpoUsefulnessValidationFailures(),
            BoundaryMetadata: DpoUsefulnessBoundaryMetadata());

    public static DesignConversionModelServerTrainingIntakeContractDescriptor DescribeTrainingIntakeRoute()
        => new(
            SchemaVersion: TrainingIntakeRouteSchemaVersion,
            Boundary: TrainingIntakeRouteBoundary,
            EndpointMethod: DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod,
            EndpointPath: DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            RequestContentType: "application/json",
            ResponseContentType: "application/json; charset=utf-8",
            RequestFields: TrainingIntakeRequestFields(),
            ResponseFields: TrainingIntakeResponseFields(),
            LocalCallerEnvelopeFields: LocalCallerResponseEnvelopeFields(),
            ValidationFailures: TrainingIntakeValidationFailures(),
            SupportedActions: TrainingIntakeSupportedActions(),
            BoundaryMetadata: TrainingIntakeBoundaryMetadata());

    public static IReadOnlyList<DesignConversionModelServerCallerContractResponseField> LocalCallerResponseEnvelopeFields()
        =>
        [
            new("envelopeSchemaVersion", "string", true, "Stable envelope schema version for local caller success and error handling."),
            new("envelopeBoundary", "string", true, "Boundary label proving the envelope is local and not cross-repo wiring."),
            new("envelopeKind", "string", true, "success or error."),
            new("statusCode", "number", true, "HTTP-like status code from local validation, registry, handler, or response parsing."),
            new("registryInvokeSucceeded", "boolean", true, "True only when the endpoint registry returned a handler response."),
            new("runtimeInvoked", "boolean", true, "True only when the model-server runtime was invoked."),
            new("parsedResponseJsonSucceeded", "boolean", true, "True only when the response body parsed as JSON."),
            new("parsedContractStatus", "string", false, "Runtime contract status on success."),
            new("parsedJobId", "string", false, "Runtime job id on submit or status/readback success."),
            new("parsedErrorCode", "string", false, "Stable error code on local validation, registry, handler, or parse failure."),
            new("parsedErrorMessage", "string", false, "Stable error message on local validation, registry, handler, or parse failure."),
            new("boundaryMetadata", "object", true, "Boundary and evidence metadata; contains no raw HTML/CSS/design tree/private payload."),
            new("rawPrivateResponseLeakageDetected", "boolean", true, "True when metadata or response-body scanning detects raw private payload leakage."),
            new("externalCallerRepoTouched", "boolean", true, "Always false for this in-repo local adapter."),
            new("crossRepoCallerVerified", "boolean", true, "Always false until an explicitly authorized cross-repo pass verifies callers.")
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractField> ImprovementLoopRequestFields()
        =>
        [
            new("loopId", "string", true, "Stable local improvement-loop id.", false, true),
            new("initialPackage", nameof(DesignConversionModelServerLocalCallerPackage), true, "Initial local caller package; response stays refs-only and never echoes raw package payloads.", true, false),
            new("maxAttempts", "number", true, "Positive local attempt cap for the bounded improve/evaluate loop.", false, true),
            new("externalVisualProviderConfig", nameof(DesignConversionExternalVisualProviderConfigInput), false, "Optional request-scoped external visual provider config may contain private endpoint, credential, and artifact-root settings; route never echoes raw config values or secrets.", true, false),
            new("metadata", "object", false, "Optional caller metadata; private values are hashed into refs before response metadata.", true, false)
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractField> StatusReadbackRequestFields()
        =>
        [
            new("jobId", "route string", true, "Local thin-client job id returned by the submit route.", false, true)
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractField> DpoUsefulnessRequestFields()
        =>
        [
            new("includeBlocked", "query bool", false, "When true, include blocked or review-required rows in the local readback.", false, true),
            new("unitId", "query string", false, "Optional DPO unit id filter.", false, true),
            new("minimumUsefulnessScore", "query number", false, "Optional lower usefulness score filter.", false, true),
            new("maximumUsefulnessScore", "query number", false, "Optional upper usefulness score filter.", false, true),
            new("limit", "query integer", false, "Optional row limit for the local readback.", false, true)
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractField> TrainingIntakeRequestFields()
        =>
        [
            new("action", "string", true, "One of list, create, preflight, run, promote-local, or continue-training-upload-status.", false, true),
            new("intakeId", "string", false, "Required for preflight, run, promote-local, and continue-training-upload-status actions.", false, true),
            new("tenantId", "string", false, "Required for mutating create, run, promote-local, and continue-training-upload-status actions; response returns only a hash ref.", true, false),
            new("idempotencyKey", "string", false, "Required for mutating create, run, promote-local, and continue-training-upload-status actions; response returns only a hash ref.", true, false),
            new("handoff", nameof(DesignConversionApprovedDpoHandoffArtifact), false, "Required only for create; already-sanitized approved handoff artifact from the domain API.", true, false),
            new("requestedByRef", "string", false, "Required only for create; stable requester reference.", true, false),
            new("reason", "string", false, "Required for create and continue-training-upload-status; response returns only a reason ref.", true, false),
            new("requestedAtUtc", "DateTimeOffset", false, "Optional create timestamp.", false, true),
            new("promotionOutputDirectory", "string", false, "Required only for promote-local and continue-training-upload-status; response returns only a hash ref, never the path.", true, false),
            new("options", nameof(DesignConversionTrainingPipelineOptions), false, "Optional local training pipeline options for run, promote-local, or continue-training-upload-status.", true, false),
            new("metadata", "object", false, "Optional caller metadata; private values must not be echoed raw.", true, false)
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractResponseField> ImprovementLoopResponseFields()
        =>
        [
            new("endpoint", "string", true, "Improvement-loop endpoint path that handled the request."),
            new("method", "string", true, "HTTP method shape."),
            new("httpSeam", "string", true, "Local socketless HTTP seam boundary."),
            new("apiShape", "string", true, "In-process API shape identifier."),
            new("runtimeSurface", "string", true, "In-process runtime surface identifier."),
            new("statusCode", "number", true, "HTTP-like status code."),
            new("contractStatus", "string", true, "Completed or bounded non-ready route contract status."),
            new("loopId", "string", true, "Local loop id from the accepted request."),
            new("requestId", "string", true, "Initial package request id."),
            new("stopReason", "string", true, "Why the bounded loop stopped."),
            new("completed", "boolean", true, "True only when the local loop reached its completion condition."),
            new("qualityReached", "boolean", true, "True only when the local quality threshold was reached."),
            new("providerRequiredOrBlocked", "boolean", true, "True when the loop stopped because an external provider would be required or blocked."),
            new("renderManifestIncomplete", "boolean", true, "True when refs needed for render/visual evidence are incomplete."),
            new("budgetCapReached", "boolean", true, "True when the local budget cap stopped the loop."),
            new("maxAttemptsReached", "boolean", true, "True when maxAttempts stopped the loop."),
            new("cancelled", "boolean", true, "True when cancellation stopped the loop."),
            new("candidateStrategyFailed", "boolean", true, "True when local candidate generation failed."),
            new("attemptCount", "number", true, "Number of loop attempts returned."),
            new("attempts", "DesignConversionModelServerImprovementLoopAttemptResponse[]", true, "Per-attempt refs-only telemetry."),
            new("evidenceRefs", "string[]", true, "Evidence references and hashes only."),
            new("warnings", "string[]", true, "Boundary and non-ready warnings."),
            new("boundaryMetadata", "object", true, "Route and refs-only flags; no raw HTML/CSS/design tree/private payloads."),
            new("boundaryMetadata.refs_or_hashes_only", "boolean", true, "Always true for this response contract."),
            new("boundaryMetadata.raw_html_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.raw_css_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.raw_design_tree_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.response_contains_raw_private_data", "boolean", true, "Always false for this response contract.")
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractResponseField> StatusReadbackResponseFields()
        =>
        [
            new("endpoint", "string", true, "Status/readback endpoint template that handled the request."),
            new("method", "string", true, "HTTP method shape."),
            new("httpSeam", "string", true, "Local socketless HTTP seam boundary."),
            new("apiShape", "string", true, "In-process API shape identifier."),
            new("runtimeSurface", "string", true, "In-process runtime surface identifier."),
            new("statusCode", "number", true, "HTTP-like status code."),
            new("jobId", "string", true, "Local runtime job id on success."),
            new("requestId", "string", true, "Caller request id on success."),
            new("contractStatus", "string", true, "Runtime contract status on success."),
            new("jobStatus", "string", true, "Stored thin-client job status."),
            new("statusUri", "string", true, "Local memory status URI reference."),
            new("artifacts", "DesignConversionModelServerStatusArtifactRef[]", true, "Artifact refs and hashes only."),
            new("evidenceRefs", "string[]", true, "Evidence references and hashes only."),
            new("warnings", "string[]", true, "Boundary and non-ready warnings."),
            new("boundaryMetadata", "object", true, "Route and refs-only flags; no raw HTML/CSS/design tree/private payloads."),
            new("boundaryMetadata.status_readback_response_refs_only", "boolean", true, "Always true for this response contract."),
            new("boundaryMetadata.job_rerun_invoked", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.raw_html_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.raw_css_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.raw_design_tree_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.response_contains_raw_private_data", "boolean", true, "Always false for this response contract.")
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractResponseField> DpoUsefulnessResponseFields()
        =>
        [
            new("endpoint", "string", true, "DPO usefulness endpoint that handled the request."),
            new("method", "string", true, "HTTP method shape."),
            new("httpSeam", "string", true, "Local socketless HTTP seam boundary."),
            new("apiShape", "string", true, "ModelServer route shape identifier."),
            new("runtimeSurface", "string", true, "ModelServer DPO usefulness route runtime surface identifier."),
            new("statusCode", "number", true, "HTTP-like status code."),
            new("contractStatus", "string", true, "DPO usefulness readback completion status."),
            new("dpoUsefulnessApiShape", "string", true, "Delegated domain DPO usefulness API shape."),
            new("rows", "DesignConversionDpoUsefulnessApiRow[]", true, "Sanitized DPO usefulness rows for user-facing readback."),
            new("rows[].retrainDecisionStatus", "string", true, "User/architect retrain-decision status derived from usefulness, review, approval, and blockers."),
            new("rows[].retrainDecisionEvidenceRefs", "string[]", true, "Refs-only evidence tokens for deciding whether the DPO row can enter retraining."),
            new("rows[].usefulnessMetrics", nameof(DesignConversionDpoUsefulnessMetrics), true, "Dashboard-friendly usefulness metrics without parsing factors or raw payloads."),
            new("rows[].usefulnessMetrics.score", "number", true, "Display usefulness score for ranking and filtering."),
            new("rows[].usefulnessMetrics.acceptanceReason", "string", true, "Approval reason or retrain decision status that explains acceptance/display state."),
            new("rows[].usefulnessMetrics.qualityGain", "number", true, "Chosen-minus-rejected visual quality gain exposed as a metric."),
            new("rows[].usefulnessMetrics.negativePairUsefulness", "string", true, "Negative-pair taxonomy/status used for usefulness display."),
            new("rows[].usefulnessMetrics.sourceCoverage", "string", true, "Refs-only source coverage summary for source and heldout/disjoint boundaries."),
            new("rows[].usefulnessMetrics.sourceDisjointStatus", "string", true, "Source-disjoint readiness status for training usefulness display."),
            new("rows[].usefulnessMetrics.heldOutEvaluationStatus", "string", true, "Held-out/evaluation readiness status for training usefulness display."),
            new("rows[].usefulnessMetrics.promotionEligibility", "string", true, "Conservative local promotion eligibility note without ProductReady claims."),
            new("rows[].usefulnessMetrics.policyCheckpointEvidenceRefs", "string[]", true, "Refs-only policy checkpoint evidence indicators, or a not-present marker."),
            new("rows[].usefulnessMetrics.productReady", "boolean", true, "Always false for this local usefulness display boundary."),
            new("rowCount", "number", true, "Count of returned rows after filters."),
            new("trainReadyCount", "number", true, "Count of train-ready rows after filters."),
            new("heldOutCount", "number", true, "Count of held-out rows after filters."),
            new("providerMissingCount", "number", true, "Count of rows blocked by missing provider evidence."),
            new("blockedCount", "number", true, "Count of rows not train-ready or held-out after filters."),
            new("averageUsefulnessScore", "number|null", true, "Average usefulness score across returned rows, null when no rows are returned."),
            new("minimumUsefulnessScore", "number|null", true, "Minimum usefulness score across returned rows, null when no rows are returned."),
            new("maximumUsefulnessScore", "number|null", true, "Maximum usefulness score across returned rows, null when no rows are returned."),
            new("warnings", "string[]", true, "Boundary and non-ready warnings."),
            new("boundaryMetadata", "object", true, "Route and refs-only flags; no raw HTML/CSS/design tree/private payloads."),
            new("boundaryMetadata.dpo_usefulness_response_refs_only", "boolean", true, "Always true for this response contract."),
            new("boundaryMetadata.delegates_to_domain_api", "string", true, "Names the domain API that owns DPO usefulness logic."),
            new("boundaryMetadata.dpo_usefulness_row_count", "number", true, "Mirrors rowCount for metadata-only readback consumers."),
            new("boundaryMetadata.dpo_usefulness_average_usefulness_score", "number|null", true, "Mirrors averageUsefulnessScore for metadata-only readback consumers."),
            new("boundaryMetadata.product_ready", "boolean", true, "Always false for this local server readback boundary."),
            new("boundaryMetadata.raw_html_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.raw_css_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.raw_design_tree_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.response_contains_raw_private_data", "boolean", true, "Always false for this response contract.")
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractResponseField> TrainingIntakeResponseFields()
        =>
        [
            new("endpoint", "string", true, "Training-intake endpoint that handled the request."),
            new("method", "string", true, "HTTP method shape."),
            new("httpSeam", "string", true, "Local socketless HTTP seam boundary unless ASP.NET loopback test annotates a local host."),
            new("apiShape", "string", true, "ModelServer route shape identifier."),
            new("runtimeSurface", "string", true, "ModelServer training-intake route runtime surface identifier."),
            new("statusCode", "number", true, "HTTP-like status code."),
            new("contractStatus", "string", true, "training-intake-action-completed on success."),
            new("action", "string", true, "Normalized action that was executed."),
            new("intakeId", "string", false, "Current intake id for single-intake actions."),
            new("intakeCount", "number", true, "Count of intake refs returned by the list/create action."),
            new("intakes", "DesignConversionModelServerTrainingIntakeRecordRef[]", true, "Refs-only intake summaries."),
            new("intake", "DesignConversionModelServerTrainingIntakeRecordRef", false, "Refs-only created intake summary."),
            new("preflight", "DesignConversionModelServerTrainingIntakePreflightRef", false, "Refs-only preflight summary."),
            new("trainingRun", "DesignConversionModelServerTrainingIntakeRunRef", false, "Refs-only local training run summary."),
            new("localPromotion", "DesignConversionModelServerTrainingIntakeLocalPromotionRef", false, "Refs-only local promotion summary; paths are hashed refs."),
            new("lifecycleStatus", "DesignConversionModelServerTrainingIntakeLifecycleStatusRef", false, "User/architect continue-training upload/deploy status; local-only and ProductReady=false."),
            new("lifecycleStatus.promotedCheckpointId", "string", false, "Promoted checkpoint id derived from checkpoint content hash when local promotion succeeds."),
            new("lifecycleStatus.promotedCheckpointPathRef", "string", false, "Hash ref for the promoted checkpoint path; raw local paths are not returned."),
            new("lifecycleStatus.promotedRuntimeModelId", "string", false, "Runtime-facing local promoted model id when a local checkpoint is loaded."),
            new("lifecycleStatus.uploadStatus", "string", false, "Upload/deploy status remains blocked/local-only unless real upload/deploy proof exists."),
            new("lifecycleStatus.cloudUploadStatus", "string", false, "Explicit cloud upload status; local continue-training reports not_uploaded without claiming ProductReady."),
            new("lifecycleStatus.localPublishPackageRef", "string", false, "Refs-only local publish package/upload receipt reference; raw package payloads and paths are not returned."),
            new("lifecycleStatus.localPublishManifestRef", "string", false, "Refs-only local publish manifest reference for the promoted checkpoint package."),
            new("lifecycleStatus.localPublishBackupRef", "string", false, "Refs-only backup/prior-active reference, or none when no prior local promotion exists."),
            new("lifecycleStatus.rollbackBoundaryRef", "string", false, "Refs-only rollback boundary metadata for the local promotion lifecycle."),
            new("lifecycleStatus.continuedFromCheckpointRef", "string", false, "Refs-only source checkpoint reference for continue-training when available."),
            new("productReady", "boolean", true, "Always false for this local-only route."),
            new("deploymentVerified", "boolean", true, "Always false unless a separate deployment proof exists; this route does not provide one."),
            new("publicHttpEndpointVerified", "boolean", true, "Always false for registry/local caller; ASP.NET tests remain loopback only."),
            new("warnings", "string[]", true, "Boundary and non-ready warnings."),
            new("boundaryMetadata", "object", true, "Route and refs-only flags; no raw HTML/CSS/design tree/private payloads."),
            new("boundaryMetadata.training_intake_route_local_only", "boolean", true, "Always true for this response contract."),
            new("boundaryMetadata.training_intake_admission_boundary_satisfied", "boolean", true, "True when a mutating training-intake action supplied tenantId and idempotencyKey, or when the action is read/preflight-only."),
            new("boundaryMetadata.training_intake_tenant_id_ref", "string", false, "Hash ref for the tenant id supplied to mutating actions; the raw tenant id is not echoed."),
            new("boundaryMetadata.training_intake_idempotency_key_ref", "string", false, "Hash ref for the idempotency key supplied to mutating actions; the raw key is not echoed."),
            new("boundaryMetadata.delegates_to_domain_api", "string", true, "Names the domain API that owns training-intake logic."),
            new("boundaryMetadata.product_ready", "boolean", true, "Always false for this local server route."),
            new("boundaryMetadata.private_file_paths_included", "boolean", true, "Always false; local paths are represented by hash refs."),
            new("boundaryMetadata.raw_html_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.raw_css_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.raw_design_tree_included", "boolean", true, "Always false for this response contract."),
            new("boundaryMetadata.response_contains_raw_private_data", "boolean", true, "Always false for this response contract.")
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractValidationFailure> ImprovementLoopValidationFailures()
        =>
        [
            new("REQUEST_REQUIRED", 400, "Null local HTTP request."),
            new("REQUEST_BODY_REQUIRED", 400, "Missing or empty JSON request body."),
            new("REQUEST_JSON_INVALID", 400, "Invalid JSON body."),
            new("METHOD_NOT_ALLOWED", 405, "Endpoint invoked with a non-POST method."),
            new("UNSUPPORTED_MEDIA_TYPE", 415, "Content type is not application/json."),
            new("ROUTE_NOT_FOUND", 404, "Improvement-loop route path is not registered on the handler."),
            new(DesignConversionModelServerImprovementLoopHandler.CoordinatorNotConfiguredErrorCode, 503, "Coordinator not configured for the optional local route."),
            new(DesignConversionModelServerImprovementLoopHandler.ExternalVisualProviderCoordinatorResolverNotConfiguredErrorCode, 503, "External visual provider coordinator resolver is not configured."),
            new(DesignConversionModelServerImprovementLoopHandler.ExternalVisualProviderConfigInvalidErrorCode, 400, "External visual provider config is invalid."),
            new(DesignConversionModelServerImprovementLoopHandler.ExternalVisualProviderCoordinatorResolveFailedErrorCode, 503, "External visual provider coordinator resolve failed."),
            new("IMPROVEMENT_LOOP_REQUEST_REQUIRED", 400, "JSON request body deserialized to null."),
            new("IMPROVEMENT_LOOP_ID_REQUIRED", 400, "Missing loopId."),
            new("IMPROVEMENT_LOOP_MAX_ATTEMPTS_REQUIRED", 400, "Missing or non-positive maxAttempts."),
            new(
                DesignConversionModelServerImprovementLoopHandler.MaxAttemptsExceededErrorCode,
                400,
                $"maxAttempts exceeds the supported maximum of {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts}."),
            new("IMPROVEMENT_LOOP_INITIAL_PACKAGE_REQUIRED", 400, "Missing initialPackage."),
            new("REQUEST_ID_REQUIRED", 400, "Missing initialPackage.requestId."),
            new("SOURCE_KIND_UNSUPPORTED", 400, "Unsupported initialPackage.sourceKind."),
            new("DESIGN_CONVERSION_SOURCE_TREE_REQUIRED", 400, "Missing initialPackage.designTree payload, ref, or hash."),
            new("DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED", 400, "Missing initialPackage.originalTree payload, ref, or hash."),
            new("DESIGN_CONVERSION_HTML_REQUIRED", 400, "Missing initialPackage.html."),
            new("DESIGN_CONVERSION_CSS_REQUIRED", 400, "Missing initialPackage.css."),
            new("DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED", 400, "Missing both initialPackage.screenshots and initialPackage.designRenders refs."),
            new("DESIGN_CONVERSION_VIEWPORT_REQUIRED", 400, "Artifact refs do not include a positive viewport."),
            new(DesignConversionInternalService.NegativeBudgetUsdErrorCode, 400, "Negative initialPackage.budget.budgetUsd."),
            new(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, 400, "Negative initialPackage.budget.maxImprovementCandidates, or zero maxImprovementCandidates without a server-owned entitlement tier."),
            new("DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED", 400, "Screenshot/render artifact refs are missing required refs-only proof data or use unsupported proof-ref syntax.")
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractValidationFailure> StatusReadbackValidationFailures()
        =>
        [
            new("REQUEST_REQUIRED", 400, "Null local HTTP request."),
            new("METHOD_NOT_ALLOWED", 405, "Endpoint invoked with a non-GET method."),
            new("ROUTE_NOT_FOUND", 404, "Status/readback route path is not registered on the handler."),
            new("JOB_ID_REQUIRED", 400, "Missing job id route segment."),
            new("JOB_NOT_FOUND", 404, "Unknown local runtime job id."),
            new("DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND", 404, "Status/readback route is not registered in the local endpoint registry.")
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractValidationFailure> DpoUsefulnessValidationFailures()
        =>
        [
            new("REQUEST_REQUIRED", 400, "Null local HTTP request."),
            new("METHOD_NOT_ALLOWED", 405, "Endpoint invoked with a non-GET method."),
            new("ROUTE_NOT_FOUND", 404, "DPO usefulness route path is not registered on the handler."),
            new("DPO_USEFULNESS_QUERY_INVALID", 400, "DPO usefulness query parameter could not be parsed."),
            new("DPO_USEFULNESS_QUERY_FAILED", 400, "Delegated domain DPO usefulness API rejected the request."),
            new("DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND", 404, "DPO usefulness route is not registered in the local endpoint registry.")
        ];

    public static IReadOnlyList<DesignConversionModelServerCallerContractValidationFailure> TrainingIntakeValidationFailures()
        =>
        [
            new("REQUEST_REQUIRED", 400, "Null local HTTP request."),
            new("REQUEST_BODY_REQUIRED", 400, "Missing or empty JSON request body."),
            new("REQUEST_JSON_INVALID", 400, "Invalid JSON body."),
            new("METHOD_NOT_ALLOWED", 405, "Endpoint invoked with a non-POST method."),
            new("UNSUPPORTED_MEDIA_TYPE", 415, "Content type is not application/json."),
            new("ROUTE_NOT_FOUND", 404, "Training-intake route path is not registered on the handler."),
            new("TRAINING_INTAKE_ACTION_REQUIRED", 400, "Missing action."),
            new("TRAINING_INTAKE_ACTION_UNSUPPORTED", 400, "Action is not one of the documented local actions."),
            new("TRAINING_INTAKE_ID_REQUIRED", 400, "Missing intakeId for preflight, run, or promote-local."),
            new("TRAINING_INTAKE_TENANT_ID_REQUIRED", 400, "Missing tenantId for a mutating training-intake action."),
            new("TRAINING_INTAKE_IDEMPOTENCY_KEY_REQUIRED", 400, "Missing idempotencyKey for a mutating training-intake action."),
            new("TRAINING_INTAKE_HANDOFF_REQUIRED", 400, "Missing handoff for create."),
            new("TRAINING_INTAKE_REQUESTED_BY_REQUIRED", 400, "Missing requestedByRef for create."),
            new("TRAINING_INTAKE_REASON_REQUIRED", 400, "Missing reason for create."),
            new("TRAINING_INTAKE_PROMOTION_OUTPUT_DIRECTORY_REQUIRED", 400, "Missing promotionOutputDirectory for promote-local."),
            new("TRAINING_INTAKE_LIFECYCLE_REQUESTED_BY_REQUIRED", 400, "Missing requestedByRef for continue-training-upload-status."),
            new("TRAINING_INTAKE_LIFECYCLE_REASON_REQUIRED", 400, "Missing reason for continue-training-upload-status."),
            new("TRAINING_INTAKE_DOMAIN_API_FAILED", 400, "Delegated domain DPO usefulness API rejected the action."),
            new("DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND", 404, "Training-intake route is not registered in the local endpoint registry.")
        ];

    public static DesignConversionModelServerApiRequest CreateFigmaExampleRequest(
        string requestId = "pass124-figma-caller-contract-fixture")
        => CreateExampleRequest(
            requestId,
            DesignConversionSourceKind.Figma,
            "figma-json",
            "figma-tree-ref",
            "artifact://pass124/figma/source-screenshot.png",
            "artifact://pass124/figma/design-render.png");

    public static DesignConversionModelServerApiRequest CreateCanvaExampleRequest(
        string requestId = "pass124-canva-caller-contract-fixture")
        => CreateExampleRequest(
            requestId,
            DesignConversionSourceKind.Canva,
            "canva-json",
            "canva-tree-ref",
            "artifact://pass124/canva/source-screenshot.png",
            "artifact://pass124/canva/design-render.png");

    public static IReadOnlyDictionary<string, string> BoundaryMetadata()
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["caller_contract_schema_version"] = SchemaVersion,
            ["caller_contract_boundary"] = Boundary,
            ["caller_contract_scope"] = "local_modelserver_registry_and_http_seam_only",
            ["local_caller_response_envelope_schema_version"] = LocalCallerResponseEnvelopeSchemaVersion,
            ["local_caller_response_envelope_boundary"] = LocalCallerResponseEnvelopeBoundary,
            ["local_caller_success_error_envelope_stable"] = bool.TrueString,
            ["local_caller_validation_error_envelope_supported"] = bool.TrueString,
            ["local_caller_registry_error_envelope_supported"] = bool.TrueString,
            ["local_caller_parse_error_envelope_supported"] = bool.TrueString,
            ["local_caller_parse_error_code"] = "ADAPTER_RESPONSE_JSON_PARSE_FAILED",
            ["default_submit_endpoint_route"] = DesignConversionModelServerEndpointRegistry.EndpointRoute,
            ["default_submit_endpoint_method"] = DesignConversionModelServerEndpointRegistry.EndpointMethod,
            ["default_submit_endpoint_path"] = DesignConversionModelServerEndpointRegistry.EndpointPath,
            ["default_submit_base_url_configured"] = bool.FalseString,
            ["default_submit_base_url_missing"] = bool.TrueString,
            ["default_submit_base_url_status"] = DefaultSubmitBaseUrlMissingStatus,
            ["default_submit_base_url_missing_setting"] = "BaseUrl",
            ["default_submit_base_url_next_action"] = DefaultSubmitBaseUrlNextAction,
            ["local_in_process_submit_route_available"] = bool.TrueString,
            ["local_in_process_submit_route_kind"] = "socketless_modelserver_endpoint_registry",
            ["flow47_verification_authority"] = Flow47VerificationAuthority,
            ["flow47_verification_state"] = Flow47VerificationStateNotEvaluatedInLocalCaller,
            ["flow47_submit_status"] = DefaultSubmitBaseUrlMissingStatus,
            ["flow47_missing_setting_names"] = "BaseUrl",
            ["flow47_next_action"] = DefaultSubmitBaseUrlNextAction,
            ["figma_like_caller_contract_documented"] = bool.TrueString,
            ["canva_like_caller_contract_documented"] = bool.TrueString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_caller_repo_touched"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["response_contains_raw_html"] = bool.FalseString,
            ["response_contains_raw_css"] = bool.FalseString,
            ["response_contains_raw_design_tree"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["required_artifact_policy"] = "at_least_one_screenshot_or_design_render_ref_with_positive_viewport"
        };

    public static IReadOnlyDictionary<string, string> ImprovementLoopBoundaryMetadata()
    {
        var metadata = new Dictionary<string, string>(BoundaryMetadata(), StringComparer.Ordinal)
        {
            ["improvement_loop_contract_schema_version"] = ImprovementLoopRouteSchemaVersion,
            ["improvement_loop_contract_boundary"] = ImprovementLoopRouteBoundary,
            ["improvement_loop_endpoint_route"] = DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointRoute,
            ["improvement_loop_endpoint_method"] = DesignConversionModelServerEndpointRegistry.EndpointMethod,
            ["improvement_loop_endpoint_path"] = DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            ["improvement_loop_required_fields"] = string.Join(",", ImprovementLoopRequiredRequestFieldNames()),
            ["improvement_loop_local_caller_envelope_schema_version"] = LocalCallerResponseEnvelopeSchemaVersion,
            ["improvement_loop_local_caller_envelope_boundary"] = LocalCallerResponseEnvelopeBoundary,
            ["improvement_loop_route_discoverable_by_contract"] = bool.TrueString,
            ["improvement_loop_route_local_only"] = bool.TrueString,
            ["improvement_loop_response_refs_only"] = bool.TrueString,
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_caller_repo_touched"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        return metadata;
    }

    public static IReadOnlyDictionary<string, string> StatusReadbackBoundaryMetadata()
    {
        var metadata = new Dictionary<string, string>(BoundaryMetadata(), StringComparer.Ordinal)
        {
            ["status_readback_contract_schema_version"] = StatusReadbackRouteSchemaVersion,
            ["status_readback_contract_boundary"] = StatusReadbackRouteBoundary,
            ["status_readback_endpoint_route"] = DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointRoute,
            ["status_readback_endpoint_method"] = DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointMethod,
            ["status_readback_endpoint_path"] = DesignConversionModelServerStatusHandler.EndpointTemplatePath,
            ["status_readback_required_fields"] = string.Join(",", StatusReadbackRequiredRequestFieldNames()),
            ["status_readback_local_caller_envelope_schema_version"] = LocalCallerResponseEnvelopeSchemaVersion,
            ["status_readback_local_caller_envelope_boundary"] = LocalCallerResponseEnvelopeBoundary,
            ["status_readback_route_discoverable_by_contract"] = bool.TrueString,
            ["status_readback_route_local_only"] = bool.TrueString,
            ["status_readback_response_refs_only"] = bool.TrueString,
            ["job_rerun_invoked"] = bool.FalseString,
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["platform_integration_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_caller_repo_touched"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        return metadata;
    }

    public static IReadOnlyDictionary<string, string> DpoUsefulnessBoundaryMetadata()
    {
        var metadata = new Dictionary<string, string>(BoundaryMetadata(), StringComparer.Ordinal)
        {
            ["dpo_usefulness_contract_schema_version"] = DpoUsefulnessRouteSchemaVersion,
            ["dpo_usefulness_contract_boundary"] = DpoUsefulnessRouteBoundary,
            ["dpo_usefulness_endpoint_route"] = DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointRoute,
            ["dpo_usefulness_endpoint_method"] = DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointMethod,
            ["dpo_usefulness_endpoint_path"] = DesignConversionModelServerDpoUsefulnessHandler.EndpointPath,
            ["dpo_usefulness_optional_query_fields"] = string.Join(",", DpoUsefulnessOptionalQueryFieldNames()),
            ["dpo_usefulness_local_caller_envelope_schema_version"] = LocalCallerResponseEnvelopeSchemaVersion,
            ["dpo_usefulness_local_caller_envelope_boundary"] = LocalCallerResponseEnvelopeBoundary,
            ["dpo_usefulness_route_discoverable_by_contract"] = bool.TrueString,
            ["dpo_usefulness_route_local_only"] = bool.TrueString,
            ["dpo_usefulness_response_refs_only"] = bool.TrueString,
            ["delegates_to_domain_api"] = nameof(DesignConversionDpoUsefulnessApi),
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["platform_integration_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_caller_repo_touched"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        return metadata;
    }

    public static IReadOnlyDictionary<string, string> TrainingIntakeBoundaryMetadata()
    {
        var metadata = new Dictionary<string, string>(BoundaryMetadata(), StringComparer.Ordinal)
        {
            ["training_intake_contract_schema_version"] = TrainingIntakeRouteSchemaVersion,
            ["training_intake_contract_boundary"] = TrainingIntakeRouteBoundary,
            ["training_intake_endpoint_route"] = DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointRoute,
            ["training_intake_endpoint_method"] = DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod,
            ["training_intake_endpoint_path"] = DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            ["training_intake_required_fields"] = string.Join(",", TrainingIntakeRequiredRequestFieldNames()),
            ["training_intake_supported_actions"] = string.Join(",", TrainingIntakeSupportedActions()),
            ["training_intake_local_caller_envelope_schema_version"] = LocalCallerResponseEnvelopeSchemaVersion,
            ["training_intake_local_caller_envelope_boundary"] = LocalCallerResponseEnvelopeBoundary,
            ["training_intake_route_discoverable_by_contract"] = bool.TrueString,
            ["training_intake_route_local_only"] = bool.TrueString,
            ["training_intake_response_refs_only"] = bool.TrueString,
            ["training_intake_create_supported"] = bool.TrueString,
            ["training_intake_preflight_supported"] = bool.TrueString,
            ["training_intake_run_supported"] = bool.TrueString,
            ["training_intake_local_promotion_supported"] = bool.TrueString,
            ["training_intake_continue_training_upload_status_supported"] = bool.TrueString,
            ["training_intake_continue_training_upload_status_local_only"] = bool.TrueString,
            ["training_intake_mutating_admission_boundary"] = "tenant_id_and_idempotency_key_v1",
            ["training_intake_tenant_id_required_for_mutations"] = bool.TrueString,
            ["training_intake_idempotency_key_required_for_mutations"] = bool.TrueString,
            ["training_intake_auth_token_verified"] = bool.FalseString,
            ["training_intake_replay_cache_checked"] = bool.FalseString,
            ["training_intake_full_tenant_isolation_verified"] = bool.FalseString,
            ["training_intake_paths_returned_as_hash_refs"] = bool.TrueString,
            ["delegates_to_domain_api"] = nameof(DesignConversionDpoUsefulnessApi),
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["private_file_paths_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["platform_integration_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_caller_repo_touched"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        return metadata;
    }

    public static IReadOnlyList<string> RequiredRequestFieldNames()
        => RequiredRequestFieldNames(Describe().RequestFields);

    private static IReadOnlyList<string> RequiredRequestFieldNames(
        IReadOnlyList<DesignConversionModelServerCallerContractField> requestFields)
        => requestFields
            .Where(field => field.Required)
            .Select(field => field.JsonName)
            .ToArray();

    public static IReadOnlyList<string> ImprovementLoopRequiredRequestFieldNames()
        => RequiredRequestFieldNames(ImprovementLoopRequestFields());

    public static IReadOnlyList<string> StatusReadbackRequiredRequestFieldNames()
        => RequiredRequestFieldNames(StatusReadbackRequestFields());

    public static IReadOnlyList<string> DpoUsefulnessOptionalQueryFieldNames()
        =>
        [
            "includeBlocked",
            "unitId",
            "minimumUsefulnessScore",
            "maximumUsefulnessScore",
            "limit"
        ];

    public static IReadOnlyList<string> TrainingIntakeRequiredRequestFieldNames()
        =>
        [
            "action"
        ];

    public static IReadOnlyList<string> TrainingIntakeSupportedActions()
        =>
        [
            DesignConversionModelServerTrainingIntakeHandler.ActionList,
            DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
            DesignConversionModelServerTrainingIntakeHandler.ActionPreflight,
            DesignConversionModelServerTrainingIntakeHandler.ActionRun,
            DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
            DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus
        ];

    private static DesignConversionModelServerApiRequest CreateExampleRequest(
        string requestId,
        DesignConversionSourceKind sourceKind,
        string sourceFormat,
        string treeRef,
        string screenshotUri,
        string designRenderUri)
        => new(
            RequestId: requestId,
            SourceKind: sourceKind,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: treeRef,
                Kind: sourceFormat,
                Sha256: $"sha256:{treeRef}",
                Payload: "{\"type\":\"FRAME\",\"name\":\"contract fixture\"}",
                Format: sourceFormat),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"original-{treeRef}",
                Kind: sourceFormat,
                Sha256: $"sha256:original-{treeRef}",
                Payload: "{\"type\":\"FRAME\",\"name\":\"original contract fixture\"}",
                Format: sourceFormat),
            Html: "<main class=\"screen\"><h1>Contract fixture</h1></main>",
            Css: ".screen{display:grid;gap:16px;}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:source-screenshot",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: screenshotUri,
                    Sha256: $"sha256:{requestId}:source-screenshot")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:design-render",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: designRenderUri,
                    Sha256: $"sha256:{requestId}:design-render")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "local-caller-redacted",
                Tier: "local-contract",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.50m),
            Metadata: new Dictionary<string, string>
            {
                ["caller_contract_schema_version"] = SchemaVersion,
                ["caller_contract_fixture"] = sourceKind.ToString()
            });
}
