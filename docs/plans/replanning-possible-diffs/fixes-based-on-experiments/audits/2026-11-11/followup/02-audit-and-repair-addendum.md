# Аудит переносов: проверка исполнения, усиление плана и рабочих карточек

**11 ноября 2026. Итог документного наряда: проверенные выводы сохранены, план уточнён, действующие карточки изменены. Продуктовые ремонты и новая native приёмка остаются отдельной работой.**

Это продолжение [аудита F01–F13](../2026-11-11-experiment-transfer-audit.md) и [плана из 14 шагов](../repair-plan.md), опубликованных предыдущей сессией в core d9fc526b61a11312baefef5b1246064552bc4c85. Нумерация находок сохранена. Новые границы существующих находок и дефекты самого проекта ремонта перечислены отдельно; число ошибок не увеличивается повторным названием одного механизма.

**Поздняя сверка перед публикацией:** ветка продвинулась до 05caa77; параллельный DRAFT6a6628 добавил PlanAdmissible без product wiring. B01 теперь IN_PROGRESS. Последующий45cd22 изменил только доску: добавлены B13–B16, уточнён B03 и признан непригодный знаменатель нового решения; продуктовый код05caa не изменился. В его новом resource обосновании повторился F02; карточка109 дополнительно уточнена. [Отдельная delta с источниками и границами](05-late-b01-delta.md) имеет приоритет для этого нового состояния.

## 1. Источники и достаточность проверки

Исходный код и сохранённые результаты закреплены на core **55cfd9d00bbebdeae04da7c505b84d0b1cb9b747**. Planning исходно читался на f7a05c7dc13a335006caa4cd8ef6695adce27bf0; перед правками учтена параллельная публикация **3fb0519e4a37e72bcf65b4dde85bab154d23511f**. Core d9fc менял документы, а не проверенный код. Этот факт проверяется по составу commit; он не заменяет сверку новой версии перед будущим исполнением.

Список 124 work-строк реестра и 131 Q-строки указателя — **инвентаризация**, не 124/131 полноценно повторённых экспериментов. Глубокий охват этого продолжения: P48/P49/P50/P52/P54/P58, Wiki feature reuse/эпохи, PromptFlow active target/body, correction provenance, probe loss, evidence sink/finalizer, batch inventory и ограниченный текстовый proof ROW124. Остальные карточки не объявлены научно аттестованными.

В двух основных воспроизводимых расчётах проверены **58 уникальных входных файлов**: девять в product-recomputation, 50 в evidence-recomputation с одним общим исходником P52. Для каждого проверены Git blob SHA1 и SHA256; после чтения входы неизменны. В поздней delta отдельно пересчитаны ещё два новых JSON receipts на pin05caa: суммарно60 уникальных inputs трёх расчётов. Это число входов расчёта, не число полностью прочитанных документов. [Product receipts](product-recomputation.json), [evidence receipts](evidence-review-portable-recomputed.json).

| Уровень | Что выполнено | Что этим не подтверждено |
|---|---|---|
| Source review | Producer/caller/guard/consumer, writer/runner, конкретные ветви текущих классов | Нативная достижимость каждого случая обычным API |
| Сохранённые raw | Независимый разбор трёх guard TRX; console full-suite; source fixtures/JSON | Новый текущий test run |
| Производные receipts | ROW124 joins/counts/размеры и author full-suite JSON | Повтор двоичного преобразования всех объектов |
| Свежий аналитический расчёт | Exhaustive interval oracle; WC01 численный replay; P49/P52; scalar loss | C# tokenizer/facade, скорость, allocations, обученный результат нового продукта |
| Документная проверка | Вставки с сохранением прежних байтов, source manifests, review и публикация | Реализация описанного ремонта |

.NET SDK и PowerShell в рабочей среде не найдены; новых native build/test/mutation, production model calls и научных опытов — **0**. Главный full-suite TRX размером 22 273 912 байт не получен: два чтения blob закончились Transport closed. Пустое содержимое ответа не было принято за пустой файл. Независимого XML-разбора этого главного TRX нет.

## 2. Сверка всех 13 выводов с текущими носителями

| F | Связь с B | Вердикт продолжения | Остающееся действие |
|---|---|---|---|
| F01 | B01 | Потеря допустимого phrase-плана подтверждена source/replay; в позднем DRAFT уже есть exact-count helper без wiring | Завершить wiring существующего helper и принять ordinary reader/target/fallback/resources |
| F02 | B13 | Историческая дробь 3,3% не доказывает product overhead<5%; новая смешанная4,4% повторяет ошибку, добавлена поздняя поправка109 | Исторические/новые описания уточнены; matched product A/B ещё отсутствует |
| F03 | B05 | Untracked root не имеет исключительного owner; lazy sink создаётся после начала действий | Claim до preparation/Train, owner-only writes и collision tests |
| F04 | B06 | Внутренний receipt не удостоверяет outer run; P58 имеет ранние ветви до status | Сохранить partial/invalid/no-contrast и проверить настоящий executor→sink→terminal |
| F05 | B14 | В P50 guardProblems не управляет learning/hypothesis и return 0 | Единое validity решение и subprocess checks с learner=0 |
| F06 | B03 | В P52 attempt2 сохранённое числовое поле ceiling=0 неверно; настоящий 2/4 | Исправить writer, независимый oracle и датированную correction, сохранив raw |
| F07 | B15 | New-only P49 имеет input overlap 3/4 и pair overlap 2/4 | Исправить названия/группы writer; native learning повтор не нужен |
| F08 | B11 | История A мешает накоплению B1/B2; body ограничен FlowRef, но не active revision | Active revision, immutable PairId, проверка целого checkpoint до публикации |
| F09 | B10 | Неравенства строк не доказывают actual response; source-echo может быть законным | Server-owned provenance и атомарный claim доставки до Train |
| F10 | B04 | Clipped objective и p−y расходятся на насыщении | Stable CE в logit domain, отдельное имя clipped diagnostic, gradient controls |
| F11 | B02 | 7 updates = 4 активные/14 пустых эпох; все 3 975 параметров совпали побитово | Численная поправка уже сделана; early-stop остаётся отдельной гипотезой |
| F12 | B08 | 119 классов не имеют положительного термина в закреплённом manifest; 21 исход guard исправлен | Явное распределение классов, discovery и first-match проверка |
| F13 | B16 | Healthy main P58: 105 Unit lessons и 56 двух-Unit chains | Полный stage/selected-case ledger; не называть subtotal стоимостью класса |

Таблица учитывает [доску на45cd22](https://github.com/lubochka/llm_mvp_core/blob/45cd22eb4cd15ad75511ac8580d185775c965c06/docs/plans/replanning-possible-diffs/claude-audit-board-2026-11-11.md): все F01–F13 сопоставлены без искусственного равенства числа строк.

B07 — уже признанная граница прерванного исторического full-suite. B09 — отдельное условие сканера при отсутствующем/непригодном solution. B12 — отдельный store/caller договор пустых и нулевых соседей. Они сохраняются в плане, но не выдаются за три новых измеренных пользовательских сбоя.

### 2.1. Подтверждённые полезные части не отменены

Current P48/P49 finite-verdict исправлен в описанной области. Current P58 считает ceiling 1/8 отдельно от cold 0/8, использует TRAIN-control и ComposeStatusPayload из validator. Предыдущее обвинение P58 в сплошных литералах больше не относится к этому source. F04 касается достижимости записи и внешнего terminal, F13 — расхода.

Wiki hoist Featurize уже выполнен. Его зелёная семантическая reverse mutation показывает эквивалентность, но не защищает ресурсный выигрыш — это отдельный критерий. Карточка32 FIX-WK не получает разрешение на перенос из принятого FIX-WC.

Обычный Mode2 теперь читает chosen body на нормализованном exact prompt благодаря FIX-P118. Историческое N28-B «не читает тело» исправлено датированной вставкой в planning. На парафразы это не распространяется. Утверждение «любой zero-overlap сосед выдаёт чужой body» также было бы неверным: current composer имеет дополнительный exact guard. [Composer source](https://github.com/lubochka/llm_mvp_core/blob/55cfd9d00bbebdeae04da7c505b84d0b1cb9b747/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs).

Положительные выводы предыдущего аудита по Method KNN facade и DPO cache сохранены как его bounded evidence; новым native запуском этого продолжения они не переудостоверялись.

## 3. Что именно пересчитано

### P54: согласованность выбора и допуска

Для 8 токенов current winner покрывает 6/8 одним span и отвергается, хотя 4/8 двумя spans допускается. Второй случай при 6 токенах имеет одинаковое покрытие 4/6, но current tie-break выбирает один span вместо допустимых двух. Каждый рисунок даёт пять непересекающихся планов, включая пустой. [Planner](https://github.com/lubochka/llm_mvp_core/blob/55cfd9d00bbebdeae04da7c505b84d0b1cb9b747/27.TranslationDomain/TranslationPhraseCompositionPlanner.cs), [caller](https://github.com/lubochka/llm_mvp_core/blob/55cfd9d00bbebdeae04da7c505b84d0b1cb9b747/27.TranslationDomain/TranslationTrainedUnits.cs).

Предлагаемый DP хранит лучшие suffix отдельно для count=0, count=1 и count≥2, с точным count внутри tie-break. Полный combinations oracle независимо перечисляет допустимые непересекающиеся подмножества. На всех 33 866 interval graphs для длин 1–5 расхождений нет. Это конечная проверка конструкции, а не доказательство всех Unicode key orders, длин 6–8, target compatibility или расходов. Продуктовая приёмка §3.1 остаётся необходимой.

### Wiki: обновление не равно эпохе

На pinned ASCII fixture получен ledger [2,2,2,1,0,…,0], совпадение всех 3 975 double bit patterns с архивным snapshot. Метод source-equivalent replay прежнего аудита использован повторно; независимость здесь — проверка исходных blobs, формы и результата, а не заявление о второй независимой реализации C#.

Первая полная эпоха без изменений — пятая. Нельзя зашить 4 или 7 как общий новый предел. После выполненного hoist пустые эпохи повторяют scoring, а не прежний Featurize внутри цикла. Скорость early-stop не измерялась. [Native source](https://github.com/lubochka/llm_mvp_core/blob/55cfd9d00bbebdeae04da7c505b84d0b1cb9b747/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs).

### P49/P52: разные оси нельзя называть одним числом

P49 относительно new-only: e-t1=(0,1) имеет знакомый input с новой target, e-t2=(2,3) — новый input, e-t3/e-t4 — знакомые pairs. Три группы 2+1+1. Warm+new даёт 4+0+0. Constant ceiling панели 2/4; потолок функции только от фактического input — 3/4 из-за противоречивых gold при input=0. Пример Input1→5 из опубликованного плана не является строкой этого корпуса; он теперь явно заменён датированным уточнением на реальный e-t1. [P49 helper](https://github.com/lubochka/llm_mvp_core/blob/55cfd9d00bbebdeae04da7c505b84d0b1cb9b747/experiments-v2/RP01-MICRO/P4901/src/FinitenessGovernedVerdict.cs).

P52 имеет два gold ответа по два вхождения. Answer ceiling=2/4, tense ceiling=2/4. Поле сохранённого attempt-0002 действительно равно 0, а writer получает его из cold. Правильный результат другой попытки не исправляет этот JSON pointer. Адресный выигрыш 1/4→3/4 сохраняется отдельно от нулевого A/B выигрыша перевода 4/4→4/4. [P52 saved metrics](https://github.com/lubochka/llm_mvp_core/blob/55cfd9d00bbebdeae04da7c505b84d0b1cb9b747/experiments-v2/RP01-MICRO/P5201/results/attempt-0002/metrics.json), [writer](https://github.com/lubochka/llm_mvp_core/blob/55cfd9d00bbebdeae04da7c505b84d0b1cb9b747/Tests/ExperimentFixes/P5201RejectedAnnotationAddressArmsTests.cs).

### Loss: finite ещё не означает согласованную функцию

При logits [0,−20] вероятность target≈2,061×10⁻⁹. Current clipped loss=16,11809565 имеет центральную разность 0. Stable CE=20,000000002 имеет производную≈−0,999999998, совпадающую с используемым p−y. Обнулять полезный gradient для согласования с clipped diagnostic нельзя; выбран ремонт формулы отчётной CE. Это скалярный расчёт, не gradient test нейросети. [TrainStep](https://github.com/lubochka/llm_mvp_core/blob/55cfd9d00bbebdeae04da7c505b84d0b1cb9b747/35.ExperimentsV2/Core/ProbeNeuralNetwork.cs).

## 4. Достоверность исполнения QA

[Независимый evidence review](evidence-review.md) содержит ссылки на строки source и подробные ограничения. Его основные результаты:

| Исторический guard run | Passed | Failed | Recorded |
|---|---:|---:|---:|
| Reverse mutation | 24 | 36 | 60 |
| Fix | 45 | 15 | 60 |
| Restore | 45 | 15 | 60 |

Все 60 имён совпали; 21 Failed→Passed, 15 Failed→Failed, 24 Passed→Passed. Это подтверждение адресного ремонта, а не число текущих full-suite ошибок. Из оставшихся raw messages извлечены 119 классов, совпавших с сохранённым списком; в 33 физических строках закреплённого batches.tsv для них не найден positive term. Native discovery/first-match scheduler не запускался.

Полный исторический console даёт 13 751 passed + 501 failed + 94 skipped = 14 346 recorded и Test Run Aborted. Имена всех 501 failures совпали с author JSON. Эти 501 — нижняя граница обнаруженных отказов того запуска. Арифметика 501−21 не является новым baseline.

Найден существующий runner, передающий реальный exit child; он не объявлен отсутствующим. Однако bounded incomplete может иметь exit 0. Summarizer при missing TRX берёт journal counts, а COMPLETED_* остаётся readable/closed. Это source-confirmed путь, не утверждение о подмене конкретного прошлого запуска. Приёмочный finalizer должен различать неполную диагностическую сводку и подтверждённый native run.

B09: count=0 parsed projects смешивает synthetic no-solution с malformed либо all-missing real solution. Здоровый текущий solution не объявлен сломанным. Ремонт проверяет valid/foreign-copy, synthetic/no-solution, malformed, all-missing и mixed-target случаи.

## 5. ROW124: что удалось подтвердить о двоичных доказательствах

31 опубликованный текстовый файл получен целиком и проверен по blob. Пересчитаны 6 201 уникальный путь, 2 027 пар объектов, 622 840 снятых байтов, 16 партий и 255 buckets. Joins размеров и totals согласованы; это не повтор преобразования самих двоичных объектов.

Пакет относится к историческим commits 68f0ce4949958b6d560b1630626fdfcc2af8f0dc → 187b54b8844a3cdce7d6c931afe0aa7dca67d9a0. В нём нет исходного executable transform script, а per-path detail хранит PROVEN и размеры без вычисленного output SHA256. Поэтому повторная проверка ограничена составом/арифметикой receipts. Прежнее доказательство автора этим не опровергнуто; нынешнее восстановление также не доказано.

Для будущей отдельной операции нужны source/hash измерителя и per-pair pre/expected/computed oid. Восстановление делается из проверенного оригинала, не механическим добавлением CR перед каждым LF. Новая операция получает отдельную final-state проверку. Она этим аудитом не исполнялась.

## 6. Сам план проверен и исправлен до передачи

Независимый review обнаружил конкретные пробелы предложенной конструкции. Они исправлены датированными дополнениями непосредственно в плане и карточках, с сохранением опубликованной истории.

| Ошибка/недостаточная определённость | Внесённое решение | Различающий контроль |
|---|---|---|
| Два duplicate одновременно видят отсутствие committed outcome | Atomic in-flight claim ReceiptId+LessonId до Train, canonical fingerprint, shared IN_PROGRESS/terminal | 2 deliveries / 1 принятая corrective dose; Train counter, не только один published state |
| Новая typed receipt всё ещё может быть целиком caller-supplied | Предлагаемый server-owned immutable issued/verification registry, opaque IDs и canonical lookup | Поддельный третий text/чужая receipt отказывают до Train; настоящий source-echo допускается |
| Повтор PairId может связать новые weights со старой KNN-парой | Immutable PairId→payload, conflict до mutation, candidate import проверяет target/mode/FlowRef | B использует ID A; conflicting checkpoint; прежнее состояние неизменно |
| После failed Train/CAS claim можно ошибочно очистить и повторить дозу | Terminal FAILED_AFTER_TRAIN/UNKNOWN_COMMIT_STATE; автоматического retrain нет | Повтор после затраченного Train возвращает исход без второго вызова |
| P49 control описан несуществующей в корпусе парой | Явная поправка на e-t1=(0,1) против new 0→3 | Смена target не меняет input novelty |
| Python -O выключает assert, оставляя поля PASS | Fail-closed guard при отключённых assertions | Optimized invocation отвергнут до расчёта/output |

Server-owned registry, active revision и in-flight claim здесь **проектируются**. Не заявлены уже существующие APIs или доказанная exactly-once доставка через crash до durable checkpoint. Unknown commit state остаётся явно ограниченным исходом.

## 7. Очередь ремонта и критерий закрытия

Сохраняются 14 шагов существующего плана. После позднего DRAFT шаг3.1 — IN_PROGRESS; done=0, in-progress=1, remaining=13. Ни один продуктовый шаг не переводится в done по факту этой документации. Численные поправки F02/F11 и подготовка воспроизводимого расчёта сделаны; сопоставимые продуктовые измерения и алгоритмические изменения остаются открытыми.

| Очередь | Владельцы / шаги | Что должно появиться для закрытия |
|---|---|---|
| 1 | EvidenceSink + runner/finalizer; §1.1–1.2 | Exclusive owner до действий; неизменные старые bytes; terminal на late failure/abort/missing raw |
| 2 | P50 §1.3; P52/P49/P58 §2.1–2.3 | Guard управляет verdict/exit; независимые ceilings/overlap; полный фактический ledger |
| 3 | Phrase planner/caller §3.1 и ресурсы §2.4 | Лучший допустимый план; оба witnesses через обычный reader; matched A/B, чувствительная mutation |
| 4 | PromptFlow §3.2; correction §3.3 | Active state/body identity; authenticated provenance; duplicate concurrency; atomic restore |
| 5 | Probe loss §3.4 | Stable CE и независимые finite differences, прежний backward-order fix сохранён |
| 6 | Inventory §4.1; Wiki §4.2–4.3 | Явные routes 119 классов и discovery; resource-sensitive guard; отдельный bounded early-stop опыт, если назначен |

Приёмка соединяет конкретный owner, ordinary consumer, независимый expected, positive/negative/retention, sensitive mutation, source/assembly identity, полный расход и настоящий terminal. Исправление документа, helper-only PASS, ручная замена JSON, ослабление threshold/gold или вычитание исторических failures этого не заменяют.

## 8. Какие карточки действительно усилены

В этом продолжении изменены **14 действующих core карточек/общих договоров**, отдельно сам repair-plan и **3 действующие planning постановки**: всего18 существующих документов. Предыдущая публикация d9fc/3fb сохранена; с ней объединённый документный охват составляет 24 уникальные карточки/договора, плюс план. Это не число научно перепроверенных экспериментов.

Core: 03, 04, 15, 20, 32, 48, 106, 109; P48, P49, P50, P52, P54, P58. Planning: N28 memory/correction identity, N29 rejected annotation, N60-01 measurement seams. [Карта с точными paths, основаниями и проверками](03-card-acceptance-matrix.md) и [manifest вставок](document-change-manifest.json) показывают реальные изменения. Каждая вставка датирована; удаление только вставки восстанавливает прежние байты файла.

Соседние карточки получили **свои** условия: 20/48 — full body/revision/fallback; 32 — собственный ресурсный допуск вместо чужой метрики; N28 — временную границу уже исправленного consumer; N29 — адресную/переводческую оси и доступность runtime features; N60 — физический stage ledger и отказную ветвь. Это проверяемые договоры, а не одинаковый общий лозунг во всех файлах.

## 9. Оставшиеся пределы

Нет нового full-suite baseline, полного binary/LFS scan, аудита всех writers и shell runner целиком, всех 131 карточки, всех language directions или всех neural/state pathways. Прочтение больших governance/карточек выполнялось по применимым разделам; не заявлено полное чтение всего корпуса. Использованы названные архитектурные, planning/review, prompt-authoring, code examination, experiment design, test integrity, failure triage и human-readable навыки; [review](final-review.md) различает область корневой и независимой проверки.

Полезные следующие проверки выбраны по конкретным незакрытым рискам выше. Дополнительные массовые прогоны ради числа зелёных не выполнялись. Исторические raw, product code и тесты этим документным нарядом не изменялись.
