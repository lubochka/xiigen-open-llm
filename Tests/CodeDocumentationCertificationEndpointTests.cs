// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using ContinuousLearning;
using Microsoft.Extensions.DependencyInjection;
using ModelServer;
using TrainableRouter.Training;

namespace LlmFromScratch.Tests;

[Collection(ModelServerContinuousLearningEnvironmentCollection.Name)]
public sealed class CodeDocumentationCertificationEndpointTests
{
    private const int MaximumCertificationRequestUtf8Bytes = 8_192;

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_RouteIsMappedAndRejectsInvalidBodyInsteadOf404()
    {
        using var environment = new EnvironmentScope(new Dictionary<string, string?>
        {
            [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                = bool.FalseString,
            [Program.RepositoryOwnedTrainingEnabledEnvironmentName]
                = bool.FalseString,
            [CodeDocumentationCertificationProfileResolver.EnvironmentName] = null,
        });
        var builder = Program.CreateLocalLoopbackBuilder([]);
        await using var app = builder.Build();
        Program.ConfigureLocalLoopbackRoutes(app);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
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
    public async Task CodeDocumentationCertificationEndpoint_RejectsUnknownLengthOversizedBodyBeforeProfile()
    {
        using var environment = new EnvironmentScope(new Dictionary<string, string?>
        {
            [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                = bool.FalseString,
            [Program.RepositoryOwnedTrainingEnabledEnvironmentName]
                = bool.FalseString,
            [CodeDocumentationCertificationProfileResolver.EnvironmentName] = null,
        });
        var builder = Program.CreateLocalLoopbackBuilder([]);
        await using var app = builder.Build();
        Program.ConfigureLocalLoopbackRoutes(app);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            var oversized = PadRequestWithTrailingWhitespace(
                "unknown-length-oversized",
                MaximumCertificationRequestUtf8Bytes + 1);
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                new UnknownLengthUtf8JsonContent(oversized));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .InvalidCertificationRequestErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_AllowsUnknownLengthExactBoundaryBody()
    {
        using var environment = new EnvironmentScope(new Dictionary<string, string?>
        {
            [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                = bool.FalseString,
            [Program.RepositoryOwnedTrainingEnabledEnvironmentName]
                = bool.FalseString,
            [CodeDocumentationCertificationProfileResolver.EnvironmentName] = null,
        });
        var builder = Program.CreateLocalLoopbackBuilder([]);
        await using var app = builder.Build();
        Program.ConfigureLocalLoopbackRoutes(app);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            var boundary = PadRequestWithTrailingWhitespace(
                "unknown-length-boundary",
                MaximumCertificationRequestUtf8Bytes);
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                new UnknownLengthUtf8JsonContent(boundary));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.Equal(
                CodeDocumentationCertificationProfileResolver
                    .ConfigurationRequiredErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_RejectsUnknownAndDuplicateAuthorityFieldsBeforeProfile()
    {
        using var environment = new EnvironmentScope(new Dictionary<string, string?>
        {
            [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                = bool.FalseString,
            [Program.RepositoryOwnedTrainingEnabledEnvironmentName]
                = bool.FalseString,
            [CodeDocumentationCertificationProfileResolver.EnvironmentName] = null,
        });
        var builder = Program.CreateLocalLoopbackBuilder([]);
        await using var app = builder.Build();
        Program.ConfigureLocalLoopbackRoutes(app);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            const string unknown = "{\"domainSlug\":\"csharp\",\"designPatternSlug\":\"repository-owned-dpo\",\"certificationRunId\":\"run\",\"provenanceRefs\":[\"test:route\"],\"clean\":true}";
            using var unknownResponse = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                new StringContent(unknown, Encoding.UTF8, "application/json"));

            const string duplicate = "{\"domainSlug\":\"csharp\",\"domainSlug\":\"other\",\"designPatternSlug\":\"repository-owned-dpo\",\"certificationRunId\":\"run\",\"provenanceRefs\":[\"test:route\"]}";
            using var duplicateResponse = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                new StringContent(duplicate, Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, unknownResponse.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public void CodeDocumentationCertificationEndpoint_ReadyMetadataListsPostRoute()
    {
        var payload = Program.CreateLocalLoopbackReadyFilePayload(
            new Uri("http://127.0.0.1:12345"),
            ContinuousLearningModelServerStartup.Create(_ => null));
        var paths = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            payload["routePaths"]);
        var routes = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            payload["routes"]);

        Assert.Contains(
            CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
            paths);
        Assert.Contains(
            "POST " + CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
            routes);
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_PositiveRouteReturnsSafeReceipt()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var descriptor = CreateApprovedProvisioningDescriptor(fixture);
        Func<string, string?> readEnvironment = name =>
            name == CodeDocumentationCertificationProvisioning.EnvironmentName
                ? descriptor
                : fixture.ReadEnvironment(name);
        var factory = new RecordingCertificationClientFactory(pass: true);
        ICodeDocumentationCertificationAuthorityProvider? firstProvider = null;
        string? receiptIdentity = null;
        var calibratedAt = default(DateTimeOffset);
        var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            readEnvironment,
            fixture.RepositoryRoot,
            evaluationClientFactory: factory);
        try
        {
            firstProvider = app.Services.GetRequiredService<
                ICodeDocumentationCertificationAuthorityProvider>();
            Assert.IsType<
                ServerOwnedCodeDocumentationCertificationAuthorityProvider>(
                    firstProvider);
            await app.StartAsync();
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("positive-route")));
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                $"status={response.StatusCode}");
            using var json = JsonDocument.Parse(body);
            Assert.True(json.RootElement.GetProperty("isSuccess").GetBoolean());
            Assert.True(json.RootElement.GetProperty("certification")
                .GetProperty("certified").GetBoolean());
            var certification = json.RootElement.GetProperty("certification");
            receiptIdentity = certification
                .GetProperty("receiptIdentitySha256")
                .GetString();
            calibratedAt = certification
                .GetProperty("measurementCalibratedAtUtc")
                .GetDateTimeOffset();
            Assert.False(certification.GetProperty("replayed").GetBoolean());
            Assert.False(string.IsNullOrWhiteSpace(receiptIdentity));
            Assert.Equal(1, factory.CreateCalls);
            var firstClient = factory.Client;
            Assert.True(firstClient!.IssuedCallCount > 0);
            Assert.Equal(1, firstClient.DisposeCalls);
            AssertEvaluatorObservedSyntheticCertificationReference(firstClient);
            AssertResponseBodyDoesNotLeak(
                body,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                body,
                fixture);

            var completedPath = Assert.Single(
                CompletedCertificationSessionFiles(fixture.RepositoryRoot));
            using var completedJson = JsonDocument.Parse(
                await File.ReadAllTextAsync(completedPath));
            Assert.Equal(
                "code-documentation-certification-session-envelope-v1",
                completedJson.RootElement.GetProperty("schema").GetString());
            Assert.Equal(
                "completed",
                completedJson.RootElement.GetProperty("payload")
                    .GetProperty("kind").GetString());
            var durableReceipt = completedJson.RootElement.GetProperty("payload")
                .GetProperty("receipt");
            Assert.Equal(
                calibratedAt,
                durableReceipt.GetProperty("measurementNoisePassport")
                    .GetProperty("calibratedAtUtc").GetDateTimeOffset());
            Assert.Equal(
                "saved-model-calibration:positive-route",
                durableReceipt.GetProperty("calibrationRunId").GetString());
        }
        finally
        {
            await app.DisposeAsync();
        }

        Assert.NotNull(firstProvider);
        var replayFactory = new RecordingCertificationClientFactory(pass: true);
        var replayApp = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            readEnvironment,
            fixture.RepositoryRoot,
            evaluationClientFactory: replayFactory);
        try
        {
            var replayProvider = replayApp.Services.GetRequiredService<
                ICodeDocumentationCertificationAuthorityProvider>();
            Assert.IsType<
                ServerOwnedCodeDocumentationCertificationAuthorityProvider>(
                    replayProvider);
            Assert.NotSame(firstProvider, replayProvider);
            await replayApp.StartAsync();
            using var replayClient = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(replayApp),
            };
            using var replayResponse = await replayClient.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("positive-route")));
            var replayBody = await replayResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, replayResponse.StatusCode);
            using var replayJson = JsonDocument.Parse(replayBody);
            var replayCertification = replayJson.RootElement
                .GetProperty("certification");
            Assert.True(replayCertification.GetProperty("replayed").GetBoolean());
            Assert.Equal(
                receiptIdentity,
                replayCertification.GetProperty("receiptIdentitySha256").GetString());
            Assert.Equal(
                calibratedAt,
                replayCertification.GetProperty("measurementCalibratedAtUtc")
                    .GetDateTimeOffset());
            Assert.Equal(0, replayFactory.CreateCalls);
            Assert.Null(replayFactory.Client);
            AssertResponseBodyDoesNotLeak(
                replayBody,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                replayBody,
                fixture);
            Assert.Single(CompletedCertificationSessionFiles(fixture.RepositoryRoot));
        }
        finally
        {
            await replayApp.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_BelowTargetReturnsCompletedNegative()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var authority = new SyntheticCertificationAuthorityProvider(
            fixture.RepositoryRoot,
            fixture.ParentManifest);
        var factory = new RecordingCertificationClientFactory(pass: false);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: factory);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("below-target")));
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                $"status={response.StatusCode}");
            using var json = JsonDocument.Parse(body);
            Assert.True(json.RootElement.GetProperty("isSuccess").GetBoolean());
            Assert.False(json.RootElement.GetProperty("certification")
                .GetProperty("certified").GetBoolean());
            Assert.Equal(1, factory.CreateCalls);
            Assert.True(factory.Client!.IssuedCallCount > 0);
            AssertEvaluatorObservedSyntheticCertificationReference(factory.Client);
            AssertResponseBodyDoesNotLeak(
                body,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                body,
                fixture);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_MissingAuthorityFailsBeforeEvaluator()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var factory = new RecordingCertificationClientFactory(pass: true);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            evaluationClientFactory: factory);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("missing-authority")));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationAuthorityMissingErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(0, factory.CreateCalls);
            AssertResponseBodyDoesNotLeak(
                body,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                body,
                fixture);
        }
        finally
        {
            await app.DisposeAsync();
        }

        Func<string, string?> malformedReadEnvironment = name =>
            name == CodeDocumentationCertificationProvisioning.EnvironmentName
                ? "{\"schema\":"
                : fixture.ReadEnvironment(name);
        var malformedFactory = new RecordingCertificationClientFactory(pass: true);
        await using var malformedApp = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            malformedReadEnvironment,
            fixture.RepositoryRoot,
            evaluationClientFactory: malformedFactory);
        Assert.Null(malformedApp.Services.GetService<
            ICodeDocumentationCertificationAuthorityProvider>());

        await malformedApp.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(malformedApp),
            };
            using var diagnosticResponse = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.EndpointPath,
                JsonContent.Create(new
                {
                    domainSlug = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .CSharpDomainSlug,
                    designPatternSlug = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .DesignPatternSlug,
                    assessmentRunId = "malformed-provisioning-diagnostic",
                    provenanceRefs = new[] { "test:s03.11-normal-startup" },
                }));
            var diagnosticBody = await diagnosticResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, diagnosticResponse.StatusCode);
            AssertResponseBodyDoesNotLeak(
                diagnosticBody,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                diagnosticBody,
                fixture);

            var diagnosticFactoryCalls = malformedFactory.CreateCalls;
            var diagnosticClient = malformedFactory.Client;
            Assert.True(diagnosticFactoryCalls > 0);
            Assert.NotNull(diagnosticClient);
            var diagnosticEvaluationCalls = diagnosticClient.IssuedCallCount;
            Assert.True(diagnosticEvaluationCalls > 0);

            using var certificationResponse = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("malformed-provisioning")));
            var certificationBody =
                await certificationResponse.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.ServiceUnavailable,
                certificationResponse.StatusCode);
            using var certificationJson = JsonDocument.Parse(certificationBody);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationAuthorityMissingErrorCode,
                certificationJson.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(diagnosticFactoryCalls, malformedFactory.CreateCalls);
            Assert.Same(diagnosticClient, malformedFactory.Client);
            Assert.Equal(
                diagnosticEvaluationCalls,
                diagnosticClient.IssuedCallCount);
            AssertResponseBodyDoesNotLeak(
                certificationBody,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                certificationBody,
                fixture);
        }
        finally
        {
            await malformedApp.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_NormalCompositionRejectsWrongProvisioningCandidateBeforeEvaluator()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var descriptor = CreateApprovedProvisioningDescriptor(
            fixture,
            descriptorDesignPatternSlug:
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug + "-foreign");
        Func<string, string?> readEnvironment = name =>
            name == CodeDocumentationCertificationProvisioning.EnvironmentName
                ? descriptor
                : fixture.ReadEnvironment(name);
        var factory = new RecordingCertificationClientFactory(pass: true);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            readEnvironment,
            fixture.RepositoryRoot,
            evaluationClientFactory: factory);

        Assert.Null(app.Services.GetService<
            ICodeDocumentationCertificationAuthorityProvider>());
        await app.StartAsync();
        using var client = new HttpClient
        {
            BaseAddress = Program.ResolveStartedLoopbackAddress(app),
        };
        using var response = await client.PostAsync(
            CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
            JsonContent.Create(CreateRequest("wrong-provisioning-candidate")));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(
            CodeDocumentationSavedModelAssessmentEndpoint
                .CertificationAuthorityMissingErrorCode,
            json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(0, factory.CreateCalls);
        Assert.Null(factory.Client);
    }

    [Fact]
    public Task CodeDocumentationCertificationEndpoint_NormalProviderRejectsWrongProfileOwnerIdBeforeEvaluator()
        => AssertNormalProviderRejectsWrongProfileOwnerBeforeEvaluatorAsync(
            "ownerSnapshotId",
            "foreign-owner-snapshot",
            "wrong-profile-owner-id");

    [Fact]
    public Task CodeDocumentationCertificationEndpoint_NormalProviderRejectsWrongProfileOwnerVersionBeforeEvaluator()
        => AssertNormalProviderRejectsWrongProfileOwnerBeforeEvaluatorAsync(
            "ownerSnapshotVersion",
            "foreign-owner-version",
            "wrong-profile-owner-version");

    private async Task AssertNormalProviderRejectsWrongProfileOwnerBeforeEvaluatorAsync(
        string ownerField,
        string foreignOwnerValue,
        string runId)
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var profiles = JsonNode.Parse(fixture.CertificationProfileJson)!.AsArray();
        var profile = Assert.IsType<JsonObject>(Assert.Single(profiles));
        profile[ownerField] = foreignOwnerValue;
        using var profileEnvironment = new EnvironmentScope(
            new Dictionary<string, string?>
            {
                [CodeDocumentationCertificationProfileResolver.EnvironmentName]
                    = profiles.ToJsonString(),
            });
        var descriptor = CreateApprovedProvisioningDescriptor(fixture);
        Func<string, string?> readEnvironment = name =>
            name == CodeDocumentationCertificationProvisioning.EnvironmentName
                ? descriptor
                : fixture.ReadEnvironment(name);
        var factory = new RecordingCertificationClientFactory(pass: true);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            readEnvironment,
            fixture.RepositoryRoot,
            evaluationClientFactory: factory);

        Assert.IsType<
            ServerOwnedCodeDocumentationCertificationAuthorityProvider>(
                app.Services.GetRequiredService<
                    ICodeDocumentationCertificationAuthorityProvider>());
        await app.StartAsync();
        using var client = new HttpClient
        {
            BaseAddress = Program.ResolveStartedLoopbackAddress(app),
        };
        using var response = await client.PostAsync(
            CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
            JsonContent.Create(CreateRequest(runId)));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(
            CodeDocumentationCertificationProvisioning.ErrorPrefix +
                "SELECTED_CONTEXT_MISMATCH",
            json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(0, factory.CreateCalls);
        Assert.Null(factory.Client);
    }

    [Fact]
    public Task CodeDocumentationCertificationEndpoint_NormalProviderRejectsMissingProfileBeforeEvaluator()
        => AssertNormalProviderRejectsMissingOrInvalidProfileBeforeEvaluatorAsync(
            null,
            CodeDocumentationCertificationProfileResolver
                .ConfigurationRequiredErrorCode,
            "missing-certification-profile");

    [Fact]
    public Task CodeDocumentationCertificationEndpoint_NormalProviderRejectsInvalidProfileBeforeEvaluator()
        => AssertNormalProviderRejectsMissingOrInvalidProfileBeforeEvaluatorAsync(
            "[{}]",
            CodeDocumentationCertificationProfileResolver
                .ConfigurationInvalidErrorCode,
            "invalid-certification-profile");

    private async Task AssertNormalProviderRejectsMissingOrInvalidProfileBeforeEvaluatorAsync(
        string? certificationProfilesJson,
        string expectedErrorCode,
        string runId)
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var descriptor = CreateApprovedProvisioningDescriptor(fixture);
        Func<string, string?> readEnvironment = name =>
            name == CodeDocumentationCertificationProvisioning.EnvironmentName
                ? descriptor
                : fixture.ReadEnvironment(name);
        var factory = new RecordingCertificationClientFactory(pass: true);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            readEnvironment,
            fixture.RepositoryRoot,
            evaluationClientFactory: factory);
        using var profileEnvironment = new EnvironmentScope(
            new Dictionary<string, string?>
            {
                [CodeDocumentationCertificationProfileResolver.EnvironmentName]
                    = certificationProfilesJson,
            });

        Assert.IsType<
            ServerOwnedCodeDocumentationCertificationAuthorityProvider>(
                app.Services.GetRequiredService<
                    ICodeDocumentationCertificationAuthorityProvider>());
        await app.StartAsync();
        using var client = new HttpClient
        {
            BaseAddress = Program.ResolveStartedLoopbackAddress(app),
        };
        using var response = await client.PostAsync(
            CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
            JsonContent.Create(CreateRequest(runId)));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(
            expectedErrorCode,
            json.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(0, factory.CreateCalls);
        Assert.Null(factory.Client);
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_AuthorityThrowFailsClosedWithoutLeakingException()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        const string secret = "simulated authority nonfatal secret";
        var authority = new SyntheticCertificationAuthorityProvider(
            fixture.RepositoryRoot,
            fixture.ParentManifest,
            resolveAction: () => throw new InvalidOperationException(secret));
        var factory = new RecordingCertificationClientFactory(pass: true);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: factory);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("authority-throws")));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationUnavailableErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(0, factory.CreateCalls);
            AssertResponseBodyDoesNotLeak(body, secret, "exception secret");
            AssertResponseBodyDoesNotLeak(
                body,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                body,
                fixture);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_FactoryThrowFailsClosedWithoutLeakingException()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        const string secret = "simulated factory nonfatal secret";
        var authority = new SyntheticCertificationAuthorityProvider(
            fixture.RepositoryRoot,
            fixture.ParentManifest);
        var factory = new RecordingCertificationClientFactory(
            pass: true,
            createAction: () => throw new InvalidOperationException(secret));
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: factory);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("factory-throws")));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationUnavailableErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(1, factory.CreateCalls);
            Assert.Null(factory.Client);
            AssertResponseBodyDoesNotLeak(body, secret, "exception secret");
            AssertResponseBodyDoesNotLeak(
                body,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                body,
                fixture);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public Task CodeDocumentationCertificationEndpoint_FactoryFailureAfterBeginReturns503WithoutRetry()
        => AssertFactoryOutcomeAfterBeginReturns503WithoutRetry("failure");

    [Fact]
    public Task CodeDocumentationCertificationEndpoint_FactoryNullAfterBeginReturns503WithoutRetry()
        => AssertFactoryOutcomeAfterBeginReturns503WithoutRetry("null");

    private async Task AssertFactoryOutcomeAfterBeginReturns503WithoutRetry(
        string outcome)
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        const string failureCode = "S03_TEST_FACTORY_FAILURE";
        var runId = "factory-outcome-after-begin-" + outcome;
        var authority = new SyntheticCertificationAuthorityProvider(
            fixture.RepositoryRoot,
            fixture.ParentManifest);
        var failingFactory = new RecordingCertificationClientFactory(
            pass: true,
            createResultFactory: _ => outcome == "failure"
                ? OperationResult<ILegalTeacherEvaluationInferenceClient>.Fail(
                    "synthetic factory failure",
                    failureCode)
                : OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(null!));
        await using (var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: failingFactory))
        {
            await app.StartAsync();
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest(runId)));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationUnavailableErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(1, failingFactory.CreateCalls);
            Assert.Null(failingFactory.Client);
            AssertResponseBodyDoesNotLeak(
                body,
                fixture.RepositoryRoot,
                "repository root");
        }

        var replayFactory = new RecordingCertificationClientFactory(pass: true);
        await using var replayApp = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: replayFactory);
        await replayApp.StartAsync();
        try
        {
            using var replayClient = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(replayApp),
            };
            using var replayResponse = await replayClient.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest(runId)));
            var replayBody = await replayResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Conflict, replayResponse.StatusCode);
            using var replayJson = JsonDocument.Parse(replayBody);
            Assert.Equal(
                CodeDocumentationCertificationSessionStore.ErrorPrefix +
                "INCOMPLETE_EXPOSED",
                replayJson.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(0, replayFactory.CreateCalls);
            Assert.Null(replayFactory.Client);
        }
        finally
        {
            await replayApp.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_SuccessWithDisposeFailureFailsButReplaysReceipt()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        const string secret = "simulated dispose nonfatal secret after success";
        const string runId = "dispose-success-replay";
        var authority = new SyntheticCertificationAuthorityProvider(
            fixture.RepositoryRoot,
            fixture.ParentManifest);
        var failingFactory = new RecordingCertificationClientFactory(
            pass: true,
            disposeAction: () => throw new InvalidOperationException(secret));
        await using (var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: failingFactory))
        {
            await app.StartAsync();
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest(runId)));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.False(json.RootElement.GetProperty("isSuccess").GetBoolean());
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationUnavailableErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(
                JsonValueKind.Null,
                json.RootElement.GetProperty("certification").ValueKind);
            Assert.Equal(1, failingFactory.CreateCalls);
            Assert.True(failingFactory.Client!.IssuedCallCount > 0);
            AssertResponseBodyDoesNotLeak(body, secret, "exception secret");
            AssertResponseBodyDoesNotLeak(
                body,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                body,
                fixture);
        }

        var replayFactory = new RecordingCertificationClientFactory(pass: true);
        await using var replayApp = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: replayFactory);
        await replayApp.StartAsync();
        try
        {
            using var replayClient = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(replayApp),
            };
            using var replayResponse = await replayClient.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest(runId)));
            var replayBody = await replayResponse.Content.ReadAsStringAsync();

            Assert.True(
                replayResponse.StatusCode == HttpStatusCode.OK,
                $"status={replayResponse.StatusCode}");
            using var replayJson = JsonDocument.Parse(replayBody);
            var certification = replayJson.RootElement.GetProperty("certification");
            Assert.True(certification.GetProperty("replayed").GetBoolean());
            Assert.True(certification.GetProperty("certified").GetBoolean());
            Assert.NotEqual(
                default,
                certification.GetProperty("measurementCalibratedAtUtc")
                    .GetDateTimeOffset());
            Assert.Equal(0, replayFactory.CreateCalls);
            Assert.Null(replayFactory.Client);
            AssertResponseBodyDoesNotLeak(
                replayBody,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                replayBody,
                fixture);
        }
        finally
        {
            await replayApp.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_PrimaryFailureSurvivesDisposeFailure()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        const string secret = "simulated dispose nonfatal secret after primary failure";
        var authority = new SyntheticCertificationAuthorityProvider(
            fixture.RepositoryRoot,
            fixture.ParentManifest);
        var factory = new RecordingCertificationClientFactory(
            pass: true,
            disposeAction: () => throw new InvalidOperationException(secret),
            failEvaluation: true);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: factory);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("primary-failure-dispose")));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.Equal(
                RecordingCertificationClient.PrimaryEvaluationFailureErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(
                JsonValueKind.Null,
                json.RootElement.GetProperty("certification").ValueKind);
            AssertResponseBodyDoesNotLeak(body, secret, "exception secret");
            AssertResponseBodyDoesNotLeak(
                body,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                body,
                fixture);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_EvaluationThrowSurvivesDisposeFailure()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        const string secret = "simulated evaluation exception secret";
        const string disposeSecret = "simulated dispose exception secret";
        var authority = new SyntheticCertificationAuthorityProvider(
            fixture.RepositoryRoot,
            fixture.ParentManifest);
        var factory = new RecordingCertificationClientFactory(
            pass: true,
            disposeAction: () => throw new InvalidOperationException(disposeSecret),
            evaluationAction: () => throw new InvalidOperationException(secret));
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: factory);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("evaluation-throws-dispose")));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.False(json.RootElement.GetProperty("isSuccess").GetBoolean());
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentService.GenerationFailedErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(
                JsonValueKind.Null,
                json.RootElement.GetProperty("certification").ValueKind);
            Assert.Equal(1, factory.CreateCalls);
            Assert.NotNull(factory.Client);
            AssertResponseBodyDoesNotLeak(body, secret, "evaluation exception secret");
            AssertResponseBodyDoesNotLeak(body, disposeSecret, "dispose exception secret");
            AssertResponseBodyDoesNotLeak(
                body,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                body,
                fixture);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_PrimaryCancellationSurvivesDisposeFailure()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        const string secret = "simulated dispose nonfatal secret after cancellation";
        var authority = new SyntheticCertificationAuthorityProvider(
            fixture.RepositoryRoot,
            fixture.ParentManifest);
        var factory = new RecordingCertificationClientFactory(
            pass: true,
            disposeAction: () => throw new InvalidOperationException(secret),
            cancelEvaluation: true);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: factory);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("primary-cancel-dispose")));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.Equal(
                CodeDocumentationSavedModelAssessmentService.CancelledErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(
                JsonValueKind.Null,
                json.RootElement.GetProperty("certification").ValueKind);
            AssertResponseBodyDoesNotLeak(body, secret, "exception secret");
            AssertResponseBodyDoesNotLeak(
                body,
                fixture.RepositoryRoot,
                "repository root");
            AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
                body,
                fixture);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_ForeignAuthorityFailsBeforeEvaluator()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var authority = new SyntheticCertificationAuthorityProvider(
            fixture.RepositoryRoot,
            fixture.ParentManifest,
            mismatch: true);
        var factory = new RecordingCertificationClientFactory(pass: true);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: factory);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("foreign-authority")));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal(0, factory.CreateCalls);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CodeDocumentationCertificationEndpoint_ValidForeignCorpusScopeRejectsSelectedCorpusBeforeEvaluator()
    {
        using var fixture = new CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var foreignVersion = fixture.CertificationCorpusVersion + "-foreign";
        const string foreignAnswer =
            "Synthetic foreign reference: Multiply returns the product.";
        var foreignProvenance = PaidReferenceProvenance.Create(
            "paid-reference-fixture",
            "closed-doc-reference-fixture",
            "2026-09-09",
            "reference-request-c03-foreign",
            DateTimeOffset.UnixEpoch,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(foreignAnswer));
        Assert.True(foreignProvenance.IsSuccess, foreignProvenance.ErrorMessage);
        var foreignEntry = FrozenReferenceEntry.Create(
            "Document Multiply method for the foreign certification corpus.",
            foreignAnswer,
            foreignProvenance.Value!,
            fixture.CertificationCorpus.Corpus.DomainTag,
            fixture.CertificationCorpus.Corpus.SubjectId,
            FrozenReferencePartition.HeldOut,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(
                "s03-c03-foreign-multiply-source"));
        Assert.True(foreignEntry.IsSuccess, foreignEntry.ErrorMessage);
        var built = FrozenReferenceCorpus.Build(
            fixture.CertificationCorpus.Corpus.CorpusName,
            foreignVersion,
            fixture.CertificationCorpus.Corpus.FrozenAtUtc,
            [foreignEntry.Value!, fixture.CertificationCorpus.Corpus.Entries[1]]);
        Assert.True(built.IsSuccess, built.ErrorMessage);
        Assert.NotEqual(
            fixture.CertificationCorpus.Corpus.CorpusSha256,
            built.Value!.CorpusSha256);
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
            fixture.RepositoryRoot,
            fixture.CertificationCorpus.Corpus.DomainTag,
            fixture.CertificationCorpus.Corpus.SubjectId,
            foreignVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var sealedForeign = RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
            address.Value!, built.Value!);
        Assert.True(sealedForeign.IsSuccess, sealedForeign.ErrorMessage);
        var openedForeign = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(
            address.Value!);
        Assert.True(openedForeign.IsSuccess, openedForeign.ErrorMessage);
        Assert.NotEqual(
            fixture.CertificationCorpus.Manifest.ManifestSha256,
            openedForeign.Value!.Manifest.ManifestSha256);

        var authority = new SyntheticCertificationAuthorityProvider(
            fixture.RepositoryRoot,
            fixture.ParentManifest,
            scopeCorpus: openedForeign.Value);
        var factory = new RecordingCertificationClientFactory(pass: true);
        await using var app = Program.CreateLocalLoopbackHostCore(
            fixture.ReadyFileArguments,
            fixture.ReadEnvironment,
            fixture.RepositoryRoot,
            certificationAuthorityProvider: authority,
            evaluationClientFactory: factory);

        await app.StartAsync();
        try
        {
            var filesBefore = SnapshotRepositoryFiles(fixture.RepositoryRoot);
            using var client = new HttpClient
            {
                BaseAddress = Program.ResolveStartedLoopbackAddress(app),
            };
            using var response = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.CertificationEndpointPath,
                JsonContent.Create(CreateRequest("valid-foreign-corpus")));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.Equal(
                CodeDocumentationCertificationService.AdmissionRejectedErrorCode,
                json.RootElement.GetProperty("errorCode").GetString());
            Assert.Equal(1, authority.ResolveCalls);
            Assert.True(authority.ReturnedOkScope);
            Assert.Equal(
                fixture.CertificationCorpus.Manifest.ManifestSha256,
                authority.ObservedCorpusManifestSha256);
            var foreignScope = Assert.IsType<ServerOwnedCodeDocumentationCertificationScope>(
                authority.IssuedScope);
            Assert.Equal(
                openedForeign.Value.Manifest.ManifestSha256,
                foreignScope.Corpus.Manifest.ManifestSha256);
            var admittedForForeignCorpus = CodeDocumentationCertificationAdmission.TryCreate(
                foreignScope,
                foreignScope.Candidate,
                openedForeign.Value,
                foreignScope.Index);
            Assert.True(admittedForForeignCorpus.IsSuccess,
                admittedForForeignCorpus.ErrorMessage);
            Assert.Equal(0, factory.CreateCalls);
            Assert.Null(factory.Client);
            var filesAfter = SnapshotRepositoryFiles(fixture.RepositoryRoot);
            Assert.Equal(
                filesBefore.Keys.OrderBy(static path => path, StringComparer.Ordinal),
                filesAfter.Keys.OrderBy(static path => path, StringComparer.Ordinal));
            foreach (var (path, hash) in filesBefore)
                Assert.Equal(hash, filesAfter[path]);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    private static IReadOnlyDictionary<string, string> SnapshotRepositoryFiles(
        string repositoryRoot)
        => Directory.EnumerateFiles(repositoryRoot, "*", SearchOption.AllDirectories)
            .ToDictionary(
                path => Path.GetRelativePath(repositoryRoot, path)
                    .Replace(Path.DirectorySeparatorChar, '/'),
                path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                StringComparer.Ordinal);

    private static IReadOnlyList<string> CompletedCertificationSessionFiles(
        string repositoryRoot)
    {
        var sessionsRoot = Path.Combine(
            repositoryRoot,
            "training-artifacts",
            "models",
            CodeDocumentationCertificationSessionStore.DirectoryName,
            "sessions");
        return Directory.Exists(sessionsRoot)
            ? Directory.EnumerateFiles(
                sessionsRoot,
                "completed.json",
                SearchOption.AllDirectories).ToArray()
            : [];
    }

    private static string CreateApprovedProvisioningDescriptor(
        CodeDocumentationSavedModelAssessmentEndpointTests.AssessmentServerFixture
            fixture,
        string? descriptorDesignPatternSlug = null)
    {
        var resolvedProfiles = CodeDocumentationCertificationProfileResolver
            .Resolve(fixture.ReadEnvironment);
        Assert.True(
            resolvedProfiles.IsSuccess,
            resolvedProfiles.ErrorMessage);
        var profile = Assert.Single(resolvedProfiles.Value!.Values);
        var corpus = fixture.CertificationCorpus;
        var checkpoint = fixture.ParentManifest;
        Assert.Equal(
            profile.ExpectedCorpusManifestSha256,
            corpus.Manifest.ManifestSha256);
        Assert.Equal(profile.DomainSlug, corpus.Manifest.Domain);
        Assert.Equal(profile.SubjectSlug, corpus.Manifest.Subject);
        Assert.Equal(profile.DomainSlug, checkpoint.Domain);
        Assert.Equal(profile.DesignPatternSlug, checkpoint.DesignPattern);

        const string approvalRef = "s03-c06-normal-startup-approved-snapshot";
        var history = new
        {
            training = "complete",
            diagnostic = "complete",
            teaching = "complete",
        };
        var entries = corpus.Corpus.Entries;
        var sourceBindings = entries.Select(entry => new
        {
            entryId = entry.EntryId,
            sourceEntityFingerprint = entry.SourceEntityFingerprint,
            ownerSnapshotId = profile.OwnerSnapshotId,
            ownerSnapshotVersion = profile.OwnerSnapshotVersion,
        }).ToArray();
        var sources = entries.Select(entry => new
        {
            sourceEntityFingerprint = entry.SourceEntityFingerprint,
            domain = profile.DomainSlug,
            subject = profile.SubjectSlug,
            ownerSnapshotId = profile.OwnerSnapshotId,
            ownerSnapshotVersion = profile.OwnerSnapshotVersion,
            sourceInput = entry.OwnerInput,
            originEvidenceRef = FrozenReferenceEntry
                .ComputeNormalizedTextSha256("s03-c06-normal-startup-origin"),
            roles = new[] { "reference-acquisition-only" },
            lineageKind = "known-root",
            ancestors = Array.Empty<string>(),
        }).ToArray();
        var snapshot = new
        {
            schema = CodeDocumentationCertificationProvisioning.SnapshotSchema,
            approvalRef,
            declaration = new
            {
                domain = profile.DomainSlug,
                designPattern = profile.DesignPatternSlug,
                subject = profile.SubjectSlug,
                scopeId = profile.ScopeId,
                scopeVersion = profile.ScopeVersion,
                purpose = CodeDocumentationCertificationScopeDeclaration
                    .CertificationPurpose,
                coverageCutUtc = profile.CoverageCutUtc.UtcDateTime
                    .ToString("O"),
                coverageRevision = profile.CoverageRevision,
                checkpointLineageManifestIdentities =
                    new[] { checkpoint.ManifestSha256 },
                ownerSnapshots = new[]
                {
                    new
                    {
                        id = profile.OwnerSnapshotId,
                        version = profile.OwnerSnapshotVersion,
                    },
                },
                history,
                certificationSourceBindings = sourceBindings,
            },
            exposureIndex = new
            {
                version = profile.ScopeVersion,
                coveredCheckpointManifestSha256 = checkpoint.ManifestSha256,
                history,
                sources,
            },
        };
        var payloadBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(snapshot));
        var snapshotHash = Convert.ToHexString(SHA256.HashData(payloadBytes))
            .ToLowerInvariant();
        var relativePath =
            $"training-artifacts/assessments/{profile.DomainSlug}/{profile.DesignPatternSlug}/provisioning/{snapshotHash}.json";
        var provisioningDirectory = Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "assessments",
            profile.DomainSlug,
            profile.DesignPatternSlug,
            "provisioning");
        Directory.CreateDirectory(Path.GetDirectoryName(provisioningDirectory)!);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(
            provisioningDirectory);
        var snapshotPath = Path.Combine(
            fixture.RepositoryRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        using (var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                   snapshotPath,
                   FileMode.CreateNew,
                   FileAccess.ReadWrite,
                   FileShare.None,
                   FileOptions.WriteThrough,
                   bufferSize: 4096))
        {
            stream.Write(payloadBytes);
            stream.Flush(flushToDisk: true);
        }

        var descriptor = new
        {
            schema = CodeDocumentationCertificationProvisioning.DescriptorSchema,
            snapshots = new[]
            {
                new
                {
                    domainSlug = profile.DomainSlug,
                    designPatternSlug = descriptorDesignPatternSlug ??
                        profile.DesignPatternSlug,
                    subjectSlug = profile.SubjectSlug,
                    scopeId = profile.ScopeId,
                    scopeVersion = profile.ScopeVersion,
                    ownerSnapshotId = profile.OwnerSnapshotId,
                    ownerSnapshotVersion = profile.OwnerSnapshotVersion,
                    approvalRef,
                    snapshotRelativePath = relativePath,
                    snapshotSha256 = snapshotHash,
                    snapshotByteLength = payloadBytes.LongLength,
                },
            },
        };
        return JsonSerializer.Serialize(descriptor);
    }

    internal static string CreateApprovedProvisioningDescriptorForLinkedRecovery(
        string repositoryRoot,
        CodeDocumentationCertificationProfile profile,
        RepositoryOwnedReferenceCorpusOpenResult certificationCorpus,
        RepositoryOwnedModelCheckpointManifest parentManifest,
        RepositoryOwnedModelCheckpointManifest recoveredChildManifest,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .RepositoryOwnedCodeDocumentationDpoSetup parentDpoSetup,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests
            .RepositoryOwnedCodeDocumentationDpoSetup dpoSetup,
        IReadOnlyList<FrozenReferenceEntry> persistedDiagnosticEntries)
    {
        Assert.Equal(profile.ExpectedCorpusManifestSha256,
            certificationCorpus.Manifest.ManifestSha256);
        Assert.Equal(profile.DomainSlug, certificationCorpus.Manifest.Domain);
        Assert.Equal(profile.SubjectSlug, certificationCorpus.Manifest.Subject);
        Assert.Equal(profile.DomainSlug, parentManifest.Domain);
        Assert.Equal(profile.DesignPatternSlug, parentManifest.DesignPattern);
        Assert.Equal(profile.DomainSlug, recoveredChildManifest.Domain);
        Assert.Equal(profile.DesignPatternSlug, recoveredChildManifest.DesignPattern);
        Assert.Equal(
            parentManifest.RepositoryRelativeManifestPath,
            recoveredChildManifest.ParentCheckpoint?.RepositoryRelativeManifestPath);
        Assert.Equal(parentManifest.Step,
            recoveredChildManifest.ParentCheckpoint?.Step);
        Assert.Equal(parentManifest.WeightsSha256,
            recoveredChildManifest.ParentCheckpoint?.CheckpointSha256);
        Assert.Equal(parentManifest.WeightsByteLength,
            recoveredChildManifest.ParentCheckpoint?.CheckpointByteLength);
        Assert.NotEqual(parentDpoSetup.Identity.ManifestSha256,
            dpoSetup.Identity.ManifestSha256);
        var trainingDatasets = new[]
        {
            (Checkpoint: parentManifest, Setup: parentDpoSetup),
            (Checkpoint: recoveredChildManifest, Setup: dpoSetup),
        };

        var entries = certificationCorpus.Corpus.Entries;
        Assert.NotEmpty(entries);
        Assert.All(entries, entry =>
        {
            Assert.Equal(FrozenReferencePartition.HeldOut, entry.Partition);
            Assert.Equal(profile.DomainSlug, entry.DomainTag);
            Assert.Equal(profile.SubjectSlug, entry.SubjectId);
        });
        Assert.All(persistedDiagnosticEntries, entry =>
        {
            Assert.Equal(FrozenReferencePartition.HeldOut, entry.Partition);
            Assert.Equal(profile.DomainSlug, entry.DomainTag);
            Assert.Equal(profile.SubjectSlug, entry.SubjectId);
        });
        var certificationFingerprints = entries
            .Select(entry => entry.SourceEntityFingerprint)
            .ToHashSet(StringComparer.Ordinal);
        var diagnosticFingerprints = persistedDiagnosticEntries
            .Select(entry => entry.SourceEntityFingerprint)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Empty(certificationFingerprints.Intersect(
            diagnosticFingerprints,
            StringComparer.Ordinal));
        var certificationInputs = entries
            .Select(entry => FrozenReferenceEntry.ComputeNormalizedTextSha256(
                entry.OwnerInput))
            .ToHashSet(StringComparer.Ordinal);
        var diagnosticInputs = persistedDiagnosticEntries
            .Select(entry => FrozenReferenceEntry.ComputeNormalizedTextSha256(
                entry.OwnerInput))
            .ToHashSet(StringComparer.Ordinal);
        Assert.Empty(certificationInputs.Intersect(
            diagnosticInputs,
            StringComparer.Ordinal));

        // The seeded parent step and the recovered child consume different
        // sealed pairs. Include the exact pair fields for both checkpoints.
        var trainingMaterial = trainingDatasets.SelectMany(dataset =>
        {
            var setup = dataset.Setup;
            Assert.Equal(setup.Identity.RepositoryRelativeManifestPath,
                dataset.Checkpoint.DpoDatasetManifest.RepositoryRelativeManifestPath);
            Assert.Equal(setup.Identity.ManifestSha256,
                dataset.Checkpoint.DpoDatasetManifest.ManifestSha256);
            Assert.Equal(setup.Receipt.ManifestSha256,
                dataset.Checkpoint.DpoDatasetManifest.ManifestSha256);
            Assert.Equal(setup.RequestIdentity.RequestIdentitySha256,
                setup.Receipt.RequestIdentitySha256);
            var sealedDataset = RepositoryOwnedDpoDatasetManifestSealer
                .OpenSealed(setup.Address, setup.RequestIdentity);
            Assert.True(sealedDataset.IsSuccess, sealedDataset.ErrorMessage);
            var opened = sealedDataset.Value!;
            var manifest = opened.Manifest;
            Assert.Equal(setup.Receipt.ManifestSha256,
                manifest.ManifestSha256);
            Assert.Equal(setup.Receipt.RequestIdentitySha256,
                manifest.RequestIdentity.RequestIdentitySha256);
            Assert.Equal(setup.Receipt.PairCount, manifest.PairCount);
            Assert.Equal(setup.Pairs.Length, manifest.PairCount);
            Assert.Equal(setup.Receipt.OrderedPairs, manifest.OrderedPairs);
            Assert.Equal(setup.Pairs.Length, opened.Pairs.Count);
            return Enumerable.Range(0, setup.Pairs.Length)
                .SelectMany(index =>
            {
                var pair = setup.Pairs[index];
                var sealedPair = opened.Pairs[index];
                var pairRecord = manifest.OrderedPairs[index];
                Assert.Equal(pair.Prompt, sealedPair.Prompt);
                Assert.Equal(pair.Chosen, sealedPair.Chosen);
                Assert.Equal(pair.Rejected, sealedPair.Rejected);
                Assert.False(string.IsNullOrWhiteSpace(pairRecord.PairId));
                Assert.False(string.IsNullOrWhiteSpace(
                    pairRecord.PairFileSha256));
                return new[]
                {
                    (RequestHash: setup.Receipt.RequestIdentitySha256,
                        ManifestHash: setup.Receipt.ManifestSha256,
                        Pair: pairRecord, Field: "prompt", Input: pair.Prompt),
                    (RequestHash: setup.Receipt.RequestIdentitySha256,
                        ManifestHash: setup.Receipt.ManifestSha256,
                        Pair: pairRecord, Field: "chosen", Input: pair.Chosen),
                    (RequestHash: setup.Receipt.RequestIdentitySha256,
                        ManifestHash: setup.Receipt.ManifestSha256,
                        Pair: pairRecord, Field: "rejected", Input: pair.Rejected),
                };
            }).ToArray();
        }).ToArray();
        Assert.Equal(trainingDatasets.Sum(dataset => dataset.Setup.Pairs.Length) * 3,
            trainingMaterial.Length);
        var trainingInputs = trainingMaterial
            .Select(material => FrozenReferenceEntry
                .ComputeNormalizedTextSha256(material.Input))
            .ToHashSet(StringComparer.Ordinal);
        Assert.Equal(trainingMaterial.Length, trainingInputs.Count);
        Assert.Empty(certificationInputs.Intersect(
            trainingInputs,
            StringComparer.Ordinal));
        Assert.Empty(diagnosticInputs.Intersect(
            trainingInputs,
            StringComparer.Ordinal));

        const string approvalRefPrefix = "s03-linked-recovery-certification-";
        var approvalRef = approvalRefPrefix +
            recoveredChildManifest.ManifestSha256[..16];
        var history = new
        {
            training = "complete",
            diagnostic = "complete",
            teaching = "complete",
        };
        var sourceBindings = entries.Select(entry => new
        {
            entryId = entry.EntryId,
            sourceEntityFingerprint = entry.SourceEntityFingerprint,
            ownerSnapshotId = profile.OwnerSnapshotId,
            ownerSnapshotVersion = profile.OwnerSnapshotVersion,
        }).ToArray();
        var certificationSources = entries.Select(entry => new
        {
            sourceEntityFingerprint = entry.SourceEntityFingerprint,
            domain = profile.DomainSlug,
            subject = profile.SubjectSlug,
            ownerSnapshotId = profile.OwnerSnapshotId,
            ownerSnapshotVersion = profile.OwnerSnapshotVersion,
            sourceInput = entry.OwnerInput,
            originEvidenceRef = entry.PaidReferenceProvenance
                .ReferenceAnswerSha256,
            roles = new[] { "reference-acquisition-only" },
            lineageKind = "known-root",
            ancestors = Array.Empty<string>(),
        }).ToArray();
        var diagnosticSources = persistedDiagnosticEntries.Select(entry => new
        {
            sourceEntityFingerprint = entry.SourceEntityFingerprint,
            domain = profile.DomainSlug,
            subject = profile.SubjectSlug,
            ownerSnapshotId = profile.OwnerSnapshotId,
            ownerSnapshotVersion = profile.OwnerSnapshotVersion,
            sourceInput = entry.OwnerInput,
            originEvidenceRef = entry.PaidReferenceProvenance
                .ReferenceAnswerSha256,
            roles = new[] { "teacher-visible-diagnostic" },
            lineageKind = "known-root",
            ancestors = Array.Empty<string>(),
        }).ToArray();
        var trainingSources = trainingMaterial.Select(material => new
        {
            sourceEntityFingerprint = FrozenReferenceEntry
                .ComputeNormalizedTextSha256(
                    "s03-linked-recovery-dpo-training-source|" +
                    material.RequestHash + "|" +
                    material.ManifestHash + "|" +
                    material.Pair.PairId + "|" +
                    material.Pair.PairFileSha256 + "|" +
                    material.Field + "|" +
                    FrozenReferenceEntry.ComputeNormalizedTextSha256(
                        material.Input)),
            domain = profile.DomainSlug,
            subject = profile.SubjectSlug,
            ownerSnapshotId = profile.OwnerSnapshotId,
            ownerSnapshotVersion = profile.OwnerSnapshotVersion,
            sourceInput = material.Input,
            originEvidenceRef = "sha256:" + material.Pair.PairFileSha256,
            roles = new[] { "training" },
            lineageKind = "known-root",
            ancestors = Array.Empty<string>(),
        }).ToArray();
        var trainingFingerprints = trainingSources
            .Select(source => source.sourceEntityFingerprint)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(trainingSources, source =>
        {
            Assert.True(FrozenReferenceCanonicalization.IsCanonicalSha256Ref(
                source.sourceEntityFingerprint));
            Assert.True(FrozenReferenceCanonicalization.IsCanonicalSha256Ref(
                source.originEvidenceRef));
        });
        Assert.Equal(trainingSources.Length, trainingFingerprints.Count);
        Assert.Empty(certificationFingerprints.Intersect(
            trainingFingerprints,
            StringComparer.Ordinal));
        Assert.Empty(diagnosticFingerprints.Intersect(
            trainingFingerprints,
            StringComparer.Ordinal));
        var sources = certificationSources.Concat(diagnosticSources)
            .Concat(trainingSources).ToArray();
        Assert.All(sources, source => Assert.Empty(source.ancestors));
        var certificationLineage = certificationSources
            .SelectMany(source => new[] { source.sourceEntityFingerprint }
                .Concat(source.ancestors))
            .ToHashSet(StringComparer.Ordinal);
        var diagnosticLineage = diagnosticSources
            .SelectMany(source => new[] { source.sourceEntityFingerprint }
                .Concat(source.ancestors))
            .ToHashSet(StringComparer.Ordinal);
        var trainingLineage = trainingSources
            .SelectMany(source => new[] { source.sourceEntityFingerprint }
                .Concat(source.ancestors))
            .ToHashSet(StringComparer.Ordinal);
        Assert.Empty(certificationLineage.Intersect(
            diagnosticLineage,
            StringComparer.Ordinal));
        Assert.Empty(certificationLineage.Intersect(
            trainingLineage,
            StringComparer.Ordinal));
        var checkpointLineageManifestIdentities = new[]
        {
            parentManifest.ManifestSha256,
            recoveredChildManifest.ManifestSha256,
        };
        Assert.Equal(
            new[]
            {
                parentManifest.ManifestSha256,
                recoveredChildManifest.ManifestSha256,
            },
            checkpointLineageManifestIdentities);
        var snapshot = new
        {
            schema = CodeDocumentationCertificationProvisioning.SnapshotSchema,
            approvalRef,
            declaration = new
            {
                domain = profile.DomainSlug,
                designPattern = profile.DesignPatternSlug,
                subject = profile.SubjectSlug,
                scopeId = profile.ScopeId,
                scopeVersion = profile.ScopeVersion,
                purpose = CodeDocumentationCertificationScopeDeclaration
                    .CertificationPurpose,
                coverageCutUtc = profile.CoverageCutUtc.UtcDateTime
                    .ToString("O"),
                coverageRevision = profile.CoverageRevision,
                checkpointLineageManifestIdentities,
                ownerSnapshots = new[]
                {
                    new
                    {
                        id = profile.OwnerSnapshotId,
                        version = profile.OwnerSnapshotVersion,
                    },
                },
                history,
                certificationSourceBindings = sourceBindings,
            },
            exposureIndex = new
            {
                version = profile.ScopeVersion,
                coveredCheckpointManifestSha256 =
                    recoveredChildManifest.ManifestSha256,
                history,
                sources,
            },
        };
        var payloadBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(snapshot));
        var snapshotHash = Convert.ToHexString(SHA256.HashData(payloadBytes))
            .ToLowerInvariant();
        var relativePath =
            $"training-artifacts/assessments/{profile.DomainSlug}/{profile.DesignPatternSlug}/provisioning/{snapshotHash}.json";
        var provisioningDirectory = Path.Combine(
            repositoryRoot,
            "training-artifacts",
            "assessments",
            profile.DomainSlug,
            profile.DesignPatternSlug,
            "provisioning");
        Directory.CreateDirectory(Path.GetDirectoryName(provisioningDirectory)!);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(
            provisioningDirectory);
        var snapshotPath = Path.Combine(
            repositoryRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        using (var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                   snapshotPath,
                   FileMode.CreateNew,
                   FileAccess.ReadWrite,
                   FileShare.None,
                   FileOptions.WriteThrough,
                   bufferSize: 4096))
        {
            stream.Write(payloadBytes);
            stream.Flush(flushToDisk: true);
        }

        var descriptor = new
        {
            schema = CodeDocumentationCertificationProvisioning.DescriptorSchema,
            snapshots = new[]
            {
                new
                {
                    domainSlug = profile.DomainSlug,
                    designPatternSlug = profile.DesignPatternSlug,
                    subjectSlug = profile.SubjectSlug,
                    scopeId = profile.ScopeId,
                    scopeVersion = profile.ScopeVersion,
                    ownerSnapshotId = profile.OwnerSnapshotId,
                    ownerSnapshotVersion = profile.OwnerSnapshotVersion,
                    approvalRef,
                    snapshotRelativePath = relativePath,
                    snapshotSha256 = snapshotHash,
                    snapshotByteLength = payloadBytes.LongLength,
                },
            },
        };
        return JsonSerializer.Serialize(descriptor);
    }

    internal static object CreateRequest(string runId)
        => new
        {
            domainSlug = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .CSharpDomainSlug,
            designPatternSlug = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .DesignPatternSlug,
            certificationRunId = runId,
            provenanceRefs = new[] { "test:s03.11-endpoint" },
        };

    private static string PadRequestWithTrailingWhitespace(
        string runId,
        int targetUtf8Bytes)
    {
        var json = JsonSerializer.Serialize(CreateRequest(runId));
        var byteCount = Encoding.UTF8.GetByteCount(json);
        Assert.True(
            byteCount <= targetUtf8Bytes,
            $"request is {byteCount} bytes; target={targetUtf8Bytes}");
        return json + new string(' ', targetUtf8Bytes - byteCount);
    }

    private static void AssertResponseBodyDoesNotLeak(
        string responseBody,
        string sensitiveValue,
        string valueDescription)
    {
        Assert.False(
            responseBody.Contains(sensitiveValue, StringComparison.Ordinal),
            "response body leaked " + valueDescription);
        Assert.False(
            responseBody.Contains(
                JsonEscapedValue(sensitiveValue),
                StringComparison.Ordinal),
            "response body leaked JSON-escaped " + valueDescription);
    }

    private static void AssertResponseBodyDoesNotLeakSyntheticCertificationReferences(
        string responseBody,
        CodeDocumentationSavedModelAssessmentEndpointTests.AssessmentServerFixture
            fixture)
    {
        var sentinel = CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture
            .SyntheticCertificationReferenceSentinel;
        AssertResponseBodyDoesNotLeak(
            responseBody,
            sentinel,
            "synthetic certification reference sentinel");
        AssertResponseBodyDoesNotLeak(
            responseBody,
            JsonEscapedValue(sentinel),
            "JSON-escaped synthetic certification reference sentinel");

        foreach (var referenceAnswer in fixture.CertificationCorpus.Corpus.Entries
                     .Where(static entry =>
                         entry.Partition == FrozenReferencePartition.HeldOut)
                     .Select(static entry => entry.PaidModelReferenceAnswer))
        {
            AssertResponseBodyDoesNotLeak(
                responseBody,
                referenceAnswer,
                "full certification reference answer");
            AssertResponseBodyDoesNotLeak(
                responseBody,
                JsonEscapedValue(referenceAnswer),
                "JSON-escaped full certification reference answer");
        }
    }

    private static void AssertEvaluatorObservedSyntheticCertificationReference(
        RecordingCertificationClient client)
    {
        var sentinel = CodeDocumentationSavedModelAssessmentEndpointTests
            .AssessmentServerFixture
            .SyntheticCertificationReferenceSentinel;
        Assert.True(
            client.Observations.Any(observation =>
                observation.ClosedReferenceAnswer.Contains(
                    sentinel,
                    StringComparison.Ordinal)),
            "evaluator did not receive the synthetic certification reference sentinel");
    }

    private static string JsonEscapedValue(string value)
    {
        var serialized = JsonSerializer.Serialize(value);
        Assert.True(
            serialized.Length >= 2,
            "serialized sensitive value should be a JSON string");
        return serialized[1..^1];
    }

    internal sealed class SyntheticCertificationAuthorityProvider(
        string expectedRepositoryRoot,
        RepositoryOwnedModelCheckpointManifest expectedParent,
        bool mismatch = false,
        Action? resolveAction = null,
        RepositoryOwnedReferenceCorpusOpenResult? scopeCorpus = null)
        : ICodeDocumentationCertificationAuthorityProvider
    {
        private static readonly CertificationHistoryCoverage Complete = new(
            CertificationHistoryCompleteness.Complete,
            CertificationHistoryCompleteness.Complete,
            CertificationHistoryCompleteness.Complete);

        public int ResolveCalls { get; private set; }
        public bool ReturnedOkScope { get; private set; }
        public string? ObservedCorpusManifestSha256 { get; private set; }
        public ServerOwnedCodeDocumentationCertificationScope? IssuedScope { get; private set; }

        public OperationResult<ServerOwnedCodeDocumentationCertificationScope> Resolve(
            string trustedRepositoryRoot,
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest startRequest,
            RepositoryOwnedModelCheckpointManifest parent,
            RepositoryOwnedReferenceCorpusOpenResult corpus,
            CodeDocumentationCertificationProfile profile,
            ContinuousLearningTp1DpoConfiguration evaluator)
        {
            ResolveCalls++;
            ObservedCorpusManifestSha256 = corpus.Manifest.ManifestSha256;
            resolveAction?.Invoke();
            if (!StringComparer.Ordinal.Equals(
                    trustedRepositoryRoot,
                    expectedRepositoryRoot) ||
                !StringComparer.Ordinal.Equals(
                    startRequest.RepositoryRoot,
                    expectedRepositoryRoot) ||
                parent.ManifestSha256 != expectedParent.ManifestSha256)
            {
                return OperationResult<ServerOwnedCodeDocumentationCertificationScope>
                    .Fail("foreign certification authority", "FOREIGN_AUTHORITY");
            }

            if (mismatch)
            {
                return OperationResult<ServerOwnedCodeDocumentationCertificationScope>
                    .Fail("foreign certification authority", "FOREIGN_AUTHORITY");
            }

            var selectedCorpus = scopeCorpus ?? corpus;

            var declaration = new CodeDocumentationCertificationScopeDeclaration(
                CodeDocumentationCertificationScopeDeclaration.CurrentSchema,
                profile.DomainSlug,
                profile.DesignPatternSlug,
                profile.SubjectSlug,
                profile.ScopeId,
                profile.ScopeVersion,
                CodeDocumentationCertificationScopeDeclaration.CertificationPurpose,
                profile.CoverageCutUtc,
                profile.CoverageRevision,
                [parent.ManifestSha256],
                [new CertificationOwnerSnapshot(
                    profile.OwnerSnapshotId,
                    profile.OwnerSnapshotVersion)],
                Complete,
                selectedCorpus.Corpus.Entries.Select(entry => new CertificationSourceBinding(
                    entry.EntryId,
                    entry.SourceEntityFingerprint,
                    profile.OwnerSnapshotId,
                    profile.OwnerSnapshotVersion)));
            var exposure = new CodeDocumentationCertificationExposureIndex(
                CodeDocumentationCertificationExposureIndex.CurrentSchema,
                profile.ScopeVersion,
                declaration,
                parent.ManifestSha256,
                Complete,
                selectedCorpus.Corpus.Entries.Select(entry => new CodeDocumentationCertificationSource(
                    entry.SourceEntityFingerprint,
                    profile.DomainSlug,
                    profile.SubjectSlug,
                    profile.OwnerSnapshotId,
                    profile.OwnerSnapshotVersion,
                    entry.OwnerInput,
                    FrozenReferenceEntry.ComputeNormalizedTextSha256(
                        "synthetic-certification-origin"),
                    CertificationExposureRole.ReferenceAcquisitionOnly,
                    CertificationLineageKind.KnownRoot,
                    [])));
            var passport = new CodeDocumentationSavedModelAssessmentModelPassport(
                expectedParent.Domain,
                expectedParent.DesignPattern,
                expectedParent.ModelId,
                expectedParent.Version,
                expectedParent.Step,
                expectedParent.RepositoryRelativeManifestPath,
                expectedParent.ManifestSha256,
                expectedParent.RepositoryRelativeWeightsPath,
                expectedParent.WeightsSha256,
                expectedParent.WeightsByteLength,
                startRequest.MaximumCheckpointBytes,
                CodeDocumentationSavedModelAssessmentModelPassport
                    .AcceptedRepositoryCheckpointSourceKind,
                expectedParent.RepositoryRelativeManifestPath,
                expectedParent.ManifestSha256);
            if (!CertificationValidation.StructuralScope(declaration))
                return OperationResult<ServerOwnedCodeDocumentationCertificationScope>
                    .Fail("scope", "DEBUG_SCOPE");
            if (!CertificationValidation.StructuralIndex(exposure))
                return OperationResult<ServerOwnedCodeDocumentationCertificationScope>
                    .Fail("index", "DEBUG_INDEX");
            if (!CertificationValidation.Candidate(passport))
                return OperationResult<ServerOwnedCodeDocumentationCertificationScope>
                    .Fail("candidate", "DEBUG_CANDIDATE");
            if (!CertificationValidation.Corpus(selectedCorpus))
                return OperationResult<ServerOwnedCodeDocumentationCertificationScope>
                    .Fail("corpus", "DEBUG_CORPUS");
            var created = ServerOwnedCodeDocumentationCertificationScope
                .CreateFromServerOwnedSnapshot(
                    declaration,
                    passport,
                    selectedCorpus,
                    profile.GenerationSettings,
                    evaluator,
                    MeasurementNoisePolicyConfig.Canonical,
                    exposure);
            if (!created.IsSuccess || created.Value is null)
            {
                return OperationResult<ServerOwnedCodeDocumentationCertificationScope>
                    .Fail(created.ErrorMessage, created.ErrorCode);
            }

            IssuedScope = created.Value;

            var admitted = CodeDocumentationCertificationAdmission.TryCreate(
                created.Value,
                passport,
                selectedCorpus,
                exposure);
            ReturnedOkScope = admitted.IsSuccess;
            return admitted.IsSuccess
                ? created
                : OperationResult<ServerOwnedCodeDocumentationCertificationScope>
                    .Fail(admitted.ErrorMessage, admitted.ErrorCode);
        }
    }

    internal sealed record CertificationEvaluationObservation(
        string CaseId,
        string ClosedReferenceAnswer);

    internal sealed class RecordingCertificationClientFactory(
        bool pass,
        Action? createAction = null,
        Action? disposeAction = null,
        bool failEvaluation = false,
        bool cancelEvaluation = false,
        Action? evaluationAction = null,
        Func<ContinuousLearningTp1DpoConfiguration,
            OperationResult<ILegalTeacherEvaluationInferenceClient>>?
            createResultFactory = null)
        : ICodeDocumentationSavedModelAssessmentEvaluationClientFactory
    {
        public int CreateCalls { get; private set; }
        public RecordingCertificationClient? Client { get; private set; }

        public OperationResult<ILegalTeacherEvaluationInferenceClient> Create(
            ContinuousLearningTp1DpoConfiguration configuration)
        {
            CreateCalls++;
            createAction?.Invoke();
            if (createResultFactory is not null)
                return createResultFactory(configuration);
            Client = new RecordingCertificationClient(
                configuration,
                pass,
                disposeAction,
                failEvaluation,
                cancelEvaluation,
                evaluationAction);
            return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(Client);
        }
    }

    internal sealed class RecordingCertificationClient(
        ContinuousLearningTp1DpoConfiguration configuration,
        bool pass,
        Action? disposeAction = null,
        bool failEvaluation = false,
        bool cancelEvaluation = false,
        Action? evaluationAction = null)
        : ILegalTeacherEvaluationInferenceClient
    {
        public const string PrimaryEvaluationFailureErrorCode =
            "S03_TEST_PRIMARY_EVALUATION_FAILURE";

        private int _calls;
        private int _issuedCallCount;
        private int _completedResponseCount;
        private int _disposeCalls;

        public int IssuedCallCount => Volatile.Read(ref _issuedCallCount);
        public int CompletedResponseCount =>
            Volatile.Read(ref _completedResponseCount);
        public int DisposeCalls => Volatile.Read(ref _disposeCalls);

        public List<CertificationEvaluationObservation> Observations { get; } = [];

        public Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(
            string prompt,
            ILegalTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var document = JsonDocument.Parse(
                prompt[prompt.IndexOf('{')..]);
            var root = document.RootElement;
            var observation = new CertificationEvaluationObservation(
                root.GetProperty("caseId").GetString()!,
                root.GetProperty("closedReferenceAnswer").GetString()!);
            Observations.Add(observation);

            evaluationAction?.Invoke();

            if (cancelEvaluation)
                throw new OperationCanceledException(cancellationToken);

            if (failEvaluation)
            {
                var failedIssue = callPermit.TryIssue();
                if (!failedIssue.IsSuccess)
                {
                    return Task.FromResult(
                        failedIssue.MapError<TeacherEvaluationInferenceResult>());
                }

                Interlocked.Increment(ref _issuedCallCount);
                return Task.FromResult(
                    OperationResult<TeacherEvaluationInferenceResult>.Fail(
                        "simulated primary evaluation failure",
                        PrimaryEvaluationFailureErrorCode));
            }

            var caseId = observation.CaseId;
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
            {
                return Task.FromResult(
                    issued.MapError<TeacherEvaluationInferenceResult>());
            }

            Interlocked.Increment(ref _issuedCallCount);
            var call = Interlocked.Increment(ref _calls);
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
                "s03.11-certification-endpoint-" + call.ToString("D4"),
                InvocationCount: 1)
            {
                ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = configuration.ExpectedOllamaModelLayerDigest,
            };
            var output = JsonSerializer.Serialize(new
            {
                caseId,
                pass,
                reason = pass
                    ? CodeDocumentationSavedModelAssessmentService.CorrectReasonCode
                    : CodeDocumentationSavedModelAssessmentService
                        .MissingEssentialBehaviorReasonCode,
            });
            Interlocked.Increment(ref _completedResponseCount);
            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Ok(
                    new TeacherEvaluationInferenceResult(output, provenance)
                    {
                        InferenceProfile = LocalQwenOllamaInferenceProfileAuthority
                            .Evaluation,
                    }));
        }

        public void Dispose()
        {
            Interlocked.Increment(ref _disposeCalls);
            disposeAction?.Invoke();
        }
    }

    private sealed class UnknownLengthUtf8JsonContent : HttpContent
    {
        private readonly byte[] _bytes;

        public UnknownLengthUtf8JsonContent(string content)
        {
            _bytes = Encoding.UTF8.GetBytes(content);
            Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = Encoding.UTF8.WebName,
            };
        }

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context)
            => stream.WriteAsync(_bytes, 0, _bytes.Length);

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context,
            CancellationToken cancellationToken)
            => stream.WriteAsync(_bytes, cancellationToken).AsTask();

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
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
