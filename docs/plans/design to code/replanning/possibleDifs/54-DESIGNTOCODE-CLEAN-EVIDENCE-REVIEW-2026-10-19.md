# N54 — первичные доказательства, review против самообмана и ограничения выводов

**Исследование:** 19.10.2026, исходное требование — `DesignToCode-clean` как обязательная **первая** золотая ветка, искать новые пробелы и быстрые доменные опыты. **Выполнено сейчас:** live GitHub file reads из двух branches, ручной code/caller comparison, source-equivalent Python toy. **Не выполнено:** независимый второй reviewer, C# compile, real training, visual QA, native cold restore, автономный CODEX. Запрещённый `llm_mvp_core/docs/plans/replanning-possible-diffs/**` **не изменялся**.

## А. Точные anchors источников

**Gold** `llm_mvp_core@DesignToCode-clean=e346b88a3cc1fcce48aefa626bfc7b00509eefd6`:

1. [`44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:L1487–1520`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L1487-L1520) — canonical prompt с полным vector/source, chosen/rejected `html/css/visual_score`.
2. [`44.DesignConversionDomain/DesignConversionMachine.cs:L2755–2820`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L2755-L2820) — gold parser **жёстко возвращает []**, если `visual_score` отсутствует хотя бы с одной стороны; source-kind/vector/pair-family/hash-diversity deltas (но они не умножаются на margin). Это важная поправка к гипотезе: old score был *admission prerequisite и report*, а не показанным signed factor каждого веса.
3. [`44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs:L670–742`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs#L670-L742) — **иной материал** в dataset builder: metadata/hashes/scores вместо HTML/CSS, outcome leakage в prompt. Не считать тот Builder каноническим HTML/CSS generator.
4. [`44.DesignConversionDomain/DesignConversionMachine.cs:L2665–2673`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L2665-L2673) — у gold permissive whitelist feature keys (исключая explicit visual chosen/rejected score).
5. [`44.DesignConversionDomain/DesignConversionMachine.cs:L4335–4351`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L4335-L4351) — gold Critic **inference** `source_manifest:hash_diversity` считал по distinct compare artifact hashes, но **train** на L2789–2815 использовал другую группу source/structure hashes; *несоответствие train/serve семантики* делает прямой перенос старого признака сомнительным.
6. [gold `DesignConversionTrainedUnits.cs`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L298-L353) — полностью тот же blob SHA, что в current; two-defects top1 — **унаследованный** алгоритм, не золотая branch delta.
7. [archive `codex-pass114`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/docs/evidence/design-conversion/codex-pass114-real-learned-weight-proof.md), [`pass116`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/docs/evidence/design-conversion/codex-pass116-runtime-learned-inference-authority.md), [`pass150`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/docs/evidence/design-conversion/codex-pass150-real-training-noop-closure.md), [`pass151`](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/docs/evidence/design-conversion/codex-pass151-improver-non-copy-closure.md) — локальные implementation/test receipts, но не независимая proof общего semantic heldout quality после cold restore.

**Current** `llm_mvp_core@codex/dpo-implementation=358af351e22a92a3f7938d1f9221d5aeb7ddb053`:

1. [`TrainingPipeline.cs:L2712–2727`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L2712-L2727) — complete chosen/rejected output projection **только `html/css`**.
2. [`DpoDatasetBuilder.cs:L1231–1260`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs#L1231-L1260) — source-safe prompt, internal vector prefix16; [L1442–1447](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs#L1442-L1447) — output html/css.
3. [`Machine.cs:L2937–3005`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2937-L3005) — **gold no-score→[] превратился в current no-score→`1.0`** при неравных strings; одновременно `AddDelta` строится из `internal_vector` и `source_kind`, а не знака margin.
4. [`Machine.cs:L3008–3091`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3008-L3091) — categories появляются только если есть в chosen/rejected JSON; текущий canonical HTML/CSS таких ключей не имеет. **Current source-only learner при swap chosen↔rejected формирует одинаковые наборы feature deltas** для одинакового prompt, кроме ситуации появления новых категорий в другом materialization path; это вывод из функций, не runtime countercheck.
5. [`Machine.cs:L2844–2855`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2844-L2855) — *строгая* feature whitelist: `internal_vector:mean/dimension`, `source_kind:figma/canva/other`, пять `correction_category:*`; `pair_family`, `source_manifest:hash_diversity` из gold уже отсутствуют. Это **доказанное сокращение пространства локальных признаков**, но не автоматически ухудшение (gold train/serve diversity не была эквивалентна).
6. [`Machine.cs:L349–407`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L349-L407) — local delta действительно применяются; `RewardMargin` выносится **в отчёт**; [`CarryForwardAccuracyWithoutHeldOutMeasurement:L3333–3339`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3333-L3339) переносит `AccuracyAfter = AccuracyBefore`. **Нельзя утверждать**, что fallback margin непосредственно меняет `AccuracyAfter` или gradient. Гораздо ближе к causal loss — missing category sidecars и source-only signed-indifference.
7. [`Machine.cs:L1501–1525`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L1501-L1525), [L4232–4254](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4232-L4254), [L4388–4401](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4388-L4401) — producer/consumer существующих `correction_category:*` и learned rule ranking **реально реализованы**.
8. [`DesignConversionTrainedUnits.cs:L298–353`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L298-L353) — top-1 cliff и multi-defect hidden deterministic fallback.
9. [`DpoDatasetBuilder.cs:L553–586`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs#L553-L586), [L1308–1380](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs#L1308-L1380) — sanitization forbidden keys и invalid JSON `{}`.
10. [`Machine.cs:L183–217`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L183-L217), [`Pipeline.cs:L300–392`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L300-L392) — legacy API explicitly blocked, authorised paths remain.

## Б. Особенно опасный ложный вывод, который этот audit запрещает

> Плохо: «gold записывал визуальный score, current больше не записывает, поэтому current DPO/LoRA перестало учиться».

**Это слишком сильная причинная версия**. Корректно:
- `ToCanonicalTrainingItem` score потерян, и **старый local Machine без score отказывал в deltas**, а **нынешний local Machine заменил оценку константой и допускает source-only updates**. Это **контрактное расхождение и потенциальное обучение без реальной предпочтительной семантики**.
- Конкретные local learned weights из source_kind/vector могут обновляться одинаково при swapped chosen/rejected, потому что `preferenceMargin` не умножает на contributions. Это **не исправляется одной только метрикой reward margin**.
- Нейронный DPO Student обучается иному objective в другом recipient: наличие HTML/CSS как chosen/rejected может быть именно **правильным исправлением** прошлой меточной утечки.
- Чтобы назвать пробел причиной провала RP01 C#/ENRU, нужен мост конкретного route к `CodeDocumentationSimulationDpoExperiment`, которого в этом сравнении не найдено. Сейчас `RP01_QUALITY_CAUSE=UNPROVEN`.

## В. Одна честная source-equivalent проверка выполнена

Python 3 script [`54-DESIGNTOCODE-CLEAN-PROXY.py`](54-DESIGNTOCODE-CLEAN-PROXY.py) покрывает **ограниченную** логику на искусственных данных; его [результат](54-DESIGNTOCODE-CLEAN-PROXY-RESULT.json):

| Условие | Наблюдаемое toy-значение | Что означает |
|---|---|---|
| Gold, визуальные scores 0.9 vs 0.1 / reversed | margin `+0.8 / -0.8` | В gold score-поля различают направление в **отчёте** |
| Current, только два различных JSON HTML/CSS / reversed scores outside JSON | margin `+1.0 / +1.0` | Нынешняя fallback ветка игнорирует sign реального score, если он не попал в JSON |
| Current, same prompt + swapped `chosen↔rejected`, ответы только HTML/CSS без категорий | local feature-deltas одинаковы: `internal_vector:mean=0.1; dimension=16; source_kind:figma=1` | Локальный обучаемый checkpoint не различает направление предпочтения **в этой source-only ветке**; не экстраполировать на LoRA |
| Two defects, weights none / contrast only / contrast+overflow / nonpositive | selected count `2 / 1 / 1 / 2` | Top-1 cliff; пока не измеренная визуальная полезность |
| Two parentRef values | cleaned structure equal | Возможная потеря полезного реляционного признака; privacy guard сохранять |
| 18-d vectors differ only at dimension17 | first 16 equal | Условная коллизия. Не проверено, попадают ли реальные данных/входы в неё |

Это не компиляция C#, поэтому расхождение Python regex tokenizer на иных UTF8 ключах возможно; native C# check обязателен перед code change. Модель, teacher, LoRA, kNN, browser и обычный API в этой пробе не запускались.

## Г. Уже известные доменные восстановления и антидублирование

Унаследованные from other agents `possibleDifs`:
- [master conflict](00-MASTER-SYNTHESIS-AND-CONFLICTS.md): `random B0` vs learned memory, DPO labels, KNN, LoRA, EOS/tokenization, checkpoint.
- [language_translate](00-LANGUAGE-TRANSLATE-RECOVERY-FIRST-2026-10-19.md): POS order E25, contextual dictionary E26 (преднаполненный, не online learner), structural KNN E09 (несовпадающие denom!), replay E43, переводческие trained Units.
- [external_domains](00-EXTERNAL-DOMAINS-POSITIVE-RESTORE-P0-2026-10-19.md): реальный positive historical receipts по отдельным доменам.
- [local_wiki_planning](00-LOCAL-WIKI-SUCCESS-FIRST-2026-10-19.md): CodeTraceBuilder learned triplet потерян при `RejectedType` projection, QueryPlanning positive archive и дисбаланс уникальности.
- [orchestrator extensions](00-ORCHESTRATOR-EXTENSIONS-GOLD-RECOVERY-FIRST-2026-10-19.md): 15-pattern checkpoint schema-v1 → current schema-v2 admission; consumer could empty fallback.
- [matrix to KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [LoRA/vector/tokenization](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [restore](06-CHECKPOINT-RESTORE-ADOPTION.md).
- Protected, **only read** `llm_mvp_core/docs/plans/replanning-possible-diffs/{00,02,03,47,80,81}*`. В частности сам защищённый документ [80-research-resume-verified-and-pending](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/80-research-resume-verified-and-pending.md) предупреждает, что нейронная LoRA изменилась, но свободное качество не показано.

**Новизна с оговоркой:** N54-00 gold/current *specific Machine acceptance* и N54-01 *learned single-rule cliff* выведены прямым сравнением и source read. Но без автоматического полного сравнения всех 443 файлов каталога говорить «ни один из других агентов это нигде не заметил» **нельзя**. Не присваивать заявлению `globally_unique_verified`.

## Д. Skill-based review

- **Architecture / requirements:** `architect-behavior-classifier-SKILL.md` + `phase-planning-SKILL.md` — отделить user goal, done vs not done, источник и получателя.
- **Code examination:** `code-examination-SKILL.md` — проследить caller → `ToCanonicalTrainingItem` → `BuildLearnedWeightDeltas` → checkpoint → `CreateRuntimeRoutingFlow`, а не остановиться на файле builder.
- **Experiment design:** `experiment-design-SKILL.md` — один контраст, baseline/enhanced, Tier1→Tier2, запрет `0/3` переименовать в успешность.
- **Plan review / claim verification:** `plan-review-SKILL.md` + `claim-verification-SKILL.md` — публикация source-backed **DRAFT**, без claims `quality restored`; отделять золотую ветку от exact RP01, no source leak.
- **Prompt engineering:** `prompt-patch-authoring-SKILL.md` — один bottleneck на patch, wrong/correct literal, regression check.
- **Human communication:** `human-communication-GLOSSARY.md` — домен как *предмет*; HTML/CSS, C# и EN→RU — конкретные профили/языки; Unit — получатель; DPO/LoRA/KNN — механизмы.

**Self-review verdict:** `SOURCE_REVIEW_DRAFT; TOY_PROXY_EXECUTED; INDEPENDENT_NATIVE_REVIEW=NOT_RUN; QUALITY_RESTORED=NOT_PROVEN`. Следующий продуктовый эксперимент — **GDC-A0**, следом **GDC-A1/A2**, с независимым оценщиком на обычном consumer и одинаковым удержанным набором. См. [задания](54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md).
