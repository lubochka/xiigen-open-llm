// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelServer.Contracts;
using ModelServer.DesignConversion;
using TransformerModel;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class DesignConversionModelServerTrainingIntakeTests : IAsyncLifetime
{
    private readonly HashSet<string> _ownedTempPaths = new(
        OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal);
    private static readonly Uri PromotionUploadEndpoint = new("https://promotion.example/v1/design-conversion/promotions");
    private const string RepositoryPromotionServerBaseUrl = "https://models.example/repository/releases/stable";
    private static readonly DateTimeOffset PromotionUploadedAtUtc = DateTimeOffset.Parse("2026-07-14T08:30:00Z");
    private static readonly DateTimeOffset RepositoryPromotionRegisteredAtUtc = DateTimeOffset.Parse("2026-07-27T08:30:00Z");
    private const string PromotionHostTenantId = "tenant-alpha";
    private const string PromotionHostBearerToken = "promotion-host-bearer-token-123456";

    [Theory]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionRun)]
    [InlineData("train")]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal)]
    [InlineData("promote")]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus)]
    [InlineData("continue-training")]
    public async Task ProductionHandler_TrainingActionWithoutConfiguredThresholds_Fails503BeforeArtifactDbSideEffect(
        string action)
    {
        var root = CreateOwnedTempPath("training-threshold-config-required");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var handler = DesignConversionModelServerTrainingIntakeHandler.CreateForProductionHost(
            new DesignConversionDpoUsefulnessApi(db),
            measurementGateThresholdConfiguration: null);
        var before = SnapshotFiles(root);

        var response = await handler.HandleAsync(CreateProductionThresholdRequest(
            action,
            BuildTrainingIntakeActualRouteOptions(
                "server-thresholds-not-configured",
                "server-thresholds-not-configured-dataset",
                "server-thresholds-not-configured-gate",
                sourceDisjoint: true),
            root));

        Assert.Equal(503, response.StatusCode);
        Assert.Equal(
            "TRAINING_INTAKE_MEASUREMENT_GATE_THRESHOLDS_NOT_CONFIGURED",
            ReadResponseErrorCode(response));
        Assert.Equal(before, SnapshotFiles(root));
    }

    [Theory]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionRun)]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal)]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus)]
    public async Task ProductionHandler_ConfiguredThresholdsRequireCallerMeasurementGateRequest(
        string action)
    {
        var root = CreateOwnedTempPath("training-measurement-request-required");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var handler = DesignConversionModelServerTrainingIntakeHandler.CreateForProductionHost(
            new DesignConversionDpoUsefulnessApi(db),
            ProductionMeasurementGateThresholdConfiguration());
        var before = SnapshotFiles(root);

        var response = await handler.HandleAsync(CreateProductionThresholdRequest(
            action,
            new DesignConversionTrainingPipelineOptions(
                RunId: "server-thresholds-request-required",
                BatchSize: 1),
            root));

        Assert.Equal(400, response.StatusCode);
        Assert.Equal(
            "TRAINING_INTAKE_MEASUREMENT_GATE_REQUEST_REQUIRED",
            ReadResponseErrorCode(response));
        Assert.Equal(before, SnapshotFiles(root));
    }

    [Theory]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionRun)]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal)]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus)]
    public async Task ProductionHandler_ConfiguredThresholdsRejectFailingCallerMeasurementBeforeArtifactDbMutation(
        string action)
    {
        const string privateMarker = "PRIVATE_TRAINING_FAILING_CALLER_MEASUREMENT_SECRET";
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            dpoPolicyBaseProvider: null);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:failing-caller-measurement",
            "failing caller admission measurement must not mutate training intake state",
            DateTimeOffset.UnixEpoch.AddMinutes(649));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var handler = DesignConversionModelServerTrainingIntakeHandler.CreateForProductionHost(
            api,
            ProductionMeasurementGateThresholdConfiguration());
        var before = SnapshotMaterialFiles(seeded.RootDirectory);
        var promotionOutputDirectory = Path.Combine(
            seeded.RootDirectory,
            "training-failing-caller-measurement-output");

        var response = await handler.HandleAsync(CreateTrainingIntakeHttpRequest(
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: action,
                IntakeId: intake.Value!.IntakeId,
                TenantId: $"tenant:failing-caller-measurement:{action}",
                IdempotencyKey: $"idempotency:failing-caller-measurement:{action}",
                RequestedByRef: "architect:failing-caller-measurement",
                Reason: "caller measurement must pass admission before local mutation",
                PromotionOutputDirectory: promotionOutputDirectory,
                Options: new DesignConversionTrainingPipelineOptions(
                    RunId: $"failing-caller-measurement-{action}",
                    BatchSize: 25,
                    DatasetOptions: new DesignConversionDpoDatasetOptions(
                        DatasetId: $"failing-caller-measurement-{action}",
                        MinimumTrainRecords: 50,
                        MinimumHeldOutRecords: 50,
                        Preflight:
                            DesignConversionDpoDatasetPreflight
                                .LocalTrainingIntakeSourceManifestDisjointProof()),
                    MeasurementGateRequest:
                        BuildFailingProductionCallerMeasurementGate(
                            $"failing-caller-measurement-{action}")))));

        Assert.Equal(400, response.StatusCode);
        Assert.Equal(
            "DESIGN_CONVERSION_CALLER_ADMISSION_MEASUREMENT_NOT_PASSED",
            ReadResponseErrorCode(response));
        Assert.DoesNotContain(privateMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.Equal(before, SnapshotMaterialFiles(seeded.RootDirectory));
        Assert.False(Directory.Exists(promotionOutputDirectory));
    }

    [Theory]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionRun)]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal)]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus)]
    public async Task ProductionHandler_InvalidInjectedThresholdConfiguration_Fails503BeforeDomainOrArtifactDbSideEffect(
        string action)
    {
        var root = CreateOwnedTempPath("training-invalid-threshold-config");
        var db = new NoCallLocalArtifactDb();
        var invalidRuntime = DesignConversionMeasurementAdmission.MaxReportedRuntimeMs + 1;
        var invalidCost = DesignConversionMeasurementAdmission.MaxReportedCostUsd + 0.0001m;
        var invalidRam = DesignConversionMeasurementAdmission.MaxReportedPeakRamMb + 1;
        var handler = DesignConversionModelServerTrainingIntakeHandler.CreateForProductionHost(
            new DesignConversionDpoUsefulnessApi(db),
            new DesignConversionModelServerMeasurementGateThresholdConfiguration(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: invalidRuntime,
                MaxCostUsd: invalidCost,
                MaxPeakRamMb: invalidRam));
        var before = SnapshotFiles(root);

        var response = await handler.HandleAsync(CreateProductionThresholdRequest(
            action,
            BuildTrainingIntakeActualRouteOptions(
                "server-thresholds-invalid",
                "server-thresholds-invalid-dataset",
                "server-thresholds-invalid-gate",
                sourceDisjoint: true),
            root));

        Assert.Equal(503, response.StatusCode);
        Assert.Equal(
            "TRAINING_INTAKE_MEASUREMENT_GATE_THRESHOLDS_INVALID",
            ReadResponseErrorCode(response));
        Assert.NotEqual(
            "DESIGN_CONVERSION_CALLER_ADMISSION_MEASUREMENT_INVALID",
            ReadResponseErrorCode(response));
        Assert.Equal(before, SnapshotFiles(root));
        Assert.Equal(0, db.CallCount);
        var visible = response.BodyJson;
        Assert.DoesNotContain(invalidRuntime.ToString(CultureInfo.InvariantCulture), visible, StringComparison.Ordinal);
        Assert.DoesNotContain(invalidCost.ToString(CultureInfo.InvariantCulture), visible, StringComparison.Ordinal);
        Assert.DoesNotContain(invalidRam.ToString(CultureInfo.InvariantCulture), visible, StringComparison.Ordinal);
        Assert.DoesNotContain(root, visible, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LegacyPublicHandlerConstructor_DoesNotEnableProductionThresholdEnforcement()
    {
        var root = CreateOwnedTempPath("legacy-training-handler");
        var handler = new DesignConversionModelServerTrainingIntakeHandler(
            new DesignConversionDpoUsefulnessApi(new JsonlDesignConversionLocalArtifactDb(root)));

        var response = await handler.HandleAsync(CreateProductionThresholdRequest(
            DesignConversionModelServerTrainingIntakeHandler.ActionRun,
            options: null,
            root));
        var errorCode = ReadResponseErrorCode(response);

        Assert.NotEqual("TRAINING_INTAKE_MEASUREMENT_GATE_THRESHOLDS_NOT_CONFIGURED", errorCode);
        Assert.NotEqual("TRAINING_INTAKE_MEASUREMENT_GATE_REQUEST_REQUIRED", errorCode);
    }

    [Theory]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionList, 200, "")]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionCreate, 400, "TRAINING_INTAKE_HANDOFF_REQUIRED")]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionPreflight, 400, "TRAINING_INTAKE_ID_REQUIRED")]
    [InlineData(DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion, 400, "TRAINING_INTAKE_LOCAL_PROMOTION_ROLLBACK_REF_REQUIRED")]
    public async Task ProductionHandler_NonTrainingActionsRemainIndependentOfThresholdConfiguration(
        string action,
        int expectedStatus,
        string expectedErrorCode)
    {
        var root = CreateOwnedTempPath("nontraining-threshold-independence");
        var handler = DesignConversionModelServerTrainingIntakeHandler.CreateForProductionHost(
            new DesignConversionDpoUsefulnessApi(new JsonlDesignConversionLocalArtifactDb(root)),
            measurementGateThresholdConfiguration: null);
        var routeRequest = new DesignConversionModelServerTrainingIntakeRequest(
            Action: action,
            TenantId: "tenant:threshold-independent",
            IdempotencyKey: $"idempotency:threshold-independent:{action}",
            RequestedByRef: "architect:threshold-independent",
            Reason: "threshold configuration must not affect non-training actions");

        var response = await handler.HandleAsync(CreateTrainingIntakeHttpRequest(routeRequest));

        Assert.Equal(expectedStatus, response.StatusCode);
        if (expectedStatus != 200)
            Assert.Equal(expectedErrorCode, ReadResponseErrorCode(response));
    }

    [Theory]
    [InlineData("quality", false)]
    [InlineData("quality", true)]
    [InlineData("runtime", false)]
    [InlineData("runtime", true)]
    [InlineData("cost", false)]
    [InlineData("cost", true)]
    [InlineData("ram", false)]
    [InlineData("ram", true)]
    public void ApplyServerOwnedMeasurementGateThresholds_ReplacesEveryAxisRegardlessOfCallerStrictness(
        string axis,
        bool callerIsStricter)
    {
        var configuration = new DesignConversionModelServerMeasurementGateThresholdConfiguration(
            RequiredHeldOutQualityScore: 0.87,
            MaxRuntimeMs: 1_234,
            MaxCostUsd: 5.6789m,
            MaxPeakRamMb: 999);
        var callerThresholds = axis switch
        {
            "quality" => new DesignConversionMeasurementGateThresholds(
                callerIsStricter ? 0.99 : 0.10,
                1_234,
                5.6789m,
                999),
            "runtime" => new DesignConversionMeasurementGateThresholds(
                0.87,
                callerIsStricter ? 1 : 99_999,
                5.6789m,
                999),
            "cost" => new DesignConversionMeasurementGateThresholds(
                0.87,
                1_234,
                callerIsStricter ? 0m : 999m,
                999),
            "ram" => new DesignConversionMeasurementGateThresholds(
                0.87,
                1_234,
                5.6789m,
                callerIsStricter ? 1 : 99_999),
            _ => throw new InvalidOperationException(axis)
        };
        var measurementRequest = BuildPassingLifecycleMeasurementGate(
            "threshold-replacement-gate") with
        {
            Thresholds = callerThresholds
        };
        var options = new DesignConversionTrainingPipelineOptions(
            "threshold-replacement-run",
            25,
            MeasurementGateRequest: measurementRequest);

        var effective = DesignConversionModelServerTrainingIntakeHandler
            .ApplyServerOwnedMeasurementGateThresholds(options, configuration);

        Assert.NotSame(options, effective);
        Assert.NotSame(measurementRequest, effective.MeasurementGateRequest);
        Assert.Equal(options.RunId, effective.RunId);
        Assert.Equal(options.BatchSize, effective.BatchSize);
        Assert.Same(measurementRequest.HeldOutMetrics, effective.MeasurementGateRequest!.HeldOutMetrics);
        Assert.Same(measurementRequest.IntegrityProof, effective.MeasurementGateRequest.IntegrityProof);
        Assert.Equal(configuration.ToDomainThresholds(), effective.MeasurementGateRequest.Thresholds);
        Assert.Same(callerThresholds, measurementRequest.Thresholds);
    }

    [Fact]
    public void ApplyServerOwnedMeasurementGateThresholds_ReplacesNullCallerThresholdObject()
    {
        var configuration = ProductionMeasurementGateThresholdConfiguration();
        var request = BuildPassingLifecycleMeasurementGate() with { Thresholds = null! };
        var options = new DesignConversionTrainingPipelineOptions(
            "null-caller-thresholds",
            1,
            MeasurementGateRequest: request);

        var effective = DesignConversionModelServerTrainingIntakeHandler
            .ApplyServerOwnedMeasurementGateThresholds(options, configuration);

        Assert.Equal(configuration.ToDomainThresholds(), effective.MeasurementGateRequest!.Thresholds);
        Assert.Null(request.Thresholds);
    }

    [Fact]
    public void TrainingCapableStartupOverloads_AppendNullableMeasurementGateConfigurationLast()
    {
        var startupMethods = typeof(DesignConversionModelServerAspNetHostStartup)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(method =>
                (string.Equals(
                     method.Name,
                     nameof(DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry),
                     StringComparison.Ordinal)
                 || string.Equals(
                     method.Name,
                     nameof(DesignConversionModelServerAspNetHostStartup.ConfigureDesignConversionModelServer),
                     StringComparison.Ordinal))
                && method.GetParameters().Any(parameter =>
                    parameter.ParameterType == typeof(IDesignConversionLocalArtifactDb)
                    || parameter.ParameterType == typeof(DesignConversionModelServerPromotionHostOptions)))
            .ToArray();

        var actualInventory = startupMethods
            .Select(method =>
            {
                var parameters = method.GetParameters();
                var surface = parameters[0].ParameterType == typeof(WebApplication)
                    ? "web-application"
                    : parameters[0].ParameterType == typeof(IEndpointRouteBuilder)
                        ? "endpoint-route-builder"
                        : "registry";
                var authority = parameters.Any(parameter =>
                    parameter.ParameterType == typeof(IDesignConversionLocalArtifactDb))
                    ? "artifact-db"
                    : "promotion-host";
                var preflight = parameters.Any(parameter =>
                    parameter.ParameterType == typeof(
                        DesignConversionTrainingIntakePreflightCoordinatorOptions));
                return $"{method.Name}|{surface}|{authority}|preflight:{preflight}";
            })
            .OrderBy(signature => signature, StringComparer.Ordinal)
            .ToArray();
        var expectedInventory = new[]
        {
            "ConfigureDesignConversionModelServer|endpoint-route-builder|artifact-db|preflight:False",
            "ConfigureDesignConversionModelServer|endpoint-route-builder|artifact-db|preflight:True",
            "ConfigureDesignConversionModelServer|endpoint-route-builder|promotion-host|preflight:False",
            "ConfigureDesignConversionModelServer|endpoint-route-builder|promotion-host|preflight:True",
            "ConfigureDesignConversionModelServer|web-application|artifact-db|preflight:False",
            "ConfigureDesignConversionModelServer|web-application|artifact-db|preflight:True",
            "ConfigureDesignConversionModelServer|web-application|promotion-host|preflight:False",
            "ConfigureDesignConversionModelServer|web-application|promotion-host|preflight:True",
            "CreateLocalRouteRegistry|registry|artifact-db|preflight:False",
            "CreateLocalRouteRegistry|registry|artifact-db|preflight:True",
            "CreateLocalRouteRegistry|registry|promotion-host|preflight:False",
            "CreateLocalRouteRegistry|registry|promotion-host|preflight:True"
        }.OrderBy(signature => signature, StringComparer.Ordinal).ToArray();

        Assert.Equal(expectedInventory, actualInventory);
        Assert.DoesNotContain(
            typeof(DesignConversionModelServerAspNetHostStartup)
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static),
            method => method.Name.Contains("ExternalVisualProvider", StringComparison.Ordinal));
        Assert.All(startupMethods, method =>
        {
            var last = method.GetParameters().Last();
            Assert.Equal("measurementGateThresholdConfiguration", last.Name);
            Assert.Equal(
                typeof(DesignConversionModelServerMeasurementGateThresholdConfiguration),
                last.ParameterType);
            Assert.True(last.HasDefaultValue);
            Assert.Null(last.DefaultValue);
        });
    }

    [Fact]
    public async Task ProductionStartupRegistry_PropagatesConfiguredThresholdsThroughActualTrainingRoute()
    {
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            "PRIVATE_STARTUP_THRESHOLD_PROPAGATION",
            dpoPolicyBaseProvider: null);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:startup-threshold-propagation",
            "prove startup-owned measurement thresholds override caller values",
            DateTimeOffset.UnixEpoch.AddMinutes(650));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);

        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting(),
            new UnexpectedCandidateStrategy(),
            seeded.Db,
            measurementGateThresholdConfiguration:
                new DesignConversionModelServerMeasurementGateThresholdConfiguration(
                    RequiredHeldOutQualityScore: 0.99,
                    MaxRuntimeMs: DesignConversionMeasurementAdmission.MaxReportedRuntimeMs,
                    MaxCostUsd: DesignConversionMeasurementAdmission.MaxReportedCostUsd,
                    MaxPeakRamMb: DesignConversionMeasurementAdmission.MaxReportedPeakRamMb));
        var callerGate = BuildPassingProductionCallerMeasurementGate(
            "startup-threshold-propagation") with
        {
            Thresholds = new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0d,
                MaxRuntimeMs: DesignConversionMeasurementAdmission.MaxReportedRuntimeMs,
                MaxCostUsd: DesignConversionMeasurementAdmission.MaxReportedCostUsd,
                MaxPeakRamMb: DesignConversionMeasurementAdmission.MaxReportedPeakRamMb)
        };

        var response = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler.ActionRun,
                IntakeId: intake.Value!.IntakeId,
                TenantId: "tenant:startup-threshold-propagation",
                IdempotencyKey: "idempotency:startup-threshold-propagation",
                Options: new DesignConversionTrainingPipelineOptions(
                    RunId: "startup-threshold-propagation",
                    BatchSize: 25,
                    DatasetOptions: new DesignConversionDpoDatasetOptions(
                        DatasetId: "startup-threshold-propagation",
                        MinimumTrainRecords: 50,
                        MinimumHeldOutRecords: 50,
                        Preflight:
                            DesignConversionDpoDatasetPreflight
                                .LocalTrainingIntakeSourceManifestDisjointProof()),
                    MeasurementGateRequest: callerGate)));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.BodyJson);
        Assert.Equal(
            DesignConversionMeasurementGateStatus.Failed,
            document.RootElement
                .GetProperty("trainingRun")
                .GetProperty("measurementGateStatus")
                .GetString());
        Assert.Equal(0d, callerGate.Thresholds.RequiredHeldOutQualityScore);
        AssertNoServerOwnedMeasurementGateThresholdPayload(document.RootElement);
    }

    [Fact]
    public async Task ProductionStartupRegistry_PropagatesRepositoryOwnedTrainingOptionsBeforeTrainingRouteMutation()
    {
        var artifactRoot = CreateOwnedTempPath(
            "startup-repository-owned-artifacts");
        var repositoryRoot = CreateOwnedTempPath(
            "startup-repository-owned-context");
        CreateRepositoryOwnedTrainingSentinels(repositoryRoot);
        var sealedDataset = CreateSealedRepositoryOwnedDpoDataset(
            repositoryRoot,
            "startup-propagation");
        var pair = Assert.Single(sealedDataset.Manifest.OrderedPairs);
        File.Delete(Path.Combine(
            repositoryRoot,
            pair.RepositoryRelativePairFilePath));
        var registry = DesignConversionModelServerAspNetHostStartup
            .CreateLocalRouteRegistry(
                DesignConversionLocalServiceComposition
                    .CreateDeterministicLocalForLiveHostTesting(),
                new UnexpectedCandidateStrategy(),
                new DesignConversionModelServerPromotionHostOptions(
                    artifactRoot,
                    RepositoryOwnedTrainingOptions:
                    new DesignConversionModelServerRepositoryOwnedTrainingOptions(
                        CreateRepositoryOwnedTrainingRunContextSpec(
                            repositoryRoot,
                            "startup-propagation"))),
                measurementGateThresholdConfiguration:
                ProductionMeasurementGateThresholdConfiguration());
        var beforeRepositoryFiles = SnapshotFiles(repositoryRoot);
        var beforeArtifactFiles = SnapshotFiles(artifactRoot);

        var response = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler
                    .ActionRun,
                IntakeId: "startup-repository-owned-intake",
                TenantId: "tenant:startup-repository-owned",
                IdempotencyKey:
                    "idempotency:startup-repository-owned",
                Options: BuildTrainingIntakeActualRouteOptions(
                    "startup-repository-owned",
                    "startup-repository-owned-dataset",
                    "startup-repository-owned-gate",
                    sourceDisjoint: true)));

        Assert.Equal(409, response.StatusCode);
        Assert.Equal(
            DesignConversionRepositoryOwnedTrainingRunContextFactory
                .DpoDatasetUnavailableErrorCode,
            ReadResponseErrorCode(response));
        Assert.False(
            Directory.Exists(RepositoryOwnedModelArtifactRoot(repositoryRoot)));
        Assert.True(File.Exists(sealedDataset.Address.ManifestPath));
        Assert.Equal(beforeRepositoryFiles, SnapshotFiles(repositoryRoot));
        Assert.Equal(beforeArtifactFiles, SnapshotFiles(artifactRoot));
        AssertNoPublicPathLeak(response.BodyJson, repositoryRoot);
        AssertNoPublicPathLeak(response.BodyJson, artifactRoot);
    }

    [Fact]
    public async Task ProgramLocalLoopbackRegistry_ServerEnvRepositoryOwnedTrainingFailsClosedBeforeArtifactDbRunForMissingSealedDpo()
    {
        var artifactDbRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-artifact-db");
        var artifactDb = RunOptionsLocalArtifactDbSpy.Create(
            fallback: new JsonlDesignConversionLocalArtifactDb(artifactDbRoot));
        var localPngArtifactRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-png");
        var repositoryRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-context");
        CreateRepositoryOwnedTrainingSentinels(repositoryRoot);
        var serverEnvironment =
            BuildRepositoryOwnedTrainingEnvironment(
                repositoryRoot,
                "program-loopback");
        var serverOptions = ModelServer.Program
            .LocalLoopbackRepositoryOwnedTrainingConfiguration
            .Resolve(name => serverEnvironment.TryGetValue(name, out var value)
                ? value
                : null,
                trustedRepositoryRoot: repositoryRoot);
        Assert.NotNull(serverOptions.RunContextSpec);

        using var registry = ModelServer.Program
            .CreateLocalLoopbackRouteRegistryWithLocalPngArtifactRoot(
                localPngArtifactRoot,
                artifactDb.Db,
                measurementGateThresholdConfiguration:
                    ProductionMeasurementGateThresholdConfiguration(),
                repositoryOwnedTrainingOptions: serverOptions,
                trustedRepositoryRoot: repositoryRoot);

        var response = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler
                    .ActionRun,
                IntakeId:
                    "program-loopback-missing-intake-proves-repository-gate-before-db",
                TenantId: "tenant:program-loopback-repository-owned",
                IdempotencyKey:
                    "idempotency:program-loopback-repository-owned",
                Options: BuildTrainingIntakeActualRouteOptions(
                    "program-loopback-repository-owned",
                    "program-loopback-repository-owned-dataset",
                    "program-loopback-repository-owned-gate",
                    sourceDisjoint: true)));

        Assert.Equal(409, response.StatusCode);
        Assert.Equal(
            DesignConversionRepositoryOwnedTrainingRunContextFactory
                .DpoDatasetUnavailableErrorCode,
            ReadResponseErrorCode(response));
        Assert.Equal(0, artifactDb.RunTrainingIntakeCallCount);
        Assert.Null(artifactDb.LastRunOptions);
        Assert.False(
            Directory.Exists(
                RepositoryOwnedModelArtifactRoot(repositoryRoot)));
        AssertNoPublicPathLeak(response.BodyJson, repositoryRoot);
        AssertNoPublicPathLeak(response.BodyJson, artifactDbRoot);

        var listResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler
                    .ActionList,
                TenantId:
                    "tenant:program-loopback-repository-owned-list",
                IdempotencyKey:
                    "idempotency:program-loopback-repository-owned-list"));

        Assert.Equal(200, listResponse.StatusCode);
        Assert.DoesNotContain(
            ModelServer.Program
                .RepositoryOwnedTrainingRepositoryRootEnvironmentName,
            listResponse.BodyJson,
            StringComparison.Ordinal);
        AssertNoPublicPathLeak(listResponse.BodyJson, repositoryRoot);
        AssertNoPublicPathLeak(listResponse.BodyJson, artifactDbRoot);
    }

    [Fact]
    public void ProgramLocalLoopbackRepositoryOwnedResolver_UsesTrustedRootWithoutRootEnvironmentAuthority()
    {
        var trustedRepositoryRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-trusted-root");
        CreateRepositoryOwnedTrainingSentinels(trustedRepositoryRoot);
        var serverEnvironment = new Dictionary<string, string>(
            BuildRepositoryOwnedTrainingEnvironment(
                trustedRepositoryRoot,
                "trusted-root-without-env-root"),
            StringComparer.Ordinal);
        Assert.True(serverEnvironment.Remove(
            ModelServer.Program
                .RepositoryOwnedTrainingRepositoryRootEnvironmentName));

        var serverOptions = ModelServer.Program
            .LocalLoopbackRepositoryOwnedTrainingConfiguration
            .Resolve(name => serverEnvironment.TryGetValue(name, out var value)
                ? value
                : null,
                trustedRepositoryRoot);

        Assert.NotNull(serverOptions.RunContextSpec);
        Assert.Equal(
            Path.GetFullPath(trustedRepositoryRoot),
            serverOptions.RunContextSpec!.RepositoryRoot);
    }

    [Fact]
    public void ProgramLocalLoopbackRepositoryOwnedResolver_RejectsForeignRootAssertionEvenWithSentinels()
    {
        var trustedRepositoryRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-trusted-root-rejects-foreign");
        var foreignRepositoryRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-foreign-root");
        CreateRepositoryOwnedTrainingSentinels(trustedRepositoryRoot);
        CreateRepositoryOwnedTrainingSentinels(foreignRepositoryRoot);
        var serverEnvironment = BuildRepositoryOwnedTrainingEnvironment(
            foreignRepositoryRoot,
            "foreign-root-assertion");

        var serverOptions = ModelServer.Program
            .LocalLoopbackRepositoryOwnedTrainingConfiguration
            .Resolve(name => serverEnvironment.TryGetValue(name, out var value)
                ? value
                : null,
                trustedRepositoryRoot);

        Assert.Null(serverOptions.RunContextSpec);
        Assert.Equal(
            DesignConversionModelServerRepositoryOwnedTrainingOptions
                .ServerConfigurationInvalidErrorCode,
            serverOptions.ConfigurationErrorCode);
    }

    [Theory]
    [InlineData(null, DesignConversionModelServerRepositoryOwnedTrainingOptions.ServerConfigurationRequiredErrorCode)]
    [InlineData("false", DesignConversionModelServerRepositoryOwnedTrainingOptions.ServerConfigurationDisabledIsNotBypassErrorCode)]
    public async Task ProgramLocalLoopbackRegistry_MissingOrFalseRepositoryOwnedConfigFailsClosedBeforeTrainingDbAndKeepsListRoute(
        string? enabledValue,
        string expectedErrorCode)
    {
        var artifactDbRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-disabled-artifact-db");
        var artifactDb = RunOptionsLocalArtifactDbSpy.Create(
            fallback: new JsonlDesignConversionLocalArtifactDb(artifactDbRoot));
        var localPngArtifactRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-disabled-png");
        var trustedRepositoryRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-disabled-trusted-root");
        CreateRepositoryOwnedTrainingSentinels(trustedRepositoryRoot);
        var serverEnvironment = new Dictionary<string, string>(
            StringComparer.Ordinal);
        if (enabledValue is not null)
        {
            serverEnvironment[
                ModelServer.Program
                    .RepositoryOwnedTrainingEnabledEnvironmentName] =
                enabledValue;
        }

        var serverOptions = ModelServer.Program
            .LocalLoopbackRepositoryOwnedTrainingConfiguration
            .Resolve(name => serverEnvironment.TryGetValue(name, out var value)
                ? value
                : null,
                trustedRepositoryRoot);
        Assert.Null(serverOptions.RunContextSpec);
        Assert.Equal(expectedErrorCode, serverOptions.ConfigurationErrorCode);
        var beforeRepositoryFiles = SnapshotFiles(trustedRepositoryRoot);
        var beforeArtifactFiles = SnapshotFiles(artifactDbRoot);

        using var registry = ModelServer.Program
            .CreateLocalLoopbackRouteRegistryWithLocalPngArtifactRoot(
                localPngArtifactRoot,
                artifactDb.Db,
                measurementGateThresholdConfiguration:
                    ProductionMeasurementGateThresholdConfiguration(),
                repositoryOwnedTrainingOptions: serverOptions,
                trustedRepositoryRoot: trustedRepositoryRoot);

        var response = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler
                    .ActionRun,
                IntakeId:
                    "program-loopback-disabled-config-proves-no-legacy-run",
                TenantId:
                    "tenant:program-loopback-disabled-config",
                IdempotencyKey:
                    "idempotency:program-loopback-disabled-config",
                Options: BuildTrainingIntakeActualRouteOptions(
                    "program-loopback-disabled-config",
                    "program-loopback-disabled-config-dataset",
                    "program-loopback-disabled-config-gate",
                    sourceDisjoint: true)));

        Assert.Equal(503, response.StatusCode);
        Assert.Equal(expectedErrorCode, ReadResponseErrorCode(response));
        Assert.Equal(0, artifactDb.RunTrainingIntakeCallCount);
        Assert.Null(artifactDb.LastRunOptions);
        Assert.False(
            Directory.Exists(
                RepositoryOwnedModelArtifactRoot(trustedRepositoryRoot)));
        Assert.Equal(beforeRepositoryFiles, SnapshotFiles(trustedRepositoryRoot));
        Assert.Equal(beforeArtifactFiles, SnapshotFiles(artifactDbRoot));
        AssertNoPublicPathLeak(response.BodyJson, trustedRepositoryRoot);
        AssertNoPublicPathLeak(response.BodyJson, artifactDbRoot);

        var listResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler
                    .ActionList,
                TenantId:
                    "tenant:program-loopback-disabled-config-list",
                IdempotencyKey:
                    "idempotency:program-loopback-disabled-config-list"));

        Assert.Equal(200, listResponse.StatusCode);
        Assert.Equal(0, artifactDb.RunTrainingIntakeCallCount);
        AssertNoPublicPathLeak(listResponse.BodyJson, trustedRepositoryRoot);
        AssertNoPublicPathLeak(listResponse.BodyJson, artifactDbRoot);
    }

    [Fact]
    public async Task ProgramLocalLoopbackRegistry_NullRepositoryOwnedOptionsFailsClosedBeforeTrainingDb()
    {
        var artifactDbRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-null-artifact-db");
        var artifactDb = RunOptionsLocalArtifactDbSpy.Create(
            fallback: new JsonlDesignConversionLocalArtifactDb(artifactDbRoot));
        var localPngArtifactRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-null-png");

        using var registry = ModelServer.Program
            .CreateLocalLoopbackRouteRegistryWithLocalPngArtifactRoot(
                localPngArtifactRoot,
                artifactDb.Db,
                measurementGateThresholdConfiguration:
                    ProductionMeasurementGateThresholdConfiguration());

        var response = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler
                    .ActionRun,
                IntakeId:
                    "program-loopback-null-options-proves-no-legacy-run",
                TenantId:
                    "tenant:program-loopback-null-options",
                IdempotencyKey:
                    "idempotency:program-loopback-null-options",
                Options: BuildTrainingIntakeActualRouteOptions(
                    "program-loopback-null-options",
                    "program-loopback-null-options-dataset",
                    "program-loopback-null-options-gate",
                    sourceDisjoint: true)));

        Assert.Equal(503, response.StatusCode);
        Assert.Equal(
            DesignConversionModelServerRepositoryOwnedTrainingOptions
                .ServerConfigurationRequiredErrorCode,
            ReadResponseErrorCode(response));
        Assert.Equal(0, artifactDb.RunTrainingIntakeCallCount);
        Assert.Null(artifactDb.LastRunOptions);
        Assert.Empty(SnapshotFiles(artifactDbRoot));
    }

    [Fact]
    public async Task ProgramLocalLoopbackRegistry_ExplicitForeignRepositoryOwnedOptionsFailClosedBeforeTrainingDb()
    {
        var artifactDbRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-explicit-foreign-artifact-db");
        var artifactDb = RunOptionsLocalArtifactDbSpy.Create(
            fallback: new JsonlDesignConversionLocalArtifactDb(artifactDbRoot));
        var localPngArtifactRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-explicit-foreign-png");
        var trustedRepositoryRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-explicit-trusted-root");
        var foreignRepositoryRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-explicit-foreign-root");
        CreateRepositoryOwnedTrainingSentinels(trustedRepositoryRoot);
        CreateRepositoryOwnedTrainingSentinels(foreignRepositoryRoot);
        var foreignOptions =
            new DesignConversionModelServerRepositoryOwnedTrainingOptions(
                CreateRepositoryOwnedTrainingRunContextSpec(
                    foreignRepositoryRoot,
                    "explicit-foreign-root"));

        using var registry = ModelServer.Program
            .CreateLocalLoopbackRouteRegistryWithLocalPngArtifactRoot(
                localPngArtifactRoot,
                artifactDb.Db,
                measurementGateThresholdConfiguration:
                    ProductionMeasurementGateThresholdConfiguration(),
                repositoryOwnedTrainingOptions: foreignOptions,
                trustedRepositoryRoot: trustedRepositoryRoot);

        var response = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler
                    .ActionRun,
                IntakeId:
                    "program-loopback-explicit-foreign-root-no-db",
                TenantId:
                    "tenant:program-loopback-explicit-foreign-root",
                IdempotencyKey:
                    "idempotency:program-loopback-explicit-foreign-root",
                Options: BuildTrainingIntakeActualRouteOptions(
                    "program-loopback-explicit-foreign-root",
                    "program-loopback-explicit-foreign-root-dataset",
                    "program-loopback-explicit-foreign-root-gate",
                    sourceDisjoint: true)));

        Assert.Equal(503, response.StatusCode);
        Assert.Equal(
            DesignConversionModelServerRepositoryOwnedTrainingOptions
                .ServerConfigurationInvalidErrorCode,
            ReadResponseErrorCode(response));
        Assert.Equal(0, artifactDb.RunTrainingIntakeCallCount);
        Assert.Null(artifactDb.LastRunOptions);
        Assert.False(
            Directory.Exists(
                RepositoryOwnedModelArtifactRoot(trustedRepositoryRoot)));
        Assert.False(
            Directory.Exists(
                RepositoryOwnedModelArtifactRoot(foreignRepositoryRoot)));
        AssertNoPublicPathLeak(response.BodyJson, trustedRepositoryRoot);
        AssertNoPublicPathLeak(response.BodyJson, foreignRepositoryRoot);
        AssertNoPublicPathLeak(response.BodyJson, artifactDbRoot);
    }

    [Fact]
    public async Task ProgramLocalLoopbackRegistry_ExplicitTrustedRepositoryOwnedOptionsReachArtifactDbWithServerContexts()
    {
        var artifactDb = RunOptionsLocalArtifactDbSpy.Create();
        var localPngArtifactRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-explicit-trusted-png");
        var trustedRepositoryRoot = CreateOwnedTempPath(
            "program-loopback-repository-owned-explicit-trusted-pass-root");
        CreateRepositoryOwnedTrainingSentinels(trustedRepositoryRoot);
        var sealedDataset = CreateSealedRepositoryOwnedDpoDataset(
            trustedRepositoryRoot,
            "explicit-trusted-root");
        var trustedOptions =
            new DesignConversionModelServerRepositoryOwnedTrainingOptions(
                CreateRepositoryOwnedTrainingRunContextSpec(
                    trustedRepositoryRoot,
                    "explicit-trusted-root"));

        using var registry = ModelServer.Program
            .CreateLocalLoopbackRouteRegistryWithLocalPngArtifactRoot(
                localPngArtifactRoot,
                artifactDb.Db,
                measurementGateThresholdConfiguration:
                    ProductionMeasurementGateThresholdConfiguration(),
                repositoryOwnedTrainingOptions: trustedOptions,
                trustedRepositoryRoot: trustedRepositoryRoot);

        var response = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler
                    .ActionRun,
                IntakeId:
                    "program-loopback-explicit-trusted-root-reaches-db",
                TenantId:
                    "tenant:program-loopback-explicit-trusted-root",
                IdempotencyKey:
                    "idempotency:program-loopback-explicit-trusted-root",
                Options: BuildTrainingIntakeActualRouteOptions(
                    "program-loopback-explicit-trusted-root",
                    "program-loopback-explicit-trusted-root-dataset",
                    "program-loopback-explicit-trusted-root-gate",
                    sourceDisjoint: true)));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(1, artifactDb.RunTrainingIntakeCallCount);
        Assert.NotNull(artifactDb.LastRunOptions);
        AssertRepositoryOwnedDpoContextMatches(
            sealedDataset,
            artifactDb.LastRunOptions!.RepositoryOwnedDpoTransactionContext);
        AssertRepositoryOwnedModelContextMatches(
            trustedRepositoryRoot,
            "explicit-trusted-root",
            artifactDb.LastRunOptions.RepositoryOwnedModelCheckpointContext);
        Assert.False(
            Directory.Exists(
                RepositoryOwnedModelArtifactRoot(trustedRepositoryRoot)));
    }

    [Fact]
    public async Task ProductionHandler_ServerOwnedThresholdConfiguration_ReplacesCallerThresholdsBeforeRunCall()
    {
        var artifactDb = RunOptionsLocalArtifactDbSpy.Create();
        var configuration = new DesignConversionModelServerMeasurementGateThresholdConfiguration(
            RequiredHeldOutQualityScore: 0.99,
            MaxRuntimeMs: DesignConversionMeasurementAdmission.MaxReportedRuntimeMs,
            MaxCostUsd: DesignConversionMeasurementAdmission.MaxReportedCostUsd,
            MaxPeakRamMb: DesignConversionMeasurementAdmission.MaxReportedPeakRamMb);
        var root = CreateOwnedTempPath("training-threshold-handler-wiring");
        var handler = DesignConversionModelServerTrainingIntakeHandler.CreateForProductionHost(
            new DesignConversionDpoUsefulnessApi(artifactDb.Db),
            configuration);
        var callerThresholds = new DesignConversionMeasurementGateThresholds(
            RequiredHeldOutQualityScore: 0d,
            MaxRuntimeMs: 1,
            MaxCostUsd: 0m,
            MaxPeakRamMb: 1);
        var callerGate = BuildPassingLifecycleMeasurementGate(
            "startup-threshold-wiring") with
        {
            Thresholds = callerThresholds
        };
        var callerOptions = new DesignConversionTrainingPipelineOptions(
            RunId: "startup-threshold-wiring",
            BatchSize: 25,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "startup-threshold-wiring",
                MinimumTrainRecords: 50,
                MinimumHeldOutRecords: 50,
                Preflight:
                    DesignConversionDpoDatasetPreflight
                        .LocalTrainingIntakeSourceManifestDisjointProof()),
            MeasurementGateRequest: callerGate);

        var response = await handler.HandleAsync(CreateProductionThresholdRequest(
            DesignConversionModelServerTrainingIntakeHandler.ActionRun,
            callerOptions,
            root));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(1, artifactDb.RunTrainingIntakeCallCount);
        Assert.Equal("training-intake-server-threshold-test", artifactDb.LastRunIntakeId);
        Assert.NotNull(artifactDb.LastRunOptions);
        var capturedOptions = artifactDb.LastRunOptions!;
        Assert.Equal("startup-threshold-wiring", capturedOptions.RunId);
        Assert.NotNull(capturedOptions.MeasurementGateRequest);
        Assert.NotSame(callerGate, capturedOptions.MeasurementGateRequest);
        Assert.Equal(configuration.ToDomainThresholds(), capturedOptions.MeasurementGateRequest!.Thresholds);
        Assert.Same(callerThresholds, callerGate.Thresholds);
        Assert.Same(callerGate, callerOptions.MeasurementGateRequest);
        using var document = JsonDocument.Parse(response.BodyJson);
        AssertNoServerOwnedMeasurementGateThresholdPayload(document.RootElement);
    }

    [Fact]
    public async Task ProductionHandler_ServerOwnedRepositoryTrainingContext_AddsContextsBeforeRunCall()
    {
        var artifactDb = RunOptionsLocalArtifactDbSpy.Create();
        var repositoryRoot = CreateOwnedTempPath(
            "training-repository-context-handler");
        CreateRepositoryOwnedTrainingSentinels(repositoryRoot);
        var sealedDataset = CreateSealedRepositoryOwnedDpoDataset(
            repositoryRoot,
            "handler-context");
        var serverOptions = new DesignConversionModelServerRepositoryOwnedTrainingOptions(
            CreateRepositoryOwnedTrainingRunContextSpec(
                repositoryRoot,
                "handler-context"));
        var handler = DesignConversionModelServerTrainingIntakeHandler
            .CreateForProductionHost(
                new DesignConversionDpoUsefulnessApi(artifactDb.Db),
                ProductionMeasurementGateThresholdConfiguration(),
                repositoryOwnedTrainingOptions: serverOptions);

        var response = await handler.HandleAsync(CreateProductionThresholdRequest(
            DesignConversionModelServerTrainingIntakeHandler.ActionRun,
            BuildTrainingIntakeActualRouteOptions(
                "repository-context-handler",
                "repository-context-handler-dataset",
                "repository-context-handler-gate",
                sourceDisjoint: true),
            repositoryRoot));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(1, artifactDb.RunTrainingIntakeCallCount);
        Assert.NotNull(artifactDb.LastRunOptions);
        var capturedOptions = artifactDb.LastRunOptions!;
        AssertRepositoryOwnedDpoContextMatches(
            sealedDataset,
            capturedOptions.RepositoryOwnedDpoTransactionContext);
        AssertRepositoryOwnedModelContextMatches(
            repositoryRoot,
            "handler-context",
            capturedOptions.RepositoryOwnedModelCheckpointContext);
        Assert.False(
            Directory.Exists(RepositoryOwnedModelArtifactRoot(repositoryRoot)));
    }

    [Fact]
    public async Task ProductionHandler_ClientRepositoryOwnedContextCannotSubstituteServerContext()
    {
        var serverRepositoryRoot = CreateOwnedTempPath(
            "training-repository-context-server-owned");
        var callerRepositoryRoot = CreateOwnedTempPath(
            "training-repository-context-caller-owned");
        CreateRepositoryOwnedTrainingSentinels(serverRepositoryRoot);
        var serverDataset = CreateSealedRepositoryOwnedDpoDataset(
            serverRepositoryRoot,
            "server-owned");
        var callerOptions = BuildTrainingIntakeActualRouteOptions(
            "repository-context-caller-substitution",
            "repository-context-caller-substitution-dataset",
            "repository-context-caller-substitution-gate",
            sourceDisjoint: true) with
            {
                // Caller-owned repository contexts are server-only slots. These
                // parseable but invalid contexts prove the handler rejects or
                // overwrites caller input before any artifact DB training call.
                RepositoryOwnedDpoTransactionContext =
                    new DesignConversionRepositoryOwnedDpoTransactionContext(
                        Enabled: true,
                        Address: null,
                        RequestIdentity: null),
                RepositoryOwnedModelCheckpointContext =
                    new DesignConversionRepositoryOwnedModelCheckpointContext(
                        Enabled: true,
                        Request: null)
            };

        var rejectingDb = RunOptionsLocalArtifactDbSpy.Create();
        var rejectingHandler = DesignConversionModelServerTrainingIntakeHandler
            .CreateForProductionHost(
                new DesignConversionDpoUsefulnessApi(rejectingDb.Db),
                ProductionMeasurementGateThresholdConfiguration());

        var rejected = await rejectingHandler.HandleAsync(
            CreateProductionThresholdRequest(
                DesignConversionModelServerTrainingIntakeHandler.ActionRun,
                callerOptions,
                callerRepositoryRoot));

        Assert.Equal(400, rejected.StatusCode);
        Assert.Equal(
            "TRAINING_INTAKE_REPOSITORY_OWNED_CONTEXT_REQUIRES_SERVER_CONFIGURATION",
            ReadResponseErrorCode(rejected));
        Assert.Equal(0, rejectingDb.RunTrainingIntakeCallCount);

        var serverDb = RunOptionsLocalArtifactDbSpy.Create();
        var serverHandler = DesignConversionModelServerTrainingIntakeHandler
            .CreateForProductionHost(
                new DesignConversionDpoUsefulnessApi(serverDb.Db),
                ProductionMeasurementGateThresholdConfiguration(),
                repositoryOwnedTrainingOptions:
                new DesignConversionModelServerRepositoryOwnedTrainingOptions(
                    CreateRepositoryOwnedTrainingRunContextSpec(
                        serverRepositoryRoot,
                        "server-owned")));

        var accepted = await serverHandler.HandleAsync(
            CreateProductionThresholdRequest(
                DesignConversionModelServerTrainingIntakeHandler.ActionRun,
                callerOptions,
                serverRepositoryRoot));

        Assert.Equal(200, accepted.StatusCode);
        Assert.Equal(1, serverDb.RunTrainingIntakeCallCount);
        Assert.NotNull(serverDb.LastRunOptions);
        AssertRepositoryOwnedDpoContextMatches(
            serverDataset,
            serverDb.LastRunOptions!.RepositoryOwnedDpoTransactionContext);
        AssertRepositoryOwnedModelContextMatches(
            serverRepositoryRoot,
            "server-owned",
            serverDb.LastRunOptions.RepositoryOwnedModelCheckpointContext);
        Assert.NotSame(
            callerOptions.RepositoryOwnedDpoTransactionContext,
            serverDb.LastRunOptions.RepositoryOwnedDpoTransactionContext);
        Assert.NotSame(
            callerOptions.RepositoryOwnedModelCheckpointContext,
            serverDb.LastRunOptions.RepositoryOwnedModelCheckpointContext);
        Assert.NotEqual(
            callerRepositoryRoot,
            serverDb.LastRunOptions
                .RepositoryOwnedModelCheckpointContext!
                .Request!
                .RepositoryRoot,
            StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProductionHandler_IncompleteServerOwnedDpoFailsBeforeRunCallAndModelDirectory()
    {
        var artifactDb = RunOptionsLocalArtifactDbSpy.Create();
        var repositoryRoot = CreateOwnedTempPath(
            "training-repository-context-incomplete-dpo");
        CreateRepositoryOwnedTrainingSentinels(repositoryRoot);
        var sealedDataset = CreateSealedRepositoryOwnedDpoDataset(
            repositoryRoot,
            "incomplete-dpo");
        var pair = Assert.Single(sealedDataset.Manifest.OrderedPairs);
        File.Delete(Path.Combine(
            repositoryRoot,
            pair.RepositoryRelativePairFilePath));
        var before = SnapshotFiles(repositoryRoot);
        var handler = DesignConversionModelServerTrainingIntakeHandler
            .CreateForProductionHost(
                new DesignConversionDpoUsefulnessApi(artifactDb.Db),
                ProductionMeasurementGateThresholdConfiguration(),
                repositoryOwnedTrainingOptions:
                new DesignConversionModelServerRepositoryOwnedTrainingOptions(
                    CreateRepositoryOwnedTrainingRunContextSpec(
                        repositoryRoot,
                        "incomplete-dpo")));

        var response = await handler.HandleAsync(CreateProductionThresholdRequest(
            DesignConversionModelServerTrainingIntakeHandler.ActionRun,
            BuildTrainingIntakeActualRouteOptions(
                "repository-context-incomplete-dpo",
                "repository-context-incomplete-dpo-dataset",
                "repository-context-incomplete-dpo-gate",
                sourceDisjoint: true),
            repositoryRoot));

        Assert.Equal(409, response.StatusCode);
        Assert.Equal(
            DesignConversionRepositoryOwnedTrainingRunContextFactory
                .DpoDatasetUnavailableErrorCode,
            ReadResponseErrorCode(response));
        Assert.Equal(0, artifactDb.RunTrainingIntakeCallCount);
        Assert.False(
            Directory.Exists(RepositoryOwnedModelArtifactRoot(repositoryRoot)));
        Assert.True(File.Exists(sealedDataset.Address.ManifestPath));
        Assert.Equal(before, SnapshotFiles(repositoryRoot));
    }

    [Fact]
    public async Task ProductionHandler_PublicBoundaryMetadataHashesLocalPathShapesOnSuccessAndError()
    {
        var artifactDb = RunOptionsLocalArtifactDbSpy.Create();
        var handler = DesignConversionModelServerTrainingIntakeHandler.CreateForProductionHost(
            new DesignConversionDpoUsefulnessApi(artifactDb.Db),
            ProductionMeasurementGateThresholdConfiguration());
        var root = CreateOwnedTempPath("training-public-path-sanitizer");
        var options = new DesignConversionTrainingPipelineOptions(
            RunId: "public-path-sanitizer",
            BatchSize: 25,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "public-path-sanitizer",
                MinimumTrainRecords: 50,
                MinimumHeldOutRecords: 50,
                Preflight:
                    DesignConversionDpoDatasetPreflight
                        .LocalTrainingIntakeSourceManifestDisjointProof()),
            MeasurementGateRequest:
                BuildPassingProductionCallerMeasurementGate(
                    "public-path-sanitizer",
                    sourceDisjoint: true));

        var success = await handler.HandleAsync(CreateProductionThresholdRequest(
            DesignConversionModelServerTrainingIntakeHandler.ActionRun,
            options,
            root));

        Assert.Equal(200, success.StatusCode);
        AssertNoPrivateTrainingIntakePayload(success.BodyJson, "PUBLIC_PATH_SANITIZER_MARKER");
        AssertPublicTrainingIntakePayloadDoesNotLeakLocalPathShapes(success.BodyJson);
        using (var successDocument = JsonDocument.Parse(success.BodyJson))
        {
            var metadata = successDocument.RootElement
                .GetProperty("trainingRun")
                .GetProperty("boundaryMetadata");
            Assert.StartsWith(
                "public_sanitizer_posix_tmp_path:sha256:",
                metadata.GetProperty("public_sanitizer_posix_tmp_path").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "public_sanitizer_linux_root_path:sha256:",
                metadata.GetProperty("public_sanitizer_linux_root_path").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "public_sanitizer_linux_home_path:sha256:",
                metadata.GetProperty("public_sanitizer_linux_home_path").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "public_sanitizer_escaped_posix_path:sha256:",
                metadata.GetProperty("public_sanitizer_escaped_posix_path").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "public_sanitizer_windows_path:sha256:",
                metadata.GetProperty("public_sanitizer_windows_path").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "public_sanitizer_unc_path:sha256:",
                metadata.GetProperty("public_sanitizer_unc_path").GetString(),
                StringComparison.Ordinal);
            Assert.StartsWith(
                "public_sanitizer_file_uri:sha256:",
                metadata.GetProperty("public_sanitizer_file_uri").GetString(),
                StringComparison.Ordinal);
            Assert.Equal(
                "source_manifest_disjoint",
                metadata.GetProperty("public_sanitizer_safe_label").GetString());
            Assert.Equal(
                PublicSanitizerCanonicalSha256Ref,
                metadata.GetProperty("public_sanitizer_canonical_sha256").GetString());
        }

        var error = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: "/tmp/PUBLIC_PATH_SANITIZER_MARKER/route",
            BodyJson: JsonSerializer.Serialize(
                new DesignConversionModelServerTrainingIntakeRequest(
                    Action: @"\/home\/user\/PUBLIC_PATH_SANITIZER_MARKER\/unsupported"),
                DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json"));

        Assert.Equal(404, error.StatusCode);
        AssertPublicTrainingIntakePayloadDoesNotLeakLocalPathShapes(error.BodyJson);
        Assert.StartsWith("http-path:sha256:", error.Headers["x-design-conversion-path"], StringComparison.Ordinal);
        using var errorDocument = JsonDocument.Parse(error.BodyJson);
        var errorMetadata = errorDocument.RootElement.GetProperty("boundaryMetadata");
        Assert.StartsWith("http-path:sha256:", errorMetadata.GetProperty("http_path").GetString(), StringComparison.Ordinal);
        Assert.StartsWith(
            "training-intake-error-message:sha256:",
            errorDocument.RootElement.GetProperty("errorMessage").GetString(),
            StringComparison.Ordinal);

        var unsupportedAction = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerTrainingIntakeHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(
                new DesignConversionModelServerTrainingIntakeRequest(
                    Action: @"\/home\/user\/PUBLIC_PATH_SANITIZER_MARKER\/unsupported"),
                DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json"));

        Assert.Equal(400, unsupportedAction.StatusCode);
        AssertPublicTrainingIntakePayloadDoesNotLeakLocalPathShapes(unsupportedAction.BodyJson);
        using var unsupportedActionDocument = JsonDocument.Parse(unsupportedAction.BodyJson);
        Assert.Contains(
            "training-intake-action:sha256:",
            unsupportedActionDocument.RootElement.GetProperty("errorMessage").GetString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void ServerOwnedMeasurementThresholdNonDisclosure_AllowsStatusTimestampAndUnrelatedNumbers()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "measurementGateStatus": "Failed",
              "status": "visible",
              "generatedAtUtc": "2026-07-30T10:59:10.9931234Z",
              "unrelatedScore": 0.99,
              "thresholds": {
                "warningCount": 0.99
              }
            }
            """);

        AssertNoServerOwnedMeasurementGateThresholdPayload(document.RootElement);
    }

    [Theory]
    [InlineData("""{"requiredHeldOutQualityScore":0.99}""")]
    [InlineData("""{"required_held_out_quality_score":0.99}""")]
    [InlineData("""{"maxRuntimeMs":9223372036854775807}""")]
    [InlineData("""{"max_runtime_ms":9223372036854775807}""")]
    [InlineData("""{"maxCostUsd":79228162514264337593543950335}""")]
    [InlineData("""{"max_cost_usd":79228162514264337593543950335}""")]
    [InlineData("""{"maxPeakRamMb":9223372036854775807}""")]
    [InlineData("""{"max_peak_ram_mb":9223372036854775807}""")]
    [InlineData("""{"measurementGateThresholds":{}}""")]
    [InlineData("""{"measurement_gate_thresholds":{}}""")]
    [InlineData(
        """
        {
          "thresholds": {
            "quality": 0.99,
            "runtimeMs": 9223372036854775807,
            "costUsd": 79228162514264337593543950335,
            "ramMb": 9223372036854775807
          }
        }
        """)]
    public void ServerOwnedMeasurementThresholdNonDisclosure_RejectsPrivateThresholdShape(
        string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);

        var exception = Assert.Throws<Xunit.Sdk.XunitException>(
            () => AssertNoServerOwnedMeasurementGateThresholdPayload(document.RootElement));

        Assert.DoesNotContain("0.99", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("9223372036854775807", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("79228162514264337593543950335", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ServerOwnedMeasurementThresholdNonDisclosure_FailureMessageDoesNotEchoPrivateValue()
    {
        using var document = JsonDocument.Parse(
            """{"wrapper":{"maxCostUsd":"C:\\private\\secret-token"}}""");

        var exception = Assert.Throws<Xunit.Sdk.XunitException>(
            () => AssertNoServerOwnedMeasurementGateThresholdPayload(document.RootElement));

        Assert.DoesNotContain("C:\\", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProductionStartupRegistryPaths_PropagateConfiguredThresholdsToTrainingHandler()
    {
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            "PRIVATE_STARTUP_THRESHOLD_PROPAGATION_ALL_PATHS",
            dpoPolicyBaseProvider: null);
        var root = seeded.RootDirectory;
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
        var strategy = new UnexpectedCandidateStrategy();
        var dpoPolicyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        using var legalTeacherAssociation =
            AssociateStartupIssuedLegalTeacherCapabilityForPromotionHost(
                dpoPolicyBaseProvider);
        var configuration = new DesignConversionModelServerMeasurementGateThresholdConfiguration(
            RequiredHeldOutQualityScore: 0.99,
            MaxRuntimeMs: DesignConversionMeasurementAdmission.MaxReportedRuntimeMs,
            MaxCostUsd: DesignConversionMeasurementAdmission.MaxReportedCostUsd,
            MaxPeakRamMb: DesignConversionMeasurementAdmission.MaxReportedPeakRamMb);
        using var promotionHttpClient = new HttpClient(
            new CapturingPromotionUploadHttpHandler(PromotionUploadResponseMode.Success),
            disposeHandler: true);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api
            .CreateTrainingIntakeAsync(
                seeded.Handoff,
                "architect:startup-threshold-propagation-all-paths",
                "prove every startup-owned measurement threshold path overrides permissive caller values",
                DateTimeOffset.UnixEpoch.AddMinutes(651));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var intakeId = intake.Value!.IntakeId;
        Assert.NotNull(seeded.TeacherRequestIds);
        var admittedTeacherRequestIds = seeded.TeacherRequestIds!
            .ToHashSet(StringComparer.Ordinal);

        async Task<string> CreateFreshIntakeAsync(
            string scenario,
            string sourceNamespace,
            int requestedMinute)
        {
            var fresh = await AppendSourceDisjointApprovedHandoffAsync(
                seeded.Db,
                $"PRIVATE_STARTUP_THRESHOLD_PROPAGATION_{scenario.ToUpperInvariant()}",
                sourceNamespace);
            AssertTeacherRequestIdsDisjoint(
                admittedTeacherRequestIds.ToArray(),
                fresh.TeacherRequestIds);
            admittedTeacherRequestIds.UnionWith(fresh.TeacherRequestIds);
            var created = await api.CreateTrainingIntakeAsync(
                fresh.Handoff,
                "architect:startup-threshold-propagation-all-paths",
                $"fresh approved intake for startup threshold path {scenario}",
                DateTimeOffset.UnixEpoch.AddMinutes(requestedMinute));
            Assert.True(created.IsSuccess, created.ErrorMessage);
            return created.Value!.IntakeId;
        }

        var localPromotionIntakeId = await CreateFreshIntakeAsync(
            "local-promotion",
            "startup-local",
            653);
        var remotePromotionIntakeId = await CreateFreshIntakeAsync(
            "remote-promotion",
            "startup-remote",
            654);
        var standard = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            strategy,
            seeded.Db,
            measurementGateThresholdConfiguration: configuration);
        await AssertRegistryUsesConfiguredMeasurementGateThresholdsAsync(
            standard,
            "tenant:startup-standard",
            intakeId,
            "standard");

        var localPromotion = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            strategy,
            new DesignConversionModelServerPromotionHostOptions(
                root,
                DpoPolicyBaseProvider: dpoPolicyBaseProvider),
            measurementGateThresholdConfiguration: configuration);
        await AssertRegistryUsesConfiguredMeasurementGateThresholdsAsync(
            localPromotion,
            "tenant:startup-local-promotion",
            localPromotionIntakeId,
            "local-promotion");

        var remotePromotion = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            strategy,
            CreatePromotionHostOptions(root, promotionHttpClient, dpoPolicyBaseProvider),
            routeAdmissionOptions: CreatePromotionRouteAdmissionOptions(PromotionHostTenantId),
            measurementGateThresholdConfiguration: configuration);
        await AssertRegistryUsesConfiguredMeasurementGateThresholdsAsync(
            remotePromotion,
            PromotionHostTenantId,
            remotePromotionIntakeId,
            "remote-promotion");

    }



    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        List<Exception>? cleanupFailures = null;
        foreach (var path in _ownedTempPaths
                     .OrderByDescending(path => path.Length)
                     .ToArray())
        {
            try
            {
                DeleteDirectoryIfExists(path);
            }
            catch (Exception exception)
            {
                cleanupFailures ??= [];
                cleanupFailures.Add(exception);
            }
        }

        if (cleanupFailures is not null)
        {
            throw new AggregateException(
                "One or more owned training-intake test paths could not be cleaned.",
                cleanupFailures);
        }

        return Task.CompletedTask;
    }

    [Fact]
    public void DeleteDirectoryIfExists_RegisteredPathRemovesLeafAndEmptyGuidParent()
    {
        var requestedPath = CreateOwnedTempPath("owned-test-artifacts");
        var ownedParent = Directory.GetParent(requestedPath)!.FullName;
        Directory.CreateDirectory(requestedPath);

        try
        {
            DeleteDirectoryIfExists(requestedPath);

            Assert.False(Directory.Exists(requestedPath));
            Assert.False(Directory.Exists(ownedParent));
            Assert.DoesNotContain(requestedPath, _ownedTempPaths);
        }
        finally
        {
            if (Directory.Exists(requestedPath))
                Directory.Delete(requestedPath, recursive: true);
            if (Directory.Exists(ownedParent))
                Directory.Delete(ownedParent, recursive: false);
        }
    }

    [Fact]
    public void DeleteDirectoryIfExists_UnregisteredTempRootRemainsIntact()
    {
        var tempRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(Path.GetTempPath()));

        DeleteDirectoryIfExists(tempRoot);

        Assert.True(Directory.Exists(tempRoot));
    }

    [Fact]
    public void DeleteDirectoryIfExists_UnownedNonGuidLeafAndSentinelRemainIntact()
    {
        var retainedParent = Path.Combine(
            Path.GetTempPath(),
            $"training-intake-cleanup-non-guid-{Guid.NewGuid():N}");
        var requestedPath = Path.Combine(retainedParent, "owned-test-artifacts");
        var sentinelPath = Path.Combine(requestedPath, "sentinel.txt");
        Directory.CreateDirectory(requestedPath);
        File.WriteAllText(sentinelPath, "must remain");

        try
        {
            DeleteDirectoryIfExists(requestedPath);

            Assert.True(Directory.Exists(requestedPath));
            Assert.True(File.Exists(sentinelPath));
            Assert.True(Directory.Exists(retainedParent));
        }
        finally
        {
            if (Directory.Exists(retainedParent))
                Directory.Delete(retainedParent, recursive: true);
        }
    }

    [Fact]
    public void DeleteDirectoryIfExists_UnownedNonemptyGuidLeafAndSiblingRemainIntact()
    {
        var retainedParent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var requestedPath = Path.Combine(retainedParent, "owned-test-artifacts");
        var siblingPath = Path.Combine(retainedParent, "sibling");
        var sentinelPath = Path.Combine(requestedPath, "sentinel.txt");
        Directory.CreateDirectory(requestedPath);
        Directory.CreateDirectory(siblingPath);
        File.WriteAllText(sentinelPath, "must remain");

        try
        {
            DeleteDirectoryIfExists(requestedPath);

            Assert.True(Directory.Exists(requestedPath));
            Assert.True(File.Exists(sentinelPath));
            Assert.True(Directory.Exists(retainedParent));
            Assert.True(Directory.Exists(siblingPath));
        }
        finally
        {
            if (Directory.Exists(retainedParent))
                Directory.Delete(retainedParent, recursive: true);
        }
    }

    [Fact]
    public void DeleteDirectoryIfExists_UnownedDeeperGuidLeafRemainsIntact()
    {
        var outerParent = Path.Combine(
            Path.GetTempPath(),
            $"training-intake-cleanup-nested-{Guid.NewGuid():N}");
        var retainedParent = Path.Combine(outerParent, Guid.NewGuid().ToString("N"));
        var requestedPath = Path.Combine(retainedParent, "owned-test-artifacts");
        var sentinelPath = Path.Combine(requestedPath, "sentinel.txt");
        Directory.CreateDirectory(requestedPath);
        File.WriteAllText(sentinelPath, "must remain");

        try
        {
            DeleteDirectoryIfExists(requestedPath);

            Assert.True(Directory.Exists(requestedPath));
            Assert.True(File.Exists(sentinelPath));
            Assert.True(Directory.Exists(retainedParent));
        }
        finally
        {
            if (Directory.Exists(outerParent))
                Directory.Delete(outerParent, recursive: true);
        }
    }

    [Fact]
    public void DeleteDirectoryIfExists_InvalidOwnedShapeFailsBeforeMutation()
    {
        var invalidParent = Path.Combine(
            Path.GetTempPath(),
            $"training-intake-cleanup-invalid-owned-{Guid.NewGuid():N}");
        var requestedPath = Path.GetFullPath(
            Path.Combine(invalidParent, "owned-test-artifacts"));
        var sentinelPath = Path.Combine(requestedPath, "sentinel.txt");
        Directory.CreateDirectory(requestedPath);
        File.WriteAllText(sentinelPath, "must remain");
        Assert.True(_ownedTempPaths.Add(requestedPath));

        try
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => DeleteDirectoryIfExists(requestedPath));

            Assert.Contains("%TEMP%/<N-format GUID>/<nonempty leaf>", exception.Message);
            Assert.True(Directory.Exists(requestedPath));
            Assert.True(File.Exists(sentinelPath));
            Assert.Contains(requestedPath, _ownedTempPaths);
        }
        finally
        {
            _ownedTempPaths.Remove(requestedPath);
            if (Directory.Exists(invalidParent))
                Directory.Delete(invalidParent, recursive: true);
        }
    }

    [Fact]
    public async Task DisposeAsync_AggregatesFailureAndStillCleansEveryOtherOwnedPath()
    {
        var invalidParent = Path.Combine(
            Path.GetTempPath(),
            $"training-intake-cleanup-dispose-invalid-{Guid.NewGuid():N}");
        var invalidPath = Path.GetFullPath(
            Path.Combine(invalidParent, "owned-test-artifacts"));
        var invalidSentinelPath = Path.Combine(invalidPath, "sentinel.txt");
        Directory.CreateDirectory(invalidPath);
        File.WriteAllText(invalidSentinelPath, "must remain");
        Assert.True(_ownedTempPaths.Add(invalidPath));

        var validPath = CreateOwnedTempPath("dispose-valid-owned-test-artifacts");
        var validParent = Directory.GetParent(validPath)!.FullName;
        Directory.CreateDirectory(validPath);

        try
        {
            var exception = await Assert.ThrowsAsync<AggregateException>(DisposeAsync);

            Assert.Single(exception.InnerExceptions);
            Assert.IsType<InvalidOperationException>(exception.InnerExceptions[0]);
            Assert.True(Directory.Exists(invalidPath));
            Assert.True(File.Exists(invalidSentinelPath));
            Assert.Contains(invalidPath, _ownedTempPaths);
            Assert.False(Directory.Exists(validPath));
            Assert.False(Directory.Exists(validParent));
            Assert.DoesNotContain(validPath, _ownedTempPaths);
        }
        finally
        {
            _ownedTempPaths.Remove(invalidPath);
            if (Directory.Exists(invalidParent))
                Directory.Delete(invalidParent, recursive: true);
            if (Directory.Exists(validPath))
                Directory.Delete(validPath, recursive: true);
            if (Directory.Exists(validParent))
                Directory.Delete(validParent, recursive: false);
        }
    }

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
        var repositoryToStringMarker = $"repository-options-private-repository-{Guid.NewGuid():N}";
        var locatorToStringMarker = $"repository-options-private-locator-{Guid.NewGuid():N}";
        var repositoryOptionsServerBaseUrl = $"https://models.example/repository/{Guid.NewGuid():N}";
        var copiedRepositoryOptionsServerBaseUrl = $"https://models.example/repository/{Guid.NewGuid():N}";
        var repositoryCalls = new List<string>();
        var repository = new RecordingTrainingIntakeModelRepository(repositoryCalls, repositoryToStringMarker);
        var locator = new RecordingTrainingIntakeRemoteUnitLocator(repositoryCalls, locatorToStringMarker);
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
        var repositoryOptions = new DesignConversionModelServerRepositoryPromotionOptions(
            repository,
            locator,
            repositoryOptionsServerBaseUrl,
            PromotionHostTenantId);
        var hostOptions = new DesignConversionModelServerPromotionHostOptions(
            root,
            promotionHttpClient,
            uploaderOptions,
            repositoryOptions);
        var copiedUploaderOptions = uploaderOptions with
        {
            AllowedArtifactRoot = copiedUploaderAllowedRoot,
            Timeout = TimeSpan.FromSeconds(6)
        };
        var copiedRepositoryOptions = repositoryOptions with
        {
            ServerBaseUrl = copiedRepositoryOptionsServerBaseUrl,
            TenantId = "tenant-beta"
        };
        var copied = hostOptions with
        {
            ArtifactRootDirectory = copiedRoot,
            PromotionHttpUploaderOptions = copiedUploaderOptions,
            RepositoryPromotionOptions = copiedRepositoryOptions
        };

        Assert.Equal(root, hostOptions.ArtifactRootDirectory);
        Assert.Equal(uploaderAllowedRoot, hostOptions.PromotionHttpUploaderOptions!.AllowedArtifactRoot);
        Assert.Same(repository, hostOptions.RepositoryPromotionOptions!.Repository);
        Assert.Same(locator, hostOptions.RepositoryPromotionOptions.Locator);
        Assert.Equal(repositoryOptionsServerBaseUrl, hostOptions.RepositoryPromotionOptions.ServerBaseUrl);
        Assert.Equal(copiedRoot, copied.ArtifactRootDirectory);
        Assert.Equal(copiedUploaderAllowedRoot, copied.PromotionHttpUploaderOptions!.AllowedArtifactRoot);
        Assert.Same(repository, copied.RepositoryPromotionOptions!.Repository);
        Assert.Same(locator, copied.RepositoryPromotionOptions.Locator);
        Assert.Equal(copiedRepositoryOptionsServerBaseUrl, copied.RepositoryPromotionOptions.ServerBaseUrl);
        Assert.Equal("tenant-beta", copied.RepositoryPromotionOptions.TenantId);
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
            Assert.Contains(nameof(DesignConversionModelServerPromotionHostOptions.ArtifactRootDirectory), surface, StringComparison.Ordinal);
            Assert.Contains(nameof(DesignConversionModelServerPromotionHostOptions.PromotionHttpClient), surface, StringComparison.Ordinal);
            Assert.Contains(nameof(DesignConversionModelServerPromotionHostOptions.PromotionHttpUploaderOptions), surface, StringComparison.Ordinal);
            Assert.Contains(nameof(DesignConversionModelServerPromotionHostOptions.RepositoryPromotionOptions), surface, StringComparison.Ordinal);
            Assert.Contains("ArtifactRootDirectory = [REDACTED]", surface, StringComparison.Ordinal);
            Assert.Contains("PromotionHttpUploaderOptions = [REDACTED]", surface, StringComparison.Ordinal);
            Assert.Contains("RepositoryPromotionOptionsConfigured = True", surface, StringComparison.Ordinal);
            Assert.Contains("TenantId = [REDACTED]", surface, StringComparison.Ordinal);
            Assert.Contains("[REDACTED]", surface, StringComparison.Ordinal);
            Assert.DoesNotContain(PromotionUploadEndpoint.AbsoluteUri, surface, StringComparison.Ordinal);
            Assert.DoesNotContain(PromotionHostTenantId, surface, StringComparison.Ordinal);
            Assert.DoesNotContain(sentinelBearer, surface, StringComparison.Ordinal);
            Assert.DoesNotContain(repositoryOptionsServerBaseUrl, surface, StringComparison.Ordinal);
            Assert.DoesNotContain(copiedRepositoryOptionsServerBaseUrl, surface, StringComparison.Ordinal);
            Assert.DoesNotContain(repositoryToStringMarker, surface, StringComparison.Ordinal);
            Assert.DoesNotContain(locatorToStringMarker, surface, StringComparison.Ordinal);
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
    public void PromotionHostOptions_ThreadExactNullableDpoProviderAcrossEveryInternalDbVariant()
    {
        const string privateSentinel = "C:/private/dpo/provider/model.bin PRIVATE_DPO_PROVIDER_SENTINEL";
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();

        var localOptions = new DesignConversionModelServerPromotionHostOptions(
            CreateOwnedTempPath("promotion-dpo-provider-local"),
            DpoPolicyBaseProvider: provider);
        var localDb = ReadTrainingIntakeArtifactDb(
            DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                new UnexpectedCandidateStrategy(),
                localOptions));
        Assert.Same(provider, ReadDpoPolicyBaseProvider(localDb));

        using var promotionHttpClient = new HttpClient(
            new CapturingPromotionUploadHttpHandler(PromotionUploadResponseMode.Success),
            disposeHandler: true);
        var httpRoot = CreateOwnedTempPath("promotion-dpo-provider-http");
        var httpOptions = CreatePromotionHostOptions(
            httpRoot,
            promotionHttpClient,
            provider);
        var httpDb = ReadTrainingIntakeArtifactDb(
            DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                new UnexpectedCandidateStrategy(),
                httpOptions));
        Assert.Same(provider, ReadDpoPolicyBaseProvider(httpDb));

        var repositoryCalls = new List<string>();
        var repositoryRoot = CreateOwnedTempPath("promotion-dpo-provider-repository");
        var repositoryOptions = CreateRepositoryPromotionHostOptions(
            repositoryRoot,
            new RecordingTrainingIntakeModelRepository(repositoryCalls),
            new RecordingTrainingIntakeRemoteUnitLocator(repositoryCalls),
            RepositoryPromotionServerBaseUrl,
            PromotionHostTenantId,
            provider);
        var repositoryDb = ReadTrainingIntakeArtifactDb(
            DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                new UnexpectedCandidateStrategy(),
                repositoryOptions));
        Assert.Same(provider, ReadDpoPolicyBaseProvider(repositoryDb));

        var unconfiguredOptions = new DesignConversionModelServerPromotionHostOptions(
            CreateOwnedTempPath("promotion-dpo-provider-unconfigured"));
        var unconfiguredDb = ReadTrainingIntakeArtifactDb(
            DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                new UnexpectedCandidateStrategy(),
                unconfiguredOptions));
        Assert.Null(ReadDpoPolicyBaseProvider(unconfiguredDb));

        var externalProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var externalDb = new JsonlDesignConversionLocalArtifactDb(
            CreateOwnedTempPath("promotion-dpo-provider-external-db"),
            dpoPolicyBaseProvider: externalProvider);
        var externalRegistry = DesignConversionModelServerAspNetHostStartup
            .CreateLocalRouteRegistry(
                composition,
                new UnexpectedCandidateStrategy(),
                (IDesignConversionLocalArtifactDb)externalDb);
        Assert.Same(externalDb, ReadTrainingIntakeArtifactDb(externalRegistry));
        Assert.Same(externalProvider, ReadDpoPolicyBaseProvider(externalDb));
        Assert.NotSame(provider, ReadDpoPolicyBaseProvider(externalDb));

        Assert.DoesNotContain(
            typeof(IDesignConversionDpoPolicyBaseProvider),
            typeof(DesignConversionModelServerAspNetHostStartup)
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(method => method.GetParameters().Any(parameter =>
                    parameter.ParameterType == typeof(IDesignConversionLocalArtifactDb)))
                .SelectMany(method => method.GetParameters())
                .Select(parameter => parameter.ParameterType));

        var configuredSurface = localOptions.ToString();
        Assert.Contains("DpoPolicyBaseProviderConfigured = True", configuredSurface, StringComparison.Ordinal);
        Assert.DoesNotContain(provider.GetType().FullName!, configuredSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("test_in_memory", configuredSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("test-org/non-default-policy", configuredSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("revision-v1", configuredSurface, StringComparison.Ordinal);
        Assert.DoesNotContain(privateSentinel, configuredSurface, StringComparison.Ordinal);
        Assert.Contains(
            "DpoPolicyBaseProviderConfigured = False",
            unconfiguredOptions.ToString(),
            StringComparison.Ordinal);

        var providerProperty = typeof(DesignConversionModelServerPromotionHostOptions)
            .GetProperty(nameof(DesignConversionModelServerPromotionHostOptions.DpoPolicyBaseProvider))!;
        Assert.Equal(typeof(IDesignConversionDpoPolicyBaseProvider), providerProperty.PropertyType);
        Assert.Equal(
            System.Reflection.NullabilityState.Nullable,
            new System.Reflection.NullabilityInfoContext()
                .Create(providerProperty)
                .ReadState);
        Assert.Same(provider, (localOptions with { ArtifactRootDirectory = localOptions.ArtifactRootDirectory })
            .DpoPolicyBaseProvider);
        Assert.Equal(0, provider.InvocationCount);
        Assert.Equal(0, externalProvider.InvocationCount);
        Assert.Empty(repositoryCalls);
    }

    private static JsonlDesignConversionLocalArtifactDb ReadTrainingIntakeArtifactDb(
        DesignConversionModelServerEndpointRegistry registry)
    {
        var registration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod,
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath);
        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        var handler = Assert.IsType<DesignConversionModelServerTrainingIntakeHandler>(
            registration.Value!.Handler.Target);
        var api = Assert.IsType<DesignConversionDpoUsefulnessApi>(
            typeof(DesignConversionModelServerTrainingIntakeHandler)
                .GetField(
                    "_api",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(handler));
        return Assert.IsType<JsonlDesignConversionLocalArtifactDb>(
            typeof(DesignConversionDpoUsefulnessApi)
                .GetField(
                    "_artifactDb",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(api));
    }

    private static IDesignConversionDpoPolicyBaseProvider? ReadDpoPolicyBaseProvider(
        JsonlDesignConversionLocalArtifactDb artifactDb)
        => (IDesignConversionDpoPolicyBaseProvider?)
            typeof(JsonlDesignConversionLocalArtifactDb)
                .GetField(
                    "_dpoPolicyBaseProvider",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)!
                .GetValue(artifactDb);

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
        Assert.Equal(bool.FalseString, registered.Value.BoundaryMetadata["training_intake_remote_promotion_configured"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.TrainingIntakeRouteDependentRemotePromotionConfigurationMetadataValue,
            registered.Value.BoundaryMetadata["training_intake_route_local_only_descriptor_semantics"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.TrainingIntakeRouteDependentRemotePromotionConfigurationMetadataValue,
            registered.Value.BoundaryMetadata["training_intake_remote_promotion_configured_descriptor_semantics"]);
        Assert.Equal(bool.TrueString, registered.Value.BoundaryMetadata["training_intake_remote_promotion_configuration_supported"]);
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
                MinimumHeldOutRecords: 50),
            MeasurementGateRequest:
                BuildPassingProductionCallerMeasurementGate(
                    "training-intake-route-create-run-promote-gate"));
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

        var promotionOutputDirectory = CreateOwnedTempPath(
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
    public async Task PostTrainingIntake_ApproveAndExportRuntimeCandidate_DelegatesThroughSecureRouteAndKeepsRefsOnlyHandoff()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_RUNTIME_APPROVAL_ROUTE_SECRET";
        const string rawReviewerMarker = "REVIEWER-CLEAR-MARKER-7F31A9";
        const string rawReasonMarker = "REASON-CLEAR-MARKER-8C42B0";
        const string expectedToken = "local-training-intake-runtime-approval-token";
        const string tenantId = "tenant:modelserver-training-intake-runtime-approval";
        const string approveIdempotencyKey = "idempotency:training-intake-runtime-approval";
        const string exportIdempotencyKey = "idempotency:training-intake-runtime-export";
        const string createIdempotencyKey = "idempotency:training-intake-runtime-export-create";
        const string eligibleJobId = "training-intake-runtime-approval-route-job";
        var root = CreateOwnedTempPath("modelserver-training-intake-runtime-approval-route");
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
        var toApprove = ToRuntimePendingCandidate(WithPersistedRuntimeArtifactRefs(
            BuildTrainingRecord(
                "training-intake-runtime-approval-route-approved-later",
                DesignConversionDpoUnits.CriticUnit,
                0.96),
            "training-intake-runtime-approval-route-approved-later-artifact"));
        var unapproved = ToRuntimePendingCandidate(WithPersistedRuntimeArtifactRefs(
            BuildTrainingRecord(
                "training-intake-runtime-approval-route-stays-pending",
                DesignConversionDpoUnits.ImproverUnit,
                0.95),
            "training-intake-runtime-approval-route-stays-pending-artifact"));
        await Save(db, BuildArtifactRecord(
            "training-intake-runtime-approval-route-record",
            eligibleJobId,
            "training-intake-runtime-approval-route-request",
            [toApprove, unapproved],
            privateMarker));
        var ineligibleApproved = ToRuntimeApprovedCandidate(WithPersistedRuntimeArtifactRefs(
            BuildTrainingRecord(
                "training-intake-runtime-approval-route-ineligible-approved",
                DesignConversionDpoUnits.CriticUnit,
                0.99),
            "training-intake-runtime-approval-route-ineligible-approved-artifact"));
        var ineligibleAppend = await db.AppendRecordAsync(BuildArtifactRecord(
            "training-intake-runtime-approval-route-ineligible-record",
            "training-intake-runtime-approval-route-ineligible-job",
            "training-intake-runtime-approval-route-ineligible-request",
            [ineligibleApproved],
            privateMarker));
        Assert.True(ineligibleAppend.IsSuccess, ineligibleAppend.ErrorMessage);
        var registry = CreateRegistry(db, expectedToken);
        var authHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName] = expectedToken
        };

        var approvalResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
                tenantId,
                idempotencyKey = approveIdempotencyKey,
                localArtifactRecordId = "training-intake-runtime-approval-route-record",
                candidateOrTrainingRecordId = toApprove.RecordId,
                requestedByRef = rawReviewerMarker,
                reason = rawReasonMarker,
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(214)
            },
            authHeaders);

        AssertSanitizedTrainingIntakeBody(approvalResponse, privateMarker);
        Assert.DoesNotContain(tenantId, approvalResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(approveIdempotencyKey, approvalResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawReviewerMarker, approvalResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(rawReasonMarker, approvalResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
        using var approvalDocument = JsonDocument.Parse(approvalResponse.BodyJson);
        var approvalRoot = approvalDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(
            approvalRoot,
            DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate);
        Assert.Equal(0, approvalRoot.GetProperty("intakeCount").GetInt32());
        Assert.Equal(JsonValueKind.Null, approvalRoot.GetProperty("approvedHandoff").ValueKind);
        var candidateApproval = approvalRoot.GetProperty("candidateApproval");
        Assert.Equal("training-intake-runtime-approval-route-record", candidateApproval.GetProperty("localArtifactRecordId").GetString());
        Assert.Equal(toApprove.RecordId, candidateApproval.GetProperty("trainingRecordId").GetString());
        Assert.Equal("approved", candidateApproval.GetProperty("approvalStatus").GetString());
        Assert.Equal(DesignConversionDpoUsefulnessApi.RuntimeCandidateReviewApprovedStatus, candidateApproval.GetProperty("reviewStatus").GetString());
        Assert.True(candidateApproval.GetProperty("approvedForTraining").GetBoolean());
        var candidateApprovedByRef = candidateApproval.GetProperty("approvedByRef").GetString();
        Assert.StartsWith(
            "dpo-candidate-approval-reviewer:sha256:",
            candidateApprovedByRef,
            StringComparison.Ordinal);
        Assert.StartsWith(
            "dpo-candidate-approval-reason:sha256:",
            candidateApproval.GetProperty("approvalReasonRef").GetString(),
            StringComparison.Ordinal);
        var approvalMetadata = candidateApproval.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, approvalMetadata.GetProperty("approved_for_training").GetString());
        Assert.Equal(bool.TrueString, approvalMetadata.GetProperty("product_not_ready").GetString());
        Assert.Equal(bool.FalseString, approvalMetadata.GetProperty("model_readiness_claimed").GetString());
        Assert.Equal(bool.FalseString, approvalMetadata.GetProperty("production_db_ready").GetString());
        Assert.Equal(bool.FalseString, approvalMetadata.GetProperty("trained_model_ready").GetString());
        Assert.Equal(candidateApprovedByRef, approvalMetadata.GetProperty("approved_by_ref").GetString());
        Assert.Equal(
            candidateApproval.GetProperty("approvalReasonRef").GetString(),
            approvalMetadata.GetProperty("approval_reason").GetString());

        var approvedReadback = await db.QueryApprovedTrainingCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: eligibleJobId));
        Assert.True(approvedReadback.IsSuccess, approvedReadback.ErrorMessage);
        var approvedTrainingRecord = Assert.Single(approvedReadback.Value!);
        Assert.Equal(toApprove.RecordId, approvedTrainingRecord.RecordId);
        Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus, approvedTrainingRecord.Provenance.Metadata["approval_status"]);

        var exportResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionExportApprovedRuntimeCandidates,
                tenantId,
                idempotencyKey = exportIdempotencyKey,
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(215)
            },
            authHeaders);

        AssertSanitizedTrainingIntakeBody(exportResponse, privateMarker);
        Assert.DoesNotContain(tenantId, exportResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(exportIdempotencyKey, exportResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawReviewerMarker, exportResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(rawReasonMarker, exportResponse.BodyJson, StringComparison.OrdinalIgnoreCase);
        using var exportDocument = JsonDocument.Parse(exportResponse.BodyJson);
        var exportRoot = exportDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(
            exportRoot,
            DesignConversionModelServerTrainingIntakeHandler.ActionExportApprovedRuntimeCandidates);
        Assert.Equal(JsonValueKind.Null, exportRoot.GetProperty("candidateApproval").ValueKind);
        var approvedHandoffElement = exportRoot.GetProperty("approvedHandoff");
        Assert.Equal(1, approvedHandoffElement.GetProperty("approvedCandidateCount").GetInt32());
        var exportedRow = Assert.Single(approvedHandoffElement.GetProperty("rows").EnumerateArray());
        Assert.Equal(toApprove.RecordId, exportedRow.GetProperty("trainingRecordId").GetString());
        var exportedApprovedByRef = exportedRow.GetProperty("approvedByRef").GetString();
        var exportedApprovalReasonRef = exportedRow.GetProperty("approvalReason").GetString();
        Assert.StartsWith(
            "approved-dpo-handoff-reviewer:sha256:",
            exportedApprovedByRef,
            StringComparison.Ordinal);
        Assert.StartsWith(
            "approved-dpo-handoff-reason:sha256:",
            exportedApprovalReasonRef,
            StringComparison.Ordinal);
        var exportedRowMetadata = exportedRow.GetProperty("boundaryMetadata");
        Assert.Equal(exportedApprovedByRef, exportedRowMetadata.GetProperty("approved_by_ref").GetString());
        Assert.Equal(exportedApprovalReasonRef, exportedRowMetadata.GetProperty("approval_reason").GetString());
        Assert.DoesNotContain(unapproved.RecordId, approvedHandoffElement.GetRawText(), StringComparison.Ordinal);
        Assert.DoesNotContain(ineligibleApproved.RecordId, approvedHandoffElement.GetRawText(), StringComparison.Ordinal);
        Assert.Equal(bool.TrueString, approvedHandoffElement.GetProperty("boundaryMetadata").GetProperty("training_handoff_only").GetString());
        Assert.Equal(bool.FalseString, approvedHandoffElement.GetProperty("boundaryMetadata").GetProperty("model_readiness_claimed").GetString());
        Assert.Equal(bool.FalseString, approvedHandoffElement.GetProperty("boundaryMetadata").GetProperty("trained_model_ready").GetString());
        var exportedResponse = JsonSerializer.Deserialize<DesignConversionModelServerTrainingIntakeResponse>(
            exportResponse.BodyJson,
            DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.NotNull(exportedResponse);
        Assert.NotNull(exportedResponse!.ApprovedHandoff);
        Assert.Equal(1, exportedResponse.ApprovedHandoff!.ApprovedCandidateCount);

        var createResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = exportedResponse.ApprovedHandoff,
                requestedByRef = "architect:modelserver-training-intake-runtime-export-create",
                reason = "create training intake from exported approved runtime candidate handoff",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(216)
            },
            authHeaders);

        Assert.True(createResponse.StatusCode == 200, createResponse.BodyJson);
        AssertSanitizedTrainingIntakeBody(createResponse, privateMarker);
        using var createDocument = JsonDocument.Parse(createResponse.BodyJson);
        var createRoot = createDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(createRoot, DesignConversionModelServerTrainingIntakeHandler.ActionCreate);
        Assert.Equal(1, createRoot.GetProperty("intake").GetProperty("approvedCandidateCount").GetInt32());
        Assert.Equal(1, createRoot.GetProperty("intake").GetProperty("acceptedCandidateCount").GetInt32());
        await AssertTrainingIntakeCountAsync(db, expectedCount: 1);
    }

    [Fact]
    public async Task PostTrainingIntake_PostApprovalPreflight_CreatesOneIntakeAndReplaysWithoutTrainingOrPromotion()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_POST_APPROVAL_PREFLIGHT_SECRET";
        const string expectedToken = "local-training-intake-post-approval-preflight-token";
        const string tenantId = "tenant:modelserver-training-intake-post-approval-preflight";
        const string routeIdempotencyKey = "idempotency:training-intake-post-approval-preflight";
        const string rawApprovalReviewer = "POST-APPROVAL-REVIEWER-CLEAR-3B1F";
        const string rawApprovalReason = "POST-APPROVAL-REASON-CLEAR-4C2A";
        const string rawPreflightRequester = "POST-APPROVAL-PREFLIGHT-REQUESTER-CLEAR-5D3B";
        const string rawPreflightReason = "POST-APPROVAL-PREFLIGHT-REASON-CLEAR-6E4C";
        const string recordId = "training-intake-post-approval-preflight-record";
        var root = CreateOwnedTempPath("modelserver-training-intake-post-approval-preflight-route");
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
        var pending = ToRuntimePendingCandidate(WithPersistedRuntimeArtifactRefs(
            BuildTrainingRecord(
                "training-intake-post-approval-preflight-pending",
                DesignConversionDpoUnits.CriticUnit,
                0.97),
            "training-intake-post-approval-preflight-artifact"));
        await Save(db, BuildArtifactRecord(
            recordId,
            "training-intake-post-approval-preflight-job",
            "training-intake-post-approval-preflight-request",
            [pending],
            privateMarker));
        var registry = CreateRegistry(db, expectedToken);
        var authHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName] = expectedToken
        };

        var approvalResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
                tenantId,
                idempotencyKey = "idempotency:training-intake-post-approval-preflight-approval",
                localArtifactRecordId = recordId,
                candidateOrTrainingRecordId = pending.RecordId,
                requestedByRef = rawApprovalReviewer,
                reason = rawApprovalReason,
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(220)
            },
            authHeaders);
        Assert.Equal(200, approvalResponse.StatusCode);
        AssertSanitizedTrainingIntakeBody(approvalResponse, privateMarker);
        await AssertTrainingIntakeCountAsync(db, expectedCount: 0);

        object BuildPreflightBody(DateTimeOffset requestedAtUtc) => new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionPostApprovalPreflight,
            tenantId,
            idempotencyKey = routeIdempotencyKey,
            localArtifactRecordId = recordId,
            candidateOrTrainingRecordId = pending.RecordId,
            requestedByRef = rawPreflightRequester,
            reason = rawPreflightReason,
            requestedAtUtc
        };

        var firstRequestedAt = DateTimeOffset.UnixEpoch.AddMinutes(221);
        var secondRequestedAt = DateTimeOffset.UnixEpoch.AddMinutes(229);
        var firstResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            BuildPreflightBody(firstRequestedAt),
            authHeaders);
        var secondResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            BuildPreflightBody(secondRequestedAt),
            authHeaders);

        AssertSanitizedTrainingIntakeBody(firstResponse, privateMarker);
        AssertSanitizedTrainingIntakeBody(secondResponse, privateMarker);
        foreach (var rawValue in new[]
        {
            expectedToken,
            tenantId,
            routeIdempotencyKey,
            rawApprovalReviewer,
            rawApprovalReason,
            rawPreflightRequester,
            rawPreflightReason
        })
        {
            Assert.DoesNotContain(rawValue, firstResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rawValue, secondResponse.BodyJson, StringComparison.Ordinal);
        }

        using var firstDocument = JsonDocument.Parse(firstResponse.BodyJson);
        using var secondDocument = JsonDocument.Parse(secondResponse.BodyJson);
        var firstRoot = firstDocument.RootElement;
        var secondRoot = secondDocument.RootElement;
        AssertSafeTrainingIntakeRouteRoot(firstRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPostApprovalPreflight);
        AssertSafeTrainingIntakeRouteRoot(secondRoot, DesignConversionModelServerTrainingIntakeHandler.ActionPostApprovalPreflight);
        Assert.Equal(JsonValueKind.Null, firstRoot.GetProperty("intake").ValueKind);
        Assert.Equal(JsonValueKind.Null, firstRoot.GetProperty("preflight").ValueKind);
        Assert.Equal(JsonValueKind.Null, firstRoot.GetProperty("trainingRun").ValueKind);
        Assert.Equal(JsonValueKind.Null, firstRoot.GetProperty("localPromotion").ValueKind);
        Assert.Equal(JsonValueKind.Null, firstRoot.GetProperty("lifecycleStatus").ValueKind);
        Assert.Equal(JsonValueKind.Null, firstRoot.GetProperty("candidateApproval").ValueKind);
        Assert.Equal(JsonValueKind.Null, firstRoot.GetProperty("approvedHandoff").ValueKind);

        var firstPreflight = firstRoot.GetProperty("approvedRuntimeDpoPreflight");
        var secondPreflight = secondRoot.GetProperty("approvedRuntimeDpoPreflight");
        Assert.Equal(recordId, firstPreflight.GetProperty("localArtifactRecordId").GetString());
        Assert.Equal(pending.RecordId, firstPreflight.GetProperty("trainingRecordId").GetString());
        Assert.Equal(firstPreflight.GetProperty("intakeId").GetString(), firstRoot.GetProperty("intakeId").GetString());
        Assert.Equal(firstPreflight.GetProperty("intakeId").GetString(), secondPreflight.GetProperty("intakeId").GetString());
        Assert.Equal(firstPreflight.GetProperty("preflightId").GetString(), secondPreflight.GetProperty("preflightId").GetString());
        Assert.False(firstPreflight.GetProperty("idempotencyReplayHit").GetBoolean());
        Assert.True(firstPreflight.GetProperty("idempotencyCreatedNewSideEffect").GetBoolean());
        Assert.True(secondPreflight.GetProperty("idempotencyReplayHit").GetBoolean());
        Assert.False(secondPreflight.GetProperty("idempotencyCreatedNewSideEffect").GetBoolean());
        Assert.Equal(firstRequestedAt, firstPreflight.GetProperty("persistedRequestedAtUtc").GetDateTimeOffset());
        Assert.Equal(firstRequestedAt, secondPreflight.GetProperty("persistedRequestedAtUtc").GetDateTimeOffset());
        Assert.Equal(1, firstPreflight.GetProperty("acceptedCandidateCount").GetInt32());
        Assert.Equal(1, firstPreflight.GetProperty("dpoStepCount").GetInt32());
        Assert.False(firstPreflight.GetProperty("trainingRunnerInvoked").GetBoolean());
        Assert.False(firstPreflight.GetProperty("trainingStarted").GetBoolean());
        Assert.False(firstPreflight.GetProperty("checkpointMutated").GetBoolean());
        Assert.False(firstPreflight.GetProperty("weightsProduced").GetBoolean());
        Assert.False(firstPreflight.GetProperty("modelUploadReady").GetBoolean());
        Assert.StartsWith(
            "dcm-approved-runtime-dpo-preflight-idempotency-",
            firstPreflight.GetProperty("idempotencyKeyRef").GetString(),
            StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(firstPreflight.GetProperty("measurementGateContractRef").GetString()));
        var firstPreflightEvidenceRefs = firstPreflight.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();
        Assert.Contains("approved_runtime_dpo_preflight_route_refs_only", firstPreflightEvidenceRefs);
        Assert.Contains("approved_runtime_dpo_preflight_raw_domain_evidence_refs_not_forwarded", firstPreflightEvidenceRefs);
        Assert.Contains("training_intake_idempotency_created_new_side_effect=true", firstPreflightEvidenceRefs);
        Assert.Contains("training_runner_invoked=false", firstPreflightEvidenceRefs);
        Assert.Contains("checkpoint_mutated=false", firstPreflightEvidenceRefs);
        Assert.Contains("weights_produced=false", firstPreflightEvidenceRefs);
        Assert.Contains("model_upload_ready=false", firstPreflightEvidenceRefs);
        Assert.DoesNotContain(firstPreflightEvidenceRefs, item =>
            item?.Contains("POST-APPROVAL", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(firstPreflightEvidenceRefs, item =>
            item?.Contains("PREFLIGHT-REQUESTER", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(firstPreflightEvidenceRefs, item =>
            item?.Contains("PREFLIGHT-REASON", StringComparison.Ordinal) == true);
        var secondPreflightEvidenceRefs = secondPreflight.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();
        Assert.Contains("training_intake_idempotency_replay_hit=true", secondPreflightEvidenceRefs);
        var firstPreflightWarnings = firstPreflight.GetProperty("warnings")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();
        Assert.Contains("domain_evidence_refs_and_warnings_not_forwarded", firstPreflightWarnings);
        Assert.Contains("post_approval_preflight_only", firstPreflightWarnings);
        Assert.DoesNotContain(firstPreflightWarnings, item =>
            item?.Contains("POST-APPROVAL", StringComparison.Ordinal) == true);

        var firstMetadata = firstRoot.GetProperty("boundaryMetadata");
        var secondMetadata = secondRoot.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_approved_runtime_dpo_preflight_supported").GetString());
        Assert.Equal(bool.TrueString, firstMetadata.GetProperty("approved_runtime_dpo_preflight_only").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_runner_invoked").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("checkpoint_mutated").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("weights_produced").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("model_upload_ready").GetString());
        Assert.Equal(bool.FalseString, firstMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
        Assert.Equal(bool.TrueString, firstMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
        Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
        Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());

        var intakes = await new DesignConversionDpoUsefulnessApi(db).ListTrainingIntakeRequestsAsync();
        Assert.True(intakes.IsSuccess, intakes.ErrorMessage);
        var storedIntake = Assert.Single(intakes.Value!);
        Assert.Equal(firstPreflight.GetProperty("intakeId").GetString(), storedIntake.IntakeId);
        Assert.Equal(firstRequestedAt, storedIntake.RequestedAtUtc);
        Assert.True(storedIntake.TrainingIntakeOnly);
        Assert.True(storedIntake.TrainingNotStarted);
        Assert.True(storedIntake.CheckpointNotMutated);
        Assert.True(storedIntake.WeightsNotProduced);
        Assert.True(storedIntake.ModelUploadNotReady);
    }

    [Fact]
    public async Task PostTrainingIntake_PostApprovalPreflight_FailsClosedForAuthFieldsAndUnapprovedCandidate()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_POST_APPROVAL_PREFLIGHT_AUTH_SECRET";
        const string expectedToken = "local-training-intake-post-approval-preflight-auth-token";
        const string invalidToken = "local-training-intake-post-approval-preflight-invalid-token";
        const string tenantId = "tenant:modelserver-training-intake-post-approval-preflight-auth";
        const string idempotencyKey = "idempotency:modelserver-training-intake-post-approval-preflight-auth";
        const string recordId = "training-intake-post-approval-preflight-auth-record";
        var root = CreateOwnedTempPath("modelserver-training-intake-post-approval-preflight-auth-route");
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
        var pending = ToRuntimePendingCandidate(WithPersistedRuntimeArtifactRefs(
            BuildTrainingRecord(
                "training-intake-post-approval-preflight-auth-pending",
                DesignConversionDpoUnits.CriticUnit,
                0.95),
            "training-intake-post-approval-preflight-auth-artifact"));
        await Save(db, BuildArtifactRecord(
            recordId,
            "training-intake-post-approval-preflight-auth-job",
            "training-intake-post-approval-preflight-auth-request",
            [pending],
            privateMarker));
        var registry = CreateRegistry(db, expectedToken);
        var authHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName] = expectedToken
        };
        var invalidAuthHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName] = invalidToken
        };
        var validBody = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["action"] = DesignConversionModelServerTrainingIntakeHandler.ActionPostApprovalPreflight,
            ["tenantId"] = tenantId,
            ["idempotencyKey"] = idempotencyKey,
            ["localArtifactRecordId"] = recordId,
            ["candidateOrTrainingRecordId"] = pending.RecordId,
            ["requestedByRef"] = "reviewer:modelserver-training-intake-post-approval-preflight-auth",
            ["reason"] = "post approval preflight should fail before mutation when not approved",
            ["requestedAtUtc"] = DateTimeOffset.UnixEpoch.AddMinutes(230)
        };

        var missingTokenResponse = await InvokeTrainingIntakeRouteAsync(registry, validBody);
        AssertTrainingIntakeRouteError(
            missingTokenResponse,
            "TRAINING_INTAKE_AUTH_TOKEN_REQUIRED",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionPostApprovalPreflight,
            expectedAdmissionSatisfied: false,
            expectedStatusCode: 401);
        AssertAuthAdmissionRejectedWithoutRawSecrets(
            missingTokenResponse,
            expectedToken,
            invalidToken,
            tenantId,
            idempotencyKey);
        await AssertTrainingIntakeCountAsync(db, expectedCount: 0);

        var invalidTokenResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            validBody,
            invalidAuthHeaders);
        AssertTrainingIntakeRouteError(
            invalidTokenResponse,
            "TRAINING_INTAKE_AUTH_TOKEN_INVALID",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionPostApprovalPreflight,
            expectedAdmissionSatisfied: false,
            expectedStatusCode: 401);
        await AssertTrainingIntakeCountAsync(db, expectedCount: 0);

        async Task AssertRejectedFieldAsync(
            string key,
            object? value,
            string expectedErrorCode)
        {
            var body = new Dictionary<string, object?>(validBody, StringComparer.Ordinal)
            {
                [key] = value
            };
            var response = await InvokeTrainingIntakeRouteAsync(registry, body, authHeaders);
            AssertTrainingIntakeRouteError(
                response,
                expectedErrorCode,
                privateMarker,
                DesignConversionModelServerTrainingIntakeHandler.ActionPostApprovalPreflight,
                expectedAdmissionSatisfied: true);
            await AssertTrainingIntakeCountAsync(db, expectedCount: 0);
        }

        await AssertRejectedFieldAsync("localArtifactRecordId", null, "TRAINING_INTAKE_LOCAL_ARTIFACT_RECORD_ID_REQUIRED");
        await AssertRejectedFieldAsync("localArtifactRecordId", $" {recordId}", "TRAINING_INTAKE_LOCAL_ARTIFACT_RECORD_ID_REQUIRED");
        await AssertRejectedFieldAsync("candidateOrTrainingRecordId", null, "TRAINING_INTAKE_DPO_CANDIDATE_ID_REQUIRED");
        await AssertRejectedFieldAsync("candidateOrTrainingRecordId", $"{pending.RecordId} ", "TRAINING_INTAKE_DPO_CANDIDATE_ID_REQUIRED");
        await AssertRejectedFieldAsync("requestedByRef", null, "TRAINING_INTAKE_APPROVED_RUNTIME_DPO_PREFLIGHT_REQUESTER_REQUIRED");
        await AssertRejectedFieldAsync("requestedByRef", " reviewer:modelserver-training-intake-post-approval-preflight-auth", "TRAINING_INTAKE_APPROVED_RUNTIME_DPO_PREFLIGHT_REQUESTER_REQUIRED");
        await AssertRejectedFieldAsync("reason", null, "TRAINING_INTAKE_APPROVED_RUNTIME_DPO_PREFLIGHT_REASON_REQUIRED");
        await AssertRejectedFieldAsync("reason", " padded post approval preflight reason ", "TRAINING_INTAKE_APPROVED_RUNTIME_DPO_PREFLIGHT_REASON_REQUIRED");

        var unapprovedResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            validBody,
            authHeaders);
        AssertTrainingIntakeRouteError(
            unapprovedResponse,
            "DESIGN_CONVERSION_APPROVED_RUNTIME_DPO_PREFLIGHT_EXACT_APPROVED_ROW_REQUIRED",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionPostApprovalPreflight,
            expectedAdmissionSatisfied: true);
        Assert.DoesNotContain(expectedToken, unapprovedResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(tenantId, unapprovedResponse.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(idempotencyKey, unapprovedResponse.BodyJson, StringComparison.Ordinal);
        await AssertTrainingIntakeCountAsync(db, expectedCount: 0);
    }

    [Fact]
    public async Task PostTrainingIntake_RuntimeApprovalAndExport_RejectsMissingFieldsAndAuthBeforeMutation()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_RUNTIME_APPROVAL_AUTH_SECRET";
        const string expectedToken = "local-training-intake-runtime-approval-auth-token";
        const string invalidToken = "local-training-intake-runtime-approval-invalid-token";
        const string tenantId = "tenant:modelserver-training-intake-runtime-approval-auth";
        const string idempotencyKey = "idempotency:training-intake-runtime-approval-auth";
        const string recordId = "training-intake-runtime-approval-auth-record";
        var root = CreateOwnedTempPath("modelserver-training-intake-runtime-approval-auth-route");
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
        var pending = ToRuntimePendingCandidate(WithPersistedRuntimeArtifactRefs(
            BuildTrainingRecord(
                "training-intake-runtime-approval-auth-pending",
                DesignConversionDpoUnits.CriticUnit,
                0.94),
            "training-intake-runtime-approval-auth-pending-artifact"));
        await Save(db, BuildArtifactRecord(
            recordId,
            "training-intake-runtime-approval-auth-job",
            "training-intake-runtime-approval-auth-request",
            [pending],
            privateMarker));
        var registry = CreateRegistry(db, expectedToken);
        var authHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName] = expectedToken
        };
        var invalidAuthHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName] = invalidToken
        };

        var missingTokenResponse = await InvokeTrainingIntakeRouteAsync(registry, new
        {
            action = DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
            tenantId,
            idempotencyKey,
            localArtifactRecordId = recordId,
            candidateOrTrainingRecordId = pending.RecordId,
            requestedByRef = "reviewer:modelserver-training-intake-runtime-approval-auth",
            reason = "route approval auth missing should not mutate"
        });
        AssertTrainingIntakeRouteError(
            missingTokenResponse,
            "TRAINING_INTAKE_AUTH_TOKEN_REQUIRED",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
            expectedAdmissionSatisfied: false,
            expectedStatusCode: 401);
        AssertAuthAdmissionRejectedWithoutRawSecrets(
            missingTokenResponse,
            expectedToken,
            invalidToken,
            tenantId,
            idempotencyKey);

        var invalidTokenResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionExportApprovedRuntimeCandidates,
                tenantId,
                idempotencyKey = "idempotency:training-intake-runtime-export-auth-invalid",
                query = new DesignConversionLocalArtifactDbQuery(JobId: "training-intake-runtime-approval-auth-job")
            },
            invalidAuthHeaders);
        AssertTrainingIntakeRouteError(
            invalidTokenResponse,
            "TRAINING_INTAKE_AUTH_TOKEN_INVALID",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionExportApprovedRuntimeCandidates,
            expectedAdmissionSatisfied: false,
            expectedStatusCode: 401);

        var missingRecordIdResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
                tenantId,
                idempotencyKey = "idempotency:training-intake-runtime-approval-missing-record",
                candidateOrTrainingRecordId = pending.RecordId,
                requestedByRef = "reviewer:modelserver-training-intake-runtime-approval-auth",
                reason = "route approval missing record id should not mutate"
            },
            authHeaders);
        AssertTrainingIntakeRouteError(
            missingRecordIdResponse,
            "TRAINING_INTAKE_LOCAL_ARTIFACT_RECORD_ID_REQUIRED",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
            expectedAdmissionSatisfied: true);

        var missingCandidateIdResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
                tenantId,
                idempotencyKey = "idempotency:training-intake-runtime-approval-missing-candidate",
                localArtifactRecordId = recordId,
                requestedByRef = "reviewer:modelserver-training-intake-runtime-approval-auth",
                reason = "route approval missing candidate id should not mutate"
            },
            authHeaders);
        AssertTrainingIntakeRouteError(
            missingCandidateIdResponse,
            "TRAINING_INTAKE_DPO_CANDIDATE_ID_REQUIRED",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
            expectedAdmissionSatisfied: true);

        var missingReviewerResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
                tenantId,
                idempotencyKey = "idempotency:training-intake-runtime-approval-missing-reviewer",
                localArtifactRecordId = recordId,
                candidateOrTrainingRecordId = pending.RecordId,
                reason = "route approval missing reviewer should not mutate"
            },
            authHeaders);
        AssertTrainingIntakeRouteError(
            missingReviewerResponse,
            "TRAINING_INTAKE_APPROVAL_REVIEWER_REQUIRED",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
            expectedAdmissionSatisfied: true);

        var missingReasonResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
                tenantId,
                idempotencyKey = "idempotency:training-intake-runtime-approval-missing-reason",
                localArtifactRecordId = recordId,
                candidateOrTrainingRecordId = pending.RecordId,
                requestedByRef = "reviewer:modelserver-training-intake-runtime-approval-auth"
            },
            authHeaders);
        AssertTrainingIntakeRouteError(
            missingReasonResponse,
            "TRAINING_INTAKE_APPROVAL_REASON_REQUIRED",
            privateMarker,
            DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate,
            expectedAdmissionSatisfied: true);

        var approvedTraining = await db.QueryApprovedTrainingCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "training-intake-runtime-approval-auth-job"));
        Assert.True(approvedTraining.IsSuccess, approvedTraining.ErrorMessage);
        Assert.Empty(approvedTraining.Value!);
        await AssertTrainingIntakeCountAsync(db, expectedCount: 0);
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
    public async Task PostTrainingIntake_RemotePromotionConfiguredEarlyError_UsesBooleanRouteFlags()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_REMOTE_EARLY_ERROR_SECRET";
        var seeded = await CreateApprovedHandoffAsync(privateMarker);
        var handler = DesignConversionModelServerTrainingIntakeHandler.CreateForValidatedPromotionHost(
            new DesignConversionDpoUsefulnessApi(seeded.Db),
            PromotionHostTenantId,
            remotePromotionConfigured: true,
            measurementGateThresholdConfiguration: null);

        try
        {
            var response = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
                Method: "POST",
                Path: DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                BodyJson: JsonSerializer.Serialize(new
                {
                    action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate
                }),
                ContentType: "text/plain"));

            Assert.Equal(415, response.StatusCode);
            AssertNoPrivateTrainingIntakePayload(response.BodyJson, privateMarker);
            using var document = JsonDocument.Parse(response.BodyJson);
            var root = document.RootElement;
            Assert.Equal("UNSUPPORTED_MEDIA_TYPE", root.GetProperty("errorCode").GetString());
            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_remote_promotion_configured").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("training_intake_route_local_only").GetString());
            Assert.NotEqual(
                DesignConversionModelServerCallerContract.TrainingIntakeRouteDependentRemotePromotionConfigurationMetadataValue,
                metadata.GetProperty("training_intake_remote_promotion_configured").GetString());
            Assert.NotEqual(
                DesignConversionModelServerCallerContract.TrainingIntakeRouteDependentRemotePromotionConfigurationMetadataValue,
                metadata.GetProperty("training_intake_route_local_only").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        }
        finally
        {
            DeleteDirectoryIfExists(seeded.RootDirectory);
        }
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
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            dpoPolicyBaseProvider: null,
            bridgeBalancedTopRows: true);
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
        => await AssertPromoteLocalDedupesSameTenantIdempotencyPayloadWithoutSecondPromotionRecordAsync();

    [Fact]
    public async Task PostTrainingIntake_PromoteLocal_DpoBaseInitializationIgnoresPriorModelConstructionNoise()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        for (var index = 0; index < 5; index++)
        {
            // Создаём обычные модели до route-вызова: regression ловит
            // возвращение зависимости local readiness от порядка соседних тестов.
            _ = new LlmModel(config);
        }

        await AssertPromoteLocalDedupesSameTenantIdempotencyPayloadWithoutSecondPromotionRecordAsync();
    }

    private async Task AssertPromoteLocalDedupesSameTenantIdempotencyPayloadWithoutSecondPromotionRecordAsync()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_PROMOTE_LOCAL_IDEMPOTENCY_SECRET";
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, policyBaseProvider);
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
            seeded.RootDirectory,
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
            var filesAfterFirstPromotion = Directory.GetFiles(promotionOutputDirectory, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(promotionOutputDirectory, path)
                    .Replace(Path.DirectorySeparatorChar, '/')
                    .Replace(Path.AltDirectorySeparatorChar, '/'))
                .Order(StringComparer.Ordinal)
                .ToArray();
            var secondResponse = await InvokeTrainingIntakeRouteAsync(registry, promoteBody);
            var filesAfterSecondPromotion = Directory.GetFiles(promotionOutputDirectory, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(promotionOutputDirectory, path)
                    .Replace(Path.DirectorySeparatorChar, '/')
                    .Replace(Path.AltDirectorySeparatorChar, '/'))
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
                "promote_local_fenced_operation_journal_then_legacy_projection",
                secondMetadata.GetProperty("training_intake_idempotency_side_effect_boundary").GetString());
            Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_promote_local_artifact_append_atomic").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_promote_local_recoverable_residue_possible").GetString());
            Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_promote_local_retry_may_repeat_training").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_promote_local_no_retrain_claimed").GetString());

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
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, policyBaseProvider);
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
            seeded.RootDirectory,
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
            var filesAfterFirstStatus = Directory.GetFiles(promotionOutputDirectory, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(promotionOutputDirectory, path)
                    .Replace(Path.DirectorySeparatorChar, '/')
                    .Replace(Path.AltDirectorySeparatorChar, '/'))
                .Order(StringComparer.Ordinal)
                .ToArray();
            var secondResponse = await InvokeTrainingIntakeRouteAsync(registry, uploadStatusBody);
            var filesAfterSecondStatus = Directory.GetFiles(promotionOutputDirectory, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(promotionOutputDirectory, path)
                    .Replace(Path.DirectorySeparatorChar, '/')
                    .Replace(Path.AltDirectorySeparatorChar, '/'))
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
            Assert.Contains(filesAfterFirstStatus, path =>
                path.StartsWith("dpo-", StringComparison.Ordinal) && path.Contains("/", StringComparison.Ordinal));
            Assert.Contains(filesAfterFirstStatus, path => path.EndsWith("-promoted-checkpoint.json", StringComparison.Ordinal));
            Assert.Contains(filesAfterFirstStatus, path => path.EndsWith("-tokenizer-artifact.json", StringComparison.Ordinal));
            Assert.Contains(filesAfterFirstStatus, path => path.EndsWith("-source-encoder-contract.json", StringComparison.Ordinal));
            Assert.Contains(filesAfterFirstStatus, path => path.EndsWith("-policy-checkpoint-contract.json", StringComparison.Ordinal));
            Assert.Contains(filesAfterFirstStatus, path => path.EndsWith(".bin", StringComparison.Ordinal));

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
                "promote_local_fenced_operation_journal_then_legacy_projection",
                secondMetadata.GetProperty("training_intake_idempotency_side_effect_boundary").GetString());
            Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_promote_local_artifact_append_atomic").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_promote_local_recoverable_residue_possible").GetString());
            Assert.Equal(bool.FalseString, secondMetadata.GetProperty("training_intake_promote_local_retry_may_repeat_training").GetString());
            Assert.Equal(bool.TrueString, secondMetadata.GetProperty("training_intake_promote_local_no_retrain_claimed").GetString());

            var secondLifecycleMetadata = secondLifecycle.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, secondLifecycleMetadata.GetProperty("training_intake_replay_cache_checked").GetString());
            Assert.Equal(bool.TrueString, secondLifecycleMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
            Assert.Equal(bool.FalseString, secondLifecycleMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
            Assert.Equal(bool.FalseString, secondLifecycleMetadata.GetProperty("training_intake_promote_local_artifact_append_atomic").GetString());
            Assert.Equal(bool.TrueString, secondLifecycleMetadata.GetProperty("training_intake_promote_local_recoverable_residue_possible").GetString());
            Assert.Equal(bool.FalseString, secondLifecycleMetadata.GetProperty("training_intake_promote_local_retry_may_repeat_training").GetString());
            Assert.Equal(bool.TrueString, secondLifecycleMetadata.GetProperty("training_intake_promote_local_no_retrain_claimed").GetString());
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
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, policyBaseProvider);
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
            seeded.RootDirectory,
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
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, policyBaseProvider);
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
            seeded.RootDirectory,
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
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, policyBaseProvider);
        var secondSeeded = await AppendSourceDisjointApprovedHandoffAsync(
            seeded.Db,
            privateMarker,
            "rollback-second");
        Assert.NotNull(seeded.TeacherRequestIds);
        AssertTeacherRequestIdsDisjoint(
            seeded.TeacherRequestIds!,
            secondSeeded.TeacherRequestIds);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-rollback-local-promotion";
        const string createIdempotencyKey = "idempotency:create-before-rollback-local-promotion";
        const string secondCreateIdempotencyKey = "idempotency:create-second-before-rollback-local-promotion";
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
            seeded.RootDirectory,
            "training-intake-rollback-route-first-output");
        var secondPromotionOutputDirectory = Path.Combine(
            seeded.RootDirectory,
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
            var secondCreateResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = secondCreateIdempotencyKey,
                handoff = secondSeeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-rollback-local-promotion-test",
                reason = "route-level second local promotion rollback setup with fresh approved teacher requests",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(216)
            });

            Assert.Equal(200, createResponse.StatusCode);
            Assert.Equal(200, secondCreateResponse.StatusCode);
            using var createDocument = JsonDocument.Parse(createResponse.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();
            using var secondCreateDocument = JsonDocument.Parse(secondCreateResponse.BodyJson);
            var secondIntakeId = secondCreateDocument.RootElement.GetProperty("intakeId").GetString();
            Assert.NotEqual(intakeId, secondIntakeId);

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
                intakeId = secondIntakeId,
                promotionOutputDirectory = secondPromotionOutputDirectory,
                options = secondOptions
            });

            AssertSanitizedTrainingIntakeBody(firstPromotionResponse, privateMarker);
            Assert.True(secondPromotionResponse.StatusCode == 200, secondPromotionResponse.BodyJson);
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
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, policyBaseProvider);
        var secondSeeded = await AppendSourceDisjointApprovedHandoffAsync(
            seeded.Db,
            privateMarker,
            "rollback-replay-second");
        Assert.NotNull(seeded.TeacherRequestIds);
        AssertTeacherRequestIdsDisjoint(
            seeded.TeacherRequestIds!,
            secondSeeded.TeacherRequestIds);
        var registry = CreateRegistry(seeded.Db);
        const string tenantId = "tenant:modelserver-training-intake-rollback-local-promotion-replay";
        const string createIdempotencyKey = "idempotency:create-before-rollback-local-promotion-replay";
        const string secondCreateIdempotencyKey = "idempotency:create-second-before-rollback-local-promotion-replay";
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
            seeded.RootDirectory,
            "training-intake-rollback-replay-route-first-output");
        var secondPromotionOutputDirectory = Path.Combine(
            seeded.RootDirectory,
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
            var secondCreateResponse = await InvokeTrainingIntakeRouteAsync(registry, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = secondCreateIdempotencyKey,
                handoff = secondSeeded.Handoff,
                requestedByRef,
                reason = "route-level second local promotion rollback replay setup with fresh approved teacher requests",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(218)
            });
            Assert.Equal(200, createResponse.StatusCode);
            Assert.Equal(200, secondCreateResponse.StatusCode);
            using var createDocument = JsonDocument.Parse(createResponse.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();
            using var secondCreateDocument = JsonDocument.Parse(secondCreateResponse.BodyJson);
            var secondIntakeId = secondCreateDocument.RootElement.GetProperty("intakeId").GetString();
            Assert.NotEqual(intakeId, secondIntakeId);

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
                intakeId = secondIntakeId,
                promotionOutputDirectory = secondPromotionOutputDirectory,
                options = secondOptions
            };

            var firstPromotionResponse = await InvokeTrainingIntakeRouteAsync(registry, firstPromotionBody);
            var secondPromotionResponse = await InvokeTrainingIntakeRouteAsync(registry, secondPromotionBody);
            Assert.Equal(200, firstPromotionResponse.StatusCode);
            Assert.True(secondPromotionResponse.StatusCode == 200, secondPromotionResponse.BodyJson);
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
        var dpoPolicyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            dpoPolicyBaseProvider: dpoPolicyBaseProvider);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:modelserver-training-intake-actual-route-positive",
            "actual local training-intake route lifecycle into promoted local API factory; not ProductReady",
            DateTimeOffset.UnixEpoch.AddMinutes(205));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var promotionOutputDirectory = Path.Combine(
            seeded.RootDirectory,
            "training-intake-actual-route-positive-promotion-output");
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-actual-route-source-disjoint-positive",
            "training-intake-actual-route-source-disjoint-positive-dataset",
            "training-intake-actual-route-source-disjoint-positive-gate",
            sourceDisjoint: true);

        try
        {
            var promotion = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["training_intake_tenant_id_ref"] =
                            "tenant:training-intake-actual-route-source-disjoint-positive",
                        ["training_intake_idempotency_key_ref"] =
                            "idempotency:training-intake-actual-route-source-disjoint-positive"
                    },
                    () => api.RunTrainingIntakeAndPromoteLocalAsync(
                        intake.Value!.IntakeId,
                        promotionOutputDirectory,
                        options));

            Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
            Assert.Equal(1, dpoPolicyBaseProvider.InvocationCount);
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
                "PROMOTION_ALLOWED_BY_EVALUATED_DPO_DECISION",
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
            Assert.True(dpoPolicyBaseProvider.InvocationCount > 0);
            Assert.NotNull(dpoPolicyBaseProvider.LastModelSet);
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
                () => DateTimeOffset.Parse("2026-07-08T11:00:00Z"),
                artifactDb: seeded.Db);

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
            await AssertPromotedDpoPolicyCheckpointUsesProviderAsync(
                promotion.Value.PromotedCheckpointPath,
                factory.Value.PromotedLoad,
                promotion.Value,
                dpoPolicyBaseProvider);
            var loadedAuthority = factory.Value.PromotedLoad.Machine
                .GetLoadedLearnedWeightAuthoritySummary();
            Assert.True(loadedAuthority.HasRealLearnedWeightAuthority);
            Assert.True(loadedAuthority.DpoPolicyAuthorityLoaded);
            Assert.Equal(0, loadedAuthority.EmbeddedLearnedWeightArtifactCount);
            Assert.Equal(1, loadedAuthority.DpoPolicyLearnedWeightArtifactCount);
            Assert.Equal(1, loadedAuthority.TotalLearnedWeightArtifactCount);
            Assert.True(loadedAuthority.DpoPolicyLearnedWeightObservationCount > 0);
            Assert.Equal(
                loadedAuthority.DpoPolicyLearnedWeightObservationCount,
                loadedAuthority.TotalLearnedWeightObservationCount);

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
                "fresh_promotion_loader_result",
                response.Value.BoundaryMetadata["local_model_verification_authority"]);
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
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, response.Value.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, response.Value.EvidenceRefs);
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
    public async Task RunTrainingIntakeAndPromoteLocalAsync_UnboundedConfiguredUploaderFailsClosedBeforeInvocation()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_CONFIGURED_UPLOADER_SECRET";
        const string failureCode = "DESIGN_CONVERSION_CONFIGURED_PROMOTION_UPLOAD_FAILED";
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var uploader = new FailingTrainingIntakePromotionUploader(
            failureCode,
            $@"Configured uploader failed at C:\Users\lubao\private\{privateMarker}\checkpoint.json?token=credential");
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            policyBaseProvider,
            uploader);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:modelserver-training-intake-configured-uploader-failure",
            "configured promotion uploader failure must not fall back to local receipt",
            DateTimeOffset.UnixEpoch.AddMinutes(206));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var promotionOutputDirectory = Path.Combine(
            seeded.RootDirectory,
            "training-intake-configured-uploader-failure-output");
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-configured-uploader-failure",
            "training-intake-configured-uploader-failure-dataset",
            "training-intake-configured-uploader-failure-gate",
            sourceDisjoint: true);

        try
        {
            var promotion = await DesignConversionPromotionAdmissionTestScope.RunAsync(
                () => api.RunTrainingIntakeAndPromoteLocalAsync(
                    intake.Value!.IntakeId,
                    promotionOutputDirectory,
                    options));

            Assert.False(promotion.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_PROMOTE_LOCAL_UPLOADER_BOUND_REQUIRED",
                promotion.ErrorCode);
            Assert.Equal(0, uploader.UploadCallCount);
            Assert.DoesNotContain(privateMarker, promotion.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(@"C:\Users\lubao\private", promotion.ErrorMessage, StringComparison.OrdinalIgnoreCase);
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
        var promotionOutputDirectory = CreateOwnedTempPath(
            "training-intake-actual-route-negative-promotion-output");
        var statusOnlyOptions = BuildTrainingIntakeActualRouteOptions(
            "training-intake-actual-route-status-only-negative",
            "training-intake-actual-route-status-only-negative-dataset",
            "training-intake-actual-route-status-only-negative-gate",
            sourceDisjoint: true);
        var before = SnapshotMaterialFiles(seeded.RootDirectory);

        try
        {
            var promotion = await api.RunTrainingIntakeAndPromoteLocalAsync(
                intake.Value!.IntakeId,
                promotionOutputDirectory,
                statusOnlyOptions);

            Assert.False(promotion.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_PROMOTE_LOCAL_STABLE_IDENTITY_REQUIRED",
                promotion.ErrorCode);
            Assert.Equal(
                "DesignConversion local promotion requires stable tenant and idempotency identity before mutation.",
                promotion.ErrorMessage);
            Assert.Equal(before, SnapshotMaterialFiles(seeded.RootDirectory));
            Assert.Empty(SnapshotFiles(promotionOutputDirectory));
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
        const string promoteCreateIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-promote-create";
        const string runIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-run-promote-run";
        const string promoteIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-run-promote-promote";
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, policyBaseProvider);
        var promotionSeeded = await AppendSourceDisjointApprovedHandoffAsync(
            seeded.Db,
            privateMarker,
            "aspnet-promote");
        Assert.NotNull(seeded.TeacherRequestIds);
        AssertTeacherRequestIdsDisjoint(
            seeded.TeacherRequestIds!,
            promotionSeeded.TeacherRequestIds);
        var registry = CreateRegistry(seeded.Db);
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-aspnet-run-promote-source-disjoint",
            "training-intake-aspnet-run-promote-source-disjoint-dataset",
            "training-intake-aspnet-run-promote-source-disjoint-gate",
            sourceDisjoint: true);
        var promotionOutputDirectory = Path.Combine(
            seeded.RootDirectory,
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
                Timeout = TimeSpan.FromSeconds(30)
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

            var promotionCreated = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId,
                idempotencyKey = promoteCreateIdempotencyKey,
                handoff = promotionSeeded.Handoff,
                requestedByRef = "architect:modelserver-training-intake-aspnet-run-promote-test",
                reason = "aspnet mapper fresh approved intake for promotion after separate run proof",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(208)
            });
            using var promotionCreatedResponse = promotionCreated.Response;
            Assert.True(
                promotionCreatedResponse.StatusCode == HttpStatusCode.OK,
                promotionCreated.BodyJson);
            using var promotionCreateDocument = JsonDocument.Parse(promotionCreated.BodyJson);
            var promotionIntakeId = promotionCreateDocument.RootElement
                .GetProperty("intakeId")
                .GetString();
            Assert.NotEqual(intakeId, promotionIntakeId);

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
            Assert.False(trainingRun.GetProperty("modelUploadReady").GetBoolean());
            Assert.Equal("proven", trainingRun.GetProperty("sourceManifestStatus").GetString());
            Assert.Equal("proven", trainingRun.GetProperty("sourceDisjointStatus").GetString());
            Assert.Equal(100, trainingRun.GetProperty("resolvedTrainingRecordCount").GetInt32());
            var runnerEvidence = trainingRun.GetProperty("runnerEvidence");
            Assert.True(runnerEvidence.GetProperty("sourceDisjoint").GetBoolean());
            Assert.Equal("proven", runnerEvidence.GetProperty("sourceDisjointStatus").GetString());
            Assert.Equal(
                "PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED",
                runnerEvidence.GetProperty("promotionStatus").GetString());
            var trainingRunMetadata = trainingRun.GetProperty("boundaryMetadata");
            Assert.Contains(trainingRunMetadata.EnumerateObject(), property =>
                property.Name.Contains("source_disjoint", StringComparison.Ordinal) &&
                string.Equals(property.Value.GetString(), "proven", StringComparison.Ordinal));
            Assert.Equal(bool.FalseString, trainingRunMetadata.GetProperty("model_upload_ready").GetString());
            Assert.Equal(bool.TrueString, trainingRunMetadata.GetProperty("model_upload_not_ready").GetString());
            Assert.Equal(bool.FalseString, trainingRunMetadata.GetProperty("promotion_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, trainingRunMetadata.GetProperty("promotion_claimed").GetString());
            Assert.Equal(bool.FalseString, trainingRunMetadata.GetProperty("promotion_upload_allowed").GetString());
            Assert.Equal(bool.FalseString, trainingRunMetadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.TrueString, trainingRunMetadata.GetProperty("product_not_ready").GetString());

            var promoted = await PostTrainingIntakeAsync(new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = promoteIdempotencyKey,
                intakeId = promotionIntakeId,
                promotionOutputDirectory,
                options
            });
            using var promotedResponse = promoted.Response;

            Assert.True(promotedResponse.StatusCode == HttpStatusCode.OK, promoted.BodyJson);
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
            Assert.Equal(promotionIntakeId, promotedRoot.GetProperty("intakeId").GetString());
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
            Assert.Equal(promotionIntakeId, promotedRun.GetProperty("trainingIntakeId").GetString());
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
        var root = CreateOwnedTempPath("host-promotion-route-metadata");
        var uploadHandler = new CapturingPromotionUploadHttpHandler(PromotionUploadResponseMode.Success);
        using var promotionHttpClient = new HttpClient(uploadHandler, disposeHandler: true);
        var hostOptions = CreatePromotionHostOptions(
            root,
            promotionHttpClient,
            dpoPolicyBaseProvider: null);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
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
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        using var legalTeacherAssociation =
            AssociateStartupIssuedLegalTeacherCapabilityForPromotionHost(
                policyBaseProvider);
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, dpoPolicyBaseProvider: null);
        var promotionOutputDirectory = Path.Combine(seeded.RootDirectory, "remote-promotion-output");
        var uploadHandler = new CapturingPromotionUploadHttpHandler(PromotionUploadResponseMode.Success);
        using var promotionHttpClient = new HttpClient(uploadHandler, disposeHandler: true);
        var hostOptions = CreatePromotionHostOptions(
            seeded.RootDirectory,
            promotionHttpClient,
            policyBaseProvider);
        var routeAdmissionOptions = CreatePromotionRouteAdmissionOptions(PromotionHostTenantId);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
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
            routeAdmissionOptions: routeAdmissionOptions,
            measurementGateThresholdConfiguration: ProductionMeasurementGateThresholdConfiguration());
        Assert.NotNull(startupBuilder);

        await app.StartAsync();
        try
        {
            AssertTrainingIntakeStartupMetadata(app, externalProviderConfigured: true);
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(30)
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
            Assert.True(
                promoteResponse.StatusCode == HttpStatusCode.OK,
                $"intakeId={intakeId}; sendCount={uploadHandler.SendCount}; " +
                "promote=" + promote.BodyJson);
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
            var expectedEvaluatedArtifactDirectory = Path.Combine(
                promotionOutputDirectory,
                $"dpo-{ComputeSha256Hex(Encoding.UTF8.GetBytes(runId))[..12]}-dpo-policy-checkpoints");
            var expectedPromotionPaths = DesignConversionDpoPromotionArtifactContractCodec.ResolvePaths(
                seeded.RootDirectory,
                expectedEvaluatedArtifactDirectory,
                runId);
            Assert.True(expectedPromotionPaths.IsSuccess, expectedPromotionPaths.ErrorMessage);
            var expectedPolicyContract = JsonSerializer.Deserialize<DesignConversionDpoPolicyCheckpointContract>(
                File.ReadAllBytes(expectedPromotionPaths.Value!.PolicyCheckpointContractPath),
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(expectedPolicyContract);
            var expectedPolicyCheckpointFileName = expectedPolicyContract!.Binding.PolicyCheckpointFileName;
            var expectedPolicyCheckpointPath = Path.Combine(
                expectedEvaluatedArtifactDirectory,
                expectedPolicyCheckpointFileName);
            var expectedPolicyCheckpointBytes = await File.ReadAllBytesAsync(expectedPolicyCheckpointPath);
            var expectedPolicyCheckpointSha256 = ComputeSha256Hex(expectedPolicyCheckpointBytes);
            Assert.EndsWith(".bin", expectedPolicyCheckpointFileName, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(expectedPolicyContract.Binding.PolicyCheckpointSha256, expectedPolicyCheckpointSha256);
            Assert.Equal(
                expectedPolicyContract.Binding.PolicyCheckpointByteLength,
                expectedPolicyCheckpointBytes.LongLength);

            var policyCheckpointPart = uploadHandler.Parts.SingleOrDefault(
                part => string.Equals(part.Name, "policy-checkpoint", StringComparison.Ordinal));
            Assert.True(
                policyCheckpointPart is not null,
                $"Expected evaluated policy checkpoint part role=policy-checkpoint, " +
                $"filename={expectedPolicyCheckpointFileName}, sha256={expectedPolicyCheckpointSha256}; " +
                $"actual parts: {string.Join(", ", uploadHandler.Parts.Select(part =>
                    $"{part.Name}/{part.FileName}/{ComputeSha256Hex(part.Body)}"))}");
            Assert.Equal(expectedPolicyCheckpointFileName, policyCheckpointPart!.FileName);
            Assert.Equal(expectedPolicyCheckpointBytes, policyCheckpointPart.Body);

            Assert.Equal(5, uploadHandler.Metadata.Artifacts.Count);
            Assert.Equal(
                [
                    "policy-checkpoint",
                    "policy-checkpoint-contract",
                    "runtime-checkpoint",
                    "source-encoder-contract",
                    "tokenizer"
                ],
                uploadHandler.Metadata.Artifacts.Select(artifact => artifact.Role).Order(StringComparer.Ordinal).ToArray());
            Assert.DoesNotContain(uploadHandler.Metadata.Artifacts, artifact =>
                string.Equals(artifact.Role, "policy", StringComparison.Ordinal));
            Assert.All(uploadHandler.Metadata.Artifacts, artifact =>
            {
                Assert.True(artifact.Length > 0);
                Assert.Matches("^[a-f0-9]{64}$", artifact.Sha256);
            });
            var policyCheckpointMetadata = Assert.Single(
                uploadHandler.Metadata.Artifacts,
                artifact => string.Equals(artifact.Role, "policy-checkpoint", StringComparison.Ordinal));
            Assert.Equal(expectedPolicyCheckpointFileName, policyCheckpointMetadata.FileName);
            Assert.Equal(expectedPolicyCheckpointBytes.LongLength, policyCheckpointMetadata.Length);
            Assert.Equal(expectedPolicyCheckpointSha256, policyCheckpointMetadata.Sha256);
            var policyContractPart = Assert.Single(
                uploadHandler.Parts,
                part => string.Equals(part.Name, "policy-checkpoint-contract", StringComparison.Ordinal));
            Assert.Equal(
                Path.GetFileName(expectedPromotionPaths.Value.PolicyCheckpointContractPath),
                policyContractPart.FileName);
            Assert.NotEqual(expectedPolicyCheckpointSha256, ComputeSha256Hex(policyContractPart.Body));
            Assert.Contains(uploadHandler.Parts, part =>
                string.Equals(part.Name, "source-encoder-contract", StringComparison.Ordinal) &&
                string.Equals(
                    part.FileName,
                    Path.GetFileName(expectedPromotionPaths.Value.SourceEncoderContractPath),
                    StringComparison.Ordinal));
            Assert.Contains(uploadHandler.Parts, part =>
                string.Equals(part.Name, "runtime-checkpoint", StringComparison.Ordinal) &&
                string.Equals(
                    part.FileName,
                    Path.GetFileName(expectedPromotionPaths.Value.RuntimeCheckpointPath),
                    StringComparison.Ordinal));
            Assert.Contains(uploadHandler.Parts, part =>
                string.Equals(part.Name, "tokenizer", StringComparison.Ordinal) &&
                string.Equals(
                    part.FileName,
                    Path.GetFileName(expectedPromotionPaths.Value.TokenizerArtifactPath),
                    StringComparison.Ordinal));

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
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("cloud_model_upload_attempted").GetString());
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

    [Fact]
    public async Task PostTrainingIntake_ContinueTrainingUploadStatus_HostStartupConfiguredHttpPromotionUploader_ReturnsRemoteAwareLifecycleStatusWithoutDeployClaim()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_REMOTE_LIFECYCLE_SECRET";
        const string createIdempotencyKey = "idempotency:host-http-remote-lifecycle-create";
        const string lifecycleIdempotencyKey = "idempotency:host-http-remote-lifecycle-status";
        const string runId = "training-intake-host-http-remote-lifecycle";
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        using var legalTeacherAssociation =
            AssociateStartupIssuedLegalTeacherCapabilityForPromotionHost(
                policyBaseProvider);
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, dpoPolicyBaseProvider: null);
        var promotionOutputDirectory = Path.Combine(seeded.RootDirectory, "remote-lifecycle-promotion-output");
        var uploadHandler = new CapturingPromotionUploadHttpHandler(PromotionUploadResponseMode.Success);
        using var promotionHttpClient = new HttpClient(uploadHandler, disposeHandler: true);
        var hostOptions = CreatePromotionHostOptions(
            seeded.RootDirectory,
            promotionHttpClient,
            policyBaseProvider);
        var routeAdmissionOptions = CreatePromotionRouteAdmissionOptions(PromotionHostTenantId);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
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
            () => DateTimeOffset.UnixEpoch.AddMinutes(607),
            routeAdmissionOptions: routeAdmissionOptions,
            measurementGateThresholdConfiguration: ProductionMeasurementGateThresholdConfiguration());
        Assert.NotNull(startupBuilder);

        await app.StartAsync();
        try
        {
            AssertTrainingIntakeStartupMetadata(app, externalProviderConfigured: true);
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(30)
            };

            var create = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId = PromotionHostTenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:host-http-remote-lifecycle-create-test",
                reason = "host startup configured HTTP promotion uploader create path for remote-aware lifecycle",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(608)
            });
            using var createResponse = create.Response;
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            AssertNoPrivateTrainingIntakePayload(create.BodyJson, privateMarker);
            Assert.DoesNotContain(PromotionHostTenantId, create.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(createIdempotencyKey, create.BodyJson, StringComparison.Ordinal);
            using var createDocument = JsonDocument.Parse(create.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();
            Assert.False(createDocument.RootElement.GetProperty("productReady").GetBoolean());

            var lifecycle = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus,
                tenantId = PromotionHostTenantId,
                idempotencyKey = lifecycleIdempotencyKey,
                intakeId,
                requestedByRef = "architect:host-http-remote-lifecycle-status-test",
                reason = "continue-train, upload through configured HTTP promotion host, and report lifecycle without deploy proof",
                promotionOutputDirectory,
                options = BuildTrainingIntakeActualRouteOptions(
                    runId,
                    "training-intake-host-http-remote-lifecycle-dataset",
                    "training-intake-host-http-remote-lifecycle-gate",
                    sourceDisjoint: true)
            });
            using var lifecycleResponse = lifecycle.Response;
            Assert.True(
                lifecycleResponse.StatusCode == HttpStatusCode.OK,
                $"intakeId={intakeId}; sendCount={uploadHandler.SendCount}; " +
                "lifecycle=" + lifecycle.BodyJson);
            AssertNoPrivateTrainingIntakePayload(lifecycle.BodyJson, privateMarker);
            Assert.DoesNotContain(PromotionHostTenantId, lifecycle.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(lifecycleIdempotencyKey, lifecycle.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(PromotionHostBearerToken, lifecycle.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain("Authorization", lifecycle.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory, lifecycle.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), lifecycle.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(seeded.RootDirectory, lifecycle.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"local_only_lifecycle_status\":\"True\"", lifecycle.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain("upload_deploy_blocked_local_only", lifecycle.BodyJson, StringComparison.Ordinal);
            Assert.Equal(1, uploadHandler.SendCount);

            using var lifecycleDocument = JsonDocument.Parse(lifecycle.BodyJson);
            var root = lifecycleDocument.RootElement;
            Assert.Equal(DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus, root.GetProperty("action").GetString());
            Assert.False(root.GetProperty("productReady").GetBoolean());
            Assert.False(root.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());

            var rootMetadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_remote_promotion_configured").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("training_intake_route_local_only").GetString());
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_host_tenant_verified").GetString());
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("upload_attempted").GetString());
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("external_cloud_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("local_only_lifecycle_status").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("product_ready").GetString());

            var localPromotion = root.GetProperty("localPromotion");
            Assert.False(localPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.True(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
            Assert.Equal("https://promotion.example/receipts/route-remote-upload-1", localPromotion.GetProperty("promotionUploadRef").GetString());
            Assert.Equal(JsonValueKind.Null, localPromotion.GetProperty("localOnly").ValueKind);
            Assert.Equal(JsonValueKind.Null, localPromotion.GetProperty("promotedLocalModelId").ValueKind);

            var lifecycleStatus = root.GetProperty("lifecycleStatus");
            Assert.Equal("continue-training-and-upload-deploy-status", lifecycleStatus.GetProperty("requestedOperation").GetString());
            Assert.True(lifecycleStatus.GetProperty("uploadRequested").GetBoolean());
            Assert.True(lifecycleStatus.GetProperty("uploadAttempted").GetBoolean());
            Assert.Equal("model_server_upload_verified", lifecycleStatus.GetProperty("uploadStatus").GetString());
            Assert.Equal(DesignConversionPromotionPublicationStatus.UploadedVerified, lifecycleStatus.GetProperty("cloudUploadStatus").GetString());
            Assert.Equal("https://promotion.example/receipts/route-remote-upload-1", lifecycleStatus.GetProperty("promotionUploadRef").GetString());
            Assert.Equal("https://promotion.example/receipts/route-remote-upload-1", lifecycleStatus.GetProperty("localPublishPackageRef").GetString());
            Assert.True(lifecycleStatus.GetProperty("deployRequested").GetBoolean());
            Assert.False(lifecycleStatus.GetProperty("deploymentAttempted").GetBoolean());
            Assert.False(lifecycleStatus.GetProperty("deploymentVerified").GetBoolean());
            Assert.Equal("blocked_no_deployment_proof_after_verified_upload", lifecycleStatus.GetProperty("deploymentStatus").GetString());
            Assert.False(lifecycleStatus.GetProperty("productReady").GetBoolean());
            Assert.False(lifecycleStatus.GetProperty("localPromotionReady").GetBoolean());
            Assert.False(lifecycleStatus.GetProperty("localOnly").GetBoolean());

            var lifecycleMetadata = lifecycleStatus.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("upload_attempted").GetString());
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("external_cloud_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("model_server_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("model_server_upload_verified").GetString());
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("model_server_promotion_claimed").GetString());
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("remote_upload_evidence_present").GetString());
            Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("local_only_lifecycle_status").GetString());
            Assert.Equal("model_server_upload_verified", lifecycleMetadata.GetProperty("upload_status").GetString());
            Assert.Equal(DesignConversionPromotionPublicationStatus.UploadedVerified, lifecycleMetadata.GetProperty("cloud_upload_status").GetString());
            Assert.Equal("blocked_no_deployment_proof_after_verified_upload", lifecycleMetadata.GetProperty("deployment_status").GetString());
            Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("deployment_attempted").GetString());
            Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("product_ready").GetString());

            var lifecycleEvidenceRefs = lifecycleStatus.GetProperty("evidenceRefs")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray();
            Assert.Contains("DESIGN_CONVERSION_MODEL_LIFECYCLE_COMMAND_SURFACE_REMOTE_UPLOAD_VERIFIED", lifecycleEvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_MODEL_SERVER_UPLOAD_VERIFIED_BY_CONFIGURED_UPLOADER", lifecycleEvidenceRefs);
            Assert.Contains("MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT", lifecycleEvidenceRefs);
            Assert.Contains("MODEL_SERVER_PROMOTION_CLAIMED_VERIFIED_REMOTE_RECEIPT", lifecycleEvidenceRefs);
            Assert.Contains(UploadReceiptEvidenceHashRef("MODEL_SERVER_PROMOTION_HTTP_UPLOAD_RECEIPT_VERIFIED"), lifecycleEvidenceRefs);

            var lifecycleWarnings = lifecycleStatus.GetProperty("warnings")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray();
            Assert.Contains("continue_training_upload_status_remote_upload_verified_non_deployed", lifecycleWarnings);
            Assert.Contains("product_ready=false", lifecycleWarnings);
            Assert.DoesNotContain("upload_deploy_blocked_local_only", lifecycleWarnings);
            Assert.DoesNotContain("continue_training_upload_status_local_only", lifecycleWarnings);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(seeded.RootDirectory);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_HostStartupConfiguredRepositoryPromotionUploader_UploadsLoadsRegistersLocatesAndReturnsVerifiedRemoteReceipt()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_HOST_REPOSITORY_PROMOTION_SECRET";
        const string createIdempotencyKey = "idempotency:host-repository-promotion-create";
        const string promoteIdempotencyKey = "idempotency:host-repository-promotion-promote";
        const string runId = "training-intake-host-repository-promotion";
        var policyBaseProvider = CreateTrainingIntakeRepositoryPromotionPolicyBaseProvider();
        using var legalTeacherAssociation =
            AssociateStartupIssuedLegalTeacherCapabilityForPromotionHost(
                policyBaseProvider);
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            dpoPolicyBaseProvider: policyBaseProvider,
            bridgeBalancedTopRows: true,
            bridgeBalancedHeldOutRows: true,
            bridgeEvaluatedDpoContrastRows: true);
        var promotionOutputDirectory = Path.Combine(seeded.RootDirectory, "repository-promotion-output");
        var calls = new List<string>();
        var repository = new RecordingTrainingIntakeModelRepository(
            calls,
            "training-intake-host-repository-promotion-repository");
        var locator = new RecordingTrainingIntakeRemoteUnitLocator(
            calls,
            "training-intake-host-repository-promotion-locator");
        var hostOptions = CreateRepositoryPromotionHostOptions(
            seeded.RootDirectory,
            repository,
            locator,
            RepositoryPromotionServerBaseUrl,
            PromotionHostTenantId,
            policyBaseProvider);
        var routeAdmissionOptions = CreatePromotionRouteAdmissionOptions(
            PromotionHostTenantId,
            "design-conversion-training-intake-host-repository-promotion");
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
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
            () => DateTimeOffset.UnixEpoch.AddMinutes(641),
            routeAdmissionOptions: routeAdmissionOptions,
            measurementGateThresholdConfiguration: ProductionMeasurementGateThresholdConfiguration());
        Assert.NotNull(startupBuilder);

        await app.StartAsync();
        try
        {
            AssertTrainingIntakeStartupMetadata(app, externalProviderConfigured: true);
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(15)
            };

            var create = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId = PromotionHostTenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:host-repository-promotion-create-test",
                reason = "host startup configured repository promotion uploader create path; productReady remains false",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(642)
            });
            using var createResponse = create.Response;
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            AssertNoPrivateTrainingIntakePayload(create.BodyJson, privateMarker);
            Assert.DoesNotContain(PromotionHostTenantId, create.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(createIdempotencyKey, create.BodyJson, StringComparison.Ordinal);
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
                    "training-intake-host-repository-promotion-dataset",
                    "training-intake-host-repository-promotion-gate",
                    sourceDisjoint: true)
            });
            using var promoteResponse = promote.Response;
            Assert.True(
                promoteResponse.StatusCode == HttpStatusCode.OK,
                promote.BodyJson);
            AssertNoPrivateTrainingIntakePayload(promote.BodyJson, privateMarker);
            Assert.DoesNotContain(PromotionHostTenantId, promote.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(promoteIdempotencyKey, promote.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(PromotionHostBearerToken, promote.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain("Authorization", promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory, promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(seeded.RootDirectory, promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"productReady\":true", promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"deploymentVerified\":true", promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"publicHttpEndpointVerified\":true", promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("cloud_ready", promote.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("dpo_train_ready\":true", promote.BodyJson, StringComparison.OrdinalIgnoreCase);

            Assert.Equal(
                [
                    "upload:critic",
                    "upload:improver",
                    "load:critic",
                    "load:improver",
                    "register:critic",
                    "register:improver",
                    "locate:critic",
                    "locate:improver"
                ],
                calls);

            var expectedUnitIds = new[]
            {
                DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                DesignConversionRuntimeScaffoldImproverUnit.UnitId
            };
            var uploadedByUnit = repository.UploadedManifests.ToDictionary(
                manifest => manifest.UnitId,
                StringComparer.Ordinal);
            Assert.Equal(expectedUnitIds.Length, uploadedByUnit.Count);
            Assert.Equal(expectedUnitIds.Order(StringComparer.Ordinal).ToArray(), uploadedByUnit.Keys.Order(StringComparer.Ordinal).ToArray());
            Assert.Equal(expectedUnitIds.Order(StringComparer.Ordinal).ToArray(), locator.RegisteredByUnit.Keys.Order(StringComparer.Ordinal).ToArray());
            Assert.Equal(expectedUnitIds.Order(StringComparer.Ordinal).ToArray(), locator.LocatedByUnit.Keys.Order(StringComparer.Ordinal).ToArray());

            foreach (var unitId in expectedUnitIds)
            {
                var uploadedManifest = uploadedByUnit[unitId];
                var expectedEndpoint = $"/models/{Uri.EscapeDataString(unitId)}";
                var expectedCheckpointSha256 = ComputeSha256Hex(uploadedManifest.PolicyCheckpointPath);
                var expectedRemoteManifestSha256 = uploadedManifest.ManifestSha256;
                Assert.False(string.IsNullOrWhiteSpace(expectedRemoteManifestSha256));
                Assert.Equal(expectedCheckpointSha256, repository.UploadedCheckpointSha256ByUnit[unitId]);
                Assert.Equal(expectedCheckpointSha256, repository.LoadedCheckpointSha256ByUnit[unitId]);
                Assert.Equal(uploadedManifest.PolicyCheckpointPath, repository.LoadFallbackPathByUnit[unitId]);
                Assert.Equal(RepositoryPromotionServerBaseUrl, repository.UploadServerBaseUrlByUnit[unitId]);
                Assert.Equal(RepositoryPromotionServerBaseUrl, repository.LoadServerBaseUrlByUnit[unitId]);

                var registered = locator.RegisteredByUnit[unitId];
                var located = locator.LocatedByUnit[unitId];
                Assert.Equal(unitId, registered.UnitId);
                Assert.Equal(uploadedManifest.UnitRole, registered.UnitRole);
                Assert.Equal(uploadedManifest.DomainTag, registered.DomainTag);
                Assert.Equal(uploadedManifest.Version, registered.Version);
                Assert.Equal(RepositoryPromotionServerBaseUrl, registered.ServerBaseUrl);
                Assert.Equal(expectedEndpoint, registered.UnitEndpoint);
                Assert.Equal(expectedRemoteManifestSha256, registered.ManifestSha256);
                Assert.Equal(uploadedManifest.SourceDatasetSha256, registered.SourceDatasetSha256);
                Assert.Equal(registered.UnitId, located.UnitId);
                Assert.Equal(registered.Version, located.Version);
                Assert.Equal(registered.ManifestSha256, located.ManifestSha256);
                Assert.Equal(registered.UnitEndpoint, located.UnitEndpoint);

                AssertNoPublicPathLeak(promote.BodyJson, uploadedManifest.PolicyCheckpointPath);
                AssertNoPublicPathLeak(promote.BodyJson, uploadedManifest.TokenizerPath);
            }

            using var promoteDocument = JsonDocument.Parse(promote.BodyJson);
            var promoteRoot = promoteDocument.RootElement;
            Assert.Equal(DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal, promoteRoot.GetProperty("action").GetString());
            Assert.False(promoteRoot.GetProperty("productReady").GetBoolean());
            Assert.False(promoteRoot.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(promoteRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());
            var rootMetadata = promoteRoot.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_remote_promotion_configured").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("training_intake_route_local_only").GetString());
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_host_tenant_verified").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("product_ready").GetString());

            var localPromotion = promoteRoot.GetProperty("localPromotion");
            Assert.False(localPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.False(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
            Assert.Equal(
                $"{RepositoryPromotionServerBaseUrl}/models/{Uri.EscapeDataString(DesignConversionRuntimeScaffoldCriticUnit.UnitId)}",
                localPromotion.GetProperty("promotionUploadRef").GetString());
            Assert.Equal(JsonValueKind.Null, localPromotion.GetProperty("localOnly").ValueKind);
            Assert.Equal(JsonValueKind.Null, localPromotion.GetProperty("promotedLocalModelId").ValueKind);
            var localPromotionEvidenceRefs = localPromotion.GetProperty("evidenceRefs")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray();
            Assert.Contains(UploadReceiptEvidenceHashRef("MODEL_REPOSITORY_UPLOAD_BOTH_UNITS_SUCCEEDED"), localPromotionEvidenceRefs);
            Assert.Contains(UploadReceiptEvidenceHashRef("MODEL_REPOSITORY_FRESH_LOAD_SHA_VERIFIED_BOTH_UNITS"), localPromotionEvidenceRefs);
            Assert.Contains(UploadReceiptEvidenceHashRef("REMOTE_LOCATOR_REGISTERED_BOTH_UNITS"), localPromotionEvidenceRefs);
            Assert.Contains(UploadReceiptEvidenceHashRef("REMOTE_LOCATOR_LOCATED_BOTH_UNITS"), localPromotionEvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_MODEL_SERVER_UPLOAD_VERIFIED_BY_CONFIGURED_UPLOADER", localPromotionEvidenceRefs);
            Assert.Contains(UploadReceiptEvidenceHashRef("NO_CLOUD_READINESS_CLAIM"), localPromotionEvidenceRefs);

            var localPromotionMetadata = localPromotion.GetProperty("boundaryMetadata");
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("external_cloud_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("external_cloud_upload_not_attempted").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("cloud_model_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("model_server_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("model_server_upload_verified").GetString());
            Assert.Equal(bool.TrueString, localPromotionMetadata.GetProperty("model_server_promotion_claimed").GetString());
            Assert.Equal("model_server_upload_verified", localPromotionMetadata.GetProperty("promotion_receipt_kind").GetString());
            Assert.Equal(bool.FalseString, localPromotionMetadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(seeded.RootDirectory);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_ContinueTrainingUploadStatus_HostStartupConfiguredRepositoryPromotionUploader_ReturnsRepositoryUploadLifecycleWithoutExternalCloudClaim()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_REPOSITORY_LIFECYCLE_SECRET";
        const string createIdempotencyKey = "idempotency:host-repository-lifecycle-create";
        const string lifecycleIdempotencyKey = "idempotency:host-repository-lifecycle-status";
        const string runId = "training-intake-host-repository-lifecycle";
        var policyBaseProvider = CreateTrainingIntakeRepositoryPromotionPolicyBaseProvider();
        using var legalTeacherAssociation =
            AssociateStartupIssuedLegalTeacherCapabilityForPromotionHost(
                policyBaseProvider);
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            dpoPolicyBaseProvider: policyBaseProvider,
            bridgeBalancedTopRows: true,
            bridgeBalancedHeldOutRows: true,
            bridgeEvaluatedDpoContrastRows: true);
        var promotionOutputDirectory = Path.Combine(seeded.RootDirectory, "repository-lifecycle-output");
        var calls = new List<string>();
        var repository = new RecordingTrainingIntakeModelRepository(
            calls,
            "training-intake-host-repository-lifecycle-repository");
        var locator = new RecordingTrainingIntakeRemoteUnitLocator(
            calls,
            "training-intake-host-repository-lifecycle-locator");
        var hostOptions = CreateRepositoryPromotionHostOptions(
            seeded.RootDirectory,
            repository,
            locator,
            RepositoryPromotionServerBaseUrl,
            PromotionHostTenantId,
            policyBaseProvider);
        var routeAdmissionOptions = CreatePromotionRouteAdmissionOptions(
            PromotionHostTenantId,
            "design-conversion-training-intake-host-repository-lifecycle");
        var repositoryIdentity = repository.PreflightStableConfigurationIdentitySha256();
        var locatorIdentity = locator.PreflightStableConfigurationIdentitySha256();
        Assert.True(repositoryIdentity.IsSuccess, repositoryIdentity.ErrorMessage);
        Assert.True(locatorIdentity.IsSuccess, locatorIdentity.ErrorMessage);
        Assert.Matches(@"^sha256:[a-f0-9]{64}$", repositoryIdentity.Value!);
        Assert.Matches(@"^sha256:[a-f0-9]{64}$", locatorIdentity.Value!);
        Assert.NotEqual(repositoryIdentity.Value, locatorIdentity.Value);

        var promotionUploaderFingerprint =
            new DesignConversionModelRepositoryPromotionUploader(
                repository,
                locator,
                RepositoryPromotionServerBaseUrl)
            .PreflightConfigurationFingerprintSha256();
        var distinctPromotionUploaderFingerprint =
            new DesignConversionModelRepositoryPromotionUploader(
                new RecordingTrainingIntakeModelRepository(
                    calls,
                    "training-intake-host-repository-lifecycle-distinct-repository"),
                new RecordingTrainingIntakeRemoteUnitLocator(
                    calls,
                    "training-intake-host-repository-lifecycle-distinct-locator"),
                RepositoryPromotionServerBaseUrl)
            .PreflightConfigurationFingerprintSha256();
        Assert.True(promotionUploaderFingerprint.IsSuccess, promotionUploaderFingerprint.ErrorMessage);
        Assert.True(distinctPromotionUploaderFingerprint.IsSuccess, distinctPromotionUploaderFingerprint.ErrorMessage);
        Assert.Matches(@"^sha256:[a-f0-9]{64}$", promotionUploaderFingerprint.Value!);
        Assert.Matches(@"^sha256:[a-f0-9]{64}$", distinctPromotionUploaderFingerprint.Value!);
        Assert.NotEqual(promotionUploaderFingerprint.Value, distinctPromotionUploaderFingerprint.Value);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
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
            () => DateTimeOffset.UnixEpoch.AddMinutes(643),
            routeAdmissionOptions: routeAdmissionOptions,
            measurementGateThresholdConfiguration: ProductionMeasurementGateThresholdConfiguration());
        Assert.NotNull(startupBuilder);

        await app.StartAsync();
        try
        {
            AssertTrainingIntakeStartupMetadata(app, externalProviderConfigured: true);
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(15)
            };

            var create = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                tenantId = PromotionHostTenantId,
                idempotencyKey = createIdempotencyKey,
                handoff = seeded.Handoff,
                requestedByRef = "architect:host-repository-lifecycle-create-test",
                reason = "host startup configured repository promotion uploader create path for lifecycle status",
                requestedAtUtc = DateTimeOffset.UnixEpoch.AddMinutes(644)
            });
            using var createResponse = create.Response;
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            AssertNoPrivateTrainingIntakePayload(create.BodyJson, privateMarker);
            Assert.DoesNotContain(PromotionHostTenantId, create.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(createIdempotencyKey, create.BodyJson, StringComparison.Ordinal);
            using var createDocument = JsonDocument.Parse(create.BodyJson);
            var intakeId = createDocument.RootElement.GetProperty("intakeId").GetString();
            Assert.False(createDocument.RootElement.GetProperty("productReady").GetBoolean());

            var lifecycle = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus,
                tenantId = PromotionHostTenantId,
                idempotencyKey = lifecycleIdempotencyKey,
                intakeId,
                requestedByRef = "architect:host-repository-lifecycle-status-test",
                reason = "continue-train, upload through configured model repository, and report lifecycle without deploy proof",
                promotionOutputDirectory,
                options = BuildTrainingIntakeActualRouteOptions(
                    runId,
                    "training-intake-host-repository-lifecycle-dataset",
                    "training-intake-host-repository-lifecycle-gate",
                    sourceDisjoint: true)
            });
            using var lifecycleResponse = lifecycle.Response;
            Assert.True(lifecycleResponse.StatusCode == HttpStatusCode.OK, lifecycle.BodyJson);
            AssertNoPrivateTrainingIntakePayload(lifecycle.BodyJson, privateMarker);
            Assert.DoesNotContain(PromotionHostTenantId, lifecycle.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(lifecycleIdempotencyKey, lifecycle.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(PromotionHostBearerToken, lifecycle.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain("Authorization", lifecycle.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory, lifecycle.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(promotionOutputDirectory.Replace("\\", "\\\\", StringComparison.Ordinal), lifecycle.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(seeded.RootDirectory, lifecycle.BodyJson, StringComparison.OrdinalIgnoreCase);

            var expectedRepositoryCalls = new[]
            {
                "upload:critic",
                "upload:improver",
                "load:critic",
                "load:improver",
                "register:critic",
                "register:improver",
                "locate:critic",
                "locate:improver"
            };

            foreach (var uploadedManifest in repository.UploadedManifests)
            {
                AssertNoPublicPathLeak(lifecycle.BodyJson, uploadedManifest.PolicyCheckpointPath);
                AssertNoPublicPathLeak(lifecycle.BodyJson, uploadedManifest.TokenizerPath);
            }

            using var lifecycleDocument = JsonDocument.Parse(lifecycle.BodyJson);
            var root = lifecycleDocument.RootElement;
            Assert.Equal(DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus, root.GetProperty("action").GetString());
            Assert.False(root.GetProperty("productReady").GetBoolean());
            Assert.False(root.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());

            var rootMetadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_remote_promotion_configured").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("training_intake_route_local_only").GetString());
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_host_tenant_verified").GetString());
            if (calls.Count == 0)
            {
                Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_idempotency_replay_hit").GetString());
                Assert.Equal(bool.FalseString, rootMetadata.GetProperty("training_intake_idempotency_created_new_side_effect").GetString());
                Assert.Equal(bool.TrueString, rootMetadata.GetProperty("training_intake_idempotent_replay_result").GetString());
                Assert.Equal(bool.FalseString, rootMetadata.GetProperty("upload_attempted").GetString());
                Assert.Equal(bool.FalseString, rootMetadata.GetProperty("model_server_upload_verified").GetString());
                Assert.Equal(bool.FalseString, rootMetadata.GetProperty("external_cloud_upload_attempted").GetString());
                Assert.Equal(bool.FalseString, rootMetadata.GetProperty("deployment_verified").GetString());
                Assert.Equal(bool.FalseString, rootMetadata.GetProperty("product_ready").GetString());

                var replayLifecycleStatus = root.GetProperty("lifecycleStatus");
                Assert.True(replayLifecycleStatus.GetProperty("uploadRequested").GetBoolean());
                Assert.False(replayLifecycleStatus.GetProperty("uploadAttempted").GetBoolean());
                Assert.False(replayLifecycleStatus.GetProperty("deploymentVerified").GetBoolean());
                Assert.False(replayLifecycleStatus.GetProperty("productReady").GetBoolean());
                return;
            }

            Assert.Equal(expectedRepositoryCalls, calls);
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("upload_attempted").GetString());
            Assert.Equal(bool.TrueString, rootMetadata.GetProperty("model_server_upload_verified").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("external_cloud_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("local_only_lifecycle_status").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, rootMetadata.GetProperty("product_ready").GetString());

            var localPromotion = root.GetProperty("localPromotion");
            Assert.False(localPromotion.GetProperty("localPromotionReady").GetBoolean());
            Assert.False(localPromotion.GetProperty("externalCloudUploadAttempted").GetBoolean());
            Assert.Equal(
                $"{RepositoryPromotionServerBaseUrl}/models/{Uri.EscapeDataString(DesignConversionRuntimeScaffoldCriticUnit.UnitId)}",
                localPromotion.GetProperty("promotionUploadRef").GetString());
            Assert.Equal(JsonValueKind.Null, localPromotion.GetProperty("localOnly").ValueKind);
            Assert.Equal(JsonValueKind.Null, localPromotion.GetProperty("promotedLocalModelId").ValueKind);

            var lifecycleStatus = root.GetProperty("lifecycleStatus");
            Assert.True(lifecycleStatus.GetProperty("uploadRequested").GetBoolean());
            Assert.True(lifecycleStatus.GetProperty("uploadAttempted").GetBoolean());
            Assert.Equal("model_server_upload_verified", lifecycleStatus.GetProperty("uploadStatus").GetString());
            Assert.Equal("not_external_cloud_repository_upload_verified", lifecycleStatus.GetProperty("cloudUploadStatus").GetString());
            Assert.Equal(
                $"{RepositoryPromotionServerBaseUrl}/models/{Uri.EscapeDataString(DesignConversionRuntimeScaffoldCriticUnit.UnitId)}",
                lifecycleStatus.GetProperty("promotionUploadRef").GetString());
            Assert.True(lifecycleStatus.GetProperty("deployRequested").GetBoolean());
            Assert.False(lifecycleStatus.GetProperty("deploymentAttempted").GetBoolean());
            Assert.False(lifecycleStatus.GetProperty("deploymentVerified").GetBoolean());
            Assert.Equal("blocked_no_deployment_proof_after_verified_upload", lifecycleStatus.GetProperty("deploymentStatus").GetString());
            Assert.False(lifecycleStatus.GetProperty("productReady").GetBoolean());
            Assert.False(lifecycleStatus.GetProperty("localPromotionReady").GetBoolean());
            Assert.False(lifecycleStatus.GetProperty("localOnly").GetBoolean());

            var lifecycleMetadata = lifecycleStatus.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("upload_attempted").GetString());
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("model_server_upload_attempted").GetString());
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("model_server_upload_verified").GetString());
            Assert.Equal(bool.TrueString, lifecycleMetadata.GetProperty("model_server_promotion_claimed").GetString());
            Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("external_cloud_upload_attempted").GetString());
            Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("local_only_lifecycle_status").GetString());
            Assert.Equal("model_server_upload_verified", lifecycleMetadata.GetProperty("upload_status").GetString());
            Assert.Equal("not_external_cloud_repository_upload_verified", lifecycleMetadata.GetProperty("cloud_upload_status").GetString());
            Assert.Equal("blocked_no_deployment_proof_after_verified_upload", lifecycleMetadata.GetProperty("deployment_status").GetString());
            Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, lifecycleMetadata.GetProperty("product_ready").GetString());

            var lifecycleEvidenceRefs = lifecycleStatus.GetProperty("evidenceRefs")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray();
            Assert.Contains("DESIGN_CONVERSION_MODEL_LIFECYCLE_COMMAND_SURFACE_REMOTE_UPLOAD_VERIFIED", lifecycleEvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_MODEL_SERVER_UPLOAD_VERIFIED_BY_CONFIGURED_UPLOADER", lifecycleEvidenceRefs);
            Assert.Contains(UploadReceiptEvidenceHashRef("MODEL_REPOSITORY_UPLOAD_BOTH_UNITS_SUCCEEDED"), lifecycleEvidenceRefs);
            Assert.Contains(UploadReceiptEvidenceHashRef("MODEL_REPOSITORY_FRESH_LOAD_SHA_VERIFIED_BOTH_UNITS"), lifecycleEvidenceRefs);
            Assert.Contains(UploadReceiptEvidenceHashRef("REMOTE_LOCATOR_REGISTERED_BOTH_UNITS"), lifecycleEvidenceRefs);
            Assert.Contains(UploadReceiptEvidenceHashRef("REMOTE_LOCATOR_LOCATED_BOTH_UNITS"), lifecycleEvidenceRefs);

            var lifecycleWarnings = lifecycleStatus.GetProperty("warnings")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray();
            Assert.Contains("continue_training_upload_status_remote_upload_verified_non_deployed", lifecycleWarnings);
            Assert.Contains("product_ready=false", lifecycleWarnings);
            Assert.DoesNotContain("upload_deploy_blocked_local_only", lifecycleWarnings);
            Assert.DoesNotContain("continue_training_upload_status_local_only", lifecycleWarnings);
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
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        using var legalTeacherAssociation =
            AssociateStartupIssuedLegalTeacherCapabilityForPromotionHost(
                policyBaseProvider);
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, dpoPolicyBaseProvider: null);
        var promotionOutputDirectory = Path.Combine(seeded.RootDirectory, "remote-promotion-failure-output");
        var uploadHandler = new CapturingPromotionUploadHttpHandler(responseMode);
        using var promotionHttpClient = new HttpClient(uploadHandler, disposeHandler: true);
        var hostOptions = CreatePromotionHostOptions(
            seeded.RootDirectory,
            promotionHttpClient,
            policyBaseProvider);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
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
            routeAdmissionOptions: CreatePromotionRouteAdmissionOptions(PromotionHostTenantId),
            measurementGateThresholdConfiguration: ProductionMeasurementGateThresholdConfiguration());

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(30)
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
            var actualErrorCode = errorDocument.RootElement.GetProperty("errorCode").GetString();
            Assert.True(
                string.Equals(expectedErrorCode, actualErrorCode, StringComparison.Ordinal),
                promote.BodyJson);
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
        var root = CreateOwnedTempPath("host-promotion-config-fail");
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
                    DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting(),
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

    [Theory]
    [InlineData("mixed-http-and-repository", "DESIGN_CONVERSION_PROMOTION_HOST_REMOTE_MODE_EXCLUSIVE_REQUIRED")]
    [InlineData("null-repository", "DESIGN_CONVERSION_PROMOTION_HOST_REPOSITORY_CONFIG_ATOMIC_REQUIRED")]
    [InlineData("null-locator", "DESIGN_CONVERSION_PROMOTION_HOST_REPOSITORY_CONFIG_ATOMIC_REQUIRED")]
    [InlineData("http-server-url", "DESIGN_CONVERSION_PROMOTION_HOST_REPOSITORY_SERVER_BASE_URL_HTTPS_REQUIRED")]
    [InlineData("query-server-url", "DESIGN_CONVERSION_PROMOTION_HOST_REPOSITORY_SERVER_BASE_URL_HTTPS_REQUIRED")]
    [InlineData("unsafe-path-server-url", "DESIGN_CONVERSION_PROMOTION_HOST_REPOSITORY_SERVER_BASE_URL_HTTPS_REQUIRED")]
    [InlineData("raw-traversal-server-url", "DESIGN_CONVERSION_PROMOTION_HOST_REPOSITORY_SERVER_BASE_URL_HTTPS_REQUIRED")]
    [InlineData("invalid-tenant", "DESIGN_CONVERSION_PROMOTION_HOST_TENANT_INVALID")]
    [InlineData("route-tenant-invalid", "DESIGN_CONVERSION_PROMOTION_HOST_ROUTE_ADMISSION_TENANT_INVALID")]
    [InlineData("tenant-mismatch", "DESIGN_CONVERSION_PROMOTION_HOST_ROUTE_ADMISSION_TENANT_MISMATCH")]
    public void CreateLocalRouteRegistry_HostStartupRepositoryPromotionConfig_FailsClosedBeforeRouteServingOrRepositoryLocatorOrSend(
        string scenario,
        string expectedCode)
    {
        var root = CreateOwnedTempPath("host-repository-promotion-config-fail");
        var calls = new List<string>();
        var repository = new RecordingTrainingIntakeModelRepository(calls);
        var locator = new RecordingTrainingIntakeRemoteUnitLocator(calls);
        var uploadHandler = new CapturingPromotionUploadHttpHandler(PromotionUploadResponseMode.Success);
        using var promotionHttpClient = new HttpClient(uploadHandler, disposeHandler: true);
        var serverBaseUrl = scenario switch
        {
            "http-server-url" => "http://models.example/repository",
            "query-server-url" => "https://models.example/repository?token=secret",
            "unsafe-path-server-url" => "https://models.example/repository/%252fprivate",
            "raw-traversal-server-url" => "https://models.example/repository/../private",
            _ => RepositoryPromotionServerBaseUrl
        };
        var repositoryOptions = new DesignConversionModelServerRepositoryPromotionOptions(
            scenario == "null-repository" ? null! : repository,
            scenario == "null-locator" ? null! : locator,
            serverBaseUrl,
            scenario == "invalid-tenant" ? " " : PromotionHostTenantId);
        var hostOptions = scenario == "mixed-http-and-repository"
            ? new DesignConversionModelServerPromotionHostOptions(
                root,
                promotionHttpClient,
                CreatePromotionHttpUploaderOptions(root, PromotionHostTenantId),
                repositoryOptions)
            : new DesignConversionModelServerPromotionHostOptions(
                root,
                RepositoryPromotionOptions: repositoryOptions);
        var routeAdmissionOptions = scenario switch
        {
            "route-tenant-invalid" => CreatePromotionRouteAdmissionOptions(" "),
            "tenant-mismatch" => CreatePromotionRouteAdmissionOptions("tenant-beta"),
            _ => CreatePromotionRouteAdmissionOptions(PromotionHostTenantId)
        };

        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                    DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting(),
                    new UnexpectedCandidateStrategy(),
                    hostOptions,
                    () => DateTimeOffset.UnixEpoch.AddMinutes(651),
                    routeAdmissionOptions: routeAdmissionOptions));

            Assert.Contains(expectedCode, exception.Message, StringComparison.Ordinal);
            Assert.DoesNotContain(PromotionHostBearerToken, exception.Message, StringComparison.Ordinal);
            Assert.DoesNotContain(RepositoryPromotionServerBaseUrl, exception.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("token=secret", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(calls);
            Assert.Empty(repository.UploadedManifests);
            Assert.Empty(locator.RegisteredByUnit);
            Assert.Empty(locator.LocatedByUnit);
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
        var dpoPolicyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        using var legalTeacherAssociation =
            AssociateStartupIssuedLegalTeacherCapabilityForPromotionHost(
                dpoPolicyBaseProvider);
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(privateMarker, dpoPolicyBaseProvider: null);
        var promotionOutputDirectory = Path.Combine(
            seeded.RootDirectory,
            "host-local-only-promotion-output");
        var hostOptions = new DesignConversionModelServerPromotionHostOptions(
            seeded.RootDirectory,
            DpoPolicyBaseProvider: dpoPolicyBaseProvider);
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting();
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
            () => DateTimeOffset.UnixEpoch.AddMinutes(631),
            measurementGateThresholdConfiguration: ProductionMeasurementGateThresholdConfiguration());

        await app.StartAsync();
        try
        {
            AssertTrainingIntakeStartupMetadata(app, externalProviderConfigured: false);
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(30)
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
            Assert.True(dpoPolicyBaseProvider.InvocationCount > 0);
            Assert.NotNull(dpoPolicyBaseProvider.LastModelSet);
            var localPromotionEvidenceRefs = localPromotion.GetProperty("evidenceRefs")
                .EnumerateArray()
                .Select(evidence => evidence.GetString())
                .Where(evidence => !string.IsNullOrWhiteSpace(evidence))
                .ToArray();
            Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED", localPromotionEvidenceRefs);
            Assert.Contains(
                $"dpo_policy_base_provider_kind:{dpoPolicyBaseProvider.LastModelSet!.Identity.ProviderKind}",
                localPromotionEvidenceRefs);
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
    public async Task PostTrainingIntake_HostStartupUnassociatedCustomProviderFailsLegalTeacherBoundaryBeforeProviderOrPromotionArtifacts()
    {
        const string privateMarker =
            "PRIVATE_TRAINING_INTAKE_UNASSOCIATED_PROVIDER_SECRET";
        const string tenantId =
            "tenant:modelserver-training-intake-unassociated-provider";
        var admittedButUnassociatedAuthority =
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted();
        Assert.True(admittedButUnassociatedAuthority.IsAvailable);
        var customProvider = new DesignConversionDpoPolicyBaseTestProvider();
        Assert.Null(
            DesignConversionRetainedDpoPolicyBaseActivation
                .ResolveLegalTeacherAdmissionAuthority(customProvider));
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            dpoPolicyBaseProvider: null);
        var promotionOutputDirectory = Path.Combine(
            seeded.RootDirectory,
            "unassociated-provider-promotion-output");
        var hostOptions = new DesignConversionModelServerPromotionHostOptions(
            seeded.RootDirectory,
            DpoPolicyBaseProvider: customProvider);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(
            DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting(),
            new UnexpectedCandidateStrategy(),
            hostOptions,
            () => DateTimeOffset.UnixEpoch.AddMinutes(632),
            measurementGateThresholdConfiguration:
                ProductionMeasurementGateThresholdConfiguration());

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
                tenantId,
                idempotencyKey = "idempotency:unassociated-provider-create",
                handoff = seeded.Handoff,
                requestedByRef = "architect:unassociated-provider-test",
                reason = "custom provider must not inherit an admitted but unassociated authority"
            });
            using var createResponse = create.Response;
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            using var createDocument = JsonDocument.Parse(create.BodyJson);
            var intakeId = createDocument.RootElement
                .GetProperty("intakeId")
                .GetString();

            var promote = await PostTrainingIntakeAsync(client, new
            {
                action = DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal,
                tenantId,
                idempotencyKey = "idempotency:unassociated-provider-promote",
                intakeId,
                promotionOutputDirectory,
                options = BuildTrainingIntakeActualRouteOptions(
                    "training-intake-unassociated-provider",
                    "training-intake-unassociated-provider-dataset",
                    "training-intake-unassociated-provider-gate",
                    sourceDisjoint: true)
            });
            using var promoteResponse = promote.Response;

            Assert.Equal(HttpStatusCode.BadRequest, promoteResponse.StatusCode);
            using var errorDocument = JsonDocument.Parse(promote.BodyJson);
            Assert.Equal(
                ContinuousLearning.LegalTrainingTeacherAdmissionAuthority
                    .ConfigurationRequiredErrorCode,
                errorDocument.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(0, customProvider.InvocationCount);
            Assert.Null(customProvider.LastModelSet);
            Assert.Null(
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(customProvider));
            Assert.False(Directory.Exists(promotionOutputDirectory));
            Assert.False(File.Exists(Path.Combine(
                seeded.RootDirectory,
                "training-intake-local-promotions.jsonl")));
            Assert.Empty(Directory.EnumerateFiles(
                seeded.RootDirectory,
                "*contract.json",
                SearchOption.AllDirectories));
            AssertNoPrivateTrainingIntakePayload(promote.BodyJson, privateMarker);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(seeded.RootDirectory);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_AspNetMapper_TamperedPartialSourceManifestHandoffIsRejectedBeforePromotion()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ASPNET_PARTIAL_MANIFEST_NEGATIVE_SECRET";
        const string tenantId = "tenant:modelserver-training-intake-aspnet-partial-manifest-negative";
        const string createIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-partial-manifest-negative-create";
        var seeded = await CreatePartialSourceManifestApprovedHandoffAsync(privateMarker);
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

            Assert.Equal(
                HttpStatusCode.BadRequest,
                createdResponse.StatusCode);
            Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
            Assert.True(baseAddress.Port > 0);
            AssertLocalOnlyAspNetHeaders(createdResponse);
            AssertNoPrivateTrainingIntakePayload(created.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, created.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(createIdempotencyKey, created.BodyJson, StringComparison.Ordinal);
            using var rejectedDocument =
                JsonDocument.Parse(created.BodyJson);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_POST_EVALUATION_INELIGIBLE",
                rejectedDocument.RootElement
                    .GetProperty("errorCode")
                    .GetString());
            await AssertTrainingIntakeCountAsync(seeded.Db, expectedCount: 0);
            var promotionAuthority =
                Assert.IsAssignableFrom<IDesignConversionLocalPromotionActivationAuthority>(seeded.Db);
            var promotions = await promotionAuthority.ListTrainingIntakeLocalPromotionRecordsAsync();
            Assert.True(promotions.IsSuccess, promotions.ErrorMessage);
            Assert.Empty(promotions.Value!);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(seeded.RootDirectory);
        }
    }

    [Fact]
    public async Task PostTrainingIntake_AspNetMapper_TamperedFullyMissingSourceManifestHandoffIsRejectedBeforePromotion()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ASPNET_FULLY_MISSING_MANIFEST_NEGATIVE_SECRET";
        const string tenantId = "tenant:modelserver-training-intake-aspnet-fully-missing-manifest-negative";
        const string createIdempotencyKey = "idempotency:modelserver-training-intake-aspnet-fully-missing-manifest-negative-create";
        var seeded = await CreateFullyMissingSourceManifestApprovedHandoffAsync(privateMarker);
        Assert.All(seeded.Handoff.Rows, row => Assert.True(string.IsNullOrWhiteSpace(row.SourceManifestRef)));
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

            Assert.Equal(
                HttpStatusCode.BadRequest,
                createdResponse.StatusCode);
            Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
            Assert.True(baseAddress.Port > 0);
            AssertLocalOnlyAspNetHeaders(createdResponse);
            AssertNoPrivateTrainingIntakePayload(created.BodyJson, privateMarker);
            Assert.DoesNotContain(tenantId, created.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(createIdempotencyKey, created.BodyJson, StringComparison.Ordinal);
            using var rejectedDocument =
                JsonDocument.Parse(created.BodyJson);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_POST_EVALUATION_INELIGIBLE",
                rejectedDocument.RootElement
                    .GetProperty("errorCode")
                    .GetString());
            await AssertTrainingIntakeCountAsync(seeded.Db, expectedCount: 0);
            var promotionAuthority =
                Assert.IsAssignableFrom<IDesignConversionLocalPromotionActivationAuthority>(seeded.Db);
            var promotions = await promotionAuthority.ListTrainingIntakeLocalPromotionRecordsAsync();
            Assert.True(promotions.IsSuccess, promotions.ErrorMessage);
            Assert.Empty(promotions.Value!);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(seeded.RootDirectory);
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
        var promotionOutputDirectory = CreateOwnedTempPath(
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
    public void DescribeTrainingIntakeRoute_DocumentsRouteDependentRemotePromotionRefsOnlyContract()
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
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "localArtifactRecordId", Required: false, MayCarryPrivateInput: false, ResponseMayEchoRawValue: true });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "candidateOrTrainingRecordId", Required: false, MayCarryPrivateInput: false, ResponseMayEchoRawValue: true });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "query", Required: false, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionList, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionPreflight, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionRun, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionPromoteLocal, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionContinueTrainingUploadStatus, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionRollbackLocalPromotion, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionApproveRuntimeCandidate, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionExportApprovedRuntimeCandidates, descriptor.SupportedActions);
        Assert.Contains(DesignConversionModelServerTrainingIntakeHandler.ActionPostApprovalPreflight, descriptor.SupportedActions);
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "intakes", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "preflight", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "approvedRuntimeDpoPreflight", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "approvedRuntimeDpoPreflight.persistedRequestedAtUtc", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "approvedRuntimeDpoPreflight.trainingRunnerInvoked", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "trainingRun", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "localPromotion", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "lifecycleStatus", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "candidateApproval", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "candidateApproval.approvalReasonRef", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "approvedHandoff", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "approvedHandoff.rows", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "lifecycleStatus.promotedCheckpointPathRef", Required: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "lifecycleStatus.promotedRuntimeModelId", Required: false });
        var productReadyField = Assert.Single(
            descriptor.ResponseFields,
            field => string.Equals(field.JsonName, "productReady", StringComparison.Ordinal));
        Assert.DoesNotContain("local-only route", productReadyField.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("separate deployment proof", productReadyField.Description, StringComparison.OrdinalIgnoreCase);

        var routeLocalOnlyField = Assert.Single(
            descriptor.ResponseFields,
            field => string.Equals(field.JsonName, "boundaryMetadata.training_intake_route_local_only", StringComparison.Ordinal));
        Assert.True(routeLocalOnlyField.Required);
        Assert.DoesNotContain("Always true", routeLocalOnlyField.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("remote", routeLocalOnlyField.Description, StringComparison.OrdinalIgnoreCase);
        var remotePromotionConfiguredField = Assert.Single(
            descriptor.ResponseFields,
            field => string.Equals(field.JsonName, "boundaryMetadata.training_intake_remote_promotion_configured", StringComparison.Ordinal));
        Assert.Equal("boolean", remotePromotionConfiguredField.DotNetType);
        Assert.True(remotePromotionConfiguredField.Required);
        Assert.Contains("Runtime route flag", remotePromotionConfiguredField.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Always false", remotePromotionConfiguredField.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.training_intake_continue_training_upload_status_local_only", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.training_intake_remote_promotion_status_metadata_supported", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.private_file_paths_included", Required: true });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_ID_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_AUTH_TOKEN_NOT_CONFIGURED", StatusCode: 503 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_AUTH_TOKEN_REQUIRED", StatusCode: 401 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_AUTH_TOKEN_INVALID", StatusCode: 401 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_LOCAL_ARTIFACT_RECORD_ID_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_DPO_CANDIDATE_ID_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_APPROVAL_REVIEWER_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_APPROVAL_REASON_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_APPROVED_RUNTIME_DPO_PREFLIGHT_REQUESTER_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_APPROVED_RUNTIME_DPO_PREFLIGHT_REASON_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "TRAINING_INTAKE_LIFECYCLE_REASON_REQUIRED", StatusCode: 400 });
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_route_discoverable_by_contract"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_route_local_only"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["training_intake_remote_promotion_configured"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.TrainingIntakeRouteDependentRemotePromotionConfigurationMetadataValue,
            descriptor.BoundaryMetadata["training_intake_route_local_only_descriptor_semantics"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.TrainingIntakeRouteDependentRemotePromotionConfigurationMetadataValue,
            descriptor.BoundaryMetadata["training_intake_remote_promotion_configured_descriptor_semantics"]);
        Assert.NotEqual(
            DesignConversionModelServerCallerContract.TrainingIntakeRouteDependentRemotePromotionConfigurationMetadataValue,
            descriptor.BoundaryMetadata["training_intake_route_local_only"]);
        Assert.NotEqual(
            DesignConversionModelServerCallerContract.TrainingIntakeRouteDependentRemotePromotionConfigurationMetadataValue,
            descriptor.BoundaryMetadata["training_intake_remote_promotion_configured"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_remote_promotion_configuration_supported"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_response_refs_only"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_paths_returned_as_hash_refs"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_continue_training_upload_status_supported"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_local_promotion_rollback_supported"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_runtime_candidate_approval_supported"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_approved_runtime_export_supported"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_approved_runtime_dpo_preflight_supported"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_runtime_candidate_approval_requires_local_auth_token"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_approved_runtime_export_requires_local_auth_token"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_approved_runtime_dpo_preflight_requires_local_auth_token"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_continue_training_upload_status_local_only"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.TrainingIntakeRouteDependentRemotePromotionStatusMetadataValue,
            descriptor.BoundaryMetadata["training_intake_continue_training_upload_status_descriptor_semantics"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_continue_training_upload_status_remote_promotion_supported"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["training_intake_remote_promotion_status_metadata_supported"]);
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
        Assert.DoesNotContain("/tmp/", value!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/root/", value!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/home/", value!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\/tmp\/", value!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\/root\/", value!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\/home\/", value!, StringComparison.OrdinalIgnoreCase);
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
        Assert.DoesNotContain("C:/Users/lubao", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.png", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_provider_payload", bodyJson, StringComparison.OrdinalIgnoreCase);
        AssertPublicTrainingIntakePayloadDoesNotLeakLocalPathShapes(bodyJson);
    }

    private static void AssertPublicTrainingIntakePayloadDoesNotLeakLocalPathShapes(string bodyJson)
    {
        // Public JSON должен содержать только refs/hash-refs: эти проверки ловят
        // прямые и JSON-escaped локальные пути до того, как они попадут к caller-у.
        Assert.DoesNotContain("/tmp/", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/root/", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/home/", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\/tmp\/", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\/root\/", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\/home\/", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"C:\Users\", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"C:\\Users\\", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\\server\share", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\\\\server\\share", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("file://", bodyJson, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoServerOwnedMeasurementGateThresholdPayload(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var normalizedName = NormalizeJsonPropertyName(property.Name);
                    if (IsPrivateMeasurementThresholdPropertyName(normalizedName))
                    {
                        throw new Xunit.Sdk.XunitException(
                            $"Server-owned measurement threshold property '{normalizedName}' must not be serialized.");
                    }

                    if (IsPrivateMeasurementThresholdObjectName(normalizedName)
                        || (normalizedName == "thresholds"
                            && LooksLikePrivateMeasurementThresholdPayload(property.Value)))
                    {
                        throw new Xunit.Sdk.XunitException(
                            "Server-owned measurement threshold object must not be serialized.");
                    }

                    AssertNoServerOwnedMeasurementGateThresholdPayload(property.Value);
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    AssertNoServerOwnedMeasurementGateThresholdPayload(item);
                break;
        }
    }

    private static bool IsPrivateMeasurementThresholdPropertyName(string normalizedName)
        => normalizedName is
            "requiredheldoutqualityscore"
            or "maxruntimems"
            or "maxcostusd"
            or "maxpeakrammb";

    private static bool IsPrivateMeasurementThresholdObjectName(string normalizedName)
        => normalizedName is
            "measurementgatethresholds"
            or "measurementgatethresholdconfiguration"
            or "serverownedmeasurementgatethresholds";

    private static bool LooksLikePrivateMeasurementThresholdPayload(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var hasQuality = false;
        var hasRuntime = false;
        var hasCost = false;
        var hasRam = false;
        foreach (var property in element.EnumerateObject())
        {
            switch (NormalizeJsonPropertyName(property.Name))
            {
                case "quality":
                case "qualityscore":
                case "heldoutquality":
                case "heldoutscore":
                    hasQuality = true;
                    break;
                case "runtime":
                case "runtimems":
                    hasRuntime = true;
                    break;
                case "cost":
                case "costusd":
                    hasCost = true;
                    break;
                case "ram":
                case "rammb":
                case "peakram":
                case "peakrammb":
                    hasRam = true;
                    break;
            }
        }

        return hasQuality && hasRuntime && hasCost && hasRam;
    }

    private static string NormalizeJsonPropertyName(string propertyName)
    {
        var normalized = new StringBuilder(propertyName.Length);
        foreach (var character in propertyName)
        {
            if (char.IsLetterOrDigit(character))
                normalized.Append(char.ToLowerInvariant(character));
        }

        return normalized.ToString();
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

    private static async Task AssertRegistryUsesConfiguredMeasurementGateThresholdsAsync(
        DesignConversionModelServerEndpointRegistry registry,
        string tenantId,
        string intakeId,
        string scenario)
    {
        var missingMeasurementRequestResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler.ActionRun,
                IntakeId: intakeId,
                TenantId: tenantId,
                IdempotencyKey:
                    $"idempotency:startup-threshold-missing:{scenario}:{tenantId}",
                Options: new DesignConversionTrainingPipelineOptions(
                    $"startup-threshold-missing-{scenario}",
                    1)));

        Assert.Equal(400, missingMeasurementRequestResponse.StatusCode);
        Assert.Equal(
            "TRAINING_INTAKE_MEASUREMENT_GATE_REQUEST_REQUIRED",
            ReadResponseErrorCode(missingMeasurementRequestResponse));

        var callerMeasurementGate = BuildPassingProductionCallerMeasurementGate(
            $"startup-threshold-propagation-{scenario}",
            sourceDisjoint: true) with
        {
            Thresholds = new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0d,
                MaxRuntimeMs: DesignConversionMeasurementAdmission.MaxReportedRuntimeMs,
                MaxCostUsd: DesignConversionMeasurementAdmission.MaxReportedCostUsd,
                MaxPeakRamMb: DesignConversionMeasurementAdmission.MaxReportedPeakRamMb)
        };
        var callerOptions = new DesignConversionTrainingPipelineOptions(
            RunId: $"startup-threshold-propagation-{scenario}",
            BatchSize: 25,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: $"startup-threshold-propagation-{scenario}",
                MinimumTrainRecords: 50,
                MinimumHeldOutRecords: 50,
                Preflight:
                    DesignConversionDpoDatasetPreflight
                        .LocalTrainingIntakeSourceManifestDisjointProof()),
            MeasurementGateRequest: callerMeasurementGate);
        var configuredThresholdResponse = await InvokeTrainingIntakeRouteAsync(
            registry,
            new DesignConversionModelServerTrainingIntakeRequest(
                Action: DesignConversionModelServerTrainingIntakeHandler.ActionRun,
                IntakeId: intakeId,
                TenantId: tenantId,
                IdempotencyKey:
                    $"idempotency:startup-threshold-propagation:{scenario}:{tenantId}",
                Options: callerOptions));

        Assert.True(configuredThresholdResponse.StatusCode == 200, configuredThresholdResponse.BodyJson);
        using var document = JsonDocument.Parse(configuredThresholdResponse.BodyJson);
        var trainingRun = document.RootElement.GetProperty("trainingRun");
        Assert.Equal(
            DesignConversionMeasurementGateStatus.Failed,
            trainingRun.GetProperty("measurementGateStatus").GetString());
        Assert.Equal(
            DesignConversionMeasurementGateStatus.Failed,
            trainingRun
                .GetProperty("runnerEvidence")
                .GetProperty("measurementGateStatus")
                .GetString());
        Assert.Equal(0d, callerMeasurementGate.Thresholds.RequiredHeldOutQualityScore);
    }

    private const string PublicSanitizerCanonicalSha256Ref =
        "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

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

    private async Task<SeededTrainingIntakeContext> CreateSeededIntakeAsync(string privateMarker)
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

    private async Task<SeededApprovedHandoffContext> CreateApprovedHandoffAsync(string privateMarker)
    {
        var root = CreateOwnedTempPath("modelserver-training-intake-route");
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
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

    private async Task<SeededApprovedHandoffContext> CreateSingleSourcePersistedApprovedHandoffAsync(string privateMarker)
    {
        var root = CreateOwnedTempPath("modelserver-training-intake-single-source-route");
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
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

    private async Task<SeededApprovedHandoffContext> CreateSourceDisjointApprovedHandoffAsync(
        string privateMarker,
        IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider = null,
        IDesignConversionPromotionUploader? promotionUploader = null,
        bool bridgeBalancedTopRows = false,
        bool bridgeBalancedHeldOutRows = false,
        bool bridgeEvaluatedDpoContrastRows = false)
    {
        var root = CreateOwnedTempPath("modelserver-training-intake-lifecycle-route");
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            promotionUploader: promotionUploader,
            dpoPolicyBaseProvider: dpoPolicyBaseProvider,
            legalTeacherAdmissionAuthority:
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
        if (dpoPolicyBaseProvider is not null)
            Assert.Same(dpoPolicyBaseProvider, ReadDpoPolicyBaseProvider(db));
        var api = new DesignConversionDpoUsefulnessApi(db);
        var sourceRecords = BuildSourceDisjointTrainingRecords(
            100,
            bridgeBalancedTopRows,
            bridgeBalancedHeldOutRows: bridgeBalancedHeldOutRows,
            bridgeEvaluatedDpoContrastRows: bridgeEvaluatedDpoContrastRows);
        var teacherRequestIds = ReadTeacherRequestIds(sourceRecords);
        var records = sourceRecords
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

        return new SeededApprovedHandoffContext(
            db,
            handoff.Value,
            root,
            teacherRequestIds);
    }

    private async Task<FreshApprovedHandoffContext>
        AppendSourceDisjointApprovedHandoffAsync(
            IDesignConversionLocalArtifactDb artifactDb,
        string privateMarker,
        string sourceNamespace,
        bool bridgeBalancedTopRows = false)
    {
        var db = Assert.IsType<JsonlDesignConversionLocalArtifactDb>(artifactDb);
        var api = new DesignConversionDpoUsefulnessApi(db);
        var sourceRecords = BuildSourceDisjointTrainingRecords(
            100,
            bridgeBalancedTopRows,
            sourceNamespace);
        var teacherRequestIds = ReadTeacherRequestIds(sourceRecords);
        var records = sourceRecords
            .Select(ToRuntimeApprovedCandidate)
            .ToArray();
        var jobId = $"{sourceNamespace}-job";
        await Save(db, BuildArtifactRecord(
            $"{sourceNamespace}-record",
            jobId,
            $"{sourceNamespace}-request",
            records,
            privateMarker));

        var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: jobId),
            DateTimeOffset.UnixEpoch.AddMinutes(700));
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.Equal(100, handoff.Value!.ApprovedCandidateCount);

        return new FreshApprovedHandoffContext(handoff.Value, teacherRequestIds);
    }

    private async Task<SeededApprovedHandoffContext> CreatePartialSourceManifestApprovedHandoffAsync(string privateMarker)
    {
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            dpoPolicyBaseProvider: null);
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

    private async Task<SeededApprovedHandoffContext> CreateFullyMissingSourceManifestApprovedHandoffAsync(string privateMarker)
    {
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            dpoPolicyBaseProvider: null);
        var rows = seeded.Handoff.Rows
            .Select(row => row with { SourceManifestRef = string.Empty })
            .ToArray();

        return new SeededApprovedHandoffContext(
            seeded.Db,
            seeded.Handoff with { Rows = rows },
            seeded.RootDirectory);
    }

    private static async Task Save(
        JsonlDesignConversionLocalArtifactDb db,
        DesignConversionLocalArtifactDbRecord record)
    {
        _ = await DesignConversionPostEvaluationEligibilityTestFixture
            .AppendEligibleRecordAsync(db, record);
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

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildSourceDisjointTrainingRecords(
        int count,
        bool bridgeBalancedTopRows = false,
        string sourceNamespace = "training-intake-lifecycle",
        bool bridgeBalancedHeldOutRows = false,
        bool bridgeEvaluatedDpoContrastRows = false)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            Enumerable.Range(0, count)
                .Select(index =>
                {
                    var unitId = index % 2 == 0
                        ? DesignConversionDpoUnits.CriticUnit
                        : DesignConversionDpoUnits.ImproverUnit;
                    var sourceId = $"{sourceNamespace}-source-{index:000}";
                    var usefulness = bridgeBalancedTopRows &&
                                     (index < 8 || bridgeBalancedHeldOutRows)
                        ? 0.99 - (index % 8 * 0.01)
                        : 0.80 + (index % 10 * 0.01);
                    var example = DesignConversionDpoGeneratorTests.CreateExample(
                        $"{sourceNamespace}-record-{index:000}",
                        unitId,
                        usefulness);
                    if (bridgeEvaluatedDpoContrastRows)
                    {
                        example = example with
                        {
                            SourceStructurePayload = BuildTrainingIntakeEvaluatedDpoContrastSourceStructurePayload(index),
                            Chosen = DesignConversionDpoGeneratorTests.CreateOutput(
                                $"{sourceNamespace}-record-{index:000}-approved-evaluated-dpo-contrast",
                                accepted: true,
                                score: 0.98,
                                htmlSuffix: "approved-evaluated-dpo-contrast"),
                            Rejected = DesignConversionDpoGeneratorTests.CreateOutput(
                                $"{sourceNamespace}-record-{index:000}-rejected-evaluated-dpo-contrast",
                                accepted: false,
                                score: 0.02,
                                htmlSuffix: "rejected-evaluated-dpo-contrast")
                        };
                    }

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
                $"{sourceNamespace}-artifact-{index:000}"))
            .ToArray();
    }

    private static string BuildTrainingIntakeEvaluatedDpoContrastSourceStructurePayload(int index)
        => JsonSerializer.Serialize(new
        {
            schema = "training-intake-evaluated-dpo-contrast-source-v1",
            layout = new
            {
                frames = new[] { "hero-frame", "content-frame", "cta-frame" },
                components = new[] { "primary-button", "secondary-button", "responsive-card" },
                constraints = new[] { "grid", "flex", "responsive-breakpoint", "width", "height" },
                text = new[] { "headline", "supporting-copy", "button-label" },
                assets = new[] { "icon", "image" },
                ordinal_bucket = index % 8
            },
            redacted = true
        }, DesignConversionCheckpointJsonOptions.Standard);

    private static DesignConversionDpoPolicyBaseTestProvider
        CreateTrainingIntakeRepositoryPromotionPolicyBaseProvider()
        => new(modelSet =>
        {
            SetTrainingIntakeRepositoryPromotionDeterministicPolicyBaseState(modelSet.Policy);
            DesignConversionDpoPolicyBaseTestProvider.CopyParameters(
                modelSet.Policy,
                modelSet.Reference);
            DesignConversionDpoPolicyBaseTestProvider.CopyParameters(
                modelSet.Policy,
                modelSet.FreshLoadTarget);

            AssertTrainingIntakeRepositoryPromotionPolicyBaseModelsBitwiseEqual(
                modelSet.Policy,
                modelSet.Reference);
            AssertTrainingIntakeRepositoryPromotionPolicyBaseModelsBitwiseEqual(
                modelSet.Policy,
                modelSet.FreshLoadTarget);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet);
        });

    private static void SetTrainingIntakeRepositoryPromotionDeterministicPolicyBaseState(
        LlmModel model)
    {
        var rng = new Random(169);
        var baseParameters = model.BaseParameters.ToArray();
        Assert.Equal(12, baseParameters.Length);
        FillTrainingIntakeRepositoryPromotionNormal(baseParameters[0].Value.Data, rng, std: 0.02f);
        FillTrainingIntakeRepositoryPromotionOnes(baseParameters[1].Value.Data);
        FillTrainingIntakeRepositoryPromotionKaiming(
            baseParameters[2].Value.Data,
            rng,
            baseParameters[2].Value.Shape.Dimensions[1]);
        FillTrainingIntakeRepositoryPromotionKaiming(
            baseParameters[3].Value.Data,
            rng,
            baseParameters[3].Value.Shape.Dimensions[1]);
        FillTrainingIntakeRepositoryPromotionKaiming(
            baseParameters[4].Value.Data,
            rng,
            baseParameters[4].Value.Shape.Dimensions[1]);
        FillTrainingIntakeRepositoryPromotionKaiming(
            baseParameters[5].Value.Data,
            rng,
            baseParameters[5].Value.Shape.Dimensions[1]);
        FillTrainingIntakeRepositoryPromotionOnes(baseParameters[6].Value.Data);
        FillTrainingIntakeRepositoryPromotionKaiming(
            baseParameters[7].Value.Data,
            rng,
            baseParameters[7].Value.Shape.Dimensions[1]);
        FillTrainingIntakeRepositoryPromotionKaiming(
            baseParameters[8].Value.Data,
            rng,
            baseParameters[8].Value.Shape.Dimensions[1]);
        FillTrainingIntakeRepositoryPromotionKaiming(
            baseParameters[9].Value.Data,
            rng,
            baseParameters[9].Value.Shape.Dimensions[1]);
        FillTrainingIntakeRepositoryPromotionOnes(baseParameters[10].Value.Data);
        FillTrainingIntakeRepositoryPromotionKaiming(
            baseParameters[11].Value.Data,
            rng,
            baseParameters[11].Value.Shape.Dimensions[1]);

        foreach (var entry in model.GetCheckpointParameterEntries())
        {
            if (entry.Key.EndsWith(".lora_a", StringComparison.Ordinal))
            {
                for (var index = 0; index < entry.Parameter.Value.Data.Length; index++)
                {
                    entry.Parameter.Value.Data[index] =
                        ((index % 7) - 3) * 0.03125f;
                }
            }
            else if (entry.Key.EndsWith(".lora_b", StringComparison.Ordinal))
            {
                Array.Clear(entry.Parameter.Value.Data);
            }
            else
            {
                Assert.Contains(
                    baseParameters,
                    parameter => ReferenceEquals(parameter, entry.Parameter));
            }

            Assert.All(entry.Parameter.Value.Data, value => Assert.True(float.IsFinite(value)));
        }
    }

    private static void FillTrainingIntakeRepositoryPromotionKaiming(
        float[] data,
        Random rng,
        int fanIn)
    {
        var bound = MathF.Sqrt(6.0f / fanIn);
        for (var index = 0; index < data.Length; index++)
            data[index] = (float)(rng.NextDouble() * 2 * bound - bound);
    }

    private static void FillTrainingIntakeRepositoryPromotionNormal(
        float[] data,
        Random rng,
        float std)
    {
        for (var index = 0; index < data.Length; index += 2)
        {
            var u1 = 1.0 - rng.NextDouble();
            var u2 = 1.0 - rng.NextDouble();
            var radius = Math.Sqrt(-2.0 * Math.Log(u1));
            var theta = 2.0 * Math.PI * u2;
            data[index] = (float)(radius * Math.Cos(theta) * std);
            if (index + 1 < data.Length)
                data[index + 1] = (float)(radius * Math.Sin(theta) * std);
        }
    }

    private static void FillTrainingIntakeRepositoryPromotionOnes(float[] data)
        => Array.Fill(data, 1.0f);

    private static void AssertTrainingIntakeRepositoryPromotionPolicyBaseModelsBitwiseEqual(
        LlmModel expected,
        LlmModel actual)
    {
        var expectedEntries = expected.GetCheckpointParameterEntries();
        var actualEntries = actual.GetCheckpointParameterEntries();
        Assert.Equal(expectedEntries.Count, actualEntries.Count);
        for (var entryIndex = 0; entryIndex < expectedEntries.Count; entryIndex++)
        {
            var expectedEntry = expectedEntries[entryIndex];
            var actualEntry = actualEntries[entryIndex];
            Assert.Equal(expectedEntry.Key, actualEntry.Key);
            Assert.Equal(
                expectedEntry.Parameter.Value.Shape.Dimensions,
                actualEntry.Parameter.Value.Shape.Dimensions);
            Assert.NotSame(
                expectedEntry.Parameter.Value.Data,
                actualEntry.Parameter.Value.Data);
            Assert.Equal(
                expectedEntry.Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray(),
                actualEntry.Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }
    }

    private static IReadOnlyList<string> ReadTeacherRequestIds(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records)
    {
        var requestIds = records
            .Select(record => record.TeacherProvenance?.RequestId)
            .ToArray();
        Assert.All(requestIds, requestId => Assert.False(string.IsNullOrWhiteSpace(requestId)));
        Assert.Equal(requestIds.Length, requestIds.Distinct(StringComparer.Ordinal).Count());
        return requestIds.Select(requestId => requestId!).ToArray();
    }

    private static void AssertTeacherRequestIdsDisjoint(
        IReadOnlyList<string> first,
        IReadOnlyList<string> second)
        => Assert.Empty(first.Intersect(second, StringComparer.Ordinal));

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

    private static DesignConversionModelServerHttpRequest CreateProductionThresholdRequest(
        string action,
        DesignConversionTrainingPipelineOptions? options,
        string promotionOutputDirectory)
        => new(
            Method: "POST",
            Path: DesignConversionModelServerTrainingIntakeHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(
                new DesignConversionModelServerTrainingIntakeRequest(
                    Action: action,
                    IntakeId: "training-intake-server-threshold-test",
                    TenantId: "tenant:training-intake-server-threshold-test",
                    IdempotencyKey: $"idempotency:training-intake-server-threshold-test:{action}",
                    RequestedByRef: "architect:training-intake-server-threshold-test",
                    Reason: "verify production-owned measurement gate thresholds",
                    PromotionOutputDirectory: promotionOutputDirectory,
                    Options: options),
                DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json");

    private static DesignConversionModelServerHttpRequest CreateTrainingIntakeHttpRequest(
        DesignConversionModelServerTrainingIntakeRequest routeRequest)
        => new(
            Method: "POST",
            Path: DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            BodyJson: JsonSerializer.Serialize(
                routeRequest,
                DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json");

    private static DesignConversionModelServerMeasurementGateThresholdConfiguration
        ProductionMeasurementGateThresholdConfiguration()
        => new(
            RequiredHeldOutQualityScore: 0.90,
            MaxRuntimeMs: 800,
            MaxCostUsd: 1.00m,
            MaxPeakRamMb: 512);

    private static string ReadResponseErrorCode(
        DesignConversionModelServerHttpResponse response)
    {
        using var document = JsonDocument.Parse(response.BodyJson);
        return document.RootElement.GetProperty("errorCode").GetString()!;
    }

    private static IReadOnlyList<string> SnapshotFiles(string root)
    {
        if (!Directory.Exists(root))
            return [];

        return Directory
            .EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .Select(path =>
            {
                var relativePath = Path.GetRelativePath(root, path).Replace('\\', '/');
                var bytes = File.ReadAllBytes(path);
                return $"{relativePath}|{bytes.Length}|{Convert.ToHexString(SHA256.HashData(bytes))}";
            })
            .ToArray();
    }

    private static IReadOnlyList<string> SnapshotMaterialFiles(string root)
        => SnapshotFiles(root)
            .Where(entry => !entry.Contains(".mutation.lock|", StringComparison.Ordinal))
            .ToArray();

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
                EvidenceRefs: [$"ref:{gateId}:heldout-fixture"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: sourceDisjoint,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs: [$"ref:{gateId}:integrity-fixture"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private sealed record RepositoryOwnedDpoDatasetFixture(
        RepositoryOwnedDpoDatasetAddress Address,
        RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
        RepositoryOwnedDpoDatasetManifest Manifest);

    private static DesignConversionRepositoryOwnedTrainingRunContextSpec
        CreateRepositoryOwnedTrainingRunContextSpec(
            string repositoryRoot,
            string scope)
        => new(
            repositoryRoot,
            DomainSlug: "figma-html-css",
            DesignPatternSlug: "design-to-code",
            DatasetVersion: "v0001",
            ModelIdSlug: "qwen3-5-9b",
            ModelVersion: "v0001",
            RuntimeCompatibility:
            CreateRepositoryOwnedRuntimeCompatibility(scope),
            AllowColdStart: true,
            ColdStartReason:
            "first design-to-code checkpoint after sealed DPO",
            ProvenanceRefs:
            [$"test:design-conversion-modelserver-repository-owned-training:{scope}"]);

    private static RepositoryOwnedDpoDatasetFixture
        CreateSealedRepositoryOwnedDpoDataset(
            string repositoryRoot,
            string scope)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                repositoryRoot,
                "figma-html-css",
                "design-to-code",
                "v0001");
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var requestIdentity =
            CreateRepositoryOwnedDpoRequestIdentity(scope);
        var pair = CreateRepositoryOwnedDpoPair(requestIdentity, scope);
        var manifest = RepositoryOwnedDpoDatasetManifestSealer
            .PersistAndSeal(
                address.Value!,
                requestIdentity,
                [pair]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);

        return new RepositoryOwnedDpoDatasetFixture(
            address.Value!,
            requestIdentity,
            manifest.Value!);
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity
        CreateRepositoryOwnedDpoRequestIdentity(string scope)
    {
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [HashIdentity("paid DesignConversion reference " + scope)],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen-test:9b",
                    HexForTest('c'),
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    HexForTest('d')),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "design-conversion-modelserver-handler",
                    "v1",
                    HashIdentity("generation policy " + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "design-conversion-modelserver-training-intake-tests",
                    "v1",
                    HashIdentity("creator policy " + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    private static DpoPair CreateRepositoryOwnedDpoPair(
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        string scope)
    {
        var passport = new TrainingTeacherProvenance(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            identity.OpenTeacher.ModelId,
            identity.OpenTeacher.ModelVersion,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            "repository-owned-modelserver-handler-" + scope,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };

        var bound = DpoPairTrainingContentAuthority.Bind(new DpoPair(
            "convert Figma tree to HTML and CSS for " + scope,
            SerializeRepositoryOwnedModelSide(
                "<main class=\"card\"><h1>open teacher</h1></main>",
                ".card{display:grid;gap:1rem;}"),
            SerializeRepositoryOwnedModelSide(
                "<main><h1>local candidate</h1></main>",
                "main{display:block;}"),
            "figma-html-css",
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs: ["sha256:" + HashIdentity("governance " + scope)]),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.LocalModel)
        });
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        return bound.Value!;
    }

    private static void AssertRepositoryOwnedDpoContextMatches(
        RepositoryOwnedDpoDatasetFixture expected,
        DesignConversionRepositoryOwnedDpoTransactionContext? actual)
    {
        Assert.NotNull(actual);
        Assert.True(actual!.Enabled);
        Assert.NotNull(actual.Address);
        Assert.Equal(
            expected.Address.RepositoryRoot,
            actual.Address!.RepositoryRoot);
        Assert.Equal(
            expected.Address.DpoArtifactRoot,
            actual.Address.DpoArtifactRoot);
        Assert.Equal(expected.Address.DomainSlug, actual.Address.DomainSlug);
        Assert.Equal(
            expected.Address.DesignPatternSlug,
            actual.Address.DesignPatternSlug);
        Assert.Equal(
            expected.Address.DatasetVersion,
            actual.Address.DatasetVersion);
        Assert.Equal(
            expected.Address.ManifestRelativePath,
            actual.Address.ManifestRelativePath);
        Assert.NotNull(actual.RequestIdentity);
        Assert.Equal(
            expected.RequestIdentity.RequestIdentitySha256,
            actual.RequestIdentity!.RequestIdentitySha256);
        Assert.True(actual.Validate().IsSuccess);
    }

    private static void AssertRepositoryOwnedModelContextMatches(
        string repositoryRoot,
        string scope,
        DesignConversionRepositoryOwnedModelCheckpointContext? actual)
    {
        Assert.NotNull(actual);
        Assert.True(actual!.Enabled);
        Assert.NotNull(actual.Request);
        var request = actual.Request!;
        Assert.Equal(Path.GetFullPath(repositoryRoot), request.RepositoryRoot);
        Assert.Equal("figma-html-css", request.DomainSlug);
        Assert.Equal("design-to-code", request.DesignPatternSlug);
        Assert.Equal("qwen3-5-9b", request.ModelIdSlug);
        Assert.Equal("v0001", request.ModelVersion);
        Assert.Equal(
            CreateRepositoryOwnedRuntimeCompatibility(scope),
            request.RuntimeCompatibility);
        Assert.True(request.AllowColdStart);
        Assert.Equal(
            "first design-to-code checkpoint after sealed DPO",
            request.ColdStartReason);
        Assert.Equal(
            [$"test:design-conversion-modelserver-repository-owned-training:{scope}"],
            request.ProvenanceRefs);
        Assert.True(actual.Validate().IsSuccess);
    }

    private static RepositoryOwnedModelRuntimeCompatibility
        CreateRepositoryOwnedRuntimeCompatibility(string scope)
        => new(
            ConfigSha256: HexForTest('a'),
            TopologySha256: HexForTest('b'),
            TokenizerSha256: HexForTest('c'),
            TrainerSha256: HexForTest('d'),
            SchemaSha256: HexForTest('e'),
            CompatibilityKey: "design-conversion-modelserver-" + scope);

    private static IReadOnlyDictionary<string, string>
        BuildRepositoryOwnedTrainingEnvironment(
            string repositoryRoot,
            string scope)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ModelServer.Program
                .RepositoryOwnedTrainingEnabledEnvironmentName] =
                bool.TrueString,
            [ModelServer.Program
                .RepositoryOwnedTrainingRepositoryRootEnvironmentName] =
                repositoryRoot,
            [ModelServer.Program
                .RepositoryOwnedTrainingDomainSlugEnvironmentName] =
                "figma-html-css",
            [ModelServer.Program
                .RepositoryOwnedTrainingDesignPatternSlugEnvironmentName] =
                "design-to-code",
            [ModelServer.Program
                .RepositoryOwnedTrainingDatasetVersionEnvironmentName] =
                "v0001",
            [ModelServer.Program
                .RepositoryOwnedTrainingModelIdSlugEnvironmentName] =
                "qwen3-5-9b",
            [ModelServer.Program
                .RepositoryOwnedTrainingModelVersionEnvironmentName] =
                "v0001",
            [ModelServer.Program
                .RepositoryOwnedTrainingRuntimeConfigSha256EnvironmentName] =
                HexForTest('a'),
            [ModelServer.Program
                .RepositoryOwnedTrainingRuntimeTopologySha256EnvironmentName] =
                HexForTest('b'),
            [ModelServer.Program
                .RepositoryOwnedTrainingRuntimeTokenizerSha256EnvironmentName] =
                HexForTest('c'),
            [ModelServer.Program
                .RepositoryOwnedTrainingRuntimeTrainerSha256EnvironmentName] =
                HexForTest('d'),
            [ModelServer.Program
                .RepositoryOwnedTrainingRuntimeSchemaSha256EnvironmentName] =
                HexForTest('e'),
            [ModelServer.Program
                .RepositoryOwnedTrainingRuntimeCompatibilityKeyEnvironmentName] =
                "design-conversion-modelserver-" + scope,
            [ModelServer.Program
                .RepositoryOwnedTrainingAllowColdStartEnvironmentName] =
                bool.TrueString,
            [ModelServer.Program
                .RepositoryOwnedTrainingColdStartReasonEnvironmentName] =
                "first design-to-code checkpoint after sealed DPO",
            [ModelServer.Program
                .RepositoryOwnedTrainingProvenanceRefsEnvironmentName] =
                $"test:design-conversion-modelserver-repository-owned-training:{scope}"
        };

    private static void CreateRepositoryOwnedTrainingSentinels(
        string repositoryRoot)
    {
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "DesignConversion model-server repository-owned training fixture");
        Directory.CreateDirectory(
            Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(
            Path.Combine(repositoryRoot, "11.DPO"));
    }

    private static string RepositoryOwnedModelArtifactRoot(
        string repositoryRoot)
        => Path.Combine(repositoryRoot, "training-artifacts", "models");

    private static string SerializeRepositoryOwnedModelSide(
        string html,
        string css)
        => JsonSerializer.Serialize(new { html, css });

    private static string HexForTest(char value) => new(value, 64);

    private static string HashIdentity(string value)
        => ComputeSha256Hex(Encoding.UTF8.GetBytes(value));

    private static DesignConversionMeasurementGateRequest BuildPassingProductionCallerMeasurementGate(
        string gateId,
        bool sourceDisjoint = true)
    {
        var measurement = BuildPassingLifecycleMeasurementGate(gateId, sourceDisjoint);
        return measurement with
        {
            HeldOutMetrics = measurement.HeldOutMetrics! with
            {
                HeldOutQualityScore = 1.0,
                EvidenceRefs =
                [
                    $"ref:{gateId}:heldout-fixture",
                    $"ref:{gateId}:production-caller-admission:truthful-pass"
                ]
            },
            IntegrityProof = measurement.IntegrityProof! with
            {
                EvidenceRefs =
                [
                    $"ref:{gateId}:integrity-fixture",
                    $"ref:{gateId}:production-caller-admission:source-disjoint:{sourceDisjoint.ToString().ToLowerInvariant()}"
                ]
            }
        };
    }

    private static DesignConversionMeasurementGateRequest BuildFailingProductionCallerMeasurementGate(
        string gateId)
    {
        var measurement = BuildPassingLifecycleMeasurementGate(gateId, sourceDisjoint: true);
        return measurement with
        {
            HeldOutMetrics = measurement.HeldOutMetrics! with
            {
                HeldOutQualityScore = 0.5,
                EvidenceRefs =
                [
                    $"ref:{gateId}:heldout-fixture",
                    $"ref:{gateId}:production-caller-admission:truthful-fail"
                ]
            }
        };
    }

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

    private static DesignConversionDpoTrainingRecord ToRuntimePendingCandidate(
        DesignConversionDpoTrainingRecord record)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = DesignConversionDpoUsefulnessApi.ApprovalPendingStatus,
            ["review_status"] = DesignConversionDpoUsefulnessApi.RuntimeCandidateReviewPendingStatus,
            ["approved_for_training"] = bool.FalseString,
            ["training_candidate_only"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["split"] = "runtime_training_candidate_review_pending"
        };
        var licenseMetadata = new Dictionary<string, string>(record.License.Metadata, StringComparer.Ordinal)
        {
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = DesignConversionDpoUsefulnessApi.ApprovalPendingStatus,
            ["approved_for_training"] = bool.FalseString,
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
                ReviewerRef = "reviewer:modelserver-training-intake-pending-test",
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
                ["source_manifest_ref"] = $"file:///C:/Users/lubao/Documents/private.fig?token={privateMarker}",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = $"artifact://design-conversion/{recordId}/design-tree.json",
                ["original_tree_payload_ref"] = $"artifact://design-conversion/{recordId}/original-tree.json",
                ["source_artifact:manifest"] = $"artifact://design-conversion/{recordId}/source-manifest.json"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion-result",
                HtmlRef: $"<main data-secret=\"{privateMarker}\">exact_prompt private html</main>",
                HtmlSha256: string.Empty,
                CssRef: $".private{{background:url('C:/Users/lubao/{privateMarker}.png');}}",
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
                    UriRef: $"file:///C:/Users/lubao/private/{privateMarker}/render-desktop.png",
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

    private static IDisposable
        AssociateStartupIssuedLegalTeacherCapabilityForPromotionHost(
            IDesignConversionDpoPolicyBaseProvider provider)
    {
        var authority = DesignConversionLegalTeacherCapabilityTestFactory
            .CreateAdmitted();
        var association = DesignConversionRetainedDpoPolicyBaseActivation
            .AssociateLegalTeacherAdmissionAuthority(provider, authority);
        try
        {
            Assert.Same(
                authority,
                DesignConversionRetainedDpoPolicyBaseActivation
                    .ResolveLegalTeacherAdmissionAuthority(provider));
            return new VerifiedLegalTeacherAssociation(provider, association);
        }
        catch
        {
            association.Dispose();
            throw;
        }
    }

    private sealed class VerifiedLegalTeacherAssociation : IDisposable
    {
        private VerifiedLegalTeacherAssociationState? _state;

        internal VerifiedLegalTeacherAssociation(
            IDesignConversionDpoPolicyBaseProvider provider,
            IDisposable association)
        {
            _state = new VerifiedLegalTeacherAssociationState(
                provider,
                association);
        }

        public void Dispose()
        {
            var state = Interlocked.Exchange(ref _state, null);
            if (state is not null)
            {
                state.Association.Dispose();
                Assert.Null(
                    DesignConversionRetainedDpoPolicyBaseActivation
                        .ResolveLegalTeacherAdmissionAuthority(state.Provider));
            }
        }

        private sealed record VerifiedLegalTeacherAssociationState(
            IDesignConversionDpoPolicyBaseProvider Provider,
            IDisposable Association);
    }

    private static DesignConversionModelServerPromotionHostOptions CreatePromotionHostOptions(
        string artifactRoot,
        HttpClient promotionHttpClient,
        IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider)
        => new(
            artifactRoot,
            promotionHttpClient,
            CreatePromotionHttpUploaderOptions(artifactRoot, PromotionHostTenantId),
            DpoPolicyBaseProvider: dpoPolicyBaseProvider);

    private static DesignConversionModelServerPromotionHostOptions CreateRepositoryPromotionHostOptions(
        string artifactRoot,
        IModelRepository repository,
        IRemoteUnitLocator locator,
        string serverBaseUrl,
        string tenantId,
        IDesignConversionDpoPolicyBaseProvider? dpoPolicyBaseProvider)
        => new(
            artifactRoot,
            RepositoryPromotionOptions: new DesignConversionModelServerRepositoryPromotionOptions(
                repository,
                locator,
                serverBaseUrl,
                tenantId),
            DpoPolicyBaseProvider: dpoPolicyBaseProvider);

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
        string tenantId,
        string routeScope = "design-conversion-training-intake-host-promotion")
        => new(
            enabled: true,
            currentToken: "route-admission-token-1234567890",
            routeScope: routeScope,
            tenantId: tenantId);

    private static async Task AssertPromotedDpoPolicyCheckpointUsesProviderAsync(
        string promotedCheckpointPath,
        DesignConversionPromotedLocalModelLoadResult promotedLoad,
        DesignConversionTrainingIntakeLocalPromotionResult promotion,
        DesignConversionDpoPolicyBaseTestProvider provider)
    {
        Assert.True(provider.InvocationCount > 0);
        Assert.NotNull(provider.LastModelSet);

        var providerModelSet = provider.LastModelSet!;
        Assert.NotNull(promotedLoad.Checkpoint.DpoPolicyCheckpointRef);
        var typedRef = promotedLoad.Checkpoint.DpoPolicyCheckpointRef!;

        var expectedConfig = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var expectedConfigSha256 = DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(expectedConfig);
        var expectedInitialWeightSha256 =
            DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(providerModelSet.Reference);
        Assert.False(string.IsNullOrWhiteSpace(expectedInitialWeightSha256));

        Assert.Equal("11.DPO:DpoTrainer", typedRef.SourceSystem);
        Assert.Equal("11.DPO:DpoTrainer.StepVector:fresh_loaded", typedRef.SourceMechanism);
        Assert.Equal(DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind, typedRef.RuntimeBridgeKind);
        Assert.True(typedRef.FreshLoadVerified);
        Assert.True(typedRef.ConvertedToRuntimeWeights);
        Assert.Equal(4, typedRef.TrainingContext.SchemaVersion);
        Assert.True(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
            typedRef.TrainingContext.LegalTeacherAdmissionCommitment));
        Assert.True(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
            typedRef.TrainingContext.LegalTeacherAdmissionCommitment,
            typedRef.EvidenceRefs));
        Assert.Equal(providerModelSet.Identity.ProviderKind, typedRef.TrainingContext.ProviderKind);
        Assert.Equal(providerModelSet.Identity.ConfigurationKind, typedRef.TrainingContext.ConfigKind);
        Assert.Equal(providerModelSet.Identity.BaseModelId, typedRef.TrainingContext.BaseModelId);
        Assert.Equal(providerModelSet.Identity.BaseRevision, typedRef.TrainingContext.BaseRevision);
        Assert.Equal(providerModelSet.Identity.BaseCheckpointSha256, typedRef.TrainingContext.BaseCheckpointSha256);
        Assert.Equal(expectedConfigSha256, typedRef.TrainingContext.ConfigSha256);
        Assert.Equal(expectedInitialWeightSha256, typedRef.TrainingContext.InitialWeightSha256);
        Assert.Equal(
            expectedInitialWeightSha256,
            DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(providerModelSet.Reference));
        var postPromotionPolicyWeightSha256 =
            DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(providerModelSet.Policy);
        if (!string.Equals(postPromotionPolicyWeightSha256, expectedInitialWeightSha256, StringComparison.Ordinal))
            Assert.NotEqual(postPromotionPolicyWeightSha256, typedRef.TrainingContext.InitialWeightSha256);
        Assert.Equal(expectedConfig.VocabSize, typedRef.TrainingContext.VocabSize);
        Assert.Equal(expectedConfig.ContextLen, typedRef.TrainingContext.ContextLength);
        Assert.Equal(expectedConfig.HiddenDim, typedRef.TrainingContext.HiddenDim);
        Assert.Equal(expectedConfig.NumLayers, typedRef.TrainingContext.NumLayers);
        Assert.Equal(expectedConfig.NumHeads, typedRef.TrainingContext.NumHeads);
        Assert.Equal(expectedConfig.NumKvHeads, typedRef.TrainingContext.NumKvHeads);
        Assert.Equal(expectedConfig.FfnHiddenDim, typedRef.TrainingContext.FfnHiddenDim);
        Assert.Equal(expectedConfig.RopeTheta, typedRef.TrainingContext.RopeTheta);
        Assert.Equal(expectedConfig.RmsNormEps, typedRef.TrainingContext.RmsNormEps);
        Assert.DoesNotContain("CreateTinyPolicyConfig", typedRef.TrainingContext.ConfigKind, StringComparison.Ordinal);
        Assert.True(typedRef.TrainingContext.RawPayloadPersisted);
        Assert.False(typedRef.TrainingContext.AbsolutePathPersisted);
        Assert.False(Path.IsPathRooted(typedRef.PolicyCheckpointFileName));
        Assert.Equal(Path.GetFileName(typedRef.PolicyCheckpointFileName), typedRef.PolicyCheckpointFileName);

        var checkpointDirectory = Path.GetDirectoryName(promotedCheckpointPath);
        Assert.False(string.IsNullOrWhiteSpace(checkpointDirectory));
        var policyCheckpointPath = Path.Combine(checkpointDirectory!, typedRef.PolicyCheckpointFileName);
        Assert.True(File.Exists(policyCheckpointPath));
        var payloadBytes = await File.ReadAllBytesAsync(policyCheckpointPath);
        Assert.True(payloadBytes.Length > 0);
        Assert.Equal(payloadBytes.Length, typedRef.PolicyCheckpointByteLength);
        Assert.Equal(typedRef.Sha256, ComputeSha256Hex(payloadBytes));
        Assert.Equal($"dpo_policy_checkpoint_sha256:{typedRef.Sha256}", promotedLoad.Model.DpoPolicyCheckpointSha256Ref);
        Assert.Equal($"dpo_policy_checkpoint_file:{typedRef.PolicyCheckpointFileName}", promotedLoad.Model.DpoPolicyCheckpointFileRef);
        Assert.Equal(promotedLoad.Model.DpoPolicyCheckpointSha256Ref, promotion.PromotedLocalModel!.DpoPolicyCheckpointSha256Ref);
        Assert.Equal(promotedLoad.Model.DpoPolicyCheckpointFileRef, promotion.PromotedLocalModel.DpoPolicyCheckpointFileRef);

        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, typedRef.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, typedRef.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.DpoPolicyDirectInferenceConfiguredEvidence, typedRef.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_TYPED", typedRef.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_PAYLOAD_FILE_SIBLING", typedRef.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_BASE_PROVIDER_INJECTED", typedRef.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_DPO_POLICY_BASE_IDENTITY_VALIDATED", typedRef.EvidenceRefs);
        Assert.Contains($"dpo_policy_base_provider_kind:{providerModelSet.Identity.ProviderKind}", typedRef.EvidenceRefs);
        Assert.Contains($"dpo_policy_base_configuration_kind:{providerModelSet.Identity.ConfigurationKind}", typedRef.EvidenceRefs);
        Assert.Contains($"dpo_policy_base_model_id:{providerModelSet.Identity.BaseModelId}", typedRef.EvidenceRefs);
        Assert.Contains($"dpo_policy_base_revision:{providerModelSet.Identity.BaseRevision}", typedRef.EvidenceRefs);
        Assert.Contains($"dpo_policy_base_checkpoint_sha256:{providerModelSet.Identity.BaseCheckpointSha256}", typedRef.EvidenceRefs);
        Assert.Contains($"dpo_policy_config_sha256:{expectedConfigSha256}", typedRef.EvidenceRefs);
        Assert.Contains($"dpo_policy_initial_weight_sha256:{expectedInitialWeightSha256}", typedRef.EvidenceRefs);
        Assert.DoesNotContain("DESIGN_CONVERSION_DPO_POLICY_CHECKPOINT_REF_RAW_PAYLOAD_EMBEDDED", typedRef.EvidenceRefs);
    }

    private static string ComputeSha256Hex(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string ComputeSha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string UploadReceiptEvidenceHashRef(string evidenceRef)
        => $"UPLOAD_RECEIPT_EVIDENCE_SHA256:{ComputeSha256Hex(Encoding.UTF8.GetBytes(evidenceRef))}";

    private static void AssertNoPublicPathLeak(string publicJson, string path)
    {
        var fullPath = Path.GetFullPath(path);
        Assert.DoesNotContain(fullPath, publicJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fullPath.Replace('\\', '/'), publicJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fullPath.Replace("\\", "\\\\", StringComparison.Ordinal), publicJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(new Uri(fullPath).AbsoluteUri, publicJson, StringComparison.OrdinalIgnoreCase);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Assert.DoesNotContain(directory, publicJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(directory.Replace('\\', '/'), publicJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(directory.Replace("\\", "\\\\", StringComparison.Ordinal), publicJson, StringComparison.OrdinalIgnoreCase);
        }
    }

    private string CreateOwnedTempPath(string leafName)
    {
        if (string.IsNullOrWhiteSpace(leafName) ||
            !string.Equals(Path.GetFileName(leafName), leafName, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "An owned test path must have exactly one non-empty leaf name.",
                nameof(leafName));
        }

        var path = Path.GetFullPath(
            Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString("N"),
                leafName));
        Assert.True(_ownedTempPaths.Add(path));
        return path;
    }

    private void DeleteDirectoryIfExists(string path)
    {
        var normalizedPath = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(path));
        if (!_ownedTempPaths.Contains(normalizedPath))
            return;

        var parent = Directory.GetParent(normalizedPath);
        var tempRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(Path.GetTempPath()));
        var leafName = Path.GetFileName(normalizedPath);
        if (parent is null || string.IsNullOrWhiteSpace(leafName))
        {
            throw new InvalidOperationException(
                "Owned test cleanup path must match %TEMP%/<N-format GUID>/<nonempty leaf>.");
        }

        var parentContainer = parent.Parent is null
            ? string.Empty
            : Path.TrimEndingDirectorySeparator(Path.GetFullPath(parent.Parent.FullName));
        var pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!Guid.TryParseExact(parent.Name, "N", out _) ||
            !string.Equals(parentContainer, tempRoot, pathComparison) ||
            string.Equals(normalizedPath, tempRoot, pathComparison) ||
            string.Equals(normalizedPath, parent.FullName, pathComparison))
        {
            throw new InvalidOperationException(
                "Owned test cleanup path must match %TEMP%/<N-format GUID>/<nonempty leaf>.");
        }

        if (Directory.Exists(normalizedPath))
            Directory.Delete(normalizedPath, recursive: true);

        if (Directory.Exists(parent.FullName) &&
            !Directory.EnumerateFileSystemEntries(parent.FullName).Any())
        {
            Directory.Delete(parent.FullName, recursive: false);
        }

        _ownedTempPaths.Remove(normalizedPath);
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

        public IReadOnlyList<CapturedPromotionUploadPart> Parts { get; private set; } = [];

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
                Parts = await CaptureMultipartPartsAsync(request.Content, cancellationToken);
                var bodyBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                BodyText = Encoding.UTF8.GetString(bodyBytes);
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

        private static async Task<IReadOnlyList<CapturedPromotionUploadPart>> CaptureMultipartPartsAsync(
            HttpContent content,
            CancellationToken cancellationToken)
        {
            if (content is not MultipartFormDataContent multipart)
                return [];

            var parts = new List<CapturedPromotionUploadPart>();
            foreach (var part in multipart)
            {
                var disposition = part.Headers.ContentDisposition;
                var name = disposition?.Name?.Trim('"') ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var fileName = (disposition?.FileNameStar ?? disposition?.FileName)?.Trim('"') ?? string.Empty;
                parts.Add(new CapturedPromotionUploadPart(
                    name,
                    fileName,
                    await part.ReadAsByteArrayAsync(cancellationToken)));
            }

            return parts;
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

    private sealed record CapturedPromotionUploadPart(
        string Name,
        string FileName,
        byte[] Body);

    private static string TrainingIntakeUnitAlias(string unitId)
        => string.Equals(unitId, DesignConversionRuntimeScaffoldCriticUnit.UnitId, StringComparison.Ordinal)
            ? "critic"
            : string.Equals(unitId, DesignConversionRuntimeScaffoldImproverUnit.UnitId, StringComparison.Ordinal)
                ? "improver"
                : unitId;

    private static string BuildTrainingIntakeFakeStableConfigurationIdentitySha256(
        string domain,
        string? marker,
        string defaultMarker)
        => "sha256:" + ComputeSha256Hex(Encoding.UTF8.GetBytes(string.Join(
            "\n",
            [
                "design-conversion-training-intake-test-fake-stable-configuration/v1",
                domain,
                marker ?? defaultMarker
            ])));

    private sealed class RecordingTrainingIntakeModelRepository(
        List<string> calls,
        string? toStringMarker = null) : IModelRepository
    {
        private readonly string _stableConfigurationIdentitySha256 =
            BuildTrainingIntakeFakeStableConfigurationIdentitySha256(
                "recording-training-intake-model-repository",
                toStringMarker,
                nameof(RecordingTrainingIntakeModelRepository));

        public List<GeneratedCapabilityCheckpointManifest> UploadedManifests { get; } = [];
        public Dictionary<string, string> UploadedCheckpointSha256ByUnit { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, string> LoadedCheckpointSha256ByUnit { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, string> UploadServerBaseUrlByUnit { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, string> LoadServerBaseUrlByUnit { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, string> LoadFallbackPathByUnit { get; } = new(StringComparer.Ordinal);

        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok(_stableConfigurationIdentitySha256);

        public Task<OperationResult<Stream>> LoadWeightsAsync(
            string unitId,
            string? serverBaseUrl,
            string localFallbackPath,
            CancellationToken ct = default)
        {
            var alias = TrainingIntakeUnitAlias(unitId);
            calls.Add($"load:{alias}");
            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<Stream>.Fail("cancelled", "CANCELLED"));

            if (string.IsNullOrWhiteSpace(localFallbackPath) || !File.Exists(localFallbackPath))
            {
                return Task.FromResult(OperationResult<Stream>.Fail(
                    "checkpoint fallback path missing",
                    "TEST_REPOSITORY_CHECKPOINT_MISSING"));
            }

            var bytes = File.ReadAllBytes(localFallbackPath);
            LoadedCheckpointSha256ByUnit[unitId] = ComputeSha256Hex(bytes);
            LoadServerBaseUrlByUnit[unitId] = serverBaseUrl?.TrimEnd('/') ?? string.Empty;
            LoadFallbackPathByUnit[unitId] = Path.GetFullPath(localFallbackPath);
            Stream stream = new MemoryStream(bytes, writable: false);
            return Task.FromResult(OperationResult<Stream>.Ok(stream));
        }

        public Task<OperationResult<RemoteUnitManifest>> UploadAsync(
            GeneratedCapabilityCheckpointManifest manifest,
            string serverBaseUrl,
            CancellationToken ct = default)
        {
            var alias = TrainingIntakeUnitAlias(manifest.UnitId);
            calls.Add($"upload:{alias}");
            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("cancelled", "CANCELLED"));

            UploadedManifests.Add(manifest);
            var checkpointSha256 = ComputeSha256Hex(manifest.PolicyCheckpointPath);
            UploadedCheckpointSha256ByUnit[manifest.UnitId] = checkpointSha256;
            UploadServerBaseUrlByUnit[manifest.UnitId] = serverBaseUrl.TrimEnd('/');
            var manifestSha256 = manifest.ManifestSha256 ?? string.Empty;
            var remote = new RemoteUnitManifest(
                UnitId: manifest.UnitId,
                UnitRole: manifest.UnitRole,
                DomainTag: manifest.DomainTag,
                Version: manifest.Version,
                ServerBaseUrl: serverBaseUrl.TrimEnd('/'),
                UnitEndpoint: $"/models/{Uri.EscapeDataString(manifest.UnitId)}",
                ManifestSha256: manifestSha256,
                TrainingBranch: manifest.TrainingBranch ?? string.Empty,
                TrainingRunId: manifest.TrainingRunId ?? string.Empty,
                SourceDatasetSha256: manifest.SourceDatasetSha256 ?? string.Empty,
                ModelConfig: new RemoteModelConfig(
                    manifest.VocabSize,
                    manifest.ContextLen,
                    manifest.HiddenDim,
                    manifest.NumLayers,
                    manifest.NumHeads,
                    manifest.NumKvHeads,
                    manifest.FfnHiddenDim,
                    manifest.RopeTheta,
                    manifest.RmsNormEps),
                LoraConfig: new RemoteLoraConfig(
                    manifest.LoraRank,
                    manifest.LoraAlpha,
                    manifest.LoraDropout,
                    manifest.LoraTargetModules),
                TokenizerPath: manifest.TokenizerPath,
                RegisteredAtUtc: RepositoryPromotionRegisteredAtUtc);

            return Task.FromResult(OperationResult<RemoteUnitManifest>.Ok(remote));
        }

        public override string ToString()
            => toStringMarker ?? nameof(RecordingTrainingIntakeModelRepository);
    }

    private sealed class RecordingTrainingIntakeRemoteUnitLocator(
        List<string> calls,
        string? toStringMarker = null) : IRemoteUnitLocator
    {
        private readonly string _stableConfigurationIdentitySha256 =
            BuildTrainingIntakeFakeStableConfigurationIdentitySha256(
                "recording-training-intake-remote-unit-locator",
                toStringMarker,
                nameof(RecordingTrainingIntakeRemoteUnitLocator));

        public Dictionary<string, RemoteUnitManifest> RegisteredByUnit { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, RemoteUnitManifest> LocatedByUnit { get; } = new(StringComparer.Ordinal);

        public OperationResult<string> PreflightStableConfigurationIdentitySha256()
            => OperationResult<string>.Ok(_stableConfigurationIdentitySha256);

        public Task<OperationResult<RemoteUnitManifest>> LocateAsync(
            string unitId,
            CancellationToken ct = default)
        {
            var alias = TrainingIntakeUnitAlias(unitId);
            calls.Add($"locate:{alias}");
            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail("cancelled", "CANCELLED"));

            if (!RegisteredByUnit.TryGetValue(unitId, out var manifest))
            {
                return Task.FromResult(OperationResult<RemoteUnitManifest>.Fail(
                    "remote unit not found",
                    "TEST_REMOTE_UNIT_NOT_FOUND"));
            }

            LocatedByUnit[unitId] = manifest;
            return Task.FromResult(OperationResult<RemoteUnitManifest>.Ok(manifest));
        }

        public Task<OperationResult<bool>> RegisterAsync(
            RemoteUnitManifest manifest,
            CancellationToken ct = default)
        {
            var alias = TrainingIntakeUnitAlias(manifest.UnitId);
            calls.Add($"register:{alias}");
            if (ct.IsCancellationRequested)
                return Task.FromResult(OperationResult<bool>.Fail("cancelled", "CANCELLED"));

            RegisteredByUnit[manifest.UnitId] = manifest;
            return Task.FromResult(OperationResult<bool>.Ok(true));
        }

        public override string ToString()
            => toStringMarker ?? nameof(RecordingTrainingIntakeRemoteUnitLocator);
    }

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

    private sealed class CountingBoundedTrainingIntakePromotionUploader :
        IDesignConversionBoundedPromotionUploader,
        IDesignConversionPromotionUploaderPreflight
    {
        private const string Fingerprint =
            "sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        public int MaximumSerializedReceiptUtf8Bytes => 64 * 1024;

        public int UploadCallCount { get; private set; }

        public OperationResult<string> PreflightConfigurationFingerprintSha256()
            => OperationResult<string>.Ok(Fingerprint);

        public Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
        {
            UploadCallCount++;
            return Task.FromResult(OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
                new DesignConversionPromotionUploadReceipt(
                    "direct-untrusted-uploader",
                    "https://promotion.example.test/models/direct-untrusted-uploader",
                    DateTimeOffset.UnixEpoch.AddMinutes(812),
                    ["direct-untrusted-uploader-receipt"])
                {
                    PublicationKind =
                        DesignConversionPromotionPublicationKind
                            .VerifiedModelServerUpload,
                    UploadAttempted = true,
                    UploadSucceeded = true,
                    VerificationSucceeded = true,
                    PublicationStatus =
                        DesignConversionPromotionPublicationStatus
                            .UploadedVerified,
                    ProvenanceRef = "direct-untrusted-uploader-provenance"
                }));
        }
    }

    [Fact]
    public void ServerOwnedConfiguredRemotePromotionUploadAuthority_RejectsForgedOrCrossBoundUploader()
    {
        var legalAuthority =
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted();
        var uploader = new CountingBoundedTrainingIntakePromotionUploader();
        var fingerprint = uploader.PreflightConfigurationFingerprintSha256();
        Assert.True(fingerprint.IsSuccess, fingerprint.ErrorMessage);
        const string routeBindingFingerprintSha256 =
            "sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        var issued =
            DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority
                .Issue(
                    uploader,
                    legalAuthority,
                    routeBindingFingerprintSha256,
                    "DESIGN_CONVERSION_MODEL_SERVER_CONFIGURED_REMOTE_PROMOTION_AUTHORITY_ISSUED");
        Assert.True(issued.IsSuccess, issued.ErrorMessage);

        var valid = issued.Value!.Validate(
            uploader,
            legalAuthority,
            fingerprint.Value!,
            uploader.MaximumSerializedReceiptUtf8Bytes);
        Assert.True(valid.IsSuccess, valid.ErrorMessage);
        Assert.StartsWith(
            "sha256:",
            valid.Value!.StableAuthorityFingerprintSha256,
            StringComparison.Ordinal);
        Assert.Equal(
            routeBindingFingerprintSha256,
            valid.Value.RouteBindingFingerprintSha256);
        Assert.Equal(
            "DESIGN_CONVERSION_MODEL_SERVER_CONFIGURED_REMOTE_PROMOTION_AUTHORITY_ISSUED",
            valid.Value.IssuerEvidenceRef);

        var otherUploader = new CountingBoundedTrainingIntakePromotionUploader();
        var wrongUploader = issued.Value.Validate(
            otherUploader,
            legalAuthority,
            fingerprint.Value!,
            otherUploader.MaximumSerializedReceiptUtf8Bytes);
        Assert.False(wrongUploader.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_INVALID",
            wrongUploader.ErrorCode);

        var otherLegalAuthority =
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted();
        var wrongLegalAuthority = issued.Value.Validate(
            uploader,
            otherLegalAuthority,
            fingerprint.Value!,
            uploader.MaximumSerializedReceiptUtf8Bytes);
        Assert.False(wrongLegalAuthority.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_INVALID",
            wrongLegalAuthority.ErrorCode);

        var wrongFingerprint = issued.Value.Validate(
            uploader,
            legalAuthority,
            "sha256:cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
            uploader.MaximumSerializedReceiptUtf8Bytes);
        Assert.False(wrongFingerprint.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_INVALID",
            wrongFingerprint.ErrorCode);

        var malformedRouteBinding =
            DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority
                .Issue(
                    uploader,
                    legalAuthority,
                    "route:tenant-alpha",
                    "DESIGN_CONVERSION_MODEL_SERVER_CONFIGURED_REMOTE_PROMOTION_AUTHORITY_ISSUED");
        Assert.False(malformedRouteBinding.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_ROUTE_BINDING_INVALID",
            malformedRouteBinding.ErrorCode);
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

    private class RunOptionsLocalArtifactDbSpy : DispatchProxy
    {
        private IDesignConversionLocalArtifactDb _fallback = null!;

        public string IntakeId { get; private set; } = null!;
        public IDesignConversionLocalArtifactDb Db { get; private set; } = null!;
        public int RunTrainingIntakeCallCount { get; private set; }
        public string? LastRunIntakeId { get; private set; }
        public DesignConversionTrainingPipelineOptions? LastRunOptions { get; private set; }
        public IReadOnlyDictionary<string, string>? LastAdmissionBoundaryMetadata { get; private set; }

        public static RunOptionsLocalArtifactDbSpy Create(
            string intakeId = "training-intake-startup-threshold-wiring",
            IDesignConversionLocalArtifactDb? fallback = null)
        {
            var proxy = Create<IDesignConversionLocalArtifactDb, RunOptionsLocalArtifactDbSpy>();
            var spy = (RunOptionsLocalArtifactDbSpy)(object)proxy;
            spy.IntakeId = intakeId;
            spy.Db = proxy;
            spy._fallback = fallback ?? new NoCallLocalArtifactDb();
            return spy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(IDesignConversionLocalArtifactDb.RunTrainingIntakeAsync))
            {
                RunTrainingIntakeCallCount++;
                LastRunIntakeId = args?[0] as string;
                LastRunOptions = args?[1] as DesignConversionTrainingPipelineOptions;
                LastAdmissionBoundaryMetadata = args?[3] as IReadOnlyDictionary<string, string>;
                return Task.FromResult(
                    OperationResult<DesignConversionTrainingIntakeRunResult>.Ok(
                        BuildSpyRunResult(LastRunIntakeId!, LastRunOptions)));
            }

            return targetMethod!.Invoke(_fallback, args);
        }

        private static DesignConversionTrainingIntakeRunResult BuildSpyRunResult(
            string intakeId,
            DesignConversionTrainingPipelineOptions? options)
        {
            var runId = string.IsNullOrWhiteSpace(options?.RunId)
                ? "spy-training-intake-run"
                : options!.RunId;
            var runnerEvidence = new DesignConversionTrainingIntakeRunnerEvidence(
                BoundaryDecisionKind: "spy_startup_threshold_wiring",
                DatasetCountGatePassed: true,
                RequiredTrainRecords: options?.DatasetOptions?.MinimumTrainRecords ?? 0,
                RequiredHeldOutRecords: options?.DatasetOptions?.MinimumHeldOutRecords ?? 0,
                ActualTrainRecords: options?.DatasetOptions?.MinimumTrainRecords ?? 0,
                ActualHeldOutRecords: options?.DatasetOptions?.MinimumHeldOutRecords ?? 0,
                SourceDisjoint: true,
                SourceManifestStatus: "source_manifest_disjoint",
                SourceDisjointStatus: "source_disjoint",
                MachineLifecycleHookAttempted: false,
                TrainingStarted: false,
                TrainingExecutionStatus: "blocked",
                RunnerBlockedReason: "measurement_gate_failed",
                LocalLifecycleBatchResultCount: 0,
                CheckpointCandidateStatus: "not_created",
                CheckpointCandidateCreated: false,
                CheckpointMutationStatus: "not_mutated",
                CheckpointMutationEvidenceProven: false,
                CheckpointContainsRealTrainedWeights: false,
                WeightProofStatus: "not_produced",
                FreshLoadStatus: "not_loaded",
                ContinueTrainingStatus: "not_started",
                MeasurementGateStatus: DesignConversionMeasurementGateStatus.Failed,
                PromotionStatus: "not_ready",
                PromotionUploadAllowed: false,
                EvidenceRefs: ["spy_startup_threshold_wiring"],
                FailureReasons: ["measurement_gate_failed"],
                Warnings: []);

            return new DesignConversionTrainingIntakeRunResult(
                RunId: runId,
                TrainingIntakeId: intakeId,
                PreflightId: $"preflight:{intakeId}",
                AcceptedCandidateCount: 100,
                ResolvedTrainingRecordCount: 100,
                MayStartTraining: false,
                SourceManifestStatus: "source_manifest_disjoint",
                SourceDisjointStatus: "source_disjoint",
                TrainingRunnerInvoked: true,
                LocalLifecycleHookAttempted: false,
                TrainingStarted: false,
                CheckpointMutated: false,
                CheckpointCandidateCreated: false,
                WeightsProduced: false,
                ModelUploadReady: false,
                TrainingExecutionBoundary: "spy_startup_threshold_wiring",
                TrainingExecutionStatus: "blocked",
                RunnerBlockedReason: "measurement_gate_failed",
                CheckpointStatus: "not_created",
                CheckpointMutationStatus: "not_mutated",
                WeightProofStatus: "not_produced",
                MeasurementGateStatus: DesignConversionMeasurementGateStatus.Failed,
                PromotionGateStatus: "not_ready",
                UploadReadinessStatus: "not_ready",
                PromotionBlockedReasons: ["measurement_gate_failed"],
                RunnerEvidence: runnerEvidence,
                Warnings: [],
                BlockReasons: ["measurement_gate_failed"],
                BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["spy_startup_threshold_wiring"] = bool.TrueString,
                    ["public_sanitizer_posix_tmp_path"] = "/tmp/design-conversion/private-cache/checkpoint.json",
                    ["public_sanitizer_linux_root_path"] = "/root/.cache/design-conversion/checkpoint.json",
                    ["public_sanitizer_linux_home_path"] = "/home/user/design-conversion/checkpoint.json",
                    ["public_sanitizer_escaped_posix_path"] = @"\/home\/user\/design-conversion\/manifest.json",
                    ["public_sanitizer_windows_path"] = @"C:\Users\design-user\design-conversion\manifest.json",
                    ["public_sanitizer_unc_path"] = @"\\server\share\design-conversion\manifest.json",
                    ["public_sanitizer_file_uri"] = "file:///tmp/design-conversion/manifest.json",
                    ["public_sanitizer_safe_label"] = "source_manifest_disjoint",
                    ["public_sanitizer_canonical_sha256"] = PublicSanitizerCanonicalSha256Ref,
                    ["training_started"] = bool.FalseString,
                    ["checkpoint_mutated"] = bool.FalseString,
                    ["weights_produced"] = bool.FalseString,
                    ["model_upload_ready"] = bool.FalseString,
                    ["product_ready"] = bool.FalseString
                });
        }
    }

    private sealed class NoCallLocalArtifactDb : IDesignConversionLocalArtifactDb
    {
        public int CallCount { get; private set; }

        public Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordAsync(
            DesignConversionLocalArtifactDbRecord record,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionLocalArtifactDbRecord>();

        public Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ListRecordsAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => UnexpectedAsync<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>();

        public Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryApprovedTrainingCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => UnexpectedAsync<IReadOnlyList<DesignConversionDpoTrainingRecord>>();

        public Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            DateTimeOffset? exportedAtUtc = null,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionApprovedDpoHandoffArtifact>();

        public Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
            DesignConversionApprovedDpoHandoffArtifact handoff,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => UnexpectedAsync<DesignConversionTrainingIntakeRecord>();

        public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
            CancellationToken ct = default)
            => UnexpectedAsync<IReadOnlyList<DesignConversionTrainingIntakeRecord>>();

        public Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
            string intakeId,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionTrainingIntakePreflightResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
            string intakeId,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => UnexpectedAsync<DesignConversionTrainingIntakeRunResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
            string intakeId,
            string promotionOutputDirectory,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionTrainingIntakeLocalPromotionResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
            string promotedLocalModelRef,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();

        public Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
            string localArtifactRecordId,
            string candidateOrTrainingRecordId,
            string reviewerRef,
            string reason,
            DateTimeOffset? approvedAtUtc = null,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionDpoCandidateApprovalResult>();

        public Task<OperationResult<DesignConversionDpoDisplayResult>> QueryDisplayRowsAsync(
            DesignConversionDpoDisplayQuery? query = null,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionDpoDisplayResult>();

        private Task<OperationResult<T>> UnexpectedAsync<T>()
        {
            CallCount++;
            return Task.FromResult(OperationResult<T>.Fail(
                "NoCallLocalArtifactDb should not be reached by invalid threshold configuration tests.",
                "NO_CALL_LOCAL_ARTIFACT_DB_REACHED"));
        }
    }

    private sealed record SeededTrainingIntakeContext(
        IDesignConversionLocalArtifactDb Db,
        string IntakeId);

    private sealed record SeededApprovedHandoffContext(
        IDesignConversionLocalArtifactDb Db,
        DesignConversionApprovedDpoHandoffArtifact Handoff,
        string RootDirectory,
        IReadOnlyList<string>? TeacherRequestIds = null);

    private sealed record FreshApprovedHandoffContext(
        DesignConversionApprovedDpoHandoffArtifact Handoff,
        IReadOnlyList<string> TeacherRequestIds);


    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_DirectBoundedConfiguredUploaderWithoutAuthorityIsRejectedBeforeUpload()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_CONFIGURED_UPLOADER_SECRET";
        var policyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var uploader = new CountingBoundedTrainingIntakePromotionUploader();
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            policyBaseProvider,
            uploader);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:modelserver-training-intake-configured-uploader-failure",
            "configured promotion uploader failure must not fall back to local receipt",
            DateTimeOffset.UnixEpoch.AddMinutes(206));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var promotionOutputDirectory = Path.Combine(
            seeded.RootDirectory,
            "training-intake-configured-uploader-failure-output");
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-configured-uploader-failure",
            "training-intake-configured-uploader-failure-dataset",
            "training-intake-configured-uploader-failure-gate",
            sourceDisjoint: true);

        try
        {
            var promotion = await DesignConversionPromotionAdmissionTestScope.RunAsync(
                () => api.RunTrainingIntakeAndPromoteLocalAsync(
                intake.Value!.IntakeId,
                promotionOutputDirectory,
                options));

            Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
            Assert.False(promotion.Value!.LocalPromotionReady);
            Assert.False(promotion.Value.TrainingRun.ModelUploadReady);
            Assert.NotNull(promotion.Value.Publication);
            Assert.Equal(
                DesignConversionPromotionPublicationKind.Rejected,
                promotion.Value.Publication!.PublicationKind);
            Assert.Contains("BUDGET_TIER_POLICY_FAILED", promotion.Value.BlockReasons);
            Assert.Equal(
                "ConfiguredUploaderUntrusted",
                promotion.Value.BoundaryMetadata["local_promotion_uploader_authority_mode"]);
            Assert.Equal(0, uploader.UploadCallCount);
            Assert.DoesNotContain(privateMarker, JsonSerializer.Serialize(promotion.Value), StringComparison.Ordinal);
            Assert.DoesNotContain(
                @"C:\Users\lubao\private",
                JsonSerializer.Serialize(promotion.Value),
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }


    [Fact]
    public async Task RunTrainingIntakeAndPromoteLocalAsync_SourceDisjointLifecyclePublishesLocalReceiptWithoutRemotePromotion()
    {
        const string privateMarker = "PRIVATE_TRAINING_INTAKE_ACTUAL_ROUTE_POSITIVE_SECRET";
        var dpoPolicyBaseProvider = new DesignConversionDpoPolicyBaseTestProvider();
        var seeded = await CreateSourceDisjointApprovedHandoffAsync(
            privateMarker,
            dpoPolicyBaseProvider: dpoPolicyBaseProvider);
        var api = new DesignConversionDpoUsefulnessApi(seeded.Db);
        var intake = await api.CreateTrainingIntakeAsync(
            seeded.Handoff,
            "architect:modelserver-training-intake-actual-route-positive",
            "actual local training-intake route lifecycle into promoted local API factory; not ProductReady",
            DateTimeOffset.UnixEpoch.AddMinutes(205));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        var promotionOutputDirectory = Path.Combine(
            seeded.RootDirectory,
            "training-intake-actual-route-positive-promotion-output");
        var options = BuildTrainingIntakeActualRouteOptions(
            "training-intake-actual-route-source-disjoint-positive",
            "training-intake-actual-route-source-disjoint-positive-dataset",
            "training-intake-actual-route-source-disjoint-positive-gate",
            sourceDisjoint: true);

        try
        {
            var promotion = await DesignConversionPromotionAdmissionTestScope.RunAsync(
                () => api.RunTrainingIntakeAndPromoteLocalAsync(
                intake.Value!.IntakeId,
                promotionOutputDirectory,
                options));

            Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
            Assert.Equal(1, dpoPolicyBaseProvider.InvocationCount);

            // Local acceptance proves that a fresh checkpoint and its local receipt are ready for later use.
            Assert.True(promotion.Value!.LocalPromotionReady);
            Assert.NotNull(promotion.Value.Publication);
            Assert.Equal(
                DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
                promotion.Value.Publication!.PublicationKind);
            Assert.Equal(
                DesignConversionPromotionPublicationStatus.NotUploaded,
                promotion.Value.Publication.PublicationStatus);
            Assert.NotNull(promotion.Value.Publication.UploadReceipt);
            Assert.False(promotion.Value.Publication.UploadReceipt!.UploadAttempted);
            Assert.False(promotion.Value.Publication.UploadReceipt.ExternalCloudUploadAttempted);
            Assert.False(promotion.Value.Publication.UploadReceipt.UploadSucceeded);
            Assert.False(promotion.Value.Publication.UploadReceipt.VerificationSucceeded);
            Assert.Equal(
                DesignConversionPromotionPublicationStatus.NotUploaded,
                promotion.Value.Publication.UploadReceipt.PublicationStatus);
            Assert.True(promotion.Value.Publication.Manifest.LocalPromotionReceiptWritten);
            Assert.Equal("local_receipt_only", promotion.Value.Publication.Manifest.PromotionReceiptKind);
            Assert.True(promotion.Value.TrainingRun.TrainingRunnerInvoked);
            Assert.True(promotion.Value.TrainingRun.TrainingStarted);
            Assert.True(promotion.Value.TrainingRun.CheckpointMutated);
            Assert.True(promotion.Value.TrainingRun.WeightsProduced);
            Assert.True(promotion.Value.TrainingRun.ModelUploadReady);
            Assert.Equal(
                DesignConversionMeasurementGateStatus.Passed,
                promotion.Value.TrainingRun.MeasurementGateStatus);
            Assert.Equal("proven", promotion.Value.TrainingRun.SourceManifestStatus);
            Assert.Equal("proven", promotion.Value.TrainingRun.SourceDisjointStatus);
            Assert.Equal(100, promotion.Value.TrainingRun.ResolvedTrainingRecordCount);
            Assert.NotNull(promotion.Value.TrainingRun.RunnerEvidence);
            Assert.True(promotion.Value.TrainingRun.RunnerEvidence!.SourceDisjoint);
            Assert.Equal("proven", promotion.Value.TrainingRun.RunnerEvidence.SourceDisjointStatus);
            Assert.Equal(
                "PROMOTION_ALLOWED_BY_EVALUATED_DPO_DECISION",
                promotion.Value.TrainingRun.RunnerEvidence.PromotionStatus);
            Assert.Contains(
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                promotion.Value.EvidenceRefs);
            Assert.DoesNotContain(
                "DESIGN_CONVERSION_SERVER_OWNED_PROMOTION_BUDGET_AUTHORITY_NOT_CONFIGURED",
                promotion.Value.EvidenceRefs);
            Assert.DoesNotContain(
                "DESIGN_CONVERSION_SERVER_OWNED_PROMOTION_INTEGRITY_AUTHORITY_NOT_CONFIGURED",
                promotion.Value.EvidenceRefs);
            Assert.NotNull(promotion.Value.PromotedLocalModel);
            Assert.True(promotion.Value.PromotedLocalModel!.CheckpointFreshLoadVerified);
            Assert.True(promotion.Value.PromotedLocalModel.CheckpointContainsRealTrainedWeights);
            Assert.True(promotion.Value.PromotedLocalModel.PromotionManifestClaimed);
            Assert.True(promotion.Value.PromotedLocalModel.LocalOnly);
            Assert.True(promotion.Value.PromotedLocalModel.SourceDisjointHeldOutReadinessGatePassed);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["local_promotion_ready"]);

            // A local receipt records no network action: neither a cloud upload nor a server promotion was attempted.
            Assert.False(promotion.Value.ExternalCloudUploadAttempted);
            Assert.False(promotion.Value.Publication.Manifest.ModelServerUploadAttempted);
            Assert.False(promotion.Value.Publication.Manifest.ModelServerPromotionClaimed);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["external_cloud_upload_attempted"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["external_cloud_upload_not_attempted"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["model_server_upload_attempted"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["model_server_upload_verified"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["model_server_promotion_claimed"]);
            Assert.Equal("BuiltInLocalReceiptOnly", promotion.Value.BoundaryMetadata["local_promotion_uploader_authority_mode"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["model_server_upload_not_attempted_local_receipt"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["model_server_promotion_not_claimed_local_receipt"]);

            // Local readiness does not make the model product-ready and does not prove a deployment happened.
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["product_ready"]);
            Assert.False(promotion.Value.BoundaryMetadata.ContainsKey("deployment_verified"));
            Assert.DoesNotContain(privateMarker, JsonSerializer.Serialize(promotion.Value), StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(promotionOutputDirectory))
                Directory.Delete(promotionOutputDirectory, recursive: true);
        }
    }

}
