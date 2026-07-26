// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AutonomousLearning;

using AgentInfrastructure;
using ContinuousLearning;

// ReplayBuffer stores past training examples and mixes them with new data.
// E43 keeps the final training batch at roughly 70% new and 30% replay.
public sealed class ReplayBuffer : BaseService
{
    public const float ReplayRatio = 0.3f;

    // Keep replay memory bounded across long autonomous learning sessions.
    public const int DefaultMaxCapacity = 4096;

    private readonly List<TrainingExample> _buffer = [];
    private readonly Random _rng;
    private readonly int _maxCapacity;

    public ReplayBuffer(ISessionStorage storage, int? seed = null, int maxCapacity = DefaultMaxCapacity) : base(storage)
    {
        _rng = seed.HasValue ? new Random(seed.Value) : Random.Shared;
        _maxCapacity = Math.Max(1, maxCapacity);
    }

    public int Count => _buffer.Count;

    public void Add(TrainingExample example)
    {
        _buffer.Add(example);
        if (_buffer.Count > _maxCapacity)
            _buffer.RemoveRange(0, _buffer.Count - _maxCapacity);
    }

    public IReadOnlyList<TrainingExample> Sample(IReadOnlyList<TrainingExample> newExamples)
    {
        if (_buffer.Count == 0)
            return newExamples;

        var replayCount = (int)Math.Round(newExamples.Count * ReplayRatio / (1f - ReplayRatio));
        replayCount = Math.Min(replayCount, _buffer.Count);

        var indices = Enumerable.Range(0, _buffer.Count).ToArray();
        for (int i = 0; i < replayCount; i++)
        {
            var j = _rng.Next(i, indices.Length);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        var replayed = indices.Take(replayCount)
            .Select(idx => _buffer[idx])
            .ToList();

        return newExamples.Concat(replayed).ToList();
    }
}
