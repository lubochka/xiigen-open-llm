# Позднее уточнение core0cf3634: результат LC и общие обученные старты

Исходный аудит остаётся привязан к `43e2337d68dad0e4ab8d7fbb0cea38406b592b61`. После его публикации (`74832c0`) появился дочерний core `0cf3634ce03d811498ec66e317e5a80ae84488e7` от 21.10.2026 09:10:39 UTC, «14th experiment results and plan conflicts resolution». Это ограниченная сверка новых сохранённых данных и их документного статуса; весь аудит не перезапускался. Новых model calls, сборок, исправлений продукта и изменений защищённой папки в этой работе **0**.

## 1. Что изменилось в статусе

Теперь сохранены qualification receipt, A/B raw, pair-summary и отчёт. Общая подготовка успела выполнить **1000 SFT updates и записать веса**, затем упала на неверной перегрузке проверки артефакта. Позднее из этих весов выполнен recovery: **8 Forward, 0 Train**. Оба измеряемых плеча имеют по два DPO updates. Старый статус `TERMINAL_UNKNOWN_AT_SNAPSHOT` был корректен для прежнего commit; на новом срезе он заменяется наличием законченных result artifacts и запретом повторять consumed попытку.

Сам отчёт заявляет `COMPLETED` и exit0 обоих плеч, но квитанция сверки явно оставляет **`subsequent_terminal_exit=NOT_INDEPENDENTLY_VERIFIED`**. Здесь не утверждается независимая проверка выхода процессов. Точный действующий статус карточки — `CONSUMED_EXTERNAL_RECOVERY_AND_ARM_OUTPUTS_NO_RERUN`; новых вызовов сейчас не требуется. [reconciliation](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/reconciliation/lc-saved-result-reuse-2026-10-21.json); [карточка:3–5](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md#L3-L5); [отчёт:6](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/report.md#L6)

## 2. Обучение действительно приобретено и удержано на двух TRAIN-строках

`preparation.json.learning_qualified=true`, `completed_updates=1000`, обе строки в `acquired_ids`. Это **teacher-forced exact**, а не проверка авторегрессионного ответа: `Generate=0`. Контрольных отложенных строк здесь нет. Утверждение ограничено данными о поведении C# кода в двух фиксированных учебных парах.

| TRAIN ID | W0 правильных токенов | S0 | После DPO | NLL до → после, одинаково A/B | Margin до → после, одинаково A/B |
|---|---:|---:|---:|---|---|
| `simulation-csharp-doc-f06-008` | 0/46 | 46/46 | 46/46 в A и B | 0.0002897947 → 0.00029096575 | 1.6698191 → 1.700693 |
| `simulation-csharp-doc-f21-001` | 0/44 | 44/44 | 44/44 в A и B | 0.00030099385 → 0.0003021206 | 1.8870742 → 1.9145378 |

У обеих строк после DPO чуть **ухудшается chosen NLL**, одновременно улучшается предпочтение и полностью сохраняется teacher-forced exact. Рост margin общий для A и B, поэтому это не отдельный эффект cache. `EvaluateMany.Loss` в обоих плечах 0.00012252886 → 0.000108037435, `RewardMargin` 9.009271 → 9.134691, `Accuracy` 1 → 1. [common raw](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/common/preparation.json) (`readouts.W0/S0[*].score`); [A raw](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/A/arm-result.json) и [B raw](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/B/arm-result.json) (`readouts.before/after`, `metrics.evaluateBefore/evaluateAfter`).

## 3. Полезный результат cache: меньше выделений и наблюдаемого времени учебного перехода

Каждое плечо выполнило два `StepVector`, по две пары на шаг, два `EvaluateMany` и восемь panel forwards. Всего — **4 DPO optimizer updates, 8 предъявлений пар, 4 EvaluateMany, 16 panel forwards**. Поле `plannedDpoUpdates=4` встречается в study и в каждом raw, в обоих местах source буквально присваивает4, не связывая это поле с actual count: **AMBIGUOUS_OR_MISNAMED**. Не приписываем ему семантику study-level бюджета или числа предъявлений. Фактические `actualOptimizerStep=2`, `stepVectorCalls=2`, `pairsPerStep=2` подтверждают два обновления и четыре предъявления пар на плечо. `evaluateAfter.Step=3` — метка оценки, не третий вызов оптимизатора. [Точный native bridge: два вызова и literal поля budget](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/35.ExperimentsV2/Experiments/Rp01Micro/N61RefCache/Common/ContinuousLearning/N61RefCacheBridge.cs#L421-L456). Cache B: 4 lookups, 2 misses, 2 hits, 2 stores, capacity2.

| Raw показатель | A | B | B−A |
|---|---:|---:|---:|
| `trainWallMs` | 23,096.5536 | 19,375.8694 | -3,720.684 (-16.109%) |
| `trainAllocatedBytes` | 7,331,104,632 | 6,010,567,464 | -1.320537e+09 (-18.013%) |
| `referenceForwardCompletedCalls` | 8 | 6 | -2 (-25.000%) |
| `referenceForwardMs` | 6,651.4938 | 3,051.1557000000003 | -3,600.338 (-54.128%) |
| `policyForwardMs` | 6,918.1003 | 8,222.7217 | +1,304.621 (+18.858%) |
| `backwardMs` | 6,339.814400000001 | 6,385.4859 | +45.6715 (+0.720%) |
| `optimizerMs` | 417.7764 | 446.4895 | +28.7131 (+6.873%) |
| `managedLiveBytes` | 667,774,072 | 292,646,736 | -3.751273e+08 (-56.176%) |
| `peakWorkingSetBytes` | 1,808,465,920 | 1,807,183,872 | -1,282,048 (-0.071%) |

Уменьшение **trainAllocatedBytes на 1,320,537,168 байт (18.013%)** и наблюдаемого `trainWallMs` на 3,720.6842 мс (16.109%) — полезные измеренные показатели настоящего обучения. Повышение accuracy для их признания не требуется. Это успех соответствующих осей **на этой паре учебных строк и этом запуске**, уже записанный кандидатом переноса в `required-changes`; устойчивое ускорение, перенос на новые данные и готовность продукта этим не установлены. Сравнение одно, без повторов, прогрева и смены порядка A→B. [pair summary](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/pair-summary.json) (`differences`, `equivalence`, `limitation`); [кандидат](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/candidate.md); [required-changes](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/required-changes.md).

### Ограничение фазовой телеметрии и точные оговорки к отчёту

В **обоих** шагах B по `costs.stepNTiming.PolicyForward` записано `StartedCalls=2`, `CompletedCalls=4`, `IncompleteCalls=-2`. Значит счётчики начала/завершения фаз несогласованы. `referenceForwardCompletedCalls:8→6` нельзя выдавать за точный подсчёт реальных reference model forwards. Фазовые миллисекунды показываем как raw с этой оговоркой; увеличение `policyForwardMs` не доказывает регрессию политики, и его нельзя уверенно объявлять измеренным масштабом машинного шума. Этот дефект измерителя не стирает aggregate wall/allocation наблюдения и не превращается в новый эксперимент вне обучения. Для будущей отдельно назначенной измерительной копии нужно согласовать Start/Complete timing scopes в B-копии `DpoTrainer.StepVectorCore`; текущие raw сохраняются, повтор не назначается. [B raw](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/B/arm-result.json) (`costs.step1Timing.PolicyForward`, `costs.step2Timing.PolicyForward`).

Дополнительно: optimizer time вырос на **6.8728%**, а не «0,7% и меньше», как написано в report:52. Экономия памяти равна **1.320537168 GB** в десятичной системе или примерно **1.2298 GiB**, а не 1.23 десятичных GB. `managedLiveBytes` — снимок живого heap, не training peak. Самый ранний сохранённый loss подготовки — update50 `0.681177`; выбранное в отчёте `0.0113` относится к update200, не к началу обучения. [отчёт:46–52](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/report.md#L46-L52); [журнал](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/preparation.log).

## 4. Какие состояния сохранены и что можно переиспользовать

Сохранённые веса **S0**: `c444092ffb0370b703450a6b365e03570f7c67ee05ffa4adac5625c8d3cffd3c`, **47,899,519 байт**, Git LFS pointer blob `ffd3b71030d959ac62fb96005f2af2625f0cb274`. Это обученные веса модели `rp01-csharp-small/v1`, q/v LoRA rank8 alpha16 dropout0, vocabulary629, 55 тензоров =39 base +16 adapter. Исходный fixture SHA256 `75fa380a51e96083332c9609ae69ce368b04e29011fab4af0c0fc161335202c8`; tokenizer `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066`; config `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`; W0 weights `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`. Ordered carrier lengths121/123, materialized chosen IDs, без нового LF. Полные paths/identities — в JSON рядом и [study](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/study.json) / [preparation](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/common/preparation.json).

SFT optimizer и SFT RNG **не захвачены**. Полного продолжения SFT из этого bundle нет. Но эти компоненты не нужны как SFT continuation для уже использованного конкретного DPO-потребителя: он явно создал fresh DPO optimizer0, одинаковый fresh native RNG при dropout0, загрузив S0 weights. Сохранённые веса пригодны в этой точно описанной семантике; повтор1000SFT «ради полного bundle» не назначается и пользователю не предлагается. Не выдавать их за canonical DPO parent, h64 baseline или общий старт любых других fixture. [актуальное исключение карточки](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md#L3-L5); [family h512-ref-two](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json) (`families[0].weights_only_reuse`, `full_bundle_ready=false`).

У A и B **одинаковый Git tree checkpoints** `7c7e10cf4a586f4d18dcade5501c1dd326f0eaf2`, включая оба шага. Для final step2 LFS content identities:

| Артефакт | SHA256 содержимого из LFS pointer | Размер |
|---|---|---:|
| policy | `5a0eb8c90ed5fb75158ce34f011b75ea2a4a66b2d61304319f036f9e6140d601` | 47,899,519 |
| optimizer | `a89c525a97d31a3c600f518796b804f1a931828e604b0ba644cfc6e9d45b17f6` | 426,788 |
| RNG | `7788f9e20ddd9e99de38b9ee5259bf0df2e48c8534624e33745da64a3bc53c75` | 689 |

Это совпадение сохранённых object identities и raw endpoints. В этом аудите бинарники не скачивались, не загружались в модель и не сравнивались с утраченной памятью исходного процесса. `checkpointReadbackIdentical=true` — квитанция повторного чтения сохранённого файла, не доказательство сравнения с утраченным состоянием до ошибки. [A checkpoints](https://github.com/lubochka/llm_mvp_core/tree/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/A/checkpoints); [B checkpoints](https://github.com/lubochka/llm_mvp_core/tree/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/B/checkpoints); [reconciliation source_review](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/reconciliation/lc-saved-result-reuse-2026-10-21.json).

## 5. Какие прежние замечания закрывает новый commit

| Замечание | Новый статус | Что остаётся |
|---|---|---|
| EA-01, статус LC «не запускалось» | **Исправлен**: row16 и верх карточки CONSUMED; raw опубликованы | Независимый terminal exit не проверен. Wrapper всё ещё открывает build/preparation log redirects до раннего existing-attempt guard; он не переиздаётся |
| EA-02, повтор common1000 у совместимых consumers | **Проектный mapping введён**: h512-dt-joint (MS01/MC01); h512-dt-segmented (TS01/KGB/NH01); h512-postfix-one (AGC1/FBM1) | Все три FUTURE_NOT_MATERIALIZED. Готовых bundle/исполненных exporters эта сверка не обнаруживает и не утверждает. LC weights-only — другое семейство |
| EA-03, неверная тема Wiki | **Index29/30 исправлен** на «Оркестрация D07, маршруты Wiki» | KN-ROUTER-LEARNED-KEY-01:58 всё ещё называет C#/.NET и документацию «доменами». Это контексты маршрутов, тема обучения — оркестрация |
| EA-04, цель whole-response N63C | В этом ограниченном delta не переоткрывался | Две другие усвоенные строки N61 автоматически не закрывают квалификацию N63C |

[индекс](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L76-L113); [shared mapping](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json); [остаток темы](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md#L58); [wrapper:9–19](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/experiments-v2/RP01-MICRO/N61-REF-CACHE/Run-With-Learning-Signal.ps1#L9-L19).

## 6. Очередь после commit

Сейчас **53 ID,14 сохранённых/частичных,39 будущих**. Это счётчик позиций, не число валидных положительных результатов. Первые50позиций сохранены, добавлено:

| Позиция | ID | NEXT |
|---|---|---|
| 51 | HIERARCHICAL-LOCAL-HISTORY-01 | SVD-DIRECTED-TARGET-01 |
| 52 | SVD-DIRECTED-TARGET-01 | ATTN-CAUSAL-SCORES-01 |
| 53 | ATTN-CAUSAL-SCORES-01 | END |

LC остаётся consumed, его исторический NEXT — RETENTION-M1-01; первое ещё не выданное задание по общему правилу — PARENT-ADOPTION-01. Прямое назначение Любы имеет приоритет. Наши три только опубликованные FILE-STATIC-RECEIVER-01 / RMS-MEAN-BACKWARD-01 / ATTN-SOFTMAX-COPY-01 не являются этими новыми51–53; их включение в защищённый registry этим commit не заявлено. [index:27](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L27); [index:110–113](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L110-L113).

Это дополнение сохраняет прежний аудит43e2337 как датированный срез и задаёт более поздний статус. Исходные raw не изменяются; новые runtime-вызовы и перенос в продукт не выполняются.

## 7. Согласование наших новых SFT карточек с контрактом33

Общие стартовые состояния теперь имеют canonical paths. Для `RMS-MEAN-BACKWARD-01` и `ATTN-SOFTMAX-COPY-01` в этом planning-пакете явно назначен один источник:

```text
C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1
```

Protected JSON перечисляет только исходный consumer `RMS-ROW-REDUCE-01`; две наши карточки добавлены как **документные consumers только здесь**, без изменения защищённого списка. Прежний `RD01/a001/results/common` остаётся original source отдельно назначенного экспорта. Исполнитель не выбирает между двумя paths, не запускает Prepare/Export и не повторяет400 общих SFT steps. Полный bundle ещё `FUTURE_NOT_MATERIALIZED`.

Карточки, промпты и реестр согласованы:11 файлов, `bundle.json` COMPLETE с hashes/length остальных10, полное SFT AdamW400/m/v, native RNG, qualification; A/B из одних bytes, затем4 updates каждого arm до404. Контракт и source pins разделены: **native code43e2337, donor contract0cf3634**. Ровно36 source blobs прежнего closure сохранились — шесть полных subtree SHA совпадают. Это совместимость исходников, не исполненный restore или измеренная польза новых факторов. [Receipt неизменности и family identities](evidence/late-shared-start-delta.json), [canonical mapping и наши дополнительные consumers](evidence/current-shared-donors-0cf3634.json), [каталог](05-reusable-trained-states.md).

Private RNG identities закреплены по RD01 literals, а не по имени consumer. LC weights-only исключение этим SFT-парам не подходит. Каждая сохраняет8 updates /24 forward, common training0. Softmax использует исходный S0, не состояние после RMS. Новые51–53 protected карточки исследуют local history, directed SVD и causal QK scores; это другие факторы. Новых измеренных улучшений трём нашим карточкам не приписано.

## 8. Что опубликовано вместе с уточнением

[Машиночитаемый LC readback](evidence/late-core-delta.json) сохраняет raw keys, числа, object identities, ограничения и pinned sources. [Реестр](registry.json), README, каталог S0 и все новые карточки/промпты получили актуальные статусы и правило автоматического reuse пригодного сохранённого результата без вопроса о повторном обучении. Исходные1368 файлов корпуса повторно не переаудировались; новые сведения проверены ограниченным отдельным проходом. Контроль связности дополнения — [late-update-review](evidence/late-update-review.json).
