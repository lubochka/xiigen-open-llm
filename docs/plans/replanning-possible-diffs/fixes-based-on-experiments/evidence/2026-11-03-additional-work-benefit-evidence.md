# 54 дополнительные работы: исходный опыт, показатели и основание очереди

Дата: 3 ноября 2026. Это исследование сохранённых свидетельств и документация. Новые сборки, тесты, обучение, генерация, эксперименты и git-операции не выполнялись.

Здесь разобраны **54 независимые работы сверх прежних 60**, а не 54 новых опыта. Номера N взяты из [независимого учёта](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/evidence/2026-11-03-independent-work-accounting.md>). N07/T111 отдельно не считается: адресное исправление входит в N21/FIX-KDP. [Реестр](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md>) задаёт принадлежность работ; числа ниже взяты из самих отчётов, сохранённых результатов и адресных аудитов, а не выведены из количества строк реестра.

**Нельзя утверждать, что все54 прошли правило допуска.** Среди них есть переносы с локальным полезным эффектом, ремонты доказанных ошибок, проверки уже внесённого кода и удержанные гипотезы. У ремонтной или проверочной работы может не быть собственного A/B; в таком случае это прямо сказано, проценты не выдуманы.

## Как читать допуск

- **Приоритет I** требует полезного улучшения более5% и отсутствия ухудшений других показателей более5%. **Приоритет II** — меньший полезный выигрыш с той же границей ухудшений. Исключён CPU; wall-time, allocations, retained heap и объём состояния не становятся CPU автоматически.
- **REPAIR** означает самостоятельное основание: конкретная ошибка диагноза, адреса, сохранения или восстановления ответа. Это не доказательство прироста обучаемости и не утверждение, что ресурсная цена исчезла.
- **VERIFY** означает необходимую приёмку существующего переноса. Результат исходного опыта не считается второй раз новым положительным эффектом проверочного задания.
- **CONDITIONAL_P1** означает лишь кандидата на первый приоритет по названному локальному полезному показателю. Это **не полный допуск**: оставшаяся ресурсная, потребительская или причинная граница перечислена.
- **HOLD** запрещает называть перенос безусловно прошедшим правило. В частности, N24/P54: allocations+75.18%; N26/P60: потеря слова; N40/P35: allocations+56.40%; N42/P53: valid allocations+11.19%; N52/P08: allocations+155.65%. Для доказанной ошибки сохранения самостоятельное основание ремонта остаётся отдельно.
- Ноль в знаменателе не превращён в «бесконечное улучшение»: для0→N дана абсолютная разница/процентные пункты. Проценты относительные: (B−A)/A×100%; п.п. — изменение доли. «Не измерено» не означает «без ухудшений».

Состояние реализации приведено только в границах уже существующих адресных аудитов/учёта. Этот документ не заменяет новую проверку актуального кода и не объявляет приёмку выполненной. Сохранённый журнал тестов — историческое свидетельство; он не означает запуск тестов в этом исследовании.

## Граница чтения

Крупные обязательные governance/skill файлы были полностью обработаны как входные данные, применимые разделы извлечены и прочитаны точно. Это **не заявление о дословном личном чтении каждой строки всех исторических файлов**. Полностью прочитаны локальный AGENTS, RTK, Context Mode, code-execution, code-examination, prompt-patch-authoring и glossary; в больших canonical AGENTS/CLAUDE и остальных обязательных skills выполнено адресное смысловое чтение применимых разделов. Последний work order родителя разрешил такую границу вместо бесконечного вывода исторического текста. Дословное «reload complete» не заявляется.

Первичные отчёты и нужные raw-поля читались адресно; наличие ссылки не означает, что весь соответствующий JSON был прочитан целиком. После исправления N01 проверены существование 133 уникальных адресов источников, состав 54 уникальных работ и формат JSON. Такая проверка документа не является продуктовым тестом.

## Разбор работ

### N01. 135-R01-INTERFACE — ремонт интерфейсного контракта измерения

**Опыт и изменение.** Не самостоятельный опыт. TranslationMachine перестал реализовывать ILearningMachine: у EvaluateAsync появился третий optional аргумент expectedStateVersion, которого нет в интерфейсе. Вернуть EvaluateAsync(evalSet,ct) ровно с двумя аргументами; проверку названной версии вынести в отдельный EvaluateAtVersionAsync(evalSet,expectedStateVersion,ct).

**Точность.** Никакого A/B точности для исправления сигнатуры нет. Эта работа не относится к выбору owner/payload P111.

**Полезный показатель до → после.** До: метод с третьим, хотя и optional, аргументом не реализует двухаргументный член интерфейса. Текущий исходник: EvaluateAsync на строках727–732 имеет ровно два аргумента и делегирует в отдельно названный EvaluateAtVersionAsync на743–746.

**Ухудшения и пределы.** Историческое несобираемое состояние описано в комментарии719–726; новая сборка здесь не запускалась. Совпадение текущей сигнатуры прочитано напрямую; прохождение остальных потребителей и runtime этим не доказано. Проценты не измерены.

**Допуск: REPAIR_CONTRACT; NO_LEARNING_GAIN_CLAIM.** Основание — конкретный нарушенный контракт измерения и невозможность собирать соответствующую область, а не улучшение обучения. Это уже внесённый ремонт интерфейса; самостоятельный положительный эксперимент или приоритет I/II из него не следует.

Источники: [1: TranslationMachine.cs](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs>); [2: codex-135-post113-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md>).

### N02. 135-R01-CONCURRENCY — ремонт состояния

**Опыт и изменение.** Разбор конкурентных исправлений и поздней оценки состояния в аудите134; наблюдённая производственная гонка не заявлена. Публиковать изменение памяти и её оценку с проверкой версии; поздняя оценка S0 не должна заменить оценку S1.

**Точность.** Нового A/B ответов нет.

**Полезный показатель до → после.** Разобран сценарий: два читателя v7 публикуют v8, последнее сохранение теряет первое изменение. Отдельно разобрано присоединение старой оценки к новой памяти. Это логические контрпримеры, не измеренная частота сбоев.

**Ухудшения и пределы.** CAS частично есть по учёту; цепочка выбранного payload и версия оценки остаются границами. Проценты потерь/нагрузки не измерены.

**Допуск: REPAIR.** Защита уже принятых исправлений и достоверности диагноза — самостоятельное основание; проценты выигрыша обучения здесь неприменимы.

Источники: [1: codex-134-post113-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md>); [2: codex-135-post113-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md>).

### N03. 135-R09-PAIR — ремонт адреса памяти

**Опыт и изменение.** P106 и аудит123/134: служебный PAIR-ключ мог включать смысловой допуск с нулевой похожестью; разные писатели и читатели используют связанные ключи. Единообразно канонизировать ключ у записи, исправления, чтения и восстановления; отдельно подтвердить RULE/import и законный положительный случай.

**Точность.** Самостоятельного A/B этого переноса нет. Исправление аннотации P106 не равно исправлению всех ролей слов.

**Полезный показатель до → после.** До: служебная пара могла считаться смысловым основанием. Канонизация уже внесена по учёту; полный обычный цикл записи/чтения/загрузки ещё требует приёмки. Процент не измерен.

**Ухудшения и пределы.** У P106 пунктуационные случаи сохраняли 0/2; это предел исходного опыта, не исчезнувший благодаря канонизации.

**Допуск: REPAIR.** Законный ремонт ложного допуска и восстановления ключей; нельзя выдавать за новый рост точности модели.

Источники: [1: codex-123-post107-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md>); [2: codex-134-post113-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md>); [3: codex-135-post113-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md>); [4: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10601/results/attempt-0001/report.md>); [5: 26-arm-B-reads-and-verdicts.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10601/results/attempt-0001/26-arm-B-reads-and-verdicts.json>).

### N04. 135-R02-EXECUTION — ремонт диагностики запуска

**Опыт и изменение.** Аудит134 общей границы build/run и прежних попыток; независимого учебного A/B нет. Разделить состояние окончания сборки и исполнения, связывать receipt с реально выполненным callback и потреблёнными файлами, применять общий deadline.

**Точность.** Точность модели этим не измеряется.

**Полезный показатель до → после.** Разобран путь: общий terminal после build подавляет run, callback=0, но exit=0. Должен получиться явный незапуск, а не успешный результат. Отдельные дочерние ремонты относятся к той же работе.

**Ухудшения и пределы.** Число реально случившихся подавлений и экономия времени не измерены. Новый прогон в этой исследовательской сессии не выполнялся.

**Допуск: REPAIR.** Ошибка диагностики выполнения — прямое самостоятельное основание очереди; не оптимизация обучения.

Источники: [1: codex-134-post113-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md>); [2: codex-135-post113-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md>); [3: codex-127-postmerge-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md>).

### N05. T110 — квалификация численного ядра; глобальный перенос удержан

**Опыт и изменение.** P110: скалярное и векторное вычисление одной проекции на одном обученном состоянии; независимый double-oracle. Проверить численную эквивалентность сохранённых выходов и границы допуска, не переносить локальное ядро как ускорение всего обучения.

**Точность.** A=12/16, B=12/16; прирост точности 0. Общее обучение 0/16→12/16 не является A/B ядра.

**Полезный показатель до → после.** Макс. абсолютная ошибка 1.3435864723e-6→5.6376216051e-7 (−58.04%); макс. относительная 2.0233471079e-5→3.2370326193e-5 (+59.98%). Argmax-расхождения A/B=0.

**Ухудшения и пределы.** Ошибка отдельной координаты token0 выросла примерно на188.77%, token1 на22.48%. Внутреннее время ядра .5039→.7418ms (+47.21%), внешнее 3.8354→1.3959ms; порядок/JIT не отделены. Это не доказанный общий speedup.

**Допуск: HOLD_GLOBAL; VERIFY_SAVED.** Улучшение одного агрегата не отменяет ухудшение другого; допустима сохранённая квалификация эквивалентности. Полезный выигрыш обычного учебного потребителя не доказан.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11001/results/attempt-0001/report.md>); [2: scalar-equivalence.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11001/results/attempt-0001/raw/scalar-equivalence.json>); [3: corruption-control.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11001/results/attempt-0001/raw/corruption-control.json>); [4: codex-130-p110-numeric-projection-disposition-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-130-p110-numeric-projection-disposition-2026-11-02.md>).

### N06. T112 — ремонт измерителя и происхождения данных

**Опыт и изменение.** P112: разбиение по строкам против разбиения по семье материала с обучением одной и той же единицы. Связать разбиение, обучение, отчёт и контроль с одним неизменным фактически прочитанным материалом; исключить пересечение родственных примеров.

**Точность.** EXAM 2/4→2/4, DEV 1/2→1/2; постоянный ответ также2/4. В экзамене приобретены2 и потеряны2 строки.

**Полезный показатель до → после.** Пересекающихся семей2→0; DEV confidence .7143/.8845→.3333. Это снятие утечки и ложной уверенности, а не рост качества.

**Ухудшения и пределы.** Баланс меток меняется4R/2B→3R/3B; семейный контроль меняет состав обучения. Нельзя объявить уменьшение confidence регрессией правильности или новым выигрышем точности.

**Допуск: REPAIR.** Доказанная нечестность независимости проверки оправдывает ремонт диагностики. Новый перенос должен подтверждать фактически потреблённые bytes; отдельного A/B такого переноса нет.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11201/results/attempt-0001/report.md>); [2: material-ledgers.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11201/results/attempt-0001/raw/material-ledgers.json>); [3: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11201/results/attempt-0001/arms.json>); [4: codex-132-p112-material-lineage-transfer-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-132-p112-material-lineage-transfer-2026-11-02.md>).

### N08. T113 — условная политика представления

**Опыт и изменение.** P113: общая L2-нормировка текста и предикатов против отдельной нормировки каналов с равными весами. Сохранять совместную нормировку по умолчанию; рассматривать отдельный opt-in только для названного потребителя и его полезной метрики.

**Точность.** EXAM 4/4→4/4, retention8/8→8/8, короткие запросы4/4→4/4; приобретено0, потеряно0.

**Полезный показатель до → после.** Доля энергии предикатов длинных запросов около .00124→.5; искусственный арифметический margin −.000935→+.499376. Это изменение геометрии, а не дополнительный правильный ответ.

**Ухудшения и пределы.** Доля предикатов короткого канала падает примерно на9.48% как предусмотренное перераспределение; само по себе это не регресс полезной метрики. Allocations1237464→1051624 не квалифицированы по порядку/JIT.

**Допуск: HOLD_GLOBAL; CONDITIONAL.** Нет измеренного полезного выигрыша обычного потребителя. Ни большая энергия, ни её уменьшение сами по себе не дают приоритет I/II; глобальный перенос исходным опытом не обоснован.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11301/results/attempt-0001/report.md>); [2: channel-receipts.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11301/results/attempt-0001/raw/channel-receipts.json>); [3: arithmetic-fixture.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11301/results/attempt-0001/raw/arithmetic-fixture.json>); [4: codex-133-p113-channel-balance-disposition-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-133-p113-channel-balance-disposition-2026-11-02.md>).

### N09. 135-R08-P114 — ремонт нулевого веса DPO

**Опыт и изменение.** P114: A обрабатывает пары весов[1,0] с нижним порогом.01; B исключает нулевую; C содержит только положительную. Не проводить Forward/Backward и не учитывать нулевую пару в знаменателе; подтверждать совпадение B с C.

**Точность.** Обычная точность2/4→2/4; постоянный ответ также2/4. Рост точности не найден.

**Полезный показатель до → после.** Forward8→4 и Backward4→2 (−50% вызовов); нормированный вес положительной пары .990099→1. B совпадает с P-only C по весам/моментам в допусках.

**Ухудшения и пределы.** Осталась отдельная граница tiny-positive floor; all-zero случай исходным учебным опытом не запускался. Проценты времени/памяти обычного потребителя не установлены.

**Допуск: REPAIR; CONDITIONAL_P1_COST.** Нулевая пара не должна менять градиент — самостоятельная ошибка. Вдвое меньше вызовов доказывает локальный объём работы, но не полный ресурсный допуск и не прирост качества.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11401/results/attempt-0001/report.md>); [2: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11401/results/attempt-0001/raw/arms.json>); [3: fingerprints-and-comparisons.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11401/results/attempt-0001/raw/fingerprints-and-comparisons.json>); [4: codex-135-post113-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md>).

### N10. 124-S03 — ремонт диагностики допуска

**Опыт и изменение.** P104 и аудит123: router доверял сохранённым PassedGate и порогам, допуская нечисловые/недопустимые наблюдения. Проверять конечность и диапазон наблюдений, применять доверенные пороги перед обычным Route/ExecuteAsync.

**Точность.** Учебного A/B нет; acceptance gate не является точностью ответа.

**Полезный показатель до → после.** До: сохранённый флаг мог разрешить запуск при негодной оценке. После по учёту код finite+trusted threshold внесён; собственные отрицательные и обычные случаи ещё отделены от наличия кода.

**Ухудшения и пределы.** Частота таких checkpoint и ресурсная цена не измерены. Старые тесты с постоянным хорошим gate эту ошибку не проверяли.

**Допуск: REPAIR.** Ложное заключение измерителя о готовности — самостоятельное основание. Не требуется выдуманный процент улучшения обучения.

Источники: [1: codex-123-post107-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md>); [2: codex-124-post107-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md>); [3: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10401/results/attempt-0001/report.md>).

### N11. 124-S06 — ремонт восстановления оценки

**Опыт и изменение.** P104/аудит123: local threshold .75, но импорт мог принять accuracy .1; второй ImportCheckpoint копировал PassesGate напрямую. Один валидатор конечности, диапазона и порога.75 для обоих путей загрузки до изменения состояния и повторного export.

**Точность.** Сравнения точности обучения нет; .1 и .75 — значения допуска, а не результаты A/B.

**Полезный показатель до → после.** До: .1 не проходит местный gate, но проходит импорт при валидных ссылках. Требуемый результат: оба входа отвергают .1/.749 и одинаково принимают законную .75. Код общего валидатора уже указан в учёте.

**Ухудшения и пределы.** Это достижимое расхождение контракта, а не установленный исторический инцидент с плохим checkpoint. Производительность не измерена.

**Допуск: REPAIR.** Ошибка восстановления и повторного сохранения диагноза прямо подпадает под самостоятельный ремонт.

Источники: [1: codex-123-post107-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md>); [2: codex-124-post107-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md>); [3: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10401/results/attempt-0001/report.md>).

### N12. 127-M02b — ремонт восстановления читаемого ответа

**Опыт и изменение.** P101: на сохранённых состояниях генератор выбирал одиночный байт182, не образующий законченный символ. Отличать маску допустимых кандидатов до выбора от обрезания незавершённого хвоста уже готовой строки; подтвердить допустимые UTF8 продолжения.

**Точность.** На точках25 и50 ограничение допустимых однотокенных ответов вернуло EXAM0/2→1/2 (+50п.п.; относительный процент от0 не определён). На100 не исправило ответ.

**Полезный показатель до → после.** Из20 сохранённых чтений6 имели недопустимый верхний токен;2 стали правильными. В14 законных строках tokenID и logit побайтно совпали. 0 новых optimizer updates.

**Ухудшения и пределы.** На100 выбирается законная, но неверная звёздочка. Маска только для одиночного символа не разрешает автоматически общую UTF8 генерацию. Текущий trim-tail не доказывает предвыборную маску.

**Допуск: REPAIR.** Восстановление читаемого ответа имеет самостоятельное основание; эффект локален и не устраняет ухудшение модели от продолжения обучения.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10101/results/attempt-0001/report.md>); [2: codex-127-postmerge-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md>); [3: 121-post102-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md>).

### N13. 124-S01 — ремонт отрицательных контролей

**Опыт и изменение.** P104 и аудит123/134: старые «контроли» возвращали false по enum, не пропуская повреждённый объект через настоящий измеритель. Проверять честную и намеренно повреждённую запись одним реальным путём; отделять исключение выполнения от правильного отрицательного результата.

**Точность.** P104 attempt2 имел законные2/5→4/5 и правильные исходы3/5→5/5; это исходный опыт, а не выигрыш ремонта контролей.

**Полезный показатель до → после.** Новая сохранённая запись ремонта содержит9 различимых пар;16 старых условий остаются непокрыты. Число boolean=21 не равно21 доказанному контролю.

**Ухудшения и пределы.** SafeRun, превращающий любое исключение вfalse, может принять поломку за успешное отвержение. Полного процента исправленных измерителей нет.

**Допуск: REPAIR.** Явный дефект диагностики допуска — самостоятельное основание. В очередь входит достоверность проверки, а не повторное увеличение точности.

Источники: [1: codex-123-post107-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md>); [2: codex-124-post107-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md>); [3: codex-134-post113-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md>); [4: negative-controls-replay.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10401/results/s02-repair-0001/negative-controls-replay.json>).

### N14. 124-S05 — ремонт согласованности сохранённого смысла

**Опыт и изменение.** P106 raw armB: исправленные pattern/signature сосуществуют с «проверь»=NOUN и «файл»=VERB/IMPERATIVE, lemma «фай». Исправлять единый semantic result вместе с токенами, POS, ролями и тегами; не менять только два верхних поля.

**Точность.** Пунктуационные строки сохраняют0/2. Для этого целостного ремонта независимого A/B ещё нет.

**Полезный показатель до → после.** До: аннотация и роли одних слов противоречат друг другу; две верхние строки могут выглядеть исправленными. Требуемое после: согласованный разбор и сохранение/чтение того же результата.

**Ухудшения и пределы.** Суффиксное правило ошибочно считает «файл» глаголом; исправление только кеша аннотации ошибку токенизации не снимает. Ресурсы не измерены.

**Допуск: REPAIR.** Есть первичный raw с противоречивым восстановленным разбором. Это законный ремонт целостности ответа, но без заявленного процента роста точности.

Источники: [1: 26-arm-B-reads-and-verdicts.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10601/results/attempt-0001/26-arm-B-reads-and-verdicts.json>); [2: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10601/results/attempt-0001/report.md>); [3: codex-123-post107-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md>); [4: codex-124-post107-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md>).

### N15. 124-S07 — ремонт отчёта и сохранённых выводов

**Опыт и изменение.** P103: raw вывод о забывании был опровергнут — сравнивались два независимых Train, не продолжение одной траектории; создана адресная поправка. Обычный reporter должен применять проверенную поправку по source hash/path/JSON pointer и сохранять исходную историю.

**Точность.** Никакого A/B точности ремонта нет. Старое утверждение о забывании нельзя считать измеренным эффектом.

**Полезный показатель до → после.** До: обычный отчёт может повторить уже опровергнутый вывод. Helper чтения поправок существует; соединение с обычными reporters остаётся отдельной работой.

**Ухудшения и пределы.** Поправка чужого hash должна отклоняться. Нельзя молча перезаписать исторический raw или улучшить счётчик качества переименованием.

**Допуск: REPAIR.** Исправление доказанной ошибки диагностики законно независимо от процента; наличие helper не равно исправлению пользовательского отчёта.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10301/results/attempt-0001/report.md>); [2: codex-123-post107-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md>); [3: codex-124-post107-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md>); [4: codex-claim-corrections.json](<C:/Projects/llm_mvp_core/experiments-v2/_audits/2026-11-02-post107-audit/codex-claim-corrections.json>).

### N16. 124-S08 — проверка перенесённого tie-break

**Опыт и изменение.** P85 и последующий ремонт MethodKnn: при равном числе голосов учитывать расстояние до ближайшего примера перед ordinal fallback. Подтвердить уже внесённое разрешение ничьей в native predictor и обычном SemanticCompilerFacade; не повторять реализацию.

**Точность.** Самостоятельной пары before/after точности именно этой проверочной работы нет. Исходный P85 про набор обязанностей нельзя целиком приписать tie-break.

**Полезный показатель до → после.** Компонентная сохранённая проверка35/35; native targeted и достижение predictor через facade по аудиту не подтверждены. 35/35 — контрольные случаи, не точность на35 новых задачах.

**Ухудшения и пределы.** Сборка общего Tests и фильтр173 чужих тестов не подтверждают этот consumer. Ресурсного сравнения нет.

**Допуск: VERIFY.** Законна как недостающая приёмка уже перенесённой ошибки чтения памяти; это не отдельный экспериментальный выигрыш или новый алгоритм.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P8501/results/attempt-0001/report.md>); [2: codex-123-post107-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md>); [3: codex-124-post107-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md>); [4: 121-post102-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md>).

### N17. 124-S13 — ремонт определения правильного ответа

**Опыт и изменение.** P102: calibration сравнивает полное Trim==gold, generation использует StartsWith(gold); actual IDs подменялись повторным Encode готовой строки. Один evaluator для калибровки и результата; хранить реально выданные ID отдельно от повторно кодированных; отвергать противоречащий хвост.

**Точность.** Исходный P102: подсказанные токены36/47→37/47 (+2.78% относительно счёта), собственная генерация7/47; это разные способы чтения, не выигрыш scorer-ремонта.

**Полезный показатель до → после.** До: противоречащий ответ с правильным началом может считаться верным. После должно проверяться полное назначенное смысловое условие. Собственного A/B ремонта нет.

**Ухудшения и пределы.** В исходном P102 exam loss1.5593541→1.5593718 (+0.00114%) ухудшилась при росте token accuracy; новое обучение не требуется для честного пересчёта verdict.

**Допуск: REPAIR.** Явно разные определения правильности и ненастоящие emitted IDs — самостоятельный дефект диагностики, а не доказанное улучшение обучения.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10201/results/attempt-0001/report.md>); [2: codex-124-post107-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md>); [3: 121-post102-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md>).

### N18. 127-M05a — ремонт наблюдаемости поиска

**Опыт и изменение.** P66: KnnReached выводился из имени выбранного adapter, а не из события попытки поиска. Записывать search_attempted, число кандидатов/допустимых, выбранный lesson и final source в реальном поисковом пути.

**Точность.** Исходный P66 открыл2/2 ранее скрытых полярностных случая вместо0/2; качество перевода этим не улучшено. Этот ремонт не имеет A/B ответов.

**Полезный показатель до → после.** Из старого raw можно узнать selected/not-selected; attempted=NOT_MEASURED. Unknown нельзя заменитьfalse. Новый receipt должен различать ранний exact, поиск без допустимых и выбранного соседа.

**Ухудшения и пределы.** Старые raw не позволяют восстановить пропущенное событие. Процент посещений и выигрыш скорости не измерены.

**Допуск: REPAIR.** Ложное наблюдение исполнения — самостоятельная ошибка диагностики; ремонт законен без выдуманного A/B.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6601/results/attempt-0001/report.md>); [2: frozen-flow-raw.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6601/results/attempt-0001/frozen-flow-raw.json>); [3: codex-127-postmerge-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md>).

### N19. 127-M05b — ремонт счёта работы

**Опыт и изменение.** P66: pairSimilarityEvaluations отражал число решений допуска/пар, но повторные физические ComputeTokenOverlap не учитывались. Считать каждый реальный callable в едином wrapper с категориями admission/grid/oracle/edge; отдельно хранить уникальные пары и обновления.

**Точность.** Точность ответа не является предметом этой работы.

**Полезный показатель до → после.** Исторические12 admission evaluations не равны полному числу вызовов; аудит сохраняет12+1 и дополнительную оценку26 (source-derived39). Требуется увеличивать ledger на1 при дополнительном вызове даже той же пары.

**Ухудшения и пределы.** Это исправление заниженной стоимости, а не сокращение39→12 и не найденная экономия. Собственного измеренного после ещё нет.

**Допуск: REPAIR.** Доказанный дефект измерителя стоимости сам оправдывает очередь; новых Train для честного описания не нужно.

Источники: [1: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6601/results/attempt-0001/metrics.json>); [2: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6601/results/attempt-0001/report.md>); [3: codex-127-postmerge-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md>).

### N20. T108-CARRIER — ремонт передачи признаков

**Опыт и изменение.** P108: чтение предикатов только из string[] против типизированного чтения допустимых коллекций. Сохранять предикаты из List/read-only carriers, а неподдерживаемый/испорченный carrier явно отвергать.

**Точность.** Маршрутизация12/12→12/12, controls8/8→8/8; ответы уже копировались из памяти и не доказывают качество признаков.

**Полезный показатель до → после.** Доставленные предикаты4/12→12/12 (+200% относительно; +66.67п.п.); именованный отказ на недопустимом carrier0/2→2/2 (относительный процент от0 не определён).

**Ухудшения и пределы.** Историческая трасса потреблённых файлов/финализации неполна; не считать repair receipt новым учебным результатом. Ресурсного A/B нет.

**Допуск: REPAIR.** Молчаливая потеря сохранённых/переданных признаков и отсутствие диагноза — самостоятельное основание; не рост точности модели.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10801/results/attempt-0001/report.md>); [2: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10801/results/attempt-0001/arms.json>); [3: 126-p108-predicate-carrier-transfer-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/126-p108-predicate-carrier-transfer-2026-11-02.md>); [4: m06-measurer-repair-receipt.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10801/results/m06-repair-0001/m06-measurer-repair-receipt.json>).

### N21. FIX-KDP — единая работа чтения доменной памяти и адресного исправления

**Опыт и изменение.** KDP сравнивает базовое чтение с доменными принципами; KDR отдельно добавляет маршрутизацию и функции. P111/T111 — дочерняя проверка владельца исправления, не55-я независимая работа. Включить доменные правила через законный конструктор, сохранить единицу-владельца от выбора до изменения и восстановления состояния.

**Точность.** KDP S1: RU5/8→8/8 (+60%), HE6/8→8/8 (+33.33%), OR5/8→8/8 (+60%); CS6/8 и EN8/8 без изменения. KDR: fixed10/46→domain24/46→domain+functions28/46.

**Полезный показатель до → после.** Чистая добавка функций KDR24→28 (+16.67%); полный10→28 смешивает два фактора. P111 правильный owner1/2→2/2, чужие изменения1→0; allocations33752→33752 на переход.

**Ухудшения и пределы.** KDP HE-retention4/4→3/4 возникает в общей учебной траектории, не доказан как A/B регресс принципов. В KDR RU/HE routes0/8 остаются провалом. Ресурсы в основном общепроцессные; полная граница−5% не подтверждена.

**Допуск: CONDITIONAL_P1; REPAIR_OWNER.** Локальный рост правильных чтений >5% есть, но широкое включение требует обычного потребителя/ресурсов. Защита адресованного сохранения исправления имеет самостоятельное основание. Не заявлять ускоренное обучение.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/report.md>); [2: raw.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json>); [3: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KDR01/results/attempt-01/report.md>); [4: raw.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KDR01/results/attempt-01/raw.json>); [5: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11101/results/attempt-0001/report.md>); [6: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11101/results/attempt-0001/arms.json>); [7: 74-knn-domain-constructor-transfer.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/74-knn-domain-constructor-transfer.md>).

### N22. FIX-P51-TOPIC — ремонт адреса точной памяти

**Опыт и изменение.** P51: одинаковый исходный текст в разных темах конфликтует в ExactSourceMap. Включить topic в ключ у писателя, читателя и checkpoint, сохраняя законный старый профиль.

**Точность.** Обычный consumer2/4→4/4 (+100% относительно; +50п.п.); training controls2/3→3/3 (+50%).

**Полезный показатель до → после.** Ложных принятий1→0; различимых exact-ключей2→3 (+50% записей). Это предотвращение коллизии уже выученных ответов.

**Ухудшения и пределы.** ArmA полной машиной и ArmB прямым flow не дают сопоставимого времени/allocations. Heap670472→833168 снят накопительно, не как независимое A/B. Цена дополнительного ключа не квалифицирована.

**Допуск: REPAIR; CONDITIONAL_P1.** Коллизия адресов сохранённого ответа оправдывает ремонт. Локальная точность проходит порог>5%, но полноценный приоритет I по всем ресурсам ещё не доказан.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5101/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5101/results/attempt-0001/metrics.json>); [3: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5101/results/attempt-0001/arms.json>); [4: 111-post-p55-transfer-plan-2026-11-01.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md>).

### N23. FIX-P52-TAGS — ремонт метаданных выученного ответа

**Опыт и изменение.** P52: для выбранной пары использовались грамматические теги противоположной стороны. Записывать и читать теги именно выбранного ответа согласованно с его токенами.

**Точность.** Точный перевод4/4→4/4; правильные tense tags при сохранении0/2→2/2 и при фактическом разборе0/2→2/2.

**Полезный показатель до → после.** Улучшение тегов+100п.п.; относительный процент от нуля не определён. Это исправление содержания сохранённой пары, а не ещё четыре правильных перевода.

**Ухудшения и пределы.** Exact-map обходит KNN; полные RULE-ключи0/4 достижимы. Исправление helper само не закрывает устаревшие токены/теги, префиксные коллизии сохраняются.

**Допуск: REPAIR.** Первичный опыт показывает неверное сохранение/восстановление грамматических метаданных. Законный самостоятельный ремонт; общего прироста точности перевода нет.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5201/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5201/results/attempt-0001/metrics.json>); [3: 111-post-p55-transfer-plan-2026-11-01.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md>).

### N24. FIX-P54-PHRASE-COVERAGE — условное изменение выбора фраз

**Опыт и изменение.** P54: greedy берёт длинную первую фразу и оставляет слово, bounded partition находит покрывающий набор. Выбирать разбиение с полным допустимым покрытием в ограниченном диапазоне; сохранить fallback и предел длины.

**Точность.** Панель3/4→4/4 (+33.33%; +25п.п.), controls3/3→3/3. На основном запросе покрыто2/3→3/3 слов.

**Полезный показатель до → после.** Полезный выигрыш — ещё один правильный собранный ответ. Восьмисловный край2→3 покрытых слова всё ещё отвергается; девятисловный fallback не меняется.

**Ухудшения и пределы.** Allocated bytes19336→33872 (+75.18%). Наблюдавшееся время37983.8→130173µs (+242.71%) не отделено от JIT/порядка, но не может быть объявлено допустимым через исключение CPU.

**Допуск: HOLD_UNCONDITIONAL; CONDITIONAL_REPAIR.** По критерию>5% без>5% иных ухудшений безусловный перенос не проходит из-за памяти. Возможен ограниченный ремонт конкретного ошибочного чтения с явно принятой ценой; независимое основание и ресурсная граница должны быть названы.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5401/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5401/results/attempt-0001/metrics.json>); [3: 111-post-p55-transfer-plan-2026-11-01.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md>).

> **Пометка 10 ноября 2026 (исполнитель): расход памяти «до → после» и прирост плюс 75,18
> процента в графе «Ухудшения и пределы» выше принадлежат ОТВЕРГНУТОМУ ПРОТОТИПУ опыта, а не
> установленному в продукте планировщику.** Значит, и класс допуска этой работы —
> `HOLD_UNCONDITIONAL` из-за памяти — посчитан по числам прототипа. В границах чтения этого
> документа и на свою дату запись верна; как цена принятого переноса она читается неверно.
>
> **Что измерено у установленного планировщика.** Слайс квалификации покрытия фразы (устойчивое
> имя `FIX-P54-PHRASE-COVERAGE`), принятый 10 ноября 2026 года, измерил установленную форму
> впервые: изолированное сравнение двух сборщиков на шести чтениях — 9 248 против 1 344 байт, плюс
> 588,1 процента; плотный вход — 4 656 против 224 байт, плюс 1 978,57 процента; доля целого
> продуктового чтения шести строк — 239 488 байт всего, доля планировщика около 3,3 процента, и эта
> доля объявлена НИЖНЕЙ ОЦЕНКОЙ, а не точной долей.
>
> **Какое решение вынесено и кем.** Управляет третий знаменатель — доля целого продуктового
> чтения. Первые два сравнивают планировщик с прежним сборщиком, который не умеет делать требуемое,
> поэтому такое отношение измеряет цену новой способности, а не ухудшение продукта. Перенос допущен
> как небольшое ухудшение ниже порога владельца в пять процентов, но НЕ С ЗАПАСОМ: от порога его
> отделяет 1,7 процентных пункта, и названы три условия, при любом из которых допуск отменяется.
> Решение вынес родитель-архитектор и опубликовал.
>
> **Оговорка о форме: числа прототипа неприменимы не только по величине, но и по сути.** В
> продукте стоит другой алгоритм, чем в прототипе, — динамическое программирование с одним лучшим
> путём на точку вместо перебора. Прототип платил памятью за хранение всех готовых вариантов;
> установленная форма этого не делает по построению.
>
> **Где лежат улики и решение.** Каталог улик слайса —
> `artifacts/fbe/FIX-P54-PHRASE-COVERAGE/attempt-0001/`; в нём документ измерения расхода памяти
> (`allocation-measurement.md`, раздел «Вердикт по порогу») и документ границ заявлений
> (`claim-boundaries.md`). Решение о знаменателе — записка фильтра пользы переносов
> (`../../claude-transfer-benefit-filter-2026-11-10.md`, раздел «Решение архитектора того же дня: какой
> знаменатель управляет допуском по расходу памяти»). Та же подмена носителя уже помечена в реестре
> переносов (`../01-registry.md`, строка очереди о покрытии фразы).
>
> **Исходный текст не изменён намеренно.** Тот же процент прототипа стоит ещё в двух местах
> этого документа — в правиле чтения допуска (раздел «Как читать допуск», пункт **HOLD**) и в
> массиве раздела «Данные для интеграции» в конце файла. Они тоже оставлены как есть: это учётная
> история. Сколько всего носителей несёт это число по проекту — не измерено, и эта пометка такого не
> утверждает.

### N25. FIX-P56 — ремонт ложного совпадения слова

**Опыт и изменение.** P56: поиск через Contains принимает часть слова за требуемый целый термин. Проверять границы целого термина в назначенной ветке signature, сохраняя допустимые формы.

**Точность.** Основная панель3/4→4/4 (+33.33%), edge2/4→4/4 (+100%); controls/покрытие4/4 без изменения.

**Полезный показатель до → после.** Ложное принятие1→0; исправляется конкретная выдача чужого ответа по подстроке.

**Ухудшения и пределы.** Реальная native signature отличается от копии: обычная достижимость не доказана. «not ready» принимается в обоих плечах, polarity вне этого фактора. Train allocations41808→39416 (−5.72%) не квалифицированы по порядку.

**Допуск: REPAIR; CONDITIONAL_P1.** Ошибочное чтение сохранённой пары даёт самостоятельное узкое основание. Прирост>5% локален; его нельзя распространять на все Contains и на неподтверждённого обычного потребителя.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5601/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5601/results/attempt-0001/metrics.json>); [3: 111-post-p55-transfer-plan-2026-11-01.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md>).

### N26. FIX-P60 — экономия памяти с регрессией ответа

**Опыт и изменение.** P60: удалить повторные записи одной пары, созданные full/prefix/structure путями. Хранить уникальную пару один раз при сохранении семантики допуска и полного ответа.

**Точность.** Учебные controls4/4→4/4. Новый запрос с please: A отказывается, B отвечает без «please»; полноценного нового правильного ответа это не доказывает.

**Полезный показатель до → после.** Память1704→1164bytes (−31.69%); pairs12→8 (−33.33%); дубликаты4→0; уникальных пар8→8.

**Ухудшения и пределы.** На полярном случае неполный ответ появляется вместо отказа: ложное принятие0/1→1/1. Train333000→16104bytes и236→5ms сравнивают разные полные/прогретые пути, не причинный выигрыш.

**Допуск: HOLD_UNCONDITIONAL.** Экономия>5% есть, но исходный опыт выявляет ухудшение корректности. В очередь допустим только ограниченный ремонт дублирования с сохранением отказа/полного ответа; безусловное включение по правилу пользователя незаконно.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6001/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6001/results/attempt-0001/metrics.json>); [3: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6001/results/attempt-0001/arms.json>); [4: 111-post-p55-transfer-plan-2026-11-01.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md>).

### N27. FIX-P62 — ремонт чужого ответа по слабому совпадению

**Опыт и изменение.** P62: full source signature допускает ответ по нерелевантному частичному совпадению. Применять исходный source guard с порогом.35 до выдачи сохранённой пары.

**Точность.** Компонент1/3→3/3 (+200%; +66.67п.п.); обычный consumer2/2→2/2.

**Полезный показатель до → после.** Чужие ответы2→0; правильный отказ на неизвестное0→1. Две из трёх обычных веток обходят проверяемую память.

**Ухудшения и пределы.** Ресурсы плеч отдельно не измерены; разница времени смешана с JIT. Нельзя приписывать компонентный прирост всему ordinary flow.

**Допуск: REPAIR.** Отказ вместо чужого восстановленного ответа — самостоятельное основание узкого ремонта. Измеренного роста обычной точности нет.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6201/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6201/results/attempt-0001/metrics.json>); [3: 111-post-p55-transfer-plan-2026-11-01.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md>).

### N28. FIX-P64 — ремонт допуска кандидата в каскаде

**Опыт и изменение.** P64: найденный верхний кандидат непригоден, но поиск прекращается до законного нижнего кандидата. Проверять допустимость кандидата до окончательного выбора/ограничения списка и искать допустимый следующий в установленной границе.

**Точность.** Компонент1/2→2/2 (+100%; +50п.п.), ordinary2/2→2/2; сохранённые законные controls без потерь.

**Полезный показатель до → после.** Lookup1→2, в худшем описанном случае1→3. Недопустимые target0 в обоих плечах на14 чтениях; после восстановления результат сохраняется.

**Ухудшения и пределы.** Число обращений растёт+100%/+200%; это структурная стоимость, не измеренный CPU и не бесплатное исключение. Время/память по плечам отсутствуют. Ограничение до проверки может всё ещё потерять шестого законного кандидата.

**Допуск: REPAIR.** Неверное восстановление/отбрасывание законной памяти оправдывает исправление. Полного допуска оптимизации по5% нет; обязателен отдельный контроль cap/admission.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6401/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6401/results/attempt-0001/metrics.json>); [3: 111-post-p55-transfer-plan-2026-11-01.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md>).

### N29. FIX-P65 — условный допуск проверенного нового урока

**Опыт и изменение.** P65: подтверждённая контрастная пара включается в память, раньше она не проходила допуск. Принимать только адресованную проверенную contrast-пару вместе с доказательством истинности и обычным consumer receipt.

**Точность.** Обычный consumer2/3→3/3 (+50%; +33.33п.п.); потерянных правильных строк в панели нет.

**Полезный показатель до → после.** Пары3→6, exact entries1→2 (+100% количества). Требуется один новый доверенный manifest.

**Ухудшения и пределы.** Количество записей удвоилось; bytes/retained heap не измерены. Это цена, которую нельзя объявить≤5%. Сама форма manifest не проверяет истинность. Время смешано с прогревом.

**Допуск: CONDITIONAL_P1; NO_GLOBAL_ADMISSION.** Полезный локальный выигрыш есть, но происхождение и стоимость нового урока — часть допуска. Это не бесплатное улучшение обучения и не автоматическое разрешение любой contrast-пары.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6501/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6501/results/attempt-0001/metrics.json>); [3: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6501/results/attempt-0001/arms.json>); [4: 111-post-p55-transfer-plan-2026-11-01.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md>).

### N30. R34-FB — доставка исправления в обучение памяти

**Опыт и изменение.** P42: A не превращает отказ пользователя в урок, B принимает адресованное исправление и обучает память. Довести обычную обратную связь до TrainStructuredBatchAsync правильной единицы с исходным отвергнутым ответом и границей повторного применения.

**Точность.** На двух исправленных известных запросах0/2→2/2 (+100п.п.; процент от0 не определён); исходное обучение8/8. Это не held-out обобщение.

**Полезный показатель до → после.** Принятые уроки0→2; exact entries8→10 и pairs24→30 (+25% количества), состояние меняется после действительного обучения.

**Ухудшения и пределы.** Содержание уроков и объём памяти различаются; отсутствие общей цены не позволяет объявить безусловный приоритет I. Текущая запись corrected response без Train не равна исправлению памяти.

**Допуск: REPAIR_DELIVERY; CONDITIONAL_P1.** Восстановление ожидаемой цепочки пользовательского исправления законно как ремонт доставки. Эффект — запомнить два действительно данных исправления, а не улучшить способность на новых задачах.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4201/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4201/results/attempt-0001/metrics.json>); [3: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4201/results/attempt-0001/arms.json>); [4: 106-feedback-and-transition-memory-plan-2026-10-30.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/106-feedback-and-transition-memory-plan-2026-10-30.md>).

### N31. R34-CA — ремонт адреса переходной памяти

**Опыт и изменение.** P47: адрес кеша перехода без текущего predicate сталкивает различные состояния. Включить текущий predicate в ключ перехода и сохранить это различие при чтении/восстановлении.

**Точность.** Контроль8/12→12/12 (+50%; +33.33п.п.); reference4/4→4/4; правильные ответы не потеряны.

**Полезный показатель до → после.** Разрушительные коллизии4→0; все коллизии8→4; уникальные адреса4→8 (+100%).

**Ухудшения и пределы.** Удвоение числа адресов — цена состояния; bytes/устойчивое время отдельно не квалифицированы. Перенос доказан для узкого экспериментального E30PredicateCache, не для любого product dispatcher.

**Допуск: REPAIR; CONDITIONAL_P1.** Ошибка адресации сохранённого ответа — самостоятельное основание. Локальная точность выросла>5%, но общий ресурсный/потребительский допуск ещё нельзя объявить закрытым.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4701/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4701/results/attempt-0001/metrics.json>); [3: 106-feedback-and-transition-memory-plan-2026-10-30.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/106-feedback-and-transition-memory-plan-2026-10-30.md>).

### N32. EVID-P115-ACCEPTANCE — приёмка уже перенесённого snapshot

**Опыт и изменение.** P115: пересчёт статистики корпуса для каждого кандидата против неизменного snapshot с проверкой версии. Подтвердить ordinary/native consumer, cold build, инвалидирование и восстановление уже внесённого snapshot; не считать приёмку новым алгоритмом.

**Точность.** Ответы2/4→2/4; blind10/20 — повторения тех же четырёх запросов, не20 независимых задач; порядок кандидатов совпадает.

**Полезный показатель до → после.** Warm stemming156→4 (−97.44%); allocated159344→28336bytes (−82.22%). Cold build6800bytes,4 stems,10.0631ms; cold+warm35136bytes (−77.95% против159344).

**Ухудшения и пределы.** Retained memory самого snapshot не измерена A/B. Wall-time на4 запросах не квалифицирован. Одинаковое число документов не гарантирует одинаковые данные; такой испорченный receipt отвергнут.

**Допуск: VERIFY; CONDITIONAL_P1_COST.** Сильный локальный ресурсный эффект есть, но эта строка — недостающая приёмка существующего переноса. Для полного I остаются retained memory, холодный путь и обычное восстановление.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11501/results/attempt-0001/report.md>); [2: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11501/results/attempt-0001/arms.json>); [3: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11501/results/attempt-0001/metrics.json>); [4: lineage-controls.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11501/results/attempt-0001/raw/lineage-controls.json>).

### N33. R31-QD — ремонт диагностики размерности

**Опыт и изменение.** P18: запрос размерности63 попадает в обученный64-мерный поиск и молча возвращает первый ответ. До Search проверять размерность и возвращать именованный QUERY_DIMENSION_MISMATCH.

**Точность.** Законные TRAIN8/8 и EXAM4/4 без изменения. На незаконном запросе правильный отказ0/1→1/1.

**Полезный показатель до → после.** До: review-first с cosine1 при неверной форме; после: отказ до поиска. Размер состояния11008bytes не меняется.

**Ухудшения и пределы.** Процент от нулевого числа правильных отказов не определён. Это не увеличение точности модели на валидных запросах.

**Допуск: REPAIR.** Ложный ответ вместо диагноза неправильного входа — самостоятельное основание очереди.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1801/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1801/results/attempt-0001/metrics.json>); [3: 97-learned-knn-state-integrity-transfer-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/97-learned-knn-state-integrity-transfer-plan-2026-10-29.md>).

### N34. R31-OO — ремонт владения выученной памятью

**Опыт и изменение.** P19: внешнее изменение входного DTO после Train изменяет ответы A; B хранит owned snapshot. Копировать принимаемые данные и возвращать безопасный снимок результата в назначенном typed контракте.

**Точность.** До внешней мутации12/12 оба; после A9/12, B12/12 (+33.33%; +25п.п.). TRAIN7/8→8/8, EXAM2/4→4/4.

**Полезный показатель до → после.** Память B сохраняет выученный ответ без дополнительного Train; allocated101128bytes у обоих на12 Execute.

**Ухудшения и пределы.** Наблюдаемое время1.9115→2.1502ms (+12.49%) — ухудшение wall-time в коротком измерении, не исключаемый CPU. JIT и initial train не разделены.

**Допуск: REPAIR.** Защита ответа от чужой мутации — прямой самостоятельный ремонт. Нельзя на основании этой записи объявить безусловный ресурсный приоритет I.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1901/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1901/results/attempt-0001/metrics.json>); [3: 97-learned-knn-state-integrity-transfer-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/97-learned-knn-state-integrity-transfer-plan-2026-10-29.md>).

### N35. R31-RF — проверка полного пересчёта обученной памяти

**Опыт и изменение.** P17: после изменения словаря переобучить/перекодировать только новые записи против всего корпуса. Сохранять согласованность encoder и всех старых векторов; подтвердить уже имеющийся full refit обычным regression case.

**Точность.** Старые TRAIN6/8→8/8 (+33.33%), EXAM4/4→4/4; все строки11/13→13/13 (+18.18%). Исходное удержание S0=8/8.

**Полезный показатель до → после.** Неполный пересчёт теряет два старых ответа, полный их сохраняет. Это инвариант согласованной памяти, не новый способ обучения.

**Ухудшения и пределы.** Окно encode/rebuild allocations +39184bytes (+145.43%); время+.1849ms (+19.90%). Total window allocations−12.11% смешаны с порядком/JIT и не отменяют цену rebuild.

**Допуск: VERIFY_REPAIR.** Ремонт потери старых ответов законен независимо от нового A/B проверочной задачи. Full Train уже пересчитывает корпус; повторная реализация не нужна. Не заявлять оптимизацию без цены.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1701/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1701/results/attempt-0001/metrics.json>); [3: 97-learned-knn-state-integrity-transfer-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/97-learned-knn-state-integrity-transfer-plan-2026-10-29.md>).

### N36. R32-DR — условная интеграция добавления уроков

**Опыт и изменение.** P36: Train только новой строкой заменяет корпус; Train полным корпусом удерживает старые ответы. Если обычный caller действительно хочет append, он должен явно сформировать полный корпус или вызвать отдельный назначенный append API.

**Точность.** До2/3 оба; после A1/3, B3/3 (+200% относительно A; +66.67п.п.). B сохраняет два старых и добавляет новый ответ.

**Полезный показатель до → после.** Количество хранимых строк1→3 (+200%); B получает дополнительные старые предъявления. Это различие данных, не новый learner.

**Ухудшения и пределы.** Train по контракту означает замену; ошибочный обычный caller, использующий замену вместо append, не установлен. Изменить сам Train на append было бы сменой контракта. Цена хранения увеличена.

**Допуск: HOLD_WITHOUT_CALLER; CONDITIONAL_REPAIR.** Опыт доказывает ожидаемую семантику replacement. В независимую очередь ремонта законно входит только найденная ошибочная интеграция или явно требуемый append consumer; общей ошибки Train он не доказывает.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3601/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3601/results/attempt-0001/metrics.json>); [3: 99-new-learning-consumer-transfer-addenda-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/99-new-learning-consumer-transfer-addenda-2026-10-29.md>).

### N37. R32-DQ — ремонт ответа на пустой запрос

**Опыт и изменение.** P37: пустой вектор запроса выбирает первую метку, которая меняется с порядком записи. Отвергать запрос без содержательных признаков вместо выдачи произвольной обученной метки.

**Точность.** Прямой и обратный порядок:3/4→4/4 (+33.33%; +25п.п.) каждый;3 законных контроля сохранены.

**Полезный показатель до → после.** На пустом запросе A выдаёт CSharp либо Tests, B выдаёт явное отсутствие ответа000000.

**Ухудшения и пределы.** Новых полезных признаков и нового обучения нет; собственная ресурсная цена не установлена.

**Допуск: REPAIR.** Доказанное ложное чтение памяти без основания — самостоятельный диагностический ремонт; не новый рост способности на содержательных запросах.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3701/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3701/results/attempt-0001/metrics.json>); [3: 99-new-learning-consumer-transfer-addenda-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/99-new-learning-consumer-transfer-addenda-2026-10-29.md>).

### N38. R32-SR — условный перенос выбора навыка

**Опыт и изменение.** P32: чистый Jaccard даёт ничью; сигнал обученного generic KNN меняет выбор и фактически выполненные шаги. Подключить наблюдаемый типизированный learned score в назначенный SkillRetriever consumer, проверив происхождение сигнала.

**Точность.** Все6/12→12/12 (+100%; +50п.п.); EXAM2/4→4/4; TRAIN4/8→8/8. Изменились6 реально выбранных/загруженных/исполненных шагов.

**Полезный показатель до → после.** Источник полезного сигнала — generic Knn route prediction. Оценки собственно SkillValueUnit=0 в обоих плечах, поэтому его обучаемость опытом не подтверждена.

**Ухудшения и пределы.** Ресурсная граница ordinary каталога не установлена. A на этой панели вырожден в постоянный review из-за ничьих; результат не доказан для полного каталога.

**Допуск: CONDITIONAL_P1.** Есть полезный локальный эффект>5%, но его нельзя приписать другой единице или считать глобально допущенным без стоимости и обычного потребителя.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3201/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3201/results/attempt-0001/metrics.json>); [3: 99-new-learning-consumer-transfer-addenda-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/99-new-learning-consumer-transfer-addenda-2026-10-29.md>).

### N39. R32-EX — проверка границы известной формулы

**Опыт и изменение.** P29: обученная ExpressionCompletion memory хранит известные правые части и отказывается на неизвестных. Сохранить явный отказ для неизвестной формулы при обычном потреблении; не объявлять память решателем новых выражений.

**Точность.** Холодная память0/10; после обучения8/8 известных (4 точных+4 вариантов пробелов) и2 корректных отказа. Переставленные неправильные RHS не воспроизведены0/8.

**Полезный показатель до → после.** Постоянный ответ даёт2/8. Это доказательство памяти известных пар; самостоятельного A/B нового wrapper отказа нет.

**Ухудшения и пределы.** Обобщение на новые формулы и математический solver не проверены. Цена отдельного переноса не измерена.

**Допуск: VERIFY_REPAIR.** Честная граница ответа и отказа допустима как приёмка восстановления известных пар. Не считать эту строку ещё одним улучшением обучения.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P2901/results/attempt-0001/report.md>); [2: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P2901/results/attempt-0001/arms.json>); [3: comparison.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P2901/results/attempt-0001/comparison.json>); [4: 99-new-learning-consumer-transfer-addenda-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/99-new-learning-consumer-transfer-addenda-2026-10-29.md>).

### N40. P35 — ремонт знаменателя голосования

**Опыт и изменение.** P35: порог большинства сравнивается с запрошенным k, хотя найдено меньше соседей. Использовать фактическое число найденных соседей в знаменателе; сохранить политику пустого набора.

**Точность.** В изменяемом правиле2/3→3/3 (+50%; +33.33п.п.); приобретён случай одного соседа, потерянных нет. n=0 вне изменения.

**Полезный показатель до → после.** Одиночный реальный выученный голос перестаёт теряться; train264bytes одинаков.

**Ухудшения и пределы.** Read allocations6128→9584bytes (+56.40%); короткое время смешано с прогревом. Память нарушает безусловный допуск по5%.

**Допуск: REPAIR; NO_UNCONDITIONAL_P1.** Неправильный знаменатель теряет уже выученный ответ и даёт самостоятельное основание ремонта. Однако как оптимизацию/улучшение с ценой≤5% эту запись представлять нельзя.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3501/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3501/results/attempt-0001/metrics.json>); [3: 100-neighbor-vote-divisor-transfer-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/100-neighbor-vote-divisor-transfer-2026-10-29.md>).

### N41. TEST-P49-WARM — проверка условий warm start

**Опыт и изменение.** P49:60 обновлений новой задаче с холодного состояния против600 подготовительных+60 новых с warm state. Создать отсутствующую проверку заданной дозы/сохранённого состояния, отделяя новые задачи от удержания старых; существующий bootstrapper другой дозы её не заменяет.

**Точность.** Новые8 строк A6/8→B7/8 (+16.67%; +12.5п.п.), EXAM2/4→3/4. Полный итогA6/16, B11/16 включает разный исходный навык.

**Полезный показатель до → после.** A получает60 обновлений новой задачи; для B отдельно один раз подготовлено состояние600 шагами, затем также60 новых. Эти600 — цена producer сохранённого состояния, а не дополнительные600 для каждого consumer. Allocations новых96520bytes одинаковы, однократной подготовки964840bytes.

**Ухудшения и пределы.** Часть старой панели у B:6/6→3/6; old-only4/4 сохранены. Восемь строк имеют одинаковые входы с разными gold между задачами, поэтому потеря на них не доказывает чистое forgetting согласованного набора. Различные исходные состояния и конфликтующие цели не позволяют объявить warm start как метод вредным или безусловно полезным.

**Допуск: VERIFY; NO_UNCONDITIONAL_METHOD_ADMISSION.** Само проверочное задание законно: нужно воспроизвести назначенную дозу и честно различить обучение новой задаче, old-only удержание и конфликт целей. Безусловный метод не обоснован сопоставимым началом/набором; это не доказательство общего вреда warm start.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4901/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4901/results/attempt-0001/metrics.json>); [3: arms.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4901/results/attempt-0001/arms.json>); [4: 107-warm-start-learning-coverage-2026-10-30.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/107-warm-start-learning-coverage-2026-10-30.md>).

### N42. FIX-P53-BATCH-JSON — ремонт атомарного сохранения урока

**Опыт и изменение.** P53: повреждённый JSON batch успевает частично записать урок; B проверяет/готовит целую партию до публикации. Ничего не менять при malformed batch, затем атомарно опубликовать законную партию.

**Точность.** Законная партия3/3 у обоих. При незаконной A1→2/3 из-за неположенного урока, B1→1/3; такое увеличение A не является полезной точностью.

**Полезный показатель до → после.** Незаконно добавленные записи5→0, уроки1→0. Сохранение предыдущего состояния — измеряемый эффект.

**Ухудшения и пределы.** Valid allocations481264→535136bytes (+11.19%); invalid649856→650320 (+0.0714%). Valid time4→6ms единично, не причинный стабильный замер.

**Допуск: REPAIR; NO_UNCONDITIONAL_P1.** Незаконное частичное сохранение ответа — самостоятельное основание несмотря на цену. Не называть это бесплатным ростом качества или улучшением обучения.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5301/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5301/results/attempt-0001/metrics.json>); [3: 109-p53-p55-transfer-plan-2026-10-30.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/109-p53-p55-transfer-plan-2026-10-30.md>).

### N43. FIX-P55-SOURCE-POLARITY — ремонт полярности читаемого ответа

**Опыт и изменение.** P55: сохранённая пара противоположной source polarity проходит как подходящая. Проверять source polarity до выдачи кандидата, сохраняя законные пары.

**Точность.** Ordinary3/4→4/4 (+33.33%; +25п.п.); component1/4→4/4 (+300%); training controls3/3 сохранены.

**Полезный показатель до → после.** Устранён неверный ответ с противоположной полярностью; обучение одинаковое, меняется чтение памяти.

**Ухудшения и пределы.** Ресурсы сняты для всего процесса, не по плечам. Native ordinary соответствуетA, B использует копию; собственная обычная приёмка переноса остаётся нужна.

**Допуск: REPAIR; CONDITIONAL_P1.** Первичная запись показывает ошибку восстановления смысла. Законный узкий ремонт; общего ресурсного допуска и нового улучшения learner нет.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5501/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5501/results/attempt-0001/metrics.json>); [3: 109-p53-p55-transfer-plan-2026-10-30.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/109-p53-p55-transfer-plan-2026-10-30.md>).

### N44. RF10 — ремонт диагностики сходимости

**Опыт и изменение.** План112 разделяет две логические болезни мониторинга и требует второго DPO consumer; независимого нового учебного A/B нет. Раздельно диагностировать недостаточный горизонт наблюдений и невозможный strict threshold; подтвердить фактические callers/configuration.

**Точность.** Точность обучения до/после не измерялась для этой работы.

**Полезный показатель до → после.** Монитору нужны3 наблюдения: budget<3×interval никогда не позволяет срабатывание; при budget=3×interval экономии ещё нет. При threshold0 условие difference<0 недостижимо.

**Ухудшения и пределы.** Эти логические условия не доказывают число сэкономленных updates, wall-time или сохранённое качество. Часть диагностики внесена, остаток второго потребителя не закрыт.

**Допуск: REPAIR_DIAGNOSTIC; VERIFY.** Ошибочное обещание остановки/наблюдаемости — самостоятельный дефект диагностики. В очередь законно входит его точная проверка, не выдуманная экономия обучения.

Источники: [1: 112-convergence-two-diseases-second-consumer-2026-11-01.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/112-convergence-two-diseases-second-consumer-2026-11-01.md>); [2: 118-post101-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/118-post101-addressed-repair-plan-2026-11-02.md>).

### N45. 124-S02 — ремонт приёмки и бюджета P107

**Опыт и изменение.** P107: процесс мог закончиться exit0 независимо от отрицательного контроля; исходная попытка выполнила51 чтение при лимите50. Отделять правильное отвержение от ошибки измерителя; финализировать бюджет и результат фактической попытки независимо от успеха отчёта.

**Точность.** Исходные TRAIN8/8 и EXAM4/4 одинаковы у A/B; majority/first baseline также совпадают. Ремонт измерителя не добавляет правильных ответов.

**Полезный показатель до → после.** Сохранённый ремонт различает4 честные/испорченные пары. Исторический51/50 остаётся NOT_ACCEPTED/code7; нельзя превратить51 в50 пересчётом.

**Ухудшения и пределы.** Save тогда читался из того же живого Unit, свежий Load не доказан. Ресурсный A/B не относится к этой работе.

**Допуск: REPAIR.** Ложный успешный результат и заниженный бюджет — прямые ошибки диагностики. Достаточное самостоятельное основание без процентов обучения.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10701/results/attempt-0001/report.md>); [2: codex-123-post107-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md>); [3: codex-124-post107-addressed-repair-plan-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md>); [4: codex-134-post113-transfer-and-experiment-audit-2026-11-02.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md>); [5: outcome-separation-replay.json](<C:/Projects/llm_mvp_core/experiments-v2/_audits/2026-11-02-post107-audit/s02-outcome-separation/outcome-separation-replay.json>).

### N46. 102-R1 — ремонт дублирующего адреса памяти

**Опыт и изменение.** Аудит102 поздних переносов: нормализованные одинаковые ID могли расходиться между map и индексом. До построения новой памяти отвергать повторный нормализованный ID и сохранить прежнее принятое состояние.

**Точность.** Отдельного A/B качества/точности нет; это контракт идентичности записи.

**Полезный показатель до → после.** До: одинаковый адрес может обозначать разные записи в двух представлениях. После по учёту: duplicate ID отвергается до build. Количество исторических инцидентов не измерено.

**Ухудшения и пределы.** Нельзя считать отказ на malformed dataset новым правильным ответом модели. Ресурсная цена не измерена.

**Допуск: REPAIR.** Повреждение адресов сохранённой памяти — самостоятельное основание. Уже внесённый код требует своей приёмки, а не повторной реализации.

Источники: [1: 102-late-transfer-repair-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md>); [2: 108-router-memory-transfer-audit-2026-10-30.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/108-router-memory-transfer-audit-2026-10-30.md>).

### N47. 102-R3 — ремонт владения массивами dataset

**Опыт и изменение.** R102-R3: входные features и bool-labels должны принадлежать обученной памяти, а не внешнему вызывающему коду. Копировать массивы при Train, чтобы последующая внешняя мутация не меняла выученные ответы.

**Точность.** Контроль без копий45/47 tests, финальная запись47/47. Это тесты контракта, не точность47 учебных задач и не самостоятельный опыт обучения.

**Полезный показатель до → после.** Два конкретных случая мутации перестают ломать память. Финальный журнал подтверждает47 passed; публикация исходного ремонта относится к истории.

**Ухудшения и пределы.** Время финального набора17.0755s не является сравнением A/B; процент памяти/производительности отсутствует.

**Допуск: REPAIR; HISTORICAL_IMPLEMENTATION.** Сохранность выученного ответа при внешней мутации — самостоятельное основание. В очереди остаётся только назначенный остаток/приёмка, не повторное внесение уже существующих копий.

Источники: [1: 102-late-transfer-repair-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md>); [2: 108-router-memory-transfer-audit-2026-10-30.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/108-router-memory-transfer-audit-2026-10-30.md>); [3: Run-DatasetRouterMemoryOwnershipTests-CONTROL-input-array-copy-removed.log](<C:/Projects/llm_mvp_core/experiments-v2/_runners/R102-R3-DATASET-ROUTER-MEMORY-attempt-0001/Run-DatasetRouterMemoryOwnershipTests-CONTROL-input-array-copy-removed.log>); [4: Run-DatasetRouterMemoryOwnershipTests-FINAL-after-restore.log](<C:/Projects/llm_mvp_core/experiments-v2/_runners/R102-R3-DATASET-ROUTER-MEMORY-attempt-0001/Run-DatasetRouterMemoryOwnershipTests-FINAL-after-restore.log>).

### N48. 102-R4 — ремонт атомарной замены dataset

**Опыт и изменение.** R102-R4: Clear-before-read стирал принятую память, если новая партия не могла быть прочитана. Сначала построить полный новый dataset, публиковать его только при успехе; законная пустая замена остаётся разрешена.

**Точность.** Контроль52/56 tests, финальный56/56; это четыре нарушения контракта, не рост точности модели на4/56.

**Полезный показатель до → после.** При malformed replacement старые выученные ответы сохраняются. Финальный журнал подтверждает56 passed.

**Ухудшения и пределы.** Время финального набора12.0273s не является A/B. Стоимость временного нового состояния отдельно не измерена.

**Допуск: REPAIR; HISTORICAL_IMPLEMENTATION.** Потеря принятой памяти при неудачном сохранении — прямое самостоятельное основание. Уже внесённую атомарность нельзя второй раз засчитать новым улучшением.

Источники: [1: 102-late-transfer-repair-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md>); [2: 108-router-memory-transfer-audit-2026-10-30.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/108-router-memory-transfer-audit-2026-10-30.md>); [3: Run-DatasetRouterTrainingAtomicityTests-CONTROL-clear-before-read-restored.log](<C:/Projects/llm_mvp_core/experiments-v2/_runners/R102-R4-DATASET-ROUTER-ATOMICITY-attempt-0001/Run-DatasetRouterTrainingAtomicityTests-CONTROL-clear-before-read-restored.log>); [4: Run-DatasetRouterTrainingAtomicityTests-FINAL-after-restore.log](<C:/Projects/llm_mvp_core/experiments-v2/_runners/R102-R4-DATASET-ROUTER-ATOMICITY-attempt-0001/Run-DatasetRouterTrainingAtomicityTests-FINAL-after-restore.log>).

### N49. 29-W1 — ремонт первого шага warmup

**Опыт и изменение.** P12: на первом шаге warmup получался LR0, который обновитель отвергал до действительного update. Исправить только zero-step/индексацию первого SFT шага, сохранив назначенный остальной schedule.

**Точность.** Ответы2/4 до/после и у обоих плеч; прироста точности нет.

**Полезный показатель до → после.** Назначенные эффективные updates0→4; исправлена возможность выполнить заданное обучение. Относительный процент от0 не определяется.

**Ухудшения и пределы.** E1 probability gold ухудшилась примерно на0.27636%, margin примерно на3.258%; это меньше5%, но полезного выигрыша точности всё равно нет. Глобально менять schedule опыт не разрешает.

**Допуск: REPAIR.** Потеря назначенного первого update — доказанная ошибка исполнения; чинить её законно отдельно от вопроса, улучшили ли эти четыре шага ответы.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1201/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1201/results/attempt-0001/metrics.json>); [3: 88-gradient-warmup-transfer-repair-plan-2026-10-28.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/88-gradient-warmup-transfer-repair-plan-2026-10-28.md>); [4: 95-experiment-transfers-repair-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/95-experiment-transfers-repair-plan-2026-10-29.md>).

### N50. 95-S2 — ремонт численного хвоста sampler

**Опыт и изменение.** Аудит переноса P08: численно неполная positive mass и fallback последнего индекса могут выбрать замаскированный нулевой token при draw близком1. Суммировать finite положительную массу, масштабировать draw на неё, использовать строгую границу CDF и fallback последнего положительного элемента.

**Точность.** Нового A/B точности этой численной поправки нет; исходный P08 имел2/4→2/4 фактических ответов.

**Полезный показатель до → после.** Требуемая граница: нулевой/маскированный token никогда не возвращается из хвоста. План≤64 чистых Sample calls — будущая проверка, не выполненный результат этой сессии.

**Ухудшения и пределы.** Общий перенос композиции P08 имеет отдельный memory HOLD (+155.65% allocations); исправление хвоста не снимает его. Проценты выигрыша tail-ремонта не измерены.

**Допуск: REPAIR.** Доказанная ошибка восстановления разрешённого token из распределения даёт самостоятельное основание. Нельзя приписывать ей выигрыш всей композиции.

Источники: [1: 94-experiment-transfers-audit-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/94-experiment-transfers-audit-2026-10-29.md>); [2: 95-experiment-transfers-repair-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/95-experiment-transfers-repair-plan-2026-10-29.md>); [3: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/report.md>).

### N51. 95-S3 — ремонт метаданных embedding памяти

**Опыт и изменение.** Аудит переноса памяти: PredicateIds/DomainTags остаются внешне изменяемыми массивами на входе и выходе. Делать owned copies именно этих typed metadata при Train и выдаче; immutable string Output не требует выдуманного generic deep clone.

**Точность.** Самостоятельного A/B точности нет. Будущая проверка1 строки/6 чтений — контрактная приёмка, не уже измеренный рост.

**Полезный показатель до → после.** До: внешняя мутация может изменить адрес/семантические теги принятой памяти. После по учёту копии есть; нужно подтвердить ingress и egress обычного потребителя.

**Ухудшения и пределы.** Память/время копирования не измерены. Нельзя переносить проценты P19 на этот другой typed consumer.

**Допуск: REPAIR.** Защита сохранённых признаков от чужой мутации — самостоятельное основание. Это отдельный контракт, а не повторный эксперимент обучения.

Источники: [1: 94-experiment-transfers-audit-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/94-experiment-transfers-audit-2026-10-29.md>); [2: 95-experiment-transfers-repair-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/95-experiment-transfers-repair-plan-2026-10-29.md>).

### N52. P08-CONSUMER — проверка обычного генератора

**Опыт и изменение.** P08: TopK+TopP композиция на logits восстановленного обученного состояния; задача — доказать обычного потребителя этого состояния. Проверить настоящий generator на S1, реальные hashes/optimizer step и actual emitted tokens; не подменять наблюдаемые вызовы объявленными нулями.

**Точность.** Фактические ответы2/4→2/4. На T1 P(correct)=.86879754→1 (+15.10%); неверная альтернатива13.12%→0, но sampled ответы одинаковы.

**Полезный показатель до → после.** Оба плеча используют общие4 SFT updates400→404; их нельзя засчитать эффектом sampler. Отдельная проверка consumer не является новым A/B улучшения.

**Ухудшения и пределы.** Ресурсное измерение: allocations10120→25872bytes на вызов дляT2/E1/E2 (+155.6522%);800 warmup+8000 measured calls. Общая композиция остаётся HOLD, независимо от CPU.

**Допуск: VERIFY; HOLD_GLOBAL.** Проверка реального обученного consumer законна как закрытие доказательства. Алгоритм целиком не проходит5%-критерий из-за памяти; наличие уже внесённого кода не равно допуску.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/report.md>); [2: receipt.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/sampler-resource-0001/receipt.json>); [3: warmup-trend-correction.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/sampler-resource-0001/warmup-trend-correction.md>); [4: evidence-boundary-correction.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/consumer-completion-0001/evidence-boundary-correction.md>); [5: 91-positive-transfer-followup-audit-2026-10-28.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/91-positive-transfer-followup-audit-2026-10-28.md>); [6: 92-positive-transfer-followup-plan-2026-10-28.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/92-positive-transfer-followup-plan-2026-10-28.md>).

### N53. P09-CONSUMER — проверка кеша восстановленного состояния

**Опыт и изменение.** P09: full forward против cache/chunks на одном восстановленном обученном состоянии. Подтвердить actual restored weights, cache invalidation/chunks и обычный readout без дополнительного скрытого обучения.

**Точность.** Все21504 числовых выхода побайтно совпали, top84/84 и ответы4/4 совпали; истинная точность2/4→2/4.

**Полезный показатель до → после.** Allocations7010408→7102120bytes (+1.308%); median wall-time7.015→7.289ms (+3.906%). Общие4 SFT updates не являются выигрышем кеша.

**Ухудшения и пределы.** Total time100.110→35.314ms искажён одиночным выбросом/порядком, не доказанная экономия. Небольшие регрессии<5% не создают положительного полезного выигрыша, которого здесь не показано.

**Допуск: VERIFY; NO_NEW_P1_OR_P2.** Законна приёмка эквивалентности восстановления и кеша. В этой строке нет самостоятельного измеренного улучшения, поэтому её нельзя назвать ещё одним положительным опытом.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0901/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0901/results/attempt-0001/metrics.json>); [3: 92-positive-transfer-followup-plan-2026-10-28.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/92-positive-transfer-followup-plan-2026-10-28.md>); [4: 95-experiment-transfers-repair-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/95-experiment-transfers-repair-plan-2026-10-29.md>).

### N54. 102-R7 — ремонт распознавания вырожденного измерителя

**Опыт и изменение.** P30 и P40: измеритель объявлял фактор полезным/подтверждённым там, где панель не различает плечи. Вычислять actual observations и baseline/headroom для конкретной панели; классифицировать неспособность проверить отдельно от отрицательного результата.

**Точность.** P30 ответы4/4 в обоих плечах, при любом alpha тот же winner. P40 accuracy50% у обоих, spread0; различие headroom50п.п. относится к измеримости панели.

**Полезный показатель до → после.** До: совпадение ответов/чисел могло получить неверный статус гипотезы. После по учёту отдельные evaluators используют наблюдения. Отдельного A/B ремонта нет.

**Ухудшения и пределы.** Ни положительный margin, ни сохранённые50% сами не подтверждают пользу обучаемого коэффициента. Ресурсной экономии не установлено.

**Допуск: REPAIR.** Дефект классификации экспериментального результата — самостоятельное основание диагностики; это не ещё один выигрыш обучения.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3001/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3001/results/attempt-0001/metrics.json>); [3: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4001/results/attempt-0001/report.md>); [4: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4001/results/attempt-0001/metrics.json>); [5: 102-late-transfer-repair-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md>).

### N55. 102-R8 — ремонт привязки поправки к исходному результату

**Опыт и изменение.** P31: устаревший budget/счётчик против фактического ledger; требуется effective correction, связанная с hash исходного файла. Присоединять поправку только к точно совпавшему source path/hash/полю, сохраняя исходный raw; не доверять голому названию эксперимента.

**Точность.** Ремонт не меняет ответы и не имеет собственного A/B точности. Исходный выигрыш P31 был на дословно известных строках, не на3 новых.

**Полезный показатель до → после.** Фактический учёт:6 уроков,21 запрос в бюджете+4 дополнительных=25. Историческое27/budget=false сохраняется рядом с объяснением; не переписывается как будто всегда было25.

**Ухудшения и пределы.** Применение чужой/устаревшей поправки должно быть отвергнуто. Никакой экономии27→25 не было — исправляется описание уже выполненной работы.

**Допуск: REPAIR.** Ошибочный счёт и непривязанный correction — прямое основание ремонта диагностики; количество исправленных полей не является улучшением обучения.

Источники: [1: report.md](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3101/results/attempt-0001/report.md>); [2: metrics.json](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3101/results/attempt-0001/metrics.json>); [3: 102-late-transfer-repair-plan-2026-10-29.md](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md>).

## Данные для интеграции

Каждая строка массива — одна из54 независимых работ. Поле accountingId сохраняет связь с исходным учётом; id — точное рабочее обозначение. Источники — абсолютные адреса первичных отчётов/raw либо адресного аудита, когда самостоятельного опыта у ремонта нет.

```json
[
{"accountingId":"N01","id":"135-R01-INTERFACE","kind":"ремонт интерфейсного контракта измерения","experiment":"Не самостоятельный опыт. TranslationMachine перестал реализовывать ILearningMachine: у EvaluateAsync появился третий optional аргумент expectedStateVersion, которого нет в интерфейсе.","change":"Вернуть EvaluateAsync(evalSet,ct) ровно с двумя аргументами; проверку названной версии вынести в отдельный EvaluateAtVersionAsync(evalSet,expectedStateVersion,ct).","accuracy":"Никакого A/B точности для исправления сигнатуры нет. Эта работа не относится к выбору owner/payload P111.","beforeAfter":"До: метод с третьим, хотя и optional, аргументом не реализует двухаргументный член интерфейса. Текущий исходник: EvaluateAsync на строках727–732 имеет ровно два аргумента и делегирует в отдельно названный EvaluateAtVersionAsync на743–746.","regressions":"Историческое несобираемое состояние описано в комментарии719–726; новая сборка здесь не запускалась. Совпадение текущей сигнатуры прочитано напрямую; прохождение остальных потребителей и runtime этим не доказано. Проценты не измерены.","admission":"REPAIR_CONTRACT; NO_LEARNING_GAIN_CLAIM","reason":"Основание — конкретный нарушенный контракт измерения и невозможность собирать соответствующую область, а не улучшение обучения. Это уже внесённый ремонт интерфейса; самостоятельный положительный эксперимент или приоритет I/II из него не следует.","sources":["C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N02","id":"135-R01-CONCURRENCY","kind":"ремонт состояния","experiment":"Разбор конкурентных исправлений и поздней оценки состояния в аудите134; наблюдённая производственная гонка не заявлена.","change":"Публиковать изменение памяти и её оценку с проверкой версии; поздняя оценка S0 не должна заменить оценку S1.","accuracy":"Нового A/B ответов нет.","beforeAfter":"Разобран сценарий: два читателя v7 публикуют v8, последнее сохранение теряет первое изменение. Отдельно разобрано присоединение старой оценки к новой памяти. Это логические контрпримеры, не измеренная частота сбоев.","regressions":"CAS частично есть по учёту; цепочка выбранного payload и версия оценки остаются границами. Проценты потерь/нагрузки не измерены.","admission":"REPAIR","reason":"Защита уже принятых исправлений и достоверности диагноза — самостоятельное основание; проценты выигрыша обучения здесь неприменимы.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N03","id":"135-R09-PAIR","kind":"ремонт адреса памяти","experiment":"P106 и аудит123/134: служебный PAIR-ключ мог включать смысловой допуск с нулевой похожестью; разные писатели и читатели используют связанные ключи.","change":"Единообразно канонизировать ключ у записи, исправления, чтения и восстановления; отдельно подтвердить RULE/import и законный положительный случай.","accuracy":"Самостоятельного A/B этого переноса нет. Исправление аннотации P106 не равно исправлению всех ролей слов.","beforeAfter":"До: служебная пара могла считаться смысловым основанием. Канонизация уже внесена по учёту; полный обычный цикл записи/чтения/загрузки ещё требует приёмки. Процент не измерен.","regressions":"У P106 пунктуационные случаи сохраняли 0/2; это предел исходного опыта, не исчезнувший благодаря канонизации.","admission":"REPAIR","reason":"Законный ремонт ложного допуска и восстановления ключей; нельзя выдавать за новый рост точности модели.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10601/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10601/results/attempt-0001/26-arm-B-reads-and-verdicts.json"]},
{"accountingId":"N04","id":"135-R02-EXECUTION","kind":"ремонт диагностики запуска","experiment":"Аудит134 общей границы build/run и прежних попыток; независимого учебного A/B нет.","change":"Разделить состояние окончания сборки и исполнения, связывать receipt с реально выполненным callback и потреблёнными файлами, применять общий deadline.","accuracy":"Точность модели этим не измеряется.","beforeAfter":"Разобран путь: общий terminal после build подавляет run, callback=0, но exit=0. Должен получиться явный незапуск, а не успешный результат. Отдельные дочерние ремонты относятся к той же работе.","regressions":"Число реально случившихся подавлений и экономия времени не измерены. Новый прогон в этой исследовательской сессии не выполнялся.","admission":"REPAIR","reason":"Ошибка диагностики выполнения — прямое самостоятельное основание очереди; не оптимизация обучения.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N05","id":"T110","kind":"квалификация численного ядра; глобальный перенос удержан","experiment":"P110: скалярное и векторное вычисление одной проекции на одном обученном состоянии; независимый double-oracle.","change":"Проверить численную эквивалентность сохранённых выходов и границы допуска, не переносить локальное ядро как ускорение всего обучения.","accuracy":"A=12/16, B=12/16; прирост точности 0. Общее обучение 0/16→12/16 не является A/B ядра.","beforeAfter":"Макс. абсолютная ошибка 1.3435864723e-6→5.6376216051e-7 (−58.04%); макс. относительная 2.0233471079e-5→3.2370326193e-5 (+59.98%). Argmax-расхождения A/B=0.","regressions":"Ошибка отдельной координаты token0 выросла примерно на188.77%, token1 на22.48%. Внутреннее время ядра .5039→.7418ms (+47.21%), внешнее 3.8354→1.3959ms; порядок/JIT не отделены. Это не доказанный общий speedup.","admission":"HOLD_GLOBAL; VERIFY_SAVED","reason":"Улучшение одного агрегата не отменяет ухудшение другого; допустима сохранённая квалификация эквивалентности. Полезный выигрыш обычного учебного потребителя не доказан.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11001/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11001/results/attempt-0001/raw/scalar-equivalence.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11001/results/attempt-0001/raw/corruption-control.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-130-p110-numeric-projection-disposition-2026-11-02.md"]},
{"accountingId":"N06","id":"T112","kind":"ремонт измерителя и происхождения данных","experiment":"P112: разбиение по строкам против разбиения по семье материала с обучением одной и той же единицы.","change":"Связать разбиение, обучение, отчёт и контроль с одним неизменным фактически прочитанным материалом; исключить пересечение родственных примеров.","accuracy":"EXAM 2/4→2/4, DEV 1/2→1/2; постоянный ответ также2/4. В экзамене приобретены2 и потеряны2 строки.","beforeAfter":"Пересекающихся семей2→0; DEV confidence .7143/.8845→.3333. Это снятие утечки и ложной уверенности, а не рост качества.","regressions":"Баланс меток меняется4R/2B→3R/3B; семейный контроль меняет состав обучения. Нельзя объявить уменьшение confidence регрессией правильности или новым выигрышем точности.","admission":"REPAIR","reason":"Доказанная нечестность независимости проверки оправдывает ремонт диагностики. Новый перенос должен подтверждать фактически потреблённые bytes; отдельного A/B такого переноса нет.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11201/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11201/results/attempt-0001/raw/material-ledgers.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11201/results/attempt-0001/arms.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-132-p112-material-lineage-transfer-2026-11-02.md"]},
{"accountingId":"N08","id":"T113","kind":"условная политика представления","experiment":"P113: общая L2-нормировка текста и предикатов против отдельной нормировки каналов с равными весами.","change":"Сохранять совместную нормировку по умолчанию; рассматривать отдельный opt-in только для названного потребителя и его полезной метрики.","accuracy":"EXAM 4/4→4/4, retention8/8→8/8, короткие запросы4/4→4/4; приобретено0, потеряно0.","beforeAfter":"Доля энергии предикатов длинных запросов около .00124→.5; искусственный арифметический margin −.000935→+.499376. Это изменение геометрии, а не дополнительный правильный ответ.","regressions":"Доля предикатов короткого канала падает примерно на9.48% как предусмотренное перераспределение; само по себе это не регресс полезной метрики. Allocations1237464→1051624 не квалифицированы по порядку/JIT.","admission":"HOLD_GLOBAL; CONDITIONAL","reason":"Нет измеренного полезного выигрыша обычного потребителя. Ни большая энергия, ни её уменьшение сами по себе не дают приоритет I/II; глобальный перенос исходным опытом не обоснован.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11301/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11301/results/attempt-0001/raw/channel-receipts.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11301/results/attempt-0001/raw/arithmetic-fixture.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-133-p113-channel-balance-disposition-2026-11-02.md"]},
{"accountingId":"N09","id":"135-R08-P114","kind":"ремонт нулевого веса DPO","experiment":"P114: A обрабатывает пары весов[1,0] с нижним порогом.01; B исключает нулевую; C содержит только положительную.","change":"Не проводить Forward/Backward и не учитывать нулевую пару в знаменателе; подтверждать совпадение B с C.","accuracy":"Обычная точность2/4→2/4; постоянный ответ также2/4. Рост точности не найден.","beforeAfter":"Forward8→4 и Backward4→2 (−50% вызовов); нормированный вес положительной пары .990099→1. B совпадает с P-only C по весам/моментам в допусках.","regressions":"Осталась отдельная граница tiny-positive floor; all-zero случай исходным учебным опытом не запускался. Проценты времени/памяти обычного потребителя не установлены.","admission":"REPAIR; CONDITIONAL_P1_COST","reason":"Нулевая пара не должна менять градиент — самостоятельная ошибка. Вдвое меньше вызовов доказывает локальный объём работы, но не полный ресурсный допуск и не прирост качества.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11401/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11401/results/attempt-0001/raw/arms.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11401/results/attempt-0001/raw/fingerprints-and-comparisons.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N10","id":"124-S03","kind":"ремонт диагностики допуска","experiment":"P104 и аудит123: router доверял сохранённым PassedGate и порогам, допуская нечисловые/недопустимые наблюдения.","change":"Проверять конечность и диапазон наблюдений, применять доверенные пороги перед обычным Route/ExecuteAsync.","accuracy":"Учебного A/B нет; acceptance gate не является точностью ответа.","beforeAfter":"До: сохранённый флаг мог разрешить запуск при негодной оценке. После по учёту код finite+trusted threshold внесён; собственные отрицательные и обычные случаи ещё отделены от наличия кода.","regressions":"Частота таких checkpoint и ресурсная цена не измерены. Старые тесты с постоянным хорошим gate эту ошибку не проверяли.","admission":"REPAIR","reason":"Ложное заключение измерителя о готовности — самостоятельное основание. Не требуется выдуманный процент улучшения обучения.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10401/results/attempt-0001/report.md"]},
{"accountingId":"N11","id":"124-S06","kind":"ремонт восстановления оценки","experiment":"P104/аудит123: local threshold .75, но импорт мог принять accuracy .1; второй ImportCheckpoint копировал PassesGate напрямую.","change":"Один валидатор конечности, диапазона и порога.75 для обоих путей загрузки до изменения состояния и повторного export.","accuracy":"Сравнения точности обучения нет; .1 и .75 — значения допуска, а не результаты A/B.","beforeAfter":"До: .1 не проходит местный gate, но проходит импорт при валидных ссылках. Требуемый результат: оба входа отвергают .1/.749 и одинаково принимают законную .75. Код общего валидатора уже указан в учёте.","regressions":"Это достижимое расхождение контракта, а не установленный исторический инцидент с плохим checkpoint. Производительность не измерена.","admission":"REPAIR","reason":"Ошибка восстановления и повторного сохранения диагноза прямо подпадает под самостоятельный ремонт.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10401/results/attempt-0001/report.md"]},
{"accountingId":"N12","id":"127-M02b","kind":"ремонт восстановления читаемого ответа","experiment":"P101: на сохранённых состояниях генератор выбирал одиночный байт182, не образующий законченный символ.","change":"Отличать маску допустимых кандидатов до выбора от обрезания незавершённого хвоста уже готовой строки; подтвердить допустимые UTF8 продолжения.","accuracy":"На точках25 и50 ограничение допустимых однотокенных ответов вернуло EXAM0/2→1/2 (+50п.п.; относительный процент от0 не определён). На100 не исправило ответ.","beforeAfter":"Из20 сохранённых чтений6 имели недопустимый верхний токен;2 стали правильными. В14 законных строках tokenID и logit побайтно совпали. 0 новых optimizer updates.","regressions":"На100 выбирается законная, но неверная звёздочка. Маска только для одиночного символа не разрешает автоматически общую UTF8 генерацию. Текущий trim-tail не доказывает предвыборную маску.","admission":"REPAIR","reason":"Восстановление читаемого ответа имеет самостоятельное основание; эффект локален и не устраняет ухудшение модели от продолжения обучения.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10101/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N13","id":"124-S01","kind":"ремонт отрицательных контролей","experiment":"P104 и аудит123/134: старые «контроли» возвращали false по enum, не пропуская повреждённый объект через настоящий измеритель.","change":"Проверять честную и намеренно повреждённую запись одним реальным путём; отделять исключение выполнения от правильного отрицательного результата.","accuracy":"P104 attempt2 имел законные2/5→4/5 и правильные исходы3/5→5/5; это исходный опыт, а не выигрыш ремонта контролей.","beforeAfter":"Новая сохранённая запись ремонта содержит9 различимых пар;16 старых условий остаются непокрыты. Число boolean=21 не равно21 доказанному контролю.","regressions":"SafeRun, превращающий любое исключение вfalse, может принять поломку за успешное отвержение. Полного процента исправленных измерителей нет.","admission":"REPAIR","reason":"Явный дефект диагностики допуска — самостоятельное основание. В очередь входит достоверность проверки, а не повторное увеличение точности.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10401/results/s02-repair-0001/negative-controls-replay.json"]},
{"accountingId":"N14","id":"124-S05","kind":"ремонт согласованности сохранённого смысла","experiment":"P106 raw armB: исправленные pattern/signature сосуществуют с «проверь»=NOUN и «файл»=VERB/IMPERATIVE, lemma «фай».","change":"Исправлять единый semantic result вместе с токенами, POS, ролями и тегами; не менять только два верхних поля.","accuracy":"Пунктуационные строки сохраняют0/2. Для этого целостного ремонта независимого A/B ещё нет.","beforeAfter":"До: аннотация и роли одних слов противоречат друг другу; две верхние строки могут выглядеть исправленными. Требуемое после: согласованный разбор и сохранение/чтение того же результата.","regressions":"Суффиксное правило ошибочно считает «файл» глаголом; исправление только кеша аннотации ошибку токенизации не снимает. Ресурсы не измерены.","admission":"REPAIR","reason":"Есть первичный raw с противоречивым восстановленным разбором. Это законный ремонт целостности ответа, но без заявленного процента роста точности.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10601/results/attempt-0001/26-arm-B-reads-and-verdicts.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10601/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N15","id":"124-S07","kind":"ремонт отчёта и сохранённых выводов","experiment":"P103: raw вывод о забывании был опровергнут — сравнивались два независимых Train, не продолжение одной траектории; создана адресная поправка.","change":"Обычный reporter должен применять проверенную поправку по source hash/path/JSON pointer и сохранять исходную историю.","accuracy":"Никакого A/B точности ремонта нет. Старое утверждение о забывании нельзя считать измеренным эффектом.","beforeAfter":"До: обычный отчёт может повторить уже опровергнутый вывод. Helper чтения поправок существует; соединение с обычными reporters остаётся отдельной работой.","regressions":"Поправка чужого hash должна отклоняться. Нельзя молча перезаписать исторический raw или улучшить счётчик качества переименованием.","admission":"REPAIR","reason":"Исправление доказанной ошибки диагностики законно независимо от процента; наличие helper не равно исправлению пользовательского отчёта.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10301/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md","C:/Projects/llm_mvp_core/experiments-v2/_audits/2026-11-02-post107-audit/codex-claim-corrections.json"]},
{"accountingId":"N16","id":"124-S08","kind":"проверка перенесённого tie-break","experiment":"P85 и последующий ремонт MethodKnn: при равном числе голосов учитывать расстояние до ближайшего примера перед ordinal fallback.","change":"Подтвердить уже внесённое разрешение ничьей в native predictor и обычном SemanticCompilerFacade; не повторять реализацию.","accuracy":"Самостоятельной пары before/after точности именно этой проверочной работы нет. Исходный P85 про набор обязанностей нельзя целиком приписать tie-break.","beforeAfter":"Компонентная сохранённая проверка35/35; native targeted и достижение predictor через facade по аудиту не подтверждены. 35/35 — контрольные случаи, не точность на35 новых задачах.","regressions":"Сборка общего Tests и фильтр173 чужих тестов не подтверждают этот consumer. Ресурсного сравнения нет.","admission":"VERIFY","reason":"Законна как недостающая приёмка уже перенесённой ошибки чтения памяти; это не отдельный экспериментальный выигрыш или новый алгоритм.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P8501/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N17","id":"124-S13","kind":"ремонт определения правильного ответа","experiment":"P102: calibration сравнивает полное Trim==gold, generation использует StartsWith(gold); actual IDs подменялись повторным Encode готовой строки.","change":"Один evaluator для калибровки и результата; хранить реально выданные ID отдельно от повторно кодированных; отвергать противоречащий хвост.","accuracy":"Исходный P102: подсказанные токены36/47→37/47 (+2.78% относительно счёта), собственная генерация7/47; это разные способы чтения, не выигрыш scorer-ремонта.","beforeAfter":"До: противоречащий ответ с правильным началом может считаться верным. После должно проверяться полное назначенное смысловое условие. Собственного A/B ремонта нет.","regressions":"В исходном P102 exam loss1.5593541→1.5593718 (+0.00114%) ухудшилась при росте token accuracy; новое обучение не требуется для честного пересчёта verdict.","admission":"REPAIR","reason":"Явно разные определения правильности и ненастоящие emitted IDs — самостоятельный дефект диагностики, а не доказанное улучшение обучения.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10201/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N18","id":"127-M05a","kind":"ремонт наблюдаемости поиска","experiment":"P66: KnnReached выводился из имени выбранного adapter, а не из события попытки поиска.","change":"Записывать search_attempted, число кандидатов/допустимых, выбранный lesson и final source в реальном поисковом пути.","accuracy":"Исходный P66 открыл2/2 ранее скрытых полярностных случая вместо0/2; качество перевода этим не улучшено. Этот ремонт не имеет A/B ответов.","beforeAfter":"Из старого raw можно узнать selected/not-selected; attempted=NOT_MEASURED. Unknown нельзя заменитьfalse. Новый receipt должен различать ранний exact, поиск без допустимых и выбранного соседа.","regressions":"Старые raw не позволяют восстановить пропущенное событие. Процент посещений и выигрыш скорости не измерены.","admission":"REPAIR","reason":"Ложное наблюдение исполнения — самостоятельная ошибка диагностики; ремонт законен без выдуманного A/B.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6601/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6601/results/attempt-0001/frozen-flow-raw.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N19","id":"127-M05b","kind":"ремонт счёта работы","experiment":"P66: pairSimilarityEvaluations отражал число решений допуска/пар, но повторные физические ComputeTokenOverlap не учитывались.","change":"Считать каждый реальный callable в едином wrapper с категориями admission/grid/oracle/edge; отдельно хранить уникальные пары и обновления.","accuracy":"Точность ответа не является предметом этой работы.","beforeAfter":"Исторические12 admission evaluations не равны полному числу вызовов; аудит сохраняет12+1 и дополнительную оценку26 (source-derived39). Требуется увеличивать ledger на1 при дополнительном вызове даже той же пары.","regressions":"Это исправление заниженной стоимости, а не сокращение39→12 и не найденная экономия. Собственного измеренного после ещё нет.","admission":"REPAIR","reason":"Доказанный дефект измерителя стоимости сам оправдывает очередь; новых Train для честного описания не нужно.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6601/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6601/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N20","id":"T108-CARRIER","kind":"ремонт передачи признаков","experiment":"P108: чтение предикатов только из string[] против типизированного чтения допустимых коллекций.","change":"Сохранять предикаты из List/read-only carriers, а неподдерживаемый/испорченный carrier явно отвергать.","accuracy":"Маршрутизация12/12→12/12, controls8/8→8/8; ответы уже копировались из памяти и не доказывают качество признаков.","beforeAfter":"Доставленные предикаты4/12→12/12 (+200% относительно; +66.67п.п.); именованный отказ на недопустимом carrier0/2→2/2 (относительный процент от0 не определён).","regressions":"Историческая трасса потреблённых файлов/финализации неполна; не считать repair receipt новым учебным результатом. Ресурсного A/B нет.","admission":"REPAIR","reason":"Молчаливая потеря сохранённых/переданных признаков и отсутствие диагноза — самостоятельное основание; не рост точности модели.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10801/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10801/results/attempt-0001/arms.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/126-p108-predicate-carrier-transfer-2026-11-02.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10801/results/m06-repair-0001/m06-measurer-repair-receipt.json"]},
{"accountingId":"N21","id":"FIX-KDP","kind":"единая работа чтения доменной памяти и адресного исправления","experiment":"KDP сравнивает базовое чтение с доменными принципами; KDR отдельно добавляет маршрутизацию и функции. P111/T111 — дочерняя проверка владельца исправления, не55-я независимая работа.","change":"Включить доменные правила через законный конструктор, сохранить единицу-владельца от выбора до изменения и восстановления состояния.","accuracy":"KDP S1: RU5/8→8/8 (+60%), HE6/8→8/8 (+33.33%), OR5/8→8/8 (+60%); CS6/8 и EN8/8 без изменения. KDR: fixed10/46→domain24/46→domain+functions28/46.","beforeAfter":"Чистая добавка функций KDR24→28 (+16.67%); полный10→28 смешивает два фактора. P111 правильный owner1/2→2/2, чужие изменения1→0; allocations33752→33752 на переход.","regressions":"KDP HE-retention4/4→3/4 возникает в общей учебной траектории, не доказан как A/B регресс принципов. В KDR RU/HE routes0/8 остаются провалом. Ресурсы в основном общепроцессные; полная граница−5% не подтверждена.","admission":"CONDITIONAL_P1; REPAIR_OWNER","reason":"Локальный рост правильных чтений >5% есть, но широкое включение требует обычного потребителя/ресурсов. Защита адресованного сохранения исправления имеет самостоятельное основание. Не заявлять ускоренное обучение.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KDR01/results/attempt-01/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/KDR01/results/attempt-01/raw.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11101/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11101/results/attempt-0001/arms.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/74-knn-domain-constructor-transfer.md"]},
{"accountingId":"N22","id":"FIX-P51-TOPIC","kind":"ремонт адреса точной памяти","experiment":"P51: одинаковый исходный текст в разных темах конфликтует в ExactSourceMap.","change":"Включить topic в ключ у писателя, читателя и checkpoint, сохраняя законный старый профиль.","accuracy":"Обычный consumer2/4→4/4 (+100% относительно; +50п.п.); training controls2/3→3/3 (+50%).","beforeAfter":"Ложных принятий1→0; различимых exact-ключей2→3 (+50% записей). Это предотвращение коллизии уже выученных ответов.","regressions":"ArmA полной машиной и ArmB прямым flow не дают сопоставимого времени/allocations. Heap670472→833168 снят накопительно, не как независимое A/B. Цена дополнительного ключа не квалифицирована.","admission":"REPAIR; CONDITIONAL_P1","reason":"Коллизия адресов сохранённого ответа оправдывает ремонт. Локальная точность проходит порог>5%, но полноценный приоритет I по всем ресурсам ещё не доказан.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5101/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5101/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5101/results/attempt-0001/arms.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md"]},
{"accountingId":"N23","id":"FIX-P52-TAGS","kind":"ремонт метаданных выученного ответа","experiment":"P52: для выбранной пары использовались грамматические теги противоположной стороны.","change":"Записывать и читать теги именно выбранного ответа согласованно с его токенами.","accuracy":"Точный перевод4/4→4/4; правильные tense tags при сохранении0/2→2/2 и при фактическом разборе0/2→2/2.","beforeAfter":"Улучшение тегов+100п.п.; относительный процент от нуля не определён. Это исправление содержания сохранённой пары, а не ещё четыре правильных перевода.","regressions":"Exact-map обходит KNN; полные RULE-ключи0/4 достижимы. Исправление helper само не закрывает устаревшие токены/теги, префиксные коллизии сохраняются.","admission":"REPAIR","reason":"Первичный опыт показывает неверное сохранение/восстановление грамматических метаданных. Законный самостоятельный ремонт; общего прироста точности перевода нет.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5201/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5201/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md"]},
{"accountingId":"N24","id":"FIX-P54-PHRASE-COVERAGE","kind":"условное изменение выбора фраз","experiment":"P54: greedy берёт длинную первую фразу и оставляет слово, bounded partition находит покрывающий набор.","change":"Выбирать разбиение с полным допустимым покрытием в ограниченном диапазоне; сохранить fallback и предел длины.","accuracy":"Панель3/4→4/4 (+33.33%; +25п.п.), controls3/3→3/3. На основном запросе покрыто2/3→3/3 слов.","beforeAfter":"Полезный выигрыш — ещё один правильный собранный ответ. Восьмисловный край2→3 покрытых слова всё ещё отвергается; девятисловный fallback не меняется.","regressions":"Allocated bytes19336→33872 (+75.18%). Наблюдавшееся время37983.8→130173µs (+242.71%) не отделено от JIT/порядка, но не может быть объявлено допустимым через исключение CPU.","admission":"HOLD_UNCONDITIONAL; CONDITIONAL_REPAIR","reason":"По критерию>5% без>5% иных ухудшений безусловный перенос не проходит из-за памяти. Возможен ограниченный ремонт конкретного ошибочного чтения с явно принятой ценой; независимое основание и ресурсная граница должны быть названы.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5401/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5401/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md"]},
{"accountingId":"N25","id":"FIX-P56","kind":"ремонт ложного совпадения слова","experiment":"P56: поиск через Contains принимает часть слова за требуемый целый термин.","change":"Проверять границы целого термина в назначенной ветке signature, сохраняя допустимые формы.","accuracy":"Основная панель3/4→4/4 (+33.33%), edge2/4→4/4 (+100%); controls/покрытие4/4 без изменения.","beforeAfter":"Ложное принятие1→0; исправляется конкретная выдача чужого ответа по подстроке.","regressions":"Реальная native signature отличается от копии: обычная достижимость не доказана. «not ready» принимается в обоих плечах, polarity вне этого фактора. Train allocations41808→39416 (−5.72%) не квалифицированы по порядку.","admission":"REPAIR; CONDITIONAL_P1","reason":"Ошибочное чтение сохранённой пары даёт самостоятельное узкое основание. Прирост>5% локален; его нельзя распространять на все Contains и на неподтверждённого обычного потребителя.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5601/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5601/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md"]},
{"accountingId":"N26","id":"FIX-P60","kind":"экономия памяти с регрессией ответа","experiment":"P60: удалить повторные записи одной пары, созданные full/prefix/structure путями.","change":"Хранить уникальную пару один раз при сохранении семантики допуска и полного ответа.","accuracy":"Учебные controls4/4→4/4. Новый запрос с please: A отказывается, B отвечает без «please»; полноценного нового правильного ответа это не доказывает.","beforeAfter":"Память1704→1164bytes (−31.69%); pairs12→8 (−33.33%); дубликаты4→0; уникальных пар8→8.","regressions":"На полярном случае неполный ответ появляется вместо отказа: ложное принятие0/1→1/1. Train333000→16104bytes и236→5ms сравнивают разные полные/прогретые пути, не причинный выигрыш.","admission":"HOLD_UNCONDITIONAL","reason":"Экономия>5% есть, но исходный опыт выявляет ухудшение корректности. В очередь допустим только ограниченный ремонт дублирования с сохранением отказа/полного ответа; безусловное включение по правилу пользователя незаконно.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6001/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6001/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6001/results/attempt-0001/arms.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md"]},
{"accountingId":"N27","id":"FIX-P62","kind":"ремонт чужого ответа по слабому совпадению","experiment":"P62: full source signature допускает ответ по нерелевантному частичному совпадению.","change":"Применять исходный source guard с порогом.35 до выдачи сохранённой пары.","accuracy":"Компонент1/3→3/3 (+200%; +66.67п.п.); обычный consumer2/2→2/2.","beforeAfter":"Чужие ответы2→0; правильный отказ на неизвестное0→1. Две из трёх обычных веток обходят проверяемую память.","regressions":"Ресурсы плеч отдельно не измерены; разница времени смешана с JIT. Нельзя приписывать компонентный прирост всему ordinary flow.","admission":"REPAIR","reason":"Отказ вместо чужого восстановленного ответа — самостоятельное основание узкого ремонта. Измеренного роста обычной точности нет.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6201/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6201/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md"]},
{"accountingId":"N28","id":"FIX-P64","kind":"ремонт допуска кандидата в каскаде","experiment":"P64: найденный верхний кандидат непригоден, но поиск прекращается до законного нижнего кандидата.","change":"Проверять допустимость кандидата до окончательного выбора/ограничения списка и искать допустимый следующий в установленной границе.","accuracy":"Компонент1/2→2/2 (+100%; +50п.п.), ordinary2/2→2/2; сохранённые законные controls без потерь.","beforeAfter":"Lookup1→2, в худшем описанном случае1→3. Недопустимые target0 в обоих плечах на14 чтениях; после восстановления результат сохраняется.","regressions":"Число обращений растёт+100%/+200%; это структурная стоимость, не измеренный CPU и не бесплатное исключение. Время/память по плечам отсутствуют. Ограничение до проверки может всё ещё потерять шестого законного кандидата.","admission":"REPAIR","reason":"Неверное восстановление/отбрасывание законной памяти оправдывает исправление. Полного допуска оптимизации по5% нет; обязателен отдельный контроль cap/admission.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6401/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6401/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md"]},
{"accountingId":"N29","id":"FIX-P65","kind":"условный допуск проверенного нового урока","experiment":"P65: подтверждённая контрастная пара включается в память, раньше она не проходила допуск.","change":"Принимать только адресованную проверенную contrast-пару вместе с доказательством истинности и обычным consumer receipt.","accuracy":"Обычный consumer2/3→3/3 (+50%; +33.33п.п.); потерянных правильных строк в панели нет.","beforeAfter":"Пары3→6, exact entries1→2 (+100% количества). Требуется один новый доверенный manifest.","regressions":"Количество записей удвоилось; bytes/retained heap не измерены. Это цена, которую нельзя объявить≤5%. Сама форма manifest не проверяет истинность. Время смешано с прогревом.","admission":"CONDITIONAL_P1; NO_GLOBAL_ADMISSION","reason":"Полезный локальный выигрыш есть, но происхождение и стоимость нового урока — часть допуска. Это не бесплатное улучшение обучения и не автоматическое разрешение любой contrast-пары.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6501/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6501/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P6501/results/attempt-0001/arms.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/111-post-p55-transfer-plan-2026-11-01.md"]},
{"accountingId":"N30","id":"R34-FB","kind":"доставка исправления в обучение памяти","experiment":"P42: A не превращает отказ пользователя в урок, B принимает адресованное исправление и обучает память.","change":"Довести обычную обратную связь до TrainStructuredBatchAsync правильной единицы с исходным отвергнутым ответом и границей повторного применения.","accuracy":"На двух исправленных известных запросах0/2→2/2 (+100п.п.; процент от0 не определён); исходное обучение8/8. Это не held-out обобщение.","beforeAfter":"Принятые уроки0→2; exact entries8→10 и pairs24→30 (+25% количества), состояние меняется после действительного обучения.","regressions":"Содержание уроков и объём памяти различаются; отсутствие общей цены не позволяет объявить безусловный приоритет I. Текущая запись corrected response без Train не равна исправлению памяти.","admission":"REPAIR_DELIVERY; CONDITIONAL_P1","reason":"Восстановление ожидаемой цепочки пользовательского исправления законно как ремонт доставки. Эффект — запомнить два действительно данных исправления, а не улучшить способность на новых задачах.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4201/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4201/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4201/results/attempt-0001/arms.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/106-feedback-and-transition-memory-plan-2026-10-30.md"]},
{"accountingId":"N31","id":"R34-CA","kind":"ремонт адреса переходной памяти","experiment":"P47: адрес кеша перехода без текущего predicate сталкивает различные состояния.","change":"Включить текущий predicate в ключ перехода и сохранить это различие при чтении/восстановлении.","accuracy":"Контроль8/12→12/12 (+50%; +33.33п.п.); reference4/4→4/4; правильные ответы не потеряны.","beforeAfter":"Разрушительные коллизии4→0; все коллизии8→4; уникальные адреса4→8 (+100%).","regressions":"Удвоение числа адресов — цена состояния; bytes/устойчивое время отдельно не квалифицированы. Перенос доказан для узкого экспериментального E30PredicateCache, не для любого product dispatcher.","admission":"REPAIR; CONDITIONAL_P1","reason":"Ошибка адресации сохранённого ответа — самостоятельное основание. Локальная точность выросла>5%, но общий ресурсный/потребительский допуск ещё нельзя объявить закрытым.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4701/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4701/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/106-feedback-and-transition-memory-plan-2026-10-30.md"]},
{"accountingId":"N32","id":"EVID-P115-ACCEPTANCE","kind":"приёмка уже перенесённого snapshot","experiment":"P115: пересчёт статистики корпуса для каждого кандидата против неизменного snapshot с проверкой версии.","change":"Подтвердить ordinary/native consumer, cold build, инвалидирование и восстановление уже внесённого snapshot; не считать приёмку новым алгоритмом.","accuracy":"Ответы2/4→2/4; blind10/20 — повторения тех же четырёх запросов, не20 независимых задач; порядок кандидатов совпадает.","beforeAfter":"Warm stemming156→4 (−97.44%); allocated159344→28336bytes (−82.22%). Cold build6800bytes,4 stems,10.0631ms; cold+warm35136bytes (−77.95% против159344).","regressions":"Retained memory самого snapshot не измерена A/B. Wall-time на4 запросах не квалифицирован. Одинаковое число документов не гарантирует одинаковые данные; такой испорченный receipt отвергнут.","admission":"VERIFY; CONDITIONAL_P1_COST","reason":"Сильный локальный ресурсный эффект есть, но эта строка — недостающая приёмка существующего переноса. Для полного I остаются retained memory, холодный путь и обычное восстановление.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11501/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11501/results/attempt-0001/arms.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11501/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P11501/results/attempt-0001/raw/lineage-controls.json"]},
{"accountingId":"N33","id":"R31-QD","kind":"ремонт диагностики размерности","experiment":"P18: запрос размерности63 попадает в обученный64-мерный поиск и молча возвращает первый ответ.","change":"До Search проверять размерность и возвращать именованный QUERY_DIMENSION_MISMATCH.","accuracy":"Законные TRAIN8/8 и EXAM4/4 без изменения. На незаконном запросе правильный отказ0/1→1/1.","beforeAfter":"До: review-first с cosine1 при неверной форме; после: отказ до поиска. Размер состояния11008bytes не меняется.","regressions":"Процент от нулевого числа правильных отказов не определён. Это не увеличение точности модели на валидных запросах.","admission":"REPAIR","reason":"Ложный ответ вместо диагноза неправильного входа — самостоятельное основание очереди.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1801/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1801/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/97-learned-knn-state-integrity-transfer-plan-2026-10-29.md"]},
{"accountingId":"N34","id":"R31-OO","kind":"ремонт владения выученной памятью","experiment":"P19: внешнее изменение входного DTO после Train изменяет ответы A; B хранит owned snapshot.","change":"Копировать принимаемые данные и возвращать безопасный снимок результата в назначенном typed контракте.","accuracy":"До внешней мутации12/12 оба; после A9/12, B12/12 (+33.33%; +25п.п.). TRAIN7/8→8/8, EXAM2/4→4/4.","beforeAfter":"Память B сохраняет выученный ответ без дополнительного Train; allocated101128bytes у обоих на12 Execute.","regressions":"Наблюдаемое время1.9115→2.1502ms (+12.49%) — ухудшение wall-time в коротком измерении, не исключаемый CPU. JIT и initial train не разделены.","admission":"REPAIR","reason":"Защита ответа от чужой мутации — прямой самостоятельный ремонт. Нельзя на основании этой записи объявить безусловный ресурсный приоритет I.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1901/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1901/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/97-learned-knn-state-integrity-transfer-plan-2026-10-29.md"]},
{"accountingId":"N35","id":"R31-RF","kind":"проверка полного пересчёта обученной памяти","experiment":"P17: после изменения словаря переобучить/перекодировать только новые записи против всего корпуса.","change":"Сохранять согласованность encoder и всех старых векторов; подтвердить уже имеющийся full refit обычным regression case.","accuracy":"Старые TRAIN6/8→8/8 (+33.33%), EXAM4/4→4/4; все строки11/13→13/13 (+18.18%). Исходное удержание S0=8/8.","beforeAfter":"Неполный пересчёт теряет два старых ответа, полный их сохраняет. Это инвариант согласованной памяти, не новый способ обучения.","regressions":"Окно encode/rebuild allocations +39184bytes (+145.43%); время+.1849ms (+19.90%). Total window allocations−12.11% смешаны с порядком/JIT и не отменяют цену rebuild.","admission":"VERIFY_REPAIR","reason":"Ремонт потери старых ответов законен независимо от нового A/B проверочной задачи. Full Train уже пересчитывает корпус; повторная реализация не нужна. Не заявлять оптимизацию без цены.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1701/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1701/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/97-learned-knn-state-integrity-transfer-plan-2026-10-29.md"]},
{"accountingId":"N36","id":"R32-DR","kind":"условная интеграция добавления уроков","experiment":"P36: Train только новой строкой заменяет корпус; Train полным корпусом удерживает старые ответы.","change":"Если обычный caller действительно хочет append, он должен явно сформировать полный корпус или вызвать отдельный назначенный append API.","accuracy":"До2/3 оба; после A1/3, B3/3 (+200% относительно A; +66.67п.п.). B сохраняет два старых и добавляет новый ответ.","beforeAfter":"Количество хранимых строк1→3 (+200%); B получает дополнительные старые предъявления. Это различие данных, не новый learner.","regressions":"Train по контракту означает замену; ошибочный обычный caller, использующий замену вместо append, не установлен. Изменить сам Train на append было бы сменой контракта. Цена хранения увеличена.","admission":"HOLD_WITHOUT_CALLER; CONDITIONAL_REPAIR","reason":"Опыт доказывает ожидаемую семантику replacement. В независимую очередь ремонта законно входит только найденная ошибочная интеграция или явно требуемый append consumer; общей ошибки Train он не доказывает.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3601/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3601/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/99-new-learning-consumer-transfer-addenda-2026-10-29.md"]},
{"accountingId":"N37","id":"R32-DQ","kind":"ремонт ответа на пустой запрос","experiment":"P37: пустой вектор запроса выбирает первую метку, которая меняется с порядком записи.","change":"Отвергать запрос без содержательных признаков вместо выдачи произвольной обученной метки.","accuracy":"Прямой и обратный порядок:3/4→4/4 (+33.33%; +25п.п.) каждый;3 законных контроля сохранены.","beforeAfter":"На пустом запросе A выдаёт CSharp либо Tests, B выдаёт явное отсутствие ответа000000.","regressions":"Новых полезных признаков и нового обучения нет; собственная ресурсная цена не установлена.","admission":"REPAIR","reason":"Доказанное ложное чтение памяти без основания — самостоятельный диагностический ремонт; не новый рост способности на содержательных запросах.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3701/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3701/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/99-new-learning-consumer-transfer-addenda-2026-10-29.md"]},
{"accountingId":"N38","id":"R32-SR","kind":"условный перенос выбора навыка","experiment":"P32: чистый Jaccard даёт ничью; сигнал обученного generic KNN меняет выбор и фактически выполненные шаги.","change":"Подключить наблюдаемый типизированный learned score в назначенный SkillRetriever consumer, проверив происхождение сигнала.","accuracy":"Все6/12→12/12 (+100%; +50п.п.); EXAM2/4→4/4; TRAIN4/8→8/8. Изменились6 реально выбранных/загруженных/исполненных шагов.","beforeAfter":"Источник полезного сигнала — generic Knn route prediction. Оценки собственно SkillValueUnit=0 в обоих плечах, поэтому его обучаемость опытом не подтверждена.","regressions":"Ресурсная граница ordinary каталога не установлена. A на этой панели вырожден в постоянный review из-за ничьих; результат не доказан для полного каталога.","admission":"CONDITIONAL_P1","reason":"Есть полезный локальный эффект>5%, но его нельзя приписать другой единице или считать глобально допущенным без стоимости и обычного потребителя.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3201/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3201/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/99-new-learning-consumer-transfer-addenda-2026-10-29.md"]},
{"accountingId":"N39","id":"R32-EX","kind":"проверка границы известной формулы","experiment":"P29: обученная ExpressionCompletion memory хранит известные правые части и отказывается на неизвестных.","change":"Сохранить явный отказ для неизвестной формулы при обычном потреблении; не объявлять память решателем новых выражений.","accuracy":"Холодная память0/10; после обучения8/8 известных (4 точных+4 вариантов пробелов) и2 корректных отказа. Переставленные неправильные RHS не воспроизведены0/8.","beforeAfter":"Постоянный ответ даёт2/8. Это доказательство памяти известных пар; самостоятельного A/B нового wrapper отказа нет.","regressions":"Обобщение на новые формулы и математический solver не проверены. Цена отдельного переноса не измерена.","admission":"VERIFY_REPAIR","reason":"Честная граница ответа и отказа допустима как приёмка восстановления известных пар. Не считать эту строку ещё одним улучшением обучения.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P2901/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P2901/results/attempt-0001/arms.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P2901/results/attempt-0001/comparison.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/99-new-learning-consumer-transfer-addenda-2026-10-29.md"]},
{"accountingId":"N40","id":"P35","kind":"ремонт знаменателя голосования","experiment":"P35: порог большинства сравнивается с запрошенным k, хотя найдено меньше соседей.","change":"Использовать фактическое число найденных соседей в знаменателе; сохранить политику пустого набора.","accuracy":"В изменяемом правиле2/3→3/3 (+50%; +33.33п.п.); приобретён случай одного соседа, потерянных нет. n=0 вне изменения.","beforeAfter":"Одиночный реальный выученный голос перестаёт теряться; train264bytes одинаков.","regressions":"Read allocations6128→9584bytes (+56.40%); короткое время смешано с прогревом. Память нарушает безусловный допуск по5%.","admission":"REPAIR; NO_UNCONDITIONAL_P1","reason":"Неправильный знаменатель теряет уже выученный ответ и даёт самостоятельное основание ремонта. Однако как оптимизацию/улучшение с ценой≤5% эту запись представлять нельзя.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3501/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3501/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/100-neighbor-vote-divisor-transfer-2026-10-29.md"]},
{"accountingId":"N41","id":"TEST-P49-WARM","kind":"проверка условий warm start","experiment":"P49:60 обновлений новой задаче с холодного состояния против600 подготовительных+60 новых с warm state.","change":"Создать отсутствующую проверку заданной дозы/сохранённого состояния, отделяя новые задачи от удержания старых; существующий bootstrapper другой дозы её не заменяет.","accuracy":"Новые8 строк A6/8→B7/8 (+16.67%; +12.5п.п.), EXAM2/4→3/4. Полный итогA6/16, B11/16 включает разный исходный навык.","beforeAfter":"A получает60 обновлений новой задачи; для B отдельно один раз подготовлено состояние600 шагами, затем также60 новых. Эти600 — цена producer сохранённого состояния, а не дополнительные600 для каждого consumer. Allocations новых96520bytes одинаковы, однократной подготовки964840bytes.","regressions":"Часть старой панели у B:6/6→3/6; old-only4/4 сохранены. Восемь строк имеют одинаковые входы с разными gold между задачами, поэтому потеря на них не доказывает чистое forgetting согласованного набора. Различные исходные состояния и конфликтующие цели не позволяют объявить warm start как метод вредным или безусловно полезным.","admission":"VERIFY; NO_UNCONDITIONAL_METHOD_ADMISSION","reason":"Само проверочное задание законно: нужно воспроизвести назначенную дозу и честно различить обучение новой задаче, old-only удержание и конфликт целей. Безусловный метод не обоснован сопоставимым началом/набором; это не доказательство общего вреда warm start.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4901/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4901/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4901/results/attempt-0001/arms.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/107-warm-start-learning-coverage-2026-10-30.md"]},
{"accountingId":"N42","id":"FIX-P53-BATCH-JSON","kind":"ремонт атомарного сохранения урока","experiment":"P53: повреждённый JSON batch успевает частично записать урок; B проверяет/готовит целую партию до публикации.","change":"Ничего не менять при malformed batch, затем атомарно опубликовать законную партию.","accuracy":"Законная партия3/3 у обоих. При незаконной A1→2/3 из-за неположенного урока, B1→1/3; такое увеличение A не является полезной точностью.","beforeAfter":"Незаконно добавленные записи5→0, уроки1→0. Сохранение предыдущего состояния — измеряемый эффект.","regressions":"Valid allocations481264→535136bytes (+11.19%); invalid649856→650320 (+0.0714%). Valid time4→6ms единично, не причинный стабильный замер.","admission":"REPAIR; NO_UNCONDITIONAL_P1","reason":"Незаконное частичное сохранение ответа — самостоятельное основание несмотря на цену. Не называть это бесплатным ростом качества или улучшением обучения.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5301/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5301/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/109-p53-p55-transfer-plan-2026-10-30.md"]},
{"accountingId":"N43","id":"FIX-P55-SOURCE-POLARITY","kind":"ремонт полярности читаемого ответа","experiment":"P55: сохранённая пара противоположной source polarity проходит как подходящая.","change":"Проверять source polarity до выдачи кандидата, сохраняя законные пары.","accuracy":"Ordinary3/4→4/4 (+33.33%; +25п.п.); component1/4→4/4 (+300%); training controls3/3 сохранены.","beforeAfter":"Устранён неверный ответ с противоположной полярностью; обучение одинаковое, меняется чтение памяти.","regressions":"Ресурсы сняты для всего процесса, не по плечам. Native ordinary соответствуетA, B использует копию; собственная обычная приёмка переноса остаётся нужна.","admission":"REPAIR; CONDITIONAL_P1","reason":"Первичная запись показывает ошибку восстановления смысла. Законный узкий ремонт; общего ресурсного допуска и нового улучшения learner нет.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5501/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5501/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/109-p53-p55-transfer-plan-2026-10-30.md"]},
{"accountingId":"N44","id":"RF10","kind":"ремонт диагностики сходимости","experiment":"План112 разделяет две логические болезни мониторинга и требует второго DPO consumer; независимого нового учебного A/B нет.","change":"Раздельно диагностировать недостаточный горизонт наблюдений и невозможный strict threshold; подтвердить фактические callers/configuration.","accuracy":"Точность обучения до/после не измерялась для этой работы.","beforeAfter":"Монитору нужны3 наблюдения: budget<3×interval никогда не позволяет срабатывание; при budget=3×interval экономии ещё нет. При threshold0 условие difference<0 недостижимо.","regressions":"Эти логические условия не доказывают число сэкономленных updates, wall-time или сохранённое качество. Часть диагностики внесена, остаток второго потребителя не закрыт.","admission":"REPAIR_DIAGNOSTIC; VERIFY","reason":"Ошибочное обещание остановки/наблюдаемости — самостоятельный дефект диагностики. В очередь законно входит его точная проверка, не выдуманная экономия обучения.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/112-convergence-two-diseases-second-consumer-2026-11-01.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/118-post101-addressed-repair-plan-2026-11-02.md"]},
{"accountingId":"N45","id":"124-S02","kind":"ремонт приёмки и бюджета P107","experiment":"P107: процесс мог закончиться exit0 независимо от отрицательного контроля; исходная попытка выполнила51 чтение при лимите50.","change":"Отделять правильное отвержение от ошибки измерителя; финализировать бюджет и результат фактической попытки независимо от успеха отчёта.","accuracy":"Исходные TRAIN8/8 и EXAM4/4 одинаковы у A/B; majority/first baseline также совпадают. Ремонт измерителя не добавляет правильных ответов.","beforeAfter":"Сохранённый ремонт различает4 честные/испорченные пары. Исторический51/50 остаётся NOT_ACCEPTED/code7; нельзя превратить51 в50 пересчётом.","regressions":"Save тогда читался из того же живого Unit, свежий Load не доказан. Ресурсный A/B не относится к этой работе.","admission":"REPAIR","reason":"Ложный успешный результат и заниженный бюджет — прямые ошибки диагностики. Достаточное самостоятельное основание без процентов обучения.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P10701/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-124-post107-addressed-repair-plan-2026-11-02.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md","C:/Projects/llm_mvp_core/experiments-v2/_audits/2026-11-02-post107-audit/s02-outcome-separation/outcome-separation-replay.json"]},
{"accountingId":"N46","id":"102-R1","kind":"ремонт дублирующего адреса памяти","experiment":"Аудит102 поздних переносов: нормализованные одинаковые ID могли расходиться между map и индексом.","change":"До построения новой памяти отвергать повторный нормализованный ID и сохранить прежнее принятое состояние.","accuracy":"Отдельного A/B качества/точности нет; это контракт идентичности записи.","beforeAfter":"До: одинаковый адрес может обозначать разные записи в двух представлениях. После по учёту: duplicate ID отвергается до build. Количество исторических инцидентов не измерено.","regressions":"Нельзя считать отказ на malformed dataset новым правильным ответом модели. Ресурсная цена не измерена.","admission":"REPAIR","reason":"Повреждение адресов сохранённой памяти — самостоятельное основание. Уже внесённый код требует своей приёмки, а не повторной реализации.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/108-router-memory-transfer-audit-2026-10-30.md"]},
{"accountingId":"N47","id":"102-R3","kind":"ремонт владения массивами dataset","experiment":"R102-R3: входные features и bool-labels должны принадлежать обученной памяти, а не внешнему вызывающему коду.","change":"Копировать массивы при Train, чтобы последующая внешняя мутация не меняла выученные ответы.","accuracy":"Контроль без копий45/47 tests, финальная запись47/47. Это тесты контракта, не точность47 учебных задач и не самостоятельный опыт обучения.","beforeAfter":"Два конкретных случая мутации перестают ломать память. Финальный журнал подтверждает47 passed; публикация исходного ремонта относится к истории.","regressions":"Время финального набора17.0755s не является сравнением A/B; процент памяти/производительности отсутствует.","admission":"REPAIR; HISTORICAL_IMPLEMENTATION","reason":"Сохранность выученного ответа при внешней мутации — самостоятельное основание. В очереди остаётся только назначенный остаток/приёмка, не повторное внесение уже существующих копий.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/108-router-memory-transfer-audit-2026-10-30.md","C:/Projects/llm_mvp_core/experiments-v2/_runners/R102-R3-DATASET-ROUTER-MEMORY-attempt-0001/Run-DatasetRouterMemoryOwnershipTests-CONTROL-input-array-copy-removed.log","C:/Projects/llm_mvp_core/experiments-v2/_runners/R102-R3-DATASET-ROUTER-MEMORY-attempt-0001/Run-DatasetRouterMemoryOwnershipTests-FINAL-after-restore.log"]},
{"accountingId":"N48","id":"102-R4","kind":"ремонт атомарной замены dataset","experiment":"R102-R4: Clear-before-read стирал принятую память, если новая партия не могла быть прочитана.","change":"Сначала построить полный новый dataset, публиковать его только при успехе; законная пустая замена остаётся разрешена.","accuracy":"Контроль52/56 tests, финальный56/56; это четыре нарушения контракта, не рост точности модели на4/56.","beforeAfter":"При malformed replacement старые выученные ответы сохраняются. Финальный журнал подтверждает56 passed.","regressions":"Время финального набора12.0273s не является A/B. Стоимость временного нового состояния отдельно не измерена.","admission":"REPAIR; HISTORICAL_IMPLEMENTATION","reason":"Потеря принятой памяти при неудачном сохранении — прямое самостоятельное основание. Уже внесённую атомарность нельзя второй раз засчитать новым улучшением.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/108-router-memory-transfer-audit-2026-10-30.md","C:/Projects/llm_mvp_core/experiments-v2/_runners/R102-R4-DATASET-ROUTER-ATOMICITY-attempt-0001/Run-DatasetRouterTrainingAtomicityTests-CONTROL-clear-before-read-restored.log","C:/Projects/llm_mvp_core/experiments-v2/_runners/R102-R4-DATASET-ROUTER-ATOMICITY-attempt-0001/Run-DatasetRouterTrainingAtomicityTests-FINAL-after-restore.log"]},
{"accountingId":"N49","id":"29-W1","kind":"ремонт первого шага warmup","experiment":"P12: на первом шаге warmup получался LR0, который обновитель отвергал до действительного update.","change":"Исправить только zero-step/индексацию первого SFT шага, сохранив назначенный остальной schedule.","accuracy":"Ответы2/4 до/после и у обоих плеч; прироста точности нет.","beforeAfter":"Назначенные эффективные updates0→4; исправлена возможность выполнить заданное обучение. Относительный процент от0 не определяется.","regressions":"E1 probability gold ухудшилась примерно на0.27636%, margin примерно на3.258%; это меньше5%, но полезного выигрыша точности всё равно нет. Глобально менять schedule опыт не разрешает.","admission":"REPAIR","reason":"Потеря назначенного первого update — доказанная ошибка исполнения; чинить её законно отдельно от вопроса, улучшили ли эти четыре шага ответы.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1201/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1201/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/88-gradient-warmup-transfer-repair-plan-2026-10-28.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/95-experiment-transfers-repair-plan-2026-10-29.md"]},
{"accountingId":"N50","id":"95-S2","kind":"ремонт численного хвоста sampler","experiment":"Аудит переноса P08: численно неполная positive mass и fallback последнего индекса могут выбрать замаскированный нулевой token при draw близком1.","change":"Суммировать finite положительную массу, масштабировать draw на неё, использовать строгую границу CDF и fallback последнего положительного элемента.","accuracy":"Нового A/B точности этой численной поправки нет; исходный P08 имел2/4→2/4 фактических ответов.","beforeAfter":"Требуемая граница: нулевой/маскированный token никогда не возвращается из хвоста. План≤64 чистых Sample calls — будущая проверка, не выполненный результат этой сессии.","regressions":"Общий перенос композиции P08 имеет отдельный memory HOLD (+155.65% allocations); исправление хвоста не снимает его. Проценты выигрыша tail-ремонта не измерены.","admission":"REPAIR","reason":"Доказанная ошибка восстановления разрешённого token из распределения даёт самостоятельное основание. Нельзя приписывать ей выигрыш всей композиции.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/94-experiment-transfers-audit-2026-10-29.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/95-experiment-transfers-repair-plan-2026-10-29.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/report.md"]},
{"accountingId":"N51","id":"95-S3","kind":"ремонт метаданных embedding памяти","experiment":"Аудит переноса памяти: PredicateIds/DomainTags остаются внешне изменяемыми массивами на входе и выходе.","change":"Делать owned copies именно этих typed metadata при Train и выдаче; immutable string Output не требует выдуманного generic deep clone.","accuracy":"Самостоятельного A/B точности нет. Будущая проверка1 строки/6 чтений — контрактная приёмка, не уже измеренный рост.","beforeAfter":"До: внешняя мутация может изменить адрес/семантические теги принятой памяти. После по учёту копии есть; нужно подтвердить ingress и egress обычного потребителя.","regressions":"Память/время копирования не измерены. Нельзя переносить проценты P19 на этот другой typed consumer.","admission":"REPAIR","reason":"Защита сохранённых признаков от чужой мутации — самостоятельное основание. Это отдельный контракт, а не повторный эксперимент обучения.","sources":["C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/94-experiment-transfers-audit-2026-10-29.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/95-experiment-transfers-repair-plan-2026-10-29.md"]},
{"accountingId":"N52","id":"P08-CONSUMER","kind":"проверка обычного генератора","experiment":"P08: TopK+TopP композиция на logits восстановленного обученного состояния; задача — доказать обычного потребителя этого состояния.","change":"Проверить настоящий generator на S1, реальные hashes/optimizer step и actual emitted tokens; не подменять наблюдаемые вызовы объявленными нулями.","accuracy":"Фактические ответы2/4→2/4. На T1 P(correct)=.86879754→1 (+15.10%); неверная альтернатива13.12%→0, но sampled ответы одинаковы.","beforeAfter":"Оба плеча используют общие4 SFT updates400→404; их нельзя засчитать эффектом sampler. Отдельная проверка consumer не является новым A/B улучшения.","regressions":"Ресурсное измерение: allocations10120→25872bytes на вызов дляT2/E1/E2 (+155.6522%);800 warmup+8000 measured calls. Общая композиция остаётся HOLD, независимо от CPU.","admission":"VERIFY; HOLD_GLOBAL","reason":"Проверка реального обученного consumer законна как закрытие доказательства. Алгоритм целиком не проходит5%-критерий из-за памяти; наличие уже внесённого кода не равно допуску.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/sampler-resource-0001/receipt.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/sampler-resource-0001/warmup-trend-correction.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/consumer-completion-0001/evidence-boundary-correction.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/91-positive-transfer-followup-audit-2026-10-28.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/92-positive-transfer-followup-plan-2026-10-28.md"]},
{"accountingId":"N53","id":"P09-CONSUMER","kind":"проверка кеша восстановленного состояния","experiment":"P09: full forward против cache/chunks на одном восстановленном обученном состоянии.","change":"Подтвердить actual restored weights, cache invalidation/chunks и обычный readout без дополнительного скрытого обучения.","accuracy":"Все21504 числовых выхода побайтно совпали, top84/84 и ответы4/4 совпали; истинная точность2/4→2/4.","beforeAfter":"Allocations7010408→7102120bytes (+1.308%); median wall-time7.015→7.289ms (+3.906%). Общие4 SFT updates не являются выигрышем кеша.","regressions":"Total time100.110→35.314ms искажён одиночным выбросом/порядком, не доказанная экономия. Небольшие регрессии<5% не создают положительного полезного выигрыша, которого здесь не показано.","admission":"VERIFY; NO_NEW_P1_OR_P2","reason":"Законна приёмка эквивалентности восстановления и кеша. В этой строке нет самостоятельного измеренного улучшения, поэтому её нельзя назвать ещё одним положительным опытом.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0901/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0901/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/92-positive-transfer-followup-plan-2026-10-28.md","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/95-experiment-transfers-repair-plan-2026-10-29.md"]},
{"accountingId":"N54","id":"102-R7","kind":"ремонт распознавания вырожденного измерителя","experiment":"P30 и P40: измеритель объявлял фактор полезным/подтверждённым там, где панель не различает плечи.","change":"Вычислять actual observations и baseline/headroom для конкретной панели; классифицировать неспособность проверить отдельно от отрицательного результата.","accuracy":"P30 ответы4/4 в обоих плечах, при любом alpha тот же winner. P40 accuracy50% у обоих, spread0; различие headroom50п.п. относится к измеримости панели.","beforeAfter":"До: совпадение ответов/чисел могло получить неверный статус гипотезы. После по учёту отдельные evaluators используют наблюдения. Отдельного A/B ремонта нет.","regressions":"Ни положительный margin, ни сохранённые50% сами не подтверждают пользу обучаемого коэффициента. Ресурсной экономии не установлено.","admission":"REPAIR","reason":"Дефект классификации экспериментального результата — самостоятельное основание диагностики; это не ещё один выигрыш обучения.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3001/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3001/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4001/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P4001/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md"]},
{"accountingId":"N55","id":"102-R8","kind":"ремонт привязки поправки к исходному результату","experiment":"P31: устаревший budget/счётчик против фактического ledger; требуется effective correction, связанная с hash исходного файла.","change":"Присоединять поправку только к точно совпавшему source path/hash/полю, сохраняя исходный raw; не доверять голому названию эксперимента.","accuracy":"Ремонт не меняет ответы и не имеет собственного A/B точности. Исходный выигрыш P31 был на дословно известных строках, не на3 новых.","beforeAfter":"Фактический учёт:6 уроков,21 запрос в бюджете+4 дополнительных=25. Историческое27/budget=false сохраняется рядом с объяснением; не переписывается как будто всегда было25.","regressions":"Применение чужой/устаревшей поправки должно быть отвергнуто. Никакой экономии27→25 не было — исправляется описание уже выполненной работы.","admission":"REPAIR","reason":"Ошибочный счёт и непривязанный correction — прямое основание ремонта диагностики; количество исправленных полей не является улучшением обучения.","sources":["C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3101/results/attempt-0001/report.md","C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P3101/results/attempt-0001/metrics.json","C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/102-late-transfer-repair-plan-2026-10-29.md"]}
]
```
