# 91. Последующие переносы: что подтверждено, что осталось и где изменился допуск

Срез: 28.10.2026, последний прочитанный HEAD `203ba60fc487ce47fe3415b0a4abb7ce95bc13e4`. Фаза — READ_ONLY_RESEARCH и правка Markdown. Исходники, сохранённые JSON, журналы и Git прочитаны; новые сборки, тесты, обучение и генерация не выполнялись. Это дополнение к [84/85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), а не повторное назначение уже реализованных изменений. Новый остаток — [92](92-positive-transfer-followup-plan-2026-10-28.md).

## Решение по допуску

**P08: полезный эффект сохранён, общий перенос HOLD по памяти.** Вероятность правильного знака T1 выросла с 0,86879754 до 1: +0,13120246 абсолютной вероятности, около +15,10% относительно A. Фактические ответы остались 2/4. Новая ресурсная запись показала managed allocations 10120→25872 байта на вызов на T2/E1/E2: +15752 байта, +155,6522%. На T1 медиана B 25936, средняя парная разность 15803,2; эти два способа агрегации не смешиваются. Рост существенно больше 5%, поэтому CPU-исключение из [69](69-transfer-admission-and-priority-2026-10-27.md) не даёт общего допуска. Наличие уже внесённого кода не доказывает прохождения допуска и не является поручением откатить код в исследовательской сессии.

Измерен весь sampler на сохранённых logits обученного P08: 800 warmup +8000 measured calls, одинаковые RNG draws, строгая CDF в обоих плечах, пять блоков AB/BA. Счётчик `GC.GetAllocatedBytesForCurrentThread` охватывает вызов, запись чисел идёт после окна: [Program.cs](../../../../experiments-v2/RP01-MICRO/P0801/sampler-resource/Program.cs), строки 541–565. Peak процесса 40992768 байт — другая величина, это не цена одного вызова и не сравнительный peak A/B. У первой строки заметен прогрев; её точное отношение wall-time не переносится на устойчивый режим. Направление роста allocations устойчиво по сохранённым блокам. Источники: [receipt](../../../../experiments-v2/RP01-MICRO/P0801/results/attempt-0001/sampler-resource-0001/receipt.json), [поправка прогрева](../../../../experiments-v2/RP01-MICRO/P0801/results/attempt-0001/sampler-resource-0001/warmup-trend-correction.md), commit `203ba60fc487`. Ресурсная ось больше не NOT_MEASURED; измерение не повторять ради прежней формулировки плана.

## Проверенные изменения и пределы доказательства

| Основание | Что сделано в коде | Что подтверждает прочитанное | Оставшееся действие |
|---|---|---|---|
| P02, `c26a90f6bccf` | Разделены три разных показателя; внешний writer вызывает общие определения; неизвестное время SFT nullable | `P0201/Program.cs:443,495–497,590,706`, новый builder и append-only corrected-reading; сохранённый журнал 15 passed/exit0 | R28-M1/M2/W1 не назначать повторно; это ремонт диагностики, не новый выигрыш обучения |
| CDF, `9a199e5bd608` | `Sampler.SampleFromDistribution` использует `r < acc` | `09.Inference/Sampler.cs:108–121`; сохранённый журнал 26 passed/exit0 | Устаревший oracle `UniformToken` в seeded tests ещё использует `<=`: R30-S0 |
| P08 consumer, `d9a42e6cc7c1` | Восстановлен S1, выполнены настоящие A/B TextGenerator с прежним A и экспериментальным B | 8 Generate/8 prefill/8 Sample; все ответы совпали с исходными; optimizer404; S1 weights/optimizer/RNG hashes сверены чтением файлов в этой работе | Это не отдельное обучение и ещё не цельная цепочка с нынешним native B |
| P08 native composition, `563e9b7f96de` | TopKMask → TopPFilter(masked logits), без softmax от вероятностей | Самpler реализован; сохранённый журнал 41 passed/exit0; sensitivity log 37 passed/4 failed на legacy | Четыре generator tests заменяют фактические logits сохранёнными: R30-S1. Общий допуск HOLD по allocations |
| P09, `d039923dc223` | Реальный learned S1 включён в empty-cache regression; неверная ресурсная интерпретация исправлена | 4 ordinary +4 empty-cache Forward, 21504 побитовых совпадения; сохранённый журнал 11 passed/exit0 | Learned chunked path не проверен: R30-C1. Это дополнение теста, не новый cache-алгоритм |

Журналы — исторические результаты исполнителя, прочитанные в этой работе; повторного запуска родитель не делал. Адреса: [P02](../../../../experiments-v2/_runners/Run-PreferenceSeparationTests-r28.log), [CDF](../../../../experiments-v2/_runners/Run-SamplerBoundaryTests-r28s0.log), [composition](../../../../experiments-v2/_runners/Run-SamplerCompositionTests-r28s1.log), [cache](../../../../experiments-v2/_runners/Run-CacheAwareForwardTests-r28c.log). Числа не означают независимый повтор аудитором.

## R30-S1. Цепочка learned model → native sampler разорвана в тесте

[P08SamplerCompositionTests.cs](../../../../Tests/ExperimentFixes/P08SamplerCompositionTests.cs), строки 62–117: создаётся `CreateCarrierModel`; delegate принимает `carrierLogits`, проверяет только длину и вызывает `Sampler.Sample(ToTensor(savedRow.Logits), ...)` в строках 89–93. Такой тест полезен для wiring генератора и native sampler на записанном обученном распределении. Он не проверяет, что нынешний продукт читает восстановленный S1 и передаёт именно его результат в native sampler. Ранее выполненный consumer восстановил S1, но B был экспериментальным sampler. Эти два доказательства нельзя объединять в утверждение о выполненной единой цепочке.

Нужен узкий ремонт четырёх уже имеющихся случаев без нового SFT и без повторения старых восьми A/B вызовов; точные файлы и бюджет в92. Ошибка реализации sampler из этого пробела не следует.

## R30-C1. Обученный empty cache не закрывает обученное чтение по частям

[CacheAwareForwardTests.cs](../../../../Tests/CacheAwareForwardTests.cs): старый chunked тест строки39–54 использует случайную TinyConfig. Новый learned Fact строки300–443 загружает S1, проверяет исходное обучение400→404, хеши и все full/prefill позиции, но не вызывает chunks. Его доказательство корректно для empty prefill. Для chunked пути на том же выученном состоянии остаются абсолютные позиции, GQA cache и удержание T1. План92 дополняет этот же Fact, используя уже вычисленный full baseline. `AssertLogitWindowClose` строки550–576 имеет абсолютный допуск2e−5; не подменять его побитовым обещанием для иного порядка вычислений.

## R30-S0/R1. Oracle и квитанция не должны обещать больше наблюдаемого

1. [TextGeneratorSeededSamplingTests.cs](../../../../Tests/TextGeneratorSeededSamplingTests.cs):252–262 по-прежнему выбирает `randomValue <= acc`. После strict-CDF ремонта helper неверен на точных границах положительных интервалов. Текущие случайные seeds могут этого не проявлять. Меняется независимый oracle и адресные boundary cases, а не ожидаемые ответы произвольно.
2. [consumer Program.cs](../../../../experiments-v2/RP01-MICRO/P0801/consumer-completion/Program.cs):108,417–418,437,453,509: `_updates` объявлен, но не увеличивается; сравнение с нулём само ничего не наблюдает. Реальная проверка optimizer step и hashes остаётся полезной. Это уже признано исполнителем в [evidence-boundary-correction](../../../../experiments-v2/RP01-MICRO/P0801/results/attempt-0001/consumer-completion-0001/evidence-boundary-correction.md), не новое открытие аудита. Остаток — явное назначение ремонта writer: measured/structural/unknown и источник каждой величины. Неизменные веса не доказывают отсутствие Backward.

## Границы нового поиска

P13/Q014 проверяет уже работающий отказ nonfinite на LogicalDpo ingress. Нельзя переносить дополнительный B-validator как успешное улучшение, если до него обе руки одинаково отвергнуты. Соседние локальные ветки и старые E51 проверены отдельным исследовательским worker; точная граница и дедуп с библиотекой — [пакет источников](reviews/2026-10-28-positive-candidate-delta-source-audit.md). Старые проблемы oracle, одинаковых CodeContext и фиктивных baseline не объявляются новыми успешными алгоритмами.

## R30-E1. Ранее известная ошибка E51 получила конкретный адрес ремонта

Worker `/root/p08_transfer_audit` сверил46 локальных refs/37tips; незлитых branch/remote commits после15.10 не найдено. `--all` отдельно показывал stash/index snapshot, не новый опыт. Исторические E51A/E51B/E52/E53/P3/T34/T35 не являются недостающим пакетом алгоритмов: сопоставленные E51A/B/P3/T34 исходники отличаются от старой ветки лишь заголовками SPDX. Покрытие поиска и ограничения описаны в его пакете; это не утверждение о всех когда-либо существовавших удалённых ветках.

E51A действительно учит память27строк и получает required-predicate overlap0,205128205→1; прежний baseline выбирает один predicate по ExpectedDomain. E51B trace6/6 сохраняется, но baseline0,333 задан литералом. В обоих опытах времена0,1мс — литералы. Это уже известно библиотеке11/15/01a, поэтому новым открытием не называется. Новый остаток в плане переноса — полный тракт квалификации отчётов: `E51ACodeContextPipeline.cs:114–115,145–159`, `E51BDynamicFlowMiniE2E.cs:116–124,151–165`, общий `ExperimentRunner.cs:33–51,93`, `Infrastructure/ResultWriter.cs:46–118`. Родитель повторно прочитал места присваивания и generic writer/console. Вычисленные `Passed/NextAction` из synthetic baseline не являются измеренным допуском.

[92/R30-E1](92-positive-transfer-followup-plan-2026-10-28.md#r30-e1--квалификация-e51-без-фиктивного-сравнения) назначает только E51-specific projection и перекрытие legacy export/advance, без изменения общего ResultTypes и без нового Train. Это разрешённый пользователем логический ремонт диагностики уже обученных результатов. Ремонт не повышает E51 до общего алгоритмического допуска; causal A/B/C уже принадлежит библиотеке15.

Новые алгоритмические FIX-ID по данному срезу не добавлены. Существующий первоочередной учебный кандидат P12 сохраняется в [88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md), с узким step0 и AdamW400/scheduler0. Общая маршрутизация домен→модель→функции остаётся в [74](74-knn-domain-constructor-transfer.md); P08 не доказывает доменное ранжирование или качество маршрутизатора.
