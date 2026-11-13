# MICRO-N30 · быстро проверить, какие E05/E07/E20/E21 отличия реально портят измеренное качество

**Состояние 16.10.2026:** runnable **source-logic-only** Python demonstration подготовлена (см. `30-historical-seams-microproxy.py` и JSON receipt). **Ни один native .NET test, Student generation, LoRA training, independent CODEX/Astra judge не запускался.** Материал — план следующего короткого C# опыта, не retrospective rerun архивов. Входные эксперименты и frozen historical artifacts не редактировать.

## 1. Первый приоритет: E05-LK «маска знает ответы из validation?»

**Вопрос:** улучшение E05 происходит из выученной C# синтаксической обобщаемости или из наблюдения holdout продолжения при построении mask?

- **Experiment ID:** `MICRO-N30-E05-LK`, Tier1=1–10 C# source files; затем Tier2=11–100 независимых файлов **только если Tier1 неразличителен**.
- **Baseline A (archive parity):** `GrammarMaskFilter.Train(trainTokens, allSourcesIncludingHeldout, kindToIdx)`, при том же `NgramPredictor`.
- **Enhanced B (fair feature gate):** `GrammarMaskFilter.Train(trainTokens, trainOnlySources, sameKnownKinds)`; ни одна heldout `(prev2,prev1,next)` не находится в построении mask. Input/output и версия AST kind codec те же. Только **одна** причинная переменная: corpus источников для grammar table.
- **Стратификация:** 8 маленьких C# методов из ≥2 разных source families, синтаксис null check/throw/await/loop/brace; минимум 3–4 метода на family. Разделить по файлу/методу и family; baseline и enhanced оцениваются на **точно одной и той же** панели, без training annotation из проверки. Дополнительный *within-file replication* 80/20 воспроизводит legacy bug, но **не** подменяет file-disjoint fairness. Специально добавить OOV SyntaxKind и неизвестный context.
- **Внутренняя трасса каждой позиции:** `kindToIdxSha`, `trainingTripleSha`, `prev2,prev1,goldNext`, `seenInTrainMask`, `seenOnlyInHoldoutMask`, `maskA`, `maskB`, `predA`, `predB`, `usedFallback`, `gold`, per-case correctness. Метрики `top1_accuracy@sameIndices`, `grammar_coverage`, `heldout_only_triplet_rate`, `false_allowed`, `false_blocked`, latency.
- **Фальсификатор:** если `predA==predB` для всех cases, E05 leakage присутствует по source, но **эффект качества на этой панели 0**; не объявлять E05 «снижал качество RP01». Если `AccA>AccB` и отличающиеся решения обусловлены `holdout_only` тройками, архивный PASS имеет unfair information advantage; лечить экспериментальное измерение до любых product изменений.
- **Acceptance:** stdout native test receipt + SHA корпуса + полный paired table + независимая сверка исходников, не статус `dotnet test green` без задач.

**Минимальный будущий кодовый harness (псевдо-C#, API показаны на source):**
```csharp
// Рабочий пример будущего теста; сейчас НЕ запускался.
var vocab = GrammarMaskFilter.BuildVocabulary(trainSources);
var trainTokens = vocab.Value.Tokens;
var trainOnly = new GrammarMaskFilter();
trainOnly.Train(trainTokens, trainSources, vocab.Value.KindToIdx);
var legacy = new GrammarMaskFilter();
legacy.Train(trainTokens, trainSources.Concat(heldOutSources).ToArray(),
             vocab.Value.KindToIdx);
// ВАЖНО: НЕ стройте vocab и нормализатор из heldOut; покажите OOV отдельно.
// Для каждого heldOut AST next-token сравните PredictNext, ValidContinuations
// и token-only baseline НА ТЕХ ЖЕ ПОЗИЦИЯХ.
```

**Параметры, которые нельзя менять между A/B:** code source families, feature flag, predictor weights, tokenizer/rank, kind mapping, `k`, random seed, fallback policy, scoring, stop tokens, compilation settings. Только `allSources`.

## 2. Остальные независимые короткие A/B (не объединять факторы)

| ID / домен | Frozen A | Single-change B | Что измерять | Как отвергать гипотезу |
|---|---|---|---|---|
| `MICRO-N30-E07-ALIGN` C# AST+BPE | `posMap[tokenIndex]`, исходный код | byte→UTF16 char-span→Roslyn AST mapping; тот же model/data | alignment mismatch, top1, fallback, cost | если changed features, но нет changed predictions и downstream gain — только ошибка представления |
| `MICRO-N30-E07-TIER` C# | ключ `(bpe,ast,depth)` для всех `tier=1..5` | по одному `astParent`, `scopeDepth`, `fileId`; каждый отдельная копия train/test | decision parity (старых tiers), unique contexts, top1, source-disjoint transfer | если old tiers меняют predictions при одинаковом input — проверить внешнюю скрытую переменную; если добавление новых dimensions не помогает — не внедрять |
| `MICRO-N30-E20-RETENTION` stack routing | old `presentAfter/(6*N)` + routing after normalization | true conditional `presentBeforeAndAfter/presentBefore`, same input route A(before E15) vs B(after E15) | raw marker survival, per-stack false route, abstain, latency | если actual markers сохранились и routes одинаковы — переносить только исправление метрики |
| `MICRO-N30-E21-TUNING` NL task decomposition | current `TuneThreshold` без участия t, deterministic Decompose | явное forwarding threshold при *реально поддержанном contract* | output sensitivity to t, correct segment boundaries, final execution quality | одинаковые outputs всех t означают no learned threshold, а не failure Decomposer |

**Приоритет:** E05 и E07-ALIGN можно проверить без DPO generation. E20 не менять locked StackDetector order без настоящих input-level causal результатов. E21 нужен `git history` поиск archive-producing SHA прежде чем принимать несовпадение threshold=0.30 vs current expectation 0.20 за баг старого запуска. 8/20/32 — проектные размеры будущего пилота, не проверенные числа успешных задач.

## 3. Независимая проверка настоящей обучаемости, а не «улучшили тесты»

После обнаруженного source seam выбрать 8–32 **новых** законов, не являющихся вариантами одного шаблона. Зафиксировать групповую train/heldout границу и 3 работающих режима:

```text
A0: обычный RP01 Student (B0/DPO/LoRA), без memory consumer
A1: A0 + конкретный исправленный Unit/structural KNN, без DPO retrain
A2: A1 + 8–32 EXACT DPO after legal teacher intake, only if useful
      (and optional warmed base SFT→DPO as separate causal arm)
```

Не сравнивать A0 free-generation с A1 unit-lookup как доказательство улучшения **нейронной** модели: показывать два score `neural_free_correct` и `actual_user_consumer_correct`. Отдельно `precisions@answered`, `coverage`, `hard-law violations`, retention (старые законы), p95 latency, RAM and source disjointness. Включить негативные контроли `not`, роли subject/object, порядок, unknowns, C# AST version/compilation. Если routing Unit отсутствует, `UNIT_NOT_INVOCABLE`, а не `model_failed`.

**Роли без подмены:** `CODEX / GPT-6 Astra 6 UltraCode` выбирает урок и domain law («человеческий» отбор), проверяет законность исходника и independently grades quality/exact final pair; разрешённая **внешняя открытая китайская модель** генерирует **both chosen and rejected**. CODEX/Astra отдельно оценивает blind Student/Unit answer. Ни одна модель **здесь не вызывалась**; surrogate fixtures не training admission. Обе стороны DPO и provenance должны иметь frozen raw bytes/hash, независимый judge не видит train rubric.

## 4. Критерии честного завершения

- **SOURCE_PROVEN:** показан конфликт source/параметров; артефакт нельзя маркировать `learning-improved`.
- **PROXY_PASS:** toy counterexample воспроизводим без .NET — это доказательство вычисления, не живой причинности.
- **NATIVE_MECHANISM_PASS:** native C# подтверждает различие на тех же данных, caller/consumer известен.
- **QUALITY_CAUSAL_GAIN:** real end-user answer on blind split улучшен, domain coverage и regressions явно измерены, и reviewer независим.
- **INCONCLUSIVE/NEGATIVE:** невозможно различить эффект, нет invocation, старые метрики не сопоставимы, наблюдаемая деградация; сохранять без перекраски статуса.

**Gate безопасности:** один writer checkpoint, никогда не изменять архивные Main500/heldout; не плодить GitHub Windows CI и не выдавать компиляцию за domain learning. Source-only tasks не требуют новой полной матрицы Windows. [Подробный E05/E07 анализ](30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md), [E20/E21](30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md), [work order](30-LIVE-CROSSWALK-BACKLOG-AND-CODEX-WORK-ORDER-2026-10-16.md). `llm_mvp_core/docs/plans/replanning-possible-diffs`: **DO_NOT_TOUCH**.
