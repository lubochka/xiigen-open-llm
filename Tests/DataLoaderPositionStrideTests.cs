// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DatasetPipeline;

namespace LlmFromScratch.Tests;

/// <summary>
/// Проверки шага продвижения позиции чтения ПО УМОЛЧАНИЮ в загрузчике данных
/// предобучения. Перенос результата опыта номер 68 задаёт шаг по умолчанию,
/// равный длине окна, вместо прежнего жёстко зашитого шага в один токен.
/// Этот набор намеренно пользуется только прежним трёхаргументным
/// конструктором, поэтому он собирается и на коде до переноса: на нём он
/// падает утверждениями, и это доказательство чувствительности проверок.
/// Дополнительно здесь закрыты две ветки, которые опыт НЕ измерял: граница
/// между файлами (маркер конца файла) и хвост осколка короче окна. Проверка
/// хвоста требует ПРАВИЛЬНОГО поведения — полного прочтения осколка, — а не
/// закрепляет потерю токенов: зелёная проверка не имеет права переопределять
/// сломанный инвариант продукта.
/// </summary>
public class DataLoaderPositionStrideTests
{
    /// <summary>
    /// Значение маркера конца файла, который сборщик набора
    /// (<c>DatasetBuilder</c>) дописывает между файлами источника.
    /// </summary>
    private const ushort EndOfFileToken = 1;

    [Fact]
    public void NextBatch_DefaultStride_UsesWindowLengthAsPositionStride()
    {
        using var directory = new TemporaryNpyDirectory();
        NpyTestFile.Write(directory.File("tokens.npy"), Sequence(100, 20));
        var loader = new DataLoader(directory.Path, batchSize: 4, seqLen: 4);

        var (inputs, targets) = loader.NextBatch();

        // Строка партии 0 начинается с нулевой позиции потока.
        Assert.Equal(new long[] { 100, 101, 102, 103 }, Row(inputs, 0));
        Assert.Equal(new long[] { 101, 102, 103, 104 }, Row(targets, 0));
        // Строка партии 1 отстоит от нулевой ровно на длину окна, а не на один
        // токен: это и есть переносимое поведение.
        Assert.Equal(new long[] { 104, 105, 106, 107 }, Row(inputs, 1));
        Assert.Equal(new long[] { 105, 106, 107, 108 }, Row(targets, 1));
        Assert.Equal(new long[] { 108, 109, 110, 111 }, Row(inputs, 2));
        Assert.Equal(new long[] { 109, 110, 111, 112 }, Row(targets, 2));
        Assert.Equal(new long[] { 112, 113, 114, 115 }, Row(inputs, 3));
        Assert.Equal(new long[] { 113, 114, 115, 116 }, Row(targets, 3));
    }

    [Fact]
    public void NextBatch_DefaultStride_CrossesEndOfFileMarkerExactlyOnce()
    {
        using var directory = new TemporaryNpyDirectory();
        // Поток повторяет форму настоящего набора: байты первого файла, маркер
        // конца файла, байты второго файла. Маркер стоит на позиции 8.
        ushort[] stream = [.. Sequence(10, 8), EndOfFileToken, .. Sequence(20, 9)];
        NpyTestFile.Write(directory.File("tokens.npy"), stream);
        var loader = new DataLoader(directory.Path, batchSize: 3, seqLen: 4);

        var (inputs, targets) = loader.NextBatch();

        // При шаге на длину окна окна идут подряд, поэтому маркер конца файла
        // предъявляется как вход ровно одному окну и как цель ровно одному.
        Assert.Equal(1, CountRowsContaining(inputs, EndOfFileToken));
        Assert.Equal(1, CountRowsContaining(targets, EndOfFileToken));
        // Маркер не пропускается, окно по нему не режется, позиция на нём не
        // сбрасывается: он просто обычный токен внутри окна.
        Assert.Equal(new long[] { 15, 16, 17, EndOfFileToken }, Row(targets, 1));
        Assert.Equal(new long[] { EndOfFileToken, 20, 21, 22 }, Row(inputs, 2));
        Assert.Equal(new long[] { 20, 21, 22, 23 }, Row(targets, 2));
    }

    [Fact]
    public void NextBatch_DefaultStride_ReadsEveryShardPositionIncludingTailShorterThanWindow()
    {
        using var directory = new TemporaryNpyDirectory();
        // Десять токенов при длине окна 4: кратные шагу позиции дают окна со
        // смещениями 0 и 4, а на смещении 8 остатка уже не хватает. Хвост
        // осколка обязан быть прочитан, а не пропущен.
        const int shardLength = 10;
        const int windowLength = 4;
        const int tokenBase = 200;
        NpyTestFile.Write(
            directory.File("tokens.npy"),
            Sequence(tokenBase, shardLength));
        var loader = new DataLoader(
            directory.Path,
            batchSize: 1,
            seqLen: windowLength);

        var observedStarts = new List<int>();
        var observedPositions = new HashSet<int>();
        // Восьми обращений хватает, чтобы пройти полный цикл по осколку и
        // вернуться к его началу при любом из рассматриваемых поведений.
        for (int pass = 0; pass < 8; pass++)
        {
            var (inputs, targets) = loader.NextBatch();
            observedStarts.Add((int)(inputs[0, 0] - tokenBase));
            foreach (long token in Row(inputs, 0))
                observedPositions.Add((int)(token - tokenBase));
            foreach (long token in Row(targets, 0))
                observedPositions.Add((int)(token - tokenBase));
        }

        // Главное требование: ни одна позиция осколка не теряется. Падая, эта
        // проверка печатает точные номера непрочитанных позиций, чтобы
        // масштаб потери был виден сразу.
        int[] unreadPositions = [.. Enumerable
            .Range(0, shardLength)
            .Where(position => !observedPositions.Contains(position))];
        Assert.True(
            unreadPositions.Length == 0,
            $"Непрочитанные позиции осколка: {string.Join(", ", unreadPositions)}. " +
            $"Длина осколка {shardLength}, длина окна {windowLength}, " +
            "шаг по умолчанию равен длине окна. " +
            $"Наблюдённые начала окон: {string.Join(", ", observedStarts.Distinct().Order())}. " +
            "Назначенное поведение: последнее окно осколка прижимается к его " +
            "концу с перекрытием предыдущего окна.");

        // Назначенный способ покрытия закрепляется отдельно, чтобы покрытие не
        // могло быть достигнуто укороченным последним окном или иным обходом:
        // за цикл выдаются окна со смещениями 0, 4 и прижатое к концу 5.
        Assert.Equal(
            new[] { 0, 4, shardLength - windowLength - 1 },
            observedStarts.Distinct().Order().ToArray());
    }

    [Fact]
    public void NextBatch_DefaultStride_CoversSixteenDistinctSourcePositionsWithoutRepeats()
    {
        using var directory = new TemporaryNpyDirectory();
        // Значение токена равно 300 плюс его позиция в потоке, поэтому позицию
        // цели можно восстановить из самого значения.
        NpyTestFile.Write(directory.File("tokens.npy"), Sequence(300, 24));
        var loader = new DataLoader(directory.Path, batchSize: 4, seqLen: 4);

        var (_, targets) = loader.NextBatch();

        var targetPositions = new List<long>();
        for (int row = 0; row < targets.GetLength(0); row++)
            foreach (long token in Row(targets, row))
                targetPositions.Add(token - 300);

        // Четыре обновления обучения при длине окна 4 дают 16 предъявлений
        // цели. На шаге по умолчанию все 16 попадают в разные позиции
        // источника, повторных предъявлений нет: ровно это измерил опыт.
        Assert.Equal(16, targetPositions.Count);
        Assert.Equal(16, targetPositions.Distinct().Count());
        Assert.Equal(0, targetPositions.Count - targetPositions.Distinct().Count());
        Assert.Equal(Enumerable.Range(1, 16).Select(position => (long)position).ToArray(),
            targetPositions.Distinct().Order().ToArray());
    }

    [Fact]
    public void NextBatch_DefaultStride_KeepsTargetShiftedByExactlyOneToken()
    {
        using var directory = new TemporaryNpyDirectory();
        NpyTestFile.Write(directory.File("tokens.npy"), Sequence(400, 12));
        var loader = new DataLoader(directory.Path, batchSize: 2, seqLen: 3);

        var (inputs, targets) = loader.NextBatch();

        // Договор сдвига цели на один токен вправо переносом не меняется: он
        // держится и при прежнем шаге в один токен, и при шаге на длину окна.
        for (int row = 0; row < inputs.GetLength(0); row++)
            for (int position = 0; position < inputs.GetLength(1); position++)
                Assert.Equal(inputs[row, position] + 1, targets[row, position]);
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

    /// <summary>
    /// Считает, сколько строк партии содержат заданный токен хотя бы один раз.
    /// </summary>
    private static int CountRowsContaining(long[,] values, long token)
    {
        int matchingRows = 0;
        for (int row = 0; row < values.GetLength(0); row++)
            if (Row(values, row).Contains(token))
                matchingRows++;
        return matchingRows;
    }
}
