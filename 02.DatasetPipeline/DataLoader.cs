// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DatasetPipeline;

/// <summary>
/// Streams batches from a list of .npy shards.
/// Each call to NextBatch returns (inputs, targets) where targets = inputs shifted left by 1.
/// Шаг продвижения позиции чтения между соседними окнами партии настраивается
/// четвёртым аргументом конструктора. По умолчанию он равен длине окна, то есть
/// окна идут подряд и не пересекаются: при равном числе обновлений обучения это
/// покрывает больше разных позиций источника и не предъявляет один и тот же
/// участок повторно (результат опыта номер 68). Прежнее поведение — шаг в один
/// токен с перекрытием окон — остаётся доступным явным аргументом.
/// </summary>
public class DataLoader
{
    private readonly int _batchSize;
    private readonly int _seqLen;
    // На сколько токенов сдвигается позиция чтения после выдачи одного окна.
    // Всегда положителен: ноль в аргументе конструктора уже превращён в длину
    // окна.
    private readonly int _positionStride;
    private readonly string[] _shardPaths;

    private int _shardIdx;
    private int _pos;
    // Начало последнего выданного окна внутри текущего осколка. Минус один
    // означает, что из этого осколка ещё ни одного окна не выдано. По этому
    // числу видно, не прочитан ли ещё хвост осколка.
    private int _lastWindowStart = -1;
    private ushort[] _current = [];

    public DataLoader(string dataDir, int batchSize, int seqLen, int positionStride = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDir);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seqLen);
        // Шаг проверяется здесь же, до обращения к файловой системе, как и
        // остальные аргументы. Отрицательный шаг отвергается; ноль допустим и
        // означает «шаг по умолчанию».
        ArgumentOutOfRangeException.ThrowIfNegative(positionStride);

        if (!Directory.Exists(dataDir))
            throw new DirectoryNotFoundException($"Data directory not found: {dataDir}");

        _batchSize = batchSize;
        _seqLen = seqLen;
        // Ноль означает шаг по умолчанию, равный длине окна: соседние окна
        // партии идут подряд и не перекрываются.
        _positionStride = positionStride == 0 ? seqLen : positionStride;
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
            // Запоминаем, с какой позиции было выдано это окно: по ней решается,
            // остался ли непрочитанный хвост осколка.
            _lastWindowStart = _pos;
            // Позиция чтения двигается на настраиваемый шаг. По умолчанию он
            // равен длине окна, поэтому следующее окно начинается там, где
            // закончилось предыдущее, вместо почти полного повтора.
            _pos += _positionStride;
        }
        return (inputs, targets);
    }

    private void EnsureAvailable(int needed)
    {
        if (_pos + needed <= _current.Length)
            return;

        // Хвост осколка короче окна. При шаге, равном длине окна, кратные шагу
        // позиции до хвоста не доходят, и его токены не попали бы ни в одно
        // окно — причём на каждом проходе одни и те же. Поэтому выдаём одно
        // последнее окно, прижатое к концу осколка. Перекрытие с предыдущим
        // окном здесь — назначенная цена: одно повторное предъявление на
        // осколок дешевле, чем навсегда потерянные токены.
        int tailAlignedStart = _current.Length - needed;
        if (needed <= _current.Length && tailAlignedStart > _lastWindowStart)
        {
            _pos = tailAlignedStart;
            return;
        }

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

    private void LoadShard(int idx)
    {
        // Читаем осколок целиком в память.
        _current = NpyReader.Read(_shardPaths[idx]);
        // История окон прежнего осколка к новому не относится: отсчёт
        // последнего выданного окна начинается заново, иначе хвост нового
        // осколка мог бы быть сочтён уже прочитанным.
        _lastWindowStart = -1;
    }
}
