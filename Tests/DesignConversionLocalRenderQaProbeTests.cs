// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionLocalRenderQaProbeTests
{
    [Fact]
    public async Task RunAsync_MultiViewportRequestInvokesRenderAndCompareAndReturnsRefsOnlySummary()
    {
        var renderProvider = new RecordingRenderProvider(browserUsed: false);
        var comparisonProvider = new RecordingComparisonProvider(score: 0.94, passed: true);
        var probe = new DesignConversionLocalRenderQaProbe(renderProvider, comparisonProvider);
        var request = CreateRequest("pass118-local-render-qa");

        var result = await probe.RunAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, result.Value!.Viewports.Count);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["render_provider_invoked"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["comparison_provider_invoked"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["visual_ai_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_ai_not_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_css_included"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["viewport_count"]);
        Assert.Equal("0.94", result.Value.BoundaryMetadata["average_score"]);
        Assert.Equal(0, result.Value.TotalDefectCount);
        Assert.True(result.Value.Passed);
        Assert.Contains(DesignConversionLocalRenderQaProbe.RenderProviderCallThroughMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionLocalRenderQaProbe.ComparisonProviderCallThroughMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionLocalRenderQaProbe.BrowserRenderNotExecutedMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionLocalRenderQaProbe.VisualAiNotExecutedMarker, result.Value.EvidenceRefs);
        Assert.All(result.Value.Viewports, viewport =>
        {
            Assert.StartsWith("sha256:", viewport.Actual.Sha256, StringComparison.Ordinal);
            Assert.StartsWith("sha256:", viewport.Comparison.Sha256, StringComparison.Ordinal);
            Assert.Equal(bool.TrueString, viewport.Actual.Metadata["refs_or_hashes_only"]);
            Assert.True(viewport.Score >= 0.90);
        });

        var serialized = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(request.Html, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Css, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PRIVATE_PASS118_SECRET", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_BrowserProviderEvidenceMarksBrowserExecutedWithoutVisualAi()
    {
        var probe = new DesignConversionLocalRenderQaProbe(
            new RecordingRenderProvider(browserUsed: true),
            new RecordingComparisonProvider(score: 0.91, passed: true));

        var result = await probe.RunAsync(CreateRequest("pass118-browser-provider-boundary"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(bool.TrueString, result.Value!.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_ai_not_executed"]);
        Assert.DoesNotContain(DesignConversionLocalRenderQaProbe.BrowserRenderNotExecutedMarker, result.Value.EvidenceRefs);
    }

    [Fact]
    public async Task RunAsync_InvalidViewportFailsClearly()
    {
        var probe = CreateProbe();
        var request = CreateRequest("pass118-invalid-viewport") with
        {
            Viewports =
            [
                new RenderQaResolution("bad-mobile", 0, 740, 1.0)
            ]
        };

        var result = await probe.RunAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("LOCAL_RENDER_QA_VIEWPORT_INVALID", result.ErrorCode);
    }

    [Fact]
    public async Task RunAsync_InvalidExpectedDesignRefFailsClearly()
    {
        var probe = CreateProbe();
        var request = CreateRequest("pass118-invalid-expected-ref") with
        {
            ExpectedDesignExports =
            [
                new DesignRenderArtifact(
                    ArtifactId: "missing-uri",
                    Kind: "design-export",
                    Width: 390,
                    Height: 844,
                    Uri: "",
                    Sha256: "sha256:missing-uri",
                    Metadata: new Dictionary<string, string>())
            ]
        };

        var result = await probe.RunAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("LOCAL_RENDER_QA_EXPECTED_REF_INVALID", result.ErrorCode);
    }

    [Fact]
    public async Task RunAsync_MissingExpectedDesignRefForViewportFailsClearly()
    {
        var probe = CreateProbe();
        var request = CreateRequest("pass118-missing-expected-ref") with
        {
            ExpectedDesignExports =
            [
                new DesignRenderArtifact(
                    ArtifactId: "desktop-only",
                    Kind: "design-export",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://pass118/desktop.png",
                    Sha256: "sha256:desktop-only",
                    Metadata: new Dictionary<string, string>())
            ]
        };

        var result = await probe.RunAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("LOCAL_RENDER_QA_EXPECTED_REF_MISSING", result.ErrorCode);
        Assert.Contains("mobile", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static DesignConversionLocalRenderQaProbe CreateProbe()
        => new(
            new RecordingRenderProvider(browserUsed: false),
            new RecordingComparisonProvider(score: 0.94, passed: true));

    private static DesignConversionLocalRenderQaProbeRequest CreateRequest(string probeId)
        => new(
            ProbeId: probeId,
            RequestId: $"{probeId}-request",
            Html: "<main data-secret=\"PRIVATE_PASS118_SECRET\" class=\"screen\"><h1>Dashboard</h1></main>",
            Css: ".screen{display:grid;gap:16px;background:url('PRIVATE_PASS118_SECRET');}",
            Viewports:
            [
                new RenderQaResolution("mobile", 390, 844, 1.0),
                new RenderQaResolution("desktop", 1440, 900, 1.0)
            ],
            ExpectedDesignExports:
            [
                Expected("mobile-export", 390, 844),
                Expected("desktop-export", 1440, 900)
            ],
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "pass118",
                ["source"] = "local_contract_probe"
            });

    private static DesignRenderArtifact Expected(string id, int width, int height)
        => new(
            ArtifactId: id,
            Kind: "design-export",
            Width: width,
            Height: height,
            Uri: $"artifact://pass118/{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private sealed class RecordingRenderProvider(bool browserUsed) : IDesignConversionRenderProvider
    {
        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var sha = Sha256Text(string.Join(
                "|",
                request.RenderRequestId,
                request.Resolution.ResolutionId,
                request.Resolution.Width,
                request.Resolution.Height,
                request.Html,
                request.Css,
                browserUsed));
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass118/render/{sha}.png",
                Sha256: $"sha256:{sha}",
                Metadata: new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
                {
                    ["provider_kind"] = "pass118-recording-render-provider",
                    ["browser_or_playwright_used"] = browserUsed.ToString(),
                    ["external_ai_used"] = bool.FalseString
                });

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 4,
                Warnings: ["pass118-recording-render-provider"])));
        }

        private static string Sha256Text(string value)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private sealed class RecordingComparisonProvider(double score, bool passed) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var defect = new VisualDefect(
                DefectId: $"{request.ComparisonRequestId}:spacing",
                Category: "spacing",
                Severity: "minor",
                Description: "Spacing differs.",
                X: 10,
                Y: 12,
                Width: 40,
                Height: 20,
                EvidenceImageIds: [request.Expected.ImageId, request.Actual.ImageId]);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:verdict",
                Score: score,
                Defects: passed ? [] : [defect],
                EvidenceRefs: [$"pass118-verdict:{request.ComparisonRequestId}"],
                Passed: passed);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{request.ComparisonRequestId}:artifact",
                    ComparisonRequestId: request.ComparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"artifact://pass118/diff/{request.ComparisonRequestId}.png",
                    DiffImageSha256: $"sha256:{Sha256Text(request.ComparisonRequestId)}",
                    Metrics: new Dictionary<string, double>
                    {
                        ["visual_distance"] = 1.0 - score
                    },
                    EvidenceRefs: ["pass118-comparison-provider"])));
        }

        private static string Sha256Text(string value)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }
}
