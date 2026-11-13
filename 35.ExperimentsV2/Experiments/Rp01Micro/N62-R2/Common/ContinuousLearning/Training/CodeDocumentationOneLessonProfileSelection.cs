// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Passive startup values live below the routing/material owner.

using System.Globalization; // Floating inputs retain invariant round-trip spelling.
using System.Security.Cryptography; // Commit complete raw inputs and the typed preimage.
using System.Text; // Reject malformed Unicode rather than replacing it.
using System.Text.Json; // Validate the closed protected value shape before typed decoding.
using AgentInfrastructure; // Pure admission returns an ordinary failed value.

/// <summary>A descriptive, deeply frozen profile input; it carries no training or Session capability.</summary>
internal sealed class CodeDocumentationOneLessonProfileSelection // Keep the complete descriptive profile state on its existing owner.
{ // Keep the complete descriptive profile state on its existing owner.
    internal const string InvalidErrorCode = "ONE_LESSON_PROFILE_SELECTION_INVALID"; // Shared structural/current-input disagreement code.
    internal const int MaximumBytes = 1_048_576; // Preserve the existing catalog ceiling for pure preimages too.
    private static readonly UTF8Encoding Utf8 = new(false, true); // Exact Unicode is part of every commitment.
    private readonly CodeDocumentationProfilePreimageV2 _value; // No caller-owned arrays or dictionaries survive admission.
    internal string ProfileSelectionSha256 { get; } // Derived locally; never supplied in the preimage.
    internal CodeDocumentationProfilePreimageV2 Snapshot => Copy(_value); // Legacy mutable DTO views are complete fresh deep copies.

    private CodeDocumentationOneLessonProfileSelection(CodeDocumentationProfilePreimageV2 value) // Copy every writable vector while preserving complete selected order.
    { // Copy every writable vector while preserving complete selected order.
        _value = Copy(value); // Copy before hashing so later caller mutation cannot change either identity or content.
        var bytes = CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(this); // Copy every writable vector while preserving complete selected order.
        if (bytes.Length > MaximumBytes) throw new JsonException("Profile input exceeds the catalog ceiling."); // Copy every writable vector while preserving complete selected order.
        ProfileSelectionSha256 = Convert.ToHexString(SHA256.HashData( // Copy every writable vector while preserving complete selected order.
            Utf8.GetBytes("xiigen-s04-profileselection-v2\n").Concat(bytes).ToArray())).ToLowerInvariant(); // Copy every writable vector while preserving complete selected order.
    } // Copy every writable vector while preserving complete selected order.

    internal static OperationResult<CodeDocumentationOneLessonProfileSelection> Create(CodeDocumentationProfilePreimageV2 value) // Validate and copy all complete selected profile inputs before deriving their digest.
    { // Validate and copy all complete selected profile inputs before deriving their digest.
        try // Validate and copy all complete selected profile inputs before deriving their digest.
        { // Validate and copy all complete selected profile inputs before deriving their digest.
            // The same closed typed schema is checked for both trusted DTO and protected JSON input.
            ValidateOriginalUnicode(value, typeof(CodeDocumentationProfilePreimageV2)); // Reject malformed original UTF-16 before JSON can replace a surrogate with U+FFFD.
            var frozen = Copy(value); // Validate the same private vector snapshot that will be committed, without retaining caller arrays.
            ValidateOriginalUnicode(frozen, typeof(CodeDocumentationProfilePreimageV2)); // Recheck copied leaves so an intervening array mutation cannot enter the committed snapshot.
            ValidateShape(JsonSerializer.SerializeToElement(frozen), typeof(CodeDocumentationProfilePreimageV2)); // The closed JSON shape now represents already checked original strings.
            Validate(frozen); // Apply all existing counts, scalar spelling and raw identity checks to that same private snapshot.
            return OperationResult<CodeDocumentationOneLessonProfileSelection>.Ok(new(frozen)); // Only the validated complete snapshot reaches hashing and retention.
        } // Validate and copy all complete selected profile inputs before deriving their digest.
        catch (Exception e) when (e is JsonException or ArgumentException or InvalidOperationException or EncoderFallbackException or DecoderFallbackException or FormatException or OverflowException) // Convert only the declared nonfatal validation failures to ordinary rejection.
        { // Validate and copy all complete selected profile inputs before deriving their digest.
            return OperationResult<CodeDocumentationOneLessonProfileSelection>.Fail("The complete profile selection is invalid.", InvalidErrorCode); // Validate and copy all complete selected profile inputs before deriving their digest.
        } // Validate and copy all complete selected profile inputs before deriving their digest.
    } // Validate and copy all complete selected profile inputs before deriving their digest.

    internal static CodeDocumentationOneLessonProfileSelection Decode(JsonElement value) // Decode only the closed passive profile graph at startup.
    { // Decode only the closed passive profile graph at startup.
        ValidateShape(value, typeof(CodeDocumentationProfilePreimageV2)); // Reject unknown/duplicate/aliased/null members before deserialization.
        var result = Create(value.Deserialize<CodeDocumentationProfilePreimageV2>()!); // Decode only the closed passive profile graph at startup.
        if (!result.IsSuccess || result.Value is null) throw new JsonException("Invalid profile selection."); // Decode only the closed passive profile graph at startup.
        return result.Value; // Decode only the closed passive profile graph at startup.
    } // Decode only the closed passive profile graph at startup.

    private static CodeDocumentationProfilePreimageV2 Copy(CodeDocumentationProfilePreimageV2 v) // Copy every writable vector while preserving complete selected order.
        => v with // Copy every writable vector while preserving complete selected order.
        { // Copy every writable vector while preserving complete selected order.
            ArtifactMap = v.ArtifactMap.Select(x => x with { }).ToArray(), // Copy every writable vector while preserving complete selected order.
            UnitMetadata = v.UnitMetadata.Select(x => x with { AllowedInputs = x.AllowedInputs.ToArray() }).ToArray(), // Copy every writable vector while preserving complete selected order.
            RoutingTrainingInputs = v.RoutingTrainingInputs.Select(x => x with { CandidateUnitIds = x.CandidateUnitIds.ToArray() }).ToArray(), // Copy every writable vector while preserving complete selected order.
            EvaluationRoutingInputs = v.EvaluationRoutingInputs.Select(x => x with // Copy every writable vector while preserving complete selected order.
            { CandidateUnitIds = x.CandidateUnitIds.ToArray(), Context = new Dictionary<string, string>(x.Context, StringComparer.Ordinal) }).ToArray(), // Copy every writable vector while preserving complete selected order.
            RoutingConfiguration = v.RoutingConfiguration with { TrainerStopWords = v.RoutingConfiguration.TrainerStopWords.ToArray() }, // Copy every writable vector while preserving complete selected order.
            RuntimeArtifactMap = v.RuntimeArtifactMap.Select(x => x with { LoraTargetModules = x.LoraTargetModules.ToArray() }).ToArray(), // Copy every writable vector while preserving complete selected order.
        }; // Every mutable vector, including nested evidence projections, crosses a copy boundary.

    private static void Validate(CodeDocumentationProfilePreimageV2 v) // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
    { // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        if (v.PreimageSchemaVersion != 2 || v.ArtifactMap.Length != 2 || v.UnitMetadata.Length != 3 || // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            v.RuntimeArtifactMap.Length != 3 || v.RoutingTrainingInputs.Length != 50 || v.EvaluationRoutingInputs.Length != 50) throw Invalid(); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        var paths = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        for (var i = 0; i < v.ArtifactMap.Length; i++) // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        { // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            var a = v.ArtifactMap[i]; // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            if (a.Role != (i == 0 ? "TrainingRows" : "HeldOutRows") || !ValidRelativePath(a.RepositoryRelativePath) || // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
                !paths.Add(a.RepositoryRelativePath) || !CodeDocumentationOneLessonV2Wire.Hash(a.FileSha256) || // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
                a.FileByteLength is <= 0 or > MaximumBytes || a.RawUtf8Base64.Length != checked(((a.FileByteLength + 2) / 3) * 4)) throw Invalid(); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            var raw = Convert.FromBase64String(a.RawUtf8Base64); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            if (raw.LongLength != a.FileByteLength || Convert.ToBase64String(raw) != a.RawUtf8Base64 || // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
                Convert.ToHexString(SHA256.HashData(raw)).ToLowerInvariant() != a.FileSha256) throw Invalid(); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            _ = Utf8.GetString(raw); // Raw JSONL is descriptive here; only the Batch owner validates its row semantics.
        } // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        // These are the closed three-unit schema identities, not a metadata registry or algorithm parameter copy.
        string[] units = ["unit-orchestration-plan-v309", "unit-ru-en-general", "unit-dotnet-legacy-doc-l1"]; // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        if (!v.RuntimeArtifactMap.Select(x => x.UnitId).SequenceEqual(units) || !v.UnitMetadata.Select(x => x.UnitId).SequenceEqual(units)) throw Invalid(); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        foreach (var m in v.UnitMetadata) // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        { // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            if (string.IsNullOrWhiteSpace(m.Role) || string.IsNullOrWhiteSpace(m.Domain) || string.IsNullOrWhiteSpace(m.MetadataSourceRef)) throw Invalid(); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            Double(m.ConfidenceFloor); Double(m.BaselinePrior); // Finite ranges remain the actual registry's responsibility, without clamping.
        } // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        foreach (var t in v.RoutingTrainingInputs) // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        { // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            Presence(t.RequiredDirectionPresent, t.RequiredDirection); Presence(t.DomainHintPresent, t.DomainHint); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            if (!t.CandidateUnitIds.SequenceEqual(units) || !units.Contains(t.ChosenUnitId, StringComparer.Ordinal) || !units.Contains(t.RejectedUnitId, StringComparer.Ordinal)) throw Invalid(); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        } // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        foreach (var e in v.EvaluationRoutingInputs) // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        { // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            Presence(e.RequiredDirectionPresent, e.RequiredDirection); Presence(e.DomainHintPresent, e.DomainHint); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            if (!e.CandidateUnitIds.SequenceEqual(units) || !e.Context.Keys.Order(StringComparer.Ordinal).SequenceEqual(ContextKeys.Order(StringComparer.Ordinal))) throw Invalid(); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        } // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        var c = v.RoutingConfiguration; // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        if (!c.TrainerStopWords.SequenceEqual(c.TrainerStopWords.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))) throw Invalid(); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        foreach (var d in new[] { c.ScorerDomainBoost, c.ScorerDirectionBoost, c.ScorerMaximumConfidence, c.ScorerBaseConfidence, // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            c.ScorerKeywordConfidenceMultiplier, c.ScorerDomainConfidenceMultiplier, c.ScorerDirectionConfidenceMultiplier, // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            c.MinimumRoutingConfidence, c.MinimumScoreGap }) Double(d); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        foreach (var a in v.RuntimeArtifactMap) // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        { // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            Presence(a.RopeThetaPresent, a.RopeTheta); Presence(a.RmsNormEpsPresent, a.RmsNormEps); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            if (a.RopeThetaPresent) Single(a.RopeTheta); if (a.RmsNormEpsPresent) Single(a.RmsNormEps); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
            Single(a.LoraAlpha); Single(a.LoraDropout); // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        } // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
    } // Check the complete preimage counts, identities, byte pins and finite scalar spelling.

    private static readonly string[] ContextKeys = ["app_side", "runtime_family", "target_framework", "csharp_language_version", // Check the complete preimage counts, identities, byte pins and finite scalar spelling.
        "project_style", "web_framework", "frontend_framework", "documentation_level", "stack_label"]; // Exact request-projection schema, including blank values.

    internal static bool ValidRelativePath(string path) // Exclude ambiguous relative names before any material path is opened.
    { // Exclude ambiguous relative names before any material path is opened.
        if (string.IsNullOrEmpty(path) || !CodeDocumentationOneLessonV2Wire.ValidUtf8Text(path, MaximumBytes) || // Exclude ambiguous relative names before any material path is opened.
            path.Any(char.IsControl) || path.Contains('\\') || path.Contains(':')) return false; // Exclude ambiguous relative names before any material path is opened.
        foreach (var p in path.Split('/')) // Exclude ambiguous relative names before any material path is opened.
        { // Exclude ambiguous relative names before any material path is opened.
            if (p.Length == 0 || p is "." or ".." || p.EndsWith('.') || p.EndsWith(' ')) return false; // Exclude ambiguous relative names before any material path is opened.
            var stem = p.Split('.')[0].ToUpperInvariant(); // Exclude ambiguous relative names before any material path is opened.
            if (stem is "CON" or "PRN" or "AUX" or "NUL" || stem.Length == 4 && // Exclude ambiguous relative names before any material path is opened.
                (stem.StartsWith("COM", StringComparison.Ordinal) || stem.StartsWith("LPT", StringComparison.Ordinal)) && stem[3] is >= '1' and <= '9') return false; // Exclude ambiguous relative names before any material path is opened.
        } // Exclude ambiguous relative names before any material path is opened.
        return true; // Platform full-path validation and reparse checks remain with the real material owner.
    } // Exclude ambiguous relative names before any material path is opened.

    private static void Presence(bool present, string value) { if (!present && value != "") throw Invalid(); } // Absent optional values retain an exact empty spelling rather than a null writer.
    private static void Double(string s) // Admit only finite invariant binary64 round-trip text, preserving its sign and value.
    { if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) || !double.IsFinite(d) || d.ToString("R", CultureInfo.InvariantCulture) != s) throw Invalid(); } // Reject alternate numeric spellings instead of normalizing the commitment.
    private static void Single(string s) // Admit only finite invariant binary32 round-trip text from the actual runtime configuration.
    { if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) || !float.IsFinite(d) || d.ToString("R", CultureInfo.InvariantCulture) != s) throw Invalid(); } // Reject alternate numeric spellings instead of clamping the commitment.
    private static JsonException Invalid() => new("Invalid full profile value."); // Structural failures have one ordinary fail-closed admission path.

    private static void ValidateOriginalUnicode(object? value, Type type) // Walk only original typed DTO leaves before any JSON encoding can replace malformed Unicode.
    { // This validates UTF-16 identity; it does not parse rows or write a second canonical representation.
        if (value is null) throw Invalid(); // Every field in the passive profile graph is required and non-null.
        if (type == typeof(string)) { _ = Utf8.GetByteCount((string)value); return; } // Strict UTF-8 encoding rejects both unmatched high and low surrogates.
        if (type == typeof(bool) || type == typeof(int) || type == typeof(long)) return; // Non-text CLR primitives remain subject to the existing shape/value checks.
        if (type.IsArray) // Inspect every original element of each declared DTO or string vector.
        { foreach (var item in (Array)value) ValidateOriginalUnicode(item, type.GetElementType()!); return; } // No selected position or nested string array is skipped.
        if (type == typeof(Dictionary<string, string>)) // Context keys and values are original strings too.
        { // Validate keys before the existing exact nine-key shape policy runs.
            foreach (var item in (Dictionary<string, string>)value) // Preserve all declared context entries while checking their original text.
            { ValidateOriginalUnicode(item.Key, typeof(string)); ValidateOriginalUnicode(item.Value, typeof(string)); } // Neither malformed keys nor values can be silently replaced.
            return; // This traversal changes no caller or private dictionary.
        } // Only the existing context dictionary is part of this closed graph.
        if (type.Namespace != typeof(CodeDocumentationProfilePreimageV2).Namespace || !type.Name.StartsWith("CodeDocumentationProfile", StringComparison.Ordinal)) throw Invalid(); // Restrict reflection to the same passive profile DTO graph as shape validation.
        foreach (var property in type.GetProperties()) ValidateOriginalUnicode(property.GetValue(value), property.PropertyType); // Check every declared original string leaf, including metadata/configuration/projections/runtime.
    } // All original strings are checked before the sole JSON shape/canonical writer can observe them.

    private static void ValidateShape(JsonElement e, Type type) // Keep primitive and object validation restricted to the declared passive profile graph.
    { // Keep primitive and object validation restricted to the declared passive profile graph.
        // Reflection is restricted to the passive closed profile DTO graph; it is not a JSONL or general canonical parser.
        if (type == typeof(string)) { if (e.ValueKind != JsonValueKind.String) throw Invalid(); _ = Utf8.GetByteCount(e.GetString()!); return; } // Keep primitive and object validation restricted to the declared passive profile graph.
        if (type == typeof(bool)) { if (e.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw Invalid(); return; } // Keep primitive and object validation restricted to the declared passive profile graph.
        if (type == typeof(int) || type == typeof(long)) // Keep primitive and object validation restricted to the declared passive profile graph.
        { if (e.ValueKind != JsonValueKind.Number || !e.TryGetInt64(out var n) || n < 0 || type == typeof(int) && n > int.MaxValue || e.GetRawText() != n.ToString(CultureInfo.InvariantCulture)) throw Invalid(); return; } // Keep primitive and object validation restricted to the declared passive profile graph.
        if (type.IsArray) // Keep primitive and object validation restricted to the declared passive profile graph.
        { if (e.ValueKind != JsonValueKind.Array) throw Invalid(); foreach (var x in e.EnumerateArray()) ValidateShape(x, type.GetElementType()!); return; } // Keep primitive and object validation restricted to the declared passive profile graph.
        if (type == typeof(Dictionary<string, string>)) // Keep primitive and object validation restricted to the declared passive profile graph.
        { if (!CodeDocumentationOneLessonV2Wire.ExactFields(e, ContextKeys)) throw Invalid(); foreach (var p in e.EnumerateObject()) ValidateShape(p.Value, typeof(string)); return; } // Keep primitive and object validation restricted to the declared passive profile graph.
        if (type.Namespace != typeof(CodeDocumentationProfilePreimageV2).Namespace || !type.Name.StartsWith("CodeDocumentationProfile", StringComparison.Ordinal)) throw Invalid(); // Keep primitive and object validation restricted to the declared passive profile graph.
        var properties = type.GetProperties(); // Keep primitive and object validation restricted to the declared passive profile graph.
        if (!CodeDocumentationOneLessonV2Wire.ExactFields(e, properties.Select(p => p.Name).ToArray())) throw Invalid(); // Keep primitive and object validation restricted to the declared passive profile graph.
        foreach (var p in properties) ValidateShape(e.GetProperty(p.Name), p.PropertyType); // Keep primitive and object validation restricted to the declared passive profile graph.
    } // Keep primitive and object validation restricted to the declared passive profile graph.
} // Keep primitive and object validation restricted to the declared passive profile graph.

internal sealed record CodeDocumentationProfilePreimageV2(int PreimageSchemaVersion, // Retain every declared field of this closed passive profile tuple.
    CodeDocumentationProfileArtifactV2[] ArtifactMap, CodeDocumentationProfileMetadataV2[] UnitMetadata, // Retain every declared field of this closed passive profile tuple.
    CodeDocumentationProfileTrainingV2[] RoutingTrainingInputs, CodeDocumentationProfileEvaluationV2[] EvaluationRoutingInputs, // Retain every declared field of this closed passive profile tuple.
    CodeDocumentationProfileConfigurationV2 RoutingConfiguration, CodeDocumentationProfileRuntimeV2[] RuntimeArtifactMap); // Retain every declared field of this closed passive profile tuple.
internal sealed record CodeDocumentationProfileArtifactV2(string Role, string RepositoryRelativePath, string FileSha256, long FileByteLength, string RawUtf8Base64); // Retain every declared field of this closed passive profile tuple.
internal sealed record CodeDocumentationProfileMetadataV2(string UnitId, string Role, string Domain, string[] AllowedInputs, string ConfidenceFloor, string MetadataSourceRef, string BaselinePrior); // Retain every declared field of this closed passive profile tuple.
internal sealed record CodeDocumentationProfileTrainingV2(string ExampleId, string Prompt, string TaskSlice, // Retain every declared field of this closed passive profile tuple.
    bool RequiredDirectionPresent, string RequiredDirection, bool DomainHintPresent, string DomainHint, string[] CandidateUnitIds, // Retain every declared field of this closed passive profile tuple.
    string ChosenUnitId, string RejectedUnitId, string CorrectReason, string WrongRouteConsequence); // Retain every declared field of this closed passive profile tuple.
internal sealed record CodeDocumentationProfileEvaluationV2(string RequestId, string OriginalPrompt, string TaskSlice, // Retain every declared field of this closed passive profile tuple.
    bool RequiredDirectionPresent, string RequiredDirection, bool DomainHintPresent, string DomainHint, string[] CandidateUnitIds, Dictionary<string, string> Context); // Retain every declared field of this closed passive profile tuple.
internal sealed record CodeDocumentationProfileConfigurationV2(string[] TrainerStopWords, int TrainerMinimumKeywordLength, // Retain every declared field of this closed passive profile tuple.
    string TrainerKeywordSeparators, string TrainerCSharpFrom, string TrainerCSharpTo, string TrainerDotNetFrom, string TrainerDotNetTo, // Retain every declared field of this closed passive profile tuple.
    bool TrainerLowercaseInvariant, string TrainerKeywordComparer, string ScorerMatchComparer, string ScorerAuthorityRole, // Retain every declared field of this closed passive profile tuple.
    string ScorerDomainBoost, string ScorerDirectionBoost, string ScorerMaximumConfidence, string ScorerBaseConfidence, // Retain every declared field of this closed passive profile tuple.
    string ScorerKeywordConfidenceMultiplier, string ScorerDomainConfidenceMultiplier, string ScorerDirectionConfidenceMultiplier, // Retain every declared field of this closed passive profile tuple.
    string MinimumRoutingConfidence, string MinimumScoreGap, string ThresholdsRef, string RoutingFlowId, string RoutingScenarioId); // Retain every declared field of this closed passive profile tuple.
internal sealed record CodeDocumentationProfileRuntimeV2(string UnitId, string UnitRole, string DomainTag, string Version, // Retain every declared field of this closed passive profile tuple.
    string ServerBaseUrl, string UnitEndpoint, string ManifestSha256, string TrainingBranch, string TrainingRunId, // Retain every declared field of this closed passive profile tuple.
    string SourceDatasetSha256, string TokenizerPath, int VocabSize, int ContextLen, int HiddenDim, int NumLayers, int NumHeads, // Retain every declared field of this closed passive profile tuple.
    int NumKvHeads, int FfnHiddenDim, bool RopeThetaPresent, string RopeTheta, bool RmsNormEpsPresent, string RmsNormEps, // Retain every declared field of this closed passive profile tuple.
    int LoraRank, string LoraAlpha, string LoraDropout, string[] LoraTargetModules); // Retain every declared field of this closed passive profile tuple.
