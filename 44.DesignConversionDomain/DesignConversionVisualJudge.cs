// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure;

[assembly: InternalsVisibleTo("LlmFromScratch.Tests")]
[assembly: InternalsVisibleTo("DesignConversion.ModelActivation.Tests")]

namespace DesignConversionDomain;

public interface IDesignConversionVisualJudge
{
    Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
        DesignConversionVisualJudgeRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record DesignConversionVisualJudgeRequest(
    string JudgeRequestId,
    string RequestId,
    RenderQaImageRecord ExpectedScreenshot,
    RenderQaImageRecord RenderedImage,
    string? DiffArtifactUri,
    string? DiffArtifactSha256,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionVisualJudgeProviderMetadata(
    string ProviderName,
    string EndpointHost,
    string ModelId,
    bool ExternalCallExecuted,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionVisualJudgeResult(
    string JudgeResultId,
    string JudgeRequestId,
    string Status,
    bool IsBlocked,
    VisualVerdict Verdict,
    DesignConversionVisualJudgeProviderMetadata Provider,
    decimal? EstimatedCostUsd,
    IReadOnlyDictionary<string, double> TokenMetadata,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> Metadata)
{
    public DesignConversionQaComparisonArtifact ToComparisonArtifact(DesignConversionVisualJudgeRequest request)
        => new(
            ArtifactId: $"{JudgeResultId}:comparison-artifact",
            ComparisonRequestId: request.JudgeRequestId,
            Expected: request.ExpectedScreenshot,
            Actual: request.RenderedImage,
            Verdict: Verdict,
            DiffImageUri: request.DiffArtifactUri ?? $"artifact://design-conversion/visual-judge/{Uri.EscapeDataString(JudgeResultId)}",
            DiffImageSha256: request.DiffArtifactSha256 ?? string.Empty,
            Metrics: new Dictionary<string, double>
            {
                ["visual_judge_score"] = Verdict.Score,
                ["external_ai_visual_judge_executed"] = Provider.ExternalCallExecuted ? 1.0 : 0.0,
                ["visual_judge_blocked"] = IsBlocked ? 1.0 : 0.0
            },
            EvidenceRefs: EvidenceRefs);
}

public sealed record DesignConversionExternalVisualJudgeOptions(
    Uri? Endpoint,
    string ProviderName,
    string ModelId,
    string? ApiKey,
    string ApiKeyHeaderName = "Authorization",
    string ApiKeyScheme = "Bearer",
    IReadOnlyDictionary<string, string>? Metadata = null,
    decimal? ServerOwnedCostPerRequestUsd = null);

public sealed record DesignConversionVisualJudgeImageContent(
    string MediaType,
    string Sha256,
    byte[] Bytes);

public interface IDesignConversionVisualJudgeImageContentResolver
{
    Task<OperationResult<DesignConversionVisualJudgeImageContent>> ResolveAsync(
        RenderQaImageRecord image,
        string role,
        long maxBytes,
        CancellationToken cancellationToken = default);
}

public sealed class NoConfiguredDesignConversionVisualJudgeImageContentResolver : IDesignConversionVisualJudgeImageContentResolver
{
    public const string ErrorCode = "EXTERNAL_VISUAL_JUDGE_IMAGE_RESOLVER_REQUIRED";

    public Task<OperationResult<DesignConversionVisualJudgeImageContent>> ResolveAsync(
        RenderQaImageRecord image,
        string role,
        long maxBytes,
        CancellationToken cancellationToken = default)
        => Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
            "External visual judge image content resolver is required before vendor dispatch.",
            ErrorCode));
}

public sealed class DefaultLocalDesignConversionVisualJudge : IDesignConversionVisualJudge
{
    public const string StatusOk = "LOCAL_VISUAL_LABELER_VERDICT_RECEIVED";
    public const string ProviderName = "local-metadata-visual-labeler";
    public const string ModelId = "deterministic-metadata-v1";
    public const string EvidenceMarker = "local_visual_labeler_executed:true";
    public const string ExternalProviderNotUsedMarker = "external_visual_provider_not_used_by_default";
    public const string ComparisonMeasurementMissingCategory = "comparison_measurement_missing";
    public const string ComparisonMeasurementMissingEvidenceMarker = "local_visual_labeler_missing_comparison_measurement";
    public const string ComparisonMeasurementMissingStatus = "missing_supported_score_or_distance";

    private const double DefaultQualityThreshold = 0.85;

    public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
        DesignConversionVisualJudgeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Fail(
                "DesignConversion visual judge cancelled.",
                "CANCELLED"));

        var validation = DesignConversionVisualJudgeValidation.Validate(request);
        if (!validation.IsSuccess)
            return Task.FromResult(validation.MapError<DesignConversionVisualJudgeResult>());

        var threshold = ResolveQualityThreshold(request.Metadata);
        var comparisonMeasurement = ResolveComparisonMeasurement(request.Metadata);
        var defects = BuildDefects(request, threshold, comparisonMeasurement);
        var score = ResolveScore(comparisonMeasurement, defects);
        var passed = comparisonMeasurement.IsPresent &&
                     score >= threshold &&
                     defects.All(defect => !IsMajorOrBlocker(defect.Severity));
        var comparisonMeasurementPresent = comparisonMeasurement.IsPresent ? bool.TrueString : bool.FalseString;
        var comparisonMeasurementEvidenceRefs = comparisonMeasurement.IsPresent
            ? Array.Empty<string>()
            : new[]
            {
                $"local_visual_labeler_comparison_measurement_present={bool.FalseString}",
                ComparisonMeasurementMissingEvidenceMarker
            };
        var evidenceRefs = new[]
            {
                $"visual-judge-request:{request.JudgeRequestId}",
                EvidenceMarker,
                ExternalProviderNotUsedMarker,
                NoConfiguredVisualJudge.EvidenceMarker,
                DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderBoundaryMarker,
                DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
                DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker,
                $"local_visual_labeler_score={score.ToString("0.###", CultureInfo.InvariantCulture)}",
                $"local_visual_labeler_threshold={threshold.ToString("0.###", CultureInfo.InvariantCulture)}"
            }
            .Concat(comparisonMeasurementEvidenceRefs)
            .Concat(defects.Select(defect => $"local_visual_labeler_defect:{defect.Category}"))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var verdict = new VisualVerdict(
            VerdictId: $"{request.JudgeRequestId}:local-visual-labeler",
            Score: score,
            Defects: defects,
            EvidenceRefs: evidenceRefs,
            Passed: passed);

        return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
            JudgeResultId: $"{request.JudgeRequestId}:local-labeler-result",
            JudgeRequestId: request.JudgeRequestId,
            Status: StatusOk,
            IsBlocked: false,
            Verdict: verdict,
            Provider: new DesignConversionVisualJudgeProviderMetadata(
                ProviderName: ProviderName,
                EndpointHost: "local",
                ModelId: ModelId,
                ExternalCallExecuted: false,
                Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(
                    new Dictionary<string, string>
                    {
                        ["configured"] = bool.TrueString,
                        ["provider_boundary"] = DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
                        [DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker] = bool.TrueString,
                        ["external_provider_call"] = bool.FalseString,
                        ["external_ai_visual_judge_executed"] = bool.FalseString,
                        ["visual_ai_executed"] = bool.FalseString
                    },
                    DesignConversionVisualAiProviderSelectionPolicy.DescribeLocalStub(
                        ProviderName,
                        ExternalProviderNotUsedMarker))),
            EstimatedCostUsd: 0m,
            TokenMetadata: new Dictionary<string, double>(),
            EvidenceRefs: evidenceRefs,
            Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(
                new Dictionary<string, string>
                {
                    ["status"] = StatusOk,
                    ["provider_boundary"] = DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
                    [DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker] = bool.TrueString,
                    ["external_ai_visual_judge_executed"] = bool.FalseString,
                    ["visual_ai_executed"] = bool.FalseString,
                    ["defect_count"] = defects.Count.ToString(CultureInfo.InvariantCulture),
                    ["quality_threshold"] = threshold.ToString("0.###", CultureInfo.InvariantCulture),
                    ["score"] = score.ToString("0.###", CultureInfo.InvariantCulture),
                    ["comparison_measurement_present"] = comparisonMeasurementPresent,
                    ["comparison_measurement_source"] = comparisonMeasurement.SourceKey,
                    ["comparison_measurement_status"] = comparisonMeasurement.IsPresent
                        ? "present"
                        : ComparisonMeasurementMissingStatus
                },
                DesignConversionVisualAiProviderSelectionPolicy.DescribeLocalStub(
                    ProviderName,
                    ExternalProviderNotUsedMarker)))));
    }

    private static IReadOnlyList<VisualDefect> BuildDefects(
        DesignConversionVisualJudgeRequest request,
        double threshold,
        ComparisonMeasurement comparisonMeasurement)
    {
        var defects = new List<VisualDefect>();
        if (request.ExpectedScreenshot.Resolution.Width != request.RenderedImage.Resolution.Width ||
            request.ExpectedScreenshot.Resolution.Height != request.RenderedImage.Resolution.Height)
        {
            defects.Add(CreateDefect(
                request,
                "viewport_resolution_mismatch",
                "major",
                $"Expected {request.ExpectedScreenshot.Resolution.Width}x{request.ExpectedScreenshot.Resolution.Height}, rendered {request.RenderedImage.Resolution.Width}x{request.RenderedImage.Resolution.Height}."));
        }

        if (Math.Abs(request.ExpectedScreenshot.Resolution.DeviceScaleFactor - request.RenderedImage.Resolution.DeviceScaleFactor) > 0.001)
        {
            defects.Add(CreateDefect(
                request,
                "device_scale_factor_mismatch",
                "major",
                $"Expected device scale {request.ExpectedScreenshot.Resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)}, rendered {request.RenderedImage.Resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)}."));
        }

        if (string.IsNullOrWhiteSpace(request.ExpectedScreenshot.Sha256) || string.IsNullOrWhiteSpace(request.RenderedImage.Sha256))
        {
            defects.Add(CreateDefect(
                request,
                "image_hash_metadata_missing",
                "major",
                "Expected and rendered image metadata must include stable sha256 references for local judging."));
        }

        if (string.IsNullOrWhiteSpace(request.DiffArtifactUri) || string.IsNullOrWhiteSpace(request.DiffArtifactSha256))
        {
            defects.Add(CreateDefect(
                request,
                "diff_artifact_missing",
                "minor",
                "Diff artifact reference or sha256 is missing; local judge can only score metadata."));
        }

        var comparisonPassed = ReadBool(request.Metadata, "comparison_verdict_passed");
        if (!comparisonMeasurement.IsPresent)
        {
            defects.Add(CreateDefect(
                request,
                ComparisonMeasurementMissingCategory,
                "blocker",
                "No valid local comparison measurement was supplied; expected comparison_verdict_score, comparison_score, or visual_distance."));
        }
        else if (comparisonMeasurement.Score < threshold || comparisonPassed == false)
        {
            var categories = ReadCategories(request.Metadata, "comparison_defect_categories");
            if (categories.Count == 0)
                categories = ["visual_difference"];

            foreach (var category in categories)
            {
                defects.Add(CreateDefect(
                    request,
                    $"comparison_{category}",
                    comparisonMeasurement.Score < 0.65 ? "major" : "minor",
                    $"Comparison metadata reported category '{category}' below the local quality threshold."));
            }
        }

        return defects
            .GroupBy(defect => defect.Category, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
    }

    private static double ResolveScore(
        ComparisonMeasurement comparisonMeasurement,
        IReadOnlyList<VisualDefect> defects)
    {
        var score = comparisonMeasurement.IsPresent
            ? comparisonMeasurement.Score
            : 0.0;

        foreach (var defect in defects)
        {
            score = defect.Category switch
            {
                "viewport_resolution_mismatch" => Math.Min(score, 0.50),
                "device_scale_factor_mismatch" => Math.Min(score, 0.70),
                "image_hash_metadata_missing" => Math.Min(score, 0.80),
                "diff_artifact_missing" => Math.Min(score, 0.92),
                _ when defect.Category.StartsWith("comparison_", StringComparison.Ordinal) => score,
                _ => Math.Min(score, 0.85)
            };
        }

        return Clamp01(score);
    }

    private static double ResolveQualityThreshold(IReadOnlyDictionary<string, string> metadata)
        => ReadDouble(metadata, "quality_threshold") is { } threshold
            ? Clamp01(threshold)
            : DefaultQualityThreshold;

    private static ComparisonMeasurement ResolveComparisonMeasurement(IReadOnlyDictionary<string, string> metadata)
    {
        if (TryReadFiniteDouble(metadata, "comparison_verdict_score", out var verdictScore))
            return new ComparisonMeasurement(true, verdictScore, "comparison_verdict_score");

        if (TryReadFiniteDouble(metadata, "comparison_score", out var comparisonScore))
            return new ComparisonMeasurement(true, comparisonScore, "comparison_score");

        if (TryReadFiniteDouble(metadata, "visual_distance", out var visualDistance))
            return new ComparisonMeasurement(true, Clamp01(1.0 - visualDistance), "visual_distance");

        return new ComparisonMeasurement(false, 0.0, "none");
    }

    private static VisualDefect CreateDefect(
        DesignConversionVisualJudgeRequest request,
        string category,
        string severity,
        string description)
        => new(
            DefectId: $"{request.JudgeRequestId}:local-defect:{category}",
            Category: category,
            Severity: severity,
            Description: description,
            X: 0,
            Y: 0,
            Width: 1,
            Height: 1,
            EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId]);

    private static IReadOnlyList<string> ReadCategories(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value)
            ? value
                .Split(['|', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(NormalizeCategory)
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Distinct(StringComparer.Ordinal)
                .ToArray()
            : [];

    private static string NormalizeCategory(string category)
        => new(category
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
            .ToArray());

    private static bool IsMajorOrBlocker(string severity)
        => string.Equals(severity, "major", StringComparison.OrdinalIgnoreCase)
           || string.Equals(severity, "blocker", StringComparison.OrdinalIgnoreCase);

    private static bool? ReadBool(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed)
            ? parsed
            : null;

    private static double? ReadDouble(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static bool TryReadFiniteDouble(IReadOnlyDictionary<string, string> metadata, string key, out double parsed)
    {
        if (metadata.TryGetValue(key, out var value) &&
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) &&
            !double.IsNaN(parsed) &&
            !double.IsInfinity(parsed))
        {
            return true;
        }

        parsed = 0.0;
        return false;
    }

    private static double Clamp01(double value)
        => Math.Min(1.0, Math.Max(0.0, value));

    private sealed record ComparisonMeasurement(bool IsPresent, double Score, string SourceKey);
}

public sealed class NoConfiguredVisualJudge : IDesignConversionVisualJudge
{
    public const string Status = DesignConversionVisualQaBoundaryPolicy.ProviderRequired;
    public const string ReasonCode = DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker;
    public const string LegacyStatus = "BLOCKED_NO_CONFIGURED_VISUAL_JUDGE";
    public const string EvidenceMarker = "external_ai_visual_judge_executed:false";

    public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
        DesignConversionVisualJudgeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Fail(
                "DesignConversion visual judge cancelled.",
                "CANCELLED"));

        var validation = DesignConversionVisualJudgeValidation.Validate(request);
        if (!validation.IsSuccess)
            return Task.FromResult(validation.MapError<DesignConversionVisualJudgeResult>());

        return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(CreateBlockedResult(
            request,
            "none",
            "none",
            "none",
            "No external visual judge provider/key is configured.")));
    }

    internal static DesignConversionVisualJudgeResult CreateBlockedResult(
        DesignConversionVisualJudgeRequest request,
        string providerName,
        string endpointHost,
        string modelId,
        string reason)
    {
        var verdict = new VisualVerdict(
            VerdictId: $"{request.JudgeRequestId}:no-configured-visual-judge",
            Score: 0.0,
            Defects:
            [
                new VisualDefect(
                    DefectId: $"{request.JudgeRequestId}:visual-judge-blocked",
                    Category: "external_visual_judge",
                    Severity: "blocker",
                    Description: reason,
                    X: 0,
                    Y: 0,
                    Width: 1,
                    Height: 1,
                    EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
            ],
            EvidenceRefs:
            [
                $"visual-judge-request:{request.JudgeRequestId}",
                EvidenceMarker,
                DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderBoundaryMarker,
                DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker,
                DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker,
                ReasonCode,
                LegacyStatus,
                Status
            ],
            Passed: false);

        return new DesignConversionVisualJudgeResult(
            JudgeResultId: $"{request.JudgeRequestId}:blocked",
            JudgeRequestId: request.JudgeRequestId,
            Status: Status,
            IsBlocked: true,
            Verdict: verdict,
            Provider: new DesignConversionVisualJudgeProviderMetadata(
                ProviderName: providerName,
                EndpointHost: endpointHost,
                ModelId: modelId,
                ExternalCallExecuted: false,
                Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(
                    new Dictionary<string, string>
                    {
                        ["configured"] = bool.FalseString,
                        ["blocker"] = LegacyStatus,
                        ["provider_boundary"] = Status,
                        ["provider_required_reason"] = ReasonCode
                    },
                    DesignConversionVisualAiProviderSelectionPolicy.DescribeNotConfigured(
                        providerName,
                        ReasonCode))),
            EstimatedCostUsd: null,
            TokenMetadata: new Dictionary<string, double>(),
            EvidenceRefs: verdict.EvidenceRefs,
            Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(
                new Dictionary<string, string>
                {
                    ["status"] = Status,
                    ["external_ai_visual_judge_executed"] = bool.FalseString,
                    ["visual_ai_executed"] = bool.FalseString,
                    ["provider_boundary"] = Status,
                    ["provider_required_reason"] = ReasonCode,
                    ["legacy_blocker"] = LegacyStatus,
                    ["reason"] = reason
                },
                DesignConversionVisualAiProviderSelectionPolicy.DescribeNotConfigured(
                    providerName,
                    ReasonCode)));
    }
}

public sealed class ExternalHttpDesignConversionVisualJudge : IDesignConversionVisualJudge
{
    public const string StatusOk = "EXTERNAL_VISUAL_JUDGE_VERDICT_RECEIVED";
    public const string TestStatusOk = "TEST_VISUAL_JUDGE_VERDICT_RECEIVED";
    public const string TestProviderNoNetworkStatus = "TEST_VISUAL_JUDGE_NO_NETWORK_BLOCKED";
    public const string TestProviderNoNetworkReason = "test_provider_only_no_network";
    public const string TestProviderNoNetworkEvidenceMarker = "test_provider_only_no_network:true";
    public const string PayloadMode = "bounded_inline_png_base64_no_local_paths";
    public const string RedactionPolicy = "refs_hashes_prompt_metadata_only";
    public const string RequestPayloadSha256HeaderName = "X-Design-Conversion-Payload-Sha256";
    public const string ServerCallReceiptKey = "server_visual_provider_call_receipt";
    public const string ServerCallReceiptVersionKey = "server_visual_provider_call_receipt_version";
    public const string ServerCallReceiptIdKey = "server_visual_provider_call_receipt_id";
    public const string ServerCallReceiptJobIdKey = "server_visual_provider_call_job_id";
    public const string ServerCallReceiptJudgeRequestIdKey = "server_visual_provider_call_judge_request_id";
    public const string ServerCallReceiptRequestIdKey = "server_visual_provider_call_request_id";
    public const string ServerCallReceiptRootRequestIdKey = "server_visual_provider_call_root_request_id";
    public const string ServerCallReceiptProviderRequestIdKey = "server_visual_provider_call_provider_request_id";
    public const string ServerCallReceiptComparisonRequestIdKey = "server_visual_provider_call_comparison_request_id";
    public const string ServerCallReceiptVisualJudgeRequestIdKey = "server_visual_provider_call_visual_judge_request_id";
    public const string ServerCallReceiptVisualResultIdKey = "server_visual_provider_call_visual_result_id";
    public const string ServerCallReceiptComparisonArtifactIdKey = "server_visual_provider_call_comparison_artifact_id";
    public const string ServerCallReceiptProviderNameKey = "server_visual_provider_call_provider_name";
    public const string ServerCallReceiptProviderModelKey = "server_visual_provider_call_provider_model";
    public const string ServerCallReceiptEndpointHostKey = "server_visual_provider_call_endpoint_host";
    public const string ServerCallReceiptExternalCallExecutedKey = "server_visual_provider_call_external_call_executed";
    public const string ServerCallReceiptResponseIdentityVerifiedKey = "server_visual_provider_call_response_identity_verified";
    public const string ServerCallReceiptResponseSchemaVerifiedKey = "server_visual_provider_call_response_schema_verified";
    public const string ServerCallReceiptRequestPayloadSha256Key = "server_visual_provider_call_request_payload_sha256";
    public const string ServerCallReceiptVersion = "design_conversion_visual_provider_call_receipt_v1";
    public const string ServerOwnedVisualResultSegment = "server-owned-visual-result";
    public const string StatusHttpBlocked = "EXTERNAL_VISUAL_JUDGE_HTTP_BLOCKED";
    public const string ServerOwnedAttemptCountKey = "server_owned_attempt_count";
    public const string ServerOwnedAttemptCountSourceKey = "server_owned_attempt_count_source";
    public const string ServerOwnedAttemptCountSourceValue = "server_http_client_send_async";
    public const string ServerOwnedCostPerAttemptUsdKey = "server_owned_cost_per_attempt_usd";
    public const string ServerOwnedTotalCostUsdKey = "server_owned_total_cost_usd";
    public const string ServerOwnedCostAccountingKey = "server_owned_cost_accounting";
    public const string ServerOwnedCostAccountingFixedPerAttempt = "fixed_per_request_times_actual_http_attempts";
    public const string ServerOwnedCostAccountingUnknown = "unknown_unpriced";
    public const string TerminalHttpStatusCodeKey = "terminal_http_status_code";
    public const string TerminalHttpStatusMarker = "external_visual_judge_terminal_http_status";
    public const string StatusTransportBlocked = "EXTERNAL_VISUAL_JUDGE_TRANSPORT_BLOCKED";
    public const string TransportFailureMarker = "external_visual_judge_transport_failure";
    public const string TransportFailureCodeKey = "transport_failure_code";
    public const string TransportFailureRequestTimeoutCode = "request_timeout";
    public const string TransportFailureHttpRequestFailedCode = "http_request_failed";
    public const string NetworkCallAttemptedKey = "network_call_attempted";
    public const string NetworkCallExecutionVerifiedKey = "network_call_execution_verified";
    public const string ProviderResponseReceivedKey = "provider_response_received";
    public const string TransportFailureEndpointHostPlaceholder = "redacted_transport_failure";
    public const string StatusResponseReadBlocked = "EXTERNAL_VISUAL_JUDGE_RESPONSE_READ_BLOCKED";
    public const string ResponseReadFailureMarker = "external_visual_judge_response_read_failure";
    public const string ResponseReadFailureCodeKey = "response_read_failure_code";
    public const string ResponseReadFailureTimeoutCode = "response_body_timeout";
    public const string ResponseReadFailureExceptionCode = "response_body_read_failed";
    public const string StatusResponsePolicyBlocked = "EXTERNAL_VISUAL_JUDGE_RESPONSE_POLICY_BLOCKED";
    public const string ResponsePolicyBlockMarker = "external_visual_judge_response_policy_block";
    public const string ResponsePolicyCodeKey = "response_policy_code";
    public const string ResponsePolicyResponseTooLargeCode = "response_body_too_large";
    public const string StatusResponseValidationBlocked = "EXTERNAL_VISUAL_JUDGE_RESPONSE_VALIDATION_BLOCKED";
    public const string ResponseValidationFailureMarker = "external_visual_judge_response_validation_failure";
    public const string ResponseValidationCodeKey = "response_validation_code";
    public const string ResponseValidationInvalidJsonCode = "invalid_json";
    public const string ResponseValidationIdentityMismatchCode = "identity_mismatch";
    public const string ResponseValidationProviderMismatchCode = "provider_mismatch";
    public const string ResponseValidationModelMismatchCode = "model_mismatch";
    public const string ResponseValidationSchemaInvalidCode = "schema_invalid";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan TransientRetryFallbackDelay = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan MaxRetryAfterDelay = TimeSpan.FromSeconds(2);
    private const int MaxTransientRetries = 1;
    public const int MaxServerOwnedHttpAttemptCount = MaxTransientRetries + 1;
    private const int MaxImageWidth = 8192;
    private const int MaxImageHeight = 8192;
    private const long MaxImagePixels = 16_777_216;
    private const long MaxImageByteLength = 25L * 1024L * 1024L;
    public const long MaxInlinePngImageBytes = MaxImageByteLength;
    public const long MaxInlinePngTotalBytes = checked(MaxInlinePngImageBytes * 2L);
    public const long MaxRequestMetadataEnvelopeBytes = 1L * 1024L * 1024L;
    private const long MaxInlinePngBase64TextBytes = checked(((MaxInlinePngTotalBytes + 2L) / 3L) * 4L);
    private const long MaxRequestBodyBytes = checked(MaxInlinePngBase64TextBytes + MaxRequestMetadataEnvelopeBytes);
    private const long MaxResponseBodyBytes = 256L * 1024L;
    private const int MaxSanitizedMetadataEntries = 64;
    private const int MaxSanitizedMetadataKeyLength = 96;
    private const int MaxSanitizedMetadataValueLength = 512;
    private const string RedactedMetadataValue = "[redacted]";

    private static readonly Regex JwtLikeValuePattern = new(
        @"\b[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{10,}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex CommonCredentialPrefixPattern = new(
        @"(?:^|[^A-Za-z0-9])(?:sk-[A-Za-z0-9_-]{20,}|gh[pousr]_[A-Za-z0-9_]{20,}|github_pat_[A-Za-z0-9_]{20,}|xox[baprs]-[A-Za-z0-9-]{16,}|glpat-[A-Za-z0-9_-]{16,}|AIza[0-9A-Za-z_-]{20,}|AKIA[0-9A-Z]{16})(?:$|[^A-Za-z0-9])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex MarkupElementPattern = new(
        @"<\s*(?<tag>[A-Za-z][A-Za-z0-9:_-]{0,63})(?:\s+[^<>]{0,256})?\s*>[\s\S]{0,512}</\s*\k<tag>\s*>|<\s*[A-Za-z][A-Za-z0-9:_-]{0,63}(?:\s+[^<>]{0,256})?\s*/\s*>|<!DOCTYPE\s+[^<>]{1,256}>|<\?xml\s+[^<>]{1,256}\?>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex CssRulePayloadPattern = new(
        @"\A\s*(?:[A-Za-z][A-Za-z0-9_-]{0,63}|[.#:][A-Za-z_-][A-Za-z0-9_-]{0,63})[^{\r\n]{0,128}\{\s*(?:--)?[A-Za-z_-][A-Za-z0-9_-]{0,63}\s*:\s*[^{}]{1,512}\}\s*\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex CssColorTokenValuePattern = new(
        @"\A#(?:[0-9A-Fa-f]{3}|[0-9A-Fa-f]{4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex CssLengthTokenValuePattern = new(
        @"\A(?:0|(?:\d{1,4}(?:\.\d{1,4})?|\.\d{1,4})(?:px|rem|em|%))\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private enum MetadataKeyDisposition
    {
        Production = 0,
        Reference = 1,
        DesignToken = 2,
        ComparisonMetric = 3,
        Sensitive = 4,
        Unknown = 5
    }

    private enum DesignTokenCategory
    {
        None = 0,
        Color = 1,
        Spacing = 2,
        Radius = 3
    }

    private readonly DesignConversionExternalVisualJudgeOptions _options;
    private readonly HttpClient _httpClient;
    private readonly IDesignConversionVisualJudgeImageContentResolver _imageContentResolver;
    private readonly Func<TimeSpan, CancellationToken, Task> _transientRetryDelayAsync;

    public ExternalHttpDesignConversionVisualJudge(
        DesignConversionExternalVisualJudgeOptions options,
        HttpClient httpClient,
        IDesignConversionVisualJudgeImageContentResolver? imageContentResolver = null)
        : this(
            options,
            httpClient,
            static (delay, cancellationToken) => Task.Delay(delay, cancellationToken),
            imageContentResolver)
    {
    }

    internal ExternalHttpDesignConversionVisualJudge(
        DesignConversionExternalVisualJudgeOptions options,
        HttpClient httpClient,
        Func<TimeSpan, CancellationToken, Task> transientRetryDelayAsync,
        IDesignConversionVisualJudgeImageContentResolver? imageContentResolver = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _imageContentResolver = imageContentResolver ?? new NoConfiguredDesignConversionVisualJudgeImageContentResolver();
        _transientRetryDelayAsync = transientRetryDelayAsync ?? throw new ArgumentNullException(nameof(transientRetryDelayAsync));
    }

    public async Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
        DesignConversionVisualJudgeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionVisualJudgeResult>.Fail(
                "DesignConversion visual judge cancelled.",
                "CANCELLED");

        var validation = DesignConversionVisualJudgeValidation.Validate(request);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionVisualJudgeResult>();

        if (!IsConfigured(_options))
        {
            return OperationResult<DesignConversionVisualJudgeResult>.Ok(NoConfiguredVisualJudge.CreateBlockedResult(
                request,
                string.IsNullOrWhiteSpace(_options.ProviderName) ? "unconfigured" : _options.ProviderName,
                _options.Endpoint?.Host ?? "unconfigured",
                string.IsNullOrWhiteSpace(_options.ModelId) ? "unconfigured" : _options.ModelId,
                "External visual judge endpoint, provider, model, or key is not configured."));
        }

        if (IsTestProviderOnly(_options))
            return OperationResult<DesignConversionVisualJudgeResult>.Ok(CreateTestProviderOnlyNoNetworkResult(request));

        var endpointValidation = ValidateEndpoint(_options.Endpoint!);
        if (!endpointValidation.IsSuccess)
            return endpointValidation.MapError<DesignConversionVisualJudgeResult>();

        var imageContent = await ResolveRequestImageContentAsync(request, cancellationToken);
        if (!imageContent.IsSuccess)
            return imageContent.MapError<DesignConversionVisualJudgeResult>();

        var payloadJson = JsonSerializer.Serialize(BuildPayload(request, imageContent.Value!), JsonOptions);
        if (Encoding.UTF8.GetByteCount(payloadJson) > MaxRequestBodyBytes)
        {
            return OperationResult<DesignConversionVisualJudgeResult>.Fail(
                "External visual judge request exceeded the bounded JSON payload size.",
                "EXTERNAL_VISUAL_JUDGE_REQUEST_TOO_LARGE");
        }

        var payloadSha256 = Sha256Text(payloadJson);
        var responseResult = await SendWithRetryAsync(request.JudgeRequestId, payloadJson, payloadSha256, cancellationToken);
        if (!responseResult.IsSuccess)
            return responseResult.MapError<DesignConversionVisualJudgeResult>();

        var responseOutcome = responseResult.Value!;
        if (responseOutcome.TransportFailureCode is { } transportFailureCode)
        {
            return OperationResult<DesignConversionVisualJudgeResult>.Ok(CreateTransportFailureBlockedResult(
                request,
                transportFailureCode,
                responseOutcome.AttemptCount,
                payloadSha256));
        }

        using var response = responseOutcome.Response!;
        if (!response.IsSuccessStatusCode)
        {
            return OperationResult<DesignConversionVisualJudgeResult>.Ok(CreateTerminalHttpBlockedResult(
                request,
                response.StatusCode,
                responseOutcome.AttemptCount,
                payloadSha256));
        }

        var responseJson = await ReadBoundedResponseStringAsync(
            response.Content,
            ResolveResponseBodyReadTimeout(_httpClient.Timeout),
            cancellationToken);
        if (!responseJson.IsSuccess)
        {
            if (string.Equals(
                    responseJson.ErrorCode,
                    "EXTERNAL_VISUAL_JUDGE_RESPONSE_TOO_LARGE",
                    StringComparison.Ordinal))
            {
                return OperationResult<DesignConversionVisualJudgeResult>.Ok(CreateResponsePolicyBlockedResult(
                    request,
                    ResponsePolicyResponseTooLargeCode,
                    responseOutcome.AttemptCount,
                    payloadSha256));
            }

            var responseReadFailureCode = MapResponseReadFailureCode(responseJson.ErrorCode);
            if (responseReadFailureCode is not null)
            {
                return OperationResult<DesignConversionVisualJudgeResult>.Ok(CreateResponseReadFailureBlockedResult(
                    request,
                    responseReadFailureCode,
                    responseOutcome.AttemptCount,
                    payloadSha256));
            }

            return responseJson.MapError<DesignConversionVisualJudgeResult>();
        }

        return ParseResponse(request, responseJson.Value!, responseOutcome.AttemptCount, payloadSha256);
    }

    private static bool IsConfigured(DesignConversionExternalVisualJudgeOptions options)
        => options.Endpoint is not null
           && !string.IsNullOrWhiteSpace(options.ProviderName)
           && !string.IsNullOrWhiteSpace(options.ModelId)
           && !string.IsNullOrWhiteSpace(options.ApiKey);

    private DesignConversionVisualJudgeResult CreateTestProviderOnlyNoNetworkResult(
        DesignConversionVisualJudgeRequest request)
    {
        const string reason = "Test provider metadata requests a deterministic no-network visual judge result; no endpoint, image payload, or provider response was used.";
        var evidenceRefs = new[]
        {
            $"visual-judge-request:{request.JudgeRequestId}",
            TestProviderNoNetworkStatus,
            TestProviderNoNetworkReason,
            TestProviderNoNetworkEvidenceMarker,
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderBoundaryMarker,
            DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker,
            NoConfiguredVisualJudge.EvidenceMarker,
            "network_call_executed:false",
            "external_ai_visual_judge_executed:false",
            "visual_ai_executed:false"
        };
        var verdict = new VisualVerdict(
            VerdictId: $"{request.JudgeRequestId}:test-provider-no-network",
            Score: 0.0,
            Defects:
            [
                new VisualDefect(
                    DefectId: $"{request.JudgeRequestId}:test-provider-no-network-blocker",
                    Category: TestProviderNoNetworkReason,
                    Severity: "blocker",
                    Description: reason,
                    X: 0,
                    Y: 0,
                    Width: 1,
                    Height: 1,
                    EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
            ],
            EvidenceRefs: evidenceRefs,
            Passed: false);
        var providerPolicy = DesignConversionVisualAiProviderSelectionPolicy.DescribeLocalStub(
            _options.ProviderName,
            TestProviderNoNetworkReason);
        var providerMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["configured"] = bool.TrueString,
            ["provider"] = _options.ProviderName,
            ["status"] = TestProviderNoNetworkStatus,
            ["reason"] = TestProviderNoNetworkReason,
            ["network_call_executed"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["visual_ai_executed"] = bool.FalseString,
            ["provider_response_identity_verified"] = bool.FalseString,
            ["provider_response_schema_verified"] = bool.FalseString,
            ["provider_response_verified"] = bool.FalseString,
            [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString,
            [ServerCallReceiptKey] = bool.FalseString,
            [ServerCallReceiptExternalCallExecutedKey] = bool.FalseString,
            [ServerCallReceiptResponseIdentityVerifiedKey] = bool.FalseString,
            [ServerCallReceiptResponseSchemaVerifiedKey] = bool.FalseString
        };
        var resultMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["status"] = TestProviderNoNetworkStatus,
            ["reason"] = TestProviderNoNetworkReason,
            ["image_payload_mode"] = "not_constructed_test_provider_only",
            ["redaction"] = RedactionPolicy,
            ["network_call_executed"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["visual_ai_executed"] = bool.FalseString,
            ["provider_response_identity_verified"] = bool.FalseString,
            ["provider_response_schema_verified"] = bool.FalseString,
            ["provider_response_verified"] = bool.FalseString,
            [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString,
            [ServerCallReceiptKey] = bool.FalseString,
            [ServerCallReceiptExternalCallExecutedKey] = bool.FalseString,
            [ServerCallReceiptResponseIdentityVerifiedKey] = bool.FalseString,
            [ServerCallReceiptResponseSchemaVerifiedKey] = bool.FalseString
        };

        return new DesignConversionVisualJudgeResult(
            JudgeResultId: $"{request.JudgeRequestId}:test-provider-no-network-blocked",
            JudgeRequestId: request.JudgeRequestId,
            Status: TestProviderNoNetworkStatus,
            IsBlocked: true,
            Verdict: verdict,
            Provider: new DesignConversionVisualJudgeProviderMetadata(
                ProviderName: _options.ProviderName,
                EndpointHost: "not_called",
                ModelId: _options.ModelId,
                ExternalCallExecuted: false,
                Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(providerMetadata, providerPolicy)),
            EstimatedCostUsd: null,
            TokenMetadata: new Dictionary<string, double>(),
            EvidenceRefs: evidenceRefs,
            Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(resultMetadata, providerPolicy));
    }

    private static OperationResult<bool> ValidateEndpoint(Uri endpoint)
        => DesignConversionExternalVisualProviderConfigIntake.ValidateEndpoint(endpoint);

    private object BuildPayload(
        DesignConversionVisualJudgeRequest request,
        ResolvedVisualJudgeImageSet imageContent)
        => new
        {
            judge_request_id = request.JudgeRequestId,
            request_id = request.RequestId,
            provider = _options.ProviderName,
            model = _options.ModelId,
            quality_threshold = ResolveQualityThreshold(request.Metadata),
            provider_boundary = IsTestProviderOnly(_options)
                ? DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker
                : "external_provider_configured",
            image_payload_mode = PayloadMode,
            image_payload_limits = new
            {
                per_image_max_bytes = MaxInlinePngImageBytes,
                total_max_bytes = MaxInlinePngTotalBytes,
                metadata_envelope_max_bytes = MaxRequestMetadataEnvelopeBytes,
                request_body_max_bytes = MaxRequestBodyBytes,
                encoding = "base64"
            },
            expected_screenshot = ToSafeImage(request.ExpectedScreenshot, imageContent.Expected),
            rendered_image = ToSafeImage(request.RenderedImage, imageContent.Rendered),
            diff_artifact = new
            {
                uri = ToSafeArtifactRef(request.DiffArtifactUri, "diff-artifact"),
                sha256 = request.DiffArtifactSha256 ?? string.Empty
            },
            metadata = SanitizeMetadata(request.Metadata),
            provider_metadata = SanitizeMetadata(_options.Metadata),
            redaction = new
            {
                marker = DesignConversionVisualQaBoundaryPolicy.RedactionMarker,
                policy = RedactionPolicy,
                raw_html_css_tree_private_fields = "redacted",
                local_paths = "artifact_refs_only"
            },
            response_schema = new
            {
                judge_request_id = request.JudgeRequestId,
                request_id = request.RequestId,
                score = "0..1",
                quality_threshold = "0..1 server acceptance threshold; provider passed is diagnostic only",
                passed = "required boolean diagnostic; server recomputes final Passed from score and quality_threshold",
                defects = "array",
                issues = "array alias for defects",
                confidence = "optional 0..1",
                evidence_refs = "array",
                cost_estimate_usd = "ignored if supplied; server-owned fixed per-request pricing is authoritative",
                estimated_cost_usd = "ignored alias if supplied",
                tokens = "optional token metadata"
            }
        };

    private static double ResolveQualityThreshold(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is not null &&
            metadata.TryGetValue("quality_threshold", out var configured) &&
            double.TryParse(configured, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) &&
            !double.IsNaN(parsed) &&
            !double.IsInfinity(parsed))
        {
            return Clamp01(parsed);
        }

        return 0.85;
    }

    private static object ToSafeImage(
        RenderQaImageRecord image,
        DesignConversionVisualJudgeImageContent content)
        => new
        {
            image_id = image.ImageId,
            label = image.Label,
            resolution = new
            {
                id = image.Resolution.ResolutionId,
                width = image.Resolution.Width,
                height = image.Resolution.Height,
                device_scale_factor = image.Resolution.DeviceScaleFactor
            },
            uri = ToSafeArtifactRef(image.Uri, image.ImageId),
            sha256 = image.Sha256,
            content = new
            {
                media_type = content.MediaType,
                encoding = "base64",
                sha256 = $"sha256:{content.Sha256}",
                byte_length = content.Bytes.LongLength,
                base64 = Convert.ToBase64String(content.Bytes)
            },
            metadata = SanitizeMetadata(image.Metadata)
        };

    private static IReadOnlyDictionary<string, string> SanitizeMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        var sanitized = new Dictionary<string, string>(StringComparer.Ordinal);
        var candidates = (metadata ?? new Dictionary<string, string>())
            .Select(pair => new KeyValuePair<string, string>(NormalizeMetadataKey(pair.Key), pair.Value))
            .Where(pair => IsSafeMetadataKeyShape(pair.Key))
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(group => new
            {
                Key = group.Key,
                Values = group.Select(pair => pair.Value).ToArray(),
                Disposition = ClassifyMetadataKey(group.Key)
            })
            .Where(candidate => candidate.Disposition != MetadataKeyDisposition.Unknown)
            .OrderBy(candidate => (int)candidate.Disposition)
            .ThenBy(candidate => candidate.Key, StringComparer.Ordinal);

        foreach (var candidate in candidates)
        {
            if (sanitized.Count >= MaxSanitizedMetadataEntries)
                break;

            sanitized[candidate.Key] = candidate.Values.Length == 1
                ? SanitizeMetadataValue(candidate.Disposition, candidate.Key, candidate.Values[0])
                : RedactedMetadataValue;
        }

        return sanitized;
    }

    private static string NormalizeMetadataKey(string key)
        => (key ?? string.Empty)
            .Trim()
            .Replace("-", "_", StringComparison.Ordinal)
            .ToLowerInvariant();

    private static bool IsSafeMetadataKeyShape(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > MaxSanitizedMetadataKeyLength)
            return false;

        return key.All(ch =>
            IsAsciiLetterOrDigit(ch) ||
            ch is '_' or '.');
    }

    private static bool IsAsciiLetterOrDigit(char ch)
        => (ch >= 'a' && ch <= 'z') ||
           (ch >= 'A' && ch <= 'Z') ||
           (ch >= '0' && ch <= '9');

    private static MetadataKeyDisposition ClassifyMetadataKey(string normalized)
    {
        if (IsSensitiveKey(normalized))
            return MetadataKeyDisposition.Sensitive;
        if (IsSafeReferenceMetadataKey(normalized))
            return MetadataKeyDisposition.Reference;
        if (IsDesignTokenMetadataKey(normalized))
            return MetadataKeyDisposition.DesignToken;
        if (IsKnownProductionMetadataKey(normalized))
            return MetadataKeyDisposition.Production;
        if (IsComparisonMetricMetadataKey(normalized))
            return MetadataKeyDisposition.ComparisonMetric;

        return MetadataKeyDisposition.Unknown;
    }

    private static string SanitizeMetadataValue(
        MetadataKeyDisposition disposition,
        string key,
        string? value)
    {
        if (disposition == MetadataKeyDisposition.Sensitive)
            return RedactedMetadataValue;
        if (disposition == MetadataKeyDisposition.Reference)
            return SanitizeReferenceMetadataValue(key, value);
        if (MetadataValueRequiresRedaction(value))
            return RedactedMetadataValue;
        if (disposition == MetadataKeyDisposition.ComparisonMetric &&
            !IsFiniteInvariantDouble(value))
        {
            return RedactedMetadataValue;
        }
        if (disposition == MetadataKeyDisposition.DesignToken &&
            !IsValidDesignTokenValue(key, value))
        {
            return RedactedMetadataValue;
        }

        return ToBoundedSafeMetadataValue(value);
    }

    private static bool IsSensitiveKey(string normalized)
        => normalized is "secret"
            or "token"
            or "auth_token"
            or "access_token"
            or "refresh_token"
            or "api_key"
            or "apikey"
            or "password"
            or "authorization"
            or "private_key"
            or "client_secret"
            or "user_id"
            or "userid"
            or "source_path"
            or "local_path"
            or "browser_screenshot_path"
            || IsRawDesignContentKey(normalized);

    private static bool IsKnownProductionMetadataKey(string normalized)
        => normalized is
            "actual_png_bytes_written" or
            "ai_visual_judge_claimed" or
            "api_key_header_name" or
            "api_key_scheme" or
            "artifact_file_name" or
            "artifact_path_ref" or
            "browser_or_playwright_used" or
            "browser_screenshot_distinct_color_count" or
            "browser_screenshot_non_white_pixel_count" or
            "candidate_providers" or
            "comparison" or
            "comparison_artifact_id" or
            "comparison_boundary" or
            "comparison_defect_categories" or
            "comparison_defect_count" or
            "comparison_diff_readback_sha256_verified" or
            "comparison_index" or
            "comparison_metric_count" or
            "comparison_proof_status" or
            "comparison_provider_kind" or
            "comparison_request_id" or
            "comparison_verdict_passed" or
            "comparison_verdict_score" or
            "config_descriptor_status" or
            "config_intake_source" or
            "content_type" or
            "cost_source" or
            "css_sha256" or
            "device_scale_factor" or
            "diff_image_sha256" or
            "diff_png_nonblank_pixels_verified" or
            "diff_png_signature_verified" or
            "diff_sha256_verified" or
            "dpo_eligible" or
            "endpoint_host" or
            "expected_png_height" or
            "expected_png_width" or
            "expected_sha256_verified" or
            "external_ai_used" or
            "external_ai_visual_judge_executed" or
            "external_visual_provider_api_key_configured" or
            "external_visual_provider_api_key_value_included" or
            "external_visual_provider_config_complete" or
            "external_visual_provider_config_intake" or
            "external_visual_provider_factory_ready" or
            "external_visual_provider_image_artifact_root_setting_name" or
            "external_visual_provider_image_artifact_root_value_included" or
            "external_visual_provider_image_content_resolver_configured" or
            "external_visual_provider_image_payload_mode" or
            "external_visual_provider_api_key_header_name" or
            "external_visual_provider_api_key_scheme" or
            "external_visual_provider_executed" or
            "hidden_determinism_boundary" or
            "html_sha256" or
            "input_metadata_redaction_count" or
            "invalid_setting_names" or
            "job_id" or
            "local_png_pixel_comparison" or
            "media_type" or
            "metadata_only_comparison_rejected" or
            "missing_setting_names" or
            "model_training_verified" or
            "model_weights_published" or
            "network_call_executed" or
            "pixel_difference_ratio" or
            "png_height" or
            "png_width" or
            "primary_api_key_setting" or
            "primary_endpoint_setting" or
            "primary_model_setting" or
            "primary_provider_setting" or
            "product_ready" or
            "provider_kind" or
            "provider_model" or
            "provider_name" or
            "provider_payload_included" or
            "qa_data_role" or
            "quality_threshold" or
            "real_external_provider_executable_when_network_authorized" or
            "real_external_provider_executed" or
            "redaction_count" or
            "render_artifact_kind" or
            "render_index" or
            "render_result_id" or
            "rendered_png_height" or
            "rendered_png_width" or
            "rendered_sha256_verified" or
            "refs_or_hashes_only" or
            "server_canonicalized" or
            "server_owned_comparison_trust_boundary" or
            "server_owned_comparison_trust_classification" or
            "server_owned_local_png_comparison_trust_verified" or
            "server_owned_pricing_configured" or
            "server_owned_pricing_cost_source" or
            "server_owned_pricing_raw_value_included" or
            "server_owned_pricing_setting_name" or
            "server_owned_pricing_status" or
            "server_owned_pricing_valid" or
            "source_artifact_id" or
            "source_artifact_kind" or
            "source_artifact_ref" or
            "status" or
            "training_authority_verified" or
            "visual_ai_candidate_providers" or
            "visual_ai_provider_fallback_reason" or
            "visual_ai_provider_fallback_status" or
            "visual_ai_provider_mode" or
            "visual_ai_provider_selection_policy" or
            "visual_ai_real_cloud_provider_claimed" or
            "visual_ai_requested_provider" or
            "visual_ai_requested_provider_name" or
            "visual_ai_selected_provider" or
            "visual_ai_selected_provider_name" or
            "visual_ai_selected_provider_status" or
            "visual_ai_unavailable_provider_count" or
            "visual_ai_unavailable_providers" or
            "visual_comparison_proven" or
            "visual_distance" or
            "visual_fidelity_claimed" or
            "visual_judge" or
            "visual_judge_index" or
            "viewport_height" or
            "viewport_id" or
            "viewport_width";

    private static bool IsDesignTokenMetadataKey(string normalized)
        => TryResolveDesignTokenCategory(normalized, out _);

    private static bool TryResolveDesignTokenCategory(
        string normalized,
        out DesignTokenCategory category)
    {
        category = DesignTokenCategory.None;
        const string prefix = "design_token_";
        if (!normalized.StartsWith(prefix, StringComparison.Ordinal) ||
            normalized.Length <= prefix.Length ||
            normalized.Length > MaxSanitizedMetadataKeyLength)
        {
            return false;
        }

        var segments = normalized[prefix.Length..].Split('_', StringSplitOptions.None);
        if (segments.Length is < 1 or > 5)
            return false;

        foreach (var segment in segments)
        {
            if (segment.Length is < 1 or > 24 ||
                !segment.All(IsAsciiLetterOrDigit) ||
                IsSensitiveMetadataSegment(segment))
            {
                return false;
            }

            var segmentCategory = segment switch
            {
                "color" => DesignTokenCategory.Color,
                "spacing" => DesignTokenCategory.Spacing,
                "radius" => DesignTokenCategory.Radius,
                _ => DesignTokenCategory.None
            };
            if (segmentCategory == DesignTokenCategory.None)
                continue;
            if (category != DesignTokenCategory.None && category != segmentCategory)
                return false;

            category = segmentCategory;
        }

        return category != DesignTokenCategory.None;
    }

    private static bool IsValidDesignTokenValue(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !TryResolveDesignTokenCategory(key, out var category))
        {
            return false;
        }

        var text = value.Trim();
        return category switch
        {
            DesignTokenCategory.Color => CssColorTokenValuePattern.IsMatch(text),
            DesignTokenCategory.Spacing or DesignTokenCategory.Radius => CssLengthTokenValuePattern.IsMatch(text),
            _ => false
        };
    }

    private static bool IsComparisonMetricMetadataKey(string normalized)
    {
        const string prefix = "comparison_metric_";
        if (!normalized.StartsWith(prefix, StringComparison.Ordinal) ||
            normalized.Length <= prefix.Length)
        {
            return false;
        }

        return normalized[prefix.Length..]
            .Split('_', StringSplitOptions.None)
            .All(segment => segment.Length is > 0 and <= 24 &&
                            segment.All(IsAsciiLetterOrDigit) &&
                            !IsSensitiveMetadataSegment(segment));
    }

    private static bool IsFiniteInvariantDouble(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           double.TryParse(
               value.Trim(),
               NumberStyles.Float,
               CultureInfo.InvariantCulture,
               out var parsed) &&
           !double.IsNaN(parsed) &&
           !double.IsInfinity(parsed);

    private static bool IsSafeReferenceMetadataKey(string normalized)
    {
        const string suffix = "_ref";
        if (!normalized.EndsWith(suffix, StringComparison.Ordinal) ||
            normalized.Length <= suffix.Length)
        {
            return false;
        }

        var segments = normalized[..^suffix.Length].Split('_', StringSplitOptions.None);
        return segments.Length is >= 1 and <= 6 &&
               segments.All(segment => segment.Length is > 0 and <= 24 &&
                                       segment.All(IsAsciiLetterOrDigit) &&
                                       !IsSensitiveMetadataSegment(segment));
    }

    private static bool IsSensitiveMetadataSegment(string segment)
        => segment is "access"
            or "api"
            or "auth"
            or "authorization"
            or "bearer"
            or "client"
            or "cookie"
            or "credential"
            or "credentials"
            or "key"
            or "password"
            or "private"
            or "refresh"
            or "secret"
            or "session"
            or "token"
            or "user"
            or "userid";

    private static string SanitizeReferenceMetadataValue(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var text = value.Trim();
        if (text.Length > MaxSanitizedMetadataValueLength ||
            LooksLikeRawDesignPayload(text) ||
            LooksLikeCredentialValue(text) ||
            LooksLikeCredentialedUri(text))
        {
            return RedactedMetadataValue;
        }

        if (LooksLikeLocalPath(text))
            return $"artifact-ref:{key}";

        return IsRecognizedSafeReference(text)
            ? text
            : RedactedMetadataValue;
    }

    private static bool IsRecognizedSafeReference(string value)
    {
        if (value.StartsWith("artifact-ref:", StringComparison.Ordinal) &&
            value.Length > "artifact-ref:".Length &&
            value["artifact-ref:".Length..].All(IsSafeReferenceCharacter))
        {
            return true;
        }

        if (value.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) &&
            value.Length == "sha256:".Length + 64 &&
            value["sha256:".Length..].All(Uri.IsHexDigit))
        {
            return true;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            !string.IsNullOrEmpty(uri.UserInfo))
        {
            return false;
        }

        if (string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return !string.IsNullOrWhiteSpace(uri.Host);

        return (string.Equals(uri.Scheme, "artifact", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(uri.Scheme, "local-png", StringComparison.OrdinalIgnoreCase)) &&
               !string.IsNullOrWhiteSpace(uri.Host) &&
               string.IsNullOrEmpty(uri.Query) &&
               string.IsNullOrEmpty(uri.Fragment);
    }

    private static bool IsSafeReferenceCharacter(char ch)
        => IsAsciiLetterOrDigit(ch) || ch is '-' or '_' or '.' or ':' or '/';

    private static bool MetadataValueRequiresRedaction(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var text = value.Trim();
        if (text.Length > MaxSanitizedMetadataValueLength)
            return true;
        if (LooksLikeLocalPath(text) ||
            LooksLikeRawDesignPayload(text) ||
            LooksLikeCredentialValue(text) ||
            LooksLikeCredentialedUri(text))
        {
            return true;
        }

        return false;
    }

    private static string ToBoundedSafeMetadataValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return ToSafeArtifactRef(value.Trim(), "metadata-value");
    }

    private static bool IsRawDesignContentKey(string normalized)
        => normalized is "html"
            or "css"
            or "raw_html"
            or "raw_css"
            or "document_html"
            or "document_css"
            or "tree"
            or "design_tree"
            or "original_tree"
            or "tree_payload"
            or "design_tree_payload"
            or "original_tree_payload"
            or "content";

    private static bool LooksLikeRawDesignPayload(string value)
    {
        var text = value.Trim();
        var lower = text.ToLowerInvariant();
        if (MarkupElementPattern.IsMatch(text))
            return true;

        if (CssRulePayloadPattern.IsMatch(text))
            return true;

        if (text.Contains('{') && text.Contains('}') && text.Contains(':') &&
            (text.Contains(';') || lower.Contains("color", StringComparison.Ordinal) ||
             lower.Contains("display", StringComparison.Ordinal) ||
             lower.Contains("private", StringComparison.Ordinal) ||
             lower.StartsWith("{", StringComparison.Ordinal) ||
             lower.StartsWith("[", StringComparison.Ordinal)))
        {
            return true;
        }

        return false;
    }

    private static bool LooksLikeCredentialValue(string value)
    {
        var lower = value.ToLowerInvariant();
        if (lower.Contains("bearer ", StringComparison.Ordinal) ||
            lower.Contains("authorization:", StringComparison.Ordinal) ||
            ContainsCredentialAssignment(lower) ||
            CommonCredentialPrefixPattern.IsMatch(value))
        {
            return true;
        }

        if (lower.Contains("-----begin", StringComparison.Ordinal) &&
            lower.Contains("private key-----", StringComparison.Ordinal))
        {
            return true;
        }

        return JwtLikeValuePattern.IsMatch(value);
    }

    private static bool ContainsCredentialAssignment(string lower)
        => ContainsAssignment(lower, "authorization") ||
           ContainsAssignment(lower, "api_key") ||
           ContainsAssignment(lower, "api-key") ||
           ContainsAssignment(lower, "apikey") ||
           ContainsAssignment(lower, "x-api-key") ||
           ContainsAssignment(lower, "access_token") ||
           ContainsAssignment(lower, "refresh_token") ||
           ContainsAssignment(lower, "client_secret") ||
           ContainsAssignment(lower, "password") ||
           ContainsAssignment(lower, "secret") ||
           ContainsAssignment(lower, "token");

    private static bool ContainsAssignment(string lower, string name)
        => lower.Contains($"{name}=", StringComparison.Ordinal) ||
           lower.Contains($"{name}:", StringComparison.Ordinal);

    private static bool LooksLikeCredentialedUri(string value)
    {
        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
            return false;

        if (!string.IsNullOrEmpty(uri.UserInfo))
            return true;

        return ContainsCredentialQueryKey(uri.Query);
    }

    private static bool ContainsCredentialQueryKey(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return false;

        var queryText = query[0] == '?' ? query[1..] : query;
        foreach (var component in queryText.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = component.IndexOf('=');
            var encodedKey = separatorIndex >= 0 ? component[..separatorIndex] : component;
            if (!TryDecodeQueryKey(encodedKey, out var decodedKey))
                return true;

            var normalizedKey = decodedKey
                .Trim()
                .Replace("-", "_", StringComparison.Ordinal)
                .ToLowerInvariant();
            if (IsCredentialQueryKey(normalizedKey))
                return true;
        }

        return false;
    }

    private static bool TryDecodeQueryKey(string encodedKey, out string decodedKey)
    {
        decodedKey = string.Empty;
        for (var index = 0; index < encodedKey.Length; index++)
        {
            if (encodedKey[index] != '%')
                continue;
            if (index + 2 >= encodedKey.Length ||
                !Uri.IsHexDigit(encodedKey[index + 1]) ||
                !Uri.IsHexDigit(encodedKey[index + 2]))
            {
                return false;
            }

            index += 2;
        }

        try
        {
            decodedKey = Uri.UnescapeDataString(encodedKey.Replace("+", " ", StringComparison.Ordinal));
            return true;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    private static bool IsCredentialQueryKey(string normalized)
        => normalized is "authorization"
            or "api_key"
            or "apikey"
            or "x_api_key"
            or "access_token"
            or "refresh_token"
            or "client_secret"
            or "password"
            or "secret"
            or "token";

    private static string ToSafeArtifactRef(string? value, string fallbackId)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (LooksLikeLocalPath(value))
            return $"artifact-ref:{fallbackId}";

        return value;
    }

    private static bool LooksLikeLocalPath(string value)
    {
        var text = value.Trim();
        if (text.Length == 0)
            return false;

        if (text.Length >= 3 && char.IsLetter(text[0]) && text[1] == ':' && (text[2] == '\\' || text[2] == '/'))
            return true;
        if (text.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
            text.StartsWith(@"\\", StringComparison.Ordinal) ||
            text.StartsWith("//", StringComparison.Ordinal) ||
            text.StartsWith("/", StringComparison.Ordinal) ||
            text is "." or ".." ||
            text.StartsWith("./", StringComparison.Ordinal) ||
            text.StartsWith(".\\", StringComparison.Ordinal) ||
            text.StartsWith("../", StringComparison.Ordinal) ||
            text.StartsWith("..\\", StringComparison.Ordinal) ||
            text.StartsWith("~/", StringComparison.Ordinal) ||
            text.StartsWith("~\\", StringComparison.Ordinal))
        {
            return true;
        }

        if (Uri.TryCreate(text, UriKind.Absolute, out var uri) && !string.Equals(uri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            return false;

        if (text.Contains('<') || text.Contains('>'))
            return false;

        return text.Contains('/', StringComparison.Ordinal) || text.Contains('\\', StringComparison.Ordinal);
    }

    private async Task<OperationResult<HttpResponseOutcome>> SendWithRetryAsync(
        string judgeRequestId,
        string payloadJson,
        string payloadSha256,
        CancellationToken cancellationToken)
    {
        var sendAttemptCount = 0;
        for (var attempt = 0; attempt <= MaxTransientRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(RequestTimeout);
                HttpResponseMessage response;
                using (var httpRequest = BuildHttpRequest(judgeRequestId, payloadJson, payloadSha256))
                {
                    sendAttemptCount++;
                    response = await _httpClient.SendAsync(
                        httpRequest,
                        HttpCompletionOption.ResponseHeadersRead,
                        timeoutCts.Token);
                }

                if (IsTransient(response.StatusCode) && attempt < MaxTransientRetries)
                {
                    var retryDelay = ResolveTransientRetryDelay(response);
                    response.Dispose();
                    await DelayBeforeTransientRetryAsync(retryDelay, cancellationToken);
                    continue;
                }

                return OperationResult<HttpResponseOutcome>.Ok(new HttpResponseOutcome(response, sendAttemptCount));
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (sendAttemptCount > 0)
                {
                    return OperationResult<HttpResponseOutcome>.Ok(new HttpResponseOutcome(
                        null,
                        sendAttemptCount,
                        TransportFailureRequestTimeoutCode));
                }

                return OperationResult<HttpResponseOutcome>.Fail(
                    "External visual judge request timed out.",
                    "EXTERNAL_VISUAL_JUDGE_TIMEOUT");
            }
            catch (HttpRequestException) when (attempt < MaxTransientRetries)
            {
                await DelayBeforeTransientRetryAsync(TransientRetryFallbackDelay, cancellationToken);
                continue;
            }
            catch (HttpRequestException)
            {
                if (sendAttemptCount > 0)
                {
                    return OperationResult<HttpResponseOutcome>.Ok(new HttpResponseOutcome(
                        null,
                        sendAttemptCount,
                        TransportFailureHttpRequestFailedCode));
                }

                return OperationResult<HttpResponseOutcome>.Fail(
                    "External visual judge request failed.",
                    "EXTERNAL_VISUAL_JUDGE_HTTP_REQUEST_FAILED");
            }
        }

        if (sendAttemptCount > 0)
        {
            return OperationResult<HttpResponseOutcome>.Ok(new HttpResponseOutcome(
                null,
                sendAttemptCount,
                TransportFailureHttpRequestFailedCode));
        }

        return OperationResult<HttpResponseOutcome>.Fail(
            "External visual judge request failed after a bounded transient retry.",
            "EXTERNAL_VISUAL_JUDGE_HTTP_REQUEST_FAILED");
    }

    private HttpRequestMessage BuildHttpRequest(
        string judgeRequestId,
        string payloadJson,
        string payloadSha256)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
        httpRequest.Headers.TryAddWithoutValidation("Idempotency-Key", judgeRequestId);
        httpRequest.Headers.TryAddWithoutValidation(RequestPayloadSha256HeaderName, $"sha256:{payloadSha256}");
        httpRequest.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
        ApplyApiKey(httpRequest);
        return httpRequest;
    }

    private static bool IsTransient(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private async Task DelayBeforeTransientRetryAsync(TimeSpan retryDelay, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (retryDelay <= TimeSpan.Zero)
            return;

        await _transientRetryDelayAsync(retryDelay, cancellationToken);
    }

    private static TimeSpan ResolveTransientRetryDelay(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.TooManyRequests)
            return TransientRetryFallbackDelay;

        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter is null)
            return TransientRetryFallbackDelay;

        TimeSpan? requestedDelay = retryAfter.Delta;
        if (!requestedDelay.HasValue && retryAfter.Date.HasValue)
            requestedDelay = retryAfter.Date.Value - DateTimeOffset.UtcNow;

        if (!requestedDelay.HasValue || requestedDelay.Value <= TimeSpan.Zero)
            return TransientRetryFallbackDelay;

        return requestedDelay.Value > MaxRetryAfterDelay
            ? MaxRetryAfterDelay
            : requestedDelay.Value;
    }

    private static TimeSpan ResolveResponseBodyReadTimeout(TimeSpan httpClientTimeout)
        => IsPositiveFiniteTimeout(httpClientTimeout) && httpClientTimeout < RequestTimeout
            ? httpClientTimeout
            : RequestTimeout;

    private static bool IsPositiveFiniteTimeout(TimeSpan timeout)
        => timeout > TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan;

    private static string? MapResponseReadFailureCode(string? errorCode)
        => errorCode switch
        {
            "EXTERNAL_VISUAL_JUDGE_TIMEOUT" => ResponseReadFailureTimeoutCode,
            "EXTERNAL_VISUAL_JUDGE_RESPONSE_READ_FAILED" => ResponseReadFailureExceptionCode,
            _ => null
        };

    private static async Task<OperationResult<string>> ReadBoundedResponseStringAsync(
        HttpContent content,
        TimeSpan bodyReadTimeout,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength is > MaxResponseBodyBytes)
        {
            return OperationResult<string>.Fail(
                "External visual judge response exceeded the bounded Content-Length.",
                "EXTERNAL_VISUAL_JUDGE_RESPONSE_TOO_LARGE");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(bodyReadTimeout);

        try
        {
            await using var stream = await content.ReadAsStreamAsync(timeoutCts.Token);
            using var memory = new MemoryStream();
            var buffer = new byte[8192];
            while (true)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), timeoutCts.Token);
                if (read == 0)
                    break;

                if (memory.Length + read > MaxResponseBodyBytes)
                {
                    return OperationResult<string>.Fail(
                        "External visual judge response exceeded the bounded byte limit.",
                        "EXTERNAL_VISUAL_JUDGE_RESPONSE_TOO_LARGE");
                }

                memory.Write(buffer, 0, read);
            }

            return OperationResult<string>.Ok(Encoding.UTF8.GetString(memory.ToArray()));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
        {
            return OperationResult<string>.Fail(
                "External visual judge response body read timed out.",
                "EXTERNAL_VISUAL_JUDGE_TIMEOUT");
        }
        catch (HttpRequestException ex)
        {
            return OperationResult<string>.Fail(
                $"External visual judge response read failed: {ex.GetType().Name}.",
                "EXTERNAL_VISUAL_JUDGE_RESPONSE_READ_FAILED");
        }
        catch (IOException ex)
        {
            return OperationResult<string>.Fail(
                $"External visual judge response read failed: {ex.GetType().Name}.",
                "EXTERNAL_VISUAL_JUDGE_RESPONSE_READ_FAILED");
        }
    }

    private async Task<OperationResult<ResolvedVisualJudgeImageSet>> ResolveRequestImageContentAsync(
        DesignConversionVisualJudgeRequest request,
        CancellationToken cancellationToken)
    {
        var expected = await ResolveImageForExternalRequestAsync(
            request.ExpectedScreenshot,
            "expected_screenshot",
            cancellationToken);
        if (!expected.IsSuccess)
            return expected.MapError<ResolvedVisualJudgeImageSet>();

        var rendered = await ResolveImageForExternalRequestAsync(
            request.RenderedImage,
            "rendered_image",
            cancellationToken);
        if (!rendered.IsSuccess)
            return rendered.MapError<ResolvedVisualJudgeImageSet>();

        long totalBytes;
        try
        {
            totalBytes = checked(expected.Value!.Bytes.LongLength + rendered.Value!.Bytes.LongLength);
        }
        catch (OverflowException)
        {
            return OperationResult<ResolvedVisualJudgeImageSet>.Fail(
                "External visual judge inline PNG payload exceeded the total byte limit.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_TOTAL_BYTES_OUT_OF_BOUNDS");
        }

        if (totalBytes > MaxInlinePngTotalBytes)
        {
            return OperationResult<ResolvedVisualJudgeImageSet>.Fail(
                "External visual judge inline PNG payload exceeded the total byte limit.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_TOTAL_BYTES_OUT_OF_BOUNDS");
        }

        return OperationResult<ResolvedVisualJudgeImageSet>.Ok(new ResolvedVisualJudgeImageSet(
            expected.Value!,
            rendered.Value!));
    }

    private async Task<OperationResult<DesignConversionVisualJudgeImageContent>> ResolveImageForExternalRequestAsync(
        RenderQaImageRecord image,
        string role,
        CancellationToken cancellationToken)
    {
        if (image is null)
        {
            return OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} image is required.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_REQUIRED");
        }

        if (image.Resolution.Width <= 0 ||
            image.Resolution.Height <= 0 ||
            image.Resolution.Width > MaxImageWidth ||
            image.Resolution.Height > MaxImageHeight ||
            (long)image.Resolution.Width * image.Resolution.Height > MaxImagePixels)
        {
            return OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} image dimensions are outside configured bounds.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_DIMENSIONS_OUT_OF_BOUNDS");
        }

        var byteLength = TryReadLong(image.Metadata, "browser_screenshot_byte_length")
            ?? TryReadLong(image.Metadata, "png_byte_length");
        if (byteLength > MaxInlinePngImageBytes)
        {
            return OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} PNG bytes are outside configured bounds.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_BYTES_OUT_OF_BOUNDS");
        }

        if (IsRemoteImageRef(image.Uri))
        {
            return OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} URI must not be fetched by the server.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_URI_UNSUPPORTED");
        }

        var content = await _imageContentResolver.ResolveAsync(
            image,
            role,
            MaxInlinePngImageBytes,
            cancellationToken);
        if (!content.IsSuccess)
            return content;

        if (!string.Equals(content.Value!.MediaType, "image/png", StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} image content type must be image/png.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_CONTENT_TYPE_UNSUPPORTED");
        }
        if (content.Value.Bytes.LongLength <= 0 || content.Value.Bytes.LongLength > MaxInlinePngImageBytes)
        {
            return OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} PNG bytes are outside configured bounds.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_BYTES_OUT_OF_BOUNDS");
        }
        if (!TryNormalizeSha256(image.Sha256, out var declaredSha) ||
            !string.Equals(content.Value.Sha256, declaredSha, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} PNG SHA-256 did not match resolved bytes.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_SHA_MISMATCH");
        }

        return content;
    }

    private static bool IsRemoteImageRef(string? uri)
        => !string.IsNullOrWhiteSpace(uri) &&
           (uri.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
            uri.StartsWith("https:", StringComparison.OrdinalIgnoreCase));

    private static bool TryNormalizeSha256(string? value, out string sha)
    {
        sha = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var text = value.Trim();
        if (text.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
            text = text["sha256:".Length..];
        if (text.Length != 64 || !text.All(Uri.IsHexDigit))
            return false;

        sha = text.ToLowerInvariant();
        return true;
    }

    private static long? TryReadLong(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private void ApplyApiKey(HttpRequestMessage request)
    {
        if (string.Equals(_options.ApiKeyHeaderName, "Authorization", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                string.IsNullOrWhiteSpace(_options.ApiKeyScheme) ? "Bearer" : _options.ApiKeyScheme,
                _options.ApiKey);
            return;
        }

        request.Headers.TryAddWithoutValidation(_options.ApiKeyHeaderName, _options.ApiKey);
    }

    private OperationResult<DesignConversionVisualJudgeResult> ParseResponse(
        DesignConversionVisualJudgeRequest request,
        string responseJson,
        int actualAttemptCount,
        string? payloadSha256 = null)
    {
        try
        {
            using var document = JsonDocument.Parse(responseJson);
            var root = document.RootElement;
            if (!IdentityMatches(root, "judge_request_id", request.JudgeRequestId) ||
                !IdentityMatches(root, "request_id", request.RequestId))
            {
                return OperationResult<DesignConversionVisualJudgeResult>.Ok(
                    CreateResponseValidationBlockedResult(
                        request,
                        ResponseValidationIdentityMismatchCode,
                        actualAttemptCount,
                        payloadSha256));
            }

            if (!IdentityMatches(root, "provider", _options.ProviderName) ||
                !IdentityMatches(root, "provider_name", _options.ProviderName))
            {
                return OperationResult<DesignConversionVisualJudgeResult>.Ok(
                    CreateResponseValidationBlockedResult(
                        request,
                        ResponseValidationProviderMismatchCode,
                        actualAttemptCount,
                        payloadSha256));
            }

            var modelId = ReadString(root, "model_id") ?? _options.ModelId;
            if (!string.Equals(modelId, _options.ModelId, StringComparison.Ordinal))
            {
                return OperationResult<DesignConversionVisualJudgeResult>.Ok(
                    CreateResponseValidationBlockedResult(
                        request,
                        ResponseValidationModelMismatchCode,
                        actualAttemptCount,
                        payloadSha256));
            }

            if (!TryReadScore(root, out var score))
            {
                return OperationResult<DesignConversionVisualJudgeResult>.Ok(
                    CreateResponseValidationBlockedResult(
                        request,
                        ResponseValidationSchemaInvalidCode,
                        actualAttemptCount,
                        payloadSha256));
            }

            if (ReadBool(root, "passed") is not { } providerReportedPassed)
            {
                return OperationResult<DesignConversionVisualJudgeResult>.Ok(
                    CreateResponseValidationBlockedResult(
                        request,
                        ResponseValidationSchemaInvalidCode,
                        actualAttemptCount,
                        payloadSha256));
            }

            var requiredQualityThreshold = ResolveQualityThreshold(request.Metadata);
            var serverComputedPassed = score >= requiredQualityThreshold;
            var providerPassedMismatch = providerReportedPassed != serverComputedPassed;
            if (!TryReadIssueArray(root, out var defectsElement))
            {
                return OperationResult<DesignConversionVisualJudgeResult>.Ok(
                    CreateResponseValidationBlockedResult(
                        request,
                        ResponseValidationSchemaInvalidCode,
                        actualAttemptCount,
                        payloadSha256));
            }

            var testProviderOnly = IsTestProviderOnly(_options);
            var confidence = ReadDouble(root, "confidence");
            var providerEvidenceRef = testProviderOnly
                ? $"test-visual-judge:{_options.ProviderName}"
                : $"external-visual-judge:{_options.ProviderName}";
            var modelEvidenceRef = testProviderOnly
                ? $"test-visual-judge-model:{modelId}"
                : $"external-visual-judge-model:{modelId}";
            var executionEvidenceRef = testProviderOnly
                ? NoConfiguredVisualJudge.EvidenceMarker
                : "external_ai_visual_judge_executed:true";
            var evidenceRefs = ReadStringArray(root, "evidence_refs")
                .Append(providerEvidenceRef)
                .Append(modelEvidenceRef)
                .Append(executionEvidenceRef)
                .Append(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderBoundaryMarker)
                .Append(DesignConversionVisualQaBoundaryPolicy.RedactionMarker)
                .Append($"provider_reported_passed:{providerReportedPassed}")
                .Append($"required_quality_threshold:{requiredQualityThreshold.ToString("R", CultureInfo.InvariantCulture)}")
                .Append($"server_quality_threshold_passed:{serverComputedPassed}")
                .Append($"provider_server_passed_mismatch:{providerPassedMismatch}")
                .Append("passed_authority:server_quality_threshold")
                .Concat(testProviderOnly
                    ? new[]
                    {
                        DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                        DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker
                    }
                    : Array.Empty<string>())
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var verdict = new VisualVerdict(
                VerdictId: ReadString(root, "verdict_id") ?? $"{request.JudgeRequestId}:external-visual-judge",
                Score: score,
                Defects: ReadDefects(defectsElement, request),
                EvidenceRefs: evidenceRefs,
                Passed: serverComputedPassed);
            var tokenMetadata = ReadTokenMetadata(root);
            var serverOwnedPricing = ResolveServerOwnedPricing(_options.ServerOwnedCostPerRequestUsd);
            var estimatedCostUsd = serverOwnedPricing.ResolveTotalCostUsd(actualAttemptCount);
            var providerReportedCostPresent = HasProviderReportedCost(root);
            var providerRawJudgeResultId = ReadString(root, "judge_result_id");
            var comparisonRequestId = ResolveComparisonRequestId(request);
            var judgeResultId = BuildCanonicalVisualResultId(request.JudgeRequestId, comparisonRequestId);
            var providerPolicy = DesignConversionVisualAiProviderSelectionPolicy.DescribeExternalConfig(
                _options.ProviderName,
                configComplete: true,
                completeProviderMode: testProviderOnly ? "local_stub" : "external_executed",
                completeProviderStatus: testProviderOnly ? "test_double_no_network" : "executed");
            var rawResultIdDiagnostic = HashProviderRawResultId(providerRawJudgeResultId);
            var callReceiptMetadata = BuildServerCallReceiptMetadata(
                request,
                modelId,
                comparisonRequestId,
                judgeResultId,
                testProviderOnly,
                responseIdentityVerified: true,
                responseSchemaVerified: true,
                payloadSha256: payloadSha256);
            var providerMetadata = new Dictionary<string, string>(_options.Metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal)
            {
                ["configured"] = bool.TrueString,
                ["provider"] = _options.ProviderName,
                ["network_call_executed"] = (!testProviderOnly).ToString(),
                ["provider_response_identity_verified"] = bool.TrueString,
                ["provider_response_schema_verified"] = bool.TrueString,
                ["provider_raw_visual_result_id_sha256"] = rawResultIdDiagnostic,
                ["provider_raw_visual_result_id_public"] = bool.FalseString,
                ["cost_source"] = serverOwnedPricing.CostSource,
                ["server_owned_pricing_configured"] = serverOwnedPricing.Configured.ToString(),
                ["server_owned_pricing_valid"] = serverOwnedPricing.Valid.ToString(),
                ["server_owned_pricing_status"] = serverOwnedPricing.Status,
                [ServerOwnedAttemptCountKey] = actualAttemptCount.ToString(CultureInfo.InvariantCulture),
                [ServerOwnedAttemptCountSourceKey] = ServerOwnedAttemptCountSourceValue,
                [ServerOwnedCostPerAttemptUsdKey] = FormatDecimalOrEmpty(serverOwnedPricing.CostPerRequestUsd),
                [ServerOwnedTotalCostUsdKey] = FormatDecimalOrEmpty(estimatedCostUsd),
                [ServerOwnedCostAccountingKey] = serverOwnedPricing.Valid
                    ? ServerOwnedCostAccountingFixedPerAttempt
                    : ServerOwnedCostAccountingUnknown,
                ["provider_reported_passed"] = providerReportedPassed.ToString(),
                ["required_quality_threshold"] = requiredQualityThreshold.ToString("R", CultureInfo.InvariantCulture),
                ["server_quality_threshold_passed"] = serverComputedPassed.ToString(),
                ["provider_server_passed_mismatch"] = providerPassedMismatch.ToString(),
                ["passed_authority"] = "server_quality_threshold",
                ["provider_reported_cost_present"] = providerReportedCostPresent.ToString(),
                ["provider_reported_cost_value_included"] = bool.FalseString,
                [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = testProviderOnly.ToString()
            };
            foreach (var pair in callReceiptMetadata)
                providerMetadata[pair.Key] = pair.Value;

            var resultMetadata = new Dictionary<string, string>
            {
                ["status"] = testProviderOnly ? TestStatusOk : StatusOk,
                ["image_payload_mode"] = PayloadMode,
                ["redaction"] = RedactionPolicy,
                ["external_ai_visual_judge_executed"] = (!testProviderOnly).ToString(),
                ["visual_ai_executed"] = (!testProviderOnly).ToString(),
                ["network_call_executed"] = (!testProviderOnly).ToString(),
                ["provider_response_identity_verified"] = bool.TrueString,
                ["provider_response_schema_verified"] = bool.TrueString,
                ["provider_raw_visual_result_id_sha256"] = rawResultIdDiagnostic,
                ["provider_raw_visual_result_id_public"] = bool.FalseString,
                ["cost_source"] = serverOwnedPricing.CostSource,
                ["server_owned_pricing_configured"] = serverOwnedPricing.Configured.ToString(),
                ["server_owned_pricing_valid"] = serverOwnedPricing.Valid.ToString(),
                ["server_owned_pricing_status"] = serverOwnedPricing.Status,
                [ServerOwnedAttemptCountKey] = actualAttemptCount.ToString(CultureInfo.InvariantCulture),
                [ServerOwnedAttemptCountSourceKey] = ServerOwnedAttemptCountSourceValue,
                [ServerOwnedCostPerAttemptUsdKey] = FormatDecimalOrEmpty(serverOwnedPricing.CostPerRequestUsd),
                [ServerOwnedTotalCostUsdKey] = FormatDecimalOrEmpty(estimatedCostUsd),
                [ServerOwnedCostAccountingKey] = serverOwnedPricing.Valid
                    ? ServerOwnedCostAccountingFixedPerAttempt
                    : ServerOwnedCostAccountingUnknown,
                ["provider_reported_passed"] = providerReportedPassed.ToString(),
                ["required_quality_threshold"] = requiredQualityThreshold.ToString("R", CultureInfo.InvariantCulture),
                ["server_quality_threshold_passed"] = serverComputedPassed.ToString(),
                ["provider_server_passed_mismatch"] = providerPassedMismatch.ToString(),
                ["passed_authority"] = "server_quality_threshold",
                ["provider_reported_cost_present"] = providerReportedCostPresent.ToString(),
                ["provider_reported_cost_value_included"] = bool.FalseString,
                ["confidence"] = confidence?.ToString("0.####", CultureInfo.InvariantCulture) ?? string.Empty,
                [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = testProviderOnly.ToString()
            };
            foreach (var pair in callReceiptMetadata)
                resultMetadata[pair.Key] = pair.Value;

            return OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: judgeResultId,
                JudgeRequestId: request.JudgeRequestId,
                Status: testProviderOnly ? TestStatusOk : StatusOk,
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: _options.ProviderName,
                    EndpointHost: _options.Endpoint!.Host,
                    ModelId: modelId,
                    ExternalCallExecuted: !testProviderOnly,
                    Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(providerMetadata, providerPolicy)),
                EstimatedCostUsd: estimatedCostUsd,
                TokenMetadata: tokenMetadata,
                EvidenceRefs: evidenceRefs,
                Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(resultMetadata, providerPolicy)));
        }
        catch (JsonException)
        {
            return OperationResult<DesignConversionVisualJudgeResult>.Ok(
                CreateResponseValidationBlockedResult(
                    request,
                    ResponseValidationInvalidJsonCode,
                    actualAttemptCount,
                    payloadSha256));
        }
    }

    private IReadOnlyDictionary<string, string> BuildServerCallReceiptMetadata(
        DesignConversionVisualJudgeRequest request,
        string modelId,
        string comparisonRequestId,
        string judgeResultId,
        bool testProviderOnly,
        bool responseIdentityVerified,
        bool responseSchemaVerified,
        string? endpointHostOverride = null,
        string? payloadSha256 = null)
    {
        var receiptId = $"{request.JudgeRequestId}:server-visual-provider-call-receipt";
        var visualJudgeRequestId = IsVisualJudgeRequestId(request.JudgeRequestId)
            ? request.JudgeRequestId
            : string.Empty;
        var endpointHost = endpointHostOverride ?? _options.Endpoint?.Host ?? string.Empty;
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ServerCallReceiptKey] = (!testProviderOnly).ToString(),
            [ServerCallReceiptVersionKey] = ServerCallReceiptVersion,
            [ServerCallReceiptIdKey] = SafeReceiptScalar(receiptId, ServerCallReceiptIdKey),
            [ServerCallReceiptJobIdKey] = ResolveReceiptJobId(request),
            [ServerCallReceiptJudgeRequestIdKey] = SafeReceiptScalar(request.JudgeRequestId, ServerCallReceiptJudgeRequestIdKey),
            [ServerCallReceiptRequestIdKey] = SafeReceiptScalar(request.RequestId, ServerCallReceiptRequestIdKey),
            [ServerCallReceiptRootRequestIdKey] = SafeReceiptScalar(request.RequestId, ServerCallReceiptRootRequestIdKey),
            [ServerCallReceiptProviderRequestIdKey] = SafeReceiptScalar(request.JudgeRequestId, ServerCallReceiptProviderRequestIdKey),
            [ServerCallReceiptComparisonRequestIdKey] = SafeReceiptScalar(comparisonRequestId, ServerCallReceiptComparisonRequestIdKey),
            [ServerCallReceiptVisualJudgeRequestIdKey] = SafeReceiptScalar(visualJudgeRequestId, ServerCallReceiptVisualJudgeRequestIdKey),
            [ServerCallReceiptVisualResultIdKey] = SafeReceiptScalar(judgeResultId, ServerCallReceiptVisualResultIdKey),
            [ServerCallReceiptProviderNameKey] = SafeReceiptScalar(_options.ProviderName, ServerCallReceiptProviderNameKey),
            [ServerCallReceiptProviderModelKey] = SafeReceiptScalar(modelId, ServerCallReceiptProviderModelKey),
            [ServerCallReceiptEndpointHostKey] = SafeReceiptScalar(endpointHost, ServerCallReceiptEndpointHostKey),
            [ServerCallReceiptExternalCallExecutedKey] = (!testProviderOnly).ToString(),
            [ServerCallReceiptResponseIdentityVerifiedKey] = responseIdentityVerified.ToString(),
            [ServerCallReceiptResponseSchemaVerifiedKey] = responseSchemaVerified.ToString(),
            [ServerCallReceiptRequestPayloadSha256Key] = string.IsNullOrWhiteSpace(payloadSha256)
                ? string.Empty
                : $"sha256:{payloadSha256}"
        };
    }

    public static string BuildCanonicalVisualResultId(
        string providerRequestId,
        string? comparisonRequestId)
    {
        var safeProviderRequestId = SafeReceiptScalar(providerRequestId, ServerCallReceiptProviderRequestIdKey);
        var safeComparisonRequestId = SafeReceiptScalar(
            string.IsNullOrWhiteSpace(comparisonRequestId) ? providerRequestId : comparisonRequestId,
            ServerCallReceiptComparisonRequestIdKey);

        if (string.Equals(safeComparisonRequestId, safeProviderRequestId, StringComparison.Ordinal))
            return $"{safeComparisonRequestId}:{ServerOwnedVisualResultSegment}";

        return $"{safeComparisonRequestId}:{ServerOwnedVisualResultSegment}:{ShortHash(safeProviderRequestId)}";
    }

    public static string BuildCanonicalComparisonArtifactId(
        string providerRequestId,
        string? comparisonRequestId)
        => $"{BuildCanonicalVisualResultId(providerRequestId, comparisonRequestId)}:comparison-artifact";

    private static string ResolveReceiptJobId(DesignConversionVisualJudgeRequest request)
    {
        if (request.Metadata.TryGetValue("job_id", out var jobId) &&
            !string.IsNullOrWhiteSpace(jobId))
        {
            return SafeReceiptScalar(jobId, ServerCallReceiptJobIdKey);
        }

        foreach (var marker in new[] { ":comparison-request:", ":visual-judge-request:" })
        {
            var index = request.JudgeRequestId.IndexOf(marker, StringComparison.Ordinal);
            if (index > 0)
                return SafeReceiptScalar(request.JudgeRequestId[..index], ServerCallReceiptJobIdKey);
        }

        return SafeReceiptScalar(request.JudgeRequestId, ServerCallReceiptJobIdKey);
    }

    private static string ResolveComparisonRequestId(DesignConversionVisualJudgeRequest request)
    {
        if (request.Metadata.TryGetValue("comparison_request_id", out var comparisonRequestId) &&
            !string.IsNullOrWhiteSpace(comparisonRequestId))
        {
            return comparisonRequestId;
        }

        return IsComparisonRequestId(request.JudgeRequestId)
            ? request.JudgeRequestId
            : string.Empty;
    }

    private static bool IsComparisonRequestId(string requestId)
        => requestId.Contains(":comparison-request:", StringComparison.Ordinal);

    private static bool IsVisualJudgeRequestId(string requestId)
        => requestId.Contains(":visual-judge-request:", StringComparison.Ordinal);

    private static string SafeReceiptScalar(string? value, string scope)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        return DesignConversionDpoLeakageGuard.SanitizeScalar(value ?? string.Empty, scope, redactions);
    }

    private static string HashProviderRawResultId(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? "missing"
            : $"sha256:{ShortHash(value)}";

    private static string ShortHash(string? value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)))
            .ToLowerInvariant()[..16];

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static IReadOnlyList<VisualDefect> ReadDefects(JsonElement defectsElement, DesignConversionVisualJudgeRequest request)
    {
        var defects = new List<VisualDefect>();
        var index = 0;
        foreach (var defect in defectsElement.EnumerateArray())
        {
            index++;
            defects.Add(new VisualDefect(
                DefectId: ReadString(defect, "defect_id") ?? ReadString(defect, "id") ?? $"{request.JudgeRequestId}:external-defect:{index}",
                Category: ReadString(defect, "category") ?? "visual",
                Severity: ReadString(defect, "severity") ?? "unknown",
                Description: ReadString(defect, "description") ?? string.Empty,
                X: ReadDouble(defect, "x") ?? 0,
                Y: ReadDouble(defect, "y") ?? 0,
                Width: ReadDouble(defect, "width") ?? 1,
                Height: ReadDouble(defect, "height") ?? 1,
                EvidenceImageIds: ReadStringArray(defect, "evidence_image_ids").DefaultIfEmpty(request.RenderedImage.ImageId).ToArray()));
        }

        return defects;
    }

    private static bool TryReadIssueArray(JsonElement root, out JsonElement issuesElement)
    {
        if (root.TryGetProperty("defects", out issuesElement) &&
            issuesElement.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        if (root.TryGetProperty("issues", out issuesElement) &&
            issuesElement.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        issuesElement = default;
        return false;
    }

    private static bool TryReadScore(JsonElement root, out double score)
    {
        var rawScore = ReadDouble(root, "score") ?? ReadDouble(root, "visual_score");
        if (!rawScore.HasValue ||
            double.IsNaN(rawScore.Value) ||
            double.IsInfinity(rawScore.Value) ||
            rawScore.Value < 0.0 ||
            rawScore.Value > 1.0)
        {
            score = 0.0;
            return false;
        }

        score = rawScore.Value;
        return true;
    }

    private static bool IdentityMatches(JsonElement root, string propertyName, string expected)
        => ReadString(root, propertyName) is not { } actual ||
           string.Equals(actual, expected, StringComparison.Ordinal);

    private static IReadOnlyDictionary<string, double> ReadTokenMetadata(JsonElement root)
    {
        if (!root.TryGetProperty("tokens", out var tokens) || tokens.ValueKind != JsonValueKind.Object)
            return new Dictionary<string, double>();

        var result = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var pair in tokens.EnumerateObject())
        {
            if (pair.Value.ValueKind == JsonValueKind.Number && pair.Value.TryGetDouble(out var value))
                result[pair.Name] = value;
        }

        return result;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
            return property.GetString();

        return null;
    }

    private static bool? ReadBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(property.GetString(), out var value) => value,
            _ => null
        };
    }

    private static double? ReadDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value))
            return value;

        if (property.ValueKind == JsonValueKind.String &&
            double.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return value;
        }

        return null;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
            return [];

        return property
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();
    }

    private static double Clamp01(double value)
        => Math.Min(1.0, Math.Max(0.0, value));

    private static bool HasProviderReportedCost(JsonElement root)
        => HasNonNullProperty(root, "cost_estimate_usd") ||
           HasNonNullProperty(root, "estimated_cost_usd");

    private static bool HasNonNullProperty(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var property) &&
           property.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined;

    private static ServerOwnedPricingRuntimeState ResolveServerOwnedPricing(decimal? costPerRequestUsd)
    {
        if (!costPerRequestUsd.HasValue)
        {
            return new ServerOwnedPricingRuntimeState(
                Configured: false,
                Valid: false,
                CostPerRequestUsd: null,
                Status: "unconfigured",
                CostSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced);
        }

        if (costPerRequestUsd.Value < 0m ||
            costPerRequestUsd.Value > DesignConversionExternalVisualProviderConfigIntake.MaxServerOwnedCostPerRequestUsd)
        {
            return new ServerOwnedPricingRuntimeState(
                Configured: false,
                Valid: false,
                CostPerRequestUsd: null,
                Status: "invalid",
                CostSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced);
        }

        return new ServerOwnedPricingRuntimeState(
            Configured: true,
            Valid: true,
            CostPerRequestUsd: costPerRequestUsd.Value,
            Status: "configured",
            CostSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest);
    }

    private sealed record ServerOwnedPricingRuntimeState(
        bool Configured,
        bool Valid,
        decimal? CostPerRequestUsd,
        string Status,
        string CostSource)
    {
        public decimal? ResolveTotalCostUsd(int actualAttemptCount)
            => Valid && CostPerRequestUsd.HasValue && actualAttemptCount > 0
                ? CostPerRequestUsd.Value * actualAttemptCount
                : null;
    }

    private DesignConversionVisualJudgeResult CreateTerminalHttpBlockedResult(
        DesignConversionVisualJudgeRequest request,
        HttpStatusCode statusCode,
        int actualAttemptCount,
        string payloadSha256)
    {
        const string reason = "External visual judge provider returned a terminal non-success HTTP status.";
        var statusCodeText = ((int)statusCode).ToString(CultureInfo.InvariantCulture);
        var serverOwnedPricing = ResolveServerOwnedPricing(_options.ServerOwnedCostPerRequestUsd);
        var estimatedCostUsd = serverOwnedPricing.ResolveTotalCostUsd(actualAttemptCount);
        var comparisonRequestId = ResolveComparisonRequestId(request);
        var judgeResultId = $"{BuildCanonicalVisualResultId(request.JudgeRequestId, comparisonRequestId)}:http-blocked";
        var evidenceRefs = new[]
        {
            $"visual-judge-request:{request.JudgeRequestId}",
            StatusHttpBlocked,
            TerminalHttpStatusMarker,
            $"terminal_http_status_code:{statusCodeText}",
            "external_ai_visual_judge_executed:true",
            DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderBoundaryMarker,
            DesignConversionVisualQaBoundaryPolicy.RedactionMarker
        };
        var verdict = new VisualVerdict(
            VerdictId: $"{request.JudgeRequestId}:external-visual-judge-http-blocked",
            Score: 0.0,
            Defects:
            [
                new VisualDefect(
                    DefectId: $"{request.JudgeRequestId}:external-visual-judge-http-blocked",
                    Category: "external_visual_judge_http_non_success",
                    Severity: "blocker",
                    Description: reason,
                    X: 0,
                    Y: 0,
                    Width: 1,
                    Height: 1,
                    EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
            ],
            EvidenceRefs: evidenceRefs,
            Passed: false);
        var providerPolicy = DesignConversionVisualAiProviderSelectionPolicy.DescribeExternalConfig(
            _options.ProviderName,
            configComplete: true,
            completeProviderMode: "external_executed",
            completeProviderStatus: "terminal_http_status");
        var callReceiptMetadata = BuildServerCallReceiptMetadata(
            request,
            _options.ModelId,
            comparisonRequestId,
            judgeResultId,
            testProviderOnly: false,
            responseIdentityVerified: false,
            responseSchemaVerified: false,
            payloadSha256: payloadSha256);
        var providerMetadata = new Dictionary<string, string>(_options.Metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            ["configured"] = bool.TrueString,
            ["provider"] = _options.ProviderName,
            ["status"] = StatusHttpBlocked,
            ["reason"] = "terminal_http_non_success",
            ["network_call_executed"] = bool.TrueString,
            ["external_ai_visual_judge_executed"] = bool.TrueString,
            ["visual_ai_executed"] = bool.TrueString,
            ["provider_response_identity_verified"] = bool.FalseString,
            ["provider_response_schema_verified"] = bool.FalseString,
            ["provider_response_verified"] = bool.FalseString,
            [TerminalHttpStatusCodeKey] = statusCodeText,
            ["cost_source"] = serverOwnedPricing.CostSource,
            ["server_owned_pricing_configured"] = serverOwnedPricing.Configured.ToString(),
            ["server_owned_pricing_valid"] = serverOwnedPricing.Valid.ToString(),
            ["server_owned_pricing_status"] = serverOwnedPricing.Status,
            [ServerOwnedAttemptCountKey] = actualAttemptCount.ToString(CultureInfo.InvariantCulture),
            [ServerOwnedAttemptCountSourceKey] = ServerOwnedAttemptCountSourceValue,
            [ServerOwnedCostPerAttemptUsdKey] = FormatDecimalOrEmpty(serverOwnedPricing.CostPerRequestUsd),
            [ServerOwnedTotalCostUsdKey] = FormatDecimalOrEmpty(estimatedCostUsd),
            [ServerOwnedCostAccountingKey] = serverOwnedPricing.Valid
                ? ServerOwnedCostAccountingFixedPerAttempt
                : ServerOwnedCostAccountingUnknown,
            ["provider_reported_cost_present"] = bool.FalseString,
            ["provider_reported_cost_value_included"] = bool.FalseString,
            [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.FalseString
        };
        foreach (var pair in callReceiptMetadata)
            providerMetadata[pair.Key] = pair.Value;

        var resultMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["status"] = StatusHttpBlocked,
            ["reason"] = "terminal_http_non_success",
            ["image_payload_mode"] = PayloadMode,
            ["redaction"] = RedactionPolicy,
            ["network_call_executed"] = bool.TrueString,
            ["external_ai_visual_judge_executed"] = bool.TrueString,
            ["visual_ai_executed"] = bool.TrueString,
            ["provider_response_identity_verified"] = bool.FalseString,
            ["provider_response_schema_verified"] = bool.FalseString,
            ["provider_response_verified"] = bool.FalseString,
            [TerminalHttpStatusCodeKey] = statusCodeText,
            ["cost_source"] = serverOwnedPricing.CostSource,
            ["server_owned_pricing_configured"] = serverOwnedPricing.Configured.ToString(),
            ["server_owned_pricing_valid"] = serverOwnedPricing.Valid.ToString(),
            ["server_owned_pricing_status"] = serverOwnedPricing.Status,
            [ServerOwnedAttemptCountKey] = actualAttemptCount.ToString(CultureInfo.InvariantCulture),
            [ServerOwnedAttemptCountSourceKey] = ServerOwnedAttemptCountSourceValue,
            [ServerOwnedCostPerAttemptUsdKey] = FormatDecimalOrEmpty(serverOwnedPricing.CostPerRequestUsd),
            [ServerOwnedTotalCostUsdKey] = FormatDecimalOrEmpty(estimatedCostUsd),
            [ServerOwnedCostAccountingKey] = serverOwnedPricing.Valid
                ? ServerOwnedCostAccountingFixedPerAttempt
                : ServerOwnedCostAccountingUnknown,
            ["provider_reported_cost_present"] = bool.FalseString,
            ["provider_reported_cost_value_included"] = bool.FalseString,
            [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.FalseString
        };
        foreach (var pair in callReceiptMetadata)
            resultMetadata[pair.Key] = pair.Value;

        return new DesignConversionVisualJudgeResult(
            JudgeResultId: judgeResultId,
            JudgeRequestId: request.JudgeRequestId,
            Status: StatusHttpBlocked,
            IsBlocked: true,
            Verdict: verdict,
            Provider: new DesignConversionVisualJudgeProviderMetadata(
                ProviderName: _options.ProviderName,
                EndpointHost: _options.Endpoint!.Host,
                ModelId: _options.ModelId,
                ExternalCallExecuted: true,
                Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(providerMetadata, providerPolicy)),
            EstimatedCostUsd: estimatedCostUsd,
            TokenMetadata: new Dictionary<string, double>(),
            EvidenceRefs: evidenceRefs,
            Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(resultMetadata, providerPolicy));
    }

    private DesignConversionVisualJudgeResult CreateTransportFailureBlockedResult(
        DesignConversionVisualJudgeRequest request,
        string transportFailureCode,
        int actualAttemptCount,
        string payloadSha256)
        => CreateExternalAttemptBlockedResult(
            request,
            actualAttemptCount,
            payloadSha256,
            status: StatusTransportBlocked,
            resultIdSuffix: "transport-blocked",
            reason: "External visual judge provider transport failed after an HTTP send attempt.",
            marker: TransportFailureMarker,
            failureCodeKey: TransportFailureCodeKey,
            failureCode: transportFailureCode,
            failureEvidencePrefix: "transport_failure_code",
            verdictIdSuffix: "external-visual-judge-transport-blocked",
            metadataReason: "transport_failure",
            providerPolicyMode: "external_attempted",
            providerPolicyStatus: "transport_failure",
            networkCallAttempted: true,
            networkCallExecutionVerified: false,
            providerResponseReceived: false,
            networkCallExecuted: false);

    private DesignConversionVisualJudgeResult CreateResponseReadFailureBlockedResult(
        DesignConversionVisualJudgeRequest request,
        string responseReadFailureCode,
        int actualAttemptCount,
        string payloadSha256)
        => CreateExternalAttemptBlockedResult(
            request,
            actualAttemptCount,
            payloadSha256,
            status: StatusResponseReadBlocked,
            resultIdSuffix: "response-read-blocked",
            reason: "External visual judge provider response body could not be read after response headers were received.",
            marker: ResponseReadFailureMarker,
            failureCodeKey: ResponseReadFailureCodeKey,
            failureCode: responseReadFailureCode,
            failureEvidencePrefix: "response_read_failure_code",
            verdictIdSuffix: "external-visual-judge-response-read-blocked",
            metadataReason: "response_read_failure",
            providerPolicyMode: "external_response_received",
            providerPolicyStatus: "response_read_failure",
            networkCallAttempted: true,
            networkCallExecutionVerified: true,
            providerResponseReceived: true,
            networkCallExecuted: true);

    private DesignConversionVisualJudgeResult CreateResponsePolicyBlockedResult(
        DesignConversionVisualJudgeRequest request,
        string responsePolicyCode,
        int actualAttemptCount,
        string payloadSha256)
        => CreateExternalAttemptBlockedResult(
            request,
            actualAttemptCount,
            payloadSha256,
            status: StatusResponsePolicyBlocked,
            resultIdSuffix: "response-policy-blocked",
            reason: "External visual judge provider response body was rejected by the bounded response-size policy after response headers were received.",
            marker: ResponsePolicyBlockMarker,
            failureCodeKey: ResponsePolicyCodeKey,
            failureCode: responsePolicyCode,
            failureEvidencePrefix: "response_policy_code",
            verdictIdSuffix: "external-visual-judge-response-policy-blocked",
            metadataReason: "response_policy_block",
            providerPolicyMode: "external_response_received",
            providerPolicyStatus: "response_policy_blocked",
            networkCallAttempted: true,
            networkCallExecutionVerified: true,
            providerResponseReceived: true,
            networkCallExecuted: true);

    private DesignConversionVisualJudgeResult CreateResponseValidationBlockedResult(
        DesignConversionVisualJudgeRequest request,
        string responseValidationCode,
        int actualAttemptCount,
        string? payloadSha256 = null)
        => CreateExternalAttemptBlockedResult(
            request,
            actualAttemptCount,
            payloadSha256,
            status: StatusResponseValidationBlocked,
            resultIdSuffix: $"response-validation-blocked-{responseValidationCode}",
            reason: "External visual judge provider response failed server validation after the response body was received.",
            marker: ResponseValidationFailureMarker,
            failureCodeKey: ResponseValidationCodeKey,
            failureCode: responseValidationCode,
            failureEvidencePrefix: "response_validation_code",
            verdictIdSuffix: "external-visual-judge-response-validation-blocked",
            metadataReason: "response_validation_failure",
            providerPolicyMode: "external_response_received",
            providerPolicyStatus: "response_validation_failure",
            networkCallAttempted: true,
            networkCallExecutionVerified: true,
            providerResponseReceived: true,
            networkCallExecuted: true);

    private DesignConversionVisualJudgeResult CreateExternalAttemptBlockedResult(
        DesignConversionVisualJudgeRequest request,
        int actualAttemptCount,
        string? payloadSha256,
        string status,
        string resultIdSuffix,
        string reason,
        string marker,
        string failureCodeKey,
        string failureCode,
        string failureEvidencePrefix,
        string verdictIdSuffix,
        string metadataReason,
        string providerPolicyMode,
        string providerPolicyStatus,
        bool networkCallAttempted,
        bool networkCallExecutionVerified,
        bool providerResponseReceived,
        bool networkCallExecuted)
    {
        var serverOwnedPricing = ResolveServerOwnedPricing(_options.ServerOwnedCostPerRequestUsd);
        var estimatedCostUsd = serverOwnedPricing.ResolveTotalCostUsd(actualAttemptCount);
        var comparisonRequestId = ResolveComparisonRequestId(request);
        var judgeResultId = $"{BuildCanonicalVisualResultId(request.JudgeRequestId, comparisonRequestId)}:{resultIdSuffix}";
        var evidenceRefs = new[]
        {
            $"visual-judge-request:{request.JudgeRequestId}",
            status,
            marker,
            $"{failureEvidencePrefix}:{failureCode}",
            BuildBoolEvidenceRef(NetworkCallAttemptedKey, networkCallAttempted),
            BuildBoolEvidenceRef(NetworkCallExecutionVerifiedKey, networkCallExecutionVerified),
            BuildBoolEvidenceRef(ProviderResponseReceivedKey, providerResponseReceived),
            DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker,
            DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderBoundaryMarker,
            DesignConversionVisualQaBoundaryPolicy.RedactionMarker
        };
        var verdict = new VisualVerdict(
            VerdictId: $"{request.JudgeRequestId}:{verdictIdSuffix}",
            Score: 0.0,
            Defects:
            [
                new VisualDefect(
                    DefectId: $"{request.JudgeRequestId}:{verdictIdSuffix}",
                    Category: marker,
                    Severity: "blocker",
                    Description: reason,
                    X: 0,
                    Y: 0,
                    Width: 1,
                    Height: 1,
                    EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
            ],
            EvidenceRefs: evidenceRefs,
            Passed: false);
        var providerPolicy = DesignConversionVisualAiProviderSelectionPolicy.DescribeExternalConfig(
            _options.ProviderName,
            configComplete: true,
            completeProviderMode: providerPolicyMode,
            completeProviderStatus: providerPolicyStatus);
        var callReceiptMetadata = BuildServerCallReceiptMetadata(
            request,
            _options.ModelId,
            comparisonRequestId,
            judgeResultId,
            testProviderOnly: false,
            responseIdentityVerified: false,
            responseSchemaVerified: false,
            endpointHostOverride: TransportFailureEndpointHostPlaceholder,
            payloadSha256: payloadSha256);
        var providerMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["configured"] = bool.TrueString,
            ["provider"] = _options.ProviderName,
            ["status"] = status,
            ["reason"] = metadataReason,
            [failureCodeKey] = failureCode,
            [marker] = bool.TrueString,
            [NetworkCallAttemptedKey] = networkCallAttempted.ToString(),
            [NetworkCallExecutionVerifiedKey] = networkCallExecutionVerified.ToString(),
            [ProviderResponseReceivedKey] = providerResponseReceived.ToString(),
            ["network_call_executed"] = networkCallExecuted.ToString(),
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["visual_ai_executed"] = bool.FalseString,
            ["provider_response_identity_verified"] = bool.FalseString,
            ["provider_response_schema_verified"] = bool.FalseString,
            ["provider_response_verified"] = bool.FalseString,
            ["cost_source"] = serverOwnedPricing.CostSource,
            ["server_owned_pricing_configured"] = serverOwnedPricing.Configured.ToString(),
            ["server_owned_pricing_valid"] = serverOwnedPricing.Valid.ToString(),
            ["server_owned_pricing_status"] = serverOwnedPricing.Status,
            [ServerOwnedAttemptCountKey] = actualAttemptCount.ToString(CultureInfo.InvariantCulture),
            [ServerOwnedAttemptCountSourceKey] = ServerOwnedAttemptCountSourceValue,
            [ServerOwnedCostPerAttemptUsdKey] = FormatDecimalOrEmpty(serverOwnedPricing.CostPerRequestUsd),
            [ServerOwnedTotalCostUsdKey] = FormatDecimalOrEmpty(estimatedCostUsd),
            [ServerOwnedCostAccountingKey] = serverOwnedPricing.Valid
                ? ServerOwnedCostAccountingFixedPerAttempt
                : ServerOwnedCostAccountingUnknown,
            ["provider_reported_cost_present"] = bool.FalseString,
            ["provider_reported_cost_value_included"] = bool.FalseString,
            [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.FalseString
        };
        foreach (var pair in callReceiptMetadata)
            providerMetadata[pair.Key] = pair.Value;

        var resultMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["status"] = status,
            ["reason"] = metadataReason,
            [failureCodeKey] = failureCode,
            [marker] = bool.TrueString,
            ["image_payload_mode"] = PayloadMode,
            ["redaction"] = RedactionPolicy,
            [NetworkCallAttemptedKey] = networkCallAttempted.ToString(),
            [NetworkCallExecutionVerifiedKey] = networkCallExecutionVerified.ToString(),
            [ProviderResponseReceivedKey] = providerResponseReceived.ToString(),
            ["network_call_executed"] = networkCallExecuted.ToString(),
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["visual_ai_executed"] = bool.FalseString,
            ["provider_response_identity_verified"] = bool.FalseString,
            ["provider_response_schema_verified"] = bool.FalseString,
            ["provider_response_verified"] = bool.FalseString,
            ["cost_source"] = serverOwnedPricing.CostSource,
            ["server_owned_pricing_configured"] = serverOwnedPricing.Configured.ToString(),
            ["server_owned_pricing_valid"] = serverOwnedPricing.Valid.ToString(),
            ["server_owned_pricing_status"] = serverOwnedPricing.Status,
            [ServerOwnedAttemptCountKey] = actualAttemptCount.ToString(CultureInfo.InvariantCulture),
            [ServerOwnedAttemptCountSourceKey] = ServerOwnedAttemptCountSourceValue,
            [ServerOwnedCostPerAttemptUsdKey] = FormatDecimalOrEmpty(serverOwnedPricing.CostPerRequestUsd),
            [ServerOwnedTotalCostUsdKey] = FormatDecimalOrEmpty(estimatedCostUsd),
            [ServerOwnedCostAccountingKey] = serverOwnedPricing.Valid
                ? ServerOwnedCostAccountingFixedPerAttempt
                : ServerOwnedCostAccountingUnknown,
            ["provider_reported_cost_present"] = bool.FalseString,
            ["provider_reported_cost_value_included"] = bool.FalseString,
            [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.FalseString
        };
        foreach (var pair in callReceiptMetadata)
            resultMetadata[pair.Key] = pair.Value;

        return new DesignConversionVisualJudgeResult(
            JudgeResultId: judgeResultId,
            JudgeRequestId: request.JudgeRequestId,
            Status: status,
            IsBlocked: true,
            Verdict: verdict,
            Provider: new DesignConversionVisualJudgeProviderMetadata(
                ProviderName: _options.ProviderName,
                EndpointHost: TransportFailureEndpointHostPlaceholder,
                ModelId: _options.ModelId,
                ExternalCallExecuted: true,
                Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(providerMetadata, providerPolicy)),
            EstimatedCostUsd: estimatedCostUsd,
            TokenMetadata: new Dictionary<string, double>(),
            EvidenceRefs: evidenceRefs,
            Metadata: DesignConversionVisualAiProviderSelectionPolicy.MergeInto(resultMetadata, providerPolicy));
    }

    private static string BuildBoolEvidenceRef(string key, bool value)
        => $"{key}:{value.ToString().ToLowerInvariant()}";

    private static string FormatDecimalOrEmpty(decimal? value)
        => value.HasValue
            ? value.Value.ToString("0.####", CultureInfo.InvariantCulture)
            : string.Empty;

    private sealed record HttpResponseOutcome(
        HttpResponseMessage? Response,
        int AttemptCount,
        string? TransportFailureCode = null);

    private sealed record ResolvedVisualJudgeImageSet(
        DesignConversionVisualJudgeImageContent Expected,
        DesignConversionVisualJudgeImageContent Rendered);

    private static bool IsTestProviderOnly(DesignConversionExternalVisualJudgeOptions options)
        => IsTruthyMetadata(options.Metadata, DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker)
           || IsTruthyMetadata(options.Metadata, "test_provider");

    private static bool IsTruthyMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null
           && metadata.TryGetValue(key, out var value)
           && (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "1", StringComparison.Ordinal)
               || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));
}

internal static class DesignConversionVisualJudgeValidation
{
    public static OperationResult<bool> Validate(DesignConversionVisualJudgeRequest request)
    {
        if (request is null)
            return OperationResult<bool>.Fail("DesignConversion visual judge request is required.", "VISUAL_JUDGE_REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.JudgeRequestId))
            return OperationResult<bool>.Fail("DesignConversion visual judge request id is required.", "VISUAL_JUDGE_REQUEST_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return OperationResult<bool>.Fail("DesignConversion request id is required.", "REQUEST_ID_REQUIRED");
        if (request.ExpectedScreenshot is null || request.RenderedImage is null)
            return OperationResult<bool>.Fail("DesignConversion visual judge images are required.", "VISUAL_JUDGE_IMAGES_REQUIRED");
        if (request.ExpectedScreenshot.Resolution is null || request.RenderedImage.Resolution is null)
            return OperationResult<bool>.Fail("DesignConversion visual judge image resolutions are required.", "VISUAL_JUDGE_RESOLUTION_REQUIRED");

        return OperationResult<bool>.Ok(true);
    }
}
