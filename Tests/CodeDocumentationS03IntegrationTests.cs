// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ContinuousLearning;
using ModelServer;
using TrainableRouter.Training;

namespace LlmFromScratch.Tests;

[Collection(ModelServerContinuousLearningEnvironmentCollection.Name)]
public sealed class CodeDocumentationS03IntegrationTests
{
    [Fact]
    public async Task DiagnosticAssessmentDisclosurePreventsCleanCertificationOnSameHistory()
    {
        using var fixture =
            new CodeDocumentationSavedModelAssessmentEndpointTests
                .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var authority =
            new CodeDocumentationCertificationEndpointTests
                .SyntheticCertificationAuthorityProvider(
                    fixture.RepositoryRoot,
                    fixture.ParentManifest);
        var factory =
            new CodeDocumentationCertificationEndpointTests
                .RecordingCertificationClientFactory(pass: true);
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

            using var diagnosticResponse = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint.EndpointPath,
                JsonContent.Create(CreateAssessmentRequest(
                    "s03-12-diagnostic-history")));
            var diagnosticBody =
                await diagnosticResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, diagnosticResponse.StatusCode);
            using var diagnosticJson = JsonDocument.Parse(diagnosticBody);
            Assert.True(
                diagnosticJson.RootElement.GetProperty("isSuccess")
                    .GetBoolean());
            AssertSafeHttpBody(fixture.RepositoryRoot, diagnosticBody);
            Assert.Equal(1, factory.CreateCalls);
            var diagnosticClient = factory.Client;
            Assert.NotNull(diagnosticClient);
            var diagnosticIssuedCallCount = diagnosticClient.IssuedCallCount;
            Assert.True(diagnosticIssuedCallCount > 0);
            Assert.NotEmpty(ExposureFiles(fixture.RepositoryRoot));

            using var certificationResponse = await client.PostAsync(
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationEndpointPath,
                JsonContent.Create(
                    CodeDocumentationCertificationEndpointTests.CreateRequest(
                        "s03-12-after-diagnostic-disclosure")));
            var certificationBody =
                await certificationResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Conflict, certificationResponse.StatusCode);
            using var certificationJson = JsonDocument.Parse(certificationBody);
            Assert.False(
                certificationJson.RootElement.GetProperty("isSuccess")
                    .GetBoolean());
            Assert.Equal(
                CodeDocumentationCertificationSessionStore.ErrorPrefix +
                "SOURCE_ALREADY_EXPOSED",
                certificationJson.RootElement.GetProperty("errorCode")
                    .GetString());
            Assert.Equal(
                JsonValueKind.Null,
                certificationJson.RootElement.GetProperty("certification")
                    .ValueKind);
            AssertSafeHttpBody(fixture.RepositoryRoot, certificationBody);
            Assert.Equal(1, factory.CreateCalls);
            Assert.Same(diagnosticClient, factory.Client);
            Assert.Equal(diagnosticIssuedCallCount, diagnosticClient.IssuedCallCount);
            Assert.Empty(CompletedSessionFiles(fixture.RepositoryRoot));
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CleanCertificationRouteReturnsCertifiedAndPersistsCompletedSession()
    {
        using var fixture =
            new CodeDocumentationSavedModelAssessmentEndpointTests
                .AssessmentServerFixture();
        using var environment = fixture.ApplyEnvironment();
        var authority =
            new CodeDocumentationCertificationEndpointTests
                .SyntheticCertificationAuthorityProvider(
                    fixture.RepositoryRoot,
                    fixture.ParentManifest);
        var factory =
            new CodeDocumentationCertificationEndpointTests
                .RecordingCertificationClientFactory(pass: true);
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
                CodeDocumentationSavedModelAssessmentEndpoint
                    .CertificationEndpointPath,
                JsonContent.Create(
                    CodeDocumentationCertificationEndpointTests.CreateRequest(
                        "s03-12-clean-certification")));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var json = JsonDocument.Parse(body);
            Assert.True(json.RootElement.GetProperty("isSuccess").GetBoolean());
            var certification = json.RootElement.GetProperty("certification");
            Assert.True(certification.GetProperty("certified").GetBoolean());
            Assert.False(certification.GetProperty("replayed").GetBoolean());
            Assert.Equal(
                "saved-model-calibration:s03-12-clean-certification",
                certification.GetProperty("certificationRunId")
                    .GetString());
            AssertSafeHttpBody(fixture.RepositoryRoot, body);
            Assert.Equal(1, factory.CreateCalls);
            Assert.NotNull(factory.Client);
            Assert.True(factory.Client!.IssuedCallCount > 0);

            var completedPath = Assert.Single(
                CompletedSessionFiles(fixture.RepositoryRoot));
            using var completedJson = JsonDocument.Parse(
                await File.ReadAllTextAsync(completedPath));
            Assert.Equal(
                "code-documentation-certification-session-envelope-v1",
                completedJson.RootElement.GetProperty("schema")
                    .GetString());
            Assert.Equal(
                "completed",
                completedJson.RootElement.GetProperty("payload")
                    .GetProperty("kind")
                    .GetString());
            var receipt = completedJson.RootElement.GetProperty("payload")
                .GetProperty("receipt");
            Assert.Equal(
                "saved-model-calibration:s03-12-clean-certification",
                receipt.GetProperty("calibrationRunId").GetString());
            Assert.StartsWith(
                "s03-12-clean-certification-repeat-",
                receipt.GetProperty("observations")[0]
                    .GetProperty("assessmentRunId")
                    .GetString());
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    private static object CreateAssessmentRequest(string runId)
        => new
        {
            domainSlug = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .CSharpDomainSlug,
            designPatternSlug = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .DesignPatternSlug,
            assessmentRunId = runId,
            provenanceRefs = new[] { "test:s03.12-integration" },
        };

    private static void AssertSafeHttpBody(
        string repositoryRoot,
        string body)
    {
        Assert.DoesNotContain(
            repositoryRoot,
            body,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            JsonSerializer.Serialize(repositoryRoot).Trim('"'),
            body,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "closedReferenceAnswer",
            body,
            StringComparison.Ordinal);
    }

    private static IReadOnlyList<string> ExposureFiles(string repositoryRoot)
    {
        var exposureRoot = Path.Combine(StoreRoot(repositoryRoot), "exposures");
        return Directory.Exists(exposureRoot)
            ? Directory
                .EnumerateFiles(
                    exposureRoot,
                    "exposure.json",
                    SearchOption.AllDirectories)
                .ToArray()
            : [];
    }

    private static IReadOnlyList<string> CompletedSessionFiles(
        string repositoryRoot)
    {
        var sessionsRoot = Path.Combine(StoreRoot(repositoryRoot), "sessions");
        return Directory.Exists(sessionsRoot)
            ? Directory
                .EnumerateFiles(
                    sessionsRoot,
                    "completed.json",
                    SearchOption.AllDirectories)
                .ToArray()
            : [];
    }

    private static string StoreRoot(string repositoryRoot)
        => Path.Combine(
            repositoryRoot,
            "training-artifacts",
            "models",
            CodeDocumentationCertificationSessionStore.DirectoryName);
}
