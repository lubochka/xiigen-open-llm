// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Core;

// Форма тензора — описывает размеры каждого измерения.
// Например [8, 512, 64] = 8 примеров, 512 позиций, 64 числа на позицию.
public sealed class TensorShape
{
    public int[] Dimensions { get; }

    // Количество измерений (0=скаляр, 1=вектор, 2=матрица, ...)
    public int Rank => Dimensions.Length;

    // Полное число элементов = произведение всех размерностей
    public int TotalElements { get; }

    // Шаги (strides): на сколько сдвинуться в плоском массиве чтобы перейти
    // к следующему элементу по каждому измерению.
    // Матрица 3×4: strides = [4, 1]; элемент [i,j] = data[i*4 + j]
    public int[] Strides { get; }

    public TensorShape(params int[] dimensions)
    {
        if (dimensions.Length == 0)
            throw new ArgumentException("Тензор должен иметь хотя бы одно измерение");
        if (dimensions.Any(d => d <= 0))
            throw new ArgumentException(
                $"Все размерности должны быть положительными. Получено: [{string.Join(", ", dimensions)}]");

        Dimensions    = dimensions.ToArray();
        TotalElements = dimensions.Aggregate(1, (acc, d) => acc * d);

        Strides    = new int[dimensions.Length];
        Strides[^1] = 1;
        for (int i = dimensions.Length - 2; i >= 0; i--)
            Strides[i] = Strides[i + 1] * dimensions[i + 1];
    }

    // Позиция в плоском массиве по многомерным индексам.
    // [2, 1] для матрицы 3×4 → 2*4 + 1 = 9
    public int FlatIndex(int[] indices)
    {
        if (indices.Length != Rank)
            throw new ArgumentException(
                $"Количество индексов {indices.Length} не совпадает с рангом {Rank}");

        int flatIndex = 0;
        for (int i = 0; i < Rank; i++)
        {
            if (indices[i] < 0 || indices[i] >= Dimensions[i])
                throw new IndexOutOfRangeException(
                    $"Индекс {indices[i]} выходит за пределы размерности {Dimensions[i]} на оси {i}");
            flatIndex += indices[i] * Strides[i];
        }
        return flatIndex;
    }

    public override string ToString() => $"Shape[{string.Join(", ", Dimensions)}]";
}
