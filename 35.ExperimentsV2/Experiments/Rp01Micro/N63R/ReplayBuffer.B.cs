// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// Плечо B опыта N63-AR-CAP: та же копия 16.AutonomousLearning/Training/ReplayBuffer.cs
// с теми же техническими изменениями, что и в плече A.
// ЕДИНСТВЕННЫЙ СОДЕРЖАТЕЛЬНЫЙ ФАКТОР опыта находится в методе Add: при переполнении
// удаляется не самая старая запись вообще, а самая старая запись того домена, у которого
// записей больше одной. Так за каждым доменом остаётся хотя бы одно место в общей памяти.
// Sample, коэффициент 0.3, зерно и фильтр по домену не меняются.
namespace N63R.B;

using N63R.Contracts;

// ReplayBuffer stores past training examples and mixes them with new data.
// E43 keeps the final training batch at roughly 70% new and 30% replay.
public sealed class ReplayBuffer
{
    public const float ReplayRatio = 0.3f;

    // Keep replay memory bounded across long autonomous learning sessions.
    public const int DefaultMaxCapacity = 4096;

    // Входной контракт опыта: допускаются только эти два домена, и он проверяется
    // до обучения. Нарушение контракта — ошибка, а не тихая смена политики.
    private static readonly string[] AllowedDomains = ["CODE_Q", "BUG"];

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
        while (_buffer.Count > _maxCapacity)
            _buffer.RemoveAt(OldestIndexOfOverrepresentedDomain());
    }

    // Возвращает позицию первой по порядку вставки записи того домена, у которого
    // в буфере больше одной записи. Именно она и вытесняется.
    private int OldestIndexOfOverrepresentedDomain()
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var stored in _buffer)
        {
            if (!AllowedDomains.Contains(stored.Domain, StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Replay domain '{stored.Domain}' is outside the declared CODE_Q/BUG contract.");
            }

            counts[stored.Domain] = counts.TryGetValue(stored.Domain, out var seen) ? seen + 1 : 1;
        }

        for (var index = 0; index < _buffer.Count; index++)
            if (counts[_buffer[index].Domain] > 1)
                return index;

        throw new InvalidOperationException(
            "Replay capacity exceeded while every domain holds exactly one reserved slot.");
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
