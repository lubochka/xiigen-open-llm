// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DesignConversionDomain;

public sealed class DesignConversionQaProviderTests
{
    [Fact]
    public async Task RenderAsync_CreatesDeterministicEvidenceForMultipleResolutions()
    {
        var provider = new DeterministicDesignConversionRenderProvider();
        var desktop = new RenderQaResolution("desktop", 1440, 900, 1.0);
        var mobile = new RenderQaResolution("mobile", 390, 844, 3.0);

        var desktopResult = await provider.RenderAsync(CreateRenderRequest("render-desktop", desktop));
        var mobileResult = await provider.RenderAsync(CreateRenderRequest("render-mobile", mobile));
        var repeatedDesktopResult = await provider.RenderAsync(CreateRenderRequest("render-desktop", desktop));

        Assert.True(desktopResult.IsSuccess, desktopResult.ErrorMessage);
        Assert.True(mobileResult.IsSuccess, mobileResult.ErrorMessage);
        Assert.True(repeatedDesktopResult.IsSuccess, repeatedDesktopResult.ErrorMessage);
        Assert.NotEqual(desktopResult.Value!.Image.Sha256, mobileResult.Value!.Image.Sha256);
        Assert.Equal(desktopResult.Value.Image.Sha256, repeatedDesktopResult.Value!.Image.Sha256);
        Assert.Equal("deterministic-render-evidence", desktopResult.Value.Image.Label);
        Assert.Equal("deterministic_render_scaffold", desktopResult.Value.Image.Metadata["provider_kind"]);
        Assert.Equal(bool.FalseString, desktopResult.Value.Image.Metadata["visual_fidelity_claimed"]);
        Assert.Equal(bool.FalseString, desktopResult.Value.Image.Metadata["browser_or_playwright_used"]);
        Assert.Equal(bool.FalseString, desktopResult.Value.Image.Metadata["external_ai_used"]);
        Assert.Equal("metadata_only_render_evidence", desktopResult.Value.Image.Metadata["hidden_determinism_boundary"]);
        Assert.Equal("1440", desktopResult.Value.Image.Metadata["viewport_width"]);
        Assert.Equal("390", mobileResult.Value.Image.Metadata["viewport_width"]);
        Assert.Contains("DETERMINISTIC_RENDER_SCAFFOLD_DOES_NOT_CLAIM_VISUAL_FIDELITY", desktopResult.Value.Warnings);
    }

    [Fact]
    public async Task CompareAsync_RejectsMetadataOnlyEchoComparisonAsNotProven()
    {
        var provider = new DeterministicDesignConversionComparisonProvider();
        var image = CreateImage(
            "expected",
            "shared-artifact",
            new RenderQaResolution("desktop", 1440, 900, 1.0),
            "sha256:shared",
            new Dictionary<string, string> { ["viewport_width"] = "1440" });
        var actual = image with { ImageId = "actual" };

        var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
            ComparisonRequestId: "comparison-match",
            RequestId: "request-001",
            Expected: image,
            Actual: actual,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.Verdict.Passed);
        Assert.Equal(0.0, result.Value.Verdict.Score);
        Assert.Contains(result.Value.Verdict.Defects, defect =>
            defect.Category == DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker &&
            defect.Severity == "blocker");
        Assert.Equal(1.0, result.Value.Metrics["metadata_only_score"]);
        Assert.Equal(0.0, result.Value.Metrics["visual_comparison_proven"]);
        Assert.Equal(1.0, result.Value.Metrics["bounded_non_ready"]);
        Assert.Equal(1.0, result.Value.Metrics["visual_comparison_not_proven"]);
        Assert.Equal(0.0, result.Value.Metrics["ai_visual_judge_claimed"]);
        Assert.Equal(DeterministicDesignConversionComparisonProvider.BoundedNonReadyStatus, result.Value.Metadata["status"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["visual_comparison_not_proven"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["product_ready"]);
        Assert.Equal("expected", result.Value.Metadata["expected_image_id"]);
        Assert.Equal("actual", result.Value.Metadata["actual_image_id"]);
        Assert.Equal("sha256:shared", result.Value.Metadata["expected_image_sha256"]);
        Assert.Equal("sha256:shared", result.Value.Metadata["actual_image_sha256"]);
        Assert.StartsWith("artifact://design-conversion/comparison-scaffold/request-001/", result.Value.DiffImageUri, StringComparison.Ordinal);
        Assert.Contains(DeterministicDesignConversionComparisonProvider.AiJudgeClaimedMarker, result.Value.EvidenceRefs);
        Assert.Contains(DeterministicDesignConversionComparisonProvider.BoundaryMarker, result.Value.Verdict.EvidenceRefs);
        Assert.Contains(DeterministicDesignConversionComparisonProvider.BoundedNonReadyStatus, result.Value.EvidenceRefs);
        Assert.Contains(DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker, result.Value.Verdict.EvidenceRefs);
        Assert.Contains(DeterministicDesignConversionComparisonProvider.MetadataOnlyComparisonRejectedMarker, result.Value.EvidenceRefs);
    }

    [Fact]
    public async Task CompareAsync_EmitsDefectsAndScoresMetadataDifferences()
    {
        var provider = new DeterministicDesignConversionComparisonProvider();
        var expected = CreateImage(
            "expected",
            "expected-design-render",
            new RenderQaResolution("desktop", 1440, 900, 1.0),
            "sha256:expected",
            new Dictionary<string, string> { ["source_artifact_kind"] = "design-render" });
        var actual = CreateImage(
            "actual",
            "deterministic-render-evidence",
            new RenderQaResolution("mobile", 390, 844, 3.0),
            "sha256:actual",
            new Dictionary<string, string> { ["provider_kind"] = "deterministic_render_scaffold" });

        var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
            ComparisonRequestId: "comparison-mismatch",
            RequestId: "request-001",
            Expected: expected,
            Actual: actual,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.Verdict.Passed);
        Assert.Equal(0.0, result.Value.Verdict.Score);
        Assert.Equal(0.0, result.Value.Metrics["resolution_match"]);
        Assert.Equal(0.0, result.Value.Metrics["sha256_match"]);
        Assert.Equal(0.0, result.Value.Metrics["metadata_fingerprint_match"]);
        Assert.Equal(0.0, result.Value.Metrics["label_match"]);
        Assert.True(result.Value.Metrics["metadata_only_score"] < DeterministicDesignConversionComparisonProvider.PassThreshold);
        Assert.Contains(result.Value.Verdict.Defects, defect => defect.Category == DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker);
        Assert.Contains(result.Value.Verdict.Defects, defect => defect.Category == "resolution");
        Assert.Contains(result.Value.Verdict.Defects, defect => defect.Category == "artifact-sha256");
        Assert.Contains(result.Value.Verdict.Defects, defect => defect.Category == "metadata-fingerprint");
        Assert.Contains(result.Value.Verdict.Defects, defect => defect.Category == "artifact-label");
        Assert.All(result.Value.Verdict.Defects.Where(defect => defect.Category != DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker), defect =>
            Assert.Contains("not AI visual judgment", defect.Description, StringComparison.Ordinal));
    }

    [Fact]
    public async Task RenderAndComparisonProvidersExposeHiddenDeterminismBoundaryMarkers()
    {
        var renderProvider = new DeterministicDesignConversionRenderProvider();
        var comparisonProvider = new DeterministicDesignConversionComparisonProvider();
        var resolution = new RenderQaResolution("desktop", 1440, 900, 1.0);
        var renderResult = await renderProvider.RenderAsync(CreateRenderRequest("render-boundary", resolution));

        Assert.True(renderResult.IsSuccess, renderResult.ErrorMessage);
        var comparison = await comparisonProvider.CompareAsync(new DesignConversionQaComparisonRequest(
            ComparisonRequestId: "comparison-boundary",
            RequestId: "request-001",
            Expected: renderResult.Value!.Image,
            Actual: renderResult.Value.Image,
            Metadata: new Dictionary<string, string>()));

        Assert.True(comparison.IsSuccess, comparison.ErrorMessage);
        Assert.Equal("metadata_only_render_evidence", renderResult.Value.Image.Metadata["hidden_determinism_boundary"]);
        Assert.False(comparison.Value!.Verdict.Passed);
        Assert.Equal(DeterministicDesignConversionComparisonProvider.BoundedNonReadyStatus, comparison.Value.Metadata["status"]);
        Assert.Contains(DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker, comparison.Value.Verdict.EvidenceRefs);
        Assert.Contains(DeterministicDesignConversionComparisonProvider.BoundaryMarker, comparison.Value!.EvidenceRefs);
        Assert.Contains(DeterministicDesignConversionComparisonProvider.AiJudgeClaimedMarker, comparison.Value.Verdict.EvidenceRefs);
        Assert.Equal(0.0, comparison.Value.Metrics["ai_visual_judge_claimed"]);
    }

    private static DesignConversionRenderRequest CreateRenderRequest(string renderRequestId, RenderQaResolution resolution)
        => new(
            RenderRequestId: renderRequestId,
            RequestId: "request-001",
            Html: "<main class=\"card\"><h1>Hello</h1></main>",
            Css: ".card{display:grid;gap:12px;color:#111;}",
            Resolution: resolution,
            Metadata: new Dictionary<string, string>
            {
                ["source_screenshot_uri"] = "artifact://source-shot.png",
                ["source_screenshot_sha256"] = "sha256:source-shot",
                ["qa_data_role"] = "input_evidence_only"
            });

    private static RenderQaImageRecord CreateImage(
        string imageId,
        string label,
        RenderQaResolution resolution,
        string sha256,
        IReadOnlyDictionary<string, string> metadata)
        => new(
            ImageId: imageId,
            Label: label,
            Resolution: resolution,
            Uri: $"artifact://{imageId}.json",
            Sha256: sha256,
            Metadata: metadata);
}
