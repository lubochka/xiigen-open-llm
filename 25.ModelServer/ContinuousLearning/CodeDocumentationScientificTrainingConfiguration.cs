// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace ModelServer; // First-RP01 scientific trust is process-local server configuration.
using AgentInfrastructure; // Refuse missing/malformed choices before model/evaluation effects.
using TrainableRouter.Training; // Reuse the strict frozen-policy owner.

internal sealed class CodeDocumentationScientificTrainingConfiguration // Never reconstructed from HTTP flags or accepted model presence.
{ // Disabled by default, separately from the existing root optimizer grant.
    internal const string Prefix = "XIIGEN_CODE_DOCUMENTATION_SCIENTIFIC_"; // Public startup selectors contain hashes only.
    private readonly string? _mode, _policy, _held, _verdict; // Snapshot at host startup, not each request.
    private CodeDocumentationScientificTrainingConfiguration(string? mode, string? policy, string? held, string? verdict) { _mode = mode; _policy = policy; _held = held; _verdict = verdict; } // No source read or model effect in construction.
    internal static CodeDocumentationScientificTrainingConfiguration Resolve(Func<string, string?> read) => new(read(Prefix + "MODE"), read(Prefix + "POLICY_SHA256"), read(Prefix + "HELD_SHA256"), read(Prefix + "VERDICT_SHA256")); // Use the existing trusted startup environment reader.
    internal OperationResult<CodeDocumentationScientificTrainingPolicy> Open(string root, bool publishOnly, CancellationToken token) // Require the pre-T1 policy and later exact real examiner artifact.
    { // A client cannot choose criteria, review path or acceptance boolean.
        if (_mode != "FIRST_RP01_SCIENTIFIC_HOLD" || !Hex(_policy) || publishOnly && (!Hex(_held) || !Hex(_verdict))) return Refuse<CodeDocumentationScientificTrainingPolicy>(); // Missing/malformed choice fails before preparing any model or C0.
        var opened = CodeDocumentationScientificTrainingPolicy.Open(root, _policy!, token); // Fixed private policy path and hash/custody/strict-schema admission.
        if (!opened.IsSuccess || opened.Value is null) return opened; // Preserve actual policy/source drift refusal.
        var policy = opened.Value.Document; // Server pins the actual immutable public preregistration and evaluation-only retention authority.
        if (policy.Protocol.Sha256 != "bbbcfd4f00d749306024b231d23d5e490a0a9beb58f48a2869d89558898429c8" || policy.Commitment.Sha256 != "c4680151e6a93fb6276ac3e701c36f5f5643c4657a540778ff0b147434c79fac" || policy.EvaluationRetentionGrant.Sha256 != "fe3b5eb01ae013bd2750a826c7dc23670a185e5c057b8d8c19bbd02de0d8345f") return Refuse<CodeDocumentationScientificTrainingPolicy>(); // No posthoc different protocol/grant can activate this first-RP01 route.
        return opened; // This is explicit operator provenance trust, not invented scientific/model attestation.
    } // The actual independent grader runs separately after held Trial completion.
    internal string HeldSha256 => _held!; // Available only after publish-only Open succeeds.
    internal string VerdictSha256 => _verdict!; // Exact real private artifact, not a hardcoded PASS.
    private static bool Hex(string? text) => text is { Length: 64 } && text.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'); // Canonical lowercase SHA256.
    private static OperationResult<T> Refuse<T>() => OperationResult<T>.Fail("Explicit first-RP01 scientific startup policy and pinned real held/verdict artifacts are required.", CodeDocumentationScientificTrainingPolicy.Invalid); // No raw/private content in diagnostics.
} // Generic routes/defaults retain their existing behavior.
