// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E43: ReplayBuffer — 70/30 смешивание предотвращает катастрофическое забывание.
// Baseline:  обучить только на новых данных (100% SECURITY на раунде 2)
// Enhanced:  70% SECURITY + 30% из Replay Buffer (CODE_Q из раунда 1)
// Метрика:   retention_accuracy на CODE_Q после обучения на SECURITY
public sealed class E43ReplayBuffer
{
    private const float ReplayRatio = 0.3f;

    // projectId — проект, которому принадлежат поданные файлы. Довод необязательный и стоит
    // последним, поэтому все прежние вызывающие работают без правок. Он нужен затем, что ключ
    // происхождения потока — это проект И файл: без проекта одинаковая метка файла в двух
    // проектах выглядела бы одним и тем же материалом.
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1,
        string projectId = DefaultProjectId)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 4 files");

        var rng       = new Random(42);
        int split     = (int)(files.Count * 0.5);
        var oldDomain = files.Take(split).ToList();
        var newDomain = files.Skip(split).ToList();

        // Пары нарезаются ПО ФАЙЛАМ с проверенным происхождением, а не из одного склеенного
        // списка. Отказ происхождения возвращается своим названным кодом и наружу не бросается.
        var oldExtracted = ExtractPairs(oldDomain, projectId);
        if (!oldExtracted.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail(oldExtracted.ErrorMessage, oldExtracted.ErrorCode);
        var newExtracted = ExtractPairs(newDomain, projectId);
        if (!newExtracted.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail(newExtracted.ErrorMessage, newExtracted.ErrorCode);

        // Для самой модели нужны только значения концов пары; происхождение остаётся у
        // проверенных пар и используется для учёта швов ниже.
        var oldPairs = oldExtracted.Value!.Select(pair => (pair.Current, pair.Next)).ToList();
        var newPairs = newExtracted.Value!.Select(pair => (pair.Current, pair.Next)).ToList();

        // Сколько пар дала бы прежняя склейка всех файлов в один список. Разница с нынешним
        // числом — это ровно пары-швы, которых в материале нет: по одной на каждую границу
        // файлов, давшую непустой поток.
        var seamsRemoved = SeamCount(oldDomain) + SeamCount(newDomain);

        if (oldPairs.Count < 5 || newPairs.Count < 5)
            return OperationResult<ExperimentV2Result>.Fail("Too few pairs");

        int valSplit = (int)(oldPairs.Count * 0.8);
        var oldTrain = oldPairs.Take(valSplit).ToList();
        var oldVal   = oldPairs.Skip(valSplit).ToList();

        var baseModel    = BuildBigram(oldTrain);
        double baseAccOnOld = Evaluate(baseModel, oldVal);

        // ── Baseline: только новые данные (без replay) ────────────────────────
        var t0 = Stopwatch();
        var noReplayModel   = BuildBigram(newPairs);
        double noReplayAcc  = Evaluate(noReplayModel, oldVal);
        double forgetting   = baseAccOnOld - noReplayAcc;
        double baselineMs   = ElapsedMs(t0);

        // ── Enhanced: 70% новые + 30% Replay Buffer ───────────────────────────
        var t1 = Stopwatch();
        int replayCount  = (int)(newPairs.Count * ReplayRatio / (1 - ReplayRatio));
        var replayBuffer = oldTrain.OrderBy(_ => rng.Next()).Take(replayCount).ToList();
        var mixedPairs   = newPairs.Concat(replayBuffer).OrderBy(_ => rng.Next()).ToList();
        var replayModel  = BuildBigram(mixedPairs);
        double replayAcc = Evaluate(replayModel, oldVal);
        double retention = baseAccOnOld > 0 ? replayAcc / baseAccOnOld : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E43-REPLAY-BUFFER",
            Tier              = tier,
            BaselinePrimary   = noReplayAcc,
            EnhancedPrimary   = replayAcc,
            PrimaryMetricName = "retention_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = retention,
            ScalingSignalNote = "retention_rate (1.0 = полное сохранение, < 0.7 = плохо)",
            ShouldScaleUp     = oldVal.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"oldPairs={oldPairs.Count}, newPairs={newPairs.Count}, " +
                    $"replay={replayBuffer.Count}, baseOnOld={baseAccOnOld:P1}, " +
                    $"noReplay={noReplayAcc:P1}(forgot={forgetting:+.1%}), " +
                    $"withReplay={replayAcc:P1}(retained={retention:P1}), " +
                    $"seamsRemoved={seamsRemoved}, project={projectId}"
        });
    }

    // DefaultProjectId — проект по умолчанию для прежних вызывающих, которые проект не называют.
    // Он назван явной постоянной, а не пустой строкой: ключ происхождения обязан быть непустым,
    // и подставленное значение должно быть видно в отчёте, а не выглядеть настоящим проектом.
    public const string DefaultProjectId = "experiments-v2-local";

    // ExtractPairs нарезает пары ПО ФАЙЛАМ с проверенным происхождением.
    //
    // Что было неправильно. Прежняя нарезка складывала предикаты всех файлов в один список и
    // брала соседние элементы. На каждой границе файлов это давало пару-шов: последний предикат
    // одного файла становился «предыдущим» для первого предиката следующего. Такой пары в
    // материале нет — она изобретена склейкой, и обучение считало её законным переходом.
    // Происхождения у пары при этом не было вовсе: ни проекта, ни файла, ни позиции.
    //
    // Что стало. Каждый файл объявляется доверенным потоком со своим ключом проекта и файла и
    // отпечатком, пересчитанным из фактических опознаний; пары берутся только внутри потока.
    // Швов не возникает по построению, а у каждой пары есть проверяемое происхождение.
    private static OperationResult<IReadOnlyList<E43TrustedPair>> ExtractPairs(
        IReadOnlyList<(string Path, string Text)> files,
        string projectId)
    {
        var streams = new List<E43TrustedStream>();   // Доверенные потоки по одному на файл.
        foreach (var (path, text) in files)
        {
            var extracted = LogicExtractor.Extract(text); // Предикаты одного файла.
            if (!extracted.IsSuccess) continue;           // Нечитаемый файл потоком не становится.

            var stream = E43PairProvenance.BuildStream(projectId, path, extracted.Value!);
            // Поток без ключа — названный отказ, а не молчаливый пропуск материала.
            if (!stream.IsSuccess)
                return OperationResult<IReadOnlyList<E43TrustedPair>>.Fail(
                    stream.ErrorMessage, stream.ErrorCode);

            streams.Add(stream.Value!); // Поток принят вместе со своим происхождением.
        }

        // Нарезка внутри потоков: проверка происхождения идёт до неё и отказ отдаётся наружу.
        return E43PairProvenance.ExtractPairsWithinStreams(streams);
    }

    // SeamCount считает, сколько пар-швов дала бы прежняя склейка всех файлов в один список:
    // по одной на каждую границу между двумя непустыми потоками предикатов.
    private static int SeamCount(IReadOnlyList<(string Path, string Text)> files)
    {
        var nonEmpty = 0; // Сколько файлов дали непустой поток предикатов.
        foreach (var (_, text) in files)
        {
            var extracted = LogicExtractor.Extract(text); // Предикаты одного файла.
            if (extracted.IsSuccess && extracted.Value!.Count > 0) nonEmpty++; // Непустой поток.
        }

        // Границ между непустыми потоками на единицу меньше, чем самих потоков.
        return nonEmpty > 1 ? nonEmpty - 1 : 0;
    }

    private static Dictionary<int, Dictionary<int, int>> BuildBigram(
        IEnumerable<(int, int)> pairs)
    {
        var m = new Dictionary<int, Dictionary<int, int>>();
        foreach (var (c, n) in pairs)
        {
            if (!m.TryGetValue(c, out var d)) m[c] = d = new();
            d[n] = d.GetValueOrDefault(n, 0) + 1;
        }
        return m;
    }

    private static double Evaluate(Dictionary<int, Dictionary<int, int>> model,
        List<(int, int)> val)
    {
        if (val.Count == 0) return 0;
        return val.Count(p =>
        {
            if (!model.TryGetValue(p.Item1, out var d) || d.Count == 0) return false;
            return d.MaxBy(kv => kv.Value).Key == p.Item2;
        }) / (double)val.Count;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
