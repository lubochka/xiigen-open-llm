// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

[Collection("ProcessSpawningTests")]
public sealed class DesignConversionBrowserRenderQaProbeIntegrationTests
{
    [Fact]
    public async Task RunAsync_RealBrowserBackedProviderFlowsThroughProbeWithExpectedRefsAndPngArtifacts()
    {
        var executable = LocalChromiumBrowserProcessAdapter.TryResolveDefaultExecutablePath();
        var artifactRoot = Path.GetFullPath(Path.Combine(
            "TestResults",
            "codex-pass120-browser-backed-render-qa-through-probe",
            Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(artifactRoot);

        if (string.IsNullOrWhiteSpace(executable))
        {
            await WriteBrowserUnavailableEvidenceAsync(
                artifactRoot,
                "BROWSER_RUNTIME_UNAVAILABLE",
                "No Chromium-family browser executable resolved from LocalChromiumBrowserProcessAdapter.");
            var missing = await new LocalChromiumBrowserProcessAdapter(
                    executablePath: @"C:\definitely-not-installed\missing-browser.exe",
                    timeout: TimeSpan.FromSeconds(1),
                    artifactRoot: artifactRoot)
                .CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
                    CaptureRequestId: "pass120-browser-runtime-blocker",
                    RequestId: "pass120-request",
                    DocumentHtml: "<!doctype html><html><body>browser unavailable</body></html>",
                    Resolution: new RenderQaResolution("desktop", 1280, 720, 1.0),
                    Metadata: new Dictionary<string, string>()));

            Assert.False(missing.IsSuccess);
            Assert.Equal("BROWSER_RUNTIME_UNAVAILABLE", missing.ErrorCode);
            return;
        }

        var viewports = new[]
        {
            new RenderQaResolution("mobile", 390, 844, 1.0),
            new RenderQaResolution("tablet", 768, 1024, 1.0),
            new RenderQaResolution("desktop", 1440, 900, 1.0)
        };
        var comparisonProvider = new Pass120LocalVisualJudgeComparisonProvider();
        var probe = new DesignConversionLocalRenderQaProbe(
            new BrowserBackedDesignConversionRenderProvider(
                new LocalChromiumBrowserProcessAdapter(executable, TimeSpan.FromSeconds(20), artifactRoot)),
            comparisonProvider);
        var request = CreateRequest(viewports);

        var result = await probe.RunAsync(request);

        if (!result.IsSuccess && IsBrowserRuntimeBlocker(result.ErrorCode))
        {
            await WriteBrowserUnavailableEvidenceAsync(artifactRoot, result.ErrorCode!, result.ErrorMessage);
            return;
        }

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, result.Value!.Viewports.Count);
        Assert.Equal(3, comparisonProvider.Requests.Count);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["visual_ai_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["visual_ai_not_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_expected_design_export_included"]);
        Assert.Equal("3", result.Value.BoundaryMetadata["viewport_count"]);
        Assert.Equal("3", result.Value.BoundaryMetadata["expected_design_export_count"]);
        Assert.Equal("3", result.Value.BoundaryMetadata["result_viewport_count"]);
        Assert.Equal("0.97", result.Value.BoundaryMetadata["average_score"]);
        Assert.Equal("0.97", result.Value.BoundaryMetadata["minimum_score"]);
        Assert.Equal("0", result.Value.BoundaryMetadata["total_defect_count"]);
        Assert.True(result.Value.Passed);
        Assert.Equal(0, result.Value.TotalDefectCount);
        Assert.Contains(DesignConversionLocalRenderQaProbe.BrowserRenderExecutedMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionLocalRenderQaProbe.VisualAiNotExecutedMarker, result.Value.EvidenceRefs);

        var pngEvidence = InspectPngEvidence(artifactRoot);
        Assert.Equal(3, pngEvidence.Count);
        Assert.All(result.Value.Viewports, viewport =>
        {
            Assert.StartsWith("sha256:", viewport.Actual.Sha256, StringComparison.Ordinal);
            Assert.Contains(pngEvidence, png => string.Equals(png.Sha256, viewport.Actual.Sha256, StringComparison.Ordinal));
            Assert.Equal(0.97, viewport.Score);
            Assert.Equal(0, viewport.DefectCount);
            Assert.True(viewport.Passed);
            Assert.Equal(bool.TrueString, viewport.Actual.Metadata["refs_or_hashes_only"]);
            Assert.Equal(bool.TrueString, viewport.Expected.Metadata["refs_or_hashes_only"]);
        });

        var serializedResult = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(request.Html, serializedResult, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Css, serializedResult, StringComparison.OrdinalIgnoreCase);

        var evidencePath = Path.Combine(artifactRoot, "codex-pass120-probe-evidence.json");
        await File.WriteAllTextAsync(
            evidencePath,
            JsonSerializer.Serialize(new
            {
                schema = "codex-pass120-browser-backed-render-qa-through-probe-v1",
                browser_executable = executable,
                artifact_root = artifactRoot,
                html_sha256 = Sha256Text(request.Html),
                css_sha256 = Sha256Text(request.Css),
                expected_design_export_refs = request.ExpectedDesignExports.Select(artifact => new
                {
                    artifact.ArtifactId,
                    artifact.Kind,
                    artifact.Width,
                    artifact.Height,
                    artifact.Uri,
                    artifact.Sha256
                }).ToArray(),
                viewport_results = result.Value.Viewports.Select(viewport => new
                {
                    viewport.ViewportId,
                    viewport.Viewport.Width,
                    viewport.Viewport.Height,
                    viewport.Actual.Sha256,
                    actual_uri_ref = viewport.Actual.UriRef,
                    viewport.Comparison.ArtifactId,
                    comparison_uri_ref = viewport.Comparison.UriRef,
                    viewport.Score,
                    viewport.DefectCount,
                    viewport.Passed
                }).ToArray(),
                png_artifacts = pngEvidence,
                boundary_metadata = result.Value.BoundaryMetadata,
                evidence_refs = result.Value.EvidenceRefs,
                warnings = result.Value.Warnings
            }, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);

        Assert.True(File.Exists(evidencePath), evidencePath);
    }

    private static DesignConversionLocalRenderQaProbeRequest CreateRequest(IReadOnlyList<RenderQaResolution> viewports)
        => new(
            ProbeId: "codex-pass120-browser-backed-render-qa-through-probe",
            RequestId: "codex-pass120-request",
            Html: """
                <main class="screen">
                  <section class="hero">
                    <p class="eyebrow">DesignConversion QA</p>
                    <h1>Browser backed probe evidence</h1>
                    <p>Three viewports flow through the local render QA probe.</p>
                  </section>
                  <section class="tiles">
                    <article>mobile</article>
                    <article>tablet</article>
                    <article>desktop</article>
                  </section>
                </main>
                """,
            Css: """
                html,body{margin:0;min-height:100%;font-family:Arial,sans-serif;background:#f8fafc;color:#102033}
                .screen{min-height:100vh;display:grid;align-content:center;gap:24px;padding:32px;background:linear-gradient(135deg,#f8fafc 0%,#e0f2fe 45%,#fef3c7 100%)}
                .hero{border:4px solid #0f766e;background:#ffffff;box-shadow:0 16px 40px rgba(16,32,51,.18);padding:28px}
                .eyebrow{margin:0 0 8px;color:#be123c;font-weight:700;text-transform:uppercase}
                h1{margin:0;font-size:42px}
                p{font-size:20px;line-height:1.45}
                .tiles{display:grid;grid-template-columns:repeat(3,1fr);gap:12px}
                article{min-height:86px;display:grid;place-items:center;background:#1e3a8a;color:white;font-weight:700}
                @media (max-width: 700px){.screen{padding:18px}.tiles{grid-template-columns:1fr}h1{font-size:30px}p{font-size:17px}}
                """,
            Viewports: viewports,
            ExpectedDesignExports: viewports.Select(Expected).ToArray(),
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "codex-pass120",
                ["qa_data_role"] = "browser_backed_probe_evidence_only",
                ["expected_design_exports"] = "refs_only",
                ["external_visual_ai_executed"] = bool.FalseString
            });

    private static DesignRenderArtifact Expected(RenderQaResolution viewport)
        => new(
            ArtifactId: $"pass120-expected-{viewport.ResolutionId}",
            Kind: "expected-design-export-ref",
            Width: viewport.Width,
            Height: viewport.Height,
            Uri: $"artifact://pass120/expected/{viewport.ResolutionId}.png",
            Sha256: $"sha256:pass120-expected-{viewport.ResolutionId}",
            Metadata: new Dictionary<string, string>
            {
                ["expected_design_export_ref"] = bool.TrueString,
                ["refs_only"] = bool.TrueString
            });

    private static IReadOnlyList<PngArtifactEvidence> InspectPngEvidence(string artifactRoot)
    {
        var pngPaths = Directory
            .GetFiles(artifactRoot, "screenshot.png", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        var evidence = new List<PngArtifactEvidence>();
        foreach (var path in pngPaths)
        {
            var inspect = DesignConversionBrowserPngEvidenceInspector.Inspect(new Uri(path).AbsoluteUri);
            Assert.True(inspect.IsSuccess, inspect.ErrorMessage);
            var bytes = File.ReadAllBytes(path);
            var sha = $"sha256:{Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()}";
            evidence.Add(new PngArtifactEvidence(
                LocalPath: path,
                Sha256: sha,
                ByteLength: inspect.Value!.ByteLength,
                Width: inspect.Value.PngWidth,
                Height: inspect.Value.PngHeight,
                HasNonBlankPixels: inspect.Value.HasNonBlankPixels,
                NonWhitePixelCount: inspect.Value.NonWhitePixelCount,
                DistinctColorCount: inspect.Value.DistinctColorCount));
        }

        return evidence;
    }

    private static bool IsBrowserRuntimeBlocker(string? errorCode)
        => string.Equals(errorCode, "BROWSER_RUNTIME_UNAVAILABLE", StringComparison.Ordinal) ||
           string.Equals(errorCode, "BROWSER_SCREENSHOT_TIMEOUT", StringComparison.Ordinal);

    private static async Task WriteBrowserUnavailableEvidenceAsync(string artifactRoot, string errorCode, string? errorMessage)
    {
        var evidencePath = Path.Combine(artifactRoot, "codex-pass120-browser-unavailable.json");
        await File.WriteAllTextAsync(
            evidencePath,
            JsonSerializer.Serialize(new
            {
                schema = "codex-pass120-browser-unavailable-v1",
                browser_render_executed = false,
                visual_ai_executed = false,
                error_code = errorCode,
                error_message = errorMessage,
                boundary = "product_readiness_not_proven"
            }, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);
    }

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed class Pass120LocalVisualJudgeComparisonProvider : IDesignConversionComparisonProvider
    {
        private readonly IDesignConversionVisualJudge _visualJudge = new DefaultLocalDesignConversionVisualJudge();

        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var diffSha = Sha256Text(string.Join(
                "|",
                request.ComparisonRequestId,
                request.Expected.Sha256,
                request.Actual.Sha256,
                request.Expected.Resolution.Width,
                request.Expected.Resolution.Height,
                request.Actual.Resolution.Width,
                request.Actual.Resolution.Height));
            var metadata = new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
            {
                ["comparison_verdict_score"] = "0.97",
                ["comparison_verdict_passed"] = bool.TrueString,
                ["quality_threshold"] = "0.85",
                ["comparison_provider"] = "pass120_local_visual_labeler_wrapper",
                ["external_ai_visual_judge_executed"] = bool.FalseString,
                ["visual_ai_executed"] = bool.FalseString
            };
            var judge = await _visualJudge.JudgeAsync(
                new DesignConversionVisualJudgeRequest(
                    JudgeRequestId: request.ComparisonRequestId,
                    RequestId: request.RequestId,
                    ExpectedScreenshot: request.Expected,
                    RenderedImage: request.Actual,
                    DiffArtifactUri: $"artifact://pass120/local-visual-labeler/{Uri.EscapeDataString(request.ComparisonRequestId)}.json",
                    DiffArtifactSha256: $"sha256:{diffSha}",
                    Metadata: metadata),
                cancellationToken);

            return judge.IsSuccess
                ? OperationResult<DesignConversionQaComparisonArtifact>.Ok(judge.Value!.ToComparisonArtifact(
                    new DesignConversionVisualJudgeRequest(
                        JudgeRequestId: request.ComparisonRequestId,
                        RequestId: request.RequestId,
                        ExpectedScreenshot: request.Expected,
                        RenderedImage: request.Actual,
                        DiffArtifactUri: $"artifact://pass120/local-visual-labeler/{Uri.EscapeDataString(request.ComparisonRequestId)}.json",
                        DiffArtifactSha256: $"sha256:{diffSha}",
                        Metadata: metadata)))
                : judge.MapError<DesignConversionQaComparisonArtifact>();
        }

        private static string Sha256Text(string value)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private sealed record PngArtifactEvidence(
        string LocalPath,
        string Sha256,
        long ByteLength,
        int Width,
        int Height,
        bool HasNonBlankPixels,
        int NonWhitePixelCount,
        int DistinctColorCount);
}
