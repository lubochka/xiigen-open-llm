// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning; // Invoke the existing corpus owner without referencing generation or model-host commands.
using System.Text.Json; // Print only a fixed content-free receipt projection.

try // Keep malformed CLI arguments and filesystem exceptions out of public private-content output.
{ // Operator explicitly chooses one offline persistence or readback action.
    if (args.Length == 4 && args[0] == "seal") // Usage: seal trusted-root private-intake-path exact-raw-intake-sha256.
    { // The strict library owner performs custody, parse, seal and actual reopen.
        var result = RepositoryOwnedNativeReferenceCorpusIntake.SealFromPrivateFile(args[1], args[2], args[3]); // No new capture, Teacher, Student or optimizer call occurs.
        if (!result.IsSuccess || result.Value is null) return Refuse(result.ErrorCode); // Failed effects remain failures with no private contents printed.
        Print(result.Value.Opened, result.Value.CaseBindings); return 0; // Output IDs/hashes/counts only after actual reopened owner agreement.
    } // Existing same bytes are idempotent; changed same-address content is refused by the sealer.
    if (args.Length == 7 && args[0] == "reopen") // Usage: reopen trusted-root subject version corpus-sha256-ref payload-sha256 manifest-self-sha256.
    { // A separate process can verify exact stored native corpus pins without reading an intake again.
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(args[1], "csharp-code-documentation", args[2], args[3]); // Derive only the canonical reference-corpus address.
        if (!address.IsSuccess || address.Value is null) return Refuse(address.ErrorCode); // No alternate root or path fallback.
        var result = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedNativeCapture(address.Value, args[4], args[5], args[6]); // Require native origin and all expected pins before any legacy preparation.
        if (!result.IsSuccess || result.Value is null) return Refuse(result.ErrorCode); // Actual readback refusal remains nonzero.
        Print(result.Value, Array.Empty<NativeReferenceCorpusCaseBinding>()); return 0; // Print no owner tasks, Gold, capture bodies or model output.
    } // Any unsupported argument shape is refused without initiating an effect.
    return Refuse("Usage: seal <trusted-root> <private-intake> <raw-sha256> | reopen <trusted-root> <subject> <version> <corpus-sha256-ref> <payload-sha256> <manifest-self-sha256>"); // Publish the complete offline CLI ABI.
} // No inference, background work, automatic timer or resource cancellation exists in this executable.
catch (Exception e) when (e is not OutOfMemoryException and not StackOverflowException and not OperationCanceledException) // Preserve caller cancellation and fatal runtime errors.
{ return Refuse("NATIVE_REFERENCE_CORPUS_COMMAND_FAILED"); } // Never echo exceptions that could contain a private path or body value.

static int Refuse(string? code) { Console.Error.WriteLine(code ?? "NATIVE_REFERENCE_CORPUS_COMMAND_FAILED"); return 1; } // One content-free refusal with an actual nonzero exit.
static void Print(RepositoryOwnedReferenceCorpusOpenResult opened, IReadOnlyList<NativeReferenceCorpusCaseBinding> bindings) // Public receipt exposes content identity and scientific alias joins only.
{ // Raw reference material stays in the local owner snapshot and its private payload.
    var m = opened.Manifest; // Copy only the approved metadata projection.
    Console.WriteLine(JsonSerializer.Serialize(new { purpose = "CLOSED_REFERENCE_ASSESSMENT_ONLY", backendAttested = false, trainingGrantCreated = false, // State the descriptive boundary explicitly.
        m.Schema, m.Context, m.EntryCount, m.CorpusSha256, m.PayloadSha256, m.ManifestSha256, CaseBindings = bindings, // Return exact persistence IDs, pins and counts.
        entries = opened.Corpus.Entries.Select(e => new { e.EntryId, e.SourceEntityFingerprint, partition = e.Partition.ToString(), // Never serialize the entry itself or its private answer.
            provenanceSha256 = e.NativeCaptureProvenance!.ProvenanceSha256, referenceAnswerRawUtf8Sha256 = e.NativeCaptureProvenance.ReferenceAnswerRawUtf8Sha256, // Return exact origin/answer commitments only.
            captureRecordSha256 = e.NativeCaptureProvenance.Capture.CaptureRecordSha256 }).ToArray() })); // No capture metadata text or raw model output is printed.
} // Receipt hashes describe stored bytes and do not attest a provider/model/license.
