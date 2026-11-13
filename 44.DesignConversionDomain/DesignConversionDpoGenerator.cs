// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using ContinuousLearning;
using DPO.Models;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public static class DesignConversionDpoUnits
{
    public const string CriticUnit = "CriticUnit";
    public const string ImproverUnit = "ImproverUnit";

    public static bool TryNormalize(string unitId, out string normalizedUnitId)
    {
        normalizedUnitId = string.Empty;
        if (string.IsNullOrWhiteSpace(unitId))
            return false;

        if (string.Equals(unitId, CriticUnit, StringComparison.Ordinal)
            || unitId.Contains("critic", StringComparison.OrdinalIgnoreCase))
        {
            normalizedUnitId = CriticUnit;
            return true;
        }

        if (string.Equals(unitId, ImproverUnit, StringComparison.Ordinal)
            || unitId.Contains("improver", StringComparison.OrdinalIgnoreCase))
        {
            normalizedUnitId = ImproverUnit;
            return true;
        }

        return false;
    }
}

public sealed record DesignConversionSourceEncoding(
    string TargetUnitId,
    string MachinePart,
    string SourceKind,
    string InternalUnitStatePayload,
    IReadOnlyList<double> InternalVectorPayload,
    string SourceStructurePayload,
    string ResultFeaturePayload,
    IReadOnlyList<string> RedactionCodes,
    IReadOnlyDictionary<string, string> Metadata);

public interface IDesignConversionSourceEncoder
{
    OperationResult<DesignConversionSourceEncoding> Encode(
        DesignConversionStoredDpoExample example,
        string normalizedUnitId);
}

public sealed class DesignConversionDefaultSourceEncoder : IDesignConversionSourceEncoder
{
    public const string EncoderName = "DesignConversionDefaultSourceEncoder";
    public const string PayloadSchema = "dcm-source-encoder-v1";

    private static readonly Regex HtmlTagRegex = new(
        @"<\s*[a-z][a-z0-9-]*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex HtmlClassRegex = new(
        @"\bclass\s*=",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex CssClassRegex = new(
        @"\.[A-Za-z_][A-Za-z0-9_-]*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex CssRuleRegex = new(
        @"\{",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex StyleHintRegex = new(
        @"(?i)(display|grid|flex|gap|padding|margin|width|height|color|font|border|position|z-index|transform)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly JsonSerializerOptions CompactJsonOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    public OperationResult<DesignConversionSourceEncoding> Encode(
        DesignConversionStoredDpoExample example,
        string normalizedUnitId)
    {
        if (example is null)
            return OperationResult<DesignConversionSourceEncoding>.Fail(
                "DesignConversion DPO example is required for source encoding.",
                "DESIGN_CONVERSION_SOURCE_ENCODING_EXAMPLE_REQUIRED");

        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var validation = new List<string>();
        var sourceKind = DesignConversionDpoLeakageGuard.SanitizeScalar(
            example.Provenance?.SourceKind ?? "unknown",
            "source_kind",
            redactions);
        var internalUnitStatePayload = DesignConversionDpoLeakageGuard.SanitizeJsonPayload(
            example.InternalUnitStatePayload,
            "internal_unit_state",
            redactions);
        var sourceStructurePayload = DesignConversionDpoLeakageGuard.SanitizeJsonPayload(
            example.SourceStructurePayload,
            "source_structure",
            redactions);
        var vectorEncoding = BuildInternalVectorEncoding(
            example,
            sourceStructurePayload,
            normalizedUnitId,
            redactions);
        var vector = vectorEncoding.Vector;
        var resultFeaturePayload = BuildResultFeaturePayload(example, normalizedUnitId, redactions);

        if (string.IsNullOrWhiteSpace(normalizedUnitId))
            validation.Add("target_unit_required");
        if (string.IsNullOrWhiteSpace(sourceStructurePayload))
            validation.Add("source_structure_required");
        if (vector.Count == 0)
            validation.Add("internal_vector_required");

        if (DesignConversionDpoLeakageGuard.ContainsForbiddenLeakage(
                internalUnitStatePayload,
                sourceStructurePayload,
                resultFeaturePayload,
                sourceKind))
        {
            validation.Add("source_encoder_forbidden_leakage_remains");
        }

        if (validation.Count > 0)
        {
            return OperationResult<DesignConversionSourceEncoding>.Fail(
                $"DesignConversion source encoding failed: {string.Join(";", validation)}.",
                "DESIGN_CONVERSION_SOURCE_ENCODING_INVALID");
        }

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["source_encoder"] = EncoderName,
            ["source_encoder_schema"] = PayloadSchema,
            ["source_encoder_role"] = "sanitizes_source_structure_provenance_vector_for_dpo",
            ["source_encoder_authority"] = "deterministic_preprocessing_not_trainable_authority",
            ["source_encoder_feature_namespace"] = "source_structure_provenance_only",
            ["result_feature_namespace"] = "result_features",
            ["anti_leakage"] = "forbidden_source_fields_redacted_before_train_ready_payload",
            ["source_encoder_dimensions"] = vector.Count.ToString(),
            ["source_encoder_feature_names"] = string.Join(",", vectorEncoding.FeatureNames),
            ["source_encoder_vector_hash"] = vectorEncoding.VectorHash,
            ["source_encoder_feature_map_hash"] = vectorEncoding.FeatureMapHash,
            ["source_structure_signal_summary"] = vectorEncoding.SourceStructureSummary,
            ["result_signal_summary"] = vectorEncoding.ResultSummary,
            ["source_encoder_shortcut_guard"] = "source_kind_source_id_not_vector_features",
            ["source_kind_in_vector"] = bool.FalseString,
            ["source_id_in_vector"] = bool.FalseString,
            ["result_features_in_source_vector"] = bool.FalseString,
            ["visual_score_margin_in_source_vector"] = bool.FalseString,
            ["usefulness_score_in_source_vector"] = bool.FalseString,
            ["answer_payload_in_source_vector"] = bool.FalseString,
            ["provided_internal_vector_in_source_vector"] = bool.FalseString,
            ["provided_internal_vector_role"] = "ignored_untrusted_not_source_authority",
            ["redaction_count"] = redactions.Count.ToString()
        };

        if (redactions.Count > 0)
            metadata["redaction_codes"] = string.Join(",", redactions);

        return OperationResult<DesignConversionSourceEncoding>.Ok(new DesignConversionSourceEncoding(
            TargetUnitId: normalizedUnitId,
            MachinePart: ToMachinePart(normalizedUnitId),
            SourceKind: string.IsNullOrWhiteSpace(sourceKind) ? "unknown" : sourceKind,
            InternalUnitStatePayload: internalUnitStatePayload,
            InternalVectorPayload: vector,
            SourceStructurePayload: sourceStructurePayload,
            ResultFeaturePayload: resultFeaturePayload,
            RedactionCodes: redactions.ToArray(),
            Metadata: metadata));
    }

    public static string ToMachinePart(string unitId)
    {
        if (string.Equals(unitId, DesignConversionDpoUnits.CriticUnit, StringComparison.Ordinal))
            return "design_conversion_critic_unit";
        if (string.Equals(unitId, DesignConversionDpoUnits.ImproverUnit, StringComparison.Ordinal))
            return "design_conversion_improver_unit";

        return "design_conversion_unknown_unit";
    }

    private static SourceVectorEncoding BuildInternalVectorEncoding(
        DesignConversionStoredDpoExample example,
        string sanitizedSourceStructurePayload,
        string normalizedUnitId,
        SortedSet<string> redactions)
    {
        var sourceMetrics = ExtractSourceStructureMetrics(sanitizedSourceStructurePayload);
        var chosenMetrics = ExtractResultSignalMetrics(example.Chosen);
        var rejectedMetrics = ExtractResultSignalMetrics(example.Rejected);
        var features = new List<KeyValuePair<string, double>>();

        AddFeature(features, "target_unit_is_critic", string.Equals(normalizedUnitId, DesignConversionDpoUnits.CriticUnit, StringComparison.Ordinal) ? 1 : 0);
        AddFeature(features, "target_unit_is_improver", string.Equals(normalizedUnitId, DesignConversionDpoUnits.ImproverUnit, StringComparison.Ordinal) ? 1 : 0);
        AddFeature(features, "source_json_node_count", ScaleCount(sourceMetrics.NodeCount, 256));
        AddFeature(features, "source_json_max_depth", ScaleCount(sourceMetrics.MaxDepth, 32));
        AddFeature(features, "source_object_property_count", ScaleCount(sourceMetrics.PropertyCount, 256));
        AddFeature(features, "source_array_item_count", ScaleCount(sourceMetrics.ArrayItemCount, 256));
        AddFeature(features, "source_frame_like_count", ScaleCount(sourceMetrics.FrameLikeCount, 64));
        AddFeature(features, "source_text_like_count", ScaleCount(sourceMetrics.TextLikeCount, 64));
        AddFeature(features, "source_image_like_count", ScaleCount(sourceMetrics.ImageLikeCount, 64));
        AddFeature(features, "source_component_like_count", ScaleCount(sourceMetrics.ComponentLikeCount, 64));
        AddFeature(features, "source_layout_hint_count", ScaleCount(sourceMetrics.LayoutHintCount, 128));
        AddFeature(features, "source_breakpoint_hint_count", ScaleCount(sourceMetrics.BreakpointHintCount, 32));
        AddFeature(features, "source_size_hint_count", ScaleCount(sourceMetrics.SizeHintCount, 128));

        var featureNames = features.Select(feature => feature.Key).ToArray();
        var vector = features.Select(feature => Math.Round(feature.Value, 6)).ToArray();
        var featureMap = JsonSerializer.Serialize(featureNames, CompactJsonOptions);
        var vectorPayload = JsonSerializer.Serialize(vector, CompactJsonOptions);

        return new SourceVectorEncoding(
            Vector: vector,
            FeatureNames: featureNames,
            VectorHash: DesignConversionDpoLeakageGuard.HashRef("dcm-source-encoder-vector", vectorPayload),
            FeatureMapHash: DesignConversionDpoLeakageGuard.HashRef("dcm-source-encoder-feature-map", featureMap),
            SourceStructureSummary: BuildSourceStructureSummary(sourceMetrics),
            ResultSummary: BuildResultSignalSummary(chosenMetrics, rejectedMetrics));
    }

    private static void AddFeature(
        ICollection<KeyValuePair<string, double>> features,
        string name,
        double value)
        => features.Add(new KeyValuePair<string, double>(
            name,
            double.IsFinite(value) ? value : 0));

    private static double ScaleCount(double value, double cap)
        => Math.Round(Math.Clamp(value, 0, cap) / cap, 6);

    private static double Clamp01(double value)
        => Math.Round(Math.Clamp(double.IsFinite(value) ? value : 0, 0, 1), 6);

    private static string BuildResultFeaturePayload(
        DesignConversionStoredDpoExample example,
        string normalizedUnitId,
        SortedSet<string> redactions)
        => JsonSerializer.Serialize(new
        {
            schema = "dcm-source-encoder-result-features-v1",
            target = new
            {
                unit_id = normalizedUnitId,
                machine_part = ToMachinePart(normalizedUnitId)
            },
            chosen = ToResultFeature(example.Chosen, "chosen", redactions),
            rejected = ToResultFeature(example.Rejected, "rejected", redactions),
            preference = new
            {
                pair_family = DesignConversionDpoLeakageGuard.SanitizeScalar(
                    string.IsNullOrWhiteSpace(example.NegativeReasonTaxonomy)
                        ? "unknown"
                        : example.NegativeReasonTaxonomy,
                    "negative_reason_taxonomy",
                    redactions),
                negative_quality_reason_hash = DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-negative-quality",
                    example.NegativeQualityReason ?? string.Empty),
                visual_score_margin = example.Chosen is null || example.Rejected is null
                    ? 0
                    : Math.Round(example.Chosen.Verdict.Score - example.Rejected.Verdict.Score, 6)
            }
        }, CompactJsonOptions);

    private static object ToResultFeature(
        DesignConversionImprovementOutput? output,
        string label,
        SortedSet<string> redactions)
    {
        if (output is null)
        {
            return new
            {
                result_ref = $"{label}:missing",
                visual_score = 0.0,
                accepted = false,
                defect_categories = Array.Empty<string>()
            };
        }

        return new
        {
            result_ref = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-result", output.OutputId),
            candidate_ref = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-candidate", output.CandidateId),
            html_ref = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-html", output.Html),
            css_ref = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-css", output.Css),
            html_tag_count = HtmlTagRegex.Matches(output.Html ?? string.Empty).Count,
            html_class_count = HtmlClassRegex.Matches(output.Html ?? string.Empty).Count,
            css_rule_count = CssRuleRegex.Matches(output.Css ?? string.Empty).Count,
            css_class_count = CssClassRegex.Matches(output.Css ?? string.Empty).Count,
            style_hint_count = StyleHintRegex.Matches($"{output.Html} {output.Css}").Count,
            visual_score = output.Verdict.Score,
            accepted = output.Accepted,
            defect_categories = output.Verdict.Defects
                .Select(defect => DesignConversionDpoLeakageGuard.SanitizeScalar(
                    defect.Category,
                    $"{label}_defect_category",
                    redactions))
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static SourceStructureMetrics ExtractSourceStructureMetrics(string payload)
    {
        var builder = new SourceStructureMetricsBuilder();
        if (string.IsNullOrWhiteSpace(payload))
            return builder.ToMetrics();

        try
        {
            WalkSourceNode(JsonNode.Parse(payload), 1, builder);
        }
        catch (JsonException)
        {
            builder.NodeCount++;
        }

        return builder.ToMetrics();
    }

    private static void WalkSourceNode(
        JsonNode? node,
        int depth,
        SourceStructureMetricsBuilder builder)
    {
        if (node is null)
            return;

        builder.NodeCount++;
        builder.MaxDepth = Math.Max(builder.MaxDepth, depth);

        if (node is JsonObject obj)
        {
            builder.PropertyCount += obj.Count;
            foreach (var pair in obj)
            {
                InspectSourceStructureToken(pair.Key, builder);
                WalkSourceNode(pair.Value, depth + 1, builder);
            }

            return;
        }

        if (node is JsonArray array)
        {
            builder.ArrayItemCount += array.Count;
            for (var i = 0; i < array.Count; i++)
                WalkSourceNode(array[i], depth + 1, builder);
            return;
        }

        var element = node.GetValue<JsonElement>();
        if (element.ValueKind == JsonValueKind.String)
            InspectSourceStructureToken(element.GetString() ?? string.Empty, builder);
    }

    private static void InspectSourceStructureToken(
        string token,
        SourceStructureMetricsBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        var normalized = token.Replace("-", "_", StringComparison.Ordinal).ToLowerInvariant();
        if (normalized.Contains("frame", StringComparison.Ordinal)
            || normalized.Contains("section", StringComparison.Ordinal)
            || normalized.Contains("group", StringComparison.Ordinal))
        {
            builder.FrameLikeCount++;
        }

        if (normalized.Contains("text", StringComparison.Ordinal)
            || normalized.Contains("label", StringComparison.Ordinal)
            || normalized.Contains("copy", StringComparison.Ordinal))
        {
            builder.TextLikeCount++;
        }

        if (normalized.Contains("image", StringComparison.Ordinal)
            || normalized.Contains("icon", StringComparison.Ordinal)
            || normalized.Contains("asset", StringComparison.Ordinal))
        {
            builder.ImageLikeCount++;
        }

        if (normalized.Contains("component", StringComparison.Ordinal)
            || normalized.Contains("button", StringComparison.Ordinal)
            || normalized.Contains("instance", StringComparison.Ordinal))
        {
            builder.ComponentLikeCount++;
        }

        if (normalized.Contains("layout", StringComparison.Ordinal)
            || normalized.Contains("constraint", StringComparison.Ordinal)
            || normalized.Contains("grid", StringComparison.Ordinal)
            || normalized.Contains("flex", StringComparison.Ordinal)
            || normalized.Contains("gap", StringComparison.Ordinal)
            || normalized.Contains("padding", StringComparison.Ordinal))
        {
            builder.LayoutHintCount++;
        }

        if (normalized.Contains("breakpoint", StringComparison.Ordinal)
            || normalized.Contains("viewport", StringComparison.Ordinal)
            || normalized.Contains("desktop", StringComparison.Ordinal)
            || normalized.Contains("mobile", StringComparison.Ordinal))
        {
            builder.BreakpointHintCount++;
        }

        if (normalized is "x" or "y" or "w" or "h" or "width" or "height"
            || normalized.Contains("bounds", StringComparison.Ordinal)
            || normalized.Contains("size", StringComparison.Ordinal))
        {
            builder.SizeHintCount++;
        }
    }

    private static ResultSignalMetrics ExtractResultSignalMetrics(DesignConversionImprovementOutput? output)
    {
        if (output is null)
            return new ResultSignalMetrics();

        var html = output.Html ?? string.Empty;
        var css = output.Css ?? string.Empty;
        return new ResultSignalMetrics
        {
            HtmlLength = html.Length,
            CssLength = css.Length,
            HtmlTagCount = HtmlTagRegex.Matches(html).Count,
            HtmlClassCount = HtmlClassRegex.Matches(html).Count,
            CssRuleCount = CssRuleRegex.Matches(css).Count,
            CssClassCount = CssClassRegex.Matches(css).Count,
            StyleHintCount = StyleHintRegex.Matches($"{html} {css}").Count,
            VisualScore = output.Verdict?.Score ?? 0,
            DefectCount = output.Verdict?.Defects?.Count ?? 0,
            Accepted = output.Accepted
        };
    }

    private static string BuildSourceStructureSummary(SourceStructureMetrics metrics)
        => string.Join(";",
        [
            $"nodes={metrics.NodeCount}",
            $"depth={metrics.MaxDepth}",
            $"properties={metrics.PropertyCount}",
            $"arrays={metrics.ArrayItemCount}",
            $"layout_hints={metrics.LayoutHintCount}",
            $"breakpoint_hints={metrics.BreakpointHintCount}",
            $"size_hints={metrics.SizeHintCount}"
        ]);

    private static string BuildResultSignalSummary(
        ResultSignalMetrics chosen,
        ResultSignalMetrics rejected)
        => string.Join(";",
        [
            $"chosen_score={Math.Round(chosen.VisualScore, 6)}",
            $"rejected_score={Math.Round(rejected.VisualScore, 6)}",
            $"visual_score_margin={Math.Round(chosen.VisualScore - rejected.VisualScore, 6)}",
            $"chosen_tags={chosen.HtmlTagCount}",
            $"rejected_tags={rejected.HtmlTagCount}",
            $"chosen_css_rules={chosen.CssRuleCount}",
            $"rejected_css_rules={rejected.CssRuleCount}"
        ]);

    private sealed record SourceVectorEncoding(
        IReadOnlyList<double> Vector,
        IReadOnlyList<string> FeatureNames,
        string VectorHash,
        string FeatureMapHash,
        string SourceStructureSummary,
        string ResultSummary);

    private sealed record SourceStructureMetrics(
        int NodeCount,
        int MaxDepth,
        int PropertyCount,
        int ArrayItemCount,
        int FrameLikeCount,
        int TextLikeCount,
        int ImageLikeCount,
        int ComponentLikeCount,
        int LayoutHintCount,
        int BreakpointHintCount,
        int SizeHintCount);

    private sealed class SourceStructureMetricsBuilder
    {
        public int NodeCount { get; set; }

        public int MaxDepth { get; set; }

        public int PropertyCount { get; set; }

        public int ArrayItemCount { get; set; }

        public int FrameLikeCount { get; set; }

        public int TextLikeCount { get; set; }

        public int ImageLikeCount { get; set; }

        public int ComponentLikeCount { get; set; }

        public int LayoutHintCount { get; set; }

        public int BreakpointHintCount { get; set; }

        public int SizeHintCount { get; set; }

        public SourceStructureMetrics ToMetrics()
            => new(
                NodeCount,
                MaxDepth,
                PropertyCount,
                ArrayItemCount,
                FrameLikeCount,
                TextLikeCount,
                ImageLikeCount,
                ComponentLikeCount,
                LayoutHintCount,
                BreakpointHintCount,
                SizeHintCount);
    }

    private sealed record ResultSignalMetrics
    {
        public int HtmlLength { get; init; }

        public int CssLength { get; init; }

        public int HtmlTagCount { get; init; }

        public int HtmlClassCount { get; init; }

        public int CssRuleCount { get; init; }

        public int CssClassCount { get; init; }

        public int StyleHintCount { get; init; }

        public double VisualScore { get; init; }

        public int DefectCount { get; init; }

        public bool Accepted { get; init; }
    }
}

public static class DesignConversionDpoLeakageGuard
{
    private static readonly JsonSerializerOptions CompactJsonOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    // Live-local тесты в WSL держат server-owned runtime под /tmp; это тоже
    // локальная инфраструктура, а не публичное доказательство для клиента.
    private static readonly Regex LocalPathRegex = new(
        @"(?i)(file:///?[A-Za-z]:[\\/][^\s""']+|(?<![A-Za-z])[A-Za-z]:[\\/][^\s""']+|\\\\[^\s""']+|/(Users|home|tmp|var/tmp|mnt)/[^\s""']+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex UserIdentifierRegex = new(
        @"(?i)(user[-_:][A-Za-z0-9._-]+|[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}|lubao|luba)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex RawBytesRegex = new(
        @"(?i)(data:image/[a-z0-9.+-]+;base64,[A-Za-z0-9+/=]{40,}|[A-Za-z0-9+/=]{160,})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string SanitizeJsonPayload(
        string? payload,
        string scope,
        SortedSet<string> redactions)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return string.Empty;

        try
        {
            var node = JsonNode.Parse(payload);
            var sanitized = SanitizeNode(node, scope, redactions);
            return sanitized?.ToJsonString(CompactJsonOptions) ?? "{}";
        }
        catch (JsonException)
        {
            redactions.Add($"{scope}:non_json_payload_hashed");
            return JsonSerializer.Serialize(new
            {
                schema = "dcm-redacted-non-json-payload-v1",
                payload_hash = HashRef("dcm-payload", payload),
                redacted = true
            }, CompactJsonOptions);
        }
    }

    public static IReadOnlyDictionary<string, string> SanitizeMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        string scope,
        SortedSet<string> redactions)
    {
        var sanitized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            var keyCode = ClassifyForbiddenKey(pair.Key);
            if (keyCode is not null)
            {
                redactions.Add($"{scope}:{keyCode}");
                continue;
            }

            sanitized[pair.Key] = SanitizeScalar(pair.Value, $"{scope}.{pair.Key}", redactions);
        }

        return sanitized;
    }

    public static string SanitizeScalar(
        string? value,
        string scope,
        SortedSet<string> redactions)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var sanitized = LocalPathRegex.Replace(value, match =>
        {
            redactions.Add($"{scope}:local_path");
            return HashRef("redacted-local-path", match.Value);
        });
        sanitized = RawBytesRegex.Replace(sanitized, match =>
        {
            redactions.Add($"{scope}:binary_payload");
            return HashRef("redacted-binary", match.Value);
        });
        sanitized = UserIdentifierRegex.Replace(sanitized, match =>
        {
            redactions.Add($"{scope}:user_identifier");
            return HashRef("redacted-user", match.Value);
        });

        return sanitized;
    }

    public static bool ContainsForbiddenLeakage(params string?[] values)
        => values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Any(value =>
                LocalPathRegex.IsMatch(value!) ||
                RawBytesRegex.IsMatch(value!) ||
                value!.Contains("exact_prompt", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("candidate_index", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("candidate_order", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("transient_run_id", StringComparison.OrdinalIgnoreCase));

    public static string HashRef(string prefix, string? value)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)))
            .ToLowerInvariant();
        return $"{prefix}-{hash[..16]}";
    }

    private static JsonNode? SanitizeNode(
        JsonNode? node,
        string scope,
        SortedSet<string> redactions)
    {
        if (node is null)
            return null;

        if (node is JsonObject obj)
        {
            var sanitized = new JsonObject();
            foreach (var pair in obj)
            {
                var keyCode = ClassifyForbiddenKey(pair.Key);
                if (keyCode is not null)
                {
                    redactions.Add($"{scope}:{keyCode}");
                    continue;
                }

                sanitized[pair.Key] = SanitizeNode(pair.Value, $"{scope}.{pair.Key}", redactions);
            }

            if (sanitized.Count == 0 && obj.Count > 0)
                sanitized["redacted"] = true;

            return sanitized;
        }

        if (node is JsonArray array)
        {
            var sanitized = new JsonArray();
            for (var i = 0; i < array.Count; i++)
                sanitized.Add(SanitizeNode(array[i], $"{scope}[{i}]", redactions));

            return sanitized;
        }

        var element = node.GetValue<JsonElement>();
        return element.ValueKind == JsonValueKind.String
            ? JsonValue.Create(SanitizeScalar(element.GetString(), scope, redactions))
            : node.DeepClone();
    }

    private static string? ClassifyForbiddenKey(string key)
    {
        var normalized = (key ?? string.Empty).Replace("-", "_", StringComparison.Ordinal).ToLowerInvariant();
        if (normalized.Contains("exact_prompt", StringComparison.Ordinal) ||
            normalized is "prompt" or "prompt_text" or "raw_prompt")
        {
            return "prompt_text";
        }

        if (normalized.Contains("candidate_index", StringComparison.Ordinal) ||
            normalized.Contains("candidate_order", StringComparison.Ordinal) ||
            normalized.Contains("candidate_position", StringComparison.Ordinal))
        {
            return "candidate_position";
        }

        if (normalized.Contains("raw_screenshot", StringComparison.Ordinal) ||
            normalized.Contains("screenshot_bytes", StringComparison.Ordinal) ||
            normalized is "bytes" or "raw_bytes")
        {
            return "binary_payload";
        }

        if (normalized.Contains("transient_run_id", StringComparison.Ordinal) ||
            normalized.Contains("session_id", StringComparison.Ordinal) ||
            normalized is "run_id" or "request_id" or "source_request_id")
        {
            return "transient_id";
        }

        if (normalized is "user" or "user_id" or "userid" or "reviewer_ref" ||
            normalized.Contains("user_id", StringComparison.Ordinal) ||
            normalized.Contains("reviewer_id", StringComparison.Ordinal))
        {
            return "user_identifier";
        }

        if (normalized.Contains("local_path", StringComparison.Ordinal) ||
            normalized.Contains("file_path", StringComparison.Ordinal) ||
            normalized.Contains("filename", StringComparison.Ordinal) ||
            normalized.EndsWith("_path", StringComparison.Ordinal))
        {
            return "local_path";
        }

        return null;
    }
}

public static class DesignConversionNegativeReasonTaxonomy
{
    public const string Spacing = "spacing";
    public const string Color = "color";
    public const string Typography = "typography";
    public const string Overflow = "overflow";
    public const string Responsive = "responsive";
    public const string OverflowResponsive = "overflow_responsive";
    public const string MissingElement = "missing_element";
    public const string ZOrder = "z_order";
    public const string ZOrderLayering = "z_order_layering";
    public const string AssetImageMismatch = "asset_image_mismatch";
    public const string SemanticLayoutHierarchy = "semantic_layout_hierarchy";
    public const string AccessibilityContrast = "accessibility_contrast";
    public const string PlatformNormalizationMismatch = "platform_normalization_mismatch";
    public const string TrivialBroken = "trivial_broken";
    public const string MultiError = "multi_error";
    public const string LeakageShortcut = "leakage_shortcut";
    public const string BlankRender = "blank_render";
    public const string RenderTimeout = "render_timeout";
    public const string AssetResolutionFailed = "asset_resolution_failed";
    public const string SyntheticRuntimeNotTrainReady = "synthetic_runtime_not_train_ready";

    private static readonly HashSet<string> AllowedValues = new(StringComparer.Ordinal)
    {
        Spacing,
        Color,
        Typography,
        Overflow,
        Responsive,
        OverflowResponsive,
        MissingElement,
        ZOrder,
        ZOrderLayering,
        AssetImageMismatch,
        SemanticLayoutHierarchy,
        AccessibilityContrast,
        PlatformNormalizationMismatch,
        TrivialBroken,
        MultiError,
        LeakageShortcut,
        BlankRender,
        RenderTimeout,
        AssetResolutionFailed,
        SyntheticRuntimeNotTrainReady
    };

    private static readonly HashSet<string> QuarantineOnlyValues = new(StringComparer.Ordinal)
    {
        TrivialBroken,
        MultiError,
        LeakageShortcut,
        BlankRender,
        RenderTimeout,
        AssetResolutionFailed,
        SyntheticRuntimeNotTrainReady
    };

    public static bool IsAllowed(string? value)
        => !string.IsNullOrWhiteSpace(value) && AllowedValues.Contains(value);

    public static bool IsQuarantineOnly(string? value)
        => !string.IsNullOrWhiteSpace(value) && QuarantineOnlyValues.Contains(value);
}

public sealed record DesignConversionNegativeQualityValidation(
    bool IsTrainReadyNearMiss,
    IReadOnlyList<string> Reasons);

public static class DesignConversionNegativeQualityValidator
{
    public static DesignConversionNegativeQualityValidation Validate(DesignConversionStoredDpoExample example)
    {
        var reasons = new List<string>();

        if (example is null)
        {
            reasons.Add("dpo_example_required");
            return new DesignConversionNegativeQualityValidation(false, reasons);
        }

        if (string.IsNullOrWhiteSpace(example.NegativeQualityReason))
            reasons.Add("negative_quality_reason_required");

        if (!DesignConversionNegativeReasonTaxonomy.IsAllowed(example.NegativeReasonTaxonomy))
        {
            reasons.Add("negative_reason_taxonomy_unknown");
        }
        else if (DesignConversionNegativeReasonTaxonomy.IsQuarantineOnly(example.NegativeReasonTaxonomy))
        {
            reasons.Add($"negative_reason_taxonomy_quarantine_only:{example.NegativeReasonTaxonomy}");
        }

        if (example.Chosen is not null && !example.Chosen.Accepted)
            reasons.Add("chosen_not_evidence_accepted");

        if (example.Rejected is not null && example.Rejected.Accepted)
            reasons.Add("rejected_not_evidence_rejected");

        if (example.Chosen is not null
            && example.Rejected is not null
            && string.Equals(example.Chosen.CandidateId, example.Rejected.CandidateId, StringComparison.Ordinal))
        {
            reasons.Add("chosen_rejected_candidate_id_same");
        }

        if (example.Rejected is not null && !IsRealisticRejected(example.Rejected))
            reasons.Add("rejected_not_realistic_negative");

        return new DesignConversionNegativeQualityValidation(reasons.Count == 0, reasons);
    }

    private static bool IsRealisticRejected(DesignConversionImprovementOutput output)
        => (!string.IsNullOrWhiteSpace(output.Html) || !string.IsNullOrWhiteSpace(output.Css))
            && output.EvidenceRefs.Count + output.QaResults.Count > 0
            && !output.Accepted;
}

public sealed record DesignConversionStoredDpoExample(
    string ExampleId,
    string UnitId,
    string InternalUnitStatePayload,
    IReadOnlyList<double> InternalVectorPayload,
    string SourceStructurePayload,
    DesignConversionImprovementOutput Chosen,
    DesignConversionImprovementOutput Rejected,
    string? TieBreakerPayload,
    double UsefulnessScore,
    string NegativeQualityReason,
    string NegativeReasonTaxonomy,
    DesignConversionDpoProvenance Provenance,
    DesignConversionLicenseScope License,
    IReadOnlyList<string> TeacherEvidenceRefs,
    IReadOnlyDictionary<string, string> Metadata,
    TrainingTeacherProvenance? TeacherProvenance = null)
{
    // Сохраненный example получает вид автора только как init-снимок:
    // направление выбирается выше, пока generated/incumbent стороны различимы.
    public DpoPairSideAuthorKind? ChosenAuthorKind { get; init; }

    // Вторая сторона хранится явно, чтобы bridge не гадал по score или тексту.
    public DpoPairSideAuthorKind? RejectedAuthorKind { get; init; }
}

public sealed record DesignConversionDpoBuildResult(
    [property: System.Text.Json.Serialization.JsonIgnore]
    IReadOnlyList<DesignConversionDpoTrainingRecord> AcceptedRecords,
    [property: System.Text.Json.Serialization.JsonIgnore]
    IReadOnlyList<DesignConversionDpoTrainingRecord> QuarantinedRecords,
    IReadOnlyList<string> Warnings)
{
    [System.Text.Json.Serialization.JsonIgnore]
    internal IReadOnlyList<DesignConversionDpoTrainingRecordSummary>?
        AcceptedSummaryOverride { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    internal IReadOnlyList<DesignConversionDpoTrainingRecordSummary>?
        QuarantinedSummaryOverride { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("safeProjectionSchemaVersion")]
    public string SafeProjectionSchemaVersion =>
        DesignConversionSafeTrainingProjection.SchemaVersion;

    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyList<DesignConversionDpoTrainingRecord> AllRecords =>
        AcceptedRecords.Concat(QuarantinedRecords).ToArray();

    [System.Text.Json.Serialization.JsonPropertyName("acceptedRecords")]
    public IReadOnlyList<DesignConversionDpoTrainingRecordSummary> AcceptedRecordSummaries =>
        AcceptedSummaryOverride ??
        AcceptedRecords.Select(DesignConversionSafeTrainingProjection.ToSummary).ToArray();

    [System.Text.Json.Serialization.JsonPropertyName("quarantinedRecords")]
    public IReadOnlyList<DesignConversionDpoTrainingRecordSummary> QuarantinedRecordSummaries =>
        QuarantinedSummaryOverride ??
        QuarantinedRecords.Select(DesignConversionSafeTrainingProjection.ToSummary).ToArray();

    [System.Text.Json.Serialization.JsonPropertyName("allRecords")]
    public IReadOnlyList<DesignConversionDpoTrainingRecordSummary> AllRecordSummaries =>
        AcceptedRecordSummaries.Concat(QuarantinedRecordSummaries).ToArray();
}

internal static class DesignConversionSafeTrainingProjection
{
    internal const string SchemaVersion =
        "design-conversion-safe-training-projection-v2";

    internal static bool IsSupportedSchemaVersion(string? schemaVersion)
        => string.Equals(schemaVersion, SchemaVersion, StringComparison.Ordinal);

    internal static DesignConversionDpoTrainingRecordSummary ToSummary(
        DesignConversionDpoTrainingRecord record)
    {
        var item = DesignConversionTrainingPipeline.ToCanonicalTrainingItem(record);
        return new DesignConversionDpoTrainingRecordSummary(
            record.RecordId,
            record.UnitId,
            record.Quarantine.IsQuarantined ? "quarantined" : "accepted",
            Read(record.Provenance.Metadata,
                DesignConversionDpoGenerator.PayloadCustodyRequiredMetadataKey)
                .Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
                    ? "hash_committed_or_pending"
                    : "not_required",
            DesignConversionDpoGenerator.Sha256Ref(item.Prompt),
            DesignConversionDpoGenerator.Sha256Ref(item.ChosenResponse),
            DesignConversionDpoGenerator.Sha256Ref(item.RejectedResponse),
            Read(record.Provenance.Metadata,
                DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey));
    }

    internal static DesignConversionLearningTrainingItemSummary ToSummary(
        LearningTrainingItem item)
        => new(
            item.ItemId,
            DesignConversionDpoGenerator.Sha256Ref(item.Prompt),
            DesignConversionDpoGenerator.Sha256Ref(item.ChosenResponse),
            DesignConversionDpoGenerator.Sha256Ref(item.RejectedResponse));

    private static string Read(
        IReadOnlyDictionary<string, string>? metadata,
        string key)
        => metadata is not null && metadata.TryGetValue(key, out var value)
            ? value ?? string.Empty
            : string.Empty;
}

public sealed class DesignConversionDpoGenerator
{
    internal const string PayloadCustodyVersion =
        "design-conversion-dpo-payload-custody-v1";
    internal const string PayloadCustodyRequiredMetadataKey =
        "payload_custody_required";
    internal const string PayloadCustodyVersionMetadataKey =
        "payload_custody_version";
    internal const string PayloadCustodyPromptSha256MetadataKey =
        "payload_custody_prompt_sha256";
    internal const string PayloadCustodyChosenSha256MetadataKey =
        "payload_custody_chosen_response_sha256";
    internal const string PayloadCustodyRejectedSha256MetadataKey =
        "payload_custody_rejected_response_sha256";
    internal const string PayloadCustodyGovernanceSha256MetadataKey =
        "payload_custody_governance_sha256";
    internal const string PayloadCustodyBindingSha256MetadataKey =
        "payload_custody_training_record_binding_sha256";
    // ReviewerRef and these fields are authority-workflow state, not immutable teacher payload
    // governance. They are deliberately excluded from the custody snapshot so
    // the separately audited local approval transition can change them without
    // silently rebinding teacher content. Custody commitment fields are excluded
    // because including them would make the governance hash recursive.
    private static readonly IReadOnlySet<string> MutableCustodyGovernanceMetadataKeys =
        new HashSet<string>(StringComparer.Ordinal)
        {
            PayloadCustodyRequiredMetadataKey,
            PayloadCustodyVersionMetadataKey,
            PayloadCustodyPromptSha256MetadataKey,
            PayloadCustodyChosenSha256MetadataKey,
            PayloadCustodyRejectedSha256MetadataKey,
            PayloadCustodyGovernanceSha256MetadataKey,
            PayloadCustodyBindingSha256MetadataKey,
            "payload_custody_raw_in_public_record",
            "review_required",
            "approval_required",
            "approval_status",
            "review_status",
            "approved_for_training",
            "approved_by_ref",
            "approval_reason",
            "approved_at_utc",
            "local_reviewer_approval_evidence"
        };
    // Approval writes these explicit false values when older candidate metadata
    // omitted them. Canonical defaults make absence and the same safe false value
    // equivalent, while any later true mutation changes the governance hash.
    private static readonly IReadOnlyDictionary<string, string>
        CustodyGovernanceMetadataDefaults =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["public_api_verified"] = bool.FalseString,
                ["production_db_ready"] = bool.FalseString,
                ["trained_model_ready"] = bool.FalseString
            };
    private const string RuntimeCandidateCanPersistFalseReason =
        "visual_decision_can_persist_training_candidate_false";
    private const string RuntimeCandidateCanPersistFalseWarning =
        "runtime_created_dpo_candidate_not_created_visual_decision_can_persist_training_candidate_false";
    private const string RuntimeRejectedCandidateEvidenceRequiredReason =
        "runtime_rejected_candidate_independent_evaluation_required";
    private const string RuntimeTeacherAttestationRequiredReason =
        "runtime_teacher_attestation_required";
    private static readonly IReadOnlySet<string> OperationalScaffoldMarkerKeys =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "service_first_no_asp_net_host",
            "render_provider_injected",
            "comparison_provider_injected",
            "visual_judge_provider_injected",
            "browser_renderer_provider_available",
            "browser_renderer_explicit_local_injection_proven",
            "browser_renderer_default_production_wired",
            "browser_renderer_default_scope",
            "browser_renderer_injected_provider_kind",
            "external_ai_visual_judge_provider_available",
            "external_ai_visual_judge_configured",
            "local_visual_judge_default_wired",
            "local_visual_judge_explicitly_injected",
            "visual_judge_not_configured_fallback",
            "persistent_storage_implemented",
            "deterministic_logic_claimed_as_learning",
            "active_model_runtime_selection",
            "active_model_runtime_selection_boundary",
            "active_model_runtime_tenant_context_source",
            "active_model_runtime_trusted_tenant_bound",
            "active_model_runtime_caller_tenant_metadata_authoritative",
            "active_model_runtime_fresh_load_attempted",
            "active_model_runtime_fresh_load_succeeded",
            "active_model_runtime_previous_preserved",
            "active_model_generation",
            "active_model_tenant_id_ref",
            "active_model_admitted_tenant_id_ref",
            "active_model_admitted_scope_ref",
            "active_model_id",
            "active_model_manifest_id",
            "active_model_version",
            "active_model_checkpoint_sha256",
            "active_model_checkpoint_path_ref",
            "active_model_checkpoint_weights_source",
            "active_model_checkpoint_fresh_load_verified",
            "active_model_checkpoint_real_weights",
            "active_model_local_only",
            "product_ready"
        };
    private static readonly IReadOnlySet<string> BooleanOperationalScaffoldMarkerKeys =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "service_first_no_asp_net_host",
            "render_provider_injected",
            "comparison_provider_injected",
            "visual_judge_provider_injected",
            "browser_renderer_provider_available",
            "browser_renderer_explicit_local_injection_proven",
            "browser_renderer_default_production_wired",
            "external_ai_visual_judge_provider_available",
            "external_ai_visual_judge_configured",
            "local_visual_judge_default_wired",
            "local_visual_judge_explicitly_injected",
            "visual_judge_not_configured_fallback",
            "persistent_storage_implemented",
            "deterministic_logic_claimed_as_learning",
            "active_model_runtime_trusted_tenant_bound",
            "active_model_runtime_caller_tenant_metadata_authoritative",
            "active_model_runtime_fresh_load_attempted",
            "active_model_runtime_fresh_load_succeeded",
            "active_model_runtime_previous_preserved",
            "active_model_checkpoint_fresh_load_verified",
            "active_model_checkpoint_real_weights",
            "active_model_local_only",
            "product_ready"
        };
    private static readonly IReadOnlySet<string> OperationalRenderProviderKinds =
        new HashSet<string>(StringComparer.Ordinal)
        {
            BrowserBackedDesignConversionRenderProvider.ProviderKind,
            DeterministicLocalDesignConversionPngRenderProvider.ProviderKind,
            DeterministicDesignConversionRenderProvider.ProviderKind
        };

    private static readonly JsonSerializerOptions CompactJsonOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    private static readonly string[] HiddenLearningLeakageTokens =
    [
        "deterministic_evidence_refs",
        "deterministic-input-evidence",
        "qa_data_role",
        "render_qa_as_learning",
        "source_request_id_as_feature"
    ];

    private readonly IDesignConversionSourceEncoder _sourceEncoder;

    public DesignConversionDpoGenerator(IDesignConversionSourceEncoder? sourceEncoder = null)
    {
        _sourceEncoder = sourceEncoder ?? new DesignConversionDefaultSourceEncoder();
    }

    public OperationResult<DesignConversionDpoBuildResult> BuildRuntimeCreatedTrainingCandidates(
        DesignConversionInternalEvaluationResult evaluation,
        DateTimeOffset? createdAtUtc = null)
    {
        if (evaluation is null)
            return OperationResult<DesignConversionDpoBuildResult>.Fail(
                "DesignConversion runtime evaluation is required to create DPO training candidates.",
                "DESIGN_CONVERSION_RUNTIME_EVALUATION_REQUIRED");

        var normalizedEvaluation = NormalizeMeasuredVisualScoreForDpo(evaluation);
        var evidenceGate = ValidateRuntimeCreatedCandidateEvidence(normalizedEvaluation);
        if (evidenceGate.Count > 0)
        {
            var reason = string.Join(";", evidenceGate);
            return OperationResult<DesignConversionDpoBuildResult>.Ok(new DesignConversionDpoBuildResult(
                AcceptedRecords: [],
                QuarantinedRecords: [],
                Warnings: BuildRuntimeCandidateNotCreatedWarnings(reason)));
        }

        if (!TryResolveRouteAdmissionBinding(
                normalizedEvaluation,
                out var routeBinding,
                out var routeBindingReason))
        {
            return OperationResult<DesignConversionDpoBuildResult>.Ok(
                new DesignConversionDpoBuildResult(
                    AcceptedRecords: [],
                    QuarantinedRecords: [],
                    Warnings: BuildRuntimeCandidateNotCreatedWarnings(
                        routeBindingReason)));
        }

        if (!routeBinding.TenantBound)
        {
            return OperationResult<DesignConversionDpoBuildResult>.Ok(new DesignConversionDpoBuildResult(
                AcceptedRecords: [],
                QuarantinedRecords: [],
                Warnings:
                    BuildRuntimeCandidateNotCreatedWarnings(
                        "server_owned_route_admission_not_bound")));
        }

        var serverOwnedBaselineAuthorityGate =
            ValidateServerOwnedSingleEvaluationBaselineAuthority(normalizedEvaluation);
        if (serverOwnedBaselineAuthorityGate.Count > 0)
        {
            return OperationResult<DesignConversionDpoBuildResult>.Ok(
                new DesignConversionDpoBuildResult(
                    AcceptedRecords: [],
                    QuarantinedRecords: [],
                    Warnings:
                        BuildRuntimeCandidateNotCreatedWarnings(
                            string.Join(";", serverOwnedBaselineAuthorityGate))));
        }

        if (!TryBuildServerOwnedSingleEvaluationBaselineEvidence(
                normalizedEvaluation,
                out var baselineEvidence,
                out var baselineEvidenceReason))
        {
            return OperationResult<DesignConversionDpoBuildResult>.Ok(
                new DesignConversionDpoBuildResult(
                    AcceptedRecords: [],
                    QuarantinedRecords: [],
                    Warnings:
                        BuildRuntimeCandidateNotCreatedWarnings(
                            baselineEvidenceReason)));
        }

        var observedAtUtc = createdAtUtc ?? DateTimeOffset.UtcNow;
        var record = BuildServerOwnedSingleEvaluationBaselineStoredExample(
            normalizedEvaluation,
            baselineEvidence,
            routeBinding,
            observedAtUtc);
        return OperationResult<DesignConversionDpoBuildResult>.Ok(new DesignConversionDpoBuildResult(
            AcceptedRecords: [record],
            QuarantinedRecords: [],
            Warnings: BuildRuntimeServerOwnedBaselineCreatedWarnings()));
    }

    public OperationResult<DesignConversionDpoBuildResult> BuildRuntimeCreatedTrainingCandidates(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionInternalEvaluationResult rejectedEvaluation,
        DateTimeOffset? createdAtUtc = null)
        => BuildRuntimeCreatedTrainingCandidatesCore(
            evaluation,
            rejectedEvaluation,
            createdAtUtc);

    internal OperationResult<DesignConversionDpoBuildResult>
        BuildRuntimeCreatedTrainingCandidates(
            DesignConversionInternalEvaluationResult evaluation,
            DesignConversionInternalEvaluationResult rejectedEvaluation,
            DesignConversionDpoTeacherAttestation teacherAttestation,
            DateTimeOffset? createdAtUtc = null)
        => OperationResult<DesignConversionDpoBuildResult>.Ok(
            new DesignConversionDpoBuildResult(
                AcceptedRecords: [],
                QuarantinedRecords: [],
                Warnings: BuildRuntimeTeacherAttestationNotCreatedWarnings(
                    "runtime_teacher_attestation_only_is_non_authoritative")));

    internal OperationResult<DesignConversionDpoBuildResult>
        BuildRuntimeTeacherAuthoredTrainingCandidates(
            DesignConversionInternalEvaluationResult generatedEvaluation,
            DesignConversionInternalEvaluationResult incumbentEvaluation,
            DesignConversionDpoTeacherCandidateAuthoringEvidence authoringEvidence,
            DateTimeOffset? createdAtUtc = null)
    {
        if (generatedEvaluation is null || incumbentEvaluation is null ||
            authoringEvidence is null)
        {
            return OperationResult<DesignConversionDpoBuildResult>.Fail(
                "DesignConversion teacher-authored evaluation pair is required.",
                "DESIGN_CONVERSION_TEACHER_AUTHORED_EVALUATION_PAIR_REQUIRED");
        }

        return BuildRuntimeTeacherAuthoredTrainingCandidates(
        [
            new DesignConversionTeacherAuthoredEvaluationPair(
                authoringEvidence.AttemptNumber,
                generatedEvaluation,
                incumbentEvaluation,
                authoringEvidence,
                authoringEvidence.GeneratedCandidateId)
        ],
        createdAtUtc);
    }

    internal OperationResult<DesignConversionDpoBuildResult>
        BuildRuntimeTeacherAuthoredTrainingCandidates(
            IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
                evaluationPairs,
            DateTimeOffset? createdAtUtc = null)
    {
        if (evaluationPairs is null || evaluationPairs.Count == 0 ||
            evaluationPairs.Count >
                LegalTrainingTeacherAdmissionAuthority.MaxBatchRows ||
            evaluationPairs.Any(static pair => pair is null))
        {
            return OperationResult<DesignConversionDpoBuildResult>.Fail(
                "A bounded DesignConversion teacher-authored evaluation batch is required.",
                "DESIGN_CONVERSION_TEACHER_AUTHORED_EVALUATION_PAIR_REQUIRED");
        }

        var orderedPairs = evaluationPairs
            .OrderBy(static pair => pair.AttemptNumber)
            .ToArray();
        var attemptNumbers = new HashSet<int>();
        var generatedBindings = new HashSet<string>(StringComparer.Ordinal);
        var generatedRequestIds = new HashSet<string>(StringComparer.Ordinal);
        var teacherRequestIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pair in orderedPairs)
        {
            var validationReason = string.Empty;
            if (!attemptNumbers.Add(pair.AttemptNumber) ||
                !TryValidateTeacherAuthoredPairStructure(pair, out validationReason) ||
                !generatedBindings.Add(pair.AuthoringEvidence.GeneratedBinding) ||
                !generatedRequestIds.Add(pair.AuthoringEvidence.GeneratedRequestId) ||
                !teacherRequestIds.Add(pair.AuthoringEvidence.Provenance.RequestId))
            {
                var reason = string.IsNullOrWhiteSpace(validationReason)
                    ? "runtime_teacher_authoring_duplicate_binding_or_request_id"
                    : validationReason;
                return OperationResult<DesignConversionDpoBuildResult>.Ok(
                    new DesignConversionDpoBuildResult(
                        AcceptedRecords: [],
                        QuarantinedRecords: [],
                        Warnings: BuildRuntimeTeacherAuthoredPairNotCreatedWarnings(
                            reason)));
            }
        }

        var candidateOnly = new List<(
            DesignConversionTeacherAuthoredEvaluationPair Pair,
            DesignConversionInternalEvaluationResult Generated,
            DesignConversionInternalEvaluationResult Incumbent,
            RuntimeRejectedCandidateEvidence RejectedEvidence,
            RouteAdmissionBindingResolution RouteBinding)>();
        foreach (var pair in orderedPairs)
        {
            if (!TryResolveRouteAdmissionBinding(
                    pair,
                    out var routeBinding,
                    out var routeBindingReason))
            {
                return OperationResult<DesignConversionDpoBuildResult>.Ok(
                    new DesignConversionDpoBuildResult(
                        AcceptedRecords: [],
                        QuarantinedRecords: [],
                        Warnings:
                            BuildRuntimeTeacherCandidateOnlyNotCreatedWarnings(
                                routeBindingReason)));
            }

            if (!routeBinding.TenantBound)
            {
                return OperationResult<DesignConversionDpoBuildResult>.Ok(
                    new DesignConversionDpoBuildResult(
                        AcceptedRecords: [],
                        QuarantinedRecords: [],
                        Warnings:
                            BuildRuntimeTeacherCandidateOnlyNotCreatedWarnings(
                                "server_owned_route_admission_not_bound")));
            }

            var normalizedPair = NormalizeMeasuredVisualScoresForDpo(pair);
            var evidenceGate =
                ValidateRuntimeCreatedCandidateEvidence(
                    normalizedPair.GeneratedEvaluation);
            if (evidenceGate.Count > 0)
            {
                return OperationResult<DesignConversionDpoBuildResult>.Ok(
                    new DesignConversionDpoBuildResult(
                        AcceptedRecords: [],
                        QuarantinedRecords: [],
                        Warnings:
                            BuildRuntimeTeacherCandidateOnlyNotCreatedWarnings(
                                string.Join(";", evidenceGate))));
            }

            if (!TryBuildRuntimeRejectedCandidateEvidence(
                    normalizedPair.GeneratedEvaluation,
                    normalizedPair.IncumbentEvaluation,
                    out var rejectedEvidence,
                    out var rejectedEvidenceReason))
            {
                return OperationResult<DesignConversionDpoBuildResult>.Ok(
                    new DesignConversionDpoBuildResult(
                        AcceptedRecords: [],
                        QuarantinedRecords: [],
                        Warnings:
                            BuildRuntimeTeacherCandidateOnlyNotCreatedWarnings(
                                rejectedEvidenceReason)));
            }

            candidateOnly.Add((
                normalizedPair,
                normalizedPair.GeneratedEvaluation,
                normalizedPair.IncumbentEvaluation,
                rejectedEvidence,
                routeBinding));
        }

        var observedAtUtc = createdAtUtc ?? DateTimeOffset.UtcNow;
        var build = BuildTrainReadyRecords(candidateOnly
            .Select(item => BuildTeacherCandidateOnlyStoredExample(
                item.Pair,
                item.Generated,
                item.Incumbent,
                item.RejectedEvidence,
                item.RouteBinding,
                observedAtUtc))
            .ToArray());
        if (!build.IsSuccess)
            return build;

        if (build.Value!.AcceptedRecords.Count == 0)
        {
            return OperationResult<DesignConversionDpoBuildResult>.Ok(
                new DesignConversionDpoBuildResult(
                    AcceptedRecords: [],
                    QuarantinedRecords: build.Value.QuarantinedRecords,
                    Warnings: build.Value.Warnings
                        .Concat(BuildRuntimeTeacherCandidateOnlyNotCreatedWarnings(
                            "teacher_candidate_only_record_quarantined"))
                        .Distinct(StringComparer.Ordinal)
                        .ToArray()));
        }

        return OperationResult<DesignConversionDpoBuildResult>.Ok(
            new DesignConversionDpoBuildResult(
                AcceptedRecords: build.Value.AcceptedRecords,
                QuarantinedRecords: build.Value.QuarantinedRecords,
                Warnings: build.Value.Warnings
                    .Concat(BuildRuntimeTeacherCandidateOnlyCreatedWarnings())
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()));
    }

    internal OperationResult<DesignConversionDpoBuildResult>
        BuildRuntimeTeacherAuthoredTrainingCandidates(
            IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
                evaluationPairs,
            IReadOnlyList<DesignConversionIndependentPreferenceEvidence>
                preferenceEvidence,
            string artifactRootBinding,
            DateTimeOffset? createdAtUtc = null)
    {
        if (evaluationPairs is null || preferenceEvidence is null ||
            evaluationPairs.Count == 0 ||
            evaluationPairs.Count >
                LegalTrainingTeacherAdmissionAuthority.MaxBatchRows ||
            preferenceEvidence.Count != evaluationPairs.Count ||
            evaluationPairs.Any(static pair => pair is null) ||
            preferenceEvidence.Any(static evidence => evidence is null) ||
            !IsSha256Ref(artifactRootBinding))
        {
            return OperationResult<DesignConversionDpoBuildResult>.Ok(
                new DesignConversionDpoBuildResult(
                    AcceptedRecords: [],
                    QuarantinedRecords: [],
                    Warnings: BuildRuntimeTeacherAuthoredPairNotCreatedWarnings(
                        "independent_preference_evidence_batch_invalid")));
        }

        var observedAtUtc = createdAtUtc ?? DateTimeOffset.UtcNow;
        var orderedPairs = evaluationPairs
            .OrderBy(static pair => pair.AttemptNumber)
            .ToArray();
        var evidenceByAttempt = preferenceEvidence
            .GroupBy(static evidence => evidence.AttemptNumber)
            .ToDictionary(static group => group.Key, static group => group.ToArray());
        var attemptNumbers = new HashSet<int>();
        var generatedBindings = new HashSet<string>(StringComparer.Ordinal);
        var generatedRequestIds = new HashSet<string>(StringComparer.Ordinal);
        var authoringRequestIds = new HashSet<string>(StringComparer.Ordinal);
        var replayNonces = new HashSet<string>(StringComparer.Ordinal);
        var scorerRequestIds = new HashSet<string>(StringComparer.Ordinal);
        var scorerOutputIds = new HashSet<string>(StringComparer.Ordinal);
        var scorerRequestHashes = new HashSet<string>(StringComparer.Ordinal);
        var scorerOutputHashes = new HashSet<string>(StringComparer.Ordinal);
        var validated = new List<(
            DesignConversionTeacherAuthoredEvaluationPair Pair,
            DesignConversionIndependentPreferenceEvidence Receipt,
            DesignConversionInternalEvaluationResult Chosen,
            DesignConversionInternalEvaluationResult Rejected,
            RuntimeRejectedCandidateEvidence RejectedEvidence,
            RouteAdmissionBindingResolution RouteBinding)>();

        foreach (var pair in orderedPairs)
        {
            var validationReason = string.Empty;
            if (!attemptNumbers.Add(pair.AttemptNumber) ||
                !TryValidateTeacherAuthoredPairStructure(pair, out validationReason) ||
                !generatedBindings.Add(pair.AuthoringEvidence.GeneratedBinding) ||
                !generatedRequestIds.Add(pair.AuthoringEvidence.GeneratedRequestId) ||
                !authoringRequestIds.Add(pair.AuthoringEvidence.Provenance.RequestId))
            {
                var reason = string.IsNullOrWhiteSpace(validationReason)
                    ? "runtime_teacher_authoring_duplicate_binding_or_request_id"
                    : validationReason;
                return IndependentPreferenceNotCreated(reason);
            }

            if (!TryResolveRouteAdmissionBinding(
                    pair,
                    out var routeBinding,
                    out var routeBindingReason))
            {
                return IndependentPreferenceNotCreated(routeBindingReason);
            }

            if (!evidenceByAttempt.TryGetValue(pair.AttemptNumber, out var matches) ||
                matches.Length != 1)
            {
                return IndependentPreferenceNotCreated(
                    "independent_preference_evidence_attempt_binding_invalid");
            }

            var receipt = matches[0];
            if (!TryValidateIndependentPreferenceEvidence(
                    pair,
                    receipt,
                    artifactRootBinding,
                    observedAtUtc,
                    out var evidenceReason) ||
                !replayNonces.Add(receipt.ReplayNonce) ||
                !scorerRequestIds.Add(receipt.ScorerProvenance.RequestId) ||
                !scorerOutputIds.Add(receipt.ScorerProvenance.OutputId) ||
                !scorerRequestHashes.Add(receipt.ScorerRequestSha256) ||
                !scorerOutputHashes.Add(receipt.ScorerOutputSha256))
            {
                var reason = string.IsNullOrWhiteSpace(evidenceReason)
                    ? "independent_preference_evidence_replay_or_duplicate"
                    : evidenceReason;
                return IndependentPreferenceNotCreated(reason);
            }

            var normalizedPair = NormalizeMeasuredVisualScoresForDpo(pair);
            var chosen = string.Equals(
                receipt.PreferredDirection,
                DesignConversionIndependentPreferenceEvidenceContract.GeneratedSide,
                StringComparison.Ordinal)
                    ? normalizedPair.GeneratedEvaluation
                    : normalizedPair.IncumbentEvaluation;
            var rejected = ReferenceEquals(chosen, normalizedPair.GeneratedEvaluation)
                ? normalizedPair.IncumbentEvaluation
                : normalizedPair.GeneratedEvaluation;
            var evidenceGate = ValidateRuntimeCreatedCandidateEvidence(chosen);
            var rejectedReason = string.Empty;
            if (evidenceGate.Count > 0 ||
                !TryBuildRuntimeRejectedCandidateEvidence(
                    chosen,
                    rejected,
                    out var rejectedEvidence,
                    out rejectedReason))
            {
                return IndependentPreferenceNotCreated(
                    evidenceGate.Count > 0
                        ? string.Join(";", evidenceGate)
                        : rejectedReason);
            }

            validated.Add((
                pair,
                receipt,
                chosen,
                rejected,
                rejectedEvidence,
                routeBinding));
        }

        foreach (var item in validated)
        {
            if (!DesignConversionLocalIndependentPreferenceEvaluator
                    .TryConsumeIssuedAuthority(item.Receipt))
            {
                return IndependentPreferenceNotCreated(
                    "independent_preference_evidence_authority_replay_or_missing");
            }
        }

        var accepted = new List<DesignConversionDpoTrainingRecord>();
        var quarantined = new List<DesignConversionDpoTrainingRecord>();
        var warnings = new List<string>();
        foreach (var item in validated)
        {
            var build = BuildTrainReadyRecords(
            [
                BuildIndependentPreferenceStoredExample(
                    item.Pair,
                    item.Receipt,
                    item.Chosen,
                    item.Rejected,
                    item.RejectedEvidence,
                    item.RouteBinding,
                    DesignConversionDpoUnits.CriticUnit,
                    observedAtUtc),
                BuildIndependentPreferenceStoredExample(
                    item.Pair,
                    item.Receipt,
                    item.Chosen,
                    item.Rejected,
                    item.RejectedEvidence,
                    item.RouteBinding,
                    DesignConversionDpoUnits.ImproverUnit,
                    observedAtUtc)
            ]);
            if (!build.IsSuccess)
                return build;

            foreach (var record in build.Value!.AcceptedRecords
                         .OrderBy(static record =>
                             string.Equals(
                                 record.UnitId,
                                 DesignConversionDpoUnits.CriticUnit,
                                 StringComparison.Ordinal)
                                    ? 0
                                    : 1))
            {
                var withCustody = AttachIndependentPreferencePayloadCustody(
                    record,
                    item.Chosen,
                    item.Rejected);
                if (withCustody.PayloadCustodySecret is null)
                {
                    quarantined.Add(record with
                    {
                        Quarantine = new DesignConversionQuarantineState(
                            true,
                            "independent_preference_payload_custody_exact_validation_failed",
                            ["DESIGN_CONVERSION_DPO_PAYLOAD_CUSTODY_EXACT_VALIDATION_FAILED"])
                    });
                    warnings.Add(
                        "independent_preference_payload_custody_exact_validation_failed");
                }
                else
                {
                    accepted.Add(withCustody);
                }
            }

            quarantined.AddRange(build.Value.QuarantinedRecords);
            warnings.AddRange(build.Value.Warnings);
        }

        if (accepted.Count == 0)
        {
            return OperationResult<DesignConversionDpoBuildResult>.Ok(
                new DesignConversionDpoBuildResult(
                    AcceptedRecords: [],
                    QuarantinedRecords: quarantined.ToArray(),
                    Warnings: warnings
                        .Concat(BuildRuntimeTeacherAuthoredPairNotCreatedWarnings(
                            "independent_preference_candidate_projection_rejected"))
                        .Distinct(StringComparer.Ordinal)
                        .ToArray()));
        }

        return OperationResult<DesignConversionDpoBuildResult>.Ok(
            new DesignConversionDpoBuildResult(
                AcceptedRecords: accepted.ToArray(),
                QuarantinedRecords: quarantined.ToArray(),
                Warnings: warnings
                    .Append("runtime_created_dpo_candidate")
                    .Append("runtime_created_dpo_candidate_review_pending")
                    .Append("runtime_created_dpo_candidate_approval_required")
                    .Append("independent_preference_proven=true")
                    .Append("chosen_rejected_pair_proven")
                    .Append("runtime_rejected_independently_evaluated=true")
                    .Append("high_quality_negative_pair_proven")
                    .Append("runtime_teacher_authored_candidate=True")
                    .Append("candidate_authoring_only=true")
                    .Append("training_candidate_only=true")
                    .Append("training_grade=false")
                    .Append("not_training_grade=true")
                    .Append("approved_for_training=false")
                    .Append("production_dpo=false")
                    .Append("product_not_ready")
                    .Append("trained_model_authority=False")
                    .Append("held_out_not_proven")
                    .Append("source_disjoint_not_proven")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()));

        OperationResult<DesignConversionDpoBuildResult>
            IndependentPreferenceNotCreated(string reason)
            => OperationResult<DesignConversionDpoBuildResult>.Ok(
                new DesignConversionDpoBuildResult(
                    AcceptedRecords: [],
                    QuarantinedRecords: [],
                    Warnings: BuildRuntimeTeacherAuthoredPairNotCreatedWarnings(
                        reason)));
    }

    internal static DesignConversionInternalEvaluationResult
        NormalizeMeasuredVisualScoreForDpo(
            DesignConversionInternalEvaluationResult evaluation)
    {
        var output = evaluation.RuntimeFlowResult?.Output;
        var decision = evaluation.VisualQaBoundaryTrace?.VisualDecision;
        if (output is null || decision is null ||
            !double.IsFinite(decision.QualityScore) ||
            decision.QualityScore is < 0 or > 1)
        {
            return evaluation;
        }

        var measuredOutput =
            Math.Abs(output.Verdict.Score - decision.QualityScore) < 0.0000001
                ? output
                : output with
                {
                    Verdict = output.Verdict with
                    {
                        Score = decision.QualityScore
                    }
                };
        return evaluation with
        {
            RuntimeFlowResult = evaluation.RuntimeFlowResult! with
            {
                Output = measuredOutput
            }
        };
    }

    internal static string BuildIndependentPreferenceEvaluationIdentity(
        DesignConversionInternalEvaluationResult evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);
        var output = evaluation.RuntimeFlowResult?.Output;
        var decision = evaluation.VisualQaBoundaryTrace?.VisualDecision;
        var snapshot = new
        {
            schema = "design-conversion-exact-evaluation-identity-v1",
            job_id = evaluation.JobId ?? string.Empty,
            request_id = evaluation.SubmittedRequest?.RequestId ?? string.Empty,
            source_kind = evaluation.SubmittedRequest?.SourceKind.ToString() ??
                string.Empty,
            submitted_html_sha256 = Sha256Ref(
                evaluation.SubmittedRequest?.Html ?? string.Empty),
            submitted_css_sha256 = Sha256Ref(
                evaluation.SubmittedRequest?.Css ?? string.Empty),
            improved_html_sha256 = Sha256Ref(evaluation.ImprovedHtml ?? string.Empty),
            improved_css_sha256 = Sha256Ref(evaluation.ImprovedCss ?? string.Empty),
            output_id = output?.OutputId ?? string.Empty,
            candidate_id = output?.CandidateId ?? string.Empty,
            verdict_id = output?.Verdict?.VerdictId ?? string.Empty,
            output_score = output?.Verdict?.Score.ToString(
                "R",
                CultureInfo.InvariantCulture) ?? string.Empty,
            output_accepted = output?.Accepted ?? false,
            visual_score = decision?.QualityScore.ToString(
                "R",
                CultureInfo.InvariantCulture) ?? string.Empty,
            can_persist_training_candidate =
                decision?.CanPersistTrainingCandidate ?? false,
            qa_result_ids = (evaluation.QaResults ?? [])
                .Select(static result => result.ResultId ?? string.Empty)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            comparison_artifact_ids = (evaluation.ComparisonArtifacts ?? [])
                .Select(static artifact => artifact.ArtifactId ?? string.Empty)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            evidence_refs = (evaluation.EvidenceRefs ?? [])
                .Order(StringComparer.Ordinal)
                .ToArray(),
            boundary_evidence_refs =
                (evaluation.VisualQaBoundaryTrace?.EvidenceRefs ?? [])
                .Order(StringComparer.Ordinal)
                .ToArray()
        };
        return Sha256Ref(JsonSerializer.Serialize(snapshot, CompactJsonOptions));
    }

    internal static string BuildIndependentPreferenceScorerRequestSha256(
        DesignConversionTeacherAuthoredEvaluationPair pair,
        string artifactRootBinding,
        DesignConversionIndependentPreferenceScorerProvenance provenance)
        => BuildIndependentPreferenceScorerRequestSha256(
            pair,
            artifactRootBinding,
            provenance,
            DesignConversionIndependentPreferenceEvidenceContract
                .EvaluationOnlyProfileId,
            DesignConversionIndependentPreferenceEvidenceContract
                .EvaluationOnlyProfileVersion,
            DesignConversionIndependentPreferenceEvidenceContract
                .EvaluationOnlyProfileConfigurationSha256,
            DesignConversionIndependentPreferenceEvidenceContract
                .EvaluationOnlyModelSha256);

    internal static string BuildIndependentPreferenceScorerRequestSha256(
        DesignConversionTeacherAuthoredEvaluationPair pair,
        string artifactRootBinding,
        DesignConversionIndependentPreferenceScorerProvenance provenance,
        string evaluationProfileId,
        string evaluationProfileVersion,
        string evaluationProfileConfigurationSha256,
        string evaluationModelSha256)
    {
        ArgumentNullException.ThrowIfNull(pair);
        ArgumentNullException.ThrowIfNull(provenance);
        var snapshot = new
        {
            schema = "design-conversion-independent-scorer-request-v1",
            artifact_root_binding = artifactRootBinding ?? string.Empty,
            attempt_number = pair.AttemptNumber,
            generated_ref = pair.AuthoringEvidence.GeneratedRef ?? string.Empty,
            incumbent_ref = pair.AuthoringEvidence.IncumbentRef ?? string.Empty,
            generated_binding = pair.AuthoringEvidence.GeneratedBinding ??
                string.Empty,
            incumbent_binding = pair.AuthoringEvidence.IncumbentBinding ??
                string.Empty,
            generated_evaluation_identity =
                BuildIndependentPreferenceEvaluationIdentity(
                    pair.GeneratedEvaluation),
            incumbent_evaluation_identity =
                BuildIndependentPreferenceEvaluationIdentity(
                    pair.IncumbentEvaluation),
            evaluation_profile_id = evaluationProfileId ?? string.Empty,
            evaluation_profile_version = evaluationProfileVersion ?? string.Empty,
            evaluation_profile_configuration_sha256 =
                evaluationProfileConfigurationSha256 ?? string.Empty,
            evaluation_model_sha256 = evaluationModelSha256 ?? string.Empty,
            scorer = new
            {
                kind = provenance.Kind ?? string.Empty,
                scorer_id = provenance.ScorerId ?? string.Empty,
                scorer_version = provenance.ScorerVersion ?? string.Empty,
                request_id = provenance.RequestId ?? string.Empty,
                runtime_instance_ref = provenance.RuntimeInstanceRef ?? string.Empty,
                evaluation_profile_id = provenance.EvaluationProfileId ??
                    string.Empty,
                evaluation_profile_version = provenance.EvaluationProfileVersion ??
                    string.Empty,
                evaluation_profile_configuration_sha256 =
                    provenance.EvaluationProfileConfigurationSha256 ?? string.Empty,
                evaluation_model_sha256 = provenance.EvaluationModelSha256 ??
                    string.Empty
            }
        };
        return Sha256Ref(JsonSerializer.Serialize(snapshot, CompactJsonOptions));
    }

    internal static string BuildIndependentPreferenceScorerOutputId(
        string scorerOutputSha256,
        string scorerRequestId,
        string inferenceRequestId,
        string runtimeInstanceRef)
    {
        var snapshot = new
        {
            schema = "design-conversion-independent-scorer-output-id-v1",
            scorer_output_sha256 = scorerOutputSha256 ?? string.Empty,
            scorer_request_id = scorerRequestId ?? string.Empty,
            inference_request_id = inferenceRequestId ?? string.Empty,
            runtime_instance_ref = runtimeInstanceRef ?? string.Empty
        };
        return Sha256Ref(JsonSerializer.Serialize(snapshot, CompactJsonOptions));
    }

    internal static string BuildIndependentPreferenceScorerOutputCommitmentSha256(
        DesignConversionIndependentPreferenceEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        var provenance = evidence.ScorerProvenance;
        var snapshot = new
        {
            schema = "design-conversion-independent-scorer-output-commitment-v1",
            attempt_number = evidence.AttemptNumber,
            generated_ref = evidence.GeneratedRef ?? string.Empty,
            incumbent_ref = evidence.IncumbentRef ?? string.Empty,
            generated_binding = evidence.GeneratedBinding ?? string.Empty,
            incumbent_binding = evidence.IncumbentBinding ?? string.Empty,
            generated_evaluation_identity =
                evidence.GeneratedEvaluationIdentity ?? string.Empty,
            incumbent_evaluation_identity =
                evidence.IncumbentEvaluationIdentity ?? string.Empty,
            preferred_direction = evidence.PreferredDirection ?? string.Empty,
            scorer_request_sha256 = evidence.ScorerRequestSha256 ?? string.Empty,
            scorer_output_sha256 = evidence.ScorerOutputSha256 ?? string.Empty,
            evaluation_profile_id = evidence.EvaluationProfileId ?? string.Empty,
            evaluation_profile_version =
                evidence.EvaluationProfileVersion ?? string.Empty,
            evaluation_profile_configuration_sha256 =
                evidence.EvaluationProfileConfigurationSha256 ?? string.Empty,
            evaluation_model_sha256 = evidence.EvaluationModelSha256 ?? string.Empty,
            artifact_root_binding = evidence.ArtifactRootBinding ?? string.Empty,
            scorer_provenance = provenance is null
                ? null
                : new
                {
                    kind = provenance.Kind ?? string.Empty,
                    scorer_id = provenance.ScorerId ?? string.Empty,
                    scorer_version = provenance.ScorerVersion ?? string.Empty,
                    request_id = provenance.RequestId ?? string.Empty,
                    output_id = provenance.OutputId ?? string.Empty,
                    runtime_instance_ref = provenance.RuntimeInstanceRef ??
                        string.Empty,
                    evaluation_profile_id = provenance.EvaluationProfileId ??
                        string.Empty,
                    evaluation_profile_version =
                        provenance.EvaluationProfileVersion ?? string.Empty,
                    evaluation_profile_configuration_sha256 =
                        provenance.EvaluationProfileConfigurationSha256 ??
                        string.Empty,
                    evaluation_model_sha256 = provenance.EvaluationModelSha256 ??
                        string.Empty,
                    inference_request_id = provenance.InferenceRequestId ??
                        string.Empty
                }
        };
        return Sha256Ref(JsonSerializer.Serialize(snapshot, CompactJsonOptions));
    }

    internal static string BuildIndependentPreferenceReceiptSha256(
        DesignConversionIndependentPreferenceEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        var provenance = evidence.ScorerProvenance;
        var snapshot = new
        {
            schema = evidence.Schema ?? string.Empty,
            attempt_number = evidence.AttemptNumber,
            generated_ref = evidence.GeneratedRef ?? string.Empty,
            incumbent_ref = evidence.IncumbentRef ?? string.Empty,
            generated_binding = evidence.GeneratedBinding ?? string.Empty,
            incumbent_binding = evidence.IncumbentBinding ?? string.Empty,
            generated_evaluation_identity =
                evidence.GeneratedEvaluationIdentity ?? string.Empty,
            incumbent_evaluation_identity =
                evidence.IncumbentEvaluationIdentity ?? string.Empty,
            preferred_direction = evidence.PreferredDirection ?? string.Empty,
            scorer_request_sha256 = evidence.ScorerRequestSha256 ?? string.Empty,
            scorer_output_sha256 = evidence.ScorerOutputSha256 ?? string.Empty,
            evaluation_profile_id = evidence.EvaluationProfileId ?? string.Empty,
            evaluation_profile_version =
                evidence.EvaluationProfileVersion ?? string.Empty,
            evaluation_profile_configuration_sha256 =
                evidence.EvaluationProfileConfigurationSha256 ?? string.Empty,
            evaluation_model_sha256 = evidence.EvaluationModelSha256 ?? string.Empty,
            artifact_root_binding = evidence.ArtifactRootBinding ?? string.Empty,
            scorer_provenance = provenance is null
                ? null
                : new
                {
                    kind = provenance.Kind ?? string.Empty,
                    scorer_id = provenance.ScorerId ?? string.Empty,
                    scorer_version = provenance.ScorerVersion ?? string.Empty,
                    request_id = provenance.RequestId ?? string.Empty,
                    output_id = provenance.OutputId ?? string.Empty,
                    runtime_instance_ref = provenance.RuntimeInstanceRef ??
                        string.Empty,
                    evaluation_profile_id = provenance.EvaluationProfileId ??
                        string.Empty,
                    evaluation_profile_version =
                        provenance.EvaluationProfileVersion ?? string.Empty,
                    evaluation_profile_configuration_sha256 =
                        provenance.EvaluationProfileConfigurationSha256 ??
                        string.Empty,
                    evaluation_model_sha256 = provenance.EvaluationModelSha256 ??
                        string.Empty,
                    inference_request_id = provenance.InferenceRequestId ??
                        string.Empty
                },
            issued_at_utc = evidence.IssuedAtUtc.ToUniversalTime()
                .ToString("O", CultureInfo.InvariantCulture),
            expires_at_utc = evidence.ExpiresAtUtc.ToUniversalTime()
                .ToString("O", CultureInfo.InvariantCulture),
            replay_nonce = evidence.ReplayNonce ?? string.Empty
        };
        return Sha256Ref(JsonSerializer.Serialize(snapshot, CompactJsonOptions));
    }

    private static bool TryValidateIndependentPreferenceEvidence(
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DesignConversionIndependentPreferenceEvidence evidence,
        string artifactRootBinding,
        DateTimeOffset observedAtUtc,
        out string reason)
    {
        reason = "independent_preference_evidence_invalid";
        var authoring = pair.AuthoringEvidence;
        var provenance = evidence.ScorerProvenance;
        var generatedIdentity = BuildIndependentPreferenceEvaluationIdentity(
            pair.GeneratedEvaluation);
        var incumbentIdentity = BuildIndependentPreferenceEvaluationIdentity(
            pair.IncumbentEvaluation);
        if (!string.Equals(
                evidence.Schema,
                DesignConversionIndependentPreferenceEvidenceContract.Schema,
                StringComparison.Ordinal) ||
            evidence.AttemptNumber != pair.AttemptNumber ||
            !string.Equals(evidence.GeneratedRef, authoring.GeneratedRef,
                StringComparison.Ordinal) ||
            !string.Equals(evidence.IncumbentRef, authoring.IncumbentRef,
                StringComparison.Ordinal) ||
            !string.Equals(evidence.GeneratedBinding, authoring.GeneratedBinding,
                StringComparison.Ordinal) ||
            !string.Equals(evidence.IncumbentBinding, authoring.IncumbentBinding,
                StringComparison.Ordinal) ||
            !string.Equals(evidence.GeneratedEvaluationIdentity,
                generatedIdentity, StringComparison.Ordinal) ||
            !string.Equals(evidence.IncumbentEvaluationIdentity,
                incumbentIdentity, StringComparison.Ordinal) ||
            string.Equals(generatedIdentity, incumbentIdentity,
                StringComparison.Ordinal))
        {
            reason = "independent_preference_evidence_pair_binding_invalid";
            return false;
        }

        if (!string.Equals(
                evidence.PreferredDirection,
                DesignConversionIndependentPreferenceEvidenceContract.GeneratedSide,
                StringComparison.Ordinal) &&
            !string.Equals(
                evidence.PreferredDirection,
                DesignConversionIndependentPreferenceEvidenceContract.IncumbentSide,
                StringComparison.Ordinal))
        {
            reason = "independent_preference_evidence_direction_invalid";
            return false;
        }

        if (!string.Equals(evidence.EvaluationProfileId,
                DesignConversionIndependentPreferenceEvidenceContract
                    .EvaluationOnlyProfileId, StringComparison.Ordinal) ||
            !string.Equals(evidence.EvaluationProfileVersion,
                DesignConversionIndependentPreferenceEvidenceContract
                    .EvaluationOnlyProfileVersion, StringComparison.Ordinal) ||
            !string.Equals(evidence.EvaluationProfileConfigurationSha256,
                DesignConversionIndependentPreferenceEvidenceContract
                    .EvaluationOnlyProfileConfigurationSha256,
                StringComparison.Ordinal) ||
            !IsSha256Ref(evidence.EvaluationModelSha256) ||
            !string.Equals(evidence.ArtifactRootBinding, artifactRootBinding,
                StringComparison.Ordinal))
        {
            reason = "independent_preference_evidence_profile_or_root_invalid";
            return false;
        }

        if (provenance is null ||
            !string.Equals(provenance.Kind,
                DesignConversionIndependentPreferenceEvidenceContract
                    .LocalScorerProvenanceKind, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(provenance.ScorerId) ||
            string.IsNullOrWhiteSpace(provenance.ScorerVersion) ||
            string.IsNullOrWhiteSpace(provenance.RequestId) ||
            string.IsNullOrWhiteSpace(provenance.OutputId) ||
            string.IsNullOrWhiteSpace(provenance.RuntimeInstanceRef) ||
            string.IsNullOrWhiteSpace(provenance.InferenceRequestId) ||
            !string.Equals(provenance.EvaluationProfileId,
                evidence.EvaluationProfileId, StringComparison.Ordinal) ||
            !string.Equals(provenance.EvaluationProfileVersion,
                evidence.EvaluationProfileVersion, StringComparison.Ordinal) ||
            !string.Equals(provenance.EvaluationProfileConfigurationSha256,
                evidence.EvaluationProfileConfigurationSha256,
                StringComparison.Ordinal) ||
            !string.Equals(provenance.EvaluationModelSha256,
                evidence.EvaluationModelSha256, StringComparison.Ordinal) ||
            string.Equals(provenance.ScorerId, authoring.Provenance.ModelId,
                StringComparison.Ordinal) ||
            string.Equals(provenance.RequestId, authoring.Provenance.RequestId,
                StringComparison.Ordinal) ||
            string.Equals(provenance.RequestId, authoring.GeneratedRequestId,
                StringComparison.Ordinal) ||
            string.Equals(provenance.InferenceRequestId,
                authoring.Provenance.RequestId, StringComparison.Ordinal) ||
            string.Equals(provenance.InferenceRequestId,
                authoring.GeneratedRequestId, StringComparison.Ordinal) ||
            string.Equals(provenance.OutputId, authoring.OutputHash,
                StringComparison.Ordinal) ||
            !string.Equals(
                provenance.OutputId,
                BuildIndependentPreferenceScorerOutputId(
                    evidence.ScorerOutputSha256,
                    provenance.RequestId,
                    provenance.InferenceRequestId,
                    provenance.RuntimeInstanceRef),
                StringComparison.Ordinal))
        {
            reason = "independent_preference_evidence_provenance_invalid";
            return false;
        }

        if (!IsSha256Ref(evidence.ScorerRequestSha256) ||
            !IsSha256Ref(evidence.ScorerOutputSha256) ||
            !IsSha256Ref(evidence.ReplayNonce) ||
            !IsSha256Ref(evidence.ReceiptSha256) ||
            !string.Equals(
                evidence.ScorerRequestSha256,
                BuildIndependentPreferenceScorerRequestSha256(
                    pair,
                    artifactRootBinding,
                    provenance,
                    evidence.EvaluationProfileId,
                    evidence.EvaluationProfileVersion,
                    evidence.EvaluationProfileConfigurationSha256,
                    evidence.EvaluationModelSha256),
                StringComparison.Ordinal) ||
            !string.Equals(
                evidence.ReceiptSha256,
                BuildIndependentPreferenceReceiptSha256(evidence),
                StringComparison.Ordinal))
        {
            reason = "independent_preference_evidence_hash_binding_invalid";
            return false;
        }

        var issuedAtUtc = evidence.IssuedAtUtc.ToUniversalTime();
        var expiresAtUtc = evidence.ExpiresAtUtc.ToUniversalTime();
        var observedUtc = observedAtUtc.ToUniversalTime();
        if (issuedAtUtc > observedUtc ||
            expiresAtUtc <= observedUtc ||
            expiresAtUtc <= issuedAtUtc ||
            observedUtc - issuedAtUtc >
                DesignConversionIndependentPreferenceEvidenceContract
                    .MaximumFreshness ||
            expiresAtUtc - issuedAtUtc >
                DesignConversionIndependentPreferenceEvidenceContract
                    .MaximumFreshness)
        {
            reason = "independent_preference_evidence_freshness_invalid";
            return false;
        }

        if (!DesignConversionLocalIndependentPreferenceEvaluator
                .HasIssuedAuthority(evidence))
        {
            reason = "independent_preference_evidence_authority_missing_or_tampered";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    internal static bool TryValidateIndependentPreferenceEvidenceForRuntime(
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DesignConversionIndependentPreferenceEvidence evidence,
        string artifactRootBinding,
        DateTimeOffset observedAtUtc,
        out string reason)
        => TryValidateIndependentPreferenceEvidence(
            pair,
            evidence,
            artifactRootBinding,
            observedAtUtc,
            out reason);

    private static bool IsSha256Ref(string? value)
        => value is { Length: 71 } &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.AsSpan(7).ToArray().All(
               static character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool TryResolveRouteAdmissionBinding(
        DesignConversionTeacherAuthoredEvaluationPair pair,
        out RouteAdmissionBindingResolution binding,
        out string reason)
    {
        binding = new RouteAdmissionBindingResolution(false, string.Empty, string.Empty);
        reason = string.Empty;
        var generatedState = ClassifyRouteAdmissionBinding(
            pair.GeneratedEvaluation.SubmittedRequest.Provenance,
            out var generatedTenantIdRef,
            out var generatedScopeRef);
        var incumbentState = ClassifyRouteAdmissionBinding(
            pair.IncumbentEvaluation.SubmittedRequest.Provenance,
            out var incumbentTenantIdRef,
            out var incumbentScopeRef);

        if (generatedState == RouteAdmissionBindingState.Unbound &&
            incumbentState == RouteAdmissionBindingState.Unbound)
        {
            return true;
        }

        if (generatedState == RouteAdmissionBindingState.Invalid ||
            incumbentState == RouteAdmissionBindingState.Invalid)
        {
            reason = "route_admission_binding_malformed";
            return false;
        }

        if (generatedState != incumbentState)
        {
            reason = "route_admission_binding_one_sided";
            return false;
        }

        if (!string.Equals(
                generatedTenantIdRef,
                incumbentTenantIdRef,
                StringComparison.Ordinal) ||
            !string.Equals(
                generatedScopeRef,
                incumbentScopeRef,
                StringComparison.Ordinal))
        {
            reason = "route_admission_binding_mismatch";
            return false;
        }

        binding = new RouteAdmissionBindingResolution(
            true,
            generatedTenantIdRef,
            generatedScopeRef);
        return true;
    }

    private static bool TryResolveRouteAdmissionBinding(
        DesignConversionInternalEvaluationResult evaluation,
        out RouteAdmissionBindingResolution binding,
        out string reason)
    {
        binding = new RouteAdmissionBindingResolution(false, string.Empty, string.Empty);
        reason = string.Empty;
        var state = ClassifyRouteAdmissionBinding(
            evaluation.SubmittedRequest.Provenance,
            out var tenantIdRef,
            out var scopeRef);
        if (state == RouteAdmissionBindingState.Invalid)
        {
            reason = "route_admission_binding_malformed";
            return false;
        }

        if (state == RouteAdmissionBindingState.Unbound)
            return true;

        binding = new RouteAdmissionBindingResolution(true, tenantIdRef, scopeRef);
        return true;
    }

    private static RouteAdmissionBindingState ClassifyRouteAdmissionBinding(
        IReadOnlyDictionary<string, string>? metadata,
        out string tenantIdRef,
        out string scopeRef)
    {
        tenantIdRef = string.Empty;
        scopeRef = string.Empty;
        var hasBound = metadata is not null && metadata.ContainsKey(
            DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey);
        var hasTenant = metadata is not null && metadata.ContainsKey(
            DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey);
        var hasScope = metadata is not null && metadata.ContainsKey(
            DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey);
        if (!hasBound && !hasTenant && !hasScope)
            return RouteAdmissionBindingState.Unbound;
        if (!hasBound || !hasTenant || !hasScope)
            return RouteAdmissionBindingState.Invalid;

        var bound = metadata![
            DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey];
        tenantIdRef = metadata[
            DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey];
        scopeRef = metadata[
            DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey];
        if (string.Equals(bound, bool.FalseString, StringComparison.Ordinal))
        {
            return tenantIdRef.Length == 0 && scopeRef.Length == 0
                ? RouteAdmissionBindingState.Unbound
                : RouteAdmissionBindingState.Invalid;
        }

        return string.Equals(bound, bool.TrueString, StringComparison.Ordinal) &&
               DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionTenantIdRef(
                   tenantIdRef) &&
               DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionScopeRef(
                   scopeRef)
            ? RouteAdmissionBindingState.Bound
            : RouteAdmissionBindingState.Invalid;
    }

    private static void AppendRouteAdmissionBindingMetadata(
        IDictionary<string, string> metadata,
        RouteAdmissionBindingResolution binding)
    {
        if (!binding.TenantBound)
            return;

        metadata[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
            bool.TrueString;
        metadata[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
            binding.TenantIdRef;
        metadata[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey] =
            binding.ScopeRef;
    }

    private static DesignConversionPairAuthorDirection
        ResolveIndependentPreferencePairAuthors(
            DesignConversionTeacherAuthoredEvaluationPair pair,
            DesignConversionInternalEvaluationResult chosenEvaluation,
            DesignConversionInternalEvaluationResult rejectedEvaluation)
    {
        var chosenSide = ResolveEvaluationSide(pair, chosenEvaluation);
        var rejectedSide = ResolveEvaluationSide(pair, rejectedEvaluation);
        if (string.Equals(
                chosenSide,
                DesignConversionDpoPolicyCheckpointBridge.PairSourceSideGenerated,
                StringComparison.Ordinal) &&
            string.Equals(
                rejectedSide,
                DesignConversionDpoPolicyCheckpointBridge.PairSourceSideIncumbent,
                StringComparison.Ordinal))
        {
            return new DesignConversionPairAuthorDirection(
                DpoPairSideAuthorKind.OpenTeacher,
                DpoPairSideAuthorKind.LocalModel,
                chosenSide,
                rejectedSide,
                true);
        }

        if (string.Equals(
                chosenSide,
                DesignConversionDpoPolicyCheckpointBridge.PairSourceSideIncumbent,
                StringComparison.Ordinal) &&
            string.Equals(
                rejectedSide,
                DesignConversionDpoPolicyCheckpointBridge.PairSourceSideGenerated,
                StringComparison.Ordinal))
        {
            return new DesignConversionPairAuthorDirection(
                DpoPairSideAuthorKind.LocalModel,
                DpoPairSideAuthorKind.OpenTeacher,
                chosenSide,
                rejectedSide,
                true);
        }

        return new DesignConversionPairAuthorDirection(
            null,
            null,
            string.IsNullOrWhiteSpace(chosenSide) ? "unknown" : chosenSide,
            string.IsNullOrWhiteSpace(rejectedSide) ? "unknown" : rejectedSide,
            false);
    }

    private static string ResolveEvaluationSide(
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DesignConversionInternalEvaluationResult evaluation)
    {
        if (pair is null || evaluation is null)
            return "unknown";

        var request = evaluation.SubmittedRequest;
        var authoring = pair.AuthoringEvidence;
        if (request is null || authoring is null)
            return "unknown";

        var generatedBinding =
            DesignConversionDpoTeacherAttestationRuntime.BuildGeneratedBinding(
                request.Html,
                request.Css);
        if (string.Equals(
                request.RequestId,
                authoring.GeneratedRequestId,
                StringComparison.Ordinal) &&
            string.Equals(
                generatedBinding,
                authoring.GeneratedBinding,
                StringComparison.Ordinal))
        {
            return DesignConversionDpoPolicyCheckpointBridge
                .PairSourceSideGenerated;
        }

        var incumbentBinding =
            DesignConversionDpoTeacherAttestationRuntime.BuildIncumbentBinding(
                request);
        return string.Equals(
                   incumbentBinding,
                   authoring.IncumbentBinding,
                   StringComparison.Ordinal)
            ? DesignConversionDpoPolicyCheckpointBridge.PairSourceSideIncumbent
            : "unknown";
    }

    private enum RouteAdmissionBindingState
    {
        Unbound,
        Bound,
        Invalid
    }

    private sealed record RouteAdmissionBindingResolution(
        bool TenantBound,
        string TenantIdRef,
        string ScopeRef);

    private sealed record DesignConversionPairAuthorDirection(
        DpoPairSideAuthorKind? ChosenAuthor,
        DpoPairSideAuthorKind? RejectedAuthor,
        string ChosenSourceSide,
        string RejectedSourceSide,
        bool IsKnown);

    private static DesignConversionStoredDpoExample
        BuildIndependentPreferenceStoredExample(
            DesignConversionTeacherAuthoredEvaluationPair pair,
            DesignConversionIndependentPreferenceEvidence receipt,
            DesignConversionInternalEvaluationResult chosenEvaluation,
            DesignConversionInternalEvaluationResult rejectedEvaluation,
            RuntimeRejectedCandidateEvidence rejectedEvidence,
            RouteAdmissionBindingResolution routeBinding,
            string unitId,
            DateTimeOffset createdAtUtc)
    {
        var authoring = pair.AuthoringEvidence;
        var request = chosenEvaluation.SubmittedRequest;
        var chosenOutput = chosenEvaluation.RuntimeFlowResult.Output;
        var chosenScore = Math.Max(
            0.01,
            Math.Clamp(chosenOutput.Verdict.Score, 0.0, 1.0));
        var rejectedScore = Math.Round(rejectedEvidence.VisualScore, 6);
        var receiptRefs = new[]
        {
            receipt.ReceiptSha256,
            receipt.ScorerRequestSha256,
            receipt.ScorerOutputSha256,
            receipt.EvaluationProfileConfigurationSha256,
            receipt.EvaluationModelSha256,
            receipt.ArtifactRootBinding
        };
        var chosen = chosenOutput with
        {
            Html = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-independent-scorer-chosen-html",
                chosenEvaluation.SubmittedRequest.Html),
            Css = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-independent-scorer-chosen-css",
                chosenEvaluation.SubmittedRequest.Css),
            EvidenceRefs = chosenOutput.EvidenceRefs
                .Concat(chosenEvaluation.EvidenceRefs)
                .Concat(authoring.EvidenceRefs)
                .Concat(receiptRefs)
                .Append("runtime_created_dpo_candidate:independent_scorer_chosen_ref")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            QaResults = BuildRuntimeQaResults(chosenEvaluation),
            Verdict = chosenOutput.Verdict with { Score = chosenScore },
            Accepted = true
        };
        var rejected = new DesignConversionImprovementOutput(
            OutputId: rejectedEvidence.OutputId,
            CandidateId: rejectedEvidence.CandidateId,
            Html: DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-independent-scorer-rejected-html",
                rejectedEvaluation.SubmittedRequest.Html),
            Css: DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-independent-scorer-rejected-css",
                rejectedEvaluation.SubmittedRequest.Css),
            Verdict: new VisualVerdict(
                VerdictId: rejectedEvidence.VerdictId,
                Score: rejectedScore,
                Defects: rejectedEvidence.Defects,
                EvidenceRefs: rejectedEvidence.EvidenceRefs,
                Passed: false),
            QaResults: rejectedEvidence.QaResults,
            EvidenceRefs: rejectedEvidence.EvidenceRefs
                .Concat(authoring.EvidenceRefs)
                .Concat(receiptRefs)
                .Append("runtime_created_dpo_candidate:independent_scorer_rejected_ref")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Accepted: false);
        var pairAuthors = ResolveIndependentPreferencePairAuthors(
            pair,
            chosenEvaluation,
            rejectedEvaluation);

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["runtime_teacher_authored_candidate"] = bool.TrueString,
            ["candidate_authoring_only"] = bool.TrueString,
            ["teacher_authoring_schema"] = authoring.Schema,
            ["teacher_authoring_output_hash"] = authoring.OutputHash,
            ["teacher_authoring_incumbent_ref"] = authoring.IncumbentRef,
            ["teacher_authoring_generated_ref"] = authoring.GeneratedRef,
            ["teacher_inference_profile_id"] = authoring.InferenceProfileId,
            ["teacher_inference_profile_configuration_sha256"] =
                authoring.InferenceProfileConfigurationSha256,
            ["raw_teacher_prompt_persisted"] = bool.FalseString,
            ["raw_teacher_output_persisted"] = bool.FalseString,
            ["raw_teacher_rationale_persisted"] = bool.FalseString,
            ["independent_preference_proven"] = bool.TrueString,
            ["independent_preference_receipt_sha256"] = receipt.ReceiptSha256,
            ["independent_preference_attempt_number"] =
                receipt.AttemptNumber.ToString(CultureInfo.InvariantCulture),
            ["independent_preference_generated_evaluation_identity"] =
                receipt.GeneratedEvaluationIdentity,
            ["independent_preference_incumbent_evaluation_identity"] =
                receipt.IncumbentEvaluationIdentity,
            ["independent_preference_artifact_root_binding"] =
                receipt.ArtifactRootBinding,
            ["directionality_source"] =
                DesignConversionIndependentPreferenceEvidenceContract
                    .LocalScorerProvenanceKind,
            [DesignConversionDpoPolicyCheckpointBridge
                .PairAuthorshipSourceMetadataKey] =
                    "independent_preference_chosen_evaluation",
            [DesignConversionDpoPolicyCheckpointBridge
                .PairChosenSourceSideMetadataKey] =
                    pairAuthors.ChosenSourceSide,
            [DesignConversionDpoPolicyCheckpointBridge
                .PairRejectedSourceSideMetadataKey] =
                    pairAuthors.RejectedSourceSide,
            [DesignConversionDpoPolicyCheckpointBridge
                .PairChosenAuthorKindMetadataKey] =
                    pairAuthors.ChosenAuthor?.ToString() ?? "unknown",
            [DesignConversionDpoPolicyCheckpointBridge
                .PairRejectedAuthorKindMetadataKey] =
                    pairAuthors.RejectedAuthor?.ToString() ?? "unknown",
            ["independent_scorer_request_sha256"] = receipt.ScorerRequestSha256,
            ["independent_scorer_output_sha256"] = receipt.ScorerOutputSha256,
            ["independent_scorer_profile_id"] = receipt.EvaluationProfileId,
            ["independent_scorer_profile_version"] =
                receipt.EvaluationProfileVersion,
            ["independent_scorer_profile_configuration_sha256"] =
                receipt.EvaluationProfileConfigurationSha256,
            ["independent_scorer_model_sha256"] = receipt.EvaluationModelSha256,
            ["independent_scorer_provenance_kind"] =
                receipt.ScorerProvenance.Kind,
            ["independent_scorer_id"] = receipt.ScorerProvenance.ScorerId,
            ["independent_scorer_version"] =
                receipt.ScorerProvenance.ScorerVersion,
            ["independent_scorer_request_id"] =
                receipt.ScorerProvenance.RequestId,
            ["independent_scorer_output_id"] =
                receipt.ScorerProvenance.OutputId,
            ["independent_scorer_runtime_instance_ref"] =
                receipt.ScorerProvenance.RuntimeInstanceRef,
            ["independent_scorer_replay_nonce"] = receipt.ReplayNonce,
            ["raw_independent_scorer_request_persisted"] = bool.FalseString,
            ["raw_independent_scorer_output_persisted"] = bool.FalseString,
            ["deterministic_measurement_used_as_admission_veto"] = bool.TrueString,
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
                bool.FalseString,
            ["review_required"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["approval_status"] = "pending",
            ["review_status"] = "runtime-candidate-review-pending",
            ["approved_for_training"] = bool.FalseString,
            ["training_candidate_only"] = bool.TrueString,
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["runtime_rejected_independently_evaluated"] = bool.TrueString,
            ["high_quality_negative_pair_proven"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["local_artifact_db"] = bool.TrueString,
            ["raw_payloads_included"] = bool.FalseString
        };
        AppendRouteAdmissionBindingMetadata(metadata, routeBinding);
        var statePayload = JsonSerializer.Serialize(
            new
            {
                schema = "design-conversion-unit-state-v1",
                unit_id = unitId,
                focus = string.Equals(
                    unitId,
                    DesignConversionDpoUnits.CriticUnit,
                    StringComparison.Ordinal)
                        ? "critique"
                        : "improvement",
                attempt_number = pair.AttemptNumber
            },
            CompactJsonOptions);
        var sourceStructurePayload = JsonSerializer.Serialize(
            new
            {
                schema = "design-conversion-runtime-source-structure-v1",
                tree = new[] { "frame", "candidate" },
                generated_binding = authoring.GeneratedBinding,
                incumbent_binding = authoring.IncumbentBinding,
                redacted = true
            },
            CompactJsonOptions);
        return new DesignConversionStoredDpoExample(
            ExampleId: Sha256Ref(string.Join("|",
            [
                receipt.ReceiptSha256,
                authoring.GeneratedRef,
                authoring.IncumbentRef,
                unitId
            ])),
            UnitId: unitId,
            InternalUnitStatePayload: statePayload,
            InternalVectorPayload: [0.12, 0.34, 0.56],
            SourceStructurePayload: sourceStructurePayload,
            Chosen: chosen,
            Rejected: rejected,
            TieBreakerPayload: null,
            UsefulnessScore: Math.Round(
                Math.Clamp(
                    chosenScore + Math.Max(0.01, chosenScore - rejectedScore),
                    0.1,
                    1.5),
                6),
            NegativeQualityReason: rejectedEvidence.NegativeQualityReason,
            NegativeReasonTaxonomy: rejectedEvidence.NegativeReasonTaxonomy,
            Provenance: new DesignConversionDpoProvenance(
                SourceRequestId: DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-source-request",
                    request.RequestId),
                SourceKind: request.SourceKind.ToString(),
                SourceManifestRef: Sha256Ref(string.Join("|",
                [
                    authoring.GeneratedBinding,
                    authoring.IncumbentBinding,
                    receipt.ArtifactRootBinding
                ])),
                ReviewerRef: "runtime-local-artifact-reviewer",
                CreatedAtUtc: createdAtUtc,
                Metadata: metadata),
            License: new DesignConversionLicenseScope(
                LicenseId: "runtime-local-artifact-db-training-candidate-only",
                RightsScope: "local_artifact_db_candidate_review_only",
                AllowsTraining: true,
                ContainsUserPrivateData: false,
                Metadata: metadata),
            TeacherEvidenceRefs: chosenEvaluation.EvidenceRefs
                .Concat(rejectedEvaluation.EvidenceRefs)
                .Concat(authoring.EvidenceRefs)
                .Concat(receiptRefs)
                .Append(
                    DesignConversionDpoTeacherAuthoringCandidateStrategy
                        .CandidateAuthoringMarker)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Metadata: metadata,
            TeacherProvenance: authoring.Provenance)
        {
            // Эти поля берутся из фактических evaluation сторон chosen/rejected,
            // поэтому переворот направления receipt меняет авторов даже при
            // одинаковых HTML/CSS и профиле score.
            ChosenAuthorKind = pairAuthors.ChosenAuthor,
            RejectedAuthorKind = pairAuthors.RejectedAuthor
        };
    }

    private static DesignConversionStoredDpoExample
        BuildTeacherCandidateOnlyStoredExample(
            DesignConversionTeacherAuthoredEvaluationPair pair,
            DesignConversionInternalEvaluationResult generatedEvaluation,
            DesignConversionInternalEvaluationResult incumbentEvaluation,
            RuntimeRejectedCandidateEvidence rejectedEvidence,
            RouteAdmissionBindingResolution routeBinding,
            DateTimeOffset createdAtUtc)
    {
        var authoring = pair.AuthoringEvidence;
        var request = generatedEvaluation.SubmittedRequest;
        var chosenOutput = generatedEvaluation.RuntimeFlowResult.Output;
        var chosenScore = Math.Max(
            0.01,
            Math.Clamp(
                chosenOutput.Verdict.Score,
                0.0,
                1.0));
        var rejectedScore = Math.Round(rejectedEvidence.VisualScore, 6);
        var chosen = chosenOutput with
        {
            Html = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-teacher-candidate-only-chosen-html",
                generatedEvaluation.SubmittedRequest.Html),
            Css = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-teacher-candidate-only-chosen-css",
                generatedEvaluation.SubmittedRequest.Css),
            EvidenceRefs = chosenOutput.EvidenceRefs
                .Concat(generatedEvaluation.EvidenceRefs)
                .Concat(authoring.EvidenceRefs)
                .Append(
                    "runtime_created_dpo_candidate:teacher_candidate_only_chosen_ref")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            QaResults = BuildRuntimeQaResults(generatedEvaluation),
            Verdict = chosenOutput.Verdict with { Score = chosenScore },
            Accepted = true
        };
        var rejected = new DesignConversionImprovementOutput(
            OutputId: rejectedEvidence.OutputId,
            CandidateId: rejectedEvidence.CandidateId,
            Html: rejectedEvidence.HtmlRef,
            Css: rejectedEvidence.CssRef,
            Verdict: new VisualVerdict(
                VerdictId: rejectedEvidence.VerdictId,
                Score: rejectedScore,
                Defects: rejectedEvidence.Defects,
                EvidenceRefs: rejectedEvidence.EvidenceRefs,
                Passed: false),
            QaResults: rejectedEvidence.QaResults,
            EvidenceRefs: rejectedEvidence.EvidenceRefs
                .Concat(incumbentEvaluation.EvidenceRefs)
                .Concat(authoring.EvidenceRefs)
                .Append(
                    "runtime_created_dpo_candidate:teacher_candidate_only_incumbent_ref")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Accepted: false);

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["runtime_teacher_authored_candidate"] = bool.TrueString,
            ["candidate_authoring_only"] = bool.TrueString,
            ["teacher_candidate_only_boundary"] =
                "server_owned_route_admission_bound_pending_review",
            ["teacher_authoring_schema"] = authoring.Schema,
            ["teacher_authoring_output_hash"] = authoring.OutputHash,
            ["teacher_authoring_incumbent_ref"] = authoring.IncumbentRef,
            ["teacher_authoring_generated_ref"] = authoring.GeneratedRef,
            ["teacher_inference_profile_id"] = authoring.InferenceProfileId,
            ["teacher_inference_profile_configuration_sha256"] =
                authoring.InferenceProfileConfigurationSha256,
            ["raw_teacher_prompt_persisted"] = bool.FalseString,
            ["raw_teacher_output_persisted"] = bool.FalseString,
            ["raw_teacher_rationale_persisted"] = bool.FalseString,
            ["independent_preference_proven"] = bool.FalseString,
            ["independent_preference_not_proven"] = bool.TrueString,
            ["chosen_rejected_pair_proven"] = bool.FalseString,
            ["chosen_rejected_pair_not_proven"] = bool.TrueString,
            [DesignConversionDpoPolicyCheckpointBridge
                .PairAuthorshipSourceMetadataKey] =
                    "teacher_candidate_only_direction",
            [DesignConversionDpoPolicyCheckpointBridge
                .PairChosenSourceSideMetadataKey] =
                    DesignConversionDpoPolicyCheckpointBridge
                        .PairSourceSideGenerated,
            [DesignConversionDpoPolicyCheckpointBridge
                .PairRejectedSourceSideMetadataKey] =
                    DesignConversionDpoPolicyCheckpointBridge
                        .PairSourceSideIncumbent,
            [DesignConversionDpoPolicyCheckpointBridge
                .PairChosenAuthorKindMetadataKey] =
                    DpoPairSideAuthorKind.OpenTeacher.ToString(),
            [DesignConversionDpoPolicyCheckpointBridge
                .PairRejectedAuthorKindMetadataKey] =
                    DpoPairSideAuthorKind.LocalModel.ToString(),
            ["runtime_rejected_independently_evaluated"] = bool.FalseString,
            ["high_quality_negative_pair_proven"] = bool.FalseString,
            ["high_quality_negative_pair_not_proven"] = bool.TrueString,
            ["runtime_teacher_attestation_verified"] = bool.TrueString,
            ["deterministic_measurement_used_as_admission_veto"] =
                bool.TrueString,
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
                bool.FalseString,
            ["review_required"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["approval_status"] = "pending",
            ["review_status"] =
                "runtime-candidate-review-pending",
            ["approved_for_training"] = bool.FalseString,
            ["training_candidate_only"] = bool.TrueString,
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["local_artifact_db"] = bool.TrueString,
            ["raw_payloads_included"] = bool.FalseString
        };
        AppendRouteAdmissionBindingMetadata(metadata, routeBinding);

        var statePayload = JsonSerializer.Serialize(
            new
            {
                schema =
                    "design-conversion-teacher-candidate-only-unit-state-v1",
                unit_id = DesignConversionDpoUnits.ImproverUnit,
                focus = "improvement",
                attempt_number = pair.AttemptNumber
            },
            CompactJsonOptions);
        var sourceStructurePayload = JsonSerializer.Serialize(
            new
            {
                schema =
                    "design-conversion-teacher-candidate-only-source-structure-v1",
                tree = new[] { "frame", "candidate" },
                generated_binding = authoring.GeneratedBinding,
                incumbent_binding = authoring.IncumbentBinding,
                route_admission_tenant_bound = routeBinding.TenantBound,
                redacted = true
            },
            CompactJsonOptions);

        return new DesignConversionStoredDpoExample(
            ExampleId: Sha256Ref(string.Join("|",
            [
                "teacher-candidate-only",
                authoring.GeneratedRef,
                authoring.IncumbentRef,
                pair.AttemptNumber.ToString(CultureInfo.InvariantCulture),
                routeBinding.TenantIdRef,
                routeBinding.ScopeRef
            ])),
            UnitId: DesignConversionDpoUnits.ImproverUnit,
            InternalUnitStatePayload: statePayload,
            InternalVectorPayload: [0.18, 0.37, 0.59],
            SourceStructurePayload: sourceStructurePayload,
            Chosen: chosen,
            Rejected: rejected,
            TieBreakerPayload: null,
            UsefulnessScore: Math.Round(
                Math.Clamp(
                    chosenScore + Math.Max(0.01, chosenScore - rejectedScore),
                    0.1,
                    1.5),
                6),
            NegativeQualityReason: rejectedEvidence.NegativeQualityReason,
            NegativeReasonTaxonomy: rejectedEvidence.NegativeReasonTaxonomy,
            Provenance: new DesignConversionDpoProvenance(
                SourceRequestId: DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-source-request",
                    request.RequestId),
                SourceKind: request.SourceKind.ToString(),
                SourceManifestRef: Sha256Ref(string.Join("|",
                [
                    "teacher-candidate-only",
                    authoring.GeneratedBinding,
                    authoring.IncumbentBinding,
                    routeBinding.TenantIdRef,
                    routeBinding.ScopeRef
                ])),
                ReviewerRef: "runtime-local-artifact-reviewer",
                CreatedAtUtc: createdAtUtc,
                Metadata: metadata),
            License: new DesignConversionLicenseScope(
                LicenseId:
                    "runtime-local-artifact-db-teacher-candidate-only",
                RightsScope:
                    "local_artifact_db_candidate_review_only",
                AllowsTraining: true,
                ContainsUserPrivateData: false,
                Metadata: metadata),
            TeacherEvidenceRefs: generatedEvaluation.EvidenceRefs
                .Concat(incumbentEvaluation.EvidenceRefs)
                .Concat(authoring.EvidenceRefs)
                .Concat(rejectedEvidence.EvidenceRefs)
                .Append(
                    DesignConversionDpoTeacherAuthoringCandidateStrategy
                        .CandidateAuthoringMarker)
                .Append("server_owned_route_admission_bound")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Metadata: metadata,
            TeacherProvenance: authoring.Provenance)
        {
            // Строки candidate-only показывают направление авторства для
            // аудита, но без независимого preference receipt они не становятся
            // строками обучения.
            ChosenAuthorKind = DpoPairSideAuthorKind.OpenTeacher,
            RejectedAuthorKind = DpoPairSideAuthorKind.LocalModel
        };
    }

    private static DesignConversionDpoTrainingRecord
        BuildServerOwnedSingleEvaluationBaselineStoredExample(
            DesignConversionInternalEvaluationResult evaluation,
            RuntimeRejectedCandidateEvidence rejectedEvidence,
            RouteAdmissionBindingResolution routeBinding,
            DateTimeOffset createdAtUtc)
    {
        var request = evaluation.SubmittedRequest;
        var chosenOutput = evaluation.RuntimeFlowResult.Output;
        var chosenScore = Math.Max(
            0.01,
            Math.Clamp(chosenOutput.Verdict.Score, 0.0, 1.0));
        var rejectedScore = Math.Round(rejectedEvidence.VisualScore, 6);
        var chosen = chosenOutput with
        {
            Html = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-server-owned-baseline-chosen-html",
                evaluation.ImprovedHtml),
            Css = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-server-owned-baseline-chosen-css",
                evaluation.ImprovedCss),
            EvidenceRefs = chosenOutput.EvidenceRefs
                .Concat(evaluation.EvidenceRefs)
                .Append(
                    "runtime_created_dpo_candidate:server_owned_baseline_chosen_ref")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            QaResults = BuildRuntimeQaResults(evaluation),
            Verdict = chosenOutput.Verdict with { Score = chosenScore },
            Accepted = true
        };
        var rejected = new DesignConversionImprovementOutput(
            OutputId: rejectedEvidence.OutputId,
            CandidateId: rejectedEvidence.CandidateId,
            Html: rejectedEvidence.HtmlRef,
            Css: rejectedEvidence.CssRef,
            Verdict: new VisualVerdict(
                VerdictId: rejectedEvidence.VerdictId,
                Score: rejectedScore,
                Defects: rejectedEvidence.Defects,
                EvidenceRefs: rejectedEvidence.EvidenceRefs,
                Passed: false),
            QaResults: rejectedEvidence.QaResults,
            EvidenceRefs: rejectedEvidence.EvidenceRefs
                .Append(
                    "runtime_created_dpo_candidate:server_owned_baseline_rejected_ref")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Accepted: false);

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["runtime_created_dpo_candidate_boundary"] =
                "server_owned_single_evaluation_baseline_pending_review",
            ["runtime_teacher_authored_candidate"] = bool.FalseString,
            ["candidate_authoring_only"] = bool.FalseString,
            ["independent_preference_proven"] = bool.FalseString,
            ["independent_preference_not_proven"] = bool.TrueString,
            ["chosen_rejected_pair_proven"] = bool.FalseString,
            ["chosen_rejected_pair_not_proven"] = bool.TrueString,
            ["runtime_rejected_independently_evaluated"] = bool.FalseString,
            ["runtime_rejected_baseline_source"] = rejectedEvidence.EvidenceSource,
            ["high_quality_negative_pair_proven"] = bool.FalseString,
            ["high_quality_negative_pair_not_proven"] = bool.TrueString,
            ["runtime_teacher_attestation_verified"] = bool.FalseString,
            ["deterministic_measurement_used_as_admission_veto"] =
                bool.TrueString,
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
                bool.FalseString,
            ["review_required"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["approval_status"] = "pending",
            ["review_status"] =
                "runtime-candidate-review-pending",
            ["approved_for_training"] = bool.FalseString,
            ["training_candidate_only"] = bool.TrueString,
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["local_artifact_db"] = bool.TrueString,
            ["raw_payloads_included"] = bool.FalseString
        };
        AppendRouteAdmissionBindingMetadata(metadata, routeBinding);

        var statePayload = JsonSerializer.Serialize(
            new
            {
                schema =
                    "design-conversion-server-owned-baseline-unit-state-v1",
                unit_id = DesignConversionDpoUnits.ImproverUnit,
                focus = "improvement",
                request_ref = DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-runtime-server-owned-baseline-request",
                    request.RequestId)
            },
            CompactJsonOptions);
        var sourceStructurePayload = JsonSerializer.Serialize(
            new
            {
                schema =
                    "design-conversion-server-owned-baseline-source-structure-v1",
                tree = new[] { "frame", "candidate" },
                route_admission_tenant_bound = routeBinding.TenantBound,
                baseline_ref = DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-runtime-server-owned-baseline-source",
                    string.Concat(request.Html, "|", request.Css)),
                generated_ref = DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-runtime-server-owned-baseline-generated",
                    string.Concat(evaluation.ImprovedHtml, "|", evaluation.ImprovedCss)),
                redacted = true
            },
            CompactJsonOptions);

        return new DesignConversionDpoTrainingRecord(
            RecordId: Sha256Ref(string.Join("|",
            [
                "server-owned-single-evaluation-baseline",
                request.RequestId,
                chosenOutput.OutputId,
                rejectedEvidence.OutputId,
                routeBinding.TenantIdRef,
                routeBinding.ScopeRef
            ])),
            UnitId: DesignConversionDpoUnits.ImproverUnit,
            InternalUnitStatePayload: statePayload,
            InternalVectorPayload: [0.16, 0.31, 0.53],
            SourceStructurePayload: sourceStructurePayload,
            Chosen: chosen,
            Rejected: rejected,
            TieBreakerPayload: null,
            UsefulnessScore: Math.Round(
                Math.Clamp(
                    chosenScore + Math.Max(0.01, chosenScore - rejectedScore),
                    0.1,
                    1.5),
                6),
            NegativeQualityReason: rejectedEvidence.NegativeQualityReason,
            NegativeReasonTaxonomy: rejectedEvidence.NegativeReasonTaxonomy,
            Provenance: new DesignConversionDpoProvenance(
                SourceRequestId: DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-source-request",
                    request.RequestId),
                SourceKind: request.SourceKind.ToString(),
                SourceManifestRef: Sha256Ref(string.Join("|",
                [
                    "server-owned-single-evaluation-baseline",
                    request.Provenance.TryGetValue(
                        "source_manifest_ref",
                        out var sourceManifestRef)
                            ? sourceManifestRef
                            : request.DesignTreePayload.PayloadId,
                    routeBinding.TenantIdRef,
                    routeBinding.ScopeRef
                ])),
                ReviewerRef: "runtime-local-artifact-reviewer",
                CreatedAtUtc: createdAtUtc,
                Metadata: metadata),
            License: new DesignConversionLicenseScope(
                LicenseId:
                    "runtime-local-artifact-db-server-owned-baseline-candidate-only",
                RightsScope:
                    "local_artifact_db_candidate_review_only",
                AllowsTraining: true,
                ContainsUserPrivateData: false,
                Metadata: metadata),
            Quarantine: new DesignConversionQuarantineState(
                IsQuarantined: false,
                Reason: "runtime-candidate-review-pending",
                EvidenceRefs:
                    rejectedEvidence.EvidenceRefs
                        .Append(
                            "runtime_created_dpo_candidate:server_owned_baseline_pending_review")
                        .Distinct(StringComparer.Ordinal)
                        .Order(StringComparer.Ordinal)
                        .ToArray()),
            TeacherProvenance: null);
    }

    private static DesignConversionDpoTrainingRecord
        AttachIndependentPreferencePayloadCustody(
            DesignConversionDpoTrainingRecord record,
            DesignConversionInternalEvaluationResult chosenEvaluation,
            DesignConversionInternalEvaluationResult rejectedEvaluation)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var chosenHtml = chosenEvaluation.SubmittedRequest.Html ?? string.Empty;
        var chosenCss = chosenEvaluation.SubmittedRequest.Css ?? string.Empty;
        var rejectedHtml = rejectedEvaluation.SubmittedRequest.Html ?? string.Empty;
        var rejectedCss = rejectedEvaluation.SubmittedRequest.Css ?? string.Empty;
        if (!IsIndependentPreferenceCustodyScalarAllowed(
                chosenHtml, "payload_custody_chosen_html", redactions) ||
            !IsIndependentPreferenceCustodyScalarAllowed(
                chosenCss, "payload_custody_chosen_css", redactions) ||
            !IsIndependentPreferenceCustodyScalarAllowed(
                rejectedHtml, "payload_custody_rejected_html", redactions) ||
            !IsIndependentPreferenceCustodyScalarAllowed(
                rejectedCss, "payload_custody_rejected_css", redactions))
        {
            return record;
        }

        var materialized = record with
        {
            Chosen = record.Chosen with { Html = chosenHtml, Css = chosenCss },
            Rejected = record.Rejected with
            {
                Html = rejectedHtml,
                Css = rejectedCss
            },
            PayloadCustodySecret = null
        };
        var item = DesignConversionTrainingPipeline.ToCanonicalTrainingItem(
            materialized);
        var promptSha256 = Sha256Ref(item.Prompt);
        var chosenSha256 = Sha256Ref(item.ChosenResponse);
        var rejectedSha256 = Sha256Ref(item.RejectedResponse);
        var governanceSha256 = BuildPayloadCustodyGovernanceSha256(record);
        var bindingSha256 = BuildPayloadCustodyBinding(
            record,
            promptSha256,
            chosenSha256,
            rejectedSha256,
            governanceSha256);
        var metadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            [PayloadCustodyRequiredMetadataKey] = bool.TrueString,
            [PayloadCustodyVersionMetadataKey] = PayloadCustodyVersion,
            [PayloadCustodyPromptSha256MetadataKey] = promptSha256,
            [PayloadCustodyChosenSha256MetadataKey] = chosenSha256,
            [PayloadCustodyRejectedSha256MetadataKey] = rejectedSha256,
            [PayloadCustodyGovernanceSha256MetadataKey] = governanceSha256,
            [PayloadCustodyBindingSha256MetadataKey] = bindingSha256,
            ["payload_custody_raw_in_public_record"] = bool.FalseString
        };
        return record with
        {
            Provenance = record.Provenance with { Metadata = metadata },
            PayloadCustodySecret = new DesignConversionDpoPayloadCustodySecret(
                PayloadCustodyVersion,
                item.Prompt,
                item.ChosenResponse,
                item.RejectedResponse,
                promptSha256,
                chosenSha256,
                rejectedSha256,
                governanceSha256,
                bindingSha256)
        };
    }

    private static bool IsIndependentPreferenceCustodyScalarAllowed(
        string value,
        string scope,
        SortedSet<string> redactions)
    {
        try
        {
            _ = new UTF8Encoding(false, true).GetByteCount(value);
        }
        catch (EncoderFallbackException)
        {
            return false;
        }

        if (Encoding.UTF8.GetByteCount(value) > 4 * 1024 * 1024)
            return false;

        var sanitized = DesignConversionDpoLeakageGuard.SanitizeScalar(
            value,
            scope,
            redactions);
        return redactions.Count == 0 &&
               string.Equals(sanitized, value, StringComparison.Ordinal);
    }

    internal static string BuildPayloadCustodyGovernanceSha256(
        DesignConversionDpoTrainingRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var teacher = record.TeacherProvenance;
        var snapshot = new
        {
            schema = "design-conversion-dpo-immutable-governance-v1",
            provenance = new
            {
                source_request_id = record.Provenance.SourceRequestId ?? string.Empty,
                source_kind = record.Provenance.SourceKind ?? string.Empty,
                source_manifest_ref = record.Provenance.SourceManifestRef ?? string.Empty,
                created_at_utc = record.Provenance.CreatedAtUtc
                    .ToUniversalTime()
                    .ToString("O", CultureInfo.InvariantCulture),
                immutable_metadata = CanonicalizeCustodyGovernanceMetadata(
                    record.Provenance.Metadata)
            },
            license = new
            {
                license_id = record.License.LicenseId ?? string.Empty,
                rights_scope = record.License.RightsScope ?? string.Empty,
                allows_training = record.License.AllowsTraining,
                contains_user_private_data = record.License.ContainsUserPrivateData,
                immutable_metadata = CanonicalizeCustodyGovernanceMetadata(
                    record.License.Metadata)
            },
            quarantine = new
            {
                is_quarantined = record.Quarantine.IsQuarantined,
                reason = record.Quarantine.Reason ?? string.Empty,
                evidence_refs = (record.Quarantine.EvidenceRefs ?? [])
                    .Order(StringComparer.Ordinal)
                    .ToArray()
            },
            teacher_provenance = teacher is null
                ? null
                : new
                {
                    source_kind = teacher.SourceKind.ToString(),
                    family = teacher.Family?.ToString() ?? string.Empty,
                    access_mode = teacher.AccessMode?.ToString() ?? string.Empty,
                    hosting_provider = teacher.HostingProvider?.ToString() ?? string.Empty,
                    model_id = teacher.ModelId ?? string.Empty,
                    model_version = teacher.ModelVersion ?? string.Empty,
                    model_license = teacher.ModelLicense?.ToString() ?? string.Empty,
                    provider_terms_checked = teacher.ProviderTermsChecked,
                    provider_terms_allow_training = teacher.ProviderTermsAllowTraining,
                    closed_proprietary_model = teacher.ClosedProprietaryModel,
                    teacher_output_used_for_training = teacher.TeacherOutputUsedForTraining,
                    request_id = teacher.RequestId ?? string.Empty,
                    invocation_count = teacher.InvocationCount,
                    model_artifact_digest_kind = teacher.ModelArtifactDigestKind ?? string.Empty,
                    model_artifact_digest = teacher.ModelArtifactDigest ?? string.Empty
                }
        };

        return Sha256Ref(JsonSerializer.Serialize(snapshot, CompactJsonOptions));
    }

    private static IReadOnlyDictionary<string, string>
        CanonicalizeCustodyGovernanceMetadata(
            IReadOnlyDictionary<string, string>? metadata)
    {
        var canonical = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in CustodyGovernanceMetadataDefaults)
            canonical[pair.Key] = pair.Value;
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            var key = pair.Key ?? string.Empty;
            if (MutableCustodyGovernanceMetadataKeys.Contains(key))
                continue;

            canonical[key] = pair.Value ?? string.Empty;
        }

        return canonical;
    }

    internal static string BuildPayloadCustodyBinding(
        DesignConversionDpoTrainingRecord record,
        string promptSha256,
        string chosenSha256,
        string rejectedSha256,
        string governanceSha256)
        => Sha256Ref(string.Join("|",
        [
            PayloadCustodyVersion,
            record.RecordId,
            record.UnitId,
            record.Chosen.OutputId,
            record.Chosen.CandidateId,
            record.Chosen.Verdict.Score.ToString("R", CultureInfo.InvariantCulture),
            record.Chosen.Accepted.ToString(CultureInfo.InvariantCulture),
            record.Rejected.OutputId,
            record.Rejected.CandidateId,
            record.Rejected.Verdict.Score.ToString("R", CultureInfo.InvariantCulture),
            record.Rejected.Accepted.ToString(CultureInfo.InvariantCulture),
            record.UsefulnessScore.ToString("R", CultureInfo.InvariantCulture),
            record.NegativeQualityReason,
            record.NegativeReasonTaxonomy,
            Sha256Ref(record.TieBreakerPayload ?? string.Empty),
            record.Provenance.SourceManifestRef,
            record.License.LicenseId,
            record.License.RightsScope,
            promptSha256,
            chosenSha256,
            rejectedSha256,
            governanceSha256
        ]));

    internal static string Sha256Ref(string value)
        => $"sha256:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty))).ToLowerInvariant()}";

    private static DesignConversionTeacherAuthoredEvaluationPair
        NormalizeMeasuredVisualScoresForDpo(
            DesignConversionTeacherAuthoredEvaluationPair pair)
    {
        if (pair.GeneratedEvaluation is null || pair.IncumbentEvaluation is null)
            return pair;

        return pair with
        {
            GeneratedEvaluation = NormalizeMeasuredVisualScoreForDpo(
                pair.GeneratedEvaluation),
            IncumbentEvaluation = NormalizeMeasuredVisualScoreForDpo(
                pair.IncumbentEvaluation)
        };
    }

    internal OperationResult<bool> CanBuildRuntimeCreatedTrainingCandidatePair(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionInternalEvaluationResult rejectedEvaluation)
    {
        if (evaluation is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion runtime evaluation is required to create DPO training candidates.",
                "DESIGN_CONVERSION_RUNTIME_EVALUATION_REQUIRED");
        }

        if (ValidateRuntimeCreatedCandidateEvidence(evaluation).Count > 0)
            return OperationResult<bool>.Ok(false);

        return OperationResult<bool>.Ok(
            TryBuildRuntimeRejectedCandidateEvidence(
                evaluation,
                rejectedEvaluation,
                out _,
                out _));
    }

    private OperationResult<DesignConversionDpoBuildResult>
        BuildRuntimeCreatedTrainingCandidatesCore(
            DesignConversionInternalEvaluationResult evaluation,
            DesignConversionInternalEvaluationResult rejectedEvaluation,
            DateTimeOffset? createdAtUtc = null)
    {
        if (evaluation is null)
            return OperationResult<DesignConversionDpoBuildResult>.Fail(
                "DesignConversion runtime evaluation is required to create DPO training candidates.",
                "DESIGN_CONVERSION_RUNTIME_EVALUATION_REQUIRED");

        var evidenceGate = ValidateRuntimeCreatedCandidateEvidence(evaluation);
        if (evidenceGate.Count > 0)
        {
            var reason = string.Join(";", evidenceGate);
            return OperationResult<DesignConversionDpoBuildResult>.Ok(new DesignConversionDpoBuildResult(
                AcceptedRecords: [],
                QuarantinedRecords: [],
                Warnings: BuildRuntimeCandidateNotCreatedWarnings(reason)));
        }

        if (!TryBuildRuntimeRejectedCandidateEvidence(
                evaluation,
                rejectedEvaluation,
                out _,
                out var rejectedEvidenceReason))
        {
            var candidateOnly =
                BuildRuntimeCreatedTrainingCandidates(evaluation, createdAtUtc);
            if (!candidateOnly.IsSuccess ||
                candidateOnly.Value!.AcceptedRecords.Count == 0)
            {
                return OperationResult<DesignConversionDpoBuildResult>.Ok(
                    new DesignConversionDpoBuildResult(
                        AcceptedRecords: [],
                        QuarantinedRecords: [],
                        Warnings:
                            BuildRuntimeCandidateNotCreatedWarnings(
                                rejectedEvidenceReason)
                                .Concat(candidateOnly.IsSuccess
                                    ? candidateOnly.Value!.Warnings
                                    : [candidateOnly.ErrorCode ??
                                       "runtime_single_evaluation_candidate_build_failed"])
                                .Distinct(StringComparer.Ordinal)
                                .ToArray()));
            }

            return OperationResult<DesignConversionDpoBuildResult>.Ok(
                new DesignConversionDpoBuildResult(
                    AcceptedRecords: candidateOnly.Value.AcceptedRecords,
                    QuarantinedRecords: candidateOnly.Value.QuarantinedRecords,
                    Warnings: candidateOnly.Value.Warnings
                        .Concat([
                            "runtime_rejected_candidate_pair_unavailable",
                            $"runtime_rejected_candidate_pair_unavailable_reason={rejectedEvidenceReason}"
                        ])
                        .Distinct(StringComparer.Ordinal)
                        .ToArray()));
        }

        return OperationResult<DesignConversionDpoBuildResult>.Ok(
            new DesignConversionDpoBuildResult(
                AcceptedRecords: [],
                QuarantinedRecords: [],
                Warnings: BuildRuntimeTeacherAttestationNotCreatedWarnings(
                    RuntimeTeacherAttestationRequiredReason)));
    }

    private static IReadOnlyList<string> ValidateRuntimeCreatedCandidateEvidence(
        DesignConversionInternalEvaluationResult evaluation)
    {
        var reasons = new List<string>();
        if (evaluation.VisualQaBoundaryTrace?.VisualDecision?.CanPersistTrainingCandidate != true)
            reasons.Add(RuntimeCandidateCanPersistFalseReason);

        var hasChosenOutput =
            evaluation.RuntimeFlowResult?.Output is { } output &&
            !string.IsNullOrWhiteSpace(output.OutputId) &&
            !string.IsNullOrWhiteSpace(output.CandidateId);
        var hasRejectedBaseline =
            evaluation.SubmittedRequest is { } request &&
            !string.IsNullOrWhiteSpace(request.RequestId) &&
            !string.IsNullOrWhiteSpace(request.Html) &&
            !string.IsNullOrWhiteSpace(request.Css);
        if (!hasChosenOutput || !hasRejectedBaseline)
            reasons.Add("chosen_rejected_pair_not_proven");

        if (evaluation.RenderResults is null || evaluation.RenderResults.Count == 0)
            reasons.Add("render_evidence_not_proven");

        if (evaluation.ComparisonArtifacts is null || evaluation.ComparisonArtifacts.Count == 0)
            reasons.Add("comparison_evidence_not_proven");

        if (evaluation.VisualJudgeResults is null || evaluation.VisualJudgeResults.Count == 0)
            reasons.Add("visual_judge_evidence_not_proven");

        if (!HasRuntimeNegativeQualityEvidence(evaluation))
            reasons.Add("negative_quality_evidence_not_proven");

        return reasons.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static bool HasRuntimeNegativeQualityEvidence(
        DesignConversionInternalEvaluationResult evaluation)
        => evaluation.ComparisonArtifacts?.Any(HasComparisonNegativeQualityEvidence) == true ||
           evaluation.VisualJudgeResults?.Any(HasVisualJudgeNegativeQualityEvidence) == true;

    private static bool HasComparisonNegativeQualityEvidence(
        DesignConversionQaComparisonArtifact artifact)
        => artifact is not null &&
           (artifact.Verdict.Defects.Count > 0 ||
            !artifact.Verdict.Passed ||
            artifact.Verdict.Score < 0.999 ||
            MetricEquals(artifact.Metrics, "sha256_match", 0.0) ||
            MetricEquals(artifact.Metrics, "label_match", 0.0) ||
            MetricEquals(artifact.Metrics, "metadata_fingerprint_match", 0.0) ||
            MetricGreaterThan(artifact.Metrics, "visual_distance", 0.0));

    private static bool HasVisualJudgeNegativeQualityEvidence(
        DesignConversionVisualJudgeResult result)
        => result is not null &&
           !result.IsBlocked &&
           (result.Verdict.Defects.Count > 0 ||
            !result.Verdict.Passed ||
            result.Verdict.Score < 0.999);

    private static bool MetricEquals(
        IReadOnlyDictionary<string, double> metrics,
        string key,
        double expected)
        => metrics is not null &&
           metrics.TryGetValue(key, out var value) &&
           Math.Abs(value - expected) < 0.000001;

    private static bool MetricGreaterThan(
        IReadOnlyDictionary<string, double> metrics,
        string key,
        double floor)
        => metrics is not null &&
           metrics.TryGetValue(key, out var value) &&
           value > floor;

    private static IReadOnlyList<string> BuildRuntimeCandidateNotCreatedWarnings(
        string reason)
    {
        List<string> warnings =
        [
            "runtime_created_dpo_candidate_not_created_insufficient_evidence",
            $"runtime_created_dpo_candidate_quarantine_reason={reason}",
            "runtime_created_dpo_candidate=false",
            "chosen_rejected_pair_or_negative_quality_not_proven",
            "training_candidate_only=false",
            "approved_for_training=false",
            "production_dpo=false",
            "product_not_ready",
            "model_readiness_claimed=false",
            "high_quality_negative_pair_not_proven",
            "held_out_not_proven",
            "source_disjoint_not_proven"
        ];

        if (reason.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(RuntimeCandidateCanPersistFalseReason, StringComparer.Ordinal))
        {
            warnings.Insert(0, RuntimeCandidateCanPersistFalseWarning);
        }

        return warnings;
    }

    private static IReadOnlyList<string>
        BuildRuntimeServerOwnedBaselineCreatedWarnings()
        =>
        [
            "runtime_created_dpo_candidate",
            "runtime_created_dpo_candidate_review_pending",
            "runtime_created_dpo_candidate_approval_required",
            "server_owned_single_evaluation_baseline_candidate=true",
            "independent_preference_proven=false",
            "independent_preference_not_proven",
            "chosen_rejected_pair_proven=false",
            "chosen_rejected_pair_not_proven",
            "runtime_rejected_independently_evaluated=false",
            "high_quality_negative_pair_proven=false",
            "high_quality_negative_pair_not_proven",
            "runtime_teacher_attestation_verified=false",
            "runtime_teacher_authored_candidate=False",
            "candidate_authoring_only=false",
            "training_candidate_only=true",
            "training_grade=false",
            "not_training_grade=true",
            "approved_for_training=false",
            "production_dpo=false",
            "product_not_ready",
            "trained_model_authority=False",
            "held_out_not_proven",
            "source_disjoint_not_proven"
        ];

    private static IReadOnlyList<string>
        BuildRuntimeTeacherAuthoredPairNotCreatedWarnings(string reason)
        =>
        [
            "runtime_created_dpo_candidate_not_created_teacher_authored_pair_invalid",
            $"runtime_created_dpo_candidate_quarantine_reason={reason}",
            reason,
            "runtime_created_dpo_candidate=false",
            "independent_preference_not_proven",
            "chosen_rejected_pair_or_negative_quality_not_proven",
            "runtime_rejected_independently_evaluated=false",
            "high_quality_negative_pair_not_proven",
            "training_candidate_only=false",
            "training_grade=false",
            "not_training_grade=true",
            "approved_for_training=false",
            "production_dpo=false",
            "product_not_ready",
            "model_readiness_claimed=false",
            "trained_model_authority=False",
            "held_out_not_proven",
            "source_disjoint_not_proven"
        ];

    private static IReadOnlyList<string>
        BuildRuntimeTeacherCandidateOnlyNotCreatedWarnings(
            string reason = "independent_preference_not_proven")
        =>
        [
            "runtime_created_dpo_candidate_not_created_independent_preference_not_proven",
            $"runtime_created_dpo_candidate_quarantine_reason={reason}",
            reason,
            "runtime_created_dpo_candidate=false",
            "independent_preference_not_proven",
            "chosen_rejected_pair_or_negative_quality_not_proven",
            "runtime_rejected_independently_evaluated=false",
            "high_quality_negative_pair_not_proven",
            "runtime_teacher_authored_candidate=True",
            "candidate_authoring_only=true",
            "training_candidate_only=false",
            "training_grade=false",
            "not_training_grade=true",
            "approved_for_training=false",
            "production_dpo=false",
            "product_not_ready",
            "model_readiness_claimed=false",
            "trained_model_authority=False",
            "held_out_not_proven",
            "source_disjoint_not_proven"
        ];

    private static IReadOnlyList<string>
        BuildRuntimeTeacherCandidateOnlyCreatedWarnings()
        =>
        [
            "runtime_created_dpo_candidate",
            "runtime_created_dpo_candidate_review_pending",
            "runtime_created_dpo_candidate_approval_required",
            "independent_preference_proven=false",
            "independent_preference_not_proven",
            "chosen_rejected_pair_or_negative_quality_not_proven",
            "runtime_rejected_independently_evaluated=false",
            "high_quality_negative_pair_not_proven",
            "runtime_teacher_attestation_verified=true",
            "runtime_teacher_authored_candidate=True",
            "candidate_authoring_only=true",
            "training_candidate_only=true",
            "training_grade=false",
            "not_training_grade=true",
            "approved_for_training=false",
            "production_dpo=false",
            "product_not_ready",
            "trained_model_authority=False",
            "held_out_not_proven",
            "source_disjoint_not_proven"
        ];

    private static IReadOnlyList<string>
        BuildRuntimeTeacherAttestationNotCreatedWarnings(string reason)
        =>
        [
            "runtime_created_dpo_candidate_not_created_teacher_attestation_not_verified",
            $"runtime_created_dpo_candidate_quarantine_reason={reason}",
            "runtime_created_dpo_candidate=false",
            "chosen_rejected_pair_proven",
            "runtime_rejected_independently_evaluated=true",
            "high_quality_negative_pair_proven",
            "runtime_teacher_attestation_verified=false",
            "runtime_teacher_attestation_not_verified",
            reason,
            "runtime_created_dpo_candidate_review_status=not-created",
            "review_required=false",
            "approval_required=false",
            "approval_status=not-required",
            "training_candidate_only=false",
            "approved_for_training=false",
            "production_dpo=false",
            "product_not_ready",
            "model_readiness_claimed=false",
            "held_out_not_proven",
            "source_disjoint_not_proven"
        ];

    public OperationResult<DesignConversionDpoBuildResult> BuildTrainReadyRecords(
        IReadOnlyList<DesignConversionStoredDpoExample> examples)
    {
        if (examples is null)
            return OperationResult<DesignConversionDpoBuildResult>.Fail(
                "DesignConversion DPO examples are required.",
                "DESIGN_CONVERSION_DPO_EXAMPLES_REQUIRED");

        var accepted = new List<DesignConversionDpoTrainingRecord>();
        var quarantined = new List<DesignConversionDpoTrainingRecord>();

        for (var i = 0; i < examples.Count; i++)
        {
            var record = BuildRecord(examples[i], i + 1);
            if (record.Quarantine.IsQuarantined)
                quarantined.Add(record);
            else
                accepted.Add(record);
        }

        var orderedAccepted = accepted
            .OrderByDescending(record => record.UsefulnessScore)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();

        return OperationResult<DesignConversionDpoBuildResult>.Ok(new DesignConversionDpoBuildResult(
            AcceptedRecords: orderedAccepted,
            QuarantinedRecords: quarantined.ToArray(),
            Warnings:
            [
                "DESIGN_CONVERSION_DPO_RECORDS_ARE_TRAIN_READY_SCaffold_INPUTS_ONLY",
                "DETERMINISTIC_QA_EVIDENCE_IS_NOT_TREATED_AS_LEARNING"
            ]));
    }

    private static bool TryValidateTeacherAuthoredPairStructure(
        DesignConversionTeacherAuthoredEvaluationPair pair,
        out string reason)
    {
        reason = "runtime_teacher_candidate_authoring_binding_mismatch";
        var authoringEvidence = pair.AuthoringEvidence;
        if (pair.AttemptNumber <= 1 ||
            authoringEvidence is null ||
            pair.GeneratedEvaluation is null ||
            pair.IncumbentEvaluation is null ||
            authoringEvidence.AttemptNumber != pair.AttemptNumber ||
            !string.Equals(
                pair.GeneratedCandidateId,
                authoringEvidence.GeneratedCandidateId,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.Equals(
                authoringEvidence.Schema,
                DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                StringComparison.Ordinal) ||
            !IsLowerHexSha256(authoringEvidence.OutputHash) ||
            !string.Equals(
                authoringEvidence.InferenceProfileId,
                LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId,
                StringComparison.Ordinal) ||
            !string.Equals(
                authoringEvidence.InferenceProfileConfigurationSha256,
                LocalQwenOllamaInferenceProfileAuthority.Authoring
                .ConfigurationSha256,
                StringComparison.Ordinal) ||
            !DesignConversionDpoTeacherAttestationRuntime
                .HasCanonicalAuthoredProvenance(authoringEvidence.Provenance) ||
            !HasCanonicalTeacherCandidateAuthoringEnvelope(authoringEvidence))
        {
            return false;
        }

        if (!string.Equals(
                pair.GeneratedEvaluation.SubmittedRequest.RequestId,
                authoringEvidence.GeneratedRequestId,
                StringComparison.Ordinal) ||
            !string.Equals(
                DesignConversionDpoTeacherAttestationRuntime.BuildGeneratedBinding(
                    pair.GeneratedEvaluation.SubmittedRequest.Html,
                    pair.GeneratedEvaluation.SubmittedRequest.Css),
                authoringEvidence.GeneratedBinding,
                StringComparison.Ordinal) ||
            !string.Equals(
                DesignConversionDpoTeacherAttestationRuntime.BuildIncumbentBinding(
                    pair.IncumbentEvaluation.SubmittedRequest),
                authoringEvidence.IncumbentBinding,
                StringComparison.Ordinal) ||
            !string.Equals(
                DesignConversionDpoTeacherAttestationRuntime
                    .BuildGeneratedRefFromCandidateId(
                        authoringEvidence.GeneratedCandidateId),
                authoringEvidence.GeneratedRef,
                StringComparison.Ordinal) ||
            !string.Equals(
                DesignConversionDpoTeacherAttestationRuntime
                    .BuildGeneratedRequestIdFromCandidateId(
                        authoringEvidence.GeneratedCandidateId),
                authoringEvidence.GeneratedRequestId,
                StringComparison.Ordinal) ||
            !string.Equals(
                DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-dpo-teacher-incumbent",
                    authoringEvidence.IncumbentBinding),
                authoringEvidence.IncumbentRef,
                StringComparison.Ordinal) ||
            (string.Equals(
                 pair.GeneratedEvaluation.SubmittedRequest.Html,
                 pair.IncumbentEvaluation.SubmittedRequest.Html,
                 StringComparison.Ordinal) &&
             string.Equals(
                 pair.GeneratedEvaluation.SubmittedRequest.Css,
                 pair.IncumbentEvaluation.SubmittedRequest.Css,
                 StringComparison.Ordinal)))
        {
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool HasCanonicalTeacherCandidateAuthoringEnvelope(
        DesignConversionDpoTeacherCandidateAuthoringEvidence authoringEvidence)
    {
        if (authoringEvidence.Metadata is null ||
            authoringEvidence.EvidenceRefs is null)
            return false;

        var expectedMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["candidate_strategy"] =
                DesignConversionDpoTeacherAuthoringCandidateStrategy.StrategyMarker,
            ["runtime_teacher_authored_candidate"] = bool.TrueString,
            ["candidate_authoring_only"] = bool.TrueString,
            ["teacher_authoring_schema"] = authoringEvidence.Schema,
            ["teacher_authoring_output_hash"] = authoringEvidence.OutputHash,
            ["teacher_authoring_incumbent_ref"] = authoringEvidence.IncumbentRef,
            ["teacher_authoring_generated_ref"] = authoringEvidence.GeneratedRef,
            ["teacher_inference_profile_id"] = authoringEvidence.InferenceProfileId,
            ["teacher_inference_profile_configuration_sha256"] =
                authoringEvidence.InferenceProfileConfigurationSha256,
            ["raw_teacher_prompt_persisted"] = bool.FalseString,
            ["raw_teacher_output_persisted"] = bool.FalseString,
            ["raw_teacher_rationale_persisted"] = bool.FalseString,
            ["deterministic_measurement_used_as_admission_veto"] = bool.TrueString,
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = bool.FalseString,
            ["external_provider_called"] = bool.TrueString,
            ["network_call_executed"] = bool.TrueString,
            ["product_ready"] = bool.FalseString
        };
        if (authoringEvidence.Metadata.Count != expectedMetadata.Count ||
            expectedMetadata.Any(expected =>
                !authoringEvidence.Metadata.TryGetValue(expected.Key, out var actual) ||
                !string.Equals(actual, expected.Value, StringComparison.Ordinal)))
        {
            return false;
        }

        var expectedEvidenceRefs = new[]
        {
            DesignConversionDpoTeacherAuthoringCandidateStrategy.StrategyMarker,
            DesignConversionDpoTeacherAuthoringCandidateStrategy.CandidateAuthoringMarker,
            $"teacher-authoring:attempt={authoringEvidence.AttemptNumber.ToString(CultureInfo.InvariantCulture)}",
            $"teacher-authoring:incumbent_ref={authoringEvidence.IncumbentRef}",
            $"teacher-authoring:generated_ref={authoringEvidence.GeneratedRef}",
            $"teacher-authoring:output_hash={authoringEvidence.OutputHash}",
            $"teacher-inference-profile:id={authoringEvidence.InferenceProfileId}",
            $"teacher-inference-profile:configuration_sha256={authoringEvidence.InferenceProfileConfigurationSha256}"
        };
        return authoringEvidence.EvidenceRefs.SequenceEqual(
            expectedEvidenceRefs,
            StringComparer.Ordinal);
    }

    private static bool IsLowerHexSha256(string? value)
        => value is { Length: 64 } &&
           value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool HasServerOwnedSingleEvaluationBaselineAuthority(
        DesignConversionInternalEvaluationResult evaluation)
        => ValidateServerOwnedSingleEvaluationBaselineAuthority(evaluation)
            .Count == 0;

    private static IReadOnlyList<string>
        ValidateServerOwnedSingleEvaluationBaselineAuthority(
            DesignConversionInternalEvaluationResult evaluation)
    {
        List<string> reasons = [];

        if (HasSyntheticOrScaffoldMarkers(evaluation))
            reasons.Add("synthetic_or_scaffold_single_evaluation_not_allowed");
        if (!HasTrustedServerOwnedBrowserRender(evaluation))
            reasons.Add("server_owned_browser_render_not_proven");
        if (!HasTrustedServerOwnedLocalPngComparison(evaluation))
            reasons.Add("server_owned_local_png_comparison_not_proven");
        if (!HasLocalVisualLabelerBoundary(evaluation))
            reasons.Add("local_visual_labeler_boundary_not_proven");

        return reasons.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static bool HasTrustedServerOwnedBrowserRender(
        DesignConversionInternalEvaluationResult evaluation)
        => evaluation.RenderResults?.Any(result =>
            result?.Image is not null &&
            MetadataTruthy(
                result.Image.Metadata,
                DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey) &&
            MetadataTruthy(
                result.Image.Metadata,
                DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey) &&
            MetadataEquals(
                result.Image.Metadata,
                DesignConversionRenderEvidenceTrustBoundary.TrustClassificationKey,
                DesignConversionRenderEvidenceTrustBoundary.BrowserBackedClassification) &&
            MetadataEquals(
                result.Image.Metadata,
                "provider_kind",
                BrowserBackedDesignConversionRenderProvider.ProviderKind) &&
            MetadataTruthy(result.Image.Metadata, "browser_or_playwright_used")) == true;

    private static bool HasTrustedServerOwnedLocalPngComparison(
        DesignConversionInternalEvaluationResult evaluation)
        => evaluation.ComparisonArtifacts?.Any(artifact =>
            artifact is not null &&
            MetadataTruthy(
                artifact.Metadata,
                DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey) &&
            MetadataTruthy(
                artifact.Metadata,
                DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey) &&
            MetadataEquals(
                artifact.Metadata,
                DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey,
                DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedLocalPngClassification) &&
            MetadataTruthy(
                artifact.Metadata,
                "server_owned_local_png_comparison_trust_verified") &&
            MetadataTruthy(artifact.Metadata, "visual_comparison_proven") &&
            EvidenceContains(
                artifact.EvidenceRefs,
                ServerOwnedLocalPngComparisonProvider.ProviderKind) &&
            EvidenceContains(
                artifact.EvidenceRefs,
                ServerOwnedLocalPngComparisonProvider.Boundary) &&
            EvidenceContains(
                artifact.Verdict.EvidenceRefs,
                ServerOwnedLocalPngComparisonProvider.ProviderKind) &&
            EvidenceContains(
                artifact.Verdict.EvidenceRefs,
                ServerOwnedLocalPngComparisonProvider.Boundary)) == true;

    private static bool HasLocalVisualLabelerBoundary(
        DesignConversionInternalEvaluationResult evaluation)
        => string.Equals(
               evaluation.VisualQaBoundaryTrace?.VisualJudgeProviderBoundary,
               DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
               StringComparison.Ordinal) ||
           evaluation.VisualJudgeResults?.Any(result =>
               result is not null &&
               (MetadataTruthy(
                    result.Provider.Metadata,
                    DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker) ||
                MetadataTruthy(
                    result.Metadata,
                    DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker) ||
                EvidenceContains(
                    result.EvidenceRefs,
                    DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker) ||
                EvidenceContains(
                    result.Verdict.EvidenceRefs,
                    DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker))) == true;

    private static bool TryBuildServerOwnedSingleEvaluationBaselineEvidence(
        DesignConversionInternalEvaluationResult evaluation,
        out RuntimeRejectedCandidateEvidence evidence,
        out string reason)
    {
        evidence = RuntimeRejectedCandidateEvidence.Empty;
        reason = RuntimeRejectedCandidateEvidenceRequiredReason;

        var chosenOutput = evaluation.RuntimeFlowResult?.Output;
        var request = evaluation.SubmittedRequest;
        if (chosenOutput is null || request is null ||
            !HasRequiredRuntimeIdentity(chosenOutput) ||
            string.IsNullOrWhiteSpace(request.RequestId) ||
            string.IsNullOrWhiteSpace(request.Html) ||
            string.IsNullOrWhiteSpace(request.Css))
        {
            reason = "runtime_server_owned_baseline_identity_required";
            return false;
        }

        if (!TryGetMeasuredVisualScore(chosenOutput.Verdict.Score, out var chosenScore))
        {
            reason = "runtime_server_owned_baseline_chosen_visual_score_required";
            return false;
        }

        var defects = BuildRuntimeRejectedDefects(evaluation, chosenOutput);
        if (defects.Count == 0 || !HasRuntimeNegativeQualityEvidence(evaluation))
        {
            reason = "runtime_server_owned_baseline_negative_quality_evidence_not_proven";
            return false;
        }

        var negativeReasonTaxonomy =
            ResolveRuntimeRejectedNegativeReasonTaxonomy(
                defects,
                allowServerOwnedLocalPngPixelDifference: true);
        if (string.IsNullOrWhiteSpace(negativeReasonTaxonomy))
        {
            reason = "runtime_server_owned_baseline_negative_reason_taxonomy_required";
            return false;
        }

        var evidenceRefs = BuildRuntimeRejectedEvidenceRefs(evaluation, chosenOutput)
            .Append("runtime_created_dpo_candidate:server_owned_single_evaluation_baseline")
            .Append(DesignConversionRenderEvidenceTrustBoundary.BrowserBackedClassification)
            .Append(DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedLocalPngClassification)
            .Append(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker)
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (evidenceRefs.Length == 0)
        {
            reason = "runtime_server_owned_baseline_evidence_refs_required";
            return false;
        }

        var rejectedScore = Math.Max(0.0, Math.Round(chosenScore - 0.01, 6));
        evidence = new RuntimeRejectedCandidateEvidence(
            OutputId: Sha256Ref(string.Join("|",
            [
                "server-owned-baseline-output",
                request.RequestId,
                chosenOutput.OutputId
            ])),
            CandidateId: Sha256Ref(string.Join("|",
            [
                "server-owned-baseline-candidate",
                request.RequestId,
                chosenOutput.CandidateId
            ])),
            HtmlRef: DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-server-owned-baseline-rejected-html",
                request.Html),
            CssRef: DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-server-owned-baseline-rejected-css",
                request.Css),
            VerdictId: Sha256Ref(string.Join("|",
            [
                "server-owned-baseline-verdict",
                request.RequestId,
                chosenOutput.Verdict.VerdictId
            ])),
            VisualScore: rejectedScore,
            NegativeQualityReason:
                BuildServerOwnedSingleEvaluationBaselineReviewReason(
                    rejectedScore,
                    negativeReasonTaxonomy),
            NegativeReasonTaxonomy: negativeReasonTaxonomy,
            Defects: defects,
            EvidenceSource:
                "server_owned_single_evaluation_baseline",
            EvidenceRefs: evidenceRefs,
            QaResults: BuildRuntimeQaResults(evaluation));
        reason = string.Empty;
        return true;
    }

    private static bool TryBuildRuntimeRejectedCandidateEvidence(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionInternalEvaluationResult? rejectedEvaluation,
        out RuntimeRejectedCandidateEvidence evidence,
        out string reason)
    {
        evidence = RuntimeRejectedCandidateEvidence.Empty;
        reason = RuntimeRejectedCandidateEvidenceRequiredReason;

        if (rejectedEvaluation is null)
            return false;

        var chosenOutput = evaluation.RuntimeFlowResult?.Output;
        var rejectedOutput = rejectedEvaluation.RuntimeFlowResult?.Output;
        if (chosenOutput is null || rejectedOutput is null)
        {
            reason = "runtime_rejected_candidate_identity_required";
            return false;
        }

        if (IsSameRuntimeIdentity(chosenOutput, rejectedOutput))
        {
            reason = "runtime_rejected_candidate_identity_must_be_distinct";
            return false;
        }

        if (!HasRequiredRuntimeIdentity(chosenOutput) || !HasRequiredRuntimeIdentity(rejectedOutput))
        {
            reason = "runtime_rejected_candidate_identity_required";
            return false;
        }

        if (HasSyntheticOrScaffoldMarkers(rejectedEvaluation))
        {
            reason = "runtime_rejected_candidate_synthetic_or_scaffold";
            return false;
        }

        if (!TryGetMeasuredVisualScore(chosenOutput.Verdict.Score, out var chosenScore))
        {
            reason = "runtime_chosen_visual_score_required";
            return false;
        }

        if (!TryGetMeasuredVisualScore(rejectedOutput.Verdict.Score, out var rejectedScore))
        {
            reason = "runtime_rejected_visual_score_required";
            return false;
        }

        if (rejectedScore >= chosenScore)
        {
            reason = "runtime_rejected_visual_score_not_lower_than_chosen";
            return false;
        }

        var realEvidenceRefs = BuildRuntimeRejectedEvidenceRefs(rejectedEvaluation, rejectedOutput);
        if (realEvidenceRefs.Count == 0)
        {
            reason = "runtime_rejected_candidate_evidence_refs_required";
            return false;
        }

        var defects = BuildRuntimeRejectedDefects(rejectedEvaluation, rejectedOutput);
        if (defects.Count == 0 || !HasRuntimeNegativeQualityEvidence(rejectedEvaluation))
        {
            reason = "runtime_rejected_negative_quality_evidence_not_proven";
            return false;
        }

        var negativeReasonTaxonomy = ResolveRuntimeRejectedNegativeReasonTaxonomy(defects);
        if (string.IsNullOrWhiteSpace(negativeReasonTaxonomy))
        {
            reason = "runtime_rejected_negative_reason_taxonomy_required";
            return false;
        }

        var htmlRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-runtime-rejected-html",
            $"{rejectedEvaluation.ImprovedHtml}|{rejectedOutput.OutputId}");
        var cssRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-runtime-rejected-css",
            $"{rejectedEvaluation.ImprovedCss}|{rejectedOutput.OutputId}");

        evidence = new RuntimeRejectedCandidateEvidence(
            OutputId: rejectedOutput.OutputId,
            CandidateId: rejectedOutput.CandidateId,
            HtmlRef: htmlRef,
            CssRef: cssRef,
            VerdictId: rejectedOutput.Verdict.VerdictId,
            VisualScore: Math.Round(rejectedScore, 6),
            NegativeQualityReason: BuildRuntimeRejectedNegativeQualityReason(rejectedScore, negativeReasonTaxonomy),
            NegativeReasonTaxonomy: negativeReasonTaxonomy,
            Defects: defects,
            EvidenceSource: "typed_prior_internal_evaluation_result",
            EvidenceRefs: realEvidenceRefs,
            QaResults: BuildRuntimeQaResults(rejectedEvaluation));
        reason = string.Empty;
        return true;
    }

    private static IReadOnlyList<RenderQaResultRecord> BuildRuntimeQaResults(
        DesignConversionInternalEvaluationResult evaluation)
    {
        if (evaluation.QaResults is { Count: > 0 })
            return evaluation.QaResults;

        var fallbackWarnings = BuildRuntimeQaFallbackWarnings(evaluation.Warnings);
        return (evaluation.ComparisonArtifacts ?? [])
            .Select((artifact, index) => new RenderQaResultRecord(
                ResultId: string.IsNullOrWhiteSpace(artifact.ArtifactId)
                    ? $"{evaluation.JobId}:comparison-qa:{(index + 1).ToString(CultureInfo.InvariantCulture)}"
                    : $"{artifact.ArtifactId}:qa",
                RequestId: evaluation.SubmittedRequest.RequestId,
                Expected: artifact.Expected,
                Actual: artifact.Actual,
                Verdict: artifact.Verdict,
                Metrics: artifact.Metrics ?? new Dictionary<string, double>(),
                Warnings: fallbackWarnings))
            .ToArray();
    }

    private static IReadOnlyList<string> BuildRuntimeQaFallbackWarnings(
        IReadOnlyList<string>? warnings)
        => (warnings ?? [])
            .Select(warning => warning?.Trim() ?? string.Empty)
            .Where(warning => warning.Length > 0)
            .Where(IsRuntimeQaBoundaryWarning)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static readonly string[] RuntimeQaBoundaryWarningAllowlist =
    [
        "DETERMINISTIC_RENDER_SCAFFOLD_DOES_NOT_CLAIM_VISUAL_FIDELITY",
        "DETERMINISTIC_RENDER_SCAFFOLD_NOT_BROWSER_SCREENSHOT",
        "SERVER_OWNED_SCAFFOLD_RENDER_PROVENANCE_CANONICALIZED",
        "SERVER_OWNED_BROWSER_RENDER_PROVENANCE_CANONICALIZED",
        "BROWSER_RENDER_PROOF_RECEIPT_REQUIRED"
    ];

    private static bool IsRuntimeQaBoundaryWarning(string warning)
        => RuntimeQaBoundaryWarningAllowlist.Contains(warning, StringComparer.Ordinal);

    private static bool HasRequiredRuntimeIdentity(DesignConversionImprovementOutput output)
        => !string.IsNullOrWhiteSpace(output.OutputId) &&
           !string.IsNullOrWhiteSpace(output.CandidateId) &&
           !string.IsNullOrWhiteSpace(output.Verdict.VerdictId);

    private static bool IsSameRuntimeIdentity(
        DesignConversionImprovementOutput chosen,
        DesignConversionImprovementOutput rejected)
        => string.Equals(chosen.OutputId, rejected.OutputId, StringComparison.Ordinal) ||
           string.Equals(chosen.CandidateId, rejected.CandidateId, StringComparison.Ordinal) ||
           string.Equals(chosen.Verdict.VerdictId, rejected.Verdict.VerdictId, StringComparison.Ordinal);

    private static bool TryGetMeasuredVisualScore(double rawScore, out double score)
    {
        score = 0.0;
        if (!double.IsFinite(rawScore) || rawScore is < 0 or > 1)
            return false;

        score = rawScore;
        return true;
    }

    private static bool HasSyntheticOrScaffoldMarkers(DesignConversionInternalEvaluationResult evaluation)
        => !HasOnlyOperationalScaffoldMarkers(evaluation.ScaffoldMarkers) ||
           ContainsSyntheticOrScaffoldMarker(evaluation.Warnings) ||
           ContainsSyntheticOrScaffoldMarker(evaluation.EvidenceRefs);

    private static bool HasOnlyOperationalScaffoldMarkers(
        IReadOnlyDictionary<string, string>? scaffoldMarkers)
        => scaffoldMarkers is null || scaffoldMarkers.All(IsOperationalScaffoldMarker);

    private static bool IsOperationalScaffoldMarker(KeyValuePair<string, string> marker)
    {
        var isKnownOperationalProviderKind =
            string.Equals(
                marker.Key,
                "browser_renderer_injected_provider_kind",
                StringComparison.Ordinal) &&
            IsOperationalRenderProviderKind(marker.Value);
        if (!OperationalScaffoldMarkerKeys.Contains(marker.Key) ||
            ContainsSyntheticOrScaffoldMarkerTerm(marker.Key) ||
            (!isKnownOperationalProviderKind &&
             ContainsSyntheticOrScaffoldMarkerTerm(marker.Value)))
        {
            return false;
        }

        if (BooleanOperationalScaffoldMarkerKeys.Contains(marker.Key))
        {
            return string.Equals(marker.Value, bool.TrueString, StringComparison.Ordinal) ||
                   string.Equals(marker.Value, bool.FalseString, StringComparison.Ordinal);
        }

        return marker.Key switch
        {
            "browser_renderer_default_scope" =>
                string.Equals(
                    marker.Value,
                    "explicit_local_injection_only_not_production_default",
                    StringComparison.Ordinal) ||
                string.Equals(
                    marker.Value,
                    "not_browser_backed_for_current_injection",
                    StringComparison.Ordinal),
            "browser_renderer_injected_provider_kind" =>
                IsOperationalRenderProviderKind(marker.Value),
            "active_model_runtime_selection" =>
                string.Equals(marker.Value, "durable_registry", StringComparison.Ordinal),
            "active_model_runtime_selection_boundary" =>
                string.Equals(
                    marker.Value,
                    "design_conversion_modelserver_runtime_selection_reads_durable_active_model_and_fresh_loads_checkpoint",
                    StringComparison.Ordinal),
            "active_model_runtime_tenant_context_source" =>
                string.Equals(marker.Value, "server_owned_route_admission", StringComparison.Ordinal),
            "active_model_generation" =>
                long.TryParse(
                    marker.Value,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var generation) &&
                generation >= 0,
            "active_model_tenant_id_ref" or "active_model_admitted_tenant_id_ref" =>
                DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionTenantIdRef(marker.Value),
            "active_model_admitted_scope_ref" =>
                DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionScopeRef(marker.Value),
            "active_model_checkpoint_sha256" =>
                IsLowerHexSha256(marker.Value),
            "active_model_checkpoint_path_ref" =>
                IsScopedSha256Ref(
                    marker.Value,
                    "design-conversion-model-activation-checkpoint-path"),
            "active_model_checkpoint_weights_source" =>
                string.Equals(marker.Value, "local_checkpoint_feature_weights", StringComparison.Ordinal) ||
                string.Equals(marker.Value, "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted", StringComparison.Ordinal) ||
                string.Equals(marker.Value, "none", StringComparison.Ordinal),
            "active_model_id" or "active_model_manifest_id" or "active_model_version" =>
                IsSafeNonEmptyOperationalScalar(marker.Value),
            _ => false
        };
    }

    private static bool IsOperationalRenderProviderKind(string? value)
        => OperationalRenderProviderKinds.Contains(value ?? string.Empty);

    private static bool IsSafeNonEmptyOperationalScalar(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           string.Equals(value, value.Trim(), StringComparison.Ordinal) &&
           !ContainsSyntheticOrScaffoldMarkerTerm(value);

    private static bool IsScopedSha256Ref(string? value, string scope)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        var prefix = $"sha256:{scope}:";
        if (!value.StartsWith(prefix, StringComparison.Ordinal) ||
            value.Length != prefix.Length + 64)
        {
            return false;
        }

        for (var index = prefix.Length; index < value.Length; index++)
        {
            var character = value[index];
            if (character is not (>= '0' and <= '9') &&
                character is not (>= 'a' and <= 'f'))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsSyntheticOrScaffoldMarker(IReadOnlyList<string>? values)
        => values?.Any(value =>
            string.Equals(value, "synthetic_negative", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "scaffold_negative", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "synthetic_runtime_not_train_ready", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("synthetic_negative=", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("scaffold_negative=", StringComparison.OrdinalIgnoreCase)) == true;

    private static bool ContainsSyntheticOrScaffoldMarkerTerm(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           (value.Contains("synthetic", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("scaffold", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("fake", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("placeholder", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("stand_in", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("stand-in", StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<string> BuildRuntimeRejectedEvidenceRefs(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionImprovementOutput output)
        => output.EvidenceRefs
            .Concat(output.Verdict.EvidenceRefs)
            .Concat(evaluation.EvidenceRefs)
            .Concat(evaluation.RenderResults.Select(result => result.RenderResultId))
            .Concat(evaluation.ComparisonArtifacts.SelectMany(artifact => artifact.EvidenceRefs))
            .Concat(evaluation.ComparisonArtifacts.SelectMany(artifact => artifact.Verdict.EvidenceRefs))
            .Concat(evaluation.VisualJudgeResults.SelectMany(result => result.EvidenceRefs))
            .Concat(evaluation.VisualJudgeResults.SelectMany(result => result.Verdict.EvidenceRefs))
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<VisualDefect> BuildRuntimeRejectedDefects(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionImprovementOutput output)
        => output.Verdict.Defects
            .Concat(evaluation.ComparisonArtifacts.SelectMany(artifact => artifact.Verdict.Defects))
            .Concat(evaluation.VisualJudgeResults.SelectMany(result => result.Verdict.Defects))
            .GroupBy(
                defect => $"{defect.Category}|{defect.Severity}|{defect.Description}",
                StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();

    private static string ResolveRuntimeRejectedNegativeReasonTaxonomy(
        IReadOnlyList<VisualDefect> defects,
        bool allowServerOwnedLocalPngPixelDifference = false)
    {
        var canonical = defects
            .Select(defect => defect.Category)
            .Where(category =>
                DesignConversionNegativeReasonTaxonomy.IsAllowed(category) &&
                !DesignConversionNegativeReasonTaxonomy.IsQuarantineOnly(category))
            .Order(StringComparer.Ordinal)
            .FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(canonical))
            return canonical;

        return allowServerOwnedLocalPngPixelDifference &&
               defects.Any(defect =>
                   string.Equals(
                       defect.Category,
                       "pixel-difference",
                       StringComparison.Ordinal))
            ? DesignConversionNegativeReasonTaxonomy.PlatformNormalizationMismatch
            : string.Empty;
    }

    private static string BuildRuntimeRejectedNegativeQualityReason(double score, string taxonomy)
        => $"Rejected is a realistic near-miss from a typed prior evaluation with measured visual score {Math.Round(score, 6):0.######} and evidence-backed {taxonomy} defects.";

    private static string BuildServerOwnedSingleEvaluationBaselineReviewReason(
        double score,
        string taxonomy)
        => $"Server-owned single evaluation has evidence-backed {taxonomy} visual deltas with review baseline score {Math.Round(score, 6):0.######}; the baseline is stored only for review and is not an independently evaluated rejected candidate.";

    private sealed record RuntimeRejectedCandidateEvidence(
        string OutputId,
        string CandidateId,
        string HtmlRef,
        string CssRef,
        string VerdictId,
        double VisualScore,
        string NegativeQualityReason,
        string NegativeReasonTaxonomy,
        IReadOnlyList<VisualDefect> Defects,
        string EvidenceSource,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<RenderQaResultRecord> QaResults)
    {
        public static RuntimeRejectedCandidateEvidence Empty { get; } = new(
            OutputId: string.Empty,
            CandidateId: string.Empty,
            HtmlRef: string.Empty,
            CssRef: string.Empty,
            VerdictId: string.Empty,
            VisualScore: 0,
            NegativeQualityReason: string.Empty,
            NegativeReasonTaxonomy: string.Empty,
            Defects: [],
            EvidenceSource: string.Empty,
            EvidenceRefs: [],
            QaResults: []);
    }

    private DesignConversionDpoTrainingRecord BuildRecord(
        DesignConversionStoredDpoExample example,
        int ordinal)
    {
        var reasons = Validate(example).ToList();
        var pairAuthorsValid = TryResolveStoredExamplePairAuthorKinds(
            example,
            out var chosenAuthorKind,
            out var rejectedAuthorKind);
        if (!pairAuthorsValid)
        {
            reasons.Add(
                DesignConversionDpoPolicyCheckpointBridge
                    .PairAuthorshipMissingOrInvalidErrorCode);
        }

        var rawRecordId = string.IsNullOrWhiteSpace(example.ExampleId)
            ? $"design-conversion-dpo-{ordinal:0000}"
            : example.ExampleId;
        var unitId = DesignConversionDpoUnits.TryNormalize(example.UnitId, out var normalizedUnitId)
            ? normalizedUnitId
            : example.UnitId;
        var encoding = DesignConversionDpoUnits.TryNormalize(example.UnitId, out var targetUnitId)
            ? _sourceEncoder.Encode(example, targetUnitId)
            : OperationResult<DesignConversionSourceEncoding>.Fail(
                "DesignConversion source encoding requires a supported target unit.",
                "DESIGN_CONVERSION_SOURCE_ENCODING_UNIT_REQUIRED");
        if (!encoding.IsSuccess)
            reasons.Add(encoding.ErrorCode ?? "source_encoder_failed");

        var leakageRedactions = new SortedSet<string>(encoding.Value?.RedactionCodes ?? [], StringComparer.Ordinal);
        var recordId = DesignConversionDpoLeakageGuard.SanitizeScalar(rawRecordId, "record_id", leakageRedactions);
        var chosen = SanitizeOutput(example.Chosen, "chosen", leakageRedactions);
        var rejected = SanitizeOutput(example.Rejected, "rejected", leakageRedactions);

        var record = new DesignConversionDpoTrainingRecord(
            RecordId: recordId,
            UnitId: unitId,
            InternalUnitStatePayload: encoding.Value?.InternalUnitStatePayload ?? string.Empty,
            InternalVectorPayload: encoding.Value?.InternalVectorPayload ?? [],
            SourceStructurePayload: encoding.Value?.SourceStructurePayload ?? string.Empty,
            Chosen: chosen,
            Rejected: rejected,
            TieBreakerPayload: DesignConversionDpoLeakageGuard.SanitizeJsonPayload(
                example.TieBreakerPayload,
                "tie_breaker",
                leakageRedactions),
            UsefulnessScore: example.UsefulnessScore,
            NegativeQualityReason: example.NegativeQualityReason,
            NegativeReasonTaxonomy: example.NegativeReasonTaxonomy,
            Provenance: MergeProvenance(example, encoding.Value, leakageRedactions),
            License: example.License,
            Quarantine: new DesignConversionQuarantineState(
                IsQuarantined: reasons.Count > 0,
                Reason: reasons.Count == 0 ? "train_ready" : string.Join(";", reasons),
                EvidenceRefs: BuildQuarantineEvidence(example, reasons, leakageRedactions)),
            TeacherProvenance: example.TeacherProvenance)
        {
            // Снимок записи получает уже разрешенные значения author-kind:
            // bridge ниже не должен восстанавливать направление из score.
            ChosenAuthorKind = chosenAuthorKind,
            RejectedAuthorKind = rejectedAuthorKind
        };
        if (record.TeacherProvenance is null)
            return record;
        var trainingContent =
            DesignConversionDpoTrainingContentAuthority.Project([record]);
        return trainingContent.IsSuccess && trainingContent.Value is not null
            ? record with
            {
                TeacherProvenance = record.TeacherProvenance with
                {
                    TrainingContentSha256 =
                        trainingContent.Value.Rows[0].Sha256
                }
            }
            : record;
    }

    private static bool TryResolveStoredExamplePairAuthorKinds(
        DesignConversionStoredDpoExample? example,
        out DpoPairSideAuthorKind? chosenAuthorKind,
        out DpoPairSideAuthorKind? rejectedAuthorKind)
    {
        chosenAuthorKind = null;
        rejectedAuthorKind = null;
        if (example is null ||
            !IsValidOptionalPairAuthorKind(example.ChosenAuthorKind) ||
            !IsValidOptionalPairAuthorKind(example.RejectedAuthorKind))
        {
            return false;
        }

        if (example.ChosenAuthorKind.HasValue ||
            example.RejectedAuthorKind.HasValue)
        {
            if (!example.ChosenAuthorKind.HasValue ||
                !example.RejectedAuthorKind.HasValue)
            {
                return false;
            }

            chosenAuthorKind = example.ChosenAuthorKind.Value;
            rejectedAuthorKind = example.RejectedAuthorKind.Value;
            return true;
        }

        if (!IsExactDeterministicNoExternalTeacher(example.TeacherProvenance))
            return false;

        chosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator;
        rejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator;
        return true;
    }

    private static bool IsValidOptionalPairAuthorKind(
        DpoPairSideAuthorKind? authorKind)
        => authorKind is null ||
           Enum.IsDefined(typeof(DpoPairSideAuthorKind), authorKind.Value);

    private static bool IsExactDeterministicNoExternalTeacher(
        TrainingTeacherProvenance? provenance)
        => provenance is
           {
               SourceKind: TrainingTeacherSourceKind.DeterministicNoExternalTeacher,
               Family: null,
               AccessMode: null,
               HostingProvider: null,
               ModelLicense: null,
               ProviderTermsChecked: false,
               ProviderTermsAllowTraining: false,
               ClosedProprietaryModel: false,
               TeacherOutputUsedForTraining: false,
               InvocationCount: 0
           } &&
           string.IsNullOrEmpty(provenance.ModelId) &&
           string.IsNullOrEmpty(provenance.ModelVersion) &&
           string.IsNullOrEmpty(provenance.ModelArtifactDigestKind) &&
           string.IsNullOrEmpty(provenance.ModelArtifactDigest);

    private static IReadOnlyList<string> Validate(DesignConversionStoredDpoExample example)
    {
        var reasons = new List<string>();

        if (!DesignConversionDpoUnits.TryNormalize(example.UnitId, out _))
            reasons.Add("unsupported_unit_id");

        if (string.IsNullOrWhiteSpace(example.InternalUnitStatePayload))
            reasons.Add("internal_unit_state_required");

        if (string.IsNullOrWhiteSpace(example.SourceStructurePayload))
            reasons.Add("source_structure_required");

        if (ContainsHiddenLearningLeakage(example.InternalUnitStatePayload)
            || ContainsHiddenLearningLeakage(example.SourceStructurePayload)
            || ContainsHiddenLearningLeakage(example.TieBreakerPayload ?? string.Empty)
            || ContainsHiddenLearningLeakage(example.NegativeQualityReason ?? string.Empty)
            || ContainsHiddenLearningLeakage(example.NegativeReasonTaxonomy ?? string.Empty))
        {
            reasons.Add("hidden_deterministic_or_qa_leakage_field");
        }

        if (example.License is null)
        {
            reasons.Add("license_required");
        }
        else
        {
            if (!example.License.AllowsTraining)
                reasons.Add("license_blocks_training");

            if (example.License.ContainsUserPrivateData)
                reasons.Add("private_data_requires_quarantine");
        }

        if (example.TeacherProvenance is null)
            reasons.Add("teacher_provenance_required");

        ValidateOutput(example.Chosen, "chosen", reasons);
        ValidateOutput(example.Rejected, "rejected", reasons);

        var negativeQuality = DesignConversionNegativeQualityValidator.Validate(example);
        reasons.AddRange(negativeQuality.Reasons);

        if (HasSyntheticNegativeMarker(example))
            reasons.Add("synthetic_negative_not_train_ready");

        if (HasTeacherCandidateOnlyMarker(example))
            reasons.Add("teacher_candidate_only_not_train_ready");

        if (example.Chosen is not null && example.Rejected is not null)
        {
            if (IsSameOutput(example.Chosen, example.Rejected))
                reasons.Add("chosen_rejected_same");

            if (DesignConversionDpoUnits.TryNormalize(example.UnitId, out var normalizedUnitId)
                && string.Equals(normalizedUnitId, DesignConversionDpoUnits.ImproverUnit, StringComparison.Ordinal)
                && example.Chosen.Verdict.Score <= example.Rejected.Verdict.Score)
            {
                reasons.Add("improver_rejected_quality_not_lower_than_chosen");
            }
        }

        if (example.UsefulnessScore <= 0 || example.UsefulnessScore > 2)
            reasons.Add("usefulness_score_out_of_range");

        return reasons;
    }

    private static bool HasSyntheticNegativeMarker(DesignConversionStoredDpoExample example)
        => string.Equals(example.NegativeReasonTaxonomy, DesignConversionNegativeReasonTaxonomy.SyntheticRuntimeNotTrainReady, StringComparison.Ordinal)
           || ContainsSyntheticNegativeMarker(example.NegativeQualityReason)
           || MetadataTruthy(example.Metadata, "synthetic_negative")
           || MetadataTruthy(example.Provenance?.Metadata, "synthetic_negative")
           || MetadataTruthy(example.License?.Metadata, "synthetic_negative");

    private static bool HasTeacherCandidateOnlyMarker(
        DesignConversionStoredDpoExample example)
        => MetadataTruthy(example.Metadata, "chosen_rejected_pair_not_proven")
           || MetadataTruthy(example.Provenance?.Metadata, "chosen_rejected_pair_not_proven")
           || MetadataTruthy(example.License?.Metadata, "chosen_rejected_pair_not_proven")
           || MetadataTruthy(example.Metadata, "independent_preference_not_proven")
           || MetadataTruthy(example.Provenance?.Metadata, "independent_preference_not_proven")
           || MetadataTruthy(example.License?.Metadata, "independent_preference_not_proven")
           || MetadataEquals(
               example.Metadata,
               "teacher_candidate_only_boundary",
               "server_owned_route_admission_bound_pending_review")
           || MetadataEquals(
               example.Provenance?.Metadata,
               "teacher_candidate_only_boundary",
               "server_owned_route_admission_bound_pending_review")
           || MetadataEquals(
               example.License?.Metadata,
               "teacher_candidate_only_boundary",
               "server_owned_route_admission_bound_pending_review");

    private static bool ContainsSyntheticNegativeMarker(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Contains("synthetic_negative", StringComparison.OrdinalIgnoreCase);

    private static bool MetadataTruthy(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           bool.TryParse(value, out var parsed) &&
           parsed;

    private static bool MetadataEquals(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        string expected)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           string.Equals(value, expected, StringComparison.Ordinal);

    private static bool EvidenceContains(
        IReadOnlyList<string>? evidenceRefs,
        string expected)
        => evidenceRefs?.Any(reference =>
            string.Equals(reference, expected, StringComparison.Ordinal) ||
            (!string.IsNullOrWhiteSpace(reference) &&
             reference.Contains(expected, StringComparison.Ordinal))) == true;

    private static DesignConversionDpoProvenance MergeProvenance(
        DesignConversionStoredDpoExample example,
        DesignConversionSourceEncoding? encoding,
        SortedSet<string> redactions)
    {
        var provenance = example.Provenance ?? new DesignConversionDpoProvenance(
            SourceRequestId: "",
            SourceKind: "",
            SourceManifestRef: "",
            ReviewerRef: "",
            CreatedAtUtc: DateTimeOffset.UtcNow,
            Metadata: new Dictionary<string, string>());

        var metadata = new Dictionary<string, string>(
            DesignConversionDpoLeakageGuard.SanitizeMetadata(provenance.Metadata, "provenance_metadata", redactions),
            StringComparer.Ordinal)
        {
            ["dpo_example_id"] = DesignConversionDpoLeakageGuard.SanitizeScalar(
                string.IsNullOrWhiteSpace(example.ExampleId) ? "generated" : example.ExampleId,
                "dpo_example_id",
                redactions),
            ["deterministic_logic_claimed_as_learning"] = bool.FalseString,
            ["qa_teacher_evidence_role"] = "evidence_only",
            ["negative_quality_reason_role"] = "evidence_explanation_only",
            ["negative_reason_taxonomy"] = string.IsNullOrWhiteSpace(example.NegativeReasonTaxonomy)
                ? "unknown"
                : example.NegativeReasonTaxonomy,
            ["usefulness_score_role"] = "review_priority_only"
        };

        if (encoding is not null)
        {
            foreach (var pair in encoding.Metadata)
                metadata[pair.Key] = pair.Value;

            metadata["target_unit_id"] = encoding.TargetUnitId;
            metadata["machine_part"] = encoding.MachinePart;
            metadata["result_feature_payload_hash"] = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-result-features",
                encoding.ResultFeaturePayload);
        }

        foreach (var pair in example.Metadata ?? new Dictionary<string, string>())
        {
            if (!metadata.ContainsKey(pair.Key))
            {
                var keyCode = pair.Key.Contains("prompt", StringComparison.OrdinalIgnoreCase)
                    ? "example_metadata:prompt_text"
                    : null;
                if (keyCode is not null)
                {
                    redactions.Add(keyCode);
                    continue;
                }

                metadata[pair.Key] = DesignConversionDpoLeakageGuard.SanitizeScalar(
                    pair.Value,
                    $"example_metadata.{pair.Key}",
                    redactions);
            }
        }

        if (redactions.Count > 0)
            metadata["source_encoder_redaction_codes"] = string.Join(",", redactions);

        return provenance with
        {
            SourceRequestId = DesignConversionDpoLeakageGuard.SanitizeScalar(
                provenance.SourceRequestId,
                "source_request_ref",
                redactions),
            SourceManifestRef = DesignConversionDpoLeakageGuard.SanitizeScalar(
                provenance.SourceManifestRef,
                "source_manifest_ref",
                redactions),
            ReviewerRef = DesignConversionDpoLeakageGuard.SanitizeScalar(
                provenance.ReviewerRef,
                "reviewer_ref",
                redactions),
            Metadata = metadata
        };
    }

    private static IReadOnlyList<string> BuildQuarantineEvidence(
        DesignConversionStoredDpoExample example,
        IReadOnlyList<string> reasons,
        SortedSet<string> redactions)
    {
        var refs = (example.TeacherEvidenceRefs ?? [])
            .Concat(example.Chosen?.EvidenceRefs ?? [])
            .Concat(example.Rejected?.EvidenceRefs ?? [])
            .Select(reference => DesignConversionDpoLeakageGuard.HashRef("dcm-dpo-evidence", reference))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (reasons.Count == 0)
            refs.Add("design-conversion-dpo-validation:train-ready");
        else
            refs.AddRange(reasons.Select(reason => $"design-conversion-dpo-validation:{reason}"));

        return refs.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static void ValidateOutput(
        DesignConversionImprovementOutput? output,
        string label,
        List<string> reasons)
    {
        if (output is null)
        {
            reasons.Add($"{label}_output_required");
            return;
        }

        if (string.IsNullOrWhiteSpace(output.OutputId))
            reasons.Add($"{label}_output_id_required");

        if (string.IsNullOrWhiteSpace(output.CandidateId))
            reasons.Add($"{label}_candidate_id_required");

        if (string.IsNullOrWhiteSpace(output.Html) && string.IsNullOrWhiteSpace(output.Css))
            reasons.Add($"{label}_markup_required");

        if (string.IsNullOrWhiteSpace(output.Verdict.VerdictId))
            reasons.Add($"{label}_verdict_required");

        if (output.Verdict.Score is < 0 or > 1)
            reasons.Add($"{label}_verdict_score_out_of_range");
    }

    private static bool IsSameOutput(
        DesignConversionImprovementOutput chosen,
        DesignConversionImprovementOutput rejected)
        => string.Equals(Normalize(chosen.Html), Normalize(rejected.Html), StringComparison.Ordinal)
            && string.Equals(Normalize(chosen.Css), Normalize(rejected.Css), StringComparison.Ordinal);

    private static DesignConversionImprovementOutput SanitizeOutput(
        DesignConversionImprovementOutput output,
        string label,
        SortedSet<string> redactions)
        => output with
        {
            OutputId = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-output", output.OutputId),
            CandidateId = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-candidate", output.CandidateId),
            Html = DesignConversionDpoLeakageGuard.SanitizeScalar(output.Html, $"{label}_html", redactions),
            Css = DesignConversionDpoLeakageGuard.SanitizeScalar(output.Css, $"{label}_css", redactions),
            Verdict = SanitizeVerdict(output.Verdict, label, redactions),
            QaResults = output.QaResults.Select(result => SanitizeQaResult(result, label, redactions)).ToArray(),
            EvidenceRefs = output.EvidenceRefs
                .Select(reference => DesignConversionDpoLeakageGuard.HashRef($"{label}-evidence", reference))
                .ToArray()
        };

    private static VisualVerdict SanitizeVerdict(
        VisualVerdict verdict,
        string label,
        SortedSet<string> redactions)
        => verdict with
        {
            VerdictId = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-verdict", verdict.VerdictId),
            Defects = verdict.Defects.Select(defect => SanitizeDefect(defect, label, redactions)).ToArray(),
            EvidenceRefs = verdict.EvidenceRefs
                .Select(reference => DesignConversionDpoLeakageGuard.HashRef($"{label}-verdict-evidence", reference))
                .ToArray()
        };

    private static VisualDefect SanitizeDefect(
        VisualDefect defect,
        string label,
        SortedSet<string> redactions)
        => defect with
        {
            DefectId = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-defect", defect.DefectId),
            Category = DesignConversionDpoLeakageGuard.SanitizeScalar(defect.Category, $"{label}_defect_category", redactions),
            Severity = DesignConversionDpoLeakageGuard.SanitizeScalar(defect.Severity, $"{label}_defect_severity", redactions),
            Description = DesignConversionDpoLeakageGuard.SanitizeScalar(defect.Description, $"{label}_defect_description", redactions),
            EvidenceImageIds = defect.EvidenceImageIds
                .Select(reference => DesignConversionDpoLeakageGuard.HashRef($"{label}-defect-image", reference))
                .ToArray()
        };

    private static RenderQaResultRecord SanitizeQaResult(
        RenderQaResultRecord result,
        string label,
        SortedSet<string> redactions)
        => result with
        {
            ResultId = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-qa-result", result.ResultId),
            RequestId = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-qa-request", result.RequestId),
            Expected = SanitizeImage(result.Expected, $"{label}_expected", redactions),
            Actual = SanitizeImage(result.Actual, $"{label}_actual", redactions),
            Verdict = SanitizeVerdict(result.Verdict, $"{label}_qa", redactions),
            Warnings = result.Warnings
                .Select(warning => DesignConversionDpoLeakageGuard.SanitizeScalar(warning, $"{label}_qa_warning", redactions))
                .ToArray()
        };

    private static RenderQaImageRecord SanitizeImage(
        RenderQaImageRecord image,
        string label,
        SortedSet<string> redactions)
        => image with
        {
            ImageId = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-image", image.ImageId),
            Uri = DesignConversionDpoLeakageGuard.HashRef($"dcm-{label}-uri", image.Uri),
            Metadata = DesignConversionDpoLeakageGuard.SanitizeMetadata(image.Metadata, $"{label}_metadata", redactions)
        };

    private static bool ContainsHiddenLearningLeakage(string payload)
        => HiddenLearningLeakageTokens.Any(token =>
            payload.Contains(token, StringComparison.OrdinalIgnoreCase));

    private static string Normalize(string value)
        => string.Join("", (value ?? string.Empty).Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
}
