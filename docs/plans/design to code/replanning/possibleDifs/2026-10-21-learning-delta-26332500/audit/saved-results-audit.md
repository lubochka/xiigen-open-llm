# Сохранённые результаты: что действительно обучалось и что доказано

**Срез: 21 октября 2026, core `26332500f76ea8345256c73ecfd44c0023b7e883`.** В активной очереди 50 ID: 13 сохранённых или частичных попыток и 37 будущих постановок. У 13 попыток различаются завершённость исполнения, наличие учебного эффекта и готовность вывода к переносу. Число попыток нельзя называть числом успешных навыков. Актуальные документы уже признают существенную часть ограничений, перечисленных ниже; повторно выдавать их за новые находки нельзя. [Действующий индекс][index]

Это независимый документный readback существующих файлов. **Новых Train/Forward/Verify, сборок, повторов, исправлений production и перезаписи старых результатов — 0.** Защищённая папка `docs/plans/replanning-possible-diffs` прочитана, но не изменена. В [saved-results-readback.json](saved-results-readback.json) находятся выбранные поля 28 локальных JSON и одного дополнительно прочитанного GitHub JSON N63R, с источниками и идентичностью байтов. Парсинг целого JSON не означает повторное исполнение или полную проверку исторического бинарника.

## 1. Краткая карта 13 сохранённых попыток

Успех полезной метрики сохраняется вместе с её доменом и ухудшениями остальных осей. Он не превращается в доказательство другой, неизмеренной способности. Цена реальных updates сохраняется как наблюдение даже там, где эксперимент не предъявил освоенного ответа; такой результат не закрывает открытую учебную цель.

| № / ID | Фактически выполненное обучение | Полезное наблюдение | Ограничение и ухудшения |
|---|---|---|---|
| 01 N-KU-01 | По одному `FileActionKnn.Train` на A/B, 4 урока; 8 запросов до попали в empty-state guard, 8 native Predict после | По критерию `ADD_LOOP` / любой ненулевой `NOT_ADD_LOOP`: A 0→1/4, B 0→3/4; ложные лишние циклы 3→0 | A отвечает одним классом на все запросы, B другим одним классом; единственный необходимый цикл потерян. Это не доказанное различение входов и не точность полного совета. [Raw A][ku-a], [Raw B][ku-b], [gold][ku-gold] |
| 02 NEURAL-FROZEN-TRANSPOSE-01 | Настоящий SFT/LoRA, 4 updates на руку | Train allocations 13 647 840→12 140 448 Б, конечные состояния одинаковы | EXAM 0/2→0/2 в обеих руках; правило знака не выучено. Retained cache +249 856 Б; CPU 62,5→78,125 мс; wall не улучшился. Невалидно выдавать 0→0 за сохранение навыка. [Raw A][lft-a], [Raw B][lft-b] |
| 03 BPE-NO-MATCH-COPY-01 | 4 непустых Train, отдельно A/B для двух профилей; по 64 реально выученных merges, словарь 256→320 | Code-doc allocations 3 904 664→1 126 744 Б; EN→RU 2 892 824→893 656 Б. State и tokens A/B одинаковы; roundtrip 4/4 в каждом профиле | Это обучение токенизатора. Neural/KNN accuracy не измерялась. Code-doc CPU 31,25→46,875 мс; один порядок A→B не доказывает устойчивый speedup. [Code A][bpe-ca], [Code B][bpe-cb], [EN→RU A][bpe-ta], [EN→RU B][bpe-tb] |
| 04 N62-R1 | По 2 завершённых exact DPO `StepVector`, 16 adapter arrays изменены, base не изменён | Allocations −90 504 Б; одинаковые конечные policy hashes и успешный optimizer readback | Полная учебная панель не назначалась; навыка по hashes не вывести. GC0/1/2 выросли на 1; время одного A/B не причинный speedup. [A][r1-a], [B][r1-b] |
| 05 N62-R2 | По 2 завершённых DPO updates | Allocations −312 328 Б, конечные состояния равны | Peak всего процесса +30 474 240 Б, GC +1; сохранённый placeholder/pre-factor B-source hash не доказывает custody итогового бинарника. Полное качество не измерено. [A][r2-a], [B][r2-b] |
| 06 E-OPT-STEP | По 2 завершённых DPO updates | Allocations −467 944 Б; GC0 −4; equal final state | Peak процесса +204 546 048 Б; аналогичная граница provenance B; learned-answer panel отсутствует. [A][opt-a], [B][opt-b] |
| 07 LF-03 | По одному завершённому DPO update | Allocations −646 816 Б, GC0 −3; конечные policy hashes совпали | Wall +1 908,1644 мс, CPU +609,375 мс. Диагностика первого токена: ответ остаётся неверным; полного ответа нет. Не запускать поздний diagnostic заново. [A][lf-a], [B][lf-b] |
| 08 N63-INGRESS-LENGTH | A: отказ до updates; B: 2 SFT updates, 70 targets | Два коротких урока перестали теряться из-за длинного | Correct tokens до/после 0; учебный эффект не доказан. A 0 updates против B 2 — разная работа, поэтому сравнение времени не speedup. Старый entry consumed; исправленная постановка имеет другой ID. [Arms][ni], [report][ni-report] |
| 09 N63-AR-CAP | 9 updates всего; WA восстановлен в A/B; A продолжил 2, B 3 updates | B сохранил A07 в буфере и реально повторил урок | Исходная попытка 0 correct tokens во всех четырёх состояниях. Состав буфера — механизм, не качество. Отдельная probe ниже показывает частичное удержание и цену. [Raw][nr] |
| 12 N63-CORRECTION-COLLISION | Сохранённое A реально выполнило 1 DPO + 1 SFT и 18 scoring calls; B не дошло до запуска | Подтверждён конкретный отказ RNG после обучения, а не отсутствие обучения | A correct 0/23 во всех стадиях; raw `dpoUpdates=0/sftUpdates=0` ошибочны из-за зависимости от поздней ошибки. Неполное A/B и учебная цель OPEN. Отдельная probe не исправляет исходный опыт задним числом. [Raw A][nc], [bridge][nc-bridge] |
| 13 WIKI-TAXONOMY-01 | По одному непустому Train, 4 строки ×18 эпох; updates A3/B7 | Correct-selected A0→2/6, B0→4/6; новые классы стали представимы | Регрессия отрицательного контроля, близкие парафразы. Тема — оркестрация D07, а не обучение документации. Состав отличается от WM01, причинный вывод между опытами запрещён. [A][wt-a], [B][wt-b] |
| 14 WIKI-MARGIN-01 | Одноклассовый Train, 2 строки ×18 эпох; A0 updates, B1 | В B появились ненулевые веса и выбор своей учебной метки | B2/4 равен постоянному классу 2/4; два отрицательных запроса теперь выбраны неверно. Условие обновления сработало, различение маршрутов не показано. [A][wm-a], [B][wm-b] |
| 15 N60-E1 | A6/B5 завершённых Unit lessons, из них Verifier A2/B1; 16 Verify | Ложные отказы A1→B0; состояние −151 Б | Ложные приёмы A0→B1; суммарно 3/4 в обеих руках. Оба raw сохранены до позднего отказа summary. Ни full Translate, ни широкий transfer не проверены. [A][nv-a], [B][nv-b], [report][nv-report] |

В N62/R2/E-OPT/LF сохранённая цена относится к действительному native DPO, а не к внешнему валидатору. Но отсутствие полной панели ответа и границы source→binary нельзя устранить редактурой итоговой формулировки. Текущий `required-changes` уже ограничивает эти выводы; повторные прогоны для «закрытия» отчёта не назначены. [Актуальные кандидаты][required]

## 2. N60-E1: успех одной метрики при регрессии другой

Здесь есть полный путь участия в обучении: `TranslationStructuredBatchBuilder.BuildGeneralBatch` → `TranslationMachine.TrainStructuredBatchAsync` → `QualityVerifierUnit.TrainOnExample` → `_requiredTermsBySourceMarker` → `Verify`. B оборачивает единственный `examples.Add(...:verifier...)` условием `!string.Equals(rejectedCandidate, pair.Chosen, StringComparison.Ordinal)`. У пары V1 маркер `[WRONG:tense]` без отрицательного текста приводит к candidate==chosen; A всё равно учит требование `спокойно→calmly`, B пропускает этот ложный отрицательный урок. Остальные уроки остаются одинаковыми. [Фактор и call chain][nv-report]

| Метрика | A0 | A1 | B0 | B1 |
|---|---:|---:|---:|---:|
| Верных вердиктов /4 | 2 | 3 | 2 | 3 |
| Ложных отказов допустимого перевода | 0 | 1 | 0 | 0 |
| Ложных приёмов ошибочного перевода | 2 | 0 | 2 | 1 |
| Размер состояния, Б | 1 751 | 6 695 | 1 751 | 6 544 |

Полезный результат — допустимая парафраза `in a composed manner` перестала отвергаться. Одновременно отсутствие слова со смыслом «спокойно» стало проходить проверку. B действительно обучил и использовал другой маркер, `громко→loudly`; это не сравнение двух не обучавшихся проверяющих. Но **парафраза не стала семантически распознаваться**: конкретное требование вообще не выучено в B. Поэтому перенос одного guard в продукт остаётся кандидатом с показанной ценой, а не завершённой коррекцией смыслового Verifier. Это ограничение уже отражено в текущем отчёте. [Состояния и вердикты][nv-a], [B][nv-b]

Ресурсная причинность ограничена. Даже панель до вмешательства, с одинаковым кодом и пустым состоянием, показала allocations 89 056 против 41 000 Б и wall 124,46 против 59,39 мс. Разность Train allocations −32 800 Б меньше этой pre-factor разности. Она сохраняется как наблюдение, но не как доказанная экономия фактора. `AccuracyAfter=1` нативного batch result означает покрытие трёх Unit ID; это не 100% правильных вердиктов. `CpuMs` там — TickCount elapsed, `PeakRamMb` — снимок managed heap. Для качества используются четыре actual Verify; для стоимости — отдельные измеренные интервалы. [Значения и происхождение полей][nv-report]

`Program.WritePairSummary` завершился `KeyNotFoundException` после сохранения A и B из-за регистра JSON-полей. Отдельный `pair-summary-report.py` разобрал существующие receipts без повторного обучения. В текущем аудите эти два завершённых плеча не объявляются failed training. [Сводка][nv-summary]

## 3. N63C: поздняя ошибка, неверные счётчики и отдельный частичный успех

В исходном A `dpoMetrics.optimizerStep=1`, DPO timing имеет RETURNED; затем bridge получил SFT return и снял S2. Ошибка возникла при `WriteCreateNew(... after-sft.rng.json)`: нативный writer требует собственное каноническое имя. Счётчики в `N63CollisionBridge.cs:492–493` вычисляются через `error is null ? 1 : 0`, поэтому поздняя ошибка обнулила уже состоявшиеся операции только в отчётных полях. **Нельзя выводить actual updates из общего exit/save status.** B отсутствует, и его показатели равны «не измерено», а не нулю. [Bridge:395–427,492–493][nc-bridge], [raw][nc]

Актуальная карточка уже назначает отдельный owner-only SFT state directory, native `BuildFileName(snapshot.RandomStateStep)` и раздельные issued/returned/applied/save counters. Это исправление контракта сохранения; само по себе оно не добавляет приобретённого навыка. [Текущий документ доказательств][nc-evidence]

Отдельный сохранённый `N63C-PROBE/results/n63c-probe.json` завершён: 600 acquiring SFT + по 100 SFT на руку + 2 DPO = **802 updates**, 60 scoring forwards. До приобретения T1 0/23; после приобретения 22/23. После DPO обе руки сохраняют 22; после stale SFT A20/23, после corrected SFT B21/23. Это полезное частичное удержание одного токена в B относительно A, вместе с потерей одного токена относительно общего learned before. Полный T1 exact=false; Q1/Q2 имеют 0 correct tokens во всех состояниях, их NLL в B немного хуже A. Ни перенос правила, ни «DPO приобрёл целый правильный ответ» не доказаны. [Полный raw probe][nc-probe]

Дополнительная граница воспроизводимости: нынешний `N63CollisionProbe.cs` задаёт 60/.003/20, а raw описывает 600/.0001/100. Исторический source/binary, создавший raw, не восстановлен этим чтением. Существующий raw сохраняется как измеренное свидетельство, а не приписывается автоматически сегодняшнему исходнику. Старые snapshots с `UNKNOWN` нельзя цитировать как окончательный статус: поздний полный JSON уже существует. [Current source][nc-probe-src], [ранее ограниченный readback][nc-evidence]

## 4. N63R: частичное удержание реально есть, цена и остальные строки тоже

Исходный 9-update опыт непригоден для вердикта о сохранении освоенного ответа: TRAIN и EXAM во всех W0/WA/A1/B1 дают 0 correct response tokens. В raw это прямо признано. Отдельный N63R-PROBE получил ненулевое before: 1000 prior updates и 400 BUG updates, затем A800/B1200/C1200, всего **4600 updates**, 40 scoring forwards. C — контроль количества updates, но не числа токенов и не учебной последовательности. [Исходный raw][nr], [probe raw][nr-probe]

| Строка / denominator | Обученный before | A после | B после | C после |
|---|---:|---:|---:|---:|
| Повторяемый старый A07 /49 | 22 | 2 | **15** | 4 |
| Близкий старый EXAM EO02 /49 | 15 | 1 | **15** | 3 |
| Неповторяемый старый A01 /43 | 23 | 2 | **1** | 2 |
| Новый N01 /26 | 0 | 22 | **18** | 24 |
| Новый N02 /31 | 0 | 28 | **25** | 28 |

Все значения — teacher-forced correct response tokens, не число целиком правильных ответов. Full response exact=false во всех показанных состояниях. A обучил 63 600 токенов, B118 800, C95 400. В B против A updates +50%, tokens +86,79%, allocations примерно +106,77%, wall примерно +131,86%. B лучше сохраняет конкретно повторённый A07 и близкий EO02, но хуже по A01 и новым строкам. Это локальный успех retention по названным метрикам с ценой; универсального улучшения или равной стоимости здесь нет. [Probe raw][nr-probe]

## 5. Wiki: состав набора — часть вывода

WT01 и WM01 обучают один предмет — **выбор маршрута оркестрации, D07**. Запросы про перевод, wiki и поездку являются входами маршрутизатора; этим опытом не обучаются нижележащие переводчик, документационный генератор или планировщик поездок. Положительные классы и отрицательные контроли в рамках одного такого предмета допустимы. [Словарь тем][topics]

WT01 использует 4 учебные строки в четырёх группах. B добавляет два ранее непредставимых класса; A2/6→B4/6 — узкий эффект representability и обучения на близких формулировках. Это не широкое обобщение. WM01 использует только две строки единственного класса: A не делает updates, B делает один, затем выбирает один и тот же класс на всех четырёх запросах. Его 2/4 равно constant-class baseline. Поэтому срабатывание margin criterion и изменение весов доказаны, input discrimination — нет. [WT A/B][wt-a], [WT B][wt-b], [WM A][wm-a], [WM B][wm-b]

Сравнение этих двух опытов **не устанавливает**, что отсутствие отрицательных уроков вызвало регрессию WM01: одновременно различаются набор и вмешательство. Даже правдоподобное объяснение здесь остаётся гипотезой. Контракт 32 теперь прямо требует фиксированных классов, ролей строк, семейства TRAIN/EXAM, постоянного ответа и запрета выбирать состав после исхода. Дополнительный balanced/factorial run в этой сессии не назначен. [Контракт состава][dataset]

Остающееся документное несоответствие: index №29/30 WIKI-FEATURE-KEYS/FEATURES-ONCE всё ещё подписаны «Документация / Wiki», хотя их карточки учат тот же выбор CodeTraceTraining/WikiQuestionAnswer. Предлагаемая точная правка домена дана отдельно; она не меняет набор и не создаёт новую гипотезу. [Additive поправки](additive-card-amendments.md)

## 6. Что реально разблокировано в будущих карточках

PARENT-ADOPTION-01 и N63-INGRESS-LENGTH-LEARNED-01 имеют фиксированные H/R/entry, donor, минимальный набор и конечный бюджет. Однако сам PA01 host не был материализован или собран этой сессией. H16 donor после 1000 SFT показывает суммарно **3/40** TRAIN response tokens и **0/2** full responses; его историческое продолжение на 200 updates даёт 4/40, не целый навык. Это пригодный предмет проверки принятия, полного восстановления и продолжения **частично обученного состояния**; не следует переименовывать его в модель, почти решающую целую задачу. Actual LFS payload и runtime restore этим аудитом не выполнялись. [PA01][pa], [исправленный ingress][nil]

Исправленный ingress сравнивает A, отказавшее всему batch до updates, с B, принимающим ровно 200 коротких предъявлений и откладывающим 100 длинных. Известный частичный before предъявляется до фактора. Это осмысленный ограниченный ремонт старого 0→0, но не сравнение equal-work throughput. Допустимы отдельный useful token/NLL показатель и обязательные regressions; полное освоение остаётся открытым, пока full exact не наблюдается. [Карточка][nil]

В группе будущих H512 cost-опытов назначены 1000 COMMON SFT, затем qualification приобретения хотя бы одной **полной** TRAIN response, сохранение S0 и одинаковый старт A/B. В H64 группе — 400 COMMON и явно названное приобретение хотя бы одного полного однотокенного ответа; 1/2 не объявляется общим правилом знака. Если qualification не выполнена, arms не стартуют, steps не увеличиваются до PASS, goal остаётся OPEN. Эти ремонты уже есть в core; **400 и 1000 — конечные design budgets, не измеренная гарантия достаточности**. [Интеграция актуальных рецептов][integration], [H64 пример][rope], [H512 пример][agc]

Для открытого N63C готового доказанного полного learned-before нет. Текущий индекс честно оставляет цель OPEN и старый entry consumed. Назначение маршрута к строке №12 не разрешает ещё одну probe, замену модели или новый suffix. Автор следующего ремонта должен заранее дать конкретный trained-before predicate, источник/восстановление состояния, точный конечный бюджет, source/binary custody и все команды; исполнитель не выбирает это самостоятельно. [Индекс][index], [N63C card][nc-card]

## 7. Что добавляется и что остаётся открытым

Этот пакет добавляет независимую карту доказательств и ограничения финальных raw, а не новые численные результаты. Ресурсные кандидаты BPE, N62 и частичные удержания N63R/N63C не удаляются. Сохранённая положительная метрика не даёт автоматического разрешения на production transfer. Решение о переносе принимает Люба; полный CI, исправление продуктовых тестов и повтор старых опытов здесь не запускались.

Открыты: восстановление exact historical binary provenance для названных probes/копий, full-response qualification для N63 целей, runtime-подготовка будущих hosts и широкая применимость доменных эффектов. Структурная сверка всего реестра отражена в [registry-coverage.md](registry-coverage.md); конкретные неустранённые расхождения документов — в [additive-card-amendments.md](additive-card-amendments.md).

[index]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md
[required]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/required-changes.md
[ku-a]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json
[ku-b]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json
[ku-gold]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/gold.json
[lft-a]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/A/code-sign.json
[lft-b]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/B/code-sign.json
[bpe-ca]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/code-doc.json
[bpe-cb]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/code-doc.json
[bpe-ta]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/en-ru.json
[bpe-tb]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/en-ru.json
[r1-a]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/A/arm-result.json
[r1-b]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/B/arm-result.json
[r2-a]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/A/arm-result.json
[r2-b]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/B/arm-result.json
[opt-a]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/A/arm-result.json
[opt-b]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/B/arm-result.json
[lf-a]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json
[lf-b]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json
[ni]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/N63I/results/arms.json
[ni-report]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/N63I/results/report.md
[nr]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/N63R/results/n63-ar-cap.json
[nr-probe]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/N63R-PROBE/results/n63r-probe.json
[nc]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json
[nc-bridge]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionBridge.cs
[nc-evidence]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/2026-10-21-n63c-rng-learning-evidence.md
[nc-probe]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json
[nc-probe-src]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs#L54-L58
[nc-card]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md
[wt-a]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/WT01/results/A/report.json
[wt-b]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/WT01/results/B/report.json
[wm-a]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/WM01/results/A/report.json
[wm-b]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/WM01/results/B/report.json
[nv-a]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/A/arm-result.json
[nv-b]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/B/arm-result.json
[nv-report]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/report.md
[nv-summary]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/pair-summary.json
[topics]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/05-topics-and-profiles.md
[dataset]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/32-training-set-and-inference-contract.md
[pa]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/PARENT-ADOPTION-01.md
[nil]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/N63-INGRESS-LENGTH-LEARNED-01.md
[integration]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/2026-10-21-14308-integration.md
[rope]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/ROPE-TRAIN-TABLE-01.md
[agc]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/GRAD-ACCUM-COPY-01.md
