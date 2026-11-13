// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training; // Keep the structural outcome description inside the session owner's module.

using System.Security.Cryptography; // Recompute the descriptive session identity from the expected objective bytes.
using System.Text; // Measure the complete raw object with strict UTF-8 encoding.
using System.Text.Json; // Retain and clone the complete parsed outcome graph.
using AgentInfrastructure; // Return failed results rather than throwing for untrusted input.
using ContinuousLearning; // Reuse the existing restricted canonical wire primitives.

/// <summary>
/// Immutable, structurally valid terminal bytes, still untrusted. Neither the
/// descriptive identities nor the closed references establish receipt custody,
/// branch/slot identity, terminal authority, replay or permission for effects.
/// </summary>
internal sealed class CodeDocumentationOneLessonTerminalOutcomeV2 // Describe closed terminal syntax without exposing a public authority API.
{ // Own a detached graph and its complete raw canonical bytes.
    private const int ObjectCeiling = 2_053; // Preserve the one fixed conservative complete-terminal-object ceiling.
    private static readonly UTF8Encoding Utf8 = new(false, true); // Reject malformed Unicode and emit no encoding preamble.
    private readonly byte[] _canonicalBytes; // Keep bytes private so callers cannot mutate this description.

    private CodeDocumentationOneLessonTerminalOutcomeV2(JsonElement value, byte[] bytes) // Construct only after every structural check succeeds.
    { // Detach this successful result from the caller's document and storage.
        Value = value.Clone(); // Preserve the full graph after the original JsonDocument is disposed.
        _canonicalBytes = bytes; // Take ownership of the decoder's newly allocated raw canonical byte array.
    } // Keep the construction seam private to the pure decoder.

    internal JsonElement Value { get; } // Expose a read-only cloned graph; no reference is resolved from it.
    internal string OutcomeId => Value.GetProperty("OutcomeId").GetString()!; // Read the validated descriptive identity without deriving an authorized slot.
    internal string TerminalState => Value.GetProperty("TerminalState").GetString()!; // Describe the supplied closed state without proving the branch.
    internal byte[] CanonicalBytes => (byte[])_canonicalBytes.Clone(); // Return a defensive copy of complete canonical raw bytes.

    internal static OperationResult<CodeDocumentationOneLessonTerminalOutcomeV2> Decode( // Validate untrusted syntax and repeated identity only.
        JsonElement value, CodeDocumentationOneLessonSessionBinding? expectedBinding) // Accept the parsed object and the caller's expected binding, with no owner or effects.
    { // Return either a complete immutable description or a stable failed result.
        try // Contain ordinary malformed graph and lifetime failures at this pure boundary.
        { // Require a real object before measuring its complete raw representation.
            if (value.ValueKind != JsonValueKind.Object) return Recovery(); // Reject scalars, arrays, null and undefined elements without a partial description.

            // The one fixed complete-object ceiling precedes every ordinary
            // state/member/schema/binding check. Session separately checks the
            // LF-inclusive frame bound and common chain guards before decoding.
            var rawBytes = Utf8.GetBytes(value.GetRawText()); // Count actual complete object bytes, including noncanonical spelling or unknown members.
            if (rawBytes.Length > ObjectCeiling) // Give the fixed capacity refusal priority over ordinary state/schema/binding faults.
                return OperationResult<CodeDocumentationOneLessonTerminalOutcomeV2>.Fail( // Reject an over-cap object with no usable graph.
                    "The complete V2 terminal outcome exceeds its object ceiling.", // Explain which capacity boundary failed.
                    CodeDocumentationOneLessonSession.CapacityExceededErrorCode); // Preserve the existing stable session capacity code.

            var references = ReferenceFields(value); // Select exactly the three permitted structural member sets by state and pair presence.
            if (references is null || // Reject an absent, mistyped or noncanonical state.
                !CodeDocumentationOneLessonV2Wire.ExactFields(value, // Reject duplicate, aliased, unknown, missing and null terminal members.
                    new[] { "OutcomeId", "SchemaVersion", "ObjectiveId", "SessionId", // Include the common schema and objective/session descriptive identities.
                        "TrainingRunId", "BindingSha256", "TerminalState" }.Concat(references).ToArray()) || // Add only this variant's required closed references.
                value.GetProperty("SchemaVersion").GetRawText() != "2" || // Require raw JSON integer two, rejecting normalized numeric aliases.
                !HashField(value, "OutcomeId", 32) || // Require the supplied identity to be a bare lowercase 32-character hex string.
                !CodeDocumentationOneLessonSession.ValidProgressReceiptBindingV2(expectedBinding) || // Reuse inherited pure identity syntax and count checks without owner authority.
                !SameString(value, "ObjectiveId", expectedBinding!.ObjectiveId) || // Require exact objective equality after expected-binding syntax succeeds.
                !SameString(value, "TrainingRunId", expectedBinding.TrainingRunId) || // Require the same expected ASCII run identity without an alphanumeric-first substitute.
                !SameString(value, "SessionId", Convert.ToHexString(SHA256.HashData( // Recompute the inherited lowercase objective session hash.
                    Utf8.GetBytes(expectedBinding.ObjectiveId))).ToLowerInvariant()) || // Hash strict raw objective UTF-8 without canonical JSON quotes or LF.
                !SameString(value, "BindingSha256", CodeDocumentationOneLessonSession.ComputeBindingSha256(expectedBinding)) || // Require the existing full LF-domain binding digest.
                !references.All(name => ValidReference(value.GetProperty(name))) || // Validate every required reference's exact two-member string primitives.
                !rawBytes.AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalBytes(value))) // Reject raw order, whitespace or escape aliases with the unchanged writer.
                return Recovery(); // Refuse all inside-cap malformed representations without retaining a partial value.

            // Only the cloned graph and newly allocated raw canonical bytes are
            // retained. There is deliberately no terminal-authority digest.
            return OperationResult<CodeDocumentationOneLessonTerminalOutcomeV2>.Ok(new(value, rawBytes)); // Return only a detached structural description, never a terminal capability.
        } // Finish validation before exposing either graph or bytes.
        catch (Exception exception) when (exception is JsonException or EncoderFallbackException or // Contain malformed JSON representations and invalid UTF-8 encoding.
            InvalidOperationException or ArgumentException) // Also contain disposed/default graph access and ordinary malformed graph arguments.
        { // Preserve failure semantics when a caller graph cannot be safely inspected.
            return Recovery(); // Return the existing recovery code with no partial immutable result.
        } // Keep unexpected infrastructure/programming failures outside this ordinary-input exception filter.
    } // Complete the pure decoder without reading artifacts, selecting checkpoints or performing effects.

    private static string[]? ReferenceFields(JsonElement value) // Select the closed reference set, without resolving decision or branch provenance.
    { // The subsequent ExactFields guard rejects partial pairs and every forbidden extra member.
        if (!value.TryGetProperty("TerminalState", out var state) || state.ValueKind != JsonValueKind.String) // Require a supplied string discriminant.
            return null; // Supply no shape for a missing or mistyped discriminant.
        return state.GetString() switch // Accept only the two exact state spellings.
        { // Negative variants are distinguished by complete member sets, not receipt authority.
            "TERMINAL_NEGATIVE_PARENT_RETAINED" when // Consider the early negative set only when both trial references are absent.
                !value.TryGetProperty("BaselineReceipt", out _) && !value.TryGetProperty("TrialEvaluationReceipt", out _) // Leave any partial pair for the five-reference exact-field refusal.
                => ["QualityDecisionReceipt", "DispositionReceipt", "FinalReopenReceipt"], // Require all three early negative references.
            "TERMINAL_NEGATIVE_PARENT_RETAINED" // A negative object containing either trial member must contain both.
                => ["QualityDecisionReceipt", "DispositionReceipt", "FinalReopenReceipt", "BaselineReceipt", "TrialEvaluationReceipt"], // Require the complete five-reference post-trial set.
            "TERMINAL_PROMOTED" // Promoted syntax requires both trial and publication reference pairs.
                => ["QualityDecisionReceipt", "DispositionReceipt", "FinalReopenReceipt", "BaselineReceipt", "TrialEvaluationReceipt", // Require all common and trial references.
                    "PublicationStartedReceipt", "PublicationReceipt"], // Require both publication references with no optional omission.
            _ => null, // Refuse every other state string rather than normalizing an alias.
        }; // Supply only these exact reference sets to the closed terminal-field guard.
    } // Keep schema selection descriptive and independent from replay decisions.

    private static bool ValidReference(JsonElement value) // Validate reference syntax without looking up an earlier receipt.
        => CodeDocumentationOneLessonV2Wire.ExactFields(value, "ReceiptId", "ReceiptSha256") && // Require an exact non-null object of two uniquely spelled members.
           HashField(value, "ReceiptId", 32) && HashField(value, "ReceiptSha256", 64); // Require bare lowercase ASCII identity and digest strings of their exact widths.

    private static bool HashField(JsonElement value, string name, int width) // Inspect one already closed identity or reference member.
        => value.GetProperty(name).ValueKind == JsonValueKind.String && // Reject non-string primitives before inspecting the exact hex alphabet.
           CodeDocumentationOneLessonV2Wire.Hash(value.GetProperty(name).GetString(), width); // Reuse the unchanged lowerhex width predicate without coercion.

    private static bool SameString(JsonElement value, string name, string expected) // Compare one repeated binding identity with the expected exact value.
        => value.GetProperty(name).ValueKind == JsonValueKind.String && value.GetProperty(name).GetString() == expected; // Require string type and ordinal content equality without normalization.

    private static OperationResult<CodeDocumentationOneLessonTerminalOutcomeV2> Recovery() // Use one stable refusal for ordinary inside-cap malformed input.
        => OperationResult<CodeDocumentationOneLessonTerminalOutcomeV2>.Fail( // Preserve Fail's message-then-code contract with no value.
            "The V2 terminal outcome is malformed, noncanonical, or differs from the expected binding.", // Explain the structural and identity validation boundary.
            CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode); // Return the existing recovery code used by the actual session owner.
} // Finish the untrusted structural outcome type without introducing a digest or authority seam.
