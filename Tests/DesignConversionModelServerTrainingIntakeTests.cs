// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelServer.DesignConversion;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class DesignConversionModelServerTrainingIntakeTests
{
    private static readonly Uri PromotionUploadEndpoint = new("https://promotion.example/v1/design-conversion/promotions");
    private static readonly DateTimeOffset PromotionUploadedAtUtc = DateTimeOffset.Parse("2026-07-14T08:30:00Z");
    private const string PromotionHostTenantId = "tenant-alpha";
    private const string PromotionHostBearerToken = "promotion-host-bearer-token-123456";

    [Fact]
    public void PromotionHostOptions_ToStringRedactsNestedBearerTokenAndPreservesCopyWith()
    {
        var originalHostRootSegment = $"promotion-host-options-private-host-root-{Guid.NewGuid():N}";
        var originalUploaderRootSegment = $"promotion-host-options-private-uploader-root-{Guid.NewGuid():N}";
        var copiedHostRootSegment = $"promotion-host-options-copy-host-root-{Guid.NewGuid():N}";
        var copiedUploaderRootSegment = $"promotion-host-options-copy-uploader-root-{Guid.NewGuid():N}";
        var root = Path.Combine(Path.GetTempPath(), originalHostRootSegment, "host-artifacts");
        var uploaderAllowedRoot = Path.Combine(Path.GetTempPath(), originalUploaderRootSegment, "uploader-artifacts");
        var copiedRoot = Path.Combine(Path.GetTempPath(), copiedHostRootSegment, "host-artifacts");
        var copiedUploaderAllowedRoot = Path.Combine(Path.GetTempPath(), copiedUploaderRootSegment, "uploader-artifacts");
        var sentinelBearer = $"promotion-host-options-secret-bearer-{Guid.NewGuid():N}";
        using var promotionHttpClient = new HttpClient(
            new CapturingPromotionUploadHttpHandler(PromotionUploadResponseMode.Success),
            disposeHandler: true);
        var uploaderOptions = new DesignConversionModelServerPromotionHttpUploaderOptions(
            PromotionUploadEndpoint,
            PromotionHostTenantId,
            sentinelBearer,
            uploaderAllowedRoot)
        {
            Timeout = TimeSpan.FromSeconds(5),
            MaxArtifactBytes = 1024 * 1024,
            MaxTotalArtifactBytes = 3 * 1024 * 1024,
            MaxResponseBodyBytes = 64 * 1024,
            MaxArtifactCount = DesignConversionModelServerPromotionHttpUploaderOptions.HardMaxArtifactCount
        };
        var hostOptions = new DesignConversionModelServerPromotionHostOptions(
            root,
            promotionHttpClient,
            uploaderOptions);
        var copiedUploaderOptions = uploaderOptions with
        {
            AllowedArtifactRoot = copiedUploaderAllowedRoot,
            Timeout = TimeSpan.FromSeconds(6)
        };
        var copied = hostOptions with
        {
            ArtifactRootDirectory = copiedRoot,
            PromotionHttpUploaderOptions = copiedUploaderOptions
        };

        Assert.Equal(root, hostOptions.ArtifactRootDirectory);
        Assert.Equal(uploaderAllowedRoot, hostOptions.PromotionHttpUploaderOptions!.AllowedArtifactRoot);
        Assert.Equal(copiedRoot, copied.ArtifactRootDirectory);
        Assert.Equal(copiedUploaderAllowedRoot, copied.PromotionHttpUploaderOptions!.AllowedArtifactRoot);
        Assert.Equal(TimeSpan.FromSeconds(5), hostOptions.PromotionHttpUploaderOptions.Timeout);
        Assert.Equal(TimeSpan.FromSeconds(6), copied.PromotionHttpUploaderOptions.Timeout);
        Assert.Same(promotionHttpClient, copied.PromotionHttpClient);
        Assert.Equal(sentinelBearer, copied.PromotionHttpUploaderOptions.BearerToken);
        foreach (var (surface, fullRoots, rootSegments) in new[]
        {
            (
                hostOptions.ToString(),
                new[] { root, uploaderAllowedRoot },
                new[] { originalHostRootSegment, originalUploaderRootSegment }
            ),
            (
                copied.ToString(),
                new[] { copiedRoot, copiedUploaderAllowedRoot },
                new[] { copiedHostRootSegment, copiedUploaderRootSegment }
            )
        })
        {
            Assert.Contains(PromotionUploadEndpoint.AbsoluteUri, surface, StringComparison.Ordinal);
            Assert.Contains(PromotionHostTenantId, surface, StringComparison.Ordinal);
            Assert.Contains(nameof(DesignConversionModelServerPromotionHostOptions.ArtifactRootDirectory), surface, StringComparison.Ordinal);
            Assert.Contains(nameof(DesignConversionModelServerPromotionHostOptions.PromotionHttpClient), surface, StringComparison.Ordinal);
            Assert.Contains(nameof(DesignConversionModelServerPromotionHostOptions.PromotionHttpUploaderOptions), surface, StringComparison.Ordinal);
            Assert.Contains(nameof(DesignConversionModelServerPromotionHttpUploaderOptions.AllowedArtifactRoot), surface, StringComparison.Ordinal);
            Assert.Contains("ArtifactRootDirectory = [REDACTED]", surface, StringComparison.Ordinal);
            Assert.Contains("BearerToken = [REDACTED]", surface, StringComparison.Ordinal);
            Assert.Contains("AllowedArtifactRoot = [REDACTED]", surface, StringComparison.Ordinal);
            Assert.Contains("[REDACTED]", surface, StringComparison.Ordinal);
            Assert.DoesNotContain(sentinelBearer, surface, StringComparison.Ordinal);
            foreach (var fullRoot in fullRoots)
            {
                Assert.DoesNotContain(fullRoot, surface, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(fullRoot.Replace('\\', '/'), surface, StringComparison.OrdinalIgnoreCase);
            }

            foreach (var rootSegment in rootSegments)
                Assert.DoesNotContain(rootSegment, surface, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_RegistryRoute_DelegatesToDomainApiAndListsSanitizedIntakes()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ROUTE_SECRET";
        var seeded = await CreateSeededIntakeAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        var registered = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod,
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath);

        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointRoute, registered.Value!.BoundaryMetadata["endpoint_route"]);
        Assert.Equal(nameof(DesignConversionModelServerTrainingIntakeHandler), registered.Value.BoundaryMetadata["delegates_to_handler"]);
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), registered.Value.BoundaryMetadata["delegates_to_domain_api"]);
        Assert.Equal(bool.TrueString, registered.Value.BoundaryMetadata["training_intake_route_local_only"]);
        Assert.Equal(bool.FalseString, registered.Value.BoundaryMetadata["product_ready"]);

        var response = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            BodyJson: JsonSerializer.Serialize(new { action = "list" }),
            ContentType: "application/json"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        AssertSanitizedTrainingIntakeBody(response.Value!, privateMarker);
        using var document = JsonDocument.Parse(response.Value!.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerTrainingIntakeHandler.ApiShape, root.GetProperty("apiShape").GetString());
        Assert.Equal("training-intake-action-completed", root.GetProperty("contractStatus").GetString());
        Assert.Equal("list", root.GetProperty("action").GetString());
        Assert.Equal(1, root.GetProperty("intakeCount").GetInt32());
        Assert.Equal(seeded.IntakeId, root.GetProperty("intakes")[0].GetProperty("intakeId").GetString());
        Assert.False(root.GetProperty("productReady").GetBoolean());
        Assert.False(root.GetProperty("deploymentVerified").GetBoolean());
        Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), metadata.GetProperty("delegates_to_domain_api").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_route_local_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_response_refs_only").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    [Fact]
    public async Task PostTrainingIntake_CreateRunAndPromoteLocal_UsesApprovedHandoffThroughRouteAndKeepsLocalOnlyBoundary()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ROUTE_CREATE_RUN_PROMOTE_SECRET";
        var seeded = await CreateApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-route-create-run-promote";
        const string createIdempotencyKey = "idempotency:create-training-intake-route-create-run-promote";
        const string runIdempotencyKey = "idempotency:run-training-intake-route-create-run-promote";
        const string promoteIdempotencyKey = "idempotency:promote-training-intake-route-create-run-promote";
        var options = new DesignConversionTrainingPipelineOptions(
            RunId: "training-intake-route-create-run-promote",
            BatchSize: 1,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "training-intake-route-create-run-promote-dataset",
                MinimumTrainRecords: 50,
                MinimumHeldOutRecords: 50));
        AssertTrainingDatabaseDurabilityMetadata(seeded.Handoff.BoundaryMetadata, expectedRecordCount: 1, privateMarker);

        var createResponse = await InvokeTrainingIntakeRouteAsync(registry, new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
            tenantId,
            idempotencyKey = createIdempotencyKey,
            handoff = seeded.Handoff,
            requestedByRef = "architect:modelserver-training-intake-route-create-test",
            reason = "route-level approved handoff intake creation; local only and not ProductReady",
            requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(203)
        });

        AssertSanitizedTrainingIntakeBody(createResponse, privateMarker);
        Assert.DoesNotContain(tenantId, createResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(createIdempotencyKey, createResponse.BodyJson, StringComparison.Ordinal);
        using var createDocument = JsonDocument.Parse(createResponse.BodyJson);
        var createRoot = createDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(createRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
        var createMetadata = createRoot.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, createMetadata.GetProperty("training_intake_admission_boundary_satisfied").GetString());
        Assert.StartsWith(
            "training-intake-tenant-id:sha256:",
            createMetadata.GetProperty("training_intake_tenant_id_ref").GetString(),
            StringComparison.Ordinal);
        Assert.StartsWith(
            "training-intake-idempotency-key:sha256:",
            createMetadata.GetProperty("training_intake_idempotency_key_ref").GetString(),
            StringComparison.Ordinal);
        AssertTrainingDatabaseDurabilityMetadata(createMetadata, expectedRecordCount: 1, privateMarker);
        Assert.Equal(1, createRoot.GetProperty("intakeCount").GetInt32());
        var intakeId = createRoot.GetProperty("intakeId").GetString();
        AssertSafeTrainingIntakeScalar(intakeId, privateMarker);
        Assert.Equal(intakeId, createRoot.GetProperty("intake").GetProperty("intakeId").GetString());
        Assert.Equal(intakeId, createRoot.GetProperty("intakes")[0].GetProperty("intakeId").GetString());
        Assert.True(createRoot.GetProperty("intake").GetProperty("trainingIntakeOnly").GetBoolean());
        Assert.True(createRoot.GetProperty("intake").GetProperty("productNotReady").GetBoolean());
        var intakeMetadata = createRoot.GetProperty("intake").GetProperty("boundaryMetadata");
        Assert.Equal(
            createMetadata.GetProperty("training_intake_tenant_id_ref").GetString(),
            intakeMetadata.GetProperty("training_intake_tenant_id_ref").GetString());
        Assert.Equal(
            createMetadata.GetProperty("training_intake_idempotency_key_ref").GetString(),
            intakeMetadata.GetProperty("training_intake_idempotency_key_ref").GetString());
        AssertTrainingDatabaseDurabilityMetadata(intakeMetadata, expectedRecordCount: 1, privateMarker);

        var listResponse = await InvokeTrainingIntakeRouteAsync(registry, new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionList
        });
        AssertSanitizedTrainingIntakeBody(listResponse, privateMarker);
        using (var listDocument = JsonDocument.Parse(listResponse.BodyJson))
        {
            var listedIntakeMetadata = listDocument.RootElement
                .GetProperty("intakes")[0]
                .GetProperty("boundaryMetadata");
            AssertTrainingDatabaseDurabilityMetadata(listedIntakeMetadata, expectedRecordCount: 1, privateMarker);
        }

        var runResponse = await InvokeTrainingIntakeRouteAsync(registry, new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionRun,
            tenantId,
            idempotencyKey = runIdempotencyKey,
            intakeId,
            options
        });

        AssertSanitizedTrainingIntakeBody(runResponse, privateMarker);
        using var runDocument = JsonDocument.Parse(runResponse.BodyJson);
        var runRoot = runDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(runRoot, DesignConversionModelServerTrainingIntakeHandler.ActionRun);
        Assert.Equal(intakeId, runRoot.GetProperty("intakeId").GetString());
        var trainingRun = runRoot.GetProperty("trainingRun");
        Assert.Equal(intakeId, trainingRun.GetProperty("trainingIntakeId").GetString());
        Assert.False(trainingRun.GetProperty("trainingStarted").GetBoolean());
        Assert.False(trainingRun.GetProperty("checkpointMutated").GetBoolean());
        Assert.False(trainingRun.GetProperty("weightsProduced").GetBoolean());
        Assert.False(trainingRun.GetProperty("modelUploadReady").GetBoolean());
        Assert.NotEqual("none", trainingRun.GetProperty("runnerBlockedReason").GetString());
        Assert.Contains(trainingRun.GetProperty("blockReasons").EnumerateArray(), item =>
            string.Equals(item.GetString(), "source_disjoint_not_proven_required", StringComparison.Ordinal) ||
            string.Equals(item.GetString(), "training_not_started", StringComparison.Ordinal));
        var runMetadata = trainingRun.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, runMetadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, runMetadata.GetProperty("model_upload_ready").GetString());
        Assert.Equal(bool.FalseString, runMetadata.GetProperty("promotion_upload_attempted").GetString());
        AssertTrainingDatabaseDurabilityMetadata(runMetadata, expectedRecordCount: 0, privateMarker);

        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-route-create-run-promote-promotion-output");
        var promoteResponse = await InvokeTrainingIntakeRouteAsync(registry, new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
            tenantId,
            idempotencyKey = promoteIdempotencyKey,
            intakeId,
            promotionOutputDirectory,
            options
        });

        AssertSanitizedTrainingIntakeBody(promoteResponse, privateMarker);
        Assert.DoesNotContain(promotionOutputDirectory, promoteResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), promoteResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetFileName(promotionOutputDirectory), promoteResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
        using var promoteDocument = JsonDocument.Parse(promoteResponse.BodyJson);
        var promoteRoot = promoteDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(promoteRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);
        Assert.Equal(intakeId, promoteRoot.GetProperty("intakeId").GetString());
        var localPromotion = promoteRoot.GetProperty("localPromotion");
        Assert.False(localPromotion.GetProperty("localPromotionReady").GetBoolean());
        Assert.False(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
        Assert.StartsWith(
            "local-promotion-output-directory:sha256:",
            localPromotion.GetProperty("promotionOutputDirectoryRef").GetString(),
            StringComparison.Ordinal);
        var promotedCheckpointRef = localPromotion.GetProperty("promotedCheckpointRef").GetString();
        Assert.True(
            string.IsNullOrEmpty(promotedCheckpointRef) ||
            promotedCheckpointRef.StartsWith("local-promoted-checkpoint-path:sha256:", StringComparison.Ordinal),
            promotedCheckpointRef);
        Assert.Equal(intakeId, localPromotion.GetProperty("trainingRun").GetProperty("trainingIntakeId").GetString());
        Assert.Contains(localPromotion.GetProperty("blockReasons").EnumerateArray(), item =>
            string.Equals(item.GetString(), "local_promotion_blocked_training_intake_preflight", StringComparison.Ordinal) ||
            string.Equals(item.GetString(), "model_upload_not_ready", StringComparison.Ordinal));

        var localPromotionMetadata = localPromotion.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("external_cloud_upload_attempted").GetString());
        Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("external_cloud_upload_not_attempted").GetString());
        Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("local_promotion_not_ready").GetString());
        AssertTrainingDatabaseDurabilityMetadata(localPromotionMetadata, expectedRecordCount: 0, privateMarker);
    }

    [Fact]
    public async Task PostTrainingIntake_Create_RejectsMissingTenantOrIdempotencyBeforeDomainApi()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ADMISSION_BOUNDARY_SECRET";
        var seeded = await CreateApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);

        await AssertTrainingIntakeCountAsync(seeded.Db, expectedCount: 0);
        await AssertMutatingActionRequiresTenantAndIdempotencyAsync(
            registry,
            seeded.Db,
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["handoff"] = seeded.Handoff,
                ["requestedByRef"] = "architect:modelserver-training-intake-admission-test",
                ["reason"] = "route-level admission boundary test"
            },
            expectedIntakeCount: 0);
        await AssertMutatingActionRequiresTenantAndIdempotencyAsync(
            registry,
            seeded.Db,
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionRun,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["intakeId"] = "training-intake-admission-run-should-not-reach-domain"
            },
            expectedIntakeCount: 0);
        await AssertMutatingActionRequiresTenantAndIdempotencyAsync(
            registry,
            seeded.Db,
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["intakeId"] = "training-intake-admission-promote-should-not-reach-domain",
                ["promotionOutputDirectory"] = Path.Combine(
                    Path.GetTempPath(),
                    "training-intake-admission-promote-should-not-reach-domain")
            },
            expectedIntakeCount: 0);
        await AssertMutatingActionRequiresTenantAndIdempotencyAsync(
            registry,
            seeded.Db,
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["intakeId"] = "training-intake-admission-lifecycle-should-not-reach-domain",
                ["requestedByRef"] = "architect:modelserver-training-intake-admission-test",
                ["reason"] = "route-level lifecycle admission boundary test",
                ["promotionOutputDirectory"] = Path.Combine(
                    Path.GetTempPath(),
                    "training-intake-admission-lifecycle-should-not-reach-domain")
            },
            expectedIntakeCount: 0);
    }

    [Fact]
    public async Task PostTrainingIntake_ConfiguredTokenRejectsMissingAndInvalidBeforeSideEffects()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_AUTH_ADMISSION_SECRET";
        const string expectedToken = "local-training-intake-expected-token";
        const string invalidToken = "local-training-intake-invalid-token";
        const string tenantId = "tenant:modelserver-training-intake-auth-admission";
        const string idempotencyKey = "idempotency:modelserver-training-intake-auth-admission";
        var seeded = await CreateApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db, expectedToken);
        var body = new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
            tenantId,
            idempotencyKey,
            handoff = seeded.Handoff,
            requestedByRef = "architect:modelserver-training-intake-auth-admission-test",
            reason = "configured local auth token admission boundary test",
            requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(207)
        };

        await AssertTrainingIntakeCountAsync(seeded.Db, expectedCount: 0);

        var missingTokenResponse = await InvokeTrainingIntakeRouteAsync(registry, body);

        AssertTrainingIntakeRouteError(
            missingTokenResponse,
            "TRAINING_INTAKE_AUTH_TOKEN_REQUIRED",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
            expectedAdmissionSatisfied: false,
            expectedStatusCode: 401);
        AssertAuthAdmissionRejectedWithoutRawSecrets(
            missingTokenResponse,
            expectedToken,
            invalidToken,
            tenantId,
            idempotencyKey);
        await AssertTrainingIntakeCountAsync(seeded.Db, expectedCount: 0);

        var invalidTokenResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            body,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName] = invalidToken
            });

        AssertTrainingIntakeRouteError(
            invalidTokenResponse,
            "TRAINING_INTAKE_AUTH_TOKEN_INVALID",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
            expectedAdmissionSatisfied: false,
            expectedStatusCode: 401);
        AssertAuthAdmissionRejectedWithoutRawSecrets(
            invalidTokenResponse,
            expectedToken,
            invalidToken,
            tenantId,
            idempotencyKey);
        await AssertTrainingIntakeCountAsync(seeded.Db, expectedCount: 0);

        var validTokenResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            body,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName] = expectedToken
            });

        AssertSanitizedTrainingIntakeBody(validTokenResponse, privateMarker);
        Assert.DoesNotContain(expectedToken, validTokenResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(invalidToken, validTokenResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantId, validTokenResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(idempotencyKey, validTokenResponse.BodyJson, StringComparison.Ordinal);
        using var validDocument = JsonDocument.Parse(validTokenResponse.BodyJson);
        var validRoot = validDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(validRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
        var validMetadata = validRoot.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, validMetadata.GetProperty("training_intake_auth_token_configured").GetString());
        Assert.Equal(bool.TrueString, validMetadata.GetProperty("training_intake_auth_token_required").GetString());
        Assert.Equal(bool.TrueString, validMetadata.GetProperty("training_intake_auth_token_enforced").GetString());
        Assert.Equal(bool.TrueString, validMetadata.GetProperty("training_intake_auth_token_verified").GetString());
        Assert.Equal(
            DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName,
            validMetadata.GetProperty("training_intake_auth_token_header_name").GetString());
        Assert.Equal(bool.FalseString, validMetadata.GetProperty("product_ready").GetString());
        await AssertTrainingIntakeCountAsync(seeded.Db, expectedCount: 1);
    }

    [Fact]
    public async Task PostTrainingIntake_Create_DedupesSameTenantIdempotencyPayloadWithoutSecondIntake()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_CREATE_IDEMPOTENCY_SECRET";
        var seeded = await CreateApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-create-idempotency";
        const string idempotencyKey = "idempotency:create-training-intake-replay";
        var body = new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
            tenantId,
            idempotencyKey,
            handoff = seeded.Handoff,
            requestedByRef = "architect:modelserver-training-intake-idempotency-test",
            reason = "route-level create idempotency replay test; local only and not ProductReady",
            requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(206)
        };

        var firstResponse = await InvokeTrainingIntakeRouteAsync(registry, body);
        var secondResponse = await InvokeTrainingIntakeRouteAsync(registry, body);

        AssertSanitizedTrainingIntakeBody(firstResponse, privateMarker);
        AssertSanitizedTrainingIntakeBody(secondResponse, privateMarker);
        Assert.DoesNotContain(tenantId, firstResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(idempotencyKey, firstResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantId, secondResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(idempotencyKey, secondResponse.BodyJson, StringComparison.Ordinal);

        using var firstDocument = JsonDocument.Parse(firstResponse.BodyJson);
        using var secondDocument = JsonDocument.Parse(secondResponse.BodyJson);
        var firstRoot = firstDocument.RootElement;
        var secondRoot = secondDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(firstRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
        AssertSafeTrainingIntakeRouteRoot(secondRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
        Assert.Equal(firstRoot.GetProperty("intakeId").GetString(), secondRoot.GetProperty("intakeId").GetString());
        Assert.Equal(firstRoot.GetProperty("intake").GetProperty("intakeId").GetString(), secondRoot.GetProperty("intake").GetProperty("intakeId").GetString());
        Assert.Equal(1, firstRoot.GetProperty("intakeCount").GetInt32());
        Assert.Equal(1, secondRoot.GetProperty("intakeCount").GetInt32());
        Assert.False(firstRoot.GetProperty("productReady").GetBoolean());
        Assert.False(secondRoot.GetProperty("productReady").GetBoolean());

        var firstMetadata = firstRoot.GetProperty("boundaryMetadata");
        var secondMetadata = secondRoot.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
        Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
        Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
        Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
        Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
        Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
        Assert.Equal(
            "tenant_id_idempotency_key_same_payload_create_replay_cache_v1",
            secondMetadata.GetProperty("training_intake_create_replay_cache_boundary").GetString());
        Assert.Equal(
            "create_training_intake_append_only_once",
            secondMetadata.GetProperty("training_intake_idempotency_side_effect_boundary").GetString());

        var listed = await new DesignConversionDpoUsefulnessApi(seeded.Db).ListTrainingIntakeRequestsAsync();
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        Assert.Equal(firstRoot.GetProperty("intakeId").GetString(), stored.IntakeId);
        Assert.Equal(bool.TrueString, stored.BoundaryMetadata["training_intake_replay_cache_checked"]);
        Assert.Equal(bool.FalseString, stored.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
        Assert.Equal(bool.TrueString, stored.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"]);
    }

    [Fact]
    public async Task PostTrainingIntake_Create_ConcurrentSameTenantIdempotencyPayloadAppendsOnlyOneIntake()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_CREATE_CONCURRENCY_SECRET";
        var seeded = await CreateApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-create-concurrency";
        const string idempotencyKey = "idempotency:create-training-intake-concurrency-replay";
        var body = new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
            tenantId,
            idempotencyKey,
            handoff = seeded.Handoff,
            requestedByRef = "architect:modelserver-training-intake-concurrency-test",
            reason = "route-level create idempotency concurrency regression; local only and not ProductReady",
            requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(207)
        };

        var responses = await Task.WhenAll(
            InvokeTrainingIntakeRouteAsync(registry, body),
            InvokeTrainingIntakeRouteAsync(registry, body));

        foreach (var response in responses)
        {
            AssertSanitizedTrainingIntakeBody(response, privateMarker);
            Assert.DoesNotContain(tenantId, response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(idempotencyKey, response.BodyJson, StringComparison.Ordinal);
        }

        using var firstDocument = JsonDocument.Parse(responses[0].BodyJson);
        using var secondDocument = JsonDocument.Parse(responses[1].BodyJson);
        var firstRoot = firstDocument.RootElement;
        var secondRoot = secondDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(firstRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
        AssertSafeTrainingIntakeRouteRoot(secondRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
        Assert.Equal(firstRoot.GetProperty("intakeId").GetString(), secondRoot.GetProperty("intakeId").GetString());
        Assert.Equal(firstRoot.GetProperty("intake").GetProperty("intakeId").GetString(), secondRoot.GetProperty("intake").GetProperty("intakeId").GetString());
        Assert.Equal(1, firstRoot.GetProperty("intakeCount").GetInt32());
        Assert.Equal(1, secondRoot.GetProperty("intakeCount").GetInt32());
        Assert.False(firstRoot.GetProperty("productReady").GetBoolean());
        Assert.False(secondRoot.GetProperty("productReady").GetBoolean());

        var responseMetadata = new[]
        {
            firstRoot.GetProperty("boundaryMetadata"),
            secondRoot.GetProperty("boundaryMetadata")
        };
        Assert.All(responseMetadata, metadata =>
            Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_replay_cache_checked").GetString()));
        Assert.Equal(
            1,
            responseMetadata.Count(metadata =>
                metadata.GetProperty("training_intake_idempotency_replay_hit").GetString() == bool.FalseString
                && metadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString() == bool.TrueString
                && metadata.GetProperty("training_intake_idempotent_replay_result").GetString() == bool.FalseString));
        Assert.Equal(
            1,
            responseMetadata.Count(metadata =>
                metadata.GetProperty("training_intake_idempotency_replay_hit").GetString() == bool.TrueString
                && metadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString() == bool.FalseString
                && metadata.GetProperty("training_intake_idempotent_replay_result").GetString() == bool.TrueString));
        Assert.All(responseMetadata, metadata =>
            Assert.Equal(
                "create_training_intake_append_only_once",
                metadata.GetProperty("training_intake_idempotency_side_effect_boundary").GetString()));

        var listed = await new DesignConversionDpoUsefulnessApi(seeded.Db).ListTrainingIntakeRequestsAsync();
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        Assert.Equal(firstRoot.GetProperty("intakeId").GetString(), stored.IntakeId);
        Assert.Equal(bool.TrueString, stored.BoundaryMetadata["training_intake_replay_cache_checked"]);
        Assert.Equal(bool.FalseString, stored.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
        Assert.Equal(bool.TrueString, stored.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"]);
    }

    [Fact]
    public async Task PostTrainingIntake_Create_DoesNotReplayAcrossTenantsWithSameIdempotencyKey()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_CREATE_TENANT_ISOLATION_SECRET";
        var seeded = await CreateApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantA = "tenant:modelserver-training-intake-create-isolation-a";
        const string tenantB = "tenant:modelserver-training-intake-create-isolation-b";
        const string idempotencyKey = "idempotency:create-training-intake-tenant-isolation";

        object BuildBody(string tenantId) => new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
            tenantId,
            idempotencyKey,
            handoff = seeded.Handoff,
            requestedByRef = "architect:modelserver-training-intake-tenant-isolation-test",
            reason = "route-level create idempotency tenant isolation regression; local only and not ProductReady",
            requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(207)
        };

        var firstResponse = await InvokeTrainingIntakeRouteAsync(registry, BuildBody(tenantA));
        var secondResponse = await InvokeTrainingIntakeRouteAsync(registry, BuildBody(tenantB));

        AssertSanitizedTrainingIntakeBody(firstResponse, privateMarker);
        AssertSanitizedTrainingIntakeBody(secondResponse, privateMarker);
        Assert.DoesNotContain(tenantA, firstResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantB, firstResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(idempotencyKey, firstResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantA, secondResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantB, secondResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(idempotencyKey, secondResponse.BodyJson, StringComparison.Ordinal);

        using var firstDocument = JsonDocument.Parse(firstResponse.BodyJson);
        using var secondDocument = JsonDocument.Parse(secondResponse.BodyJson);
        var firstRoot = firstDocument.RootElement;
        var secondRoot = secondDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(firstRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
        AssertSafeTrainingIntakeRouteRoot(secondRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
        Assert.NotEqual(firstRoot.GetProperty("intakeId").GetString(), secondRoot.GetProperty("intakeId").GetString());
        Assert.NotEqual(
            firstRoot.GetProperty("intake").GetProperty("intakeId").GetString(),
            secondRoot.GetProperty("intake").GetProperty("intakeId").GetString());
        Assert.Equal(1, firstRoot.GetProperty("intakeCount").GetInt32());
        Assert.Equal(1, secondRoot.GetProperty("intakeCount").GetInt32());
        Assert.False(firstRoot.GetProperty("productReady").GetBoolean());
        Assert.False(secondRoot.GetProperty("productReady").GetBoolean());

        var firstMetadata = firstRoot.GetProperty("boundaryMetadata");
        var secondMetadata = secondRoot.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
        Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
        Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
        Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
        Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
        Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
        Assert.NotEqual(
            firstMetadata.GetProperty("training_intake_tenant_id_ref").GetString(),
            secondMetadata.GetProperty("training_intake_tenant_id_ref").GetString());
        Assert.Equal(
            firstMetadata.GetProperty("training_intake_idempotency_key_ref").GetString(),
            secondMetadata.GetProperty("training_intake_idempotency_key_ref").GetString());
        Assert.Equal(
            "tenant_id_idempotency_key_same_payload_create_replay_cache_v1",
            secondMetadata.GetProperty("training_intake_create_replay_cache_boundary").GetString());

        var listed = await new DesignConversionDpoUsefulnessApi(seeded.Db).ListTrainingIntakeRequestsAsync();
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Equal(2, listed.Value!.Count);
        Assert.Equal(
            2,
            listed.Value!
                .Select(record => record.IntakeId)
                .Distinct(StringComparer.Ordinal)
                .Count());
    }

    [Fact]
    public async Task PostTrainingIntake_Run_DedupesSameTenantIdempotencyPayloadWithoutSecondRunRecord()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_RUN_IDEMPOTENCY_SECRET";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-run-idempotency";
        const string createIdempotencyKey = "idempotency:create-before-run-idempotency-replay";
        const string runIdempotencyKey = "idempotency:run-training-intake-replay";
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-run-idempotency",
            "training-intake-run-idempotency-dataset",
            "training-intake-run-idempotency-gate",
            sourceDisjoint: true);

        var createResponse = await InvokeTrainingIntakeRouteAsync(registry, new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
            tenantId,
            idempotencyKey = createIdempotencyKey,
            handoff = seeded.Handoff,
            requestedByRef = "architect:modelserver-training-intake-run-idempotency-test",
            reason = "route-level run idempotency setup; local only and not ProductReady",
            requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(210)
        });

        using var createDocument = JsonDocument.Parse(createResponse.BodyJson);
        var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();
        var runBody = new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionRun,
            tenantId,
            idempotencyKey = runIdempotencyKey,
            intakeId,
            options
        };

        var firstResponse = await InvokeTrainingIntakeRouteAsync(registry, runBody);
        var secondResponse = await InvokeTrainingIntakeRouteAsync(registry, runBody);

        AssertSanitizedTrainingIntakeBody(firstResponse, privateMarker);
        AssertSanitizedTrainingIntakeBody(secondResponse, privateMarker);
        Assert.DoesNotContain(tenantId, firstResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(runIdempotencyKey, firstResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantId, secondResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(runIdempotencyKey, secondResponse.BodyJson, StringComparison.Ordinal);

        using var firstDocument = JsonDocument.Parse(firstResponse.BodyJson);
        using var secondDocument = JsonDocument.Parse(secondResponse.BodyJson);
        var firstRoot = firstDocument.RootElement;
        var secondRoot = secondDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(firstRoot, DesignConversionModelServerTrainingIntakeHandler.ActionRun);
        AssertSafeTrainingIntakeRouteRoot(secondRoot, DesignConversionModelServerTrainingIntakeHandler.ActionRun);
        Assert.Equal(firstRoot.GetProperty("intakeId").GetString(), secondRoot.GetProperty("intakeId").GetString());
        Assert.False(firstRoot.GetProperty("productReady").GetBoolean());
        Assert.False(secondRoot.GetProperty("productReady").GetBoolean());

        var firstRun = firstRoot.GetProperty("trainingRun");
        var secondRun = secondRoot.GetProperty("trainingRun");
        Assert.Equal(firstRun.GetProperty("runId").GetString(), secondRun.GetProperty("runId").GetString());
        Assert.True(firstRun.GetProperty("trainingRunnerInvoked").GetBoolean());
        Assert.True(secondRun.GetProperty("trainingRunnerInvoked").GetBoolean());
        Assert.Equal(100, firstRun.GetProperty("resolvedTrainingRecordCount").GetInt32());
        Assert.Equal(100, secondRun.GetProperty("resolvedTrainingRecordCount").GetInt32());
        Assert.False(firstRun.GetProperty("boundaryMetadata").GetProperty("product_ready").GetString() == bool.TrueString);
        Assert.False(secondRun.GetProperty("boundaryMetadata").GetProperty("product_ready").GetString() == bool.TrueString);

        var firstMetadata = firstRoot.GetProperty("boundaryMetadata");
        var secondMetadata = secondRoot.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
        Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
        Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
        Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
        Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
        Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
        Assert.Equal(
            "tenant_id_idempotency_key_same_intake_options_run_replay_cache_v1",
            secondMetadata.GetProperty("training_intake_run_replay_cache_boundary").GetString());
        Assert.Equal(
            "run_training_intake_append_only_once",
            secondMetadata.GetProperty("training_intake_idempotency_side_effect_boundary").GetString());

        var runRecordsPath = Path.Combine(seeded.RootDirectory, "training-intake-runs.jsonl");
        Assert.True(File.Exists(runRecordsPath));
        Assert.Single(File.ReadAllLines(runRecordsPath).Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    [Fact]
    public async Task PostTrainingIntake_PromoteLocal_DedupesSameTenantIdempotencyPayloadWithoutSecondPromotionRecord()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_PROMOTE_LOCAL_IDEMPOTENCY_SECRET";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-promote-local-idempotency";
        const string createIdempotencyKey = "idempotency:create-before-promote-local-idempotency-replay";
        const string promoteIdempotencyKey = "idempotency:promote-local-training-intake-replay";
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-promote-local-idempotency",
            "training-intake-promote-local-idempotency-dataset",
            "training-intake-promote-local-idempotency-gate",
            sourceDisjoint: true);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-promote-local-idempotency-output");

        try
        {
            var createResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-promote-local-idempotency-test",
                reason = "route-level promote-local idempotency setup; local only and not ProductReady",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(211)
            });

            using var createDocument = JsonDocument.Parse(createResponse.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();
            var promoteBody = new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = promoteIdempotencyKey,
                intakeId,
                promotionOutputDirectory,
                options
            };

            var firstResponse = await InvokeTrainingIntakeRouteAsync(registry, promoteBody);
            var filesAfterFirstPromotion = Directory.GetFiles(promotionOutputDirectory, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Order(StringComparer.Ordinal)
                .ToArray();
            var secondResponse = await InvokeTrainingIntakeRouteAsync(registry, promoteBody);
            var filesAfterSecondPromotion = Directory.GetFiles(promotionOutputDirectory, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            AssertSanitizedTrainingIntakeBody(firstResponse, privateMarker);
            AssertSanitizedTrainingIntakeBody(secondResponse, privateMarker);
            Assert.DoesNotContain(tenantId, firstResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promoteIdempotencyKey, firstResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(tenantId, secondResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promoteIdempotencyKey, secondResponse.BodyJson, StringComparison.Ordinal);
            Assert.NotEmpty(filesAfterFirstPromotion);
            Assert.Equal(filesAfterFirstPromotion, filesAfterSecondPromotion);

            using var firstDocument = JsonDocument.Parse(firstResponse.BodyJson);
            using var secondDocument = JsonDocument.Parse(secondResponse.BodyJson);
            var firstRoot = firstDocument.RootElement;
            var secondRoot = secondDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(firstRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);
            AssertSafeTrainingIntakeRouteRoot(secondRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);
            Assert.Equal(firstRoot.GetProperty("intakeId").GetString(), secondRoot.GetProperty("intakeId").GetString());
            Assert.False(firstRoot.GetProperty("productReady").GetBoolean());
            Assert.False(secondRoot.GetProperty("productReady").GetBoolean());

            var firstPromotion = firstRoot.GetProperty("localPromotion");
            var secondPromotion = secondRoot.GetProperty("localPromotion");
            Assert.True(firstPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.True(secondPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.Equal(
                firstPromotion.GetProperty("trainingRun").GetProperty("runId").GetString(),
                secondPromotion.GetProperty("trainingRun").GetProperty("runId").GetString());
            Assert.Equal(
                firstPromotion.GetProperty("promotedLocalModelId").GetString(),
                secondPromotion.GetProperty("promotedLocalModelId").GetString());
            Assert.Equal(
                firstPromotion.GetProperty("promotedCheckpointSha256").GetString(),
                secondPromotion.GetProperty("promotedCheckpointSha256").GetString());

            var firstMetadata = firstRoot.GetProperty("boundaryMetadata");
            var secondMetadata = secondRoot.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
            Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
            Assert.Equal(
                "tenant_id_idempotency_key_same_intake_run_options_output_promote_local_replay_cache_v1",
                secondMetadata.GetProperty("training_intake_promote_local_replay_cache_boundary").GetString());
            Assert.Equal(
                "promote_local_append_and_local_artifacts_once",
                secondMetadata.GetProperty("training_intake_idempotency_side_effect_boundary").GetString());

            var promotionRecordsPath = Path.Combine(seeded.RootDirectory, "training-intake-local-promotions.jsonl");
            Assert.True(File.Exists(promotionRecordsPath));
            Assert.Single(File.ReadAllLines(promotionRecordsPath).Where(line => !string.IsNullOrWhiteSpace(line)));
        }
        finally
        {
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_ContinueTrainingUploadStatus_DedupesSameTenantIdempotencyPayloadWithoutSecondStatusRecord()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_UPLOAD_STATUS_IDEMPOTENCY_SECRET";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-upload-status-idempotency";
        const string createIdempotencyKey = "idempotency:create-before-upload-status-idempotency-replay";
        const string uploadStatusIdempotencyKey = "idempotency:continue-training-upload-status-replay";
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-upload-status-idempotency",
            "training-intake-upload-status-idempotency-dataset",
            "training-intake-upload-status-idempotency-gate",
            sourceDisjoint: true);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-upload-status-idempotency-output");

        try
        {
            var createResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-upload-status-idempotency-test",
                reason = "route-level upload-status idempotency setup; local only and not ProductReady",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(212)
            });

            using var createDocument = JsonDocument.Parse(createResponse.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();
            var uploadStatusBody = new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus,
                tenantId,
                idempotencyKey = uploadStatusIdempotencyKey,
                intakeId,
                requestedByRef = "architect:modelserver-training-intake-upload-status-idempotency-test",
                reason = "continue-train local model and report upload/deploy status idempotently",
                promotionOutputDirectory,
                options
            };

            var firstResponse = await InvokeTrainingIntakeRouteAsync(registry, uploadStatusBody);
            var filesAfterFirstStatus = Directory.GetFiles(promotionOutputDirectory, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Order(StringComparer.Ordinal)
                .ToArray();
            var secondResponse = await InvokeTrainingIntakeRouteAsync(registry, uploadStatusBody);
            var filesAfterSecondStatus = Directory.GetFiles(promotionOutputDirectory, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            AssertSanitizedTrainingIntakeBody(firstResponse, privateMarker);
            AssertSanitizedTrainingIntakeBody(secondResponse, privateMarker);
            Assert.DoesNotContain(tenantId, firstResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(uploadStatusIdempotencyKey, firstResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(tenantId, secondResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(uploadStatusIdempotencyKey, secondResponse.BodyJson, StringComparison.Ordinal);
            Assert.NotEmpty(filesAfterFirstStatus);
            Assert.Equal(filesAfterFirstStatus, filesAfterSecondStatus);

            using var firstDocument = JsonDocument.Parse(firstResponse.BodyJson);
            using var secondDocument = JsonDocument.Parse(secondResponse.BodyJson);
            var firstRoot = firstDocument.RootElement;
            var secondRoot = secondDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(firstRoot, DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus);
            AssertSafeTrainingIntakeRouteRoot(secondRoot, DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus);
            Assert.Equal(firstRoot.GetProperty("intakeId").GetString(), secondRoot.GetProperty("intakeId").GetString());
            Assert.False(firstRoot.GetProperty("productReady").GetBoolean());
            Assert.False(secondRoot.GetProperty("productReady").GetBoolean());

            var firstLifecycle = firstRoot.GetProperty("lifecycleStatus");
            var secondLifecycle = secondRoot.GetProperty("lifecycleStatus");
            Assert.Equal(firstLifecycle.GetProperty("commandId").GetString(), secondLifecycle.GetProperty("commandId").GetString());
            Assert.Equal(
                firstLifecycle.GetProperty("promotedRuntimeModelId").GetString(),
                secondLifecycle.GetProperty("promotedRuntimeModelId").GetString());
            Assert.True(firstLifecycle.GetProperty("uploadRequested").GetBoolean());
            Assert.True(secondLifecycle.GetProperty("uploadRequested").GetBoolean());
            Assert.False(firstLifecycle.GetProperty("uploadAttempted").GetBoolean());
            Assert.False(secondLifecycle.GetProperty("uploadAttempted").GetBoolean());
            Assert.False(secondLifecycle.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(secondLifecycle.GetProperty("productReady").GetBoolean());

            var firstMetadata = firstRoot.GetProperty("boundaryMetadata");
            var secondMetadata = secondRoot.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
            Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
            Assert.Equal(
                "tenant_id_idempotency_key_same_intake_run_options_output_promote_local_replay_cache_v1",
                secondMetadata.GetProperty("training_intake_promote_local_replay_cache_boundary").GetString());
            Assert.Equal(
                "promote_local_append_and_local_artifacts_once",
                secondMetadata.GetProperty("training_intake_idempotency_side_effect_boundary").GetString());

            var secondLifecycleMetadata = secondLifecycle.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, secondLifecycleMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
            Assert.Equal(bool.TrueString, secondLifecycleMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.FalseString, secondLifecycleMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(bool.FalseString, secondLifecycleMetadata.GetProperty("upload_attempted").GetString());
            Assert.Equal(bool.FalseString, secondLifecycleMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, secondLifecycleMetadata.GetProperty("product_ready").GetString());

            var promotionRecordsPath = Path.Combine(seeded.RootDirectory, "training-intake-local-promotions.jsonl");
            Assert.True(File.Exists(promotionRecordsPath));
            Assert.Single(File.ReadAllLines(promotionRecordsPath).Where(line => !string.IsNullOrWhiteSpace(line)));
        }
        finally
        {
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_ContinueTrainingUploadStatus_ReturnsCheckpointAndRuntimeModelRefsWithoutDeployClaim()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_LIFECYCLE_COMMAND_SECRET";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-lifecycle";
        const string idempotencyKey = "idempotency:continue-training-upload-status-lifecycle";
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:modelserver-lifecycle-command-test",
            "local continue-training upload-status command test; refs only and not ProductReady",
            DateTimeOffset.UnixEpoch.AddMinutes(204));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);

        var options = new DesignConversionTrainingPipelineOptions(
            RunId: "training-intake-lifecycle-command",
            BatchSize: 25,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "training-intake-lifecycle-command-dataset",
                MinimumTrainRecords: 50,
                MinimumHeldOutRecords: 50,
                Preflight: DesignConversionDpoDatasetPreflight.LocalTrainingIntakeSourceManifestDisjointProof()),
            MeasurementGateRequest: BuildPassingLifecycleMeasurementGate());
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-lifecycle-command-promotion-output");

        var response = await InvokeTrainingIntakeRouteAsync(registry, new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus,
            tenantId,
            idempotencyKey,
            intakeId = intake.Value!.IntakeId,
            requestedByRef = "architect:modelserver-lifecycle-command-test",
            reason = "continue-train local model and report upload/deploy status without real deploy",
            promotionOutputDirectory,
            options
        });

        AssertSanitizedTrainingIntakeBody(response, privateMarker);
        Assert.DoesNotContain(tenantId, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(idempotencyKey, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(promotionOutputDirectory, response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetFileName(promotionOutputDirectory), response.BodyJson, StringComparison.OrdinalIgnoreCase);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        AssertSafeTrainingIntakeRouteRoot(root, DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus);
        Assert.Equal(intake.Value.IntakeId, root.GetProperty("intakeId").GetString());
        Assert.False(root.GetProperty("productReady").GetBoolean());
        Assert.False(root.GetProperty("deploymentVerified").GetBoolean());

        var localPromotion = root.GetProperty("localPromotion");
        Assert.True(localPromotion.GetProperty("localPromotionReady").GetBoolean());
        Assert.False(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
        Assert.StartsWith(
            "local-promoted-checkpoint-path:sha256:",
            localPromotion.GetProperty("promotedCheckpointRef").GetString(),
            StringComparison.Ordinal);
        var promotedModelId = localPromotion.GetProperty("promotedLocalModelId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(promotedModelId));
        Assert.True(localPromotion.GetProperty("localOnly").GetBoolean());

        var lifecycleStatus = root.GetProperty("lifecycleStatus");
        Assert.Equal("continue-training-and-upload-deploy-status", lifecycleStatus.GetProperty("requestedOperation").GetString());
        Assert.True(lifecycleStatus.GetProperty("continueTrainingRequested").GetBoolean());
        Assert.Equal(
            DesignConversionLocalContinueTrainingProbe.VerifiedStatus,
            lifecycleStatus.GetProperty("continueTrainingStatus").GetString());
        Assert.True(lifecycleStatus.GetProperty("localContinueTrainingVerified").GetBoolean());
        Assert.True(lifecycleStatus.GetProperty("uploadRequested").GetBoolean());
        Assert.False(lifecycleStatus.GetProperty("uploadAttempted").GetBoolean());
        Assert.StartsWith(
            "blocked_local_only_no_external_upload",
            lifecycleStatus.GetProperty("uploadStatus").GetString(),
            StringComparison.Ordinal);
        Assert.Equal("not_uploaded", lifecycleStatus.GetProperty("cloudUploadStatus").GetString());
        Assert.StartsWith(
            "local://design-conversion/promoted/",
            lifecycleStatus.GetProperty("localPublishPackageRef").GetString(),
            StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(lifecycleStatus.GetProperty("localPublishManifestRef").GetString()));
        Assert.Equal("none", lifecycleStatus.GetProperty("localPublishBackupRef").GetString());
        Assert.Equal("explicit_unsupported_nonready_boundary", lifecycleStatus.GetProperty("rollbackBoundaryRef").GetString());
        Assert.DoesNotContain(
            promotionOutputDirectory,
            lifecycleStatus.GetProperty("localPublishPackageRef").GetString(),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            promotionOutputDirectory,
            lifecycleStatus.GetProperty("localPublishManifestRef").GetString(),
            StringComparison.OrdinalIgnoreCase);
        Assert.True(lifecycleStatus.GetProperty("deployRequested").GetBoolean());
        Assert.False(lifecycleStatus.GetProperty("deploymentAttempted").GetBoolean());
        Assert.False(lifecycleStatus.GetProperty("deploymentVerified").GetBoolean());
        Assert.Equal("blocked_local_only_no_deploy", lifecycleStatus.GetProperty("deploymentStatus").GetString());
        Assert.False(lifecycleStatus.GetProperty("productReady").GetBoolean());
        Assert.Equal(intake.Value.IntakeId, lifecycleStatus.GetProperty("trainingIntakeId").GetString());
        Assert.Equal(promotedModelId, lifecycleStatus.GetProperty("promotedRuntimeModelId").GetString());
        Assert.StartsWith(
            "checkpoint-sha256:",
            lifecycleStatus.GetProperty("promotedCheckpointId").GetString(),
            StringComparison.Ordinal);
        Assert.StartsWith(
            "local-promoted-checkpoint-path:sha256:",
            lifecycleStatus.GetProperty("promotedCheckpointPathRef").GetString(),
            StringComparison.Ordinal);
        Assert.True(lifecycleStatus.GetProperty("localPromotionReady").GetBoolean());
        Assert.True(lifecycleStatus.GetProperty("localOnly").GetBoolean());

        var lifecycleMetadata = lifecycleStatus.GetProperty("boundaryMetadata");
        Assert.Equal("continue_training_upload_deploy_status", lifecycleMetadata.GetProperty("lifecycle_command_surface").GetString());
        Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("continue_training_requested").GetString());
        Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("local_continue_training_verified").GetString());
        Assert.StartsWith(
            "blocked_local_only_no_external_upload",
            lifecycleMetadata.GetProperty("upload_status").GetString(),
            StringComparison.Ordinal);
        Assert.Equal("not_uploaded", lifecycleMetadata.GetProperty("cloud_upload_status").GetString());
        Assert.Equal(
            lifecycleStatus.GetProperty("localPublishPackageRef").GetString(),
            lifecycleMetadata.GetProperty("local_publish_package_ref").GetString());
        Assert.Equal(
            lifecycleStatus.GetProperty("localPublishManifestRef").GetString(),
            lifecycleMetadata.GetProperty("local_publish_manifest_ref").GetString());
        Assert.Equal(
            lifecycleStatus.GetProperty("localPublishBackupRef").GetString(),
            lifecycleMetadata.GetProperty("local_publish_backup_ref").GetString());
        Assert.Equal(
            lifecycleStatus.GetProperty("rollbackBoundaryRef").GetString(),
            lifecycleMetadata.GetProperty("local_publish_rollback_boundary_ref").GetString());
        Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("upload_attempted").GetString());
        Assert.Equal("blocked_local_only_no_deploy", lifecycleMetadata.GetProperty("deployment_status").GetString());
        Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("deployment_attempted").GetString());
        Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("product_ready").GetString());
        Assert.Equal(lifecycleStatus.GetProperty("promotedCheckpointId").GetString(), lifecycleMetadata.GetProperty("promoted_checkpoint_id").GetString());
        Assert.Equal(promotedModelId, lifecycleMetadata.GetProperty("promoted_runtime_model_id").GetString());
        Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("local_only_lifecycle_status").GetString());
        Assert.Contains(lifecycleStatus.GetProperty("evidenceRefs").EnumerateArray(), item =>
            string.Equals(item.GetString(), "DESIGN_CONVERSION_CONTINUE_TRAINING_UPLOAD_STATUS_PRODUCT_READY_FALSE", StringComparison.Ordinal));
        Assert.Contains(lifecycleStatus.GetProperty("warnings").EnumerateArray(), item =>
            string.Equals(item.GetString(), "upload_deploy_blocked_local_only", StringComparison.Ordinal));
        Assert.Contains(lifecycleStatus.GetProperty("warnings").EnumerateArray(), item =>
            string.Equals(item.GetString(), "product_ready=false", StringComparison.Ordinal));
        Assert.Contains(lifecycleStatus.GetProperty("blockReasons").EnumerateArray(), item =>
            string.Equals(item.GetString(), "external_upload_deploy_not_proven", StringComparison.Ordinal));

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_continue_training_upload_status_supported").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    [Fact]
    public async Task PostTrainingIntake_ContinueTrainingUploadStatus_ReturnsUnsupportedRollbackBoundaryAndStaysNonReady()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ROLLBACK_BOUNDARY_SECRET";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-rollback-boundary";
        const string idempotencyKey = "idempotency:continue-training-upload-status-rollback-boundary";
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:modelserver-training-intake-rollback-boundary-test",
            "local promotion rollback boundary command test; refs only and not ProductReady",
            DateTimeOffset.UnixEpoch.AddMinutes(214));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);

        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-rollback-boundary",
            "training-intake-rollback-boundary-dataset",
            "training-intake-rollback-boundary-gate",
            sourceDisjoint: true);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-rollback-boundary-promotion-output");

        try
        {
            var response = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus,
                tenantId,
                idempotencyKey,
                intakeId = intake.Value!.IntakeId,
                requestedByRef = "architect:modelserver-training-intake-rollback-boundary-test",
                reason = "continue-train local model and expose rollback unsupported nonready boundary",
                promotionOutputDirectory,
                options
            });

            AssertSanitizedTrainingIntakeBody(response, privateMarker);
            Assert.DoesNotContain(tenantId, response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(idempotencyKey, response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promotionOutputDirectory, response.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), response.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFileName(promotionOutputDirectory), response.BodyJson, StringComparison.OrdinalIgnoreCase);

            using var document = JsonDocument.Parse(response.BodyJson);
            var root = document.RootElement;
            AssertSafeTrainingIntakeRouteRoot(root, DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus);
            Assert.Equal(intake.Value.IntakeId, root.GetProperty("intakeId").GetString());
            Assert.False(root.GetProperty("productReady").GetBoolean());
            Assert.False(root.GetProperty("deploymentVerified").GetBoolean());

            var localPromotion = root.GetProperty("localPromotion");
            Assert.True(localPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.False(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
            Assert.StartsWith(
                "local-promoted-checkpoint-path:sha256:",
                localPromotion.GetProperty("promotedCheckpointRef").GetString(),
                StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(localPromotion.GetProperty("promotedLocalModelId").GetString()));

            var localPromotionMetadata = localPromotion.GetProperty("boundaryMetadata");
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("local_promotion_rollback_supported").GetString());
            Assert.Equal("explicit_unsupported_nonready_boundary", localPromotionMetadata.GetProperty("local_promotion_rollback_boundary").GetString());
            Assert.Equal(
                "unsupported_no_prior_active_local_model_checkpoint_state",
                localPromotionMetadata.GetProperty("local_promotion_rollback_status").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("local_promotion_previous_model_checkpoint_ref_available").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("local_promotion_restore_previous_state_supported").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("product_ready_blocked_by_local_rollback_boundary").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("product_ready").GetString());
            Assert.Contains(localPromotion.GetProperty("evidenceRefs").EnumerateArray(), item =>
                string.Equals(item.GetString(), "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_UNSUPPORTED_NONREADY_BOUNDARY", StringComparison.Ordinal));
            Assert.Contains(localPromotion.GetProperty("warnings").EnumerateArray(), item =>
                string.Equals(item.GetString(), "local_promotion_rollback_unsupported", StringComparison.Ordinal));

            var lifecycleStatus = root.GetProperty("lifecycleStatus");
            Assert.False(lifecycleStatus.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(lifecycleStatus.GetProperty("productReady").GetBoolean());
            var lifecycleMetadata = lifecycleStatus.GetProperty("boundaryMetadata");
            Assert.Equal("explicit_unsupported_nonready_boundary", lifecycleMetadata.GetProperty("local_promotion_rollback_boundary").GetString());
            Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("local_promotion_rollback_supported").GetString());
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("product_ready_blocked_by_local_rollback_boundary").GetString());
            Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("product_ready").GetString());

            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal("explicit_unsupported_nonready_boundary", metadata.GetProperty("local_promotion_rollback_boundary").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("local_promotion_rollback_supported").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("product_ready_blocked_by_local_rollback_boundary").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_RollbackLocalPromotion_RestoresPriorActiveLocalModelThroughRoute()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ROUTE_ROLLBACK_LOCAL_PROMOTION_SECRET";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-rollback-local-promotion";
        const string createIdempotencyKey = "idempotency:create-before-rollback-local-promotion";
        const string firstPromoteIdempotencyKey = "idempotency:first-promote-before-rollback-local-promotion";
        const string secondPromoteIdempotencyKey = "idempotency:second-promote-before-rollback-local-promotion";
        const string rollbackIdempotencyKey = "idempotency:rollback-local-promotion-route";
        var firstOptions = BuildTrainingIntakeActualRouteOptions(
            "training-intake-rollback-route-first",
            "training-intake-rollback-route-first-dataset",
            "training-intake-rollback-route-first-gate",
            sourceDisjoint: true);
        var secondOptions = BuildTrainingIntakeActualRouteOptions(
            "training-intake-rollback-route-second",
            "training-intake-rollback-route-second-dataset",
            "training-intake-rollback-route-second-gate",
            sourceDisjoint: true);
        var firstPromotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-rollback-route-first-output");
        var secondPromotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-rollback-route-second-output");

        try
        {
            var createResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-rollback-local-promotion-test",
                reason = "route-level local promotion rollback setup; local only and not ProductReady",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(215)
            });

            using var createDocument = JsonDocument.Parse(createResponse.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();

            var firstPromotionResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = firstPromoteIdempotencyKey,
                intakeId,
                promotionOutputDirectory = firstPromotionOutputDirectory,
                options = firstOptions
            });
            var secondPromotionResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = secondPromoteIdempotencyKey,
                intakeId,
                promotionOutputDirectory = secondPromotionOutputDirectory,
                options = secondOptions
            });

            AssertSanitizedTrainingIntakeBody(firstPromotionResponse, privateMarker);
            AssertSanitizedTrainingIntakeBody(secondPromotionResponse, privateMarker);
            Assert.DoesNotContain(firstPromotionOutputDirectory, firstPromotionResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(secondPromotionOutputDirectory, secondPromotionResponse.BodyJson, StringComparison.OrdinalIgnoreCase);

            using var firstPromotionDocument = JsonDocument.Parse(firstPromotionResponse.BodyJson);
            using var secondPromotionDocument = JsonDocument.Parse(secondPromotionResponse.BodyJson);
            var firstPromotionRoot = firstPromotionDocument.RootElement;
            var secondPromotionRoot = secondPromotionDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(firstPromotionRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);
            AssertSafeTrainingIntakeRouteRoot(secondPromotionRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);

            var firstLocalPromotion = firstPromotionRoot.GetProperty("localPromotion");
            var secondLocalPromotion = secondPromotionRoot.GetProperty("localPromotion");
            Assert.True(firstLocalPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.True(secondLocalPromotion.GetProperty("localPromotionReady").GetBoolean());

            var firstLocalPromotionMetadata = firstLocalPromotion.GetProperty("boundaryMetadata");
            var secondLocalPromotionMetadata = secondLocalPromotion.GetProperty("boundaryMetadata");
            var firstActiveRef = firstLocalPromotionMetadata.GetProperty("local_promotion_active_local_model_ref").GetString();
            var secondActiveRef = secondLocalPromotionMetadata.GetProperty("local_promotion_active_local_model_ref").GetString();
            var firstCheckpointSha256 = firstLocalPromotion.GetProperty("promotedCheckpointSha256").GetString();
            var secondPromotedModelId = secondLocalPromotion.GetProperty("promotedLocalModelId").GetString();

            Assert.False(string.IsNullOrWhiteSpace(firstActiveRef));
            Assert.False(string.IsNullOrWhiteSpace(secondActiveRef));
            Assert.NotEqual(firstActiveRef, secondActiveRef);
            Assert.False(string.IsNullOrWhiteSpace(secondPromotedModelId));
            Assert.Equal(firstActiveRef, secondLocalPromotionMetadata.GetProperty("local_promotion_prior_active_local_model_ref").GetString());
            Assert.Equal(bool.TrueString, secondLocalPromotionMetadata.GetProperty("local_promotion_rollback_supported").GetString());

            var rollbackResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion,
                tenantId,
                idempotencyKey = rollbackIdempotencyKey,
                promotedLocalModelRef = secondPromotedModelId,
                requestedByRef = "architect:modelserver-training-intake-rollback-local-promotion-test",
                reason = "rollback second local promotion and restore prior active local model",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(216)
            });

            AssertSanitizedTrainingIntakeBody(rollbackResponse, privateMarker);
            Assert.DoesNotContain(tenantId, rollbackResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rollbackIdempotencyKey, rollbackResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(firstPromotionOutputDirectory, rollbackResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(secondPromotionOutputDirectory, rollbackResponse.BodyJson, StringComparison.OrdinalIgnoreCase);

            using var rollbackDocument = JsonDocument.Parse(rollbackResponse.BodyJson);
            var rollbackRoot = rollbackDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(rollbackRoot, DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion);
            Assert.False(rollbackRoot.GetProperty("productReady").GetBoolean());
            Assert.False(rollbackRoot.GetProperty("deploymentVerified").GetBoolean());

            var rollback = rollbackRoot.GetProperty("localPromotionRollback");
            Assert.True(rollback.GetProperty("rollbackApplied").GetBoolean());
            Assert.True(rollback.GetProperty("priorStateRestored").GetBoolean());
            Assert.False(rollback.GetProperty("noPriorStateRecorded").GetBoolean());
            Assert.Equal(secondActiveRef, rollback.GetProperty("rolledBackLocalModelRef").GetString());
            Assert.Equal(secondActiveRef, rollback.GetProperty("previousActiveLocalModelRef").GetString());
            Assert.Equal(firstActiveRef, rollback.GetProperty("restoredActiveLocalModelRef").GetString());
            Assert.Equal(firstCheckpointSha256, rollback.GetProperty("restoredActiveCheckpointSha256").GetString());
            Assert.Contains(rollback.GetProperty("evidenceRefs").EnumerateArray(), item =>
                string.Equals(item.GetString(), "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_APPLIED", StringComparison.Ordinal));

            var rollbackMetadata = rollback.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, rollbackMetadata.GetProperty("local_promotion_rollback_applied").GetString());
            Assert.Equal(bool.TrueString, rollbackMetadata.GetProperty("local_promotion_prior_state_restored").GetString());
            Assert.Equal("actual_local_rollback_via_local_artifact_db_metadata", rollbackMetadata.GetProperty("local_promotion_rollback_boundary").GetString());
            Assert.Equal("restored_prior_active_local_model", rollbackMetadata.GetProperty("local_promotion_rollback_status").GetString());
            Assert.Equal(firstActiveRef, rollbackMetadata.GetProperty("local_promotion_active_local_model_ref").GetString());
            Assert.Equal(bool.FalseString, rollbackMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, rollbackMetadata.GetProperty("product_ready").GetString());

            var rootMetadata = rollbackRoot.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_local_promotion_rollback_supported").GetString());
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("local_promotion_rollback_applied").GetString());
            Assert.Equal(firstActiveRef, rootMetadata.GetProperty("local_promotion_rollback_restored_active_local_model_ref").GetString());
            Assert.Contains(
                DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion,
                rootMetadata.GetProperty("training_intake_supported_actions").GetString(),
                StringComparison.Ordinal);
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("product_ready").GetString());

            var promotionRecordsPath = Path.Combine(seeded.RootDirectory, "training-intake-local-promotions.jsonl");
            Assert.True(File.Exists(promotionRecordsPath));
            Assert.Equal(3, File.ReadAllLines(promotionRecordsPath).Count(line => !string.IsNullOrWhiteSpace(line)));
        }
        finally
        {
            if (Directory.Exists(firstPromotionOutputDirectory))
                Directory.Delete(firstPromotionOutputDirectory, recursive: true);
            if (Directory.Exists(secondPromotionOutputDirectory))
                Directory.Delete(secondPromotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_RollbackLocalPromotion_ReplaysSameTenantIdempotencyPayloadThroughRoute()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ROUTE_ROLLBACK_LOCAL_PROMOTION_REPLAY_SECRET";
        const string privateReason = "PRIVATE_ROLLBACK_LOCAL_PROMOTION_REPLAY_REASON_MARKER";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-rollback-local-promotion-replay";
        const string createIdempotencyKey = "idempotency:create-before-rollback-local-promotion-replay";
        const string firstPromoteIdempotencyKey = "idempotency:first-promote-before-rollback-local-promotion-replay";
        const string secondPromoteIdempotencyKey = "idempotency:second-promote-before-rollback-local-promotion-replay";
        const string rollbackIdempotencyKey = "idempotency:rollback-local-promotion-route-replay";
        const string requestedByRef = "architect:modelserver-training-intake-rollback-local-promotion-replay-test";
        var firstOptions = BuildTrainingIntakeActualRouteOptions(
            "training-intake-rollback-replay-route-first",
            "training-intake-rollback-replay-route-first-dataset",
            "training-intake-rollback-replay-route-first-gate",
            sourceDisjoint: true);
        var secondOptions = BuildTrainingIntakeActualRouteOptions(
            "training-intake-rollback-replay-route-second",
            "training-intake-rollback-replay-route-second-dataset",
            "training-intake-rollback-replay-route-second-gate",
            sourceDisjoint: true);
        var firstPromotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-rollback-replay-route-first-output");
        var secondPromotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-rollback-replay-route-second-output");

        try
        {
            var createResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef,
                reason = "route-level local promotion rollback replay setup; local only and not ProductReady",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(217)
            });
            Assert.Equal(200, createResponse.StatusCode);
            using var createDocument = JsonDocument.Parse(createResponse.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();

            var firstPromotionBody = new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = firstPromoteIdempotencyKey,
                intakeId,
                promotionOutputDirectory = firstPromotionOutputDirectory,
                options = firstOptions
            };
            var secondPromotionBody = new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = secondPromoteIdempotencyKey,
                intakeId,
                promotionOutputDirectory = secondPromotionOutputDirectory,
                options = secondOptions
            };

            var firstPromotionResponse = await InvokeTrainingIntakeRouteAsync(registry, firstPromotionBody);
            var secondPromotionResponse = await InvokeTrainingIntakeRouteAsync(registry, secondPromotionBody);
            Assert.Equal(200, firstPromotionResponse.StatusCode);
            Assert.Equal(200, secondPromotionResponse.StatusCode);
            AssertSanitizedTrainingIntakeBody(firstPromotionResponse, privateMarker);
            AssertSanitizedTrainingIntakeBody(secondPromotionResponse, privateMarker);

            using var firstPromotionDocument = JsonDocument.Parse(firstPromotionResponse.BodyJson);
            using var secondPromotionDocument = JsonDocument.Parse(secondPromotionResponse.BodyJson);
            var firstPromotionRoot = firstPromotionDocument.RootElement;
            var secondPromotionRoot = secondPromotionDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(firstPromotionRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);
            AssertSafeTrainingIntakeRouteRoot(secondPromotionRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);

            var firstLocalPromotion = firstPromotionRoot.GetProperty("localPromotion");
            var secondLocalPromotion = secondPromotionRoot.GetProperty("localPromotion");
            var firstActiveRef = firstLocalPromotion.GetProperty("boundaryMetadata").GetProperty("local_promotion_active_local_model_ref").GetString();
            var secondActiveRef = secondLocalPromotion.GetProperty("boundaryMetadata").GetProperty("local_promotion_active_local_model_ref").GetString();
            var firstCheckpointSha256 = firstLocalPromotion.GetProperty("promotedCheckpointSha256").GetString();
            var secondPromotedModelId = secondLocalPromotion.GetProperty("promotedLocalModelId").GetString();
            Assert.False(string.IsNullOrWhiteSpace(firstActiveRef));
            Assert.False(string.IsNullOrWhiteSpace(secondActiveRef));
            Assert.False(string.IsNullOrWhiteSpace(secondPromotedModelId));
            Assert.NotEqual(firstActiveRef, secondActiveRef);

            var rollbackBody = new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion,
                tenantId,
                idempotencyKey = rollbackIdempotencyKey,
                promotedLocalModelRef = secondPromotedModelId,
                requestedByRef,
                reason = privateReason
            };

            var firstRollbackResponse = await InvokeTrainingIntakeRouteAsync(registry, rollbackBody);
            Assert.Equal(200, firstRollbackResponse.StatusCode);
            AssertSanitizedTrainingIntakeBody(firstRollbackResponse, privateMarker);
            Assert.DoesNotContain(tenantId, firstRollbackResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rollbackIdempotencyKey, firstRollbackResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(privateReason, firstRollbackResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(firstPromotionOutputDirectory, firstRollbackResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(secondPromotionOutputDirectory, firstRollbackResponse.BodyJson, StringComparison.OrdinalIgnoreCase);

            var promotionRecordsPath = Path.Combine(seeded.RootDirectory, "training-intake-local-promotions.jsonl");
            Assert.True(File.Exists(promotionRecordsPath));
            var linesAfterFirstRollback = File.ReadAllLines(promotionRecordsPath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
            var bytesAfterFirstRollback = File.ReadAllBytes(promotionRecordsPath);
            Assert.Equal(3, linesAfterFirstRollback.Length);

            var secondRollbackResponse = await InvokeTrainingIntakeRouteAsync(registry, rollbackBody);
            Assert.Equal(200, secondRollbackResponse.StatusCode);
            AssertSanitizedTrainingIntakeBody(secondRollbackResponse, privateMarker);
            Assert.DoesNotContain(tenantId, secondRollbackResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rollbackIdempotencyKey, secondRollbackResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(privateReason, secondRollbackResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(firstPromotionOutputDirectory, secondRollbackResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(secondPromotionOutputDirectory, secondRollbackResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(bytesAfterFirstRollback, File.ReadAllBytes(promotionRecordsPath));
            Assert.Equal(3, File.ReadAllLines(promotionRecordsPath).Count(line => !string.IsNullOrWhiteSpace(line)));

            using var firstRollbackDocument = JsonDocument.Parse(firstRollbackResponse.BodyJson);
            using var secondRollbackDocument = JsonDocument.Parse(secondRollbackResponse.BodyJson);
            var firstRollbackRoot = firstRollbackDocument.RootElement;
            var secondRollbackRoot = secondRollbackDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(firstRollbackRoot, DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion);
            AssertSafeTrainingIntakeRouteRoot(secondRollbackRoot, DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion);

            var firstRollback = firstRollbackRoot.GetProperty("localPromotionRollback");
            var secondRollback = secondRollbackRoot.GetProperty("localPromotionRollback");
            Assert.Equal(firstRollback.GetProperty("rollbackId").GetString(), secondRollback.GetProperty("rollbackId").GetString());
            Assert.Equal(firstRollback.GetProperty("rolledBackLocalModelRef").GetString(), secondRollback.GetProperty("rolledBackLocalModelRef").GetString());
            Assert.Equal(firstRollback.GetProperty("previousActiveLocalModelRef").GetString(), secondRollback.GetProperty("previousActiveLocalModelRef").GetString());
            Assert.Equal(firstRollback.GetProperty("restoredActiveLocalModelRef").GetString(), secondRollback.GetProperty("restoredActiveLocalModelRef").GetString());
            Assert.Equal(firstRollback.GetProperty("restoredActiveCheckpointSha256").GetString(), secondRollback.GetProperty("restoredActiveCheckpointSha256").GetString());
            Assert.Equal(secondActiveRef, firstRollback.GetProperty("rolledBackLocalModelRef").GetString());
            Assert.Equal(secondActiveRef, firstRollback.GetProperty("previousActiveLocalModelRef").GetString());
            Assert.Equal(firstActiveRef, firstRollback.GetProperty("restoredActiveLocalModelRef").GetString());
            Assert.Equal(firstCheckpointSha256, firstRollback.GetProperty("restoredActiveCheckpointSha256").GetString());

            var firstMetadata = firstRollbackRoot.GetProperty("boundaryMetadata");
            var secondMetadata = secondRollbackRoot.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
            Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
            Assert.Equal(
                "tenant_id_idempotency_key_same_target_requester_reason_timestamp_rollback_local_replay_cache_v1",
                firstMetadata.GetProperty("training_intake_rollback_local_replay_cache_boundary").GetString());
            Assert.Equal(
                "rollback_local_promotion_append_only_once",
                firstMetadata.GetProperty("training_intake_idempotency_side_effect_boundary").GetString());
            Assert.Equal(firstMetadata.GetProperty("training_intake_rollback_local_idempotency_payload_hash").GetString(), secondMetadata.GetProperty("training_intake_rollback_local_idempotency_payload_hash").GetString());

            using var persistedRollbackDocument = JsonDocument.Parse(linesAfterFirstRollback[2]);
            var persistedRollbackMetadata = persistedRollbackDocument.RootElement.GetProperty("boundaryMetadata");
            Assert.Equal("rollback", persistedRollbackMetadata.GetProperty("local_promotion_record_kind").GetString());
            Assert.Equal(bool.TrueString, persistedRollbackMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
            Assert.Equal(bool.FalseString, persistedRollbackMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.TrueString, persistedRollbackMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(bool.FalseString, persistedRollbackMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
            Assert.Equal(
                "tenant_id_idempotency_key_same_target_requester_reason_timestamp_rollback_local_replay_cache_v1",
                persistedRollbackMetadata.GetProperty("training_intake_rollback_local_replay_cache_boundary").GetString());
            Assert.StartsWith(
                "sha256:",
                persistedRollbackMetadata.GetProperty("training_intake_rollback_local_idempotency_payload_hash").GetString(),
                StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(persistedRollbackMetadata.GetProperty("training_intake_rollback_local_recorded_at_utc").GetString()));

            var changedReasonResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion,
                tenantId,
                idempotencyKey = rollbackIdempotencyKey,
                promotedLocalModelRef = secondPromotedModelId,
                requestedByRef,
                reason = "changed rollback reason for replay conflict"
            });
            AssertTrainingIntakeRouteError(
                changedReasonResponse,
                "DESIGN_CONVERSION_TRAINING_INTAKE_ROLLBACK_LOCAL_IDEMPOTENCY_REPLAY_CONFLICT",
                privateMarker,
                expectedAction: DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion);
            using (var changedReasonDocument = JsonDocument.Parse(changedReasonResponse.BodyJson))
            {
                Assert.Equal(
                    "DesignConversion training intake rollback-local-promotion idempotency key was replayed with a different rollback payload.",
                    changedReasonDocument.RootElement.GetProperty("errorMessage").GetString());
            }
            Assert.DoesNotContain(tenantId, changedReasonResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rollbackIdempotencyKey, changedReasonResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(privateReason, changedReasonResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(firstPromotionOutputDirectory, changedReasonResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(secondPromotionOutputDirectory, changedReasonResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(bytesAfterFirstRollback, File.ReadAllBytes(promotionRecordsPath));
            Assert.Equal(3, File.ReadAllLines(promotionRecordsPath).Count(line => !string.IsNullOrWhiteSpace(line)));

            var explicitTimestampResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion,
                tenantId,
                idempotencyKey = rollbackIdempotencyKey,
                promotedLocalModelRef = secondPromotedModelId,
                requestedByRef,
                reason = privateReason,
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(218)
            });
            AssertTrainingIntakeRouteError(
                explicitTimestampResponse,
                "DESIGN_CONVERSION_TRAINING_INTAKE_ROLLBACK_LOCAL_IDEMPOTENCY_REPLAY_CONFLICT",
                privateMarker,
                expectedAction: DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion);
            using (var explicitTimestampDocument = JsonDocument.Parse(explicitTimestampResponse.BodyJson))
            {
                Assert.Equal(
                    "DesignConversion training intake rollback-local-promotion idempotency key was replayed with a different rollback payload.",
                    explicitTimestampDocument.RootElement.GetProperty("errorMessage").GetString());
            }
            Assert.DoesNotContain(tenantId, explicitTimestampResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rollbackIdempotencyKey, explicitTimestampResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(privateReason, explicitTimestampResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(firstPromotionOutputDirectory, explicitTimestampResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(secondPromotionOutputDirectory, explicitTimestampResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(bytesAfterFirstRollback, File.ReadAllBytes(promotionRecordsPath));
            Assert.Equal(3, File.ReadAllLines(promotionRecordsPath).Count(line => !string.IsNullOrWhiteSpace(line)));

            var promotionReplayAfterRollbackResponse = await InvokeTrainingIntakeRouteAsync(registry, secondPromotionBody);
            Assert.Equal(200, promotionReplayAfterRollbackResponse.StatusCode);
            AssertSanitizedTrainingIntakeBody(promotionReplayAfterRollbackResponse, privateMarker);
            using var promotionReplayAfterRollbackDocument = JsonDocument.Parse(promotionReplayAfterRollbackResponse.BodyJson);
            var promotionReplayAfterRollbackMetadata = promotionReplayAfterRollbackDocument.RootElement.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, promotionReplayAfterRollbackMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.FalseString, promotionReplayAfterRollbackMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(
                "tenant_id_idempotency_key_same_intake_run_options_output_promote_local_replay_cache_v1",
                promotionReplayAfterRollbackMetadata.GetProperty("training_intake_promote_local_replay_cache_boundary").GetString());
            Assert.Equal(bytesAfterFirstRollback, File.ReadAllBytes(promotionRecordsPath));
            Assert.Equal(3, File.ReadAllLines(promotionRecordsPath).Count(line => !string.IsNullOrWhiteSpace(line)));
        }
        finally
        {
            if (Directory.Exists(firstPromotionOutputDirectory))
                Directory.Delete(firstPromotionOutputDirectory, recursive: true);
            if (Directory.Exists(secondPromotionOutputDirectory))
                Directory.Delete(secondPromotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_SourceDisjointLifecycleFeedsPromotedLocalApiFactory()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ACTUAL_ROUTE_POSITIVE_SECRET";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:modelserver-training-intake-actual-route-positive",
            "actual local training-intake route lifecycle into promoted local API factory; not ProductReady",
            DateTimeOffset.UnixEpoch.AddMinutes(205));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-actual-route-positive-promotion-output");
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-actual-route-source-disjoint-positive",
            "training-intake-actual-route-source-disjoint-positive-dataset",
            "training-intake-actual-route-source-disjoint-positive-gate",
            sourceDisjoint: true);

        try
        {
            var promotion = await api.RunTrainingIntakeAndPromoteLocalAsync(
                intake.Value!.IntakeId,
                promotionOutputDirectory,
                options);

            Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
            Assert.True(promotion.Value!.LocalPromotionReady);
            Assert.False(promotion.Value.ExternalCloudUploadAttempted);
            Assert.NotNull(promotion.Value.Publication);
            Assert.Equal(
                DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
                promotion.Value.Publication!.PublicationKind);
            Assert.Equal(
                DesignConversionPromotionPublicationStatus.NotUploaded,
                promotion.Value.Publication.PublicationStatus);
            Assert.NotNull(promotion.Value.Publication.UploadReceipt);
            Assert.False(promotion.Value.Publication.UploadReceipt!.UploadAttempted);
            Assert.False(promotion.Value.Publication.UploadReceipt.UploadSucceeded);
            Assert.False(promotion.Value.Publication.UploadReceipt.VerificationSucceeded);
            Assert.Equal("local_receipt_only", promotion.Value.Publication.Manifest.PromotionReceiptKind);
            Assert.True(promotion.Value.TrainingRun.TrainingRunnerInvoked);
            Assert.True(promotion.Value.TrainingRun.TrainingStarted);
            Assert.True(promotion.Value.TrainingRun.CheckpointMutated);
            Assert.True(promotion.Value.TrainingRun.WeightsProduced);
            Assert.True(promotion.Value.TrainingRun.ModelUploadReady);
            Assert.Equal("proven", promotion.Value.TrainingRun.SourceManifestStatus);
            Assert.Equal("proven", promotion.Value.TrainingRun.SourceDisjointStatus);
            Assert.Equal(100, promotion.Value.TrainingRun.ResolvedTrainingRecordCount);
            Assert.NotNull(promotion.Value.TrainingRun.RunnerEvidence);
            Assert.True(promotion.Value.TrainingRun.RunnerEvidence!.SourceDisjoint);
            Assert.Equal("proven", promotion.Value.TrainingRun.RunnerEvidence.SourceDisjointStatus);
            Assert.Equal(
                "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS",
                promotion.Value.TrainingRun.RunnerEvidence.PromotionStatus);
            Assert.Contains(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                promotion.Value.EvidenceRefs);
            Assert.NotNull(promotion.Value.PromotedLocalModel);
            Assert.True(promotion.Value.PromotedLocalModel!.CheckpointFreshLoadVerified);
            Assert.True(promotion.Value.PromotedLocalModel.CheckpointContainsRealTrainedWeights);
            Assert.True(promotion.Value.PromotedLocalModel.PromotionManifestClaimed);
            Assert.True(promotion.Value.PromotedLocalModel.LocalOnly);
            Assert.True(promotion.Value.PromotedLocalModel.SourceDisjointHeldOutReadinessGatePassed);
            Assert.True(promotion.Value.PromotedLocalModel.DpoTrainerPolicyCheckpointConverted);
            Assert.True(promotion.Value.PromotedLocalModel.DpoTrainerPolicyCheckpointFreshLoaded);
            Assert.Equal(
                "source_disjoint_heldout_gate_passed",
                promotion.Value.PromotedLocalModel.SourceDisjointHeldOutReadinessStatus);
            Assert.Equal(
                "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted",
                promotion.Value.PromotedLocalModel.LocalCheckpointWeightsSource);
            Assert.Equal("11.DPO/DpoTrainer", promotion.Value.PromotedLocalModel.SourceProvenanceRef);
            Assert.StartsWith(
                "dpo_policy_checkpoint_sha256:",
                promotion.Value.PromotedLocalModel.DpoPolicyCheckpointSha256Ref,
                StringComparison.Ordinal);
            Assert.StartsWith(
                "dpo_policy_checkpoint_file:dpo_policy_step_",
                promotion.Value.PromotedLocalModel.DpoPolicyCheckpointFileRef,
                StringComparison.Ordinal);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["local_promotion_ready"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["external_cloud_upload_attempted"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["model_server_upload_not_attempted_local_receipt"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["model_server_promotion_not_claimed_local_receipt"]);
            Assert.Equal(
                "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted",
                promotion.Value.BoundaryMetadata["local_promoted_checkpoint_weights_source"]);
            Assert.Equal("11.DPO/DpoTrainer", promotion.Value.BoundaryMetadata["local_promoted_source_provenance_ref"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["local_promoted_dpo_trainer_policy_checkpoint_converted"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["local_promoted_dpo_trainer_policy_checkpoint_fresh_loaded"]);
            Assert.StartsWith(
                "dpo_policy_checkpoint_sha256:",
                promotion.Value.BoundaryMetadata["local_promoted_dpo_policy_checkpoint_sha256_ref"],
                StringComparison.Ordinal);
            Assert.StartsWith(
                "dpo_policy_checkpoint_file:dpo_policy_step_",
                promotion.Value.BoundaryMetadata["local_promoted_dpo_policy_checkpoint_file_ref"],
                StringComparison.Ordinal);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, promotion.Value.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, promotion.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO", promotion.Value.EvidenceRefs);

            var factory = await DesignConversionModelServerApi.CreateFromTrainingIntakeLocalPromotionAsync(
                promotion.Value,
                new TrainingIntakeRecordingRenderProvider(),
                new TrainingIntakeRecordingComparisonProvider(CreateTrainingIntakeVerdict(
                    "training-intake-actual-route-promoted-factory-verdict",
                    0.91,
                    passed: true)),
                () => DateTimeOffset.Parse("2026-07-08T11:00:00Z"));

            Assert.True(factory.IsSuccess, factory.ErrorMessage);
            Assert.True(factory.Value!.PromotedLoad.Model.CheckpointFreshLoadVerified);
            Assert.True(factory.Value.PromotedLoad.Model.CheckpointContainsRealTrainedWeights);
            Assert.True(factory.Value.PromotedLoad.Model.LocalOnly);
            Assert.True(factory.Value.PromotedLoad.Model.SourceDisjointHeldOutReadinessGatePassed);
            Assert.Contains(
                "DESIGN_CONVERSION_MODEL_SERVER_API_CREATED_FROM_TRAINING_INTAKE_LOCAL_PROMOTION",
                factory.Value.EvidenceRefs);
            Assert.Contains(
                "DESIGN_CONVERSION_MODEL_SERVER_API_LOCAL_PROMOTION_RUNTIME_PRODUCT_READY_FALSE",
                factory.Value.EvidenceRefs);
            Assert.Contains(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                factory.Value.EvidenceRefs);
            Assert.True(factory.Value.PromotedLoad.Model.DpoTrainerPolicyCheckpointConverted);
            Assert.True(factory.Value.PromotedLoad.Model.DpoTrainerPolicyCheckpointFreshLoaded);
            Assert.Equal(
                promotion.Value.PromotedLocalModel.DpoPolicyCheckpointSha256Ref,
                factory.Value.PromotedLoad.Model.DpoPolicyCheckpointSha256Ref);
            Assert.Equal(
                promotion.Value.PromotedLocalModel.DpoPolicyCheckpointFileRef,
                factory.Value.PromotedLoad.Model.DpoPolicyCheckpointFileRef);
            var promotedCheckpointWeights = (factory.Value.PromotedLoad.Checkpoint.CriticUnit.LearnedWeights ?? Enumerable.Empty<DesignConversionLearnedWeightCheckpoint>())
                .Concat(factory.Value.PromotedLoad.Checkpoint.ImproverUnit.LearnedWeights ?? Enumerable.Empty<DesignConversionLearnedWeightCheckpoint>())
                .ToArray();
            Assert.Contains(promotedCheckpointWeights, weight =>
                weight.Derivation.Contains("source=11.DPO/DpoTrainer", StringComparison.Ordinal) &&
                weight.Derivation.Contains("policy_checkpoint=fresh_loaded_converted", StringComparison.Ordinal) &&
                weight.EvidenceRefs.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, StringComparer.Ordinal) &&
                weight.EvidenceRefs.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, StringComparer.Ordinal) &&
                weight.EvidenceRefs.Contains(promotion.Value.PromotedLocalModel.DpoPolicyCheckpointSha256Ref, StringComparer.Ordinal));

            var response = await factory.Value.Api.SubmitAndEvaluateAsync(
                CreateTrainingIntakeModelServerRequest("training-intake-actual-route-promoted-factory-request"));

            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.True(response.Value!.ModelTrainingVerified);
            Assert.False(response.Value.ModelUploadReady);
            Assert.False(response.Value.DeploymentVerified);
            Assert.False(response.Value.PublicHttpEndpointVerified);
            Assert.False(response.Value.ProductReady);
            Assert.False(response.Value.TrainedModelAuthority);
            Assert.True(response.Value.NotTrainedModelAuthority);
            Assert.Equal("promoted_local_checkpoint_loaded", response.Value.TrainingReadiness.Status);
            Assert.Equal(
                "source_disjoint_heldout_gate_passed",
                response.Value.TrainingReadiness.SourceDisjointStatus);
            Assert.False(response.Value.TrainingReadiness.ModelUploadReady);
            Assert.False(response.Value.TrainingReadiness.ProductReady);
            Assert.NotNull(response.Value.PromotedLocalModel);
            Assert.True(response.Value.PromotedLocalModel!.DpoTrainerPolicyCheckpointConverted);
            Assert.True(response.Value.PromotedLocalModel.DpoTrainerPolicyCheckpointFreshLoaded);
            Assert.Equal(
                "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted",
                response.Value.PromotedLocalModel.LocalCheckpointWeightsSource);
            Assert.Equal("11.DPO/DpoTrainer", response.Value.PromotedLocalModel.SourceProvenanceRef);
            Assert.Equal(
                promotion.Value.PromotedLocalModel.DpoPolicyCheckpointSha256Ref,
                response.Value.PromotedLocalModel.DpoPolicyCheckpointSha256Ref);
            Assert.Equal(
                promotion.Value.PromotedLocalModel.DpoPolicyCheckpointFileRef,
                response.Value.PromotedLocalModel.DpoPolicyCheckpointFileRef);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, response.Value.PromotedLocalModel.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, response.Value.PromotedLocalModel.EvidenceRefs);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["trained_model_authority"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["not_trained_model_authority"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_model_promotion_verified"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_checkpoint_loaded"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_checkpoint_real_weights"]);
            Assert.Equal(
                "source_disjoint_heldout_gate_passed",
                response.Value.BoundaryMetadata["source_disjoint_status"]);
            Assert.Equal(
                "source_disjoint_heldout_gate_passed",
                response.Value.BoundaryMetadata["local_promoted_source_disjoint_heldout_readiness_status"]);
            Assert.Equal(
                bool.TrueString,
                response.Value.BoundaryMetadata["local_promoted_source_disjoint_heldout_readiness_gate_passed"]);
            Assert.Equal(
                "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted",
                response.Value.BoundaryMetadata["local_promoted_checkpoint_weights_source"]);
            Assert.Equal("11.DPO/DpoTrainer", response.Value.BoundaryMetadata["local_promoted_source_provenance_ref"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_dpo_trainer_policy_checkpoint_converted"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_dpo_trainer_policy_checkpoint_fresh_loaded"]);
            Assert.Contains(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                response.Value.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence, response.Value.EvidenceRefs);
            Assert.DoesNotContain(privateMarker, JsonSerializer.Serialize(promotion.Value), StringComparison.Ordinal);
            Assert.DoesNotContain(privateMarker, JsonSerializer.Serialize(response.Value), StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_ConfiguredUploaderFailureIsCalledOnceWithoutLocalFallback()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_CONFIGURED_UPLOADER_SECRET";
        const string failureCode = "DESIGN_CONVERSION_CONFIGURED_PROMOTION_UPLOAD_FAILED";
        var uploader = new FailingTrainingIntakePromotionUploader(
            failureCode,
            $@"Configured uploader failed at C:\Users\private-user\private\{privateMarker}\checkpoint.json?token=credential");
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, uploader);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:modelserver-training-intake-configured-uploader-failure",
            "configured promotion uploader failure must not fall back to local receipt",
            DateTimeOffset.UnixEpoch.AddMinutes(206));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-configured-uploader-failure-output");
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-configured-uploader-failure",
            "training-intake-configured-uploader-failure-dataset",
            "training-intake-configured-uploader-failure-gate",
            sourceDisjoint: true);

        try
        {
            var promotion = await api.RunTrainingIntakeAndPromoteLocalAsync(
                intake.Value!.IntakeId,
                promotionOutputDirectory,
                options);

            Assert.False(promotion.IsSuccess);
            Assert.Equal(failureCode, promotion.ErrorCode);
            Assert.Equal("DesignConversion promotion uploader failed.", promotion.ErrorMessage);
            Assert.Equal(1, uploader.UploadCallCount);
            Assert.DoesNotContain(privateMarker, promotion.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(@"C:\Users\", promotion.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(Path.Combine(
                seeded.RootDirectory,
                "training-intake-local-promotions.jsonl")));
        }
        finally
        {
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_StatusOnlySourceDisjointProofDoesNotReachPromotedLocalApiFactory()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ACTUAL_ROUTE_NEGATIVE_SECRET";
        var seeded = await CreateSingleSourcePersistedApprovedHandoffAsync(privateMarker);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:modelserver-training-intake-actual-route-negative",
            "status-only source-disjoint claim must not bypass actual local intake proof",
            DateTimeOffset.UnixEpoch.AddMinutes(206));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-actual-route-negative-promotion-output");
        var statusOnlyOptions = BuildTrainingIntakeActualRouteOptions(
            "training-intake-actual-route-status-only-negative",
            "training-intake-actual-route-status-only-negative-dataset",
            "training-intake-actual-route-status-only-negative-gate",
            sourceDisjoint: true);

        try
        {
            var promotion = await api.RunTrainingIntakeAndPromoteLocalAsync(
                intake.Value!.IntakeId,
                promotionOutputDirectory,
                statusOnlyOptions);

            Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
            Assert.False(promotion.Value!.LocalPromotionReady);
            Assert.False(promotion.Value.ExternalCloudUploadAttempted);
            Assert.False(promotion.Value.TrainingRun.TrainingRunnerInvoked);
            Assert.False(promotion.Value.TrainingRun.TrainingStarted);
            Assert.False(promotion.Value.TrainingRun.CheckpointMutated);
            Assert.False(promotion.Value.TrainingRun.WeightsProduced);
            Assert.False(promotion.Value.TrainingRun.ModelUploadReady);
            Assert.Equal("proven", promotion.Value.TrainingRun.SourceManifestStatus);
            Assert.Equal("blocked_same_source", promotion.Value.TrainingRun.SourceDisjointStatus);
            Assert.Contains(
                promotion.Value.TrainingRun.BlockReasons,
                reason => reason.Contains("source_disjoint", StringComparison.Ordinal));
            Assert.Contains(
                "local_promotion_blocked_training_intake_preflight",
                promotion.Value.BlockReasons);
            Assert.Null(promotion.Value.PromotedLocalModel);
            Assert.DoesNotContain(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                promotion.Value.EvidenceRefs);
            Assert.DoesNotContain(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, promotion.Value.EvidenceRefs);
            Assert.DoesNotContain(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, promotion.Value.EvidenceRefs);
            Assert.DoesNotContain(DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence, promotion.Value.EvidenceRefs);
            Assert.DoesNotContain("DESIGN_CONVERSION_DPO_TRAINER_POLICY_CHECKPOINT_SOURCE_11_DPO", promotion.Value.EvidenceRefs);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["local_promotion_ready"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["local_promotion_not_ready"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["external_cloud_upload_attempted"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["local_promoted_dpo_trainer_policy_checkpoint_converted"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["local_promoted_dpo_trainer_policy_checkpoint_fresh_loaded"]);
            Assert.Equal(string.Empty, promotion.Value.BoundaryMetadata["local_promoted_checkpoint_weights_source"]);
            Assert.Equal(string.Empty, promotion.Value.BoundaryMetadata["local_promoted_source_provenance_ref"]);
            Assert.Equal(string.Empty, promotion.Value.BoundaryMetadata["local_promoted_dpo_policy_checkpoint_sha256_ref"]);
            Assert.Equal(string.Empty, promotion.Value.BoundaryMetadata["local_promoted_dpo_policy_checkpoint_file_ref"]);

            var factory = await DesignConversionModelServerApi.CreateFromTrainingIntakeLocalPromotionAsync(
                promotion.Value,
                new TrainingIntakeRecordingRenderProvider(),
                new TrainingIntakeRecordingComparisonProvider(CreateTrainingIntakeVerdict(
                    "training-intake-actual-route-status-only-factory-verdict",
                    0.91,
                    passed: true)));

            Assert.False(factory.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_LOCAL_PROMOTION_READY_REQUIRED", factory.ErrorCode);
            Assert.DoesNotContain(privateMarker, JsonSerializer.Serialize(promotion.Value), StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_Preflight_DelegatesThroughHandlerAndKeepsLocalOnlyBoundary()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_PREFLIGHT_SECRET";
        var seeded = await CreateSeededIntakeAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);

        var response = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            BodyJson: JsonSerializer.Serialize(new
            {
                action = "preflight",
                intakeId = seeded.IntakeId
            }),
            ContentType: "application/json"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        AssertSanitizedTrainingIntakeBody(response.Value!, privateMarker);
        using var document = JsonDocument.Parse(response.Value!.BodyJson);
        var root = document.RootElement;
        Assert.Equal("preflight", root.GetProperty("action").GetString());
        Assert.Equal(seeded.IntakeId, root.GetProperty("intakeId").GetString());
        Assert.False(root.GetProperty("productReady").GetBoolean());
        Assert.Contains(root.GetProperty("warnings").EnumerateArray(), item =>
            string.Equals(item.GetString(), "training_intake_route_local_only=true", StringComparison.Ordinal));
        Assert.Contains(root.GetProperty("warnings").EnumerateArray(), item =>
            string.Equals(item.GetString(), "product_ready=false", StringComparison.Ordinal));

        var preflight = root.GetProperty("preflight");
        Assert.Equal(seeded.IntakeId, preflight.GetProperty("trainingIntakeId").GetString());
        Assert.False(preflight.GetProperty("trainingStarted").GetBoolean());
        Assert.False(preflight.GetProperty("checkpointMutated").GetBoolean());
        Assert.False(preflight.GetProperty("weightsProduced").GetBoolean());
        Assert.False(preflight.GetProperty("modelUploadReady").GetBoolean());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal("preflight", metadata.GetProperty("training_intake_action").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), metadata.GetProperty("delegates_to_domain_api").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    [Fact]
    public async Task TryRunTrainingIntakeAsync_LocalCaller_ReturnsStableRefsOnlyEnvelopeForListAndPreflight()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_LOCAL_CALLER_SECRET";
        var seeded = await CreateSeededIntakeAsync(privateMarker);
        var adapter = new DesignConversionModelServerLocalCallerAdapter(CreateRegistry(seeded.Db));

        var listEnvelope = await adapter.TryRunTrainingIntakeAsync(
            new DesignConversionModelServerTrainingIntakeRequest(Action: "list"),
            callerSourceKind: "training-intake-local-caller-test");

        AssertTrainingIntakeCallerEnvelope(listEnvelope, privateMarker);
        Assert.Contains("action", listEnvelope.RequiredRequestFields);
        Assert.Contains("intakeId", listEnvelope.RequiredRequestFields);
        Assert.Contains("options", listEnvelope.RequiredRequestFields);
        Assert.Contains("promotionOutputDirectory", listEnvelope.RequiredRequestFields);
        Assert.Contains("requestedByRef", listEnvelope.RequiredRequestFields);
        Assert.Contains("reason", listEnvelope.RequiredRequestFields);
        using (var listDocument = JsonDocument.Parse(listEnvelope.ResponseBodyJson))
        {
            var root = listDocument.RootElement;
            Assert.Equal("list", root.GetProperty("action").GetString());
            Assert.Equal(1, root.GetProperty("intakeCount").GetInt32());
            Assert.Equal(seeded.IntakeId, root.GetProperty("intakes")[0].GetProperty("intakeId").GetString());
        }

        var preflightEnvelope = await adapter.TryRunTrainingIntakeAsync(
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: "preflight",
                IntakeId: seeded.IntakeId),
            callerSourceKind: "training-intake-local-caller-test");

        AssertTrainingIntakeCallerEnvelope(preflightEnvelope, privateMarker);
        using var preflightDocument = JsonDocument.Parse(preflightEnvelope.ResponseBodyJson);
        var preflightRoot = preflightDocument.RootElement;
        Assert.Equal("preflight", preflightRoot.GetProperty("action").GetString());
        Assert.Equal(seeded.IntakeId, preflightRoot.GetProperty("intakeId").GetString());
        Assert.False(preflightRoot.GetProperty("productReady").GetBoolean());
        Assert.False(preflightRoot.GetProperty("deploymentVerified").GetBoolean());
        Assert.False(preflightRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());

        var metadata = preflightRoot.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_route_local_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_response_refs_only").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    [Fact]
    public async Task PostTrainingIntake_AspNetMapper_MapsKestrelLoopbackRouteAndAnnotatesLocalOnlyBoundary()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ASPNET_SECRET";
        const string tenantId = "tenant:modelserver-training-intake-aspnet-create";
        const string idempotencyKey = "idempotency:modelserver-training-intake-aspnet-create";
        const string missingTenantIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-missing-tenant";
        var seeded = await CreateSingleSourcePersistedApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var routeBuilder = app.MapDesignConversionModelServerTrainingIntake(registry);
        Assert.NotNull(routeBuilder);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            async Task<(HttpResponseMessage Response, string BodyJson)> PostTrainingIntakeAsync(object body)
            {
                using var content = new StringContent(
                    JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    content);
                var bodyJson = await response.Content.ReadAsStringAsync();
                return (response, bodyJson);
            }

            var missingTenant = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                idempotencyKey = missingTenantIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-aspnet-admission-test",
                reason = "aspnet mapper admission rejection should stay local only and refs only"
            });
            using var missingTenantResponse = missingTenant.Response;

            Assert.Equal(HttpStatusCode.BadRequest, missingTenantResponse.StatusCode);
            Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
            Assert.True(baseAddress.Port > 0);
            Assert.Equal(
                bool.TrueString,
                missingTenantResponse.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                missingTenantResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
            Assert.Equal(
                bool.FalseString,
                missingTenantResponse.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
            Assert.Equal(
                bool.FalseString,
                missingTenantResponse.Headers.GetValues("x-design-conversion-product-ready").Single());
            AssertNoPrivateTrainingIntakePayload(missingTenant.BodyJson, privateMarker);
            Assert.DoesNotContain(missingTenantIdempotencyKey, missingTenant.BodyJson, StringComparison.Ordinal);

            using var missingTenantDocument = JsonDocument.Parse(missingTenant.BodyJson);
            var missingTenantRoot = missingTenantDocument.RootElement;
            Assert.Equal("TRAINING_INTAKE_TENANT_ID_REQUIRED", missingTenantRoot.GetProperty("errorCode").GetString());
            Assert.Equal(400, missingTenantRoot.GetProperty("statusCode").GetInt32());
            var missingTenantMetadata = missingTenantRoot.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), missingTenantMetadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(bool.TrueString, missingTenantMetadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, missingTenantMetadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(bool.FalseString, missingTenantMetadata.GetProperty("runtime_invoked").GetString());
            Assert.Equal(DesignConversionModelServerTrainingIntakeHandler.ActionCreate, missingTenantMetadata.GetProperty("training_intake_action").GetString());
            Assert.Equal(bool.TrueString, missingTenantMetadata.GetProperty("training_intake_mutating_admission_boundary_required").GetString());
            Assert.Equal(bool.TrueString, missingTenantMetadata.GetProperty("training_intake_tenant_id_required").GetString());
            Assert.Equal(bool.TrueString, missingTenantMetadata.GetProperty("training_intake_idempotency_key_required").GetString());
            Assert.Equal(bool.FalseString, missingTenantMetadata.GetProperty("training_intake_admission_boundary_satisfied").GetString());
            Assert.Equal(bool.TrueString, missingTenantMetadata.GetProperty("training_intake_response_refs_only").GetString());
            Assert.Equal(bool.FalseString, missingTenantMetadata.GetProperty("public_http_endpoint_verified").GetString());
            Assert.Equal(bool.FalseString, missingTenantMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, missingTenantMetadata.GetProperty("product_ready").GetString());

            var created = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-aspnet-create-test",
                reason = "aspnet mapper mutating admission should hash refs and stay local only",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(204)
            });
            using var createdResponse = created.Response;

            Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
            Assert.Equal(
                bool.TrueString,
                createdResponse.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                createdResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
            Assert.Equal(
                bool.FalseString,
                createdResponse.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
            Assert.Equal(
                bool.FalseString,
                createdResponse.Headers.GetValues("x-design-conversion-product-ready").Single());
            AssertNoPrivateTrainingIntakePayload(created.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, created.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(idempotencyKey, created.BodyJson, StringComparison.Ordinal);

            using var document = JsonDocument.Parse(created.BodyJson);
            var root = document.RootElement;
            Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath, root.GetProperty("endpoint").GetString());
            Assert.Equal("POST", root.GetProperty("method").GetString());
            Assert.Equal(DesignConversionModelServerTrainingIntakeHandler.ApiShape, root.GetProperty("apiShape").GetString());
            Assert.Equal(DesignConversionModelServerTrainingIntakeHandler.ActionCreate, root.GetProperty("action").GetString());
            Assert.Equal(1, root.GetProperty("intakeCount").GetInt32());
            Assert.False(root.GetProperty("productReady").GetBoolean());
            Assert.False(root.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());
            Assert.True(root.GetProperty("intake").GetProperty("trainingIntakeOnly").GetBoolean());
            Assert.True(root.GetProperty("intake").GetProperty("productNotReady").GetBoolean());
            Assert.True(root.GetProperty("intake").GetProperty("sourceDisjointNotProven").GetBoolean());

            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), metadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                metadata.GetProperty("asp_net_route_path").GetString());
            Assert.Equal(
                nameof(DesignConversionModelServerTrainingIntakeHandler),
                metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_response_refs_only").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_mutating_admission_boundary_required").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_tenant_id_required").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_idempotency_key_required").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_admission_boundary_satisfied").GetString());
            Assert.Equal(
                "route_admission_plus_local_create_replay_cache_no_auth_token_no_full_tenant_isolation",
                metadata.GetProperty("training_intake_admission_boundary_scope").GetString());
            Assert.StartsWith(
                "training-intake-tenant-id:sha256:",
                metadata.GetProperty("training_intake_tenant_id_ref").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "training-intake-idempotency-key:sha256:",
                metadata.GetProperty("training_intake_idempotency_key_ref").GetString(),
                StringComparison.Ordinal);
            Assert.Equal(bool.FalseString, metadata.GetProperty("training_intake_auth_token_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_replay_cache_checked").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("training_intake_full_tenant_isolation_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());

            var intakeMetadata = root.GetProperty("intake").GetProperty("boundaryMetadata");
            Assert.Equal(
                metadata.GetProperty("training_intake_tenant_id_ref").GetString(),
                intakeMetadata.GetProperty("training_intake_tenant_id_ref").GetString());
            Assert.Equal(
                metadata.GetProperty("training_intake_idempotency_key_ref").GetString(),
                intakeMetadata.GetProperty("training_intake_idempotency_key_ref").GetString());
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostTrainingIntake_AspNetMapper_RunAndPromoteLocalOverKestrelLoopbackKeepsSourceDisjointLocalOnlyBoundary()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ASPNET_RUN_PROMOTE_SECRET";
        const string tenantId = "tenant:modelserver-training-intake-aspnet-run-promote";
        const string createIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-run-promote-create";
        const string runIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-run-promote-run";
        const string promoteIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-run-promote-promote";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-aspnet-run-promote-source-disjoint",
            "training-intake-aspnet-run-promote-source-disjoint-dataset",
            "training-intake-aspnet-run-promote-source-disjoint-gate",
            sourceDisjoint: true);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-aspnet-run-promote-promotion-output");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var routeBuilder = app.MapDesignConversionModelServerTrainingIntake(registry);
        Assert.NotNull(routeBuilder);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            async Task<(HttpResponseMessage Response, string BodyJson)> PostTrainingIntakeAsync(object body)
            {
                using var content = new StringContent(
                    JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    content);
                var bodyJson = await response.Content.ReadAsStringAsync();
                return (response, bodyJson);
            }

            void AssertLocalOnlyAspNetHeaders(HttpResponseMessage response)
            {
                Assert.Equal(
                    bool.TrueString,
                    response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.Equal(
                    bool.FalseString,
                    response.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
                Assert.Equal(
                    bool.FalseString,
                    response.Headers.GetValues("x-design-conversion-product-ready").Single());
            }

            var created = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-aspnet-run-promote-test",
                reason = "aspnet mapper create before run/promote proof; local only and not ProductReady",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(207)
            });
            using var createdResponse = created.Response;

            Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
            Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
            Assert.True(baseAddress.Port > 0);
            AssertLocalOnlyAspNetHeaders(createdResponse);
            AssertNoPrivateTrainingIntakePayload(created.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, created.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(createIdempotencyKey, created.BodyJson, StringComparison.Ordinal);

            using var createDocument = JsonDocument.Parse(created.BodyJson);
            var createRoot = createDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(createRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
            var intakeId = createRoot.GetProperty("intakeId").GetString();
            AssertSafeTrainingIntakeScalar(intakeId, privateMarker);
            var createMetadata = createRoot.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), createMetadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(bool.TrueString, createMetadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, createMetadata.GetProperty("asp_net_route_matched").GetString());
            Assert.StartsWith(
                "training-intake-tenant-id:sha256:",
                createMetadata.GetProperty("training_intake_tenant_id_ref").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "training-intake-idempotency-key:sha256:",
                createMetadata.GetProperty("training_intake_idempotency_key_ref").GetString(),
                StringComparison.Ordinal);
            Assert.Equal(bool.FalseString, createMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, createMetadata.GetProperty("product_ready").GetString());

            var run = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionRun,
                tenantId,
                idempotencyKey = runIdempotencyKey,
                intakeId,
                options
            });
            using var runResponse = run.Response;

            Assert.Equal(HttpStatusCode.OK, runResponse.StatusCode);
            AssertLocalOnlyAspNetHeaders(runResponse);
            AssertNoPrivateTrainingIntakePayload(run.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, run.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(runIdempotencyKey, run.BodyJson, StringComparison.Ordinal);

            using var runDocument = JsonDocument.Parse(run.BodyJson);
            var runRoot = runDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(runRoot, DesignConversionModelServerTrainingIntakeHandler.ActionRun);
            Assert.Equal(intakeId, runRoot.GetProperty("intakeId").GetString());
            var runRootMetadata = runRoot.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), runRootMetadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.StartsWith(
                "training-intake-tenant-id:sha256:",
                runRootMetadata.GetProperty("training_intake_tenant_id_ref").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "training-intake-idempotency-key:sha256:",
                runRootMetadata.GetProperty("training_intake_idempotency_key_ref").GetString(),
                StringComparison.Ordinal);
            Assert.Equal(bool.FalseString, runRootMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, runRootMetadata.GetProperty("product_ready").GetString());

            var trainingRun = runRoot.GetProperty("trainingRun");
            Assert.Equal(intakeId, trainingRun.GetProperty("trainingIntakeId").GetString());
            Assert.True(trainingRun.GetProperty("trainingRunnerInvoked").GetBoolean());
            Assert.True(trainingRun.GetProperty("trainingStarted").GetBoolean());
            Assert.True(trainingRun.GetProperty("checkpointMutated").GetBoolean());
            Assert.True(trainingRun.GetProperty("weightsProduced").GetBoolean());
            Assert.True(trainingRun.GetProperty("modelUploadReady").GetBoolean());
            Assert.Equal("proven", trainingRun.GetProperty("sourceManifestStatus").GetString());
            Assert.Equal("proven", trainingRun.GetProperty("sourceDisjointStatus").GetString());
            Assert.Equal(100, trainingRun.GetProperty("resolvedTrainingRecordCount").GetInt32());
            var runnerEvidence = trainingRun.GetProperty("runnerEvidence");
            Assert.True(runnerEvidence.GetProperty("sourceDisjoint").GetBoolean());
            Assert.Equal("proven", runnerEvidence.GetProperty("sourceDisjointStatus").GetString());
            Assert.Equal(
                "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS",
                runnerEvidence.GetProperty("promotionStatus").GetString());
            var trainingRunMetadata = trainingRun.GetProperty("boundaryMetadata");
            Assert.Contains(trainingRunMetadata.EnumerateObject(), property =>
                property.Name.Contains("source_disjoint", StringComparison.Ordinal) &&
                string.Equals(property.Value.GetString(), "proven", StringComparison.Ordinal));
            Assert.Equal(bool.TrueString, trainingRunMetadata.GetProperty("model_upload_ready").GetString());
            Assert.Equal(bool.FalseString, trainingRunMetadata.GetProperty("promotion_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, trainingRunMetadata.GetProperty("product_ready").GetString());

            var promoted = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = promoteIdempotencyKey,
                intakeId,
                promotionOutputDirectory,
                options
            });
            using var promotedResponse = promoted.Response;

            Assert.Equal(HttpStatusCode.OK, promotedResponse.StatusCode);
            AssertLocalOnlyAspNetHeaders(promotedResponse);
            AssertNoPrivateTrainingIntakePayload(promoted.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, promoted.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promoteIdempotencyKey, promoted.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promotionOutputDirectory, promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFileName(promotionOutputDirectory), promoted.BodyJson, StringComparison.OrdinalIgnoreCase);

            using var promotedDocument = JsonDocument.Parse(promoted.BodyJson);
            var promotedRoot = promotedDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(promotedRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);
            Assert.Equal(intakeId, promotedRoot.GetProperty("intakeId").GetString());
            var promotedRootMetadata = promotedRoot.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), promotedRootMetadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.StartsWith(
                "training-intake-tenant-id:sha256:",
                promotedRootMetadata.GetProperty("training_intake_tenant_id_ref").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "training-intake-idempotency-key:sha256:",
                promotedRootMetadata.GetProperty("training_intake_idempotency_key_ref").GetString(),
                StringComparison.Ordinal);
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("product_ready").GetString());

            var localPromotion = promotedRoot.GetProperty("localPromotion");
            Assert.True(localPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.False(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
            Assert.StartsWith(
                "local-promotion-output-directory:sha256:",
                localPromotion.GetProperty("promotionOutputDirectoryRef").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "local-promoted-checkpoint-path:sha256:",
                localPromotion.GetProperty("promotedCheckpointRef").GetString(),
                StringComparison.Ordinal);
            Assert.True(localPromotion.GetProperty("checkpointFreshLoadVerified").GetBoolean());
            Assert.True(localPromotion.GetProperty("checkpointContainsRealTrainedWeights").GetBoolean());
            Assert.True(localPromotion.GetProperty("promotionManifestClaimed").GetBoolean());
            Assert.True(localPromotion.GetProperty("localOnly").GetBoolean());
            Assert.Contains(localPromotion.GetProperty("evidenceRefs").EnumerateArray(), item =>
                string.Equals(item.GetString(), DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence, StringComparison.Ordinal));

            var promotedRun = localPromotion.GetProperty("trainingRun");
            Assert.Equal(intakeId, promotedRun.GetProperty("trainingIntakeId").GetString());
            Assert.True(promotedRun.GetProperty("trainingRunnerInvoked").GetBoolean());
            Assert.Equal("proven", promotedRun.GetProperty("sourceDisjointStatus").GetString());
            Assert.True(promotedRun.GetProperty("modelUploadReady").GetBoolean());
            var localPromotionMetadata = localPromotion.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("local_promotion_ready").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("external_cloud_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("external_cloud_upload_not_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("cloud_model_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("model_server_upload_not_attempted_local_receipt").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("model_server_promotion_not_claimed_local_receipt").GetString());
        }
        finally
        {
            await app.StopAsync();
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task HostStartupConfiguredHttpPromotionUploader_MetadataScopesRemoteProviderToTrainingIntakeRoute()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "host-promotion-route-metadata");
        var uploadHandler = new CapturingPromotionUploadHttpHandler(PromotionUploadResponseMode.Success);
        using var promotionHttpClient = new HttpClient(uploadHandler, disposeHandler: true);
        var hostOptions = CreatePromotionHostOptions(root, promotionHttpClient);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(
            composition,
            new UnexpectedCandidateStrategy(),
            hostOptions,
            () => DateTimeOffset.UnixEpoch.AddMinutes(606),
            routeAdmissionOptions: CreatePromotionRouteAdmissionOptions(PromotionHostTenantId));

        await app.StartAsync();
        try
        {
            var expectedRoutes = DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet();
            var startupMetadata = app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .Select(endpoint => endpoint.Metadata.GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>())
                .Where(metadata => metadata is not null)
                .Select(metadata => metadata!)
                .ToArray();
            var metadataByRoute = startupMetadata.ToDictionary(metadata => metadata.MappedRoute, StringComparer.Ordinal);

            Assert.Equal(11, expectedRoutes.Count);
            Assert.Equal(expectedRoutes.Count, startupMetadata.Length);
            Assert.Equal(
                expectedRoutes.OrderBy(route => route, StringComparer.Ordinal).ToArray(),
                metadataByRoute.Keys.OrderBy(route => route, StringComparer.Ordinal).ToArray());

            foreach (var expectedRoute in expectedRoutes)
            {
                Assert.True(metadataByRoute.TryGetValue(expectedRoute, out var metadata), expectedRoute);
                Assert.Equal(DesignConversionModelServerAspNetHostStartup.HostStartupSurface, metadata.StartupSurface);
                Assert.Equal(DesignConversionModelServerAspNetHostStartup.HostStartupBoundary, metadata.Boundary);
                Assert.Equal(expectedRoute, metadata.MappedRoute);
                Assert.Equal(expectedRoutes, metadata.MappedRoutes);
                Assert.True(metadata.RefsOnly);
                Assert.False(metadata.ProductReady);
                Assert.False(metadata.PublicDeploymentVerified);
                Assert.False(metadata.CrossRepoCallerVerified);

                var isTrainingIntake = string.Equals(
                    expectedRoute,
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    StringComparison.Ordinal);
                Assert.Equal(isTrainingIntake, metadata.ExternalProviderConfigured);
                Assert.Equal(!isTrainingIntake, metadata.LocalOnly);
            }
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_HostStartupConfiguredHttpPromotionUploader_SendsMultipartAndReturnsVerifiedRemoteReceipt()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_HOST_HTTP_PROMOTION_SECRET";
        const string createIdempotencyKey = "idempotency:host-http-promotion-create";
        const string promoteIdempotencyKey = "idempotency:host-http-promotion-promote";
        const string runId = "training-intake-host-http-promotion";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var promotionOutputDirectory = Path.Combine(seeded.RootDirectory, "remote-promotion-output");
        var uploadHandler = new CapturingPromotionUploadHttpHandler(PromotionUploadResponseMode.Success);
        using var promotionHttpClient = new HttpClient(uploadHandler, disposeHandler: true);
        var hostOptions = CreatePromotionHostOptions(seeded.RootDirectory, promotionHttpClient);
        var routeAdmissionOptions = CreatePromotionRouteAdmissionOptions(PromotionHostTenantId);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var startupBuilder = app.ConfigureDesignConversionModelServer(
            composition,
            new UnexpectedCandidateStrategy(),
            hostOptions,
            () => DateTimeOffset.UnixEpoch.AddMinutes(601),
            routeAdmissionOptions: routeAdmissionOptions);
        Assert.NotNull(startupBuilder);

        await app.StartAsync();
        try
        {
            AssertTrainingIntakeStartupMetadata(app, externalProviderConfigured: true);
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var create = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId = PromotionHostTenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:host-http-promotion-create-test",
                reason = "host startup configured HTTP promotion uploader create path; productReady remains false",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(602)
            });
            using var createResponse = create.Response;
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            AssertNoPrivateTrainingIntakePayload(create.BodyJson, privateMarker);
            using var createDocument = JsonDocument.Parse(create.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();
            Assert.False(createDocument.RootElement.GetProperty("productReady").GetBoolean());

            var promote = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId = PromotionHostTenantId,
                idempotencyKey = promoteIdempotencyKey,
                intakeId,
                promotionOutputDirectory,
                options = BuildTrainingIntakeActualRouteOptions(
                    runId,
                    "training-intake-host-http-promotion-dataset",
                    "training-intake-host-http-promotion-gate",
                    sourceDisjoint: true)
            });
            using var promoteResponse = promote.Response;
            Assert.Equal(HttpStatusCode.OK, promoteResponse.StatusCode);
            AssertNoPrivateTrainingIntakePayload(promote.BodyJson, privateMarker);
            Assert.DoesNotContain(PromotionHostBearerToken, promote.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promotionOutputDirectory, promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(seeded.RootDirectory, promote.BodyJson, StringComparison.OrdinalIgnoreCase);

            Assert.Equal(1, uploadHandler.SendCount);
            Assert.Equal("POST", uploadHandler.Method);
            Assert.Equal(PromotionUploadEndpoint.AbsolutePath, uploadHandler.RequestUri!.AbsolutePath);
            Assert.Equal($"Bearer {PromotionHostBearerToken}", Assert.Single(uploadHandler.HeaderValues("Authorization")));
            Assert.Equal(PromotionHostTenantId, Assert.Single(uploadHandler.HeaderValues(DesignConversionModelServerPromotionHttpUploader.TenantHeaderName)));
            Assert.Equal(
                DesignConversionModelServerPromotionHttpUploader.ContractVersion,
                Assert.Single(uploadHandler.HeaderValues(DesignConversionModelServerPromotionHttpUploader.ContractHeaderName)));
            Assert.Contains(
                "multipart/form-data",
                Assert.Single(uploadHandler.HeaderValues("Content-Type")),
                StringComparison.OrdinalIgnoreCase);
            Assert.Contains("name=metadata", uploadHandler.BodyText, StringComparison.Ordinal);
            Assert.Contains("name=checkpoint", uploadHandler.BodyText, StringComparison.Ordinal);
            Assert.Contains("name=policy", uploadHandler.BodyText, StringComparison.Ordinal);
            Assert.Contains("name=tokenizer", uploadHandler.BodyText, StringComparison.Ordinal);
            Assert.Contains($"\"tenantId\":\"{PromotionHostTenantId}\"", uploadHandler.BodyText, StringComparison.Ordinal);
            Assert.DoesNotContain(PromotionHostBearerToken, uploadHandler.BodyText, StringComparison.Ordinal);
            Assert.NotNull(uploadHandler.Metadata);
            Assert.Equal(DesignConversionModelServerPromotionHttpUploader.ContractVersion, uploadHandler.Metadata!.ContractVersion);
            Assert.Equal(PromotionHostTenantId, uploadHandler.Metadata.TenantId);
            Assert.Equal($"{runId}-local-promotion-manifest", uploadHandler.Metadata.ManifestId);
            Assert.Equal(DesignConversionMachine.DomainTag, uploadHandler.Metadata.DomainTag);
            Assert.Equal(100, uploadHandler.Metadata.AcceptedRecordCount);
            Assert.Equal(0, uploadHandler.Metadata.QuarantinedRecordCount);
            Assert.StartsWith("design-conversion-promotion-v1-", uploadHandler.Metadata.IdempotencyKey, StringComparison.Ordinal);
            Assert.Equal(3, uploadHandler.Metadata.Artifacts.Count);
            Assert.Equal(["checkpoint", "policy", "tokenizer"], uploadHandler.Metadata.Artifacts.Select(artifact => artifact.Role).Order(StringComparer.Ordinal).ToArray());
            Assert.All(uploadHandler.Metadata.Artifacts, artifact =>
            {
                Assert.True(artifact.Length > 0);
                Assert.Matches("^[a-f0-9]{64}$", artifact.Sha256);
            });

            using var promoteDocument = JsonDocument.Parse(promote.BodyJson);
            var promoteRoot = promoteDocument.RootElement;
            Assert.Equal(DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal, promoteRoot.GetProperty("action").GetString());
            Assert.False(promoteRoot.GetProperty("productReady").GetBoolean());
            var rootMetadata = promoteRoot.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_remote_promotion_configured").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("training_intake_route_local_only").GetString());
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_host_tenant_verified").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("product_ready").GetString());

            var localPromotion = promoteRoot.GetProperty("localPromotion");
            Assert.False(localPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.True(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
            Assert.Equal("https://promotion.example/receipts/route-remote-upload-1", localPromotion.GetProperty("promotionUploadRef").GetString());
            Assert.Equal(JsonValueKind.Null, localPromotion.GetProperty("localOnly").ValueKind);
            Assert.Equal(JsonValueKind.Null, localPromotion.GetProperty("promotedLocalModelId").ValueKind);
            var localPromotionEvidenceRefs = localPromotion.GetProperty("evidenceRefs")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray();
            Assert.Contains("MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT", localPromotionEvidenceRefs);
            Assert.Contains("MODEL_SERVER_PROMOTION_CLAIMED_VERIFIED_REMOTE_RECEIPT", localPromotionEvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_MODEL_SERVER_UPLOAD_VERIFIED_BY_CONFIGURED_UPLOADER", localPromotionEvidenceRefs);

            var localPromotionMetadata = localPromotion.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("external_cloud_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("external_cloud_upload_not_attempted").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("model_server_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("model_server_upload_verified").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("model_server_promotion_claimed").GetString());
            Assert.Equal("model_server_upload_verified", localPromotionMetadata.GetProperty("promotion_receipt_kind").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("model_server_upload_not_attempted_local_receipt").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("model_server_promotion_not_claimed_local_receipt").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(seeded.RootDirectory);
        }
    }

    [Theory]
    [InlineData(PromotionUploadResponseMode.NonSuccess, "DESIGN_CONVERSION_PROMOTION_UPLOAD_HTTP_NON_SUCCESS")]
    [InlineData(PromotionUploadResponseMode.MalformedReceipt, "DESIGN_CONVERSION_PROMOTION_UPLOAD_RESPONSE_REMOTE_REF_INVALID")]
    public async Task PostTrainingIntake_HostStartupConfiguredHttpPromotionUploaderFailure_FailsClosedWithoutLocalFallback(
        PromotionUploadResponseMode responseMode,
        string expectedErrorCode)
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_HOST_HTTP_PROMOTION_FAILURE_SECRET";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var promotionOutputDirectory = Path.Combine(seeded.RootDirectory, "remote-promotion-failure-output");
        var uploadHandler = new CapturingPromotionUploadHttpHandler(responseMode);
        using var promotionHttpClient = new HttpClient(uploadHandler, disposeHandler: true);
        var hostOptions = CreatePromotionHostOptions(seeded.RootDirectory, promotionHttpClient);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(
            composition,
            new UnexpectedCandidateStrategy(),
            hostOptions,
            () => DateTimeOffset.UnixEpoch.AddMinutes(611),
            routeAdmissionOptions: CreatePromotionRouteAdmissionOptions(PromotionHostTenantId));

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            var create = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId = PromotionHostTenantId,
                idempotencyKey = "idempotency:host-http-promotion-failure-create",
                handoff = seeded.Handoff,
                requestedByRef = "architect:host-http-promotion-failure-test",
                reason = "host startup HTTP promotion failure setup; productReady remains false"
            });
            using var createResponse = create.Response;
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            using var createDocument = JsonDocument.Parse(create.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();

            var promote = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId = PromotionHostTenantId,
                idempotencyKey = $"idempotency:host-http-promotion-failure-{responseMode}",
                intakeId,
                promotionOutputDirectory,
                options = BuildTrainingIntakeActualRouteOptions(
                    $"training-intake-host-http-promotion-failure-{responseMode}",
                    $"training-intake-host-http-promotion-failure-{responseMode}-dataset",
                    $"training-intake-host-http-promotion-failure-{responseMode}-gate",
                    sourceDisjoint: true)
            });
            using var promoteResponse = promote.Response;

            Assert.Equal(HttpStatusCode.BadRequest, promoteResponse.StatusCode);
            Assert.Equal(1, uploadHandler.SendCount);
            AssertNoPrivateTrainingIntakePayload(promote.BodyJson, privateMarker);
            Assert.DoesNotContain(PromotionHostBearerToken, promote.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain("local_receipt_only", promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("localPromotionReady\":true", promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            using var errorDocument = JsonDocument.Parse(promote.BodyJson);
            Assert.Equal(expectedErrorCode, errorDocument.RootElement.GetProperty("errorCode").GetString());
            var errorMetadata = errorDocument.RootElement.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, errorMetadata.GetProperty("training_intake_remote_promotion_configured").GetString());
            Assert.Equal(bool.FalseString, errorMetadata.GetProperty("training_intake_route_local_only").GetString());
            Assert.Equal(bool.FalseString, errorMetadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(seeded.RootDirectory);
        }
    }

    [Theory]
    [InlineData("missing-client", "DESIGN_CONVERSION_PROMOTION_HOST_REMOTE_CONFIG_ATOMIC_REQUIRED")]
    [InlineData("missing-options", "DESIGN_CONVERSION_PROMOTION_HOST_REMOTE_CONFIG_ATOMIC_REQUIRED")]
    [InlineData("root-mismatch", "DESIGN_CONVERSION_PROMOTION_HOST_ARTIFACT_ROOT_MISMATCH")]
    [InlineData("tenant-mismatch", "DESIGN_CONVERSION_PROMOTION_HOST_ROUTE_ADMISSION_TENANT_MISMATCH")]
    public void CreateLocalRouteRegistry_HostStartupPromotionConfig_FailsClosedBeforeRouteServingOrSend(
        string scenario,
        string expectedCode)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "host-promotion-config-fail");
        var uploadHandler = new CapturingPromotionUploadHttpHandler(PromotionUploadResponseMode.Success);
        using var promotionHttpClient = new HttpClient(uploadHandler, disposeHandler: true);
        var uploaderOptions = CreatePromotionHttpUploaderOptions(
            scenario == "root-mismatch"
                ? Path.Combine(root, "other-root")
                : root,
            PromotionHostTenantId);
        var hostOptions = scenario switch
        {
            "missing-client" => new DesignConversionModelServerPromotionHostOptions(root, null, uploaderOptions),
            "missing-options" => new DesignConversionModelServerPromotionHostOptions(root, promotionHttpClient, null),
            _ => new DesignConversionModelServerPromotionHostOptions(root, promotionHttpClient, uploaderOptions)
        };
        var routeAdmissionOptions = scenario == "tenant-mismatch"
            ? CreatePromotionRouteAdmissionOptions("tenant-beta")
            : CreatePromotionRouteAdmissionOptions(PromotionHostTenantId);

        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                    DesignConversionLocalServiceComposition.CreateDeterministicLocal(),
                    new UnexpectedCandidateStrategy(),
                    hostOptions,
                    () => DateTimeOffset.UnixEpoch.AddMinutes(621),
                    routeAdmissionOptions: routeAdmissionOptions));

            Assert.Contains(expectedCode, exception.Message, StringComparison.Ordinal);
            Assert.DoesNotContain(PromotionHostBearerToken, exception.Message, StringComparison.Ordinal);
            Assert.Equal(0, uploadHandler.SendCount);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_HostStartupPromotionHostOptionsWithoutRemoteConfig_PreservesLocalOnlyPromotionMarkers()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_HOST_LOCAL_ONLY_PROMOTION_SECRET";
        const string tenantId = "tenant:modelserver-training-intake-host-local-only";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "host-local-only-promotion-output");
        var hostOptions = new DesignConversionModelServerPromotionHostOptions(seeded.RootDirectory);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(
            composition,
            new UnexpectedCandidateStrategy(),
            hostOptions,
            () => DateTimeOffset.UnixEpoch.AddMinutes(631));

        await app.StartAsync();
        try
        {
            AssertTrainingIntakeStartupMetadata(app, externalProviderConfigured: false);
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            var create = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = "idempotency:host-local-only-promotion-create",
                handoff = seeded.Handoff,
                requestedByRef = "architect:host-local-only-promotion-test",
                reason = "host startup promotion options without remote config preserves local-only behavior"
            });
            using var createResponse = create.Response;
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            using var createDocument = JsonDocument.Parse(create.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();

            var promote = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = "idempotency:host-local-only-promotion-promote",
                intakeId,
                promotionOutputDirectory,
                options = BuildTrainingIntakeActualRouteOptions(
                    "training-intake-host-local-only-promotion",
                    "training-intake-host-local-only-promotion-dataset",
                    "training-intake-host-local-only-promotion-gate",
                    sourceDisjoint: true)
            });
            using var promoteResponse = promote.Response;
            Assert.Equal(HttpStatusCode.OK, promoteResponse.StatusCode);
            AssertNoPrivateTrainingIntakePayload(promote.BodyJson, privateMarker);
            using var promoteDocument = JsonDocument.Parse(promote.BodyJson);
            var promoteRoot = promoteDocument.RootElement;
            var rootMetadata = promoteRoot.GetProperty("boundaryMetadata");
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("training_intake_remote_promotion_configured").GetString());
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_route_local_only").GetString());

            var localPromotion = promoteRoot.GetProperty("localPromotion");
            Assert.True(localPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.False(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
            Assert.True(localPromotion.GetProperty("localOnly").GetBoolean());
            Assert.StartsWith(
                DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix,
                localPromotion.GetProperty("promotionUploadRef").GetString(),
                StringComparison.Ordinal);
            var localPromotionMetadata = localPromotion.GetProperty("boundaryMetadata");
            Assert.Equal("local_receipt_only", localPromotionMetadata.GetProperty("promotion_receipt_kind").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("model_server_upload_not_attempted_local_receipt").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("model_server_promotion_not_claimed_local_receipt").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("model_server_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(seeded.RootDirectory);
            DeleteDirectoryIfExists(promotionOutputDirectory);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_AspNetMapper_PartialSourceManifestProofDoesNotPromoteLocalOverKestrelLoopback()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ASPNET_PARTIAL_MANIFEST_NEGATIVE_SECRET";
        const string tenantId = "tenant:modelserver-training-intake-aspnet-partial-manifest-negative";
        const string createIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-partial-manifest-negative-create";
        const string promoteIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-partial-manifest-negative-promote";
        var seeded = await CreatePartialSourceManifestApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        var forgedReadyOptions = BuildTrainingIntakeActualRouteOptions(
            "training-intake-aspnet-partial-manifest-negative",
            "training-intake-aspnet-partial-manifest-negative-dataset",
            "training-intake-aspnet-partial-manifest-negative-gate",
            sourceDisjoint: true);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-aspnet-partial-manifest-negative-promotion-output");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var routeBuilder = app.MapDesignConversionModelServerTrainingIntake(registry);
        Assert.NotNull(routeBuilder);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            async Task<(HttpResponseMessage Response, string BodyJson)> PostTrainingIntakeAsync(object body)
            {
                using var content = new StringContent(
                    JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    content);
                var bodyJson = await response.Content.ReadAsStringAsync();
                return (response, bodyJson);
            }

            void AssertLocalOnlyAspNetHeaders(HttpResponseMessage response)
            {
                Assert.Equal(
                    bool.TrueString,
                    response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.Equal(
                    bool.FalseString,
                    response.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
                Assert.Equal(
                    bool.FalseString,
                    response.Headers.GetValues("x-design-conversion-product-ready").Single());
            }

            var created = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-aspnet-partial-manifest-negative-test",
                reason = "aspnet mapper create before partial-manifest promote-local rejection; local only and not ProductReady",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(209)
            });
            using var createdResponse = created.Response;

            Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
            Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
            Assert.True(baseAddress.Port > 0);
            AssertLocalOnlyAspNetHeaders(createdResponse);
            AssertNoPrivateTrainingIntakePayload(created.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, created.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(createIdempotencyKey, created.BodyJson, StringComparison.Ordinal);

            using var createDocument = JsonDocument.Parse(created.BodyJson);
            var createRoot = createDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(createRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
            var intakeId = createRoot.GetProperty("intakeId").GetString();
            AssertSafeTrainingIntakeScalar(intakeId, privateMarker);
            var createMetadata = createRoot.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), createMetadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(bool.TrueString, createMetadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, createMetadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(bool.FalseString, createMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, createMetadata.GetProperty("product_ready").GetString());

            var promoted = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = promoteIdempotencyKey,
                intakeId,
                promotionOutputDirectory,
                options = forgedReadyOptions
            });
            using var promotedResponse = promoted.Response;

            Assert.Equal(HttpStatusCode.OK, promotedResponse.StatusCode);
            AssertLocalOnlyAspNetHeaders(promotedResponse);
            AssertNoPrivateTrainingIntakePayload(promoted.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, promoted.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promoteIdempotencyKey, promoted.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promotionOutputDirectory, promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFileName(promotionOutputDirectory), promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                promoted.BodyJson,
                StringComparison.Ordinal);
            Assert.DoesNotContain("\"productReady\":true", promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"deploymentVerified\":true", promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"localPromotionReady\":true", promoted.BodyJson, StringComparison.OrdinalIgnoreCase);

            using var promotedDocument = JsonDocument.Parse(promoted.BodyJson);
            var promotedRoot = promotedDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(promotedRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);
            Assert.Equal(intakeId, promotedRoot.GetProperty("intakeId").GetString());
            Assert.False(promotedRoot.GetProperty("productReady").GetBoolean());
            Assert.False(promotedRoot.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(promotedRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());
            var promotedRootMetadata = promotedRoot.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), promotedRootMetadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(bool.TrueString, promotedRootMetadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, promotedRootMetadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("public_http_endpoint_verified").GetString());

            var localPromotion = promotedRoot.GetProperty("localPromotion");
            Assert.False(localPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.False(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
            Assert.StartsWith(
                "local-promotion-output-directory:sha256:",
                localPromotion.GetProperty("promotionOutputDirectoryRef").GetString(),
                StringComparison.Ordinal);
            var promotedCheckpointRef = localPromotion.GetProperty("promotedCheckpointRef").GetString();
            Assert.True(
                string.IsNullOrEmpty(promotedCheckpointRef) ||
                promotedCheckpointRef.StartsWith("local-promoted-checkpoint-path:sha256:", StringComparison.Ordinal),
                promotedCheckpointRef);
            Assert.Contains(localPromotion.GetProperty("blockReasons").EnumerateArray(), item =>
                string.Equals(item.GetString(), "local_promotion_blocked_training_intake_preflight", StringComparison.Ordinal));
            Assert.Contains(localPromotion.GetProperty("blockReasons").EnumerateArray(), item =>
                item.GetString()?.Contains("source_manifest", StringComparison.Ordinal) == true);
            Assert.DoesNotContain(localPromotion.GetProperty("evidenceRefs").EnumerateArray(), item =>
                string.Equals(item.GetString(), DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence, StringComparison.Ordinal));

            var negativeRun = localPromotion.GetProperty("trainingRun");
            Assert.Equal(intakeId, negativeRun.GetProperty("trainingIntakeId").GetString());
            Assert.False(negativeRun.GetProperty("trainingRunnerInvoked").GetBoolean());
            Assert.False(negativeRun.GetProperty("trainingStarted").GetBoolean());
            Assert.False(negativeRun.GetProperty("checkpointMutated").GetBoolean());
            Assert.False(negativeRun.GetProperty("weightsProduced").GetBoolean());
            Assert.False(negativeRun.GetProperty("modelUploadReady").GetBoolean());
            Assert.Equal("missing", negativeRun.GetProperty("sourceManifestStatus").GetString());
            Assert.Equal("missing", negativeRun.GetProperty("sourceDisjointStatus").GetString());
            Assert.Contains(negativeRun.GetProperty("blockReasons").EnumerateArray(), item =>
                string.Equals(item.GetString(), "source_manifest_missing", StringComparison.Ordinal));
            Assert.Contains(negativeRun.GetProperty("blockReasons").EnumerateArray(), item =>
                string.Equals(item.GetString(), "source_disjoint_missing", StringComparison.Ordinal));

            var localPromotionMetadata = localPromotion.GetProperty("boundaryMetadata");
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("local_promotion_ready").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("local_promotion_not_ready").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("external_cloud_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("external_cloud_upload_not_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("cloud_model_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("model_upload_ready").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("product_ready").GetString());
            Assert.Equal("missing", localPromotionMetadata.GetProperty("source_manifest_status").GetString());
            Assert.Equal("missing", localPromotionMetadata.GetProperty("source_disjoint_status").GetString());
        }
        finally
        {
            await app.StopAsync();
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_AspNetMapper_FullyMissingSourceManifestProofDoesNotPromoteLocalOverKestrelLoopback()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ASPNET_FULLY_MISSING_MANIFEST_NEGATIVE_SECRET";
        const string tenantId = "tenant:modelserver-training-intake-aspnet-fully-missing-manifest-negative";
        const string createIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-fully-missing-manifest-negative-create";
        const string promoteIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-fully-missing-manifest-negative-promote";
        var seeded = await CreateFullyMissingSourceManifestApprovedHandoffAsync(privateMarker);
        Assert.All(seeded.Handoff.Rows, row => Assert.True(string.IsNullOrWhiteSpace(row.SourceManifestRef)));
        var registry = CreateRegistry(seeded.Db);
        var forgedReadyOptions = BuildTrainingIntakeActualRouteOptions(
            "training-intake-aspnet-fully-missing-manifest-negative",
            "training-intake-aspnet-fully-missing-manifest-negative-dataset",
            "training-intake-aspnet-fully-missing-manifest-negative-gate",
            sourceDisjoint: true);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-aspnet-fully-missing-manifest-negative-promotion-output");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var routeBuilder = app.MapDesignConversionModelServerTrainingIntake(registry);
        Assert.NotNull(routeBuilder);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            async Task<(HttpResponseMessage Response, string BodyJson)> PostTrainingIntakeAsync(object body)
            {
                using var content = new StringContent(
                    JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    content);
                var bodyJson = await response.Content.ReadAsStringAsync();
                return (response, bodyJson);
            }

            void AssertLocalOnlyAspNetHeaders(HttpResponseMessage response)
            {
                Assert.Equal(
                    bool.TrueString,
                    response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.Equal(
                    bool.FalseString,
                    response.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
                Assert.Equal(
                    bool.FalseString,
                    response.Headers.GetValues("x-design-conversion-product-ready").Single());
            }

            var created = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-aspnet-fully-missing-manifest-negative-test",
                reason = "aspnet mapper create before fully-missing-manifest promote-local rejection; local only and not ProductReady",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(210)
            });
            using var createdResponse = created.Response;

            Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
            Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
            Assert.True(baseAddress.Port > 0);
            AssertLocalOnlyAspNetHeaders(createdResponse);
            AssertNoPrivateTrainingIntakePayload(created.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, created.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(createIdempotencyKey, created.BodyJson, StringComparison.Ordinal);

            using var createDocument = JsonDocument.Parse(created.BodyJson);
            var createRoot = createDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(createRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
            var intakeId = createRoot.GetProperty("intakeId").GetString();
            AssertSafeTrainingIntakeScalar(intakeId, privateMarker);
            var createMetadata = createRoot.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), createMetadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(bool.TrueString, createMetadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, createMetadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(bool.FalseString, createMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, createMetadata.GetProperty("product_ready").GetString());

            var promoted = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = promoteIdempotencyKey,
                intakeId,
                promotionOutputDirectory,
                options = forgedReadyOptions
            });
            using var promotedResponse = promoted.Response;

            Assert.Equal(HttpStatusCode.OK, promotedResponse.StatusCode);
            AssertLocalOnlyAspNetHeaders(promotedResponse);
            AssertNoPrivateTrainingIntakePayload(promoted.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, promoted.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promoteIdempotencyKey, promoted.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promotionOutputDirectory, promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFileName(promotionOutputDirectory), promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                promoted.BodyJson,
                StringComparison.Ordinal);
            Assert.DoesNotContain("\"productReady\":true", promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"deploymentVerified\":true", promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"localPromotionReady\":true", promoted.BodyJson, StringComparison.OrdinalIgnoreCase);

            using var promotedDocument = JsonDocument.Parse(promoted.BodyJson);
            var promotedRoot = promotedDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(promotedRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);
            Assert.Equal(intakeId, promotedRoot.GetProperty("intakeId").GetString());
            Assert.False(promotedRoot.GetProperty("productReady").GetBoolean());
            Assert.False(promotedRoot.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(promotedRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());
            var promotedRootMetadata = promotedRoot.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), promotedRootMetadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(bool.TrueString, promotedRootMetadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, promotedRootMetadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("public_http_endpoint_verified").GetString());

            var localPromotion = promotedRoot.GetProperty("localPromotion");
            Assert.False(localPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.False(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
            Assert.StartsWith(
                "local-promotion-output-directory:sha256:",
                localPromotion.GetProperty("promotionOutputDirectoryRef").GetString(),
                StringComparison.Ordinal);
            var promotedCheckpointRef = localPromotion.GetProperty("promotedCheckpointRef").GetString();
            Assert.True(
                string.IsNullOrEmpty(promotedCheckpointRef) ||
                promotedCheckpointRef.StartsWith("local-promoted-checkpoint-path:sha256:", StringComparison.Ordinal),
                promotedCheckpointRef);
            Assert.Contains(localPromotion.GetProperty("blockReasons").EnumerateArray(), item =>
                string.Equals(item.GetString(), "local_promotion_blocked_training_intake_preflight", StringComparison.Ordinal));
            Assert.Contains(localPromotion.GetProperty("blockReasons").EnumerateArray(), item =>
                item.GetString()?.Contains("source_manifest", StringComparison.Ordinal) == true);
            Assert.DoesNotContain(localPromotion.GetProperty("evidenceRefs").EnumerateArray(), item =>
                string.Equals(item.GetString(), DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence, StringComparison.Ordinal));

            var negativeRun = localPromotion.GetProperty("trainingRun");
            Assert.Equal(intakeId, negativeRun.GetProperty("trainingIntakeId").GetString());
            Assert.False(negativeRun.GetProperty("trainingRunnerInvoked").GetBoolean());
            Assert.False(negativeRun.GetProperty("trainingStarted").GetBoolean());
            Assert.False(negativeRun.GetProperty("checkpointMutated").GetBoolean());
            Assert.False(negativeRun.GetProperty("weightsProduced").GetBoolean());
            Assert.False(negativeRun.GetProperty("modelUploadReady").GetBoolean());
            Assert.Equal("missing", negativeRun.GetProperty("sourceManifestStatus").GetString());
            Assert.Equal("missing", negativeRun.GetProperty("sourceDisjointStatus").GetString());
            Assert.Contains(negativeRun.GetProperty("blockReasons").EnumerateArray(), item =>
                string.Equals(item.GetString(), "source_manifest_missing", StringComparison.Ordinal));
            Assert.Contains(negativeRun.GetProperty("blockReasons").EnumerateArray(), item =>
                string.Equals(item.GetString(), "source_disjoint_missing", StringComparison.Ordinal));

            var localPromotionMetadata = localPromotion.GetProperty("boundaryMetadata");
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("local_promotion_ready").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("local_promotion_not_ready").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("external_cloud_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("external_cloud_upload_not_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("cloud_model_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("model_upload_ready").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("product_ready").GetString());
            Assert.Equal("missing", localPromotionMetadata.GetProperty("source_manifest_status").GetString());
            Assert.Equal("missing", localPromotionMetadata.GetProperty("source_disjoint_status").GetString());
        }
        finally
        {
            await app.StopAsync();
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_AspNetMapper_StatusOnlySourceDisjointProofDoesNotPromoteLocalOverKestrelLoopback()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ASPNET_STATUS_ONLY_NEGATIVE_SECRET";
        const string tenantId = "tenant:modelserver-training-intake-aspnet-status-only-negative";
        const string createIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-status-only-negative-create";
        const string promoteIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-status-only-negative-promote";
        var seeded = await CreateSingleSourcePersistedApprovedHandoffAsync(privateMarker);
        var registry = CreateRegistry(seeded.Db);
        var statusOnlyOptions = BuildTrainingIntakeActualRouteOptions(
            "training-intake-aspnet-status-only-negative",
            "training-intake-aspnet-status-only-negative-dataset",
            "training-intake-aspnet-status-only-negative-gate",
            sourceDisjoint: true);
        var promotionOutputDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-aspnet-status-only-negative-promotion-output");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var routeBuilder = app.MapDesignConversionModelServerTrainingIntake(registry);
        Assert.NotNull(routeBuilder);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            async Task<(HttpResponseMessage Response, string BodyJson)> PostTrainingIntakeAsync(object body)
            {
                using var content = new StringContent(
                    JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    content);
                var bodyJson = await response.Content.ReadAsStringAsync();
                return (response, bodyJson);
            }

            void AssertLocalOnlyAspNetHeaders(HttpResponseMessage response)
            {
                Assert.Equal(
                    bool.TrueString,
                    response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.Equal(
                    bool.FalseString,
                    response.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
                Assert.Equal(
                    bool.FalseString,
                    response.Headers.GetValues("x-design-conversion-product-ready").Single());
            }

            var created = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-aspnet-status-only-negative-test",
                reason = "aspnet mapper create before status-only promote-local rejection; local only and not ProductReady",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(208)
            });
            using var createdResponse = created.Response;

            Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
            Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
            Assert.True(baseAddress.Port > 0);
            AssertLocalOnlyAspNetHeaders(createdResponse);
            AssertNoPrivateTrainingIntakePayload(created.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, created.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(createIdempotencyKey, created.BodyJson, StringComparison.Ordinal);

            using var createDocument = JsonDocument.Parse(created.BodyJson);
            var createRoot = createDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(createRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
            var intakeId = createRoot.GetProperty("intakeId").GetString();
            AssertSafeTrainingIntakeScalar(intakeId, privateMarker);
            var createMetadata = createRoot.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), createMetadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(bool.TrueString, createMetadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, createMetadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(bool.FalseString, createMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, createMetadata.GetProperty("product_ready").GetString());

            var promoted = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = promoteIdempotencyKey,
                intakeId,
                promotionOutputDirectory,
                options = statusOnlyOptions
            });
            using var promotedResponse = promoted.Response;

            Assert.Equal(HttpStatusCode.OK, promotedResponse.StatusCode);
            AssertLocalOnlyAspNetHeaders(promotedResponse);
            AssertNoPrivateTrainingIntakePayload(promoted.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, promoted.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promoteIdempotencyKey, promoted.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promotionOutputDirectory, promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFileName(promotionOutputDirectory), promoted.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                promoted.BodyJson,
                StringComparison.Ordinal);

            using var promotedDocument = JsonDocument.Parse(promoted.BodyJson);
            var promotedRoot = promotedDocument.RootElement;
            AssertSafeTrainingIntakeRouteRoot(promotedRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal);
            Assert.Equal(intakeId, promotedRoot.GetProperty("intakeId").GetString());
            Assert.False(promotedRoot.GetProperty("productReady").GetBoolean());
            Assert.False(promotedRoot.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(promotedRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());
            var promotedRootMetadata = promotedRoot.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), promotedRootMetadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(bool.TrueString, promotedRootMetadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, promotedRootMetadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.FalseString, promotedRootMetadata.GetProperty("public_http_endpoint_verified").GetString());

            var localPromotion = promotedRoot.GetProperty("localPromotion");
            Assert.False(localPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.False(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
            Assert.StartsWith(
                "local-promotion-output-directory:sha256:",
                localPromotion.GetProperty("promotionOutputDirectoryRef").GetString(),
                StringComparison.Ordinal);
            var promotedCheckpointRef = localPromotion.GetProperty("promotedCheckpointRef").GetString();
            Assert.True(
                string.IsNullOrEmpty(promotedCheckpointRef) ||
                promotedCheckpointRef.StartsWith("local-promoted-checkpoint-path:sha256:", StringComparison.Ordinal),
                promotedCheckpointRef);
            Assert.Contains(localPromotion.GetProperty("blockReasons").EnumerateArray(), item =>
                string.Equals(item.GetString(), "local_promotion_blocked_training_intake_preflight", StringComparison.Ordinal) ||
                item.GetString()?.Contains("source_disjoint", StringComparison.Ordinal) == true);
            Assert.DoesNotContain(localPromotion.GetProperty("evidenceRefs").EnumerateArray(), item =>
                string.Equals(item.GetString(), DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence, StringComparison.Ordinal));

            var negativeRun = localPromotion.GetProperty("trainingRun");
            Assert.Equal(intakeId, negativeRun.GetProperty("trainingIntakeId").GetString());
            Assert.False(negativeRun.GetProperty("trainingRunnerInvoked").GetBoolean());
            Assert.False(negativeRun.GetProperty("trainingStarted").GetBoolean());
            Assert.False(negativeRun.GetProperty("checkpointMutated").GetBoolean());
            Assert.False(negativeRun.GetProperty("weightsProduced").GetBoolean());
            Assert.False(negativeRun.GetProperty("modelUploadReady").GetBoolean());
            Assert.Equal("proven", negativeRun.GetProperty("sourceManifestStatus").GetString());
            Assert.Equal("blocked_same_source", negativeRun.GetProperty("sourceDisjointStatus").GetString());
            Assert.Contains(negativeRun.GetProperty("blockReasons").EnumerateArray(), item =>
                item.GetString()?.Contains("source_disjoint", StringComparison.Ordinal) == true);

            var localPromotionMetadata = localPromotion.GetProperty("boundaryMetadata");
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("local_promotion_ready").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("local_promotion_not_ready").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("external_cloud_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("external_cloud_upload_not_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("cloud_model_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("model_upload_ready").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            await app.StopAsync();
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

    [Fact]
    public void DescribeTrainingIntakeRoute_DocumentsLocalOnlyRefsOnlyContract()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeTrainingIntakeRoute();

        Assert.Equal(DesignConversionModelServerCallerContract.TrainingIntakeRouteSchemaVersion, descriptor.SchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.TrainingIntakeRouteBoundary, descriptor.Boundary);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod, descriptor.EndpointMethod);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath, descriptor.EndpointPath);
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "action", Required: true, MayCarryPrivateInput: false });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "intakeId", Required: false, ResponseMayEchoRawValue: true });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "options", Required: false, MayCarryPrivateInput: true });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "promotionOutputDirectory", Required: false, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "requestedByRef", Required: false, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "reason", Required: false, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionList, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionPreflight, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionRun, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus, descriptor.SupportedActions);
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "intakes", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "preflight", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "trainingRun", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "localPromotion", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "lifecycleStatus", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "lifecycleStatus.promotedCheckpointPathRef", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "lifecycleStatus.promotedRuntimeModelId", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.training_intake_route_local_only", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.private_file_paths_included", Required: true });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_ID_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_LIFECYCLE_REASON_REQUIRED", StatusCode: 400 });
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_route_discoverable_by_contract"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_response_refs_only"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_paths_returned_as_hash_refs"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_continue_training_upload_status_supported"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_continue_training_upload_status_local_only"]);
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), descriptor.BoundaryMetadata["delegates_to_domain_api"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["socket_opened"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["deployment_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["product_ready"]);
    }

    private static void AssertSanitizedTrainingIntakeBody(
        DesignConversionModelServerHttpResponse response,
        string privateMarker)
    {
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        Assert.Equal(DesignConversionModelServerTrainingIntakeHandler.LocalTrainingIntakeRouteBoundary, response.Headers["x-design-conversion-training-intake-route"]);
        Assert.Equal(bool.FalseString, response.Headers["x-design-conversion-public-deployment-verified"]);
        AssertNoPrivateTrainingIntakePayload(response.BodyJson, privateMarker);
    }

    private static void AssertTrainingIntakeRouteError(
        DesignConversionModelServerHttpResponse response,
        string expectedErrorCode,
        string privateMarker,
        string? expectedAction = null,
        bool? expectedAdmissionSatisfied = null,
        int expectedStatusCode = 400)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        AssertNoPrivateTrainingIntakePayload(response.BodyJson, privateMarker);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(expectedStatusCode, root.GetProperty("statusCode").GetInt32());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        if (!string.IsNullOrWhiteSpace(expectedAction))
            Assert.Equal(expectedAction, metadata.GetProperty("training_intake_action").GetString());
        if (expectedAdmissionSatisfied.HasValue)
            Assert.Equal(expectedAdmissionSatisfied.Value.ToString(), metadata.GetProperty("training_intake_admission_boundary_satisfied").GetString());
    }

    private static void AssertAuthAdmissionRejectedWithoutRawSecrets(
        DesignConversionModelServerHttpResponse response,
        string expectedToken,
        string invalidToken,
        string tenantId,
        string idempotencyKey)
    {
        Assert.DoesNotContain(expectedToken, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(invalidToken, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantId, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(idempotencyKey, response.BodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(response.BodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_auth_token_configured").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_auth_token_required").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_auth_token_enforced").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("training_intake_auth_token_verified").GetString());
        Assert.Equal(
            DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName,
            metadata.GetProperty("training_intake_auth_token_header_name").GetString());
    }

    private static async Task AssertMutatingActionRequiresTenantAndIdempotencyAsync(
        DesignConversionModelServerEndpointRegistry registry,
        IDesignConversionLocalArtifactDb db,
        string privateMarker,
        string action,
        IReadOnlyDictionary<string, object?> actionFields,
        int expectedIntakeCount)
    {
        var actionToken = action.Replace("-", "_", StringComparison.Ordinal);
        var rawTenantId = $"tenant:modelserver-training-intake-admission-{actionToken}";
        var rawIdempotencyKey = $"idempotency:modelserver-training-intake-admission-{actionToken}";

        await AssertTrainingIntakeAdmissionErrorAsync(
            registry,
            db,
            BuildTrainingIntakeBody(
                action,
                actionFields,
                ("idempotencyKey", rawIdempotencyKey)),
            "TRAINING_INTAKE_TENANT_ID_REQUIRED",
            privateMarker,
            action,
            expectedIntakeCount,
            rawIdempotencyKey);
        await AssertTrainingIntakeAdmissionErrorAsync(
            registry,
            db,
            BuildTrainingIntakeBody(
                action,
                actionFields,
                ("tenantId", "   "),
                ("idempotencyKey", rawIdempotencyKey)),
            "TRAINING_INTAKE_TENANT_ID_REQUIRED",
            privateMarker,
            action,
            expectedIntakeCount,
            rawIdempotencyKey);
        await AssertTrainingIntakeAdmissionErrorAsync(
            registry,
            db,
            BuildTrainingIntakeBody(
                action,
                actionFields,
                ("tenantId", rawTenantId)),
            "TRAINING_INTAKE_IDEMPOTENCY_KEY_REQUIRED",
            privateMarker,
            action,
            expectedIntakeCount,
            rawTenantId);
        await AssertTrainingIntakeAdmissionErrorAsync(
            registry,
            db,
            BuildTrainingIntakeBody(
                action,
                actionFields,
                ("tenantId", rawTenantId),
                ("idempotencyKey", "   ")),
            "TRAINING_INTAKE_IDEMPOTENCY_KEY_REQUIRED",
            privateMarker,
            action,
            expectedIntakeCount,
            rawTenantId);
    }

    private static async Task AssertTrainingIntakeAdmissionErrorAsync(
        DesignConversionModelServerEndpointRegistry registry,
        IDesignConversionLocalArtifactDb db,
        IReadOnlyDictionary<string, object?> body,
        string expectedErrorCode,
        string privateMarker,
        string expectedAction,
        int expectedIntakeCount,
        params string[] forbiddenRawValues)
    {
        var response = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        AssertTrainingIntakeRouteError(
            response.Value!,
            expectedErrorCode,
            privateMarker,
            expectedAction,
            expectedAdmissionSatisfied: false);
        foreach (var forbiddenRawValue in forbiddenRawValues)
        {
            if (!string.IsNullOrWhiteSpace(forbiddenRawValue))
                Assert.DoesNotContain(forbiddenRawValue, response.Value!.BodyJson, StringComparison.Ordinal);
        }

        await AssertTrainingIntakeCountAsync(db, expectedIntakeCount);
    }

    private static IReadOnlyDictionary<string, object?> BuildTrainingIntakeBody(
        string action,
        IReadOnlyDictionary<string, object?> actionFields,
        params (string Key, object? Value)[] boundaryFields)
    {
        var body = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["action"] = action
        };
        foreach (var field in actionFields)
            body[field.Key] = field.Value;
        foreach (var field in boundaryFields)
            body[field.Key] = field.Value;
        return body;
    }

    private static async Task AssertTrainingIntakeCountAsync(
        IDesignConversionLocalArtifactDb db,
        int expectedCount)
    {
        var api = new DesignConversionDpoUsefulnessApi(db);
        var listed = await api.ListTrainingIntakeRequestsAsync();
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Equal(expectedCount, listed.Value!.Count);
    }

    private static void AssertTrainingIntakeCallerEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        string privateMarker)
    {
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion, envelope.EnvelopeSchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary, envelope.EnvelopeBoundary);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionModelServerCallerContract.TrainingIntakeRouteSchemaVersion, envelope.ContractSchemaVersion);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod, envelope.EndpointMethod);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath, envelope.EndpointPath);
        Assert.Equal(200, envelope.StatusCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.True(envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.Equal("training-intake-action-completed", envelope.ParsedContractStatus);
        Assert.Null(envelope.ParsedErrorCode);
        AssertNoPrivateTrainingIntakePayload(envelope.ResponseBodyJson, privateMarker);
        Assert.False(envelope.RawHtmlIncluded);
        Assert.False(envelope.RawCssIncluded);
        Assert.False(envelope.RawDesignTreeIncluded);
        Assert.False(envelope.ResponseContainsRawPrivateData);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["training_intake_response_refs_only"]);
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), envelope.BoundaryMetadata["delegates_to_domain_api"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["deployment_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
    }

    private static void AssertSafeTrainingIntakeRouteRoot(JsonElement root, string expectedAction)
    {
        Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerTrainingIntakeHandler.ApiShape, root.GetProperty("apiShape").GetString());
        Assert.Equal("training-intake-action-completed", root.GetProperty("contractStatus").GetString());
        Assert.Equal(expectedAction, root.GetProperty("action").GetString());
        Assert.False(root.GetProperty("productReady").GetBoolean());
        Assert.False(root.GetProperty("deploymentVerified").GetBoolean());
        Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(expectedAction, metadata.GetProperty("training_intake_action").GetString());
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), metadata.GetProperty("delegates_to_domain_api").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_route_local_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_response_refs_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_admission_boundary_satisfied").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    private static void AssertTrainingDatabaseDurabilityMetadata(
        JsonElement metadata,
        int expectedRecordCount,
        string privateMarker)
    {
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, metadata.GetProperty("training_database_kind").GetString());
        Assert.Equal("design_conversion_special_training_artifact_db", metadata.GetProperty("training_database_name").GetString());
        Assert.Equal("design-conversion-local-artifact-db-v1", metadata.GetProperty("training_database_version").GetString());
        Assert.StartsWith(
            "design-conversion-",
            metadata.GetProperty("training_database_schema_version").GetString(),
            StringComparison.Ordinal);
        Assert.StartsWith(
            "manifest://design-conversion-local-artifact-db/",
            metadata.GetProperty("training_database_manifest_ref").GetString(),
            StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata.GetProperty("training_database_checksum_sha256").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata.GetProperty("training_database_manifest_checksum_sha256").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata.GetProperty("training_database_source_refs_sha256").GetString(), StringComparison.Ordinal);
        Assert.Equal(expectedRecordCount.ToString(CultureInfo.InvariantCulture), metadata.GetProperty("training_database_record_count").GetString());
        Assert.Equal("source_refs_hashed_or_manifest_refs_only", metadata.GetProperty("training_database_source_refs_policy").GetString());
        Assert.Equal("local_refs_only_training_trace", metadata.GetProperty("training_database_retention_class").GetString());
        Assert.Equal("retain_manifest_checksums_and_refs_only_no_raw_payloads", metadata.GetProperty("training_database_retention_policy").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("training_database_refs_only_trace").GetString());
        Assert.Equal(
            "raw_html_css_design_tree_provider_credentials_not_persisted",
            metadata.GetProperty("training_database_raw_payload_redaction_marker").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("raw_payload_redacted").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        AssertFalseIfPresent(metadata, "raw_user_private_data_included");
        AssertFalseIfPresent(metadata, "provider_payload_included");

        AssertNoPrivateTrainingIntakePayload(metadata.GetRawText(), privateMarker);
    }

    private static void AssertFalseIfPresent(JsonElement metadata, string propertyName)
    {
        if (metadata.TryGetProperty(propertyName, out var value))
            Assert.Equal(bool.FalseString, value.GetString());
    }

    private static void AssertTrainingDatabaseDurabilityMetadata(
        IReadOnlyDictionary<string, string> metadata,
        int expectedRecordCount,
        string privateMarker)
    {
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, metadata["training_database_kind"]);
        Assert.Equal("design_conversion_special_training_artifact_db", metadata["training_database_name"]);
        Assert.Equal("design-conversion-local-artifact-db-v1", metadata["training_database_version"]);
        Assert.StartsWith("design-conversion-", metadata["training_database_schema_version"], StringComparison.Ordinal);
        Assert.StartsWith(
            "manifest://design-conversion-local-artifact-db/",
            metadata["training_database_manifest_ref"],
            StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata["training_database_checksum_sha256"], StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata["training_database_manifest_checksum_sha256"], StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata["training_database_source_refs_sha256"], StringComparison.Ordinal);
        Assert.Equal(expectedRecordCount.ToString(CultureInfo.InvariantCulture), metadata["training_database_record_count"]);
        Assert.Equal("source_refs_hashed_or_manifest_refs_only", metadata["training_database_source_refs_policy"]);
        Assert.Equal("local_refs_only_training_trace", metadata["training_database_retention_class"]);
        Assert.Equal("retain_manifest_checksums_and_refs_only_no_raw_payloads", metadata["training_database_retention_policy"]);
        Assert.Equal(bool.TrueString, metadata["training_database_refs_only_trace"]);
        Assert.Equal("raw_html_css_design_tree_provider_credentials_not_persisted", metadata["training_database_raw_payload_redaction_marker"]);
        Assert.Equal(bool.TrueString, metadata["raw_payload_redacted"]);
        Assert.Equal(bool.FalseString, metadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, metadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, metadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, metadata["raw_user_private_data_included"]);
        Assert.Equal(bool.FalseString, metadata["provider_payload_included"]);

        AssertNoPrivateTrainingIntakePayload(string.Join("|", metadata.Select(pair => $"{pair.Key}={pair.Value}")), privateMarker);
    }

    private static void AssertSafeTrainingIntakeScalar(string? value, string privateMarker)
    {
        Assert.False(string.IsNullOrWhiteSpace(value));
        Assert.DoesNotContain(privateMarker, value!, StringComparison.Ordinal);
        Assert.DoesNotContain("<", value!, StringComparison.Ordinal);
        Assert.DoesNotContain("{", value!, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/", value!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:\\", value!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("file://", value!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private", value!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", value!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_provider_payload", value!, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoPrivateTrainingIntakePayload(
        string bodyJson,
        string privateMarker)
    {
        Assert.DoesNotContain(privateMarker, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_provider_payload", bodyJson, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<DesignConversionModelServerHttpResponse> InvokeTrainingIntakeRouteAsync(
        DesignConversionModelServerEndpointRegistry registry,
        object body,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        var response = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: headers));
        Assert.True(response.IsSuccess, response.ErrorMessage);
        return response.Value!;
    }

    private static DesignConversionModelServerEndpointRegistry CreateRegistry(
        IDesignConversionLocalArtifactDb db,
        string? expectedTrainingIntakeAuthToken = null)
    {
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionTrainingIntake(
            new DesignConversionModelServerTrainingIntakeHandler(
                new DesignConversionDpoUsefulnessApi(db),
                expectedTrainingIntakeAuthToken));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return registry;
    }

    private static async Task<SeededTrainingIntakeContext> CreateSeededIntakeAsync(string privateMarker)
    {
        var seeded = await CreateApprovedHandoffAsync(privateMarker);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);

        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:modelserver-training-intake-test",
            "local ModelServer training-intake route test; no ProductReady claim",
            DateTimeOffset.UnixEpoch.AddMinutes(202));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        Assert.True(intake.Value!.TrainingIntakeOnly);
        Assert.True(intake.Value.ProductNotReady);

        return new SeededTrainingIntakeContext(seeded.Db, intake.Value.IntakeId);
    }

    private static async Task<SeededApprovedHandoffContext> CreateApprovedHandoffAsync(string privateMarker)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "modelserver-training-intake-route");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var api = new DesignConversionDpoUsefulnessApi(db);
        await Save(db, BuildArtifactRecord(
            "training-intake-route-record",
            "training-intake-route-job",
            "training-intake-route-request",
            [
                ToRuntimeApprovedCandidate(WithPersistedRuntimeArtifactRefs(
                    BuildTrainingRecord("training-intake-route-approved", DesignConversionDpoUnits.CriticUnit, 0.94),
                    "training-intake-route-artifact-000"))
            ],
            privateMarker));

        var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "training-intake-route-job"),
            DateTimeOffset.UnixEpoch.AddMinutes(201));
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.Equal(1, handoff.Value!.ApprovedCandidateCount);

        return new SeededApprovedHandoffContext(db, handoff.Value, root);
    }

    private static async Task<SeededApprovedHandoffContext> CreateSingleSourcePersistedApprovedHandoffAsync(string privateMarker)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "modelserver-training-intake-single-source-route");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var api = new DesignConversionDpoUsefulnessApi(db);
        await Save(db, BuildArtifactRecord(
            "training-intake-single-source-route-record",
            "training-intake-single-source-route-job",
            "training-intake-single-source-route-request",
            [
                ToRuntimeApprovedCandidate(WithPersistedRuntimeArtifactRefs(
                    BuildTrainingRecord(
                        "training-intake-single-source-route-approved",
                        DesignConversionDpoUnits.CriticUnit,
                        0.94),
                    "training-intake-single-source-route-artifact-000"))
            ],
            privateMarker));

        var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "training-intake-single-source-route-job"),
            DateTimeOffset.UnixEpoch.AddMinutes(204));
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.Equal(1, handoff.Value!.ApprovedCandidateCount);

        return new SeededApprovedHandoffContext(db, handoff.Value, root);
    }

    private static async Task<SeededApprovedHandoffContext> CreateSourceDisjointApprovedHandoffAsync(
        string privateMarker,
        IDesignConversionPromotionUploader? promotionUploader = null)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "modelserver-training-intake-lifecycle-route");
        var db = new JsonlDesignConversionLocalArtifactDb(root, promotionUploader);
        var api = new DesignConversionDpoUsefulnessApi(db);
        var records = BuildSourceDisjointTrainingRecords(100)
            .Select(ToRuntimeApprovedCandidate)
            .ToArray();
        await Save(db, BuildArtifactRecord(
            "training-intake-lifecycle-route-record",
            "training-intake-lifecycle-route-job",
            "training-intake-lifecycle-route-request",
            records,
            privateMarker));

        var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "training-intake-lifecycle-route-job"),
            DateTimeOffset.UnixEpoch.AddMinutes(203));
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.Equal(100, handoff.Value!.ApprovedCandidateCount);

        return new SeededApprovedHandoffContext(db, handoff.Value, root);
    }

    private static async Task<SeededApprovedHandoffContext> CreatePartialSourceManifestApprovedHandoffAsync(string privateMarker)
    {
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var rows = seeded.Handoff.Rows
            .Select((row, index) => index == 0
                ? row with { SourceManifestRef = string.Empty }
                : row)
            .ToArray();

        return new SeededApprovedHandoffContext(
            seeded.Db,
            seeded.Handoff with { Rows = rows },
            seeded.RootDirectory);
    }

    private static async Task<SeededApprovedHandoffContext> CreateFullyMissingSourceManifestApprovedHandoffAsync(string privateMarker)
    {
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker);
        var rows = seeded.Handoff.Rows
            .Select(row => row with { SourceManifestRef = string.Empty })
            .ToArray();

        return new SeededApprovedHandoffContext(
            seeded.Db,
            seeded.Handoff with { Rows = rows },
            seeded.RootDirectory);
    }

    private static async Task Save(
        IDesignConversionLocalArtifactDb db,
        DesignConversionLocalArtifactDbRecord record)
    {
        var saved = await db.AppendRecordAsync(record);
        Assert.True(saved.IsSuccess, saved.ErrorMessage);
    }

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string unitId,
        double usefulness)
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample(id, unitId, usefulness) with
        {
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "modelserver-training-intake-request-redacted",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://design-conversion/modelserver-training-intake-source-redacted",
                ReviewerRef: "teacher:modelserver-training-intake-redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["split"] = "runtime_training_candidate_review_approved",
                    ["source_quality"] = "teacher-reviewed"
                })
        };
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords([example]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        return build.Value!.AllRecords.Single();
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildSourceDisjointTrainingRecords(int count)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            Enumerable.Range(0, count)
                .Select(index =>
                {
                    var unitId = index % 2 == 0
                        ? DesignConversionDpoUnits.CriticUnit
                        : DesignConversionDpoUnits.ImproverUnit;
                    var sourceId = $"training-intake-lifecycle-source-{index:000}";
                    var example = DesignConversionDpoGeneratorTests.CreateExample(
                        $"training-intake-lifecycle-record-{index:000}",
                        unitId,
                        usefulness: 0.80 + (index % 10 * 0.01));
                    return example with
                    {
                        Provenance = example.Provenance with
                        {
                            SourceRequestId = $"request:{sourceId}",
                            SourceManifestRef = $"manifest://design-conversion/modelserver-lifecycle/{sourceId}",
                            Metadata = new Dictionary<string, string>
                            {
                                ["source_design_id"] = sourceId,
                                ["source_quality"] = "teacher-reviewed",
                                ["split"] = "runtime_training_candidate_review_approved"
                            }
                        }
                    };
                })
                .ToArray());

        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);
        Assert.Equal(count, build.Value.AcceptedRecords.Count);
        return build.Value.AcceptedRecords
            .Select((record, index) => WithPersistedRuntimeArtifactRefs(
                record,
                $"training-intake-lifecycle-artifact-{index:000}"))
            .ToArray();
    }

    private static DesignConversionDpoTrainingRecord WithPersistedRuntimeArtifactRefs(
        DesignConversionDpoTrainingRecord record,
        string artifactId)
        => record with
        {
            Chosen = record.Chosen with
            {
                Html = $"artifact://design-conversion/{artifactId}/chosen.html",
                Css = $"artifact://design-conversion/{artifactId}/chosen.css",
                QaResults = [CreateRuntimeArtifactQaResult($"{artifactId}:chosen", score: 0.92, passed: true)]
            },
            Rejected = record.Rejected with
            {
                Html = $"artifact://design-conversion/{artifactId}/rejected.html",
                Css = $"artifact://design-conversion/{artifactId}/rejected.css",
                QaResults = [CreateRuntimeArtifactQaResult($"{artifactId}:rejected", score: 0.42, passed: false)]
            }
        };

    private static RenderQaResultRecord CreateRuntimeArtifactQaResult(
        string artifactId,
        double score,
        bool passed)
    {
        var resolution = new RenderQaResolution("desktop", 1440, 900, 1);
        var expected = new RenderQaImageRecord(
            ImageId: $"{artifactId}:expected",
            Label: "expected",
            Resolution: resolution,
            Uri: $"artifact://design-conversion/{artifactId}/expected.png",
            Sha256: $"sha256:{artifactId}:expected",
            Metadata: new Dictionary<string, string>());
        var actual = new RenderQaImageRecord(
            ImageId: $"{artifactId}:actual",
            Label: "actual",
            Resolution: resolution,
            Uri: $"artifact://design-conversion/{artifactId}/actual.png",
            Sha256: $"sha256:{artifactId}:actual",
            Metadata: new Dictionary<string, string>());
        var verdict = new VisualVerdict(
            VerdictId: $"{artifactId}:verdict",
            Score: score,
            Defects: [],
            EvidenceRefs: [$"artifact://design-conversion/{artifactId}/comparison-evidence"],
            Passed: passed);

        return new RenderQaResultRecord(
            ResultId: $"{artifactId}:qa-result",
            RequestId: $"{artifactId}:qa-request",
            Expected: expected,
            Actual: actual,
            Verdict: verdict,
            Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - score },
            Warnings: []);
    }

    private static DesignConversionTrainingPipelineOptions BuildTrainingIntakeActualRouteOptions(
        string runId,
        string datasetId,
        string gateId,
        bool sourceDisjoint)
        => new(
            RunId: runId,
            BatchSize: 25,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: datasetId,
                MinimumTrainRecords: 50,
                MinimumHeldOutRecords: 50,
                Preflight: DesignConversionDpoDatasetPreflight.LocalTrainingIntakeSourceManifestDisjointProof()),
            MeasurementGateRequest: BuildPassingLifecycleMeasurementGate(gateId, sourceDisjoint));

    private static DesignConversionMeasurementGateRequest BuildPassingLifecycleMeasurementGate(
        string gateId = "training-intake-lifecycle-command-measurement-gate",
        bool sourceDisjoint = true)
        => new(
            GateId: gateId,
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 50,
                HeldOutQualityScore: 0.94,
                RuntimeMs: 320,
                CostUsd: 0m,
                PeakRamMb: 256,
                EvidenceRefs: [$"{gateId}:heldout-fixture"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: sourceDisjoint,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs: [$"{gateId}:integrity-fixture"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionModelServerApiRequest CreateTrainingIntakeModelServerRequest(string requestId)
        => new(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: "training-intake-factory-figma-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:training-intake-factory-figma-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"factory-safe\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: "training-intake-factory-original-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:training-intake-factory-original-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"factory-original\"}",
                Format: "figma-json"),
            Html: "<main class=\"screen\"><h1>Factory dashboard</h1></main>",
            Css: ".screen{display:grid;gap:16px;color:#111827;}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "training-intake-factory-screenshot-desktop",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://training-intake/factory/screenshot-desktop.png",
                    Sha256: "sha256:training-intake-factory-screenshot-desktop")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "training-intake-factory-design-render-desktop",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://training-intake/factory/design-render-desktop.png",
                    Sha256: "sha256:training-intake-factory-design-render-desktop")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "user-redacted",
                Tier: "pro",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.25m),
            Metadata: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "training-intake-factory-local-source-manifest",
                ["caller"] = "training-intake-actual-route-test"
            });

    private static VisualVerdict CreateTrainingIntakeVerdict(
        string verdictId,
        double score,
        bool passed)
        => new(
            VerdictId: verdictId,
            Score: score,
            Defects: [],
            EvidenceRefs: [$"verdict:{verdictId}"],
            Passed: passed);

    private static DesignConversionDpoTrainingRecord ToRuntimeApprovedCandidate(
        DesignConversionDpoTrainingRecord record)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus,
            ["review_status"] = DesignConversionDpoUsefulnessApi.RuntimeCandidateReviewApprovedStatus,
            ["approved_for_training"] = bool.TrueString,
            ["approved_by_ref"] = "reviewer:modelserver-training-intake-test",
            ["approval_reason"] = "local training-intake route test approval",
            ["approved_at_utc"] = DateTimeOffset.UnixEpoch.AddMinutes(200).ToString("O"),
            ["training_candidate_only"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["split"] = "runtime_training_candidate_review_approved"
        };
        var licenseMetadata = new Dictionary<string, string>(record.License.Metadata, StringComparer.Ordinal)
        {
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus,
            ["approved_for_training"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString
        };

        return record with
        {
            Provenance = record.Provenance with
            {
                ReviewerRef = "reviewer:modelserver-training-intake-test",
                Metadata = metadata
            },
            License = record.License with
            {
                Metadata = licenseMetadata
            }
        };
    }

    private static DesignConversionLocalArtifactDbRecord BuildArtifactRecord(
        string recordId,
        string jobId,
        string requestId,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        string privateMarker)
        => new(
            RecordId: recordId,
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: jobId,
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "pro",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = $"file:///C:/Users/private-user/Documents/private.fig?token={privateMarker}",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = $"artifact://design-conversion/{recordId}/design-tree.json",
                ["original_tree_payload_ref"] = $"artifact://design-conversion/{recordId}/original-tree.json",
                ["source_artifact:manifest"] = $"artifact://design-conversion/{recordId}/source-manifest.json"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion-result",
                HtmlRef: $"<main data-secret=\"{privateMarker}\">exact_prompt private html</main>",
                HtmlSha256: string.Empty,
                CssRef: $".private{{background:url('C:/Users/private-user/{privateMarker}.png');}}",
                CssSha256: string.Empty,
                Metadata: new Dictionary<string, string>
                {
                    ["raw_provider_payload"] = privateMarker
                }),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: $"{recordId}:render:desktop",
                    Kind: "browser_screenshot",
                    UriRef: $"file:///C:/Users/private-user/private/{privateMarker}/render-desktop.png",
                    Sha256: "sha256:render-desktop",
                    ByteLength: 1234,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>
                    {
                        ["private_marker"] = privateMarker
                    })
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ProviderBoundary: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: null,
                TierDecision: DesignConversionVisualQaBoundaryPolicy.CostCapMarker,
                EvidenceRefs: [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, "visual_judge_status", "product_not_ready"],
                Metadata: new Dictionary<string, string>
                {
                    ["visual_judge_status"] = DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    ["raw_provider_payload"] = privateMarker
                }),
            DpoCandidates: records
                .Select(record => DesignConversionLocalArtifactDbDpoCandidate.FromTrainingRecord(record))
                .ToArray(),
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "storage_training_artifact_db",
                "local_artifact_db",
                "conversion_result",
                "render_artifact_refs",
                "visual_judge_status",
                "dpo_candidate",
                "usefulness_score",
                "training_candidate_only",
                "product_not_ready"
            ],
            Metadata: new Dictionary<string, string>
            {
                ["private_marker"] = privateMarker
            });

    private static Uri ResolveStartedAddress(WebApplication app)
    {
        var addresses = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()?
            .Addresses
            .ToArray() ?? [];
        var address = Assert.Single(addresses);
        return new Uri(address);
    }

    private static async Task<(HttpResponseMessage Response, string BodyJson)> PostTrainingIntakeAsync(
        HttpClient client,
        object body)
    {
        using var content = new StringContent(
            JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            content);
        var bodyJson = await response.Content.ReadAsStringAsync();
        return (response, bodyJson);
    }

    private static void AssertTrainingIntakeStartupMetadata(
        WebApplication app,
        bool externalProviderConfigured)
    {
        var startupMetadata = app.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .Select(endpoint => endpoint.Metadata.GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>())
            .Where(metadata => metadata is not null)
            .Select(metadata => metadata!)
            .Single(metadata => string.Equals(
                metadata.MappedRoute,
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                StringComparison.Ordinal));

        Assert.Equal(DesignConversionModelServerAspNetHostStartup.HostStartupSurface, startupMetadata.StartupSurface);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath, startupMetadata.MappedRoute);
        Assert.Equal(externalProviderConfigured, startupMetadata.ExternalProviderConfigured);
        Assert.Equal(!externalProviderConfigured, startupMetadata.LocalOnly);
        Assert.True(startupMetadata.RefsOnly);
        Assert.False(startupMetadata.PublicDeploymentVerified);
        Assert.False(startupMetadata.CrossRepoCallerVerified);
        Assert.False(startupMetadata.ProductReady);
    }

    private static DesignConversionModelServerPromotionHostOptions CreatePromotionHostOptions(
        string artifactRoot,
        HttpClient promotionHttpClient)
        => new(
            artifactRoot,
            promotionHttpClient,
            CreatePromotionHttpUploaderOptions(artifactRoot, PromotionHostTenantId));

    private static DesignConversionModelServerPromotionHttpUploaderOptions CreatePromotionHttpUploaderOptions(
        string allowedArtifactRoot,
        string tenantId)
        => new(
            PromotionUploadEndpoint,
            tenantId,
            PromotionHostBearerToken,
            allowedArtifactRoot)
        {
            Timeout = TimeSpan.FromSeconds(5),
            MaxArtifactBytes = 1024 * 1024,
            MaxTotalArtifactBytes = 3 * 1024 * 1024,
            MaxResponseBodyBytes = 64 * 1024,
            MaxArtifactCount = DesignConversionModelServerPromotionHttpUploaderOptions.HardMaxArtifactCount
        };

    private static DesignConversionModelServerRouteAdmissionOptions CreatePromotionRouteAdmissionOptions(
        string tenantId)
        => new(
            enabled: true,
            currentToken: "route-admission-token-1234567890",
            routeScope: "design-conversion-training-intake-host-promotion",
            tenantId: tenantId);

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    public enum PromotionUploadResponseMode
    {
        Success,
        NonSuccess,
        MalformedReceipt
    }

    private sealed class CapturingPromotionUploadHttpHandler(
        PromotionUploadResponseMode responseMode) : HttpMessageHandler
    {
        private readonly Dictionary<string, IReadOnlyList<string>> _headers = new(StringComparer.OrdinalIgnoreCase);

        public int SendCount { get; private set; }

        public string? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string BodyText { get; private set; } = string.Empty;

        public CapturedPromotionUploadMetadata? Metadata { get; private set; }

        public IReadOnlyList<string> HeaderValues(string name)
            => _headers.TryGetValue(name, out var values) ? values : [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            Method = request.Method.Method;
            RequestUri = request.RequestUri;
            _headers.Clear();
            foreach (var header in request.Headers)
                _headers[header.Key] = header.Value.ToArray();
            if (request.Content is not null)
            {
                foreach (var header in request.Content.Headers)
                    _headers[header.Key] = header.Value.ToArray();
                BodyText = await request.Content.ReadAsStringAsync(cancellationToken);
                Metadata = ExtractMetadata(BodyText);
            }

            return responseMode switch
            {
                PromotionUploadResponseMode.NonSuccess => JsonResponse(
                    HttpStatusCode.BadGateway,
                    "{\"error\":\"remote promotion unavailable\"}"),
                PromotionUploadResponseMode.MalformedReceipt => JsonResponse(
                    HttpStatusCode.OK,
                    JsonSerializer.Serialize(new
                    {
                        uploadId = "bad-remote-upload",
                        uploadedAtUtc = PromotionUploadedAtUtc
                    })),
                _ => JsonResponse(HttpStatusCode.Created, BuildSuccessResponse())
            };
        }

        private string BuildSuccessResponse()
        {
            if (Metadata is null)
                return "{}";

            return JsonSerializer.Serialize(
                new
                {
                    uploadId = "route-remote-upload-1",
                    remoteRef = "https://promotion.example/receipts/route-remote-upload-1",
                    uploadedAtUtc = PromotionUploadedAtUtc,
                    tenantId = Metadata.TenantId,
                    manifestId = Metadata.ManifestId,
                    machineId = Metadata.MachineId,
                    domainTag = Metadata.DomainTag,
                    version = Metadata.Version,
                    idempotencyKey = Metadata.IdempotencyKey,
                    artifacts = Metadata.Artifacts
                        .Select(artifact => new
                        {
                            role = artifact.Role,
                            length = artifact.Length,
                            sha256 = artifact.Sha256
                        })
                        .ToArray()
                },
                DesignConversionModelServerHttpHandler.JsonOptions);
        }

        private static CapturedPromotionUploadMetadata ExtractMetadata(string bodyText)
        {
            var markerIndex = bodyText.IndexOf("name=metadata", StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
                markerIndex = bodyText.IndexOf("name=\"metadata\"", StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
                throw new InvalidOperationException("Promotion upload multipart metadata part was not found.");

            var jsonStart = bodyText.IndexOf('{', markerIndex);
            if (jsonStart < 0)
                throw new InvalidOperationException("Promotion upload multipart metadata JSON was not found.");
            var jsonEnd = bodyText.IndexOf("\r\n--", jsonStart, StringComparison.Ordinal);
            if (jsonEnd < 0)
                jsonEnd = bodyText.IndexOf("\n--", jsonStart, StringComparison.Ordinal);
            if (jsonEnd < 0)
                jsonEnd = bodyText.Length;

            using var document = JsonDocument.Parse(bodyText[jsonStart..jsonEnd].Trim());
            var root = document.RootElement;
            var artifacts = root.GetProperty("artifacts")
                .EnumerateArray()
                .Select(artifact => new CapturedPromotionUploadArtifact(
                    artifact.GetProperty("role").GetString() ?? string.Empty,
                    artifact.GetProperty("fileName").GetString() ?? string.Empty,
                    artifact.GetProperty("length").GetInt64(),
                    artifact.GetProperty("sha256").GetString() ?? string.Empty))
                .ToArray();

            return new CapturedPromotionUploadMetadata(
                root.GetProperty("contractVersion").GetString() ?? string.Empty,
                root.GetProperty("tenantId").GetString() ?? string.Empty,
                root.GetProperty("manifestId").GetString() ?? string.Empty,
                root.GetProperty("machineId").GetString() ?? string.Empty,
                root.GetProperty("domainTag").GetString() ?? string.Empty,
                root.GetProperty("version").GetString() ?? string.Empty,
                root.GetProperty("idempotencyKey").GetString() ?? string.Empty,
                root.GetProperty("acceptedRecordCount").GetInt32(),
                root.GetProperty("quarantinedRecordCount").GetInt32(),
                artifacts);
        }

        private static HttpResponseMessage JsonResponse(
            HttpStatusCode statusCode,
            string body)
            => new(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
    }

    private sealed record CapturedPromotionUploadMetadata(
        string ContractVersion,
        string TenantId,
        string ManifestId,
        string MachineId,
        string DomainTag,
        string Version,
        string IdempotencyKey,
        int AcceptedRecordCount,
        int QuarantinedRecordCount,
        IReadOnlyList<CapturedPromotionUploadArtifact> Artifacts);

    private sealed record CapturedPromotionUploadArtifact(
        string Role,
        string FileName,
        long Length,
        string Sha256);

    private sealed class TrainingIntakeRecordingRenderProvider : IDesignConversionRenderProvider
    {
        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://training-intake/render/{request.RenderRequestId}.png",
                Sha256: $"sha256:{request.RenderRequestId}:actual",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 7,
                Warnings: ["training-intake-recording-render-provider"])));
        }
    }

    private sealed class TrainingIntakeRecordingComparisonProvider(VisualVerdict verdict) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://training-intake/diff/{request.ComparisonRequestId}.png",
                DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                EvidenceRefs: ["training-intake-comparison-provider"])));
        }
    }

    private sealed class FailingTrainingIntakePromotionUploader(
        string errorCode,
        string errorMessage) : IDesignConversionPromotionUploader
    {
        public int UploadCallCount { get; private set; }

        public Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
        {
            UploadCallCount++;
            return Task.FromResult(OperationResult<DesignConversionPromotionUploadReceipt>.Fail(
                errorMessage,
                errorCode));
        }
    }

    private sealed class UnexpectedCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "Training-intake host promotion tests do not expect improvement candidate generation.",
                "TRAINING_INTAKE_HOST_PROMOTION_UNEXPECTED_CANDIDATE_REQUEST"));
    }

    private sealed record SeededTrainingIntakeContext(
        IDesignConversionLocalArtifactDb Db,
        string IntakeId);

    private sealed record SeededApprovedHandoffContext(
        IDesignConversionLocalArtifactDb Db,
        DesignConversionApprovedDpoHandoffArtifact Handoff,
        string RootDirectory);
}
