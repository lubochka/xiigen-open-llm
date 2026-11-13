# RP01/possible-diffs: BPE re-train drift, learned-state identity и границы consumer

**Дата:** 2026-10-16. **Статус:** independent source-focused DRAFT. **Сделано в этом проходе:** просмотрены текущие реализации `KnnLogicalUnit`, `BpeTokenizer`, `TransformerEmbeddingKnnLogicalUnit`, исходный RP01 контракт и опубликованные аудиты 01–20; выполнена лишь внешняя Node.js проверка одной BPE-итерации. **Не сделано:** native .NET эксперимент, модельное обучение, blind оценка, самостоятельный CODEX/Astra review. Влияние на качество — пока H, не M.

Параллельные подробные документы и исполнимый small proxy:
- [N21 — BPE order/growth/OS drift](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md> "Исторический ref llm_plan3; локальный файл может отличаться")
- [N22 — KNN state identity и Transformer confidence](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md> "Исторический ref llm_plan3; локальный файл может отличаться")
- [MICRO-BPE first-merge runnable JS proxy](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/21-MICRO-BPE-FIRST-MERGE-PROXY.mjs> "Исторический ref llm_plan3; локальный файл может отличаться")

Pinned model implementation for comparison: `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`. Branch `codex/dpo-implementation` later reached `0eccbdc3c4efb90adf1d699806cbfa9f5610998d`; inspected diff changes published diagnostic results/tool/docs, not the cited implementation bodies. Current core documentation already includes many earlier analyses: [00](00-README.md), [04](04-hypotheses-and-source-coverage.md), [09](09-domain-functions-and-sibling-branches.md), [10](10-historical-function-transfer-and-learning.md) and [11](11-new-helper-source-reconciliation.md). Не считать их чужие находки новыми открытиями здесь.

## Выделены три НЕэквивалентных вида learning, вместо единой метрики

`1. Student-DPO/LoRA`: [`CodeDocumentationSimulationDpoExperiment.cs`](<C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:103> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L103-L139; локальный файл может отличаться") обновляет генеративную модель; reported 500 пар/50 updates на домен и 0/3 C#, 0/4 EN→RU свободных верных ответов, хотя LoRA изменена.

`2. Structured translation memory`: [`TranslationTrainedUnits.cs`](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:403> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L403-L445; локальный файл может отличаться") имеет trained exact/phrase/word/KNN/correction maps. Доказанное обучение этого Unit не равно изменению нейронного Student и не обязано попасть в его `TextGenerator`.

`3. Generic KNN memory`: [`KnnLogicalUnit.Train`](<C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:102> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L102-L181; локальный файл может отличаться") учит tokenizer BPE320 на входах, строит HNSW vectors, хранит `Output` и [`ExecuteAsync`](<C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:200> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L200-L262; локальный файл может отличаться") выбирает nearest output. Изменение его learned memory невозможно доказать метриками Student DPO.

**Архитектурная цепь доказательства полезности:** `approved_lesson → actual_recipient → state_before/after → query_feature_vector → ranked_hits → selected_learned_output → final_consumer_output → independent_law_grade`. Если `actual_recipient` в RP01 Student — только LoRA, не объявлять недостающий KNN причиной 0/4 без сравнения композитного serving. [ProductHost mismatch подробно описан](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md> "Исторический ref llm_plan3; локальный файл может отличаться").

## NEW-C21.1 — Learned coordinates are corpus-order-dependent, не только HNSW-dependent

**Точная первичная цепочка:**

- [`KnnLogicalUnit.cs:116–153`](<C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:116> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L116-L153; локальный файл может отличаться"): `string.Join(Environment.NewLine, examples.Select(textSelector))` → new `BpeTokenizer` → `Train(...,320)` → new `HnswIndex` → `Vectorize`.
- [`BpeTokenizer.cs:42–68`](<C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:42> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L42-L68; локальный файл может отличаться") и [`526–537`](<C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:526> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L526-L537; локальный файл может отличаться"): пары обходятся в порядке корпусных чанков; при одинаковой frequency нет явно заданного canonical tie-break; token IDs присваиваются начиная с 256.
- [`KnnLogicalUnit.cs:264–299`](<C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:264> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L264-L299; локальный файл может отличаться"): BPE IDs хешируются `tokenId % tokenTierWidth` и нормализуются; порядок токенов отдельно не сохраняется.
- [`BpeTokenizer.cs:112–124`](<C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:112> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L112-L124; локальный файл может отличаться"): уже существует canonical `ComputeStateSha256`, позволяющий проверить фактическое отличие.
- [`LogicalKnnModelManifest.cs type location`](<C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:17> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L17-L29; локальный файл может отличаться"): версия BPE и SHA output/index отсутствуют.

**Минимальный выполненный counterexample — ТОЛЬКО proxy first-merge:** `[abab,cdcd]` и `[cdcd,abab]` при одинаковых парных частотах назначают token 256 соответственно byte pair `97,98` и `99,100`. Результат локального Node.js: `PASS_PROXY_ONLY`. Это математическая демонстрация зависимости раннего `CountPairs+MaxBy` от порядка, а не выполнение нативного C# или проверка извлечения ответов. Нативный `BpeTokenizer.Train(vocabSize:320)` может нивелировать или усилить разницу; смотреть фактический SHA и координаты. Нельзя подменять выводом о BPE ошибку ANN RNG, старый E07 AST mismatch или N20 candidate-set BM25 scaling.

**Новые тестируемые изменения:** 
- `N21-ORDER`: одинаковые train inputs/labels, но другой порядок.
- `N21-GROWTH`: дополнительный unrelated lawful lesson → переобучение BPE → старые vectors/ranks могут измениться.
- `N21-OS`: `Environment.NewLine` даёт `\r\n` на Windows и `\n` на Linux; корпус байтов неодинаков, возможны разные merges. Это подгипотеза, а не измеренная регрессия.

**Опровергнуть:** native tokens/shas идентичны в целевой панели или различаются, но clean paired heldout correctness не меняется; отчёт соответственно `NO_MECHANISM` или `REPRESENTATION_DRIFT_ONLY`.

## NEW-C22.1 — Manifest ∉ frozen identity of learned memory

Код декларирует role/count/ingredients в manifest, но не unique tokenizer hash, training row+label fingerprint и index-output mapping checksum. Поле `TrainingExampleCount` может остаться тем же после перестановки/исправления labels. Это source-verified **недостаточность identity metadata**, не доказанный провал checkpoint. Следствие для аудита: любые «same manifest» и «successfully restored» должны дополнительно показывать learned-state SHA; иначе deployment может принять другой learner под прежним logical ID. Проверка: `MICRO-IDENTITY-01` из планового [N22](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md> "Исторический ref llm_plan3; локальный файл может отличаться").

## NEW-C22.2 — Transformer KNN confidence floor is a scoring transform, not a law verdict

В [`LlmIngredientLogicalUnits.cs:166–210`](<C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:166> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L166-L210; локальный файл может отличаться") confidence = clamp(round(`0.50 + .49*fusedScore`)) и нет warnings. [`ScoreCandidates`](<C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:215> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L215-L244; локальный файл может отличаться") все компоненты fused неотрицательны после clamp, следовательно непустой candidate pool выдаёт confidence≥.50. Это не означает вероятность correct law. Отличие от уже известного generic-KNN confidence/warning [DIF-404](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/04-KNN-HYBRID-RETRIEVAL.md> "Исторический ref llm_plan3; локальный файл может отличаться"): этот метод использует Transformer-fusion и не добавляет предупреждений. Неизвестно, как используется поле конкретным production caller; `P1 conditional`.

## Почему это связано с первоначальным замечанием о «неудачной матрице»

[E07→E09](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md> "Исторический ref llm_plan3; локальный файл может отличаться") и [E40V2 learned TrustScorer→fixed rule](10-historical-function-transfer-and-learning.md) демонстрируют: люди могли назвать «неудачей» плохой encoding/alignment, малую выборку или другой consumer, а не плохую предметную функцию. Точная миграция **одного идентичного матричного алгоритма в ту же KNN-функцию** пока `USER-REPORTED / SOURCE_LINEAGE_OPEN`, поэтому восстановление нельзя заменять механическим копированием старого кода.

## Источник, ограничение, честная приёмка

- `C` = поведение кода на pinned SHA; `P` = внешняя proxy арифметика; `H` = предположительное изменение знания/качества; `M` = native и paired semantic результат (сейчас **отсутствует**).
- Предложения не изменяют код, native state, frozen 500 DPO или historical results. Следующий исполнимый минимальный протокол — [13](13-micro-bpe-retrain-executor-work-order.md).
- Человеческую роль выбора урока и оценки качества DPO выполняет **CODEX с Astra 6 UltraCode**; обе стороны `chosen` / `rejected` новых DPO генерирует разрешённая **внешняя открытая китайская модель**. Автор DPO не является судьёй; model utility отдельно оценивает CODEX. В этой source-сессии такой внешний запуск/вердикт отсутствует.

## Применённые навыки и реальный объём

Адресно использованы `docs/skills/architect-behavior-classifier-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `experiment-design-SKILL.md`, `claim-as-hypothesis-SKILL.md` и `conversation-intent-and-self-reflection-SKILL.md`. Это НЕ их полная сертификация; независимого code-review/30-pass evidence здесь нет. Объём поиска: выборочные первичные методы и указатели RAG; полный корпус всех веток/всех RAG документов не читался.
