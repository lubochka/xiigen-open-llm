# GO-01…GO-04 — четыре быстрых причинных проверки для восстановления обучения `orchestrator-extensions`

**Приоритет:** читать вместе с [золотой ГЛАВОЙ 0](00-ORCHESTRATOR-EXTENSIONS-GOLDEN-RESTORE-P0-2026-10-19.md) и [N52 — новые различия](52-ORCHESTRATOR-NEW-GAPS-AND-EVIDENCE-2026-10-19.md). **Дата:** 19.10.2026. **Режим этого документа:** план нативных экспериментов + выполненный только source-equivalent Python proxy GO-02; никакой тренировки C# / LoRA / продукта из этого пакета не запускалось. **Запрещено менять** `llm_mvp_core/docs/plans/replanning-possible-diffs`, фиксировать baseline в текущем коде, ослаблять legal admission, выдавать toy proxy за neural-result.

**Важное уточнение очереди:** прежде GO-01…GO-04 нужно запустить независимый [GOLD-OR-A0 для 15 сохранённых OrchestrationReasoning паттернов](52-ORCHESTRATOR-GOLD-DOMAIN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md), потому что [v1 checkpoint текущим v2 loader отвергается](52-ORCHESTRATION-REASONING-CHECKPOINT-AND-ADMISSION-GAP-2026-10-19.md). GO-01…GO-04 — следующие *разделяющие механизмы* проверки, а не замена восстановления существующего обученного Orchestrator. Нет native quality A/B ни для A0, ни для GO.


## Общие инварианты: как увидеть обучение, а не тестовую подгонку

**Каждый эксперимент** публикует `ExperimentId`, точно идентифицированный donor SHA/current SHA, тему обучения (домен), задачу/язык/направление, имена recipients Unit/Flow/Student, вид механизма (DPO, LoRA, kNN, rules, replay), source/provenance разрешение, hashes исходных пар, chosen/rejected, train/heldout ID, `checkpoint` hash и runtime owner. Отдельно фиксируются: consumed sample count, delta learned state, unseen quality по слепому арбитру, стоимость на batch, peak RAM/VRAM, after-cold-restart quality, old-skill retention. **Запрещено сравнивать разные eval panels или метрики с тем же именем, но другой формулой.**

**Обязательные контрольные плечи:** baseline до обучения; enhanced после одного изменения; control с перемешанной меткой/отключённым learned state; cold restored после открытия **новым** процессом; обычный consumer, а не direct unit test. Нельзя повторять тренировочные примеры в blind heldout, генерировать золотые ответы тем же правилом, что оценивается, выбирать порог по exam. Все границы law/security остаются действовать. Малое число случаев — **индикатор механизма**, не статистическое доказательство общей точности. Результат `PASS` по unit trace отличается от `CAUSAL QUALITY GAIN`.

## GOLD-P0-1 / GO-01 — точный DPO материал, получатель и нейронное качество golden → RP01

| Обязательное поле | Значение |
|---|---|
| ExperimentId / Tier | `GO-01-GOLD-DPO-RECIPIENT` / Tier 1 (несколько маленьких файлов/пар) |
| Темы обучения | Project Wiki, документация C#; отдельно перевод EN→RU только на разрешённых примерах |
| Donor | [`orchestrator-extensions` исторический 150-pair run](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/experiments-v2/INTERNAL-TRAIN-READY-V191/results/base-wiki-session-01-batch-004-cumulative150-20260515-1025/internal-train-ready-dpo-summary.json). Замечание: положительна **minibatch preference**, не свободная генерация |
| Current recipient | RP01 Student/LoRA + отдельный old-style parameter-space контроль; **не** объявлять их одним и тем же обучаемым объектом |
| Baseline | Идентичная свежая исходная модель до любого update с точно тем же tokenizer/context/decoder и заранее записанными ответами; reference frozen |
| Enhanced | Один DPO step на 8–16 разрешённых chosen/rejected пар с неизменными identity и seed; затем 2–3 шага с recording gradients, но никаких скрытых fallback rules |
| Измерение | (а) отличимость chosen/rejected **на входе модели**; (б) положительный directional reward margin на train vs heldout; (в) **domain judge**: correctness/rubric on unseen generated answers; (г) `||Δweights||` по блокам LoRA/base; (д) same after cold restore |
| Фальсификация | если exact material collapse (одинаковые token ids chosen/rejected), отсутствие actual recipient update или Δweights=0 → остановить эксперименты на этом слое; если margin растёт, а unseen quality нет → **не** объявлять «DPO сломан», проверять base capacity/answer boundary/consumer |

**Процедура в порядке исполнения.** 1) Из [старого StepMany #L75–L125](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/11.DPO/DpoTrainer.cs#L75-L125) извлечь фактические trainable groups и objective; из [current #L701–L764](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L701-L764) их counterpart. 2) Мини-пары выбрать до настройки модели, разнообразные по предметным подзадачам и длине; на каждый получить точные model-visible ids, response masks, `logp(chosen)` и `logp(rejected)`. 3) Убедиться, что **когда одинаковое начальное состояние доступно**, randomized control и treatment действительно начинаются с одной и той же SHA. 4) Сравнить (A) B0 до, (B) B0+DPO, (C) B0+specialized Unit memory, (D) B0+DPO+Unit memory — *четыре плеча на одном текущем base*; исторический donor результат показать **рядом отдельной строкой**, если совместимая модель/инициализация не восстановлена. 5) На 4–8 совершенно новых задачах (например, C# функции с иным именованием, Wiki запрос с изменённым порядком слов) выполнить обычный consumer до/после, judge blinded. 6) Cold restore и повторить.

**Минимальная интерпретация:** B улучшился — neuro recipient contributes; C улучшился — памяти/Unit достаточно, D > C — нейронный дополнительный эффект; all 0 при росте train margin — objective/consumer mismatch, а не причина автоматически в kNN. Результаты C и D должны быть отдельно по каждому домену, не «общий accuracy».

## GOLD-P0-2 / GO-03 — перенести рабочую Unit/kNN функцию в реальный доменный потребитель

| Поле | Значение |
|---|---|
| ExperimentId / Tier | `GO-03-GOLD-UNIT-KNN-CONSUMER` / Tier 1 |
| Темы | **Перевод** — направление RU→EN и EN→RU, POS, word order, отрицания; **программирование** — C# AST/structure; не называть направления отдельными доменами |
| Подтверждённая пара файлов | Веточный `27.TranslationDomain/TranslationTrainedUnits.cs` [old](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/27.TranslationDomain/TranslationTrainedUnits.cs) и [new](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/27.TranslationDomain/TranslationTrainedUnits.cs) функционально совпадают после удаления лицензионного заголовка |
| Baseline | Тот же Unit/checkpoint до урока. У normal consumer неизменные ответы на 6–12 разрешённых heldout |
| Enhanced | Один реально вызванный `TrainOnExample` на 2–4 новых chosen cases, сохранение index/maps, затем ordinary Flow/consumer с теми же входами |
| Domain feature arms | language: exact-only, POS/left-right-context, order-aware sequence, typed phrase map; C#: plain lexical, stable AST-label structural kNN; **без global включения всех features** |
| Метрики | exact vs unseen paraphrase correct; сохранение отрицания/рода/word order; wrong acceptance; per-Unit trained count, kNN candidate topk and score, cold-restored answer parity, old-case retention |
| Фальсификация | old/new source identical, но same learned state & consumer still different → искать ingest/route/serialization; kNN topk изменился, а consumer answer нет → consumer bypass, не «плохая LoRA» |

**Контрастные задачи:** перевод: «Не сохраняй файл» против «Сохрани файл», «быстрый перевод» с разным контекстом, SVO vs SOV; C#: две функции с одинаковыми идентификаторами, но разным AST-порядком вызовов `Validate→Save` vs `Save→Validate`; отдельные новые файлы в exam. Train и exam разделить по семейству/файлу, а не по строкам одного документа. Уровень exact-match должен уступить при смене имён, а learned structural memory всё ещё должна быть применима на соответствующем домене. **Проверять learned kNN + scope + safety**, не чистый детерминированный lexer в качестве «обучившегося». Релевантные ранее восстановленные позитивные рецепты: [language_translate E25/E26/E09/E43](00-LANGUAGE-TRANSLATE-RECOVERY-FIRST-2026-10-19.md), [external_domains](00-EXTERNAL-DOMAINS-SUCCESS-RESTORATION-P0-2026-10-19.md).

## GOLD-P0-3 / GO-04 — Design-to-Code: observational-counter cannot replace learned weight & output proof

| Поле | Значение |
|---|---|
| ExperimentId / Tier | `GO-04-DTC-WEIGHTS-AND-COLD-CONSUMER` / Tier 1 |
| Тема/получатель | Design-to-Code HTML/CSS correction; trained Critic vs trained Improver и конечный candidate scorer |
| Baseline | Saved checkpoint до continuation; JSON `FeatureKey -> Weight, ObservationCount`, heldout blind scores, реальный HTML/CSS output |
| Enhanced | Тот же checkpoint, один **адmitted** continuation batch; без изменения evaluator; весь pipeline с локальным trained consumer |
| Контроли | X без learned records; Y только ObservationCount+1 при неизменном Weight (source-equivalent predicate test); Z действительная изменённая весовая карта после разрешённого update; cold-process load |
| Метрики | `ΔWeight` на каждую Unit, `ΔObservationCount` отдельно, drift in selected HTML/CSS, quality judged visually/semantic by distinct teacher, cold parity, no unauthorized promotion |
| Заявляемый эффект | Только когда новая unseen quality выросла при changed actual consumer, с сохранением старого качества. `RealTrainedWeightsContinued` сам по себе не proof |
| Falsification | если в full consumed path доказано, что нельзя увеличить `ObservationCount` без изменения хотя бы одного `Weight`, N52-02 becomes theoretical only; если никто не использует flag как promotion criterion — false-positive claim не распространяется на продукт |

**Изучить код сначала:** [continue predicate](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionLocalContinueTrainingProbe.cs#L248-L294), [weight counter](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionMachine.cs#L2817-L2843). Отдельная проверка старого `VerifyFreshLoadedCheckpointAsync` должна установить реальные callers; current pipeline admitted-aware — не терять безопасность.

## GOLD-P0-4 / GO-02 — «10 первых примеров» и слепая зона по другим Unit

| Поле | Значение |
|---|---|
| ExperimentId / Tier | `GO-02-DTC-PROBE-ORDER-AND-COVERAGE` / Tier 1 |
| Тема | Design-to-Code; 3 **получателя**, не 3 домена |
| Baseline | 24 одинаковых допущенных lesson-objects, порядок 10 A, 7 B, 7 C; внутренний cap=10 |
| Enhanced | Тот же полный набор объектов, `interleave(A,B,C)`; ни training pairs, ни weights не менять |
| Источник механизма | [`BuildPreparedLocalContinueTrainingProbeBatch` #L2588–L2633](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L2588-L2633) |
| Метрики | `distinct probe UnitId coverage` (1/3 vs 3/3), per-Unit included count, order invariance of report; actual learned quality measured independently |
| Negative controls | Повторить same order twice; shuffled only within same Unit; all A 24 and one Unit should not trigger false uncovered warning |
| Источник уже выполненного опыта | [SOURCE-EQUIVALENT Python](52-ORCHESTRATOR-PROBE-ORDER-PROXY-2026-10-19.py) и [frozen JSON](52-ORCHESTRATOR-PROBE-ORDER-PROXY-RESULT-2026-10-19.json) — **только selection math**, не C# |
| Native outcome, который требуется | Легальный C# caller через настоящую `PrepareExecutionWithDataset` и admitted API. Получить RecordIds выбранных 10; дополнительно blind per-Unit on all 24 или независимый untouched heldout |

**Важный source-counterexample:** [`ProjectFinalOptimizerInputs` #L1810–L1843](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L1810-L1843) получает **все** `TrainingItems`. Из GO-02 нельзя выводить, что в основном DPO обучались лишь первые десять или что найден источник 0/4 перевода. Если hypothesis confirmed, *первое исправление — формировать отдельный стратифицированный probe/report*, не трогать optimizer. Этот эксперимент отдельно проверяет качество контроля, не качество модели.

## Единый output contract исполнителя для каждого GO

```text
EXPERIMENT_ID: GO-0X-...
SOURCE_PROOF: old-ref:path#lines ; current-ref:path#lines
TOPIC (DOMAIN): translation | programming | documentation | design-to-code
TASK/DIRECTION: concrete independent heldout profile
RECIPIENTS: base_student / lora_adapters / exact Unit / kNN Unit / Flow / consumer
WHAT_CHANGED: one named factor
WHAT_STAYED_FIXED: inputs, weights at t0, seed, decoder, train and exam IDs
MECHANISM_REACHABLE: actual caller->trained owner->ordinary consumer, yes/no
TRAIN_STATE_DELTA: per owner actual learned tensor/map/index count/hash and meaning
RESULT_BEFORE/AFTER: correct count and false accepts on EXACT SAME UNSEEN panel
OLD_RETENTION: old correct count on same panel
COLD_RESTORE: new PID + checkpoint hash + before/after answer identities
NEGATIVE_CONTROL: shuffled label / disabled memory / unchanged weight
GOLDEN_COMPARE_BOUNDARY: same model and rubric? if no, descriptive only
TIME+RAM+VRAM: exact measured if run; else NOT_MEASURED
STATUS: SOURCE_CONFIRMED | ANALYTIC_PROXY | NATIVE_RUN | CAUSAL_GAIN | NO_GAIN
VERDICT_AND_NEXT_SINGLE_CHANGE: ...
```

**Исполнительский промпт (без скрытой реализации):** «Прочти skill `human-communication-GLOSSARY.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `plan-review-SKILL.md` из заданного `llm_mvp_core/docs/skills`; прочти полностью методы и их caller/consumer; выбери **один** GO и pinned SHA; не модифицируй read-only `docs/plans/replanning-possible-diffs`; не меняй модели вне разрешения; сформируй Before/After/Negative/Cold cases; не смешивай исторические source receipts с новой моделью. После запуска приложи реальный stdout, learning corpus hash, code SHA, answers and grades, не synthetic summary». Наличие этого промпта **не означает состоявшийся запуск**.

## Чего ожидать в результате, и чего точно НЕ ожидать

GO-01 различит optimizer/material mismatch и невоспроизводимое golden student preference; GO-03 проверит, действительно ли старые обученные специализированные механизмы работают в обычном ответе; GO-04 отделит весовую/metadata метрику от действительной DTC компетенции; GO-02 обнаружит слепую зону coverage. **Ни один GO поодиночке не доказывает перенос всех доменов.** После позитивного микрорезультата масштабировать по семьям задач и направлениям; при отрицательном вернуться к конкретной причине, а не расширять сразу размер модели или число GitHub Windows-run.
