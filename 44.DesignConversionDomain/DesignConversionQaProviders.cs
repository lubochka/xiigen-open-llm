// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

public sealed class DeterministicDesignConversionRenderProvider : IDesignConversionRenderProvider
{
    public const string ProviderKind = "deterministic_render_scaffold";
    public const string VisualFidelityClaimedMarker = "visual_fidelity_claimed:false";
    public const string BrowserUsedMarker = "browser_or_playwright_used:false";
    public const string ExternalAiUsedMarker = "external_ai_used:false";
    public const string BoundaryMarker = "hidden_determinism_boundary:metadata_only_render_evidence";

    public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
        DesignConversionRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion deterministic render scaffold cancelled.",
                "CANCELLED"));
        if (request is null)
            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion render request is required.",
                "RENDER_REQUEST_REQUIRED"));
        if (string.IsNullOrWhiteSpace(request.RenderRequestId))
            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion render request id is required.",
                "RENDER_REQUEST_ID_REQUIRED"));
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion request id is required.",
                "REQUEST_ID_REQUIRED"));
        if (request.Resolution is null)
            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion render resolution is required.",
                "RENDER_RESOLUTION_REQUIRED"));

        var htmlSha = Sha256Text(request.Html ?? string.Empty);
        var cssSha = Sha256Text(request.Css ?? string.Empty);
        var metadataFingerprint = FingerprintMetadata(request.Metadata);
        var stablePayload = string.Join(
            "|",
            ProviderKind,
            request.RenderRequestId,
            request.RequestId,
            request.Resolution.ResolutionId,
            request.Resolution.Width.ToString(CultureInfo.InvariantCulture),
            request.Resolution.Height.ToString(CultureInfo.InvariantCulture),
            request.Resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture),
            htmlSha,
            cssSha,
            metadataFingerprint);
        var artifactSha = Sha256Text(stablePayload);
        var renderResultId = $"{request.RenderRequestId}:deterministic-render:{artifactSha[..12]}";
        var metadata = MergeMetadata(
            request.Metadata,
            new Dictionary<string, string>
            {
                ["provider_kind"] = ProviderKind,
                ["render_artifact_kind"] = "deterministic_render_evidence_record",
                ["visual_fidelity_claimed"] = bool.FalseString,
                ["browser_or_playwright_used"] = bool.FalseString,
                ["external_ai_used"] = bool.FalseString,
                ["hidden_determinism_boundary"] = "metadata_only_render_evidence",
                ["html_sha256"] = htmlSha,
                ["css_sha256"] = cssSha,
                ["input_metadata_sha256"] = metadataFingerprint,
                ["viewport_id"] = request.Resolution.ResolutionId,
                ["viewport_width"] = request.Resolution.Width.ToString(CultureInfo.InvariantCulture),
                ["viewport_height"] = request.Resolution.Height.ToString(CultureInfo.InvariantCulture),
                ["device_scale_factor"] = request.Resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)
            });

        var image = new RenderQaImageRecord(
            ImageId: $"{request.RenderRequestId}:image:{artifactSha[..12]}",
            Label: "deterministic-render-evidence",
            Resolution: request.Resolution,
            Uri: $"artifact://design-conversion/render-scaffold/{Uri.EscapeDataString(request.RequestId)}/{artifactSha}.json",
            Sha256: $"sha256:{artifactSha}",
            Metadata: metadata);

        return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
            RenderResultId: renderResultId,
            RenderRequestId: request.RenderRequestId,
            Image: image,
            ElapsedMs: 0,
            Warnings:
            [
                "DETERMINISTIC_RENDER_SCAFFOLD_NOT_BROWSER_SCREENSHOT",
                "DETERMINISTIC_RENDER_SCAFFOLD_DOES_NOT_CLAIM_VISUAL_FIDELITY"
            ])));
    }

    internal static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    internal static string FingerprintMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
            return Sha256Text(string.Empty);

        var canonical = string.Join(
            "\n",
            metadata
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .ThenBy(pair => pair.Value, StringComparer.Ordinal)
                .Select(pair => $"{pair.Key}={pair.Value}"));
        return Sha256Text(canonical);
    }

    internal static IReadOnlyDictionary<string, string> MergeMetadata(
        IReadOnlyDictionary<string, string>? primary,
        IReadOnlyDictionary<string, string> additions)
        => (primary ?? new Dictionary<string, string>())
            .Concat(additions)
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
}

public sealed class DeterministicDesignConversionComparisonProvider : IDesignConversionComparisonProvider
{
    public const string ProviderKind = "deterministic_comparison_scaffold";
    public const string AiJudgeClaimedMarker = "ai_visual_judge_claimed:false";
    public const string BoundaryMarker = "hidden_determinism_boundary:metadata_only_comparison";
    public const string BoundedNonReadyStatus = "bounded_non_ready";
    public const string VisualComparisonNotProvenMarker = "visual_comparison_not_proven";
    public const string MetadataOnlyComparisonRejectedMarker = "metadata_only_echo_comparison_not_visual_qa_proof";
    public const double PassThreshold = 0.85;

    public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
        DesignConversionQaComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion deterministic comparison scaffold cancelled.",
                "CANCELLED"));
        if (request is null)
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion comparison request is required.",
                "COMPARISON_REQUEST_REQUIRED"));
        if (string.IsNullOrWhiteSpace(request.ComparisonRequestId))
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion comparison request id is required.",
                "COMPARISON_REQUEST_ID_REQUIRED"));
        if (request.Expected is null || request.Actual is null)
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion expected and actual render evidence are required.",
                "COMPARISON_IMAGES_REQUIRED"));

        var resolutionMatch = SameResolution(request.Expected.Resolution, request.Actual.Resolution);
        var shaMatch = string.Equals(request.Expected.Sha256, request.Actual.Sha256, StringComparison.Ordinal);
        var labelMatch = string.Equals(request.Expected.Label, request.Actual.Label, StringComparison.Ordinal);
        var expectedMetadataFingerprint = DeterministicDesignConversionRenderProvider.FingerprintMetadata(request.Expected.Metadata);
        var actualMetadataFingerprint = DeterministicDesignConversionRenderProvider.FingerprintMetadata(request.Actual.Metadata);
        var metadataMatch = string.Equals(expectedMetadataFingerprint, actualMetadataFingerprint, StringComparison.Ordinal);

        var defects = BuildDefects(request, resolutionMatch, shaMatch, labelMatch, metadataMatch)
            .Prepend(CreateNotProvenDefect(request))
            .ToArray();
        var metadataOnlyScore = Score(resolutionMatch, shaMatch, labelMatch, metadataMatch);
        var score = 0.0;
        var stablePayload = string.Join(
            "|",
            ProviderKind,
            request.ComparisonRequestId,
            request.RequestId,
            request.Expected.ImageId,
            request.Expected.Sha256,
            request.Actual.ImageId,
            request.Actual.Sha256,
            expectedMetadataFingerprint,
            actualMetadataFingerprint,
            metadataOnlyScore.ToString("0.###", CultureInfo.InvariantCulture));
        var artifactSha = DeterministicDesignConversionRenderProvider.Sha256Text(stablePayload);
        var evidenceRefs = new[]
        {
            $"comparison-artifact-scaffold:{request.ComparisonRequestId}",
            $"expected-image:{request.Expected.ImageId}",
            $"expected-sha:{request.Expected.Sha256}",
            $"actual-image:{request.Actual.ImageId}",
            $"actual-sha:{request.Actual.Sha256}",
            $"diff-sha:sha256:{artifactSha}",
            ProviderKind,
            AiJudgeClaimedMarker,
            BoundaryMarker,
            BoundedNonReadyStatus,
            VisualComparisonNotProvenMarker,
            MetadataOnlyComparisonRejectedMarker
        };
        var verdict = new VisualVerdict(
            VerdictId: $"{request.ComparisonRequestId}:deterministic-verdict:{artifactSha[..12]}",
            Score: score,
            Defects: defects,
            EvidenceRefs: evidenceRefs,
            Passed: false);

        var artifact = new DesignConversionQaComparisonArtifact(
            ArtifactId: $"{request.ComparisonRequestId}:deterministic-comparison:{artifactSha[..12]}",
            ComparisonRequestId: request.ComparisonRequestId,
            Expected: request.Expected,
            Actual: request.Actual,
            Verdict: verdict,
            DiffImageUri: $"artifact://design-conversion/comparison-scaffold/{Uri.EscapeDataString(request.RequestId)}/{artifactSha}.json",
            DiffImageSha256: $"sha256:{artifactSha}",
            Metrics: new Dictionary<string, double>
            {
                ["metadata_only_score"] = metadataOnlyScore,
                ["visual_comparison_proven"] = 0.0,
                ["bounded_non_ready"] = 1.0,
                ["visual_comparison_not_proven"] = 1.0,
                ["resolution_match"] = resolutionMatch ? 1.0 : 0.0,
                ["sha256_match"] = shaMatch ? 1.0 : 0.0,
                ["label_match"] = labelMatch ? 1.0 : 0.0,
                ["metadata_fingerprint_match"] = metadataMatch ? 1.0 : 0.0,
                ["ai_visual_judge_claimed"] = 0.0
            },
            EvidenceRefs: evidenceRefs)
        {
            Metadata = new Dictionary<string, string>
            {
                ["status"] = BoundedNonReadyStatus,
                ["provider_kind"] = ProviderKind,
                ["hidden_determinism_boundary"] = "metadata_only_comparison",
                ["visual_comparison_not_proven"] = bool.TrueString,
                ["metadata_only_comparison_rejected"] = bool.TrueString,
                ["product_ready"] = bool.FalseString,
                ["expected_image_id"] = request.Expected.ImageId,
                ["expected_image_sha256"] = request.Expected.Sha256,
                ["actual_image_id"] = request.Actual.ImageId,
                ["actual_image_sha256"] = request.Actual.Sha256,
                ["diff_image_sha256"] = $"sha256:{artifactSha}",
                ["metadata_only_score"] = metadataOnlyScore.ToString("0.###", CultureInfo.InvariantCulture),
                ["verdict_score"] = score.ToString("0.###", CultureInfo.InvariantCulture)
            }
        };

        return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(artifact));
    }

    private static double Score(bool resolutionMatch, bool shaMatch, bool labelMatch, bool metadataMatch)
    {
        var penalty = 0.0;
        if (!resolutionMatch)
            penalty += 0.40;
        if (!shaMatch)
            penalty += 0.35;
        if (!metadataMatch)
            penalty += 0.15;
        if (!labelMatch)
            penalty += 0.10;

        return Math.Max(0.0, Math.Round(1.0 - penalty, 3, MidpointRounding.AwayFromZero));
    }

    private static IReadOnlyList<VisualDefect> BuildDefects(
        DesignConversionQaComparisonRequest request,
        bool resolutionMatch,
        bool shaMatch,
        bool labelMatch,
        bool metadataMatch)
    {
        var defects = new List<VisualDefect>();
        if (!resolutionMatch)
            defects.Add(CreateDefect(request, "resolution", "major", "Expected and rendered artifact resolution metadata differ.", 0));
        if (!shaMatch)
            defects.Add(CreateDefect(request, "artifact-sha256", "major", "Expected and rendered artifact hashes differ.", 1));
        if (!metadataMatch)
            defects.Add(CreateDefect(request, "metadata-fingerprint", "minor", "Expected and rendered artifact metadata fingerprints differ.", 2));
        if (!labelMatch)
            defects.Add(CreateDefect(request, "artifact-label", "minor", "Expected and rendered artifact labels differ.", 3));

        return defects;
    }

    private static VisualDefect CreateDefect(
        DesignConversionQaComparisonRequest request,
        string category,
        string severity,
        string description,
        int offset)
    {
        var width = Math.Max(1, request.Actual.Resolution?.Width ?? request.Expected.Resolution?.Width ?? 1);
        var height = Math.Max(1, request.Actual.Resolution?.Height ?? request.Expected.Resolution?.Height ?? 1);
        return new VisualDefect(
            DefectId: $"{request.ComparisonRequestId}:deterministic-defect:{category}",
            Category: category,
            Severity: severity,
            Description: $"{description} Scaffold comparison uses metadata only, not AI visual judgment.",
            X: Math.Min(width - 1, 8 + (offset * 12)),
            Y: Math.Min(height - 1, 8 + (offset * 12)),
            Width: Math.Max(1, Math.Round(width * 0.1, 3, MidpointRounding.AwayFromZero)),
            Height: Math.Max(1, Math.Round(height * 0.1, 3, MidpointRounding.AwayFromZero)),
            EvidenceImageIds: [request.Expected.ImageId, request.Actual.ImageId]);
    }

    private static VisualDefect CreateNotProvenDefect(DesignConversionQaComparisonRequest request)
    {
        var width = Math.Max(1, request.Actual.Resolution?.Width ?? request.Expected.Resolution?.Width ?? 1);
        var height = Math.Max(1, request.Actual.Resolution?.Height ?? request.Expected.Resolution?.Height ?? 1);
        return new VisualDefect(
            DefectId: $"{request.ComparisonRequestId}:deterministic-defect:{VisualComparisonNotProvenMarker}",
            Category: VisualComparisonNotProvenMarker,
            Severity: "blocker",
            Description: "Deterministic comparison scaffold only compares refs and metadata; it is bounded_non_ready and cannot prove visual QA.",
            X: 0,
            Y: 0,
            Width: Math.Max(1, Math.Round(width * 0.1, 3, MidpointRounding.AwayFromZero)),
            Height: Math.Max(1, Math.Round(height * 0.1, 3, MidpointRounding.AwayFromZero)),
            EvidenceImageIds: [request.Expected.ImageId, request.Actual.ImageId]);
    }

    private static bool SameResolution(RenderQaResolution? expected, RenderQaResolution? actual)
        => expected is not null &&
           actual is not null &&
           string.Equals(expected.ResolutionId, actual.ResolutionId, StringComparison.Ordinal) &&
           expected.Width == actual.Width &&
           expected.Height == actual.Height &&
           Math.Abs(expected.DeviceScaleFactor - actual.DeviceScaleFactor) < 0.000001;
}
