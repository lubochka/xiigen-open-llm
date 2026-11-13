// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning; // Keep selection syntax in this assembly without transferring runtime owner authority.

using System.Security.Cryptography; // Hash frozen identity and synthetic acknowledgement preimages with SHA256.
using System.Text; // Encode the exact UTF8 domain preimages and canonical JSON text.
using System.Text.Json; // Generate and inspect closed JSON field sets without importing runtime capabilities.
using System.Globalization; // Preserve canonical decimal and binary32 settings independently of process culture.
using AgentInfrastructure; // Import the dependency used by this closed selection syntax implementation.
using AgentInfrastructure.Logical; // Import the dependency used by this closed selection syntax implementation.

// Full-material records are copied syntax only. The typed Session lease, not a JSON
// marker or this readback, selects their parser and authenticates their recipients.
internal sealed record CodeDocumentationFullMaterialRequestV1(
    string Schema, string Origin, string RequestedModel, string RequestedReasoning,
    string InvocationId, string GenesisFrameSha256, string BindingSha256,
    string BindingCanonicalBase64, string PacketCanonicalBase64, int PacketByteLength,
    string PacketSha256, string OriginalReceiptId, string OriginalReceiptSha256,
    string SourceSha256, string FrozenContextSha256, string[] Categories);

internal sealed record CodeDocumentationFullMaterialPrefixV1(
    int Frames, string OriginalReceiptId, string OriginalReceiptSha256,
    string GenesisSha256, string? StartedSha256, string? RequestSha256,
    CodeDocumentationFullMaterialRequestV1? Request,
    CodeDocumentationTeachingSelectionPayloadV1? Choice);

internal static partial class CodeDocumentationOneLessonV2Wire
{
    internal const int FullMaterialPacketMaximumBytes = 16384; // Preserve the decoded whole-material allowance independently of its Base64 transport.
    internal const int FullMaterialRequestMaximumBytes = 25731; // Base64(packet16384)=21848 + Base64(binding2120)=2828 + closed request envelope1055.
    internal const int FullMaterialReasonMaximumBytes = 1024;
    internal const int FullMaterialGenesisMaximumBytes = 4096;
    internal const int FullMaterialStartedMaximumBytes = 8192;
    internal const int FullMaterialCompletedMaximumBytes = 45076; // Base64(request25731)=34308 + max choice7713 + binding2120 + remaining envelope/LF935.
    internal const int FullMaterialTotalMaximumBytes = 57364; // Exactly the three LF-inclusive slots: 4096 + 8192 + 45076.
    private const string FullMaterialFamily = "code-documentation-full-material-selection-v1";
    internal static Action? FullMaterialParsePointForTesting { get; set; } // Observe actual entry after all physical fences, not a fabricated decoder result.
    internal static Action? FullMaterialRequestParsePointForTesting { get; set; } // Observe inner JSON only after its distinct transport byte/Base64/hash admission.
    internal static string FullMaterialFrameSha256(ReadOnlySpan<byte> line)
    {
        if (line.Length < 2 || line[^1] != (byte)'\n') throw new JsonException("Expected one LF-inclusive canonical frame.");
        return SelectedStudentBytesSha256(line[..^1]); // Chain canonical JSON bytes; only the capacity charge includes the physical LF.
    }
    internal static bool FullMaterialPhysicalCapacityWithin(IReadOnlyList<byte[]> lines)
    {
        if (lines.Count is < 1 or > 3) return false;
        var maximum = new[] { FullMaterialGenesisMaximumBytes, FullMaterialStartedMaximumBytes, FullMaterialCompletedMaximumBytes }; long total = 0;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i]; if (line.Length is < 2 || line[^1] != (byte)'\n' || line.Length > maximum[i]) return false;
            total = checked(total + line.LongLength);
        }
        return total <= FullMaterialTotalMaximumBytes; // This byte fence is not canonical syntax, recipient rights or tokenizer admission.
    }

    internal static byte[] FullMaterialRequestBytes(CodeDocumentationFullMaterialRequestV1 request)
    {
        var bytes = SelectionGeneratedBytes(JsonSerializer.SerializeToElement(request));
        if (!FullMaterialRequestByteCapacityWithin(bytes) || !ValidFullMaterialRequest(request))
            throw new JsonException("Full selection request exceeds its closed supported family.");
        return bytes;
    }
    internal static bool FullMaterialRequestByteCapacityWithin(ReadOnlySpan<byte> bytes)
        => bytes.Length is > 0 and <= FullMaterialRequestMaximumBytes; // A byte-only subcheck; full generated/decoded requests still require exact closed canonical grammar.

    private static bool ExactBase64(string text, int maximum, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        if (text.Length > ((maximum + 2) / 3) * 4) return false; // Bound allocation before decoding untrusted text.
        bytes = Convert.FromBase64String(text);
        return bytes.Length <= maximum && Convert.ToBase64String(bytes) == text;
    }

    private static bool ValidFullMaterialRequest(CodeDocumentationFullMaterialRequestV1 r)
        => r.Schema == FullMaterialFamily && r.Origin == "SyntheticTest" &&
            r.RequestedModel == "gpt-6-astra" && r.RequestedReasoning == "ultra" && SelectionToken(r.InvocationId) &&
            Hash(r.OriginalReceiptId, 32) && new[] { r.GenesisFrameSha256, r.BindingSha256, r.PacketSha256,
                r.OriginalReceiptSha256, r.SourceSha256, r.FrozenContextSha256 }.All(x => Hash(x)) &&
            SelectionCategories(r.Categories) && ExactBase64(r.BindingCanonicalBase64, 2120, out var binding) &&
            binding.Length > 0 && ExactBase64(r.PacketCanonicalBase64, FullMaterialPacketMaximumBytes, out var packet) &&
            packet.Length == r.PacketByteLength && packet.Length > 0 && SelectedStudentBytesSha256(packet) == r.PacketSha256;

    internal static string FullMaterialSettingsSha256(byte[] request)
        => SelectedStudentBytesSha256(Utf8.GetBytes("S04-full-material-SyntheticTest-gpt-6-astra-ultra\n").Concat(request).ToArray());

    internal static string FullMaterialAcknowledgment(byte[] request, string started, CodeDocumentationTeachingSelectionPayloadV1 choice)
        => SelectedStudentBytesSha256(Utf8.GetBytes("S04-full-material-issued-choice-v1\n").Concat(request)
            .Concat(Utf8.GetBytes(started)).Concat(SelectionPayloadBytesWithoutAck(choice)).ToArray());

    private static byte[] SelectionPayloadBytesWithoutAck(CodeDocumentationTeachingSelectionPayloadV1 p)
        => SelectionGeneratedBytes(JsonSerializer.SerializeToElement(new { p.ItemId, p.SourceId, p.ReferenceEntryId,
            p.SourceEntityFingerprint, p.Reason, p.MaterialAdmissionEvidenceSha256, p.InvocationId, p.LaunchSettingsSha256 }));

    private static object FullMaterialZeroEffects() => new { BaselineDispatched = "0", BaselineCompleted = "0",
        TeachingDispatched = "0", TeachingCompleted = "0", EvaluatorDispatched = "0", EvaluatorCompleted = "0",
        OptimizerEntries = "0", EffectOutcomeUnknown = false };

    private static byte[] FullMaterialFrame(CodeDocumentationOneLessonBindingProjectionV2 binding, string session,
        int revision, string previous, object payload, int maximum)
    {
        var value = JsonSerializer.SerializeToElement(new { Kind = revision == 0 ? "Genesis" : revision == 1 ?
            "TeachingSelectionStarted" : "TeachingSelectionCompleted", FormatVersion = 2, SessionId = session,
            BindingV2 = binding, BindingSha256 = SelectedStudentBindingSha256(binding), Revision = revision,
            PreviousFrameSha256 = previous, Effects = FullMaterialZeroEffects(), FullMaterialSelection = payload });
        var bytes = SelectionGeneratedBytes(value).Append((byte)'\n').ToArray();
        if (bytes.Length > maximum) throw new JsonException("Full selection frame exceeds its fixed LF-inclusive slot.");
        return bytes;
    }

    internal static byte[] FullMaterialGenesisBytes(CodeDocumentationOneLessonBindingProjectionV2 binding, string session,
        string originalId, string originalSha256)
    {
        if (!Hash(originalId, 32) || !Hash(originalSha256)) throw new JsonException("Invalid saved ORIGINAL expectation.");
        return FullMaterialFrame(binding, session, 0, new string('0', 64), new { Schema = FullMaterialFamily,
            Origin = "SyntheticTest", OriginalReceiptId = originalId, OriginalReceiptSha256 = originalSha256 }, FullMaterialGenesisMaximumBytes);
    }

    internal static byte[] FullMaterialStartedBytes(CodeDocumentationOneLessonBindingProjectionV2 binding, string session,
        CodeDocumentationFullMaterialRequestV1 request)
    {
        var bytes = FullMaterialRequestBytes(request);
        return FullMaterialFrame(binding, session, 1, request.GenesisFrameSha256, new { Schema = FullMaterialFamily,
            Origin = "SyntheticTest", request.InvocationId, RequestSha256 = SelectedStudentBytesSha256(bytes),
            RequestByteLength = bytes.Length, request.PacketSha256, LaunchSettingsSha256 = FullMaterialSettingsSha256(bytes),
            ReservedInvocations = 1, CompletedInvocations = 0, OutcomeUnknown = true }, FullMaterialStartedMaximumBytes);
    }

    internal static byte[] FullMaterialCompletedBytes(CodeDocumentationOneLessonBindingProjectionV2 binding, string session,
        CodeDocumentationFullMaterialRequestV1 request, string started, CodeDocumentationTeachingSelectionPayloadV1 choice)
    {
        var bytes = FullMaterialRequestBytes(request);
        if (!ValidUtf8Text(choice.Reason, FullMaterialReasonMaximumBytes) || choice.InvocationId != request.InvocationId ||
            choice.LaunchSettingsSha256 != FullMaterialSettingsSha256(bytes) || choice.LaunchAckSha256 != FullMaterialAcknowledgment(bytes, started, choice))
            throw new JsonException("Full selection choice lineage is invalid.");
        return FullMaterialFrame(binding, session, 2, started, new { Schema = FullMaterialFamily, Origin = "SyntheticTest",
            RequestCanonicalBase64 = Convert.ToBase64String(bytes), RequestSha256 = SelectedStudentBytesSha256(bytes),
            StartedFrameSha256 = started, Choice = SelectionPayloadElement(choice), ReservedInvocations = 1,
            CompletedInvocations = 1, OutcomeUnknown = false }, FullMaterialCompletedMaximumBytes);
    }

    private static CodeDocumentationTeachingSelectionPayloadV1 ReadFullMaterialChoice(JsonElement p)
    {
        if (!ExactFields(p, "Schema", "PolicyDocument", "OriginalReceiptId", "OriginalReceiptSha256", "SourceSha256", "Categories",
            "ItemId", "SourceId", "ReferenceEntryId", "SourceEntityFingerprint", "Reason", "FrozenContextSha256",
            "MaterialAdmissionEvidenceSha256", "Origin", "InvocationId", "LaunchSettingsSha256", "LaunchAckSha256",
            "RequestedModel", "RequestedReasoning", "ProviderTelemetry") || p.GetProperty("Schema").GetString() != CodeDocumentationTeachingSelectionPayloadV1.SchemaValue ||
            p.GetProperty("PolicyDocument").GetString() != CodeDocumentationTeachingSelectionPayloadV1.PolicyValue || p.GetProperty("Origin").GetString() != "SyntheticTest" ||
            p.GetProperty("RequestedModel").GetString() != "gpt-6-astra" || p.GetProperty("RequestedReasoning").GetString() != "ultra" ||
            p.GetProperty("ProviderTelemetry").GetString() != "UNKNOWN") throw new JsonException("Invalid full selection choice fields.");
        var value = CodeDocumentationTeachingSelectionPayloadV1.Capture(p.GetProperty("OriginalReceiptId").GetString()!,
            p.GetProperty("OriginalReceiptSha256").GetString()!, p.GetProperty("SourceSha256").GetString()!,
            p.GetProperty("Categories").EnumerateArray().Select(x => x.GetString()!).ToArray(), p.GetProperty("ItemId").GetString()!,
            p.GetProperty("SourceId").GetString()!, p.GetProperty("ReferenceEntryId").GetString()!, p.GetProperty("SourceEntityFingerprint").GetString()!,
            p.GetProperty("Reason").GetString()!, p.GetProperty("FrozenContextSha256").GetString()!, p.GetProperty("MaterialAdmissionEvidenceSha256").GetString()!,
            p.GetProperty("InvocationId").GetString()!, p.GetProperty("LaunchSettingsSha256").GetString()!, p.GetProperty("LaunchAckSha256").GetString()!);
        return value.IsSuccess && value.Value is not null ? value.Value : throw new JsonException("Invalid choice syntax.");
    }

    internal static OperationResult<CodeDocumentationFullMaterialPrefixV1> DecodeFullMaterialPrefix(byte[] bytes,
        CodeDocumentationOneLessonBindingProjectionV2 binding, string session, bool allowIncomplete)
    {
        try
        {
            if (bytes.Length == 0) return allowIncomplete ? OperationResult<CodeDocumentationFullMaterialPrefixV1>.Ok(
                new(0, "", "", "", null, null, null, null)) : SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>();
            if (bytes.Length > FullMaterialTotalMaximumBytes || bytes[^1] != (byte)'\n' || bytes.Contains((byte)'\r')) return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>();
            var lines = new List<byte[]>(); var offset = 0;
            for (var i = 0; i < bytes.Length; i++) if (bytes[i] == (byte)'\n') { lines.Add(bytes.AsSpan(offset, i - offset + 1).ToArray()); offset = i + 1; }
            if (!FullMaterialPhysicalCapacityWithin(lines) || (!allowIncomplete && lines.Count != 3)) return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>();
            FullMaterialParsePointForTesting?.Invoke(); // Oversized physical input cannot reach UTF8/JSON allocation below.
            var limits = new[] { FullMaterialGenesisMaximumBytes, FullMaterialStartedMaximumBytes, FullMaterialCompletedMaximumBytes };
            JsonElement[] roots = new JsonElement[lines.Count];
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length > limits[i]) return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>(); // Bound every actual line before JSON parsing.
                _ = Utf8.GetString(lines[i]); using var doc = JsonDocument.Parse(lines[i].AsMemory(0, lines[i].Length - 1), new JsonDocumentOptions { MaxDepth = 8 });
                var root = roots[i] = doc.RootElement.Clone();
                if (!ExactFields(root, "Kind", "FormatVersion", "SessionId", "BindingV2", "BindingSha256", "Revision", "PreviousFrameSha256", "Effects", "FullMaterialSelection") ||
                    root.GetProperty("Kind").GetString() != (i == 0 ? "Genesis" : i == 1 ? "TeachingSelectionStarted" : "TeachingSelectionCompleted") ||
                    root.GetProperty("Revision").GetRawText() != i.ToString(CultureInfo.InvariantCulture) || root.GetProperty("FormatVersion").GetRawText() != "2" ||
                    root.GetProperty("SessionId").GetString() != session || root.GetProperty("BindingSha256").GetString() != SelectedStudentBindingSha256(binding) ||
                    !CanonicalBytes(root.GetProperty("BindingV2")).AsSpan().SequenceEqual(CanonicalBytes(JsonSerializer.SerializeToElement(binding))) ||
                    !CanonicalBytes(root.GetProperty("Effects")).AsSpan().SequenceEqual(CanonicalBytes(JsonSerializer.SerializeToElement(FullMaterialZeroEffects()))) ||
                    root.GetProperty("PreviousFrameSha256").GetString() != (i == 0 ? new string('0', 64) : FullMaterialFrameSha256(lines[i - 1])) ||
                    !SelectionGeneratedBytes(root).Append((byte)'\n').ToArray().AsSpan().SequenceEqual(lines[i])) return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>();
            }
            var g = roots[0].GetProperty("FullMaterialSelection");
            if (!ExactFields(g, "Schema", "Origin", "OriginalReceiptId", "OriginalReceiptSha256") || g.GetProperty("Schema").GetString() != FullMaterialFamily ||
                g.GetProperty("Origin").GetString() != "SyntheticTest") return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>();
            var id = g.GetProperty("OriginalReceiptId").GetString()!; var original = g.GetProperty("OriginalReceiptSha256").GetString()!;
            if (!lines[0].AsSpan().SequenceEqual(FullMaterialGenesisBytes(binding, session, id, original))) return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>();
            var genesis = FullMaterialFrameSha256(lines[0]);
            if (lines.Count == 1) return OperationResult<CodeDocumentationFullMaterialPrefixV1>.Ok(new(1, id, original, genesis, null, null, null, null));
            var s = roots[1].GetProperty("FullMaterialSelection");
            if (!ExactFields(s, "Schema", "Origin", "InvocationId", "RequestSha256", "RequestByteLength", "PacketSha256", "LaunchSettingsSha256", "ReservedInvocations", "CompletedInvocations", "OutcomeUnknown") ||
                s.GetProperty("Schema").GetString() != FullMaterialFamily || s.GetProperty("Origin").GetString() != "SyntheticTest" ||
                !SelectionToken(s.GetProperty("InvocationId").GetString()) || !new[] { "RequestSha256", "PacketSha256", "LaunchSettingsSha256" }.All(x => Hash(s.GetProperty(x).GetString())) ||
                s.GetProperty("RequestByteLength").GetInt32() is < 1 or > FullMaterialRequestMaximumBytes || s.GetProperty("ReservedInvocations").GetRawText() != "1" ||
                s.GetProperty("CompletedInvocations").GetRawText() != "0" || s.GetProperty("OutcomeUnknown").ValueKind != JsonValueKind.True) return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>();
            var started = FullMaterialFrameSha256(lines[1]); var requestHash = s.GetProperty("RequestSha256").GetString()!;
            if (lines.Count == 2) return OperationResult<CodeDocumentationFullMaterialPrefixV1>.Ok(new(2, id, original, genesis, started, requestHash, null, null));
            var c = roots[2].GetProperty("FullMaterialSelection");
            if (!ExactFields(c, "Schema", "Origin", "RequestCanonicalBase64", "RequestSha256", "StartedFrameSha256", "Choice", "ReservedInvocations", "CompletedInvocations", "OutcomeUnknown") ||
                c.GetProperty("Schema").GetString() != FullMaterialFamily || c.GetProperty("Origin").GetString() != "SyntheticTest" || c.GetProperty("RequestSha256").GetString() != requestHash ||
                c.GetProperty("StartedFrameSha256").GetString() != started || c.GetProperty("ReservedInvocations").GetRawText() != "1" || c.GetProperty("CompletedInvocations").GetRawText() != "1" ||
                c.GetProperty("OutcomeUnknown").ValueKind != JsonValueKind.False || !ExactBase64(c.GetProperty("RequestCanonicalBase64").GetString()!, FullMaterialRequestMaximumBytes, out var requestBytes) ||
                SelectedStudentBytesSha256(requestBytes) != requestHash) return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>();
            FullMaterialRequestParsePointForTesting?.Invoke();
            using var requestDoc = JsonDocument.Parse(requestBytes, new JsonDocumentOptions { MaxDepth = 4 }); var r = requestDoc.RootElement;
            if (!ExactFields(r, "Schema", "Origin", "RequestedModel", "RequestedReasoning", "InvocationId", "GenesisFrameSha256", "BindingSha256", "BindingCanonicalBase64", "PacketCanonicalBase64", "PacketByteLength",
                "PacketSha256", "OriginalReceiptId", "OriginalReceiptSha256", "SourceSha256", "FrozenContextSha256", "Categories")) return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>();
            var request = JsonSerializer.Deserialize<CodeDocumentationFullMaterialRequestV1>(requestBytes)!; var choice = ReadFullMaterialChoice(c.GetProperty("Choice"));
            if (!requestBytes.AsSpan().SequenceEqual(FullMaterialRequestBytes(request)) || request.OriginalReceiptId != id || request.OriginalReceiptSha256 != original ||
                request.GenesisFrameSha256 != genesis || request.BindingSha256 != SelectedStudentBindingSha256(binding) ||
                !Convert.FromBase64String(request.BindingCanonicalBase64).AsSpan().SequenceEqual(CanonicalBytes(JsonSerializer.SerializeToElement(binding))) ||
                choice.OriginalReceiptId != id || choice.OriginalReceiptSha256 != original || choice.SourceSha256 != request.SourceSha256 || choice.FrozenContextSha256 != request.FrozenContextSha256 ||
                !choice.Categories.SequenceEqual(request.Categories, StringComparer.Ordinal) || !lines[1].AsSpan().SequenceEqual(FullMaterialStartedBytes(binding, session, request)) ||
                !lines[2].AsSpan().SequenceEqual(FullMaterialCompletedBytes(binding, session, request, started, choice))) return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>();
            return OperationResult<CodeDocumentationFullMaterialPrefixV1>.Ok(new(3, id, original, genesis, started, requestHash, request, choice));
        }
        catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { return SelectionInvalid<CodeDocumentationFullMaterialPrefixV1>(); }
    }
}

// These lower-assembly values describe copied syntax. They never carry a Session,
// corpus, client, issuer or material/effect capability.
internal sealed record CodeDocumentationOneLessonBindingProjectionV2( // Project immutable grant scalars and descriptors without retaining an owner.
    string ObjectiveId, string GrantId, string GrantVersion, string TrainingRunId, // Project immutable grant scalars and descriptors without retaining an owner.
    string CandidateSha256, string ParentManifestSha256, string ParentWeightsSha256, // Project immutable grant scalars and descriptors without retaining an owner.
    string ConfigurationSha256, string ExamConditionSha256, // Project immutable grant scalars and descriptors without retaining an owner.
    CodeDocumentationOneLessonDescriptorsV2 DescriptorsV2); // Project immutable grant scalars and descriptors without retaining an owner.

internal sealed class CodeDocumentationTeachingSelectionInputV1 // Own detached frozen input bytes and category order for the single invocation.
{ // Begin this scope: own detached frozen input bytes and category order for the single invocation.
    private readonly byte[] bytes; // Retain a private copy so callbacks cannot mutate the frozen input preimage.
    internal string OriginalReceiptId { get; } // Expose the detached ORIGINAL receipt identifier, without authenticating storage.
    internal string OriginalReceiptSha256 { get; } // Expose the detached ORIGINAL raw-byte commitment.
    internal string SourceSha256 { get; } // Expose the full SOURCE snapshot commitment for later held-owner comparison.
    internal IReadOnlyList<string> Categories { get; } // Expose the copied diagnostic order without permitting collection mutation.
    internal string FrozenContextSha256 { get; } // Commit the complete copied invocation context under its distinct hash domain.
    internal CodeDocumentationTeachingSelectionInputV1(string id, string hash, string source, // Own detached frozen input bytes and category order for the single invocation.
        string[] categories, byte[] canonicalBytes) // Own detached frozen input bytes and category order for the single invocation.
    { // Begin this scope: own detached frozen input bytes and category order for the single invocation.
        OriginalReceiptId = id; OriginalReceiptSha256 = hash; SourceSha256 = source; // Own detached frozen input bytes and category order for the single invocation.
        Categories = Array.AsReadOnly(categories.ToArray()); bytes = canonicalBytes.ToArray(); // Detach the ordered categories and canonical bytes from caller-owned arrays.
        FrozenContextSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes( // Own detached frozen input bytes and category order for the single invocation.
            "code-documentation-selection-binding-input-v1\n").Concat(bytes).ToArray())).ToLowerInvariant(); // Own detached frozen input bytes and category order for the single invocation.
    } // Close this scope: own detached frozen input bytes and category order for the single invocation.
    internal byte[] CopyBytes() => bytes.ToArray(); // Return a new byte array on every read to preserve the private frozen preimage.
} // Close this scope: own detached frozen input bytes and category order for the single invocation.

internal sealed class CodeDocumentationTeachingSelectionPayloadV1 // Capture bounded scalar choice syntax only after validating every identity and reason.
{ // Begin this scope: capture bounded scalar choice syntax only after validating every identity and reason.
    internal const string SchemaValue = "code-documentation-teaching-selection-binding-v1"; // Capture bounded scalar choice syntax only after validating every identity and reason.
    internal const string PolicyValue = "S04-CODEX-TEACHING-SELECTION-v1"; // Capture bounded scalar choice syntax only after validating every identity and reason.
    internal string OriginalReceiptId { get; } // Expose the detached ORIGINAL receipt identifier, without authenticating storage.
    internal string OriginalReceiptSha256 { get; } // Expose the detached ORIGINAL raw-byte commitment.
    internal string SourceSha256 { get; } // Expose the full SOURCE snapshot commitment for later held-owner comparison.
    internal IReadOnlyList<string> Categories { get; } // Expose the copied diagnostic order without permitting collection mutation.
    internal string ItemId { get; } // Carry the chosen declared item identity; the Session still authenticates the join.
    internal string SourceId { get; } // Carry the selected item source namespace without retaining a corpus owner.
    internal string ReferenceEntryId { get; } // Carry the canonical selected entry identity for later genuine graph validation.
    internal string SourceEntityFingerprint { get; } // Carry the exact selected entity fingerprint for later graph comparison.
    internal string Reason { get; } // Carry a bounded nonblank explanation that still requires ORIGINAL disclosure checks.
    internal string FrozenContextSha256 { get; } // Commit the complete copied invocation context under its distinct hash domain.
    internal string MaterialAdmissionEvidenceSha256 { get; } // Carry only an evidence digest; this scalar grants no material admission.
    internal string InvocationId { get; } // Carry the concrete issuer invocation identifier for this one dispatch.
    internal string LaunchSettingsSha256 { get; } // Carry the synthetic requested-settings commitment without claiming provider telemetry.
    internal string LaunchAckSha256 { get; } // Carry the synthetic acknowledgement commitment minted by the concrete issuer.
    private CodeDocumentationTeachingSelectionPayloadV1(string id, string originalHash, string sourceHash, // Capture bounded scalar choice syntax only after validating every identity and reason.
        string[] categories, string item, string source, string entry, string fingerprint, string reason, // Capture bounded scalar choice syntax only after validating every identity and reason.
        string frozen, string material, string invocation, string settings, string ack) // Capture bounded scalar choice syntax only after validating every identity and reason.
    { // Begin this scope: capture bounded scalar choice syntax only after validating every identity and reason.
        OriginalReceiptId=id; OriginalReceiptSha256=originalHash; SourceSha256=sourceHash; // Capture bounded scalar choice syntax only after validating every identity and reason.
        Categories=Array.AsReadOnly(categories.ToArray()); ItemId=item; SourceId=source; // Capture bounded scalar choice syntax only after validating every identity and reason.
        ReferenceEntryId=entry; SourceEntityFingerprint=fingerprint; Reason=reason; // Capture bounded scalar choice syntax only after validating every identity and reason.
        FrozenContextSha256=frozen; MaterialAdmissionEvidenceSha256=material; InvocationId=invocation; // Capture bounded scalar choice syntax only after validating every identity and reason.
        LaunchSettingsSha256=settings; LaunchAckSha256=ack; // Capture bounded scalar choice syntax only after validating every identity and reason.
    } // Close this scope: capture bounded scalar choice syntax only after validating every identity and reason.
    internal static OperationResult<CodeDocumentationTeachingSelectionPayloadV1> Capture( // Capture bounded scalar choice syntax only after validating every identity and reason.
        string id, string originalHash, string sourceHash, IReadOnlyList<string> categories, // Capture bounded scalar choice syntax only after validating every identity and reason.
        string item, string source, string entry, string fingerprint, string reason, // Capture bounded scalar choice syntax only after validating every identity and reason.
        string frozen, string material, string invocation, string settings, string ack) // Capture bounded scalar choice syntax only after validating every identity and reason.
    { // Begin this scope: capture bounded scalar choice syntax only after validating every identity and reason.
        var copied = categories?.ToArray(); // Copy supplied categories before validating their finite ordered grammar.
        if (!CodeDocumentationOneLessonV2Wire.Hash(id,32) || // Capture bounded scalar choice syntax only after validating every identity and reason.
            !new[]{originalHash,sourceHash,frozen,material,settings,ack}.All(x => CodeDocumentationOneLessonV2Wire.Hash(x)) || // Capture bounded scalar choice syntax only after validating every identity and reason.
            !CodeDocumentationOneLessonV2Wire.SelectionCategories(copied) || // Reject duplicates, empty order and unsupported diagnostic codes.
            !CodeDocumentationOneLessonV2Wire.SelectionToken(item) || !CodeDocumentationOneLessonV2Wire.SelectionToken(source) || // Capture bounded scalar choice syntax only after validating every identity and reason.
            !CodeDocumentationOneLessonV2Wire.SelectionToken(invocation) || // Capture bounded scalar choice syntax only after validating every identity and reason.
            !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(entry) || // Require an exact canonical reference-entry identity without normalizing syntax.
            !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(fingerprint) || // Require an exact canonical entity fingerprint without accepting aliases.
            string.IsNullOrWhiteSpace(reason) || !CodeDocumentationOneLessonV2Wire.ValidUtf8Text(reason,4096)) // Reject empty explanations and enforce the admitted UTF8 reason ceiling.
            return CodeDocumentationOneLessonV2Wire.SelectionInvalid<CodeDocumentationTeachingSelectionPayloadV1>(); // Capture bounded scalar choice syntax only after validating every identity and reason.
        return OperationResult<CodeDocumentationTeachingSelectionPayloadV1>.Ok(new(id,originalHash,sourceHash, // Capture bounded scalar choice syntax only after validating every identity and reason.
            copied!,item,source,entry,fingerprint,reason,frozen,material,invocation,settings,ack)); // Capture bounded scalar choice syntax only after validating every identity and reason.
    } // Close this scope: capture bounded scalar choice syntax only after validating every identity and reason.
} // Close this scope: capture bounded scalar choice syntax only after validating every identity and reason.

internal sealed record CodeDocumentationTeachingSelectionFrameV1(string SessionId, // Describe decoded syntax and its exact Genesis link without granting replay authority.
    string BindingSha256, string GenesisFrameSha256, CodeDocumentationTeachingSelectionPayloadV1 Payload); // Describe decoded syntax and its exact Genesis link without granting replay authority.

internal sealed record CodeDocumentationSelectedStudentSettingsV1( // Describe all seven frozen Student settings without carrying an inference permit.
    string MaxNewTokens, string Temperature, string TopK, string TopP, // Preserve exact numeric limits and binary32 bits through string-only canonical syntax.
    string Seed, string MaxHeldOutCases, string MaxAnswerChars); // Keep the signed seed and both policy caps in the same commitment.

internal sealed record CodeDocumentationSelectedStudentRequestV1( // Commit the complete task and current owner identities, never a caller-owned response.
    string Schema, string Origin, string LeaseSha256, string BindingSha256, // Separate the Synthetic capacity description from genuine Session effect authority.
    string SelectionFrameSha256, string OriginalReceiptId, string OriginalReceiptSha256, // Bind the exact saved choice and issued ORIGINAL expectation.
    string SourceSha256, string FrozenContextSha256, string MaterialAdmissionEvidenceSha256, // Retain whole SOURCE, operator context and opaque material eligibility commitments.
    string ItemId, string SourceId, string ReferenceEntryId, string SourceEntityFingerprint, // Preserve every endpoint of the genuine nonordinal selected join.
    string Task, string TaskUtf8ByteLength, string TaskSha256, // Carry full detached task bytes instead of a routing candidate or digest-only witness.
    string ParentManifestSha256, string ParentWeightsSha256, string ConfigurationSha256, // Pin the verified loaded parent and frozen model configuration.
    string RuntimeCompatibilitySha256, string TokenizerSha256, string ModelStateBeforeSha256, // Distinguish the manifest passport, actual BPE bytes and complete model/random state.
    CodeDocumentationSelectedStudentSettingsV1 GenerationSettings, string GenerationSettingsSha256); // Bind every actual frozen generation setting and both policy caps.

internal sealed record CodeDocumentationSelectedStudentFrameV1( // Return copied syntax only; this record cannot mint a held-owner result or permit.
    string SessionId, string BindingSha256, string PreviousFrameSha256, bool IsCompleted, // Preserve frame position and exact preceding canonical hash.
    CodeDocumentationSelectedStudentRequestV1? Request, string RequestSha256, // Started holds the full request; Completed must match its prior commitment.
    string? ModelStateAfterSha256, string? Response, string? ResponseSha256, // Preserve complete raw output and state comparison without trimming or normalization.
    int ResponseUtf8ByteLength, int ResponseCharCount); // Retain exact byte and UTF16 counts for recipient revalidation.

internal static partial class CodeDocumentationOneLessonV2Wire // Restrict categories and tokens to the admitted finite selection grammar.
{ // Begin this scope: restrict categories and tokens to the admitted finite selection grammar.
    internal const int SelectionFrameMaximumBytes = 32768; // Fix the LF-inclusive selection reservation independently of legacy frame ceilings.
    internal static bool SelectionToken(string? value) => value is {Length: >=1 and <=128} && // Restrict categories and tokens to the admitted finite selection grammar.
        char.IsAsciiLetterOrDigit(value[0]) && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or ':' or '-'); // Restrict categories and tokens to the admitted finite selection grammar.
    internal static bool SelectionCategories(IReadOnlyList<string>? values) // Restrict categories and tokens to the admitted finite selection grammar.
        => values is {Count: >=1 and <=4} && values.Distinct(StringComparer.Ordinal).Count()==values.Count && // Restrict categories and tokens to the admitted finite selection grammar.
        values.All(x => x is CodeDocumentationSavedModelAssessmentService.MissingEssentialBehaviorReasonCode // Restrict categories and tokens to the admitted finite selection grammar.
            or CodeDocumentationSavedModelAssessmentService.InventedBehaviorReasonCode // Restrict categories and tokens to the admitted finite selection grammar.
            or CodeDocumentationSavedModelAssessmentService.WrongPurposeOrResultReasonCode // Restrict categories and tokens to the admitted finite selection grammar.
            or CodeDocumentationSavedModelAssessmentService.UnclearOrUnverifiableReasonCode); // Restrict categories and tokens to the admitted finite selection grammar.
    internal static OperationResult<T> SelectionInvalid<T>() => OperationResult<T>.Fail( // Restrict categories and tokens to the admitted finite selection grammar.
        "The closed selection binding syntax is invalid.","ONE_LESSON_SELECTION_BINDING_INVALID"); // Restrict categories and tokens to the admitted finite selection grammar.

    internal static OperationResult<CodeDocumentationTeachingSelectionInputV1> CaptureTeachingSelectionInput( // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
        string originalReceiptId, string originalReceiptSha256, IReadOnlyList<string> orderedCategories, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
        CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback graph) // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
    { // Begin this scope: project the complete ordered teaching graph with every safe owner identity and no closed answers.
        var categories=orderedCategories?.ToArray(); // Copy the diagnostic order independently from the complete graph category order.
        if (!Hash(originalReceiptId,32) || !Hash(originalReceiptSha256) || !SelectionCategories(categories) || // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
            graph?.Selection.TeachingCurriculum is not {} curriculum) // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
            return SelectionInvalid<CodeDocumentationTeachingSelectionInputV1>(); // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
        // Generate every permitted field explicitly; no arbitrary JSON input, computed
        // property expansion, owner pointer, raw task or reference answer is retained.
        object TeachingSource(CodeDocumentationOneLessonTeachingSourceV2 s) => new { // Project all ten descriptor fields from the actual teaching owner.
            s.SourceId,s.CorpusId,s.DomainSlug,s.SubjectSlug,s.CorpusVersion,s.ManifestFileSha256, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
            s.ManifestFileByteLength,s.CorpusSha256,s.PayloadSha256,s.PayloadByteLength}; // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
        object Item(CodeDocumentationOneLessonTeachingItemV2 i) => new { // Project every declared item link without choosing or dropping items.
            i.ItemId,i.SourceId,i.ReferenceEntryId,i.SourceEntityFingerprint}; // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
        var value=JsonSerializer.SerializeToElement(new { // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
            Schema="code-documentation-selection-binding-input-v1", OriginalReceiptId=originalReceiptId, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
            OriginalReceiptSha256=originalReceiptSha256,SourceSha256=graph.Selection.SourceSelectionSha256, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
            Categories=categories,TeachingGraph=new { // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                Selection=new { // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                    Sources=graph.Selection.Sources.Select(s=>new {s.SourceId,s.Role,s.DomainSlug,s.DesignPatternSlug, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                        s.DatasetVersion,s.ManifestFileSha256,s.ManifestFileByteLength,s.SealedManifestSha256}).ToArray(), // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                    PairSelections=graph.Selection.PairSelections.Select(p=>new {p.TrainingPairId,p.SourceId}).ToArray(), // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                    TeachingCurriculum=new {curriculum.PolicyId,curriculum.PolicyVersion, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                        Sources=curriculum.Sources.Select(TeachingSource).ToArray(), // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                        Items=curriculum.Items.Select(Item).ToArray(), // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                        CategoryBindings=curriculum.CategoryBindings.Select(c=>new {c.Category,OrderedItemIds=c.OrderedItemIds.ToArray()}).ToArray()}}, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                Sources=graph.Sources.Select(s=>new {Descriptor=TeachingSource(s.Descriptor), // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                    Manifest=new {s.Manifest.Schema,s.Manifest.Context,s.Manifest.Domain,s.Manifest.Subject, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                        s.Manifest.CorpusVersion,s.Manifest.CorpusName,s.Manifest.FrozenAtUtc,s.Manifest.EntryCount, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                        s.Manifest.CorpusSha256,s.Manifest.RepositoryRelativeCorpusDirectory,s.Manifest.RepositoryRelativePayloadPath, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                        s.Manifest.PayloadByteLength,s.Manifest.PayloadSha256,s.Manifest.RepositoryRelativeManifestPath,s.Manifest.ManifestSha256}, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                    s.ManifestFileSha256,s.ManifestFileByteLength, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                    Entries=s.Entries.Select(e=>new {e.EntryId,e.SourceEntityFingerprint,e.DomainTag,e.SubjectId, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                        Partition=e.Partition.ToString(),e.OwnerInputSha256,PaidReferenceProvenance=new { // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                            e.PaidReferenceProvenance.ProviderRef,e.PaidReferenceProvenance.ModelId,e.PaidReferenceProvenance.ModelVersion, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                            e.PaidReferenceProvenance.RequestId,CapturedAtUtc=FrozenReferenceCanonicalization.FormatUtc(e.PaidReferenceProvenance.CapturedAtUtc), // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                            e.PaidReferenceProvenance.ReferenceAnswerSha256,e.PaidReferenceProvenance.CanonicalRepresentation,e.PaidReferenceProvenance.ProvenanceSha256}}).ToArray()}).ToArray(), // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                Items=graph.Items.Select(i=>new {DeclaredItem=Item(i.DeclaredItem),SourceId=i.Source.Descriptor.SourceId, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                    ReferenceEntryId=i.Entry.EntryId,i.Entry.SourceEntityFingerprint}).ToArray(), // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
                CategoryBindings=graph.CategoryBindings.Select(c=>new {c.Category,OrderedItemIds=c.OrderedItemIds.ToArray()}).ToArray()}}); // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
        return OperationResult<CodeDocumentationTeachingSelectionInputV1>.Ok(new(originalReceiptId,originalReceiptSha256, // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
            graph.Selection.SourceSelectionSha256,categories!,SelectionGeneratedBytes(value))); // Project the complete ordered teaching graph with every safe owner identity and no closed answers.
    } // Close this scope: project the complete ordered teaching graph with every safe owner identity and no closed answers.
    internal static byte[] CanonicalTeachingSelectionInputBytes(CodeDocumentationTeachingSelectionInputV1 input) => input.CopyBytes(); // Serialize only generated typed input or payload values through the dedicated array-enabled writer.
    private static byte[] SelectionGeneratedBytes(JsonElement generated) // Serialize only generated typed input or payload values through the dedicated array-enabled writer.
    { // Begin this scope: serialize only generated typed input or payload values through the dedicated array-enabled writer.
        var output=new StringBuilder(); Write(generated,output,selectionArrays:true); // Enable arrays only while writing this generated typed selection preimage.
        return Utf8.GetBytes(output.ToString()); // Return exact UTF8 canonical bytes without an added frame delimiter.
    } // Close this scope: serialize only generated typed input or payload values through the dedicated array-enabled writer.
    internal static byte[] CanonicalTeachingSelectionPayloadBytes(CodeDocumentationTeachingSelectionPayloadV1 p) // Serialize only generated typed input or payload values through the dedicated array-enabled writer.
        => SelectionGeneratedBytes(SelectionPayloadElement(p)); // Serialize only generated typed input or payload values through the dedicated array-enabled writer.
    private static JsonElement SelectionPayloadElement(CodeDocumentationTeachingSelectionPayloadV1 p) => JsonSerializer.SerializeToElement(new { // Serialize only generated typed input or payload values through the dedicated array-enabled writer.
        Schema=CodeDocumentationTeachingSelectionPayloadV1.SchemaValue,PolicyDocument=CodeDocumentationTeachingSelectionPayloadV1.PolicyValue, // Serialize only generated typed input or payload values through the dedicated array-enabled writer.
        p.OriginalReceiptId,p.OriginalReceiptSha256,p.SourceSha256,Categories=p.Categories.ToArray(),p.ItemId,p.SourceId, // Serialize only generated typed input or payload values through the dedicated array-enabled writer.
        p.ReferenceEntryId,p.SourceEntityFingerprint,p.Reason,p.FrozenContextSha256,p.MaterialAdmissionEvidenceSha256, // Serialize only generated typed input or payload values through the dedicated array-enabled writer.
        Origin="SyntheticTest",p.InvocationId,p.LaunchSettingsSha256,p.LaunchAckSha256, // Serialize only generated typed input or payload values through the dedicated array-enabled writer.
        RequestedModel="gpt-6-astra",RequestedReasoning="ultra",ProviderTelemetry="UNKNOWN"}); // Record requested roles and UNKNOWN telemetry with explicit SyntheticTest origin.
    internal static byte[] CanonicalTeachingSelectionFrameBytes(CodeDocumentationOneLessonBindingProjectionV2 binding, // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
        string sessionId,string bindingSha256,string genesisFrameSha256,CodeDocumentationTeachingSelectionPayloadV1 payload) // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
    { // Begin this scope: write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
        bool Text(string text,int limit)=>!string.IsNullOrWhiteSpace(text) && text.Length<=limit && text==text.Trim() && // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
            !text.Any(char.IsControl) && ValidUtf8Text(text,checked(4*limit)); // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
        if(binding is null || payload is null || !Text(binding.ObjectiveId,128) || !Text(binding.GrantId,128) || // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
            !Text(binding.GrantVersion,64) || !Text(binding.TrainingRunId,128) || !SelectionToken(binding.TrainingRunId) || // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
            !new[]{binding.CandidateSha256,binding.ParentManifestSha256,binding.ParentWeightsSha256, // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
                binding.ConfigurationSha256,binding.ExamConditionSha256,sessionId,bindingSha256,genesisFrameSha256}.All(x=>Hash(x)) || // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
            ReadDescriptors(JsonSerializer.SerializeToElement(binding.DescriptorsV2)) is not {} descriptors || // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
            (long)descriptors.CaseCount*descriptors.RepeatCount>5000) // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
            throw new JsonException("Selection frame binding syntax is invalid."); // Reject syntax that cannot fit the admitted closed selection contract.
        var value=JsonSerializer.SerializeToElement(new {Kind="TeachingSelectionBinding",FormatVersion=2,SessionId=sessionId, // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
            BindingV2=binding,BindingSha256=bindingSha256,Revision=1,PreviousFrameSha256=genesisFrameSha256, // Use numeric Revision1 only for the new selection frame and retain its Genesis hash.
            Effects=new {BaselineDispatched="0",BaselineCompleted="0",TeachingDispatched="0",TeachingCompleted="0", // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
                EvaluatorDispatched="0",EvaluatorCompleted="0",OptimizerEntries="0",EffectOutcomeUnknown=false}, // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
            TeachingSelectionBinding=SelectionPayloadElement(payload)}); // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
        var bytes=SelectionGeneratedBytes(value).Append((byte)'\n').ToArray(); // Charge the final LF to the fixed selection slot.
        if(bytes.Length>SelectionFrameMaximumBytes) throw new JsonException("Selection frame exceeds its fixed slot."); // Reject oversized frames before returning any appendable bytes.
        return bytes; // Write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
    } // Close this scope: write the exact numeric Revision1 selection frame, zero effects, Genesis link and LF-inclusive cap.
    internal static OperationResult<CodeDocumentationTeachingSelectionFrameV1> DecodeTeachingSelectionFrame( // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
        ReadOnlyMemory<byte> lfInclusiveLine,CodeDocumentationOneLessonBindingProjectionV2 expectedBinding) // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
    { // Begin this scope: decode exact field sets and compare complete canonical bytes before returning copied syntax.
        try { // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
            var bytes=lfInclusiveLine.ToArray(); // Detach untrusted incoming bytes before bounded exact decoding.
            if(bytes.Length is <2 or >SelectionFrameMaximumBytes || bytes[^1]!=(byte)'\n' || bytes.Contains((byte)'\r')) // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                return SelectionInvalid<CodeDocumentationTeachingSelectionFrameV1>(); // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
            _=Utf8.GetString(bytes); // Reject malformed UTF8 before JSON field and canonical-byte validation.
            using var doc=JsonDocument.Parse(bytes.AsMemory(0,bytes.Length-1),new JsonDocumentOptions {MaxDepth=8}); // Bound nesting while parsing the exact LF-excluded JSON line.
            var f=doc.RootElement; // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
            if(!ExactFields(f,"Kind","FormatVersion","SessionId","BindingV2","BindingSha256","Revision","PreviousFrameSha256","Effects","TeachingSelectionBinding") || // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                f.GetProperty("Kind").GetString()!="TeachingSelectionBinding" || f.GetProperty("FormatVersion").GetRawText()!="2" || // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                f.GetProperty("Revision").GetRawText()!="1" || !Hash(f.GetProperty("SessionId").GetString()) || // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                !Hash(f.GetProperty("BindingSha256").GetString()) || !Hash(f.GetProperty("PreviousFrameSha256").GetString()) || // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                !CanonicalBytes(f.GetProperty("BindingV2")).AsSpan().SequenceEqual(CanonicalBytes(JsonSerializer.SerializeToElement(expectedBinding)))) // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                return SelectionInvalid<CodeDocumentationTeachingSelectionFrameV1>(); // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
            var p=f.GetProperty("TeachingSelectionBinding"); // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
            if(!ExactFields(p,"Schema","PolicyDocument","OriginalReceiptId","OriginalReceiptSha256","SourceSha256","Categories","ItemId","SourceId", // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                "ReferenceEntryId","SourceEntityFingerprint","Reason","FrozenContextSha256","MaterialAdmissionEvidenceSha256","Origin","InvocationId", // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                "LaunchSettingsSha256","LaunchAckSha256","RequestedModel","RequestedReasoning","ProviderTelemetry") || // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                p.GetProperty("Schema").GetString()!=CodeDocumentationTeachingSelectionPayloadV1.SchemaValue || // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                p.GetProperty("PolicyDocument").GetString()!=CodeDocumentationTeachingSelectionPayloadV1.PolicyValue || // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                p.GetProperty("Origin").GetString()!="SyntheticTest" || p.GetProperty("RequestedModel").GetString()!="gpt-6-astra" || // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                p.GetProperty("RequestedReasoning").GetString()!="ultra" || p.GetProperty("ProviderTelemetry").GetString()!="UNKNOWN" || // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                p.GetProperty("Categories").ValueKind!=JsonValueKind.Array) // Permit an array only at the admitted category field of this closed payload.
                return SelectionInvalid<CodeDocumentationTeachingSelectionFrameV1>(); // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
            string S(string name)=>p.GetProperty(name).GetString()!; // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
            var payload=CodeDocumentationTeachingSelectionPayloadV1.Capture(S("OriginalReceiptId"),S("OriginalReceiptSha256"),S("SourceSha256"), // Bind the candidate syntax to the frozen ORIGINAL, SOURCE and diagnostic order.
                p.GetProperty("Categories").EnumerateArray().Select(x=>x.GetString()!).ToArray(),S("ItemId"),S("SourceId"),S("ReferenceEntryId"), // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
                S("SourceEntityFingerprint"),S("Reason"),S("FrozenContextSha256"),S("MaterialAdmissionEvidenceSha256"),S("InvocationId"),S("LaunchSettingsSha256"),S("LaunchAckSha256")); // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
            if(!payload.IsSuccess || payload.Value is null) return payload.MapError<CodeDocumentationTeachingSelectionFrameV1>(); // Return no decoded frame when scalar payload validation fails.
            var result=new CodeDocumentationTeachingSelectionFrameV1(f.GetProperty("SessionId").GetString()!,f.GetProperty("BindingSha256").GetString()!,f.GetProperty("PreviousFrameSha256").GetString()!,payload.Value); // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
            var canonical=CanonicalTeachingSelectionFrameBytes(expectedBinding,result.SessionId,result.BindingSha256,result.GenesisFrameSha256,result.Payload); // Reconstruct the complete trusted shape before comparing every incoming byte.
            return bytes.AsSpan().SequenceEqual(canonical) ? OperationResult<CodeDocumentationTeachingSelectionFrameV1>.Ok(result) : SelectionInvalid<CodeDocumentationTeachingSelectionFrameV1>(); // Reject whitespace, unknown fields and alternate encodings through exact byte comparison.
        } catch(Exception ex) when(SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { return SelectionInvalid<CodeDocumentationTeachingSelectionFrameV1>(); } // Decode exact field sets and compare complete canonical bytes before returning copied syntax.
    } // Close this scope: decode exact field sets and compare complete canonical bytes before returning copied syntax.
} // Close this scope: decode exact field sets and compare complete canonical bytes before returning copied syntax.

internal static partial class CodeDocumentationOneLessonV2Wire // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
{ // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    internal const int SelectedStudentStartedMaximumBytes = 136139; // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    internal const int SelectedStudentCompletedMaximumBytes = 396458; // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    internal const int SelectedStudentPrivateBindingMaximumBytes = 1928; // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    internal static string SelectedStudentBytesSha256(ReadOnlySpan<byte> bytes) // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(); // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    internal static JsonElement SelectedStudentLeaseProjection() // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
        => JsonSerializer.SerializeToElement(new { CompletedMaximumBytes = "396458", GenerationSlots = "1", // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
            Origin = "SyntheticTest", Schema = "code-documentation-selected-student-lease-v1", StartedMaximumBytes = "136139" }); // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    internal static string SelectedStudentLeaseSha256() // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
        => SelectedStudentDomainSha256("xiigen-s04-selected-student-lease-v1\n", SelectedStudentLeaseProjection()); // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    private static string SelectedStudentDomainSha256(string domain, JsonElement value) // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
        => SelectedStudentBytesSha256(Utf8.GetBytes(domain).Concat(CanonicalBytes(value)).ToArray()); // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    internal static string SelectedStudentRequestSha256(CodeDocumentationSelectedStudentRequestV1 request) // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
        => SelectedStudentDomainSha256("xiigen-s04-selected-student-input-v1\n", JsonSerializer.SerializeToElement(request)); // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    internal static string SelectedStudentSettingsSha256(CodeDocumentationSelectedStudentSettingsV1 settings) // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
        => SelectedStudentDomainSha256("xiigen-s04-selected-student-settings-v1\n", JsonSerializer.SerializeToElement(settings)); // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    internal static string SelectedStudentBindingSha256(CodeDocumentationOneLessonBindingProjectionV2 binding) // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
        => SelectedStudentDomainSha256("code-documentation-one-lesson-binding-v2\n", JsonSerializer.SerializeToElement(binding)); // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
    internal static string SelectedStudentRuntimeSha256(RepositoryOwnedModelRuntimeCompatibility value) // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
        => SelectedStudentDomainSha256("xiigen-s04-selected-student-runtime-v1\n", JsonSerializer.SerializeToElement(new { // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
            configSha256 = value.ConfigSha256, topologySha256 = value.TopologySha256, tokenizerSha256 = value.TokenizerSha256, // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.
            trainerSha256 = value.TrainerSha256, schemaSha256 = value.SchemaSha256, compatibilityKey = value.CompatibilityKey })); // Keep the new Synthetic response family and domain hashes separate from legacy capacity and authority.

    internal static CodeDocumentationSelectedStudentSettingsV1 CaptureSelectedStudentSettings( // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        CodeDocumentationSavedModelAssessmentGenerationSettings settings) // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
    { // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        static string D(int value) => value.ToString(CultureInfo.InvariantCulture); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        static string Bits(float value) => unchecked((uint)BitConverter.SingleToInt32Bits(value)).ToString("x8", CultureInfo.InvariantCulture); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        var result = new CodeDocumentationSelectedStudentSettingsV1(D(settings.MaxNewTokens), Bits(settings.Temperature), // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
            D(settings.TopK), Bits(settings.TopP), D(settings.Seed), D(settings.MaxHeldOutCases), D(settings.MaxAnswerChars)); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        if (!ValidSelectedStudentSettings(result)) throw new JsonException("Invalid frozen selected Student settings."); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        return result; // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
    } // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
    private static bool StudentDecimal(string? value, int minimum, int maximum, out int parsed) // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        => int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out parsed) && // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
            parsed >= minimum && parsed <= maximum && value == parsed.ToString(CultureInfo.InvariantCulture); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
    private static bool StudentFloatBits(string? value, out float parsed) // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
    { // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        parsed = float.NaN; // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        if (!Hash(value, 8) || !uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var bits)) return false; // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        parsed = BitConverter.Int32BitsToSingle(unchecked((int)bits)); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        return float.IsFinite(parsed); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
    } // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
    internal static bool ValidSelectedStudentSettings(CodeDocumentationSelectedStudentSettingsV1? value) // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        => value is not null && StudentDecimal(value.MaxNewTokens, 1, 2048, out _) && // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
            StudentFloatBits(value.Temperature, out var temperature) && temperature is >= 0 and <= 2 && // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
            StudentDecimal(value.TopK, 0, 10000, out _) && StudentFloatBits(value.TopP, out var topP) && topP is > 0 and <= 1 && // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
            StudentDecimal(value.Seed, int.MinValue, int.MaxValue, out _) && // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
            StudentDecimal(value.MaxHeldOutCases, 1, 200, out _) && StudentDecimal(value.MaxAnswerChars, 1, 65536, out _); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
    internal static CodeDocumentationSavedModelAssessmentGenerationSettings ReadSelectedStudentSettings(CodeDocumentationSelectedStudentSettingsV1 value) // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
    { // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        if (!ValidSelectedStudentSettings(value)) throw new JsonException("Invalid selected Student settings syntax."); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        StudentFloatBits(value.Temperature, out var temperature); StudentFloatBits(value.TopP, out var topP); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        int D(string text) => int.Parse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
        return new(D(value.MaxNewTokens), temperature, D(value.TopK), topP, D(value.Seed), D(value.MaxHeldOutCases), D(value.MaxAnswerChars)); // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.
    } // Validate canonical frozen settings and exact binary32 bits without admitting generic floats or signed numbers.

    internal static bool ValidSelectedStudentRequest(CodeDocumentationSelectedStudentRequestV1? value) // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
    { // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        if (value is null || value.Schema != "code-documentation-selected-student-input-v1" || value.Origin != "SyntheticTest" || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            !new[] { value.LeaseSha256, value.BindingSha256, value.SelectionFrameSha256, value.OriginalReceiptSha256, // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
                value.SourceSha256, value.FrozenContextSha256, value.MaterialAdmissionEvidenceSha256, value.TaskSha256, // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
                value.ParentManifestSha256, value.ParentWeightsSha256, value.ConfigurationSha256, value.RuntimeCompatibilitySha256, // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
                value.TokenizerSha256, value.ModelStateBeforeSha256, value.GenerationSettingsSha256 }.All(x => Hash(x)) || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            !Hash(value.OriginalReceiptId, 32) || !SelectionToken(value.ItemId) || !SelectionToken(value.SourceId) || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(value.ReferenceEntryId) || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(value.SourceEntityFingerprint) || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            !ValidUtf8Text(value.Task, 65536) || value.Task.Length == 0 || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            value.Task.Any(c => char.IsControl(c) && c is not '\n' and not '\t') || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            !StudentDecimal(value.TaskUtf8ByteLength, 1, 65536, out var length) || Utf8.GetByteCount(value.Task) != length || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            SelectedStudentBytesSha256(Utf8.GetBytes(value.Task)) != value.TaskSha256 || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            !ValidSelectedStudentSettings(value.GenerationSettings)) return false; // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        return value.LeaseSha256 == SelectedStudentLeaseSha256() && // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            value.GenerationSettingsSha256 == SelectedStudentSettingsSha256(value.GenerationSettings); // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
    } // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
    internal static bool ValidSelectedStudentResponse(CodeDocumentationSelectedStudentRequestV1 request, string? response, // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        string? stateAfter, out byte[] responseBytes) // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
    { // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        responseBytes = []; // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        if (!ValidSelectedStudentRequest(request) || response is null || response.Length == 0 || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            response.Length > ReadSelectedStudentSettings(request.GenerationSettings).MaxAnswerChars || // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            !ValidUtf8Text(response, 196608) || !Hash(stateAfter) || stateAfter != request.ModelStateBeforeSha256) return false; // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        responseBytes = Utf8.GetBytes(response); // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        return responseBytes.Length <= 196608; // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
    } // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
    private static object SelectedStudentZeroEffects() => new { BaselineDispatched = "0", BaselineCompleted = "0", // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        TeachingDispatched = "0", TeachingCompleted = "0", EvaluatorDispatched = "0", EvaluatorCompleted = "0", // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        OptimizerEntries = "0", EffectOutcomeUnknown = false }; // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
    private static byte[] SelectedStudentFrameBytes(CodeDocumentationOneLessonBindingProjectionV2 binding, string sessionId, // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        string previous, int revision, object payload, int maximumBytes) // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
    { // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        if (!Hash(sessionId) || !Hash(previous) || CanonicalBytes(JsonSerializer.SerializeToElement(binding)).Length > 2120) // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            throw new JsonException("Invalid selected Student frame binding."); // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        var value = JsonSerializer.SerializeToElement(new { BindingSha256 = SelectedStudentBindingSha256(binding), // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            BindingV2 = binding, Effects = SelectedStudentZeroEffects(), FormatVersion = 2, Kind = "student-response", // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
            PreviousFrameSha256 = previous, Revision = revision, SessionId = sessionId, StudentResponse = payload }); // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        var line = CanonicalBytes(value).Append((byte)'\n').ToArray(); // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        if (line.Length > maximumBytes) throw new JsonException("Selected Student frame exceeds its private family ceiling."); // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
        return line; // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.
    } // Bind complete raw payloads and state while leaving syntax unable to issue model or material rights.

    internal static byte[] CanonicalSelectedStudentStartedFrameBytes(CodeDocumentationOneLessonBindingProjectionV2 binding, // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        string sessionId, CodeDocumentationSelectedStudentRequestV1 request) // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
    { // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        if (!ValidSelectedStudentRequest(request) || request.BindingSha256 != SelectedStudentBindingSha256(binding)) // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            throw new JsonException("Selected Student request differs from its frame binding."); // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        return SelectedStudentFrameBytes(binding, sessionId, request.SelectionFrameSha256, 2, new { // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            Schema = "code-documentation-selected-student-started-v1", Request = request, // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            RequestSha256 = SelectedStudentRequestSha256(request), ReservedGenerationSlots = "1", // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            CompletedGenerationSlots = "0", OutcomeUnknown = true }, SelectedStudentStartedMaximumBytes); // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
    } // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
    internal static byte[] CanonicalSelectedStudentCompletedFrameBytes(CodeDocumentationOneLessonBindingProjectionV2 binding, // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        string sessionId, CodeDocumentationSelectedStudentRequestV1 request, string startedFrameSha256, string response, string stateAfter) // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
    { // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        if (request.BindingSha256 != SelectedStudentBindingSha256(binding) || // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            !ValidSelectedStudentResponse(request, response, stateAfter, out var bytes)) // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            throw new JsonException("Invalid complete selected Student result."); // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        return SelectedStudentFrameBytes(binding, sessionId, startedFrameSha256, 3, new { // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            Schema = "code-documentation-selected-student-completed-v1", RequestSha256 = SelectedStudentRequestSha256(request), // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            StartedFrameSha256 = startedFrameSha256, ModelStateAfterSha256 = stateAfter, Response = response, // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            ResponseSha256 = SelectedStudentBytesSha256(bytes), ResponseUtf8ByteLength = bytes.Length.ToString(CultureInfo.InvariantCulture), // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            ResponseCharCount = response.Length.ToString(CultureInfo.InvariantCulture), ReservedGenerationSlots = "1", // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            CompletedGenerationSlots = "1", OutcomeUnknown = false }, SelectedStudentCompletedMaximumBytes); // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
    } // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
    private static OperationResult<T> SelectedStudentInvalid<T>() => OperationResult<T>.Fail( // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        "The closed selected Student response syntax is invalid.", "ONE_LESSON_SELECTED_STUDENT_RESPONSE_INVALID"); // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
    private static JsonDocument ParseSelectedStudentLine(ReadOnlyMemory<byte> line, int maximum) // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
    { // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        var bytes = line.Span; // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        if (bytes.Length is < 2 || bytes.Length > maximum || bytes[^1] != (byte)'\n' || // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            bytes[..^1].Contains((byte)'\n') || bytes.Contains((byte)'\r')) throw new JsonException("Invalid response LF boundary."); // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        _ = Utf8.GetString(bytes); // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        return JsonDocument.Parse(line[..^1], new JsonDocumentOptions { MaxDepth = 8 }); // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
    } // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
    private static bool SelectedStudentRootMatches(JsonElement root, CodeDocumentationOneLessonBindingProjectionV2 binding, int revision) // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
        => ExactFields(root, "BindingSha256", "BindingV2", "Effects", "FormatVersion", "Kind", "PreviousFrameSha256", "Revision", "SessionId", "StudentResponse") && // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            root.GetProperty("Kind").GetString() == "student-response" && root.GetProperty("FormatVersion").GetRawText() == "2" && // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            root.GetProperty("Revision").GetRawText() == revision.ToString(CultureInfo.InvariantCulture) && // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            Hash(root.GetProperty("SessionId").GetString()) && Hash(root.GetProperty("PreviousFrameSha256").GetString()) && // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            root.GetProperty("BindingSha256").GetString() == SelectedStudentBindingSha256(binding) && // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            CanonicalBytes(root.GetProperty("BindingV2")).AsSpan().SequenceEqual(CanonicalBytes(JsonSerializer.SerializeToElement(binding))) && // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.
            CanonicalBytes(root.GetProperty("Effects")).AsSpan().SequenceEqual(CanonicalBytes(JsonSerializer.SerializeToElement(SelectedStudentZeroEffects()))); // Encode fixed Started and Completed families with bounded LF parsing before any JSON allocation.

    internal static OperationResult<CodeDocumentationSelectedStudentFrameV1> DecodeSelectedStudentStarted( // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        ReadOnlyMemory<byte> line, CodeDocumentationOneLessonBindingProjectionV2 binding) // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
    { // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        try // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        { // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            using var doc = ParseSelectedStudentLine(line, SelectedStudentStartedMaximumBytes); var root = doc.RootElement; // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            if (!SelectedStudentRootMatches(root, binding, 2)) return SelectedStudentInvalid<CodeDocumentationSelectedStudentFrameV1>(); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            var p = root.GetProperty("StudentResponse"); var r = p.GetProperty("Request"); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            if (!ExactFields(p, "Schema", "Request", "RequestSha256", "ReservedGenerationSlots", "CompletedGenerationSlots", "OutcomeUnknown") || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                p.GetProperty("Schema").GetString() != "code-documentation-selected-student-started-v1" || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                p.GetProperty("ReservedGenerationSlots").GetString() != "1" || p.GetProperty("CompletedGenerationSlots").GetString() != "0" || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                p.GetProperty("OutcomeUnknown").ValueKind != JsonValueKind.True || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                !ExactFields(r, "Schema", "Origin", "LeaseSha256", "BindingSha256", "SelectionFrameSha256", "OriginalReceiptId", // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                    "OriginalReceiptSha256", "SourceSha256", "FrozenContextSha256", "MaterialAdmissionEvidenceSha256", "ItemId", "SourceId", // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                    "ReferenceEntryId", "SourceEntityFingerprint", "Task", "TaskUtf8ByteLength", "TaskSha256", "ParentManifestSha256", // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                    "ParentWeightsSha256", "ConfigurationSha256", "RuntimeCompatibilitySha256", "TokenizerSha256", "ModelStateBeforeSha256", // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                    "GenerationSettings", "GenerationSettingsSha256") || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                !ExactFields(r.GetProperty("GenerationSettings"), "MaxNewTokens", "Temperature", "TopK", "TopP", "Seed", "MaxHeldOutCases", "MaxAnswerChars")) // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                return SelectedStudentInvalid<CodeDocumentationSelectedStudentFrameV1>(); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            var request = JsonSerializer.Deserialize<CodeDocumentationSelectedStudentRequestV1>(r.GetRawText()); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            if (!ValidSelectedStudentRequest(request) || request!.BindingSha256 != SelectedStudentBindingSha256(binding) || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                request.SelectionFrameSha256 != root.GetProperty("PreviousFrameSha256").GetString() || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                p.GetProperty("RequestSha256").GetString() != SelectedStudentRequestSha256(request)) // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                return SelectedStudentInvalid<CodeDocumentationSelectedStudentFrameV1>(); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            var canonical = CanonicalSelectedStudentStartedFrameBytes(binding, root.GetProperty("SessionId").GetString()!, request); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            if (!line.Span.SequenceEqual(canonical)) return SelectedStudentInvalid<CodeDocumentationSelectedStudentFrameV1>(); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            return OperationResult<CodeDocumentationSelectedStudentFrameV1>.Ok(new(root.GetProperty("SessionId").GetString()!, // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                request.BindingSha256, request.SelectionFrameSha256, false, request, SelectedStudentRequestSha256(request), null, null, null, 0, 0)); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        } // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { return SelectedStudentInvalid<CodeDocumentationSelectedStudentFrameV1>(); } // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
    } // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
    internal static OperationResult<CodeDocumentationSelectedStudentFrameV1> DecodeSelectedStudentCompleted( // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        ReadOnlyMemory<byte> line, CodeDocumentationOneLessonBindingProjectionV2 binding, // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        CodeDocumentationSelectedStudentRequestV1 request, string startedFrameSha256) // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
    { // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        try // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        { // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            using var doc = ParseSelectedStudentLine(line, SelectedStudentCompletedMaximumBytes); var root = doc.RootElement; // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            if (!SelectedStudentRootMatches(root, binding, 3) || root.GetProperty("PreviousFrameSha256").GetString() != startedFrameSha256) // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                return SelectedStudentInvalid<CodeDocumentationSelectedStudentFrameV1>(); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            var p = root.GetProperty("StudentResponse"); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            if (!ExactFields(p, "Schema", "RequestSha256", "StartedFrameSha256", "ModelStateAfterSha256", "Response", "ResponseSha256", // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                "ResponseUtf8ByteLength", "ResponseCharCount", "ReservedGenerationSlots", "CompletedGenerationSlots", "OutcomeUnknown") || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                p.GetProperty("Schema").GetString() != "code-documentation-selected-student-completed-v1" || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                p.GetProperty("RequestSha256").GetString() != SelectedStudentRequestSha256(request) || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                p.GetProperty("StartedFrameSha256").GetString() != startedFrameSha256 || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                p.GetProperty("ReservedGenerationSlots").GetString() != "1" || p.GetProperty("CompletedGenerationSlots").GetString() != "1" || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                p.GetProperty("OutcomeUnknown").ValueKind != JsonValueKind.False) return SelectedStudentInvalid<CodeDocumentationSelectedStudentFrameV1>(); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            var response = p.GetProperty("Response").GetString(); var after = p.GetProperty("ModelStateAfterSha256").GetString(); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            if (!ValidSelectedStudentResponse(request, response, after, out var bytes) || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                p.GetProperty("ResponseSha256").GetString() != SelectedStudentBytesSha256(bytes) || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                !StudentDecimal(p.GetProperty("ResponseUtf8ByteLength").GetString(), 1, 196608, out var byteLength) || bytes.Length != byteLength || // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                !StudentDecimal(p.GetProperty("ResponseCharCount").GetString(), 1, 65536, out var chars) || response!.Length != chars) // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                return SelectedStudentInvalid<CodeDocumentationSelectedStudentFrameV1>(); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            var canonical = CanonicalSelectedStudentCompletedFrameBytes(binding, root.GetProperty("SessionId").GetString()!, request, startedFrameSha256, response!, after!); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            if (!line.Span.SequenceEqual(canonical)) return SelectedStudentInvalid<CodeDocumentationSelectedStudentFrameV1>(); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
            return OperationResult<CodeDocumentationSelectedStudentFrameV1>.Ok(new(root.GetProperty("SessionId").GetString()!, // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                request.BindingSha256, startedFrameSha256, true, null, SelectedStudentRequestSha256(request), after, response, // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
                SelectedStudentBytesSha256(bytes), byteLength, chars)); // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        } // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
        catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { return SelectedStudentInvalid<CodeDocumentationSelectedStudentFrameV1>(); } // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
    } // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
} // Decode closed canonical syntax and hashes without minting a Session issuer, admission or Generate permit.
