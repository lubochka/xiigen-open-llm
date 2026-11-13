# 20. Довести выученный выбор от промпта до реального получателя

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q11](01-registry.md#work-fix-e2e). В строке указан конкретный выбор владельца. До решения продолжайте первый доступный перенос в очереди. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-E2E20:START -->
## Выбранное тело и переход к запасному пути — уточнение 11 ноября 2026

Применимость — обычный OrchestrationPlanMachine.GeneratePlanAsync и wrapper в области FIX-E2E. Основание: F08/B11 и B12; [план §3.2](audits/2026-11-11/repair-plan.md), [продолжение аудита](audits/2026-11-11/followup/02-audit-and-repair-addendum.md). Это требование будущей приёмки, не новый эксперимент.

Проверять связку **режим → FlowRef → активная редакция → PairId → полное chosen body → настоящий получатель**. Правильный FlowRef при старом, чужом или пустом теле не является успешной передачей. После A→B1→B2 одного маршрута требуются удержание совместимых B1/B2, тело B и сохранение независимого C; после cold restore — те же identity и ответ. Предлагаемая active revision карточки15 пока не является существующим API. Конфликтующий повтор PairId отсекается до изменения weights/history/revision.

Отказ Mode1 не доказывает безопасность Mode2. Сохранить причину перехода, actual fallback и конечный output. TryGround уже проверяет положительные scores; у store/Mode2/Mode3 собственная граница пустых и zero-overlap соседей. Пустой/пунктуационный или нерелевантный запрос не должен получать смысловую атрибуцию чужому уроку только из-за PairId tie.

**Сохранить FIX-P118:** current RenderMode2Plan добавляет chosen body при нормализованном точном совпадении prompt. Утверждение «Mode2 вообще не читает learned body» устарело; exact recall не доказывает обобщение. Проверки exact, reordered tokens, partial overlap, no overlap и empty различают эти пути. Expected задаётся независимо от candidate serializer.

Имеющиеся состояния и назначенный расход сохраняются. Новый A→B Train учитывается по отдельному бюджету §3.2; структурная проверка с Train=0 не выдаётся за удержание после обучения. История, active state и исполнение внешнего специалиста принимаются раздельно.
<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-E2E20:END -->


<!-- SNAPSHOT-AUDIT-2026-11-09-F37A:START -->
## Уточнение аудита — 9 ноября 2026

**Согласование с поздним решением Q11 и локальными приёмками.** Срез `f37a8faa`; [аудит F06](audits/2026-11-09/2026-11-09-experiment-transfer-audit.md), [A06](audits/2026-11-09/repair-plan.md). Это уточнение постановки, не разрешение продуктового исполнения, ранее поставленного на удержание.

Реестр уже содержит архитектурное решение: shared handoff/request contracts размещаются в `00.Core`, каталог остаётся в проекте оркестратора. Не спрашивать заново, в какой сборке они могут жить, и не создавать циклы Wiki→Orchestrator либо ModelServer→Orchestrator. Предложенные договоры/каталог/caller пока не становятся существующими от упоминания в плане.

Открытый вопрос владельца Q11 — делать ли сейчас новое продуктовое звено через перечисленные области либо отложить; также требуется явный договор о двух статических селекторах до learned path. Текущий аудит не снимает это удержание. Оно касается зависимой E2E-leg и не отменяет независимые принятые переносы.

Для каждой leg заполнить producer→фактический payload→learner→изменённое состояние→обычный consumer→fresh-load. Наличие coordinator Q12, R34-FB adapter, правильного FlowRef или счётчика доставок не доказывает включённый пользовательский маршрут. Catalog lookup назвать детерминированным binding; обучение приписывать только реально изменяемой памяти выбора.

Positive читает полный learned payload у назначенного receiver; negative с чужим телом при прежнем ID должен отказать. После load новый объект/процесс читает без Train, и назван штатный API. Reflection-reader опыта не закрывает native persistence. Для повторной доставки явно указать lesson identity, момент публикации/ack и recovery; наличие поля ID не доказывает идемпотентность. Все ещё не выполненные проверки помечаются UNRUN.
<!-- SNAPSHOT-AUDIT-2026-11-09-F37A:END -->


**Расширение предметного покрытия, 4 ноября:** [158](codex-158-learning-sets-transfer-coverage-2026-11-04.md) назначает реальные исходники OR-ROUTE-INTENT/M004 и отдельные C#/SQL/Angular/переводческие панели. Существующие Q1–Q6, три темы и бюджет этого плана не меняются. Дополнительная панель требует собственного подготовленного экзамена и действительно зарегистрированных получателей; отсутствующий receiver означает NOT_COVERED, а не ответ из gold. Атомарный M004 сохраняет clarify для нескольких действий. Новый порядокP127 не переносить в него подменой expected. Исполнение нового расширения пока не назначено.

## Разбор учебного теста и обязательное исполнение проверок — 3 ноября 2026

Правильный маршрут, правильный ответ и обучаемость выбранного получателя проверяются раздельно. Нельзя исправить отсутствие нужных уроков фиксированным роутом/ответом в dispatcher. Сборка ordinary consumer и e2e тестов обязательна при разрешённой реализации; отсутствие запуска не выдавать за покрытую цепь.

Использовать [договор45](../experimets/45-learning-claim-and-native-acceptance-contract-2026-11-03.md) и его пакет эскалации архитектору; основной стек120 дополнить experiment-design/test-integrity/test-failure-triage/bug-to-tests. Перед приёмкой назвать затронутые проекты и тестовые фильтры; при разрешённой реализации собрать их, выполнить проверки на конечном SHA/дельте и сверить discovery. При compile/setup/assert failure сохранить команду, полный log, actual DLL, expected/actual и вернуть точный вопрос архитектору. Дождаться исходного handle; не завершать задачу с UNRUN, не исключать тест/источник и не ослаблять assertion ради зелёного вывода. Исследовательская правка этой карточки запуск не разрешает; STOP/NEXT пользователя немедленны.

<!-- POST113-CURRENT-2026-11-02:START -->
## После P113 — адрес урока, прочитанное содержание и обычный caller

End-to-end подтверждение связывает prompt/domain→фактический lesson→authorised owner/version→learning state→ordinary consumer→restore. P111 selected metadata не закрывает реальный feedback ingress; P112 claimed hash не подтверждает обучаемый Content; P113 экспериментальный B не доказывает изменение native caller. Positive доносит собственный урок до назначенного ответа. Negative чужой payload с прежним ID/hash либо дополнительный owner при тех же counts обязан нарушить приёмку.

Source mapping OPEN фиксируется адресно, не закрывается новым одноимённым mock/helper. Reference, направление, topic и авторизация owner остаются прежними; бюджет native E2E назначается отдельно от completed PRIMARY.

Основание: [адресный план 135](codex-135-post113-addressed-repair-plan-2026-11-02.md). Срез `21007e25ec3f6f6634dd8733fc8631ae267e518c`, 2 ноября 2026. Этот блок уточняет указанные условия; исторические результаты, собственные данные, единственный фактор, учебный бюджет и очередь сохраняются. Порча копии квитанции не назначает новое обучение или дополнительное чтение модели. Нижние датированные указания применяются в своей исходной области; новые требования не делают старые ненаблюдённые поля измеренными.
<!-- POST113-CURRENT-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски FLOW/ROUTE:** Правильная строка Unit/FlowRef не заменяет исполненный результат: typed refs→вызванные nodes, learned edge→Retry/Stop, graph order→sink. Положительный контроль использует обученный payload, отрицательный выявляет его потерю/чужое тело; trivial baseline и exact recall показаны отдельно. На названном маршрутизаторе/каталоге проверить writer→обученная память→ordinary route/payload и собственные positive/negative строки; label без выбранного payload не закрывает маршрут. Exact recall, простой baseline и предметная тема раздельно. Дополнительный dynamic same-functions фактор допустим только если назначен паспортом и проверяет скорость native Train.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**План должен дойти до исполнения со своим содержимым.** P104 получил payload непервого урока из обучающего корпуса, P107 сравнил first/majority без разногласий. Для этого E2E назначить trace ordinary route→selected lesson→returned payload→исполненные действия. Host lookup эталона не становится learned output.

**Положительный пример:** выбранный второй законный lesson возвращает свои шаги, и наблюдаемый порядок действий им соответствует. **Отрицательный:** сохранить правильный route ID, но подставить шаги первого соседа; приёмка должна упасть. Проверка только количества шагов или invocation count недостаточна. Постоянный отказ не получает успех от безопасности без отдельного счёта полезных законных ответов.

Отделить завершение процесса, валидность измерителя, результат гипотезы и product transfer. False control обязан менять технический статус, а A=B при валидном измерении остаётся отрицательным результатом. Fresh restore требует новой единицы и чтения файлов; стоимость off-panel/control вызовов включается до выдачи вызова.

Основание: [аудит 123](codex-123-post107-transfer-and-experiment-audit-2026-11-02.md), [план 124](codex-124-post107-addressed-repair-plan-2026-11-02.md), [контекст 41](../experimets/codex-41-post107-executor-checks-2026-11-02.md). Это уточнение приёмки будущего переноса; прежние результаты и назначенная доза сохраняются.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Проверять один фактически исполненный путь: prompt → обученный выбор → выбранные model/profile → actual результат получателя → адрес записи нового урока → чтение после restore. Подстановка сохранённых logits/ответа вместо результата выбранной модели не доказывает этот путь: именно так ограничен новый P08 product test. Исторический oracle читается только для сравнения, а не подаётся как ответ следующему звену.

Для FIX-E2E исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Доказывать вызов реального получателя после выбора, а не наличие созданного constructor/bridge. Контрпример P08: A/B TextGenerator создан, но sampled ответы получены вне него. Для этой карточки записать выбранную модель/профиль, реально вызванный метод потребителя, данные ответа и получателя нового урока; альтернативный маршрут и restore проверяются отдельно. Подмена ответа ближайшей строкой/таблицей gold не проходит E2E даже при правильном выборе домена.

Применимость: FIX-E2E. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Проверять фактическую конфигурацию модели после маршрутизации: domain, checkpoint, tokenizer, функции KNN, trainable recipient и параметры обучения. Контрпример P12: direct Trainer Warmup4 отказывает, но фабрика IncrementalTrainingSession уже задаёт Warmup0; дефект одного вызова не доказывает отказ всего маршрута. После урока проверять сохранение выбранной и неизменность чужих моделей; hash без полного snapshot не является restore.

Применимость: FIX-E2E. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

**Дополнение P07, 27 октября:** для выбранного neural/BPE получателя проверить связку domain→model/checkpoint→tokenizer hash→framing в уже назначенном учебном E2E; response targets после restore относятся к тому же профилю. Использовать общий receipt [83](83-learned-bpe-boundary-transfer-addendum.md), без повторного Train/Forward ради этого дополнения. У KNN-only получателя, не использующего BPE carrier, условие неприменимо. P07 не квалифицирует router и не заменяет собственные TRAIN/EXAM и отрицательные route-контроли этого плана.

<!-- BEGIN audit-79-followup -->
## Уточнение после последующих исправлений и P03–P05

В E2E binding проверять не только выбранный domain/model/functions, но и consumer contract: prompt framing, tokenizer/EOS/stop и полный выход. Иначе правильный маршрут может получить ложную регрессию из-за чужого reader. Для KDP сейчас доступна защита отказом capture, не полное восстановление configured binding; после отказа не подставлять пустой каталог или другую модель. Доказательства маршрута, learned state и правильного ответа остаются раздельными.

Основания — [79](79-followup-transfers-and-learning-audit-2026-10-27.md), адресный план — [80](80-followup-repair-plan-2026-10-27.md). Это изменение требований и статуса по прочитанным источникам; неприменённые проверки не объявляются пройденными. Собственные бюджеты и исторические результаты карточки сохраняются.
<!-- END audit-79-followup -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-E2E

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** FLOW/ROUTE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**K — владение допущенным состоянием и восстановление.** Показать, какой объект хранит допущенные параметры, примеры, конфигурацию или ключ кэша; интерфейс IReadOnlyList сам по себе не доказывает неизменность. Контроль после создания/допуска: изменение исходного caller-owned массива или внешнего представления не меняет захваченную запись незаметно. Законное обновление/смена model identity вызывает предусмотренное обновление или инвалидирование кэша, а не запрет обучения. Если reader сохраняется/восстанавливается, сравнить поведение обычного consumer по сохранённым данным и настройкам: reconstruction с default не является эквивалентным restore opt-in режима. Ошибка из KDP: catalogue был проверен, затем изменён через возвращённый массив; schema не содержала catalogue. Для этой карточки проверять её собственный carrier, не добавлять чужой KNN-механизм ради теста. Если изменяемого carrier/сохранения нет, отметить это явно в evidence и не выдумывать лишний runtime.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Адресный контракт каталога, 27 октября 2026.** Для Q1–Q6 проверить одну непрерывную цепочку: learned decision с действительным допуском → server-owned binding → actual receiver invocation → caller-visible payload → тот же смысл после fresh load. Правильный predicted FlowRef/label без допуска или с пустым/чужим телом не считается правильным ответом. Из уже полученных receipts построить отрицательные копии: label правильный, invocation отсутствует; receiver ID правильный, payload пуст; confidence высока, binding недопустим. Сравнитель обязан разделить эти три причины, не вызывать модель и не «дорисовать» dispatch.

Restore использует только реально записанные state artifacts; fixture/gold доступны scorer, но не восстановителю пропавшего body/answer. Для memory-only handler AdamW неприменим; для neural handler weights identity не утверждает optimizer/RNG continuation. Это конкретизация прежних двенадцати Q1–Q6 чтений и typed handler receipts, не новая тема, route или дополнительное обучение. KDP/KDR — отдельные [FIX-KDP/E2E-KDR74](74-knn-domain-constructor-transfer.md), не подмена существующего E2E уже выбранным profile hint.

**Дельта 26 октября по 141–143:** [сравнитель 63](63-paired-learning-effect-and-factor-contrasts.md) дополняет происхождение готового downstream receipt: metric axis, arm orientation, pair panel, window и observed sign counts. Устойчивость зависит от парной разницы, а не только размаха уровня. Ни 121 correlated checkpoints, ни teacher-forced fraction не заменяют `answer_correct`, `routing_correct` или реальный вызов receiver.143 различает имя функции и правило внутри C# темы; это не новые предметные домены и не основание hardcoded маршрута по строке Sign. Q1–Q6, budgets и native handler calls сохраняются; E2E не запускает 141–143 ради заполнения новых полей. Отсутствующая траектория обозначается `not_measured`.

## Уточнение 23 октября: новые kernels и цена ошибочного выбора

[FIX-FBC54](54-frozen-base-check-scope.md) и [FIX-LFT55](55-frozen-linear-transpose-cache.md) не обучают выбор домена. Их завершённые native receipts подключаются к прежнему E2E как evidence downstream handler; новый learned router, synthetic keyword shortcut и дополнительный H512 fit не назначаются. На уже запланированных Q1–Q6 до/после restart сохранить раздельно `routing_correct`, `domain_invoked`, `answer_correct`, `deferred`, `wrong_dispatch`; дополнительного набора запросов или training budget эта дельта не добавляет.

Отрицательный balanced WM02 показывает, почему `deferred↓` само по себе неуспех: при прежних5/8correct selected неправильные выбранные маршруты выросли0→2. Существующие unsupported/reject контроли должны остаться assertions, а не удаляться для прохождения теста. Ошибка selector и ошибка ответа выбранного handler чинятся в своих существующих получателях; правильный dispatch не доказывает правильный ответ и наоборот. Источники и границы — [56](56-measured-delta-and-existing-coverage.md). Уже описанные FIX-IL/SC/SVD/FSR и resource deltas прежнего каталога там связаны без дублирования планов.

ID: **FIX-E2E**. Статус: **план интеграционной проверки, результат ещё не измерен**. Основания — [FIX-WT / WIKI-TAXONOMY-01](14-wiki-routing-taxonomy.md) и [FIX-RET / RETENTION-M1-01](15-prompt-flow-retention.md). Этот документ не добавляет третий «успешный router»: он проверяет, доходит ли польза двух измеренных механизмов до настоящего потребителя, восстановления и ответа. Выполнение — только после решения о внедрении; сейчас нет новых Train, test, inference или product правок.

## 1. Что уже есть, а какой переход отсутствует

WT доказал обучение **типу запроса внутри Wiki**, RET — удержание **выученного плана**. Ни один опыт не доказал общий выбор всех предметных тем.

[TEST-E22](27-predicate-dataset-router-learning-coverage.md) добавляет отдельное покрытие обученного выбора datasets по C#-исходнику. Его выход `bool[6]` — маска учебных datasets, а не предметный домен, `FlowRef` или решение о вызове получателя. Локальный рост macro-F1 E22 не закрывает требуемые здесь prompt selection, dispatch и readout; не вводить неявный adapter «dataset label → domain route». Основания этого E2E остаются [WT](14-wiki-routing-taxonomy.md) и [RET](15-prompt-flow-retention.md), без дополнительного runtime-теста или нового router из E22.

Актуальная цепочка assistant:
`LogicalOrchestratorAssistant.RunAsync → PrepareRunRequest → AdaptiveLogicalOrchestrator.RunAsync`. Если запрос не получил explicit scenario, adaptive путь выполняет `CompilePromptAsync → ClassifyDomainAsync → RouteAsync → ExecuteBranchesAsync → ConsumeEdgeTargetHandoffAsync`. Реальный flow вызывается через `ExecuteBindingAsync`, а не только записывается в отчёт. [Assistant:585–669](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs#L585), [Adaptive:353–429](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs#L353).

Есть четыре конкретных разрыва:

1. `PrepareRunRequest` сегодня вызывает статический `new ProjectWikiRequestRouterUnit().Route` ещё до adaptive пути. `IsProjectWikiTrainingRoute` не перечисляет обе новые WT-метки. Получается возможный обход обученного решения. [Assistant:624–659,1215–1226](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs#L624).
2. `ProjectWikiTrainingOrchestrator.RunCoreAsync` снова вызывает свой статический `_router.Route(prompt)`, хотя dispatch в два новых training pipeline уже существует. [RunCoreAsync:205–223](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs#L205).
3. Настоящий learned `ProjectWikiRequestRoutingUnit.ExecuteAsync` существует, но он читает product gate. Исторический WT4/6 этот gate не прошёл; вручную проставлять допуск нельзя. [Unit:405–447](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L405).
4. RET заканчивается `GeneratePlanAsync` и текстом. `OrchestrationPlanOrchestrator.RunAsync` возвращает план; он не выполняет найденный `FlowRef`. `MlGenerated=true` и verdict генерации плана не доказывают ни корректный домен, ни исполнение. [Machine:99–118](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L99), [Orchestrator:24–86](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/42.ProjectWikiDomain/OrchestrationPlanOrchestrator.cs#L24).

`ClassifyDomainAsync` может взять loaded learned unit из registry при `UseLearnedUnitWhenAvailable`, иначе использует `CompiledDomainClassifierUnit` ([Adaptive:693–720](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs#L693)). Этот существующий алгоритм не заменяется неподтверждённым новым классификатором. Его enum `CODE_Q/BUG/PLAN/ARCH/REFACTOR` обозначает виды работы в текущем code pipeline; это не доказательство выбора темы «перевод» против «документация». Комментарий E18 в коде не используется здесь как первичный положительный результат.

## 2. Выбранная архитектура: повторно использовать механизм RET

Для проверки широкого маршрута используется отдельный профиль **того же** `OrchestrationPlanMachine/PromptFlowGroundingModel`. Его задача обучения — оркестрация D07: выучить связь пользовательской формулировки с идентификатором зарегистрированного предметного обработчика. Мы не вводим другой KNN, новый tokenizer, произвольные k/пороги или новый критерий обучения.

После `TryGround` и `FindLearnedPair` нужен **типизированный результат**. Новый контракт, которого сейчас нет: `LearnedDomainHandoff(RunId, SourceCheckpointId, PairId, FlowRef, TargetDomainTag, TargetBindingId, DecisionStatus)`. Формируется внутри того же branch `GeneratePlanAsync`, где уже доступны grounding и найденный pair; не парсить `FLOW_REF` из готового текста. Аддитивное поле результата не меняет score/выбор.

`TargetDomainTag/TargetBindingId` берутся из фиксированного каталога **по уже выученному FlowRef**, не по словам prompt, substring UnitId или ожидаемой метке теста. Это связывание ID с существующим обработчиком. Каталог — новый `15.OrchestratorModel/Logical/LearnedDomainTargetCatalog.cs`; handoff consumer — новый `LearnedPromptDomainDispatchFlow.cs`, реализующий `ILogicalFlow<LogicalOrchestratorRequest>`. Оба — **планируемые файлы**. Решение темы остаётся у выученной памяти.

| Выученный FlowRef | Предметный получатель | Настоящая вызываемая операция |
|---|---|---|
| `TOPIC-ORCHESTRATION` | отдельный восстановленный RET learner | `OrchestrationPlanMachine.GetOrchestratorAsync → OrchestrationPlanOrchestrator.RunAsync` |
| `TOPIC-TRANSLATION` | восстановленный `TranslationMachine` | `GetOrchestratorAsync → InlineTranslationOrchestrator.RunAsync → TranslationRoutingFlow.ExecuteAsync` |
| `TOPIC-WIKI` | native learned Wiki routing unit | `ProjectWikiRequestRoutingUnit.ExecuteAsync`, затем только допустимый выбранный handler |

Реальный translation путь использует `source_language`, `target_language`, `topic_domain` из request metadata и `UserRequest` как переводимый текст; это установлено по [TranslationMachine:653–660,818–864](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/27.TranslationDomain/TranslationMachine.cs#L653). Поэтому новый контракт в планируемом `15.OrchestratorModel/Logical/LearnedPromptDomainContracts.cs` — `LearnedPromptDomainRequest(string UserPrompt, string TaskInput, IReadOnlyDictionary<string,string> TaskMetadata)`. `UserPrompt` выбирает тему, `TaskInput` несёт содержимое задачи. Gold/expected topic, desired binding и caller-chosen checkpoint в DTO отсутствуют. Каталог не извлекает «текст после Translate» эвристикой. Результат предметного обработчика возвращается как actual domain result, не заменяется заготовленным chosen из selector.

Перед любым target call consumer проверяет: решение learned и Selected; `grounding.FlowRef == learnedPair.FlowRef == learnedPair.Chosen.FlowRef`; binding существует; checkpoint/подпрофиль соответствует; контракт полезной нагрузки подходит выбранному обработчику. Нет совпадения — typed deferred/error, **ноль** target calls. Это необходимая граница интеграции. Поздний завершённый IDENTITY-M1-01 теперь имеет положительный reader-результат и отдельный [план FIX-IM48](48-learned-route-body-identity.md); он подтверждает исправление связи header/body, но ещё не выполнение внешнего получателя. Неизвестная тема, отсутствие обученного state или low confidence не переводятся в первый доступный обработчик.

### Подключение к assistant и Wiki

В `LogicalOrchestratorAssistant` зарегистрировать новый flow в существующем `InMemoryLogicalFlowRegistry.Register<LogicalOrchestratorRequest>` и добавить явную конструкцию `CreateWithLearnedDomainDispatch` для принятого checkpoint/каталога. Это новый factory, не существующий API. Новый overload `RunAsync(LearnedPromptDomainRequest request, CancellationToken ct)` передаёт `UserPrompt` в имеющийся `LogicalOrchestratorRequest.UserRequest`, а typed payload — под одним ключом `learned_domain_payload` его Metadata. При этой конструкции `PrepareRunRequest` выбирает только контейнерный scenario `learned-prompt-domain-dispatch`, не целевую тему; static Wiki pre-route пропускается. Trace маркируется `scenario_selection_mode=learned-topic`, поскольку сценарий контейнера не предоставлен пользователем. Домен внутри контейнера выбирает исключительно восстановленный RET learner. Explicit scenario старого overload сохраняет своё исходное значение и не считается тестом выбора темы. Исторический default constructor не получает скрытого глобального переключения.

Для Wiki вместо второго semantic выбора по строке добавить в `ProjectWikiTrainingOrchestrator` новый `RunRoutedAsync(ProjectWikiRequestRoutingDecision decision, string prompt, string artifactRoot, string batchId, string serverBaseUrl, string learningDataRoot, CancellationToken ct)`; он выполняет проверенный typed dispatch существующих ветвей с теми же фактическими аргументами, что [текущий RunAsync:107–122](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs#L107). Продуктовые допуски на обучение не возникают из одного routing decision. `ProjectWikiTrainingFlowAdapter` передаёт один retained decision. Он не пересчитывается static router. Добавление двух WT-меток в старую whitelist не заменяет эту связность. Каждый вызов содержит реальный route source/checkpoint/target identity.

HTTP-шов также отсутствует: в [25.ModelServer/Program.cs:641–676](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/25.ModelServer/Program.cs#L641) `ConfigureLocalLoopbackRoutesCore` регистрирует DesignConversion/ContinuousLearning/CodeDocumentation handlers, но этот learned dispatch там не подключён. После квалификации fixture добавить планируемый `25.ModelServer/Http/LearnedPromptDomainEndpoint.cs` с `MapPost("/api/orchestration/dispatch", ...)`, принимающим ровно DTO выше и вызывающим новый assistant overload. Отдельная server-owned DI-композиция передаёт восстановленные selector, catalog и recipient instances; caller не назначает checkpoints/target. Подключение — в том же `ConfigureLocalLoopbackRoutesCore`, только когда эта композиция явно задана. E2E поднимает настоящий loopback host и отправляет DTO через HTTP до native recipient; прямой вызов нового helper один не закрывает API-путь. Это будущий локальный маршрут, не уже доступный endpoint и не план внешней публикации.

Продвижение outer selector и inner Wiki — независимые допуски. Outer может правильно отправить запрос в Wiki, а inner честно вернуть NeedsSecondPass. В отчёте это `correct_topic=true, domain_result=deferred`, а не полноценный успех ответа.

## 3. Минимальная будущая фикстура без поиска исполнителем

Это **новая интеграционная фикстура по измеренному механизму RET**, её показатели ещё неизвестны. Она не объявляется отдельным успешным экспериментом и не разрешает интеграцию сама по себе.

Файлы после разрешения внедрения: `Tests/Fixtures/ExperimentFixes/v1/E2E/topics.train.json`, `topics.exam.json`, `target-catalog.json`, `topic-s0.checkpoint.json`, `topic-s1.checkpoint.json`, `translation.checkpoint.json`, `provenance.json`. Producer: новый `Tests/Fixtures/ExperimentFixes/v1/E2E/PromptDomainFixtureProducer.cs`, используемый общей test fixture; ручной выбор learner/host/path запрещён. Текущая задача producer не создаёт и не запускает. Новые результаты — `artifacts/fbe/FIX-E2E/attempt-0001/`; журналы тестов — `tests/`, расходы и квитанции реальных вызовов — `resources/`.

Все topic-уроки имеют `Mode1LearnedFlowGrounding`, `ExpectedRouteMode=LearnedFlowGrounding`, `ExpectedBoundarySource=Classifier`, `ExpectedMlGenerated=true`, `ExpectedIntentKind=ImplementationExecutionRequest`, `IsHeldOut=false`. `Chosen.FlowRef` совпадает с FlowRef; `Chosen.Title="Endpoint dispatch"`; SourcePath — `fixture://FIX-E2E/<PairId>`; SourceBundleId — `FIX-E2E-topics-v1`. Каждый вызов `TrainOrchestrationBatchAsync` получает один pair, `BatchId="FIX-E2E/" + PairId`. Все литералы для `Chosen.PlanText` и каталога заданы ниже. Для T1 RejectedText равен Wiki-паспорту, для T2 — orchestration-паспорту, для T3/T4 — translation-паспорту. Это лабораторная размеченная фикстура orchestration, не выданный за внешнюю генерацию production DPO.

| FlowRef | DomainTag | TargetBindingId | Точный Chosen.PlanText |
|---|---|---|---|
| TOPIC-ORCHESTRATION | orchestration | fbe-orchestration-ret-v1 | `domain=orchestration;binding=fbe-orchestration-ret-v1` |
| TOPIC-TRANSLATION | translation | fbe-translation-v1 | `domain=translation;binding=fbe-translation-v1` |
| TOPIC-WIKI | project-wiki | fbe-wiki-wt-v1 | `domain=project-wiki;binding=fbe-wiki-wt-v1` |

Нативный `HumanIntentBoundaryClassifier` сохраняется. Для этих точных формулировок его правила не выбирают explanation/migration/review/testing/explicit-flow ветви и оставляют `ImplementationExecutionRequest`; это вывод из [Classifier:20–105](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/42.ProjectWikiDomain/HumanIntentBoundaryClassifier.cs#L20), не измеренный E2E. Нельзя дописать в classifier распознавание трёх предметных тем: исчезновение learned selection при ablation обязательно.

| Порядок / PairId | Точный TRAIN prompt | FlowRef |
|---|---|---|
| 1 / E2E-T1 | `translate a Russian greeting into English` | TOPIC-TRANSLATION |
| 2 / E2E-T2 | `train system boundary mapper using project evidence` | TOPIC-WIKI |
| 3 / E2E-T3 | `select an approval ledger execution plan` | TOPIC-ORCHESTRATION |
| 4 / E2E-T4 | `choose a payment consent workflow` | TOPIC-ORCHESTRATION |

Первые три урока создают common before; четвёртый — согласованный парафраз того же плана. Тела T3/T4 одинаковы. Не смешивать сюда обучение переводу: в selector меняется только связь prompt→topic.

| EXAM ID | Точный prompt, которого нет в TRAIN | Ожидаемая тема | TaskInput |
|---|---|---|---|
| E2E-Q1 | `select an approval ledger execution plan now` | orchestration | RET RQ1: `validate payment approval ledger audit` |
| E2E-Q2 | `choose a payment consent workflow now` | orchestration | RET RQ2: `check invoice consent journal review` |
| E2E-Q3 | `translate a Russian greeting into English please` | translation | исходный a_side `product-b001-001` |
| E2E-Q4 | `train system boundary mapper using project evidence now` | Wiki | WT Q2: `Train system boundary mapper on API project evidence.` |
| E2E-Q5 | `weather tomorrow near the coast` | unsupported/deferred | `weather tomorrow near the coast` |
| E2E-Q6 | `purchase airline tickets for tomorrow` | unsupported/deferred | `purchase airline tickets for tomorrow` |

Это минимальные изменённые формулировки, не доказательство языкового обобщения. Ожидаемая тема хранится только у scorer; ни TaskInput, ни каталог не выбирают topic вместо learner. Строки Q5/Q6 не учат путешествиям/погоде, они проверяют отрицательный выбор в теме оркестрации.

Конечный budget producer: **4 Train вызова / 4 урока**, 6 запросов после первых трёх уроков и 6 после четвёртого; один save/fresh load и ещё 6 readout, без нового Train. Затем 6 readout независимой абляции с пустой learned memory. Итого **24 outer readout** для квалификации; получатели в этой подготовке не вызываются. Не подбирать дополнительные формулировки или число повторов до PASS. Требуемый начальный сигнал: Q1/Q3/Q4 уже правильно выбираются после подготовки; если его нет — `fixture_not_qualified`, dependent E2E не исполнять, записать фактическое состояние. После T4 — Q1–Q4 правильные темы, Q5/Q6 без target call; state ablation должна убрать learned selection. Непрохождение этой новой фикстуры не отменяет измеренных WT/RET.

После квалификации общий E2E fixture выполняет один HTTP-набор Q1–Q6 и один такой же набор после остановки/нового создания host: ещё **12 outer readout**, максимум 8 native receiver вызовов (4 orchestration, 2 translation, 2 Wiki-deferred). Три дополнительных повреждённых typed handoff — unknown binding, несовпадение pair/grounding, неверная payload-schema — проверяются непосредственно у consumer, без Train/Generate и без target call. Тесты ниже разделяют одну неизменяемую квитанцию этой фиксированной последовательности; каждый assertion не запускает producer заново. Число повторов не растёт автоматически. Два native translation readout для отдельной подготовки receiver учитываются отдельно.

### Дочерние состояния

- Orchestration receiver — S0 и продолжение RET из [15](15-prompt-flow-retention.md); другой экземпляр памяти, не outer selector. RQ1/RQ2 проверяются по полному телу LAB-A.
- Wiki receiver — реальный сохранённый B/after-snapshot WT. Исторический `GatePassed=false` сохраняется. Реальный вызов routing unit должен отложить решение, а `RunRoutedAsync` не должен запускать training pipeline. Положительное выполнение training target станет отдельной ступенью **только после** настоящего native held-out gate, не через подклейку gate=true.
- Translation receiver — существующие строки `product-b001-001` и `product-b001-002` из [product-prompt-b001-all-directions-train-ready.jsonl](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/27.TranslationDomain/TrainingData/product-prompt-b001-all-directions-train-ready.jsonl), обе RU_EN. Если принятого совместимого state ещё нет, producer **один раз** делает `TranslationStructuredBatchBuilder.BuildGeneralBatch("FIX-E2E-TR-v1", twoRows, heldOut:false, TranslationSignatureMode.ExactPair)` и один `TranslationMachine.TrainStructuredBatchAsync` (6 native unit examples), затем настоящий SaveCheckpointAsync/fresh LoadCheckpointAsync. Flat `TrainBatchAsync` не использовать: в source это счётчик `+0.01`, не unit learning ([TranslationMachine:51–86](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/27.TranslationDomain/TranslationMachine.cs#L51)). Один before и один after native ответ на a_side001 квалифицируют сохранённое acquisition; exact lookup явно помечается `memory_reuse`, не обобщение. Если реальный ответ не приобретён, translation leg блокируется и не заменяется мок-ответом. Metadata: source_language=ru, target_language=en, topic_domain=general. Новый результат этой подготовки пока неизвестен.

Так фиксированы механизм, данные, место, budget, вызовы и поведение при неусвоении. Ни подбор нового donor, ни поиск нового набора исполнителю не делегируются. Существующие исходные datasets и результаты не меняются; новые local fixture checkpoints не становятся каноническими продуктами.

## 4. Что именно измеряет E2E

| Уровень | Проверка | Что не является её заменой |
|---|---|---|
| Обучение selector | Ненулевые поштучные before и after, сохранение Q1 и приобретение Q2 | Наличие весов, рост confidence |
| Тема | learned FlowRef связывается с нужным DomainTag | Равенство enum CODE_Q или наличие слова “translate” |
| Получатель | Фактический native вызов ровно выбранного instance/checkpoint | Запись target ID в metadata |
| Доставка | TaskInput, языки и исходный prompt не искажены; gold отсутствует | Заранее выбранный scenario |
| Результат | RET: полное тело; Translation: реальный output; Wiki: честный deferred без Train | Общее IsSuccess/ORCHESTRATED |
| Изоляция | Чужие доменные counters и состояние не изменились | Только правильный общий процент |
| Восстановление | Новый процесс/экземпляры, те же ответы без Train | Пересоздать модель и переучить |
| Необученный/отрицательный случай | Нет learned target и нет side effect | Выдать первый/keyword fallback как trained |

На каждом запросе сохраняются UserPrompt, task payload без gold, actual topic, FlowRef, binding, receiver checkpoint ID, route source, actual invocation, raw result, expected и оценка. Ожидаемые ответы присутствуют только в отчёте/scorer, а не в decision metadata. Успех любого полезного показателя по теме фиксируется даже без роста accuracy; регрессии показываются в той же таблице. `routing_correct`, `domain_invoked`, `answer_correct`, `deferred`, `wrong_dispatch` — разные поля.

## 5. Тесты и порядок внедрения

Новый файл: `Tests/ExperimentBasedPromptDomainE2ETests.cs`; имена ниже **запланированы**.

1. `LearnedTopicDispatch_AcquiresAndRetainsTopicSelection` — ровно producer выше и сохранение old/new запросов; без семантических keyword shortcuts.
2. `LearnedTopicDispatch_InvokesCorrectNativeDomain` — настоящие restored orchestration/translation recipients; итоговый ответ обязан происходить из выбранной модели.
3. `LearnedWikiDispatch_RetainsOneDecisionAndHonorsGate` — WT classifier вызывается один раз, static router не вызывает второй выбор; gate=false не запускает downstream Train.
4. `LearnedTopicDispatch_RestartNeedsNoRetraining` — поднять те же состояния fresh; counters Train не растут, поштучные outputs совпадают.
5. `LearnedTopicDispatch_UnknownOrConflictingTargetDoesNotExecute` — Q5/Q6 и несовпадение learned pair/grounding/binding завершаются до target.
6. `LearnedTopicDispatch_ExplicitScenarioIsNotCountedAsPromptRouting` — compatibility path остаётся отдельным transport test, не результатом обучения.

Существующие адреса регрессий: `Tests/ProjectWikiDomainTests.cs` (`LogicalOrchestratorAssistant_AdaptivelyRoutesProjectWikiTrainingRequestToLearningFlow`, `ProjectWikiTrainingOrchestrator_DoesNotRunTrainingForWikiQuestionRoute`), `Tests/AdaptiveLogicalOrchestratorTests.cs`, `Tests/ProjectWikiOrchestrationFlowTests.cs`, `Tests/OrchestrationPlanTests.cs`. Старые static-route проверки сохраняются как совместимость старого входа; новый обученный route проверяется отдельной квалифицированной state fixture. Падающий тест, требующий ответ от пустой модели, разделить на untrained честный отказ и trained positive, не понизить порог и не создать ручной PASS. Свежих падений этого набора в этой документной работе не измерялось.

Будущая команда из корня:

```powershell
dotnet test .\Tests\LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~ExperimentBasedPromptDomainE2ETests" --logger "trx;LogFileName=prompt-domain-e2e.trx" --results-directory .\artifacts\fbe\FIX-E2E\attempt-0001\tests
```

Порядок: принять планы14/15 → перенести измеренные механизмы и пройти их локальные learning tests → материализовать typed handoff/registry consumer и фиксированные fixtures → проверить восстановление → один multi-domain e2e набор → только при фактической квалификации обсуждать более широкий runtime activation. Не запускать всю production training chain для получения локального допуска.

## 6. Что остаётся условием, а не обещанием

Сейчас отсутствуют: готовый outer topic checkpoint для этого набора; typed handoff/consumer; новое измерение полного multi-domain пути; product gate WT; квалификация малого translation состояния. Эти пробелы перечислены с выбранными местами и рецептами выше. Они блокируют **соответствующий dependent E2E результат**, а не документирование/проверку независимых успешных исправлений. Полный router для всех тем и качество любых новых доменов этим планом не заявлены.

Модель исполнителя — текущая default без override. Скилы — `intent-to-plan`, `phase-planning`, `plan-review`, `generated-code-review`, `test-integrity`, `human-communication-GLOSSARY` из `docs/skills`; технический и пользовательский отчёт — русский. Внешняя открытая китайская модель создаёт новые production DPO; CODEX с `astra 6 ultracode` выбирает и оценивает их качество. Сам тест не подделывает эти роли и не выдаёт fixture acceptance за production DPO review.

STOP/NEXT немедленно прекращает действия прежней карточки: не дописывать её отчёт, тесты, commit или cleanup. Коротко сообщить уже подтверждённое состояние; NEXT переводит к точному назначенному ID, STOP запрещает дальнейшие действия. Полные роли skills и русский шаблон отчёта — в [общем контракте](03-test-and-execution-contract.md). Один запуск получает один handle; штатное ожидание продолжается в том же ходе до terminal/deadline, без нового polling-цикла, повторной выдачи или выдуманной диагностики «процесс пропал». Старые результаты сохраняются. Порядок следующих ID — только [реестр](01-registry.md).

## 7. Связь новых MC/PD/MS с уже выбранной E2E-архитектурой

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** FLOW/ROUTE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Дополнение2026-10-21 UTC по `4e993a33c9396f155a4aaeb42170e0251f34aa77`. [FIX-MC](21-sft-optimizer-continuity.md), [FIX-PD](22-dpo-valid-length.md), [FIX-MS](23-sft-response-mask.md) относятся к нейронному code-documentation learner. Они не дают нового измеренного outer-router или нового learned FlowRef. Каталог TOPIC-ORCHESTRATION/TRANSLATION/WIKI и фиксированный TRAIN/EXAM этого плана сохраняются; ещё одна конкурирующая схема диспетчеризации не добавляется.

Недостающая связь для будущей совместной интеграции — изоляция training state/config от prompt-based dispatch. MC использует **отдельный планируемый** `CodeDocumentationSupervisedContinuationConfiguration`/startup grant catalog→`CodeDocumentationSupervisedContinuationCoordinator`→`TrainSupervisedTransactionAsync`→SFT transaction/v3 publisher (точные файлы в21). `POST /api/code-documentation/supervised/continue` принимает только opaque startup selector и не является learned domain router. Он не использует DPO `CreateRepositoryStartRequest` как SFT authority. MS target mode в существующем DPO→SFT пути остаётся server-owned exact profile, формируемым [ContinuousLearningModelServerComposition.CreateRepositoryStartRequest:3199–3237](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/14.ContinuousLearning/Training/ContinuousLearningModelServerComposition.cs#L3199-L3237), с проверкой domain в [ResolveRepositoryStartRequest:2652–2672](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/14.ContinuousLearning/Training/ContinuousLearningModelServerComposition.cs#L2652-L2672). Если оба кандидата отдельно выбраны вместе, MC v3 identity включает mode/framing; `ResumeSft` не переключает loss target, для смены нужен явный `NewSftEpoch`. Комбинированный product route ещё не измерен, нового опыта этот пункт не назначает.

`LearnedPromptDomainRequest` не получает checkpoint/optimizer/mask flags. Learned handoff сам не выдаёт admission на обучение. PD execution view находится внутри уже допущенного DpoTrainer и не может выбирать предметный домен. Каталог трёх тем, router TRAIN/EXAM и положительные запросы остаются прежними.

При отдельно выбранной совместной интеграции добавить assertions к **тому же** `ExperimentBasedPromptDomainE2ETests` и его существующим requests: при Selected вызван ровно один ожидаемый handler, при deferred/unknown —0target calls; запросы translation/orchestration/Wiki не создают code-documentation SFT/DPO session; unselected domain не получает optimizer import/update/publication. Наблюдать существующий session factory/executor/publisher boundary через readonly call receipts, без дополнительного H512 load/Train/Generate. Это проверка, что роутинг не изменяет чужой результат обучения; она добавляет0training updates к бюджету раздела3. Не объявлять эту отрицательную проверку обучением нового neural domain selector.

Положительный neural A/B выполняется ровно один раз по21/22/23, не повторяется внутри общего router suite. Полный новый путь `prompt→code-documentation training receiver` этими опытами не доказан и не добавлен в каталог по строковому совпадению. Если такой receiver отдельно назначен, он должен иметь собственный qualified learned FlowRef и действующий native gate; нынешние candidate plans не дают автоматического права расширить topics TRAIN или заменить3-topic результат обещанием общего роутера.


## 8. Связь дополнений KHC/ORDER/KHE с существующим маршрутом

Дополнение 22 октября 2026 по `213f0c9b0568384632ab6bb5a67c57d6f17b3df9` и позднему `c8b0d8a029ce721ecd906d3ff747e740b524be23`. [FIX-KHC](24-file-action-shared-parse.md) уменьшает подготовку C# file-action памяти, [TEST-ORDER](25-dpo-sft-order-learning-coverage.md) покрывает порядок native SFT/DPO стадий на сохранённом code-documentation состоянии, [FIX-KHE](26-method-knn-training-admission.md) сохраняет уже обученную память методов при непригодном новом наборе. Ни один опыт не обучал новый prompt→domain selector. Прежние три topic, fixed TRAIN/EXAM, квалификация RET/WT, порядок реального dispatch и бюджеты раздела3 сохраняются.

Для совместно назначенной интеграции добавить к **существующим** HTTP/readout receipts отрицательные assertions: translation/orchestration/Wiki/unsupported запрос не вызывает `SemanticCompilerFacade.TrainFileActionKnn` или `TrainMethodKnn`, не меняет соответствующие обученные таблицы и не запускает OR SFT/DPO stages; prompt/Metadata не назначают stage order, SFT optimizer state или reference checkpoint. Наблюдение на существующем receiver/factory boundary, 0 новых запросов и 0 training updates. Не нужно загружать H512 только для проверки отсутствия вызова его trainer.

Это осмысленная проверка сохранности результата чужого обучения, а не отдельный эксперимент на наличие флага. Положительный KHC consumer проверяется через настоящий Compile в24; положительный ORDER learning replay — один раз в25; приобретение и удержание MethodKnn через Compile — в26. Они не повторяются внутри E2E и не объявляются доказательством нового C# routing topic. `ClassifyDomain` keyword fast-path в SemanticCompilerFacade не заменяет выученный выбор. Новый `prompt→C# action` или `prompt→mixed-stage training` handler этим дополнением не добавляется: historical evidence не подтверждает такой путь, а новый routing contract не требуется для сохранения измеренных результатов.


<a id="resource-transfer-e2e"></a>
## 9. Связь FIX-GB / FIX-RMS / FIX-BW с тем же prompt→domain E2E

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** FLOW/ROUTE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

**Привязка к двум стадиям переноса.** Для GB/RMS/BW и Wiki WK/WC общий [03§9–11](03-test-and-execution-contract.md#native-ab-transfer) теперь сохраняет A до product diff и B после него в одном canonical checkout. E2E принимает только завершённую пару: неизменяемый `intent.json` A, `baseline-stage.json`, отдельный `candidate-binding.json` с digest A intent/стадии и per-arm native receipts. Candidate SHA берётся из binding, а не из `intent.candidateCommit:null`; общий путь DLL или наличие A-файла ещё не означает пригодный B. Несогласованный SHA/fixture/receiver или незавершённый stage даёт конкретную precondition error до E2E readout, без повторного fit и поиска другого состояния. Это проверка происхождения уже полученного результата обучения; нового domain-training бюджета, запросов, labels или допуска Wiki в обход gate здесь нет.

Дополнение по `7a9fa58d55127ae88e93cd5d71c24a579f730ca3`. Новые [FIX-GB](29-matmul-right-gradient-locality.md), [FIX-RMS](30-broadcast-coordinate-allocation.md), [FIX-BW](31-bpe-weighted-chunks.md) имеют измеренный ресурсный результат **в обучении**, но не обучали outer selector. Сохраняются архитектура раздела2, каталог TOPIC-ORCHESTRATION/TRANSLATION/WIKI, fixed TRAIN/EXAM, выбранный `LearnedPromptDomainRequest`, HTTP dispatch, шесть запросов Q1–Q6 до/после restart и бюджеты раздела3. Новая keyword-классификация, четвёртый topic или второй конкурирующий E2E не добавляются.

Положительная E2E-логика остаётся обязательной: пользовательский prompt без gold/target → обученный outer selector с learned FlowRef → проверенное соответствие registered domain/receiver → ровно один native handler → его фактический ответ → повтор после cold restore без Train. У Wiki gate=false остаётся честный deferred; token/weight/resource success не выдаёт права обойти admission.

| Новое основание | Где положительно проверяется перенос | Что добавить к существующим E2E receipts без нового обучения |
|---|---|---|
| FIX-GB | Два native DPO updates/arm из segmented S0 в29, real adapter changes | На прежних translation/orchestration/Wiki/unsupported запросах не создаётся code-documentation DPO session, не импортируется S0 и не меняется чужой optimizer/reference. Наблюдение существующей factory/executor границы, не H512 load ради проверки |
| FIX-RMS | Четыре native SFT updates/arm из optimizer400 в30 | Те же запросы не вызывают code-sign/SFT Train и не переключают чужую topology/RNG. В request metadata нет kernel flags/checkpoint selection; область learning state задаёт настоящий получатель |
| FIX-BW | Native BPE writer A/B на двух профилях и существующий KnnLogicalUnit Train→HNSW→Execute test в31 | Cold dispatch не запускает BPE.Train для восстановления уже обученного состояния; выбранный handler и его state identity совпадают до/после restart. Когда конкретный receiver реально использует tokenizer, записать его существующую identity; если не использует — not_applicable, не добавлять BPE ради поля |

Это assertions сохранности и использования результата обучения на **том же** реальном consumer path, не отдельные эксперименты о наличии флагов. Снимать readonly call receipts на уже выбранных factory/receiver boundaries; не вводить вместо handler mock, который сам возвращает ожидаемый route. Сохранить разделение `routing_correct`, `domain_invoked`, `answer_correct`, `deferred`, `wrong_dispatch`; scalar resource gain не заменяет ни один из этих исходов.

Дополнение использует0новых запросов,0training updates,0новых topics,0дополнительных H512 restores. Учебные A/B29–31 здесь не повторяются. Если существующий receiver использует обученный BPE, источник его state закрепляется уже назначенной fixture; нельзя подставить code-doc/EN→RU BW320 вместо чужого tokenizer лишь потому, что у них одинаковый vocab size. KNN smoke из31 проверяет собственную двухклассовую unit, а не новый learned outer topic route. Положительный новый `prompt→code-documentation learner` путь этими опытами не доказан и данным дополнением не обещан.

При будущей реализации падающий E2E разбирается по месту: неверный learned selection — fixtures/RET/WT; неверный binding/вызов — dispatch contract раздела2; после restart исчезло состояние — loader/receiver identity. Не чинить это изменением kernel, keyword shortcut, скрытым Train, снятием Wiki gate или заменой actual handler response. Назначенная интеграция использует общий test/execution/STOP/NEXT контракт03; текущее изменение остаётся документным.

<a id="wiki-codec-e2e"></a>
## 10. FIX-WK: сохранить смысл выученных Wiki keys при передаче и restart

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Позднее основание — [WIKI-FEATURE-KEYS-01 / FIX-WK32](32-wiki-feature-key-isolation.md), source32d888. Оно относится к внутреннему обучаемому Wiki action selector, не создаёт новый outer topic. Сохраняются три темы, тот же six-request набор Q1–Q6 до/после restart, learned FlowRef→registry→native handler и бюджет раздела3. Дополнение добавляет **0запросов,0Train,0новых topics**.

В `LearnedWikiDispatch_RetainsOneDecisionAndHonorsGate` и `LearnedTopicDispatch_RestartNeedsNoRetraining` добавить assertions на уже используемом Wiki state: codec identity до и после handoff/restore совпадает; labels и weights относятся к той же версии; native unit вызывает gated Route ровно один раз; downstream Train не возникает при NeedsSecondPass. `FeatureKeyCodecVersion` принадлежит серверному snapshot, не флагу из prompt/HTTP metadata. Тип записи соответствует плану32, любое изменение формата не выбирается по тексту пользовательского запроса.

Legacy state из ранее назначенной fixture сохраняет legacy reader; новый native v2 state — namespaced reader. Не заменять квалифицированную Wiki fixture историческим untagged WK B ради проверки codec. Если выбранный state не является v2, записать legacy и проверить его восстановление; положительный v2 cold restore уже проверяется локально в32 на единственном native fit. Сам WK snapshot имеет missing held-out gate: в product path остаётся честный deferred/0target, хотя internal evaluation показала приобретение0→3/6. Это не новый положительный product dispatch. Синтетический passed gate/прямая подстановка ожидаемого unit запрещены.

Падение после restart с потерянными keys чинится в конкретных FromSnapshot/ToSnapshot/WithGateEvidence и handoff snapshot preservation; несовместимые labels — по FIX-WT; неверное соответствие learned FlowRef/handler — по разделу2. Ни keyword fast-path, ни повторный fit не заменяют сохранённое состояние. Read-only gate/codec assertions поддерживают использование результата обучения и не получают отдельного статуса успешного эксперимента. По каждой прежней строке E2E сохраняются routing_correct/domain_invoked/answer_correct/deferred/wrong_dispatch, а не общий PASS по одному полю версии.

<a id="wiki-once-pos-e2e"></a>
## 11. FIX-WC и TEST-POS в прежнем prompt→domain пути

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** FLOW/ROUTE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Дополнение по `1fa9bac16ff209646f7abbd29dad2461df3ed538` сохраняет выбранный outer selector → learned FlowRef → зарегистрированный native receiver → его реальный ответ → cold restart. Три темы, TRAIN/EXAM, шесть Q1–Q6 и бюджет раздела3 прежние. [FIX-WC33](33-wiki-training-feature-reuse.md) оптимизирует обучение Wiki request-router, [TEST-POS34](34-pos-bigram-learning-coverage.md) покрывает уже работающий POS learner. Ни один опыт не обучал новый outer prompt→domain selector.

Для отдельно выбранного WC дополнить **те же** Wiki cold-dispatch assertions: FromSnapshot читает сохранённые weights/labels/codec/gate без Train; per-call feature cache не сериализуется и не появляется как источник authority после restart. Выбранный handler использует свой exact saved state. Historical WC gate=false остаётся deferred на public Route/Unit; internal RouteForEvaluation из учебного harness не подключается к HTTP/runtime. Ресурсный выигрыш и local4/4 не дают passed gate. Если Wiki receiver после отдельной квалификации уже допущен, прежние Q1–Q6 проверяют фактическое принятие/отклонение и вызов ровно нужного target; новое обучение33 внутри E2E не повторяется.

TEST-POS возвращает класс порядка POS, не domain ID/FlowRef. Native learner project12 и heuristic classifier project29 — разные получатели; наличие успешного E25 не разрешает переключить существующий fusion на project12 или добавить строковое правило dispatch. Карточка34 не меняет ни один runtime handler и не доказывает готовность нового translation route. Её frozen6+4panel остаётся локальным learning test одной темы; скрытая подача его gold в outer selector запрещена. При обращении к существующему translation receiver сохраняется его прежняя qualified fixture/state identity, а не новый произвольно обученный POS экземпляр.

Отчёт на прежних E2E receipts раздельно показывает `routing_correct`, `domain_invoked`, `answer_correct`, `deferred` и `wrong_dispatch`; POS label/ресурсный scalar не подменяют эти показатели. Наблюдение existing receiver boundary добавляет **0новых запросов,0Train/updates,0topics и0H512 restores**. Положительный native WC writer измеряется один раз по33, POS acquisition один раз по34; их сохранённые receipts читаются здесь как provenance, без повторного fit ради общего отчёта.


<a id="ks-nh-sk-e2e"></a>
## 12. FIX-KS / FIX-NH / TEST-SK: сохранить границы получателей в том же E2E

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Дополнение по `3acae42f7ef474e449737a57f0cf064921c4bbc7` связывает [35](35-method-knn-target-scope.md), [36](36-dpo-scored-row-head.md) и [37](37-structural-knn-learning-coverage.md) с уже назначенным выше learned dispatch. Ни один из трёх опытов не обучал outer prompt→domain selector. Поэтому остаются прежние TOPIC-ORCHESTRATION/TRANSLATION/WIKI, learned FlowRef→registered handler, те же Q1–Q6 до/после restart, настоящая HTTP-композиция и квалификация дочерних состояний. Четвёртая тема или новый C# HTTP handler из этих метрик не следуют.

| План | Где уже запланирован положительный native учебный путь | Что проверить на тех же существующих E2E receipts |
|---|---|---|
| FIX-KS |35: pinnedA/nativeB, один scoped fit B, direct Predict и Compile того же обученного instance; отдельные derived facade inputs с одинаковым C# marker | Foreign/unsupported request не вызывает ни TrainMethodKnn, ни TrainMethodKnnWithTargetScope и не заменяет pairs. Prompt/TaskMetadata не назначают k или target projection. Method category не превращается в DomainTag/FlowRef |
| FIX-NH |36: native DpoTrainer, policyS0/referenceB0/freshDPO0, полный transformer→selected head→backward→optimizer | Запрос к другому receiver не запускает DPO/SFT. Scored-row map создаётся внутри уже выбранного и допущенного learner из captured exact material, не из внешней gold/mask metadata. Caller не выбирает чужой checkpoint, optimizer или authority |
| TEST-SK |37: настоящий TrainStructuralKnn→Compile→PredictedNextTokens, один96pair fit и общая before/after-квитанция | На чужом receiver не возникает StructuralKnn Train; predicted SyntaxKind ID не используется как topic. TaskInput передаётся исходному выбранному handler; structural expected kind не дописывается в prompt или payload |

Эти assertions читают уже существующий trace/call ledger и state identities назначенной E2E fixture; не создают новые predictors или H512-модели ради проверки счётчика. Если метод отсутствует в используемой композиции, фиксируется проверенный `not_in_receiver_graph`, а не вымышленное runtime число. На сохранённых состояниях сравниваются их реальные идентификаторы и уже назначенные ответы после cold restart. У Method/Structural KNN нет native persistence; этот факт не заменяется JSON evidence или скрытым переобучением при загрузке.

Положительные learning suites35–37 исполняются только по своим назначениям и один раз. Их completed receipts связываются с интеграционным отчётом ссылками; E2E не запускает их вновь. Дополнение расходует **0новых запросов,0новых Train/DPOupdates,0дополнительных restore/producer,0новых topics**. Исходный бюджет§3 не меняется. C# marker в facade fixture35 — локальный вход StackDetector, не правило выученного outer выбора.

Сохранить отдельные результаты `routing_correct`, `domain_invoked`, `answer_correct`, `deferred`, `wrong_dispatch` и source/checkpoint/binding identity. Правильный topic с Wiki gate=false остаётся честным deferred; missing/unknown binding не запускает первого доступного handler. Неверный route чинится в selector/dispatch, неверный локальный ответ — в выбранном domain consumer/fixture; не заменять это static keyword shortcut, ослаблением gate, подачей gold или дополнительным Train. Нового измеренного сквозного успеха эта документная связь не заявляет.

<a id="ast-sibling-e2e"></a>
## 13. FIX-AST: downstream C# память не становится маршрутизатором

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[FIX-AST38](38-structural-knn-sibling-cache.md) экономит подготовку sibling-признаков при native StructuralKNN Train. Он не меняет и не обучает topic selector, FlowRef или catalogue dispatch. Публичный Extract/query path, domain flags и `%200` сохраняются. Положительные12/16 и7/16 в AST01 не являются prompt→domain результатом.

Прежний E2E уже содержит три topic-профиля, явную передачу TaskInput, native receivers и cold restart. При совместном принятом переносе на **тех же** запросах проверить отсутствие чужого StructuralKNN Train/изменения памяти; для этого не добавлять новый C# handler, четвёртую тему или повторный producer. FIX-AST добавляет0E2E queries/0E2E fits. Его положительная consumer-проверка находится в38: настоящий facade учится всем файлам и возвращает actual PredictedNextTokens на том же instance.

У StructuralKNN нет native Save/Load; evidence rows не становятся восстановленным recipient checkpoint. Полный cold restart прежних selector/recipients проверяется как раньше, а cold restore structural таблицы не объявляется выполненным и не заменяется скрытым Train. Кэш SyntaxNode имеет жизнь одного дерева и не попадает ни в handoff, ни в сохранённую обученную память.

<a id="ng-br-e2e"></a>
## 14. FIX-NG / FIX-BR: сохранить предметные границы двух новых learner-кандидатов

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

[FIX-NG39](39-null-guard-training-label.md) и [FIX-BR40](40-bpe-realized-merge-objective.md) основаны на70062c и не обучали outer selector. Общая E2E архитектура, три зарегистрированные темы, Q1–Q6, typed payload, learned FlowRef и gate Wiki остаются прежними. Дополнение добавляет **0 тем,0 запросов,0fits**; не создаётся словарная маршрутизация по `null`, `lambda` или символам кода.

Если отдельно выбранная интеграция действительно использует изменённый компонент, её learning provenance берётся из completed native receipts39/40: exact source/fixture/DLL identity и связь A intent→baseline-stage→candidate-binding. Старый source-copy результат не заменяет проверку actual receiver. Если компонент не участвует в выбранном recipient, отмечается `not_applicable`; не добавлять его в путь только ради теста или metadata.

| Кандидат | Уже назначенная непосредственная проверка обучения | Ограничение общего dispatch/restart |
|---|---|---|
| FIX-NG | Public facade writer12rows → actual private reader на том же instance, k3; после обучения target и controls из39 | Не запускать FileAction Train в orchestration/translation/Wiki recipient. Native Save/Load нет: captured rows не становятся production checkpoint; hidden fit для restart запрещён. UseFileActionKnn default=false сохраняется |
| FIX-BR | Native Train257 из готового S0, отдельно legacy default и realized opt-in; canonical after→fresh Load без Train | Выбранный receiver сохраняет **собственный** tokenizer identity. BR257 не заменяет его BPE320. Пользовательский prompt/HTTP metadata не выбирает training objective; KNN320 caller остаётся legacy |

Новый положительный `prompt→C# domain` маршрут этими опытами не доказан и не объявляется реализованным. Его нельзя получить заменой сохранённого RET selector вызовом статического ClassifyDomain/StackDetector или добавлением детерминированного if по тексту. Обязательная положительная сквозная проверка уже задана§2–5: реальный learned выбор, проверенный binding, ровно один native recipient, его фактический ответ и восстановление поддержанного состояния без переобучения. Новые карточки расширяют доказательства downstream обучения, сохраняя границы этого задания.

<a id="new-measured-e2e"></a>
## 15. Новые IM/KR и downstream-факторы: три разных уровня доказательства

Положительный [IDENTITY-M1-01 / FIX-IM48](48-learned-route-body-identity.md) закрывает ранее известную возможность соединить заголовок выбранного маршрута с телом другого. При реализации typed handoff из§2 он должен использовать тот же исправленный FindLearnedPair, передавая grounding.FlowRef в ограниченный lookup **до** ранжирования пар. Нельзя выполнить второй глобальный lookup в consumer и снова потерять связь. Guard grounding.FlowRef==pair.FlowRef==pair.Chosen.FlowRef остаётся необходимым для некорректного входного checkpoint; исправление reader не делает любое сохранённое состояние правильным.

| Уровень | Что теперь подтверждено опытом или запланировано | Чего этот уровень не доказывает |
|---|---|---|
| Доступ к выученному ключу | [FIX-KR49](49-router-learned-key-normalization.md): два урока, та же память,2/4→4/4; native prompt→profile→manifest consumer запланирован | BaselineKeywordCandidateGenerator не становится semantic teacher. DistributedModelOrchestratorFlow в specialist branch возвращает manifest/endpoint, а не ответ запущенного специалиста |
| Соответствие выбранного плана его телу | FIX-IM:2/3→3/3; actual OrchestrationPlanMachine и OrchestrationPlanOrchestrator readout проверяются на восстановленном snapshot | GeneratePlan/RunAsync здесь возвращают план. Само наличие FlowRef или MlGenerated не означает запуск доменного обработчика |
| Выполнение предметной задачи | Прежний FIX-E2E§2–5: learned outer selector → проверенный binding/payload → ровно один native recipient → его фактический ответ; затем restart/ablation | Новые результаты KR/IM не объявляются успешным полным E2E. Проверка12HTTP запросов и допустимых target calls ещё только назначена |

KR не заменяет выбранный RET outer-selector и не добавляет keyword fallback по C#/.NET для темы. LAB-A/LAB-B и code/docs — классы задачи оркестрации, не новые предметные домены. Новая карточка49 отдельно показывает false matches внутри слов и override DomainHint; эти controls не чинятся повышением ожидаемого score или тихим снижением thresholds.

### Как использовать остальные новые переносы

AGC/FBM/RRD/ATS/SSR влияют на стоимость реального training path соответствующего Student. E2E использует их completed native receipts только если выбранный recipient действительно вызывает этот TensorCore/learner. H64 code-sign state не подставляется в outer selector, и никакой producer400/1000 не повторяется ради интеграционного smoke. Математическая эквивалентность локального kernel не заменяет subject-specific качество доменного ответа.

JO фиксирует стоимость EN→RU writer. У назначенного здесь translation recipient собственные RU→EN данные и metadata; наблюдение JO не превращается в его измеренный выигрыш. После принятия нового writer сохраняется его ordinary flow и checkpoint identity. VA проверяет собственное learned marker state; его два ложных отказа не удаляются из общего отчёта и не скрываются как допустимое изменение gold. Если в конкретном recipient такой marker не обучался, связь отмечается not_applicable, без искусственного добавления уроков.

Наборы, три предметных получателя, Q1–Q6, gate Wiki и общий budget§3 остаются прежними: эта дельта добавляет **0outer Train,0E2E prompts,0receiver calls**. Новые reader/contract captures48/49 принадлежат своим карточкам и не запускаются ещё раз внутри FIX-E2E. Если результат отдельного native переноса отсутствует, он остаётся недостающим интеграционным свидетельством, а historical source-copy PASS не подставляется вместо него. Порядок, restore, русский отчёт и немедленный STOP/NEXT определяются01/03.


### HNQ, PQ и IDX в том же предметном пути

[FIX-HNQ50](50-hnsw-training-query-norm.md) касается создания памяти HNSW во время Train. Его controlled RNG7331 принадлежит common test harness; production default не становится детерминированным и не выбирает FlowRef. Полный Index-only snapshot нельзя подставить вместо tokenizer/labels всего Unit. [FIX-PQ51](51-pos-query-bigram-reuse.md) относится к classifier проекта12: syntactic order POS — узкий профиль темы перевода и языковых формулировок, не готовый domain selector. LlmIngredient использует другой проект29; перенос между ними не назначается без отдельного положительного опыта. [FIX-IDX52](52-translation-training-identity-index.md) индексирует доступ к уже записанной учебной памяти translation receiver и сохраняет его точную identity/restore семантику.

Эти планы не добавляют E2E inputs, producer или вызов специалиста. Их small native comparisons выполняются только при назначении соответствующей карточки; общий E2E использует finished receipts, если компонент действительно участвует в выбранном recipient. Временные выигрыши HNQ/IDX и resource gain PQ не выдаются за улучшение topic selection, языкового обобщения или успешный end-to-end ответ.


### HCL: последовательность фаз не заменяет выбор домена по промпту

[FIX-HCL53](53-hierarchical-local-history.md) исправляет reader обученных Markov таблиц в существующем экспериментальном модуле34. Его тема — оркестрация последовательности проверки/сохранения; вход — наблюдённые события, не произвольный пользовательский текст. Положительные16/16 и permutation-check получают высокий приоритет для этого механизма. Они не доказывают, что модель умеет выбрать предметный домен, разобрать TaskInput или исполнить binding. Общий E2E не вызывает full E05, kMeans или нового Markov producer. Существующий RET selector и бюджет сохраняются; HCL native receipts относятся только к реально использующему такой reader пути.

## 16. Связанные кандидаты23октября: receiver identity и отрицательные Wiki-классы

Существующая архитектура§2–5 остаётся единственным E2E планом: learned outer selector → typed handoff → проверенный domain binding → настоящий receiver → его ответ → cold restore. Новые ID из [56](56-measured-delta-and-existing-coverage.md) не обучали общий выбор темы. Положительные SVD next-event, FSR static label и IL/SC partial token не дают основания открыть новый C# outer handler или назначить cluster/component доменом.

Новый недостающий контракт — **явно привязать evidence конкретного learner к реально вызванному receiver** в существующем E2E report. В записи receiver добавить прочитанные из completed native receipt `PlanId, nativeSourceSha, loadedAssemblySha, fixtureSha, recipientId, model/tokenizerIdentity, executionProfile, checkpointIdentity` и отдельно `applied/not_applicable/not_verified`. Эти поля описывают выполненный путь, не выбирают маршрут. Сравнить их с действительным target invocation и восстановленным состоянием. Historical source-copy receipt не заменяет native candidate receipt.

| Связанная правка | Что проверяется на уже назначенном пути | Когда честно `not_applicable` |
|---|---|---|
| IL / SC | Действительный neural receiver использует свой parent/tokenizer/context и выбранный SFT/shifted-DPO profile; prompt не выбирает authority, optimizer или training mode. Completed state и deferred/admitted receipt относятся к этому recipient | В текущем трёхтемном E2E нет отдельного зарегистрированного C# neural handler: локальная пара не становится его положительным E2E |
| CS / MB / RPE / AC / SEXP / FBC / LFT | Только реально вызываемый native training path имеет соответствующий source/recipe; memory и state другой темы не меняются | При read-only запросе нет Train, поэтому нельзя приписывать resource gain этому запросу или запускать обучение ради coverage |
| FSR / SVD | Их собственные native consumer tests проверяют learned labels/next-event reader; identities сохраняются в отчёте интеграции при наличии consumer | Нет traced consumer в выбранном binding — не вставлять E10/Compile искусственно в outer dispatch |
| PA / MC | Последний **опубликованный** compatible checkpoint выбран и холодно восстановлен; pending/adoption artifact не выдаётся за accepted state | Private PA host не реализует product route и не получает product grant автоматически |

Успех каждого локального кандидата остаётся самостоятельным; отсутствие применимости в данном E2E не стирает измеренный выигрыш и не даёт ложный E2E PASS. Не нужно повторять его acquisition/learning suite ради заполнения поля отчёта.

[WM02 дополнение14§7](14-wiki-routing-taxonomy.md#7-wm02-квалифицированный-отрицательный-контроль-и-предел-margin-гипотезы) уточняет oracle: `deferred`, `wrong_selected` и `correct_selected` — разные исходы. Q7 про перевод отрицателен только для Wiki-подмаршрута; outer translation остаётся допустимым. Не менять назначенный outer TRAIN/EXAM на основании WM02 и не сравнивать разные наборы как один margin factor. False evaluation gate запрещает запуск training handler даже из непустого learned state; это проверяют существующие WT gate contracts. У WM02 Q7/Q8 более ранний margin refusal, поэтому их два readout не объявляются покрытием evaluation-gate branch.

Дополнение использует только существующие E2E receipts/assertions и результат отдельно назначенного двухчтенного WT deferral/migration test. **Новых outer Train, prompts, receiver calls, domains, bootstrap и повторов других suites —0.** Основной положительный E2E budget и12HTTP проверок остаются§3–5. Полный prompt→domain→полезный ответ ещё требуется измерить при назначенной реализации; документация не выдаёт его за уже успешный результат. STOP/NEXT/default модель/русский отчёт и ожидание исходного handle — по01/03.

<a id="sep133-e2e"></a>

## 17. SEP01/133: результат получателя оценивается в его учебном профиле

Единственное расширение по [62 / FIX-SEP](62-supervised-framing-and-learning-readout.md) — явный формат чтения результата уже вызванного neural handler. Learned selector, server-owned domain binding, typed `TaskInput`, реальные downstream вызовы, identity guard и cold restore из §2–5/16 остаются прежними. Новая эвристика выбора домена, переобучение selector и ещё один маршрут не требуются.

| Наблюдение в существующем receipt | Что подтверждает | Что не заменяет |
|---|---|---|
| `routing_correct` / `domain_invoked` | Выбран правильный learned route и вызван его настоящий получатель | Приобретение ответа в предметной модели |
| `teacher_forced_response_correct` + `sequence_profile` | Усвоенный ответ читается в объявленном контексте и на заданных targets | Свободную генерацию или E2E ответ пользователю |
| `answer_correct` | Реальную Generate/предметную функцию по существующему gold | Не может заполняться из teacher-forced массива |
| `checkpointIdentity`, `sequence_digest`, `readout_mode`, `response_target_count` | Сопоставимость назначенного чтения до/после cold restore | Не доказывает optimizer/RNG continuation из weights-only файла |

Профиль берётся из server-owned handler/fixture binding, не из текста промпта. Для обученного SFT-получателя это `sft-joint-lf-v1`; exact-DPO score сохраняет `prompt-response-segments-no-separator-v2`. Не добавлять LF в admitted DPO prompt для получения красивой общей цифры. После cold restore сверить checkpoint/tokenizer/profile/digest и только затем сравнивать уже назначенные показатели.

Эти поля добавляются к существующему receiver receipt из §16. Если у Q1–Q6 нет соответствующего neural SFT readout, записать `FIX-SEP=not_applicable` с причиной: не подменять TaskInput примерами Abs/Sign, не создавать новый handler или дополнительный Forward ради заполнения таблицы. Qualified SEP states проверяются отдельно ограниченным fixture 62, **не становятся одной универсальной моделью или новой темой outer TRAIN**.

Учебная связь здесь конкретна: в SEP01 ошибочный профиль управлял `AllCorrect` в цикле acquisition, а исправленный прочитал уже выученные токены; в native цепочке metadata связывают этот oracle с фактически вызванным получателем. Это не новое положительное E2E измерение. Бюджет 20 не расширяется: дополнительных outer Train, prompts и downstream calls — 0. Полный prompt→domain→ответ→restart остаётся будущей проверкой; нельзя поставить PASS по одному удачному SEP-readout. Правила ожидания, STOP/NEXT и русского отчёта — по 03 и 62.

### Уточнение 25 октября: receipt удержания не берёт gold из последнего урока

Связанный [N63C/17§11](17-dpo-sft-correction-collision.md#n63c-fixed-gold-readout) использует то же Abs-state, что измерено SEP, и сохраняет правильный gold независимо от ошибочного training input A. Если выбранный neural handler уже имеет этот завершённый learning receipt, существующий E2E receipt переносит его `readout_role`, `sequence_origin`, `probe_digest`, профиль и aggregation как происхождение downstream-проверки. Проверяемое expected остаётся у scorer; caller не выбирает его и не передаёт через TaskMetadata. Отсутствующий receipt даёт `not_measured`, а не восстановление метрики из ответа модели.

`teacher_forced_response_correct` при `retention_gold` означает правильность относительно ранее приобретённого ответа. Оно не равно успешному воспроизведению последнего поданного SFT текста и не заполняет `answer_correct` свободной генерации. Mean-logP margin N63C, response-sum native DPO reward и SFT/LF retention — разные поля; их нельзя усреднять в один общий «успех маршрута».

Для уже назначенных Q1–Q6 прежний budget и реальные вызовы получателей сохраняются. N63C receipt переиспользуется, 42 дополнительных чтения внутри E2E не запускаются. Если у выбранного получателя нет neural N63C/SEP-профиля, эти поля получают `not_applicable`; подмена предметной задачи Abs-функцией не разрешена. Это план проверки связи существующих успешных механизмов с consumer, не новый измеренный E2E-результат и не новый learned router.

### Дополнение25октября: потребитель и основание вывода проверяются отдельно

[Source-проверка HCL/SVD](04-evidence-and-novelty.md#transfer-boundaries-2026-10-25) установила вызовы E05/E10 через экспериментальный dispatcher; прямого продуктового потребителя названных классов в проверенных ветках не найдено. Их успешные10/16→16/16 и2/5→5/5 не разрешают заменить предметный handler на прогноз следующего события. Сохранить реальные вызовы и отрицательные domain/reject assertions Q1–Q6. Общий матричный SVD уже использует правый фактор и не является получателем FIX-SVD.

Для downstream repair59 [совместимый receipt квалификации62](57-native-transfer-contract-gaps.md#separator-133-calibration) переиспользуется до ремонта; E2E не повторяет ни6qualification Forward, ни58repair Forward. Receipt endpoint характеризует только выбранное состояние/набор. [Позднее ограничение139](59-collision-repair-transfer.md#endpoint-comparison-limit) запрещает заполнять из одной margin поля сравнительного превосходства scheduler или общей безопасности домена. `routing_correct`, `domain_invoked`, teacher-forced readout и фактический `answer_correct` по-прежнему имеют разные источники; неизвестное остаётся неизвестным. Дополнительных запросов, модельных вызовов или gate на создание новых наборов эта дельта не вводит.
