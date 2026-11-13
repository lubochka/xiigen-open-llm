# Что действительно работало — и что НЕ следует переносить вслепую
## Измерения из исходных сохранённых JSON, 1 мая 2026

**ВАЖНОЕ УТОЧНЕНИЕ ПОСЛЕ ПРОВЕРКИ КОДА:** E09 baseline и E09-KNN сравнивали **разные validation subsets** (`E09StructuralKnn.Run:50–77`: baseline на всех `val`, kNN только `val.Take(500)`). E34v2 baseline считался на **всех** `val`, тогда как hybrid только на второй половине; alpha подбирался на первой половине (`E34HybridMatcher.Run:32–57`). Следовательно указанные ниже приросты — **записанные показатели, НЕ доказанное парное причинное улучшение**. В таблице значения сохранены без ретуши, но E09/E34 имеют статус `NONMATCHED_COMPARISON`. Источники: [E09StructuralKnn.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L50-L77); [E34HybridMatcher.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E34HybridMatcher.cs#L32-L57). Требуется повторная same-panel recomputation. E17 и E43 также не доказывают генерацию Student по определению других целевых метрик.
| Опыт | Сохранённые показатели | Эффект | Механизм/объём | Источник | Ограничение переноса в RP01 |
| --- | --- | --- | --- | --- | --- |
| E09 Structural kNN | top1 0.2413→0.3507 | +10.94 п.п. | AST structural vectors; k=5; 4492 train pairs | [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json#L1-L25) | Plausible для feature-based retrieval, не proof генерации |
| E17 Logical predicates | perplexity 190.1389→5.5964 | -97.06% perplexity | 12 AST predicates, 583 symbols, vocab10 | [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E17-LOGICAL-PREDICATES/results/run-2026-05-01-001.json) | Упрощённая bigram perplexity; не model fluency |
| E34v2 BM25+KNN | hit@1 .7222→.7778 | +5.56 п.п. | train72 val18, alpha .10, 106.7ms vs10.5ms | [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E34V2-HYBRID-MATCHER/results/run-2026-05-01-001.json) | fusion retrieval, price ~10×; not neural DPO |
| E43 Replay | retention .3226→.3978 | +7.52 п.п. | 177 replay pairs 70/30, bigram | [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) | Retention partial; not RP01's LoRA |
| E14 Context-action KNN | accuracy .8824→.8676 | -1.48 п.п. | severe class imbalance; KNN worse majority | [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-001.json#L1-L20) | Counterexample first-neighbor not universally beneficial |
| E29v2 pair weights | accuracy .2308→.2308 | 0 п.п. | repeat chosen TrainStep, NOT DPO objective | [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E29V2-DPO-PAIR-VALUE/results/run-2026-05-01-001.json) | Don't transfer chosen×2 as evidence DPO quality |
| E31v2 ordered loop | accuracy .2807→.2807 | 0 п.п. | domain-sorted ProbeNeuralNetwork updates | [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E31V2-CONTINUOUS-LOOP/results/run-2026-05-01-001.json) | Curriculum may not help |
| E44v2 security×10 | weighted F1 .1663→.0039 | -97.67% | fixed x10 weight; imbalance | [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E44V2-DATASET-WEIGHTER/results/run-2026-05-01-001.json) | D-09 rejects fixed 10× as general rule |
| E45v2 hard-last | accuracy .3623→.3478 | -1.45 п.п. | curriculum ProbeNN | [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E45V2-CURRICULUM-ORGANIZER/results/run-2026-05-01-001.json) | Negative control for ordering |
| E48 warm domain | final .3036→.3036 | 0 п.п. final; N56 14%→34% | warm source training vs random cold | [run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E48-DOMAIN-BOOTSTRAPPER/results/run-2026-05-01-001.json) | Early sample-efficiency hypothesis, not final gain |

**Важно:** E17 снижает perplexity на сильно сокращённом предикатном алфавите, E09 использует C# AST features и поиск соседей, E34 использует BM25+Jaccard, E43 использует bigram replay. Они не проверяли способность случайного Transformer RP01 освоить русский или C# после DPO. Положительный результат другой функции нельзя выдавать за регрессию идентичного алгоритма.

## Исторические решения из первичной родительской папки
[DECISIONS-LOCKED.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/llm%20dynamic%20flow/DECISIONS-LOCKED.md#L1-L21) фиксирует:
- **D-03**: пять уровней semantic compression; сравнить сохранение source laws, не просто размер vocab.
- **D-04**: пять intent/domain route labels `CODE_Q/BUG/PLAN/ARCH/REFACTOR`; не путать их с C#/ENRU/SQL предметными доменами.
- **D-05**: дешёвые LoRA adapters **per project**; RP01 trainable q/v Student не доказывает маршрутизации по проекту.
- **D-06**: replay 70% new /30% old из E43; присутствие старого правила не означает, что текущий RP01 trainer его вызывает.
- **D-07**: StackDetector до normalization; R04 отмечает, что этот порядок уже есть в некотором pipeline — не фиксировать как действующий дефект без trace.
- **D-08**: StratifiedSplit для экспериментов; RP01 25/20 семейств требуют **family-disjoint**, простое стратифицирование строк недостаточно.
- **D-09**: dynamic max_weight, а не механическое ×10. Негатив E44 подтверждает риск.
- **D-10**: BM25+KNN rank fusion; `ReplayFusionRanker` существует в core, но его вызов из `CodeDocumentationSimulationDpoExperiment.Run` не наблюдался.
- **D-13**: promotion не только из-за метрики; также проверить depth/coverage.
- **D-14**: internal-first/external-gated + independent assessment; содержимое исторического внешнего teacher не приписывать RP01 simulation.
- **D-15**: арбитр решает приемлемость, kNN — только память. Дополнительный первичный контекст: [ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md#L1-L113) (positive/negative/warning neighbors, pre/post arbiter). Это **конкретный вероятный функциональный пропуск** при переносе в RP01, но не доказанная причина 0/4 free translations.

## Практическая таблица соответствия
| Исторический способ | Непосредственный RP01 аналог | Статус |
| --- | --- | --- |
| AST predicate / structure | Transformer token/LoRA route; generic TeachingVector scalar | Не эквивалентны |
| BM25+KNN | отдельный ReplayFusionRanker в core | Прямой вызов из RP01 НЕ ДОКАЗАН |
| Negative/warning neighbor | kNN first-hit + evidence | Из этого Unit полноценный arbiter pipeline НЕ ДОКАЗАН |
| 70/30 ReplayBuffer | ENRU продолжает C50; training на новых ENRU парах | В данной выбранной training loop replay НЕ ПОКАЗАН |
| Warm-start | C50→ENRU C100 | Уже существует технически, **но C50 не владел полезной C# компетенцией** |
| DPO-like pair weight E29v2 | actual log-prob DPO с ref в trainer | **Разные алгоритмы**, нельзя сравнивать их числовые метрики напрямую |

## Что перепроверить в старых RAG
[09.09.26WorkLeft-RAG-MASTER-INDEX.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RAG-MASTER-INDEX.md#L1-L25), [09.09.26WorkLeft-RAG-01-dpo-legal-build.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RAG-01-dpo-legal-build.md#L1-L45), [FINDINGS.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/replanning/results/R03/FINDINGS.md#L25-L70), [FINDINGS.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/replanning/results/R04/FINDINGS.md#L40-L85). Исторические RAG являются поисковой картой; статус каждого эксперимента берётся из самого JSON, текущий статус реализации — из кода/нового runtime.
