// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using AgentInfrastructure; // Reuse the existing result contract.
using DPO; // Return the canonical trainer input rather than another training algorithm.
using DPO.Models; // Keep the exact response material version explicit.
using Tokenizer; // Preserve the production segmented byte BPE tokenizer.

namespace AgentInfrastructure.Logical; // The preparation owner already owns the internal prepared batches.

/// <summary>Exposes the existing, once-prepared exact batches to bounded experiment runners.</summary>
public static class LogicalDpoPreparedBatchMaterializer // This adapter grants no mutation or publication capability.
{ // Preparation remains implemented by LogicalDpoTrainingUnit, including typed pair admission.
    public static OperationResult<IReadOnlyList<DpoVectorBatch>> Prepare( // Let one trainer retain its optimizer/reference across finite minibatches.
        LogicalDpoPreferenceExample[] preferences, BpeTokenizer tokenizer) // Only the three texts and existing typed authors reach preparation.
    { // Do not add a second tokenizer, masking rule, or DPO loss.
        var result = LogicalDpoTrainingUnit.PrepareExecution(preferences, tokenizer, DpoExactPairMaterial.Version); // Reuse BuildBatch and exact response masks.
        return result.IsSuccess && result.Value is not null // Preserve the canonical preparation refusal when material is invalid.
            ? OperationResult<IReadOnlyList<DpoVectorBatch>>.Ok(Array.AsReadOnly(result.Value.OptimizerInputs.Select(x => x.Batch).ToArray())) // DpoTrainer snapshots mutable tensor carriers again at admission.
            : result.MapError<IReadOnlyList<DpoVectorBatch>>(); // No batch escapes a failed preparation.
    } // No model is constructed or mutated here.
} // Production wrapper step and preference limits remain unchanged.
