# Финальная адресная сверка output-назначений: core827 против planning53cb

**Срез зафиксирован:** core `827a1acdbef95d220886e54f9d36bce0b405c8cd`; дальнейшее движение HEAD не проверяется. Прочитаны только `00-execution-index.md` и его ROPE/AST/BPE-REALIZED карточки. Для сравнения использованы сохранённые точные версии тех же трёх planning-карточек на `53cbcabd3926cda714bab428d294eb666301e9e4`; их blobs совпали с concurrent source manifest. Новых запусков, materialization или production-изменений нет.

**Результат: AST и BPE получили новые host/output/entry назначения. У ROPE эти назначения не изменились.** Но в уже собранном `concurrent-merged-registry.json` обнаружена отдельная ошибка проекции ROPE candidate filename: она исправляется по исходной карточке, без выдуманного нового назначения. Машинная дельта: [core827-changed-output-assignments.json](evidence/core827-changed-output-assignments.json) — только 2 изменившихся assignments и 1 отдельная поправка прежнего mapping.

## 1. Текущая очередь в пределах адресной проверки

Индекс core827 явно задаёт `NEXT=LF-03`, №01–06 завершены, 36 разных ID; SKNN — alias AST, не дополнительный запуск. Проверенные строки:31 ROPE→32 AST→33 NG01→34 BPE-REALIZED→35 IDENTITY→36 KN-ROUTER. Дополнительные 2 фактора пакета и итоговый пакетный счёт root интегрирует отдельно; здесь не переаудирован весь каталог36 карточек.[index]

NG01 в индексе ссылается на **тот же** protected path `NG01-null-guard-label.md`, который уже прочитан на669. Этот агент повторно файл не загружал. Root отдельно получил NG01 на827, blob `904f25866003dfdd535eab22043370cab55e4b6f`: его readback установил только два добавленных абзаца — output/owner/claim на строке3 и текущую очередь33→BPE с завершёнными01–06 на строке6. Source pin, данные, host/results/report/candidate.md и entry не изменились. Поэтому полный669 mapping из [предыдущего дополнения](06-core669-gap-status.md) семантически сохраняется; текущие позиция33 и NEXT=BPE-REALIZED берутся из827 index. Это явно помеченный receipt root, а не повторная проверка данного агента.[index]

## 2. AST: явный перенос outputs в host и единый entry

Planning53cb назначал `Core=C:\x\ast01`, host `<Core>/experiments-v2/RP01-MICRO/AST-SIB-CACHE-01`, а output в `<Planning>/llm mvp/design to code session/replanning/possibleDifs/runs/AST-SIB-CACHE-01/20261020-01`. Следовала отдельная последовательность prepare/build/четыре arms/compare.[old-ast]

Core827 заменяет это буквальным назначением ниже. Последнее уточнение явно отменяет вложение второго `experiments-v2`, прежний relative path к35.ExperimentsV2 и запись в planning/runs. Старые абзацы карточки, где ещё описан planning output, помечены историческими; они не являются альтернативой текущему выходу.[ast-final]

| Поле | Действующее значение |
| --- | --- |
| SourceCore | `C:/Projects/llm_mvp_core`, read-only, source pin `0b76ab950d82534f5904005dda096545505ae307` |
| Planning | `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session`, read-only; sibling checkout не вычислять |
| Host / Core | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01` |
| Project | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/Runner.csproj` |
| Native copies | `Host/Native/A.cs`, `Host/Native/B.cs`; Compile только эти 2 файла и `Program.cs` |
| Results | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/results/attempt-0001` |
| Report | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/report.md` |
| Candidate | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/candidate-fix.md` |
| Единственный entry | `powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/Run-Assigned.ps1` |

Источник literal wrapper — строки 177–205.[ast-entry] Несмотря на общий текст «report.md и candidate.md» в начале, там же явно указано AST-исключение `candidate-fix.md`; раздел отчёта повторяет его на строке 146. Поэтому правильный basename AST-кандидата — **candidate-fix.md**, без переименования в generic candidate.md.[ast-output]

Wrapper получает `Host/execution.claim` до подготовки. Только Prepare эксклюзивно резервирует Out; последующие фазы не повторяют prepare/attempt guard. Отдельные arms получают свой `training.claim` до construction/firstPredict. Эти уточнения не дают права обойти существующую legacy попытку сменой корня; результат остаётся будущим `DESIGNED_NOT_RUN`/`DESIGN_READY_NOT_RUN`, build не проверен.[ast-final]

## 3. BPE-REALIZED: BR01 вместо C:/xg/br01

Planning53cb использовал `C:/xg/br01`, results `r/0001`, report/candidate.md и отдельные две сборки с последующим `run-pair`.[old-bpe] Core827 назначает следующие пути и один внешний wrapper:[bpe-entry][bpe-final]

| Поле | Действующее значение |
| --- | --- |
| SourceCore / source pin | `C:/Projects/llm_mvp_core`; `883496bca18113bf37e0afe683c75b05a0075048`, не HEAD827 |
| Host | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01` |
| Project | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/Runner.csproj` |
| Fixture | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/in/fixture.json` |
| Results | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/r/0001` |
| Report | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/r/0001/report.md` |
| Candidate | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/r/0001/candidate.md` |
| Единственный entry | `powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/Run-Assigned.ps1` |

Только Run-Assigned получает host execution.claim до build; `run-pair` один раз резервирует result intent. Старый `C:/xg/br01`, если существует, остаётся readonly legacy ownership: новый root не разрешает повтор. Candidate basename не изменился; изменён полный путь.[bpe-final]

## 4. ROPE: source-назначения сохранились, ошибка только в нашей проекции

На planning53cb строки 47 и 78 уже указывали `results/candidate.md`; строки 66–67 — тот же Python314 entry. Core827 повторяет эти назначения.[old-rope][rope]

| Поле | Значение в обоих источниках |
| --- | --- |
| Host | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001` |
| Results | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001/results` |
| Report | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001/results/report.md` |
| Candidate | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001/results/candidate.md` |
| Entry | Из Host: `C:/Python314/python.exe ./run.py`; отдельный PowerShell wrapper не добавляется |

В сохранённом пакетном `experiments/concurrent-merged-registry.json:outputs` для ROPE было записано `candidate-fix.md`. Это **ошибка mapping**, а не изменение назначения между source snapshots. В JSON дельты она вынесена отдельно в `mapping_corrections`; в `changed_assignments` ROPE не добавлен.

Core827 дополнительно закрепляет host claim до build и training claims до model/readouts/Train; эти правила не меняют перечисленные host/results/report/candidate/entry и не разрешают повтор ранее выданного запуска.[rope-final]

## 5. Receipt

Четыре новых core blobs проверены побайтно; три planning blobs совпали с сохранённым source manifest. Полный новый receipt — `sources/core827/fetch-receipt.json`.

| Snapshot / файл | Git blob SHA |
| --- | --- |
| core827 index | `920b648d83bc625f43ba393657f5348263826032` |
| core827 ROPE | `a0e0e33d41a555cb78d10980a599a104ad7e5f75` |
| core827 AST | `764f400ce82127050c9a1b4957493d252a8a31fb` |
| core827 BPE-REALIZED | `1e6e717450cd898dd08c67b63c566b9dd6616315` |
| planning53cb ROPE | `7baa33f059260b7e53579c30688ac65bdfec86f9` |
| planning53cb AST | `94db0a52c548dd152e1b5eed6f48e917bb712468` |
| planning53cb BPE-REALIZED | `ff10bf1d0900168fc0e77b19d5b0709649936ea3` |

[index]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L5-L64
[ast-entry]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md#L177-L205
[ast-output]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md#L146
[ast-final]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md#L221
[bpe-entry]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/BPE-REALIZED-MERGE-COUNT-01.md#L148-L190
[bpe-final]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/BPE-REALIZED-MERGE-COUNT-01.md#L199-L206
[rope]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/ROPE-TRAIN-TABLE-01.md#L55-L88
[rope-final]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/ROPE-TRAIN-TABLE-01.md#L101
[old-ast]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md#L38-L41
[old-bpe]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/08-BPE-REALIZED-MERGE-COUNT-01.md#L138-L178
[old-rope]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/06-ROPE-TRAIN-TABLE-01.md#L47-L78
