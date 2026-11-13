# N54 — независимый source-аудит скрытых потерь признаков и ложной обученной памяти в Logical KNN

<!-- POST113-CURRENT-2026-11-02:START -->
## Сверка после P113 — actual input и полные координаты

Для native KnnLogicalUnit на 21007e25 подтверждены прежний string[]-only ingress и общая L2; экспериментальные B из P108/P113 не встроены в ordinary Unit. P108 raw carrier signal имеет неполную историческую приёмку; P113 mean predicate energy fraction 0,00123718→0,50000004 при exam4/4 у обоих. Рост точности между разными опытами из этого не выводится.

Приёмка связывает consumed predicate IDs с raw input после собственной NormalizePredicates; выбранные output IDs не подставляются вместо входных. R06 проверяет finite компоненты/доли по независимому expected: перестановка при прежней норме или fraction 0,5→0,9 обязана отклоняться. Если исторический tokenizer/raw halves не сохранены, новый Build не переаттестовывает их. Нормировка train/query/restore имеет общую policy version. Остальные исторические N54 seams ниже этим узким проходом заново не подтверждены.

Основание: [сводка этого аудита](codex-2026-11-02-POST113-TRANSFER-AND-EXPERIMENT-AUDIT.md), [core 134](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md), [план 135](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md). Core `21007e25ec3f6f6634dd8733fc8631ae267e518c`, planning `5fc34f3c19a12d46b7e48707aea89c28b6ec32a0`, 2 ноября 2026. Нижний текст сохраняет свой исторический срез; он не получает новую нативную приёмку и не переопределяет нынешний контракт. Новые Train/inference/PowerShell/.NET этим аудитом не исполнялись; данные, дозы и очередь сохранены.
<!-- POST113-CURRENT-2026-11-02:END -->

**Дата:** 19.10.2026 · **Тип:** NEW NARROW SOURCE HYPOTHESES · **GOLD-диапазон:** `DesignToCode-clean` → `codex/dpo-implementation`.  
**Проверено:** точные методы и GitHub blob SHA; Python source-equivalent арифметика для int overflow. **Не проверено:** native .NET вызов на действующем consumer, реальное падение качества RP01, независимая полная дедупликация всех сотен карточек остальных агентов.  
**Принцип:** кодовый факт ≠ вызывающий код передал проблемный вход ≠ наблюдённая ошибка студента.

## N54-01 — условная потеря доменного predicate tier при несовпадении типа metadata (NEW конкретный seam)

**Источник:** [KnnLogicalUnit.cs L102–153: train](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L102-L153), [L201–225: inference](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L201-L225), [L265–300: vectorization](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L265-L300).

1. **Во время обучения** берётся `example.PredicateIds` (`string[]`) и записывается во вторую половину вектора с прибавкой **+2 на hashed slot**.
2. **Во время inference** код обращается к `request.Metadata["predicate_ids"]` и принимает значения **только если runtime тип `string[]`**; любое другое значение (`List<string>`, `IReadOnlyList<string>`, `JsonElement` и пр.) трактуется как **пустое множество**, а не ошибка. Запрос идёт в kNN уже без правил/структурных признаков.
3. Реальный [LogicalMetadataSnapshot.cs L229–320](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/LogicalMetadataSnapshot.cs#L229-L320) пропускает `string[]`, но может **отклонять незнакомые reference types** раньше. Поэтому **при прямом unit caller** канал реально беззвучно исчезнет, **при snapshot-orchestrator caller** тот же пример может закончиться явной ошибкой. Оба исхода проверять, не обещать silent loss во всех маршрутах.

**Триггер:** actual caller передал предикаты как не-`string[]` и unit выполнялся напрямую (или metadata дошли без fail-close). **Эффект гипотетический:** homograph/operator/negation cases начинают использовать текстовую близость и игнорируют выученный закон. **Сфера:** Language POS / C# / SQL / Orchestration, но эффект зависит от вызывающего Flow, не универсальное правило. **Не утверждать:** объясняет neural RP01 0/3 / 0/4.

**Граничные случаи:** null metadata; `IReadOnlyList<string>`; `List<string>`; `JsonElement`; `string[]` с разным регистром; одинаковый текст, разные `predicate_ids`; пустые string[]. Метод `NormalizePredicates` правильно приводит к uppercase при корректном типе, поэтому ввод только другой упаковки — однофакторная проверка.

## N54-02 — коллизии ID оставляют старые векторы в индексе, но переадресуют их на новый OUTPUT (NEW конкретный seam)

**Источник:** [HnswIndex.Add L51–58: принимает каждый vector/id без проверки уникальности](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/06.VectorSearch/HnswIndex.cs#L51-L58), [HnswIndex.Search L99–110: отдаёт ID по сохранённому индексу](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/06.VectorSearch/HnswIndex.cs#L99-L110), [KnnLogicalUnit.cs L125–155](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L125-L155) и [L221–253](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L221-L253).

Код проверяет только `!string.IsNullOrWhiteSpace(example.ExampleId)`, но **не** проверяет уникальность `ExampleId.Trim()`. Затем:
```csharp
exampleMap[trimmedId] = example with { ExampleId = trimmedId };
index.Add(Vectorize(...example.Input...), trimmedId);
```
При повторном ID исходный вектор остаётся в HNSW, но словарь `_examples[id]` держит **последний** `Output`; затем inference извлекает `best.id` из HNSW и `bestExample = _examples[best.id]`. Так **самый близкий ранний вектор** может вернуть **чужую позднюю метку/закон**. `Manifest.TrainingExampleCount = exampleMap.Count` скрывает отличие от числа вставок индекса, `safeK` ограничен числом входных примеров, а не уникальных ID.

**Контрпример (условный, только с разрешёнными искусственными учебными строками):**

| Вставка | `ExampleId` | `Input` | `Output` |
|---|---|---|---|
| 1 | `OP-01` | `x++` | `returns-old-value` |
| 2 | `OP-01` | `++x` | `returns-new-value` |

HNSW содержит **две** записи с `OP-01`; `_examples["OP-01"]` содержит только вторую. Если query `x++` вернёт первую запись как nearest, результат будет `returns-new-value` несмотря на совпавшую текстовую структуру. Порядок вставки меняет ошибочную метку без новой информации о мире. Это точный **aliasing механизм**; не гарантируем конкретный nearest без native проверки (зависит от vectorization/HNSW score и других соседей).

**Сфера:** C# операторов, SQL `IS NULL` vs `= NULL`, EN→RU омонимы/negation; особенно риск при авто-декомпозиции одного DPO в несколько child-unit examples и повторе parent-ID. **Нужна source lineage:** действительно ли pipeline выдаёт такие IDs — пока не доказано. Это *дополнение* к известной проблеме duplicate проверок `TranslationTrainedUnits`, **не** идентичная ей ошибка.

**Ожидаемое исправление при подтверждении:** отвергать duplicate IDs с `DUPLICATE_TRAIN_EXAMPLE_ID` до единого вызова `index.Add`, либо индексировать парой `(parentId, childOrdinal, recipientUnitId)` с проверенной уникальностью и versioning. Не удалять старые записи из индекса «по месту», если индекс не умеет delete/replace.

## N54-03 — детерминистически достижимый integer-overflow в StableHash (NEW узкий edge)

**Источник:** [KnnLogicalUnit.cs L312–321](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L312-L321):

```csharp
unchecked
{
    var hash = 23;
    foreach (var ch in value)
        hash = (hash * 31) + ch;
    return Math.Abs(hash);
}
```

**Точный искусственный uppercase predicate ID:** `ZSJBINM`. Для seed=23 и C# `unchecked int32` полиномиальный hash после семи ASCII букв равен **`0x80000000` = `Int32.MinValue`**. В .NET `Math.Abs(int.MinValue)` бросает `OverflowException`, до `% predicateTierWidth`. Проверка source-equivalent: `python 54-KNN-SOURCE-EQUIVALENT-PROXY-2026-10-19.py`; выполнимый вывод документирован в соседней JSON-квитанции.

**Условия:** реальный `PredicateIds` после `ToUpperInvariant()` содержит такую строку; во время train `Train` перехватывает Exception и возвращает `KNN_LOGICAL_UNIT_TRAINING_FAILED` ([L194–197](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L194-L197)); во время query `ExecuteAsync` может вернуть `KNN_LOGICAL_UNIT_INFERENCE_FAILED` ([L256–261](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L256-L261)). **Низкая практическая вероятность, P2/P3 robustness**, не аргумент, что произошёл массовый neural quality regression.

Ожидаемое исправление: `(uint)hash` и `% (uint)width`, либо `(int)((uint)hash % (uint)width)`; и **версия feature-vector hash**, потому что смена алгоритма адресации изменяет embeddings persisted HNSW, требует rebuild вместе с tokenizer.

## 4. Ключевая отрицательная проверка: эти seam есть и в GOLD

В прямом сравнении blob SHA [GOLD KnnLogicalUnit](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode-clean/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs) и [current KnnLogicalUnit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs) **совпадают: `2957816a88aa...`**. `LogicalMetadataSnapshot` тоже совпадает: `0dec91ca749c...`. Поэтому N54-01/02/03 **НЕ являются доказательством ухудшения между этими ветками**. Это **новые локализованные механистические гипотезы**, возможные при изменившихся runtime-данных/путях/ID; для реального regression proof необходимо показать, что в GOLD опасное состояние не возникало, а текущий consumer его создаёт или достигает.

**Тест причинности:** если один и тот же dataset+types+metadata+caller на GOLD и current возвращает **одинаково ошибочный** ответ, это inherited bug, не причина *diff качества между ветками*. Если GOLD использует другой путь сохранения, а current этот, проверять route-change и identity, а не переписывать всю kNN математику.

## 5. Точечная дедупликация и ограничения

Известные соседние гипотезы: [11A порядок kNN](11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md), [16 KNN tier weight](16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md), [16 translation duplicate/negation](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md), [37 alias signature](37-NEW-KNN-SIGNATURE-GUARD-MONOTONICITY-CONTAINMENT-AND-CASCADE-2026-10-16.md), [53 external_domains sparse refinements](53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md), [protected core 60 (READ-ONLY)](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/60-knn-signature-alias-and-candidate-budget.md). Здесь заявлена **новизна точного указанного source seam и контрпримера**, а не абсолютная новизна общих тем `duplicate`/`kNN`/`hash`. Не было полнотекстового independent reread всех ~443 planning файлов и всех protected core files.

**Немедленная проверка:** [54-MICRO-DOMAIN-CAUSAL-EXPERIMENTS-2026-10-19.md](54-MICRO-DOMAIN-CAUSAL-EXPERIMENTS-2026-10-19.md).  
**Первый продуктовый приоритет всё равно GOLD:** [DesignToCode-clean GOLD-FIRST](00-54-DESIGNTOCODE-CLEAN-GOLD-FIRST-2026-10-19.md).
