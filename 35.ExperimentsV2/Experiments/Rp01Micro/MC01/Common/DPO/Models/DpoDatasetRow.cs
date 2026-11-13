// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DPO.Models;

// Строка dataset хранит бедную пару рядом с nullable admission: старый loader
// остается совместимым, а новый паспорт не объявляет пару trainable сам по себе.
public sealed record DpoDatasetRow
{
    internal DpoDatasetRow(
        DpoPair pair,
        DpoTrainerPairAdmission? admission)
    {
        ArgumentNullException.ThrowIfNull(pair);

        Pair = pair;
        Admission = admission;
    }

    // Pair остается тем же трехстрочным контрактом для indexer/Shuffle/trainer
    // совместимости; provenance живет отдельно и не вшивается в DpoPair.
    public DpoPair Pair { get; }

    // Null означает legacy JSONL или evaluation-only материал без допуска пары.
    public DpoTrainerPairAdmission? Admission { get; }
}
