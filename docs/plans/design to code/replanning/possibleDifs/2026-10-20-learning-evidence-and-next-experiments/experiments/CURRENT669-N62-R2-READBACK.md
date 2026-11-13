# Дополнение669: N62-R2 завершён, следующий E-OPT-STEP

**Текущий core-снимок — `66947785d1318f7cbbc88118f3156cdc75001e16`. N62-R2 имеет сохранённый результат; его не запускать заново. Следующий невыполненный — исходный №06 E-OPT-STEP.** Это прямо указано в [новом index][index]. В объединённой программе38 ID теперь **5 завершённых /33 pending**. Добавленный core NG01 уже есть в объединённой программе; второй NG01 не нужен. Это дополнение заменяет статус05 в прежнем снимке38/4/34, сохраняя историю его проверки.

Прочитаны R2 report, реальные A/B arm JSON, study, четыре сохранённых step-metrics JSON, существующий кандидат и обновлённый R1 report. Полученные байты проверены по Git blob SHA; **все перечисленные документы — actual content, не LFS pointers**. Бинарные веса/assembly не загружались и не аттестованы. Новых запусков, production или protected изменений не было.

## Наблюдения N62-R2

[A][a] и [B][b]: `COMPLETED`, `error=null`, issued2/completed2 на каждое плечо; по два настоящих DPO→project-LoRA AdamW updates, D-T1→D-T2, batch1. В обоих raw `core_commit=62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`: **669 — commit публикации/readback, а не версия выполненного обучения**.

Дельты ниже пересчитаны из actual JSON; времена фаз — сумма двух `step_timing` записей, до четырёх знаков после запятой. Округлённые строки report не использованы как исходные числа.

| Метрика и область | A | B | B−A |
|---|---:|---:|---:|
| Train allocated bytes | 3 764 947 024 | 3 764 634 696 | **−312 328; −0,008295681%** |
| Train elapsed, мс | 40 459,8146 | 34 658,2724 | −5 801,5422; −14,3390% |
| Train process CPU, мс | 34 593,750 | 30 546,875 | −4 046,875; −11,6983% |
| Backward phase, мс | 8 983,9543 | 7 167,7086 | −1 816,2457; −20,2166% |
| Reference forward, мс | 8 815,7486 | 8 136,2537 | −679,4949 |
| Policy forward, мс | 7 378,5903 | 6 203,8328 | −1 174,7575 |
| ApplyOptimizer phase, мс | 2 153,5537 | 1 498,3882 | −655,1655 |
| Whole-process peak working set, байт | 949 817 344 | 980 291 584 | **+30 474 240; +3,2084% — ухудшение** |
| GC0 /GC1 /GC2 | 188 /12 /9 | 189 /13 /10 | **+1 /+1 /+1 — ухудшения** |

**Любой полезный измеренный знак сохраняется как успех своей метрики и домена, без требования accuracy gain.** Allocations снизились; времена/CPU также имеют полезный наблюдаемый знак. Причинная уверенность — отдельная ось: один процесс на arm, fixed A→B, уменьшились и нетронутые фазы. Доказанного переносимого ускорения нет. Меньшие allocations не означают меньший peak; retained heap здесь не измерен.

Raw одинаково записывают изменение16 LoRA arrays, изменение0 из39 base arrays, неизменность reference, одинаковые конечные policy hashes A/B. Readback сообщает optimizerStep2, `optimizerEqualLive=true`, `freshWeightsEqualLive=true`, `randomStateReopened=true` у обоих. Это свидетельства состояния в сохранённых receipts; независимое декодирование checkpoint binaries здесь не выполнялось.

## Before/after и неполная привязка B

Четыре actual step JSON подтверждают совпадение A/B: step1 Loss0.91264373, margin0, accuracy0; step2 Loss0.9189987, margin−0.009629821, accuracy0. Это **`TRAIN_STEP_PRE_UPDATE` на двух разных уроках**, не оценка одного target до/после двух обновлений. Новая `learning_signal`/TRAINv2 панель отсутствует; NLL/rank/token/exact после обучения и качество свободных ответов **не измерены**, не равны нулю. Табличные столбцы «A после/B после» в текущем report применительно к этим scalars нельзя принимать за post-update learning readout. State-before/state-after действительно различаются; изменение16 тензоров само по себе не доказывает положительный учебный output. Новая диагностика не назначается старому запуску задним числом.

В [study][study] `armTrainerASha256` и `armTrainerBSha256` одинаковы: `09d03fc15b783384f730796773538dc60add748a35718058b5274c0c60200b48`. [Текущий report][report] уже объясняет, что hash снят до отдельной правки B, source-manifest содержит placeholder; текущая B-текстовая перестановка проверялась другим readback, но **точный исторический B binary не привязан полностью**. Этот дополнительный review не реконструирует отсутствующий custody. История обрыва внешнего launcher и последующего Run-Arms остаётся report-level; полного attempt-boundary доказательства здесь нет.

Расчёт удаления128 float-массивов даёт322 048 payload bytes; общий observed выигрыш312 328 меньше на9 720. Это разные показатели, причина остатка не установлена. Не приравнивать payload к GC/RSS и не исправлять raw под расчёт.

## Кандидат уже существует; R1 уже уточнён

В [required-changes.md][candidate] уже есть **N62-R2**: один локальный `expVals` на вызов `DpoTrainer.BackwardLogProbs` после cnt-guard, отдельный для chosen/rejected, с полной перезаписью элементов. Там записаны −312 328 allocations, peak/GC ухудшения, отсутствие TRAIN quality и неполная binary custody. Это кандидат решения пользователя; повторный опыт, второй кандидат с новым ID и production-перенос этим дополнением не назначаются. Protected-кандидат не редактировался.

[Обновлённый R1 report][r1] теперь признаёт −90 504 bytes как полезный наблюдаемый результат, показывает GC0/1/2 по+1, не выдаёт phase ratio за установленное ускорение RequireValid и не считает16 изменённых тензоров доказательством качества. Он прямо исключает сравнение двух разных уроков как before/after target. Наш прежний corrigendum остаётся историей обнаружения, **не утверждением, что эти оговорки всё ещё отсутствуют в current report**.

## Exact источники и blob identities

Общий путь: `docs/plans/replanning-possible-diffs/`; ref всех ссылок —66947785d1318f7cbbc88118f3156cdc75001e16.

| Относительный путь | Git blob SHA-1 |
|---|---|
| `experimets/cards/00-execution-index.md` | `b9b4e60ac08bf32930bc766bf0696b3cb16a920c` |
| `experimets/results/2026-10-20/N62-R2-01/report.md` | `0ee9657f3dd22dbc08d45e4f0767c0a113fbd672` |
| `experimets/results/2026-10-20/N62-R2-01/A/arm-result.json` | `cd30a064c76a9539dfb039d08ee70ad4841488b7` |
| `experimets/results/2026-10-20/N62-R2-01/B/arm-result.json` | `bccb1f00b67cfd84a103ae9b8aefdca92d9ed966` |
| `experimets/results/2026-10-20/N62-R2-01/study.json` | `1861594ebf891306b3a4053b019edb4fff4a7085` |
| R2 `A/checkpoints/dpo_step_000001.json` и `B/checkpoints/dpo_step_000001.json` | `7153b42117d9778d2f9fd2739b1337dde59caa3f` у обоих |
| R2 `A/checkpoints/dpo_step_000002.json` и `B/checkpoints/dpo_step_000002.json` | `3fff9f7cf1adf883aa11c04327d7b86085017779` у обоих |
| `experimets/results/2026-10-20/N62-R1-01/report.md` | `b291fca37f631c42a31255bbd9ffc20597bd5ef8` |
| `required-changes.md` | `d60fadbd207194e80e4fa66671bf984538ef4f75` |

[index]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md
[report]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/report.md
[a]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/A/arm-result.json
[b]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/B/arm-result.json
[study]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/study.json
[candidate]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/required-changes.md
[r1]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/report.md
