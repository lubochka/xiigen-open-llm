# Предобновляющая метрика, послеобучающая оценка и реальный consumer остановки

**Уточнение host по bcf880:** для каждого указанного ID код находится в `35.ExperimentsV2/Experiments/Rp01Micro/<ID>/`, host — `experiments-v2/RP01-MICRO/<ID>/Runner.csproj` и `Program.cs`. Собирать только этот host, затем вызывать его DLL; не ссылаться на весь ExperimentsV2.csproj, не использовать общий dispatcher/reflection. Не добавлять warmup, повторы или новые уроки сверх бюджета; прежние эксперименты, outputs и bin/obj сохранять без перезаписи. Один раз зафиксированные source/card hashes не требуют бесконечного перечитывания из-за чужих Markdown-правок. [Финальная дельта](05-CONCURRENT-PUBLICATION-AND-EXPERIMENT-COPIES.md#5-последнее-уточнение-путей-и-версий-bcf880).

Дата: 20 октября 2026. Core: `bf9b543e149ef197da85a767821d98c9b67916d2`. Planning: `1ad622fe1f6d1e8b0860265d8c208aeada3fefb0`. Статус: source-backed уточнение **уже известного** planning54, покрытие экспериментов требует дополнения. Нативные updates этим аудитом не запускались; source и protected каталог не изменялись.

**Текущая работа — только документы, `NOT_RUN`; подготовка опыта — `PREPARABLE`.** По [уточнению о точной экспериментальной копии](https://github.com/lubochka/llm_mvp_core/blob/a8222d1b15745fe6207f36cd294fd3fb61c01d3b/docs/plans/replanning-possible-diffs/experimets/cards/P01-trainable-recipient.md#L5-L17) будущий A/B выполняется в `35.ExperimentsV2/Experiments/Rp01Micro/<ID>/`; малый runner — `experiments-v2/RP01-MICRO/E-METRIC-PHASE/`. Копируются необходимые исходники и реальные зависимости, сохраняются source commit/path/hash, hashes копий и A/B diff. Настоящие learner, gradients и optimizer исполняются в экспериментальном проекте; surrogate недопустим. Основной код/продуктовые тесты не меняются; только сборка выбранного experiment project, без full CI/Main500/S-слайсов.

Original paths в этом документе показывают provenance и будущий адрес возможного переноса, а не разрешение править продукт. Перенос после измерений требует отдельного решения пользователя. Текущий прямой запрет сохраняет `docs/plans/replanning-possible-diffs` read-only даже для результатов; иной results-путь в исходной карточке его не отменяет. Отчёт/raw outputs будущего опыта — в разрешённый `possibleDifs` либо пользователю. Сейчас не создаётся и не исполняется runner. Сначала выполняется готовая работа по реальным блокерам обучения и уже принятым изменениям обучаемой памяти; phase-monitor не является обязательным gate перед ней.

## 1. Что именно доказано

Метрика, возвращённая последним `StepVector`, не является независимым измерением состояния после этого шага. Она получена на forward до optimizer. Это нормальная полезная метрика самого training-step; проблема возникает только если её назвать результатом оценки сохранённой обновлённой модели.

В текущем источнике порядок однозначен:

```csharp
float margin = chosenReward - rejectedReward;
var objective = ObjectiveTerms(batch, margin);
float loss = objective.Loss;
// backward обоих ответов
lossSum += loss * (IsExact(batch) ? scaledWeight : vectorWeight);
// после всех пар
ApplyOptimizer();
_policy.ZeroGrad();
var metrics = new DpoMetrics { Step = step, Loss = lossSum, /* ... */ };
publication.Publish(_policy, metrics, BuildPublicationBundle(step));
```

[Точное вычисление, строки891–914][trainer-pre]; [optimizer → metrics → publication, строки917–941][trainer-post]. Этот фрагмент сокращает исходник только для показа порядка, не является предлагаемым новым trainer.

У consumer есть две разные роли:

| Место | Что фактически потребляет | Следствие |
|---|---|---|
| `LogicalDpoTrainingUnit`,1378–1389 | `lastMetrics = trainer.StepVector(...)`; затем `monitor.ShouldStop(lastMetrics.Loss)` | Loss действительно влияет на число следующих optimizer updates, когда convergence window достижим. Здесь возможен реальный learning A/B. |
| `LogicalDpoTrainingResult`,1421–1426 | `lastMetrics?.Loss` и `lastMetrics?.Accuracy` | Итоговые `LastLoss/LastAccuracy` имеют ту же фазу pre-update. Их переименование не меняет обучение само по себе. |
| Quality promotion, рассмотренный прежним planning54 | Отдельная trial calibration | Ошибочное принятие checkpoint только по этим loss/accuracy не установлено. Нельзя объявлять такой дефект. |

[Loop и monitor][unit-loop]; [поля результата][unit-result]; [прежний автор и оговорка про calibration][prior54].

## 2. Почему просто вызвать EvaluateMany недостаточно

Для exact-v2 `StepVector` возвращает:

```text
w_i = max(0.01, Weight_i) / sum_j max(0.01, Weight_j)
s_i = clamp(1 + activeTeachingSlots_i / 60, 1, 2)
m_i = beta * ((policyChosen_i - referenceChosen_i)
            - (policyRejected_i - referenceRejected_i))
loss_vector = sum_i w_i * s_i * ObjectiveTerms(batch_i, m_i).Loss
reward_margin_vector = sum_i w_i * m_i
accuracy_vector = sum_i w_i * 1[m_i > 0]
```

Точный denominator в оригинале дополнительно ограничен снизу `1e-6`; для непустого конечного набора положительных effective weights это не изменяет формулу. `activeTeachingSlots` в текущем коде — число координат с `abs(value)>1e-6`, а не семантическая величина координат. [Weight и scaling][vector-weight]; [накопление][trainer-pre]; [VectorSignal][vector-signal].

`EvaluateMany` принимает `DpoBatch`, теряя `TeachingVector`/вес, и возвращает равномерное среднее `ObjectiveTerms(...).Loss`. Даже при равных pair weights оно не совпадает с exact-vector objective, если `s_i≠1`. [Исходник767–807][eval-many]. Нельзя вычитать before из `EvaluateMany` и after из `StepVector`, называя результат изменением одной функции качества.

Другая ось — dropout. Policy training forward использует `training:true`, evaluation — `training:false`. При dropout0 на том же состоянии и inputs можно сопоставить одинаковую детерминированную функцию. При dropout>0 paired inference-evaluation остаётся допустимым полезным показателем до/после, но не равна конкретному stochastic training forward. Нельзя скрывать эту разницу. [Policy/eval forward1007–1029][forward]; [Dropout56][dropout].

## 3. Исправление экспериментальной копии и будущий адрес переноса

Это спецификация изменения точной копии в выбранном `34.Experiments`/`35.ExperimentsV2`; API ниже пока не реализован. Original source и product tests остаются read-only. Сначала A/B проверяет изменение на копиях с одинаковыми настоящими зависимостями; пользователю передаются фактический результат и кандидат переноса в указанные original owners. Сам результат не разрешает этот перенос автоматически.

1. В экспериментальной копии `11.DPO/DpoTrainer.cs` добавить общий для A/B метод оценки, условное имя `EvaluateVectorObjective`, принимающий `IReadOnlyList<DpoVectorBatch>`. Сохранять exact material validation/capture и reference identity. Вычислять те же `TargetMask`, `ComputeTargetLogProbs`, `ObjectiveTerms`, normalized pair weights и `VectorSignal` в том же порядке, что actual step. Не переизобретать вторую формулу DPO и не менять его gradient. Общая instrumentation-копия и единственный A/B factor diff записываются отдельно.
2. Оценка не вызывает `Backward`, `ApplyOptimizer` или `ZeroGrad`, не меняет `RequiresGrad`, не сбрасывает и не продвигает RNG. `training:false` отключает dropout, но само по себе не доказывает отсутствие всех внутренних autograd allocations; их стоимость честно входит в probe timing. Не называть эту функцию “бесплатной”.
3. Сохранить значение прежнего `DpoMetrics.Loss` как метрику выполненного training forward. Добавить явную фазу измерения либо отдельный typed observation: `phase=pre_update_training_forward`, policy optimizer step перед update, committed step/checkpoint после него, objective version. Число `DpoMetrics.Step` сейчас пользовательский publication step; оно не всегда равно глобальному AdamW step после continuation, поэтому брать последний из настоящего optimizer state.
4. В отчёте хранить отдельные before/after оценки одним и тем же новым evaluator, привязанные к точным material/vector/weight set, reference, checkpoint и `training:false`. After вычислять только после успешного commit шага. Не подменять неуспешную/частичную попытку сохранённым after.
5. В B-копии `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` текущий `monitor.ShouldStop(lastMetrics.Loss)` заменить входом post-probe; A-копия передаёт прежнюю pre-update loss. Оба экспериментальных результата сохраняют оба наблюдения с разными именами. Это единственный фактор сравнения. Original scheduler и публичный продуктовый результат не меняются; возможный перенос отдельно решает пользователь после измерений.

Метод должен получать те же exact пары, которые действительно участвовали в данном update. Отдельная heldout-панель также нужна для предметной пользы, но она не заменяет train-objective measurement. Для after-score без риска вмешаться в train mutable state можно оценить уже сохранённый checkpoint отдельной копией модели; тогда load/copy стоимость показывается отдельно и включается в общий расход этого режима. Если используется live recipient, его ordinary validation и сериализация доступа должны сохраняться; независимое `EvaluateMany` не должно одновременно читать изменяемые tensors. Точка владельца текущей step-транзакции — [`ExecuteWithPublication`1228–1245][publication].

Первый ремонт metadata допустим как часть корректной отчётности настоящего учебного опыта. **Отдельный benchmark переименования поля не входит в очередь learning experiments.**

## 4. Маленький опыт E-METRIC-PHASE: измерение реально управляет обучением

**Статус:** `NOT_RUN`; подготовка `PREPARABLE`. Actual model/tokenizer/checkpoint bytes, точный admitted lesson и полноценный bounded paired caller ещё не материализованы в этом документном аудите. Это задачи подготовки исполнителя, не общий запрет карточки и не требование нового личного допуска. `NEEDS_INPUT` назначается лишь конкретному недоступному входу после адресной попытки его получить. Не указана вымышленная команда, которая будто уже умеет этот режим. Existing raw scientific CLI одношаговый и имеет свой 12×5 exam; этот эксперимент не меняет его существующий протокол.

**Место и границы этой карточки:** необходимые pinned source-файлы и настоящие зависимости копируются в `35.ExperimentsV2/Experiments/Rp01Micro/<ID>/`; runner — `experiments-v2/RP01-MICRO/E-METRIC-PHASE/`. Сохранить commit/path/hash исходников, hashes копий и отдельно общий instrumentation diff/единственный A/B phase diff. Настоящий copied learner исполняет forward/backward и меняет реальные LoRA/AdamW; surrogate или ручная модель loss не подходит. Production/test код не менять, full CI/Main500/S-слайсы не запускать, собирать только выбранный экспериментальный проект. Original paths — provenance и будущий адрес переноса по отдельному решению пользователя после результата. Protected папка read-only включая outputs; отчёт/raw сохраняются в разрешённом `possibleDifs` либо передаются пользователю. Текущий проход остаётся документным.

**Ограничение product server route:** два run IDs на тех же pair identities и canonical model/version не создают два допустимых продуктовых attempts: [reservation journal](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.cs#L91-L144) отвергает sibling reuse до trainer entry. Это не общий blocker для разрешённого experimental-copy caller. Малый runner вызывает настоящую скопированную учебную цепь с прозрачными experiment/source/state bindings; он не выдаёт себя за два production attempts. Исходный journal остаётся неизменным; identities не подделываются, reservations не удаляются, искусственный model/root для обхода не создаётся. Новое личное одобрение каждой версии не требуется.

**Тема обучения:** C# документация. **Задача:** научиться точно объяснять два коротких независимых закона программы. **Learner:** настоящий exact-v2 DPO ProjectLoRA recipient; его A/B tensors, AdamW moments/step и RNG. **Изменяемый фактор:** фазовое состояние policy, чьё loss получает уже существующий convergence monitor.

### Обязательные фиксированные входы

До запуска материализовать один небольшой accepted lesson из двух независимых source families и четыре закрытых новых вопроса. Обе стороны DPO должны происходить из разрешённого существующего authoring процесса и пройти независимого examiner; этот текст не даёт исполнителю права придумать provenance. Bytes выбранных inputs/outputs, masks, teaching vectors и pair weights фиксируются один раз и используются одинаково в A/B. Экзамен не передаётся в DPO.

Стартовые policy/ref/tokenizer/optimizer/RNG — одинаковые полные snapshots выбранного native profile. Reference неизменен. Для изоляции временной фазы требуется dropout0 в обоих arms; нельзя менять исходный ненулевой dropout только в B. Если подходящего сохранённого profile нет, этот подслучай ещё не готов. Обоим arms заранее задать `MaxStepCount=5`, `ConvergenceWindowSteps=1`, threshold=.002, одинаковый учебный порядок, `Beta`, LR, LoRA rank/targets/alpha и warm/cold state. Бюджет максимум 10 завершённых updates на два arms, без автоматического расширения после неудачного результата. Это именно tiny native LogicalDpo path; ограничение 5 не переносится на все XIIGEN learners.

### A и B

| Фаза | A: текущий consumer | B: post-update consumer |
|---|---|---|
| A0/B0 | Одни и те же четыре heldout вопроса; одинаковый weighted deterministic train probe | То же |
| Каждый optimizer step | Настоящий `StepVector`, затем одинаковый post-probe; monitor получает возвращённый pre-update `lastMetrics.Loss` | Тот же update и тот же post-probe; monitor получает weighted after-loss |
| Остановка | Условия monitor threshold/window/max общие | Общие; отличается только вход loss |
| A1/B1 | Повтор той же панели; cold checkpoint readback того же обученного состояния | То же |

Оба arms считают одинаковый post-probe, чтобы влияние дополнительных evaluation forwards не было вторым скрытым фактором. Текущая реализация monitor требует первого baseline loss и двух последовательных стабильных изменений; значит не объявлять stopping ранее третьего наблюдения его обычным поведением. [Monitor16–39][monitor].

Не нужно добиваться различия stop count подбором threshold после результатов. Если pre/post сдвиг не изменил stop решения, по этому уроку learning benefit не показан. Если stop отличается, возможны как экономия updates/времени, так и потеря дальнейшего обучения. Это и есть проверяемый эффект; никаких обещаний роста accuracy нет.

### Что измерять

- Все train-objective значения с точной фазой policy, реальные gradient norm до `ZeroGrad`, post-update A/B tensors и AdamW step; число успешно завершённых updates.
- Полное время A/B, load/setup, train forward/backward/optimizer, paired probe, checkpoint/write/readback. Не складывать вложенные phase timings повторно.
- Предметные outputs четырёх новых вопросов на A0/A1/B0/B1; число верных законов, critical errors, correct/all, correct/answered, abstention/coverage, частично верные пункты и длину.
- Полная память процесса, checkpoint/optimizer bytes, число trainable tensors/elements и unchanged model architecture. Структура Unit/Flow отдельно от числа Transformer layers.
- Для каждого полезного quality показателя: `A1−A0`, `B1−B0`, затем разность этих изменений. Для времени сравнить одинаково очерченные законченные учебные workload; если B закончил меньше updates, показать именно эту причинную экономию вместе с итоговым качеством.
- Четыре heldout cases дают маленькое диагностическое наблюдение, не оценку качества всего домена; повторы одного вопроса не считать независимыми source families.

**Правило успеха карточки:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения. Более правильная подпись timestamp без изменений в реальном обучении не объявляется самостоятельной измеренной пользой обучения.

**Промпт исполнителю:** «Для порученного E-METRIC-PHASE скопируй необходимые pinned исходники и реальные зависимости в `35.ExperimentsV2/Experiments/Rp01Micro/E-METRIC-PHASE/`; малый runner — experiments-v2/RP01-MICRO/E-METRIC-PHASE/. Сохрани source commit/path/hash, hashes копий, общий instrumentation diff и единственный A/B phase diff. Исполняй настоящий exact-v2 learner, forward/backward/gradients и optimizer из одинаковых полных states; surrogate недопустим. В обеих копиях используй один weighted vector evaluator той же DPO objective; `EvaluateMany` без teaching weights не подставляй. A-monitor получает прежнюю pre-update loss, B-monitor — post-update loss. Общие max5/window1/threshold.002/dropout0, максимум 10 updates суммарно; probes одинаковы, frozen 4-case panel до/после, actual steps, gradient/Δweights, checkpoint readback, время и память сохранены. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Изменения остальных показателей, включая ухудшения, покажи рядом. Product journal не меняй, identities не подделывай и reservations не удаляй; product sibling guard не превращай в blocker для experimental-copy caller. Основной код и product tests не меняй, собирай только выбранный experiment project, без full CI/Main500/S-слайсов. Original paths — provenance и будущий адрес переноса, который возможен только по отдельному решению пользователя после фактического результата. Protected docs/plans/replanning-possible-diffs read-only даже для outputs; отчёт/raw передай пользователю или запиши в разрешённый possibleDifs. Недостающие доступные inputs готовь в рамках поручения, конкретный недоступный вход обозначь после адресной попытки, не выдумывай выполненный run Host: experiments-v2/RP01-MICRO/E-METRIC-PHASE/Runner.csproj и Program.cs; компилируй только выбранные копии и Program.cs, без ссылки на весь ExperimentsV2.csproj. После сборки вызывай DLL. Не добавляй прогревов или повторов сверх бюджета; старые experiments, результаты и bin/obj не удаляй и не перезаписывай. Чужой документный HEAD не требует перезапуска зафиксированного опыта».

## 5. Новизна и защищённое покрытие

Исходный timestamp факт уже опубликован planning54. Planning35 прямо описывает `monitor.ShouldStop(lastMetrics.Loss)`, planning38 — weighted objective как input остановки и сравнение другой нормализации. Их приоритет сохраняется. Здесь фазовый contrast сохраняет **ту же** weighted objective и teaching density; он не дублирует normalization×density factorial.

В 170 прочитанных Markdown защищённого каталога нет `LastLoss`, `LastAccuracy`, `lastMetrics` или `EvaluateMany`; близкий N60/PAD текст требует gradient доApplyOptimizer и Δweights после, но не задаёт этот фазовый монитор. Это ограниченная проверка отсутствия конкретного рецепта, не утверждение, что защищённые документы не содержат никаких before/after требований. [Полный поисковый журнал с 555 planning и 170 protected файлами](NOVELTY-EVIDENCE-2026-10-20.json).

В связанной теме стоимости reference найдено параллельное совпадение: [LC-REF-CACHE на planning fb9aadf](https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/dpo/01-reference-score-reuse.md) описывает тот же cache, независимо найденный относительно исходных 555 + 170 документов. Это одна семья, а не наша уникально новая гипотеза и не основание для двух запусков. Уточнение weighted after-probe/phase-monitor здесь сохраняет собственный измеряемый фактор и attribution prior54.

Приоритет исполнения ниже подтверждённого блокера update, восстановления успешного learner и уже принятых изменений обучаемой памяти. Этот документ прежде всего препятствует неверной интерпретации after-метрик в других учебных опытах; cache и phase-monitor не должны вытеснять готовую приоритетную работу. Прямое улучшение качества от замены monitor пока не измерено.

[trainer-pre]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L891-L914
[trainer-post]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L917-L941
[vector-weight]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L861-L873
[vector-signal]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1159-L1166
[eval-many]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L767-L807
[forward]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1007-L1029
[dropout]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Layers/Dropout.cs#L53-L59
[unit-loop]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1371-L1389
[unit-result]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1419-L1428
[publication]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1228-L1245
[monitor]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoConvergenceMonitor.cs#L16-L39
[prior54]: https://github.com/lubochka/xiigen-planning/blob/1ad622fe1f6d1e8b0860265d8c208aeada3fefb0/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/54-DESIGNTOCODE-CLEAN-RECOVERY-2026-10-19/03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md#L40

