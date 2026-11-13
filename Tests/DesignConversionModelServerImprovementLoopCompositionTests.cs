// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerImprovementLoopCompositionTests
{
    private const int FullLocalPreflightDpoStepCount = 2;
    private static readonly DateTimeOffset FullLocalPreflightNow =
        new(2026, 8, 10, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task TryRunImprovementLoopAsync_FullLocalStartupPreflightOptInRunsAtomicPromotionOnce()
    {
        const string privateMarker = "PRIVATE_FULL_LOCAL_PREFLIGHT_OPT_IN_SECRET";
        const string loopId = "full-local-preflight-opt-in-loop";
        const string requestId = "full-local-preflight-opt-in-request";
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "dc-promo-full",
            Guid.NewGuid().ToString("N"));
        try
        {
            var artifactDb = CreateRecordingArtifactDb(
                root,
                SourceManifestSha(requestId, "source-b"));
            using var teacherAssociation =
                DesignConversionDpoTeacherTestAssociation.Associate(
                    artifactDb,
                    supportTeacherAuthoring: true);
            var localEvaluationClient = new LocalIndependentEvaluationClient();
            using var localEvaluatorRuntime = OwnLocalEvaluatorRuntime(
                artifactDb,
                localEvaluationClient,
                "full-local-preflight-opt-in");
            var browserAdapter = new RecordingBrowserAdapter(Path.Combine(root, "browser-artifacts"));
            var comparisonProvider = new AttemptScoreComparisonProvider(
                CreateRepeatedScores(2, 0.70, 0.82, 0.94, 0.70, 0.82, 0.94));
            var visualJudge = new AttemptScoreVisualJudge(
                CreateRepeatedScores(2, 0.70, 0.82, 0.94, 0.70, 0.82, 0.94));
            var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
                browserAdapter,
                comparisonProvider,
                visualJudge);
            var strategy = CreateScriptedCandidateStrategy("full-local-opt-in", 6);
            var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                strategy,
                artifactDb,
                CreateFullLocalPreflightOptions(),
                () => FullLocalPreflightNow);
            var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);

            var envelope = await adapter.TryRunImprovementLoopAsync(CreateLoopPackage(
                loopId,
                requestId,
                privateMarker,
                sourceVariant: "source-a"));

            AssertAtomicPromotionEnvelope(
                envelope,
                artifactDb,
                root,
                privateMarker,
                expectedSourceManifestRefs:
                [
                    SourceManifestSha(requestId, "source-b")
                ]);
            Assert.True(localEvaluationClient.CallCount > 0);
            Assert.Empty(strategy.Requests);
            Assert.NotEmpty(browserAdapter.Requests);
            Assert.NotEmpty(comparisonProvider.Requests);
            Assert.NotEmpty(visualJudge.Requests);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_FullLocalStartupDefaultOptionsDoNotRunAtomicPromotion()
    {
        const string privateMarker = "PRIVATE_FULL_LOCAL_PREFLIGHT_DEFAULT_SECRET";
        const string requestId = "full-local-preflight-default-request";
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-full-local-preflight-default",
            Guid.NewGuid().ToString("N"));
        try
        {
            var artifactDb = CreateRecordingArtifactDb(root);
            using var teacherAssociation =
                DesignConversionDpoTeacherTestAssociation.Associate(artifactDb);
            var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
                new RecordingBrowserAdapter(Path.Combine(root, "browser-artifacts")),
                new AttemptScoreComparisonProvider(0.70, 0.82, 0.94),
                new AttemptScoreVisualJudge(0.70, 0.82, 0.94));
            var strategy = CreateScriptedCandidateStrategy("full-local-default", 3);
            var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                strategy,
                artifactDb,
                () => FullLocalPreflightNow.AddMinutes(2));
            var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);

            var envelope = await adapter.TryRunImprovementLoopAsync(CreateLoopPackage(
                "full-local-preflight-default-loop",
                requestId,
                privateMarker,
                sourceVariant: "source-default"));

            Assert.Equal("success", envelope.EnvelopeKind);
            Assert.Equal(200, envelope.StatusCode);
            Assert.Equal(0, artifactDb.RunTrainingIntakeAndPromoteLocalCalls);
            Assert.Equal(0, artifactDb.RunTrainingIntakePreflightCalls);
            using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
            var boundaryMetadata = document.RootElement.GetProperty("boundaryMetadata");
            AssertNoTrainingIntakeHandoffClaim(boundaryMetadata, envelope.ResponseBodyJson);
            Assert.DoesNotContain(root, envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }




    [Fact]
    public async Task TryRunImprovementLoopAsync_FullLocalStartupPreflightEnabledWithoutArtifactDbDoesNotClaimHandoff()
    {
        const string privateMarker = "PRIVATE_FULL_LOCAL_PREFLIGHT_NULL_DB_SECRET";
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-full-local-preflight-null-db",
            Guid.NewGuid().ToString("N"));
        try
        {
            var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
                new RecordingBrowserAdapter(Path.Combine(root, "browser-artifacts")),
                new AttemptScoreComparisonProvider(0.70, 0.82, 0.94),
                new AttemptScoreVisualJudge(0.70, 0.82, 0.94));
            var strategy = CreateScriptedCandidateStrategy("full-local-null-db", 3);
            var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                strategy,
                artifactDb: null,
                trainingIntakePreflightOptions: CreateFullLocalPreflightOptions(),
                utcNow: () => FullLocalPreflightNow.AddMinutes(6));
            var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);

            var envelope = await adapter.TryRunImprovementLoopAsync(CreateLoopPackage(
                "full-local-preflight-null-db-loop",
                "full-local-preflight-null-db-request",
                privateMarker,
                sourceVariant: "source-null-db"));

            Assert.Equal("success", envelope.EnvelopeKind);
            Assert.Equal(200, envelope.StatusCode);
            using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
            var boundaryMetadata = document.RootElement.GetProperty("boundaryMetadata");
            AssertNoTrainingIntakeHandoffClaim(boundaryMetadata, envelope.ResponseBodyJson);
            AssertNoLocalArtifactDbPersistenceClaim(
                boundaryMetadata,
                envelope.ResponseBodyJson);
            Assert.DoesNotContain(root, envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

    [Fact]
    public void PublicImprovementLoopCompositionOverloads_ExposeLocalOnlySignaturesAndNoExternalCarrier()
    {
        AssertPublicMethodSignature(
            typeof(DesignConversionModelServerEndpointRegistry),
            nameof(DesignConversionModelServerEndpointRegistry.RegisterDesignConversionImprovementLoopFromComposition),
            typeof(OperationResult<DesignConversionModelServerEndpointRegistration>),
            typeof(DesignConversionLocalServiceComposition),
            typeof(IDesignConversionImprovementCandidateStrategy),
            typeof(DesignConversionModelServerRouteAdmission));
        AssertPublicMethodSignature(
            typeof(DesignConversionModelServerEndpointRegistry),
            nameof(DesignConversionModelServerEndpointRegistry.RegisterDesignConversionImprovementLoopFromComposition),
            typeof(OperationResult<DesignConversionModelServerEndpointRegistration>),
            typeof(DesignConversionLocalServiceComposition),
            typeof(IDesignConversionImprovementCandidateStrategy),
            typeof(IDesignConversionLocalArtifactDb),
            typeof(Func<DateTimeOffset>),
            typeof(DesignConversionModelServerRouteAdmission));

        AssertPublicMethodSignature(
            typeof(DesignConversionModelServerAspNetHostStartup),
            nameof(DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry),
            typeof(DesignConversionModelServerEndpointRegistry),
            typeof(DesignConversionLocalServiceComposition),
            typeof(IDesignConversionImprovementCandidateStrategy));
        AssertPublicMethodSignature(
            typeof(DesignConversionModelServerAspNetHostStartup),
            nameof(DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry),
            typeof(DesignConversionModelServerEndpointRegistry),
            typeof(DesignConversionLocalServiceComposition),
            typeof(IDesignConversionImprovementCandidateStrategy),
            typeof(IDesignConversionLocalArtifactDb),
            typeof(Func<DateTimeOffset>),
            typeof(DesignConversionModelServerRouteAdmissionOptions));

        AssertPublicMethodSignature(
            typeof(DesignConversionModelServerAspNetHostStartup),
            nameof(DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry),
            typeof(DesignConversionModelServerEndpointRegistry),
            LegacyFullLocalStartupParameterTypes());
        AssertPublicMethodSignature(
            typeof(DesignConversionModelServerAspNetHostStartup),
            nameof(DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry),
            typeof(DesignConversionModelServerEndpointRegistry),
            LegacyPromotionHostStartupParameterTypes());

        foreach (var type in new[]
                 {
                     typeof(DesignConversionModelServerEndpointRegistry),
                     typeof(DesignConversionModelServerAspNetHostStartup),
                     typeof(DesignConversionModelServerAspNetEndpointMapper)
                 })
        {
            Assert.DoesNotContain(
                type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static |
                                System.Reflection.BindingFlags.Instance),
                candidate => candidate.Name.Contains("ExternalVisualProvider", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CompositionRegisteredRoute_UsesBrowserBackedLocalComposition()
    {
        const string privateMarker = "PRIVATE_PASS133_COMPOSITION_SECRET";
        var testRoot = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-composition-registered-route",
            Guid.NewGuid().ToString("N"));
        try
        {
            var browserAdapter = new RecordingBrowserAdapter(Path.Combine(testRoot, "browser-artifacts"));
            var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
            var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
                browserAdapter,
                comparisonProvider);
            var strategy = new RecordingCandidateStrategy();
            var registry = new DesignConversionModelServerEndpointRegistry();
            var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
                composition,
                strategy);
            Assert.True(registered.IsSuccess, registered.ErrorMessage);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.LocalCompositionImprovementLoopRegistrationBoundary,
                registered.Value!.BoundaryMetadata["improvement_loop_local_service_composition_boundary"]);
            Assert.Equal(bool.TrueString, registered.Value.BoundaryMetadata["browser_renderer_default_wired"]);

            var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);
            var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "pass133-composition-loop",
                InitialPackage: CreatePackage("pass133-composition-request", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["caller_private_marker"] = privateMarker
                });

            var envelope = await adapter.TryRunImprovementLoopAsync(package);

            Assert.Equal("success", envelope.EnvelopeKind);
            Assert.Equal(200, envelope.StatusCode);
            Assert.True(envelope.RegistryInvokeSucceeded);
            Assert.True(envelope.RuntimeInvoked);
            Assert.True(envelope.ParsedResponseJsonSucceeded);
            Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
            Assert.Equal("pass133-composition-request", envelope.ParsedRequestId);
            Assert.Null(envelope.ParsedErrorCode);
            Assert.False(envelope.RawHtmlIncluded);
            Assert.False(envelope.RawCssIncluded);
            Assert.False(envelope.RawDesignTreeIncluded);
            Assert.False(envelope.ResponseContainsRawPrivateData);
            Assert.False(envelope.RawPrivateResponseLeakageDetected);
            Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);

            Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_only_public_response"]);
            Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["improvement_loop_registered_from_local_service_composition"]);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.LocalCompositionImprovementLoopRegistrationBoundary,
                envelope.BoundaryMetadata["improvement_loop_local_service_composition_boundary"]);
            Assert.Equal(
                DesignConversionLocalServiceComposition.BrowserBackedLocalCompositionKind,
                envelope.BoundaryMetadata["composition_kind"]);
            Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["explicit_local_service_composition"]);
            Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["browser_renderer_default_wired"]);
            Assert.Equal("explicit_local_composition_only", envelope.BoundaryMetadata["browser_renderer_default_scope"]);
            Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, envelope.BoundaryMetadata["render_provider_kind"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_production_default_ready"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_ai_visual_judge_executed"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["network_call_executed"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_html_included"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_css_included"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_design_tree_included"]);

            using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
            var root = document.RootElement;
            Assert.Equal(DesignConversionImprovementLoopCoordinator.CandidateStrategyFailed, root.GetProperty("stopReason").GetString());
            Assert.False(root.GetProperty("completed").GetBoolean());
            Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
            var attempt = root.GetProperty("attempts")[0];
            Assert.Equal(2, attempt.GetProperty("renderResultRefs").GetArrayLength());
            Assert.Equal(2, attempt.GetProperty("comparisonArtifactRefs").GetArrayLength());
            Assert.Equal(2, attempt.GetProperty("visualJudgeResultRefs").GetArrayLength());
            var attemptMetadata = attempt.GetProperty("publicMetadata");
            Assert.Equal("2", attemptMetadata.GetProperty("render_result_count").GetString());
            Assert.Equal("2", attemptMetadata.GetProperty("comparison_artifact_count").GetString());
            Assert.Equal("2", attemptMetadata.GetProperty("visual_judge_result_count").GetString());
            Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
            Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("product_ready").GetString());

            Assert.Equal(2, browserAdapter.Requests.Count);
            Assert.Equal(2, comparisonProvider.Requests.Count);
            Assert.Single(strategy.Requests);
        }
        finally
        {
            DeleteDirectoryIfExists(testRoot);
        }
    }

    private static DesignConversionModelServerLocalCallerPackage CreatePackage(
        string requestId,
        string privateMarker,
        int maxImprovementCandidates = 2,
        int maxRenderQaRuns = 2,
        string? sourceVariant = null)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{SourceToken(requestId, sourceVariant)}:design-tree-ref",
                Kind: "figma-json",
                Sha256: SourceManifestSha(requestId, sourceVariant),
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"generated\",\"source\":\"{sourceVariant ?? "default"}\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker,
                    ["source_variant"] = sourceVariant ?? "default"
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{SourceToken(requestId, sourceVariant)}:original-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{SourceToken(requestId, sourceVariant)}:original-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"original\",\"source\":\"{sourceVariant ?? "default"}\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker,
                    ["source_variant"] = sourceVariant ?? "default"
                }),
            Html: $"<main data-secret=\"{privateMarker}\">Pass133</main>",
            Css: $".pass133{{background-image:url('{privateMarker}');}}",
            Screenshots:
            [
                CreateArtifact($"{requestId}:source-desktop", "screenshot", 1440, 900),
                CreateArtifact($"{requestId}:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                CreateArtifact($"{requestId}:design-desktop", "design-render", 1440, 900),
                CreateArtifact($"{requestId}:design-mobile", "design-render", 390, 844)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "pass133-local-loop",
                MaxImprovementCandidates: maxImprovementCandidates,
                MaxRenderQaRuns: maxRenderQaRuns,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90",
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker,
                ["source_variant"] = sourceVariant ?? "default"
            });

    private static RecordingAtomicPromotionLocalArtifactDb CreateRecordingArtifactDb(
        string root,
        string? sourceDisjointCompanionSourceManifestRef = null)
        => new(
            new JsonlDesignConversionLocalArtifactDb(Path.Combine(root, "artifact-db")),
            root,
            sourceDisjointCompanionSourceManifestRef);

    private static DesignConversionLocalIndependentPreferenceEvaluatorRuntime
        OwnLocalEvaluatorRuntime(
            IDesignConversionLocalArtifactDb artifactDb,
            LocalIndependentEvaluationClient client,
            string runtimeScope)
    {
        var evaluator =
            DesignConversionLocalIndependentPreferenceEvaluator.OwnForTesting(
                client,
                LocalIndependentEvaluationClient.ModelId,
                LocalIndependentEvaluationClient.ManifestDigest,
                LocalIndependentEvaluationClient.ModelLayerDigest,
                DesignConversionDpoGenerator.Sha256Ref(runtimeScope));
        var owned = DesignConversionLocalIndependentPreferenceEvaluatorRuntime
            .OwnForTesting(artifactDb, evaluator);
        Assert.True(owned.IsSuccess, owned.ErrorMessage);
        return owned.Value!;
    }

    private static DesignConversionTrainingIntakePreflightCoordinatorOptions
        CreateFullLocalPreflightOptions()
        => new(
            enabled: true,
            dpoStepCount: FullLocalPreflightDpoStepCount,
            runTrainingIntakeAndPromoteLocal: true);

    private static DesignConversionModelServerImprovementLoopLocalCallerPackage
        CreateLoopPackage(
            string loopId,
            string requestId,
            string privateMarker,
            string? sourceVariant = null,
            int maxAttempts = 3)
        => new(
            LoopId: loopId,
            InitialPackage: CreatePackage(
                requestId,
                privateMarker,
                maxImprovementCandidates: maxAttempts,
                maxRenderQaRuns: maxAttempts,
                sourceVariant: sourceVariant) with
            {
                MeasurementGateRequest = BuildRouteAdmissionMeasurement(loopId)
            },
            MaxAttempts: maxAttempts,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker,
                ["source_variant"] = sourceVariant ?? "default"
            });

    private static DesignConversionMeasurementGateRequest
        BuildRouteAdmissionMeasurement(string scope)
        => new(
            GateId: $"modelserver-admission:{scope}",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                2,
                0.95,
                10,
                0.01m,
                64,
                [$"artifact://measurement/{scope}/heldout.json"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                true,
                false,
                false,
                true,
                [$"artifact://measurement/{scope}/integrity.json"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                0.90,
                100,
                1m,
                512));

    private static IReadOnlyDictionary<string, string> RouteAdmissionHeaders(
        string idempotencyKey)
        => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] =
                "Bearer request-scoped-route-token",
            [DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] =
                idempotencyKey
        };

    private static Task<OperationResult<DesignConversionModelServerHttpResponse>>
        InvokeImprovementLoopRouteAsync(
            DesignConversionModelServerEndpointRegistry registry,
            DesignConversionModelServerImprovementLoopRequest routeRequest,
            string idempotencyKey)
        => registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(
                routeRequest,
                DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: RouteAdmissionHeaders(idempotencyKey)));

    private static void AssertAtomicPromotionEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        RecordingAtomicPromotionLocalArtifactDb artifactDb,
        string testRoot,
        string privateMarker,
        IReadOnlyList<string> expectedSourceManifestRefs)
    {
        Assert.True(
            string.Equals("success", envelope.EnvelopeKind, StringComparison.Ordinal),
            artifactDb.LastApprovalError ??
            artifactDb.LastAppendDiagnostic + Environment.NewLine +
            envelope.ResponseBodyJson);
        Assert.Equal(200, envelope.StatusCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.True(envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        AssertAtomicPromotionBody(
            envelope.ResponseBodyJson,
            artifactDb,
            testRoot,
            privateMarker,
            expectedSourceManifestRefs);
    }

    private static void AssertAtomicPromotionBody(
        string responseBodyJson,
        RecordingAtomicPromotionLocalArtifactDb artifactDb,
        string testRoot,
        string privateMarker,
        IReadOnlyList<string> expectedSourceManifestRefs,
        bool? expectedExternalVisualProviderRequestConfigPresent = null)
    {
        Assert.True(
            artifactDb.RunTrainingIntakePreflightCalls >= 1,
            $"preflight_calls={artifactDb.RunTrainingIntakePreflightCalls}" +
            Environment.NewLine +
            artifactDb.LastAppendDiagnostic +
            Environment.NewLine +
            (artifactDb.LastApprovalError ?? string.Empty) +
            Environment.NewLine +
            responseBodyJson);
        Assert.Equal(artifactDb.LastCreatedIntakeId, artifactDb.LastPreflightIntakeId);
        Assert.Equal(artifactDb.LastCreatedIntakeId, artifactDb.LastAtomicIntakeId);
        Assert.NotNull(artifactDb.LastPreflightResult);
        Assert.True(
            artifactDb.LastPreflightResult!.MayStartTraining,
            string.Join("|", artifactDb.LastPreflightResult.BlockReasons) +
            Environment.NewLine +
            responseBodyJson);
        Assert.True(
            artifactDb.RunTrainingIntakeAndPromoteLocalCalls == 1,
            $"atomic_calls={artifactDb.RunTrainingIntakeAndPromoteLocalCalls}" +
            Environment.NewLine +
            responseBodyJson);
        Assert.True(artifactDb.LastPreflightResult.AcceptedCandidateCount >= 2);
        Assert.Equal("proven", artifactDb.LastPreflightResult.SourceManifestStatus);
        Assert.Equal("proven", artifactDb.LastPreflightResult.SourceDisjointStatus);
        Assert.DoesNotContain(
            "source_disjoint_blocked_same_source",
            artifactDb.LastPreflightResult.BlockReasons);
        var preflightMetadata = artifactDb.LastPreflightResult.BoundaryMetadata;
        Assert.Equal(
            artifactDb.LastPreflightResult.AcceptedCandidateCount.ToString(
                CultureInfo.InvariantCulture),
            preflightMetadata["source_manifest_candidate_count"]);
        Assert.Equal(
            artifactDb.LastCreatedIntakeSourceManifestRefs
                .Distinct(StringComparer.Ordinal)
                .Count()
                .ToString(CultureInfo.InvariantCulture),
            preflightMetadata["source_manifest_hash_count"]);
        Assert.True(int.Parse(
            preflightMetadata["source_disjoint_train_source_count"],
            CultureInfo.InvariantCulture) >= 1);
        Assert.True(int.Parse(
            preflightMetadata["source_disjoint_heldout_source_count"],
            CultureInfo.InvariantCulture) >= 1);
        Assert.Equal(
            "0",
            preflightMetadata["source_disjoint_overlap_count"]);
        Assert.All(
            expectedSourceManifestRefs,
            sourceManifestRef => Assert.Contains(
                sourceManifestRef,
                artifactDb.LastCreatedIntakeSourceManifestRefs));
        Assert.Equal(
            artifactDb.LastCreatedIntakeSourceManifestRefs
                .Distinct(StringComparer.Ordinal)
                .Count(),
            artifactDb.LastCreatedIntakeSourceIdentityHashes
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.All(
            artifactDb.LastCreatedIntakeSourceIdentityHashes,
            sourceIdentityHash =>
                Assert.StartsWith("sha256:", sourceIdentityHash, StringComparison.Ordinal));
        Assert.NotNull(artifactDb.LastAtomicTrainingOptions);
        Assert.NotNull(artifactDb.LastAtomicTrainingOptions!.MeasurementGateRequest);
        Assert.Equal(
            FullLocalPreflightDpoStepCount,
            artifactDb.LastAtomicTrainingOptions.DpoPolicyBridgeOptions!.DpoStepCount);
        Assert.False(string.IsNullOrWhiteSpace(
            artifactDb.LastAtomicTrainingOptions.MeasurementGateRequest!.GateId));
        Assert.Equal(
            DesignConversionDpoUnits.CriticUnit,
            artifactDb.LastAtomicTrainingOptions.MeasurementGateRequest.UnitId);

        using var document = JsonDocument.Parse(responseBodyJson);
        var responseRoot = document.RootElement;
        var boundaryMetadata = responseRoot.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty(
            "refs_only_public_response").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty(
            "raw_html_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty(
            "raw_css_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty(
            "raw_design_tree_included").GetString());
        if (expectedExternalVisualProviderRequestConfigPresent is { } requestConfigPresent)
        {
            Assert.Equal(
                requestConfigPresent ? bool.TrueString : bool.FalseString,
                boundaryMetadata.GetProperty(
                    "external_visual_provider_request_config_present").GetString());
        }

        Assert.Contains(
            responseRoot.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => string.Equals(
                "fake-atomic-run-promote-evidence",
                evidence.GetString(),
                StringComparison.Ordinal));
        Assert.False(boundaryMetadata.TryGetProperty(
            "training_intake_local_promotion_output_directory",
            out _));
        Assert.False(boundaryMetadata.TryGetProperty(
            "training_intake_local_promotion_checkpoint_path",
            out _));
        Assert.DoesNotContain(testRoot, responseBodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fake-promotion-output", responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("fake-checkpoint.json", responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, responseBodyJson, StringComparison.Ordinal);
    }

    private static void AssertNoTrainingIntakeHandoffClaim(
        JsonElement boundaryMetadata,
        string responseBodyJson)
    {
        Assert.False(boundaryMetadata.TryGetProperty(
            "training_intake_atomic_run_promote_invoked",
            out _));
        Assert.False(boundaryMetadata.TryGetProperty(
            DesignConversionImprovementLoopCoordinator
                .TrainingIntakePreflightHandoffStatusMetadataKey,
            out _));
        Assert.False(boundaryMetadata.TryGetProperty(
            "training_intake_local_promotion_ready",
            out _));
        Assert.False(boundaryMetadata.TryGetProperty(
            "training_intake_local_promotion_output_directory",
            out _));
        Assert.False(boundaryMetadata.TryGetProperty(
            "training_intake_local_promotion_checkpoint_path",
            out _));
        Assert.DoesNotContain(
            "fake-atomic-run-promote-evidence",
            responseBodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "fake-promotion-output",
            responseBodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "fake-checkpoint.json",
            responseBodyJson,
            StringComparison.Ordinal);
    }

    private static void AssertNoLocalArtifactDbPersistenceClaim(
        JsonElement boundaryMetadata,
        string responseBodyJson)
    {
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty(
            "persistence_invoked").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty(
            DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker).GetString());
        Assert.Equal("not_configured_non_durable", boundaryMetadata.GetProperty(
            "local_artifact_db_persistence_status").GetString());
        Assert.DoesNotContain(
            DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker,
            responseBodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain("\"runtime_created_dpo_candidate\":\"True\"", responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("\"runtime_created_dpo_candidate\":true", responseBodyJson, StringComparison.Ordinal);
    }

    private static void AssertPublicMethodSignature(
        Type declaringType,
        string methodName,
        Type returnType,
        params Type[] parameterTypes)
    {
        var matches = declaringType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(method => string.Equals(method.Name, methodName, StringComparison.Ordinal))
            .Where(method => method
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
                .SequenceEqual(parameterTypes))
            .ToArray();

        Assert.True(
            matches.Length == 1,
            $"Expected exactly one public {declaringType.Name}.{methodName} signature with parameters " +
            $"{FormatParameterTypes(parameterTypes)}, found {matches.Length}." +
            Environment.NewLine +
            FormatPublicSignatures(declaringType, methodName));
        Assert.Equal(returnType, matches[0].ReturnType);
    }

    private static Type[] LegacyFullLocalStartupParameterTypes()
        =>
        [
            typeof(DesignConversionLocalServiceComposition),
            typeof(IDesignConversionImprovementCandidateStrategy),
            typeof(IDesignConversionLocalArtifactDb),
            typeof(Func<DateTimeOffset>),
            typeof(DesignConversionPromotedLocalModel),
            typeof(string),
            typeof(string),
            typeof(DesignConversionModelServerModelLifecycleTokenKeyRingOptions),
            typeof(string),
            typeof(IDesignConversionModelServerArtifactRefResolver),
            typeof(DesignConversionModelServerRouteAdmissionOptions),
            typeof(IDesignConversionVisualCostLedger),
            typeof(IDesignConversionVisualCostBudgetStore),
            typeof(IDesignConversionFlow38QuarantineStore),
            typeof(string),
            typeof(DesignConversionModelServerMeasurementGateThresholdConfiguration)
        ];

    private static Type[] LegacyFullLocalRequestScopedStartupParameterTypes()
        =>
        [
            typeof(DesignConversionLocalServiceComposition),
            typeof(IDesignConversionImprovementCandidateStrategy),
            typeof(IDesignConversionLocalArtifactDb),
            typeof(HttpClient),
            typeof(Func<DateTimeOffset>),
            typeof(DesignConversionPromotedLocalModel),
            typeof(string),
            typeof(string),
            typeof(DesignConversionModelServerModelLifecycleTokenKeyRingOptions),
            typeof(string),
            typeof(IDesignConversionModelServerArtifactRefResolver),
            typeof(DesignConversionModelServerRouteAdmissionOptions),
            typeof(IDesignConversionVisualCostLedger),
            typeof(IDesignConversionVisualCostBudgetStore),
            typeof(IDesignConversionFlow38QuarantineStore),
            typeof(string),
            typeof(DesignConversionModelServerMeasurementGateThresholdConfiguration)
        ];

    private static Type[] LegacyPromotionHostStartupParameterTypes()
        =>
        [
            typeof(DesignConversionLocalServiceComposition),
            typeof(IDesignConversionImprovementCandidateStrategy),
            typeof(DesignConversionModelServerPromotionHostOptions),
            typeof(Func<DateTimeOffset>),
            typeof(DesignConversionPromotedLocalModel),
            typeof(string),
            typeof(string),
            typeof(DesignConversionModelServerModelLifecycleTokenKeyRingOptions),
            typeof(string),
            typeof(IDesignConversionModelServerArtifactRefResolver),
            typeof(DesignConversionModelServerRouteAdmissionOptions),
            typeof(IDesignConversionFlow38QuarantineStore),
            typeof(string),
            typeof(DesignConversionModelServerMeasurementGateThresholdConfiguration)
        ];

    private static Type[] LegacyPromotionHostRequestScopedStartupParameterTypes()
        =>
        [
            typeof(DesignConversionLocalServiceComposition),
            typeof(IDesignConversionImprovementCandidateStrategy),
            typeof(DesignConversionModelServerPromotionHostOptions),
            typeof(HttpClient),
            typeof(Func<DateTimeOffset>),
            typeof(DesignConversionPromotedLocalModel),
            typeof(string),
            typeof(string),
            typeof(DesignConversionModelServerModelLifecycleTokenKeyRingOptions),
            typeof(string),
            typeof(IDesignConversionModelServerArtifactRefResolver),
            typeof(DesignConversionModelServerRouteAdmissionOptions),
            typeof(IDesignConversionFlow38QuarantineStore),
            typeof(string),
            typeof(DesignConversionModelServerMeasurementGateThresholdConfiguration)
        ];

    private static string FormatPublicSignatures(
        Type declaringType,
        string methodName)
        => string.Join(
            Environment.NewLine,
            declaringType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(method => string.Equals(method.Name, methodName, StringComparison.Ordinal))
                .Select(method =>
                    $"{method.ReturnType.Name} {method.Name}(" +
                    $"{FormatParameterTypes(method.GetParameters().Select(parameter => parameter.ParameterType))})"));

    private static string FormatParameterTypes(IEnumerable<Type> parameterTypes)
        => string.Join(
            ", ",
            parameterTypes.Select(parameterType =>
                parameterType.FullName ?? parameterType.Name));

    private static ScriptedCandidateStrategy CreateScriptedCandidateStrategy(
        string prefix,
        int count)
        => new(
            Enumerable.Range(1, count)
                .Select(index => OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreateCandidate(
                        $"{prefix}-candidate-{index + 1:000}",
                        $"<main class=\"{prefix}-candidate-{index + 1:000}\">candidate-{index}</main>",
                        $".{prefix}-candidate-{index + 1:000}{{display:grid;gap:{8 + index}px;}}")))
                .ToArray());

    private static string SourceToken(string requestId, string? sourceVariant)
        => string.IsNullOrWhiteSpace(sourceVariant)
            ? requestId
            : $"{requestId}:{sourceVariant}";

    private static string SourceManifestSha(string requestId, string? sourceVariant)
        => DesignConversionDpoGenerator.Sha256Ref(
            $"source-manifest|{SourceToken(requestId, sourceVariant)}|design-tree");

    private static double[] CreateExpandedScores(params double[] attemptScores)
        => CreateRepeatedScores(4, attemptScores);

    private static double[] CreateRepeatedScores(int repeatCount, params double[] attemptScores)
        => attemptScores
            .SelectMany(score => Enumerable.Repeat(score, repeatCount))
            .ToArray();

    private static async Task<DesignConversionModelServerLocalCallerPackage>
        CreatePackageWithServerOwnedPngArtifactsAsync(
            string requestId,
            string privateMarker,
            string artifactRoot,
            string? sourceVariant = null)
    {
        var package = CreatePackage(
            requestId,
            privateMarker,
            maxImprovementCandidates: 3,
            maxRenderQaRuns: 3,
            sourceVariant: sourceVariant);
        var sourceDesktop = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            sourceVariant,
            "source-desktop",
            "screenshot",
            1440,
            900);
        var sourceMobile = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            sourceVariant,
            "source-mobile",
            "screenshot",
            390,
            844);
        var designDesktop = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            sourceVariant,
            "design-desktop",
            "design-render",
            1440,
            900);
        var designMobile = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            sourceVariant,
            "design-mobile",
            "design-render",
            390,
            844);

        return package with
        {
            Screenshots = [sourceDesktop, sourceMobile],
            DesignRenders = [designDesktop, designMobile]
        };
    }

    private static async Task<DesignConversionModelServerArtifactRef>
        CreateServerOwnedPngArtifactAsync(
            string artifactRoot,
            string requestId,
            string? sourceVariant,
            string artifactSuffix,
            string kind,
            int width,
            int height)
    {
        var renderProvider = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot);
        var resolution = new RenderQaResolution(
            $"{artifactSuffix}-{width.ToString(CultureInfo.InvariantCulture)}x{height.ToString(CultureInfo.InvariantCulture)}",
            width,
            height,
            1.0);
        var render = await renderProvider.RenderAsync(new DesignConversionRenderRequest(
            RenderRequestId: $"{SourceToken(requestId, sourceVariant)}:{artifactSuffix}:server-owned-png",
            RequestId: requestId,
            Html: $"<main class=\"server-owned-png-fixture\">{artifactSuffix}</main>",
            Css: ".server-owned-png-fixture{color:#123456;background:#f8fafc}",
            Resolution: resolution,
            Metadata: new Dictionary<string, string>
            {
                ["fixture_role"] = kind,
                ["fixture_artifact_suffix"] = artifactSuffix,
                ["source_variant"] = sourceVariant ?? "default"
            }));
        Assert.True(render.IsSuccess, render.ErrorMessage);

        var image = render.Value!.Image;
        return new DesignConversionModelServerArtifactRef(
            ArtifactId: $"{SourceToken(requestId, sourceVariant)}:{artifactSuffix}",
            Kind: kind,
            Width: width,
            Height: height,
            Uri: image.Sha256,
            Sha256: image.Sha256,
            Metadata: image.Metadata
                .Where(pair => !DesignConversionRenderResolutionFactory
                    .IsViewportIdentityMetadataKey(pair.Key))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private static DesignConversionModelServerArtifactRef CreateArtifact(
        string id,
        string kind,
        int width,
        int height)
        => new(
            ArtifactId: id,
            Kind: kind,
            Width: width,
            Height: height,
            Uri: $"artifact://pass133/{id}.png",
            Sha256: DesignConversionDpoGenerator.Sha256Ref(
                $"modelserver-composition-artifact|{id}"));

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string ReadMetadata(
        IReadOnlyDictionary<string, string> metadata,
        string key)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : "<missing>";

    private static DesignConversionImprovementCandidate CreateCandidate(
        string candidateId,
        string? html = null,
        string? css = null)
        => new(
            CandidateId: candidateId,
            RequestId: $"{candidateId}-request",
            Html: html ?? "<main class=\"improved\">PRIVATE_HTML_MARKER_FIXED</main>",
            Css: css ?? ".improved{display:grid;gap:12px;}",
            ChangeReasons: ["scripted local composition candidate"],
            EvidenceRefs: [$"candidate:{candidateId}"],
            Metadata: new Dictionary<string, string>
            {
                ["local_strategy"] = "scripted"
            });

    private static DesignConversionTrainingIntakeLocalPromotionResult
        CreateFakeAtomicPromotionResult(
            string intakeId,
            DesignConversionTrainingPipelineOptions? options,
            string testRoot)
    {
        var run = new DesignConversionTrainingIntakeRunResult(
            RunId: "fake-atomic-run",
            TrainingIntakeId: intakeId,
            PreflightId: "fake-preflight",
            AcceptedCandidateCount: 1,
            ResolvedTrainingRecordCount: 1,
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
            TrainingExecutionBoundary: "fake-atomic-boundary",
            TrainingExecutionStatus: "completed",
            RunnerBlockedReason: string.Empty,
            CheckpointStatus: "created",
            CheckpointMutationStatus: "mutated",
            WeightProofStatus: "fake-weight-proof",
            MeasurementGateStatus: options?.MeasurementGateRequest is null
                ? "missing"
                : DesignConversionMeasurementGateStatus.Passed,
            PromotionGateStatus: "passed",
            UploadReadinessStatus: "ready",
            PromotionBlockedReasons: [],
            RunnerEvidence: null,
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata:
                new Dictionary<string, string>(StringComparer.Ordinal));
        return new DesignConversionTrainingIntakeLocalPromotionResult(
            TrainingRun: run,
            LocalPromotionReady: true,
            ExternalCloudUploadAttempted: false,
            PromotionOutputDirectory:
                Path.Combine(testRoot, "fake-promotion-output"),
            PromotedCheckpointPath:
                Path.Combine(testRoot, "fake-promotion-output", "fake-checkpoint.json"),
            Publication: null,
            PromotedLocalModel: null,
            EvidenceRefs: ["fake-atomic-run-promote-evidence"],
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata:
                new Dictionary<string, string>(StringComparer.Ordinal));
    }

    private sealed class LocalIndependentEvaluationClient
        : ILegalTeacherEvaluationInferenceClient
    {
        internal const string ModelId =
            "qwen-modelserver-composition-evaluator-test:9b";
        internal static readonly string ManifestDigest = new('c', 64);
        internal static readonly string ModelLayerDigest = new('d', 64);
        private int _callCount;

        internal int CallCount => Volatile.Read(ref _callCount);

        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var permit = callPermit.TryIssue();
            if (!permit.IsSuccess)
            {
                return Task.FromResult(
                    permit.MapError<TeacherEvaluationInferenceResult>());
            }

            var call = Interlocked.Increment(ref _callCount);
            using var document = JsonDocument.Parse(prompt);
            var root = document.RootElement;
            var output = JsonSerializer.Serialize(new
            {
                schema = DesignConversionLocalIndependentPreferenceEvaluator
                    .ResultSchema,
                scorer_request_sha256 = root
                    .GetProperty("scorer_request_sha256").GetString(),
                generated_evaluation_identity = root
                    .GetProperty("generated_evaluation_identity").GetString(),
                incumbent_evaluation_identity = root
                    .GetProperty("incumbent_evaluation_identity").GetString(),
                preferred_direction =
                    DesignConversionIndependentPreferenceEvidenceContract
                        .GeneratedSide
            });
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                ModelId,
                ManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: false,
                RequestId: "modelserver-composition-local-evaluation-" + call,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = ModelLayerDigest
            };
            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Ok(
                    new TeacherEvaluationInferenceResult(output, provenance)
                    {
                        InferenceProfile =
                            LocalQwenOllamaInferenceProfileAuthority.Evaluation
                    }));
        }

        public void Dispose()
        {
        }
    }

    private sealed class RecordingBrowserAdapter(string artifactRoot)
        : IDesignConversionBrowserProcessAdapter
    {
        private readonly string _artifactRoot = !string.IsNullOrWhiteSpace(artifactRoot)
            ? artifactRoot
            : throw new ArgumentException(
                "An explicit test-owned artifact root is required.",
                nameof(artifactRoot));

        public List<DesignConversionBrowserCaptureRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var render = await new DeterministicLocalDesignConversionPngRenderProvider(_artifactRoot)
                .RenderAsync(
                    new DesignConversionRenderRequest(
                        RenderRequestId: $"{request.CaptureRequestId}:recording-browser-png",
                        RequestId: request.RequestId,
                        Html: request.DocumentHtml,
                        Css: "body{margin:0}.pass133-browser-proof{color:#123456;background:#f8fafc}",
                        Resolution: request.Resolution,
                        Metadata: request.Metadata),
                    cancellationToken);

            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserCaptureResult>();

            var sha = render.Value!.Image.Sha256["sha256:".Length..];
            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: render.Value.Image.Uri,
                    ScreenshotSha256: sha,
                    ElapsedMs: 5,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "pass133-fake-browser-adapter",
                        ["browser_process_boundary"] = "adapter_supplied_inspectable_png_file"
                    },
                    Warnings: []));
        }
    }

    private sealed class RecordingComparisonProvider(double score) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:pass133-verdict",
                Score: score,
                Defects: [],
                EvidenceRefs: [$"pass133-comparison:{request.ComparisonRequestId}"],
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{request.ComparisonRequestId}:pass133-artifact",
                    ComparisonRequestId: request.ComparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"artifact://pass133/diff/{Sha256Text(request.ComparisonRequestId)}.json",
                    DiffImageSha256: $"sha256:{Sha256Text(request.ComparisonRequestId + ":diff")}",
                    Metrics: new Dictionary<string, double>
                    {
                        ["visual_distance"] = 1.0 - score
                    },
                    EvidenceRefs: ["pass133-comparison-provider"])
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["external_ai_visual_judge_executed"] = bool.FalseString,
                        ["visual_ai_executed"] = bool.FalseString,
                        ["raw_html_included"] = bool.FalseString,
                        ["raw_css_included"] = bool.FalseString,
                        ["raw_design_tree_included"] = bool.FalseString
                    }
                }));
        }
    }

    private sealed class AttemptScoreComparisonProvider(params double[] scores)
        : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var attemptIndex = Math.Max(0, Requests.Count - 1);
            var score = attemptIndex < scores.Length
                ? scores[attemptIndex]
                : scores[^1];
            var passed = score >= 0.90;
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:attempt-score-verdict",
                Score: score,
                Defects: passed
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.ComparisonRequestId}:attempt-score-defect",
                            Category: DesignConversionNegativeReasonTaxonomy.Spacing,
                            Severity: "minor",
                            Description: "Scripted comparison result for full-local composition tests.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds:
                            [
                                request.Expected.ImageId,
                                request.Actual.ImageId
                            ])
                    ],
                EvidenceRefs: [$"attempt-score-comparison:{request.ComparisonRequestId}"],
                Passed: passed);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{request.ComparisonRequestId}:attempt-score-artifact",
                    ComparisonRequestId: request.ComparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"artifact://attempt-score/diff/{Sha256Text(request.ComparisonRequestId)}.json",
                    DiffImageSha256: $"sha256:{Sha256Text(request.ComparisonRequestId + ":diff")}",
                    Metrics: new Dictionary<string, double>
                    {
                        ["visual_distance"] = 1.0 - score
                    },
                    EvidenceRefs: ["attempt-score-comparison-provider"])
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["external_ai_visual_judge_executed"] = bool.FalseString,
                        ["visual_ai_executed"] = bool.FalseString,
                        ["raw_html_included"] = bool.FalseString,
                        ["raw_css_included"] = bool.FalseString,
                        ["raw_design_tree_included"] = bool.FalseString
                    }
                }));
        }
    }

    private sealed class RecordingCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "Pass133 composition test expects quality reached before a candidate is requested.",
                "PASS133_UNEXPECTED_CANDIDATE_REQUEST"));
        }
    }

    private sealed class ScriptedCandidateStrategy(
        params OperationResult<DesignConversionImprovementCandidate>[] results)
        : IDesignConversionImprovementCandidateStrategy
    {
        private readonly Queue<OperationResult<DesignConversionImprovementCandidate>> _results = new(results);

        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(
                _results.Count == 0
                    ? OperationResult<DesignConversionImprovementCandidate>.Fail(
                        "No scripted candidate result was configured.",
                        "SCRIPTED_CANDIDATE_MISSING")
                    : _results.Dequeue());
        }
    }

    private sealed class AttemptScoreVisualJudge(params double[] scores)
        : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var attemptIndex = Math.Max(0, Requests.Count - 1);
            var score = attemptIndex < scores.Length
                ? scores[attemptIndex]
                : scores[^1];
            var passed = score >= 0.90;
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:attempt-score-verdict",
                Score: score,
                Defects: passed
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.JudgeRequestId}:attempt-score-defect",
                            Category: DesignConversionNegativeReasonTaxonomy.Spacing,
                            Severity: "minor",
                            Description: "Scripted visual judge result for composition startup tests.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds:
                            [
                                request.ExpectedScreenshot.ImageId,
                                request.RenderedImage.ImageId
                            ])
                    ],
                EvidenceRefs: [$"visual-judge:{request.JudgeRequestId}"],
                Passed: passed);
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(
                new DesignConversionVisualJudgeResult(
                    JudgeResultId: $"{request.JudgeRequestId}:attempt-score-result",
                    JudgeRequestId: request.JudgeRequestId,
                    Status: "SCRIPTED_VISUAL_JUDGE_VERDICT_RECEIVED",
                    IsBlocked: false,
                    Verdict: verdict,
                    Provider: new DesignConversionVisualJudgeProviderMetadata(
                        ProviderName: "scripted-local-judge",
                        EndpointHost: "none",
                        ModelId: "scripted-local-v1",
                        ExternalCallExecuted: false,
                        Metadata: new Dictionary<string, string>
                        {
                            ["external_ai_visual_judge_executed"] =
                                bool.FalseString
                        }),
                    EstimatedCostUsd: 0.01m,
                    TokenMetadata: new Dictionary<string, double>(),
                    EvidenceRefs: verdict.EvidenceRefs,
                    Metadata: new Dictionary<string, string>
                    {
                        ["external_ai_visual_judge_executed"] = bool.FalseString
                    })));
        }
    }

    private sealed class SequencedExternalVisualJudgeHandler(double[] scores)
        : HttpMessageHandler
    {
        public const string ProviderName = "openai-request-scoped-composition-provider";
        public const string ModelId = "gpt-request-scoped-composition-model-v1";

        public int CallCount { get; private set; }
        public string LastProviderName { get; private set; } = string.Empty;
        public string LastModelId { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            var requestJson = request.Content is null
                ? "{}"
                : await request.Content.ReadAsStringAsync(cancellationToken);
            using var requestDocument = JsonDocument.Parse(requestJson);
            var requestRoot = requestDocument.RootElement;
            var judgeRequestId = ReadString(requestRoot, "judge_request_id");
            var requestId = ReadString(requestRoot, "request_id");
            var score = scores[Math.Min(CallCount - 1, scores.Length - 1)];
            var passed = score >= 0.90;
            LastProviderName = ProviderName;
            LastModelId = ModelId;
            object[] defects = passed
                ? []
                :
                [
                    new
                    {
                        defect_id = $"{judgeRequestId}:request-scoped-provider-defect",
                        category = DesignConversionNegativeReasonTaxonomy.Spacing,
                        severity = "minor",
                        description = "Request-scoped external visual provider reported measured spacing drift.",
                        x = 0,
                        y = 0,
                        width = 1,
                        height = 1
                    }
                ];
            var response = new
            {
                judge_request_id = judgeRequestId,
                request_id = requestId,
                provider = ProviderName,
                provider_name = ProviderName,
                model_id = ModelId,
                judge_result_id = $"{judgeRequestId}:request-scoped-provider-result",
                verdict_id = $"{judgeRequestId}:request-scoped-provider-verdict",
                score,
                passed,
                defects,
                evidence_refs = new[]
                {
                    "request-scoped-provider:evidence",
                    "request-scoped-provider:source-disjoint-fixture"
                },
                confidence = 0.99,
                tokens = new Dictionary<string, double>
                {
                    ["input"] = 100,
                    ["output"] = 20,
                    ["total"] = 120
                }
            };
            var body = JsonSerializer.Serialize(
                response,
                DesignConversionModelServerHttpHandler.JsonOptions);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }

        private static string ReadString(JsonElement root, string propertyName)
            => root.TryGetProperty(propertyName, out var value) &&
               value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
    }

    private sealed class RecordingAtomicPromotionLocalArtifactDb(
        JsonlDesignConversionLocalArtifactDb inner,
        string testRoot,
        string? sourceDisjointCompanionSourceManifestRef = null)
        : IDesignConversionLocalArtifactDb,
          IDesignConversionLocalPromotionActivationAuthority,
          IDesignConversionPostEvaluationStateRootProvider
    {
        private bool _sourceDisjointCompanionAppended;
        private DesignConversionLocalArtifactDbRecord?
            _lastAppendedRecordWithPayload;

        public int RunTrainingIntakeAndPromoteLocalCalls { get; private set; }
        public string? LastCreatedIntakeId { get; private set; }
        public string? LastPreflightIntakeId { get; private set; }
        public int RunTrainingIntakePreflightCalls { get; private set; }
        public DesignConversionTrainingIntakePreflightResult? LastPreflightResult { get; private set; }
        public string? LastAtomicIntakeId { get; private set; }
        public string? LastApprovalError { get; private set; }
        public DesignConversionTrainingPipelineOptions? LastAtomicTrainingOptions { get; private set; }
        public string LastAppendDiagnostic { get; private set; } = "append_not_observed";
        public IReadOnlyList<string> LastCreatedIntakeSourceManifestRefs { get; private set; } =
            [];
        public IReadOnlyList<string> LastCreatedIntakeSourceIdentityHashes { get; private set; } =
            [];

        public string AllowedArtifactRootDirectory =>
            inner.AllowedArtifactRootDirectory;

        public string PostEvaluationStateRootDirectory =>
            inner.PostEvaluationStateRootDirectory;

        public async Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordAsync(
            DesignConversionLocalArtifactDbRecord record,
            CancellationToken ct = default)
        {
            LastAppendDiagnostic =
                "append_observed " +
                $"record_id={record.RecordId} " +
                $"job_id={record.JobId} " +
                $"request_id={record.RequestId} " +
                $"dpo_candidates={record.DpoCandidates.Count.ToString(CultureInfo.InvariantCulture)} " +
                $"metadata_candidate_count={ReadMetadata(record.Metadata, "runtime_created_dpo_candidate_count")} " +
                $"selected_rejected={ReadMetadata(record.Metadata, DesignConversionImprovementLoopCoordinator.LocalArtifactDbDpoSelectedRejectedAttemptMarker)} " +
                $"no_eligible={ReadMetadata(record.Metadata, DesignConversionImprovementLoopCoordinator.LocalArtifactDbDpoNoEligibleRejectedAttemptMarker)} " +
                $"dpo_warnings={ReadMetadata(record.Metadata, "runtime_created_dpo_candidate_warnings")}";
            var appended = await inner.AppendRecordAsync(record, ct);
            if (appended.IsSuccess && record.DpoCandidates.Any(candidate =>
                    candidate.TrainingRecord.PayloadCustodySecret is not null))
            {
                _lastAppendedRecordWithPayload = record;
            }
            if (!appended.IsSuccess)
            {
                LastAppendDiagnostic +=
                    $" append_error_code={appended.ErrorCode ?? "<missing>"}" +
                    $" append_error={appended.ErrorMessage ?? "<missing>"}";
            }
            return appended;
        }

        public Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ListRecordsAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => inner.ListRecordsAsync(query, ct);

        public Task<DesignConversionPostEvaluationTrainingEligibility> EvaluatePostEvaluationTrainingEligibilityAsync(
            DesignConversionLocalArtifactDbRecord record,
            CancellationToken ct = default)
            => inner.EvaluatePostEvaluationTrainingEligibilityAsync(record, ct);

        public Task<IReadOnlyList<DesignConversionPostEvaluationTrainingEligibility>>
            EvaluatePostEvaluationTrainingEligibilityManyAsync(
                IReadOnlyList<DesignConversionLocalArtifactDbRecord?> records,
                CancellationToken ct = default)
            => inner.EvaluatePostEvaluationTrainingEligibilityManyAsync(records, ct);

        public Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryApprovedTrainingCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => inner.QueryApprovedTrainingCandidatesAsync(query, ct);

        public async Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            DateTimeOffset? exportedAtUtc = null,
            CancellationToken ct = default)
        {
            var approvalQuery = query is null
                ? new DesignConversionLocalArtifactDbQuery()
                : query with { ApprovedTrainingOnly = false };
            var records = await inner.ListRecordsAsync(approvalQuery, ct);
            if (!records.IsSuccess || records.Value is null)
                return records.MapError<DesignConversionApprovedDpoHandoffArtifact>();

            var sourceDisjointReadyRecords =
                await EnsureSourceDisjointCompanionRecordAsync(
                    records.Value,
                    query,
                    ct);
            if (!sourceDisjointReadyRecords.IsSuccess ||
                sourceDisjointReadyRecords.Value is null)
                return sourceDisjointReadyRecords
                    .MapError<DesignConversionApprovedDpoHandoffArtifact>();

            foreach (var record in sourceDisjointReadyRecords.Value)
            {
                foreach (var candidate in record.DpoCandidates)
                {
                    if (IsApprovedForTraining(candidate))
                        continue;

                    var approval = await inner.ApproveRuntimeCreatedDpoCandidateAsync(
                        record.RecordId,
                        candidate.CandidateId,
                        "reviewer:full-local-composition-fake",
                        "fake local composition approval for atomic gateway proof",
                        exportedAtUtc ?? DateTimeOffset.UnixEpoch.AddMinutes(10),
                        ct);
                    if (!approval.IsSuccess)
                    {
                        LastApprovalError =
                            $"{approval.ErrorCode}: {approval.ErrorMessage}";
                        return approval.MapError<DesignConversionApprovedDpoHandoffArtifact>();
                    }
                }
            }

            return await inner.ExportApprovedRuntimeDpoCandidatesAsync(
                query,
                exportedAtUtc,
                ct);
        }

        private async Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>>
            EnsureSourceDisjointCompanionRecordAsync(
                IReadOnlyList<DesignConversionLocalArtifactDbRecord> records,
                DesignConversionLocalArtifactDbQuery? query,
                CancellationToken ct)
        {
            if (_sourceDisjointCompanionAppended ||
                string.IsNullOrWhiteSpace(sourceDisjointCompanionSourceManifestRef) ||
                query is null ||
                string.IsNullOrWhiteSpace(query.JobId) ||
                string.IsNullOrWhiteSpace(query.RequestId))
            {
                return OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok(records);
            }

            var sourceRecord = _lastAppendedRecordWithPayload is not null &&
                               string.Equals(
                                   _lastAppendedRecordWithPayload.JobId,
                                   query.JobId,
                                   StringComparison.Ordinal) &&
                               string.Equals(
                                   _lastAppendedRecordWithPayload.RequestId,
                                   query.RequestId,
                                   StringComparison.Ordinal)
                ? _lastAppendedRecordWithPayload
                : records
                .Where(record => string.Equals(record.JobId, query.JobId, StringComparison.Ordinal) &&
                                 string.Equals(record.RequestId, query.RequestId, StringComparison.Ordinal) &&
                                 record.DpoCandidates.Count > 0)
                .OrderBy(record => record.RecordId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (sourceRecord is null)
                return OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok(records);

            var companion = CreateSourceDisjointCompanionRecord(
                sourceRecord,
                sourceDisjointCompanionSourceManifestRef!);
            var appended = await inner.AppendRecordAsync(companion, ct);
            if (!appended.IsSuccess)
                return appended.MapError<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>();

            var committedState = await CommitSourceDisjointCompanionStateAsync(
                appended.Value!,
                ct);
            if (!committedState.IsSuccess)
                return committedState
                    .MapError<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>();

            _sourceDisjointCompanionAppended = true;
            var refreshed = await inner.ListRecordsAsync(
                query with { ApprovedTrainingOnly = false },
                ct);
            return refreshed;
        }

        private async Task<OperationResult<DesignConversionPostEvaluationState>>
            CommitSourceDisjointCompanionStateAsync(
                DesignConversionLocalArtifactDbRecord companionRecord,
                CancellationToken ct)
        {
            if (!companionRecord.Metadata.TryGetValue(
                    JsonDesignConversionPostEvaluationStateStore
                        .StateRefMetadataKey,
                    out var stateRef))
            {
                return OperationResult<DesignConversionPostEvaluationState>.Fail(
                    "Source-disjoint companion fixture is missing post-evaluation state ref.",
                    "SOURCE_DISJOINT_COMPANION_STATE_REF_MISSING");
            }

            var store = new JsonDesignConversionPostEvaluationStateStore(
                inner.PostEvaluationStateRootDirectory);
            var provisional = await store.WriteAsync(
                new DesignConversionPostEvaluationState(
                    StateRef: stateRef,
                    ReservationRef: stateRef,
                    UserRef:
                        JsonDesignConversionPostEvaluationStateStore
                            .NoReservationUserRef,
                    PeriodStartUtc:
                        JsonDesignConversionPostEvaluationStateStore
                            .NoReservationPeriodStartUtc,
                    PeriodEndExclusiveUtc:
                        JsonDesignConversionPostEvaluationStateStore
                            .NoReservationPeriodEndExclusiveUtc,
                    ChargedCostUsd: 0m,
                    AccountingStatus:
                        JsonDesignConversionPostEvaluationStateStore
                            .AccountingSettled,
                    AccountingDesiredAction:
                        JsonDesignConversionPostEvaluationStateStore
                            .DesiredNone,
                    SettlementAttempted: false,
                    SettlementDefinitivelyNotApplied: false,
                    ReleaseAttempted: false,
                    PersistenceStatus:
                        JsonDesignConversionPostEvaluationStateStore
                            .PersistenceProvisional,
                    LedgerRecordId: string.Empty,
                    ArtifactRecordId: string.Empty,
                    CompletionBindingKind: string.Empty,
                    CompletionBindingRef: string.Empty,
                    LastErrorCode: string.Empty,
                    UpdatedAtUtc: FullLocalPreflightNow,
                    Version: 1),
                ct);
            if (!provisional.IsSuccess || provisional.Value is null)
                return provisional;

            return await store.WriteAsync(
                provisional.Value with
                {
                    PersistenceStatus =
                        JsonDesignConversionPostEvaluationStateStore
                            .PersistenceCommitted,
                    ArtifactRecordId = companionRecord.RecordId,
                    CompletionBindingKind =
                        JsonDesignConversionPostEvaluationStateStore
                            .CompletionBindingLocalArtifactDbRecord,
                    CompletionBindingRef = companionRecord.RecordId,
                    UpdatedAtUtc = FullLocalPreflightNow.AddSeconds(1),
                    Version = provisional.Value.Version
                },
                ct);
        }

        private static DesignConversionLocalArtifactDbRecord
            CreateSourceDisjointCompanionRecord(
                DesignConversionLocalArtifactDbRecord sourceRecord,
                string sourceManifestRef)
        {
            var companionRecordId =
                $"{sourceRecord.RecordId}:source-disjoint-companion:{Sha256Text(sourceManifestRef)[..12]}";
            var sourceRefs = new Dictionary<string, string>(
                sourceRecord.SanitizedSourceRefs,
                StringComparer.Ordinal)
            {
                ["design_tree_payload_ref"] = sourceManifestRef,
                ["source_manifest_ref"] = sourceManifestRef
            };
            var dpoCandidates = sourceRecord.DpoCandidates
                .Select((candidate, index) => CreateSourceDisjointCompanionCandidate(
                    candidate,
                    companionRecordId,
                    sourceManifestRef,
                    index + 1))
                .ToArray();
            var metadata = new Dictionary<string, string>(
                sourceRecord.Metadata,
                StringComparer.Ordinal)
            {
                ["source_disjoint_companion_fixture"] = bool.TrueString,
                ["source_manifest_ref"] = sourceManifestRef,
                [JsonDesignConversionPostEvaluationStateStore
                    .StateRefMetadataKey] =
                    JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                        $"source-disjoint-companion:{companionRecordId}"),
                [JsonDesignConversionPostEvaluationStateStore
                    .PersistenceStatusMetadataKey] =
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceProvisional,
                [JsonDesignConversionPostEvaluationStateStore
                    .TrainingEligibleMetadataKey] = bool.FalseString
            };

            return sourceRecord with
            {
                RecordId = companionRecordId,
                SanitizedSourceRefs = sourceRefs,
                DpoCandidates = dpoCandidates,
                Metadata = metadata
            };
        }

        private static DesignConversionLocalArtifactDbDpoCandidate
            CreateSourceDisjointCompanionCandidate(
                DesignConversionLocalArtifactDbDpoCandidate candidate,
                string companionRecordId,
                string sourceManifestRef,
                int index)
        {
            var companionSuffix =
                $"source-disjoint-companion-{index.ToString(CultureInfo.InvariantCulture)}";
            var trainingRecord = candidate.TrainingRecord;
            var provenanceMetadata = new Dictionary<string, string>(
                trainingRecord.Provenance.Metadata,
                StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = sourceManifestRef,
                ["source_disjoint_companion_fixture"] = bool.TrueString
            };
            var companionTrainingRecord = trainingRecord with
            {
                RecordId = $"{trainingRecord.RecordId}:{companionSuffix}",
                Provenance = trainingRecord.Provenance with
                {
                    SourceManifestRef = sourceManifestRef,
                    Metadata = provenanceMetadata
                }
            };
            if (companionTrainingRecord.PayloadCustodySecret is { } payload)
            {
                var governanceSha256 = DesignConversionDpoGenerator
                    .BuildPayloadCustodyGovernanceSha256(
                        companionTrainingRecord);
                var bindingSha256 = DesignConversionDpoGenerator
                    .BuildPayloadCustodyBinding(
                        companionTrainingRecord,
                        payload.PromptSha256,
                        payload.ChosenResponseSha256,
                        payload.RejectedResponseSha256,
                        governanceSha256);
                provenanceMetadata[
                    DesignConversionDpoGenerator
                        .PayloadCustodyGovernanceSha256MetadataKey] =
                    governanceSha256;
                provenanceMetadata[
                    DesignConversionDpoGenerator
                        .PayloadCustodyBindingSha256MetadataKey] =
                    bindingSha256;
                companionTrainingRecord = companionTrainingRecord with
                {
                    Provenance = companionTrainingRecord.Provenance with
                    {
                        Metadata = provenanceMetadata
                    },
                    PayloadCustodySecret = payload.Rebind(
                        governanceSha256,
                        bindingSha256)
                };
            }

            return candidate with
            {
                CandidateId = $"{candidate.CandidateId}:{companionSuffix}",
                RecordId = companionTrainingRecord.RecordId,
                TrainingRecord = companionTrainingRecord
            };
        }

        public async Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
            DesignConversionApprovedDpoHandoffArtifact handoff,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
        {
            var created = await inner.CreateTrainingIntakeAsync(
                handoff,
                requestedByRef,
                reason,
                requestedAtUtc,
                ct,
                admissionBoundaryMetadata);
            LastCreatedIntakeId = created.Value?.IntakeId;
            LastCreatedIntakeSourceManifestRefs =
                created.Value?.CandidateRefs
                    .Select(candidate => candidate.SourceManifestRef)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray() ??
                [];
            LastCreatedIntakeSourceIdentityHashes =
                created.Value?.CandidateRefs
                    .Select(candidate => candidate.SourceIdentityHash)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray() ??
                [];
            return created;
        }

        public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
            CancellationToken ct = default)
            => inner.ListTrainingIntakeRequestsAsync(ct);

        public async Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
            string intakeId,
            CancellationToken ct = default)
        {
            RunTrainingIntakePreflightCalls++;
            LastPreflightIntakeId = intakeId;
            var preflight = await inner.RunTrainingIntakePreflightAsync(intakeId, ct);
            LastPreflightResult = preflight.Value;
            return preflight;
        }

        public Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
            string intakeId,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => inner.RunTrainingIntakeAsync(intakeId, options, ct, admissionBoundaryMetadata);

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>>
            RunTrainingIntakeAndPromoteLocalAsync(
                string intakeId,
                string promotionOutputDirectory,
                DesignConversionTrainingPipelineOptions? options = null,
                CancellationToken ct = default)
        {
            RunTrainingIntakeAndPromoteLocalCalls++;
            LastAtomicIntakeId = intakeId;
            LastAtomicTrainingOptions = options;
            return Task.FromResult(
                OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>.Ok(
                    CreateFakeAtomicPromotionResult(intakeId, options, testRoot)));
        }

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
            string promotedLocalModelRef,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default)
            => inner.RollbackTrainingIntakeLocalPromotionAsync(
                promotedLocalModelRef,
                requestedByRef,
                reason,
                requestedAtUtc,
                ct);

        public Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
            string localArtifactRecordId,
            string candidateOrTrainingRecordId,
            string reviewerRef,
            string reason,
            DateTimeOffset? approvedAtUtc = null,
            CancellationToken ct = default)
            => inner.ApproveRuntimeCreatedDpoCandidateAsync(
                localArtifactRecordId,
                candidateOrTrainingRecordId,
                reviewerRef,
                reason,
                approvedAtUtc,
                ct);

        public Task<OperationResult<DesignConversionDpoDisplayResult>> QueryDisplayRowsAsync(
            DesignConversionDpoDisplayQuery? query = null,
            CancellationToken ct = default)
            => inner.QueryDisplayRowsAsync(query, ct);

        public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>> ListTrainingIntakeLocalPromotionRecordsAsync(
            CancellationToken ct = default)
            => inner.ListTrainingIntakeLocalPromotionRecordsAsync(ct);

        private static bool IsApprovedForTraining(
            DesignConversionLocalArtifactDbDpoCandidate candidate)
            => candidate.TrainingRecord.Provenance.Metadata.TryGetValue(
                   "approved_for_training",
                   out var approved) &&
               string.Equals(approved, bool.TrueString, StringComparison.OrdinalIgnoreCase);

    }


    [Fact]
    public async Task TryRunImprovementLoopAsync_InvalidInitialMeasurementFailsBeforeRegistryOrRuntime()
    {
        const string privateMarker = "private:invalid-adapter-measurement-secret";
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-invalid-adapter-measurement",
            Guid.NewGuid().ToString("N"));
        try
        {
            var browserAdapter = new RecordingBrowserAdapter(Path.Combine(root, "browser-artifacts"));
            var strategy = CreateScriptedCandidateStrategy("invalid-adapter-measurement", 1);
            var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
                    browserAdapter,
                    new AttemptScoreComparisonProvider(0.95),
                    new AttemptScoreVisualJudge(0.95)),
                strategy,
                artifactDb: null,
                utcNow: () => FullLocalPreflightNow);
            var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);
            var package = CreateLoopPackage(
                "invalid-adapter-measurement-loop",
                "invalid-adapter-measurement-request",
                privateMarker) with
            {
                InitialPackage = CreatePackage(
                    "invalid-adapter-measurement-request",
                    privateMarker) with
                {
                    MeasurementGateRequest = BuildRouteAdmissionMeasurement(
                        "invalid-adapter-measurement-loop") with
                    {
                        GateId = privateMarker
                    }
                }
            };

            var envelope = await adapter.TryRunImprovementLoopAsync(package);

            Assert.Equal("error", envelope.EnvelopeKind);
            Assert.Equal(400, envelope.StatusCode);
            Assert.False(envelope.RegistryInvokeSucceeded);
            Assert.False(envelope.RuntimeInvoked);
            Assert.Equal(
                DesignConversionMeasurementAdmission.InvalidErrorCode,
                envelope.ParsedErrorCode);
            Assert.Empty(strategy.Requests);
            Assert.Empty(browserAdapter.Requests);
            Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(root);
        }
    }

}
