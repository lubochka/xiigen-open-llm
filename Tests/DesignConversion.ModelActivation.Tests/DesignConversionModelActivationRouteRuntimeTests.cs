// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using ModelServer.DesignConversion;
using DesignDomain = DesignConversionDomain.DesignConversionDomain;

public sealed class DesignConversionModelActivationRouteRuntimeTests
{
    private const string TenantA = "tenant-design-conversion-route-alpha";
    private const string TenantB = "tenant-design-conversion-route-beta";
    private const string LocalToken = "route-runtime-local-token";
    private const string PreviousLocalToken = "route-runtime-previous-token";
    private const string UnknownLocalToken = "route-runtime-unknown-token";
    private const string RouteAdmissionCurrentToken = "route-runtime-admission-current-token";
    private const string RouteAdmissionPreviousToken = "route-runtime-admission-previous-token";
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions PromotionJournalJsonOptions = new(
        DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    [Fact]
    public async Task ModelActivationRoute_UnauthorizedMissingStartupConfigAndTokenRotationReturnBoundedResponses()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "unauthorized", batchCount: 1);
        var readyRegistry = CreateRegistry(root);

        var unauthorized = await InvokeActivationAsync(
            readyRegistry,
            ActivateRequest(TenantA, "activate:unauthorized", 0, candidate),
            token: null);

        Assert.Equal(401, unauthorized.StatusCode);
        AssertErrorCode(unauthorized.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_REQUIRED");
        Assert.DoesNotContain(LocalToken, unauthorized.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(root, unauthorized.BodyJson, StringComparison.OrdinalIgnoreCase);

        var nonReadyRegistry = CreateRegistry(root, configureActivation: false);
        var nonReady = await InvokeActivationAsync(
            nonReadyRegistry,
            ActivateRequest(TenantA, "activate:not-ready", 0, candidate),
            token: LocalToken);

        Assert.Equal(503, nonReady.StatusCode);
        AssertErrorCode(nonReady.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_STATE_ROOT_NOT_CONFIGURED");
        using var document = JsonDocument.Parse(nonReady.BodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal("bounded_non_ready", metadata.GetProperty("model_activation_route_startup_configuration_status").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("private_file_paths_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("secret_values_included").GetString());
        Assert.DoesNotContain(LocalToken, nonReady.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(root, nonReady.BodyJson, StringComparison.OrdinalIgnoreCase);

        var rotationNow = DateTimeOffset.Parse("2026-07-10T10:00:00Z");
        var previousExpiresAt = rotationNow.AddMinutes(5);
        var rotationOptions = new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
            LocalToken,
            PreviousLocalToken,
            previousExpiresAt);
        var rotationRegistry = CreateRegistry(
            root,
            utcNow: () => rotationNow,
            modelLifecycleTokenKeyRing: rotationOptions);

        var currentAccepted = await InvokeActivationAsync(
            rotationRegistry,
            ActivateRequest(TenantA, "activate:rotation-current", 0, candidate),
            token: LocalToken);
        AssertSuccessActivation(currentAccepted.BodyJson, candidate.Model.ModelId, expectedGeneration: 1, replay: false);
        AssertDoesNotExposeLifecycleTokens(currentAccepted.BodyJson);

        var previousAcceptedBeforeExpiry = await InvokeActivationAsync(
            rotationRegistry,
            ReadActiveRequest(TenantA),
            token: PreviousLocalToken);
        Assert.Equal(200, previousAcceptedBeforeExpiry.StatusCode);
        AssertDoesNotExposeLifecycleTokens(previousAcceptedBeforeExpiry.BodyJson);

        var unknownRejected = await InvokeActivationAsync(
            rotationRegistry,
            ReadActiveRequest(TenantA),
            token: UnknownLocalToken);
        Assert.Equal(403, unknownRejected.StatusCode);
        AssertErrorCode(unknownRejected.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_INVALID");
        AssertDoesNotExposeLifecycleTokens(unknownRejected.BodyJson);

        var expiryRegistry = CreateRegistry(
            root,
            utcNow: () => previousExpiresAt,
            modelLifecycleTokenKeyRing: rotationOptions);
        var previousRejectedAtExpiry = await InvokeActivationAsync(
            expiryRegistry,
            ReadActiveRequest(TenantA),
            token: PreviousLocalToken);
        Assert.Equal(403, previousRejectedAtExpiry.StatusCode);
        AssertErrorCode(previousRejectedAtExpiry.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_INVALID");
        AssertDoesNotExposeLifecycleTokens(previousRejectedAtExpiry.BodyJson);

        var afterExpiryRegistry = CreateRegistry(
            root,
            utcNow: () => previousExpiresAt.AddTicks(1),
            modelLifecycleTokenKeyRing: rotationOptions);
        var previousRejectedAfterExpiry = await InvokeActivationAsync(
            afterExpiryRegistry,
            ReadActiveRequest(TenantA),
            token: PreviousLocalToken);
        Assert.Equal(403, previousRejectedAfterExpiry.StatusCode);
        AssertErrorCode(previousRejectedAfterExpiry.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_INVALID");
        AssertDoesNotExposeLifecycleTokens(previousRejectedAfterExpiry.BodyJson);

        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                LocalToken,
                PreviousLocalToken),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_EXPIRATION_REQUIRED",
            LocalToken,
            PreviousLocalToken);
        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                LocalToken,
                previousTokenExpiresAtUtc: previousExpiresAt),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_REQUIRED",
            LocalToken);
        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(string.Empty),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_REQUIRED");
        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions("short"),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_INVALID",
            "short");
        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                LocalToken,
                LocalToken,
                previousExpiresAt),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_PREVIOUS_MATCH",
            LocalToken);
        await AssertInvalidKeyRingConfigReturnsNonReadyAsync(
            root,
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                LocalToken,
                "route-runtime-\u0001previous-token",
                previousExpiresAt),
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_INVALID",
            LocalToken,
            "route-runtime-\u0001previous-token");
    }

    [Fact]
    public async Task ModelActivationRoute_CrossTenantCandidateReturnsConflict()
    {
        var root = NewRoot();
        var tenantARecord = await BuildActivationRecordAsync(root, TenantA, "tenant-a", batchCount: 1);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantB, "activate:cross-tenant", 0, tenantARecord));

        Assert.Equal(409, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_TENANT_SCOPE_MISMATCH");
        Assert.DoesNotContain(root, response.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_BodyTenantWithoutTrustedAdmissionDoesNotSelectActiveModel()
    {
        var root = NewRoot();
        var modelB = await BuildActivationRecordAsync(root, TenantB, "body-spoof-model-b", batchCount: 2);
        var registry = CreateRegistry(root);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantB, "activate:body-spoof-b", 0, modelB));
        AssertSuccessActivation(activated.BodyJson, modelB.Model.ModelId, expectedGeneration: 1, replay: false);

        var evaluated = await InvokeEvaluateAsync(
            registry,
            TenantB,
            "evaluate-body-only-spoof",
            routeAdmissionToken: null);

        Assert.Equal(200, evaluated.StatusCode);
        AssertEvaluateDidNotSelectActiveModel(evaluated.BodyJson, modelB.Model.ModelId);
        Assert.DoesNotContain(TenantB, evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, evaluated.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_DirectApiCallerTenantMetadataSpoofDoesNotSelectActiveModel()
    {
        var root = NewRoot();
        var modelB = await BuildActivationRecordAsync(root, TenantB, "direct-api-spoof-model-b", batchCount: 2);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal();
        var artifactDbRoot = ArtifactDbRoot(root);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var visualCostLedger = new JsonlDesignConversionVisualCostLedger(
            Path.Combine(root, "visual-cost-ledger"));
        var activationRegistry = new DesignConversionModelActivationRegistry(
            Path.Combine(root, "activation-state"),
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activationApi = new DesignConversionModelServerModelActivationApi(
            activationRegistry,
            LocalToken,
            artifactDb);

        var activated = await activationApi.ActivatePublishedModelAsync(
            new DesignConversionModelServerModelActivationAuthorization(
                TenantId: TenantB,
                RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
                Reason: "direct API tenant metadata spoof setup",
                IdempotencyKey: "activate:direct-api-spoof-b",
                ExpectedGeneration: 0,
                LocalAuthorizationToken: LocalToken),
            modelB,
            expectedModelVersion: modelB.Model.Version,
            requestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"));
        Assert.True(activated.IsSuccess, activated.ErrorMessage);

        var fallbackEvaluator = new DesignConversionInternalServiceThinClientJobEvaluator(composition.Service);
        var activatedEvaluator = new DesignConversionActivatedModelRuntimeEvaluator(
            fallbackEvaluator,
            activationRegistry,
            composition.RenderProvider,
            composition.ComparisonProvider,
            composition.VisualJudge);
        var api = new DesignConversionModelServerApi(
            activatedEvaluator,
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            artifactDb,
            trainingReadbackApi: new DesignConversionDpoUsefulnessApi(artifactDb),
            activatedModelRuntimeReadback: activatedEvaluator,
            visualCostLedger: visualCostLedger);

        var result = await api.SubmitAndEvaluateAsync(
            CreateEvaluateRequest(TenantB, "direct-api-body-spoof-b"),
            routeAdmissionContext: null);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var value = result.Value!;
        Assert.Contains(
            DesignConversionActivatedModelRuntimeEvaluator.NoActiveSelectionEvidence,
            value.EvidenceRefs);
        Assert.DoesNotContain(
            DesignConversionActivatedModelRuntimeEvaluator.ActiveSelectionEvidence,
            value.EvidenceRefs);
        Assert.Null(value.PromotedLocalModel);
        Assert.Equal(bool.FalseString, value.BoundaryMetadata["local_model_promotion_verified"]);
        Assert.Equal(bool.FalseString, value.BoundaryMetadata["local_promoted_checkpoint_loaded"]);
        Assert.False(value.BoundaryMetadata.ContainsKey("local_promoted_model_id"));

        var publicJson = JsonSerializer.Serialize(value, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(TenantB, publicJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.ModelId, publicJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, publicJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_AdmittedTenantMismatchRejectsBeforeRuntimeOrPersistence()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "mismatch-model-a", batchCount: 1);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:mismatch-a", 0, modelA));
        AssertSuccessActivation(activated.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);

        var evaluated = await InvokeEvaluateAsync(
            registry,
            TenantB,
            "evaluate-admitted-a-body-b");

        Assert.Equal(403, evaluated.StatusCode);
        AssertErrorCode(evaluated.BodyJson, "DESIGN_CONVERSION_ACTIVE_MODEL_TENANT_CONTEXT_MISMATCH");
        Assert.DoesNotContain(TenantA, evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantB, evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.ModelId, evaluated.BodyJson, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(evaluated.BodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_invoked").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_tenant_bound").GetString());
        Assert.Equal(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(TenantA),
            metadata.GetProperty("route_admission_tenant_id_ref").GetString());
    }

    [Fact]
    public async Task ModelActivationRuntime_ActivateBThenInferenceAndReadbackUseB()
    {
        var root = NewRoot();
        var modelB = await BuildActivationRecordAsync(root, TenantB, "model-b", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantB);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantB, "activate:b", 0, modelB));
        AssertSuccessActivation(activated.BodyJson, modelB.Model.ModelId, expectedGeneration: 1, replay: false);

        var evaluated = await InvokeEvaluateAsync(registry, TenantB, "evaluate-tenant-b");
        Assert.Equal(200, evaluated.StatusCode);
        AssertEvaluateSelectedModel(evaluated.BodyJson, modelB.Model.ModelId);
        Assert.DoesNotContain(TenantB, evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, evaluated.BodyJson, StringComparison.OrdinalIgnoreCase);

        var readback = await InvokeActivationAsync(
            registry,
            new DesignConversionModelServerModelActivationRequest(
                Action: DesignConversionModelServerModelActivationHandler.ActionReadActive,
                TenantId: TenantB));
        AssertSuccessActivation(readback.BodyJson, modelB.Model.ModelId, expectedGeneration: 1, replay: false);
        Assert.DoesNotContain(root, readback.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_SameActiveSelectionUsesCachedRuntimeAfterFreshLoadHashMismatch()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "snapshot-model-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "snapshot-model-b", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:snapshot-a", 0, modelA));
        AssertSuccessActivation(activated.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);
        var activationARef = ReadActiveActivationRef(activated.BodyJson);

        var cached = await InvokeEvaluateAsync(registry, TenantA, "evaluate-snapshot-cache-a");
        Assert.Equal(200, cached.StatusCode);
        AssertEvaluateSelectedModel(cached.BodyJson, modelA.Model.ModelId);
        AssertRuntimeSelection(cached.BodyJson, loadedFresh: true, preservedPrevious: false);
        AssertEvaluationPromotedModelIdentity(cached.BodyJson, modelA.Model.ModelId, modelA.Model.CheckpointSha256);

        var originalBytes = await File.ReadAllBytesAsync(modelA.Model.CheckpointPath);
        var replacementBytes = await File.ReadAllBytesAsync(modelB.Model.CheckpointPath);
        Assert.NotEqual(modelA.Model.ModelId, modelB.Model.ModelId);
        Assert.NotEqual(modelA.Model.CheckpointSha256, modelB.Model.CheckpointSha256);
        var readCount = 0;
        using (DesignConversionMachine.UseCheckpointByteReaderForCurrentAsyncFlow(
                   async (requestedPath, cancellationToken) =>
                   {
                       readCount++;
                       Assert.Equal(Path.GetFullPath(modelA.Model.CheckpointPath), Path.GetFullPath(requestedPath));
                       await File.WriteAllBytesAsync(requestedPath, replacementBytes, cancellationToken);
                       return originalBytes;
                   }))
        {
            var loadedFromSnapshot = await InvokeEvaluateAsync(registry, TenantA, "evaluate-snapshot-a-after-mutation");
            Assert.Equal(200, loadedFromSnapshot.StatusCode);
            AssertEvaluateSelectedModel(loadedFromSnapshot.BodyJson, modelA.Model.ModelId);
            AssertRuntimeSelection(loadedFromSnapshot.BodyJson, loadedFresh: true, preservedPrevious: false);
        }

        Assert.Equal(1, readCount);
        Assert.Equal(Sha256Bytes(replacementBytes), await ReadSha256Async(modelA.Model.CheckpointPath));

        var activeAfterReplacement = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        AssertSuccessActivation(activeAfterReplacement.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);
        Assert.Equal(activationARef, ReadActiveActivationRef(activeAfterReplacement.BodyJson));

        var preserved = await InvokeEvaluateAsync(registry, TenantA, "evaluate-snapshot-mismatch-preserves-cached-a");
        Assert.Equal(200, preserved.StatusCode);
        AssertEvaluateSelectedModel(preserved.BodyJson, modelA.Model.ModelId);
        AssertRuntimeSelection(preserved.BodyJson, loadedFresh: false, preservedPrevious: true);
        AssertEvaluationPromotedModelIdentity(preserved.BodyJson, modelA.Model.ModelId, modelA.Model.CheckpointSha256);
        Assert.Contains(
            DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
            preserved.BodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.ModelId, preserved.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, preserved.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.CheckpointPath, preserved.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, preserved.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_RollbackThenSubsequentInferenceUsesA()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "model-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "model-b", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:a", 0, modelA))).StatusCode);
        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:b", 1, modelB))).StatusCode);

        var beforeRollback = await InvokeEvaluateAsync(registry, TenantA, "evaluate-before-rollback");
        AssertEvaluateSelectedModel(beforeRollback.BodyJson, modelB.Model.ModelId);

        var rollback = await InvokeActivationAsync(
            registry,
            RollbackRequest(TenantA, "rollback:b-to-a", expectedGeneration: 2));
        AssertSuccessActivation(rollback.BodyJson, modelA.Model.ModelId, expectedGeneration: 3, replay: false);

        var afterRollback = await InvokeEvaluateAsync(registry, TenantA, "evaluate-after-rollback");
        Assert.Equal(200, afterRollback.StatusCode);
        AssertEvaluateSelectedModel(afterRollback.BodyJson, modelA.Model.ModelId);
    }

    [Fact]
    public async Task ModelActivationRuntime_RollbackToAThenCorruptAFailsClosedWithoutCachedB()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "rollback-corrupt-model-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "rollback-corrupt-model-b", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activatedA = await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:rollback-corrupt-a", 0, modelA));
        Assert.Equal(200, activatedA.StatusCode);
        var activationARef = ReadActiveActivationRef(activatedA.BodyJson);
        var activatedB = await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:rollback-corrupt-b", 1, modelB));
        Assert.Equal(200, activatedB.StatusCode);
        var cachedBActivationRef = ReadActiveActivationRef(activatedB.BodyJson);
        Assert.NotEqual(activationARef, cachedBActivationRef);
        Assert.NotEqual(modelA.Model.ModelId, modelB.Model.ModelId);
        Assert.NotEqual(modelA.Model.CheckpointSha256, modelB.Model.CheckpointSha256);

        var cachedB = await InvokeEvaluateAsync(registry, TenantA, "evaluate-rollback-corrupt-cache-b");
        Assert.Equal(200, cachedB.StatusCode);
        AssertEvaluateSelectedModel(cachedB.BodyJson, modelB.Model.ModelId);
        AssertRuntimeSelection(cachedB.BodyJson, loadedFresh: true, preservedPrevious: false);
        AssertEvaluationPromotedModelIdentity(cachedB.BodyJson, modelB.Model.ModelId, modelB.Model.CheckpointSha256);

        var rollback = await InvokeActivationAsync(
            registry,
            RollbackRequest(TenantA, "rollback:corrupt-b-to-a", expectedGeneration: 2));
        AssertSuccessActivation(rollback.BodyJson, modelA.Model.ModelId, expectedGeneration: 3, replay: false);
        var rollbackAActivationRef = ReadActiveActivationRef(rollback.BodyJson);
        Assert.Equal(activationARef, rollbackAActivationRef);
        Assert.NotEqual(cachedBActivationRef, rollbackAActivationRef);

        var replacementBytes = await File.ReadAllBytesAsync(modelB.Model.CheckpointPath);
        await File.WriteAllBytesAsync(modelA.Model.CheckpointPath, replacementBytes);

        var failed = await InvokeEvaluateAsync(registry, TenantA, "evaluate-rollback-corrupt-a");
        AssertActiveRuntimeLoadFailedClosed(
            failed,
            "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_SHA_MISMATCH",
            root,
            TenantA,
            activationARef,
            cachedBActivationRef,
            rollbackAActivationRef,
            modelA.Model.ModelId,
            modelB.Model.ModelId,
            modelA.Model.CheckpointPath,
            modelB.Model.CheckpointPath);
    }

    [Fact]
    public async Task ModelActivationRuntime_RollbackToPreviouslyCachedAThenCorruptAUsesExactCachedA()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "rollback-cached-corrupt-model-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "rollback-cached-corrupt-model-b", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activatedA = await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:rollback-cached-corrupt-a", 0, modelA));
        Assert.Equal(200, activatedA.StatusCode);
        var activationARef = ReadActiveActivationRef(activatedA.BodyJson);

        var cachedA = await InvokeEvaluateAsync(registry, TenantA, "evaluate-rollback-cached-corrupt-cache-a");
        Assert.Equal(200, cachedA.StatusCode);
        AssertEvaluateSelectedModel(cachedA.BodyJson, modelA.Model.ModelId);
        AssertRuntimeSelection(cachedA.BodyJson, loadedFresh: true, preservedPrevious: false);
        AssertEvaluationPromotedModelIdentity(cachedA.BodyJson, modelA.Model.ModelId, modelA.Model.CheckpointSha256);

        var activatedB = await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:rollback-cached-corrupt-b", 1, modelB));
        Assert.Equal(200, activatedB.StatusCode);
        var activationBRef = ReadActiveActivationRef(activatedB.BodyJson);
        Assert.NotEqual(activationARef, activationBRef);
        Assert.NotEqual(modelA.Model.ModelId, modelB.Model.ModelId);
        Assert.NotEqual(modelA.Model.CheckpointSha256, modelB.Model.CheckpointSha256);

        var cachedB = await InvokeEvaluateAsync(registry, TenantA, "evaluate-rollback-cached-corrupt-cache-b");
        Assert.Equal(200, cachedB.StatusCode);
        AssertEvaluateSelectedModel(cachedB.BodyJson, modelB.Model.ModelId);
        AssertRuntimeSelection(cachedB.BodyJson, loadedFresh: true, preservedPrevious: false);
        AssertEvaluationPromotedModelIdentity(cachedB.BodyJson, modelB.Model.ModelId, modelB.Model.CheckpointSha256);

        var rollback = await InvokeActivationAsync(
            registry,
            RollbackRequest(TenantA, "rollback:cached-corrupt-b-to-a", expectedGeneration: 2));
        AssertSuccessActivation(rollback.BodyJson, modelA.Model.ModelId, expectedGeneration: 3, replay: false);
        Assert.Equal(activationARef, ReadActiveActivationRef(rollback.BodyJson));

        var replacementBytes = await File.ReadAllBytesAsync(modelB.Model.CheckpointPath);
        await File.WriteAllBytesAsync(modelA.Model.CheckpointPath, replacementBytes);

        var recovered = await InvokeEvaluateAsync(registry, TenantA, "evaluate-rollback-cached-corrupt-a");
        Assert.True(recovered.StatusCode == 200, recovered.BodyJson);
        AssertEvaluateSelectedModel(recovered.BodyJson, modelA.Model.ModelId);
        AssertRuntimeSelection(recovered.BodyJson, loadedFresh: false, preservedPrevious: true);
        AssertEvaluationPromotedModelIdentity(recovered.BodyJson, modelA.Model.ModelId, modelA.Model.CheckpointSha256);
        Assert.Contains(
            DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
            recovered.BodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.ModelId, recovered.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(activationBRef, recovered.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, recovered.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.CheckpointPath, recovered.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, recovered.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRuntime_PerTenantRuntimeCacheRetainsTwoAndEvictsOldestExactIdentity()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "runtime-cache-evict-model-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "runtime-cache-evict-model-b", batchCount: 2);
        var modelC = await BuildActivationRecordAsync(root, TenantA, "runtime-cache-evict-model-c", batchCount: 3);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:runtime-cache-evict-a", 0, modelA))).StatusCode);
        var cachedA = await InvokeEvaluateAsync(registry, TenantA, "evaluate-runtime-cache-evict-a");
        Assert.Equal(200, cachedA.StatusCode);
        AssertEvaluateSelectedModel(cachedA.BodyJson, modelA.Model.ModelId);
        AssertRuntimeSelection(cachedA.BodyJson, loadedFresh: true, preservedPrevious: false);

        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:runtime-cache-evict-b", 1, modelB))).StatusCode);
        var cachedB = await InvokeEvaluateAsync(registry, TenantA, "evaluate-runtime-cache-evict-b");
        Assert.Equal(200, cachedB.StatusCode);
        AssertEvaluateSelectedModel(cachedB.BodyJson, modelB.Model.ModelId);
        AssertRuntimeSelection(cachedB.BodyJson, loadedFresh: true, preservedPrevious: false);

        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:runtime-cache-evict-c", 2, modelC))).StatusCode);
        var cachedC = await InvokeEvaluateAsync(registry, TenantA, "evaluate-runtime-cache-evict-c");
        Assert.Equal(200, cachedC.StatusCode);
        AssertEvaluateSelectedModel(cachedC.BodyJson, modelC.Model.ModelId);
        AssertRuntimeSelection(cachedC.BodyJson, loadedFresh: true, preservedPrevious: false);

        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:runtime-cache-retained-b", 3, modelB))).StatusCode);
        var replacementForB = await File.ReadAllBytesAsync(modelC.Model.CheckpointPath);
        await File.WriteAllBytesAsync(modelB.Model.CheckpointPath, replacementForB);

        var retainedB = await InvokeEvaluateAsync(registry, TenantA, "evaluate-runtime-cache-retained-b");
        Assert.Equal(200, retainedB.StatusCode);
        AssertEvaluateSelectedModel(retainedB.BodyJson, modelB.Model.ModelId);
        AssertRuntimeSelection(retainedB.BodyJson, loadedFresh: false, preservedPrevious: true);
        AssertEvaluationPromotedModelIdentity(retainedB.BodyJson, modelB.Model.ModelId, modelB.Model.CheckpointSha256);
        Assert.DoesNotContain(modelA.Model.ModelId, retainedB.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelC.Model.ModelId, retainedB.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, retainedB.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.CheckpointPath, retainedB.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, retainedB.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(modelC.Model.CheckpointPath, retainedB.BodyJson, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:runtime-cache-evicted-a", 4, modelA))).StatusCode);
        var replacementForA = await File.ReadAllBytesAsync(modelC.Model.CheckpointPath);
        await File.WriteAllBytesAsync(modelA.Model.CheckpointPath, replacementForA);

        var evictedA = await InvokeEvaluateAsync(registry, TenantA, "evaluate-runtime-cache-evicted-a");
        AssertActiveRuntimeLoadFailedClosed(
            evictedA,
            "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_SHA_MISMATCH",
            root,
            TenantA,
            modelA.Model.ModelId,
            modelB.Model.ModelId,
            modelC.Model.ModelId,
            modelA.Model.CheckpointPath,
            modelB.Model.CheckpointPath,
            modelC.Model.CheckpointPath);
    }

    [Fact]
    public async Task ModelActivationRuntime_FreshHostRegistryRetainsDurableState()
    {
        var root = NewRoot();
        var modelB = await BuildActivationRecordAsync(root, TenantB, "fresh-host-b", batchCount: 2);
        var firstHostRegistry = CreateRegistry(root, routeAdmissionTenantId: TenantB);

        var activated = await InvokeActivationAsync(
            firstHostRegistry,
            ActivateRequest(TenantB, "activate:fresh-host-b", 0, modelB));
        AssertSuccessActivation(activated.BodyJson, modelB.Model.ModelId, expectedGeneration: 1, replay: false);

        var freshHostRegistry = CreateRegistry(root, routeAdmissionTenantId: TenantB);
        var readback = await InvokeActivationAsync(
            freshHostRegistry,
            new DesignConversionModelServerModelActivationRequest(
                Action: DesignConversionModelServerModelActivationHandler.ActionReadActive,
                TenantId: TenantB));
        AssertSuccessActivation(readback.BodyJson, modelB.Model.ModelId, expectedGeneration: 1, replay: false);

        var evaluated = await InvokeEvaluateAsync(freshHostRegistry, TenantB, "evaluate-fresh-host-b");
        Assert.Equal(200, evaluated.StatusCode);
        AssertEvaluateSelectedModel(evaluated.BodyJson, modelB.Model.ModelId);
    }

    [Fact]
    public async Task ModelActivationRuntime_AdmittedTenantSelectsAWithMatchingOrCaseVariantBodyAndFailsClosedWhenBodyTenantAbsent()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "admitted-a-model", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantB, "admitted-b-model", batchCount: 2);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantA, "activate:admitted-a", 0, modelA))).StatusCode);
        Assert.Equal(200, (await InvokeActivationAsync(registry, ActivateRequest(TenantB, "activate:admitted-b", 0, modelB))).StatusCode);

        var matchingBody = await InvokeEvaluateAsync(
            registry,
            TenantA,
            "evaluate-admitted-a-matching-body",
            routeAdmissionToken: RouteAdmissionCurrentToken);
        Assert.Equal(200, matchingBody.StatusCode);
        AssertEvaluateSelectedModel(matchingBody.BodyJson, modelA.Model.ModelId);
        Assert.DoesNotContain(modelB.Model.ModelId, matchingBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, matchingBody.BodyJson, StringComparison.Ordinal);

        var caseVariantBody = await InvokeEvaluateAsync(
            registry,
            TenantA.ToUpperInvariant(),
            "evaluate-admitted-a-case-variant-body",
            routeAdmissionToken: RouteAdmissionCurrentToken);
        Assert.Equal(200, caseVariantBody.StatusCode);
        AssertEvaluateSelectedModel(caseVariantBody.BodyJson, modelA.Model.ModelId);
        Assert.DoesNotContain(modelB.Model.ModelId, caseVariantBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA.ToUpperInvariant(), caseVariantBody.BodyJson, StringComparison.Ordinal);

        var absentBody = await InvokeEvaluateAsync(
            registry,
            tenantId: null,
            requestId: "evaluate-admitted-a-absent-body",
            routeAdmissionToken: RouteAdmissionPreviousToken,
            includeTenantMetadata: false);
        Assert.Equal(403, absentBody.StatusCode);
        AssertErrorCode(absentBody.BodyJson, "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        Assert.DoesNotContain(modelA.Model.ModelId, absentBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelB.Model.ModelId, absentBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantA, absentBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantB, absentBody.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.CheckpointPath, absentBody.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(modelB.Model.CheckpointPath, absentBody.BodyJson, StringComparison.OrdinalIgnoreCase);
        using var absentDocument = JsonDocument.Parse(absentBody.BodyJson);
        var metadata = absentDocument.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_invoked").GetString());
    }

    [Fact]
    public async Task ModelActivationRuntime_InvalidBodyTenantRejectsBeforeRuntimeOrPersistence()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "invalid-body-tenant-model-a", batchCount: 1);
        var registry = CreateRegistry(root, routeAdmissionTenantId: TenantA);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:invalid-body-tenant-a", 0, modelA));
        AssertSuccessActivation(activated.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);

        var evaluated = await InvokeEvaluateAsync(
            registry,
            "tenant:design-conversion:route-alpha",
            "evaluate-invalid-body-tenant");

        Assert.Equal(403, evaluated.StatusCode);
        AssertErrorCode(evaluated.BodyJson, "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        Assert.DoesNotContain("tenant:design-conversion:route-alpha", evaluated.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(modelA.Model.ModelId, evaluated.BodyJson, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(evaluated.BodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_invoked").GetString());
    }

    [Fact]
    public async Task ModelActivationRoute_StaleGenerationReturnsConflict409()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "stale-first", batchCount: 1);
        var stale = await BuildActivationRecordAsync(root, TenantA, "stale-second", batchCount: 2);
        var registry = CreateRegistry(root);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:stale-first", 0, first));
        Assert.Equal(200, activated.StatusCode);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:stale-second", 0, stale));

        Assert.Equal(409, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_GENERATION_CONFLICT");
        Assert.DoesNotContain(root, response.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ModelActivationRoute_DuplicateActivateOperationIsIdempotentReplay()
    {
        var root = NewRoot();
        var model = await BuildActivationRecordAsync(root, TenantA, "duplicate", batchCount: 1);
        var registry = CreateRegistry(root);
        var request = ActivateRequest(TenantA, "activate:duplicate", 0, model);

        var first = await InvokeActivationAsync(registry, request);
        AssertSuccessActivation(first.BodyJson, model.Model.ModelId, expectedGeneration: 1, replay: false);

        var replay = await InvokeActivationAsync(registry, request);
        AssertSuccessActivation(replay.BodyJson, model.Model.ModelId, expectedGeneration: 1, replay: true);
        using var document = JsonDocument.Parse(replay.BodyJson);
        Assert.False(document.RootElement.GetProperty("applied").GetBoolean());
        Assert.True(document.RootElement.GetProperty("replay").GetBoolean());
    }

    [Fact]
    public async Task ModelActivationRoute_ForgedCallerOnlyPromotionReceiptFailsWithoutStateMutation()
    {
        var root = NewRoot();
        var forged = await BuildActivationRecordAsync(
            root,
            TenantA,
            "forged-caller-only",
            batchCount: 1,
            persistDurablePromotion: false);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:forged-caller-only", 0, forged));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_RECORD_NOT_FOUND");

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(readback.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    [Fact]
    public async Task ModelActivationRoute_CorruptPromotionAuthorityJournalFailsClosedWithoutStateMutation()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(
            root,
            TenantA,
            "corrupt-authority-journal",
            batchCount: 1,
            persistDurablePromotion: false);
        Directory.CreateDirectory(Path.GetDirectoryName(PromotionJournalPath(root))!);
        await File.WriteAllTextAsync(PromotionJournalPath(root), "{not-json", StrictUtf8);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:corrupt-authority-journal", 0, candidate));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_READ_FAILED");

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(readback.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    [Fact]
    public async Task ModelActivationRoute_DurableAuthorityRootConfinesCheckpointArtifacts()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(
            root,
            TenantA,
            "authority-root-confined",
            batchCount: 1,
            persistDurablePromotion: false);
        var outsideRoot = ArtifactDbRoot(root) + "-sibling";
        Directory.CreateDirectory(outsideRoot);
        var outsideCheckpoint = Path.Combine(outsideRoot, "authority-root-confined-checkpoint.json");
        File.Copy(candidate.Model.CheckpointPath, outsideCheckpoint);
        var forged = WithArtifactPaths(candidate, checkpointPath: outsideCheckpoint);
        await PersistPromotionAuthorityRecordAsync(root, TenantA, "authority-root-confined", forged, batchCount: 1);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:authority-root-confined", 0, forged));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_PATH_INVALID");
        Assert.DoesNotContain(outsideRoot, response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(outsideCheckpoint, response.BodyJson, StringComparison.OrdinalIgnoreCase);

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(readback.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    [Fact]
    public async Task ModelActivationRoute_NullPublicationManifestFailsClosedWithoutStateMutation()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "null-manifest-shape", batchCount: 1);
        var malformed = candidate with
        {
            Publication = candidate.Publication with { Manifest = null! }
        };
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:null-manifest-shape", 0, malformed));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_MANIFEST_REQUIRED");

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(readback.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    [Fact]
    public async Task ModelActivationRoute_DurablePromotionNullEvidenceCollectionFailsClosedWithoutStateMutation()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(
            root,
            TenantA,
            "durable-null-evidence",
            batchCount: 1,
            persistDurablePromotion: false);
        var malformedDurable = candidate with
        {
            Publication = candidate.Publication with
            {
                Decision = candidate.Publication.Decision with { EvidenceRefs = null! }
            }
        };
        await PersistPromotionAuthorityRecordAsync(root, TenantA, "durable-null-evidence", malformedDurable, batchCount: 1);
        var registry = CreateRegistry(root);

        var response = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:durable-null-evidence", 0, candidate));

        Assert.Equal(400, response.StatusCode);
        AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_RECORD_NOT_FOUND");

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        Assert.Equal(404, readback.StatusCode);
        AssertErrorCode(readback.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
    }

    [Fact]
    public async Task ModelActivationRoute_DurablePromotionBindingMismatchFailsAndPreservesActiveModel()
    {
        var root = NewRoot();
        var modelA = await BuildActivationRecordAsync(root, TenantA, "binding-active-a", batchCount: 1);
        var modelB = await BuildActivationRecordAsync(root, TenantA, "binding-candidate-b", batchCount: 2);
        var registry = CreateRegistry(root);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:binding-active-a", 0, modelA));
        AssertSuccessActivation(activated.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);

        var tamperedModel = modelB with
        {
            Model = modelB.Model with { ModelId = modelB.Model.ModelId + "-forged" }
        };
        var tamperedCheckpoint = modelB with
        {
            Model = modelB.Model with { CheckpointSha256 = Sha256Text("forged-checkpoint-sha") }
        };
        var tamperedPublication = modelB with
        {
            Publication = modelB.Publication with
            {
                Manifest = modelB.Publication.Manifest with
                {
                    ManifestRef = modelB.Publication.Manifest.ManifestRef + "-forged"
                }
            }
        };

        foreach (var (candidate, key) in new[]
                 {
                     (tamperedModel, "model"),
                     (tamperedCheckpoint, "checkpoint"),
                     (tamperedPublication, "publication")
                 })
        {
            var response = await InvokeActivationAsync(
                registry,
                ActivateRequest(TenantA, $"activate:binding-mismatch-{key}", 1, candidate));

            Assert.Equal(400, response.StatusCode);
            AssertErrorCode(response.BodyJson, "DESIGN_CONVERSION_MODEL_ACTIVATION_DURABLE_PROMOTION_BINDING_MISMATCH");
        }

        var readback = await InvokeActivationAsync(registry, ReadActiveRequest(TenantA));
        AssertSuccessActivation(readback.BodyJson, modelA.Model.ModelId, expectedGeneration: 1, replay: false);
    }

    [Fact]
    public async Task ModelActivationRoute_MultiplePartialAuthorityMatchesAcceptsExactDurableBinding()
    {
        var root = NewRoot();
        var exact = await BuildActivationRecordAsync(root, TenantA, "partial-match-exact", batchCount: 2);
        var laterPartial = await BuildActivationRecordAsync(
            root,
            TenantA,
            "partial-match-later-mismatch",
            batchCount: 3,
            persistDurablePromotion: false);
        var sharedRef = exact.Publication.Manifest.PromotionUploadRef;
        var laterPartialWithSharedRef = laterPartial with
        {
            Publication = laterPartial.Publication with
            {
                Manifest = laterPartial.Publication.Manifest with { PromotionUploadRef = sharedRef },
                UploadReceipt = laterPartial.Publication.UploadReceipt! with { RemoteRef = sharedRef! }
            }
        };
        await PersistPromotionAuthorityRecordAsync(root, TenantA, "partial-match-later-mismatch", laterPartialWithSharedRef, batchCount: 3);
        var registry = CreateRegistry(root);

        var activated = await InvokeActivationAsync(
            registry,
            ActivateRequest(TenantA, "activate:partial-match-exact", 0, exact));

        AssertSuccessActivation(activated.BodyJson, exact.Model.ModelId, expectedGeneration: 1, replay: false);
    }

    private static DesignConversionModelServerEndpointRegistry CreateRegistry(
        string root,
        bool configureActivation = true,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? modelLifecycleTokenKeyRing = null,
        string? routeAdmissionTenantId = null)
    {
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(ArtifactDbRoot(root));
        var previousVisualCostBudgetStoreRoot = Environment.GetEnvironmentVariable(
            JsonlDesignConversionVisualCostBudgetStore.VisualCostBudgetStoreRootEnvironmentVariable);
        var previousVisualCostLedgerRoot = Environment.GetEnvironmentVariable(
            JsonlDesignConversionVisualCostLedger.VisualCostLedgerRootEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostBudgetStore.VisualCostBudgetStoreRootEnvironmentVariable,
                Path.Combine(root, "visual-cost-budget-store"));
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostLedger.VisualCostLedgerRootEnvironmentVariable,
                Path.Combine(root, "visual-cost-ledger"));
            return DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                new UnexpectedCandidateStrategy(),
                artifactDb,
                utcNow ?? (() => DateTimeOffset.Parse("2026-07-10T10:00:00Z")),
                activationStateRootDirectory: configureActivation ? Path.Combine(root, "activation-state") : null,
                modelLifecycleAuthToken: configureActivation ? LocalToken : null,
                modelLifecycleTokenKeyRing: configureActivation ? modelLifecycleTokenKeyRing : null,
                routeAdmissionOptions: string.IsNullOrWhiteSpace(routeAdmissionTenantId)
                    ? null
                    : new DesignConversionModelServerRouteAdmissionOptions(
                        enabled: true,
                        currentToken: RouteAdmissionCurrentToken,
                        previousToken: RouteAdmissionPreviousToken,
                        previousTokenExpiresAtUtc: DateTimeOffset.Parse("2026-07-10T10:05:00Z"),
                        tenantId: routeAdmissionTenantId));
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostBudgetStore.VisualCostBudgetStoreRootEnvironmentVariable,
                previousVisualCostBudgetStoreRoot);
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostLedger.VisualCostLedgerRootEnvironmentVariable,
                previousVisualCostLedgerRoot);
        }
    }

    private static async Task<DesignConversionModelServerHttpResponse> InvokeActivationAsync(
        DesignConversionModelServerEndpointRegistry registry,
        DesignConversionModelServerModelActivationRequest request,
        string? token = LocalToken)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (token is not null)
            headers[DesignConversionModelServerModelActivationApi.LocalAuthorizationHeaderName] = token;

        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.ModelActivationEndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath,
            BodyJson: JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: headers));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        return invoked.Value!;
    }

    private static async Task<DesignConversionModelServerHttpResponse> InvokeEvaluateAsync(
        DesignConversionModelServerEndpointRegistry registry,
        string? tenantId,
        string requestId,
        string? routeAdmissionToken = RouteAdmissionCurrentToken,
        bool includeTenantMetadata = true)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["x-design-conversion-model-activation-test"] = "route-runtime"
        };
        if (routeAdmissionToken is not null)
        {
            headers[DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = "Bearer " + routeAdmissionToken;
            headers[DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] = requestId + "-idempotency";
        }

        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.EndpointPath,
            BodyJson: JsonSerializer.Serialize(CreateEvaluateRequest(tenantId, requestId, includeTenantMetadata), DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: headers));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        return invoked.Value!;
    }

    private static DesignConversionModelServerModelActivationRequest ActivateRequest(
        string tenantId,
        string idempotencyKey,
        long expectedGeneration,
        DesignConversionPromotedModelActivationRecord candidate)
        => new(
            Action: DesignConversionModelServerModelActivationHandler.ActionActivate,
            TenantId: tenantId,
            RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
            Reason: $"activation route runtime test {idempotencyKey}",
            IdempotencyKey: idempotencyKey,
            ExpectedGeneration: expectedGeneration,
            Candidate: candidate,
            ExpectedModelVersion: candidate.Model.Version,
            RequestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"));

    private static DesignConversionModelServerModelActivationRequest RollbackRequest(
        string tenantId,
        string idempotencyKey,
        long expectedGeneration)
        => new(
            Action: DesignConversionModelServerModelActivationHandler.ActionRollback,
            TenantId: tenantId,
            RequestedByRef: "architect:design-conversion-model-activation-route-runtime-test",
            Reason: $"activation route runtime rollback test {idempotencyKey}",
            IdempotencyKey: idempotencyKey,
            ExpectedGeneration: expectedGeneration,
            RequestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"));

    private static DesignConversionModelServerModelActivationRequest ReadActiveRequest(string tenantId)
        => new(
            Action: DesignConversionModelServerModelActivationHandler.ActionReadActive,
            TenantId: tenantId);

    private static DesignConversionModelServerApiRequest CreateEvaluateRequest(
        string? tenantId,
        string requestId,
        bool includeTenantMetadata = true)
    {
        var metadata = new Dictionary<string, string>
        {
            ["activation_route_runtime_test"] = bool.TrueString,
            [DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] = "paid",
            [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] = "125.75",
            [DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            [DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            [DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority
        };
        if (includeTenantMetadata && !string.IsNullOrWhiteSpace(tenantId))
            metadata["tenant_id"] = tenantId;

        return new DesignConversionModelServerApiRequest(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":design-tree")}",
                Payload: "{\"kind\":\"route-runtime-design\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":original-tree")}",
                Payload: "{\"kind\":\"route-runtime-original\"}",
                Format: "figma-json"),
            Html: "<main><h1>Route runtime activation</h1></main>",
            Css: "body{margin:0;color:#123456;background:#f8fafc}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:source",
                    Kind: "screenshot",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://activation-route/{requestId}/source.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":source")}")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:render",
                    Kind: "design-render",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://activation-route/{requestId}/render.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":render")}")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "activation-route-runtime-user",
                Tier: "paid",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0m,
                Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["design_tier"] = "paid",
                    ["monthly_visual_cost_limit_usd"] = "125.75"
                }),
            Metadata: metadata);
    }

    private static DesignConversionModelServerApiRequest CreateEvaluateRequestLegacyUnused(string tenantId, string requestId)
        => new(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":design-tree")}",
                Payload: "{\"kind\":\"route-runtime-design\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":original-tree")}",
                Payload: "{\"kind\":\"route-runtime-original\"}",
                Format: "figma-json"),
            Html: "<main><h1>Route runtime activation</h1></main>",
            Css: "body{margin:0;color:#123456;background:#f8fafc}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:source",
                    Kind: "screenshot",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://activation-route/{requestId}/source.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":source")}")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:render",
                    Kind: "design-render",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://activation-route/{requestId}/render.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":render")}")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "activation-route-runtime-user",
                Tier: "local",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0m),
            Metadata: new Dictionary<string, string>
            {
                ["tenant_id"] = tenantId,
                ["activation_route_runtime_test"] = bool.TrueString
            });

    private static void AssertSuccessActivation(
        string bodyJson,
        string expectedModelId,
        long expectedGeneration,
        bool replay)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("model-activation-completed", root.GetProperty("contractStatus").GetString());
        Assert.Equal(expectedGeneration, root.GetProperty("generation").GetInt64());
        Assert.Equal(replay, root.GetProperty("replay").GetBoolean());
        Assert.Equal(expectedModelId, root.GetProperty("activeVersion").GetProperty("modelId").GetString());
        Assert.False(root.GetProperty("productReady").GetBoolean());

        var active = root.GetProperty("activeVersion");
        Assert.True(active.TryGetProperty("checkpointPathRef", out _));
        Assert.False(active.TryGetProperty("checkpointPath", out _));
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("model_activation_internal_token_required").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("model_activation_public_anonymous_control").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("model_activation_checkpoints_deleted").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    private static void AssertEvaluateSelectedModel(string bodyJson, string expectedModelId)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        var evidenceRefs = root.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
        var warnings = root.GetProperty("warnings")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.False(root.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.True(root.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.Equal(expectedModelId, metadata.GetProperty("local_promoted_model_id").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_model_promotion_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_promoted_checkpoint_loaded").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_promoted_checkpoint_real_weights").GetString());
        Assert.Equal("critic_local_checkpoint_feature_adjusted_score", metadata.GetProperty("logical_flow_primary_metric_name").GetString());
        Assert.True(
            int.Parse(metadata.GetProperty("LocalCheckpointFeatureContributionCount").GetString() ?? "0", System.Globalization.CultureInfo.InvariantCulture) > 0);
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_checkpoint_weights_configured").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("trained_model_authority").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("not_trained_model_authority").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, evidenceRefs);
        Assert.Contains(DesignConversionMachine.LocalCheckpointFeatureAdjustmentEvidence, evidenceRefs);
        Assert.Contains("logical-flow:design-conversion-routing-flow", evidenceRefs);
        Assert.Contains("logical-orchestrator-selected-flow:design-conversion-routing-flow", evidenceRefs);
        Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, warnings);
    }

    private static void AssertRuntimeSelection(string bodyJson, bool loadedFresh, bool preservedPrevious)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        var evidenceRefs = root.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
        var warnings = root.GetProperty("warnings")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();

        Assert.Contains($"active_model_runtime_fresh_load_succeeded={loadedFresh}", warnings);
        Assert.Contains($"active_model_runtime_previous_preserved={preservedPrevious}", warnings);
        Assert.Contains(
            loadedFresh
                ? DesignConversionActivatedModelRuntimeEvaluator.FreshLoadEvidence
                : DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
            evidenceRefs);
        if (loadedFresh)
        {
            Assert.DoesNotContain(
                DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
                evidenceRefs);
        }
    }

    private static void AssertEvaluationPromotedModelIdentity(
        string bodyJson,
        string expectedModelId,
        string expectedCheckpointSha256)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(expectedModelId, metadata.GetProperty("local_promoted_model_id").GetString());
        Assert.Equal(expectedCheckpointSha256, metadata.GetProperty("local_promoted_checkpoint_sha256").GetString());
    }

    private static void AssertActiveRuntimeLoadFailedClosed(
        DesignConversionModelServerHttpResponse response,
        string expectedErrorCode,
        params string[] forbiddenValues)
    {
        Assert.False(response.StatusCode is >= 200 and < 300, response.BodyJson);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(response.StatusCode, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());
        Assert.False(root.TryGetProperty("evidenceRefs", out _));
        Assert.False(root.TryGetProperty("runtimeStatus", out _));
        Assert.False(root.TryGetProperty("trainingReadiness", out _));

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_user_private_data_included").GetString());

        Assert.DoesNotContain(
            DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence,
            response.BodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain("active_model_runtime_previous_preserved", response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("local_promoted_model_id", response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("active_model_id", response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("active_model_checkpoint", response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("CHECKPOINT_PATH:", response.BodyJson, StringComparison.Ordinal);

        foreach (var forbiddenValue in forbiddenValues.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal))
            Assert.DoesNotContain(forbiddenValue, response.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertEvaluateDidNotSelectActiveModel(string bodyJson, string forbiddenModelId)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.DoesNotContain(forbiddenModelId, bodyJson, StringComparison.Ordinal);

        var evidenceRefs = root.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
        Assert.Contains(
            DesignConversionActivatedModelRuntimeEvaluator.NoActiveSelectionEvidence,
            evidenceRefs);
        Assert.DoesNotContain(
            DesignConversionActivatedModelRuntimeEvaluator.ActiveSelectionEvidence,
            evidenceRefs);
        Assert.DoesNotContain(
            DesignConversionActivatedModelRuntimeEvaluator.FreshLoadEvidence,
            evidenceRefs);

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("local_model_promotion_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("local_promoted_checkpoint_loaded").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("local_promoted_checkpoint_real_weights").GetString());
        Assert.False(metadata.TryGetProperty("local_promoted_model_id", out _));
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    private static void AssertErrorCode(string bodyJson, string expectedErrorCode)
    {
        using var document = JsonDocument.Parse(bodyJson);
        Assert.Equal(expectedErrorCode, document.RootElement.GetProperty("errorCode").GetString());
    }

    private static string ReadActiveActivationRef(string bodyJson)
    {
        using var document = JsonDocument.Parse(bodyJson);
        return document.RootElement.GetProperty("activeVersion").GetProperty("activationRef").GetString() ?? string.Empty;
    }

    private static void AssertDoesNotExposeLifecycleTokens(string bodyJson)
    {
        Assert.DoesNotContain(LocalToken, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(PreviousLocalToken, bodyJson, StringComparison.Ordinal);
    }

    private static async Task AssertInvalidKeyRingConfigReturnsNonReadyAsync(
        string root,
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions options,
        string expectedErrorCode,
        params string[] forbiddenTokens)
    {
        var registry = CreateRegistry(root, modelLifecycleTokenKeyRing: options);
        var response = await InvokeActivationAsync(
            registry,
            ReadActiveRequest(TenantA),
            token: LocalToken);

        Assert.Equal(503, response.StatusCode);
        AssertErrorCode(response.BodyJson, expectedErrorCode);
        foreach (var forbiddenToken in forbiddenTokens)
        {
            if (!string.IsNullOrEmpty(forbiddenToken))
                Assert.DoesNotContain(forbiddenToken, response.BodyJson, StringComparison.Ordinal);
        }
    }

    private static async Task<DesignConversionPromotedModelActivationRecord> BuildActivationRecordAsync(
        string root,
        string tenantId,
        string name,
        int batchCount,
        bool persistDurablePromotion = true)
    {
        var outputDirectory = Path.Combine(ArtifactDbRoot(root), "models", name);
        Directory.CreateDirectory(outputDirectory);
        var machine = new DesignConversionMachine();
        for (var index = 0; index < batchCount; index++)
        {
            var trained = await machine.TrainBatchAsync(new LearningBatch<DesignDomain>(
                BatchId: $"{name}-batch-{index}",
                DomainTag: DesignConversionMachine.DomainTag,
                TrainingItems:
                [
                    new LearningTrainingItem(
                        ItemId: $"{name}-item-{index}",
                        Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
                        {
                            ["unit_id"] = DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                            ["source_kind"] = "Figma",
                            ["source_manifest_hash"] = $"{name}-manifest-source-{index}",
                            ["source_identity_hash"] = $"{name}-identity-{index}",
                            ["source_structure_hash"] = $"{name}-structure-{index}",
                            ["pair_family"] = "visual"
                        }),
                        ChosenResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                        {
                            ["visual_score"] = 0.96,
                            ["accepted"] = true
                        }),
                        RejectedResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                        {
                            ["visual_score"] = 0.12,
                            ["accepted"] = false
                        }))
                ],
                SourceIds: [$"{name}-source-{index}"]));
            Assert.True(trained.IsSuccess, trained.ErrorMessage);
        }

        var checkpointPath = Path.Combine(outputDirectory, $"{name}-checkpoint.json");
        var save = await machine.SaveCheckpointAsync(checkpointPath);
        Assert.True(save.IsSuccess, save.ErrorMessage);
        var load = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(load.Value!.Checkpoint));

        var tokenizerPath = Path.Combine(outputDirectory, $"{name}-tokenizer.json");
        var policyPath = Path.Combine(outputDirectory, $"{name}-policy.json");
        await File.WriteAllTextAsync(tokenizerPath, "{\"kind\":\"activation-route-tokenizer\"}");
        await File.WriteAllTextAsync(policyPath, "{\"kind\":\"activation-route-policy\"}");

        var checkpointFullPath = Path.GetFullPath(checkpointPath);
        var checkpointSha = await ReadSha256Async(checkpointFullPath);
        var manifestId = $"{name}-manifest";
        var manifestRef = $"manifest://design-conversion/{manifestId}";
        var promotionRef = DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                           Uri.EscapeDataString(manifestId);
        var artifacts = new DesignConversionTrainingManifestArtifacts(
            checkpointFullPath,
            Path.GetFullPath(tokenizerPath),
            Path.GetFullPath(policyPath));
        var manifest = new DesignConversionTrainingManifest(
            ManifestId: manifestId,
            MachineId: load.Value.Checkpoint.MachineId,
            DomainTag: load.Value.Checkpoint.DomainTag,
            Version: load.Value.Checkpoint.Version,
            CheckpointRef: checkpointFullPath,
            ManifestRef: manifestRef,
            CreatedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            AcceptedRecordCount: batchCount,
            QuarantinedRecordCount: 0,
            UnitIds: [DesignConversionRuntimeScaffoldCriticUnit.UnitId],
            Batches:
            [
                new DesignConversionTrainingManifestBatch(
                    BatchId: $"{name}-manifest-batch",
                    UnitId: DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                    RecordCount: batchCount,
                    RecordIds: Enumerable.Range(0, batchCount).Select(index => $"{name}-record-{index}").ToArray(),
                    SourceIds: Enumerable.Range(0, batchCount).Select(index => $"{name}-source-{index}").ToArray())
            ],
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN",
                "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED_LOCAL_RECEIPT",
                $"CHECKPOINT_PATH:{checkpointFullPath}"
            ],
            ModelServerUploadAttempted: false,
            ModelServerPromotionClaimed: false,
            DeterministicLogicClaimedAsLearning: false)
        {
            PromotionArtifacts = artifacts,
            PromotionUploadRef = promotionRef,
            LocalPromotionReceiptWritten = true,
            PromotionReceiptKind = "local_receipt_only"
        };
        var decision = new DesignConversionPromotionDecision(
            DesignConversionPromotionDecisionKind.Promote,
            ["ALL_PROMOTION_GATES_PASSED"],
            ["DESIGN_CONVERSION_PROMOTION_GATES_PASSED"]);
        var receipt = new DesignConversionPromotionUploadReceipt(
            UploadId: $"{name}-local-receipt",
            RemoteRef: promotionRef,
            UploadedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            EvidenceRefs: ["DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN"]);
        var publication = new DesignConversionPromotionPublicationResult(decision, manifest, receipt);
        var modelId = $"{manifestId}:{checkpointSha[..12]}";
        var model = new DesignConversionPromotedLocalModel(
            ModelId: modelId,
            ManifestId: manifest.ManifestId,
            ManifestRef: manifest.ManifestRef,
            CheckpointPath: checkpointFullPath,
            CheckpointSha256: checkpointSha,
            MachineId: load.Value.Checkpoint.MachineId,
            DomainTag: load.Value.Checkpoint.DomainTag,
            Version: load.Value.Checkpoint.Version,
            LearnedWeightArtifactCount: DesignConversionMachine.CountRealLearnedWeightArtifacts(load.Value.Checkpoint),
            LearnedWeightObservationCount: DesignConversionMachine.CountRealLearnedWeightObservations(load.Value.Checkpoint),
            CheckpointFreshLoadVerified: true,
            CheckpointContainsRealTrainedWeights: true,
            PromotionManifestClaimed: true,
            LocalOnly: true,
            EvidenceRefs:
            [
                DesignConversionPromotedLocalModelLoader.LocalPromotionBoundary,
                "DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN",
                "DESIGN_CONVERSION_PROMOTED_LOCAL_CHECKPOINT_FRESH_LOADED",
                "DESIGN_CONVERSION_PROMOTED_LOCAL_CHECKPOINT_REAL_WEIGHTS_VERIFIED"
            ]);

        var record = new DesignConversionPromotedModelActivationRecord(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(tenantId),
            publication,
            model);
        if (persistDurablePromotion)
            await PersistPromotionAuthorityRecordAsync(root, tenantId, name, record, batchCount);

        return record;
    }

    private static async Task PersistPromotionAuthorityRecordAsync(
        string root,
        string tenantId,
        string name,
        DesignConversionPromotedModelActivationRecord record,
        int batchCount)
    {
        var promotion = BuildPromotionAuthorityRecord(tenantId, name, record, batchCount);
        var journalPath = PromotionJournalPath(root);
        Directory.CreateDirectory(Path.GetDirectoryName(journalPath)!);
        await File.AppendAllTextAsync(
            journalPath,
            JsonSerializer.Serialize(promotion, PromotionJournalJsonOptions) + Environment.NewLine,
            StrictUtf8);
    }

    private static string PromotionJournalPath(string root)
        => Path.Combine(
            ArtifactDbRoot(root),
            JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionFileName);

    private static string ArtifactDbRoot(string root)
        => Path.Combine(root, "artifact-db");

    private static DesignConversionPromotedModelActivationRecord WithArtifactPaths(
        DesignConversionPromotedModelActivationRecord record,
        string? checkpointPath = null,
        string? tokenizerPath = null,
        string? policyPath = null)
    {
        Assert.NotNull(record.Publication.Manifest.PromotionArtifacts);
        var artifacts = record.Publication.Manifest.PromotionArtifacts!;
        var nextCheckpointPath = checkpointPath ?? artifacts.CheckpointPath;
        var nextArtifacts = artifacts with
        {
            CheckpointPath = nextCheckpointPath,
            TokenizerPath = tokenizerPath ?? artifacts.TokenizerPath,
            PolicyPath = policyPath ?? artifacts.PolicyPath
        };
        var nextManifest = record.Publication.Manifest with
        {
            CheckpointRef = nextCheckpointPath,
            PromotionArtifacts = nextArtifacts
        };

        return record with
        {
            Publication = record.Publication with { Manifest = nextManifest },
            Model = checkpointPath is null
                ? record.Model
                : record.Model with { CheckpointPath = checkpointPath }
        };
    }

    private static DesignConversionTrainingIntakeLocalPromotionResult BuildPromotionAuthorityRecord(
        string tenantId,
        string name,
        DesignConversionPromotedModelActivationRecord record,
        int batchCount)
    {
        var run = new DesignConversionTrainingIntakeRunResult(
            RunId: $"{name}-authority-run",
            TrainingIntakeId: $"{name}-authority-intake",
            PreflightId: $"{name}-authority-preflight",
            AcceptedCandidateCount: batchCount,
            ResolvedTrainingRecordCount: batchCount,
            MayStartTraining: true,
            SourceManifestStatus: "proven",
            SourceDisjointStatus: "proven",
            TrainingRunnerInvoked: true,
            LocalLifecycleHookAttempted: true,
            TrainingStarted: true,
            CheckpointMutated: true,
            CheckpointCandidateCreated: true,
            WeightsProduced: true,
            ModelUploadReady: true,
            TrainingExecutionBoundary: "model_activation_route_test_server_owned_promotion_authority",
            TrainingExecutionStatus: "completed",
            RunnerBlockedReason: string.Empty,
            CheckpointStatus: "created",
            CheckpointMutationStatus: "mutated",
            WeightProofStatus: "real_weights_proven",
            MeasurementGateStatus: DesignConversionMeasurementGateStatus.Passed,
            PromotionGateStatus: "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS",
            UploadReadinessStatus: "upload_ready_local_promotion_gate_passed_no_external_upload",
            PromotionBlockedReasons: [],
            RunnerEvidence: null,
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["product_ready"] = bool.FalseString
            });

        return new DesignConversionTrainingIntakeLocalPromotionResult(
            TrainingRun: run,
            LocalPromotionReady: true,
            ExternalCloudUploadAttempted: false,
            PromotionOutputDirectory: Path.GetDirectoryName(record.Model.CheckpointPath) ?? string.Empty,
            PromotedCheckpointPath: record.Model.CheckpointPath,
            Publication: record.Publication,
            PromotedLocalModel: record.Model,
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_READY",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_ACTIVE_STATE_RECORDED",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_TEST_DURABLE_PROMOTION_RECORD"
            ],
            Warnings: ["local_promotion_ready"],
            BlockReasons: [],
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["schema_version"] = JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionSchemaVersion,
                ["training_intake_tenant_id_ref"] = DesignConversionModelActivationRegistry.BuildTenantIdRef(tenantId),
                ["local_promotion_record_kind"] = "promotion",
                ["local_promotion_ready"] = bool.TrueString,
                ["local_promotion_not_ready"] = bool.FalseString,
                ["local_promotion_active_local_model_ref"] = record.Publication.Manifest.PromotionUploadRef ?? string.Empty,
                ["local_promotion_active_model_id"] = record.Model.ModelId,
                ["local_promotion_active_checkpoint_sha256"] = record.Model.CheckpointSha256,
                ["promotion_upload_ref"] = record.Publication.Manifest.PromotionUploadRef ?? string.Empty,
                ["local_promoted_model_id"] = record.Model.ModelId,
                ["local_promoted_checkpoint_sha256"] = record.Model.CheckpointSha256,
                ["deployment_verified"] = bool.FalseString,
                ["product_ready"] = bool.FalseString
            });
    }

    private static string NewRoot()
        => Path.Combine(Path.GetTempPath(), "llm_mvp_core", "design-conversion-model-activation-route", Guid.NewGuid().ToString("N"));

    private static async Task<string> ReadSha256Async(string path)
    {
        var bytes = await File.ReadAllBytesAsync(path);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static string Sha256Bytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed class UnexpectedCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "Model activation route/runtime tests do not exercise the improvement candidate path.",
                "MODEL_ACTIVATION_ROUTE_UNEXPECTED_CANDIDATE_REQUEST"));
    }
}
