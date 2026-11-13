# N37-D — E42v2: сохранённые признаки ≠ сохранённые знания

**Дата:** 2026-10-16. **Статус:** SOURCE_CONFIRMED + HISTORICAL_RUN_READBACK + MATHEMATICAL_NEGATIVE_CONTROL; NATIVE_CAUSAL_QUALITY_NOT_RUN. Core `codex/dpo-implementation@793d7b755b23aac90445da3ac6661b1bff7e6672`. Защищённый `llm_mvp_core/docs/plans/replanning-possible-diffs/**` не менялся.

## Ключевая логическая ошибка

В E42v2 заявленная метрика `knowledge_retention` — на деле **доля присутствующих где-нибудь 12 типов предикатов**. Из памяти можно удалить каждую правильную формулировку, контекст, актуальную версию закона и обученный ответ, заменить их неверными знаниями с теми же 12 маркерами — score останется 100%. Поэтому историческое `retention=1` у обоих алгоритмов не доказывает, что сохраняются знания, которые KNN, Graph или Unit способны применить.

Это не доказанная первопричина RP01 neural DPO и не повод немедленно переписывать memory eviction. Но это **рисковый мост «артефакты выжили → знания сохранились»**, которого нет в сохранённом эксперименте.

## Точное source evidence

- [E42DiskManagerV2.cs:8–14](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E42DiskManagerV2.cs#L8-L14): E42v1 0 gain объясняется распределением данных; E42v2 обещает retention при stratified incoming.
- [E42v2:20–40](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E42DiskManagerV2.cs#L20-L40): `DataChunk` содержит `Predicates`, `Domain`, `TrustScore`, `AgeDays`, `SizeKb`; **нет** `source→chosen correct rule`, `target/response`, `training_unit_identity` или версионированного model checkpoint в данном экспериментальном DTO.
- [E42v2:41–85](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E42DiskManagerV2.cs#L41-L85): baseline FIFO / enhanced value-based, но primary computed `ComputeCoverage(store)/ComputeCoverage(allChunks)`, а secondary лишь разница числа различных доменов. Нет cold retrieval или correct-law consumer.
- [E42v2:94–107](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E42DiskManagerV2.cs#L94-L107): `allPreds=chunks.SelectMany(c => c.Predicates).ToHashSet()`, `return allPreds.Count/12.0`; `ComputeValue` считает «diversity» только по признакам, не по полезному закону.
- [E42v2:110–125](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E42DiskManagerV2.cs#L110-L125): synthetic trust зависит от domain (`domain0` высокий по конструкции); это потенциал смешения domain и trust при изучении retention.
- [Архивный E42v2 Tier1, 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/E42V2-DISK-MANAGER/results/run-2026-05-01-001.json): `baseRetention=100%`, `enhRetention=100%`; **domain coverage baseline 5/5, enhanced 3/5** (`scaling_signal=-0.4`), `passed=false`, `next_action=TUNE_OR_ABANDON`. Это прямое свидетельство, что «100% retention» сосуществует с потерей разнообразия доменов в том же эксперименте.

## Мини-контрпример (чистая логика)

Пусть словарь marker ID состоит из `{0,1}`. Хранилище A сохраняет корректные пары `(0→right_law_A)`, `(1→right_law_B)`. Хранилище B сохраняет неверные `(0→wrong_law_A)`, `(1→wrong_law_B)`. Обе памяти имеют `predicate_set={0,1}`, поэтому `retention=1` по E42-подобному coverage, хотя `correctness@1(A)=1`, `correctness@1(B)=0`. Это строго доказывает *недостаточность proxy*, **не** что этот контрпример наблюдался в RP01.

## Гипотеза с конкурирующими объяснениями

- **H37-D1:** value-based eviction может сохранять типы признаков и одновременно выкидывать редкие правильные corrections и source-specific Units; E42 retention покажет 1.0 при падении реального correct-law@1. Особенно при конфликте одинаковых predicate IDs, но разном domain/source/version.
- **H37-D2:** true beneficial eviction может быть незаметна по saturating predicate coverage, даже когда улучшает verified memory recall; прежний `FAIL` не закрывает policy.
- **H0:** при реальных source-family балансах обе eviction политики одинаково хорошо сохраняют полезные law bindings, либо enhanced вредна; документирует отрицательный результат, не проталкивает value-based default.
- **Предусловие:** сначала доказать, что конкретный learned KNN/Graph/Unit действительно хранится в memory owner, на который влияет этот eviction mechanism. E42 самостоятельно **не отвечает** за RP01 Student checkpoint.

## Быстрый N37-E42 (память и доменные законы; без 500 DPO)

**Данные:** 20–40 уже размеченных, разрешённых «уроков» с `sourceFamilyId`, `domain`, `version`, `lawId`, `positive/negative`, `chosen/rejected` если у урока есть DPO происхождение. Сконструировать 4–8 конфликтных marker-equal corrections (SQL `LEFT JOIN WHERE` vs `ON`; C# nullability + async await; EN→RU negation/role/order только для direction-tagged units) и 6–12 независимых SEALED запросов. CODEX/Astra 6 UltraCode независимо утверждает gold/score, а DPO pairs, если создаются новые, авторит **внешняя открытая китайская модель**, обе стороны.

**A:** FIFO / current eviction, capacity=10, same insertion order. **B:** value-based eviction, capacity=10, same input/source/hash. **C:** domain+law-aware policy (дополнительная гипотеза; не смешивать с B). **D:** negative control: замена target на wrong law при сохранении marker sets.

**Primary:** paired `correct_retained_law@1` **после** cold restore и ordinary Unit consumer, макро по доменам; дополнительно corrected-vs-obsolete, rare law recall, false-confidence, abstain, memory size/latency. Secondary E42 predicate coverage **как диагностический proxy**, а не gate успеха. Важно отдельное `old-law retention` после новых уроков и обратное `new correction visibility`.

**Правило интерпретации:** B хуже A на law accuracy при marker coverage 1 ⇒ истинная потеря знаний скрывалась proxy. B лучше A ⇒ policy потенциально полезна для этого домена, при review всех ошибок. A=B ⇒ нет подтверждённого эффекта. Прежде чем перенести в продукт, нужен per-Unit recipient + checkpoint/journal + model SHA readback. `NATIVE` и quality judge **NOT_RUN**.

## Novelty/dedup

Смежные имеющиеся: [KNN correction visibility](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md), [memory index / state](30-NEW-HNSW-FAILED-ADD-PARTIAL-MEMORY-2026-10-16.md), [replay boundary](34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md). Это **другие seams**. Новый адресный N37-D: **конкретная E42v2 metric/DTO/archived domain-coverage contradiction**. Полная уникальность в 292 документах не подтверждена.