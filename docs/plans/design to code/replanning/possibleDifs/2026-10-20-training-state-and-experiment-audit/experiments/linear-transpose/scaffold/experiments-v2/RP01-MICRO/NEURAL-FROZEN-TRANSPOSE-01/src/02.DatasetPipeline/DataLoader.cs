// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DatasetPipeline;

/// <summary>
/// Streams batches from a list of .npy shards.
/// Each call to NextBatch returns (inputs, targets) where targets = inputs shifted left by 1.
/// </summary>
public class DataLoader
{
    private readonly int _batchSize;
    private readonly int _seqLen;
    private readonly string[] _shardPaths;

    private int _shardIdx;
    private int _pos;
    private ushort[] _current = [];

    public DataLoader(string dataDir, int batchSize, int seqLen)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDir);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seqLen);

        if (!Directory.Exists(dataDir))
            throw new DirectoryNotFoundException($"Data directory not found: {dataDir}");

        _batchSize = batchSize;
        _seqLen = seqLen;
        _shardPaths = Directory
            .EnumerateFiles(dataDir, "*.npy", SearchOption.TopDirectoryOnly)
            .Where(path => StringComparer.Ordinal.Equals(Path.GetExtension(path), ".npy"))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        if (_shardPaths.Length == 0)
            throw new InvalidOperationException($"No .npy shards found in {dataDir}");
        LoadShard(0);
    }

    public (long[,] inputs, long[,] targets) NextBatch()
    {
        var inputs = new long[_batchSize, _seqLen];
        var targets = new long[_batchSize, _seqLen];

        for (int b = 0; b < _batchSize; b++)
        {
            EnsureAvailable(_seqLen + 1);
            for (int t = 0; t < _seqLen; t++)
                inputs[b, t] = _current[_pos + t];
            for (int t = 0; t < _seqLen; t++)
                targets[b, t] = _current[_pos + t + 1];
            _pos++;
        }
        return (inputs, targets);
    }

    private void EnsureAvailable(int needed)
    {
        if (_pos + needed <= _current.Length)
            return;

        for (int inspected = 0; inspected < _shardPaths.Length; inspected++)
        {
            _shardIdx = (_shardIdx + 1) % _shardPaths.Length;
            LoadShard(_shardIdx);
            _pos = 0;

            if (needed <= _current.Length)
                return;
        }

        throw new InvalidDataException(
            $"None of the {_shardPaths.Length} .npy shards contains the required {needed} tokens.");
    }

    private void LoadShard(int idx) => _current = NpyReader.Read(_shardPaths[idx]);
}
