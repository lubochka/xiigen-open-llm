// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AgentInfrastructure;

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

    private static readonly Regex LocalPathRegex = new(
        @"(?i)(file:///?[A-Za-z]:[\\/][^\s""']+|(?<![A-Za-z])[A-Za-z]:[\\/][^\s""']+|\\\\[^\s""']+|/(Users|home)/[^\s""']+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex UserIdentifierRegex = BuildUserIdentifierRegex();

    private static Regex BuildUserIdentifierRegex()
    {
        // Redact the current machine login (and any configured aliases) without
        // hardcoding a personal username in source.
        var logins = new List<string> { Environment.UserName };
        var aliases = Environment.GetEnvironmentVariable("XIIGEN_REDACTION_ALIASES");
        if (!string.IsNullOrWhiteSpace(aliases))
            logins.AddRange(aliases.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        var loginAlternation = string.Join("|", logins.Where(l => l.Length > 0).Select(Regex.Escape));
        var pattern = @"(?i)(user[-_:][A-Za-z0-9._-]+|[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}"
            + (loginAlternation.Length > 0 ? "|" + loginAlternation : "") + ")";
        return new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

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
        AssetResolutionFailed
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
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionDpoBuildResult(
    IReadOnlyList<DesignConversionDpoTrainingRecord> AcceptedRecords,
    IReadOnlyList<DesignConversionDpoTrainingRecord> QuarantinedRecords,
    IReadOnlyList<string> Warnings)
{
    public IReadOnlyList<DesignConversionDpoTrainingRecord> AllRecords =>
        AcceptedRecords.Concat(QuarantinedRecords).ToArray();
}

public sealed class DesignConversionDpoGenerator
{
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

        var evidenceGate = ValidateRuntimeCreatedCandidateEvidence(evaluation);
        if (evidenceGate.Count > 0)
        {
            var reason = string.Join(";", evidenceGate);
            return OperationResult<DesignConversionDpoBuildResult>.Ok(new DesignConversionDpoBuildResult(
                AcceptedRecords: [],
                QuarantinedRecords: [],
                Warnings: BuildRuntimeCandidateNotCreatedWarnings(reason)));
        }

        var build = BuildTrainReadyRecords(
        [
            BuildRuntimeCreatedStoredExample(evaluation, createdAtUtc ?? DateTimeOffset.UtcNow)
        ]);
        if (!build.IsSuccess)
            return build;

        return OperationResult<DesignConversionDpoBuildResult>.Ok(build.Value! with
        {
            Warnings = build.Value.Warnings
                .Append("runtime_created_dpo_candidate")
                .Append("runtime_created_dpo_candidate_review_pending")
                .Append("runtime_created_dpo_candidate_approval_required")
                .Append("training_candidate_only")
                .Append("approved_for_training=false")
                .Append("synthetic_negative=true")
                .Append("scaffold_negative=true")
                .Append("training_grade=false")
                .Append("not_training_grade=true")
                .Append("production_dpo=false")
                .Append("product_not_ready")
                .Append("model_readiness_claimed=false")
                .Append("high_quality_negative_pair_not_proven")
                .Append("held_out_not_proven")
                .Append("source_disjoint_not_proven")
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        });
    }

    private static IReadOnlyList<string> ValidateRuntimeCreatedCandidateEvidence(
        DesignConversionInternalEvaluationResult evaluation)
    {
        var reasons = new List<string>();
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
        =>
        [
            "runtime_created_dpo_candidate_not_created_insufficient_evidence",
            $"runtime_created_dpo_candidate_quarantine_reason={reason}",
            "runtime_created_dpo_candidate=false",
            "chosen_rejected_pair_or_negative_quality_not_proven",
            "training_candidate_only",
            "approved_for_training=false",
            "production_dpo=false",
            "product_not_ready",
            "model_readiness_claimed=false",
            "high_quality_negative_pair_not_proven",
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

    private static DesignConversionStoredDpoExample BuildRuntimeCreatedStoredExample(
        DesignConversionInternalEvaluationResult evaluation,
        DateTimeOffset createdAtUtc)
    {
        var request = evaluation.SubmittedRequest;
        var output = evaluation.RuntimeFlowResult.Output;
        var chosenScore = Math.Max(0.01, Clamp01(output.Verdict.Score));
        // The runtime baseline is synthetic review material, but it must stay lower than chosen
        // so the negative-pair guard can keep the row available for approval review.
        var rejectedScore = Math.Max(0.0, Math.Round(chosenScore - 0.01, 6));

        var chosen = output with
        {
            Html = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-chosen-html",
                $"{evaluation.ImprovedHtml}|{output.OutputId}"),
            Css = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-chosen-css",
                $"{evaluation.ImprovedCss}|{output.OutputId}"),
            EvidenceRefs = output.EvidenceRefs
                .Concat(evaluation.EvidenceRefs)
                .Append("runtime_created_dpo_candidate:chosen_output_ref")
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Verdict = output.Verdict with
            {
                Score = chosenScore
            },
            Accepted = true
        };
        var rejected = new DesignConversionImprovementOutput(
            OutputId: $"{output.OutputId}:runtime-baseline-rejected",
            CandidateId: $"{output.CandidateId}:runtime-baseline-rejected",
            Html: DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-rejected-html",
                $"{request.Html}|{request.RequestId}|baseline"),
            Css: DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-rejected-css",
                $"{request.Css}|{request.RequestId}|baseline"),
            Verdict: new VisualVerdict(
                VerdictId: $"{output.Verdict.VerdictId}:runtime-baseline-rejected",
                Score: rejectedScore,
                Defects: BuildRuntimeRejectedDefects(evaluation),
                EvidenceRefs: evaluation.EvidenceRefs
                    .Append("runtime_created_dpo_candidate:rejected_baseline_ref")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Passed: false),
            QaResults: evaluation.QaResults.ToArray(),
            EvidenceRefs: evaluation.EvidenceRefs
                .Append("runtime_created_dpo_candidate:rejected_baseline_ref")
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Accepted: false);

        return new DesignConversionStoredDpoExample(
            ExampleId: DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-dpo-example",
                $"{evaluation.JobId}|{output.OutputId}"),
            UnitId: DesignConversionDpoUnits.ImproverUnit,
            InternalUnitStatePayload: BuildRuntimeInternalUnitStatePayload(evaluation),
            InternalVectorPayload: BuildRuntimeVector(evaluation),
            SourceStructurePayload: BuildRuntimeSourceStructurePayload(evaluation),
            Chosen: chosen,
            Rejected: rejected,
            TieBreakerPayload: null,
            UsefulnessScore: Math.Round(Math.Clamp(chosenScore + Math.Max(0.01, chosenScore - rejectedScore), 0.1, 1.5), 6),
            NegativeQualityReason: "synthetic_runtime_not_train_ready;synthetic_negative;scaffold_negative;not_training_grade;training_grade=false;runtime_baseline_current_output_review_candidate;rejected_visual_score_not_measured;real_rejected_artifact_not_proven;high_quality_negative_pair_not_proven",
            NegativeReasonTaxonomy: DesignConversionNegativeReasonTaxonomy.SyntheticRuntimeNotTrainReady,
            Provenance: new DesignConversionDpoProvenance(
                SourceRequestId: DesignConversionDpoLeakageGuard.HashRef("dcm-source-request", request.RequestId),
                SourceKind: request.SourceKind.ToString(),
                SourceManifestRef: ResolveSourceManifestRef(request),
                ReviewerRef: "runtime-local-artifact-reviewer",
                CreatedAtUtc: createdAtUtc,
                Metadata: new Dictionary<string, string>
                {
                    ["runtime_created_dpo_candidate"] = bool.TrueString,
                    ["review_required"] = bool.TrueString,
                    ["approval_required"] = bool.TrueString,
                    ["approval_status"] = "pending",
                    ["review_status"] = "runtime-candidate-review-pending",
                    ["approved_for_training"] = bool.FalseString,
                    ["synthetic_negative"] = bool.TrueString,
                    ["scaffold_negative"] = bool.TrueString,
                    ["training_grade"] = bool.FalseString,
                    ["not_training_grade"] = bool.TrueString,
                    ["training_grade_boundary"] = "synthetic_runtime_scaffold_not_training_grade",
                    ["synthetic_negative_source"] = "runtime_created_baseline_rejected_candidate",
                    ["training_candidate_only"] = bool.TrueString,
                    ["production_dpo"] = bool.FalseString,
                    ["product_not_ready"] = bool.TrueString,
                    ["model_readiness_claimed"] = bool.FalseString,
                    ["held_out_not_proven"] = bool.TrueString,
                    ["local_artifact_db"] = bool.TrueString,
                    ["source_disjoint_not_proven"] = bool.TrueString,
                    ["high_quality_negative_pair_not_proven"] = bool.TrueString,
                    ["deterministic_scaffold_not_learned_authority"] = bool.TrueString,
                    ["raw_html_included"] = bool.FalseString,
                    ["raw_css_included"] = bool.FalseString,
                    ["raw_design_tree_included"] = bool.FalseString,
                    ["provider_payload_included"] = bool.FalseString,
                    ["split"] = "runtime_training_candidate_review_pending"
                }),
            License: new DesignConversionLicenseScope(
                LicenseId: "runtime-local-artifact-db-training-candidate-only",
                RightsScope: "local_artifact_db_candidate_review_only",
                AllowsTraining: true,
                ContainsUserPrivateData: false,
                Metadata: new Dictionary<string, string>
                {
                    ["training_candidate_only"] = bool.TrueString,
                    ["review_required"] = bool.TrueString,
                    ["approval_required"] = bool.TrueString,
                    ["approval_status"] = "pending",
                    ["review_status"] = "runtime-candidate-review-pending",
                    ["approved_for_training"] = bool.FalseString,
                    ["synthetic_negative"] = bool.TrueString,
                    ["scaffold_negative"] = bool.TrueString,
                    ["training_grade"] = bool.FalseString,
                    ["not_training_grade"] = bool.TrueString,
                    ["training_grade_boundary"] = "synthetic_runtime_scaffold_not_training_grade",
                    ["production_dpo"] = bool.FalseString,
                    ["product_not_ready"] = bool.TrueString,
                    ["model_readiness_claimed"] = bool.FalseString,
                    ["high_quality_negative_pair_not_proven"] = bool.TrueString,
                    ["held_out_not_proven"] = bool.TrueString,
                    ["source_disjoint_not_proven"] = bool.TrueString,
                    ["real_training_ready"] = bool.FalseString,
                    ["trained_weights_ready"] = bool.FalseString,
                    ["production_db_ready"] = bool.FalseString
                }),
            TeacherEvidenceRefs: evaluation.EvidenceRefs
                .Append("runtime_created_dpo_candidate:training_candidate_only")
                .Append("runtime_created_dpo_candidate:review_pending")
                .Append("runtime_created_dpo_candidate:approval_required")
                .Append("runtime_created_dpo_candidate:product_not_ready")
                .Append("runtime_created_dpo_candidate:held_out_not_proven")
                .Append("runtime_created_dpo_candidate:source_disjoint_not_proven")
                .ToArray(),
            Metadata: new Dictionary<string, string>
            {
                ["runtime_created_dpo_candidate"] = bool.TrueString,
                ["review_required"] = bool.TrueString,
                ["approval_required"] = bool.TrueString,
                ["approval_status"] = "pending",
                ["review_status"] = "runtime-candidate-review-pending",
                ["approved_for_training"] = bool.FalseString,
                ["training_candidate_only"] = bool.TrueString,
                ["production_dpo"] = bool.FalseString,
                ["product_not_ready"] = bool.TrueString,
                ["model_readiness_claimed"] = bool.FalseString,
                ["held_out_not_proven"] = bool.TrueString,
                ["source_disjoint_not_proven"] = bool.TrueString,
                ["local_artifact_db"] = bool.TrueString,
                ["raw_payloads_included"] = bool.FalseString
            });
    }

    private static IReadOnlyList<VisualDefect> BuildRuntimeRejectedDefects(
        DesignConversionInternalEvaluationResult evaluation)
    {
        var visualDefects = evaluation.VisualJudgeResults
            .SelectMany(result => result.Verdict.Defects)
            .Concat(evaluation.ComparisonArtifacts.SelectMany(artifact => artifact.Verdict.Defects))
            .GroupBy(defect => defect.Category, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
        return visualDefects;
    }

    private static string BuildRuntimeInternalUnitStatePayload(
        DesignConversionInternalEvaluationResult evaluation)
        => JsonSerializer.Serialize(new
        {
            schema = "runtime-created-dpo-candidate-v1",
            role = "local_artifact_db_candidate_review_only",
            unit_id = DesignConversionDpoUnits.ImproverUnit,
            job_ref = DesignConversionDpoLeakageGuard.HashRef("dcm-runtime-job", evaluation.JobId),
            result_ref = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-result",
                evaluation.RuntimeFlowResult.Output.OutputId),
            visual = new
            {
                status = evaluation.VisualQaBoundaryTrace.VisualJudgeStatus,
                tier = evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason,
                external_ai = evaluation.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted
            },
            boundary = new
            {
                training_candidate_only = true,
                review_required = true,
                approval_required = true,
                approval_status = "pending",
                review_status = "runtime-candidate-review-pending",
                approved_for_training = false,
                production_dpo = false,
                product_not_ready = true,
                runtime_created_dpo_candidate = true,
                model_readiness_claimed = false,
                held_out_not_proven = true,
                high_quality_negative_pair_not_proven = true,
                source_disjoint_not_proven = true,
                deterministic_scaffold_not_learned_authority = true
            }
        }, CompactJsonOptions);

    private static IReadOnlyList<double> BuildRuntimeVector(
        DesignConversionInternalEvaluationResult evaluation)
        =>
        [
            evaluation.RenderResults.Count,
            evaluation.ComparisonArtifacts.Count,
            evaluation.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted ? 1.0 : 0.0
        ];

    private static string BuildRuntimeSourceStructurePayload(
        DesignConversionInternalEvaluationResult evaluation)
    {
        var request = evaluation.SubmittedRequest;
        return JsonSerializer.Serialize(new
        {
            schema = "runtime-created-dpo-source-refs-v1",
            source_kind = request.SourceKind.ToString(),
            source_manifest_ref = ResolveSourceManifestRef(request),
            design_tree_ref = request.DesignTreePayload.Sha256,
            original_tree_ref = request.OriginalTreePayload.Sha256,
            source_artifact_refs = request.Screenshots
                .Concat(request.DesignRenders)
                .Select(artifact => new
                {
                    artifact_ref = DesignConversionDpoLeakageGuard.HashRef("dcm-source-artifact", artifact.ArtifactId),
                    sha256 = artifact.Sha256,
                    kind = artifact.Kind
                })
                .ToArray(),
            result_ref = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-runtime-result",
                evaluation.RuntimeFlowResult.Output.OutputId),
            raw_payloads_included = false
        }, CompactJsonOptions);
    }

    private static string ResolveSourceManifestRef(SubmittedDesignConversionRequest request)
        => request.Provenance.TryGetValue("source_manifest_ref", out var sourceManifestRef)
           && !string.IsNullOrWhiteSpace(sourceManifestRef)
            ? DesignConversionDpoLeakageGuard.HashRef("dcm-source-manifest", sourceManifestRef)
            : request.DesignTreePayload.Sha256;

    private static double Clamp01(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return 0.0;

        return Math.Clamp(value, 0.0, 1.0);
    }

    private DesignConversionDpoTrainingRecord BuildRecord(
        DesignConversionStoredDpoExample example,
        int ordinal)
    {
        var reasons = Validate(example).ToList();
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

        return new DesignConversionDpoTrainingRecord(
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
                EvidenceRefs: BuildQuarantineEvidence(example, reasons, leakageRedactions)));
    }

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

        ValidateOutput(example.Chosen, "chosen", reasons);
        ValidateOutput(example.Rejected, "rejected", reasons);

        var negativeQuality = DesignConversionNegativeQualityValidator.Validate(example);
        reasons.AddRange(negativeQuality.Reasons);

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
