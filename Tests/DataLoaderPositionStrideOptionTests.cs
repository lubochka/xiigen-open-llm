// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DatasetPipeline;

namespace LlmFromScratch.Tests;

/// <summary>
/// Проверки НАСТРАИВАЕМОСТИ шага продвижения позиции чтения в загрузчике
/// данных предобучения. Перенос результата опыта номер 68 добавляет четвёртый
/// аргумент конструктора, поэтому этот набор намеренно не собирается на коде до
/// переноса: отсутствие аргумента — тоже доказательство чувствительности, но
/// оно выражается ошибкой компилятора, а не упавшим утверждением.
/// Прежнее поведение — шаг в один токен — остаётся доступным явным аргументом,
/// чтобы прежние прогоны и абляции можно было воспроизвести.
/// </summary>
public class DataLoaderPositionStrideOptionTests
{
    [Fact]
    public void NextBatch_ExplicitSingleTokenStride_ReproducesOverlappingWindows()
    {
        using var directory = new TemporaryNpyDirectory();
        NpyTestFile.Write(directory.File("tokens.npy"), 10, 11, 12, 13, 14);
        var loader = new DataLoader(
            directory.Path,
            batchSize: 2,
            seqLen: 3,
            positionStride: 1);

        var (inputs, targets) = loader.NextBatch();

        // Это дословно прежнее поведение продукта: соседние окна перекрываются
        // на длину окна минус один токен.
        Assert.Equal(new long[] { 10, 11, 12 }, Row(inputs, 0));
        Assert.Equal(new long[] { 11, 12, 13 }, Row(targets, 0));
        Assert.Equal(new long[] { 11, 12, 13 }, Row(inputs, 1));
        Assert.Equal(new long[] { 12, 13, 14 }, Row(targets, 1));
    }

    [Fact]
    public void NextBatch_ExplicitStride_UsesGivenStrideNotWindowLength()
    {
        using var directory = new TemporaryNpyDirectory();
        NpyTestFile.Write(directory.File("tokens.npy"), Sequence(300, 24));
        var loader = new DataLoader(
            directory.Path,
            batchSize: 3,
            seqLen: 4,
            positionStride: 2);

        var (inputs, _) = loader.NextBatch();

        // Заданный шаг в два токена уважается и не подменяется длиной окна
        // молча: смещения окон 0, 2 и 4.
        Assert.Equal(new long[] { 300, 301, 302, 303 }, Row(inputs, 0));
        Assert.Equal(new long[] { 302, 303, 304, 305 }, Row(inputs, 1));
        Assert.Equal(new long[] { 304, 305, 306, 307 }, Row(inputs, 2));
    }

    [Fact]
    public void Constructor_NegativePositionStride_ThrowsArgumentOutOfRangeExceptionBeforeFilesystemAccess()
    {
        string missingPath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"missing-npy-directory-{Guid.NewGuid():N}");

        // Новый аргумент проверяется до обращения к файловой системе — ровно
        // так же, как уже проверяются размер партии и длина окна. Поэтому
        // ожидается исключение об аргументе, а не об отсутствующем каталоге.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DataLoader(
                missingPath,
                batchSize: 1,
                seqLen: 1,
                positionStride: -1));
    }

    [Fact]
    public void Constructor_ZeroPositionStride_MeansDefaultWindowLengthStride()
    {
        using var directory = new TemporaryNpyDirectory();
        NpyTestFile.Write(directory.File("tokens.npy"), Sequence(300, 24));
        var loader = new DataLoader(
            directory.Path,
            batchSize: 2,
            seqLen: 4,
            positionStride: 0);

        var (inputs, _) = loader.NextBatch();

        // Ноль означает «шаг по умолчанию», то есть ровно длину окна: смещения
        // окон 0 и 4, как и без явного аргумента.
        Assert.Equal(new long[] { 300, 301, 302, 303 }, Row(inputs, 0));
        Assert.Equal(new long[] { 304, 305, 306, 307 }, Row(inputs, 1));
    }

    /// <summary>
    /// Готовит подряд идущие значения токенов: значение равно началу плюс
    /// позиция, поэтому позицию в потоке видно по самому значению.
    /// </summary>
    private static ushort[] Sequence(int start, int count) =>
        [.. Enumerable.Range(start, count).Select(value => (ushort)value)];

    /// <summary>
    /// Возвращает одну строку партии как обычный массив, чтобы сравнивать её
    /// целиком одним утверждением.
    /// </summary>
    private static long[] Row(long[,] values, int row)
    {
        var result = new long[values.GetLength(1)];
        for (int column = 0; column < result.Length; column++)
            result[column] = values[row, column];
        return result;
    }
}
