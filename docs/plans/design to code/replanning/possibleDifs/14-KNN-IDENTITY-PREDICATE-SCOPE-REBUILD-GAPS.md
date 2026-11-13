# 14 — Новые подтверждённые KNN-seams: alias ID, erased predicates, domain scope и topology RNG

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Допуск, учёт дубликатов и выдача содержимого требуют отдельных достижимых проверок.** В P104 четыре проверки сами возвращают false по названию порчи. Новый оценщик принимает реальные данные или receipt, без Corruption enum. При порче меняются данные, а правила оценки остаются прежними.

Для допуска проверить два разных случая: первые пять соседей незаконны, шестой законен; полный ключ даёт похожесть 0.50 ниже порога вызывающего 0.60, а следующий уровень содержит законного кандидата с 1.0. Для дубликатов нужны реальные псевдонимы, уже прошедшие допуск. Для содержимого — правильный выбранный ID с ошибочно подставленным телом первого соседа. Каждый случай должен дойти до обычного потребителя.

Сохраняются полный пригодный набор, logical lesson ID, область предиката, уровень поиска, отсечение и источник тела ответа. Восстановление проверяется новым объектом и фактически загруженным индексом. Два независимых Train не являются продолжением одной памяти. PairId выносится в provenance; коллизии миграции не разрешаются молча.

Основание: [POST107-аудит](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) и [проверка реального кода](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md). Исторические результаты и бюджеты сохранены; это условия будущего переноса, не новый запуск.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->

**Дата 16.10.2026 · CODE_CONFIRMED для прочитанных веток; RP01 causal impact NOT_MEASURED; никакого нового model training.**
**Source:** `llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5` (ветка `codex/dpo-implementation`). Не повторяет уже установленные `top1-only`, `fixed k`, `confidence warning` и `hashed bag` из [04](04-KNN-HYBRID-RETRIEVAL.md), [05](05-SEMANTIC-VECTOR-KNN.md), [10](10-INDEPENDENT-CODE-PATH-RECONCILIATION-2026-10-16.md).

## DIF-14.01 · P1 · ExampleId collisions разрывают соответствие «вектор → обучающий output»
[Train, строки 136–153](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L153):
```csharp
var trimmedId = example.ExampleId.Trim();
exampleMap[trimmedId] = example with { ExampleId = trimmedId };
index.Add(Vectorize(...), trimmedId);
```
[HnswIndex.Add](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/06.VectorSearch/HnswIndex.cs#L35-L55) сохраняет **каждую** пару `_vectors/_ids`. Однако словарь `exampleMap` перезаписывает записи с тем же `trimmedId`. [ExecuteAsync, строки 230–244](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L230-L244) получает `bestExample = _examples[best.id]`. Если примеры `("A", input=x, output=X)` и `(" A ", input=y, output=Y)`, индекс удерживает два вектора под одним ID `"A"`, а словарь — только второй output `Y`. Даже hit вектора первого примера может вернуть `Y`. `Manifest.TrainingExampleCount = exampleMap.Count` скрывает несовпадение с `index.Count`. **Это математически/кодово доказанное условное искажение**, даже без выполнения кода; наличие таких дублей в реальных данных UNKNOWN.

**Минимальный фальсификатор:** только два обучающих примера, несовпадающие `Input/Output`, одинаковый ID после `Trim`; измерить `index.Count` vs manifest and exact query output, compare with pair of distinct IDs. Предпочтительная исправленная инварианта: запретить неуникальный canonical `ExampleId` *до* индексации или согласованно перестроить индекс после dedupe; не перезаписывать label одной строки для векторов другой. Убедиться, что id не заменяется по случайному порядку.

## DIF-14.02 · P1 · Predicate feature silently disappears at metadata type boundary
[ExecuteAsync, строки 220–227](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L220-L227):
```csharp
request.Metadata.TryGetValue("predicate_ids", out var value) &&
value is string[] predicateArray
   ? NormalizePredicates(predicateArray)
   : new HashSet<string>(StringComparer.Ordinal)
```
Вызов [LogicalUnitRequest<TInput>](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/00.Core/LogicalContracts.cs#L61-L71) принимает `IReadOnlyDictionary<string,object>`. Тот же набор значений, переданный как `List<string>`, `IReadOnlyList<string>`-объект иной реализации или `JsonElement` после JSON-deserialization, молча превращается в **пустой predicate tier**. В обучении `example.PredicateIds` передаются в `Vectorize`, таким образом train/query feature parity может нарушиться **без предупреждения и без exception**. Это условный bug, если callers передают не `string[]`; фактический caller UNKNOWN. 

**Falsifier:** один `text`, одни predicate значения в пяти формах metadata: `string[]`, `List<string>`, `IReadOnlyList<string>` с иным backing type, JSON array, missing. Сравнить сериализованный query vector/nearest IDs и лог отклонения схемы. Безопасное решение: типизированный DTO и валидатор, либо явный parser и reject недопустимого, без silent empty fallback.

## DIF-14.03 · P0 conditional · Domain-scoped retrieval flag не является фильтром именно в этом Unit
[LogicalExecutionFlags](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/00.Core/LogicalContracts.cs#L16-L54) имеет `UseDomainScopedRetrieval = true` по умолчанию. Однако конкретный [KnnLogicalUnit.Train](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L103-L165) индексирует **все** переданные `examples` без отбора по `example.DomainTags`; [ExecuteAsync](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L216-L257) ищет `_index.Search(query, _k)` без проверки `request.DomainTag`, `request.Flags.UseDomainScopedRetrieval` либо законов, а возвращаемый `DomainTags` является метаданными результата, **не pre-filter**. [HnswIndex.Search](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/06.VectorSearch/HnswIndex.cs#L79-L95) принимает `(query,topK)` без разрешенного domain filter.
**Доказан локальный дефект ограничений этого класса, НЕ доказана утечка между tenants в product**: upstream может обеспечивать отдельный индекс на каждый домен/проект и lawful selection.

**Falsifier:** создать смешанный training pool из различных domain/tenant/law tags, один идентичный query, выполнить direct Unit request с иным `DomainTag` и флагом `UseDomainScopedRetrieval=true`; затем **отдельно** проследить реальные upstream constructors, есть ли pre-scoped index. Если сосед другого домена попадает в Unit output, считать локальную инварианту нарушенной; если upstream гарантирует отдельный pool, обозначить защиту на уровне orchestrator и обеспечить её end-to-end. Исправление — permission/law/domain pool **до** top-k, не `filter(hits)` после truncation, и совместимый tokenizer/schema версии на каждом пуле.

## DIF-14.04 · P2 · HNSW rebuild не воспроизводим по фиксированному input
[HnswIndex](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/06.VectorSearch/HnswIndex.cs#L23-L41) использует `private static readonly Random _rng = new();`; [RandomLevel](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/06.VectorSearch/HnswIndex.cs#L138-L147) consumes RNG для уровней. Фабрика [KnnLogicalUnit.Train](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L140-L153) не передает seed. Поэтому rebuild при одинаковом корпусе **может** дать иной граф ANN/ближайший hit, независимо от изменений обучающего алгоритма. Сохранённый `HnswIndex.Save/Load` сериализует граф и сохраняет его структуру; это **не** проблема replay уже сохранённого индекса, только rebuild и branch-level scientific comparability.

**Falsifier:** N rebuild одного frozen corpus и упорядоченных inputs, каждый сравнить с exact BruteForceIndex (уже есть в проекте 06), метрики hit@1/recall@k/correctness/graph hash; затем фиксировать индекс seed/order и повторить. Если results совпадают, не приписывать KNN регрессию стохастике.

## Таблица входных гипотез, приёмка и ограничения
| ID | Что доказано | Что неизвестно | Одно различающее наблюдение |
|---|---|---|---|
| 14.01 | dictionary/index расходятся при canonical duplicate ID | есть ли такие ID в реальном training | 2 input→2 vector→1 label reproducer |
| 14.02 | metadata для non-`string[]` сводится к пустому predicate set | передаёт ли так actual caller | parity of five typed metadata encodings |
| 14.03 | local class не применяет domain flag/DomainTags к admission/search | гарантирована ли доменная изоляция upstream | mixed-domain query before/after pool prefilter |
| 14.04 | index build uses unseeded shared RNG | влияет ли на actual topk | exact vs ANN repeated-build comparison |

**Распределение роли и оценки:** `DPO → learned recipient → typed Unit` не может считаться успешно реализованным по наличию ближайших соседей. Лицензированная внешняя открытая китайская модель генерирует обе стороны новых DPO. CODEX под GPT-6 Astra UltraCode выбирает учебную задачу как human-role и отдельно оценивает качество **именно той пары**, чьи байты будут допущены. Предметный tester оценивает реальный ordinary consumer; статические guards не являются собственной «моделью Astra». 
**Независимый CODEX review / empirical KNN runs:** NOT_PERFORMED. Следующий этап — минимальные причинные пробные inputs, а не очередной Windows CI марафон.
