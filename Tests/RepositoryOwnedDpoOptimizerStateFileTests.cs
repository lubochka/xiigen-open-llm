// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;
using AgentInfrastructure;
using System.Security.Cryptography;
using System.Text;
using TensorCore.Core;
using TensorCore.Layers;
using TrainingLoop.Optimizers;
using Xunit;

// Проверка файловой границы состояния оптимизатора внутри каталога попытки.
// Здесь доказывается сохранение и повторное открытие памяти оптимизатора с
// отдельной суммой, а не качество обучения модели.
public sealed class RepositoryOwnedDpoOptimizerStateFileTests
{
    private static Parameter CreateParameter(int length, float seedValue)
    {
        var tensor = new Tensor(new TensorShape(length));
        for (var index = 0; index < length; index++)
            tensor.Data[index] = seedValue + index;
        return new Parameter(tensor);
    }

    private static (AdamW Optimizer, AdamWParameterBinding[] Bindings) CreateTrainedOptimizer()
    {
        var firstParameter = new Parameter(new Tensor(
            new float[] { 1f, 2f, 3f, 4f, 5f, 6f }, new TensorShape(2, 3)));
        var secondParameter = CreateParameter(3, 7f);
        var bindings = new[]
        {
            new AdamWParameterBinding("block.0.weight", firstParameter),
            new AdamWParameterBinding("block.0.bias", secondParameter),
        };

        foreach (var binding in bindings)
        {
            var gradient = new Tensor(binding.Parameter.Value.Shape);
            for (var index = 0; index < gradient.Data.Length; index++)
                gradient.Data[index] = 0.5f - (index * 0.1f);
            binding.Parameter.Value.Grad = gradient;
        }

        var optimizer = new AdamW();
        optimizer.Step(bindings.Select(binding => binding.Parameter), 0.001f);
        return (optimizer, bindings);
    }

    private static string CreateAttemptRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "xiigen-attempt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    [Fact]
    public void WriteAndReadBack_PersistsOptimizerMemoryWithItsOwnHashAndReopensIt()
    {
        var attemptRoot = CreateAttemptRoot();
        try
        {
            var (optimizer, bindings) = CreateTrainedOptimizer();
            var exported = optimizer.ExportPortableState(bindings);

            var written = RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                attemptRoot,
                step: 1,
                exported);

            Assert.True(written.IsSuccess);
            Assert.NotNull(written.Value);
            Assert.Equal("dpo_optimizer_step_000001.bin", written.Value!.OptimizerStateFileName);
            Assert.True(File.Exists(written.Value.OptimizerStatePath));
            Assert.Equal(64, written.Value.OptimizerStateSha256.Length);
            Assert.True(written.Value.OptimizerStateByteLength > 0);
            Assert.Equal(exported.Step, written.Value.State.Step);

            // Отдельное открытие с ожидаемой суммой: так маршрут возобновления
            // сможет доказать, что память оптимизатора не подменена.
            var reopened = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
                attemptRoot,
                step: 1,
                written.Value.OptimizerStateSha256);
            Assert.True(reopened.IsSuccess);
            Assert.NotNull(reopened.Value);
            Assert.Equal(2, reopened.Value!.State.FormatVersion);
            Assert.Equal(new[] { 2, 3 }, reopened.Value.State.Entries[0].Dimensions);

            // Новый оптимизатор после перезапуска процесса принимает состояние и
            // выдаёт те же байты.
            var restarted = new AdamW();
            Assert.True(restarted.TryImportPortableState(bindings, reopened.Value!.State));
            Assert.Equal(exported.ToBytes(), restarted.ExportPortableState(bindings).ToBytes());
        }
        finally
        {
            DeleteDirectoryIfExists(attemptRoot);
        }
    }

    [Fact]
    public void OpenExisting_V1WithoutDimensions_PreservesBytesAndRejects()
    {
        var attemptRoot = CreateAttemptRoot();
        try
        {
            // Genuine old layout: header/key/momentLength/m/v, no rank or dimensions.
            using var buffer = new MemoryStream();
            using (var writer = new BinaryWriter(buffer, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(Encoding.ASCII.GetBytes("XIIGADWS"));
                writer.Write(1);
                writer.Write(1);
                writer.Write(1);
                var key = Encoding.UTF8.GetBytes("block.0.weight");
                writer.Write(key.Length);
                writer.Write(key);
                writer.Write(6);
                for (var i = 0; i < 6; i++) writer.Write(0.25f + i);
                for (var i = 0; i < 6; i++) writer.Write(0.5f + i);
            }
            var bytes = buffer.ToArray();
            var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            var path = Path.Combine(attemptRoot, RepositoryOwnedDpoOptimizerStateFile.BuildFileName(1));
            using (var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                path, FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }
            Assert.Null(AdamWPortableState.FromBytes(bytes));
            var rejected = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(attemptRoot, 1, hash);
            Assert.False(rejected.IsSuccess);
            Assert.Equal(RepositoryOwnedDpoOptimizerStateFile.InvalidErrorCode, rejected.ErrorCode);
            Assert.Equal(bytes, File.ReadAllBytes(path));
            Assert.Equal(hash, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant());
            Assert.Single(Directory.GetFiles(attemptRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(attemptRoot);
            Assert.False(Directory.Exists(attemptRoot));
        }
    }

    [Fact]
    public void WriteAndReadBack_RefusesToOverwriteAnExistingStepFile()
    {
        var attemptRoot = CreateAttemptRoot();
        try
        {
            var (optimizer, bindings) = CreateTrainedOptimizer();
            var exported = optimizer.ExportPortableState(bindings);

            Assert.True(RepositoryOwnedDpoOptimizerStateFile
                .WriteAndReadBack(attemptRoot, step: 1, exported).IsSuccess);

            var second = RepositoryOwnedDpoOptimizerStateFile
                .WriteAndReadBack(attemptRoot, step: 1, exported);
            Assert.False(second.IsSuccess);
            Assert.Equal(
                RepositoryOwnedDpoOptimizerStateFile.UnavailableErrorCode,
                second.ErrorCode);
        }
        finally
        {
            DeleteDirectoryIfExists(attemptRoot);
        }
    }

    [Fact]
    public void OpenExisting_MissingFile_FailsClosedInsteadOfReturningEmptyState()
    {
        var attemptRoot = CreateAttemptRoot();
        try
        {
            var missing = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
                attemptRoot,
                step: 1);
            Assert.False(missing.IsSuccess);
            Assert.Equal(
                RepositoryOwnedDpoOptimizerStateFile.UnavailableErrorCode,
                missing.ErrorCode);
        }
        finally
        {
            DeleteDirectoryIfExists(attemptRoot);
        }
    }

    [Fact]
    public void OpenExisting_HashMismatch_IsTreatedAsCorruption()
    {
        var attemptRoot = CreateAttemptRoot();
        try
        {
            var (optimizer, bindings) = CreateTrainedOptimizer();
            var exported = optimizer.ExportPortableState(bindings);
            var written = RepositoryOwnedDpoOptimizerStateFile
                .WriteAndReadBack(attemptRoot, step: 1, exported);
            Assert.True(written.IsSuccess);

            var wrongHash = new string('a', 64);
            var mismatch = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
                attemptRoot,
                step: 1,
                wrongHash);
            Assert.False(mismatch.IsSuccess);
            Assert.Equal(
                RepositoryOwnedDpoOptimizerStateFile.InvalidErrorCode,
                mismatch.ErrorCode);
        }
        finally
        {
            DeleteDirectoryIfExists(attemptRoot);
        }
    }

    [Fact]
    public void OpenExisting_CorruptBytes_FailsClosedBeforeAnyImport()
    {
        var attemptRoot = CreateAttemptRoot();
        try
        {
            var (optimizer, bindings) = CreateTrainedOptimizer();
            var exported = optimizer.ExportPortableState(bindings);
            var written = RepositoryOwnedDpoOptimizerStateFile
                .WriteAndReadBack(attemptRoot, step: 1, exported);
            Assert.True(written.IsSuccess);

            // Порча заголовка файла на диске.
            var bytes = File.ReadAllBytes(written.Value!.OptimizerStatePath);
            bytes[0] = (byte)(bytes[0] ^ 0xFF);
            File.WriteAllBytes(written.Value.OptimizerStatePath, bytes);

            var corrupt = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
                attemptRoot,
                step: 1);
            Assert.False(corrupt.IsSuccess);
            Assert.Equal(
                RepositoryOwnedDpoOptimizerStateFile.InvalidErrorCode,
                corrupt.ErrorCode);
        }
        finally
        {
            DeleteDirectoryIfExists(attemptRoot);
        }
    }

    [Fact]
    public void WriteAndReadBack_RejectsMissingAttemptRootAndInvalidStep()
    {
        var (optimizer, bindings) = CreateTrainedOptimizer();
        var exported = optimizer.ExportPortableState(bindings);

        var noRoot = RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
            Path.Combine(Path.GetTempPath(), "xiigen-absent-" + Guid.NewGuid().ToString("N")),
            step: 1,
            exported);
        Assert.False(noRoot.IsSuccess);

        var attemptRoot = CreateAttemptRoot();
        try
        {
            var zeroStep = RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                attemptRoot,
                step: 0,
                exported);
            Assert.False(zeroStep.IsSuccess);
            Assert.Equal(
                RepositoryOwnedDpoOptimizerStateFile.InvalidErrorCode,
                zeroStep.ErrorCode);

            var nullState = RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack(
                attemptRoot,
                step: 1,
                null);
            Assert.False(nullState.IsSuccess);
        }
        finally
        {
            DeleteDirectoryIfExists(attemptRoot);
        }
    }
}
