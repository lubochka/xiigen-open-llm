# Измеренные полезные результаты: кандидаты на исправления

**Семь завершённых опытов сохранены как кандидаты по конкретным полезным показателям. Ничего не переобучалось и не переносилось в основной код в этом аудите.** Первые пять перечитаны на core669477, шестой — из core `827a1acdbef95d220886e54f9d36bce0b405c8cd`, седьмой LF-03 — из `6e3e8864933e35fccdb4f21369c9912fa195aaf6`. Точное происхождение raw, учебная операция, регрессии и ограничения важнее общего ярлыка PASS/FAIL.

Любое измеренное улучшение полезного показателя — успех по этому показателю и предметной теме; обязательного роста accuracy нет. Результат может одновременно улучшать одну ось и ухудшать другую. Кандидат с ограниченной доказательностью не становится автоматически принятым product fix. Исходные raw остаются неизменными, поздние поля диагностики к ним не дописываются.

## Краткий реестр кандидатов

| ID | Реальная учебная операция | Полезный результат | Ухудшения и ограничения | Решение этого пакета |
|---|---|---|---|---|
| N-KU-01 | Учебная метка C# → FileActionKnn.Train → память из 4 записей | Ложный ADD_LOOP на уже содержащем цикл коде **3/3→0/3**; rubric score **1/4→3/4** | Нужный ADD_LOOP **1/1→0/1**; ответы после B все ADD_METHOD; время/CPU Predict выросли | Сохранить label-кандидат с регрессией, без заявления о полностью правильном действии |
| NEURAL-FROZEN-TRANSPOSE-01 | 4 SFT/LoRA updates на плечо | Выделено **13 647 840→12 140 448 байт**, **−11, 04%** | Удерживаемый cache **+249 856 байт**; wall и CPU немного выше; exact0/2→0/2 | Кандидат экономии выделений; срок жизни/invalidation кэша отдельно при решении о переносе |
| BPE-NO-MATCH-COPY-01 | 4 непустых native Train: A/B ×2 профиля | code-doc **−71, 14%**, EN→RU material **−69, 11%** выделений | Не измерено end-to-end качество перевода; code-doc CPU выше | Успех обучения tokenizer по стоимости; сохранён тот же learned state |
| N62-R1 | 2 настоящих DPO StepVector на плечо | **−90 504 байта**, **−0, 002404%** | GC каждого поколения +1; TRAIN before/after panel отсутствует; fixed A→B | Кандидат адресной экономии, без причинного обещания времени |
| N62-R2 | 2 настоящих DPO StepVector на плечо | **−312 328 байт**, **−0, 008296%** | Process peak **+30 474 240 байт**; GC+1; неполная привязка B-бинарника | Сохранить ресурсный успех и ограничение provenance; не повторять |
| E-OPT-STEP | 2 настоящих DPO StepVector на плечо | **−467 944 байта**, **−0, 012429%** | Process peak **+204 546 048 байт**; stale factor/measured_path от R1; B-hash placeholders | Сохранить шестой кандидат с явным ограничением атрибуции; поздний erratum учтён |
| LF-03 | 1 настоящий DPO StepVector на плечо | **−646 816 байт**, **−0, 034992%**; одинаковые state/readback | Wall **+12, 50%**, CPU **+4, 23%**; peak −43 495 424 байта; неполная historical B/bridge binding | Седьмой allocation-кандидат; не повторять; NEXT №08 N63-INGRESS-LENGTH |

Все значения allocations — накопленные выделения в обозначенной границе, **не объём занятой RAM**. Process peak — свойство всего процесса, включая подготовку/чтение; его нельзя выдавать за peak одного Train. Один fixed A→B без повторов не устанавливает причинное ускорение. Здесь не назначаются повторы ради улучшения статистики: документируются уже существующие измерения.

## 1. N-KU-01: метка do-while достигает обучения

Первичные [arm-A][nku-a], [arm-B][nku-b] и [отчёт][nku-report] показывают A0/B0 с пустой памятью, обучение четырём строкам и A1/B1 с памятью из 4 записей. Метод `ActionLabelGenerator.Label` участвует до `FeatureVector` и native `Train`; runner повторяет выбранную подготовку из `SemanticCompilerFacade.TrainFileActionKnn`. Адрес исходной правки и единственный do-loop фактор заданы в [карточке][nku-card]. Это настоящий memory learner, а не проверка строки без обучения.

На четырёх запросах A1 выдаёт ADD_LOOP везде, B1 — ADD_METHOD везде. На трёх запросах, уже содержащих цикл, исчезло ложное требование ADD_LOOP. На четвёртом, где цикл нужен, нужный ADD_LOOP потерян. При k3 и малом несбалансированном наборе 3: 1 этот результат нельзя представить как доказательство более содержательной геометрии близости. Полезный label-эффект сохраняется; универсальное качество конечной рекомендации не доказано.

Основной timer включает **разметку, признаки и copy Train**: [runner: 69–77][nku-timer]. Отдельный product-equivalence Train в A: 120–149 расположен вне этого timer. Поэтому main A/B — два copy Train плюс один product Train. Отчёт раскрывает дополнительный reverse B→A, ещё два copy и один product; итог 6 — число из отчёта, а не заново выполненная или полностью повторно извлечённая серия. Основные raw перечитаны; reverse raw этим аудитом повторно не собирались.

Measured phase allocations 1 179 032→1 169 344 байта; wall195, 8347→123, 8038 мс. Четыре послеучебных ответа занимают 22, 405→26, 0613 мс, CPU15, 625→31, 25 мс. Эти изменения показаны рядом, не превращены в устойчивые причинные оценки. Product-equivalence только в A также мешает чистому сравнению whole-process peak.

**Кандидат:** исправить исходную учебную классификацию do-while при отдельном решении пользователя, сохранив явно указанную регрессию нужного ADD_LOOP. Не расширять опыт, не менять k и не добавлять новые учебные строки в сохранённые результаты.

## 2. NEURAL-FROZEN-TRANSPOSE-01: реальные allocations против удерживаемого кэша

Сохранённый [отчёт и raw-ссылки][lft-report] относятся к маленькому native Transformer: 1 блок, 2 адаптированных слоя, 4 обучаемых массива, 12 frozen; vocab256/context64/hidden64. TRAIN — два задания о знаке вычитания, 4 updates/arm. Фактор — переиспользовать transpose только у неизменяемых весов в `00.TensorCore/Layers/Linear.cs`, `Linear.Forward`.

Allocations уменьшились на 1 507 392 байта; 8 misses/24hits подтверждают работу кэша в этой траектории. Удерживаемый payload вырос на 249 856 байт. В сохранённом сравнении совпадают learned weights, optimizer moments, четыре учебных losses и все проверенные logits; base неизменна. Target NLL на двух EXAM снизилась одинаково в A/B: 8, 015334→7, 974560; exact остался 0/2. Это полезный эффект стоимости с одинаковым результатом обучения.

Wall84, 3133→84, 5700 мс и CPU62, 5→78, 125 мс выросли. Снижение process peak на 73 728 байт — отдельное наблюдение, не равенство стоимости кэша. Нельзя вычитать retained payload из накопленных allocations и называть результат «освобождённой RAM».

Отдельная 400-step sanity дала TRAIN/EXAM1/2, но постоянные знаки `+,+` и `-,-` тоже получают 1/2 на сбалансированной паре. Она не доказывает правило выбора знака, не проверяет cacheB на 400 шагах и не меняет бюджет четырёхшагового A/B. Журнал sessions37/38 не позволяет установить суммарное число их updates; оно не объявляется ни 400, ни 800. Новый запуск не нужен для оформления.

**Кандидат:** frozen-only cache в `Linear.Forward`. При отдельном решении о переносе потребуется определить invalidation при restore, изменении base и разморозке; эти свойства исходным A/B не проверены. Подтверждённый исторический Windows long-path failure относится к подготовке этого LFT scaffold до Train.

## 3. BPE-NO-MATCH-COPY-01: успешный завершённый Train, а не нынешний blocker

Первичные [code-doc A][bpe-ca]/[B][bpe-cb] и [EN→RU A][bpe-ta]/[B][bpe-tb] подтверждают по одному непустому `Train(corpus,320)` в каждом процессе. Каждому предшествует empty-byte initialization: итого 8 вызовов Train, из них 4 содержательных перехода. Vocab256→320, learned merges64, final hashes A/B совпадают в каждом профиле.

| Профиль | Allocations A | Allocations B | B−A | Сохранённое изменение представления |
|---|---:|---:|---:|---|
| C# / документация | 3 904 664 | 1 126 744 | −2 777 920 (−71, 143637%) | Средние tokens43→21, 75, round-trip4/4; A/B совпадают |
| Материал перевода EN→RU | 2 892 824 | 893 656 | −1 999 168 (−69, 107834%) | Средние tokens48→21, 75, round-trip4/4; A/B совпадают |

Фактор — возвратить прежний chunk при отсутствии пары для merge вместо создания равнозначной копии; при настоящем совпадении создавать новый chunk. Место — `01.Tokenizer/BpeTokenizer.cs`, `MergeAll/MergeChunk`, точный patch в [карточке][bpe-card]. Не менять выбор пар, веса chunks, счётчик реализуемых merges или inplace-модифицировать shared list: это другие факторы.

Счётчики B: 41 152/28 424 reuse на unchanged chunks и 1 280/1 720 changed copies. Это подтверждение механизма на сохранённых материалах. CPU code-doc31, 25→46, 875 мс вырос; время 54, 3095→34, 3645 мс, но причинное ускорение одним fixed A→B не доказано. Влияние на learned kNN/DPO или итоговый перевод не измерялось.

Старая пользовательская цитата о раннем сбое сохранена как история поведения исполнителя. Она не отменяет более поздние completed raw. **Этот ID завершён и не должен запускаться ещё раз.**

## 4. N62-R1: побитовое сравнение frozen base без поэлементного LINQ

Первичные [A][r1-a]/[B][r1-b] относятся к source pin62bcc8f; core669477 — место текущего хранения результатов. На плечо выполнено 2 DPOupdates, 16 адаптеров изменились, 39 base остались bitwise frozen. Итоговые policy hashes совпадают, portable optimizer и повторно открытое состояние соответствуют live.

Фактор внутри `DpoProjectLoraRecipient.RequireValid`: сравнение исходных float битов через `MemoryMarshal.AsBytes(...).SequenceEqual(...)` вместо ленивого поэлементного `Select(BitConverter.SingleToInt32Bits).SequenceEqual(...)`, с сохранением прежней побитовой семантики и fallback для слишком большой длины span. Исходный LINQ не материализует полный `ToArray`; экономия не описывается как удаление копии всей base. Это находится на пути настоящего StepVector/publication, а не в отдельной утилите, не вызываемой обучением. [Адресный отчёт][r1-report].

Allocations3 764 969 984→3 764 879 480; полезная разница 90 504 байта невелика, но не обнуляется порогом accuracy. GC[188, 12, 9]→[189, 13, 10]. Наблюдаемые ускорения разных фаз не атрибутируются единственному factor, поскольку менялись и не затронутые им интервалы. Два online losses принадлежат двум урокам: это не A0/A1 одной TRAIN-панели. Новую панель задним числом не добавлять.

## 5. N62-R2: reuse softmax scratch с ограничением provenance

Первичные [A][r2-a]/[B][r2-b] содержат `COMPLETED`, issued2/completed2, end16: 12: 13 Z и 16: 13: 01 Z. Поэтому это пятый завершённый опыт, а не очередной future wrapper. Фактор — локально переиспользовать `expVals` внутри `DpoTrainer.BackwardLogProbs`, сохранив арифметику и учебную цепочку. [Отчёт][r2-report].

Allocations3 764 947 024→3 764 634 696; process peak949 817 344→980 291 584, то есть **+30 474 240 байт**. GC каждого поколения+1. Сохранённые состояния и метрики совпали; отсутствуют новая TRAIN-panel и самостоятельное доказательство качества свободного ответа.

**Ограничение атрибуции:** в study указаны одинаковые A/B trainer hashes, вычисленные до правки B, а B-manifest имеет placeholder `APPLIED_AFTER_FACTOR_EDIT`. Поздний исходный text diff виден, но точная связка исторического B-бинарника с этим diff не восстановлена. Это не повод удалить измеренный allocation-success; это причина не обещать закрытую цепочку происхождения и не называть эффект универсально доказанным. Сохранить raw и оговорку, не перезапускать ради отчёта.

## 6. E-OPT-STEP: новый шестой результат и неверные подписи raw

В core827a1 появились завершённые A/B: по 2 DPOupdates, 16 changed adapter arrays, 39 неизменных base tensors, совпадающие записанные state/readback. Этот результат [разобран по primary receipts](09-core-827-delta.md); там же приведены exact source/manifest ссылки и метод предполагаемого переноса.

Allocations3 764 994 488→3 764 526 544: **−467 944 байта, −0, 012429%**. Process peak840 916 992→1 045 463 040: **+204 546 048 байт**. GC[193, 19, 13]→[189, 14, 10] — полезное наблюдение в той же серии. Нельзя утверждать, что снижение выделений причинило рост peak, что число сэкономленных байт детерминированно для будущих запусков или что один A→B установил причинное ускорение.

В raw поля `factor` и `measured_path` остались от R1: frozen-base/RequireValid. Study/report описывают чтение optimizer step без копий m/v. Это конфликт подписей, который должен быть виден рядом с результатом. В source-manifest B-hashes всех трёх factor files также `APPLIED_AFTER_FACTOR_EDIT`; исторический бинарник не привязан полностью. Новая публикация сохраняет численные наблюдения и адресный кандидат, но **не исправляет исходные raw и не выдаёт недостающую привязку за имеющуюся**.

На core6e опубликован [measurement erratum](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/measurement-erratum.md): ошибки подписей и provenance уже признаны, исходные raw не переписаны. Его NEXT LF-03 тоже устарел после сохранения седьмого результата. Для E-OPT и LF-03 новых wrapper/diagnostic запусков нет; в inherited neural protocol осталось 12 будущих ID.

## 7. LF-03: завершённый настоящий DPO, полезная экономия и более медленная фаза

Первичные [A](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json), [B](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json), [study](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/study.json), [report](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/report.md) и source manifest перечитаны на core6e. Оба arm имеют COMPLETED и issued1/completed1. Всего **2 optimizer updates, 4 policy forwards, 4 reference forwards, 4 backward и 4 отдельные диагностические policy forwards**; teacher/generate/warmup/repeat=0. Меняются 16 LoRA arrays, 39 base arrays сохранены; reference не меняется; optimizer step1, fresh weights/optimizer/RNG readback совпадают между плечами. Полные числа и hashes — [квитанция](evidence/core6e-delta.json).

Выделения **1 848 491 176→1 847 844 360 байт**: −646 816, −0, 034992%. Wall **15 270, 2952→17 178, 4596 мс** (+12, 50%), CPU **14 406, 25→15 015, 625 мс** (+4, 23%); process peak **830 795 776→787 300 352 байта**, GC [93, 8, 6]→[90, 5, 3]. Это наблюдения одного фиксированного A→B; причинное ускорение не заявлено. Timer охватывает фактическую учебную фазу с guards/receipts, не только одну копию массива.

На одной TRAIN-панели first-token NLL **6, 814549566480718→6, 814264388510441** одинаково в A и B; rank221, predicted token589 и exact=false не изменились. Это свидетельство небольшого учебного сдвига в обоих вариантах; **разница NLL от фактора B равна нулю**. Первый токен не заменяет full-answer accuracy.

Кандидат относится к `00.TensorCore/Core/Tensor.cs::Backward(Tensor? initialGradient)`: убрать внешнее `initialGradient.Data.ToArray()` при сохранении собственной копии `TensorStorage`. Сохранённые [A Tensor](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/A/Core/Tensor.cs) и [B Tensor](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/B/Core/Tensor.cs) отличаются ожидаемым фактором. Историческая привязка B-бинарника при этом не закрыта: [manifest](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/source-manifest.json) содержит `APPLIED_AFTER_FACTOR_EDIT` вместо B Tensor hash, а его bridge hash не совпадает с нынешними bytes. Study hash самого manifest совпал. Расчёт payload608 872 не объясняет всю measured разницу 646 816: остаток 37 944 байта. Поэтому фразу «детерминированно около 647 КБ» нельзя переносить в вывод этого аудита.

Полезное снижение выделений остаётся кандидатом с перечисленными оговорками. Повтор, новая диагностика и изменение production не назначаются. [Последнее согласование и NEXT](10-final-reconciliation.md).

## Решение о переносе

Все семь записей — кандидаты для решения пользователя. В них указаны измеренная полезная ось, регрессии, конкретное исходное место и границы воспроизводимости. Никакой общий CI, исправление product tests, перенос experimental exception в production, удаление старого опыта или новый run для «красивого» отчёта этим документом не назначается. Немедленный STOP/NEXT выше любого дооформления.

[nku-a]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json
[nku-b]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json
[nku-report]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md
[nku-card]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md
[nku-timer]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/experiments-v2/RP01-MICRO/N-KU-01/runner/Program.cs#L69-L149
[lft-report]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/report.md
[bpe-ca]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/code-doc.json
[bpe-cb]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/code-doc.json
[bpe-ta]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/en-ru.json
[bpe-tb]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/en-ru.json
[bpe-card]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/BPE-NO-MATCH-COPY-01.md
[r1-a]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/A/arm-result.json
[r1-b]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/B/arm-result.json
[r1-report]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/report.md
[r2-a]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/A/arm-result.json
[r2-b]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/B/arm-result.json
[r2-report]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/report.md
