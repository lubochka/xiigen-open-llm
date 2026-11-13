// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TrainableRouter.Training;

using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;

/// <summary>Closed prepared data. Decoding never issues material, launch or completion authority.</summary>
internal static class CodeDocumentationNativeCodexSelectionWire
{
    internal const string RequestFileName = "native-codex-selection.request";
    internal const string SchemaFileName = "native-codex-selection.schema";
    internal const int PacketMaximumBytes = 16384;
    internal const int RequestMaximumBytes = 25731;
    internal const int FamilyMaximumBytes = 57364;
    internal const int SessionMaximumBytes = 229376;
    internal const string StartedFileName = "native-codex-selection.started"; // Distinct native attempt evidence never adopts Synthetic history semantics.
    internal const int StartedMaximumBytes = 8192; // Preserve the admitted LF-inclusive Started ceiling.
    internal const string StartedDomain = "code-documentation-native-selection-started-v1"; // A version marker describes data and never selects launch authority.
    internal const string Domain = "code-documentation-native-selection-prepared-v1";
    internal const string PolicyId = "S04-CODEX-TEACHING-SELECTION-v1";
    internal static readonly UTF8Encoding Utf8 = new(false, true);
    // The closed payload parser validates data only; native dispatch and exec JSON event extraction remain separate.
    private const string Schema = "{\"type\":\"object\",\"additionalProperties\":false,\"required\":[\"Nonce\",\"PreparedRequestSha256\",\"ItemId\",\"SourceId\",\"ReferenceEntryId\",\"SourceEntityFingerprint\",\"MaterialAdmissionEvidenceSha256\",\"Reason\"],\"properties\":{\"Nonce\":{\"type\":\"string\",\"pattern\":\"^[0-9a-f]{32}$\"},\"PreparedRequestSha256\":{\"type\":\"string\",\"pattern\":\"^[0-9a-f]{64}$\"},\"ItemId\":{\"type\":\"string\",\"minLength\":1},\"SourceId\":{\"type\":\"string\",\"minLength\":1},\"ReferenceEntryId\":{\"type\":\"string\",\"minLength\":1},\"SourceEntityFingerprint\":{\"type\":\"string\",\"minLength\":1},\"MaterialAdmissionEvidenceSha256\":{\"type\":\"string\",\"pattern\":\"^[0-9a-f]{64}$\"},\"Reason\":{\"type\":\"string\",\"minLength\":1,\"maxLength\":1024}}}";
    internal static byte[] CopySchemaBytes() => Utf8.GetBytes(Schema);
    internal static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    internal static Action<ReadOnlyMemory<byte>>? SelectionPayloadSnapshotForTesting { get; set; } // Deny-only observation of the new pending data snapshot, never a capability issuer.
    internal static Action<ReadOnlyMemory<byte>>? SelectionStdinScratchForTesting { get; set; } // Observe a populated new scratch before suffix writing; faults cannot change permission or limits.
    private sealed record PayloadFields(string Nonce, string PreparedRequestSha256, string ItemId, string SourceId, // The eight closed string fields are descriptive model-output data only.
        string ReferenceEntryId, string SourceEntityFingerprint, string MaterialAdmissionEvidenceSha256, string Reason); // Complete tuple and reason never establish model or material provenance.

    internal sealed class SelectionPayloadData // A parsed snapshot cannot issue material, launch, selection or completion authority.
    { // Own only the successful defensive data snapshot, not Session or process resources.
        private readonly PayloadFields fields; private readonly byte[] snapshot; // The same snapshot supplies both decoded fields and retained original bytes.
        private SelectionPayloadData(PayloadFields fields, byte[] snapshot) { this.fields = fields; this.snapshot = snapshot; } // Transfer only after the decoder's complete checks.
        internal string Nonce => fields.Nonce; // Retain the exact response nonce as data.
        internal string PreparedRequestSha256 => fields.PreparedRequestSha256; // Retain the exact response commitment without treating it as authority.
        internal string ItemId => fields.ItemId; // Keep the selected descriptive identity, with no first-item fallback.
        internal string SourceId => fields.SourceId; // Preserve the response's exact source join.
        internal string ReferenceEntryId => fields.ReferenceEntryId; // Preserve the exact reference join without importing disclosure rights.
        internal string SourceEntityFingerprint => fields.SourceEntityFingerprint; // Preserve the entire selected entity fingerprint.
        internal string MaterialAdmissionEvidenceSha256 => fields.MaterialAdmissionEvidenceSha256; // Evidence commitment alone is not an admission owner.
        internal string Reason => fields.Reason; // Preserve complete strict UTF8 reason text without normalization.
        internal byte[] CopyPayloadBytes() => snapshot.ToArray(); // Caller mutation cannot alter the accepted private snapshot or its decoded strings.
        internal static OperationResult<SelectionPayloadData> DecodeSnapshot(byte[] payload, byte[]? request, byte[]? schema, // Fixed Wire wrappers choose syntax-only or complete data binding, never process permission.
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext? context) // A Context is descriptive input; genuine runtime owners are still required by the future consumer.
        { // Own the newly copied bytes until every syntax and optional binding check finishes.
            if (payload is null || payload.Length == 0) return Refuse<SelectionPayloadData>(); // Empty/absent data has no complete payload.
            if (payload.Length > CodeDocumentationOneLessonV2Wire.FullMaterialCompletedMaximumBytes) return Refuse<SelectionPayloadData>(true); // Physical admission precedes allocation and parsing.
            byte[]? pending = payload.ToArray(); // Decode and retain one private preimage instead of rereading mutable caller bytes after parsing.
            try // Cleanup encloses the deny-only observer but syntax mapping does not.
            { // No observer result can grant authority or choose another input.
                SelectionPayloadSnapshotForTesting?.Invoke(pending); // A thrown original observer exception reaches finally and escapes unchanged.
                var parsed = ReadPayloadFields(pending); // The shared closed parser reads only this private snapshot.
                if (!parsed.IsSuccess || parsed.Value is null) return parsed.MapError<SelectionPayloadData>(); // A malformed snapshot remains data refusal.
                if (context is not null) // Only the fixed bound overload enters these complete input checks.
                { // A successful syntax parse has not yet transferred the pending copy.
                    var prepared = DecodePrepared(request!); // Reuse the exact unchanged V1/V2 canonical decoder and its lower error.
                    if (!prepared.IsSuccess || prepared.Value is null) return prepared.MapError<SelectionPayloadData>(); // Partial/overbound preparation cannot bind a response.
                    var data = prepared.Value; var f = parsed.Value; // All further checks concern this exact prepared/request pair.
                    if (!schema!.AsSpan().SequenceEqual(CopySchemaBytes()) || !schema.AsSpan().SequenceEqual(data.CopyOutputSchemaBytes()) || // Exact saved schema bytes are required, not a schema label.
                        !PreparedMatchesContext(data, context) || f.Nonce != data.Nonce || f.PreparedRequestSha256 != Sha256(request!)) return Refuse<SelectionPayloadData>(); // Full input equality precedes any tuple acceptance.
                    if (context.Candidates.Count(c => c.ItemId == f.ItemId && c.SourceId == f.SourceId && c.ReferenceEntryId == f.ReferenceEntryId && // Match every identity without sorting or selecting a fallback.
                        c.SourceEntityFingerprint == f.SourceEntityFingerprint && c.MaterialAdmissionEvidenceSha256 == f.MaterialAdmissionEvidenceSha256) != 1) return Refuse<SelectionPayloadData>(); // Missing/ambiguous/cross-source tuples remain recovery-required.
                } // No capability was created by successful full data binding.
                var result = OperationResult<SelectionPayloadData>.Ok(new(parsed.Value, pending)); pending = null; return result; // Transfer the same parsed snapshot only after all required checks succeeded.
            } // Original unexpected exceptions, including OCE, are not remapped here.
            finally { if (pending is not null) Array.Clear(pending); } // Refusal or throw clears only the new unaccepted private copy, never caller or accepted data.
        } // End this bounded data-only ownership scope.
    } // This type has no permission/receipt/Session factory or native result issuer.

    internal static OperationResult<SelectionPayloadData> DecodeSelectionPayload(byte[] payload) // Decode one payload object, never an exec JSON event stream.
        => SelectionPayloadData.DecodeSnapshot(payload, null, null, null); // A syntax-valid object remains unbound descriptive data.
    internal static OperationResult<SelectionPayloadData> DecodeSelectionPayload(byte[] payload, byte[] preparedRequest, byte[] savedSchema, // Bind exact complete saved data without issuing execution authority.
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext expectedContext) // The genuine receiving consumer supplies its full current Context.
        => preparedRequest is null || savedSchema is null || expectedContext is null ? Refuse<SelectionPayloadData>() : // A missing bound input cannot silently become syntax-only acceptance.
            SelectionPayloadData.DecodeSnapshot(payload, preparedRequest, savedSchema, expectedContext); // All matches finish before snapshot transfer.

    private static OperationResult<PayloadFields> ReadPayloadFields(byte[] snapshot) // Map only actual closed payload syntax errors; observation faults are outside this catch.
    { // Fixed schema and physical admission make all string/JSON allocations finite.
        try // Strict decoding and field validation concern only the private snapshot.
        { // Whitespace and property order are model JSON presentation, not persisted canonical request grammar.
            _ = Utf8.GetString(snapshot); using var document = JsonDocument.Parse(snapshot, new JsonDocumentOptions { MaxDepth = 2 }); // Reject invalid physical UTF8, comments, partial documents and trailing JSON objects.
            var root = document.RootElement; if (root.ValueKind != JsonValueKind.Object) return Refuse<PayloadFields>(); // Arrays/event streams cannot alias the one response object.
            string[] names = ["Nonce", "PreparedRequestSha256", "ItemId", "SourceId", "ReferenceEntryId", "SourceEntityFingerprint", "MaterialAdmissionEvidenceSha256", "Reason"]; // Preserve exactly the existing schema's eight fields.
            var present = root.EnumerateObject().Select(p => p.Name).ToArray(); // Count before set membership so duplicate fields cannot be overwritten.
            if (present.Length != names.Length || present.Distinct(StringComparer.Ordinal).Count() != names.Length || present.Any(n => !names.Contains(n, StringComparer.Ordinal))) return Refuse<PayloadFields>(); // Reject missing, duplicate, extra and case aliases.
            string Text(string name) => ReadText(root.GetProperty(name)); // Existing strict text decoding also rejects invalid escaped surrogate strings.
            var value = new PayloadFields(Text("Nonce"), Text("PreparedRequestSha256"), Text("ItemId"), Text("SourceId"), // Read only strings; null, scalar and nested values refuse.
                Text("ReferenceEntryId"), Text("SourceEntityFingerprint"), Text("MaterialAdmissionEvidenceSha256"), Text("Reason")); // Preserve exact decoded strings without trimming or rewriting.
            if (!Hex(value.Nonce, 32) || !Hex(value.PreparedRequestSha256, 64) || !Hex(value.MaterialAdmissionEvidenceSha256, 64) || // The fixed schema uses lowercase hexadecimal commitments.
                new[] { value.ItemId, value.SourceId, value.ReferenceEntryId, value.SourceEntityFingerprint }.Any(string.IsNullOrEmpty) || // Full identity matching follows in the optional bound entry.
                string.IsNullOrWhiteSpace(value.Reason) || Utf8.GetByteCount(value.Reason) > CodeDocumentationOneLessonV2Wire.FullMaterialReasonMaximumBytes) return Refuse<PayloadFields>(); // Preserve the original 1024 UTF8-byte reason bound, not a looser character count.
            return OperationResult<PayloadFields>.Ok(value); // This record is data only and owns no retained snapshot.
        } // Unexpected failures are not syntax evidence.
        catch (Exception error) when (error is JsonException or DecoderFallbackException or EncoderFallbackException or InvalidOperationException or KeyNotFoundException) // Catch only actual parsing/string-kind failures.
        { return Refuse<PayloadFields>(); } // Refusal is followed by the outer pending snapshot cleanup.
    } // No launch, storage or model effect occurs during parsing.

    private static bool PreparedMatchesContext(PreparedNativeSelectionData data, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context) // Compare complete current descriptive preimages, not commitments alone.
    { // All temporary defensive byte copies belong to this local comparison only.
        var copies = new List<byte[]>(); byte[] Keep(byte[] bytes) { copies.Add(bytes); return bytes; } // Record each successful allocation before later comparisons can throw or refuse.
        try // A mismatch never leaks new private comparison copies.
        { // Check the full binding/graph/packet before rendering the prompt.
            if (!Keep(data.CopyBindingBytes()).AsSpan().SequenceEqual(Keep(context.CopyBindingBytes())) || // Whole Session binding bytes remain exact.
                !Keep(data.CopyInputBytes()).AsSpan().SequenceEqual(Keep(context.MetadataInput.CopyBytes())) || // Include unused graph sources and ordered sanitized diagnosis.
                !Keep(data.CopyPacketBytes()).AsSpan().SequenceEqual(Keep(context.CopyCanonicalBytes()))) return false; // Keep all full teaching task/reference/provenance cells.
            return data.MatchesPromptBytes(Keep(Utf8.GetBytes(CodeDocumentationNativeCodexSelectionHost.RenderCompletePrompt(context, data.Nonce)))); // Existing streaming comparator checks the complete old base-prompt recipe.
        } // The Context and decoded prepared owners are never mutated.
        finally { foreach (var copy in copies) Array.Clear(copy); } // Clear only this comparison's new temporary copies on every exit.
    } // Equality is data consistency, not current receipt or material-rights proof.

    internal static OperationResult<byte[]> RenderSelectionStdin(byte[] preparedRequest, byte[] savedSchema) // Add the required exact request digest outside immutable Prepared bytes.
    { // Fixed assembly derives all output from admitted request/schema data; it performs no effect.
        if (preparedRequest is null || savedSchema is null) return Refuse<byte[]>(); // Missing data cannot create partial stdin.
        var decoded = DecodePrepared(preparedRequest); if (!decoded.IsSuccess || decoded.Value is null) return decoded.MapError<byte[]>(); // Preserve existing V1/V2 decode error and hash domain.
        if (!savedSchema.AsSpan().SequenceEqual(CopySchemaBytes()) || !savedSchema.AsSpan().SequenceEqual(decoded.Value.CopyOutputSchemaBytes())) return Refuse<byte[]>(); // Require the exact fixed schema before allocating the new scratch.
        var scratch = new byte[RequestMaximumBytes]; byte[]? pending = null; // A fixed non-expanding backing array cannot abandon earlier arrays during growth.
        try // Own both the scratch and any untransferred result copy through every exception/refusal.
        { // The accepted stdin gets a separate exact-length copy only after complete writing.
            using var memory = new MemoryStream(scratch, 0, scratch.Length, true, false); memory.SetLength(0); // Start empty while retaining the complete fixed 25731-byte capacity.
            using var capped = new CappedStream(memory, RequestMaximumBytes); // Reuse the actual finite production writer; no cap may be raised by callers.
            try { decoded.Value.WriteCompletePromptTo(capped); } catch (PreparationCapacityException) { return Refuse<byte[]>(true); } // Actual base writing overage is capacity refusal, with scratch clearing in finally.
            SelectionStdinScratchForTesting?.Invoke(scratch); // The populated deny-only observation is outside capacity mapping; an original injected failure escapes unchanged.
            try { WriteUtf8(capped, "\n\nExact saved request binding for this response:\nPreparedRequestSha256: " + Sha256(preparedRequest) + "\nReturn that exact value in PreparedRequestSha256.\n"); } // Digest the saved Q, never this derived stdin or expanded V2 data.
            catch (PreparationCapacityException) { return Refuse<byte[]>(true); } // The whole suffix participates in the unchanged total input ceiling.
            pending = scratch.AsSpan(0, checked((int)memory.Length)).ToArray(); var result = OperationResult<byte[]>.Ok(pending); pending = null; return result; // Transfer only complete defensive stdin; saved Q/schema/nonce stay unchanged.
        } // No syntax/capacity catch encloses the deny-only observer or arbitrary primary/OCE failures.
        finally { Array.Clear(scratch); if (pending is not null) Array.Clear(pending); } // Always clear this new scratch, including after successful accepted-copy transfer.
    } // Data preparation does not establish MAIN permission or launch admission.

    internal sealed record NativeStartedV1(string SessionId, string Nonce, string PreparedRequestSha256, int PreparedRequestLength, // Describe the immutable prepared and launch-input bindings as closed data.
        string OutputSchemaSha256, int OutputSchemaLength, string BindingSha256, string InputSha256, string ApprovalEvidenceSha256, // Describe the immutable prepared and launch-input bindings as closed data.
        string LaunchKind, string LaunchInputsCanonicalBase64, string LaunchInputsSha256); // Complete launch inputs remain descriptive and require a privately held binding to consume.

    internal static byte[] WriteStarted(NativeStartedV1 value) // Write the closed Started representation within its LF-inclusive physical bound.
    { // Write the closed Started representation within its LF-inclusive physical bound.
        if (!ValidStartedScalars(value)) throw new JsonException("Invalid closed NativeStarted data."); // Validate physical scalar/base64 limits before writer allocation.
        var json = StreamBytes(s => // Write the closed Started representation within its LF-inclusive physical bound.
        { // Write the closed Started representation within its LF-inclusive physical bound.
            using var w = new Utf8JsonWriter(s, Relaxed); w.WriteStartObject(); // The existing capped sink reserves a finite representation.
            w.WriteNumber("SchemaVersion", 1); w.WriteString("Domain", StartedDomain); // Fix the closed grammar and canonical order.
            w.WriteString("SessionId", value.SessionId); w.WriteString("Nonce", value.Nonce); // Bind the existing prepared nonce without allocating another attempt.
            w.WriteString("PreparedRequestSha256", value.PreparedRequestSha256); w.WriteNumber("PreparedRequestLength", value.PreparedRequestLength); // Pin the complete immutable prepared representation.
            w.WriteString("OutputSchemaSha256", value.OutputSchemaSha256); w.WriteNumber("OutputSchemaLength", value.OutputSchemaLength); // Pin the exact retained schema.
            w.WriteString("BindingSha256", value.BindingSha256); w.WriteString("InputSha256", value.InputSha256); w.WriteString("ApprovalEvidenceSha256", value.ApprovalEvidenceSha256); // Retain every owner/input relationship needed for changed-input rejection.
            w.WriteString("LaunchKind", value.LaunchKind); w.WriteString("LaunchInputsCanonicalBase64", value.LaunchInputsCanonicalBase64); w.WriteString("LaunchInputsSha256", value.LaunchInputsSha256); // No enforcement=true is inferred from this record.
            w.WriteEndObject(); // Write the closed Started representation within its LF-inclusive physical bound.
        }, StartedMaximumBytes - 1); // Reserve LF as part of the physical frame.
        return json.Append((byte)'\n').ToArray(); // The append remains inside the exact Started frame ceiling.
    } // Finish this bounded owner check without renewing authority or an attempt.

    internal static OperationResult<NativeStartedV1> DecodeStarted(byte[] bytes) // Decode bounded canonical Started data without issuing a launch capability.
    { // Decode bounded canonical Started data without issuing a launch capability.
        if (bytes.Length > StartedMaximumBytes) return Refuse<NativeStartedV1>(true); // Bound the physical frame before UTF8/JSON/base64 allocation.
        if (bytes.Length < 2 || bytes[^1] != (byte)'\n') return Refuse<NativeStartedV1>(); // A partial append has no canonical frame.
        try // Decode bounded canonical Started data without issuing a launch capability.
        { // Decode bounded canonical Started data without issuing a launch capability.
            _ = Utf8.GetString(bytes); using var document = JsonDocument.Parse(bytes.AsMemory(0, bytes.Length - 1), new JsonDocumentOptions { MaxDepth = 3 }); // Strict bounded decoding cannot issue a capability.
            var root = document.RootElement; // Decode bounded canonical Started data without issuing a launch capability.
            string[] fields = ["SchemaVersion", "Domain", "SessionId", "Nonce", "PreparedRequestSha256", "PreparedRequestLength", "OutputSchemaSha256", "OutputSchemaLength", "BindingSha256", "InputSha256", "ApprovalEvidenceSha256", "LaunchKind", "LaunchInputsCanonicalBase64", "LaunchInputsSha256"]; // Exact closed data fields only.
            if (root.ValueKind != JsonValueKind.Object) return Refuse<NativeStartedV1>(); // No array/null alias is admitted.
            var properties = root.EnumerateObject().Select(p => p.Name).ToArray(); // Physical admission already bounds this allocation.
            if (properties.Length != fields.Length || properties.Distinct(StringComparer.Ordinal).Count() != fields.Length || properties.Any(p => !fields.Contains(p, StringComparer.Ordinal))) return Refuse<NativeStartedV1>(); // Reject duplicate, missing, extra and case aliases.
            if (root.GetProperty("SchemaVersion").GetRawText() != "1" || root.GetProperty("Domain").GetString() != StartedDomain) return Refuse<NativeStartedV1>(); // Disallow integer/version spelling aliases.
            string Text(string name) => root.GetProperty(name).ValueKind == JsonValueKind.String ? root.GetProperty(name).GetString()! : throw new JsonException(); // Null/scalar type aliases cannot become data.
            int Natural(string name) => root.GetProperty(name).ValueKind == JsonValueKind.Number && root.GetProperty(name).TryGetInt32(out var n) && n > 0 && root.GetProperty(name).GetRawText() == n.ToString(System.Globalization.CultureInfo.InvariantCulture) ? n : throw new JsonException(); // Keep canonical positive integer lengths.
            var value = new NativeStartedV1(Text("SessionId"), Text("Nonce"), Text("PreparedRequestSha256"), Natural("PreparedRequestLength"), // Decode bounded canonical Started data without issuing a launch capability.
                Text("OutputSchemaSha256"), Natural("OutputSchemaLength"), Text("BindingSha256"), Text("InputSha256"), Text("ApprovalEvidenceSha256"), // Decode bounded canonical Started data without issuing a launch capability.
                Text("LaunchKind"), Text("LaunchInputsCanonicalBase64"), Text("LaunchInputsSha256")); // Construct bounded data only.
            return ValidStartedScalars(value) && WriteStarted(value).AsSpan().SequenceEqual(bytes) ? OperationResult<NativeStartedV1>.Ok(value) : Refuse<NativeStartedV1>(); // Byte equality rejects whitespace/order/escaping aliases.
        } // Finish this bounded owner check without renewing authority or an attempt.
        catch (PreparationCapacityException) { return Refuse<NativeStartedV1>(true); } // Keep actual physical overage classification separate from incomplete custody.
        catch (Exception ex) when (ex is JsonException or FormatException or DecoderFallbackException or EncoderFallbackException or InvalidOperationException or KeyNotFoundException or OverflowException or ArgumentException) // Decode bounded canonical Started data without issuing a launch capability.
        { return Refuse<NativeStartedV1>(); } // Malformed data never changes Session state or permission ownership.
    } // Finish this bounded owner check without renewing authority or an attempt.

    private static bool ValidStartedScalars(NativeStartedV1 value) // Validate each scalar and encoded-size limit before decoded allocation.
    { // Validate each scalar and encoded-size limit before decoded allocation.
        static bool Hex(string s, int length) => s is not null && s.Length == length && s.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'); // Fixed scalar grammar is storage validation, not semantic selection.
        if (!Hex(value.SessionId, 64) || !Hex(value.Nonce, 32) || !Hex(value.PreparedRequestSha256, 64) || !Hex(value.OutputSchemaSha256, 64) || // Validate each scalar and encoded-size limit before decoded allocation.
            !Hex(value.BindingSha256, 64) || !Hex(value.InputSha256, 64) || !Hex(value.ApprovalEvidenceSha256, 64) || !Hex(value.LaunchInputsSha256, 64) || // Validate each scalar and encoded-size limit before decoded allocation.
            value.PreparedRequestLength is < 1 or > RequestMaximumBytes || value.OutputSchemaLength is < 1 or > FamilyMaximumBytes || // Validate each scalar and encoded-size limit before decoded allocation.
            value.LaunchKind != CodeDocumentationNativeCodexLaunchInputOwner.SyntheticKind || value.LaunchInputsCanonicalBase64 is null || value.LaunchInputsCanonicalBase64.Length is < 4 or > 6144) return false; // Only the implemented synthetic storage owner is supported; native enforcement cannot be imported.
        var inputs = Convert.FromBase64String(value.LaunchInputsCanonicalBase64); // The encoded bound precedes this finite decoded allocation.
        return Convert.ToBase64String(inputs) == value.LaunchInputsCanonicalBase64 && inputs.Length > 0 && Sha256(inputs) == value.LaunchInputsSha256; // Require complete canonical bytes and their exact pin.
    } // Finish this bounded owner check without renewing authority or an attempt.

    internal sealed record PreparedNativeSelectionRequestV1(
        string SessionId, string BindingCanonicalBase64, string OriginalReceiptId, string OriginalReceiptSha256,
        IReadOnlyList<string> Categories, string InputCanonicalBase64, string MaterialPacketCanonicalBase64,
        string ApprovalCanonicalBase64, string ApprovalEvidenceSha256, string PolicyVersion,
        string PolicyContentUtf8, string Nonce, string RenderedPromptUtf8, string OutputSchemaCanonicalBase64);

    internal static byte[] Write(PreparedNativeSelectionRequestV1 value)
    {
        foreach (var text in new[] { value.SessionId, value.BindingCanonicalBase64, value.OriginalReceiptId, value.OriginalReceiptSha256,
            value.InputCanonicalBase64, value.MaterialPacketCanonicalBase64, value.ApprovalCanonicalBase64, value.ApprovalEvidenceSha256,
            value.PolicyVersion, value.PolicyContentUtf8, value.Nonce, value.RenderedPromptUtf8, value.OutputSchemaCanonicalBase64 }.Concat(value.Categories)) _ = Utf8.GetBytes(text);
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("SchemaVersion", 1); writer.WriteString("Domain", Domain);
            writer.WriteString("SessionId", value.SessionId); writer.WriteString("BindingCanonicalBase64", value.BindingCanonicalBase64);
            writer.WriteString("OriginalReceiptId", value.OriginalReceiptId); writer.WriteString("OriginalReceiptSha256", value.OriginalReceiptSha256);
            writer.WriteStartArray("Categories"); foreach (var category in value.Categories) writer.WriteStringValue(category); writer.WriteEndArray();
            writer.WriteString("InputCanonicalBase64", value.InputCanonicalBase64); writer.WriteString("MaterialPacketCanonicalBase64", value.MaterialPacketCanonicalBase64);
            writer.WriteString("ApprovalCanonicalBase64", value.ApprovalCanonicalBase64); writer.WriteString("ApprovalEvidenceSha256", value.ApprovalEvidenceSha256);
            writer.WriteString("PolicyId", PolicyId); writer.WriteString("PolicyVersion", value.PolicyVersion); writer.WriteString("PolicyContentUtf8", value.PolicyContentUtf8);
            writer.WriteString("Nonce", value.Nonce); writer.WriteString("RenderedPromptUtf8", value.RenderedPromptUtf8);
            writer.WriteString("RequestedModel", "gpt-6-astra"); writer.WriteString("RequestedReasoningEffort", "ultra");
            writer.WriteString("OutputSchemaCanonicalBase64", value.OutputSchemaCanonicalBase64); writer.WriteEndObject();
        }
        return buffer.ToArray();
    }

    internal static OperationResult<PreparedNativeSelectionRequestV1> Decode(byte[] bytes)
    {
        if (bytes.Length > RequestMaximumBytes) return Refuse<PreparedNativeSelectionRequestV1>(true);
        try
        {
            // Strict UTF8 and physical admission precede JSON parsing and every decoded allocation.
            _ = Utf8.GetString(bytes);
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 8 });
            var root = document.RootElement;
            string[] fields = ["SchemaVersion", "Domain", "SessionId", "BindingCanonicalBase64", "OriginalReceiptId", "OriginalReceiptSha256", "Categories", "InputCanonicalBase64", "MaterialPacketCanonicalBase64", "ApprovalCanonicalBase64", "ApprovalEvidenceSha256", "PolicyId", "PolicyVersion", "PolicyContentUtf8", "Nonce", "RenderedPromptUtf8", "RequestedModel", "RequestedReasoningEffort", "OutputSchemaCanonicalBase64"];
            if (root.ValueKind != JsonValueKind.Object) return Refuse<PreparedNativeSelectionRequestV1>();
            var properties = root.EnumerateObject().Select(p => p.Name).ToArray();
            if (properties.Length != fields.Length || !properties.SequenceEqual(fields, StringComparer.Ordinal)) return Refuse<PreparedNativeSelectionRequestV1>();
            string Text(string name) => root.GetProperty(name).ValueKind == JsonValueKind.String ? root.GetProperty(name).GetString()! : throw new JsonException();
            bool Hex(string value, int count) => value.Length == count && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
            bool Base64(string value, int maximum = RequestMaximumBytes)
            {
                if (value.Length > checked(4 * ((maximum + 2) / 3))) return false;
                var decoded = Convert.FromBase64String(value);
                return decoded.Length > 0 && decoded.Length <= maximum && Convert.ToBase64String(decoded) == value;
            }
            if (root.GetProperty("SchemaVersion").GetRawText() != "1" || Text("Domain") != Domain || Text("PolicyId") != PolicyId ||
                Text("RequestedModel") != "gpt-6-astra" || Text("RequestedReasoningEffort") != "ultra" ||
                !Hex(Text("SessionId"), 64) || !Hex(Text("OriginalReceiptId"), 32) || !Hex(Text("OriginalReceiptSha256"), 64) || !Hex(Text("ApprovalEvidenceSha256"), 64) ||
                !Hex(Text("Nonce"), 32) || Text("Nonce") == new string('0', 32) ||
                !Base64(Text("BindingCanonicalBase64")) || !Base64(Text("InputCanonicalBase64")) ||
                !Base64(Text("MaterialPacketCanonicalBase64"), PacketMaximumBytes) || !Base64(Text("ApprovalCanonicalBase64")) ||
                Text("OutputSchemaCanonicalBase64") != Convert.ToBase64String(CopySchemaBytes()) ||
                Text("PolicyContentUtf8") != CodeDocumentationNativeCodexSelectionHost.PolicyContent ||
                Text("PolicyVersion") != Sha256(Utf8.GetBytes(CodeDocumentationNativeCodexSelectionHost.PolicyContent))) return Refuse<PreparedNativeSelectionRequestV1>();
            var categories = root.GetProperty("Categories");
            if (categories.ValueKind != JsonValueKind.Array) return Refuse<PreparedNativeSelectionRequestV1>();
            var ordered = categories.EnumerateArray().Select(c => c.ValueKind == JsonValueKind.String ? c.GetString()! : throw new JsonException()).ToArray();
            if (ordered.Length == 0 || ordered.Any(string.IsNullOrWhiteSpace) || ordered.Distinct(StringComparer.Ordinal).Count() != ordered.Length ||
                string.IsNullOrWhiteSpace(Text("OriginalReceiptId")) || string.IsNullOrWhiteSpace(Text("RenderedPromptUtf8"))) return Refuse<PreparedNativeSelectionRequestV1>();
            var result = new PreparedNativeSelectionRequestV1(Text("SessionId"), Text("BindingCanonicalBase64"), Text("OriginalReceiptId"), Text("OriginalReceiptSha256"),
                Array.AsReadOnly(ordered), Text("InputCanonicalBase64"), Text("MaterialPacketCanonicalBase64"), Text("ApprovalCanonicalBase64"), Text("ApprovalEvidenceSha256"),
                Text("PolicyVersion"), Text("PolicyContentUtf8"), Text("Nonce"), Text("RenderedPromptUtf8"), Text("OutputSchemaCanonicalBase64"));
            return Write(result).AsSpan().SequenceEqual(bytes) ? OperationResult<PreparedNativeSelectionRequestV1>.Ok(result) : Refuse<PreparedNativeSelectionRequestV1>();
        }
        catch (Exception ex) when (ex is JsonException or FormatException or DecoderFallbackException or EncoderFallbackException or InvalidOperationException or KeyNotFoundException)
        { return Refuse<PreparedNativeSelectionRequestV1>(); }
    }

    internal static OperationResult<T> Refuse<T>(bool capacity = false) => OperationResult<T>.Fail(
        capacity ? "The complete native preparation exceeds the fixed storage limit." : "Native preparation custody requires recovery.",
        capacity ? CodeDocumentationOneLessonSession.CapacityExceededErrorCode : CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode);
    internal const string DomainV2 = "code-documentation-native-selection-prepared-v2";
    internal const int ClosedMaximumDepth = 8;
    private const string MaterialPolicy = "Judge task-specific pedagogical suitability first. Use the supplied curriculum order only to break otherwise equivalent eligible choices.";
    private static readonly JsonWriterOptions Relaxed = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    // All tuples are closed types. Atom references are strings only, never object references or authority.
    internal sealed record PreparedNativeSelectionRequestV2(string SessionId, PBinding Binding, PInput Input,
        PApproval Approval, PCandidate[] Candidates, string ApprovalEvidenceSha256, string Nonce,
        long RenderedPromptUtf8ByteLength, string RenderedPromptSha256);

    internal sealed class PreparedNativeSelectionData
    {
        private readonly PreparedNativeSelectionRequestV1? legacy;
        private readonly PreparedNativeSelectionRequestV2? compact;
        private readonly byte[] binding, input, packet;
        internal int SchemaVersion => legacy is null ? 2 : 1;
        internal string SessionId => legacy?.SessionId ?? compact!.SessionId;
        internal string Nonce => legacy?.Nonce ?? compact!.Nonce;
        internal byte[] CopyOutputSchemaBytes() => CopySchemaBytes();
        internal byte[] CopyBindingBytes() => binding.ToArray();
        internal byte[] CopyInputBytes() => input.ToArray();
        internal byte[] CopyPacketBytes() => packet.ToArray();
        // A copy requires a caller's real destination bound. Parsing never allocates a squared expansion.
        internal byte[] CopyApprovalBytes(int destinationMaximumBytes) => legacy is not null
            ? BoundedCopy(Convert.FromBase64String(legacy.ApprovalCanonicalBase64), destinationMaximumBytes)
            : CanonicalBytes(w => WriteCanonicalApproval(w, compact!.Approval), destinationMaximumBytes);
        internal byte[] RenderCompletePromptBytes(int destinationMaximumBytes) => legacy is not null
            ? BoundedCopy(Utf8.GetBytes(legacy.RenderedPromptUtf8), destinationMaximumBytes)
            : StreamBytes(s => WritePrompt(s, compact!), destinationMaximumBytes);
        internal void WriteCompletePromptTo(Stream destination) // Stream the unchanged base recipe directly into the new locally owned capped stdin scratch.
        { // This adds no new prompt format or intermediate common MemoryStream allocation.
            if (legacy is not null) WriteUtf8(destination, legacy.RenderedPromptUtf8); else WritePrompt(destination, compact!); // Preserve both accepted V1 and V2 base representations exactly.
        } // The caller owns and clears its new scratch independently of this descriptive decoded object.
        internal bool MatchesApprovalBytes(byte[] actual) => legacy is not null
            ? Convert.FromBase64String(legacy.ApprovalCanonicalBase64).AsSpan().SequenceEqual(actual)
            : CompareStream(s => WriteCanonicalApproval(new(s), compact!.Approval), actual);
        internal bool MatchesPromptBytes(byte[] actual) => legacy is not null
            ? Utf8.GetBytes(legacy.RenderedPromptUtf8).AsSpan().SequenceEqual(actual)
            : CompareStream(s => WritePrompt(s, compact!), actual);
        internal PreparedNativeSelectionData(PreparedNativeSelectionRequestV1 value)
        { legacy = value; binding = Convert.FromBase64String(value.BindingCanonicalBase64); input = Convert.FromBase64String(value.InputCanonicalBase64); packet = Convert.FromBase64String(value.MaterialPacketCanonicalBase64); }
        internal PreparedNativeSelectionData(PreparedNativeSelectionRequestV2 value, byte[] binding, byte[] input, byte[] packet)
        { compact = value; this.binding = binding; this.input = input; this.packet = packet; }
    }

    internal static PreparedNativeSelectionRequestV2 ProjectV2(string sessionId, byte[] binding, byte[] input,
        byte[] approval, string approvalHash, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context, string nonce)
    {
        using var b = JsonDocument.Parse(binding); using var i = JsonDocument.Parse(input); using var a = JsonDocument.Parse(approval);
        var candidates = context.Candidates.Select(c => new PCandidate(c.ItemId, c.SourceId, c.ReferenceEntryId,
            c.SourceEntityFingerprint, c.OwnerInput, c.PaidModelReferenceAnswer,
            new PProvenance(c.PaidReferenceProvenance.ProviderRef, c.PaidReferenceProvenance.ModelId,
                c.PaidReferenceProvenance.ModelVersion, c.PaidReferenceProvenance.RequestId,
                c.PaidReferenceProvenance.CapturedAtUtc.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture),
                c.PaidReferenceProvenance.ReferenceAnswerSha256, c.PaidReferenceProvenance.CanonicalRepresentation,
                c.PaidReferenceProvenance.ProvenanceSha256), c.MaterialAdmissionEvidenceSha256)).ToArray();
        var value = new PreparedNativeSelectionRequestV2(sessionId, ReadObjectBinding(b.RootElement), ReadObjectInput(i.RootElement),
            ReadObjectApproval(a.RootElement), candidates, approvalHash, nonce, 0, "");
        var reconstructed = ReconstructSmall(value);
        if (!reconstructed.Binding.AsSpan().SequenceEqual(binding) || !reconstructed.Input.AsSpan().SequenceEqual(input) ||
            !reconstructed.Packet.AsSpan().SequenceEqual(context.CopyCanonicalBytes()) ||
            !CompareStream(s => WriteCanonicalApproval(new(s), value.Approval), approval)) throw new JsonException("Complete owner preimages differ.");
        var prompt = MeasureHash(s => WritePrompt(s, value));
        var actualPrompt = Utf8.GetBytes(CodeDocumentationNativeCodexSelectionHost.RenderCompletePrompt(context, nonce));
        if (!CompareStream(s => WritePrompt(s, value), actualPrompt)) throw new JsonException("Complete owner prompt differs.");
        return value with { RenderedPromptUtf8ByteLength = prompt.Length, RenderedPromptSha256 = prompt.Hash };
    }

    internal static byte[] WriteV2(PreparedNativeSelectionRequestV2 value)
    {
        var atoms = new AtomTable();
        // Tuple payload is at most the admitted request. First use traversal is deliberately explicit.
        var tuples = StreamBytes(s => { using var w = new Utf8JsonWriter(s, Relaxed); w.WriteStartArray();
            WriteTupleBinding(w, value.Binding, atoms); WriteTupleInput(w, value.Input, atoms);
            WriteTupleApproval(w, value.Approval, atoms); WriteArray(w, value.Candidates, x => WriteTupleCandidate(w, x, atoms));
            w.WriteEndArray(); }, RequestMaximumBytes);
        using var d = JsonDocument.Parse(tuples, new JsonDocumentOptions { MaxDepth = ClosedMaximumDepth });
        return StreamBytes(s => {
            using var w = new Utf8JsonWriter(s, Relaxed); w.WriteStartObject();
            w.WriteNumber("SchemaVersion", 2); w.WriteString("Domain", DomainV2); w.WriteString("SessionId", value.SessionId);
            w.WriteStartArray("Atoms"); foreach (var atom in atoms.Values) w.WriteStringValue(atom); w.WriteEndArray();
            w.WritePropertyName("Binding"); d.RootElement[0].WriteTo(w); w.WritePropertyName("Input"); d.RootElement[1].WriteTo(w);
            w.WritePropertyName("Approval"); d.RootElement[2].WriteTo(w); w.WritePropertyName("Candidates"); d.RootElement[3].WriteTo(w);
            w.WriteString("ApprovalEvidenceSha256", value.ApprovalEvidenceSha256); w.WriteString("PolicyId", PolicyId);
            w.WriteString("PolicyVersion", Sha256(Utf8.GetBytes(CodeDocumentationNativeCodexSelectionHost.PolicyContent)));
            w.WriteString("PolicyContentUtf8", CodeDocumentationNativeCodexSelectionHost.PolicyContent); w.WriteString("Nonce", value.Nonce);
            w.WriteNumber("PromptRecipeVersion", 1); w.WriteNumber("RenderedPromptUtf8ByteLength", value.RenderedPromptUtf8ByteLength);
            w.WriteString("RenderedPromptSha256", value.RenderedPromptSha256); w.WriteString("RequestedModel", "gpt-6-astra");
            w.WriteString("RequestedReasoningEffort", "ultra"); w.WriteString("OutputSchemaCanonicalBase64", Convert.ToBase64String(CopySchemaBytes())); w.WriteEndObject();
        }, RequestMaximumBytes);
    }

    internal static OperationResult<PreparedNativeSelectionData> DecodePrepared(byte[] bytes)
    {
        if (bytes.Length > RequestMaximumBytes) return Refuse<PreparedNativeSelectionData>(true);
        try
        {
            _ = Utf8.GetString(bytes);
            using var d = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = ClosedMaximumDepth });
            var r = d.RootElement;
            if (r.ValueKind != JsonValueKind.Object || !r.TryGetProperty("SchemaVersion", out var version)) return Refuse<PreparedNativeSelectionData>();
            if (version.GetRawText() == "1")
            {
                var old = Decode(bytes); return old.IsSuccess && old.Value is not null
                    ? OperationResult<PreparedNativeSelectionData>.Ok(new(old.Value)) : old.MapError<PreparedNativeSelectionData>();
            }
            RequireObject(r, ["SchemaVersion", "Domain", "SessionId", "Atoms", "Binding", "Input", "Approval", "Candidates", "ApprovalEvidenceSha256", "PolicyId", "PolicyVersion", "PolicyContentUtf8", "Nonce", "PromptRecipeVersion", "RenderedPromptUtf8ByteLength", "RenderedPromptSha256", "RequestedModel", "RequestedReasoningEffort", "OutputSchemaCanonicalBase64"]);
            string Text(string n) => ReadText(r.GetProperty(n));
            if (version.GetRawText() != "2" || Text("Domain") != DomainV2 || Text("PolicyId") != PolicyId ||
                Text("PolicyContentUtf8") != CodeDocumentationNativeCodexSelectionHost.PolicyContent ||
                Text("PolicyVersion") != Sha256(Utf8.GetBytes(CodeDocumentationNativeCodexSelectionHost.PolicyContent)) ||
                !Hex(Text("SessionId"), 64) || !Hex(Text("Nonce"), 32) || Text("Nonce") == new string('0', 32) ||
                Text("RequestedModel") != "gpt-6-astra" || Text("RequestedReasoningEffort") != "ultra" ||
                Text("OutputSchemaCanonicalBase64") != Convert.ToBase64String(CopySchemaBytes()) || r.GetProperty("PromptRecipeVersion").GetRawText() != "1" ||
                !Hex(Text("ApprovalEvidenceSha256"), 64) || !Hex(Text("RenderedPromptSha256"), 64)) throw new JsonException();
            var atoms = new AtomReader(ReadArray(r.GetProperty("Atoms"), ReadText), bytes.Length);
            var binding = ReadTupleBinding(r.GetProperty("Binding"), atoms); var input = ReadTupleInput(r.GetProperty("Input"), atoms);
            var approval = ReadTupleApproval(r.GetProperty("Approval"), atoms); var candidates = ReadArray(r.GetProperty("Candidates"), x => ReadTupleCandidate(x, atoms));
            atoms.Complete();
            var value = new PreparedNativeSelectionRequestV2(Text("SessionId"), binding, input, approval, candidates,
                Text("ApprovalEvidenceSha256"), Text("Nonce"), ReadNatural(r.GetProperty("RenderedPromptUtf8ByteLength")), Text("RenderedPromptSha256"));
            // This first preflight bounds binding/input/packet before approval or prompt expansion.
            var small = ReconstructSmall(value); ValidateJoins(value, small.Binding, small.Input);
            var approvalHash = MeasureHash(s => WriteCanonicalApproval(new(s), approval));
            if (approvalHash.Hash != value.ApprovalEvidenceSha256) throw new JsonException();
            ValidateAdmission(value, small.Binding, small.Input, approvalHash.Length);
            var prompt = MeasureHash(s => WritePrompt(s, value));
            if (prompt.Length != value.RenderedPromptUtf8ByteLength || prompt.Hash != value.RenderedPromptSha256 ||
                !WriteV2(value).AsSpan().SequenceEqual(bytes)) throw new JsonException();
            return OperationResult<PreparedNativeSelectionData>.Ok(new(value, small.Binding, small.Input, small.Packet));
        }
        catch (PreparationCapacityException) { return Refuse<PreparedNativeSelectionData>(true); }
        catch (Exception ex) when (ex is JsonException or FormatException or DecoderFallbackException or EncoderFallbackException or InvalidOperationException or KeyNotFoundException or OverflowException or ArgumentException)
        { return Refuse<PreparedNativeSelectionData>(); }
    }

    private static (byte[] Binding, byte[] Input, byte[] Packet) ReconstructSmall(PreparedNativeSelectionRequestV2 v)
    {
        var b = MeasureHash(s => WriteCanonicalBinding(new(s), v.Binding));
        var i = MeasureHash(s => WriteCanonicalInput(new(s), v.Input));
        // Use the existing Session V2 binding-domain digest.
        var bindingHash = MeasureHash(s => { WriteUtf8(s, "code-documentation-one-lesson-binding-v2\n"); WriteCanonicalBinding(new(s), v.Binding); }).Hash; // Match the Session owner's digest without changing canonical binding bytes.
        using var preflight = new CountingStream(PacketMaximumBytes); WritePacket(preflight, v, b.Length, i.Length, bindingHash, null, null); // Check the whole packet before expansion using canonical lengths without the digest domain.
        var binding = CanonicalBytes(w => WriteCanonicalBinding(w, v.Binding), PacketMaximumBytes);
        var input = CanonicalBytes(w => WriteCanonicalInput(w, v.Input), PacketMaximumBytes);
        var packet = StreamBytes(s => WritePacket(s, v, binding.Length, input.Length, bindingHash, binding, input), PacketMaximumBytes); // Frame the complete binding and input with the same owner digest.
        return (binding, input, packet);
    }
    private static void WritePacket(Stream s, PreparedNativeSelectionRequestV2 v, long bindingLength, long inputLength, string bindingHash, byte[]? binding, byte[]? input)
    {
        using var w = new BinaryWriter(s, Utf8, true); w.Write(Utf8.GetBytes("code-documentation-selection-material-input-v1\n"));
        void Field(string text) { w.Write(Utf8.GetByteCount(text)); WriteUtf8(s, text); }
        void Bytes(long length, byte[]? bytes) { w.Write(checked((int)length)); if (bytes is not null) w.Write(bytes); else ((CountingStream)s).Add(length); }
        Field(MaterialPolicy); Field(bindingHash); Bytes(bindingLength, binding); Bytes(inputLength, input);
        w.Write(v.Input.Categories.Length); foreach (var category in v.Input.Categories) Field(category);
        w.Write(v.Candidates.Length); foreach (var c in v.Candidates)
        { Field(c.ItemId); Field(c.SourceId); Field(c.ReferenceEntryId); Field(c.SourceEntityFingerprint); Field(c.OwnerInput); Field(c.PaidModelReferenceAnswer); Field(c.PaidReferenceProvenance.CanonicalRepresentation); Field(c.MaterialAdmissionEvidenceSha256); }
        w.Flush();
    }
    private static void ValidateJoins(PreparedNativeSelectionRequestV2 v, byte[] binding, byte[] input)
    {
        var g = v.Input.TeachingGraph; var c = g.Selection.TeachingCurriculum;
        var selection = CodeDocumentationOneLessonSourceSelection.Create(
            g.Selection.Sources.Select(x => new CodeDocumentationOneLessonSourceV2(x.SourceId,x.Role,x.DomainSlug,x.DesignPatternSlug,x.DatasetVersion,x.ManifestFileSha256,x.ManifestFileByteLength,x.SealedManifestSha256)).ToArray(),
            g.Selection.PairSelections.Select(x => new CodeDocumentationOneLessonPairSelectionV2(x.TrainingPairId,x.SourceId)).ToArray(),
            new(c.PolicyId,c.PolicyVersion,c.Sources.Select(x => new CodeDocumentationOneLessonTeachingSourceV2(x.SourceId,x.CorpusId,x.DomainSlug,x.SubjectSlug,x.CorpusVersion,x.ManifestFileSha256,x.ManifestFileByteLength,x.CorpusSha256,x.PayloadSha256,x.PayloadByteLength)).ToArray(),
                c.Items.Select(x => new CodeDocumentationOneLessonTeachingItemV2(x.ItemId,x.SourceId,x.ReferenceEntryId,x.SourceEntityFingerprint)).ToArray(),
                c.CategoryBindings.Select(x => new CodeDocumentationOneLessonTeachingCategoryBindingV2(x.Category,x.OrderedItemIds)).ToArray()));
        if(!selection.IsSuccess)throw new JsonException(); // Existing complete graph syntax/sets, never rights or admission.
        bool Text(string x,int maximum)=>!string.IsNullOrWhiteSpace(x)&&x==x.Trim()&&!x.Any(char.IsControl)&&Utf8.GetByteCount(x)<=maximum;
        bool Ref(string x)=>x.StartsWith("sha256:",StringComparison.Ordinal)&&Hex(x[7..],64);
        bool Utc(string x)=>DateTimeOffset.TryParseExact(x,"O",System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,out var at)&&at.Offset==TimeSpan.Zero&&at!=default&&
            at.ToUniversalTime().ToString("O",System.Globalization.CultureInfo.InvariantCulture)==x;
        bool ApprovalUtc(string x)=>DateTimeOffset.TryParseExact(x,"yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal|System.Globalization.DateTimeStyles.AdjustToUniversal,out var at)&&
            at.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",System.Globalization.CultureInfo.InvariantCulture)==x;
        if(!new[]{v.Binding.ObjectiveId,v.Binding.GrantId,v.Binding.GrantVersion,v.Binding.TrainingRunId}.All(x=>Text(x,256)) ||
            !new[]{v.Binding.CandidateSha256,v.Binding.ParentManifestSha256,v.Binding.ParentWeightsSha256,v.Binding.ConfigurationSha256,v.Binding.ExamConditionSha256,
                v.Binding.DescriptorsV2.SourceSelectionSha256,v.Binding.DescriptorsV2.ParentCheckpointSha256,v.Binding.DescriptorsV2.CandidateSelectionSha256,
                v.Binding.DescriptorsV2.CorpusDescriptorSha256,v.Binding.DescriptorsV2.EvaluationPlanSha256,v.Binding.DescriptorsV2.EvaluatorProfileSha256,v.Binding.DescriptorsV2.ProfileSelectionSha256}.All(x=>Hex(x,64)) ||
            v.Binding.DescriptorsV2.CaseCount>int.MaxValue || v.Binding.DescriptorsV2.RepeatCount>int.MaxValue || v.Approval.ApprovalRevision>int.MaxValue ||
            !Text(v.Approval.ApprovalId,80)||!v.Approval.ApprovalId.All(x=>char.IsAsciiLetterOrDigit(x)||x is '-' or '_')||!Text(v.Approval.ApproverId,256)||!ApprovalUtc(v.Approval.ApprovedAtUtc)||
            v.Approval.OperatorDecision!="PermitFullTeachingSourceDisclosure"||v.Approval.Use!="PedagogicalSelectionContextOnly"||v.Approval.Recipient!="ServerOwnedNativeCodexPedagogicalSelection"||
            v.Approval.AllTeachingSources.Length is <1 or >8||v.Approval.Evidence.Any(x=>!Text(x.EvidenceRef,512)||!Hex(x.EvidenceSha256,64))||
            v.Approval.Evidence.Select(x=>x.EvidenceRef).Distinct(StringComparer.Ordinal).Count()!=v.Approval.Evidence.Length)throw new JsonException();
        // Both sides are subordinate to the already-admitted complete input/packet; compare whole bytes.
        bool Same<T>(T x, T y, Action<CanonicalSink,T> write) => CanonicalBytes(w=>write(w,x),PacketMaximumBytes).AsSpan().SequenceEqual(CanonicalBytes(w=>write(w,y),PacketMaximumBytes));
        if (v.Binding.DescriptorsV2.SchemaVersion != 2 || v.Binding.DescriptorsV2.CaseCount < 1 || v.Binding.DescriptorsV2.RepeatCount < 1 ||
            v.Approval.SchemaVersion != 1 || v.Approval.ApprovalRevision < 1 || v.Approval.Evidence.Length is < 1 or > 32 ||
            v.Input.Schema != "code-documentation-selection-binding-input-v1" || !Hex(v.Input.OriginalReceiptId,32) || !Hex(v.Input.OriginalReceiptSha256,64) ||
            v.Input.Categories.Length == 0 || v.Input.Categories.Any(string.IsNullOrWhiteSpace) || v.Input.Categories.Distinct(StringComparer.Ordinal).Count() != v.Input.Categories.Length ||
            v.Approval.GrantId != v.Binding.GrantId || v.Approval.GrantVersion != v.Binding.GrantVersion ||
            v.Approval.Model != "gpt-6-astra" || v.Approval.ReasoningEffort != "ultra" ||
            !Same(g.Selection, v.Approval.SourceSelection, WriteCanonicalSelection) ||
            !Same(g.CategoryBindings, c.CategoryBindings, (w,x) => WriteArray(w,x,y => WriteCanonicalCategory(w,y))) ||
            g.Sources.Length != c.Sources.Length || g.Sources.Length != v.Approval.AllTeachingSources.Length || g.Items.Length != c.Items.Length) throw new JsonException();
        var selectionHash = MeasureHash(s => { WriteUtf8(s,"xiigen-s04-source-selection-v2\n"); WriteCanonicalSelection(new(s),g.Selection); }).Hash;
        if (selectionHash != v.Input.SourceSha256 || selectionHash != v.Binding.DescriptorsV2.SourceSelectionSha256) throw new JsonException();
        for (var n=0;n<g.Sources.Length;n++)
        {
            var source=g.Sources[n]; var approved=v.Approval.AllTeachingSources[n]; var descriptor=c.Sources[n];
            if (!Same(source.Descriptor,descriptor,WriteCanonicalDescriptor) || !Same(descriptor,approved.Descriptor,WriteCanonicalDescriptor) ||
                source.ManifestFileSha256 != descriptor.ManifestFileSha256 || source.ManifestFileByteLength != descriptor.ManifestFileByteLength ||
                source.Manifest.EntryCount != source.Entries.Length || source.Entries.Length != approved.Entries.Length ||
                source.Manifest.Domain != descriptor.DomainSlug || source.Manifest.Subject != descriptor.SubjectSlug || source.Manifest.CorpusVersion != descriptor.CorpusVersion ||
                source.Manifest.CorpusSha256 != "sha256:"+descriptor.CorpusSha256 || source.Manifest.PayloadSha256 != descriptor.PayloadSha256 || source.Manifest.PayloadByteLength != descriptor.PayloadByteLength) throw new JsonException();
            if(source.Entries.Length is <1 or >10000||!Utc(source.Manifest.FrozenAtUtc)||!Ref(source.Manifest.CorpusSha256)||!Hex(source.Manifest.ManifestSha256,64)||
                source.Entries.Select(x=>x.EntryId).Distinct(StringComparer.Ordinal).Count()!=source.Entries.Length)throw new JsonException();
            for(var j=0;j<source.Entries.Length;j++)
            { var entry=source.Entries[j]; var a=approved.Entries[j];
                if(!Ref(entry.EntryId)||!Ref(entry.SourceEntityFingerprint)||!Ref(entry.OwnerInputSha256)||!Ref(entry.PaidReferenceProvenance.ReferenceAnswerSha256)||!Ref(entry.PaidReferenceProvenance.ProvenanceSha256)||!Utc(entry.PaidReferenceProvenance.CapturedAtUtc)||
                    !a.Eligibility.LicenseAccepted||a.Eligibility.IsQuarantined||a.Eligibility.EvidenceRefs.Length is <1 or >32||a.Eligibility.EvidenceRefs.Any(x=>!Text(x,512))||a.Eligibility.EvidenceRefs.Distinct(StringComparer.Ordinal).Count()!=a.Eligibility.EvidenceRefs.Length)throw new JsonException();
                var p=entry.PaidReferenceProvenance;
                var provenance=PaidReferenceProvenance.Create(p.ProviderRef,p.ModelId,p.ModelVersion,p.RequestId,
                    DateTimeOffset.ParseExact(p.CapturedAtUtc,"O",System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None),p.ReferenceAnswerSha256);
                if(!provenance.IsSuccess||provenance.Value is null||provenance.Value.CanonicalRepresentation!=p.CanonicalRepresentation||provenance.Value.ProvenanceSha256!=p.ProvenanceSha256)throw new JsonException();
                if(entry.EntryId!=a.EntryId || entry.SourceEntityFingerprint!=a.SourceEntityFingerprint || entry.OwnerInputSha256!=a.OwnerInputSha256 ||
                    entry.PaidReferenceProvenance.ReferenceAnswerSha256!=a.ReferenceAnswerSha256 || entry.PaidReferenceProvenance.ProvenanceSha256!=a.PaidReferenceProvenanceSha256 ||
                    entry.DomainTag!=descriptor.DomainSlug || entry.SubjectId!=descriptor.SubjectSlug || entry.Partition is not ("Train" or "HeldOut")) throw new JsonException();
            }
        }
        for(var n=0;n<c.Items.Length;n++)
        { var item=c.Items[n]; var resolved=g.Items[n];var source=g.Sources.Single(x=>x.Descriptor.SourceId==item.SourceId);var entry=source.Entries.Single(x=>x.EntryId==item.ReferenceEntryId);
            if(!Same(item,resolved.DeclaredItem,WriteCanonicalItem) || item.SourceId!=resolved.SourceId || item.ReferenceEntryId!=resolved.ReferenceEntryId || item.SourceEntityFingerprint!=resolved.SourceEntityFingerprint||entry.SourceEntityFingerprint!=item.SourceEntityFingerprint) throw new JsonException(); }
        var eligible=c.CategoryBindings.Where(x=>v.Input.Categories.Contains(x.Category,StringComparer.Ordinal)).SelectMany(x=>x.OrderedItemIds).ToHashSet(StringComparer.Ordinal);
        var selected=c.Items.Where(x=>eligible.Contains(x.ItemId)).ToArray();
        if(selected.Length==0 || selected.Length!=v.Candidates.Length) throw new JsonException();
        for(var n=0;n<selected.Length;n++)
        {var item=selected[n];var candidate=v.Candidates[n];var source=g.Sources.Single(x=>x.Descriptor.SourceId==item.SourceId);var entry=source.Entries.Single(x=>x.EntryId==item.ReferenceEntryId);
            if(candidate.ItemId!=item.ItemId || candidate.SourceId!=item.SourceId || candidate.ReferenceEntryId!=item.ReferenceEntryId || candidate.SourceEntityFingerprint!=item.SourceEntityFingerprint ||
                entry.SourceEntityFingerprint!=item.SourceEntityFingerprint || !Same(candidate.PaidReferenceProvenance,entry.PaidReferenceProvenance,WriteCanonicalProvenance) ||
                FrozenReferenceEntry.ComputeNormalizedTextSha256(candidate.OwnerInput)!=entry.OwnerInputSha256 ||
                FrozenReferenceEntry.ComputeNormalizedTextSha256(candidate.PaidModelReferenceAnswer)!=entry.PaidReferenceProvenance.ReferenceAnswerSha256) throw new JsonException();
        }
    }
    private static void ValidateAdmission(PreparedNativeSelectionRequestV2 v, byte[] binding, byte[] input, long approvalLength)
    {
        foreach(var c in v.Candidates)
        {
            var entry=v.Input.TeachingGraph.Sources.Single(x=>x.Descriptor.SourceId==c.SourceId).Entries.Single(x=>x.EntryId==c.ReferenceEntryId);
            var hash=MeasureHash(s=>{ using var w=new BinaryWriter(s,Utf8,true); WriteUtf8(s,"code-documentation-startup-teaching-disclosure-v1\n");
                w.Write(checked((int)approvalLength)); WriteCanonicalApproval(new(s),v.Approval);
                void Bytes(byte[] b){w.Write(b.Length);w.Write(b);} void Field(string t){w.Write(Utf8.GetByteCount(t));WriteUtf8(s,t);}
                Bytes(binding);Bytes(input);Field(c.ItemId);Field(c.SourceId);Field(c.ReferenceEntryId);Field(c.SourceEntityFingerprint);Field(entry.OwnerInputSha256);Field(c.PaidReferenceProvenance.CanonicalRepresentation);w.Flush(); });
            if(hash.Hash!=c.MaterialAdmissionEvidenceSha256)throw new JsonException();
        }
    }
    private static void WritePrompt(Stream s, PreparedNativeSelectionRequestV2 v)
    {
        var w = new CanonicalSink(s);
        WriteUtf8(s,CodeDocumentationNativeCodexSelectionHost.PolicyContent+"\n\nSaved sanitized weakness categories:\n");
        WriteArray(w,v.Input.Categories,x=>w.RelaxedString(x));
        WriteUtf8(s,"\nAll eligible teaching candidates in declared order:\n");
        WriteArray(w,v.Candidates,c=>{
            void Field(string name,string text){w.RelaxedString(name);w.Raw(":");w.RelaxedString(text);}
            w.Raw("{");Field("ItemId",c.ItemId);w.Raw(",");Field("SourceId",c.SourceId);w.Raw(",");Field("ReferenceEntryId",c.ReferenceEntryId);w.Raw(",");Field("SourceEntityFingerprint",c.SourceEntityFingerprint);
            w.Raw(",");Field("OwnerInput",c.OwnerInput);w.Raw(",");Field("PaidModelReferenceAnswer",c.PaidModelReferenceAnswer);w.Raw(",\"PaidReferenceProvenance\":{");var p=c.PaidReferenceProvenance;
            Field("ProviderRef",p.ProviderRef);w.Raw(",");Field("ModelId",p.ModelId);w.Raw(",");Field("ModelVersion",p.ModelVersion);w.Raw(",");Field("RequestId",p.RequestId);w.Raw(",");
            Field("CapturedAtUtc",DateTimeOffset.Parse(p.CapturedAtUtc,System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz",System.Globalization.CultureInfo.InvariantCulture));w.Raw(",");Field("ReferenceAnswerSha256",p.ReferenceAnswerSha256);
            w.Raw(",\"ClosedPaidReference\":true,\"PaidReferenceOutputNeverTrainable\":true,\"PaidReferenceOutputNeverDpoPairSide\":true,");
            Field("CanonicalRepresentation",p.CanonicalRepresentation);w.Raw(",");Field("ProvenanceSha256",p.ProvenanceSha256);w.Raw("},");Field("MaterialAdmissionEvidenceSha256",c.MaterialAdmissionEvidenceSha256);w.Raw("}");});
        WriteUtf8(s,"\nReturn one future selection tuple and reason under the fixed output schema. Prepared nonce: "+v.Nonce);
    }

    private static bool Hex(string text,int length)=>text.Length==length&&text.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f');
    private static string ReadText(JsonElement e){if(e.ValueKind!=JsonValueKind.String)throw new JsonException();var t=e.GetString()!;_=Utf8.GetByteCount(t);return t;}
    private static long ReadNatural(JsonElement e){var t=e.GetRawText();if(e.ValueKind!=JsonValueKind.Number||t.Length==0||t.Length>1&&t[0]=='0'||t.Any(c=>c is <'0' or >'9')||!e.TryGetInt64(out var n)||n<0)throw new JsonException();return n;}
    private static bool ReadBoolean(JsonElement e)=>e.ValueKind is JsonValueKind.True or JsonValueKind.False?e.GetBoolean():throw new JsonException();
    private static void RequireObject(JsonElement e,string[] fields){if(e.ValueKind!=JsonValueKind.Object||!e.EnumerateObject().Select(p=>p.Name).SequenceEqual(fields,StringComparer.Ordinal))throw new JsonException();}
    private static void RequireTuple(JsonElement e,int arity){if(e.ValueKind!=JsonValueKind.Array||e.GetArrayLength()!=arity)throw new JsonException();}
    private static T[] ReadArray<T>(JsonElement e,Func<JsonElement,T> read){if(e.ValueKind!=JsonValueKind.Array||e.GetArrayLength()>RequestMaximumBytes)throw new JsonException();return e.EnumerateArray().Select(read).ToArray();}
    private static void WriteArray<T>(Utf8JsonWriter w,T[] values,Action<T> write){w.WriteStartArray();foreach(var v in values)write(v);w.WriteEndArray();}
    private static void WriteArray<T>(CanonicalSink w,T[] values,Action<T> write){w.Raw("[");for(var i=0;i<values.Length;i++){if(i!=0)w.Raw(",");write(values[i]);}w.Raw("]");}
    private sealed class AtomTable
    {private readonly Dictionary<string,int> index=new(StringComparer.Ordinal);internal readonly List<string> Values=[];
        internal int Intern(string text){_=Utf8.GetByteCount(text);if(index.TryGetValue(text,out var n))return n;n=Values.Count;index.Add(text,n);Values.Add(text);return n;}}
    private sealed class AtomReader
    {private readonly string[] values;private int next;internal AtomReader(string[] values,int physical){this.values=values;
        if(values.Length>physical||values.Distinct(StringComparer.Ordinal).Count()!=values.Length||values.Sum(x=>(long)Utf8.GetByteCount(x))>physical)throw new JsonException();}
        internal string Read(JsonElement e){var n=ReadNatural(e);if(n>=values.Length||n>next)throw new JsonException();if(n==next)next++;return values[(int)n];}
        internal void Complete(){if(next!=values.Length)throw new JsonException();}}

    internal sealed class PreparationCapacityException:Exception;
    private sealed class CanonicalSink(Stream stream)
    {
        internal void Raw(string t)=>WriteUtf8(stream,t);
        internal void Number(long n)=>Raw(n.ToString(System.Globalization.CultureInfo.InvariantCulture));
        internal void Boolean(bool b)=>Raw(b?"true":"false");
        internal void RelaxedString(string text)
        {
            _=Utf8.GetByteCount(text);Raw("\"");Span<byte> scalar=stackalloc byte[4];Span<byte> escaped=stackalloc byte[24];
            foreach(var rune in text.EnumerateRunes())
            {
                var count=rune.EncodeToUtf8(scalar);
                var status=JavaScriptEncoder.UnsafeRelaxedJsonEscaping.EncodeUtf8(scalar[..count],escaped,out var consumed,out var written,true);
                if(status!=System.Buffers.OperationStatus.Done||consumed!=count)throw new JsonException();stream.Write(escaped[..written]);
            }
            Raw("\"");
        }
        internal void String(string t)
        {
            _=Utf8.GetByteCount(t);Raw("\"");int start=0;
            for(int i=0;i<t.Length;i++){var c=t[i];string? escaped=c switch{'"'=>"\\\"",'\\'=>"\\\\",'\b'=>"\\b",'\t'=>"\\t",'\n'=>"\\n",'\f'=>"\\f",'\r'=>"\\r",< ' ' =>"\\u"+((int)c).ToString("x4",System.Globalization.CultureInfo.InvariantCulture),_=>null};
                if(escaped is null)continue;WriteUtf8(stream,t.AsSpan(start,i-start));Raw(escaped);start=i+1;}
            WriteUtf8(stream,t.AsSpan(start));Raw("\"");
        }
    }
    // Fixed scratch is independent of reference amplification. Encoder boundaries preserve surrogate pairs.
    private static void WriteUtf8(Stream stream,string text)=>WriteUtf8(stream,text.AsSpan());
    private static void WriteUtf8(Stream stream,ReadOnlySpan<char> text)
    {var encoder=Utf8.GetEncoder();Span<byte> scratch=stackalloc byte[4096];while(!text.IsEmpty){encoder.Convert(text,scratch,true,out var chars,out var bytes,out _);stream.Write(scratch[..bytes]);text=text[chars..];}}
    private static byte[] BoundedCopy(byte[] bytes,int bound){if(bytes.Length>bound)throw new PreparationCapacityException();return bytes.ToArray();}
    private static byte[] CanonicalBytes(Action<CanonicalSink> write,int bound)=>StreamBytes(s=>write(new(s)),bound);
    private static byte[] StreamBytes(Action<Stream> write,int bound){using var memory=new MemoryStream();using var capped=new CappedStream(memory,bound);write(capped);return memory.ToArray();}
    private static (long Length,string Hash) MeasureHash(Action<Stream> write){using var sink=new HashStream();write(sink);return(sink.Length,sink.Finish());}
    private static bool CompareStream(Action<Stream> write,byte[] actual){using var sink=new CompareSink(actual);try{write(sink);return sink.Length==actual.Length;}catch(ComparisonMismatchException){return false;}}
    private abstract class WriteOnlyStream:Stream
    {public override bool CanRead=>false;public override bool CanSeek=>false;public override bool CanWrite=>true;public override long Position{get=>Length;set=>throw new NotSupportedException();}public override void Flush(){}public override int Read(byte[] b,int o,int c)=>throw new NotSupportedException();public override long Seek(long o,SeekOrigin so)=>throw new NotSupportedException();public override void SetLength(long l)=>throw new NotSupportedException();public override void Write(byte[] b,int o,int c)=>Write(b.AsSpan(o,c));}
    private sealed class CountingStream(long maximum=long.MaxValue):WriteOnlyStream
    {private long length;public override long Length=>length;internal void Add(long count){length=checked(length+count);if(length>maximum)throw new PreparationCapacityException();}public override void Write(ReadOnlySpan<byte> b)=>Add(b.Length);}
    private sealed class CappedStream(Stream target,long maximum):WriteOnlyStream
    {private long length;public override long Length=>length;public override void Write(ReadOnlySpan<byte> b){length=checked(length+b.Length);if(length>maximum)throw new PreparationCapacityException();target.Write(b);}}
    private sealed class HashStream:WriteOnlyStream
    {private readonly IncrementalHash hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);private long length;public override long Length=>length;public override void Write(ReadOnlySpan<byte>b){length=checked(length+b.Length);hash.AppendData(b);}internal string Finish()=>Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();protected override void Dispose(bool d){if(d)hash.Dispose();base.Dispose(d);}}
    private sealed class ComparisonMismatchException:Exception;
    private sealed class CompareSink(byte[] expected):WriteOnlyStream
    {private int offset;public override long Length=>offset;public override void Write(ReadOnlySpan<byte>b){if(b.Length>expected.Length-offset||!b.SequenceEqual(expected.AsSpan(offset,b.Length)))throw new ComparisonMismatchException();offset=checked(offset+b.Length);}}
    internal sealed record PBinding(string ObjectiveId, string GrantId, string GrantVersion, string TrainingRunId, string CandidateSha256, string ParentManifestSha256, string ParentWeightsSha256, string ConfigurationSha256, string ExamConditionSha256, PDescriptors DescriptorsV2);
    private static PBinding ReadObjectBinding(JsonElement e)
    {
        RequireObject(e, ["CandidateSha256", "ConfigurationSha256", "DescriptorsV2", "ExamConditionSha256", "GrantId", "GrantVersion", "ObjectiveId", "ParentManifestSha256", "ParentWeightsSha256", "TrainingRunId"]);
        return new(ReadText(e.GetProperty("ObjectiveId")), ReadText(e.GetProperty("GrantId")), ReadText(e.GetProperty("GrantVersion")), ReadText(e.GetProperty("TrainingRunId")), ReadText(e.GetProperty("CandidateSha256")), ReadText(e.GetProperty("ParentManifestSha256")), ReadText(e.GetProperty("ParentWeightsSha256")), ReadText(e.GetProperty("ConfigurationSha256")), ReadText(e.GetProperty("ExamConditionSha256")), ReadObjectDescriptors(e.GetProperty("DescriptorsV2")));
    }
    private static PBinding ReadTupleBinding(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 10);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]), atoms.Read(e[4]), atoms.Read(e[5]), atoms.Read(e[6]), atoms.Read(e[7]), atoms.Read(e[8]), ReadTupleDescriptors(e[9], atoms));
    }
    private static void WriteTupleBinding(Utf8JsonWriter w, PBinding v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.ObjectiveId));
        w.WriteNumberValue(atoms.Intern(v.GrantId));
        w.WriteNumberValue(atoms.Intern(v.GrantVersion));
        w.WriteNumberValue(atoms.Intern(v.TrainingRunId));
        w.WriteNumberValue(atoms.Intern(v.CandidateSha256));
        w.WriteNumberValue(atoms.Intern(v.ParentManifestSha256));
        w.WriteNumberValue(atoms.Intern(v.ParentWeightsSha256));
        w.WriteNumberValue(atoms.Intern(v.ConfigurationSha256));
        w.WriteNumberValue(atoms.Intern(v.ExamConditionSha256));
        WriteTupleDescriptors(w, v.DescriptorsV2, atoms);
        w.WriteEndArray();
    }
    private static void WriteCanonicalBinding(CanonicalSink w, PBinding v)
    {
        w.Raw("{");
        w.Raw(""); w.String("CandidateSha256"); w.Raw(":"); w.String(v.CandidateSha256);
        w.Raw(","); w.String("ConfigurationSha256"); w.Raw(":"); w.String(v.ConfigurationSha256);
        w.Raw(","); w.String("DescriptorsV2"); w.Raw(":"); WriteCanonicalDescriptors(w, v.DescriptorsV2);
        w.Raw(","); w.String("ExamConditionSha256"); w.Raw(":"); w.String(v.ExamConditionSha256);
        w.Raw(","); w.String("GrantId"); w.Raw(":"); w.String(v.GrantId);
        w.Raw(","); w.String("GrantVersion"); w.Raw(":"); w.String(v.GrantVersion);
        w.Raw(","); w.String("ObjectiveId"); w.Raw(":"); w.String(v.ObjectiveId);
        w.Raw(","); w.String("ParentManifestSha256"); w.Raw(":"); w.String(v.ParentManifestSha256);
        w.Raw(","); w.String("ParentWeightsSha256"); w.Raw(":"); w.String(v.ParentWeightsSha256);
        w.Raw(","); w.String("TrainingRunId"); w.Raw(":"); w.String(v.TrainingRunId);
        w.Raw("}");
    }
    internal sealed record PDescriptors(long SchemaVersion, string SourceSelectionSha256, string ParentCheckpointSha256, string CandidateSelectionSha256, string CorpusDescriptorSha256, string EvaluationPlanSha256, long CaseCount, long RepeatCount, string EvaluatorProfileSha256, string ProfileSelectionSha256);
    private static PDescriptors ReadObjectDescriptors(JsonElement e)
    {
        RequireObject(e, ["CandidateSelectionSha256", "CaseCount", "CorpusDescriptorSha256", "EvaluationPlanSha256", "EvaluatorProfileSha256", "ParentCheckpointSha256", "ProfileSelectionSha256", "RepeatCount", "SchemaVersion", "SourceSelectionSha256"]);
        return new(ReadNatural(e.GetProperty("SchemaVersion")), ReadText(e.GetProperty("SourceSelectionSha256")), ReadText(e.GetProperty("ParentCheckpointSha256")), ReadText(e.GetProperty("CandidateSelectionSha256")), ReadText(e.GetProperty("CorpusDescriptorSha256")), ReadText(e.GetProperty("EvaluationPlanSha256")), ReadNatural(e.GetProperty("CaseCount")), ReadNatural(e.GetProperty("RepeatCount")), ReadText(e.GetProperty("EvaluatorProfileSha256")), ReadText(e.GetProperty("ProfileSelectionSha256")));
    }
    private static PDescriptors ReadTupleDescriptors(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 10);
        return new(ReadNatural(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]), atoms.Read(e[4]), atoms.Read(e[5]), ReadNatural(e[6]), ReadNatural(e[7]), atoms.Read(e[8]), atoms.Read(e[9]));
    }
    private static void WriteTupleDescriptors(Utf8JsonWriter w, PDescriptors v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(v.SchemaVersion);
        w.WriteNumberValue(atoms.Intern(v.SourceSelectionSha256));
        w.WriteNumberValue(atoms.Intern(v.ParentCheckpointSha256));
        w.WriteNumberValue(atoms.Intern(v.CandidateSelectionSha256));
        w.WriteNumberValue(atoms.Intern(v.CorpusDescriptorSha256));
        w.WriteNumberValue(atoms.Intern(v.EvaluationPlanSha256));
        w.WriteNumberValue(v.CaseCount);
        w.WriteNumberValue(v.RepeatCount);
        w.WriteNumberValue(atoms.Intern(v.EvaluatorProfileSha256));
        w.WriteNumberValue(atoms.Intern(v.ProfileSelectionSha256));
        w.WriteEndArray();
    }
    private static void WriteCanonicalDescriptors(CanonicalSink w, PDescriptors v)
    {
        w.Raw("{");
        w.Raw(""); w.String("CandidateSelectionSha256"); w.Raw(":"); w.String(v.CandidateSelectionSha256);
        w.Raw(","); w.String("CaseCount"); w.Raw(":"); w.Number(v.CaseCount);
        w.Raw(","); w.String("CorpusDescriptorSha256"); w.Raw(":"); w.String(v.CorpusDescriptorSha256);
        w.Raw(","); w.String("EvaluationPlanSha256"); w.Raw(":"); w.String(v.EvaluationPlanSha256);
        w.Raw(","); w.String("EvaluatorProfileSha256"); w.Raw(":"); w.String(v.EvaluatorProfileSha256);
        w.Raw(","); w.String("ParentCheckpointSha256"); w.Raw(":"); w.String(v.ParentCheckpointSha256);
        w.Raw(","); w.String("ProfileSelectionSha256"); w.Raw(":"); w.String(v.ProfileSelectionSha256);
        w.Raw(","); w.String("RepeatCount"); w.Raw(":"); w.Number(v.RepeatCount);
        w.Raw(","); w.String("SchemaVersion"); w.Raw(":"); w.Number(v.SchemaVersion);
        w.Raw(","); w.String("SourceSelectionSha256"); w.Raw(":"); w.String(v.SourceSelectionSha256);
        w.Raw("}");
    }
    internal sealed record PInput(string Schema, string OriginalReceiptId, string OriginalReceiptSha256, string SourceSha256, string[] Categories, PGraph TeachingGraph);
    private static PInput ReadObjectInput(JsonElement e)
    {
        RequireObject(e, ["Categories", "OriginalReceiptId", "OriginalReceiptSha256", "Schema", "SourceSha256", "TeachingGraph"]);
        return new(ReadText(e.GetProperty("Schema")), ReadText(e.GetProperty("OriginalReceiptId")), ReadText(e.GetProperty("OriginalReceiptSha256")), ReadText(e.GetProperty("SourceSha256")), ReadArray(e.GetProperty("Categories"), x => ReadText(x)), ReadObjectGraph(e.GetProperty("TeachingGraph")));
    }
    private static PInput ReadTupleInput(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 6);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]), ReadArray(e[4], x => atoms.Read(x)), ReadTupleGraph(e[5], atoms));
    }
    private static void WriteTupleInput(Utf8JsonWriter w, PInput v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.Schema));
        w.WriteNumberValue(atoms.Intern(v.OriginalReceiptId));
        w.WriteNumberValue(atoms.Intern(v.OriginalReceiptSha256));
        w.WriteNumberValue(atoms.Intern(v.SourceSha256));
        WriteArray(w, v.Categories, x => w.WriteNumberValue(atoms.Intern(x)));
        WriteTupleGraph(w, v.TeachingGraph, atoms);
        w.WriteEndArray();
    }
    private static void WriteCanonicalInput(CanonicalSink w, PInput v)
    {
        w.Raw("{");
        w.Raw(""); w.String("Categories"); w.Raw(":"); WriteArray(w, v.Categories, x => w.String(x));
        w.Raw(","); w.String("OriginalReceiptId"); w.Raw(":"); w.String(v.OriginalReceiptId);
        w.Raw(","); w.String("OriginalReceiptSha256"); w.Raw(":"); w.String(v.OriginalReceiptSha256);
        w.Raw(","); w.String("Schema"); w.Raw(":"); w.String(v.Schema);
        w.Raw(","); w.String("SourceSha256"); w.Raw(":"); w.String(v.SourceSha256);
        w.Raw(","); w.String("TeachingGraph"); w.Raw(":"); WriteCanonicalGraph(w, v.TeachingGraph);
        w.Raw("}");
    }
    internal sealed record PGraph(PSelection Selection, PGraphSource[] Sources, PResolvedItem[] Items, PCategory[] CategoryBindings);
    private static PGraph ReadObjectGraph(JsonElement e)
    {
        RequireObject(e, ["CategoryBindings", "Items", "Selection", "Sources"]);
        return new(ReadObjectSelection(e.GetProperty("Selection")), ReadArray(e.GetProperty("Sources"), x => ReadObjectGraphSource(x)), ReadArray(e.GetProperty("Items"), x => ReadObjectResolvedItem(x)), ReadArray(e.GetProperty("CategoryBindings"), x => ReadObjectCategory(x)));
    }
    private static PGraph ReadTupleGraph(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 4);
        return new(ReadTupleSelection(e[0], atoms), ReadArray(e[1], x => ReadTupleGraphSource(x, atoms)), ReadArray(e[2], x => ReadTupleResolvedItem(x, atoms)), ReadArray(e[3], x => ReadTupleCategory(x, atoms)));
    }
    private static void WriteTupleGraph(Utf8JsonWriter w, PGraph v, AtomTable atoms)
    {
        w.WriteStartArray();
        WriteTupleSelection(w, v.Selection, atoms);
        WriteArray(w, v.Sources, x => WriteTupleGraphSource(w, x, atoms));
        WriteArray(w, v.Items, x => WriteTupleResolvedItem(w, x, atoms));
        WriteArray(w, v.CategoryBindings, x => WriteTupleCategory(w, x, atoms));
        w.WriteEndArray();
    }
    private static void WriteCanonicalGraph(CanonicalSink w, PGraph v)
    {
        w.Raw("{");
        w.Raw(""); w.String("CategoryBindings"); w.Raw(":"); WriteArray(w, v.CategoryBindings, x => WriteCanonicalCategory(w, x));
        w.Raw(","); w.String("Items"); w.Raw(":"); WriteArray(w, v.Items, x => WriteCanonicalResolvedItem(w, x));
        w.Raw(","); w.String("Selection"); w.Raw(":"); WriteCanonicalSelection(w, v.Selection);
        w.Raw(","); w.String("Sources"); w.Raw(":"); WriteArray(w, v.Sources, x => WriteCanonicalGraphSource(w, x));
        w.Raw("}");
    }
    internal sealed record PSelection(PTrainingSource[] Sources, PPair[] PairSelections, PCurriculum TeachingCurriculum);
    private static PSelection ReadObjectSelection(JsonElement e)
    {
        RequireObject(e, ["PairSelections", "Sources", "TeachingCurriculum"]);
        return new(ReadArray(e.GetProperty("Sources"), x => ReadObjectTrainingSource(x)), ReadArray(e.GetProperty("PairSelections"), x => ReadObjectPair(x)), ReadObjectCurriculum(e.GetProperty("TeachingCurriculum")));
    }
    private static PSelection ReadTupleSelection(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 3);
        return new(ReadArray(e[0], x => ReadTupleTrainingSource(x, atoms)), ReadArray(e[1], x => ReadTuplePair(x, atoms)), ReadTupleCurriculum(e[2], atoms));
    }
    private static void WriteTupleSelection(Utf8JsonWriter w, PSelection v, AtomTable atoms)
    {
        w.WriteStartArray();
        WriteArray(w, v.Sources, x => WriteTupleTrainingSource(w, x, atoms));
        WriteArray(w, v.PairSelections, x => WriteTuplePair(w, x, atoms));
        WriteTupleCurriculum(w, v.TeachingCurriculum, atoms);
        w.WriteEndArray();
    }
    private static void WriteCanonicalSelection(CanonicalSink w, PSelection v)
    {
        w.Raw("{");
        w.Raw(""); w.String("PairSelections"); w.Raw(":"); WriteArray(w, v.PairSelections, x => WriteCanonicalPair(w, x));
        w.Raw(","); w.String("Sources"); w.Raw(":"); WriteArray(w, v.Sources, x => WriteCanonicalTrainingSource(w, x));
        w.Raw(","); w.String("TeachingCurriculum"); w.Raw(":"); WriteCanonicalCurriculum(w, v.TeachingCurriculum);
        w.Raw("}");
    }
    internal sealed record PTrainingSource(string SourceId, string Role, string DomainSlug, string DesignPatternSlug, string DatasetVersion, string ManifestFileSha256, long ManifestFileByteLength, string SealedManifestSha256);
    private static PTrainingSource ReadObjectTrainingSource(JsonElement e)
    {
        RequireObject(e, ["DatasetVersion", "DesignPatternSlug", "DomainSlug", "ManifestFileByteLength", "ManifestFileSha256", "Role", "SealedManifestSha256", "SourceId"]);
        return new(ReadText(e.GetProperty("SourceId")), ReadText(e.GetProperty("Role")), ReadText(e.GetProperty("DomainSlug")), ReadText(e.GetProperty("DesignPatternSlug")), ReadText(e.GetProperty("DatasetVersion")), ReadText(e.GetProperty("ManifestFileSha256")), ReadNatural(e.GetProperty("ManifestFileByteLength")), ReadText(e.GetProperty("SealedManifestSha256")));
    }
    private static PTrainingSource ReadTupleTrainingSource(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 8);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]), atoms.Read(e[4]), atoms.Read(e[5]), ReadNatural(e[6]), atoms.Read(e[7]));
    }
    private static void WriteTupleTrainingSource(Utf8JsonWriter w, PTrainingSource v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.SourceId));
        w.WriteNumberValue(atoms.Intern(v.Role));
        w.WriteNumberValue(atoms.Intern(v.DomainSlug));
        w.WriteNumberValue(atoms.Intern(v.DesignPatternSlug));
        w.WriteNumberValue(atoms.Intern(v.DatasetVersion));
        w.WriteNumberValue(atoms.Intern(v.ManifestFileSha256));
        w.WriteNumberValue(v.ManifestFileByteLength);
        w.WriteNumberValue(atoms.Intern(v.SealedManifestSha256));
        w.WriteEndArray();
    }
    private static void WriteCanonicalTrainingSource(CanonicalSink w, PTrainingSource v)
    {
        w.Raw("{");
        w.Raw(""); w.String("DatasetVersion"); w.Raw(":"); w.String(v.DatasetVersion);
        w.Raw(","); w.String("DesignPatternSlug"); w.Raw(":"); w.String(v.DesignPatternSlug);
        w.Raw(","); w.String("DomainSlug"); w.Raw(":"); w.String(v.DomainSlug);
        w.Raw(","); w.String("ManifestFileByteLength"); w.Raw(":"); w.Number(v.ManifestFileByteLength);
        w.Raw(","); w.String("ManifestFileSha256"); w.Raw(":"); w.String(v.ManifestFileSha256);
        w.Raw(","); w.String("Role"); w.Raw(":"); w.String(v.Role);
        w.Raw(","); w.String("SealedManifestSha256"); w.Raw(":"); w.String(v.SealedManifestSha256);
        w.Raw(","); w.String("SourceId"); w.Raw(":"); w.String(v.SourceId);
        w.Raw("}");
    }
    internal sealed record PPair(string TrainingPairId, string SourceId);
    private static PPair ReadObjectPair(JsonElement e)
    {
        RequireObject(e, ["SourceId", "TrainingPairId"]);
        return new(ReadText(e.GetProperty("TrainingPairId")), ReadText(e.GetProperty("SourceId")));
    }
    private static PPair ReadTuplePair(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 2);
        return new(atoms.Read(e[0]), atoms.Read(e[1]));
    }
    private static void WriteTuplePair(Utf8JsonWriter w, PPair v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.TrainingPairId));
        w.WriteNumberValue(atoms.Intern(v.SourceId));
        w.WriteEndArray();
    }
    private static void WriteCanonicalPair(CanonicalSink w, PPair v)
    {
        w.Raw("{");
        w.Raw(""); w.String("SourceId"); w.Raw(":"); w.String(v.SourceId);
        w.Raw(","); w.String("TrainingPairId"); w.Raw(":"); w.String(v.TrainingPairId);
        w.Raw("}");
    }
    internal sealed record PCurriculum(string PolicyId, string PolicyVersion, PDescriptor[] Sources, PItem[] Items, PCategory[] CategoryBindings);
    private static PCurriculum ReadObjectCurriculum(JsonElement e)
    {
        RequireObject(e, ["CategoryBindings", "Items", "PolicyId", "PolicyVersion", "Sources"]);
        return new(ReadText(e.GetProperty("PolicyId")), ReadText(e.GetProperty("PolicyVersion")), ReadArray(e.GetProperty("Sources"), x => ReadObjectDescriptor(x)), ReadArray(e.GetProperty("Items"), x => ReadObjectItem(x)), ReadArray(e.GetProperty("CategoryBindings"), x => ReadObjectCategory(x)));
    }
    private static PCurriculum ReadTupleCurriculum(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 5);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), ReadArray(e[2], x => ReadTupleDescriptor(x, atoms)), ReadArray(e[3], x => ReadTupleItem(x, atoms)), ReadArray(e[4], x => ReadTupleCategory(x, atoms)));
    }
    private static void WriteTupleCurriculum(Utf8JsonWriter w, PCurriculum v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.PolicyId));
        w.WriteNumberValue(atoms.Intern(v.PolicyVersion));
        WriteArray(w, v.Sources, x => WriteTupleDescriptor(w, x, atoms));
        WriteArray(w, v.Items, x => WriteTupleItem(w, x, atoms));
        WriteArray(w, v.CategoryBindings, x => WriteTupleCategory(w, x, atoms));
        w.WriteEndArray();
    }
    private static void WriteCanonicalCurriculum(CanonicalSink w, PCurriculum v)
    {
        w.Raw("{");
        w.Raw(""); w.String("CategoryBindings"); w.Raw(":"); WriteArray(w, v.CategoryBindings, x => WriteCanonicalCategory(w, x));
        w.Raw(","); w.String("Items"); w.Raw(":"); WriteArray(w, v.Items, x => WriteCanonicalItem(w, x));
        w.Raw(","); w.String("PolicyId"); w.Raw(":"); w.String(v.PolicyId);
        w.Raw(","); w.String("PolicyVersion"); w.Raw(":"); w.String(v.PolicyVersion);
        w.Raw(","); w.String("Sources"); w.Raw(":"); WriteArray(w, v.Sources, x => WriteCanonicalDescriptor(w, x));
        w.Raw("}");
    }
    internal sealed record PDescriptor(string SourceId, string CorpusId, string DomainSlug, string SubjectSlug, string CorpusVersion, string ManifestFileSha256, long ManifestFileByteLength, string CorpusSha256, string PayloadSha256, long PayloadByteLength);
    private static PDescriptor ReadObjectDescriptor(JsonElement e)
    {
        RequireObject(e, ["CorpusId", "CorpusSha256", "CorpusVersion", "DomainSlug", "ManifestFileByteLength", "ManifestFileSha256", "PayloadByteLength", "PayloadSha256", "SourceId", "SubjectSlug"]);
        return new(ReadText(e.GetProperty("SourceId")), ReadText(e.GetProperty("CorpusId")), ReadText(e.GetProperty("DomainSlug")), ReadText(e.GetProperty("SubjectSlug")), ReadText(e.GetProperty("CorpusVersion")), ReadText(e.GetProperty("ManifestFileSha256")), ReadNatural(e.GetProperty("ManifestFileByteLength")), ReadText(e.GetProperty("CorpusSha256")), ReadText(e.GetProperty("PayloadSha256")), ReadNatural(e.GetProperty("PayloadByteLength")));
    }
    private static PDescriptor ReadTupleDescriptor(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 10);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]), atoms.Read(e[4]), atoms.Read(e[5]), ReadNatural(e[6]), atoms.Read(e[7]), atoms.Read(e[8]), ReadNatural(e[9]));
    }
    private static void WriteTupleDescriptor(Utf8JsonWriter w, PDescriptor v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.SourceId));
        w.WriteNumberValue(atoms.Intern(v.CorpusId));
        w.WriteNumberValue(atoms.Intern(v.DomainSlug));
        w.WriteNumberValue(atoms.Intern(v.SubjectSlug));
        w.WriteNumberValue(atoms.Intern(v.CorpusVersion));
        w.WriteNumberValue(atoms.Intern(v.ManifestFileSha256));
        w.WriteNumberValue(v.ManifestFileByteLength);
        w.WriteNumberValue(atoms.Intern(v.CorpusSha256));
        w.WriteNumberValue(atoms.Intern(v.PayloadSha256));
        w.WriteNumberValue(v.PayloadByteLength);
        w.WriteEndArray();
    }
    private static void WriteCanonicalDescriptor(CanonicalSink w, PDescriptor v)
    {
        w.Raw("{");
        w.Raw(""); w.String("CorpusId"); w.Raw(":"); w.String(v.CorpusId);
        w.Raw(","); w.String("CorpusSha256"); w.Raw(":"); w.String(v.CorpusSha256);
        w.Raw(","); w.String("CorpusVersion"); w.Raw(":"); w.String(v.CorpusVersion);
        w.Raw(","); w.String("DomainSlug"); w.Raw(":"); w.String(v.DomainSlug);
        w.Raw(","); w.String("ManifestFileByteLength"); w.Raw(":"); w.Number(v.ManifestFileByteLength);
        w.Raw(","); w.String("ManifestFileSha256"); w.Raw(":"); w.String(v.ManifestFileSha256);
        w.Raw(","); w.String("PayloadByteLength"); w.Raw(":"); w.Number(v.PayloadByteLength);
        w.Raw(","); w.String("PayloadSha256"); w.Raw(":"); w.String(v.PayloadSha256);
        w.Raw(","); w.String("SourceId"); w.Raw(":"); w.String(v.SourceId);
        w.Raw(","); w.String("SubjectSlug"); w.Raw(":"); w.String(v.SubjectSlug);
        w.Raw("}");
    }
    internal sealed record PItem(string ItemId, string SourceId, string ReferenceEntryId, string SourceEntityFingerprint);
    private static PItem ReadObjectItem(JsonElement e)
    {
        RequireObject(e, ["ItemId", "ReferenceEntryId", "SourceEntityFingerprint", "SourceId"]);
        return new(ReadText(e.GetProperty("ItemId")), ReadText(e.GetProperty("SourceId")), ReadText(e.GetProperty("ReferenceEntryId")), ReadText(e.GetProperty("SourceEntityFingerprint")));
    }
    private static PItem ReadTupleItem(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 4);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]));
    }
    private static void WriteTupleItem(Utf8JsonWriter w, PItem v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.ItemId));
        w.WriteNumberValue(atoms.Intern(v.SourceId));
        w.WriteNumberValue(atoms.Intern(v.ReferenceEntryId));
        w.WriteNumberValue(atoms.Intern(v.SourceEntityFingerprint));
        w.WriteEndArray();
    }
    private static void WriteCanonicalItem(CanonicalSink w, PItem v)
    {
        w.Raw("{");
        w.Raw(""); w.String("ItemId"); w.Raw(":"); w.String(v.ItemId);
        w.Raw(","); w.String("ReferenceEntryId"); w.Raw(":"); w.String(v.ReferenceEntryId);
        w.Raw(","); w.String("SourceEntityFingerprint"); w.Raw(":"); w.String(v.SourceEntityFingerprint);
        w.Raw(","); w.String("SourceId"); w.Raw(":"); w.String(v.SourceId);
        w.Raw("}");
    }
    internal sealed record PCategory(string Category, string[] OrderedItemIds);
    private static PCategory ReadObjectCategory(JsonElement e)
    {
        RequireObject(e, ["Category", "OrderedItemIds"]);
        return new(ReadText(e.GetProperty("Category")), ReadArray(e.GetProperty("OrderedItemIds"), x => ReadText(x)));
    }
    private static PCategory ReadTupleCategory(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 2);
        return new(atoms.Read(e[0]), ReadArray(e[1], x => atoms.Read(x)));
    }
    private static void WriteTupleCategory(Utf8JsonWriter w, PCategory v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.Category));
        WriteArray(w, v.OrderedItemIds, x => w.WriteNumberValue(atoms.Intern(x)));
        w.WriteEndArray();
    }
    private static void WriteCanonicalCategory(CanonicalSink w, PCategory v)
    {
        w.Raw("{");
        w.Raw(""); w.String("Category"); w.Raw(":"); w.String(v.Category);
        w.Raw(","); w.String("OrderedItemIds"); w.Raw(":"); WriteArray(w, v.OrderedItemIds, x => w.String(x));
        w.Raw("}");
    }
    internal sealed record PGraphSource(PDescriptor Descriptor, PManifest Manifest, string ManifestFileSha256, long ManifestFileByteLength, PEntry[] Entries);
    private static PGraphSource ReadObjectGraphSource(JsonElement e)
    {
        RequireObject(e, ["Descriptor", "Entries", "Manifest", "ManifestFileByteLength", "ManifestFileSha256"]);
        return new(ReadObjectDescriptor(e.GetProperty("Descriptor")), ReadObjectManifest(e.GetProperty("Manifest")), ReadText(e.GetProperty("ManifestFileSha256")), ReadNatural(e.GetProperty("ManifestFileByteLength")), ReadArray(e.GetProperty("Entries"), x => ReadObjectEntry(x)));
    }
    private static PGraphSource ReadTupleGraphSource(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 5);
        return new(ReadTupleDescriptor(e[0], atoms), ReadTupleManifest(e[1], atoms), atoms.Read(e[2]), ReadNatural(e[3]), ReadArray(e[4], x => ReadTupleEntry(x, atoms)));
    }
    private static void WriteTupleGraphSource(Utf8JsonWriter w, PGraphSource v, AtomTable atoms)
    {
        w.WriteStartArray();
        WriteTupleDescriptor(w, v.Descriptor, atoms);
        WriteTupleManifest(w, v.Manifest, atoms);
        w.WriteNumberValue(atoms.Intern(v.ManifestFileSha256));
        w.WriteNumberValue(v.ManifestFileByteLength);
        WriteArray(w, v.Entries, x => WriteTupleEntry(w, x, atoms));
        w.WriteEndArray();
    }
    private static void WriteCanonicalGraphSource(CanonicalSink w, PGraphSource v)
    {
        w.Raw("{");
        w.Raw(""); w.String("Descriptor"); w.Raw(":"); WriteCanonicalDescriptor(w, v.Descriptor);
        w.Raw(","); w.String("Entries"); w.Raw(":"); WriteArray(w, v.Entries, x => WriteCanonicalEntry(w, x));
        w.Raw(","); w.String("Manifest"); w.Raw(":"); WriteCanonicalManifest(w, v.Manifest);
        w.Raw(","); w.String("ManifestFileByteLength"); w.Raw(":"); w.Number(v.ManifestFileByteLength);
        w.Raw(","); w.String("ManifestFileSha256"); w.Raw(":"); w.String(v.ManifestFileSha256);
        w.Raw("}");
    }
    internal sealed record PManifest(string Schema, string Context, string Domain, string Subject, string CorpusVersion, string CorpusName, string FrozenAtUtc, long EntryCount, string CorpusSha256, string RepositoryRelativeCorpusDirectory, string RepositoryRelativePayloadPath, long PayloadByteLength, string PayloadSha256, string RepositoryRelativeManifestPath, string ManifestSha256);
    private static PManifest ReadObjectManifest(JsonElement e)
    {
        RequireObject(e, ["Context", "CorpusName", "CorpusSha256", "CorpusVersion", "Domain", "EntryCount", "FrozenAtUtc", "ManifestSha256", "PayloadByteLength", "PayloadSha256", "RepositoryRelativeCorpusDirectory", "RepositoryRelativeManifestPath", "RepositoryRelativePayloadPath", "Schema", "Subject"]);
        return new(ReadText(e.GetProperty("Schema")), ReadText(e.GetProperty("Context")), ReadText(e.GetProperty("Domain")), ReadText(e.GetProperty("Subject")), ReadText(e.GetProperty("CorpusVersion")), ReadText(e.GetProperty("CorpusName")), ReadText(e.GetProperty("FrozenAtUtc")), ReadNatural(e.GetProperty("EntryCount")), ReadText(e.GetProperty("CorpusSha256")), ReadText(e.GetProperty("RepositoryRelativeCorpusDirectory")), ReadText(e.GetProperty("RepositoryRelativePayloadPath")), ReadNatural(e.GetProperty("PayloadByteLength")), ReadText(e.GetProperty("PayloadSha256")), ReadText(e.GetProperty("RepositoryRelativeManifestPath")), ReadText(e.GetProperty("ManifestSha256")));
    }
    private static PManifest ReadTupleManifest(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 15);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]), atoms.Read(e[4]), atoms.Read(e[5]), atoms.Read(e[6]), ReadNatural(e[7]), atoms.Read(e[8]), atoms.Read(e[9]), atoms.Read(e[10]), ReadNatural(e[11]), atoms.Read(e[12]), atoms.Read(e[13]), atoms.Read(e[14]));
    }
    private static void WriteTupleManifest(Utf8JsonWriter w, PManifest v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.Schema));
        w.WriteNumberValue(atoms.Intern(v.Context));
        w.WriteNumberValue(atoms.Intern(v.Domain));
        w.WriteNumberValue(atoms.Intern(v.Subject));
        w.WriteNumberValue(atoms.Intern(v.CorpusVersion));
        w.WriteNumberValue(atoms.Intern(v.CorpusName));
        w.WriteNumberValue(atoms.Intern(v.FrozenAtUtc));
        w.WriteNumberValue(v.EntryCount);
        w.WriteNumberValue(atoms.Intern(v.CorpusSha256));
        w.WriteNumberValue(atoms.Intern(v.RepositoryRelativeCorpusDirectory));
        w.WriteNumberValue(atoms.Intern(v.RepositoryRelativePayloadPath));
        w.WriteNumberValue(v.PayloadByteLength);
        w.WriteNumberValue(atoms.Intern(v.PayloadSha256));
        w.WriteNumberValue(atoms.Intern(v.RepositoryRelativeManifestPath));
        w.WriteNumberValue(atoms.Intern(v.ManifestSha256));
        w.WriteEndArray();
    }
    private static void WriteCanonicalManifest(CanonicalSink w, PManifest v)
    {
        w.Raw("{");
        w.Raw(""); w.String("Context"); w.Raw(":"); w.String(v.Context);
        w.Raw(","); w.String("CorpusName"); w.Raw(":"); w.String(v.CorpusName);
        w.Raw(","); w.String("CorpusSha256"); w.Raw(":"); w.String(v.CorpusSha256);
        w.Raw(","); w.String("CorpusVersion"); w.Raw(":"); w.String(v.CorpusVersion);
        w.Raw(","); w.String("Domain"); w.Raw(":"); w.String(v.Domain);
        w.Raw(","); w.String("EntryCount"); w.Raw(":"); w.Number(v.EntryCount);
        w.Raw(","); w.String("FrozenAtUtc"); w.Raw(":"); w.String(v.FrozenAtUtc);
        w.Raw(","); w.String("ManifestSha256"); w.Raw(":"); w.String(v.ManifestSha256);
        w.Raw(","); w.String("PayloadByteLength"); w.Raw(":"); w.Number(v.PayloadByteLength);
        w.Raw(","); w.String("PayloadSha256"); w.Raw(":"); w.String(v.PayloadSha256);
        w.Raw(","); w.String("RepositoryRelativeCorpusDirectory"); w.Raw(":"); w.String(v.RepositoryRelativeCorpusDirectory);
        w.Raw(","); w.String("RepositoryRelativeManifestPath"); w.Raw(":"); w.String(v.RepositoryRelativeManifestPath);
        w.Raw(","); w.String("RepositoryRelativePayloadPath"); w.Raw(":"); w.String(v.RepositoryRelativePayloadPath);
        w.Raw(","); w.String("Schema"); w.Raw(":"); w.String(v.Schema);
        w.Raw(","); w.String("Subject"); w.Raw(":"); w.String(v.Subject);
        w.Raw("}");
    }
    internal sealed record PEntry(string EntryId, string SourceEntityFingerprint, string DomainTag, string SubjectId, string Partition, string OwnerInputSha256, PProvenance PaidReferenceProvenance);
    private static PEntry ReadObjectEntry(JsonElement e)
    {
        RequireObject(e, ["DomainTag", "EntryId", "OwnerInputSha256", "PaidReferenceProvenance", "Partition", "SourceEntityFingerprint", "SubjectId"]);
        return new(ReadText(e.GetProperty("EntryId")), ReadText(e.GetProperty("SourceEntityFingerprint")), ReadText(e.GetProperty("DomainTag")), ReadText(e.GetProperty("SubjectId")), ReadText(e.GetProperty("Partition")), ReadText(e.GetProperty("OwnerInputSha256")), ReadObjectProvenance(e.GetProperty("PaidReferenceProvenance")));
    }
    private static PEntry ReadTupleEntry(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 7);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]), atoms.Read(e[4]), atoms.Read(e[5]), ReadTupleProvenance(e[6], atoms));
    }
    private static void WriteTupleEntry(Utf8JsonWriter w, PEntry v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.EntryId));
        w.WriteNumberValue(atoms.Intern(v.SourceEntityFingerprint));
        w.WriteNumberValue(atoms.Intern(v.DomainTag));
        w.WriteNumberValue(atoms.Intern(v.SubjectId));
        w.WriteNumberValue(atoms.Intern(v.Partition));
        w.WriteNumberValue(atoms.Intern(v.OwnerInputSha256));
        WriteTupleProvenance(w, v.PaidReferenceProvenance, atoms);
        w.WriteEndArray();
    }
    private static void WriteCanonicalEntry(CanonicalSink w, PEntry v)
    {
        w.Raw("{");
        w.Raw(""); w.String("DomainTag"); w.Raw(":"); w.String(v.DomainTag);
        w.Raw(","); w.String("EntryId"); w.Raw(":"); w.String(v.EntryId);
        w.Raw(","); w.String("OwnerInputSha256"); w.Raw(":"); w.String(v.OwnerInputSha256);
        w.Raw(","); w.String("PaidReferenceProvenance"); w.Raw(":"); WriteCanonicalProvenance(w, v.PaidReferenceProvenance);
        w.Raw(","); w.String("Partition"); w.Raw(":"); w.String(v.Partition);
        w.Raw(","); w.String("SourceEntityFingerprint"); w.Raw(":"); w.String(v.SourceEntityFingerprint);
        w.Raw(","); w.String("SubjectId"); w.Raw(":"); w.String(v.SubjectId);
        w.Raw("}");
    }
    internal sealed record PProvenance(string ProviderRef, string ModelId, string ModelVersion, string RequestId, string CapturedAtUtc, string ReferenceAnswerSha256, string CanonicalRepresentation, string ProvenanceSha256);
    private static PProvenance ReadObjectProvenance(JsonElement e)
    {
        RequireObject(e, ["CanonicalRepresentation", "CapturedAtUtc", "ModelId", "ModelVersion", "ProvenanceSha256", "ProviderRef", "ReferenceAnswerSha256", "RequestId"]);
        return new(ReadText(e.GetProperty("ProviderRef")), ReadText(e.GetProperty("ModelId")), ReadText(e.GetProperty("ModelVersion")), ReadText(e.GetProperty("RequestId")), ReadText(e.GetProperty("CapturedAtUtc")), ReadText(e.GetProperty("ReferenceAnswerSha256")), ReadText(e.GetProperty("CanonicalRepresentation")), ReadText(e.GetProperty("ProvenanceSha256")));
    }
    private static PProvenance ReadTupleProvenance(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 8);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]), atoms.Read(e[4]), atoms.Read(e[5]), atoms.Read(e[6]), atoms.Read(e[7]));
    }
    private static void WriteTupleProvenance(Utf8JsonWriter w, PProvenance v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.ProviderRef));
        w.WriteNumberValue(atoms.Intern(v.ModelId));
        w.WriteNumberValue(atoms.Intern(v.ModelVersion));
        w.WriteNumberValue(atoms.Intern(v.RequestId));
        w.WriteNumberValue(atoms.Intern(v.CapturedAtUtc));
        w.WriteNumberValue(atoms.Intern(v.ReferenceAnswerSha256));
        w.WriteNumberValue(atoms.Intern(v.CanonicalRepresentation));
        w.WriteNumberValue(atoms.Intern(v.ProvenanceSha256));
        w.WriteEndArray();
    }
    private static void WriteCanonicalProvenance(CanonicalSink w, PProvenance v)
    {
        w.Raw("{");
        w.Raw(""); w.String("CanonicalRepresentation"); w.Raw(":"); w.String(v.CanonicalRepresentation);
        w.Raw(","); w.String("CapturedAtUtc"); w.Raw(":"); w.String(v.CapturedAtUtc);
        w.Raw(","); w.String("ModelId"); w.Raw(":"); w.String(v.ModelId);
        w.Raw(","); w.String("ModelVersion"); w.Raw(":"); w.String(v.ModelVersion);
        w.Raw(","); w.String("ProvenanceSha256"); w.Raw(":"); w.String(v.ProvenanceSha256);
        w.Raw(","); w.String("ProviderRef"); w.Raw(":"); w.String(v.ProviderRef);
        w.Raw(","); w.String("ReferenceAnswerSha256"); w.Raw(":"); w.String(v.ReferenceAnswerSha256);
        w.Raw(","); w.String("RequestId"); w.Raw(":"); w.String(v.RequestId);
        w.Raw("}");
    }
    internal sealed record PResolvedItem(PItem DeclaredItem, string SourceId, string ReferenceEntryId, string SourceEntityFingerprint);
    private static PResolvedItem ReadObjectResolvedItem(JsonElement e)
    {
        RequireObject(e, ["DeclaredItem", "ReferenceEntryId", "SourceEntityFingerprint", "SourceId"]);
        return new(ReadObjectItem(e.GetProperty("DeclaredItem")), ReadText(e.GetProperty("SourceId")), ReadText(e.GetProperty("ReferenceEntryId")), ReadText(e.GetProperty("SourceEntityFingerprint")));
    }
    private static PResolvedItem ReadTupleResolvedItem(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 4);
        return new(ReadTupleItem(e[0], atoms), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]));
    }
    private static void WriteTupleResolvedItem(Utf8JsonWriter w, PResolvedItem v, AtomTable atoms)
    {
        w.WriteStartArray();
        WriteTupleItem(w, v.DeclaredItem, atoms);
        w.WriteNumberValue(atoms.Intern(v.SourceId));
        w.WriteNumberValue(atoms.Intern(v.ReferenceEntryId));
        w.WriteNumberValue(atoms.Intern(v.SourceEntityFingerprint));
        w.WriteEndArray();
    }
    private static void WriteCanonicalResolvedItem(CanonicalSink w, PResolvedItem v)
    {
        w.Raw("{");
        w.Raw(""); w.String("DeclaredItem"); w.Raw(":"); WriteCanonicalItem(w, v.DeclaredItem);
        w.Raw(","); w.String("ReferenceEntryId"); w.Raw(":"); w.String(v.ReferenceEntryId);
        w.Raw(","); w.String("SourceEntityFingerprint"); w.Raw(":"); w.String(v.SourceEntityFingerprint);
        w.Raw(","); w.String("SourceId"); w.Raw(":"); w.String(v.SourceId);
        w.Raw("}");
    }
    internal sealed record PApproval(long SchemaVersion, string ApprovalId, long ApprovalRevision, string GrantId, string GrantVersion, string ApproverId, string ApprovedAtUtc, string OperatorDecision, string Use, string Recipient, string Model, string ReasoningEffort, PEvidence[] Evidence, PSelection SourceSelection, PApprovedSource[] AllTeachingSources);
    private static PApproval ReadObjectApproval(JsonElement e)
    {
        RequireObject(e, ["AllTeachingSources", "ApprovalId", "ApprovalRevision", "ApprovedAtUtc", "ApproverId", "Evidence", "GrantId", "GrantVersion", "Model", "OperatorDecision", "ReasoningEffort", "Recipient", "SchemaVersion", "SourceSelection", "Use"]);
        return new(ReadNatural(e.GetProperty("SchemaVersion")), ReadText(e.GetProperty("ApprovalId")), ReadNatural(e.GetProperty("ApprovalRevision")), ReadText(e.GetProperty("GrantId")), ReadText(e.GetProperty("GrantVersion")), ReadText(e.GetProperty("ApproverId")), ReadText(e.GetProperty("ApprovedAtUtc")), ReadText(e.GetProperty("OperatorDecision")), ReadText(e.GetProperty("Use")), ReadText(e.GetProperty("Recipient")), ReadText(e.GetProperty("Model")), ReadText(e.GetProperty("ReasoningEffort")), ReadArray(e.GetProperty("Evidence"), x => ReadObjectEvidence(x)), ReadObjectSelection(e.GetProperty("SourceSelection")), ReadArray(e.GetProperty("AllTeachingSources"), x => ReadObjectApprovedSource(x)));
    }
    private static PApproval ReadTupleApproval(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 15);
        return new(ReadNatural(e[0]), atoms.Read(e[1]), ReadNatural(e[2]), atoms.Read(e[3]), atoms.Read(e[4]), atoms.Read(e[5]), atoms.Read(e[6]), atoms.Read(e[7]), atoms.Read(e[8]), atoms.Read(e[9]), atoms.Read(e[10]), atoms.Read(e[11]), ReadArray(e[12], x => ReadTupleEvidence(x, atoms)), ReadTupleSelection(e[13], atoms), ReadArray(e[14], x => ReadTupleApprovedSource(x, atoms)));
    }
    private static void WriteTupleApproval(Utf8JsonWriter w, PApproval v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(v.SchemaVersion);
        w.WriteNumberValue(atoms.Intern(v.ApprovalId));
        w.WriteNumberValue(v.ApprovalRevision);
        w.WriteNumberValue(atoms.Intern(v.GrantId));
        w.WriteNumberValue(atoms.Intern(v.GrantVersion));
        w.WriteNumberValue(atoms.Intern(v.ApproverId));
        w.WriteNumberValue(atoms.Intern(v.ApprovedAtUtc));
        w.WriteNumberValue(atoms.Intern(v.OperatorDecision));
        w.WriteNumberValue(atoms.Intern(v.Use));
        w.WriteNumberValue(atoms.Intern(v.Recipient));
        w.WriteNumberValue(atoms.Intern(v.Model));
        w.WriteNumberValue(atoms.Intern(v.ReasoningEffort));
        WriteArray(w, v.Evidence, x => WriteTupleEvidence(w, x, atoms));
        WriteTupleSelection(w, v.SourceSelection, atoms);
        WriteArray(w, v.AllTeachingSources, x => WriteTupleApprovedSource(w, x, atoms));
        w.WriteEndArray();
    }
    private static void WriteCanonicalApproval(CanonicalSink w, PApproval v)
    {
        w.Raw("{");
        w.Raw(""); w.String("AllTeachingSources"); w.Raw(":"); WriteArray(w, v.AllTeachingSources, x => WriteCanonicalApprovedSource(w, x));
        w.Raw(","); w.String("ApprovalId"); w.Raw(":"); w.String(v.ApprovalId);
        w.Raw(","); w.String("ApprovalRevision"); w.Raw(":"); w.Number(v.ApprovalRevision);
        w.Raw(","); w.String("ApprovedAtUtc"); w.Raw(":"); w.String(v.ApprovedAtUtc);
        w.Raw(","); w.String("ApproverId"); w.Raw(":"); w.String(v.ApproverId);
        w.Raw(","); w.String("Evidence"); w.Raw(":"); WriteArray(w, v.Evidence, x => WriteCanonicalEvidence(w, x));
        w.Raw(","); w.String("GrantId"); w.Raw(":"); w.String(v.GrantId);
        w.Raw(","); w.String("GrantVersion"); w.Raw(":"); w.String(v.GrantVersion);
        w.Raw(","); w.String("Model"); w.Raw(":"); w.String(v.Model);
        w.Raw(","); w.String("OperatorDecision"); w.Raw(":"); w.String(v.OperatorDecision);
        w.Raw(","); w.String("ReasoningEffort"); w.Raw(":"); w.String(v.ReasoningEffort);
        w.Raw(","); w.String("Recipient"); w.Raw(":"); w.String(v.Recipient);
        w.Raw(","); w.String("SchemaVersion"); w.Raw(":"); w.Number(v.SchemaVersion);
        w.Raw(","); w.String("SourceSelection"); w.Raw(":"); WriteCanonicalSelection(w, v.SourceSelection);
        w.Raw(","); w.String("Use"); w.Raw(":"); w.String(v.Use);
        w.Raw("}");
    }
    internal sealed record PEvidence(string EvidenceRef, string EvidenceSha256);
    private static PEvidence ReadObjectEvidence(JsonElement e)
    {
        RequireObject(e, ["EvidenceRef", "EvidenceSha256"]);
        return new(ReadText(e.GetProperty("EvidenceRef")), ReadText(e.GetProperty("EvidenceSha256")));
    }
    private static PEvidence ReadTupleEvidence(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 2);
        return new(atoms.Read(e[0]), atoms.Read(e[1]));
    }
    private static void WriteTupleEvidence(Utf8JsonWriter w, PEvidence v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.EvidenceRef));
        w.WriteNumberValue(atoms.Intern(v.EvidenceSha256));
        w.WriteEndArray();
    }
    private static void WriteCanonicalEvidence(CanonicalSink w, PEvidence v)
    {
        w.Raw("{");
        w.Raw(""); w.String("EvidenceRef"); w.Raw(":"); w.String(v.EvidenceRef);
        w.Raw(","); w.String("EvidenceSha256"); w.Raw(":"); w.String(v.EvidenceSha256);
        w.Raw("}");
    }
    internal sealed record PApprovedSource(PDescriptor Descriptor, PApprovedEntry[] Entries);
    private static PApprovedSource ReadObjectApprovedSource(JsonElement e)
    {
        RequireObject(e, ["Descriptor", "Entries"]);
        return new(ReadObjectDescriptor(e.GetProperty("Descriptor")), ReadArray(e.GetProperty("Entries"), x => ReadObjectApprovedEntry(x)));
    }
    private static PApprovedSource ReadTupleApprovedSource(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 2);
        return new(ReadTupleDescriptor(e[0], atoms), ReadArray(e[1], x => ReadTupleApprovedEntry(x, atoms)));
    }
    private static void WriteTupleApprovedSource(Utf8JsonWriter w, PApprovedSource v, AtomTable atoms)
    {
        w.WriteStartArray();
        WriteTupleDescriptor(w, v.Descriptor, atoms);
        WriteArray(w, v.Entries, x => WriteTupleApprovedEntry(w, x, atoms));
        w.WriteEndArray();
    }
    private static void WriteCanonicalApprovedSource(CanonicalSink w, PApprovedSource v)
    {
        w.Raw("{");
        w.Raw(""); w.String("Descriptor"); w.Raw(":"); WriteCanonicalDescriptor(w, v.Descriptor);
        w.Raw(","); w.String("Entries"); w.Raw(":"); WriteArray(w, v.Entries, x => WriteCanonicalApprovedEntry(w, x));
        w.Raw("}");
    }
    internal sealed record PApprovedEntry(string EntryId, string SourceEntityFingerprint, string OwnerInputSha256, string ReferenceAnswerSha256, string PaidReferenceProvenanceSha256, PEligibility Eligibility);
    private static PApprovedEntry ReadObjectApprovedEntry(JsonElement e)
    {
        RequireObject(e, ["Eligibility", "EntryId", "OwnerInputSha256", "PaidReferenceProvenanceSha256", "ReferenceAnswerSha256", "SourceEntityFingerprint"]);
        return new(ReadText(e.GetProperty("EntryId")), ReadText(e.GetProperty("SourceEntityFingerprint")), ReadText(e.GetProperty("OwnerInputSha256")), ReadText(e.GetProperty("ReferenceAnswerSha256")), ReadText(e.GetProperty("PaidReferenceProvenanceSha256")), ReadObjectEligibility(e.GetProperty("Eligibility")));
    }
    private static PApprovedEntry ReadTupleApprovedEntry(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 6);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]), atoms.Read(e[4]), ReadTupleEligibility(e[5], atoms));
    }
    private static void WriteTupleApprovedEntry(Utf8JsonWriter w, PApprovedEntry v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.EntryId));
        w.WriteNumberValue(atoms.Intern(v.SourceEntityFingerprint));
        w.WriteNumberValue(atoms.Intern(v.OwnerInputSha256));
        w.WriteNumberValue(atoms.Intern(v.ReferenceAnswerSha256));
        w.WriteNumberValue(atoms.Intern(v.PaidReferenceProvenanceSha256));
        WriteTupleEligibility(w, v.Eligibility, atoms);
        w.WriteEndArray();
    }
    private static void WriteCanonicalApprovedEntry(CanonicalSink w, PApprovedEntry v)
    {
        w.Raw("{");
        w.Raw(""); w.String("Eligibility"); w.Raw(":"); WriteCanonicalEligibility(w, v.Eligibility);
        w.Raw(","); w.String("EntryId"); w.Raw(":"); w.String(v.EntryId);
        w.Raw(","); w.String("OwnerInputSha256"); w.Raw(":"); w.String(v.OwnerInputSha256);
        w.Raw(","); w.String("PaidReferenceProvenanceSha256"); w.Raw(":"); w.String(v.PaidReferenceProvenanceSha256);
        w.Raw(","); w.String("ReferenceAnswerSha256"); w.Raw(":"); w.String(v.ReferenceAnswerSha256);
        w.Raw(","); w.String("SourceEntityFingerprint"); w.Raw(":"); w.String(v.SourceEntityFingerprint);
        w.Raw("}");
    }
    internal sealed record PEligibility(bool LicenseAccepted, bool IsQuarantined, string[] EvidenceRefs);
    private static PEligibility ReadObjectEligibility(JsonElement e)
    {
        RequireObject(e, ["EvidenceRefs", "IsQuarantined", "LicenseAccepted"]);
        return new(ReadBoolean(e.GetProperty("LicenseAccepted")), ReadBoolean(e.GetProperty("IsQuarantined")), ReadArray(e.GetProperty("EvidenceRefs"), x => ReadText(x)));
    }
    private static PEligibility ReadTupleEligibility(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 3);
        return new(ReadBoolean(e[0]), ReadBoolean(e[1]), ReadArray(e[2], x => atoms.Read(x)));
    }
    private static void WriteTupleEligibility(Utf8JsonWriter w, PEligibility v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteBooleanValue(v.LicenseAccepted);
        w.WriteBooleanValue(v.IsQuarantined);
        WriteArray(w, v.EvidenceRefs, x => w.WriteNumberValue(atoms.Intern(x)));
        w.WriteEndArray();
    }
    private static void WriteCanonicalEligibility(CanonicalSink w, PEligibility v)
    {
        w.Raw("{");
        w.Raw(""); w.String("EvidenceRefs"); w.Raw(":"); WriteArray(w, v.EvidenceRefs, x => w.String(x));
        w.Raw(","); w.String("IsQuarantined"); w.Raw(":"); w.Boolean(v.IsQuarantined);
        w.Raw(","); w.String("LicenseAccepted"); w.Raw(":"); w.Boolean(v.LicenseAccepted);
        w.Raw("}");
    }
    internal sealed record PCandidate(string ItemId, string SourceId, string ReferenceEntryId, string SourceEntityFingerprint, string OwnerInput, string PaidModelReferenceAnswer, PProvenance PaidReferenceProvenance, string MaterialAdmissionEvidenceSha256);
    private static PCandidate ReadObjectCandidate(JsonElement e)
    {
        RequireObject(e, ["ItemId", "MaterialAdmissionEvidenceSha256", "OwnerInput", "PaidModelReferenceAnswer", "PaidReferenceProvenance", "ReferenceEntryId", "SourceEntityFingerprint", "SourceId"]);
        return new(ReadText(e.GetProperty("ItemId")), ReadText(e.GetProperty("SourceId")), ReadText(e.GetProperty("ReferenceEntryId")), ReadText(e.GetProperty("SourceEntityFingerprint")), ReadText(e.GetProperty("OwnerInput")), ReadText(e.GetProperty("PaidModelReferenceAnswer")), ReadObjectProvenance(e.GetProperty("PaidReferenceProvenance")), ReadText(e.GetProperty("MaterialAdmissionEvidenceSha256")));
    }
    private static PCandidate ReadTupleCandidate(JsonElement e, AtomReader atoms)
    {
        RequireTuple(e, 8);
        return new(atoms.Read(e[0]), atoms.Read(e[1]), atoms.Read(e[2]), atoms.Read(e[3]), atoms.Read(e[4]), atoms.Read(e[5]), ReadTupleProvenance(e[6], atoms), atoms.Read(e[7]));
    }
    private static void WriteTupleCandidate(Utf8JsonWriter w, PCandidate v, AtomTable atoms)
    {
        w.WriteStartArray();
        w.WriteNumberValue(atoms.Intern(v.ItemId));
        w.WriteNumberValue(atoms.Intern(v.SourceId));
        w.WriteNumberValue(atoms.Intern(v.ReferenceEntryId));
        w.WriteNumberValue(atoms.Intern(v.SourceEntityFingerprint));
        w.WriteNumberValue(atoms.Intern(v.OwnerInput));
        w.WriteNumberValue(atoms.Intern(v.PaidModelReferenceAnswer));
        WriteTupleProvenance(w, v.PaidReferenceProvenance, atoms);
        w.WriteNumberValue(atoms.Intern(v.MaterialAdmissionEvidenceSha256));
        w.WriteEndArray();
    }
    private static void WriteCanonicalCandidate(CanonicalSink w, PCandidate v)
    {
        w.Raw("{");
        w.Raw(""); w.String("ItemId"); w.Raw(":"); w.String(v.ItemId);
        w.Raw(","); w.String("MaterialAdmissionEvidenceSha256"); w.Raw(":"); w.String(v.MaterialAdmissionEvidenceSha256);
        w.Raw(","); w.String("OwnerInput"); w.Raw(":"); w.String(v.OwnerInput);
        w.Raw(","); w.String("PaidModelReferenceAnswer"); w.Raw(":"); w.String(v.PaidModelReferenceAnswer);
        w.Raw(","); w.String("PaidReferenceProvenance"); w.Raw(":"); WriteCanonicalProvenance(w, v.PaidReferenceProvenance);
        w.Raw(","); w.String("ReferenceEntryId"); w.Raw(":"); w.String(v.ReferenceEntryId);
        w.Raw(","); w.String("SourceEntityFingerprint"); w.Raw(":"); w.String(v.SourceEntityFingerprint);
        w.Raw(","); w.String("SourceId"); w.Raw(":"); w.String(v.SourceId);
        w.Raw("}");
    }
}
