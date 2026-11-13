# Аудит защищённого каталога экспериментов: фактическое покрытие, измерения и оставшиеся расхождения

**АКТУАЛЬНОЕ СОСТОЯНИЕ CORE6E3 / PLANNINGD7.** Ниже сохранён подробный аудит снимков 669/827. Последние подтверждённые состояния: **42 ID, 7 завершённых, 35 будущих; NEXT=N63-INGRESS-LENGTH**. LF-03 report/receipts завершены, хотя core index всё ещё указывает LF-03; saved receipts отменяют повтор. E-OPT labels и GC уже уточнены новым erratum. KN reader observer уже исправлен в канонической карточке, прежний overlay ниже не применять. P54 остаётся адресным вопросом готовности рецептов. LF03 readback и core-конфликты: [сверка core6e3](11-late-reconciliation-6e3-eaed.md). Финальное объединение с четырьмя peer-гипотезами и действующие NEXT: [сверка planningd7](12-concurrent-planning-d7.md).

Дата: 20 октября 2026. Фаза: чтение исходников, карточек и уже сохранённых результатов. Новых запусков обучения, inference, сборок, product tests, изменений core или защищённого каталога этой работой не выполнено.

## 1. Главный вывод и актуальность

**В снимке core827 были завершены шесть первых опытов; NEXT тогда был LF-03.** Реестр содержит 36 разных ID: шесть завершённых и 30 будущих. ROPE-TRAIN-TABLE-01, AST-SIB-CACHE-01 и BPE-REALIZED-MERGE-COUNT-01 перенесены в защищённые карточки; SKNN-SIBLING-PREP-01 остаётся вариантом AST, отдельным запуском его считать нельзя. Два зависимых reader-опыта IDENTITY и KN стоят в конце. Это состояние [индекса на commit 827a1ac](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L5-L64), а не разрешение запускать программу в нынешней документной фазе.

Первичный просмотр был закреплён на core **66947785d1318f7cbbc88118f3156cdc75001e16**. Во время аудита появился **827a1acdbef95d220886e54f9d36bce0b405c8cd**, добавивший E-OPT-STEP и исправления карточек. Новая версия проверена отдельно; старые receipts не переписаны. Planning для сопоставления: **53cbcabd3926cda714bab428d294eb666301e9e4**.

**Нельзя заключить, что все возможные различия уже превращены в готовые и валидные малые опыты.** Полный текстовый корпус получен; семантический аудит выполнен адресно. В нём остались конкретные ошибки границ измерения и отчётности. Семь P54-подслучаев прямо не имеют назначенных обязательных входов и host. Старые P01–P125 — семейства гипотез, а не 125 готовых work orders.

## 2. Что действительно просмотрено

| Уровень | Snapshot 66947785 | Snapshot 827a1acd | Что число означает |
|---|---:|---:|---|
| Blob-файлы дерева защищённого каталога | 1300 | 1345 | Полный список путей; все binary payloads не скачивались |
| Markdown materialized и проверены по git blob SHA | 339 | 345 | Все MD получены без пропусков и несовпадений |
| Дополнительно materialized файлы с расширением JSON | 52 | 68 | В каждом snapshot два файла — Git LFS pointers, не содержимое JSON |
| Реально разобранные JSON | 50 | 66 | Включая выбранные receipts, provenance и результаты; не вся совокупность бинарных checkpoint |
| JSONL в новом snapshot | — | 1 | Исходные два урока E-OPT-STEP |
| Фактические P/N/малые карточки, без index | 154 | 157 | Три добавленные карточки; не число успешно исполняемых опытов |
| Изменённые/добавленные MD в delta | — | 41 | Шесть новых MD, остальные правки; удаления путей не обнаружены |
| Planning MD, чей относительный путь буквально упомянут в защищённых MD/JSON | 747/799 | 750/799 | Только поиск ссылочного пути, не процент семантического покрытия |

Проверены 414 materialized git blobs нового snapshot: 345 MD, 68 файлов с расширением JSON, один JSONL; mismatch=0. **Git blob SHA подтверждает в том числе bytes LFS pointer, но не загруженный большой payload.** В данной выборке LFS pointers — N-KU-01/comparison.json и BPE-NO-MATCH-COPY-01/comparison.json. Выводы о результатах опираются на настоящие arm JSON, не на содержимое этих двух pointers.

Оставшиеся 49 неупомянутых planning-путей относятся к двум пакетам: 31 — 2026-10-20-learning-delta-audit-883496, 18 — 63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20. Отсутствие буквального пути не доказывает отсутствие соответствующей идеи: новые AST/ROPE/BPE карточки как раз содержат перенесённые идеи. Для выводов нужна карта «гипотеза → выбранный фактор → реальные входы → writer/state/consumer → метрики», а не число ссылок.

В первом snapshot все 154 карточки прошли **текстовый поиск** правил: полезный показатель, STOP/NEXT, модель по умолчанию, русский отчёт, регрессии, experimental exceptions, указания host/команд/учебной связи. Это наличие текста, не семантический PASS каждого рецепта. Собственная прежняя [валидация каталога](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/2026-10-20-card-reconciliation/new-gaps-document-validation.json) честно ограничивает 133 embedded prompts текстовыми проверками. Полный независимый review 133 промптов этим аудитом не заявляется. На свежем 827 выполнен [такой же screen 157 карточек](evidence/protected827-card-marker-screen.json): пропущенных текстовых markers 0; это по-прежнему не semantic PASS.

Воспроизводимые receipts: [новый source receipt](evidence/protected827-source-receipt.json), [старый screen карточек](evidence/protected-card-screen-66947785.json), [старый crosswalk](evidence/planning-protected-crosswalk-66947785.json), [новый crosswalk](evidence/protected827-planning-path-crosswalk.json). Они содержат SHA и границы проверки, без подписанных download URLs.

## 3. Уже измеренные успехи: сохранить вместе с потерями

### 3.1. N-KU-01

Настоящее обучение FileActionKnn: одинаковые четыре урока, k=3, отдельные A/B состояния. До обучения обе памяти не дают совета. После A отвечает ADD_LOOP на всех четырёх задачах; B — ADD_METHOD на всех четырёх. Узкая полезная ось: лишний совет «добавить цикл» на трёх задачах с циклом падает 3/3 → 0/3. Регрессия: на задаче, где цикл нужен, правильный ADD_LOOP падает 1/1 → 0/1. Итоговый rubric 1/4 → 3/4 не доказывает правильность всего совета или структурное обобщение: majority labels изменились, do-while отсутствует и в признаках. См. [кандидат и бюджет](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/required-changes.md#L22-L57).

Кандидат точный: FileActionKnn.cs, ActionLabelGenerator.Label, добавить SyntaxKind.DoStatement только в hasLoop. Изменение feature vector — другой фактор. Основной бюджет 2 Train/8 rows/16 queries; дополнительные прогревы, product-equivalence и обратный B→A отдельно раскрыты в источнике. Не повторять опыт ради исправления его отчёта.

### 3.2. BPE-NO-MATCH-COPY-01

Четыре настоящих содержательных BpeTokenizer.Train: A/B в профилях code-doc и EN/RU. В каждом arm дополнительная исходная инициализация Train("",256); всего восемь Train calls, из них четыре перехода изучения merges. Vocab 256→320, 64 merges. A/B дают одинаковые final learned state, полные EXAM token IDs и decoded strings; roundtrip 4/4. Allocation cost: code-doc 3 904 664→1 126 744 bytes (−71,14%), EN/RU 2 892 824→893 656 (−69,11%). CPU code-doc ухудшается 31,25→46,875 ms. Exact text roundtrip не является точностью переводчика. См. [текущий завершённый рецепт и границы](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/BPE-NO-MATCH-COPY-01.md).

Кандидат: BpeTokenizer.MergeChunk — выделять List только при первом фактическом merge; если merge отсутствует, возвращать исходный chunk. Статическая проверка названных callers не доказывает контракт всех будущих consumers. Экономия полного KNN обучения не измерена. Четыре выполненных arm и exit receipts важнее исторической цитаты о раннем техническом сбое: текущий результат завершён, short-path резерв и новый retry теперь не назначаются. Старые failure files сохранить.

### 3.3. Нейронные ресурсные результаты

| Опыт | Наблюдаемый полезный показатель | Регрессии и граница |
|---|---|---|
| NEURAL-FROZEN-TRANSPOSE-01 | За четыре SFT/LoRA update allocations −1 507 392 bytes, около −11,0%; записанные final trainable/optimizer state, loss и logits совпали | Retained cache +249 856 bytes. Exact0/2 → 0/2; отдельный 400-step receipt с 1/2 не превысил constant-sign baseline. Не объединять эти бюджеты |
| N62-R1 | Allocations −90 504 bytes, −0,002404% | GC0/1/2 +1 каждый. ApplyOptimizer time различается, доля RequireValid и причинное ускорение не установлены. TRAIN target quality не измерена |
| N62-R2 | Allocations −312 328 bytes, около −0,0083% | Peak +30 474 240 bytes, GC0/1/2 +1. Расчёт 128 arrays/322 048 payload не равен измеренной общей дельте. Исторический B hash был снят до factor; точная загруженная B binary не подтверждена |
| E-OPT-STEP | Allocations 3 764 994 488→3 764 526 544 bytes: −467 944, −0,01242881%; GC0 193→189, GC1 19→14, GC2 13→10 | Whole-process peak 840 916 992→1 045 463 040 bytes: +204 546 048, +24,3242%. Записанные state/metrics совпали; освоение ответа не измерено. Ограничения описания ниже |

Первые три: [required-changes L60–104](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/required-changes.md#L60-L104). E-OPT первичные [A L7–75](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/A/arm-result.json#L7-L75), [B L7–75](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/B/arm-result.json#L7-L75).

У E-OPT обе руки COMPLETED, issued 2/completed 2. Время TRAIN 50 068,549→40 665,43 ms и CPU 37 687,5→35 828,125 ms — **наблюдаемые полезные изменения**, признанные успехом соответствующих осей; устойчивый причинный эффект фактора не установлен. Пик вырос. Это не причина скрывать меньшие GC1/2, которые не попали в опубликованную короткую таблицу.

Ранний [evidence-file L11](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/2026-10-20-63-883-evidence.md#L7-L11) описывает PREPARED_NO_COMPLETION_RECEIPT **на момент чтения**. Новые A/B receipts и index устанавливают завершение; ранний текст не даёт разрешения повторить E-OPT.

### 3.4. Старые положительные результаты обучения памяти не потеряны

Translation ingress: при одинаковых восьми уроках 11/24 → 19/24, без потерь ранее правильных ответов, но accepted-yet-strict-wrong 1→3. Отдельный P125: пустая память 0/24 → 19/24; cold restore воспроизводит все 24 outputs. Это полезное измерение существующей TranslationMachine/ExecutorUnit, не синтетический scorer без обучения. [Первичное описание и ссылки P125/P25](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/required-changes.md#L5-L19).

## 4. Семантические расхождения снимка827 и их история

### E-AUD-827-01. E-OPT raw ошибочно называет фактор R1

В обоих E-OPT arm-result.json experiment=E-OPT-STEP-01, но factor A — original element-wise LINQ bit comparison of frozen base, factor B — bitwise byte-span comparison of frozen base; measured_path — DpoProjectLoraRecipient.RequireValid. Это старые названия R1, не проверенное чтение optimizer step. Источник ошибки установлен: [N62ExperimentBridge L277–281](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/Common/ContinuousLearning/N62ExperimentBridge.cs#L277-L281) содержит именно эти константы.

**Это доказанная ошибка описания, не доказательство выполнения неверного фактора.** A/B source diff реализует metadata getter; [B ReadOptimizerStepForRandomState L545–555](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/DpoTrainer.cs#L545-L555) и [переключённый caller L1277](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/DpoTrainer.cs#L1277) соответствуют E-OPT. Build props выбирают Variant/DpoTrainer.cs и Variant/Optimizers/AdamW*.cs.

Additive исправление: оставить raw bytes, добавить таблицу ошибочное поле→фактический фактор→исходный writer, сослаться на study и исходники. В будущей экспериментальной копии bridge метаданные должны браться из конкретного work order, не копироваться с R1. Production, старые receipts и completed run не менять.

### E-AUD-827-02. E-OPT отчёт расширяет вывод сверх одной панели

[Report L63–77](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/report.md#L63-L77) одновременно утверждает «детерминирован» и «не меняет обучение совсем», затем признаёт непроверенными порядок/виды отказов и другие guards. Два completed updates/arm и совпавшие записанные состояния поддерживают узкий вывод об этой панели. Они не доказывают универсальную эквивалентность всех validation/error paths, размеров моделей и последующих уроков.

Additive корректная формулировка: «В сохранённом A/B на двух уроках измерена экономия 467 944 bytes при совпавших записанных state/metrics. Прочитанный source diff объясняет устранение копий m/v; guard paths отдельно runtime не измерены. Кандидат успешен по указанной ресурсной оси, перенос остаётся решением пользователя». Показать GC1 −5 и GC2 −3 рядом с GC0 −4, а также выросший peak. Не объяснять остаток между расчётными 425 984 bytes payload и измеренными 467 944 bytes как полностью установленное разложение.

Measured interval bridge L191–234 включает два StepVector, внешние чтения optimizer state, receipts и публикацию. Это стоимость полного заданного учебного цикла, не отдельный микрозамер getter. Непустой RNG callback на L168–171 обеспечивает достижимость изменённого выражения; одинаковые внешние полные экспорты остаются в обеих руках. Независимый адресный source review сохранён в [независимом review](reviews/EOPT-independent-source-review.md); runtime review не выполнялся.

### E-AUD-827-03. KN-ROUTER: таймер не окружает изменяемый reader

Текущая карточка на 827 всё ещё измеряет Train, а B меняет только ScoreAsync. [L145–177](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md#L145-L177): before Read находится до timer; after Read — после timer. Ответы consumer валидно зависят от выученных profiles; resource delta неизменяемого Train не измеряет экономию reader.

Это **перенос уже известного peer C4**, не новая гипотеза. В additive overlay назначить одинаковый observer непосредственно вокруг существующего foreach Read, вернуть rows плюс readBeforeCost/readAfterCost. Для async использовать process-wide GC.GetTotalAllocatedBytes(false), не thread-local разность через await. Исходный Train-cost оставить контекстным. Новые Route/Score calls не добавлять: остаются 2 Train / 16 Route. Пик процесса подписать whole-process, а не пик reader. Product candidate по-прежнему только нормализация ScoreAsync.

### E-AUD-827-04. E-OPT не содержит новую TRAINv2 панель

Выполненный bridge сохраняет исходные hashes, два StepVector и конечный checkpoint/readback; оценивания TRAIN target NLL/rank/response exact до и после нет. [Source L181–267](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/Common/ContinuousLearning/N62ExperimentBridge.cs#L181-L267). Хотя [карточка N62 L15–21](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L15-L21) описывает будущую панель, index теперь прямо исключает её ретроактивное приписывание старому E-OPT.

Это не обесценивает ресурсный успех реальных updates. Вывод «навык освоен» остаётся неизмеренным. Исправление документное: различить сохранённый cost run и будущий LF-03 observer. Не повторять E-OPT, не дополнять его raw вымышленным TRAINv2.

### E-AUD-P54. Семь точных подслучаев ещё не имеют исполнимого рецепта

P54 blanket exclusion уже исправлен: learned reader допустим по source→state→consumer. Но карточка [L21](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L21) оставляет P54.1–7 NEEDS_INPUT_FOR_RECIPE. В [L37–105](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L37-L105) перечислены точные недостающие checkpoints/достижимые queries/masks/relations. Нельзя назначить исполнителю искать их и объявить каталог полностью готовым. Эти поздние readers не задерживают нынешний следующий N63-INGRESS-LENGTH.

## 5. Новое доказательное уточнение P25/P56: checkpoint-тест без изучаемого правила

В [TranslationMachine_CheckpointRoundTrip_QualityVerifierConceptRule L3052–3083](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/Tests/TranslationDomainTests.cs#L3052-L3083) комментарий обещает сохранение verifier rules. Однако [BuildVerifierExample L2679–2694](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/Tests/TranslationDomainTests.cs#L2679-L2694) выбирает Pass=true с пустым Issues. [QualityVerifierUnit.TrainOnExample L1095–1119](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L1095-L1119) создаёт concept rules лишь при !chosen.Pass и распознанном issue. Финальная проверка только исключает BOOTSTRAP в переводе; executor урок может обеспечить это без verifier-rule learning.

Имена этого теста и helper не найдены адресным поиском по 799 planning MD и 339 исходным protected MD. Это **новое доказательство ложной уверенности проверки**, подслучай известных P25/P56; не новая заявленная поломка самого restore.

Кандидат будущей проверки должен реально обучить named required rule через FAIL chosen issue, показать отсутствие/появление конкретного правила до/после урока и различимые lawful/violating native Verify outputs до/после cold restore. Test repair сам по себе не учебный эксперимент. Полный product test suite и автоматический перенос не нужны. Точный мини-рецепт назначается отдельной карточкой автора; generic «поправить тест и запустить всё» не заменяет обучение.

## 6. Сверка с четырнадцатью требованиями пользователя на снимке827

Эта таблица сохраняет исходный аудит. Текущие исключения уже перечислены в шапке: done1–7, NEXT=N63-INGRESS-LENGTH, 35 будущих строк объединённого реестра; EOPT erratum и KN observer уже исправлены. Они не являются новыми поручениями ниже. Упоминания core827 и NEXT=LF-03 в строках таблицы относятся только к тому историческому снимку.

| № | Проверка | Текущее состояние и действие overlay |
|---:|---|---|
| 1 | Малый опыт, гипотеза, предметная тема | У 36 queue ID указаны темы и factors. P01–125 остаются справочником; P54.1–7 явно не готовы. Не считать 157 cards числом runnable экспериментов |
| 2 | Любое полезное измеренное улучшение — успех | В активных заголовках карточек 827 это уточнено. Сохранять observed success отдельно от causal certainty; include GC1/2 E-OPT. Глобальная старая policy L52 всё ещё запрещает local_success для fixed-order ресурсов; прямое правило пользователя и актуальные headers имеют приоритет |
| 3 | Измерение участвует в обучении | Сохранённые N-KU/BPE/LoRA/DPO результаты имеют настоящий writer. KN reader участвует в использовании learned state, но его cost-window надо исправить. Посторонние scorer-only проверки не допускать |
| 4 | Подготовка/restore допускаются с source→state→consumer и точным fix | Верхняя policy это разрешает. Не требовать лишнего Train для уже выученного checkpoint. Старые нижние P25 standalone bans уступают этой policy; отметить это явно |
| 5 | Сначала разблокировка обучения, потом остальные | Blanket approval/gold gates сняты. Новый индекс direct learners→readers соответствует приоритету. Не превращать поздние P54 inputs в общий блокер |
| 6 | Сначала эксперимент, затем решение о product | В текущей работе только документация. Все кандидаты описываются без product changes/full CI; completed runs не повторяются |
| 7 | Точное место, default model, команды, разрешённые конфликты | Для 30 будущих строк 827 подготовлен [output-path-map.json](output-path-map.json): текущие R/entry/report/candidate. Host-created не означает built. Новые AST/BR01 roots сохраняются; старые roots не повод обойти claim |
| 8 | Старые эксперименты не удалять | Delta дерева 669→827 имеет 0 удалённых путей. Наши snapshots и raw сохранены; новые corrections только additive |
| 9 | Приоритет исправлений и порядок | Сначала learning recipes, затем conflicts, затем updates; зависимые readers последние. NEXT=LF-03, а не E-OPT. Дополнительные новые IDs parent вставляет до readers без пересоздания finished1–6 |
| 10 | Послабления экспериментов | В карточках разрешены throw/try-catch/void/minimal host. Это не меняет one-factor/data/budget и не требует product workflow |
| 11 | Успешное измерение документировать кандидатом | Все шесть completed candidates присутствуют в required-changes; E-OPT нужен additive bounded readback. Старую папку не изменять |
| 12 | Язык, формат, skills без поиска | Карточки назначают русский Markdown report и перечисляют проектные skills. Исполнитель текущей сессии по умолчанию; внешняя китайская DPO generator и CODEX selector/quality — другие роли |
| 13 | Немедленные STOP/NEXT в каждой карточке/промпте | В исходных 154 cards найдено правило текстовым screen; новые 3 содержат явный STOP/NEXT. Не продолжать отчёт, hash/review/retry после такого приказа; незавершённые files оставить |
| 14 | Порядок понятен из registry | Актуальный core827:36 ID, 1–6 завершены, NEXT — 7 LF-03. Planning overlay обязан отражать свежий индекс; промежуточный PREPARED evidence не отменяет completed receipts |

Основная нормативная база — [execution policy L27–56](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md#L27-L56). Её старый session-specific write-scope L62 не является нынешним разрешением: **прямой текущий запрет пользователя на защищённый каталог сохраняется**.

## 7. Состав исходного дополнения к planning possibleDifs (снимок827)

1. Этот bounded audit и машинные receipts с раздельными 669/827 snapshots и LFS caveat.
2. Свежий E-OPT readback/candidate: все resource deltas, регрессия peak, source-backed correction неверных raw factor labels, ограниченная формулировка эквивалентности; без изменения raw и повторного запуска.
3. Additive correction KN reader observer для существующих 2 Train / 16 Route, без нового experiment ID.
4. Current output map30 future core IDs, новые AST/BR01 entry paths; parent добавляет только свои новые независимые факторы.
5. P25/P56 vacuous checkpoint evidence как уточнение покрытия; готовый learning recipe — только с буквальными уроками, native calls и отчётом.
6. P54 readiness остаётся адресным недочётом автора семи рецептов; не выдумывать сохранённые артефакты и не перекладывать их поиск на исполнителя.

Полнота этой работы — доказуемый readback названного корпуса и адресных семантических вопросов. Это не утверждение, что все ветки, каждый binary artifact, 157 карточек и все старые подслучаи полностью проверены runtime.



**KN-ROUTER:** использовать только [каноническую карточку core6e3](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md). Reader observer уже внесён; [старый overlay](09-KN-ROUTER-reader-observer.md) сохранён как история и повторно не применяется. Бюджет остаётся 2 Train / 16 Route.
