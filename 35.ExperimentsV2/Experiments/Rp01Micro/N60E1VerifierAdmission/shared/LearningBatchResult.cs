// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

// LearningBatchResult is the metrics record returned after one training batch completes.
// All numeric fields are required — no placeholder or UNKNOWN values in production.
public sealed record LearningBatchResult(
    // BatchId identifies which batch produced this result.
    string BatchId,
    // RowsTrained is the count of DPO pairs that were processed in this batch.
    int RowsTrained,
    // AccuracyBefore is the held-out accuracy measured before this batch was applied.
    double AccuracyBefore,
    // AccuracyAfter is the held-out accuracy measured after this batch was applied.
    double AccuracyAfter,
    // RewardMargin is the average margin between chosen and rejected log-probabilities.
    double RewardMargin,
    // CpuMs is the CPU (Central Processing Unit) time consumed by this batch in milliseconds.
    long CpuMs,
    // PeakRamMb is the peak RAM (Random Access Memory) usage during this batch in megabytes.
    long PeakRamMb,
    // ElapsedMs is the wall-clock elapsed time for this batch in milliseconds.
    long ElapsedMs,
    // PromotedUnitIds lists the unit ids whose checkpoints were promoted to 25.ModelServer after this batch.
    IReadOnlyList<string> PromotedUnitIds
);
