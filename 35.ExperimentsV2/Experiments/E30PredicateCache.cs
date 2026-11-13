// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E30: PredicateCache — кэш семантически схожих предикатных векторов.
// Baseline:  биграмная модель без кэша
// Enhanced:  кэш (косинусное сходство 44-разрядных адресов: 22 разряда портрета
//            файла и 22 разряда блока текущего действия) + биграм как fallback
// Метрика:   pred_accuracy + cache_hit_rate
public sealed class E30PredicateCache
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 4 files");

        int split      = (int)(files.Count * 0.7);
        var trainFiles = files.Take(split).ToList();
        var testFiles  = files.Skip(split).ToList();

        var trainPairs = ExtractPairsWithVecs(trainFiles);
        var testPairs  = ExtractPairsWithVecs(testFiles);

        if (trainPairs.Count < 5 || testPairs.Count < 3)
            return OperationResult<ExperimentV2Result>.Fail("Too few predicate pairs");

        // Номера текущего действия вне диапазона блока адреса считаются явно.
        // Отбрасывать их молча нельзя: у таких пар блок остаётся нулевым, то есть
        // они различаются только портретом файла — это ровно прежнее поведение, и
        // его обязательно видеть в заметках результата.
        int currentOutOfRange = trainPairs.Count(p => !p.CurrentEncoded)
                              + testPairs.Count(p => !p.CurrentEncoded);

        // Человеческое сообщение о таких номерах: при нуле — просто ноль, иначе
        // предупреждение с количеством и названной причиной.
        string currentOutOfRangeNote = currentOutOfRange == 0
            ? "currentOutOfRange=0"
            : $"currentOutOfRange={currentOutOfRange} (ВНИМАНИЕ: номер текущего предиката "
              + $"вне диапазона 0..{CurrentPredicateDimensions - 1}; у таких пар блок текущего "
              + "действия нулевой, и они различаются только портретом файла)";

        // ── Baseline: чистая биграмная модель ────────────────────────────────
        var t0 = Stopwatch();
        var bigram = BuildBigram(trainPairs.Select(x => (x.Current, x.Next)));
        int baseOk = testPairs.Count(p =>
        {
            if (!bigram.TryGetValue(p.Current, out var d) || d.Count == 0) return false;
            return d.MaxBy(kv => kv.Value).Key == p.Next;
        });
        double baseAcc    = baseOk / (double)testPairs.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: кэш + биграм как fallback ──────────────────────────────
        var t1 = Stopwatch();
        var cache = new PredicateCache(threshold: 0.85f);
        foreach (var p in trainPairs) cache.Add(p.Vec, p.Next);

        int hits = 0, cacheOk = 0, enhOk = 0;
        foreach (var p in testPairs)
        {
            var (found, cached) = cache.Lookup(p.Vec);
            int prediction;
            if (found)
            {
                hits++;
                prediction = cached;
                if (cached == p.Next) cacheOk++;
            }
            else
            {
                prediction = bigram.TryGetValue(p.Current, out var d) && d.Count > 0
                    ? d.MaxBy(kv => kv.Value).Key : -1;
            }
            if (prediction == p.Next) enhOk++;
        }
        double enhAcc     = enhOk / (double)testPairs.Count;
        double hitRate    = hits / (double)testPairs.Count;
        double cacheAcc   = hits > 0 ? cacheOk / (double)hits : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E30-PREDICATE-CACHE",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "pred_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = hitRate,
            ScalingSignalNote = "cache_hit_rate (выше = больше пар кэшируется)",
            ShouldScaleUp     = testPairs.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            // К прежним заметкам добавлены длина адреса и учёт номеров текущего
            // действия вне диапазона: по ним видно, что память адресуется парой
            // «файл и текущее действие», а не одним портретом файла.
            Notes = $"trainPairs={trainPairs.Count}, testPairs={testPairs.Count}, " +
                    $"cacheSize={cache.StoreCount}, hits={hits}({hitRate:P1}), " +
                    $"cacheAcc={cacheAcc:P1}, baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}, " +
                    $"addressDims={TransitionAddressDimensions}, {currentOutOfRangeNote}"
        });
    }

    // ── Адрес памяти перехода ───────────────────────────────────────────────
    // Раньше адресом служил только портрет файла — доли видов действий в этом
    // файле. Портрет один на весь файл, а вопросов к файлу много: «что идёт после
    // проверки на пустую ссылку?» и «что идёт после возврата значения?» попадали
    // в ОДНУ строку памяти. Записывалось несколько ответов, а отвечала всегда
    // самая ранняя запись: при равной похожести чтение оставляет ту, которую
    // встретило раньше. Остальные становились недостижимыми.
    //
    // Теперь к портрету файла дописан блок текущего действия: единица на позиции
    // текущего предиката и нули на остальных. Предсказываемое действие в адрес не
    // входит — иначе память читала бы ответ из самого вопроса.
    //
    // Основание: опыт «адрес памяти кэша предсказания следующего предиката»
    // (номер опыта P47-PRIMARY-01, подслучай P47.1). На его контрольной панели
    // учебные ответы поднялись с 8 из 12 до 12 из 12, разрушительные коллизии
    // адресов убрались с 4 до 0, ни одной верной строки не потеряно.

    // Разрядность портрета файла. Ровно столько разрядов строит продуктовая
    // сборка портрета `PredicateCache.BuildVector`, и менять её здесь нельзя.
    public const int FileVectorDimensions = 22;

    // Ширина блока текущего действия. Это собственный установленный диапазон
    // этого механизма: разборчик кода нумерует предикаты от 0 до 21, поэтому блок
    // шириной 22 покрывает их все. Более узкий блок молча терял бы старшие
    // номера, а число «6» из набора опыта — это размер словаря ТОГО набора, а не
    // размер словаря предикатов продукта, и переносить его сюда запрещено.
    public const int CurrentPredicateDimensions = 22;

    // Полная длина адреса. Все адреса одного прогона обязаны быть этой длины:
    // мера похожести `PredicateCache.CosineSim` идёт циклом по длине ПЕРВОГО
    // вектора, поэтому адрес другой длины в том же хранилище дал бы чтение за
    // границей массива.
    public const int TransitionAddressDimensions =
        FileVectorDimensions + CurrentPredicateDimensions;

    // Укладывается ли номер текущего действия в блок адреса. Нужен вызывающему,
    // чтобы посчитать и показать номера вне диапазона, а не потерять их молча.
    public static bool IsCurrentPredicateEncodable(int currentPredicate)
        => currentPredicate >= 0 && currentPredicate < CurrentPredicateDimensions;

    // Собирает адрес памяти перехода: портрет файла плюс блок текущего действия.
    // Портрет считается один раз на файл и передаётся сюда как есть; новый массив
    // создаётся на каждую пару, поэтому две пары одного файла больше не делят
    // один адрес.
    public static float[] BuildTransitionAddress(float[] fileVector, int currentPredicate)
    {
        // Адрес всегда одной и той же полной длины — и когда текущее действие
        // укладывается в блок, и когда нет.
        var address = new float[TransitionAddressDimensions];

        // Портрет файла занимает первые разряды без изменений. Копируется ровно
        // столько, сколько есть на входе, но не больше отведённого места: короткий
        // или длинный портрет не должен выходить за свою часть адреса.
        int copied = Math.Min(fileVector.Length, FileVectorDimensions);
        Array.Copy(fileVector, address, copied);

        // Единица на позиции текущего действия внутри его блока. Именно она и
        // разводит разные вопросы к одному файлу по разным строкам памяти.
        if (IsCurrentPredicateEncodable(currentPredicate))
            address[FileVectorDimensions + currentPredicate] = 1f;

        // Номер вне диапазона 0..21 оставляет блок нулевым: строка не теряется, но
        // и ложного различения не появляется. Такой случай обязан быть посчитан
        // вызывающим через `IsCurrentPredicateEncodable`.
        return address;
    }

    // Пара обучения. Поле `CurrentEncoded` говорит, уложился ли номер текущего
    // действия в блок адреса: по нему вызывающий считает номера вне диапазона.
    private sealed record PairWithVec(float[] Vec, int Current, int Next, bool CurrentEncoded);

    private static List<PairWithVec> ExtractPairsWithVecs(
        IReadOnlyList<(string Path, string Text)> files)
    {
        var result = new List<PairWithVec>();
        foreach (var (_, text) in files)
        {
            var r = LogicExtractor.Extract(text);
            if (!r.IsSuccess || r.Value!.Count < 2) continue;
            var preds = r.Value!;

            // Портрет файла по-прежнему считается один раз на файл: он описывает
            // файл целиком и от текущего действия не зависит.
            var vec   = PredicateCache.BuildVector(text);

            for (int i = 0; i < preds.Count - 1; i++)
                // Адрес строится НА КАЖДУЮ пару, с текущим действием внутри.
                // Здесь и была потеря условия перехода: прежде на все пары файла
                // уходил один и тот же портрет.
                result.Add(new PairWithVec(
                    BuildTransitionAddress(vec, preds[i]),
                    preds[i],
                    preds[i + 1],
                    IsCurrentPredicateEncodable(preds[i])));
        }
        return result;
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

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
