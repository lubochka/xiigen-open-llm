// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelServer.Contracts;
using ModelServer.DesignConversion;
using DesignDomain = DesignConversionDomain.DesignConversionDomain;

public sealed class DesignConversionActivatedModelRepositoryHostRuntimeTests
{
    private const string TenantId = "tenant-design-conversion-repository-runtime";
    private const string ServerBaseUrl = "https://models.example/repository";
    private const string RouteAdmissionToken = "repository-runtime-route-admission-token";
    private const string RouteAdmissionPreviousToken = "repository-runtime-route-admission-previous-token";
    private const string LocalToken = "repository-runtime-local-lifecycle-token";
    private const string PrivateMarker = "PRIVATE_REPOSITORY_HOST_RUNTIME_MARKER";
    private static readonly DateTimeOffset FixedUtcNow = DateTimeOffset.Parse("2026-07-27T09:15:00Z");
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    [Fact]
    public async Task RepositoryPromotionHostEvaluate_LoadsActiveCheckpointThroughRepositoryInsteadOfCorruptLocalPath()
    {
        var root = NewRoot();
        WebApplication? app = null;
        try
        {
            var artifactRoot = ArtifactRoot(root);
            var activationStateRoot = Path.Combine(root, "activation-state");
            var candidate = await BuildActivationRecordAsync(artifactRoot, TenantId, "repository-host-runtime", batchCount: 2);
            var validCheckpointBytes = await File.ReadAllBytesAsync(candidate.Model.CheckpointPath);
            var checkpointSha256 = Sha256Hex(validCheckpointBytes);
            Assert.Equal(candidate.Model.CheckpointSha256, checkpointSha256);

            var activationRegistry = new DesignConversionModelActivationRegistry(
                activationStateRoot,
                () => FixedUtcNow,
                allowedArtifactRootDirectory: artifactRoot);
            var activated = await activationRegistry.ActivateAsync(
                new DesignConversionModelActivationCommand(
                    TenantId,
                    RequestedByRef: "architect:repository-runtime-host-test",
                    Reason: "pre-seed active repository runtime model before HTTP evaluate",
                    IdempotencyKey: "activate:repository-runtime-host-test",
                    ExpectedGeneration: 0,
                    Candidate: candidate,
                    ExpectedModelVersion: candidate.Model.Version,
                    RequestedAtUtc: FixedUtcNow));
            Assert.True(activated.IsSuccess, activated.ErrorMessage);

            await File.WriteAllTextAsync(
                candidate.Model.CheckpointPath,
                $"{PrivateMarker}:corrupt active local checkpoint path must not be read",
                StrictUtf8);
            Assert.NotEqual(checkpointSha256, Sha256Hex(await File.ReadAllBytesAsync(candidate.Model.CheckpointPath)));

            var calls = new List<string>();
            var repository = new RecordingModelRepository(calls, validCheckpointBytes);
            var locator = new RecordingRemoteUnitLocator(calls, BuildManifestMap(candidate.Model, checkpointSha256));
            app = BuildRepositoryPromotionApp(root, artifactRoot, activationStateRoot, repository, locator);
            await app.StartAsync();

            using var client = new HttpClient { BaseAddress = new Uri(ResolveStartedBaseAddress(app)) };
            var requestId = "repository-runtime-host-evaluate";
            using var request = new HttpRequestMessage(HttpMethod.Post, DesignConversionModelServerEndpointRegistry.EndpointPath)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(CreateEvaluateRequest(TenantId, requestId), DesignConversionModelServerHttpHandler.JsonOptions),
                    StrictUtf8,
                    "application/json")
            };
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
                "Bearer " + RouteAdmissionToken);
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
                requestId + "-idempotency");

            using var response = await client.SendAsync(request);
            var bodyJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(
                [
                    "locate:critic",
                    "load:critic",
                    "locate:improver",
                    "load:improver"
                ],
                calls);
            Assert.Empty(repository.UploadCalls);
            Assert.Empty(locator.RegisterCalls);
            AssertRepositoryCalls(repository, candidate.Model.CheckpointPath);
            AssertLocatorCalls(locator, candidate.Model, checkpointSha256);
            AssertEvaluateResponseSelectedActiveFreshRuntime(bodyJson, candidate.Model);

            var cachedRequestId = requestId + "-cached";
            using var cachedRequest = new HttpRequestMessage(HttpMethod.Post, DesignConversionModelServerEndpointRegistry.EndpointPath)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(CreateEvaluateRequest(TenantId, cachedRequestId), DesignConversionModelServerHttpHandler.JsonOptions),
                    StrictUtf8,
                    "application/json")
            };
            cachedRequest.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
                "Bearer " + RouteAdmissionToken);
            cachedRequest.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
                cachedRequestId + "-idempotency");

            using var cachedResponse = await client.SendAsync(cachedRequest);
            var cachedBodyJson = await cachedResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, cachedResponse.StatusCode);
            AssertEvaluateResponseSelectedActiveCachedRuntime(cachedBodyJson, candidate.Model);
            Assert.Equal(2, repository.LoadCalls.Count);
            Assert.Equal(2, locator.LocateCalls.Count);

            var cachePath = ExpectedRepositoryPromotionCachePath(artifactRoot, checkpointSha256);
            Assert.True(File.Exists(cachePath), cachePath);
            Assert.NotEqual(Path.GetFullPath(candidate.Model.CheckpointPath), Path.GetFullPath(cachePath));
            Assert.StartsWith(
                EnsureTrailingDirectorySeparator(Path.GetFullPath(artifactRoot)),
                Path.GetFullPath(cachePath),
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            Assert.Equal(checkpointSha256, Sha256Hex(await File.ReadAllBytesAsync(cachePath)));
            var cachedLoad = await DesignConversionMachine.LoadCheckpointAsync(cachePath);
            Assert.True(cachedLoad.IsSuccess, cachedLoad.ErrorMessage);
            Assert.Equal(candidate.Model.Version, cachedLoad.Value!.Checkpoint.Version);

            var responseSurface = bodyJson + Environment.NewLine +
                                  string.Join(Environment.NewLine, response.Headers.Select(header => header.Key + ":" + string.Join(",", header.Value)));
            AssertNoPrivateSurfaceLeak(responseSurface, root, artifactRoot, candidate.Model.CheckpointPath);
        }
        finally
        {
            if (app is not null)
            {
                await app.StopAsync();
                await app.DisposeAsync();
            }

            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task RepositoryPromotionHostEvaluate_FailedConcurrentLoadPreservesRepositoryRuntimeWithoutFreshEvidence()
    {
        var root = NewRoot();
        WebApplication? app = null;
        try
        {
            var artifactRoot = ArtifactRoot(root);
            var activationStateRoot = Path.Combine(root, "activation-state");
            var candidate = await BuildActivationRecordAsync(
                artifactRoot,
                TenantId,
                "repository-host-concurrent-preserved-runtime",
                batchCount: 2);
            var checkpointBytes = await File.ReadAllBytesAsync(candidate.Model.CheckpointPath);
            var checkpointSha256 = Sha256Hex(checkpointBytes);
            var activationRegistry = new DesignConversionModelActivationRegistry(
                activationStateRoot,
                () => FixedUtcNow,
                allowedArtifactRootDirectory: artifactRoot);
            var activated = await activationRegistry.ActivateAsync(
                new DesignConversionModelActivationCommand(
                    TenantId,
                    RequestedByRef: "architect:repository-runtime-concurrent-preservation-test",
                    Reason: "pre-seed repository runtime for concurrent failed-load preservation",
                    IdempotencyKey: "activate:repository-runtime-concurrent-preservation-test",
                    ExpectedGeneration: 0,
                    Candidate: candidate,
                    ExpectedModelVersion: candidate.Model.Version,
                    RequestedAtUtc: FixedUtcNow));
            Assert.True(activated.IsSuccess, activated.ErrorMessage);

            await File.WriteAllTextAsync(
                candidate.Model.CheckpointPath,
                $"{PrivateMarker}:concurrent preservation must use repository bytes",
                StrictUtf8);
            var repository = new FirstLoadBlockingFailingModelRepository(checkpointBytes);
            var locator = new StaticRemoteUnitLocator(BuildManifestMap(candidate.Model, checkpointSha256));
            app = BuildRepositoryPromotionApp(root, artifactRoot, activationStateRoot, repository, locator);
            await app.StartAsync();

            using var client = new HttpClient { BaseAddress = new Uri(ResolveStartedBaseAddress(app)) };
            var preservedResponseTask = SendEvaluateAsync(client, "repository-runtime-concurrent-preserved");
            await repository.FirstLoadEntered.WaitAsync(TimeSpan.FromSeconds(10));

            var freshResponse = await SendEvaluateAsync(client, "repository-runtime-concurrent-fresh");
            Assert.Equal(HttpStatusCode.OK, freshResponse.StatusCode);
            AssertEvaluateResponseSelectedActiveFreshRuntime(freshResponse.BodyJson, candidate.Model);

            repository.ReleaseFirstLoadFailure();
            var preservedResponse = await preservedResponseTask;
            Assert.Equal(HttpStatusCode.OK, preservedResponse.StatusCode);
            AssertEvaluateResponseSelectedActivePreservedRuntime(preservedResponse.BodyJson, candidate.Model);
        }
        finally
        {
            if (app is not null)
            {
                await app.StopAsync();
                await app.DisposeAsync();
            }

            DeleteDirectoryIfExists(root);
        }
    }

    private static WebApplication BuildRepositoryPromotionApp(
        string root,
        string artifactRoot,
        string activationStateRoot,
        IModelRepository repository,
        IRemoteUnitLocator locator)
    {
        var previousBudgetRoot = Environment.GetEnvironmentVariable(
            JsonlDesignConversionVisualCostBudgetStore.VisualCostBudgetStoreRootEnvironmentVariable);
        var previousLedgerRoot = Environment.GetEnvironmentVariable(
            JsonlDesignConversionVisualCostLedger.VisualCostLedgerRootEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostBudgetStore.VisualCostBudgetStoreRootEnvironmentVariable,
                Path.Combine(root, "visual-cost-budget-store"));
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostLedger.VisualCostLedgerRootEnvironmentVariable,
                Path.Combine(root, "visual-cost-ledger"));

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = Environments.Development,
                ApplicationName = typeof(DesignConversionModelServerAspNetHostStartup).Assembly.GetName().Name,
                ContentRootPath = Directory.GetCurrentDirectory()
            });
            builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
            var app = builder.Build();
            app.ConfigureDesignConversionModelServer(
                DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting(),
                new UnexpectedCandidateStrategy(),
                new DesignConversionModelServerPromotionHostOptions(
                    ArtifactRootDirectory: artifactRoot,
                    RepositoryPromotionOptions: new DesignConversionModelServerRepositoryPromotionOptions(
                        repository,
                        locator,
                        ServerBaseUrl,
                        TenantId)),
                utcNow: () => FixedUtcNow,
                activationStateRootDirectory: activationStateRoot,
                modelLifecycleAuthToken: LocalToken,
                routeAdmissionOptions: new DesignConversionModelServerRouteAdmissionOptions(
                    enabled: true,
                    currentToken: RouteAdmissionToken,
                    previousToken: RouteAdmissionPreviousToken,
                    previousTokenExpiresAtUtc: FixedUtcNow.AddMinutes(5),
                    tenantId: TenantId));
            return app;
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostBudgetStore.VisualCostBudgetStoreRootEnvironmentVariable,
                previousBudgetRoot);
            Environment.SetEnvironmentVariable(
                JsonlDesignConversionVisualCostLedger.VisualCostLedgerRootEnvironmentVariable,
                previousLedgerRoot);
        }
    }

    private static string ResolveStartedBaseAddress(WebApplication app)
    {
        var addresses = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()
            ?.Addresses
            .ToArray() ?? [];
        var address = addresses.FirstOrDefault(item => !item.EndsWith(":0", StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(address), "Started WebApplication did not expose a bound address.");
        return address;
    }

    private static async Task<(HttpStatusCode StatusCode, string BodyJson)> SendEvaluateAsync(
        HttpClient client,
        string requestId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, DesignConversionModelServerEndpointRegistry.EndpointPath)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(CreateEvaluateRequest(TenantId, requestId), DesignConversionModelServerHttpHandler.JsonOptions),
                StrictUtf8,
                "application/json")
        };
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
            "Bearer " + RouteAdmissionToken);
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
            requestId + "-idempotency");

        using var response = await client.SendAsync(request);
        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    internal static async Task<DesignConversionPromotedModelActivationRecord> BuildActivationRecordAsync(
        string artifactRoot,
        string tenantId,
        string name,
        int batchCount)
    {
        var outputDirectory = Path.Combine(artifactRoot, "models", name);
        Directory.CreateDirectory(outputDirectory);
        var machine = new DesignConversionMachine();
        for (var index = 0; index < batchCount; index++)
        {
            var trained = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(machine, new LearningBatch<DesignDomain>(
                BatchId: $"{name}-batch-{index}",
                DomainTag: DesignConversionMachine.DomainTag,
                TrainingItems:
                [
                    BuildTrainingItem(name, index, DesignConversionRuntimeScaffoldCriticUnit.UnitId, "critic"),
                    BuildTrainingItem(name, index, DesignConversionRuntimeScaffoldImproverUnit.UnitId, "improver")
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
        await File.WriteAllTextAsync(tokenizerPath, "{\"kind\":\"repository-host-runtime-tokenizer\"}", StrictUtf8);
        await File.WriteAllTextAsync(policyPath, "{\"kind\":\"repository-host-runtime-policy\"}", StrictUtf8);

        var checkpointFullPath = Path.GetFullPath(checkpointPath);
        var checkpointSha = Sha256Hex(await File.ReadAllBytesAsync(checkpointFullPath));
        var manifestId = $"{name}-manifest";
        var manifestRef = $"manifest://design-conversion/{manifestId}";
        var promotionRef = DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                           Uri.EscapeDataString(manifestId);
        var manifest = new DesignConversionTrainingManifest(
            ManifestId: manifestId,
            MachineId: load.Value.Checkpoint.MachineId,
            DomainTag: load.Value.Checkpoint.DomainTag,
            Version: load.Value.Checkpoint.Version,
            CheckpointRef: checkpointFullPath,
            ManifestRef: manifestRef,
            CreatedAtUtc: FixedUtcNow,
            AcceptedRecordCount: batchCount * 2,
            QuarantinedRecordCount: 0,
            UnitIds:
            [
                DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.UnitId
            ],
            Batches:
            [
                new DesignConversionTrainingManifestBatch(
                    BatchId: $"{name}-critic-manifest-batch",
                    UnitId: DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                    RecordCount: batchCount,
                    RecordIds: Enumerable.Range(0, batchCount).Select(i => $"{name}-critic-record-{i}").ToArray(),
                    SourceIds: Enumerable.Range(0, batchCount).Select(i => $"{name}-source-{i}").ToArray()),
                new DesignConversionTrainingManifestBatch(
                    BatchId: $"{name}-improver-manifest-batch",
                    UnitId: DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                    RecordCount: batchCount,
                    RecordIds: Enumerable.Range(0, batchCount).Select(i => $"{name}-improver-record-{i}").ToArray(),
                    SourceIds: Enumerable.Range(0, batchCount).Select(i => $"{name}-source-{i}").ToArray())
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
            PromotionArtifacts = new DesignConversionTrainingManifestArtifacts(
                checkpointFullPath,
                Path.GetFullPath(tokenizerPath),
                Path.GetFullPath(policyPath)),
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
            UploadedAtUtc: FixedUtcNow,
            EvidenceRefs: ["DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN"]);
        var publication = new DesignConversionPromotionPublicationResult(decision, manifest, receipt);
        var model = new DesignConversionPromotedLocalModel(
            ModelId: $"{manifestId}:{checkpointSha[..12]}",
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

        return new DesignConversionPromotedModelActivationRecord(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(tenantId),
            publication,
            model);
    }

    private static LearningTrainingItem BuildTrainingItem(
        string name,
        int index,
        string unitId,
        string alias)
        => new(
            ItemId: $"{name}-{alias}-item-{index}",
            Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["unit_id"] = unitId,
                ["source_kind"] = "Figma",
                ["source_manifest_hash"] = $"{name}-{alias}-manifest-source-{index}",
                ["source_identity_hash"] = $"{name}-{alias}-identity-{index}",
                ["source_structure_hash"] = $"{name}-{alias}-structure-{index}"
            }),
            ChosenResponse: "{\"html\":\"<main class=\\\"layout-a\\\"></main>\",\"css\":\".layout-a{display:grid;gap:16px}\"}",
            RejectedResponse: "{\"html\":\"<main class=\\\"layout-b\\\"></main>\",\"css\":\".layout-b{display:block;gap:2px}\"}");

    private static DesignConversionModelServerApiRequest CreateEvaluateRequest(
        string tenantId,
        string requestId)
        => new(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":design-tree")}",
                Payload: "{\"kind\":\"repository-runtime-design\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":original-tree")}",
                Payload: "{\"kind\":\"repository-runtime-original\"}",
                Format: "figma-json"),
            Html: "<main><h1>Repository runtime activation</h1></main>",
            Css: "body{margin:0;color:#123456;background:#f8fafc}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:source",
                    Kind: "screenshot",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://repository-runtime/{requestId}/source.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":source")}")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"{requestId}:render",
                    Kind: "design-render",
                    Width: 800,
                    Height: 600,
                    Uri: $"artifact://repository-runtime/{requestId}/render.png",
                    Sha256: $"sha256:{Sha256Text(requestId + ":render")}")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "repository-runtime-user",
                Tier: "paid",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["design_tier"] = "paid",
                    ["monthly_visual_cost_limit_usd"] = "125.75"
                }),
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["tenant_id"] = tenantId,
                ["repository_runtime_host_test"] = bool.TrueString,
                [DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] = "paid",
                [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] = "125.75",
                [DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                    DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                [DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                    DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                [DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                    DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority
            });

    private static Dictionary<string, RemoteUnitManifest> BuildManifestMap(
        DesignConversionPromotedLocalModel model,
        string checkpointSha256)
        => new(StringComparer.Ordinal)
        {
            [DesignConversionRuntimeScaffoldCriticUnit.UnitId] = BuildManifest(
                DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                DesignConversionRuntimeScaffoldCriticUnit.Role,
                model,
                checkpointSha256),
            [DesignConversionRuntimeScaffoldImproverUnit.UnitId] = BuildManifest(
                DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.Role,
                model,
                checkpointSha256)
        };

    private static RemoteUnitManifest BuildManifest(
        string unitId,
        string unitRole,
        DesignConversionPromotedLocalModel model,
        string checkpointSha256)
        => new(
            UnitId: unitId,
            UnitRole: unitRole,
            DomainTag: model.DomainTag,
            Version: model.Version,
            ServerBaseUrl: ServerBaseUrl,
            UnitEndpoint: $"/models/{Uri.EscapeDataString(unitId)}",
            ManifestSha256: checkpointSha256.ToUpperInvariant(),
            TrainingBranch: "DesignToCode-clean",
            TrainingRunId: "repository-host-runtime-test-run",
            SourceDatasetSha256: Sha256Text("repository-host-runtime-source-dataset"),
            ModelConfig: new RemoteModelConfig(0, 0, 0, 0, 0, 0, 0),
            LoraConfig: new RemoteLoraConfig(0, 0f, 0f, []),
            TokenizerPath: "tokenizer-ref-only",
            RegisteredAtUtc: FixedUtcNow);

    private static void AssertRepositoryCalls(
        RecordingModelRepository repository,
        string expectedFallbackPath)
    {
        Assert.Equal(
            [
                new RepositoryLoadCall(
                    DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                    ServerBaseUrl,
                    Path.GetFullPath(expectedFallbackPath)),
                new RepositoryLoadCall(
                    DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                    ServerBaseUrl,
                    Path.GetFullPath(expectedFallbackPath))
            ],
            repository.LoadCalls);
    }

    private static void AssertLocatorCalls(
        RecordingRemoteUnitLocator locator,
        DesignConversionPromotedLocalModel model,
        string checkpointSha256)
    {
        Assert.Equal(
            [
                DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.UnitId
            ],
            locator.LocateCalls);
        Assert.All(locator.ReturnedManifests, manifest =>
        {
            Assert.Equal(model.DomainTag, manifest.DomainTag);
            Assert.Equal(model.Version, manifest.Version);
            Assert.Equal(ServerBaseUrl, manifest.ServerBaseUrl);
            Assert.Equal($"/models/{Uri.EscapeDataString(manifest.UnitId)}", manifest.UnitEndpoint);
            Assert.Equal(checkpointSha256.ToUpperInvariant(), manifest.ManifestSha256);
        });
    }

    private static void AssertEvaluateResponseSelectedActiveFreshRuntime(
        string bodyJson,
        DesignConversionPromotedLocalModel model)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.False(root.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.True(root.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.False(root.GetProperty("productReady").GetBoolean());

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
        Assert.True(root.GetProperty("modelTrainingVerified").GetBoolean());
        Assert.True(root.GetProperty("trainingReadiness").GetProperty("modelTrainingVerified").GetBoolean());
        Assert.Equal(model.ModelId, metadata.GetProperty("local_promoted_model_id").GetString());
        Assert.Equal(model.CheckpointSha256, metadata.GetProperty("local_promoted_checkpoint_sha256").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_model_promotion_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_promoted_checkpoint_loaded").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_promoted_checkpoint_real_weights").GetString());
        Assert.Equal(
            "production_activated_runtime_registry_checkpoint_load",
            metadata.GetProperty("local_model_verification_authority").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_tenant_bound").GetString());
        Assert.Equal(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(TenantId),
            metadata.GetProperty("route_admission_tenant_id_ref").GetString());
        Assert.Contains(DesignConversionActivatedModelRuntimeEvaluator.ActiveSelectionEvidence, evidenceRefs);
        Assert.Contains(DesignConversionActivatedModelRuntimeEvaluator.FreshLoadEvidence, evidenceRefs);
        Assert.Contains(
            DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
            evidenceRefs);
        Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, evidenceRefs);
        Assert.Contains("active_model_runtime_fresh_load_succeeded=True", warnings);
        Assert.Contains("active_model_runtime_previous_preserved=False", warnings);
    }

    private static void AssertEvaluateResponseSelectedActiveCachedRuntime(
        string bodyJson,
        DesignConversionPromotedLocalModel model)
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
        var metadata = root.GetProperty("boundaryMetadata");

        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(model.ModelId, metadata.GetProperty("local_promoted_model_id").GetString());
        Assert.Contains(DesignConversionActivatedModelRuntimeEvaluator.CachedRuntimeReusedEvidence, evidenceRefs);
        Assert.DoesNotContain(
            DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
            evidenceRefs);
        Assert.Contains("active_model_runtime_fresh_load_attempted=False", warnings);
        Assert.Contains("active_model_runtime_fresh_load_succeeded=False", warnings);
        Assert.Contains("active_model_runtime_previous_preserved=True", warnings);
    }

    private static void AssertEvaluateResponseSelectedActivePreservedRuntime(
        string bodyJson,
        DesignConversionPromotedLocalModel model)
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
        var metadata = root.GetProperty("boundaryMetadata");

        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(model.ModelId, metadata.GetProperty("local_promoted_model_id").GetString());
        Assert.Contains(DesignConversionActivatedModelRuntimeEvaluator.PreviousRuntimePreservedEvidence, evidenceRefs);
        Assert.DoesNotContain(
            DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
            evidenceRefs);
        Assert.Contains("active_model_runtime_fresh_load_attempted=True", warnings);
        Assert.Contains("active_model_runtime_fresh_load_succeeded=False", warnings);
        Assert.Contains("active_model_runtime_previous_preserved=True", warnings);
    }

    private static void AssertNoPrivateSurfaceLeak(
        string responseSurface,
        string root,
        string artifactRoot,
        string checkpointPath)
    {
        foreach (var forbidden in new[]
                 {
                     TenantId,
                     RouteAdmissionToken,
                     RouteAdmissionPreviousToken,
                     LocalToken,
                     PrivateMarker,
                     root,
                     root.Replace('\\', '/'),
                     artifactRoot,
                     artifactRoot.Replace('\\', '/'),
                     checkpointPath,
                     checkpointPath.Replace('\\', '/'),
                     "repository-promotion-active-checkpoint-cache",
                     "design-conversion-machine-checkpoint.json"
                 })
        {
            Assert.DoesNotContain(forbidden, responseSurface, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string ExpectedRepositoryPromotionCachePath(string artifactRoot, string checkpointSha256)
        => Path.GetFullPath(Path.Combine(
            artifactRoot,
            "repository-promotion-active-checkpoint-cache",
            "design-conversion-active-checkpoints",
            checkpointSha256[..2],
            checkpointSha256,
            "design-conversion-machine-checkpoint.json"));

    private static string NewRoot()
        => Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-repository-host-runtime",
            Guid.NewGuid().ToString("N"));

    private static string ArtifactRoot(string root)
        => Path.Combine(root, "artifact-root");

    private static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string Sha256Text(string value)
        => Sha256Hex(StrictUtf8.GetBytes(value));

    private static string EnsureTrailingDirectorySeparator(string value)
        => value.EndsWith(Path.DirectorySeparatorChar) || value.EndsWith(Path.AltDirectorySeparatorChar)
            ? value
            : value + Path.DirectorySeparatorChar;

    private static string UnitAlias(string unitId)
        => string.Equals(unitId, DesignConversionRuntimeScaffoldCriticUnit.UnitId, StringComparison.Ordinal)
            ? "critic"
            : string.Equals(unitId, DesignConversionRuntimeScaffoldImproverUnit.UnitId, StringComparison.Ordinal)
                ? "improver"
                : unitId;

    private static void DeleteDirectoryIfExists(string path)
    {
        if (!Directory.Exists(path))
            return;

        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed record RepositoryLoadCall(
        string UnitId,
        string? ServerBaseUrl,
        string LocalFallbackPath);

    private sealed class RecordingModelRepository(
        List<string> calls,
        byte[] checkpointBytes) : IModelRepository
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok("sha256:" + new string('a', 64));

        public List<RepositoryLoadCall> LoadCalls { get; } = [];

        public List<GeneratedCapabilityCheckpointManifest> UploadCalls { get; } = [];

        public Task<OperationResult<Stream>> LoadWeightsAsync(
            string unitId,
            string? serverBaseUrl,
            string localFallbackPath,
            CancellationToken ct = default)
        {
            calls.Add($"load:{UnitAlias(unitId)}");
            LoadCalls.Add(new RepositoryLoadCall(unitId, serverBaseUrl, Path.GetFullPath(localFallbackPath)));
            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<Stream>.Fail("cancelled", "CANCELLED"));

            Stream stream = new MemoryStream(checkpointBytes, writable: false);
            return Task.FromResult(OperationResult<Stream>.Ok(stream));
        }

        public Task<OperationResult<RemoteUnitManifest>> UploadAsync(
            GeneratedCapabilityCheckpointManifest manifest,
            string serverBaseUrl,
            CancellationToken ct = default)
        {
            UploadCalls.Add(manifest);
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail(
                "Repository host runtime evaluate test does not upload models.",
                "REPOSITORY_HOST_RUNTIME_UPLOAD_UNEXPECTED"));
        }
    }

    private sealed class FirstLoadBlockingFailingModelRepository(byte[] checkpointBytes) : IModelRepository
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok("sha256:" + new string('b', 64));

        private readonly TaskCompletionSource _firstLoadEntered =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _releaseFirstLoadFailure =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _loadCallCount;

        public Task FirstLoadEntered => _firstLoadEntered.Task;

        public void ReleaseFirstLoadFailure() => _releaseFirstLoadFailure.TrySetResult();

        public async Task<OperationResult<Stream>> LoadWeightsAsync(
            string unitId,
            string? serverBaseUrl,
            string localFallbackPath,
            CancellationToken ct = default)
        {
            var callNumber = Interlocked.Increment(ref _loadCallCount);
            if (callNumber == 1)
            {
                _firstLoadEntered.TrySetResult();
                await _releaseFirstLoadFailure.Task.WaitAsync(ct);
                return OperationResult<Stream>.Fail(
                    "First concurrent repository load failed after known-good runtime became available.",
                    "REPOSITORY_HOST_RUNTIME_FIRST_CONCURRENT_LOAD_FAILED");
            }

            Stream stream = new MemoryStream(checkpointBytes, writable: false);
            return OperationResult<Stream>.Ok(stream);
        }

        public Task<OperationResult<RemoteUnitManifest>> UploadAsync(
            GeneratedCapabilityCheckpointManifest manifest,
            string serverBaseUrl,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<RemoteUnitManifest>.Fail(
                "Concurrent repository preservation test does not upload models.",
                "REPOSITORY_HOST_RUNTIME_UPLOAD_UNEXPECTED"));
    }

    private sealed class StaticRemoteUnitLocator(
        IReadOnlyDictionary<string, RemoteUnitManifest> manifests) : IRemoteUnitLocator
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok("sha256:" + new string('c', 64));

        public Task<OperationResult<RemoteUnitManifest>> LocateAsync(
            string unitId,
            CancellationToken ct = default)
            => Task.FromResult(manifests.TryGetValue(unitId, out var manifest)
                ? OperationResult<RemoteUnitManifest>.Ok(manifest)
                : OperationResult<RemoteUnitManifest>.Fail(
                    "Concurrent repository preservation manifest not found.",
                    "REPOSITORY_HOST_RUNTIME_MANIFEST_NOT_FOUND"));

        public Task<OperationResult<bool>> RegisterAsync(
            RemoteUnitManifest manifest,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<bool>.Fail(
                "Concurrent repository preservation test does not register manifests.",
                "REPOSITORY_HOST_RUNTIME_REGISTER_UNEXPECTED"));
    }

    private sealed class RecordingRemoteUnitLocator(
        List<string> calls,
        Dictionary<string, RemoteUnitManifest> manifests) : IRemoteUnitLocator
    {
        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok("sha256:" + new string('d', 64));

        public List<string> LocateCalls { get; } = [];

        public List<RemoteUnitManifest> ReturnedManifests { get; } = [];

        public List<RemoteUnitManifest> RegisterCalls { get; } = [];

        public Task<OperationResult<RemoteUnitManifest>> LocateAsync(
            string unitId,
            CancellationToken ct = default)
        {
            calls.Add($"locate:{UnitAlias(unitId)}");
            LocateCalls.Add(unitId);
            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("cancelled", "CANCELLED"));

            if (!manifests.TryGetValue(unitId, out var manifest))
            {
                return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail(
                    "Repository host runtime test manifest not found.",
                    "REPOSITORY_HOST_RUNTIME_MANIFEST_NOT_FOUND"));
            }

            ReturnedManifests.Add(manifest);
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Ok(manifest));
        }

        public Task<OperationResult<bool>> RegisterAsync(
            RemoteUnitManifest manifest,
            CancellationToken ct = default)
        {
            RegisterCalls.Add(manifest);
            return Task.FromResult(OperationResult<bool>.Fail(
                "Repository host runtime evaluate test does not register manifests.",
                "REPOSITORY_HOST_RUNTIME_REGISTER_UNEXPECTED"));
        }
    }

    private sealed class UnexpectedCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "Repository host runtime test does not exercise the improvement-loop candidate path.",
                "REPOSITORY_HOST_RUNTIME_UNEXPECTED_CANDIDATE_REQUEST"));
    }
}
