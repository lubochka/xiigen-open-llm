// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;
using System.Text.Json;

public sealed class DesignConversionSourceTreeContentAuthorityTests
{
    private const string CanonicalSha =
        "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    public static TheoryData<string, string> ExternalReferenceOnlyCases => new()
    {
        { "design", "blank" },
        { "design", "ref" },
        { "design", "sha" },
        { "original", "blank" },
        { "original", "ref" },
        { "original", "sha" }
    };

    public static TheoryData<string, string> DomainReferenceOnlyCases => new()
    {
        { "design", "blank" },
        { "design", "payload-id" },
        { "design", "sha-field" },
        { "design", "sha-token" },
        { "design", "sha-token-prefixed" },
        { "original", "blank" },
        { "original", "payload-id" },
        { "original", "sha-field" },
        { "original", "sha-token" },
        { "original", "sha-token-prefixed" }
    };

    [Theory]
    [MemberData(nameof(ExternalReferenceOnlyCases))]
    public async Task Api_HashOrRefOnlyTreeInputsFailBeforeRuntime(string tree, string surrogate)
    {
        var render = new EffectProbeRenderProvider();
        var comparison = new EffectProbeComparisonProvider();
        var service = new DesignConversionInternalService(render, comparison);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.Parse("2026-08-15T00:00:00Z"));
        var request = WithExternalSurrogate(CreateApiRequest("w083-api"), tree, surrogate);

        var result = await api.SubmitAndEvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ExpectedExternalCode(tree), result.ErrorCode);
        Assert.Equal(0, render.InvocationCount);
        Assert.Equal(0, comparison.InvocationCount);
    }

    [Theory]
    [MemberData(nameof(ExternalReferenceOnlyCases))]
    public async Task LocalCaller_HashOrRefOnlyTreeInputsFailBeforeRegistry(string tree, string surrogate)
    {
        using var registry = new DesignConversionModelServerEndpointRegistry();
        var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);
        var request = WithExternalSurrogate(CreateApiRequest("w083-local"), tree, surrogate);

        var result = await adapter.TrySubmitAsync(ToLocalPackage(request));

        Assert.Equal(ExpectedExternalCode(tree), result.ParsedErrorCode);
        Assert.False(result.RegistryInvokeSucceeded);
        Assert.False(result.RuntimeInvoked);
    }

    [Theory]
    [MemberData(nameof(ExternalReferenceOnlyCases))]
    public async Task ImprovementLoop_HashOrRefOnlyTreeInputsFailBeforeCoordinator(string tree, string surrogate)
    {
        var evaluator = new EffectProbeEvaluator();
        var candidateStrategy = new EffectProbeCandidateStrategy();
        var coordinator = new DesignConversionImprovementLoopCoordinator(evaluator, candidateStrategy);
        using var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopForLiveHostTesting(
            new DesignConversionModelServerImprovementLoopHandler(coordinator));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var request = WithExternalSurrogate(CreateApiRequest("w083-loop"), tree, surrogate);
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "w083-loop",
            InitialPackage: ToLocalPackage(request),
            MaxAttempts: 1);
        var httpRequest = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(
                loopRequest,
                DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var response = await registered.Value!.Handler(httpRequest, CancellationToken.None);

        Assert.Equal(400, response.StatusCode);
        using var document = JsonDocument.Parse(response.BodyJson);
        Assert.Equal(ExpectedExternalCode(tree), document.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(
            DesignConversionModelServerHttpHandler.LocalHttpSeam,
            document.RootElement.GetProperty("httpSeam").GetString());
        Assert.False(document.RootElement
            .GetProperty("boundaryMetadata")
            .TryGetProperty("adapter_error_source", out _));
        Assert.Equal(0, evaluator.InvocationCount);
        Assert.Equal(0, candidateStrategy.InvocationCount);
        foreach (var privateValue in new[] { request.DesignTree, request.OriginalTree }
                     .SelectMany(treePayload => new[]
                     {
                         treePayload?.Payload,
                         treePayload?.RefId,
                         treePayload?.Sha256
                     })
                     .Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            Assert.DoesNotContain(privateValue!, response.BodyJson, StringComparison.Ordinal);
        }
    }

    [Theory]
    [MemberData(nameof(DomainReferenceOnlyCases))]
    public void ThinClientRuntime_HashOrRefOnlyTreeInputsFailBeforeJobPersistence(
        string tree,
        string surrogate)
    {
        var evaluator = new EffectProbeEvaluator();
        var runtime = new DesignConversionThinClientJobRuntime(
            evaluator,
            () => DateTimeOffset.Parse("2026-08-15T00:00:00Z"));
        var request = WithDomainSurrogate(CreateSubmittedRequest("w083-thin"), tree, surrogate);

        var result = runtime.Submit(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionInternalService.RawPayloadRequiredCode, result.ErrorCode);
        Assert.Equal(0, evaluator.InvocationCount);
        var status = runtime.GetStatus("thin-job-w083-thin");
        Assert.False(status.IsSuccess);
        Assert.Equal("JOB_NOT_FOUND", status.ErrorCode);
    }

    [Theory]
    [MemberData(nameof(DomainReferenceOnlyCases))]
    public async Task InternalService_HashOrRefOnlyTreeInputsFailBeforeProviders(
        string tree,
        string surrogate)
    {
        var render = new EffectProbeRenderProvider();
        var comparison = new EffectProbeComparisonProvider();
        var service = new DesignConversionInternalService(render, comparison);
        var request = WithDomainSurrogate(CreateInternalRequest("w083-internal"), tree, surrogate);

        var result = await service.EvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionInternalService.RawPayloadRequiredCode, result.ErrorCode);
        Assert.Equal(0, render.InvocationCount);
        Assert.Equal(0, comparison.InvocationCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InlinePayloadWithSupplementalRefAndSha_RemainsAcceptedAcrossAllIngresses(
        bool includeSupplementalIdentities)
    {
        var identityMode = includeSupplementalIdentities ? "supplemental" : "inline-only";

        var apiPipeline = new PositiveEvaluationPipeline();
        using var apiState = new TemporaryPostEvaluationStateStore("api-" + identityMode);
        var api = new DesignConversionModelServerApi(
            apiPipeline.Evaluator,
            () => DateTimeOffset.Parse("2026-08-15T01:00:00Z"),
            postEvaluationStateStore: apiState.Store);
        var apiResult = await api.SubmitAndEvaluateAsync(
            CreatePositiveApiRequest($"w083-positive-api-{identityMode}", includeSupplementalIdentities));
        Assert.True(apiResult.IsSuccess, apiResult.ErrorMessage);
        AssertPipelineCounts(apiPipeline, evaluator: 1, render: 2, comparison: 1);

        var localPipeline = new PositiveEvaluationPipeline();
        using var localState = new TemporaryPostEvaluationStateStore("local-" + identityMode);
        var localApi = new DesignConversionModelServerApi(
            localPipeline.Evaluator,
            () => DateTimeOffset.Parse("2026-08-15T02:00:00Z"),
            postEvaluationStateStore: localState.Store);
        using var localRegistry = new DesignConversionModelServerEndpointRegistry();
        var localRegistered = localRegistry.RegisterDesignConversionEvaluate(
            new DesignConversionModelServerHttpHandler(localApi));
        Assert.True(localRegistered.IsSuccess, localRegistered.ErrorMessage);
        var localAdapter = new DesignConversionModelServerLocalCallerAdapter(localRegistry);
        var localResult = await localAdapter.TrySubmitAsync(ToLocalPackage(
            CreatePositiveApiRequest($"w083-positive-local-{identityMode}", includeSupplementalIdentities)));
        Assert.Equal(200, localResult.StatusCode);
        Assert.True(localResult.RegistryInvokeSucceeded);
        Assert.True(localResult.RuntimeInvoked);
        Assert.Null(localResult.ParsedErrorCode);
        AssertPipelineCounts(localPipeline, evaluator: 1, render: 2, comparison: 1);

        var loopPipeline = new PositiveEvaluationPipeline();
        var candidateStrategy = new EffectProbeCandidateStrategy();
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            loopPipeline.Evaluator,
            candidateStrategy);
        using var loopRegistry = new DesignConversionModelServerEndpointRegistry();
        var loopRegistered = loopRegistry.RegisterDesignConversionImprovementLoopForLiveHostTesting(
            new DesignConversionModelServerImprovementLoopHandler(coordinator));
        Assert.True(loopRegistered.IsSuccess, loopRegistered.ErrorMessage);
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: $"w083-positive-loop-{identityMode}",
            InitialPackage: ToLocalPackage(CreatePositiveApiRequest(
                $"w083-positive-loop-request-{identityMode}",
                includeSupplementalIdentities)),
            MaxAttempts: 1);
        var loopResponse = await loopRegistered.Value!.Handler(
            CreateImprovementLoopHttpRequest(loopRequest),
            CancellationToken.None);
        Assert.Equal(200, loopResponse.StatusCode);
        AssertPipelineCounts(loopPipeline, evaluator: 1, render: 2, comparison: 1);
        Assert.Equal(0, candidateStrategy.InvocationCount);

        var thinPipeline = new PositiveEvaluationPipeline();
        var runtime = new DesignConversionThinClientJobRuntime(
            thinPipeline.Evaluator,
            () => DateTimeOffset.Parse("2026-08-15T03:00:00Z"));
        var submittedRequest = CreatePositiveSubmittedRequest(
            $"w083-positive-thin-{identityMode}",
            includeSupplementalIdentities);
        var submitted = runtime.Submit(submittedRequest);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
        AssertPipelineCounts(thinPipeline, evaluator: 0, render: 0, comparison: 0);
        var run = await runtime.RunAsync(submitted.Value!.JobId);
        Assert.True(run.IsSuccess, run.ErrorMessage);
        AssertPipelineCounts(thinPipeline, evaluator: 1, render: 1, comparison: 1);

        var internalPipeline = new PositiveEvaluationPipeline();
        var internalRequest = CreateInternalRequest(
            $"w083-positive-internal-{identityMode}",
            includeSupplementalIdentities);
        var internalResult = await internalPipeline.Service.EvaluateAsync(internalRequest);
        Assert.True(internalResult.IsSuccess, internalResult.ErrorMessage);
        AssertPipelineCounts(internalPipeline, evaluator: 0, render: 1, comparison: 1);
    }

    [Fact]
    public async Task InvalidTreeContentErrors_AreStableAndDoNotLeakPayloadRefOrSha()
    {
        const string privateRef = "PRIVATE_W083_TREE_REF";
        const string privateSha = "sha256:PRIVATE_W083_TREE_SHA";
        var apiRequest = CreateApiRequest("w083-no-leak") with
        {
            DesignTree = new DesignConversionModelServerPayloadRef(
                RefId: privateRef,
                Kind: "figma-json",
                Sha256: privateSha,
                Payload: null,
                Format: "figma-json")
        };
        var external = DesignConversionModelServerPayloadSizeGuard.ValidateApiRequest(
            apiRequest,
            "W083 no-leak external");

        Assert.False(external.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_SOURCE_TREE_REQUIRED", external.ErrorCode);
        Assert.DoesNotContain(privateRef, external.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateSha, external.ErrorMessage, StringComparison.Ordinal);

        var render = new EffectProbeRenderProvider();
        var comparison = new EffectProbeComparisonProvider();
        var service = new DesignConversionInternalService(render, comparison);
        var request = CreateInternalRequest("w083-no-leak") with
        {
            DesignTreePayload = new DesignTreePayload(
                privateRef,
                "figma-json",
                privateSha,
                privateSha,
                new Dictionary<string, string>())
        };
        var domain = await service.EvaluateAsync(request);

        Assert.False(domain.IsSuccess);
        Assert.Equal(DesignConversionInternalService.RawPayloadRequiredCode, domain.ErrorCode);
        Assert.DoesNotContain(privateRef, domain.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateSha, domain.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(0, render.InvocationCount);
        Assert.Equal(0, comparison.InvocationCount);
    }

    [Fact]
    public void CallerContract_RequiresInlineTreePayloadAndDocumentsSupplementalIdentityOnly()
    {
        var descriptor = DesignConversionModelServerCallerContract.Describe();
        var designTree = Assert.Single(
            descriptor.RequestFields,
            field => field.JsonName == "designTree");
        var originalTree = Assert.Single(
            descriptor.RequestFields,
            field => field.JsonName == "originalTree");
        var designFailure = Assert.Single(
            descriptor.ValidationFailures,
            failure => failure.ErrorCode == "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED");
        var originalFailure = Assert.Single(
            descriptor.ValidationFailures,
            failure => failure.ErrorCode == "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED");
        var loopFailures = DesignConversionModelServerCallerContract
            .ImprovementLoopValidationFailures();

        Assert.Equal(
            "Generated design tree with required inline payload; ref and sha256 are optional supplemental identities.",
            designTree.Requirement);
        Assert.Equal(
            "Original source tree with required inline payload; ref and sha256 are optional supplemental identities.",
            originalTree.Requirement);
        Assert.Equal("Missing required inline designTree payload.", designFailure.Trigger);
        Assert.Equal("Missing required inline originalTree payload.", originalFailure.Trigger);
        Assert.Contains(loopFailures, failure =>
            failure.ErrorCode == "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED" &&
            failure.Trigger == "Missing required inline initialPackage.designTree payload.");
        Assert.Contains(loopFailures, failure =>
            failure.ErrorCode == "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED" &&
            failure.Trigger == "Missing required inline initialPackage.originalTree payload.");
    }

    private static string ExpectedExternalCode(string tree)
        => tree == "design"
            ? "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED"
            : "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED";

    private static DesignConversionModelServerApiRequest CreateApiRequest(string requestId)
        => DesignConversionModelServerCallerContract.CreateFigmaExampleRequest(requestId);

    private static DesignConversionModelServerApiRequest CreatePositiveApiRequest(
        string requestId,
        bool includeSupplementalIdentities)
    {
        var request = CreateApiRequest(requestId);
        return includeSupplementalIdentities
            ? request
            : WithoutExternalSupplementalIdentities(request);
    }

    private static DesignConversionModelServerApiRequest WithExternalSurrogate(
        DesignConversionModelServerApiRequest request,
        string tree,
        string surrogate)
    {
        var invalid = surrogate switch
        {
            "blank" => new DesignConversionModelServerPayloadRef("", "figma-json", Payload: " "),
            "ref" => new DesignConversionModelServerPayloadRef(
                "PRIVATE_W083_REF_ONLY",
                "figma-json",
                Payload: null),
            "sha" => new DesignConversionModelServerPayloadRef(
                "",
                "figma-json",
                Sha256: "sha256:" + CanonicalSha,
                Payload: null),
            _ => throw new ArgumentOutOfRangeException(nameof(surrogate), surrogate, null)
        };
        return tree == "design"
            ? request with { DesignTree = invalid }
            : request with { OriginalTree = invalid };
    }

    private static DesignConversionModelServerLocalCallerPackage ToLocalPackage(
        DesignConversionModelServerApiRequest request)
        => new(
            request.RequestId,
            request.SourceKind.ToString(),
            request.DesignTree,
            request.OriginalTree,
            request.Html,
            request.Css,
            request.Screenshots,
            request.DesignRenders,
            request.Budget,
            request.Metadata);

    private static DesignConversionModelServerHttpRequest CreateImprovementLoopHttpRequest(
        DesignConversionModelServerImprovementLoopRequest request)
        => new(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(
                request,
                DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

    private static DesignConversionModelServerApiRequest WithoutExternalSupplementalIdentities(
        DesignConversionModelServerApiRequest request)
        => request with
        {
            DesignTree = request.DesignTree with { RefId = "", Sha256 = null },
            OriginalTree = request.OriginalTree with { RefId = "", Sha256 = null }
        };

    private static SubmittedDesignConversionRequest CreateSubmittedRequest(string requestId)
        => new(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main>W083</main>",
            Css: "main{display:block;}",
            DesignTreePayload: CreateTree("design-tree", "{\"type\":\"FRAME\"}"),
            OriginalTreePayload: CreateTree("original-tree", "{\"type\":\"FRAME\",\"original\":true}"),
            Screenshots: [CreateArtifact("w083-screenshot", "screenshot")],
            DesignRenders: [CreateArtifact("w083-render", "design-render")],
            Budget: new DesignConversionBudgetMetadata(
                "w083-user",
                "internal",
                1,
                1,
                0.1m,
                new Dictionary<string, string>()),
            Provenance: new Dictionary<string, string>());

    private static SubmittedDesignConversionRequest CreatePositiveSubmittedRequest(
        string requestId,
        bool includeSupplementalIdentities)
    {
        var request = CreateSubmittedRequest(requestId);
        return includeSupplementalIdentities
            ? request
            : WithoutDomainSupplementalIdentities(request);
    }

    private static SubmittedDesignConversionRequest WithoutDomainSupplementalIdentities(
        SubmittedDesignConversionRequest request)
        => request with
        {
            DesignTreePayload = request.DesignTreePayload with { PayloadId = "", Sha256 = "" },
            OriginalTreePayload = request.OriginalTreePayload with { PayloadId = "", Sha256 = "" }
        };

    private static DesignConversionInternalEvaluateRequest CreateInternalRequest(string requestId)
        => CreateInternalRequest(requestId, includeSupplementalIdentities: true);

    private static DesignConversionInternalEvaluateRequest CreateInternalRequest(
        string requestId,
        bool includeSupplementalIdentities)
    {
        var submitted = CreatePositiveSubmittedRequest(requestId, includeSupplementalIdentities);
        return new DesignConversionInternalEvaluateRequest(
            JobId: "job-" + requestId,
            RequestId: submitted.RequestId,
            SourceKind: submitted.SourceKind,
            Html: submitted.Html,
            Css: submitted.Css,
            DesignTreePayload: submitted.DesignTreePayload,
            OriginalTreePayload: submitted.OriginalTreePayload,
            Screenshots: submitted.Screenshots,
            DesignRenders: submitted.DesignRenders,
            Budget: submitted.Budget,
            RenderResolutions: [new RenderQaResolution("desktop", 1440, 900, 1.0)],
            AiTeacherEvidenceRefs: [],
            DeterministicEvidenceRefs: new Dictionary<string, string>(),
            Provenance: new Dictionary<string, string>());
    }

    private static void AssertPipelineCounts(
        PositiveEvaluationPipeline pipeline,
        int evaluator,
        int render,
        int comparison)
    {
        Assert.Equal(evaluator, pipeline.Evaluator.InvocationCount);
        Assert.Equal(render, pipeline.RenderProvider.InvocationCount);
        Assert.Equal(comparison, pipeline.ComparisonProvider.InvocationCount);
    }

    private static SubmittedDesignConversionRequest WithDomainSurrogate(
        SubmittedDesignConversionRequest request,
        string tree,
        string surrogate)
    {
        var current = tree == "design"
            ? request.DesignTreePayload
            : request.OriginalTreePayload;
        var content = surrogate switch
        {
            "blank" => " ",
            "payload-id" => current.PayloadId,
            "sha-field" => current.Sha256,
            "sha-token" => CanonicalSha.ToUpperInvariant(),
            "sha-token-prefixed" => "SHA256:" + new string('A', 64),
            _ => throw new ArgumentOutOfRangeException(nameof(surrogate), surrogate, null)
        };
        var invalid = current with { Content = content };
        return tree == "design"
            ? request with { DesignTreePayload = invalid }
            : request with { OriginalTreePayload = invalid };
    }

    private static DesignConversionInternalEvaluateRequest WithDomainSurrogate(
        DesignConversionInternalEvaluateRequest request,
        string tree,
        string surrogate)
    {
        var submitted = new SubmittedDesignConversionRequest(
            request.RequestId,
            request.SourceKind,
            request.Html,
            request.Css,
            request.DesignTreePayload,
            request.OriginalTreePayload,
            request.Screenshots,
            request.DesignRenders,
            request.Budget,
            request.Provenance);
        var invalid = WithDomainSurrogate(submitted, tree, surrogate);
        return request with
        {
            DesignTreePayload = invalid.DesignTreePayload,
            OriginalTreePayload = invalid.OriginalTreePayload
        };
    }

    private static DesignTreePayload CreateTree(string id, string content)
        => new(
            PayloadId: id,
            Format: "figma-json",
            Content: content,
            Sha256: "sha256:" + CanonicalSha,
            Metadata: new Dictionary<string, string>());

    private static DesignRenderArtifact CreateArtifact(string id, string kind)
        => new(
            ArtifactId: id,
            Kind: kind,
            Width: 1440,
            Height: 900,
            Uri: "artifact://" + id,
            Sha256: "sha256:" + CanonicalSha,
            Metadata: new Dictionary<string, string>());

    private sealed class EffectProbeEvaluator : IDesignConversionThinClientJobEvaluator
    {
        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            throw new InvalidOperationException("W083 evaluator effect must not run.");
        }
    }

    private sealed class EffectProbeCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            throw new InvalidOperationException("W083 candidate-strategy effect must not run.");
        }
    }

    private sealed class PositiveEvaluationPipeline
    {
        public PositiveEvaluationPipeline()
        {
            RenderProvider = new CountingRenderProvider();
            ComparisonProvider = new CountingComparisonProvider();
            Service = new DesignConversionInternalService(RenderProvider, ComparisonProvider);
            Evaluator = new CountingEvaluator(
                new DesignConversionInternalServiceThinClientJobEvaluator(Service));
        }

        public CountingRenderProvider RenderProvider { get; }

        public CountingComparisonProvider ComparisonProvider { get; }

        public DesignConversionInternalService Service { get; }

        public CountingEvaluator Evaluator { get; }
    }

    private sealed class TemporaryPostEvaluationStateStore : IDisposable
    {
        private readonly string _root;

        public TemporaryPostEvaluationStateStore(string suffix)
        {
            _root = Path.Combine(
                Path.GetTempPath(),
                "w083-source-tree-content-authority",
                suffix + "-" + Guid.NewGuid().ToString("N"));
            Store = new JsonDesignConversionPostEvaluationStateStore(_root);
        }

        public JsonDesignConversionPostEvaluationStateStore Store { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_root))
                    Directory.Delete(_root, recursive: true);
            }
            catch (IOException)
            {
                // Best-effort cleanup must not hide the authority assertion result.
            }
            catch (UnauthorizedAccessException)
            {
                // Best-effort cleanup must not hide the authority assertion result.
            }
        }
    }

    private sealed class CountingEvaluator(IDesignConversionThinClientJobEvaluator inner)
        : IDesignConversionThinClientJobEvaluator
    {
        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            return inner.EvaluateAsync(request, cancellationToken);
        }
    }

    private sealed class CountingRenderProvider : IDesignConversionRenderProvider
    {
        private readonly DeterministicDesignConversionRenderProvider _inner = new();

        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            return _inner.RenderAsync(request, cancellationToken);
        }
    }

    private sealed class CountingComparisonProvider : IDesignConversionComparisonProvider
    {
        private readonly DeterministicDesignConversionComparisonProvider _inner = new();

        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            return _inner.CompareAsync(request, cancellationToken);
        }
    }

    private sealed class EffectProbeRenderProvider : IDesignConversionRenderProvider
    {
        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            throw new InvalidOperationException("W083 render effect must not run.");
        }
    }

    private sealed class EffectProbeComparisonProvider : IDesignConversionComparisonProvider
    {
        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            throw new InvalidOperationException("W083 comparison effect must not run.");
        }
    }
}
