# 21 — Независимый аудит: урок принят, но исправление не обучает или применяется вне закона

**Дата:** 2026-10-16. **Статус:** DRAFT / SOURCE-CONFIRMED MECHANISMS / CAUSAL USER QUALITY NOT RUN / INDEPENDENT ASTRA REVIEW NOT PERFORMED.

**Точный источник:** `lubochka/llm_mvp_core`, ветка `codex/dpo-implementation`, snapshot `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`. Проверка 16.10: файлы `TranslationStructuredBatchBuilder.cs`, `TranslationTrainedUnits.cs` и `TranslationBatch029Runner.cs` имеют тот же blob SHA на текущем HEAD `0eccbdc3c4efb90adf1d699806cbfa9f5610998d`. Это **не** доказательство неизменности всех соседних файлов и не выполненный native запуск.

**Рамки:** здесь обнаружены дополнительные конкретные seams, отсутствовавшие в изученных картах DIF-01…44, DIF-14 и новых N16/N17/N18; это НЕ объявление их уже доказанной причиной отрицательного результата отдельно стоящего RP01 Student. RP01 neural StepVector→LoRA→TextGenerator и TranslationMachine structured training — различные учебные и serving пути.

## Что означает для человека

Система может получить корректную DPO-пару и записать «Unit обучен», но в обычном запросе не получить исправления: (1) исходный урок подменён выводом старого детерминистического алгоритма; (2) результат сохранён под другим ключом; (3) новый отличающийся по смыслу урок отфильтрован как похожий; (4) проверка применяет когда-то выученный закон в неверном контексте. Файлы/счётчики/«PASS» не доказывают improvement.

### N21-01 — P0 для структурного обучения: SemanticTokenizer получает СВОЮ старую метку, а correction-address не совпадает с runtime

**Прямое исходное доказательство C:**
- [TranslationStructuredBatchBuilder.BuildGeneralBatch L65–115](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L65-L115): строит `sequence = tokenizer.Tokenize(pair.ASide, sourceLanguage)`, затем выбранная метка для семантического Unit — `new TokenizerOutput(sequence.Pattern, signature)`. Она НЕ является исправленным человеком/teacher `chosen` и не содержит независимого semantic gold; её `Pattern` равен первоначальному выводу rules.
- В режиме `ExactPair` `signature` получает `|PAIR:<pair.Id>`, затем могут добавиться rule tags (L95–99). Это provenance-идентификатор учебной записи, а не канонический ключ нового пользовательского текста.
- [SemanticTokenizerUnit.TrainOnExample / Tokenize L88–140](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L88-L140): `_patternCorrections[example.SemanticSignature]=chosen.Pattern`, но `Tokenize` ищет `_patternCorrections.TryGetValue(seq.Signature,...)` и применяет только если `corrected != seq.Pattern`. Для *другого* input точная `_signatureCorrections[Normalize(rawText)]` тоже не срабатывает.
- [TranslationBatch029Runner L126–150](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationBatch029Runner.cs#L126-L150) реально выбирает `Structural` для новых filtered и supplemental пар, но `ExactPair` для historical corpus. Значит, нельзя утверждать, что **весь** batch работает в `ExactPair`. Для `Structural` остаётся фундаментальная самопроверка `chosen.Pattern == bootstrap.Pattern`, а rule tags могут изменять сохранённый адрес.

**Предсказание:** новый source со сходной структурой не получает исправленной классификации от этого учебного семантического примера. Точная старая фраза может получить подмену signature через exact-map и тем самым повлиять на downstream KNN — это НЕ нулевая работа во всех случаях. Параметризацией `pair.Id` нельзя лечить сам факт отсутствия независимого gold.

**Короткая проверка:** для 2–4 законно исправленных на основании слепого oracle примеров снять (a) исходный `Pattern`, (b) chosen Pattern из builder, (c) адрес сохранения, (d) адрес, который реально ищет Unit на source-disjoint парафразе, (e) end-user verified result. Контраст `current` vs `oracle-corrected label` и `canonical structural key` в 2×2 матрице; не менять всё сразу. Отдельно негативный контроль с заведомо ложной меткой.

**Статус:** C — self-label и несовпадение ключа в ExactPair; R — реальный вызов обоих режимов из Batch029; H — влияние на unseen answer. Historical `DesignToCode` и `orchestrator-extensions` уже содержали ту же форму конструктора; это **наследуемая возможность недообучения**, НЕ доказанная новая регрессия при RP01 merge.

### N21-02 — P1: контекстный закон сохранён с двумя концептами, но применяется по одному

[QualityVerifierUnit.TrainOnExample L1094–1119](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L1094-L1119) записывает контекстный закон по ключу `_conceptRules[(src,ctx)] = required`. Но [Verify L1071–1081](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L1071-L1081) проверяет наличие `src` и отсутствие слова в output — **не проверяет, что `ctx` присутствует в source**. Следовательно, при одинаковом `src` правило из другого контекста может вызвать ложный FAIL, хотя к текущему input не относится. Комментарий в этом файле прямо приводит пару `THREAD_REF` при `LAUNCH_OP`; [TranslationSemanticTokenizer L15–27](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationSemanticTokenizer.cs#L15-L27) знает оба значения.

**Нюанс:** это подтверждённый source conditional: он проявится, только когда `_conceptRules` заполнены через `TryParseIssue`; типовые generated quality-marker issues могут заполнять другие словари, а не `_conceptRules`. Не утверждать, что контекстная ошибка произошла в данном RP01500.

**Проба:** обучить только один валидный `(src,ctx)→required`; для 8 слепых input проверить 4 с `src+ctx` и 4 c `src` без `ctx`; A=current, B=read-only evaluator that also requires ctx. Основная метрика — false rejection of lawful answers при неизменных required-law violations. Нельзя подбирать gold по коду evaluator.

### N21-03 — P1: условно ложное принятие проверкой из-за substring вместо токена

В том же [QualityVerifierUnit.Verify L1051–1089](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L1051-L1089) требования проверяются `candidateText.Contains(required, OrdinalIgnoreCase)` для per-signature, source-marker и concept rules. Если required является отдельным доменным словом, наличие его как *подстроки другого слова* ещё не значит, что закон соблюдён. Минимальный контрпример: требование токена `not` и текст с отдельным словом `knot` (строка `knot` содержит подстроку `not`); условия настоящего domain-law oracle должны определить, когда применимо сравнение слова целиком, леммы, выражения или допускается подстрока.

**Проба:** один frozen набор из 8 valid/invalid кандидатов, A=current `Contains` vs B=language-aware whole-token/approved inflection check, без изменения learned rules; измерить **false accept** и **false reject**, особенно отрицание и идентификаторы. Если реальные требования подстрочные (например, часть идентификатора), закон допускает substring явно, а не глобально меняется поведение.

### N21-04 — P1 conditional: конструктор negative стороны может создать «ошибку» с тем же текстом, что chosen

[BuildRejectedCandidate L269–326](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L269-L326): если rejection начинается с `[WRONG: ...]`, `ExtractExplicitRejectedCandidate` возвращает пустое значение; если `rejectedType` содержит `gender` / `agreement` / `case` / `tense` и не относится к omission, `BuildRejectedCandidate` возвращает просто `chosen`. [BuildGeneralBatch L117–140](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L117-L140) тогда формирует `ExecutorOutput.Text` у обеих сторон одинаковым (различаются confidence/word map) и `VerifierInput.CandidateText` равным хорошему переводу, но `VerifierOutput.Pass=false`.

**Точная область:** условие реально для допущенной записи с маркером типа `[WRONG: gender_agreement - ...]`. В четырёх проверенных опубликованных `TrainingData` JSONL (50 строк) встречался только `phrase_boundary_lost` / `product_phrase_boundary_lost`: эти категории попадают в другую omission-ветку. Поэтому **не утверждаем**, что конкретные 50 строк уже имеют эту коллизию. Она не эквивалентна ранее найденному hash-ID collision в `TranslationDpoTextFeaturizer`: здесь исходные **семантические строки** двух альтернатив становятся одинаковыми ещё до токенизации.

**Дополнительное ограничение:** `TranslationMachine.TrainStructuredBatchAsync` отправляет в Unit объект целиком, но [ExecutorUnit.TrainOnExample L403–445](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L445) учит chosen-карты; нельзя обещать рост через простую замену rejected text в этом Unit. Нужно доказать consumer границу, где rejected действительно используется как предпочтение, иначе A/B останется no-effect.

**Проба:** три synthetic pairs (`gender_agreement`, `omission`, explicit literal wrong candidate) через **настоящий** BuildGeneralBatch; до training проверка `chosen.Text != rejected.Text` и согласованность `VerifierInput.CandidateText`, затем independent oracle на неверность rejected. Только если конкретный recipient обрабатывает отрицательный сигнал, запускать локальный A/B с одним исправленным контрастом и 8 hidden heldouts.

### N21-05 — P0 conditional для continual learning: novelty-filter отбрасывает важное исключение

[FilterByTrainingHistoryUniqueness L190–250](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L190-L250): порог `Jaccard(source token sets) >= 0.70` исключает candidate **до** анализа закона/типа ошибки. [TranslationBatch029Runner L92–105](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationBatch029Runner.cs#L92-L105) действительно применяет его к новым batch examples с existing corpus training history.

**Конструктивный источник:** history `пожалуйста отправьте отчёт Марии сегодня утром`; candidate `пожалуйста отправьте отчёт Ольге сегодня утром`. Их lowercase token sets различаются в одном именованном получателе, пересечение 5, объединение 7 → 5/7=0.7142857, значит candidate отбрасывается при пороге 0.70, хотя правило о recipient изменилось. Это **аргумент из точного алгоритма**, не доказанный фактический пропуск именно такого sample в продовой истории.

**Значение:** система может сохранять старый перевод и отсеять единственный важный corrective lesson, потому что его исходный текст похож. Для instruction-like задач эта потеря особенно существенна.

**Контраст:** A=сохранить фактический filter; B=изменить ТОЛЬКО admission по независимому operator/recipient/referent/negation difference (не blanket disable). Freeze corpus, Unit code, seed, labels, train order и отдельный sealed heldout. Тестировать 8–12 **новых** формулировок, не training exact; negative controls: punctuation/case-only variant, deliberately wrong repair, no-training. Основная метрика — законно правильный ответ на heldout при реально вызванном обученном Unit, отдельно число admitted lawful deltas, false-positive, abstain, kNN hit identity. В релиз не переносить без независимой semantic verification и проверки на междоменные коллизии.

## Отношение к другим агентам, историческим веткам и RP01

| Исходный материал | Уже исследованное | Что конкретно добавлено здесь |
|---|---|---|
| `01-PRIORITIZED-DIFFERENCES.md` DIF-01…44 | LoRA/DPO, rank, generic kNN, pair quality, checkpoints, consumer | Семантическая самоподстановка label + несовпадающий key при создании typed Unit; law-blind novelty filter с caller proof |
| `10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md` | Exact-source vs generalization, chosen-only learning, restore cache | Точные поля `sequence.Pattern` / `|PAIR:...`, сравнение с `TryGetValue(seq.Signature)`; контекстный закон сохранён как пара, проверен как один концепт |
| `14-KNN-IDENTITY...` и N16 коррекции | HNSW IDs, domain scope, FIFO, language/TopicDomain loss | Не идентификатор HNSW, а *semantically missing correction label*, *filter admission*, *verifier (src,ctx) predicate* |
| `16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md` | raw DPO text → same hashed tokens / truncated IDs | alternative semantic text уже совпал в BuildRejectedCandidate, **до** нейросетевого представления |
| `18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md` | Неподключённая trained memory в ordinary Generate | Проблемы данных/ключей/выученного закона *внутри* structured Unit; adoption нужно доказывать независимо |

Исторический `DesignToCode@8f20ceb8...` и `orchestrator-extensions@b293045...` содержит constructor того же вида — НЕ называть новые N21 дефектами, внесёнными именно RP01. `language_translate` и старые E51A/B могут иметь иной обучающий механизм (морфология/структура и данные); положительные записи из их архива не являются causal result для `StepVector`.

**Проверки существующих данных:** в 4 доступных файлах `27.TranslationDomain/TrainingData` (16+16+8+10 строк) все встреченные `[WRONG]` типы были `product_phrase_boundary_lost` и `phrase_boundary_lost`; показатель является выборочной проверкой, НЕ аудитом полного corpus и не подтверждением N21-04 на исторических запусках.

## Как строго разделять доказательства

- `C/SOURCE` — буквальный путь кода; эти пять механизмов имеют конкретные методы.
- `R/ROUTE` — указан caller `TranslationBatch029Runner` для ExactPair и novelty filter; другие callers требуют отдельной трассы.
- `M/MEASURED` — только опубликованные frozen historical results; в этом документе НЕТ нового измерения Student.
- `H/CAUSAL` — влияние механизма на законный ответ (гипотеза). Отдельный matched A/B с независимым gold обязателен.
- `X/RUN` — NOT RUN native C#, NOT RUN model, NOT independently judged. Не записывать `PASS` из бумажной реконструкции.

## Что сделать в первую очередь

1. Один короткий `L0` в действующем builder/Unit проверяет N21-01, N21-02, N21-04 и filter. Без 500 DPO, без LoRA и без Windows Actions.
2. Для N21-05 выполнить ОДИН источниково-дизъюнктный mini causal A/B на обучаемом Unit (см. соседний `21-MICRO-CAUSAL-LEARNING-QUALITY-PILOT.md`), с output на user-like endpoint.
3. Если loss обнаружен только в structured Unit, классифицировать как RP02/Translation learning gap, не переписывать отрицательные `RP01 0/3`/`0/4`. Если actual neural student path использует этот material — тогда отдельно трассировать мост с exact SHA.

**Роли DPO (из октябрьского RP01 контракта):** CODEX/Astra выбирает законный урок и отдельно независимо оценивает exact pair; разрешённая внешняя open Chinese teacher модель генерирует ОБЕ финальные стороны chosen/rejected; Student и отдельный evaluator не подменяют teacher/judge. Результаты новых моделей не придуманы. Ни один synthetic pair ниже не маркировать как внешнюю финальную DPO-пару.

**Требуемое независимое ревью:** архитектор — recipient/caller topology; code reviewer — подтверждение source/branch/путей; planning reviewer — изоляция причинной переменной; prompt engineer — source-disjoint и отсутствие oracle в runtime input; human communication — исходный ответ, честный статус, один следующий запуск. Пока все роли покрыты документарным self-review, внешнего CODEX/Astra review не было.
