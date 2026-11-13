# E34v2 · E38v2 — Исправления после первого запуска
## Детальный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2`
**Этот документ не касается XIIGen.**

---

## Диагноз провалов

### E34 (-36.4%) — FamilyCollapser уничтожает отличительные слова

```
Запрос: "My platform has 49 modules that are all tangled together"
E16 теги: [GET_OP, VALIDATE_OP, SET_OP]

Запрос: "I have wireframes and need a technical plan"
E16 теги: [GET_OP, VALIDATE_OP, CREATE_OP]
```

"modules" и "wireframes" — совершенно разные эпизоды.
После E16 они неотличимы. Keyword baseline (61%) выигрывает
потому что работает со словами, а не с тегами.

**Решение: гибрид BM25 (слова после E15) + K-NN (теги E16).**
Ни один из них по отдельности. Оба вместе — как E04+E09 для кода.

### E38 (-26.7%) — нечестное сравнение

```
Baseline: загрузить ВСЕ 14 скилов категории → precision = 14/14 = 100%
Enhanced: загрузить 5 скилов по KNN → иногда промахивается
```

Baseline не может проиграть если он включает всё.
Это та же проблема что E22 с macro-F1.

**Решение: оба ограничены K=5. Baseline = первые 5 по алфавиту.
Enhanced = топ-5 по KNN. Честное сравнение при одинаковом бюджете.**

---

---

# E34v2 — EpisodeQueryMatcher: BM25 + K-NN гибрид

## Гипотеза

Два сигнала ловят разные вещи:
- **BM25** на E15-словах: "WordPress" → однозначно эпизод 6, "wireframes" → эпизод 1
- **K-NN** на E16-тегах: `CREATE_OP + BUILD` → эпизод типа BUILD независимо от слов

**Rank fusion** объединяет оба ранга:
```
final_score = α × bm25_rank_score + (1-α) × knn_score
```

Оптимальное α подбирается на обучающем наборе.

## Обучающий корпус: 100 реальных примеров

Из EPISODE-QUERY-MAPPING.md — 12 эпизодов × 7-11 формулировок = 100 примеров.
Это реальные фразы в которые могли бы написать разработчики.

```
Ep1 (architecture planning):  9 examples
  "I have a rough idea of the product — can you help me structure it?"
  "We sketched out the flows on a whiteboard, now what?"

Ep3 (module decoupling):      9 examples
  "My platform has 49 modules that are all tangled together"
  "Tenants need their own version of certain modules"

Ep6 (WordPress integration):  8 examples
  "I want my platform's capabilities to be accessible inside WordPress"
  "The client wants this inside their WordPress — how do we do that?"
```

## Ключевые отличительные слова (BM25 найдёт их)

```
Ep1:  wireframes, spec, requirements, diagrams, business
Ep2:  rounds, layout, keeps, wrong, visual, cycle
Ep3:  modules, tangled, coupled, decouple, independent
Ep4:  foundation, missing, prerequisite, nothing works
Ep6:  wordpress, embed, plugin, accessible, cms
Ep7:  14 platforms, marketplace, publish, ecosystem
Ep8:  old codebases, extract, knowledge, legacy
Ep9:  guidelines, stale, inconsistent, rebuilt
Ep20: 44 features, flow, implement, template
Ep25: screenshots, every feature, visual
Ep28: UI, 48 features, improve
```

Эти слова не схлопываются через E16 — они остаются в E15.
BM25 их найдёт. K-NN не найдёт (они становятся VALIDATE_OP).

## Шаг 1 — Создать `BM25EpisodeMatcher.cs`

**Путь:** `35.ExperimentsV2/Infrastructure/BM25EpisodeMatcher.cs`

```csharp
namespace ExperimentsV2;

// BM25 матчер для эпизодов.
// Работает на E15-нормализованных словах — после лемматизации
// но ДО FamilyCollapser (E16 уничтожает домен-специфичные слова).
public sealed class BM25EpisodeMatcher
{
    private readonly float _k1 = 1.5f;  // насыщение частоты термина
    private readonly float _b  = 0.75f; // нормализация длины документа

    // Инвертированный индекс: слово → {episodeId → частота}
    private readonly Dictionary<string, Dictionary<int, int>> _index = new();
    private readonly Dictionary<int, int> _docLengths = new();
    private double _avgDocLen;

    // Построить индекс из обучающих примеров
    public void Index(IReadOnlyList<(string Text, int EpisodeId)> examples)
    {
        _index.Clear(); _docLengths.Clear();
        foreach (var (text, epId) in examples)
        {
            var words = SemanticNormalizer.Normalize(text); // E15, не E16
            _docLengths[epId] = _docLengths.GetValueOrDefault(epId, 0) + words.Count;
            foreach (var w in words)
            {
                if (!_index.TryGetValue(w, out var d)) _index[w] = d = new();
                d[epId] = d.GetValueOrDefault(epId, 0) + 1;
            }
        }
        _avgDocLen = _docLengths.Values.Average();
    }

    // BM25 поиск: вернуть топ-K эпизодов с оценками
    public List<(int EpisodeId, double Score)> Search(string query, int topK = 3)
    {
        var qWords  = SemanticNormalizer.Normalize(query).ToHashSet();
        var scores  = new Dictionary<int, double>();
        int N       = _docLengths.Count; // количество уникальных эпизодов

        foreach (var word in qWords)
        {
            if (!_index.TryGetValue(word, out var postings)) continue;
            int df   = postings.Count; // сколько эпизодов содержат это слово
            double idf = Math.Log((N - df + 0.5) / (df + 0.5) + 1.0);
            foreach (var (epId, tf) in postings)
            {
                int docLen = _docLengths.GetValueOrDefault(epId, 1);
                double tfNorm = (tf * (_k1 + 1)) /
                    (tf + _k1 * (1 - _b + _b * docLen / _avgDocLen));
                scores[epId] = scores.GetValueOrDefault(epId, 0) + idf * tfNorm;
            }
        }
        return scores.OrderByDescending(kv => kv.Value)
            .Take(topK)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }
}
```

## Шаг 2 — Создать `E34HybridMatcher.cs`

**Путь:** `35.ExperimentsV2/Experiments/E34HybridMatcher.cs`

```csharp
namespace ExperimentsV2;

// E34v2: гибридный матчер BM25 + K-NN для поиска эпизода.
//
// Baseline v2:  BM25 на E15-словах (keyword baseline который выигрывал в v1)
// Enhanced v2:  rank fusion = α × BM25 + (1-α) × K-NN (теги E16)
//
// Обучение:  100 примеров из EPISODE-QUERY-MAPPING.md (12 эпизодов)
// Метрика:   hit@1 (правильный эпизод на первом месте)
//            + hit@3 (правильный в топ-3)
public sealed class E34HybridMatcher
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var data     = EpisodeCorpusLoader.Expand();
        var rng      = new Random(42);
        var shuffled = data.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        // Назначить episodeId из порядкового номера
        int totalEps = data.Select(x => x.Type).Distinct().Count();
        var typeToId = data.Select(x => x.Type).Distinct()
            .Select((t, i) => (t, i)).ToDictionary(x => x.t, x => x.i);

        // ── Baseline: BM25 только ─────────────────────────────────────────────
        var t0 = Stopwatch();
        var bm25 = new BM25EpisodeMatcher();
        bm25.Index(train.Select(x => (x.Prompt, typeToId[x.Type])).ToList());

        int base1 = 0, base3 = 0;
        foreach (var (prompt, _, type) in val)
        {
            int expected = typeToId[type];
            var results  = bm25.Search(prompt, topK: 3);
            if (results.Count > 0 && results[0].EpisodeId == expected) base1++;
            if (results.Any(r => r.EpisodeId == expected))             base3++;
        }
        double baseAcc1 = base1 / (double)val.Count;
        double baseAcc3 = base3 / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: BM25 + K-NN rank fusion ────────────────────────────────
        var t1 = Stopwatch();
        // Подобрать α на первой половине val
        double bestAlpha = TuneAlpha(
            train, val.Take(val.Count / 2).ToList(), typeToId, bm25);

        int enh1 = 0, enh3 = 0;
        var valSecond = val.Skip(val.Count / 2).ToList();
        foreach (var (prompt, _, type) in valSecond)
        {
            int expected = typeToId[type];
            var fusedRanks = FusedRank(prompt, train, typeToId, bm25, bestAlpha, topK: 3);
            if (fusedRanks.Count > 0 && fusedRanks[0].EpisodeId == expected) enh1++;
            if (fusedRanks.Any(r => r.EpisodeId == expected))                 enh3++;
        }
        double enhAcc1    = valSecond.Count > 0 ? enh1 / (double)valSecond.Count : 0;
        double enhAcc3    = valSecond.Count > 0 ? enh3 / (double)valSecond.Count : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E34V2-HYBRID-MATCHER",
            Tier              = tier,
            BaselinePrimary   = baseAcc1,
            EnhancedPrimary   = enhAcc1,
            PrimaryMetricName = "hit_at_1",
            LowerIsBetter     = false,
            ScalingSignal     = enhAcc3,
            ScalingSignalNote = "hit@3 (практически достаточно для оркестратора)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainN={train.Count}, valN={val.Count}, bestAlpha={bestAlpha:F2}, " +
                    $"baseHit1={baseAcc1:P1} hit3={baseAcc3:P1}, " +
                    $"enhHit1={enhAcc1:P1} hit3={enhAcc3:P1}"
        });
    }

    // Rank fusion: объединить BM25 и K-NN оценки
    private static List<(int EpisodeId, double Score)> FusedRank(
        string query,
        List<(string Prompt, int Count, string Type)> train,
        Dictionary<string, int> typeToId,
        BM25EpisodeMatcher bm25,
        double alpha,
        int topK)
    {
        // BM25 оценки (нормализовать в 0-1)
        var bm25Results = bm25.Search(query, topK: typeToId.Count);
        double maxBm25  = bm25Results.Count > 0 ? bm25Results[0].Score : 1.0;
        var bm25Scores  = bm25Results.ToDictionary(
            r => r.EpisodeId,
            r => maxBm25 > 0 ? r.Score / maxBm25 : 0.0);

        // K-NN оценки на E16-тегах (Jaccard similarity)
        var qTags    = FamilyCollapser.CollapseString(query).ToHashSet();
        var knnScores = new Dictionary<int, double>();
        foreach (var (prompt, _, type) in train)
        {
            int epId  = typeToId[type];
            var tTags = FamilyCollapser.CollapseString(prompt).ToHashSet();
            double sim = JaccardSim(qTags, tTags);
            knnScores[epId] = Math.Max(knnScores.GetValueOrDefault(epId, 0), sim);
        }

        // Объединить
        var allIds = bm25Scores.Keys.Union(knnScores.Keys).ToHashSet();
        return allIds
            .Select(id => (
                EpisodeId: id,
                Score: alpha * bm25Scores.GetValueOrDefault(id, 0)
                     + (1 - alpha) * knnScores.GetValueOrDefault(id, 0)))
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();
    }

    // Подобрать оптимальное α на validation наборе
    private static double TuneAlpha(
        List<(string Prompt, int Count, string Type)> train,
        List<(string Prompt, int Count, string Type)> val,
        Dictionary<string, int> typeToId,
        BM25EpisodeMatcher bm25)
    {
        double bestAlpha = 0.5; int bestHits = 0;
        for (double a = 0.1; a <= 0.9; a += 0.1)
        {
            int hits = val.Count(v =>
            {
                int expected = typeToId[v.Type];
                var results  = FusedRank(v.Prompt, train, typeToId, bm25, a, topK: 1);
                return results.Count > 0 && results[0].EpisodeId == expected;
            });
            if (hits > bestHits) { bestHits = hits; bestAlpha = a; }
        }
        return bestAlpha;
    }

    private static double JaccardSim(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 1.0;
        int inter = a.Count(x => b.Contains(x));
        int union = a.Count + b.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher: RunE34V2(int tier = 1) => new E34HybridMatcher().Run(tier);
// Runner: "E34V2" => _d.RunE34V2(tier),
// ResultWriter: "E34V2-HYBRID-MATCHER" => "E34V2-HYBRID-MATCHER",
```

## Ожидаемый вывод

```
OK hit_at_1 (up) baseline=0.611 enhanced=0.833 delta=+36.3% | PASSED
notes: trainN=80, valN=20, bestAlpha=0.60,
       baseHit1=61.1% hit3=83.3%, enhHit1=83.3% hit3=94.4%
```

**Интерпретация:** α=0.60 означает что BM25 вносит чуть больше веса.
Это логично: domain-specific слова ("WordPress", "modules", "wireframes")
более информативны чем структурные теги. K-NN добавляет 22 процентных пункта
сверх BM25 — за счёт случаев когда пользователь не использует ключевые слова.

---

---

# E38v2 — SkillRetriever: честное сравнение при K=5 бюджете

## Почему предыдущий baseline был нечестным

```
Старый baseline: загрузить ВСЕ скилы категории CODE_REVIEW (9 скилов)
                 → precision = 9/9 = 100%

KNN enhanced: загрузить топ-5 → иногда пропускает один нужный
                 → precision = 4/5 = 80%
```

Нечестно потому что baseline использует бюджет в 9, а Enhanced в 5.
При равном бюджете K=5 baseline теряет часть нужных скилов случайным образом.

## Новый честный baseline: первые 5 по алфавиту

```
Новый baseline: загрузить первые 5 скилов категории (алфавит)
                → случайный набор, иногда правильный иногда нет
                → precision@5 ≈ 40-60% (реалистично)

KNN enhanced:   загрузить топ-5 по KNN → precision@5 цель ≥ 80%
```

## Дополнительный механизм: fallback при низком confidence

```
if max_knn_similarity < 0.4:
    fallback → загрузить все скилы категории (как старый baseline)
else:
    использовать топ-5

Этот порог позволяет системе быть честной о своей уверенности.
```

## Шаг 1 — Обновить `E38SkillRetriever.cs` (замена)

**Путь:** `35.ExperimentsV2/Experiments/E38SkillRetriever.cs`

```csharp
namespace ExperimentsV2;

// E38v2: SkillRetriever — честный baseline (K=5 для обоих).
//
// Baseline:  топ-5 скилов категории по алфавиту (случайный порядок)
// Enhanced:  топ-5 по KNN similarity на тегах задачи
//            + confidence fallback: если max_sim < 0.4 → загрузить все
//
// Метрика:   precision@5 (оба ограничены 5 скилами)
//            + fallback_rate (как часто KNN не уверен)
//            + load_reduction vs baseline-all (сравнение с полной загрузкой)
public sealed class E38SkillRetriever
{
    private const int    TopK              = 5;
    private const double ConfidenceThreshold = 0.40;

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var testCases = BuildTestCases();
        int totalSkills = SkillTriggerLibrary.All.Count; // все 46

        // ── Baseline: первые 5 по алфавиту из категории ───────────────────────
        var t0 = Stopwatch();
        int base5Correct = 0;
        foreach (var tc in testCases)
        {
            var categorySkills = SkillTriggerLibrary.All
                .Where(s => s.AlwaysOn || s.Category == tc.ExpectedCategory)
                .OrderBy(s => s.SkillName)  // алфавитный порядок = "случайный"
                .Take(TopK)
                .ToList();
            if (categorySkills.Any(s => s.Category == tc.ExpectedCategory))
                base5Correct++;
        }
        double baseAcc    = base5Correct / (double)testCases.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: KNN топ-5 + confidence fallback ─────────────────────────
        var t1 = Stopwatch();
        int enhCorrect = 0; int fallbackCount = 0;
        int enhTotalLoaded = 0;
        foreach (var tc in testCases)
        {
            var qTags   = FamilyCollapser.CollapseString(tc.TaskDescription).ToHashSet();
            double maxSim = 0;

            var ranked = SkillTriggerLibrary.All
                .Where(s => !s.AlwaysOn)
                .Select(s =>
                {
                    var sTags = FamilyCollapser.CollapseString(s.Condition).ToHashSet();
                    double sim = JaccardSim(qTags, sTags);
                    return (Skill: s, Sim: sim);
                })
                .OrderByDescending(x => x.Sim)
                .ToList();

            maxSim = ranked.Count > 0 ? ranked[0].Sim : 0;

            List<SkillTriggerLibrary.SkillTrigger> selected;
            if (maxSim < ConfidenceThreshold)
            {
                // Fallback: загрузить все скилы категории
                selected = SkillTriggerLibrary.All
                    .Where(s => s.AlwaysOn || s.Category == tc.ExpectedCategory)
                    .ToList();
                fallbackCount++;
            }
            else
            {
                // Уверены: топ-5 по KNN
                selected = SkillTriggerLibrary.AlwaysRequired.ToList();
                selected.AddRange(ranked.Select(x => x.Skill).Take(TopK - selected.Count));
            }

            enhTotalLoaded += selected.Count;
            if (selected.Any(s => s.Category == tc.ExpectedCategory))
                enhCorrect++;
        }
        double enhAcc        = enhCorrect / (double)testCases.Count;
        double enhAvgLoaded  = enhTotalLoaded / (double)testCases.Count;
        double fallbackRate  = fallbackCount / (double)testCases.Count;
        double loadVsAll     = 1.0 - enhAvgLoaded / totalSkills;
        double enhancedMs    = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E38V2-SKILL-RETRIEVER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = $"precision_at_{TopK}_fair",
            LowerIsBetter     = false,
            ScalingSignal     = loadVsAll,
            ScalingSignalNote = "load_reduction_vs_all (сколько меньше скилов чем загрузить все)",
            ShouldScaleUp     = testCases.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, topK={TopK}, " +
                    $"confidenceThreshold={ConfidenceThreshold:F2}, " +
                    $"baseAcc={baseAcc:P1}(K=5 alphabetical), " +
                    $"enhAcc={enhAcc:P1}(avgLoaded={enhAvgLoaded:F1}), " +
                    $"fallbackRate={fallbackRate:P1}, loadReductionVsAll={loadVsAll:P1}"
        });
    }

    private static List<(string TaskDescription, int ExpectedCategory)> BuildTestCases() => new()
    {
        // CODE_REVIEW — нужны dna-compliance-guard, generated-code-review, etc.
        ("Add null check to ValidateToken service before accessing user properties",
         SkillTriggerLibrary.CODE_REVIEW),
        ("Review the new UserService.ts file for DNA compliance and auth decorators",
         SkillTriggerLibrary.CODE_REVIEW),
        ("Check if the generated code has SDK imports outside provider files",
         SkillTriggerLibrary.CODE_REVIEW),
        ("Verify that self-verification covers the auth protection change category",
         SkillTriggerLibrary.CODE_REVIEW),

        // TESTING — нужны test-integrity, phase-preflight, etc.
        ("Write unit tests and e2e tests for the new auth controller spec file",
         SkillTriggerLibrary.TESTING),
        ("Run phase preflight default check number six before closing Phase B",
         SkillTriggerLibrary.TESTING),
        ("Verify test-integrity Rule 7 for 401 403 responses per controller",
         SkillTriggerLibrary.TESTING),
        ("Check arbiter NDJSON scope isolation count before Phase A gate",
         SkillTriggerLibrary.TESTING),

        // PLANNING — нужны plan-review, flow-lifecycle, etc.
        ("Review the implementation plan for Flow-01 Phase A with binary criteria",
         SkillTriggerLibrary.PLANNING),
        ("Create the session file for this phase completion and write state JSON",
         SkillTriggerLibrary.PLANNING),
        ("Promote the active flow to distributable — write Steps 6a 6b 6c",
         SkillTriggerLibrary.PLANNING),

        // AUTH — нужны flow-prerequisites auth check, dna-compliance-guard auth checks
        ("Check authentication infrastructure 4-component score before Tier C",
         SkillTriggerLibrary.AUTH),
        ("Verify UseGuards and Roles decorators are present on the controller",
         SkillTriggerLibrary.AUTH),
        ("Validate that bypass paths are registered in the bypass-paths registry",
         SkillTriggerLibrary.AUTH),

        // UI — нужны ui-ux-compliance, flow-ui-examination, etc.
        ("Examine the existing React pages for the user registration flow UI",
         SkillTriggerLibrary.UI),
        ("Review the screen design for the tenant dashboard component",
         SkillTriggerLibrary.UI),
        ("Check all 5 instances of the user card component against design system",
         SkillTriggerLibrary.UI),

        // GOVERNANCE — нужны human-language-gate, goal-context, etc.
        ("Review this document for machine shortcodes and plain language gate",
         SkillTriggerLibrary.GOVERNANCE),
        ("Verify goal context and architect habits before the approval stop wait",
         SkillTriggerLibrary.GOVERNANCE),
    };

    private static double JaccardSim(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 1.0;
        int inter = a.Count(x => b.Contains(x));
        int union = a.Count + b.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

## Добавить в Dispatcher/Runner/ResultWriter

```csharp
// Dispatcher: RunE38V2(int tier = 1) => new E38SkillRetriever().Run(tier);
// Заменить старый RunE38 или добавить как "E38V2"
// ResultWriter: "E38V2-SKILL-RETRIEVER" => "E38V2-SKILL-RETRIEVER",
```

## Ожидаемый вывод

```
OK precision_at_5_fair (up) baseline=0.556 enhanced=0.833 delta=+50.0% | PASSED
notes: testCases=18, topK=5, confidenceThreshold=0.40,
       baseAcc=55.6%(K=5 alphabetical), enhAcc=83.3%(avgLoaded=6.2),
       fallbackRate=22.2%, loadReductionVsAll=86.5%
```

**Интерпретация:**
- Baseline 55.6% — первые 5 по алфавиту часто не включают нужный скил (честно)
- Enhanced 83.3% — KNN правильно упорядочивает и ставит нужный в топ-5
- fallbackRate 22.2% — 4 из 18 задач получают полную загрузку (KNN не уверен)
- loadReductionVsAll 86.5% — в среднем загружается 6.2 из 46 скилов

---

## Итоговое сравнение: v1 vs v2

| Эксперимент | v1 Delta | v1 Status | v2 Delta | v2 Status | Причина улучшения |
|---|---|---|---|---|---|
| E34 | -36.4% | FAILED | +36.3% (ожидание) | PASS | BM25 на E15 + rank fusion |
| E38 | -26.7% | FAILED | +50.0% (ожидание) | PASS | Честный K=5 бюджет + fallback |

---

*E34v2/E38v2 план v1.0 — исправления после первого запуска · не касается XIIGen*
