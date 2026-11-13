// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// Canonical byte representation used at every OrchestrationReasoning authority
/// boundary. The representation is deliberately independent of
/// JsonSerializer naming policies and runtime collection order.
/// </summary>
public static class OrchestrationReasoningCanonicalState
{
    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Indented = false,
        SkipValidation = false
    };

    public static IReadOnlyList<OrchestrationReasoningLearnedPattern> NormalizePatterns(
        IEnumerable<OrchestrationReasoningLearnedPattern> patterns)
        => (patterns ?? throw new InvalidDataException("Learned patterns are required."))
            .Select(NormalizePattern)
            .OrderBy(pattern => pattern.PatternId, StringComparer.Ordinal)
            .ToArray();

    public static byte[] SerializePatterns(IEnumerable<OrchestrationReasoningLearnedPattern> patterns)
    {
        var normalized = NormalizePatterns(patterns);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
        {
            writer.WriteStartArray();
            foreach (var pattern in normalized)
                WritePattern(writer, pattern);
            writer.WriteEndArray();
        }

        return stream.ToArray();
    }

    public static string ComputeLearnedPatternsSha256(
        IEnumerable<OrchestrationReasoningLearnedPattern> patterns)
        => HashBytes(SerializePatterns(patterns));

    public static byte[] SerializeStateCore(OrchestrationReasoningLearnedGuideStateDescriptor state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var learnedPatternsSha256 = RequiredHash(
            state.LearnedPatternsSha256,
            nameof(state.LearnedPatternsSha256));

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
        {
            writer.WriteStartObject();
            writer.WriteString("state_id", NormalizeRequired(state.StateId, nameof(state.StateId)));
            writer.WriteString("guide_state_ref", NormalizeRequired(state.GuideStateRef, nameof(state.GuideStateRef)));
            writer.WriteString("manifest_id", NormalizeRequired(state.ManifestId, nameof(state.ManifestId)));
            writer.WriteString("manifest_sha256", RequiredHash(state.ManifestSha256, nameof(state.ManifestSha256)));
            writer.WriteString("machine_type", NormalizeRequired(state.MachineType, nameof(state.MachineType)));
            writer.WriteString("schema_version", NormalizeRequired(state.SchemaVersion, nameof(state.SchemaVersion)));
            writer.WriteString("source_base_commit", NormalizeRequired(state.SourceBaseCommit, nameof(state.SourceBaseCommit)));
            writer.WriteString("learned_patterns_sha256", learnedPatternsSha256);
            writer.WriteEndObject();
        }

        return stream.ToArray();
    }

    public static string ComputeStateSha256(OrchestrationReasoningLearnedGuideStateDescriptor state)
        => HashBytes(SerializeStateCore(state));

    public static OrchestrationReasoningLearnedGuideStateDescriptor CreateDescriptor(
        string stateId,
        string guideStateRef,
        string manifestId,
        string manifestSha256,
        string sourceBaseCommit,
        IEnumerable<OrchestrationReasoningLearnedPattern> patterns)
    {
        var normalized = NormalizePatterns(patterns);
        var patternHash = ComputeLearnedPatternsSha256(normalized);
        var state = new OrchestrationReasoningLearnedGuideStateDescriptor(
            StateId: NormalizeRequired(stateId, nameof(stateId)),
            GuideStateRef: NormalizeRequired(guideStateRef, nameof(guideStateRef)),
            ManifestId: NormalizeRequired(manifestId, nameof(manifestId)),
            ManifestSha256: RequiredHash(manifestSha256, nameof(manifestSha256)),
            MachineType: nameof(OrchestrationReasoningMachine),
            SchemaVersion: "orchestration-reasoning-state-v2",
            SourceBaseCommit: NormalizeRequired(sourceBaseCommit, nameof(sourceBaseCommit)),
            StateSha256: string.Empty,
            LearnedPatterns: normalized,
            LearnedPatternsSha256: patternHash);

        return state with { StateSha256 = ComputeStateSha256(state) };
    }

    public static bool TryValidate(
        OrchestrationReasoningLearnedGuideStateDescriptor? state,
        out string error)
    {
        error = string.Empty;
        try
        {
            if (state is null)
                throw new InvalidDataException("Learned state is required.");
            if (!string.Equals(state.SchemaVersion, "orchestration-reasoning-state-v2", StringComparison.Ordinal))
                throw new InvalidDataException("Learned state schema is not v2.");
            if (!string.Equals(state.MachineType, nameof(OrchestrationReasoningMachine), StringComparison.Ordinal))
                throw new InvalidDataException("Learned state machine type is invalid.");
            if (state.LearnedPatterns is null)
                throw new InvalidDataException("Learned state patterns collection is required.");

            var canonicalPatterns = NormalizePatterns(state.LearnedPatterns);
            if (canonicalPatterns.Select(pattern => pattern.PatternId).Distinct(StringComparer.Ordinal).Count()
                != canonicalPatterns.Count)
            {
                throw new InvalidDataException("Learned pattern ids must be unique.");
            }

            var patternHash = ComputeLearnedPatternsSha256(canonicalPatterns);
            if (!string.Equals(patternHash, state.LearnedPatternsSha256, StringComparison.Ordinal))
                throw new InvalidDataException("Learned pattern hash mismatch.");

            var stateHash = ComputeStateSha256(state);
            if (!string.Equals(stateHash, state.StateSha256, StringComparison.Ordinal))
                throw new InvalidDataException("Learned state hash mismatch.");

            return true;
        }
        catch (Exception ex) when (ex is InvalidDataException or ArgumentException)
        {
            error = ex.Message;
            return false;
        }
    }

    public static byte[] SerializeReceiptCore(OrchestrationReasoningSourceAdmissionReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schema_version", receipt.SchemaVersion);
            writer.WriteString("payload_sha256", RequiredHash(receipt.PayloadSha256, nameof(receipt.PayloadSha256)));
            writer.WriteString("manifest_sha256", RequiredHash(receipt.ManifestSha256, nameof(receipt.ManifestSha256)));
            writer.WriteString("ledger_sha256", RequiredHash(receipt.LedgerSha256, nameof(receipt.LedgerSha256)));
            writer.WriteString("evidence_sha256", RequiredHash(receipt.EvidenceSha256, nameof(receipt.EvidenceSha256)));
            writer.WriteString("allowed_source_kind", NormalizeRequired(receipt.AllowedSourceKind, nameof(receipt.AllowedSourceKind)));
            WriteStringArray(writer, "row_ids", receipt.RowIds);
            WriteStringArray(writer, "source_ids", receipt.SourceIds);
            WriteStringArray(writer, "pair_ids", receipt.PairIds);
            WriteStringArray(writer, "source_refs", receipt.SourceRefs);
            WriteStringArray(writer, "source_sha256s", receipt.SourceSha256s.Select(value => RequiredHash(value, "source_sha256")));
            writer.WriteString("learned_patterns_sha256", RequiredHash(receipt.LearnedPatternsSha256, nameof(receipt.LearnedPatternsSha256)));
            writer.WriteString("state_sha256", RequiredHash(receipt.StateSha256, nameof(receipt.StateSha256)));
            WriteStringArray(writer, "parent_receipt_sha256s", receipt.ParentReceiptSha256s.Select(value => RequiredHash(value, "parent_receipt_sha256")));
            writer.WriteEndObject();
        }

        return stream.ToArray();
    }

    public static string ComputeReceiptSha256(OrchestrationReasoningSourceAdmissionReceipt receipt)
        => HashBytes(SerializeReceiptCore(receipt));

    public static byte[] SerializeAdmissionProof(OrchestrationReasoningSourceAdmissionProof proof)
    {
        ArgumentNullException.ThrowIfNull(proof);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schema_version", proof.SchemaVersion);
            writer.WritePropertyName("receipt");
            WriteReceipt(writer, proof.Receipt);
            writer.WritePropertyName("source_materials");
            writer.WriteStartArray();
            foreach (var source in proof.SourceMaterials
                .OrderBy(item => item.PayloadRef, StringComparer.Ordinal))
            {
                writer.WriteStartObject();
                writer.WriteString("payload_ref", NormalizeRequired(source.PayloadRef, nameof(source.PayloadRef)));
                writer.WriteString("manifest_ref", NormalizeRequired(source.ManifestRef, nameof(source.ManifestRef)));
                writer.WriteString("ledger_ref", NormalizeRequired(source.LedgerRef, nameof(source.LedgerRef)));
                writer.WriteString("evidence_ref", NormalizeRequired(source.EvidenceRef, nameof(source.EvidenceRef)));
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return stream.ToArray();
    }

    public static string HashBytes(ReadOnlySpan<byte> bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string NormalizeString(string value)
        => (value ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Normalize(NormalizationForm.FormC);

    private static OrchestrationReasoningLearnedPattern NormalizePattern(
        OrchestrationReasoningLearnedPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        if (!double.IsFinite(pattern.Confidence) || pattern.Confidence is < 0.0 or > 1.0)
            throw new InvalidDataException("Learned pattern confidence must be finite and in [0,1].");

        return pattern with
        {
            PatternId = NormalizeRequired(pattern.PatternId, nameof(pattern.PatternId)),
            ScenarioRef = NormalizeRequired(pattern.ScenarioRef, nameof(pattern.ScenarioRef)),
            RequiredTaskTypes = NormalizeSet(pattern.RequiredTaskTypes),
            RequiredConstraintSignals = NormalizeSet(pattern.RequiredConstraintSignals),
            RequiredRequirementSignals = NormalizeSet(pattern.RequiredRequirementSignals),
            RequiredDeliverableSignals = NormalizeSet(pattern.RequiredDeliverableSignals),
            RequiredEvidenceSignals = NormalizeSet(pattern.RequiredEvidenceSignals),
            PhaseFamilies = NormalizeSet(pattern.PhaseFamilies),
            GateFamilies = NormalizeSet(pattern.GateFamilies),
            AssetFamilies = NormalizeSet(pattern.AssetFamilies),
            HostCheckSets = NormalizeSet(pattern.HostCheckSets),
            RiskFlags = NormalizeSet(pattern.RiskFlags),
            NextGranularizationActions = NormalizeSet(pattern.NextGranularizationActions)
        };
    }

    private static IReadOnlyList<string> NormalizeSet(IEnumerable<string>? values)
        => (values ?? throw new InvalidDataException("Canonical set-valued collection is required."))
            .Select(NormalizeString)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string NormalizeRequired(string? value, string name)
    {
        var normalized = NormalizeString(value ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidDataException($"{name} is required.");
        return normalized;
    }

    private static string RequiredHash(string? value, string name)
    {
        var normalized = NormalizeRequired(value, name).ToLowerInvariant();
        if (normalized.Length != 64 || normalized.Any(ch => !Uri.IsHexDigit(ch)))
            throw new InvalidDataException($"{name} must be a lowercase SHA-256 hex value.");
        return normalized;
    }

    private static void WritePattern(Utf8JsonWriter writer, OrchestrationReasoningLearnedPattern pattern)
    {
        writer.WriteStartObject();
        writer.WriteString("pattern_id", pattern.PatternId);
        writer.WriteString("scenario_ref", pattern.ScenarioRef);
        WriteStringArray(writer, "required_task_types", pattern.RequiredTaskTypes);
        WriteStringArray(writer, "required_constraint_signals", pattern.RequiredConstraintSignals);
        WriteStringArray(writer, "required_requirement_signals", pattern.RequiredRequirementSignals);
        WriteStringArray(writer, "required_deliverable_signals", pattern.RequiredDeliverableSignals);
        WriteStringArray(writer, "required_evidence_signals", pattern.RequiredEvidenceSignals);
        WriteStringArray(writer, "phase_families", pattern.PhaseFamilies);
        WriteStringArray(writer, "gate_families", pattern.GateFamilies);
        WriteStringArray(writer, "asset_families", pattern.AssetFamilies);
        WriteStringArray(writer, "host_check_sets", pattern.HostCheckSets);
        WriteStringArray(writer, "risk_flags", pattern.RiskFlags);
        WriteStringArray(writer, "next_granularization_actions", pattern.NextGranularizationActions);
        writer.WriteNumber("confidence", pattern.Confidence);
        writer.WriteEndObject();
    }

    private static void WriteReceipt(Utf8JsonWriter writer, OrchestrationReasoningSourceAdmissionReceipt receipt)
    {
        writer.WriteStartObject();
        writer.WriteNumber("schema_version", receipt.SchemaVersion);
        writer.WriteString("payload_sha256", receipt.PayloadSha256);
        writer.WriteString("manifest_sha256", receipt.ManifestSha256);
        writer.WriteString("ledger_sha256", receipt.LedgerSha256);
        writer.WriteString("evidence_sha256", receipt.EvidenceSha256);
        writer.WriteString("allowed_source_kind", receipt.AllowedSourceKind);
        WriteStringArray(writer, "row_ids", receipt.RowIds);
        WriteStringArray(writer, "source_ids", receipt.SourceIds);
        WriteStringArray(writer, "pair_ids", receipt.PairIds);
        WriteStringArray(writer, "source_refs", receipt.SourceRefs);
        WriteStringArray(writer, "source_sha256s", receipt.SourceSha256s);
        writer.WriteString("learned_patterns_sha256", receipt.LearnedPatternsSha256);
        writer.WriteString("state_sha256", receipt.StateSha256);
        WriteStringArray(writer, "parent_receipt_sha256s", receipt.ParentReceiptSha256s);
        writer.WriteString("receipt_sha256", receipt.ReceiptSha256);
        writer.WriteEndObject();
    }

    private static void WriteStringArray(
        Utf8JsonWriter writer,
        string propertyName,
        IEnumerable<string> values)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (var value in NormalizeSet(values))
            writer.WriteStringValue(value);
        writer.WriteEndArray();
    }
}
