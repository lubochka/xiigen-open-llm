# Последняя сверка planning d7c41ad3: два фактора, объединение42

Прочитаны ровно6 назначенных файлов: [registry](https://github.com/lubochka/xiigen-planning/blob/d7c41ad3b306a2d94163d3a013a79edb4369626f/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-delta-669477/execution-registry.json), [output map](https://github.com/lubochka/xiigen-planning/blob/d7c41ad3b306a2d94163d3a013a79edb4369626f/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-delta-669477/output-path-map.json), две карточки и два промпта. Все6 локальных copies совпадают с Git blob SHA. Core cutoff сохранён: `6e3e8864933e35fccdb4f21369c9912fa195aaf6`. Новые HEAD, полный корпус, source manifests и другие документы не искались. Сборок, обучения, model calls и тестов нет.

**Вычислено по ID: наш предыдущий40 + peer40 −38общих =42 различных ID.** Первые36 совпадают в том же порядке. Peer-only: VT-APOSTROPHE-01, RMS-ROW-REDUCE-01; ours-only: ATTN-TRANSPOSE-STRIDES-01, JSON-OPTIONS-REUSE-01. Полные records для подстановки: `planning-d7-final-delta.json`, массивы `records`, `registry_records`, `output_records`.

## Порядок и status

Root принял: core01–34 → GRAD35 → FROZEN36 → **VT37 → RMS38 → ATTN39 → JSON40 → IDENTITY41 → KN42**. Итого7completed/35pending, NEXT=N63-INGRESS-LENGTH. LF-03 завершён, повтор не назначается.

В source peer последние4 — VT37→RMS38→IDENTITY39→KN40. Для общего пакета обязательны два согласования: FROZEN NEXT=VT по опубликованному d7; **RMS NEXT=ATTN вместо source IDENTITY**. Последний override относится также к effective карточке и отдельному RMS prompt. Source recipe и бюджет не меняются, peer документы сохраняются.

## Почему не дубли

**RMS-ROW-REDUCE-01** меняет только `TensorOps.SumAlongAxis`: при последней сохранённой оси читает `src[dstFlat*axisSize+k]` в прежнем порядке суммирования. Среднее, backward, MultiplyBroadcast и Transpose сохраняются. ATTN меняет индексы rank4 Transpose, FROZEN убирает mappedBIndices в no-grad MultiplyBroadcast, GRAD меняет копию первого AccumulateGrad; это другие методы/guards. Прежний RMS-COORD явно исключал Mean. Совпадение маленького SFT fixture с ATTN не делает фактор дублем. [Карточка L9–71](https://github.com/lubochka/xiigen-planning/blob/d7c41ad3b306a2d94163d3a013a79edb4369626f/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-delta-669477/cards/RMS-ROW-REDUCE-01.md#L9-L71), [промпт](https://github.com/lubochka/xiigen-planning/blob/d7c41ad3b306a2d94163d3a013a79edb4369626f/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-delta-669477/prompts/RMS-ROW-REDUCE-01.md).

**VT-APOSTROPHE-01** исправляет literal target в `QualityVerifierUnit.TryParseRequiredTerm`: native builder issue с `brother's` старый Split обрезает до `brother`; B извлекает строку между точными prefix/suffix до прежнего fallback. Native BuildGeneralBatch→2реальных verifier TrainOnExample→обученная память→обычный Verify; parser-only probe не допускается. Это не JSON-options lifetime ExecutorUnit, не concept enum parser и не source contraction `can't`. [Карточка L9–58](https://github.com/lubochka/xiigen-planning/blob/d7c41ad3b306a2d94163d3a013a79edb4369626f/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-delta-669477/cards/VT-APOSTROPHE-01.md#L9-L58), [промпт](https://github.com/lubochka/xiigen-planning/blob/d7c41ad3b306a2d94163d3a013a79edb4369626f/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-delta-669477/prompts/VT-APOSTROPHE-01.md).

Оба фактора **DESIGN_ONLY_NOT_MATERIALIZED / NOT_RUN**. Новый host не написан/не собран; измеренных побед нет. Source pin обоих669477, status cutoff6e3; pin не заменён moving HEAD.

## Exact исполнения

| Поле | VT-APOSTROPHE-01 | RMS-ROW-REDUCE-01 |
|---|---|---|
| Source root | `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/Qva01` | `C:/xg/RD01/a001/src` |
| Host | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01` | `C:/xg/RD01/a001` |
| Project | `Host/Runner.csproj` | `Host/Runner.csproj` |
| Results | `C:/xexp/QVA01/a01` | `C:/xg/RD01/a001/results` |
| Report | `C:/xexp/QVA01/a01/report.md` | `C:/xg/RD01/a001/results/report.md` |
| Candidate | `C:/xexp/QVA01/a01/candidate.md` | `C:/xg/RD01/a001/results/candidate.md` |
| Единственная внешняя chain | `pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01/launch.ps1` | `Set-Location 'C:/xg/RD01/a001'`, затем `python ./run.py` |
| Native budget всего | 2fresh Unit;4TrainOnExample;32Verify;4ExportCheckpoint | 8SFTupdates;8EXAM+8TRAIN diagnostic=16readout forwards |
| Deadline | build120s,A30s,B30s | build120s,A60s,B60s,общийnative120s |
| Source closure | 7полных native файлов на arm,2metadata files,19compile items;BCL only | 36native source files;исходный Linear;плоский .NET9project |

RMS использует заданный source entry `python`; произвольный иной interpreter здесь не подставлен. Список36файлов закреплён ссылкой source manifest, в этой срочной сверке он не перечитывался. VT seven-file manifest включён в карточку и перенесён в records. Missing runtime, existing source/host/attempt, timeout и durable counters следуют exact source recipes; ничего не удалять и не создавать a002 автоматически.

## Измерения и границы

VT:2TRAIN pairs→6builder rows/arm, учатся только2verifier rows с разными ADV markers.8literal candidate queries до/после; native generated issues и exported terms обязательны. Gold deserialize только после outputs. Главные denominators: term fidelity/2,falseaccept/3,falsereject/5,correct/8. Q7перефразировка и Q8U+2019 остаются допустимыми. Прогноз same6/8accuracy с falseaccept2/3→0/3 и falsereject0/5→2/5 — **статический прогноз, не результат**. Если такой результат измерится, полезный выигрыш не скрывается из-за общей точности; регрессии сохраняются рядом. [VT L167–178](https://github.com/lubochka/xiigen-planning/blob/d7c41ad3b306a2d94163d3a013a79edb4369626f/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-delta-669477/cards/VT-APOSTROPHE-01.md#L167-L178)

RMS: настоящий tinyC#SFT/LoRA, два TRAIN и два EXAM sign inputs;4updates/arm. A0/A1/B0/B1 на одинаковых panels, отдельно pre-update online losses. TRAIN cost включает forward/backward/clipping/AdamW; readout/build/hash/serialization вне окна. Изменённый trainable state обязателен; один optimizer counter недостаточен. Competition gold rank=1+strictgreater отличается от ordinal tie-break rank ATTN: не объединять как одну метрику. [RMS L73–98,131–139](https://github.com/lubochka/xiigen-planning/blob/d7c41ad3b306a2d94163d3a013a79edb4369626f/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-delta-669477/cards/RMS-ROW-REDUCE-01.md#L73-L139)

Любое измеренное полезное улучшение по оси/домену — успех, все остальные изменения и ухудшения рядом. Однократное A→B не устанавливает причинный wall-time gain. Полная production/test/CI chain и перенос исправлений не назначены. По STOP/NEXT — немедленное прекращение прежней работы, без «ещё одного оформления».
