// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TransformerModel; // Keep the declared checkpoint contract in its existing owner layer.

using TransformerModel.Layers; // Reuse CausalSelfAttention.CanonicalProjectionNames as the canonical attention-target allowlist.

/// <summary>
/// Exact adapter slots declared by the topology owner for a full checkpoint.
/// This describes serialized tensors only; it grants no execution permission.
/// </summary>
public sealed class FullCheckpointLoraTopology // This immutable slot declaration grants no model or training authority.
{ // This immutable slot declaration grants no model or training authority.
    public FullCheckpointLoraTopology(int rank, IEnumerable<string> targetModules) // Capture the rank and explicit targets supplied by the canonical topology owner.
    { // Malformed slot declarations are rejected before any artifact is read.
        ArgumentNullException.ThrowIfNull(targetModules); // A missing target list cannot describe a full-LoRA checkpoint.
        var targets = targetModules.ToArray(); // Own the captured slot list so callers cannot mutate its schema.
        if (rank <= 0 || targets.Length == 0 // Admit only the declared nonduplicate canonical attention slots and positive rank.
            || targets.Distinct(StringComparer.Ordinal).Count() != targets.Length // Admit only the declared nonduplicate canonical attention slots and positive rank.
            || targets.Any(target => !CausalSelfAttention.CanonicalProjectionNames // Admit only the declared nonduplicate canonical attention slots and positive rank.
                .Contains(target, StringComparer.Ordinal))) // Match exact ordinal names from the attention owner's allowlist.
            throw new ArgumentException("Full checkpoint adapter topology is invalid."); // Refuse a declaration that violates the exact saved-root contract.
        Rank = rank; // Use the captured rank to derive exact A/B tensor dimensions.
        TargetModules = Array.AsReadOnly(targets); // Freeze the copied target list against caller mutation.
    } // Only validated rank and target metadata survive construction.

    public int Rank { get; } // Expose the admitted adapter width used by the codec schema.
    public IReadOnlyList<string> TargetModules { get; } // Expose the immutable exact projection set without adding execution rights.
} // This immutable slot declaration grants no model or training authority.
