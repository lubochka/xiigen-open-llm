# E-OPT-STEP: независимая адресная проверка source на core827

**Статус:** read-only review сохранённых исходников; build/обучение/изменения старых результатов не выполнялись. Pin: `827a1acdbef95d220886e54f9d36bce0b405c8cd`. Численные receipts параллельно проверяет основной аудит; этот документ устанавливает, какой фактор выражен в source и какие измерения делает реальный bridge.

## Главный вывод

**A/B source задаёт правильный E-OPT-STEP фактор, но arm-result metadata скопировано из N62-R1. TRAIN readout до/после отсутствует.** Нельзя делать вывод «вместо EOPT фактически запущен R1» только по строкам factor/measured_path: они являются жёсткими literals старого bridge. Равным образом source review не заменяет установление historical binary custody; итоговую силу атрибуции связывать с сохранёнными source/build/dependency receipts, не с одним названием каталога.

## Что подтверждено кодом

1. [`Program:19,35–49`][program] задаёт `LessonCount=2`, experiment `E-OPT-STEP-01`, compiled arm A/B и вызывает `N62ExperimentBridge.Run`. Сам он не делает дополнительных training/evaluation.
2. [`Prepare-Opt:21–24,93–108`][prepare] выбирает ровно три variant-owned файла: `DpoTrainer.cs`, `AdamW.cs`, `AdamWPortableState.cs`. Прочитанные `Sources.DPO.props`/`Sources.TrainingLoop.props` действительно Compile-include `$(N62SourceRoot)/$(Variant)/...`; остальные источники общие.
3. Между полными A/B `DpoTrainer.cs` только добавлены `RequireAllowedMetadata`, `ReadOptimizerStepForRandomState` и заменён аргумент RNG callback. [A:1239–1243][a-dpo] вызывает `ExportOptimizerState().Step`; [B:1271–1278][b-dpo] вызывает `ReadOptimizerStepForRandomState()`. При null callback short-circuit сохраняется.
4. [B metadata reader:546–555][reader] берёт `_optimizer.ReadPortableMetadata` только для project-LoRA recipient, затем `RequireAllowedMetadata`, возвращает step; для остальных receiver оставлен прежний полный export. Public exports, rollback snapshot, publication и внешние owner exports не заменены.
5. [B AdamW:235–275][adamw] сохраняет binding/null/key/duplicate/reference/shape/length проверки исходного `ExportPortableState`, но не копирует m/v payload. Новые metadata types сохраняют format/step/count/key-length/duplicate checks. [B recipient:274–286][recipient] всё ещё вызывает полный `RequireValid`, сверяет allowed key, dims и moment lengths. Никакого удаления frozen-base validation не найдено.
6. В обоих DpoTrainer frozen comparison остаётся исходным LINQ `Select(BitConverter.SingleToInt32Bits).SequenceEqual(...)`: [B:255–261][frozen]. **R1 byte-span изменения в B нет.** Изменения source соответствуют одному metadata-copy фактору; признаков добавленного второго алгоритмического фактора в трёх A/B файлах не обнаружено.

## Откуда взялось неверное описание raw

[`Prepare-Opt:10,47`][prepare] копирует мост из **N62-R1/Common/ContinuousLearning/N62ExperimentBridge.cs**. Actual EOPT Common bridge прочитан целиком; Git blob SHA `9dbfcd8d9f9f5290ed378b5d96fb0d86c938732e`. Его [строки277–281][metadata] буквально пишут:

- A: `original element-wise LINQ bit comparison of frozen base`;
- B: `bitwise byte-span comparison of frozen base`;
- `core_commit = 62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`;
- `measured_path = 11.DPO/DpoTrainer.cs::DpoProjectLoraRecipient.RequireValid`.

Это provenance/descriptor defect. Старые raw не переписывать. В новом дополнении указать actual source factor с permalink, отдельно устаревшие literals и оставшуюся неопределённость historical binary identity. Для будущего иного назначенного опыта descriptor должен явно приходить из его fixed study/manifest; не исполнять дополнительный EOPT ради исправления названия.

## Что действительно измеряется

Bridge создаёт настоящий project-LoRA DpoTrainer с **непустыми capture/restore RNG callbacks** [168–171][bridge-trainer], поэтому заменяемый callback-аргумент достижим даже при dropout0. Он выполняет [ровно `rows.Count` вызовов StepVector:198–224][loop]; текущий Program ожидает две строки. После первого update накопленные moments существуют, второй step способен проявить фактор. Это не detached DTO benchmark.

Но [A0/B0:181–189][zero] содержат только optimizer/model/reference identity. В [конечном readback:239–267][readback] заново читаются checkpoint/optimizer/RNG и сравниваются состояния. Ни там, ни в остальном bridge нет before/after teacher-forced TRAIN NLL, target ranks или argmax; `Evaluate/Generate` отсутствуют. Таким образом, сохранённые EOPT результаты **не содержат новой TRAINv2 панели**, и final answer improvement нельзя вывести из числа изменившихся adapter arrays. Нельзя подставлять нули или достраивать историю новым прогоном.

`training metrics` от двух StepVector — online наблюдения на **разных уроках D-T1/D-T2**, не A0/A1 на одинаковой оценочной панели. Их изменение не заменяет before/after loss при неизменных inputs. Actual state writes, completed updates и successful readback по-прежнему являются данными о настоящем обучении; отсутствие score-панели не обнуляет измеренный полезный ресурсный выигрыш.

Измерительное окно [191–234][window] включает не только native `StepVector`, но и внешние `ExportOptimizerState` для budget/issued/completed, JSON receipts, checkpoint publication внутри StepVector и bookkeeping. Эти действия одинаковы в A/B. Поэтому метрика корректно описывается как **стоимость данного полного учебного цикла с сохранением результатов**, а не isolated optimizer getter latency. `GC.GetAllocatedBytesForCurrentThread` — allocation volume одного потока; process peak — процессная величина. Прямой parse raw обязан сохранять эти scopes.

## Что добавить к отчёту без переисполнения

Сохранить наблюдаемые улучшения каждой полезной метрики и ухудшения рядом; назвать успех ресурсной оси на C# code-to-documentation панели, если числа его подтверждают. Указать `TRAIN before/after NLL/rank/exact = NOT_MEASURED`, stale factor/measured_path/core_commit literals, actual source factor и раздельную уверенность source→binary. Сохранённый успешный опыт остаётся кандидатом исправления; ни product change, ни новая проверка, ни повторный EOPT не следуют автоматически из этого review.

[program]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/experiments-v2/RP01-MICRO/E-OPT-STEP/Program.cs#L19-L49
[prepare]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/experiments-v2/RP01-MICRO/E-OPT-STEP/Prepare-Opt.ps1#L10-L108
[a-dpo]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/A/DpoTrainer.cs#L1239-L1243
[b-dpo]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/DpoTrainer.cs#L1271-L1278
[reader]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/DpoTrainer.cs#L546-L555
[adamw]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/Optimizers/AdamW.cs#L235-L275
[recipient]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/DpoTrainer.cs#L274-L286
[frozen]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/DpoTrainer.cs#L255-L261
[metadata]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/Common/ContinuousLearning/N62ExperimentBridge.cs#L277-L281
[bridge-trainer]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/Common/ContinuousLearning/N62ExperimentBridge.cs#L168-L171
[loop]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/Common/ContinuousLearning/N62ExperimentBridge.cs#L198-L224
[zero]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/Common/ContinuousLearning/N62ExperimentBridge.cs#L181-L189
[readback]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/Common/ContinuousLearning/N62ExperimentBridge.cs#L239-L267
[window]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/Common/ContinuousLearning/N62ExperimentBridge.cs#L191-L234
