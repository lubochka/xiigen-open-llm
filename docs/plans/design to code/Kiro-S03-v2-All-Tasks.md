# S03 v2 — руководство и 13 полных заданий

Human reference, not every-worker preload.


---

## Файл: .kiro/xiigen-slices/luna/s03/README.md

# XIIGEN S03 v2 — автономное выполнение без изменения требований

Пакет переработан по переданной истории остановок. Его цель — не начать S03 заново,
а убрать противоречивые разрешения и восстановить непрерывный проверяемый цикл.

Сохранены 13 задач, 52 индивидуальных role prompts, 8 разделов каждой задачи,
продуктовые задания/положительные и отрицательные примеры/сценарии/краевые случаи,
R1–R8, исторические source pins и предложенные certification contracts.
Отдельная review и непустые реальные tests остаются обязательными.

Изменено: START/OPERATING-POLICY/AUTHORIZATION/CI-PLAYBOOK, все общие permissions и
execution blocks в 13 tasks и 52 dispatch, возобновление текущих S03.00/S03.01,
разделение catalogue и live state, task-specific gate mapping и публикация CI
candidate до приёмки. Не поставляется новый runner или готовый workflow.

## Использование

В текущей сессии Kiro применить root MIGRATE-AND-START.md. Он описывает безопасное
сопоставление с installed v1 и продолжение. Полностью перезаписывать изменённую
локально папку нельзя. Подробности MIGRATION.md, соответствия в migration.json.
Не копировать большой человеческий combined reference каждому работнику.

## Источники и границы

Продуктовые окна — из v1 target ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d и planning
dfcf26996e20104f66fffb005da6e9c3d8d604cd. Они не освежались по GitHub при v2.
Текущий статус надо читать из live repo/evidence. Переданная история предполагает
принятое S03.00 и pending verification S03.01; это не наш fresh verdict.

AUTHORIZATION.md содержит прежний общий лимит 20 USD. Установка нового пакета
не создаёт новый бюджет. Реальные платежи, CI и Git мы не запускали.
VALIDATION.md описывает выполненные проверки пакета: не тесты приложения,
не независимый агентский review и не benchmark Luna.


---

## Файл: .kiro/xiigen-slices/luna/s03/START-PROMPT.md

# S03 v2 — продолжение, а не перезапуск

Этот файл заменяет прежний S03 START-PROMPT. Загрузка пакета сама не выполняет
команды и не подтверждает приёмку. Владелец принимает документ одной текущей
инструкцией; после этого обычные разрешённые операции не требуют новых вопросов.

## Текст текущего задания

Продолжай S03 в `lubochka/llm_mvp_core`, ветка `codex/dpo-implementation`.
Я принимаю S03 v2 и включённый `AUTHORIZATION.md` как единое постоянное задание.
Операционные ограничения старых S03-prompts заменены `OPERATING-POLICY.md`.
Содержание продуктовых требований R1–R8, проверки качества и отдельная review
не ослаблены. Защиту ОС, реальные policy denials и обязательные protections
не обходить.

Я разрешаю необходимую инженерную работу, commits кандидатов, исправления CI,
штатные PR, dispatch/rerun/cancel и документированный force-cancel в границах
AUTHORIZATION.md. Не возвращай вопрос о каждом из этих действий.
Суммарный лимит серии GitHub Actions — 20 USD, включая хранение. Если серия уже
принята, используй ОСТАТОК того же лимита; установка v2, новый task, session или
commit не создают ещё 20 USD. Не меняй account billing cap и платёжный метод.
Наличие действующего более позднего ограничения владельца обязательно учитывать.

Локальная рабочая область — `C:\Projects\llm_mvp_core`. Я явно принимаю исключение
из hard no-sibling/no-copy/no-worktree/no-quarantine-fallback rule для ephemeral
`GITHUB_WORKSPACE` разрешённого CI job; фактический путь фиксируется в job.
Новые локальные копии не разрешены. Известную SAC/WDAC-блокировку не перепроверять
тем же запуском; использовать уже допущенный CI по `CI-PLAYBOOK.md`.

Сначала прочитай `RESUME.md`, текущую короткую запись в существующем active/state
и только относящиеся к текущему кандидату evidence. Не повторяй S03.00, если его
planning milestone подтверждён. Восемь уже написанных identity-сценариев S03.01
сохранить; восстановить их проверку и отдельную приёмку, не писать их заново.
Run 34939441448 — исторический locator для чтения текущего статуса, не вечная
команда ожидания или повторного запуска. Если нужный результат уже получен,
принять по evidence; если очередь устарела — выполнить разрешённое восстановление.

Используй GPT-5.6 Luna / xhigh через доступные настройки. Модель и effort,
не наблюдаемые инструментом, отмечай UNVERIFIED без выдумки и без бессмысленного
поиска "доказательства" в тексте ответа. Не менять модель самовольно.

Сохрани 13 task IDs и их восемь разделов. Для выбранного task загрузи его packet,
соответствующий role dispatch, нужные source/skill sections и принятую часть плана.
Роли: архитектор → отдельный plan reviewer → исполнитель → отдельный
code/evidence reviewer. Не создавать заново эти четыре роли для каждой API-команды
или перечитывать неизменные мегабайты после status-only commit.
Повторяется только затронутая часть после исправления. При реальной недоступности
работника действует fallback AUTHORIZATION.md, без подмены независимой review.

Различай product defect, test defect, CI/environment issue, настоящий policy
запрет и историческую очистку. Исправление workflow — работа по инфраструктуре
текущего task, а не доказательство выполнения его product assertions.
Отсутствие тестового исполнения закрывает приёмку этого task, но не запрещает
разрешённую независимую подготовку. Принятие S03.02 по-прежнему требует
проверенного identity-контракта S03.01.

После принятия task выбирай следующий dependency-ready task автоматически.
После S03.12 подготовь S04 handoff и продолжай ранее разрешённую инженерную часть
исходной очереди. Этот пакет не содержит новых полномочий на реальные платные
teacher/evaluator calls, раскрытие закрытого экзамена, production deployment или
публикацию модельных весов: для них нужен уже принятый конкретный work order.
Не заканчивай отчёт как будто S03 — весь проект. Если следующего executable
brief нет, подготовь его по исходной очереди, а не придумывай реализацию.

## Прочитать менеджеру, не всем работникам целиком

`AUTHORIZATION.md` — единый источник полномочий и бюджета.
`OPERATING-POLICY.md` — роли, состояния, обработка проблем, продолжение.
`CI-PLAYBOOK.md` — проверочный путь и восстановление CI.
`RESUME.md` — перенос текущей работы без сброса состояния.
`ROADMAP.md` / `tasks.json` — каталог требований и зависимостей, НЕ статус выполнения.
`SKILLS.md` — маршруты чтения; реальную библиотеку не заменяет.

Работнику передаётся его bounded grant с источником принятого полномочия,
ролью, точным write scope, доступной средой и неувеличенным бюджетом.
Каждый свежий работник читает свои нужные источники; не наследует чужую видимость.


---

## Файл: .kiro/xiigen-slices/luna/s03/OPERATING-POLICY.md

# S03 v2 — единые правила выполнения

## 1. Один источник полномочий

Принятый владельцем `AUTHORIZATION.md` определяет допустимые инженерные действия,
Git/Actions операции, среды, бюджет и исключения. Этот документ их не расширяет.
Более поздние прямые ограничения владельца и реальные ограничения инструментов
сохраняются. Копия старого запрета в task/report не возрождает заменённое правило.
Подтверждение принятия фиксируется один раз существующим manager state. Если
владелец уже принял общую серию, v2 не требует повторного подтверждения операций.
Само наличие файла без принятия владельцем не создаёт разрешение на расходы.

Явные замены только для данной работы:
- прежний запрет Git writes в S03 заменён адресной публикацией кандидатов;
- единственный CI run заменён ограниченным циклом исправлений в одном бюджете;
- Windows-only execution заменён допущенными CI-средами из AUTHORIZATION.md;
- фиксированные 30/50 review-проходов и GPT-5.5 defaults заменены принятым
  Luna/xhigh и проверкой всех требований с исправлением замечаний;
- blanket reload после каждой записи заменён identity-check и чтением изменённого;
- менеджер может управлять CI/Git без отдельного архитектора для каждой команды;
- доказанная недоступность исполнителя допускает явно обозначенный parent-executor,
  но не self-approval.

Это не изменение требований к продукту, лицензий, порогов качества или защиты.
Не переписывать AGENTS.md/CLAUDE.md/исходные skills в рамках миграции v2.

## 2. Три разных состояния

Для каждого task в СУЩЕСТВУЮЩЕМ state/report храни отдельно:
`implementation` (not_started/in_progress/present),
`verification` (not_run/running/failed/blocked/passed),
`acceptance` (pending/needs_changes/accepted).
Используй эквивалентные существующие поля; не создавай новый ledger ради названий.
Отдельно укажи blocker domain, affected operation, evidence, next permitted action.

Статически одобренные тесты при executed=0 — implementation=present,
verification=blocked, acceptance=pending. Это не новый кодовый дефект и не PASS.
S03.00 закрывается artifact review без .NET тестов, поскольку это planning task.
S03.12 требует общего requirement-to-evidence review, не только числа тестов.

Порядок: выбрать остаток → reuse accepted plan → закрыть конкретный gap →
проверить нужный candidate → отдельный review → принять → следующий task.
При возврате замечаний сохраняй уже принятые неизменные части. Один автор исходников
одновременно. Документация инфраструктуры не увеличивает product progress.

## 3. Что делать при проблеме

| Ситуация | Действие | Что не делать |
|---|---|---|
| Существующий код и тесты покрывают требование | Проверить эквивалентность и candidate-matched evidence; сохранить реализацию | Переписать ради нового task ID |
| Compiler/test defect | Исправить причину в ограниченном scope, rebuild, rerun, targeted review | Спросить владельца о каждом методе |
| Нужен соседний production-файл | Архитектор обосновывает owner/инвариант/файл; отдельный plan reviewer допускает delta | Вводить несовместимую политику или безграничный рефакторинг |
| CI queued/transient/YAML/validator | Применить CI-PLAYBOOK, reuse product review при неизменном коде | Новый authority-intake и начало S03.00 |
| Policy/401/403/billing denial | Установить точную область; использовать только существующие полномочия; продолжить независимое | Другая оболочка/токен для обхода |
| Новый product/licensing/security decision | Одна конкретная развилка владельцу после чтения источников | Назвать обычное имя класса "решением владельца" |
| Неизвестно, был ли запущен dispatch | Сначала GET с идентичностью invocation/run | Повторить POST наугад |
| Исторический cleanup hold | Оставить отдельным; запрещённые/неизвестные файлы не трогать | Из него вывести запрет всей разработки |
| Reviewer недоступен | Другой отдельный reviewer; при невозможности сохранить pending и работать независимо | Автор объявляет свой код принятым |
| Контекст/сессия заканчивается | Оставить короткий воспроизводимый handoff с run ID, бюджетом, scope и evidence | Обещать незапущенную фоновую работу |

Нет новых вопросов для действий, уже покрытых grant. Реальные 2FA, неизвестные
секреты, чужое approval или расходы сверх лимита не выдаются за решённые.

## 4. Роли и рабочий контекст

Архитектор определяет поведение/owner/точный scope, не редактирует product source.
Plan reviewer проверяет полноту и риски плана отдельно от его автора.
Executor реализует и проверяет; владеет только допущенными исходниками/фикстурами.
Code/evidence reviewer проверяет итоговый diff и raw results отдельно от автора.
Менеджер владеет active/state, обычными GitHub/Git операциями и очередью.
Каждый добавляет только свою атрибутированную часть task report; reviewer verdict
не пишется от его имени менеджером. Прямо обозначать пределы видимости инструмента.

Используй принятый план повторно при совпадении существенных условий. Не прогоняй
все четыре роли на отмену run, загрузку TRX, обновление URI или report-only commit.
Для правки инфраструктурного кода нужен review её delta; для product code — цепочка
ответственных ролей. Исправление типографики в отчёте не запускает .NET gate.

Worker не вернул packet: проверить completion/процесс/diff, прекратить его запись,
назначить замену и передать уже установленные факты. После подтверждённой
недоступности исполнителей manager может быть executor по AUTHORIZATION.md.
Отдельный финальный reviewer обязателен; fallback маркируется parent-executed.

В активном контексте: один task, один role dispatch, relevant skill sections,
текущий scope и source windows, только необходимые результаты предшественников.
Не загружать весь combined document, все 52 dispatch или transcript 5000 строк.
Не ограничивать качество искусственным числом токенов; читать весь изменяемый файл
и полные вызванные методы, следовать применимым обязательным skill dependencies.
Запись requested_model не подтверждает runtime. Отсутствие модельного telemetry
не компенсируется бесконечной проверкой; оно честно остаётся UNVERIFIED.

## 5. Зависимость — это нужный контракт, не общий красный флаг

Сохраняется исходный `depends_on` всех 13 задач как acceptance order.
Принятие задачи требует принятия её обязательных контрактов-предшественников.
Без выполнения этих условий нельзя объявить S03.02/S03.12 accepted.

При инфраструктурном ожидании разрешена независимая source/contract preparation,
проверка тестовых данных и анализ интерфейсов. Код, реально зависящий от
неподтверждённого предшественника, не выдаётся за финально интегрированный.
Обычный новый запуск в фоне не является основанием для второго source writer.
Нет автоматического снятия S02 holds; для S03.05 и final интеграции проверить
конкретный recovery-контракт, а не только равенство старого phase_status.

## 6. Evidence и отчётность

Цепочка приёмки: R-ID → retained/changed production owner и caller → конкретная
assertion → выполненный тест → raw result и candidate identity → отдельный review.
Не требовать изменения production-файла там, где требовалась лишь coverage.
Нулевой executed, platform-return, fake без нужного guard, скопированный ожидаемый
hash, missing results и самопроверка не закрывают требование.

Один текущий краткий summary/указатель в существующем active/state; один report
на task, существующие raw artifacts по run/attempt. Историю не удалять, но не
дублировать её при каждой смене статуса. Текущий summary отделён от архивного.
Записывай проверенный source SHA честно; report-only commit не переименовывает
старый проверенный SHA. Дубликат focused/impacted не удваивает покрытие или счета.

Acceptance product не включает доказательство SAC-совместимости локальной машины
или реальной обученности модели, если это не проверялось отдельным экспериментом.
После S03.12 — S04 handoff и ранее разрешённая инженерная очередь, не конец проекта.


---

## Файл: .kiro/xiigen-slices/luna/s03/CI-PLAYBOOK.md

# S03 v2 — один проверочный маршрут, управляемые повторы

Этот документ — инструкция к существующему CI, не новый runner или workflow.
Разрешение действий/расходов — только AUTHORIZATION.md. Смена среды допускается
как самостоятельная проверка в разрешённом CI, не как обход локального запрета.

## 1. Preflight выполняется при изменении условий, а не при каждом status

До затрат проверь: Actions enabled; workflow enabled и доступен для выбранного
trigger; GitHub-owned actions допущены; необходимые runtime/SDK/фиды доступны;
известна точная платформа обязательных assertions; бюджет/потенциальные дубли учтены;
candidate SHA и workflow revision известны; нет эквивалентного активного run.
Для workflow_dispatch файл должен находиться в default branch; это не значит
сливать весь непринятый backlog. CI-only PR разрешён по protections.

Сохранённый проверенный preflight переиспользовать, пока нет изменения источника
настройки, SDK/workflow или сообщения об отказе. Отсутствие доступа к billing API
не доказывает нулевой бюджет; считать резерв по тарифу без бесплатной квоты.
Если максимальный расход невозможно обоснованно ограничить остатком, не тратить
наугад. Неизвестная авторизация не превращается в новую учётную запись/токен.

Основной S03.01 route — уже созданный Windows CI workflow. Не отправлять старую
локальную DLL под SAC ещё раз и не строить инфраструктуру подписания как часть S03.
При отсутствии локального исходника CI прочитай его обычным GitHub connector/CLI;
не подменять private repo публичным веб-поиском.

## 2. Карта состояний запуска

| Условие | Следующий шаг |
|---|---|
| completed/success | Читать actual results, сверять SHA/definitions/assertions. Не запускать заново ради статуса |
| in_progress и виден прогресс | Ограниченно наблюдать тот же run; не создавать duplicate |
| queued/jobs=0 без прогресса 15 минут после устранения причины | Проверить concurrency/approval/billing/service. При отсутствии объясняющего условия обычный cancel, GET, replacement |
| cancel принят | Это ещё не terminal: GET состояния до замены либо разрешённый ниже case |
| cancel не завершился | Один штатный force-cancel после recheck; не эскалация защиты ОС |
| cancel/force-cancel=409, queued/jobs=0 | Если доступ/политика/оплата не запрещают и зарезервированы оба потенциальных запуска, одна replacement-попытка разрешена; старый остаётся потенциально исполнимым |
| старый ожил позже | Оставить один нужный кандидат, отменить лишний; учесть затраты обоих |
| две replacements для того же scheduling failure не помогли | Не плодить queue. Поддерживаемое исправление, совместимый разрешённый image или одно Support обращение; затем независимая безопасная работа |
| 401/403 или account suspension | Точный диагноз доступа; не новые токены/вторая учётная запись/смена repo для обхода |
| waiting/approval | Разрешённое собственное approval, если политика позволяет; чужое/SSO/protection не обходить |
| transient runtime/network без изменения candidate | Один-два обоснованных rerun, все результаты сохранить |
| code/workflow/fixtures изменены | Commit нового кандидата и новый dispatch; rerun старого события не подхватывает новые исходники |
| dispatch response неоднозначен | Найти run по repo/workflow/ref/actor/времени/уникальному invocation id, затем решать о повторе |

15 минут и максимум две replacements — операционные пороги принятого задания,
не гарантии платформы. Уже прошедшее подтверждённое ожидание не обнулять.
Интервалы GET выбирать умеренные, учитывать API backoff/Retry-After. Не опрашивать
busy-loop и не спать бесконечно без progress check. Ветка восстановления только
для собственных runs разрешённого workflow; чужие очереди не отменять.

## 3. Версии и стоимость

Отдельно фиксируй исходный workflow ref/blob, input candidate SHA, actual checkout
SHA, run/attempt/job и итоговые DLL hashes. `head_sha` API не подменяет actual
checkout, если workflow принимает отдельный candidate input.
Не отправлять дубликат после уже успешного POST. Для API с веткой вместо SHA
в invocation гарантировать, что исполняемая версия workflow и выбранный кандидат
соответствуют рассмотренному коду, либо исправить явную version-binding ошибку.

Один плановый compute job; transient overlap дубликатов учесть. Резерв 20 USD общий
для ранее принятой серии, не на task. Оценивать по актуальному официальному тарифу
и upper bound timeout, плюс artifacts/вызванные push-triggered jobs. 45 минут по
умолчанию; до 90 только при полезном прогрессе и достаточном резерве. Лимиты
аккаунта не менять. Не нужен новый вопрос на каждый run внутри оставшегося лимита.

## 4. Ошибки сборки и проверки

Сначала первый реальный diagnostic, фактическая команда/числовой exit. Не угадывать
ModelConfig, field names, SDK или namespace. Existing reference/project file —
источник. Setup SDK, restore, build и test используют одну выбранную конфигурацию;
setup-dotnet сам по себе не доказывает active SDK — сохранить dotnet --info/version.
На Linux multi-targeted restore при необходимости использовать установленное
`-p:EnableWindowsTargeting=true`; это не запуск Windows UI и не замена platform proof.
Не повышать target framework/версии зависимостей без обусловленного требования.

Rebuild после source edit до --no-build. Никогда не ждать TRX после failed build.
Ошибку YAML/context/validator исправить минимально и отдельно review delta;
не перестраивать весь supervisor. Ошибку теста или production guard вернуть
исполнителю; обычный ограниченный scope расширяет architect/plan reviewer.
Для flaky результата исправлять источник, не выбирать удачный повтор.

## 5. TRX — проверка именно выполнявшихся assertions

Проверять реальный формат установленного VSTest, не выдумывать Counters/@skipped.
Использовать его стандартные поля total/executed/passed/failed/notExecuted/aborted
и остальные обязательные поля по фактической схеме; одних дополнительных нулей
недостаточно. Числа неотрицательны, total>0, total=executed=passed для полного gate;
UnitTestResult.count согласован; нет Failed/NotExecuted/Aborted/Error/Timeout или
run-level incomplete outcome. Каждый required case связан с единственным
определением по testId, полным class+method, а для Theory — также test-case identity.
Несколько Theory rows не объявлять ошибочным duplicate метода; каждая требуемая
строка должна быть выполнена. Для восьми S03.01 facts требовать все восемь исходов.

Валидатор проверить на successful native TRX и synthetic negative fixtures:
zero tests, missing required case, skipped result, failure, malformed XML,
counter/result mismatch и duplicate case identity. Это проверка валидатора,
не fake продуктовый PASS. Файлы полученных реальных результатов не исправлять.
Platform early-return с Passed не доказывает находящуюся ниже assertion.

Focused/impacted с одинаковым набором may share один полный run при reviewed
mapping обеим обязанностям. Не выполнять два одинаковых платных прогона без цели.
Повторы самого измерительного эксперимента, заложенные в product contract,
не сокращать. Финальный S03 union — все фактические обязательные cases, не список
несозданных suggested class names из v1.

## 6. Сохранение и закрытие

Raw logs/TRX/metadata загружать и при сбое; отсутствие artifact не превращать
в success. Скачать нужные доказательства до retention expiry. Не коммитить DLL,
cache/весь checkout/секреты/закрытый corpus как доказательства. Сохранить отдельную
identity требований и входов тестов; скрытый fixture/config, исключённый из общего
fingerprint, требует собственной проверки эквивалентности.

После успешного gate отдельный reviewer читает diff и evidence. Только тогда
task accepted. Published candidate != accepted task; CI green != реальное качество
модели. Неизменный report-only commit не повод повторять сборку, если source/config/
fixtures/workflow equivalence установлена и исходный tested SHA сохранён.

## Первичные справочники для точного API

Сверены при подготовке v2 2026-09-15, не являются проверкой текущего private run:
- https://docs.github.com/en/rest/actions/workflow-runs
- https://docs.github.com/en/actions/how-tos/manage-workflow-runs/re-run-workflows-and-jobs
- https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow
Текущие цены перед резервированием брать из официальной billing reference.


---

## Файл: .kiro/xiigen-slices/luna/s03/RESUME.md

# S03 v2 — продолжить уже сделанное

## Наблюдения из переданной истории, а не свежая проверка репозитория

| Объект | Сообщённое состояние | Действие при входе |
|---|---|---|
| S03.00 | planning milestone принят; назван commit de2d167e0c | Проверить доступный report/review и сохранить принятие; не повторять задачу |
| S03.01 | 8 identity cases в существующем EndpointTests, static review; runtime не принят | Искать новый evidence, иначе закрыть проверочный остаток; не создавать дублирующий класс |
| Windows local | DLL была блокирована SAC/Code Integrity | Сохранить причину, не запускать те же байты снова и не искать signer заново |
| Windows CI | workflow был опубликован; Actions включён по сообщённому readback | Одно чтение текущего run/workflow; не применять старое enabled=false как факт |
| Run 34939441448 | queued/jobs=0 в последнем переданном отчёте | GET текущего состояния; success -> evidence, stale queue -> CI-PLAYBOOK |
| Ссылка candidate aa2c4414d9998e824e4e860646e34210bfc1f19e | candidate существующего run | Не reset к нему и не называть текущим HEAD без проверки |
| S02/Old16/New12 | общая приёмка/cleanup были открыты | Сохранить, не присваивать чужое и не делать универсальным запретом S03 |

Мы не выполняли свежий GitHub query, SDK build или .NET test при изготовлении v2.
Значения выше не импортируются автоматически как machine status.

## Конечный алгоритм входа

1. Проверить actual root/branch/HEAD и относящийся к задаче local diff. Сохранить
   незакоммиченный S02.FINAL-EVIDENCE-execution.md; не включать в S03 candidate.
2. Перечитать только актуальные state pointers и последние относящиеся к S03.01
   результаты. Предыдущие неизменные product reviews переиспользовать.
3. Если S03.01 уже accepted с достаточным actual evidence — следующий task.
   Если implementation present и verification pending — выполнять только gate,
   необходимые технические исправления и отдельный окончательный review.
4. Для existing run считать его текущие поля/jobs/attempt. Если он завершён,
   не отменять на основании старой строки queued. Если он заменён — читать
   записанный replacement. Не создавать новый run только из-за миграции пакета.
5. Применить CI-PLAYBOOK. Нужные cancel/replace/rerun/code correction уже в grant.
6. При PASS принять S03.01 и перейти к S03.02 по реальным зависимостям. Во время
   infrastructure wait допустима независимая подготовка, но не ложная приёмка.

Это конечное чтение для возобновления, не новый planning milestone. Не добавлять
заново review packet только чтобы подтвердить уже подтверждённое право.

## Контекст S03.01: фактические восемь методов из истории

Сверить существующие definitions, а не создавать их по этому перечню:
- CodeDocumentationModelAssessment_IdentityPositiveControlUsesCanonicalEvaluationEvidence
- CodeDocumentationModelAssessment_IdentityRejectsValidOutputWithoutPermit
- CodeDocumentationModelAssessment_IdentityRejectsAuthoringProfileWithValidEvaluationProvenance
- CodeDocumentationModelAssessment_IdentityRejectsMutatedModelId
- CodeDocumentationModelAssessment_IdentityRejectsMutatedManifestDigest
- CodeDocumentationModelAssessment_IdentityRejectsMutatedLayerDigest
- CodeDocumentationModelAssessment_IdentityRejectsMutatedEvaluationProfileId
- CodeDocumentationModelAssessment_IdentityRejectsEvaluationProfileConfigurationHashDrift

Owner reported: Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs.
Исходный proposed CodeDocumentationEvaluationIdentityTests — допустимый старый
design option, не обязательное новое имя. Привязать actual cases к тому же R1.


---

## Файл: .kiro/xiigen-slices/luna/s03/ROADMAP.md

# S03 v2 — те же 13 требований, исправленный цикл выполнения

| Task | Human outcome | Depends on | Default classification |
| --- | --- | --- | --- |
| S03.00 | Admit S03 and freeze the local work contract | Entry | Planning |
| S03.01 | Pin evaluator identity and keep authoring authority separate | S03.00 | Reuse-first validation/repair |
| S03.02 | Freeze the exam contract and canonical case set | S03.00, S03.01 | Reuse-first validation/repair |
| S03.03 | Reject invalid evaluator responses without partial success | S03.00, S03.01 | Reuse-first validation/repair |
| S03.04 | Enforce the complete call budget, unique receipts and deadline | S03.00, S03.01, S03.03 | Reuse-first validation/repair |
| S03.05 | Reject changed exam conditions before resuming evaluation | S03.02, S03.04 | Reuse-first validation/repair |
| S03.06 | Preserve uncertainty decisions and distinguish certification referral | S03.02, S03.04 | Reuse-first validation/repair |
| S03.07 | Define clean certification admission without relabeling HeldOut | S03.00, S03.02, S03.06 | New boundary only if no equivalent exists |
| S03.08 | Persist certification purpose and exposure state safely | S03.07 | New boundary only if no equivalent exists |
| S03.09 | Keep closed exam material out of authoring and public feedback | S03.03, S03.07, S03.08 | New boundary only if no equivalent exists |
| S03.10 | Execute independent certification on an admitted saved checkpoint | S03.04, S03.06, S03.07, S03.08, S03.09 | New boundary only if no equivalent exists |
| S03.11 | Wire certification through server-owned configuration and safe responses | S03.10 | New boundary only if no equivalent exists |
| S03.12 | Close S03 with candidate-matched integration evidence and S04 handoff | S03.01, S03.02, S03.03, S03.04, S03.05, S03.06, S03.07, S03.08, S03.09, S03.10, S03.11 | Final integration/evidence |


## Продолжение

Это каталог, а не запрос повторить всё с нуля. Читай RESUME.md: S03.00 может быть
уже принят, S03.01 — реализован, но ожидать CI/evidence. Сначала остаток текущей
задачи. `depends_on` сохраняет исходный порядок приёмки, не запрет независимой
подготовки при блокировке инфраструктуры.

Менеджер применяет AUTHORIZATION.md / OPERATING-POLICY.md и восстанавливает CI
по CI-PLAYBOOK.md без нового вопроса на cancel/retry/обычную правку. Нормальная
приёмка остаётся после required tests и отдельного review. No status reset from
this catalogue. Historical cleanup holds remain separate.

S03.12 завершает инженерную приёмку этой фазы и даёт S04 handoff; он не означает
реальный сертификат модели или завершение S04–S09. Ранее разрешённую инженерную
часть очереди продолжать; отдельные live experiments — по их конкретному допуску.


---

## Файл: .kiro/xiigen-slices/luna/s03/SKILLS.md

# S03 v2 — роли и фактическое чтение навыков

This file is an index, not a replacement for the actual skill documents. The
principal windows were read when v1 was prepared; the whole library was not loaded or
independently audited here. Existing repository sources remain unchanged.

| Principal skill | Path | Inspected kickoff lines | Task use |
| --- | --- | --- | --- |
| architecture | docs/skills/trainable-architecture-decision-SKILL.md | [[1, 160]] | Owner, existing/proposed distinction, actual data flow and failure boundaries. |
| planning | docs/skills/phase-planning-SKILL.md | [[1, 110]] | Live scope, dependency-ready tasks and implementation evidence map. |
| plan_review | docs/skills/plan-review-SKILL.md | [[1, 115]] | Contract completeness, no invented authority, all branches before execution. |
| code_review | docs/skills/generated-code-review-SKILL.md | [[1, 140]] | Real callers and assertions, evidence matched to the final candidate. |
| human_language | docs/skills/output-readability-gate-SKILL.md | [[1, 130]] | Human behavior first, technical evidence second; no jargon-only status. |

## Required routing and dependencies

| Existing route/path | What to resolve/read |
| --- | --- |
| docs/skills/planning-session-startup-SKILL.md | SK routes and the currently applicable session/delegation contract |
| docs/SESSION-START-PROMPT.md | Current work-order and authority boundary; this is under docs/, not the repository root |
| docs/skills/architect-behavior-classifier-SKILL.md | Intent and deterministic validation versus learned behavior |
| docs/skills/authority-requirement-binding-SKILL.md | Bind requirements to the current user scope rather than quoted history |
| docs/skills/reference-inventory-and-source-map-SKILL.md | Exact source/revision/line references |
| docs/skills/no-product-decisions-SKILL.md | Do not silently replace target, evaluator/provider or product scope |
| docs/skills/human-readable-architecture-audit-SKILL.md | Owner/data-flow/normal/failure explanation before implementation |
| docs/skills/claim-verification-SKILL.md | Separate source, execution, review and real-world claims |
| docs/skills/test-integrity-SKILL.md | Do not weaken, disable or misreport tests |
| .skills/code-execution--dev-safety-SKILL.md | Workspace/write/test safety; do not execute unrelated TypeScript examples |
| .skills/DNA-DOTNET-TRANSLATION.md | Applicable local .NET guidance; do not import another product line |
| .skills/code-execution--test-integrity-SKILL-v2.2.0-GR001.md | Actual assertions, cleanup and evidence integrity |

Read AGENTS.md/CLAUDE.md applicable entry, workspace, live-authority, role and
verification sections. Read source-complete affected methods/files; skill reading
may be scoped by role under the adopted S03 task method. Resolve current required
skill dependencies rather than recursively loading every unrelated historical flow.
The architect adds implementation-documentation/algorithm/source-reference skills
only when the actual task changes those corresponding artifacts.

## Four passes with different jobs, not invented pass counts

Each task uses a real architect, separate plan reviewer, executor and separate
code/evidence reviewer. These are four roles, not four independent code reviews.
In-scope corrections cycle between the responsible writer and reviewer. No minimum
50-pass claim or count inflation is created by this pack.

The start prompt explicitly proposes extending the owner's previously adopted Luna
method and coverage-driven review to S03. This task-scoped adoption replaces inherited
GPT-5.5/fixed-pass defaults for this batch only; it does not edit or erase governance.
A newer explicit user requirement takes precedence. If the user has not adopted
that method, do not silently treat this index as an override.

## Context practice

A fresh worker reads its one task and one role dispatch, current contract, necessary
predecessor result and applicable skills/source. Do not load the combined human
reference document into each worker. Retain source ownership, invocation order,
forbidden operations, expected observations and failure handling when compacting.
A source summary is navigation, not a substitute for reading code being changed.
Avoid hard token claims: this pack has no measured optimal token budget or Luna
benchmark. Split an oversized task by an observable invariant instead of excluding
mandatory rules to fit it.

## Human-language output

Use the operator's current language (Russian in this continuation). Preserve canonical
Russian comments/user messages and the project's language rules where relevant;
do not translate them as a side effect. Each report opens with the behavior/result,
then evidence. Say "a changed evaluator is rejected before calls" before internal
error codes. Explain uncertainties and exact remaining work without copying the
entire execution transcript.

## Операционная дельта v2

Полный текст навыков и их продуктовые требования не редактировались. Принятый
AUTHORIZATION.md и START-PROMPT дают task-scoped operating overrides. Каждый
fresh context читает нужное сам; в том же контексте неизменные источники
переиспользуются с identity-check. Нельзя требовать load всех исторических traces
после update live status. Для API cancel/download не нужна product review; для
изменения source и финальной приёмки разделение ролей сохранено.

Воркеры получают конкретный grant, нужные dependency sections, свои code windows,
принятый scope и required observations. Не загружать весь all-tasks reference.
Отдельный plan reviewer не повторяет принятый план без изменения его существенных
условий. Нет нового измерения "оптимальных токенов" или benchmark Luna.


---

## Файл: .kiro/xiigen-slices/luna/s03/REQUIREMENTS.md

# Original requirement → small task map

The R IDs are local traceability labels created for this pack, not pre-existing requirement numbers.

| ID | Requirement | Meaning | Source basis | Task coverage |
| --- | --- | --- | --- | --- |
| R1 | Approved independent evaluator identity | Same approved evaluator/configuration across before/after/resume; preserve real profile/provenance guards. | Original queue S03; active program steps2-3/9; actual I/A/C/G implementations. | S03.00, S03.01, S03.02, S03.03, S03.04, S03.05, S03.11, S03.12 |
| R2 | Fixed cases and baseline reuse | Canonical case/conditions identity, original saved baseline, no recalibration or duplicate pair update during same-run recovery. | Original queue S03; active existing-lesson branch; assessment/calibration/coordinator. | S03.00, S03.02, S03.05, S03.12 |
| R3 | Complete bounded measurement | All admitted repeated calls and actual unique receipts; no successful partial average or uncounted retry. | Existing C/G/I and HT tests; decomposition expands their preserved invariants. | S03.00, S03.03, S03.04, S03.05, S03.12 |
| R4 | Noise-aware decision and separate final target | Improvement beyond both measurements; sufficient diagnostic baseline refers to independent certification, not another pair or automatic certificate. | Active program step4/common acceptance; G/N source. Strict boundary discrepancy explicitly preserved for review. | S03.00, S03.06, S03.10, S03.12 |
| R5 | Clean certification purpose | Teacher-visible diagnostics excluded from unseen certification; Train/HeldOut alone does not establish purpose/history. | Original S03 row. New admission/session types in this pack are proposed designs, not existing APIs. | S03.00, S03.07, S03.08, S03.09, S03.10, S03.11, S03.12 |
| R6 | No exam/paid outputs as DPO sides | Separate teaching input and examples; closed exam excluded from authoring; preserve actual paid teaching-reference use without copying it into pair sides. | Active program steps5/7; F/D/I source. | S03.00, S03.07, S03.08, S03.09, S03.10, S03.12 |
| R7 | Bounded truthful disclosure | Evaluator may receive isolated closed reference; human/public output cannot create false certification or leak prohibited closed content. | A parser/prompt and H response sources; restricted certification projection is a proposed extension. | S03.00, S03.03, S03.09, S03.10, S03.11, S03.12 |
| R8 | Actual wiring and candidate-matched acceptance | Real callers, disk state, tests/assertions, independent review, no publication/quality overclaim, S04 handoff remains. | Original queue and inspected principal skills; prior S01/S02 history is evidence only. | S03.00, S03.08, S03.10, S03.11, S03.12 |

## Source facts versus proposals

Source facts: fixed evaluator/cases/conditions, no same-run recalibration/retraining,
noise-aware decisions, clean final certification, separate teaching examples, no
exam/paid output as DPO pair sides, existing server/evaluator/provenance contracts.

Proposed decomposition/design: the S03.00–S03.12 numbering, proposed certification
admission/session/service classes, a certification-specific restricted response,
and the conservative no-retry-until-pass session rule. The exact owner/schema/path
must be reconciled with current equivalent code before implementation. The program
does not supply those class names, wire formats or a complete exposure database.
Do not cite them as already implemented or as verified historical requirements.

The program's 95%-with-uncertainty wording and the code's strict noise comparison
are presented separately. This pack preserves the current strict predicate; it
does not silently replace policy. Any deliberate product-policy change must be
explicit and versioned before implementation, not decided after seeing results.

## Boundary after S03

S03 code acceptance is not actual student certification or evidence of improved
model quality. S04 remains the original real measured-lesson milestone and requires
its own actual data, model, budget/provider/training authority and evidence.
S05–S09 remain in the original roadmap. No later requirement is deleted or marked
complete by this pack.


---

## Файл: .kiro/xiigen-slices/luna/s03/PROPOSED-CONTRACTS.md

# Concrete design choices to freeze before new-contract tasks

These are **proposals for the missing S03 boundary**, not claims that these names
or schemas exist in the repository. S03.00 searches the current selected path and
uses an equivalent existing owner when it meets the requirement. The architect
must freeze any remaining exact signatures/wire names before executor release;
no executor may guess an existing API or model configuration type.

## Existing architecture retained

1. ModelServer owns the external request/profile boundary.
2. AssessmentService generates from saved student weights and measures each case.
3. CalibrationService repeats the same measurement and constructs existing noise
   evidence; QualityGateService owns baseline/trial improvement/referral decisions.
4. The evaluation-only client legitimately receives the closed reference. The
   authoring client receives separate approved teaching input, never that exam.
5. Coordinator preserves the already-applied lesson's saved baseline and does not
   train the pair twice. S02 owns weights/optimizer/RNG recovery.

## Proposed minimal added responsibilities

Layer direction: ModelServer (25) -> certification orchestration in TrainableRouter (26) -> ContinuousLearning (14) assessment/admission/session storage. Never introduce 14 -> 26 or 14 -> 25. The new CertificationService belongs in 26; pure admission and session storage belong in 14.

| Owner | Contract to freeze | Specifically not its job |
| --- | --- | --- |
| CodeDocumentationCertificationAdmission | Immutable validated purpose, declared provenance-scope completeness, source disjointness, checkpoint/corpus/evaluation-policy tuple. Controlled construction, no caller-set clean bit. | Scoring, training, model promotion, universal semantic-overlap detection. |
| CodeDocumentationCertificationSessionStore | Durable reservation before calls and completion receipt after full measurement; trusted expected identity; no reset of exposure by renaming a run. | Model checkpoint storage redesign, another process runner, rewriting old evidence. |
| CodeDocumentationCertificationService | Admission -> reserve/readback -> existing calibrated evaluation -> target/noise decision -> complete/readback -> safe result. | Authoring a pair, fine-tuning, replacing the evaluator, tuning on certification results. |
| ModelServer certification projection | Strict request and safe aggregate receipt; separate configuration for the independently frozen certification set. | Accepting caller-provided scores/checker/corpus/clean status; exposing gold or diagnostic per-case feedback. |

Prefer local owned value types rather than a global FrozenReferencePartition enum
migration. The source currently has Train/HeldOut, which remains the training split.
The same topic/domain does not imply identical source; source-fingerprint, normalized
content and declared lineage checks are limited to their actual provenance coverage.

## Admission contents

Reuse the inspected CodeDocumentationSavedModelAssessmentModelPassport,
RepositoryOwnedReferenceCorpusManifest and CodeDocumentationSavedModelAssessmentGenerationSettings
where appropriate; read their full current contracts before coding. New factory
signatures remain explicitly proposed until the architect freezes them. The
semantic fields required are:

- Versioned purpose and bounded CodeDocumentation source scope.
- Exact accepted checkpoint identity, including saved bytes, not only ModelId.
- Certification corpus manifest/version/hash and canonical ordered cases.
- Trusted source/exposure catalogue identity, its covered scope and completeness.
- Actual evaluator model + manifest/layer/profile identity, generation conditions,
  canonical repeats, budget, metric/target/noise-policy identity.
- Owning server/admission provenance; immutable identity derived from validated
  values, not a self-signed HTTP assertion.

A missing field in a legacy record is not silently filled with a current expected
value. Validate current compatibility and fail closed for a clean claim when proof
is absent. Hashes establish integrity relative to a trusted expected value; they
are not independent attestation of origin or absence of prior exposure.

## Session lifecycle

Proposed conservative default: a clean certification admission is reserved for one
fixed candidate and predeclared series before the first call. Reservation uniqueness
is keyed by the admission/clean-set identity, not merely a user-selected run ID.
A changed candidate cannot reuse it by choosing another run name. Exact completed
replay returns the validated prior result, explicitly labelled as reused evidence.

A reserved session without a complete valid receipt is incomplete/ambiguous, never
fresh/clean. Do not retry until pass. If current source already provides a principled
continuation rule for the exact same predeclared immutable series, the architect may
reuse it with its existing budget/exposure guarantees; do not invent that rule after
seeing scores. Persist attempted-call status where required by that contract.

Use existing safe bounded storage and scope ownership. A create-new reservation
plus a create-new completion receipt can avoid rewriting historical identity;
these two records must have one owner/versioned contract, not competing manifests.
The proposed metadata root is training-artifacts/assessments/<domain>/<design-pattern>/;
S03.00 reconciles the exact layout before code. This is not a claim that the root
already exists. Model weights/DPO data remain under their original canonical roots.

## Pure versus exposed material

| Material | Isolated evaluation client | Teaching/authoring client | Public certificate |
| --- | --- | --- | --- |
| Closed certification input/reference | Only within the admitted evaluation session | No | No detailed payload |
| Student final-exam answer | Evaluator input | Not reused for teaching/tuning | No per-case preview in the proposed final projection |
| Diagnostic teaching feedback | According to diagnostic route | Only independently permitted teaching material; never relabel as clean exam | Not final certification evidence |
| Separate paid teaching reference | Not a certification reference by default | Existing approved context use remains | Never copied into a DPO side or final receipt payload |
| Completed aggregate certificate | Safe projection/receipt | Not a way to extract exam content | Yes, bound to exact measured candidate and conditions |

Do not read real exam contents into development packets. Contract tests use distinct
synthetic sentinel text to demonstrate recipients and rejection. They do not prove
that the underlying LLM can never leak a paraphrase.

## Target-policy boundary

The program says 95% with uncertainty; current code classifies (mean - 0.95) using
the project's noise policy and requires a strictly proven positive difference for
referral. This pack does not change that criterion or introduce a new statistical
formula. S03.06 documents equality behavior and obtains any genuine policy decision
before implementation. Diagnostic referral is never itself a final certificate.


---

## Файл: .kiro/xiigen-slices/luna/s03/TESTING.md

# S03 v2 — обязательная проверка, не обязательная локальная машина

## Контракты не изменены

Сценарии и required observations в разделе 6 каждой задачи сохранены из v1.
Границы данных/ролей и R1–R8 в REQUIREMENTS.md не изменены. Меняется способ довести
проверку до конца: существующий разрешённый CI, управляемые повторы, точные версии.

Команды формируются из действительного Tests/LlmFromScratch.Tests.csproj,
конфигурации, SDK и обнаруженных tests. Старые suggested class names — только
предложения. S03.01 использует восемь уже сообщённых EndpointTests cases.
Последующие gates должны расширять actual scope, не вечно выполнять только их.

`CI-PLAYBOOK.md` определяет execution, `AUTHORIZATION.md` — полномочия/стоимость.
Не заменять текущий runner файлом из старого ZIP. Не вводить новый supervisor
ради одного s03 gate. Исправлять конкретный workflow/adapter defect разрешено.

## Сильные тесты

Каждый negative test начинает с валидного setup, меняет один фактор и доказывает
достижение предназначенного guard. Positive control не позволяет always-reject
реализации удовлетворить suite. Spy/recording client наблюдает реальные factory,
permit, trainer, checker и publication boundaries. Не мокать сам проверяемый
loader/guard/journal успешным ответом.

Сохранять baseline, trial/parent identity и настройки по contract. Не копировать
actual output в expected и не вычислять единственную expected identity тем же
product методом без независимого oracle. Не снижать repeats, корпус, пороги или
precision ради скорости. Не смешивать policy-уровень CI с измерительным шумом модели.

## Evidence

Source evidence — guard и утверждение существуют. Executed contract evidence —
конкретный candidate достиг assertion в указанной среде. Real quality evidence —
отдельный разрешённый эксперимент с реальной моделью/корпусом. Не объединять их.

Raw TRX/логи/exit codes, workflow/candidate/actual checkout/DLL identities,
полные case definitions/results и отдельный review нужны для acceptance.
Схема TRX берётся от установленного logger; строгая проверка должна принимать
нативный successful result и отвергать incomplete/zero/missing/failed outcome.
Не вводить обязательный несуществующий skipped attribute. Подробнее CI-PLAYBOOK §5.

## Финальная проверка S03.12

Reviewed union всех фактических S03 cases плюс обязательные impacted recovery
contracts. Одинаковые sets не удваивать; experimental repeats не устранять.
При совместном финальном кандидатe результаты этапов можно переиспользовать только
при доказанной relevant input equivalence. Реальный новый код требует проверки
затронутых компонентов. В финальном review каждая строка R1–R8 закрывается evidence
или остаётся явно незакрытой; числа tests сами по себе coverage не доказывают.

## Фикстуры

Каждый run владеет своими точно названными scratch/fixtures; finally и static-hook
restoration обязательны. Evidence вне удаляемого scratch. Старые Old16/New12,
неизвестные roots, caches и реальные model/data не disposable. Не устранять cleanup
проблему глобальным delete/ACL change. CI VM disposal не доказывает cleanup
локального диска и не отменяет проверку finally в тесте.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.00.md

# S03.00 — Admit S03 and freeze the local work contract

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** Identify the first genuine S03 gap, preserve completed S02 work, and make all later prompts executable against the actual checkout.

This is task **1 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** None; this is S03 entry preparation.


**Возобновление:** если этот planning milestone уже принят, сохранить его и перейти
к остатку S03.01 по RESUME.md. Не создавать повторный admission ради v2.

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** Identify the first genuine S03 gap, preserve completed S02 work, and make all later prompts executable against the actual checkout.

**CURRENT SOURCE / EVIDENCE:** The original queue asks for fixed independent evaluation and clean certification. The latest inspected S02 executor packet still has unverified coverage rows and historical cleanup holds. Source already contains assessment, repeated calibration, evaluator receipts and a same-run recovery path.

**TARGET WORK (proposed decomposition of the original requirement):**

Read the original S03 row and active training-program opening. Recheck HEAD and the active S02 decision; do not execute old transcript commands. Record S03 preparation separately from any dependency-sensitive implementation admission.
Produce one concise S03 contract in the existing work record: requirement -> current owner/caller -> existing test -> actual gap -> assigned task ID. Resolve all symbol-only source entries needed by the next task to exact current line ranges. Search the selected CodeDocumentation path and its tests for existing certification-purpose, exposure and receipt contracts before accepting proposed new names.
Select this minimal design unless a current equivalent is found: preserve Train/HeldOut as the data split; add a CodeDocumentation-owned certification purpose/admission contract rather than globally reinterpreting the enum. Reuse the existing evaluator, calibration and noise classifier. Keep assessment authority server-owned. New certification metadata is not new model weights or a DPO dataset.
Before releasing the new-contract tasks, fix exact class ownership, constructor visibility, versioned wire fields, server-owned trust roots, metadata path, replay semantics and final-threshold predicate in this one contract. A proposed default path is training-artifacts/assessments/<domain>/<design-pattern>/<assessment-run>/; it is a NEW proposal to reconcile with the layout, not an existing canonical route. Prefer an equivalent existing owner/path.
No open provider/model choice is needed: preserve the approved configuration and its digests. An incompatible policy change requires an explicit decision; ordinary API naming is the architect's job, not another user question. Stop mapping once the enumerated S03 requirements and next task are covered; do not inventory the whole repository.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R1, R2, R3, R4, R5, R6, R7, R8; see `REQUIREMENTS.md`.
**Exit predicate:** An accepted S03 contract and next dependency-ready task, not an assertion that S02 or S03 is implemented.

No product or test source changes in this task.

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.00.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| Q | `.kiro/xiigen-slices/04-QUEUE.md` | 1–30 | S03.*, S04, Do not prefill future slices | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| P | `llm mvp/design to code session/09.09.26WorkLeft-TASK-PROMPTS-TRAINING.md` | 1–34 | active opening before historical details | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| S2 | `.kiro/xiigen-slices/work/S02.FINAL-EVIDENCE-execution.md` | 1–80 | Result and boundary, matrix | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| A | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | 1–290, 315–670, 644–815 | AssessAsync, SnapshotHeldOut, EvaluateCaseAsync, BuildCheckerPrompt, ParseCheckerOutput | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| C | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs` | 1–455 | PlanCalibration, CalibrateAsync, ConditionsMatch, EvaluatorMatches | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| G | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs` | 1–425 | PlanQualityGate, CalibrateBaselineAsync, BaselineRequiresNewTrainingPair, EvaluateTrialAsync, SameExamConditions | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| F | `14.ContinuousLearning/Core/FrozenReferenceCorpus.cs` | 1–270, 325–750 | FrozenReferencePartition, PaidReferenceProvenance, FrozenReferenceEntry, FrozenReferenceCorpus.Build | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| I | `14.ContinuousLearning/Core/LegalTrainingTeacherInference.cs` | 1–260 | ILegalTeacherEvaluationInferenceClient, ITrainingTeacherInferenceClient, LocalQwenOllamaInferenceProfileAuthority | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| H | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentEndpoint.cs` | 1–230 | CodeDocumentationSavedModelAssessmentHttpRequest, MapCodeDocumentationSavedModelAssessment, ExecuteAsync, ExecuteCalibrationAsync | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HP | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentConfiguration.cs` | 1–280 | CodeDocumentationSavedModelAssessmentProfileResolver, ParseProfile, SettingsAreWithinServerBounds | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HT | `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs` | 1–265, 265–660 | CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority, CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker, CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport, CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient, CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| D | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.cs` | 1–250, 320–625 | ResolveOrAuthorAsync, AuthorPairsAsync, AuthorOnePairAsync, BuildTeacherPrompt | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| R | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs` | 529–753, 1002–1264, 1298–1521 | TrainAndPublishAsync, ResumeQualityDecisionFromExistingAttemptAsync | Method windows inspected at prior revision 67c22705; the current tree has the same Git blob 7dccd4ed. Reuse is explicit, not a new full-file read. |
| RT | `Tests/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests.cs` | Resolve the named definition locally; no invented line pin | SameRunRecoveryRemainsEvaluationOnlyWithoutTrainerUpdate, CreateQualityGateContext, CreateAppliedButUnpublishedAttemptAsync | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |
| J | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.cs` | 1–155 | WriteAndReadBack, OpenExisting, MatchesSingleTrainingPair | Current state/receipt definitions and write-before-training prefix inspected. Expand OpenExisting and complete validation methods locally. |
| CF | `14.ContinuousLearning/Training/RepositoryOwnedReferenceCorpusManifest.cs` | 1–175 | RepositoryOwnedReferenceCorpusManifest | Current manifest/open-result definitions, bounds and sealer entry inspected. Expand full read/seal helpers before editing. |
| LAY | `14.ContinuousLearning/Training/RepositoryOwnedTrainingArtifactLayout.cs` | Resolve the named definition locally; no invented line pin | RepositoryOwnedTrainingArtifactLayout | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Correct scope | S02 recovery is retained; S03.01 adds only an uncovered evaluator-identity regression. |
| Resolved contract | The packet names the existing evaluator interface, corpus sealer, admission owner, schema/version and exact caller before code work. |
| Honest admission | S03 planning is complete; coordinator integration remains waiting for the unresolved S02 dependency, not reported as accepted. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Rebuild recovery | Duplicates S02 instead of implementing evaluation integrity. |
| Start with an all-new evaluator | Ignores the source-confirmed service, profile authority and admission path. |
| Copy every history into each worker | Expands context without resolving the current task; can reactivate superseded authority. |
| READY with design TBD | Pushes unresolved trust, persistence or route contracts onto the executor. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Existing implementation | Locate a current assessment guard plus an assertion that exercises it. | Classify source and executed evidence separately; skip duplicate implementation only when equivalent coverage is established. |
| S02 still not accepted | Inspect its current final record, not a historic status string. | Preserve its hold; identify exactly which S03 preparation/independent task is permitted and which integration depends on admission. |
| Proposed API collides | Find an existing certification owner during source search. | Bind later work orders to that real owner; do not create a parallel type. |
| Drift | HEAD differs from the inspected pin. | Refresh relevant ranges and code facts; no reset, repin of history, or broad re-investigation. |

Planning task: actual artifact/source-map review, not application tests.
При уже принятом S03.00 проверить только конкретный drift и нужный migration delta.
Новая версия prompts не требует повторного product build или обнуления статуса.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Resolve existing versus proposed owners and the dependency edge; produce the finite requirement map and contracts. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.00/architect.md` |
| plan-reviewer | Check every source requirement has a task and that no unresolved API/trust/policy choice is hidden in later execution. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.00/plan-reviewer.md` |
| executor | Only after plan admission, reconcile the allowed work records and exact context ranges; no product edits or builds for this planning-only task. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.00/executor.md` |
| code-reviewer | Review the planning artifact against actual source and task coverage; call this artifact review, not code/test acceptance. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.00/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- An active S02 writer exists: do not start a second writer.
- No native separate contexts: supply role handoffs and report the method gap.
- Unknown genuine certification provenance: prepare code using synthetic fixtures, but do not declare real data clean.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.01.md

# S03.01 — Pin evaluator identity and keep authoring authority separate

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** Make every accepted assessment attributable to the approved evaluation model, exact digests and evaluation profile—not an authoring response or an unverified model label.

This is task **2 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.00


**Возобновление:** сохранить восемь существующих EndpointTests identity-cases.
Сначала закрыть runtime/evidence остаток; новое имя тестового класса не обязательно.
Список ожидаемых методов — RESUME.md. Исторические окна HT не включают новые строки.

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** Make every accepted assessment attributable to the approved evaluation model, exact digests and evaluation profile—not an authoring response or an unverified model label.

**CURRENT SOURCE / EVIDENCE:** LegalTrainingTeacherInference defines separate authoring/evaluation interfaces and canonical profiles. EvaluateCaseAsync checks the evaluation profile, permit use and local provenance. This is reuse-first validation work, not proof that these checks are missing.

**TARGET WORK (proposed decomposition of the original requirement):**

Drive the existing assessment path with a recording evaluation client. Vary one identity field at a time while keeping the answer JSON valid. Preserve the approved model, Ollama manifest/layer digests and Evaluation profile configuration hash. Trace the actual evidence received from the client into the receipt; a caller echo of expected settings is not measured identity.
Compare canonical profile identity before accepting a score; do not accept authoring-profile output merely because it comes from the same Qwen model. Trace the actual production composition to the evaluation-only client and investigate any unbound parameter at its real owner.
Repair only a demonstrated gap. Do not require a different vendor or replace the approved Qwen model: the original requirement is the same approved independent measuring path, not a newly selected commercial model. Source claims about isolation must match actual capabilities; interface naming alone does not prove separate storage or session memory.
Keep any new helper as the single owner of evaluator identity comparison; do not add a second hash recipe beside LocalQwenOllamaInferenceProfileAuthority.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R1; see `REQUIREMENTS.md`.
**Exit predicate:** Approved evaluator identity is enforced and tested; no claim of real evaluator quality.

Candidate existing-source allowlist (architect must narrow before writing):
- `14.ContinuousLearning/Core/LegalTrainingTeacherInference.cs`
- `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs`
- `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs`

PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `Tests/CodeDocumentationEvaluationIdentityTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.01.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| I | `14.ContinuousLearning/Core/LegalTrainingTeacherInference.cs` | 1–260 | ILegalTeacherEvaluationInferenceClient, ITrainingTeacherInferenceClient, LocalQwenOllamaInferenceProfileAuthority | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| A | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | 1–290, 315–670, 644–815 | AssessAsync, SnapshotHeldOut, EvaluateCaseAsync, BuildCheckerPrompt, ParseCheckerOutput | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| C | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs` | 1–455 | PlanCalibration, CalibrateAsync, ConditionsMatch, EvaluatorMatches | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HT | `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs` | 1–265, 265–660 | CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority, CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker, CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport, CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient, CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| QCLIENT | `14.ContinuousLearning/Training/LocalQwenOllamaTeacherInferenceClient.cs` | Resolve the named definition locally; no invented line pin | LocalQwenOllamaTeacherInferenceClient | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |
| QT | `14.ContinuousLearning/Core/LegalTrainingTeacherBoundary.cs` | Resolve the named definition locally; no invented line pin | LegalTrainingTeacherAdmissionAuthority | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Real profile guard | An evaluation receipt names EvaluationProfileId and its exact hash; the fake client proves the profile was actually returned. |
| Same family, distinct duty | Qwen may serve both roles through the existing separately admitted profiles; authoring output is never counted as evaluation. |
| No duplicate work | An existing endpoint test already proves a mismatch: retain it and add only the missing digest dimension. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Different display name means independent | Names do not establish model bytes, profile or isolation. |
| Copy expected values into observed fields | Fabricates the very evidence the test should verify. |
| Mock every identity check true | Removes the production boundary under test. |
| Ban references from the evaluator | The evaluator legitimately needs the isolated closed reference; the ban applies to authoring/training leakage. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Valid canonical evaluator | Use the existing canonical Evaluation evidence with valid provenance and one issued permit. | Assessment succeeds; observed request ID and all expected identity fields are recorded. |
| Wrong profile | Return Authoring evidence and otherwise valid output. | Reject with the existing profile/provenance failure; no partial quality report. |
| Same label, different bytes | Keep ModelId but change manifest digest, then layer digest in separate cases. | Reject each mismatch; a model name is not sufficient identity. |
| Configuration drift | Change one bound evaluation parameter/profile hash. | Reject; no silent normalization to the configured expected value. |
| No permit | Return valid response without issuing the permit. | Reject; positive control proves the client was reached. |

**Proposed focused class(es):** `CodeDocumentationEvaluationIdentityTests`  
**Known impacted seed class(es):** `CodeDocumentationSavedModelAssessmentEndpointTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.01` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Trace configuration -> factory -> evaluation response -> receipt and choose the smallest uncovered identity case. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.01/architect.md` |
| plan-reviewer | Require one-field mutations and a valid positive control; reject a plan that swaps providers or tests only expected-value echoes. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.01/plan-reviewer.md` |
| executor | Add focused identity tests and repair only the demonstrated guard at its existing owner. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.01/executor.md` |
| code-reviewer | Read the real factory/receipt chain and ensure an authoring response cannot satisfy an evaluator check. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.01/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- Mutable model tag resolves to different bytes.
- Null or malformed provenance even when pass=true.
- Both roles use one stateless transport: verify purpose and access boundary, not assumed process separation.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.02.md

# S03.02 — Freeze the exam contract and canonical case set

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** Keep corpus, canonical case sequence, generation settings and measurement conditions fixed for an assessment series and its comparison.

This is task **3 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.00, S03.01

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** Keep corpus, canonical case sequence, generation settings and measurement conditions fixed for an assessment series and its comparison.

**CURRENT SOURCE / EVIDENCE:** FrozenReferenceCorpus orders entries and rejects train/held-out source overlap. SnapshotHeldOut selects all HeldOut entries and rejects excess size. Calibration compares conditions and builds an EvaluationSetId. Reuse these contracts.

**TARGET WORK (proposed decomposition of the original requirement):**

Inventory the existing EvaluationSetId preimage and every condition checked by ConditionsMatch/SameExamConditions, including the complete builder definition. Bind corpus version/hash, canonical held-out identities, generation profile, evaluator profile and relevant noise-policy identity once, before the series begins.
Use the existing immutable corpus and record types. Snapshot only actually mutable input collections that affect execution; do not pretend a C# record replacement mutates an existing record. If extracting an evaluation-contract value avoids inconsistent comparisons, make it the one shared representation rather than introducing another parallel identity.
Canonical input enumeration order and observed execution order are different: reordering inputs that the builder sorts must produce the same canonical exam; a different executed case set/order is not the same exam. Keep candidate checkpoint identity fixed within repeats, but allow baseline and trial weights to differ across the intended comparison.
Reject missing/extra cases, unknown corpus provenance and over-cap corpora. Never truncate a held-out corpus with Take(max) to make a test cheap. Keep tokenizer/runtime identity under its existing owning compatibility contract; identify actual fields before adding redundant ones.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R1, R2; see `REQUIREMENTS.md`.
**Exit predicate:** One stable, source-bound exam contract with proven drift rejection.

Candidate existing-source allowlist (architect must narrow before writing):
- `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs`
- `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs`
- `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs`
- `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs`

PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `14.ContinuousLearning/Training/CodeDocumentationEvaluationContract.cs`
- `Tests/CodeDocumentationEvaluationContractTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.02.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| F | `14.ContinuousLearning/Core/FrozenReferenceCorpus.cs` | 1–270, 325–750 | FrozenReferencePartition, PaidReferenceProvenance, FrozenReferenceEntry, FrozenReferenceCorpus.Build | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| A | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | 1–290, 315–670, 644–815 | AssessAsync, SnapshotHeldOut, EvaluateCaseAsync, BuildCheckerPrompt, ParseCheckerOutput | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| C | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs` | 1–455 | PlanCalibration, CalibrateAsync, ConditionsMatch, EvaluatorMatches | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| G | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs` | 1–425 | PlanQualityGate, CalibrateBaselineAsync, BaselineRequiresNewTrainingPair, EvaluateTrialAsync, SameExamConditions | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HP | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentConfiguration.cs` | 1–280 | CodeDocumentationSavedModelAssessmentProfileResolver, ParseProfile, SettingsAreWithinServerBounds | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HT | `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs` | 1–265, 265–660 | CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority, CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker, CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport, CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient, CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| CF | `14.ContinuousLearning/Training/RepositoryOwnedReferenceCorpusManifest.cs` | 1–175 | RepositoryOwnedReferenceCorpusManifest | Current manifest/open-result definitions, bounds and sealer entry inspected. Expand full read/seal helpers before editing. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Fixed means effective inputs | The contract pins actual canonical cases and settings, not just a friendly corpus name. |
| Model comparison remains possible | Baseline A and trial B have different weights but share evaluation conditions; repeats of A remain on A. |
| Small honest fixture | Two held-out cases with five repeats exercise a ten-call series without discarding production rules. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Take the first five cases | Changes the exam and can select easier cases. |
| Compare baseline and trial weights for equality | Would reject the intended learned update rather than condition drift. |
| Reject harmless input reorder | Confuses pre-canonical enumeration with the stable executed order. |
| Hash only case IDs | Different contents under reused labels could be treated as the same exam. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Canonical equivalence | Build the same valid entries in opposite input enumeration orders. | Same canonical corpus and case contract; actual executions use the canonical order. |
| Different set | Change one held-out source/answer/hash or remove one case. | Different identity or rejection; never reuse the original comparison authority. |
| Changed generation | Change seed, token limit, temperature, TopK or TopP one at a time. | Detected as a changed exam contract before accepting a combined result. |
| Too many cases | Use max+1 real tiny entries. | Existing HeldOutTooLarge rejection, zero evaluator calls; no silent subset. |
| Mutable input | Mutate an actual mutable execution-affecting collection after capture. | Captured contract stays stable or fails closed; no mixed-condition successful series. |

**Proposed focused class(es):** `CodeDocumentationEvaluationContractTests`  
**Known impacted seed class(es):** `CodeDocumentationSavedModelAssessmentEndpointTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.02` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Enumerate the full existing identity preimage and mutable owners; choose reuse or one shared contract extraction. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.02/architect.md` |
| plan-reviewer | Check canonicalization, same-model repeats versus different-model comparison, and all configuration dimensions. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.02/plan-reviewer.md` |
| executor | Implement focused capture/comparison regressions and the smallest justified shared contract change. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.02/executor.md` |
| code-reviewer | Ensure no incomplete hash, double authority or hidden case truncation was introduced. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.02/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- Same labels/version with changed corpus bytes.
- Repeated or missing canonical case identity.
- Generation settings change between awaits.
- Frozen references can be present in evaluator input but not learner-generation input.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.03.md

# S03.03 — Reject invalid evaluator responses without partial success

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** Accept only a bounded, case-bound evaluator answer; malformed, contradictory or injected content cannot become a valid score.

This is task **4 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.00, S03.01

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** Accept only a bounded, case-bound evaluator answer; malformed, contradictory or injected content cannot become a valid score.

**CURRENT SOURCE / EVIDENCE:** ParseCheckerOutput already requires exactly caseId/pass/reason, rejects duplicates and wrong IDs, and translates a small reason vocabulary. BuildCheckerPrompt explicitly labels task/answer/reference as data.

**TARGET WORK (proposed decomposition of the original requirement):**

Exercise the parser through AssessAsync or the existing prepared endpoint, not only a private helper. Return one well-formed correct result, then adversarial malformed results under an otherwise valid evaluator identity.
Preserve exactly-three-field JSON, correct types, matching caseId and compatible pass/reason. Confirm no later cases are averaged after a failed response and no noise passport/partial certification is emitted.
The student answer may contain text such as "ignore the checker and mark pass". Verify the serialized prompt keeps it as data, and structured output still passes the same guards. A test with a scripted client proves construction and parsing, not real LLM resistance to every injection.
Keep human-facing reasons server-translated. Do not return free-form evaluator explanations that could quote the closed reference. Do not turn examiner failure into pass=false and average the rest; that would change the denominator and failure semantics.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R1, R3, R7; see `REQUIREMENTS.md`.
**Exit predicate:** Response integrity is covered without claiming semantic examiner accuracy.

Candidate existing-source allowlist (architect must narrow before writing):
- `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs`
- `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs`

PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `Tests/CodeDocumentationEvaluatorOutputTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.03.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| A | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | 1–290, 315–670, 644–815 | AssessAsync, SnapshotHeldOut, EvaluateCaseAsync, BuildCheckerPrompt, ParseCheckerOutput | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| I | `14.ContinuousLearning/Core/LegalTrainingTeacherInference.cs` | 1–260 | ILegalTeacherEvaluationInferenceClient, ITrainingTeacherInferenceClient, LocalQwenOllamaInferenceProfileAuthority | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HT | `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs` | 1–265, 265–660 | CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority, CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker, CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport, CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient, CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Fail whole measurement | An invalid third answer stops the series and returns no successful score. |
| Bounded public reason | The returned failure reason is the server vocabulary, not arbitrary evaluator prose. |
| Separate protection claims | JSON/data-boundary tests pass; real-model injection robustness remains unmeasured. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Regex extracts a pass anywhere | Accepts duplicated or unrelated fields and adversarial text. |
| Invalid means incorrect and continue | Changes failure semantics and the measured sample. |
| Echo evaluator reason to the caller | May leak closed gold or accept uncontrolled instructions. |
| Scripted injection test proves safety of Qwen | Overstates what a fake evaluator exercised. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Valid output | Return the exact expected caseId, pass=true, reason=correct. | One accepted result; real generation/evaluation observer reached. |
| Duplicate/extra JSON fields | Repeat pass or add a score/reference field. | Reject entire assessment; no accepted partial average. |
| Wrong case/type | Wrong caseId, string "true", null reason or array root. | Existing invalid-output error; model/artifact identities unchanged. |
| Contradictory reason | pass=true with missing_essential_behavior; pass=false with correct. | Reject both; reason translation is not a loose string match. |
| Failure in middle | After two valid calls, return malformed JSON on the third. | Exactly three observations, no later calls, no completed calibration/report. |
| Data injection | Student answer includes role delimiters or an instruction to reveal the reference. | Prompt payload remains data; output leak/extra fields rejected. Claim is limited to the scripted contract test. |

**Proposed focused class(es):** `CodeDocumentationEvaluatorOutputTests`  
**Known impacted seed class(es):** `CodeDocumentationSavedModelAssessmentEndpointTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.03` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Select the public/prepared path and parser invariants; no new grammar unless source requires it. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.03/architect.md` |
| plan-reviewer | Check every negative reaches valid admission before parser rejection; include a positive control. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.03/plan-reviewer.md` |
| executor | Add response/early-exit tests; repair only demonstrated parsing or propagation defects. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.03/executor.md` |
| code-reviewer | Inspect strict parsing, reason disclosure and the absence of partial-success fallback. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.03/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- Oversized/empty output, invalid nesting, trailing JSON.
- Cancellation before versus after permit issuance.
- A malformed last case must not accidentally return the earlier accumulated report.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.04.md

# S03.04 — Enforce the complete call budget, unique receipts and deadline

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** Count the calls that actually happened and reject incomplete or duplicated evaluation evidence without spending outside the admitted budget.

This is task **5 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.00, S03.01, S03.03

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** Count the calls that actually happened and reject incomplete or duplicated evaluation evidence without spending outside the admitted budget.

**CURRENT SOURCE / EVIDENCE:** PlanCalibration uses checked held-out-count × repeat-count; PlanQualityGate doubles that plan. CalibrateAsync requires unique observed IDs. Existing endpoint tests cover a 10-call series, a 9-call budget and malformed repeat failure.

**TARGET WORK (proposed decomposition of the original requirement):**

Preserve existing plan arithmetic and atomic single-call permits. Add only missing cases for overflow/budget boundaries, duplicate request IDs across repeats, cancellation and disposal.
Assert factory creation, attempted calls, issued permits and completed responses separately when observable. A timeout after a remote call was issued does not prove no cost; preserve the issued-call evidence and do not automatically replace it with an uncounted retry.
Use the existing configuration's repeat count; do not reduce it to one for speed. Reject an inadequate budget before constructing the client when the existing path supports that preflight. No caller-supplied completed-call count or passport can establish a measurement.
Keep a single bounded deadline for the series and normal cancellation behavior. Restore environment/static test hooks in finally. No new process runner, retry daemon or provider installation belongs here.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R1, R3; see `REQUIREMENTS.md`.
**Exit predicate:** Bounded complete-call evidence, without changing repetition or retry policy.

Candidate existing-source allowlist (architect must narrow before writing):
- `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs`
- `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs`
- `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs`

PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `Tests/CodeDocumentationEvaluationBudgetTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.04.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| C | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs` | 1–455 | PlanCalibration, CalibrateAsync, ConditionsMatch, EvaluatorMatches | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| G | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs` | 1–425 | PlanQualityGate, CalibrateBaselineAsync, BaselineRequiresNewTrainingPair, EvaluateTrialAsync, SameExamConditions | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| I | `14.ContinuousLearning/Core/LegalTrainingTeacherInference.cs` | 1–260 | ILegalTeacherEvaluationInferenceClient, ITrainingTeacherInferenceClient, LocalQwenOllamaInferenceProfileAuthority | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HT | `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs` | 1–265, 265–660 | CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority, CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker, CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport, CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient, CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HP | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentConfiguration.cs` | 1–280 | CodeDocumentationSavedModelAssessmentProfileResolver, ParseProfile, SettingsAreWithinServerBounds | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Count evidence, not intentions | Budget ten, ten permits issued, ten unique results: the reported count is backed by observations. |
| Fail before expense | Budget nine rejects without constructing the evaluation client. |
| Saved baseline receipts remain historical | Resume adds fresh trial IDs; it does not pretend baseline calls happened again. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Budget equals successes only | Ignores issued failures/retries and can exceed permission. |
| Same request IDs means same conditions | Confuses identity of a call with identity of an exam. |
| repeatCount=1 to keep green | Weakens the approved measurement policy. |
| Catch timeout and certify partial mean | A missing result is not passing evidence. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Exact budget | Two held-out cases × canonical five repeats, budget ten. | Exactly ten distinct completed request IDs and ten observations; noise passport exists. |
| One below budget | Same corpus/repeats with budget nine. | Rejected before client factory; zero calls, no partial result. |
| Duplicate receipts | Return valid scores but recycle IDs between repeats. | Condition mismatch; not a successful ten-call series. |
| Incomplete series | Cancel or fail after an issued call. | No completed passport; observed issued/completed counts remain honest; no hidden retry. |
| Combined budget | Baseline+trial expected total is twice the single-series plan. | Both planned and observed counts agree before acceptance; saved baseline IDs plus fresh trial IDs remain unique. |

**Proposed focused class(es):** `CodeDocumentationEvaluationBudgetTests`  
**Known impacted seed class(es):** `CodeDocumentationSavedModelAssessmentEndpointTests`, `CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.04` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Map the existing plan, permit, client lifetime and deadline owners. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.04/architect.md` |
| plan-reviewer | Check before-call rejection versus after-issued-call accounting and duplicate-ID conditions. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.04/plan-reviewer.md` |
| executor | Write deterministic counters/cancellation fixtures with no real network or provider calls. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.04/executor.md` |
| code-reviewer | Ensure all result-count claims are observed and incomplete runs cannot yield acceptance. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.04/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- Integer overflow in planned multiplication.
- Client disposal throws: preserve primary failure and no success.
- Cancellation after transport dispatch but before response.
- Do not infer network cost from a fake client.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.05.md

# S03.05 — Reject changed exam conditions before resuming evaluation

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** A resumed lesson must use its original saved baseline and approved exam conditions; a changed examiner or corpus cannot consume calls or publish a result under the old comparison.

This is task **6 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.02, S03.04

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** A resumed lesson must use its original saved baseline and approved exam conditions; a changed examiner or corpus cannot consume calls or publish a result under the old comparison.

**CURRENT SOURCE / EVIDENCE:** The coordinator reopens the immutable baseline and trial. EvaluateTrialAsync currently performs trial calibration and then checks SameExamConditions. The new work is a targeted preflight/coverage improvement only if the same earlier validation is not already provided by an upstream owner.

**TARGET WORK (proposed decomposition of the original requirement):**

Reconcile the saved baseline's actual fields with the current evaluation contract from S03.02. Add the earliest supported exact comparison before the first resumed evaluation call, without creating another baseline or resetting its request IDs.
Validate evaluator model/digests/profile, corpus identity/canonical cases, generation settings and applicable noise policy from trusted persisted/server values. Do not compare baseline and trial checkpoint hashes for equality: the trial is intentionally different. Validate the trial's own bound identity through the existing S02 loader/bundle guards.
Use a same-TrainingRunId applied-but-unpublished attempt created through the real coordinator. Recreate dependencies for the second invocation. With unchanged conditions, only the trial series runs, saved baseline bytes remain identical and there are zero additional trainer calls. With one changed condition, reject before evaluation and before publication.
Retain SameExamConditions after measurement as defense against mid-run drift. Preflight is not a substitute for verifying actual responses. Do not modify the S02 optimizer/RNG protocols or repair historical cleanup here.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R1, R2, R3; see `REQUIREMENTS.md`.
**Exit predicate:** Changed conditions cannot be silently substituted; S01/S02 recovery semantics are preserved.

Candidate existing-source allowlist (architect must narrow before writing):
- `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs`
- `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs`
- `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs`
- `Tests/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests.cs`

PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `Tests/CodeDocumentationResumeEvaluationContractTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.05.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| R | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs` | 529–753, 1002–1264, 1298–1521 | TrainAndPublishAsync, ResumeQualityDecisionFromExistingAttemptAsync | Method windows inspected at prior revision 67c22705; the current tree has the same Git blob 7dccd4ed. Reuse is explicit, not a new full-file read. |
| RT | `Tests/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests.cs` | Resolve the named definition locally; no invented line pin | SameRunRecoveryRemainsEvaluationOnlyWithoutTrainerUpdate, CreateQualityGateContext, CreateAppliedButUnpublishedAttemptAsync | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |
| J | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.cs` | 1–155 | WriteAndReadBack, OpenExisting, MatchesSingleTrainingPair | Current state/receipt definitions and write-before-training prefix inspected. Expand OpenExisting and complete validation methods locally. |
| C | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs` | 1–455 | PlanCalibration, CalibrateAsync, ConditionsMatch, EvaluatorMatches | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| G | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs` | 1–425 | PlanQualityGate, CalibrateBaselineAsync, BaselineRequiresNewTrainingPair, EvaluateTrialAsync, SameExamConditions | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| A | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | 1–290, 315–670, 644–815 | AssessAsync, SnapshotHeldOut, EvaluateCaseAsync, BuildCheckerPrompt, ParseCheckerOutput | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| I | `14.ContinuousLearning/Core/LegalTrainingTeacherInference.cs` | 1–260 | ILegalTeacherEvaluationInferenceClient, ITrainingTeacherInferenceClient, LocalQwenOllamaInferenceProfileAuthority | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Keep the original yardstick | The baseline file is reopened and hash-compared; no new calibration call is made. |
| Reject before calls | The second request changes the model-layer digest; evaluator-call count remains zero. |
| Keep the later check | A provenance change after admission still fails during measurement. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Different run ID | Exercises a fresh attempt instead of recovery. |
| Replace baseline when settings differ | Moves the comparison target and invalidates the original lesson decision. |
| Assert trainer count zero but never reach recovery | An earlier invalid fixture can make this assertion pass falsely. |
| Require baseline/trial weights equal | Blocks legitimate comparison and is unrelated to fixed exam conditions. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Valid same-run recovery | First call applies but does not promote; second call uses the same run ID and exact conditions with fresh evaluation IDs. | Only trial calls execute, trainer delta zero, persisted baseline unchanged; positive outcome reaches actual intended evaluation. |
| Changed evaluator/corpus | Mutate one examiner digest/profile/corpus identity before second call. | Reject before client evaluation, trainer and publication; original artifacts retained. |
| Changed generation/noise policy | Change seed/token settings or repeat/multiplier identity before second call. | Reject old comparison authority, do not recalibrate the baseline. |
| Mid-series drift | Preflight passes but response provenance changes during evaluation. | Existing response/post-series checks reject; no accepted child. |
| Already published replay | Replay a successfully published pair. | Existing lineage guard remains; do not mistake that rejection for the recovery test. |

**Proposed focused class(es):** `CodeDocumentationResumeEvaluationContractTests`  
**Known impacted seed class(es):** `CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests`, `CodeDocumentationSavedModelAssessmentEndpointTests`, `CodeDocumentationRepositoryOwnedModelCheckpointStartServiceTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.05` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Trace the earliest genuine saved-baseline comparison owner and select exact fields; preserve the recovery call order. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.05/architect.md` |
| plan-reviewer | Check one-field drift cases, a real same-run positive fixture, and no baseline rewrite. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.05/plan-reviewer.md` |
| executor | Implement preflight only where demonstrably absent and add coordinator-route call-count/artifact-retention tests. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.05/executor.md` |
| code-reviewer | Trace guard ordering before evaluation, actual response validation afterward, and zero duplicate training. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.05/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- A null source attempt does not waive same-run comparison checks.
- Old saved baseline lacks a required identity: fail closed and report compatibility, never synthesize historical values.
- Request IDs differ across calls even when the exam is identical.
- S02 acceptance/admission unresolved: finish independent planning/tests only as authorized, do not erase its hold.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.06.md

# S03.06 — Preserve uncertainty decisions and distinguish certification referral

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** A higher number is not automatically an improvement, and a sufficient diagnostic baseline is not a final certificate.

This is task **7 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.02, S03.04

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** A higher number is not automatically an improvement, and a sufficient diagnostic baseline is not a final certificate.

**CURRENT SOURCE / EVIDENCE:** The quality gate compares the score delta against both noise passports and rejects unchanged weights. BaselineRequiresNewTrainingPair compares mean minus 0.95 against the existing policy and returns BaselineAlreadySufficientErrorCode when a separate final exam is needed.

**TARGET WORK (proposed decomposition of the original requirement):**

Freeze a truth table for existing MeasurementNoisePolicy semantics before changing code. Difference strictly above each relevant floor may establish improvement; equality at a floor is indistinguishable. Nonfinite input and invalid/missing calibration cannot be accepted.
Pin the difference between (a) improvement over the baseline and (b) reaching the final target. A trial can improve but remain below target. A diagnostic result beyond the target can request independent certification but does not itself certify the model or justify another pair.
The active prose says "95% accounting for uncertainty"; the implementation uses strict ClassifyDifference(mean - 0.95, passport). Preserve that current strict predicate unless a source-authorized policy decision explicitly changes it. Do not silently substitute >=0.95 or a new confidence interval. Report the equality behavior plainly.
Add mathematical boundary tests using the existing BuildPassport factory and actual computed floors, and route-level tests with scripted outcome series. Never manually create a self-inconsistent passport or loosen float tolerance to force green.
For missing explicit public decision names, propose a small typed distinction at the existing result owner; do not turn success/error transport status into a new model-quality proof.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R4; see `REQUIREMENTS.md`.
**Exit predicate:** Decision behavior is explicit and tested; certification remains a separate requirement.

Candidate existing-source allowlist (architect must narrow before writing):
- `14.ContinuousLearning/Core/MeasurementNoise.cs`
- `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs`
- `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs`
- `Tests/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests.cs`

PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `Tests/CodeDocumentationQualityDecisionPolicyTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.06.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| N | `14.ContinuousLearning/Core/MeasurementNoise.cs` | 1–300 | MeasurementNoisePolicyConfig, BuildPassport, ClassifyDifference, TryValidatePassportForMeasurement | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| G | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs` | 1–425 | PlanQualityGate, CalibrateBaselineAsync, BaselineRequiresNewTrainingPair, EvaluateTrialAsync, SameExamConditions | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| C | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs` | 1–455 | PlanCalibration, CalibrateAsync, ConditionsMatch, EvaluatorMatches | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HT | `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs` | 1–265, 265–660 | CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority, CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker, CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport, CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient, CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| RT | `Tests/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests.cs` | Resolve the named definition locally; no invented line pin | SameRunRecoveryRemainsEvaluationOnlyWithoutTrainerUpdate, CreateQualityGateContext, CreateAppliedButUnpublishedAttemptAsync | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Concrete calculation | For synthetic repeated means, compute the expected floor through the existing policy and independently verify the comparison direction. |
| Honest referral | The model is ready for an independent exam; it has not passed that exam yet. |
| Two comparisons | Improvement must survive the baseline and trial uncertainty checks. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Any positive delta promotes | Ignores noise and turns randomness into progress. |
| 95% means certified | Confuses a diagnostic trigger with a clean final measurement. |
| Replace the policy with another statistical formula | Changes the product criterion without authority. |
| Increase tolerances until tests pass | Weakens the contract rather than fixing implementation. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Improvement under both floors | Build two valid passports and choose a delta above both computed floors. | Improvement succeeds only with changed trial weights and valid identical exam conditions. |
| Equality boundary | Call ClassifyDifference using exactly passport.NoiseFloor. | Indistinguishable; no direction proven. Avoid accidental floating subtraction approximations. |
| One noisy series | Delta exceeds baseline floor but not trial floor. | No promotion; both checks are required. |
| Improved but not certified | An improving diagnostic series remains below the target. | Improvement and target readiness are distinct; no certificate. |
| Sufficient diagnostic baseline | Valid diagnostic mean exceeds 0.95 by more than its existing floor. | Existing referral/no-new-pair behavior; no certificate record or extra training. |
| Invalid measurement | Nonfinite score or incomplete passport. | Rejected, never interpreted as zero noise or perfect quality. |

**Proposed focused class(es):** `CodeDocumentationQualityDecisionPolicyTests`  
**Known impacted seed class(es):** `CodeDocumentationSavedModelAssessmentEndpointTests`, `CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.06` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Write the exact source-based decision truth table, including the threshold wording/strict-predicate distinction. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.06/architect.md` |
| plan-reviewer | Check both-noise requirement and separation of promotion, referral and certification. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.06/plan-reviewer.md` |
| executor | Add discriminating policy and route tests; repair only a verified divergence from the accepted truth table. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.06/executor.md` |
| code-reviewer | Reject altered thresholds, invalid test passports and diagnostic-as-certificate outputs. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.06/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- Perfect repeat scores can yield zero project-policy floor: still need purpose/eligibility for certification.
- Boundary 0.95 and exactly-equal noise values.
- A configured target change requires explicit policy provenance/version.
- A valid negative decision is not an infrastructure failure.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.07.md

# S03.07 — Define clean certification admission without relabeling HeldOut

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** A final exam uses a separately admitted, unseen set; a diagnostic or teacher-exposed case cannot become clean merely by changing its ID or folder.

This is task **8 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.00, S03.02, S03.06

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** A final exam uses a separately admitted, unseen set; a diagnostic or teacher-exposed case cannot become clean merely by changing its ID or folder.

**CURRENT SOURCE / EVIDENCE:** FrozenReferencePartition contains Train/HeldOut and enforces exact source-fingerprint split disjointness. That is not by itself a purpose or exposure-history contract. A certification admission type is a proposed addition, subject to the duplicate-owner search in S03.00.

**TARGET WORK (proposed decomposition of the original requirement):**

Implement only the pure admission boundary selected in S03.00. Preferred proposed file/type: CodeDocumentationCertificationAdmission in 14.ContinuousLearning/Training. Keep purpose separate from Train/HeldOut; reuse a current equivalent if found.
Inputs must come from the server-owned corpus/provenance owner: the candidate accepted-checkpoint identity, frozen certification corpus manifest/hash, evaluation-contract identity, target/noise policy, and a complete declared exposure/source index for the selected CodeDocumentation scope. An HTTP caller's clean=true or an arbitrary supplied hash is not authority.
Admission verifies disjoint source identity against training and teacher-visible diagnostics, matching declared scope/versions, nonempty closed set and complete provenance. Unknown historical exposure means not eligible, not "probably clean". Track exact source identity, normalized content identity and declared derivation lineage where available; exact hashes do not prove semantic disjointness from arbitrary paraphrases.
Return an immutable validated admission object with controlled construction; do not expose a public settable IsClean flag or allow a with-expression to rewrite validated authority without revalidation. Define explicit rejection reasons and no model mutation.
This task does not run an exam or create a certificate. It must not replace the existing paid-reference-never-DPO-side rules.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R5, R6; see `REQUIREMENTS.md`.
**Exit predicate:** A tested admission contract; no real corpus is certified clean by this code fixture.


PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `14.ContinuousLearning/Training/CodeDocumentationCertificationAdmission.cs`
- `Tests/CodeDocumentationCertificationAdmissionTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.07.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| F | `14.ContinuousLearning/Core/FrozenReferenceCorpus.cs` | 1–270, 325–750 | FrozenReferencePartition, PaidReferenceProvenance, FrozenReferenceEntry, FrozenReferenceCorpus.Build | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| A | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | 1–290, 315–670, 644–815 | AssessAsync, SnapshotHeldOut, EvaluateCaseAsync, BuildCheckerPrompt, ParseCheckerOutput | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| D | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.cs` | 1–250, 320–625 | ResolveOrAuthorAsync, AuthorPairsAsync, AuthorOnePairAsync, BuildTeacherPrompt | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HP | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentConfiguration.cs` | 1–280 | CodeDocumentationSavedModelAssessmentProfileResolver, ParseProfile, SettingsAreWithinServerBounds | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| CF | `14.ContinuousLearning/Training/RepositoryOwnedReferenceCorpusManifest.cs` | 1–175 | RepositoryOwnedReferenceCorpusManifest | Current manifest/open-result definitions, bounds and sealer entry inspected. Expand full read/seal helpers before editing. |
| LAY | `14.ContinuousLearning/Training/RepositoryOwnedTrainingArtifactLayout.cs` | Resolve the named definition locally; no invented line pin | RepositoryOwnedTrainingArtifactLayout | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |
| Q | `.kiro/xiigen-slices/04-QUEUE.md` | 1–30 | S03.*, S04, Do not prefill future slices | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| P | `llm mvp/design to code session/09.09.26WorkLeft-TASK-PROMPTS-TRAINING.md` | 1–34 | active opening before historical details | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Purpose is explicit | The corpus is held out and separately admitted for certification; both conditions are checked. |
| Relabeling fails | new-case-7 still traces to diagnostic-source-3, so it is not eligible. |
| Bounded claim | No known exposure under this declared source scope is supported; universal semantic non-overlap is not claimed. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| HeldOut means never seen | The enum records a split, not all future disclosures. |
| Different IDs prove disjointness | Labels can conceal the same source. |
| Missing history equals clean | Converts absence of evidence into authority. |
| Reject all paid references | Confuses prohibited training reuse with legitimate isolated reference-based evaluation. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Clean synthetic set | Use disjoint trusted training/diagnostic/certification source identities and complete provenance. | Admission succeeds and binds checkpoint, corpus, conditions, purpose and policy. |
| Relabeled diagnostic | Copy a teacher-exposed source into a new corpus/name/entry ID. | Reject by source/content/declared-lineage overlap, before evaluator or trainer calls. |
| Unknown exposure | Omit history/completeness for the applicable source scope. | Explicit eligibility failure, not zero overlaps interpreted as proof. |
| Caller assertion | Attempt admission from clean=true or an untrusted digest without the owning provenance. | Rejected; the test must exercise the real trust boundary. |
| Legitimate distinct teaching task | Separate source with shared topic but no shared entity/derived exam case. | Do not reject merely for domain/topic equality; apply the defined provenance policy. |
| Mutable authority | Attempt to alter source lists after admission. | Validated identity is stable or revalidation rejects; no mutable clean bypass. |

**Proposed focused class(es):** `CodeDocumentationCertificationAdmissionTests`  
**Known impacted seed class(es):** `CodeDocumentationSavedModelAssessmentEndpointTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.07` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Finalize trust root, immutable value shape, source-scope and overlap policy in the existing S03 contract before coding. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.07/architect.md` |
| plan-reviewer | Try forged clean flags, aliases and missing history; check new design is explicitly proposed and not described as existing. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.07/plan-reviewer.md` |
| executor | Implement pure validation and tests with synthetic source identities only. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.07/executor.md` |
| code-reviewer | Trace all admission constructors/factories and verify there is no alternate unvalidated path. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.07/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- Aliases, identical normalized input, declared derived samples.
- Empty corpus or multiple domains/subjects when original corpus contract forbids them.
- Old corpus has no exposure provenance: not eligible until valid evidence exists.
- Do not add a global Certification enum migration to unrelated domains.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.08.md

# S03.08 — Persist certification purpose and exposure state safely

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** Restarting or renaming a run must not erase exposure history or turn an incomplete exam into an unused clean opportunity.

This is task **9 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.07

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** Restarting or renaming a run must not erase exposure history or turn an incomplete exam into an unused clean opportunity.

**CURRENT SOURCE / EVIDENCE:** The repository already has safe bounded artifact readers/writers and corpus manifests. The new certification session record is proposed; its exact placement/version is selected once in S03.00 rather than fabricated as an existing path.

**TARGET WORK (proposed decomposition of the original requirement):**

Implement the durable side of the admission selected in S03.07 at the existing artifact boundary or, if no equivalent exists, the proposed CodeDocumentationCertificationSessionStore. Keep a single versioned owner for a session's immutable checkpoint/corpus/conditions identity and monotonic lifecycle.
Before the first evaluation call, reserve the exact fixed-candidate session with trusted admission and planned cases/repeats/budget. Persist and reopen the record before spending calls. A reservation does not mean a result exists. A completed result may be reused only for the same sealed tuple; a new run ID cannot clear the source's exposure state.
Proposed conservative replay rule for this pack: completed exact-session replay returns the prior validated receipt without new calls. An interrupted/ambiguous session does not automatically retry until a favorable score; it remains incomplete/exposed unless the predeclared server policy explicitly admits continuation of the same immutable series. Choose and encode that policy before implementation, never after seeing outcomes.
Use bounded create-new/write-readback and expected identity from the trusted owner. Test hash/length/version/EOF and unsafe path handling. Hash agreement proves matching bytes, not provenance: do not allow the same untrusted caller to invent payload and expected hash and thereby mint authority.
Store only necessary metadata/results; raw closed answers do not belong in general logs or user-readable state. No bulk cleanup of old16/old12 or new native process containment work.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R5, R6, R8; see `REQUIREMENTS.md`.
**Exit predicate:** Durable, fail-closed certification session identity; no actual exam completed by storage alone.


PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `14.ContinuousLearning/Training/CodeDocumentationCertificationSessionStore.cs`
- `Tests/CodeDocumentationCertificationSessionStoreTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.08.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| CF | `14.ContinuousLearning/Training/RepositoryOwnedReferenceCorpusManifest.cs` | 1–175 | RepositoryOwnedReferenceCorpusManifest | Current manifest/open-result definitions, bounds and sealer entry inspected. Expand full read/seal helpers before editing. |
| LAY | `14.ContinuousLearning/Training/RepositoryOwnedTrainingArtifactLayout.cs` | Resolve the named definition locally; no invented line pin | RepositoryOwnedTrainingArtifactLayout | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |
| F | `14.ContinuousLearning/Core/FrozenReferenceCorpus.cs` | 1–270, 325–750 | FrozenReferencePartition, PaidReferenceProvenance, FrozenReferenceEntry, FrozenReferenceCorpus.Build | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| J | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.cs` | 1–155 | WriteAndReadBack, OpenExisting, MatchesSingleTrainingPair | Current state/receipt definitions and write-before-training prefix inspected. Expand OpenExisting and complete validation methods locally. |
| HP | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentConfiguration.cs` | 1–280 | CodeDocumentationSavedModelAssessmentProfileResolver, ParseProfile, SettingsAreWithinServerBounds | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| Q | `.kiro/xiigen-slices/04-QUEUE.md` | 1–30 | S03.*, S04, Do not prefill future slices | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| P | `llm mvp/design to code session/09.09.26WorkLeft-TASK-PROMPTS-TRAINING.md` | 1–34 | active opening before historical details | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Durability before expense | A valid reservation survives reconstruction before the first examiner call is permitted. |
| Honest incomplete state | A failed exam remains incomplete; the next run cannot relabel it unseen. |
| Safe replay | A completed same-identity receipt is reopened and reused without spending again. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Only in-memory exposure flag | Restart loses the state and permits contamination. |
| New ID resets eligibility | Allows repeated optimization against the same exam. |
| Hash alone proves trusted admission | A caller can hash its own forged data. |
| Retry until pass | Turns certification into training/model selection on the test set. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Reserve then reopen | Admit a synthetic set, create the session and reopen it before any evaluation. | Exact identity preserved; write/readback failure results in zero calls. |
| Completed exact replay | Reopen a completed immutable session for the same checkpoint/corpus/conditions. | Same validated receipt, no fresh evaluator calls; label evidence replay, not new measurement. |
| Interrupted session | Persist started/incomplete session, recreate services and request reuse. | No automatic fresh clean status or favorable-result retry. |
| Tampered record | Missing, truncated, duplicate/unknown schema fields or wrong hash/length/version. | Fail closed; no evaluator, trainer or promotion. |
| Wrong candidate | Try existing receipt with new checkpoint hash or evaluation contract. | Reject reuse; do not patch the old record. |
| Concurrent admission | Two requests reserve the same session under the owning boundary. | At most one admitted call sequence, no overwritten history. |

**Proposed focused class(es):** `CodeDocumentationCertificationSessionStoreTests`  
**Known impacted seed class(es):** `CodeDocumentationCertificationAdmissionTests`, `CodeDocumentationSavedModelAssessmentEndpointTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.08` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Specify exact schema, state transitions, trusted expected identities and incomplete/replay policy; prefer existing store. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.08/architect.md` |
| plan-reviewer | Walk interrupted and concurrent transitions and require a no-retry-until-pass rule. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.08/plan-reviewer.md` |
| executor | Implement bounded persistence and reconstruction tests, retaining existing file-safety helpers. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.08/executor.md` |
| code-reviewer | Check write-before-call ordering, monotonic exposure state and no caller-minted trust. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.08/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- Cancellation after reserve but before first call differs from after call issuance.
- Concurrent reservations and late completion conflict.
- Old schema that cannot prove eligibility is not upgraded by invented defaults.
- Test-owned cleanup failure reported without deleting uncertain historical paths.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.09.md

# S03.09 — Keep closed exam material out of authoring and public feedback

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** Teaching can use separate approved teaching examples, but certification inputs, references and feedback must not flow into new DPO pairs or later tuning.

This is task **10 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.03, S03.07, S03.08

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** Teaching can use separate approved teaching examples, but certification inputs, references and feedback must not flow into new DPO pairs or later tuning.

**CURRENT SOURCE / EVIDENCE:** The authoring route already distinguishes ITrainingTeacherInferenceClient and reuses matching sealed datasets without another factory call. BuildTeacherPrompt can contain a paid teaching reference. The evaluator intentionally receives the closed reference. These uses must not be confused.

**TARGET WORK (proposed decomposition of the original requirement):**

Trace the selected CodeDocumentation data flow into ResolveOrAuthorAsync/AuthorPairsAsync and the saved-assessment/public output. Use the certification admission/session identities from S03.07/.08 to reject exam-origin or teacher-exposed certification material before constructing an authoring client, requesting a pair or admitting training data.
Keep separate paid teaching references allowed only as approved authoring context; neither paid model text nor exam outputs may become chosen/rejected bytes. Preserve existing authorship/content binding and reuse-before-teacher-call behavior. Do not implement this as a prompt warning with no server-side guard.
Protect the legitimate isolated examiner path: its closedReferenceAnswer remains available to the evaluation-only client. Do not break semantic scoring by stripping its reference. Keep diagnostic public behavior compatible where required, but explicitly classify disclosed diagnostics as ineligible for later clean certification.
For certification outputs, use an aggregate/opaque receipt projection, not the diagnostic DTO's per-case answer previews or raw examiner text. Unknown lineage blocks a clean claim. Do not claim a finite string-sentinel test proves universal semantic nonleakage.
Where raw RoutingDecisionRecord lacks purpose/provenance, add an owned validated teaching-request adapter at the CodeDocumentation boundary rather than broadening every shared routing type. Its concrete type and call-site list must already be frozen in S03.00/.07 before execution.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R5, R6, R7; see `REQUIREMENTS.md`.
**Exit predicate:** Actual data-flow separation tested; no universal semantic-secrecy claim.

Candidate existing-source allowlist (architect must narrow before writing):
- `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.cs`
- `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentEndpoint.cs`
- `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs`

PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `Tests/CodeDocumentationEvaluationDisclosureTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.09.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| D | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.cs` | 1–250, 320–625 | ResolveOrAuthorAsync, AuthorPairsAsync, AuthorOnePairAsync, BuildTeacherPrompt | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| F | `14.ContinuousLearning/Core/FrozenReferenceCorpus.cs` | 1–270, 325–750 | FrozenReferencePartition, PaidReferenceProvenance, FrozenReferenceEntry, FrozenReferenceCorpus.Build | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| I | `14.ContinuousLearning/Core/LegalTrainingTeacherInference.cs` | 1–260 | ILegalTeacherEvaluationInferenceClient, ITrainingTeacherInferenceClient, LocalQwenOllamaInferenceProfileAuthority | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| A | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | 1–290, 315–670, 644–815 | AssessAsync, SnapshotHeldOut, EvaluateCaseAsync, BuildCheckerPrompt, ParseCheckerOutput | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| H | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentEndpoint.cs` | 1–230 | CodeDocumentationSavedModelAssessmentHttpRequest, MapCodeDocumentationSavedModelAssessment, ExecuteAsync, ExecuteCalibrationAsync | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HT | `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs` | 1–265, 265–660 | CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority, CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker, CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport, CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient, CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Correct reference boundary | Closed gold is visible to the measuring client, not the authoring client or public receipt. |
| Preserve useful authoring | A separate teaching example is still usable under existing provenance, without copying its paid reference as a DPO side. |
| Real ingress test | The negative exercises ResolveOrAuthorAsync or the actual new owned adapter, not only an isolated predicate. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Remove every reference from every prompt | Breaks legitimate evaluation and approved teaching context. |
| Add do-not-leak prose only | No enforceable data/capability boundary. |
| Rename a diagnostic record | Does not remove prior exposure. |
| Public certificate includes detailed correct answers | Makes future reuse an exposed exam and invites tuning on it. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Separated teaching/reference | Distinct synthetic teaching source carries an approved paid teaching reference; examiner uses a different closed sentinel. | Teacher may receive teaching context, never the closed sentinel; DPO sides are independently teacher-authored. |
| Exam-origin attempt | Send a certification source/alias through the actual authoring ingress. | Reject before factory, inference and pair seal; zero calls and no DPO writes. |
| Cached dataset | A matching existing sealed teaching dataset is requested again. | No teacher client construction/call; admission rules are not bypassed by cache metadata. |
| Public projection | Evaluation client sees required closed gold; public certification JSON/log projection is captured. | No closed gold, per-case diagnostic explanation or answer preview leaves the allowed evaluator boundary. |
| Direct copy | Teacher response uses the paid reference as chosen or rejected. | Existing content/provenance policy rejects; no training admission. |
| Contamination already happened | A previously clean source is disclosed through diagnostics. | Durable eligibility changes; later certification cannot reuse the old clean claim. |

**Proposed focused class(es):** `CodeDocumentationEvaluationDisclosureTests`  
**Known impacted seed class(es):** `CodeDocumentationSavedModelAssessmentEndpointTests`, `CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.09` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Map authoring, scoring, storage and public projections with explicit allowed recipients for each data class. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.09/architect.md` |
| plan-reviewer | Reject both prompt-only protection and overbroad bans that disable valid paid teaching references. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.09/plan-reviewer.md` |
| executor | Wire the source-purpose guard at actual ingress and add two-client spy plus file-retention tests. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.09/executor.md` |
| code-reviewer | Trace every selected ingress/projection and verify the actual forbidden destination receives no closed payload. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.09/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- Correct short student answer can coincide with reference: inspect provenance and role, not blanket text equality for scoring.
- Nested log/exception payloads can leak raw examiner output.
- Cache hit may refer to ineligible/unknown source history.
- Other domains remain outside this CodeDocumentation change.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.10.md

# S03.10 — Execute independent certification on an admitted saved checkpoint

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** Run the final exam on a fixed accepted model and a separately admitted clean set, without training, model promotion or reuse of diagnostic scores as results.

This is task **11 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.04, S03.06, S03.07, S03.08, S03.09

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** Run the final exam on a fixed accepted model and a separately admitted clean set, without training, model promotion or reuse of diagnostic scores as results.

**CURRENT SOURCE / EVIDENCE:** The existing assessment/calibration service can load/measure saved models and return noise evidence. BaselineAlreadySufficientErrorCode only refers the model to an independent final exam. A certification orchestrator is proposed if no current equivalent is found.

**TARGET WORK (proposed decomposition of the original requirement):**

Implement the selected CodeDocumentationCertificationService as a thin owner around trusted admission, durable session reservation, the existing saved-model assessment/calibration route, the existing target/noise predicate and a minimal result receipt.
Required order: validate admitted clean purpose and exact current checkpoint -> validate conditions/budget -> reserve/reopen durable session -> invoke evaluation-only measurement -> require full actual observations and unchanged model/conditions -> apply the accepted final threshold predicate -> persist/reopen the completed receipt -> return a safe projection.
The certification set is intentionally different from the diagnostic baseline/trial set. Do not require equality with the diagnostic EvaluationSetId; require equality with this independently fixed certification contract. Do not reuse diagnostic scores or optimize the candidate after seeing certification output.
No trainer, authoring factory or model-publication service belongs on this path. A valid completed negative exam is not the same as infrastructure failure. Incomplete measurement returns no successful certificate. A successful certificate binds exact checkpoint bytes, corpus/admission identity, evaluator/conditions/policy, observed call identities and result.
Use scripted evaluators and tiny saved models for tests only. Synthetic pass results demonstrate certification wiring, not real model accuracy or actual evaluator effectiveness.
Layering is fixed: ModelServer (25) calls certification orchestration in TrainableRouter (26), which uses the existing ContinuousLearning (14) assessment/admission/store contracts. Do not make ContinuousLearning depend on TrainableRouter or ModelServer, and do not call an HTTP endpoint from core code. Reuse the checkpoint-start owner in 26 rather than recreating its loader.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R4, R5, R6, R7, R8; see `REQUIREMENTS.md`.
**Exit predicate:** The clean certification route is implemented and tested; no real student is certified by this fixture.


PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `26.TrainableRouter/Training/CodeDocumentationCertificationService.cs`
- `Tests/CodeDocumentationCertificationServiceTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.10.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| A | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | 1–290, 315–670, 644–815 | AssessAsync, SnapshotHeldOut, EvaluateCaseAsync, BuildCheckerPrompt, ParseCheckerOutput | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| C | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs` | 1–455 | PlanCalibration, CalibrateAsync, ConditionsMatch, EvaluatorMatches | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| G | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs` | 1–425 | PlanQualityGate, CalibrateBaselineAsync, BaselineRequiresNewTrainingPair, EvaluateTrialAsync, SameExamConditions | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| N | `14.ContinuousLearning/Core/MeasurementNoise.cs` | 1–300 | MeasurementNoisePolicyConfig, BuildPassport, ClassifyDifference, TryValidatePassportForMeasurement | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| H | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentEndpoint.cs` | 1–230 | CodeDocumentationSavedModelAssessmentHttpRequest, MapCodeDocumentationSavedModelAssessment, ExecuteAsync, ExecuteCalibrationAsync | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HT | `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs` | 1–265, 265–660 | CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority, CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker, CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport, CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient, CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| START | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs` | Resolve the named definition locally; no invented line pin | PrepareStart, LoadTrainingAttemptCheckpoint | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |
| CF | `14.ContinuousLearning/Training/RepositoryOwnedReferenceCorpusManifest.cs` | 1–175 | RepositoryOwnedReferenceCorpusManifest | Current manifest/open-result definitions, bounds and sealer entry inspected. Expand full read/seal helpers before editing. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Separate final exam | Diagnostic success triggers a new clean, fixed exam; only that exam produces certification evidence. |
| No learning in assessment | The generated answers are measured but no weights, optimizer or DPO data are mutated. |
| Honest negative | The final exam completed but the target was not proven; this is not a successful certificate. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Return certified from baseline>=0.95 | Skips clean admission and independent measurement. |
| Use the diagnostic corpus for convenience | Breaks the unseen-certification boundary. |
| Train inside the cert service after failure | Turns the exam into a teacher loop. |
| Unit fake scored 100%, so student quality is proven | Confuses contract tests with a real measurement. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Clean passing exam | Valid admission, tiny saved checkpoint and deterministic full passing series. | Completed bound receipt; no trainer/authoring/publisher calls; weights and DPO bytes unchanged. |
| Diagnostic score high | Supply only a sufficient diagnostic report without clean final admission. | Certification not issued; prior diagnostic result is not reused as final score. |
| Clean below-target exam | Valid full series does not meet the accepted target/noise predicate. | Completed negative certification outcome, not false infrastructure success or automatic retraining. |
| Partial/invalid series | Evaluator fails or changes identity mid-series. | No successful receipt; reserved session remains accurately incomplete/exposed. |
| Checkpoint changed | Change saved bytes after admission/before return. | Reject stale result; do not certify whichever model happens to be loaded. |
| Exact replay | Reissue a completed exact session. | Use validated stored receipt with zero new examiner calls and explicitly historical measurement time. |

**Proposed focused class(es):** `CodeDocumentationCertificationServiceTests`  
**Known impacted seed class(es):** `CodeDocumentationSavedModelAssessmentEndpointTests`, `CodeDocumentationCertificationAdmissionTests`, `CodeDocumentationCertificationSessionStoreTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.10` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Freeze service order and receipt contents, keeping evaluation and checkpoint ownership in existing layers. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.10/architect.md` |
| plan-reviewer | Check that clean-set identity differs legitimately from diagnostics and that no training path is reachable. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.10/plan-reviewer.md` |
| executor | Wire the thin service and test full positive, negative, interrupted and stale-candidate paths. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.10/executor.md` |
| code-reviewer | Follow actual calls and receipt persistence; reject diagnostic reuse and unpersisted or partial success. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.10/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- Accepted checkpoint replaced during the series.
- Same model display name but different bytes.
- A completion receipt write fails after calls: do not issue an unpersisted success.
- Valid negative result versus cancelled/invalid exam must remain distinguishable.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.11.md

# S03.11 — Wire certification through server-owned configuration and safe responses

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** The application must reach the real certification service without letting HTTP input select its checker, corpus, score, clean status or weight path.

This is task **12 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.10

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** The application must reach the real certification service without letting HTTP input select its checker, corpus, score, clean status or weight path.

**CURRENT SOURCE / EVIDENCE:** The current server exposes assess-saved and assess-saved/calibrate; its request DTO allows only domain, design pattern, assessment run ID and provenance refs. Unknown fields and missing profiles are already tested.

**TARGET WORK (proposed decomposition of the original requirement):**

Add the reviewed certification route or wire an existing equivalent to the S03.10 service. Proposed route, not an existing route: /internal/continuous-learning/code-documentation/repository-owned-model/assess-saved/certify. Preserve existing assessment/calibration semantics and request rejection.
Resolve certification corpus/purpose/admission authority, checkpoint, evaluator, generation/noise/target policy and budget from server-owned configuration. A caller may request the operation but cannot supply clean=true, an exposure index, expected digest as authority, scores, thresholds, raw corpus or model filesystem paths.
Use strict unknown/duplicate-property rejection consistent with current DTO/config parsing. Validate prerequisites before constructing the evaluation client. Keep loopback/auth and transport constraints as they actually exist; do not claim production internet-facing authorization from local mapping tests.
Return a certification-specific safe receipt, not the existing detailed diagnostic report. Separate completed-negative from incomplete/unavailable outcomes, with appropriate actual project status conventions. Do not expose raw closed references or local paths in errors.
Update existing route registration/readiness metadata only if its current owner requires it. Record that exact additional existing file during architect admission; do not perform an unrelated Program.cs reorganization.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R1, R5, R7, R8; see `REQUIREMENTS.md`.
**Exit predicate:** A tested application entrypoint, not merely a directly callable unused helper.

Candidate existing-source allowlist (architect must narrow before writing):
- `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentEndpoint.cs`
- `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentConfiguration.cs`
- `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs`

PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `Tests/CodeDocumentationCertificationEndpointTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.11.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| H | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentEndpoint.cs` | 1–230 | CodeDocumentationSavedModelAssessmentHttpRequest, MapCodeDocumentationSavedModelAssessment, ExecuteAsync, ExecuteCalibrationAsync | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HP | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentConfiguration.cs` | 1–280 | CodeDocumentationSavedModelAssessmentProfileResolver, ParseProfile, SettingsAreWithinServerBounds | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HT | `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs` | 1–265, 265–660 | CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority, CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker, CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport, CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient, CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| A | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | 1–290, 315–670, 644–815 | AssessAsync, SnapshotHeldOut, EvaluateCaseAsync, BuildCheckerPrompt, ParseCheckerOutput | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| I | `14.ContinuousLearning/Core/LegalTrainingTeacherInference.cs` | 1–260 | ILegalTeacherEvaluationInferenceClient, ITrainingTeacherInferenceClient, LocalQwenOllamaInferenceProfileAuthority | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| START | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs` | Resolve the named definition locally; no invented line pin | PrepareStart, LoadTrainingAttemptCheckpoint | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Real wiring | Loopback request -> server profile -> trusted admission -> cert service -> safe receipt, with observed spies. |
| Server owns the exam | The request has no checker/corpus/score field; strict parsing rejects attempts to add them. |
| Separate diagnostic output | Existing detailed diagnostic response remains diagnostic; final certification has its own restricted projection. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| Route returns hardcoded success | Provides an endpoint but no certification capability. |
| Allow client clean=true | Turns the requester into the authority that should be checked. |
| Use diagnostic profile when certification config missing | Silently changes the exam. |
| Loopback test proves production auth | Overstates the exercised deployment boundary. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Actual mapped path | Start the existing loopback host with synthetic server-owned dependencies and call the route. | Not a 404/stub; response and spies prove the real certification service ran. |
| Caller overrides | Send checker model, corpus path, score, repeatCount, clean flag or target. | 400/actual strict request rejection before factory; not accepted as authority. |
| Missing clean profile | Valid request but no certification corpus/admission configured. | Fail closed before evaluator creation; ordinary diagnostic profile is not fallback. |
| Passing versus negative | Call with complete passing then separate complete below-target fixtures. | Distinct safe outcomes; neither trains or publishes weights. |
| Leakage | Capture successful and error JSON plus permitted logs. | No closed payload, raw path or per-case diagnostic content in certificate projection. |
| Existing routes | Run original assess-saved/calibrate tests. | No authority broadening or response regression to existing callers. |

**Proposed focused class(es):** `CodeDocumentationCertificationEndpointTests`  
**Known impacted seed class(es):** `CodeDocumentationSavedModelAssessmentEndpointTests`, `CodeDocumentationCertificationServiceTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.11` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Resolve actual route/profile/registration files and preserve the current request-authority boundary. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.11/architect.md` |
| plan-reviewer | Trace every forbidden client field and distinguish endpoint wiring from deployment/auth proof. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.11/plan-reviewer.md` |
| executor | Wire the actual service and run loopback positive/negative request tests using existing host conventions. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.11/executor.md` |
| code-reviewer | Verify real service invocation, server-only authority and response projection across both success and failure. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.11/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- Duplicate JSON properties and casing variants.
- Domain/profile mismatch and unsupported domain.
- API retry with same run ID uses exact receipt rules.
- Error message includes server path or examiner raw output.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.


---

## Файл: .kiro/xiigen-slices/luna/s03/tasks/S03.12.md

# S03.12 — Close S03 with candidate-matched integration evidence and S04 handoff

**Luna work order · requested model `gpt-5.6-luna` · reasoning `xhigh`**  
**Human outcome:** Demonstrate the whole selected S03 contract, preserve S01/S02 behavior, and hand off the real-training prerequisites without treating test fixtures as trained-model quality.

This is task **13 of 13**, not a claim that it has run.
Пакет v2: читать этот task, свой role dispatch и `OPERATING-POLICY.md`.
Полномочия — принятый `AUTHORIZATION.md`; repository safety остаётся применимой. Do not load all other S03 task bodies.
Historical v1 source reference (not re-inspected for v2): `ce17708d12a72cf0067c12bfd7fc9ef1dac4b24d`. Planning source: `dfcf26996e20104f66fffb005da6e9c3d8d604cd`.
**Acceptance dependencies (preparation may be independent):** S03.01, S03.02, S03.03, S03.04, S03.05, S03.06, S03.07, S03.08, S03.09, S03.10, S03.11

## 1. Task prompt, acceptance and exact scope

**CURRENT TASK:** Demonstrate the whole selected S03 contract, preserve S01/S02 behavior, and hand off the real-training prerequisites without treating test fixtures as trained-model quality.

**CURRENT SOURCE / EVIDENCE:** This final task evaluates the delivered code and accepted task evidence. No S03 implementation or test results were produced while authoring this pack. S04 is the original real measured-lesson stage, not removed from the program.

**TARGET WORK (proposed decomposition of the original requirement):**

Build one final candidate after the last source edit and run the admitted union of S03 focused/affected classes with S01/S02 regression classes chosen from actual shared changes. Reuse matching completed evidence only when source, relevant configuration/fixtures, runtime and required test coverage are demonstrably equivalent; a changed commit message alone need not force a rerun, but altered source cannot reuse stale binaries.
Run a synthetic end-to-end scenario through the actual entrypoints: admitted diagnostics -> applied/unpublished recovery with same exam -> changed-condition rejection -> distinct clean certification admission -> full final measurement -> safe persisted receipt. Include contaminated-set and malformed/incomplete evaluator negatives. No real external teacher call or domain training occurs without a separate live authorization.
Join each original requirement to its owner, caller, concrete assertion and completed test result. Parse TRX definitions/results rather than relying on a summary string. Inspect early returns and platform guards; Passed may not mean a platform-specific assertion executed. Do not count repeated runs or focused subsets twice.
Separate code integration, automated execution, independent review, real data eligibility, actual model certification, deployment and publication status. Report unresolved S02 holds without rewriting them or automatically blocking unrelated safe preparation.
After independent review accepts the selected S03 implementation, prepare the S04 entry packet using the original queue: exact accepted checkpoint/data/corpus/evaluator identities, any missing real-data/budget/authority, and the next bounded task. Do not start a real lesson or conclude the entire S00-S09 program is complete.

**NOT THE TASK:** no optimizer/RNG redesign, no reopening completed S02 fixes,
no new provider selection, no real certification data exposure, no actual
external-model run, no S04 implementation, and no global process/skill cleanup.

**Original requirement bindings:** R1, R2, R3, R4, R5, R6, R7, R8; see `REQUIREMENTS.md`.
**Exit predicate:** S03 implementation acceptance plus an honest S04 handoff; no automatic real-training or whole-program completion claim.


PROPOSED new files; create only after an equivalent-owner check and plan admission:
- `Tests/CodeDocumentationS03IntegrationTests.cs`

### Полномочия и рабочая область v2

Единый grant — принятый `AUTHORIZATION.md`; поведение — `OPERATING-POLICY.md`.
Он разрешает необходимые commits кандидатов, CI fixes, dispatch/rerun/cancel,
Actions read/write в существующих правах и reviewed in-scope code corrections.
Обычный расширенный файл в том же требовании допускают architect/plan reviewer,
а не новый вопрос владельцу. Grant не разрешает раскрывать реальные экзамены,
менять security policy, удалять чужое или превышать общий бюджет.

Существующий Windows checkout и разрешённый ephemeral CI checkout — по grant.
Проверить actual HEAD/собственный diff, не reset к pin. Один source writer.
Сохранять историю; текущий report и state update не переписывают прошлый evidence.

Один task report: `.kiro/xiigen-slices/work/S03/S03.12.md`.
Каждая роль пишет свою атрибутированную часть, manager — live state/Git/CI.
Results — уникальный run/attempt directory в существующем evidence store либо
скачанный artifact; TEMP/scratch не смешивать с evidence. Не создавать новый ledger.

PROPOSED file/API в списке выше — не требование создавать дубликат. Сверить реального
owner, narrowing scope и замену equivalent; обновить только impacted contract.
Executor — единственный product writer, reviewer — отдельно. При недоступности
работника применяется явный fallback OPERATING-POLICY §4; никакого self-approval.

Не начинать новую задачу ради уже принятого неизменного результата. Локальная
недоступность исполнения не означает отсутствие кода и не запрещает разрешённый CI.

## 2. Exact source context and current-range refresh

| Key | Path | Source-file lines | Required symbols/sections | Read status |
| --- | --- | --- | --- | --- |
| Q | `.kiro/xiigen-slices/04-QUEUE.md` | 1–30 | S03.*, S04, Do not prefill future slices | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| P | `llm mvp/design to code session/09.09.26WorkLeft-TASK-PROMPTS-TRAINING.md` | 1–34 | active opening before historical details | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| A | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs` | 1–290, 315–670, 644–815 | AssessAsync, SnapshotHeldOut, EvaluateCaseAsync, BuildCheckerPrompt, ParseCheckerOutput | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| C | `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs` | 1–455 | PlanCalibration, CalibrateAsync, ConditionsMatch, EvaluatorMatches | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| G | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs` | 1–425 | PlanQualityGate, CalibrateBaselineAsync, BaselineRequiresNewTrainingPair, EvaluateTrialAsync, SameExamConditions | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| N | `14.ContinuousLearning/Core/MeasurementNoise.cs` | 1–300 | MeasurementNoisePolicyConfig, BuildPassport, ClassifyDifference, TryValidatePassportForMeasurement | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| F | `14.ContinuousLearning/Core/FrozenReferenceCorpus.cs` | 1–270, 325–750 | FrozenReferencePartition, PaidReferenceProvenance, FrozenReferenceEntry, FrozenReferenceCorpus.Build | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| I | `14.ContinuousLearning/Core/LegalTrainingTeacherInference.cs` | 1–260 | ILegalTeacherEvaluationInferenceClient, ITrainingTeacherInferenceClient, LocalQwenOllamaInferenceProfileAuthority | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| D | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.cs` | 1–250, 320–625 | ResolveOrAuthorAsync, AuthorPairsAsync, AuthorOnePairAsync, BuildTeacherPrompt | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| H | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentEndpoint.cs` | 1–230 | CodeDocumentationSavedModelAssessmentHttpRequest, MapCodeDocumentationSavedModelAssessment, ExecuteAsync, ExecuteCalibrationAsync | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HP | `25.ModelServer/ContinuousLearning/CodeDocumentationSavedModelAssessmentConfiguration.cs` | 1–280 | CodeDocumentationSavedModelAssessmentProfileResolver, ParseProfile, SettingsAreWithinServerBounds | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| HT | `Tests/CodeDocumentationSavedModelAssessmentEndpointTests.cs` | 1–265, 265–660 | CodeDocumentationModelAssessment_RequestDtoExcludesCallerControlledExamAuthority, CodeDocumentationModelAssessment_PreparedPathLoadsTinySavedModelGeneratesHeldOutAndUsesControlledChecker, CodeDocumentationModelAssessment_CalibrationRepeatsRealAssessmentsAndBuildsNoisePassport, CodeDocumentationModelAssessment_CalibrationBudgetExceededFailsBeforeCheckerClient, CodeDocumentationModelAssessment_CalibrationStopsWithoutPassportWhenRepeatFails | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |
| R | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs` | 529–753, 1002–1264, 1298–1521 | TrainAndPublishAsync, ResumeQualityDecisionFromExistingAttemptAsync | Method windows inspected at prior revision 67c22705; the current tree has the same Git blob 7dccd4ed. Reuse is explicit, not a new full-file read. |
| RT | `Tests/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests.cs` | Resolve the named definition locally; no invented line pin | SameRunRecoveryRemainsEvaluationOnlyWithoutTrainerUpdate, CreateQualityGateContext, CreateAppliedButUnpublishedAttemptAsync | Path/blob verified at the v1 source pin; complete method windows must be resolved in S03.00 or the first consuming architect packet. |
| S2 | `.kiro/xiigen-slices/work/S02.FINAL-EVIDENCE-execution.md` | 1–80 | Result and boundary, matrix | v1 pinned source windows inspected; not a claim of full-file or repository-wide audit. |


Paths without a range are clearly marked resolution work, not invented citations.
The exact existing file ranges above are kickoff windows, not permission to edit
unread code. Read complete affected files before modifications and the full relevant
methods/callers of read-only dependencies. Refresh moved ranges and record the actual
revision/blob. Do not rewrite historical evidence hashes to make a check green.

Also read only the accepted predecessor sections needed by this task in
`.kiro/xiigen-slices/work/S03/<dependency-ID>.md`, plus the active S03 contract from S03.00. A predecessor
report is context, not proof that you read its source or ran its tests.
Do not read real closed exam/reference payloads into an authoring/development prompt;
use synthetic fixtures for the work in this pack.


При v2 эти строки не освежались по GitHub. Разрешить актуальные symbols перед изменением;
сохранить historical hashes как provenance, а не подменять их текущими.

## 3. Skills to load and use

- **architecture**: `docs/skills/trainable-architecture-decision-SKILL.md` — Owner, existing/proposed distinction, actual data flow and failure boundaries.
- **planning**: `docs/skills/phase-planning-SKILL.md` — Live scope, dependency-ready tasks and implementation evidence map.
- **plan review**: `docs/skills/plan-review-SKILL.md` — Contract completeness, no invented authority, all branches before execution.
- **code review**: `docs/skills/generated-code-review-SKILL.md` — Real callers and assertions, evidence matched to the final candidate.
- **human language**: `docs/skills/output-readability-gate-SKILL.md` — Human behavior first, technical evidence second; no jargon-only status.

All four roles receive these five exact skill paths, but read the applicable
sections for their own task. Use `SKILLS.md` for the current startup/intent,
source-map, no-product-decisions, .NET, test-integrity and claim-verification
routes. Follow relevant mandatory dependencies. Fresh contexts perform their own
reads; a previous worker's receipt is not a skill load. Within the same context,
reuse only actually read unchanged sections and say so.

For this task the architecture decision is the behavior described in section1,
not a learned replacement for deterministic admission/validation. Produce a small
normal/failure flow and an implementation evidence map before release. Request
concise decisions and testable reasons, not a transcript of private reasoning.


Операционная дельта v2: менеджер может выполнять обычные GitHub/Git действия без
повторной архитектуры; правила reuse/fallback в OPERATING-POLICY.md §4.

## 4. Positive examples to preserve

| Example | Expected behavior |
| --- | --- |
| Traceable acceptance | Requirement R2 points to a real caller, a changed-condition assertion and the matching completed result. |
| Bounded final claim | S03 integration tests pass and separate review accepts; actual model certification requires a real authorized run. |
| Continue program honestly | S04 handoff names real remaining prerequisites; S03 acceptance is a milestone, not the end of the original queue. |


These examples are synthetic behavioral specifications. They are not reported
measurements of a real model or actual results from this authoring session.

## 5. Negative examples and why they are wrong

| Bad implementation or claim | Why it fails |
| --- | --- |
| 309 old S02 tests prove S03 | They do not establish newly added purpose/exposure/certification behavior. |
| Manifest says PASS | Bookkeeping is not source, execution or review evidence. |
| Leave unused service and accept its unit tests | No user/application route reaches the new capability. |
| Open-ended final audit loop | Adds documentation churn after requirements are already covered instead of making the bounded decision. |


A negative test must fail for the intended reason after valid prerequisite setup;
an unrelated early failure is not evidence of the intended protection.

## 6. Tests, observable assertions and evidence

| Case | Arrange / act | Required observations |
| --- | --- | --- |
| Full positive chain | Run synthetic data through actual diagnostic/recovery/certification entrypoints. | All identities/calls/bundles bound and expected success branches observed. |
| Contaminated final set | High diagnostic score plus teacher-exposed certification source. | No certificate; admission rejects before evaluation. |
| Changed examiner on resume | Saved baseline then one examiner/config change. | No recalibration, no additional training, no promotion. |
| Incomplete final run | Missing/duplicate/malformed examiner result. | No accepted final certificate or partial average. |
| Candidate drift | Edit a governed source after prior test completion. | Old evidence not reused as current; rebuild/test affected candidate before acceptance. |
| Platform no-op | A required test returns before its assertions on this host. | Record unexercised predicate; no cross-platform claim or fabricated pass. |

**Proposed focused class(es):** `CodeDocumentationS03IntegrationTests`  
**Known impacted seed class(es):** `CodeDocumentationSavedModelAssessmentEndpointTests`, `CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests`, `CodeDocumentationRepositoryOwnedModelCheckpointStartServiceTests`, `CodeDocumentationServerOwnedDpoTrainingExecutorTests`, `DpoTrainerOptimizerStateHandoffTests`, `AdamWOptimizerStateRecoveryTests`, `RepositoryOwnedDpoOptimizerStateFileTests`, `LoraRandomSeedTests`

### Проверочный маршрут v2

Списки классов выше — исходные предложения, не автоматическая команда создавать
новые файлы. Реальный reviewed gate для `S03.12` использует существующие определения,
отдельные case IDs и исходные наблюдения из таблицы. Для S03.01 приоритет имеют
восемь actual EndpointTests cases из RESUME.md, не старое proposed имя.

Использовать допущенный существующий CI workflow и `CI-PLAYBOOK.md`. Manager разрешает
конкретный gate из tasks/gate-plan в существующем test-gates/workflow и фиксирует
согласованные focused/impacted scopes; инфраструктурные исправления допущены grant.
Не ограничивать весь S03 статическим S03.01 class-filter. Для последующих задач
адаптировать текущий workflow к их фактическому набору без произвольного shell input.

Полный cycle: preflight по изменившимся условиям → candidate commit → CI build →
required tests → raw evidence → отдельный review → принятие. При source/workflow
правке нужен новый dispatch; при transient на прежнем кандидате допустим rerun.
Не запускать заблокированную local lane снова. Подходящая CI-платформа выбирается
по required assertions, не для получения раннего Passed без проверки.

Сборка: существующий `Tests/LlmFromScratch.Tests.csproj`, единая конфигурация,
существующие зависимости и требуемый SDK. В нужном multi-targeted контексте
применить `-p:EnableWindowsTargeting=true`. Test --no-build только после сборки
этого candidate. Точный фильтр/команды формируются из реально найденных tests,
а не из непроверенного proposed класса; записываются в существующий task report.

TRX: ненулевые executed counts, полный required case set, соответствие definitions,
results и native exits. Не требовать несуществующий skipped attribute. Исключить
platform-no-op, incomplete/failed/NotExecuted. При совпадающих focused/impacted
достаточен один полный набор с явным mapping, если эксперимент не требует повторов.
Метрики измерения, количества повторов и погрешность продукта не сокращать.

Сохранять candidate/ref/DLL/workflow hashes, run/attempt/job, команды и цифровые
exits, exact TRX/log paths, conflicts и cleanup. Не складывать subsets как unique.
Synthetic evaluator fixture не подтверждает реальную обученность модели.

## 7. Roles, orchestration and continuation

| Role | Task-specific responsibility | Exact dispatch prompt |
| --- | --- | --- |
| architect | Confirm the final requirement map and actual impacted test union; no new speculative product redesign. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.12/architect.md` |
| plan-reviewer | Check that final coverage includes all original S03 requirements and separate S04 prerequisites. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.12/plan-reviewer.md` |
| executor | Run the admitted final tests and return exact candidate/input/runtime/results identities. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.12/executor.md` |
| code-reviewer | Independently inspect source, actual assertions and raw results; issue the final bounded S03 verdict and remaining real-world limitations. | `.kiro/xiigen-slices/luna/s03/dispatch/S03.12/code-reviewer.md` |


Accepted unchanged plan/review sections are reused. Instantiate the four task roles
only for the remaining substantive work; do not restart them for each CI GET/cancel
or status-only commit. Normal corrections return to the responsible writer, with
separate review of the affected delta and candidate-matched verification.

The manager performs ordinary CI/Git/state work under AUTHORIZATION.md; workers
remain bounded. If a worker produces no result, isolate its writes and replace it;
confirmed executor unavailability permits labelled parent-executed work. Final
independent review cannot be replaced by self-review. Apply OPERATING-POLICY.md.

Report product implementation, verification and acceptance separately. Missing
runtime proof keeps the task pending, not its whole implementation absent. Continue
independent permitted preparation, then the next dependency-ready task on acceptance.
The original acceptance dependencies remain; do not bypass S03.01 to accept S03.02.

## 8. Edge cases, stopping conditions and return packet

- A late review fix invalidates affected results and must be retested.
- Raw evidence missing locally/remotely: preserve truth, rerun only necessary checks if artifacts cannot be recovered.
- Unknown real corpus exposure remains a deployment/data hold, not synthetic-code evidence of cleanliness.
- S02 cleanup policy denial is not bypassed by S03 acceptance.


If code changes concurrently, inspect only the affected contract/diff and preserve
others' work. Do not reset. An actual defect triggers a bounded correction, not a
new approval request for each file. Security/product changes beyond AUTHORIZATION
remain distinct decisions. A dependency gap is named precisely; historical cleanup
is not an automatic block on all safe S03 work.

For CI/environment failures use CI-PLAYBOOK.md; do not repeatedly probe a known
policy denial. Ordinary cancel/replacement/rerun is allowed by the standing grant,
within its unchanged total budget. A correct build or static review is not a test pass.

Return in Russian: behavior retained/changed, exact failed or passing scenarios,
remaining requirement and next action. Then concise technical receipt:

```text
task_id; role; actual_agent_id_or_UNVERIFIED
requested_model=gpt-5.6-luna; requested_effort=xhigh; observed_runtime_or_UNVERIFIED
actual_skill_reads; source_ranges_and_revision; exact_changed_files
requirement -> owner/caller -> assertion -> test_result -> evidence
candidate_SHA; workflow_SHA; run/attempt/job; numeric_exits; executed_counts
independent_review_verdict; mandatory_findings; next_action
```

Do not dump transcripts or closed exam content. Separate compiler/static/artifact
checks from executed product assertions and actual student quality. Stop a child
only after its bounded handoff; the manager continues. End the whole engineering
chain only at scope completion, explicit owner stop or a genuine remaining external
barrier after permitted alternatives, without promising unstarted background work.
