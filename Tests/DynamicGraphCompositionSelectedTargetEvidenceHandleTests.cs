// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

extern alias productiondgc;

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;
using LlmFromScratch.DynamicGraphCompositionLiveLocalServer;
using OrchestratorModel.Logical;
using SemanticCompilerAdapter;
using ProductionPersistence = productiondgc::LlmFromScratch.DynamicGraphCompositionLiveLocalServer.DynamicGraphCompositionSessionPersistence;
using ProductionServerOptions = productiondgc::LlmFromScratch.DynamicGraphCompositionLiveLocalServer.DynamicGraphCompositionServerOptions;
using ProductionCompositionEvidence = productiondgc::LlmFromScratch.DynamicGraphCompositionLiveLocalServer.CompositionEvidenceHttpResponse;

public sealed class DynamicGraphCompositionSelectedTargetEvidenceHandleTests
{
    private const string TenantId = "tenant-a";
    private const string EdgeId = "registry-selected-target";
    private const string AuthorityUnitId = "registry-selected-authority";
    private const string EvaluationId = "registry-selected-evaluation";
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
    public void InternalSurface_IsExactAndPublicConstructorsRemainUnchanged()
    {
        var registryType = typeof(DynamicGraphCompositionSessionRegistry);
        var publicConstructors = registryType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .OrderBy(constructor => constructor.GetParameters().Length)
            .ToArray();

        Assert.Equal(2, publicConstructors.Length);
        Assert.Equal(
            [typeof(DynamicGraphCompositionServerOptions), typeof(TimeProvider)],
            publicConstructors[0].GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                typeof(DynamicGraphCompositionServerOptions),
                typeof(DynamicGraphCompositionSessionPersistence),
                typeof(TimeProvider)
            ],
            publicConstructors[1].GetParameters().Select(parameter => parameter.ParameterType));
        Assert.False(publicConstructors[0].GetParameters()[0].HasDefaultValue);
        Assert.True(publicConstructors[0].GetParameters()[1].HasDefaultValue);
        Assert.False(publicConstructors[1].GetParameters()[0].HasDefaultValue);
        Assert.False(publicConstructors[1].GetParameters()[1].HasDefaultValue);
        Assert.True(publicConstructors[1].GetParameters()[2].HasDefaultValue);

        var internalConstructor = Assert.Single(
            registryType.GetConstructors(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            constructor => constructor.IsAssembly);
        Assert.Equal(
            [
                typeof(DynamicGraphCompositionServerOptions),
                typeof(DynamicGraphCompositionSessionPersistence),
                typeof(TimeProvider),
                typeof(IDynamicGraphSelectedTargetEvidenceInvoker),
                typeof(Func<string>)
            ],
            internalConstructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.All(internalConstructor.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));

        var admit = Assert.Single(
            registryType.GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => method.Name == "AdmitSelectedTargetEvidence");
        Assert.True(admit.IsAssembly);
        Assert.Equal(typeof(SelectedTargetEvidenceAdmissionResult), admit.ReturnType);
        Assert.Equal(
            [
                typeof(string),
                typeof(string),
                typeof(DynamicGraphMeasurementTarget),
                typeof(EdgeAuthorityExecutionResult),
                typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequest),
                typeof(Func<bool>),
                typeof(CancellationToken)
            ],
            admit.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                "tenantId",
                "sessionId",
                "target",
                "authorityResult",
                "noveltyRequest",
                "executionAllowed",
                "cancellationToken"
            ],
            admit.GetParameters().Select(parameter => parameter.Name));
        Assert.All(admit.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));

        var resolve = Assert.Single(
            registryType.GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => method.Name == "ResolveSelectedTargetEvidence");
        Assert.True(resolve.IsAssembly);
        Assert.Equal(typeof(SelectedTargetEvidenceResolutionResult), resolve.ReturnType);
        Assert.Equal(
            [typeof(string), typeof(string), typeof(string), typeof(Func<bool>)],
            resolve.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["tenantId", "sessionId", "handle", "executionAllowed"],
            resolve.GetParameters().Select(parameter => parameter.Name));
        Assert.All(resolve.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));

        var claim = Assert.Single(
            registryType.GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => method.Name == "ClaimSelectedTargetEvidence");
        Assert.True(claim.IsAssembly);
        Assert.Equal(typeof(SelectedTargetEvidenceResolutionResult), claim.ReturnType);
        Assert.Equal(
            [typeof(string), typeof(string), typeof(string), typeof(Func<bool>)],
            claim.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["tenantId", "sessionId", "handle", "executionAllowed"],
            claim.GetParameters().Select(parameter => parameter.Name));
        Assert.All(claim.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));

        var admittedRun = Assert.Single(
            registryType.GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => method.Name == "RunAdmittedSelectedTargetAsync");
        Assert.True(admittedRun.IsAssembly);
        Assert.Equal(typeof(Task<SelectedTargetSessionExecutionResult>), admittedRun.ReturnType);
        Assert.Equal(
            [
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(Func<bool>),
                typeof(CancellationToken)
            ],
            admittedRun.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                "tenantId",
                "sessionId",
                "handle",
                "runId",
                "scenarioId",
                "userRequest",
                "executionAllowed",
                "cancellationToken"
            ],
            admittedRun.GetParameters().Select(parameter => parameter.Name));
        Assert.All(admittedRun.GetParameters(), parameter => Assert.False(
            parameter.HasDefaultValue));

        var entryType = typeof(DynamicGraphCompositionSessionRegistry.SessionEntry);
        var supportsAdmitted = Assert.IsAssignableFrom<PropertyInfo>(entryType.GetProperty(
            "SupportsAdmittedSelectedTargetExecution",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.True(supportsAdmitted.GetMethod!.IsAssembly);
        Assert.Equal(typeof(bool), supportsAdmitted.PropertyType);
        var executeAdmitted = Assert.Single(
            entryType.GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => method.Name == "ExecuteAdmittedSelectedTargetAsync");
        Assert.True(executeAdmitted.IsAssembly);
        Assert.Equal(
            typeof(Task<OperationResult<
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>),
            executeAdmitted.ReturnType);
        Assert.Equal(
            [
                typeof(DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest),
                typeof(CancellationToken)
            ],
            executeAdmitted.GetParameters().Select(parameter => parameter.ParameterType));
        var entryClaim = Assert.Single(
            entryType.GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => method.Name == "ClaimSelectedTargetEvidence");
        Assert.True(entryClaim.IsAssembly);
        Assert.Equal(
            typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement),
            entryClaim.ReturnType);
        Assert.Equal([typeof(string)], entryClaim.GetParameters().Select(
            parameter => parameter.ParameterType));
        Assert.Equal(
            ["CreateAsync", "Dispose", "Get", "RunAsync", "RunSelectedTargetAsync"],
            registryType.GetMethods(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName)
                .Select(method => method.Name)
                .OrderBy(name => name, StringComparer.Ordinal));

        Assert.False(typeof(IDynamicGraphSelectedTargetEvidenceInvoker).IsPublic);
        var invoke = Assert.Single(typeof(IDynamicGraphSelectedTargetEvidenceInvoker).GetMethods());
        Assert.Equal(
            typeof(OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>),
            invoke.ReturnType);
        Assert.Equal(
            [
                typeof(DynamicGraphMeasurementTarget),
                typeof(EdgeAuthorityExecutionResult),
                typeof(DynamicGraphEdgeAuthorityNoveltyEvaluationRequest)
            ],
            invoke.GetParameters().Select(parameter => parameter.ParameterType));

        Assert.False(typeof(SelectedTargetEvidenceHandle).IsPublic);
        var handleConstructor = Assert.Single(typeof(SelectedTargetEvidenceHandle).GetConstructors());
        Assert.Equal(
            [typeof(string), typeof(DateTimeOffset)],
            handleConstructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["Completed", "Expired", "Unknown", "CapacityExceeded", "ForbiddenAssembly"],
            Enum.GetNames<SelectedTargetEvidenceAdmissionStatus>());
        Assert.Equal(
            ["Found", "Expired", "Unknown", "ForbiddenAssembly"],
            Enum.GetNames<SelectedTargetEvidenceResolutionStatus>());

        var admissionConstructor = Assert.Single(
            typeof(SelectedTargetEvidenceAdmissionResult).GetConstructors());
        Assert.Equal(
            [
                typeof(SelectedTargetEvidenceAdmissionStatus),
                typeof(OperationResult<SelectedTargetEvidenceHandle>)
            ],
            admissionConstructor.GetParameters().Select(parameter => parameter.ParameterType));
        var resolutionConstructor = Assert.Single(
            typeof(SelectedTargetEvidenceResolutionResult).GetConstructors());
        Assert.Equal(
            [
                typeof(SelectedTargetEvidenceResolutionStatus),
                typeof(DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement)
            ],
            resolutionConstructor.GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public async Task Admit_EntersLifecycleThenValidatesArgumentsInExactOrderAndForwardsNullableInputs()
    {
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) =>
                OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
                    .Fail("forwarded", "FORWARDED")
        };
        using var registry = CreateRegistry(invoker);
        var sessionId = await PublishSessionAsync(registry, "forwarding");

        var tenantException = Assert.Throws<ArgumentException>(() => registry.AdmitSelectedTargetEvidence(
            " ",
            " ",
            null,
            null,
            null,
            null!,
            CancellationToken.None));
        Assert.Equal("tenantId", tenantException.ParamName);

        var sessionException = Assert.Throws<ArgumentException>(() => registry.AdmitSelectedTargetEvidence(
            TenantId,
            " ",
            null,
            null,
            null,
            null!,
            CancellationToken.None));
        Assert.Equal("sessionId", sessionException.ParamName);

        var gateException = Assert.Throws<ArgumentNullException>(() => registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            null!,
            CancellationToken.None));
        Assert.Equal("executionAllowed", gateException.ParamName);
        Assert.Equal(0, invoker.CallCount);

        var target = new DynamicGraphMeasurementTarget(
            "forwarded-edge",
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
            DynamicGraphMeasurementMechanismKind.LearningSubject,
            false);
        var first = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            target,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, first.Status);
        Assert.NotNull(first.Result);
        Assert.False(first.Result.IsSuccess);
        Assert.Same(target, invoker.LastTarget);
        Assert.Null(invoker.LastAuthorityResult);
        Assert.Null(invoker.LastNoveltyRequest);
        Assert.Equal(1, invoker.CallCount);

        var second = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, second.Status);
        Assert.NotNull(second.Result);
        Assert.False(second.Result.IsSuccess);
        Assert.Null(invoker.LastTarget);
        Assert.Null(invoker.LastAuthorityResult);
        Assert.Null(invoker.LastNoveltyRequest);
        Assert.Equal(2, invoker.CallCount);

        registry.Dispose();
        Assert.Throws<ObjectDisposedException>(() => registry.AdmitSelectedTargetEvidence(
            " ",
            " ",
            null,
            null,
            null,
            null!,
            CancellationToken.None));
        Assert.Equal(2, invoker.CallCount);
    }

    [Fact]
    public async Task Admit_SuccessInvokesOnceReturnsExactHandleShapeAndSessionExpiry()
    {
        const string handleId = "dgeh_0123456789abcdef0123456789abcdef";
        var now = new DateTimeOffset(2026, 7, 25, 10, 30, 0, TimeSpan.Zero);
        var association = ValidAssociation();
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) =>
                OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
                    .Ok(association)
        };
        using var registry = CreateRegistry(invoker, () => handleId, new FixedTimeProvider(now));
        var session = await PublishSessionEntryAsync(registry, "success");

        var result = registry.AdmitSelectedTargetEvidence(
            TenantId,
            session.SessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);

        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, result.Status);
        Assert.NotNull(result.Result);
        Assert.True(result.Result.IsSuccess, result.Result.ErrorMessage);
        Assert.NotNull(result.Result.Value);
        Assert.Equal(handleId, result.Result.Value.HandleId);
        Assert.Matches("^dgeh_[0-9a-f]{32}$", result.Result.Value.HandleId);
        Assert.Equal(session.ExpiresAtUtc, result.Result.Value.ExpiresAtUtc);
        Assert.Equal(now.AddMinutes(15), result.Result.Value.ExpiresAtUtc);
        Assert.Equal(1, invoker.CallCount);

        var defaultOptions = new DynamicGraphCompositionServerOptions
        {
            TenantId = TenantId,
            MaxSessions = 1,
            MaxConcurrentCompositions = 1,
            MaxConcurrentRuns = 1,
            SessionTtl = TimeSpan.FromMinutes(15)
        };
        using var defaultRegistry = new DynamicGraphCompositionSessionRegistry(
            defaultOptions,
            new FixedTimeProvider(now));
        var defaultSession = await PublishSessionEntryAsync(
            defaultRegistry,
            "default-canonical-coordinator");
        var target = ValidTarget();
        var defaultResult = defaultRegistry.AdmitSelectedTargetEvidence(
            TenantId,
            defaultSession.SessionId,
            target,
            ValidAuthorityResult(),
            ValidNoveltyRequest(),
            () => true,
            CancellationToken.None);

        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, defaultResult.Status);
        Assert.NotNull(defaultResult.Result);
        Assert.True(defaultResult.Result.IsSuccess, defaultResult.Result.ErrorMessage);
        Assert.NotNull(defaultResult.Result.Value);
        Assert.Matches("^dgeh_[0-9a-f]{32}$", defaultResult.Result.Value.HandleId);
        Assert.Equal(defaultSession.ExpiresAtUtc, defaultResult.Result.Value.ExpiresAtUtc);
        var defaultResolution = defaultRegistry.ResolveSelectedTargetEvidence(
            TenantId,
            defaultSession.SessionId,
            defaultResult.Result.Value.HandleId,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, defaultResolution.Status);
        Assert.NotNull(defaultResolution.Evidence);
        Assert.Equal(target.TargetId, defaultResolution.Evidence.Target.TargetId);
        Assert.Equal(
            target.TargetId,
            defaultResolution.Evidence.EdgeAuthoritySelectionQualityEvidence
                .EdgeAuthorityControl.AuthorityEvidence.EdgeId);
    }

    [Fact]
    public async Task Admit_StoresOwnedAssociationAndResolveReturnsFreshDeepCopies()
    {
        const string handleId = "dgeh_11111111111111111111111111111111";
        var association = ValidAssociation();
        var originalRequestedRows = RequestedRowIdsBackingArray(association);
        var admittedFirstRow = originalRequestedRows[0];
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) =>
                OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
                    .Ok(association)
        };
        using var registry = CreateRegistry(invoker, () => handleId);
        var sessionId = await PublishSessionAsync(registry, "deep-copy");

        var admission = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, admission.Status);
        Assert.True(admission.Result!.IsSuccess, admission.Result.ErrorMessage);

        originalRequestedRows[0] = "mutated-original-row";
        var first = registry.ResolveSelectedTargetEvidence(
            TenantId,
            sessionId,
            handleId,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, first.Status);
        Assert.NotNull(first.Evidence);
        Assert.NotSame(association, first.Evidence);
        Assert.Equal(admittedFirstRow, first.Evidence.EdgeAuthoritySelectionQualityEvidence
            .NoveltyEvaluation.RequestedRowIds[0]);

        var firstRequestedRows = RequestedRowIdsBackingArray(first.Evidence);
        firstRequestedRows[0] = "mutated-first-resolution-row";
        var second = registry.ResolveSelectedTargetEvidence(
            TenantId,
            sessionId,
            handleId,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, second.Status);
        Assert.NotNull(second.Evidence);
        var secondRequestedRows = RequestedRowIdsBackingArray(second.Evidence);

        Assert.Equal(admittedFirstRow, secondRequestedRows[0]);
        Assert.NotSame(first.Evidence, second.Evidence);
        Assert.NotSame(originalRequestedRows, firstRequestedRows);
        Assert.NotSame(originalRequestedRows, secondRequestedRows);
        Assert.NotSame(firstRequestedRows, secondRequestedRows);
        Assert.NotSame(
            first.Evidence.EdgeAuthoritySelectionQualityEvidence,
            second.Evidence.EdgeAuthoritySelectionQualityEvidence);
        Assert.NotSame(
            first.Evidence.EdgeAuthoritySelectionQualityEvidence.NoveltyEvaluation,
            second.Evidence.EdgeAuthoritySelectionQualityEvidence.NoveltyEvaluation);
    }

    [Fact]
    public async Task Admit_InvokerFailurePreservesExactCodeAndMessageWithoutPublishing()
    {
        const string errorCode = "SELECTED_TARGET_TEST_FAILURE";
        const string errorMessage = "The selected-target evidence test invoker failed.";
        var association = ValidAssociation();
        var handleFactoryCalls = 0;
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) =>
                OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
                    .Fail(errorMessage, errorCode)
        };
        using var registry = CreateRegistry(
            invoker,
            () => $"dgeh_{++handleFactoryCalls:x32}");
        var sessionId = await PublishSessionAsync(registry, "failure");

        var failure = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, failure.Status);
        Assert.NotNull(failure.Result);
        Assert.False(failure.Result.IsSuccess);
        Assert.Equal(errorCode, failure.Result.ErrorCode);
        Assert.Equal(errorMessage, failure.Result.ErrorMessage);
        Assert.Equal(0, handleFactoryCalls);

        invoker.Handler = (_, _, _) =>
            OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
                .Ok(association);
        for (var index = 0; index < 16; index++)
        {
            var admitted = registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None);
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, admitted.Status);
            Assert.True(admitted.Result!.IsSuccess, admitted.Result.ErrorMessage);
        }

        var capacity = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, capacity.Status);
        Assert.Null(capacity.Result);
        Assert.Equal(16, handleFactoryCalls);
        Assert.Equal(17, invoker.CallCount);
    }

    [Fact]
    public async Task Admit_NullAndSuccessfulNullInvokerShapesReturnExactRequiredFailure()
    {
        const string requiredCode = "HOST_SELECTED_TARGET_EVIDENCE_ADMISSION_RESULT_REQUIRED";
        const string requiredMessage = "A selected-target evidence admission result is required.";
        const string handleId = "dgeh_22222222222222222222222222222222";
        var handleFactoryCalls = 0;
        var invoker = new RecordingEvidenceInvoker { Handler = (_, _, _) => null };
        using var registry = CreateRegistry(
            invoker,
            () =>
            {
                handleFactoryCalls++;
                return handleId;
            });
        var sessionId = await PublishSessionAsync(registry, "null-shapes");

        var nullEnvelope = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        AssertRequiredFailure(nullEnvelope, requiredCode, requiredMessage);

        invoker.Handler = (_, _, _) =>
            OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
                .Ok(null!);
        var successfulNull = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        AssertRequiredFailure(successfulNull, requiredCode, requiredMessage);
        Assert.Equal(0, handleFactoryCalls);

        invoker.Handler = (_, _, _) =>
            OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
                .Ok(ValidAssociation());
        var recovery = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, recovery.Status);
        Assert.True(recovery.Result!.IsSuccess, recovery.Result.ErrorMessage);
        Assert.Equal(handleId, recovery.Result.Value!.HandleId);
        Assert.Equal(1, handleFactoryCalls);
        Assert.Equal(3, invoker.CallCount);
    }

    [Fact]
    public async Task Admit_InitialForbiddenExpiredUnknownAndCapacityPrecedeCancellationAndInvocation()
    {
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        var lifecycleInvoker = new RecordingEvidenceInvoker();
        using (var forbiddenRegistry = CreateRegistry(lifecycleInvoker))
        {
            var forbidden = forbiddenRegistry.AdmitSelectedTargetEvidence(
                TenantId,
                "missing-forbidden-session",
                null,
                null,
                null,
                () => false,
                cancelled.Token);
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.ForbiddenAssembly, forbidden.Status);
            Assert.Null(forbidden.Result);
        }

        var time = new MutableTimeProvider(
            new DateTimeOffset(2026, 7, 25, 11, 0, 0, TimeSpan.Zero));
        using (var expiredRegistry = CreateRegistry(lifecycleInvoker, timeProvider: time))
        {
            var expiredSessionId = await PublishSessionAsync(expiredRegistry, "ordering-expired");
            time.Advance(TimeSpan.FromMinutes(16));
            var expired = expiredRegistry.AdmitSelectedTargetEvidence(
                TenantId,
                expiredSessionId,
                null,
                null,
                null,
                () => true,
                cancelled.Token);
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Expired, expired.Status);
            Assert.Null(expired.Result);
        }

        using (var unknownRegistry = CreateRegistry(lifecycleInvoker))
        {
            var unknown = unknownRegistry.AdmitSelectedTargetEvidence(
                TenantId,
                "missing-unknown-session",
                null,
                null,
                null,
                () => true,
                cancelled.Token);
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Unknown, unknown.Status);
            Assert.Null(unknown.Result);
        }
        Assert.Equal(0, lifecycleInvoker.CallCount);

        var blockingInvoker = new BlockingEvidenceInvoker(
            16,
            () => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
                .Fail("reservation released", "RESERVATION_RELEASED"));
        using var capacityRegistry = CreateRegistry(blockingInvoker);
        var capacitySessionId = await PublishSessionAsync(capacityRegistry, "ordering-capacity");
        var reservations = Enumerable.Range(0, 16)
            .Select(_ => Task.Factory.StartNew(
                () => capacityRegistry.AdmitSelectedTargetEvidence(
                    TenantId,
                    capacitySessionId,
                    null,
                    null,
                    null,
                    () => true,
                    CancellationToken.None),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default))
            .ToArray();
        await blockingInvoker.AllExpectedCallsEntered.WaitAsync(TimeSpan.FromSeconds(10));

        var capacity = capacityRegistry.AdmitSelectedTargetEvidence(
            TenantId,
            capacitySessionId,
            null,
            null,
            null,
            () => true,
            cancelled.Token);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, capacity.Status);
        Assert.Null(capacity.Result);
        Assert.Equal(16, blockingInvoker.CallCount);

        blockingInvoker.ReleaseAll();
        var completedReservations = await Task.WhenAll(reservations);
        Assert.All(
            completedReservations,
            admission =>
            {
                Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, admission.Status);
                Assert.NotNull(admission.Result);
                Assert.False(admission.Result.IsSuccess);
            });
    }

    [Fact]
    public async Task Admit_SixteenReservationsBoundConcurrentInvocationAndSeventeenthIsCapacityExceeded()
    {
        var association = ValidAssociation();
        var blockingInvoker = new BlockingEvidenceInvoker(
            16,
            () => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association));
        var handleSequence = 0;
        using var registry = CreateRegistry(
            blockingInvoker,
            () => $"dgeh_{Interlocked.Increment(ref handleSequence):x32}");
        var sessionId = await PublishSessionAsync(registry, "concurrent-capacity");
        var admissions = Enumerable.Range(0, 16)
            .Select(_ => Task.Factory.StartNew(
                () => registry.AdmitSelectedTargetEvidence(
                    TenantId,
                    sessionId,
                    null,
                    null,
                    null,
                    () => true,
                    CancellationToken.None),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default))
            .ToArray();
        await blockingInvoker.AllExpectedCallsEntered.WaitAsync(TimeSpan.FromSeconds(10));

        var seventeenth = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, seventeenth.Status);
        Assert.Null(seventeenth.Result);
        Assert.Equal(16, blockingInvoker.CallCount);
        Assert.Equal(0, Volatile.Read(ref handleSequence));

        blockingInvoker.ReleaseAll();
        var completed = await Task.WhenAll(admissions);
        var handles = completed
            .Select(admission =>
            {
                Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, admission.Status);
                Assert.NotNull(admission.Result);
                Assert.True(admission.Result.IsSuccess, admission.Result.ErrorMessage);
                return Assert.IsType<SelectedTargetEvidenceHandle>(admission.Result.Value);
            })
            .ToArray();

        Assert.Equal(16, blockingInvoker.CallCount);
        Assert.Equal(16, Volatile.Read(ref handleSequence));
        Assert.Equal(16, handles.Select(handle => handle.HandleId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(
            handles,
            handle =>
            {
                Assert.Matches("^dgeh_[0-9a-f]{32}$", handle.HandleId);
                var resolved = registry.ResolveSelectedTargetEvidence(
                    TenantId,
                    sessionId,
                    handle.HandleId,
                    () => true);
                Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, resolved.Status);
                Assert.NotNull(resolved.Evidence);
            });
    }

    [Fact]
    public async Task Admit_PreAndMidInvocationCancellationDoNotPublishAndReleaseReservation()
    {
        var association = ValidAssociation();
        var handleFactoryCalls = 0;
        var invoker = new RecordingEvidenceInvoker();
        using var registry = CreateRegistry(
            invoker,
            () => $"dgeh_{++handleFactoryCalls:x32}");
        var sessionId = await PublishSessionAsync(registry, "cancellation");

        using var preCancelled = new CancellationTokenSource();
        preCancelled.Cancel();
        var preObserved = Assert.Throws<OperationCanceledException>(() =>
            registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                preCancelled.Token));
        Assert.Equal(preCancelled.Token, preObserved.CancellationToken);
        Assert.Equal(0, invoker.CallCount);
        Assert.Equal(0, handleFactoryCalls);

        using var midInvocation = new CancellationTokenSource();
        invoker.Handler = (_, _, _) =>
        {
            midInvocation.Cancel();
            return OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association);
        };
        var midObserved = Assert.Throws<OperationCanceledException>(() =>
            registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                midInvocation.Token));
        Assert.Equal(midInvocation.Token, midObserved.CancellationToken);
        Assert.Equal(1, invoker.CallCount);
        Assert.Equal(0, handleFactoryCalls);
        var unpublished = registry.ResolveSelectedTargetEvidence(
            TenantId,
            sessionId,
            "dgeh_00000000000000000000000000000001",
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Unknown, unpublished.Status);
        Assert.Null(unpublished.Evidence);

        invoker.Handler = (_, _, _) => OperationResult<
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association);
        for (var index = 0; index < 16; index++)
        {
            var recovered = registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None);
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, recovered.Status);
            Assert.NotNull(recovered.Result);
            Assert.True(recovered.Result.IsSuccess, recovered.Result.ErrorMessage);
        }

        var capacity = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, capacity.Status);
        Assert.Null(capacity.Result);
        Assert.Equal(17, invoker.CallCount);
        Assert.Equal(16, handleFactoryCalls);
    }

    [Fact]
    public async Task Admit_PreInvokeAndPostInvokeGateFailuresSuppressOrRollbackPublication()
    {
        var association = ValidAssociation();
        var handleFactoryCalls = 0;
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association)
        };
        using var registry = CreateRegistry(
            invoker,
            () => $"dgeh_{++handleFactoryCalls:x32}");
        var sessionId = await PublishSessionAsync(registry, "gate-rollback");

        var preGateCalls = 0;
        bool PreInvokeGate() => ++preGateCalls == 1;
        var preInvoke = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            PreInvokeGate,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.ForbiddenAssembly, preInvoke.Status);
        Assert.Null(preInvoke.Result);
        Assert.Equal(2, preGateCalls);
        Assert.Equal(0, invoker.CallCount);
        Assert.Equal(0, handleFactoryCalls);

        var postGateCalls = 0;
        bool PostInvokeGate() => ++postGateCalls <= 3;
        var postInvoke = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            PostInvokeGate,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.ForbiddenAssembly, postInvoke.Status);
        Assert.Null(postInvoke.Result);
        Assert.Equal(4, postGateCalls);
        Assert.Equal(1, invoker.CallCount);
        Assert.Equal(0, handleFactoryCalls);

        var neverPublished = registry.ResolveSelectedTargetEvidence(
            TenantId,
            sessionId,
            "dgeh_00000000000000000000000000000001",
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Unknown, neverPublished.Status);
        Assert.Null(neverPublished.Evidence);

        var throwingGateCalls = 0;
        var finalGateFailure = new InvalidOperationException("final admission gate failed");
        bool ThrowingFinalGate()
        {
            if (++throwingGateCalls == 4)
                throw finalGateFailure;
            return true;
        }
        var observedFinalGateFailure = Assert.Throws<InvalidOperationException>(() =>
            registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                ThrowingFinalGate,
                CancellationToken.None));
        Assert.Same(finalGateFailure, observedFinalGateFailure);
        Assert.Equal(4, throwingGateCalls);
        Assert.Equal(2, invoker.CallCount);
        Assert.Equal(0, handleFactoryCalls);

        for (var index = 0; index < 16; index++)
        {
            var recovered = registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None);
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, recovered.Status);
            Assert.NotNull(recovered.Result);
            Assert.True(recovered.Result.IsSuccess, recovered.Result.ErrorMessage);
        }

        var capacity = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, capacity.Status);
        Assert.Null(capacity.Result);
        Assert.Equal(18, invoker.CallCount);
        Assert.Equal(16, handleFactoryCalls);

        var rollbackInvoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association)
        };
        var rollbackFactoryCalls = 0;
        using var rollbackRegistry = CreateRegistry(
            rollbackInvoker,
            () =>
            {
                rollbackFactoryCalls++;
                return "dgeh_17171717171717171717171717171717";
            });
        var rollbackEntry = await PublishSessionEntryAsync(
            rollbackRegistry,
            "final-gate-reentrant-rollback");
        var rollbackGateCalls = 0;
        bool ReentrantRollbackGate()
        {
            if (++rollbackGateCalls == 4)
                Assert.True(rollbackRegistry.RollbackPublished(rollbackEntry));
            return true;
        }
        var rollbackAdmission = await Task.Run(() => rollbackRegistry.AdmitSelectedTargetEvidence(
                TenantId,
                rollbackEntry.SessionId,
                null,
                null,
                null,
                ReentrantRollbackGate,
                CancellationToken.None))
            .WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Unknown, rollbackAdmission.Status);
        Assert.Null(rollbackAdmission.Result);
        Assert.Equal(4, rollbackGateCalls);
        Assert.Equal(0, rollbackFactoryCalls);

        var invalidationInvoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association)
        };
        var invalidationFactoryCalls = 0;
        using var invalidationRegistry = CreateRegistry(
            invalidationInvoker,
            () =>
            {
                invalidationFactoryCalls++;
                return "dgeh_18181818181818181818181818181818";
            });
        var invalidationSessionId = await PublishSessionAsync(
            invalidationRegistry,
            "final-gate-reentrant-invalidation");
        var invalidationGateCalls = 0;
        bool ReentrantInvalidationGate()
        {
            if (++invalidationGateCalls == 4)
                invalidationRegistry.InvalidateAll();
            return true;
        }
        var invalidationAdmission = await Task.Run(() =>
                invalidationRegistry.AdmitSelectedTargetEvidence(
                    TenantId,
                    invalidationSessionId,
                    null,
                    null,
                    null,
                    ReentrantInvalidationGate,
                    CancellationToken.None))
            .WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(
            SelectedTargetEvidenceAdmissionStatus.ForbiddenAssembly,
            invalidationAdmission.Status);
        Assert.Null(invalidationAdmission.Result);
        Assert.Equal(4, invalidationGateCalls);
        Assert.Equal(0, invalidationFactoryCalls);
    }

    [Fact]
    public async Task Admit_NonFatalExceptionGraphRethrowsExactAndReleasesReservation()
    {
        var association = ValidAssociation();
        var invokerExpected = new AggregateException(
            "nonfatal-invoker-aggregate",
            new InvalidOperationException(
                "nonfatal-invoker-wrapper",
                new ArgumentException("nonfatal-invoker-leaf")),
            new NotSupportedException("nonfatal-invoker-sibling"));
        var factoryExpected = new AggregateException(
            "nonfatal-factory-aggregate",
            new InvalidOperationException(
                "nonfatal-factory-wrapper",
                new ArgumentException("nonfatal-factory-leaf")));
        var handleFactoryCalls = 0;
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => throw invokerExpected
        };
        using var registry = CreateRegistry(
            invoker,
            () =>
            {
                var attempt = ++handleFactoryCalls;
                if (attempt == 1)
                    throw factoryExpected;
                return $"dgeh_{attempt:x32}";
            });
        var sessionId = await PublishSessionAsync(registry, "nonfatal-exceptions");

        var observedInvoker = Assert.Throws<AggregateException>(() =>
            registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None));
        Assert.Same(invokerExpected, observedInvoker);
        Assert.Equal(1, invoker.CallCount);
        Assert.Equal(0, handleFactoryCalls);

        invoker.Handler = (_, _, _) => OperationResult<
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association);
        var observedFactory = Assert.Throws<AggregateException>(() =>
            registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None));
        Assert.Same(factoryExpected, observedFactory);
        Assert.Equal(2, invoker.CallCount);
        Assert.Equal(1, handleFactoryCalls);

        for (var index = 0; index < 16; index++)
        {
            var recovered = registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None);
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, recovered.Status);
            Assert.NotNull(recovered.Result);
            Assert.True(recovered.Result.IsSuccess, recovered.Result.ErrorMessage);
        }

        var capacity = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, capacity.Status);
        Assert.Null(capacity.Result);
        Assert.Equal(18, invoker.CallCount);
        Assert.Equal(17, handleFactoryCalls);
    }

    [Fact]
    public async Task Admit_FatalExceptionGraphRethrowsExactAndRetainsReservation()
    {
        var safeFatalMarker = new OutOfMemoryException(
            "constructed fatal-classifier marker; no memory exhaustion occurred");
        var expected = new AggregateException(
            "safe fatal aggregate",
            new InvalidOperationException("ordinary sibling"),
            new InvalidOperationException("fatal wrapper", safeFatalMarker));
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => throw expected
        };
        var handleFactoryCalls = 0;
        using var registry = CreateRegistry(
            invoker,
            () => $"dgeh_{++handleFactoryCalls:x32}");
        var sessionId = await PublishSessionAsync(registry, "fatal-exceptions");

        for (var index = 0; index < 16; index++)
        {
            var observed = Assert.Throws<AggregateException>(() =>
                registry.AdmitSelectedTargetEvidence(
                    TenantId,
                    sessionId,
                    null,
                    null,
                    null,
                    () => true,
                    CancellationToken.None));
            Assert.Same(expected, observed);
            Assert.Equal(index + 1, invoker.CallCount);
        }

        var capacity = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, capacity.Status);
        Assert.Null(capacity.Result);
        Assert.Equal(16, invoker.CallCount);
        Assert.Equal(0, handleFactoryCalls);
    }

    [Fact]
    public async Task Admit_HandleCollisionRetriesThenSucceedsWithOrdinalUniqueHandle()
    {
        const string storedHandle = "dgeh_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        const string uniqueHandle = "dgeh_bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        var storedAssociation = ValidAssociation();
        var uniqueAssociation = ValidAssociation();
        RequestedRowIdsBackingArray(storedAssociation)[0] = "stored-association-marker";
        RequestedRowIdsBackingArray(uniqueAssociation)[0] = "unique-association-marker";
        var outputs = new Queue<string?>(
        [
            storedHandle,
            storedHandle,
            null,
            string.Empty,
            "malformed-handle",
            "dgeh_BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
            uniqueHandle
        ]);
        var handleFactoryCalls = 0;
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
                    storedAssociation)
        };
        using var registry = CreateRegistry(
            invoker,
            () =>
            {
                handleFactoryCalls++;
                return outputs.Dequeue()!;
            });
        var sessionId = await PublishSessionAsync(registry, "handle-collision");

        var stored = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(storedHandle, stored.Result!.Value!.HandleId);

        invoker.Handler = (_, _, _) => OperationResult<
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
                uniqueAssociation);
        var unique = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, unique.Status);
        Assert.NotNull(unique.Result);
        Assert.True(unique.Result.IsSuccess, unique.Result.ErrorMessage);
        Assert.Equal(uniqueHandle, unique.Result.Value!.HandleId);
        Assert.Equal(7, handleFactoryCalls);
        Assert.Empty(outputs);
        Assert.Equal(2, invoker.CallCount);

        RequestedRowIdsBackingArray(storedAssociation)[0] = "mutated-stored-source";
        RequestedRowIdsBackingArray(uniqueAssociation)[0] = "mutated-unique-source";
        var storedResolution = registry.ResolveSelectedTargetEvidence(
            TenantId,
            sessionId,
            storedHandle,
            () => true);
        var uniqueResolution = registry.ResolveSelectedTargetEvidence(
            TenantId,
            sessionId,
            uniqueHandle,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, storedResolution.Status);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, uniqueResolution.Status);
        Assert.Equal(
            "stored-association-marker",
            storedResolution.Evidence!.EdgeAuthoritySelectionQualityEvidence
                .NoveltyEvaluation.RequestedRowIds[0]);
        Assert.Equal(
            "unique-association-marker",
            uniqueResolution.Evidence!.EdgeAuthoritySelectionQualityEvidence
                .NoveltyEvaluation.RequestedRowIds[0]);
        Assert.NotSame(storedAssociation, storedResolution.Evidence);
        Assert.NotSame(uniqueAssociation, uniqueResolution.Evidence);
        Assert.NotSame(storedResolution.Evidence, uniqueResolution.Evidence);
    }

    [Fact]
    public async Task Admit_ThirtyTwoCollisionsFailClosedWithExactAllocationFailureAndReleaseReservation()
    {
        const string allocationCode = "HOST_SELECTED_TARGET_EVIDENCE_HANDLE_ALLOCATION_FAILED";
        const string allocationMessage =
            "A selected-target evidence handle could not be allocated safely.";
        const string storedHandle = "dgeh_cccccccccccccccccccccccccccccccc";
        var storedAssociation = ValidAssociation();
        var recoveryAssociation = ValidAssociation();
        RequestedRowIdsBackingArray(storedAssociation)[0] = "allocation-stored-marker";
        RequestedRowIdsBackingArray(recoveryAssociation)[0] = "allocation-recovery-marker";
        var factoryCalls = 0;
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
                    storedAssociation)
        };
        using var registry = CreateRegistry(
            invoker,
            () =>
            {
                var attempt = ++factoryCalls;
                if (attempt == 1)
                    return storedHandle;
                if (attempt <= 33)
                {
                    return attempt switch
                    {
                        2 => storedHandle,
                        3 => storedHandle + "\n",
                        4 => storedHandle + "\r\n",
                        _ => attempt % 2 == 0
                            ? storedHandle
                            : "invalid-allocation-candidate"
                    };
                }
                return $"dgeh_{attempt:x32}";
            });
        var sessionId = await PublishSessionAsync(registry, "allocation-failure");

        var seed = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(storedHandle, seed.Result!.Value!.HandleId);

        invoker.Handler = (_, _, _) => OperationResult<
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
                recoveryAssociation);
        var failed = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, failed.Status);
        Assert.NotNull(failed.Result);
        Assert.False(failed.Result.IsSuccess);
        Assert.Equal(allocationCode, failed.Result.ErrorCode);
        Assert.Equal(allocationMessage, failed.Result.ErrorMessage);
        Assert.Equal(33, factoryCalls);

        var storedAfterFailure = registry.ResolveSelectedTargetEvidence(
            TenantId,
            sessionId,
            storedHandle,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, storedAfterFailure.Status);
        Assert.Equal(
            "allocation-stored-marker",
            storedAfterFailure.Evidence!.EdgeAuthoritySelectionQualityEvidence
                .NoveltyEvaluation.RequestedRowIds[0]);

        var recovered = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, recovered.Status);
        Assert.NotNull(recovered.Result);
        Assert.True(recovered.Result.IsSuccess, recovered.Result.ErrorMessage);
        Assert.Equal("dgeh_00000000000000000000000000000022", recovered.Result.Value!.HandleId);
        Assert.Equal(34, factoryCalls);

        for (var index = 0; index < 14; index++)
        {
            var admitted = registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None);
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, admitted.Status);
            Assert.NotNull(admitted.Result);
            Assert.True(admitted.Result.IsSuccess, admitted.Result.ErrorMessage);
        }
        var capacity = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, capacity.Status);
        Assert.Null(capacity.Result);
        Assert.Equal(48, factoryCalls);
        Assert.Equal(17, invoker.CallCount);
    }

    [Fact]
    public async Task Resolve_ValidatesExactlyAndTenantSessionOrHandleMismatchIsUnknownWithoutLeakage()
    {
        var disposedInvoker = new RecordingEvidenceInvoker();
        var disposedRegistry = CreateRegistry(disposedInvoker);
        disposedRegistry.Dispose();
        Assert.Throws<ObjectDisposedException>(() => disposedRegistry.ResolveSelectedTargetEvidence(
            " ",
            " ",
            " ",
            null!));

        var association = ValidAssociation();
        var handles = new Queue<string>(
        [
            "dgeh_dddddddddddddddddddddddddddddddd",
            "dgeh_eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"
        ]);
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association)
        };
        using var registry = CreateRegistry(invoker, () => handles.Dequeue());
        var ownerSessionId = await PublishSessionAsync(registry, "resolve-owner");
        var otherSessionId = await PublishSessionAsync(registry, "resolve-other");
        const string ownerHandle = "dgeh_dddddddddddddddddddddddddddddddd";
        const string otherHandle = "dgeh_eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
        Assert.True(registry.AdmitSelectedTargetEvidence(
            TenantId,
            ownerSessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None).Result!.IsSuccess);
        Assert.True(registry.AdmitSelectedTargetEvidence(
            TenantId,
            otherSessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None).Result!.IsSuccess);

        var tenantException = Assert.Throws<ArgumentException>(() =>
            registry.ResolveSelectedTargetEvidence(" ", " ", " ", null!));
        Assert.Equal("tenantId", tenantException.ParamName);
        var sessionException = Assert.Throws<ArgumentException>(() =>
            registry.ResolveSelectedTargetEvidence(TenantId, " ", " ", null!));
        Assert.Equal("sessionId", sessionException.ParamName);
        var handleException = Assert.Throws<ArgumentException>(() =>
            registry.ResolveSelectedTargetEvidence(TenantId, ownerSessionId, " ", null!));
        Assert.Equal("handle", handleException.ParamName);
        var gateException = Assert.Throws<ArgumentNullException>(() =>
            registry.ResolveSelectedTargetEvidence(TenantId, ownerSessionId, ownerHandle, null!));
        Assert.Equal("executionAllowed", gateException.ParamName);

        var forbidden = registry.ResolveSelectedTargetEvidence(
            "wrong-tenant",
            "missing-session",
            "malformed-nonblank-handle",
            () => false);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.ForbiddenAssembly, forbidden.Status);
        Assert.Null(forbidden.Evidence);

        var unknownCases = new[]
        {
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                ownerSessionId,
                "malformed-nonblank-handle",
                () => true),
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                ownerSessionId,
                "dgeh_ffffffffffffffffffffffffffffffff",
                () => true),
            registry.ResolveSelectedTargetEvidence(
                "wrong-tenant",
                ownerSessionId,
                ownerHandle,
                () => true),
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                "missing-session",
                ownerHandle,
                () => true),
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                otherSessionId,
                ownerHandle,
                () => true),
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                ownerSessionId,
                otherHandle,
                () => true)
        };
        Assert.All(
            unknownCases,
            result =>
            {
                Assert.Equal(SelectedTargetEvidenceResolutionStatus.Unknown, result.Status);
                Assert.Null(result.Evidence);
            });

        var found = registry.ResolveSelectedTargetEvidence(
            TenantId,
            ownerSessionId,
            ownerHandle,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, found.Status);
        Assert.NotNull(found.Evidence);
        Assert.Equal(2, invoker.CallCount);
    }

    [Fact]
    public async Task Claim_EntersLifecycleValidatesExactlyAndRevalidatesWithCallbacksOutsideLocks()
    {
        var disposedRegistry = CreateRegistry(new RecordingEvidenceInvoker());
        disposedRegistry.Dispose();
        Assert.Throws<ObjectDisposedException>(() => disposedRegistry.ClaimSelectedTargetEvidence(
            " ",
            " ",
            " ",
            null!));

        const string handle = "dgeh_24242424242424242424242424242424";
        var association = ValidAssociation();
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association)
        };
        using var registry = CreateRegistry(invoker, () => handle);
        var entry = await PublishSessionEntryAsync(registry, "claim-validation");
        Assert.True(registry.AdmitSelectedTargetEvidence(
            TenantId,
            entry.SessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None).Result!.IsSuccess);

        var tenantException = Assert.Throws<ArgumentException>(() =>
            registry.ClaimSelectedTargetEvidence(" ", " ", " ", null!));
        Assert.Equal("tenantId", tenantException.ParamName);
        var sessionException = Assert.Throws<ArgumentException>(() =>
            registry.ClaimSelectedTargetEvidence(TenantId, " ", " ", null!));
        Assert.Equal("sessionId", sessionException.ParamName);
        var handleException = Assert.Throws<ArgumentException>(() =>
            registry.ClaimSelectedTargetEvidence(TenantId, entry.SessionId, " ", null!));
        Assert.Equal("handle", handleException.ParamName);
        var gateException = Assert.Throws<ArgumentNullException>(() =>
            registry.ClaimSelectedTargetEvidence(TenantId, entry.SessionId, handle, null!));
        Assert.Equal("executionAllowed", gateException.ParamName);

        var deniedCalls = 0;
        var denied = registry.ClaimSelectedTargetEvidence(
            "wrong-tenant",
            "missing-session",
            "malformed-nonblank-handle",
            () =>
            {
                deniedCalls++;
                return false;
            });
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.ForbiddenAssembly, denied.Status);
        Assert.Null(denied.Evidence);
        Assert.Equal(1, deniedCalls);

        var gateCalls = 0;
        bool ExecutionAllowed()
        {
            gateCalls++;
            var outsideLockProbe = Task.Run(() =>
            {
                var lookup = registry.Get(TenantId, entry.SessionId);
                return lookup.Status == SessionLookupStatus.Found
                       && entry.ResolveSelectedTargetEvidence(handle) is not null;
            });
            Assert.True(
                outsideLockProbe.Wait(TimeSpan.FromSeconds(5)),
                "The claim callback ran while a registry or evidence lock was held.");
            return outsideLockProbe.GetAwaiter().GetResult();
        }

        var found = registry.ClaimSelectedTargetEvidence(
            TenantId,
            entry.SessionId,
            handle,
            ExecutionAllowed);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, found.Status);
        Assert.NotNull(found.Evidence);
        Assert.Equal(3, gateCalls);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Unknown,
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                entry.SessionId,
                handle,
                () => true).Status);

        const string rollbackHandle = "dgeh_25252525252525252525252525252525";
        var rollbackInvoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
                ValidAssociation())
        };
        using var rollbackRegistry = CreateRegistry(rollbackInvoker, () => rollbackHandle);
        var rollbackEntry = await PublishSessionEntryAsync(rollbackRegistry, "claim-rollback");
        Assert.True(rollbackRegistry.AdmitSelectedTargetEvidence(
            TenantId,
            rollbackEntry.SessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None).Result!.IsSuccess);
        var rollbackGateCalls = 0;
        bool RollbackGate()
        {
            if (++rollbackGateCalls == 2)
                Assert.True(rollbackRegistry.RollbackPublished(rollbackEntry));
            return true;
        }
        var rolledBack = rollbackRegistry.ClaimSelectedTargetEvidence(
            TenantId,
            rollbackEntry.SessionId,
            rollbackHandle,
            RollbackGate);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Unknown, rolledBack.Status);
        Assert.Null(rolledBack.Evidence);
        Assert.Equal(2, rollbackGateCalls);

        const string invalidationHandle = "dgeh_26262626262626262626262626262626";
        var invalidationInvoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
                ValidAssociation())
        };
        using var invalidationRegistry = CreateRegistry(
            invalidationInvoker,
            () => invalidationHandle);
        var invalidationEntry = await PublishSessionEntryAsync(
            invalidationRegistry,
            "claim-invalidation");
        Assert.True(invalidationRegistry.AdmitSelectedTargetEvidence(
            TenantId,
            invalidationEntry.SessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None).Result!.IsSuccess);
        var invalidationGateCalls = 0;
        bool InvalidationGate()
        {
            if (++invalidationGateCalls == 3)
                invalidationRegistry.InvalidateAll();
            return true;
        }
        var invalidated = invalidationRegistry.ClaimSelectedTargetEvidence(
            TenantId,
            invalidationEntry.SessionId,
            invalidationHandle,
            InvalidationGate);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.ForbiddenAssembly,
            invalidated.Status);
        Assert.Null(invalidated.Evidence);
        Assert.Equal(3, invalidationGateCalls);

        const string expiredHandle = "dgeh_27272727272727272727272727272727";
        var time = new MutableTimeProvider(
            new DateTimeOffset(2026, 7, 25, 14, 0, 0, TimeSpan.Zero));
        var expiredInvoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
                ValidAssociation())
        };
        using var expiredRegistry = CreateRegistry(expiredInvoker, () => expiredHandle, time);
        var expiredEntry = await PublishSessionEntryAsync(expiredRegistry, "claim-expired");
        Assert.True(expiredRegistry.AdmitSelectedTargetEvidence(
            TenantId,
            expiredEntry.SessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None).Result!.IsSuccess);
        time.Advance(TimeSpan.FromMinutes(15));
        var expiredGateCalls = 0;
        var expired = expiredRegistry.ClaimSelectedTargetEvidence(
            TenantId,
            expiredEntry.SessionId,
            expiredHandle,
            () =>
            {
                expiredGateCalls++;
                return true;
            });
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Expired, expired.Status);
        Assert.Null(expired.Evidence);
        Assert.Equal(1, expiredGateCalls);
    }

    [Fact]
    public async Task Claim_IsAtomicOwnedIrreversibleAndFreesExactlyOneLiveCapacitySlot()
    {
        var association = ValidAssociation();
        var sourceRows = RequestedRowIdsBackingArray(association);
        sourceRows[0] = "claim-capacity-owned-marker";
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association)
        };
        var handles = new Queue<string>(Enumerable.Range(1, 17)
            .Select(index => $"dgeh_{index:x32}"));
        using var registry = CreateRegistry(invoker, () => handles.Dequeue());
        var entry = await PublishSessionEntryAsync(registry, "claim-concurrent-capacity");
        var publishedHandles = new List<string>();
        for (var index = 0; index < 16; index++)
        {
            var admission = registry.AdmitSelectedTargetEvidence(
                TenantId,
                entry.SessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None);
            Assert.True(admission.Result!.IsSuccess, admission.Result.ErrorMessage);
            publishedHandles.Add(admission.Result.Value!.HandleId);
        }

        var full = registry.AdmitSelectedTargetEvidence(
            TenantId,
            entry.SessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, full.Status);
        Assert.Null(full.Result);
        Assert.Equal(16, invoker.CallCount);

        sourceRows[0] = "claim-capacity-source-mutated";
        var ready = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var readyCount = 0;
        var claims = Enumerable.Range(0, 16)
            .Select(async _ =>
            {
                if (Interlocked.Increment(ref readyCount) == 16)
                    ready.TrySetResult();
                await start.Task.ConfigureAwait(false);
                return registry.ClaimSelectedTargetEvidence(
                    TenantId,
                    entry.SessionId,
                    publishedHandles[0],
                    () => true);
            })
            .ToArray();
        await ready.Task.WaitAsync(TimeSpan.FromSeconds(5));
        start.TrySetResult();
        var results = await Task.WhenAll(claims).WaitAsync(TimeSpan.FromSeconds(10));
        var winner = Assert.Single(
            results,
            result => result.Status == SelectedTargetEvidenceResolutionStatus.Found);
        Assert.All(
            results.Where(result => !ReferenceEquals(result, winner)),
            result =>
            {
                Assert.Equal(SelectedTargetEvidenceResolutionStatus.Unknown, result.Status);
                Assert.Null(result.Evidence);
            });
        Assert.NotNull(winner.Evidence);
        Assert.NotSame(association, winner.Evidence);
        Assert.Equal(
            "claim-capacity-owned-marker",
            winner.Evidence.EdgeAuthoritySelectionQualityEvidence
                .NoveltyEvaluation.RequestedRowIds[0],
            StringComparer.Ordinal);
        RequestedRowIdsBackingArray(winner.Evidence)[0] = "claim-winner-mutated";
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Unknown,
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                entry.SessionId,
                publishedHandles[0],
                () => true).Status);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Unknown,
            registry.ClaimSelectedTargetEvidence(
                TenantId,
                entry.SessionId,
                publishedHandles[0],
                () => true).Status);

        var replacement = registry.AdmitSelectedTargetEvidence(
            TenantId,
            entry.SessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.True(replacement.Result!.IsSuccess, replacement.Result.ErrorMessage);
        Assert.Equal("dgeh_00000000000000000000000000000011", replacement.Result.Value!.HandleId);
        var fullAgain = registry.AdmitSelectedTargetEvidence(
            TenantId,
            entry.SessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, fullAgain.Status);
        Assert.Null(fullAgain.Result);
        Assert.Equal(17, invoker.CallCount);
        Assert.Empty(handles);
    }

    [Fact]
    public async Task Claim_RegistryOnlyCallerFailureOrCancellationAfterFoundNeverRestoresEvidence()
    {
        const string failureHandle = "dgeh_28282828282828282828282828282828";
        const string cancellationHandle = "dgeh_29292929292929292929292929292929";
        var handles = new Queue<string>([failureHandle, cancellationHandle]);
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
                ValidAssociation())
        };
        using var registry = CreateRegistry(invoker, () => handles.Dequeue());
        var sessionId = await PublishSessionAsync(registry, "claim-caller-outcome");
        foreach (var _ in handles.ToArray())
        {
            Assert.True(registry.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None).Result!.IsSuccess);
        }

        var callerFailure = new InvalidOperationException("caller failed after registry claim");
        var observedFailure = Assert.Throws<InvalidOperationException>(
            (Action)(() =>
            {
                var claimed = registry.ClaimSelectedTargetEvidence(
                    TenantId,
                    sessionId,
                    failureHandle,
                    () => true);
                Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, claimed.Status);
                Assert.NotNull(claimed.Evidence);
                throw callerFailure;
            }));
        Assert.Same(callerFailure, observedFailure);

        using var cancellation = new CancellationTokenSource();
        var cancellationClaim = registry.ClaimSelectedTargetEvidence(
            TenantId,
            sessionId,
            cancellationHandle,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, cancellationClaim.Status);
        Assert.NotNull(cancellationClaim.Evidence);
        cancellation.Cancel();
        var observedCancellation = Assert.Throws<OperationCanceledException>(
            cancellation.Token.ThrowIfCancellationRequested);
        Assert.Equal(cancellation.Token, observedCancellation.CancellationToken);

        foreach (var handleId in new[] { failureHandle, cancellationHandle })
        {
            Assert.Equal(
                SelectedTargetEvidenceResolutionStatus.Unknown,
                registry.ResolveSelectedTargetEvidence(
                    TenantId,
                    sessionId,
                    handleId,
                    () => true).Status);
            Assert.Equal(
                SelectedTargetEvidenceResolutionStatus.Unknown,
                registry.ClaimSelectedTargetEvidence(
                    TenantId,
                    sessionId,
                    handleId,
                    () => true).Status);
        }

        // This test intentionally stops at the registry boundary: downstream execution and its
        // cancellation/failure cannot roll back an already-returned Found claim.
        Assert.Equal(2, invoker.CallCount);
    }

    [Fact]
    public async Task Claim_LiveAndRetiredHandlesCauseThirtyTwoAbaCollisionsBeforeFreshIdSucceeds()
    {
        const string allocationCode = "HOST_SELECTED_TARGET_EVIDENCE_HANDLE_ALLOCATION_FAILED";
        const string allocationMessage =
            "A selected-target evidence handle could not be allocated safely.";
        const string retiredHandle = "dgeh_30303030303030303030303030303030";
        const string liveHandle = "dgeh_31313131313131313131313131313131";
        const string freshHandle = "dgeh_32323232323232323232323232323232";
        var candidates = new Queue<string>();
        candidates.Enqueue(retiredHandle);
        candidates.Enqueue(liveHandle);
        for (var index = 0; index < 32; index++)
            candidates.Enqueue(index % 2 == 0 ? retiredHandle : liveHandle);
        candidates.Enqueue(freshHandle);
        var factoryCalls = 0;
        string HandleFactory()
        {
            factoryCalls++;
            return candidates.Dequeue();
        }
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
                ValidAssociation())
        };
        using var registry = CreateRegistry(invoker, HandleFactory);
        var sessionId = await PublishSessionAsync(registry, "claim-aba");

        var retiredAdmission = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.True(retiredAdmission.Result!.IsSuccess, retiredAdmission.Result.ErrorMessage);
        Assert.Equal(retiredHandle, retiredAdmission.Result.Value!.HandleId);
        var retiredClaim = registry.ClaimSelectedTargetEvidence(
            TenantId,
            sessionId,
            retiredHandle,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, retiredClaim.Status);
        Assert.NotNull(retiredClaim.Evidence);

        var liveAdmission = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.True(liveAdmission.Result!.IsSuccess, liveAdmission.Result.ErrorMessage);
        Assert.Equal(liveHandle, liveAdmission.Result.Value!.HandleId);

        var failed = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, failed.Status);
        Assert.NotNull(failed.Result);
        Assert.False(failed.Result.IsSuccess);
        Assert.Equal(allocationCode, failed.Result.ErrorCode);
        Assert.Equal(allocationMessage, failed.Result.ErrorMessage);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Unknown,
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                sessionId,
                retiredHandle,
                () => true).Status);
        Assert.Equal(
            SelectedTargetEvidenceResolutionStatus.Found,
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                sessionId,
                liveHandle,
                () => true).Status);

        var recovered = registry.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.True(recovered.Result!.IsSuccess, recovered.Result.ErrorMessage);
        Assert.Equal(freshHandle, recovered.Result.Value!.HandleId);
        Assert.Equal(35, factoryCalls);
        Assert.Equal(4, invoker.CallCount);
        Assert.Empty(candidates);
    }

    [Fact]
    public async Task Expiry_RevokesAllHandlesImmediatelyThenPreservesExpiredToUnknownTombstoneTransition()
    {
        const string retiredHandleId = "dgeh_11111111111111111111111111111111";
        const string handleId = "dgeh_12121212121212121212121212121212";
        var time = new MutableTimeProvider(
            new DateTimeOffset(2026, 7, 25, 12, 0, 0, TimeSpan.Zero));
        var association = ValidAssociation();
        var invoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association)
        };
        var handles = new Queue<string>([retiredHandleId, handleId]);
        using var registry = CreateRegistry(invoker, () => handles.Dequeue(), time);
        var session = await PublishSessionEntryAsync(registry, "expiry-handles");
        var retiredAdmission = registry.AdmitSelectedTargetEvidence(
            TenantId,
            session.SessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.True(retiredAdmission.Result!.IsSuccess, retiredAdmission.Result.ErrorMessage);
        var retiredClaim = registry.ClaimSelectedTargetEvidence(
            TenantId,
            session.SessionId,
            retiredHandleId,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found, retiredClaim.Status);
        Assert.NotNull(retiredClaim.Evidence);
        Assert.Contains(retiredHandleId, RetiredSelectedTargetEvidenceHandles(session));
        var admission = registry.AdmitSelectedTargetEvidence(
            TenantId,
            session.SessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.True(admission.Result!.IsSuccess, admission.Result.ErrorMessage);
        Assert.Equal(time.GetUtcNow().AddMinutes(15), session.ExpiresAtUtc);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Found,
            registry.ResolveSelectedTargetEvidence(
                TenantId,
                session.SessionId,
                handleId,
                () => true).Status);

        time.Advance(TimeSpan.FromMinutes(15));
        var atExactExpiry = registry.ResolveSelectedTargetEvidence(
            TenantId,
            session.SessionId,
            handleId,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Expired, atExactExpiry.Status);
        Assert.Null(atExactExpiry.Evidence);
        Assert.Null(session.ResolveSelectedTargetEvidence(handleId));
        Assert.Empty(RetiredSelectedTargetEvidenceHandles(session));
        var otherTenantDuringTombstone = registry.ResolveSelectedTargetEvidence(
            "other-tenant",
            session.SessionId,
            handleId,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Unknown, otherTenantDuringTombstone.Status);
        Assert.Null(otherTenantDuringTombstone.Evidence);

        time.Advance(TimeSpan.FromMinutes(14).Add(TimeSpan.FromSeconds(59)));
        var retainedTombstone = registry.ResolveSelectedTargetEvidence(
            TenantId,
            session.SessionId,
            handleId,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Expired, retainedTombstone.Status);
        Assert.Null(retainedTombstone.Evidence);

        time.Advance(TimeSpan.FromSeconds(1));
        var afterRetention = registry.ResolveSelectedTargetEvidence(
            TenantId,
            session.SessionId,
            handleId,
            () => true);
        var otherTenantAfterRetention = registry.ResolveSelectedTargetEvidence(
            "other-tenant",
            session.SessionId,
            handleId,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Unknown, afterRetention.Status);
        Assert.Null(afterRetention.Evidence);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Unknown, otherTenantAfterRetention.Status);
        Assert.Null(otherTenantAfterRetention.Evidence);
    }

    [Fact]
    public async Task RollbackInvalidationDisposeAndAdmissionRaceCannotResolveOrRepublishRevokedEvidence()
    {
        var association = ValidAssociation();

        var rollbackInvoker = new BlockingEvidenceInvoker(
            1,
            () => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association));
        var rollbackFactoryCalls = 0;
        using (var rollbackRegistry = CreateRegistry(
                   rollbackInvoker,
                   () =>
                   {
                       rollbackFactoryCalls++;
                       return "dgeh_13131313131313131313131313131313";
                   }))
        {
            var rollbackEntry = await PublishSessionEntryAsync(
                rollbackRegistry,
                "race-rollback");
            var rollbackAdmission = Task.Factory.StartNew(
                () => rollbackRegistry.AdmitSelectedTargetEvidence(
                    TenantId,
                    rollbackEntry.SessionId,
                    null,
                    null,
                    null,
                    () => true,
                    CancellationToken.None),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            await rollbackInvoker.AllExpectedCallsEntered.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.True(rollbackRegistry.RollbackPublished(rollbackEntry));
            Assert.Equal(
                DynamicGraphCompositionSessionRegistry.EvidenceReservationStatus.Revoked,
                rollbackEntry.ReserveSelectedTargetEvidence().Status);
            rollbackInvoker.ReleaseAll();
            var rollbackResult = await rollbackAdmission;
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Unknown, rollbackResult.Status);
            Assert.Null(rollbackResult.Result);
            Assert.Equal(0, rollbackFactoryCalls);
            var rollbackResolution = rollbackRegistry.ResolveSelectedTargetEvidence(
                TenantId,
                rollbackEntry.SessionId,
                "dgeh_13131313131313131313131313131313",
                () => true);
            Assert.Equal(SelectedTargetEvidenceResolutionStatus.Unknown, rollbackResolution.Status);
            Assert.Null(rollbackResolution.Evidence);
        }

        var invalidationInvoker = new BlockingEvidenceInvoker(
            1,
            () => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association));
        var invalidationFactoryCalls = 0;
        using (var invalidationRegistry = CreateRegistry(
                   invalidationInvoker,
                   () =>
                   {
                       invalidationFactoryCalls++;
                       return "dgeh_14141414141414141414141414141414";
                   }))
        {
            var invalidationEntry = await PublishSessionEntryAsync(
                invalidationRegistry,
                "race-invalidation");
            var invalidationAdmission = Task.Factory.StartNew(
                () => invalidationRegistry.AdmitSelectedTargetEvidence(
                    TenantId,
                    invalidationEntry.SessionId,
                    null,
                    null,
                    null,
                    () => true,
                    CancellationToken.None),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            await invalidationInvoker.AllExpectedCallsEntered.WaitAsync(TimeSpan.FromSeconds(10));
            invalidationRegistry.InvalidateAll();
            Assert.Equal(
                DynamicGraphCompositionSessionRegistry.EvidenceReservationStatus.Revoked,
                invalidationEntry.ReserveSelectedTargetEvidence().Status);
            invalidationInvoker.ReleaseAll();
            var invalidationResult = await invalidationAdmission;
            Assert.Equal(
                SelectedTargetEvidenceAdmissionStatus.ForbiddenAssembly,
                invalidationResult.Status);
            Assert.Null(invalidationResult.Result);
            Assert.Equal(0, invalidationFactoryCalls);
            var invalidationResolution = invalidationRegistry.ResolveSelectedTargetEvidence(
                TenantId,
                invalidationEntry.SessionId,
                "dgeh_14141414141414141414141414141414",
                () => true);
            Assert.Equal(
                SelectedTargetEvidenceResolutionStatus.ForbiddenAssembly,
                invalidationResolution.Status);
            Assert.Null(invalidationResolution.Evidence);
        }

        var disposeInvoker = new BlockingEvidenceInvoker(
            1,
            () => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association));
        const string disposeHandle = "dgeh_15151515151515151515151515151515";
        var disposeRegistry = CreateRegistry(disposeInvoker, () => disposeHandle);
        var disposeEntry = await PublishSessionEntryAsync(disposeRegistry, "race-dispose");
        var disposeAdmission = Task.Factory.StartNew(
            () => disposeRegistry.AdmitSelectedTargetEvidence(
                TenantId,
                disposeEntry.SessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        await disposeInvoker.AllExpectedCallsEntered.WaitAsync(TimeSpan.FromSeconds(10));
        var disposeTask = Task.Factory.StartNew(
            disposeRegistry.Dispose,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        await WaitForDisposedStateAsync(disposeRegistry);
        Assert.False(disposeTask.IsCompleted);
        disposeInvoker.ReleaseAll();
        var disposeAdmissionResult = await disposeAdmission;
        Assert.Equal(
            SelectedTargetEvidenceAdmissionStatus.Completed,
            disposeAdmissionResult.Status);
        Assert.True(disposeAdmissionResult.Result!.IsSuccess);
        Assert.Equal(disposeHandle, disposeAdmissionResult.Result.Value!.HandleId);
        await disposeTask;
        Assert.Null(disposeEntry.ResolveSelectedTargetEvidence(disposeHandle));
        Assert.Equal(
            DynamicGraphCompositionSessionRegistry.EvidenceReservationStatus.Revoked,
            disposeEntry.ReserveSelectedTargetEvidence().Status);
        Assert.Throws<ObjectDisposedException>(() => disposeRegistry.ResolveSelectedTargetEvidence(
            TenantId,
            disposeEntry.SessionId,
            disposeHandle,
            () => true));

        const string resolveRollbackHandle = "dgeh_19191919191919191919191919191919";
        var resolveRollbackInvoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association)
        };
        using (var resolveRollbackRegistry = CreateRegistry(
                   resolveRollbackInvoker,
                   () => resolveRollbackHandle))
        {
            var resolveRollbackEntry = await PublishSessionEntryAsync(
                resolveRollbackRegistry,
                "resolve-reentrant-rollback");
            Assert.True(resolveRollbackRegistry.AdmitSelectedTargetEvidence(
                TenantId,
                resolveRollbackEntry.SessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None).Result!.IsSuccess);
            var resolveRollbackGateCalls = 0;
            bool ResolveRollbackGate()
            {
                if (++resolveRollbackGateCalls == 2)
                    Assert.True(resolveRollbackRegistry.RollbackPublished(resolveRollbackEntry));
                return true;
            }
            var resolveRollbackResult = await Task.Run(() =>
                    resolveRollbackRegistry.ResolveSelectedTargetEvidence(
                        TenantId,
                        resolveRollbackEntry.SessionId,
                        resolveRollbackHandle,
                        ResolveRollbackGate))
                .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(
                SelectedTargetEvidenceResolutionStatus.Unknown,
                resolveRollbackResult.Status);
            Assert.Null(resolveRollbackResult.Evidence);
            Assert.Equal(2, resolveRollbackGateCalls);
            Assert.Null(resolveRollbackEntry.ResolveSelectedTargetEvidence(resolveRollbackHandle));
        }

        const string resolveInvalidationHandle = "dgeh_20202020202020202020202020202020";
        var resolveInvalidationInvoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association)
        };
        using (var resolveInvalidationRegistry = CreateRegistry(
                   resolveInvalidationInvoker,
                   () => resolveInvalidationHandle))
        {
            var resolveInvalidationEntry = await PublishSessionEntryAsync(
                resolveInvalidationRegistry,
                "resolve-reentrant-invalidation");
            Assert.True(resolveInvalidationRegistry.AdmitSelectedTargetEvidence(
                TenantId,
                resolveInvalidationEntry.SessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None).Result!.IsSuccess);
            var resolveInvalidationGateCalls = 0;
            bool ResolveInvalidationGate()
            {
                if (++resolveInvalidationGateCalls == 3)
                    resolveInvalidationRegistry.InvalidateAll();
                return true;
            }
            var resolveInvalidationResult = await Task.Run(() =>
                    resolveInvalidationRegistry.ResolveSelectedTargetEvidence(
                        TenantId,
                        resolveInvalidationEntry.SessionId,
                        resolveInvalidationHandle,
                        ResolveInvalidationGate))
                .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(
                SelectedTargetEvidenceResolutionStatus.ForbiddenAssembly,
                resolveInvalidationResult.Status);
            Assert.Null(resolveInvalidationResult.Evidence);
            Assert.Equal(3, resolveInvalidationGateCalls);
            Assert.Null(resolveInvalidationEntry.ResolveSelectedTargetEvidence(
                resolveInvalidationHandle));
        }
    }

    [Fact]
    public async Task PersistenceRestore_StartsWithZeroHandlesAndNeverPersistsHandleOrSensitiveEvidence()
    {
        const string oldHandle = "dgeh_16161616161616161616161616161616";
        const string sensitiveMarker = "selected-target-sensitive-marker-20as";
        const string mutatedMarker = "selected-target-mutated-source-20as";
        var now = new DateTimeOffset(2026, 7, 25, 13, 0, 0, TimeSpan.Zero);
        var time = new FixedTimeProvider(now);
        var options = new DynamicGraphCompositionServerOptions
        {
            TenantId = TenantId,
            MaxSessions = 4,
            MaxConcurrentCompositions = 1,
            MaxConcurrentRuns = 1,
            SessionTtl = TimeSpan.FromMinutes(15)
        };
        using var persistence = new DynamicGraphCompositionSessionPersistence(
            options,
            enabled: true);
        var association = ValidAssociation();
        var sourceRows = RequestedRowIdsBackingArray(association);
        sourceRows[0] = sensitiveMarker;
        var writerInvoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(association)
        };
        string sessionId;
        PersistedCompositionSession checkpoint;
        using (var writer = new DynamicGraphCompositionSessionRegistry(
                   options,
                   persistence,
                   time,
                   writerInvoker,
                   () => oldHandle))
        {
            sessionId = await PublishSessionAsync(writer, "persistence-evidence");
            var admission = writer.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None);
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, admission.Status);
            Assert.True(admission.Result!.IsSuccess, admission.Result.ErrorMessage);
            Assert.Equal(oldHandle, admission.Result.Value!.HandleId);
            sourceRows[0] = mutatedMarker;

            var checkpoints = persistence.PersistedSnapshots;
            checkpoint = Assert.Single(checkpoints);
            Assert.Equal(sessionId, checkpoint.SessionId);
            var checkpointText = JsonSerializer.Serialize(checkpoints);
            var checkpointBytes = Encoding.UTF8.GetBytes(checkpointText);
            var checkpointUtf8 = Encoding.UTF8.GetString(checkpointBytes);
            Assert.DoesNotContain(oldHandle, checkpointText, StringComparison.Ordinal);
            Assert.DoesNotContain(sensitiveMarker, checkpointText, StringComparison.Ordinal);
            Assert.DoesNotContain(mutatedMarker, checkpointText, StringComparison.Ordinal);
            Assert.DoesNotContain("SelectedTargetEvidence", checkpointText, StringComparison.Ordinal);
            Assert.DoesNotContain(
                "EdgeAuthoritySelectionQualityEvidence",
                checkpointText,
                StringComparison.Ordinal);
            Assert.Equal(checkpointText, checkpointUtf8, StringComparer.Ordinal);
        }

        var productionRoundTrip = RoundTripThroughProductionPersistence(checkpoint);
        Assert.Equal(
            DynamicGraphCompositionSessionPersistence.CheckpointSchema,
            productionRoundTrip.Schema);
        using (var checkpointDocument = JsonDocument.Parse(productionRoundTrip.SerializedCheckpoint))
        {
            Assert.Equal(
                DynamicGraphCompositionSessionPersistence.CheckpointSchema,
                checkpointDocument.RootElement.GetProperty("schema").GetString());
            Assert.Matches(
                "^[0-9A-F]{64}$",
                checkpointDocument.RootElement.GetProperty("hmac_sha256").GetString()!);
        }
        Assert.DoesNotContain(oldHandle, productionRoundTrip.SerializedCheckpoint, StringComparison.Ordinal);
        Assert.DoesNotContain(sensitiveMarker, productionRoundTrip.SerializedCheckpoint, StringComparison.Ordinal);
        Assert.DoesNotContain(mutatedMarker, productionRoundTrip.SerializedCheckpoint, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "SelectedTargetEvidence",
            productionRoundTrip.SerializedCheckpoint,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "EdgeAuthoritySelectionQualityEvidence",
            productionRoundTrip.SerializedCheckpoint,
            StringComparison.Ordinal);

        using var restoredPersistence = new DynamicGraphCompositionSessionPersistence(
            options,
            enabled: true);
        restoredPersistence.PersistNew(checkpoint with
        {
            Capture = null,
            AuthenticatedCheckpoint = productionRoundTrip.AuthenticatedCheckpoint
        });
        var authenticatedCheckpointText = JsonSerializer.Serialize(
            restoredPersistence.PersistedSnapshots);
        Assert.DoesNotContain(oldHandle, authenticatedCheckpointText, StringComparison.Ordinal);
        Assert.DoesNotContain(sensitiveMarker, authenticatedCheckpointText, StringComparison.Ordinal);
        Assert.DoesNotContain(mutatedMarker, authenticatedCheckpointText, StringComparison.Ordinal);

        var restoredAssociation = ValidAssociation();
        var restoredInvoker = new RecordingEvidenceInvoker
        {
            Handler = (_, _, _) => OperationResult<
                DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
                    restoredAssociation)
        };
        var restoredHandleSequence = 0;
        var restoredRunSession = new SuccessfulRunSession();
        using var reader = new DynamicGraphCompositionSessionRegistry(
            options,
            restoredPersistence,
            time,
            restoredInvoker,
            () => $"dgeh_{Interlocked.Increment(ref restoredHandleSequence):x32}");
        reader.Restore(profile =>
            (new CompositionEvidenceHttpResponse(
                 profile.ProfileId,
                 1,
                 "LogicalRegistryProjection",
                 profile.NodeDefinitions.Length,
                 profile.EdgeBindings.Length,
                 true),
              restoredRunSession));

        var recaptured = restoredRunSession.CaptureCheckpoint(checkpoint.Profile);
        Assert.True(recaptured.IsSuccess, recaptured.ErrorMessage);
        AssertCheckpointMatchesAuthenticated(
            recaptured.Value!,
            checkpoint.Profile,
            productionRoundTrip.AuthenticatedCheckpoint);
        var current = restoredRunSession.ReadCurrent();
        Assert.True(current.IsSuccess, current.ErrorMessage);
        AssertCurrentMatchesAuthenticated(
            current.Value!,
            checkpoint.Profile,
            productionRoundTrip.AuthenticatedCheckpoint);

        var restoredSession = reader.Get(TenantId, sessionId);
        Assert.Equal(SessionLookupStatus.Found, restoredSession.Status);
        Assert.NotNull(restoredSession.Session);
        var oldResolution = reader.ResolveSelectedTargetEvidence(
            TenantId,
            sessionId,
            oldHandle,
            () => true);
        Assert.Equal(SelectedTargetEvidenceResolutionStatus.Unknown, oldResolution.Status);
        Assert.Null(oldResolution.Evidence);

        for (var index = 0; index < 16; index++)
        {
            var admitted = reader.AdmitSelectedTargetEvidence(
                TenantId,
                sessionId,
                null,
                null,
                null,
                () => true,
                CancellationToken.None);
            Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, admitted.Status);
            Assert.NotNull(admitted.Result);
            Assert.True(admitted.Result.IsSuccess, admitted.Result.ErrorMessage);
        }
        var capacity = reader.AdmitSelectedTargetEvidence(
            TenantId,
            sessionId,
            null,
            null,
            null,
            () => true,
            CancellationToken.None);
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, capacity.Status);
        Assert.Null(capacity.Result);
        Assert.Equal(16, restoredInvoker.CallCount);
        Assert.Equal(16, restoredHandleSequence);
        Assert.Single(restoredPersistence.PersistedSnapshots);
    }

    private static DynamicGraphCompositionSessionRegistry CreateRegistry(
        IDynamicGraphSelectedTargetEvidenceInvoker invoker,
        Func<string>? handleFactory = null,
        TimeProvider? timeProvider = null)
    {
        var options = new DynamicGraphCompositionServerOptions
        {
            TenantId = TenantId,
            MaxSessions = 4,
            MaxConcurrentCompositions = 1,
            MaxConcurrentRuns = 1,
            SessionTtl = TimeSpan.FromMinutes(15)
        };
        return new DynamicGraphCompositionSessionRegistry(
            options,
            new DynamicGraphCompositionSessionPersistence(options),
            timeProvider ?? TimeProvider.System,
            invoker,
            handleFactory ?? (() => "dgeh_ffffffffffffffffffffffffffffffff"));
    }

    private static ProductionPersistenceRoundTrip RoundTripThroughProductionPersistence(
        PersistedCompositionSession checkpoint)
    {
        var capture = Assert.IsType<DynamicGraphLogicalOrchestratorCheckpointCapture>(
            checkpoint.Capture);
        AssertValidatorCleanInitialCapture(capture, checkpoint.Profile);
        Assert.Equal(checkpoint.Profile.ProfileId, checkpoint.Evidence.ProfileId);
        Assert.Equal(capture.Revision, checkpoint.Evidence.Revision);
        Assert.Equal(capture.SourceKind.ToString(), checkpoint.Evidence.SourceKind);
        Assert.Equal(capture.NodeCount, checkpoint.Evidence.NodeCount);
        Assert.Equal(capture.EdgeCount, checkpoint.Evidence.EdgeCount);
        Assert.Equal(
            capture.InitialRevisionAdvanced,
            checkpoint.Evidence.InitialRevisionAdvanced);

        var root = CreatePrivateProductionRoundTripRoot(
            $"xiigen-evidence-handle-production-roundtrip-{Guid.NewGuid():N}");
        var stateDirectory = Path.Combine(root, "state");
        var options = new ProductionServerOptions
        {
            TenantId = TenantId,
            ReadyFile = Path.Combine(root, "ready.json"),
            StateDirectory = stateDirectory,
            MaxSessions = 4,
            MaxConcurrentCompositions = 1,
            MaxConcurrentRuns = 1,
            SessionTtl = TimeSpan.FromMinutes(15)
        };

        try
        {
            var assembly = typeof(ProductionPersistence).Assembly;
            var persistenceType = typeof(ProductionPersistence);
            var schema = Assert.IsType<string>(
                Assert.IsAssignableFrom<FieldInfo>(persistenceType.GetField(
                        "CheckpointSchema",
                        BindingFlags.Static | BindingFlags.NonPublic))
                    .GetRawConstantValue());
            var deriveIndex = Assert.IsAssignableFrom<MethodInfo>(persistenceType.GetMethod(
                "DeriveIndex",
                BindingFlags.Instance | BindingFlags.NonPublic));
            var persistNew = Assert.IsAssignableFrom<MethodInfo>(persistenceType.GetMethod(
                "PersistNew",
                BindingFlags.Instance | BindingFlags.NonPublic));
            var readAll = Assert.IsAssignableFrom<MethodInfo>(persistenceType.GetMethod(
                "ReadAll",
                BindingFlags.Instance | BindingFlags.NonPublic));

            var policyType = Assert.IsAssignableFrom<Type>(assembly.GetType(
                "LlmFromScratch.DynamicGraphCompositionLiveLocalServer.DynamicGraphCompositionProfilePolicy"));
            var admit = Assert.IsAssignableFrom<MethodInfo>(policyType.GetMethod(
                "Admit",
                BindingFlags.Static | BindingFlags.NonPublic));
            var admission = admit.Invoke(null, [checkpoint.Profile, TenantId]);
            Assert.NotNull(admission);
            Assert.True(
                Assert.IsType<bool>(admission.GetType().GetProperty("IsAccepted")!.GetValue(admission)),
                Assert.IsType<string>(admission.GetType().GetProperty("Message")!.GetValue(admission)));
            var admittedProfile = Assert.IsType<DynamicGraphLogicalOrchestratorBootstrapProfile>(
                admission.GetType().GetProperty("Profile")!.GetValue(admission));
            var canonicalDigest = Assert.IsType<string>(
                admission.GetType().GetProperty("CanonicalDigest")!.GetValue(admission));

            object InvokeIndex(ProductionPersistence persistence, params string[] components)
            {
                var index = deriveIndex.Invoke(persistence, [components]);
                Assert.NotNull(index);
                return index;
            }

            using (var writer = new ProductionPersistence(options))
            {
                var persistedType = Assert.IsAssignableFrom<Type>(assembly.GetType(
                    "LlmFromScratch.DynamicGraphCompositionLiveLocalServer.PersistedCompositionSession"));
                var persistedConstructor = Assert.Single(
                    persistedType.GetConstructors(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                    constructor => constructor.GetParameters().Length == 11);
                var productionEvidence = new ProductionCompositionEvidence(
                    checkpoint.Evidence.ProfileId,
                    checkpoint.Evidence.Revision,
                    checkpoint.Evidence.SourceKind,
                    checkpoint.Evidence.NodeCount,
                    checkpoint.Evidence.EdgeCount,
                    checkpoint.Evidence.InitialRevisionAdvanced);
                var productionSession = persistedConstructor.Invoke(
                [
                    schema,
                    checkpoint.SessionId,
                    InvokeIndex(writer, "tenant", TenantId),
                    InvokeIndex(writer, "idempotency", TenantId, "evidence-handle-roundtrip"),
                    canonicalDigest,
                    checkpoint.CreatedAtUtc,
                    checkpoint.ExpiresAtUtc,
                    admittedProfile,
                    productionEvidence,
                    capture,
                    null
                ]);
                persistNew.Invoke(writer, [productionSession]);
            }

            var checkpointPath = Path.Combine(
                stateDirectory,
                "sessions",
                $"{checkpoint.SessionId}.json");
            var serializedCheckpoint = File.ReadAllText(checkpointPath, Encoding.UTF8);
            DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint authenticatedCheckpoint;
            using (var reader = new ProductionPersistence(options))
            {
                var restoredEnumerable = Assert.IsAssignableFrom<System.Collections.IEnumerable>(
                    readAll.Invoke(reader, [options.MaxSessions]));
                var restored = Assert.Single(restoredEnumerable.Cast<object>());
                authenticatedCheckpoint = Assert.IsType<
                    DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint>(
                    restored.GetType().GetProperty("AuthenticatedCheckpoint")!.GetValue(restored));
                Assert.Null(restored.GetType().GetProperty("Capture")!.GetValue(restored));
            }

            return new ProductionPersistenceRoundTrip(
                schema,
                serializedCheckpoint,
                authenticatedCheckpoint);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static string CreatePrivateProductionRoundTripRoot(string directoryName)
    {
        var localApplicationData =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var baseDirectory = string.IsNullOrWhiteSpace(localApplicationData)
            ? Path.GetTempPath()
            : localApplicationData;
        var root = Path.Combine(baseDirectory, directoryName);
        Directory.CreateDirectory(root);

        if (OperatingSystem.IsWindows())
        {
            // Production persistence refuses to write state under a directory that inherited
            // broad user access. The test creates an owner-only root first, so the assertion
            // checks checkpoint contents instead of failing on the machine's default ACLs.
            using var identity = WindowsIdentity.GetCurrent();
            var sid = identity.User!;
            var security = new DirectorySecurity();
            security.SetOwner(sid);
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            security.AddAccessRule(new FileSystemAccessRule(
                sid,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));
            FileSystemAclExtensions.SetAccessControl(new DirectoryInfo(root), security);
            AssertPrivateWindowsProductionRoundTripRoot(root, sid);
        }
        else
        {
            // Unix runners need the same owner-only contract; otherwise this test would
            // pass or fail because of the temp directory defaults rather than persistence.
            File.SetUnixFileMode(
                root,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            Assert.Equal(
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute,
                File.GetUnixFileMode(root));
        }

        return root;
    }

    [SupportedOSPlatform("windows")]
    private static void AssertPrivateWindowsProductionRoundTripRoot(
        string root,
        SecurityIdentifier ownerSid)
    {
        if (!OperatingSystem.IsWindows())
            return;

        var security = FileSystemAclExtensions.GetAccessControl(new DirectoryInfo(root));
        Assert.True(security.AreAccessRulesProtected);
        var actualOwnerSid = Assert.IsType<SecurityIdentifier>(
            security.GetOwner(typeof(SecurityIdentifier)));
        Assert.Equal(ownerSid.Value, actualOwnerSid.Value);

        var allowedIdentities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ownerSid.Value,
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null).Value,
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Value,
        };
        var rules = security
            .GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier))
            .Cast<FileSystemAccessRule>()
            .ToArray();
        Assert.NotEmpty(rules);
        Assert.All(rules, rule =>
        {
            var sid = Assert.IsType<SecurityIdentifier>(rule.IdentityReference);
            Assert.Contains(sid.Value, allowedIdentities);
            Assert.Equal(AccessControlType.Allow, rule.AccessControlType);
            Assert.False(rule.IsInherited);
        });

        // The root must pass the same owner-only shape production expects for child files,
        // so the round trip cannot become green by inheriting broad profile permissions.
        Assert.Contains(
            rules,
            rule => rule.IdentityReference is SecurityIdentifier sid
                    && string.Equals(sid.Value, ownerSid.Value, StringComparison.OrdinalIgnoreCase)
                    && rule.AccessControlType == AccessControlType.Allow
                    && (rule.FileSystemRights & FileSystemRights.FullControl) == FileSystemRights.FullControl
                    && (rule.InheritanceFlags
                        & (InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit))
                    == (InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit));
    }

    private static void AssertValidatorCleanInitialCapture(
        DynamicGraphLogicalOrchestratorCheckpointCapture capture,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
    {
        Assert.Equal(1, capture.Revision);
        Assert.Equal(DynamicGraphBuildSourceKind.LogicalRegistryProjection, capture.SourceKind);
        Assert.True(capture.InitialRevisionAdvanced);
        Assert.Equal(profile.SignalId, capture.SignalId);
        Assert.Equal(profile.SourceRef, capture.SourceRef);
        Assert.Equal(profile.TraceRef, capture.TraceRef);
        Assert.Equal(profile.TenantId, capture.Topology.TenantId);
        Assert.Equal(profile.DomainTag, capture.Topology.DomainTag);
        Assert.Equal(profile.ContextId, capture.Topology.ContextId);
        Assert.Empty(profile.NodeDefinitions);
        Assert.Empty(profile.EdgeBindings);
        Assert.Empty(capture.Topology.Nodes);
        Assert.Empty(capture.Topology.Edges);
        Assert.Empty(capture.Topology.Order);
        Assert.Equal(profile.NodeDefinitions.Length, capture.NodeCount);
        Assert.Equal(profile.EdgeBindings.Length, capture.EdgeCount);
        Assert.Equal(capture.Topology.Nodes.Count, capture.NodeCount);
        Assert.Equal(capture.Topology.Edges.Count, capture.EdgeCount);
        Assert.Equal(profile.Budget.MaxNodes, capture.BudgetProof.MaxNodes);
        Assert.Equal(profile.Budget.MaxEdges, capture.BudgetProof.MaxEdges);
        Assert.Equal(profile.Budget.MaxDepth, capture.BudgetProof.MaxDepth);
        Assert.Equal(
            profile.Budget.MaxParallelBranches,
            capture.BudgetProof.MaxParallelBranches);
        Assert.Equal(0, capture.BudgetProof.ActualNodes);
        Assert.Equal(0, capture.BudgetProof.ActualEdges);
        Assert.Equal(0, capture.BudgetProof.ActualDepth);
        Assert.Equal(1, capture.BudgetProof.ActualParallelBranches);
        Assert.True(capture.BudgetProof.WithinBudget);
        Assert.Empty(capture.BudgetProof.ExceededDimensions);
        Assert.Equal(profile.SignalId, capture.MutationEvidence.SignalId);
        Assert.Equal(
            DynamicGraphBuildSourceKind.LogicalRegistryProjection,
            capture.MutationEvidence.SourceKind);
        Assert.Equal(profile.SourceRef, capture.MutationEvidence.SourceRef);
        Assert.Equal(profile.TraceRef, capture.MutationEvidence.TraceRef);
        Assert.Empty(capture.MutationEvidence.ProposedAddedNodeIds);
        Assert.Empty(capture.MutationEvidence.ProposedRemovedNodeIds);
        Assert.Empty(capture.MutationEvidence.ProposedUpdatedNodeIds);
        Assert.Empty(capture.MutationEvidence.ProposedAddedEdgeIds);
        Assert.Empty(capture.MutationEvidence.ProposedRemovedEdgeIds);
        Assert.Empty(capture.MutationEvidence.ProposedUpdatedEdgeIds);
        Assert.Empty(capture.MutationEvidence.ProposedReorderedNodeIds);
        Assert.True(capture.MutationEvidence.MutationApplied);
    }

    private static void AssertCheckpointMatchesAuthenticated(
        DynamicGraphLogicalOrchestratorCheckpointCapture capture,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint)
    {
        AssertValidatorCleanInitialCapture(capture, profile);
        Assert.Equal(checkpoint.Revision, capture.Revision);
        Assert.Equal(checkpoint.SourceKind, capture.SourceKind);
        Assert.Equal(checkpoint.InitialRevisionAdvanced, capture.InitialRevisionAdvanced);
        Assert.Equal(checkpoint.BudgetProof, capture.BudgetProof);
        Assert.Equal(checkpoint.MutationEvidence, capture.MutationEvidence);
    }

    private static void AssertCurrentMatchesAuthenticated(
        DynamicGraphLogicalOrchestratorSessionResult current,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint)
    {
        Assert.Equal(profile.ProfileId, current.Descriptor.ProfileId);
        Assert.Equal(profile.TenantId, current.Descriptor.TenantId);
        Assert.Equal(profile.DomainTag, current.Descriptor.DomainTag);
        Assert.Equal(profile.ContextId, current.Descriptor.ContextId);
        Assert.Equal(profile.SignalId, current.SignalId);
        Assert.Equal(profile.NodeDefinitions.Length, current.NodeCount);
        Assert.Equal(profile.EdgeBindings.Length, current.EdgeCount);
        Assert.Equal(checkpoint.Revision, current.Revision);
        Assert.Equal(checkpoint.SourceKind, current.SourceKind);
        Assert.Equal(checkpoint.InitialRevisionAdvanced, current.InitialRevisionAdvanced);
    }

    private sealed record ProductionPersistenceRoundTrip(
        string Schema,
        string SerializedCheckpoint,
        DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint AuthenticatedCheckpoint);

    private static async Task<string> PublishSessionAsync(
        DynamicGraphCompositionSessionRegistry registry,
        string suffix)
        => (await PublishSessionEntryAsync(registry, suffix)).SessionId;

    private static async Task<DynamicGraphCompositionSessionRegistry.SessionEntry>
        PublishSessionEntryAsync(
            DynamicGraphCompositionSessionRegistry registry,
            string suffix)
    {
        var profile = CreateProfile($"profile-{suffix}");
        var creation = await registry.CreateAsync(
            TenantId,
            $"idempotency-{suffix}",
            new string('A', 64),
            profile,
            _ => Task.FromResult<(
                CompositionEvidenceHttpResponse Evidence,
                IDynamicGraphLogicalOrchestratorRunSession RunSession)>(
                (new CompositionEvidenceHttpResponse(
                    $"profile-{suffix}",
                    1,
                    "LogicalRegistryProjection",
                    profile.NodeDefinitions.Length,
                    profile.EdgeBindings.Length,
                    true),
                 new SuccessfulRunSession())),
            () => true,
            CancellationToken.None);

        Assert.Equal(SessionCreationStatus.Created, creation.Status);
        return Assert.IsType<DynamicGraphCompositionSessionRegistry.SessionEntry>(creation.Session);
    }

    private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfile(string profileId)
        => new(
            profileId,
            TenantId,
            "domain-a",
            "context-a",
            "signal-a",
            "source-a",
            "trace-a",
            [],
            [],
            new TopologyBuildBudget(1, 1, 1, 1));

    private static void AssertRequiredFailure(
        SelectedTargetEvidenceAdmissionResult admission,
        string expectedCode,
        string expectedMessage)
    {
        Assert.Equal(SelectedTargetEvidenceAdmissionStatus.Completed, admission.Status);
        Assert.NotNull(admission.Result);
        Assert.False(admission.Result.IsSuccess);
        Assert.Equal(expectedCode, admission.Result.ErrorCode);
        Assert.Equal(expectedMessage, admission.Result.ErrorMessage);
    }

    private static string[] RequestedRowIdsBackingArray(
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement association)
    {
        var selection = GetRequiredField(
            association,
            "_edgeAuthoritySelectionQualityEvidence");
        var novelty = GetRequiredField(selection, "_noveltyEvaluation");
        return Assert.IsType<string[]>(GetRequiredField(novelty, "_requestedRowIds"));
    }

    private static IReadOnlyCollection<string> RetiredSelectedTargetEvidenceHandles(
        DynamicGraphCompositionSessionRegistry.SessionEntry entry)
    {
        var field = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphCompositionSessionRegistry.SessionEntry).GetField(
                "_retiredSelectedTargetEvidenceHandleIds",
                BindingFlags.Instance | BindingFlags.NonPublic));
        return Assert.IsType<HashSet<string>>(field.GetValue(entry));
    }

    private static object GetRequiredField(object owner, string fieldName)
    {
        var field = owner.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        var value = field.GetValue(owner);
        Assert.NotNull(value);
        return value;
    }

    private static async Task WaitForDisposedStateAsync(
        DynamicGraphCompositionSessionRegistry registry)
    {
        var disposedField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphCompositionSessionRegistry).GetField(
                "_disposed",
                BindingFlags.Instance | BindingFlags.NonPublic));
        var observed = await Task.Run(() => SpinWait.SpinUntil(
            () => disposedField.GetValue(registry) is int state && state == 1,
            TimeSpan.FromSeconds(10)));
        Assert.True(observed, "Dispose did not enter its lifecycle wait.");
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

    private sealed class BlockingEvidenceInvoker(
        int expectedCalls,
        Func<OperationResult<
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>> resultFactory)
        : IDynamicGraphSelectedTargetEvidenceInvoker
    {
        private readonly TaskCompletionSource _allExpectedCallsEntered =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _release =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _callCount;

        internal Task AllExpectedCallsEntered => _allExpectedCallsEntered.Task;
        internal int CallCount => Volatile.Read(ref _callCount);

        internal void ReleaseAll() => _release.TrySetResult();

        public OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
            Invoke(
                DynamicGraphMeasurementTarget? target,
                EdgeAuthorityExecutionResult? authorityResult,
                DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest)
        {
            var call = Interlocked.Increment(ref _callCount);
            if (call == expectedCalls)
                _allExpectedCallsEntered.TrySetResult();
            if (call > expectedCalls)
                throw new InvalidOperationException("More blocking invocations entered than expected.");

            _release.Task.GetAwaiter().GetResult();
            return resultFactory();
        }
    }

    private sealed class RecordingEvidenceInvoker : IDynamicGraphSelectedTargetEvidenceInvoker
    {
        internal Func<
            DynamicGraphMeasurementTarget?,
            EdgeAuthorityExecutionResult?,
            DynamicGraphEdgeAuthorityNoveltyEvaluationRequest?,
            OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>?>
            Handler { get; set; } = (_, _, _) => throw new InvalidOperationException();

        internal int CallCount { get; private set; }
        internal DynamicGraphMeasurementTarget? LastTarget { get; private set; }
        internal EdgeAuthorityExecutionResult? LastAuthorityResult { get; private set; }
        internal DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? LastNoveltyRequest { get; private set; }

        public OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
            Invoke(
                DynamicGraphMeasurementTarget? target,
                EdgeAuthorityExecutionResult? authorityResult,
                DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest)
        {
            CallCount++;
            LastTarget = target;
            LastAuthorityResult = authorityResult;
            LastNoveltyRequest = noveltyRequest;
            return Handler(target, authorityResult, noveltyRequest)!;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class MutableTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        internal void Advance(TimeSpan duration) => _now = _now.Add(duration);
    }

    private sealed class SuccessfulRunSession :
        IDynamicGraphLogicalOrchestratorRunSession,
        IDynamicGraphLogicalOrchestratorCompositionSession,
        IDynamicGraphLogicalOrchestratorCheckpointSession
    {
        private DynamicGraphLogicalOrchestratorBootstrapProfile? _profile;
        private DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint? _checkpoint;
        private DynamicGraphLogicalOrchestratorCheckpointCapture? _capture;

        public DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor
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

        public OperationResult<DynamicGraphLogicalOrchestratorSessionResult> ReadCurrent()
        {
            var profile = Assert.IsType<DynamicGraphLogicalOrchestratorBootstrapProfile>(_profile);
            var capture = Assert.IsType<DynamicGraphLogicalOrchestratorCheckpointCapture>(_capture);
            return OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Ok(
                new DynamicGraphLogicalOrchestratorSessionResult(
                    Descriptor,
                    capture.Revision,
                    profile.SignalId,
                    capture.SourceKind,
                    profile.NodeDefinitions.Length,
                    profile.EdgeBindings.Length,
                    capture.InitialRevisionAdvanced));
        }

        public Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
            DynamicGraphLogicalOrchestratorRunRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Ok(
                new DynamicGraphLogicalOrchestratorRunResult(
                    "scenario",
                    "flow",
                    "PASS",
                    "ok",
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    [])));

        public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            CaptureCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile)
        {
            _profile = expectedCurrentProfile;
            var captured = CaptureDeterministicCheckpoint(expectedCurrentProfile, _checkpoint);
            if (captured.IsSuccess)
                _capture = captured.Value;
            return captured;
        }

        public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            RestoreCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile,
                DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint)
        {
            _profile = expectedCurrentProfile;
            _checkpoint = checkpoint;
            var restored = CaptureDeterministicCheckpoint(expectedCurrentProfile, checkpoint);
            if (restored.IsSuccess)
                _capture = restored.Value;
            return restored;
        }

        private static OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            CaptureDeterministicCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile,
                DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint? checkpoint)
        {
            var budgetProof = checkpoint?.BudgetProof ?? new TopologyBudgetProof(
                expectedCurrentProfile.Budget.MaxNodes,
                expectedCurrentProfile.NodeDefinitions.Length,
                expectedCurrentProfile.Budget.MaxEdges,
                expectedCurrentProfile.EdgeBindings.Length,
                expectedCurrentProfile.Budget.MaxDepth,
                ActualDepth: 0,
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
                    new DynamicGraphTopology(
                        expectedCurrentProfile.TenantId,
                        expectedCurrentProfile.DomainTag,
                        expectedCurrentProfile.ContextId,
                        [],
                        [],
                        []),
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
    }
}
