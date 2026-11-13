# ГЛАВА 0 — DesignToCode-clean GOLD-FIRST: вернуть подтверждённо полезные механизмы, а не имитировать метрику RP01

**Дата:** 19.10.2026 · **Получатель:** Xiigen Planning / `possibleDifs` · **Эталон:** `llm_mvp_core@DesignToCode-clean` (наблюдённая HEAD `e346b88a3cc1fcce48aefa626bfc7b00509eefd6`) · **Сравнение:** `codex/dpo-implementation`.  
**Статус:** READ-ONLY SOURCE AUDIT + HISTORICAL JSON; **нового native .NET обучения/semantic A/B не выполнено**. Защищённая `llm_mvp_core/docs/plans/replanning-possible-diffs/**` только прочитана; модификация запрещена.

## 0. Что означает слово «успешно» — первичные GOLD-квитанции

| Механизм и тема обучения | GOLD-квитанция и исходный код | Замерено | Что НЕ следует из результата |
|---|---|---|---|
| **E25 POS/order pattern classifier** · последовательность слов / языковой синтаксис | [gold E25 JSON](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json) · [gold code](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/Experiments/E25PatternClassifier.cs) | top1 0.3438 → **0.8750**, val=32, examples=160 | Не доказывает улучшения живого Translator или LoRA |
| **E26 contextual dictionary** · многозначный перевод | [gold JSON](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json) · [gold code](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs) | disambiguation 0.40 → **1.00** на 15 встроенных случаях | `BuildDefault()` с готовыми entry — **не обученный с нуля** словарь, не generalisation |
| **E09 structural kNN** · AST/структура кода | [gold JSON](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) · [gold code](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L45-L120) | reported 0.2413 → **0.3507** | Baseline на `val`, kNN на `val.Take(500)`: **не matched causal lift**; per-file SyntaxKind IDs тоже нестабильны |
| **E43 replay** · сохранение ранних знаний | [gold JSON](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) · [gold code](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs) | old retention 0.3226 → **0.3978** | Пересоздаётся bigram с replay; **не** доказано online продолжение того же LoRA checkpoint |
| **E40V2 learned trust** · доверие к источнику | [gold JSON](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/results/E40V2-TRUST-SCORER/results/run-2026-05-01-001.json) · [gold code](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/Experiments/E40TrustScorerV2.cs) | trust 0.6364 → **0.8182**, val=11 | Не измерено качество ordinary domain answer; узкий trust классификатор |
| **E22 ранний контрпример** · router across datasets | [gold FAIL JSON](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/35.ExperimentsV2/results/E22-DATASET-ROUTER/results/run-2026-05-01-002.json) | macro-F1 0.4892 → 0.4944, `passed=false`, val=65 | «FAIL» этого раннего варианта **не** опровергает иной поздний успех / перенос полезного подалгоритма в kNN |

**Вывод для GOLD-приоритета:** вернуть проверяемую цепочку от соответствующего механизма, а не объявлять общий победный процент. Особенно отделять **параметрическое обучение**, **обученную память**, **заготовленный детерминистический алгоритм**, **обучаемый feature scorer** и **результат обычного consumer**.

## 1. RP01 — получатель обучения и различия, которые реально влияют на постановку

[Независимый аудит RP01, source+artifacts](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): C# 500 пар / 50 шагов / 16 изменившихся LoRA tensor / свободный экзамен **0/3→0/3**; EN→RU 500 пар / 50 дополнительных шагов / те же 16 trainable tensor / **0/4→0/4**. Frozen random B0, словарь 629, q/v LoRA; это **другой обучаемый получатель**, чем E09/E25/E26/E40/E43 или обученные `TranslationTrainedUnits`. Не доказана общая регрессия единой модели.

Source подтверждает, что в нынешнем [DpoTrainer.StepVectorCore](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L832-L941) `TeachingVector` используется для **скалярного** множителя градиента `VectorSignal`, а не как перенос learned AST/POS/kNN представления. Это **разный алгоритм**, а не автоматическое восстановление проверенных функций E09/E25.

### GOLD-0 — самый ранний GO/NO-GO для нейронного Student (C#/ENRU)

- **A** тот же законно допущенный random B0 + DPO-only; **B** тот же B0 + короткий chosen-only supervised warm-up; **C** warm-up + DPO; **D** дополнительно **явно подключённый** обучаемый Unit/memory. Держать tokenizer, train/eval family split, бюджет обучения и decoding options на контроле; D и C различаются только маршрутом learned memory.
- Начать с 8–32 train-only пар, отдельного unseen holdout; измерять teacher-forced chosen probability, length/tokenization, **реальные free answers**, first-token choices, policy/reference hash, LoRA delta, cold restore и provenance.
- Если tiny-train не улучшается, **STOP**: исследовать gradient/optimizer/token-mask/head/LoRA attachment/decoder и не гонять следующие 500. Если train растёт, но free ответ нет, исследовать pretraining/language modeling, decoding/EOS и семантический prior. Если исправляется только D, ключевая проблема — **loss of hybrid consumer**, не только оптимизатор.
- Сохранить новые exact-DPO, autograd, admission и checkpoint-state исправления нынешнего core; **не** откатывать всю ветку ради исторического кода.

### GOLD-1 — восстановление E25 (порядок и POS), приоритет P0 для Language / P1 для Code

Получатель: конкретный POS-pattern classifier или translation unit, **не** общий Student. **A** vector bag-of-tokens, **B** тот же вектор + POS bigrams/order, **C** B + syntax-tree structure **только для C#**, **D** доменно настроенная фузия. Честные 12 train / 8 dev / 24 holdout (например); зеркальные инверсии порядка, negation/agent-patient и unseen verbs. Равные dataset IDs, одна и та же tokenization/memory/семантическая rubric, `train→export→import→ordinary inference`. Положительный E25 исторический JSON — мотив для опыта, **не порог гарантированной победы**.

### GOLD-2 — восстановление E26 (контекст / многозначность), приоритет P0 для перевода

Группы: `bank` financial/river, `book` noun/verb, различение роли/референта и EN→RU/RU→EN; направления — **не** отдельные домены. **A** `BuildDefault` lookup (старый static donor), **B** обучаемый context-index со стабильным адресом урока, **C** B после cold restore, **D** B на реальном `TranslationMachine / TranslationRoutingFlow`; 8 known + 16 previously unseen contrast prompts. Измерить 1) корректный смысл, 2) нарушение отрицания/порядка, 3) abstention при отсутствии контекста, 4) actual consumer. Не объявлять E26 static accuracy доказательством онлайн обучения. Предыдущие чужие E26 gap: [N51](51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md), [language_translate restoration](44-RESTORE-LANGUAGE-TRANSLATE-FIRST-2026-10-19.md).

### GOLD-3 — E09 structural kNN без иллюзии metric и без порчи соседних доменов

C# AST feature надо мерить на **том же** holdout, где сравнивают TF/BPE retrieval; SyntaxKind enum/map единый по корпусу, не новый per source file; `k`, threshold, metric, candidate budget фиксированы. Язык перевода AST C# использовать **нельзя**; возможен POS/order-only профиль. Доменные мини-наборы по operator precedence, null coalescing, invocation, scope; отдельный SQL профиль с собственным parser/null rules. Перенос E09 из матрицы в kNN надо маркировать `RETRIEVAL_FEATURE_REHABILITATION`, не как LoRA-матрицу. Crossref [E09/validity](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md), [KNN order test](11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md).

### GOLD-4 — E43 replay, сохранение без ложного lineage

Получатель: **один и тот же** current trainable recipient (LoRA или persisted Unit), неизменный базовый checkpoint. Сравнить A=no-replay, B=lawful replay, C=B+cold restore, на **одних и тех же** прежних и новых доменах. E43 historical bigram — **отдельный положительный evidence для смеси обучающих данных**, а не доказательство online LoRA memory.

### GOLD-5 — E40V2 source trust и orchestration

Изолированное обученное `TrustScorer` состояние; 8 train / 8 unseen trusted/untrusted cases + 4 конфликтных evidence; оценить false trust, abstention и пропуск в **обычный** domain answer. Для orchestration к этому добавляется отдельная provenance-aware ветка `orchestrator-extensions`; не путать источник `DesignToCode-clean` с этим donor. См. [основной мастер, глава 0A](00-MASTER-SYNTHESIS-AND-CONFLICTS.md) и [orchestration GOLD](00-ORCHESTRATOR-EXTENSIONS-GOLD-RECOVERY-FIRST-2026-10-19.md).

## 2. Приоритет решения (не выдавать существование файла за применённое умение)

| Очерёдность | Действие | Критерий доказанного восстановления |
|---|---|---|
| **P0-A** | GOLD-0: tiny neural learnability, затем hybrid route | Изменился **смысл** free answers на независимом holdout, и эффект пережил restart; или честный `NO_GAIN` |
| **P0-B** | GOLD-1/2: E25 + contextual Unit через ordinary Translation Flow | Конкретные unseen referent/order/case ответы лучше одинакового контроля; нет критических false-positive |
| **P0-C** | GOLD-3 C# structure kNN, matched panel | Улучшение **на одинаковых example IDs**, не старые E09 unmatched значения |
| **P1** | GOLD-4 replay и GOLD-5 trust/orchestration | Same-state retention, provenance и конечный consumer, а не только отчёт |
| **P1/P2** | [новые узкие seam N54](54-NEW-LOGICAL-KNN-METADATA-ID-HASH-SEAMS-2026-10-19.md) | Подтверждённый actual caller → проявление на ordinary output → исправление одним фактором |

## 3. Обязательный формат квитанции на каждый GOLD опыт

`{gold_branch_commit, current_branch_commit, source_dataset_shas, legal_admission, train_case_ids, heldout_case_ids, tokenizer_sha, feature_version, recipient_model_id, graph_unit_id, optimizer_state_id, checkpoint_sha, cold_restore_sha, caller_trace, request_metadata_shape, prediction_before, prediction_after, judge_semantics, abstentions, critical_errors, elapsed_ms, peak_rss, original_historical_metric, exact_denominator, status}`.

Статусы **`SOURCE_CONFIRMED / HISTORICAL_RECEIPT / CAUSAL_HYPOTHESIS / NATIVE_NOT_RUN / OBSERVED_HELDOUT_GAIN`** сохранять раздельно. Сильный исторический результат у другого recipient — это направление поиска, не замена эксперименту. Никаких адаптаций test rubric под модель, фиктивных admission receipts и Windows CI как предварительного условия маленького обучения.

## 4. Специальная сверка отсутствия регрессии

Прямой GitHub blob-SHA для `DesignToCode-clean` vs `codex/dpo-implementation`: [`KnnLogicalUnit.cs`](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs) **совпадает** (`2957816a88aa...`), [`LogicalMetadataSnapshot.cs`](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/21.AgentInfrastructure/Logical/LogicalMetadataSnapshot.cs) **совпадает** (`0dec91ca749c...`); `HnswIndex.cs` и `04.TrainingLoop/Trainer.cs` также SHA-идентичны. Старые потенциальные дефекты в них **НЕ МОГЛИ появиться только от diff этих файлов**. Их можно исследовать как условные риски, **не** как доказанную причину RP01. В `TranslationRoutingFlow` current добавил отказ принимать bootstrap-only output как passed; это предохранитель, не механически ухудшившийся learner.

## 5. Передача исполнителю

Прочесть эту главу → [новые N54](54-NEW-LOGICAL-KNN-METADATA-ID-HASH-SEAMS-2026-10-19.md) → [короткие эксперименты](54-MICRO-DOMAIN-CAUSAL-EXPERIMENTS-2026-10-19.md) → [сквозной реестр механизмов и review](54-MECHANISM-COVERAGE-AND-REVIEW-2026-10-19.md). Каждый опыт имеет один владелец feature, один consumer, замороженные controls и два outcome (train signal vs unseen semantics). Нельзя называть `PASS` только наличие метода, graph node или красивый test report.
