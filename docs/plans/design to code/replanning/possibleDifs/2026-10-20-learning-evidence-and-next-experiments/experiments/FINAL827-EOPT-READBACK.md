# Финальный снимок827: E-OPT-STEP завершён, следующий LF-03

**Окончательно зафиксирован core `827a1acdbef95d220886e54f9d36bce0b405c8cd`; более поздние HEAD не проверяются.** [Index][index] прямо назначает следующим **№07 LF-03**, №01–06 имеют сохранённые результаты и не повторяются. Объединённая программа сохраняет **38 ID: 6 завершённых /32 pending**. Это чтение сохранённых документов, не запуск.

## Сохранённое измерение E-OPT-STEP

[A][a] и [B][b] содержат actual JSON: `COMPLETED`, `error=null`, issued2/completed2 на плечо. Оба raw указывают executed `core_commit=62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`;827 — снимок публикации, не выполненная версия модели. Два урока D-T1→D-T2, batch1, настоящий DPO→project-LoRA AdamW. Семь полученных файлов проверены по Git blob SHA; **LFS pointers среди них нет**. Точные поля и пересчитанные дельты — [FINAL827-EOPT-METRICS.json](FINAL827-EOPT-METRICS.json).

| Метрика | A | B | B−A |
|---|---:|---:|---:|
| Train allocated bytes | 3 764 994 488 | 3 764 526 544 | **−467 944; −0,01242881%** |
| Train elapsed, мс | 50 068,549 | 40 665,430 | −9 403,119 |
| Train CPU, мс | 37 687,500 | 35 828,125 | −1 859,375 |
| GC0 /GC1 /GC2 | 193 /19 /13 | 189 /14 /10 | **−4 /−5 /−3** |
| Whole-process peak working set, байт | 840 916 992 | 1 045 463 040 | **+204 546 048; +24,3242% — ухудшение** |

**Полезные наблюдаемые дельты считаются успехом своей метрики и домена; роста accuracy не требуется.** Report показывает только GC0, actual raw дополнительно подтверждает уменьшение GC1 и GC2. Стоимость отдельных фаз также изменилась: backward −3 816,7838 мс, applyOptimizer −1 322,3107 мс. Вычитанием фаз получается residual −1 469,8208 мс; это не отдельный таймер getter. Один A→B и изменения нетронутых фаз не устанавливают причинную долю вмешательства. Формулировки report/кандидата «детерминированная экономия» и «не меняет обучение совсем» сильнее сохранённого наблюдения и здесь не принимаются как доказанные.

## Существенное противоречие происхождения фактора

[Study][study] и [report][report] заявляют metadata-only чтение optimizer Step вместо копии m/v. Но actual JSON содержит:

- A `factor`: `original element-wise LINQ bit comparison of frozen base`;
- B `factor`: `bitwise byte-span comparison of frozen base`;
- **оба** `measured_path`: `11.DPO/DpoTrainer.cs::DpoProjectLoraRecipient.RequireValid`.

Это описания R1, противоречащие заявленному E-OPT-STEP. По прочитанным records нельзя решить, являются ли они только оставшимися метками или затрагивают исполненный фактор. Raw не исправляются. Полезные записанные дельты сохраняются, но **их специфическая привязка к metadata-reader не установлена этим readback**. Study объявляет source-manifest SHA256 `4cb4ff6ae241d45c2f9cc65425b0f04bff854aeb0dd6d877cdc4fbf227a7fc26`; исторические binaries и source→binary цепочка здесь независимо не аттестованы. Это ограничение, не утверждение, что обучение отсутствовало.

Оба receipts записывают одинаковые A/B state/readback, 16 изменённых LoRA arrays,0 изменённых base arrays, frozen reference, optimizerStep2. Бинарные checkpoints независимо не декодировались. В report loss0.91264373/0.9189987 и margin−0.009629821 относятся к **PRE_UPDATE двух разных уроков**, не к одной before/after-панели. В actual arm JSON `learning_signal` отсутствует. TRAIN NLL/rank/exact после обновления и качество свободных ответов **не измерены**; нельзя приписать завершённому опыту поздний TRAINv2 или запустить восемь новых диагностических forward ради отчёта.

## Кандидат и адресное разрешение конфликта

[Существующий кандидат E-OPT-STEP][candidate] уже находится в `required-changes.md`: проверенный metadata-reader в `DpoTrainer`/`AdamW`, без копий m/v ради номера шага. К нему относится приведённая выше оговорка contradictory raw labels и ограничение причинности. Protected не редактируется, отдельный новый ID/повтор не назначается; решение о production остаётся за пользователем.

В [N62-карточке][card] на827 есть **устаревшие исполняемые указания E-OPT-STEP**: строки11/15, `$SelectedId='E-OPT-STEP'` в257 и runtime-промпт334, хотя index уже говорит completed. Для текущей программы они отозваны. Сохранённые E-OPT-STEP/R1/R2 не запускаются, их raw не дополняются. Следующая будущая реализация — только LF-03 по точному дополнению ниже.

## Исполняемый договор ближайшего LF-03 — только для будущего порученного запуска

Текущее поручение остаётся документным. Следующие значения разрешают конфликт рецепта заранее, без создания host или запуска сейчас.

| Поле | Назначенное значение |
|---|---|
| ID /study identity | `LF-03` / `LF-03-01`; не E-OPT-STEP |
| Source-root | `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/` |
| Host-root /project | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LF-03/` / `Runner.csproj` |
| Единственный result-root | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LF-03/results` |
| Report /candidate | В этом R: `report.md` / `candidate.md`; protected не является output |
| Один внешний entry | `powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LF-03/Run-With-Learning-Signal.ps1` |
| Experimental composition | Полные копии TrainingLoop, DPO, ContinuousLearning плюс TensorCore; native bridge `Common/ContinuousLearning/N62ExperimentBridge.cs` внутри Source-root; не product build |
| Единственный factor | В `Tensor.Backward(Tensor)` заменить только `new Tensor(initialGradient.Data.ToArray(), initialGradient.Shape)` на `new Tensor(initialGradient.Data, initialGradient.Shape)`; defensive copy TensorStorage сохранить |
| Student | Canonical saved B0 `rp01-csharp-small/v1`, прежние q/v-LoRA/config/tokenizer/RNG/AdamW из карточки; не модель исполнителя |
| TRAIN | Только исходная `simulation-csharp-doc-f06-008` из `fixtures/csharp-one-pair.jsonl`, immutable bytes; postfix increment, без subtraction-prefix |
| Весь A/B бюджет | 1 StepVector/ApplyOptimizer на arm: **2 updates всего**,4 policy TRAIN forward,4 reference forward,4 backward; отдельные **4 DIAG_TRAIN forward** |
| Диагностика | Одна chosen TRAIN-панель A0/A1/B0/B1 по тем же IDs/masks; один policy forward/state, NLL/rank/token/exact из тех же logits; StepVector return не after-readout |
| Следующий в порученной программе | Исходный №08 `N63-INGRESS-LENGTH`; при поручении одного ID — закончить после него |

Исходный SelectedId приведён в карточке как **inline PowerShell**, отдельное существующее имя файла для этого блока не задано. При **будущей materialization назначенного** `LF-03/Run-With-Learning-Signal.ps1` и его внутренней цепочки зафиксировать:

```powershell
$SelectedId = 'LF-03'
if ($SelectedId -ne 'LF-03') { throw 'Only the assigned LF-03 is allowed; completed IDs must not be rerun.' }
$TaskHostRoot = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LF-03'
$TaskResultRoot = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LF-03/results'
```

Это замена stale literals при создании experiment-only wrapper, **не правка protected-карточки или production**. Старую формулу `$TaskResultRoot=...docs/plans/replanning-possible-diffs/.../$SelectedId-01` не применять. Wrapper исполняет уже назначенную build→A→B→summary цепочку один раз; внутреннюю CLI не запускать рядом. Сохраняется один host-owned `execution.claim` и canonical owner резервирования R: не создавать generic claim, results заранее, новый attempt или второй prepare. Existing/partial files сохраняются; неполный запуск не обходится другим ID.

Модель исполнителя/reviewer — default текущей сессии; навыки и русский report заданы [общим договором](../05-executor-contract.md). Любой полезный measured gain — успех по своей оси, ухудшения рядом. **По «следующий» немедленно перейти к назначенному следующему ID в разрешённой фазе, без дописывания прежнего report/hash/review/test/commit; по «остановить всё» прекратить всю работу.** Допустимо только необходимое прекращение собственного активного процесса. Незавершённое оформление не исключение. Никаких новых запусков ради E-OPT readback.

## Источники

Git blob SHA: index `920b648d83bc625f43ba393657f5348263826032`; N62 card `cf61ffefc7e7ab4af75301a6d15dc0585e3a13d2`; EOPT report `e32210e13dfde716c067661d4a028e7a9aade5dd`; A `3cbdd20c7f18ee4e26874cc6785481f66b308a18`; B `90465760423f29dbb660bb5ca8ad295357b75e9e`; study `182db17dd573d1a5b26b16fd241bfb42a93c4754`; required-changes `5f5cce9b82fccb4a84197c88fc9c3c9d8de87057`. Полные paths/byte lengths сохранены в соседнем metric JSON. Более поздние HEAD не рассматриваются.

[index]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md
[card]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md
[report]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/report.md
[a]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/A/arm-result.json
[b]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/B/arm-result.json
[study]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/study.json
[candidate]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/required-changes.md
