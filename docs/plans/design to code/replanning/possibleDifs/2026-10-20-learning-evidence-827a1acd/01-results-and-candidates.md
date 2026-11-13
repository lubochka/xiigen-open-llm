# Сохранённые результаты обучения и кандидаты на исправления

**Обновление:** первые разделы фиксируют шесть результатов core827; ниже добавлен раздел 6 о седьмом завершённом LF-03 на core6e3. Общее число завершённых опытов — 7. Сохранённые raw не менялись; текущий NEXT=N63-INGRESS-LENGTH.

**Прочитаны уже существующие результаты, новые обучения не запускались.** Исторический снимок core `827a1acdbef95d220886e54f9d36bce0b405c8cd` содержит завершённые №01–06 и тогдашний NEXT=LF-03. Последующее завершение LF-03 описано в разделе 6; текущий NEXT=N63-INGRESS-LENGTH. Успех фиксируется по любой полезной измеренной метрике и конкретной теме; рост accuracy не обязателен. Все регрессии и границы причинного вывода сохраняются рядом.

Ниже значения пересчитаны из настоящих A/B JSON. [Машиночитаемая сводка](evidence/saved-results-recomputed.json) содержит пути, Git blob SHA, SHA256, исходные величины и deltas. У N-KU сравнение строится из actual arm-A/arm-B; файл comparison.json оказался LFS pointer и не выдаётся за прочитанный payload. Полный исторический chain всех DLL/checkpoint binaries заново не реконструировался.

## 1. Шесть результатов, которые нельзя потерять или назначить повторно

| Опыт | Реальное обучение и полезный результат | Регрессия / граница вывода | Кандидат |
|---|---|---|---|
| N-KU-01 | `FileActionKnn.Train`, 4 урока: ложный ADD_LOOP на уже имеющихся циклах 3/3 → 0/3; correct 1/4 → 3/4 после обучения A/B | Нужный совет добавить цикл 1/1 → 0/1; все A-ответы ADD_LOOP, все B-ответы ADD_METHOD. Выигрыш объясняется изменением label majority; структурное обобщение не доказано. | Учесть DoStatement в учебной метке; не смешивать с отдельным ремонтом features. |
| NEURAL-FROZEN-TRANSPOSE-01 | 4 SFT/LoRA updates/arm; выделения −1 507 392 байта (−11,0449%); записанные state/loss/logits A/B совпали | Retained cache +249 856 payload bytes; CPU вырос; exact 0/2 → 0/2. NLL немного лучше после обучения в обоих arms, но это не преимущество B по качеству. | Кеш transpose только immutable frozen weights. |
| BPE-NO-MATCH-COPY-01 | Настоящий BPE Train; allocations code-doc −71,1436%, EN→RU −69,1078%; 64 learned merges и vocab 256→320 | В code-doc CPU 31,25→46,875 мс; по 16 input/output проверкам A/B token/state одинаковы. Меньше allocations не означает доказанный end-to-end KNN/LoRA quality gain. | Создавать новый chunk list лишь при первой настоящей merge. |
| N62-R1 | 2 DPO updates/arm; allocations −90 504 байта (−0,002404%); recorded state/metrics равны | GC0/1/2 каждый +1; target quality не измерено; причинное ускорение не установлено | Bitwise byte-span comparison frozen base вместо element LINQ. |
| N62-R2 | 2 DPO updates/arm; allocations −312 328 байт (−0,008296%); recorded state/metrics равны | Peak +30 474 240 байт; GC0/1/2 каждый +1; исторический B binary hash неполон | Один local softmax scratch на вызов backward, раздельно для chosen/rejected. |
| E-OPT-STEP | 2 DPO updates/arm; allocations −467 944 байта (−0,012429%); GC [193,19,13]→[189,14,10] | Peak +204 546 048 байт; raw factor/path ошибочно описывают R1. Восстановлена документальная source-связь с оговорками отдельного review; универсальная эквивалентность не доказана. | Читать optimizer step через проверенные metadata без копий m/v. |

Измеренные полезные результаты уже имеют статус кандидатов. Этот пакет дополняет их доказательства, не превращает их в новые эксперименты. Краткие решения для пользователя — в [CANDIDATES.md](CANDIDATES.md).

## 2. Все сопоставленные ресурсы рядом, включая ухудшения

TRAIN cost относится к actual update/train windows. Peak — whole process working set, не retained heap. Время/CPU/peak при одном фиксированном порядке A→B — наблюдения, не доказанное стабильное причинное ускорение. Полезные измеренные изменения остаются успехами своей оси. GC также не получает автоматически причинное объяснение.

| Опыт / профиль; ссылка на A raw | Allocated bytes A→B | Δbytes и доля A | TRAIN wall, мс A→B | TRAIN CPU, мс A→B | Process peak, bytes A→B | GC0/1/2 A→B |
|---|---:|---:|---:|---:|---:|---|
| [NEURAL-FROZEN-TRANSPOSE-01](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/A/code-sign.json) | 13 647 840 → 12 140 448 | -1 507 392 (-11.044913%) | 84.3133 → 84.57 | 62.5 → 78.125 | 37 105 664 → 37 031 936 | [3, 1, 0] → [2, 1, 0] |
| [BPE-NO-MATCH-COPY-01 / code-doc](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/code-doc.json) | 3 904 664 → 1 126 744 | -2 777 920 (-71.143637%) | 54.3095 → 34.3645 | 31.25 → 46.875 | 36 581 376 → 33 652 736 | [0, 0, 0] → [0, 0, 0] |
| [BPE-NO-MATCH-COPY-01 / en-ru](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/en-ru.json) | 2 892 824 → 893 656 | -1 999 168 (-69.107834%) | 44.9896 → 38.1428 | 46.875 → 46.875 | 35 467 264 → 33 419 264 | [0, 0, 0] → [0, 0, 0] |
| [N62-R1](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/A/arm-result.json) | 3 764 969 984 → 3 764 879 480 | -90 504 (-0.002404%) | 40 977.3226 → 24 298.8544 | 32 578.125 → 22 140.625 | 996 343 808 → 962 547 712 | [188, 12, 9] → [189, 13, 10] |
| [N62-R2](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/A/arm-result.json) | 3 764 947 024 → 3 764 634 696 | -312 328 (-0.008296%) | 40 459.8146 → 34 658.2724 | 34 593.75 → 30 546.875 | 949 817 344 → 980 291 584 | [188, 12, 9] → [189, 13, 10] |
| [E-OPT-STEP](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/A/arm-result.json) | 3 764 994 488 → 3 764 526 544 | -467 944 (-0.012429%) | 50 068.549 → 40 665.43 | 37 687.5 → 35 828.125 | 840 916 992 → 1 045 463 040 | [193, 19, 13] → [189, 14, 10] |

B raw, source SHA и точные неокруглённые deltas — в [сводке](evidence/saved-results-recomputed.json). Эти отдельные опыты не сравниваются между собой по времени как один A/B: они запускались в разные моменты с разными затратами и не образуют последовательное накопление всех оптимизаций.

У N-KU отдельно измерялись label+FeatureVector+Train: allocations 1 179 032→1 169 344; wall 195,8347→123,8038 мс; CPU 109,375→93,75 мс. Но послеобучающие запросы стали дороже по wall 22,405→26,0613 мс и CPU 15,625→31,25 мс; общий process CPU 500→546,875 мс. Memory payload estimate 496 bytes не изменился. Это наблюдения иных окон, не доказательство причинного ускорения label fix.

## 3. Что показывают before/after состояния

**N-KU:** до уроков guard возвращает null на 4/4 запросах; это не четыре реальных native Predict на пустой памяти. После каждого одного Train память содержит 4 строки. A выдаёт ADD_LOOP 4/4, B — ADD_METHOD 4/4. В пользу B свидетельствуют конкретные исправленные false positives; регрессию нужного цикла нельзя спрятать итоговыми 3/4. Источник: [A](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json), [B](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json), [отчёт](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md).

**Frozen transpose:** 4 updates меняют LoRA при неизменной базе; A1/B1 совпадают в архиве. Два exam NLL уменьшаются 8,530802→8,479319 и 7,499867→7,469800 одинаково в A и B. По правилу пользователя это полезное изменение учебной вероятности в обоих arms, без accuracy gain и без преимущества оптимизации по NLL. Отдельная сохранённая 400-step диагностика даёт partial 0/2 → 1/2, но не превышает constant-sign baseline и не суммируется с бюджетом этого A/B. Источник: [архивный отчёт](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/report.md).

**BPE:** до Train vocab 256, после 320 и 64 merges; оба профиля дают roundtrip 4/4 до и после. Суммарные token counts на четырёх случаях code-doc 172→87, EN→RU 192→87. Это эффект приобретения BPE merges в обоих arms. Вмешательство no-match-copy сохраняет learned state и token sequences, а выигрывает allocations. Downstream neural/KNN quality здесь не измерялось. Предыдущий Windows-path failure — ранняя история подготовки, не текущий terminal status этого завершённого опыта. Источник: [отчёт](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/report.md).

**R1/R2/E-OPT:** одинаковый исходный checkpoint, 2 lessons, 2 completed updates/arm, 16 из 16 adapter arrays changed, 0 из 39 base arrays changed. Сохранённые final hashes, optimizer reopen и reference совпадают. Урок 1, loss 0,91264373 и урок 2, loss 0,9189987 относятся к разным targets: это не before/after panel. Final margin −0,009629821 и pair accuracy 0 не показывают освоение темы; target readout отсутствует, новая TRAINv2 диагностика задним числом не приписывается. Источники: [R1](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/report.md), [R2](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/report.md), [E-OPT](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/report.md).

## 4. Новые уточнения доказательности R2 и E-OPT

В **R2** теоретически устраняются 128 float arrays, 322 048 байт payload; измеренная общая разница 312 328 байт меньше на 9 720. Причина разницы не установлена. Это не точное совпадение расчёта с allocation counter. В study первоначально одинаковые A/B source hashes, в manifest placeholder; фактическая source-правка найдена, но точный исторический B binary не восстановлен. Пользовательский запрет повторов сохраняется: пробел документирован, не лечится новым запуском.

В **E-OPT** actual raw под правильным experiment ID содержит `factor` и `measured_path` от **R1** (`DpoProjectLoraRecipient.RequireValid`). Нельзя молча править архив. [Адресный review source и происхождения](08-eopt-provenance-addendum.md) сопоставляет owner, source copies, build и raw metadata, оставляя остаточные ограничения. Сам факт неверного текста поля не доказывает, что был исполнен другой factor. Полезные GC1/GC2 deltas −5/−3 есть в raw, хотя исходный report показал только GC0 −4; здесь они восстановлены в таблице.

Фразы E-OPT «не меняет обучение совсем» и «детерминированно» шире прочитанного измерения. Корректный вывод: **в этой сохранённой маленькой паре записанные состояния/метрики совпали и измеренные allocations уменьшились**. Failure paths, все проверки/порядок отказа и большие модели этим опытом не проверены. Это ограничение кандидата, а не причина скрывать пользу.

## 5. Исходный RP01 и ранее успешная память

Первичные [C# scientific-result](evidence/rp01-csharp-scientific-result.json) и [EN→RU scientific-result](evidence/rp01-enru-scientific-result.json) повторно прочитаны как actual JSON с исторического core `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`; это сводки прошлых запусков, не новые native результаты. Полные вложенные score/grade/checkpoint chains заново не реконструировались.

| Ось | C# | EN→RU |
|---|---:|---:|
| Уникальные пары / новые updates | 500 /50 | 500 /50, продолжение C50 до global step100 |
| Изменённые LoRA / base tensors | 16 /0 | 16 /0 |
| Средний preference margin до→после | 0,0373330498→0,0385605812 | 0,0122930527→0,0124030590 |
| Улучшившиеся / ухудшившиеся cases | 14/25 /11/25 | 11/20 /9/20 |
| Chosen preferred до→после | 16/25→16/25 | 12/20→12/20 |
| Правильные свободные ответы | 0/3→0/3 | 0/4→0/4 |
| Full state reopened | Да, по summary | Да, по summary; referenceC50 unchanged |

Рост mean margin — полезный результат конкретной оси. Отсутствие правильной генерации ограничивает другой вывод. Исходный savedB0 был random/untrained; EN→RU стартовал с C50. Нет основания объявить полное отсутствие обучения или одну доказанную причину слабого качества. Источники и source chain: [предыдущий полный primary readback](https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/16-RP01-primary-readback.md).

Ранее выполненный **P125 EN→RU memory** дал 0/24 → 19/24 после 8 уроков; отдельный **P25 coldload** сохранил 24 ответа и 19 correct. Это настоящее приобретение и чтение memory-state. Уже внесённое 18 октября исправление загрузки `phrase_mappings` дало 11/24 → 19/24, одновременно wrong-accepted 1→3; оно не выдаётся за новый план внедрения. [Исторический разбор и первичные ссылки](06-historical-evidence.md) сохраняет различия между memory, AST/POS KNN и LoRA.

Решение о полном product repair остаётся за пользователем. Этот аудит не изменял исходный код, tests, веса или защищённую папку и не выполнял ни один из будущих опытов.

## 6. Поздний завершённый LF-03: отдельный седьмой кандидат

Сохранённые [A](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json) и [B](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json) имеют COMPLETED/error=null, по одному issued/completed StepVector. Новый запуск в этой работе не выполнялся. Удалена только внешняя копия initialGradient в Tensor.Backward; защитная копия TensorStorage остаётся. Это не будущий peer GRAD-ACCUM-COPY-01, который меняет другой метод TensorOps.AccumulateGrad.

| Показатель | A | B | B−A |
|---|---:|---:|---:|
| TRAIN allocated bytes | 1 848 491 176 | 1 847 844 360 | **−646 816** |
| TRAIN elapsed, мс | 15 270,2952 | 17 178,4596 | **+1 908,1644; хуже** |
| TRAIN CPU, мс | 14 406,25 | 15 015,625 | **+609,375; хуже** |
| GC0 / GC1 / GC2 | 93 / 8 / 6 | 90 / 5 / 3 | **−3 / −3 / −3** |
| Whole-process peak, байт | 830 795 776 | 787 300 352 | −43 495 424 |

Есть и настоящий before/after TRAIN-сигнал: first-token NLL **6,814549566480718 → 6,814264388510441**, дельта −0,000285177970277 в обоих плечах. Rank остаётся 221/629, exact_first_token=false. Это успех обучения по NLL первого токена, не измерение NLL всего ответа. **Разность учебных дельт между B и A равна нулю**: правка не показала дополнительного выигрыша качества. EXAM/Generate равны нулю по бюджету.

Все 16 adapter arrays изменились,0 из 39 base arrays; recorded final state и панели A/B совпадают. Сравнение опирается на сохранённые hashes/receipts; внутренние binary payloads не переаттестованы. Source-manifest сохраняет APPLIED_AFTER_FACTOR_EDIT вместо hash изменённого B/Tensor.cs; поэтому полный historical B source→binary provenance не доказан. Наблюдённая польза allocations/GC/peak сохраняется, причинная уверенность подписана отдельно.

Расчёт убираемого payload: 2×4×121×629 = 608 872 байта, измерено 646 816; остаток 37 944 байта не разложен по объектам. Одна пара не подтверждает универсальное «детерминированно647КБ» или полную эквивалентность всех callers. Смена знака временной дельты относительно прежних опытов также не доказывает, что фактор не влияет на время; она ограничивает причинную интерпретацию фиксированного A→B. Исходный report не переписан, повтор не назначен.

Кандидат **C-07** сохранён для решения пользователя. Приоритет actual receipts над запоздавшим index устанавливает **NEXT=N63-INGRESS-LENGTH**. [Пересчитанный JSON](evidence/LF03-saved-results-recomputed.json), [последняя сверка](11-late-reconciliation-6e3-eaed.md).
