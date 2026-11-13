# ГЛАВА 0 — language_translate: восстановить реально подтверждённые способности, а не оптимизировать 500 DPO

**Дата:** 19.10.2026. **Статус:** SOURCE_READ / HISTORICAL_RECEIPTS_VERIFIED / NO_NEW_NATIVE_TRAINING. **Проверенная успешная ветка:** [language_translate @ f6032aff](https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9). **Текущая ветка:** [codex/dpo-implementation @ a392b6be](https://github.com/lubochka/llm_mvp_core/tree/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6). Целевой каталог — только planning/possibleDifs. Каталог core/docs/plans/replanning-possible-diffs использован **исключительно для чтения и дедупликации**.

## 0. Сначала четыре восстановления: приоритет для следующего запуска

1. **LT-P0/E09 — код и AST.** Вернуть реальный структурный KNN-предсказатель как обучаемого получателя структурных сигналов для C#/Design-to-Code (но не применять Roslyn к переводу). Привязать сохранённый структурный закон к обычному выдающему коду; проверить одни и те же source-family-disjoint файлы и одну метку SyntaxKind для baseline/KNN.
2. **LT-P0/E26 — контекст значения.** Для RU→EN сначала воспроизвести прежнее различение значений технических слов по POS-контексту, затем доказать, что новые пары меняют обычный перевод на *новых* контекстах. Ручной словарь без нового обучения = хороший deterministic baseline, не научное доказательство neural learning.
3. **LT-P0/E43 — сохранение старого при новом.** Перенести механизм replay как отдельно измеряемый возврат старых знаний: old-set before → new-only → new+replay, с одинаковым budget и file-disjoint проверкой. Это не равно переносу AdamW moments или просто сохранению checkpoint.
4. **LT-P1/E22 — условная маршрутизация.** Сохранить поздний **PASS** как источник полезной идеи, но обязательно тестировать классы с нулевым F1 и реальное использование правильного набора данных. Ранний FAIL и поздний PASS относятся к различным корпусам и не являются одним причинным A/B.

**Главная проверяемая цель:** прежнее поведение в том же типе learner, на одном и том же наборе вопросов и у того же normal consumer. Не подменять сравнение E09 AST→KNN сравнением с DPO q/v LoRA Student; это разные recipients. После восстановления каждого исторического capability отдельно провести мост к RP01/ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph. Поведение четырех интерфейсов должно быть **фактически вызвано и сохранено**, а не только упомянуто в плане.

## 1. Прямые исторические receipts именно в language_translate

| ID / область | Baseline → enhanced | Обучаемое состояние, границы | Оригинальный receipt |
| --- | --- | --- | --- |
| **E09, C# структурный KNN** | top1 **0.2413 → 0.3507**; 4,492 пар, k=5 | learned neighbor index; 763.5 ms enhanced против 14.5 ms baseline; **непарные validation denominators**, нельзя утверждать причинный прирост 10.94 п.п. | [E09 run 2026-05-01-001](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) |
| **E26, RU техническая неоднозначность** | accuracy **0.40 → 1.00**, всего **15** fixed cases, **6** hard | контекстная словарная функция/POS, НЕ показано онлайн-обновление весов от новых уроков; не переносить 100% на production corpus | [E26 run 2026-05-01-003](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json) |
| **E43, continual replay** | old-retention **0.3226 → 0.3978** | oldPairs=465, newPairs=414, replay=177; восстановлена полезность replay для bigram, НЕ LoRA или живой сервис | [E43 run 2026-05-01-001](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) |
| **E22, ранний** | macro-F1 **0.4892 → 0.4944**, **FAIL** | 325 samples, 260/65; TypeScript, Orchestration, Tests = 0 | [ранний receipt](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E22-DATASET-ROUTER/results/run-2026-05-01-002.json) |
| **E22, поздний** | macro-F1 **0.4709 → 0.5646**, **PASS** | другой корпус 450 samples, 360/90; Tests и TypeScript остаются 0 | [поздний receipt](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json) |
| **E51A, C# context pipeline** | baseline predicate overlap **0.2051 → 1.0**, 13 V-проб | 27 T-уроков; этот результат показывает узкую predicate trace/retrieval, НЕ исходный код, написанный нейросетью с нуля | [E51A receipt](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/experiments-v2/E51A-CODE-CONTEXT-PIPELINE/results/run-2026-05-04-100228.json) |

**Точность интерпретации E09:** [исходник E09 L25–75](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L25-L75): bigram оценивается на всей validation, KNN — на первых 500 элементах. [E09 L97–119](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L97-L119) использует локальные per-file SyntaxKind ordinal как псевдо-BpeId, а не универсальный BPE ID. Оба обстоятельства уже отмечались ранее, поэтому **это НЕ новое открытие**. Сначала выровнять метки и cohort. [E26 код](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L13-L61) вызывает BuildDefault, а не training loop. [E43 код](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L18-L53) строит новый bigram с/без replay — это позитивный исторический механизм, но не доказательство общего Continual Learner.

## 2. Почему RP01 всё ещё не был восстановлением этих способностей

Из [RP01 learning recovery audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) и [RP01 current code map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md): C#500 — 50 optimizer steps и free **0/3 → 0/3**; EN→RU500 — ещё 50 steps, free **0/4 → 0/4**, при небольшом изменении LoRA/margin. Random B0, vocab=629, q/v LoRA без доказанной базовой компетенции. Между двумя историческими семействами и RP01 нет общего trainable state и общего quality consumer по умолчанию.

Прежний положительный путь typed memory вообще может не зависеть от LoRA. Например, уже выполненный [P125 memory ingress](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/required-changes.md) зафиксировал **11/24 → 19/24** на узкой панели из 8 уроков. Не объявлять этот результат результатом ветки language_translate: это отдельный позднейший тест нынешней гибридной машины.

## 3. Карта утраченных или неподключённых функций: не путать поиск, обновление весов, правила и обычный ответ

| Функция | Где видна в успешной ветке | Подозреваемый gap текущего learning-first | Домен, в котором имеет смысл | Статус |
| --- | --- | --- | --- | --- |
| Roslyn structural feature→kNN→target | E09 .cs и archived JSON | generic hash/vector learner или q/v Student не обязан иметь ту же структуру, file-local label codec и обычную точку потребления | C#/code, не RU/HE natural-language перевод | CONFIRMED_DONOR / TRANSFER_UNPROVEN |
| POS-constrained contextual meaning | E26 .cs, E26 receipt | отсутствие контекстно-выбранного значения или обучаемой связи в современном translation Unit | RU→EN, технический русский; для EN→RU нужен отдельный parser | CONFIRMED_HISTORICAL_BASELINE / CONTINUAL_LEARNING_UNPROVEN |
| Replay rehearsal | E43 .cs/JSON | DPO continuation без loss of old-law probes либо без replay правильного owner | Любой continual domain, но этот опыт использует bigram | CONFIRMED_DONOR / ADOPTION_UNPROVEN |
| Predicate multi-label routing | E22 early+late и код | маршрутизатор не доносит несколько datasets до consuming Flow | C#, TS, tests, orchestration; особенно классы с нулевым F1 | HISTORICAL_MIXED |
| Code predicate trace | E51A 27 T + 13 V | RP01 free generation не доказывает actual invocation typed Trace Units | C# only | RECEIPT_CONFIRMED / END_USER_SKILL_UNPROVEN |
| Восстановление соответствий после сохранения | поздний P125/core required-changes | cold-load/migration и checkpoint owner могут не обслуживать production consumer | Перевод; другие домены по отдельной схеме | EXISTING_POSITIVE_NOT_LANG_TRANSLATE |
| Product-side semantic judge не совпадает с правильным переводом | текущий ProductHeldOutRunner (отдельный аудит) | случайный правдоподобный ответ может засчитываться как product success | EN↔RU/HE, где рубрика требует полного смысла | **NEW_CANDIDATE**, см. [45](45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md) |

## 4. Дедупликация и граница двух реестров

| Тема | Где уже была | Этот аудит не присваивает себе |
| --- | --- | --- |
| Per-file E09 local IDs, modulo 200 & mismatch | [planning 26](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md) | не новый механизм |
| E09 unmatched cohorts и file leakage | [planning 12](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md), [planning 38](38-NEW-E09-WITHIN-FILE-HOLDOUT-LEAKAGE-2026-10-16.md) | не новое доказанное improvement |
| DPO response mask, prompt truncation, SHA tokenizer | [planning 16 bridge](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md), [planning 39](39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md) | не считать повторно |
| Semantic Tokenizer language key/RuleTags | [planning 26](26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md) | не новое |
| QualityVerifier concept/context parser enum order | [protected core 44](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/44-structured-lessons-roles-keys-and-word-alignment.md) | проверено — **уже найдено другим агентом**, не новый DIF |
| Legacy four→six direction checkpoint migration | [planning 41](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md) | не новое |
| Rejected phrase-marker→positive memory | [planning 41](41-REJECTED-SIDE-ANNOTATION-CONTROLS-POSITIVE-EXECUTOR-MEMORY-2026-10-18.md) | не новое |
| Успешная память P125, но не neural free answer | [protected core required changes](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/required-changes.md), [planning 43](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/README.md) | не приписывать language_translate |

**Арбитраж:** ни один historical PASS не разрешает запуск большой DPO/LoRA партии ради «повторения успеха» без same-cohort, same-recipient, same-consumer и независимого смысла ответа. Для быстрого исполнения см. [микропротокол 46](46-MICRO-LANGUAGE-TRANSLATE-RESTORATION-AND-EVAL-2026-10-19.md).