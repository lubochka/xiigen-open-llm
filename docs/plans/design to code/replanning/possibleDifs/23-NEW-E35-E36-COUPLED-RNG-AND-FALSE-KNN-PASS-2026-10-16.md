# 23 — Новое историческое расследование: E35/E36 validation зависела от label и E35 «KNN» без обучаемого состояния

**16.10.2026 | DRAFT / DIRECT_SOURCE + SAVED_RESULTS + VERIFIED_PYTHON_RNG_PROXY / NATIVE_REPLAY_NOT_RUN / INDEPENDENT_CODEX_REVIEW_NOT_RUN.** Актуальная ветка core `codex/dpo-implementation`, pin первичных исходников `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`. **Отдельно от** [44 DIF](01-PRIORITIZED-DIFFERENCES.md), [E07/E09 matrix-kNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [historical trust E40V2](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md).

## Коротко человеку

В двух старых опытах учебные примеры создавали с `Random(42)`, а затем сортировали другим `Random(42)`. И тот и другой генератор выдавал одинаковые по индексам псевдослучайные числа: первый преобразовывал число в **номер шаблона/метку**, второй — в **ключ сортировки**. При разделении «80% train / 20% validation» validation практически превращалась в последние по номеру шаблоны, а не в случайную или независимую предметную выборку.

Хуже: в одном из этих опытов `E35DependencyDetector` сказано `Enhanced: K-NN`, но процедура принятия решения **вообще не обучает KNN**, а использует константный `IsSequential(a,b)`. Поэтому его reported `PASSED` не доказывает обучения. Это независимая от RP01 DPO/LoRA причина ошибочной исторической интерпретации.

## NEW-H23-A · P0 для исторического audit · Coupled random split отбирает метки через шаблонный индекс

### Исходный код

- [E35: `Run`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E35DependencyDetector.cs#L18-L33): `pairs=GenerateDependencyPairs(seed:42, ...)`; затем `rng=new Random(42)`; `OrderBy(_ => rng.Next())`; в validation попадают последние 20% отсортированных строк.
- [E35: `GenerateDependencyPairs`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E35DependencyDetector.cs#L74-L100): ровно 14 шаблонов, сначала 8 с `SEQUENTIAL=0`, затем 6 с `PARALLEL=1`; шаблон каждого i выбирается `new Random(42).Next(templates.Length)`. Между генерацией отдельных строк дополнительных PRNG calls нет.
- [E36: `Run`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E36FeedbackCorrector.cs#L20-L35): ровно та же пара `GenerateFeedbackExamples(seed:42)` + новый `Random(42)` в `OrderBy`.
- [E36: `GenerateFeedbackExamples`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E36FeedbackCorrector.cs#L81-L115): 16 шаблонов подряд сгруппированы по компонентам 32/33/34/35/37. Последние 6 входят в классы 35/37.

**Механизм:** один и тот же seed и одинаковое количество `Next` на строку дают одинаковый базовый PRNG draw `u_i`. `Next(m)` ставит индекс шаблона, монотонно зависящий от `u_i`; `Next()` ставит монотонный ключ сортировки. Следовательно, сортировка ключей одновременно сортирует шаблоны/labels, поэтому holdout не независим от ответа.

Это не «просто одинаковые seeds» в любых функциях: **именно одинаковые последовательности по позициям, монотонные преобразования и упорядоченные по классу шаблоны** создают label-stratification в неправильную сторону.

### Архивные результаты и независимое воспроизведение состава

| Источник | Сохранённый `val` / class | `baseline → enhanced` | Что реально видно |
| --- | --- | --- | --- |
| [E35 Tier1 JSON](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E35-DEPENDENCY-DETECTOR/results/run-2026-04-30-001.json) | 10/10 **PARALLEL** (`parallels=10`, val=10), **нет SEQUENTIAL** | 0% → 10%, `passed=true` | baseline принудительно 0 при таком holdout |
| [E35 Tier2 JSON](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E35-DEPENDENCY-DETECTOR/results/run-2026-04-30-002.json) | 20/20 **PARALLEL** (`parallels=20`, val=20), **нет SEQUENTIAL** | 0% → 30%, `passed=true` | это 30% правильных на одном классе, а не доказанная generalization |
| [E36 Tier1 JSON](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E36-FEEDBACK-CORRECTOR/results/run-2026-04-30-001.json) | 8; лишь 2/5 classes (`scaling_signal=.4`) | 0% → 25%, `passed=true` | 2/8 верных, baseline занижен несбалансированным holdout |
| [E36 Tier2 JSON](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E36-FEEDBACK-CORRECTOR/results/run-2026-05-01-002.json) | 16; лишь 2/5 classes | 0% → 0%, `passed=false` | поздний отрицательный результат не означает, что обучаемый kNN вообще бесполезен |

**Выполнен внешний Python-прокси seed 42**: воспроизведена классическая последовательность seeded `System.Random` и `Next(max)` / `Next()`, без выполнения .NET. Получены составы:
- E35 Tier1: `PARALLEL=10/10`; Tier2: `PARALLEL=20/20`.
- E36 Tier1: `DEPENDENCY(35)=4, MERGER(37)=4`; Tier2: `DEPENDENCY=11, MERGER=5`.
- Для всех строк в прокси `templateIndex_i == floor(shuffleKey_i / 2147483647 * templateCount)`, что подтверждает именно coupling. Исторические отчёты независимо подтверждают все PARALLEL для E35 и 2 категории для E36, но исходные case-level validation receipts не прочитаны — **точный native replay ещё не сделан**.

**Вывод:** эти два E35 `PASS` нельзя использовать как свидетельство правильности learned kNN или надежной dependence architecture. E36 Tier1 `PASS` — реальное историческое число на узком, label-biased holdout, не честная оценка пяти-компонентного routing.

## NEW-H23-B · P0 · E35 Enhanced на самом деле не учится, несмотря на `K-NN` в header

[E35DependencyDetector.cs:7–12](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E35DependencyDetector.cs#L7-L12) обещает `Enhanced: K-NN on predicate overlap`. Фактическое [`IsSequential` 27–36](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E35DependencyDetector.cs#L27-L36) вычисляет `FamilyCollapser.CollapseString(a/b)`, затем `aCreates && bGets || overlap>=2`. [Enhanced val loop 45–51](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E35DependencyDetector.cs#L45-L51) вызывает именно это локальное правило. Здесь **нет train-kNN set, nearest-neighbour search, model update, callback к обучаемому KNN или kNN vote**. Факт CODE_CONFIRMED: результат E35 не демонстрирует полезного изменения выученного state. Правило может быть полезно само по себе, но должно называться heuristic, а не successful training.

В отличие от него [E36:28–51](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E36FeedbackCorrector.cs#L28-L51) действительно собирает `trainK` из размеченных поправок и выбирает 3 ближайших по Jaccard-подобному `TagDist`, затем делает majority vote. Это **действительный обучаемый nearest-neighbour mapping по labels**, хотя его Tier results пока не доказывают масштабирование. Отдельная полезная нить: нынешний [generic KNN first-hit](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L216-L255) не повторяет именно этот majority-vote, а берёт первый hit. Перенос из E36 в нынешний generic KNN **НЕ установлен по Git lineage**, значит это H о потере алгоритмической детали, не доказанный refactor.

## MICRO-H23 — минимальная различающая проверка, без 500 DPO

**ID:** `MICRO-E35-E36-SPLIT-01`; **Tier:** 1; **фактор:** только split RNG. Сохранить исходные 14/16 templates, rules, label IDs, `TagDist`, KNN `k=3` и scoring. A = исторический `seed42 generation + seed42 sort`. B = independent shuffle seed 43 (только для разделения), без изменения corpus. C = заранее заданный **category/family-stratified** validation, где присутствуют SEQUENTIAL/PARALLEL (E35) и все 5 components (E36); одинаковые cases и labels для baseline и enhanced. **Четвёртая отдельная граница:** template-family-disjoint для проверки обобщения за рамки повторов всего 14/16 канонических шаблонов; нельзя смешивать эту ось с RNG-only A/B.

**L0:** native .NET replay только распределений и checkpoints case IDs, подтвердить/опровергнуть proxy. Вывести `seed, indexed template, class, split flag, digest`, counts per class. E35 дополнительно показать, что удаление train list не влияет на fixed `IsSequential`. E36 переставить train labels при том же feature corpus: prediction должен измениться, иначе есть скрытый deterministic shortcut.

**L1:** на B и C считать `balanced_accuracy, false_PARALLEL (незаконное распараллеливание), recall каждого класса, routing macro-F1, confusion, coverage, latency`. Заранее зафиксировать evaluation panel и не выбирать seed по best accuracy. Данный опыт оценивает истинность **старого сообщения PASS**, а не LoRA RP01 quality.

**Подтверждение:** coupling подтверждён, если native A воспроизводит 100% PARALLEL в E35 и 2/5 classes E36 и B/C снимает искусственное отсутствие классов. Качество нового алгоритма признавать только по matched, независимой панели; если новая accuracy хорошая — прежний механизм имеет шанс, если плохая — старый PASS был артефактом отбора. Нельзя назвать любой исход исправлением RP01 без actual ordinary-recipient trace.

## Роли, ограничение области, статус

**CODEX с моделью Astra 6 UltraCode выполняет человеческий выбор учебной задачи, законной метки и независимую оценку качества DPO-пары; внешний разрешённый open Chinese model создаёт ОБЕ стороны chosen/rejected каждой новой DPO.** Здесь новый DPO не нужен; CODEX/Astra должен оценить semantic gold, split и quality будущего опыта. Независимый review и native run в этом проходе НЕ выполнены. Исходные архивы/grades сохраняются неизменными. `SOURCE-CONFIRMED` для PRNG coupling и отсутствия learner в E35; `PROXY-CONFIRMED` для label composition; `RP01-CAUSAL-IMPACT UNKNOWN`.
