// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DPO.Models;
using Inference;
using LoRA;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ModelServer;
using Tokenizer;
using TrainableRouter.Training;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

[Collection(ModelServerContinuousLearningEnvironmentCollection.Name)]
public sealed class CodeDocumentationSavedModelAssessmentEndpointTests
{
    private const string DomainSlug =
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug;
    private const string DesignPatternSlug =
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug;
    private const string SubjectSlug = "csharp-documentation-fixture";
    private const string CorpusVersion = "v20260909-assessment";
    private const string ModelIdSlug = "code-documentation-assessment-llm";
    private const string ModelVersion = "v1";

    [Fact]
    public async Task CodeDocumentationModelAssessment_RouteIsMappedAndRejectsInvalidBodyInsteadOf404()
    {
        using var environment = new EnvironmentScope(new Dictionary<string, string?>
        {
            [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                = bool.FalseString,
            [Program.RepositoryOwnedTrainingEnabledEnvironmentName]
                = bool.FalseString,
            [CodeDocumentationSavedModelAssessmentProfileResolver
                .EnvironmentName] = null,
        });
        var builder = Program.CreateLocalLoopbackBuilder([]);
        await using var app = builder.Build();
        Program.ConfigureLocalLoopbackRoutes(app);

        try
        {
            await app.StartAsync();
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.EndpointPath,
                new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_HttpRejectsCallerSuppliedCalibrationFieldsBeforeServerProfile()
    {
        using var environment = new EnvironmentScope(new Dictionary<string, string?>
        {
            [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                = bool.FalseString,
            [Program.RepositoryOwnedTrainingEnabledEnvironmentName]
                = bool.FalseString,
            [CodeDocumentationSavedModelAssessmentProfileResolver
                .EnvironmentName] = null,
        });
        var builder = Program.CreateLocalLoopbackBuilder([]);
        await using var app = builder.Build();
        Program.ConfigureLocalLoopbackRoutes(app);

        try
        {
            await app.StartAsync();
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            var callerControlledPayload = JsonSerializer.Serialize(new
            {
                domainSlug = DomainSlug,
                designPatternSlug = DesignPatternSlug,
                assessmentRunId = "code-documentation-assessment-test-run",
                provenanceRefs = new[]
                {
                    "test:code-documentation-model-assessment",
                },
                repeatCount = 1,
                score = 1.0,
                measurementNoisePassport = new { repeatCount = 1 },
            });

            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CalibrationEndpointPath,
                new StringContent(
                    callerControlledPayload,
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public void CodeDocumentationModelAssessment_ReadyFileListsPostRoute()
    {
        var payload = Program.CreateLocalLoopbackReadyFilePayload(
            new Uri("http://127.0.0.1:12345"),
            ContinuousLearningModelServerStartup.Create(_ => null));

        var paths = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            payload["routePaths"]);
        var routes = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            payload["routes"]);

        Assert.Contains(
            CodeDocumentationSavedModelAssessmentEndpoint.EndpointPath,
            paths);
        Assert.Contains(
            CodeDocumentationSavedModelAssessmentEndpoint.CalibrationEndpointPath,
            paths);
        Assert.Contains(
            "POST " + CodeDocumentationSavedModelAssessmentEndpoint.EndpointPath,
            routes);
        Assert.Contains(
            "POST " +
            CodeDocumentationSavedModelAssessmentEndpoint.CalibrationEndpointPath,
            routes);
    }

    [Fact]
    public void CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority()
    {
        var propertyNames = typeof(
                CodeDocumentationSavedModelAssessmentHttpRequest)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(static property => property.Name)
            .ToArray();

        Assert.Equal(
            [
                "DomainSlug",
                "DesignPatternSlug",
                "AssessmentRunId",
                "ProvenanceRefs"
            ],
            propertyNames);
        foreach (var forbidden in new[]
                 {
                     "RepositoryRoot",
                     "WeightsPath",
                     "CorpusPath",
                     "ExpectedQuality",
                     "CheckerModelId",
                     "TeacherPort",
                     "MaxHeldOutCases",
                     "GenerationSettings",
                     "RepeatCount",
                     "RepeatedScores",
                     "Score",
                     "CorrectFraction",
                     "MeasurementNoisePassport",
                     "Calibration",
                 })
        {
            Assert.DoesNotContain(forbidden, propertyNames);
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_MissingServerProfileFailsBeforeContextOrChecker()
    {
        using var environment = new EnvironmentScope(new Dictionary<string, string?>
        {
            [CodeDocumentationSavedModelAssessmentProfileResolver
                .EnvironmentName] = null,
        });
        var factory = new RecordingEvaluationClientFactory();
        var services = new ServiceCollection()
            .AddSingleton<
                ICodeDocumentationSavedModelAssessmentEvaluationClientFactory>(
                factory)
            .BuildServiceProvider();

        var result =
            await CodeDocumentationSavedModelAssessmentEndpoint.ExecuteAsync(
                CreateValidShapeRequest(),
                services,
                DateTimeOffset.UnixEpoch);
        var response = await CaptureAsync(result);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, response.StatusCode);
        Assert.False(response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(
            CodeDocumentationSavedModelAssessmentProfileResolver
                .ConfigurationRequiredErrorCode,
            response.Json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(0, factory.CreateCalls);
        Assert.DoesNotContain(@"C:\", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/mnt/", response.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_PublicExecuteMissingMaterialBoundaryFailsBeforeChecker()
    {
        using var fixture = new AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot);
        var started = false;
        try
        {
            await app.StartAsync();
            started = true;

            var startup = app.Services.GetRequiredService<
                ContinuousLearningModelServerStartup>();
            var fileAccess = app.Services.GetRequiredService<
                ServerOwnedContinuousLearningFileAccess>();
            var prepared = startup.PrepareRepositoryOwnedTrainingExecutionContext(
                DomainSlug,
                fileAccess);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            Assert.NotNull(prepared.Value);

            var factory = new RecordingEvaluationClientFactory();
            var services = new FactoryReplacingServiceProvider(
                app.Services,
                factory,
                suppressMaterialBoundaryProvider: true);
            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint.ExecuteAsync(
                    CreateValidShapeRequest(),
                    services,
                    DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(
                StatusCodes.Status503ServiceUnavailable,
                response.StatusCode);
            Assert.False(response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .AssessmentUnavailableErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(0, factory.CreateCalls);
            Assert.DoesNotContain("closedReferenceAnswer", response.Body, StringComparison.Ordinal);
            Assert.DoesNotContain(@"C:\", response.Body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("/mnt/", response.Body, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (started)
                await app.StopAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_NormalHostUsesServerOwnedDiagnosticBoundary()
    {
        using var fixture = new AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot);
        var started = false;
        try
        {
            await app.StartAsync();
            started = true;

            var factory = new RecordingEvaluationClientFactory();
            var services = new FactoryReplacingServiceProvider(
                app.Services,
                factory);
            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint.ExecuteAsync(
                    CreateValidShapeRequest(),
                    services,
                    DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.True(response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
            Assert.Equal(1, factory.CreateCalls);
            Assert.Equal(2, factory.Client!.Observations.Count);
            var trustedAssessmentRoot = Assert.IsType<
                CodeDocumentationSavedModelAssessmentTrustedRoot>(
                app.Services.GetService<
                    CodeDocumentationSavedModelAssessmentTrustedRoot>());
            Assert.Equal(fixture.RepositoryRoot, trustedAssessmentRoot.RepositoryRoot);
            Assert.NotNull(app.Services.GetService<
                ICodeDocumentationMaterialBoundaryProvider>());

            var storeRoot = Path.Combine(
                fixture.RepositoryRoot,
                "training-artifacts",
                "models",
                CodeDocumentationCertificationSessionStore.DirectoryName);
            var exposureRoot = Path.Combine(storeRoot, "exposures");
            Assert.True(Directory.Exists(exposureRoot));
            Assert.NotEmpty(Directory.GetDirectories(exposureRoot));
        }
        finally
        {
            if (started)
                await app.StopAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_NormalHostOwnsBoundaryStoreUntilDisposal()
    {
        using var fixture = new AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot);
        var lifetime = Assert.IsType<
            CodeDocumentationServerOwnedMaterialBoundaryLifetime>(
            app.Services.GetRequiredService<
                CodeDocumentationServerOwnedMaterialBoundaryLifetime>());

        Assert.True(
            lifetime.StoreForTesting.ValidateOwnerBinding(
                fixture.RepositoryRoot,
                Program.ComputeServerOwnedMaterialBoundaryStoreId(
                    fixture.RepositoryRoot)).IsSuccess);

        await app.DisposeAsync();

        Assert.False(
            lifetime.StoreForTesting.ValidateOwnerBinding(
                fixture.RepositoryRoot,
                Program.ComputeServerOwnedMaterialBoundaryStoreId(
                    fixture.RepositoryRoot)).IsSuccess);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_ForeignMaterialBoundaryFailsBeforeChecker()
    {
        using var fixture = new AssessmentServerFixture();
        using var foreignFixture = new AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        const string foreignStoreId =
            "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
        var foreignStoreResult = CodeDocumentationCertificationSessionStore
            .CreateNewForServerOwnedRoot(
                foreignFixture.RepositoryRoot,
                foreignStoreId);
        Assert.True(
            foreignStoreResult.IsSuccess,
            foreignStoreResult.ErrorMessage);
        using var foreignStore = Assert.IsType<CodeDocumentationCertificationSessionStore>(
            foreignStoreResult.Value);
        var foreignProviderResult =
            CodeDocumentationServerOwnedMaterialBoundaryProvider.Create(
                foreignFixture.RepositoryRoot,
                foreignStoreId,
                foreignStore);
        Assert.True(
            foreignProviderResult.IsSuccess,
            foreignProviderResult.ErrorMessage);
        var foreignProvider = Assert.IsType<
            CodeDocumentationServerOwnedMaterialBoundaryProvider>(
            foreignProviderResult.Value);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            materialBoundaryProvider: foreignProvider);
        var started = false;
        try
        {
            await app.StartAsync();
            started = true;

            var factory = new RecordingEvaluationClientFactory();
            var services = new FactoryReplacingServiceProvider(
                app.Services,
                factory);
            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint.ExecuteAsync(
                    CreateValidShapeRequest(),
                    services,
                    DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(
                StatusCodes.Status503ServiceUnavailable,
                response.StatusCode);
            Assert.False(response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .AssessmentUnavailableErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(0, factory.CreateCalls);
        }
        finally
        {
            if (started)
                await app.StopAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "positive");
            var parent = SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
            var profile = ResolveProfile(
                corpus.Manifest.ManifestSha256,
                maxHeldOutCases: 10,
                maxNewTokens: 1,
                maxAnswerChars: 256);
            var independentlyGeneratedAnswers =
                GenerateHeldOutAnswersFromSavedWeights(
                    fixture.RepositoryRoot,
                    parent,
                    corpus,
                    modelConfig,
                    profile.GenerationSettings);
            var teacher = CreateTeacherConfiguration();
            var factory = new RecordingEvaluationClientFactory();

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .AssessPreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        teacher,
                        profile,
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.True(response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
            var assessment = response.Json.RootElement.GetProperty("assessment");
            Assert.True(assessment.GetProperty("isSuccess").GetBoolean());
            Assert.Equal(2, assessment.GetProperty("evaluatedCaseCount").GetInt32());
            Assert.Equal(1, assessment.GetProperty("passedCaseCount").GetInt32());
            Assert.Equal(
                0.5,
                assessment.GetProperty("correctFraction").GetDouble());
            Assert.Equal(
                corpus.Manifest.ManifestSha256,
                assessment.GetProperty("corpusManifestSha256").GetString());
            Assert.Equal(
                LocalQwenOllamaInferenceProfileAuthority.Evaluation.ProfileId,
                assessment.GetProperty("evaluationProfileId").GetString());
            Assert.Equal(
                LocalQwenOllamaInferenceProfileAuthority.Evaluation
                    .ConfigurationSha256,
                assessment.GetProperty("evaluationProfileConfigurationSha256")
                    .GetString());

            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("calibration").ValueKind);

            Assert.Equal(2, factory.Client!.Observations.Count);
            Assert.Equal(
                2,
                factory.Client.Observations
                    .Select(static item => item.RequestId)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.DoesNotContain(
                factory.Client.Observations,
                item => item.OwnerTask.Contains("train-only", StringComparison.Ordinal));
            foreach (var observed in factory.Client.Observations)
            {
                Assert.True(
                    independentlyGeneratedAnswers.TryGetValue(
                        observed.CaseId,
                        out var independentlyGeneratedAnswer),
                    "Independent generator did not produce this held-out case.");
                Assert.Equal(
                    independentlyGeneratedAnswer,
                    observed.StudentAnswer);
            }
            Assert.DoesNotContain("cases", assessment.EnumerateObject()
                .Select(static property => property.Name));
            Assert.DoesNotContain("generationSettings", assessment.EnumerateObject()
                .Select(static property => property.Name));
            Assert.DoesNotContain("evaluator", assessment.EnumerateObject()
                .Select(static property => property.Name));
            Assert.DoesNotContain("Reference answer", response.Body,
                StringComparison.OrdinalIgnoreCase);

            Assert.Equal(
                response.Json.RootElement
                    .GetProperty("savedWeightsBeforeSha256").GetString(),
                response.Json.RootElement
                    .GetProperty("savedWeightsAfterSha256").GetString());
            Assert.Equal(
                assessment.GetProperty("modelSerializationBeforeSha256")
                    .GetString(),
                assessment.GetProperty("modelSerializationAfterSha256")
                    .GetString());
            Assert.Equal(1, factory.CreateCalls);
            var filesAfterAssessment = SnapshotFiles(fixture.RepositoryRoot);
            AssertSameFileSnapshot(
                filesBeforeAssessment,
                filesAfterAssessment);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "varied-series");
            var parent = SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
            var profile = ResolveProfile(corpus.Manifest.ManifestSha256);
            var decisions = new Queue<bool>(
                [
                    true, true,
                    true, false,
                    false, false,
                    true, false,
                    true, true,
                ]);
            var gate = new object();
            var factory = new RecordingEvaluationClientFactory(observation =>
            {
                bool pass;
                lock (gate)
                {
                    pass = decisions.Dequeue();
                }

                return JsonSerializer.Serialize(new
                {
                    caseId = observation.CaseId,
                    pass,
                    reason = pass
                        ? CodeDocumentationSavedModelAssessmentService
                            .CorrectReasonCode
                        : CodeDocumentationSavedModelAssessmentService
                            .MissingEssentialBehaviorReasonCode,
                });
            });

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .CalibratePreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        profile,
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            var calibration = response.Json.RootElement.GetProperty("calibration");
            Assert.Equal(
                5,
                calibration.GetProperty("repeatCount").GetInt32());
            Assert.Equal(0.6d, calibration.GetProperty("meanScore").GetDouble(), 12);
            Assert.True(
                calibration.GetProperty("noiseFloor").GetDouble() > 0d);
            Assert.DoesNotContain("observations", calibration.EnumerateObject()
                .Select(static property => property.Name));
            Assert.DoesNotContain("measurementNoisePassport", calibration.EnumerateObject()
                .Select(static property => property.Name));
            Assert.Equal(10, factory.Client!.Observations.Count);
            Assert.Equal(10, factory.Client.IssuedCallCount);
            Assert.Equal(10, factory.Client.CompletedResponseCount);
            Assert.Equal(
                10,
                factory.Client.Observations
                    .Select(static item => item.RequestId)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.Empty(decisions);
            AssertSameFileSnapshot(
                filesBeforeAssessment,
                SnapshotFiles(fixture.RepositoryRoot));

            var assessment = response.Json.RootElement.GetProperty("assessment");
            Assert.Equal(
                parent.WeightsSha256,
                assessment.GetProperty("savedWeightsSha256").GetString());
            Assert.Equal(
                assessment.GetProperty("modelSerializationBeforeSha256")
                    .GetString(),
                assessment.GetProperty("modelSerializationAfterSha256")
                    .GetString());
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "budget");
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
            var factory = new RecordingEvaluationClientFactory();

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .CalibratePreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(maxTeacherCalls: 9),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentCalibrationService
                    .CalibrationBudgetExceededErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(0, factory.CreateCalls);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("calibration").ValueKind);
            AssertSameFileSnapshot(
                filesBeforeAssessment,
                SnapshotFiles(fixture.RepositoryRoot));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CalibrationDeadlineAfterIssuedCallReturnsNoPassport()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        var modelConfig = CreateModelConfig();
        var tokenizer = CreateTokenizer(modelConfig);
        var compatibility = CreateCompatibility(modelConfig, "deadline-after-issued");
        var parent = SealModelCheckpoint(
            fixture.RepositoryRoot,
            new LlmModel(modelConfig),
            compatibility,
            CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
            step: 1,
            parent: null);
        var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
        var profile = ResolveProfile(
            corpus.Manifest.ManifestSha256,
            maxHeldOutCases: 2);
        var teacherConfiguration = CreateTeacherConfiguration();
        var dispatched = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var deadlineCancelled = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var factory = new RecordingEvaluationClientFactory(
            afterPermitAsync: (_, cancellationToken) =>
            {
                dispatched.TrySetResult(true);
                cancellationToken.Register(
                    () => deadlineCancelled.TrySetResult(true));
                return Task.Delay(
                    Timeout.InfiniteTimeSpan,
                    cancellationToken);
            });
        var created = factory.Create(teacherConfiguration);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        using var client = Assert.IsType<RecordingEvaluationClient>(
            created.Value);
        var request = CreateDirectCalibrationRequest(
            LoadSavedModelCheckpoint(
                fixture.RepositoryRoot,
                parent,
                modelConfig),
            tokenizer,
            parent,
            corpus,
            client,
            teacherConfiguration,
            profile);
        var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);

        var calibrationTask =
            new CodeDocumentationSavedModelAssessmentCalibrationService()
                .CalibrateAsync(
                    request,
                    maximumDuration: TimeSpan.FromSeconds(5));

        Assert.True(
            await dispatched.Task.WaitAsync(TimeSpan.FromSeconds(15)));
        Assert.True(
            await deadlineCancelled.Task.WaitAsync(TimeSpan.FromSeconds(15)));
        var result = await calibrationTask.WaitAsync(TimeSpan.FromSeconds(15));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationSavedModelAssessmentService.CancelledErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(1, client.Observations.Count);
        Assert.Equal(1, client.IssuedCallCount);
        Assert.Equal(0, client.CompletedResponseCount);
        Assert.Empty(client.Outputs);
        AssertSameFileSnapshot(
            filesBeforeAssessment,
            SnapshotFiles(fixture.RepositoryRoot));
    }

    [Fact]
    public void CodeDocumentationModelAssessment_CalibrationPlanRejectsIntegerMultiplicationOverflow()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        var entries = new[]
        {
            CreateReferenceEntry(
                "Document overflow-only train helper.",
                "Reference answer for overflow-only train helper.",
                FrozenReferencePartition.Train,
                "overflow-train"),
            CreateReferenceEntry(
                "Document Add method.",
                "Reference answer: Add returns the sum and has no side effects.",
                FrozenReferencePartition.HeldOut,
                "overflow-heldout-add"),
            CreateReferenceEntry(
                "Document Subtract method.",
                "Reference answer: Subtract returns left minus right and does not mutate inputs.",
                FrozenReferencePartition.HeldOut,
                "overflow-heldout-subtract"),
            CreateReferenceEntry(
                "Document Multiply method.",
                "Reference answer: Multiply returns the product without side effects.",
                FrozenReferencePartition.HeldOut,
                "overflow-heldout-multiply"),
        };
        var corpus = FrozenReferenceCorpus.Build(
            "csharp-documentation-overflow-fixture",
            "v20260915-overflow",
            DateTimeOffset.UnixEpoch,
            entries);
        Assert.True(corpus.IsSuccess, corpus.ErrorMessage);
        var profile = ResolveProfile(
            Hex('d'),
            maxHeldOutCases: 3);
        var heldOutCaseCount = corpus.Value!.Entries.Count(
            static entry => entry.Partition == FrozenReferencePartition.HeldOut);
        const int repeatCount = 1_431_655_766;
        Assert.Equal(3, heldOutCaseCount);
        Assert.Equal(
            4_294_967_298L,
            checked((long)heldOutCaseCount * repeatCount));
        Assert.Equal(
            2,
            unchecked(heldOutCaseCount * repeatCount));
        Assert.True(unchecked(heldOutCaseCount * repeatCount) > 0);
        Assert.True(unchecked(heldOutCaseCount * repeatCount) <= 10_000);

        var plan = CodeDocumentationSavedModelAssessmentCalibrationService
            .PlanCalibration(
                corpus.Value,
                profile.GenerationSettings,
                CreateTeacherConfiguration(maxTeacherCalls: 10_000),
                new MeasurementNoisePolicyConfig(repeatCount));

        Assert.False(plan.IsSuccess);
        Assert.Equal(
            CodeDocumentationSavedModelAssessmentCalibrationService
                .CalibrationBudgetExceededErrorCode,
            plan.ErrorCode);
        Assert.Null(plan.Value);
    }

    [Fact]
    public void CodeDocumentationModelAssessment_QualityGatePlanHonorsPublicArithmeticBounds()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
        var settings = ResolveProfile(
            corpus.Manifest.ManifestSha256,
            maxHeldOutCases: 2).GenerationSettings;
        var teacherConfiguration = CreateTeacherConfiguration(
            maxTeacherCalls: 10_000);

        var baselineAt2500 = CodeDocumentationSavedModelAssessmentCalibrationService
            .PlanCalibration(
                corpus.Corpus,
                settings,
                teacherConfiguration,
                new MeasurementNoisePolicyConfig(2_500));
        Assert.True(baselineAt2500.IsSuccess, baselineAt2500.ErrorMessage);
        Assert.Equal(5_000, baselineAt2500.Value!.ExpectedEvaluationCallCount);
        var gateAt2500 = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
            .PlanQualityGate(
                corpus.Corpus,
                settings,
                teacherConfiguration,
                new MeasurementNoisePolicyConfig(2_500));
        Assert.True(gateAt2500.IsSuccess, gateAt2500.ErrorMessage);
        Assert.Equal(
            10_000,
            gateAt2500.Value!.ExpectedTotalEvaluationCallCount);

        var baselineAt2501 = CodeDocumentationSavedModelAssessmentCalibrationService
            .PlanCalibration(
                corpus.Corpus,
                settings,
                teacherConfiguration,
                new MeasurementNoisePolicyConfig(2_501));
        Assert.True(baselineAt2501.IsSuccess, baselineAt2501.ErrorMessage);
        Assert.Equal(5_002, baselineAt2501.Value!.ExpectedEvaluationCallCount);
        var gateAt2501 = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
            .PlanQualityGate(
                corpus.Corpus,
                settings,
                teacherConfiguration,
                new MeasurementNoisePolicyConfig(2_501));
        Assert.False(gateAt2501.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .QualityGateBudgetExceededErrorCode,
            gateAt2501.ErrorCode);

        var baselineAt5000 = CodeDocumentationSavedModelAssessmentCalibrationService
            .PlanCalibration(
                corpus.Corpus,
                settings,
                teacherConfiguration,
                new MeasurementNoisePolicyConfig(5_000));
        Assert.True(baselineAt5000.IsSuccess, baselineAt5000.ErrorMessage);
        Assert.Equal(
            10_000,
            baselineAt5000.Value!.ExpectedEvaluationCallCount);
        var gateAt5000 = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
            .PlanQualityGate(
                corpus.Corpus,
                settings,
                teacherConfiguration,
                new MeasurementNoisePolicyConfig(5_000));
        Assert.False(gateAt5000.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .QualityGateBudgetExceededErrorCode,
            gateAt5000.ErrorCode);

        var rejectedConfiguration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen-assessment-test:9b",
            Hex('a'),
            Hex('b'),
            10_001,
            "sha256:" + Hex('c'),
            "sha256:" + Hex('d'),
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        Assert.False(rejectedConfiguration.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            rejectedConfiguration.ErrorCode);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_DisposalFailurePreservesPrimaryErrorForAssessmentAndCalibration()
    {
        await AssertDisposalFailurePreservesPrimaryErrorAsync(
            runCalibration: false);
        await AssertDisposalFailurePreservesPrimaryErrorAsync(
            runCalibration: true);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_DisposalFailureAfterSuccessfulAssessmentOrCalibrationReturnsNoResult()
    {
        await AssertSuccessfulEndpointDisposalFailureAsync(
            runCalibration: false);
        await AssertSuccessfulEndpointDisposalFailureAsync(
            runCalibration: true);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CalibrationDisposalFailurePreservesCancellation()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        using var cancellation = new CancellationTokenSource();
        var factory = new RecordingEvaluationClientFactory(
            afterPermitAsync: (_, cancellationToken) =>
            {
                cancellation.Cancel();
                return Task.FromException(
                    new OperationCanceledException(cancellationToken));
            },
            disposeAction: () => throw new InvalidOperationException(
                "simulated nonfatal disposal failure after cancellation"));

        var run = await RunPreparedEndpointAsync(
            fixture,
            factory,
            runCalibration: true,
            compatibilitySuffix: "disposal-cancellation",
            cancellationToken: cancellation.Token);

        Assert.Equal(StatusCodes.Status409Conflict, run.Response.StatusCode);
        Assert.Equal(
            CodeDocumentationSavedModelAssessmentService.CancelledErrorCode,
            run.Response.Json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(
            JsonValueKind.Null,
            run.Response.Json.RootElement.GetProperty("assessment").ValueKind);
        Assert.Equal(
            JsonValueKind.Null,
            run.Response.Json.RootElement.GetProperty("calibration").ValueKind);
        Assert.NotNull(factory.Client);
        Assert.Equal(1, factory.Client!.Observations.Count);
        Assert.Equal(1, factory.Client.IssuedCallCount);
        Assert.Equal(0, factory.Client.CompletedResponseCount);
        Assert.Empty(factory.Client.Outputs);
        Assert.Equal(1, factory.Client.DisposeCalls);
        AssertSameFileSnapshot(
            run.FilesBeforeAssessment,
            SnapshotFiles(fixture.RepositoryRoot));
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "repeat-fails");
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
            var outputCalls = 0;
            var factory = new RecordingEvaluationClientFactory(observation =>
            {
                var callNumber = Interlocked.Increment(ref outputCalls);
                if (callNumber == 5)
                    return "{not-json";

                var pass = observation.OwnerTask.Contains(
                    "Add",
                    StringComparison.Ordinal);
                return JsonSerializer.Serialize(new
                {
                    caseId = observation.CaseId,
                    pass,
                    reason = pass
                        ? CodeDocumentationSavedModelAssessmentService
                            .CorrectReasonCode
                        : CodeDocumentationSavedModelAssessmentService
                            .MissingEssentialBehaviorReasonCode,
                });
            });

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .CalibratePreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentService
                    .EvaluationOutputInvalidErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(5, factory.Client!.Observations.Count);
            Assert.Equal(5, factory.Client.IssuedCallCount);
            Assert.Equal(5, factory.Client.CompletedResponseCount);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("assessment").ValueKind);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("calibration").ValueKind);
            AssertSameFileSnapshot(
                filesBeforeAssessment,
                SnapshotFiles(fixture.RepositoryRoot));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CalibrationStopsAfterThirdInvalidCheckerResponseWithoutPassport()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "third-invalid");
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
            var outputCalls = 0;
            var factory = new RecordingEvaluationClientFactory(observation =>
            {
                var callNumber = Interlocked.Increment(ref outputCalls);
                if (callNumber == 3)
                {
                    return JsonSerializer.Serialize(new
                    {
                        caseId = observation.CaseId,
                        pass = true,
                        reason = CodeDocumentationSavedModelAssessmentService
                            .CorrectReasonCode,
                        evaluatorExplanation = "evaluator-secret",
                    });
                }

                return JsonSerializer.Serialize(new
                {
                    caseId = observation.CaseId,
                    pass = true,
                    reason = CodeDocumentationSavedModelAssessmentService
                        .CorrectReasonCode,
                });
            });

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .CalibratePreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.False(
                response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentService
                    .EvaluationOutputInvalidErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(3, outputCalls);
            Assert.Equal(3, factory.Client!.Observations.Count);
            Assert.Equal(3, factory.Client.ReturnedOutcomes.Count);
            Assert.Equal(3, factory.Client.Outputs.Count);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("assessment").ValueKind);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("calibration").ValueKind);
            Assert.All(
                factory.Client.Observations,
                observation =>
                    Assert.DoesNotContain(
                        observation.ClosedReferenceAnswer,
                        response.Body,
                        StringComparison.Ordinal));
            Assert.DoesNotContain(
                "evaluator-secret",
                response.Body,
                StringComparison.Ordinal);
            AssertSameFileSnapshot(
                filesBeforeAssessment,
                SnapshotFiles(fixture.RepositoryRoot));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CalibrationTransportFailureAfterPermitDoesNotRetryOrBuildPassport()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(
                modelConfig,
                "transport-fails-after-permit");
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
            var factory = new RecordingEvaluationClientFactory(
                _ => throw new InvalidOperationException(
                    "simulated checker transport failure after permit"));

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .CalibratePreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentService.GenerationFailedErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(1, factory.CreateCalls);
            Assert.Single(factory.Client!.Observations);
            Assert.Equal(1, factory.Client.IssuedCallCount);
            Assert.Equal(0, factory.Client.CompletedResponseCount);
            Assert.Empty(factory.Client.Outputs);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("assessment").ValueKind);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("calibration").ValueKind);
            AssertSameFileSnapshot(
                filesBeforeAssessment,
                SnapshotFiles(fixture.RepositoryRoot));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CalibrationRejectsDuplicateRequestIdsBetweenRepeats()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "dup-ids");
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
            var factory = new RecordingEvaluationClientFactory(
                requestIdFactory: callNumber =>
                    "assessment-evaluation-" +
                    (((callNumber - 1) % 2) + 1).ToString("D4"));

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .CalibratePreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentCalibrationService
                    .CalibrationConditionMismatchErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(4, factory.Client!.Observations.Count);
            Assert.Equal(4, factory.Client.IssuedCallCount);
            Assert.Equal(4, factory.Client.CompletedResponseCount);
            Assert.Equal(
                2,
                factory.Client.Observations
                    .Select(static item => item.RequestId)
                    .Distinct()
                    .Count());
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("calibration").ValueKind);
            AssertSameFileSnapshot(
                filesBeforeAssessment,
                SnapshotFiles(fixture.RepositoryRoot));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CalibrationCancelledAfterStartedCallsReturnsNoPassport()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(
                modelConfig,
                "cancel-after-calls");
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
            using var cancellation = new CancellationTokenSource();
            var outputCalls = 0;
            var factory = new RecordingEvaluationClientFactory(observation =>
            {
                var callNumber = Interlocked.Increment(ref outputCalls);
                if (callNumber == 5)
                    cancellation.Cancel();

                var pass = observation.OwnerTask.Contains(
                    "Add",
                    StringComparison.Ordinal);
                return JsonSerializer.Serialize(new
                {
                    caseId = observation.CaseId,
                    pass,
                    reason = pass
                        ? CodeDocumentationSavedModelAssessmentService
                            .CorrectReasonCode
                        : CodeDocumentationSavedModelAssessmentService
                            .MissingEssentialBehaviorReasonCode,
                });
            });

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .CalibratePreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch,
                        cancellation.Token);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentService.CancelledErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(5, factory.Client!.Observations.Count);
            Assert.Equal(5, factory.Client.IssuedCallCount);
            Assert.Equal(5, factory.Client.CompletedResponseCount);
            Assert.Equal(5, outputCalls);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("assessment").ValueKind);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("calibration").ValueKind);
            AssertSameFileSnapshot(
                filesBeforeAssessment,
                SnapshotFiles(fixture.RepositoryRoot));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CalibrationRejectsSavedWeightBytesChangedBetweenRepeats()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "disk-change");
            var parent = SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
            var savedWeightsPath = Path.GetFullPath(Path.Combine(
                fixture.RepositoryRoot,
                parent.RepositoryRelativeWeightsPath.Replace(
                    '/',
                    Path.DirectorySeparatorChar)));
            var savedWeightsRelativePath = Path
                .GetRelativePath(fixture.RepositoryRoot, savedWeightsPath)
                .Replace(Path.DirectorySeparatorChar, '/');
            var originalWeights = File.ReadAllBytes(savedWeightsPath);
            Assert.Equal(parent.WeightsSha256, Sha256Hex(originalWeights));

            var outputCalls = 0;
            var savedWeightsChanged = false;
            var factory = new RecordingEvaluationClientFactory(observation =>
            {
                var callNumber = Interlocked.Increment(ref outputCalls);
                if (callNumber == 4)
                {
                    var changedWeights = File.ReadAllBytes(savedWeightsPath);
                    Assert.Equal(originalWeights, changedWeights);
                    changedWeights[^1] ^= 0x5A;
                    Assert.Equal(originalWeights.Length, changedWeights.Length);
                    Assert.NotEqual(
                        Sha256Hex(originalWeights),
                        Sha256Hex(changedWeights));
                    File.WriteAllBytes(savedWeightsPath, changedWeights);
                    savedWeightsChanged = true;
                }

                var pass = observation.OwnerTask.Contains(
                    "Add",
                    StringComparison.Ordinal);
                return JsonSerializer.Serialize(new
                {
                    caseId = observation.CaseId,
                    pass,
                    reason = pass
                        ? CodeDocumentationSavedModelAssessmentService
                            .CorrectReasonCode
                        : CodeDocumentationSavedModelAssessmentService
                            .MissingEssentialBehaviorReasonCode,
                });
            });

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .CalibratePreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.True(savedWeightsChanged);
            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .SavedWeightsChangedErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(10, factory.Client!.Observations.Count);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("assessment").ValueKind);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("calibration").ValueKind);
            AssertOnlyFileHashChanged(
                filesBeforeAssessment,
                SnapshotFiles(fixture.RepositoryRoot),
                savedWeightsRelativePath);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public Task CodeDocumentationModelAssessment_CheckerReasonThatQuotesGoldRejectsWholeAssessment()
        => AssertCheckerReasonFailureAsync(
            observation => JsonSerializer.Serialize(new
            {
                caseId = observation.CaseId,
                pass = false,
                reason = observation.ClosedReferenceAnswer,
            }));

    [Fact]
    public Task CodeDocumentationModelAssessment_UnknownCheckerReasonRejectsWholeAssessment()
        => AssertCheckerReasonFailureAsync(
            observation => JsonSerializer.Serialize(new
            {
                caseId = observation.CaseId,
                pass = false,
                reason = "unknown_reason_code",
            }));

    [Fact]
    public Task CodeDocumentationModelAssessment_ContradictingCheckerPassAndReasonRejectsWholeAssessment()
        => AssertCheckerReasonFailureAsync(
            observation => JsonSerializer.Serialize(new
            {
                caseId = observation.CaseId,
                pass = true,
                reason = CodeDocumentationSavedModelAssessmentService
                    .MissingEssentialBehaviorReasonCode,
            }));

    [Fact]
    public Task CodeDocumentationModelAssessment_RejectsDuplicateCheckerPropertiesWithoutPartialAssessment()
        => AssertCheckerReasonFailureAsync(
            observation =>
                "{\"caseId\":\"" + observation.CaseId +
                "\",\"pass\":true,\"pass\":false,\"reason\":\"correct\"}");

    [Fact]
    public Task CodeDocumentationModelAssessment_RejectsExtraCheckerPropertiesWithoutPartialAssessment()
        => AssertCheckerReasonFailureAsync(
            observation => JsonSerializer.Serialize(new
            {
                caseId = observation.CaseId,
                pass = true,
                reason = CodeDocumentationSavedModelAssessmentService
                    .CorrectReasonCode,
                evaluatorReason = "evaluator-secret",
            }),
            "evaluator-secret");

    [Fact]
    public Task CodeDocumentationModelAssessment_RejectsWrongCaseIdWithoutPartialAssessment()
        => AssertCheckerReasonFailureAsync(
            observation => JsonSerializer.Serialize(new
            {
                caseId = observation.CaseId + "-other",
                pass = true,
                reason = CodeDocumentationSavedModelAssessmentService
                    .CorrectReasonCode,
            }));

    [Fact]
    public Task CodeDocumentationModelAssessment_RejectsStringPassWithoutPartialAssessment()
        => AssertCheckerReasonFailureAsync(
            observation => JsonSerializer.Serialize(new
            {
                caseId = observation.CaseId,
                pass = "true",
                reason = CodeDocumentationSavedModelAssessmentService
                    .CorrectReasonCode,
            }));

    [Fact]
    public Task CodeDocumentationModelAssessment_RejectsNullReasonWithoutPartialAssessment()
        => AssertCheckerReasonFailureAsync(
            observation => JsonSerializer.Serialize(new
            {
                caseId = observation.CaseId,
                pass = true,
                reason = (string?)null,
            }));

    [Fact]
    public Task CodeDocumentationModelAssessment_RejectsArrayCheckerRootWithoutPartialAssessment()
        => AssertCheckerReasonFailureAsync(_ => "[]");

    [Fact]
    public Task CodeDocumentationModelAssessment_RejectsContradictoryFalseCorrectReasonWithoutPartialAssessment()
        => AssertCheckerReasonFailureAsync(
            observation => JsonSerializer.Serialize(new
            {
                caseId = observation.CaseId,
                pass = false,
                reason = CodeDocumentationSavedModelAssessmentService
                    .CorrectReasonCode,
            }));

    [Fact]
    public async Task CodeDocumentationModelAssessment_WrongCorpusHashFailsBeforeChecker()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "wrong-corpus");
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            _ = SealReferenceCorpus(fixture.RepositoryRoot);
            var factory = new RecordingEvaluationClientFactory();

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .AssessPreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(Hex('a')),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CorpusMismatchErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(0, factory.CreateCalls);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_MaxHeldOutAndContextOverflowRejectWholeExamWithoutCheckerCall()
    {
        await AssertPreparedFailureBeforeCheckerCallAsync(
            profileFactory: manifest => ResolveProfile(
                manifest.ManifestSha256,
                maxHeldOutCases: 1),
            CodeDocumentationSavedModelAssessmentService
                .HeldOutTooLargeErrorCode);
        await AssertPreparedFailureBeforeCheckerCallAsync(
            profileFactory: manifest => ResolveProfile(
                manifest.ManifestSha256,
                maxHeldOutCases: 10,
                maxNewTokens: 128),
            CodeDocumentationSavedModelAssessmentService
                .ContextBudgetExceededErrorCode);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_InvalidCheckerJsonRejectsWholeExam()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "bad-json");
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var factory = new RecordingEvaluationClientFactory(_ => "{not-json");

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .AssessPreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentService
                    .EvaluationOutputInvalidErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Single(factory.Client!.Observations);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_CancelledPreparedRunDoesNotCallChecker()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "cancelled");
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var factory = new RecordingEvaluationClientFactory();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .AssessPreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch,
                        cancellation.Token);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .CancelledErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(0, factory.CreateCalls);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_MissingSavedParentUsesDeniedColdStartAndNeverCallsChecker()
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "no-parent");
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var factory = new RecordingEvaluationClientFactory();

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .AssessPreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .ColdStartNotAuthorizedErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(0, factory.CreateCalls);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public void CodeDocumentationModelAssessment_RepositoryRootIsCanonicalAllowsOnlyNativeCanonicalCheckout()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.True(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .RepositoryRootIsCanonical(
                        Program.RepositoryOwnedTrainingTrustedRepositoryRoot));
            Assert.True(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .RepositoryRootIsCanonical(
                        Program.RepositoryOwnedTrainingTrustedRepositoryRoot
                            .ToUpperInvariant()));
            Assert.False(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .RepositoryRootIsCanonical(
                        CodeDocumentationSavedModelAssessmentEndpoint
                            .WslRepositoryOwnedTrainingTrustedRepositoryRoot));
            Assert.False(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .RepositoryRootIsCanonical(
                        @"D:\Projects\llm_mvp_core"));
            Assert.False(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .RepositoryRootIsCanonical(
                        @"C:\Projects\llm_mvp_core-DpoImplementation"));
        }
        else if (OperatingSystem.IsLinux())
        {
            Assert.True(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .RepositoryRootIsCanonical(
                        CodeDocumentationSavedModelAssessmentEndpoint
                            .WslRepositoryOwnedTrainingTrustedRepositoryRoot));
            Assert.False(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .RepositoryRootIsCanonical(
                        Program.RepositoryOwnedTrainingTrustedRepositoryRoot));
            Assert.False(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .RepositoryRootIsCanonical(
                        "/mnt/d/Projects/llm_mvp_core"));
            Assert.False(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .RepositoryRootIsCanonical(
                        "/mnt/c/Projects/llm_mvp_core-DpoImplementation"));
        }
    }

    private static async Task AssertDisposalFailurePreservesPrimaryErrorAsync(
        bool runCalibration)
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        var factory = new RecordingEvaluationClientFactory(
            _ => "{not-json",
            disposeAction: () => throw new InvalidOperationException(
                "simulated nonfatal disposal failure after primary error"));

        var run = await RunPreparedEndpointAsync(
            fixture,
            factory,
            runCalibration,
            runCalibration
                ? "disposal-primary-calibration"
                : "disposal-primary-assessment");

        Assert.Equal(StatusCodes.Status409Conflict, run.Response.StatusCode);
        Assert.Equal(
            CodeDocumentationSavedModelAssessmentService
                .EvaluationOutputInvalidErrorCode,
            run.Response.Json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(
            JsonValueKind.Null,
            run.Response.Json.RootElement.GetProperty("assessment").ValueKind);
        Assert.Equal(
            JsonValueKind.Null,
            run.Response.Json.RootElement.GetProperty("calibration").ValueKind);
        Assert.Equal(1, factory.CreateCalls);
        Assert.NotNull(factory.Client);
        Assert.Equal(1, factory.Client!.Observations.Count);
        Assert.Equal(1, factory.Client.IssuedCallCount);
        Assert.Equal(1, factory.Client.CompletedResponseCount);
        Assert.Single(factory.Client.Outputs);
        Assert.Equal(1, factory.Client.DisposeCalls);
        AssertSameFileSnapshot(
            run.FilesBeforeAssessment,
            SnapshotFiles(fixture.RepositoryRoot));
    }

    private static async Task AssertSuccessfulEndpointDisposalFailureAsync(
        bool runCalibration)
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        var factory = new RecordingEvaluationClientFactory(
            disposeAction: () => throw new InvalidOperationException(
                "simulated nonfatal disposal failure after success"));

        var run = await RunPreparedEndpointAsync(
            fixture,
            factory,
            runCalibration,
            runCalibration
                ? "disposal-success-calibration"
                : "disposal-success-assessment");
        var expectedCallCount = runCalibration ? 10 : 2;
        var expectedErrorCode = runCalibration
            ? CodeDocumentationSavedModelAssessmentCalibrationService
                .CalibrationConditionMismatchErrorCode
            : CodeDocumentationSavedModelAssessmentService
                .EvaluationFailedErrorCode;

        Assert.Equal(StatusCodes.Status409Conflict, run.Response.StatusCode);
        Assert.Equal(
            expectedErrorCode,
            run.Response.Json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(
            JsonValueKind.Null,
            run.Response.Json.RootElement.GetProperty("assessment").ValueKind);
        Assert.Equal(
            JsonValueKind.Null,
            run.Response.Json.RootElement.GetProperty("calibration").ValueKind);
        Assert.Equal(1, factory.CreateCalls);
        Assert.NotNull(factory.Client);
        Assert.Equal(expectedCallCount, factory.Client!.Observations.Count);
        Assert.Equal(expectedCallCount, factory.Client.IssuedCallCount);
        Assert.Equal(expectedCallCount, factory.Client.CompletedResponseCount);
        Assert.Equal(expectedCallCount, factory.Client.Outputs.Count);
        Assert.Equal(1, factory.Client.DisposeCalls);
        AssertSameFileSnapshot(
            run.FilesBeforeAssessment,
            SnapshotFiles(fixture.RepositoryRoot));
    }

    private static async Task<
        (CapturedResult Response,
            IReadOnlyDictionary<string, string> FilesBeforeAssessment)>
        RunPreparedEndpointAsync(
            AssessmentFixture fixture,
            RecordingEvaluationClientFactory factory,
            bool runCalibration,
            string compatibilitySuffix,
            CancellationToken cancellationToken = default)
    {
        var modelConfig = CreateModelConfig();
        var tokenizer = CreateTokenizer(modelConfig);
        var compatibility = CreateCompatibility(
            modelConfig,
            compatibilitySuffix);
        _ = SealModelCheckpoint(
            fixture.RepositoryRoot,
            new LlmModel(modelConfig),
            compatibility,
            CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
            step: 1,
            parent: null);
        var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
        var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
        var profile = ResolveProfile(corpus.Manifest.ManifestSha256);

        IResult result;
        if (runCalibration)
        {
            result = await CodeDocumentationSavedModelAssessmentEndpoint
                .CalibratePreparedRepositoryModelAsync(
                    CreateValidShapeRequest(),
                    fixture.RepositoryRoot,
                    DomainSlug,
                    DesignPatternSlug,
                    ModelIdSlug,
                    ModelVersion,
                    modelConfig,
                    compatibility,
                    tokenizer,
                    new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                    CreateTeacherConfiguration(),
                    profile,
                    ["test:prepared-disposal-fixture"],
                    factory,
                    DateTimeOffset.UnixEpoch,
                    cancellationToken);
        }
        else
        {
            result = await CodeDocumentationSavedModelAssessmentEndpoint
                .AssessPreparedRepositoryModelAsync(
                    CreateValidShapeRequest(),
                    fixture.RepositoryRoot,
                    DomainSlug,
                    DesignPatternSlug,
                    ModelIdSlug,
                    ModelVersion,
                    modelConfig,
                    compatibility,
                    tokenizer,
                    new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                    CreateTeacherConfiguration(),
                    profile,
                    ["test:prepared-disposal-fixture"],
                    factory,
                    DateTimeOffset.UnixEpoch,
                    cancellationToken);
        }

        return (
            await CaptureAsync(result),
            filesBeforeAssessment);
    }

    private static async Task AssertPreparedFailureBeforeCheckerCallAsync(
        Func<RepositoryOwnedReferenceCorpusManifest,
            CodeDocumentationSavedModelAssessmentProfile> profileFactory,
        string expectedErrorCode)
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(
                modelConfig,
                expectedErrorCode.ToLowerInvariant()[..16]);
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var factory = new RecordingEvaluationClientFactory();

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .AssessPreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        profileFactory(corpus.Manifest),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                expectedErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(1, factory.CreateCalls);
            Assert.Empty(factory.Client!.Observations);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    private static async Task AssertCheckerReasonFailureAsync(
        Func<CheckerObservation, string> checkerOutput,
        params string[] forbiddenResponseTexts)
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        try
        {
            var modelConfig = CreateModelConfig();
            var tokenizer = CreateTokenizer(modelConfig);
            var compatibility = CreateCompatibility(modelConfig, "reason-fail");
            SealModelCheckpoint(
                fixture.RepositoryRoot,
                new LlmModel(modelConfig),
                compatibility,
                CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
                step: 1,
                parent: null);
            var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
            var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
            var factory = new RecordingEvaluationClientFactory(checkerOutput);

            var result =
                await CodeDocumentationSavedModelAssessmentEndpoint
                    .AssessPreparedRepositoryModelAsync(
                        CreateValidShapeRequest(),
                        fixture.RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        ModelIdSlug,
                        ModelVersion,
                        modelConfig,
                        compatibility,
                        tokenizer,
                        new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                        CreateTeacherConfiguration(),
                        ResolveProfile(corpus.Manifest.ManifestSha256),
                        ["test:prepared-server-context"],
                        factory,
                        DateTimeOffset.UnixEpoch);
            var response = await CaptureAsync(result);

            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentService
                    .EvaluationOutputInvalidErrorCode,
                response.Json.RootElement.GetProperty("errorCode").GetString());
            Assert.False(
                response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("assessment").ValueKind);
            Assert.Equal(
                JsonValueKind.Null,
                response.Json.RootElement.GetProperty("calibration").ValueKind);
            var observation = Assert.Single(factory.Client!.Observations);
            Assert.DoesNotContain(
                observation.ClosedReferenceAnswer,
                response.Body,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "Reference answer",
                response.Body,
                StringComparison.OrdinalIgnoreCase);
            foreach (var forbiddenResponseText in forbiddenResponseTexts)
            {
                Assert.DoesNotContain(
                    forbiddenResponseText,
                    response.Body,
                    StringComparison.Ordinal);
            }
            AssertSameFileSnapshot(
                filesBeforeAssessment,
                SnapshotFiles(fixture.RepositoryRoot));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_IdentityPositiveControlUsesCanonicalEvaluationEvidence()
    {
        var run = await RunPreparedIdentityAssessmentAsync(
            static (configuration, observation) =>
                RecordingEvaluationOutcome.Canonical(
                    configuration,
                    observation.RequestId));

        Assert.Equal(StatusCodes.Status200OK, run.Response.StatusCode);
        Assert.True(
            run.Response.Json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.True(
            run.Response.Json.RootElement
                .GetProperty("assessment")
                .GetProperty("isSuccess")
                .GetBoolean());
        var assessment = run.Response.Json.RootElement
            .GetProperty("assessment");
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Evaluation.ProfileId,
            assessment.GetProperty("evaluationProfileId").GetString());
        Assert.DoesNotContain("observedEvaluationRequestIds", assessment.EnumerateObject()
            .Select(static property => property.Name));
        Assert.All(
            run.Factory.Client.ReturnedOutcomes,
            outcome =>
            {
                Assert.True(outcome.IssuePermit);
                Assert.Equal(
                    CreateTeacherConfiguration().ModelId,
                    outcome.Provenance.ModelId);
                Assert.Equal(
                    LocalQwenOllamaInferenceProfileAuthority.Evaluation,
                    outcome.InferenceProfile);
            });

        using var directFixture = new AssessmentFixture(privateNativeStorage: true);
        var directModelConfig = CreateModelConfig();
        var directCompatibility = CreateCompatibility(
            directModelConfig,
            "identity-direct-receipt");
        var directParent = SealModelCheckpoint(
            directFixture.RepositoryRoot,
            new LlmModel(directModelConfig),
            directCompatibility,
            CreateSealedCodeDocumentationDpoIdentity(
                directFixture.RepositoryRoot),
            step: 1,
            parent: null);
        var directCorpus = SealReferenceCorpus(
            directFixture.RepositoryRoot);
        var expectedHeldOutCaseCount = directCorpus.Corpus.Entries.Count(
            static entry =>
                entry.Partition == FrozenReferencePartition.HeldOut);
        Assert.Equal(2, expectedHeldOutCaseCount);
        var directProfile = ResolveProfile(
            directCorpus.Manifest.ManifestSha256,
            maxHeldOutCases: expectedHeldOutCaseCount);
        var directTeacherConfiguration = CreateTeacherConfiguration();
        var directFactory = new RecordingEvaluationClientFactory();
        var createdClient = directFactory.Create(directTeacherConfiguration);
        Assert.True(createdClient.IsSuccess, createdClient.ErrorMessage);
        using var directClient = Assert.IsType<RecordingEvaluationClient>(
            createdClient.Value);
        var directRequest = CreateDirectCalibrationRequest(
            LoadSavedModelCheckpoint(
                directFixture.RepositoryRoot,
                directParent,
                directModelConfig),
            CreateTokenizer(directModelConfig),
            directParent,
            directCorpus,
            directClient,
            directTeacherConfiguration,
            directProfile);

        var directResult = await new CodeDocumentationSavedModelAssessmentService()
            .AssessAsync(directRequest);

        Assert.True(directResult.IsSuccess, directResult.ErrorMessage);
        var directReport = Assert.IsType<
            CodeDocumentationSavedModelAssessmentReport>(directResult.Value);
        Assert.True(directReport.IsSuccess, directReport.HumanMessage);
        Assert.Equal(
            expectedHeldOutCaseCount,
            directReport.EvaluatedCaseCount);
        Assert.Equal(
            expectedHeldOutCaseCount,
            directClient.IssuedCallCount);
        Assert.Equal(
            expectedHeldOutCaseCount,
            directClient.CompletedResponseCount);
        Assert.Equal(
            expectedHeldOutCaseCount,
            directClient.ReturnedOutcomes.Count);

        var returnedRequestIds = directClient.ReturnedOutcomes
            .Select(static outcome => outcome.Provenance.RequestId)
            .ToArray();
        Assert.All(
            returnedRequestIds,
            static requestId =>
                Assert.False(string.IsNullOrWhiteSpace(requestId)));
        Assert.Equal(
            expectedHeldOutCaseCount,
            returnedRequestIds.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            returnedRequestIds,
            directReport.Evaluator.ObservedEvaluationRequestIds);

        Assert.Equal(
            directTeacherConfiguration.ModelId,
            directReport.Evaluator.ExpectedModelId);
        Assert.Equal(
            directTeacherConfiguration.ExpectedOllamaManifestDigest,
            directReport.Evaluator.ExpectedOllamaManifestDigest);
        Assert.Equal(
            directTeacherConfiguration.ExpectedOllamaModelLayerDigest,
            directReport.Evaluator.ExpectedOllamaModelLayerDigest);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Evaluation.ProfileId,
            directReport.Evaluator.EvaluationProfileId);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Evaluation
                .ConfigurationSha256,
            directReport.Evaluator.EvaluationProfileConfigurationSha256);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_IdentityRejectsValidOutputWithoutPermit()
    {
        var run = await RunPreparedIdentityAssessmentAsync(
            static (configuration, observation) =>
                RecordingEvaluationOutcome.Canonical(
                    configuration,
                    observation.RequestId) with
                {
                    IssuePermit = false,
                });

        AssertIdentityFailure(
            run,
            CodeDocumentationSavedModelAssessmentService
                .EvaluationFailedErrorCode);
        var returned = Assert.Single(run.Factory.Client!.ReturnedOutcomes);
        Assert.False(returned.IssuePermit);
        Assert.True(
            run.Factory.Client.PermitOutcomes.Single() == false,
            "The valid checker JSON reached the fake, but no permit was issued.");
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_IdentityRejectsAuthoringProfileWithValidEvaluationProvenance()
    {
        var run = await RunPreparedIdentityAssessmentAsync(
            static (configuration, observation) =>
                RecordingEvaluationOutcome.Canonical(
                    configuration,
                    observation.RequestId) with
                {
                    InferenceProfile =
                        LocalQwenOllamaInferenceProfileAuthority.Authoring,
                });

        AssertIdentityFailure(
            run,
            CodeDocumentationSavedModelAssessmentService
                .EvaluationProfileInvalidErrorCode);
        var returned = Assert.Single(run.Factory.Client!.ReturnedOutcomes);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Authoring,
            returned.InferenceProfile);
        AssertCanonicalEvaluationProvenance(
            returned,
            CreateTeacherConfiguration());
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_IdentityRejectsMutatedModelId()
    {
        var run = await RunPreparedIdentityAssessmentAsync(
            static (configuration, observation) =>
            {
                var canonical = RecordingEvaluationOutcome.Canonical(
                    configuration,
                    observation.RequestId);
                return canonical with
                {
                    Provenance = canonical.Provenance with
                    {
                        ModelId = "qwen-assessment-spoof:9b",
                    },
                };
            });

        AssertIdentityFailure(
            run,
            CodeDocumentationSavedModelAssessmentService
                .EvaluationProvenanceInvalidErrorCode);
        var returned = Assert.Single(run.Factory.Client!.ReturnedOutcomes);
        Assert.Equal("qwen-assessment-spoof:9b", returned.Provenance.ModelId);
        Assert.Equal(
            CreateTeacherConfiguration().ExpectedOllamaManifestDigest,
            returned.Provenance.ModelVersion);
        Assert.Equal(
            CreateTeacherConfiguration().ExpectedOllamaModelLayerDigest,
            returned.Provenance.ModelArtifactDigest);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_IdentityRejectsMutatedManifestDigest()
    {
        var run = await RunPreparedIdentityAssessmentAsync(
            static (configuration, observation) =>
            {
                var canonical = RecordingEvaluationOutcome.Canonical(
                    configuration,
                    observation.RequestId);
                return canonical with
                {
                    Provenance = canonical.Provenance with
                    {
                        ModelVersion = new string('e', 64),
                    },
                };
            });

        AssertIdentityFailure(
            run,
            CodeDocumentationSavedModelAssessmentService
                .EvaluationProvenanceInvalidErrorCode);
        var returned = Assert.Single(run.Factory.Client!.ReturnedOutcomes);
        Assert.Equal(new string('e', 64), returned.Provenance.ModelVersion);
        Assert.Equal(
            CreateTeacherConfiguration().ModelId,
            returned.Provenance.ModelId);
        Assert.Equal(
            CreateTeacherConfiguration().ExpectedOllamaModelLayerDigest,
            returned.Provenance.ModelArtifactDigest);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_IdentityRejectsMutatedLayerDigest()
    {
        var run = await RunPreparedIdentityAssessmentAsync(
            static (configuration, observation) =>
            {
                var canonical = RecordingEvaluationOutcome.Canonical(
                    configuration,
                    observation.RequestId);
                return canonical with
                {
                    Provenance = canonical.Provenance with
                    {
                        ModelArtifactDigest = new string('f', 64),
                    },
                };
            });

        AssertIdentityFailure(
            run,
            CodeDocumentationSavedModelAssessmentService
                .EvaluationProvenanceInvalidErrorCode);
        var returned = Assert.Single(run.Factory.Client!.ReturnedOutcomes);
        Assert.Equal(new string('f', 64), returned.Provenance.ModelArtifactDigest);
        Assert.Equal(
            CreateTeacherConfiguration().ModelId,
            returned.Provenance.ModelId);
        Assert.Equal(
            CreateTeacherConfiguration().ExpectedOllamaManifestDigest,
            returned.Provenance.ModelVersion);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_IdentityRejectsMutatedEvaluationProfileId()
    {
        var run = await RunPreparedIdentityAssessmentAsync(
            static (configuration, observation) =>
            {
                var canonical = RecordingEvaluationOutcome.Canonical(
                    configuration,
                    observation.RequestId);
                return canonical with
                {
                    InferenceProfile = canonical.InferenceProfile with
                    {
                        ProfileId =
                            LocalQwenOllamaInferenceProfileAuthority
                                .EvaluationProfileId + "-drift",
                    },
                };
            });

        AssertIdentityFailure(
            run,
            CodeDocumentationSavedModelAssessmentService
                .EvaluationProfileInvalidErrorCode);
        var returned = Assert.Single(run.Factory.Client!.ReturnedOutcomes);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.EvaluationProfileId +
            "-drift",
            returned.InferenceProfile.ProfileId);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Evaluation.ConfigurationSha256,
            returned.InferenceProfile.ConfigurationSha256);
    }

    [Fact]
    public async Task CodeDocumentationModelAssessment_IdentityRejectsEvaluationProfileConfigurationHashDrift()
    {
        var run = await RunPreparedIdentityAssessmentAsync(
            static (configuration, observation) =>
            {
                var canonical = RecordingEvaluationOutcome.Canonical(
                    configuration,
                    observation.RequestId);
                return canonical with
                {
                    InferenceProfile = canonical.InferenceProfile with
                    {
                        ConfigurationSha256 = new string('d', 64),
                    },
                };
            });

        AssertIdentityFailure(
            run,
            CodeDocumentationSavedModelAssessmentService
                .EvaluationProfileInvalidErrorCode);
        var returned = Assert.Single(run.Factory.Client!.ReturnedOutcomes);
        Assert.Equal(new string('d', 64), returned.InferenceProfile.ConfigurationSha256);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.EvaluationProfileId,
            returned.InferenceProfile.ProfileId);
    }

    private static async Task<
        (CapturedResult Response, RecordingEvaluationClientFactory Factory)>
        RunPreparedIdentityAssessmentAsync(
            Func<
                ContinuousLearningTp1DpoConfiguration,
                CheckerObservation,
                RecordingEvaluationOutcome> outcomeFactory)
    {
        using var fixture = new AssessmentFixture(privateNativeStorage: true);
        var modelConfig = CreateModelConfig();
        var tokenizer = CreateTokenizer(modelConfig);
        var compatibility = CreateCompatibility(modelConfig, "identity");
        SealModelCheckpoint(
            fixture.RepositoryRoot,
            new LlmModel(modelConfig),
            compatibility,
            CreateSealedCodeDocumentationDpoIdentity(fixture.RepositoryRoot),
            step: 1,
            parent: null);
        var corpus = SealReferenceCorpus(fixture.RepositoryRoot);
        var filesBeforeAssessment = SnapshotFiles(fixture.RepositoryRoot);
        var factory = new RecordingEvaluationClientFactory(
            outcomeFactory: outcomeFactory);

        var result = await CodeDocumentationSavedModelAssessmentEndpoint
            .AssessPreparedRepositoryModelAsync(
                CreateValidShapeRequest(),
                fixture.RepositoryRoot,
                DomainSlug,
                DesignPatternSlug,
                ModelIdSlug,
                ModelVersion,
                modelConfig,
                compatibility,
                tokenizer,
                new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f },
                CreateTeacherConfiguration(),
                ResolveProfile(corpus.Manifest.ManifestSha256),
                ["test:code-documentation-identity-candidate"],
                factory,
                DateTimeOffset.UnixEpoch);
        var response = await CaptureAsync(result);

        AssertSameFileSnapshot(
            filesBeforeAssessment,
            SnapshotFiles(fixture.RepositoryRoot));
        Assert.Equal(
            factory.Client!.Observations.Count,
            factory.Client.ReturnedOutcomes.Count);
        Assert.Equal(
            factory.Client.Observations.Count,
            factory.Client.Outputs.Count);
        for (var i = 0; i < factory.Client.Observations.Count; i++)
        {
            using var output = JsonDocument.Parse(factory.Client.Outputs[i]);
            Assert.Equal(
                factory.Client.Observations[i].CaseId,
                output.RootElement.GetProperty("caseId").GetString());
            Assert.True(output.RootElement.GetProperty("pass").ValueKind is
                JsonValueKind.True or JsonValueKind.False);
            Assert.NotEqual(
                JsonValueKind.Null,
                output.RootElement.GetProperty("reason").ValueKind);
        }

        return (response, factory);
    }

    private static void AssertIdentityFailure(
        (CapturedResult Response, RecordingEvaluationClientFactory Factory) run,
        string errorCode)
    {
        Assert.Equal(StatusCodes.Status409Conflict, run.Response.StatusCode);
        Assert.Equal(
            errorCode,
            run.Response.Json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(
            JsonValueKind.Null,
            run.Response.Json.RootElement.GetProperty("assessment").ValueKind);
        Assert.Equal(
            JsonValueKind.Null,
            run.Response.Json.RootElement.GetProperty("calibration").ValueKind);
        Assert.Single(run.Factory.Client!.Observations);
        Assert.Single(run.Factory.Client.ReturnedOutcomes);
    }

    private static void AssertCanonicalEvaluationProvenance(
        RecordingEvaluationOutcome outcome,
        ContinuousLearningTp1DpoConfiguration configuration)
    {
        Assert.True(outcome.IssuePermit);
        Assert.Equal(configuration.ModelId, outcome.Provenance.ModelId);
        Assert.Equal(
            configuration.ExpectedOllamaManifestDigest,
            outcome.Provenance.ModelVersion);
        Assert.Equal(
            configuration.ExpectedOllamaModelLayerDigest,
            outcome.Provenance.ModelArtifactDigest);
    }

    private static CodeDocumentationSavedModelAssessmentHttpRequest
        CreateValidShapeRequest()
        => new(
            DomainSlug,
            DesignPatternSlug,
            "code-documentation-assessment-test-run",
            ["test:code-documentation-model-assessment"]);

    private static CodeDocumentationSavedModelAssessmentRequest
        CreateDirectCalibrationRequest(
            LlmModel model,
            BpeTokenizer tokenizer,
            RepositoryOwnedModelCheckpointManifest parent,
            RepositoryOwnedReferenceCorpusOpenResult corpus,
            ILegalTeacherEvaluationInferenceClient evaluationClient,
            ContinuousLearningTp1DpoConfiguration teacherConfiguration,
            CodeDocumentationSavedModelAssessmentProfile profile)
        => new(
            model,
            tokenizer,
            new CodeDocumentationSavedModelAssessmentModelPassport(
                parent.Domain,
                parent.DesignPattern,
                parent.ModelId,
                parent.Version,
                parent.Step,
                parent.RepositoryRelativeManifestPath,
                parent.ManifestSha256,
                parent.RepositoryRelativeWeightsPath,
                parent.WeightsSha256,
                parent.WeightsByteLength,
                16L * 1024L * 1024L * 1024L),
            corpus.Corpus,
            corpus.Manifest,
            evaluationClient,
            teacherConfiguration,
            profile.GenerationSettings,
            "code-documentation-assessment-deadline",
            DateTimeOffset.UnixEpoch,
            ["test:code-documentation-calibration-deadline"]);

    private static LlmModel LoadSavedModelCheckpoint(
        string repositoryRoot,
        RepositoryOwnedModelCheckpointManifest parent,
        ModelConfig modelConfig)
    {
        var model = new LlmModel(modelConfig);
        var weightsPath = Path.Combine(
            repositoryRoot,
            parent.RepositoryRelativeWeightsPath.Replace(
                '/',
                Path.DirectorySeparatorChar));
        using var weights = File.OpenRead(weightsPath);
        model.LoadCheckpoint(weights);
        return model;
    }

    private static CodeDocumentationSavedModelAssessmentProfile ResolveProfile(
        string expectedCorpusManifestSha256,
        int maxHeldOutCases = 10,
        int maxNewTokens = 1,
        int maxAnswerChars = 256)
    {
        var json = CreateProfileJson(
            expectedCorpusManifestSha256,
            maxHeldOutCases,
            maxNewTokens,
            maxAnswerChars,
            CorpusVersion,
            SubjectSlug);
        var resolved =
            CodeDocumentationSavedModelAssessmentProfileResolver.Resolve(
                name => name == CodeDocumentationSavedModelAssessmentProfileResolver
                    .EnvironmentName
                    ? json
                    : null);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        Assert.True(resolved.Value!.TryGetValue(DomainSlug, out var profile));
        return profile!;
    }

    private static string CreateProfileJson(
        string expectedCorpusManifestSha256,
        int maxHeldOutCases,
        int maxNewTokens,
        int maxAnswerChars,
        string corpusVersion,
        string subjectSlug)
        => JsonSerializer.Serialize(new[]
        {
            new
            {
                domainSlug = DomainSlug,
                designPatternSlug = DesignPatternSlug,
                subjectSlug,
                corpusVersion,
                expectedCorpusManifestSha256,
                generation = new
                {
                    maxNewTokens,
                    temperature = 0f,
                    topK = 1,
                    topP = 1f,
                    seed = 17,
                },
                maxHeldOutCases,
                maxAnswerChars,
            },
        });

    private static string CreateCertificationProfileJson(
        string expectedCorpusManifestSha256,
        string corpusVersion,
        string subjectSlug,
        string scopeId,
        string ownerSnapshotId)
        => JsonSerializer.Serialize(new[]
        {
            new
            {
                domainSlug = DomainSlug,
                designPatternSlug = DesignPatternSlug,
                subjectSlug,
                corpusVersion,
                expectedCorpusManifestSha256,
                scopeId,
                scopeVersion = "v1",
                coverageRevision = "coverage-v1",
                coverageCutUtc = DateTimeOffset.UtcNow,
                ownerSnapshotId,
                ownerSnapshotVersion = "v1",
                generation = new
                {
                    maxNewTokens = 1,
                    temperature = 0f,
                    topK = 1,
                    topP = 1f,
                    seed = 17,
                },
                maxHeldOutCases = 10,
                maxAnswerChars = 256,
            },
        });

    private static RepositoryOwnedReferenceCorpusOpenResult SealReferenceCorpus(
        string repositoryRoot,
        string? corpusVersion = null,
        string? subjectSlug = null,
        bool certificationOnly = false)
    {
        corpusVersion ??= CorpusVersion;
        subjectSlug ??= SubjectSlug;
        var entries = certificationOnly
            ? new[]
            {
                CreateReferenceEntry(
                    "Document Add method.",
                    "Reference answer: Add returns the sum and has no side effects. " +
                    AssessmentServerFixture
                        .SyntheticCertificationReferenceSentinel,
                    FrozenReferencePartition.HeldOut,
                    "cert-heldout-add",
                    subjectSlug),
                CreateReferenceEntry(
                    "Document Subtract method.",
                    "Reference answer: Subtract returns left minus right and does not mutate inputs. " +
                    AssessmentServerFixture
                        .SyntheticCertificationReferenceSentinel,
                    FrozenReferencePartition.HeldOut,
                    "cert-heldout-subtract",
                    subjectSlug),
            }
            : new[]
            {
                CreateReferenceEntry(
                    "Document train-only helper.",
                    "Reference answer for train-only helper.",
                    FrozenReferencePartition.Train,
                    "train",
                    subjectSlug),
                CreateReferenceEntry(
                    "Document Add method.",
                    "Reference answer: Add returns the sum and has no side effects.",
                    FrozenReferencePartition.HeldOut,
                    "heldout-add",
                    subjectSlug),
                CreateReferenceEntry(
                    "Document Subtract method.",
                    "Reference answer: Subtract returns left minus right and does not mutate inputs.",
                    FrozenReferencePartition.HeldOut,
                    "heldout-subtract",
                    subjectSlug),
            };
        var corpus = FrozenReferenceCorpus.Build(
            "csharp-documentation-assessment-fixture",
            corpusVersion,
            DateTimeOffset.UnixEpoch,
            entries);
        Assert.True(corpus.IsSuccess, corpus.ErrorMessage);

        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceCorpusAddress(
                repositoryRoot,
                DomainSlug,
                subjectSlug,
                corpusVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var corpusAddress = address.Value!;
        var sealedManifest =
            RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
                corpusAddress,
                corpus.Value!);
        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);

        var opened =
            RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(
                corpusAddress);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        return opened.Value!;
    }

    private static FrozenReferenceEntry CreateReferenceEntry(
        string ownerInput,
        string referenceAnswer,
        FrozenReferencePartition partition,
        string scope,
        string? subjectSlug = null)
    {
        var provenance = PaidReferenceProvenance.Create(
            "paid-reference-fixture",
            "closed-doc-reference-fixture",
            "2026-09-09",
            "reference-request-" + scope,
            DateTimeOffset.UnixEpoch,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer));
        Assert.True(provenance.IsSuccess, provenance.ErrorMessage);
        var entry = FrozenReferenceEntry.Create(
            ownerInput,
            referenceAnswer,
            provenance.Value!,
            DomainSlug,
            subjectSlug ?? SubjectSlug,
            partition,
            "sha256:" + Sha256Hex("source-" + scope));
        Assert.True(entry.IsSuccess, entry.ErrorMessage);
        return entry.Value!;
    }

    private static RepositoryOwnedModelCheckpointManifest SealModelCheckpoint(
        string repositoryRoot,
        LlmModel model,
        RepositoryOwnedModelRuntimeCompatibility compatibility,
        RepositoryOwnedDpoDatasetManifestIdentity dpoIdentity,
        int step,
        RepositoryOwnedModelCheckpointManifest? parent,
        string? modelIdSlug = null,
        string? modelVersion = null)
    {
        modelIdSlug ??= ModelIdSlug;
        modelVersion ??= ModelVersion;
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                repositoryRoot,
                DomainSlug,
                DesignPatternSlug,
                modelIdSlug,
                modelVersion,
                step);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var request = new RepositoryOwnedModelCheckpointManifestRequest(
            ParentCheckpoint: parent is null
                ? null
                : new RepositoryOwnedModelCheckpointParentIdentity(
                    parent.RepositoryRelativeManifestPath,
                    parent.Step,
                    parent.WeightsSha256,
                    parent.WeightsByteLength),
            ColdStartReason: parent is null
                ? "first saved CodeDocumentation assessment fixture"
                : string.Empty,
            DpoDatasetManifest: dpoIdentity,
            RuntimeCompatibility: compatibility,
            CreatedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(step),
            ProvenanceRefs: ["test:code-documentation-model-assessment"]);
        var sealedManifest =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address.Value!,
                model.SaveCheckpointToStream,
                request);
        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);
        return sealedManifest.Value!;
    }

    private static RepositoryOwnedDpoDatasetManifestIdentity
        CreateSealedCodeDocumentationDpoIdentity(
            string repositoryRoot,
            string? datasetVersion = null)
    {
        datasetVersion ??= CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .DefaultDatasetVersion;
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                repositoryRoot,
                DomainSlug,
                DesignPatternSlug,
                datasetVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var datasetAddress = address.Value!;
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [Sha256Hex("paid reference dpo identity")],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen-code-doc-test:9b",
                    Hex('c'),
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    Hex('d')),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "code-documentation-assessment",
                    "v1",
                    Sha256Hex("generation policy")),
                new RepositoryOwnedDpoCreatorIdentity(
                    "code-documentation-model-assessment-tests",
                    "v1",
                    Sha256Hex("creator policy")));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        var pair = CreateRepositoryOwnedDpoPair(identity.Value!);
        var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            datasetAddress,
            identity.Value!,
            [pair]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        return new RepositoryOwnedDpoDatasetManifestIdentity(
            datasetAddress.ManifestRelativePath,
            manifest.Value!.ManifestSha256);
    }

    private static ContinuousLearning.DpoPair CreateRepositoryOwnedDpoPair(
        RepositoryOwnedDpoDatasetRequestIdentity identity)
    {
        var provenance = new TrainingTeacherProvenance(
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
            "repository-owned-assessment-fixture",
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };
        var pair = new ContinuousLearning.DpoPair(
            "Document a C# assessment fixture method.",
            "Explain the method purpose and observable behavior.",
            "Give a vague summary.",
            DomainSlug,
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = provenance,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs: ["sha256:" + Sha256Hex("governance")]),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                provenance),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.LocalModel)
        };
        var bound = DpoPairTrainingContentAuthority.Bind(pair);
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        return bound.Value!;
    }

    private static ModelConfig CreateModelConfig()
        => new()
        {
            VocabSize = 256,
            ContextLen = 64,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 1,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
            RopeTheta = 10_000f,
            RmsNormEps = 1e-5f,
        };

    private static BpeTokenizer CreateTokenizer(ModelConfig config)
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            "Document Add method. Document Subtract method. Document train-only helper. XML docs returns side effects",
            config.VocabSize);
        return tokenizer;
    }

    private static RepositoryOwnedModelRuntimeCompatibility CreateCompatibility(
        ModelConfig config,
        string keySuffix)
        => new(
            TrainingSourceProvenance.Identify(config).Sha256,
            Hex('2'),
            Hex('3'),
            Hex('4'),
            Hex('5'),
            "code-documentation-assessment-" + keySuffix);

    private static ContinuousLearningTp1DpoConfiguration
        CreateTeacherConfiguration(int maxTeacherCalls = 50)
    {
        var created = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen-assessment-test:9b",
            Hex('a'),
            Hex('b'),
            maxTeacherCalls,
            "sha256:" + Hex('c'),
            "sha256:" + Hex('d'),
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return created.Value!;
    }

    private static async Task<CapturedResult> CaptureAsync(IResult result)
    {
        var context = new DefaultHttpContext();
        await using var services = new ServiceCollection()
            .AddLogging()
            .AddRouting()
            .BuildServiceProvider();
        await using var body = new MemoryStream();
        context.RequestServices = services;
        context.Response.Body = body;
        await result.ExecuteAsync(context);
        body.Position = 0;
        using var reader = new StreamReader(body, Encoding.UTF8);
        var text = await reader.ReadToEndAsync();
        return new CapturedResult(
            context.Response.StatusCode,
            text,
            JsonDocument.Parse(text));
    }

    private static IReadOnlyList<double> ReadDoubleArray(JsonElement element)
        => element
            .EnumerateArray()
            .Select(static item => item.GetDouble())
            .ToArray();

    private static string Hex(char value) => new(value, 64);

    private static string Sha256Hex(string value)
        => Sha256Hex(Encoding.UTF8.GetBytes(value));

    private static string Sha256Hex(byte[] value)
        => Convert.ToHexString(SHA256.HashData(value))
            .ToLowerInvariant();

    private static IReadOnlyDictionary<string, string> SnapshotFiles(
        string repositoryRoot)
    {
        var fullRoot = Path.GetFullPath(repositoryRoot);
        return Directory
            .EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
            .Select(path => new KeyValuePair<string, string>(
                Path.GetRelativePath(fullRoot, path)
                    .Replace(Path.DirectorySeparatorChar, '/'),
                Sha256Hex(File.ReadAllBytes(path))))
            .OrderBy(static item => item.Key, StringComparer.Ordinal)
            .ToDictionary(
                static item => item.Key,
                static item => item.Value,
                StringComparer.Ordinal);
    }

    private static void AssertSameFileSnapshot(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after)
    {
        Assert.Equal(before.Keys, after.Keys);
        foreach (var (path, beforeHash) in before)
        {
            Assert.True(after.TryGetValue(path, out var afterHash), path);
            Assert.Equal(beforeHash, afterHash);
        }
    }

    private static void AssertOnlyFileHashChanged(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        string changedRelativePath)
    {
        Assert.Equal(before.Keys, after.Keys);
        foreach (var (path, beforeHash) in before)
        {
            Assert.True(after.TryGetValue(path, out var afterHash), path);
            if (StringComparer.Ordinal.Equals(path, changedRelativePath))
            {
                Assert.NotEqual(beforeHash, afterHash);
                continue;
            }

            Assert.Equal(beforeHash, afterHash);
        }
    }

    private static IReadOnlyDictionary<string, string>
        GenerateHeldOutAnswersFromSavedWeights(
            string repositoryRoot,
            RepositoryOwnedModelCheckpointManifest parent,
            RepositoryOwnedReferenceCorpusOpenResult corpus,
            ModelConfig modelConfig,
            CodeDocumentationSavedModelAssessmentGenerationSettings settings)
    {
        var model = new LlmModel(modelConfig);
        var weightsPath = Path.GetFullPath(Path.Combine(
            repositoryRoot,
            parent.RepositoryRelativeWeightsPath.Replace(
                '/',
                Path.DirectorySeparatorChar)));
        using (var weights = File.OpenRead(weightsPath))
        {
            model.LoadCheckpoint(weights);
        }

        var generator = new TextGenerator(model, CreateTokenizer(modelConfig));
        return corpus.Corpus.Entries
            .Where(static entry =>
                entry.Partition == FrozenReferencePartition.HeldOut)
            .OrderBy(static entry => entry.EntryId, StringComparer.Ordinal)
            .ToDictionary(
                static entry => entry.EntryId,
                entry => generator.Generate(
                    entry.OwnerInput,
                    settings.MaxNewTokens,
                    settings.Temperature,
                    settings.TopK,
                    settings.TopP,
                    settings.Seed,
                    CancellationToken.None),
                StringComparer.Ordinal);
    }

    private sealed record CapturedResult(
        int StatusCode,
        string Body,
        JsonDocument Json);

    private sealed class FactoryReplacingServiceProvider(
        IServiceProvider inner,
        ICodeDocumentationSavedModelAssessmentEvaluationClientFactory factory,
        bool suppressMaterialBoundaryProvider = false)
        : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => serviceType == typeof(IServiceProvider)
                ? this
                : serviceType == typeof(
                    ICodeDocumentationSavedModelAssessmentEvaluationClientFactory)
                    ? factory
                    : suppressMaterialBoundaryProvider && serviceType == typeof(
                        ICodeDocumentationMaterialBoundaryProvider)
                        ? null
                    : inner.GetService(serviceType);
    }

    private sealed record CheckerObservation(
        string CaseId,
        string OwnerTask,
        string StudentAnswer,
        string ClosedReferenceAnswer,
        string RequestId);

    private sealed record RecordingEvaluationOutcome(
        bool IssuePermit,
        TrainingTeacherProvenance Provenance,
        TrainingTeacherInferenceProfileEvidence InferenceProfile)
    {
        public static RecordingEvaluationOutcome Canonical(
            ContinuousLearningTp1DpoConfiguration configuration,
            string requestId)
        {
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                configuration.ModelId,
                configuration.ExpectedOllamaManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: false,
                requestId,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest =
                    configuration.ExpectedOllamaModelLayerDigest,
            };
            return new RecordingEvaluationOutcome(
                IssuePermit: true,
                provenance,
                LocalQwenOllamaInferenceProfileAuthority.Evaluation);
        }
    }

    private sealed class RecordingEvaluationClientFactory(
        Func<CheckerObservation, string>? outputFactory = null,
        Func<int, string>? requestIdFactory = null,
        Func<
            ContinuousLearningTp1DpoConfiguration,
            CheckerObservation,
            RecordingEvaluationOutcome>? outcomeFactory = null,
        Func<CheckerObservation, CancellationToken, Task>? afterPermitAsync = null,
        Action? disposeAction = null)
        : ICodeDocumentationSavedModelAssessmentEvaluationClientFactory
    {
        public int CreateCalls { get; private set; }
        public RecordingEvaluationClient? Client { get; private set; }

        public OperationResult<ILegalTeacherEvaluationInferenceClient> Create(
            ContinuousLearningTp1DpoConfiguration configuration)
        {
            CreateCalls++;
            Client = new RecordingEvaluationClient(
                configuration,
                outputFactory,
                requestIdFactory,
                outcomeFactory,
                afterPermitAsync,
                disposeAction);
            return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(
                Client);
        }
    }

    private sealed class RecordingEvaluationClient(
        ContinuousLearningTp1DpoConfiguration configuration,
        Func<CheckerObservation, string>? outputFactory,
        Func<int, string>? requestIdFactory,
        Func<
            ContinuousLearningTp1DpoConfiguration,
            CheckerObservation,
            RecordingEvaluationOutcome>? outcomeFactory,
        Func<CheckerObservation, CancellationToken, Task>? afterPermitAsync,
        Action? disposeAction)
        : ILegalTeacherEvaluationInferenceClient
    {
        private int _calls;

        public List<CheckerObservation> Observations { get; } = [];

        public List<RecordingEvaluationOutcome> ReturnedOutcomes { get; } = [];

        public List<bool> PermitOutcomes { get; } = [];

        public List<string> Outputs { get; } = [];

        private int _issuedCallCount;
        private int _completedResponseCount;

        public int IssuedCallCount =>
            Volatile.Read(ref _issuedCallCount);

        public int CompletedResponseCount =>
            Volatile.Read(ref _completedResponseCount);

        public async Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var payloadStart = prompt.IndexOf('{');
            using var document = JsonDocument.Parse(prompt[payloadStart..]);
            var root = document.RootElement;
            var callNumber = Interlocked.Increment(ref _calls);
            var requestId = requestIdFactory?.Invoke(callNumber) ??
                            "assessment-evaluation-" +
                            callNumber.ToString("D4");
            var observation = new CheckerObservation(
                root.GetProperty("caseId").GetString()!,
                root.GetProperty("ownerTask").GetString()!,
                root.GetProperty("studentAnswer").GetString()!,
                root.GetProperty("closedReferenceAnswer").GetString()!,
                requestId);
            Observations.Add(observation);

            var outcome = outcomeFactory?.Invoke(configuration, observation) ??
                          RecordingEvaluationOutcome.Canonical(
                              configuration,
                              requestId);
            ReturnedOutcomes.Add(outcome);
            if (outcome.IssuePermit)
            {
                var issued = callPermit.TryIssue();
                if (!issued.IsSuccess)
                    return issued.MapError<TeacherEvaluationInferenceResult>();

                Interlocked.Increment(ref _issuedCallCount);

                if (afterPermitAsync is not null)
                {
                    await afterPermitAsync(observation, cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            PermitOutcomes.Add(callPermit.CallIssued);

            var output = outputFactory is null
                ? JsonSerializer.Serialize(new
                {
                    caseId = observation.CaseId,
                    pass = observation.OwnerTask.Contains(
                        "Add",
                        StringComparison.Ordinal),
                    reason = observation.OwnerTask.Contains(
                        "Add",
                        StringComparison.Ordinal)
                        ? CodeDocumentationSavedModelAssessmentService
                            .CorrectReasonCode
                        : CodeDocumentationSavedModelAssessmentService
                            .MissingEssentialBehaviorReasonCode,
                })
                : outputFactory(observation);
            Outputs.Add(output);
            Interlocked.Increment(ref _completedResponseCount);
            return OperationResult<TeacherEvaluationInferenceResult>.Ok(
                new TeacherEvaluationInferenceResult(
                    output,
                    outcome.Provenance)
                {
                    InferenceProfile = outcome.InferenceProfile,
                });
        }

        private int _disposeCalls;

        public int DisposeCalls =>
            Volatile.Read(ref _disposeCalls);

        public void Dispose()
        {
            Interlocked.Increment(ref _disposeCalls);
            disposeAction?.Invoke();
        }
    }

    internal sealed class AssessmentServerFixture : IDisposable
    {
        internal const string SyntheticCertificationReferenceSentinel =
            "S03_F4_CERTIFICATION_REFERENCE_SENTINEL_20260923_7F4C2A1B\"sealed";

        private readonly Dictionary<string, string> _environment;
        private bool _disposed;

        public AssessmentServerFixture()
        {
            var lane = Guid.NewGuid().ToString("N");
            Root = Path.Combine(
                Path.GetTempPath(),
                "xiigen-s0309-assessment-endpoint-fixtures",
                lane);
            RepositoryRoot = Path.Combine(Root, "repository");

            try
            {
                Directory.CreateDirectory(Root);
                Directory.CreateDirectory(RepositoryRoot);
                File.WriteAllText(
                    Path.Combine(RepositoryRoot, "LlmFromScratch.sln"),
                    "synthetic assessment fixture repository sentinel");
                Directory.CreateDirectory(Path.Combine(
                    RepositoryRoot,
                    "14.ContinuousLearning"));
                Directory.CreateDirectory(Path.Combine(RepositoryRoot, "11.DPO"));
                Directory.CreateDirectory(Path.Combine(
                    RepositoryRoot,
                    "training-artifacts",
                    "models"));
                Directory.CreateDirectory(Path.Combine(Root, "ready"));

                DatasetVersion = "s0309-" + lane;
                SubjectSlug = "s0309-" + lane;
                CorpusVersion = "s0309-" + lane;
                ModelIdSlug = "s0309-" + lane;
                ModelVersion = "s0309-" + lane;
                ReadyFilePath = Path.Combine(
                    Root,
                    "ready",
                    "modelserver-ready.json");
                BaseCheckpoint = Path.Combine(Root, "base.bin");
                TokenizerPath = Path.Combine(Root, "tokenizer.bin");
                AdapterRoot = Path.Combine(Root, "adapter");
                CacheRoot = Path.Combine(Root, "cache");
                RegistryRoot = Path.Combine(Root, "registry");
                ModelConfiguration = CreateModelConfig();
                new LlmModel(ModelConfiguration).SaveCheckpoint(BaseCheckpoint);
                CreateTokenizer(ModelConfiguration).Save(TokenizerPath);
                Directory.CreateDirectory(AdapterRoot);
                Directory.CreateDirectory(CacheRoot);
                Directory.CreateDirectory(RegistryRoot);

                var dpoAddress = RepositoryOwnedTrainingArtifactLayout
                    .CreateDpoDatasetAddress(
                        RepositoryRoot,
                        DomainSlug,
                        DesignPatternSlug,
                        DatasetVersion);
                Assert.True(dpoAddress.IsSuccess, dpoAddress.ErrorMessage);
                DpoAddress = dpoAddress.Value!;
                DpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                    RepositoryRoot,
                    DatasetVersion);
                Directory.CreateDirectory(DpoAddress.PairsRoot);
                var corpusAddress = RepositoryOwnedTrainingArtifactLayout
                    .CreateReferenceCorpusAddress(
                        RepositoryRoot,
                        DomainSlug,
                        SubjectSlug,
                        CorpusVersion);
                Assert.True(corpusAddress.IsSuccess, corpusAddress.ErrorMessage);
                CorpusAddress = corpusAddress.Value!;
                Corpus = SealReferenceCorpus(
                    RepositoryRoot,
                    CorpusVersion,
                    SubjectSlug);
                Directory.CreateDirectory(CorpusAddress.CorpusRoot);
                CertificationCorpusVersion = "s0309-cert-" + lane;
                var certificationCorpusAddress = RepositoryOwnedTrainingArtifactLayout
                    .CreateReferenceCorpusAddress(
                        RepositoryRoot,
                        DomainSlug,
                        SubjectSlug,
                        CertificationCorpusVersion);
                Assert.True(
                    certificationCorpusAddress.IsSuccess,
                    certificationCorpusAddress.ErrorMessage);
                CertificationCorpusAddress = certificationCorpusAddress.Value!;
                CertificationCorpus = SealReferenceCorpus(
                    RepositoryRoot,
                    CertificationCorpusVersion,
                    SubjectSlug,
                    certificationOnly: true);
                Directory.CreateDirectory(CertificationCorpusAddress.CorpusRoot);
                var runtimeCompatibility =
                    new RepositoryOwnedModelRuntimeCompatibility(
                        TrainingSourceProvenance
                            .Identify(ModelConfiguration)
                            .Sha256,
                        Hex('2'),
                        Hex('3'),
                        Hex('4'),
                        Hex('5'),
                        "tp1-dpo-runtime-v0001");
                ParentManifest = SealModelCheckpoint(
                    RepositoryRoot,
                    new LlmModel(ModelConfiguration),
                    runtimeCompatibility,
                    DpoIdentity,
                    step: 1,
                    parent: null,
                    modelIdSlug: ModelIdSlug,
                    modelVersion: ModelVersion);
                ProfileJson = CreateProfileJson(
                    Corpus.Manifest.ManifestSha256,
                    maxHeldOutCases: 10,
                    maxNewTokens: 1,
                    maxAnswerChars: 256,
                    CorpusVersion,
                    SubjectSlug);
                CertificationProfileJson = CreateCertificationProfileJson(
                    CertificationCorpus.Manifest.ManifestSha256,
                    CertificationCorpusVersion,
                    SubjectSlug,
                    "synthetic-certification-scope-" + lane,
                    "synthetic-owner-" + lane);

                _environment = new Dictionary<string, string>(
                    StringComparer.Ordinal)
                {
                [ContinuousLearningModelServerConfigurationLoader
                    .EnabledEnvironmentName] = bool.TrueString,
                [ContinuousLearningModelServerConfigurationLoader
                    .ModelIdEnvironmentName] = "qwen-s0309-fixture:9b",
                [ContinuousLearningModelServerConfigurationLoader
                    .OllamaManifestDigestEnvironmentName] = Hex('d'),
                [ContinuousLearningModelServerConfigurationLoader
                    .OllamaModelLayerDigestEnvironmentName] = Hex('c'),
                [ContinuousLearningModelServerConfigurationLoader
                    .BaseModelIdEnvironmentName] = "registry/s0309-base-" + lane,
                [ContinuousLearningModelServerConfigurationLoader
                    .BaseModelRevisionEnvironmentName] = "s0309-base-revision-" + lane,
                [ContinuousLearningModelServerConfigurationLoader
                    .MaxTeacherCallsEnvironmentName] = "50",
                [ContinuousLearningModelServerConfigurationLoader
                    .AuditSaltEnvironmentName] = Convert.ToBase64String(
                    Enumerable.Range(40, 32)
                        .Select(static value => checked((byte)value))
                        .ToArray()),
                [ContinuousLearningModelServerConfigurationLoader
                    .LocalRetentionPolicyRefEnvironmentName] =
                    "sha256:" + Hex('a'),
                [ContinuousLearningModelServerConfigurationLoader
                    .OutputLicenseRefEnvironmentName] = "sha256:" + Hex('b'),
                [ContinuousLearningModelServerConfigurationLoader
                    .BaseCheckpointEnvironmentName] = BaseCheckpoint,
                [ContinuousLearningModelServerConfigurationLoader
                    .TokenizerEnvironmentName] = TokenizerPath,
                [ContinuousLearningModelServerConfigurationLoader
                    .AdapterRootEnvironmentName] = AdapterRoot,
                [ContinuousLearningModelServerConfigurationLoader
                    .CacheRootEnvironmentName] = CacheRoot,
                [ContinuousLearningModelServerConfigurationLoader
                    .RegistryRootEnvironmentName] = RegistryRoot,
                [ContinuousLearningModelServerConfigurationLoader
                    .RepositoryRootEnvironmentName] = RepositoryRoot,
                [ContinuousLearningModelServerConfigurationLoader
                    .RepositoryModelIdEnvironmentName] = ModelIdSlug,
                [ContinuousLearningModelServerConfigurationLoader
                    .RepositoryModelVersionEnvironmentName] = ModelVersion,
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeConfigSha256EnvironmentName] =
                    runtimeCompatibility.ConfigSha256,
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeTopologySha256EnvironmentName] = Hex('2'),
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeTokenizerSha256EnvironmentName] = Hex('3'),
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeTrainerSha256EnvironmentName] = Hex('4'),
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeSchemaSha256EnvironmentName] = Hex('5'),
                [ContinuousLearningModelServerConfigurationLoader
                    .RuntimeCompatibilityKeyEnvironmentName] =
                    runtimeCompatibility.CompatibilityKey,
                [ContinuousLearningModelServerConfigurationLoader
                    .DpoPairRootEnvironmentName] = DpoAddress.PairsRoot,
                [ContinuousLearningModelServerConfigurationLoader
                    .TrainingActivationEnabledEnvironmentName] = bool.TrueString,
                [ContinuousLearningModelServerConfigurationLoader
                    .TrainingRootInitializationSeedEnvironmentName] = "0",
                [Program.RepositoryOwnedTrainingEnabledEnvironmentName] =
                    bool.FalseString,
                [Program.ReadyFileEnvironmentName] = ReadyFilePath,
                [CodeDocumentationSavedModelAssessmentProfileResolver
                    .EnvironmentName] = ProfileJson,
                [CodeDocumentationCertificationProfileResolver
                    .EnvironmentName] = CertificationProfileJson,
                };
                AddExplicitModelAndTrainingConfiguration(
                    _environment,
                    ModelConfiguration);
            }
            catch
            {
                try { DeleteDirectoryIfExists(Root); }
                catch { }
                throw;
            }
        }

        public string Root { get; }
        public string RepositoryRoot { get; }
        public string DatasetVersion { get; }
        public string SubjectSlug { get; }
        public string CorpusVersion { get; }
        public string ModelIdSlug { get; }
        public string ModelVersion { get; }
        public string ReadyFilePath { get; }
        public string BaseCheckpoint { get; }
        public string TokenizerPath { get; }
        public string AdapterRoot { get; }
        public string CacheRoot { get; }
        public string RegistryRoot { get; }
        public ModelConfig ModelConfiguration { get; }
        public RepositoryOwnedDpoDatasetAddress DpoAddress { get; }
        public RepositoryOwnedDpoDatasetManifestIdentity DpoIdentity { get; }
        public RepositoryOwnedReferenceCorpusAddress CorpusAddress { get; }
        public RepositoryOwnedReferenceCorpusOpenResult Corpus { get; }
        public RepositoryOwnedReferenceCorpusOpenResult CertificationCorpus { get; }
        public RepositoryOwnedReferenceCorpusAddress CertificationCorpusAddress { get; }
        public string CertificationCorpusVersion { get; }
        public string ProfileJson { get; }
        public string CertificationProfileJson { get; }
        public RepositoryOwnedModelCheckpointManifest ParentManifest { get; }

        public string? ReadEnvironment(string name)
            => _environment.TryGetValue(name, out var value) ? value : null;

        public string[] ReadyFileArguments =>
        [
            Program.ReadyFileArgumentName,
            ReadyFilePath,
        ];

        public IDisposable ApplyEnvironment()
        {
            var values = _environment.ToDictionary(
                static pair => pair.Key,
                static pair => (string?)pair.Value,
                StringComparer.Ordinal);
            return new EnvironmentScope(values);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            DeleteDirectoryIfExists(Root);
            DeleteDirectoryIfExists(DpoAddress.DatasetRoot);
            DeleteDirectoryIfExists(CorpusAddress.CorpusRoot);
            DeleteDirectoryIfExists(CertificationCorpusAddress.CorpusRoot);
        }

        private static void AddExplicitModelAndTrainingConfiguration(
            IDictionary<string, string> environment,
            ModelConfig model)
        {
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelVocabSizeEnvironmentName] = model.VocabSize.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelContextLenEnvironmentName] = model.ContextLen.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelHiddenDimEnvironmentName] = model.HiddenDim.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelNumLayersEnvironmentName] = model.NumLayers.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelNumHeadsEnvironmentName] = model.NumHeads.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelNumKvHeadsEnvironmentName] = model.NumKvHeads.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelFfnHiddenDimEnvironmentName] = model.FfnHiddenDim.ToString();
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelRopeThetaEnvironmentName] = "10000";
            environment[ContinuousLearningModelServerConfigurationLoader
                .ModelRmsNormEpsEnvironmentName] = "1E-05";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingBatchSizeEnvironmentName] = "1";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingSeqLenEnvironmentName] = "8";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingMaxLrEnvironmentName] = "0.01";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingMinLrEnvironmentName] = "0.001";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingWarmupStepsEnvironmentName] = "0";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingTotalStepsEnvironmentName] = "10";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingGradAccumStepsEnvironmentName] = "1";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingGradClipNormEnvironmentName] = "1";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingWeightDecayEnvironmentName] = "0";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingCheckpointEveryEnvironmentName] = "1";
            environment[ContinuousLearningModelServerConfigurationLoader
                .TrainingLogEveryEnvironmentName] = "1";
        }

        private static void DeleteDirectoryIfExists(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
    }

    private sealed class AssessmentFixture : IDisposable
    {
        private static readonly string FixtureRoot = ResolveFixtureRoot();
        private readonly string _cleanupRoot;
        private readonly bool _privateNativeStorage;
        private bool _disposed;

        public AssessmentFixture(bool privateNativeStorage = false)
        {
            // Only direct/internal planning cases opt into portable local Linux
            // storage. Default HTTP roots and the required CI workspace stay unchanged.
            _privateNativeStorage = privateNativeStorage && OperatingSystem.IsLinux() &&
                !StringComparer.OrdinalIgnoreCase.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), bool.TrueString);
            _cleanupRoot = _privateNativeStorage ? Path.GetFullPath(Path.GetTempPath()) : FixtureRoot;
            Root = Path.Combine(_cleanupRoot, "assessment-" + Guid.NewGuid().ToString("N"));
            RepositoryRoot = Path.Combine(Root, "repository-root");
            if (_privateNativeStorage && OperatingSystem.IsLinux())
            {
                Directory.CreateDirectory(Root, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                Directory.CreateDirectory(RepositoryRoot, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }
            else Directory.CreateDirectory(RepositoryRoot);
            File.WriteAllText(
                Path.Combine(RepositoryRoot, "LlmFromScratch.sln"),
                "fixture solution sentinel");
            Directory.CreateDirectory(Path.Combine(
                RepositoryRoot,
                "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(
                RepositoryRoot,
                "11.DPO"));
        }

        public string Root { get; }
        public string RepositoryRoot { get; }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            DeleteDirectoryIfExists(Root);
            if (!_privateNativeStorage) TryDeleteFixtureRootIfEmpty();
        }

        private void DeleteDirectoryIfExists(string path)
        {
            var target = EnsureFixtureChild(path);
            if (Directory.Exists(target))
                Directory.Delete(target, recursive: true);
        }

        private static void TryDeleteFixtureRootIfEmpty()
        {
            var root = Path.GetFullPath(FixtureRoot);
            if (!Directory.Exists(root) ||
                Directory.EnumerateFileSystemEntries(root).Any())
            {
                return;
            }

            Directory.Delete(root);
        }

        private string EnsureFixtureChild(string path)
        {
            var root = Path.GetFullPath(_cleanupRoot).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            var target = Path.GetFullPath(path).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            if (!string.Equals(target, Path.GetFullPath(Root), comparison) || string.Equals(target, root, comparison) ||
                !target.StartsWith(
                    root + Path.DirectorySeparatorChar,
                    comparison))
            {
                throw new InvalidOperationException(
                    "Assessment fixture cleanup target is outside the allowed test-results child directory.");
            }

            return target;
        }

        private static string ResolveFixtureRoot()
        {
            var githubActions = Environment.GetEnvironmentVariable(
                "GITHUB_ACTIONS");
            var isCi = StringComparer.OrdinalIgnoreCase.Equals(
                githubActions,
                bool.TrueString);
            if (isCi)
            {
                var workspace = Environment.GetEnvironmentVariable(
                    "GITHUB_WORKSPACE");
                if (string.IsNullOrWhiteSpace(workspace) ||
                    !Path.IsPathFullyQualified(workspace) ||
                    !Directory.Exists(workspace))
                {
                    throw new InvalidOperationException(
                        "Assessment CI fixtures require an existing absolute GITHUB_WORKSPACE; local fallback is forbidden.");
                }

                var ciRoot = Path.GetFullPath(Path.Combine(
                    workspace,
                    "artifacts",
                    "test-results",
                    "code-documentation-model-assessment-fixtures"));
                Directory.CreateDirectory(ciRoot);
                return ciRoot;
            }

            return Path.GetFullPath(Path.Combine(
                OperatingSystem.IsWindows()
                    ? Program.RepositoryOwnedTrainingTrustedRepositoryRoot
                    : CodeDocumentationSavedModelAssessmentEndpoint
                        .WslRepositoryOwnedTrainingTrustedRepositoryRoot,
                "artifacts",
                "test-results",
                "code-documentation-model-assessment-fixtures"));
        }
    }

    private sealed class EnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> _prior = new(
            StringComparer.Ordinal);

        public EnvironmentScope(IReadOnlyDictionary<string, string?> values)
        {
            foreach (var (name, value) in values)
            {
                _prior[name] = Environment.GetEnvironmentVariable(name);
                Environment.SetEnvironmentVariable(name, value);
            }
        }

        public void Dispose()
        {
            foreach (var (name, value) in _prior)
                Environment.SetEnvironmentVariable(name, value);
        }
    }
}
