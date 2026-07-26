// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Net.Http;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerEndpointRegistration(
    string Method,
    string Path,
    string RequestContentType,
    string ResponseContentType,
    bool AspNetHostVerified,
    bool SocketOpened,
    bool PublicHttpEndpointVerified,
    bool CrossRepoCallerVerified,
    Func<DesignConversionModelServerHttpRequest, CancellationToken, Task<DesignConversionModelServerHttpResponse>> Handler,
    IReadOnlyDictionary<string, string> BoundaryMetadata,
    DesignConversionModelServerRouteAdmission? RouteAdmission = null);

public sealed class DesignConversionModelServerEndpointRegistry
{
    public const string EndpointMethod = "POST";
    public const string EndpointPath = DesignConversionModelServerHttpHandler.EndpointPath;
    public const string EndpointRoute = EndpointMethod + " " + EndpointPath;
    public const string InternalEndpointPath = DesignConversionModelServerHttpHandler.InternalEndpointPath;
    public const string InternalEndpointRoute = EndpointMethod + " " + InternalEndpointPath;
    public const string EndpointCancelledErrorCode = "CANCELLED";
    public const string EndpointCancelledErrorMessage = "DesignConversion endpoint invocation was cancelled.";
    public const string EndpointUnhandledErrorCode = "DESIGN_CONVERSION_ENDPOINT_UNHANDLED";
    public const string EndpointUnhandledErrorMessage =
        "DesignConversion endpoint invocation failed with an internal error.";
    public const string ImprovementLoopEndpointPath = DesignConversionModelServerImprovementLoopHandler.EndpointPath;
    public const string ImprovementLoopEndpointRoute = EndpointMethod + " " + ImprovementLoopEndpointPath;
    public const string StatusReadbackEndpointMethod = "GET";
    public const string StatusReadbackEndpointPath = DesignConversionModelServerStatusHandler.EndpointTemplatePath;
    public const string StatusReadbackEndpointRoute = StatusReadbackEndpointMethod + " " + StatusReadbackEndpointPath;
    public const string VisualCostReadbackEndpointMethod = "GET";
    public const string VisualCostReadbackEndpointPath = DesignConversionModelServerVisualCostHandler.EndpointTemplatePath;
    public const string VisualCostReadbackEndpointRoute = VisualCostReadbackEndpointMethod + " " + VisualCostReadbackEndpointPath;
    public const string DpoUsefulnessEndpointMethod = "GET";
    public const string DpoUsefulnessEndpointPath = DesignConversionModelServerDpoUsefulnessHandler.EndpointPath;
    public const string DpoUsefulnessEndpointRoute = DpoUsefulnessEndpointMethod + " " + DpoUsefulnessEndpointPath;
    public const string TrainingIntakeEndpointMethod = "POST";
    public const string TrainingIntakeEndpointPath = DesignConversionModelServerTrainingIntakeHandler.EndpointPath;
    public const string TrainingIntakeEndpointRoute = TrainingIntakeEndpointMethod + " " + TrainingIntakeEndpointPath;
    public const string Flow38QuarantineEndpointMethod = DesignConversionModelServerFlow38QuarantineHandler.EndpointMethod;
    public const string Flow38QuarantineEndpointPath = DesignConversionModelServerFlow38QuarantineHandler.EndpointPath;
    public const string Flow38QuarantineEndpointRoute = Flow38QuarantineEndpointMethod + " " + Flow38QuarantineEndpointPath;
    public const string ModelActivationEndpointMethod = "POST";
    public const string ModelActivationEndpointPath = DesignConversionModelServerModelActivationHandler.EndpointPath;
    public const string ModelActivationEndpointRoute = ModelActivationEndpointMethod + " " + ModelActivationEndpointPath;
    public const string HumanApprovalEndpointMethod = "POST";
    public const string HumanApprovalChallengeEndpointPath = DesignConversionModelServerHumanApprovalHandler.ChallengeEndpointPath;
    public const string HumanApprovalApproveEndpointPath = DesignConversionModelServerHumanApprovalHandler.ApproveEndpointPath;
    public const string HumanApprovalChallengeEndpointRoute = HumanApprovalEndpointMethod + " " + HumanApprovalChallengeEndpointPath;
    public const string HumanApprovalApproveEndpointRoute = HumanApprovalEndpointMethod + " " + HumanApprovalApproveEndpointPath;
    public const string HostIntegrationBoundary =
        "local_endpoint_registry_contract_no_asp_net_host_no_socket_no_public_deployment";
    public const string LocalCompositionImprovementLoopRegistrationBoundary =
        "local_improvement_loop_route_registered_from_explicit_service_composition";
    public const string ExternalVisualProviderSharedHttpClientRequiredErrorCode =
        "DESIGN_CONVERSION_EXTERNAL_VISUAL_PROVIDER_SHARED_HTTP_CLIENT_REQUIRED";

    private readonly Dictionary<string, DesignConversionModelServerEndpointRegistration> _routes = new(StringComparer.Ordinal);

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionEvaluate(
        DesignConversionModelServerHttpHandler handler)
    {
        if (handler is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion HTTP handler is required.",
                "DESIGN_CONVERSION_HTTP_HANDLER_REQUIRED");
        }

        var registration = CreateEvaluateRegistration(
            handler,
            EndpointPath,
            "design_conversion_evaluate");
        var internalRegistration = CreateEvaluateRegistration(
            handler,
            InternalEndpointPath,
            "design_conversion_evaluate_internal_alias");

        _routes[RouteKey(EndpointMethod, EndpointPath)] = registration;
        _routes[RouteKey(EndpointMethod, InternalEndpointPath)] = internalRegistration;
        return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(registration);
    }

    private static DesignConversionModelServerEndpointRegistration CreateEvaluateRegistration(
        DesignConversionModelServerHttpHandler handler,
        string endpointPath,
        string routeKind)
        => new(
            Method: EndpointMethod,
            Path: endpointPath,
            RequestContentType: "application/json",
            ResponseContentType: "application/json; charset=utf-8",
            AspNetHostVerified: false,
            SocketOpened: false,
            PublicHttpEndpointVerified: false,
            CrossRepoCallerVerified: false,
            Handler: handler.HandleAsync,
            BoundaryMetadata: BuildBoundaryMetadata(
                EndpointMethod,
                endpointPath,
                nameof(DesignConversionModelServerHttpHandler),
                routeKind,
                "application/json"),
            RouteAdmission: handler.RouteAdmission);

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionImprovementLoop(
        DesignConversionModelServerImprovementLoopHandler handler)
        => RegisterDesignConversionImprovementLoop(handler, routeBoundaryMetadata: null);

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionImprovementLoopFromComposition(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        DesignConversionModelServerRouteAdmission? routeAdmission = null)
    {
        if (composition is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion local service composition is required.",
                "DESIGN_CONVERSION_LOCAL_SERVICE_COMPOSITION_REQUIRED");
        }

        if (candidateStrategy is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion improvement-loop candidate strategy is required.",
                "DESIGN_CONVERSION_IMPROVEMENT_LOOP_STRATEGY_REQUIRED");
        }

        var routeBoundaryMetadata = MergeBoundaryMetadata(
            BuildCompositionRouteBoundaryMetadata(composition),
            BuildNoLocalArtifactDbBoundaryMetadata());
        return RegisterDesignConversionImprovementLoopFromCompositionCore(
            composition,
            candidateStrategy,
            artifactDb: null,
            utcNow: null,
            routeAdmission,
            routeBoundaryMetadata,
            externalVisualProviderCoordinatorResolver: null);
    }

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionImprovementLoopFromComposition(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb artifactDb,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionModelServerRouteAdmission? routeAdmission = null)
    {
        if (composition is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion local service composition is required.",
                "DESIGN_CONVERSION_LOCAL_SERVICE_COMPOSITION_REQUIRED");
        }

        if (candidateStrategy is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion improvement-loop candidate strategy is required.",
                "DESIGN_CONVERSION_IMPROVEMENT_LOOP_STRATEGY_REQUIRED");
        }

        if (artifactDb is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion local artifact DB is required for the full local route registry.",
                "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_REQUIRED");
        }

        var routeBoundaryMetadata = MergeBoundaryMetadata(
            BuildCompositionRouteBoundaryMetadata(composition),
            BuildFullLocalArtifactDbBoundaryMetadata());
        return RegisterDesignConversionImprovementLoopFromCompositionCore(
            composition,
            candidateStrategy,
            artifactDb,
            utcNow,
            routeAdmission,
            routeBoundaryMetadata,
            externalVisualProviderCoordinatorResolver: null);
    }

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        HttpClient sharedHttpClient,
        DesignConversionModelServerRouteAdmission? routeAdmission = null)
    {
        if (composition is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion local service composition is required.",
                "DESIGN_CONVERSION_LOCAL_SERVICE_COMPOSITION_REQUIRED");
        }

        if (candidateStrategy is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion improvement-loop candidate strategy is required.",
                "DESIGN_CONVERSION_IMPROVEMENT_LOOP_STRATEGY_REQUIRED");
        }

        if (sharedHttpClient is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion external visual provider shared HTTP client is required for request-scoped route registration.",
                ExternalVisualProviderSharedHttpClientRequiredErrorCode);
        }

        var routeBoundaryMetadata = MergeBoundaryMetadata(
            BuildCompositionRouteBoundaryMetadata(composition),
            MergeBoundaryMetadata(
                BuildNoLocalArtifactDbBoundaryMetadata(),
                BuildRequestScopedExternalVisualProviderBoundaryMetadata()));
        return RegisterDesignConversionImprovementLoopFromCompositionCore(
            composition,
            candidateStrategy,
            artifactDb: null,
            utcNow: null,
            routeAdmission,
            routeBoundaryMetadata,
            CreateRequestScopedExternalVisualProviderCoordinatorResolver(
                composition,
                candidateStrategy,
                sharedHttpClient,
                artifactDb: null,
                utcNow: null));
    }

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb artifactDb,
        HttpClient sharedHttpClient,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionModelServerRouteAdmission? routeAdmission = null)
    {
        if (composition is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion local service composition is required.",
                "DESIGN_CONVERSION_LOCAL_SERVICE_COMPOSITION_REQUIRED");
        }

        if (candidateStrategy is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion improvement-loop candidate strategy is required.",
                "DESIGN_CONVERSION_IMPROVEMENT_LOOP_STRATEGY_REQUIRED");
        }

        if (artifactDb is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion local artifact DB is required for the full local route registry.",
                "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_REQUIRED");
        }

        if (sharedHttpClient is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion external visual provider shared HTTP client is required for request-scoped route registration.",
                ExternalVisualProviderSharedHttpClientRequiredErrorCode);
        }

        var routeBoundaryMetadata = MergeBoundaryMetadata(
            BuildCompositionRouteBoundaryMetadata(composition),
            MergeBoundaryMetadata(
                BuildFullLocalArtifactDbBoundaryMetadata(),
                BuildRequestScopedExternalVisualProviderBoundaryMetadata()));
        return RegisterDesignConversionImprovementLoopFromCompositionCore(
            composition,
            candidateStrategy,
            artifactDb,
            utcNow,
            routeAdmission,
            routeBoundaryMetadata,
            CreateRequestScopedExternalVisualProviderCoordinatorResolver(
                composition,
                candidateStrategy,
                sharedHttpClient,
                artifactDb,
                utcNow));
    }

    private OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionImprovementLoopFromCompositionCore(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb,
        Func<DateTimeOffset>? utcNow,
        DesignConversionModelServerRouteAdmission? routeAdmission,
        IReadOnlyDictionary<string, string> routeBoundaryMetadata,
        DesignConversionExternalVisualProviderCoordinatorResolver? externalVisualProviderCoordinatorResolver)
    {
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            composition.Service,
            candidateStrategy,
            artifactDb,
            utcNow);
        var coordinatorResolver = externalVisualProviderCoordinatorResolver ??
            (_ => OperationResult<DesignConversionImprovementLoopCoordinator>.Ok(coordinator));
        return RegisterDesignConversionImprovementLoop(
            new DesignConversionModelServerImprovementLoopHandler(
                coordinator,
                routeBoundaryMetadata,
                routeAdmission,
                coordinatorResolver),
            routeBoundaryMetadata);
    }

    private static DesignConversionExternalVisualProviderCoordinatorResolver CreateRequestScopedExternalVisualProviderCoordinatorResolver(
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        HttpClient sharedHttpClient,
        IDesignConversionLocalArtifactDb? artifactDb,
        Func<DateTimeOffset>? utcNow)
        => config =>
        {
            var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(
                config,
                sharedHttpClient);
            if (!visualJudge.IsSuccess)
                return visualJudge.MapError<DesignConversionImprovementLoopCoordinator>();

            var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(visualJudge.Value!);
            var requestScopedService = composition.CreateRequestScopedService(
                comparisonProvider,
                visualJudge.Value!);
            return OperationResult<DesignConversionImprovementLoopCoordinator>.Ok(
                new DesignConversionImprovementLoopCoordinator(
                    requestScopedService,
                    candidateStrategy,
                    artifactDb,
                    utcNow));
        };

    private OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionImprovementLoop(
        DesignConversionModelServerImprovementLoopHandler handler,
        IReadOnlyDictionary<string, string>? routeBoundaryMetadata)
    {
        if (handler is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion improvement-loop HTTP handler is required.",
                "DESIGN_CONVERSION_IMPROVEMENT_LOOP_HANDLER_REQUIRED");
        }

        var registration = new DesignConversionModelServerEndpointRegistration(
            Method: EndpointMethod,
            Path: ImprovementLoopEndpointPath,
            RequestContentType: "application/json",
            ResponseContentType: "application/json; charset=utf-8",
            AspNetHostVerified: false,
            SocketOpened: false,
            PublicHttpEndpointVerified: false,
            CrossRepoCallerVerified: false,
            Handler: handler.HandleAsync,
            BoundaryMetadata: MergeBoundaryMetadata(
                BuildBoundaryMetadata(
                    EndpointMethod,
                    ImprovementLoopEndpointPath,
                    nameof(DesignConversionModelServerImprovementLoopHandler),
                    "design_conversion_improvement_loop",
                    "application/json"),
                routeBoundaryMetadata),
            RouteAdmission: handler.RouteAdmission);

        _routes[RouteKey(EndpointMethod, ImprovementLoopEndpointPath)] = registration;
        return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(registration);
    }

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionStatusReadback(
        DesignConversionModelServerStatusHandler handler)
    {
        if (handler is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion status/readback HTTP handler is required.",
                "DESIGN_CONVERSION_STATUS_READBACK_HANDLER_REQUIRED");
        }

        var registration = new DesignConversionModelServerEndpointRegistration(
            Method: StatusReadbackEndpointMethod,
            Path: StatusReadbackEndpointPath,
            RequestContentType: "none",
            ResponseContentType: "application/json; charset=utf-8",
            AspNetHostVerified: false,
            SocketOpened: false,
            PublicHttpEndpointVerified: false,
            CrossRepoCallerVerified: false,
            Handler: handler.HandleAsync,
            BoundaryMetadata: BuildBoundaryMetadata(
                StatusReadbackEndpointMethod,
                StatusReadbackEndpointPath,
                nameof(DesignConversionModelServerStatusHandler),
                "design_conversion_status_readback",
                "none"),
            RouteAdmission: handler.RouteAdmission);

        _routes[RouteKey(StatusReadbackEndpointMethod, StatusReadbackEndpointPath)] = registration;
        return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(registration);
    }

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionVisualCostReadback(
        DesignConversionModelServerVisualCostHandler handler)
    {
        if (handler is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion visual-cost readback HTTP handler is required.",
                "DESIGN_CONVERSION_VISUAL_COST_READBACK_HANDLER_REQUIRED");
        }

        var registration = new DesignConversionModelServerEndpointRegistration(
            Method: VisualCostReadbackEndpointMethod,
            Path: VisualCostReadbackEndpointPath,
            RequestContentType: "none",
            ResponseContentType: "application/json; charset=utf-8",
            AspNetHostVerified: false,
            SocketOpened: false,
            PublicHttpEndpointVerified: false,
            CrossRepoCallerVerified: false,
            Handler: handler.HandleAsync,
            BoundaryMetadata: BuildBoundaryMetadata(
                VisualCostReadbackEndpointMethod,
                VisualCostReadbackEndpointPath,
                nameof(DesignConversionModelServerVisualCostHandler),
                "design_conversion_visual_cost_readback",
                "none"),
            RouteAdmission: handler.RouteAdmission);

        _routes[RouteKey(VisualCostReadbackEndpointMethod, VisualCostReadbackEndpointPath)] = registration;
        return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(registration);
    }

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionDpoUsefulness(
        DesignConversionModelServerDpoUsefulnessHandler handler)
    {
        if (handler is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion DPO usefulness HTTP handler is required.",
                "DESIGN_CONVERSION_DPO_USEFULNESS_HANDLER_REQUIRED");
        }

        var registration = new DesignConversionModelServerEndpointRegistration(
            Method: DpoUsefulnessEndpointMethod,
            Path: DpoUsefulnessEndpointPath,
            RequestContentType: "none",
            ResponseContentType: "application/json; charset=utf-8",
            AspNetHostVerified: false,
            SocketOpened: false,
            PublicHttpEndpointVerified: false,
            CrossRepoCallerVerified: false,
            Handler: handler.HandleAsync,
            BoundaryMetadata: BuildDpoUsefulnessBoundaryMetadata());

        _routes[RouteKey(DpoUsefulnessEndpointMethod, DpoUsefulnessEndpointPath)] = registration;
        return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(registration);
    }

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionTrainingIntake(
        DesignConversionModelServerTrainingIntakeHandler handler)
    {
        if (handler is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion training intake HTTP handler is required.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_HANDLER_REQUIRED");
        }

        var registration = new DesignConversionModelServerEndpointRegistration(
            Method: TrainingIntakeEndpointMethod,
            Path: TrainingIntakeEndpointPath,
            RequestContentType: "application/json",
            ResponseContentType: "application/json; charset=utf-8",
            AspNetHostVerified: false,
            SocketOpened: false,
            PublicHttpEndpointVerified: false,
            CrossRepoCallerVerified: false,
            Handler: handler.HandleAsync,
            BoundaryMetadata: BuildTrainingIntakeBoundaryMetadata());

        _routes[RouteKey(TrainingIntakeEndpointMethod, TrainingIntakeEndpointPath)] = registration;
        return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(registration);
    }

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionFlow38Quarantine(
        DesignConversionModelServerFlow38QuarantineHandler handler)
    {
        if (handler is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion FLOW-38 quarantine HTTP handler is required.",
                "DESIGN_CONVERSION_FLOW38_QUARANTINE_HANDLER_REQUIRED");
        }

        var registration = new DesignConversionModelServerEndpointRegistration(
            Method: Flow38QuarantineEndpointMethod,
            Path: Flow38QuarantineEndpointPath,
            RequestContentType: "application/json",
            ResponseContentType: "application/json; charset=utf-8",
            AspNetHostVerified: false,
            SocketOpened: false,
            PublicHttpEndpointVerified: false,
            CrossRepoCallerVerified: false,
            Handler: handler.HandleAsync,
            BoundaryMetadata: BuildFlow38QuarantineBoundaryMetadata(handler.IsConfigured));

        _routes[RouteKey(Flow38QuarantineEndpointMethod, Flow38QuarantineEndpointPath)] = registration;
        return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(registration);
    }

    public OperationResult<DesignConversionModelServerEndpointRegistration> RegisterDesignConversionModelActivation(
        DesignConversionModelServerModelActivationHandler handler)
    {
        if (handler is null)
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion model activation HTTP handler is required.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_HANDLER_REQUIRED");
        }

        var registration = new DesignConversionModelServerEndpointRegistration(
            Method: ModelActivationEndpointMethod,
            Path: ModelActivationEndpointPath,
            RequestContentType: "application/json",
            ResponseContentType: "application/json; charset=utf-8",
            AspNetHostVerified: false,
            SocketOpened: false,
            PublicHttpEndpointVerified: false,
            CrossRepoCallerVerified: false,
            Handler: handler.HandleAsync,
            BoundaryMetadata: BuildModelActivationBoundaryMetadata());

        _routes[RouteKey(ModelActivationEndpointMethod, ModelActivationEndpointPath)] = registration;
        return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(registration);
    }

    public OperationResult<IReadOnlyList<DesignConversionModelServerEndpointRegistration>> RegisterDesignConversionHumanApproval(
        DesignConversionModelServerHumanApprovalHandler handler)
    {
        if (handler is null)
        {
            return OperationResult<IReadOnlyList<DesignConversionModelServerEndpointRegistration>>.Fail(
                "DesignConversion human approval HTTP handler is required.",
                "DESIGN_CONVERSION_HUMAN_APPROVAL_HANDLER_REQUIRED");
        }

        var challengeRegistration = new DesignConversionModelServerEndpointRegistration(
            Method: HumanApprovalEndpointMethod,
            Path: HumanApprovalChallengeEndpointPath,
            RequestContentType: "application/json",
            ResponseContentType: "application/json; charset=utf-8",
            AspNetHostVerified: false,
            SocketOpened: false,
            PublicHttpEndpointVerified: false,
            CrossRepoCallerVerified: false,
            Handler: handler.HandleAsync,
            BoundaryMetadata: BuildHumanApprovalBoundaryMetadata(
                HumanApprovalChallengeEndpointRoute,
                HumanApprovalChallengeEndpointPath,
                "design_conversion_human_approval_challenge"));

        var approveRegistration = new DesignConversionModelServerEndpointRegistration(
            Method: HumanApprovalEndpointMethod,
            Path: HumanApprovalApproveEndpointPath,
            RequestContentType: "application/json",
            ResponseContentType: "application/json; charset=utf-8",
            AspNetHostVerified: false,
            SocketOpened: false,
            PublicHttpEndpointVerified: false,
            CrossRepoCallerVerified: false,
            Handler: handler.HandleAsync,
            BoundaryMetadata: BuildHumanApprovalBoundaryMetadata(
                HumanApprovalApproveEndpointRoute,
                HumanApprovalApproveEndpointPath,
                "design_conversion_human_approval_approve"));

        _routes[RouteKey(HumanApprovalEndpointMethod, HumanApprovalChallengeEndpointPath)] = challengeRegistration;
        _routes[RouteKey(HumanApprovalEndpointMethod, HumanApprovalApproveEndpointPath)] = approveRegistration;
        return OperationResult<IReadOnlyList<DesignConversionModelServerEndpointRegistration>>.Ok(
            [challengeRegistration, approveRegistration]);
    }

    public OperationResult<DesignConversionModelServerEndpointRegistration> Resolve(string? method, string? path)
    {
        var normalizedMethod = NormalizeMethod(method);
        if (string.IsNullOrWhiteSpace(normalizedMethod))
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                "DesignConversion endpoint method is required.",
                "DESIGN_CONVERSION_ENDPOINT_METHOD_REQUIRED");
        }

        var normalizedPath = NormalizePath(path);
        if (_routes.TryGetValue(RouteKey(normalizedMethod, normalizedPath), out var registration))
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(registration);

        if (string.Equals(normalizedMethod, VisualCostReadbackEndpointMethod, StringComparison.Ordinal) &&
            IsVisualCostReadbackPath(normalizedPath) &&
            _routes.TryGetValue(RouteKey(VisualCostReadbackEndpointMethod, VisualCostReadbackEndpointPath), out var visualCostRegistration))
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(visualCostRegistration);
        }

        if (string.Equals(normalizedMethod, StatusReadbackEndpointMethod, StringComparison.Ordinal) &&
            IsStatusReadbackPath(normalizedPath) &&
            _routes.TryGetValue(RouteKey(StatusReadbackEndpointMethod, StatusReadbackEndpointPath), out var statusRegistration))
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(statusRegistration);
        }

        if (string.Equals(normalizedMethod, DpoUsefulnessEndpointMethod, StringComparison.Ordinal) &&
            string.Equals(normalizedPath, DpoUsefulnessEndpointPath, StringComparison.Ordinal) &&
            _routes.TryGetValue(RouteKey(DpoUsefulnessEndpointMethod, DpoUsefulnessEndpointPath), out var dpoRegistration))
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(dpoRegistration);
        }

        if (string.Equals(normalizedMethod, TrainingIntakeEndpointMethod, StringComparison.Ordinal) &&
            string.Equals(normalizedPath, TrainingIntakeEndpointPath, StringComparison.Ordinal) &&
            _routes.TryGetValue(RouteKey(TrainingIntakeEndpointMethod, TrainingIntakeEndpointPath), out var trainingIntakeRegistration))
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(trainingIntakeRegistration);
        }

        if (string.Equals(normalizedMethod, ModelActivationEndpointMethod, StringComparison.Ordinal) &&
            string.Equals(normalizedPath, ModelActivationEndpointPath, StringComparison.Ordinal) &&
            _routes.TryGetValue(RouteKey(ModelActivationEndpointMethod, ModelActivationEndpointPath), out var modelActivationRegistration))
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(modelActivationRegistration);
        }

        if (string.Equals(normalizedMethod, HumanApprovalEndpointMethod, StringComparison.Ordinal) &&
            (string.Equals(normalizedPath, HumanApprovalChallengeEndpointPath, StringComparison.Ordinal) ||
             string.Equals(normalizedPath, HumanApprovalApproveEndpointPath, StringComparison.Ordinal)) &&
            _routes.TryGetValue(RouteKey(HumanApprovalEndpointMethod, normalizedPath), out var humanApprovalRegistration))
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Ok(humanApprovalRegistration);
        }

        if (RegisteredPathExistsForDifferentMethod(normalizedMethod, normalizedPath))
        {
            return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
                $"DesignConversion endpoint method is not allowed for registered route: {normalizedMethod} {normalizedPath}",
                "DESIGN_CONVERSION_ENDPOINT_METHOD_NOT_ALLOWED");
        }

        return OperationResult<DesignConversionModelServerEndpointRegistration>.Fail(
            $"DesignConversion endpoint route is not registered: {normalizedMethod} {normalizedPath}",
            "DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND");
    }

    public async Task<OperationResult<DesignConversionModelServerHttpResponse>> InvokeAsync(
        DesignConversionModelServerHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return OperationResult<DesignConversionModelServerHttpResponse>.Fail(
                "DesignConversion endpoint request is required.",
                "DESIGN_CONVERSION_ENDPOINT_REQUEST_REQUIRED");
        }

        var registration = Resolve(request.Method, request.Path);
        if (!registration.IsSuccess)
        {
            return OperationResult<DesignConversionModelServerHttpResponse>.Fail(
                registration.ErrorMessage,
                registration.ErrorCode);
        }

        try
        {
            return OperationResult<DesignConversionModelServerHttpResponse>.Ok(
                await registration.Value!.Handler(request, cancellationToken));
        }
        catch (OperationCanceledException exception)
            when (IsSuppliedCancellation(exception, cancellationToken))
        {
            return OperationResult<DesignConversionModelServerHttpResponse>.Fail(
                EndpointCancelledErrorMessage,
                EndpointCancelledErrorCode);
        }
        catch (Exception)
        {
            return OperationResult<DesignConversionModelServerHttpResponse>.Fail(
                EndpointUnhandledErrorMessage,
                EndpointUnhandledErrorCode);
        }
    }

    private static IReadOnlyDictionary<string, string> BuildBoundaryMetadata(
        string endpointMethod,
        string endpointPath,
        string handlerName,
        string routeKind,
        string requestContentType)
    {
        var endpointRoute = endpointMethod + " " + endpointPath;
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["endpoint_route"] = endpointRoute,
            ["endpoint_method"] = endpointMethod,
            ["endpoint_path"] = endpointPath,
            ["endpoint_route_kind"] = routeKind,
            ["endpoint_request_content_type"] = requestContentType,
            ["endpoint_response_content_type"] = "application/json; charset=utf-8",
            ["host_integration_boundary"] = HostIntegrationBoundary,
            ["delegates_to_handler"] = handlerName,
            ["caller_contract_schema_version"] = DesignConversionModelServerCallerContract.SchemaVersion,
            ["caller_contract_boundary"] = DesignConversionModelServerCallerContract.Boundary,
            ["caller_contract_required_fields"] = string.Join(",", DesignConversionModelServerCallerContract.RequiredRequestFieldNames()),
            ["refs_only_response_required"] = bool.TrueString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        foreach (var pair in DesignConversionModelServerCallerContract.BoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildCompositionRouteBoundaryMetadata(
        DesignConversionLocalServiceComposition composition)
    {
        var metadata = new Dictionary<string, string>(composition.Descriptor.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["improvement_loop_registered_from_local_service_composition"] = bool.TrueString,
            ["improvement_loop_local_service_composition_boundary"] =
                LocalCompositionImprovementLoopRegistrationBoundary,
            ["composition_kind"] = composition.Descriptor.CompositionKind,
            ["refs_only_public_response"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["real_external_provider_executed"] = bool.FalseString,
            ["network_call_executed"] = bool.FalseString,
            ["public_production_default_ready"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["local_service_composition_evidence_refs"] =
                string.Join("|", composition.Descriptor.EvidenceRefs)
        };

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildNoLocalArtifactDbBoundaryMetadata()
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["improvement_loop_local_artifact_db_autowrite_enabled"] = bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker] = bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker] = bool.FalseString,
            ["improvement_loop_to_local_training_store"] = bool.FalseString,
            ["local_training_store_dpo_source"] = bool.FalseString,
            ["local_artifact_db_configured"] = bool.FalseString,
            ["local_artifact_db_persistence_status"] = "not_configured_non_durable",
            ["persistence_invoked"] = bool.FalseString,
            ["runtime_created_dpo_candidate"] = bool.FalseString,
            ["runtime_created_dpo_candidate_count"] = "0",
            ["production_db_ready"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

    private static IReadOnlyDictionary<string, string> BuildFullLocalArtifactDbBoundaryMetadata()
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["improvement_loop_local_artifact_db_autowrite_enabled"] = bool.TrueString,
            ["local_training_store_shared_with_dpo_and_training_intake"] = bool.TrueString
        };

    private static IReadOnlyDictionary<string, string> BuildRequestScopedExternalVisualProviderBoundaryMetadata()
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["external_visual_provider_request_config_supported"] = bool.TrueString,
            ["external_visual_provider_request_scoped_resolver_configured"] = bool.TrueString,
            ["external_visual_provider_shared_http_client_injected"] = bool.TrueString,
            ["external_visual_provider_http_client_owned_by_caller"] = bool.TrueString,
            ["external_visual_provider_http_client_disposed_by_registry"] = bool.FalseString,
            ["raw_external_visual_provider_config_in_route_metadata"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString
        };

    private static IReadOnlyDictionary<string, string> BuildDpoUsefulnessBoundaryMetadata()
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["endpoint_route"] = DpoUsefulnessEndpointRoute,
            ["endpoint_method"] = DpoUsefulnessEndpointMethod,
            ["endpoint_path"] = DpoUsefulnessEndpointPath,
            ["endpoint_route_kind"] = "design_conversion_dpo_usefulness_readback",
            ["endpoint_request_content_type"] = "none",
            ["endpoint_response_content_type"] = "application/json; charset=utf-8",
            ["host_integration_boundary"] = HostIntegrationBoundary,
            ["delegates_to_handler"] = nameof(DesignConversionModelServerDpoUsefulnessHandler),
            ["delegates_to_domain_api"] = nameof(DesignConversionDpoUsefulnessApi),
            ["dpo_usefulness_contract_schema_version"] =
                DesignConversionModelServerCallerContract.DpoUsefulnessRouteSchemaVersion,
            ["dpo_usefulness_endpoint_route"] = DpoUsefulnessEndpointRoute,
            ["refs_only_response_required"] = bool.TrueString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        foreach (var pair in DesignConversionModelServerCallerContract.DpoUsefulnessBoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildHumanApprovalBoundaryMetadata(
        string endpointRoute,
        string endpointPath,
        string routeKind)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["endpoint_route"] = endpointRoute,
            ["endpoint_method"] = HumanApprovalEndpointMethod,
            ["endpoint_path"] = endpointPath,
            ["endpoint_route_kind"] = routeKind,
            ["endpoint_request_content_type"] = "application/json",
            ["endpoint_response_content_type"] = "application/json; charset=utf-8",
            ["host_integration_boundary"] = HostIntegrationBoundary,
            ["delegates_to_handler"] = nameof(DesignConversionModelServerHumanApprovalHandler),
            ["delegates_to_domain_api"] = nameof(DesignConversionHumanApprovalService),
            ["human_approval_boundary"] = DesignConversionModelServerHumanApprovalHandler.HumanApprovalBoundary,
            ["typed_tenant_actor_authorization_required"] = bool.TrueString,
            ["anonymous_public_authority"] = bool.FalseString,
            ["external_platform_auth_boundary"] = "external_to_modelserver_actor_context_required",
            ["refs_only_response_required"] = bool.TrueString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
            [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue,
            ["dpo_eligible"] = bool.FalseString,
            ["model_weights_published"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildTrainingIntakeBoundaryMetadata()
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["endpoint_route"] = TrainingIntakeEndpointRoute,
            ["endpoint_method"] = TrainingIntakeEndpointMethod,
            ["endpoint_path"] = TrainingIntakeEndpointPath,
            ["endpoint_route_kind"] = "design_conversion_training_intake_actions",
            ["endpoint_request_content_type"] = "application/json",
            ["endpoint_response_content_type"] = "application/json; charset=utf-8",
            ["host_integration_boundary"] = HostIntegrationBoundary,
            ["delegates_to_handler"] = nameof(DesignConversionModelServerTrainingIntakeHandler),
            ["delegates_to_domain_api"] = nameof(DesignConversionDpoUsefulnessApi),
            ["training_intake_contract_schema_version"] =
                DesignConversionModelServerCallerContract.TrainingIntakeRouteSchemaVersion,
            ["training_intake_endpoint_route"] = TrainingIntakeEndpointRoute,
            ["training_intake_supported_actions"] = string.Join(",", DesignConversionModelServerCallerContract.TrainingIntakeSupportedActions()),
            ["refs_only_response_required"] = bool.TrueString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        foreach (var pair in DesignConversionModelServerCallerContract.TrainingIntakeBoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildFlow38QuarantineBoundaryMetadata(bool configured)
    {
        var metadata = new Dictionary<string, string>(
            DesignConversionModelServerFlow38QuarantineHandler.BoundaryMetadata(
                configured,
                persisted: false),
            StringComparer.Ordinal)
        {
            ["endpoint_route"] = Flow38QuarantineEndpointRoute,
            ["endpoint_method"] = Flow38QuarantineEndpointMethod,
            ["endpoint_path"] = Flow38QuarantineEndpointPath,
            ["endpoint_route_kind"] = "flow38_quarantined_feedback_core_intake",
            ["endpoint_request_content_type"] = "application/json",
            ["endpoint_response_content_type"] = "application/json; charset=utf-8",
            ["host_integration_boundary"] = HostIntegrationBoundary,
            ["delegates_to_handler"] = nameof(DesignConversionModelServerFlow38QuarantineHandler),
            ["legal_verdict"] = DesignConversionModelServerFlow38QuarantineHandler.QuarantineVerdict,
            ["allowed_use"] = DesignConversionModelServerFlow38QuarantineHandler.PersonalAdaptiveUse,
            ["route"] = DesignConversionModelServerFlow38QuarantineHandler.AdaptiveOrQuarantineRoute,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildModelActivationBoundaryMetadata()
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["endpoint_route"] = ModelActivationEndpointRoute,
            ["endpoint_method"] = ModelActivationEndpointMethod,
            ["endpoint_path"] = ModelActivationEndpointPath,
            ["endpoint_route_kind"] = "design_conversion_model_activation_internal_lifecycle",
            ["endpoint_request_content_type"] = "application/json",
            ["endpoint_response_content_type"] = "application/json; charset=utf-8",
            ["host_integration_boundary"] = HostIntegrationBoundary,
            ["delegates_to_handler"] = nameof(DesignConversionModelServerModelActivationHandler),
            ["delegates_to_domain_api"] = nameof(DesignConversionModelServerModelActivationApi),
            ["model_activation_endpoint_route"] = ModelActivationEndpointRoute,
            ["model_activation_route_boundary"] =
                DesignConversionModelServerModelActivationHandler.LocalModelActivationRouteBoundary,
            ["model_activation_supported_actions"] = string.Join(
                ",",
                [
                    DesignConversionModelServerModelActivationHandler.ActionActivate,
                    DesignConversionModelServerModelActivationHandler.ActionRollback,
                    DesignConversionModelServerModelActivationHandler.ActionReadActive
                ]),
            ["model_activation_internal_token_required"] = bool.TrueString,
            ["model_activation_public_anonymous_control"] = bool.FalseString,
            ["model_activation_paths_returned_as_hash_refs"] = bool.TrueString,
            ["model_activation_checkpoints_deleted"] = bool.FalseString,
            ["refs_only_response_required"] = bool.TrueString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> MergeBoundaryMetadata(
        IReadOnlyDictionary<string, string> baseMetadata,
        IReadOnlyDictionary<string, string>? routeBoundaryMetadata)
    {
        if (routeBoundaryMetadata is null || routeBoundaryMetadata.Count == 0)
            return baseMetadata;

        var metadata = new Dictionary<string, string>(baseMetadata, StringComparer.Ordinal);
        foreach (var pair in routeBoundaryMetadata)
            metadata[pair.Key] = pair.Value;

        metadata["refs_only_response_required"] = bool.TrueString;
        metadata["refs_only_public_response"] = bool.TrueString;
        metadata["raw_html_included"] = bool.FalseString;
        metadata["raw_css_included"] = bool.FalseString;
        metadata["raw_design_tree_included"] = bool.FalseString;
        metadata["response_contains_raw_private_data"] = bool.FalseString;
        metadata["external_provider_called"] = bool.FalseString;
        metadata["external_ai_visual_judge_executed"] = bool.FalseString;
        metadata["network_call_executed"] = bool.FalseString;
        metadata["public_production_default_ready"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        return metadata;
    }

    private static string NormalizeMethod(string? method)
        => string.IsNullOrWhiteSpace(method) ? string.Empty : method.Trim().ToUpperInvariant();

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? trimmed[..queryIndex] : trimmed;
    }

    private static bool IsStatusReadbackPath(string path)
        => string.Equals(path, DesignConversionModelServerStatusHandler.EndpointBasePath, StringComparison.Ordinal) ||
           path.StartsWith(DesignConversionModelServerStatusHandler.EndpointPathPrefix, StringComparison.Ordinal);

    private static bool IsVisualCostReadbackPath(string path)
        => DesignConversionModelServerVisualCostHandler.IsVisualCostReadbackPath(path);

    private bool RegisteredPathExistsForDifferentMethod(string normalizedMethod, string normalizedPath)
    {
        foreach (var route in _routes.Values)
        {
            if (!string.Equals(route.Path, normalizedPath, StringComparison.Ordinal))
                continue;

            return !string.Equals(route.Method, normalizedMethod, StringComparison.Ordinal);
        }

        var statusPathRegisteredForDifferentMethod =
            IsStatusReadbackPath(normalizedPath) &&
            _routes.ContainsKey(RouteKey(StatusReadbackEndpointMethod, StatusReadbackEndpointPath)) &&
            !string.Equals(normalizedMethod, StatusReadbackEndpointMethod, StringComparison.Ordinal);
        var visualCostPathRegisteredForDifferentMethod =
            IsVisualCostReadbackPath(normalizedPath) &&
            _routes.ContainsKey(RouteKey(VisualCostReadbackEndpointMethod, VisualCostReadbackEndpointPath)) &&
            !string.Equals(normalizedMethod, VisualCostReadbackEndpointMethod, StringComparison.Ordinal);

        return statusPathRegisteredForDifferentMethod || visualCostPathRegisteredForDifferentMethod;
    }

    private static bool IsSuppliedCancellation(
        OperationCanceledException exception,
        CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested &&
           (!exception.CancellationToken.CanBeCanceled ||
            exception.CancellationToken == cancellationToken);

    private static string RouteKey(string method, string path)
        => NormalizeMethod(method) + " " + NormalizePath(path);
}
