// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>
/// Visual evidence admitted by the DesignConversion trust boundary.
/// The payload stores identities and hashes as data, but the matcher never
/// treats matching hashes or a self-claimed metadata marker as proof by itself.
/// </summary>
public sealed record DesignConversionReferenceVisualAxisEvidence(
    string TrustBoundary,
    string TrustClassification,
    string ProviderKind,
    string ComparisonRequestId,
    string ExpectedImageId,
    string ExpectedImageSha256,
    string ActualImageId,
    string ActualImageSha256,
    string DiffImageId,
    string DiffImageSha256,
    IReadOnlyList<string> EvidenceRefs,
    double Score);

/// <summary>
/// Markup evidence is separate from pixels: it asks whether the generated
/// HTML/CSS keeps the same kinds of nodes, depth, and user-visible anchors.
/// </summary>
public sealed record DesignConversionReferenceMarkupAxisEvidence(
    string ExpectedMarkupFingerprint,
    double NodeTypeCompositionScore,
    double NestingDepthScore,
    double AnchorValuesScore);

/// <summary>
/// Reports whether the serialized visual-axis contract is recognized by this
/// matcher. It does not prove arbitrary JSON was actually produced by a
/// server-owned provider; callers must serialize upstream canonicalized output.
/// </summary>
public sealed record DesignConversionReferenceVisualAxisStatus(
    bool IsPresent,
    bool IsContractRecognized,
    bool Passed,
    double? Score,
    double Threshold,
    string Status);

public sealed record DesignConversionReferenceMarkupAxisStatus(
    bool IsPresent,
    bool Passed,
    double? NodeTypeCompositionScore,
    double? NestingDepthScore,
    double? AnchorValuesScore,
    double NodeTypeCompositionThreshold,
    double NestingDepthThreshold,
    double AnchorValuesThreshold,
    string Status);

public sealed record DesignConversionReferenceMatchResult(
    bool IsMatch,
    string Status,
    DesignConversionReferenceVisualAxisStatus CandidateVisualAxis,
    DesignConversionReferenceVisualAxisStatus ReferenceVisualAxis,
    DesignConversionReferenceMarkupAxisStatus CandidateMarkupAxis,
    DesignConversionReferenceMarkupAxisStatus ReferenceMarkupAxis);

/// <summary>
/// Canonical frozen-reference payload for design-to-code comparison.
/// It is deliberately typed and asymmetric: a student candidate is compared
/// against a paid reference, while prose and metadata-only claims stay outside
/// the authority boundary.
/// </summary>
public sealed record DesignConversionReferenceEvidencePayload
{
    public const string Schema =
        "xiigen.design-conversion-reference-evidence.v1";

    private const int MaximumPayloadChars = 131_072;
    private const int MaximumIdentityChars = 256;

    private static readonly JsonDocumentOptions StrictJsonDocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 6
    };

    private static readonly JsonWriterOptions CanonicalJsonWriterOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Indented = false
    };

    private readonly string _canonicalString;

    private DesignConversionReferenceEvidencePayload(
        string domainTag,
        string subjectId,
        string patternId,
        string role,
        DesignConversionReferenceVisualAxisEvidence? visualAxis,
        DesignConversionReferenceMarkupAxisEvidence? markupAxis,
        string canonicalString)
    {
        DomainTag = domainTag;
        SubjectId = subjectId;
        PatternId = patternId;
        Role = role;
        VisualAxis = visualAxis;
        MarkupAxis = markupAxis;
        _canonicalString = canonicalString;
    }

    public string DomainTag { get; }

    public string SubjectId { get; }

    public string PatternId { get; }

    public string Role { get; }

    public DesignConversionReferenceVisualAxisEvidence? VisualAxis { get; }

    public DesignConversionReferenceMarkupAxisEvidence? MarkupAxis { get; }

    public static OperationResult<DesignConversionReferenceEvidencePayload> Create(
        string domainTag,
        string subjectId,
        string patternId,
        string role,
        DesignConversionReferenceVisualAxisEvidence? visualAxis,
        DesignConversionReferenceMarkupAxisEvidence? markupAxis)
    {
        var normalizedDomain = NormalizeIdentity(domainTag, "DESIGN_CONVERSION_REFERENCE_DOMAIN_INVALID");
        var normalizedSubject = NormalizeIdentity(subjectId, "DESIGN_CONVERSION_REFERENCE_SUBJECT_INVALID");
        var normalizedPattern = NormalizeIdentity(patternId, "DESIGN_CONVERSION_REFERENCE_PATTERN_INVALID");
        var normalizedRole = NormalizeIdentity(role, "DESIGN_CONVERSION_REFERENCE_ROLE_INVALID");
        if (!normalizedDomain.IsSuccess || normalizedDomain.Value is null)
            return normalizedDomain.MapError<DesignConversionReferenceEvidencePayload>();
        if (!normalizedSubject.IsSuccess || normalizedSubject.Value is null)
            return normalizedSubject.MapError<DesignConversionReferenceEvidencePayload>();
        if (!normalizedPattern.IsSuccess || normalizedPattern.Value is null)
            return normalizedPattern.MapError<DesignConversionReferenceEvidencePayload>();
        if (!normalizedRole.IsSuccess || normalizedRole.Value is null)
            return normalizedRole.MapError<DesignConversionReferenceEvidencePayload>();

        var normalizedVisual = NormalizeVisualAxis(visualAxis);
        if (!normalizedVisual.IsSuccess)
            return normalizedVisual.MapError<DesignConversionReferenceEvidencePayload>();
        var normalizedMarkup = NormalizeMarkupAxis(markupAxis);
        if (!normalizedMarkup.IsSuccess)
            return normalizedMarkup.MapError<DesignConversionReferenceEvidencePayload>();

        var canonical = WriteCanonicalString(
            normalizedDomain.Value,
            normalizedSubject.Value,
            normalizedPattern.Value,
            normalizedRole.Value,
            normalizedVisual.Value,
            normalizedMarkup.Value);
        if (canonical.Length > MaximumPayloadChars)
        {
            return Failure(
                "DesignConversion reference evidence payload exceeds the frozen-reference content limit.",
                "DESIGN_CONVERSION_REFERENCE_PAYLOAD_TOO_LARGE");
        }

        return OperationResult<DesignConversionReferenceEvidencePayload>.Ok(
            new DesignConversionReferenceEvidencePayload(
                normalizedDomain.Value,
                normalizedSubject.Value,
                normalizedPattern.Value,
                normalizedRole.Value,
                normalizedVisual.Value,
                normalizedMarkup.Value,
                canonical));
    }

    public static OperationResult<DesignConversionReferenceEvidencePayload> Parse(
        string payload)
    {
        var normalizedPayload = NormalizePayloadContent(payload);
        if (!normalizedPayload.IsSuccess || normalizedPayload.Value is null)
            return normalizedPayload.MapError<DesignConversionReferenceEvidencePayload>();
        if (normalizedPayload.Value.Length > MaximumPayloadChars)
        {
            return Failure(
                "DesignConversion reference evidence payload exceeds the frozen-reference content limit.",
                "DESIGN_CONVERSION_REFERENCE_PAYLOAD_TOO_LARGE");
        }

        try
        {
            using var document = JsonDocument.Parse(
                normalizedPayload.Value,
                StrictJsonDocumentOptions);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return Failure(
                    "DesignConversion reference evidence payload must be a JSON object.",
                    "DESIGN_CONVERSION_REFERENCE_PAYLOAD_SHAPE_INVALID");
            }

            var properties = root.EnumerateObject().ToArray();
            if (properties.Length != 7 ||
                !StringComparer.Ordinal.Equals(properties[0].Name, "schema") ||
                !StringComparer.Ordinal.Equals(properties[1].Name, "domain_tag") ||
                !StringComparer.Ordinal.Equals(properties[2].Name, "subject_id") ||
                !StringComparer.Ordinal.Equals(properties[3].Name, "pattern_id") ||
                !StringComparer.Ordinal.Equals(properties[4].Name, "role") ||
                !StringComparer.Ordinal.Equals(properties[5].Name, "visual_axis") ||
                !StringComparer.Ordinal.Equals(properties[6].Name, "markup_axis"))
            {
                return Failure(
                    "DesignConversion reference evidence payload must use the canonical schema, domain_tag, subject_id, pattern_id, role, visual_axis, markup_axis property order with no extras.",
                    "DESIGN_CONVERSION_REFERENCE_PAYLOAD_SHAPE_INVALID");
            }

            if (properties[0].Value.ValueKind != JsonValueKind.String ||
                !StringComparer.Ordinal.Equals(properties[0].Value.GetString(), Schema))
            {
                return Failure(
                    "DesignConversion reference evidence payload schema is not supported.",
                    "DESIGN_CONVERSION_REFERENCE_PAYLOAD_SCHEMA_INVALID");
            }

            if (properties[1].Value.ValueKind != JsonValueKind.String ||
                properties[2].Value.ValueKind != JsonValueKind.String ||
                properties[3].Value.ValueKind != JsonValueKind.String ||
                properties[4].Value.ValueKind != JsonValueKind.String)
            {
                return Failure(
                    "DesignConversion reference evidence payload has invalid scalar value types.",
                    "DESIGN_CONVERSION_REFERENCE_PAYLOAD_SHAPE_INVALID");
            }

            var visualAxis = ParseVisualAxis(properties[5].Value);
            if (!visualAxis.IsSuccess)
                return visualAxis.MapError<DesignConversionReferenceEvidencePayload>();
            var markupAxis = ParseMarkupAxis(properties[6].Value);
            if (!markupAxis.IsSuccess)
                return markupAxis.MapError<DesignConversionReferenceEvidencePayload>();

            return Create(
                properties[1].Value.GetString()!,
                properties[2].Value.GetString()!,
                properties[3].Value.GetString()!,
                properties[4].Value.GetString()!,
                visualAxis.Value,
                markupAxis.Value);
        }
        catch (JsonException)
        {
            return Failure(
                "DesignConversion reference evidence payload is malformed JSON.",
                "DESIGN_CONVERSION_REFERENCE_PAYLOAD_MALFORMED");
        }
    }

    public string ToCanonicalString() => _canonicalString;

    private static OperationResult<DesignConversionReferenceVisualAxisEvidence?>
        ParseVisualAxis(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
            return OperationResult<DesignConversionReferenceVisualAxisEvidence?>.Ok(null);
        if (element.ValueKind != JsonValueKind.Object)
        {
            return OperationResult<DesignConversionReferenceVisualAxisEvidence?>.Fail(
                "DesignConversion visual axis must be an object or null.",
                "DESIGN_CONVERSION_REFERENCE_VISUAL_AXIS_SHAPE_INVALID");
        }

        var properties = element.EnumerateObject().ToArray();
        if (properties.Length != 12 ||
            !StringComparer.Ordinal.Equals(properties[0].Name, "trust_boundary") ||
            !StringComparer.Ordinal.Equals(properties[1].Name, "trust_classification") ||
            !StringComparer.Ordinal.Equals(properties[2].Name, "provider_kind") ||
            !StringComparer.Ordinal.Equals(properties[3].Name, "comparison_request_id") ||
            !StringComparer.Ordinal.Equals(properties[4].Name, "expected_image_id") ||
            !StringComparer.Ordinal.Equals(properties[5].Name, "expected_image_sha256") ||
            !StringComparer.Ordinal.Equals(properties[6].Name, "actual_image_id") ||
            !StringComparer.Ordinal.Equals(properties[7].Name, "actual_image_sha256") ||
            !StringComparer.Ordinal.Equals(properties[8].Name, "diff_image_id") ||
            !StringComparer.Ordinal.Equals(properties[9].Name, "diff_image_sha256") ||
            !StringComparer.Ordinal.Equals(properties[10].Name, "evidence_refs") ||
            !StringComparer.Ordinal.Equals(properties[11].Name, "score"))
        {
            return OperationResult<DesignConversionReferenceVisualAxisEvidence?>.Fail(
                "DesignConversion visual axis must use the canonical linked-field shape with no extras.",
                "DESIGN_CONVERSION_REFERENCE_VISUAL_AXIS_SHAPE_INVALID");
        }

        for (var index = 0; index < 10; index++)
        {
            if (properties[index].Value.ValueKind != JsonValueKind.String)
            {
                return OperationResult<DesignConversionReferenceVisualAxisEvidence?>.Fail(
                    "DesignConversion visual axis linked identities must be strings.",
                    "DESIGN_CONVERSION_REFERENCE_VISUAL_AXIS_SHAPE_INVALID");
            }
        }

        if (properties[10].Value.ValueKind != JsonValueKind.Array ||
            !properties[11].Value.TryGetDouble(out var score))
        {
            return OperationResult<DesignConversionReferenceVisualAxisEvidence?>.Fail(
                "DesignConversion visual axis evidence refs and score have invalid value types.",
                "DESIGN_CONVERSION_REFERENCE_VISUAL_AXIS_SHAPE_INVALID");
        }

        var refs = new List<string>();
        foreach (var refElement in properties[10].Value.EnumerateArray())
        {
            if (refElement.ValueKind != JsonValueKind.String)
            {
                return OperationResult<DesignConversionReferenceVisualAxisEvidence?>.Fail(
                    "DesignConversion visual evidence refs must be strings.",
                    "DESIGN_CONVERSION_REFERENCE_EVIDENCE_REF_INVALID");
            }

            refs.Add(refElement.GetString()!);
        }

        return NormalizeVisualAxis(
            new DesignConversionReferenceVisualAxisEvidence(
                properties[0].Value.GetString()!,
                properties[1].Value.GetString()!,
                properties[2].Value.GetString()!,
                properties[3].Value.GetString()!,
                properties[4].Value.GetString()!,
                properties[5].Value.GetString()!,
                properties[6].Value.GetString()!,
                properties[7].Value.GetString()!,
                properties[8].Value.GetString()!,
                properties[9].Value.GetString()!,
                refs,
                score));
    }

    private static OperationResult<DesignConversionReferenceMarkupAxisEvidence?>
        ParseMarkupAxis(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
            return OperationResult<DesignConversionReferenceMarkupAxisEvidence?>.Ok(null);
        if (element.ValueKind != JsonValueKind.Object)
        {
            return OperationResult<DesignConversionReferenceMarkupAxisEvidence?>.Fail(
                "DesignConversion markup axis must be an object or null.",
                "DESIGN_CONVERSION_REFERENCE_MARKUP_AXIS_SHAPE_INVALID");
        }

        var properties = element.EnumerateObject().ToArray();
        if (properties.Length != 4 ||
            !StringComparer.Ordinal.Equals(properties[0].Name, "expected_markup_fingerprint") ||
            !StringComparer.Ordinal.Equals(properties[1].Name, "node_type_composition_score") ||
            !StringComparer.Ordinal.Equals(properties[2].Name, "nesting_depth_score") ||
            !StringComparer.Ordinal.Equals(properties[3].Name, "anchor_values_score") ||
            properties[0].Value.ValueKind != JsonValueKind.String ||
            !properties[1].Value.TryGetDouble(out var composition) ||
            !properties[2].Value.TryGetDouble(out var nesting) ||
            !properties[3].Value.TryGetDouble(out var anchors))
        {
            return OperationResult<DesignConversionReferenceMarkupAxisEvidence?>.Fail(
                "DesignConversion markup axis must carry expected markup fingerprint and the three canonical numeric scores.",
                "DESIGN_CONVERSION_REFERENCE_MARKUP_AXIS_SHAPE_INVALID");
        }

        return NormalizeMarkupAxis(
            new DesignConversionReferenceMarkupAxisEvidence(
                properties[0].Value.GetString()!,
                composition,
                nesting,
                anchors));
    }

    private static OperationResult<DesignConversionReferenceVisualAxisEvidence?>
        NormalizeVisualAxis(DesignConversionReferenceVisualAxisEvidence? axis)
    {
        if (axis is null)
            return OperationResult<DesignConversionReferenceVisualAxisEvidence?>.Ok(null);

        var trustBoundary = NormalizeIdentity(axis.TrustBoundary, "DESIGN_CONVERSION_REFERENCE_VISUAL_TRUST_BOUNDARY_INVALID");
        var trustClassification = NormalizeIdentity(axis.TrustClassification, "DESIGN_CONVERSION_REFERENCE_VISUAL_TRUST_CLASSIFICATION_INVALID");
        var providerKind = NormalizeIdentity(axis.ProviderKind, "DESIGN_CONVERSION_REFERENCE_VISUAL_PROVIDER_KIND_INVALID");
        var comparisonRequestId = NormalizeIdentity(axis.ComparisonRequestId, "DESIGN_CONVERSION_REFERENCE_VISUAL_COMPARISON_REQUEST_INVALID");
        var expectedImageId = NormalizeIdentity(axis.ExpectedImageId, "DESIGN_CONVERSION_REFERENCE_VISUAL_EXPECTED_IMAGE_INVALID");
        var actualImageId = NormalizeIdentity(axis.ActualImageId, "DESIGN_CONVERSION_REFERENCE_VISUAL_ACTUAL_IMAGE_INVALID");
        var diffImageId = NormalizeIdentity(axis.DiffImageId, "DESIGN_CONVERSION_REFERENCE_VISUAL_DIFF_IMAGE_INVALID");
        if (!trustBoundary.IsSuccess || trustBoundary.Value is null)
            return trustBoundary.MapError<DesignConversionReferenceVisualAxisEvidence?>();
        if (!trustClassification.IsSuccess || trustClassification.Value is null)
            return trustClassification.MapError<DesignConversionReferenceVisualAxisEvidence?>();
        if (!providerKind.IsSuccess || providerKind.Value is null)
            return providerKind.MapError<DesignConversionReferenceVisualAxisEvidence?>();
        if (!comparisonRequestId.IsSuccess || comparisonRequestId.Value is null)
            return comparisonRequestId.MapError<DesignConversionReferenceVisualAxisEvidence?>();
        if (!expectedImageId.IsSuccess || expectedImageId.Value is null)
            return expectedImageId.MapError<DesignConversionReferenceVisualAxisEvidence?>();
        if (!actualImageId.IsSuccess || actualImageId.Value is null)
            return actualImageId.MapError<DesignConversionReferenceVisualAxisEvidence?>();
        if (!diffImageId.IsSuccess || diffImageId.Value is null)
            return diffImageId.MapError<DesignConversionReferenceVisualAxisEvidence?>();

        var expectedSha = NormalizeSha256(axis.ExpectedImageSha256);
        var actualSha = NormalizeSha256(axis.ActualImageSha256);
        var diffSha = NormalizeSha256(axis.DiffImageSha256);
        if (!expectedSha.IsSuccess || expectedSha.Value is null)
            return expectedSha.MapError<DesignConversionReferenceVisualAxisEvidence?>();
        if (!actualSha.IsSuccess || actualSha.Value is null)
            return actualSha.MapError<DesignConversionReferenceVisualAxisEvidence?>();
        if (!diffSha.IsSuccess || diffSha.Value is null)
            return diffSha.MapError<DesignConversionReferenceVisualAxisEvidence?>();

        if (StringComparer.Ordinal.Equals(expectedImageId.Value, actualImageId.Value))
        {
            return OperationResult<DesignConversionReferenceVisualAxisEvidence?>.Fail(
                "DesignConversion visual axis must compare a candidate image to a separate expected reference image.",
                "DESIGN_CONVERSION_REFERENCE_VISUAL_SELF_COMPARISON_INVALID");
        }

        if (StringComparer.Ordinal.Equals(expectedSha.Value, actualSha.Value))
        {
            return OperationResult<DesignConversionReferenceVisualAxisEvidence?>.Fail(
                "DesignConversion visual axis must not use the expected image bytes as the rendered candidate bytes.",
                "DESIGN_CONVERSION_REFERENCE_VISUAL_SELF_HASH_INVALID");
        }

        var evidenceRefs = NormalizeEvidenceRefs(axis.EvidenceRefs);
        if (!evidenceRefs.IsSuccess || evidenceRefs.Value is null)
            return evidenceRefs.MapError<DesignConversionReferenceVisualAxisEvidence?>();
        var score = NormalizeScore(axis.Score, "DESIGN_CONVERSION_REFERENCE_VISUAL_SCORE_INVALID");
        if (!score.IsSuccess || score.Value is null)
            return score.MapError<DesignConversionReferenceVisualAxisEvidence?>();

        return OperationResult<DesignConversionReferenceVisualAxisEvidence?>.Ok(
            new DesignConversionReferenceVisualAxisEvidence(
                trustBoundary.Value,
                trustClassification.Value,
                providerKind.Value,
                comparisonRequestId.Value,
                expectedImageId.Value,
                expectedSha.Value,
                actualImageId.Value,
                actualSha.Value,
                diffImageId.Value,
                diffSha.Value,
                evidenceRefs.Value,
                score.Value.Value));
    }

    private static OperationResult<DesignConversionReferenceMarkupAxisEvidence?>
        NormalizeMarkupAxis(DesignConversionReferenceMarkupAxisEvidence? axis)
    {
        if (axis is null)
            return OperationResult<DesignConversionReferenceMarkupAxisEvidence?>.Ok(null);

        var expectedMarkupFingerprint = NormalizeSha256(axis.ExpectedMarkupFingerprint);
        var composition = NormalizeScore(
            axis.NodeTypeCompositionScore,
            "DESIGN_CONVERSION_REFERENCE_MARKUP_COMPOSITION_SCORE_INVALID");
        var nesting = NormalizeScore(
            axis.NestingDepthScore,
            "DESIGN_CONVERSION_REFERENCE_MARKUP_NESTING_SCORE_INVALID");
        var anchors = NormalizeScore(
            axis.AnchorValuesScore,
            "DESIGN_CONVERSION_REFERENCE_MARKUP_ANCHOR_SCORE_INVALID");
        if (!expectedMarkupFingerprint.IsSuccess || expectedMarkupFingerprint.Value is null)
            return expectedMarkupFingerprint.MapError<DesignConversionReferenceMarkupAxisEvidence?>();
        if (!composition.IsSuccess || composition.Value is null)
            return composition.MapError<DesignConversionReferenceMarkupAxisEvidence?>();
        if (!nesting.IsSuccess || nesting.Value is null)
            return nesting.MapError<DesignConversionReferenceMarkupAxisEvidence?>();
        if (!anchors.IsSuccess || anchors.Value is null)
            return anchors.MapError<DesignConversionReferenceMarkupAxisEvidence?>();

        return OperationResult<DesignConversionReferenceMarkupAxisEvidence?>.Ok(
            new DesignConversionReferenceMarkupAxisEvidence(
                expectedMarkupFingerprint.Value,
                composition.Value.Value,
                nesting.Value.Value,
                anchors.Value.Value));
    }

    private static OperationResult<IReadOnlyList<string>> NormalizeEvidenceRefs(
        IEnumerable<string?> evidenceRefs)
    {
        if (evidenceRefs is null)
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "DesignConversion visual evidence refs are required.",
                "DESIGN_CONVERSION_REFERENCE_EVIDENCE_REFS_REQUIRED");
        }

        var normalized = new List<string>();
        try
        {
            foreach (var evidenceRef in evidenceRefs)
            {
                var normalizedRef = NormalizeIdentity(
                    evidenceRef,
                    "DESIGN_CONVERSION_REFERENCE_EVIDENCE_REF_INVALID");
                if (!normalizedRef.IsSuccess || normalizedRef.Value is null)
                    return normalizedRef.MapError<IReadOnlyList<string>>();

                normalized.Add(normalizedRef.Value);
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "DesignConversion visual evidence refs could not be snapshotted.",
                "DESIGN_CONVERSION_REFERENCE_EVIDENCE_REFS_SNAPSHOT_FAILED");
        }

        if (normalized.Count == 0)
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "DesignConversion visual evidence refs must be explicit.",
                "DESIGN_CONVERSION_REFERENCE_EVIDENCE_REFS_REQUIRED");
        }

        var duplicate = normalized
            .GroupBy(static value => value, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1)
            ?.Key;
        if (duplicate is not null)
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "DesignConversion visual evidence refs must be unique.",
                "DESIGN_CONVERSION_REFERENCE_EVIDENCE_REF_DUPLICATE");
        }

        return OperationResult<IReadOnlyList<string>>.Ok(
            Array.AsReadOnly(
                normalized
                    .OrderBy(static value => value, StringComparer.Ordinal)
                    .ToArray()));
    }

    private static OperationResult<double?> NormalizeScore(
        double score,
        string errorCode)
    {
        if (!double.IsFinite(score) || score < 0.0 || score > 1.0)
        {
            return OperationResult<double?>.Fail(
                "DesignConversion reference score must be finite and within 0..1.",
                errorCode);
        }

        return OperationResult<double?>.Ok(score);
    }

    private static OperationResult<string> NormalizeSha256(string? value)
    {
        if (value is not { Length: 71 } ||
            !value.StartsWith("sha256:", StringComparison.Ordinal) ||
            value.AsSpan("sha256:".Length).IndexOfAnyExcept("0123456789abcdef") >= 0)
        {
            return OperationResult<string>.Fail(
                "DesignConversion visual SHA-256 must be a canonical sha256 ref.",
                "DESIGN_CONVERSION_REFERENCE_SHA256_INVALID");
        }

        return OperationResult<string>.Ok(value);
    }

    private static OperationResult<string> NormalizeIdentity(
        string? value,
        string errorCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return OperationResult<string>.Fail(
                "DesignConversion reference identity is required.",
                errorCode);
        }

        string normalized;
        try
        {
            normalized = value.Normalize(NormalizationForm.FormC);
        }
        catch (ArgumentException)
        {
            return OperationResult<string>.Fail(
                "DesignConversion reference identity must be valid Unicode.",
                errorCode);
        }

        if (!StringComparer.Ordinal.Equals(normalized, normalized.Trim()) ||
            normalized.Length > MaximumIdentityChars ||
            !char.IsAsciiLetterOrDigit(normalized[0]) ||
            normalized.Any(static character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not ('-' or '_' or '.' or ':')))
        {
            return OperationResult<string>.Fail(
                "DesignConversion reference identity is blank or unsafe.",
                errorCode);
        }

        return OperationResult<string>.Ok(normalized);
    }

    private static OperationResult<string> NormalizePayloadContent(string? value)
    {
        if (value is null)
        {
            return OperationResult<string>.Fail(
                "DesignConversion reference evidence payload is required.",
                "DESIGN_CONVERSION_REFERENCE_PAYLOAD_INVALID");
        }

        string normalized;
        try
        {
            normalized = value
                .Normalize(NormalizationForm.FormC)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');
        }
        catch (ArgumentException)
        {
            return OperationResult<string>.Fail(
                "DesignConversion reference evidence payload must be valid Unicode.",
                "DESIGN_CONVERSION_REFERENCE_PAYLOAD_INVALID");
        }

        if (normalized.Any(static character =>
                char.IsControl(character) && character is not ('\n' or '\t')))
        {
            return OperationResult<string>.Fail(
                "DesignConversion reference evidence payload content is unsafe.",
                "DESIGN_CONVERSION_REFERENCE_PAYLOAD_INVALID");
        }

        return OperationResult<string>.Ok(normalized);
    }

    private static string WriteCanonicalString(
        string domainTag,
        string subjectId,
        string patternId,
        string role,
        DesignConversionReferenceVisualAxisEvidence? visualAxis,
        DesignConversionReferenceMarkupAxisEvidence? markupAxis)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer, CanonicalJsonWriterOptions))
        {
            writer.WriteStartObject();
            writer.WriteString("schema", Schema);
            writer.WriteString("domain_tag", domainTag);
            writer.WriteString("subject_id", subjectId);
            writer.WriteString("pattern_id", patternId);
            writer.WriteString("role", role);
            WriteVisualAxis(writer, visualAxis);
            WriteMarkupAxis(writer, markupAxis);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static void WriteVisualAxis(
        Utf8JsonWriter writer,
        DesignConversionReferenceVisualAxisEvidence? visualAxis)
    {
        writer.WritePropertyName("visual_axis");
        if (visualAxis is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteString("trust_boundary", visualAxis.TrustBoundary);
        writer.WriteString("trust_classification", visualAxis.TrustClassification);
        writer.WriteString("provider_kind", visualAxis.ProviderKind);
        writer.WriteString("comparison_request_id", visualAxis.ComparisonRequestId);
        writer.WriteString("expected_image_id", visualAxis.ExpectedImageId);
        writer.WriteString("expected_image_sha256", visualAxis.ExpectedImageSha256);
        writer.WriteString("actual_image_id", visualAxis.ActualImageId);
        writer.WriteString("actual_image_sha256", visualAxis.ActualImageSha256);
        writer.WriteString("diff_image_id", visualAxis.DiffImageId);
        writer.WriteString("diff_image_sha256", visualAxis.DiffImageSha256);
        writer.WritePropertyName("evidence_refs");
        writer.WriteStartArray();
        foreach (var evidenceRef in visualAxis.EvidenceRefs)
            writer.WriteStringValue(evidenceRef);
        writer.WriteEndArray();
        writer.WriteNumber("score", visualAxis.Score);
        writer.WriteEndObject();
    }

    private static void WriteMarkupAxis(
        Utf8JsonWriter writer,
        DesignConversionReferenceMarkupAxisEvidence? markupAxis)
    {
        writer.WritePropertyName("markup_axis");
        if (markupAxis is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteString("expected_markup_fingerprint", markupAxis.ExpectedMarkupFingerprint);
        writer.WriteNumber("node_type_composition_score", markupAxis.NodeTypeCompositionScore);
        writer.WriteNumber("nesting_depth_score", markupAxis.NestingDepthScore);
        writer.WriteNumber("anchor_values_score", markupAxis.AnchorValuesScore);
        writer.WriteEndObject();
    }

    private static OperationResult<DesignConversionReferenceEvidencePayload> Failure(
        string message,
        string code)
        => OperationResult<DesignConversionReferenceEvidencePayload>.Fail(message, code);
}

/// <summary>
/// Frozen-reference matching rule for DesignConversion design-to-code output.
/// It is a prerequisite instrument only: it checks admitted visual evidence and
/// separate markup scores, but it does not create paid corpora, DPO rows, model
/// calls, or training authority.
/// </summary>
public static class DesignConversionReferenceMatchingRule
{
    public const string DomainTag = "design-conversion";
    public const string SubjectId = "design-tree-to-markup";
    public const string PatternId = "design-to-code";
    public const string StudentCandidateRole = "student_candidate";
    public const string PaidReferenceRole = "paid_reference";
    public const double VisualScoreThreshold = 0.94;
    public const double NodeTypeCompositionThreshold = 0.90;
    public const double NestingDepthThreshold = 0.90;
    public const double AnchorValuesThreshold = 0.90;
    public const string RuleId = "design-conversion-design-to-code-two-axis-reference-match";
    public const string RuleVersion =
        "v3.schema-dc-ref-v1.domain-design-conversion.subject-design-tree-to-markup.pattern-design-to-code.roles-student_candidate-paid_reference.visual-0.94.markup-0.90.bind-img-id-sha.markup-fingerprint.trust-local-png-only";
    public const string RuleIdentityMismatchErrorCode =
        "DESIGN_CONVERSION_REFERENCE_MATCHING_RULE_IDENTITY_MISMATCH";
    public const string CorpusSubjectMismatchErrorCode =
        "DESIGN_CONVERSION_REFERENCE_MATCHING_CORPUS_SUBJECT_MISMATCH";
    public const string MatchStatus = "match";
    public const string IncompleteMarkupAxisMissingStatus = "incomplete_markup_axis_missing";
    public const string VisualTrustAbsentStatus = "visual_contract_absent_or_unrecognized";
    public const string RoleMismatchStatus = "role_mismatch_or_swapped";
    public const string EnvelopeMismatchStatus = "schema_domain_subject_pattern_mismatch";
    public const string ExpectedReferenceMismatchStatus =
        "candidate_expected_reference_mismatch";
    public const string ExpectedMarkupMismatchStatus =
        "candidate_expected_markup_fingerprint_mismatch";
    public const string NodeTypeCompositionBelowThresholdStatus =
        "markup_node_type_composition_below_threshold";
    public const string NestingDepthBelowThresholdStatus =
        "markup_nesting_depth_below_threshold";
    public const string AnchorValuesBelowThresholdStatus =
        "markup_anchor_values_below_threshold";

    public static string SupportedTrustBoundary =>
        DesignConversionComparisonEvidenceTrustBoundary.TrustBoundary;

    public static string SupportedLocalPngTrustClassification =>
        DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedLocalPngClassification;

    public static string SupportedExternalVisualAiTrustClassification =>
        DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification;

    public static OperationResult<ReferenceMatchingRuleIdentity> CreateIdentity()
        => ReferenceMatchingRuleIdentity.Create(
            DomainTag,
            RuleId,
            RuleVersion,
            "DesignConversion design-to-code frozen-reference prerequisite matcher. It consumes caller-serialized canonicalized trust-boundary output; arbitrary JSON is not server-authenticated by this matcher. It requires a student_candidate payload against a paid_reference payload, server-owned local PNG comparison contract refs bound to the axis fields, the candidate comparison expected image id and SHA-256 bound to the frozen paid_reference expected image, matching expected markup fingerprint, candidate visual score >= 0.94, and candidate markup composition, nesting/depth, and anchor-values scores >= 0.90. Equal files, equal hashes, copied markers, prose, or visual_comparison_proven metadata alone are not authority. This is not a paid corpus, not DPO, not training, and not a model improvement claim.");

    public static ReferenceMatchingRulePredicate CreatePredicate()
        => IsMatch;

    public static bool IsMatch(
        string candidateAnswer,
        string referenceAnswer)
    {
        var result = EvaluateMatch(candidateAnswer, referenceAnswer);
        return result.IsSuccess && result.Value is { IsMatch: true };
    }

    public static OperationResult<DesignConversionReferenceMatchResult> EvaluateMatch(
        string candidateAnswer,
        string referenceAnswer)
    {
        var candidate = DesignConversionReferenceEvidencePayload.Parse(candidateAnswer);
        if (!candidate.IsSuccess || candidate.Value is null)
            return candidate.MapError<DesignConversionReferenceMatchResult>();
        var reference = DesignConversionReferenceEvidencePayload.Parse(referenceAnswer);
        if (!reference.IsSuccess || reference.Value is null)
            return reference.MapError<DesignConversionReferenceMatchResult>();

        var emptyVisual = VisualMissingStatus();
        var emptyMarkup = MarkupMissingStatus();
        if (!HasExpectedEnvelope(candidate.Value) ||
            !HasExpectedEnvelope(reference.Value))
        {
            return OperationResult<DesignConversionReferenceMatchResult>.Ok(
                new DesignConversionReferenceMatchResult(
                    false,
                    EnvelopeMismatchStatus,
                    emptyVisual,
                    emptyVisual,
                    emptyMarkup,
                    emptyMarkup));
        }

        if (!StringComparer.Ordinal.Equals(candidate.Value.Role, StudentCandidateRole) ||
            !StringComparer.Ordinal.Equals(reference.Value.Role, PaidReferenceRole))
        {
            return OperationResult<DesignConversionReferenceMatchResult>.Ok(
                new DesignConversionReferenceMatchResult(
                    false,
                    RoleMismatchStatus,
                    emptyVisual,
                    emptyVisual,
                    emptyMarkup,
                    emptyMarkup));
        }

        var candidateVisual = EvaluateVisualAxis(candidate.Value.VisualAxis);
        var referenceVisual = EvaluateVisualAxis(reference.Value.VisualAxis);
        var candidateMarkup = EvaluateMarkupAxis(candidate.Value.MarkupAxis);
        var referenceMarkup = EvaluateMarkupAxis(reference.Value.MarkupAxis);
        var expectedReferenceBindingStatus = ResolveExpectedReferenceBindingStatus(
            candidate.Value.VisualAxis,
            reference.Value.VisualAxis);
        var expectedMarkupBindingStatus = ResolveExpectedMarkupBindingStatus(
            candidate.Value.MarkupAxis,
            reference.Value.MarkupAxis);
        var status = ResolveOverallStatus(
            candidateVisual,
            referenceVisual,
            candidateMarkup,
            referenceMarkup,
            expectedReferenceBindingStatus,
            expectedMarkupBindingStatus);

        return OperationResult<DesignConversionReferenceMatchResult>.Ok(
            new DesignConversionReferenceMatchResult(
                StringComparer.Ordinal.Equals(status, MatchStatus),
                status,
                candidateVisual,
                referenceVisual,
                candidateMarkup,
                referenceMarkup));
    }

    public static OperationResult<ReferenceMatchingRuleCalibrationResult>
        EvaluateCalibration(ReferenceMatchingCalibrationInput input)
    {
        if (input is null)
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "DesignConversion reference matching calibration input is required.",
                "DESIGN_CONVERSION_REFERENCE_MATCHING_CALIBRATION_INPUT_REQUIRED");
        }

        var expectedIdentity = CreateIdentity();
        if (!expectedIdentity.IsSuccess || expectedIdentity.Value is null)
            return expectedIdentity.MapError<ReferenceMatchingRuleCalibrationResult>();

        if (!StringComparer.Ordinal.Equals(
                input.RuleIdentity.IdentitySha256,
                expectedIdentity.Value.IdentitySha256))
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Calibration input rule identity does not match the DesignConversion two-axis frozen-reference policy.",
                RuleIdentityMismatchErrorCode);
        }

        if (!StringComparer.Ordinal.Equals(input.Corpus.SubjectId, SubjectId))
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Calibration input frozen corpus subject does not match the DesignConversion two-axis frozen-reference policy.",
                CorpusSubjectMismatchErrorCode);
        }

        return ReferenceMatchingRuleCalibrationEvaluator.Evaluate(
            input,
            CreatePredicate());
    }

    private static bool HasExpectedEnvelope(DesignConversionReferenceEvidencePayload payload)
        => StringComparer.Ordinal.Equals(payload.DomainTag, DomainTag) &&
           StringComparer.Ordinal.Equals(payload.SubjectId, SubjectId) &&
           StringComparer.Ordinal.Equals(payload.PatternId, PatternId);

    private static DesignConversionReferenceVisualAxisStatus EvaluateVisualAxis(
        DesignConversionReferenceVisualAxisEvidence? axis)
    {
        if (axis is null)
            return VisualMissingStatus();

        if (!IsSupportedVisualTrust(axis))
        {
            return new DesignConversionReferenceVisualAxisStatus(
                true,
                false,
                false,
                axis.Score,
                VisualScoreThreshold,
                VisualTrustAbsentStatus);
        }

        if (axis.Score < VisualScoreThreshold)
        {
            return new DesignConversionReferenceVisualAxisStatus(
                true,
                true,
                false,
                axis.Score,
                VisualScoreThreshold,
                "visual_score_below_threshold");
        }

        return new DesignConversionReferenceVisualAxisStatus(
            true,
            true,
            true,
            axis.Score,
            VisualScoreThreshold,
            "visual_axis_passed");
    }

    private static DesignConversionReferenceMarkupAxisStatus EvaluateMarkupAxis(
        DesignConversionReferenceMarkupAxisEvidence? axis)
    {
        if (axis is null)
            return MarkupMissingStatus();

        if (axis.NodeTypeCompositionScore < NodeTypeCompositionThreshold)
            return MarkupStatus(axis, false, NodeTypeCompositionBelowThresholdStatus);
        if (axis.NestingDepthScore < NestingDepthThreshold)
            return MarkupStatus(axis, false, NestingDepthBelowThresholdStatus);
        if (axis.AnchorValuesScore < AnchorValuesThreshold)
            return MarkupStatus(axis, false, AnchorValuesBelowThresholdStatus);

        return MarkupStatus(axis, true, "markup_axis_passed");
    }

    private static string ResolveOverallStatus(
        DesignConversionReferenceVisualAxisStatus candidateVisual,
        DesignConversionReferenceVisualAxisStatus referenceVisual,
        DesignConversionReferenceMarkupAxisStatus candidateMarkup,
        DesignConversionReferenceMarkupAxisStatus referenceMarkup,
        string? expectedReferenceBindingStatus,
        string? expectedMarkupBindingStatus)
    {
        if (!candidateVisual.Passed)
            return candidateVisual.Status;
        if (!referenceVisual.Passed)
            return referenceVisual.Status;
        if (expectedReferenceBindingStatus is not null)
            return expectedReferenceBindingStatus;
        if (!candidateMarkup.Passed)
            return candidateMarkup.Status;
        if (!referenceMarkup.Passed)
            return referenceMarkup.Status;
        if (expectedMarkupBindingStatus is not null)
            return expectedMarkupBindingStatus;

        return MatchStatus;
    }

    private static string? ResolveExpectedReferenceBindingStatus(
        DesignConversionReferenceVisualAxisEvidence? candidateVisualAxis,
        DesignConversionReferenceVisualAxisEvidence? referenceVisualAxis)
    {
        if (candidateVisualAxis is null || referenceVisualAxis is null)
            return null;

        if (!StringComparer.Ordinal.Equals(
                candidateVisualAxis.ExpectedImageId,
                referenceVisualAxis.ExpectedImageId) ||
            !StringComparer.Ordinal.Equals(
                candidateVisualAxis.ExpectedImageSha256,
                referenceVisualAxis.ExpectedImageSha256))
        {
            return ExpectedReferenceMismatchStatus;
        }

        return null;
    }

    private static string? ResolveExpectedMarkupBindingStatus(
        DesignConversionReferenceMarkupAxisEvidence? candidateMarkupAxis,
        DesignConversionReferenceMarkupAxisEvidence? referenceMarkupAxis)
    {
        if (candidateMarkupAxis is null || referenceMarkupAxis is null)
            return null;

        if (!StringComparer.Ordinal.Equals(
                candidateMarkupAxis.ExpectedMarkupFingerprint,
                referenceMarkupAxis.ExpectedMarkupFingerprint))
        {
            return ExpectedMarkupMismatchStatus;
        }

        return null;
    }

    private static bool IsSupportedVisualTrust(DesignConversionReferenceVisualAxisEvidence axis)
        => StringComparer.Ordinal.Equals(axis.TrustBoundary, SupportedTrustBoundary) &&
           IsSupportedLocalPng(axis);

    private static bool IsSupportedLocalPng(DesignConversionReferenceVisualAxisEvidence axis)
        => StringComparer.Ordinal.Equals(
               axis.TrustClassification,
               SupportedLocalPngTrustClassification) &&
           StringComparer.Ordinal.Equals(
               axis.ProviderKind,
               ServerOwnedLocalPngComparisonProvider.ProviderKind) &&
            axis.EvidenceRefs.Contains(
                "SERVER_OWNED_LOCAL_PNG_COMPARISON_TRUST_CANONICALIZED",
                StringComparer.Ordinal) &&
            axis.EvidenceRefs.Contains(
                ServerOwnedLocalPngComparisonProvider.ProviderKind,
                StringComparer.Ordinal) &&
            axis.EvidenceRefs.Contains(
                ServerOwnedLocalPngComparisonProvider.Boundary,
                StringComparer.Ordinal) &&
            axis.EvidenceRefs.Contains(
                $"local-png-comparison:{axis.ComparisonRequestId}",
                StringComparer.Ordinal) &&
            axis.EvidenceRefs.Contains(
                $"expected-sha256:{axis.ExpectedImageSha256}",
                StringComparer.Ordinal) &&
            axis.EvidenceRefs.Contains(
                $"rendered-sha256:{axis.ActualImageSha256}",
                StringComparer.Ordinal) &&
            axis.EvidenceRefs.Contains(
                $"diff-sha256:{axis.DiffImageSha256}",
                StringComparer.Ordinal);

    private static bool IsSupportedExternalVisualAiReceipt(DesignConversionReferenceVisualAxisEvidence axis)
        => StringComparer.Ordinal.Equals(
               axis.TrustClassification,
               SupportedExternalVisualAiTrustClassification) &&
           StringComparer.Ordinal.Equals(
               axis.ProviderKind,
               DesignConversionExternalVisualAiComparisonProvider.ProviderKind) &&
           axis.EvidenceRefs.Contains(
               DesignConversionExternalVisualAiReceiptValidator.ValidatedEvidenceRef,
               StringComparer.Ordinal);

    private static DesignConversionReferenceVisualAxisStatus VisualMissingStatus()
        => new(
            false,
            false,
            false,
            null,
            VisualScoreThreshold,
            "incomplete_visual_axis_missing");

    private static DesignConversionReferenceMarkupAxisStatus MarkupMissingStatus()
        => new(
            false,
            false,
            null,
            null,
            null,
            NodeTypeCompositionThreshold,
            NestingDepthThreshold,
            AnchorValuesThreshold,
            IncompleteMarkupAxisMissingStatus);

    private static DesignConversionReferenceMarkupAxisStatus MarkupStatus(
        DesignConversionReferenceMarkupAxisEvidence axis,
        bool passed,
        string status)
        => new(
            true,
            passed,
            axis.NodeTypeCompositionScore,
            axis.NestingDepthScore,
            axis.AnchorValuesScore,
            NodeTypeCompositionThreshold,
            NestingDepthThreshold,
            AnchorValuesThreshold,
            status);
}
