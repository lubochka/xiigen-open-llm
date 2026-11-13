// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace DPO.Models;

// Батч данных для DPO: предпочтительный (chosen) и отклонённый (rejected) ответы.
public record DpoBatch
{
    // Token IDs: [1, T] — целые числа в виде float
    public required Tensor ChosenIds    { get; init; }
    public required Tensor RejectedIds  { get; init; }
    // Маски внимания: [1, T] — 1 для реальных токенов, 0 для padding
    public required Tensor ChosenMask   { get; init; }
    public required Tensor RejectedMask { get; init; }
    // Допуск переносится рядом с tensor batch; null остается допустимым только
    // для evaluation/legacy чтения, а mutating входы тренера падают закрыто.
    public DpoTrainerPairAdmission? Admission { get; init; }
    // Legacy callers keep the full-sequence mean. Exact CodeDocumentation
    // material explicitly selects response targets and the matching sum objective.
    public string MaterialVersion { get; init; } = DpoExactPairMaterial.LegacyVersion;
    public DpoExactPairMaterial? ExactMaterial { get; init; }
    public Tensor? ChosenResponseMask { get; init; }
    public Tensor? RejectedResponseMask { get; init; }
}
