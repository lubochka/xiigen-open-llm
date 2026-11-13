// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DatasetPipeline;

namespace LlmFromScratch.Tests;

public class DataLoaderTests
{
    [Fact]
    public void NextBatch_ReturnsShiftedOverlappingWindowsWithBatchAndTimeShape()
    {
        using var directory = new TemporaryNpyDirectory();
        NpyTestFile.Write(directory.File("tokens.npy"), 10, 11, 12, 13, 14);
        // Шаг в один токен задан явно: перекрытие окон — это договор, который
        // закрепляет именно эта проверка, а не поведение по умолчанию. После
        // переноса опыта номер 68 по умолчанию шаг равен длине окна, и на таком
        // коротком осколке прижатое к концу последнее окно случайно даёт те же
        // числа, то есть без явного шага проверка перестала бы различать свой
        // договор от другого механизма.
        var loader = new DataLoader(
            directory.Path,
            batchSize: 2,
            seqLen: 3,
            positionStride: 1);

        var (inputs, targets) = loader.NextBatch();

        Assert.Equal(2, inputs.GetLength(0));
        Assert.Equal(3, inputs.GetLength(1));
        Assert.Equal(2, targets.GetLength(0));
        Assert.Equal(3, targets.GetLength(1));
        Assert.Equal(new long[] { 10, 11, 12 }, Row(inputs, 0));
        Assert.Equal(new long[] { 11, 12, 13 }, Row(targets, 0));
        Assert.Equal(new long[] { 11, 12, 13 }, Row(inputs, 1));
        Assert.Equal(new long[] { 12, 13, 14 }, Row(targets, 1));
    }

    [Fact]
    public void Constructor_ValidatesArgumentsBeforeFilesystemAccess()
    {
        string missingPath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"missing-npy-directory-{Guid.NewGuid():N}");

        Assert.Throws<ArgumentNullException>(
            () => new DataLoader(null!, batchSize: 1, seqLen: 1));
        Assert.Throws<ArgumentException>(
            () => new DataLoader("   ", batchSize: 1, seqLen: 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DataLoader(missingPath, batchSize: 0, seqLen: 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DataLoader(missingPath, batchSize: -1, seqLen: 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DataLoader(missingPath, batchSize: 1, seqLen: 0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DataLoader(missingPath, batchSize: 1, seqLen: -1));
    }

    [Fact]
    public void Constructor_MissingDirectory_ThrowsDirectoryNotFoundException()
    {
        string missingPath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"missing-npy-directory-{Guid.NewGuid():N}");

        Assert.Throws<DirectoryNotFoundException>(
            () => new DataLoader(missingPath, batchSize: 1, seqLen: 1));
    }

    [Fact]
    public void Constructor_NoLowercaseNpyShards_ThrowsInvalidOperationException()
    {
        using var directory = new TemporaryNpyDirectory();
        File.WriteAllText(directory.File("tokens.txt"), "not a shard");
        NpyTestFile.Write(directory.File("tokens.NPY"), 1, 2);

        Assert.Throws<InvalidOperationException>(
            () => new DataLoader(directory.Path, batchSize: 1, seqLen: 1));
    }

    [Fact]
    public void Constructor_UsesOrdinalShardFilenameOrder()
    {
        using var directory = new TemporaryNpyDirectory();
        NpyTestFile.Write(directory.File("a.npy"), 100, 101, 102);
        NpyTestFile.Write(directory.File("Z.npy"), 500, 501, 502);
        var loader = new DataLoader(directory.Path, batchSize: 1, seqLen: 2);

        var (inputs, targets) = loader.NextBatch();

        Assert.Equal(new long[] { 500, 501 }, Row(inputs, 0));
        Assert.Equal(new long[] { 501, 502 }, Row(targets, 0));
    }

    [Fact]
    public void NextBatch_ShortFirstShardAndValidSecondShard_Succeeds()
    {
        using var directory = new TemporaryNpyDirectory();
        NpyTestFile.Write(directory.File("0-short.npy"), 1, 2, 3);
        NpyTestFile.Write(directory.File("1-valid.npy"), 20, 21, 22, 23);
        var loader = new DataLoader(directory.Path, batchSize: 1, seqLen: 3);

        var (inputs, targets) = loader.NextBatch();

        Assert.Equal(new long[] { 20, 21, 22 }, Row(inputs, 0));
        Assert.Equal(new long[] { 21, 22, 23 }, Row(targets, 0));
    }

    [Fact(Timeout = 2000)]
    public async Task NextBatch_AllShardsTooShort_ThrowsInvalidDataExceptionWithinBound()
    {
        using var directory = new TemporaryNpyDirectory();
        NpyTestFile.Write(directory.File("0-short.npy"), 1, 2);
        NpyTestFile.Write(directory.File("1-short.npy"), 3, 4, 5);
        var loader = new DataLoader(directory.Path, batchSize: 1, seqLen: 3);

        Task<(long[,] inputs, long[,] targets)> nextBatch =
            Task.Run(loader.NextBatch);

        await Assert.ThrowsAsync<InvalidDataException>(
            async () => await nextBatch.WaitAsync(TimeSpan.FromSeconds(1)));
    }

    private static long[] Row(long[,] values, int row)
    {
        var result = new long[values.GetLength(1)];
        for (int column = 0; column < result.Length; column++)
            result[column] = values[row, column];
        return result;
    }
}
