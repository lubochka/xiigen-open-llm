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

[Collection("ProcessSpawningTests")]
public sealed class DesignConversionModelServerImprovementLoopBrowserIntegrationTests
{
    [Fact]
    public async Task TryRunImprovementLoopAsync_ExplicitBrowserCompositionExecutesRealBrowserWhenAvailableOrReportsBlocker()
    {
        const string privateMarker = "PRIVATE_PASS134_REAL_BROWSER_SECRET";
        var executable = LocalChromiumBrowserProcessAdapter.TryResolveDefaultExecutablePath();
        var artifactRoot = Path.GetFullPath(Path.Combine(
            "TestResults",
            "codex-pass134-real-browser-improvement-loop-composition",
            Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(artifactRoot);

        if (string.IsNullOrWhiteSpace(executable))
        {
            var missing = await new LocalChromiumBrowserProcessAdapter(
                    executablePath: @"C:\definitely-not-installed\missing-browser.exe",
                    timeout: TimeSpan.FromSeconds(1),
                    artifactRoot: artifactRoot)
                .CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
                    CaptureRequestId: "pass134-browser-runtime-blocker",
                    RequestId: "pass134-browser-runtime-blocker",
                    DocumentHtml: "<!doctype html><html><body>browser unavailable</body></html>",
                    Resolution: new RenderQaResolution("desktop", 800, 600, 1.0),
                    Metadata: new Dictionary<string, string>()));

            Assert.False(missing.IsSuccess);
            Assert.Equal("BROWSER_RUNTIME_UNAVAILABLE", missing.ErrorCode);
            await WriteEvidenceAsync(
                artifactRoot,
                verdict: "NEEDS_FIX",
                executable,
                response: null,
                pngEvidence: [],
                errorCode: missing.ErrorCode,
                errorMessage: missing.ErrorMessage);
            return;
        }

        var comparisonProvider = new Pass134ComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            new LocalChromiumBrowserProcessAdapter(executable, TimeSpan.FromSeconds(20), artifactRoot),
            comparisonProvider);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            new Pass134CandidateStrategy());
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        Assert.Equal(bool.TrueString, registered.Value!.BoundaryMetadata["browser_renderer_default_wired"]);
        Assert.Equal("explicit_local_composition_only", registered.Value.BoundaryMetadata["browser_renderer_default_scope"]);

        var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass134-real-browser-composition-loop",
            InitialPackage: CreatePackage("pass134-real-browser-request", privateMarker),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "codex-pass134",
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);
        if (!IsSuccessfulEnvelope(envelope))
        {
            Assert.Contains(envelope.ParsedErrorCode, BrowserRuntimeBlockerErrorCodes);
            await WriteEvidenceAsync(
                artifactRoot,
                verdict: "NEEDS_FIX",
                executable,
                envelope,
                pngEvidence: [],
                errorCode: envelope.ParsedErrorCode,
                errorMessage: envelope.ParsedErrorMessage);
            return;
        }

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(200, envelope.StatusCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.True(envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.Equal("pass134-real-browser-request", envelope.ParsedRequestId);
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
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["improvement_loop_registered_from_local_service_composition"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["browser_renderer_default_wired"]);
        Assert.Equal("explicit_local_composition_only", envelope.BoundaryMetadata["browser_renderer_default_scope"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal("2", envelope.BoundaryMetadata["browser_render_result_count"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_production_default_ready"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["provider_payload_included"]);

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
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("browser_render_executed").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("browser_render_not_executed").GetString());
        Assert.Equal("2", attemptMetadata.GetProperty("browser_render_result_count").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("product_ready").GetString());

        var pngEvidence = InspectPngEvidence(artifactRoot);
        Assert.Equal(2, pngEvidence.Count);
        Assert.All(pngEvidence, evidence =>
        {
            Assert.True(File.Exists(evidence.LocalPath), evidence.LocalPath);
            Assert.True(evidence.ByteLength > 1000, evidence.LocalPath);
            Assert.True(evidence.HasNonBlankPixels, evidence.LocalPath);
            Assert.True(evidence.NonWhitePixelCount > 0, evidence.LocalPath);
            Assert.True(evidence.DistinctColorCount > 1, evidence.LocalPath);
        });
        Assert.Contains(pngEvidence, evidence => evidence.Width == 800 && evidence.Height == 600);
        Assert.Contains(pngEvidence, evidence => evidence.Width == 390 && evidence.Height == 844);
        Assert.Equal(2, comparisonProvider.Requests.Count);

        await WriteEvidenceAsync(
            artifactRoot,
            verdict: "PASS",
            executable,
            envelope,
            pngEvidence,
            errorCode: null,
            errorMessage: null);
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
                Sha256: $"sha256:{Sha256Text(requestId + ":design-tree")}",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"generated\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{Sha256Text(requestId + ":original-tree")}",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"original\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            Html: """
                <main class="pass134-screen">
                  <section class="hero">
                    <p class="eyebrow">PASS134</p>
                    <h1>Real browser route proof</h1>
                    <p>Local ModelServer composition renders through Edge or Chromium.</p>
                  </section>
                </main>
                """,
            Css: """
                html,body{margin:0;min-height:100%;font-family:Arial,sans-serif;background:#f8fafc;color:#14213d}
                .pass134-screen{min-height:100vh;display:grid;place-items:center;padding:32px;background:linear-gradient(135deg,#f8fafc 0%,#cffafe 48%,#fef08a 100%)}
                .hero{max-width:720px;border:4px solid #0f766e;background:#ffffff;padding:32px;box-shadow:0 16px 42px rgba(20,33,61,.2)}
                .eyebrow{margin:0 0 10px;color:#be123c;font-weight:700;text-transform:uppercase}
                h1{margin:0;font-size:42px}
                p{font-size:20px;line-height:1.45}
                @media (max-width: 700px){.pass134-screen{padding:18px}.hero{padding:22px}h1{font-size:30px}p{font-size:17px}}
                """,
            Screenshots:
            [
                CreateArtifact($"{requestId}:source-desktop", "screenshot", 800, 600),
                CreateArtifact($"{requestId}:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                CreateArtifact($"{requestId}:design-desktop", "design-render", 800, 600),
                CreateArtifact($"{requestId}:design-mobile", "design-render", 390, 844)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "pass134-local-user-ref",
                Tier: "pass134-local-browser-loop",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90"
                }),
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "codex-pass134",
                ["raw_private_marker_present_only_in_request"] = bool.TrueString
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
            Uri: $"artifact://pass134/{id}.png",
            Sha256: $"sha256:{Sha256Text(id)}");

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
            evidence.Add(new PngArtifactEvidence(
                LocalPath: path,
                Sha256: $"sha256:{Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()}",
                ByteLength: inspect.Value!.ByteLength,
                Width: inspect.Value.PngWidth,
                Height: inspect.Value.PngHeight,
                HasNonBlankPixels: inspect.Value.HasNonBlankPixels,
                NonWhitePixelCount: inspect.Value.NonWhitePixelCount,
                DistinctColorCount: inspect.Value.DistinctColorCount));
        }

        return evidence;
    }

    private static async Task WriteEvidenceAsync(
        string artifactRoot,
        string verdict,
        string? executable,
        DesignConversionModelServerLocalCallerAdapterResponse? response,
        IReadOnlyList<PngArtifactEvidence> pngEvidence,
        string? errorCode,
        string? errorMessage)
    {
        var path = Path.Combine(artifactRoot, "codex-pass134-real-browser-improvement-loop-composition.json");
        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(new
            {
                schema = "codex-pass134-real-browser-improvement-loop-composition-v1",
                verdict,
                browser_executable_resolved = !string.IsNullOrWhiteSpace(executable),
                browser_executable = executable,
                artifact_root = artifactRoot,
                endpoint_path = DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                registry_invoke_succeeded = response?.RegistryInvokeSucceeded,
                runtime_invoked = response?.RuntimeInvoked,
                parsed_response_json_succeeded = response?.ParsedResponseJsonSucceeded,
                envelope_kind = response?.EnvelopeKind,
                status_code = response?.StatusCode,
                contract_status = response?.ParsedContractStatus,
                error_code = errorCode,
                error_message = errorMessage,
                boundary_metadata = response?.BoundaryMetadata,
                response_body_sha256 = response is null ? null : Sha256Text(response.ResponseBodyJson),
                png_artifacts = pngEvidence,
                raw_html_included = response?.RawHtmlIncluded,
                raw_css_included = response?.RawCssIncluded,
                raw_design_tree_included = response?.RawDesignTreeIncluded,
                response_contains_raw_private_data = response?.ResponseContainsRawPrivateData,
                raw_private_response_leakage_detected = response?.RawPrivateResponseLeakageDetected
            }, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);
    }

    private static bool IsSuccessfulEnvelope(DesignConversionModelServerLocalCallerAdapterResponse envelope)
        => envelope.StatusCode is >= 200 and <= 299 &&
           string.Equals(envelope.EnvelopeKind, "success", StringComparison.Ordinal) &&
           envelope.ParsedResponseJsonSucceeded &&
           string.IsNullOrWhiteSpace(envelope.ParsedErrorCode);

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static readonly string[] BrowserRuntimeBlockerErrorCodes =
    [
        "BROWSER_RUNTIME_UNAVAILABLE",
        "BROWSER_SCREENSHOT_TIMEOUT",
        "BROWSER_SCREENSHOT_FAILED"
    ];

    private sealed class Pass134ComparisonProvider(double score) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:pass134-verdict",
                Score: score,
                Defects: [],
                EvidenceRefs: [$"pass134-comparison:{request.ComparisonRequestId}"],
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{request.ComparisonRequestId}:pass134-artifact",
                    ComparisonRequestId: request.ComparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"artifact://pass134/diff/{Sha256Text(request.ComparisonRequestId)}.json",
                    DiffImageSha256: $"sha256:{Sha256Text(request.ComparisonRequestId + ":diff")}",
                    Metrics: new Dictionary<string, double>
                    {
                        ["visual_distance"] = 1.0 - score
                    },
                    EvidenceRefs: ["pass134-local-comparison-provider"])
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

    private sealed class Pass134CandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "PASS134 expects quality reached before a candidate is requested.",
                "PASS134_UNEXPECTED_CANDIDATE_REQUEST"));
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
