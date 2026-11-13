// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training; // Keep the untrusted slot description in the existing session owner module.

using System.Text; // Measure complete raw object bytes with strict UTF-8 rather than parsed field lengths.
using System.Text.Json; // Clone the complete validated graph independently from the caller's document lifetime.
using AgentInfrastructure; // Return the existing failed-result contract for ordinary untrusted inputs.
using ContinuousLearning; // Reuse the unchanged shared restricted canonical writer.

/// <summary>
/// Detached closed slot syntax, still untrusted. This description does not prove
/// slot identity, plan membership, assessment provenance, lifecycle or replay
/// authority, and never performs an effect.
/// </summary>
internal sealed class CodeDocumentationOneLessonTrialSlotV2 // Describe a complete started or completed object without creating a public capability.
{ // Own only a detached graph and privately held complete canonical bytes.
    private const int StartedObjectCeiling = 355; // Preserve the normative conservative cap even though the largest legal started tuple is 353 bytes.
    private const int CompletedObjectCeiling = 547; // Preserve the normative cap for the complete slot plus its observation.
    private static readonly UTF8Encoding Utf8 = new(false, true); // Reject invalid Unicode while counting the original complete representation.
    private readonly byte[] _canonicalBytes; // Prevent callers from changing the decoder's owned raw canonical representation.

    private CodeDocumentationOneLessonTrialSlotV2(JsonElement value, bool started, byte[] bytes) // Construct only after raw capacity, inherited binding, shape and canonical spelling succeed.
    { // Detach the description from caller-owned JSON and keep the decoder's fresh byte storage.
        Value = value.Clone(); // Preserve the entire object and nested observation after the input document is disposed.
        IsStarted = started; // Retain the caller's exact envelope-kind selection as descriptive metadata only.
        _canonicalBytes = bytes; // Take the newly allocated complete raw bytes without exposing their mutable backing array.
    } // Keep construction private so no partial description escapes a failed decode.

    internal JsonElement Value { get; } // Expose a read-only detached graph without resolving any plan or selected case.
    internal bool IsStarted { get; } // Describe the already selected wire variant without supplying lifecycle authority.
    internal string SlotId => Value.GetProperty("SlotId").GetString()!; // Return supplied validated lowerhex syntax without computing or authenticating the slot identity.
    internal byte[] CanonicalBytes => (byte[])_canonicalBytes.Clone(); // Give each caller a fresh defensive copy of the complete canonical bytes.

    internal static OperationResult<CodeDocumentationOneLessonTrialSlotV2> Decode( // Validate pure untrusted representation without opening files or dispatching work.
        JsonElement value, bool started, CodeDocumentationOneLessonSessionBinding? expectedBinding) // Accept the complete object, trusted kind selection and expected binding syntax.
    { // Expose only a complete detached success or a stable failure with no value.
        try // Contain ordinary malformed graph, encoding and lifetime failures at this pure boundary.
        { // Capacity of a real object's original bytes takes priority over its ordinary grammar faults.
            if (value.ValueKind != JsonValueKind.Object) return Recovery(); // Refuse scalars, arrays, null and undefined without measuring a different representation.
            var rawBytes = Utf8.GetBytes(value.GetRawText()); // Count the whole original object, including whitespace, extra members and noncanonical spelling.
            if (rawBytes.Length > (started ? StartedObjectCeiling : CompletedObjectCeiling)) // Enforce the fixed variant cap before expected binding or member validation.
                return OperationResult<CodeDocumentationOneLessonTrialSlotV2>.Fail( // Refuse oversized raw bytes without retaining a partial graph.
                    "The complete V2 trial slot exceeds its object ceiling.", // Identify the complete nested-object capacity boundary.
                    CodeDocumentationOneLessonSession.CapacityExceededErrorCode); // Preserve the existing stable capacity refusal used by Session.
            if (!CodeDocumentationOneLessonSession.ValidProgressReceiptBindingV2(expectedBinding)) // Reuse inherited identity syntax and bounded N/R policy without owner authority.
                return Recovery(); // Refuse invalid expected bindings before dereferencing their descriptors.
            var descriptor = expectedBinding!.DescriptorsV2!; // Use only descriptors admitted by the unchanged pure binding predicate.
            var slots = descriptor.CaseCount * descriptor.RepeatCount; // The inherited bounded product has already proved this multiplication safe.
            if (!CodeDocumentationOneLessonSession.ValidSlotShapeV2(value, started, descriptor, slots) || // Preserve the existing exact primitive grammar and ordinal bounds unchanged.
                !rawBytes.AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalBytes(value))) // Require the unchanged restricted writer's complete bytes, rejecting spelling aliases.
                return Recovery(); // Reject all inside-cap malformed or noncanonical objects with no partial description.
            return OperationResult<CodeDocumentationOneLessonTrialSlotV2>.Ok(new(value, started, rawBytes)); // Return detached syntax only, leaving actual owner prefixes unsupported.
        } // Complete every validation before returning the graph or owned bytes.
        catch (Exception exception) when (exception is JsonException or EncoderFallbackException or // Contain ordinary malformed JSON representations and invalid UTF-8 encoding.
            InvalidOperationException or ArgumentException) // Also contain default/disposed graph access and malformed ordinary arguments.
        { // Keep unreadable graph failures equivalent to ordinary malformed input.
            return Recovery(); // Refuse without a partially initialized value or any external effect.
        } // Unexpected infrastructure failures remain outside this narrow ordinary-input filter.
    } // Complete the pure decode without computing authority, chain relationships or deterministic slot identities.

    private static OperationResult<CodeDocumentationOneLessonTrialSlotV2> Recovery() // Preserve one ordinary refusal for nonobjects and inside-cap malformed data.
        => OperationResult<CodeDocumentationOneLessonTrialSlotV2>.Fail( // Follow the existing message-then-code OperationResult contract.
            "The V2 trial slot or expected binding is malformed or noncanonical.", // Explain the bounded syntactic validation failure.
            CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode); // Match the stable recovery refusal used by the real session owner.
} // Close the structural description without adding phase, assessment or effect authority.
