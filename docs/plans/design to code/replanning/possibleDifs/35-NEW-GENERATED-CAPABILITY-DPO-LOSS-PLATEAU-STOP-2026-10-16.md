# N35-CVG — Досрочное окончание DPO по плато loss до проверки полезности обучения

**Дата:** 2026-10-16. **Режим:** независимый source-аудит + иллюстративный арифметический proxy; **native .NET, новые model updates, CODEX/Astra judge — NOT_RUN**. Репозиторий кодовых источников: `lubochka/llm_mvp_core`, ветка `codex/dpo-implementation`. Документ создан исключительно в `xiigen-planning/llm_plan3/.../replanning/possibleDifs`; protected `llm_mvp_core/docs/plans/replanning-possible-diffs/**` — READ ONLY.

## Для человека

Генерируемая обучаемая способность может сделать несколько настоящих обновлений DPO и объявить тренировочный цикл законченным только потому, что значение *train-loss* менялось мало. Но если модель ещё не научилась отвечать на новые вопросы, такое «плато» — не доказательство успеха. Это отдельный маршрут от нейронного RP01 C#500/ENRU500. **Нельзя приписывать ему RP01 0/3→0/3 и 0/4→0/4 без actual caller trace**.

### Что подтверждено исходниками

| ID | Точная ссылка на исходник | Доказуемое свойство | Не доказано |
|---|---|---|---|
| CVG-01 | [DefaultLogicalCapabilityTrainer.cs L48–52](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L48-L52), [L511–522](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L511-L522) | Default max steps = `LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount`, default convergence window = **1**. | Какую метадату дал конкретный пользовательский запуск. |
| CVG-02 | [DefaultLogicalCapabilityTrainer.cs L2253–2310](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L2253-L2310) | Параметры действительно передаются `LogicalDpoTrainingRequest`, затем `LogicalDpoTrainingUnit.ExecuteAsync`. | Не является маршрутом `CodeDocumentationSimulationDpoExperiment`. |
| CVG-03 | [LlmIngredientLogicalUnits.cs L457–480](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L457-L480), [L584–589](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L584-L589) | `ConvergenceThreshold` по умолчанию **0.002**, `MaxStepCount` разрешён **от 1 до 5**. | Граница 5 может быть осознанным safety/cost budget; отдельно не называем багом. |
| CVG-04 | [LlmIngredientLogicalUnits.cs L1343–1391](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1343-L1391) | Каждый шаг делает `trainer.StepVector(batches, step)`, а после окна вызывает `monitor.ShouldStop(lastMetrics.Loss)` и `break`. В предикате нет качества обычного ответа или новых доменных законов. | Что фактически было раннее завершение на исторических положительных либо RP01-экзаменах. |
| CVG-05 | [DpoConvergenceMonitor.cs L9–41](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/11.DPO/DpoConvergenceMonitor.cs#L9-L41) | При `abs(previousLoss-currentLoss) < threshold` два окна подряд даёт STOP; первый loss только инициализирует. | Что снижение `loss` однозначно должно продолжаться или что пять шагов всегда лучше трёх. |

**Вывод по коду:** для generated-capability с `windowSteps=1`, `maxSteps=5`, `threshold=0.002` и последовательностью loss `[0.6931, 0.6929, 0.6927, 0.6926, 0.6925]` штатный monitor прекращает цикл **после шага 3**. Контроль `threshold=0` выполнит все **5** при той же траектории, так как `abs(diff) < 0` невозможно. Это *источник-эквивалентная арифметика*, не запуск C# learner: в реальном A/B loss после шага 3 и веса могут различаться, значения нельзя фиксировать как измерение.

## Отличие от существующих N16/N23/N27/N33/N34 и граница новизны

Исходные владельцы [N33 autograd/SFT warmup](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md), [N27 replay aperture](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md), [N16 DPO gradient/vector](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md) и [RP01 diagnostic pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md) затрагивают другие границы. В адресно просмотренных сводках эта конкретная **метадата default window=1 → 2 стабильных изменения loss → STOP на шаге 3 при max5** не была отдельным проверенным контрастом. Полная уникальность относительно всех сотен файлов и параллельных агентов **не установлена**.

Этот маршрут создаёт/публикует generated capability, а RP01 simulation обучает другой DPO Student. Следовательно **CVG-01..05 — доказанный механизм возможной остановки, НЕ доказанная регрессия RP01**, и сравнение с историческими E26/KNN не является прямым аналогом.

## Причинный прогноз / конкурирующие объяснения

* **H35-CVG (conditional):** на frozen наборе, где loss уже платообразен на шагах 1–3, но domain-law-answer accuracy ещё меняется на шагах 4–5, current stop недоучивает generated-capability. `threshold=0` при том же лимите 5 может увеличить law accuracy и сохранить старые способности.
* **H0 (null):** раннего STOP нет (loss меняется сильнее порога); либо STOP есть, но до/после качество совпадает; либо более длинное обучение ухудшает перенос/забывает старое. В этих случаях причинная версия отвергается для данной конфигурации.
* **Альтернатива:** неправильные DPO labels, неверный recipient, неактивный generated flow, оторванный от checkpoint tokenizer, детерминистический fallback, слишком слабая base/LoRA. Они проверяются отдельно, не лечатся увеличением steps.

**Диагностический признак:** `StepsTaken < MaxStepCount` и одновременно `heldoutCorrect@5 > heldoutCorrect@stop` на заранее замороженных unseen source-family задачах. Просто больше изменений весов/меньше loss/лучше trainer margin — **недостаточно**.

## Домены

| Домен | Предположительная актуальность | Какие независимые вопросы |
|---|---|---|
| C# / design→code | Только generated-capability/compiled-code route с реальным таким DPO caller; не RP01 TextGenerator | 4–8 AST/операторных новых комбинаций; сохранять компилируемость и закон |
| EN→RU, RU→EN, HE→EN | Только при доказанном caller generated-capability; для `TranslationMachine` отдельный Unit scorer | 4–8 unseen polarity, порядок ролей, agreement; оценивать значение, не буквальный exact |
| Logical routing / graph / DSL | Наиболее прямой адресат generated-capability с DPO и flow-registration | 4–8 новых переходов/ветвей, law-negative controls, old-law retention |
| Domain-agnostic KNN / E26 | **Не применимо прямо**: их веса/память не обновляет данный DPO monitor | Сравнивать отдельно `KnnLogicalUnit`/E26, не ожидать эффекта смены порога |

**Следующий шаг:** [N35-MICRO-CVG-01](35-MICRO-DPO-CONVERGENCE-VS-DOMAIN-QUALITY-2026-10-16.md). Никакого массового прогона Windows CI, 500 новых DPO-пар или правки core, пока точная цепь вызовов и доменный ответ не различили A/B.

**Статус:** SOURCE_CONFIRMED_STOP_RULE; SOURCE_EQUIVALENT_NUMERICAL_PROXY; CAUSAL_DOMAIN_EFFECT_UNKNOWN; NATIVE_NOT_RUN; INDEPENDENT_REVIEW_NOT_RUN.
