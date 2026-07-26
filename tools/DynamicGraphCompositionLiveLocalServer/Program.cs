// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure.Logical;
using LlmFromScratch.DynamicGraphCompositionLiveLocalServer;
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
    builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = Array.Empty<string>(),
        ApplicationName = typeof(Program).Assembly.FullName,
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
    Results.Ok(new ReadinessHttpResponse("live", Persistent: persistenceOwner.IsEnabled)));
app.MapGet("/api/v1/dynamic-graph/readiness", () => readiness.IsReady
    ? Results.Ok(new ReadinessHttpResponse("ready", Persistent: persistenceOwner.IsEnabled))
    : Results.Json(
        new ReadinessHttpResponse(
            "not_ready",
            readiness.ReasonCode,
            Persistent: persistenceOwner.IsEnabled),
        statusCode: StatusCodes.Status503ServiceUnavailable));

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

static IResult Error(int status, string requestId, string code, string message)
    => Results.Json(new SafeErrorHttpResponse(requestId, code, message), statusCode: status, options: HttpJson.Options);

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

static bool IsNonFatalException(Exception exception)
    => FatalExceptionPolicy.IsNonFatal(exception);

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
                applies_graph = false,
                performs_training = false,
                routes = new[]
                {
                    "GET /api/v1/dynamic-graph/health",
                    "GET /api/v1/dynamic-graph/readiness",
                    "POST /api/v1/dynamic-graph/sessions",
                    "GET /api/v1/dynamic-graph/sessions/{sessionId}",
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
