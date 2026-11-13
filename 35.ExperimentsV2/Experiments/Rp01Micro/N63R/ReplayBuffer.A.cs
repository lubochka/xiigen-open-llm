// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// Плечо A опыта N63-AR-CAP: точная копия 16.AutonomousLearning/Training/ReplayBuffer.cs.
// Технические изменения, одинаковые для A и B: своё пространство имён, usings
// AgentInfrastructure/ContinuousLearning заменены на N63R.Contracts, убраны базовый
// класс BaseService и конструктор с хранилищем (Add/Sample их не используют).
// Логика вытеснения здесь исходная: удаляются самые старые записи независимо от домена.
namespace N63R.A;

using N63R.Contracts;

// ReplayBuffer stores past training examples and mixes them with new data.
// E43 keeps the final training batch at roughly 70% new and 30% replay.
public sealed class ReplayBuffer
{
    public const float ReplayRatio = 0.3f;

    // Keep replay memory bounded across long autonomous learning sessions.
    public const int DefaultMaxCapacity = 4096;

    private readonly List<TrainingExample> _buffer = [];
    private readonly Random _rng;
    private readonly int _maxCapacity;

    public ReplayBuffer(int? seed = null, int maxCapacity = DefaultMaxCapacity)
    {
        _rng = seed.HasValue ? new Random(seed.Value) : Random.Shared;
        _maxCapacity = Math.Max(1, maxCapacity);
    }

    public int Count => _buffer.Count;

    // Диагностическое чтение содержимого для отчёта; порядок вставки сохраняется.
    public IReadOnlyList<TrainingExample> Snapshot() => _buffer.ToArray();

    public void Add(TrainingExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        _buffer.Add(example);
        if (_buffer.Count > _maxCapacity)
            _buffer.RemoveRange(0, _buffer.Count - _maxCapacity);
    }

    public IReadOnlyList<TrainingExample> Sample(IReadOnlyList<TrainingExample> newExamples)
    {
        ArgumentNullException.ThrowIfNull(newExamples);
        if (_buffer.Count == 0 || newExamples.Count == 0)
            return newExamples;

        var requestedDomains = new HashSet<string>(
            newExamples.Select(static example => example.Domain),
            StringComparer.Ordinal);
        var eligibleIndices = Enumerable.Range(0, _buffer.Count)
            .Where(index => requestedDomains.Contains(_buffer[index].Domain))
            .ToArray();
        if (eligibleIndices.Length == 0)
            return newExamples;

        var replayCount = (int)Math.Round(newExamples.Count * ReplayRatio / (1f - ReplayRatio));
        replayCount = Math.Min(replayCount, eligibleIndices.Length);

        for (int i = 0; i < replayCount; i++)
        {
            var j = _rng.Next(i, eligibleIndices.Length);
            (eligibleIndices[i], eligibleIndices[j]) = (eligibleIndices[j], eligibleIndices[i]);
        }

        var replayed = eligibleIndices.Take(replayCount)
            .Select(idx => _buffer[idx])
            .ToList();

        return newExamples.Concat(replayed).ToList();
    }
}
