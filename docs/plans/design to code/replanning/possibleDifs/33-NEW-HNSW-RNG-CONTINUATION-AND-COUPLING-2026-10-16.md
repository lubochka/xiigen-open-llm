# N33-01 — HNSW после рестарта: незафиксированная случайность уровней и межиндексное вмешательство

**16.10.2026 | SOURCE-CONFIRMED STATE SEAM / QUALITY HYPOTHESIS / NATIVE NOT RUN / REVIEW NOT RUN**

Репозиторий: llm_mvp_core @ **32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7**, код в 06.VectorSearch/HnswIndex.cs. Это **другой** механизм, чем уже описанные [неатомарный failed Add](30-NEW-HNSW-FAILED-ADD-PARTIAL-MEMORY-2026-10-16.md), [reverse-prune reachability](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md), [alias/NaN](30-NEW-VECTOR-ALIAS-AND-NONFINITE-KNN-INGRESS-2026-10-16.md) и [shape mismatch](23-INDEPENDENT-AUDIT-SHAPE-DECODER-AND-LEARNING-2026-10-16.md). Полную мировую уникальность находки не заявляем.

## Что происходит

Представим, что мы выучили 300 правильных пар, сохранили HNSW и выключили программу. После холодного восстановления добавили ещё 50 тех же уроков, в том же порядке. Кажется, индекс должен оказаться в одной и той же форме при одном и том же initial checkpoint. Но уровень каждого добавляемого узла определяется случайным генератором, а состояние этого генератора в checkpoint индекса **не записывается**. Более того, этот генератор **общий для всех HNSW экземпляров**. Вставки в несвязанный индекс другого домена могут изменить уровни при вставке в наш.

Таким образом, restored historical lessons в файле присутствуют, но последующие вставки способны построить иную ANN-топологию и изменить качество отбора learned corrections **без изменения DPO, LoRA, encoder, текстов и labels**. Это пока прогноз/гипотеза влияния на качество, НЕ наблюдавшийся пользовательский дефект.

## Прямая исходная цепь

- [HnswIndex.cs:24–44](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/06.VectorSearch/HnswIndex.cs#L24-L44): HNSW хранит параметры m, ef, vector/id/graph, объявляет **private static readonly Random _rng = new()**. Static => один источник случайности на все экземпляры в процессе.
- [Add:49–97](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/06.VectorSearch/HnswIndex.cs#L49-L97): каждый Add вызывает RandomLevel(), затем использует полученный уровень для списка связей и выбора entry point.
- [RandomLevel:156–161](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/06.VectorSearch/HnswIndex.cs#L156-L161): считывает _rng.NextDouble() до прекращения цикла, число вызовов меняется от сгенерированного уровня.
- [Save/Load:115–151](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/06.VectorSearch/HnswIndex.cs#L115-L151): persist только Vectors, Ids, Meta, Graph, EntryPoint, MaxLevel, M, EfConstruction, EfSearch; Random seed/consumed cursor/state среди полей нет. Load восстанавливает граф, не состояние _rng.
- [Search:99–110, 164–222](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/06.VectorSearch/HnswIndex.cs#L99-L110): hit зависит от entry-point, уровня и связей графа; exact results не гарантируются при ограниченном efSearch.

**C / proven**: shared RNG + random graph-levels + non-persisted RNG state. **H / unproven**: конкретное расхождение финальных ответов после рестарта. Само Save→Load **без последующего Add** здесь не обвиняется: сериализуется существующий граф, а не пересоздаётся. И даже разные графы могут возвращать тот же hit при достаточной плотности, поэтому изменение sha НЕ равно ухудшению качества.

## Быстрый причинный опыт H33-RNG

**Подготовка:** фиксированный список из 64–256 двухмерных/трёхмерных float[] (все одинаковой размерности, finite), уникальные stable ID, 12–20 source-disjoint запросов с известным CORRECT vs WRONG lesson label. Один из доменов: C# structural vectors; другой контроль — лингвистический embedding index без AST. Приоритет на query disambiguation, где wrong answer может выбрать сосед другого law.

**L0, без изменения production:** создать HNSW с m=2 или 4, efConstruction=8, efSearch=2–4 (сильнее выявляет topological effects), обучить первые N примеров и сохранить base.json. Проводить две независимые холодные continuation A и B от **байтово одного и того же** base.json. Перед одинаковыми next M Add в B выполнить N чужих Add к совершенно отдельному HNSW; в A этого нет. Сохранить обе serialized graph и сравнить graph signature, topK ID, correctness и recall@K против exact BruteForceIndex с **той же правильной размерностью**. Все query/labels/insert-order исходного индекса неизменны. Три повтора допустимы только если первый не дал результата; каждый запуск сохранить отдельно. До поправки потребитель обязан реально вызывать этот HNSW; otherwise source-only.

**L1, remediation conditional:** isolated per-index seeded generator + persist RNG state/cursor (либо иной устойчиво воспроизводимый stream) в *версионированном* HNSW checkpoint; одинаковые inputs и seed. Два fresh restores должны дать одинаковую последовательность levels и новые graph fingerprints. Затем измерить paired ordinary consumer correctness, не считать graph hash достаточной метрикой. Не менять encoder/k/threshold/labels одновременно.

**Предсказания:** A/B graph может различаться; при unlucky ANN topK один и тот же урок не извлекается в B или наоборот. Если уровни/graphs отличаются, а hit и final law correctness не меняются, verdict **STATE_NONDETERMINISM_WITH_NO_MEASURED_QUALITY_GAIN**. Если граф и результат одинаковы при нескольких фиксированных стресс-вставках, verdict **NOT_REPRODUCED**, не "bug resolved". Если реальный RP01 only Student и не использует HNSW, verdict **NOT_APPLICABLE_TO_RP01_STUDENT**.

## Воспроизводимость и границы

Риск существует только при **последующих Add/continual learning** и backend HNSW. Нет оснований ожидать эффект от изменения LR/LoRA. Режим default m=16 имеет другую частоту верхних уровней, поэтому опыт m=2–4 — механическая stress проверка, а финальный user-domain effect нужно отдельно подтвердить default config. Бюджет: index-only в памяти, не Windows CI и не DPO 500; один native short run + короткий counterfactual. Расширенный протокол [N33 MICRO](33-MICRO-CAUSAL-EXECUTOR-AND-EXPERIMENT-2026-10-16.md).

**Роли для создания нового DPO, если позже нужен парный опыт:** CODEX/Astra 6 UltraCode = человеческий case selector; внешняя открытая китайская модель = BOTH final chosen/rejected; независимый CODEX/Astra 6 UltraCode = качество пары и ordinary response. Никакие такие вызовы в этом source audit НЕ выполнялись.
