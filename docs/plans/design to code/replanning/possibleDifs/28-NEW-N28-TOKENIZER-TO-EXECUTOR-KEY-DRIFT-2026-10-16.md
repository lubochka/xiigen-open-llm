# N28-A — Новый разрыв: обученный SemanticTokenizer и адрес памяти ExecutorUnit строятся разными алгоритмами

<!-- POSTMERGE-AUDIT-2026-11-02:START -->
## После объединения веток: тест не должен закрепить второй несовместимый контракт

Новый PAIR-тест комментирует «хвосты снимаются только на выходе, сохранённые ключи не меняются». План 124 S04 уже назначает совместную versioned миграцию ключей с отдельным provenance. Для N28 действует S04; односторонняя срезка не является его реализацией. Сначала согласовать ожидания теста с writer/tokenizer/executor/verifier/loader, затем принимать код.

Различающий пример: сохранённый ключ содержит `PAIR:id|RULE:...`; после миграции RULE и требование verifier доступны обычному запросу, а PairId не разрешает нулевой порог похожести. Нужны правильный перевод и verdict, а не только отсутствие строки `PAIR:` в Signature. Отдельно проверить коллизию старых ключей до изменения живой памяти.

После `ImportCheckpoint` чтение выполняет target, в который импортировали bytes. Созданная и заново обученная третья единица не заменяет этот путь. Мутация, сохраняющая re-export, но отключающая использование импортированной correction, обязана нарушить приёмку. Событие входа в KNN записывается отдельно от финального AdapterId.

Основание: [Аудит нового среза](codex-2026-11-02-POSTMERGE-TRANSFER-AND-EXPERIMENT-AUDIT.md); неизменные исходные результаты и назначенные бюджеты сохраняются.
<!-- POSTMERGE-AUDIT-2026-11-02:END -->

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Ключ меняется у всех писателей и читателей согласованно.** SemanticTokenizerUnit может вернуть исправленную signature с PAIR. ExecutorUnit видит этот сегмент и включает KNN с нулевым порогом похожести вместо обычного EN_RU 0.60. Verifier читает требуемые термины по связанному учебному ключу. Удаление сегмента только у одного читателя может сделать эти требования недостижимыми.

Выбранное решение: provenance отдельно; смысловой ключ сохраняет pattern, concepts и проверенные RULE; dictionaries, индексы и loader переходят на одну версию. Коллизии выявляются до публикации. Номер пары не даёт права обходить допуск.

Контрольные случаи: тот же смысл без PAIR и с двумя разными PairId; RULE после PAIR; неизвестная семантика с похожестью ниже, на и выше 0.60; исправный старый checkpoint и конфликт миграции. Нужна запись достигнутой ветви KNN: exact, phrase или word lookup могут вернуть ответ раньше. После нового Import без Train проверяются полный перевод и verdict обычного verifier.

Основание: [POST107-аудит](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) и [проверка реального кода](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md). Исторические результаты и бюджеты сохранены; это условия будущего переноса, не новый запуск.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


**Дата:** 2026-10-16. **Статус:** SOURCE-CONFIRMED DIFFERENT CALL PATHS / CAUSAL QUALITY HYPOTHESIS / NATIVE EXPERIMENT NOT RUN / independent judge NOT RUN.  
**Scope:** структурный переводческий контур `TranslationMachine` и `TrainedTranslationAdapter`; **не** прямой RP01 `CodeDocumentationSimulationDpoExperiment → DpoTrainer → LoRA → TextGenerator`. Это дополнительная гипотеза, а не установленная причина результатов RP01 0/3 или 0/4.

## Что упущено, простыми словами

После обучения SemanticTokenizer способен исправлять структуру предложения. Но ExecutorUnit во время собственного обучения заново токенизирует исходную строку **обычным** `new TranslationSemanticTokenizer()`, который не содержит этих выученных исправлений. При выдаче ответа Executor получает признаки уже от **обученного** `SemanticTokenizerUnit`. В результате KNN может хранить пример по адресу *до исправления*, а искать — по адресу *после исправления*.

Это не «в kNN недостаточно функций» и не «LoRA маленькая»; это **несовместимый feature extractor на границе между двумя обучаемыми Unit**. Даже хороший обученный семантический классификатор в таком случае может уменьшить retrieval coverage: исправил структуру, но downstream память перестала находиться.

## Прямые доказательства по исходному коду

| ID | Подтверждённый факт | Первичный код |
| --- | --- | --- |
| N28A-C1 | Семантический Unit сохраняет `_patternCorrections` и `_signatureCorrections`, а `Tokenize` применяет correction к `Signature`, `PrefixSignature`, `StructureSignature`. | [TranslationTrainedUnits.cs:70–122](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L70-L122) |
| N28A-C2 | Executor при `TrainOnExample` кладёт `example.SemanticSignature`, но дополнительные prefix/structure индексы строит через **новый** `TranslationSemanticTokenizer`, не через обученный `SemanticTokenizerUnit`. | [TranslationTrainedUnits.cs:403–424](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L424) |
| N28A-C3 | Live `TrainedTranslationAdapter.TranslateAsync` получает признаки из обученного `_semanticTokenizer.Tokenize` и отдаёт их в `_executor.Execute`. | [TranslationDirectionAdapters.cs:111–143](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L111-L143) |
| N28A-C4 | `ExecutorUnit.TrySelectKnnCandidate` ищет `tokens.Signature`, `tokens.PrefixSignature`, `tokens.StructureSignature`. `TranslationKnnStore.FindNearestCascade` ищет **точное** совпадение каждого из трёх ключей, а не автоматически переносит пример между «до» и «после» коррекции. | [TranslationTrainedUnits.cs:333–362](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L362), [TranslationUnitContracts.cs:241–293](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationUnitContracts.cs#L241-L293) |
| N28A-C5 | `TranslationMachine.CreateRuntime` создаёт один обучаемый SemanticTokenizer и передаёт его в live adapters, но Executor при training не имеет такой зависимости в своём конструкторе. | [TranslationMachine.cs:714–793](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L714-L793), [TranslationTrainedUnits.cs:254–260](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L254-L260) |

Проверенные blob identity (срез 16.10): `TranslationTrainedUnits.cs=1742abfc9fb617dfc66e4d618e0c169da01a6ccc`, `TranslationMachine.cs=fd0e736de3739236b4dde2d6b9d000f5ebae8416`; конкретный текущий commit при запуске проверки необходимо заново закрепить.

## Точная цепь возможной потери качества

1. До урока: rule tokenizer считает структуру A и выпускает адрес `K_old`.
2. Разрешённая обучающая поправка меняет Pattern/Signature: live SemanticTokenizer выпускает `K_new`, где `K_new ≠ K_old`. Это проверяемое условие; без него гипотеза не срабатывает.
3. Executor получает урок и через `new TranslationSemanticTokenizer()` кладёт пример в память по `K_old` (для prefix/structure), а для full — по `example.SemanticSignature`; последний может быть отдельным `|PAIR:id`, а не `K_new`.
4. Live запрос класса A (в особенности новый, source-disjoint) приходит с `K_new`. Exact-source и phrase-map на незнакомой фразе не обязаны сработать; `FindNearestCascade` не находит старый bucket, даже если учитель передал правильный `chosen`.
5. Система может вернуть fallback, а отчёт «Unit обучился» останется положительным. Влияние на реальные correct answers пока **не измерено**.

**Нюанс:** `TranslationStructuredBatchBuilder.BuildGeneralBatch` обычно строит `chosen.Pattern` из собственного bootstrap tokenizer, а не из независимого oracle. Поэтому в стандартном наборе **может вообще не возникнуть реально отличающегося Pattern**. Это ранее найдено как [N21-01](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md). Наше N28-A проверяет **следующий независимый разрыв**, *если* полезная коррекция действительно появилась: передаётся ли она в адреса downstream learned Executor. Исправление только self-label N21 без исправления N28-A может оставить learning gain невидимым.

## Что уже найдено другими агентами — не выдавать за нашу новизну

- [N21-01](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md): bootstrap self-label и `|PAIR:id` против runtime correction key.
- [N26 архитектурная схема](26-ARCHITECTURE-BLUEPRINT-TOKENIZER-KNN-STATE-AND-RESTORE-2026-10-16.md): язык/регистр correction key и потеря RuleTags.
- [N16 KNN](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md): FIFO/top-k, source predicate и topic-domain.
- [N23 POS](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md): неконсистентность регистра `RU/ru`.

**Узкая новизна N28-A:** линия **обученный upstream feature extractor → новый, необученный extractor для indexing downstream Unit → обученный extractor для inference**, и необходимость исправлять её *в одном* controlled A/B, а не вместе с LoRA, правилами, шириной BPE и HNSW.

## Условия применимости и приоритет

- **P1 для domain-dependent learning** (перевод, морфология, AST/syntax-aware правила, contextual kNN), если есть подтверждённые изменённые токенизатором Pattern/Signature.
- **P3 / NOT_APPLICABLE** для чистого frozen-tokenizer kNN и RP01 neural-only: этот конкретный объект не вызывается в их измеренном маршруте.
- Точность самих tokenizer labels, масштаб влияния на translation и доля запросов с изменившимся адресом — **OPEN**.

## Быстрая опровержимая проверка

См. [28-MICRO-N28-CAUSAL-AB-EXPERIMENT](28-MICRO-N28-CAUSAL-AB-EXPERIMENT-2026-10-16.md), основной фактор A/B: `ExecutorUnit` записывает learned index features от bootstrap vs от того же `SemanticTokenizerUnit`, что используется для ordinary inference. Сначала проверить **key divergence**, затем **blind source-disjoint user answer delta**. Никакой масштабной DPO кампании или Windows GitHub run до различающего результата не требуется.

**Вердикт self-review:** SOURCE GAP CONFIRMED, **QUALITY IMPACT NOT PROVEN**. Нельзя делать production-fix по одному совпадению гипотезы с симптомом.
