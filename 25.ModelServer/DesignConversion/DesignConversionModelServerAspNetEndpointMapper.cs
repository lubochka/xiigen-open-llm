// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class DesignConversionModelServerAspNetEndpointMapper
{
    public const string AspNetEndpointMapperBoundary =
        "asp_net_minimal_api_endpoint_mapper_refs_only_no_public_deployment";
    public const string AspNetHostKind = "asp_net_core_minimal_api";
    public const string AspNetLiveLocalBaseUrlSource = "asp_net_local_connection";
    public const string AspNetRouteVerificationAuthority = "asp_net_endpoint_mapper";
    public const string AspNetRouteVerifiedState = "verified_by_server_owned_loopback_route_dispatch";
    public const int MaximumJsonRequestBodyBytes = 8 * 1024 * 1024;
    public const string JsonRequestBodyTooLargeErrorCode = "REQUEST_BODY_TOO_LARGE";
    public const string JsonRequestBodyTooLargeErrorMessage =
        "DesignConversion ASP.NET JSON request body is too large.";
    private const string InvalidUtf8JsonSentinel = "{";
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly HashSet<string> ArtifactUploadDispatchHeaderAllowlist = new(StringComparer.OrdinalIgnoreCase)
    {
        DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
        DesignConversionModelServerRouteAdmission.ProjectAuthorizationHeaderName,
        DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
        DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName,
        DesignConversionModelServerArtifactUploadHttpHandler.ArtifactKindHeaderName,
        DesignConversionModelServerArtifactUploadHttpHandler.ExpectedSha256HeaderName,
        DesignConversionModelServerArtifactUploadHttpHandler.WidthHeaderName,
        DesignConversionModelServerArtifactUploadHttpHandler.HeightHeaderName
    };

    public static IEndpointConventionBuilder MapDesignConversionModelServerEvaluate(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        return endpoints.MapPost(
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            context => DispatchEvaluateAsync(context, registry));
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerInternalEvaluate(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        return endpoints.MapPost(
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            context => DispatchEvaluateAsync(context, registry));
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerImprovementLoop(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        return endpoints.MapPost(
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            context => DispatchImprovementLoopAsync(context, registry));
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerDpoUsefulness(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        return endpoints.MapGet(
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            context => DispatchDpoUsefulnessAsync(context, registry));
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerStatusReadback(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        return endpoints.MapGet(
            DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
            context => DispatchStatusReadbackAsync(context, registry));
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerVisualCostReadback(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        return endpoints.MapGet(
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
            context => DispatchVisualCostReadbackAsync(context, registry));
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerTrainingIntake(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        return endpoints.MapPost(
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            context => DispatchTrainingIntakeAsync(context, registry));
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerFlow38Quarantine(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        return endpoints.MapPost(
            DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
            context => DispatchFlow38QuarantineAsync(context, registry));
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerModelActivation(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        return endpoints.MapPost(
            DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath,
            context => DispatchModelActivationAsync(context, registry));
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerHumanApproval(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        var group = endpoints.MapGroup("/api/design-conversion/human-approval");
        group.MapPost("/challenge", context => DispatchHumanApprovalAsync(
            context,
            registry,
            DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath));
        group.MapPost("/approve", context => DispatchHumanApprovalAsync(
            context,
            registry,
            DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath));
        return group;
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerHumanApprovalChallenge(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
        => MapDesignConversionModelServerHumanApprovalEndpoint(
            endpoints,
            registry,
            DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath);

    public static IEndpointConventionBuilder MapDesignConversionModelServerHumanApprovalApprove(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry)
        => MapDesignConversionModelServerHumanApprovalEndpoint(
            endpoints,
            registry,
            DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath);

    public static IEndpointConventionBuilder MapDesignConversionModelServerArtifactUpload(
        this IEndpointRouteBuilder endpoints,
        DesignConversionModelServerArtifactUploadHttpHandler handler)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        return endpoints.MapPost(
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
            context => DispatchArtifactUploadAsync(context, handler));
    }

    private static IEndpointConventionBuilder MapDesignConversionModelServerHumanApprovalEndpoint(
        IEndpointRouteBuilder endpoints,
        DesignConversionModelServerEndpointRegistry registry,
        string endpointPath)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));

        return endpoints.MapPost(
            endpointPath,
            context => DispatchHumanApprovalAsync(context, registry, endpointPath));
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerImprovementLoopFromComposition(
        this IEndpointRouteBuilder endpoints,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy)
    {
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
        if (candidateStrategy is null)
            throw new ArgumentNullException(nameof(candidateStrategy));

        var registry = DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
            composition,
            candidateStrategy);

        return endpoints.MapDesignConversionModelServerImprovementLoop(registry);
    }

    public static IEndpointConventionBuilder MapDesignConversionModelServerImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
        this IEndpointRouteBuilder endpoints,
        DesignConversionLocalServiceComposition composition,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        HttpClient sharedHttpClient,
        DesignConversionModelServerRouteAdmission? routeAdmission = null)
    {
        if (endpoints is null)
            throw new ArgumentNullException(nameof(endpoints));
        if (composition is null)
            throw new ArgumentNullException(nameof(composition));
        if (candidateStrategy is null)
            throw new ArgumentNullException(nameof(candidateStrategy));
        if (sharedHttpClient is null)
            throw new ArgumentNullException(nameof(sharedHttpClient));

        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromCompositionWithRequestScopedExternalVisualProvider(
            composition,
            candidateStrategy,
            sharedHttpClient,
            routeAdmission);
        if (!registered.IsSuccess)
        {
            throw new InvalidOperationException(
                registered.ErrorMessage ??
                "DesignConversion improvement-loop ASP.NET endpoint registration with request-scoped external visual provider failed.");
        }

        return endpoints.MapDesignConversionModelServerImprovementLoop(registry);
    }

    private static async Task DispatchEvaluateAsync(
        HttpContext context,
        DesignConversionModelServerEndpointRegistry registry)
    {
        var routePath = NormalizeRoutePath(context.Request.Path.Value);
        var preBodyAdmissionRejection = await TryDispatchAdmissionRejectionBeforeBodyAsync(
            context,
            registry,
            PreBodyAdmissionRoute.Evaluate);
        if (preBodyAdmissionRejection is not null)
        {
            var admissionResponse = AnnotateAspNetHost(
                preBodyAdmissionRejection,
                context,
                routePath,
                nameof(DesignConversionModelServerHttpHandler));
            await WriteResponseAsync(context, admissionResponse);
            return;
        }

        var read = await ToModelServerRequestAsync(context);
        if (await TryWriteJsonRequestReadRejectionAsync(
                context,
                read,
                routePath,
                nameof(DesignConversionModelServerHttpHandler)))
            return;

        var request = read.Request!;
        var invoked = await registry.InvokeAsync(request, context.RequestAborted);
        var response = AnnotateAspNetHost(invoked.IsSuccess
            ? invoked.Value!
            : BuildRegistryErrorResponse(request, invoked.ErrorCode, invoked.ErrorMessage),
            context,
            routePath,
            nameof(DesignConversionModelServerHttpHandler));

        await WriteResponseAsync(context, response);
    }

    private static async Task DispatchImprovementLoopAsync(
        HttpContext context,
        DesignConversionModelServerEndpointRegistry registry)
    {
        var preBodyAdmissionRejection = await TryDispatchAdmissionRejectionBeforeBodyAsync(
            context,
            registry,
            PreBodyAdmissionRoute.IdempotentMutation);
        if (preBodyAdmissionRejection is not null)
        {
            var admissionResponse = AnnotateAspNetHost(
                preBodyAdmissionRejection,
                context,
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                nameof(DesignConversionModelServerImprovementLoopHandler));
            await WriteResponseAsync(context, admissionResponse);
            return;
        }

        var read = await ToModelServerRequestAsync(context);
        if (await TryWriteJsonRequestReadRejectionAsync(
                context,
                read,
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                nameof(DesignConversionModelServerImprovementLoopHandler)))
            return;

        var request = read.Request!;
        var invoked = await registry.InvokeAsync(request, context.RequestAborted);
        var response = AnnotateAspNetHost(invoked.IsSuccess
            ? invoked.Value!
            : BuildRegistryErrorResponse(request, invoked.ErrorCode, invoked.ErrorMessage),
            context,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            nameof(DesignConversionModelServerImprovementLoopHandler));

        await WriteResponseAsync(context, response);
    }

    private static async Task DispatchStatusReadbackAsync(
        HttpContext context,
        DesignConversionModelServerEndpointRegistry registry)
    {
        var preBodyAdmissionRejection = await TryDispatchAdmissionRejectionBeforeBodyAsync(
            context,
            registry,
            PreBodyAdmissionRoute.Status);
        if (preBodyAdmissionRejection is not null)
        {
            var admissionResponse = AnnotateAspNetHost(
                preBodyAdmissionRejection,
                context,
                DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
                nameof(DesignConversionModelServerStatusHandler));
            await WriteResponseAsync(context, admissionResponse);
            return;
        }

        var read = await ToModelServerRequestAsync(context);
        if (await TryWriteJsonRequestReadRejectionAsync(
                context,
                read,
                DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
                nameof(DesignConversionModelServerStatusHandler)))
            return;

        var request = read.Request!;
        var invoked = await registry.InvokeAsync(request, context.RequestAborted);
        var response = AnnotateAspNetHost(invoked.IsSuccess
            ? invoked.Value!
            : BuildRegistryErrorResponse(request, invoked.ErrorCode, invoked.ErrorMessage),
            context,
            DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
            nameof(DesignConversionModelServerStatusHandler));

        await WriteResponseAsync(context, response);
    }

    private static async Task DispatchVisualCostReadbackAsync(
        HttpContext context,
        DesignConversionModelServerEndpointRegistry registry)
    {
        var preBodyAdmissionRejection = await TryDispatchAdmissionRejectionBeforeBodyAsync(
            context,
            registry,
            PreBodyAdmissionRoute.Status);
        if (preBodyAdmissionRejection is not null)
        {
            var admissionResponse = AnnotateAspNetHost(
                preBodyAdmissionRejection,
                context,
                DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
                nameof(DesignConversionModelServerVisualCostHandler));
            await WriteResponseAsync(context, admissionResponse);
            return;
        }

        var read = await ToModelServerRequestAsync(context);
        if (await TryWriteJsonRequestReadRejectionAsync(
                context,
                read,
                DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
                nameof(DesignConversionModelServerVisualCostHandler)))
            return;

        var request = read.Request!;
        var invoked = await registry.InvokeAsync(request, context.RequestAborted);
        var response = AnnotateAspNetHost(invoked.IsSuccess
            ? invoked.Value!
            : BuildRegistryErrorResponse(request, invoked.ErrorCode, invoked.ErrorMessage),
            context,
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
            nameof(DesignConversionModelServerVisualCostHandler));

        await WriteResponseAsync(context, response);
    }

    private static async Task DispatchDpoUsefulnessAsync(
        HttpContext context,
        DesignConversionModelServerEndpointRegistry registry)
    {
        var read = await ToModelServerRequestAsync(context);
        if (await TryWriteJsonRequestReadRejectionAsync(
                context,
                read,
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
                nameof(DesignConversionModelServerDpoUsefulnessHandler)))
            return;

        var request = read.Request!;
        var invoked = await registry.InvokeAsync(request, context.RequestAborted);
        var response = AnnotateAspNetHost(invoked.IsSuccess
            ? invoked.Value!
            : BuildRegistryErrorResponse(request, invoked.ErrorCode, invoked.ErrorMessage),
            context,
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            nameof(DesignConversionModelServerDpoUsefulnessHandler));

        await WriteResponseAsync(context, response);
    }

    private static async Task DispatchTrainingIntakeAsync(
        HttpContext context,
        DesignConversionModelServerEndpointRegistry registry)
    {
        var read = await ToModelServerRequestAsync(context);
        if (await TryWriteJsonRequestReadRejectionAsync(
                context,
                read,
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                nameof(DesignConversionModelServerTrainingIntakeHandler)))
            return;

        var request = read.Request!;
        var invoked = await registry.InvokeAsync(request, context.RequestAborted);
        var response = AnnotateAspNetHost(invoked.IsSuccess
            ? invoked.Value!
            : BuildRegistryErrorResponse(request, invoked.ErrorCode, invoked.ErrorMessage),
            context,
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            nameof(DesignConversionModelServerTrainingIntakeHandler));

        await WriteResponseAsync(context, response);
    }

    private static async Task DispatchFlow38QuarantineAsync(
        HttpContext context,
        DesignConversionModelServerEndpointRegistry registry)
    {
        var request = await ToBoundedModelServerRequestAsync(
            context,
            DesignConversionModelServerFlow38QuarantineHandler.MaximumRequestBodyBytes);
        var invoked = await registry.InvokeAsync(request, context.RequestAborted);
        var response = AnnotateAspNetHost(
            invoked.IsSuccess
                ? invoked.Value!
                : BuildRegistryErrorResponse(request, invoked.ErrorCode, invoked.ErrorMessage),
            context,
            DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
            nameof(DesignConversionModelServerFlow38QuarantineHandler),
            publishLiveRouteMappingFlags: true);

        await WriteResponseAsync(context, response);
    }

    private static async Task DispatchModelActivationAsync(
        HttpContext context,
        DesignConversionModelServerEndpointRegistry registry)
    {
        var read = await ToModelServerRequestAsync(context);
        if (await TryWriteJsonRequestReadRejectionAsync(
                context,
                read,
                DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath,
                nameof(DesignConversionModelServerModelActivationHandler)))
            return;

        var request = read.Request!;
        var invoked = await registry.InvokeAsync(request, context.RequestAborted);
        var response = AnnotateAspNetHost(invoked.IsSuccess
            ? invoked.Value!
            : BuildRegistryErrorResponse(request, invoked.ErrorCode, invoked.ErrorMessage),
            context,
            DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath,
            nameof(DesignConversionModelServerModelActivationHandler));

        await WriteResponseAsync(context, response);
    }

    private static async Task DispatchHumanApprovalAsync(
        HttpContext context,
        DesignConversionModelServerEndpointRegistry registry,
        string endpointPath)
    {
        var read = await ToModelServerRequestAsync(context);
        if (await TryWriteJsonRequestReadRejectionAsync(
                context,
                read,
                endpointPath,
                nameof(DesignConversionModelServerHumanApprovalHandler)))
            return;

        var request = read.Request!;
        var invoked = await registry.InvokeAsync(request, context.RequestAborted);
        var response = AnnotateAspNetHost(invoked.IsSuccess
            ? invoked.Value!
            : BuildRegistryErrorResponse(request, invoked.ErrorCode, invoked.ErrorMessage),
            context,
            endpointPath,
            nameof(DesignConversionModelServerHumanApprovalHandler));

        await WriteResponseAsync(context, response);
    }

    private static async Task DispatchArtifactUploadAsync(
        HttpContext context,
        DesignConversionModelServerArtifactUploadHttpHandler handler)
    {
        var request = new DesignConversionModelServerArtifactUploadHttpRequest(
            Method: context.Request.Method,
            Path: context.Request.Path.Value ?? "/",
            Body: context.Request.Body,
            ContentType: context.Request.ContentType,
            ContentLength: context.Request.ContentLength,
            Headers: ToArtifactUploadHeaderDictionary(context.Request.Headers));
        var handled = await handler.HandleAsync(request, context.RequestAborted);
        var response = AnnotateAspNetHost(
            handled,
            context,
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
            nameof(DesignConversionModelServerArtifactUploadHttpHandler),
            publishLiveRouteMappingFlags: true);

        await WriteResponseAsync(context, response);
    }

    private static async Task WriteResponseAsync(
        HttpContext context,
        DesignConversionModelServerHttpResponse response)
    {
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = response.ContentType;
        foreach (var pair in response.Headers)
        {
            if (pair.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                pair.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            context.Response.Headers[pair.Key] = pair.Value;
        }

        await context.Response.WriteAsync(response.BodyJson, context.RequestAborted);
    }

    private static async Task<DesignConversionModelServerHttpResponse?> TryDispatchAdmissionRejectionBeforeBodyAsync(
        HttpContext context,
        DesignConversionModelServerEndpointRegistry registry,
        PreBodyAdmissionRoute admissionRoute)
    {
        var resolved = registry.Resolve(context.Request.Method, context.Request.Path.Value ?? "/");
        if (!resolved.IsSuccess ||
            resolved.Value!.RouteAdmission is not { Enabled: true } routeAdmission)
        {
            return null;
        }

        var request = new DesignConversionModelServerHttpRequest(
            Method: context.Request.Method,
            Path: context.Request.Path.Value ?? "/",
            BodyJson: null,
            ContentType: context.Request.ContentType,
            Headers: context.Request.Headers.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToString(),
                StringComparer.OrdinalIgnoreCase),
            QueryString: context.Request.QueryString.Value);
        var admission = admissionRoute switch
        {
            PreBodyAdmissionRoute.Evaluate => routeAdmission.AdmitEvaluate(request),
            PreBodyAdmissionRoute.Status => routeAdmission.AdmitStatus(request),
            PreBodyAdmissionRoute.IdempotentMutation => routeAdmission.AdmitIdempotentMutation(request),
            _ => throw new ArgumentOutOfRangeException(nameof(admissionRoute), admissionRoute, "Unknown pre-body admission route.")
        };
        if (admission.IsAccepted)
            return null;

        return await resolved.Value.Handler(request, context.RequestAborted);
    }

    private enum PreBodyAdmissionRoute
    {
        Evaluate,
        Status,
        IdempotentMutation
    }

    private static async Task<bool> TryWriteJsonRequestReadRejectionAsync(
        HttpContext context,
        JsonRequestReadResult read,
        string routePath,
        string handlerName,
        bool publishLiveRouteMappingFlags = false)
    {
        if (read.RejectionResponse is null)
            return false;

        var response = AnnotateAspNetHost(
            read.RejectionResponse,
            context,
            routePath,
            handlerName,
            publishLiveRouteMappingFlags,
            registryDispatchVerified: false);
        await WriteResponseAsync(context, response);
        return true;
    }

    private static async Task<JsonRequestReadResult> ToModelServerRequestAsync(HttpContext context)
    {
        var headers = context.Request.Headers.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);

        if (context.Request.ContentLength is > MaximumJsonRequestBodyBytes)
            return JsonRequestReadResult.Rejected(BuildJsonRequestBodyTooLargeResponse(context));

        await using var body = new MemoryStream(
            context.Request.ContentLength is > 0 and <= MaximumJsonRequestBodyBytes
                ? checked((int)context.Request.ContentLength.Value)
                : 0);
        var buffer = new byte[4096];
        while (true)
        {
            var requestedBytes = checked((int)Math.Min(
                buffer.Length,
                MaximumJsonRequestBodyBytes - body.Length + 1));
            var read = await context.Request.Body.ReadAsync(
                buffer.AsMemory(0, requestedBytes),
                context.RequestAborted);
            if (read == 0)
                break;
            if (body.Length + read > MaximumJsonRequestBodyBytes)
                return JsonRequestReadResult.Rejected(BuildJsonRequestBodyTooLargeResponse(context));

            await body.WriteAsync(buffer.AsMemory(0, read), context.RequestAborted);
        }

        string bodyJson;
        try
        {
            bodyJson = StrictUtf8.GetString(body.GetBuffer(), 0, checked((int)body.Length));
        }
        catch (DecoderFallbackException)
        {
            bodyJson = InvalidUtf8JsonSentinel;
        }

        return JsonRequestReadResult.Read(new DesignConversionModelServerHttpRequest(
            Method: context.Request.Method,
            Path: context.Request.Path.Value ?? "/",
            BodyJson: bodyJson,
            ContentType: context.Request.ContentType,
            Headers: headers,
            QueryString: context.Request.QueryString.Value));
    }

    private static async Task<DesignConversionModelServerHttpRequest> ToBoundedModelServerRequestAsync(
        HttpContext context,
        int maximumBodyBytes)
    {
        var headers = context.Request.Headers.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);
        if (context.Request.ContentLength is > 0 && context.Request.ContentLength > maximumBodyBytes)
        {
            return new DesignConversionModelServerHttpRequest(
                Method: context.Request.Method,
                Path: context.Request.Path.Value ?? "/",
                BodyJson: new string('x', maximumBodyBytes + 1),
                ContentType: context.Request.ContentType,
                Headers: headers,
                QueryString: context.Request.QueryString.Value);
        }

        await using var body = new MemoryStream();
        var buffer = new byte[4096];
        while (true)
        {
            var read = await context.Request.Body.ReadAsync(buffer, context.RequestAborted);
            if (read == 0)
                break;
            if (body.Length + read > maximumBodyBytes)
            {
                return new DesignConversionModelServerHttpRequest(
                    Method: context.Request.Method,
                    Path: context.Request.Path.Value ?? "/",
                    BodyJson: new string('x', maximumBodyBytes + 1),
                    ContentType: context.Request.ContentType,
                    Headers: headers,
                    QueryString: context.Request.QueryString.Value);
            }

            await body.WriteAsync(buffer.AsMemory(0, read), context.RequestAborted);
        }

        string bodyJson;
        try
        {
            bodyJson = StrictUtf8.GetString(body.GetBuffer(), 0, checked((int)body.Length));
        }
        catch (DecoderFallbackException)
        {
            // Preserve registry/handler dispatch and its stable error envelope. This sentinel is
            // deliberately malformed JSON, so authenticated invalid UTF-8 deterministically maps
            // to REQUEST_JSON_INVALID instead of escaping the ASP.NET pipeline as a 500.
            bodyJson = InvalidUtf8JsonSentinel;
        }

        return new DesignConversionModelServerHttpRequest(
            Method: context.Request.Method,
            Path: context.Request.Path.Value ?? "/",
            BodyJson: bodyJson,
            ContentType: context.Request.ContentType,
            Headers: headers,
            QueryString: context.Request.QueryString.Value);
    }

    private static IReadOnlyDictionary<string, string> ToArtifactUploadHeaderDictionary(
        IHeaderDictionary headers)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in headers)
        {
            if (!ArtifactUploadDispatchHeaderAllowlist.Contains(pair.Key))
                continue;

            result[pair.Key] = pair.Value.ToString();
        }

        return result;
    }

    private static DesignConversionModelServerHttpResponse AnnotateAspNetHost(
        DesignConversionModelServerHttpResponse response,
        HttpContext context,
        string routePath,
        string handlerName,
        bool publishLiveRouteMappingFlags = false,
        bool registryDispatchVerified = true)
    {
        if (response is null)
            throw new ArgumentNullException(nameof(response));

        var localAddress = context.Connection.LocalIpAddress?.ToString() ?? "unknown";
        var socketOpened = context.Connection.LocalPort > 0;
        var loopbackOnly = IsLoopback(context);
        var headers = new Dictionary<string, string>(response.Headers, StringComparer.Ordinal)
        {
            ["x-design-conversion-asp-net-endpoint-mapper"] = nameof(DesignConversionModelServerAspNetEndpointMapper),
            ["x-design-conversion-asp-net-boundary"] = AspNetEndpointMapperBoundary,
            ["x-design-conversion-asp-net-host-verified"] = bool.TrueString,
            ["x-design-conversion-asp-net-host-kind"] = AspNetHostKind,
            ["x-design-conversion-asp-net-route"] = routePath,
            ["x-design-conversion-localhost-loopback-only"] = loopbackOnly.ToString(),
            ["x-design-conversion-loopback-host"] = localAddress,
            ["x-design-conversion-socket-opened"] = socketOpened.ToString(),
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-public-production-default-ready"] = bool.FalseString,
            ["x-design-conversion-product-ready"] = bool.FalseString
        };
        if (publishLiveRouteMappingFlags)
        {
            headers["x-design-conversion-live-route-registered"] = bool.TrueString;
            headers["x-design-conversion-aspnet-endpoint-mapped"] = bool.TrueString;
            headers["x-design-conversion-asp-net-route-id"] = RouteIdFor(routePath);
            headers["x-design-conversion-asp-net-handler-dispatch-target"] = handlerName;
        }

        return response with
        {
            BodyJson = AnnotateBodyJson(
                response.BodyJson,
                context,
                loopbackOnly,
                socketOpened,
                localAddress,
                routePath,
                handlerName,
                response.StatusCode is >= 200 and < 300,
                publishLiveRouteMappingFlags,
                registryDispatchVerified),
            Headers = headers
        };
    }

    private static string AnnotateBodyJson(
        string bodyJson,
        HttpContext context,
        bool loopbackOnly,
        bool socketOpened,
        string localAddress,
        string routePath,
        string handlerName,
        bool responseDispatchSucceeded,
        bool publishLiveRouteMappingFlags,
        bool registryDispatchVerified)
    {
        try
        {
            if (JsonNode.Parse(bodyJson) is not JsonObject root)
                return bodyJson;

            if (publishLiveRouteMappingFlags)
                ApplyTopLevelLiveRouteMappingFlags(root);

            var metadata = root["boundaryMetadata"] as JsonObject ?? new JsonObject();
            ApplyAspNetMetadata(
                metadata,
                context,
                loopbackOnly,
                socketOpened,
                localAddress,
                routePath,
                handlerName,
                responseDispatchSucceeded,
                publishLiveRouteMappingFlags,
                registryDispatchVerified);
            root["boundaryMetadata"] = metadata;
            return root.ToJsonString(DesignConversionModelServerHttpHandler.JsonOptions);
        }
        catch (JsonException)
        {
            return bodyJson;
        }
    }

    private static void ApplyTopLevelLiveRouteMappingFlags(JsonObject root)
    {
        if (root.ContainsKey("liveHttpRouteRegistered") ||
            root.ContainsKey("aspNetEndpointMapped"))
        {
            root["liveHttpRouteRegistered"] = true;
            root["aspNetEndpointMapped"] = true;
        }
    }

    private static void ApplyAspNetMetadata(
        JsonObject metadata,
        HttpContext context,
        bool loopbackOnly,
        bool socketOpened,
        string localAddress,
        string routePath,
        string handlerName,
        bool responseDispatchSucceeded,
        bool publishLiveRouteMappingFlags,
        bool registryDispatchVerified)
    {
        var routeVerified = IsAspNetRouteVerified(
            context,
            loopbackOnly,
            socketOpened,
            routePath,
            handlerName,
            responseDispatchSucceeded);

        metadata["asp_net_endpoint_mapper"] = nameof(DesignConversionModelServerAspNetEndpointMapper);
        metadata["asp_net_endpoint_mapper_boundary"] = AspNetEndpointMapperBoundary;
        metadata["asp_net_host_verified"] = bool.TrueString;
        metadata["asp_net_minimal_api_pipeline_verified"] = bool.TrueString;
        metadata["asp_net_route_matched"] = bool.TrueString;
        metadata["asp_net_route_path"] = routePath;
        metadata["asp_net_route_id"] = RouteIdFor(routePath);
        metadata["asp_net_route_verified"] = routeVerified.ToString();
        metadata["asp_net_route_verification_authority"] = AspNetRouteVerificationAuthority;
        metadata["asp_net_route_verification_state"] = routeVerified
            ? AspNetRouteVerifiedState
            : "not_verified_server_owned_loopback_route_dispatch";
        metadata["asp_net_route_verification_route_id"] = RouteIdFor(routePath);
        metadata["asp_net_route_verification_route_path"] = routePath;
        metadata["asp_net_route_verification_successful_handler_dispatch"] =
            responseDispatchSucceeded.ToString();
        metadata["asp_net_registry_dispatch_verified"] = registryDispatchVerified.ToString();
        metadata["asp_net_handler_dispatch_target"] = handlerName;
        metadata["asp_net_host_kind"] = AspNetHostKind;
        metadata["localhost_loopback_only"] = loopbackOnly.ToString();
        metadata["loopback_host"] = localAddress;
        metadata["asp_net_local_port"] = context.Connection.LocalPort.ToString(CultureInfo.InvariantCulture);
        metadata["bound_port_mode"] = socketOpened ? "asp_net_runtime_bound" : "not_socket_bound";
        metadata["socket_opened"] = socketOpened.ToString();
        metadata["asp_net_kestrel_socket_opened"] = socketOpened.ToString();
        metadata["local_loopback_http_call_executed"] = loopbackOnly.ToString();
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        metadata["public_production_default_ready"] = bool.FalseString;
        DesignConversionModelServerNetworkBoundary.PreserveTrustedNetworkBoundaryMetadata(metadata);
        if (publishLiveRouteMappingFlags)
        {
            metadata["live_http_route_registered"] = bool.TrueString;
            metadata["aspnet_endpoint_mapped"] = bool.TrueString;
        }
        ApplyDefaultSubmitMetadata(metadata, context);
        ApplyDurableReplayStableResponseMetadata(metadata);
        metadata["product_ready"] = bool.FalseString;
        metadata["refs_only_public_response"] = bool.TrueString;
        metadata["raw_html_included"] = bool.FalseString;
        metadata["raw_css_included"] = bool.FalseString;
        metadata["raw_design_tree_included"] = bool.FalseString;
        metadata["response_contains_raw_private_data"] = bool.FalseString;
    }

    private static bool IsAspNetRouteVerified(
        HttpContext context,
        bool loopbackOnly,
        bool socketOpened,
        string routePath,
        string handlerName,
        bool responseDispatchSucceeded)
        => responseDispatchSucceeded &&
           loopbackOnly &&
           socketOpened &&
           context.Connection.LocalPort is > 0 and <= 65535 &&
           MapperRouteMatchesCurrentRequest(context, routePath) &&
           IsMapperSuppliedDispatchTarget(routePath, handlerName);

    private static bool MapperRouteMatchesCurrentRequest(HttpContext context, string routePath)
    {
        var requestPath = NormalizeRoutePath(context.Request.Path.Value);
        if (string.Equals(
                routePath,
                DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
                StringComparison.Ordinal))
        {
            return requestPath.StartsWith(
                       DesignConversionModelServerStatusHandler.EndpointPathPrefix,
                       StringComparison.Ordinal) &&
                   requestPath.Length > DesignConversionModelServerStatusHandler.EndpointPathPrefix.Length;
        }

        if (string.Equals(
                routePath,
                DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
                StringComparison.Ordinal))
        {
            return DesignConversionModelServerVisualCostHandler.IsConcreteVisualCostReadbackPath(requestPath);
        }

        return string.Equals(requestPath, routePath, StringComparison.Ordinal);
    }

    private static bool IsMapperSuppliedDispatchTarget(string routePath, string handlerName)
        => routePath switch
        {
            DesignConversionModelServerEndpointRegistry.EndpointPath or
                DesignConversionModelServerEndpointRegistry.InternalEndpointPath
                => string.Equals(handlerName, nameof(DesignConversionModelServerHttpHandler), StringComparison.Ordinal),
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath
                => string.Equals(handlerName, nameof(DesignConversionModelServerImprovementLoopHandler), StringComparison.Ordinal),
            DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath
                => string.Equals(handlerName, nameof(DesignConversionModelServerStatusHandler), StringComparison.Ordinal),
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath
                => string.Equals(handlerName, nameof(DesignConversionModelServerVisualCostHandler), StringComparison.Ordinal),
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath
                => string.Equals(handlerName, nameof(DesignConversionModelServerDpoUsefulnessHandler), StringComparison.Ordinal),
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath
                => string.Equals(handlerName, nameof(DesignConversionModelServerTrainingIntakeHandler), StringComparison.Ordinal),
            DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath
                => string.Equals(handlerName, nameof(DesignConversionModelServerFlow38QuarantineHandler), StringComparison.Ordinal),
            DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath
                => string.Equals(handlerName, nameof(DesignConversionModelServerModelActivationHandler), StringComparison.Ordinal),
            DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath or
                DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath
                => string.Equals(handlerName, nameof(DesignConversionModelServerHumanApprovalHandler), StringComparison.Ordinal),
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath
                => string.Equals(handlerName, nameof(DesignConversionModelServerArtifactUploadHttpHandler), StringComparison.Ordinal),
            _ => false
        };

    private static string NormalizeRoutePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? trimmed[..queryIndex] : trimmed;
    }

    private static string RouteIdFor(string routePath)
        => routePath switch
        {
            DesignConversionModelServerEndpointRegistry.EndpointPath => "design_conversion_evaluate",
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath => "design_conversion_internal_evaluate",
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath => "design_conversion_improvement_loop",
            DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath => "design_conversion_status_readback",
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath => "design_conversion_visual_cost_readback",
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath => "design_conversion_dpo_usefulness",
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath => "design_conversion_training_intake",
            DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath => "design_conversion_flow38_quarantine",
            DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath => "design_conversion_model_activation",
            DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath => "design_conversion_human_approval_challenge",
            DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath => "design_conversion_human_approval_approve",
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath => "design_conversion_artifact_upload",
            _ => "unknown"
        };

    private static void ApplyDefaultSubmitMetadata(JsonObject metadata, HttpContext context)
    {
        var trustedBaseUrl = TryCreateTrustedAspNetLocalBaseUrl(context);
        if (trustedBaseUrl is null)
        {
            ApplyMissingDefaultSubmitMetadata(metadata);
            return;
        }

        metadata["default_submit_endpoint_route"] = DesignConversionModelServerEndpointRegistry.EndpointRoute;
        metadata["default_submit_endpoint_method"] = DesignConversionModelServerEndpointRegistry.EndpointMethod;
        metadata["default_submit_endpoint_path"] = DesignConversionModelServerEndpointRegistry.EndpointPath;
        metadata["default_submit_base_url_configured"] = bool.TrueString;
        metadata["default_submit_base_url_missing"] = bool.FalseString;
        metadata["default_submit_base_url_status"] = DesignConversionModelServerReadyJsonSubmitClient.ConfiguredSubmitStatus;
        metadata["default_submit_base_url_missing_setting"] = string.Empty;
        metadata["default_submit_base_url_next_action"] = string.Empty;
        metadata["default_submit_base_url_loopback"] = bool.TrueString;
        metadata["default_submit_base_url_source"] = AspNetLiveLocalBaseUrlSource;
        metadata["default_submit_base_url"] = FormatBaseUrl(trustedBaseUrl.BaseUrl);
        metadata["default_submit_base_url_scheme"] = trustedBaseUrl.BaseUrl.Scheme;
        metadata["default_submit_base_url_path_base"] = trustedBaseUrl.PathBase;
        metadata["default_submit_base_url_authority"] = trustedBaseUrl.BaseUrl.GetLeftPart(UriPartial.Authority);
        metadata["default_submit_request_uri"] = trustedBaseUrl.SubmitUri.ToString();
        metadata["flow47_verified"] = bool.TrueString;
        metadata["flow47_verification_authority"] = DesignConversionModelServerCallerContract.Flow47VerificationAuthority;
        metadata["flow47_verification_state"] = "verified_by_asp_net_loopback_boundary";
        metadata["flow47_submit_status"] = DesignConversionModelServerReadyJsonSubmitClient.ConfiguredFlow47SubmitStatus;
        metadata["flow47_missing_setting_names"] = string.Empty;
        metadata["flow47_next_action"] = string.Empty;
        metadata["flow47_base_url_source"] = AspNetLiveLocalBaseUrlSource;
        metadata["flow47_base_url"] = FormatBaseUrl(trustedBaseUrl.BaseUrl);
        metadata["flow47_endpoint_path"] = DesignConversionModelServerEndpointRegistry.EndpointPath;
        metadata["flow47_submit_request_uri"] = trustedBaseUrl.SubmitUri.ToString();
        metadata["flow47_live_local_loopback_verified"] = bool.TrueString;
    }

    private static void ApplyMissingDefaultSubmitMetadata(JsonObject metadata)
    {
        metadata["default_submit_endpoint_route"] = DesignConversionModelServerEndpointRegistry.EndpointRoute;
        metadata["default_submit_endpoint_method"] = DesignConversionModelServerEndpointRegistry.EndpointMethod;
        metadata["default_submit_endpoint_path"] = DesignConversionModelServerEndpointRegistry.EndpointPath;
        metadata["default_submit_base_url_configured"] = bool.FalseString;
        metadata["default_submit_base_url_missing"] = bool.TrueString;
        metadata["default_submit_base_url_status"] = DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus;
        metadata["default_submit_base_url_missing_setting"] = "BaseUrl";
        metadata["default_submit_base_url_next_action"] = DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlNextAction;
        metadata["default_submit_base_url_loopback"] = bool.FalseString;
        metadata["default_submit_base_url_source"] = "untrusted_asp_net_context";
        metadata["default_submit_base_url"] = string.Empty;
        metadata["default_submit_base_url_authority"] = string.Empty;
        metadata["default_submit_request_uri"] = string.Empty;
        metadata["flow47_verification_authority"] = DesignConversionModelServerCallerContract.Flow47VerificationAuthority;
        metadata["flow47_verification_state"] = "not_evaluated_untrusted_asp_net_context";
        metadata["flow47_submit_status"] = DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus;
        metadata["flow47_missing_setting_names"] = "BaseUrl";
        metadata["flow47_next_action"] = DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlNextAction;
        metadata["flow47_base_url_source"] = "untrusted_asp_net_context";
        metadata["flow47_base_url"] = string.Empty;
        metadata["flow47_endpoint_path"] = DesignConversionModelServerEndpointRegistry.EndpointPath;
        metadata["flow47_submit_request_uri"] = string.Empty;
        metadata["flow47_live_local_loopback_verified"] = bool.FalseString;
    }

    private static void ApplyDurableReplayStableResponseMetadata(JsonObject metadata)
    {
        if (!IsTruthy(metadata, "idempotency_durable_replay_enabled") &&
            !IsTruthy(metadata, "route_admission_enabled"))
            return;

        metadata["asp_net_local_port"] = string.Empty;
        metadata["bound_port_mode"] = "durable_replay_response_redacted";
        metadata["default_submit_base_url_source"] = "durable_replay_response_stable_redacted";
        metadata["default_submit_base_url"] = string.Empty;
        metadata["default_submit_base_url_authority"] = string.Empty;
        metadata["default_submit_request_uri"] = string.Empty;
        metadata["flow47_base_url_source"] = "durable_replay_response_stable_redacted";
        metadata["flow47_base_url"] = string.Empty;
        metadata["flow47_submit_request_uri"] = string.Empty;
        metadata["idempotency_durable_replay_response_metadata_stable"] = bool.TrueString;
        metadata["idempotency_durable_replay_response_volatiles_redacted"] = bool.TrueString;
        metadata["idempotency_durable_replay_response_fake_endpoint"] = bool.FalseString;
    }

    private static bool IsTruthy(JsonObject metadata, string key)
        => metadata.TryGetPropertyValue(key, out var value) &&
           string.Equals(value?.ToString(), bool.TrueString, StringComparison.OrdinalIgnoreCase);

    private static TrustedAspNetLocalBaseUrl? TryCreateTrustedAspNetLocalBaseUrl(HttpContext context)
    {
        var scheme = context.Request.Scheme?.Trim();
        if (!IsHttpScheme(scheme))
            return null;

        var localAddress = context.Connection.LocalIpAddress;
        if (localAddress is null || !IPAddress.IsLoopback(localAddress))
            return null;

        var localPort = context.Connection.LocalPort;
        if (localPort is <= 0 or > 65535)
            return null;

        if (!IsSafePathBase(context.Request.PathBase))
            return null;

        var pathBase = NormalizePathBase(context.Request.PathBase);
        var builder = new UriBuilder(scheme!, localAddress.ToString(), localPort)
        {
            Path = pathBase.Length == 0 ? "/" : pathBase + "/"
        };
        var baseUrl = builder.Uri;
        return new TrustedAspNetLocalBaseUrl(
            baseUrl,
            BuildEndpointUri(baseUrl, DesignConversionModelServerEndpointRegistry.EndpointPath),
            pathBase);
    }

    private static bool IsHttpScheme(string? scheme)
        => string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

    private static bool IsSafePathBase(PathString pathBase)
    {
        if (!pathBase.HasValue || string.IsNullOrEmpty(pathBase.Value))
            return true;

        var value = pathBase.Value;
        return value.StartsWith("/", StringComparison.Ordinal) &&
               !value.Contains("\\", StringComparison.Ordinal) &&
               !value.Contains("//", StringComparison.Ordinal) &&
               !value.Contains("..", StringComparison.Ordinal) &&
               value.IndexOfAny(new[] { '?', '#' }) < 0;
    }

    private static string NormalizePathBase(PathString pathBase)
    {
        if (!pathBase.HasValue || string.IsNullOrEmpty(pathBase.Value))
            return string.Empty;

        return pathBase.ToUriComponent().TrimEnd('/');
    }

    private static Uri BuildEndpointUri(Uri baseUrl, string endpointPath)
        => new(baseUrl, endpointPath.TrimStart('/'));

    private static string FormatBaseUrl(Uri baseUrl)
        => baseUrl.ToString().TrimEnd('/');

    private static DesignConversionModelServerHttpResponse BuildJsonRequestBodyTooLargeResponse(
        HttpContext context)
    {
        var method = string.IsNullOrWhiteSpace(context.Request.Method)
            ? "UNKNOWN"
            : context.Request.Method.Trim().ToUpperInvariant();
        var path = string.IsNullOrWhiteSpace(context.Request.Path.Value)
            ? "/"
            : context.Request.Path.Value!.Trim();
        var statusCode = StatusCodes.Status413PayloadTooLarge;
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = $"{method} {path}",
            ["http_method"] = method,
            ["http_path"] = path,
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["runtime_invoked"] = bool.FalseString,
            ["refs_only_public_response"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["asp_net_json_request_body_limit_bytes"] =
                MaximumJsonRequestBodyBytes.ToString(CultureInfo.InvariantCulture),
            ["asp_net_request_body_rejected_before_registry"] = bool.TrueString
        };
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: path,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: JsonRequestBodyTooLargeErrorCode,
            ErrorMessage: JsonRequestBodyTooLargeErrorMessage,
            BoundaryMetadata: metadata);

        return new DesignConversionModelServerHttpResponse(
            StatusCode: statusCode,
            ContentType: "application/json; charset=utf-8",
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-endpoint"] = path,
                ["x-design-conversion-http-seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
                ["x-design-conversion-status-code"] = statusCode.ToString(CultureInfo.InvariantCulture)
            });
    }

    private static DesignConversionModelServerHttpResponse BuildRegistryErrorResponse(
        DesignConversionModelServerHttpRequest request,
        string? errorCode,
        string? errorMessage)
    {
        var method = string.IsNullOrWhiteSpace(request.Method)
            ? "UNKNOWN"
            : request.Method.Trim().ToUpperInvariant();
        var path = string.IsNullOrWhiteSpace(request.Path)
            ? "/"
            : request.Path.Trim();
        var statusCode = StatusCodeForRegistryError(errorCode);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = $"{method} {path}",
            ["http_method"] = method,
            ["http_path"] = path,
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["runtime_invoked"] = bool.FalseString,
            ["refs_only_public_response"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString
        };

        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: path,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: string.IsNullOrWhiteSpace(errorCode)
                ? "DESIGN_CONVERSION_ASPNET_ROUTE_REGISTRY_ERROR"
                : errorCode,
            ErrorMessage: string.IsNullOrWhiteSpace(errorMessage)
                ? "DesignConversion endpoint registry rejected the ASP.NET host request."
                : errorMessage,
            BoundaryMetadata: metadata);

        return new DesignConversionModelServerHttpResponse(
            StatusCode: statusCode,
            ContentType: "application/json; charset=utf-8",
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-endpoint"] = path,
                ["x-design-conversion-http-seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
                ["x-design-conversion-status-code"] = statusCode.ToString(CultureInfo.InvariantCulture)
            });
    }

    private static bool IsLoopback(HttpContext context)
    {
        var local = context.Connection.LocalIpAddress;
        var remote = context.Connection.RemoteIpAddress;
        return local is not null &&
               System.Net.IPAddress.IsLoopback(local) &&
               (remote is null || System.Net.IPAddress.IsLoopback(remote));
    }

    private static int StatusCodeForRegistryError(string? errorCode)
        => errorCode switch
        {
            "DESIGN_CONVERSION_ENDPOINT_REQUEST_REQUIRED" => 400,
            "DESIGN_CONVERSION_ENDPOINT_METHOD_REQUIRED" => 400,
            "DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND" => 404,
            DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode => 500,
            _ => 502
        };

    private sealed record JsonRequestReadResult(
        DesignConversionModelServerHttpRequest? Request,
        DesignConversionModelServerHttpResponse? RejectionResponse)
    {
        public static JsonRequestReadResult Read(DesignConversionModelServerHttpRequest request)
            => new(request, null);

        public static JsonRequestReadResult Rejected(DesignConversionModelServerHttpResponse response)
            => new(null, response);
    }

    private sealed record TrustedAspNetLocalBaseUrl(
        Uri BaseUrl,
        Uri SubmitUri,
        string PathBase);
}
