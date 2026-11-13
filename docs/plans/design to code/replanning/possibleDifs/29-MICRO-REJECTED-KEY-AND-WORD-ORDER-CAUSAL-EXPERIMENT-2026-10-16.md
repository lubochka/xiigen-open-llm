# N29-MICRO — два дешёвых, НЕ смешиваемых эксперимента о потере правильных уроков в переводческих Units

**Дата:** 16.10.2026. **Состояние:** **EXPERIMENT DESIGN / NATIVE .NET NOT_RUN / USER-QUALITY AB NOT_RUN / INDEPENDENT ASSESSOR NOT_RUN / NO PRODUCT PATCH**. Основание: [N29-A учительская метка в KNN](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) и [N29-B первое слово не равно переводу первого слова](29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md). Исследование **не** является заявкой на полный RP01 neural retrain и не меняет `llm_mvp_core/docs/plans/replanning-possible-diffs`.

## Назначение и приёмка

**Цель:** за минимальный объём native локальной работы определить, превращаются ли реальные, корректно представленные lessons в неверно индексированную/семантически искаженную память, которая ухудшает **обычный пользовательский перевод**. Результат `KNN.Count+1`, `UnitAccuracy>0`, `test passed` — не доказательство улучшения. Все тестируемые объекты и checkpoint живут только в disposable shadow workspace; исходный Student, frozen RP01 corpus, DPO pair IDs, reference и production weights не меняются.

**Проверка заранее:** при фиксированных модельных/Unit версиях, языке, temperature/seed если задействован генератор, judge rubric и memory budget выполнить три уровня:
- **L0 Feature/State:** native `BuildGeneralBatch` → `TrainStructuredBatchAsync` → `ExportCheckpoint`, прочитать фактический полученный `SemanticSignature` и пары `WordMap`/KNN buckets.
- **L1 Consumer:** полная выдача `TranslationRoutingFlow` через `TrainedTranslationAdapter`, **после загрузки сохранённого checkpoint в новый `TranslationMachine`**, логируя adapter ID, exact/phrase/word/KNN/fallback, candidate, verifier pass, accepted vs returned answer.
- **L2 Quality:** неизменная **слепая** панель новых source families; отдельный CODEX/Astra 6 UltraCode в роли evaluator только при фактическом разрешённом вызове; для offline предварительного дизайна допустим явно помеченный независимый human gold без ложной отметки CODEX APPROVED.

Пороги качества и критерии должны быть зафиксированы в локальном `intent.json` **до** первого наблюдения результатов. Не изменять gold после просмотра ответов.

## Эксперимент A: нельзя ли запомнить chosen по метке rejected, которой нет при inference?

| Элемент | Контракт |
| --- | --- |
| `ExperimentId` | `N29-KEY-MICRO-01` |
| `Baseline CURRENT` | Существующий `BuildGeneralBatch`: `signature += BuildRuleTags(pair.Rejected)`; обычный KNN/Unit state |
| `Treatment SOURCE-KEY` | Единственное отличие в disposable shadow: аннотация `[RULE:...]` сохраняется **в metadata/audit**, но ключ `SemanticSignature` для training/retrieval определяется только признаками, которые способен создать runtime из source; chosen/rejected **байты НЕ менять** |
| `Training` | 2–4 разрешённые обучающие строки RU→EN одного domain, часть с реально присутствующей `[RULE: ...]`. Для L0 разрешена искусственная isolated fixture; она **не** считается новой production DPO парой |
| `Heldout` | 8–12 новых предложений RU→EN, распределённых по 2–3 meaning families; exact-source/family overlap с train исключён. Сделать также 2 exact-source diagnostic controls — **не включать их в primary metric** |
| `Invariant` | Те же source/chosen/rejected, ID/signatureMode, train order, Unit/Flow graph, KNN k, thresholds, checkpoint code и судья; не менять LoRA, BPE, EOS, Flow priority |
| `Primary metric` | `blind_semantic_correct / heldout_N`, сначала показать `reached_retrieval_consumer / heldout_N` |
| `Mechanism measures` | `train_signature_has_rejected_rule`, `unknown_source_runtime_has_rule`, `full/prefix/structure_hit`, `retrieved_right_law`, `fallback`, false accept/false reject, cold-restore parity |
| `Falsifier` | Нет ни одного изменённого full bucket или все новые примеры получают тот же правильный кандидат благодаря prefix/structure → **quality effect не доказан**, verdict `KEY_SHAPE_ONLY/NO_GAIN` |

### Короткая последовательность A

1. В native test runner (не GitHub Windows action) взять действительный `TranslationTrainReadyPair` с `[RULE:...]` в `Rejected` и проверить `BuildGeneralBatch` в `TranslationSignatureMode.Structural`. Записать `hash+length` raw пары, `trainingKey` и `sourceOnlyKey` до mutation. На одной искусственной тестовой паре показать, что изменение только marker меняет `trainingKey` — это **source mechanism**, не ML win.
2. Создать A/CURRENT и B/SOURCE-KEY из одного immutable input snapshot; записать 2–4 structured lessons; `ExportCheckpoint → save → fresh machine load`; получить обычный ответ на каждой из 8–12 unseen задач. Сохранить один trace на case: `case_id/source_family/direction/expected_law/train_key/runtime_key/bucket_path/adapter_id/output/verifier/judge`.
3. Отдельно оценить lawful meaning, где есть syntactic roles, negation и required terms. При изменившемся full-key, но равных хороших ответах — **не рекомендовать механическое удаление всех RULE tags**: прежде доказать, какие метки допустимо вычислять из input и какие нужны отдельному learnable rule classifier.

**Важно:** для чисто нейросетевого RP01 direct `DpoExactPairMaterial → DpoTrainer` эта гипотеза `NOT_REACHABLE`, пока не доказан вызов `TranslationStructuredBatchBuilder`.

## Эксперимент B: впервые проверить причинный эффект first-word pseudoalignment

| Элемент | Контракт |
| --- | --- |
| `ExperimentId` | `N29-ALIGN-MICRO-02` |
| `Baseline CURRENT` | Полный `BuildWordMappings`, включая `firstSourceToken→firstTargetWord` |
| `Treatment NO-POSITIONAL` | Отключить **только** непроверенное first-token fallback mapping; оставить explicit mappings, `InferTargetWord`, marker, KNN, order и checkpoint |
| `Training` | Две-три fresh тестовые RU→EN пары с инверсией порядка слов и *adverb source-marker control*: `Книгу долго читал я` / `Книгу долго читал он`, chosen `I read the book for a long time` / `He read the book for a long time`; слово `долго` — распознаваемый ADV, поэтому `ExtractSourceMarker` должен выбрать его, **не** `Книгу`; сначала проверить настоящие POS/WordMappings. Повторить с **обратным порядком двух уроков** в двух isolated machines |
| `Heldout` | 8–12 новых (не те же exact sources) с различными субъектами, объектами, утвердительными/отрицательными вариантами, плюс неинвертированные предложения как negative controls |
| `Primary metric` | `semantic_role_and_polarity_correct / heldout_N`, отдельно `false_word_alignment_count` по реально экспортированному `ExecutorUnitCheckpoint.WordMap` |
| `Mechanism` | Зафиксировать origin каждого mapping: `explicit`, `concept`, `marker`, `first_positional`; source POS/role; `before/after` map и collision of identical key from two lawful pairs |
| `Falsifier` | В проверяемых примерах positional fallback никогда не создаёт неправильного mapping (его занял explicit/concept); или consumer не достигает wordmap; или semantic quality не меняется. Тогда **не объявлять его причиной RP01** |

### Короткая последовательность B

1. Сначала *без обучения* вызвать настоящий `BuildGeneralBatch` с 2 synthetic test-only парами, распечатать только подготовленные `WordMappings` и происхождение каждого mapping. **Проверить, что `долго` классифицируется как ADV и уходит в source-marker mapping, а `книгу→I/He` создаётся именно веткой first-positional**, а не более ранней `ExtractSourceMarker`/`requiredTerm`. На обычных строках `Книгу я прочитал` marker может выбрать саму `Книгу` и тогда выключение positional ветки не является честным A/B; их для primary L0 не использовать. Если раздельный origin не подтверждён — `AB_INVALID/INCONCLUSIVE`, не заявлять мнимый win.
2. Для каждой исходной версии сравнить **A затем B** и **B затем A**; сохранить словарь до/после; повторить после cold restore. Если базовая версия меняет перевод словарного ключа только при перестановке двух правильных уроков, это сильное подтверждение *order-dependent contamination*, но **пока не** доказательство реального ухудшения ответов.
3. По тем же 8–12 sealed source-disjoint запросам вызвать live Flow, сравнить смысл, неизменённый kNN/phrase/exact, пути и стоимость. Запрещено изменять одновременно POS heuristic и prior KNN rank/LoRA или делать словарь детерминистическим правильным ответом только ради прохождения теста.

## Что сдаёт исполнитель вместо десятков CI запусков

**Ровно один маленький локальный runner / два отдельных режима** `N29-KEY-MICRO-01`, `N29-ALIGN-MICRO-02`; без Windows GitHub Actions, без полного corpus replay, без 500 DPO pairs. Достаточные выходы:

```text
results/N29-MICRO-20261016/
  intent.json                # precommitted design: data families, one factor, thresholds, repo SHAs
  inputs.sha256.json         # source/teacher/license state hashes; no private texts in reports
  mechanism.ndjson           # line-level feature/Unit/key evidence before consumer
  checkpoints.json           # cold restore & trained state hash comparisons
  answers.ndjson             # normal Flow output, path and blind evaluator
  comparison.json            # paired A/B + order reversal, no hidden denominator
  verdict.md                 # IMPROVE / REGRESS / NO_GAIN / INCONCLUSIVE / NOT_RUN
```

**Пример `mechanism.ndjson` (схема, НЕ наблюдавшийся результат):**
```json
{"case_id":"unseen-01","source_family":"book-fronted-sentence","training_key":"<actual runtime key>","query_key":"<actual runtime key>","recipient":"executor-ru_en","path":"knn|wordmap|fallback","checkpoint_sha256":"<real hash>","did_reach_consumer":null,"assessor_status":"NOT_RUN"}
```

**Критерий успеха**: у treatment есть независимое улучшение правильных ответов **на новых source families**, неизменённое либо улучшенное сохранение старых корректных случаев, отсутствие роста critical errors; state/route доказаны native trace и cold restore. На малой панели 8–12 задач даже +2 корректных ответа — **пилотный сигнал, не статистическое доказательство общей надёжности**. При одинаковом качестве фиксировать `NO_GAIN` или `INCONCLUSIVE`, не подгонять проверки под результат. При ошибочной authoring/provenance — `INVALID_EVIDENCE`; контроль запуска переоформить, не применять нелегитимные пары.

## Роли и границы публикации

Architect проверяет, **что** обучается и какие consumer читает состояние; executor читает вызовы (code-examination); planning отделяет A и B, plan-review не допускает смешанных факторов; prompt-engineering защищает границу teacher annotations/gold; human communication подаёт вывод простым языком. CODEX c запрошенной моделью Astra 6 UltraCode — human-like choice и **отдельная** оценка готовой пары только при фактическом вызове. Две финальные chosen/rejected для **production DPO** создаёт разрешённая внешняя открытая китайская модель, исходное происхождение сохраняется. Synthetic native fixture здесь служит только проверке механизма; не может быть выдана за legally authored production dataset.

**Состояние при публикации документа: только план эксперимента, `NOT_RUN`; никаких новых измерений качества или native C# success не утверждается.**
