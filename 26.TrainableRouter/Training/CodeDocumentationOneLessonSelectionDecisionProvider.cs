// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training; // Keep selection syntax in this assembly without transferring runtime owner authority.

using System.Security.Cryptography; // Hash frozen identity and synthetic acknowledgement preimages with SHA256.
using System.Text; // Encode the exact UTF8 domain preimages and canonical JSON text.
using AgentInfrastructure; // Import the dependency used by this closed selection syntax implementation.
using ContinuousLearning; // Import the dependency used by this closed selection syntax implementation.

// A test callback returns untrusted scalar syntax. Only this concrete host-owned
// wrapper issues an envelope, and its only factory fixes SyntheticTest lineage.
// There is deliberately no Native/model-string/JSON import factory.
internal sealed class CodeDocumentationOneLessonSelectionDecisionProvider // Confine the callback and only SyntheticTest factory to the concrete host issuer.
{ // Begin this scope: confine the callback and only SyntheticTest factory to the concrete host issuer.
    internal sealed record SyntheticChoice(string ItemId,string SourceId,string ReferenceEntryId, // Confine the callback and only SyntheticTest factory to the concrete host issuer.
        string SourceEntityFingerprint,string Reason,string MaterialAdmissionEvidenceSha256); // Confine the callback and only SyntheticTest factory to the concrete host issuer.
    private readonly Func<CodeDocumentationTeachingSelectionInputV1,CancellationToken,Task<OperationResult<SyntheticChoice>>> operation; // Retain only the synthetic test callback inside this concrete issuer.
    private CodeDocumentationOneLessonSelectionDecisionProvider( // Confine the callback and only SyntheticTest factory to the concrete host issuer.
        Func<CodeDocumentationTeachingSelectionInputV1,CancellationToken,Task<OperationResult<SyntheticChoice>>> operation) // Confine the callback and only SyntheticTest factory to the concrete host issuer.
        => this.operation=operation; // Confine the callback and only SyntheticTest factory to the concrete host issuer.
    internal static CodeDocumentationOneLessonSelectionDecisionProvider CreateSyntheticForTesting( // Confine the callback and only SyntheticTest factory to the concrete host issuer.
        Func<CodeDocumentationTeachingSelectionInputV1,CancellationToken,Task<OperationResult<SyntheticChoice>>> operation) // Confine the callback and only SyntheticTest factory to the concrete host issuer.
        => new(operation ?? throw new ArgumentNullException(nameof(operation))); // Reject a missing callback; the only factory remains explicitly synthetic.

    private readonly Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext, CancellationToken, Task<OperationResult<SyntheticChoice>>>? materialInspection; // Add a full-packet Synthetic consumer within this owner, without a parallel selector or Native factory.
    private enum MaterialMode { None, Inspection, Durable } // A private factory choice separates identical callback types without importing authority from response labels.
    private readonly MaterialMode materialMode; // Inspection results cannot enter the durable issuer.
    private CodeDocumentationOneLessonSelectionDecisionProvider(Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext, CancellationToken, Task<OperationResult<SyntheticChoice>>> inspect, MaterialMode mode) // Both full consumers retain their closed factory mode.
    { // Keep the old durable invocation path fail-closed for this explicitly inspection-only instance.
        materialInspection = inspect; materialMode = mode; operation = (_, _) => Task.FromResult(CodeDocumentationOneLessonV2Wire.SelectionInvalid<SyntheticChoice>()); // Neither full mode manufactures a legacy issued decision.
    } // No model ID, caller origin or imported JSON can choose this mode.
    internal static CodeDocumentationOneLessonSelectionDecisionProvider CreateSyntheticMaterialInspectionForTesting( // This factory remains clearly Synthetic; actual host launch authority is not implemented here.
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext, CancellationToken, Task<OperationResult<SyntheticChoice>>> inspect) // Callback receives all real candidate bytes after same-Session preparation checks.
        => new(inspect ?? throw new ArgumentNullException(nameof(inspect)), MaterialMode.Inspection); // A missing callback is a programming error, never a constant unavailable wrapper.
    internal static CodeDocumentationOneLessonSelectionDecisionProvider CreateSyntheticMaterialSelectionForTesting( // Explicit fixture issuance remains Synthetic, even when its result is durable.
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext, CancellationToken, Task<OperationResult<SyntheticChoice>>> choose) // Consume the same complete immutable packet as inspection.
        => new(choose ?? throw new ArgumentNullException(nameof(choose)), MaterialMode.Durable); // The private mode avoids duplicate constructors and caller-controlled origin.
    internal bool IsDurableMaterialSelection => materialMode == MaterialMode.Durable; // Session checks the concrete factory before reserving dispatch.

    internal sealed class SyntheticSelectionMaterialInspection // Inspection evidence is deliberately incompatible with persisted decision/permit/Native authority.
    { // Only the concrete full-packet consumer can issue this immutable result.
        private readonly CodeDocumentationOneLessonSelectionDecisionProvider issuer; // Retain the exact concrete Synthetic callback owner.
        private readonly CodeDocumentationOneLessonSession.PreparedSelectionMaterialReceipt receipt; // Bind evidence to the exact private same-Session complete input receipt.
        internal SyntheticChoice Choice { get; } // Expose validated exact candidate syntax, not a mutable public decision passport.
        internal string PacketSha256 => receipt.Context.PacketSha256; // Identify the actual full input consumed by this callback.
        internal string Origin => "SyntheticTest"; // No actual Astra invocation is claimed or reconstructed from metadata.
        private SyntheticSelectionMaterialInspection(CodeDocumentationOneLessonSelectionDecisionProvider owner, CodeDocumentationOneLessonSession.PreparedSelectionMaterialReceipt input, SyntheticChoice choice) // Construction follows exact full input/member/syntax validation.
        { issuer = owner; receipt = input; Choice = choice with { }; } // Copy immutable choice scalars and retain concrete private issuing lineage.
        internal bool IsIssuedBy(CodeDocumentationOneLessonSelectionDecisionProvider owner, CodeDocumentationOneLessonSession.PreparedSelectionMaterialReceipt input) // Session authenticates result without exposing an authority-bearing constructor.
            => ReferenceEquals(issuer, owner) && ReferenceEquals(receipt, input); // Matching digest strings are insufficient to impersonate the invocation.
        internal static async Task<OperationResult<SyntheticSelectionMaterialInspection>> Inspect(CodeDocumentationOneLessonSelectionDecisionProvider owner, // Execute one actual full-packet Synthetic callback.
            CodeDocumentationOneLessonSession receiving, CodeDocumentationOneLessonSession.PreparedSelectionMaterialReceipt receipt, CancellationToken token) // A caller-held receipt outside the active Session operation cannot dispatch.
        { // Validate complete opaque input before any callback sees task/reference material.
            token.ThrowIfCancellationRequested(); // Observe caller cancellation before dispatch.
            if (owner.materialMode != MaterialMode.Inspection || owner.materialInspection is null || !receipt.IsActiveFor(receiving) || receipt.Context.Origin != "SyntheticTest") return CodeDocumentationOneLessonV2Wire.SelectionInvalid<SyntheticSelectionMaterialInspection>(); // Require active Synthetic lineage; protected-origin contexts refuse before any inspection callback receives raw material.
            var context = receipt.Context; var frozen = context.CopyCanonicalBytes(); // Freeze every full task/reference/binding/graph byte before untrusted callback execution.
            var candidate = await owner.materialInspection(context, token).ConfigureAwait(false); // Deliver the complete real recipient once; returned choice remains untrusted scalars.
            token.ThrowIfCancellationRequested(); // Callback-triggered cancellation stays primary rather than a successful inspection.
            if (!candidate.IsSuccess || candidate.Value is null) return candidate.MapError<SyntheticSelectionMaterialInspection>(); // Callback refusal mints no result, choice or effect.
            if (!receipt.IsActiveFor(receiving) || !frozen.AsSpan().SequenceEqual(context.CopyCanonicalBytes())) return CodeDocumentationOneLessonV2Wire.SelectionInvalid<SyntheticSelectionMaterialInspection>(); // Detect receiving mutation and any full preimage disagreement.
            var c = candidate.Value; var matches = context.Candidates.Where(i => i.ItemId == c.ItemId && i.SourceId == c.SourceId && // Exact eligible membership never substitutes the first candidate.
                i.ReferenceEntryId == c.ReferenceEntryId && i.SourceEntityFingerprint == c.SourceEntityFingerprint && i.MaterialAdmissionEvidenceSha256 == c.MaterialAdmissionEvidenceSha256).ToArray(); // Require every joined owner endpoint and the actual opaque admission evidence.
            if (matches.Length != 1) return CodeDocumentationOneLessonV2Wire.SelectionInvalid<SyntheticSelectionMaterialInspection>(); // Ineligible/foreign/substituted choices withhold the whole result.
            var input = context.MetadataInput; var hash = context.PacketSha256; // Reuse the existing closed scalar syntax guard without issuing its payload.
            var syntax = CodeDocumentationTeachingSelectionPayloadV1.Capture(input.OriginalReceiptId, input.OriginalReceiptSha256, input.SourceSha256, input.Categories, // Bind all sanitized metadata in the grammar check.
                c.ItemId, c.SourceId, c.ReferenceEntryId, c.SourceEntityFingerprint, c.Reason, input.FrozenContextSha256, c.MaterialAdmissionEvidenceSha256, "synthetic-material-inspection", hash, hash); // Synthetic syntax checking is not a host launch acknowledgement or saved frame.
            return syntax.IsSuccess ? OperationResult<SyntheticSelectionMaterialInspection>.Ok(new(owner, receipt, c)) : syntax.MapError<SyntheticSelectionMaterialInspection>(); // Mint only private inspection evidence, never IssuedSelectionDecision.
        } // Receiving Session still performs reason exclusion and full final None rechecks.
    } // End the actual full-packet Synthetic consumer result.
    internal Task<OperationResult<SyntheticSelectionMaterialInspection>> InspectPreparedMaterialAsync(CodeDocumentationOneLessonSession receiving, // Only the same active held Session can consume this complete packet.
        CodeDocumentationOneLessonSession.PreparedSelectionMaterialReceipt receipt, CancellationToken token) // No public DTO/import/native route is added.
        => SyntheticSelectionMaterialInspection.Inspect(this, receiving, receipt, token); // Reuse the existing concrete provider owner for the additive inspection mode.

    internal sealed class IssuedMaterialSelectionDecision // Only this concrete full-packet provider creates Started-linked durable evidence.
    {
        private readonly CodeDocumentationOneLessonSelectionDecisionProvider issuer; // Preserve actual callback owner identity.
        private readonly CodeDocumentationOneLessonSession.MaterialSelectionDispatchReceipt receipt; // A matching hash cannot impersonate the active dispatch receipt.
        internal SyntheticChoice Choice { get; } // Copy candidate scalars after complete membership/syntax checks.
        private IssuedMaterialSelectionDecision(CodeDocumentationOneLessonSelectionDecisionProvider owner, CodeDocumentationOneLessonSession.MaterialSelectionDispatchReceipt input, SyntheticChoice choice) // Constructor is not a caller/import surface.
        { issuer = owner; receipt = input; Choice = choice with { }; } // Retain the actual concrete invocation lineage.
        internal bool IsIssuedBy(CodeDocumentationOneLessonSelectionDecisionProvider owner, CodeDocumentationOneLessonSession.MaterialSelectionDispatchReceipt input) // Only the active receiving Session consumes this envelope.
            => ReferenceEquals(issuer, owner) && ReferenceEquals(receipt, input); // Strings and inspection receipts do not satisfy lineage.
        internal static async Task<OperationResult<IssuedMaterialSelectionDecision>> Invoke(CodeDocumentationOneLessonSelectionDecisionProvider owner, CodeDocumentationOneLessonSession receiving, CodeDocumentationOneLessonSession.MaterialSelectionDispatchReceipt input, CancellationToken token) // Dispatch at most once under the already persisted Started.
        {
            token.ThrowIfCancellationRequested(); // Preserve exact cancellation before exposing permitted material.
            if (!owner.IsDurableMaterialSelection || owner.materialInspection is null || input.Context.Origin != "SyntheticTest" || !input.TryConsumeInvocationFor(receiving, owner)) return CodeDocumentationOneLessonV2Wire.SelectionInvalid<IssuedMaterialSelectionDecision>(); // Origin denial precedes consumption; repeated/foreign/expired attempts cannot invoke the Synthetic callback.
            var context = input.Context; var frozen = context.CopyCanonicalBytes(); // Freeze all actual task/reference/provenance/order bytes.
            var selected = await owner.materialInspection(context, token).ConfigureAwait(false); // Deliver the full recipient once; callback output remains untrusted.
            token.ThrowIfCancellationRequested(); // Callback cancellation is the primary object/token.
            if (!selected.IsSuccess || selected.Value is null) return selected.MapError<IssuedMaterialSelectionDecision>(); // A refused call leaves Started unknown.
            if (!input.IsActiveFor(receiving, owner) || !frozen.AsSpan().SequenceEqual(context.CopyCanonicalBytes())) return CodeDocumentationOneLessonV2Wire.SelectionInvalid<IssuedMaterialSelectionDecision>(); // Do not issue evidence for changed input or abandoned dispatch.
            var c = selected.Value; var matches = context.Candidates.Where(i => i.ItemId == c.ItemId && i.SourceId == c.SourceId && i.ReferenceEntryId == c.ReferenceEntryId && i.SourceEntityFingerprint == c.SourceEntityFingerprint && i.MaterialAdmissionEvidenceSha256 == c.MaterialAdmissionEvidenceSha256).Count(); // Require one exact member, never a first-item substitution.
            if (matches != 1 || string.IsNullOrWhiteSpace(c.Reason) || !CodeDocumentationOneLessonV2Wire.ValidUtf8Text(c.Reason,CodeDocumentationOneLessonV2Wire.FullMaterialReasonMaximumBytes)) return CodeDocumentationOneLessonV2Wire.SelectionInvalid<IssuedMaterialSelectionDecision>(); // Refuse malformed/oversized response syntax before computing its acknowledgement; legacy allowance is unchanged.
            var m = context.MetadataInput; var syntax = CodeDocumentationTeachingSelectionPayloadV1.Capture(m.OriginalReceiptId, m.OriginalReceiptSha256, m.SourceSha256, m.Categories, c.ItemId, c.SourceId, c.ReferenceEntryId, c.SourceEntityFingerprint, c.Reason, m.FrozenContextSha256, c.MaterialAdmissionEvidenceSha256, input.InvocationId, input.SettingsSha256, input.Acknowledgment(c)); // Reuse closed scalar grammar without promoting its syntax to native authority.
            return syntax.IsSuccess ? OperationResult<IssuedMaterialSelectionDecision>.Ok(new(owner, input, c)) : syntax.MapError<IssuedMaterialSelectionDecision>(); // Concrete receipt identity is required in addition to validated syntax.
        }
    }
    internal Task<OperationResult<IssuedMaterialSelectionDecision>> InvokePreparedMaterialSelectionAsync(CodeDocumentationOneLessonSession receiving, CodeDocumentationOneLessonSession.MaterialSelectionDispatchReceipt receipt, CancellationToken token) // The fixed receiving owner consumes the full durable factory only.
        => IssuedMaterialSelectionDecision.Invoke(this, receiving, receipt, token); // No model/JSON/native import route is introduced.

    internal sealed class IssuedSelectionDecision // Freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
    { // Begin this scope: freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
        private readonly CodeDocumentationOneLessonSelectionDecisionProvider issuer; // Retain the concrete issuer identity privately; scalar syntax cannot mint this envelope.
        internal CodeDocumentationTeachingSelectionPayloadV1 Payload {get;} // Expose only validated copied syntax from this private issued envelope.
        private IssuedSelectionDecision(CodeDocumentationOneLessonSelectionDecisionProvider issuer, // Freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
            CodeDocumentationTeachingSelectionPayloadV1 payload) {this.issuer=issuer;Payload=payload;} // Freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
        internal bool IsIssuedBy(CodeDocumentationOneLessonSelectionDecisionProvider expected) // Freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
            => ReferenceEquals(issuer,expected); // Authenticate envelope lineage by concrete object identity rather than supplied strings.
        internal static async Task<OperationResult<IssuedSelectionDecision>> Invoke( // Freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
            CodeDocumentationOneLessonSelectionDecisionProvider owner,CodeDocumentationTeachingSelectionInputV1 input, // Freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
            CancellationToken token) // Freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
        { // Begin this scope: freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
            token.ThrowIfCancellationRequested(); // Preserve caller cancellation before dispatch and again before envelope issuance.
            var frozen=CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionInputBytes(input); // Freeze a detached input preimage before the untrusted synthetic callback runs.
            var choice=await owner.operation(input,token).ConfigureAwait(false); // Invoke the concrete callback once; its returned choice remains untrusted syntax.
            token.ThrowIfCancellationRequested(); // Preserve caller cancellation before dispatch and again before envelope issuance.
            if(!choice.IsSuccess || choice.Value is null) return choice.MapError<IssuedSelectionDecision>(); // Propagate callback refusal without minting an issued decision.
            if(!frozen.AsSpan().SequenceEqual(CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionInputBytes(input))) // Reject any mismatch against the private frozen invocation preimage.
                return CodeDocumentationOneLessonV2Wire.SelectionInvalid<IssuedSelectionDecision>(); // Freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
            string Hash(string value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant(); // Hash synthetic lineage preimages without reading runtime metadata or calling a model.
            var invocation=Guid.NewGuid().ToString("N"); // Allocate a distinct opaque invocation identity for this synthetic dispatch.
            var settings=Hash("SyntheticTest\ngpt-6-astra\nultra\n"+input.FrozenContextSha256); // Commit SyntheticTest origin, requested Astra role and the frozen context.
            var ack=Hash("SyntheticTest\n"+invocation+"\n"+settings); // Bind the synthetic acknowledgement to this invocation and settings commitment.
            var c=choice.Value; // Treat callback scalars as candidate syntax until closed Capture validation passes.
            var payload=CodeDocumentationTeachingSelectionPayloadV1.Capture(input.OriginalReceiptId,input.OriginalReceiptSha256, // Bind the candidate syntax to the frozen ORIGINAL, SOURCE and diagnostic order.
                input.SourceSha256,input.Categories,c.ItemId,c.SourceId,c.ReferenceEntryId,c.SourceEntityFingerprint,c.Reason, // Freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
                input.FrozenContextSha256,c.MaterialAdmissionEvidenceSha256,invocation,settings,ack); // Freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
            return payload.IsSuccess && payload.Value is not null // Mint the private envelope only after every closed scalar constraint succeeds.
                ? OperationResult<IssuedSelectionDecision>.Ok(new(owner,payload.Value)) // Attach this concrete owner identity to the successfully captured payload.
                : payload.MapError<IssuedSelectionDecision>(); // Keep malformed callback output as a refusal without envelope authority.
        } // Close this scope: freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
    } // Close this scope: freeze input, invoke once, and mint a private envelope with synthetic lineage and validated payload.
    internal Task<OperationResult<IssuedSelectionDecision>> InvokeAsync(CodeDocumentationTeachingSelectionInputV1 input, // Delegate invocation through the concrete issuer without exposing another minting factory.
        CancellationToken token) => IssuedSelectionDecision.Invoke(this,input,token); // Delegate invocation through the concrete issuer without exposing another minting factory.
} // Close this scope: delegate invocation through the concrete issuer without exposing another minting factory.
