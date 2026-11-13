# N26-C — E30: отрицательный matrix-cache, неудачная v2 «коррекция» и невалидный перенос вывода в KNN

**Дата:** 2026-10-16. **Статус:** SOURCE+ARCHIVE_VERIFIED, влияние на RP01 inference **NOT_MEASURED**; `llm_mvp_core@dfd13067ecebdf7b3c3da5000ddbe8604b94ce27`. Новый **узкий** контрпример к «E30 был неудачен → просто перенести word Jaccard в KNN — значит доказано улучшение». Он дополняет, а не отменяет уже известные [matrix→KNN misreads](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [KNN hybrid](04-KNN-HYBRID-RETRIEVAL.md) и [source-group evidence](26-NOVEL-E32-GROUP-LEAKAGE-AND-KNN-TO-CONSTANT-2026-10-16.md).

## Что показывает сохранённый результат (НЕ смешивать метрики)

| Проверка | Baseline | Enhanced | Условие | Сохр. факт |
|---|---:|---:|---|---|
| E30 v1: cache cosine по 22 predicate features поверх bigram | 32.25% | 16.67% | 276 test pairs; 128 cache hits; среди hits верно **2.3%** | `improvement -48.31%`, `passed=false` |
| E30 v2: cache word Jaccard .30 против predicate-cosine .85, **новый ручной набор** | 18.75% | 12.50% | 16 test prompts, cache16, Jaccard hits **1/16** | `improvement -33.33%`, `passed=false` |

**Доказательства:** [v1 archive](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/results/E30-PREDICATE-CACHE/results/run-2026-05-01-001.json), [v2 archive](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/results/E30V2-PREDICATE-CACHE/results/run-2026-05-01-001.json). **Нельзя** напрямую вычитать -48.31% и -33.33%: разные задачи, baseline, метрика (`pred_accuracy` vs `cache_accuracy`), TRAIN/HELDOUT и denominator. В частности v2 **не** доказала rescue исходного v1.

## Найденная причинная механика

1. [PredicateCache.cs L9–51](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Infrastructure/PredicateCache.cs#L9-L51): один 22D normalized predicate frequency vector, cosine threshold .85, ближайший hit переопределяет bigram. Содержательно разные C# операции делят `NULL_GUARD/VALIDATE/SUCCESS` → высокое cosine при разных labels; v1 128 hits, cacheAcc2.3% — конкретная отрицательная функция.
2. [E30PredicateCacheV2.cs L8–72](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Experiments/E30PredicateCacheV2.cs#L8-L72): baseline v2 на natural-language strings, `BuildVector` (через code `LogicExtractor`) может отдавать нули и не воспроизводит распределение code predicate pairs v1; **новый тест не повторяет старое исходное условие**. v2 word Jaccard порог .30 оказался чрезмерно селективен на этих примерах (один hit).
3. [E30PredicateCacheV2.cs L92–141](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Experiments/E30PredicateCacheV2.cs#L92-L141): 8 классов по два примера, но fallback жестко `label=0`, снабжён комментарием «most common label». В этих данных нет наиболее частого класса: все имеют равную представленность. Это **новая data/model prior mismatch**, может маскировать качество слова и cosine.
4. [E30PredicateCache.cs L31–70](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Experiments/E30PredicateCache.cs#L31-L70): v1 baseline — настоящая bigram модель; v2 fallback=0. Сравнение величин между двумя версиями не отвечает на вопрос, улучшает ли новый retrieval живой consumer.

## Новый альтернативный механизм N26-E30-04

**Гипотеза «semantic-collision + fallback collapse»**: не столько «матрица плоха» или «всё решает Jaccard», сколько два независимых эффекта: (a) косинус структурных предикатов не сохраняет доменный intent; (b) word-only Jaccard без вспомогательных structural signals и с single-class fallback сокращает coverage и усиливает предвзятость к label0. В production KNN нужны **не просто все признаки сложенные вместе**, а отдельная проверка доменного закона/contradiction hard-negative, late fusion и прозрачная abstention при низком сходстве. Это проектная гипотеза, не доказанный winner.

**Отрицательные контрпримеры:** «add null check» vs «remove null check», «grant access» vs «revoke access», «return 0» vs «return 1»; проверять, что high overlap не перебивает смысловой оператор. Текущие другие файлы уже разбирают operator identity, поэтому **не переименовывать это в якобы новую находку про `not`**: новизна здесь — точно измеренная **отрицательная v2** и неправильный fallback prior.

## Мини-проверка E30-REPLAY-01 (дёшево, один fixture)

На одних и тех же 16 известных v2 cache entries и 16 heldout prompts, **без Student**:
- A: v2 predicate-vector cosine .85, fallback 0 (архивный baseline).
- B: word Jaccard .30, fallback 0 (архивный v2).
- C: тот же word Jaccard, но `no_hit=ABSTAIN` (coverage-aware, без угадывания label0).
- D: контраст B + отдельно domain-operator veto при одной минимальной выбранной contradictory паре (фиксирован до оценки); не тюнить пороги на HELDOUT.

Сохранять `correct/16`, `covered/16`, `precision@covered`, `false_accept/contradictions`, `label0_predictions`, `fallback_predictions`, trace каждой строки и `model_input_digest`. Проверить сначала, воспроизводится ли сохранённое `A=3/16 B=2/16 B_hits=1/16` на исходном commit. Если нет — `REPRO_FAILED`, нельзя заявлять improvement. Эксперимент C проверяет **природу ложной уверенности**, а не обязательно рост accuracy. Для влияния на RP01 требуется отдельная интеграция с actual ordinary-consumer; без неё STOP: LOCAL_FUNCTION_ONLY.

## Governance

Это **DRAFT_FOR_INDEPENDENT_CODEX_ASTRA6_ULTRACODE_REVIEW**. Source/archive read != новый E30 run. Исторические failures не переименовывать в PASS, другие успешные переносы функций в KNN не отрицать и не приписывать им эту v2. Внешняя открытая китайская модель создаёт DPO; CODEX/Astra 6 UltraCode выполняет человеческий выбор **и** оценку качества DPO, независимо от проверок Student после обучения. Записи ограничены `xiigen-planning/.../possibleDifs`; core `docs/plans/replanning-possible-diffs` только читать.
