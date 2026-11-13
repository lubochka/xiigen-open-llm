# N54-MICRO — быстрые различающие эксперименты по доменам (C#, SQL, перевод, orchestration)

**Статус 19.10.2026:** SPEC CREATED; Python source-equivalent узкие проверки выполнены отдельно, **native .NET runner NOT_RUN**, реальный Student training NOT_RUN; **не утверждать `OBSERVED_LEARNING_GAIN`**. Перед запуском убедиться, что данные законно допущены, а изменяемая ветка / owner не нарушают R17. Не запускать Windows CI, большой GitHub workflow и 500 DPO до короткого результата.

## Общий immutable контур

- Разделить `train`, `dev`, `heldout` по семантической family/source lineage; сохранить case IDs и SHA. Нельзя использовать `heldout` для подбора признаков, порога, prompt/label/rubric.
- Не считать `Unit` и `domain` одним и тем же: C# / SQL / перевод / orchestration — **предметные темы**; kNN, LoRA, AST — методы; EN→RU / RU→EN — направления темы перевод.
- Для маленьких тестов N54 ниже использовать независимый `KnnLogicalUnit<TInput,TOutput>` в **isolated test harness** и обычный зарегистрированный `ILearningFlow` в отдельном шаге; никакой ручной фальсификации результата обычного consumer.
- Каждый A/B: одинаковый tokenizer version, fixed seed (где доступен), `train_examples`, `heldout_case_ids`, IDs, k, confidence, efSearch, vectorDimensions, domain profile, replay budget; **ровно одна факторная разница**.
- Самые важные выходы: верный/неверный смысл, критические ошибки, abstentions, доказательства маршрута learned memory, холодная перезагрузка, время/память, quality **до→после**. GPU / PC ограничения отдельно учитывать при neural-этапе; этих kNN микро-экспериментов локальный CPU достаточно по объёму.
- Описать `EXPERIMENT NOT RUN` если runner не исполнялся. Не превращать source-equivalent proxy в .NET experiment или исторические метрики в новые.

## N54-M1 — semantic predicate metadata erasure: один и тот же запрос, разные формы metadata

**Гипотеза:** если `predicate_ids` приходит в `List<string>`/`IReadOnlyList` вместо `string[]`, прямой `KnnLogicalUnit.ExecuteAsync` игнорирует второй tier и может выбрать неверный Output; upstream snapshot может вместо этого fail-close.

**Темы:** C# оператор / SQL NULL / translation negation.  
**Мини fixture (обучение 6, экспериментальные query 12):**

- TRAIN: `OP-PRE` текст «value increment», `PredicateIds=["PRE_INCREMENT"]`, результат «new»; `OP-POST` **тот же текст**, `PredicateIds=["POST_INCREMENT"]`, результат «old»; 4 другие законные заполнители без повторных IDs и противоречивых меток. Во второй теме: `NULL-IS` vs `NULL-EQUAL`, третий компонент negation scope (не применять SQL синтаксис к ENRU).
- Query Q1/Q2 отличаются **только predicate value** (учительский смысл), в обоих arms задаётся один и тот же текст. A `Metadata["predicate_ids"] = new string[] {"POST_INCREMENT"}`; B `new List<string>{"POST_INCREMENT"}`; C metadata отсутствуют (negative); D тот же B через `LogicalMetadataSnapshot.TryCreate` + registry ordinary flow (чтобы отличить fail-close).
- **Контроль:** повторить всю процедуру на GOLD с тем же caller. Если оба золото/текущая дают одинаковый ошибочный B, это latent seam, не regression по ветке.
- **Pass SOURCE BUG:** `Vectorize` query для B использовал пустые предикаты; **Pass causal for domain:** на source-disjoint heldout `correct_B < correct_A` и M1-only serialization adapter `List→string[]` восстанавливает accuracy без нового обучения; для D возможно ожидаемое явное исключение snapshot. **No effect:** разница отсутствует — сохраняем как нерелевантное этому маршруту, без «победы».
- **Артефакт:** `M1-predicate-format.json`: `caller_shape, train_predicates, query_predicates_effective, top_ids, output, consumer_status, correct/12, error_code`. **Не предполагается** что запрет Snapshot нужно ослабить; правильный repair может быть строгая ранняя валидация и канонизация.

## N54-M2 — duplicate ID indexes old vector with new output (micro structural kNN)

**Гипотеза:** один parent DPO ID используется для двух child examples с разными inputs/outputs, HNSW хранит обе точки, а dictionary связывает оба с последней меткой.

**Темы:** C# `x++` vs `++x`; SQL `IS NULL` vs `= NULL`; EN→RU `not authorized` vs `authorized`, с учётом контекста; для каждого набор из **двух** явных противостоящих семантических правил + 4 distraction neighbors. Сначала **native Unit** 6 train / 8 holdout, затем (только если consumer реально wired) 1 ordinary Flow case.

A = `OP-A` и `OP-B` distinct IDs; B = тот же train/corpus/order, но **оба имеют `OP-A`**; C = B в обратном порядке (должен меня́ть последний stored output при том же id); D = reject duplicate before HNSW mutation (candidate fix); E = B с прежним tokenizer+exact cosine index без approximation как diagnostic, чтобы убрать confound поиска.

**Pass source seam:** B `Manifest.TrainingExampleCount=5` для 6 вставок (assuming other IDs unique), `index` имеет 6 векторов; хотя в B выполняется запрос к старому вектору, возвращается last stored output. **Causal semantic pass:** B больше критических ошибок, чем A при **одинаковых case IDs**; D восстанавливает корректность или легитимно отвергает bad training с именованным error. Если B рассыпался ещё в upstream duplicate admission, defect уже отфильтрован и claim ограничить unit API.

**Контроль на golden:** same `KnnLogicalUnit.cs` имеет тот же SHA; нельзя трактовать совпадающую ошибку как новую branch regression. Положительная причинность в **production** требует доказать, что actual DPO→subunit mapping/parent-child IDs создаёт коллизию. Гипотеза этого шва — связь с декомпозицией, не подтверждённый факт.

**Запись:** `M2-index-size, dict-size, nearest-hit-vector-source, nearest-hit-id, materialized-output, correct/8, consumer-call-path, checkpoint-identity`. Отдельно протестировать `" OP-A "` vs `"OP-A"` (коллизия после `.Trim()`).

## N54-M3 — exact synthetic signed-overflow в hash predicate feature

**Гипотеза:** массив `PredicateIds=["ZSJBINM"]` вызывает ошибку `Math.Abs(Int32.MinValue)`. Условие выведено из исходной формулы `hash=23; hash=unchecked(hash*31+(int)char)`. Python-proxy сохранён в [скрипте](54-KNN-SOURCE-EQUIVALENT-PROXY-2026-10-19.py) / [квитанции](54-KNN-SOURCE-EQUIVALENT-PROXY-RESULT-2026-10-19.json).

**Темы:** только synthetic robustness across all subject topics; **не** замер learning quality.  
A `PredicateIds=["ZSJBINN"]` контроль; B `["ZSJBINM"]` exact min value; C `["zsjbinm"]` (normalize uppercase); D `[""]` empty filtered; E candidate unsigned modulo repair + tokenizer/vector index **rebuild with feature version**.

**Native .NET probe** (в отдельном маленьком harness, без CI; код псевдо-setup — типы формы запроса взять из текущего `AgentInfrastructure`):
```csharp
unchecked {
    int h = 23;
    foreach (char c in "ZSJBINM") h = h * 31 + c;
    // Expect: h == int.MinValue.
    // Math.Abs(h) throws OverflowException in current code.
}
```
**Pass:** native `Train` B returns `KNN_LOGICAL_UNIT_TRAINING_FAILED` and A succeeds; исправленный candidate B+E стабильно ведут вектора одинаково train/query после rebuild. Никакого изменения качества RP01 этот тест не обещает.

## N54-G0 — GOLD-first semantic reconstruction (важнее N54 buglets)

Ниже короткая привязка исторически позитивных алгоритмов [ГЛАВЫ 0](00-54-DESIGNTOCODE-CLEAN-GOLD-FIRST-2026-10-19.md) к обычному продукту:

| Пилот | Предметная тема | Arm A vs B (+C) | Необходимая native метрика и критерий |
|---|---|---|---|
| **G0-N** | C# и перевод | random B0+DPO-only vs warm CE→DPO vs +Persisted Unit | 8–32 train; unseen semantics / free generation; identical source split; if no train-token improvement stop before 500 |
| **G0-E25** | порядок предложения | current bag-of-token vs learned POS order feature | 12 train/8 dev/24 heldout, 6 order-inversion probes, same actual Flow, cold restore |
| **G0-E09** | C# AST | bag vs structurally typed kNN | matched eval subset, one SyntaxKind mapping, guard law; no transfer AST to translation by default |
| **G0-E26** | перевод / контекст | preloaded `BuildDefault` vs persisted learned context entries | 8 known/16 unseen, cold restore, ordinary `TranslationRoutingFlow`; avoid false claim E26 was learned |
| **G0-E43** | multi-domain retention | old+new no replay vs old+new with replay, **same state** | old retention and new adaptation on same holdout; separate bigram archived indicator |
| **G0-E40** | source trust | fixed scorer vs learned scorer | 8/8 legal trust cases + 4 conflict probes; false accept/error and real consumer |

**Все G0: DESIGN/SPEC, NOT RUN.** Для каждого реальный criterion: улучшенный семантический ответ на independent source-disjoint holdout при одинаковом denominator; зафиксированы границы применимости; после cold restore тот же learned behavior, не только manifest `trainingCount`.

## Обязательный stop/triage и короткое поручение executor

1. Сначала 15 минут source tracing: `RP01 inputs → materializer → policy LoRA` отдельно `child unit admission → kNN train → manifest → export/import → ordinary consumer`. Не смешивать траектории.
2. Исполнить N54-M2 (6 examples) и N54-M1 (12 probes) на Windows local .NET только после preflight legal source; вывести JSON `PASS / FAIL / BLOCKED / INCONCLUSIVE`, пути/refs, before/after. M3 выполнить как cheap robustness check.
3. Выполнить **один** G0-N и **один** G0-E25/E26 на самом важном целевом домене, прежде новой недельной архитектурной фазы. Дальше работа только по измеренной причинной цепочке.
4. **Ревью:** Architect проверяет назначенного recipient и доменный закон, QA проверяет одинаковость входов/denominator, Planning reviewer — одну переменную, Prompt reviewer — явный output, Human Communication — формулировку о том, что именно обучалось.
5. **Good:** «В C# kNN 8/8 HELDOUT после уникализации IDs, 5/8 до; same model/corpus/seed; native logs …; old GOOD had same implementation, regression arose from caller mapping». **Bad:** «E25 исторически 87.5% и значит RP01 вернул качество; все green tests». Числа в Good — **формат гипотетического примера**, не полученный результат.
