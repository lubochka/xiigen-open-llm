// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace DPO.Models;

// DPO data batch: the preferred (chosen) and rejected (rejected) responses.
public record DpoBatch
{
    // Token IDs: [1, T] — integers stored as float
    public required Tensor ChosenIds    { get; init; }
    public required Tensor RejectedIds  { get; init; }
    // Attention masks: [1, T] — 1 for real tokens, 0 for padding
    public required Tensor ChosenMask   { get; init; }
    public required Tensor RejectedMask { get; init; }
}
