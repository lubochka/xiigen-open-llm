// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

public sealed record DesignConversionLocalRenderQaProbeRequest(
    string ProbeId,
    string RequestId,
    string Html,
    string Css,
    IReadOnlyList<RenderQaResolution> Viewports,
    IReadOnlyList<DesignRenderArtifact> ExpectedDesignExports,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionLocalRenderQaArtifactRef(
    string ArtifactId,
    string Kind,
    string ViewportId,
    string UriRef,
    string Sha256,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionLocalRenderQaViewportResult(
    string ViewportId,
    RenderQaResolution Viewport,
    DesignConversionLocalRenderQaArtifactRef Expected,
    DesignConversionLocalRenderQaArtifactRef Actual,
    DesignConversionLocalRenderQaArtifactRef Comparison,
    double Score,
    int DefectCount,
    bool Passed,
    IReadOnlyList<string> DefectCategories,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings);

public sealed record DesignConversionLocalRenderQaProbeResult(
    string ProbeId,
    string RequestId,
    IReadOnlyList<DesignConversionLocalRenderQaViewportResult> Viewports,
    double AverageScore,
    double MinimumScore,
    int TotalDefectCount,
    bool Passed,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionLocalRenderQaProbe
{
    public const string ProbeMarker = "local_render_qa_probe";
    public const string RenderProviderCallThroughMarker = "render_provider_call_through";
    public const string ComparisonProviderCallThroughMarker = "comparison_provider_call_through";
    public const string BrowserRenderNotExecutedMarker = "browser_render_not_executed";
    public const string BrowserRenderExecutedMarker = "browser_render_executed";
    public const string VisualAiNotExecutedMarker = "visual_ai_not_executed";
    public const string RefsOnlyMarker = "render_qa_refs_or_hashes_only";

    private readonly IDesignConversionRenderProvider _renderProvider;
    private readonly IDesignConversionComparisonProvider _comparisonProvider;

    public DesignConversionLocalRenderQaProbe(
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider)
    {
        _renderProvider = renderProvider ?? throw new ArgumentNullException(nameof(renderProvider));
        _comparisonProvider = comparisonProvider ?? throw new ArgumentNullException(nameof(comparisonProvider));
    }

    public async Task<OperationResult<DesignConversionLocalRenderQaProbeResult>> RunAsync(
        DesignConversionLocalRenderQaProbeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionLocalRenderQaProbeResult>.Fail(
                "DesignConversion local render QA probe cancelled.",
                "CANCELLED");

        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionLocalRenderQaProbeResult>();

        var viewportResults = new List<DesignConversionLocalRenderQaViewportResult>();
        var comparisonArtifacts = new List<DesignConversionQaComparisonArtifact>();
        var warnings = new List<string>();
        var evidenceRefs = new List<string>
        {
            ProbeMarker,
            RenderProviderCallThroughMarker,
            ComparisonProviderCallThroughMarker,
            RefsOnlyMarker,
        };
        var browserRenderExecuted = false;

        for (var index = 0; index < request.Viewports.Count; index++)
        {
            var viewport = request.Viewports[index];
            var expectedArtifact = FindExpectedArtifact(request.ExpectedDesignExports, viewport);
            if (expectedArtifact is null)
            {
                return OperationResult<DesignConversionLocalRenderQaProbeResult>.Fail(
                    $"DesignConversion local render QA expected design export is missing for viewport '{ViewportLabel(viewport)}'.",
                    "LOCAL_RENDER_QA_EXPECTED_REF_MISSING");
            }

            var render = await _renderProvider.RenderAsync(
                new DesignConversionRenderRequest(
                    RenderRequestId: $"{request.ProbeId}:render:{index + 1}",
                    RequestId: request.RequestId,
                    Html: request.Html,
                    Css: request.Css,
                    Resolution: viewport,
                    Metadata: BuildProviderMetadata(request, viewport, index)),
                cancellationToken);

            if (!render.IsSuccess)
                return render.MapError<DesignConversionLocalRenderQaProbeResult>();

            browserRenderExecuted |= IsTruthy(render.Value!.Image.Metadata, "browser_or_playwright_used");
            warnings.AddRange(render.Value.Warnings);

            var expectedImage = BuildExpectedImage(expectedArtifact, viewport, index);
            var comparison = await _comparisonProvider.CompareAsync(
                new DesignConversionQaComparisonRequest(
                    ComparisonRequestId: $"{request.ProbeId}:comparison:{index + 1}",
                    RequestId: request.RequestId,
                    Expected: expectedImage,
                    Actual: render.Value.Image,
                    Metadata: new Dictionary<string, string>
                    {
                        ["probe_id"] = request.ProbeId,
                        ["viewport_id"] = SafeViewportId(viewport),
                        ["comparison_provider"] = "injected_provider",
                        ["qa_data_role"] = "local_render_qa_evidence_only"
                    }),
                cancellationToken);

            if (!comparison.IsSuccess)
                return comparison.MapError<DesignConversionLocalRenderQaProbeResult>();

            var artifact = comparison.Value!;
            comparisonArtifacts.Add(artifact);
            evidenceRefs.Add($"render-artifact:{HashRef(render.Value.RenderResultId)}");
            evidenceRefs.Add($"comparison-artifact:{HashRef(artifact.ArtifactId)}");
            evidenceRefs.AddRange(artifact.EvidenceRefs.Select(reference => HashRef(reference)));

            viewportResults.Add(new DesignConversionLocalRenderQaViewportResult(
                ViewportId: SafeViewportId(viewport),
                Viewport: viewport,
                Expected: ToArtifactRef(expectedImage.ImageId, "expected-design-export", SafeViewportId(viewport), expectedImage.Uri, expectedImage.Sha256),
                Actual: ToArtifactRef(render.Value.Image.ImageId, "rendered-output", SafeViewportId(viewport), render.Value.Image.Uri, render.Value.Image.Sha256),
                Comparison: ToArtifactRef(
                    artifact.ArtifactId,
                    "comparison-diff",
                    SafeViewportId(viewport),
                    artifact.DiffImageUri,
                    artifact.DiffImageSha256,
                    artifact.Metadata),
                Score: artifact.Verdict.Score,
                DefectCount: artifact.Verdict.Defects.Count,
                Passed: artifact.Verdict.Passed,
                DefectCategories: artifact.Verdict.Defects
                    .Select(defect => DesignConversionDpoLeakageGuard.SanitizeScalar(defect.Category, "local_render_qa_defect_category", new SortedSet<string>(StringComparer.Ordinal)))
                    .Where(category => !string.IsNullOrWhiteSpace(category))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(category => category, StringComparer.Ordinal)
                    .ToArray(),
                EvidenceRefs: artifact.Verdict.EvidenceRefs
                    .Concat(artifact.EvidenceRefs)
                    .Select(reference => HashRef(reference))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Warnings: render.Value.Warnings.Distinct(StringComparer.Ordinal).ToArray()));
        }

        var scoreValues = viewportResults.Select(result => result.Score).ToArray();
        var totalDefects = viewportResults.Sum(result => result.DefectCount);
        var averageScore = scoreValues.Length == 0
            ? 0.0
            : Math.Round(scoreValues.Average(), 4, MidpointRounding.AwayFromZero);
        var minimumScore = scoreValues.Length == 0
            ? 0.0
            : Math.Round(scoreValues.Min(), 4, MidpointRounding.AwayFromZero);
        var passed = viewportResults.Count == request.Viewports.Count &&
                     viewportResults.All(result => result.Passed) &&
                     totalDefects == 0;
        var visualAiBoundaryMetadata = BuildVisualAiBoundaryMetadata(comparisonArtifacts);
        var realExternalVisualAiExecuted = IsTruthy(visualAiBoundaryMetadata, "real_external_provider_executed") ||
                                           IsTruthy(visualAiBoundaryMetadata, "external_visual_ai_executed");

        evidenceRefs.Add(browserRenderExecuted ? BrowserRenderExecutedMarker : BrowserRenderNotExecutedMarker);
        evidenceRefs.Add(realExternalVisualAiExecuted ? "visual_ai_executed" : VisualAiNotExecutedMarker);
        if (!browserRenderExecuted)
            warnings.Add("BROWSER_RENDER_NOT_EXECUTED_PROVIDER_BOUNDARY");
        if (!realExternalVisualAiExecuted)
            warnings.Add("VISUAL_AI_NOT_EXECUTED_LOCAL_RENDER_QA_PROBE");

        return OperationResult<DesignConversionLocalRenderQaProbeResult>.Ok(
            new DesignConversionLocalRenderQaProbeResult(
                ProbeId: request.ProbeId,
                RequestId: request.RequestId,
                Viewports: viewportResults,
                AverageScore: averageScore,
                MinimumScore: minimumScore,
                TotalDefectCount: totalDefects,
                Passed: passed,
                EvidenceRefs: evidenceRefs.Distinct(StringComparer.Ordinal).OrderBy(reference => reference, StringComparer.Ordinal).ToArray(),
                Warnings: warnings.Distinct(StringComparer.Ordinal).OrderBy(warning => warning, StringComparer.Ordinal).ToArray(),
                BoundaryMetadata: BuildBoundaryMetadata(
                    request,
                    viewportResults,
                    comparisonArtifacts,
                    browserRenderExecuted,
                    averageScore,
                    minimumScore,
                    totalDefects,
                    passed)));
    }

    private static OperationResult<bool> ValidateRequest(DesignConversionLocalRenderQaProbeRequest request)
    {
        if (request is null)
            return OperationResult<bool>.Fail("DesignConversion local render QA probe request is required.", "LOCAL_RENDER_QA_REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.ProbeId))
            return OperationResult<bool>.Fail("DesignConversion local render QA probe id is required.", "LOCAL_RENDER_QA_PROBE_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return OperationResult<bool>.Fail("DesignConversion request id is required.", "REQUEST_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.Html))
            return OperationResult<bool>.Fail("DesignConversion submitted HTML is required for local render QA.", "LOCAL_RENDER_QA_HTML_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.Css))
            return OperationResult<bool>.Fail("DesignConversion submitted CSS is required for local render QA.", "LOCAL_RENDER_QA_CSS_REQUIRED");
        if (request.Viewports is null || request.Viewports.Count == 0)
            return OperationResult<bool>.Fail("DesignConversion local render QA requires at least one viewport.", "LOCAL_RENDER_QA_VIEWPORTS_REQUIRED");
        if (request.ExpectedDesignExports is null || request.ExpectedDesignExports.Count == 0)
            return OperationResult<bool>.Fail("DesignConversion local render QA requires expected design export refs.", "LOCAL_RENDER_QA_EXPECTED_REFS_REQUIRED");

        foreach (var viewport in request.Viewports)
        {
            if (viewport is null)
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion local render QA viewport is required.",
                    "LOCAL_RENDER_QA_VIEWPORT_INVALID");
            }

            if (viewport.Width <= 0 || viewport.Height <= 0 || viewport.DeviceScaleFactor <= 0)
            {
                return OperationResult<bool>.Fail(
                    $"DesignConversion local render QA viewport dimensions must be positive for '{ViewportLabel(viewport)}'.",
                    "LOCAL_RENDER_QA_VIEWPORT_INVALID");
            }
        }

        foreach (var artifact in request.ExpectedDesignExports)
        {
            if (artifact is null ||
                artifact.Width <= 0 ||
                artifact.Height <= 0 ||
                string.IsNullOrWhiteSpace(artifact.Uri) ||
                string.IsNullOrWhiteSpace(artifact.Sha256))
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion local render QA expected design refs require positive dimensions, uri, and sha256.",
                    "LOCAL_RENDER_QA_EXPECTED_REF_INVALID");
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private static IReadOnlyDictionary<string, string> BuildProviderMetadata(
        DesignConversionLocalRenderQaProbeRequest request,
        RenderQaResolution viewport,
        int index)
        => new Dictionary<string, string>
        {
            ["probe_id"] = request.ProbeId,
            ["viewport_id"] = SafeViewportId(viewport),
            ["viewport_index"] = (index + 1).ToString(CultureInfo.InvariantCulture),
            ["qa_data_role"] = "local_render_qa_evidence_only",
            ["html_sha256"] = Sha256Text(request.Html),
            ["css_sha256"] = Sha256Text(request.Css)
        };

    private static RenderQaImageRecord BuildExpectedImage(
        DesignRenderArtifact artifact,
        RenderQaResolution viewport,
        int index)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        return new RenderQaImageRecord(
            ImageId: $"{HashRef(artifact.ArtifactId)}:expected:{index + 1}",
            Label: "expected-design-export",
            Resolution: viewport,
            Uri: DesignConversionDpoLeakageGuard.SanitizeScalar(artifact.Uri, "local_render_qa_expected_uri", redactions),
            Sha256: artifact.Sha256,
            Metadata: new Dictionary<string, string>
            {
                ["source_artifact_ref"] = HashRef(artifact.ArtifactId),
                ["source_artifact_kind"] = DesignConversionDpoLeakageGuard.SanitizeScalar(artifact.Kind, "local_render_qa_artifact_kind", redactions),
                ["redaction_count"] = redactions.Count.ToString(CultureInfo.InvariantCulture)
            });
    }

    private static DesignConversionLocalRenderQaArtifactRef ToArtifactRef(
        string artifactId,
        string kind,
        string viewportId,
        string uri,
        string sha256,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var artifactMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["refs_or_hashes_only"] = bool.TrueString,
            ["redaction_count"] = redactions.Count.ToString(CultureInfo.InvariantCulture)
        };
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (IsAllowedArtifactMetadataKey(pair.Key))
                artifactMetadata[pair.Key] = DesignConversionDpoLeakageGuard.SanitizeScalar(
                    pair.Value,
                    $"local_render_qa_{kind}_{pair.Key}",
                    redactions);
        }

        artifactMetadata["redaction_count"] = redactions.Count.ToString(CultureInfo.InvariantCulture);
        return new DesignConversionLocalRenderQaArtifactRef(
            ArtifactId: HashRef(artifactId),
            Kind: kind,
            ViewportId: viewportId,
            UriRef: DesignConversionDpoLeakageGuard.SanitizeScalar(uri, $"local_render_qa_{kind}_uri", redactions),
            Sha256: sha256,
            Metadata: artifactMetadata);
    }

    private static IReadOnlyDictionary<string, string> BuildBoundaryMetadata(
        DesignConversionLocalRenderQaProbeRequest request,
        IReadOnlyList<DesignConversionLocalRenderQaViewportResult> results,
        IReadOnlyList<DesignConversionQaComparisonArtifact> comparisonArtifacts,
        bool browserRenderExecuted,
        double averageScore,
        double minimumScore,
        int totalDefects,
        bool passed)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["local_render_qa_probe"] = bool.TrueString,
            ["render_provider_invoked"] = bool.TrueString,
            ["comparison_provider_invoked"] = bool.TrueString,
            ["viewport_count"] = request.Viewports.Count.ToString(CultureInfo.InvariantCulture),
            ["expected_design_export_count"] = request.ExpectedDesignExports.Count.ToString(CultureInfo.InvariantCulture),
            ["result_viewport_count"] = results.Count.ToString(CultureInfo.InvariantCulture),
            ["browser_render_executed"] = browserRenderExecuted.ToString(),
            ["browser_render_not_executed"] = (!browserRenderExecuted).ToString(),
            ["render_artifact_refs"] = "refs_or_hashes_only",
            ["comparison_artifact_refs"] = "refs_or_hashes_only",
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_expected_design_export_included"] = bool.FalseString,
            ["html_sha256"] = Sha256Text(request.Html),
            ["css_sha256"] = Sha256Text(request.Css),
            ["average_score"] = averageScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["minimum_score"] = minimumScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["total_defect_count"] = totalDefects.ToString(CultureInfo.InvariantCulture),
            ["passed"] = passed.ToString()
        };
        foreach (var pair in BuildVisualAiBoundaryMetadata(comparisonArtifacts))
            metadata[pair.Key] = pair.Value;

        metadata["visual_ai_executed"] = metadata["real_external_provider_executed"];
        metadata["visual_ai_not_executed"] = (!IsTruthy(metadata, "visual_ai_executed")).ToString();
        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildVisualAiBoundaryMetadata(
        IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts)
    {
        var configured = artifacts.Any(artifact => IsTruthy(artifact.Metadata, "external_visual_ai_configured"));
        var contractExecuted = artifacts.Any(artifact => IsTruthy(artifact.Metadata, "visual_ai_provider_contract_executed"));
        var realExternalExecuted = artifacts.Any(artifact => IsTruthy(artifact.Metadata, "real_external_provider_executed"));
        var networkCallExecuted = artifacts.Any(artifact => IsTruthy(artifact.Metadata, "network_call_executed"));
        var fakeClient = artifacts.Any(artifact => IsTruthy(artifact.Metadata, "fake_client"));
        var blocked = artifacts.Any(artifact => IsTruthy(artifact.Metadata, "external_visual_ai_blocked"));
        var providerNames = DistinctMetadataValues(artifacts, "provider_name");
        var providerModels = DistinctMetadataValues(artifacts, "provider_model");
        var blockedReasons = DistinctMetadataValues(artifacts, "blocked_reason")
            .Where(reason => !string.Equals(reason, "none", StringComparison.Ordinal))
            .ToArray();
        var estimatedCost = artifacts
            .Select(artifact => TryReadDecimal(artifact.Metadata, "estimated_cost_usd"))
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .Sum();

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["external_visual_ai_configured"] = configured.ToString(),
            ["external_visual_ai_executed"] = realExternalExecuted.ToString(),
            ["visual_ai_provider_contract_executed"] = contractExecuted.ToString(),
            ["real_external_provider_executed"] = realExternalExecuted.ToString(),
            ["network_call_executed"] = networkCallExecuted.ToString(),
            ["fake_client"] = fakeClient.ToString(),
            ["external_visual_ai_blocked"] = blocked.ToString(),
            ["blocked_reason"] = blockedReasons.Length == 0 ? "none" : string.Join("|", blockedReasons),
            ["provider_name"] = providerNames.Length == 0 ? "none" : string.Join("|", providerNames),
            ["provider_model"] = providerModels.Length == 0 ? "none" : string.Join("|", providerModels),
            ["estimated_cost_usd"] = estimatedCost.ToString("0.####", CultureInfo.InvariantCulture),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey, DesignConversionVisualAiProviderSelectionPolicy.PolicyName),
            [DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey, DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders),
            [DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey, "auto"),
            [DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey, "auto"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey, "none"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderNameKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderNameKey, "none"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey, "not_selected"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey, "not_configured"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey, "fallback_unavailable"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey, "no_visual_provider_artifact"),
            [DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey, "3"),
            [DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey] = JoinPolicyMetadata(artifacts, DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey, DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders),
            [DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };
    }

    private static string[] DistinctMetadataValues(
        IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts,
        string key)
        => artifacts
            .Select(artifact => artifact.Metadata.TryGetValue(key, out var value) ? value : null)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray()!;

    private static string JoinPolicyMetadata(
        IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts,
        string key,
        string fallback)
    {
        var values = DistinctMetadataValues(artifacts, key);
        return values.Length == 0 ? fallback : string.Join("|", values);
    }

    private static decimal? TryReadDecimal(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static bool IsAllowedArtifactMetadataKey(string key)
        => key is "external_visual_ai_configured"
            or "external_visual_ai_executed"
            or "visual_ai_provider_contract_executed"
            or "real_external_provider_executed"
            or "network_call_executed"
            or "fake_client"
            or "external_visual_ai_blocked"
            or "blocked_reason"
            or "provider_name"
            or "provider_model"
            or "estimated_cost_usd"
            or "visual_ai_provider_selection_policy"
            or "visual_ai_candidate_providers"
            or "visual_ai_requested_provider"
            or "visual_ai_requested_provider_name"
            or "visual_ai_selected_provider"
            or "visual_ai_selected_provider_name"
            or "visual_ai_selected_provider_status"
            or "visual_ai_provider_mode"
            or "visual_ai_provider_fallback_status"
            or "visual_ai_provider_fallback_reason"
            or "visual_ai_unavailable_provider_count"
            or "visual_ai_unavailable_providers"
            or "visual_ai_real_cloud_provider_claimed"
            or "image_payload_mode"
            or "raw_html_included"
            or "raw_css_included"
            or "raw_design_tree_included"
            or "provider_payload_included";

    private static DesignRenderArtifact? FindExpectedArtifact(
        IReadOnlyList<DesignRenderArtifact> artifacts,
        RenderQaResolution viewport)
        => artifacts.FirstOrDefault(artifact =>
            artifact.Width == viewport.Width &&
            artifact.Height == viewport.Height);

    private static string SafeViewportId(RenderQaResolution resolution)
        => string.IsNullOrWhiteSpace(resolution.ResolutionId)
            ? $"{resolution.Width}x{resolution.Height}@{resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)}"
            : resolution.ResolutionId;

    private static string ViewportLabel(RenderQaResolution resolution)
        => $"{SafeViewportId(resolution)} ({resolution.Width.ToString(CultureInfo.InvariantCulture)}x{resolution.Height.ToString(CultureInfo.InvariantCulture)}@{resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)})";

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty))).ToLowerInvariant();

    private static string HashRef(string? value)
        => DesignConversionDpoLeakageGuard.HashRef("dcm-local-render-qa", value);
}
