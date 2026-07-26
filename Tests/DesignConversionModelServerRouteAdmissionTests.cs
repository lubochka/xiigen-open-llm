// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Globalization;
using AgentInfrastructure;
using DesignConversionDomain;
using Microsoft.AspNetCore.Http;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerRouteAdmissionTests
{
    private static readonly DateTimeOffset FixedNow = DateTimeOffset.Parse("2026-07-11T12:00:00Z");
    private const string CurrentToken = "current-route-token-0123456789";
    private const string PreviousToken = "previous-route-token-0123456789";
    private const string TenantA = "tenant-route-admission-alpha";
    private const string TenantB = "tenant-route-admission-beta";
    private const int DurableReplayRecordMaxBytes = 1024 * 1024 + 64 * 1024;

    [Fact]
    public void AdmitEvaluate_RequiresValidCurrentOrActivePreviousTokenBeforeIdempotency()
    {
        var admission = CreateAdmission();

        var missing = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Idempotency: "key-1")));
        Assert.False(missing.IsAccepted);
        Assert.Equal(401, missing.StatusCode);
        Assert.Equal("DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED", missing.ErrorCode);
        Assert.Equal(bool.FalseString, missing.BoundaryMetadata["runtime_invoked"]);

        var malformed = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Authorization: "Token nope", Idempotency: "key-1")));
        Assert.False(malformed.IsAccepted);
        Assert.Equal(401, malformed.StatusCode);
        Assert.Equal("DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_MALFORMED", malformed.ErrorCode);

        var invalid = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: "wrong-token", Idempotency: "key-1")));
        Assert.False(invalid.IsAccepted);
        Assert.Equal(403, invalid.StatusCode);
        Assert.Equal("DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_INVALID", invalid.ErrorCode);

        var current = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "key-1")));
        Assert.True(current.IsAccepted);
        Assert.Contains(DesignConversionModelServerRouteAdmission.RouteScopeDefault, current.Scope, StringComparison.Ordinal);
        Assert.Contains(DesignConversionModelActivationRegistry.BuildTenantIdRef(TenantA), current.Scope, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, current.Scope, StringComparison.Ordinal);
        Assert.True(current.AdmissionContext.TenantBound);
        Assert.Equal(DesignConversionModelActivationRegistry.BuildTenantIdRef(TenantA), current.AdmissionContext.TenantIdRef);

        var previous = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: PreviousToken, Idempotency: "key-2")));
        Assert.True(previous.IsAccepted);

        var retiredAdmission = CreateAdmission(previousExpiresAtUtc: FixedNow.AddSeconds(-1));
        var retired = retiredAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: PreviousToken, Idempotency: "key-3")));
        Assert.False(retired.IsAccepted);
        Assert.Equal(403, retired.StatusCode);
        Assert.Equal("previous_token_retired", retired.BoundaryMetadata["route_admission_rotation_state"]);
    }

    [Fact]
    public void EnabledAdmission_RequiresConfiguredTenantAndPublishesOnlyTenantRef()
    {
        var ready = CreateAdmission();
        var metadata = ready.PublicMetadata();
        Assert.True((bool)metadata["routeAdmissionTenantBound"]!);
        Assert.Equal(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(TenantA),
            metadata["routeAdmissionTenantIdRef"]);
        var metadataJson = JsonSerializer.Serialize(metadata);
        Assert.DoesNotContain(TenantA, metadataJson, StringComparison.Ordinal);
        Assert.DoesNotContain(CurrentToken, metadataJson, StringComparison.Ordinal);
        Assert.DoesNotContain(PreviousToken, metadataJson, StringComparison.Ordinal);

        var missingTenant = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: CurrentToken),
            () => FixedNow);
        Assert.True(missingTenant.Enabled);
        Assert.False(missingTenant.Ready);
        Assert.False((bool)missingTenant.PublicMetadata()["routeAdmissionTenantBound"]!);

        var rejected = missingTenant.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "missing-tenant")));
        Assert.False(rejected.IsAccepted);
        Assert.Equal(503, rejected.StatusCode);
        Assert.Equal("DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_REQUIRED", rejected.ErrorCode);
        Assert.Equal(bool.FalseString, rejected.BoundaryMetadata["route_admission_tenant_bound"]);
        Assert.DoesNotContain(CurrentToken, rejected.ErrorMessage, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(InvalidTenantIds))]
    public void EnabledAdmission_RejectsUnsafeConfiguredTenantIds(string tenantId, string expectedErrorCode)
    {
        var admission = CreateAdmission(tenantId: tenantId);

        Assert.True(admission.Enabled);
        Assert.False(admission.Ready);
        Assert.False((bool)admission.PublicMetadata()["routeAdmissionTenantBound"]!);

        var rejected = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "invalid-tenant")));
        Assert.False(rejected.IsAccepted);
        Assert.Equal(503, rejected.StatusCode);
        Assert.Equal(expectedErrorCode, rejected.ErrorCode);
        Assert.Equal(bool.FalseString, rejected.BoundaryMetadata["route_admission_tenant_bound"]);
        Assert.Equal(bool.FalseString, rejected.BoundaryMetadata["runtime_invoked"]);
        if (!string.IsNullOrWhiteSpace(tenantId) && tenantId != ".")
            Assert.DoesNotContain(tenantId, rejected.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void EnabledAdmission_CanonicalizesCaseVariantsToSameTenantRef()
    {
        var lower = CreateAdmission(tenantId: TenantA);
        var upper = CreateAdmission(tenantId: TenantA.ToUpperInvariant());
        var lowerDecision = lower.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "case-lower")));
        var upperDecision = upper.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "case-upper")));

        Assert.True(lower.Ready);
        Assert.True(upper.Ready);
        Assert.True(lowerDecision.IsAccepted);
        Assert.True(upperDecision.IsAccepted);
        Assert.Equal(lowerDecision.AdmissionContext.TenantIdRef, upperDecision.AdmissionContext.TenantIdRef);
        Assert.Equal(TenantA, upperDecision.AdmissionContext.TenantId);
        Assert.Equal(lowerDecision.Scope, upperDecision.Scope);
    }

    [Fact]
    public async Task StatusAdmission_IsScopedAndDeniedStatusDoesNotDiscloseJobIds()
    {
        const string jobId = "thin-job-route-admission-1";
        const string unknownJobId = "thin-job-PRIVATE-PATH-SECRET";
        var admission = CreateAdmission();

        var evaluate = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "bind-job")));
        Assert.True(evaluate.IsAccepted);
        var response = await admission.ExecuteEvaluateOnceAsync(
            evaluate,
            """{"requestId":"bind-job","value":1}""",
            _ => Task.FromResult(JsonResponse(200, $$"""{"jobId":"{{jobId}}","requestId":"bind-job"}""")),
            Conflict,
            InvalidJson,
            CancellationToken.None);
        Assert.Equal(200, response.StatusCode);

        var currentStatus = admission.AdmitStatus(StatusRequest(jobId, Header(Token: CurrentToken)));
        var previousStatus = admission.AdmitStatus(StatusRequest(jobId, Header(Token: PreviousToken)));
        Assert.True(currentStatus.IsAccepted);
        Assert.True(previousStatus.IsAccepted);
        Assert.True(admission.IsStatusJobAllowed(currentStatus, jobId));
        Assert.True(admission.IsStatusJobAllowed(previousStatus, jobId));

        var missingToken = admission.AdmitStatus(StatusRequest(jobId, Header()));
        Assert.False(missingToken.IsAccepted);
        Assert.Equal(401, missingToken.StatusCode);

        var invalidToken = admission.AdmitStatus(StatusRequest(jobId, Header(Token: "wrong-token")));
        Assert.False(invalidToken.IsAccepted);
        Assert.Equal(403, invalidToken.StatusCode);

        var evaluator = new ThrowingEvaluator();
        var api = new DesignConversionModelServerApi(evaluator, () => FixedNow);
        var handler = new DesignConversionModelServerStatusHandler(api, admission);
        var denied = await handler.HandleAsync(StatusRequest(unknownJobId, Header(Token: CurrentToken)));

        Assert.Equal(404, denied.StatusCode);
        Assert.Equal(0, evaluator.CallCount);
        Assert.DoesNotContain(unknownJobId, denied.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(CurrentToken, denied.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(PreviousToken, denied.BodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(denied.BodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(
            DesignConversionModelServerStatusHandler.EndpointTemplatePath,
            metadata.GetProperty("route_admission_path").GetString());
    }

    [Fact]
    public void AdmitEvaluate_RejectsMissingInvalidAndTooLongIdempotencyKey()
    {
        var admission = CreateAdmission();

        var missing = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken)));
        Assert.False(missing.IsAccepted);
        Assert.Equal(400, missing.StatusCode);
        Assert.Equal("DESIGN_CONVERSION_IDEMPOTENCY_KEY_REQUIRED", missing.ErrorCode);

        var invalid = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "key with spaces")));
        Assert.False(invalid.IsAccepted);
        Assert.Equal(400, invalid.StatusCode);
        Assert.Equal("DESIGN_CONVERSION_IDEMPOTENCY_KEY_INVALID", invalid.ErrorCode);

        var tooLong = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: new string('a', 129))));
        Assert.False(tooLong.IsAccepted);
        Assert.Equal(400, tooLong.StatusCode);
        Assert.Equal("DESIGN_CONVERSION_IDEMPOTENCY_KEY_TOO_LONG", tooLong.ErrorCode);
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_ReplaysSequentialAndConcurrentSameBodyOnceAndConflictsDifferentBody()
    {
        var admission = CreateAdmission();
        var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "same-key")));
        Assert.True(decision.IsAccepted);
        var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var executeCount = 0;
        var appendCount = 0;

        async Task<DesignConversionModelServerHttpResponse> Execute(CancellationToken ct)
        {
            Interlocked.Increment(ref executeCount);
            Interlocked.Increment(ref appendCount);
            firstStarted.TrySetResult(true);
            await release.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
            return JsonResponse(200, """{"jobId":"thin-job-same-key","requestId":"same-key"}""");
        }

        var first = admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"b":2,"a":1}""",
            Execute,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var concurrentReplay = admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"b":2,"a":1}""",
            Execute,
            Conflict,
            InvalidJson,
            CancellationToken.None);

        release.TrySetResult(true);
        var concurrentResponses = await Task.WhenAll(first, concurrentReplay);
        var sequentialReplay = await admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"a":1,"b":2}""",
            Execute,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        var conflict = await admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"a":1,"b":3}""",
            Execute,
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.All(concurrentResponses, item => Assert.Equal(200, item.StatusCode));
        Assert.Equal(concurrentResponses[0].BodyJson, concurrentResponses[1].BodyJson);
        Assert.Equal(concurrentResponses[0].BodyJson, sequentialReplay.BodyJson);
        Assert.Equal(409, conflict.StatusCode);
        Assert.Equal(1, executeCount);
        Assert.Equal(1, appendCount);

        var status = admission.AdmitStatus(StatusRequest("thin-job-same-key", Header(Token: CurrentToken)));
        Assert.True(admission.IsStatusJobAllowed(status, "thin-job-same-key"));
    }

    [Fact]
    public async Task ExecuteIdempotentMutationOnceAsync_ReplaysConflictsAndSeparatesRoutesInMemory()
    {
        const string routeA = "/api/design-conversion/future-png-upload";
        const string routeB = "/api/design-conversion/future-png-upload/commit";
        const string idempotencyKey = "mutation-same-key";
        const string bodyA = """{"b":2,"a":1}""";
        var admission = CreateAdmission();
        var decisionA = admission.AdmitIdempotentMutation(
            MutationRequest(" post ", routeA + "?attempt=1", Header(Token: CurrentToken, Idempotency: idempotencyKey)));
        var equivalentRouteDecision = admission.AdmitIdempotentMutation(
            MutationRequest("POST", routeA, Header(Token: CurrentToken, Idempotency: idempotencyKey)));
        var decisionB = admission.AdmitIdempotentMutation(
            MutationRequest("POST", routeB, Header(Token: CurrentToken, Idempotency: idempotencyKey)));

        Assert.True(decisionA.IsAccepted);
        Assert.True(equivalentRouteDecision.IsAccepted);
        Assert.True(decisionB.IsAccepted);
        Assert.Equal(decisionA.Scope, equivalentRouteDecision.Scope);
        Assert.NotEqual(decisionA.Scope, decisionB.Scope);
        Assert.Equal(
            decisionA.BoundaryMetadata["idempotency_effective_scope_ref"],
            equivalentRouteDecision.BoundaryMetadata["idempotency_effective_scope_ref"]);
        Assert.NotEqual(
            decisionA.BoundaryMetadata["idempotency_route_scope_ref"],
            decisionB.BoundaryMetadata["idempotency_route_scope_ref"]);
        AssertMutationMetadataDoesNotLeak(decisionA, routeA, bodyA);

        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var routeAExecuteCount = 0;

        async Task<DesignConversionModelServerHttpResponse> ExecuteRouteA(CancellationToken ct)
        {
            Interlocked.Increment(ref routeAExecuteCount);
            started.TrySetResult(true);
            await release.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
            return JsonResponse(200, """{"jobId":"thin-job-mutation-route-a","route":"a"}""");
        }

        var first = admission.ExecuteIdempotentMutationOnceAsync(
            decisionA,
            bodyA,
            ExecuteRouteA,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var concurrentReplay = admission.ExecuteIdempotentMutationOnceAsync(
            equivalentRouteDecision,
            """{"a":1,"b":2}""",
            ExecuteRouteA,
            Conflict,
            InvalidJson,
            CancellationToken.None);

        release.TrySetResult(true);
        var replayed = await Task.WhenAll(first, concurrentReplay).WaitAsync(TimeSpan.FromSeconds(5));
        var conflict = await admission.ExecuteIdempotentMutationOnceAsync(
            decisionA,
            """{"a":1,"b":3}""",
            ExecuteRouteA,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        var routeBExecuteCount = 0;
        var independentRoute = await admission.ExecuteIdempotentMutationOnceAsync(
            decisionB,
            """{"a":1,"b":3}""",
            _ =>
            {
                Interlocked.Increment(ref routeBExecuteCount);
                return Task.FromResult(JsonResponse(201, """{"jobId":"thin-job-mutation-route-b","route":"b"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.All(replayed, response => Assert.Equal(200, response.StatusCode));
        Assert.Equal(replayed[0].BodyJson, replayed[1].BodyJson);
        Assert.Equal(409, conflict.StatusCode);
        Assert.Equal(201, independentRoute.StatusCode);
        Assert.Equal(1, routeAExecuteCount);
        Assert.Equal(1, routeBExecuteCount);
    }

    [Fact]
    public async Task ExecuteIdempotentMutationOnceAsync_DurableRestartReplaysSameRouteAndSeparatesRoutes()
    {
        const string routeA = "/api/design-conversion/future-png-upload";
        const string routeB = "/api/design-conversion/future-png-upload/commit";
        const string idempotencyKey = "durable-mutation-same-key";
        var storeRoot = CreateTempDirectory();
        try
        {
            var firstAdmission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var firstDecision = firstAdmission.AdmitIdempotentMutation(
                MutationRequest("POST", routeA + "?attempt=1", Header(Token: CurrentToken, Idempotency: idempotencyKey)));
            Assert.True(firstDecision.IsAccepted);
            var firstExecuteCount = 0;
            var first = await firstAdmission.ExecuteIdempotentMutationOnceAsync(
                firstDecision,
                """{"requestId":"durable-mutation","value":1}""",
                _ =>
                {
                    Interlocked.Increment(ref firstExecuteCount);
                    return Task.FromResult(JsonResponse(
                        202,
                        """{"jobId":"thin-job-durable-mutation-a","route":"a","status":"accepted"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);
            Assert.Equal(202, first.StatusCode);
            Assert.Equal(1, firstExecuteCount);

            var secondAdmission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var replayDecision = secondAdmission.AdmitIdempotentMutation(
                MutationRequest(" post ", routeA, Header(Token: CurrentToken, Idempotency: idempotencyKey)));
            Assert.True(replayDecision.IsAccepted);
            Assert.Equal(firstDecision.Scope, replayDecision.Scope);
            AssertMutationMetadataDoesNotLeak(
                replayDecision,
                routeA,
                """{"value":1,"requestId":"durable-mutation"}""");
            var replayExecuteCount = 0;
            var replay = await secondAdmission.ExecuteIdempotentMutationOnceAsync(
                replayDecision,
                """{"value":1,"requestId":"durable-mutation"}""",
                _ =>
                {
                    Interlocked.Increment(ref replayExecuteCount);
                    return Task.FromResult(JsonResponse(500, """{"errorCode":"SHOULD_NOT_EXECUTE"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            var routeBDecision = secondAdmission.AdmitIdempotentMutation(
                MutationRequest("POST", routeB, Header(Token: CurrentToken, Idempotency: idempotencyKey)));
            Assert.True(routeBDecision.IsAccepted);
            Assert.NotEqual(replayDecision.Scope, routeBDecision.Scope);
            var routeBExecuteCount = 0;
            var routeBResponse = await secondAdmission.ExecuteIdempotentMutationOnceAsync(
                routeBDecision,
                """{"requestId":"durable-mutation","value":2}""",
                _ =>
                {
                    Interlocked.Increment(ref routeBExecuteCount);
                    return Task.FromResult(JsonResponse(
                        201,
                        """{"jobId":"thin-job-durable-mutation-b","route":"b","status":"accepted"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            var thirdAdmission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var routeBReplayDecision = thirdAdmission.AdmitIdempotentMutation(
                MutationRequest("POST", routeB, Header(Token: CurrentToken, Idempotency: idempotencyKey)));
            var routeBReplayExecuteCount = 0;
            var routeBReplay = await thirdAdmission.ExecuteIdempotentMutationOnceAsync(
                routeBReplayDecision,
                """{"value":2,"requestId":"durable-mutation"}""",
                _ =>
                {
                    Interlocked.Increment(ref routeBReplayExecuteCount);
                    return Task.FromResult(JsonResponse(500, """{"errorCode":"SHOULD_NOT_EXECUTE"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(first.StatusCode, replay.StatusCode);
            Assert.Equal(first.BodyJson, replay.BodyJson);
            Assert.Equal(0, replayExecuteCount);
            Assert.Equal(201, routeBResponse.StatusCode);
            Assert.Equal(1, routeBExecuteCount);
            Assert.Equal(routeBResponse.BodyJson, routeBReplay.BodyJson);
            Assert.Equal(0, routeBReplayExecuteCount);
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_ReplaysAcrossAdmissionRestartAndBindsStatusScope()
    {
        var storeRoot = CreateTempDirectory();
        try
        {
            var firstAdmission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var firstDecision = firstAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-same-key")));
            Assert.True(firstDecision.IsAccepted);
            var firstExecuteCount = 0;
            var first = await firstAdmission.ExecuteEvaluateOnceAsync(
                firstDecision,
                """{"requestId":"durable-replay-public","value":1}""",
                _ =>
                {
                    Interlocked.Increment(ref firstExecuteCount);
                    return Task.FromResult(JsonResponse(
                        202,
                        """{"jobId":"thin-job-durable-replay-public","requestId":"durable-replay-public","status":"accepted"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);
            Assert.Equal(202, first.StatusCode);
            Assert.Equal(1, firstExecuteCount);

            var secondAdmission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var secondDecision = secondAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-same-key")));
            Assert.True(secondDecision.IsAccepted);
            var replayExecuteCount = 0;
            var replay = await secondAdmission.ExecuteEvaluateOnceAsync(
                secondDecision,
                """{"value":1,"requestId":"durable-replay-public"}""",
                _ =>
                {
                    Interlocked.Increment(ref replayExecuteCount);
                    return Task.FromResult(JsonResponse(500, """{"errorCode":"SHOULD_NOT_EXECUTE"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(first.StatusCode, replay.StatusCode);
            Assert.Equal(first.ContentType, replay.ContentType);
            Assert.Equal(first.BodyJson, replay.BodyJson);
            Assert.Equal(0, replayExecuteCount);

            var status = secondAdmission.AdmitStatus(StatusRequest("thin-job-durable-replay-public", Header(Token: CurrentToken)));
            Assert.True(secondAdmission.IsStatusJobAllowed(status, "thin-job-durable-replay-public"));
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_ConflictsAcrossAdmissionRestartWithoutExecuting()
    {
        var storeRoot = CreateTempDirectory();
        try
        {
            var firstAdmission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var firstDecision = firstAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-conflict-key")));
            Assert.True(firstDecision.IsAccepted);
            var first = await firstAdmission.ExecuteEvaluateOnceAsync(
                firstDecision,
                """{"requestId":"durable-conflict","value":1}""",
                _ => Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-durable-conflict","requestId":"durable-conflict"}""")),
                Conflict,
                InvalidJson,
                CancellationToken.None);
            Assert.Equal(200, first.StatusCode);

            var secondAdmission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var secondDecision = secondAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-conflict-key")));
            Assert.True(secondDecision.IsAccepted);
            var executeCount = 0;
            var conflict = await secondAdmission.ExecuteEvaluateOnceAsync(
                secondDecision,
                """{"requestId":"durable-conflict","value":2}""",
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-should-not-execute"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(409, conflict.StatusCode);
            Assert.Equal("""{"errorCode":"DESIGN_CONVERSION_IDEMPOTENCY_KEY_CONFLICT"}""", conflict.BodyJson);
            Assert.Equal(0, executeCount);
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_ExpiredRecordPrunesAndExecutesFresh()
    {
        var storeRoot = CreateTempDirectory();
        var now = FixedNow;
        try
        {
            var firstAdmission = CreateAdmission(
                idempotencyTtl: TimeSpan.FromSeconds(1),
                utcNow: () => now,
                durableIdempotencyStoreRoot: storeRoot);
            var firstDecision = firstAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-expired-key")));
            Assert.True(firstDecision.IsAccepted);
            var first = await firstAdmission.ExecuteEvaluateOnceAsync(
                firstDecision,
                """{"requestId":"durable-expired","value":1}""",
                _ => Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-durable-expired-old","requestId":"durable-expired"}""")),
                Conflict,
                InvalidJson,
                CancellationToken.None);
            Assert.Equal(200, first.StatusCode);

            now = now.AddSeconds(2);
            var secondAdmission = CreateAdmission(
                idempotencyTtl: TimeSpan.FromSeconds(1),
                utcNow: () => now,
                durableIdempotencyStoreRoot: storeRoot);
            var secondDecision = secondAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-expired-key")));
            Assert.True(secondDecision.IsAccepted);
            var freshExecuteCount = 0;
            var fresh = await secondAdmission.ExecuteEvaluateOnceAsync(
                secondDecision,
                """{"requestId":"durable-expired","value":1}""",
                _ =>
                {
                    Interlocked.Increment(ref freshExecuteCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-durable-expired-fresh","requestId":"durable-expired"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(200, fresh.StatusCode);
            Assert.Contains("thin-job-durable-expired-fresh", fresh.BodyJson, StringComparison.Ordinal);
            Assert.Equal(1, freshExecuteCount);
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_TamperedSchemaAndOversizedRecordsFailClosedWithoutExecuting()
    {
        var tamperedStoreRoot = CreateTempDirectory();
        var schemaStoreRoot = CreateTempDirectory();
        var oversizedStoreRoot = CreateTempDirectory();
        try
        {
            await PersistDurableRecordAsync(
                tamperedStoreRoot,
                "durable-tamper-key",
                """{"requestId":"durable-tamper","value":1}""",
                """{"jobId":"thin-job-durable-tamper","requestId":"durable-tamper"}""");
            var tamperedPath = Assert.Single(Directory.EnumerateFiles(tamperedStoreRoot, "*.json", SearchOption.AllDirectories));
            var tamperedText = await File.ReadAllTextAsync(tamperedPath);
            await File.WriteAllTextAsync(
                tamperedPath,
                tamperedText.Replace("thin-job-durable-tamper", "thin-job-durable-tamper-edited", StringComparison.Ordinal),
                Encoding.UTF8);

            var tamperedAdmission = CreateAdmission(durableIdempotencyStoreRoot: tamperedStoreRoot);
            var tamperedDecision = tamperedAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-tamper-key")));
            var tamperedExecuteCount = 0;
            var tampered = await tamperedAdmission.ExecuteEvaluateOnceAsync(
                tamperedDecision,
                """{"requestId":"durable-tamper","value":1}""",
                _ =>
                {
                    Interlocked.Increment(ref tamperedExecuteCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-should-not-execute"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(503, tampered.StatusCode);
            Assert.Contains("DESIGN_CONVERSION_IDEMPOTENCY_DURABLE_REPLAY_FAILED_CLOSED", tampered.BodyJson, StringComparison.Ordinal);
            Assert.Equal(0, tamperedExecuteCount);

            await PersistDurableRecordAsync(
                schemaStoreRoot,
                "durable-schema-key",
                """{"requestId":"durable-schema","value":1}""",
                """{"jobId":"thin-job-durable-schema","requestId":"durable-schema"}""");
            var schemaPath = Assert.Single(Directory.EnumerateFiles(schemaStoreRoot, "*.json", SearchOption.AllDirectories));
            var schemaText = await File.ReadAllTextAsync(schemaPath);
            await File.WriteAllTextAsync(
                schemaPath,
                schemaText.Replace("\"SchemaVersion\":1", "\"SchemaVersion\":999", StringComparison.Ordinal),
                Encoding.UTF8);

            var schemaAdmission = CreateAdmission(durableIdempotencyStoreRoot: schemaStoreRoot);
            var schemaDecision = schemaAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-schema-key")));
            var schemaExecuteCount = 0;
            var schema = await schemaAdmission.ExecuteEvaluateOnceAsync(
                schemaDecision,
                """{"requestId":"durable-schema","value":1}""",
                _ =>
                {
                    Interlocked.Increment(ref schemaExecuteCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-should-not-execute"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(503, schema.StatusCode);
            Assert.Contains("DESIGN_CONVERSION_IDEMPOTENCY_DURABLE_REPLAY_FAILED_CLOSED", schema.BodyJson, StringComparison.Ordinal);
            Assert.Equal(0, schemaExecuteCount);

            await PersistDurableRecordAsync(
                oversizedStoreRoot,
                "durable-oversized-key",
                """{"requestId":"durable-oversized","value":1}""",
                """{"jobId":"thin-job-durable-oversized","requestId":"durable-oversized"}""");
            var oversizedPath = Assert.Single(Directory.EnumerateFiles(oversizedStoreRoot, "*.json", SearchOption.AllDirectories));
            await File.AppendAllTextAsync(oversizedPath, new string('x', 2 * 1024 * 1024), Encoding.UTF8);

            var oversizedAdmission = CreateAdmission(durableIdempotencyStoreRoot: oversizedStoreRoot);
            var oversizedDecision = oversizedAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-oversized-key")));
            var oversizedExecuteCount = 0;
            var oversized = await oversizedAdmission.ExecuteEvaluateOnceAsync(
                oversizedDecision,
                """{"requestId":"durable-oversized","value":1}""",
                _ =>
                {
                    Interlocked.Increment(ref oversizedExecuteCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-should-not-execute"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(503, oversized.StatusCode);
            Assert.Contains("DESIGN_CONVERSION_IDEMPOTENCY_DURABLE_REPLAY_FAILED_CLOSED", oversized.BodyJson, StringComparison.Ordinal);
            Assert.Equal(0, oversizedExecuteCount);
        }
        finally
        {
            DeleteTempDirectory(tamperedStoreRoot);
            DeleteTempDirectory(schemaStoreRoot);
            DeleteTempDirectory(oversizedStoreRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_ExactMaxRecordBytesReplaysAtBoundaryWithoutExecuting()
    {
        var storeRoot = CreateTempDirectory();
        const string idempotencyKey = "durable-exact-record-boundary-key";
        const string bodyJson = """{"requestId":"durable-exact-record-boundary","value":1}""";
        try
        {
            await PersistDurableRecordAsync(
                storeRoot,
                idempotencyKey,
                bodyJson,
                """{"jobId":"thin-job-durable-exact-record-boundary","requestId":"durable-exact-record-boundary"}""");
            var recordPath = Assert.Single(Directory.EnumerateFiles(storeRoot, "*.json", SearchOption.AllDirectories));
            await PadDurableRecordToExactByteLengthAsync(recordPath, DurableReplayRecordMaxBytes);
            Assert.Equal(DurableReplayRecordMaxBytes, new FileInfo(recordPath).Length);

            var admission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: idempotencyKey)));
            var executeCount = 0;
            var replayed = await admission.ExecuteEvaluateOnceAsync(
                decision,
                bodyJson,
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-should-not-execute"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(200, replayed.StatusCode);
            Assert.Contains("thin-job-durable-exact-record-boundary", replayed.BodyJson, StringComparison.Ordinal);
            Assert.Equal(0, executeCount);
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_InvalidUtf8RecordAtMaxBytesFailsClosedAsCorruptWithoutExecuting()
    {
        var storeRoot = CreateTempDirectory();
        const string idempotencyKey = "durable-invalid-utf8-boundary-key";
        const string bodyJson = """{"requestId":"durable-invalid-utf8-boundary","value":1}""";
        try
        {
            var admission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: idempotencyKey)));
            Assert.True(decision.IsAccepted);

            var recordPath = DurableRecordPath(storeRoot, decision.Scope, idempotencyKey);
            Directory.CreateDirectory(Path.GetDirectoryName(recordPath)!);
            await File.WriteAllBytesAsync(recordPath, Enumerable.Repeat((byte)0xFF, DurableReplayRecordMaxBytes).ToArray());

            var replayAdmission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var replayDecision = replayAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: idempotencyKey)));
            var executeCount = 0;
            var response = await replayAdmission.ExecuteEvaluateOnceAsync(
                replayDecision,
                bodyJson,
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(
                        200,
                        """{"jobId":"thin-job-should-not-execute","requestId":"durable-invalid-utf8-boundary"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            AssertSanitizedDurableFailure(
                response,
                "durable_idempotency_record_corrupt",
                storeRoot,
                idempotencyKey,
                bodyJson,
                CurrentToken,
                PreviousToken,
                TenantA);
            Assert.Equal(0, executeCount);
            Assert.False(File.Exists(recordPath));
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public void DurableReplay_BoundedSnapshotReaderRejectsPostOpenExtraBytesWithOneByteSentinel()
    {
        var declaredBytes = Encoding.UTF8.GetBytes("""{"snapshot":"declared"}""");
        using var stream = new DeceptiveGrowingSnapshotStream(declaredBytes);

        var snapshot = DesignConversionModelServerRouteAdmissionDurableStore.TryReadBoundedStrictUtf8Snapshot(
            stream,
            declaredBytes.Length);

        Assert.False(snapshot.IsSuccess);
        Assert.Equal("durable_idempotency_record_oversized", snapshot.ErrorRef);
        Assert.Equal(declaredBytes.Length + 1, stream.TotalBytesReturned);
        Assert.Equal(1, stream.ExtraBytesReturned);
        Assert.True(stream.MaxReadRequestBytes <= declaredBytes.Length);
    }

    [Fact]
    public async Task DurableReplay_PersistFailureFailsClosedWithoutStatusBindingOrMemoryReplay()
    {
        var storeRoot = CreateTempDirectory();
        try
        {
            var admission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-persist-failure-key")));
            Assert.True(decision.IsAccepted);
            var executeCount = 0;
            var oversizedResponseBody =
                """{"jobId":"thin-job-durable-persist-failure","requestId":"durable-persist-failure","pad":""" +
                "\"" +
                new string('x', 1024 * 1024) +
                "\"}";

            var failed = await admission.ExecuteEvaluateOnceAsync(
                decision,
                """{"requestId":"durable-persist-failure","value":1}""",
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(200, oversizedResponseBody));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(503, failed.StatusCode);
            Assert.Contains("DESIGN_CONVERSION_IDEMPOTENCY_DURABLE_REPLAY_FAILED_CLOSED", failed.BodyJson, StringComparison.Ordinal);
            Assert.Equal(1, executeCount);
            var deniedStatus = admission.AdmitStatus(StatusRequest("thin-job-durable-persist-failure", Header(Token: CurrentToken)));
            Assert.False(admission.IsStatusJobAllowed(deniedStatus, "thin-job-durable-persist-failure"));

            var retry = await admission.ExecuteEvaluateOnceAsync(
                decision,
                """{"requestId":"durable-persist-failure","value":1}""",
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(
                        200,
                        """{"jobId":"thin-job-durable-persist-retry","requestId":"durable-persist-failure"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(200, retry.StatusCode);
            Assert.Equal(2, executeCount);
            var allowedStatus = admission.AdmitStatus(StatusRequest("thin-job-durable-persist-retry", Header(Token: CurrentToken)));
            Assert.True(admission.IsStatusJobAllowed(allowedStatus, "thin-job-durable-persist-retry"));
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_PublishesDurableReservationFailureOnlyAfterWorkloadCleanupAndEviction()
    {
        const string idempotencyKey = "ordered-durable-reservation-failure";
        const string privateRoutePath = "/api/design-conversion/PRIVATE_DURABLE_RESERVATION_FAILURE/C:\\private\\durable-reservation?secret=PRIVATE_DURABLE_RESERVATION_ROUTE";
        const string privateBodyJson =
            """{"requestId":"ordered-durable-reservation-failure","private":"PRIVATE_DURABLE_RESERVATION_BODY","tree":"PRIVATE_DURABLE_RESERVATION_TREE"}""";
        var storeRoot = CreateTempDirectory();
        var now = FixedNow;
        var ttl = TimeSpan.FromMinutes(30);
        try
        {
            var admission = CreateAdmission(
                idempotencyTtl: ttl,
                utcNow: () => now,
                durableIdempotencyStoreRoot: storeRoot,
                maxConcurrentWorkloads: 1);
            var decision = admission.AdmitEvaluate(
                EvaluateRequest(Header(Token: CurrentToken, Idempotency: idempotencyKey), privateRoutePath));
            Assert.True(decision.IsAccepted);

            var lockPath = DurableLockPath(storeRoot, decision.Scope, idempotencyKey);
            await WriteAbandonedDurableLockAsync(
                storeRoot,
                decision.Scope,
                idempotencyKey,
                privateBodyJson,
                now.AddMinutes(-1),
                ttl);
            using var heldLock = new FileStream(lockPath, FileMode.Open, FileAccess.Read, FileShare.None);
            var executeCount = 0;
            var joinedExecuteCount = 0;

            var first = admission.ExecuteEvaluateOnceAsync(
                decision,
                privateBodyJson,
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"must-not-run-reservation-failure"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);
            var concurrentWaiter = admission.ExecuteEvaluateOnceAsync(
                decision,
                privateBodyJson,
                _ =>
                {
                    Interlocked.Increment(ref joinedExecuteCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"must-not-run-reservation-waiter"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);
            Assert.Equal(1, PrivateActiveWorkloadCount(admission));
            Assert.NotNull(PrivateIdempotencyResponseTask(admission, decision.Scope, idempotencyKey));

            var lifecycleLock = PrivateLifecycleLock(admission);
            var cleanupOrCompletionObserved = false;
            var failurePublishedBeforeEviction = false;
            var concurrentFailurePublishedBeforeEviction = false;
            var activeWorkloadsBeforeEviction = -1;
            var entryPresentBeforeEviction = false;
            var entryCompletedBeforeEviction = false;
            Monitor.Enter(lifecycleLock);
            try
            {
                heldLock.Dispose();
                cleanupOrCompletionObserved = SpinWait.SpinUntil(
                    () => first.IsCompleted ||
                          PrivateActiveWorkloadCount(admission) == 0,
                    TimeSpan.FromSeconds(5));
                failurePublishedBeforeEviction = first.IsCompleted;
                concurrentFailurePublishedBeforeEviction = concurrentWaiter.IsCompleted;
                activeWorkloadsBeforeEviction = PrivateActiveWorkloadCount(admission);
                var blockedEntry = PrivateIdempotencyResponseTask(admission, decision.Scope, idempotencyKey);
                entryPresentBeforeEviction = blockedEntry is not null;
                entryCompletedBeforeEviction = blockedEntry?.IsCompleted == true;
            }
            finally
            {
                Monitor.Exit(lifecycleLock);
            }

            var failures = await Task.WhenAll(first, concurrentWaiter).WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(cleanupOrCompletionObserved);
            Assert.False(failurePublishedBeforeEviction);
            Assert.False(concurrentFailurePublishedBeforeEviction);
            Assert.Equal(0, activeWorkloadsBeforeEviction);
            Assert.True(entryPresentBeforeEviction);
            Assert.False(entryCompletedBeforeEviction);
            Assert.Equal(0, executeCount);
            Assert.Equal(0, joinedExecuteCount);
            Assert.Equal(failures[0].BodyJson, failures[1].BodyJson);
            Assert.All(
                failures,
                failure => AssertSanitizedDurableFailure(
                    failure,
                    "durable_idempotency_lock_in_doubt",
                    idempotencyKey,
                    privateRoutePath,
                    privateBodyJson,
                    CurrentToken,
                    PreviousToken,
                    TenantA,
                    storeRoot,
                    Sha256Hex(CurrentToken),
                    Sha256Hex(PreviousToken)));
            Assert.Null(PrivateIdempotencyResponseTask(admission, decision.Scope, idempotencyKey));
            Assert.Equal(0, PrivateActiveWorkloadCount(admission));

            File.Delete(lockPath);
            var retry = await admission.ExecuteEvaluateOnceAsync(
                decision,
                privateBodyJson,
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(
                        200,
                        """{"jobId":"thin-job-durable-reservation-retry","requestId":"ordered-durable-reservation-failure"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(200, retry.StatusCode);
            Assert.Equal(1, executeCount);
            Assert.Equal(0, joinedExecuteCount);
            Assert.Equal(0, PrivateActiveWorkloadCount(admission));
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_PublishesDurablePersistFailureOnlyAfterLeaseCleanupAndEviction()
    {
        const string idempotencyKey = "ordered-durable-persist-failure";
        const string privateRoutePath = "/api/design-conversion/PRIVATE_DURABLE_PERSIST_FAILURE/C:\\private\\durable-persist?secret=PRIVATE_DURABLE_PERSIST_ROUTE";
        const string privateBodyJson =
            """{"requestId":"ordered-durable-persist-failure","private":"PRIVATE_DURABLE_PERSIST_BODY","tree":"PRIVATE_DURABLE_PERSIST_TREE"}""";
        const string privateResponseMarker = "PRIVATE_DURABLE_PERSIST_RESPONSE_PAD";
        var oversizedResponseBody =
            $$"""{"jobId":"thin-job-durable-persist-failure","private":"{{privateResponseMarker}}","pad":""" +
            "\"" +
            new string('x', 1024 * 1024) +
            "\"}";
        var storeRoot = CreateTempDirectory();
        try
        {
            var admission = CreateAdmission(
                durableIdempotencyStoreRoot: storeRoot,
                maxConcurrentWorkloads: 1);
            var decision = admission.AdmitEvaluate(
                EvaluateRequest(Header(Token: CurrentToken, Idempotency: idempotencyKey), privateRoutePath));
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseOversizedResponse = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var executeCount = 0;
            var joinedExecuteCount = 0;
            Assert.True(decision.IsAccepted);

            async Task<DesignConversionModelServerHttpResponse> ReturnOversizedAfterRelease(CancellationToken ct)
            {
                Assert.False(ct.CanBeCanceled);
                Interlocked.Increment(ref executeCount);
                started.TrySetResult(true);
                await releaseOversizedResponse.Task.WaitAsync(TimeSpan.FromSeconds(5));
                return JsonResponse(200, oversizedResponseBody);
            }

            var first = admission.ExecuteEvaluateOnceAsync(
                decision,
                privateBodyJson,
                ReturnOversizedAfterRelease,
                Conflict,
                InvalidJson,
                CancellationToken.None);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));

            var concurrentWaiter = admission.ExecuteEvaluateOnceAsync(
                decision,
                privateBodyJson,
                _ =>
                {
                    Interlocked.Increment(ref joinedExecuteCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"must-not-run-persist-waiter"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);
            var durableLockPath = DurableLockPath(storeRoot, decision.Scope, idempotencyKey);
            Assert.True(File.Exists(durableLockPath));
            Assert.Equal(1, PrivateActiveWorkloadCount(admission));

            var lifecycleLock = PrivateLifecycleLock(admission);
            var cleanupOrCompletionObserved = false;
            var failurePublishedBeforeEviction = false;
            var concurrentFailurePublishedBeforeEviction = false;
            var activeWorkloadsBeforeEviction = -1;
            var durableLockPresentBeforeEviction = false;
            var entryPresentBeforeEviction = false;
            var entryCompletedBeforeEviction = false;
            Monitor.Enter(lifecycleLock);
            try
            {
                releaseOversizedResponse.TrySetResult(true);
                cleanupOrCompletionObserved = SpinWait.SpinUntil(
                    () => first.IsCompleted ||
                          (PrivateActiveWorkloadCount(admission) == 0 && !File.Exists(durableLockPath)),
                    TimeSpan.FromSeconds(5));
                failurePublishedBeforeEviction = first.IsCompleted;
                concurrentFailurePublishedBeforeEviction = concurrentWaiter.IsCompleted;
                activeWorkloadsBeforeEviction = PrivateActiveWorkloadCount(admission);
                durableLockPresentBeforeEviction = File.Exists(durableLockPath);
                var blockedEntry = PrivateIdempotencyResponseTask(admission, decision.Scope, idempotencyKey);
                entryPresentBeforeEviction = blockedEntry is not null;
                entryCompletedBeforeEviction = blockedEntry?.IsCompleted == true;
            }
            finally
            {
                Monitor.Exit(lifecycleLock);
            }

            var failures = await Task.WhenAll(first, concurrentWaiter).WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(cleanupOrCompletionObserved);
            Assert.False(failurePublishedBeforeEviction);
            Assert.False(concurrentFailurePublishedBeforeEviction);
            Assert.Equal(0, activeWorkloadsBeforeEviction);
            Assert.False(durableLockPresentBeforeEviction);
            Assert.True(entryPresentBeforeEviction);
            Assert.False(entryCompletedBeforeEviction);
            Assert.Equal(failures[0].BodyJson, failures[1].BodyJson);
            Assert.All(
                failures,
                failure => AssertSanitizedDurableFailure(
                    failure,
                    "durable_idempotency_response_body_oversized",
                    idempotencyKey,
                    privateRoutePath,
                    privateBodyJson,
                    privateResponseMarker,
                    CurrentToken,
                    PreviousToken,
                    TenantA,
                    storeRoot,
                    Sha256Hex(CurrentToken),
                    Sha256Hex(PreviousToken)));
            Assert.Null(PrivateIdempotencyResponseTask(admission, decision.Scope, idempotencyKey));
            Assert.Equal(0, PrivateActiveWorkloadCount(admission));

            var retry = await admission.ExecuteEvaluateOnceAsync(
                decision,
                privateBodyJson,
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(
                        200,
                        """{"jobId":"thin-job-durable-persist-retry","requestId":"ordered-durable-persist-failure"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(200, retry.StatusCode);
            Assert.Equal(2, executeCount);
            Assert.Equal(0, joinedExecuteCount);
            Assert.Equal(0, PrivateActiveWorkloadCount(admission));
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_AbandonedCrashLockWithinTtlFailsClosedWithoutExecuting()
    {
        var storeRoot = CreateTempDirectory();
        var now = FixedNow;
        var ttl = TimeSpan.FromMinutes(30);
        try
        {
            var admission = CreateAdmission(
                idempotencyTtl: ttl,
                utcNow: () => now,
                durableIdempotencyStoreRoot: storeRoot);
            const string key = "durable-abandoned-within-ttl-key";
            const string privateRequestMarker = "PRIVATE_ABANDONED_LOCK_WITHIN_TTL_SECRET";
            const string bodyJson =
                """{"requestId":"durable-abandoned-within-ttl","private":"PRIVATE_ABANDONED_LOCK_WITHIN_TTL_SECRET","value":1}""";
            var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: key)));
            Assert.True(decision.IsAccepted);

            var lockPath = DurableLockPath(storeRoot, decision.Scope, key);
            await WriteAbandonedDurableLockAsync(
                storeRoot,
                decision.Scope,
                key,
                bodyJson,
                now.AddMinutes(-11),
                ttl);
            var lockText = await File.ReadAllTextAsync(lockPath, Encoding.UTF8);
            AssertNoLeak(
                lockText,
                key,
                privateRequestMarker,
                CurrentToken,
                PreviousToken,
                TenantA,
                storeRoot,
                Sha256Hex(CurrentToken),
                Sha256Hex(PreviousToken));

            var executeCount = 0;
            var response = await admission.ExecuteEvaluateOnceAsync(
                decision,
                bodyJson,
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(
                        200,
                        """{"jobId":"thin-job-should-not-execute","requestId":"durable-abandoned-within-ttl"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(503, response.StatusCode);
            Assert.Contains("DESIGN_CONVERSION_IDEMPOTENCY_DURABLE_REPLAY_FAILED_CLOSED", response.BodyJson, StringComparison.Ordinal);
            Assert.Equal(0, executeCount);
            Assert.True(File.Exists(lockPath));
            AssertNoLeak(
                response.BodyJson,
                key,
                privateRequestMarker,
                CurrentToken,
                PreviousToken,
                storeRoot,
                Sha256Hex(CurrentToken),
                Sha256Hex(PreviousToken));

            var differentDigestDecision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: key)));
            Assert.True(differentDigestDecision.IsAccepted);
            var differentDigestResponse = await admission.ExecuteEvaluateOnceAsync(
                differentDigestDecision,
                """{"requestId":"durable-abandoned-within-ttl","private":"PRIVATE_ABANDONED_LOCK_WITHIN_TTL_SECRET","value":2}""",
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(
                        200,
                        """{"jobId":"thin-job-should-not-execute-different-digest"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(409, differentDigestResponse.StatusCode);
            Assert.Equal(0, executeCount);
            AssertNoLeak(
                differentDigestResponse.BodyJson,
                key,
                privateRequestMarker,
                CurrentToken,
                PreviousToken,
                storeRoot,
                Sha256Hex(CurrentToken),
                Sha256Hex(PreviousToken));
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_AbandonedCrashLockUsesStoredExpiryWhenRestartTtlIsShorter()
    {
        var storeRoot = CreateTempDirectory();
        var now = FixedNow;
        var originalTtl = TimeSpan.FromMinutes(30);
        var restartedTtl = TimeSpan.FromSeconds(1);
        try
        {
            const string key = "durable-abandoned-shorter-restart-ttl-key";
            const string privateRequestMarker = "PRIVATE_ABANDONED_RESTART_SHORTER_TTL_SECRET";
            const string bodyJson =
                """{"requestId":"durable-abandoned-shorter-restart-ttl","private":"PRIVATE_ABANDONED_RESTART_SHORTER_TTL_SECRET","value":1}""";
            var originalAdmission = CreateAdmission(
                idempotencyTtl: originalTtl,
                utcNow: () => now,
                durableIdempotencyStoreRoot: storeRoot);
            var originalDecision = originalAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: key)));
            Assert.True(originalDecision.IsAccepted);

            var lockPath = DurableLockPath(storeRoot, originalDecision.Scope, key);
            await WriteAbandonedDurableLockAsync(
                storeRoot,
                originalDecision.Scope,
                key,
                bodyJson,
                now.AddMinutes(-11),
                originalTtl);

            var restartedAdmission = CreateAdmission(
                idempotencyTtl: restartedTtl,
                utcNow: () => now,
                durableIdempotencyStoreRoot: storeRoot);
            var restartedDecision = restartedAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: key)));
            Assert.True(restartedDecision.IsAccepted);

            var executeCount = 0;
            var response = await restartedAdmission.ExecuteEvaluateOnceAsync(
                restartedDecision,
                bodyJson,
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(
                        200,
                        """{"jobId":"thin-job-should-not-execute-shorter-restart-ttl"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(503, response.StatusCode);
            Assert.Contains("DESIGN_CONVERSION_IDEMPOTENCY_DURABLE_REPLAY_FAILED_CLOSED", response.BodyJson, StringComparison.Ordinal);
            Assert.Equal(0, executeCount);
            Assert.True(File.Exists(lockPath));
            AssertNoLeak(
                response.BodyJson,
                key,
                privateRequestMarker,
                CurrentToken,
                PreviousToken,
                storeRoot,
                Sha256Hex(CurrentToken),
                Sha256Hex(PreviousToken));
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_AbandonedCrashLockAfterTtlPrunesAndExecutesFresh()
    {
        var storeRoot = CreateTempDirectory();
        var now = FixedNow;
        var ttl = TimeSpan.FromMinutes(30);
        try
        {
            var admission = CreateAdmission(
                idempotencyTtl: ttl,
                utcNow: () => now,
                durableIdempotencyStoreRoot: storeRoot);
            const string key = "durable-abandoned-after-ttl-key";
            const string privateRequestMarker = "PRIVATE_ABANDONED_LOCK_AFTER_TTL_SECRET";
            const string bodyJson =
                """{"requestId":"durable-abandoned-after-ttl","private":"PRIVATE_ABANDONED_LOCK_AFTER_TTL_SECRET","value":1}""";
            var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: key)));
            Assert.True(decision.IsAccepted);

            var lockPath = DurableLockPath(storeRoot, decision.Scope, key);
            await WriteAbandonedDurableLockAsync(
                storeRoot,
                decision.Scope,
                key,
                bodyJson,
                now.Subtract(ttl).AddSeconds(-1),
                ttl);

            var executeCount = 0;
            var response = await admission.ExecuteEvaluateOnceAsync(
                decision,
                bodyJson,
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(
                        200,
                        """{"jobId":"thin-job-durable-abandoned-after-ttl","requestId":"durable-abandoned-after-ttl"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(1, executeCount);
            Assert.False(File.Exists(lockPath));
            Assert.Single(Directory.EnumerateFiles(Path.Combine(storeRoot, "records"), "*.json", SearchOption.TopDirectoryOnly));
            foreach (var path in Directory.EnumerateFiles(storeRoot, "*", SearchOption.AllDirectories))
            {
                Assert.DoesNotContain(key, path, StringComparison.Ordinal);
                var text = await File.ReadAllTextAsync(path, Encoding.UTF8);
                AssertNoLeak(
                    text,
                    key,
                    privateRequestMarker,
                    CurrentToken,
                    PreviousToken,
                    TenantA,
                    storeRoot,
                    Sha256Hex(CurrentToken),
                    Sha256Hex(PreviousToken));
            }
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_DiskRecordsAreBoundedPrunedAndDoNotContainSecrets()
    {
        var storeRoot = CreateTempDirectory();
        try
        {
            var admission = CreateAdmission(
                idempotencyCapacity: 2,
                durableIdempotencyStoreRoot: storeRoot);
            for (var index = 0; index < 3; index++)
            {
                var key = "durable-prune-key-" + index.ToString(CultureInfo.InvariantCulture);
                var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: key)));
                Assert.True(decision.IsAccepted);
                var response = await admission.ExecuteEvaluateOnceAsync(
                    decision,
                    $$"""{"requestId":"durable-prune-{{index}}","private":"PRIVATE_DURABLE_REQUEST_SECRET","value":{{index}}}""",
                    _ => Task.FromResult(JsonResponse(
                        200,
                        $$"""{"jobId":"thin-job-durable-prune-{{index}}","requestId":"durable-prune-public-{{index}}"}""")),
                    Conflict,
                    InvalidJson,
                    CancellationToken.None);
                Assert.Equal(200, response.StatusCode);
                Assert.DoesNotContain(storeRoot, response.BodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("design-conversion-route-admission-request-digest", response.BodyJson, StringComparison.Ordinal);
            }

            var recordFiles = Directory.EnumerateFiles(storeRoot, "*.json", SearchOption.AllDirectories).ToArray();
            Assert.True(recordFiles.Length <= 2);
            foreach (var path in Directory.EnumerateFiles(storeRoot, "*", SearchOption.AllDirectories))
            {
                Assert.DoesNotContain("durable-prune-key-", path, StringComparison.Ordinal);
                var text = await File.ReadAllTextAsync(path);
                AssertNoLeak(
                    text,
                    "durable-prune-key-0",
                    "durable-prune-key-1",
                    "durable-prune-key-2",
                    "PRIVATE_DURABLE_REQUEST_SECRET",
                    CurrentToken,
                    PreviousToken,
                    TenantA,
                    storeRoot,
                    Sha256Hex(CurrentToken),
                    Sha256Hex(PreviousToken));
            }
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task DurableReplay_ConcurrentAdmissionInstancesExecuteOnlyOnceWhenFirstCompletes()
    {
        var storeRoot = CreateTempDirectory();
        try
        {
            var firstAdmission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var secondAdmission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
            var firstDecision = firstAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-concurrent-key")));
            var secondDecision = secondAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "durable-concurrent-key")));
            Assert.True(firstDecision.IsAccepted);
            Assert.True(secondDecision.IsAccepted);
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var executeCount = 0;

            async Task<DesignConversionModelServerHttpResponse> Execute(CancellationToken ct)
            {
                Assert.False(ct.CanBeCanceled);
                Interlocked.Increment(ref executeCount);
                started.TrySetResult(true);
                await release.Task.WaitAsync(TimeSpan.FromSeconds(5));
                return JsonResponse(200, """{"jobId":"thin-job-durable-concurrent","requestId":"durable-concurrent"}""");
            }

            var first = firstAdmission.ExecuteEvaluateOnceAsync(
                firstDecision,
                """{"requestId":"durable-concurrent","value":1}""",
                Execute,
                Conflict,
                InvalidJson,
                CancellationToken.None);
            var second = secondAdmission.ExecuteEvaluateOnceAsync(
                secondDecision,
                """{"value":1,"requestId":"durable-concurrent"}""",
                Execute,
                Conflict,
                InvalidJson,
                CancellationToken.None);

            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            release.TrySetResult(true);
            var responses = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(10));

            Assert.All(responses, response => Assert.Equal(200, response.StatusCode));
            Assert.Equal(responses[0].BodyJson, responses[1].BodyJson);
            Assert.Equal(1, executeCount);
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task RestoredStatus_OmitsArtifactsWithMissingOrMalformedShaWithoutFabricatingProof()
    {
        var dbRoot = CreateTempDirectory();
        try
        {
            const string jobId = "thin-job-restored-malformed-sha";
            const string requestId = "restored-malformed-sha";
            var validRenderSha = "sha256:" + new string('a', 64);
            var record = new DesignConversionLocalArtifactDbRecord(
                RecordId: "record-restored-malformed-sha",
                DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
                JobId: jobId,
                RequestId: requestId,
                SourceKind: nameof(DesignConversionSourceKind.Figma),
                Tier: "route-admission-test",
                SanitizedSourceRefs: new Dictionary<string, string>
                {
                    ["design_tree_payload_ref"] = "sha256:" + new string('b', 64),
                    ["original_tree_payload_ref"] = "sha256:" + new string('c', 64)
                },
                ConversionResult: new DesignConversionConversionResultArtifact(
                    ResultId: "conversion-restored-malformed-sha",
                    HtmlRef: "artifact://restored/html",
                    HtmlSha256: string.Empty,
                    CssRef: "artifact://restored/css",
                    CssSha256: "not-a-sha",
                    Metadata: new Dictionary<string, string>()),
                RenderArtifactRefs:
                [
                    new DesignConversionRenderArtifactDbRef(
                        ViewportId: "desktop",
                        ArtifactId: "restored-render-missing-sha",
                        Kind: "screenshot",
                        UriRef: "artifact://restored/missing-sha.png",
                        Sha256: string.Empty,
                        ByteLength: 128,
                        Width: 100,
                        Height: 100,
                        Metadata: new Dictionary<string, string>()),
                    new DesignConversionRenderArtifactDbRef(
                        ViewportId: "mobile",
                        ArtifactId: "restored-render-valid",
                        Kind: "screenshot",
                        UriRef: "artifact://restored/valid.png",
                        Sha256: validRenderSha,
                        ByteLength: 128,
                        Width: 100,
                        Height: 100,
                        Metadata: new Dictionary<string, string>())
                ],
                VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                    Status: "not_evaluated",
                    ProviderBoundary: "not_evaluated",
                    ExternalCallExecuted: false,
                    EstimatedCostUsd: null,
                    TierDecision: "not_evaluated",
                    EvidenceRefs: [],
                    Metadata: new Dictionary<string, string>()),
                DpoCandidates: [],
                CreatedAtUtc: FixedNow,
                EvidenceRefs: ["restored_status_malformed_sha_regression"],
                Metadata: new Dictionary<string, string>());
            var recordsFile = Path.Combine(dbRoot, "conversion-results.jsonl");
            var compactJsonOptions = new JsonSerializerOptions(DesignConversionCheckpointJsonOptions.Standard)
            {
                WriteIndented = false
            };
            await File.WriteAllTextAsync(
                recordsFile,
                JsonSerializer.Serialize(record, compactJsonOptions) + Environment.NewLine,
                Encoding.UTF8);

            var db = new JsonlDesignConversionLocalArtifactDb(dbRoot);
            var readback = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: jobId));
            Assert.True(readback.IsSuccess, readback.ErrorMessage);
            Assert.Single(readback.Value!);
            var api = new DesignConversionModelServerApi(
                new ThrowingEvaluator(),
                () => FixedNow,
                db);
            var status = await api.GetJobResultAsync(jobId);

            Assert.True(status.IsSuccess, status.ErrorMessage);
            var artifact = Assert.Single(status.Value!.RuntimeStatus.Artifacts);
            Assert.Equal("restored-render-valid", artifact.ArtifactId);
            Assert.Equal(validRenderSha, artifact.Sha256);
            var publicStatusJson = JsonSerializer.Serialize(
                status.Value,
                DesignConversionModelServerHttpHandler.JsonOptions);
            Assert.DoesNotContain("sha256:" + new string('0', 64), publicStatusJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("not-a-sha", publicStatusJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("restored-render-missing-sha", publicStatusJson, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteTempDirectory(dbRoot);
        }
    }

    [Fact]
    public async Task TenantBoundAdmissionScopes_DoNotCollideForIdempotencyOrStatus()
    {
        var admissionA = CreateAdmission(tenantId: TenantA);
        var admissionB = CreateAdmission(tenantId: TenantB);
        var decisionA = admissionA.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "same-key")));
        var decisionB = admissionB.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "same-key")));
        Assert.True(decisionA.IsAccepted);
        Assert.True(decisionB.IsAccepted);
        Assert.NotEqual(decisionA.Scope, decisionB.Scope);
        Assert.Contains(DesignConversionModelActivationRegistry.BuildTenantIdRef(TenantA), decisionA.Scope, StringComparison.Ordinal);
        Assert.Contains(DesignConversionModelActivationRegistry.BuildTenantIdRef(TenantB), decisionB.Scope, StringComparison.Ordinal);

        var responseA = await admissionA.ExecuteEvaluateOnceAsync(
            decisionA,
            """{"requestId":"same-key","tenant":"a"}""",
            _ => Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-tenant-a","requestId":"same-key"}""")),
            Conflict,
            InvalidJson,
            CancellationToken.None);
        var responseB = await admissionB.ExecuteEvaluateOnceAsync(
            decisionB,
            """{"requestId":"same-key","tenant":"b"}""",
            _ => Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-tenant-b","requestId":"same-key"}""")),
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(200, responseA.StatusCode);
        Assert.Equal(200, responseB.StatusCode);

        var statusA = admissionA.AdmitStatus(StatusRequest("thin-job-tenant-a", Header(Token: CurrentToken)));
        var statusB = admissionB.AdmitStatus(StatusRequest("thin-job-tenant-b", Header(Token: PreviousToken)));
        Assert.True(admissionA.IsStatusJobAllowed(statusA, "thin-job-tenant-a"));
        Assert.True(admissionB.IsStatusJobAllowed(statusB, "thin-job-tenant-b"));
        Assert.False(admissionA.IsStatusJobAllowed(statusA, "thin-job-tenant-b"));
        Assert.False(admissionB.IsStatusJobAllowed(statusB, "thin-job-tenant-a"));
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_ReplaysFirstFailureForSameKeyAndBody()
    {
        var admission = CreateAdmission();
        var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "failure-key")));
        Assert.True(decision.IsAccepted);
        var executeCount = 0;

        Task<DesignConversionModelServerHttpResponse> Execute(CancellationToken _)
        {
            Interlocked.Increment(ref executeCount);
            return Task.FromResult(JsonResponse(
                500,
                """{"errorCode":"SCRIPTED_FIRST_FAILURE","errorRef":"refs-only-failure"}"""));
        }

        var first = await admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"requestId":"failure-key"}""",
            Execute,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        var second = await admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"requestId":"failure-key"}""",
            Execute,
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(500, first.StatusCode);
        Assert.Equal(first.BodyJson, second.BodyJson);
        Assert.Equal(1, executeCount);
        Assert.Equal(
            DesignConversionModelServerRouteAdmission.IdempotencyFailureReplayPolicy,
            decision.BoundaryMetadata["idempotency_failure_replay_policy"]);
        Assert.Equal(
            DesignConversionModelServerRouteAdmission.IdempotencyFailureReplayPolicy,
            admission.PublicMetadata()["idempotencyFailureReplayPolicy"]);
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_BoundsUniqueInFlightCapacityAndRejectsBeforeSideEffects()
    {
        var admission = CreateAdmission(idempotencyCapacity: 2);
        const string privateOverflowRoutePath =
            "/api/design-conversion/evaluate/PRIVATE_IDEMPOTENCY_CAPACITY_ROUTE/C:\\private\\tenant?secret=PRIVATE_IDEMPOTENCY_CAPACITY_QUERY";
        const string privateOverflowKey = "PRIVATE_IDEMPOTENCY_CAPACITY_KEY";
        const string privateOverflowBodyMarker = "PRIVATE_IDEMPOTENCY_CAPACITY_BODY";
        var releases = new Dictionary<string, TaskCompletionSource<bool>>(StringComparer.Ordinal);
        var started = new Dictionary<string, TaskCompletionSource<bool>>(StringComparer.Ordinal);
        var executeCount = 0;
        var activeCount = 0;
        var maxActiveCount = 0;
        var coordinationTimeout = TimeSpan.FromSeconds(30);

        Task<DesignConversionModelServerHttpResponse> Start(
            string key,
            string? path = null,
            string? privateBodyMarker = null)
        {
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            releases[key] = release;
            started[key] = start;
            var decision = admission.AdmitEvaluate(
                EvaluateRequest(Header(Token: CurrentToken, Idempotency: key), path));
            Assert.True(decision.IsAccepted);

            return admission.ExecuteEvaluateOnceAsync(
                decision,
                $$"""{"requestId":"{{key}}","secret":"{{privateBodyMarker ?? "PRIVATE-" + key}}"}""",
                async ct =>
                {
                    Assert.False(ct.CanBeCanceled);
                    Interlocked.Increment(ref executeCount);
                    var active = Interlocked.Increment(ref activeCount);
                    RecordMax(ref maxActiveCount, active);
                    start.TrySetResult(true);
                    await release.Task.WaitAsync(coordinationTimeout);
                    Interlocked.Decrement(ref activeCount);
                    return JsonResponse(200, $$"""{"jobId":"thin-job-{{key}}","requestId":"{{key}}"}""");
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);
        }

        var first = Start("hold-1");
        var second = Start("hold-2");
        await Task.WhenAll(
            started["hold-1"].Task.WaitAsync(coordinationTimeout),
            started["hold-2"].Task.WaitAsync(coordinationTimeout));

        var overflowTasks = Enumerable.Range(0, 8)
            .Select(index => index == 0
                ? Start(privateOverflowKey, privateOverflowRoutePath, privateOverflowBodyMarker)
                : Start("overflow-" + index))
            .ToArray();
        var overflowResponses = await Task.WhenAll(overflowTasks).WaitAsync(coordinationTimeout);

        Assert.All(overflowResponses, response => Assert.Equal(429, response.StatusCode));
        Assert.Equal(2, executeCount);
        Assert.True(maxActiveCount <= 2);
        Assert.True(PrivateDictionaryCount(admission, "_idempotencyEntries") <= 2);
        AssertIdempotencyCapacityRejected(
            overflowResponses[0],
            privateOverflowRoutePath,
            privateOverflowKey,
            privateOverflowBodyMarker,
            CurrentToken,
            PreviousToken,
            TenantA);

        releases["hold-1"].TrySetResult(true);
        var firstResponse = await first.WaitAsync(coordinationTimeout);
        Assert.Equal(200, firstResponse.StatusCode);

        var third = Start("hold-3");
        await started["hold-3"].Task.WaitAsync(coordinationTimeout);
        Assert.Equal(3, executeCount);
        Assert.True(PrivateDictionaryCount(admission, "_idempotencyEntries") <= 2);

        releases["hold-2"].TrySetResult(true);
        releases["hold-3"].TrySetResult(true);
        var completed = await Task.WhenAll(second, third).WaitAsync(coordinationTimeout);
        Assert.All(completed, response => Assert.Equal(200, response.StatusCode));
    }

    [Fact]
    public async Task DisabledAdmission_EnforcesMaxConcurrentWorkloadsWithoutSideEffectsAndAllowsRetry()
    {
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: false,
                maxConcurrentWorkloads: 1),
            () => FixedNow);
        Assert.False(admission.Enabled);
        Assert.Equal(1, admission.MaxConcurrentWorkloads);
        Assert.Equal(1, admission.PublicMetadata()["routeAdmissionMaxConcurrentWorkloads"]);

        var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstExecuteCount = 0;
        var overflowExecuteCount = 0;
        var firstDecision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Idempotency: "disabled-hold")));
        var overflowDecision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Idempotency: "disabled-overflow")));
        Assert.True(firstDecision.IsAccepted);
        Assert.True(overflowDecision.IsAccepted);
        Assert.Equal(bool.TrueString, overflowDecision.BoundaryMetadata["route_admission_authorized"]);

        async Task<DesignConversionModelServerHttpResponse> Hold(CancellationToken ct)
        {
            Interlocked.Increment(ref firstExecuteCount);
            firstStarted.TrySetResult(true);
            await releaseFirst.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
            return JsonResponse(200, """{"jobId":"thin-job-disabled-hold"}""");
        }

        var first = admission.ExecuteEvaluateOnceAsync(
            firstDecision,
            """{"requestId":"disabled-hold"}""",
            Hold,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var overflow = await admission.ExecuteEvaluateOnceAsync(
            overflowDecision,
            """{"requestId":"disabled-overflow","private":"PRIVATE_DISABLED_WORKLOAD_SECRET"}""",
            _ =>
            {
                Interlocked.Increment(ref overflowExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-disabled-overflow"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        AssertWorkloadRejected(
            overflow,
            overflowDecision,
            1,
            1,
            "disabled-overflow",
            "PRIVATE_DISABLED_WORKLOAD_SECRET",
            CurrentToken,
            PreviousToken);
        Assert.Equal(1, firstExecuteCount);
        Assert.Equal(0, overflowExecuteCount);

        releaseFirst.TrySetResult(true);
        var firstResponse = await first.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(200, firstResponse.StatusCode);

        var retry = await admission.ExecuteEvaluateOnceAsync(
            overflowDecision,
            """{"requestId":"disabled-overflow","private":"PRIVATE_DISABLED_WORKLOAD_SECRET"}""",
            _ =>
            {
                Interlocked.Increment(ref overflowExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-disabled-overflow"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(200, retry.StatusCode);
        Assert.Equal(1, overflowExecuteCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DisabledAdmission_HandlerFailureIsSanitizedAndReleasesWorkloadLease(bool cancelHandler)
    {
        var failureKind = cancelHandler ? "CANCEL" : "EXCEPTION";
        var privateRoutePath = "/api/design-conversion/PRIVATE_DISABLED_" + failureKind +
                               "/C:\\private\\tenant-route?secret=PRIVATE_DISABLED_ROUTE_QUERY";
        var privateBodyJson = $$"""{"requestId":"disabled-{{failureKind}}","private":"PRIVATE_DISABLED_{{failureKind}}_BODY","html":"<main>PRIVATE_DISABLED_{{failureKind}}_HTML</main>"}""";
        var privateMessage = "scripted disabled handler " + failureKind +
                             " PRIVATE_DISABLED_HANDLER_MESSAGE C:\\private\\disabled-handler\\payload.json";
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: false,
                maxConcurrentWorkloads: 1),
            () => FixedNow);
        var failureDecision = admission.AdmitEvaluate(
            MutationRequest("POST", privateRoutePath, Header(Idempotency: "disabled-handler-failure")));
        var laterDecision = admission.AdmitEvaluate(
            MutationRequest("POST", "/api/design-conversion/disabled-handler-later", Header(Idempotency: "disabled-handler-later")));
        var failureExecuteCount = 0;
        var laterExecuteCount = 0;

        Task<DesignConversionModelServerHttpResponse> Fail(CancellationToken _)
        {
            Interlocked.Increment(ref failureExecuteCount);
            if (!cancelHandler)
            {
                return Task.FromException<DesignConversionModelServerHttpResponse>(
                    new InvalidOperationException(privateMessage));
            }

            using var canceledSource = new CancellationTokenSource();
            canceledSource.Cancel();
            return Task.FromException<DesignConversionModelServerHttpResponse>(
                new OperationCanceledException(privateMessage, null, canceledSource.Token));
        }

        var failure = await admission.ExecuteEvaluateOnceAsync(
            failureDecision,
            privateBodyJson,
            Fail,
            Conflict,
            InvalidJson,
            CancellationToken.None);

        AssertSanitizedHandlerFailure(
            failure,
            cancelHandler ? 499 : 500,
            cancelHandler
                ? DesignConversionModelServerEndpointRegistry.EndpointCancelledErrorCode
                : DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode,
            cancelHandler ? "handler_cancellation_failed_closed" : "handler_exception_failed_closed",
            privateRoutePath,
            privateMessage,
            privateBodyJson,
            CurrentToken,
            PreviousToken,
            TenantA);
        Assert.Equal(0, PrivateActiveWorkloadCount(admission));

        var later = await admission.ExecuteEvaluateOnceAsync(
            laterDecision,
            """{"requestId":"disabled-handler-later"}""",
            _ =>
            {
                Interlocked.Increment(ref laterExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-disabled-handler-later"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(200, later.StatusCode);
        Assert.Equal(1, failureExecuteCount);
        Assert.Equal(1, laterExecuteCount);
        Assert.Equal(0, PrivateActiveWorkloadCount(admission));
    }

    [Fact]
    public async Task DisabledAdmission_TrueCallerCancellationPropagatesAndReleasesWorkloadLease()
    {
        const string privateRoutePath = "/api/design-conversion/PRIVATE_DISABLED_CALLER_CANCEL/C:\\private\\caller-cancel";
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: false,
                maxConcurrentWorkloads: 1),
            () => FixedNow);
        var canceledDecision = admission.AdmitEvaluate(
            MutationRequest("POST", privateRoutePath, Header(Idempotency: "disabled-caller-cancel")));
        var laterDecision = admission.AdmitEvaluate(
            MutationRequest("POST", "/api/design-conversion/disabled-caller-cancel-later", Header(Idempotency: "disabled-caller-cancel-later")));
        var canceledExecuteCount = 0;
        var laterExecuteCount = 0;

        using var callerCancellation = new CancellationTokenSource();
        await callerCancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => admission.ExecuteEvaluateOnceAsync(
            canceledDecision,
            """{"requestId":"disabled-caller-cancel","private":"PRIVATE_DISABLED_CALLER_CANCEL_BODY"}""",
            ct =>
            {
                Interlocked.Increment(ref canceledExecuteCount);
                return Task.FromCanceled<DesignConversionModelServerHttpResponse>(ct);
            },
            Conflict,
            InvalidJson,
            callerCancellation.Token));
        Assert.Equal(0, PrivateActiveWorkloadCount(admission));

        var later = await admission.ExecuteEvaluateOnceAsync(
            laterDecision,
            """{"requestId":"disabled-caller-cancel-later"}""",
            _ =>
            {
                Interlocked.Increment(ref laterExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-disabled-caller-cancel-later"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(200, later.StatusCode);
        Assert.Equal(1, canceledExecuteCount);
        Assert.Equal(1, laterExecuteCount);
        Assert.Equal(0, PrivateActiveWorkloadCount(admission));
    }

    [Fact]
    public async Task EnabledAdmission_WorkloadGateLetsSameKeyReplayJoinAndDoesNotCacheDifferentKeyOverload()
    {
        var admission = CreateAdmission(idempotencyCapacity: 16, maxConcurrentWorkloads: 1);
        const string privateOverflowRoutePath =
            "/api/design-conversion/evaluate/PRIVATE_WORKLOAD_CAPACITY_ROUTE/C:\\private\\tenant?secret=PRIVATE_WORKLOAD_CAPACITY_QUERY";
        const string privateOverflowKey = "PRIVATE_WORKLOAD_CAPACITY_KEY";
        const string privateOverflowBodyMarker = "PRIVATE_ENABLED_WORKLOAD_CAPACITY_BODY";
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var executeCount = 0;
        var overflowExecuteCount = 0;
        var firstDecision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "workload-same")));
        var replayDecision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "workload-same")));
        var overflowDecision = admission.AdmitEvaluate(
            EvaluateRequest(Header(Token: CurrentToken, Idempotency: privateOverflowKey), privateOverflowRoutePath));
        Assert.True(firstDecision.IsAccepted);
        Assert.True(replayDecision.IsAccepted);
        Assert.True(overflowDecision.IsAccepted);
        Assert.Equal(bool.TrueString, overflowDecision.BoundaryMetadata["route_admission_authorized"]);

        async Task<DesignConversionModelServerHttpResponse> Hold(CancellationToken ct)
        {
            Assert.False(ct.CanBeCanceled);
            Interlocked.Increment(ref executeCount);
            started.TrySetResult(true);
            await release.Task.WaitAsync(TimeSpan.FromSeconds(5));
            return JsonResponse(200, """{"jobId":"thin-job-workload-same","requestId":"workload-same"}""");
        }

        var first = admission.ExecuteEvaluateOnceAsync(
            firstDecision,
            """{"requestId":"workload-same","value":1}""",
            Hold,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var replay = admission.ExecuteEvaluateOnceAsync(
            replayDecision,
            """{"value":1,"requestId":"workload-same"}""",
            Hold,
            Conflict,
            InvalidJson,
            CancellationToken.None);

        var overflow = await admission.ExecuteEvaluateOnceAsync(
            overflowDecision,
            $$"""{"requestId":"workload-overflow","private":"{{privateOverflowBodyMarker}}"}""",
            _ =>
            {
                Interlocked.Increment(ref overflowExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-workload-overflow"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        AssertWorkloadRejected(
            overflow,
            overflowDecision,
            1,
            1,
            privateOverflowRoutePath,
            privateOverflowKey,
            privateOverflowBodyMarker,
            CurrentToken,
            PreviousToken,
            TenantA);
        Assert.Equal(1, executeCount);
        Assert.Equal(0, overflowExecuteCount);
        Assert.Null(PrivateIdempotencyResponseTask(admission, overflowDecision.Scope, privateOverflowKey));

        release.TrySetResult(true);
        var joined = await Task.WhenAll(first, replay).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.All(joined, response => Assert.Equal(200, response.StatusCode));
        Assert.Equal(joined[0].BodyJson, joined[1].BodyJson);
        Assert.Equal(1, executeCount);

        var retry = await admission.ExecuteEvaluateOnceAsync(
            overflowDecision,
            $$"""{"requestId":"workload-overflow","private":"{{privateOverflowBodyMarker}}"}""",
            _ =>
            {
                Interlocked.Increment(ref overflowExecuteCount);
                return Task.FromResult(JsonResponse(202, """{"jobId":"thin-job-workload-overflow","accepted":true}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(202, retry.StatusCode);
        Assert.Equal(1, overflowExecuteCount);
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_WorkloadLeaseReleasesAfterExceptionAndCancellation()
    {
        var faultAdmission = CreateAdmission(idempotencyCapacity: 16, maxConcurrentWorkloads: 1);
        var faultDecision = faultAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "workload-fault")));
        var afterFaultDecision = faultAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "workload-after-fault")));
        const string privateFaultBodyJson = """{"requestId":"workload-fault","private":"PRIVATE_WORKLOAD_FAULT_BODY","html":"<main>PRIVATE_WORKLOAD_FAULT_HTML</main>"}""";
        const string privateFaultMessage = "scripted workload fault PRIVATE_WORKLOAD_FAULT_EXCEPTION current-route-token-0123456789 C:\\private\\tenant-route-admission-alpha\\payload.json";
        var faultExecuteCount = 0;
        var afterFaultExecuteCount = 0;
        Assert.True(faultDecision.IsAccepted);
        Assert.True(afterFaultDecision.IsAccepted);

        var faultResponse = await faultAdmission.ExecuteEvaluateOnceAsync(
            faultDecision,
            privateFaultBodyJson,
            _ =>
            {
                Interlocked.Increment(ref faultExecuteCount);
                return Task.FromException<DesignConversionModelServerHttpResponse>(
                    new InvalidOperationException(privateFaultMessage));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);
        AssertSanitizedHandlerFailure(
            faultResponse,
            500,
            DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode,
            "handler_exception_failed_closed",
            privateFaultMessage,
            privateFaultBodyJson,
            CurrentToken,
            PreviousToken,
            TenantA);

        var afterFault = await faultAdmission.ExecuteEvaluateOnceAsync(
            afterFaultDecision,
            """{"requestId":"workload-after-fault"}""",
            _ =>
            {
                Interlocked.Increment(ref afterFaultExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-after-fault"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);
        Assert.Equal(200, afterFault.StatusCode);
        Assert.Equal(1, faultExecuteCount);
        Assert.Equal(1, afterFaultExecuteCount);

        var canceledAdmission = CreateAdmission(idempotencyCapacity: 16, maxConcurrentWorkloads: 1);
        var canceledDecision = canceledAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "workload-canceled")));
        var afterCanceledDecision = canceledAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "workload-after-canceled")));
        const string privateCanceledBodyJson = """{"requestId":"workload-canceled","private":"PRIVATE_WORKLOAD_CANCELED_BODY","css":".PRIVATE_WORKLOAD_CANCELED_CSS{}"}""";
        var canceledExecuteCount = 0;
        var afterCanceledExecuteCount = 0;
        Assert.True(canceledDecision.IsAccepted);
        Assert.True(afterCanceledDecision.IsAccepted);

        var canceledResponse = await canceledAdmission.ExecuteEvaluateOnceAsync(
            canceledDecision,
            privateCanceledBodyJson,
            _ =>
            {
                Interlocked.Increment(ref canceledExecuteCount);
                using var canceledSource = new CancellationTokenSource();
                canceledSource.Cancel();
                return Task.FromCanceled<DesignConversionModelServerHttpResponse>(canceledSource.Token);
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);
        AssertSanitizedHandlerFailure(
            canceledResponse,
            499,
            DesignConversionModelServerEndpointRegistry.EndpointCancelledErrorCode,
            "handler_cancellation_failed_closed",
            privateCanceledBodyJson,
            CurrentToken,
            PreviousToken,
            TenantA);

        var afterCanceled = await canceledAdmission.ExecuteEvaluateOnceAsync(
            afterCanceledDecision,
            """{"requestId":"workload-after-canceled"}""",
            _ =>
            {
                Interlocked.Increment(ref afterCanceledExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-after-canceled"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);
        Assert.Equal(200, afterCanceled.StatusCode);
        Assert.Equal(1, canceledExecuteCount);
        Assert.Equal(1, afterCanceledExecuteCount);
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_PublishesHandlerFailureOnlyAfterLeaseCleanupAndEviction()
    {
        const string idempotencyKey = "ordered-handler-failure";
        const string privateRoutePath = "/api/design-conversion/PRIVATE_ORDERED_FAILURE/C:\\private\\route?secret=PRIVATE_ROUTE_QUERY";
        const string privateBodyJson = """{"requestId":"ordered-handler-failure","private":"PRIVATE_ORDERED_FAILURE_BODY","tree":"PRIVATE_ORDERED_FAILURE_TREE"}""";
        const string privateMessage = "scripted ordered failure PRIVATE_ORDERED_FAILURE_MESSAGE C:\\private\\route-admission\\ordered.json";
        var storeRoot = CreateTempDirectory();
        try
        {
            var admission = CreateAdmission(
                durableIdempotencyStoreRoot: storeRoot,
                maxConcurrentWorkloads: 1);
            var decision = admission.AdmitEvaluate(
                MutationRequest("POST", privateRoutePath, Header(Token: CurrentToken, Idempotency: idempotencyKey)));
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFailure = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var executeCount = 0;
            var joinedExecuteCount = 0;
            Assert.True(decision.IsAccepted);

            async Task<DesignConversionModelServerHttpResponse> FailAfterRelease(CancellationToken ct)
            {
                Assert.False(ct.CanBeCanceled);
                Interlocked.Increment(ref executeCount);
                started.TrySetResult(true);
                await releaseFailure.Task.WaitAsync(TimeSpan.FromSeconds(5));
                throw new InvalidOperationException(privateMessage);
            }

            var first = admission.ExecuteEvaluateOnceAsync(
                decision,
                privateBodyJson,
                FailAfterRelease,
                Conflict,
                InvalidJson,
                CancellationToken.None);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));

            var concurrentWaiter = admission.ExecuteEvaluateOnceAsync(
                decision,
                privateBodyJson,
                _ =>
                {
                    Interlocked.Increment(ref joinedExecuteCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"must-not-run-concurrent-waiter"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);
            var durableLockPath = DurableLockPath(storeRoot, decision.Scope, idempotencyKey);
            Assert.True(File.Exists(durableLockPath));
            Assert.Equal(1, PrivateActiveWorkloadCount(admission));

            var lifecycleLock = PrivateLifecycleLock(admission);
            var cleanupOrCompletionObserved = false;
            var failurePublishedBeforeEviction = false;
            var concurrentFailurePublishedBeforeEviction = false;
            var activeWorkloadsBeforeEviction = -1;
            var durableLockPresentBeforeEviction = false;
            var entryPresentBeforeEviction = false;
            var entryCompletedBeforeEviction = false;
            Monitor.Enter(lifecycleLock);
            try
            {
                releaseFailure.TrySetResult(true);
                cleanupOrCompletionObserved = SpinWait.SpinUntil(
                    () => first.IsCompleted ||
                          (PrivateActiveWorkloadCount(admission) == 0 && !File.Exists(durableLockPath)),
                    TimeSpan.FromSeconds(5));
                failurePublishedBeforeEviction = first.IsCompleted;
                concurrentFailurePublishedBeforeEviction = concurrentWaiter.IsCompleted;
                activeWorkloadsBeforeEviction = PrivateActiveWorkloadCount(admission);
                durableLockPresentBeforeEviction = File.Exists(durableLockPath);
                var blockedEntry = PrivateIdempotencyResponseTask(admission, decision.Scope, idempotencyKey);
                entryPresentBeforeEviction = blockedEntry is not null;
                entryCompletedBeforeEviction = blockedEntry?.IsCompleted == true;
            }
            finally
            {
                Monitor.Exit(lifecycleLock);
            }

            var failures = await Task.WhenAll(first, concurrentWaiter).WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(cleanupOrCompletionObserved);
            Assert.False(failurePublishedBeforeEviction);
            Assert.False(concurrentFailurePublishedBeforeEviction);
            Assert.Equal(0, activeWorkloadsBeforeEviction);
            Assert.False(durableLockPresentBeforeEviction);
            Assert.True(entryPresentBeforeEviction);
            Assert.False(entryCompletedBeforeEviction);
            Assert.Equal(failures[0].BodyJson, failures[1].BodyJson);
            Assert.All(
                failures,
                failure => AssertSanitizedHandlerFailure(
                    failure,
                    500,
                    DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode,
                    "handler_exception_failed_closed",
                    privateRoutePath,
                    privateMessage,
                    privateBodyJson,
                    CurrentToken,
                    PreviousToken,
                    TenantA,
                    storeRoot));
            Assert.Null(PrivateIdempotencyResponseTask(admission, decision.Scope, idempotencyKey));
            Assert.Equal(0, PrivateActiveWorkloadCount(admission));

            var retry = await admission.ExecuteEvaluateOnceAsync(
                decision,
                privateBodyJson,
                _ =>
                {
                    Interlocked.Increment(ref executeCount);
                    return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-ordered-handler-retry"}"""));
                },
                Conflict,
                InvalidJson,
                CancellationToken.None);

            Assert.Equal(200, retry.StatusCode);
            Assert.Equal(2, executeCount);
            Assert.Equal(0, joinedExecuteCount);
            Assert.Equal(0, PrivateActiveWorkloadCount(admission));
        }
        finally
        {
            DeleteTempDirectory(storeRoot);
        }
    }

    [Fact]
    public async Task ExecuteEvaluateAndIdempotentMutationShareOneWorkloadGate()
    {
        var admission = CreateAdmission(idempotencyCapacity: 16, maxConcurrentWorkloads: 1);
        var evaluateStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseEvaluate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var evaluateExecuteCount = 0;
        var mutationExecuteCount = 0;
        var evaluateDecision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "shared-gate-evaluate")));
        var mutationDecision = admission.AdmitIdempotentMutation(
            MutationRequest("POST", "/api/design-conversion/future-png-upload", Header(Token: CurrentToken, Idempotency: "shared-gate-mutation")));
        Assert.True(evaluateDecision.IsAccepted);
        Assert.True(mutationDecision.IsAccepted);

        async Task<DesignConversionModelServerHttpResponse> HoldEvaluate(CancellationToken ct)
        {
            Assert.False(ct.CanBeCanceled);
            Interlocked.Increment(ref evaluateExecuteCount);
            evaluateStarted.TrySetResult(true);
            await releaseEvaluate.Task.WaitAsync(TimeSpan.FromSeconds(5));
            return JsonResponse(200, """{"jobId":"thin-job-shared-gate-evaluate"}""");
        }

        var evaluate = admission.ExecuteEvaluateOnceAsync(
            evaluateDecision,
            """{"requestId":"shared-gate-evaluate"}""",
            HoldEvaluate,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        await evaluateStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var mutationOverflow = await admission.ExecuteIdempotentMutationOnceAsync(
            mutationDecision,
            """{"requestId":"shared-gate-mutation","private":"PRIVATE_SHARED_GATE_SECRET"}""",
            _ =>
            {
                Interlocked.Increment(ref mutationExecuteCount);
                return Task.FromResult(JsonResponse(201, """{"jobId":"thin-job-shared-gate-mutation"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        AssertWorkloadRejected(
            mutationOverflow,
            mutationDecision,
            1,
            1,
            "shared-gate-mutation",
            "PRIVATE_SHARED_GATE_SECRET",
            CurrentToken,
            PreviousToken);
        Assert.Equal(1, evaluateExecuteCount);
        Assert.Equal(0, mutationExecuteCount);

        releaseEvaluate.TrySetResult(true);
        var evaluateResponse = await evaluate.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(200, evaluateResponse.StatusCode);

        var mutationRetry = await admission.ExecuteIdempotentMutationOnceAsync(
            mutationDecision,
            """{"requestId":"shared-gate-mutation","private":"PRIVATE_SHARED_GATE_SECRET"}""",
            _ =>
            {
                Interlocked.Increment(ref mutationExecuteCount);
                return Task.FromResult(JsonResponse(201, """{"jobId":"thin-job-shared-gate-mutation"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(201, mutationRetry.StatusCode);
        Assert.Equal(1, mutationExecuteCount);
    }

    [Fact]
    public async Task ExecuteEvaluateAndIdempotentMutationShareOneRequestQuotaAndRolloverWithoutLeaks()
    {
        var now = FixedNow;
        const string overflowKey = "PRIVATE_QUOTA_OVERFLOW_KEY";
        const string privateRoutePath =
            "/api/design-conversion/evaluate/PRIVATE_REQUEST_QUOTA_ROUTE/C:\\private\\tenant?secret=PRIVATE_REQUEST_QUOTA_QUERY";
        const string privateMarker = "PRIVATE_QUOTA_OVERFLOW_SECRET";
        var admission = CreateAdmission(
            utcNow: () => now,
            requestQuotaLimit: 2,
            requestQuotaWindow: TimeSpan.FromSeconds(10));
        var evaluateDecision = admission.AdmitEvaluate(
            EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "quota-evaluate")));
        var mutationDecision = admission.AdmitIdempotentMutation(
            MutationRequest("POST", "/api/design-conversion/future-png-upload", Header(Token: CurrentToken, Idempotency: "quota-mutation")));
        var overflowDecision = admission.AdmitEvaluate(
            EvaluateRequest(Header(Token: CurrentToken, Idempotency: overflowKey), privateRoutePath));
        Assert.True(evaluateDecision.IsAccepted);
        Assert.True(mutationDecision.IsAccepted);
        Assert.True(overflowDecision.IsAccepted);

        var evaluateExecuteCount = 0;
        var mutationExecuteCount = 0;
        var overflowExecuteCount = 0;
        var evaluate = await admission.ExecuteEvaluateOnceAsync(
            evaluateDecision,
            """{"requestId":"quota-evaluate"}""",
            _ =>
            {
                Interlocked.Increment(ref evaluateExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-quota-evaluate"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);
        var mutation = await admission.ExecuteIdempotentMutationOnceAsync(
            mutationDecision,
            """{"requestId":"quota-mutation"}""",
            _ =>
            {
                Interlocked.Increment(ref mutationExecuteCount);
                return Task.FromResult(JsonResponse(201, """{"jobId":"thin-job-quota-mutation"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        var overflow = await admission.ExecuteEvaluateOnceAsync(
            overflowDecision,
            $$"""{"requestId":"quota-overflow","private":"{{privateMarker}}"}""",
            _ =>
            {
                Interlocked.Increment(ref overflowExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-quota-overflow"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(200, evaluate.StatusCode);
        Assert.Equal(201, mutation.StatusCode);
        Assert.Equal(1, evaluateExecuteCount);
        Assert.Equal(1, mutationExecuteCount);
        Assert.Equal(0, overflowExecuteCount);
        AssertRequestQuotaRejected(
            overflow,
            expectedLimit: 2,
            expectedWindowSeconds: 10,
            expectedRetryAfterSeconds: 10,
            privateRoutePath,
            overflowKey,
            privateMarker,
            CurrentToken,
            PreviousToken,
            TenantA);
        Assert.Null(PrivateIdempotencyResponseTask(admission, overflowDecision.Scope, overflowKey));

        now = now.AddSeconds(11);
        var retry = await admission.ExecuteEvaluateOnceAsync(
            overflowDecision,
            $$"""{"requestId":"quota-overflow","private":"{{privateMarker}}"}""",
            _ =>
            {
                Interlocked.Increment(ref overflowExecuteCount);
                return Task.FromResult(JsonResponse(202, """{"jobId":"thin-job-quota-overflow","accepted":true}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(202, retry.StatusCode);
        Assert.Equal(1, overflowExecuteCount);
    }

    [Fact]
    public async Task DisabledAdmission_StillEnforcesRequestQuotaWithoutInvokingOverflowWork()
    {
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: false,
                requestQuotaLimit: 1,
                requestQuotaWindow: TimeSpan.FromSeconds(20)),
            () => FixedNow);
        Assert.False(admission.Enabled);
        Assert.True((bool)admission.PublicMetadata()["routeAdmissionRequestQuotaActive"]!);

        var firstDecision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Idempotency: "disabled-quota-first")));
        var overflowDecision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Idempotency: "disabled-quota-overflow")));
        Assert.True(firstDecision.IsAccepted);
        Assert.True(overflowDecision.IsAccepted);

        var firstExecuteCount = 0;
        var overflowExecuteCount = 0;
        var first = await admission.ExecuteEvaluateOnceAsync(
            firstDecision,
            """{"requestId":"disabled-quota-first"}""",
            _ =>
            {
                Interlocked.Increment(ref firstExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-disabled-quota-first"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);
        var overflow = await admission.ExecuteEvaluateOnceAsync(
            overflowDecision,
            """{"requestId":"disabled-quota-overflow","private":"PRIVATE_DISABLED_QUOTA_SECRET"}""",
            _ =>
            {
                Interlocked.Increment(ref overflowExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-disabled-quota-overflow"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(200, first.StatusCode);
        Assert.Equal(1, firstExecuteCount);
        Assert.Equal(0, overflowExecuteCount);
        AssertRequestQuotaRejected(
            overflow,
            expectedLimit: 1,
            expectedWindowSeconds: 20,
            expectedRetryAfterSeconds: 20,
            "disabled-quota-overflow",
            "PRIVATE_DISABLED_QUOTA_SECRET",
            CurrentToken,
            PreviousToken,
            TenantA);
    }

    [Theory]
    [InlineData(0, 0, DesignConversionModelServerRouteAdmission.MinimumRequestQuotaLimit, DesignConversionModelServerRouteAdmission.MinimumRequestQuotaWindowSeconds)]
    [InlineData(
        DesignConversionModelServerRouteAdmission.MaximumRequestQuotaLimit + 10,
        DesignConversionModelServerRouteAdmission.MaximumRequestQuotaWindowSeconds + 10,
        DesignConversionModelServerRouteAdmission.MaximumRequestQuotaLimit,
        DesignConversionModelServerRouteAdmission.MaximumRequestQuotaWindowSeconds)]
    public void RequestQuotaBounds_AreNormalizedAndPublishedInPublicAndBoundaryMetadata(
        int configuredLimit,
        int configuredWindowSeconds,
        int expectedLimit,
        int expectedWindowSeconds)
    {
        var admission = CreateAdmission(
            requestQuotaLimit: configuredLimit,
            requestQuotaWindow: TimeSpan.FromSeconds(configuredWindowSeconds));
        Assert.Equal(expectedLimit, admission.RequestQuotaLimit);
        Assert.Equal(TimeSpan.FromSeconds(expectedWindowSeconds), admission.RequestQuotaWindow);
        Assert.True((bool)admission.PublicMetadata()["routeAdmissionRequestQuotaActive"]!);
        Assert.Equal(expectedLimit, admission.PublicMetadata()["routeAdmissionRequestQuotaLimit"]);
        Assert.Equal(expectedWindowSeconds, admission.PublicMetadata()["routeAdmissionRequestQuotaWindowSeconds"]);

        var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "quota-bounds")));
        Assert.True(decision.IsAccepted);
        Assert.Equal(expectedLimit.ToString(CultureInfo.InvariantCulture), decision.BoundaryMetadata["request_quota_limit"]);
        Assert.Equal(expectedWindowSeconds.ToString(CultureInfo.InvariantCulture), decision.BoundaryMetadata["request_quota_window_seconds"]);
        Assert.Equal("server_route_admission_instance_tenant_binding", decision.BoundaryMetadata["request_quota_partition_authority"]);
        Assert.StartsWith("sha256:", decision.BoundaryMetadata["request_quota_partition_ref"]);
        Assert.Equal(bool.FalseString, decision.BoundaryMetadata["request_quota_raw_partition_exposed"]);
        Assert.DoesNotContain(TenantA, JsonSerializer.Serialize(decision.BoundaryMetadata), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_SameKeyReplayConsumesRequestQuotaWithoutExecutingTwice()
    {
        var admission = CreateAdmission(
            requestQuotaLimit: 2,
            requestQuotaWindow: TimeSpan.FromSeconds(30));
        var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "quota-replay")));
        var overflowDecision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "quota-replay-overflow")));
        Assert.True(decision.IsAccepted);
        Assert.True(overflowDecision.IsAccepted);

        var executeCount = 0;
        var overflowExecuteCount = 0;
        var first = await admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"requestId":"quota-replay","value":1}""",
            _ =>
            {
                Interlocked.Increment(ref executeCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-quota-replay","requestId":"quota-replay"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);
        var replay = await admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"value":1,"requestId":"quota-replay"}""",
            _ =>
            {
                Interlocked.Increment(ref executeCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-quota-replay-duplicate"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);
        var overflow = await admission.ExecuteEvaluateOnceAsync(
            overflowDecision,
            """{"requestId":"quota-replay-overflow"}""",
            _ =>
            {
                Interlocked.Increment(ref overflowExecuteCount);
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-quota-replay-overflow"}"""));
            },
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(200, first.StatusCode);
        Assert.Equal(first.BodyJson, replay.BodyJson);
        Assert.Equal(1, executeCount);
        Assert.Equal(0, overflowExecuteCount);
        AssertRequestQuotaRejected(
            overflow,
            expectedLimit: 2,
            expectedWindowSeconds: 30,
            expectedRetryAfterSeconds: 30,
            "quota-replay-overflow",
            CurrentToken,
            PreviousToken,
            TenantA);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-8, 1)]
    [InlineData(
        DesignConversionModelServerRouteAdmission.MaximumConcurrentWorkloads + 10,
        DesignConversionModelServerRouteAdmission.MaximumConcurrentWorkloads)]
    public void MaxConcurrentWorkloads_IsSafelyBoundedAndPublished(int configured, int expected)
    {
        var admission = CreateAdmission(maxConcurrentWorkloads: configured);
        Assert.Equal(expected, admission.MaxConcurrentWorkloads);
        Assert.Equal(expected, admission.PublicMetadata()["routeAdmissionMaxConcurrentWorkloads"]);

        var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "bounded-workload")));
        Assert.True(decision.IsAccepted);
        Assert.Equal(expected.ToString(CultureInfo.InvariantCulture), decision.BoundaryMetadata["workload_limit"]);
        Assert.Equal(bool.FalseString, decision.BoundaryMetadata["runtime_invoked"]);

        var disabled = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: false,
                maxConcurrentWorkloads: configured),
            () => FixedNow);
        Assert.Equal(expected, disabled.MaxConcurrentWorkloads);
        Assert.Equal(expected, disabled.PublicMetadata()["routeAdmissionMaxConcurrentWorkloads"]);
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_CanceledWaiterDoesNotCancelSharedExecutionAndReplayCompletesOnce()
    {
        var admission = CreateAdmission();
        var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "cancel-waiter")));
        Assert.True(decision.IsAccepted);
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var executeCount = 0;
        var appendCount = 0;

        async Task<DesignConversionModelServerHttpResponse> Execute(CancellationToken ct)
        {
            Assert.False(ct.CanBeCanceled);
            Interlocked.Increment(ref executeCount);
            Interlocked.Increment(ref appendCount);
            started.TrySetResult(true);
            await release.Task.WaitAsync(TimeSpan.FromSeconds(5));
            return JsonResponse(200, """{"jobId":"thin-job-cancel-waiter","requestId":"cancel-waiter"}""");
        }

        using var callerCts = new CancellationTokenSource();
        var first = admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"requestId":"cancel-waiter"}""",
            Execute,
            Conflict,
            InvalidJson,
            callerCts.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await callerCts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);

        var replayWhileRunning = admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"requestId":"cancel-waiter"}""",
            Execute,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        release.TrySetResult(true);
        var replayResponse = await replayWhileRunning.WaitAsync(TimeSpan.FromSeconds(5));
        var sequentialReplay = await admission.ExecuteEvaluateOnceAsync(
            decision,
            """{"requestId":"cancel-waiter"}""",
            Execute,
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(200, replayResponse.StatusCode);
        Assert.Equal(replayResponse.BodyJson, sequentialReplay.BodyJson);
        Assert.Equal(1, executeCount);
        Assert.Equal(1, appendCount);
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_CanceledOnlyWaiterBindsJobScopeBeforeIdempotencyEviction()
    {
        var now = FixedNow;
        const string originalKey = "cancel-only-bind";
        const string originalJobId = "thin-job-cancel-only-bind";
        var admission = CreateAdmission(
            idempotencyCapacity: 2,
            idempotencyTtl: TimeSpan.FromMinutes(30),
            utcNow: () => now);
        var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: originalKey)));
        Assert.True(decision.IsAccepted);
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var executeCount = 0;

        async Task<DesignConversionModelServerHttpResponse> Execute(CancellationToken ct)
        {
            Assert.False(ct.CanBeCanceled);
            Interlocked.Increment(ref executeCount);
            started.TrySetResult(true);
            await release.Task.WaitAsync(TimeSpan.FromSeconds(5));
            return JsonResponse(200, $$"""{"jobId":"{{originalJobId}}","requestId":"{{originalKey}}"}""");
        }

        using var callerCts = new CancellationTokenSource();
        var first = admission.ExecuteEvaluateOnceAsync(
            decision,
            $$"""{"requestId":"{{originalKey}}"}""",
            Execute,
            Conflict,
            InvalidJson,
            callerCts.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await callerCts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);

        release.TrySetResult(true);
        await WaitForIdempotencyCompletionAsync(admission, decision.Scope, originalKey);

        for (var index = 0; index < 2; index++)
        {
            now = now.AddSeconds(1);
            var fillerKey = "cancel-only-bind-filler-" + index;
            var fillerDecision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: fillerKey)));
            Assert.True(fillerDecision.IsAccepted);
            var fillerResponse = await admission.ExecuteEvaluateOnceAsync(
                fillerDecision,
                $$"""{"requestId":"{{fillerKey}}"}""",
                _ => Task.FromResult(JsonResponse(500, $$"""{"errorCode":"SCRIPTED_FILLER_{{index}}"}""")),
                Conflict,
                InvalidJson,
                CancellationToken.None);
            Assert.Equal(500, fillerResponse.StatusCode);
        }

        Assert.Equal(1, executeCount);
        Assert.Null(PrivateIdempotencyResponseTask(admission, decision.Scope, originalKey));
        var status = admission.AdmitStatus(StatusRequest(originalJobId, Header(Token: CurrentToken)));
        Assert.True(status.IsAccepted);
        Assert.True(admission.IsStatusJobAllowed(status, originalJobId));
    }

    [Fact]
    public async Task ExecuteEvaluateOnceAsync_RemovesFaultedAndCanceledUnderlyingEntriesBeforeReplay()
    {
        var faultAdmission = CreateAdmission();
        var faultDecision = faultAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "fault-key")));
        Assert.True(faultDecision.IsAccepted);
        const string privateFaultBodyJson = """{"requestId":"fault-key","private":"PRIVATE_FAULT_RETRY_BODY","tree":"PRIVATE_FAULT_RETRY_TREE"}""";
        const string privateFaultMessage = "scripted route admission failure PRIVATE_FAULT_RETRY_EXCEPTION C:\\private\\route-admission\\fault.json";
        var faultExecuteCount = 0;

        Task<DesignConversionModelServerHttpResponse> FaultThenSucceed(CancellationToken _)
        {
            return Interlocked.Increment(ref faultExecuteCount) == 1
                ? Task.FromException<DesignConversionModelServerHttpResponse>(new InvalidOperationException(privateFaultMessage))
                : Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-fault-key","requestId":"fault-key"}"""));
        }

        var faultFailure = await faultAdmission.ExecuteEvaluateOnceAsync(
            faultDecision,
            privateFaultBodyJson,
            FaultThenSucceed,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        AssertSanitizedHandlerFailure(
            faultFailure,
            500,
            DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode,
            "handler_exception_failed_closed",
            privateFaultMessage,
            privateFaultBodyJson,
            CurrentToken,
            PreviousToken,
            TenantA);
        var faultRetry = await faultAdmission.ExecuteEvaluateOnceAsync(
            faultDecision,
            privateFaultBodyJson,
            FaultThenSucceed,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        var faultReplay = await faultAdmission.ExecuteEvaluateOnceAsync(
            faultDecision,
            privateFaultBodyJson,
            FaultThenSucceed,
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(200, faultRetry.StatusCode);
        Assert.Equal(faultRetry.BodyJson, faultReplay.BodyJson);
        Assert.Equal(2, faultExecuteCount);

        var canceledAdmission = CreateAdmission();
        var canceledDecision = canceledAdmission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: "underlying-cancel")));
        Assert.True(canceledDecision.IsAccepted);
        const string privateCanceledBodyJson = """{"requestId":"underlying-cancel","private":"PRIVATE_CANCEL_RETRY_BODY","css":"PRIVATE_CANCEL_RETRY_CSS"}""";
        var canceledExecuteCount = 0;

        Task<DesignConversionModelServerHttpResponse> CancelThenSucceed(CancellationToken _)
        {
            if (Interlocked.Increment(ref canceledExecuteCount) != 1)
                return Task.FromResult(JsonResponse(200, """{"jobId":"thin-job-underlying-cancel","requestId":"underlying-cancel"}"""));

            using var canceledSource = new CancellationTokenSource();
            canceledSource.Cancel();
            return Task.FromCanceled<DesignConversionModelServerHttpResponse>(canceledSource.Token);
        }

        var canceledFailure = await canceledAdmission.ExecuteEvaluateOnceAsync(
            canceledDecision,
            privateCanceledBodyJson,
            CancelThenSucceed,
            Conflict,
            InvalidJson,
            CancellationToken.None);
        AssertSanitizedHandlerFailure(
            canceledFailure,
            499,
            DesignConversionModelServerEndpointRegistry.EndpointCancelledErrorCode,
            "handler_cancellation_failed_closed",
            privateCanceledBodyJson,
            CurrentToken,
            PreviousToken,
            TenantA);
        var canceledRetry = await canceledAdmission.ExecuteEvaluateOnceAsync(
            canceledDecision,
            privateCanceledBodyJson,
            CancelThenSucceed,
            Conflict,
            InvalidJson,
            CancellationToken.None);

        Assert.Equal(200, canceledRetry.StatusCode);
        Assert.Equal(2, canceledExecuteCount);
    }

    [Fact]
    public async Task StatusJobScopes_AreTtlExpiredAndCapacityEvictedWithoutDisclosure()
    {
        var now = FixedNow;
        var admission = CreateAdmission(
            idempotencyCapacity: 2,
            idempotencyTtl: TimeSpan.FromMinutes(5),
            utcNow: () => now);
        var evaluator = new ThrowingEvaluator();
        var api = new DesignConversionModelServerApi(evaluator, () => now);
        var handler = new DesignConversionModelServerStatusHandler(api, admission);

        async Task BindJobAsync(string key, string jobId)
        {
            var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: key)));
            Assert.True(decision.IsAccepted);
            var response = await admission.ExecuteEvaluateOnceAsync(
                decision,
                $$"""{"requestId":"{{key}}"}""",
                _ => Task.FromResult(JsonResponse(200, $$"""{"jobId":"{{jobId}}","requestId":"{{key}}"}""")),
                Conflict,
                InvalidJson,
                CancellationToken.None);
            Assert.Equal(200, response.StatusCode);
        }

        await BindJobAsync("job-a-key", "thin-job-a-private");
        now = now.AddSeconds(1);
        await BindJobAsync("job-b-key", "thin-job-b-private");
        now = now.AddSeconds(1);

        var jobAStatus = admission.AdmitStatus(StatusRequest("thin-job-a-private", Header(Token: CurrentToken)));
        var jobBStatusWithPreviousToken = admission.AdmitStatus(StatusRequest("thin-job-b-private", Header(Token: PreviousToken)));
        Assert.True(admission.IsStatusJobAllowed(jobAStatus, "thin-job-a-private"));
        Assert.True(admission.IsStatusJobAllowed(jobBStatusWithPreviousToken, "thin-job-b-private"));

        await BindJobAsync("job-c-key", "thin-job-c-private");
        Assert.True(PrivateDictionaryCount(admission, "_jobScopes") <= 2);
        Assert.False(admission.IsStatusJobAllowed(jobAStatus, "thin-job-a-private"));

        var evicted = await handler.HandleAsync(StatusRequest("thin-job-a-private", Header(Token: CurrentToken)));
        Assert.Equal(404, evicted.StatusCode);
        Assert.Equal(0, evaluator.CallCount);
        AssertNoLeak(evicted.BodyJson, "thin-job-a-private", CurrentToken, PreviousToken);

        now = now.AddMinutes(6);
        var expiredStatus = admission.AdmitStatus(StatusRequest("thin-job-c-private", Header(Token: CurrentToken)));
        Assert.False(admission.IsStatusJobAllowed(expiredStatus, "thin-job-c-private"));
        Assert.Equal(0, PrivateDictionaryCount(admission, "_jobScopes"));

        var expired = await handler.HandleAsync(StatusRequest("thin-job-c-private", Header(Token: CurrentToken)));
        Assert.Equal(404, expired.StatusCode);
        Assert.Equal(0, evaluator.CallCount);
        AssertNoLeak(expired.BodyJson, "thin-job-c-private", CurrentToken, PreviousToken);
    }

    [Fact]
    public async Task HandlerRejections_DoNotLeakTokensDigestsRawPayloadsOrDeniedStatusPaths()
    {
        const string privateMarker = "PRIVATE_ROUTE_ADMISSION_PAYLOAD_SECRET";
        const string privateJobPath = "thin-job-PRIVATE-DENIED-PATH";
        var admission = CreateAdmission();
        var evaluator = new ThrowingEvaluator();
        var api = new DesignConversionModelServerApi(evaluator, () => FixedNow);
        var evaluateHandler = new DesignConversionModelServerHttpHandler(api, admission);
        var statusHandler = new DesignConversionModelServerStatusHandler(api, admission);
        var rawBody = $$"""
                      {
                        "requestId":"leak-check",
                        "html":"<main data-secret='{{privateMarker}}'>Secret</main>",
                        "css":".secret{background:'{{privateMarker}}';}"
                      }
                      """;

        var invalidToken = await evaluateHandler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: rawBody,
            ContentType: "application/json",
            Headers: Header(Token: "wrong-token", Idempotency: "leak-key")));
        var deniedStatus = await statusHandler.HandleAsync(StatusRequest(privateJobPath, Header(Token: CurrentToken)));

        Assert.Equal(403, invalidToken.StatusCode);
        Assert.Equal(404, deniedStatus.StatusCode);
        Assert.Equal(0, evaluator.CallCount);
        AssertNoLeak(invalidToken.BodyJson, rawBody, privateMarker, CurrentToken, PreviousToken, "wrong-token");
        AssertNoLeak(deniedStatus.BodyJson, privateJobPath, CurrentToken, PreviousToken);

        using var invalidTokenDocument = JsonDocument.Parse(invalidToken.BodyJson);
        Assert.Equal(
            bool.FalseString,
            invalidTokenDocument.RootElement
                .GetProperty("boundaryMetadata")
                .GetProperty("idempotency_payload_digest_exposed")
                .GetString());
    }

    [Fact]
    public async Task AspNetEvaluateAdmissionRejectsMissingTokenBeforeReadingBody()
    {
        var admission = CreateAdmission();
        var evaluator = new ThrowingEvaluator();
        var api = new DesignConversionModelServerApi(evaluator, () => FixedNow);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(
            new DesignConversionModelServerHttpHandler(api, admission));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        await using var requestBody = new ThrowOnReadStream();
        await using var responseBody = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = DesignConversionModelServerEndpointRegistry.EndpointPath;
        context.Request.ContentType = "application/json";
        context.Request.Body = requestBody;
        context.Response.Body = responseBody;

        var method = typeof(DesignConversionModelServerAspNetEndpointMapper).GetMethod(
            "DispatchEvaluateAsync",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(method.Invoke(null, [context, registry]));
        await task;

        Assert.Equal(401, context.Response.StatusCode);
        Assert.False(requestBody.ReadAttempted);
        Assert.Equal(0, evaluator.CallCount);
        var bodyJson = Encoding.UTF8.GetString(responseBody.ToArray());
        Assert.Contains("DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED", bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(CurrentToken, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(PreviousToken, bodyJson, StringComparison.Ordinal);
    }

    public static IEnumerable<object[]> InvalidTenantIds()
    {
        yield return ["", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_REQUIRED"];
        yield return ["   ", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_REQUIRED"];
        yield return ["tenant:route-admission:alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["tenant/route-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["tenant\\route-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["https://tenant-route-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["../tenant-route-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return [".", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["..", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["-tenant-route-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["tenant-route-admission-alpha-", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["tenant--route-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["tenant route-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["tenant\troute-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["tenant\nroute-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["tenant\u0001route-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["ten\u0430nt-route-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return ["tenant-\u00e9-route-admission-alpha", "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
        yield return [new string('a', DesignConversionModelActivationRegistry.MaximumCanonicalTenantIdLength + 1), "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID"];
    }

    private static DesignConversionModelServerRouteAdmission CreateAdmission(
        DateTimeOffset? previousExpiresAtUtc = null,
        int idempotencyCapacity = 16,
        TimeSpan? idempotencyTtl = null,
        Func<DateTimeOffset>? utcNow = null,
        string? tenantId = TenantA,
        string? durableIdempotencyStoreRoot = null,
        int maxConcurrentWorkloads = DesignConversionModelServerRouteAdmission.DefaultMaxConcurrentWorkloads,
        int requestQuotaLimit = DesignConversionModelServerRouteAdmission.DefaultRequestQuotaLimit,
        TimeSpan? requestQuotaWindow = null)
        => DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: CurrentToken,
                previousToken: PreviousToken,
                previousTokenExpiresAtUtc: previousExpiresAtUtc ?? FixedNow.AddHours(1),
                idempotencyCapacity: idempotencyCapacity,
                idempotencyTtl: idempotencyTtl ?? TimeSpan.FromMinutes(30),
                tenantId: tenantId,
                durableIdempotencyStoreRoot: durableIdempotencyStoreRoot,
                maxConcurrentWorkloads: maxConcurrentWorkloads,
                requestQuotaLimit: requestQuotaLimit,
                requestQuotaWindow: requestQuotaWindow),
            utcNow ?? (() => FixedNow));

    private static DesignConversionModelServerHttpRequest EvaluateRequest(
        IReadOnlyDictionary<string, string>? headers,
        string? path = null)
        => new(
            Method: "POST",
            Path: path ?? DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: null,
            ContentType: "application/json",
            Headers: headers);

    private static DesignConversionModelServerHttpRequest MutationRequest(
        string method,
        string path,
        IReadOnlyDictionary<string, string>? headers)
        => new(
            Method: method,
            Path: path,
            BodyJson: null,
            ContentType: "application/json",
            Headers: headers);

    private static DesignConversionModelServerHttpRequest StatusRequest(
        string jobId,
        IReadOnlyDictionary<string, string>? headers)
        => new(
            Method: "GET",
            Path: DesignConversionModelServerStatusHandler.EndpointPathPrefix + jobId,
            BodyJson: null,
            ContentType: null,
            Headers: headers);

    private static IReadOnlyDictionary<string, string> Header(
        string? Token = null,
        string? Authorization = null,
        string? Idempotency = null)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (Authorization is not null)
            headers[DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = Authorization;
        else if (Token is not null)
            headers[DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = "Bearer " + Token;
        if (Idempotency is not null)
            headers[DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] = Idempotency;

        return headers;
    }

    private static DesignConversionModelServerHttpResponse JsonResponse(int statusCode, string bodyJson)
        => new(
            statusCode,
            "application/json; charset=utf-8",
            bodyJson,
            new Dictionary<string, string>(StringComparer.Ordinal));

    private static DesignConversionModelServerHttpResponse Conflict(
        DesignConversionModelServerRouteAdmissionDecision _)
        => JsonResponse(409, """{"errorCode":"DESIGN_CONVERSION_IDEMPOTENCY_KEY_CONFLICT"}""");

    private static DesignConversionModelServerHttpResponse InvalidJson(string message)
        => JsonResponse(400, JsonSerializer.Serialize(new { errorCode = "REQUEST_JSON_INVALID", errorMessage = message }));

    private static void AssertNoLeak(string bodyJson, params string[] forbidden)
    {
        foreach (var value in forbidden.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            Assert.DoesNotContain(value, bodyJson, StringComparison.Ordinal);
            foreach (var marker in SensitiveMarkers(value))
                Assert.DoesNotContain(marker, bodyJson, StringComparison.Ordinal);
        }
    }

    private static void AssertSanitizedHandlerFailure(
        DesignConversionModelServerHttpResponse response,
        int expectedStatusCode,
        string expectedErrorCode,
        string expectedErrorRef,
        params string[] forbidden)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(expectedErrorRef, root.GetProperty("errorRef").GetString());
        Assert.Equal(expectedStatusCode, root.GetProperty("statusCode").GetInt32());

        var errorMessage = root.GetProperty("errorMessage").GetString();
        Assert.NotNull(errorMessage);
        Assert.DoesNotContain("scripted", errorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PRIVATE_", errorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("C:\\", errorMessage, StringComparison.OrdinalIgnoreCase);

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(expectedErrorRef, metadata.GetProperty("route_admission_error_ref").GetString());
        Assert.Equal(expectedStatusCode.ToString(CultureInfo.InvariantCulture), metadata.GetProperty("route_admission_status_code").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_authorized").GetString());
        Assert.Equal("ref_only", metadata.GetProperty("route_admission_path").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_raw_path_exposed").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("handler_execution_failed_closed").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("handler_failure_cleanup_completed").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("handler_failure_response_published_after_cleanup").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("handler_exception_message_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("handler_exception_type_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_body_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_payload_digest_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_key_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_value_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("training_ready").GetString());
        AssertNoLeak(response.BodyJson, forbidden);
    }

    private static void AssertSanitizedDurableFailure(
        DesignConversionModelServerHttpResponse response,
        string expectedErrorRef,
        params string[] forbidden)
    {
        Assert.Equal(503, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(
            "DESIGN_CONVERSION_IDEMPOTENCY_DURABLE_REPLAY_FAILED_CLOSED",
            root.GetProperty("errorCode").GetString());
        Assert.Equal(expectedErrorRef, root.GetProperty("errorRef").GetString());

        var errorMessage = root.GetProperty("errorMessage").GetString();
        Assert.NotNull(errorMessage);
        Assert.DoesNotContain("PRIVATE_", errorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("C:\\", errorMessage, StringComparison.OrdinalIgnoreCase);

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_authorized").GetString());
        Assert.Equal(expectedErrorRef, metadata.GetProperty("route_admission_error_ref").GetString());
        Assert.Equal("503", metadata.GetProperty("route_admission_status_code").GetString());
        Assert.Equal("ref_only", metadata.GetProperty("route_admission_path").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_raw_path_exposed").GetString());
        Assert.Equal("failed_closed", metadata.GetProperty("idempotency_durable_replay_result").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_body_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_payload_digest_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_key_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_value_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("browser_runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("model_provider_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("training_ready").GetString());
        AssertNoLeak(response.BodyJson, forbidden);
    }

    private static IEnumerable<string> SensitiveMarkers(string value)
    {
        var parts = value.Split(
            new[]
            {
                ' ', '"', '\'', '<', '>', '{', '}', '[', ']', '(', ')', ':', ';', ',', '?', '&',
                '=', '\\', '/', '\r', '\n', '\t'
            },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (part.Contains("PRIVATE_", StringComparison.Ordinal) ||
                part.Contains("current-route-token-", StringComparison.Ordinal) ||
                part.Contains("previous-route-token-", StringComparison.Ordinal) ||
                part.Contains("tenant-route-admission-", StringComparison.Ordinal))
            {
                yield return part;
            }
        }
    }

    private static void AssertIdempotencyCapacityRejected(
        DesignConversionModelServerHttpResponse response,
        params string[] forbidden)
    {
        Assert.Equal(429, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal("DESIGN_CONVERSION_IDEMPOTENCY_CAPACITY_EXHAUSTED", root.GetProperty("errorCode").GetString());
        Assert.Equal("idempotency_cache_capacity_exhausted", root.GetProperty("errorRef").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        AssertNoLeak(response.BodyJson, forbidden);
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_authorized").GetString());
        Assert.Equal("idempotency_cache_capacity_exhausted", metadata.GetProperty("route_admission_error_ref").GetString());
        Assert.Equal("429", metadata.GetProperty("route_admission_status_code").GetString());
        AssertRejectedResponsePrivacyMetadata(metadata);
    }

    private static void AssertRejectedResponsePrivacyMetadata(JsonElement metadata)
    {
        Assert.Equal("ref_only", metadata.GetProperty("route_admission_method").GetString());
        Assert.Equal("ref_only", metadata.GetProperty("route_admission_path").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_raw_path_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_body_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_user_private_data_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_payload_digest_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_key_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_value_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_hash_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_fingerprint_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("browser_runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("model_provider_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("training_ready").GetString());
    }

    private static void AssertWorkloadRejected(
        DesignConversionModelServerHttpResponse response,
        DesignConversionModelServerRouteAdmissionDecision acceptedAdmission,
        int expectedLimit,
        int expectedActive,
        params string[] forbidden)
    {
        Assert.Equal(429, response.StatusCode);
        Assert.Equal("1", response.Headers["Retry-After"]);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal("DESIGN_CONVERSION_WORKLOAD_CAPACITY_EXHAUSTED", root.GetProperty("errorCode").GetString());
        Assert.Equal("workload_capacity_exhausted", root.GetProperty("errorRef").GetString());
        Assert.Equal(1, root.GetProperty("retryAfterSeconds").GetInt32());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.True(acceptedAdmission.IsAccepted);
        Assert.Equal(
            acceptedAdmission.BoundaryMetadata["route_admission_authorized"],
            metadata.GetProperty("route_admission_authorized").GetString());
        Assert.Equal(
            acceptedAdmission.BoundaryMetadata["route_admission_error_ref"],
            metadata.GetProperty("route_admission_error_ref").GetString());
        Assert.Equal(
            acceptedAdmission.BoundaryMetadata["route_admission_status_code"],
            metadata.GetProperty("route_admission_status_code").GetString());
        AssertNoLeak(response.BodyJson, forbidden);
        AssertRejectedResponsePrivacyMetadata(metadata);
        Assert.Equal(bool.FalseString, metadata.GetProperty("workload_admitted").GetString());
        Assert.Equal("capacity_exhausted", metadata.GetProperty("workload_admission_rejection_reason").GetString());
        Assert.Equal("429", metadata.GetProperty("workload_admission_status_code").GetString());
        Assert.Equal(expectedLimit.ToString(CultureInfo.InvariantCulture), metadata.GetProperty("workload_limit").GetString());
        Assert.Equal(expectedActive.ToString(CultureInfo.InvariantCulture), metadata.GetProperty("workload_active_count").GetString());
        Assert.Equal("0", metadata.GetProperty("workload_available_count").GetString());
    }

    private static void AssertRequestQuotaRejected(
        DesignConversionModelServerHttpResponse response,
        int expectedLimit,
        int expectedWindowSeconds,
        int expectedRetryAfterSeconds,
        params string[] forbidden)
    {
        Assert.Equal(429, response.StatusCode);
        Assert.Equal(expectedRetryAfterSeconds.ToString(CultureInfo.InvariantCulture), response.Headers["Retry-After"]);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal("DESIGN_CONVERSION_REQUEST_QUOTA_EXHAUSTED", root.GetProperty("errorCode").GetString());
        Assert.Equal("request_quota_exhausted", root.GetProperty("errorRef").GetString());
        Assert.Equal(expectedRetryAfterSeconds, root.GetProperty("retryAfterSeconds").GetInt32());

        var metadata = root.GetProperty("boundaryMetadata");
        AssertNoLeak(response.BodyJson, forbidden);
        AssertRejectedResponsePrivacyMetadata(metadata);
        Assert.Equal(bool.FalseString, metadata.GetProperty("request_quota_admitted").GetString());
        Assert.Equal("quota_exhausted", metadata.GetProperty("request_quota_admission_rejection_reason").GetString());
        Assert.Equal("429", metadata.GetProperty("request_quota_admission_status_code").GetString());
        Assert.Equal(expectedLimit.ToString(CultureInfo.InvariantCulture), metadata.GetProperty("request_quota_limit").GetString());
        Assert.Equal(expectedWindowSeconds.ToString(CultureInfo.InvariantCulture), metadata.GetProperty("request_quota_window_seconds").GetString());
        Assert.Equal("0", metadata.GetProperty("request_quota_remaining").GetString());
        Assert.Equal(expectedRetryAfterSeconds.ToString(CultureInfo.InvariantCulture), metadata.GetProperty("request_quota_retry_after_seconds").GetString());
        Assert.Equal("server_route_admission_instance_tenant_binding", metadata.GetProperty("request_quota_partition_authority").GetString());
        Assert.StartsWith("sha256:", metadata.GetProperty("request_quota_partition_ref").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("request_quota_refs_only").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("request_quota_raw_partition_exposed").GetString());
    }

    private static void AssertMutationMetadataDoesNotLeak(
        DesignConversionModelServerRouteAdmissionDecision decision,
        string routePath,
        string bodyJson)
    {
        var metadata = decision.BoundaryMetadata;
        Assert.Equal(bool.TrueString, metadata["idempotency_route_scoped"]);
        Assert.Equal(bool.FalseString, metadata["idempotency_route_method_path_exposed"]);
        Assert.Equal(bool.FalseString, metadata["idempotency_effective_scope_contains_raw_route"]);
        Assert.Equal(bool.FalseString, metadata["idempotency_payload_digest_exposed"]);
        Assert.Equal(bool.FalseString, metadata["route_admission_raw_tenant_id_exposed"]);
        Assert.Equal(bool.FalseString, metadata["token_value_exposed"]);
        Assert.Equal(bool.FalseString, metadata["token_hash_exposed"]);
        Assert.Equal(bool.FalseString, metadata["token_fingerprint_exposed"]);
        Assert.Equal("ref_only", metadata["route_admission_method"]);
        Assert.Equal("ref_only", metadata["route_admission_path"]);
        Assert.StartsWith("sha256:", metadata["idempotency_effective_scope_ref"]);
        Assert.StartsWith("sha256:", metadata["idempotency_route_scope_ref"]);
        Assert.StartsWith("sha256:", metadata["idempotency_route_method_ref"]);
        Assert.StartsWith("sha256:", metadata["idempotency_route_path_ref"]);
        Assert.Equal(decision.AdmissionContext.ScopeRef, metadata["idempotency_effective_scope_ref"]);

        var metadataJson = JsonSerializer.Serialize(metadata);
        AssertNoLeak(
            metadataJson,
            CurrentToken,
            PreviousToken,
            TenantA,
            routePath,
            bodyJson,
            "design-conversion-route-admission-request-digest");
    }

    private static string Sha256Hex(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string Sha256Ref(string domain, string value)
        => "sha256:design-conversion-route-admission-" + domain + ":" + Sha256Hex(domain + "\n" + value);

    private static string DurableLockPath(string storeRoot, string scope, string idempotencyKey)
    {
        var keyHash = Sha256Hex(
            "design-conversion-route-admission-idempotency-key-v1\n" +
            scope +
            "\n" +
            idempotencyKey);
        return Path.Combine(storeRoot, "locks", keyHash + ".json.lock");
    }

    private static string DurableRecordPath(string storeRoot, string scope, string idempotencyKey)
    {
        var keyHash = Sha256Hex(
            "design-conversion-route-admission-idempotency-key-v1\n" +
            scope +
            "\n" +
            idempotencyKey);
        return Path.Combine(storeRoot, "records", keyHash + ".json");
    }

    private static async Task WriteAbandonedDurableLockAsync(
        string storeRoot,
        string scope,
        string idempotencyKey,
        string bodyJson,
        DateTimeOffset createdAtUtc,
        TimeSpan ttl)
    {
        var keyHash = Sha256Hex(
            "design-conversion-route-admission-idempotency-key-v1\n" +
            scope +
            "\n" +
            idempotencyKey);
        var keyRef = "sha256:design-conversion-route-admission-idempotency-key:" + keyHash;
        var scopeRef = Sha256Ref("scope-ref", scope);
        var digest = ComputeCanonicalDigest(bodyJson);
        string requestDigest;
        try
        {
            requestDigest = "sha256:design-conversion-route-admission-request-digest:" +
                Convert.ToHexString(digest).ToLowerInvariant();
        }
        finally
        {
            CryptographicOperations.ZeroMemory(digest);
        }

        var createdAtText = createdAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        var expiresAtText = createdAtUtc.Add(ttl).ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        var integrityMaterial = string.Join(
            "\n",
            "design-conversion-route-admission-durable-lock-v1",
            "1",
            "design_conversion_route_admission_idempotency_inflight",
            keyRef,
            scopeRef,
            requestDigest,
            createdAtText,
            expiresAtText);
        var lockJson = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1,
            LockKind = "design_conversion_route_admission_idempotency_inflight",
            KeyRef = keyRef,
            ScopeRef = scopeRef,
            RequestDigest = requestDigest,
            CreatedAtUtc = createdAtText,
            ExpiresAtUtc = expiresAtText,
            IntegritySha256 = Sha256Ref("lock-integrity", integrityMaterial)
        });

        var lockPath = DurableLockPath(storeRoot, scope, idempotencyKey);
        Directory.CreateDirectory(Path.GetDirectoryName(lockPath)!);
        await File.WriteAllTextAsync(lockPath, lockJson, Encoding.UTF8);
        File.SetCreationTimeUtc(lockPath, createdAtUtc.UtcDateTime);
    }

    private static byte[] ComputeCanonicalDigest(string bodyJson)
    {
        using var document = JsonDocument.Parse(bodyJson);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
        {
            WriteCanonical(document.RootElement, writer);
        }

        var canonicalBytes = stream.ToArray();
        try
        {
            return SHA256.HashData(canonicalBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(canonicalBytes);
        }
    }

    private static void WriteCanonical(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject()
                             .Select((property, index) => new { property, index })
                             .OrderBy(item => item.property.Name, StringComparer.Ordinal)
                             .ThenBy(item => item.index))
                {
                    writer.WritePropertyName(property.property.Name);
                    WriteCanonical(property.property.Value, writer);
                }

                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteCanonical(item, writer);
                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;

            case JsonValueKind.Number:
                writer.WriteRawValue(element.GetRawText(), skipInputValidation: true);
                break;

            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                writer.WriteNullValue();
                break;
        }
    }

    private static void RecordMax(ref int maxValue, int candidate)
    {
        int current;
        while (candidate > (current = Volatile.Read(ref maxValue)) &&
               Interlocked.CompareExchange(ref maxValue, candidate, current) != current)
        {
        }
    }

    private static int PrivateDictionaryCount(
        DesignConversionModelServerRouteAdmission admission,
        string fieldName)
    {
        var field = typeof(DesignConversionModelServerRouteAdmission)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        var value = field.GetValue(admission);
        Assert.NotNull(value);
        var countProperty = value.GetType().GetProperty("Count");
        Assert.NotNull(countProperty);
        return Assert.IsType<int>(countProperty.GetValue(value));
    }

    private static object PrivateLifecycleLock(DesignConversionModelServerRouteAdmission admission)
    {
        var field = typeof(DesignConversionModelServerRouteAdmission)
            .GetField("_lifecycleLock", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<object>(field.GetValue(admission));
    }

    private static int PrivateActiveWorkloadCount(DesignConversionModelServerRouteAdmission admission)
    {
        var field = typeof(DesignConversionModelServerRouteAdmission)
            .GetField("_activeWorkloadCount", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<int>(field.GetValue(admission));
    }

    private static async Task WaitForIdempotencyCompletionAsync(
        DesignConversionModelServerRouteAdmission admission,
        string scope,
        string idempotencyKey)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var task = PrivateIdempotencyResponseTask(admission, scope, idempotencyKey);
            if (task?.IsCompleted == true)
            {
                await task.WaitAsync(TimeSpan.FromSeconds(5));
                return;
            }

            await Task.Delay(10);
        }

        throw new TimeoutException("Timed out waiting for the idempotency entry to complete.");
    }

    private static Task<DesignConversionModelServerHttpResponse>? PrivateIdempotencyResponseTask(
        DesignConversionModelServerRouteAdmission admission,
        string scope,
        string idempotencyKey)
    {
        var field = typeof(DesignConversionModelServerRouteAdmission)
            .GetField("_idempotencyEntries", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        var value = field.GetValue(admission);
        Assert.NotNull(value);
        var cacheKey = scope + "\n" + idempotencyKey;

        foreach (var pair in (System.Collections.IEnumerable)value)
        {
            var pairType = pair.GetType();
            var key = Assert.IsType<string>(pairType.GetProperty("Key")?.GetValue(pair));
            if (!string.Equals(key, cacheKey, StringComparison.Ordinal))
                continue;

            var entry = pairType.GetProperty("Value")?.GetValue(pair);
            Assert.NotNull(entry);
            var responseTask = entry.GetType().GetProperty("ResponseTask")?.GetValue(entry);
            return Assert.IsAssignableFrom<Task<DesignConversionModelServerHttpResponse>>(responseTask);
        }

        return null;
    }

    private static string CreateTempDirectory()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-route-admission-durable-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempDirectory(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private static async Task PersistDurableRecordAsync(
        string storeRoot,
        string idempotencyKey,
        string bodyJson,
        string responseBodyJson)
    {
        var admission = CreateAdmission(durableIdempotencyStoreRoot: storeRoot);
        var decision = admission.AdmitEvaluate(EvaluateRequest(headers: Header(Token: CurrentToken, Idempotency: idempotencyKey)));
        Assert.True(decision.IsAccepted);
        var response = await admission.ExecuteEvaluateOnceAsync(
            decision,
            bodyJson,
            _ => Task.FromResult(JsonResponse(200, responseBodyJson)),
            Conflict,
            InvalidJson,
            CancellationToken.None);
        Assert.Equal(200, response.StatusCode);
    }

    private static async Task PadDurableRecordToExactByteLengthAsync(
        string path,
        int targetByteLength)
    {
        var text = Encoding.UTF8.GetString(await File.ReadAllBytesAsync(path));
        Assert.EndsWith("}", text, StringComparison.Ordinal);
        var prefix = ",\"Padding\":\"";
        var suffix = "\"";
        var paddingLength =
            targetByteLength -
            Encoding.UTF8.GetByteCount(text) -
            Encoding.UTF8.GetByteCount(prefix) -
            Encoding.UTF8.GetByteCount(suffix);
        Assert.True(paddingLength >= 0);

        var padded = text[..^1] + prefix + new string('x', paddingLength) + suffix + "}";
        var bytes = Encoding.UTF8.GetBytes(padded);
        Assert.Equal(targetByteLength, bytes.Length);
        await File.WriteAllBytesAsync(path, bytes);
    }

    private sealed class ThrowingEvaluator : IDesignConversionThinClientJobEvaluator
    {
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            return Task.FromException<OperationResult<DesignConversionInternalEvaluationResult>>(
                new InvalidOperationException("Route admission tests must reject before runtime evaluation."));
        }
    }

    private sealed class DeceptiveGrowingSnapshotStream : Stream
    {
        private readonly byte[] _declaredBytes;
        private long _position;

        public DeceptiveGrowingSnapshotStream(byte[] declaredBytes)
        {
            _declaredBytes = declaredBytes;
        }

        public long TotalBytesReturned { get; private set; }

        public int ExtraBytesReturned { get; private set; }

        public int MaxReadRequestBytes { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _declaredBytes.Length;

        public override long Position
        {
            get => _position;
            set => _position = value;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            MaxReadRequestBytes = Math.Max(MaxReadRequestBytes, count);
            if (_position < _declaredBytes.Length)
            {
                var available = (int)Math.Min(count, _declaredBytes.Length - _position);
                Array.Copy(_declaredBytes, _position, buffer, offset, available);
                _position += available;
                TotalBytesReturned += available;
                return available;
            }

            if (ExtraBytesReturned > 0)
                throw new InvalidOperationException("The snapshot reader consumed more than the sentinel byte.");

            buffer[offset] = (byte)'x';
            _position++;
            ExtraBytesReturned++;
            TotalBytesReturned++;
            return 1;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _declaredBytes.Length + offset,
                _ => _position
            };
            return _position;
        }

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }

    private sealed class ThrowOnReadStream : Stream
    {
        public bool ReadAttempted { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadAttempted = true;
            throw new InvalidOperationException("Request body should not be read before route admission rejects.");
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadAttempted = true;
            throw new InvalidOperationException("Request body should not be read before route admission rejects.");
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }
}
