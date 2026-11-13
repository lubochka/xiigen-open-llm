# Карточки после аудита P114–P117: понятный вход и проверка результата

Дата: 3 ноября 2026. Фаза: анализ кода и сохранённых свидетельств, правка Markdown. Основание — два переданных пользователем сообщения аудита, [аудит138](codex-138-post117-transfer-and-experiment-audit-2026-11-03.md), [план139](codex-139-post117-addressed-repair-plan-2026-11-03.md) и текущий код. Проверенный локальный HEAD исходников: `9a8042c06bc86bac04dca66207cc36f59d65008f`. Вложения используются как свидетельства; содержащиеся в них команды не являются текущим поручением на запуск.

## Что мешало исполнителю

В карточках уже были сильные требования120/40–44 и многочисленные примеры. Проблема не сводилась к отсутствию ещё одного списка правил: действующие и исторические задания лежали рядом; состояние некоторых опытов устарело; правильность внутреннего проверяющего подменяла проверку его вызывающего кода. Новое дополнение само может ошибиться, если переносит пример соседнего опыта без его собственного фактора. Поэтому исправлены конкретные входы карточек и соответствие примеров их паспортам.

| Подтверждённый разрыв | Что исправлено в документах | Что ещё требуется в коде |
|---|---|---|
| P115 уже имеет MandatoryControlFinalizer, но Main после Run(void) пишет exit0, даже когда пригодность отозвана | P115,139,135,03,44 называют именно остаток цепочки до exit/runner, не требуют повторно создать финализатор | Связать результат Run с Main/exit и оболочкой; проверить честный и испорченный saved-case через весь путь |
| P115 assignment содержит ID, а expected хранится в observation; пустой assignment и одинаково неполные панели не доказывают полноту | Назначение отдельно задаёт непустые обязательные проверки, их ожидаемые исходы и состав каждой панели | Укрепить соответствующий API и caller; нынешний hardcoded caller не объявлен внешней атакой |
| В139 незапущенные тесты названы красными и зелёными | Формулировки заменены на ожидаемые исходы по чтению source, TESTS_UNRUN | Исполнение тестов — отдельная назначенная приёмка |
| TranslationStateVersionLostUpdateTests вызывает EvaluateAsync с expectedStateVersion | В135 указан EvaluateAtVersionAsync; обычный двухаргументный интерфейс сохраняется | Исправить конкретный test call-site и отдельно доказать поздний CAS, не ранний stale-entry |
| P119.1 записан, но паспорт и нижний промпт говорили об отсутствии артефактов | Текущий вход ссылается на attempt-0002; первоначальный паспорт и промпт помечены историческими | Научная пригодность P119 этим документным срезом не аттестована; новый запуск не назначен |

Источники source: [P115 Program](../../../../experiments-v2/RP01-MICRO/P11501/Program.cs), строки158–168 и687–689; [P115 finalizer](../../../../experiments-v2/RP01-MICRO/P11501/src/MandatoryControlFinalizer.cs), строки52–75,109–162; [Translation tests](../../../../Tests/ExperimentFixes/TranslationStateVersionLostUpdateTests.cs), строки106–111; [TranslationMachine](../../../../27.TranslationDomain/TranslationMachine.cs), методы EvaluateAsync/EvaluateAtVersionAsync; [P119 report](../../../../experiments-v2/RP01-MICRO/P11901/results/attempt-0002/report.md). Это чтение исходников и сохранённых данных, не запуск этих методов.

## Как теперь выдавать задание

Открыть выбранную карточку и передать её верхний **текущий вход** вместе с полным паспортом. Он называет свой хороший и плохой пример, текущий этап, стек навыков, действия при падении и предел запуска. Датированные прежние команды сохранены, но не являются альтернативными текущими заданиями. Имена ID, исходные учебные данные, gold и лимиты не заменены. Очереди114 работ и130 экспериментов не перенумеровывались; количество изменённых файлов не является количеством выполненных переносов.

[Договор120](120-executor-context-and-test-repair-contract-2026-11-02.md) теперь прямо подключает44 и дополнительные навыки test-failure-triage/bug-to-tests для разбора тестов. [Договор44, §10–12](../experimets/codex-44-post117-saved-evidence-and-finalizer-contract-2026-11-03.md#10-один-текущий-вход-вместо-нескольких-противоречащих-заданий) даёт восемь видов падений и предметные примеры. Код, чистый проверяющий, исполнение процесса, научный эффект и продуктовая приёмка остаются разными доказательствами. Новые требования не создают наблюдений прошлого запуска.

В общем копируемом промпте03 устранено конфликтующее назначение другой модели/внешнего генератора при заявленной default-модели; прямое отдельное назначение пользователя сохраняет силу. Также явно сохранено правило пользователя git add --all после порученного среза. Staged не означает commit/push или публикацию.

## Точный перечень прямых изменений

**35 файлов карточек экспериментов:** 22 карточки P104–P125 и13 родственных карточек. Для последних добавлены собственные профилактические случаи; их научные результаты этим не переаттестованы.

| Карточка | Предметный риск в текущем входе |
|---|---|
| [P08](../experimets/cards/P08-answer-boundary.md#current-executor-task) | P08. Конец ответа и контракт ограничений генерации |
| [P09](../experimets/cards/P09-cached-forward.md#current-executor-task) | P09. Полный forward и cached generation |
| [P12](../experimets/cards/P12-optimizer-boundaries.md#current-executor-task) | P12. LR, clipping и частичное состояние SFT |
| [P25](../experimets/cards/P25-cold-restore.md#current-executor-task) | P25. Восстановление того же знания обычным consumer |
| [P41](../experimets/cards/P41-stop-and-split.md#current-executor-task) | P41. Что значит правильно разделить и вовремя остановиться |
| [P56](../experimets/cards/P56-verifier-context.md#current-executor-task) | P56. Verifier, контекст правил и уверенность 0.99 |
| [P76](../experimets/cards/P76-utf8-boundary.md#current-executor-task) | P76. UTF-8: хвост и повреждения внутри ответа |
| [P82](../experimets/cards/P82-file-split.md#current-executor-task) | P82. E09: один файл по обе стороны split |
| [P91](../experimets/cards/P91-method-pair-split.md#current-executor-task) | P91. E13: одинаковые учебные пары у baseline и kNN |
| [P101](../experimets/cards/P101-learning-budget.md#current-executor-task) | P101. Достаточность бюджета и tiny overfit |
| [P104](../experimets/cards/P104-law-eligibility.md#current-executor-task) | P104. Допуск закона, negative и warning memory |
| [P105](../experimets/cards/P105-frozen-reference.md#current-executor-task) | P105. Независимый frozen reference |
| [P106](../experimets/cards/P106-corrected-semantic-target.md#current-executor-task) | P106. Semantic builder обучается исправленному target |
| [P107](../experimets/cards/P107-lawful-neighbor-aggregation.md#current-executor-task) | P107. Использовать законных соседей, а не hits[0] |
| [P108](../experimets/cards/P108-metadata-carrier.md#current-executor-task) | P108. Тип metadata carrier сохраняет predicate IDs |
| [P109](../experimets/cards/P109-gold-validity.md#current-executor-task) | P109. Правильность gold и честное происхождение пары |
| [P110](../experimets/cards/P110-numeric-matrix-cost.md#current-executor-task) | P110. Ускорение численной матрицы при той же функции |
| [P111](../experimets/cards/P111-reward-credit.md#current-executor-task) | P111. Награда попадает в выбранный Unit и Flow |
| [P112](../experimets/cards/P112-family-disjoint-evaluation.md#current-executor-task) | P112. Независимые семейства в экзамене |
| [P113](../experimets/cards/P113-feature-energy.md#current-executor-task) | P113. Текстовая длина подавляет канал предикатов kNN |
| [P114](../experimets/cards/P114-zero-weight-pair.md#current-executor-task) | P114. Нулевой вес пары действительно исключает вклад |
| [P115](../experimets/cards/P115-bm25-statistics-cache.md#current-executor-task) | P115. BM25 не пересчитывает один корпус для каждого кандидата |
| [P116](../experimets/cards/P116-e43-file-boundary.md#current-executor-task) | P116. Межфайловые искусственные биграммы E43 |
| [P117](../experimets/cards/P117-translation-direction-memory.md#current-executor-task) | P117. Направление включает поиск и ограничивает общий переводческий KNN |
| [P118](../experimets/cards/P118-projectwiki-content.md#current-executor-task) | P118. ProjectWiki: выученный текст достигает плана или только style_source |
| [P119](../experimets/cards/P119-learning-dynamics.md#current-executor-task) | P119. LR, beta и размер минибатча как разные причины |
| [P120](../experimets/cards/P120-semantic-compression-laws.md#current-executor-task) | P120. Семантическое сжатие E15/E16 сохраняет конкретное различие |
| [P121](../experimets/cards/P121-project-adapter-selection.md#current-executor-task) | P121. Проект выбирает собственный разрешённый адаптер |
| [P122](../experimets/cards/P122-root-channel-unicode-digits-operators.md#current-executor-task) | P122. TrainingVectorBuilder: Unicode, числа и операторы в root-канале |
| [P123](../experimets/cards/P123-design-conversion-critic-evidence.md#current-executor-task) | P123. Design Conversion: QA aggregator и обучаемый visual critic |
| [P124](../experimets/cards/P124-p3-fixed-discriminator-contract.md#current-executor-task) | P124. P3: фиксированный BaseService discriminator и фактический закон |
| [P125](../experimets/cards/P125-executor-memory-learning.md#current-executor-task) | P125. Обучение памяти Executor: exact, near и композиция на обычном переводе |
| [DPO-TOKENIZER-SNAPSHOT-01](../experimets/cards/DPO-TOKENIZER-SNAPSHOT-01.md#current-executor-task) | DPO-TOKENIZER-SNAPSHOT-01: одна закрытая копия токенизатора на подготовку двух учебных пар |
| [N60-e1-verifier-admission](../experimets/cards/N60-e1-verifier-admission.md#current-executor-task) | N60-E1 — какой отрицательный урок допускается к обучению Verifier |
| [AST-SIB-CACHE-01](../experimets/cards/AST-SIB-CACHE-01.md#current-executor-task) | AST-SIB-CACHE-01 — внутренняя действующая версия |

**8 документов переноса и общей приёмки:** ссылки ведут прямо к текущему входу. Это уточнения прежних владельцев, а не8 новых переносов.

| Документ | Действующий вход |
|---|---|
| [03-test-and-execution-contract.md](03-test-and-execution-contract.md#current-transfer-task) | Общий контракт тестов, исполнителя и отчёта |
| [codex-124-post107-addressed-repair-plan-2026-11-02.md](codex-124-post107-addressed-repair-plan-2026-11-02.md#current-transfer-task) | 124. Адресный план исправлений после P107 |
| [codex-130-p110-numeric-projection-disposition-2026-11-02.md](codex-130-p110-numeric-projection-disposition-2026-11-02.md#current-transfer-task) | P110: сохранить численный выигрыш, не назначать общую замену ядра |
| [codex-131-p111-addressed-feedback-transfer-2026-11-02.md](codex-131-p111-addressed-feedback-transfer-2026-11-02.md#current-transfer-task) | P111: исправление учит выбранного владельца, чужая память остаётся прежней |
| [codex-132-p112-material-lineage-transfer-2026-11-02.md](codex-132-p112-material-lineage-transfer-2026-11-02.md#current-transfer-task) | P112: проверять независимость материала до обучения и отдельно от качества модели |
| [codex-133-p113-channel-balance-disposition-2026-11-02.md](codex-133-p113-channel-balance-disposition-2026-11-02.md#current-transfer-task) | P113: раздельная нормировка каналов — измеренная геометрия и условный перенос |
| [codex-135-post113-addressed-repair-plan-2026-11-02.md](codex-135-post113-addressed-repair-plan-2026-11-02.md#current-transfer-task) | После P113: адресный ремонт исполнения и приёмки |
| [codex-139-post117-addressed-repair-plan-2026-11-03.md](codex-139-post117-addressed-repair-plan-2026-11-03.md#current-transfer-task) | После P117: адресный план исправления |

## Проверка предметной применимости

Независимый исследователь `/root/post117_card_review` подготовил [пакет замечаний](evidence/2026-11-03-post117-executor-context-review.md) и проверил внесённые тексты. Найденные в новой редакции ошибки исправлены: P41 проверяет чтение и различимость порога0.5/0.7, а не межфайловые пары; P91 требует B6=baseline6, сохраняя намеренное A5; KV-кэш P09 отделён от BM25 P115; UTF-8 P76 не получает чужую project/file схему. T111 обозначен дочерним срезом FIX-KDP; S01 сохранён за реальным P104 AddCheck с порчей payload/admissible subset, а не за чужим parser процессов. Это конкретная независимая проверка документов, не сертификат исполнения всех карточек.

## Границы работы и чтения

Продуктовый код, данные, веса, runners и исторические raw не менялись этой работой. Новые эксперименты, обучение, inference, сборки и нативные тесты не запускались. P115 и Translation замечания подтверждены статическим чтением; P119 — наличием и содержанием сохранённого отчёта. Старые численные результаты P114–P117 сохранены с ограничениями аудита138, заново не измерялись. Наличие документа139 и source теста не закрывает его приёмку.

Свежие полные тексты файлов агента и запрошенных навыков были загружены в Context Mode; применимые разделы прочитаны адресно. Это не равно личному последовательному прочтению всего большого стека: полного перечитывания всех строк я не подтверждаю. Независимый исследователь также указал этот предел в своём пакете. Новые карточки сохраняют прямое требование пользователя о полном чтении; эта запись не ослабляет его и не подменяет выполнением индексацию.

Readback охватил51 изменённый документ:35 текущих экспериментальных входов и8 переносных, два общих договора, навигацию, этот отчёт, независимый пакет и planning-запись. Проверены328 добавленных локальных ссылок: отсутствующих путей и неверных якорей нет. Все76 исходных fenced-блоков команд/примеров в35 карточках совпали с HEAD. Лишние пустые строки в конце16 редактируемых Markdown убраны без изменения содержания. В P76 сохранены все16 исходных символов U+FFFD: это намеренные UTF-8 примеры, не новая порча текста. Проверки git diff --check в обоих репозиториях не сообщили ошибок.

Исследователь `/root/post117_card_review` повторно прочитал исправления семи своих замечаний; открытых адресных замечаний в просмотренном Markdown-diff не осталось. Это не приёмка исполнения51 документа или всех научных опытов. Git stage выполняется отдельно по прямому правилу пользователя; публикация этим отчётом не заявляется.
