# MICRO-16 · Короткая причинная проверка «потерянный оператор → потерянное обученное знание»

**Дата:** 2026-10-16. **Статус:** `READY_TO_IMPLEMENT / FIXTURE_DESIGNED / NOT_RUN / NO_INDEPENDENT_CODEX_REVIEW`. Гипотеза: [N16-02 + N16-03](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md). **Владелец:** Translation Unit owner (не neural RP01 Student). Проверить эффект на применении выученных в Unit правил, **не** объявлять это доказательством neural DPO generalization или исправлением random B0.

## 1. Известное из кода (прямая проверка без обучения)

[`NormalizeMapKey`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L1008-L1024) удаляет operator symbols. [`ExecutorUnit.TrainOnExample`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L425) пишет один `chosen.Text` по нормализованному source key. [`Execute`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L275) читает этот exact map **раньше** learned phrase, wordmap и kNN. При этом `"Check x != y"` и `"Check x == y"` оба нормализуются в `"check x y"`; одновременное обучение этих противоположных законов неизбежно вызывает коллизию key-value. Это source-logic prediction, **не runtime measurement**.

## 2. Одно проверяемое причинное предположение

**H16:** сохранение различимых законных операторов **только в адресе exact learned memory** увеличивает правильность применения двух сохранённых opposite-law lessons при тех же Unit, source text, evaluator, остальных функциях, порядке, памяти и без какой-либо смены LoRA/KNN.

**А — control:** текущая `NormalizeMapKey` для exact map read/write.

**Б — treatment:** сохранять типизированный набор relevant operators (`!=`, `==`, `<`, `>`, `<=`, `>=`) в **versioned exact-memory key** и одинаково использовать его при `TrainOnExample` и `Execute`. Это изменение не должно затрагивать phrase map, KNN signatures, BPE Student, encoder, source text, learning pairs, Verifier, decoder и scorer. Если корректное выделение операторов требует полноценного lexer, scope патча — только признанная syntax-segment identity; нельзя молча дописывать правила, которые сами решают требуемую семантику вместо Unit.

Пример ключей в treatment: `"check x OP_NEQ y"` и `"check x OP_EQ y"`. Это **addresses**, не готовые answers. Версию ключа сохранять в owner checkpoint и сравнить load/readback; не интерпретировать старый checkpoint по новой схеме без миграции.

## 3. Материал (синтетический механизмный fixture, НЕ новые DPO-пары)

Machine-readable: [16-MICRO-OPERATOR-IDENTITY-FIXTURE.json](16-MICRO-OPERATOR-IDENTITY-FIXTURE.json).

**Тренировка одного EN_RU ExecutorUnit:**
- T-NEQ: source `Check x != y`, expected chosen-answer `Проверить, что значения не равны.`
- T-EQ: source `Check x == y`, expected chosen-answer `Проверить, что значения равны.`
- source language `en`, topic `code-expressions`, две самостоятельные law IDs. Остальные Units и словари пусты/frozen. **Эти строки — локальный unit-test stimulus, не финальный Chinese-authored `chosen/rejected` и не учебный корпус.** Не помещать в `training-data/dpo`, не списывать canonical pair IDs, не заявлять CODEX pair judge.

**Контрастные eval inputs на одинаковом замороженном наборе:**
- Q-NEQ: `Check x != y` ⇒ NЕQ.
- Q-EQ: `Check x == y` ⇒ EQ.
- Q-NEQ-SPACES: `CHECK  x  !=  y` ⇒ NEQ.
- Q-EQ-SPACES: `CHECK  x  ==  y` ⇒ EQ.
- NEG-CONTROL-UNSEEN: `Check a != b` ⇒ **не требовать правильности как доказанного переноса**, но записать real route/abstention и проверить отсутствие неправомерного приписывания exact exemplar.

**Две заранее заданные перестановки** порядка уроков `[T-NEQ,T-EQ]` и `[T-EQ,T-NEQ]`; каждая выполняется в отдельном fresh research Unit с теми же state/approved fixture. Внутри каждого порядка сравниваются A и Б с идентичным материалом. Не выдавать эти два diagnostic orders за два независимых семантических эксперимента.

## 4. Исполнение максимально дешёвое

1. Зафиксировать `core SHA`, fixture SHA256, runtime/compiler версии, исходный `ExecutorUnit` и blank dictionaries до обучения. Изолировать в local research/test harness; не запускать старые C#500/ENRU500, Qwen/teacher, optimizer, GitHub Windows CI. На новом SHA перед запуском пересмотреть локальные API.
2. Через существующую [`TranslationLearningBatch / TranslationLearningExample`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L107-L157) создать два `ExecutorUnit` урока с верными typed `ExecutorInput` и `ExecutorOutput`. Для чистого воздействия не добавлять новый reranker, не включать domain-law фильтр и не менять внешние gold labels. Допускается отдельный isolated owner/harness из [`TranslationMachine.CreateRuntime`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L21-L47) и обычного [`TrainedTranslationAdapter`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDirectionAdapters.cs#L93-L139). Если factory API не даёт прямой доступ, минимально instrument exact read/write и реальный adapter, а не копировать логику в Python и называть это .NET результатом.
3. Для каждого порядка обучить A и Б на **тех же двух rows** в отдельных разрешённых synthetic mechanism states. Проверить read/write keys: A ожидаемо один `"check x y"`, Б два адреса. Измерить возвращённый обычным adapter `TranslationRoutingResult.TranslatedText`, `AdapterId`, `Confidence` и `QualityVerifier`. Фиксировать исходные поля и per-case state hash before/after.
4. Выполнить четыре matched Q и NEG-CONTROL, **не пересчитывая их gold после получения ответов**. Порядок запросов одинаков, сравнение по case ID. Протокол содержит оба train orders и paired arm labels, но grader не видит arm до замораживания quality verdicts.
5. Если выигрыш воспроизводится на matched Q, выполнить отдельный *следующий* маленький source-family-disjoint опыт с новыми именами, новой грамматикой и blind rubric. **Он не входит в эту проверку key-collision; без него заявлять обобщение запрещено.**

## 5. Метрики и ожидаемая интерпретация

**Основная:** `correct_application_of_known_lessons / 4` — exact semantic correctness при тех же выученных двух законах. **Вторичные:** `wrong_accepted / 4`, число различных state keys, число changed candidates, train-order sensitivity, normalized input diff, `SourceLang/TopicDomain` trace, latency/CPU/peak memory, save/reload parity. Оценщик CODEX с Astra 6 UltraCode делает независимый semantic check **только если реально приглашён**, не pre-claims. Для этих простых математических операторов допускается также независимый deterministic C#/law oracle; verdict оператора !=/== — внешний предметный факт, но он не должен **подставлять ответ** в inference.

**Предсказание из кода, не результат:** у A 1 exact key и один из противоположных answers замещён другим (вывод будет зависеть от order). У Б 2 exact keys и обе правильные trained-law ответы потенциально достижимы. **Нельзя** утверждать заранее `2/4→4/4` как выполненные метрики; после runtime сохранить сырые outputs и independent grades.

**Принять H16 только если:** реальные .NET read/write trace подтверждают коллизию A и сохранение обоих ключей Б; на одинаковом matched panel `quality_B > quality_A` хотя бы для противоположного закона; противоположный training order меняет ошибочную сторону A, но не смысл Б; unrelated negative control и save/load не деградируют. **Отвергнуть / инконклюзивно**, если иной более ранний route всё исправляет, patch не достигает реального adapter, оценщик использует gold в execution, или результат исчезает при cold reload.

## 6. Пример записи результата / границы доказательства

Должны быть сохранены:
- точные `coreSha/experimentDiffSha/fixtureSha`, authorizations и `runId`;
- `NOT_RUN` → `RUN` только после native execution receipt, per-case input/output/expected с blind labels и обычным consumer trace;
- A/B ключи и last-write effect; pre/post/readback state identities; полный CPU/wall/memory для каждого arm;
- отдельные `known-case application`, `novel-family transfer` (по умолчанию `NOT_TESTED`), `neural Student gain` (`NOT_TESTED`), и отсутствие product adoption;
- independent CODEX/Astra review с candidate SHA, либо явное `REVIEW_PENDING`.

**Запрет фальшивой интерпретации:** успешный тест двух известных сравнений доказывает, что Unit способен вернуть оба ранее записанных урока, но **не** доказывает, что он научился объяснять незнакомые C#/SQL программы, что KNN освоил структурный закон или что neural DPO Student улучшился. Следующая подходящая проверка N16-06 (phrase verifier bypass) должна быть **отдельным** one-factor A/B, а не второй меняемой переменной этого эксперимента.
