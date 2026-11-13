// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TrainableRouter.Training;

using System.Text.Json;
using System.Text.Encodings.Web;
using AgentInfrastructure;

/// <summary>Fixed same-operation preparation/Started consumer. Ordinary execution requires an established installed permission owner.</summary>
internal sealed class CodeDocumentationNativeCodexSelectionHost
{
    // Literal RUN-CONTRACT §2.1 preimage; version is its strict LF UTF8 SHA256.
    internal const string PolicyContent = """
### 2.1 CODEX teaching-selection policy — S04-CODEX-TEACHING-SELECTION-v1

This procedural policy implements the owner's 2026-10-07 role instruction. It is not a claim that an earlier C# selector, fixed category ranking or runtime policy registry already existed. The human-role decision is performed by CODEX with `GPT-6-Astra` and reasoning mode `ultra`, which the owner explicitly identifies as `astra 6 ultracode`; the product validates and reuses its bound result. The policy name here is a document identifier, not a magic runtime permit. The model alias is already resolved: use the supported native model/mode controls, not a search for a separate preset or a new policy approval.

1. Reuse a previously accepted, still-valid selection for this objective and identical input bindings. Do not rerun selection merely after a worker message, restart, review or commit. If relevant inputs changed, preserve the old evidence and perform the existing conflict/recovery handling before any new effects.
2. Obtain the saved sanitized weakness categories through the genuine held Session. No raw closed task, paid exam answer, Student exam answer or certification payload may enter selection or authoring context. Baseline-sufficient and missing-usable-diagnosis remain their existing branches; do not invent a weakness.
3. Open the complete declared teaching graph through its existing owner. Establish actual teaching-purpose and disclosure rights, novelty and required ORIGINAL/certification disjointness before a task is eligible. Descriptive PolicyId/PolicyVersion/order, a valid hash or Train partition alone establishes none of these rights. Preserve all supplied sources/items/order; do not silently truncate the catalog.
4. CODEX judges which eligible task addresses the diagnosed weakness and explains the task-specific pedagogical fit using only permitted material. It selects ONE task, not a closed exam question renamed as teaching. Reuse an actually established approved policy when available; otherwise exercise the delegated choice here instead of requesting a category/item ranking from Luba. Do not impose an invented global ranking of categories or claim an alphabetical first item is a semantic decision. For otherwise equivalent eligible choices, preserve the existing supplied curriculum order as the stable tie-break.
5. Before asking the Chinese author, freeze objective/grant, policy-document version, SOURCE/curriculum hash, sanitized categories, selected item/source/entry identities, material-admission evidence and the reason for selection. Persist and reopen through the existing session/selection ownership boundary. A work-record note is evidence, not a production bearer capability. If runtime consumption of this bound decision is absent, implement and test that narrow boundary; do not assert an existing API or ask Luba to name one.
6. Build the complete typed teaching packet: full selected task; the matching saved separate-task weak Student response or the one already allowed isolated response; permitted teaching reference; prior accepted good/bad examples with their context-only IDs. Check the real configured input limit before a call; byte-size or JSON validity alone is not proof of tokenizer-level completeness. Do not replace the Student response with a routing candidate, omit mandatory history or cut the task to fit.

Replay means reusing the saved decision, not assuming a generative model will independently make an identical choice twice. A synthetic fixture may provide a clearly labelled test operator decision to test its consumer; it must not be represented as a real Astra decision or policy admission.
""";
    internal sealed record NativeSelectionPreparationInspection(bool Prepared, bool Replayed,
        string RequestSha256, int RequestLength, string SchemaSha256, int SchemaLength, int CandidateCount, // Keep this native lifecycle boundary private and bounded.
        string? LaunchBoundaryErrorCode = null, string? LaunchBoundaryDetail = null); // Detached inspection/error data cannot issue or retain launch authority.
    private readonly CodeDocumentationOneLessonSession owner;
    private readonly CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt;
    private CodeDocumentationNativeCodexSelectionHost(CodeDocumentationOneLessonSession owner,
        CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt)
    { this.owner = owner; this.receipt = receipt; }
    internal static OperationResult<CodeDocumentationNativeCodexSelectionHost> FromHeldSession(
        CodeDocumentationOneLessonSession owner, CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt)
        => receipt.IsActiveFor(owner) ? OperationResult<CodeDocumentationNativeCodexSelectionHost>.Ok(new(owner, receipt))
            : CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexSelectionHost>();

    internal Task<OperationResult<NativeSelectionPreparationInspection>> PrepareOrReplayAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!receipt.IsActiveFor(owner)) return Task.FromResult(CodeDocumentationNativeCodexSelectionWire.Refuse<NativeSelectionPreparationInspection>());
        var nonce = owner.ReadOrAllocateNativePreparationNonceUnderHeldMaterial(receipt);
        if (!nonce.IsSuccess || nonce.Value is null) return Task.FromResult(nonce.MapError<NativeSelectionPreparationInspection>());
        var context = receipt.Context;
        var packet = context.CopyCanonicalBytes();
        if (packet.Length > CodeDocumentationNativeCodexSelectionWire.PacketMaximumBytes)
            return Task.FromResult(CodeDocumentationNativeCodexSelectionWire.Refuse<NativeSelectionPreparationInspection>(true));
        // Complete permitted teaching candidates and provenance are rendered; private ORIGINAL snapshots are never inputs.
        var prompt = RenderCompletePrompt(context, nonce.Value.Nonce);
        byte[] request;
        if (nonce.Value.WireVersion == 2)
        {
            try
            {
                var compact = CodeDocumentationNativeCodexSelectionWire.ProjectV2(receipt.SessionId,
                    context.CopyBindingBytes(), context.MetadataInput.CopyBytes(), receipt.CopyApprovalBytes(),
                    receipt.ApprovalEvidenceSha256, context, nonce.Value.Nonce);
                request = CodeDocumentationNativeCodexSelectionWire.WriteV2(compact);
            }
            catch (CodeDocumentationNativeCodexSelectionWire.PreparationCapacityException)
            { return Task.FromResult(CodeDocumentationNativeCodexSelectionWire.Refuse<NativeSelectionPreparationInspection>(true)); }
            catch (Exception ex) when (ex is System.Text.Json.JsonException or FormatException or OverflowException or ArgumentException)
            { return Task.FromResult(CodeDocumentationNativeCodexSelectionWire.Refuse<NativeSelectionPreparationInspection>()); }
        }
        else
        {
        var wire = new CodeDocumentationNativeCodexSelectionWire.PreparedNativeSelectionRequestV1(receipt.SessionId,
            Convert.ToBase64String(context.CopyBindingBytes()), context.MetadataInput.OriginalReceiptId, context.MetadataInput.OriginalReceiptSha256,
            Array.AsReadOnly(context.MetadataInput.Categories.ToArray()), Convert.ToBase64String(context.MetadataInput.CopyBytes()),
            Convert.ToBase64String(packet), Convert.ToBase64String(receipt.CopyApprovalBytes()), receipt.ApprovalEvidenceSha256,
            CodeDocumentationNativeCodexSelectionWire.Sha256(CodeDocumentationNativeCodexSelectionWire.Utf8.GetBytes(PolicyContent)),
            PolicyContent, nonce.Value.Nonce, prompt, Convert.ToBase64String(CodeDocumentationNativeCodexSelectionWire.CopySchemaBytes()));
        request = CodeDocumentationNativeCodexSelectionWire.Write(wire);
        }
        token.ThrowIfCancellationRequested();
        var prepared = owner.StoreNativePreparationUnderHeldMaterial(receipt, request, token); // Keep full stable preparation under this same Host/receipt/CAS.
        if (!prepared.IsSuccess || prepared.Value is null) return Task.FromResult(prepared); // Preserve exact preparation/custody refusals.
        var launch = owner.ConsumeNativeLaunchBoundaryUnderHeldMaterial(receipt, this, token); // Reach the fixed launch-input owner before the operation releases its CAS.
        if (!launch.IsSuccess && launch.ErrorCode != CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable) // Keep this native lifecycle boundary private and bounded.
            return Task.FromResult(launch.MapError<NativeSelectionPreparationInspection>()); // A partial/changed/unknown Started stays recovery-required.
        return Task.FromResult(OperationResult<NativeSelectionPreparationInspection>.Ok(prepared.Value with
        { LaunchBoundaryErrorCode = launch.IsSuccess ? null : launch.ErrorCode, LaunchBoundaryDetail = launch.IsSuccess ? null : launch.ErrorMessage })); // Unknown enforcement preserves successful PreparedUnsent and exact next-host503 behavior.
    } // Finish this bounded owner check without renewing authority or an attempt.

    internal OperationResult<bool> ConsumeStartedUnderHeldSession(CodeDocumentationOneLessonSession.NativeSelectionLaunchBindingV1 binding, CancellationToken token) // Consume Started through the exact fixed Host while the binding is active.
    { // Consume Started through the exact fixed Host while the binding is active.
        if (!binding.IsActiveFor(owner, this)) return CodeDocumentationNativeCodexSelectionWire.Refuse<bool>(); // Foreign/copied/expired bindings cannot reach this fixed consumer.
        var stored = owner.StoreNativeStartedUnderHeldLaunchBinding(this, binding, token); // Complete flush and stable whole readback precede any first-create seam.
        if (!stored.IsSuccess) return stored; // No creation seam on partial/unknown storage or changed inputs.
        if (!binding.IsActiveFor(owner, this)) return CodeDocumentationNativeCodexSelectionWire.Refuse<bool>(); // Recheck all actual held inputs immediately before the creation seam.
        owner.SelectionMaterialPointForTesting?.Invoke("native-first-process-create-seam-synthetic"); token.ThrowIfCancellationRequested(); // Explicit fixture observation only: no OS process or model is created.
        return CodeDocumentationNativeCodexSelectionWire.Refuse<bool>(); // Started remains charged/unknown; actual Windows process ownership/enforcement is the next linked source step.
    }
    internal static string RenderCompletePrompt(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context, string nonce)
    {
        var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        return PolicyContent + "\n\nSaved sanitized weakness categories:\n" + JsonSerializer.Serialize(context.MetadataInput.Categories, options) +
            "\nAll eligible teaching candidates in declared order:\n" + JsonSerializer.Serialize(context.Candidates, options) +
            "\nReturn one future selection tuple and reason under the fixed output schema. Prepared nonce: " + nonce;
    }
}
