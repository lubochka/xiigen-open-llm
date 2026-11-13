// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

public sealed record DesignConversionRenderRequest(
    string RenderRequestId,
    string RequestId,
    string Html,
    string Css,
    RenderQaResolution Resolution,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionRenderResult(
    string RenderResultId,
    string RenderRequestId,
    RenderQaImageRecord Image,
    long ElapsedMs,
    IReadOnlyList<string> Warnings)
{
    internal DesignConversionBrowserRenderProofReceipt? BrowserRenderProofReceipt { get; init; }
}

internal sealed record DesignConversionBrowserRenderProofReceipt(
    string IssuerKind,
    string RenderResultId,
    string RenderRequestId,
    string RequestId,
    string ImageId,
    string ScreenshotUri,
    string NormalizedSha256,
    long ByteLength,
    int PngWidth,
    int PngHeight,
    bool PngSignatureVerified,
    bool Sha256Verified,
    bool HasNonBlankPixels,
    int NonWhitePixelCount,
    int DistinctColorCount,
    string ResolutionId,
    int ViewportWidth,
    int ViewportHeight,
    double DeviceScaleFactor)
{
    internal string InspectedLocalPath { get; init; } = string.Empty;
}

public sealed record DesignConversionQaComparisonRequest(
    string ComparisonRequestId,
    string RequestId,
    RenderQaImageRecord Expected,
    RenderQaImageRecord Actual,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionQaComparisonArtifact(
    string ArtifactId,
    string ComparisonRequestId,
    RenderQaImageRecord Expected,
    RenderQaImageRecord Actual,
    VisualVerdict Verdict,
    string DiffImageUri,
    string DiffImageSha256,
    IReadOnlyDictionary<string, double> Metrics,
    IReadOnlyList<string> EvidenceRefs)
{
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
