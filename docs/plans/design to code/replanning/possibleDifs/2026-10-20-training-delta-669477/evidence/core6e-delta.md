# Последняя сверка core 6e3e886: LF-03 уже завершён

Проверяемый commit — **`6e3e8864933e35fccdb4f21369c9912fa195aaf6`**, прямой потомок `827a1acdbef95d220886e54f9d36bce0b405c8cd`, сообщение «7th experiment results and plan conflicts resolution». Это ограниченная проверка новых первичных результатов, erratum и изменённых карточек. Сборки, обучение, генерация и изменения защищённой папки не выполнялись. Базовый аудит 669477 и предыдущий delta 827 не переписываются.

## 1. Обязательное изменение очереди

**LF-03 исключается из будущих запусков: сохранённый A/B завершён. Следующий пункт — №08 `N63-INGRESS-LENGTH`.** В новом core index по-прежнему написано «№07 LF-03», а строка07 продолжает обозначать будущий рецепт. Это отставание документа от первичных receipts, не разрешение повторить опыт. Сама последовательность index задаёт после LF-03 именно N63-INGRESS-LENGTH; `LC-REF-CACHE` находится на позиции14. Источники: [index:5–36](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L5), [A:1–16](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json#L1), [B:1–16](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json#L1).

| Область | Всего ID | Завершены | Будущие |
|---|---:|---:|---:|
| Текущая core очередь | 36 | 7 | 29 |
| Ранее проверенный overlay с двумя новыми VT/RMS | 38 | 7 | 31 |

Вторая строка не учитывает дополнительные гипотезы, которые параллельно появились в planning и проверяются отдельно. При их добавлении автор итогового registry должен увеличить общий состав явно, сохранив семь завершённых и NEXT08. Число оставшихся ID унаследованного neural TRAIN-протокола — **12**, поскольку LF-03 уже имеет собственную сохранённую панель. Нельзя дописывать её заново или запускать дополнительные forwards ради нового формата.

## 2. LF-03: фактический update, стоимость и сигнал обучения

Оба плеча имеют `status=COMPLETED`, `error=null`, `issued=completed=1`. A закончилось **17:25:22.0873198 UTC**, B — **17:25:53.2746899 UTC** 20 октября. В каждом плече выполнен один настоящий `DpoTrainer.StepVector` и один `applyOptimizer`, два policy-forward, два reference-forward и два policy-backward. Отдельно записаны два диагностических policy-forward на плечо. Общий бюджет: **2 updates, 4 policy-forward, 4 reference-forward, 4 backward и 4 diagnostic-forward**; Evaluate, Generate, teacher, warm-up и повторов по сохранённому паспорту нет. [Study](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/study.json), [provenance](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/provenance.json), [A phase counters](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json#L177).

Домен — документация к C#; одна сохранённая simulation-пара `simulation-csharp-doc-f06-008` про постфиксный инкремент, 75 prompt tokens, 46 chosen, 43 rejected, sequence121, vocabulary629. Флаг `csharpRuntimeVerified=false` сохранён. Исторический source pin в raw — `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`; он не равен commit хранения новых результатов, и эти две роли нельзя смешивать. Начальный B0 — `rp01-csharp-small/v1`, веса SHA256 `a65887cc…`, 55 тензоров: 39 base и 16 q/v-LoRA. [Report:15–25](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/report.md#L15), [study:16–43](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/study.json#L16).

| Измеренная ось | A | B | B−A |
|---|---:|---:|---:|
| Выделения в TRAIN-окне, bytes | 1 848 491 176 | 1 847 844 360 | **−646 816; −0,0349916%** |
| TRAIN elapsed, ms | 15 270,2952 | 17 178,4596 | **+1 908,1644; ухудшение** |
| Process CPU в окне, ms | 14 406,25 | 15 015,625 | **+609,375; ухудшение** |
| Peak working set процесса, bytes | 830 795 776 | 787 300 352 | −43 495 424 |
| GC0 / GC1 / GC2 | 93 / 8 / 6 | 90 / 5 / 3 | −3 / −3 / −3 |
| Backward, ms | 3 052,7733 | 3 777,7379 | +724,9646 |
| Policy-forward, ms | 2 555,3092 | 3 824,9797 | +1 269,6705 |
| Reference-forward, ms | 2 818,2022 | 3 451,0547 | +632,8525 |
| ApplyOptimizer, ms | 756,462 | 725,1544 | −31,3076 |

Таблица получена из [A raw](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json) и [B raw](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json); вычисленные delta сохранены в `analysis/core6e-delta.json`.

TRAIN-панель действительно измеряет модель **до и после настоящего update**. В обоих плечах NLL первого gold-токена уменьшается с **6,814549566480718 до 6,814264388510441**, то есть на **0,000285177970277**. Gold rank остаётся221 из629, предсказанный token589 (` existing`), exact-first-token=false. Панели A/B совпадают поэлементно. Это положительный сигнал обучения обоих плеч; **дополнительного NLL-выигрыша B относительно A нет**. Измерение относится к учебной паре, не к held-out accuracy или качеству полной генерации. [A/B diagnostic fields:17–43](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json#L17), [observer:395–443](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/Common/ContinuousLearning/N62ExperimentBridge.cs#L395).

В обоих плечах изменились16 adapter arrays, base changes=0, reference unchanged=true. Начальный policy hash `2ba6bd6d…` изменился на одинаковый конечный `ef0222ed…`. Readback: optimizerStep1, optimizerEqualLive=true, freshWeightsEqualLive=true, randomStateReopened=true. Поэтому это действительное обучение, а не отдельный benchmark копирования. Равенство записанных контрольных показателей одного A/B не доказывает универсальную эквивалентность для любых размеров/домейнов. [State/readback:71–105](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json#L71).

### Область измерения и ограничения отчёта

Текущий bridge считает `GC.GetAllocatedBytesForCurrentThread` вокруг учебного цикла. В окно входят `StepVector`, guard-проверки с ExportOptimizerState и запись issued/completed receipts; это не process-wide allocation и не отдельная цена только `Tensor.Backward`. TRAIN-readouts стоят до/после окна, readback — после. Источник: [bridge:191–246](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/Common/ContinuousLearning/N62ExperimentBridge.cs#L191).

Ожидаемый удалённый payload массивов — `2 × 4 × 121 × 629 = 608 872` bytes. Измеренная экономия646816 больше на37944; происхождение остатка не установлено. Поэтому фраза report:75 «детерминированно … около647КБ» слишком сильна. Корректно: **наблюдаемое полезное уменьшение allocations на этом A/B, при совпавших записанных показателях обучения и оговорках происхождения ниже**. Один fixed A→B не устанавливает причинность time/CPU/peak. Замедление незатронутого policy-forward и смена знака между разными опытами показывают неоднородность наблюдений, но сами по себе не доказывают её единственную причину. [Report:61–79](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/report.md#L61).

### Новая существенная оговорка: LF-03 снова имеет неполный manifest

Current A/B `Tensor.cs` различаются ожидаемым фактором: B убирает внешний `initialGradient.Data.ToArray()`, сохраняя конструктор `Tensor` и последующую защитную копию. Factor/measured_path в raw теперь корректны. Однако наличие правильного текущего текста не закрывает привязку исторического бинарного исполнения:

- Study указывает SHA256 manifest `598dc9782a848db89cde3d83bc1c3e8c9b3998e294acba9bd41a6f6d42841ff5`; прочитанные bytes дают именно этот hash.
- В этом же manifest, строка939, `armSources["Core/Tensor.cs"].bSha256` всё ещё **`APPLIED_AFTER_FACTOR_EDIT`**. Фактический SHA256 текущего B-файла — `ebd9e3a1ca649a140486716167f78ce7a53a7e8d585d57299602e6bef06bcdcb`.
- `bridgeSha256` в manifest равен **`d00355907f83fd6e9e56daea4ddb998fb90cda24a8d98581e50fd9ad2587f3da`**, а фактические bytes текущего bridge дают **`eb765f3a95ff50814b31dd702d8d0c013734e514205236b771a6ad2ab545401a`**.

Источник: [manifest:934–944](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/source-manifest.json#L934), [B Tensor:95–107](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/B/Core/Tensor.cs#L95), [study:40](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/study.json#L40). Хэши точных загруженных bytes записаны в `analysis/core6e-source-manifest.json`.

Это **не отменяет завершённые receipts и полезное наблюдение**, но не позволяет объявить исторический B binary аттестованным данным manifest. Старые raw/manifest не исправляются задним числом, повтор для устранения пробела не назначается. Кандидат исправления сохраняется: `00.TensorCore/Core/Tensor.cs::Backward(Tensor)` — убрать внешний ToArray, оставить защитную копию TensorStorage. Решение о переносе в продукт остаётся за пользователем.

## 3. EOPT: erratum добавлен, старые raw не переписаны

Новый [measurement-erratum.md](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/measurement-erratum.md) раскрывает ранее найденные ограничения: stale R1 factor/measured_path, три placeholder B source hashes, отсутствие точной аттестации historical B binary, отсутствие новой TRAINv2 панели и запрет повтора. Он сохраняет allocation benefit−467944 bytes, ухудшение peak+204546048 и GC delta−4/−5/−3. Это исправление интерпретации документом, не новая метрика и не устранение пробела происхождения.

Git blobs четырёх прочитанных ранее файлов — A raw, B raw, report и study — **не изменились относительно827**; это сверено с полным деревом EOPT results текущего commit. Сам original report поэтому сохраняет прежние чрезмерные формулировки; читать его вместе с erratum и delta827. Последняя фраза erratum «следующий №07 LF-03» уже устарела после нового LF-03 receipt; её перекрывает NEXT08 настоящей сверки.

## 4. Изменения карточек и назначений результатов

Полная inventory cards содержит прежние158 файлов: **157 карточек + index; 27 файлов изменены, 0 добавлены и 0 удалены**. Вне этой ограниченной проверки весь новый commit не обходился; его partial commit-files API список300 не использовался как доказательство полноты.

Для29 остающихся core future IDs **новых переназначений result roots и outer entry нет** относительно effective827 map. Все29 roots проверены по действующим заголовкам карточек. Большая часть diff заменяет старые protected paths в body/CLI уже действовавшими host-local roots. Полный актуализированный mapping: `analysis/core6e-output-map.json`; изменённые тексты и источники — `analysis/core6e-card-diff.txt` и `analysis/core6e-source-manifest.json`.

Содержательные уточнения ограниченного delta:

| Карточка | Изменение | Значение для итогового overlay |
|---|---|---|
| AST | Однозначный `candidate-fix.md`; фиксированный SourceCore/Planning; claims до lifecycle, один prepare owner | Существующий root и `Run-Assigned.ps1` сохранены; extra wrapper не нужен |
| BPE-REALIZED | Early execution/training claims; явный MSBuild path/Compile contract; source-manifest до build | Root BR01/r/0001 и `Run-Assigned.ps1` прежние; отсутствие materialized host не скрывать |
| KN-ROUTER | Стоимость чтения измеряется вокруг уже назначенных `Read(before)`/`Read(after)`; process-wide allocation через await, retained claim | Бюджет по-прежнему2 Train и16 Route, дополнительных вызовов нет; reader/Train cost scopes раздельны |
| N62 | EOPT runtime prompts отозваны, требование actual LF03 factor hashes до build | Завершённый LF03 receipt теперь перекрывает и этот будущий рецепт; новый manifest всё ещё не удовлетворяет собственному требованию |
| P54 | Изменена только строка порядка запуска | Семь ранее описанных пробелов подслучаев сохраняются; готовность470 не заявлена |

Источники: [AST:203–226](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md#L203), [BPE:204–212](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/BPE-REALIZED-MERGE-COUNT-01.md#L204), [KN:218–222](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md#L218), [N62:417–419](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L417), [P54:114](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L114).

Часть новых core формулировок разрешает писать в `required-changes.md` в рамках той сессии. **В текущем поручении пользователь прямо запретил менять защищённую папку; этот запрет остаётся выше чужих карточек.** Наш overlay сохраняет один candidate в назначенном разрешённом result root и только документирует кандидатов. Никаких переносов/удалений прежних результатов, автоматического нового attempt или production promotion из этого delta не следует.

## 5. Граница независимой проверки

Сохранены и проверены по Git blob37 точных файлов:27 изменённых карточек, пять primary LF03 JSON/Markdown, EOPT erratum и четыре LF03 source/manifest. Все bytes дополнительно имеют SHA256. Полные inventories cards и двух results subtrees сохранены отдельно. Проверка source-manifest подтверждает bytes самого manifest, **не происхождение всех114 копий и не сборку исторического бинарника**. Смысл всех470 подслучаев и executable readiness всех157 карточек этой ограниченной проверкой не аттестуются.
