// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Net;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using ModelServer.Http;
using TrainableRouter;
using TrainableRouter.Training;
using ClDpoPair = ContinuousLearning.DpoPair;

public sealed class CodeDocumentationRepositoryOwnedDpoTrainingModelServerClientTests
{
    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://192.168.1.10:6060")]
    [InlineData("http://user:secret@localhost:6060")]
    [InlineData("http://localhost:6060?target=/internal")]
    [InlineData("http://localhost:6060/#fragment")]
    [InlineData("http://localhost:6060/internal")]
    public async Task TrainAndPublishAsync_RejectsNonLoopbackOrNonRootBaseUriBeforePost(
        string rawUri)
    {
        var transport = new CapturingTransport(
            _ => throw new InvalidOperationException(
                "remote or non-root URI must not be posted"));
        var client =
            new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(
                transport);

        var result = await client.TrainAndPublishAsync(
            CreateRequest(CreateReceipt(), new Uri(rawUri)));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient
                .InvalidRequestErrorCode,
            result.ErrorCode);
        Assert.Equal(0, transport.Calls);
    }

    [Theory]
    [InlineData("http://127.0.0.1:6060")]
    [InlineData("https://localhost:6060/")]
    [InlineData("http://[::1]:6060")]
    public async Task TrainAndPublishAsync_AcceptsLoopbackRootBaseUri(
        string rawUri)
    {
        var receipt = CreateReceipt();
        var transport = new CapturingTransport(
            _ => SuccessTransportResponse(CreateSuccessResponse(receipt)));
        var client =
            new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(
                transport);

        var result = await client.TrainAndPublishAsync(
            CreateRequest(receipt, new Uri(rawUri)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, transport.Calls);
        Assert.NotNull(transport.BaseUri);
        Assert.True(transport.BaseUri!.IsLoopback);
        Assert.Equal("/", transport.BaseUri.AbsolutePath);
        Assert.Equal("", transport.BaseUri.Query);
        Assert.Equal("", transport.BaseUri.Fragment);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(86_401)]
    public async Task TrainAndPublishAsync_RejectsTimeoutOutsideBoundBeforePost(
        int timeoutSeconds)
    {
        var transport = new CapturingTransport(
            _ => throw new InvalidOperationException(
                "invalid timeout must not be posted"));
        var client =
            new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(
                transport);

        var result = await client.TrainAndPublishAsync(
            CreateRequest(
                CreateReceipt(),
                timeout: TimeSpan.FromSeconds(timeoutSeconds)));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient
                .InvalidRequestErrorCode,
            result.ErrorCode);
        Assert.Equal(0, transport.Calls);
    }

    [Theory]
    [MemberData(nameof(PublicationDomainCases))]
    public async Task TrainAndPublishAsync_PostsExactlyOnceAndKeepsDomainSpecificCheckpointContract(
        string expectedDomainSlug,
        string expectedDpoManifestRelativePath,
        string expectedPublishedWeightsRelativePath,
        string expectedPublishedManifestRelativePath)
    {
        var receipt = CreateReceipt(expectedDomainSlug);
        var transport = new CapturingTransport(
            _ => OperationResult<
                    ModelServerJsonPostResponse<
                        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>>
                .Ok(new ModelServerJsonPostResponse<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>(
                    200,
                    true,
                    CreateSuccessResponse(receipt, expectedDomainSlug),
                    "{}")));
        var client =
            new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(
                transport);

        var result = await client.TrainAndPublishAsync(
            CreateRequest(receipt, domainSlug: expectedDomainSlug));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            expectedDpoManifestRelativePath,
            result.Value!.DpoManifestRelativePath);
        Assert.Equal(
            expectedPublishedWeightsRelativePath,
            result.Value.PublishedWeightsRelativePath);
        Assert.Equal(
            expectedPublishedManifestRelativePath,
            result.Value.PublishedManifestRelativePath);
        Assert.Equal(1, transport.Calls);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingWireContract.EndpointPath,
            transport.RelativePath);
        Assert.Equal(new Uri("http://127.0.0.1:6060"), transport.BaseUri);
        Assert.Equal(TimeSpan.FromSeconds(5), transport.Timeout);
        var payload = Assert.IsType<
            CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest>(
            transport.Payload);
        Assert.Equal("run-bridge-test", payload.TrainingRunId);
        Assert.Equal(expectedDomainSlug, payload.DomainSlug);
        Assert.Equal(
            expectedDpoManifestRelativePath,
            payload.RoutingRun.RepositoryOwnedDpoReceipt
                ?.RepositoryRelativeManifestPath);
        // Этот payload-чек предотвращает тихую подмену SQL на другой домен
        // ещё до того, как fake ModelServer вернёт checkpoint evidence.
        Assert.All(
            payload.RoutingRun.RepositoryOwnedDpoPairs,
            pair => Assert.Equal(expectedDomainSlug, pair.Domain));

        var json = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        foreach (var forbidden in new[]
                 {
                     "repositoryRoot",
                     "modelConfig",
                     "tokenizer",
                     "loraConfig",
                     "runtimeCompatibility",
                     "coldStartPolicy",
                     "dpoStepCount",
                     "leaseBroker",
                     "lease"
                 })
        {
            Assert.DoesNotContain(
                $"\"{forbidden}\"",
                json,
                StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotContain(@"C:\", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\\", json, StringComparison.Ordinal);
        Assert.DoesNotContain("/mnt/", json, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("unavailable")]
    [InlineData("timeout")]
    [InlineData("rejection")]
    public async Task TrainAndPublishAsync_UnavailableTimeoutOrRejectionFailClosed(
        string scenario)
    {
        var receipt = CreateReceipt();
        var transport = new CapturingTransport(_ => scenario switch
        {
            "unavailable" => OperationResult<
                    ModelServerJsonPostResponse<
                        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>>
                .Fail(
                    "fake unavailable",
                    HttpClientModelServerJsonTransport.UnavailableErrorCode),
            "timeout" => OperationResult<
                    ModelServerJsonPostResponse<
                        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>>
                .Fail(
                    "fake timeout",
                    HttpClientModelServerJsonTransport.TimeoutErrorCode),
            _ => OperationResult<
                    ModelServerJsonPostResponse<
                        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>>
                .Ok(new ModelServerJsonPostResponse<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>(
                    409,
                    false,
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse
                        .Failure("FAKE_SERVER_REJECTION", "fake rejected"),
                    "{}"))
        });
        var client =
            new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(
                transport);

        var result = await client.TrainAndPublishAsync(
            CreateRequest(receipt));

        Assert.False(result.IsSuccess);
        Assert.Equal(1, transport.Calls);
        Assert.NotEqual(
            CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient
                .UnsafeResponsePathErrorCode,
            result.ErrorCode);
        Assert.NotEqual(
            CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient
                .ManifestMismatchErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task TrainAndPublishAsync_ResponseDpoMismatchFailsClosed()
    {
        var receipt = CreateReceipt();
        var response = CreateSuccessResponse(receipt) with
        {
            DpoManifestSha256 = new string('9', 64)
        };
        var client =
            new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(
                new CapturingTransport(
                    _ => SuccessTransportResponse(response)));

        var result = await client.TrainAndPublishAsync(
            CreateRequest(receipt));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient
                .ManifestMismatchErrorCode,
            result.ErrorCode);
    }

    [Theory]
    [InlineData("non-hex-dpo-hash")]
    [InlineData("wrong-step")]
    [InlineData("wrong-path-step")]
    [InlineData("wrong-domain")]
    [InlineData("wrong-filename")]
    [InlineData("unsafe-evidence")]
    [InlineData("huge-evidence-count")]
    [InlineData("huge-evidence-item")]
    [InlineData("success-with-error-text")]
    [InlineData("null-parent")]
    [InlineData("contradictory-quality-decision")]
    public async Task TrainAndPublishAsync_InvalidSuccessEvidenceFailsClosed(
        string scenario)
    {
        var receipt = CreateReceipt();
        var response = scenario switch
        {
            "non-hex-dpo-hash" => CreateSuccessResponse(receipt) with
            {
                DpoManifestSha256 = new string('z', 64)
            },
            "wrong-step" => CreateSuccessResponse(receipt) with
            {
                PublishedCheckpointStep = 3,
                PublishedWeightsRelativePath =
                    "training-artifacts/models/angular-code-documentation/ilearning-unit/code-documentation-llm/v1/checkpoints/000003/weights.bin",
                PublishedManifestRelativePath =
                    "training-artifacts/models/angular-code-documentation/ilearning-unit/code-documentation-llm/v1/checkpoints/000003/manifest.json",
            },
            "wrong-path-step" => CreateSuccessResponse(receipt) with
            {
                PublishedWeightsRelativePath =
                    "training-artifacts/models/angular-code-documentation/ilearning-unit/code-documentation-llm/v1/checkpoints/000003/weights.bin"
            },
            "wrong-domain" => CreateSuccessResponse(receipt) with
            {
                PublishedWeightsRelativePath =
                    "training-artifacts/models/sql-code-documentation/ilearning-unit/code-documentation-llm/v1/checkpoints/000002/weights.bin"
            },
            "wrong-filename" => CreateSuccessResponse(receipt) with
            {
                PublishedManifestRelativePath =
                    "training-artifacts/models/angular-code-documentation/ilearning-unit/code-documentation-llm/v1/checkpoints/000002/model.json"
            },
            "unsafe-evidence" => CreateSuccessResponse(receipt) with
            {
                EvidenceRefs =
                [
                    "repository_model_manifest:training-artifacts/models/angular-code-documentation/ilearning-unit/code-documentation-llm/v1/checkpoints/000002/manifest.json",
                    "file://C:/Projects/llm_mvp_core/secret.txt"
                ]
            },
            "huge-evidence-count" => CreateSuccessResponse(receipt) with
            {
                EvidenceRefs = Enumerable
                    .Range(
                        0,
                        CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                            .MaximumEvidenceRefCount + 1)
                    .Select(index => "test:evidence-" + index)
                    .ToArray()
            },
            "huge-evidence-item" => CreateSuccessResponse(receipt) with
            {
                EvidenceRefs =
                [
                    "test:" + new string(
                        'x',
                        CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                            .MaximumEvidenceRefLength + 1)
                ]
            },
            "success-with-error-text" => CreateSuccessResponse(receipt) with
            {
                ErrorCode = "SHOULD_NOT_BE_ON_SUCCESS"
            },
            "null-parent" => CreateSuccessResponse(receipt) with
            {
                ParentCheckpointStep = null
            },
            "contradictory-quality-decision" => CreateSuccessResponse(receipt) with
            {
                QualityDecision = CreateQualityDecision(
                    baselineMeanCorrectFraction: 1d,
                    trialMeanCorrectFraction: 0d,
                    difference: -1d,
                    noiseFloor: 0d)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var client =
            new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(
                new CapturingTransport(
                    _ => SuccessTransportResponse(response)));

        var result = await client.TrainAndPublishAsync(CreateRequest(receipt));

        Assert.False(result.IsSuccess);
        if (scenario == "unsafe-evidence")
        {
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient
                    .UnsafeResponseEvidenceErrorCode,
                result.ErrorCode);
        }
    }

    [Theory]
    [InlineData(@"C:\Projects\llm_mvp_core\training-artifacts\models\weights.bin")]
    [InlineData(@"\\server\share\training-artifacts\models\weights.bin")]
    [InlineData("/training-artifacts/models/weights.bin")]
    public async Task TrainAndPublishAsync_RootedResponsePathsFailClosed(
        string unsafePath)
    {
        var receipt = CreateReceipt();
        var response = CreateSuccessResponse(receipt) with
        {
            PublishedWeightsRelativePath = unsafePath
        };
        var client =
            new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(
                new CapturingTransport(
                    _ => SuccessTransportResponse(response)));

        var result = await client.TrainAndPublishAsync(
            CreateRequest(receipt));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient
                .UnsafeResponsePathErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task TrainAndPublishAsync_WithoutDpoReceiptFailsBeforePost()
    {
        var transport = new CapturingTransport(
            _ => throw new InvalidOperationException(
                "transport must not be called"));
        var client =
            new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(
                transport);

        var result = await client.TrainAndPublishAsync(
            CreateRequest(null));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient
                .ReceiptRequiredErrorCode,
            result.ErrorCode);
        Assert.Equal(0, transport.Calls);
    }

    private static OperationResult<
            ModelServerJsonPostResponse<
                CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>>
        SuccessTransportResponse(
            CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse response)
        => OperationResult<ModelServerJsonPostResponse<
            CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>>.Ok(
            new ModelServerJsonPostResponse<
                CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>(
                200,
                true,
                response,
                "{}"));

    private static CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest
        CreateRequest(
            CodeDocumentationRepositoryOwnedDpoDatasetReceipt? receipt,
            Uri? baseUri = null,
            TimeSpan? timeout = null,
            string domainSlug =
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AngularDomainSlug)
        => new(
            baseUri ?? new Uri("http://127.0.0.1:6060"),
            domainSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug,
            "v7",
            "run-bridge-test",
            CreateRunResult(receipt, domainSlug),
            ["test:code-doc-cli-bridge"],
            timeout ?? TimeSpan.FromSeconds(5));

    // Синтетический run evidence держит домен рядом с DPO receipt:
    // client proof покрывает request JSON, а не только response path validator.
    private static CodeDocumentationTrainingRunResult CreateRunResult(
        CodeDocumentationRepositoryOwnedDpoDatasetReceipt? receipt,
        string domainSlug =
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AngularDomainSlug)
        => new(
            50,
            50,
            50,
            50,
            50,
            50,
            1.0,
            1.0,
            1.0,
            1.0,
            1.0,
            1.0,
            1.0,
            12.5,
            64.0,
            25.0,
            [],
            receipt,
            receipt is null
                ? []
                : [
                    new ClDpoPair(
                        "repository prompt",
                        "repository chosen",
                        "repository rejected",
                        domainSlug,
                        DateTimeOffset.UnixEpoch)
                ],
            []);

    // Receipt paths строятся из домена запроса: SQL обязан использовать свой
    // sealed manifest, а не старую Angular fixture.
    private static CodeDocumentationRepositoryOwnedDpoDatasetReceipt
        CreateReceipt(
            string domainSlug =
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AngularDomainSlug)
        => new(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            new string('a', 64),
            CreateDpoManifestRelativePath(domainSlug),
            1,
            new string('b', 64),
            [
                new RepositoryOwnedDpoDatasetManifestPair(
                    "pair-001",
                    CreateDpoPairRelativePath(domainSlug, "pair-001"),
                    new string('c', 64))
            ]);

    // Success fixture возвращает только checkpoint shape для домена запроса:
    // это даёт SQL явное positive acceptance proof.
    private static CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse
        CreateSuccessResponse(
            CodeDocumentationRepositoryOwnedDpoDatasetReceipt receipt,
            string domainSlug =
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AngularDomainSlug)
        => new(
            true,
            "",
            "",
            "DPO для документации кода открыт из репозитория и опубликован ModelServer.",
            receipt.RepositoryRelativeManifestPath,
            receipt.ManifestSha256,
            receipt.PairCount,
            1,
            2,
            CreatePublishedWeightsRelativePath(domainSlug),
            CreatePublishedManifestRelativePath(domainSlug),
            ["test:published"],
            CreateQualityDecision());

    private static CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionHttpEvidence
        CreateQualityDecision(
            double baselineMeanCorrectFraction = 0d,
            double trialMeanCorrectFraction = 1d,
            double? difference = null,
            double noiseFloor = 0.01d)
        => new(
            MeasurementNoisePolicy.ImprovedStatus,
            baselineMeanCorrectFraction,
            trialMeanCorrectFraction,
            difference ?? trialMeanCorrectFraction - baselineMeanCorrectFraction,
            noiseFloor,
            ExpectedEvaluationCallCount: 50,
            ObservedEvaluationCallCount: 50);

    public static TheoryData<string, string, string, string>
        PublicationDomainCases =>
        new()
        {
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AngularDomainSlug,
                CreateDpoManifestRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .AngularDomainSlug),
                CreatePublishedWeightsRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .AngularDomainSlug),
                CreatePublishedManifestRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .AngularDomainSlug)
            },
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .SqlDomainSlug,
                CreateDpoManifestRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .SqlDomainSlug),
                CreatePublishedWeightsRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .SqlDomainSlug),
                CreatePublishedManifestRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .SqlDomainSlug)
            }
        };

    private static string CreateDpoManifestRelativePath(string domainSlug)
        => $"training-data/dpo/{domainSlug}/ilearning-unit/v7/manifest.json";

    private static string CreateDpoPairRelativePath(
        string domainSlug,
        string pairId)
        => $"training-data/dpo/{domainSlug}/ilearning-unit/v7/pairs/{pairId}.dpo-pair.json";

    private static string CreatePublishedWeightsRelativePath(string domainSlug)
        => "training-artifacts/models/" +
           $"{domainSlug}/ilearning-unit/code-documentation-llm/v1/checkpoints/000002/weights.bin";

    private static string CreatePublishedManifestRelativePath(string domainSlug)
        => "training-artifacts/models/" +
           $"{domainSlug}/ilearning-unit/code-documentation-llm/v1/checkpoints/000002/manifest.json";

    private sealed class CapturingTransport : IModelServerJsonTransport
    {
        private readonly Func<object, OperationResult<
            ModelServerJsonPostResponse<
                CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>>>
            _respond;

        public CapturingTransport(
            Func<object, OperationResult<
                ModelServerJsonPostResponse<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>>>
                respond)
        {
            _respond = respond;
        }

        public int Calls { get; private set; }
        public Uri? BaseUri { get; private set; }
        public string? RelativePath { get; private set; }
        public TimeSpan Timeout { get; private set; }
        public object? Payload { get; private set; }

        public Task<OperationResult<ModelServerJsonPostResponse<TResponse>>>
            PostJsonAsync<TRequest, TResponse>(
                Uri baseUri,
                string relativePath,
                TRequest payload,
                TimeSpan timeout,
                CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class
        {
            Calls++;
            BaseUri = baseUri;
            RelativePath = relativePath;
            Timeout = timeout;
            Payload = payload;

            var response = _respond(payload);
            if (!response.IsSuccess || response.Value is null)
            {
                return Task.FromResult(OperationResult<
                    ModelServerJsonPostResponse<TResponse>>.Fail(
                    response.ErrorMessage,
                    response.ErrorCode));
            }

            return Task.FromResult(OperationResult<
                ModelServerJsonPostResponse<TResponse>>.Ok(
                new ModelServerJsonPostResponse<TResponse>(
                    response.Value.StatusCode,
                    response.Value.IsSuccessStatusCode,
                    response.Value.Body as TResponse,
                    response.Value.RawBody)));
        }
    }
}

public sealed class HttpClientModelServerJsonTransportTests
{
    [Fact]
    public async Task PostJsonAsync_ValidContractPostsToLoopbackRouteAndDeserializes()
    {
        Uri? requestUri = null;
        var transport = new HttpClientModelServerJsonTransport(
            new HttpClient(new FakeHttpMessageHandler(
                async (request, cancellationToken) =>
                {
                    requestUri = request.RequestUri;
                    var requestBody = await request.Content!
                        .ReadAsStringAsync(cancellationToken);
                    Assert.Contains("fixture", requestBody, StringComparison.Ordinal);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            """{"ok":true,"message":"accepted"}""",
                            Encoding.UTF8,
                            "application/json")
                    };
                })));

        var result = await transport.PostJsonAsync<TestTransportRequest,
            TestTransportResponse>(
            new Uri("http://localhost:6060/"),
            "/internal/fixture",
            new TestTransportRequest("fixture"),
            TimeSpan.FromSeconds(1));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            new Uri("http://localhost:6060/internal/fixture"),
            requestUri);
        Assert.True(result.Value!.Body!.Ok);
        Assert.Equal("accepted", result.Value.Body.Message);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://127.0.0.1:6060/path")]
    [InlineData("http://user@localhost:6060")]
    [InlineData("http://localhost:6060?x=1")]
    [InlineData("http://localhost:6060/#fragment")]
    public async Task PostJsonAsync_RejectsUnsafeBaseUriBeforeHttp(
        string rawUri)
    {
        var calls = 0;
        var transport = new HttpClientModelServerJsonTransport(
            new HttpClient(new FakeHttpMessageHandler((_, _) =>
            {
                calls++;
                throw new InvalidOperationException(
                    "unsafe base URI must not reach HTTP");
            })));

        var result = await transport.PostJsonAsync<TestTransportRequest,
            TestTransportResponse>(
            new Uri(rawUri),
            "/internal/fixture",
            new TestTransportRequest("fixture"),
            TimeSpan.FromSeconds(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            HttpClientModelServerJsonTransport.InvalidRequestErrorCode,
            result.ErrorCode);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task PostJsonAsync_ContentLengthOverLimitFailsBeforeDeserialization()
    {
        var transport = new HttpClientModelServerJsonTransport(
            new HttpClient(new FakeHttpMessageHandler((_, _) =>
            {
                var content = new StringContent(
                    new string(
                        'x',
                        HttpClientModelServerJsonTransport
                            .MaximumResponseByteCount + 1),
                    Encoding.UTF8,
                    "application/json");
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = content
                });
            })));

        var result = await transport.PostJsonAsync<TestTransportRequest,
            TestTransportResponse>(
            new Uri("http://127.0.0.1:6060"),
            "/internal/fixture",
            new TestTransportRequest("fixture"),
            TimeSpan.FromSeconds(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            HttpClientModelServerJsonTransport.ResponseTooLargeErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task PostJsonAsync_ContentLengthOverLimitDoesNotReadResponseBody()
    {
        var content = new ThrowingLengthContent(
            HttpClientModelServerJsonTransport.MaximumResponseByteCount + 1);
        var transport = new HttpClientModelServerJsonTransport(
            new HttpClient(new FakeHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = content
                }))));

        var result = await transport.PostJsonAsync<TestTransportRequest,
            TestTransportResponse>(
            new Uri("http://127.0.0.1:6060"),
            "/internal/fixture",
            new TestTransportRequest("fixture"),
            TimeSpan.FromSeconds(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            HttpClientModelServerJsonTransport.ResponseTooLargeErrorCode,
            result.ErrorCode);
        Assert.Equal(0, content.ReadAttemptCount);
    }

    [Fact]
    public async Task PostJsonAsync_StreamOverLimitFailsBeforeDeserialization()
    {
        var transport = new HttpClientModelServerJsonTransport(
            new HttpClient(new FakeHttpMessageHandler((_, _) =>
            {
                var bytes = new byte[
                    HttpClientModelServerJsonTransport
                        .MaximumResponseByteCount + 1];
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new MemoryStream(bytes))
                });
            })));

        var result = await transport.PostJsonAsync<TestTransportRequest,
            TestTransportResponse>(
            new Uri("http://127.0.0.1:6060"),
            "/internal/fixture",
            new TestTransportRequest("fixture"),
            TimeSpan.FromSeconds(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            HttpClientModelServerJsonTransport.ResponseTooLargeErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task PostJsonAsync_TimeoutReturnsExplicitTimeoutCode()
    {
        var transport = new HttpClientModelServerJsonTransport(
            new HttpClient(new FakeHttpMessageHandler(
                async (_, cancellationToken) =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                })));

        var result = await transport.PostJsonAsync<TestTransportRequest,
            TestTransportResponse>(
            new Uri("http://127.0.0.1:6060"),
            "/internal/fixture",
            new TestTransportRequest("fixture"),
            TimeSpan.FromMilliseconds(10));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            HttpClientModelServerJsonTransport.TimeoutErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task PostJsonAsync_UnavailableReturnsExplicitUnavailableCode()
    {
        var transport = new HttpClientModelServerJsonTransport(
            new HttpClient(new FakeHttpMessageHandler((_, _) =>
                throw new HttpRequestException("connection refused"))));

        var result = await transport.PostJsonAsync<TestTransportRequest,
            TestTransportResponse>(
            new Uri("http://127.0.0.1:6060"),
            "/internal/fixture",
            new TestTransportRequest("fixture"),
            TimeSpan.FromSeconds(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            HttpClientModelServerJsonTransport.UnavailableErrorCode,
            result.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(86_401)]
    public async Task PostJsonAsync_RejectsTimeoutOutsideBoundBeforeHttp(
        int timeoutSeconds)
    {
        var calls = 0;
        var transport = new HttpClientModelServerJsonTransport(
            new HttpClient(new FakeHttpMessageHandler((_, _) =>
            {
                calls++;
                throw new InvalidOperationException(
                    "invalid timeout must not reach HTTP");
            })));

        var result = await transport.PostJsonAsync<TestTransportRequest,
            TestTransportResponse>(
            new Uri("http://127.0.0.1:6060"),
            "/internal/fixture",
            new TestTransportRequest("fixture"),
            TimeSpan.FromSeconds(timeoutSeconds));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            HttpClientModelServerJsonTransport.InvalidRequestErrorCode,
            result.ErrorCode);
        Assert.Equal(0, calls);
    }

    private sealed record TestTransportRequest(string Name);

    private sealed record TestTransportResponse(bool Ok, string Message);

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken,
            Task<HttpResponseMessage>> _sendAsync;

        public FakeHttpMessageHandler(
            Func<HttpRequestMessage, CancellationToken,
                Task<HttpResponseMessage>> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => _sendAsync(request, cancellationToken);
    }

    private sealed class ThrowingLengthContent : HttpContent
    {
        public ThrowingLengthContent(long contentLength)
        {
            Headers.ContentLength = contentLength;
        }

        public int ReadAttemptCount { get; private set; }

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context)
        {
            ReadAttemptCount++;
            throw new InvalidOperationException(
                "The response body must not be buffered before the Content-Length gate.");
        }

        protected override bool TryComputeLength(out long length)
        {
            length = Headers.ContentLength ?? 0;
            return true;
        }
    }
}
