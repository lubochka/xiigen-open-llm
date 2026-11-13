# Сверка старых положительных результатов с владельцами переноса

27 октября 2026. Исследователь: `/root/knn_repair_followup`. Режим **READ_ONLY_RESEARCH + этот Markdown**. Начальный прочитанный HEAD `797dfb5702c6d67f025164630352bf311e0b8e78`; последующий read-only HEAD `7e50b52283c822ec4afa5b4150989b8542e8dcfd`. Репозиторий меняется параллельно; это срез прочитанных файлов рабочего дерева, не аттестация чистого checkout или всех изменений между этими SHA.

**В просмотренном старом списке не найден новый положительный перенос, которому одновременно отсутствует owner-план и не мешает установленное ограничение допуска.** Новых FIX-ID или owner-документов этот review не назначает. Это вывод о покрытии списка планами, а не доказательство реализации, завершённости продуктовой интеграции или повторная проверка всех исторических runtime.

Открытые ограничения сохраняются:

- **OPT01/FIX-OSR и BWS01/FIX-BWS нельзя автоматически продвигать как допустимые малые улучшения.** У обоих сохранён выигрыш allocations, но медиана process peak выросла более чем на5%. Числа и неопределённость приведены ниже. Нужна явная отметка HOLD для общего переноса по действующим критериям69; новый эксперимент этим не назначается.
- **HCL и SVD имеют владельцев ремонта экспериментальных модулей, но не доказанного production receiver.** Это уже записанный пробел продуктового плана, а не обнаруженный здесь новый механизм. SVD дополнительно удваивает retained embeddings.
- **Поздние collision-сравнения не становятся общей учебной политикой из-за положительной отдельной метрики.** Отозванные выводы134–138 и встречные ухудшения144 остаются ограничениями существующих59/63.
- P06/P07 и последние изменения их первичных источников исследует родитель `/root`; данный review учитывает наличие строк, но не выдаёт независимый вердикт об их содержимом.

## 1. Что именно сосчитано и сопоставлено

Источник списка — [execution index](../../experimets/cards/00-execution-index.md), [required-changes](../../required-changes.md), действующий [реестр01](../01-registry.md), [карта62](../62-successful-experiment-plan-coverage.md), [критерии69](../69-transfer-admission-and-priority-2026-10-27.md), адресные владельцы и дополнения57/59/63/68/74/76/79/80/81.

| Проверяемый набор | Наблюдаемый счёт | Значение и ограничение |
|---|---:|---|
| Строки index с подстрокой `RESULT_RECORDED` | 158 | Включает `RESULT_RECORDED_PARTIAL`; наличие результата не означает SUCCESS |
| Уникальные первые ссылочные ID этих строк | 157 | TTQ02 представлен дважды: Q001 и историческая строка без номера |
| Строки с числовым первым столбцом среди этих158 | 143 | Это1–144, кроме132; отсутствие132 здесь не означает отсутствие его результата |
| Нечисловые RESULT-строки | 15 | Q001–Q008 и7исторических строк;14уникальных ID |
| Числовые записи карты62 | 143 | Полный набор1–143;132/DQ01 явно разобран в62:172 |
| Заголовки второго уровня required-changes | 147 | Это структура документа, не число успешных опытов, независимых проверок или запусков |
| Строки stable ID реестра01 | 62 | 60старых числовых строк + FIX-KDP и E2E-KDR |
| Уникальные пути владельцев этих62строк | 56 | Все56целевых файлов существуют; наличие файла само по себе не доказывает достаточность плана |

Распределение143числовых RESULT-строк:1–131 и133–143 уже находятся в карте62;144 отдельно назначен63§8 и69:47.132/DQ01 не потерян: приобретённые состояния связаны с единственным FIX-SEP62 и его квалификацией; новый owner или второе обучение для него не нужны.

Нечисловые записи сверены по смыслу, а не только по отсутствию номера:

| Записи | Уже существующее назначение / исключение |
|---|---|
| TTQ01/SC02 | Внешний41, FIX-SC; карта62:71. Terminal qualification не подтверждена |
| TTQ02, включая повтор Q001 | FIX-SEP62,68/U7 и последующие ремонты; acquisition T1 не означает успешной terminal qualification |
| FTR01 | FIX-LFT/55, собственное позднее квалифицированное основание; не наследовать квалификацию раннего LF03 |
| PNL0407 |04:17,459 и отдельный будущий Q126. Чтение сохранённых состояний с0correct не доказывает отсутствия прежних updates и не квалифицирует ранние resource A/B |
| WM02 | FIX-WT/14; карта62:42.5/8 у обоих, false selections растут: безопасной общей margin-policy не найдено |
| KDP01/KDR01 | Два существующих ID с owner74; их незакрытые source/oracle/restore-контракты уже в77/79/80, повторный план не создавать |
| P01–P05 | Ремонты68/76/77/79/80; результаты не означают допущенный новый общий default |
| P06/P07 | Строки Q007/Q008 учтены; независимый первичный аудит этой части — вне данного worker scope |

Девять владельцев из соседнего каталога `fixes-based-on-experimentsperiments` являются действующими ссылками реестра. Их нельзя выбросить как «архив с опечаткой» и затем объявить соответствующие механизмы незапланированными.

## 2. Первичная проверка наиболее вероятных пропусков

Следующие выводы опираются на адресное чтение raw/report/source и текущего owner. Прочие строки списка сопоставлены на уровне реестра, карты покрытия, required-changes и существующих отчётов; все157уникальных ID заново на уровне первичных файлов не аттестованы.

### PA01: принятие обученного parent уже входит в FIX-MC

Первичные источники: [PA01 report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/results/report.md), [A raw](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/results/A/arm-result.json), [B raw](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/results/B/arm-result.json), [handoff](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/results/accepted-parent-handoff.json), [host source](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01/host/Program.cs).

Raw подтверждает выбор ordinal1→2 и optimizer start0→1000; после двух local updates шаги2/1002. TRAIN response NLL до продолжения6.689561930734801→4.766696420187372, правильные response tokens0/40→3/40. Полные TRAIN-ответы остаются0/2 до и после, EXAM correctness не улучшена. Следовательно, это полезная discovery/adoption delta сохранённого состояния, а не доказанное усвоение полного ответа за два новых урока.

Source Program:115–118 создаёт одинаковые baseline/candidate,138–141 проверяет discovery до acceptance;190–197 публикует candidate только вB и вызывает общий `PrepareStartLatest`;200–223 сохраняет handoff до продолжения;226–239 выполняет ровно два SFT updates. Handoff содержит `FROZEN_BEFORE_PARENT_CONTINUATION`, ordinal2, optimizer1000, localUpdates0. Исторические1000updates не являются новой стоимостью этой проверки и не разрешают повтор producer.

**Существующий owner [21§8](../21-sft-optimizer-continuity.md):177–188 уже назначает требуемый перенос**: native latest discovery до публикации/после recovery, ordinal отдельно от optimizer step, повторный selector без update, отказ corrupt latest. Всё добавляется в существующий `ServerOwnedSftRoute_PublishesAndColdResumesWithoutRetraining` на уже созданных accepted states. Дополнительный бюджет этого дополнения0Train/Forward/Backward/Generate,0test processes. Поэтому отдельный importer, publisher, fixture, PA1000 или новый ID здесь дублировал бы план. Native product qualification не вытекает из private H16 host.

### BWS01: малый allocation gain покрыт, peak-regression исключает общий допуск

Первичные источники: [verdict](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BWS01/results-01/verdict.json), [bridge](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BWS01/Bws01Bridge.cs); владелец [60](../60-backward-softmax-scratch.md).

В verdict шесть сохранённых runs A/B/B/A/B/A, по10updates. Поля start/final hashes, losses и correctness совпадают; сохранён learned start66correct tokens по двум строкам. Bridge:191–194 создаёт project-LoRA trainer,198–225 измеряет окно,209 вызывает настоящий `StepVector`; это не только пустой scratch microbenchmark. Эта проверка читает сохранённые данные и код, не повторяет60updates.

Медиана allocated/update1,879,189,192.8→1,879,024,291.2B, выигрыш164,901.6B или0.0087751463%. Но process peak median **1,378,791,424→1,598,201,856B, +15.9132432%**. Перекрытие диапазонов и шумный process-wide peak не позволяют приписать весь рост scratch-изменению; одновременно они не доказывают безопасный≤5%профиль. CPU-исключение на RAM не распространяется. Консервативное решение по69 — HOLD общего переноса, сохраняя положительную allocation-ось.

Owner60:27 уже явно показывает обе медианы и ухудшение.60:35–39 ограничивает код одним local scratch на backward без cache/ArrayPool,60:47 назначает неизменный BWS train fixture,60:53–63 — native contract/learning tests;60:77–89 задаёт бюджет, receipts и reuse результата в E2E20 без нового обучения. Это существующий полный план выбранного механизма, которому требуется применять актуальную границу допуска; нового owner нет.

### OPT01: ресурсный результат тоже не означает автоматический допуск

Первичные источники: [verdict](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/OPT01/results-01/verdict.json), [bridge](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/OPT01/Opt01Bridge.cs); владелец [58](../58-optimizer-step-metadata-read.md).

Сохранённая медиана allocated/update1,879,191,334.4→1,878,804,284.8B:−387,049.6B, −0.0205966041%. Verdict отмечает одинаковые learning states во всех runs и квалифицированный старт. В source:258 указан измеряемый native путь `ReadOptimizerStepForRandomState → AdamW.ReadPortableMetadata`; общий вывод о скорости всего обучения из этого не следует.

Из сохранённых `runs[].peak_working_set` отдельно пересчитана медиана: **1,280,729,088→1,362,112,512B, +6.3544605%**. Это выше5%; диапазоны перекрываются, причинная атрибуция peak не установлена. Owner58:28 уже предупреждает о более высоком максимумеB и отсутствии доказанного peak gain, но явная медианная граница полезна для admission69. Не включать OPT01 в новую допущенную очередь только за малое уменьшение allocations.

Owner58 уже назначает native metadata read, qualified fixture, tests, неизменность optimizer/RNG и final state.58:117–129 ограничивает основной A/B20updates/80training Forward/40Backward/16diagnostic Forward, не выдаёт существующий planned runner за готовый. Новый owner, повтор раннего E-OPT или отдельный microbenchmark не требуется. Уточнение HOLD не разрешает запуск этих будущих20updates.

### HCL01: предметный успех reader имеет владельца, production-связь остаётся открытой

Первичные источники: [verdict](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HCL01/results/attempt-0001/verdict.json), [report](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HCL01/results/attempt-0001/report.md), [Program](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HCL01/Program.cs); владелец [53](../53-hierarchical-local-history.md).

Сохранено canonical correctness10/16→16/16 и semantic invariance при перестановке local ID4/16→16/16. Последнее — **согласованность предсказаний при перенумерации**, не accuracy4/16: уA accuracy canonical/permuted обе10/16, меняются ошибочные направления переходов. Source:344–353 запоминает последнее наблюдаемое локальное состояние каждого кластера;385–404 сравнивает неправильный `curLocal` с history именно predicted cluster. Получателем обученных таблиц является этот reader. Raw содержит24native Train/208native Predict и128exam queries; отсутствие target history не встретилось.

Owner53:11 и184–190 уже ограничивает назначение E05: существующая цепочка `ExperimentRunner → ExperimentsDispatcher.RunE05 → E05HierarchicalPrediction.Run`, отдельный fixture7events/152tokens/16queries, semantic permutation, Observe/Predict/Reset и native tests.53:3–5 учитывает опубликованные ремонты и сохраняет незакрытую историческую панель. Production prompt→domain consumer не установлен. Этот открытый пункт уже явно зафиксирован картой62:185; общее упоминание E2E20 его не закрывает. Новый product route без найденного вызывающего кода и отдельного основания сейчас был бы придуманной интеграцией.

SVD здесь повторно на raw/source не проверялся: карта62:80,186 и внешний owner53 уже сохраняют directed-target успех2/5→5/5, удвоение retained embeddings120000→240000B и ограничение E10. Это известный PARTIAL/HOLD product-кандидат, не новая недокументированная возможность.

## 3. Остальные группы старых результатов

| Группа | Существующий владелец | Почему не добавлен новый перенос |
|---|---|---|
| Ранние N62-R1/R2, E-OPT, LF03 |54/58/60/61/55 и квалифицированные поздние FBQ/OPT/BWS/LFG/FTR | Ранние resource observations не получают acquired-task proof от0→0 или PNL readout; поздние основания уже имеют планы |
| BPE copy/weighted chunks, Wiki once/keys, AST, KNN/HNSW/PQ/identity |12/31/33/32/38/24/26/29/35/49/50/51/52 | Owner-ссылки присутствуют; предметные ограничения и ресурсные обмены сохраняются. Все raw этой группы заново не читались |
| E25/E09/E22 и P125/P25 |34/37/27/19 | Уже назначены точные наборы и учебное покрытие; рост readout-cost, synthetic scope и оставшиеся ошибочные admissions не скрываются |
| Collision1–143, включая paired/discrete readout |17/59/62/63 | Общие schedules не допущены по одному endpoint;133 — исправление reader,141 — согласованность paired effect,142/143 — границы contrast и retention |
| CDG68/144 |63§8 и69:47 | Sign14/21→20/21 полезен, но Magnitude mean gold rank42.137931→54.620690 ухудшается29.62%; обе руки290updates. Общий dose scheduler HOLD |
| KDP/KDR диагностика и restore |74/77/79/80 | Уже существующие исправления и незакрытые контракты, рассмотренные в отдельном [source review](2026-10-27-knn-repairs-followup-source-review.md); не новый owner |

Это таблица назначения и ограничений, не157независимых исследований, не62runtime-проверки и не свидетельство полной готовности всех56владельцев.

## 4. Результат и граница продолжения

Для родительского планирования: **0новых owner-планов из данного старого среза**. Сохранять62stable ID, не создавать дубликаты MC/OSR/BWS/HCL. В admission-дополнении можно адресно закрепить обе raw peak-медианы OPT/BWS и HOLD; это изменение документа, не разрешение runtime. HCL/SVD остаются известными открытыми product-boundary, а не закрытым end-to-end переносом. Новые результаты P06/P07 должны получить самостоятельную первичную проверку родителя.

Проведены чтение источников, разбор сохранённых JSON, арифметика и запись этого Markdown. Не запускались builds/tests/training/inference, не создавались fixtures/hosts, не изменялись product/runner/data/weights, реестр или владельцы. Hash-custody всех старых бинарников, совместимость56owner-планов с каждым текущим source change и все исторические run logs здесь не проверены.

Прочитаны целевые разделы AGENTS/CLAUDE и skills архитектуры, планирования, review, читаемости и prompt-authoring; полная перезагрузка гигантских инструкций не заявляется. Context-mode применялся для начального компактного исследования; последующие адресные read-only чтения и подсчёты выполнены непосредственно по файлам. Дополнительных агентов этот worker не создавал. Это один исследовательский пакет `/root/knn_repair_followup`, а не несколько независимых ревью.

## 5. Последующий адресный review82/83 и owner-дополнений

После написания основного пакета родитель добавил [82](../82-positive-transfer-delta-audit-2026-10-27.md) и [83](../83-learned-bpe-boundary-transfer-addendum.md). По отдельному поручению прочитаны их тексты, новые вводные20/22/28/58/60/62/74, соответствующие участки native carrier/tokenizer/score и existing tests. HEAD при чтении — `7e50b52283c822ec4afa5b4150989b8542e8dcfd`. Новые документы находятся в рабочем дереве; совпадение HEAD не означает, что они входят в этот commit. Исходники P06 и полный набор остальных требований владельцев в этот узкий follow-up повторно не проверялись.

**Конкретной ошибки в назначенной границе не найдено.** Это статический review плана, не выполненный fixture, не тестовый PASS и не runtime qualification.

| Проверенный пункт | Первичная опора | Вывод |
|---|---|---|
| Causal target shift | `11.DPO/DpoTrainer.cs:1116–1148`: mask и IDs берутся на `i+1`, logit row — `i`; `Tests/DpoExactPairMaterialTests.cs:173–202,456–457` |83:43 верно задаёт `(j−1, IDs[j])`. ASCII `[97,98]` имеет `(row0,98)`; RU `[208,176,208,177]` с маской`[0,0,1,1]` — `(row1,208),(row2,177)` |
| Независимый Score oracle V257 | Тот же native `ComputeTargetLogProbs`, вызванный existing `Score(...,sum:true)` через reflection | Для каждой оцениваемой row с одним logit1 и256logits0 ожидается `1−log(exp(1)+256)`; сумма для n targets — формула83:44. Это вычисление по заданному tensor, без model Forward/Generate; не accuracy и не end-to-end DPO parity |
| Learned merge и byte-gold | Сохранённые `P0701/results/attempt-0001/gold-oracle.json` и `tokenizer-trained-state.json`; `BpeTokenizer.cs:42–79,125–143,556–575` | В saved state merge97,98→256; Train устанавливает byte vocab и merges. Canonical Base64 Vocab хранит фактические bytes. Ошибка длины через per-token Decode подтверждается статической цепочкой `P0701/Program.cs:585–606 → BpeTokenizer.Decode`; новые RU/HE cases остаются NOT_RUN |
| Native guard уже существует | `DpoExactPairMaterial.cs:30–46,64–90,120–146,166–181` | Private carrier кодирует три сегмента отдельно, требует непустые response lengths и round-trip; Validate сравнивает точные IDs и masks.82 не назначает ложный новый runtime guard и не меняет framing |
| Existing learned segmentation test | `Tests/DpoExactPairMaterialTests.cs:49–58` | Тест уже выполняет настоящий BPE Train и сравнивает joint/segmented IDs.83 усиливает связь с независимым byte/target oracle, не заявляет, что такого native покрытия вообще не было |
|35Encode и8materializations | Test `Batch`:395–398 напрямую вызывает native CreateBatch; CreateBatch:72–74 делает ровно3Encode через EncodeSegment:142 |3before +4×3native +4×2joint +4×3native послеLoad =35. Train сам не вызывает public Encode. Внутренние Save/Load capture и внешнее восстановление учитываются отдельно; companion tests/baseline явно не выданы за нулевую стоимость |
| Один владелец и разные framing | Новые вводные22/28/62 и83:17,58–60 | Receipt принадлежит FIX-PD; FIX-TS/FIX-SEP переиспользуют его. Joint-LF SFT не подменён segmented DPO, KNN-only профилю BPE не навязан. Product/route learning этим не квалифицирован |
| Явный ресурсный HOLD |58:3,30;60:3;82:53 | Родитель внёс запрошенное уточнение: OPT01+6.35446%peak и BWS01+15.91324%peak, с сохранением allocation gain, неопределённости причинности и запрета автоматического общего переноса. Указанная в§1–4 необходимость явной отметки теперь документно выполнена |

Стоимость35Encode относится только к новой связанной fixture за один будущий запуск и предполагает reuse уже полученных IDs для assertions. Она не разрешает добавлять повторные Encode для удобства, новые model calls или переобучение источника. Материалы до и после restore могут проверяться повторными assertions без нового Train. Baseline/companion tests сохраняют отдельный фактический счёт, как прямо требует83:72.

RU/HE байты в83 — защитные literal cases на той же один раз обученной ASCII BPE table. Их корректность по конструкции не представлена как измеренный выигрыш RU/HE домена, перевода, модели или router. Принадлежность этого нового покрытия существующим ID не расширяет допустимость старых resource-переносов PD/TS и не снимает иных условий69.

В этом follow-up изменён только данный review. Builds/tests/train/inference, native tokenizer fixture и Score runtime **не выполнялись**. Подтверждено согласование записанного будущего контракта с прочитанным source; реализация helper, численная погрешность фактического исполнения и прохождение тестов остаются непроверенными.
