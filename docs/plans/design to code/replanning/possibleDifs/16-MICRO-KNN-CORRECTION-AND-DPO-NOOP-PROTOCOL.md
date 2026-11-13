# MICRO-16 — быстрый причинный эксперимент: видимость новых исправлений в KNN и сохранность DPO-пары
**16.10.2026 · READY_TO_IMPLEMENT / NOT_RUN.** Это **научная проверка качества** до переписывания RP01, не Windows/GitHub CI и не доказательство, что LoRA уже обучилась. Источники `llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`, `xiigen-planning/llm_plan3`. Основания: [новая гипотеза KNN](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md), [новая гипотеза DPO](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md), [synthetic fixture](16-MICRO-KNN-CORRECTION-DPO-FIXTURE.json).

## 0. Что отвечает на вопрос «отличие ДЕЙСТВИТЕЛЬНО влияет на качество обучения?»
Нужно последовательно различать:
- **L0 — кодовая причина:** ранее записанное обучающее исправление B не входит в `top-k` / две разные human-source пары после encoder становятся одинаковыми. Проверка может быть мгновенной и без модели.
- **L1 — качество использования обученного:** на **той же заранее заблокированной панели**, после одной и той же последовательности `TrainOnExample`, ответы обычного consumer A/B различаются и одна из версий действительно точнее; отдельно считать `correct`, `wrong`, `abstain`, `coverage`, `law violation`, `latency`. L1 без изменения весов доказал эффект использования выученных пар, а не DPO/LoRA.
- **L2 — перенос навыка:** исправление улучшило ответы на **новых источниках/семействах** после того же обучения, включая fresh restore and ordinary consumer. Если выигрыш только exact-trained input, это полезная память, **не** доказанный generalized learning.
- **RP01 attribution:** требуется четвёртое наблюдение — был ли названный `TranslationKnnStore` или `TranslationDpoTextFeaturizer` вообще вызван в failed RP01 free generation? [RP01 source audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) пока не даёт этого тождества. Без consumer trace причина именно RP01 остаётся `UNPROVED`.

## 1. Основной эксперимент MICRO-16.01: «новая учебная коррекция за пределами k»
**Фальсифицируемая гипотеза:** `TranslationKnnStore.FindNearestCascade(k:5)` скрывает правильную позднюю коррекцию, если пять старых допустимых пар с одинаковым structural signature стоят раньше; перестановка **только порядка выбора кандидатов** при неизменной обученной памяти меняет конечный ответ.

**Где смотреть:** [TranslationUnitContracts.cs:183–310](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L183-L310), [TranslationTrainedUnits.cs:264–365](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L365).

### Шаги реализации одного минимального C# runner
1. Взять **один** pinned source snapshot и создать disposable console / адресный тест `Micro16Probe`, который ссылается на существующий проект `27.TranslationDomain`. Зафиксировать fixture SHA256, конфигурацию и train/eval разделение до исполнения. **Не меняйте продуктовый KNN на этом шаге.**
2. Для каждого `correction_visibility`-случая fixture отдельно создать `TranslationKnnStore`. Вызвать `AddPair(signature, source, prior_target)` ровно пять раз, затем `AddPair(signature, source, latest_target)` **один раз**. Текст source/подпись одинаковые; специально исключить влияние source-law veto. Затем `FindNearestCascade(signature, signature, signature, k:5, querySourceText:source)`. Сохранить `candidate_targets[]` в исходном порядке. Следует получить только пять предыдущих — это **ожидание по source**, не измеренный результат.
3. **A = CURRENT_FIFO:** прежний `FindNearestCascade` + `Take(k)` и первый допустимый target. **B = RECENCY_FIRST:** только экспериментальный read-only selection поверх **того же набора сохранённых пар**, сортировка по обратному insertion ordinal **перед** `Take(k)`, все language/source law guards оставить теми же. Никакого знания `expected_correct` у ранжировщика. Чтобы увидеть counterfactual, можно в отдельной чистой памяти загрузить те же source→target в обратном порядке и сравнить прямой метод; но reverse-import — только smoke, не эквивалент полного продуктового патча.
4. **C = NEGATIVE_CONTROL:** случаи `negative_newest` из fixture, где старые пары правильные, а новая ошибка неверна. Без независимой проверки выбора latest-first обязательно портит качество. Отчёт должен явно показать **both positive and harm cases**. Нельзя объявлять «последний всегда лучше» новым законом алгоритма.
5. **L1 consumer:** собрать те же исправления через `TranslationLearningExample → ExecutorUnit.TrainOnExample`; создать реальные `SemanticTokenSequence` через `TranslationSemanticTokenizer`; запустить `TrainedTranslationAdapter.TranslateAsync` через обычный `TranslationRoutingFlow`. **Важно:** exact/phrase/word map имеет приоритет и может скрыть KNN — к каждому output приложить `AdapterId`, `KNN_invoked`, `signature`, `candidate_ids`. Если запрос попал в exact-map, помечать `KNN_NOT_REACHED`, не засчитывать как результат KNN. Для проверки KNN нужны заранее определённые **source-disjoint paraphrases** и совпадение реально вычисленного `Signature`; при несовпадении signature — `OUT_OF_SCOPE`, не переписывать тесты ради PASS.
6. **L2** только после положительного L1: выполнить одну новую семью того же закона с другими source words и проверить blind correctness без train/exact equality; **save→fresh load** настоящего `TranslationMachine.ExportCheckpoint/ImportCheckpoint` (НЕ metadata-only `ExportAsync/ImportAsync`) и parity с обычным consumer. Допустим `NOT_RUN`, если short probe уже опроверг гипотезу.

### Измерение (предварительно фиксировать)
| Метрика | Определение | Почему |
|---|---|---|
| visibility@5 | доля исправлений `latest_target` в `FindNearestCascade` top5 | L0 непосредственная потеря сигнала |
| correct/6 synthetic | число точных осмысленно правильных output по 6 `correction_visibility` и `negative_newest` fixtures | L1, улучшения минус регрессии |
| false acceptance | сколько заведомо неверных ответов прошло обычный verifier | основной риск |
| correct/heldout & coverage | количество корректных на source-disjoint, число допустимых выходов / все | L2 с воздержанием |
| state/readback | при одинаковых source+training records, изменился ли именно candidate selection; after restore | не путать отсутствие обучения с потерей consumer |

**Предварительный GO:** `visibility@5` изменяется **только** в B, `correct` на положительных закономерностях возрастает, при этом итоговая точность **по всей панели включая негативные** не хуже A и отсутствуют новые false acceptance; L2 улучшение только при дополнительном новом blind наборе, а не по 6 учебным строкам. **STOP:** новые исправления уже доступны в top5; пятёрка отфильтрована реальной law guard; generic consumer ни разу не вызывает store; negative controls выявили вред нового правила; нет blind panel. При STOP переименовать defect в `CODE_SEAM_NO_MEASURED_UTILITY` и не внедрять автоматическую приоритетность.

## 2. Параллельная сверхдешёвая MICRO-16.02: теряется ли `TopicDomain` в памяти?
**Меняется одна переменная:** ключ для exact-memory `source` versus `(topicDomain,source)`. Все training examples, порядок, tokenizer, verifier, входы одинаковые.

1. Cоздать `ExecutorUnit(EN_RU,shared TranslationKnnStore)`, подготовить два `TranslationLearningExample` с `ExecutorInput(SourceText="bank",SourceLang="en",TopicDomain="finance")`, chosen text `"банк"`; и `TopicDomain="geography"`, chosen `"берег"`. Их `UnitId="executor-en_ru"`, `HeldOut=false`; остальные обязательные поля валидные синтетические (см. [TranslationUnitContracts.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L126-L178)).
2. Обучить finance→geography, оценить **обе** `Execute(tokens,"finance"/"geography")`, затем создать **новый** instance и обучить geography→finance. Перед оценкой фиксировать gold. Если в обоих порядках обе темы получают последний перевод, кодовая потеря установлена; это source-predicted, не объявлять измеренным заранее.
3. B: in-memory эксперимент с domain-scoped exact map, запись и выбор по `(TopicDomain,SourceKey)`; пустой unknown topic должен abstain/fallback, а не изобретать перевод. **Не менять** phrase/KNN/LoRA одновременно. `correct/2` на каждом порядке, false accept и coverage.
4. Если L1 позитивен, проверить ordinary `TranslationRoutingFlow`, checkpoint parity; иначе остановиться на доказательстве локального last-write-wins. Доменная неоднозначность должна иметь независимую human-labelled truth: fixture — **диагностика**, не полноценная переводческая benchmark.

## 3. MICRO-16.03: DPO-материал стал «невидимым», до DPO/LoRA
**Не делать новые 500 update и не путать с RP01 `DpoExactPairMaterial` exact-v2!** Исследуем альтернативный `TranslationDpoTextFeaturizer` [L107–368](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L107-L368).

- **PRIORITY FIRST — ACTUAL HANDOFF SOURCE-ELISION:** не ограничиваться вручную собранными `DpoBatch`. Взять существующий producer [`TranslationDpoTrainingHandoffBuilder.BuildTrainerPrompt`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDpoTrainingHandoff.cs#L429-L430): `Translate from en to ru.\\nInput: bank` против `Translate from en to ru.\\nInput: river`, сохранить исходные admission/fingerprints, затем **тем же** `TranslationDpoTextFeaturizer.BuildRecord` построить два финальных batch с default ContextLength64. Предсказание source: первые 30 prefix tokens заканчиваются `input` для обоих, а `': bank'`/`': river'` полностью отсутствуют; `prefix length=32`. Это **сильнее**, чем произвольный длинный prompt: именно так настоящий handoff приклеивает исходник в конец. Вывести `InputText` hash, `TrainerPair.Prompt` hash, `PrefixBeforeCutTokens`, `PrefixAfterCutTokens`, `OmittedSourceTokens` и два BOS→SEP token spans. **Контроль A/B:** A=current64; B=единственное изменение конфигурации `ContextLength` на достаточное (например 128), чтобы источник дошёл до модели. Этот L0 доказывает потерю входа при данной конфигурации; B увеличивает также reply budget, поэтому **НЕ** делает чистого L1 causal claim о source без дополнительного control. Для L1 использовать фиксированную длину контекста и два противопоставленных prefix layouts (legacy metadata-first vs source-first) плюс empty-source negative-control на том же layout; не менять одновременно LoRA, teacher, optimizer или holdout.
- **PREF-CONDITION-LOSS:** два синтетических source prompts с `finance` vs `geography` **в конце строки**. Сравнить реальные BOS→SEP encoded prefix token IDs: если одинаковы — модель не получила отличие, хотя raw human task различен. Кодовое условие default `ContextLength=64`, prefix first 30.
- **PAIR-COLLAPSE-STRIP:** `chosen="берег"`, `rejected="берег [WRONG: wrong topic]"`; сырые стороны различны, после stripping должны совпасть. Сравнить целые `ChosenTokenIds/RejectedTokenIds`, маски и public `PromptFingerprint`. Второй контроль `rejected="банк"` не должен совпасть.
- **PAIR-COLLAPSE-HASH:** `test0` и `test3` имеют одинаковый `ToStableTokenId`=285 при default 512 (Windows/x86 little endian); это collision probe, **не перевод**. Проверить фактический `BuildRecord` на C#.
- **PAIR-COLLAPSE-TAIL:** pair chosen/rejected отличается только **после 31-го response token**; из-за `responseBudget` обе model-visible стороны могут совпасть. Логировать discarded token counts, не секретные content.
- **PROMPT-GRADIENT-DILUTION (N16-DPO-05):** `TranslationDpoTextFeaturizer.BuildRecord` создаёт `DpoBatch` **без `MaterialVersion`, `ChosenResponseMask`, `RejectedResponseMask`**. По [DpoBatch.cs L11–28](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/Models/DpoBatch.cs#L11-L28) default = `LegacyVersion`; [DpoTrainer.cs L1002–1119](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/DpoTrainer.cs#L1002-L1119) full-sequence **mean** включает промпт и заголовки, а не только response. Отдельно замерить `prefix_target_count`, `response_target_count`, weight ratio; paired contrast с legal exact-response mask (реализовать отдельный in-memory variant без retcon frozen artifacts) и frozen base/reference; если изменение target semantics несовместимо с выбранным DPO profile, **не тренировать**, только диагностировать.
- **Затем только на реально сохранённом corpus:** посчитать `raw distinct → model distinct` для всех пар, процент clipped source/answer, одинаковость разных law condition, distribution by family, source IDs. Не менять исходные DPO rows, критерии и существующие оценки.
- L1 после реальных коллизий и *выбранной отдельной модели*: 8–16 независимых корректных пар, A current encoder/B condition-preserving, **одинаковые seed, train steps, base, teacher/reviewer, compute**, paired blind out-of-family translation accuracy; фиксировать invalid/abstain/latency. Это *гипотеза будущего влияния*, не измерение, пока training не запускался.

## 4. Выход в один маленький JSON (не PASS-таблица)
Публиковать новый файл `16-MICRO-RESULT-<runid>.json` (в `possibleDifs` после реального прогона). Минимальные поля:

~~~json
{
  "experiment_id": "MICRO-16",
  "status": "NOT_RUN",
  "core_sha": "83590a462b2f8aedc9d3ab4cb775193eb9c25ce5",
  "fixture_sha256": null,
  "run_command": null,
  "consumer_route": null,
  "baseline_A": {"correct": null, "wrong": null, "abstain": null, "candidate_trace": []},
  "treatment_B": {"correct": null, "wrong": null, "abstain": null, "candidate_trace": []},
  "negative_control_C": {"correct": null, "wrong": null, "abstain": null},
  "dpo_collapsed_pairs": null,
  "heldout_family_disjoint": false,
  "ordinary_consumer_checked": false,
  "fresh_restore_checked": false,
  "causal_claim": "NOT_MEASURED",
  "review": "NOT_PERFORMED"
}
~~~

Не выдавать число компилируемых тестов, `DPO margin`, число добавленных pairs, точность на train exact-source или synthetic counts за реальное пользовательское качество. Достаточно **одного короткого локального build/run**, не `windows-latest`, не C#500, не ENRU500. Если repo не собирается без большого CI — можно начать с isolated `TranslationKnnStore` and featurizer pure-method tests, честно оставить `ordinary_consumer_checked=false`.

## 5. Work order — исполнитель CODEX (архитектура/код/план/промпт/человеческая проверка)
**CURRENT TASK:** только `MICRO-16.01` L0→L1 (соседние .02/.03 по оставшемуся малому бюджету). **NOT THE TASK:** внедрение в RP01 production, переписывание 44 DIF, длинный Windows CI, ретроспективная правка frozen результатов.

**Required skills:** `docs/skills/conversation-intent-and-self-reflection-SKILL.md`, `code-examination-SKILL.md`, `root-cause-tracing-SKILL.md`, `experiment-design-SKILL.md`, `plan-review-SKILL.md`, `plan-self-validation-SKILL.md`, `learning-path-audit-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `output-skepticism-SKILL.md`. Read call sites and consumers before patches, select actual current branch HEAD anew.

**ALLOWED:** read source/fixture; add one small disposable experiment runner and fresh evidence; report statuses and trace in `possibleDifs`; implement a local read-only B selector for comparison. **FORBIDDEN:** alter `train500.jsonl`, certified test panels, optimizer receipts, checkpoints, DPO promotion, shared Skills, generic product KNN behavior without L1 evidence; use test pass to claim semantic improvement.

**GOOD:** «вызван KNN, новая коррекция не попала в top5, при том же blind input B=правильно, A=неверно; на negative C повысился false accept — GO запрещён». **BAD:** «переставили порядок и 6/6 synthetic PASS, значит RP01 обучился / можно merge».

**Return packet:** source/fixture SHA; methods called; A/B/C candidate IDs, full user-answer texts if synthetic/public; counts on same panel including negative; normal consumer adapter IDs; unknown and abstain; source-disjoint and restore fields; measured time/allocations; code/plan review; clear `CODE_CONFIRMED`, `L1_MEASURED`, `L2_MEASURED` or `NOT_RUN`; independent reviewer `YES/NO` (не подделывать). Для нового DPO вне этих диагностик: chosen и rejected создаёт разрешённая модель, CODEX/Astra оценивает *именно сохранённые пары*, human labels/exam отдельно от DPO-pair QA.

**Publication status:** Этот документ — готовый протокол, но **не отчёт исполнения**. Эксперимент и independent scientific examination в текущем сеансе НЕ выполнены.
