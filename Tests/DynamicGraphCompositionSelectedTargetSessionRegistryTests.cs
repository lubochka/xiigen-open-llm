// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LlmFromScratch.DynamicGraphCompositionLiveLocalServer;
using OrchestratorModel.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphCompositionSelectedTargetSessionRegistryTests
{
    private const string TenantId = "tenant-a";
    private const string CanonicalDigest =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    private const string SensitiveExecutionMarker = "selected-request-must-not-persist";
    private const string EdgeId = "registry-admitted-target";
    private const string AuthorityUnitId = "registry-admitted-authority";
    private const string EvaluationId = "registry-admitted-evaluation";
    private const string A = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string B = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string C = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";
    private static readonly string[] LeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
    private static readonly string[] IdentityProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];
    private static readonly string[] IdentityControlledSignals =
        ["source_id", "source_path", "correlation_id"];
    private static readonly string[] IdentityControlValues =
    [
        "edge-authority-source-id-control",
        "identity-control://edge-authority-source-path",
        "edge-authority-correlation-id-control"
    ];

    [Fact]
    public void PublicSurface_AddsExactSelectedTargetBoundaryAndPreservesLegacyConstructors()
    {
        var registryType = typeof(DynamicGraphCompositionSessionRegistry);
        var constructors = registryType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .OrderBy(constructor => constructor.GetParameters().Length)
            .ToArray();
        Assert.Equal(2, constructors.Length);
        Assert.Equal(
            [typeof(DynamicGraphCompositionServerOptions), typeof(TimeProvider)],
            constructors[0].GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                typeof(DynamicGraphCompositionServerOptions),
                typeof(DynamicGraphCompositionSessionPersistence),
                typeof(TimeProvider)
            ],
            constructors[1].GetParameters().Select(parameter => parameter.ParameterType));
        Assert.False(constructors[0].GetParameters()[0].HasDefaultValue);
        Assert.True(constructors[0].GetParameters()[1].HasDefaultValue);
        Assert.False(constructors[1].GetParameters()[0].HasDefaultValue);
        Assert.False(constructors[1].GetParameters()[1].HasDefaultValue);
        Assert.True(constructors[1].GetParameters()[2].HasDefaultValue);

        var selectedMethod = Assert.Single(
            registryType.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => method.Name == "RunSelectedTargetAsync");
        Assert.Equal(
            typeof(Task<SelectedTargetSessionExecutionResult>),
            selectedMethod.ReturnType);
        Assert.Equal(
            ["tenantId", "sessionId", "request", "executionAllowed", "cancellationToken"],
            selectedMethod.GetParameters().Select(parameter => parameter.Name));
        Assert.Equal(
            [
                typeof(string),
                typeof(string),
                typeof(DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest),
                typeof(Func<bool>),
                typeof(CancellationToken)
            ],
            selectedMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.All(selectedMethod.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));

        var legacyMethod = Assert.Single(
            registryType.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => method.Name == "RunAsync");
        Assert.Equal(typeof(Task<SessionExecutionResult>), legacyMethod.ReturnType);
        Assert.Equal(
            [
                typeof(string),
                typeof(string),
                typeof(DynamicGraphLogicalOrchestratorRunRequest),
                typeof(Func<bool>),
                typeof(CancellationToken)
            ],
            legacyMethod.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["Completed", "Expired", "Unknown", "Busy", "ForbiddenAssembly"],
            Enum.GetNames<SessionExecutionStatus>());
        Assert.Equal(
            ["Completed", "Expired", "Unknown", "Busy", "ForbiddenAssembly", "Unavailable"],
            Enum.GetNames<SelectedTargetSessionExecutionStatus>());

        var resultConstructor = Assert.Single(
            typeof(SelectedTargetSessionExecutionResult).GetConstructors(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal(
            [
                typeof(SelectedTargetSessionExecutionStatus),
                typeof(OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>)
            ],
            resultConstructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["Completed", "Expired", "Unknown", "Busy", "ForbiddenAssembly", "Unavailable"],
            typeof(SelectedTargetSessionExecutionResult)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName)
                .Select(method => method.Name)
                .OrderBy(name => Array.IndexOf(
                    new[] { "Completed", "Expired", "Unknown", "Busy", "ForbiddenAssembly", "Unavailable" },
                    name)));
    }

    [Fact]
    public async Task RunSelectedTargetAsync_EntersLifecycleThenValidatesArgumentsInExactOrder()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var request = SelectedRequest();

        var tenant = await Assert.ThrowsAsync<ArgumentException>(() =>
            registry.RunSelectedTargetAsync(" ", "session", request, () => true, default));
        Assert.Equal("tenantId", tenant.ParamName, StringComparer.Ordinal);
        var session = await Assert.ThrowsAsync<ArgumentException>(() =>
            registry.RunSelectedTargetAsync(TenantId, " ", request, () => true, default));
        Assert.Equal("sessionId", session.ParamName, StringComparer.Ordinal);
        var missingRequest = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            registry.RunSelectedTargetAsync(TenantId, "session", null!, null!, default));
        Assert.Equal("request", missingRequest.ParamName, StringComparer.Ordinal);
        var gate = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            registry.RunSelectedTargetAsync(TenantId, "session", request, null!, default));
        Assert.Equal("executionAllowed", gate.ParamName, StringComparer.Ordinal);

        registry.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            registry.RunSelectedTargetAsync(" ", " ", null!, null!, default));
    }

    [Fact]
    public async Task RunSelectedTargetAsync_CompletedSuccessForwardsExactRequestTokenAndEnvelopeOnce()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "completed-success");
        var request = SelectedRequest("completed-success-run");
        var envelope = SelectedSuccess();
        using var cancellation = new CancellationTokenSource();
        session.SelectedHandler = (_, _) => Task.FromResult(envelope);

        var result = await registry.RunSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            request,
            () => true,
            cancellation.Token);

        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, result.Status);
        Assert.Same(envelope, result.Result);
        Assert.True(result.Result!.IsSuccess);
        Assert.Equal(1, session.SelectedCallCount);
        Assert.Same(request, session.LastSelectedRequest);
        Assert.Equal(cancellation.Token, session.LastSelectedToken);
    }

    [Fact]
    public async Task RunSelectedTargetAsync_CompletedFailurePreservesExactEnvelopeWithoutMapping()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "completed-failure");
        var envelope = OperationResult<
            DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
            "Exact selected-target failure.",
            "EXACT_SELECTED_TARGET_FAILURE");
        session.SelectedHandler = (_, _) => Task.FromResult(envelope);

        var result = await registry.RunSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            SelectedRequest(),
            () => true,
            CancellationToken.None);

        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, result.Status);
        Assert.Same(envelope, result.Result);
        Assert.False(result.Result!.IsSuccess);
        Assert.Equal("EXACT_SELECTED_TARGET_FAILURE", result.Result.ErrorCode);
        Assert.Equal("Exact selected-target failure.", result.Result.ErrorMessage);
        Assert.Equal(1, session.SelectedCallCount);
    }

    [Fact]
    public async Task RunSelectedTargetAsync_LegacyOnlyIsUnavailableBeforeSharedSlotAndCancellation()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options(maxSessions: 2));
        var blocking = new ControlledDualSession();
        var legacyOnly = new LegacyOnlySession();
        var blockingEntry = await PublishAsync(registry, blocking, "unavailable-blocking");
        var legacyEntry = await PublishAsync(registry, legacyOnly, "unavailable-legacy");
        var started = NewSignal();
        var release = NewSignal();
        blocking.SelectedHandler = async (_, _) =>
        {
            started.TrySetResult();
            await release.Task.ConfigureAwait(false);
            return SelectedSuccess();
        };
        var holder = registry.RunSelectedTargetAsync(
            TenantId,
            blockingEntry.SessionId,
            SelectedRequest("slot-holder"),
            () => true,
            CancellationToken.None);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var unavailable = await registry.RunSelectedTargetAsync(
            TenantId,
            legacyEntry.SessionId,
            SelectedRequest("legacy-only"),
            () => true,
            cancellation.Token);

        Assert.Equal(SelectedTargetSessionExecutionStatus.Unavailable, unavailable.Status);
        Assert.Null(unavailable.Result);
        Assert.Equal(0, legacyOnly.LegacyCallCount);
        release.TrySetResult();
        Assert.Equal(
            SelectedTargetSessionExecutionStatus.Completed,
            (await holder).Status);
    }

    [Fact]
    public async Task RunSelectedTargetAsync_InitialDenialOrInvalidationWinsBeforeLookupAndCancellation()
    {
        using var deniedRegistry = new DynamicGraphCompositionSessionRegistry(Options());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var gateCalls = 0;

        var denied = await deniedRegistry.RunSelectedTargetAsync(
            TenantId,
            "unknown-session",
            SelectedRequest(),
            () =>
            {
                Interlocked.Increment(ref gateCalls);
                return false;
            },
            cancellation.Token);

        Assert.Equal(SelectedTargetSessionExecutionStatus.ForbiddenAssembly, denied.Status);
        Assert.Null(denied.Result);
        Assert.Equal(1, Volatile.Read(ref gateCalls));

        using var invalidatedRegistry = new DynamicGraphCompositionSessionRegistry(Options());
        invalidatedRegistry.InvalidateAll();
        gateCalls = 0;
        var invalidated = await invalidatedRegistry.RunSelectedTargetAsync(
            TenantId,
            "unknown-session",
            SelectedRequest(),
            () =>
            {
                Interlocked.Increment(ref gateCalls);
                return true;
            },
            cancellation.Token);
        Assert.Equal(SelectedTargetSessionExecutionStatus.ForbiddenAssembly, invalidated.Status);
        Assert.Equal(0, Volatile.Read(ref gateCalls));
    }

    [Fact]
    public async Task RunSelectedTargetAsync_DenialAfterAdmissionOrExecutionSuppressesResultAndReleasesSlot()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "denial-sequences");
        var envelope = SelectedSuccess();
        session.SelectedHandler = (_, _) => Task.FromResult(envelope);

        var afterAdmissionGate = new SequenceGate(true, false);
        var afterAdmission = await registry.RunSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            SelectedRequest("denied-after-admission"),
            afterAdmissionGate.Invoke,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.ForbiddenAssembly, afterAdmission.Status);
        Assert.Null(afterAdmission.Result);
        Assert.Equal(2, afterAdmissionGate.CallCount);
        Assert.Equal(0, session.SelectedCallCount);

        var afterAdmissionRecovery = await registry.RunSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            SelectedRequest("after-admission-recovery"),
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, afterAdmissionRecovery.Status);
        Assert.Same(envelope, afterAdmissionRecovery.Result);

        var afterExecutionGate = new SequenceGate(true, true, false);
        var afterExecution = await registry.RunSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            SelectedRequest("denied-after-execution"),
            afterExecutionGate.Invoke,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.ForbiddenAssembly, afterExecution.Status);
        Assert.Null(afterExecution.Result);
        Assert.Equal(3, afterExecutionGate.CallCount);
        Assert.Equal(2, session.SelectedCallCount);

        var finalRecovery = await registry.RunSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            SelectedRequest("after-execution-recovery"),
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, finalRecovery.Status);
        Assert.Equal(3, session.SelectedCallCount);
    }

    [Fact]
    public async Task RunSelectedTargetAsync_WrongTenantAndUnknownSessionReturnUnknownWithoutExecution()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "tenant-bound");

        var wrongTenant = await registry.RunSelectedTargetAsync(
            "tenant-b",
            entry.SessionId,
            SelectedRequest(),
            () => true,
            CancellationToken.None);
        var unknown = await registry.RunSelectedTargetAsync(
            TenantId,
            "dgs_unknown",
            SelectedRequest(),
            () => true,
            CancellationToken.None);

        Assert.Equal(SelectedTargetSessionExecutionStatus.Unknown, wrongTenant.Status);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Unknown, unknown.Status);
        Assert.Null(wrongTenant.Result);
        Assert.Null(unknown.Result);
        Assert.Equal(0, session.SelectedCallCount);
    }

    [Fact]
    public async Task RunSelectedTargetAsync_ExpiryCreatesTenantTombstoneThenBecomesUnknown()
    {
        var options = Options(ttl: TimeSpan.FromMinutes(1));
        var persistence = new DynamicGraphCompositionSessionPersistence(options, enabled: true);
        var time = new MutableTimeProvider(DateTimeOffset.Parse("2026-07-25T00:00:00Z"));
        using var registry = new DynamicGraphCompositionSessionRegistry(options, persistence, time);
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "expiry");
        Assert.Single(persistence.PersistedSnapshots);
        time.Advance(TimeSpan.FromSeconds(61));

        var expired = await registry.RunSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            SelectedRequest(),
            () => true,
            CancellationToken.None);

        Assert.Equal(SelectedTargetSessionExecutionStatus.Expired, expired.Status);
        Assert.Null(expired.Result);
        Assert.Empty(persistence.PersistedSnapshots);
        Assert.Equal(1, persistence.DeleteCount);
        Assert.Equal(0, session.SelectedCallCount);

        time.Advance(TimeSpan.FromSeconds(60));
        var unknown = await registry.RunSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            SelectedRequest(),
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Unknown, unknown.Status);
    }

    [Fact]
    public async Task LegacyAndSelectedExecutionShareOneSemaphoreInBothDirections()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options(maxSessions: 2));
        var first = new ControlledDualSession();
        var second = new ControlledDualSession();
        var firstEntry = await PublishAsync(registry, first, "shared-slot-first");
        var secondEntry = await PublishAsync(registry, second, "shared-slot-second");

        var selectedStarted = NewSignal();
        var selectedRelease = NewSignal();
        first.SelectedHandler = async (_, _) =>
        {
            selectedStarted.TrySetResult();
            await selectedRelease.Task.ConfigureAwait(false);
            return SelectedSuccess();
        };
        var selectedHolder = registry.RunSelectedTargetAsync(
            TenantId,
            firstEntry.SessionId,
            SelectedRequest("selected-holder"),
            () => true,
            CancellationToken.None);
        await selectedStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var legacyBusy = await registry.RunAsync(
            TenantId,
            secondEntry.SessionId,
            LegacyRequest(),
            () => true,
            CancellationToken.None);
        Assert.Equal(SessionExecutionStatus.Busy, legacyBusy.Status);
        Assert.Equal(0, second.LegacyCallCount);
        selectedRelease.TrySetResult();
        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, (await selectedHolder).Status);

        var legacyStarted = NewSignal();
        var legacyRelease = NewSignal();
        second.LegacyHandler = async (_, _) =>
        {
            legacyStarted.TrySetResult();
            await legacyRelease.Task.ConfigureAwait(false);
            return LegacySuccess();
        };
        var legacyHolder = registry.RunAsync(
            TenantId,
            secondEntry.SessionId,
            LegacyRequest(),
            () => true,
            CancellationToken.None);
        await legacyStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var selectedBusy = await registry.RunSelectedTargetAsync(
            TenantId,
            firstEntry.SessionId,
            SelectedRequest("selected-busy"),
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Busy, selectedBusy.Status);
        Assert.Equal(1, first.SelectedCallCount);
        legacyRelease.TrySetResult();
        Assert.Equal(SessionExecutionStatus.Completed, (await legacyHolder).Status);
    }

    [Fact]
    public async Task RunSelectedTargetAsync_PreCancelledWaitThrowsWithoutConsumingSlot()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "precancelled");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            registry.RunSelectedTargetAsync(
                TenantId,
                entry.SessionId,
                SelectedRequest(),
                () => true,
                cancellation.Token));
        Assert.Equal(0, session.SelectedCallCount);

        var recovered = await registry.RunSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            SelectedRequest("precancelled-recovery"),
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, recovered.Status);
        Assert.Equal(1, session.SelectedCallCount);
    }

    [Fact]
    public async Task RunSelectedTargetAsync_NonFatalExceptionGraphRethrowsExactAndReleasesSlot()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "nonfatal");
        var expected = new AggregateException(
            "nonfatal-wrapper",
            new InvalidOperationException("nonfatal-inner"));
        session.SelectedHandler = (_, _) => Task.FromException<OperationResult<
            DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>(expected);

        var observed = await Assert.ThrowsAsync<AggregateException>(() =>
            registry.RunSelectedTargetAsync(
                TenantId,
                entry.SessionId,
                SelectedRequest(),
                () => true,
                CancellationToken.None));
        Assert.Same(expected, observed);

        session.SelectedHandler = (_, _) => Task.FromResult(SelectedSuccess());
        var recovered = await registry.RunSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            SelectedRequest("nonfatal-recovery"),
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, recovered.Status);
        Assert.Equal(2, session.SelectedCallCount);
    }

    [Fact]
    public async Task RunSelectedTargetAsync_FatalMarkerInExceptionGraphRethrowsExactAndRetainsSlot()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "fatal-marker");
        var expected = new InvalidOperationException(
            "safe fatal-classifier wrapper",
            new OutOfMemoryException("constructed classifier marker; no memory exhaustion occurred"));
        session.SelectedHandler = (_, _) => Task.FromException<OperationResult<
            DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>(expected);

        var observed = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            registry.RunSelectedTargetAsync(
                TenantId,
                entry.SessionId,
                SelectedRequest(),
                () => true,
                CancellationToken.None));
        Assert.Same(expected, observed);

        session.SelectedHandler = (_, _) => Task.FromResult(SelectedSuccess());
        var blocked = await registry.RunAsync(
            TenantId,
            entry.SessionId,
            LegacyRequest(),
            () => true,
            CancellationToken.None);
        Assert.Equal(SessionExecutionStatus.Busy, blocked.Status);
        Assert.Null(blocked.Result);
        Assert.Equal(0, session.LegacyCallCount);
    }

    [Fact]
    public async Task RunAdmittedSelectedTargetAsync_SurfaceAndValidationOrderAreExact()
    {
        var method = Assert.Single(
            typeof(DynamicGraphCompositionSessionRegistry).GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            candidate => candidate.Name == "RunAdmittedSelectedTargetAsync");
        Assert.True(method.IsAssembly);
        Assert.Equal(typeof(Task<SelectedTargetSessionExecutionResult>), method.ReturnType);
        Assert.Equal(
            [
                "tenantId", "sessionId", "handle", "runId", "scenarioId", "userRequest",
                "executionAllowed", "cancellationToken"
            ],
            method.GetParameters().Select(parameter => parameter.Name));

        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var tenant = await Assert.ThrowsAsync<ArgumentException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                " ", " ", " ", " ", " ", " ", null!, default));
        Assert.Equal("tenantId", tenant.ParamName);
        var session = await Assert.ThrowsAsync<ArgumentException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                TenantId, " ", " ", " ", " ", " ", null!, default));
        Assert.Equal("sessionId", session.ParamName);
        var handle = await Assert.ThrowsAsync<ArgumentException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                TenantId, "session", " ", " ", " ", " ", null!, default));
        Assert.Equal("handle", handle.ParamName);
        var run = await Assert.ThrowsAsync<ArgumentException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                TenantId, "session", ValidHandle(1), " ", " ", " ", null!, default));
        Assert.Equal("runId", run.ParamName);
        var scenario = await Assert.ThrowsAsync<ArgumentException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                TenantId, "session", ValidHandle(1), "run", " ", " ", null!, default));
        Assert.Equal("scenarioId", scenario.ParamName);
        var user = await Assert.ThrowsAsync<ArgumentException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                TenantId, "session", ValidHandle(1), "run", "scenario", " ", null!, default));
        Assert.Equal("userRequest", user.ParamName);
        var gate = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                TenantId,
                "session",
                ValidHandle(1),
                "run",
                "scenario",
                "request",
                null!,
                default));
        Assert.Equal("executionAllowed", gate.ParamName);

        registry.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                " ", " ", " ", " ", " ", " ", null!, default));
    }

    [Fact]
    public async Task RunAdmittedSelectedTargetAsync_ForwardsExactOwnedCarrierOnceOutsideLocks()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "admitted-forwarding");
        var association = ValidAssociation();
        var handle = PublishHandle(entry, association, 1);
        var envelope = AdmittedSuccess(association, "admitted-forwarding-run");
        var cleanupGate = PrivateField<object>(registry, "_cleanupGate");
        var evidenceGate = PrivateField<object>(entry, "_evidenceGate");
        var gateCalls = 0;
        using var cancellation = new CancellationTokenSource();
        session.AdmittedHandler = (request, token) =>
        {
            Assert.False(Monitor.IsEntered(cleanupGate));
            Assert.False(Monitor.IsEntered(evidenceGate));
            Assert.Equal(cancellation.Token, token);
            return Task.FromResult(envelope);
        };

        var result = await registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            handle,
            "admitted-forwarding-run",
            "admitted-forwarding-scenario",
            "admitted forwarding request",
            () =>
            {
                Interlocked.Increment(ref gateCalls);
                Assert.False(Monitor.IsEntered(cleanupGate));
                Assert.False(Monitor.IsEntered(evidenceGate));
                return true;
            },
            cancellation.Token);

        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, result.Status);
        Assert.Same(envelope, result.Result);
        Assert.Equal(1, session.AdmittedCallCount);
        Assert.Equal(3, Volatile.Read(ref gateCalls));
        Assert.Equal("admitted-forwarding-run", session.LastAdmittedRequest!.RunId);
        Assert.Equal("admitted-forwarding-scenario", session.LastAdmittedRequest.ScenarioId);
        Assert.Equal("admitted forwarding request", session.LastAdmittedRequest.UserRequest);
        Assert.Same(association, session.LastAdmittedRequest.Association);
        Assert.Equal(cancellation.Token, session.LastAdmittedToken);
        Assert.NotSame(result.Result!.Value!.Association, result.Result.Value.Association);
        Assert.NotSame(result.Result.Value.ExecutionResult, result.Result.Value.ExecutionResult);
        Assert.NotSame(result.Result.Value.RunSpeed, result.Result.Value.RunSpeed);
        var runSpeed = result.Result.Value.RunSpeed;
        Assert.Equal(EdgeId, runSpeed.Target.TargetId, StringComparer.Ordinal);
        Assert.Equal(1, runSpeed.RunSpeed.EvaluatedRowCount);
        Assert.Equal(0, runSpeed.RunSpeed.CandidateSampleCount);
        Assert.Equal(0, runSpeed.RunSpeed.BatchSampleCount);
        Assert.Equal(23L, runSpeed.RunSpeed.RunSpeed.InferenceElapsedMs);
        Assert.Equal(1000d / 23L, runSpeed.RunSpeed.RunSpeed.ThroughputRowsPerSec);
        Assert.Equal(0L, runSpeed.RunSpeed.RunSpeed.RoutingElapsedMs);
        Assert.Null(runSpeed.RunSpeed.RunSpeed.PerCandidateElapsedMs);
        Assert.Null(runSpeed.RunSpeed.RunSpeed.PerBatchElapsedMs);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Unknown,
            registry.ResolveSelectedTargetEvidence(
                TenantId, entry.SessionId, handle, () => true).Status);
    }

    [Fact]
    public async Task RunAdmittedSelectedTargetAsync_NonConsumingOutcomesDoNotClaimEvidence()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options(maxSessions: 2));
        var admitted = new ControlledDualSession();
        var unavailable = new LegacyOnlySession();
        var admittedEntry = await PublishAsync(registry, admitted, "admitted-non-consuming");
        var unavailableEntry = await PublishAsync(registry, unavailable, "admitted-unavailable");
        var association = ValidAssociation();
        var deniedHandle = PublishHandle(admittedEntry, association, 2);
        var unavailableHandle = PublishHandle(unavailableEntry, ValidAssociation(), 3);

        var denied = await registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            admittedEntry.SessionId,
            deniedHandle,
            "denied-run",
            "scenario",
            "request",
            () => false,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.ForbiddenAssembly, denied.Status);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Found,
            registry.ResolveSelectedTargetEvidence(
                TenantId, admittedEntry.SessionId, deniedHandle, () => true).Status);

        var wrongTenant = await registry.RunAdmittedSelectedTargetAsync(
            "tenant-b",
            admittedEntry.SessionId,
            deniedHandle,
            "wrong-tenant-run",
            "scenario",
            "request",
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Unknown, wrongTenant.Status);
        Assert.Equal(0, admitted.AdmittedCallCount);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Found,
            registry.ResolveSelectedTargetEvidence(
                TenantId, admittedEntry.SessionId, deniedHandle, () => true).Status);

        var unavailableResult = await registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            unavailableEntry.SessionId,
            unavailableHandle,
            "unavailable-run",
            "scenario",
            "request",
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Unavailable, unavailableResult.Status);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Found,
            registry.ResolveSelectedTargetEvidence(
                TenantId, unavailableEntry.SessionId, unavailableHandle, () => true).Status);

        var missing = await registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            admittedEntry.SessionId,
            ValidHandle(99),
            "missing-run",
            "scenario",
            "request",
            () => true,
            CancellationToken.None);
        var unknown = await registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            "dgs_unknown",
            deniedHandle,
            "unknown-run",
            "scenario",
            "request",
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Unknown, missing.Status);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Unknown, unknown.Status);
        Assert.Equal(0, admitted.AdmittedCallCount);

        var time = new MutableTimeProvider(DateTimeOffset.Parse("2026-07-25T00:00:00Z"));
        using var expiredRegistry = new DynamicGraphCompositionSessionRegistry(
            Options(ttl: TimeSpan.FromMinutes(1)),
            time);
        var expiredSession = new ControlledDualSession();
        var expiredEntry = await PublishAsync(expiredRegistry, expiredSession, "admitted-expired");
        var expiredHandle = PublishHandle(expiredEntry, ValidAssociation(), 4);
        time.Advance(TimeSpan.FromSeconds(61));
        var expired = await expiredRegistry.RunAdmittedSelectedTargetAsync(
            TenantId,
            expiredEntry.SessionId,
            expiredHandle,
            "expired-run",
            "scenario",
            "request",
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Expired, expired.Status);
        Assert.DoesNotContain(expiredHandle, RetiredHandles(expiredEntry));
        Assert.Equal(0, expiredSession.AdmittedCallCount);
    }

    [Fact]
    public async Task RunAdmittedSelectedTargetAsync_BusyAndPreCancellationPreserveEvidence()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options(maxSessions: 2));
        var holderSession = new ControlledDualSession();
        var admittedSession = new ControlledDualSession();
        var holderEntry = await PublishAsync(registry, holderSession, "admitted-busy-holder");
        var admittedEntry = await PublishAsync(registry, admittedSession, "admitted-busy-target");
        var started = NewSignal();
        var release = NewSignal();
        holderSession.LegacyHandler = async (_, _) =>
        {
            started.TrySetResult();
            await release.Task.ConfigureAwait(false);
            return LegacySuccess();
        };
        var holder = registry.RunAsync(
            TenantId,
            holderEntry.SessionId,
            LegacyRequest(),
            () => true,
            CancellationToken.None);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var busyHandle = PublishHandle(admittedEntry, ValidAssociation(), 5);

        var busy = await registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            admittedEntry.SessionId,
            busyHandle,
            "busy-run",
            "scenario",
            "request",
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Busy, busy.Status);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Found,
            registry.ResolveSelectedTargetEvidence(
                TenantId, admittedEntry.SessionId, busyHandle, () => true).Status);
        release.TrySetResult();
        Assert.Equal(SessionExecutionStatus.Completed, (await holder).Status);

        var cancelledHandle = PublishHandle(admittedEntry, ValidAssociation(), 6);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                TenantId,
                admittedEntry.SessionId,
                cancelledHandle,
                "cancelled-run",
                "scenario",
                "request",
                () => true,
                cancellation.Token));
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Found,
            registry.ResolveSelectedTargetEvidence(
                TenantId, admittedEntry.SessionId, cancelledHandle, () => true).Status);
        Assert.Equal(0, admittedSession.AdmittedCallCount);
    }

    [Fact]
    public async Task RunAdmittedSelectedTargetAsync_ConcurrentSameHandleHasOneWinner()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(
            Options(maxConcurrentRuns: 2));
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "admitted-concurrent");
        var handle = PublishHandle(entry, ValidAssociation(), 7);
        var bothAtSecondGate = NewSignal();
        var arrivals = 0;

        Func<bool> NewGate()
        {
            var calls = 0;
            return () =>
            {
                if (Interlocked.Increment(ref calls) == 2)
                {
                    if (Interlocked.Increment(ref arrivals) == 2)
                        bothAtSecondGate.TrySetResult();
                    bothAtSecondGate.Task.GetAwaiter().GetResult();
                }
                return true;
            };
        }

        var first = Task.Run(() => registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            handle,
            "concurrent-first",
            "scenario",
            "request",
            NewGate(),
            CancellationToken.None));
        var second = Task.Run(() => registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            handle,
            "concurrent-second",
            "scenario",
            "request",
            NewGate(),
            CancellationToken.None));

        var results = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Single(results, result =>
            result.Status == SelectedTargetSessionExecutionStatus.Completed);
        Assert.Single(results, result =>
            result.Status == SelectedTargetSessionExecutionStatus.Unknown);
        Assert.Equal(1, session.AdmittedCallCount);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Unknown,
            registry.ResolveSelectedTargetEvidence(
                TenantId, entry.SessionId, handle, () => true).Status);
    }

    [Fact]
    public async Task RunAdmittedSelectedTargetAsync_PreClaimGateLifecycleAndInvalidationRacesDoNotExecute()
    {
        using var gateRegistry = new DynamicGraphCompositionSessionRegistry(Options());
        var gateSession = new ControlledDualSession();
        var gateEntry = await PublishAsync(gateRegistry, gateSession, "admitted-gate-race");
        var gateHandle = PublishHandle(gateEntry, ValidAssociation(), 8);
        var gate = new SequenceGate(true, true, false);
        var denied = await gateRegistry.RunAdmittedSelectedTargetAsync(
            TenantId,
            gateEntry.SessionId,
            gateHandle,
            "gate-race-run",
            "scenario",
            "request",
            gate.Invoke,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.ForbiddenAssembly, denied.Status);
        Assert.Equal(3, gate.CallCount);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Found,
            gateRegistry.ResolveSelectedTargetEvidence(
                TenantId, gateEntry.SessionId, gateHandle, () => true).Status);

        var time = new MutableTimeProvider(DateTimeOffset.Parse("2026-07-25T01:00:00Z"));
        using var expiryRegistry = new DynamicGraphCompositionSessionRegistry(
            Options(ttl: TimeSpan.FromMinutes(1)),
            time);
        var expirySession = new ControlledDualSession();
        var expiryEntry = await PublishAsync(expiryRegistry, expirySession, "admitted-expiry-race");
        var expiryHandle = PublishHandle(expiryEntry, ValidAssociation(), 9);
        var expiryCalls = 0;
        var expired = await expiryRegistry.RunAdmittedSelectedTargetAsync(
            TenantId,
            expiryEntry.SessionId,
            expiryHandle,
            "expiry-race-run",
            "scenario",
            "request",
            () =>
            {
                if (Interlocked.Increment(ref expiryCalls) == 3)
                    time.Advance(TimeSpan.FromSeconds(61));
                return true;
            },
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Expired, expired.Status);
        Assert.Equal(3, Volatile.Read(ref expiryCalls));
        Assert.Equal(0, expirySession.AdmittedCallCount);

        using var replacementRegistry = new DynamicGraphCompositionSessionRegistry(
            Options(maxSessions: 2));
        var replacedSession = new ControlledDualSession();
        var replacementSession = new ControlledDualSession();
        var replacedEntry = await PublishAsync(
            replacementRegistry,
            replacedSession,
            "admitted-replacement-race-original");
        var replacementEntry = await PublishAsync(
            replacementRegistry,
            replacementSession,
            "admitted-replacement-race-new");
        var replacedAssociation = ValidAssociation();
        var replacementAssociation = ValidAssociation();
        Assert.NotSame(replacedAssociation, replacementAssociation);
        var replacementHandle = PublishHandle(replacedEntry, replacedAssociation, 10);
        var replacementEntryHandle = PublishHandle(
            replacementEntry,
            replacementAssociation,
            10);
        Assert.Equal(replacementHandle, replacementEntryHandle, StringComparer.Ordinal);
        var sessions = PrivateField<System.Collections.Concurrent.ConcurrentDictionary<
            string,
            DynamicGraphCompositionSessionRegistry.SessionEntry>>(
                replacementRegistry,
                "_sessions");
        var replacementCalls = 0;
        try
        {
            var replaced = await replacementRegistry.RunAdmittedSelectedTargetAsync(
                TenantId,
                replacedEntry.SessionId,
                replacementHandle,
                "replacement-race-run",
                "scenario",
                "request",
                () =>
                {
                    if (Interlocked.Increment(ref replacementCalls) == 3)
                        sessions[replacedEntry.SessionId] = replacementEntry;
                    return true;
                },
                CancellationToken.None);
            Assert.Equal(SelectedTargetSessionExecutionStatus.Unknown, replaced.Status);
            Assert.Equal(3, Volatile.Read(ref replacementCalls));
            Assert.Equal(0, replacedSession.AdmittedCallCount);
            Assert.Equal(0, replacementSession.AdmittedCallCount);
            Assert.DoesNotContain(replacementHandle, RetiredHandles(replacedEntry));
            Assert.DoesNotContain(replacementHandle, RetiredHandles(replacementEntry));

            sessions[replacedEntry.SessionId] = replacementEntry;
            Assert.Equal(
                SelectedTargetEvidenceResolutionStatus.Found,
                replacementRegistry.ResolveSelectedTargetEvidence(
                    TenantId,
                    replacedEntry.SessionId,
                    replacementHandle,
                    () => true).Status);
            Assert.Same(
                replacementAssociation,
                PrivateField<Dictionary<
                    string,
                    DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>>(
                        replacementEntry,
                        "_selectedTargetEvidence")[replacementHandle]);

            sessions[replacedEntry.SessionId] = replacedEntry;
            Assert.Equal(
                SelectedTargetEvidenceResolutionStatus.Found,
                replacementRegistry.ResolveSelectedTargetEvidence(
                    TenantId,
                    replacedEntry.SessionId,
                    replacementHandle,
                    () => true).Status);
            Assert.Same(
                replacedAssociation,
                PrivateField<Dictionary<
                    string,
                    DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>>(
                        replacedEntry,
                        "_selectedTargetEvidence")[replacementHandle]);
        }
        finally
        {
            sessions[replacedEntry.SessionId] = replacedEntry;
        }

        using var invalidationRegistry = new DynamicGraphCompositionSessionRegistry(Options());
        var invalidationSession = new ControlledDualSession();
        var invalidationEntry = await PublishAsync(
            invalidationRegistry,
            invalidationSession,
            "admitted-invalidation-race");
        var invalidationHandle = PublishHandle(invalidationEntry, ValidAssociation(), 11);
        var invalidationCalls = 0;
        var invalidated = await invalidationRegistry.RunAdmittedSelectedTargetAsync(
            TenantId,
            invalidationEntry.SessionId,
            invalidationHandle,
            "invalidation-race-run",
            "scenario",
            "request",
            () =>
            {
                if (Interlocked.Increment(ref invalidationCalls) == 3)
                    invalidationRegistry.InvalidateAll();
                return true;
            },
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.ForbiddenAssembly, invalidated.Status);
        Assert.Equal(3, Volatile.Read(ref invalidationCalls));
        Assert.Equal(0, invalidationSession.AdmittedCallCount);
        Assert.DoesNotContain(invalidationHandle, RetiredHandles(invalidationEntry));
    }

    [Fact]
    public async Task RunAdmittedSelectedTargetAsync_PostClaimOutcomesAreIrreversibleAndKeepSlotPolicy()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options());
        var session = new ControlledDualSession();
        var entry = await PublishAsync(registry, session, "admitted-post-claim");

        var successHandle = PublishHandle(entry, ValidAssociation(), 11);
        var success = await registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            successHandle,
            "success-run",
            "scenario",
            "request",
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, success.Status);
        AssertHandleConsumed(registry, entry, successHandle);

        var failureHandle = PublishHandle(entry, ValidAssociation(), 12);
        session.AdmittedHandler = (_, _) => Task.FromResult(OperationResult<
            DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
                "Declared admitted failure.",
                "DECLARED_ADMITTED_FAILURE"));
        var failure = await registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            entry.SessionId,
            failureHandle,
            "failure-run",
            "scenario",
            "request",
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, failure.Status);
        Assert.Equal("DECLARED_ADMITTED_FAILURE", failure.Result!.ErrorCode);
        AssertHandleConsumed(registry, entry, failureHandle);

        var cancelledHandle = PublishHandle(entry, ValidAssociation(), 13);
        using var cancellation = new CancellationTokenSource();
        session.AdmittedHandler = (_, token) =>
        {
            cancellation.Cancel();
            return Task.FromCanceled<OperationResult<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>(token);
        };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                TenantId,
                entry.SessionId,
                cancelledHandle,
                "cancel-after-claim",
                "scenario",
                "request",
                () => true,
                cancellation.Token));
        AssertHandleConsumed(registry, entry, cancelledHandle);

        var nonFatalHandle = PublishHandle(entry, ValidAssociation(), 14);
        var nonFatal = new AggregateException(
            "nonfatal-admitted-wrapper",
            new InvalidOperationException("nonfatal-admitted-inner"));
        session.AdmittedHandler = (_, _) => Task.FromException<OperationResult<
            DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>(nonFatal);
        var observedNonFatal = await Assert.ThrowsAsync<AggregateException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                TenantId,
                entry.SessionId,
                nonFatalHandle,
                "nonfatal-after-claim",
                "scenario",
                "request",
                () => true,
                CancellationToken.None));
        Assert.Same(nonFatal, observedNonFatal);
        AssertHandleConsumed(registry, entry, nonFatalHandle);

        var fatalHandle = PublishHandle(entry, ValidAssociation(), 15);
        var fatal = new InvalidOperationException(
            "safe admitted fatal wrapper",
            new OutOfMemoryException("constructed admitted fatal marker"));
        session.AdmittedHandler = (_, _) => Task.FromException<OperationResult<
            DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>(fatal);
        var observedFatal = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            registry.RunAdmittedSelectedTargetAsync(
                TenantId,
                entry.SessionId,
                fatalHandle,
                "fatal-after-claim",
                "scenario",
                "request",
                () => true,
                CancellationToken.None));
        Assert.Same(fatal, observedFatal);
        AssertHandleConsumed(registry, entry, fatalHandle);

        var busy = await registry.RunAsync(
            TenantId,
            entry.SessionId,
            LegacyRequest(),
            () => true,
            CancellationToken.None);
        Assert.Equal(SessionExecutionStatus.Busy, busy.Status);
        Assert.Equal(0, session.LegacyCallCount);
    }

    [Fact]
    public async Task RunAdmittedSelectedTargetAsync_SharesSlotsAndHasNoPostClaimChecks()
    {
        using var registry = new DynamicGraphCompositionSessionRegistry(Options(maxSessions: 2));
        var admittedSession = new ControlledDualSession();
        var otherSession = new ControlledDualSession();
        var admittedEntry = await PublishAsync(registry, admittedSession, "admitted-slot-holder");
        var otherEntry = await PublishAsync(registry, otherSession, "admitted-slot-other");
        var handle = PublishHandle(admittedEntry, ValidAssociation(), 16);
        var started = NewSignal();
        var release = NewSignal();
        admittedSession.AdmittedHandler = async (_, _) =>
        {
            started.TrySetResult();
            await release.Task.ConfigureAwait(false);
            return AdmittedSuccess(ValidAssociation(), "admitted-slot-holder-run");
        };
        var holder = registry.RunAdmittedSelectedTargetAsync(
            TenantId,
            admittedEntry.SessionId,
            handle,
            "admitted-slot-holder-run",
            "scenario",
            "request",
            () => true,
            CancellationToken.None);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var legacyBusy = await registry.RunAsync(
            TenantId,
            otherEntry.SessionId,
            LegacyRequest(),
            () => true,
            CancellationToken.None);
        var rawBusy = await registry.RunSelectedTargetAsync(
            TenantId,
            otherEntry.SessionId,
            SelectedRequest("raw-busy"),
            () => true,
            CancellationToken.None);
        Assert.Equal(SessionExecutionStatus.Busy, legacyBusy.Status);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Busy, rawBusy.Status);
        Assert.Equal(0, otherSession.LegacyCallCount);
        Assert.Equal(0, otherSession.SelectedCallCount);
        release.TrySetResult();
        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, (await holder).Status);

        var noPostCheckHandle = PublishHandle(admittedEntry, ValidAssociation(), 17);
        using var cancellation = new CancellationTokenSource();
        var gateCalls = 0;
        var sessions = PrivateField<System.Collections.Concurrent.ConcurrentDictionary<
            string,
            DynamicGraphCompositionSessionRegistry.SessionEntry>>(
                registry,
                "_sessions");
        admittedSession.AdmittedHandler = (_, token) =>
        {
            Assert.Equal(cancellation.Token, token);
            Assert.True(sessions.TryRemove(admittedEntry.SessionId, out var removedEntry));
            Assert.Same(admittedEntry, removedEntry);
            cancellation.Cancel();
            return Task.FromResult(AdmittedSuccess(
                ValidAssociation(),
                "no-post-check-run"));
        };
        SelectedTargetSessionExecutionResult noPostCheck;
        try
        {
            noPostCheck = await registry.RunAdmittedSelectedTargetAsync(
                TenantId,
                admittedEntry.SessionId,
                noPostCheckHandle,
                "no-post-check-run",
                "scenario",
                "request",
                () =>
                {
                    Interlocked.Increment(ref gateCalls);
                    return true;
                },
                cancellation.Token);
        }
        finally
        {
            sessions[admittedEntry.SessionId] = admittedEntry;
        }
        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, noPostCheck.Status);
        Assert.Equal(3, Volatile.Read(ref gateCalls));
        Assert.True(cancellation.IsCancellationRequested);
        AssertHandleConsumed(registry, admittedEntry, noPostCheckHandle);
    }

    [Fact]
    public async Task Restore_RebuildsDualAndLegacyCapabilitiesWithoutCheckpointMutationOrSensitiveState()
    {
        var options = Options(maxSessions: 2);
        var persistence = new DynamicGraphCompositionSessionPersistence(options, enabled: true);
        var dualProfile = CreateProfile("restore-dual");
        var legacyProfile = CreateProfile("restore-legacy");
        var dualSession = new ControlledDualSession();
        var legacySession = new LegacyOnlySession();
        using (var writer = new DynamicGraphCompositionSessionRegistry(options, persistence))
        {
            await PublishAsync(writer, dualSession, dualProfile);
            await PublishAsync(writer, legacySession, legacyProfile);
        }
        Assert.Same(dualProfile, dualSession.LastCheckpointProfile);
        Assert.Same(legacyProfile, legacySession.LastCheckpointProfile);
        var captured = persistence.PersistedSnapshots;
        Assert.Equal(2, captured.Count);
        Assert.All(
            captured,
            checkpoint =>
            {
                Assert.Equal(
                    "dynamic-graph-composition-session.v2",
                    checkpoint.Schema,
                    StringComparer.Ordinal);
                var capture = Assert.IsType<DynamicGraphLogicalOrchestratorCheckpointCapture>(
                    checkpoint.Capture);
                Assert.Null(checkpoint.AuthenticatedCheckpoint);
                Assert.Equal(checkpoint.Profile.ProfileId, capture.CurrentProfile.ProfileId);
                Assert.Equal(checkpoint.Profile.NodeDefinitions.Length, capture.NodeCount);
                Assert.Equal(checkpoint.Profile.EdgeBindings.Length, capture.EdgeCount);
                Assert.Equal(capture.NodeCount, capture.Topology.Nodes.Count);
                Assert.Equal(capture.EdgeCount, capture.Topology.Edges.Count);
                Assert.Equal(capture.NodeCount, capture.Topology.Order.Count);
                Assert.Equal(
                    checkpoint.Profile.NodeDefinitions
                        .Select(definition => definition.Descriptor.NodeId),
                    capture.Topology.Nodes.Select(node => node.NodeId));
                Assert.Equal(
                    checkpoint.Profile.EdgeBindings
                        .Select(binding => binding.EdgeId)
                        .OrderBy(value => value, StringComparer.Ordinal),
                    capture.Topology.Edges.Select(edge => edge.EdgeId));
                Assert.Equal(
                    capture.Topology.Nodes.Select(node => node.NodeId),
                    capture.Topology.Order.Select(entry => entry.NodeId));
                var topologyValidation = DynamicGraphValidation.Validate(capture.Topology);
                Assert.True(
                    topologyValidation.IsSuccess,
                    $"{topologyValidation.ErrorCode}: {topologyValidation.ErrorMessage}");
            });
        dualProfile.NodeDefinitions[0] = dualProfile.NodeDefinitions[0] with
        {
            LogicalSubPhaseId = "mutated-sub-phase"
        };
        legacyProfile.EdgeBindings[0].LogicalTransitionIds[0] = "mutated-transition";

        AuthenticatePersistedCheckpointsForRestart(persistence);

        var before = persistence.PersistedSnapshots;
        Assert.Equal(2, before.Count);
        Assert.All(
            before,
            checkpoint => Assert.Equal(
                "dynamic-graph-composition-session.v2",
                checkpoint.Schema,
                StringComparer.Ordinal));
        Assert.Equal("3.4", before.Single(item => item.Profile.ProfileId == "restore-dual")
            .Profile.NodeDefinitions[0].LogicalSubPhaseId);
        Assert.Equal(
            "transition-3.4-to-3.5",
            before.Single(item => item.Profile.ProfileId == "restore-legacy")
                .Profile.EdgeBindings[0].LogicalTransitionIds[0]);
        var serializedBefore = JsonSerializer.Serialize(before);
        Assert.DoesNotContain(SensitiveExecutionMarker, serializedBefore, StringComparison.Ordinal);

        ControlledDualSession? restoredDual = null;
        LegacyOnlySession? restoredLegacy = null;
        using var reader = new DynamicGraphCompositionSessionRegistry(options, persistence);
        reader.Restore(profile =>
        {
            if (profile.ProfileId == "restore-dual")
            {
                restoredDual = new ControlledDualSession();
                return (Evidence(profile.ProfileId), restoredDual);
            }
            restoredLegacy = new LegacyOnlySession();
            return (Evidence(profile.ProfileId), restoredLegacy);
        });
        var dualSessionId = before.Single(item => item.Profile.ProfileId == "restore-dual").SessionId;
        var legacySessionId = before.Single(item => item.Profile.ProfileId == "restore-legacy").SessionId;

        var dualExecution = await reader.RunSelectedTargetAsync(
            TenantId,
            dualSessionId,
            SelectedRequest(SensitiveExecutionMarker),
            () => true,
            CancellationToken.None);
        var legacyExecution = await reader.RunSelectedTargetAsync(
            TenantId,
            legacySessionId,
            SelectedRequest(SensitiveExecutionMarker),
            () => true,
            CancellationToken.None);

        Assert.Equal(SelectedTargetSessionExecutionStatus.Completed, dualExecution.Status);
        Assert.Equal(SelectedTargetSessionExecutionStatus.Unavailable, legacyExecution.Status);
        Assert.Equal(1, restoredDual!.SelectedCallCount);
        Assert.Equal(0, restoredLegacy!.LegacyCallCount);
        Assert.Equal(2, persistence.PersistCount);
        Assert.Equal(1, persistence.ReadCount);
        Assert.Equal(0, persistence.DeleteCount);
        var after = persistence.PersistedSnapshots;
        Assert.Equal(serializedBefore, JsonSerializer.Serialize(after), StringComparer.Ordinal);
        Assert.DoesNotContain(
            SensitiveExecutionMarker,
            JsonSerializer.Serialize(after),
            StringComparison.Ordinal);
    }

    private static void AuthenticatePersistedCheckpointsForRestart(
        DynamicGraphCompositionSessionPersistence persistence)
    {
        var sessions = PrivateField<Dictionary<string, PersistedCompositionSession>>(
            persistence,
            "_sessions");
        foreach (var pair in sessions.ToArray())
        {
            var capture = Assert.IsType<DynamicGraphLogicalOrchestratorCheckpointCapture>(
                pair.Value.Capture);
            sessions[pair.Key] = pair.Value with
            {
                Capture = null,
                AuthenticatedCheckpoint = new DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint(
                    capture.Revision,
                    capture.SourceKind,
                    capture.BudgetProof,
                    capture.MutationEvidence,
                    capture.InitialRevisionAdvanced)
            };
        }
    }

    private static DynamicGraphCompositionServerOptions Options(
        int maxSessions = 1,
        TimeSpan? ttl = null,
        int maxConcurrentRuns = 1)
        => new()
        {
            TenantId = TenantId,
            MaxSessions = maxSessions,
            MaxConcurrentCompositions = 1,
            MaxConcurrentRuns = maxConcurrentRuns,
            SessionTtl = ttl ?? TimeSpan.FromMinutes(5)
        };

    private static string ValidHandle(int ordinal)
        => $"dgeh_{ordinal:x32}";

    private static string PublishHandle(
        DynamicGraphCompositionSessionRegistry.SessionEntry entry,
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association,
        int ordinal)
    {
        var reservationAttempt = entry.ReserveSelectedTargetEvidence();
        Assert.Equal(
            DynamicGraphCompositionSessionRegistry.EvidenceReservationStatus.Reserved,
            reservationAttempt.Status);
        var reservation = Assert.IsType<
            DynamicGraphCompositionSessionRegistry.EvidenceReservation>(
                reservationAttempt.Reservation);
        try
        {
            var publication = entry.PublishSelectedTargetEvidence(
                reservation,
                association,
                () => ValidHandle(ordinal));
            Assert.Equal(
                DynamicGraphCompositionSessionRegistry.EvidencePublicationStatus.Published,
                publication.Status);
            return Assert.IsType<SelectedTargetEvidenceHandle>(publication.Handle).HandleId;
        }
        finally
        {
            entry.ReleaseSelectedTargetEvidenceReservation(reservation);
        }
    }

    private static T PrivateField<T>(object owner, string fieldName)
    {
        var field = owner.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<T>(field.GetValue(owner));
    }

    private static IReadOnlyCollection<string> RetiredHandles(
        DynamicGraphCompositionSessionRegistry.SessionEntry entry)
        => PrivateField<HashSet<string>>(
            entry,
            "_retiredSelectedTargetEvidenceHandleIds");

    private static void AssertHandleConsumed(
        DynamicGraphCompositionSessionRegistry registry,
        DynamicGraphCompositionSessionRegistry.SessionEntry entry,
        string handle)
    {
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Unknown,
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                entry.SessionId,
                handle,
                () => true).Status);
        Assert.Contains(handle, RetiredHandles(entry));
    }

    private static OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>
        AdmittedSuccess(
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association,
            string runId)
        => OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Ok(
            new DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt(
                association,
                new DynamicGraphL3SelectedTargetExecutionResult(
                    EdgeId,
                    runId,
                    ["admitted-evidence"]),
                DeterministicRunSpeed(association.Target, inferenceElapsedMs: 23)));

    private static DynamicGraphTargetRunSpeedMeasurement DeterministicRunSpeed(
        DynamicGraphMeasurementTarget target,
        long inferenceElapsedMs)
    {
        var result = DynamicGraphMeasurementCoordinator.MeasureRunSpeed(
            target,
            new DynamicGraphRunSpeedObservation(
                EvaluatedRowCount: 1,
                InferenceElapsedMs: inferenceElapsedMs,
                RoutingElapsedMs: 0,
                CandidateElapsedMsSamples: Array.Empty<long>(),
                BatchElapsedMsSamples: Array.Empty<long>()));
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement
        ValidAssociation()
    {
        var result = DynamicGraphMeasurementCoordinator
            .MeasureEdgeAuthoritySelectionQualityEvidence(
                ValidTarget(),
                ValidAuthorityResult(),
                ValidNoveltyRequest());
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return result.Value!;
    }

    private static DynamicGraphMeasurementTarget ValidTarget()
        => new(
            EdgeId,
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
            DynamicGraphMeasurementMechanismKind.LearningSubject,
            false);

    private static DynamicGraphEdgeAuthorityNoveltyEvaluationRequest ValidNoveltyRequest()
    {
        var paired = Paired();
        return new DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
            paired,
            [
                new DynamicGraphEdgeAuthorityNoveltyTrainingBinding(
                    "train-a",
                    "train-hash",
                    [A],
                    [A])
            ],
            paired.Rows.Select(row => row.RowId).ToArray(),
            Enumerable.Range(0, paired.Rows.Count)
                .Select(index => new DynamicGraphEdgeAuthorityNoveltyRowInput(
                    $"row-{index:D3}",
                    index % 2 == 0 ? B : C,
                    [index % 2 == 0 ? B : C]))
                .ToArray(),
            "fingerprint-normalization-v1",
            "tokenization-v1");
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult Paired()
    {
        const int rowCount = 2;
        const int learnedCorrect = 2;
        const int baselineCorrect = 1;
        var split = new DynamicGraphMeasurementSplitProof(
            ["train-a"],
            ["eval-a"],
            ["train-hash", "eval-hash"],
            0,
            "simhash",
            0.85d);
        var rows = Enumerable.Range(0, rowCount)
            .Select(index =>
            {
                var learnedMatches = index < learnedCorrect;
                var baselineMatches = index < baselineCorrect;
                return new DynamicGraphEdgeAuthorityAblationRowEvidence(
                    $"row-{index:D3}",
                    "eval-a",
                    "eval-hash",
                    "document-000",
                    "repo://eval/a",
                    $"label-{index:D3}",
                    "target-a",
                    learnedMatches ? "target-a" : "target-b",
                    baselineMatches ? "target-a" : "target-b",
                    learnedMatches,
                    baselineMatches);
            })
            .ToArray();
        var learnedAccuracy = (double)learnedCorrect / rowCount;
        var baselineAccuracy = (double)baselineCorrect / rowCount;
        var rawDrop = learnedAccuracy - baselineAccuracy;
        var normalizedDrop = Math.Round(rawDrop, 12, MidpointRounding.AwayFromZero);
        var constructor = Assert.Single(
            typeof(DynamicGraphEdgeAuthorityAblationEvaluationResult)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance),
            candidate => candidate.GetParameters().Length == 12);
        return Assert.IsType<DynamicGraphEdgeAuthorityAblationEvaluationResult>(
            constructor.Invoke(
            [
                EvaluationId,
                EdgeId,
                AuthorityUnitId,
                split,
                rows,
                learnedCorrect,
                learnedAccuracy,
                baselineCorrect,
                baselineAccuracy,
                rawDrop,
                normalizedDrop,
                normalizedDrop >= DynamicGraphAblationMeter.MinimumAccuracyDrop
            ]));
    }

    private static EdgeAuthorityExecutionResult ValidAuthorityResult()
    {
        var candidates = Enumerable.Range(0, 3)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"PREDICATE_{(char)('A' + index)}"],
                0.2d + (index * 0.2d),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();
        var decision = new EdgeAuthorityDecision(
            [("target-c", 0.93d)],
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            candidates
                .OrderByDescending(candidate => candidate.BaselineScore)
                .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .Select(candidate => candidate.TargetSubPhaseId)
                .ToArray());
        var normalizedInput = new EdgeAuthorityInput(
            EdgeId,
            ["edge", "selection", "target"],
            candidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-sub-phase"],
                ["active-edge"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.8d }),
            candidates,
            LeakageSignals);
        var orderReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            EdgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    $"candidate-order-permutation-{index + 1:D2}",
                    CreatePermutation(candidates, index),
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            EdgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentityControlledSignals[index],
                    IdentityControlValues[index],
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);

        return new EdgeAuthorityExecutionResult(
            normalizedInput,
            candidates.Select(CopyCandidate).ToArray(),
            CopyDecision(decision),
            new DynamicGraphEdgeAuthorityEvidence(
                EdgeId,
                AuthorityUnitId,
                "L2",
                false,
                "selection-target-baseline-unit",
                "candidate_generator",
                LeakageSignals,
                "selection-target-source-proof",
                "selection-target-cutover-proof",
                "selection-target-ablation-proof"),
            orderReport,
            identityReport,
            0.93d,
            ["evidence"],
            []);
    }

    private static EdgeCandidate[] CreatePermutation(
        IReadOnlyList<EdgeCandidate> candidates,
        int index)
    {
        var source = candidates.Select(CopyCandidate).ToArray();
        var offset = index switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => -1,
            4 => -2,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
        if (offset < 0)
            return Rotate(source.Reverse().ToArray(), -offset - 1);

        return Rotate(source, offset);
    }

    private static EdgeCandidate[] Rotate(IReadOnlyList<EdgeCandidate> source, int offset)
        => Enumerable.Range(0, source.Count)
            .Select(index => CopyCandidate(source[(index + offset) % source.Count]))
            .ToArray();

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static EdgeAuthorityDecision CopyDecision(EdgeAuthorityDecision source)
        => new(
            source.OrderedTargets.ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private static async Task<DynamicGraphCompositionSessionRegistry.SessionEntry> PublishAsync(
        DynamicGraphCompositionSessionRegistry registry,
        IDynamicGraphLogicalOrchestratorRunSession session,
        string profileId)
        => await PublishAsync(registry, session, CreateProfile(profileId));

    private static async Task<DynamicGraphCompositionSessionRegistry.SessionEntry> PublishAsync(
        DynamicGraphCompositionSessionRegistry registry,
        IDynamicGraphLogicalOrchestratorRunSession session,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
    {
        var created = await registry.CreateAsync(
            TenantId,
            $"idempotency-{profile.ProfileId}",
            CanonicalDigest,
            profile,
            _ => Task.FromResult((Evidence(profile.ProfileId), session)),
            () => true,
            CancellationToken.None);
        Assert.Equal(SessionCreationStatus.Created, created.Status);
        return Assert.IsType<DynamicGraphCompositionSessionRegistry.SessionEntry>(created.Session);
    }

    private static CompositionEvidenceHttpResponse Evidence(string profileId)
        => new(profileId, 1, "LogicalRegistryProjection", 2, 1, true);

    private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfile(string profileId)
        => new(
            profileId,
            TenantId,
            "domain-a",
            "context-a",
            "signal-a",
            "source-a",
            "trace-a",
            [
                new DynamicGraphLogicalOrchestratorNodeDefinition(
                    "3.4",
                    new DynamicGraphNodeDescriptor(
                        "3.4",
                        "domain-a",
                        "System.String",
                        "System.String",
                        false,
                        false,
                        false,
                        "baseline_candidate_generator",
                        null,
                        null)),
                new DynamicGraphLogicalOrchestratorNodeDefinition(
                    "3.5",
                    new DynamicGraphNodeDescriptor(
                        "3.5",
                        "domain-a",
                        "System.String",
                        "System.String",
                        false,
                        false,
                        false,
                        "baseline_candidate_generator",
                        null,
                        null))
            ],
            [
                new DynamicGraphRegistryEdgeBinding(
                    "edge-3.4-to-3.5",
                    ["transition-3.4-to-3.5"],
                    GraphEdgeKind.FanOut,
                    false,
                    "authority-3.4",
                    "L2",
                    false,
                    "baseline-edge-3.4",
                    "candidate_generator",
                    ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
                    "source-proof",
                    "cutover-proof",
                    "ablation-proof")
            ],
            new TopologyBuildBudget(2, 2, 2, 2));

    private static DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest SelectedRequest(
        string runId = "selected-run")
        => new(runId, "selected-scenario", "selected user request", null, null, null);

    private static DynamicGraphLogicalOrchestratorRunRequest LegacyRequest()
        => new("legacy user request");

    private static OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>
        SelectedSuccess()
        => OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Ok(null!);

    private static OperationResult<DynamicGraphLogicalOrchestratorRunResult> LegacySuccess()
        => OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Ok(
            new DynamicGraphLogicalOrchestratorRunResult(
                "legacy-scenario",
                "legacy-flow",
                "PASS",
                "legacy result",
                new Dictionary<string, string>(StringComparer.Ordinal),
                []));

    private static TaskCompletionSource NewSignal()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private sealed class ControlledDualSession
        : IDynamicGraphLogicalOrchestratorRunSession,
          IDynamicGraphLogicalOrchestratorCompositionSession,
          IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession,
          IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession,
          IDynamicGraphLogicalOrchestratorCheckpointSession
    {
        private readonly CheckpointFixture _checkpoint = new();
        private int _legacyCallCount;
        private int _selectedCallCount;
        private int _admittedCallCount;

        internal Func<DynamicGraphLogicalOrchestratorRunRequest, CancellationToken,
            Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>>> LegacyHandler
        { get; set; } = (_, _) => Task.FromResult(LegacySuccess());

        internal Func<DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest,
            CancellationToken,
            Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>>
            SelectedHandler { get; set; } = (_, _) => Task.FromResult(SelectedSuccess());

        internal Func<DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest,
            CancellationToken,
            Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>>
            AdmittedHandler { get; set; } = (request, _) => Task.FromResult(
                AdmittedSuccess(request.Association!, request.RunId));

        internal int LegacyCallCount => Volatile.Read(ref _legacyCallCount);

        internal int SelectedCallCount => Volatile.Read(ref _selectedCallCount);

        internal int AdmittedCallCount => Volatile.Read(ref _admittedCallCount);

        internal DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest?
            LastSelectedRequest { get; private set; }

        internal CancellationToken LastSelectedToken { get; private set; }

        internal DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest?
            LastAdmittedRequest { get; private set; }

        internal CancellationToken LastAdmittedToken { get; private set; }

        internal DynamicGraphLogicalOrchestratorBootstrapProfile? LastCheckpointProfile
            => _checkpoint.LastExpectedProfile;

        public DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor
            => _checkpoint.Descriptor;

        public OperationResult<DynamicGraphLogicalOrchestratorSessionResult> ReadCurrent()
            => _checkpoint.ReadCurrent();

        public Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
            DynamicGraphLogicalOrchestratorRunRequest request,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _legacyCallCount);
            return LegacyHandler(request, cancellationToken);
        }

        Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
            IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession.RunAsync(
                DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest request,
                CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _selectedCallCount);
            LastSelectedRequest = request;
            LastSelectedToken = cancellationToken;
            return SelectedHandler(request, cancellationToken);
        }

        public Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
            RunAdmittedAsync(
                DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest request,
                CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _admittedCallCount);
            LastAdmittedRequest = request;
            LastAdmittedToken = cancellationToken;
            return AdmittedHandler(request, cancellationToken);
        }

        public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            CaptureCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile)
            => _checkpoint.Capture(expectedCurrentProfile);

        public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            RestoreCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile,
                DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint)
            => _checkpoint.Restore(expectedCurrentProfile, checkpoint);
    }

    private sealed class LegacyOnlySession :
        IDynamicGraphLogicalOrchestratorRunSession,
        IDynamicGraphLogicalOrchestratorCompositionSession,
        IDynamicGraphLogicalOrchestratorCheckpointSession
    {
        private readonly CheckpointFixture _checkpoint = new();
        private int _legacyCallCount;

        internal int LegacyCallCount => Volatile.Read(ref _legacyCallCount);

        internal DynamicGraphLogicalOrchestratorBootstrapProfile? LastCheckpointProfile
            => _checkpoint.LastExpectedProfile;

        public DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor
            => _checkpoint.Descriptor;

        public OperationResult<DynamicGraphLogicalOrchestratorSessionResult> ReadCurrent()
            => _checkpoint.ReadCurrent();

        public Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
            DynamicGraphLogicalOrchestratorRunRequest request,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _legacyCallCount);
            return Task.FromResult(LegacySuccess());
        }

        public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            CaptureCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile)
            => _checkpoint.Capture(expectedCurrentProfile);

        public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            RestoreCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile,
                DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint)
            => _checkpoint.Restore(expectedCurrentProfile, checkpoint);
    }

    private sealed class CheckpointFixture
    {
        private DynamicGraphLogicalOrchestratorBootstrapProfile? _profile;
        private DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint? _checkpoint;

        internal DynamicGraphLogicalOrchestratorBootstrapProfile? LastExpectedProfile
            { get; private set; }

        internal DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor
        {
            get
            {
                var profile = Assert.IsType<DynamicGraphLogicalOrchestratorBootstrapProfile>(
                    _profile);
                return new DynamicGraphLogicalOrchestratorSessionDescriptor(
                    profile.ProfileId,
                    profile.TenantId,
                    profile.DomainTag,
                    profile.ContextId);
            }
        }

        internal OperationResult<DynamicGraphLogicalOrchestratorSessionResult> ReadCurrent()
        {
            var profile = Assert.IsType<DynamicGraphLogicalOrchestratorBootstrapProfile>(_profile);
            var checkpoint = Assert.IsType<DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint>(
                _checkpoint);
            return OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Ok(
                new DynamicGraphLogicalOrchestratorSessionResult(
                    Descriptor,
                    checkpoint.Revision,
                    profile.SignalId,
                    checkpoint.SourceKind,
                    profile.NodeDefinitions.Length,
                    profile.EdgeBindings.Length,
                    checkpoint.InitialRevisionAdvanced));
        }

        internal OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture> Capture(
            DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile)
        {
            LastExpectedProfile = expectedCurrentProfile;
            _profile = expectedCurrentProfile;
            _checkpoint ??= CreateAuthenticatedCheckpoint(expectedCurrentProfile);
            return CaptureDeterministicCheckpoint(expectedCurrentProfile, _checkpoint);
        }

        internal OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture> Restore(
            DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile,
            DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint)
        {
            LastExpectedProfile = expectedCurrentProfile;
            _profile = expectedCurrentProfile;
            _checkpoint = checkpoint;
            return CaptureDeterministicCheckpoint(expectedCurrentProfile, checkpoint);
        }

        private static DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint
            CreateAuthenticatedCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile profile)
        {
            var capture = Assert.IsType<DynamicGraphLogicalOrchestratorCheckpointCapture>(
                CaptureDeterministicCheckpoint(profile, null).Value);
            return new DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint(
                capture.Revision,
                capture.SourceKind,
                capture.BudgetProof,
                capture.MutationEvidence,
                capture.InitialRevisionAdvanced);
        }

        private static OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            CaptureDeterministicCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile,
                DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint? checkpoint)
        {
            var topology = CreateTopology(expectedCurrentProfile);
            var budgetProof = checkpoint?.BudgetProof ?? new TopologyBudgetProof(
                expectedCurrentProfile.Budget.MaxNodes,
                topology.Nodes.Count,
                expectedCurrentProfile.Budget.MaxEdges,
                topology.Edges.Count,
                expectedCurrentProfile.Budget.MaxDepth,
                ActualDepth: 1,
                expectedCurrentProfile.Budget.MaxParallelBranches,
                ActualParallelBranches: 1,
                WithinBudget: true,
                ExceededDimensions: []);
            var mutationEvidence = checkpoint?.MutationEvidence ?? new TopologyMutationEvidence(
                expectedCurrentProfile.SignalId,
                DynamicGraphBuildSourceKind.LogicalRegistryProjection,
                expectedCurrentProfile.SourceRef,
                expectedCurrentProfile.TraceRef,
                expectedCurrentProfile.NodeDefinitions
                    .Select(definition => definition.Descriptor.NodeId)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                [],
                [],
                expectedCurrentProfile.EdgeBindings
                    .Select(binding => binding.EdgeId)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                [],
                [],
                [],
                MutationApplied: true);
            return OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>.Ok(
                new DynamicGraphLogicalOrchestratorCheckpointCapture(
                    expectedCurrentProfile,
                    topology,
                    checkpoint?.Revision ?? 1,
                    expectedCurrentProfile.SignalId,
                    checkpoint?.SourceKind
                        ?? DynamicGraphBuildSourceKind.LogicalRegistryProjection,
                    expectedCurrentProfile.SourceRef,
                    expectedCurrentProfile.TraceRef,
                    expectedCurrentProfile.NodeDefinitions.Length,
                    expectedCurrentProfile.EdgeBindings.Length,
                    budgetProof,
                    mutationEvidence,
                    checkpoint?.InitialRevisionAdvanced ?? true));
        }

        private static DynamicGraphTopology CreateTopology(
            DynamicGraphLogicalOrchestratorBootstrapProfile profile)
        {
            var nodes = profile.NodeDefinitions
                .Select(definition => (IDynamicGraphNode)new CheckpointNode(
                    definition.Descriptor.NodeId,
                    definition.Descriptor with { }))
                .ToArray();
            Assert.Equal(2, nodes.Length);
            var edges = profile.EdgeBindings
                .Select(binding => (IDynamicGraphEdge)new CheckpointEdge(
                    binding.EdgeId,
                    nodes[0].NodeId,
                    [nodes[1].NodeId],
                    binding.Kind,
                    binding.ParallelEligible,
                    binding.AuthorityUnitId,
                    binding.AuthorityUnitLevel,
                    binding.DeterministicFinalAuthority,
                    binding.BaselineUnitId,
                    binding.BaselineRole,
                    binding.ExcludedLeakageSignals.ToArray(),
                    binding.SourceProof,
                    binding.CutoverProof,
                    binding.AblationProofRef))
                .OrderBy(edge => edge.EdgeId, StringComparer.Ordinal)
                .ToArray();
            var order = nodes
                .Select((node, position) => new DynamicGraphNodeOrder(
                    $"order-{node.NodeId}",
                    node.NodeId,
                    profile.TenantId,
                    profile.DomainTag,
                    profile.ContextId,
                    position))
                .ToArray();
            var topology = new DynamicGraphTopology(
                profile.TenantId,
                profile.DomainTag,
                profile.ContextId,
                nodes,
                edges,
                order);
            var validation = DynamicGraphValidation.Validate(topology);
            Assert.True(validation.IsSuccess, $"{validation.ErrorCode}: {validation.ErrorMessage}");
            return topology;
        }
    }

    private sealed record CheckpointNode(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;

    private sealed record CheckpointEdge(
        string EdgeId,
        string FromNodeId,
        IReadOnlyList<string> ToNodeIds,
        GraphEdgeKind Kind,
        bool ParallelEligible,
        string AuthorityUnitId,
        string AuthorityUnitLevel,
        bool DeterministicFinalAuthority,
        string? BaselineUnitId,
        string BaselineRole,
        IReadOnlyList<string> ExcludedLeakageSignals,
        string SourceProof,
        string CutoverProof,
        string AblationProofRef) : IDynamicGraphEdge;

    private sealed class SequenceGate(params bool[] values)
    {
        private int _callCount;

        internal int CallCount => Volatile.Read(ref _callCount);

        internal bool Invoke()
        {
            var index = Interlocked.Increment(ref _callCount) - 1;
            return values[Math.Min(index, values.Length - 1)];
        }
    }

    private sealed class MutableTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        internal void Advance(TimeSpan duration)
        {
            _now = _now.Add(duration);
        }
    }
}
