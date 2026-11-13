// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Core;

// Плоское хранилище данных тензора.
// Отделено от формы чтобы в будущем view-операции могли делить одно хранилище.
public sealed class TensorStorage
{
    // Все числа тензора в row-major порядке
    public float[] Data { get; }

    public TensorStorage(int totalElements)
    {
        Data = new float[totalElements];
    }

    // Конструктор с начальными данными — копируем чтобы не было внешних алиасов
    public TensorStorage(float[] data)
    {
        Data = data.ToArray();
    }

    public int Length => Data.Length;
}
