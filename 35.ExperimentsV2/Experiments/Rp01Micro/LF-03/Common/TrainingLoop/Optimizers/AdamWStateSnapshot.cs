// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Layers;

namespace TrainingLoop.Optimizers;

// Непрозрачный снимок в памяти для одного экземпляра AdamW. Он специально
// привязан к создавшему его оптимизатору, чтобы не стать скрытым импортом
// с диска или переносом накопленных оценок градиентов в другой оптимизатор
// с другой настройкой.
public sealed class AdamWStateSnapshot
{
    internal AdamWStateSnapshot(
        AdamW owner,
        int step,
        IReadOnlyList<AdamWParameterStateSnapshot> entries)
    {
        Owner = owner;
        Step = step;
        Entries = entries.ToArray();
    }

    internal AdamW Owner { get; }

    internal int Step { get; }

    internal IReadOnlyList<AdamWParameterStateSnapshot> Entries { get; }
}

internal sealed record AdamWParameterStateSnapshot(
    Parameter Parameter,
    float[] FirstMoment,
    float[] SecondMoment);

// Подготовленное состояние восстановления уже содержит отдельные копии массивов
// накопленных оценок градиентов и словарь для замены. Применение — одноразовая
// операция на том же оптимизаторе.
public sealed class AdamWPreparedState
{
    private bool _consumed;

    internal AdamWPreparedState(
        AdamW owner,
        int step,
        Dictionary<Parameter, (float[] m, float[] v)> state)
    {
        Owner = owner;
        Step = step;
        State = state;
    }

    private AdamW Owner { get; }

    private int Step { get; }

    private Dictionary<Parameter, (float[] m, float[] v)> State { get; }

    internal (int Step, Dictionary<Parameter, (float[] m, float[] v)> State)
        Consume(AdamW owner)
    {
        if (!ReferenceEquals(Owner, owner))
            throw new InvalidOperationException("ADAMW_PREPARED_STATE_OWNER_MISMATCH");
        if (_consumed)
            throw new InvalidOperationException("ADAMW_PREPARED_STATE_ALREADY_APPLIED");

        _consumed = true;
        return (Step, State);
    }
}
