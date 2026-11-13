# Параметризация семантики, вектора и kNN — точная трасса

<!-- POSTMERGE-AUDIT-2026-11-02:START -->
## После объединения веток: номер урока и рабочая семантика

**Новый конфликт приёмки:** `TranslationPairTailSignatureLearningTests` требует оставить PAIR в сохранённых рабочих ключах и убирать его только в выводе. Это не реализует уже выбранное в плане124 S04 разделение происхождения урока и смыслового ключа. Успех одного такого assert нельзя считать переносом S04.

Перед исправлением теста составить одну таблицу версии ключа: builder → tokenizer corrections → executor/index → verifier requirements → checkpoint/import. Для одинакового смысла без PairId и с двумя разными PairId допуск и полный правильный ответ одинаковы; происхождение урока сохраняется отдельно. RULE до и после PAIR остаются. Конфликт двух старых записей, сходящихся в один новый ключ, обнаруживается до публикации состояния, без произвольного последнего победителя.

Проверка проходит через обычный consumer, с отдельными `search_attempted` и `selected_source`; точный lookup и запасной ответ не подменяют доказательство ветви KNN. Это согласование приёмки, не разрешение новой учебной дозы.

Основание: [Аудит нового среза](codex-2026-11-02-POSTMERGE-TRANSFER-AND-EXPERIMENT-AUDIT.md); неизменные исходные результаты и назначенные бюджеты сохраняются.
<!-- POSTMERGE-AUDIT-2026-11-02:END -->

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Смысловой ключ, личность урока и ранжирование имеют разные назначения.** PairId хранит происхождение урока, не добавляет смысл и не снижает порог похожести источника. При смене ключа согласованно обновляются запись, чтение и сохранённое состояние; проверенные RULE-сегменты сохраняются. Один логический урок даёт один голос, независимо от числа физических записей в индексах.

Удержание проверяется по полному ответу и порядку шагов того же обученного состояния. Подмена тела ответа при прежнем ранге должна ломать неизменный оценщик. Количество векторов и длина ответа недостаточны. Для равных оценок сохраняются конкретные группы и перестановки; если все кандидаты приводят к одному ответу, вывод об устойчивости ограничен этими группами.

P107 показал 8/8 на обучающих контролях и 4/4 на новых формулировках. Но 0/12 разногласий первого соседа и большинства не проверяет пользу смены читателя. Экспериментальный доступ к соседям и обычный Execute описываются отдельно.

Основание: [POST107-аудит](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) и [проверка реального кода](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md). Исторические результаты и бюджеты сохранены; это условия будущего переноса, не новый запуск.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->

## 1. Важная потеря информации ещё ДО модели
`TrainingVectorBuilder.VectorWidth=60`: 22 predicate, 5 domain, 32 root coordinates и 1 control slot. Но `Build(prompt)` создает `rootVocab=BuildRootVocabulary([prompt])`, а `Build(compiled)` — vocab из данного compilation. В `LogicalDpoTrainingUnit.PrepareExecution` при null TeachingVector вызывается **именно** `TrainingVectorBuilder.Build(preference.Prompt)`. Поэтому разные root strings могут занимать **одну и ту же координату** в разных обучающих парах, а тот же root — другую. Это риск применения общей «семантической координаты» между парами; при этом не доказывает порчу raw DPO text. [TrainingVectorBuilder.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L14-L62); [LlmIngredientLogicalUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1711-L1745).

Дополнительная потеря **в конкретном downstream**: `DpoTrainer.VectorSignal=Clamp(1+Count(|slot|>1e-6)/60,1,2)`. Содержимое слота (включая доменный закон, знак, позицию) не определяет objective по отдельности. Даже если shared vocab исправить, нынешний DPO consumer не начнет учить meaning of slots без нового **явного** mapping/loss. [DpoTrainer.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/DpoTrainer.cs#L1159-L1166). Не подменять learned semantic projection изменением имён полей.

`TrainingVectorBuilder.NormalizeRoots` применяет regex `[^a-z]+` — в **этом root-канале** удаляются цифры/пунктуация/кириллица и структура операторов. В другом канале raw текст сохраняется. Также прошлое R04 сообщает о stopword-normalizer (`not/no/if/then/else/must/may`), но не надо приписывать его каждой функции без трассы фактического вызова. [TrainingVectorBuilder.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L104-L134); [FINDINGS.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/replanning/results/R04/FINDINGS.md#L40-L68).

## 2. Что делает текущий generic KnnLogicalUnit
`Train`: свой BPE обучен на корпусе Unit, `vocabSize=320`, HNSW(`m=8`, `efConstruction=64`, `efSearch=32`), default vector width64, k3; этот index/vocab **не тот же**, что 60d TeachingVector или 629-vocab RP01. [KnnLogicalUnit.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L105-L155).

`Vectorize` хеширует счетчики BPE tokens по `token % tokenTierWidth` и predicates по `StableHash(predicate) % predicateTierWidth`, затем norm. Порядок, scope, roles, exact числа и семантическое negation не закодированы независимо; коллизии возможны. `ExecuteAsync` запрашивает `_k` соседей, но выполняет prediction `best=hits[0]`; прочие IDs добавляются в evidence. Low score приводит к warning, но возвращается `Ok(bestExample.Output)` (не отказ). [KnnLogicalUnit.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L201-L320). Generic Unit не содержит здесь явно law-veto или adaptive k; не распространять вывод на `TranslationKnnStore` и внешние law checks.

## 3. Чего требует старый подтверждённый проектный контракт
[ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md#L1-L113):
(а) сначала допустимый предметный pool, (б) similarity / semantic constraints / context, (в) positive/negative/warning memories с причиной arbiter rejection, (г) arbiter post-generation властвует над принятием. [DECISIONS-LOCKED.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/llm%20dynamic%20flow/DECISIONS-LOCKED.md#L9-L21) закрепляет fusion и authority. Эти требования **нельзя восполнить простым k=10, сохранив first-hit**; нельзя заменять lawful answer nearest copy при задачах, требующих typed many→many.

Эксперимент E09 с AST structural tokens имел +10.94 pp top1, E34v2 BM25+KNN +5.56 pp hit1, но E14 context-action KNN оказался хуже majority. Разница связана с representation, balance и task, не с названием kNN. См. [02](02-HISTORICAL-EXPERIMENTS.md).

## 4. Различающие adversarial examples (не DPO training data)
- ENRU: `the box is not empty` / `the box is empty` — lexical near-neighbors, противоположное negation.
- ENRU: `Anna gave Maria the book` / `Maria gave Anna the book` — те же tokens, обратные semantic roles.
- SQL: `LEFT JOIN` + WHERE по правой таблице может потерять unmatched rows; различать scope/NULL.
- C#: `x++` / `++x`, `string? s; s?.Length ?? 7`, nullability и operator precedence; AST structural oracle.
- C#/translate: версии/диалекты, количества10 vs100, отрицание, порядок действий; кодbook должен сохранять существенную информацию.
- UNKNOWN referent: не делать hard veto по неполным antecedents, допустим `abstain/context request`.

## 5. Минимальные численные проверки
Парные contrast exact/hnsw; map old vs stable shared coordinate; candidate rank+allowed law; precision/recall/coverage/abstentions, law violation rate, selected UnitId, time/memory. Проверить `positive/negative/warning` без leakage правильного ответа в query metadata; negative пример может warn, но не становиться выбранным output. Все library retrieval абляции отдельно от Student LoRA learning.
