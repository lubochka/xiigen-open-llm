// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerImprovementLoopTests
{
    private const string ExpectedTeacherModelId = "qwen3.5:9b";
    private const string RouteAdmissionCurrentToken = "current-improvement-loop-route-token-0123456789";
    private const string RouteAdmissionPreviousToken = "previous-improvement-loop-route-token-0123456789";
    private const string RouteAdmissionTenantId = "tenant-improvement-loop-alpha";
    private static readonly string ExpectedTeacherManifestDigest = new('c', 64);
    private static readonly string ExpectedTeacherModelLayerDigest = new('d', 64);
    private static readonly DateTimeOffset RouteAdmissionNow = DateTimeOffset.UnixEpoch.AddHours(25);

    [Fact]
    public void ImprovementLoopAttemptHistoryBoundary_AllowsOnlyScalarStatusCountAndHashReferences()
    {
        var predicate = typeof(DesignConversionModelServerImprovementLoopHandler)
            .GetMethod(
                "IsPublicBoundaryKey",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static);
        Assert.NotNull(predicate);

        bool IsPublic(string key)
            => Assert.IsType<bool>(predicate!.Invoke(null, [key]));

        Assert.True(IsPublic(
            DesignConversionImprovementLoopCoordinator
                .ImprovementLoopHistoryStatusMetadataKey));
        Assert.True(IsPublic(
            DesignConversionImprovementLoopCoordinator
                .ImprovementLoopHistoryAttemptCountMetadataKey));
        Assert.True(IsPublic(
            DesignConversionImprovementLoopCoordinator
                .ImprovementLoopHistorySha256MetadataKey));
        Assert.True(IsPublic(
            DesignConversionImprovementLoopCoordinator
                .ImprovementLoopHistoryOnlyMetadataKey));
        Assert.False(IsPublic(
            DesignConversionLocalArtifactDbRecord
                .ImprovementLoopAttemptHistoryMetadataKey));
    }

    [Fact]
    public void ImprovementLoopAttemptHistoryBoundary_RedactsPlainRefsAndDynamicMetadataWithoutSnapshotLeak()
    {
        const string privateMarker = "PRIVATE_W082_SECRET";
        const string candidateRef = "dcm-loop-candidate-0123456789abcdef";
        const string jobRef = "dcm-loop-job-0123456789abcdef";
        const string requestRef = "dcm-loop-request-0123456789abcdef";
        var attempt = new DesignConversionImprovementLoopAttemptTelemetry(
            AttemptNumber: 1,
            CandidateRef: candidateRef,
            JobRef: jobRef,
            RequestRef: requestRef,
            StopReason: DesignConversionVisualQaBoundaryPolicy.QualityReached,
            ShouldContinue: false,
            BestScore: 0.95,
            RequiredScore: 0.90,
            EstimatedCostUsd: 0.01m,
            BudgetUsd: 1m,
            PolicyAttemptCount: 1,
            MaxImprovementCandidates: 2,
            RenderResultRefs: [privateMarker],
            ComparisonArtifactRefs: [],
            VisualJudgeResultRefs: [],
            EvidenceRefs: [privateMarker],
            PublicMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["attempt_number"] = "1",
                ["loop_candidate_ref"] = candidateRef,
                ["job_ref"] = jobRef,
                ["request_ref"] = requestRef,
                ["stop_reason"] =
                    DesignConversionVisualQaBoundaryPolicy.QualityReached,
                ["should_continue"] = bool.FalseString,
                ["refs_or_hashes_only"] = bool.TrueString,
                ["raw_html_included"] = bool.FalseString,
                ["raw_css_included"] = bool.FalseString,
                ["raw_design_tree_included"] = bool.FalseString,
                ["screenshot_bytes_included"] = bool.FalseString,
                ["provider_name"] = privateMarker,
                ["raw_style_payload"] = privateMarker
            });
        var result = new DesignConversionImprovementLoopResult(
            LoopId: "w082-handler-redaction-loop",
            StopReason: DesignConversionVisualQaBoundaryPolicy.QualityReached,
            Completed: true,
            QualityReached: true,
            ProviderRequiredOrBlocked: false,
            RenderManifestIncomplete: false,
            BudgetCapReached: false,
            MaxAttemptsReached: false,
            Cancelled: false,
            DeadlineExceeded: false,
            CandidateStrategyFailed: false,
            AttemptCount: 1,
            FinalEvaluation: null,
            Attempts: [attempt],
            EvidenceRefs: [],
            PublicMetadata: new Dictionary<string, string>(),
            Warnings: []);
        var projection = typeof(DesignConversionModelServerImprovementLoopHandler)
            .GetMethod(
                "BuildResponseAttempts",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static);

        Assert.NotNull(projection);
        var projected = Assert.IsAssignableFrom<IReadOnlyList<
            DesignConversionModelServerImprovementLoopAttemptResponse>>(
            projection!.Invoke(null, [result, null]));
        var projectedAttempt = Assert.Single(projected);
        Assert.Equal(1, projectedAttempt.AttemptNumber);
        var json = JsonSerializer.Serialize(projected);
        Assert.DoesNotContain(privateMarker, json, StringComparison.Ordinal);
        Assert.DoesNotContain("raw_style_payload", json, StringComparison.Ordinal);
        Assert.DoesNotContain("redactionCodes", json, StringComparison.Ordinal);
        Assert.DoesNotContain(
            DesignConversionLocalArtifactDbRecord
                .ImprovementLoopAttemptHistoryMetadataKey,
            json,
            StringComparison.Ordinal);
        Assert.StartsWith(
            "sha256:",
            projectedAttempt.PublicMetadata["provider_name"],
            StringComparison.Ordinal);
        Assert.All(
            projectedAttempt.RenderResultRefs
                .Concat(projectedAttempt.EvidenceRefs),
            reference => Assert.StartsWith(
                "sha256:",
                reference,
                StringComparison.Ordinal));
    }

    [Fact]
    public void CreateModelServerRequiredBrowserQaMatrix_ReturnsExactlyRequiredSlotsAndPreservesCallerRequiredWidth()
    {
        var result = DesignConversionRenderResolutionFactory.CreateModelServerRequiredBrowserQaMatrix(
        [
            new DesignRenderArtifact(
                ArtifactId: "n6-4-factory-tablet-custom",
                Kind: "design-render",
                Width: 768,
                Height: 1111,
                Uri: "artifact://n6-4/factory-tablet-custom.png",
                Sha256: "sha256:n6-4-factory-tablet-custom",
                Metadata: new Dictionary<string, string>
                {
                    [DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey] = "2",
                    ["viewport_id"] = "tablet-custom"
                }),
            new DesignRenderArtifact(
                ArtifactId: "n6-4-factory-desktop",
                Kind: "design-render",
                Width: 1440,
                Height: 900,
                Uri: "artifact://n6-4/factory-desktop.png",
                Sha256: "sha256:n6-4-factory-desktop",
                Metadata: new Dictionary<string, string>())
        ]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var resolutions = result.Value!;
        Assert.Equal(
            [
                DesignConversionRenderResolutionFactory.RequiredBrowserQaMobileResolutionId,
                "source-768x1111@2~tablet-custom",
                DesignConversionRenderResolutionFactory.RequiredBrowserQaLaptopResolutionId,
                DesignConversionRenderResolutionFactory.RequiredBrowserQaDesktopResolutionId
            ],
            resolutions.Select(resolution => resolution.ResolutionId).ToArray());
        Assert.Equal([360, 768, 1280, 1920], resolutions.Select(resolution => resolution.Width).ToArray());
        Assert.Equal([800, 1111, 720, 1080], resolutions.Select(resolution => resolution.Height).ToArray());
        Assert.Equal([1.0, 2.0, 1.0, 1.0], resolutions.Select(resolution => resolution.DeviceScaleFactor).ToArray());
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_RegisteredRoute_ReturnsRefsOnlySuccessEnvelope()
    {
        const string privateMarker = "PRIVATE_PASS129_SUCCESS_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.72);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.96, 0.01m)
                : JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("pass129-success-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-success-loop",
            InitialPackage: CreatePackage("pass129-success-request", privateMarker),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        Assert.Equal("pass129-success-request", envelope.ParsedRequestId);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("deadlineExceeded").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["socket_opened"]);
        Assert.Equal(2, renderProvider.Requests.Select(request => request.Metadata["job_id"]).Distinct(StringComparer.Ordinal).Count());
        Assert.Single(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_RepositoryOwnedTrainingOptionsAttachServerOwnedDpoContextBeforeCandidateWork()
    {
        const string privateMarker = "PRIVATE_REPOSITORY_OWNED_DPO_CONTEXT_SECRET";
        var repositoryRoot = CreateTemporaryRepositoryRoot(
            "handler-repository-owned-dpo-context");
        try
        {
            var renderProvider = new RecordingRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(request =>
                request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                    ? 0.96
                    : 0.72);
            var visualJudge = new ScriptedVisualJudge(request =>
                request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                    ? JudgeScript.FromScore(0.96, 0.01m)
                    : JudgeScript.FromScore(0.72, 0.01m));
            var strategy = new ScriptedCandidateStrategy(
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreateCandidate(
                        "repository-owned-dpo-context-candidate",
                        privateMarker)));
            var service = new DesignConversionInternalService(
                renderProvider,
                comparisonProvider,
                visualJudge);
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                service,
                strategy);
            var handler = new DesignConversionModelServerImprovementLoopHandler(
                coordinator,
                repositoryOwnedTrainingOptions:
                    CreateRepositoryOwnedTrainingOptions(repositoryRoot),
                repositoryOwnedDpoTeacherConfiguration:
                    CreateRepositoryOwnedDpoTeacherConfiguration());
            var package = CreatePackage(
                "repository-owned-dpo-context-request",
                privateMarker);

            var response = await handler.HandleAsync(
                ImprovementLoopHttpRequest(
                    SerializeLoopRequest(
                        "repository-owned-dpo-context-loop",
                        package,
                        maxAttempts: 3,
                        privateMarker),
                    headers: null),
                CancellationToken.None);

            Assert.True(response.StatusCode == 200, response.BodyJson);
            var candidateRequest = Assert.Single(strategy.Requests);
            Assert.NotNull(candidateRequest.RepositoryOwnedDpoTransactionContext);
            var context = candidateRequest.RepositoryOwnedDpoTransactionContext!;
            Assert.True(context.Enabled);
            Assert.True(context.Validate().IsSuccess);
            Assert.Equal(
                Path.GetFullPath(repositoryRoot).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar),
                context.Address!.RepositoryRoot);
            Assert.Equal(
                DesignConversionRepositoryOwnedDpoTransactionContextFactory
                    .FixedDomainSlug,
                context.Address.DomainSlug);
            Assert.Equal(
                DesignConversionRepositoryOwnedDpoTransactionContextFactory
                    .FixedDesignPatternSlug,
                context.Address.DesignPatternSlug);
            Assert.StartsWith(
                "training-data/dpo",
                context.Address.ManifestRelativePath,
                StringComparison.Ordinal);
            Assert.Equal(
                OpenWeightTeacherFamily.Qwen.ToString(),
                context.RequestIdentity!.OpenTeacher.Family);
            Assert.DoesNotContain(privateMarker, response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.Html!, response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.Css!, response.BodyJson, StringComparison.Ordinal);
        }
        finally
        {
            DeleteTemporaryDirectory(repositoryRoot);
        }
    }

    [Fact]
    public async Task HandleAsync_RepositoryOwnedDpoCleanMissSealsDatasetAndSecondIdenticalRequestReusesItWithoutTeacher()
    {
        const string privateMarker =
            "PRIVATE_REPOSITORY_OWNED_DPO_CLEAN_REUSE_SECRET";
        const string datasetVersion = "modelserver-clean-reuse-v1";
        var repositoryRoot = CreateTemporaryRepositoryRoot(
            "handler-repository-owned-dpo-clean-reuse");
        try
        {
            var fixture = CreateRepositoryOwnedDpoProviderAllowedPair(
                "modelserver-clean-reuse");
            var evaluator = new RequestBindingSequenceEvaluator(
                fixture.Rejected,
                fixture.Chosen);
            // Внутренний strategy остаётся под production teacher-authoring
            // wrapper: он может дать candidate shape, но DPO-authority в этом
            // тесте доказывают teacher count и sealed repository dataset.
            var deterministicFallback = new ScriptedCandidateStrategy(
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    fixture.Chosen.RuntimeFlowResult.Candidate));
            using var teacherFixture = CreateAuthoringTeacherFixture(
                "repository-owned-dpo-clean-reuse");
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                evaluator,
                deterministicFallback,
                teacherFixture.ArtifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);
            var teacherConfiguration =
                CreateRepositoryOwnedDpoTeacherConfiguration();
            var handler = new DesignConversionModelServerImprovementLoopHandler(
                coordinator,
                repositoryOwnedTrainingOptions:
                    CreateRepositoryOwnedTrainingOptions(
                        repositoryRoot,
                        datasetVersion),
                repositoryOwnedDpoTeacherConfiguration:
                    teacherConfiguration);
            var registry = new DesignConversionModelServerEndpointRegistry();
            var registered = registry
                .RegisterDesignConversionImprovementLoopForLiveHostTesting(
                    handler);
            Assert.True(registered.IsSuccess, registered.ErrorMessage);
            var adapter = new DesignConversionModelServerLocalCallerAdapter(
                registry);
            var basePackage = CreatePackage(
                "repository-owned-dpo-clean-reuse-request",
                privateMarker);
            var initialPackage = basePackage with
            {
                Budget = basePackage.Budget! with
                {
                    MaxImprovementCandidates = 2,
                    MaxImprovementIterations = 2,
                    MaxTeacherCalls = 1
                }
            };
            var requestPackage =
                new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                    LoopId: "repository-owned-dpo-clean-reuse-loop",
                    InitialPackage: initialPackage,
                    MaxAttempts: 2,
                    Metadata: new Dictionary<string, string>
                    {
                        ["repository_owned_dpo_clean_reuse_probe"] =
                            bool.TrueString
                    });
            var expectedContext =
                CreateExpectedServerOwnedRepositoryDpoContext(
                    repositoryRoot,
                    datasetVersion,
                    initialPackage,
                    teacherConfiguration);
            var expectedAddress = expectedContext.Address!;
            var expectedIdentity = expectedContext.RequestIdentity!;
            Assert.False(Directory.Exists(expectedAddress.DatasetRoot));

            // Первый вызов важен как доказательство clean miss: пустой repo
            // получает ровно один разрешённый fake Qwen teacher call, а не
            // заранее подложенный dataset.
            var firstEnvelope = await adapter.TryRunImprovementLoopAsync(
                requestPackage);

            AssertStableImprovementLoopEnvelope(
                firstEnvelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal("success", firstEnvelope.EnvelopeKind);
            AssertNoPrivateMarkerLeak(
                firstEnvelope.ResponseBodyJson,
                privateMarker,
                initialPackage);
            Assert.Equal(1, teacherFixture.Teacher.InvocationCount);
            using var firstDocument =
                JsonDocument.Parse(firstEnvelope.ResponseBodyJson);
            var firstRoot = firstDocument.RootElement;
            AssertRepositoryOwnedDpoEvidence(
                firstRoot,
                RepositoryOwnedDpoDatasetResolutionSource
                    .NewlyPersistedRepositoryDataset,
                expectedPairCount: 2,
                firstEnvelope.ResponseBodyJson);
            AssertEvidenceContains(
                firstRoot,
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetCleanMissGenerationAuthorizedMarker,
                firstEnvelope.ResponseBodyJson);
            var reopened = AssertSuccess(
                RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                    expectedAddress,
                    expectedIdentity));
            Assert.Equal(2, reopened.Pairs.Count);
            Assert.Equal(2, reopened.Manifest.PairCount);
            Assert.Equal(
                expectedIdentity.RequestIdentitySha256,
                reopened.Manifest.RequestIdentity.RequestIdentitySha256);
            Assert.Equal(
                reopened.Manifest.ManifestSha256,
                AssertBoundaryMetadataOrEvidenceValue(
                    firstRoot,
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetManifestSha256MetadataKey,
                    reopened.Manifest.ManifestSha256,
                    firstEnvelope.ResponseBodyJson));
            AssertRepositoryOwnedDpoPairsCarryCanonicalTeacherProvenance(
                reopened);
            await AssertNoLocalDpoCandidateWasWrittenAsync(
                teacherFixture.ArtifactDb);

            // Сравнение SHA-256 всех файлов нужно не для красоты: reuse считается
            // настоящим только если второй проход не переписал manifest, пары или
            // lock/temp-файлы внутри canonical training-data/dpo dataset root.
            var firstFiles =
                CaptureRegularFileSha256ByRepositoryRelativePath(
                    repositoryRoot,
                    expectedAddress.DatasetRoot);
            Assert.Contains(
                expectedAddress.ManifestRelativePath.Replace('\\', '/'),
                firstFiles.Keys);
            var firstManifestIdentity = (
                expectedAddress.ManifestRelativePath,
                expectedIdentity.RequestIdentitySha256,
                reopened.Manifest.ManifestSha256);

            // Второй вызов намеренно byte-identical: production identity здесь
            // зависит от server config + HTML/CSS reference, а не от нового loop
            // id. Тот же request обязан увидеть ExistingSealedRepositoryDataset
            // и не иметь повода снова спрашивать teacher.
            var secondEnvelope = await adapter.TryRunImprovementLoopAsync(
                requestPackage);

            AssertStableImprovementLoopEnvelope(
                secondEnvelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal("success", secondEnvelope.EnvelopeKind);
            AssertNoPrivateMarkerLeak(
                secondEnvelope.ResponseBodyJson,
                privateMarker,
                initialPackage);
            Assert.Equal(1, teacherFixture.Teacher.InvocationCount);
            using var secondDocument =
                JsonDocument.Parse(secondEnvelope.ResponseBodyJson);
            var secondRoot = secondDocument.RootElement;
            Assert.Equal(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetExistingReusedStop,
                secondRoot.GetProperty("stopReason").GetString());
            AssertRepositoryOwnedDpoEvidence(
                secondRoot,
                RepositoryOwnedDpoDatasetResolutionSource
                    .ExistingSealedRepositoryDataset,
                expectedPairCount: 2,
                secondEnvelope.ResponseBodyJson);
            AssertEvidenceContains(
                secondRoot,
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetExistingReusedMarker,
                secondEnvelope.ResponseBodyJson);
            var reopenedAfterReuse = AssertSuccess(
                RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                    expectedAddress,
                    expectedIdentity));
            var secondManifestIdentity = (
                expectedAddress.ManifestRelativePath,
                reopenedAfterReuse.Manifest.RequestIdentity
                    .RequestIdentitySha256,
                reopenedAfterReuse.Manifest.ManifestSha256);
            Assert.Equal(firstManifestIdentity, secondManifestIdentity);
            AssertRepositoryOwnedDpoPairsCarryCanonicalTeacherProvenance(
                reopenedAfterReuse);
            var secondFiles =
                CaptureRegularFileSha256ByRepositoryRelativePath(
                    repositoryRoot,
                    expectedAddress.DatasetRoot);
            AssertRegularFileSnapshotsUnchanged(firstFiles, secondFiles);
            await AssertNoLocalDpoCandidateWasWrittenAsync(
                teacherFixture.ArtifactDb);

            // Этот тест доказывает repository-owned DPO persistence/reuse, а не
            // качество следующей bounded candidate-фазы. Если первый ответ когда-
            // либо вернёт candidateStrategyFailed=true после уже доказанных
            // NewlyPersistedRepositoryDataset, успешного OpenSealed, одного
            // fake teacher call, ExistingSealedRepositoryDataset на втором
            // байт-идентичном вызове и неизменных SHA файлов, такой флаг
            // описывает более поздний candidate-cycle, а не DPO failure.
            AssertFirstCandidateStatusDoesNotHideDpoFailure(
                firstRoot,
                firstEnvelope.ResponseBodyJson);
        }
        finally
        {
            DeleteTemporaryDirectory(repositoryRoot);
        }
    }

    [Fact]
    public async Task HandleAsync_RepositoryOwnedTrainingWithoutServerTeacherFailsBeforeRuntime()
    {
        const string privateMarker = "PRIVATE_REPOSITORY_OWNED_DPO_NO_TEACHER_SECRET";
        var repositoryRoot = CreateTemporaryRepositoryRoot(
            "handler-repository-owned-dpo-no-teacher");
        try
        {
            var evaluator = new FailingEvaluationService("SHOULD_NOT_REACH_RUNTIME");
            var strategy = new ScriptedCandidateStrategy();
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                evaluator,
                strategy);
            var handler = new DesignConversionModelServerImprovementLoopHandler(
                coordinator,
                repositoryOwnedTrainingOptions:
                    CreateRepositoryOwnedTrainingOptions(repositoryRoot),
                repositoryOwnedDpoTeacherConfiguration: null);
            var package = CreatePackage(
                "repository-owned-dpo-no-teacher-request",
                privateMarker);

            var response = await handler.HandleAsync(
                ImprovementLoopHttpRequest(
                    SerializeLoopRequest(
                        "repository-owned-dpo-no-teacher-loop",
                        package,
                        maxAttempts: 2,
                        privateMarker),
                    headers: null),
                CancellationToken.None);

            Assert.Equal(503, response.StatusCode);
            AssertError(
                response,
                DesignConversionModelServerImprovementLoopHandler
                    .RepositoryOwnedDpoContextRequiresServerConfigurationErrorCode);
            Assert.Empty(evaluator.Requests);
            Assert.Empty(strategy.Requests);
            Assert.DoesNotContain(privateMarker, response.BodyJson, StringComparison.Ordinal);
        }
        finally
        {
            DeleteTemporaryDirectory(repositoryRoot);
        }
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_MaxImprovementIterationsOverridesLegacyMaxAttempts()
    {
        const string privateMarker = "PRIVATE_TYPED_ITERATIONS_DIRECT_LOOP_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.72);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.96, 0.01m)
                : JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("typed-iterations-direct-loop-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var basePackage = CreatePackage("typed-iterations-direct-loop-request", privateMarker);
        var initialPackage = basePackage with
        {
            Budget = basePackage.Budget! with
            {
                MaxImprovementCandidates = 5,
                MaxImprovementIterations = 1,
                BudgetUsd = 1.00m
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "typed-iterations-direct-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 5,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Null(envelope.ParsedErrorCode);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        var boundaryMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal("2", boundaryMetadata.GetProperty("configured_max_attempts").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.TypedMaxImprovementIterationsSource,
            boundaryMetadata.GetProperty("max_attempts_source").GetString());
        Assert.Equal("1", boundaryMetadata.GetProperty("typed_max_improvement_iterations").GetString());
        Assert.Equal("5", boundaryMetadata.GetProperty("legacy_max_attempts").GetString());

        Assert.Equal(4, renderProvider.Requests.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Equal(2, renderProvider.Requests.Select(request => request.Metadata["job_id"]).Distinct(StringComparer.Ordinal).Count());
        Assert.Single(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_MaxTeacherCallsSurvivesHandlerMappingAndCapsTeacherCalls()
    {
        const string privateMarker = "PRIVATE_MAX_TEACHER_CALLS_ROUTE_SECRET";
        var renderProvider = new DeterministicDesignConversionRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:3", StringComparison.Ordinal) ? 0.96 :
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.76 : 0.70);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:3", StringComparison.Ordinal)
                ? JudgeScript.FromExternalScore(0.96, 0.01m)
                : request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                    ? JudgeScript.FromExternalScore(0.76, 0.01m)
                    : JudgeScript.FromExternalScore(0.70, 0.01m));
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate("max-teacher-calls-route-candidate-1", privateMarker)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate("max-teacher-calls-route-candidate-2", privateMarker)));
        using var teacherFixture = CreateTeacherFixture("max-teacher-calls-route");
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            teacherFixture.ArtifactDb);
        var initialPackage = CreatePackage("max-teacher-calls-route-request", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "max-teacher-calls-route-local-loop",
                MaxImprovementCandidates: 4,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90",
                    ["budget_private_marker"] = privateMarker
                },
                RenderQaPassThreshold: 0.90,
                MaxImprovementIterations: 3,
                MaxTeacherCalls: 1)
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "max-teacher-calls-route-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 4,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.Null(envelope.ParsedErrorCode);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());

        var boundaryMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey).GetString());
        Assert.Equal("1", boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey).GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsContractBoundary,
            boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey).GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey).GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey).GetString());
        Assert.Equal("1", boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey).GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey).GetString());

        var attempts = root.GetProperty("attempts").EnumerateArray().ToArray();
        Assert.Equal(2, attempts.Length);
        AssertEachAttemptRenderedTwoViewports(attempts);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, attempts[1].GetProperty("publicMetadata").GetProperty("stop_reason").GetString());
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Equal(2, strategy.Requests.Count);
        Assert.Equal(1, strategy.Requests[0].CompletedAttemptNumber);
        Assert.Equal(2, strategy.Requests[1].CompletedAttemptNumber);
        Assert.Equal(1, teacherFixture.Teacher.InvocationCount);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CandidateUnchangedWithMaxTeacherCallsReachedStopsOnTeacherCap()
    {
        const string privateMarker = "PRIVATE_MAX_TEACHER_CALLS_UNCHANGED_SECRET";
        var renderProvider = new DeterministicDesignConversionRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.76 : 0.70);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromExternalScore(0.76, 0.01m)
                : JudgeScript.FromExternalScore(0.70, 0.01m));
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate("max-teacher-calls-unchanged-first", privateMarker)),
            OperationResult<DesignConversionImprovementCandidate>.Fail(
                "Scripted runtime candidate strategy returned an unchanged candidate.",
                DesignConversionRuntimeImprovementCandidateStrategy.CandidateUnchangedErrorCode));
        using var teacherFixture = CreateTeacherFixture("max-teacher-calls-unchanged");
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            teacherFixture.ArtifactDb);
        var initialPackage = CreatePackage("max-teacher-calls-unchanged-request", privateMarker) with
        {
            Budget = CreatePackage("max-teacher-calls-unchanged-budget-source", privateMarker).Budget! with
            {
                MaxImprovementCandidates = 4,
                MaxImprovementIterations = 3,
                MaxTeacherCalls = 1
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "max-teacher-calls-unchanged-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 4);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        AssertEachAttemptRenderedTwoViewports(root);
        var boundaryMetadata = root.GetProperty("boundaryMetadata");
        AssertMaxTeacherCallsCapReached(boundaryMetadata);
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("response_contains_raw_private_data").GetString());
        AssertSingleObservedTeacherCall(root);
        Assert.Contains(
            $"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_NO_PROGRESS:{DesignConversionRuntimeImprovementCandidateStrategy.CandidateUnchangedErrorCode}",
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));
        Assert.Contains(
            "IMPROVEMENT_LOOP_MAX_TEACHER_CALLS_REACHED",
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));
        Assert.DoesNotContain(
            DesignConversionImprovementLoopCoordinator.TeacherAuthoringErrorWarningPrefix +
            DesignConversionDpoTeacherAuthoringCandidateStrategy.FallbackRequiredErrorCode,
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));
        Assert.Equal(2, strategy.Requests.Count);
        Assert.Equal(1, strategy.Requests[0].CompletedAttemptNumber);
        Assert.Equal(2, strategy.Requests[1].CompletedAttemptNumber);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Equal(1, teacherFixture.Teacher.InvocationCount);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_GenericStrategyFailureAtTeacherCapRemainsCandidateStrategyFailed()
    {
        const string privateMarker = "PRIVATE_MAX_TEACHER_CALLS_GENERIC_FAILURE_SECRET";
        var renderProvider = new DeterministicDesignConversionRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.76 : 0.70);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromExternalScore(0.76, 0.01m)
                : JudgeScript.FromExternalScore(0.70, 0.01m));
        const string genericErrorCode = "SCRIPTED_GENERIC_CANDIDATE_STRATEGY_FAILED";
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate("max-teacher-calls-generic-failure-first", privateMarker)),
            OperationResult<DesignConversionImprovementCandidate>.Fail(
                "Scripted generic candidate strategy failed.",
                genericErrorCode));
        using var teacherFixture = CreateTeacherFixture("max-teacher-calls-generic-failure");
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            teacherFixture.ArtifactDb);
        var initialPackage = CreatePackage("max-teacher-calls-generic-failure-request", privateMarker) with
        {
            Budget = CreatePackage("max-teacher-calls-generic-failure-budget-source", privateMarker).Budget! with
            {
                MaxImprovementCandidates = 4,
                MaxImprovementIterations = 3,
                MaxTeacherCalls = 1
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "max-teacher-calls-generic-failure-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 4);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.CandidateStrategyFailed,
            root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("candidateStrategyFailed").GetBoolean());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        AssertMaxTeacherCallsCapReached(root.GetProperty("boundaryMetadata"));
        AssertSingleObservedTeacherCall(root);
        Assert.Contains(
            $"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:{genericErrorCode}",
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));
        Assert.DoesNotContain(
            "IMPROVEMENT_LOOP_MAX_TEACHER_CALLS_REACHED",
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));
        Assert.DoesNotContain(
            DesignConversionImprovementLoopCoordinator.TeacherAuthoringErrorWarningPrefix +
            DesignConversionDpoTeacherAuthoringCandidateStrategy.FallbackRequiredErrorCode,
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));
        Assert.Equal(2, strategy.Requests.Count);
        Assert.Equal(1, strategy.Requests[0].CompletedAttemptNumber);
        Assert.Equal(2, strategy.Requests[1].CompletedAttemptNumber);
        Assert.Equal(1, teacherFixture.Teacher.InvocationCount);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CandidateValidationNoProgressWithMaxTeacherCallsReachedStopsOnTeacherCap()
    {
        const string privateMarker = "PRIVATE_MAX_TEACHER_CALLS_VALIDATION_SECRET";
        var renderProvider = new DeterministicDesignConversionRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.76 : 0.70);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromExternalScore(0.76, 0.01m)
                : JudgeScript.FromExternalScore(0.70, 0.01m));
        var firstCandidate = CreateCandidate("max-teacher-calls-validation-first", privateMarker);
        var noOpCandidate = firstCandidate with
        {
            CandidateId = "max-teacher-calls-validation-no-op",
            RequestId = "max-teacher-calls-validation-no-op-request",
            ChangeReasons = ["scripted no-op after teacher cap"],
            EvidenceRefs = ["candidate:max-teacher-calls-validation-no-op"]
        };
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(firstCandidate),
            OperationResult<DesignConversionImprovementCandidate>.Ok(noOpCandidate));
        using var teacherFixture = CreateTeacherFixture("max-teacher-calls-validation");
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            teacherFixture.ArtifactDb);
        var initialPackage = CreatePackage("max-teacher-calls-validation-request", privateMarker) with
        {
            Budget = CreatePackage("max-teacher-calls-validation-budget-source", privateMarker).Budget! with
            {
                MaxImprovementCandidates = 4,
                MaxImprovementIterations = 3,
                MaxTeacherCalls = 1
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "max-teacher-calls-validation-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 4);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        AssertEachAttemptRenderedTwoViewports(root);
        AssertMaxTeacherCallsCapReached(root.GetProperty("boundaryMetadata"));
        AssertSingleObservedTeacherCall(root);
        Assert.Contains(
            "IMPROVEMENT_LOOP_CANDIDATE_VALIDATION_NO_PROGRESS:IMPROVEMENT_CANDIDATE_NO_OP",
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));
        Assert.Contains(
            "IMPROVEMENT_LOOP_MAX_TEACHER_CALLS_REACHED",
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));
        Assert.Equal(2, strategy.Requests.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Equal(1, teacherFixture.Teacher.InvocationCount);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_MaxTeacherCallsReachedDoesNotOverrideQualityReached()
    {
        const string privateMarker = "PRIVATE_MAX_TEACHER_CALLS_QUALITY_SECRET";
        var renderProvider = new DeterministicDesignConversionRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.70);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromExternalScore(0.96, 0.01m)
                : JudgeScript.FromExternalScore(0.70, 0.01m));
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate("max-teacher-calls-quality-candidate", privateMarker)));
        using var teacherFixture = CreateTeacherFixture("max-teacher-calls-quality");
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            teacherFixture.ArtifactDb);
        var initialPackage = CreatePackage("max-teacher-calls-quality-request", privateMarker) with
        {
            Budget = CreatePackage("max-teacher-calls-quality-budget-source", privateMarker).Budget! with
            {
                MaxImprovementCandidates = 4,
                MaxImprovementIterations = 3,
                MaxTeacherCalls = 1
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "max-teacher-calls-quality-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 4);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.True(root.GetProperty("qualityReached").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        AssertEachAttemptRenderedTwoViewports(root);
        AssertMaxTeacherCallsCapReached(root.GetProperty("boundaryMetadata"));
        AssertSingleObservedTeacherCall(root);
        Assert.Single(strategy.Requests);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Equal(1, teacherFixture.Teacher.InvocationCount);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_MultiViewportEvaluationConsumesOneImprovementAttempt()
    {
        const string privateMarker = "PRIVATE_N8_2_ATTEMPT_COUNT_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.72);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromExternalScore(0.96, 0.01m)
                : JudgeScript.FromExternalScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n8-2-second-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("n8-2-attempt-count-request", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "n8-2-local-loop",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90"
                })
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-2-attempt-count-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());

        var attempts = root.GetProperty("attempts").EnumerateArray().ToArray();
        Assert.Equal(2, attempts.Length);
        var firstAttemptMetadata = attempts[0].GetProperty("publicMetadata");
        var secondAttemptMetadata = attempts[1].GetProperty("publicMetadata");
        Assert.Equal("1", firstAttemptMetadata.GetProperty("attempt_number").GetString());
        Assert.Equal("1", firstAttemptMetadata.GetProperty("policy_attempt_count").GetString());
        Assert.Equal("2", firstAttemptMetadata.GetProperty("render_result_count").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, firstAttemptMetadata.GetProperty("stop_reason").GetString());
        Assert.Equal("2", secondAttemptMetadata.GetProperty("attempt_number").GetString());
        Assert.Equal("2", secondAttemptMetadata.GetProperty("policy_attempt_count").GetString());
        Assert.Equal("2", secondAttemptMetadata.GetProperty("render_result_count").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, secondAttemptMetadata.GetProperty("stop_reason").GetString());
        Assert.Equal("0.02", firstAttemptMetadata.GetProperty("estimated_cost_usd").GetString());
        Assert.Equal("0.02", secondAttemptMetadata.GetProperty("estimated_cost_usd").GetString());
        Assert.Equal("0.02", firstAttemptMetadata.GetProperty("loop_spent_budget_usd").GetString());
        Assert.Equal("0.04", secondAttemptMetadata.GetProperty("loop_spent_budget_usd").GetString());
        Assert.Equal("0.04", root.GetProperty("boundaryMetadata").GetProperty("spent_budget_usd").GetString());
        Assert.Equal("0.96", root.GetProperty("boundaryMetadata").GetProperty("final_remaining_budget_usd").GetString());

        Assert.Equal(4, renderProvider.Requests.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Single(strategy.Requests);
        Assert.Equal(1, strategy.Requests[0].CompletedAttemptNumber);
        Assert.Equal(0.02m, Assert.Single(strategy.Requests[0].AttemptHistory).EstimatedCostUsd);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_BudgetQualityThresholdSurvivesHandlerMappingAndControlsAcceptance()
    {
        const string privateMarker = "PRIVATE_N8_9_QUALITY_THRESHOLD_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.94 : 0.92);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.94, 0.01m)
                : JudgeScript.FromScore(0.92, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n8-9-threshold-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("n8-9-threshold-request", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "n8-9-server-custom-threshold",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.99",
                    ["quality_threshold_source"] = "caller_forged_budget_source",
                    ["budget_private_marker"] = privateMarker,
                    ["budget_raw_payload"] = $"<div data-secret=\"{privateMarker}\"></div>"
                },
                RenderQaPassThreshold: 0.93),
            Metadata = new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["quality_threshold_source"] = "caller_forged_initial_source",
                ["caller_private_marker"] = privateMarker
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-9-threshold-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["quality_threshold_source"] = "caller_forged_package_source",
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, initialPackage);
        Assert.DoesNotContain("\"quality_threshold\":\"0.10\"", envelope.ResponseBodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        Assert.Equal("0.93", root.GetProperty("boundaryMetadata").GetProperty("final_required_score").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.TypedRenderQaPassThresholdSource,
            root.GetProperty("boundaryMetadata").GetProperty("quality_threshold_source").GetString());

        var attempts = root.GetProperty("attempts").EnumerateArray().ToArray();
        Assert.Equal(2, attempts.Length);
        var firstAttemptMetadata = attempts[0].GetProperty("publicMetadata");
        var secondAttemptMetadata = attempts[1].GetProperty("publicMetadata");
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, firstAttemptMetadata.GetProperty("stop_reason").GetString());
        Assert.Equal("0.93", firstAttemptMetadata.GetProperty("required_score").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.TypedRenderQaPassThresholdSource,
            firstAttemptMetadata.GetProperty("quality_threshold_source").GetString());
        Assert.Equal("0.92", firstAttemptMetadata.GetProperty("measured_quality_score").GetString());
        Assert.Equal(bool.FalseString, firstAttemptMetadata.GetProperty("visual_decision_quality_threshold_met").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, secondAttemptMetadata.GetProperty("stop_reason").GetString());
        Assert.Equal("0.93", secondAttemptMetadata.GetProperty("required_score").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.TypedRenderQaPassThresholdSource,
            secondAttemptMetadata.GetProperty("quality_threshold_source").GetString());
        Assert.Equal("0.94", secondAttemptMetadata.GetProperty("measured_quality_score").GetString());
        Assert.Equal(bool.TrueString, secondAttemptMetadata.GetProperty("visual_decision_quality_threshold_met").GetString());

        Assert.All(visualJudge.Requests, request =>
        {
            Assert.Equal("0.93", request.Metadata["quality_threshold"]);
            Assert.DoesNotContain(privateMarker, string.Join("|", request.Metadata.Values), StringComparison.Ordinal);
        });
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Single(strategy.Requests);
    }

    public static TheoryData<double, string> InvalidTypedRenderQaPassThresholdCases()
        => new()
        {
            { double.NaN, "internal" },
            { double.PositiveInfinity, "internal" },
            { 0.0, "internal" },
            { -0.10, "internal" },
            { 1.01, "internal" },
            { 0.74, "free" },
            { 0.84, "paid" },
            { 0.89, "pro" }
        };

    [Theory]
    [MemberData(nameof(InvalidTypedRenderQaPassThresholdCases))]
    public async Task TryRunImprovementLoopAsync_InvalidTypedRenderQaPassThresholdFailsLocalValidationBeforeRegistry(
        double renderQaPassThreshold,
        string tier)
    {
        const string privateMarker = "PRIVATE_TYPED_THRESHOLD_INVALID_LOOP_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage($"n8-9-invalid-typed-threshold-{tier}", privateMarker) with
        {
            Budget = CreatePackage("n8-9-invalid-typed-threshold-base", privateMarker).Budget! with
            {
                Tier = tier,
                RenderQaPassThreshold = renderQaPassThreshold
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: $"n8-9-invalid-typed-threshold-loop-{tier}",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RenderQaPassThresholdInvalidErrorCode, envelope.ParsedErrorCode);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_TypedRenderQaPassThresholdSurvivesEvaluationFailureTelemetry()
    {
        const string privateMarker = "PRIVATE_TYPED_THRESHOLD_FAILURE_TELEMETRY_SECRET";
        var failingEvaluator = new FailingEvaluationService("SCRIPTED_EVALUATION_FAILURE_FOR_TYPED_THRESHOLD");
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(failingEvaluator, strategy);
        var initialPackage = CreatePackage("n8-9-typed-threshold-failure-telemetry", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "pro",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.99",
                    ["quality_threshold_source"] = "caller_forged_failure_source",
                    ["budget_private_marker"] = privateMarker
                },
                RenderQaPassThreshold: 0.92),
            Metadata = new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["quality_threshold_source"] = "caller_forged_package_source",
                ["caller_private_marker"] = privateMarker
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-9-typed-threshold-failure-telemetry-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["quality_threshold_source"] = "caller_forged_loop_source",
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, initialPackage);
        Assert.DoesNotContain("\"quality_threshold\":\"0.10\"", envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("\"quality_threshold\":\"0.99\"", envelope.ResponseBodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionImprovementLoopCoordinator.EvaluationFailed, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.Equal("0.92", root.GetProperty("boundaryMetadata").GetProperty("final_required_score").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.TypedRenderQaPassThresholdSource,
            root.GetProperty("boundaryMetadata").GetProperty("quality_threshold_source").GetString());

        var attemptMetadata = Assert.Single(root.GetProperty("attempts").EnumerateArray())
            .GetProperty("publicMetadata");
        Assert.Equal("0.92", attemptMetadata.GetProperty("required_score").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.TypedRenderQaPassThresholdSource,
            attemptMetadata.GetProperty("quality_threshold_source").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("visual_decision_quality_threshold_met").GetString());
        var failedEvaluationRequest = Assert.Single(failingEvaluator.Requests);
        Assert.Equal(0.92, failedEvaluationRequest.Budget.RenderQaPassThreshold);
        Assert.Empty(strategy.Requests);
    }

    [Theory]
    [InlineData("free")]
    [InlineData("paid")]
    [InlineData("pro")]
    public async Task TryRunImprovementLoopAsync_ServerOwnedTierExactQualityThresholdFloorIsCanonicalOverride(
        string tier)
    {
        const string privateMarker = "PRIVATE_N8_9_EXACT_TIER_THRESHOLD_SECRET";
        var threshold = ServerOwnedTierQualityThreshold(tier);
        var expectedThreshold = QualityThresholdText(threshold);
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            new ScriptedCandidateStrategy());
        var initialPackage = CreatePackage($"n8-9-exact-tier-threshold-{tier}", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: tier,
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = QualityThresholdRoundTripText(threshold),
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata = new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["caller_private_marker"] = privateMarker
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: $"n8-9-exact-tier-threshold-loop-{tier}",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, initialPackage);
        Assert.All(visualJudge.Requests, request => Assert.Equal(expectedThreshold, request.Metadata["quality_threshold"]));

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.Equal(expectedThreshold, root.GetProperty("boundaryMetadata").GetProperty("final_required_score").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.CanonicalQualityThresholdMetadataOverrideSource,
            root.GetProperty("boundaryMetadata").GetProperty("quality_threshold_source").GetString());

        var attemptMetadata = Assert.Single(root.GetProperty("attempts").EnumerateArray())
            .GetProperty("publicMetadata");
        Assert.Equal(expectedThreshold, attemptMetadata.GetProperty("required_score").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.CanonicalQualityThresholdMetadataOverrideSource,
            attemptMetadata.GetProperty("quality_threshold_source").GetString());
    }

    [Theory]
    [InlineData("free", "0.74")]
    [InlineData("paid", "0.84")]
    public async Task TryRunImprovementLoopAsync_ServerOwnedTierBelowFloorQualityThresholdFallsBackToProvisionalDefault(
        string tier,
        string configuredThreshold)
    {
        const string privateMarker = "PRIVATE_N8_9_BELOW_TIER_THRESHOLD_SECRET";
        var expectedThreshold = QualityThresholdText(ServerOwnedTierQualityThreshold(tier));
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            new ScriptedCandidateStrategy());
        var initialPackage = CreatePackage($"n8-9-below-tier-threshold-{tier}", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: tier,
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = configuredThreshold,
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata = new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["caller_private_marker"] = privateMarker
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: $"n8-9-below-tier-threshold-loop-{tier}",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, initialPackage);
        Assert.All(visualJudge.Requests, request => Assert.Equal(expectedThreshold, request.Metadata["quality_threshold"]));

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(expectedThreshold, root.GetProperty("boundaryMetadata").GetProperty("final_required_score").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdSource,
            root.GetProperty("boundaryMetadata").GetProperty("quality_threshold_source").GetString());

        var attemptMetadata = Assert.Single(root.GetProperty("attempts").EnumerateArray())
            .GetProperty("publicMetadata");
        Assert.Equal(expectedThreshold, attemptMetadata.GetProperty("required_score").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdSource,
            attemptMetadata.GetProperty("quality_threshold_source").GetString());
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_DevTierWithoutCanonicalQualityThresholdUsesProvisionalTraceSource()
        => await AssertTraceQualityThresholdSourceCannotBeForgedAsync(
            tier: "dev",
            expectedThreshold: "0.8",
            expectedSource: DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdSource);

    [Fact]
    public async Task TryRunImprovementLoopAsync_NonTierWithoutCanonicalQualityThresholdUsesFallbackTraceSourceAndRejectsForgedMetadata()
        => await AssertTraceQualityThresholdSourceCannotBeForgedAsync(
            tier: "enterprise",
            expectedThreshold: "0.85",
            expectedSource: DesignConversionVisualQaBoundaryPolicy.NonTierQualityThresholdFallbackSource);

    private static async Task AssertTraceQualityThresholdSourceCannotBeForgedAsync(
        string tier,
        string expectedThreshold,
        string expectedSource)
    {
        const string privateMarker = "PRIVATE_N8_9_TRACE_THRESHOLD_SOURCE_SECRET";
        var forgedSource = DesignConversionVisualQaBoundaryPolicy.CanonicalQualityThresholdMetadataOverrideSource;
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            new ScriptedCandidateStrategy());
        var initialPackage = CreatePackage($"n8-9-trace-threshold-source-{tier}", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: tier,
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold_source"] = forgedSource,
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata = new Dictionary<string, string>
            {
                ["quality_threshold_source"] = forgedSource,
                ["caller_private_marker"] = privateMarker
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: $"n8-9-trace-threshold-source-loop-{tier}",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold_source"] = forgedSource,
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, initialPackage);
        Assert.DoesNotContain(forgedSource, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.All(visualJudge.Requests, request => Assert.Equal(expectedThreshold, request.Metadata["quality_threshold"]));

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.Equal(expectedThreshold, root.GetProperty("boundaryMetadata").GetProperty("final_required_score").GetString());
        Assert.Equal(
            expectedSource,
            root.GetProperty("boundaryMetadata").GetProperty("quality_threshold_source").GetString());

        var attemptMetadata = Assert.Single(root.GetProperty("attempts").EnumerateArray())
            .GetProperty("publicMetadata");
        Assert.Equal(expectedThreshold, attemptMetadata.GetProperty("required_score").GetString());
        Assert.Equal(expectedSource, attemptMetadata.GetProperty("quality_threshold_source").GetString());
    }

    [Theory]
    [InlineData("NaN", "n8-9-invalid-threshold", "0.85")]
    [InlineData("2.00", "n8-9-invalid-threshold", "0.85")]
    [InlineData("-0.10", "n8-9-invalid-threshold", "0.85")]
    [InlineData("0.00", "n8-9-invalid-threshold", "0.85")]
    [InlineData("0.10", "pro", "0.9")]
    public async Task TryRunImprovementLoopAsync_InvalidOrLoweredQualityThresholdFallsBackToServerBoundary(
        string configuredThreshold,
        string tier,
        string expectedThreshold)
    {
        const string privateMarker = "PRIVATE_N8_9_INVALID_THRESHOLD_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            new ScriptedCandidateStrategy());
        var initialPackage = CreatePackage($"n8-9-invalid-threshold-{tier}-{configuredThreshold}", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: tier,
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = configuredThreshold,
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata = new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["caller_private_marker"] = privateMarker
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: $"n8-9-invalid-threshold-loop-{tier}-{configuredThreshold}",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, initialPackage);
        Assert.DoesNotContain("\"quality_threshold\":\"0.10\"", envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.All(visualJudge.Requests, request => Assert.Equal(expectedThreshold, request.Metadata["quality_threshold"]));

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        Assert.Equal(
            expectedThreshold,
            document.RootElement.GetProperty("boundaryMetadata").GetProperty("final_required_score").GetString());
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CumulativeVisualCostStopsBeforeNextCandidateWouldExceedBudget()
    {
        const string privateMarker = "PRIVATE_N8_4_CUMULATIVE_COST_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromExternalScore(0.72, 0.04m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n8-4-should-not-be-created", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("n8-4-cumulative-cost-request", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "n8-4-local-loop",
                MaxImprovementCandidates: 5,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.10m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90"
                })
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-4-cumulative-cost-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Contains(
            root.GetProperty("warnings").EnumerateArray(),
            warning => warning.GetString() == "IMPROVEMENT_LOOP_NEXT_ESTIMATED_COST_EXCEEDS_REMAINING_BUDGET");

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal("0.08", attemptMetadata.GetProperty("estimated_cost_usd").GetString());
        Assert.Equal("0.08", attemptMetadata.GetProperty("loop_spent_budget_usd").GetString());
        Assert.Equal("0.1", attemptMetadata.GetProperty("budget_usd").GetString());
        Assert.Equal("0.02", attemptMetadata.GetProperty("remaining_budget_usd").GetString());
        Assert.Equal("0.08", root.GetProperty("boundaryMetadata").GetProperty("spent_budget_usd").GetString());
        Assert.Equal("0.02", root.GetProperty("boundaryMetadata").GetProperty("final_remaining_budget_usd").GetString());

        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_MissingCoverageCostProjectionStopsBeforeSameCandidateRetry()
    {
        const string privateMarker = "PRIVATE_N8_4_COVERAGE_COST_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromExternalScore(0.96, 0.08m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n8-4-coverage-should-not-run", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var basePackage = CreatePackage("n8-4-coverage-cost-request", privateMarker);
        var initialPackage = basePackage with
        {
            Budget = basePackage.Budget! with
            {
                Tier = "n8-4-coverage-cost-loop",
                MaxImprovementCandidates = 5,
                MaxRenderQaRuns = 1,
                BudgetUsd = 0.10m
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-4-coverage-cost-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.True(root.GetProperty("renderManifestIncomplete").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Contains(
            root.GetProperty("warnings").EnumerateArray(),
            warning => warning.GetString() == "IMPROVEMENT_LOOP_COVERAGE_RETRY_STOPPED:cost_budget");
        Assert.Contains(
            root.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => evidence.GetString() == "improvement_loop:coverage_retry_stop_source=cost_budget");
        Assert.Contains(
            root.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => evidence.GetString() == DesignConversionVisualQaBoundaryPolicy.CoverageRetryMarker);

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal("0.08", attemptMetadata.GetProperty("estimated_cost_usd").GetString());
        Assert.Equal("0.08", attemptMetadata.GetProperty("loop_spent_budget_usd").GetString());
        Assert.Equal("0.02", attemptMetadata.GetProperty("remaining_budget_usd").GetString());
        Assert.Equal("cost_budget", attemptMetadata.GetProperty("coverage_retry_stop_source").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("coverage_retry_same_candidate").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal("0.08", root.GetProperty("boundaryMetadata").GetProperty("spent_budget_usd").GetString());
        Assert.Equal("0.02", root.GetProperty("boundaryMetadata").GetProperty("final_remaining_budget_usd").GetString());

        Assert.Single(renderProvider.Requests);
        Assert.Single(comparisonProvider.Requests);
        Assert.Single(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CoverageRetryMayConsumeExactRemainingBudget()
    {
        const string privateMarker = "PRIVATE_N8_4_COVERAGE_EQUALITY_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromExternalScore(0.96, 0.08m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n8-4-coverage-equality-should-not-run", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var basePackage = CreatePackage("n8-4-coverage-equality-request", privateMarker);
        var initialPackage = basePackage with
        {
            Budget = basePackage.Budget! with
            {
                Tier = "n8-4-coverage-equality-loop",
                MaxImprovementCandidates = 5,
                MaxRenderQaRuns = 1,
                BudgetUsd = 0.16m
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-4-coverage-equality-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("renderManifestIncomplete").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        Assert.Equal("0.16", root.GetProperty("boundaryMetadata").GetProperty("spent_budget_usd").GetString());
        Assert.Equal("0", root.GetProperty("boundaryMetadata").GetProperty("final_remaining_budget_usd").GetString());
        Assert.Contains(
            root.GetProperty("warnings").EnumerateArray(),
            warning => warning.GetString() == "IMPROVEMENT_LOOP_COVERAGE_RETRY_STOPPED:cost_budget");

        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_NullBudgetDefaultRunsRequiredBrowserQaViewportMatrix()
    {
        const string privateMarker = "PRIVATE_N6_3_NULL_BUDGET_MULTI_VIEWPORT_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("n6-3-null-budget-multi-viewport-request", privateMarker) with
        {
            Screenshots = CreateRequiredBrowserQaArtifacts("n6-3-null-budget-multi-viewport-request:source", "screenshot"),
            DesignRenders = CreateRequiredBrowserQaArtifacts("n6-3-null-budget-multi-viewport-request:design", "design-render"),
            Budget = null
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n6-3-null-budget-multi-viewport-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.True(root.GetProperty("qualityReached").GetBoolean());
        Assert.False(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        Assert.Equal(0.50m, attempt.GetProperty("budgetUsd").GetDecimal());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal("0.5", attemptMetadata.GetProperty("submitted_budget_usd").GetString());
        Assert.Equal("0.5", attemptMetadata.GetProperty("effective_budget_usd").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("tier_policy_budget_entitlement_default_applied").GetString());
        Assert.Equal(4, attempt.GetProperty("renderResultRefs").GetArrayLength());
        Assert.Equal(4, attempt.GetProperty("comparisonArtifactRefs").GetArrayLength());
        Assert.Equal(4, attempt.GetProperty("visualJudgeResultRefs").GetArrayLength());
        var responseMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal("0.5", responseMetadata.GetProperty("submitted_budget_usd").GetString());
        Assert.Equal("0.5", responseMetadata.GetProperty("effective_budget_usd").GetString());
        Assert.Equal(bool.FalseString, responseMetadata.GetProperty("tier_policy_budget_entitlement_default_applied").GetString());
        Assert.Equal("0.5", envelope.BoundaryMetadata["submitted_budget_usd"]);
        Assert.Equal("0.5", envelope.BoundaryMetadata["effective_budget_usd"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["tier_policy_budget_entitlement_default_applied"]);
        Assert.Equal(
            [
                "source-360x800",
                "source-768x1024",
                "source-1280x720",
                "source-1920x1080"
            ],
            renderProvider.Requests.Select(request => request.Resolution.ResolutionId).ToArray());
        Assert.Equal([360, 768, 1280, 1920], renderProvider.Requests.Select(request => request.Resolution.Width).ToArray());
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_SingleLogicalViewportDefaultRunsRequiredBrowserQaMatrixAndFailsClosed()
    {
        const string privateMarker = "PRIVATE_N6_4_SINGLE_VIEWPORT_EXPECTED_REF_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("n6-4-single-mobile-default-request", privateMarker) with
        {
            Screenshots =
            [
                CreateArtifact("n6-4-single-mobile-default-request:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders =
            [
                CreateArtifact("n6-4-single-mobile-default-request:design-mobile", "design-render", 390, 844)
            ],
            Budget = null
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n6-4-single-mobile-default-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.DoesNotContain("artifact://", envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("local_path", envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
        foreach (var artifact in initialPackage.Screenshots!.Concat(initialPackage.DesignRenders!))
        {
            Assert.DoesNotContain(artifact.Uri, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(artifact.Sha256, envelope.ResponseBodyJson, StringComparison.Ordinal);
        }

        Assert.False(envelope.RawPrivateResponseLeakageDetected);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("providerRequiredOrBlocked").GetBoolean());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.False(root.GetProperty("renderManifestIncomplete").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Contains(
            root.GetProperty("warnings").EnumerateArray(),
            warning => warning.GetString() == DesignConversionInternalService.ViewportExpectedReferenceMissingStatus);

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        Assert.Equal(4, attempt.GetProperty("renderResultRefs").GetArrayLength());
        Assert.Empty(attempt.GetProperty("comparisonArtifactRefs").EnumerateArray());
        Assert.Empty(attempt.GetProperty("visualJudgeResultRefs").EnumerateArray());
        Assert.Contains(
            attempt.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => evidence.GetString() == "improvement_loop:stop_reason=provider_required");
        Assert.Equal("4", attempt.GetProperty("publicMetadata").GetProperty("render_result_count").GetString());
        Assert.Equal("0", attempt.GetProperty("publicMetadata").GetProperty("comparison_artifact_count").GetString());
        Assert.Equal("0", attempt.GetProperty("publicMetadata").GetProperty("visual_judge_result_count").GetString());

        Assert.Equal(
            [
                DesignConversionRenderResolutionFactory.RequiredBrowserQaMobileResolutionId,
                DesignConversionRenderResolutionFactory.RequiredBrowserQaTabletResolutionId,
                DesignConversionRenderResolutionFactory.RequiredBrowserQaLaptopResolutionId,
                DesignConversionRenderResolutionFactory.RequiredBrowserQaDesktopResolutionId
            ],
            renderProvider.Requests.Select(request => request.Resolution.ResolutionId).ToArray());
        Assert.Equal(
            [360, 768, 1280, 1920],
            renderProvider.Requests.Select(request => request.Resolution.Width).ToArray());
        Assert.Equal(
            [800, 1024, 720, 1080],
            renderProvider.Requests.Select(request => request.Resolution.Height).ToArray());
        Assert.Equal(4, renderProvider.Requests.Count);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("quality_reached").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("viewport_operation_failed").GetString());
        Assert.Equal("4", metadata.GetProperty("viewport_failure_count").GetString());
        Assert.Equal("expected_reference", metadata.GetProperty("viewport_failure_stage").GetString());
        Assert.Equal(DesignConversionInternalService.ExpectedRenderResolutionMissingCode, metadata.GetProperty("viewport_failure_error_code").GetString());
        Assert.Equal(DesignConversionInternalService.ViewportExpectedReferenceMissingStatus, metadata.GetProperty("viewport_failure_status").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("expected_reference_failure_bounded").GetString());
        Assert.Equal(DesignConversionInternalService.ExpectedRenderResolutionMissingCode, metadata.GetProperty("expected_reference_failure_code").GetString());
        Assert.Equal(DesignConversionInternalService.ViewportExpectedReferenceMissingStatus, metadata.GetProperty("expected_reference_failure_status").GetString());
        var metadataJson = metadata.GetRawText();
        Assert.DoesNotContain(privateMarker, metadataJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Html!, metadataJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Css!, metadataJson, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact://", metadataJson, StringComparison.Ordinal);
        Assert.DoesNotContain("local_path", metadataJson, StringComparison.OrdinalIgnoreCase);
        foreach (var artifact in initialPackage.Screenshots!.Concat(initialPackage.DesignRenders!))
        {
            Assert.DoesNotContain(artifact.Uri, metadataJson, StringComparison.Ordinal);
            Assert.DoesNotContain(artifact.Sha256, metadataJson, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_ExplicitMaxRenderQaRunsOneReturnsBoundedIncompleteResponsiveCoverage()
    {
        const string privateMarker = "PRIVATE_N6_3_EXPLICIT_ONE_VIEWPORT_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var artifactDbRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "improvement-loop-incomplete-responsive-artifact-db");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy, artifactDb);
        var basePackage = CreatePackage("n6-3-explicit-one-viewport-request", privateMarker);
        var initialPackage = basePackage with
        {
            Screenshots = CreateRequiredBrowserQaArtifacts("n6-3-explicit-one-viewport-request:source", "screenshot"),
            DesignRenders = CreateRequiredBrowserQaArtifacts("n6-3-explicit-one-viewport-request:design", "design-render"),
            Budget = basePackage.Budget! with
            {
                MaxRenderQaRuns = 1
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n6-3-explicit-one-viewport-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });
        var responsiveMatrix = DesignConversionRenderResolutionFactory.CreateModelServerMinimumResponsiveMatrix(
            initialPackage.DesignRenders!
                .Concat(initialPackage.Screenshots!)
                .Select(artifact => new DesignRenderArtifact(
                    artifact.ArtifactId,
                    artifact.Kind,
                    artifact.Width,
                    artifact.Height,
                    artifact.Uri,
                    artifact.Sha256,
                    artifact.Metadata ?? new Dictionary<string, string>())));

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.Null(envelope.ParsedErrorMessage);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);

        var records = await artifactDb.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        Assert.Equal("0", stored.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Equal(bool.FalseString, stored.Metadata["runtime_created_dpo_candidate"]);
        Assert.Contains(
            "runtime_created_dpo_candidate_not_created_visual_decision_can_persist_training_candidate_false",
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Contains(
            "runtime_created_dpo_candidate_quarantine_reason=visual_decision_can_persist_training_candidate_false",
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Empty(stored.DpoCandidates);
        Assert.DoesNotContain("runtime_created_dpo_candidate", envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["evaluation_failed"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["render_manifest_incomplete"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["quality_reached"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
            envelope.BoundaryMetadata["stop_reason"]);
        Assert.Equal("1", envelope.BoundaryMetadata["render_result_count"]);
        Assert.Equal("1", envelope.BoundaryMetadata["comparison_artifact_count"]);
        Assert.Equal("1", envelope.BoundaryMetadata["visual_judge_result_count"]);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
            root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("providerRequiredOrBlocked").GetBoolean());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.True(root.GetProperty("renderManifestIncomplete").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Contains(
            root.GetProperty("warnings").EnumerateArray(),
            warning => warning.GetString() == "IMPROVEMENT_LOOP_COVERAGE_RETRY_STOPPED:configured_max_attempts");
        Assert.DoesNotContain(
            root.GetProperty("warnings").EnumerateArray(),
            warning => warning.GetString()?.StartsWith("IMPROVEMENT_LOOP_EVALUATION_FAILED:", StringComparison.Ordinal) == true);
        Assert.Contains(
            root.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => evidence.GetString() == "improvement_loop:evaluation_failed_false");
        Assert.Contains(
            root.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => evidence.GetString() == "improvement_loop:coverage_retry_stop_source=configured_max_attempts");

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        Assert.Single(attempt.GetProperty("renderResultRefs").EnumerateArray());
        Assert.Single(attempt.GetProperty("comparisonArtifactRefs").EnumerateArray());
        Assert.Single(attempt.GetProperty("visualJudgeResultRefs").EnumerateArray());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
            attempt.GetProperty("stopReason").GetString());
        Assert.False(attempt.GetProperty("shouldContinue").GetBoolean());
        Assert.Contains(
            attempt.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => evidence.GetString() == DesignConversionVisualQaBoundaryPolicy.CoverageRetryMarker);
        Assert.Contains(
            attempt.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => evidence.GetString() == "improvement_loop:stop_reason=render_manifest_incomplete");
        var publicMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
            publicMetadata.GetProperty("stop_reason").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.RetryModeRetrySameCandidate,
            publicMetadata.GetProperty("retry_mode").GetString());
        Assert.Equal(bool.FalseString, publicMetadata.GetProperty("coverage_retry").GetString());
        Assert.Equal("0", publicMetadata.GetProperty("coverage_retry_count").GetString());
        Assert.Equal("configured_max_attempts", publicMetadata.GetProperty("coverage_retry_stop_source").GetString());
        Assert.Equal(bool.TrueString, publicMetadata.GetProperty("coverage_retry_same_candidate").GetString());
        Assert.Equal("initial_incumbent", publicMetadata.GetProperty("candidate_acceptance_decision").GetString());
        Assert.Equal("none", publicMetadata.GetProperty("candidate_rejection_reason").GetString());
        Assert.Equal(bool.FalseString, publicMetadata.GetProperty("required_viewport_coverage_complete").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine, publicMetadata.GetProperty("visual_decision").GetString());
        Assert.Equal(bool.FalseString, publicMetadata.GetProperty("visual_decision_can_retry").GetString());
        Assert.Equal(bool.FalseString, publicMetadata.GetProperty("product_ready").GetString());
        Assert.Equal("1", publicMetadata.GetProperty("render_result_count").GetString());
        Assert.Equal("1", publicMetadata.GetProperty("comparison_artifact_count").GetString());
        Assert.Equal("1", publicMetadata.GetProperty("visual_judge_result_count").GetString());

        Assert.True(responsiveMatrix.IsSuccess, responsiveMatrix.ErrorMessage);
        Assert.Equal(
            [
                "source-360x800",
                "source-768x1024",
                "source-1280x720",
                "source-1920x1080"
            ],
            responsiveMatrix.Value!.Select(resolution => resolution.ResolutionId).ToArray());
        Assert.Equal("source-360x800", Assert.Single(renderProvider.Requests).Resolution.ResolutionId);
        Assert.Equal("source-360x800", Assert.Single(comparisonProvider.Requests).Actual.Resolution.ResolutionId);
        Assert.Equal("source-360x800", Assert.Single(visualJudge.Requests).RenderedImage.Resolution.ResolutionId);
        Assert.Empty(strategy.Requests);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.OriginalTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact://", envelope.ResponseBodyJson, StringComparison.Ordinal);
        foreach (var artifact in initialPackage.Screenshots!.Concat(initialPackage.DesignRenders!))
            Assert.DoesNotContain(artifact.Sha256, envelope.ResponseBodyJson, StringComparison.Ordinal);

        var storedJson = JsonSerializer.Serialize(stored, DesignConversionCheckpointJsonOptions.Standard);
        Assert.DoesNotContain(privateMarker, storedJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Html!, storedJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Css!, storedJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.DesignTree!.Payload!, storedJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.OriginalTree!.Payload!, storedJson, StringComparison.Ordinal);
        foreach (var artifact in initialPackage.Screenshots!.Concat(initialPackage.DesignRenders!))
            Assert.DoesNotContain(artifact.Uri, storedJson, StringComparison.Ordinal);
        Assert.DoesNotContain("raw-tree", storedJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", storedJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", storedJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_FreeTierCapStopsLoopAndExposesSafePolicyMetadata()
    {
        const string privateMarker = "PRIVATE_PASS_TIER_POLICY_API_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.70);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("pass-tier-policy-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("pass-tier-policy-api-request", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "free",
                MaxImprovementCandidates: 10,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["budget_private_marker"] = privateMarker
                })
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass-tier-policy-api-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        var expectedFreeThreshold = QualityThresholdText(
            DesignConversionVisualQaBoundaryPolicy.ProvisionalFreeTierQualityThreshold);
        Assert.Equal("free", attemptMetadata.GetProperty("tier").GetString());
        Assert.Equal(expectedFreeThreshold, attemptMetadata.GetProperty("required_score").GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdSource,
            attemptMetadata.GetProperty("quality_threshold_source").GetString());
        Assert.Equal("0.05", attemptMetadata.GetProperty("effective_budget_usd").GetString());
        Assert.Equal("1", attemptMetadata.GetProperty("effective_max_improvement_candidates").GetString());
        Assert.Equal("1", attemptMetadata.GetProperty("submitted_budget_usd").GetString());
        Assert.Equal("10", attemptMetadata.GetProperty("submitted_max_improvement_candidates").GetString());
        Assert.Equal("iteration_budget", attemptMetadata.GetProperty("budget_stop_source").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("tier_policy_budget_cap_applied").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("tier_policy_iteration_cap_applied").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("raw_design_tree_included").GetString());

        Assert.Empty(strategy.Requests);
        Assert.Equal("free", envelope.BoundaryMetadata["tier"]);
        Assert.Equal(expectedFreeThreshold, envelope.BoundaryMetadata["final_required_score"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdSource,
            envelope.BoundaryMetadata["quality_threshold_source"]);
        Assert.Equal("0.05", envelope.BoundaryMetadata["effective_budget_usd"]);
        Assert.Equal("1", envelope.BoundaryMetadata["effective_max_improvement_candidates"]);
        Assert.Equal("1", envelope.BoundaryMetadata["submitted_budget_usd"]);
        Assert.Equal("10", envelope.BoundaryMetadata["submitted_max_improvement_candidates"]);
        Assert.Equal("iteration_budget", envelope.BoundaryMetadata["budget_stop_source"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["tier_policy_iteration_cap_applied"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["provider_payload_included"]);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_RuntimeImprovementStrategyProducesChangedSecondAttempt()
    {
        const string privateMarker = "PRIVATE_PASS_NON_COPY_LOOP_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.72);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.96, 0.01m)
                : JudgeScript.FromScore(0.72, 0.01m));
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            new DesignConversionRuntimeImprovementCandidateStrategy());
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass-non-copy-runtime-loop",
            InitialPackage: CreatePackage("pass-non-copy-runtime-request", privateMarker),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["local_non_copy_candidate_strategy"] = bool.TrueString
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        var attempts = root.GetProperty("attempts").EnumerateArray().ToArray();
        Assert.Equal(2, attempts.Length);
        Assert.Equal(1, attempts[0].GetProperty("attemptNumber").GetInt32());
        Assert.Equal(2, attempts[1].GetProperty("attemptNumber").GetInt32());

        var firstAttemptRenderRequests = renderProvider.Requests
            .Where(request => !request.Metadata["job_id"].Contains("loop-attempt:2", StringComparison.Ordinal))
            .ToArray();
        var secondAttemptRenderRequests = renderProvider.Requests
            .Where(request => request.Metadata["job_id"].Contains("loop-attempt:2", StringComparison.Ordinal))
            .ToArray();
        Assert.NotEmpty(firstAttemptRenderRequests);
        Assert.NotEmpty(secondAttemptRenderRequests);
        Assert.All(secondAttemptRenderRequests, request =>
        {
            Assert.NotEqual(firstAttemptRenderRequests[0].RequestId, request.RequestId);
            Assert.NotEqual(firstAttemptRenderRequests[0].Html, request.Html);
            Assert.NotEqual(firstAttemptRenderRequests[0].Css, request.Css);
            Assert.Contains("dcm-local-improved", request.Html, StringComparison.Ordinal);
            Assert.Contains("gap:12px", request.Css, StringComparison.Ordinal);
            Assert.Contains("padding:16px", request.Css, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_LocalArtifactDbFailureAfterSuccessfulLoopReturnsNonDurableResult()
    {
        const string privateMarker = "PRIVATE_N8_12_DB_FAILURE_SECRET";
        const string rawDbPath = "C:\\private\\artifact-db\\PRIVATE_N8_12_DB_FAILURE_SECRET\\conversion-results.jsonl";
        const string rawExceptionMessage = "raw DB append failed at C:\\private\\artifact-db\\PRIVATE_N8_12_DB_FAILURE_SECRET\\conversion-results.jsonl";
        var artifactDb = new FailingAppendArtifactDb(rawExceptionMessage, "SCRIPTED_PERSISTENCE_FAILURE");
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy, artifactDb);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-12-db-failure-loop",
            InitialPackage: CreatePackage("n8-12-db-failure-request", privateMarker),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: true,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator
                .LocalArtifactDbAppendFailedErrorCode,
            envelope.ParsedErrorCode);
        Assert.Equal(
            "Improvement-loop persistence did not complete durably.",
            envelope.ParsedErrorMessage);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(1, artifactDb.AppendAttempts);
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);

        Assert.Null(envelope.ParsedContractStatus);
        Assert.DoesNotContain(
            "\"completed\":true",
            envelope.ResponseBodyJson,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "\"qualityReached\":true",
            envelope.ResponseBodyJson,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(rawExceptionMessage, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawDbPath, envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_ConfiguredCompositionRootsAutowriteOnceAndNoConfigIsNonDurable()
    {
        const string privateMarker = "PRIVATE_N8_13_COMPOSITION_AUTOWRITE_SECRET";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "n8-13-composition-autowrite");
        Directory.CreateDirectory(root);
        try
        {
            var startupArtifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(root, "startup-improvement-only-db"));
            var startupAdapter = CreateStartupImprovementLoopAdapter(startupArtifactDb);
            var startupPackage = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "n8-13-startup-loop",
                InitialPackage: CreatePackage("n8-13-startup-request", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["n8_13_composition"] = "startup_improvement_only",
                    ["caller_private_marker"] = privateMarker
                });

            var startupEnvelope = await startupAdapter.TryRunImprovementLoopAsync(startupPackage);

            var startupStored = await AssertConfiguredAutowriteAsync(
                startupArtifactDb,
                startupEnvelope,
                startupPackage,
                Path.Combine(root, "startup-improvement-only-db"),
                privateMarker);

            var startupReplayEnvelope =
                await startupAdapter.TryRunImprovementLoopAsync(startupPackage);
            AssertStableImprovementLoopEnvelope(
                startupReplayEnvelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal(
                startupEnvelope.BoundaryMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .ImprovementLoopHistorySha256MetadataKey],
                startupReplayEnvelope.BoundaryMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .ImprovementLoopHistorySha256MetadataKey]);
            using (var firstDocument = JsonDocument.Parse(
                       startupEnvelope.ResponseBodyJson))
            using (var replayDocument = JsonDocument.Parse(
                       startupReplayEnvelope.ResponseBodyJson))
            {
                Assert.Equal(
                    firstDocument.RootElement.GetProperty("attempts")
                        .EnumerateArray()
                        .Select(attempt =>
                            attempt.GetProperty("attemptNumber").GetInt32()),
                    replayDocument.RootElement.GetProperty("attempts")
                        .EnumerateArray()
                        .Select(attempt =>
                            attempt.GetProperty("attemptNumber").GetInt32()));
            }

            var startupRecordsAfterReplay =
                await startupArtifactDb.ListRecordsAsync();
            Assert.True(
                startupRecordsAfterReplay.IsSuccess,
                startupRecordsAfterReplay.ErrorMessage);
            var startupStoredAfterReplay = Assert.Single(
                startupRecordsAfterReplay.Value!);
            Assert.Equal(startupStored.RecordId, startupStoredAfterReplay.RecordId);
            Assert.Equal(
                startupStored.Metadata[
                    DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryMetadataKey],
                startupStoredAfterReplay.Metadata[
                    DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryMetadataKey]);

            var fullRouteArtifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(root, "full-local-route-db"));
            var fullRouteAdapter = CreateFullLocalRouteAdapter(fullRouteArtifactDb);
            var fullRoutePackage = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "n8-13-full-route-loop",
                InitialPackage: CreatePackage("n8-13-full-route-request", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["n8_13_composition"] = "full_local_route",
                    ["caller_private_marker"] = privateMarker
                });

            var fullRouteEnvelope = await fullRouteAdapter.TryRunImprovementLoopAsync(fullRoutePackage);

            var fullRouteStored = await AssertConfiguredAutowriteAsync(
                fullRouteArtifactDb,
                fullRouteEnvelope,
                fullRoutePackage,
                Path.Combine(root, "full-local-route-db"),
                privateMarker);

            AssertMatchingAutowriteMetadata(startupStored, fullRouteStored);

            var noConfigAdapter = CreateStartupImprovementLoopAdapter(artifactDb: null);
            var noConfigPackage = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "n8-13-no-config-loop",
                InitialPackage: CreatePackage("n8-13-no-config-request", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["n8_13_composition"] = "startup_no_config",
                    ["caller_private_marker"] = privateMarker
                });

            var noConfigEnvelope = await noConfigAdapter.TryRunImprovementLoopAsync(noConfigPackage);

            AssertStableImprovementLoopEnvelope(
                noConfigEnvelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal("success", noConfigEnvelope.EnvelopeKind);
            Assert.Equal(DesignConversionThinClientStatus.Completed, noConfigEnvelope.ParsedContractStatus);
            Assert.Equal("not_configured_non_durable", noConfigEnvelope.BoundaryMetadata["local_artifact_db_persistence_status"]);
            Assert.Equal(bool.FalseString, noConfigEnvelope.BoundaryMetadata["persistence_invoked"]);
            Assert.DoesNotContain(
                JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker,
                noConfigEnvelope.ResponseBodyJson,
                StringComparison.Ordinal);
            AssertNoPrivateMarkerLeak(noConfigEnvelope.ResponseBodyJson, privateMarker, noConfigPackage.InitialPackage!);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }

        static DesignConversionModelServerLocalCallerAdapter CreateStartupImprovementLoopAdapter(
            IDesignConversionLocalArtifactDb? artifactDb)
        {
            var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting(
                comparisonProvider: new RecordingComparisonProvider(_ => 0.95),
                visualJudge: new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0m)));
            var strategy = new ScriptedCandidateStrategy();
            var registry = artifactDb is null
                ? DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
                    composition,
                    strategy)
                : DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
                    composition,
                    strategy,
                    artifactDb,
                    () => RouteAdmissionNow);
            return new DesignConversionModelServerLocalCallerAdapter(registry);
        }

        static DesignConversionModelServerLocalCallerAdapter CreateFullLocalRouteAdapter(
            IDesignConversionLocalArtifactDb artifactDb)
        {
            var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting(
                comparisonProvider: new RecordingComparisonProvider(_ => 0.95),
                visualJudge: new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0m)));
            var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                new ScriptedCandidateStrategy(),
                artifactDb,
                () => RouteAdmissionNow);
            return new DesignConversionModelServerLocalCallerAdapter(registry);
        }

        static async Task<DesignConversionLocalArtifactDbRecord> AssertConfiguredAutowriteAsync(
            JsonlDesignConversionLocalArtifactDb artifactDb,
            DesignConversionModelServerLocalCallerAdapterResponse envelope,
            DesignConversionModelServerImprovementLoopLocalCallerPackage package,
            string artifactDbRoot,
            string privateMarker)
        {
            AssertStableImprovementLoopEnvelope(
                envelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal("success", envelope.EnvelopeKind);
            Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
            Assert.Equal(package.InitialPackage!.RequestId, envelope.ParsedRequestId);
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker,
                envelope.ResponseBodyJson,
                StringComparison.Ordinal);
            Assert.Equal(
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryRecordedStatus,
                envelope.BoundaryMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .ImprovementLoopHistoryStatusMetadataKey]);
            Assert.Equal(
                "1",
                envelope.BoundaryMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .ImprovementLoopHistoryAttemptCountMetadataKey]);
            Assert.StartsWith(
                "sha256:",
                envelope.BoundaryMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .ImprovementLoopHistorySha256MetadataKey],
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryMetadataKey,
                envelope.ResponseBodyJson,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "design-conversion-improvement-loop-attempt-history",
                envelope.ResponseBodyJson,
                StringComparison.Ordinal);
            Assert.DoesNotContain(artifactDbRoot, envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
            AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage);

            var allRecords = await artifactDb.ListRecordsAsync();
            Assert.True(allRecords.IsSuccess, allRecords.ErrorMessage);
            var stored = Assert.Single(allRecords.Value!);
            Assert.Equal(package.InitialPackage.RequestId, stored.RequestId);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, stored.DatabaseKind);
            Assert.Equal(nameof(DesignConversionImprovementLoopCoordinator), stored.Metadata["artifact_db_record_source"]);
            Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker]);
            Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker]);
            Assert.Equal(bool.TrueString, stored.Metadata["improvement_loop_to_local_training_store"]);
            Assert.Equal("1", stored.Metadata["attempt_count"]);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, stored.Metadata["stop_reason"]);
            Assert.Equal(bool.FalseString, stored.Metadata["final_candidate_from_loop"]);
            Assert.Equal(bool.FalseString, stored.Metadata["raw_html_included"]);
            Assert.Equal(bool.FalseString, stored.Metadata["raw_css_included"]);
            Assert.Equal(bool.FalseString, stored.Metadata["raw_design_tree_included"]);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, stored.EvidenceRefs);
            Assert.True(stored.TryGetImprovementLoopAttemptHistory(out var history));
            Assert.NotNull(history);
            Assert.Equal(1, history!.AttemptCount);
            Assert.Equal(
                envelope.BoundaryMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .ImprovementLoopHistorySha256MetadataKey],
                history.HistorySha256);

            var storedJson = JsonSerializer.Serialize(stored, DesignConversionCheckpointJsonOptions.Standard);
            Assert.DoesNotContain(privateMarker, storedJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.Html!, storedJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.Css!, storedJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, storedJson, StringComparison.Ordinal);
            Assert.DoesNotContain(artifactDbRoot, storedJson, StringComparison.OrdinalIgnoreCase);
            return stored;
        }

        static void AssertMatchingAutowriteMetadata(
            DesignConversionLocalArtifactDbRecord left,
            DesignConversionLocalArtifactDbRecord right)
        {
            var keys = new[]
            {
                "artifact_db_record_source",
                DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker,
                DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker,
                "improvement_loop_to_local_training_store",
                "attempt_count",
                "stop_reason",
                "final_candidate_from_loop",
                "raw_html_included",
                "raw_css_included",
                "raw_design_tree_included",
                "training_candidate_only",
                "product_not_ready",
                "production_db_ready"
            };

            foreach (var key in keys)
                Assert.Equal(left.Metadata[key], right.Metadata[key]);
        }
    }

    [Fact]
    public async Task Pass163_SameScenarioPackageToLocalDpoIntakeStopsAtSourceDisjointGateWithoutPromotion()
    {
        const string privateMarker = "PRIVATE_PASS163_SAME_SCENARIO_SECRET";
        const string loopId = "pass163-same-scenario-loop";
        const string requestId = "pass163-same-scenario-request";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "pass163-same-scenario-chain");
        Directory.CreateDirectory(root);
        try
        {
            var runnerInvocationCount = 0;
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                    Interlocked.Increment(ref runnerInvocationCount),
                legalTeacherAdmissionAuthority: DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(
                    modelId: ExpectedTeacherModelId,
                    expectedOllamaManifestDigest: ExpectedTeacherManifestDigest,
                    expectedOllamaModelLayerDigest: ExpectedTeacherModelLayerDigest));
            using var teacherAssociation =
                DesignConversionDpoTeacherTestAssociation.Associate(
                    artifactDb,
                    supportTeacherAuthoring: true,
                    supportIndependentPreferenceEvidence: true);
            var usefulnessApi = new DesignConversionDpoUsefulnessApi(artifactDb);
            var renderProvider = new DeterministicDesignConversionRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(request =>
                request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.95 : 0.72);
            var visualJudge = new ScriptedVisualJudge(request =>
                request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                    ? JudgeScript.FromScore(0.95, 0m)
                    : JudgeScript.FromScore(0.72, 0m));
            var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate("pass163-improved-candidate", privateMarker)));
            var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy, artifactDb);
            var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: loopId,
                InitialPackage: CreatePass163Package(requestId, privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["pass163_same_scenario_id"] = "pass163-local-chain",
                    ["local_only"] = bool.TrueString,
                    ["no_external_provider"] = bool.TrueString
                });

            var envelope = await adapter.TryRunImprovementLoopAsync(package);

            AssertStableImprovementLoopEnvelope(
                envelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal("success", envelope.EnvelopeKind);
            Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
            Assert.Equal(requestId, envelope.ParsedRequestId);
            Assert.False(envelope.RawPrivateResponseLeakageDetected);
            Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);

            using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
            var responseRoot = document.RootElement;
            Assert.Equal(loopId, responseRoot.GetProperty("loopId").GetString());
            Assert.Equal(requestId, responseRoot.GetProperty("requestId").GetString());
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, responseRoot.GetProperty("stopReason").GetString());
            Assert.True(responseRoot.GetProperty("completed").GetBoolean());
            Assert.True(responseRoot.GetProperty("qualityReached").GetBoolean());
            Assert.Equal(2, responseRoot.GetProperty("attemptCount").GetInt32());
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_ai_visual_judge_executed"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["real_external_provider_executed"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["network_call_executed"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["deployment_verified"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
            Assert.Equal("2", envelope.BoundaryMetadata["render_result_count"]);
            Assert.Equal("2", envelope.BoundaryMetadata["comparison_artifact_count"]);
            Assert.Equal("2", envelope.BoundaryMetadata["visual_judge_result_count"]);
            Assert.Equal(4, comparisonProvider.Requests.Count);
            Assert.Equal(4, visualJudge.Requests.Count);
            Assert.Empty(strategy.Requests);

            var attempts = responseRoot.GetProperty("attempts").EnumerateArray().ToArray();
            Assert.Equal(2, attempts.Length);
            var attempt = attempts[^1];
            Assert.Equal(2, attempt.GetProperty("renderResultRefs").GetArrayLength());
            Assert.Equal(2, attempt.GetProperty("comparisonArtifactRefs").GetArrayLength());
            Assert.Equal(2, attempt.GetProperty("visualJudgeResultRefs").GetArrayLength());
            var attemptMetadata = attempt.GetProperty("publicMetadata");
            Assert.Equal("2", attemptMetadata.GetProperty("render_result_count").GetString());
            Assert.Equal("2", attemptMetadata.GetProperty("comparison_artifact_count").GetString());
            Assert.Equal("2", attemptMetadata.GetProperty("visual_judge_result_count").GetString());
            Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
            Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("real_external_provider_executed").GetString());
            Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("network_call_executed").GetString());
            Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("product_ready").GetString());

            var records = await artifactDb.ListRecordsAsync();
            Assert.True(records.IsSuccess, records.ErrorMessage);
            var stored = Assert.Single(records.Value!);
            Assert.Equal(DesignConversionSourceKind.Figma.ToString(), stored.SourceKind);
            Assert.Equal("pass163-local-no-provider", stored.Tier);
            Assert.Equal(package.InitialPackage.DesignTree.RefId, stored.SanitizedSourceRefs["source_manifest_ref"]);
            Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker]);
            Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker]);
            Assert.Equal(bool.TrueString, stored.Metadata["product_not_ready"]);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, stored.EvidenceRefs);
            var attemptHistoryBeforeApproval = stored.Metadata[
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryMetadataKey];
            Assert.True(stored.TryGetImprovementLoopAttemptHistory(out var storedHistory));
            Assert.NotNull(storedHistory);
            Assert.Equal(2, storedHistory!.AttemptCount);
            Assert.Equal(2, stored.RenderArtifactRefs.Count);
            Assert.Equal(
                ["source-1440x900", "source-390x844"],
                stored.RenderArtifactRefs.Select(artifact => artifact.ViewportId).Order(StringComparer.Ordinal).ToArray());
            Assert.All(stored.RenderArtifactRefs, artifact =>
            {
                Assert.StartsWith("artifact://design-conversion/render-scaffold/", artifact.UriRef, StringComparison.Ordinal);
                Assert.StartsWith("sha256:", artifact.Sha256, StringComparison.Ordinal);
                Assert.Equal(DeterministicDesignConversionRenderProvider.ProviderKind, artifact.Metadata["provider_kind"]);
                Assert.False(artifact.Metadata.ContainsKey("raw_html_included"));
                Assert.False(artifact.Metadata.ContainsKey("raw_css_included"));
            });
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, stored.VisualJudgeStatus.Status);
            Assert.False(stored.VisualJudgeStatus.ExternalCallExecuted);
            Assert.Equal(bool.TrueString, stored.VisualJudgeStatus.Metadata["product_not_ready"]);

            var dpoCandidates = stored.DpoCandidates.ToArray();
            Assert.Collection(
                dpoCandidates,
                candidate => Assert.Equal(DesignConversionDpoUnits.CriticUnit, candidate.UnitId),
                candidate => Assert.Equal(DesignConversionDpoUnits.ImproverUnit, candidate.UnitId));
            Assert.All(dpoCandidates, candidate =>
            {
                Assert.Equal(bool.FalseString, candidate.TrainingRecord.Provenance.Metadata["training_grade"]);
                Assert.False(candidate.TrainingRecord.Provenance.Metadata.ContainsKey("runtime_teacher_selected_preference"));
                Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["runtime_teacher_authored_candidate"]);
                Assert.Equal(bool.TrueString, candidate.TrainingRecord.Provenance.Metadata["independent_preference_proven"]);
                Assert.Equal(bool.FalseString, candidate.TrainingRecord.Provenance.Metadata["raw_teacher_prompt_persisted"]);
                Assert.Equal(bool.FalseString, candidate.TrainingRecord.Provenance.Metadata["raw_teacher_output_persisted"]);
                Assert.Equal(bool.FalseString, candidate.TrainingRecord.Provenance.Metadata["raw_teacher_rationale_persisted"]);
                Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, candidate.TrainingRecord.Provenance.Metadata["review_status"]);
                Assert.Equal(bool.FalseString, candidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
            });
            Assert.Equal(
                2,
                dpoCandidates.Select(candidate => candidate.TrainingRecord.Provenance.Metadata[
                        DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey])
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            var dpoCandidate = dpoCandidates[0];
            var teacherProvenance = dpoCandidate.TrainingRecord.TeacherProvenance;
            Assert.NotNull(teacherProvenance);
            Assert.Equal(TrainingTeacherSourceKind.ExternalTeacherOutput, teacherProvenance!.SourceKind);
            Assert.NotEqual(TrainingTeacherSourceKind.DeterministicNoExternalTeacher, teacherProvenance.SourceKind);
            Assert.Equal(OpenWeightTeacherFamily.Qwen, teacherProvenance.Family);
            Assert.Equal(TrainingTeacherAccessMode.LocalHostedOpenWeight, teacherProvenance.AccessMode);
            Assert.Equal(TrainingTeacherHostingProvider.Ollama, teacherProvenance.HostingProvider);
            Assert.Equal(ExpectedTeacherModelId, teacherProvenance.ModelId);
            Assert.Equal(ExpectedTeacherManifestDigest, teacherProvenance.ModelVersion);
            Assert.Equal(TrainingTeacherModelLicense.Apache20, teacherProvenance.ModelLicense);
            Assert.False(teacherProvenance.ClosedProprietaryModel);
            Assert.True(teacherProvenance.TeacherOutputUsedForTraining);
            Assert.Equal("teacher-test-authoring-1", teacherProvenance.RequestId);
            Assert.Equal(1, teacherProvenance.InvocationCount);
            Assert.Equal(
                DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                dpoCandidate.TrainingRecord.Provenance.Metadata["teacher_authoring_schema"]);
            var teacherAuthoringOutputHash = dpoCandidate.TrainingRecord.Provenance.Metadata[
                "teacher_authoring_output_hash"];
            Assert.Equal(64, teacherAuthoringOutputHash.Length);
            Assert.All(teacherAuthoringOutputHash, character => Assert.True(
                character is >= '0' and <= '9' or >= 'a' and <= 'f'));
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, dpoCandidate.TrainingRecord.Provenance.Metadata["review_status"]);
            Assert.Equal(bool.FalseString, dpoCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
            Assert.Equal(bool.TrueString, dpoCandidate.TrainingRecord.Provenance.Metadata["source_disjoint_not_proven"]);
            Assert.StartsWith("sha256:", dpoCandidate.TrainingRecord.Provenance.SourceManifestRef, StringComparison.Ordinal);
            Assert.All(dpoCandidates, candidate => Assert.Equal(
                dpoCandidate.TrainingRecord.Provenance.SourceManifestRef,
                candidate.TrainingRecord.Provenance.SourceManifestRef));
            Assert.True(dpoCandidate.TrainingCandidateOnly);
            Assert.True(dpoCandidate.ProductNotReady);
            var chosenComparisonRequests = comparisonProvider.Requests
                .Where(request => request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal))
                .ToArray();
            var rejectedComparisonRequests = comparisonProvider.Requests
                .Where(request => !request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal))
                .ToArray();
            Assert.Equal(2, chosenComparisonRequests.Length);
            Assert.Equal(2, rejectedComparisonRequests.Length);
            AssertDerivedRuntimeQaRecords(
                dpoCandidate.TrainingRecord.Chosen.QaResults,
                chosenComparisonRequests,
                "chosen",
                expectedScore: 0.95,
                expectedPassed: true,
                expectedVisualDistance: 0.05);
            AssertDerivedRuntimeQaRecords(
                dpoCandidate.TrainingRecord.Rejected.QaResults,
                rejectedComparisonRequests,
                "rejected",
                expectedScore: 0.72,
                expectedPassed: false,
                expectedVisualDistance: 0.28);
            var serializedDpoQa = JsonSerializer.Serialize(new
            {
                ChosenQaResults = dpoCandidate.TrainingRecord.Chosen.QaResults,
                RejectedQaResults = dpoCandidate.TrainingRecord.Rejected.QaResults
            });
            Assert.DoesNotContain(privateMarker, serializedDpoQa, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.Html!, serializedDpoQa, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.Css!, serializedDpoQa, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.DesignTree.Payload!, serializedDpoQa, StringComparison.Ordinal);

            var usefulnessBeforeApproval = await usefulnessApi.QueryAsync(
                new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
            Assert.True(usefulnessBeforeApproval.IsSuccess, usefulnessBeforeApproval.ErrorMessage);
            Assert.Equal(0, usefulnessBeforeApproval.Value!.TrainReadyCount);
            Assert.Equal(2, usefulnessBeforeApproval.Value.Rows.Count);
            Assert.All(usefulnessBeforeApproval.Value.Rows, rowBeforeApproval =>
            {
                Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalPendingStatus, rowBeforeApproval.ApprovalStatus);
                Assert.False(rowBeforeApproval.TrainReady);
                Assert.True(rowBeforeApproval.ProductNotReady);
                Assert.False(rowBeforeApproval.ModelReadinessClaimed);
            });

            var displayBeforeApproval = await artifactDb.QueryDisplayRowsAsync(
                new DesignConversionDpoDisplayQuery(IncludeBlocked: true));
            Assert.True(displayBeforeApproval.IsSuccess, displayBeforeApproval.ErrorMessage);
            Assert.Equal(2, displayBeforeApproval.Value!.Rows.Count);
            Assert.All(displayBeforeApproval.Value.Rows, displayRowBeforeApproval =>
            {
                Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, displayRowBeforeApproval.BoundaryMetadata["review_status"]);
                Assert.Equal(bool.FalseString, displayRowBeforeApproval.BoundaryMetadata["approved_for_training"]);
            });

            var readbackBeforeApproval = await usefulnessApi.QueryLocalArtifactReadbackAsync(
                new DesignConversionLocalArtifactReadbackApiRequest(JobId: stored.JobId));
            Assert.True(readbackBeforeApproval.IsSuccess, readbackBeforeApproval.ErrorMessage);
            Assert.Equal(2, readbackBeforeApproval.Value!.DpoCandidateCount);
            var readbackRow = Assert.Single(readbackBeforeApproval.Value.Rows);
            Assert.Equal(2, readbackRow.RenderArtifactRefs.Count);
            Assert.Equal(
                ["source-1440x900", "source-390x844"],
                readbackRow.RenderArtifactRefs.Select(artifact => artifact.ViewportId).Order(StringComparer.Ordinal).ToArray());
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, readbackRow.VisualJudgeStatus);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ConfiguredOrLocalLabelerBoundary, readbackRow.VisualJudgeProviderBoundary);
            Assert.False(readbackRow.ExternalVisualAiCallExecuted);
            Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalPendingStatus, readbackRow.DpoApprovalStatus);
            Assert.True(readbackRow.ProductNotReady);
            Assert.False(readbackRow.ProductionDpo);
            Assert.False(readbackRow.ModelReadinessClaimed);
            Assert.Equal(DesignConversionDpoUsefulnessApi.ProductNotReadyBoundary, readbackRow.ReadinessBoundary);

            foreach (var candidate in dpoCandidates)
            {
                var approval = await usefulnessApi.ApproveRuntimeCreatedDpoCandidateAsync(
                    stored.RecordId,
                    candidate.CandidateId,
                    "reviewer:pass163-local",
                    "same-scenario package/API local chain review; approval may create intake but not bypass source-disjoint gate",
                    DateTimeOffset.UnixEpoch.AddMinutes(163));
                Assert.True(approval.IsSuccess, approval.ErrorMessage);
                Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus, approval.Value!.ApprovalStatus);
                Assert.True(approval.Value.ApprovedForTraining);
                Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["model_readiness_claimed"]);
            }

            var recordsAfterApproval = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(
                    LocalArtifactRecordId: stored.RecordId));
            Assert.True(recordsAfterApproval.IsSuccess, recordsAfterApproval.ErrorMessage);
            var storedAfterApproval = Assert.Single(recordsAfterApproval.Value!);
            Assert.Equal(
                attemptHistoryBeforeApproval,
                storedAfterApproval.Metadata[
                    DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryMetadataKey]);
            Assert.True(storedAfterApproval.TryGetImprovementLoopAttemptHistory(
                out var storedHistoryAfterApproval));
            Assert.Equal(
                storedHistory.HistorySha256,
                storedHistoryAfterApproval!.HistorySha256);

            var usefulnessAfterApproval = await usefulnessApi.QueryAsync(
                new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
            Assert.True(usefulnessAfterApproval.IsSuccess, usefulnessAfterApproval.ErrorMessage);
            Assert.Equal(2, usefulnessAfterApproval.Value!.TrainReadyCount);
            Assert.Equal(2, usefulnessAfterApproval.Value.Rows.Count);
            Assert.All(usefulnessAfterApproval.Value.Rows, rowAfterApproval =>
            {
                Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus, rowAfterApproval.ApprovalStatus);
                Assert.True(rowAfterApproval.TrainReady);
                Assert.True(rowAfterApproval.ProductNotReady);
                Assert.False(rowAfterApproval.ModelReadinessClaimed);
            });

            var handoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId),
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(2, handoff.Value!.ApprovedCandidateCount);
            Assert.Equal(2, handoff.Value.Rows.Count);
            Assert.All(handoff.Value.Rows, handoffRow =>
            {
                Assert.Equal(dpoCandidate.TrainingRecord.Provenance.SourceManifestRef, handoffRow.SourceManifestRef);
                Assert.True(handoffRow.TrainingHandoffOnly);
            });

            var intake = await usefulnessApi.CreateTrainingIntakeAsync(
                handoff.Value,
                "architect:pass163-local",
                "Pass163 same-scenario retrain request from local artifact DB; source-disjoint gate must decide runner eligibility",
                DateTimeOffset.UnixEpoch.AddMinutes(165));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);
            Assert.Equal(2, intake.Value!.AcceptedCandidateCount);
            Assert.True(intake.Value.TrainingIntakeOnly);
            Assert.True(intake.Value.ModelUploadNotReady);
            Assert.True(intake.Value.ProductNotReady);

            var preflight = await artifactDb.RunTrainingIntakePreflightAsync(intake.Value.IntakeId);
            Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
            Assert.False(preflight.Value!.MayStartTraining);
            Assert.Equal("proven", preflight.Value.SourceManifestStatus);
            Assert.Equal("blocked_same_source", preflight.Value.SourceDisjointStatus);
            Assert.False(preflight.Value.ModelUploadReady);
            Assert.Contains("source_disjoint_blocked_same_source", preflight.Value.BlockReasons);
            Assert.Equal("2", preflight.Value.BoundaryMetadata["source_manifest_candidate_count"]);
            Assert.Equal("0", preflight.Value.BoundaryMetadata["source_disjoint_train_source_count"]);
            Assert.Equal("1", preflight.Value.BoundaryMetadata["source_disjoint_heldout_source_count"]);

            var promotion = await DesignConversionPromotionAdmissionTestScope.RunAsync(
                () => usefulnessApi.RunTrainingIntakeAndPromoteLocalAsync(
                    intake.Value.IntakeId,
                    Path.Combine(root, "blocked-local-promotion"),
                    CreatePass163BlockedPromotionOptions()));
            Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
            Assert.False(promotion.Value!.TrainingRun.MayStartTraining);
            Assert.False(promotion.Value.TrainingRun.TrainingRunnerInvoked);
            Assert.False(promotion.Value.TrainingRun.TrainingStarted);
            Assert.False(promotion.Value.TrainingRun.CheckpointMutated);
            Assert.False(promotion.Value.TrainingRun.WeightsProduced);
            Assert.False(promotion.Value.TrainingRun.ModelUploadReady);
            Assert.Equal(0, Volatile.Read(ref runnerInvocationCount));
            Assert.Equal("blocked_same_source", promotion.Value.TrainingRun.SourceDisjointStatus);
            Assert.Equal("blocked_before_runner_by_training_intake_preflight", promotion.Value.TrainingRun.TrainingExecutionBoundary);
            Assert.Contains("source_disjoint_blocked_same_source", promotion.Value.TrainingRun.BlockReasons);
            Assert.False(promotion.Value.LocalPromotionReady);
            Assert.False(promotion.Value.ExternalCloudUploadAttempted);
            Assert.Null(promotion.Value.Publication);
            Assert.Null(promotion.Value.PromotedLocalModel);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["local_promotion_ready"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["external_cloud_upload_attempted"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["external_cloud_upload_not_attempted"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["cloud_model_upload_not_attempted"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["model_server_upload_not_attempted_local_only"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["model_server_promotion_not_claimed_local_only"]);

            var persistedJson = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));
            var serializedUsefulness = JsonSerializer.Serialize(usefulnessAfterApproval.Value);
            var serializedReadback = JsonSerializer.Serialize(readbackBeforeApproval.Value);
            var serializedIntake = JsonSerializer.Serialize(intake.Value);
            var serializedPromotion = JsonSerializer.Serialize(promotion.Value);
            foreach (var payload in new[] { persistedJson, serializedUsefulness, serializedReadback, serializedIntake, serializedPromotion })
            {
                Assert.DoesNotContain(privateMarker, payload, StringComparison.Ordinal);
                Assert.DoesNotContain(package.InitialPackage.Html!, payload, StringComparison.Ordinal);
                Assert.DoesNotContain(package.InitialPackage.Css!, payload, StringComparison.Ordinal);
                Assert.DoesNotContain(package.InitialPackage.DesignTree.Payload!, payload, StringComparison.Ordinal);
                Assert.DoesNotContain("C:/Users/lubao", payload, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("external-provider-payload", payload, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        static void AssertDerivedRuntimeQaRecords(
            IReadOnlyList<RenderQaResultRecord> qaResults,
            IReadOnlyList<DesignConversionQaComparisonRequest> comparisonRequests,
            string label,
            double expectedScore,
            bool expectedPassed,
            double expectedVisualDistance)
        {
            var expectedWarnings = new[]
            {
                "DETERMINISTIC_RENDER_SCAFFOLD_DOES_NOT_CLAIM_VISUAL_FIDELITY",
                "DETERMINISTIC_RENDER_SCAFFOLD_NOT_BROWSER_SCREENSHOT",
                "SERVER_OWNED_SCAFFOLD_RENDER_PROVENANCE_CANONICALIZED"
            };
            Assert.Equal(comparisonRequests.Count, qaResults.Count);
            foreach (var request in comparisonRequests)
            {
                var expectedImageId = DesignConversionDpoLeakageGuard.HashRef(
                    $"dcm-{label}_expected-image",
                    request.Expected.ImageId);
                var actualImageId = DesignConversionDpoLeakageGuard.HashRef(
                    $"dcm-{label}_actual-image",
                    request.Actual.ImageId);
                var result = Assert.Single(qaResults, candidate =>
                    string.Equals(candidate.Expected.ImageId, expectedImageId, StringComparison.Ordinal) &&
                    string.Equals(candidate.Actual.ImageId, actualImageId, StringComparison.Ordinal));
                Assert.StartsWith($"dcm-{label}-qa-result-", result.ResultId, StringComparison.Ordinal);
                Assert.Equal(
                    DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}_expected-uri", request.Expected.Uri),
                    result.Expected.Uri);
                Assert.Equal(
                    DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}_actual-uri", request.Actual.Uri),
                    result.Actual.Uri);
                Assert.StartsWith($"dcm-{label}_qa-verdict-", result.Verdict.VerdictId, StringComparison.Ordinal);
                Assert.Equal(expectedScore, result.Verdict.Score);
                Assert.Equal(expectedPassed, result.Verdict.Passed);
                Assert.Equal(expectedVisualDistance, result.Metrics["visual_distance"], precision: 12);
                Assert.Equal(expectedWarnings.Length, result.Warnings.Count);
                Assert.Equal(expectedWarnings, result.Warnings.Order(StringComparer.Ordinal).ToArray());
            }
        }
    }

    [Fact]
    public async Task Pass164_SourceDisjointTeacherAuthoredCandidatesRequireCorpusAdmissionBeforeTraining()
    {
        const string privateMarker = "PRIVATE_PASS164_SOURCE_DISJOINT_SECRET";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "pass164-source-disjoint-loop-training");
        Directory.CreateDirectory(root);
        try
        {
            var runnerInvocationCount = 0;
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                    Interlocked.Increment(ref runnerInvocationCount),
                legalTeacherAdmissionAuthority: DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(
                    modelId: ExpectedTeacherModelId,
                    expectedOllamaManifestDigest: ExpectedTeacherManifestDigest,
                    expectedOllamaModelLayerDigest: ExpectedTeacherModelLayerDigest));
            using var teacherAssociation =
                DesignConversionDpoTeacherTestAssociation.Associate(
                    artifactDb,
                    supportTeacherAuthoring: true,
                    supportIndependentPreferenceEvidence: true);
            var usefulnessApi = new DesignConversionDpoUsefulnessApi(artifactDb);
            var renderProvider = new DeterministicDesignConversionRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(request =>
                request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.72);
            var visualJudge = new ScriptedVisualJudge(request =>
                request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                    ? JudgeScript.FromScore(0.96, 0m)
                    : JudgeScript.FromScore(0.72, 0m));
            var strategy = new ScriptedCandidateStrategy(
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreatePass164SourceCandidate("pass164-source-a", privateMarker)),
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreatePass164SourceCandidate("pass164-source-b", privateMarker)));
            var adapter = CreateAdapter(
                renderProvider,
                comparisonProvider,
                visualJudge,
                strategy,
                artifactDb);

            var firstPackage = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "pass164-source-a-loop",
                InitialPackage: CreatePass164Package("pass164-source-a-request", "pass164-source-a", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["pass164_source_disjoint_probe"] = bool.TrueString,
                    ["source_family"] = "pass164-source-a",
                    ["local_only"] = bool.TrueString,
                    ["no_external_provider"] = bool.TrueString
                });

            var firstEnvelope = await adapter.TryRunImprovementLoopAsync(firstPackage);
            AssertStableImprovementLoopEnvelope(
                firstEnvelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal(bool.FalseString, firstEnvelope.BoundaryMetadata["external_provider_called"]);
            Assert.Equal(bool.FalseString, firstEnvelope.BoundaryMetadata["product_ready"]);
            AssertPass164TwoAttemptEnvelope(firstEnvelope, "pass164-source-a-loop", "pass164-source-a-request");
            Assert.Equal(4, comparisonProvider.Requests.Count);
            Assert.Equal(4, visualJudge.Requests.Count);
            AssertPass164AttemptProviderShape(
                comparisonProvider.Requests,
                visualJudge.Requests,
                "pass164-source-a-loop",
                "pass164-source-a-request",
                attemptNumber: 1);
            AssertPass164AttemptProviderShape(
                comparisonProvider.Requests,
                visualJudge.Requests,
                "pass164-source-a-loop",
                "pass164-source-a-request",
                attemptNumber: 2);
            Assert.Empty(strategy.Requests);

            var firstRecords = await artifactDb.ListRecordsAsync();
            Assert.True(firstRecords.IsSuccess, firstRecords.ErrorMessage);
            var firstStored = Assert.Single(firstRecords.Value!);
            AssertPass164DeterministicRenderBoundary(firstStored.RenderArtifactRefs);
            var firstCandidates = firstStored.DpoCandidates.ToArray();
            Assert.Collection(
                firstCandidates,
                candidate => Assert.Equal(DesignConversionDpoUnits.CriticUnit, candidate.UnitId),
                candidate => Assert.Equal(DesignConversionDpoUnits.ImproverUnit, candidate.UnitId));
            Assert.All(firstCandidates, AssertPass164ReviewOnlyTeacherCandidate);
            Assert.Equal(2, firstCandidates.Select(candidate =>
                    candidate.TrainingRecord.Provenance.Metadata[
                        DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey])
                .Distinct(StringComparer.Ordinal)
                .Count());
            var firstCandidate = firstCandidates[0];
            Assert.StartsWith("sha256:", firstCandidate.TrainingRecord.Provenance.SourceManifestRef, StringComparison.Ordinal);
            Assert.All(firstCandidates, candidate => Assert.Equal(
                firstCandidate.TrainingRecord.Provenance.SourceManifestRef,
                candidate.TrainingRecord.Provenance.SourceManifestRef));
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, firstCandidate.TrainingRecord.Provenance.Metadata["review_status"]);
            Assert.Equal(bool.FalseString, firstCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
            AssertRuntimeQualityCreatedDpoPair(firstCandidate.TrainingRecord);
            AssertRuntimeQualityScores(firstCandidate.TrainingRecord);
            AssertRuntimeTeacherAttestedCandidate(firstCandidate);

            foreach (var candidate in firstCandidates)
            {
                var firstApproval = await usefulnessApi.ApproveRuntimeCreatedDpoCandidateAsync(
                    firstStored.RecordId,
                    candidate.CandidateId,
                    "reviewer:pass164-local",
                    "Pass164 first runtime candidate approval; single-source intake must remain blocked",
                    DateTimeOffset.UnixEpoch.AddMinutes(164));
                Assert.True(firstApproval.IsSuccess, firstApproval.ErrorMessage);
            }

            var singleSourceHandoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: firstStored.JobId),
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(singleSourceHandoff.IsSuccess, singleSourceHandoff.ErrorMessage);
            Assert.Equal(2, singleSourceHandoff.Value!.ApprovedCandidateCount);
            Assert.Equal(2, singleSourceHandoff.Value.Rows.Count);
            Assert.All(singleSourceHandoff.Value.Rows, singleSourceRow =>
            {
                Assert.Equal(firstCandidate.TrainingRecord.Provenance.SourceManifestRef, singleSourceRow.SourceManifestRef);
                Assert.True(singleSourceRow.TrainingHandoffOnly);
            });

            var singleSourceIntake = await usefulnessApi.CreateTrainingIntakeAsync(
                singleSourceHandoff.Value,
                "architect:pass164-local",
                "Pass164 negative control: one loop-created runtime source must not start training",
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(singleSourceIntake.IsSuccess, singleSourceIntake.ErrorMessage);
            var singleSourcePreflight = await usefulnessApi.RunTrainingIntakePreflightAsync(singleSourceIntake.Value!.IntakeId);
            Assert.True(singleSourcePreflight.IsSuccess, singleSourcePreflight.ErrorMessage);
            Assert.False(singleSourcePreflight.Value!.MayStartTraining);
            Assert.Equal("proven", singleSourcePreflight.Value.SourceManifestStatus);
            Assert.Equal("blocked_same_source", singleSourcePreflight.Value.SourceDisjointStatus);
            Assert.Contains("source_disjoint_blocked_same_source", singleSourcePreflight.Value.BlockReasons);
            Assert.Equal("2", singleSourcePreflight.Value.BoundaryMetadata["source_manifest_candidate_count"]);
            Assert.Equal("0", singleSourcePreflight.Value.BoundaryMetadata["source_disjoint_train_source_count"]);
            Assert.Equal("1", singleSourcePreflight.Value.BoundaryMetadata["source_disjoint_heldout_source_count"]);
            Assert.Equal(bool.FalseString, singleSourcePreflight.Value.BoundaryMetadata["model_upload_ready"]);
            Assert.Equal(bool.FalseString, singleSourcePreflight.Value.BoundaryMetadata["product_ready"]);

            var secondPackage = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "pass164-source-b-loop",
                InitialPackage: CreatePass164Package("pass164-source-b-request", "pass164-source-b", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["pass164_source_disjoint_probe"] = bool.TrueString,
                    ["source_family"] = "pass164-source-b",
                    ["local_only"] = bool.TrueString,
                    ["no_external_provider"] = bool.TrueString
                });

            var secondEnvelope = await adapter.TryRunImprovementLoopAsync(secondPackage);
            AssertStableImprovementLoopEnvelope(
                secondEnvelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal(bool.FalseString, secondEnvelope.BoundaryMetadata["external_provider_called"]);
            Assert.Equal(bool.FalseString, secondEnvelope.BoundaryMetadata["product_ready"]);
            AssertPass164TwoAttemptEnvelope(secondEnvelope, "pass164-source-b-loop", "pass164-source-b-request");
            Assert.Equal(8, comparisonProvider.Requests.Count);
            Assert.Equal(8, visualJudge.Requests.Count);
            Assert.Empty(strategy.Requests);
            AssertPass164AttemptProviderShape(
                comparisonProvider.Requests,
                visualJudge.Requests,
                "pass164-source-b-loop",
                "pass164-source-b-request",
                attemptNumber: 1);
            AssertPass164AttemptProviderShape(
                comparisonProvider.Requests,
                visualJudge.Requests,
                "pass164-source-b-loop",
                "pass164-source-b-request",
                attemptNumber: 2);

            var secondRecords = await artifactDb.ListRecordsAsync();
            Assert.True(secondRecords.IsSuccess, secondRecords.ErrorMessage);
            Assert.Equal(2, secondRecords.Value!.Count);
            var secondStored = Assert.Single(
                secondRecords.Value!,
                record => string.Equals(
                    record.JobId,
                    "modelserver-improvement-loop:pass164-source-b-loop:pass164-source-b-request:loop-attempt:2",
                    StringComparison.Ordinal));
            AssertPass164DeterministicRenderBoundary(secondStored.RenderArtifactRefs);
            Assert.True(
                secondStored.DpoCandidates.Count > 0,
                secondStored.Metadata.TryGetValue("runtime_created_dpo_candidate_warnings", out var warnings)
                    ? warnings
                    : JsonSerializer.Serialize(secondStored.Metadata));
            var secondCandidates = secondStored.DpoCandidates.ToArray();
            Assert.Collection(
                secondCandidates,
                candidate => Assert.Equal(DesignConversionDpoUnits.CriticUnit, candidate.UnitId),
                candidate => Assert.Equal(DesignConversionDpoUnits.ImproverUnit, candidate.UnitId));
            Assert.All(secondCandidates, AssertPass164ReviewOnlyTeacherCandidate);
            Assert.Equal(4, firstCandidates.Concat(secondCandidates).Select(candidate =>
                    candidate.TrainingRecord.Provenance.Metadata[
                        DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey])
                .Distinct(StringComparer.Ordinal)
                .Count());
            var secondCandidate = secondCandidates[0];
            Assert.StartsWith("sha256:", secondCandidate.TrainingRecord.Provenance.SourceManifestRef, StringComparison.Ordinal);
            Assert.All(secondCandidates, candidate => Assert.Equal(
                secondCandidate.TrainingRecord.Provenance.SourceManifestRef,
                candidate.TrainingRecord.Provenance.SourceManifestRef));
            Assert.NotEqual(
                firstCandidate.TrainingRecord.Provenance.SourceManifestRef,
                secondCandidate.TrainingRecord.Provenance.SourceManifestRef);
            AssertRuntimeQualityCreatedDpoPair(secondCandidate.TrainingRecord);
            AssertRuntimeQualityScores(secondCandidate.TrainingRecord);
            AssertRuntimeTeacherAttestedCandidate(secondCandidate);

            foreach (var candidate in secondCandidates)
            {
                var secondApproval = await usefulnessApi.ApproveRuntimeCreatedDpoCandidateAsync(
                    secondStored.RecordId,
                    candidate.CandidateId,
                    "reviewer:pass164-local",
                    "Pass164 second runtime candidate approval; source-disjoint intake may reach corpus admission boundary",
                    DateTimeOffset.UnixEpoch.AddMinutes(165));
                Assert.True(secondApproval.IsSuccess, secondApproval.ErrorMessage);
            }

            var sourceDisjointHandoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(),
                DateTimeOffset.UnixEpoch.AddMinutes(165));
            Assert.True(sourceDisjointHandoff.IsSuccess, sourceDisjointHandoff.ErrorMessage);
            Assert.Equal(4, sourceDisjointHandoff.Value!.ApprovedCandidateCount);
            Assert.Equal(4, sourceDisjointHandoff.Value.Rows.Count);
            Assert.Equal(
                2,
                sourceDisjointHandoff.Value.Rows
                    .Select(row => row.SourceManifestRef)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.Equal(
                2,
                sourceDisjointHandoff.Value.Rows
                    .Select(row => row.SourceStructureHash)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.All(sourceDisjointHandoff.Value.Rows, row =>
            {
                Assert.True(row.TrainingHandoffOnly);
                Assert.Equal(bool.TrueString, row.BoundaryMetadata["runtime_created_dpo_candidate"]);
                Assert.Equal(bool.FalseString, row.BoundaryMetadata["production_dpo"]);
                Assert.Equal(bool.TrueString, row.BoundaryMetadata["product_not_ready"]);
                Assert.Equal(bool.FalseString, row.BoundaryMetadata["model_readiness_claimed"]);
            });

            var sourceDisjointIntake = await usefulnessApi.CreateTrainingIntakeAsync(
                sourceDisjointHandoff.Value,
                "architect:pass164-local",
                "Pass164 source-disjoint runtime candidates from two source manifests may cross preflight only",
                DateTimeOffset.UnixEpoch.AddMinutes(165));
            Assert.True(sourceDisjointIntake.IsSuccess, sourceDisjointIntake.ErrorMessage);
            Assert.Equal(4, sourceDisjointIntake.Value!.AcceptedCandidateCount);
            Assert.Equal(
                2,
                sourceDisjointIntake.Value.CandidateRefs
                    .Select(candidate => candidate.SourceIdentityHash)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.True(sourceDisjointIntake.Value.ProductNotReady);
            Assert.Equal(bool.FalseString, sourceDisjointIntake.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal("proven", sourceDisjointIntake.Value.BoundaryMetadata["source_manifest_status"]);
            Assert.Equal("proven", sourceDisjointIntake.Value.BoundaryMetadata["source_disjoint_status"]);

            var sourceDisjointPreflight = await usefulnessApi.RunTrainingIntakePreflightAsync(sourceDisjointIntake.Value.IntakeId);
            Assert.True(sourceDisjointPreflight.IsSuccess, sourceDisjointPreflight.ErrorMessage);
            Assert.True(sourceDisjointPreflight.Value!.MayStartTraining);
            Assert.Equal("proven", sourceDisjointPreflight.Value.SourceManifestStatus);
            Assert.Equal("proven", sourceDisjointPreflight.Value.SourceDisjointStatus);
            Assert.DoesNotContain("source_disjoint_blocked_same_source", sourceDisjointPreflight.Value.BlockReasons);
            Assert.Empty(sourceDisjointPreflight.Value.BlockReasons);
            Assert.Equal("4", sourceDisjointPreflight.Value.BoundaryMetadata["source_manifest_candidate_count"]);
            Assert.Equal("2", sourceDisjointPreflight.Value.BoundaryMetadata["source_manifest_hash_count"]);
            Assert.Equal("1", sourceDisjointPreflight.Value.BoundaryMetadata["source_disjoint_train_source_count"]);
            Assert.Equal("1", sourceDisjointPreflight.Value.BoundaryMetadata["source_disjoint_heldout_source_count"]);
            Assert.Equal("0", sourceDisjointPreflight.Value.BoundaryMetadata["source_disjoint_overlap_count"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["training_started"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["weights_produced"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["model_upload_ready"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["product_ready"]);

            var machineMutationCount = 0;
            var previousMutationObserver = DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting;
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                _ => Interlocked.Increment(ref machineMutationCount);
            OperationResult<DesignConversionTrainingIntakeRunResult> sourceDisjointRun;
            try
            {
                sourceDisjointRun = await usefulnessApi.RunTrainingIntakeAsync(
                    sourceDisjointIntake.Value.IntakeId);
            }
            finally
            {
                DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                    previousMutationObserver;
            }

            Assert.False(sourceDisjointRun.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode,
                sourceDisjointRun.ErrorCode);
            Assert.Null(sourceDisjointRun.Value);
            Assert.Equal(0, Volatile.Read(ref runnerInvocationCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.Empty(Directory.EnumerateFiles(root, "*checkpoint*", SearchOption.AllDirectories));
            Assert.Empty(Directory.EnumerateFiles(root, "*promotion*", SearchOption.AllDirectories));

            var serializedRun = JsonSerializer.Serialize(sourceDisjointRun);
            Assert.DoesNotContain(privateMarker, serializedRun, StringComparison.Ordinal);
            Assert.DoesNotContain(firstPackage.InitialPackage.Html!, serializedRun, StringComparison.Ordinal);
            Assert.DoesNotContain(secondPackage.InitialPackage.Html!, serializedRun, StringComparison.Ordinal);
            Assert.DoesNotContain(firstPackage.InitialPackage.DesignTree.Payload!, serializedRun, StringComparison.Ordinal);
            Assert.DoesNotContain(secondPackage.InitialPackage.DesignTree.Payload!, serializedRun, StringComparison.Ordinal);
            Assert.DoesNotContain("external-provider-payload", serializedRun, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        static DesignConversionImprovementCandidate CreatePass164SourceCandidate(
            string sourceSlug,
            string privateMarker)
            => new(
                CandidateId: $"{sourceSlug}-runtime-improved-candidate",
                RequestId: $"{sourceSlug}-runtime-improved-request",
                Html: $"<main class=\"{sourceSlug}-runtime-improved\" data-secret=\"{privateMarker}\">Pass164 improved {sourceSlug}</main>",
                Css: $".{sourceSlug}-runtime-improved{{display:grid;gap:16px;padding:20px;background:url('{privateMarker}');}}",
                ChangeReasons: [$"scripted source-specific local improvement for {sourceSlug}"],
                EvidenceRefs: [$"candidate:{sourceSlug}:runtime-improved"],
                Metadata: new Dictionary<string, string>
                {
                    ["source_family"] = sourceSlug,
                    ["private_marker"] = privateMarker
                });

        static void AssertPass164TwoAttemptEnvelope(
            DesignConversionModelServerLocalCallerAdapterResponse envelope,
            string expectedLoopId,
            string expectedRequestId)
        {
            Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
            Assert.Equal(expectedRequestId, envelope.ParsedRequestId);
            using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
            var responseRoot = document.RootElement;
            Assert.Equal(expectedLoopId, responseRoot.GetProperty("loopId").GetString());
            Assert.Equal(expectedRequestId, responseRoot.GetProperty("requestId").GetString());
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, responseRoot.GetProperty("stopReason").GetString());
            Assert.True(responseRoot.GetProperty("completed").GetBoolean());
            Assert.True(responseRoot.GetProperty("qualityReached").GetBoolean());
            Assert.Equal(2, responseRoot.GetProperty("attemptCount").GetInt32());
            var attempts = responseRoot.GetProperty("attempts").EnumerateArray().ToArray();
            Assert.Equal(2, attempts.Length);
            Assert.Equal(1, attempts[0].GetProperty("attemptNumber").GetInt32());
            Assert.Equal(2, attempts[1].GetProperty("attemptNumber").GetInt32());
        }

        static void AssertPass164AttemptProviderShape(
            IReadOnlyList<DesignConversionQaComparisonRequest> comparisonRequests,
            IReadOnlyList<DesignConversionVisualJudgeRequest> visualJudgeRequests,
            string loopId,
            string requestId,
            int attemptNumber)
        {
            var rootJobId = $"modelserver-improvement-loop:{loopId}:{requestId}";
            var jobId = attemptNumber == 1
                ? rootJobId
                : $"{rootJobId}:loop-attempt:{attemptNumber}";
            var attemptComparisonRequests = comparisonRequests
                .Where(request => string.Equals(request.Metadata["job_id"], jobId, StringComparison.Ordinal))
                .ToArray();
            var attemptVisualJudgeRequests = visualJudgeRequests
                .Where(request => string.Equals(request.Metadata["job_id"], jobId, StringComparison.Ordinal))
                .ToArray();
            Assert.Equal(2, attemptComparisonRequests.Length);
            Assert.Equal(2, attemptVisualJudgeRequests.Length);
            Assert.All(attemptComparisonRequests, request =>
                Assert.StartsWith($"{jobId}:comparison-request:", request.ComparisonRequestId, StringComparison.Ordinal));
            Assert.All(attemptVisualJudgeRequests, request =>
                Assert.StartsWith($"{jobId}:visual-judge-request:", request.JudgeRequestId, StringComparison.Ordinal));
        }

        static void AssertPass164DeterministicRenderBoundary(
            IReadOnlyList<DesignConversionRenderArtifactDbRef> renderArtifactRefs)
        {
            Assert.Equal(2, renderArtifactRefs.Count);
            Assert.Equal(
                ["source-1440x900", "source-390x844"],
                renderArtifactRefs.Select(artifact => artifact.ViewportId).Order(StringComparer.Ordinal).ToArray());
            Assert.All(renderArtifactRefs, artifact =>
            {
                Assert.StartsWith("artifact://design-conversion/render-scaffold/", artifact.UriRef, StringComparison.Ordinal);
                Assert.StartsWith("sha256:", artifact.Sha256, StringComparison.Ordinal);
                Assert.Equal(DeterministicDesignConversionRenderProvider.ProviderKind, artifact.Metadata["provider_kind"]);
                Assert.Equal(
                    DesignConversionRenderEvidenceTrustBoundary.DeterministicOrScaffoldClassification,
                    artifact.Metadata[DesignConversionRenderEvidenceTrustBoundary.TrustClassificationKey]);
                Assert.Equal(bool.FalseString, artifact.Metadata["visual_fidelity_claimed"]);
                Assert.Equal(bool.FalseString, artifact.Metadata["browser_or_playwright_used"]);
                Assert.Equal(bool.FalseString, artifact.Metadata[DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey]);
                Assert.Equal(bool.FalseString, artifact.Metadata[DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey]);
                Assert.Equal(bool.TrueString, artifact.Metadata["scaffold_render_evidence_server_canonicalized"]);
                Assert.Equal("metadata_only_render_evidence", artifact.Metadata["hidden_determinism_boundary"]);
                Assert.False(artifact.Metadata.ContainsKey("raw_html_included"));
                Assert.False(artifact.Metadata.ContainsKey("raw_css_included"));
            });
        }

        static void AssertRuntimeQualityScores(DesignConversionDpoTrainingRecord record)
        {
            var expectedWarnings = new[]
            {
                "DETERMINISTIC_RENDER_SCAFFOLD_DOES_NOT_CLAIM_VISUAL_FIDELITY",
                "DETERMINISTIC_RENDER_SCAFFOLD_NOT_BROWSER_SCREENSHOT",
                "SERVER_OWNED_SCAFFOLD_RENDER_PROVENANCE_CANONICALIZED"
            };
            Assert.Equal(0.96, record.Chosen.Verdict.Score);
            Assert.Equal(0.72, record.Rejected.Verdict.Score);
            Assert.Equal(2, record.Chosen.QaResults.Count);
            Assert.Equal(2, record.Rejected.QaResults.Count);
            Assert.All(record.Chosen.QaResults, result =>
            {
                Assert.Equal(0.96, result.Verdict.Score);
                Assert.True(result.Verdict.Passed);
                Assert.Equal(expectedWarnings, result.Warnings.Order(StringComparer.Ordinal).ToArray());
            });
            Assert.All(record.Rejected.QaResults, result =>
            {
                Assert.Equal(0.72, result.Verdict.Score);
                Assert.False(result.Verdict.Passed);
                Assert.Equal(expectedWarnings, result.Warnings.Order(StringComparer.Ordinal).ToArray());
            });
        }

        static void AssertRuntimeTeacherAttestedCandidate(
            DesignConversionLocalArtifactDbDpoCandidate candidate)
        {
            var teacherProvenance = candidate.TrainingRecord.TeacherProvenance;
            Assert.NotNull(teacherProvenance);
            Assert.Equal(TrainingTeacherSourceKind.ExternalTeacherOutput, teacherProvenance!.SourceKind);
            Assert.Equal(OpenWeightTeacherFamily.Qwen, teacherProvenance.Family);
            Assert.Equal(TrainingTeacherAccessMode.LocalHostedOpenWeight, teacherProvenance.AccessMode);
            Assert.Equal(TrainingTeacherHostingProvider.Ollama, teacherProvenance.HostingProvider);
            Assert.Equal(ExpectedTeacherModelId, teacherProvenance.ModelId);
            Assert.Equal(ExpectedTeacherManifestDigest, teacherProvenance.ModelVersion);
            Assert.Equal(TrainingTeacherModelLicense.Apache20, teacherProvenance.ModelLicense);
            Assert.False(teacherProvenance.ClosedProprietaryModel);
            Assert.True(teacherProvenance.TeacherOutputUsedForTraining);
            Assert.StartsWith("teacher-test-authoring-", teacherProvenance.RequestId, StringComparison.Ordinal);
            Assert.Equal(1, teacherProvenance.InvocationCount);
            Assert.Equal(
                DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                candidate.TrainingRecord.Provenance.Metadata["teacher_authoring_schema"]);
            var teacherAuthoringOutputHash = candidate.TrainingRecord.Provenance.Metadata[
                "teacher_authoring_output_hash"];
            Assert.Equal(64, teacherAuthoringOutputHash.Length);
            Assert.All(teacherAuthoringOutputHash, character => Assert.True(
                character is >= '0' and <= '9' or >= 'a' and <= 'f'));
        }

        static void AssertPass164ReviewOnlyTeacherCandidate(
            DesignConversionLocalArtifactDbDpoCandidate candidate)
        {
            var metadata = candidate.TrainingRecord.Provenance.Metadata;
            Assert.Equal(bool.TrueString, metadata["runtime_teacher_authored_candidate"]);
            Assert.Equal(bool.TrueString, metadata["independent_preference_proven"]);
            Assert.Equal(bool.TrueString, metadata["runtime_rejected_independently_evaluated"]);
            Assert.Equal(bool.FalseString, metadata["training_grade"]);
            Assert.False(metadata.ContainsKey("runtime_teacher_selected_preference"));
            Assert.Equal(bool.FalseString, metadata["raw_teacher_prompt_persisted"]);
            Assert.Equal(bool.FalseString, metadata["raw_teacher_output_persisted"]);
            Assert.Equal(bool.FalseString, metadata["raw_teacher_rationale_persisted"]);
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus,
                metadata["review_status"]);
            Assert.Equal(bool.FalseString, metadata["approved_for_training"]);
            Assert.Equal(bool.TrueString, metadata[
                DesignConversionDpoGenerator.PayloadCustodyRequiredMetadataKey]);
            Assert.StartsWith(
                "sha256:",
                metadata[DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey],
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Pass164R_ReviewOnlyTeacherCorpusRequiresAdmissionAfterBoundedSnapshotAndPreservesContentLimitPrecedenceWithoutPromotionEffects()
    {
        const string privateMarker = "PRIVATE_PASS164R_COUNT_GATE_PROMOTION_SECRET";
        const int requiredSourcePairCount = 100;
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "pass164r-loop-training-promotion");
        Directory.CreateDirectory(root);
        try
        {
            var runnerInvocationCount = 0;
            var publicationCount = 0;
            var activationCount = 0;
            var authorityRewriteCount = 0;
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                    Interlocked.Increment(ref runnerInvocationCount),
                runTrainingIntakeLocalPromotionBeforePublicationObserverForTesting: () =>
                    Interlocked.Increment(ref publicationCount),
                runTrainingIntakeLocalPromotionActivationInvokedObserverForTesting: () =>
                    Interlocked.Increment(ref activationCount),
                recordsAuthorityRewriteObserverForTesting: () =>
                    Interlocked.Increment(ref authorityRewriteCount),
                dpoPolicyBaseProvider: new DesignConversionDpoPolicyBaseTestProvider(),
                legalTeacherAdmissionAuthority: DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(
                    modelId: ExpectedTeacherModelId,
                    expectedOllamaManifestDigest: ExpectedTeacherManifestDigest,
                    expectedOllamaModelLayerDigest: ExpectedTeacherModelLayerDigest));
            using var teacherAssociation =
                DesignConversionDpoTeacherTestAssociation.Associate(
                    artifactDb,
                    supportTeacherAuthoring: true,
                    supportIndependentPreferenceEvidence: true);
            var usefulnessApi = new DesignConversionDpoUsefulnessApi(artifactDb);
            var renderProvider = new DeterministicDesignConversionRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(request =>
                request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.97 : 0.72);
            var visualJudge = new ScriptedVisualJudge(request =>
                request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                    ? JudgeScript.FromScore(0.97, 0m)
                    : JudgeScript.FromScore(0.72, 0m));
            var strategy = new Pass164RSourceAwareCandidateStrategy(privateMarker);
            var adapter = CreateAdapter(
                renderProvider,
                comparisonProvider,
                visualJudge,
                strategy,
                artifactDb);

            var first = await CreatePass164RRuntimeCandidateAsync(
                artifactDb,
                usefulnessApi,
                adapter,
                sourceOrdinal: 0,
                privateMarker,
                approveImmediately: true);
            Assert.Empty(strategy.Requests);

            var singleSourceHandoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: first.Stored.JobId),
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(singleSourceHandoff.IsSuccess, singleSourceHandoff.ErrorMessage);
            Assert.Equal(2, singleSourceHandoff.Value!.ApprovedCandidateCount);

            var singleSourceIntake = await usefulnessApi.CreateTrainingIntakeAsync(
                singleSourceHandoff.Value,
                "architect:pass164r-local",
                "Pass164R negative control: one loop-created runtime source must not start training",
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(singleSourceIntake.IsSuccess, singleSourceIntake.ErrorMessage);

            var singleSourcePreflight = await usefulnessApi.RunTrainingIntakePreflightAsync(singleSourceIntake.Value!.IntakeId);
            Assert.True(singleSourcePreflight.IsSuccess, singleSourcePreflight.ErrorMessage);
            Assert.False(singleSourcePreflight.Value!.MayStartTraining);
            Assert.Equal("blocked_same_source", singleSourcePreflight.Value.SourceDisjointStatus);
            Assert.Contains("source_disjoint_blocked_same_source", singleSourcePreflight.Value.BlockReasons);
            Assert.Equal(bool.FalseString, singleSourcePreflight.Value.BoundaryMetadata["model_upload_ready"]);
            Assert.Equal(bool.FalseString, singleSourcePreflight.Value.BoundaryMetadata["product_ready"]);

            var second = await CreatePass164RRuntimeCandidateAsync(
                artifactDb,
                usefulnessApi,
                adapter,
                sourceOrdinal: 1,
                privateMarker,
                approveImmediately: true);
            var traces = new List<Pass164RRuntimeCandidateTrace> { first, second };

            var promotionProbeHandoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(),
                DateTimeOffset.UnixEpoch.AddMinutes(165));
            Assert.True(promotionProbeHandoff.IsSuccess, promotionProbeHandoff.ErrorMessage);
            Assert.Equal(4, promotionProbeHandoff.Value!.ApprovedCandidateCount);
            Assert.Equal(4, promotionProbeHandoff.Value.Rows.Count);
            Assert.Equal(
                2,
                promotionProbeHandoff.Value.Rows
                    .Select(row => row.SourceManifestRef)
                    .Distinct(StringComparer.Ordinal)
                    .Count());

            var promotionProbeIntake = await usefulnessApi.CreateTrainingIntakeAsync(
                promotionProbeHandoff.Value,
                "architect:pass164r-local-promotion-probe",
                "Pass164R four-row source-disjoint teacher corpus must require live corpus admission after bounded snapshot",
                DateTimeOffset.UnixEpoch.AddMinutes(165));
            Assert.True(promotionProbeIntake.IsSuccess, promotionProbeIntake.ErrorMessage);
            Assert.Equal(4, promotionProbeIntake.Value!.AcceptedCandidateCount);
            var promotionProbePreflight = await usefulnessApi.RunTrainingIntakePreflightAsync(
                promotionProbeIntake.Value.IntakeId);
            Assert.True(promotionProbePreflight.IsSuccess, promotionProbePreflight.ErrorMessage);
            Assert.True(promotionProbePreflight.Value!.MayStartTraining);
            Assert.Equal("proven", promotionProbePreflight.Value.SourceDisjointStatus);

            var admissionRequiredOutputDirectory = Path.Combine(root, "pass164r-admission-required");
            var admissionRequiredMachineMutationCount = 0;
            var previousPartitionMutationObserver = DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting;
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                _ => Interlocked.Increment(ref admissionRequiredMachineMutationCount);
            OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> admissionRequiredPromotion;
            try
            {
                admissionRequiredPromotion = await DesignConversionPromotionAdmissionTestScope.RunAsync(
                    () => usefulnessApi.RunTrainingIntakeAndPromoteLocalAsync(
                        promotionProbeIntake.Value.IntakeId,
                        admissionRequiredOutputDirectory));
            }
            finally
            {
                DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                    previousPartitionMutationObserver;
            }

            Assert.False(admissionRequiredPromotion.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode,
                admissionRequiredPromotion.ErrorCode);
            Assert.Null(admissionRequiredPromotion.Value);
            Assert.Equal(0, Volatile.Read(ref runnerInvocationCount));
            Assert.Equal(0, Volatile.Read(ref admissionRequiredMachineMutationCount));
            Assert.Equal(0, Volatile.Read(ref publicationCount));
            Assert.Equal(0, Volatile.Read(ref activationCount));
            Assert.False(Directory.Exists(admissionRequiredOutputDirectory));
            Assert.Empty(Directory.EnumerateFiles(root, "*checkpoint*", SearchOption.AllDirectories));
            Assert.Empty(Directory.EnumerateFiles(root, "*receipt*", SearchOption.AllDirectories));

            var serializedAdmissionFailure = JsonSerializer.Serialize(admissionRequiredPromotion);
            Assert.DoesNotContain(privateMarker, serializedAdmissionFailure, StringComparison.Ordinal);
            Assert.DoesNotContain("<main", serializedAdmissionFailure, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Users/lubao", serializedAdmissionFailure, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("external-provider-payload", serializedAdmissionFailure, StringComparison.OrdinalIgnoreCase);

            var sourceDisjointApprovalRequests =
                new List<DesignConversionDpoCandidateApprovalRequest>(
                    (requiredSourcePairCount - 2) * 2);
            for (var sourceOrdinal = 2; sourceOrdinal < requiredSourcePairCount; sourceOrdinal++)
            {
                var trace = await CreatePass164RRuntimeCandidateAsync(
                    artifactDb,
                    usefulnessApi,
                    adapter,
                    sourceOrdinal,
                    privateMarker,
                    approveImmediately: false);
                traces.Add(trace);
                sourceDisjointApprovalRequests.AddRange(
                    trace.CandidateIds.Select(candidateId =>
                        new DesignConversionDpoCandidateApprovalRequest(
                            trace.Stored.RecordId,
                            candidateId,
                            "reviewer:pass164r-local",
                            $"Pass164R batch review-only teacher pair approval for source ordinal {sourceOrdinal:000}; corpus admission remains required",
                            DateTimeOffset.UnixEpoch.AddMinutes(164).AddSeconds(sourceOrdinal))));
            }

            Assert.Equal(requiredSourcePairCount, traces.Count);
            Assert.Equal(
                requiredSourcePairCount,
                traces.Select(trace => trace.SourceManifestRef).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(
                requiredSourcePairCount,
                traces.Select(trace => trace.SourceStructurePayload).Distinct(StringComparer.Ordinal).Count());
            Assert.Empty(strategy.Requests);
            Assert.Equal(requiredSourcePairCount * 4, comparisonProvider.Requests.Count);
            Assert.Equal(requiredSourcePairCount * 4, visualJudge.Requests.Count);
            Assert.Equal((requiredSourcePairCount - 2) * 2, sourceDisjointApprovalRequests.Count);

            var rewriteCountBeforeBatchApproval = Volatile.Read(ref authorityRewriteCount);
            var sourceDisjointApprovals =
                await usefulnessApi.ApproveRuntimeCreatedDpoCandidatesAsync(
                    sourceDisjointApprovalRequests);
            Assert.True(sourceDisjointApprovals.IsSuccess, sourceDisjointApprovals.ErrorMessage);
            Assert.Equal(
                (requiredSourcePairCount - 2) * 2,
                sourceDisjointApprovals.Value!.Approvals.Count);
            Assert.Equal(
                rewriteCountBeforeBatchApproval + 1,
                Volatile.Read(ref authorityRewriteCount));
            Assert.Equal(
                bool.TrueString,
                sourceDisjointApprovals.Value.BoundaryMetadata["approval_batch_atomic"]);

            var sourceDisjointHandoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(),
                DateTimeOffset.UnixEpoch.AddMinutes(166));
            Assert.True(sourceDisjointHandoff.IsSuccess, sourceDisjointHandoff.ErrorMessage);
            Assert.Equal(requiredSourcePairCount * 2, sourceDisjointHandoff.Value!.ApprovedCandidateCount);
            Assert.Equal(requiredSourcePairCount * 2, sourceDisjointHandoff.Value.Rows.Count);
            Assert.Equal(
                requiredSourcePairCount,
                sourceDisjointHandoff.Value.Rows
                    .Select(row => row.SourceManifestRef)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.Equal(
                requiredSourcePairCount,
                sourceDisjointHandoff.Value.Rows
                    .Select(row => row.SourceStructureHash)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.All(sourceDisjointHandoff.Value.Rows, row =>
            {
                Assert.True(row.TrainingHandoffOnly);
                Assert.Equal(bool.TrueString, row.BoundaryMetadata["runtime_created_dpo_candidate"]);
                Assert.Equal(bool.FalseString, row.BoundaryMetadata["production_dpo"]);
                Assert.Equal(bool.TrueString, row.BoundaryMetadata["product_not_ready"]);
                Assert.Equal(bool.FalseString, row.BoundaryMetadata["model_readiness_claimed"]);
            });

            var sourceDisjointIntake = await usefulnessApi.CreateTrainingIntakeAsync(
                sourceDisjointHandoff.Value,
                "architect:pass164r-local",
                "Pass164R high-cardinality source-disjoint teacher-authored review rows must fail closed at bounded training content",
                DateTimeOffset.UnixEpoch.AddMinutes(167));
            Assert.True(sourceDisjointIntake.IsSuccess, sourceDisjointIntake.ErrorMessage);
            Assert.Equal(requiredSourcePairCount * 2, sourceDisjointIntake.Value!.AcceptedCandidateCount);
            Assert.Equal(
                requiredSourcePairCount,
                sourceDisjointIntake.Value.CandidateRefs
                    .Select(candidate => candidate.SourceIdentityHash)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.Equal(
                requiredSourcePairCount * 2,
                sourceDisjointIntake.Value.CandidateRefs
                    .Select(candidate => candidate.TrainingRecordId)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.True(sourceDisjointIntake.Value.ProductNotReady);
            Assert.Equal(bool.FalseString, sourceDisjointIntake.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal("proven", sourceDisjointIntake.Value.BoundaryMetadata["source_manifest_status"]);
            Assert.Equal("proven", sourceDisjointIntake.Value.BoundaryMetadata["source_disjoint_status"]);

            var sourceDisjointPreflight = await usefulnessApi.RunTrainingIntakePreflightAsync(sourceDisjointIntake.Value.IntakeId);
            Assert.True(sourceDisjointPreflight.IsSuccess, sourceDisjointPreflight.ErrorMessage);
            Assert.True(sourceDisjointPreflight.Value!.MayStartTraining);
            Assert.Equal("proven", sourceDisjointPreflight.Value.SourceManifestStatus);
            Assert.Equal("proven", sourceDisjointPreflight.Value.SourceDisjointStatus);
            Assert.Empty(sourceDisjointPreflight.Value.BlockReasons);
            Assert.Equal((requiredSourcePairCount * 2).ToString(), sourceDisjointPreflight.Value.BoundaryMetadata["source_manifest_candidate_count"]);
            Assert.Equal(requiredSourcePairCount.ToString(), sourceDisjointPreflight.Value.BoundaryMetadata["source_manifest_hash_count"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["model_upload_ready"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["product_ready"]);

            var promotionOutputDirectory = Path.Combine(root, "pass164r-content-limit");
            var machineMutationCount = 0;
            var previousMutationObserver = DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting;
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                _ => Interlocked.Increment(ref machineMutationCount);
            OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> promoted;
            try
            {
                promoted = await DesignConversionPromotionAdmissionTestScope.RunAsync(
                    () => usefulnessApi.RunTrainingIntakeAndPromoteLocalAsync(
                        sourceDisjointIntake.Value.IntakeId,
                        promotionOutputDirectory));
            }
            finally
            {
                DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                    previousMutationObserver;
            }

            Assert.False(promoted.IsSuccess);
            Assert.Equal(
                AgentInfrastructure.Logical.BoundedDpoTrainingContentProjector.LimitExceededErrorCode,
                promoted.ErrorCode);
            Assert.Null(promoted.Value);
            Assert.Equal(0, Volatile.Read(ref runnerInvocationCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.Equal(0, Volatile.Read(ref publicationCount));
            Assert.Equal(0, Volatile.Read(ref activationCount));
            Assert.False(Directory.Exists(promotionOutputDirectory));
            Assert.Empty(Directory.EnumerateFiles(root, "*checkpoint*", SearchOption.AllDirectories));
            Assert.Empty(Directory.EnumerateFiles(root, "*receipt*", SearchOption.AllDirectories));

            var serializedPromotionFailure = JsonSerializer.Serialize(promoted);
            Assert.DoesNotContain(privateMarker, serializedPromotionFailure, StringComparison.Ordinal);
            Assert.DoesNotContain("<main", serializedPromotionFailure, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Users/lubao", serializedPromotionFailure, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("external-provider-payload", serializedPromotionFailure, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_ProviderRequired_ReturnsBoundedNonReadyEnvelopeWithoutExternalProviderClaim()
    {
        const string privateMarker = "PRIVATE_PASS129_PROVIDER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.40);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromBlockedProvider());
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-provider-required-loop",
            InitialPackage: CreatePackage("pass129-provider-required-request", privateMarker),
            MaxAttempts: 3);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.True(root.GetProperty("providerRequiredOrBlocked").GetBoolean());
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_BlockingRenderDeadlineReturnsTypedDeadlineEnvelopeWithoutAttempt()
    {
        const string privateMarker = "PRIVATE_DEADLINE_MODEL_SERVER_SECRET";
        var renderProvider = new BlockingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.70);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            runDeadline: TimeSpan.FromMilliseconds(50));
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "modelserver-deadline-loop",
            InitialPackage: CreatePackage("modelserver-deadline-request", privateMarker),
            MaxAttempts: 3);

        var stopwatch = Stopwatch.StartNew();
        var envelope = await adapter.TryRunImprovementLoopAsync(package);
        stopwatch.Stop();

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionImprovementLoopCoordinator.DeadlineExceeded, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("boundaryMetadata").TryGetProperty("quality_threshold_source", out _));
        Assert.DoesNotContain("\"quality_threshold_source\"", envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.True(root.GetProperty("deadlineExceeded").GetBoolean());
        Assert.False(root.GetProperty("cancelled").GetBoolean());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("candidateStrategyFailed").GetBoolean());
        Assert.Equal(0, root.GetProperty("attemptCount").GetInt32());
        Assert.Empty(root.GetProperty("attempts").EnumerateArray());
        Assert.Contains(
            DesignConversionImprovementLoopCoordinator.DeadlineExceededMarker,
            root.GetProperty("evidenceRefs").EnumerateArray().Select(reference => reference.GetString()));
        Assert.Contains(
            "IMPROVEMENT_LOOP_DEADLINE_EXCEEDED_DURING_EVALUATION",
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));

        Assert.Single(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
        Assert.False(envelope.BoundaryMetadata.ContainsKey("quality_threshold_source"));
        Assert.Equal(DesignConversionImprovementLoopCoordinator.DeadlineExceeded, envelope.BoundaryMetadata["stop_reason"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["completed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cancelled"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["provider_payload_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), $"deadline elapsed {stopwatch.Elapsed}");
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CandidateNoOp_PropagatesNoProgressStopAsBoundedNonReady()
    {
        const string privateMarker = "PRIVATE_NO_OP_CANDIDATE_SECRET";
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "candidate-no-op-loop",
            InitialPackage: CreatePackage("candidate-no-op-request", privateMarker),
            MaxAttempts: 3);
        var noOpCandidate = new DesignConversionImprovementCandidate(
            CandidateId: "candidate-no-op-same-html-css",
            RequestId: "candidate-no-op-next-request",
            Html: package.InitialPackage!.Html!,
            Css: package.InitialPackage.Css!,
            ChangeReasons: ["scripted no-op candidate"],
            EvidenceRefs: ["candidate-no-op:evidence"],
            Metadata: new Dictionary<string, string>
            {
                ["private_marker"] = privateMarker
            });
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(noOpCandidate));
        var adapter = CreateAdapter(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(_ => 0.72),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m)),
            strategy);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.False(root.GetProperty("candidateStrategyFailed").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Contains(
            "IMPROVEMENT_LOOP_CANDIDATE_VALIDATION_NO_PROGRESS:IMPROVEMENT_CANDIDATE_NO_OP",
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["candidate_strategy_failed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["completed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["quality_reached"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, envelope.BoundaryMetadata["stop_reason"]);
        Assert.Single(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_UnsupportedArtifactProofRefs_FailLocalValidationBeforeRuntime()
    {
        var adapter = CreateAdapter(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(_ => 0.72),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m)),
            new ScriptedCandidateStrategy());
        var initialPackage = CreatePackage(
            "pass129-unsupported-artifact-loop-request",
            "PRIVATE_PASS129_UNSUPPORTED_ARTIFACT_SECRET");
        initialPackage = initialPackage with
        {
            Screenshots =
            [
                initialPackage.Screenshots![0] with
                {
                    ArtifactId = "unsupported-improvement-loop-screenshot-proof",
                    Uri = "https://client.example.invalid/improvement-loop-screenshot-proof.png",
                    Sha256 = "sha256:unsupported-improvement-loop-screenshot-proof"
                }
            ],
            DesignRenders = []
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-unsupported-artifact-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal("DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED", envelope.ParsedErrorCode);
        Assert.Contains("refs-only", envelope.ParsedErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_OversizedInitialPackageFailsLocalValidationBeforeRuntime()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m)),
            strategy);
        var initialPackage = CreatePackage(
            "pass129-oversized-payload-loop-request",
            "PRIVATE_PASS129_OVERSIZED_PAYLOAD_SECRET") with
        {
            DesignTree = new DesignConversionModelServerPayloadRef(
                RefId: "pass129-oversized-payload-loop-request:design-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass129-oversized-payload-loop-request:design-tree",
                Payload: new string('t', DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1),
                Format: "figma-json")
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-oversized-payload-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionModelServerPayloadSizeGuard.ErrorCode, envelope.ParsedErrorCode);
        Assert.Contains("designTree.payload", envelope.ParsedErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("")]
    public async Task TryRunImprovementLoopAsync_UnknownTierZeroMaxImprovementCandidatesFailsLocalValidationBeforeRegistry(
        string tier)
    {
        const string privateMarker = "PRIVATE_PASS_CANDIDATE_BUDGET_ADAPTER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage(
            "pass129-candidate-budget-adapter-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                Tier = tier,
                MaxImprovementCandidates = 0,
                BudgetUsd = 0m
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-candidate-budget-adapter-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, envelope.ParsedErrorCode);
        Assert.Equal(
            "DesignConversion improvement candidate budget must be greater than zero.",
            envelope.ParsedErrorMessage);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public void DescribeImprovementLoopRoute_ValidationFailuresDocumentBudgetValidationRules()
    {
        var validationFailures = DesignConversionModelServerCallerContract
            .DescribeImprovementLoopRoute()
            .ValidationFailures;

        var negativeBudgetUsdFailure = Assert.Single(
            validationFailures,
            failure => failure.ErrorCode == DesignConversionInternalService.NegativeBudgetUsdErrorCode);
        Assert.Equal(400, negativeBudgetUsdFailure.StatusCode);
        Assert.Contains("negative", negativeBudgetUsdFailure.Trigger, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("budgetUsd", negativeBudgetUsdFailure.Trigger, StringComparison.Ordinal);

        var improvementCandidateBudgetFailure = Assert.Single(
            validationFailures,
            failure => failure.ErrorCode == DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);
        Assert.Equal(400, improvementCandidateBudgetFailure.StatusCode);
        Assert.Contains("negative", improvementCandidateBudgetFailure.Trigger, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("maxImprovementCandidates", improvementCandidateBudgetFailure.Trigger, StringComparison.Ordinal);
        Assert.Contains("zero", improvementCandidateBudgetFailure.Trigger, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "server-owned entitlement tier",
            improvementCandidateBudgetFailure.Trigger,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_NegativeMaxImprovementCandidatesFailsLocalValidationBeforeRegistry()
    {
        const string privateMarker = "PRIVATE_PASS_NEGATIVE_CANDIDATE_BUDGET_ADAPTER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage(
            "pass129-negative-candidate-budget-adapter-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                MaxImprovementCandidates = -1
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-negative-candidate-budget-adapter-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, envelope.ParsedErrorCode);
        Assert.Equal(
            "DesignConversion improvement candidate budget must be greater than zero.",
            envelope.ParsedErrorMessage);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_NegativeBudgetUsdFailsLocalValidationBeforeRegistry()
    {
        const string privateMarker = "PRIVATE_PASS_NEGATIVE_BUDGET_ADAPTER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage(
            "pass129-negative-budget-adapter-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                BudgetUsd = -0.01m
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-negative-budget-adapter-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionInternalService.NegativeBudgetUsdErrorCode, envelope.ParsedErrorCode);
        Assert.Equal(
            "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
            envelope.ParsedErrorMessage);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_FreeTierZeroBudgetAndMaxUsesEntitlementDefaultsThroughRegistry()
    {
        const string privateMarker = "PRIVATE_PASS_FREE_ZERO_ENTITLEMENT_HANDLER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.70);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("pass-free-zero-entitlement-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage(
            "pass-free-zero-entitlement-adapter-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: " free ",
                MaxImprovementCandidates: 0,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0m,
                Metadata: new Dictionary<string, string>
                {
                    ["budget_private_marker"] = privateMarker
                })
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass-free-zero-entitlement-adapter-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal("free", attemptMetadata.GetProperty("tier").GetString());
        Assert.Equal("0", attemptMetadata.GetProperty("submitted_budget_usd").GetString());
        Assert.Equal("0", attemptMetadata.GetProperty("submitted_max_improvement_candidates").GetString());
        Assert.Equal("0.05", attemptMetadata.GetProperty("effective_budget_usd").GetString());
        Assert.Equal("1", attemptMetadata.GetProperty("effective_max_improvement_candidates").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("tier_policy_budget_entitlement_default_applied").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("tier_policy_iteration_entitlement_default_applied").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("tier_policy_budget_cap_applied").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("tier_policy_iteration_cap_applied").GetString());
        Assert.Equal("iteration_budget", attemptMetadata.GetProperty("budget_stop_source").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal("0", metadata.GetProperty("submitted_budget_usd").GetString());
        Assert.Equal("0", metadata.GetProperty("submitted_max_improvement_candidates").GetString());
        Assert.Equal("0.05", metadata.GetProperty("effective_budget_usd").GetString());
        Assert.Equal("1", metadata.GetProperty("effective_max_improvement_candidates").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("tier_policy_budget_entitlement_default_applied").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("tier_policy_iteration_entitlement_default_applied").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("tier_policy_budget_cap_applied").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("tier_policy_iteration_cap_applied").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Equal("0", envelope.BoundaryMetadata["submitted_budget_usd"]);
        Assert.Equal("0", envelope.BoundaryMetadata["submitted_max_improvement_candidates"]);
        Assert.Equal("0.05", envelope.BoundaryMetadata["effective_budget_usd"]);
        Assert.Equal("1", envelope.BoundaryMetadata["effective_max_improvement_candidates"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["tier_policy_budget_entitlement_default_applied"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["tier_policy_iteration_entitlement_default_applied"]);

        Assert.Single(renderProvider.Requests.Select(providerRequest => providerRequest.Metadata["job_id"]).Distinct(StringComparer.Ordinal));
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("")]
    public async Task TryRunImprovementLoopAsync_ExplicitZeroBudgetForNonEntitlementTierRemainsZeroThroughHandler(
        string tier)
    {
        const string privateMarker = "PRIVATE_W084_EXPLICIT_ZERO_BUDGET_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.70);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("w084-explicit-zero-budget-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage(
            "w084-explicit-zero-budget-adapter-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: tier,
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0m,
                Metadata: new Dictionary<string, string>
                {
                    ["budget_private_marker"] = privateMarker
                })
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "w084-explicit-zero-budget-adapter-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        Assert.Equal(0.02m, attempt.GetProperty("estimatedCostUsd").GetDecimal());
        Assert.Equal(0m, attempt.GetProperty("budgetUsd").GetDecimal());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal("0.02", attemptMetadata.GetProperty("estimated_cost_usd").GetString());
        Assert.Equal("0", attemptMetadata.GetProperty("submitted_budget_usd").GetString());
        Assert.Equal("0", attemptMetadata.GetProperty("effective_budget_usd").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("tier_policy_budget_entitlement_default_applied").GetString());
        Assert.Equal("cost_budget", attemptMetadata.GetProperty("budget_stop_source").GetString());

        var responseMetadata = root.GetProperty("boundaryMetadata");
        Assert.Equal("0.02", responseMetadata.GetProperty("final_estimated_cost_usd").GetString());
        Assert.Equal("0", responseMetadata.GetProperty("submitted_budget_usd").GetString());
        Assert.Equal("0", responseMetadata.GetProperty("effective_budget_usd").GetString());
        Assert.Equal(bool.FalseString, responseMetadata.GetProperty("tier_policy_budget_entitlement_default_applied").GetString());
        Assert.Equal("cost_budget", responseMetadata.GetProperty("budget_stop_source").GetString());
        Assert.Equal(bool.FalseString, responseMetadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, responseMetadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, responseMetadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, responseMetadata.GetProperty("response_contains_raw_private_data").GetString());

        Assert.Equal("0.02", envelope.BoundaryMetadata["final_estimated_cost_usd"]);
        Assert.Equal("0", envelope.BoundaryMetadata["submitted_budget_usd"]);
        Assert.Equal("0", envelope.BoundaryMetadata["effective_budget_usd"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["tier_policy_budget_entitlement_default_applied"]);
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("")]
    public async Task HandleAsync_ZeroMaxImprovementCandidatesForUnknownOrBlankTierFailsBeforeRuntimeWhenAdapterBypassed(
        string tier)
    {
        const string privateMarker = "PRIVATE_PASS_UNKNOWN_ZERO_HANDLER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, strategy);
        var handler = new DesignConversionModelServerImprovementLoopHandler(coordinator);
        var initialPackage = CreatePackage(
            "pass129-unknown-zero-handler-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                Tier = tier,
                MaxImprovementCandidates = 0,
                BudgetUsd = 0m
            }
        };
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "pass129-unknown-zero-handler-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var response = await handler.HandleAsync(request);

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        AssertNoPrivateMarkerLeak(response.BodyJson, privateMarker, loopRequest.InitialPackage!);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(
            "DesignConversion improvement candidate budget must be greater than zero.",
            root.GetProperty("errorMessage").GetString());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_NegativeBudgetUsdFailsBeforeRuntimeWhenAdapterBypassed()
    {
        const string privateMarker = "PRIVATE_PASS_NEGATIVE_BUDGET_HANDLER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, strategy);
        var handler = new DesignConversionModelServerImprovementLoopHandler(coordinator);
        var initialPackage = CreatePackage(
            "pass129-negative-budget-handler-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                Tier = "free",
                MaxImprovementCandidates = 1,
                BudgetUsd = -0.01m
            }
        };
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "pass129-negative-budget-handler-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var response = await handler.HandleAsync(request);

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        AssertNoPrivateMarkerLeak(response.BodyJson, privateMarker, loopRequest.InitialPackage!);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerImprovementLoopHandler.NegativeBudgetUsdErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(
            "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
            root.GetProperty("errorMessage").GetString());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_NegativeMaxImprovementCandidatesFailsBeforeRuntimeWhenAdapterBypassed()
    {
        const string privateMarker = "PRIVATE_PASS_CANDIDATE_BUDGET_HANDLER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, strategy);
        var handler = new DesignConversionModelServerImprovementLoopHandler(coordinator);
        var initialPackage = CreatePackage(
            "pass129-candidate-budget-handler-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                MaxImprovementCandidates = -1
            }
        };
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "pass129-candidate-budget-handler-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var response = await handler.HandleAsync(request);

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        AssertNoPrivateMarkerLeak(response.BodyJson, privateMarker, loopRequest.InitialPackage!);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(
            "DesignConversion improvement candidate budget must be greater than zero.",
            root.GetProperty("errorMessage").GetString());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_MaxAttemptsAboveCoordinatorCapFailsLocalValidationBeforeRuntime()
    {
        const string privateMarker = "PRIVATE_PASS_MAX_ATTEMPTS_ADAPTER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-max-attempts-adapter-loop",
            InitialPackage: CreatePackage("pass129-max-attempts-adapter-request", privateMarker),
            MaxAttempts: DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts + 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.MaxAttemptsExceededErrorCode,
            envelope.ParsedErrorCode);
        Assert.Equal(ExpectedMaxAttemptsExceededErrorMessage(), envelope.ParsedErrorMessage);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_MaxAttemptsAboveCoordinatorCapFailsBeforeRuntimeWhenAdapterBypassed()
    {
        const string privateMarker = "PRIVATE_PASS_MAX_ATTEMPTS_HANDLER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, strategy);
        var handler = new DesignConversionModelServerImprovementLoopHandler(coordinator);
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "pass129-max-attempts-handler-loop",
            InitialPackage: CreatePackage("pass129-max-attempts-handler-request", privateMarker),
            MaxAttempts: DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts + 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var response = await handler.HandleAsync(request);

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        Assert.DoesNotContain(privateMarker, response.BodyJson, StringComparison.Ordinal);
        AssertNoPrivateMarkerLeak(response.BodyJson, privateMarker, loopRequest.InitialPackage!);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.MaxAttemptsExceededErrorCode,
            root.GetProperty("errorCode").GetString());
        Assert.Equal(ExpectedMaxAttemptsExceededErrorMessage(), root.GetProperty("errorMessage").GetString());
        Assert.Equal(bool.FalseString, root.GetProperty("boundaryMetadata").GetProperty("runtime_invoked").GetString());
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_EnabledAdmissionRejectsMissingAndInvalidAuthBeforeImprovementLoopRuntimeWithoutLeakage()
    {
        const string privateMarker = "PRIVATE_N25_4A1_AUTH_SECRET";
        const string idempotencyKey = "n25-4a1-auth-key";
        const string invalidToken = "wrong-improvement-loop-route-token-0123456789";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var routeAdmission = CreateEnabledImprovementLoopAdmission();
        var handler = CreateAdmissionBackedHandler(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            routeAdmission);
        var package = CreatePackage("n25-4a1-auth-request", privateMarker);
        var bodyJson = SerializeLoopRequest(
            "n25-4a1-auth-loop",
            package,
            maxAttempts: 1,
            privateMarker);

        var missingAuth = await handler.HandleAsync(ImprovementLoopHttpRequest(
            bodyJson,
            AdmissionHeaders(token: null, idempotencyKey: idempotencyKey)));
        var invalidAuth = await handler.HandleAsync(ImprovementLoopHttpRequest(
            bodyJson,
            AdmissionHeaders(token: invalidToken, idempotencyKey: idempotencyKey)));

        Assert.Equal(401, missingAuth.StatusCode);
        Assert.Equal(403, invalidAuth.StatusCode);
        AssertError(missingAuth, "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED");
        AssertError(invalidAuth, "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_INVALID");
        AssertRuntimeNotInvoked(missingAuth);
        AssertRuntimeNotInvoked(invalidAuth);
        AssertNoPrivateMarkerLeak(missingAuth.BodyJson, privateMarker, package);
        AssertNoPrivateMarkerLeak(invalidAuth.BodyJson, privateMarker, package);
        AssertResponseAndBoundaryMetadataDoNotLeak(
            missingAuth,
            RouteAdmissionCurrentToken,
            RouteAdmissionPreviousToken,
            RouteAdmissionTenantId,
            idempotencyKey,
            privateMarker,
            package.Html!,
            package.Css!,
            package.DesignTree!.Payload!,
            package.OriginalTree!.Payload!);
        AssertResponseAndBoundaryMetadataDoNotLeak(
            invalidAuth,
            RouteAdmissionCurrentToken,
            RouteAdmissionPreviousToken,
            RouteAdmissionTenantId,
            idempotencyKey,
            invalidToken,
            privateMarker,
            package.Html!,
            package.Css!,
            package.DesignTree!.Payload!,
            package.OriginalTree!.Payload!);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_EnabledAdmissionRejectsMissingIdempotencyKeyWithLoopSpecificDenialBeforeRuntime()
    {
        const string privateMarker = "PRIVATE_N25_4A1_MISSING_IDEMPOTENCY_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var handler = CreateAdmissionBackedHandler(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            CreateEnabledImprovementLoopAdmission());
        var package = CreatePackage("n25-4a1-missing-idempotency-request", privateMarker);
        var bodyJson = SerializeLoopRequest(
            "n25-4a1-missing-idempotency-loop",
            package,
            maxAttempts: 1,
            privateMarker);

        var response = await handler.HandleAsync(ImprovementLoopHttpRequest(
            bodyJson,
            AdmissionHeaders(idempotencyKey: null)));

        Assert.Equal(400, response.StatusCode);
        AssertError(
            response,
            "DESIGN_CONVERSION_IDEMPOTENCY_KEY_REQUIRED",
            "DesignConversion improvement-loop endpoint requires an Idempotency-Key header.");
        AssertRuntimeNotInvoked(response);
        AssertNoPrivateMarkerLeak(response.BodyJson, privateMarker, package);
        AssertResponseAndBoundaryMetadataDoNotLeak(
            response,
            RouteAdmissionCurrentToken,
            RouteAdmissionPreviousToken,
            RouteAdmissionTenantId,
            privateMarker,
            package.Html!,
            package.Css!,
            package.DesignTree!.Payload!,
            package.OriginalTree!.Payload!);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_EnabledAdmissionReplaysSameCanonicalImprovementLoopBodyOnceWithSafeRouteMetadata()
    {
        const string privateMarker = "PRIVATE_N25_4A1_REPLAY_SECRET";
        const string idempotencyKey = "n25-4a1-replay-key";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.72);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.96, 0.01m)
                : JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n25-4a1-replay-candidate", privateMarker)));
        var handler = CreateAdmissionBackedHandler(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            CreateEnabledImprovementLoopAdmission());
        var package = CreatePackage("n25-4a1-replay-request", privateMarker);
        var bodyJson = SerializeLoopRequest(
            "n25-4a1-replay-loop",
            package,
            maxAttempts: 2,
            privateMarker);
        var request = ImprovementLoopHttpRequest(
            bodyJson,
            AdmissionHeaders(idempotencyKey: idempotencyKey));

        var first = await handler.HandleAsync(request);
        var replay = await handler.HandleAsync(request);

        Assert.Equal(200, first.StatusCode);
        Assert.Equal(200, replay.StatusCode);
        Assert.Equal(first.BodyJson, replay.BodyJson);
        Assert.Equal(4, renderProvider.Requests.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Single(strategy.Requests);
        AssertNoPrivateMarkerLeak(first.BodyJson, privateMarker, package);
        AssertResponseAndBoundaryMetadataDoNotLeak(
            first,
            RouteAdmissionCurrentToken,
            RouteAdmissionPreviousToken,
            RouteAdmissionTenantId,
            idempotencyKey,
            privateMarker,
            package.Html!,
            package.Css!,
            package.DesignTree!.Payload!,
            package.OriginalTree!.Payload!);

        var metadata = BoundaryMetadata(first);
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_authorized").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_token_accepted").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("idempotency_key_accepted").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("idempotency_route_scoped").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_tenant_bound").GetString());
        Assert.Equal(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(RouteAdmissionTenantId),
            metadata.GetProperty("route_admission_tenant_id_ref").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_raw_tenant_id_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_payload_digest_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_route_method_path_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_effective_scope_contains_raw_route").GetString());
        Assert.Equal("ref_only", metadata.GetProperty("route_admission_method").GetString());
        Assert.Equal("ref_only", metadata.GetProperty("route_admission_path").GetString());
        Assert.StartsWith("sha256:", metadata.GetProperty("idempotency_effective_scope_ref").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata.GetProperty("idempotency_route_scope_ref").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata.GetProperty("idempotency_route_method_ref").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata.GetProperty("idempotency_route_path_ref").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_EnabledAdmissionConflictsSameKeyDifferentCanonicalLoopBodyWithoutSecondExecutionOrLeakage()
    {
        const string firstPrivateMarker = "PRIVATE_N25_4A1_CONFLICT_FIRST_SECRET";
        const string secondPrivateMarker = "PRIVATE_N25_4A1_CONFLICT_SECOND_SECRET";
        const string idempotencyKey = "n25-4a1-conflict-key";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var handler = CreateAdmissionBackedHandler(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            CreateEnabledImprovementLoopAdmission());
        var firstPackage = CreatePackage("n25-4a1-conflict-first-request", firstPrivateMarker);
        var secondPackage = CreatePackage("n25-4a1-conflict-second-request", secondPrivateMarker);

        var first = await handler.HandleAsync(ImprovementLoopHttpRequest(
            SerializeLoopRequest("n25-4a1-conflict-loop", firstPackage, maxAttempts: 1, firstPrivateMarker),
            AdmissionHeaders(idempotencyKey: idempotencyKey)));
        var renderCountAfterFirst = renderProvider.Requests.Count;
        var comparisonCountAfterFirst = comparisonProvider.Requests.Count;
        var visualJudgeCountAfterFirst = visualJudge.Requests.Count;
        var strategyCountAfterFirst = strategy.Requests.Count;
        var conflict = await handler.HandleAsync(ImprovementLoopHttpRequest(
            SerializeLoopRequest("n25-4a1-conflict-loop-changed", secondPackage, maxAttempts: 1, secondPrivateMarker),
            AdmissionHeaders(idempotencyKey: idempotencyKey)));

        Assert.Equal(200, first.StatusCode);
        Assert.Equal(409, conflict.StatusCode);
        AssertError(conflict, "DESIGN_CONVERSION_IDEMPOTENCY_KEY_CONFLICT");
        Assert.Equal(renderCountAfterFirst, renderProvider.Requests.Count);
        Assert.Equal(comparisonCountAfterFirst, comparisonProvider.Requests.Count);
        Assert.Equal(visualJudgeCountAfterFirst, visualJudge.Requests.Count);
        Assert.Equal(strategyCountAfterFirst, strategy.Requests.Count);
        AssertNoPrivateMarkerLeak(conflict.BodyJson, firstPrivateMarker, firstPackage);
        AssertNoPrivateMarkerLeak(conflict.BodyJson, secondPrivateMarker, secondPackage);
        AssertResponseAndBoundaryMetadataDoNotLeak(
            conflict,
            RouteAdmissionCurrentToken,
            RouteAdmissionPreviousToken,
            RouteAdmissionTenantId,
            idempotencyKey,
            firstPrivateMarker,
            secondPrivateMarker,
            firstPackage.Html!,
            firstPackage.Css!,
            firstPackage.DesignTree!.Payload!,
            firstPackage.OriginalTree!.Payload!,
            secondPackage.Html!,
            secondPackage.Css!,
            secondPackage.DesignTree!.Payload!,
            secondPackage.OriginalTree!.Payload!);
    }

    [Fact]
    public void RegisterDesignConversionImprovementLoop_StoresExactRouteAdmissionForHandlerAndCompositionRegistrations()
    {
        var routeAdmission = CreateEnabledImprovementLoopAdmission();
        var directRegistry = new DesignConversionModelServerEndpointRegistry();
        var directHandler = CreateAdmissionBackedHandler(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(_ => 0.96),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0.01m)),
            new ScriptedCandidateStrategy(),
            routeAdmission);

        var directRegistered = directRegistry.RegisterDesignConversionImprovementLoopForLiveHostTesting(directHandler);
        var directResolved = directRegistry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);

        Assert.True(directRegistered.IsSuccess, directRegistered.ErrorMessage);
        Assert.True(directResolved.IsSuccess, directResolved.ErrorMessage);
        Assert.Same(routeAdmission, directRegistered.Value!.RouteAdmission);
        Assert.Same(routeAdmission, directResolved.Value!.RouteAdmission);

        var compositionAdmission = CreateEnabledImprovementLoopAdmission(tenantId: "tenant-improvement-loop-composition");
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocalForLiveHostTesting(
            comparisonProvider: new RecordingComparisonProvider(_ => 0.96),
            visualJudge: new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0.01m)));
        var compositionRegistry = new DesignConversionModelServerEndpointRegistry();
        var compositionRegistered = compositionRegistry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            new ScriptedCandidateStrategy(),
            routeAdmission: compositionAdmission);
        var compositionResolved = compositionRegistry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);

        Assert.True(compositionRegistered.IsSuccess, compositionRegistered.ErrorMessage);
        Assert.True(compositionResolved.IsSuccess, compositionResolved.ErrorMessage);
        Assert.Same(compositionAdmission, compositionRegistered.Value!.RouteAdmission);
        Assert.Same(compositionAdmission, compositionResolved.Value!.RouteAdmission);

        var artifactAdmission = CreateEnabledImprovementLoopAdmission(tenantId: "tenant-improvement-loop-artifact");
        var artifactRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "n25-4a1-registry");
        Directory.CreateDirectory(artifactRoot);
        try
        {
            var artifactRegistry = new DesignConversionModelServerEndpointRegistry();
            var artifactRegistered = artifactRegistry.RegisterDesignConversionImprovementLoopFromComposition(
                composition,
                new ScriptedCandidateStrategy(),
                new JsonlDesignConversionLocalArtifactDb(artifactRoot),
                utcNow: () => RouteAdmissionNow,
                routeAdmission: artifactAdmission);
            var artifactResolved = artifactRegistry.Resolve(
                DesignConversionModelServerEndpointRegistry.EndpointMethod,
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);

            Assert.True(artifactRegistered.IsSuccess, artifactRegistered.ErrorMessage);
            Assert.True(artifactResolved.IsSuccess, artifactResolved.ErrorMessage);
            Assert.Same(artifactAdmission, artifactRegistered.Value!.RouteAdmission);
            Assert.Same(artifactAdmission, artifactResolved.Value!.RouteAdmission);
        }
        finally
        {
            Directory.Delete(artifactRoot, recursive: true);
        }
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_RouteMissing_ReturnsStableRouteErrorEnvelope()
    {
        var registry = new DesignConversionModelServerEndpointRegistry();
        var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-route-missing-loop",
            InitialPackage: CreatePackage("pass129-route-missing-request", "PRIVATE_PASS129_ROUTE_SECRET"),
            MaxAttempts: 2);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 404,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal("DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND", envelope.ParsedErrorCode);
        Assert.Equal("endpoint_registry", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, envelope.EndpointPath);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CoordinatorNotConfigured_ReturnsStableErrorEnvelope()
    {
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopForLiveHostTesting(
            new DesignConversionModelServerImprovementLoopHandler(coordinator: null));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-not-configured-loop",
            InitialPackage: CreatePackage("pass129-not-configured-request", "PRIVATE_PASS129_CONFIG_SECRET"),
            MaxAttempts: 2);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 503,
            registryInvokeSucceeded: true,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.CoordinatorNotConfiguredErrorCode,
            envelope.ParsedErrorCode);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["improvement_loop_coordinator_configured"]);
    }

    private static void AssertStableImprovementLoopEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        int statusCode,
        bool registryInvokeSucceeded,
        bool runtimeInvoked)
    {
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion, envelope.EnvelopeSchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary, envelope.EnvelopeBoundary);
        Assert.Equal(DesignConversionModelServerLocalCallerAdapter.AdapterBoundary, envelope.AdapterBoundary);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointMethod, envelope.EndpointMethod);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, envelope.EndpointPath);
        Assert.True(
            envelope.StatusCode == statusCode,
            "Expected HTTP status " + statusCode +
            " but received " + envelope.StatusCode +
            ". Response: " + envelope.ResponseBodyJson);
        Assert.Equal(registryInvokeSucceeded, envelope.RegistryInvokeSucceeded);
        Assert.Equal(runtimeInvoked, envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.Null(envelope.ResponseJsonParseError);
        var improvementLoopRequestFields = DesignConversionModelServerCallerContract
            .DescribeImprovementLoopRoute()
            .RequestFields;
        var requiredImprovementLoopRequestFields = improvementLoopRequestFields
            .Where(field => field.Required)
            .Select(field => field.JsonName)
            .ToArray();
        Assert.Equal(requiredImprovementLoopRequestFields, envelope.RequiredRequestFields);
        Assert.Contains(improvementLoopRequestFields, field =>
            field is { JsonName: "loopId", Required: true });
        Assert.Contains(improvementLoopRequestFields, field =>
            field is { JsonName: "initialPackage", Required: true });
        Assert.False(envelope.RawHtmlIncluded);
        Assert.False(envelope.RawCssIncluded);
        Assert.False(envelope.RawDesignTreeIncluded);
        Assert.False(envelope.ResponseContainsRawPrivateData);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.False(envelope.ExternalCallerRepoTouched);
        Assert.False(envelope.CrossRepoCallerVerified);
        Assert.False(envelope.PublicHttpEndpointVerified);
        Assert.Equal(registryInvokeSucceeded.ToString(), envelope.BoundaryMetadata["registry_invoke_succeeded"]);
        Assert.Equal(runtimeInvoked.ToString(), envelope.BoundaryMetadata["runtime_invoked"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_caller_repo_touched"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["socket_opened"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);
    }

    private static void AssertNoPrivateMarkerLeak(
        string responseBodyJson,
        string privateMarker,
        DesignConversionModelServerLocalCallerPackage initialPackage)
    {
        Assert.DoesNotContain(privateMarker, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Html!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Css!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.DesignTree!.Payload!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.OriginalTree!.Payload!, responseBodyJson, StringComparison.Ordinal);
    }

    private static void AssertMaxTeacherCallsCapReached(JsonElement boundaryMetadata)
    {
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey).GetString());
        Assert.Equal("1", boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey).GetString());
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsContractBoundary,
            boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey).GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey).GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey).GetString());
        Assert.Equal("1", boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey).GetString());
        Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty(DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey).GetString());
    }

    private static void AssertEachAttemptRenderedTwoViewports(JsonElement root)
        => AssertEachAttemptRenderedTwoViewports(root.GetProperty("attempts").EnumerateArray().ToArray());

    private static void AssertEachAttemptRenderedTwoViewports(IReadOnlyList<JsonElement> attempts)
        => Assert.All(
            attempts,
            attempt => Assert.Equal(
                "2",
                attempt.GetProperty("publicMetadata").GetProperty("render_result_count").GetString()));

    private static void AssertSingleObservedTeacherCall(JsonElement root)
    {
        var observedTeacherCalls = root.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(reference => reference.GetString())
            .Where(reference => reference is not null &&
                reference.StartsWith(
                    DesignConversionImprovementLoopCoordinator.ObservedTeacherCallEvidenceRefPrefix,
                    StringComparison.Ordinal))
            .ToArray();

        var observed = Assert.Single(observedTeacherCalls);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.ObservedTeacherCallEvidenceRefPrefix + "1",
            observed);
    }

    private static string ExpectedMaxAttemptsExceededErrorMessage()
        => $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts.ToString(System.Globalization.CultureInfo.InvariantCulture)}.";

    private static DesignConversionModelServerRouteAdmission CreateEnabledImprovementLoopAdmission(
        string? tenantId = RouteAdmissionTenantId)
        => DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: RouteAdmissionCurrentToken,
                previousToken: RouteAdmissionPreviousToken,
                previousTokenExpiresAtUtc: RouteAdmissionNow.AddHours(1),
                idempotencyCapacity: 16,
                idempotencyTtl: TimeSpan.FromMinutes(30),
                tenantId: tenantId),
            () => RouteAdmissionNow);

    private static DesignConversionModelServerImprovementLoopHandler CreateAdmissionBackedHandler(
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge visualJudge,
        IDesignConversionImprovementCandidateStrategy strategy,
        DesignConversionModelServerRouteAdmission routeAdmission)
    {
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, strategy);
        return new DesignConversionModelServerImprovementLoopHandler(
            coordinator,
            routeAdmission: routeAdmission);
    }

    private static string SerializeLoopRequest(
        string loopId,
        DesignConversionModelServerLocalCallerPackage package,
        int maxAttempts,
        string privateMarker)
        => JsonSerializer.Serialize(
            new DesignConversionModelServerImprovementLoopRequest(
                LoopId: loopId,
                InitialPackage: package,
                MaxAttempts: maxAttempts,
                Metadata: new Dictionary<string, string>
                {
                    ["caller_private_marker"] = privateMarker
                }),
            DesignConversionModelServerHttpHandler.JsonOptions);

    private static DesignConversionModelServerHttpRequest ImprovementLoopHttpRequest(
        string bodyJson,
        IReadOnlyDictionary<string, string>? headers)
        => new(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: bodyJson,
            ContentType: "application/json",
            Headers: headers);

    private static IReadOnlyDictionary<string, string> AdmissionHeaders(
        string? token = RouteAdmissionCurrentToken,
        string? idempotencyKey = "n25-4a1-default-key")
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (token is not null)
            headers[DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = "Bearer " + token;
        if (idempotencyKey is not null)
            headers[DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] = idempotencyKey;

        return headers;
    }

    private static void AssertError(
        DesignConversionModelServerHttpResponse response,
        string errorCode,
        string? errorMessage = null)
    {
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(errorCode, root.GetProperty("errorCode").GetString());
        if (errorMessage is not null)
            Assert.Equal(errorMessage, root.GetProperty("errorMessage").GetString());
    }

    private static void AssertRuntimeNotInvoked(DesignConversionModelServerHttpResponse response)
        => Assert.Equal(
            bool.FalseString,
            BoundaryMetadata(response).GetProperty("runtime_invoked").GetString());

    private static JsonElement BoundaryMetadata(DesignConversionModelServerHttpResponse response)
    {
        using var document = JsonDocument.Parse(response.BodyJson);
        return document.RootElement.GetProperty("boundaryMetadata").Clone();
    }

    private static void AssertResponseAndBoundaryMetadataDoNotLeak(
        DesignConversionModelServerHttpResponse response,
        params string[] forbidden)
    {
        var metadataJson = BoundaryMetadata(response).GetRawText();
        foreach (var value in forbidden.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            Assert.DoesNotContain(value, response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(value, metadataJson, StringComparison.Ordinal);
        }
    }

    private static DesignConversionModelServerLocalCallerAdapter CreateAdapter(
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge visualJudge,
        IDesignConversionImprovementCandidateStrategy strategy,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        TimeSpan? runDeadline = null)
    {
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            service,
            strategy,
            artifactDb,
            runDeadline: runDeadline);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopForLiveHostTesting(
            new DesignConversionModelServerImprovementLoopHandler(coordinator));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return new DesignConversionModelServerLocalCallerAdapter(registry);
    }

    private static DesignConversionModelServerLocalCallerAdapter CreateAdapter(
        IDesignConversionThinClientJobEvaluator evaluator,
        IDesignConversionImprovementCandidateStrategy strategy,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        TimeSpan? runDeadline = null)
    {
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            strategy,
            artifactDb,
            runDeadline: runDeadline);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopForLiveHostTesting(
            new DesignConversionModelServerImprovementLoopHandler(coordinator));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return new DesignConversionModelServerLocalCallerAdapter(registry);
    }

    private static TeacherFixture CreateTeacherFixture(string name)
        => new(name);

    private static TeacherFixture CreateAuthoringTeacherFixture(string name)
        => new(name, supportTeacherAuthoring: true);

    private static RepositoryOwnedDpoPairFixture
        CreateRepositoryOwnedDpoProviderAllowedPair(string scope)
    {
        var chosen = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: true,
            jobId: scope + "-chosen-job",
            requestId: scope + "-chosen-request",
            outputId: scope + "-chosen-output",
            candidateId: scope + "-chosen-candidate",
            verdictId: scope + "-chosen-verdict",
            score: 0.96,
            passed: true,
            htmlSuffix: scope + "-chosen");
        var rejected = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: scope + "-rejected-job",
            requestId: scope + "-rejected-request",
            outputId: scope + "-rejected-output",
            candidateId: scope + "-rejected-candidate",
            verdictId: scope + "-rejected-verdict",
            score: 0.72,
            passed: false,
            htmlSuffix: scope + "-rejected");
        chosen = chosen with
        {
            VisualQaBoundaryTrace = chosen.VisualQaBoundaryTrace with
            {
                TierPolicy = chosen.VisualQaBoundaryTrace.TierPolicy with
                {
                    ShouldContinue = false,
                    StopReason =
                        DesignConversionVisualQaBoundaryPolicy.QualityReached
                }
            }
        };
        rejected = rejected with
        {
            VisualQaBoundaryTrace = rejected.VisualQaBoundaryTrace with
            {
                TierPolicy = rejected.VisualQaBoundaryTrace.TierPolicy with
                {
                    ShouldContinue = true,
                    StopReason =
                        DesignConversionVisualQaBoundaryPolicy.ContinueImprovement
                },
                VisualDecision =
                    rejected.VisualQaBoundaryTrace.VisualDecision with
                    {
                        CanRetry = true,
                        Decision = "retry"
                    }
            }
        };

        // Серверный тест проверяет не пиксельный движок, а сохранение DPO.
        // Поэтому входная пара обязана выглядеть как уже независимо измеренная:
        // production сможет её принять, но сам dataset всё равно появится
        // только через первый live handler call и fake open-Qwen teacher.
        return new RepositoryOwnedDpoPairFixture(
            WithVisualJudgeTrainingAllowed(chosen),
            WithVisualJudgeTrainingAllowed(rejected));
    }

    private static DesignConversionInternalEvaluationResult
        WithVisualJudgeTrainingAllowed(
            DesignConversionInternalEvaluationResult evaluation)
    {
        const string providerReady = "visual_training_allowed";
        return evaluation with
        {
            VisualJudgeResults = evaluation.VisualJudgeResults
                .Select(judge => judge with
                {
                    Status = providerReady,
                    IsBlocked = false,
                    Provider = judge.Provider with
                    {
                        ExternalCallExecuted = true
                    },
                    Metadata = new Dictionary<string, string>(
                        judge.Metadata,
                        StringComparer.Ordinal)
                    {
                        ["visual_training_provider_allowed"] =
                            bool.TrueString
                    }
                })
                .ToArray(),
            VisualQaBoundaryTrace = evaluation.VisualQaBoundaryTrace with
            {
                VisualJudgeStatus = providerReady,
                VisualJudgeProviderBoundary = providerReady,
                ExternalAiVisualJudgeExecuted = true,
                VisualDecision =
                    evaluation.VisualQaBoundaryTrace.VisualDecision with
                    {
                        ProviderBoundaryStatus = providerReady,
                        ExternalAiVisualJudgeExecuted = true
                    }
            }
        };
    }

    private static DesignConversionModelServerLocalCallerPackage CreatePackage(
        string requestId,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{requestId}:design-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"generated\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{requestId}:original-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"original\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            Html: $"<main data-secret=\"{privateMarker}\">Pass129</main>",
            Css: $".pass129{{background-image:url('{privateMarker}');}}",
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
                Tier: "pass129-local-loop",
                MaxImprovementCandidates: 5,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90",
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

    private static DesignConversionModelServerLocalCallerPackage CreatePass163Package(
        string requestId,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass163-same-scenario-design-tree",
                Payload: $"{{\"node\":\"frame\",\"secret\":\"{privateMarker}\",\"source\":\"pass163\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["source_kind"] = "figma",
                    ["local_only"] = bool.TrueString
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass163-same-scenario-original-tree",
                Payload: $"{{\"node\":\"frame\",\"secret\":\"{privateMarker}\",\"source\":\"original\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["source_kind"] = "figma",
                    ["local_only"] = bool.TrueString
                }),
            Html: $"<main class=\"pass163-card\" data-secret=\"{privateMarker}\">Pass163</main>",
            Css: $".pass163-card{{display:grid;gap:12px;background:url('{privateMarker}');}}",
            Screenshots:
            [
                CreateArtifact($"{requestId}:source-desktop", "screenshot", 1440, 900),
                CreateArtifact($"{requestId}:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                CreateArtifact($"{requestId}:target-desktop", "design-render", 1440, 900),
                CreateArtifact($"{requestId}:target-mobile", "design-render", 390, 844)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "pass163-local-user",
                Tier: "pass163-local-no-provider",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>
                {
                    ["local_only"] = bool.TrueString,
                    ["no_external_provider"] = bool.TrueString,
                    ["cloud_upload"] = bool.FalseString,
                    ["deployment_verified"] = bool.FalseString,
                    ["product_ready"] = bool.FalseString
                }),
            Metadata: new Dictionary<string, string>
            {
                ["pass163_same_scenario_id"] = "pass163-local-chain",
                ["local_only"] = bool.TrueString,
                ["no_external_provider"] = bool.TrueString,
                ["cloud_upload"] = bool.FalseString,
                ["deployment_verified"] = bool.FalseString,
                ["product_ready"] = bool.FalseString
            });

    private static DesignConversionTrainingPipelineOptions CreatePass163BlockedPromotionOptions()
        => new(
            RunId: "pass163-blocked-local-promotion",
            BatchSize: 2,
            MeasurementGateRequest: new DesignConversionMeasurementGateRequest(
                GateId: "pass163-blocked-local-promotion-admission",
                UnitId: DesignConversionDpoUnits.CriticUnit,
                HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                    1,
                    0.95,
                    10,
                    0.01m,
                    64,
                    [
                        "artifact://measurement/pass163/heldout-a.json",
                        "artifact://measurement/pass163/heldout-b.json"
                    ]),
                IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                    true,
                    false,
                    false,
                    true,
                    [
                        "artifact://measurement/pass163/integrity-a.json",
                        "artifact://measurement/pass163/integrity-b.json"
                    ]),
                Thresholds: new DesignConversionMeasurementGateThresholds(
                    0.90,
                    100,
                    1m,
                    512)));

    private static DesignConversionModelServerLocalCallerPackage CreatePass164Package(
        string requestId,
        string sourceSlug,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{sourceSlug}-design-tree",
                Payload: $"{{\"node\":\"frame\",\"source\":\"{sourceSlug}\",\"layout\":\"runtime-loop\",\"secret\":\"{privateMarker}\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["source_kind"] = "figma",
                    ["source_family"] = sourceSlug,
                    ["local_only"] = bool.TrueString
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{sourceSlug}-original-tree",
                Payload: $"{{\"node\":\"frame\",\"source\":\"{sourceSlug}-original\",\"layout\":\"runtime-loop\",\"secret\":\"{privateMarker}\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["source_kind"] = "figma",
                    ["source_family"] = sourceSlug,
                    ["local_only"] = bool.TrueString
                }),
            Html: $"<main class=\"pass164-card-{sourceSlug}\" data-secret=\"{privateMarker}\">Pass164 {sourceSlug}</main>",
            Css: $".pass164-card-{sourceSlug}{{display:grid;gap:12px;background:url('{privateMarker}');}}",
            Screenshots:
            [
                CreateArtifact($"{sourceSlug}:source-desktop", "screenshot", 1440, 900),
                CreateArtifact($"{sourceSlug}:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                CreateArtifact($"{sourceSlug}:target-desktop", "design-render", 1440, 900),
                CreateArtifact($"{sourceSlug}:target-mobile", "design-render", 390, 844)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "pass164-local-user",
                Tier: "pass164-local-no-provider",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>
                {
                    ["source_family"] = sourceSlug,
                    ["local_only"] = bool.TrueString,
                    ["no_external_provider"] = bool.TrueString,
                    ["cloud_upload"] = bool.FalseString,
                    ["deployment_verified"] = bool.FalseString,
                    ["product_ready"] = bool.FalseString
                }),
            Metadata: new Dictionary<string, string>
            {
                ["pass164_source_disjoint_probe"] = bool.TrueString,
                ["source_family"] = sourceSlug,
                ["local_only"] = bool.TrueString,
                ["no_external_provider"] = bool.TrueString,
                ["cloud_upload"] = bool.FalseString,
                ["deployment_verified"] = bool.FalseString,
                ["product_ready"] = bool.FalseString
            });

    private static DesignConversionModelServerRepositoryOwnedTrainingOptions
        CreateRepositoryOwnedTrainingOptions(
            string repositoryRoot,
            string datasetVersion = "route-context-v1")
        => new(
            new DesignConversionRepositoryOwnedTrainingRunContextSpec(
                Path.GetFullPath(repositoryRoot).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar),
                DesignConversionRepositoryOwnedDpoTransactionContextFactory
                    .FixedDomainSlug,
                DesignConversionRepositoryOwnedDpoTransactionContextFactory
                    .FixedDesignPatternSlug,
                datasetVersion,
                "qwen-test-9b",
                "model-v1",
                new RepositoryOwnedModelRuntimeCompatibility(
                    new string('1', 64),
                    new string('2', 64),
                    new string('3', 64),
                    new string('4', 64),
                    new string('5', 64),
                    "design-conversion-test-runtime-v1"),
                AllowColdStart: false,
                ColdStartReason: null,
                ProvenanceRefs:
                [
                    "sha256:" + new string('6', 64)
                ]));

    private static ContinuousLearningTp1DpoConfiguration
        CreateRepositoryOwnedDpoTeacherConfiguration()
    {
        var created = ContinuousLearningTp1DpoConfiguration.Create(
            ExpectedTeacherModelId,
            ExpectedTeacherManifestDigest,
            ExpectedTeacherModelLayerDigest,
            maxTeacherCalls: 4,
            localRetentionPolicyRef: "sha256:" + new string('7', 64),
            outputLicenseRef: "sha256:" + new string('8', 64),
            serverAuditHashSalt:
                Enumerable.Range(1, 32)
                    .Select(static value => (byte)value)
                    .ToArray());
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return created.Value!;
    }

    private static DesignConversionRepositoryOwnedDpoTransactionContext
        CreateExpectedServerOwnedRepositoryDpoContext(
            string repositoryRoot,
            string datasetVersion,
            DesignConversionModelServerLocalCallerPackage package,
            ContinuousLearningTp1DpoConfiguration teacherConfiguration)
    {
        var options = CreateRepositoryOwnedTrainingOptions(
            repositoryRoot,
            datasetVersion);
        var spec = options.RunContextSpec!;
        var paidReference = DesignConversionPaidReferenceMaterial.Create(
            package.Html ?? string.Empty,
            package.Css ?? string.Empty);
        Assert.True(paidReference.IsSuccess, paidReference.ErrorMessage);

        var authoringProfile =
            LocalQwenOllamaInferenceProfileAuthority.Authoring;
        var openTeacher = new RepositoryOwnedDpoOpenTeacherIdentity(
            OpenWeightTeacherFamily.Qwen.ToString(),
            teacherConfiguration.ModelId,
            teacherConfiguration.ExpectedOllamaManifestDigest,
            LegalTrainingTeacherAdmissionAuthority
                .OllamaModelLayerSha256DigestKind,
            teacherConfiguration.ExpectedOllamaModelLayerDigest);
        var policyConfigSha256 = ComputeRepositoryOwnedDpoIdentitySha256(
            "design-conversion-repository-owned-dpo-generation-policy-v1",
            authoringProfile.ProfileId,
            authoringProfile.ConfigurationSha256,
            spec.DomainSlug,
            spec.DesignPatternSlug,
            spec.DatasetVersion,
            teacherConfiguration.MaxTeacherCalls.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        var creatorConfigSha256 = ComputeRepositoryOwnedDpoIdentitySha256(
            "modelserver-design-conversion-improvement-loop-dpo-authoring-v1",
            DesignConversionModelServerImprovementLoopHandler.EndpointPath,
            openTeacher.ModelId,
            openTeacher.ModelVersion,
            openTeacher.ModelArtifactDigestKind,
            openTeacher.ModelArtifactSha256,
            policyConfigSha256,
            spec.ModelIdSlug,
            spec.ModelVersion);
        var context = DesignConversionRepositoryOwnedDpoTransactionContextFactory
            .Create(
                new DesignConversionRepositoryOwnedDpoTransactionContextFactoryConfiguration(
                    TrustedRepositoryRoot: spec.RepositoryRoot,
                    AssertedRepositoryRoot: spec.RepositoryRoot,
                    DomainSlug: spec.DomainSlug,
                    DesignPatternSlug: spec.DesignPatternSlug,
                    DatasetVersion: spec.DatasetVersion,
                    OpenTeacher: openTeacher,
                    GenerationPolicy: new RepositoryOwnedDpoGenerationPolicyIdentity(
                        "design-conversion-local-qwen-dpo-generation-policy",
                        "v1",
                        policyConfigSha256),
                    Creator: new RepositoryOwnedDpoCreatorIdentity(
                        "modelserver-design-conversion-improvement-loop",
                        "v1",
                        creatorConfigSha256),
                    PaidReferenceMaterials:
                    [
                        paidReference.Value!
                    ]));
        Assert.True(context.IsSuccess, context.ErrorMessage);
        return context.Value!;
    }

    private static string ComputeRepositoryOwnedDpoIdentitySha256(
        params string[] parts)
        => Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(string.Join('\n', parts))))
            .ToLowerInvariant();

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    private static void AssertRepositoryOwnedDpoEvidence(
        JsonElement root,
        RepositoryOwnedDpoDatasetResolutionSource expectedSource,
        int expectedPairCount,
        string responseBodyJson)
    {
        var sourceText = expectedSource.ToString();
        var pairCountText = expectedPairCount.ToString(
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(
            sourceText,
            AssertBoundaryMetadataOrEvidenceValue(
                root,
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetResolutionSourceMetadataKey,
                sourceText,
                responseBodyJson));
        Assert.Equal(
            pairCountText,
            AssertBoundaryMetadataOrEvidenceValue(
                root,
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetPairCountMetadataKey,
                pairCountText,
                responseBodyJson));
        Assert.Equal(
            bool.FalseString,
            AssertBoundaryMetadataOrEvidenceValue(
                root,
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey,
                bool.FalseString,
                responseBodyJson));
        AssertEvidenceContains(
            root,
            "improvement_loop:" +
            DesignConversionImprovementLoopCoordinator
                .RepositoryOwnedDpoDatasetResolutionSourceMetadataKey +
            "=" + sourceText,
            responseBodyJson);
        AssertEvidenceContains(
            root,
            "improvement_loop:" +
            DesignConversionImprovementLoopCoordinator
                .RepositoryOwnedDpoDatasetPairCountMetadataKey +
            "=" + pairCountText,
            responseBodyJson);
        AssertEvidenceContains(
            root,
            "improvement_loop:" +
            DesignConversionImprovementLoopCoordinator
                .RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey +
            "=" + bool.FalseString,
            responseBodyJson);
    }

    private static string AssertBoundaryMetadataValue(
        JsonElement root,
        string metadataKey,
        string responseBodyJson)
    {
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.True(
            metadata.TryGetProperty(metadataKey, out var value),
            "Missing boundary metadata '" + metadataKey + "'. Response: " +
            responseBodyJson);
        return value.GetString() ?? string.Empty;
    }

    private static string AssertBoundaryMetadataOrEvidenceValue(
        JsonElement root,
        string metadataKey,
        string expectedValue,
        string responseBodyJson)
    {
        var metadata = root.GetProperty("boundaryMetadata");
        if (metadata.TryGetProperty(metadataKey, out var value))
            return value.GetString() ?? string.Empty;

        AssertEvidenceContains(
            root,
            "improvement_loop:" + metadataKey + "=" + expectedValue,
            responseBodyJson);
        return expectedValue;
    }

    private static void AssertEvidenceContains(
        JsonElement root,
        string expectedReference,
        string responseBodyJson)
    {
        var references = ReadEvidenceRefs(root);
        Assert.True(
            references.Contains(expectedReference, StringComparer.Ordinal),
            "Missing evidence ref '" + expectedReference + "'. Response: " +
            responseBodyJson);
    }

    private static void AssertFirstCandidateStatusDoesNotHideDpoFailure(
        JsonElement root,
        string responseBodyJson)
    {
        if (!root.GetProperty("candidateStrategyFailed").GetBoolean())
            return;

        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.CandidateStrategyFailed,
            root.GetProperty("stopReason").GetString());
        Assert.DoesNotContain(
            root.GetProperty("warnings")
                .EnumerateArray()
                .Select(warning => warning.GetString() ?? string.Empty),
            warning => warning.StartsWith(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetResolutionFailedWarning,
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            "improvement_loop:" +
            DesignConversionImprovementLoopCoordinator
                .RepositoryOwnedDpoDatasetResolutionSourceMetadataKey +
            "=unavailable",
            ReadEvidenceRefs(root));
        Assert.True(
            root.GetProperty("attemptCount").GetInt32() > 0,
            "candidateStrategyFailed=true is accepted only after the DPO " +
            "contract is already proven and the failure belongs to later " +
            "bounded candidate work. Response: " + responseBodyJson);
    }

    private static IReadOnlyList<string> ReadEvidenceRefs(JsonElement root)
        => root.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(reference => reference.GetString() ?? string.Empty)
            .ToArray();

    private static void AssertRepositoryOwnedDpoPairsCarryCanonicalTeacherProvenance(
        RepositoryOwnedDpoDatasetOpenResult reopened)
    {
        Assert.All(
            reopened.Pairs,
            pair =>
            {
                Assert.Equal("figma-html-css", pair.Domain);
                Assert.False(string.IsNullOrWhiteSpace(pair.Prompt));
                Assert.False(string.IsNullOrWhiteSpace(pair.Chosen));
                Assert.False(string.IsNullOrWhiteSpace(pair.Rejected));
                Assert.NotNull(pair.TeacherProvenance);
                var teacher = pair.TeacherProvenance!;
                Assert.Equal(
                    TrainingTeacherSourceKind.ExternalTeacherOutput,
                    teacher.SourceKind);
                Assert.Equal(OpenWeightTeacherFamily.Qwen, teacher.Family);
                Assert.Equal(
                    TrainingTeacherAccessMode.LocalHostedOpenWeight,
                    teacher.AccessMode);
                Assert.Equal(
                    TrainingTeacherHostingProvider.Ollama,
                    teacher.HostingProvider);
                Assert.Equal(ExpectedTeacherModelId, teacher.ModelId);
                Assert.Equal(ExpectedTeacherManifestDigest, teacher.ModelVersion);
                Assert.Equal(
                    TrainingTeacherModelLicense.Apache20,
                    teacher.ModelLicense);
                Assert.True(teacher.TeacherOutputUsedForTraining);
                Assert.StartsWith(
                    "repository-owned-dpo-child:sha256:",
                    teacher.RequestId,
                    StringComparison.Ordinal);
            });
        Assert.Equal(
            reopened.Pairs.Count,
            reopened.Pairs
                .Select(pair => pair.TeacherProvenance!.RequestId)
                .Distinct(StringComparer.Ordinal)
                .Count());
    }

    private static IReadOnlyDictionary<string, string>
        CaptureRegularFileSha256ByRepositoryRelativePath(
            string repositoryRoot,
            string datasetRoot)
        => Directory.GetFiles(datasetRoot, "*", SearchOption.AllDirectories)
            .Where(File.Exists)
            .Order(StringComparer.Ordinal)
            .ToDictionary(
                path => Path.GetRelativePath(repositoryRoot, path)
                    .Replace('\\', '/'),
                path => Convert.ToHexString(
                        SHA256.HashData(File.ReadAllBytes(path)))
                    .ToLowerInvariant(),
                StringComparer.Ordinal);

    private static void AssertRegularFileSnapshotsUnchanged(
        IReadOnlyDictionary<string, string> expected,
        IReadOnlyDictionary<string, string> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        foreach (var pair in expected.OrderBy(
                     item => item.Key,
                     StringComparer.Ordinal))
        {
            Assert.True(
                actual.TryGetValue(pair.Key, out var actualSha256),
                "Missing sealed dataset file after reuse: " + pair.Key);
            Assert.Equal(pair.Value, actualSha256);
        }
    }

    private static async Task AssertNoLocalDpoCandidateWasWrittenAsync(
        IDesignConversionLocalArtifactDb artifactDb)
    {
        var records = await artifactDb.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Equal(
            0,
            records.Value!.Sum(record => record.DpoCandidates.Count));
    }

    private static string CreateTemporaryRepositoryRoot(string name)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-repository-owned-route-" +
            name +
            "-" +
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(
            Path.Combine(root, "LlmFromScratch.sln"),
            "DesignConversion repository-owned DPO route fixture",
            Encoding.UTF8);
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        return root;
    }

    private static void DeleteTemporaryDirectory(string directory)
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
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
            Uri: $"artifact://pass129/{id}.png",
            Sha256: $"sha256:{id}");

    private static IReadOnlyList<DesignConversionModelServerArtifactRef> CreateRequiredBrowserQaArtifacts(
        string idPrefix,
        string kind)
        =>
        [
            CreateArtifact($"{idPrefix}-mobile-360", kind, 360, 800),
            CreateArtifact($"{idPrefix}-tablet-768", kind, 768, 1024),
            CreateArtifact($"{idPrefix}-laptop-1280", kind, 1280, 720),
            CreateArtifact($"{idPrefix}-desktop-1920", kind, 1920, 1080)
        ];

    private static async Task<Pass164RRuntimeCandidateTrace> CreatePass164RRuntimeCandidateAsync(
        JsonlDesignConversionLocalArtifactDb artifactDb,
        DesignConversionDpoUsefulnessApi usefulnessApi,
        DesignConversionModelServerLocalCallerAdapter adapter,
        int sourceOrdinal,
        string privateMarker,
        bool approveImmediately)
    {
        var sourceSlug = $"pass164r-source-{sourceOrdinal:000}";
        var requestId = $"pass164r-runtime-request-{sourceOrdinal:000}";
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: $"pass164r-runtime-loop-{sourceOrdinal:000}",
            InitialPackage: CreatePass164Package(requestId, sourceSlug, privateMarker),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["pass164r_count_gate_probe"] = bool.TrueString,
                ["source_family"] = sourceSlug,
                ["local_only"] = bool.TrueString,
                ["no_external_provider"] = bool.TrueString
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);
        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        AssertPass164RTwoAttemptEnvelope(envelope, package.LoopId, requestId);

        var records = await artifactDb.ListRecordsAsync(
            new DesignConversionLocalArtifactDbQuery(
                JobId: $"modelserver-improvement-loop:{package.LoopId}:{requestId}:loop-attempt:2"));
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        var candidates = stored.DpoCandidates.ToArray();
        Assert.Collection(
            candidates,
            candidate => Assert.Equal(DesignConversionDpoUnits.CriticUnit, candidate.UnitId),
            candidate => Assert.Equal(DesignConversionDpoUnits.ImproverUnit, candidate.UnitId));
        Assert.Equal(2, candidates.Select(candidate =>
                candidate.TrainingRecord.Provenance.Metadata[
                    DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey])
            .Distinct(StringComparer.Ordinal)
            .Count());
        Assert.All(candidates, candidate =>
        {
            var metadata = candidate.TrainingRecord.Provenance.Metadata;
            Assert.StartsWith("sha256:", candidate.TrainingRecord.Provenance.SourceManifestRef, StringComparison.Ordinal);
            Assert.Equal(
                candidates[0].TrainingRecord.Provenance.SourceManifestRef,
                candidate.TrainingRecord.Provenance.SourceManifestRef);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, metadata["review_status"]);
            Assert.Equal(bool.FalseString, metadata["approved_for_training"]);
            Assert.Equal(bool.FalseString, metadata["training_grade"]);
            Assert.False(metadata.ContainsKey("runtime_teacher_selected_preference"));
            Assert.Equal(bool.TrueString, metadata["runtime_teacher_authored_candidate"]);
            Assert.Equal(bool.TrueString, metadata["independent_preference_proven"]);
            Assert.Equal(bool.FalseString, metadata["raw_teacher_prompt_persisted"]);
            Assert.Equal(bool.FalseString, metadata["raw_teacher_output_persisted"]);
            Assert.Equal(bool.FalseString, metadata["raw_teacher_rationale_persisted"]);
            Assert.Null(candidate.TrainingRecord.PayloadCustodySecret);
        });
        AssertPass164RDeterministicRenderBoundary(stored.RenderArtifactRefs);
        Assert.All(candidates, candidate =>
        {
            AssertRuntimeQualityCreatedDpoPair(candidate.TrainingRecord);
            AssertPass164RRuntimeQualityScores(candidate.TrainingRecord);
            AssertPass164RRuntimeTeacherAttestedCandidate(candidate);
        });

        if (approveImmediately)
        {
            foreach (var candidate in candidates)
            {
                var approval = await usefulnessApi.ApproveRuntimeCreatedDpoCandidateAsync(
                    stored.RecordId,
                    candidate.CandidateId,
                    "reviewer:pass164r-local",
                    $"Pass164R review-only teacher pair approval for source ordinal {sourceOrdinal:000}; corpus admission remains required",
                    DateTimeOffset.UnixEpoch.AddMinutes(164).AddSeconds(sourceOrdinal));
                Assert.True(approval.IsSuccess, approval.ErrorMessage);
                Assert.True(approval.Value!.ApprovedForTraining);
                Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus, approval.Value.ApprovalStatus);
            }
        }

        return new Pass164RRuntimeCandidateTrace(
            Stored: stored,
            CandidateIds: candidates.Select(candidate => candidate.CandidateId).ToArray(),
            SourceManifestRef: candidates[0].TrainingRecord.Provenance.SourceManifestRef,
            SourceStructurePayload: candidates[0].TrainingRecord.SourceStructurePayload);
    }

    private static void AssertPass164RTwoAttemptEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        string expectedLoopId,
        string expectedRequestId)
    {
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        Assert.Equal(expectedRequestId, envelope.ParsedRequestId);
        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var responseRoot = document.RootElement;
        Assert.Equal(expectedLoopId, responseRoot.GetProperty("loopId").GetString());
        Assert.Equal(expectedRequestId, responseRoot.GetProperty("requestId").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, responseRoot.GetProperty("stopReason").GetString());
        Assert.True(responseRoot.GetProperty("completed").GetBoolean());
        Assert.True(responseRoot.GetProperty("qualityReached").GetBoolean());
        Assert.Equal(2, responseRoot.GetProperty("attemptCount").GetInt32());
        var attempts = responseRoot.GetProperty("attempts").EnumerateArray().ToArray();
        Assert.Equal(2, attempts.Length);
        Assert.Equal(1, attempts[0].GetProperty("attemptNumber").GetInt32());
        Assert.Equal(2, attempts[1].GetProperty("attemptNumber").GetInt32());
    }

    private static void AssertPass164RDeterministicRenderBoundary(
        IReadOnlyList<DesignConversionRenderArtifactDbRef> renderArtifactRefs)
    {
        Assert.Equal(2, renderArtifactRefs.Count);
        Assert.Equal(
            ["source-1440x900", "source-390x844"],
            renderArtifactRefs.Select(artifact => artifact.ViewportId).Order(StringComparer.Ordinal).ToArray());
        Assert.All(renderArtifactRefs, artifact =>
        {
            Assert.StartsWith("artifact://design-conversion/render-scaffold/", artifact.UriRef, StringComparison.Ordinal);
            Assert.StartsWith("sha256:", artifact.Sha256, StringComparison.Ordinal);
            Assert.Equal(DeterministicDesignConversionRenderProvider.ProviderKind, artifact.Metadata["provider_kind"]);
            Assert.Equal(
                DesignConversionRenderEvidenceTrustBoundary.DeterministicOrScaffoldClassification,
                artifact.Metadata[DesignConversionRenderEvidenceTrustBoundary.TrustClassificationKey]);
            Assert.Equal(bool.FalseString, artifact.Metadata["visual_fidelity_claimed"]);
            Assert.Equal(bool.FalseString, artifact.Metadata["browser_or_playwright_used"]);
            Assert.Equal(bool.FalseString, artifact.Metadata[DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey]);
            Assert.Equal(bool.FalseString, artifact.Metadata[DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey]);
            Assert.Equal(bool.TrueString, artifact.Metadata["scaffold_render_evidence_server_canonicalized"]);
            Assert.Equal("metadata_only_render_evidence", artifact.Metadata["hidden_determinism_boundary"]);
            Assert.False(artifact.Metadata.ContainsKey("raw_html_included"));
            Assert.False(artifact.Metadata.ContainsKey("raw_css_included"));
        });
    }

    private static void AssertPass164RRuntimeQualityScores(DesignConversionDpoTrainingRecord record)
    {
        var expectedWarnings = new[]
        {
            "DETERMINISTIC_RENDER_SCAFFOLD_DOES_NOT_CLAIM_VISUAL_FIDELITY",
            "DETERMINISTIC_RENDER_SCAFFOLD_NOT_BROWSER_SCREENSHOT",
            "SERVER_OWNED_SCAFFOLD_RENDER_PROVENANCE_CANONICALIZED"
        };
        Assert.Equal(0.97, record.Chosen.Verdict.Score);
        Assert.Equal(0.72, record.Rejected.Verdict.Score);
        Assert.Equal(2, record.Chosen.QaResults.Count);
        Assert.Equal(2, record.Rejected.QaResults.Count);
        Assert.All(record.Chosen.QaResults, result =>
        {
            Assert.Equal(0.97, result.Verdict.Score);
            Assert.True(result.Verdict.Passed);
            Assert.Equal(expectedWarnings, result.Warnings.Order(StringComparer.Ordinal).ToArray());
        });
        Assert.All(record.Rejected.QaResults, result =>
        {
            Assert.Equal(0.72, result.Verdict.Score);
            Assert.False(result.Verdict.Passed);
            Assert.Equal(expectedWarnings, result.Warnings.Order(StringComparer.Ordinal).ToArray());
        });
    }

    private static void AssertPass164RRuntimeTeacherAttestedCandidate(
        DesignConversionLocalArtifactDbDpoCandidate candidate)
    {
        var teacherProvenance = candidate.TrainingRecord.TeacherProvenance;
        Assert.NotNull(teacherProvenance);
        Assert.Equal(TrainingTeacherSourceKind.ExternalTeacherOutput, teacherProvenance!.SourceKind);
        Assert.NotEqual(TrainingTeacherSourceKind.DeterministicNoExternalTeacher, teacherProvenance.SourceKind);
        Assert.Equal(OpenWeightTeacherFamily.Qwen, teacherProvenance.Family);
        Assert.Equal(TrainingTeacherAccessMode.LocalHostedOpenWeight, teacherProvenance.AccessMode);
        Assert.Equal(TrainingTeacherHostingProvider.Ollama, teacherProvenance.HostingProvider);
        Assert.Equal(ExpectedTeacherModelId, teacherProvenance.ModelId);
        Assert.Equal(ExpectedTeacherManifestDigest, teacherProvenance.ModelVersion);
        Assert.Equal(TrainingTeacherModelLicense.Apache20, teacherProvenance.ModelLicense);
        Assert.False(teacherProvenance.ClosedProprietaryModel);
        Assert.True(teacherProvenance.TeacherOutputUsedForTraining);
        Assert.StartsWith("teacher-test-authoring-", teacherProvenance.RequestId, StringComparison.Ordinal);
        Assert.Equal(1, teacherProvenance.InvocationCount);
        Assert.Equal(
            DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
            candidate.TrainingRecord.Provenance.Metadata["teacher_authoring_schema"]);
        var teacherAuthoringOutputHash = candidate.TrainingRecord.Provenance.Metadata[
            "teacher_authoring_output_hash"];
        Assert.Equal(64, teacherAuthoringOutputHash.Length);
        Assert.All(teacherAuthoringOutputHash, character => Assert.True(
            character is >= '0' and <= '9' or >= 'a' and <= 'f'));
    }

    private static DesignConversionImprovementCandidate CreateCandidate(
        string candidateId,
        string privateMarker)
        => new(
            CandidateId: candidateId,
            RequestId: $"{candidateId}-request",
            Html: $"<main data-secret=\"{privateMarker}\">Improved</main>",
            Css: $".improved-pass129{{color:'{privateMarker}';}}",
            ChangeReasons: ["scripted local improvement"],
            EvidenceRefs: [$"candidate:{candidateId}"],
            Metadata: new Dictionary<string, string>
            {
                ["private_marker"] = privateMarker
            });

    private static double ServerOwnedTierQualityThreshold(string tier)
        => tier switch
        {
            "free" => DesignConversionVisualQaBoundaryPolicy.ProvisionalFreeTierQualityThreshold,
            "paid" => DesignConversionVisualQaBoundaryPolicy.ProvisionalPaidTierQualityThreshold,
            "pro" => DesignConversionVisualQaBoundaryPolicy.ProvisionalProTierQualityThreshold,
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unsupported server-owned tier.")
        };

    private static string QualityThresholdText(double threshold)
        => threshold.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    private static string QualityThresholdRoundTripText(double threshold)
        => threshold.ToString("R", System.Globalization.CultureInfo.InvariantCulture);

    private static void AssertRuntimeQualityCreatedDpoPair(DesignConversionDpoTrainingRecord record)
    {
        Assert.Contains(
            record.UnitId,
            new[] { DesignConversionDpoUnits.CriticUnit, DesignConversionDpoUnits.ImproverUnit });
        Assert.True(record.Chosen.Accepted);
        Assert.False(record.Rejected.Accepted);
        Assert.NotEqual(record.Chosen.CandidateId, record.Rejected.CandidateId);
        Assert.True(record.Chosen.Verdict.Score > record.Rejected.Verdict.Score);
        Assert.True(record.UsefulnessScore > 0);
        Assert.Contains("runtime_created_dpo_candidate", record.Provenance.Metadata.Keys);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["review_required"]);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["approval_required"]);
        Assert.True(record.Rejected.EvidenceRefs.Count + record.Rejected.QaResults.Count > 0);
        Assert.False(record.Quarantine.IsQuarantined);
    }

    private sealed class RecordingRenderProvider : IDesignConversionRenderProvider
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
                Uri: $"artifact://pass129/render/{request.RenderRequestId}.png",
                Sha256: $"sha256:{request.RenderRequestId}:actual",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 1,
                Warnings: [])));
        }
    }

    private sealed class BlockingRenderProvider : IDesignConversionRenderProvider
    {
        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return OperationResult<DesignConversionRenderResult>.Fail(
                "Blocking render provider unexpectedly completed.",
                "BLOCKING_RENDER_PROVIDER_COMPLETED");
        }
    }

    private sealed class FailingEvaluationService(string errorCode) : IDesignConversionThinClientJobEvaluator
    {
        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "Scripted evaluator failure for improvement-loop telemetry tests.",
                errorCode));
        }
    }

    private sealed class RecordingComparisonProvider(Func<DesignConversionQaComparisonRequest, double> resolveScore)
        : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var score = resolveScore(request);
            var passed = score >= 0.90;
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:verdict",
                Score: score,
                Defects: passed
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.ComparisonRequestId}:defect",
                            Category: "spacing",
                            Severity: "minor",
                            Description: "Scripted comparison provider for Pass129 ModelServer improvement-loop tests: elements are too close and not enough padding.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds: [request.Expected.ImageId, request.Actual.ImageId])
                    ],
                EvidenceRefs: [$"comparison:{request.ComparisonRequestId}"],
                Passed: passed);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://pass129/diff/{request.ComparisonRequestId}.png",
                DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                Metrics: new Dictionary<string, double>
                {
                    ["visual_distance"] = 1.0 - score
                },
                EvidenceRefs: [$"comparison-artifact:{request.ComparisonRequestId}"])));
        }
    }

    private sealed class ScriptedVisualJudge(Func<DesignConversionVisualJudgeRequest, JudgeScript> resolve)
        : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var script = resolve(request);
            var passed = !script.Blocked && script.Score >= 0.90;
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:scripted-verdict",
                Score: script.Score,
                Defects: passed
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.JudgeRequestId}:scripted-defect",
                            Category: script.Blocked ? "provider" : "quality",
                            Severity: script.Blocked ? "blocker" : "minor",
                            Description: "Scripted visual judge result for Pass129 ModelServer improvement-loop tests.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
                    ],
                EvidenceRefs: [$"visual-judge:{request.JudgeRequestId}"],
                Passed: passed);
            var status = script.Blocked
                ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                : "SCRIPTED_VISUAL_JUDGE_VERDICT_RECEIVED";
            var providerMetadata = new Dictionary<string, string>
            {
                [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = (!script.Blocked && !script.ExternalCallExecuted).ToString(),
                ["external_ai_visual_judge_executed"] = script.ExternalCallExecuted.ToString()
            };
            var resultMetadata = new Dictionary<string, string>
            {
                ["external_ai_visual_judge_executed"] = script.ExternalCallExecuted.ToString()
            };
            if (script.ExternalCallExecuted && script.Cost.HasValue)
            {
                providerMetadata["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest;
                resultMetadata["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest;
            }

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:scripted-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: status,
                IsBlocked: script.Blocked,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: script.Blocked ? "none" : "scripted-local-judge",
                    EndpointHost: "none",
                    ModelId: script.Blocked ? "none" : "scripted-local-v1",
                    ExternalCallExecuted: script.ExternalCallExecuted,
                    Metadata: providerMetadata),
                EstimatedCostUsd: script.Cost,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: verdict.EvidenceRefs,
                Metadata: resultMetadata)));
        }
    }

    private sealed record JudgeScript(double Score, decimal? Cost, bool Blocked, bool ExternalCallExecuted = false)
    {
        public static JudgeScript FromScore(double score, decimal? cost)
            => new(score, cost, Blocked: false);

        public static JudgeScript FromExternalScore(double score, decimal? cost)
            => new(score, cost, Blocked: false, ExternalCallExecuted: true);

        public static JudgeScript FromBlockedProvider()
            => new(0.0, 0m, Blocked: true);
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
            if (_results.Count == 0)
            {
                return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                    "No scripted candidate result was configured.",
                    "SCRIPTED_CANDIDATE_MISSING"));
            }

            return Task.FromResult(_results.Dequeue());
        }
    }

    private sealed record RepositoryOwnedDpoPairFixture(
        DesignConversionInternalEvaluationResult Chosen,
        DesignConversionInternalEvaluationResult Rejected);

    private sealed class RequestBindingSequenceEvaluator(
        params DesignConversionInternalEvaluationResult[] values)
        : IDesignConversionThinClientJobEvaluator
    {
        private int _next;

        public Task<OperationResult<DesignConversionInternalEvaluationResult>>
            EvaluateAsync(
                DesignConversionInternalEvaluateRequest request,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var template = values[Math.Min(_next++, values.Length - 1)];
            var submitted = template.SubmittedRequest with
            {
                RequestId = request.RequestId,
                Html = request.Html,
                Css = request.Css,
                Budget = request.Budget,
                Provenance = request.Provenance
            };
            var candidate = template.RuntimeFlowResult.Candidate with
            {
                RequestId = request.RequestId,
                Html = request.Html,
                Css = request.Css
            };
            var output = template.RuntimeFlowResult.Output with
            {
                Html = request.Html,
                Css = request.Css
            };
            var rebound = template with
            {
                JobId = request.JobId,
                SubmittedRequest = submitted,
                ImprovedHtml = request.Html,
                ImprovedCss = request.Css,
                RuntimeFlowResult = template.RuntimeFlowResult with
                {
                    Request = submitted,
                    Candidate = candidate,
                    Output = output,
                    ImproverOutput =
                        template.RuntimeFlowResult.ImproverOutput with
                        {
                            Candidate = candidate,
                            Output = output
                        }
                }
            };
            return Task.FromResult(
                OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                    rebound));
        }
    }

    private sealed class Pass164RSourceAwareCandidateStrategy(string privateMarker)
        : IDesignConversionImprovementCandidateStrategy
    {
        private const string RequestIdPrefix = "pass164r-runtime-request-";

        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                    "Pass164R source-aware candidate strategy cancelled.",
                    "CANCELLED"));
            }

            Requests.Add(request);
            var currentRequestId = request.CurrentRequest.RequestId;
            if (!currentRequestId.StartsWith(RequestIdPrefix, StringComparison.Ordinal) ||
                currentRequestId.Length == RequestIdPrefix.Length)
            {
                return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                    "Pass164R source-aware candidate strategy requires the stable source request identity.",
                    "PASS164R_SOURCE_REQUEST_ID_REQUIRED"));
            }

            var sourceSlug = $"pass164r-source-{currentRequestId[RequestIdPrefix.Length..]}";
            var expectedSourceFamilyRef = DesignConversionDpoLeakageGuard.HashRef(
                "design-tree:source_family",
                sourceSlug);
            if (!request.CurrentRequest.DesignTreePayload.Metadata.TryGetValue(
                    "design-tree_source_family_ref",
                    out var sourceFamilyRef) ||
                !string.Equals(sourceFamilyRef, expectedSourceFamilyRef, StringComparison.Ordinal))
            {
                return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                    "Pass164R source-aware candidate strategy requires the matching canonical source-family ref.",
                    "PASS164R_SOURCE_FAMILY_REF_REQUIRED"));
            }

            var nextAttempt = request.CompletedAttemptNumber + 1;
            return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Ok(
                new DesignConversionImprovementCandidate(
                    CandidateId: $"{sourceSlug}-runtime-improved-candidate-{nextAttempt}",
                    RequestId: $"{sourceSlug}-runtime-improved-request-{nextAttempt}",
                    Html: $"<main class=\"{sourceSlug}-runtime-improved\" data-source=\"{sourceSlug}\" data-secret=\"{privateMarker}\">Pass164R improved {sourceSlug}</main>",
                    Css: $".{sourceSlug}-runtime-improved{{display:grid;gap:18px;padding:24px;background:url('{privateMarker}');}}",
                    ChangeReasons: [$"source-aware local runtime improvement for {sourceSlug}"],
                    EvidenceRefs:
                    [
                        $"candidate:{sourceSlug}:runtime-improved",
                        $"strategy-request:{request.LoopId}:{request.CompletedAttemptNumber}"
                    ],
                    Metadata: new Dictionary<string, string>
                    {
                        ["source_family"] = sourceSlug,
                        ["candidate_changed"] = bool.TrueString,
                        ["candidate_html_changed"] = bool.TrueString,
                        ["candidate_css_changed"] = bool.TrueString,
                        ["runtime_flow_candidate_used"] = bool.TrueString,
                        ["external_provider_called"] = bool.FalseString,
                        ["network_call_executed"] = bool.FalseString,
                        ["learned_improvement_claimed"] = bool.FalseString,
                        ["deterministic_logic_claimed_as_learning"] = bool.FalseString,
                        ["product_ready"] = bool.FalseString
                    })));
        }
    }

    private sealed class TeacherFixture : IDisposable
    {
        private readonly IDisposable _association;
        private readonly IDisposable? _preferenceAssociation;
        private readonly DesignConversionDpoTeacherAttestationRuntime _runtime;
        private readonly string _root;

        internal TeacherFixture(
            string name,
            bool supportTeacherAuthoring = false)
        {
            _root = Path.Combine(
                Path.GetTempPath(),
                "design-conversion-modelserver-teacher-" +
                name +
                "-" +
                Guid.NewGuid().ToString("N"));
            var provider = new DesignConversionDpoPolicyBaseTestProvider();
            ArtifactDb = new JsonlDesignConversionLocalArtifactDb(
                _root,
                dpoPolicyBaseProvider: provider);
            Teacher = new FakeTeacherInferenceClient(supportTeacherAuthoring);
            _runtime = new DesignConversionDpoTeacherAttestationRuntime(
                Teacher,
                ExpectedTeacherModelId,
                ExpectedTeacherManifestDigest,
                ExpectedTeacherModelLayerDigest);
            _association = DesignConversionDpoTeacherAttestationAssociation
                .AssociateArtifactDbForTesting(ArtifactDb, _runtime);
            if (supportTeacherAuthoring)
            {
                _preferenceAssociation =
                    DesignConversionDpoTeacherTestAssociation
                        .AssociateIndependentPreferenceEvidence(ArtifactDb);
            }
        }

        internal JsonlDesignConversionLocalArtifactDb ArtifactDb { get; }

        internal FakeTeacherInferenceClient Teacher { get; }

        public void Dispose()
        {
            _preferenceAssociation?.Dispose();
            _association.Dispose();
            _runtime.Dispose();
            Teacher.Dispose();
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
    }

    private sealed class FakeTeacherInferenceClient : ITrainingTeacherInferenceClient, IDisposable
    {
        private readonly bool _supportTeacherAuthoring;

        internal FakeTeacherInferenceClient(bool supportTeacherAuthoring = false)
        {
            _supportTeacherAuthoring = supportTeacherAuthoring;
        }

        public int InvocationCount { get; private set; }

        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
            string prompt,
            ITrainingTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
                return Task.FromResult(issued.MapError<TrainingTeacherInferenceResult>());

            InvocationCount++;
            using var document = JsonDocument.Parse(prompt);
            var root = document.RootElement;
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                ExpectedTeacherModelId,
                ExpectedTeacherManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                RequestId: "design-conversion-modelserver-improvement-loop-test",
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = ExpectedTeacherModelLayerDigest
            };
            if (_supportTeacherAuthoring &&
                root.TryGetProperty("schema", out var schema) &&
                string.Equals(
                    DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                    schema.GetString(),
                    StringComparison.Ordinal))
            {
                var incumbentRef =
                    root.GetProperty("incumbent_ref").GetString()!;
                var candidateAttempt =
                    root.GetProperty("candidate_attempt_number").GetInt32();
                var authoredOutput = JsonSerializer.Serialize(new
                {
                    schema = DesignConversionDpoTeacherAttestationRuntime
                        .AuthoringSchema,
                    incumbent_ref = incumbentRef,
                    candidate_html =
                        "<main class=\"repository-owned-teacher-authored\" " +
                        "data-attempt=\"" + candidateAttempt + "\">" +
                        "Repository-owned DPO candidate</main>",
                    candidate_css =
                        ".repository-owned-teacher-authored{" +
                        "display:grid;gap:" + (candidateAttempt + 8) +
                        "px;padding:24px;}"
                });
                return Task.FromResult(
                    OperationResult<TrainingTeacherInferenceResult>.Ok(new(
                        authoredOutput,
                        provenance)
                    {
                        InferenceProfile =
                            LocalQwenOllamaInferenceProfileAuthority.Authoring
                    }));
            }

            var chosenRef = root.GetProperty("chosen").GetProperty("ref").GetString()!;
            var rejectedRef = root.GetProperty("rejected").GetProperty("ref").GetString()!;
            var evidenceRef = root.GetProperty("evidence_refs")[0].GetString()!;
            var output = JsonSerializer.Serialize(new
            {
                schema = DesignConversionDpoTeacherAttestationRuntime.Schema,
                chosen_ref = chosenRef,
                rejected_ref = rejectedRef,
                preferred_ref = chosenRef,
                supports_preference = true,
                evidence_refs = new[] { evidenceRef }
            });
            return Task.FromResult(OperationResult<TrainingTeacherInferenceResult>.Ok(new(
                output,
                provenance)
            {
                InferenceProfile =
                    LocalQwenOllamaInferenceProfileAuthority.Authoring
            }));
        }

        public void Dispose()
        {
        }
    }

    private sealed class FailingAppendArtifactDb(
        string errorMessage,
        string errorCode,
        bool throwCanceled = false)
        : IDesignConversionLocalArtifactDb,
          IDesignConversionPostEvaluationStateRootProvider
    {
        public int AppendAttempts { get; private set; }
        public string PostEvaluationStateRootDirectory { get; } =
            Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString("N"),
                "failing-append-post-evaluation-state");

        public Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordAsync(
            DesignConversionLocalArtifactDbRecord record,
            CancellationToken ct = default)
        {
            AppendAttempts++;
            if (throwCanceled)
                throw new OperationCanceledException(ct);

            return Task.FromResult(OperationResult<DesignConversionLocalArtifactDbRecord>.Fail(errorMessage, errorCode));
        }

        public Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ListRecordsAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => UnsupportedAsync<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>();

        public Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryApprovedTrainingCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => UnsupportedAsync<IReadOnlyList<DesignConversionDpoTrainingRecord>>();

        public Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            DateTimeOffset? exportedAtUtc = null,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionApprovedDpoHandoffArtifact>();

        public Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
            DesignConversionApprovedDpoHandoffArtifact handoff,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => UnsupportedAsync<DesignConversionTrainingIntakeRecord>();

        public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
            CancellationToken ct = default)
            => UnsupportedAsync<IReadOnlyList<DesignConversionTrainingIntakeRecord>>();

        public Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
            string intakeId,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionTrainingIntakePreflightResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
            string intakeId,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => UnsupportedAsync<DesignConversionTrainingIntakeRunResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
            string intakeId,
            string promotionOutputDirectory,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionTrainingIntakeLocalPromotionResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
            string promotedLocalModelRef,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();

        public Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
            string localArtifactRecordId,
            string candidateOrTrainingRecordId,
            string reviewerRef,
            string reason,
            DateTimeOffset? approvedAtUtc = null,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionDpoCandidateApprovalResult>();

        public Task<OperationResult<DesignConversionDpoDisplayResult>> QueryDisplayRowsAsync(
            DesignConversionDpoDisplayQuery? query = null,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionDpoDisplayResult>();

        private static Task<OperationResult<T>> UnsupportedAsync<T>()
            => Task.FromResult(OperationResult<T>.Fail(
                "Failing append artifact DB supports append only.",
                "FAILING_APPEND_ARTIFACT_DB_UNSUPPORTED"));
    }

    private sealed record Pass164RRuntimeCandidateTrace(
        DesignConversionLocalArtifactDbRecord Stored,
        IReadOnlyList<string> CandidateIds,
        string SourceManifestRef,
        string SourceStructurePayload);


    [Fact]
    public void PayloadGuard_RejectsPrivateCallerMeasurementEvidenceWithStableCode()
    {
        var request = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "private-measurement",
            InitialPackage: null,
            MaxAttempts: 1)
        {
            MeasurementGateRequest = new DesignConversionMeasurementGateRequest(
                GateId: "private-measurement",
                UnitId: DesignConversionDpoUnits.CriticUnit,
                HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                    1,
                    0.95,
                    1,
                    0m,
                    1,
                    ["raw:<html>private</html>"]),
                IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                    true,
                    false,
                    false,
                    true,
                    ["artifact://proof/integrity.json"]),
                Thresholds: new DesignConversionMeasurementGateThresholds(
                    0.90,
                    10,
                    1m,
                    64))
        };

        var result = DesignConversionModelServerPayloadSizeGuard
            .ValidateImprovementLoopRequest(request, "measurement-test");

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionMeasurementAdmission.InvalidErrorCode, result.ErrorCode);
    }
}
