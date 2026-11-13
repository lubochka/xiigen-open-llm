# 13. N62-R1: адресный разбор четвёртого сохранённого эксперимента

Дата проверки: 2026-10-20. Статус: `READBACK_ONLY`; новых сборок, вызовов модели, updates, генераций и повторов — **0**. Рассмотрен только новый результат `N62-R1-01`, добавленный в core [`883496bca18113bf37e0afe683c75b05a0075048`](https://github.com/lubochka/llm_mvp_core/commit/883496bca18113bf37e0afe683c75b05a0075048), поверх предыдущего снимка `0b76…`. Raw обоих плеч записывают `core_commit=62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`: это указанная самим экспериментом версия запуска; `883496…` — снимок хранения и текущего адресного source review. Весь runtime-набор из 101 файла заново не сверялся.

**Результат полезен по измеренным ресурсным показателям и уже зарегистрирован как кандидат.** Есть настоящий DPO/LoRA TRAIN, одинаковый наблюдаемый учебный результат A/B и несколько сниженных счётчиков; одновременно все три счётчика GC выросли на один. Кандидат ещё не перенесён в оригинальный `DpoTrainer`. Завершённый №04 повторять или дополнять новым TRAIN-панелем нельзя: обновлённый индекс назначает **№05 `N62-R2`**, с отдельным фактором и своим начальным состоянием. Источники: [raw A](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/A/arm-result.json), [raw B](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/B/arm-result.json), [новый индекс](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md).

## Реальный учебный путь и сравнимость

В каждом плече выполнены ровно два `DpoTrainer.StepVector`, в порядке `D-T1`, `D-T2`, batch 1; всего четыре native updates. `Program` вызывает `N62ExperimentBridge.Run`, а его TRAIN-loop непосредственно вызывает `StepVector` и проектный LoRA AdamW. A/B имеют одинаковые исходные policy/reference/optimizer hashes и material receipts. Все 16 adapter arrays изменились в каждом плече; ни один из 39 base arrays не изменился, reference остался прежним. Финальный policy hash A/B одинаков: `7676e8b10d68495fff490bf9d2fefaa69e18abee9f08bb3c0b7dd9d91dc6ae48`. Native readback сообщает optimizer step 2, равенство live/fresh weights и optimizer state, успешное открытие random state. [Program](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/experiments-v2/RP01-MICRO/N62-R1/Program.cs), [Bridge: исходное состояние, TRAIN и readback](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/Rp01Micro/N62-R1/Common/ContinuousLearning/N62ExperimentBridge.cs#L180-L266).

Дополнительно совпадают шесть пар Git blob ID сохранённых optimizer/policy/random-state файлов на шагах 1 и 2. Это независимое подтверждение совпадения сохранённых Git-объектов; binary/LFS payload не скачивался, hashes весов заново не вычислялись. Наблюдения подтверждают эквивалентность этого двухшагового результата, а не всех возможных обучений. Полная арифметика, hashes и 23 source receipts находятся в [машиночитаемом readback](evidence/latest-n62-r1-readback.json).

## До/после обучения и A/B — разные сравнения

В обоих плечах сохранены одинаковые online metrics:

| Шаг / материал | Loss | Reward margin | Accuracy |
|---|---:|---:|---:|
| 1 / D-T1 | 0.91264373 | 0 | 0 |
| 2 / D-T2 | 0.9189987 | −0.009629821 | 0 |

Эти значения вычислены по forward **до текущего optimizer update**, а опубликованы после него. Поэтому margin второй строки не является оценкой финального состояния после двух updates. Между строками также изменился урок: разницу loss нельзя назвать ухудшением обучения на общей панели. Общая TRAIN-fit панель до/после, финальное качество и EXAM в этом результате **не измерены**; их дельты из сохранённых данных не вычисляются. Изменение весов и совпадение A/B подтверждены, улучшение точности не заявляется и не требуется для признания измеренного ресурсного успеха. [Порядок forward → metrics → optimizer → публикация](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/Rp01Micro/N62-R1/A/DpoTrainer.cs#L883-L940).

## Ресурсы: наблюдаемые улучшения и ухудшения рядом

Дельта в таблице — `B−A`, процент — `100×(B−A)/A`. Все числа получены из уже сохранённых A/B; выборка — одна последовательная пара A→B.

| Показатель и область | A | B | B−A | Изменение |
|---|---:|---:|---:|---:|
| Managed allocations исполняющего потока host TRAIN-loop, bytes | 3 764 969 984 | 3 764 879 480 | **−90 504** | **−0.002404%** |
| Host TRAIN-loop, wall ms | 40 977.3226 | 24 298.8544 | −16 678.4682 | −40.7017% |
| Process CPU за host TRAIN-loop, ms | 32 578.125 | 22 140.625 | −10 437.5 | −32.0384% |
| Peak working set процесса к завершению TRAIN, bytes | 996 343 808 | 962 547 712 | −33 796 096 | −3.3920% |
| Сумма двух `applyOptimizer`, **wall** ms | 1 873.2654 | 378.6029 | −1 494.6625 | −79.7891% |
| GC Gen0 / Gen1 / Gen2 за TRAIN-loop | 188 / 12 / 9 | 189 / 13 / 10 | **+1 / +1 / +1** | **Рост всех трёх счётчиков** |

Allocation counter — **`GC.GetAllocatedBytesForCurrentThread`**, а не `GetTotalAllocatedBytes`. Его интервал находится внутри синхронного `void Run`, без `await`: это корректно обозначенный счётчик исполняющего потока, исключающий allocations других потоков. Он не измеряет общие allocations процесса или retained RAM. Окно включает `StepVector`, окружающие `ExportOptimizerState` и запись issued/completed JSON; финальный canonical readback выполняется после него. Peak working set — процессный максимум, включающий историю до TRAIN, а не чистая память learner. [Точные границы и счётчики](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/Rp01Micro/N62-R1/Common/ContinuousLearning/N62ExperimentBridge.cs#L192-L241).

`applyOptimizer` измерен через Stopwatch и включает `RequireValid → AdamW.Step → RequireValid`; другие вызовы `RequireValid` находятся снаружи этой фазы. Сам guard отдельно не измерен. Более короткие reference/policy/backward фазы также видны в raw: один порядок A→B не разделяет эффект фактора, разогрев и условия запуска. Наблюдаемое уменьшение времени остаётся успехом по показателю; устойчивость эффекта и объяснение всей разницы не доказаны. [Фаза и внешние проверки](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/Rp01Micro/N62-R1/A/DpoTrainer.cs#L1219-L1243), [wall timing](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/Rp01Micro/N62-R1/Common/DPO/DpoStepVectorTiming.cs).

## Добавочное уточнение существующих report/candidate

Исходные [report](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/report.md) и [кандидат в required-changes](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/required-changes.md#L85-L96) сохраняются без изменений. При принятии решения использовать следующие уточнения:

1. Общее утверждение об отсутствии выигрыша по памяти заменить точным небольшим снижением current-thread allocations и отдельным process-peak наблюдением. Нельзя скрывать полезный результат из-за его размера.
2. Общее утверждение об отсутствии ухудшений заменить явными `Gen0 +1`, `Gen1 +1`, `Gen2 +1`.
3. Phase speedup около 4.95× относится к wall interval, а не изолированной CPU-работе guard. Независимость от порядка, доля guard в фазе и объяснение остального выигрыша разогревом не доказаны.
4. «Margin после двух updates» заменить «online margin D-T2 до второго update». Не достраивать отсутствующее качество финальной модели и не повторять завершённый эксперимент ради этой панели.
5. Сохранить статус полезного кандидата с ограниченной выборкой. Полный перенос в product, CI и исправление тестов требует отдельного решения пользователя.

Единственный source factor B — замена frozen-base LINQ bit comparison на `MemoryMarshal.AsBytes(...).SequenceEqual(...)`, с прежним fallback для слишком длинного span; guard, отрицание, остальные проверки и AdamW сохранены. [B: изменённая проверка и helper](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/35.ExperimentsV2/Experiments/Rp01Micro/N62-R1/B/DpoTrainer.cs#L248-L284). Текущий [оригинал `11.DPO/DpoTrainer.cs`](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L248-L273) ещё использует LINQ; его Git blob совпадает с A (`0a7fd4d088fd37824aa213ddf0c6189675fe7dc2`). Кандидат уже записан, повторно создавать конкурирующее исправление не нужно.

**Передача:** `N62-R1=COMPLETED`; `next_id=N62-R2` по строке №05. Не возвращаться к R1, не переписывать старые попытки и не переносить его optimizer state в следующий фактор. При STOP — немедленно остановиться; при явном NEXT — сразу перейти к назначенному ID без продолжения предыдущего отчёта. Для одиночного задания readback результатом является этот документ; дополнительных runtime-действий он не назначает.
