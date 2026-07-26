// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerImprovementLoopCompositionTests
{
    [Fact]
    public async Task TryRunImprovementLoopAsync_CompositionRegisteredRoute_UsesBrowserBackedLocalComposition()
    {
        const string privateMarker = "PRIVATE_PASS133_COMPOSITION_SECRET";
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
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
                MaxImprovementCandidates: 2,
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
            Sha256: $"sha256:{id}");

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed class RecordingBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        public List<DesignConversionBrowserCaptureRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var render = await new DeterministicLocalDesignConversionPngRenderProvider(Path.Combine(
                    Path.GetTempPath(),
                    "llm_mvp_core",
                    "design-conversion-pass133-recording-browser-adapter",
                    Guid.NewGuid().ToString("N")))
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
}
