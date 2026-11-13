# MICRO-N21 — одно короткое причинное исследование: фильтр похожести скрывает полезное обучение

**Версия:** 2026-10-16 / READY_TO_IMPLEMENT, **НЕ ЗАПУЩЕНО**. Независимый CODEX/Astra-review ещё не проводился.  
**ExperimentId:** `MICRO-N21-CORRECTIVE-ADMISSION-01`. Источник: `llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`; актуальный blob для изученного builder/runner на HEAD 16.10 не изменился. Гипотезы: [N21-05 + N21-01…04](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md). Исходный [synthetic fixture](21-MICRO-CAUSAL-LAW-DELTA-FIXTURE.json).

## Кратко для человека

**Вопрос:** может ли система потерять полезное новое знание ещё до обучения — потому что в исправленном примере поменялось одно существенное слово, а Jaccard считает его «почти повтором»? Мы не предполагаем, что LoRA плоха; проверяем отдельный реальный механизм, который в прежних экспериментах участвовал в памяти Translation Units.

Текущий filter ставит `similarity >= 0.70` → **не передавать коррекцию на обучение**, хотя слово «Марии» заменено на «Ольге». Для первого маленького causal опыта сравниваем не количество зелёных тестов, а **новые законно правильные ответы пользователя**.

Эксперимент намеренно узкий: **1 закон recipient, 2 train-смысла, 8 новых source строк, одна зафиксированная схема Unit, 0 optimizer steps, 0 Windows GitHub Actions**. Время/стоимость фиксируются *при реальном запуске*, заранее не выдаются за измеренные.

## L0 — дешёвый входной screen без запуска Student

**Source links:** [FilterByTrainingHistoryUniqueness L201–250](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L201-L250), [вызывающий Batch029Runner L92–105](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationBatch029Runner.cs#L92-L105).

**Факты code path:** history состоит из исходных текстов; токенизация — нижний регистр, split whitespace, удалить всё кроме букв/цифр; Jaccard tokens union/intersection; correction исключается, если overlap ≥ 0.70. Закон/тип ошибки в filter не участвуют.

**Прозрачная proxy-проверка автора документа, НЕ выполнение .NET:** JavaScript с эквивалентным для этих строк набором lowercase Unicode letters/digits вычислил:

| Контрпример | Intersect / Union | Jaccard | Предсказание поведения текущего фильтра |
|---|---|---|---|
| «… Марии …» → «… Ольге …» | 5/7 | 0.7142857 | SKIP, хотя другой recipient |
| `send invoice to Alice today` → `do not send invoice to Alice today` | 5/7 | 0.7142857 | SKIP, хотя появилось отрицание; **math-only**, этот англ. пример не является фактическим RU_EN batch |
| Та же строка, но другое написание регистра и пунктуация | 6/6 | 1.0 | SKIP, и это ожидаемый полезный dedup |

**Граница вывода:** L0 демонстрирует математику алгоритма на synthetic input. Это **не native C# test**, не факт, что данные уже были в RP01, и не доказательство улучшения качества модели. Executor обязан подтвердить вычисления через настоящий метод C# на тех же значениях до любых кодовых изменений.

## L1 — один matched experiment на actual обучаемом Unit и actual consumer

**Вход:** fixture [21-MICRO-CAUSAL-LAW-DELTA-FIXTURE.json](21-MICRO-CAUSAL-LAW-DELTA-FIXTURE.json), только DEV_ONLY. Действующий `TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch`, `TranslationMachine.TrainStructuredBatchAsync`, `TranslationRoutingFlow` / `ExecutorUnit.Execute`. Никаких изменений в `DpoTrainer`, RP01 Student, DPO pair journals, LoRA, HNSW и production consumer. Выбрать ровно один существующий native test/tool host, не создавать тяжёлый test framework.

**До запуска:** CODEX/Astra в роли выбора и отдельной оценки законности DEV-пары либо независимый предметный судья подтверждает, что исправление действительно меняет recipient и что chosen/negative различаются *по смыслу*. Это не финальная DPO пара; для будущего production final DPO обе стороны обязана генерировать разрешённая внешняя open Chinese model. Проверить отсутствие PII, реальность разрешений и не маркировать synthetic как подтверждённое внешнее teacher происхождение.

**Протокол:** заморозить один baseline `machine checkpoint`, версии вектора/Tokenizer, закон, порядок корпусов и runtime route. Для всех режимов использовать одинаковые 8 heldout с source, которого нет в train/`exactSourceMap`. Не передавать heldout gold ни в Unit, ни в query metadata, ни в lexical reranker.

| Arm | Обучение на одном и том же старом corpus | Отличие и интерпретация |
|---|---|---|
| **A FILTERED** | Old lawful lessons; новый correct recipient урок прошёл действующий Jaccard filter и был отвергнут | Исходная система |
| **B CORRECT-DELTA** | Та же история + ровно одна законно подтверждённая recipient correction; временное narrow admission exception только для доказанного law delta | Что меняется, если нужный урок всё-таки виден learner |
| **C BAD-DELTA** | То же число/те же source как B, но correction output подменён заведомо неверным target | Отрицательный контроль: улучшение обязано зависеть от **правильного** урока, а не числа строк |
| **D NO-NEW-TRAINING** | Frozen baseline Unit без добавления урока, те же запросы | Защита от видимого эффекта детерминированных правил/seed |

**Изоляция:** A→B включает законный новый учебный сигнал и меняет count (это измеряемый эффект admission). **Основной content contrast B↔C** фиксирует count, training inputs, режим сигнатур и Unit; меняются только target labels. В отличие от ANN, здесь для основной пробы использовать работающий `TranslationKnnStore` с детерминированным порядком и теми же corpus source, потому что задача — проверить конкретный filter. Если будет выбран HNSW, следует фиксировать hit-IDs/rank и отделить ANN-стохастику от правильности labels.

**Недопустимые «победы»:** пример попал только в `_exactSourceMap`, а пользовательский input совпал с train; evaluator читает целевой recipient из `Metadata`; chosen не прошёл независимый law-review; пользователю вернулся frozen deterministic result без invocation trained Unit; различия возникли только из другого checkpoint/decoder; «RowsTrained=1» при нулевом state delta.

### Измерение восьми новых запросов

Для H01–H08, заранее утверждённых предметным судьёй, сохранять:

- `train_input_sha`, `heldout_input_sha`, `model_checkpoint_sha`, `unit_state_sha` / patch version; `unit_id`, `training_record_id`, `admission_decision`, `admission_reason`;
- `source_text`, `actual_user_output`, `selected_answer_origin` (`exact-source` / `phrase` / `wordmap` / `knn` / `fallback`), `learned_unit_invoked`; evidence/hit signature и ключи, когда существуют;
- `required_law_satisfied` и `forbidden_law_violated`, error kind, abstain/coverage, elapsed ms, peak process memory; сохранять **все восемь строк**, не только PASS count.

**Primary metric:** число `lawful_correct_final_answers` из восьми (корректный recipient, не выдуман другой, соблюдены остальные требования) — B vs C, отдельно B vs A. Индивидуальная существенная ошибка важнее среднего. Вторичные — `correct_when_answered`, forbidden false positive, wrong-language, abstain, доля улучшений именно на source-disjoint вариантах, число реально обученных law-delta строк, runtime.

**Предварительный сигнал:** если B улучшает ≥2/8 против C, не ухудшает critical law и `learned_unit_invoked=true`, это `EXPLORATORY_POSITIVE`; 8 примеров **не обеспечивают статистически устойчивый вывод**. Если B и C одинаковы: `NO_LEARNED_CONTENT_GAIN_ON_PANEL`, не объявлять весь подход ложным. Если Unit меняется, но ordinary Flow не использует его результат: `CONSUMER_ADOPTION_GAP`. Если эффект виден лишь на exact train source: `MEMORIZATION_ONLY`. Если B улучшает относительно A, но не C, это `UNCONTROLLED_ADMISSION_GAIN`, а не доказанный законный урок. Отдельно при нулевой разнице просмотреть фактические `AdapterId`/signature/cascade, а не запускать автоматически ещё 500 пар.

### Самый короткий маршрут исполнения (один bounded executor run)

1. Снять remote SHA, проверить `current blob SHA`, existence актуального `AGENTS.md` и read-only relevant skill, путь действующего native runner. Считать fixture и убедиться в отсутствии пересечений train/heldout normalised SHA.
2. В native `L0` вызвать ровно `FilterByTrainingHistoryUniqueness` для трёх случаев; проверить SKIP recipient/negation/punctuation согласно таблице, не пересказывать proxy как .NET.
3. После независимого DEV-label review материализовать A/B/C/D каждый от одинакового clean baseline, а не «обучать дальше» между arms. Тот же `signatureMode`, те же Unit types; каждую созданную structured запись привязать к `UnitId`.
4. Для H01–H08 выполнить ordinary translation через истинный `TranslationRoutingFlow`; дополнительно direct `ExecutorUnit` вызов как diagnostical split, обязательно сохранить `AdapterId`, exact-match avoidance, конечный output и verifier effect.
5. Отдать `MICRO-N21-RESULTS.json` с полями `status`, `actual_command`, `source_sha`, `approval_receipt`, `input_hashes`, `per_case[]`, `counts`, `causal_limitations`; только после реального запуска. Внести нужную one-factor кодовую правку лишь если причинность поддержана; не подгонять rubric/test labels.

## L2 — два дополнительных дешёвых screen без обучения модели

**S21.1 / SemanticTokenizer 2×2:** взять проверенную человеком source-family, где rule-only `sequence.Pattern` действительно ошибается. Разделить два изменения: F1 independent corrected `TokenizerOutput.Pattern` (current self-label vs oracle), F2 canonical lookup-key (current `|PAIR:id`/tags vs same-address structural key). Arms 00/10/01/11. После 2–4 train выполнить 4–8 новых same-family/different-source inputs, требовать `pattern_changed_in_intended_unit` и `lawful_user_output_changed`. Если `BuildGeneralBatch` не может корректно получить independent pattern label, отдельный test-only adapter, не менять production silently. Измерять влияние F1 и F2 отдельно и их взаимодействие; иначе можно ошибочно «исправить ключ», продолжая учить прежнюю ошибку.

**S21.2 / QualityVerifier context:** один synthetic `(THREAD_REF,LAUNCH_OP)→required`, затем 4 contexts с обоими concepts и 4 без LAUNCH_OP; A=current Verify, B=token-aware gated-by-both-context evaluator, frozen same rule set. Проверить source token concepts фактически tokenizer-ом; если не получается сформировать пару, `DATA/NOT_APPLICABLE`, не фабриковать «успех». Добавить пару с `required="not"` и `candidate="knot"` для проверки ложного substring-pass отдельно. Подтверждать фактическое `TrainOnExample` заполнение правил, не самодельную вставку приватного dictionary. Основная метрика — correct verifier legal verdict / false accept / false reject.

**S21.3 / collapsed chosen-rejected:** 3 допущенные DEV строки: `[WRONG: gender_agreement]`, omission и literal rejected sentence; сериализовать BuildGeneralBatch Executor/Verifier output и посчитать `TextEqual`. Это **diagnostic only**: пока Unit тренируется только от chosen, изменение rejected не станет чудом в user answer. Если этот negative идёт в DPO bridge, отдельно доказать exact recipient/material route перед малым optimizer causal contrast; не объединять с S21.1/S21.2.

## Проверка плана, архитектуры и качества (skill-aligned)

**Архитектор:** читает source→`TranslationStructuredBatchBuilder`→`TrainStructuredBatchAsync`→`ExecutorUnit`→`TranslationRoutingFlow`→served answer, обозначает все missing arrows и не утверждает, что neural RP01 Student теперь использует trained memory.

**Code reviewer:** проверяет ключи `pair_id`/`signature`, mutation site, prior used checkpoint, отсутствие скрытого retrieval shortcut и branch pin; не правит production во время получения научного baseline.

**Planning reviewer:** одна независимая переменная на контраст, 8/8 evidence, A/B/C/D, отсутствие training source в heldout, no gold in request, заранее frozen metric, stop rule. Нельзя менять acceptance после результата.

**Prompt engineer:** фиксирует задачу модели, данные/provenance/author boundaries, отвергает self-judged final quality и synthetic teacher impersonation; при QA маркирует ambiguous/incorrect gold как `DATA`, а не подгоняет тест.

**Human communication:** объясняет просто: «в выбранном кодовом маршруте новый важный урок может быть исключён как похожий; настоящий результат качества пока не получен». Только после run: числа actual scores + обычные пользовательские примеры + ограничения, без «всё обучается» и «найдена причина RP01».

## Связь с большой программой

**Положительный результат** означает только, что этот обучаемый **translation memory / verifier recipient** получил новый законный навык на выбранном наборе при контролируемом admission. Он не означает изменение LoRA и не отменяет `C# 0/3→0/3`, `ENRU 0/4→0/4`. После локального causal proof — проверить production caller/adoption и 20+ family-disjoint heldout across domains, затем исправлять рабочий план RP02/переход к RP01 лишь при доказанной общей цепи.

**Ограничение доступа этой сессии:** здесь не было .NET runtime и не запускались native C# test, Student training, external Chinese teacher, CODEX/Astra judge, cold-process consumer. Даны source proof, собственная помеченная proxy математика и готовый native эксперимент; результат `READY_TO_IMPLEMENT`, не `PASSED`.
