// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LlmFromScratch.DynamicGraphCompositionLiveLocalServer;
using Microsoft.AspNetCore.Http.Features;
using OrchestratorModel.Logical;

string? readyFileForStartupCleanup = null;
DynamicGraphCompositionServerOptions options;
try
{
    readyFileForStartupCleanup =
        DynamicGraphCompositionServerOptions.FindReadyFileForStartupCleanup(args);
    options = DynamicGraphCompositionServerOptions.Parse(args);
    ReadyFile.RemoveStale(options.ReadyFile);
}
catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
{
    if (readyFileForStartupCleanup is not null)
        ReadyFile.RemoveSafe(readyFileForStartupCleanup);
    Console.Error.WriteLine("STARTUP_CONFIGURATION_INVALID");
    Environment.ExitCode = 2;
    return;
}
WebApplicationBuilder builder;
try
{
    var contentRoot = Path.GetDirectoryName(options.ReadyFile)!;
    DynamicGraphCompositionFileSystemPolicy.EnsureSafeExistingComponents(contentRoot);
    Directory.CreateDirectory(contentRoot);
    DynamicGraphCompositionFileSystemPolicy.EnsureDirectory(contentRoot);
    builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = Array.Empty<string>(),
        ApplicationName = typeof(Program).Assembly.FullName,
        ContentRootPath = contentRoot,
    });
    builder.WebHost.UseUrls(DynamicGraphCompositionServerOptions.LoopbackUrl);
    builder.WebHost.ConfigureKestrel(kestrel => kestrel.Limits.MaxRequestBodySize = options.MaxRequestBodyBytes);
    builder.Logging.ClearProviders();
    builder.Services.AddSingleton(options);
}
catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
{
    Console.Error.WriteLine("NOT_READY");
    Environment.ExitCode = 3;
    return;
}
DynamicGraphCompositionSessionPersistence persistenceOwner;
try
{
    persistenceOwner = new DynamicGraphCompositionSessionPersistence(options);
}
catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
{
    Console.Error.WriteLine("NOT_READY");
    Environment.ExitCode = 3;
    return;
}
WebApplication? appCleanupOwner = null;
DynamicGraphCompositionSessionRegistry? registryCleanupOwner = null;
StartupReadinessState? readinessCleanupOwner = null;
Exception? hostFailure = null;
try
{
builder.Services.AddSingleton(persistenceOwner);
var registryOwner = new DynamicGraphCompositionSessionRegistry(options, persistenceOwner);
registryCleanupOwner = registryOwner;
builder.Services.AddSingleton(registryOwner);
builder.Services.AddSingleton<IDynamicGraphLogicalOrchestratorCompositionExecutor,
    DynamicGraphLogicalOrchestratorCompositionExecutorAdapter>();
builder.Services.AddSingleton<PromptToTaskUntrainedHonestyExecutionService>();

WebApplication app;
try
{
    app = builder.Build();
    appCleanupOwner = app;
}
catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
{
    Console.Error.WriteLine("NOT_READY");
    Environment.ExitCode = 3;
    return;
}
var readiness = new StartupReadinessState(options.ReadyFile, persistenceOwner.IsEnabled);
readinessCleanupOwner = readiness;
var promptToTaskUntrainedHonestyExecutionOwner =
    app.Services.GetRequiredService<PromptToTaskUntrainedHonestyExecutionService>();
readiness.RegisterInvalidation(registryOwner.InvalidateAll);
registryOwner.RegisterPersistenceFailure(readiness.FailClosed);
var executorOwner = app.Services.GetRequiredService<IDynamicGraphLogicalOrchestratorCompositionExecutor>();
if (persistenceOwner.IsEnabled)
{
    try
    {
        registryOwner.Restore(profile => Compose(executorOwner, profile, readiness, requireReady: false));
        if (!readiness.AllowsComposition(requireReady: false))
        {
            registryOwner.InvalidateAll();
            readiness.FailClosed("FORBIDDEN_TRAINING_ASSEMBLY_LOADED");
        }
    }
    catch (ForbiddenTrainingAssemblyException)
    {
        readiness.FailClosed("FORBIDDEN_TRAINING_ASSEMBLY_LOADED");
    }
    catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        readiness.FailClosed("HOST_SESSION_RESTORE_FAILED");
    }
}

app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store";
    var suppliedRequestId = ReadSingleSafeHeader(context.Request, "X-Request-Id", 1, 128);
    var requestId = suppliedRequestId.State == SingleSafeHeaderState.Valid
        ? suppliedRequestId.Value!
        : Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
    context.Items["request-id"] = requestId;
    if (suppliedRequestId.State == SingleSafeHeaderState.Invalid)
    {
        await Error(
                StatusCodes.Status400BadRequest,
                requestId,
                "HOST_REQUEST_ID_INVALID",
                "A safe single X-Request-Id header is required.")
            .ExecuteAsync(context);
        return;
    }
    await next(context);
});

app.MapGet("/api/v1/dynamic-graph/health", () =>
    Results.Ok(new ReadinessHttpResponse(
        "live",
        Persistent: persistenceOwner.IsEnabled,
        AppliesGraph: !persistenceOwner.IsEnabled)));
app.MapGet("/api/v1/dynamic-graph/readiness", () => readiness.IsReady
    ? Results.Ok(new ReadinessHttpResponse(
        "ready",
        Persistent: persistenceOwner.IsEnabled,
        AppliesGraph: !persistenceOwner.IsEnabled))
    : Results.Json(
        new ReadinessHttpResponse(
            "not_ready",
            readiness.ReasonCode,
            Persistent: persistenceOwner.IsEnabled,
            AppliesGraph: !persistenceOwner.IsEnabled),
        statusCode: StatusCodes.Status503ServiceUnavailable));

app.MapPost("/api/v1/dynamic-graph/l2-untrained-honesty-runs", async (
    HttpContext context,
    CancellationToken cancellationToken) =>
{
    var requestId = RequestId(context);
    if (!readiness.IsReady)
        return Error(503, requestId, readiness.ReasonCode ?? "HOST_NOT_READY", "The local composition server is not ready.");
    if (!context.Request.HasJsonContentType())
        return Error(415, requestId, "HOST_JSON_CONTENT_TYPE_REQUIRED", "The request content type must be JSON.");
    if (context.Request.ContentLength > options.MaxRequestBodyBytes)
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

    var tenantCheck = ValidateConfiguredTenant(context, options, requestId);
    if (tenantCheck.Error is not null)
        return tenantCheck.Error;

    PromptToTaskUntrainedHonestyRunHttpRequest? body;
    try
    {
        body = await HttpJson.DeserializeStrictAsync<PromptToTaskUntrainedHonestyRunHttpRequest>(
            context.Request.Body,
            cancellationToken);
    }
    catch (JsonException exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(
            400,
            requestId,
            "HOST_L2_UNTRAINED_HONESTY_BODY_INVALID",
            "A valid L2 untrained honesty request body is required.");
    }
    catch (BadHttpRequestException exception) when (
        exception.StatusCode == StatusCodes.Status413PayloadTooLarge
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        if (!context.Response.HasStarted) context.Response.Clear();
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        return Error(
            408,
            requestId,
            "HOST_L2_UNTRAINED_HONESTY_REQUEST_CANCELLED",
            "The L2 untrained honesty request was cancelled.");
    }

    if (body is null
        || !DynamicGraphCompositionServerOptions.IsSafeIdentifier(
            body.EvaluationId,
            1,
            128)
        || !DynamicGraphCompositionServerOptions.IsSafeIdentifier(body.RunId, 1, 128)
        || !DynamicGraphCompositionServerOptions.IsSafeIdentifier(body.PromptId, 1, 128)
        || !DynamicGraphCompositionServerOptions.IsSafeIdentifier(body.UserLanguage, 1, 32)
        || !IsSafeSelectedTargetInput(body.RawPrompt))
    {
        return Error(
            400,
            requestId,
            "HOST_L2_UNTRAINED_HONESTY_REQUEST_INVALID",
            "Safe evaluation_id, run_id, prompt_id, raw_prompt, and user_language values are required.");
    }

    OperationResult<PromptToTaskUntrainedHonestyExecutionResult> operation;
    try
    {
        operation = await promptToTaskUntrainedHonestyExecutionOwner.ExecuteAsync(
            body.EvaluationId!,
            body.RunId!,
            body.PromptId!,
            body.RawPrompt!,
            body.UserLanguage!,
            cancellationToken);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        return Error(
            408,
            requestId,
            "HOST_L2_UNTRAINED_HONESTY_REQUEST_CANCELLED",
            "The L2 untrained honesty request was cancelled.");
    }
    catch (Exception exception) when (
        SnapshotExceptionGraphClassifier.IsNonFatal(exception))
    {
        return Error(
            500,
            requestId,
            "HOST_L2_UNTRAINED_HONESTY_EXECUTION_FAILED",
            "The L2 untrained honesty execution failed safely.");
    }

    if (operation is null || (operation.IsSuccess && operation.Value is null))
    {
        return Error(
            500,
            requestId,
            "HOST_L2_UNTRAINED_HONESTY_RESULT_REQUIRED",
            "The L2 untrained honesty execution failed safely.");
    }
    if (!operation.IsSuccess)
    {
        return Error(
            500,
            requestId,
            StringComparer.Ordinal.Equals(
                operation.ErrorCode,
                PromptToTaskUntrainedHonestyExecutionService.ResultRequiredCode)
                ? "HOST_L2_UNTRAINED_HONESTY_RESULT_REQUIRED"
                : "HOST_L2_UNTRAINED_HONESTY_EXECUTION_FAILED",
            "The L2 untrained honesty execution failed safely.");
    }

    var executed = operation.Value!;
    if (executed.Status == PromptToTaskUntrainedHonestyExecutionStatus.Cancelled)
    {
        return Error(
            408,
            requestId,
            "HOST_L2_UNTRAINED_HONESTY_REQUEST_CANCELLED",
            "The L2 untrained honesty request was cancelled.");
    }
    if (executed.Status == PromptToTaskUntrainedHonestyExecutionStatus.ConcurrencyLimited)
    {
        return Error(
            429,
            requestId,
            "HOST_L2_UNTRAINED_HONESTY_CONCURRENCY_EXHAUSTED",
            "The bounded L2 untrained honesty execution concurrency is exhausted.");
    }
    if (executed.Status != PromptToTaskUntrainedHonestyExecutionStatus.Measured
        || executed.Measurement is null)
    {
        return Error(
            500,
            requestId,
            "HOST_L2_UNTRAINED_HONESTY_RESULT_REQUIRED",
            "The L2 untrained honesty execution failed safely.");
    }
    if (!readiness.IsReady)
    {
        return Error(
            503,
            requestId,
            readiness.ReasonCode ?? "HOST_NOT_READY",
            "The local composition server is not ready.");
    }

    return PromptToTaskUntrainedHonestyRun(
        requestId,
        body.EvaluationId!,
        executed);
});

app.MapPost("/api/v1/dynamic-graph/sessions", async (
    HttpContext context,
    DynamicGraphCompositionSessionRegistry registry,
    IDynamicGraphLogicalOrchestratorCompositionExecutor executor,
    CancellationToken cancellationToken) =>
{
    var requestId = RequestId(context);
    if (!readiness.IsReady)
        return Error(503, requestId, readiness.ReasonCode ?? "HOST_NOT_READY", "The local composition server is not ready.");
    if (!context.Request.HasJsonContentType())
        return Error(415, requestId, "HOST_JSON_CONTENT_TYPE_REQUIRED", "The request content type must be JSON.");
    if (context.Request.ContentLength > options.MaxRequestBodyBytes)
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

    var tenantCheck = ValidateConfiguredTenant(context, options, requestId);
    if (tenantCheck.Error is not null)
        return tenantCheck.Error;

    var idempotencyKey = ReadSingleSafeHeader(
        context.Request,
        "Idempotency-Key",
        8,
        DynamicGraphCompositionServerOptions.MaxIdempotencyKeyLength);
    if (idempotencyKey.State != SingleSafeHeaderState.Valid)
        return Error(400, requestId, "HOST_IDEMPOTENCY_KEY_INVALID", "A safe 8-128 character Idempotency-Key is required.");

    CreateCompositionSessionHttpRequest? body;
    try
    {
        body = await HttpJson.DeserializeStrictAsync<CreateCompositionSessionHttpRequest>(
            context.Request.Body, cancellationToken);
    }
    catch (JsonException exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(400, requestId, "HOST_REQUEST_BODY_INVALID", "A valid composition request body is required.");
    }
    catch (BadHttpRequestException exception) when (
        exception.StatusCode == StatusCodes.Status413PayloadTooLarge
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        if (!context.Response.HasStarted) context.Response.Clear();
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }

    if (body?.TrainedInput is not null)
        return Error(422, requestId, "HOST_TRAINED_NODE_INPUT_FORBIDDEN", "Trained node input is forbidden on this deterministic composition boundary.");
    var admission = DynamicGraphCompositionProfilePolicy.Admit(body?.Profile, options.TenantId);
    if (!admission.IsAccepted)
        return Error(admission.StatusCode, requestId, admission.Code, admission.Message);
    var admittedProfile = admission.Profile!;
    var digest = admission.CanonicalDigest!;

    SessionCreationResult created;
    try
    {
        created = await registry.CreateAsync(
            options.TenantId,
            idempotencyKey.Value!,
            digest,
            admittedProfile,
            _ => Task.FromResult(Compose(executor, admittedProfile, readiness, requireReady: true)),
            () => readiness.IsReady,
            cancellationToken);
    }
    catch (ForbiddenTrainingAssemblyException)
    {
        return Error(503, requestId, "FORBIDDEN_TRAINING_ASSEMBLY_LOADED", "The local composition server is not ready.");
    }
    catch (CompositionRejectedException exception)
    {
        return Error(
            StatusForDownstreamCode(exception.Code), requestId, exception.Code,
            StatusForDownstreamCode(exception.Code) == 500
                ? "The deterministic composition failed safely."
                : "The deterministic composition profile was rejected.");
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }
    catch (Exception exception) when (IsNonFatalException(exception))
    {
        return Error(500, requestId, "HOST_COMPOSITION_EXCEPTION", "The deterministic composition failed safely.");
    }
    if (!readiness.IsReady)
    {
        registry.RollbackPublished(created.Session);
        return Error(
            503,
            requestId,
            readiness.ReasonCode ?? "HOST_NOT_READY",
            "The local composition server is not ready.");
    }

    return created.Status switch
    {
        SessionCreationStatus.Created => Session(
            context, requestId, created.Session!, 201, "created", persistenceOwner.IsEnabled),
        SessionCreationStatus.Replayed => Session(
            context, requestId, created.Session!, 200, "replayed", persistenceOwner.IsEnabled),
        SessionCreationStatus.Conflict => Error(409, requestId, "HOST_IDEMPOTENCY_CONFLICT", "The idempotency key is bound to another request."),
        SessionCreationStatus.Pending => Error(409, requestId, "HOST_IDEMPOTENCY_PENDING", "The idempotency key has a composition in progress."),
        SessionCreationStatus.CapacityExceeded => Error(503, requestId, "HOST_SESSION_CAPACITY_EXHAUSTED", "The bounded local session capacity is exhausted."),
        SessionCreationStatus.ForbiddenAssembly => Error(503, requestId, "FORBIDDEN_TRAINING_ASSEMBLY_LOADED", "The local composition server is not ready."),
        SessionCreationStatus.PersistenceFailure => Error(503, requestId, "HOST_SESSION_PERSISTENCE_FAILED", "The local composition server is not ready."),
        _ => Error(429, requestId, "HOST_COMPOSITION_CONCURRENCY_EXHAUSTED", "The bounded local composition concurrency is exhausted."),
    };
});

app.MapPost("/api/v1/dynamic-graph/catalog-sessions", async (
    HttpContext context,
    DynamicGraphCompositionSessionRegistry registry,
    IDynamicGraphLogicalOrchestratorCompositionExecutor executor,
    CancellationToken cancellationToken) =>
{
    var requestId = RequestId(context);
    if (!readiness.IsReady)
        return Error(503, requestId, readiness.ReasonCode ?? "HOST_NOT_READY", "The local composition server is not ready.");
    if (!context.Request.HasJsonContentType())
        return Error(415, requestId, "HOST_JSON_CONTENT_TYPE_REQUIRED", "The request content type must be JSON.");
    if (context.Request.ContentLength > options.MaxRequestBodyBytes)
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

    var tenantCheck = ValidateConfiguredTenant(context, options, requestId);
    if (tenantCheck.Error is not null)
        return tenantCheck.Error;

    var idempotencyKey = ReadSingleSafeHeader(
        context.Request,
        "Idempotency-Key",
        8,
        DynamicGraphCompositionServerOptions.MaxIdempotencyKeyLength);
    if (idempotencyKey.State != SingleSafeHeaderState.Valid)
        return Error(400, requestId, "HOST_IDEMPOTENCY_KEY_INVALID", "A safe 8-128 character Idempotency-Key is required.");

    if (!TryReadSelectedProfileIdQuery(context.Request, out var selectedProfileId))
    {
        return Error(
            400,
            requestId,
            "HOST_SELECTED_PROFILE_QUERY_INVALID",
            "An exact single selected_profile_id query parameter is required.");
    }

    var requestBodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (requestBodySizeFeature is { IsReadOnly: false })
        requestBodySizeFeature.MaxRequestBodySize = null;

    OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> selection;
    var rentedBody = ArrayPool<byte>.Shared.Rent(options.MaxRequestBodyBytes + 1);
    try
    {
        BoundedRequestBodyRead bodyRead;
        try
        {
            bodyRead = await ReadBoundedRequestBodyAsync(
                context.Request.Body,
                rentedBody,
                options.MaxRequestBodyBytes,
                context.RequestAborted);
        }
        catch (BadHttpRequestException exception) when (
            exception.StatusCode == StatusCodes.Status413PayloadTooLarge
            && FatalExceptionPolicy.IsNonFatal(exception))
        {
            if (!context.Response.HasStarted) context.Response.Clear();
            return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");
        }
        catch (OperationCanceledException exception) when (
            context.RequestAborted.IsCancellationRequested
            && FatalExceptionPolicy.IsNonFatal(exception))
        {
            return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
        }
        catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
        {
            return Error(400, requestId, "HOST_REQUEST_BODY_INVALID", "A valid profile catalog request body is required.");
        }

        if (bodyRead.LimitExceeded)
            return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

        selection = DynamicGraphLogicalOrchestratorProfileCatalog.Select(
            rentedBody.AsMemory(0, bodyRead.Count),
            selectedProfileId);
    }
    finally
    {
        CryptographicOperations.ZeroMemory(rentedBody.AsSpan());
        ArrayPool<byte>.Shared.Return(rentedBody);
    }

    if (!selection.IsSuccess)
    {
        var code = SafeDownstreamCode(selection.ErrorCode);
        var status = StatusForDownstreamCode(code);
        return Error(
            status,
            requestId,
            code,
            status == 500
                ? "The deterministic composition failed safely."
                : "The deterministic composition profile was rejected.");
    }

    var admission = DynamicGraphCompositionProfilePolicy.Admit(selection.Value, options.TenantId);
    if (!admission.IsAccepted)
        return Error(admission.StatusCode, requestId, admission.Code, admission.Message);
    var admittedProfile = admission.Profile!;
    var digest = admission.CanonicalDigest!;

    SessionCreationResult created;
    try
    {
        created = await registry.CreateAsync(
            options.TenantId,
            idempotencyKey.Value!,
            digest,
            admittedProfile,
            _ => Task.FromResult(Compose(executor, admittedProfile, readiness, requireReady: true)),
            () => readiness.IsReady,
            cancellationToken);
    }
    catch (ForbiddenTrainingAssemblyException)
    {
        return Error(503, requestId, "FORBIDDEN_TRAINING_ASSEMBLY_LOADED", "The local composition server is not ready.");
    }
    catch (CompositionRejectedException exception)
    {
        return Error(
            StatusForDownstreamCode(exception.Code), requestId, exception.Code,
            StatusForDownstreamCode(exception.Code) == 500
                ? "The deterministic composition failed safely."
                : "The deterministic composition profile was rejected.");
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }
    catch (Exception exception) when (IsNonFatalException(exception))
    {
        return Error(500, requestId, "HOST_COMPOSITION_EXCEPTION", "The deterministic composition failed safely.");
    }
    if (!readiness.IsReady)
    {
        registry.RollbackPublished(created.Session);
        return Error(
            503,
            requestId,
            readiness.ReasonCode ?? "HOST_NOT_READY",
            "The local composition server is not ready.");
    }

    return created.Status switch
    {
        SessionCreationStatus.Created => Session(
            context, requestId, created.Session!, 201, "created", persistenceOwner.IsEnabled),
        SessionCreationStatus.Replayed => Session(
            context, requestId, created.Session!, 200, "replayed", persistenceOwner.IsEnabled),
        SessionCreationStatus.Conflict => Error(409, requestId, "HOST_IDEMPOTENCY_CONFLICT", "The idempotency key is bound to another request."),
        SessionCreationStatus.Pending => Error(409, requestId, "HOST_IDEMPOTENCY_PENDING", "The idempotency key has a composition in progress."),
        SessionCreationStatus.CapacityExceeded => Error(503, requestId, "HOST_SESSION_CAPACITY_EXHAUSTED", "The bounded local session capacity is exhausted."),
        SessionCreationStatus.ForbiddenAssembly => Error(503, requestId, "FORBIDDEN_TRAINING_ASSEMBLY_LOADED", "The local composition server is not ready."),
        SessionCreationStatus.PersistenceFailure => Error(503, requestId, "HOST_SESSION_PERSISTENCE_FAILED", "The local composition server is not ready."),
        _ => Error(429, requestId, "HOST_COMPOSITION_CONCURRENCY_EXHAUSTED", "The bounded local composition concurrency is exhausted."),
    };
});

app.MapGet("/api/v1/dynamic-graph/sessions/{sessionId}", (
    HttpContext context,
    string sessionId,
    DynamicGraphCompositionSessionRegistry registry) =>
{
    var requestId = RequestId(context);
    if (!readiness.IsReady)
        return Error(503, requestId, readiness.ReasonCode ?? "HOST_NOT_READY", "The local composition server is not ready.");
    var tenantCheck = ValidateConfiguredTenant(context, options, requestId);
    if (tenantCheck.Error is not null)
        return tenantCheck.Error;
    if (!Regex.IsMatch(sessionId, "^dgs_[0-9a-f]{32}$", RegexOptions.CultureInvariant))
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");

    var lookup = registry.Get(options.TenantId, sessionId);
    if (!readiness.IsReady)
        return Error(
            503,
            requestId,
            readiness.ReasonCode ?? "HOST_NOT_READY",
            "The local composition server is not ready.");
    return lookup.Status switch
    {
        SessionLookupStatus.Found => Session(
            context, requestId, lookup.Session!, 200, "composed", persistenceOwner.IsEnabled),
        SessionLookupStatus.Expired => Error(410, requestId, "HOST_SESSION_EXPIRED", "The session has expired."),
        _ => Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found."),
    };
});

app.MapGet("/api/v1/dynamic-graph/sessions/{sessionId}/topology", (
    HttpContext context,
    string sessionId,
    DynamicGraphCompositionSessionRegistry registry) =>
{
    var requestId = RequestId(context);
    if (!readiness.IsReady)
        return Error(503, requestId, readiness.ReasonCode ?? "HOST_NOT_READY", "The local composition server is not ready.");
    var tenantCheck = ValidateConfiguredTenant(context, options, requestId);
    if (tenantCheck.Error is not null)
        return tenantCheck.Error;
    if (!Regex.IsMatch(sessionId, "^dgs_[0-9a-f]{32}$", RegexOptions.CultureInvariant))
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");

    try
    {
        var read = registry.ReadCurrentTopology(
            options.TenantId,
            sessionId,
            () => readiness.IsReady);

        if (read.Status == CurrentTopologySessionStatus.Expired)
            return Error(410, requestId, "HOST_SESSION_EXPIRED", "The session has expired.");
        if (read.Status == CurrentTopologySessionStatus.Unknown)
            return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");
        if (read.Status == CurrentTopologySessionStatus.Unavailable)
            return Error(422, requestId, "HOST_TOPOLOGY_READ_UNAVAILABLE", "The session does not support current-topology reads.");
        if (read.Status == CurrentTopologySessionStatus.ForbiddenAssembly)
            return Error(503, requestId, "FORBIDDEN_TRAINING_ASSEMBLY_LOADED", "The local composition server is not ready.");

        var operation = read.Result;
        if (read.Session is null || operation is null || (operation.IsSuccess && operation.Value is null))
            return Error(500, requestId, "HOST_TOPOLOGY_READ_RESULT_REQUIRED", "The current topology could not be read safely.");
        if (!operation.IsSuccess)
            return Error(500, requestId, "HOST_TOPOLOGY_READ_FAILED", "The current topology could not be read safely.");

        var response = CreateCurrentTopologyResponse(
            requestId,
            sessionId,
            options.TenantId,
            read.Session,
            operation.Value!,
            persistenceOwner.IsEnabled);
        return response is null
            ? Error(500, requestId, "HOST_TOPOLOGY_READ_FAILED", "The current topology could not be read safely.")
            : Results.Json(
                response,
                statusCode: StatusCodes.Status200OK,
                options: HttpJson.Options);
    }
    catch (Exception exception) when (IsNonFatalException(exception))
    {
        return Error(500, requestId, "HOST_TOPOLOGY_READ_EXCEPTION", "The current topology could not be read safely.");
    }
});

app.MapPost("/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-evidence", async (
    HttpContext context,
    string sessionId,
    DynamicGraphCompositionSessionRegistry registry,
    CancellationToken cancellationToken) =>
{
    var requestId = RequestId(context);
    if (!readiness.IsReady)
        return Error(503, requestId, readiness.ReasonCode ?? "HOST_NOT_READY", "The local composition server is not ready.");
    if (!context.Request.HasJsonContentType())
        return Error(415, requestId, "HOST_JSON_CONTENT_TYPE_REQUIRED", "The request content type must be JSON.");
    if (context.Request.ContentLength > options.MaxRequestBodyBytes)
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

    var tenantCheck = ValidateConfiguredTenant(context, options, requestId);
    if (tenantCheck.Error is not null)
        return tenantCheck.Error;
    if (!Regex.IsMatch(sessionId, "^dgs_[0-9a-f]{32}$", RegexOptions.CultureInvariant))
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");

    AdmitSelectedTargetEvidenceHttpRequest? body;
    try
    {
        body = await HttpJson.DeserializeStrictAsync<AdmitSelectedTargetEvidenceHttpRequest>(
            context.Request.Body, cancellationToken);
    }
    catch (JsonException exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(400, requestId, "HOST_SELECTED_TARGET_EVIDENCE_BODY_INVALID", "A valid selected-target evidence request body is required.");
    }
    catch (NotSupportedException exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(400, requestId, "HOST_SELECTED_TARGET_EVIDENCE_BODY_INVALID", "A valid selected-target evidence request body is required.");
    }
    catch (BadHttpRequestException exception) when (
        exception.StatusCode == StatusCodes.Status413PayloadTooLarge
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        if (!context.Response.HasStarted) context.Response.Clear();
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }

    if (body?.Target is null || body.AuthorityResult is null || body.NoveltyRequest is null)
        return Error(400, requestId, "HOST_SELECTED_TARGET_EVIDENCE_REQUIRED", "Complete selected-target evidence is required.");

    SelectedTargetEvidenceAdmissionResult admitted;
    try
    {
        admitted = registry.AdmitSelectedTargetEvidence(
            options.TenantId,
            sessionId,
            body.Target,
            body.AuthorityResult,
            body.NoveltyRequest,
            () => readiness.IsReady,
            cancellationToken);
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }
    catch (Exception exception) when (IsNonFatalException(exception))
    {
        return Error(500, requestId, "HOST_SELECTED_TARGET_EVIDENCE_EXCEPTION", "Selected-target evidence admission failed safely.");
    }

    if (admitted.Status == SelectedTargetEvidenceAdmissionStatus.Expired)
        return Error(410, requestId, "HOST_SESSION_EXPIRED", "The session has expired.");
    if (admitted.Status == SelectedTargetEvidenceAdmissionStatus.Unknown)
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");
    if (admitted.Status == SelectedTargetEvidenceAdmissionStatus.CapacityExceeded)
        return Error(429, requestId, "HOST_SELECTED_TARGET_EVIDENCE_CAPACITY_EXHAUSTED", "The bounded selected-target evidence capacity is exhausted.");
    if (admitted.Status == SelectedTargetEvidenceAdmissionStatus.ForbiddenAssembly)
        return Error(503, requestId, "FORBIDDEN_TRAINING_ASSEMBLY_LOADED", "The local composition server is not ready.");

    var operation = admitted.Result;
    if (operation is null || (operation.IsSuccess && operation.Value is null))
        return Error(500, requestId, "HOST_SELECTED_TARGET_EVIDENCE_RESULT_REQUIRED", "Selected-target evidence admission failed safely.");
    if (!operation.IsSuccess)
    {
        var code = SafeDownstreamCode(operation.ErrorCode);
        var status = StatusForSelectedTargetEvidenceCode(code);
        return Error(
            status,
            requestId,
            code,
            status is 400 or 422
                ? "The selected-target evidence request was rejected."
                : "Selected-target evidence admission failed safely.");
    }

    return SelectedTargetEvidenceAdmission(
        requestId,
        sessionId,
        operation.Value!);
});

app.MapPost("/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-runs", async (
    HttpContext context,
    string sessionId,
    DynamicGraphCompositionSessionRegistry registry,
    CancellationToken cancellationToken) =>
{
    var requestId = RequestId(context);
    if (!readiness.IsReady)
        return Error(503, requestId, readiness.ReasonCode ?? "HOST_NOT_READY", "The local composition server is not ready.");
    if (!context.Request.HasJsonContentType())
        return Error(415, requestId, "HOST_JSON_CONTENT_TYPE_REQUIRED", "The request content type must be JSON.");
    if (context.Request.ContentLength > options.MaxRequestBodyBytes)
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

    var tenantCheck = ValidateConfiguredTenant(context, options, requestId);
    if (tenantCheck.Error is not null)
        return tenantCheck.Error;
    if (!Regex.IsMatch(sessionId, "^dgs_[0-9a-f]{32}$", RegexOptions.CultureInvariant))
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");

    RunAdmittedSelectedTargetHttpRequest? body;
    try
    {
        body = await HttpJson.DeserializeStrictAsync<RunAdmittedSelectedTargetHttpRequest>(
            context.Request.Body, cancellationToken);
    }
    catch (JsonException exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(400, requestId, "HOST_SELECTED_TARGET_RUN_BODY_INVALID", "A valid selected-target run request body is required.");
    }
    catch (BadHttpRequestException exception) when (
        exception.StatusCode == StatusCodes.Status413PayloadTooLarge
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        if (!context.Response.HasStarted) context.Response.Clear();
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }

    if (!Regex.IsMatch(
            body?.EvidenceHandle ?? string.Empty,
            "^dgeh_[0-9a-f]{32}$",
            RegexOptions.CultureInvariant))
    {
        return Error(404, requestId, "HOST_SELECTED_TARGET_EVIDENCE_NOT_FOUND", "The selected-target evidence was not found.");
    }
    if (!DynamicGraphCompositionServerOptions.IsSafeIdentifier(body?.RunId, 1, 128))
        return Error(400, requestId, "HOST_RUN_ID_INVALID", "A safe non-empty run_id is required.");
    if (!DynamicGraphCompositionServerOptions.IsSafeIdentifier(body?.ScenarioId, 1, 128))
        return Error(400, requestId, "HOST_SCENARIO_ID_INVALID", "A safe non-empty scenario_id is required.");
    if (!IsSafeSelectedTargetInput(body?.UserRequest))
        return Error(400, requestId, "HOST_USER_REQUEST_INVALID", "A safe non-empty user_request is required.");

    SelectedTargetSessionExecutionResult executed;
    try
    {
        executed = await registry.RunAdmittedSelectedTargetAsync(
            options.TenantId,
            sessionId,
            body!.EvidenceHandle!,
            body.RunId!,
            body.ScenarioId!,
            body.UserRequest!,
            () => readiness.IsReady,
            cancellationToken);
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }
    catch (Exception exception) when (IsNonFatalException(exception))
    {
        return Error(500, requestId, "HOST_SELECTED_TARGET_RUN_EXCEPTION", "The selected-target run failed safely.");
    }

    if (executed.Status == SelectedTargetSessionExecutionStatus.Expired)
        return Error(410, requestId, "HOST_SESSION_EXPIRED", "The session has expired.");
    if (executed.Status == SelectedTargetSessionExecutionStatus.Unknown)
        return Error(404, requestId, "HOST_SELECTED_TARGET_EVIDENCE_NOT_FOUND", "The selected-target evidence was not found.");
    if (executed.Status == SelectedTargetSessionExecutionStatus.Busy)
        return Error(429, requestId, "HOST_RUN_CONCURRENCY_EXHAUSTED", "The bounded local assistant run concurrency is exhausted.");
    if (executed.Status == SelectedTargetSessionExecutionStatus.ForbiddenAssembly)
        return Error(503, requestId, "FORBIDDEN_TRAINING_ASSEMBLY_LOADED", "The local composition server is not ready.");
    if (executed.Status == SelectedTargetSessionExecutionStatus.Unavailable)
        return Error(422, requestId, "HOST_SELECTED_TARGET_EXECUTION_UNAVAILABLE", "The session does not support selected-target execution.");

    var operation = executed.Result;
    if (operation is null || (operation.IsSuccess && operation.Value is null))
        return Error(500, requestId, "HOST_SELECTED_TARGET_RUN_RESULT_REQUIRED", "The selected-target run failed safely.");
    if (!operation.IsSuccess)
    {
        var code = SafeDownstreamCode(operation.ErrorCode);
        var status = StatusForSelectedTargetExecutionCode(code);
        return Error(
            status,
            requestId,
            code,
            status == 408
                ? "The selected-target run was cancelled."
                : "The selected-target run failed safely.");
    }

    var receipt = operation.Value!;
    DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association;
    DynamicGraphL3SelectedTargetExecutionResult result;
    DynamicGraphTargetRunSpeedMeasurement runSpeed;
    DynamicGraphMeasurementTarget associationTarget;
    DynamicGraphMeasurementTarget runSpeedTarget;
    DynamicGraphRunSpeedMeasurement measurement;
    try
    {
        association = receipt.Association;
        result = receipt.ExecutionResult;
        runSpeed = receipt.RunSpeed;
        associationTarget = association.Target;
        runSpeedTarget = runSpeed.Target;
        measurement = runSpeed.RunSpeed;
    }
    catch (Exception exception) when (IsNonFatalException(exception))
    {
        return Error(500, requestId, "HOST_SELECTED_TARGET_RUN_RESULT_INVALID", "The selected-target run failed safely.");
    }

    if (receipt.DirectTargetInvocationCount != 1
        || !Equals(runSpeedTarget, associationTarget)
        || !StringComparer.Ordinal.Equals(associationTarget.TargetId, result.TargetId)
        || !StringComparer.Ordinal.Equals(runSpeedTarget.TargetId, result.TargetId)
        || !IsValidSelectedTargetRunSpeedMeasurement(measurement))
    {
        return Error(500, requestId, "HOST_SELECTED_TARGET_RUN_RESULT_INVALID", "The selected-target run failed safely.");
    }

    return AdmittedSelectedTargetRun(
        requestId,
        sessionId,
        result,
        measurement,
        receipt.DirectTargetInvocationCount);
});

app.MapPost("/api/v1/dynamic-graph/sessions/{sessionId}/topology-mutations", async (
    HttpContext context,
    string sessionId,
    DynamicGraphCompositionSessionRegistry registry,
    CancellationToken cancellationToken) =>
{
    var requestId = RequestId(context);
    if (!readiness.IsReady)
        return Error(503, requestId, readiness.ReasonCode ?? "HOST_NOT_READY", "The local composition server is not ready.");
    if (!context.Request.HasJsonContentType())
        return Error(415, requestId, "HOST_JSON_CONTENT_TYPE_REQUIRED", "The request content type must be JSON.");
    if (context.Request.ContentLength > options.MaxRequestBodyBytes)
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

    var tenantCheck = ValidateConfiguredTenant(context, options, requestId);
    if (tenantCheck.Error is not null)
        return tenantCheck.Error;
    if (!Regex.IsMatch(sessionId, "^dgs_[0-9a-f]{32}$", RegexOptions.CultureInvariant))
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");
    if (persistenceOwner.IsEnabled)
    {
        return Error(
            409,
            requestId,
            "HOST_TOPOLOGY_MUTATION_REQUIRES_EPHEMERAL_MODE",
            "Topology mutation requires ephemeral session mode.");
    }

    ApplyTopologyMutationHttpRequest? body;
    try
    {
        body = await HttpJson.DeserializeStrictAsync<ApplyTopologyMutationHttpRequest>(
            context.Request.Body, cancellationToken);
    }
    catch (JsonException exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(400, requestId, "HOST_TOPOLOGY_MUTATION_REQUEST_BODY_INVALID", "A valid topology mutation request body is required.");
    }
    catch (BadHttpRequestException exception) when (
        exception.StatusCode == StatusCodes.Status413PayloadTooLarge
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        if (!context.Response.HasStarted) context.Response.Clear();
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }

    if (body?.ExpectedRevision is null || body.ExpectedRevision < 0)
    {
        return Error(
            400,
            requestId,
            "HOST_TOPOLOGY_MUTATION_EXPECTED_REVISION_INVALID",
            "A non-negative expected_revision is required.");
    }

    var admission = DynamicGraphCompositionProfilePolicy.Admit(body.Profile, options.TenantId);
    if (!admission.IsAccepted)
        return Error(admission.StatusCode, requestId, admission.Code, admission.Message);
    var admittedProfile = admission.Profile!;

    TopologyMutationSessionResult executed;
    try
    {
        executed = await registry.ApplyTopologyMutationAsync(
            options.TenantId,
            sessionId,
            body.ExpectedRevision.Value,
            admittedProfile,
            () => readiness.IsReady,
            cancellationToken);
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }
    catch (Exception exception) when (IsNonFatalException(exception))
    {
        return Error(500, requestId, "HOST_TOPOLOGY_MUTATION_EXCEPTION", "The topology mutation failed safely.");
    }

    if (executed.Status == TopologyMutationSessionStatus.Expired)
        return Error(410, requestId, "HOST_SESSION_EXPIRED", "The session has expired.");
    if (executed.Status == TopologyMutationSessionStatus.Unknown)
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");
    if (executed.Status == TopologyMutationSessionStatus.Busy)
        return Error(429, requestId, "HOST_RUN_CONCURRENCY_EXHAUSTED", "The bounded local assistant run concurrency is exhausted.");
    if (executed.Status == TopologyMutationSessionStatus.ForbiddenAssembly)
        return Error(503, requestId, "FORBIDDEN_TRAINING_ASSEMBLY_LOADED", "The local composition server is not ready.");
    if (executed.Status == TopologyMutationSessionStatus.Unavailable)
        return Error(422, requestId, "HOST_TOPOLOGY_MUTATION_UNAVAILABLE", "The session does not support topology mutation.");
    if (executed.Status == TopologyMutationSessionStatus.EphemeralModeRequired)
    {
        return Error(
            409,
            requestId,
            "HOST_TOPOLOGY_MUTATION_REQUIRES_EPHEMERAL_MODE",
            "Topology mutation requires ephemeral session mode.");
    }

    var operation = executed.Result;
    if (operation is null || (operation.IsSuccess && operation.Value is null))
        return Error(500, requestId, "HOST_TOPOLOGY_MUTATION_RESULT_INCOHERENT", "The topology mutation failed safely.");
    if (!operation.IsSuccess)
    {
        var code = SafeDownstreamCode(operation.ErrorCode);
        if (code == "HOST_TOPOLOGY_MUTATION_PROFILE_INVALID")
            return Error(400, requestId, code, "The topology mutation profile was rejected.");
        if (code is "HOST_TOPOLOGY_MUTATION_PROFILE_ID_MISMATCH"
            or "HOST_TOPOLOGY_MUTATION_SCOPE_MISMATCH"
            or "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REVISION_CONFLICT")
        {
            return Error(409, requestId, code, "The topology mutation conflicts with the current session.");
        }
        if (IsAllowedTopologyMutationCallerCode(code))
            return Error(422, requestId, code, "The topology mutation profile was rejected.");
        return Error(500, requestId, "HOST_TOPOLOGY_MUTATION_EXCEPTION", "The topology mutation failed safely.");
    }

    var response = CreateTopologyMutationResponse(
        requestId,
        sessionId,
        body.ExpectedRevision.Value,
        admittedProfile,
        operation.Value!);
    return response is null
        ? Error(500, requestId, "HOST_TOPOLOGY_MUTATION_RESULT_INCOHERENT", "The topology mutation failed safely.")
        : Results.Json(
            response,
            statusCode: StatusCodes.Status200OK,
            options: HttpJson.Options);
});

app.MapPost("/api/v1/dynamic-graph/sessions/{sessionId}/topology-runs", async (
    HttpContext context,
    string sessionId,
    DynamicGraphCompositionSessionRegistry registry,
    CancellationToken cancellationToken) =>
{
    var requestId = RequestId(context);
    if (!readiness.IsReady)
        return Error(503, requestId, readiness.ReasonCode ?? "HOST_NOT_READY", "The local composition server is not ready.");
    if (!context.Request.HasJsonContentType())
        return Error(415, requestId, "HOST_JSON_CONTENT_TYPE_REQUIRED", "The request content type must be JSON.");
    if (context.Request.ContentLength > options.MaxRequestBodyBytes)
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

    var tenantCheck = ValidateConfiguredTenant(context, options, requestId);
    if (tenantCheck.Error is not null)
        return tenantCheck.Error;
    if (!Regex.IsMatch(sessionId, "^dgs_[0-9a-f]{32}$", RegexOptions.CultureInvariant))
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");

    RunCompositionSessionHttpRequest? body;
    try
    {
        body = await HttpJson.DeserializeStrictAsync<RunCompositionSessionHttpRequest>(
            context.Request.Body, cancellationToken);
    }
    catch (JsonException exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(400, requestId, "HOST_TOPOLOGY_RUN_REQUEST_BODY_INVALID", "A valid topology run request body is required.");
    }
    catch (BadHttpRequestException exception) when (
        exception.StatusCode == StatusCodes.Status413PayloadTooLarge
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        if (!context.Response.HasStarted) context.Response.Clear();
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }

    if (string.IsNullOrWhiteSpace(body?.UserRequest))
        return Error(400, requestId, "HOST_USER_REQUEST_REQUIRED", "A non-empty user_request is required.");
    if (body.UserRequest.Length > DynamicGraphCompositionServerOptions.MaxInputLength)
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

    TopologySessionExecutionResult executed;
    try
    {
        executed = await registry.RunOnCurrentTopologyAsync(
            options.TenantId,
            sessionId,
            new DynamicGraphLogicalOrchestratorRunRequest(body.UserRequest),
            () => readiness.IsReady,
            cancellationToken);
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }
    catch (Exception exception) when (IsNonFatalException(exception))
    {
        return Error(500, requestId, "HOST_TOPOLOGY_RUN_EXCEPTION", "The topology run failed safely.");
    }

    if (!readiness.IsReady)
        return Error(
            503,
            requestId,
            readiness.ReasonCode ?? "HOST_NOT_READY",
            "The local composition server is not ready.");

    if (executed.Status == TopologySessionExecutionStatus.Expired)
        return Error(410, requestId, "HOST_SESSION_EXPIRED", "The session has expired.");
    if (executed.Status == TopologySessionExecutionStatus.Unknown)
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");
    if (executed.Status == TopologySessionExecutionStatus.Unavailable)
        return Error(422, requestId, "HOST_TOPOLOGY_RUN_UNAVAILABLE", "The session does not support topology execution.");
    if (executed.Status == TopologySessionExecutionStatus.Busy)
        return Error(429, requestId, "HOST_RUN_CONCURRENCY_EXHAUSTED", "The bounded local assistant run concurrency is exhausted.");
    if (executed.Status == TopologySessionExecutionStatus.ForbiddenAssembly)
        return Error(503, requestId, "FORBIDDEN_TRAINING_ASSEMBLY_LOADED", "The local composition server is not ready.");

    var operation = executed.Result;
    if (operation is null || (operation.IsSuccess && operation.Value is null))
        return Error(500, requestId, "HOST_TOPOLOGY_RUN_RESULT_REQUIRED", "The topology run failed safely.");
    if (!operation.IsSuccess)
    {
        var code = SafeDownstreamCode(operation.ErrorCode);
        var status = StatusForAssistantCode(code);
        return Error(
            status,
            requestId,
            code,
            status is 400 ? "The topology run request was rejected." : "The topology run failed safely.");
    }

    return TopologyAssistantRun(
        requestId,
        sessionId,
        operation.Value!,
        persistenceOwner.IsEnabled);
});

app.MapPost("/api/v1/dynamic-graph/sessions/{sessionId}/runs", async (
    HttpContext context,
    string sessionId,
    DynamicGraphCompositionSessionRegistry registry,
    CancellationToken cancellationToken) =>
{
    var requestId = RequestId(context);
    if (!readiness.IsReady)
        return Error(503, requestId, readiness.ReasonCode ?? "HOST_NOT_READY", "The local composition server is not ready.");
    if (!context.Request.HasJsonContentType())
        return Error(415, requestId, "HOST_JSON_CONTENT_TYPE_REQUIRED", "The request content type must be JSON.");
    if (context.Request.ContentLength > options.MaxRequestBodyBytes)
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

    var tenantCheck = ValidateConfiguredTenant(context, options, requestId);
    if (tenantCheck.Error is not null)
        return tenantCheck.Error;
    if (!Regex.IsMatch(sessionId, "^dgs_[0-9a-f]{32}$", RegexOptions.CultureInvariant))
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");

    RunCompositionSessionHttpRequest? body;
    try
    {
        body = await HttpJson.DeserializeStrictAsync<RunCompositionSessionHttpRequest>(
            context.Request.Body, cancellationToken);
    }
    catch (JsonException exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(400, requestId, "HOST_RUN_REQUEST_BODY_INVALID", "A valid assistant run request body is required.");
    }
    catch (BadHttpRequestException exception) when (
        exception.StatusCode == StatusCodes.Status413PayloadTooLarge
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        if (!context.Response.HasStarted) context.Response.Clear();
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }

    if (string.IsNullOrWhiteSpace(body?.UserRequest))
        return Error(400, requestId, "HOST_USER_REQUEST_REQUIRED", "A non-empty user_request is required.");
    if (body.UserRequest.Length > DynamicGraphCompositionServerOptions.MaxInputLength)
        return Error(413, requestId, "HOST_REQUEST_BODY_LIMIT_EXCEEDED", "The request body exceeds the local limit.");

    SessionExecutionResult executed;
    try
    {
        executed = await registry.RunAsync(
            options.TenantId,
            sessionId,
            new DynamicGraphLogicalOrchestratorRunRequest(body.UserRequest),
            () => readiness.IsReady,
            cancellationToken);
    }
    catch (OperationCanceledException exception) when (
        cancellationToken.IsCancellationRequested
        && FatalExceptionPolicy.IsNonFatal(exception))
    {
        return Error(408, requestId, "HOST_REQUEST_CANCELLED", "The request was cancelled.");
    }
    catch (Exception exception) when (IsNonFatalException(exception))
    {
        return Error(500, requestId, "HOST_ASSISTANT_RUN_EXCEPTION", "The assistant run failed safely.");
    }

    if (!readiness.IsReady)
        return Error(
            503,
            requestId,
            readiness.ReasonCode ?? "HOST_NOT_READY",
            "The local composition server is not ready.");

    if (executed.Status == SessionExecutionStatus.Expired)
        return Error(410, requestId, "HOST_SESSION_EXPIRED", "The session has expired.");
    if (executed.Status == SessionExecutionStatus.Unknown)
        return Error(404, requestId, "HOST_SESSION_NOT_FOUND", "The session was not found.");
    if (executed.Status == SessionExecutionStatus.Busy)
        return Error(429, requestId, "HOST_RUN_CONCURRENCY_EXHAUSTED", "The bounded local assistant run concurrency is exhausted.");
    if (executed.Status == SessionExecutionStatus.ForbiddenAssembly)
        return Error(503, requestId, "FORBIDDEN_TRAINING_ASSEMBLY_LOADED", "The local composition server is not ready.");

    var operation = executed.Result;
    if (operation is null || (operation.IsSuccess && operation.Value is null))
        return Error(500, requestId, "HOST_ASSISTANT_RUN_RESULT_REQUIRED", "The assistant run failed safely.");
    if (!operation.IsSuccess)
    {
        var code = SafeDownstreamCode(operation.ErrorCode);
        var status = StatusForAssistantCode(code);
        return Error(
            status,
            requestId,
            code,
            status is 400 ? "The assistant run request was rejected." : "The assistant run failed safely.");
    }

    return AssistantRun(requestId, sessionId, operation.Value!, persistenceOwner.IsEnabled);
});

app.Lifetime.ApplicationStopping.Register(
    static state =>
    {
        var (readinessState, readyFile) =
            ((StartupReadinessState Readiness, string ReadyFile))state!;
        Exception? failClosedFailure = null;
        try
        {
            readinessState.FailClosed("HOST_NOT_READY");
        }
        catch (Exception exception)
        {
            failClosedFailure = exception;
            throw;
        }
        finally
        {
            if (failClosedFailure is null || FatalExceptionPolicy.IsNonFatal(failClosedFailure))
                ReadyFile.RemoveSafe(readyFile);
        }
    },
    (readiness, options.ReadyFile));
try
{
    if (!readiness.AllowsComposition(requireReady: false))
    {
        Console.Error.WriteLine("NOT_READY");
        Environment.ExitCode = 3;
        return;
    }
    await app.StartAsync();
    if (!readiness.PublishReady(app.Urls.Single()))
    {
        Console.Error.WriteLine("NOT_READY");
        Environment.ExitCode = 3;
        return;
    }
    Console.WriteLine("READY");
    await app.WaitForShutdownAsync();
}
catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
{
    ReadyFile.RemoveSafe(options.ReadyFile);
    Console.Error.WriteLine("NOT_READY");
    Environment.ExitCode = 3;
}
finally
{
    ReadyFile.RemoveSafe(options.ReadyFile);
    appCleanupOwner = null;
    try { await app.DisposeAsync(); }
    catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception)) { /* Shutdown remains sanitized and the ready marker is already removed. */ }
}
}
catch (Exception exception)
{
    hostFailure = exception;
    throw;
}
finally
{
    await DisposeHostOwnersAsync(
        appCleanupOwner,
        readinessCleanupOwner,
        registryCleanupOwner,
        persistenceOwner,
        hostFailure);
}

static async ValueTask DisposeHostOwnersAsync(
    WebApplication? app,
    StartupReadinessState? readiness,
    DynamicGraphCompositionSessionRegistry? registry,
    DynamicGraphCompositionSessionPersistence persistence,
    Exception? pendingFailure)
{
    if (pendingFailure is not null && !FatalExceptionPolicy.IsNonFatal(pendingFailure))
        return;

    Exception? appFailure = null;
    try
    {
        if (app is not null)
            await app.DisposeAsync();
    }
    catch (Exception exception)
    {
        appFailure = exception;
        throw;
    }
    finally
    {
        DisposeHostOwners(
            readiness,
            registry,
            persistence,
            appFailure ?? pendingFailure);
    }
}

static void DisposeHostOwners(
    StartupReadinessState? readiness,
    DynamicGraphCompositionSessionRegistry? registry,
    DynamicGraphCompositionSessionPersistence persistence,
    Exception? pendingFailure)
{
    if (pendingFailure is not null && !FatalExceptionPolicy.IsNonFatal(pendingFailure))
        return;

    Exception? readinessFailure = null;
    try
    {
        readiness?.Dispose();
    }
    catch (Exception exception)
    {
        readinessFailure = exception;
        throw;
    }
    finally
    {
        if (readinessFailure is null || FatalExceptionPolicy.IsNonFatal(readinessFailure))
        {
            Exception? registryFailure = null;
            try
            {
                registry?.Dispose();
            }
            catch (Exception exception)
            {
                registryFailure = exception;
                throw;
            }
            finally
            {
                if (registryFailure is null || FatalExceptionPolicy.IsNonFatal(registryFailure))
                    persistence.Dispose();
            }
        }
    }
}

static async ValueTask<BoundedRequestBodyRead> ReadBoundedRequestBodyAsync(
    Stream body,
    byte[] rentedBody,
    int logicalLimit,
    CancellationToken cancellationToken)
{
    var count = 0;
    while (count < logicalLimit)
    {
        var read = await body.ReadAsync(
            rentedBody.AsMemory(count, logicalLimit - count),
            cancellationToken);
        if (read == 0)
            return new BoundedRequestBodyRead(count, LimitExceeded: false);
        count += read;
    }

    var eofProbe = await body.ReadAsync(
        rentedBody.AsMemory(logicalLimit, 1),
        cancellationToken);
    return new BoundedRequestBodyRead(count, LimitExceeded: eofProbe != 0);
}

static bool TryReadSelectedProfileIdQuery(HttpRequest request, out string selectedProfileId)
{
    const string prefix = "?selected_profile_id=";
    selectedProfileId = string.Empty;
    var rawQuery = request.QueryString.Value;
    if (rawQuery is null
        || !rawQuery.StartsWith(prefix, StringComparison.Ordinal)
        || rawQuery.Contains('&', StringComparison.Ordinal))
    {
        return false;
    }

    var rawValue = rawQuery.AsSpan(prefix.Length);
    var decodedBytes = new ArrayBufferWriter<byte>(Math.Max(rawValue.Length, 1));
    for (var index = 0; index < rawValue.Length; index++)
    {
        var character = rawValue[index];
        if (character == '+')
        {
            decodedBytes.GetSpan(1)[0] = (byte)' ';
            decodedBytes.Advance(1);
            continue;
        }

        if (character == '%')
        {
            if (index + 2 >= rawValue.Length
                || !TryDecodeHex(rawValue[index + 1], out var high)
                || !TryDecodeHex(rawValue[index + 2], out var low))
            {
                return false;
            }

            decodedBytes.GetSpan(1)[0] = (byte)((high << 4) | low);
            decodedBytes.Advance(1);
            index += 2;
            continue;
        }

        Rune scalar;
        if (char.IsHighSurrogate(character))
        {
            if (index + 1 >= rawValue.Length
                || !Rune.TryCreate(character, rawValue[index + 1], out scalar))
            {
                return false;
            }
            index++;
        }
        else
        {
            if (char.IsLowSurrogate(character))
                return false;
            scalar = new Rune(character);
        }

        var scalarBytes = decodedBytes.GetSpan(4);
        decodedBytes.Advance(scalar.EncodeToUtf8(scalarBytes));
    }

    try
    {
        selectedProfileId = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true).GetString(decodedBytes.WrittenSpan);
    }
    catch (DecoderFallbackException exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        return false;
    }

    IQueryCollection parsedQuery;
    try
    {
        parsedQuery = request.Query;
    }
    catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
    {
        return false;
    }

    if (parsedQuery.Count != 1)
        return false;
    var parsedEntry = parsedQuery.Single();
    return string.Equals(parsedEntry.Key, "selected_profile_id", StringComparison.Ordinal)
           && parsedEntry.Value.Count == 1
           && string.Equals(parsedEntry.Value[0], selectedProfileId, StringComparison.Ordinal);
}

static bool TryDecodeHex(char character, out int value)
{
    if (character is >= '0' and <= '9')
    {
        value = character - '0';
        return true;
    }
    if (character is >= 'A' and <= 'F')
    {
        value = character - 'A' + 10;
        return true;
    }
    if (character is >= 'a' and <= 'f')
    {
        value = character - 'a' + 10;
        return true;
    }

    value = 0;
    return false;
}

static string RequestId(HttpContext context) => (string)context.Items["request-id"]!;

static SingleSafeHeaderRead ReadSingleSafeHeader(
    HttpRequest request,
    string headerName,
    int minimumLength,
    int maximumLength)
{
    if (!request.Headers.TryGetValue(headerName, out var values))
        return new(SingleSafeHeaderState.Missing, null);

    if (values.Count != 1)
        return new(SingleSafeHeaderState.Invalid, null);

    var value = values[0];
    return value is not null
           && !value.Contains(',', StringComparison.Ordinal)
           && DynamicGraphCompositionServerOptions.IsSafeIdentifier(value, minimumLength, maximumLength)
        ? new(SingleSafeHeaderState.Valid, value)
        : new(SingleSafeHeaderState.Invalid, null);
}

static (string? Tenant, IResult? Error) ValidateConfiguredTenant(
    HttpContext context,
    DynamicGraphCompositionServerOptions options,
    string requestId)
{
    var tenantHeader = ReadSingleSafeHeader(
        context.Request,
        "X-XIIGen-Tenant-Id",
        1,
        DynamicGraphCompositionServerOptions.MaxTenantIdLength);
    if (tenantHeader.State != SingleSafeHeaderState.Valid)
        return (null, Error(400, requestId, "HOST_TENANT_HEADER_INVALID", "A safe X-XIIGen-Tenant-Id header is required."));
    var tenant = tenantHeader.Value!;
    if (!string.Equals(tenant, options.TenantId, StringComparison.Ordinal))
        return (null, Error(403, requestId, "HOST_TENANT_BINDING_MISMATCH", "The request does not match the configured tenant binding."));
    return (tenant, null);
}

static (
    CompositionEvidenceHttpResponse Evidence,
    IDynamicGraphLogicalOrchestratorRunSession RunSession) Compose(
    IDynamicGraphLogicalOrchestratorCompositionExecutor executor,
    DynamicGraphLogicalOrchestratorBootstrapProfile profile,
    StartupReadinessState readiness,
    bool requireReady)
{
    if (!readiness.AllowsComposition(requireReady))
        throw new ForbiddenTrainingAssemblyException();
    var composed = executor.Compose(profile);
    if (!readiness.AllowsComposition(requireReady))
        throw new ForbiddenTrainingAssemblyException();
    if (composed is null)
        throw new CompositionRejectedException("HOST_COMPOSITION_RESULT_REQUIRED");
    if (!composed.IsSuccess)
        throw new CompositionRejectedException(SafeDownstreamCode(composed.ErrorCode));
    if (composed.Value is null)
        throw new CompositionRejectedException("HOST_COMPOSITION_RESULT_REQUIRED");
    if (composed.Value is not IDynamicGraphLogicalOrchestratorRunSession runSession)
        throw new CompositionRejectedException("HOST_ASSISTANT_RUN_SESSION_REQUIRED");

    var current = composed.Value.ReadCurrent();
    if (!readiness.AllowsComposition(requireReady))
        throw new ForbiddenTrainingAssemblyException();
    if (current is null)
        throw new CompositionRejectedException("HOST_SESSION_RESULT_REQUIRED");
    if (!current.IsSuccess)
        throw new CompositionRejectedException(SafeDownstreamCode(current.ErrorCode));
    if (current.Value is null)
        throw new CompositionRejectedException("HOST_SESSION_RESULT_REQUIRED");

    var evidence = current.Value;
    if (!string.Equals(evidence.Descriptor.ProfileId, profile.ProfileId, StringComparison.Ordinal)
        || !string.Equals(evidence.Descriptor.TenantId, profile.TenantId, StringComparison.Ordinal)
        || !string.Equals(evidence.Descriptor.DomainTag, profile.DomainTag, StringComparison.Ordinal)
        || !string.Equals(evidence.Descriptor.ContextId, profile.ContextId, StringComparison.Ordinal)
        || !string.Equals(evidence.SignalId, profile.SignalId, StringComparison.Ordinal))
    {
        throw new CompositionRejectedException("HOST_SESSION_EVIDENCE_MISMATCH");
    }
    return (
        new CompositionEvidenceHttpResponse(
            evidence.Descriptor.ProfileId,
            evidence.Revision,
            evidence.SourceKind.ToString(),
            evidence.NodeCount,
            evidence.EdgeCount,
            evidence.InitialRevisionAdvanced),
        runSession);
}

static string SafeDownstreamCode(string? code)
    => code is { Length: > 0 and <= 128 }
       && Regex.IsMatch(code, "^[A-Z0-9_]+$", RegexOptions.CultureInvariant)
        ? code
        : "HOST_COMPOSITION_EXCEPTION";

static int StatusForDownstreamCode(string code)
{
    if (code.EndsWith("_RESULT_REQUIRED", StringComparison.Ordinal)
        || code.EndsWith("_EXCEPTION", StringComparison.Ordinal))
        return 500;
    if (code.StartsWith("DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_", StringComparison.Ordinal)
        || code is "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DEFINITIONS_REQUIRED"
            or "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DEFINITION_REQUIRED"
            or "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DESCRIPTOR_REQUIRED"
            or "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_EDGE_BINDINGS_REQUIRED")
        return 400;
    return code.StartsWith("DYNAMIC_GRAPH_", StringComparison.Ordinal) ? 422 : 500;
}

static int StatusForAssistantCode(string code)
    => code is "EMPTY_REQUEST"
        or "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_RUN_REQUEST_REQUIRED"
        or "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_USER_REQUEST_REQUIRED"
        ? 400
        : code == "CANCELLED" ? 408 : 500;

static int StatusForSelectedTargetEvidenceCode(string code)
{
    if (code == "CANCELLED")
        return 408;
    if (code == "HOST_SELECTED_TARGET_EVIDENCE_ADMISSION_RESULT_REQUIRED")
        return 500;
    if (code.EndsWith("_REQUIRED", StringComparison.Ordinal))
        return 400;
    return code.StartsWith("DYNAMIC_GRAPH_", StringComparison.Ordinal)
           || code.StartsWith("EDGE_AUTHORITY_", StringComparison.Ordinal)
        ? 422
        : 500;
}

static int StatusForSelectedTargetExecutionCode(string code)
    => code == "CANCELLED" ? 408 : 500;

static bool IsAllowedTopologyMutationCallerCode(string code)
    => code is
        "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SCOPE_INVALID"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SIGNAL_REFERENCES_INVALID"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_NODE_BINDINGS_REQUIRED"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EDGE_BINDINGS_INVALID"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_NODE_BINDING"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SUB_PHASE_NOT_FOUND"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_NODE_MISMATCH"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_EDGE_BINDING"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_TRANSITIONS_REQUIRED"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_TRANSITION_NOT_PROJECTABLE"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_TRANSITION_COVERAGE"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_INCOMPLETE_TRANSITION_COVERAGE"
        or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EDGE_SOURCE_MISMATCH"
        or "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_INVALID"
        or "DYNAMIC_GRAPH_TENANT_REQUIRED"
        or "DYNAMIC_GRAPH_DOMAIN_REQUIRED"
        or "DYNAMIC_GRAPH_CONTEXT_REQUIRED"
        or "DYNAMIC_GRAPH_NODES_REQUIRED"
        or "DYNAMIC_GRAPH_NODE_REQUIRED"
        or "DYNAMIC_GRAPH_NODE_ID_REQUIRED"
        or "DYNAMIC_GRAPH_DUPLICATE_NODE_ID"
        or "DYNAMIC_GRAPH_NODE_DESCRIPTOR_REQUIRED"
        or "DYNAMIC_GRAPH_NODE_DESCRIPTOR_ID_MISMATCH"
        or "DYNAMIC_GRAPH_NODE_INPUT_TYPE_REQUIRED"
        or "DYNAMIC_GRAPH_NODE_OUTPUT_TYPE_REQUIRED"
        or "DYNAMIC_GRAPH_NODE_DOMAIN_REQUIRED"
        or "DYNAMIC_GRAPH_DOMAIN_MISMATCH"
        or "DYNAMIC_GRAPH_NODE_AUTHORITY_ROLE_REQUIRED"
        or "DYNAMIC_GRAPH_NODE_AUTHORITY_ROLE_INVALID"
        or "DYNAMIC_GRAPH_NODE_DETERMINISTIC_AUTHORITY_FORBIDDEN"
        or "DYNAMIC_GRAPH_NODE_UNTRAINED_SEMANTIC_CLAIM"
        or "DYNAMIC_GRAPH_NODE_SEMANTIC_CLAIM_ROLE_REQUIRED"
        or "DYNAMIC_GRAPH_NODE_AUTHORITY_STATE_MISMATCH"
        or "DYNAMIC_GRAPH_NODE_CHECKPOINT_REQUIRED"
        or "DYNAMIC_GRAPH_NODE_MANIFEST_REQUIRED"
        or "DYNAMIC_GRAPH_ORDER_INCOMPLETE"
        or "DYNAMIC_GRAPH_ORDER_ENTRY_REQUIRED"
        or "DYNAMIC_GRAPH_ORDER_ID_REQUIRED"
        or "DYNAMIC_GRAPH_DUPLICATE_ORDER_ID"
        or "DYNAMIC_GRAPH_ORDER_NODE_ID_REQUIRED"
        or "DYNAMIC_GRAPH_DUPLICATE_ORDER_NODE"
        or "DYNAMIC_GRAPH_DANGLING_ORDER_NODE"
        or "DYNAMIC_GRAPH_ORDER_POSITION_INVALID"
        or "DYNAMIC_GRAPH_DUPLICATE_ORDER_POSITION"
        or "DYNAMIC_GRAPH_TENANT_MISMATCH"
        or "DYNAMIC_GRAPH_CONTEXT_MISMATCH"
        or "DYNAMIC_GRAPH_ORDER_NOT_CONTIGUOUS"
        or "DYNAMIC_GRAPH_EDGE_REQUIRED"
        or "DYNAMIC_GRAPH_EDGE_ID_REQUIRED"
        or "DYNAMIC_GRAPH_DUPLICATE_EDGE_ID"
        or "DYNAMIC_GRAPH_EDGE_KIND_INVALID"
        or "DYNAMIC_GRAPH_EDGE_SOURCE_REQUIRED"
        or "DYNAMIC_GRAPH_DANGLING_SOURCE_NODE"
        or "DYNAMIC_GRAPH_EDGE_TARGETS_REQUIRED"
        or "DYNAMIC_GRAPH_FAN_IN_CARDINALITY_INVALID"
        or "DYNAMIC_GRAPH_MANY_TO_MANY_CARDINALITY_INVALID"
        or "DYNAMIC_GRAPH_RETURN_LOOP_CARDINALITY_INVALID"
        or "DYNAMIC_GRAPH_RETURN_LOOP_PARALLEL_FORBIDDEN"
        or "DYNAMIC_GRAPH_EDGE_AUTHORITY_REQUIRED"
        or "DYNAMIC_GRAPH_EDGE_AUTHORITY_LEVEL_REQUIRED"
        or "DYNAMIC_GRAPH_EDGE_AUTHORITY_LEVEL_INVALID"
        or "DYNAMIC_GRAPH_EDGE_DETERMINISTIC_AUTHORITY_FORBIDDEN"
        or "DYNAMIC_GRAPH_EDGE_BASELINE_UNIT_REQUIRED"
        or "DYNAMIC_GRAPH_EDGE_BASELINE_ROLE_REQUIRED"
        or "DYNAMIC_GRAPH_EDGE_BASELINE_ROLE_INVALID"
        or "DYNAMIC_GRAPH_EDGE_LEAKAGE_EXCLUSIONS_REQUIRED"
        or "DYNAMIC_GRAPH_EDGE_LEAKAGE_EXCLUSION_REQUIRED"
        or "DYNAMIC_GRAPH_EDGE_SOURCE_PROOF_REQUIRED"
        or "DYNAMIC_GRAPH_EDGE_CUTOVER_PROOF_REQUIRED"
        or "DYNAMIC_GRAPH_EDGE_ABLATION_PROOF_REQUIRED"
        or "DYNAMIC_GRAPH_EDGE_TARGET_REQUIRED"
        or "DYNAMIC_GRAPH_DUPLICATE_EDGE_TARGET"
        or "DYNAMIC_GRAPH_DANGLING_TARGET_NODE"
        or "DYNAMIC_GRAPH_SELF_EDGE"
        or "DYNAMIC_GRAPH_RETURN_LOOP_DIRECTION_INVALID"
        or "DYNAMIC_GRAPH_CYCLE_DETECTED";

static bool IsSafeSelectedTargetInput(string? value)
{
    if (string.IsNullOrWhiteSpace(value)
        || value.Length > DynamicGraphCompositionServerOptions.MaxInputLength)
    {
        return false;
    }

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

static IResult Error(int status, string requestId, string code, string message)
    => Results.Json(new SafeErrorHttpResponse(requestId, code, message), statusCode: status, options: HttpJson.Options);

static IResult PromptToTaskUntrainedHonestyRun(
    string requestId,
    string evaluationId,
    PromptToTaskUntrainedHonestyExecutionResult executed)
{
    var bound = executed.Measurement;
    var target = bound?.Target;
    var probe = bound?.UntrainedHonesty;
    var producer = probe?.Producer;
    var measurement = probe?.Honesty;
    if (target is null
        || producer is null
        || measurement is null
        || executed.DirectUnitInvocationCount != 1
        || target.Level != DynamicGraphMeasurementTargetLevel.L2LogicalUnit
        || target.MechanismKind != DynamicGraphMeasurementMechanismKind.LearningSubject
        || target.DeterministicFinalAuthority
        || !StringComparer.Ordinal.Equals(target.TargetId, producer.UnitId)
        || !StringComparer.Ordinal.Equals(evaluationId, measurement.EvaluationId))
    {
        return Error(
            500,
            requestId,
            "HOST_L2_UNTRAINED_HONESTY_RESULT_REQUIRED",
            "The L2 untrained honesty execution failed safely.");
    }

    return Results.Json(
        new PromptToTaskUntrainedHonestyHttpResponse(
            requestId,
            "measured",
            evaluationId,
            new PromptToTaskUntrainedHonestyTargetHttpResponse(
                target.TargetId,
                "L2LogicalUnit",
                "LearningSubject",
                target.DeterministicFinalAuthority),
            new PromptToTaskUntrainedHonestyProducerHttpResponse(
                producer.UnitId,
                producer.UnitRole,
                producer.DomainTag,
                producer.Version,
                producer.RegistrationOrder),
            new PromptToTaskUntrainedHonestyMeasurementHttpResponse(
                measurement.RuntimeReportedNotReady,
                measurement.HasNoTrainedStateEvidence,
                measurement.HasNoSemanticClassificationEvidence,
                measurement.MeetsUntrainedHonestyGate),
            executed.DirectUnitInvocationCount,
            new PromptToTaskUntrainedHonestyBoundaryHttpResponse(
                LoopbackOnly: true,
                TenantBindingIsAuthentication: false,
                Persistent: false,
                ExecutesUnit: true,
                MeasurementOnly: true,
                PerformsTraining: false,
                ClaimsTrainedState: false,
                ClaimsSemanticQuality: false,
                ChangesAuthority: false,
                PersistsResult: false)),
        statusCode: StatusCodes.Status200OK,
        options: HttpJson.Options);
}

static IResult Session(
    HttpContext context,
    string requestId,
    DynamicGraphCompositionSessionRegistry.SessionEntry session,
    int status,
    string state,
    bool persistent)
{
    if (status == StatusCodes.Status201Created)
        context.Response.Headers.Location = $"/api/v1/dynamic-graph/sessions/{session.SessionId}";
    return Results.Json(
        new CompositionSessionHttpResponse(
            requestId, session.SessionId, state, session.CreatedAtUtc, session.ExpiresAtUtc,
            session.Result, new CompositionBoundaryHttpResponse(Persistent: persistent)),
        statusCode: status,
        options: HttpJson.Options);
}

static CurrentTopologyHttpResponse? CreateCurrentTopologyResponse(
    string requestId,
    string sessionId,
    string configuredTenantId,
    DynamicGraphCompositionSessionRegistry.SessionEntry session,
    DynamicGraphLogicalOrchestratorSessionResult current,
    bool persistent)
{
    var bootstrap = session.Result;
    var retainedDescriptor = session.TopologyReadDescriptor;
    if (bootstrap is null
        || retainedDescriptor is null
        || current.Descriptor is null
        || !string.Equals(retainedDescriptor.ProfileId, bootstrap.ProfileId, StringComparison.Ordinal)
        || !string.Equals(retainedDescriptor.TenantId, configuredTenantId, StringComparison.Ordinal)
        || !string.Equals(current.Descriptor.ProfileId, retainedDescriptor.ProfileId, StringComparison.Ordinal)
        || !string.Equals(current.Descriptor.TenantId, retainedDescriptor.TenantId, StringComparison.Ordinal)
        || !string.Equals(current.Descriptor.DomainTag, retainedDescriptor.DomainTag, StringComparison.Ordinal)
        || !string.Equals(current.Descriptor.ContextId, retainedDescriptor.ContextId, StringComparison.Ordinal)
        || current.InitialRevisionAdvanced != bootstrap.InitialRevisionAdvanced
        || bootstrap.Revision < 1
        || bootstrap.NodeCount is < 0 or > DynamicGraphCompositionServerOptions.MaxGraphNodes
        || bootstrap.EdgeCount is < 0 or > DynamicGraphCompositionServerOptions.MaxGraphEdges
        || current.Revision < bootstrap.Revision
        || !Enum.IsDefined(current.SourceKind)
        || current.NodeCount is < 0 or > DynamicGraphCompositionServerOptions.MaxGraphNodes
        || current.EdgeCount is < 0 or > DynamicGraphCompositionServerOptions.MaxGraphEdges
        || !IsSafeTopologyEvidenceSignal(current.SignalId))
    {
        return null;
    }

    if (!Enum.TryParse<DynamicGraphBuildSourceKind>(
            bootstrap.SourceKind,
            ignoreCase: false,
            out var bootstrapSourceKind)
        || !Enum.IsDefined(bootstrapSourceKind)
        || !string.Equals(
            bootstrap.SourceKind,
            bootstrapSourceKind.ToString(),
            StringComparison.Ordinal))
    {
        return null;
    }

    if (current.Revision == bootstrap.Revision
        && (current.SourceKind != bootstrapSourceKind
            || current.NodeCount != bootstrap.NodeCount
            || current.EdgeCount != bootstrap.EdgeCount))
    {
        return null;
    }

    return new CurrentTopologyHttpResponse(
        requestId,
        sessionId,
        "current",
        bootstrap,
        new CurrentTopologyEvidenceHttpResponse(
            retainedDescriptor.ProfileId,
            current.Revision,
            current.SignalId,
            current.SourceKind.ToString(),
            current.NodeCount,
            current.EdgeCount),
        current.Revision > bootstrap.Revision,
        new CompositionBoundaryHttpResponse(
            Persistent: persistent,
            ExecutesAssistant: false,
            AppliesGraph: false,
            PerformsTraining: false));
}

static bool IsSafeTopologyEvidenceSignal(string? value)
{
    if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
        return false;

    for (var index = 0; index < value.Length; index++)
    {
        var character = value[index];
        if (character is '\r' or '\n'
            || char.IsControl(character)
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

static IResult AssistantRun(
    string requestId,
    string sessionId,
    DynamicGraphLogicalOrchestratorRunResult result,
    bool persistent)
    => Results.Json(
        new AssistantRunHttpResponse(
            requestId,
            sessionId,
            "completed",
            new AssistantRunResultHttpResponse(
                result.ScenarioId,
                result.SelectedFlowId,
                result.OverallVerdict,
                result.FinalHumanResult,
                result.Metrics,
                result.EvidenceIds),
            new CompositionBoundaryHttpResponse(Persistent: persistent)),
        statusCode: StatusCodes.Status200OK,
        options: HttpJson.Options);

static IResult TopologyAssistantRun(
    string requestId,
    string sessionId,
    DynamicGraphLogicalOrchestratorTopologyRunResult result,
    bool persistent)
    => Results.Json(
        new TopologyAssistantRunHttpResponse(
            requestId,
            sessionId,
            "completed",
            new TopologyRunEvidenceHttpResponse(
                result.TopologyRevision,
                result.TopologySignalId,
                result.TopologySourceKind.ToString()),
            new AssistantRunResultHttpResponse(
                result.Run.ScenarioId,
                result.Run.SelectedFlowId,
                result.Run.OverallVerdict,
                result.Run.FinalHumanResult,
                result.Run.Metrics,
                result.Run.EvidenceIds),
            new CompositionBoundaryHttpResponse(Persistent: persistent)),
        statusCode: StatusCodes.Status200OK,
        options: HttpJson.Options);

static TopologyMutationHttpResponse? CreateTopologyMutationResponse(
    string requestId,
    string sessionId,
    long expectedRevision,
    DynamicGraphLogicalOrchestratorBootstrapProfile profile,
    DynamicGraphTopologyApplyResult result)
{
    var build = result.BuildResult;
    var current = result.Current;
    if (build is null
        || build.Topology is null
        || build.BudgetProof is null
        || build.MutationEvidence is null
        || build.BudgetProof.ExceededDimensions is null
        || current is null
        || current.Topology is null
        || result.PreviousRevision != expectedRevision
        || result.CurrentRevision != current.Revision
        || !string.Equals(build.MutationEvidence.SignalId, profile.SignalId, StringComparison.Ordinal)
        || build.MutationEvidence.SourceKind
            != DynamicGraphBuildSourceKind.LogicalRegistryProjection
        || !string.Equals(build.MutationEvidence.SourceRef, profile.SourceRef, StringComparison.Ordinal)
        || !string.Equals(build.MutationEvidence.TraceRef, profile.TraceRef, StringComparison.Ordinal)
        || !string.Equals(build.Topology.TenantId, profile.TenantId, StringComparison.Ordinal)
        || !string.Equals(build.Topology.DomainTag, profile.DomainTag, StringComparison.Ordinal)
        || !string.Equals(build.Topology.ContextId, profile.ContextId, StringComparison.Ordinal)
        || current.SourceKind != DynamicGraphBuildSourceKind.LogicalRegistryProjection
        || !string.Equals(current.Topology.TenantId, profile.TenantId, StringComparison.Ordinal)
        || !string.Equals(current.Topology.DomainTag, profile.DomainTag, StringComparison.Ordinal)
        || !string.Equals(current.Topology.ContextId, profile.ContextId, StringComparison.Ordinal)
        || build.BudgetProof.MaxNodes != profile.Budget.MaxNodes
        || build.BudgetProof.MaxEdges != profile.Budget.MaxEdges
        || build.BudgetProof.MaxDepth != profile.Budget.MaxDepth
        || build.BudgetProof.MaxParallelBranches != profile.Budget.MaxParallelBranches
        || build.BudgetProof.ActualNodes != profile.NodeDefinitions.Length
        || build.BudgetProof.ActualEdges != profile.EdgeBindings.Length)
    {
        return null;
    }

    var proof = build.BudgetProof;
    var expectedExceededDimensions = new List<string>(4);
    if (proof.ActualNodes > proof.MaxNodes)
        expectedExceededDimensions.Add("nodes");
    if (proof.ActualEdges > proof.MaxEdges)
        expectedExceededDimensions.Add("edges");
    if (proof.ActualDepth > proof.MaxDepth)
        expectedExceededDimensions.Add("depth");
    if (proof.ActualParallelBranches > proof.MaxParallelBranches)
        expectedExceededDimensions.Add("parallel_branches");

    if (!proof.ExceededDimensions.SequenceEqual(
            expectedExceededDimensions,
            StringComparer.Ordinal)
        || proof.WithinBudget != (expectedExceededDimensions.Count == 0))
    {
        return null;
    }

    string status;
    if (result.RevisionAdvanced)
    {
        if (result.PreviousRevision == long.MaxValue
            || result.CurrentRevision != result.PreviousRevision + 1
            || !build.MutationEvidence.MutationApplied
            || build.FailureReason is not null
            || !build.BudgetProof.WithinBudget
            || !string.Equals(current.SignalId, build.MutationEvidence.SignalId, StringComparison.Ordinal)
            || current.SourceKind != DynamicGraphBuildSourceKind.LogicalRegistryProjection)
        {
            return null;
        }
        status = "applied";
    }
    else if (string.Equals(
                 build.FailureReason,
                 "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_EXCEEDED",
                 StringComparison.Ordinal))
    {
        if (result.CurrentRevision != result.PreviousRevision
            || build.MutationEvidence.MutationApplied
            || build.BudgetProof.WithinBudget)
        {
            return null;
        }
        status = "budget_rejected";
    }
    else
    {
        if (result.CurrentRevision != result.PreviousRevision
            || build.MutationEvidence.MutationApplied
            || build.FailureReason is not null
            || !build.BudgetProof.WithinBudget)
        {
            return null;
        }
        status = "unchanged";
    }

    return new TopologyMutationHttpResponse(
        requestId,
        sessionId,
        status,
        result.PreviousRevision,
        result.CurrentRevision,
        result.RevisionAdvanced,
        new TopologyMutationAttemptHttpResponse(
            build.MutationEvidence.SignalId,
            build.MutationEvidence.SourceKind.ToString(),
            build.MutationEvidence.MutationApplied),
        new TopologyMutationCurrentHttpResponse(
            current.Revision,
            current.SignalId,
            current.SourceKind.ToString(),
            current.Topology.Nodes.Count,
            current.Topology.Edges.Count),
        new TopologyMutationBudgetHttpResponse(
            proof.MaxNodes,
            proof.ActualNodes,
            proof.MaxEdges,
            proof.ActualEdges,
            proof.MaxDepth,
            proof.ActualDepth,
            proof.MaxParallelBranches,
            proof.ActualParallelBranches,
            proof.WithinBudget,
            proof.ExceededDimensions.ToArray()),
        new CompositionBoundaryHttpResponse(
            Persistent: false,
            ExecutesAssistant: false,
            AppliesGraph: true,
            PerformsTraining: false));
}

static IResult SelectedTargetEvidenceAdmission(
    string requestId,
    string sessionId,
    SelectedTargetEvidenceHandle handle)
    => Results.Json(
        new SelectedTargetEvidenceAdmissionHttpResponse(
            requestId,
            sessionId,
            "admitted",
            handle.HandleId,
            handle.ExpiresAtUtc,
            new CompositionBoundaryHttpResponse(Persistent: false)),
        statusCode: StatusCodes.Status201Created,
        options: HttpJson.Options);

static bool IsValidSelectedTargetRunSpeedMeasurement(
    DynamicGraphRunSpeedMeasurement? measurement)
{
    if (measurement?.RunSpeed is not { } axis
        || axis.InferenceElapsedMs <= 0)
    {
        return false;
    }

    var expectedThroughputRowsPerSec =
        measurement.EvaluatedRowCount * 1000d / axis.InferenceElapsedMs;
    return measurement.EvaluatedRowCount == 1
           && axis.RoutingElapsedMs == 0
           && measurement.CandidateSampleCount == 0
           && measurement.BatchSampleCount == 0
           && axis.PerCandidateElapsedMs is null
           && axis.PerBatchElapsedMs is null
           && double.IsFinite(axis.ThroughputRowsPerSec)
           && axis.ThroughputRowsPerSec > 0d
           && axis.ThroughputRowsPerSec == expectedThroughputRowsPerSec;
}

static IResult AdmittedSelectedTargetRun(
    string requestId,
    string sessionId,
    DynamicGraphL3SelectedTargetExecutionResult result,
    DynamicGraphRunSpeedMeasurement runSpeed,
    int directTargetInvocationCount)
{
    var axis = runSpeed.RunSpeed;
    return Results.Json(
        new AdmittedSelectedTargetRunHttpResponse(
            requestId,
            sessionId,
            "completed",
            result.TargetId,
            result.ExecutionId,
            result.EvidenceIds.ToArray(),
            directTargetInvocationCount,
            new SelectedTargetRunSpeedHttpResponse(
                runSpeed.EvaluatedRowCount,
                axis.InferenceElapsedMs,
                axis.ThroughputRowsPerSec,
                axis.RoutingElapsedMs,
                runSpeed.CandidateSampleCount,
                runSpeed.BatchSampleCount,
                axis.PerCandidateElapsedMs,
                axis.PerBatchElapsedMs),
            new CompositionBoundaryHttpResponse(Persistent: false)),
        statusCode: StatusCodes.Status200OK,
        options: HttpJson.Options);
}

static bool IsNonFatalException(Exception exception)
    => FatalExceptionPolicy.IsNonFatal(exception);

file readonly record struct BoundedRequestBodyRead(int Count, bool LimitExceeded);

file enum SingleSafeHeaderState
{
    Missing,
    Valid,
    Invalid,
}

file readonly record struct SingleSafeHeaderRead(SingleSafeHeaderState State, string? Value);

file static class FatalExceptionPolicy
{
    internal static bool IsNonFatal(Exception exception)
    {
        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;

            if (current is OutOfMemoryException
                || current is StackOverflowException
                || current is AccessViolationException
                || current is AppDomainUnloadedException
                || current is BadImageFormatException
                || StringComparer.Ordinal.Equals(
                    current.GetType().FullName,
                    "System.Threading.ThreadAbortException"))
            {
                return false;
            }

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }

        return true;
    }
}

static class HttpJson
{
    internal static JsonSerializerOptions Options => DynamicGraphCompositionJson.HttpOptions;

    internal static async ValueTask<T?> DeserializeStrictAsync<T>(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var document = await JsonDocument.ParseAsync(
                stream,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 64,
                },
                cancellationToken)
            .ConfigureAwait(false);
        EnsureNoDuplicateProperties(document.RootElement);
        return document.RootElement.Deserialize<T>(Options);
    }

    private static void EnsureNoDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new JsonException("Duplicate JSON properties are not allowed.");
                EnsureNoDuplicateProperties(property.Value);
            }
            return;
        }

        if (element.ValueKind != JsonValueKind.Array)
            return;
        foreach (var item in element.EnumerateArray())
            EnsureNoDuplicateProperties(item);
    }
}

static class ReadyFile
{
    internal static void RemoveStale(string path)
    {
        var normalized = DynamicGraphCompositionFileSystemPolicy.NormalizeReadyFile(path);
        var directory = Path.GetDirectoryName(normalized)!;
        var fileName = Path.GetFileName(normalized);
        if (!DynamicGraphCompositionFileSystemPolicy.PathExists(directory))
            return;
        DynamicGraphCompositionFileSystemPolicy.DeleteManagedFileIfPresent(directory, fileName);
    }

    internal static void RemoveSafe(string path)
    {
        try { RemoveStale(path); }
        catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
        {
            /* Cleanup cannot change the already latched runtime readiness state. */
        }
    }

    internal static void WriteAtomic(string path, string baseUrl, bool persistent)
    {
        path = DynamicGraphCompositionFileSystemPolicy.NormalizeReadyFile(path);
        var directory = Path.GetDirectoryName(path)!;
        DynamicGraphCompositionFileSystemPolicy.EnsureSafeExistingComponents(directory);
        Directory.CreateDirectory(directory);
        DynamicGraphCompositionFileSystemPolicy.EnsureDirectory(directory);
        var fileName = Path.GetFileName(path);
        var temporaryName = $".{fileName}.{Guid.NewGuid():N}.tmp";
        var temporary = DynamicGraphCompositionFileSystemPolicy.DirectChild(
            directory,
            temporaryName);
        Exception? operationFailure = null;
        try
        {
            var payload = new
            {
                base_url = baseUrl,
                status = "ready",
                loopback_only = true,
                tenant_binding_is_authentication = false,
                persistent,
                executes_assistant = true,
                applies_graph = !persistent,
                performs_training = false,
                routes = new[]
                {
                    "GET /api/v1/dynamic-graph/health",
                    "GET /api/v1/dynamic-graph/readiness",
                    "POST /api/v1/dynamic-graph/l2-untrained-honesty-runs",
                    "POST /api/v1/dynamic-graph/sessions",
                    "POST /api/v1/dynamic-graph/catalog-sessions",
                    "GET /api/v1/dynamic-graph/sessions/{sessionId}",
                    "GET /api/v1/dynamic-graph/sessions/{sessionId}/topology",
                    "POST /api/v1/dynamic-graph/sessions/{sessionId}/selected-target-evidence",
                    "POST /api/v1/dynamic-graph/sessions/{sessionId}/selected-target-runs",
                    "POST /api/v1/dynamic-graph/sessions/{sessionId}/topology-mutations",
                    "POST /api/v1/dynamic-graph/sessions/{sessionId}/topology-runs",
                    "POST /api/v1/dynamic-graph/sessions/{sessionId}/runs"
                }
            };
            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, HttpJson.Options);
            Exception? bytesFailure = null;
            try
            {
                DynamicGraphCompositionFileSystemPolicy.EnsureRegularFileOrMissing(path);
                DynamicGraphCompositionFileSystemPolicy.EnsureRegularFileOrMissing(temporary);
                var stream = new FileStream(
                    temporary,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    FileOptions.WriteThrough);
                Exception? streamFailure = null;
                try
                {
                    stream.Write(bytes);
                    stream.Flush(flushToDisk: true);
                }
                catch (Exception exception)
                {
                    streamFailure = exception;
                    throw;
                }
                finally
                {
                    if (streamFailure is null || FatalExceptionPolicy.IsNonFatal(streamFailure))
                        stream.Dispose();
                }
                DynamicGraphCompositionFileSystemPolicy.EnsureRegularFile(temporary);
                DynamicGraphCompositionFileSystemPolicy.EnsureRegularFileOrMissing(path);
                File.Move(temporary, path, true);
                DynamicGraphCompositionFileSystemPolicy.EnsureRegularFile(path);
            }
            catch (Exception exception)
            {
                bytesFailure = exception;
                throw;
            }
            finally
            {
                if (bytesFailure is null || FatalExceptionPolicy.IsNonFatal(bytesFailure))
                    CryptographicOperations.ZeroMemory(bytes);
            }
        }
        catch (Exception exception)
        {
            operationFailure = exception;
            throw;
        }
        finally
        {
            if (operationFailure is null || FatalExceptionPolicy.IsNonFatal(operationFailure))
            {
                DynamicGraphCompositionFileSystemPolicy.DeleteManagedFileIfPresent(
                    directory,
                    temporaryName);
            }
        }
    }
}

sealed class StartupReadinessState : IDisposable
{
    private readonly object _gate = new();
    private readonly List<Action> _invalidationCallbacks = [];
    private readonly string _readyFile;
    private readonly bool _persistent;
    private int _listening;
    private int _forbiddenAssemblyLoaded;
    private int _activeInvalidationWork;
    private int _disposed;
    private string? _failureReasonCode;

    internal StartupReadinessState(string readyFile, bool persistent)
    {
        _readyFile = readyFile;
        _persistent = persistent;
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        try
        {
            if (AppDomain.CurrentDomain.GetAssemblies().Any(assembly => IsForbidden(assembly.GetName().Name)))
                Interlocked.Exchange(ref _forbiddenAssemblyLoaded, 1);
        }
        catch (Exception initializationFailure)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
            }
            catch (Exception cleanupFailure) when (FatalExceptionPolicy.IsNonFatal(cleanupFailure))
            {
                /* A non-fatal compensation failure cannot replace the initialization failure. */
            }

            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(initializationFailure).Throw();
        }
    }

    internal bool IsReady => Volatile.Read(ref _disposed) == 0
        && Volatile.Read(ref _listening) == 1
        && Volatile.Read(ref _forbiddenAssemblyLoaded) == 0
        && Volatile.Read(ref _failureReasonCode) is null;

    internal string? ReasonCode
    {
        get
        {
            var failureReasonCode = Volatile.Read(ref _failureReasonCode);
            if (failureReasonCode is not null)
                return failureReasonCode;
            if (Volatile.Read(ref _forbiddenAssemblyLoaded) == 1)
                return "FORBIDDEN_TRAINING_ASSEMBLY_LOADED";
            if (Volatile.Read(ref _disposed) != 0)
                return "HOST_NOT_READY";
            return Volatile.Read(ref _listening) == 0 ? "HOST_LISTENER_NOT_STARTED" : null;
        }
    }

    internal bool AllowsComposition(bool requireReady)
        => Volatile.Read(ref _disposed) == 0
           && (requireReady
            ? IsReady
            : Volatile.Read(ref _forbiddenAssemblyLoaded) == 0
              && Volatile.Read(ref _failureReasonCode) is null);

    internal void RegisterInvalidation(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        InvalidationWorkLease? workLease = null;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
            if (Volatile.Read(ref _forbiddenAssemblyLoaded) == 1
                || Volatile.Read(ref _failureReasonCode) is not null)
                workLease = AdmitInvalidationWork();
            else
                _invalidationCallbacks.Add(callback);
        }

        if (workLease is not null)
        {
            using (workLease)
            {
                InvokeInvalidation(callback);
            }
        }
    }

    internal bool PublishReady(string baseUrl)
    {
        lock (_gate)
        {
            if (Volatile.Read(ref _disposed) != 0)
                return false;
            Interlocked.Exchange(ref _listening, 1);
            if (Volatile.Read(ref _forbiddenAssemblyLoaded) == 1
                || Volatile.Read(ref _failureReasonCode) is not null)
                return false;
            ReadyFile.WriteAtomic(_readyFile, baseUrl, _persistent);
            return true;
        }
    }

    internal void FailClosed(string reasonCode)
    {
        if (string.IsNullOrWhiteSpace(reasonCode))
            reasonCode = "HOST_NOT_READY";

        Action[] callbacks;
        InvalidationWorkLease workLease;
        lock (_gate)
        {
            if (Volatile.Read(ref _disposed) != 0)
                return;
            if (Interlocked.CompareExchange(ref _failureReasonCode, reasonCode, null) is not null)
                return;
            callbacks = _invalidationCallbacks.ToArray();
            workLease = AdmitInvalidationWork();
            _invalidationCallbacks.Clear();
        }

        using (workLease)
        {
            try { ReadyFile.RemoveStale(_readyFile); }
            catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
            {
                /* Readiness remains latched false even if stale-file cleanup is denied. */
            }
            foreach (var callback in callbacks)
                InvokeInvalidation(callback);
        }
    }

    private void OnAssemblyLoad(object? _, AssemblyLoadEventArgs args)
    {
        if (!IsForbidden(args.LoadedAssembly.GetName().Name))
            return;

        Action[] callbacks;
        InvalidationWorkLease workLease;
        lock (_gate)
        {
            if (Volatile.Read(ref _disposed) != 0)
                return;
            if (Volatile.Read(ref _forbiddenAssemblyLoaded) == 1)
                return;
            Interlocked.Exchange(ref _forbiddenAssemblyLoaded, 1);
            callbacks = _invalidationCallbacks.ToArray();
            workLease = AdmitInvalidationWork();
            _invalidationCallbacks.Clear();
        }

        using (workLease)
        {
            try { ReadyFile.RemoveStale(_readyFile); }
            catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
            {
                /* Readiness remains latched false even if stale-file cleanup is denied. */
            }
            foreach (var callback in callbacks)
                InvokeInvalidation(callback);
        }
    }

    private InvalidationWorkLease AdmitInvalidationWork()
    {
        var workLease = new InvalidationWorkLease(this);
        var nextCount = checked(_activeInvalidationWork + 1);
        _activeInvalidationWork = nextCount;
        return workLease;
    }

    private void CompleteInvalidationWork()
    {
        lock (_gate)
        {
            if (_activeInvalidationWork <= 0)
                throw new InvalidOperationException("The readiness invalidation work count is invalid.");

            _activeInvalidationWork--;
            if (_activeInvalidationWork == 0)
                Monitor.PulseAll(_gate);
        }
    }

    private sealed class InvalidationWorkLease : IDisposable
    {
        private StartupReadinessState? _owner;

        internal InvalidationWorkLease(StartupReadinessState owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            owner?.CompleteInvalidationWork();
        }
    }

    private static void InvokeInvalidation(Action callback)
    {
        try { callback(); }
        catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
        {
            /* The readiness latch remains fail-closed if cleanup itself fails. */
        }
    }

    private static bool IsForbidden(string? name)
        => string.Equals(name, "DPO", StringComparison.OrdinalIgnoreCase)
           || string.Equals(name, "TrainingLoop", StringComparison.OrdinalIgnoreCase);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
        lock (_gate)
        {
            Interlocked.Exchange(ref _listening, 0);
            while (_activeInvalidationWork != 0)
                Monitor.Wait(_gate);
            _invalidationCallbacks.Clear();
        }
        ReadyFile.RemoveStale(_readyFile);
    }
}

sealed class CompositionRejectedException(string code) : Exception
{
    internal string Code { get; } = code;
}

sealed class ForbiddenTrainingAssemblyException : Exception;

public partial class Program;
