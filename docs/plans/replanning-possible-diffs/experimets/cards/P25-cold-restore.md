# P25. Восстановление того же знания обычным consumer

<!-- POSTQ003-2026-11-03:START -->
## Адресное уточнение после Q003 — COLD-RESTORE-AFTER-MATERIALIZATION

**Установлено / область:** Совпадение pointer и наличие полного bundle — предпосылки; cold restore требует отдельного наблюдения. Девятнадцать конфликтных путей показывают, почему одного pointer недостаточно.

**Действие существующего владельца:** Перед назначенным Load в новом процессе проверить фактический payload, затем тот же полный typed bundle и действительного ordinary consumer. Утраченный original не заменять Train. К дискретной памяти не приписывать нейронные moments.

**Различающая приёмка:** Положительный контроль: новый owner возвращает прежние полные ответы. Отрицательный: конфликтное содержимое или отсутствующий компонент отвергается без публикации и сохраняет старый live owner. Чтение прежнего живого объекта не заменяет новый owner.

Обязательны [договор50](../codex-50-postq003-binary-and-historical-evidence-contract-2026-11-03.md) и [адресный план155](../../fixes-based-on-experiments/codex-155-postq003-addressed-repair-plan-2026-11-03.md). Это уточняет приёмку и сохранность; собственные factor, данные, gold, исторические budgets и очередь не меняются. Новый PRIMARY/Train для ремонта документа или bytes не назначен. Native Load/технические проверки учитываются отдельно только в собственном назначенном ремонте; их отсутствие обозначается UNRUN.

### Обязательная часть передаваемого поручения

> COLD-RESTORE-AFTER-MATERIALIZATION: Перед назначенным Load в новом процессе проверить фактический payload, затем тот же полный typed bundle и действительного ordinary consumer. Утраченный original не заменять Train. К дискретной памяти не приписывать нейронные moments. Положительный контроль: новый owner возвращает прежние полные ответы. Отрицательный: конфликтное содержимое или отсутствующий компонент отвергается без публикации и сохраняет старый live owner. Чтение прежнего живого объекта не заменяет новый owner. Передай exact attempt/path/donor authority, фактические commands/exit и предел проверки. Сохрани исторические результаты; повтор обучения ради получения отсутствующего файла не выполняй.
<!-- POSTQ003-2026-11-03:END -->


<!-- CODEX-POST131-CURRENT:START -->
## Действующее уточнение после P131 — 3 ноября 2026

**Проверка `POST131-P25-NATIVE-READER-BINDING`. WRONG:** writer и reader сообщили одинаковые ответы и длины, значит восстановление полезного поведения доказано. **CORRECT:** ответ reader берётся из реально исполненного `TranslationMachine` в отдельном процессе и связывается с его native записью; summary writer/reader не являются двумя независимыми источниками. **Путь приёмки:** checkpoint→действительный loader→native reader output→независимый exact scorer; полное применимое состояние проверяется отдельно. В копии отчёта согласованно изменить оба `actual` и их длины, оставив native записи и checkpoint прежними: проверяющий обязан обнаружить разрыв. Показать честный проход до и после этой порчи без нового Train; reader сохраняет Train0. Для ненейронного состояния неприменимые optimizer moments помечаются N/A. Совпадение на прежних строках подтверждает retention/restore в этом scope и не переименовывается в обобщение.

**Область:** связь raw с native проверяется только по реально существующим либо явно назначенным записям. Если такого источника нет, записать точный предел подтверждения. Этот блок не назначает новый Train/Flow/Load, persistence API, повтор PRIMARY или новую научную очередь. Integrity refusal и честный отрицательный scientific result различаются.

Основание: [аудит 149](../../fixes-based-on-experiments/codex-149-post131-transfer-and-experiment-audit-2026-11-03.md) · [план 150](../../fixes-based-on-experiments/codex-150-post131-addressed-repair-plan-2026-11-03.md) · [покрытие 151](../../fixes-based-on-experiments/codex-151-post131-card-coverage-2026-11-03.md) · [договор 49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md). Ниже сохранена прежняя постановка; текущий блок уточняет только указанную границу.
**Профилактика совместимости saved reanalysis после P131:** если старое свидетельство закрепляет producer source/DLL/schema, обновлённый checker имеет отдельную verifier identity и версию обязательных проверок. Проверять неизменные original producer bytes по их прежним hashes; не сравнивать старый host hash с новой verifier DLL и не исправлять старые proof/config ради PASS. Явно указать поддержанную historical schema и mapping архивных путей; отсутствие нужных originals оставить NOT_PROVEN полной provenance. Если такой механизм в этой карточке не применяется, пометить N/A с причиной; не создавать новый loader или обучение. Связь результатов и неизменность input проверяются независимо.

<!-- CODEX-POST131-CURRENT:END -->


**Передаваемое дополнение RESTORE-OUTPUT — 3 ноября 2026.** До повторного анализа назови read-only input_root и отдельный новый verification_output_root; сравни список и хэши исторических файлов до/после. Проверка old state, восстановленного state и обычных ответов остаётся раздельной. Подмена KNN target при том же count должна достичь именно state/answer comparer через назначенный Load, а не остановиться на parse/hash. Не повторять обучение ради отчёта; открытый comparer не отменяет сохранённые 19/24 и не доказывает обобщение. Применять [48: навыки, ожидаемая стадия, исправление теста и итог](../codex-48-post128-executor-verification-2026-11-03.md) вместе с собственным паспортом. Это уточнение проверки в назначенном объёме, не новый запуск. STOP/NEXT пользователя немедленны.

## Уточнение учебного утверждения и проверки — 3 ноября 2026

P25-LEARNED-RESTORE-02 уже имеет сохранённые исполнения P2502 и P12501/Q125. Нижнее DESIGNED_NOT_RUN от27 октября и прежнее «будущая программа» являются историей, не назначением повтора. Сверить actual Load/reader Train=0 и границу старой Debug DLL; restore не требует предварительного обучения отрицанию. Новая учебная гипотеза имеет отдельный ID P131.

**Передаваемое исполнителю правило:** применить [договор45](../45-learning-claim-and-native-acceptance-contract-2026-11-03.md) вместе с основным стеком навыков и experiment-design/test-integrity/test-failure-triage/bug-to-tests. В разрешённой реализации собрать затронутый продукт и host/test project, выполнить назначенные проверки именно собранного состояния, дождаться исходного handle. При failure эскалировать архитектору команду/версию/DLL/log/input/expected/actual; не заменять свежую сборку старым бинарником, не менять gold или продуктовую семантику ради неверного теста. UNRUN не закрывает реализацию. Документный аудит не запускает эти команды. STOP/NEXT пользователя немедленны; записанное исполнение не повторять.

<a id="current-executor-task"></a>
## Текущее задание после сверки P126–P128 — 3 ноября 2026

<!-- CODEX-POST128-CURRENT:START -->
**Владелец: `P25-LEARNED-RESTORE-02 / Q125`. Состояние:** Исполнен: восемь уроков, 0→19/24, fresh-load19/24, буквальная parity24/24. Новый P128 показал тот же класс недостаточной полноты comparer; это профилактическое усиление своего restore, не повтор Q125.

**Проверяемый путь:** P12501 writer/reader, реальный LoadCheckpoint и полные memory-trained/restored; producer snapshot независим от reader.

**Положительный контроль:** Исходный checkpoint и полный набор ordinary ответов проходят; exact/phrase/KNN contents, Topic, provenance, кратность и значимый порядок совпадают с независимым ожиданием.

**Различающий отрицательный контроль:** При прежних counts заменить KNN target, Topic или source lineage; удалить exact map; подменить один restored answer. Назвать отказ соответствующего comparer/finalizer, а не только факт записи SHA.

**Итог, предел и расход:** Все mandatory state/consumer outcomes доходят до reader-status и внешнего exit. Восемь уроков не учили отрицанию/вопросам; наблюдённые неверные ответы и 0,99 сохраняются. Сохранённые 72 Flow не повторять ради отчёта.

**Текст для исполнителя — передать целиком:**

> Работай по назначенному ID `P25-LEARNED-RESTORE-02 / Q125`, сохрани исходный паспорт и его pin. Прочитай действующие AGENTS/CLAUDE и применимые архитектурные, planning/plan-review, code-examination, test-integrity, QA/automation/debug, prompt-patch-authoring и human-communication навыки; запиши фактически прочитанное. Проследи путь: P12501 writer/reader, реальный LoadCheckpoint и полные memory-trained/restored; producer snapshot независим от reader. Сначала докажи: Исходный checkpoint и полный набор ordinary ответов проходят; exact/phrase/KNN contents, Topic, provenance, кратность и значимый порядок совпадают с независимым ожиданием. Затем одной адресной порчей: При прежних counts заменить KNN target, Topic или source lineage; удалить exact map; подменить один restored answer. Назвать отказ соответствующего comparer/finalizer, а не только факт записи SHA. Обязательное поле должно иметь названного потребителя, предикат отказа и влияние на общий итог/exit. Все mandatory state/consumer outcomes доходят до reader-status и внешнего exit. Восемь уроков не учили отрицанию/вопросам; наблюдённые неверные ответы и 0,99 сохраняются. Сохранённые 72 Flow не повторять ради отчёта. SOURCE_IMPLEMENTED и статическая сверка не заменяют нативную приёмку конечного source/DLL. Сборка, discovery, целевые тесты и обоснованная правка неверного теста входят в уже назначенный ремонт по 45/46; при отсутствии runtime сохранить точную ошибку среды, не сочинять PASS. Saved reanalysis не называть новым Train/Flow. STOP/NEXT пользователя имеет приоритет. Обязательная дельта POST131 (`POST131-P25-NATIVE-READER-BINDING`): Свяжи actual writer/reader с реально сохранёнными результатами TranslationMachine; два производных summary не считай независимыми источниками. На копии согласованно измени оба actual и длины, оставив native записи прежними: проверяющий должен выявить разрыв. Состояние сравни отдельно; reader остаётся Train0. Недоступную native запись зафиксируй как ограничение, без создания нового persistence API или повторного обучения. Применяй [договор 49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md); связывай raw с native только по реально существующим либо явно назначенным записям. Для versioned saved reanalysis раздели historical producer identity/schema и новую verifier identity/gates; не переопечатывай старые proofs, missing originals оставь NOT_PROVEN по49.

Основание: [аудит 145](../../fixes-based-on-experiments/codex-145-post128-transfer-and-experiment-audit-2026-11-03.md), [адресный ремонт 146](../../fixes-based-on-experiments/codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-04), [нативный договор 45](../45-learning-claim-and-native-acceptance-contract-2026-11-03.md), [QA46](../46-qa-execution-test-repair-and-fresh-results-2026-11-03.md). Исторические блоки ниже сохраняют свои данные, gold, factor, дозы и команды. Новый блок уточняет чтение и приёмку, не запускает PRIMARY и не подтверждает заново все старые результаты.
<!-- CODEX-POST128-CURRENT:END -->

<!-- POST113-CURRENT-2026-11-02:START -->
## После P113 — отдельный загруженный target и версия его ответа

P25-LEARNED-RESTORE-02 — самостоятельная учебная программа8 уроков EN_RU и72 Flow; она уже имеет сохранённые исполнения P2502 и P12501/Q125. Исторический0 Train старого P25 её не закрывал; нынешнее требование не назначает новый повтор. Positive отвечает ordinary consumer нового target после restore, без Train в restored плече, state identity до/после чтения сохранна. Negative подменяет restored map или связывает ответ с source-instance при прежнем export hash — поведенческий контроль должен провалиться.

M03c test уже исправлен отдельно; повторный Train третьего объекта снова недопустим. R01 гарантирует целую согласованную версию, но сам snapshot hash не доказывает восстановленный навык. Квитанция измерения указывает version именно отвечавшего target.

Основание: [адресный план 135](../../fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md). Срез `21007e25ec3f6f6634dd8733fc8631ae267e518c`, 2 ноября 2026. Этот блок уточняет указанные условия; исторические результаты, собственные данные, единственный фактор, учебный бюджет и очередь сохраняются. Порча копии квитанции не назначает новое обучение или дополнительное чтение модели. Нижние датированные указания применяются в своей исходной области; новые требования не делают старые ненаблюдённые поля измеренными.
<!-- POST113-CURRENT-2026-11-02:END -->

<!-- POSTMERGE-HARDENING-2026-11-02:START -->
## M03 — отдельный обученный повтор читает восстановленный target

Q125/P25-LEARNED-RESTORE-02 сохраняет свой ненулевой учебный рецепт; исторический P25 сTrain0 его не закрывает. После Save создаётся новый объект, Load, затем обычное чтение без Train в read-плече. Проверять полный ответ и состояние, не re-export или заново обученную копию. Контроль с отключённым loaded reader при правильных сохранённых bytes должен провалить поведение.

Основание и выбранный ремонт: [план127](../../fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md). Исходные результаты и учебные дозы сохраняются. Контроли реализации не добавляют скрытые Train/Read к научному паспорту; их стоимость и scope указываются отдельно.
<!-- POSTMERGE-HARDENING-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница STATE:** Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Исходный адрес из паспорта:** `27.TranslationDomain/TranslationMachine.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-CODEX-AUDIT-2026-11-02:START -->
## Действующее уточнение POST107 — Сохранённые bytes должен прочитать новый процесс без скрытого fit

**Проверка `POST107-Q125`. WRONG:** записать checkpoint и повторно читать прежний объект, как в P107, назвав это cold restore. **CORRECT:** Q125/P25-LEARNED-RESTORE-02 исполняет `TrainStructuredBatchAsync → ExportCheckpoint → новый процесс ImportCheckpoint → GetOrchestratorAsync/RunAsync`. Reader получает именно записанные bytes/hash и query inputs без expected answers; его PID/start identity и Train/Fit0 наблюдаемы. Нейронные moments здесь N/A: это learned translation memory.

**Поля и контроль.** Сравнить live S1→loaded S1 по каждой literal выдаче и содержимому exact/phrase/KNN, directions, semantic/provenance/identity/version fields, не по одному count24. Честный полный checkpoint сохраняет nonzero learned ответы. В контрольной копии bundle убрать обязательный owner payload или подменить target при прежнем числе ключей: неизменная структурная/содержательная проверка должна отвергнуть неполноту, не восстановить память из lessons. Такая калибровка receipt не добавляет Flow/Train. Если при будущем ремонте PAIR изменяется schema, сохранённая provenance и RULE-сегменты должны пройти версионированную миграцию; legacy tail не сохраняет обход source law, collisions не сливаются молча.

**Предел и бюджет.** Новая программа остаётся проектной: 1 Train/8lessons,72 Flow,1 export/1 import,2 processes. Исторические 24/24 parity и19/24 correctness — результаты прежнего alias, не автоматически результат Q125. Нового запуска, repair-fit или дополнительных cold checks дополнение не назначает.

Основание и общий порядок проверки: [аудит POST107](../../fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md) и [инструкция41](../codex-41-post107-executor-checks-2026-11-02.md). Этот верхний блок уточняет только названные доказательства и будущую приёмку; исходный текст/результаты ниже сохранены, очередь и разрешение запуска не меняются.
<!-- POST107-CODEX-AUDIT-2026-11-02:END -->

<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее уточнение после P102 — Холодный процесс читает тот же принятый S1

**Проверка `POST102-Q125`. WRONG.** Новый объект в том же процессе или повторный Train заменяет cold restore; TranslationMachine обязан иметь нейронные moments из-за общего шаблона.

**CORRECT.** PRIMARY P25-LEARNED-RESTORE-02 восстанавливает native TranslationMachine EN_RU memory через JSON ExportCheckpoint/ImportCheckpoint. Fresh child получает тот же S1: memory/signature/provenance/phrase/direction/identity/version поля по реальной schema. Neural weights/optimizer updates/moments здесь N/A по механизму, не отсутствующий обязательный payload.

**Адресная приёмка.** Сверить named outputs live→cold, hashes существующих checkpoint полей и actual native reader path; сохранить child identity. Пропущенное обязательное поле даёт typed incomplete. Не требовать несуществующего LoRA/AdamW bundle. При late report error читать тот же S1 с additionalTrain0; первоначальный PRIMARY сохраняет назначенное native обучение памяти.

Применять [единый контекст40](../40-effective-executor-context-2026-11-02.md) и [адресный план121](../../fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md): актуальные роли навыков, один рабочий оценщик, ordinary consumer и реакция на test/report failure. Это ограничивает вывод и усиливает приёмку; исходные factor, доза, fixed panel и порядок сохраняются. Исправление saved evidence не является новой PRIMARY, а наличие этого блока не разрешает запуск. Исторические указания ниже действуют только в своей исходной области; при противоречии бюджета/фактора требуется адресное решение до исполнения.
<!-- POST102-AUDIT-2026-11-02:END -->


<!-- POST89-EXECUTION-AUDIT-2026-11-01:START -->
## Актуальная приёмка после P89 — Восстановление не должно незаметно обучать зависимость

**Названная проверка `P25-RESTORE-NO-HIDDEN-FIT`.** Основание — [аудит 116](../../fixes-based-on-experiments/116-post89-execution-boundary-audit-2026-11-01.md), core `c4d05a27963fc3edb59f5f75bb097a1fc9936654`. Это требование к названной границе и будущему ремонту; оно не объявляет новое исполнение PRIMARY.

**WRONG — повторяемая ошибка:** Назвать обычный Compile чистым чтением только потому, что внешний код не вызвал Train; SemanticCompilerFacade может создать GrammarMaskFilter и вызвать его Train внутри PredictNextKinds.

**CORRECT — обязанность владельца:** Для конкретного owner перечислить все reached Fit/Train, включая вложенный Ngram, и отличить число вложенных вызовов от числа независимых обучений. Restore reader потребляет сохранённые bytes, не восстанавливает память повторным fit.

**Наблюдаемая приёмка:** Новый процесс с тем же typed bundle даёт сохранённые ответы; счётчики всех применимых обучающих owners равны 0. Если штатная точка обязательно учит зависимость, отметить её как непригодную для no-training restore-проверки и использовать объявленный read seam.

**Граница доказанного и расхода:** при написании этого блока alias P25/P125 и audit-only reader не закрывали учебную программу. Теперь Q125 имеет результат P12501: читать его вместе с границей закреплённой старой DLL. Исторический бюджет ниже сохранён как описание исполненной постановки, а не команда потратить его снова.

Текущий статус исполнения берётся из [Q-индекса](00-execution-index.md), где receipt, качество и перенос разделены. Старый DESIGNED ниже сохраняет дату исходного рецепта. Новое научное обучение, увеличение дозы или повтор старого attempt этим дополнением не назначаются. Общая проверка — [договор 39, §12](../39-transfer-evidence-contract-2026-10-30.md#12-после-p89-исполнение-входы-и-предел-приёмки).
<!-- POST89-EXECUTION-AUDIT-2026-11-01:END -->


<!-- AUDIT-2026-11-01:START -->
## Дополнение аудита 01.11.2026 — Новый reader без повторного обучения

**Основание:** [A09/RF09; аудит107](../../fixes-based-on-experiments/107-transfer-and-experiment-audit-2026-11-01.md), core `790228c256168a336b01abae4387e28391048c0c`. Этот блок уточняет текущую границу доказанного; исходный рецепт и его историческая дата ниже сохранены.

Сохранённый cold-restore результат остаётся полезным в своём scope. Новый summary или manifest не расширяет его на другой learner.

**Адресная приёмка:** `RESTORE_ACTUAL_OWNER_NO_TRAIN`: новый PID читает actual typed state данного owner, счётчики Train/Fit0, known/unknown/retention panel совпадает. Missing payload должен отказать; автоматический rebuild памяти из lessons запрещён как замена restore. LFS pointer и состояние другого Unit не positive control.

Дополнение не назначает повтор Train, не увеличивает прежний бюджет и не меняет scientific NEXT. Saved observations проверяются до любого нового вмешательства; ordinary product caller обязателен для claim переноса, а experimental host сохраняет свою объявленную границу.
<!-- AUDIT-2026-11-01:END -->

<!-- BEGIN audit-hardening-2026-10-30:P25 -->
## Приёмка после аудита 30 октября 2026

Этот блок уточняет действующие требования по [общему договору39](../39-transfer-evidence-contract-2026-10-30.md) и имеет приоритет над расходящимися обещаниями ниже. Прежние DESIGNED / SHALL_IMPLEMENT сохраняются как история проектирования на указанную там дату.

**Текущий статус.** P2502 выполнен и арифметически проверен: cold 0/24, trained 19/24, restored 19/24, literal parity 24/24. Это cross-process retention через checkpoint; обычные ExportAsync/ImportAsync и neural continuation этим не проверены.

Основание: [сохранённый результат на проверенном SHA](https://github.com/lubochka/llm_mvp_core/blob/cbf3b1d834cbf28baa2bb13baeb1bddc17e21019/experiments-v2/RP01-MICRO/P2502/results/attempt-0001/report.md).

### Проверка P25-EXACT-STATE-API

**Где проверять:** TranslationMachine.ExportCheckpoint/ImportCheckpoint и P2502 writer/reader.

**Требование:** Назвать точный API, typed state schema и компонентный состав. Проверять сохранённые outputs/provenance по ключам в новом процессе с запретом Train.

**WRONG:** Результат checkpoint adapter перенести на другой ExportAsync/ImportAsync или назвать checkpoint hash самой памятью.

**CORRECT:** Reader загружает фактические записи всех названных units; equality связывает ID, output и provenance, а не только число записей.

**Положительная приёмка:** Новый PID, train_calls=0, parity всех 24 literal outputs и прежний learned score.

**Отрицательная приёмка:** Подмена одного output при прежних ID/count или удаление state-компонента обнаруживается; recovery не переобучает reader.

Блок назначает критерии приёмки. Он не назначает новый Train и не меняет научный NEXT; уже оплаченный receipt и его исходные данные сохраняются.
<!-- END audit-hardening-2026-10-30:P25 -->


<a id="p25-learned-restore-02"></a>
## Действующий учебный повтор P25-LEARNED-RESTORE-02 · Q125

**Историческая постановка DESIGNED_NOT_RUN / SHALL_IMPLEMENT от27 октября2026; теперь RESULT_RECORDED в Q125.** Прямое уточнение Любы потребовало обучения внутри диагностического опыта. Старый P25-enru-cold-01 ниже имел0Train и не заменял эту постановку; позднее появились P2502 и P12501. Новое обучение и запуск в этой документной сессии не выполняются. Адрес Q125 и исходные результаты сохранены.

**Вопрос и механизм.** Сохранится ли результат настоящего native обучения EN_RU памяти после export и загрузки новым процессом? Цепочка `TranslationMachine.TrainStructuredBatchAsync` → `ExportCheckpoint` → новый процесс `ImportCheckpoint` → `GetOrchestratorAsync` → `orchestrator.RunAsync`, как в original P125 harness:68–76. Это обучение памяти, не LoRA; neural updates неприменимы. Значение «диагностический» не разрешает заменить Train чтением старого файла.

**Точные данные и модель.** Использовать только MEMORY-B из[36](../36-diagnostic-learning-programmes.md#memory-b): пустая native TranslationMachine и config/tokenizer/provenance исходного[P125 harness](../results/2026-10-18/P125-enru-memory-01/harness/Program.cs), исходные `train.json`, `training-batch-B.json` с8lessons и24-query/gold panel того же результата. Направление только EN_RU. Owner/signatures/phrase mappings/chosen/rejected/simulation provenance не сокращать. Не подставлять готовый B_trained.json вместо нового обучения. Его SHA `125c15ddb400dc00cfddb2367e1ea811917a87fb641c4e58ade22da2bea2e56c` — источник исторического контроля, а не требование совпадения нового timestamped файла.

**Одна конечная программа.** Writer process:24ordinary Flow пустой модели → ровно1TrainStructuredBatchAsync(8lessons) → проверить success/RowsTrained8/promoted ExecutorFor(EN_RU)/изменённую память →24Flow той же панели → export в новый R. Reader process после завершения writer: проверить SHA записанного state → импортировать именно этот полный checkpoint →24Flow теми же input без expected values. Никакого Train в reader, потому что он уже часть программы с выполненным writer Train. Сравнить typed learned memory и каждый literal output before-save/after-load, не только counts/route IDs.

**Бюджет:**1Train/8lessons/72ordinary Flow (24cold+24trained+24restored),1export/1import,2Student processes; никаких дополнительных warmup, моделей или loops. Сериализация использует исходный native JSON contract. Gold независим от candidate: первоначальный успех19/24 strict correctness и24/24 output parity — исторические ориентиры, а не автоматически переносимые новые результаты. Отдельно TRAIN recall, composition, near-match/negative controls;0→0 не учебное подтверждение. Удержание nonzero learned answers после restore обязательно для положительного вывода. Улучшение ресурса при том же навыке допустимо, ухудшения показывать рядом.

**Контроли на уже полученном receipt, без новых Flow:** удалить одну выученную запись из копии snapshot; сохранить ID/size, но подменить output; переставить provenance. Comparator должен обнаружить различия по конкретному ключу. Эти искусственные copies не импортируются вместо основной модели. Если модель вообще не обучилась, закончить как LEARNING_NOT_ESTABLISHED и сохранить raw, не объявлять проверенный cold restore. Не менять fixture до PASS.

**H/R/entry:** H=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P2502`; R=`H/results/attempt-0001`; будущие H/Experiment.csproj,Program.cs,Run-Experiment.ps1 по[34](../34-P-experiment-execution-contract.md). Runner запускает writer затем reader, ждёт terminal каждого и суммирует calls. Build≤600s,run≤900s,total≤1800s. Перед созданием прочитать существующий claim; не перезаписывать и не удалять результаты. При отдельно назначенном повторе брать первый свободный attempt, сохранив старый. Команда после реализации host и при разрешённой программе:

```powershell
pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P2502/Run-Experiment.ps1
```

Команда не запускалась и не проверялась сборкой. Report на русском в R: exact source/fixture/state SHA, native training receipt, cold/trained/restored outputs, gained/lost, полный budget, процесс/exit и ограничения. Default-модель исполнителя, skills/tools/ожидание по34и31. Никаких production/tests/CI изменений. STOP/NEXT немедленно; при NEXT не дописывать прежний опыт. После terminal — следующий незавершённый Q живого индекса; если Q после125 ещё не назначен, не придумывать его.

<!-- BEGIN primary-experiment-2026-10-27 -->
## Исторический alias P25-PRIMARY-01 — не замена учебному повтору выше

**RECORDED_RESULT_ALIAS,27.10.2026.** Это существующий `P25-enru-cold-01`, не новый запуск. Выбран P25.1; другие исходные подслучаи не считаются выполненными этим alias.

- **Проверенный путь:** `27.TranslationDomain/TranslationMachine.cs::ImportCheckpoint` → `GetOrchestratorAsync` → `ExecuteAsync`, native EN_RU memory, без neural updates.
- **Данные:** точные8 TRAIN и24-query/gold panel из [P125](../results/2026-10-18/P125-enru-memory-01/report.md). Новый Train в P25 отсутствовал.
- **Начало:** [B_trained.json](../results/2026-10-18/P125-enru-memory-01/states/B_trained.json), SHA256 `125c15ddb400dc00cfddb2367e1ea811917a87fb641c4e58ade22da2bea2e56c`.
- **A/B:** сохранённые outputs P125 B против ordinary outputs холодного импорта того же полного состояния.
- **Фактический бюджет:**1 load,0 Train,24 ordinary Flow. Сохранённый результат24/24 output parity,19/24 strict correctness. Это воспроизводимость приобретённого навыка, не новое приобретение.
- **H/R:** `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01`.
- **Артефакты:** [report.md](../results/2026-10-18/P25-enru-cold-01/report.md), [raw-outputs.json](../results/2026-10-18/P25-enru-cold-01/raw-outputs.json), [input-provenance.json](../results/2026-10-18/P25-enru-cold-01/input-provenance.json), [harness/Program.cs](../results/2026-10-18/P25-enru-cold-01/harness/Program.cs).
- **Историческая команда:** `pwsh -NoProfile -File C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/run.ps1`. **Не выполнять повторно**: это указатель на завершённый рецепт.

**Исторический промпт alias, не текущая очередь:** readback этого результата не требует его перезаписи. Но новый учебный повтор уже назначен выше как P25-LEARNED-RESTORE-02/Q125; прежнее «пропустить P25» не отменяет его. Default-модель, русский report, skills/формат —[34](../34-P-experiment-execution-contract.md), STOP/NEXT немедленно. Исторические0Train/24Flow сохраняются как факт старого запуска, не как разрешение нового нулевого опыта.
<!-- END primary-experiment-2026-10-27 -->

<!-- BEGIN mandatory-learning-and-recovery-2026-10-21 -->
## Обязательные правила для карточки и любого её промпта

**Найти tool и дождаться своей операции — обязанность исполнителя.** До выдачи команды прочитать [контракт31](../31-tool-discovery-and-wait.md) и указанные там реальные skills/references; по каталогу текущей среды найти launch и wait/resume/status, прочитать их схему и сохранить provider, handle, start time, H/R и deadline. Не выдумывать Kiro API по Codex-примеру, не просить пользователя выбирать инструмент или запускать команду вместо исполнителя.

**Ожидание не равно бесполезному polling.** Одна выдача entry; затем штатный bounded wait/resume того же session/cell/process ID до terminal exit или исходного experiment deadline. Нормальный wait можно повторять при silent/running. Два быстрых неблокирующих чтения без прогресса требуют перейти в wait; если он неисправен/ответы противоречат — независимое чтение H/R/claim/exit и своего PID+start. Не вводить Start-Sleep в занятый терминал. Предел одного ожидания≤60с — не предел эксперимента; yield/RPC timeout не доказывает exit/kill/нулевой расход. После диагностики живой процесс продолжают ждать. Отсутствие log не доказывает, что запуска не было. Не выдавать entry заново «синхронно на25минут», не подбирать results-NN, не удалять claim/partial, не сокращать TRAIN/steps до двух минут ради tool. При UNKNOWN сохранять неизвестный расход и разбирать существующий запуск. STOP/NEXT Любы немедленны.

**Не завершать ход на ожидании и не придумывать работу.** ISSUED/WAITING → штатный блокирующий/event wait исходного handle → terminal/exit → чтение назначенных raw → отчёт. Финальное «жду уведомления / доложу потом» до результата запрещено; обещание auto-notification не доказывает возобновление. Не создавать фоновый waiter/monitor/watchdog, циклы polling, progress-события каждые N шагов, summary scripts, manifests или другую работу «пока ждём»; не запускать агентов или следующую карточку. Обычное ожидание не требует заполнения паузы работой. Соблюдать provider no-poll/no-sleep. Проверить настоящий wait/continuation в каталоге до launch; при отсутствии доказать конкретный блокер, сохранить handle и INCOMPLETE, без выдуманного API или relaunch. STOP/NEXT Любы немедленны. Полный порядок и исключения — [контракт31](../31-tool-discovery-and-wait.md).

**Без действительного обучения задача не выполнена.** Для каждой новой экспериментальной программы, включая diagnostic/reader/restore, обязателен назначенный реальный учебный участок: original файл/метод, learner→изменённое состояние→обычный consumer, точные TRAIN/EXAM, before/after и конечный бюджет. Сохранённый обученный S0 является стартом, но не заменяет этот участок. Для retention/restore навык проверяется до вмешательства; для acquisition допустим cold start с приобретением поведения. Ноль обновлений допустим у отдельного контрольного плеча или reader после общего Train, но не у всей новой программы. Unit/BPE действительно обучают память/правила; neural updates=0 у них не означает отсутствие native Train. Updates, loss, hash и случайный верный токен сами по себе не доказывают навык; 0→0 не является учебным результатом, частичные токены не объявляются целым ответом. Исторический безучебный receipt сохранить; его повтор получает отдельный назначенный рецепт по оригиналу и новый output, а не считается завершённым новым опытом. Уже выполненный учебный участок этой же попытки не повторять ради счётчика после сбоя отчёта; продолжить его сохранённые stages. Отдельная unit-проверка готового receipt не является новым экспериментом обучения. Полный охват карточек и адресные рецепты — [37](../37-full-catalogue-learning-coverage.md).

**0 → 0, отсутствие обучения или неполное A/B — не завершение эксперимента.** Раздельно записать execution_status, learning_validity и hypothesis_status; при невалидности учебная цель OPEN. Сохранить сырые свидетельства и продолжить разрешённое исправление постановки, а не только написать «невалидно» и отчитаться об успехе. Не искать другой checkpoint/модель/host, не менять seed/фактор, не увеличивать steps до PASS: точный исправленный рецепт назначает автор карточки до новой попытки. Старые опыты не удалять. Поздняя ошибка сохранения не обнуляет состоявшиеся updates; неизвестное число — null, невыполненное плечо — «не измерено».

Любое полезное измеренное улучшение в **валидном учебном сравнении** — успех своей метрики/домена; рост accuracy необязателен, регрессии рядом. Диагностика невалидной постановки не закрывает учебную цель. Default-модель исполнителя без override; точные H/R/entry ниже. Отчёт — русский R/report.md. Текущая правка документов не разрешает runtime. Прямые STOP/NEXT Любы немедленны и при открытой цели.

Полный конечный порядок диагностики и критерии: [контракт30](../30-failure-recovery-and-learning-validity.md). При конфликте со старым текстом ниже действует этот блок.
**Состав данных ограничивает вывод.** До запуска автор задаёт точные TRAIN/EXAM/CONTROL, покрытие классов, роли отрицательных примеров, семьи/пересечения и конкретный контроль постоянного ответа. Одноклассовый TRAIN и выигрыш на его единственной метке не доказывают различение маршрутов; updates/confidence не заменяют этот контроль. Разные опыты не устанавливают причину регрессии состава данных. Измерять любой исход, не предписывать ухудшение; набор и budget исполнитель не меняет сам. Полное обязательное правило — [состав данных и допустимый вывод](../32-training-set-and-inference-contract.md).

**Сохранённый учебный результат используется без повторного выбора.** В уже разрешённой фазе исполнения пригодные сохранённые веса/обученное состояние имеют приоритет: восстановить их и продолжить назначенный опыт, не спрашивая «A: использовать результат или B: обучить заново». Поздняя ошибка проверки, сериализации или отчёта сама по себе не основание повторять Train. Сначала прочитать актуальные receipts и установить, не выполнены ли recovery/плечи уже; состоявшуюся работу не переиздавать. Проверять только необходимую для следующего шага совместимость и учебную квалификацию, используя имеющиеся данные; недостающие показания получать лишь по явно ограниченному recovery-рецепту, без optimizer updates и без притворного восстановления отсутствующих moments/RNG. Если пригодность доказана, выбрать reuse автоматически. Если объективно отсутствует необходимое состояние, назвать конкретный технический пробел; не предлагать повторное обучение «для чистоты», не просить пользователя снять guard и не выдавать размер файла/падение loss за доказательство выученного навыка. Текущая документная фаза запусков не разрешает.

**Реализация не отменяет контракт.** «Код не читает RNG/optimizer/проверку» означает отклонение, а не разрешение его игнорировать. До следующего model call исправить реализацию в уже разрешённом scope либо применить только явно назначенный в карточке совместимый режим восстановления; не придумывать исключение, не просить снять guard и не переносить weights-only DPO-исключение на SFT continuation.

<!-- END mandatory-learning-and-recovery-2026-10-21 -->



**Связь с родителем следующего run:** cold consumer над тем же обученным состоянием относится к обучению без дополнительных Train. Native accepted-parent publication — отдельный [контракт29](../29-learned-start-and-parent-continuation.md); raw H16/H32 SFT import не является его доказательством. №08/09 и их probes не повторять.
**Позднее evidence81141:** [RESTORE-MINIMAL-SET](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json) сохраняет в исследовательской H16 модели weights hash/panel и optimizer import/step1000, затем продолжает200updates доstep1200. TRAIN response NLL4.766696→4.711598, exact0/2, отдельные correct tokens есть; Pos-контроль ухудшился. Это не production parent promotion и не доказательство, что каждый новый baseline-опыт обязательно неудачен. Нельзя назначить этот H16 checkpoint другой модели без конкретной topology/tokenizer/optimizer совместимости; текущие pinned B0/бюджеты других карточек не меняются. [Адресный readback](../../2026-10-20-card-reconciliation/0ea-late811-readback.json). Проба1000 с seededbase не является продолжением N63I unseededbase и не устанавливает единственную причину0→0. Новых запусков из этого свидетельства не следует.


## Проверка learned Verifier при restore: уточнение 0ea

Статус SOURCE_CONFIRMED_VALIDATION_GAP. Это семейство, не новый исполнимый ID. [BuildVerifierExample](C:/Projects/llm_mvp_core/Tests/TranslationDomainTests.cs:2679) создаёт chosen Pass=true и Issues=[]; [checkpoint-test](C:/Projects/llm_mvp_core/Tests/TranslationDomainTests.cs:3052) затем проверяет только отсутствие BOOTSTRAP. [Writer](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:1104) добавляет learned rule только для !chosen.Pass с распознанным issue. Поэтому этот witness не доказывает восстановление приобретённого правила; importer defect из него не следует. P25 memory19/24 и 24 сохранённых outputs остаются отдельным реальным результатом.

При назначении автором конкретного P25/P56 restore-рецепта обязательна цепь: заданный FAIL lesson → отсутствие/появление ровно назначенного rule key 0→1 → native Verify PASS→FAIL для нарушающего кандидата и PASS для законного → cold load тех же bytes в fresh process → тот же key и оба verdict без новых уроков. Выбрать обычный non-phrase route: phrase-map confidence>=.85 может обойти FAIL. Если key не появился, записать точную причину parser/fixture, не менять exporter наугад. Не использовать «не BOOTSTRAP» как замену этим наблюдениям.

Для этого дополнительного подслучая точные FAIL JSON, src/ctx/signature, lawful/violating запросы, checkpoint artifact и isolated host пока не назначены: NEEDS_INPUT_FOR_RECIPE. Это работа автора рецепта; исполнитель не ищет и не изобретает входы/команду. Не добавлять отдельный тестовый запуск, не расширять N60-E1 или VT-APOSTROPHE, не менять их бюджеты. Runtime в текущей документной сессии отсутствует.



**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Дополнение по новым конфликтам (20.10).** Это исходное семейство гипотез, не отдельный runtime work order. До передачи исполнителю автор конкретного рецепта назначает наблюдаемый положительный учебный сигнал; одинаковые 0→0, записанные веса или экономия не доказывают освоение задания. В изолированном опыте допустимы throw/try-catch/void и минимальный host, без переделки в product OperationResult/комментарии к каждой строке. Модель сессии по умолчанию сохраняется. При обычном завершении успешного назначенного опыта обязательны русский отчёт и одна краткая запись кандидата в [required-changes.md](../../required-changes.md); не нужны отдельное поручение регистрации, product tests/CI/Git или ручные checksums. Немедленные переход/stop имеют приоритет. Fixed-order время/CPU/process peak — наблюдения без установленной причинности. Подробности — [действующее правило](../00-execution-policy-2026-10-20.md); эти ограничения обязательны для каждого промпта ниже.

**Разрешение конфликтов из последнего аудита (20.10).** Старый запрет копий в `81-validation-program.md` не применяется: используется только назначенная A/B-копия; product/tests/CI не меняются. Ограничение «protected не менять» в отчёте помощника относилось к его поручению, а не отменяет прямое поручение Любы исправить карточки. `NOT_RUN` означает отсутствие результата данной версии, а не запрет; поздняя карточка не меняет протокол уже сохранённого опыта и не назначает повтор. Любое полезное измеренное улучшение — успех по метрике/домену, без порога5% и требования роста accuracy; несопоставимые затраты не приписываются фактору. Связанные подготовка, restore и чтение выученного состояния допустимы с точным source→state→consumer; посторонний scorer-пример не назначает опыт. Вместо отсутствующей84-ссылки — [реестр источников](../01-full-source-registry.md) и [карта покрытия](../2026-10-19-small-learning-probes/06-findings-map.md); следующая работа задаётся [индексом](00-execution-index.md). Назначенные данные, домен, host и команды нельзя заменять самостоятельным поиском другого опыта.

**Место в каталоге:** справочная карточка. Порядок маленьких запусков и точные рецепты — в [едином индексе](00-execution-index.md); номер P не является номером запуска.

## Немедленное завершение по указанию Любы

**Обязательно для каждого подслучая и каждого промпта исполнителю этой карточки.** Прямое текущее указание Любы имеет приоритет над нижними требованиями завершить отчёт, review или весь цикл.

- **«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу.
- Если следующее задание уже указано, не выбирать другое и не обходить каталог в поисках более готового. Если сказано «следующий» или назван порядковый номер — взять его из единого индекса `cards/00-execution-index.md`; не искать N-KU-02 по аналогии с N-KU-01 и не задавать вопрос о выборе. После конца назначенной очереди кратко сообщить об этом, не возобновляя прошлый опыт. Поручение редактировать карточки или описывать опыт само по себе не разрешает запуск обучения.
- **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Команду перехода не подменять остановкой всей программы; команду полной остановки не подменять следующим запуском.
- Незавершённые файлы и уже записанные данные оставить как есть. Их наличие не разрешает продолжать вопреки указанию. Не удалять raw/receipts и не устраивать закрывающую проверку; краткий статус — только по уже известным фактам, без новых инструментов.
- Исполнитель опыта не берёт на себя исправление карточек: это отдельное поручение документной сессии. Остановка одного исполнителя не отменяет отдельное поручение другой сессии, если Люба не остановила и его.

## Как выполнять этот опыт: разрешение конфликтов инструкций

## Готовый формат отчёта и навыки — без поиска образца

**Когда оформление закончено.** Достаточно одного русского report.md по этому каркасу: до 350 слов пояснений плюс таблица фактических метрик/ответов и ссылки на raw. При обычном завершении полезного измеренного опыта обязательно добавить или уточнить одну запись до 120 слов в ../../required-changes.md со ссылкой на отчёт; отдельное поручение не требуется. Не выполнять оформление после команды немедленно перейти или остановиться. Один раз прочитать записанное, исправить только фактическую ошибку, сломанную ссылку или кодировку. Не делать циклы стилистического улучшения, Mermaid/SVG/render, исследования Git или новую проверку алгоритма. Не вычислять и не сверять hash ради отчёта. Уже существующие технические receipts оставить как есть; после правки текста достаточно фразы «уточнён отчёт, данные и опыт не менялись». Не удалять старые raw или receipts ради упрощения отчёта. Состояние «независимый review не выполнен» записать честно, оно не требует искать другую модель или удерживать оформление открытым. После этих файлов задача оформления завершена; следующая работа — только уже назначенный следующий опыт, без нового поиска формата.

**Язык — русский. Формат — обычный Markdown.** Итоговый файл — `report.md` в точном каталоге результатов выбранного подслучая, указанном в этой карточке; рядом остаются исходные JSON/логи. Не искать другие отчёты, новый реестр или шаблон. Перед каждым новым аудитом/заданием свежо и полностью прочитать governance и весь основной/ролевой стек договора120 из входного блока этой карточки; старые пять навыков не являются полным перечнем. Внутри того же задания отчёт/readback/wait не требуют повторять уже выполненное полное чтение. Исторические команды навыков не расширяют текущую фазу, модель, данные или бюджет.

Скопировать следующий каркас и заполнить только фактическими данными. Пустые измерения писать «не измерено», сбой — «этап не завершён», а не 0.

```markdown
# <ID>: <что изменилось в обучении, простыми словами>
Домен: <предметная тема>. Исполнитель: <фактическая модель и режим>.
Проверяющий: <фактическая модель или «не выполнялась»>.
execution_status: <NOT_STARTED / PARTIAL / COMPLETED / FAILED / OUTCOME_UNKNOWN>.
learning_validity: <VALID_FOR_NAMED_UNIT / INVALID_NO_TRAINING / INVALID_ZERO_TO_ZERO / INVALID_UNQUALIFIED_BEFORE / INVALID_INCOMPLETE_COMPARISON / NOT_EVALUATED / UNKNOWN>; единица и raw before/after: <точно>.
hypothesis_status: <OPEN / SUPPORTED_FOR_NAMED_METRIC / NOT_SUPPORTED_IN_VALID_COMPARISON>; invalid всегда оставляет OPEN.
Источники: <паспорт, raw, команды>; завершённые updates отдельно от ошибки сохранения, неизвестные counts=null, невыполненное плечо — не измерено.

## Что меняли
<Один фактор, исходный файл/метод, куда урок записывает навык и кто его читает.
A — исходный код, B — конкретная правка только экспериментальной копии.>
## На чём проверяли
<Точные TRAIN/EXAM или ссылка на сохранённые данные; начальное состояние; реальные вызовы/предъявления.
Запланированный и фактический бюджет отдельно; лишние операции перечислить.>
## Что получилось
| Показатель, единица и область | A до | A после | B до | B после | Изменение и смысл |
|---|---:|---:|---:|---:|---|
| <точность/ошибки/состояние> | | | | | |
<Стоимость реального перехода обучения: A = …, B = …, B−A = ….
Время, CPU, allocated bytes, retained heap, RSS/peak — отдельные строки.
Слои модели, уровни Unit/Flow и усилия подготовки — значения или не измерено.
Для короткой панели привести каждый фактический ответ и gold.>
## Выигрыши и ухудшения
<Любое полезное измеренное улучшение — успех по этой оси и домену;
рост accuracy не обязателен. Все ухудшения рядом. Ожидание не результат.>
## Граница вывода и кандидат изменения
<Что измерение доказывает и чего не измеряло. Не объяснять шум/JIT без контроля.
Точный исходный метод и предлагаемый diff для решения Любы; основной код
автоматически не меняется. Никаких новых запусков ради составления отчёта.>
```

**Хорошо:** «За тот же учебный переход выделено A=12 МБ, B=9 МБ; ответы одинаковы. Выигрыш по выделениям — 3 МБ. Пик процесса вырос на 1 МБ. Один запуск; причина разницы wall-time не установлена». Числа здесь — только образец формы, не результат.
**Термины и предел редактирования.** Сначала простое русское название, затем точный ID/API в скобках; статус словами, машинный код — рядом в backticks. Не строить Mermaid, симуляцию ветвей или повторный расчёт алгоритма ради отчёта. Использовать уже записанные ответы и counters. «Не предложил лишний цикл» не означает «весь совет правильный»; «расходы не измерены/шумны» не означает «без дополнительных затрат». Проверка push, обход истории и перезапуск модели не являются частью написания отчёта.
**Плохо:** «Все тесты зелёные, обучение исправлено»; «accuracy не выросла, поэтому экономия памяти не считается»; «память стала 9 МБ» без указания, что измеряли.


**Послабление для экспериментального кода — прямое уточнение Любы.** Малый экспериментальный host может использовать `throw`, `try/catch`, `void`, простые DTO и собственный формат журнала. Не переделывать API скопированного алгоритма ради продуктового `OperationResult`, обязательных комментариев, полного code-slice workflow или требований к публикации. Для ошибки достаточно сохранить её текст, этап, фактические счётчики и ненулевой exit code; не превращать сбой в нулевую точность и не проглатывать его. Это послабление относится к оболочке опыта, не разрешает менять данные, фактор, бюджет или исследуемый алгоритм между A/B. Полные продуктовые требования применяются позже, если Люба поручит перенос результата.

**Прямое уточнение Любы, 20.10.2026:** сначала проверить изменение на экспериментальной копии того же кода и передать результаты пользователю; перенос в основной код — только по её отдельному решению. Поручение на проектирование означает правку карточки; прямое поручение на запуск означает выполнение опыта по порядку ниже.

1. Выполнить назначенный подслучай и предметную тему по конкретному паспорту ниже. Уроки, EXAM, начальное состояние, один фактор, бюджет, точные пути, зависимости и команды задаёт автор карточки; исполнитель только материализует приведённые данные и рецепт. Ему не поручается выбирать другой host, искать входы, придумывать исправление или разрешать конфликт правил. Общего `PENDING` и повторного личного разрешения на каждый ID/SHA нет. Если доступный технический рецепт ещё не заполнен, это `PREPARABLE` — адресный недочёт автора карточки, а не запрет обучения и не вопрос на новое разрешение Любы.

**После завершения двух плеч.** Продолжение по сохранённому снимку не разрешает операции, которые пользователь не поручал: дополнительные warmup, equivalence, cosine, B→A или reviewer rerun. Зафиксировать уже выполненные лишние вызовы отдельно и не выполнять оставшиеся самовольные добавления. Не обходить worktree, не исследовать `.claire`/`.clone`, не чинить чужие терминалы, навыки или память агента ради гигиены опыта. Редакционная правка report.md не требует нового A/B или вычисления hash: сохранить старые receipts, добавить одну фразу «уточнён текст, данные и опыт не менялись». Не создавать новые таблицы контрольных сумм ради отчёта. Для reviewer использовать доступную модель сессии по умолчанию; другой worker означает независимую проверку, самопроверка — нет. Не связывать сохранение результатов с разрешением на commit/push или перенос в продукт.
2. Эта P-карточка сохраняет исходные варианты, но не назначает проект, домен или запуск по шаблону <ID>. Единственный источник задания — конкретная строка [индекса](00-execution-index.md) и её исполнительская карточка: там назначены полный путь A/B-копии, host, входы, Student, команды и output. Не искать и не выбирать другой проект. Если для запрошенного подслучая такого рецепта ещё нет, это документная работа автора карточки, а не поручение исполнителю изобрести host; в разрешённой программе перейти к назначенному «Дальше». Модель исполнителя — модель его сессии по умолчанию без override; не путать её с обучаемым Student.
3. A — исходная реализация, B — её копия с одним изменением. Сохранить исходный commit, пути/методы, hash скопированных файлов и A/B diff. Общие зависимости и начальные состояния одинаковы; фактический learner исполняется в экспериментальном проекте. Не заменять его расчёт ожидаемым ответом или ручной имитацией. Ссылки ниже на оригинал — происхождение кода и будущий адрес переноса, а не разрешение редактировать production.
4. Выполнить одинаковые уроки и контрольные задачи до/после в A и B. Для зависимого от обучения участка допустим общий сохранённый обученный state. Собрать только точный host из паспорта, затем вызвать его DLL по указанной там команде; не загружать сборку через PowerShell reflection и не запускать общий dispatcher. Не добавлять warmup, повторы, проверку фасада или новые уроки сверх счётчиков карточки. Для совсем короткого измерения указать шум/предел разрешения, а не вводить 20+200 прогонов. Разница A/B не разрешает дополнительный порядок B→A, новый диагностический каталог или повтор от reviewer. Сохранить наблюдаемую разницу; без отдельного контроля не объявлять JIT, порядок запуска или «шум» доказанной причиной. Уже выполненные сверх бюджета операции сохранить и отдельно перечислить, не включать молча в основной результат. Не чинить продуктовые тесты, не запускать полный CI, старый Main500 или другой S-слайс ради этого опыта.
5. Записать реальные ответы, обучение, время, память, усилия, слои модели и уровни Unit/Flow; неизмеренное обозначить явно. Любое полезное измеренное улучшение — успех по показателю и домену, без обязательного роста точности; ухудшения указать рядом. Отчёт и raw outputs: `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/<дата>/<ID>/`. Показать Любе результат и точный кандидат переноса. Даже успешный опыт не разрешает менять основной код, его тесты, делать commit/push или разворачивать полную цепочку внедрения.

**Какие старые указания здесь не применяются.** `AGENTS.md`/`CLAUDE.md`, разделы `GOLDEN RULE`, `EXECUTION_INTEGRITY_PUBLICATION_GATE` и `G-route/G-loop`, описывают продуктовый цикл; они не превращают этот опыт в внедрение, обязательный push или бесконечный следующий слайс. `.kiro/skills/xiigen-code-slice/SKILL.md` отправляет в `.kiro/xiigen-slices/00-START.md` и чужой active brief: не использовать этот переход для данной карточки. Требования ниже изменить оригинал, запретить точную экспериментальную копию либо ждать личного допуска каждой версии заменены этим прямым уточнением пользователя. AGENTS, CLAUDE, навыки и steering в рамках карточки не редактировать.

**Навыки и роли.** Перед новым заданием свежо и полностью прочитать основной стек [договора120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md#1-свежее-полное-чтение-перед-новым-аудитом-или-заданием) и дополнительные навыки роли из [40](../40-effective-executor-context-2026-11-02.md); раздел «Готовый формат отчёта и навыки» задаёт формат, а не сокращённый перечень чтения. Исполнитель и последовательный независимый проверяющий используют модель своей текущей сессии по умолчанию, без model/reasoning override и поиска Astra. Это прямое уточнение Любы от 20.10.2026. Фактическое имя модели записать; если платформа его не сообщает — «не раскрыто платформой», без догадки. Не более одного активного worker, без вложенных агентов. Модель исполнителя не является обучаемым Student/Unit. Проверяющий читает исходники, diff и сохранённые raw/receipts. Независимость означает отдельного проверяющего, а не другую модель; самопроверку так и назвать. Никаких повторных Train/Predict или прогревов ради review. После compaction восстановить паспорт, stage и actual counts; summary не является новым поручением.

**Параллельная сессия улучшения карточек.** По прямому разделению работы Любы другая сессия читает рассуждения исполнителя и правит только карточки: она не меняет код, данные, веса или runtime и не запускает эксперименты. Поэтому её документные правки и новый HEAD с такими правками не создают конфликта запуска готового опыта. Исполнитель работает по зафиксированному паспорту; документная сессия не требует остановки, повторного личного допуска или нового обхода AGENTS/skills. Реальное пересечение возможно только при фактической работе с теми же исполняемыми файлами или состоянием; его нельзя предполагать из одной правки Markdown.

**Сохранение экспериментов — прямое правило Любы.** Можно добавлять новые экспериментальные файлы и результаты; старые эксперименты, исходники, результаты и каталоги сборки `bin/obj` не удалять, не переносить и не перезаписывать. Общие cleanup-инструкции этого не разрешают. Совпадение пути с уже начатым опытом не разрешает пересоздать его: продолжать его зафиксированную версию, если именно она поручена, сохраняя уже полученные результаты. Новая редакция карточки не означает второго запуска или автоматической миграции текущего опыта. В документной сессии разрешено менять только карточки.

**Версия без бесконечного перечитывания.** Один раз прочитать фактические bytes выбранной карточки, исходников и применяемых навыков и записать их hashes в паспорт. Изменение HEAD из-за чужих документов не требует повторного обхода всех AGENTS/skills и не меняет уже зафиксированные условия опыта. Если до запуска изменился сам используемый механизм или его входы — сравнить именно это изменение и закрепить согласованную версию. Не считать hash старых bytes hash текущего файла. Чужие правки не откатывать; результаты не перезаписывать.

**Обязательное дополнение к каждому промпту исполнителя в этой карточке:** «Работай на точной экспериментальной копии исходного механизма по порядку выше. Основной код и продуктовые тесты не меняй; после измерений передай результаты и предлагаемый перенос Любе. Не запускай продуктовый цикл и не переназначай модели самостоятельно».


## Исходные варианты для автора — не исполнительский паспорт

Это историческая карточка вариантов, **не готовый исполнительский паспорт**. Запрошенные исполнитель и reviewer: **модель текущей сессии по умолчанию**, последовательно, не более одного активного worker; фактическое назначение записывается при выполнении. Отдельное личное согласование каждой карточки отменено указанием пользователя от 20.10.2026. Student/Unit не равен модели исполнителя: точное имя, конфигурацию и исходное состояние назначает автор конкретного рецепта для одного подслучая и предметного профиля. Исполнитель не выбирает Student из справочника.

**Исходное задание и ограничения — дословно:**

«Ок, рассмотри пожалуйста последние обновления помошников и выстрои из них экперименты, как можно более простые и маленькие для каждого домейна. Задача ускорить процесс проверки и сузить обучающийся механизм до очень простых маленьких примеров, что бы проверить улучшаются показатели довольно быстро. Отчетность должна быть человеческим языком но точно показывающая какую часть механизма обучения меняем и как будем тестировать.»

«Разве эксперименты не были построены по формуле: Что  было - пробег по маленькому сету, изменение в качестве эксперимента, измеренеи как стало стем же сетом?»

«ну раз так то у меня нет особо выбора, ты должен внести изначальные промпт, скилы, начальные данные, название модели, ожидаемый  результат в тело эксперимента и мне придется тебя проверить перед началом работ.»

«Нельзя подменять задание пользователя - измерять изменения в процессе обучения - если то что измеряется никак не участвует в процессе обучения - это не валидный эксперимент! Не валидно тратить на это токены! Особенно когда, пользователь это запретил!»

Критерий пользователя: любой фактически измеренный полезный выигрыш в процессе настоящего обучения — успех по показателю и домену; рост точности не обязателен. Все другие изменения и потери рядом. Отдельный serving/report/cold контроль не становится учебным опытом из-за формальных уроков. Для изменения самого обучения оба плеча действительно учатся и фиксируются A0/A1/B0/B1. Для восстановления или использования его результата допустим общий доказанно обученный state и сравнение зависимого участка до/после одной правки; код этой связи и способ исправления обязательны. Существующие acquisition/no-update контроли не переименовываются в новое обучение.

**Данные и ожидаемый результат:** конкретные исходные строки, TRAIN/EXAM и gold находятся ниже в выбранном `Dxx — профиль проверки` и `Pxx.n — самостоятельное сравнение`. Они не образуют автоматически один общий запуск. Автор до передачи исполнительской карточки фиксирует в её теле один Pxx.n, один Dxx, точные уроки/экзамен/prompt/рубрику, native writer/update/reader, допустимые файлы, бюджет и ожидаемые различающие наблюдения. Если данные в профиле описаны только условием, это **NEEDS_INPUT**, а не разрешение исполнителю придумать их во время опыта.

**Незаполненные поля:** `selected_subcase`, `selected_domain_profile`, точные initial data/state/model/config/tokenizer/reference/optimizer/RNG bytes и применимые SHA256, actual native caller/command, одна разрешённая mutation, output/RunId, окончательные counts/cost bound и ссылка на действующее поручение пользователя. Для Unit-пути нейронные поля явно N/A с причиной, не вымышленные веса. Общий источник ниже не заполняет эти поля автоматически. Пока автор не назначил подслучай и полный рецепт, это справочник вариантов. Автор закрывает перечисленные исходные поля до передачи задания; исполнитель записывает только фактические наблюдения назначенного выполнения. Конкретная недоступная зависимость фиксируется точно, без поиска другого опыта. Фаза определяется последним поручением пользователя, а не датой карточки.

**Навыки внутри тела:** следующие hashes и адресные требования переиспользованы из проверенного паспорта04/18; это не утверждение нового полного чтения восьми файлов для каждой P-карточки.

| Навык, SHA256 и диапазон ранее прочитанного текста | Применяемое требование |
|---|---|
| [architect-behavior-classifier-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md), `e876e0ecae57aeda56cec054deedd8c2c6e8e40eb3bed6ed32e92ce86473f507`, 281–295 | «human clarity, readable architecture» — назвать реального learner и границу доказательства |
| [planning-session-startup-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/planning-session-startup-SKILL.md), `0aa3da46367b362f7bbc1a81254dbccc5e9b4ea91aed9f1cb4234ce0a573d802`, 1615–1636 | «Who executes the work and what is in scope?» — роли, разрешённые файлы и текущая фаза записаны до запуска |
| [plan-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md), `792d4bb9621154dbfb7c7dd6220e7ebd64f206e01234f7b58f32d38b83925358`, 2115–2143 | «exact source/prompt/file lookup presented as generalization» — не выдавать lookup или совпадение учебного входа за новый ответ Student |
| [generated-code-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md), `1b32d8a121fad463cdde6532cb86baa52974b6560f8a20b545f163b65e992a52`, 762–786 | «review must first verify the work-order boundary» — проверка source не расширяет поручение до реализации |
| [output-readability-gate-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/output-readability-gate-SKILL.md), `493e15347e7e28ad7780ad4aee088271107c64b403deb396db256846bd660a2d`, 197–218 | «Что ожидалось и что фактически проверено» — ожидаемые и actual поля раздельны |
| [human-readable-architecture-audit-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md), `a08e8f79995929fafa80f32c858329ffeb11a9080caa7b89b63fe50c4a5f6a7f`, 1–55 | «различать предметную тему обучения (домен), конкретную задачу и направление» — предметная тема берётся из выбранного D-профиля; механизм указан отдельно |
| [human-communication-GLOSSARY.md](C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md), `39c3e64aa78733f542be3db562c0845d8c6e47ebe11b32127baa4fd4ca49d4eb`, 5–22 | «Домен — предметная тема, по которой обучается единица.» — не называть optimizer или hash предметным доменом |
| [prompt-patch-authoring-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md), `e96526474e658664d1594dd3d72a7a0fa87c006c045eeb498774bb3c2d475409`, 1–95 | «A patch targets exactly one bottleneck per cycle.» — один выбранный фактор; изменение всего pipeline не локализует причину |

**Видимое поручение до исполнения:** выполнять только конкретный подслучай назначенной строки [единого индекса](00-execution-index.md), с заранее заполненным автором рецептом: предмет, Student, исходное состояние, данные, host, команда и бюджет. Модель исполнителя — его текущая по умолчанию без override. Исполнитель не выбирает другой подслучай и не исследует, как заполнить пропуски; автор закрывает их до передачи задания. Исторические бюджеты и весь набор подслучаев автоматически не запускать. При отсутствии доказанной учебной связи автор уточняет source→state→consumer, а проверку совсем вне обучения не назначает. Дополнительные уроки для формального допуска не придумывать.

**Правило успеха каждого задания (Люба, 19.10.2026):** любое фактически измеренное полезное улучшение — успех по этому показателю и предметной теме: скорость обучения/ответа, CPU, RAM/VRAM, усилия, число уроков/обновлений, объём состояния, слои модели, Unit/Flow, точность и иные полезные величины. Рост точности не обязателен. Остальные изменения и ухудшения показываются рядом; возможны одновременно успех RAM и ухудшение точности. Локальный измеренный эффект признаётся локальным успехом с границами, без обязательной серии повторных запусков. Для сравнения нужны одинаковая работа и сопоставимый измеритель. Исправление измерителя — успех достоверности; смена подписи/единиц не равна физическому ускорению. Это не допуск к публикации и не утверждение об иной неизмеренной способности. Паспорт сохраняет условия опыта; разрешение на уже порученную программу действует без повторного личного согласования каждой версии. Конкретные недостающие входы указываются отдельно.

**Участие в обучении — обязательная граница задания:** перед выполнением названы настоящий обучаемый получатель, запись урока/изменённого состояния и чтение этого состояния в процессе обучения; сопоставимы исходное состояние, уроки и экзамен, кроме одного выбранного фактора. Измеряется реальное обучение нейронных весов либо обучаемой памяти Unit. Допустимы также участки до и после update, если от них зависит обучение или использование выученного состояния: загрузка, подготовка пар, восстановление, выдача результата. Для каждого указать файл/метод оригинального кода, цепочку зависимости, что сломано и как исправить. Отсутствие нового update в таком опыте само по себе не исключает его; лишние уроки ради галочки не нужны. OUT_OF_LEARNING_SCOPE относится только к проверкам, совсем не связанным с обучением и его результатом.

18 октября 2026. **READ_ONLY_RESEARCH; проект, не выполнено.**

## Механизм и общий контроль

**Получатель изменения:** Конкретный владелец persisted state: RP01 private full checkpoint, TranslationMachine trained maps либо generic KNN. Один owner на сравнение; его обычный caller обязателен.

**Механизм:** конкретное вмешательство A/B описано по самостоятельным подслучаям ниже. Это поле отделено от предметной темы обучения.

Оба плеча используют один frozen corpus и независимые состояния из описанного старта, одну панель и scorer. На один A/B выбирается ровно один подслучай ниже. Все остальные изменения запрещены данным сравнением. Положительный результат на любой оси сохраняется вместе с tradeoffs; числа пилотов старых источников не являются квотой успеха.

## Самостоятельные сравнения

<a id="s-1"></a>
### P25.1 — Metadata-only export против owner full state

**Восстановление выученного состояния входит в учебную область**, когда показана цепочка trained owner → сохранённые artifacts → обычный consumer и его предметный output. OUT_OF_LEARNING_SCOPE относится лишь к metadata-only перечню полей без такого consumer. [G2](../2026-10-19-small-learning-probes/21-additions-n58-learning-scope-and-coverage.md) — отдельная передача между уроками; её незавершённость не запрещает проверку cold consumer с0новых Train. Исторические результаты не переписываются, готовность определяется точными входами подслучая.

A metadata-only export API, B соответствующий полный owner state export при тех же trained state bytes. Для TranslationMachine сравнить maps/Units; для RP01 private checkpoint отдельно. Перечень полей не заменяет cold output. Generic LogicalKnnModelManifest count/role/dimensions/k не удостоверяет BPE merges, indexed vectors и labelled output-map. В identity control одинаковые 8rows с перестановкой сохраняют logical fields, но state SHA измеряется независимо; время/alloc исключаются из equality. B identity включает canonical tokenizer hash, ordered training+labels, vectorizer spec, index и output-map; отсутствие сериализатора — точная dependency.

<a id="s-2"></a>
### P25.2 — Cold import того же обученного состояния

**LEARNING_DEPENDENT_CONSUMER: в области задания, даже при0новых Train.** Нужно сравнить обычный output действительно обученного owner до и после загрузки одних и тех же artifacts. Равные нули не доказывают сохранение навыка. Этот подслучай проверяет cold restore, а не публикацию родителя следующего run; эти механизмы и их evidence не смешивать. Новая формулировка не назначает повтор G55/RESTORE-MINIMAL-SET и не материализует недостающий host.

A live trained owner, B cold load его же artifacts; 0 lessons/updates. Exact state identity и обычный output; wrong tokenizer/corpus hash контроль. Equality — восстановление, не новое обучение.

<a id="s-3"></a>
### P25.3 — Cache alias внешней мутации

A cached mutable checkpoint объект, B isolated immutable/deep snapshot на выдаче. Изменить внешний nested field без нового lesson, затем Load/Execute. Cache-off контроль; shallow clone недостаточен.

<a id="s-4"></a>
### P25.4 — Cache mtime/length без content hash

Изменить file content, сохранив длину/mtime в разрешённой изоляции. A cache key length+mtime, B content digest. Измерить фактически loaded bytes и answer; unchanged file контроль.

<a id="s-5"></a>
### P25.5 — Миграция correction key без утраченного SourceLang

A — legacy checkpoint v1 со словарём normalizedRaw→signature загружается в новый языковой namespace с угаданным языком. B — выбранная безопасная политика: импортировать в v2 только entries с независимо сохранённым SourceLang; остальные сохранить как неизменённое read-only legacy evidence и отклонить для новой scoped memory с MissingSourceLanguage. Не пытаться восстановить уже перезаписанную RU/EN запись из одного значения. Контроль v2 RU/EN homograph с двумя entries round-trip; edge v1 содержит фактическую внешнюю provenance, тогда mapping допустим и зафиксирован. Сравнить восстановленные правильные ключи, отказ/coverage, cold outputs и затраты. Это явная предложенная миграция, не утверждение существующего importer; обычный consumer CONDITIONAL до его конкретного контракта.

## Прямые источники

- [06-CHECKPOINT-RESTORE-ADOPTION.md:1](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/06-CHECKPOINT-RESTORE-ADOPTION.md:1>) — Механизм Export и его source boundary.
- [27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md:1](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md:1>) — Механизм cache и его source boundary.
- [26-ARCHITECTURE-BLUEPRINT-TOKENIZER-KNN-STATE-AND-RESTORE-2026-10-16.md:123](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/26-ARCHITECTURE-BLUEPRINT-TOKENIZER-KNN-STATE-AND-RESTORE-2026-10-16.md:123>) — Миграция correction key без утраченного SourceLang.

Это утверждения предоставленных исследований, а не свежий code audit. Все связанные контекстные упоминания находятся в [реестре содержания](../04-source-claim-ledger.md); JSON/script — предоставленные fixtures/proxy, не наши runs. [Исходная постановка](../../83-check-catalog-and-prompts.md) не заменяет эти прямые основания.

## Обязательная запись M для каждого рецепта

Каждый D-рецепт создаёт отдельную **M(D, подслучай)** до/после A и B: sec/update, tokens/s, time-to-quality либо updates0/n/a + operation latency; предметная точность и critical errors; RAM всей группы/VRAM/index/checkpoint/parameters; **model layers total/trainable**, отдельно **Unit/Flow levels и executed depth** (unchanged/n/a с причиной). Считать также calls, retention/recovery, sample/manual effort по механизму. Правило пользы изменения структуры записать заранее. Единицы, формулы, fictional examples и полный формат — [03](../03-metrics-and-reporting.md).

<a id="d01"></a>
## D01 — профиль проверки: C# документация

**Тема обучения (домен):** Документация.

**Задача / язык / направление:** Объяснение поведения C#-метода.

**Вход/gold:** `int Size(string? s) => s is null ? 0 : s.Length;`: null→0, empty→0 по другой ветке, "ab"→2. Неверный ответ смешивает null и empty или выдумывает exception.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [CONDITIONAL: только если тот же Translation checkpoint cache действительно обслуживает этот профиль; RP01 private checkpoint не подставляется]; P25.4 [CONDITIONAL: только при установленном том же Translation cache и его invalidation contract].

**Этот механизм:** Для .1/.2 требуется собственный owner полного состояния: для RP01 C# это private full checkpoint, для generic KNN — его corpus/index/encoder. .3/.4 не приписываются этому owner: их условие указано отдельно, иначе они недоступны в данном профиле.

**M(D01):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d02"></a>
## D02 — профиль проверки: C# структура и классификация

**Тема обучения (домен):** Язык программирования.

**Задача / язык / направление:** C#: распознавание структуры, ролей и обязанностей.

**Вход/gold:** `x => x + 1` и `(x) => x + 1` — одна lambda family с arity1; `(x,y) => x+y` — arity2. `void M(){ if(x==null) return; Save(x); }` требует NULL_CHECK и порядка перед SAVE.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [CONDITIONAL: только если тот же Translation checkpoint cache действительно обслуживает этот профиль; RP01 private checkpoint не подставляется]; P25.4 [CONDITIONAL: только при установленном том же Translation cache и его invalidation contract].

**Этот механизм:** Для .1/.2 требуется собственный owner полного состояния: для RP01 C# это private full checkpoint, для generic KNN — его corpus/index/encoder. .3/.4 не приписываются этому owner: их условие указано отдельно, иначе они недоступны в данном профиле.

**M(D02):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d03"></a>
## D03 — профиль проверки: SQL

**Тема обучения (домен):** Язык программирования.

**Задача / язык / направление:** SQL: смысл запроса, NULL и состав результата.

**Вход/gold:** `LEFT JOIN orders o ON o.customer_id=c.id AND o.paid=1` сохраняет клиента без paid order; перенос `o.paid=1` в WHERE может удалить его. Отдельный закон: `IS NULL` отличается от `= NULL`.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [CONDITIONAL: только если тот же Translation checkpoint cache действительно обслуживает этот профиль; RP01 private checkpoint не подставляется]; P25.4 [CONDITIONAL: только при установленном том же Translation cache и его invalidation contract].

**Этот механизм:** Для .1/.2 требуется собственный owner полного состояния: для RP01 C# это private full checkpoint, для generic KNN — его corpus/index/encoder. .3/.4 не приписываются этому owner: их условие указано отдельно, иначе они недоступны в данном профиле.

**M(D03):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d04"></a>
## D04 — профиль проверки: EN→RU

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** EN→RU.

**Вход/gold:** `Anna follows Boris` → «Анна следует за Борисом»; swapped source меняет роли. `Alice must not open the door` → «Алиса не должна открывать дверь»: сохранить запрет.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [Translation checkpoint cache; достижимость в этом направлении подтверждается caller trace]; P25.4 [Translation cache mtime/length; отдельный контраст от alias]; P25.5 [CONDITIONAL: только SemanticTokenizerUnit._signatureCorrections и его SourceLang checkpoint importer].

**Этот механизм:** Для .1/.2 — обученные maps именно этого TranslationMachine; для .3/.4 — его конкретный checkpoint cache. Сверить output и learned record до/после cold load, не смешивать warm object с новым экземпляром.

**M(D04):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d05"></a>
## D05 — профиль проверки: RU→EN

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** RU→EN.

**Вход/gold:** «Борис не открыл окно» → `Boris did not open the window`; «Анна следует за Борисом» → `Anna follows Boris`. Исходный текст действительно русский.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [Translation checkpoint cache; достижимость в этом направлении подтверждается caller trace]; P25.4 [Translation cache mtime/length; отдельный контраст от alias]; P25.5 [CONDITIONAL: только SemanticTokenizerUnit._signatureCorrections и его SourceLang checkpoint importer].

**Этот механизм:** Для .1/.2 — обученные maps именно этого TranslationMachine; для .3/.4 — его конкретный checkpoint cache. Сверить output и learned record до/после cold load, не смешивать warm object с новым экземпляром.

**M(D05):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d06"></a>
## D06 — профиль проверки: POS, морфология и отношения

**Тема обучения (домен):** Естественный язык: грамматика и морфология; конкретный язык задаёт материал выбранного источника.

**Задача / язык / направление:** Разметка частей речи, морфологических признаков и отношений; язык фиксируется для конкретного материала.

**Вход/gold:** `book a flight` требует book=verb, `read a book` — noun; `ошибка. записать` не должно незаметно наследовать LOG_OP без явного разрешённого контекста. Ивритский homonym требует морфологического gold.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [CONDITIONAL: только если тот же Translation checkpoint cache действительно обслуживает этот профиль; RP01 private checkpoint не подставляется]; P25.4 [CONDITIONAL: только при установленном том же Translation cache и его invalidation contract]; P25.5 [CONDITIONAL: только SemanticTokenizerUnit._signatureCorrections и его SourceLang checkpoint importer].

**Этот механизм:** Для .1/.2 требуется собственный owner полного состояния: для RP01 C# это private full checkpoint, для generic KNN — его corpus/index/encoder. .3/.4 не приписываются этому owner: их условие указано отдельно, иначе они недоступны в данном профиле.

**M(D06):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d07"></a>
## D07 — профиль проверки: Выбор намерения и маршрута

**Тема обучения (домен):** Оркестрация.

**Задача / язык / направление:** Выбор намерения и маршрута обработки запроса.

**Вход/gold:** `объясни код SELECT в строковом литерале C#` относится к объяснению кода, а `составь SQL SELECT` — SQL. Слова SELECT совпадают, действие различно.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [CONDITIONAL: только если тот же Translation checkpoint cache действительно обслуживает этот профиль; RP01 private checkpoint не подставляется]; P25.4 [CONDITIONAL: только при установленном том же Translation cache и его invalidation contract].

**Этот механизм:** Для .1/.2 требуется собственный owner полного состояния: для RP01 C# это private full checkpoint, для generic KNN — его corpus/index/encoder. .3/.4 не приписываются этому owner: их условие указано отдельно, иначе они недоступны в данном профиле.

**M(D07):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d08"></a>
## D08 — профиль проверки: Последовательность действий Unit/Flow

**Тема обучения (домен):** Оркестрация.

**Задача / язык / направление:** Порядок действий и разбор результата потока Unit/Flow.

**Вход/gold:** Flow `validate→transform→persist` обязан отменить persist после failed validation. Две ветви normal и fallback имеют разные typed effects; награда за успех normal не обновляет fallback.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [CONDITIONAL: только если тот же Translation checkpoint cache действительно обслуживает этот профиль; RP01 private checkpoint не подставляется]; P25.4 [CONDITIONAL: только при установленном том же Translation cache и его invalidation contract].

**Этот механизм:** Для .1/.2 требуется собственный owner полного состояния: для RP01 C# это private full checkpoint, для generic KNN — его corpus/index/encoder. .3/.4 не приписываются этому owner: их условие указано отдельно, иначе они недоступны в данном профиле.

**M(D08):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d09"></a>
## D09 — профиль проверки: Ответы по источникам

**Тема обучения (домен):** Предметная тема исходных документов не установлена; в условном примере — факты о статусе проекта Cedar.

**Задача / язык / направление:** Ответ по источникам с датой и подтверждением; GraphRAG — возможный механизм, не тема.

**Вход/gold:** Документы: «Проект Cedar отменён» и «Проект Cedar утверждён» имеют одинаковое начало и противоположный важный хвост. Запрос — действующий статус Cedar с датой и ссылкой на источник.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [CONDITIONAL: только если тот же Translation checkpoint cache действительно обслуживает этот профиль; RP01 private checkpoint не подставляется]; P25.4 [CONDITIONAL: только при установленном том же Translation cache и его invalidation contract].

**Этот механизм:** Для .1/.2 требуется собственный owner полного состояния: для RP01 C# это private full checkpoint, для generic KNN — его corpus/index/encoder. .3/.4 не приписываются этому owner: их условие указано отдельно, иначе они недоступны в данном профиле.

**M(D09):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d10"></a>
## D10 — профиль проверки: TypeScript/Angular, условно

**Тема обучения (домен):** Язык программирования.

**Задача / язык / направление:** TypeScript/Angular: семантика операторов и жизненный цикл, условный профиль.

**Вход/gold:** `value ?? fallback` сохраняет 0/empty, `value || fallback` заменяет их. Angular компонент после destroy не должен продолжать эффекты подписки, если это его объявленный контракт.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [CONDITIONAL: только если тот же Translation checkpoint cache действительно обслуживает этот профиль; RP01 private checkpoint не подставляется]; P25.4 [CONDITIONAL: только при установленном том же Translation cache и его invalidation contract].

**Этот механизм:** Для .1/.2 требуется собственный owner полного состояния: для RP01 C# это private full checkpoint, для generic KNN — его corpus/index/encoder. .3/.4 не приписываются этому owner: их условие указано отдельно, иначе они недоступны в данном профиле.

**M(D10):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d11a"></a>
## D11a — профиль проверки: HE→EN, условно

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** HE→EN, условный профиль.

**Вход/gold:** `דנה לא פתחה את הדלת` → `Dana did not open the door`; `דנה פתחה את הדלת` — без not. Женский референт задан именем и контекстом.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [Translation checkpoint cache; достижимость в этом направлении подтверждается caller trace]; P25.4 [Translation cache mtime/length; отдельный контраст от alias].

**Этот механизм:** Для .1/.2 — обученные maps именно этого TranslationMachine; для .3/.4 — его конкретный checkpoint cache. Сверить output и learned record до/после cold load, не смешивать warm object с новым экземпляром.

**M(D11a):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d11b"></a>
## D11b — профиль проверки: EN→HE, условно

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** EN→HE, условный профиль.

**Вход/gold:** `Dana did not open the door` → `דנה לא פתחה את הדלת`; утвердительный source даёт `דנה פתחה את הדלת`. Требуется иврит, не транслитерация.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [Translation checkpoint cache; достижимость в этом направлении подтверждается caller trace]; P25.4 [Translation cache mtime/length; отдельный контраст от alias].

**Этот механизм:** Для .1/.2 — обученные maps именно этого TranslationMachine; для .3/.4 — его конкретный checkpoint cache. Сверить output и learned record до/после cold load, не смешивать warm object с новым экземпляром.

**M(D11b):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d12a"></a>
## D12a — профиль проверки: ES→EN, условно

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** ES→EN, условный профиль.

**Вход/gold:** `Ana no abrió la puerta` → `Ana did not open the door`; `Ana abrió la puerta` — утвердительно. `año` и `ano` не нормализуются в один смысл.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [Translation checkpoint cache; достижимость в этом направлении подтверждается caller trace]; P25.4 [Translation cache mtime/length; отдельный контраст от alias].

**Этот механизм:** Для .1/.2 — обученные maps именно этого TranslationMachine; для .3/.4 — его конкретный checkpoint cache. Сверить output и learned record до/после cold load, не смешивать warm object с новым экземпляром.

**M(D12a):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d12b"></a>
## D12b — профиль проверки: EN→ES, условно

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** EN→ES, условный профиль.

**Вход/gold:** `Ana did not open the door` → `Ana no abrió la puerta`; пропуск `no` меняет закон. Допустимая формулировка с нулевым субъектом заранее перечисляется.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [Translation checkpoint cache; достижимость в этом направлении подтверждается caller trace]; P25.4 [Translation cache mtime/length; отдельный контраст от alias].

**Этот механизм:** Для .1/.2 — обученные maps именно этого TranslationMachine; для .3/.4 — его конкретный checkpoint cache. Сверить output и learned record до/после cold load, не смешивать warm object с новым экземпляром.

**M(D12b):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

<a id="d13"></a>
## D13 — профиль проверки: Figma→HTML/CSS, условно

**Тема обучения (домен):** Разработка интерфейсов: структура и расположение элементов.

**Задача / язык / направление:** Figma→HTML/CSS: структура и расположение элементов, условный профиль.

**Вход/gold:** Карточка с заголовком над кнопкой: desktop две колонки, mobile одна; label связан с input, важный текст не обрезан. Перестановка DOM parent меняет flex/grid и поведение.

**Применимые подслучаи:** P25.1; P25.2; P25.3 [CONDITIONAL: только если тот же Translation checkpoint cache действительно обслуживает этот профиль; RP01 private checkpoint не подставляется]; P25.4 [CONDITIONAL: только при установленном том же Translation cache и его invalidation contract].

**Этот механизм:** Для .1/.2 требуется собственный owner полного состояния: для RP01 C# это private full checkpoint, для generic KNN — его corpus/index/encoder. .3/.4 не приписываются этому owner: их условие указано отдельно, иначе они недоступны в данном профиле.

**M(D13):** совпадение собственного ответа до/после restore, число доступных learned facts и integrity state; предметная правильность проверяется отдельно, два одинаково неправильных ответа не означают качество; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевой update и неизменный artifact — контроль; повреждённый tokenizer/corpus hash — явный отказ. Для .3 мутация внешнего массива при неизменном файле; для .4 разные bytes при одинаковых mtime/length.

## Не включённые ветви

D14: Применимость именно этого механизма к этим профилям пока не установлена; отсутствие рецепта не является измеренным отрицательным результатом. Нужен тот же owner/schema, а не другой компонент с похожим именем.

<a id="промпт-и-будущие-свидетельства"></a>
## Исторический текст задания и свидетельства

Действующий передаваемый текст находится в [начале карточки](#current-executor-task). Нижний текст сохранён как история; его императивы не назначают новый запуск или продолжение уже закрытого этапа.

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/P25-cold-restore.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

«Самодостаточный вход: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1: AGENTS/CLAUDE и полный стек навыков); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2: роли); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md (применимость и падения). Только прямо назначенный пользователем ID/этап; документальное поручение не разрешает запуск. Полная карточка C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/P25-cold-restore.md обязательна. Модель сессии по умолчанию, без override. Только назначенная строка единого индекса и её готовые host/данные/команды; отсутствующее возвращай автору, не проектируй и не ищи замену. Документное поручение не разрешает runtime. По «заканчивай и переходи» немедленно NEXT без оформления/review/retry/Git прошлого; по «остановить всё» прекрати всю работу, сохрани raw/partial. Отчёт по назначенному пути на русском; полезный измеренный эффект — успех своей оси с ухудшениями рядом. Выполняй только прямо назначенную строку единого индекса и её конкретный рецепт; эта справочная P-карточка сама запуск не назначает. Перед выполнением зафиксируй P25, один указанный подслучай и один профиль проверки (D-секцию). Используй только названный механизм и его A/B; сохрани исходные state/data/tokenizer hashes, одинаковую панель и реальные сырые выходы. Сначала покажи различающее условие, отрицательный контроль и edge, затем предметный результат. Заполни M отдельно для этого профиля проверки, включая model total/trainable layers и Unit/Flow levels до/после. Покажи любой полезный эффект и цену. Если caller/schema/gold отсутствует, назови конкретную зависимость. Не подставляй эталон вместо результата и не расширяй разрешённую программу. Любое фактически измеренное полезное улучшение — успех по этому показателю и домену; рост точности не обязателен. Все остальные изменения и ухудшения покажи рядом; локальный эффект признаётся локальным успехом без обязательных повторных прогонов. Исправление измерителя не называй физическим ускорением. Перед исполнением покажи связь проверяемого участка с обучением или использованием его результата, включая подготовку входов и восстановление. Укажи точный файл/метод оригинального кода, цепочку вызовов, неисправность и способ её исправления. Для нового обучения измеряй реальные updates; для зависимого участка допустим один доказанно обученный state и одинаковые входы до/после исправления без лишнего Train. Исключается только проверка, совсем не связанная с этой цепью. Внеси фактические наблюдения в уже назначенный паспорт. Недостающие проект, входы, метод или команду возвращай автору карточки; не ищи замену и не меняй опыт. При поручении только на проектирование подготовь карточку; при поручении на выполнение продолжай без повторного вопроса об ID, пути или SHA. Неустранимый недостающий вход назови точно.».

Формула/модель алгоритма — **симуляция**, её время не является native cost. Искусственные входы допустимы для настоящего компонента, но его измеряемые state/output должны быть реальными. Обучение памяти и neural update отражаются раздельно. Будущий пакет: raw cases и gold, state deltas, actual caller trace, measurements M и список gained/lost cases; сейчас этих экспериментальных артефактов нет.
