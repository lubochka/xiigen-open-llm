# N35-MICRO-CVG-01 — 1 фактор, не больше 5 обновлений: loss plateau против реального обучения

<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Актуализация доказательств 01.11.2026 — P78/RF10

P7801 исполнил window1/max5 и window5/max5:3 против5 updates,3 против1 monitor calls, quality2→2 оба. Current windowSteps определяет cadence в LogicalDpoTrainingUnit и счётчик monitor, не moving average. Call counts reconstructed по losses, не live hook. Pure loss sequence/max=window controls и явный compatibility contract предшествуют новой quality гипотезе; старое NOT_RUN ниже историческое.

Основание: [2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md](2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md), закреплённый core `790228c256168a336b01abae4387e28391048c0c`. Старый текст и его ограничения относятся к указанной там дате; новый блок уточняет текущий статус, не переисполняет исторический опыт и не меняет научную очередь.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->

**16.10.2026 | EXECUTOR WORK ORDER / NOT_RUN | подход: Architecture → Planning → Code Examination → Plan Review → Prompt Engineering → Human Communication.** Источник гипотезы: [N35-CVG](35-NEW-GENERATED-CAPABILITY-DPO-LOSS-PLATEAU-STOP-2026-10-16.md). Этот протокол относится **только** к `DefaultLogicalCapabilityTrainer → LogicalDpoTrainingUnit`, не к RP01 `CodeDocumentationSimulationDpoExperiment`.

## 1. Ответ на человеческом языке

Один и тот же «ученик» получает одну и ту же короткую пачку уже допустимых уроков. В A текущая конфигурация может остановить его на третьем обновлении, когда тренировочный loss меняется мало. В B обучаем ещё максимум два обновления, без новых уроков и без изменения модели. Проверяем **правильность новых запросов** и сохраняет ли ученик прежние полезные ответы. Так будет понятно, приводит ли «плато loss» к реальному недоучиванию. Предсказание не гарантирует успех B: дополнительные шаги также могут ухудшить качество.

## 2. Факт → гипотеза → минимальная переменная

В `DefaultLogicalCapabilityTrainer` два доступных metadata-настроечных поля: [`dpo.max_steps` и `dpo.convergence_window_steps`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L3296-L3312). `LogicalDpoTrainingUnit` [выполняет `StepVector` до проверки stop](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1343-L1390). `max=5` легально для этого caller ([limit](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L584-L589)).

| Параметр | A / control | B / intervention | Что удерживаем |
|---|---|---|---|
| metadata `dpo.max_steps` | 5 | 5 | ровно один и тот же предел |
| metadata `dpo.convergence_window_steps` | **1** (current default) | **5** | **единственная меняемая настройка** |
| effective stop threshold | 0.002 | 0.002 | не менять |
| DPO preference rows | одинаковые source IDs, chosen/rejected hashes и их порядок | те же | **не писать синтетические финальные DPO-пары** |
| policy / reference / tokenizer | одинаковые pinned SHA и initialization | независимые копии тех же артефактов | frozen reference и BPE identity |
| LoRA, seed, learning rate, sequence/context | одинаково | одинаково | без изменения ранга и функций |
| evaluation | одна закрытая source-family-disjoint law-panel | точно та же | scorer не получает train answers до окончания inference |
| consumer | тот же зарегистрированный `generated-capability-flow` после cold reopen | точно тот же | нет скрытого KNN/heuristic substitution |

**Почему B работает как контроль:** с `windowSteps=5` и `maxSteps=5` monitor впервые вызывается на последнем шаге и по [реальному методу](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/11.DPO/DpoConvergenceMonitor.cs#L23-L39) не может остановить раньше. Это штатное управление расписанием проверки, а не патч train-loss. A может также дойти до 5, тогда гипотеза о разнице STOP не проявилась.

### Publication / once-only safety: два допустимых исследовательских режима

**Основной A/B выше — только изолированный и предварительно разрешённый research harness.** Его A и B работают на двух независимых теневых копиях начального состояния, не публикуют learned state в canonical runtime, не обновляют production `pair_id applied-once` journal и не выдаются за два новых урока. При политике, запрещающей повторное optimizer-применение тех же pair bytes даже в изолированных shadow копиях, этот A/B **НЕ ЗАПУСКАТЬ** без отдельного разрешения владельца.

**Более дешёвый fallback: один пятишаговый training run.** Применить одну разрешённую фиксированную пачку только один раз с `windowSteps=5,maxSteps=5`; сохранить неизменяемые *shadow* snapshots после шагов 3 и 5, не публикуя промежуточный; выполнить оба состояния в двух свежих read-only inference consumer clones на одной закрытой панели. Из той же записанной траектории loss проверить, удовлетворял ли шаг 3 текущему предикату `DpoConvergenceMonitor` при `windowSteps=1`. Это даёт условный **prefix-checkpoint counterfactual**, а не непосредственный A-versus-B scheduler race: `StepVector` и stochastic RNG должны быть зафиксированы, и score на момент step3 нельзя показывать train-time optimizer. Качество after-step3 vs after-step5 всё равно оценивается настоящими ordinary answers. Если захват промежуточного checkpoint требует патча product или меняет stochastic path — `UNSUPPORTED_NO_VALID_CONTRAST` и остановка, а не скрытая модификация модели.

## 3. Доменные минимумы: отдельно, не смешивать score

**Пилот 1 — `logical-routing / graph / DSL` (предпочтительно).** Использовать до 12 уже допущенных DPO preference pairs об одном реально обучаемом routing law; 6 DEV (только для проверки пригодности эксперимента) + **12 закрытых heldout** из других сценариев/flow families, в том числе 4 явных negative-law и 4 ранее выученных retention cases. Ни один heldout не включать в replay или training. Primary: `law_correct/12` в обычном `generated-capability-flow`; дополнительно invalid route/false acceptance, old-law regressions, refuse-on-unknown и time/cost.

**Пилот 2 — C# / design-to-code (только если фактический caller использует этот generated-capability trainer).** Отдельные 8–12 обучающих допущенных пар на AST/operator law и **12 source-file-disjoint** ordinary queries. Primary: независимо проверенный correct law + синтаксически/семантически допустимый output (не exact строка). Детерминистический компилятор/проверка исполнения — инструмент оценки, не заменитель trained memory. 4 неизменных старых кейса на retention.

**Пилот 3 — EN→RU/HE→EN (опционально, только если в текущем продукте реально такой caller).** 12 unseen пар по polarity, role/order, agreement; независимый judge оценивает смысл и отрицание. Нельзя молча подменять `TranslationMachine.TrainStructuredBatchAsync` на этот DPO и объявлять его победу победой KNN. Если реального generated-capability caller для языка нет, **SKIP_NOT_APPLICABLE**.

## 4. Пять действий исполнителя (прямой быстрый запуск)

1. **Preflight source/recipient.** Зафиксировать `core SHA`, `planning SHA`, branch, `unit_id`, `flow_id`, actual DPO caller and admission bundle, модель, teacher terms/provenance и frozen train/heldout SHA. Если исполняется RP01 neural direct Student, остановить этот конкретный протокол как **WRONG_RECIPIENT**. Провести read-only dry-route; не менять core/protected docs.
2. **Материал.** Выбрать один маленький набор уже ранее допустимых chosen/rejected пар. Если новых final-DPO пар нет, не притворяться, что synthetic fixture является ими. При необходимости новые *обе* стороны генерирует только допустимая внешняя открытая китайская модель после проверки licence/terms; CODEX/Astra 6 UltraCode исполняет human lesson-selector и независимо проверяет контраст. Не передавать gold heldout генератору обучающих меток.
3. **A/B:** запустить две fresh независимые, равные версии policy/ref/tokenizer и той же train-batch; A: metadata `dpo.max_steps=5; dpo.convergence_window_steps=1`; B: `=5; =5`. Логи: `StepVector step, loss, chosen/rejected margin, weight delta, gradient finite, checkpoint hash, stepsTaken, stop reason`. A/B hashes после каждого **общего** шага должны совпадать при детерминированной среде; если расходятся раньше, stop не изолирован.
4. **Actual consumer evaluation.** После сохранения и **свежей загрузки** сравнить тот же `generated-capability-flow` на неизменных 12 heldout + 4 retention. Слепая оценка без показа A/B ярлыков. CODEX/Astra 6 UltraCode как независимый reviewer оценивает реальные ответы (не автоматически принимает авторские golden fixtures). Указаны отдельно обычный ответ, выбранный recipient, исключённая memory/heuristic-fallback ветвь, abstention, confidence, case-level reason.
5. **Report decision.** Разница `steps(A)<5, steps(B)=5` без семантической разницы означает **EARLY_STOP_CONFIRMED_BUT_QUALITY_GAIN_NOT_FOUND**. `steps(A)=steps(B)=5` = **NO_STOP_CONTRAST**. Улучшение B на ≥2 из 12 новых законных запросов без регрессии >0 из 4 retention и без нового ложного PASS = **PROMISING_LOCAL_SIGNAL**, а не статистическое доказательство. Худшее качество B = **OVERTRAINING_RISK**; не увеличивать шаги массово. Любое вмешательство в DPO labels/weights по пути = **INVALID_EXPERIMENT**.

## 5. Mandatory machine-readable return packet

```json
{
  "experiment_id":"N35-MICRO-CVG-01",
  "status":"NOT_RUN",
  "core_sha":"TO_CAPTURE",
  "actual_recipient":"generated-capability",
  "domain":"routing|code|translation",
  "training_pair_ids_sha256":"TO_CAPTURE",
  "teacher_authorization":"EXISTING_APPROVED_OR_NEW_LICENSE_VERIFIED",
  "baseline":{"window_steps":1,"max_steps":5,"actual_steps":null,"losses":[],"policy_sha":null},
  "intervention":{"window_steps":5,"max_steps":5,"actual_steps":null,"losses":[],"policy_sha":null},
  "heldout":{"source_family_disjoint":true,"case_count":12,"correct_A":null,"correct_B":null,"wins":[],"losses":[]},
  "retention":{"case_count":4,"correct_A":null,"correct_B":null},
  "cold_consumer_covered":false,
  "independent_judge_called":false,
  "decision":"NOT_RUN"
}
```

**Human gate:** если A не останавливается раньше B, не продолжать оптимизировать `threshold` на blind panel; сначала выбрать иной механизм из [N35 gap map](35-RP01-ALL-RECIPIENTS-CAUSAL-PRIORITY-MAP-2026-10-16.md). Если A останавливается, но B не исправляет *смысл*, не писать «обучение восстановлено». Никаких Windows CI как предварительного условия исследовательского A/B.
