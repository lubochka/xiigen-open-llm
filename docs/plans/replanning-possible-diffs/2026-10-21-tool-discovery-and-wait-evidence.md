# Поиск инструментов и ожидание: подтверждённые локальные ссылки

21 октября 2026. Worker `/root/rp01_primary_evidence`; READ_ONLY_RESEARCH + этот Markdown. Эксперименты, builds/tests/Train/inference, изменения skills/AGENTS/config/runner/raw и управление чужими процессами не выполнялись. Проверялись документы и доступные описания tools, а не работоспособность будущего Kiro запуска.

## Вывод и конкретная исправленная ошибка

Локальные навыки для проверки предположений, чтения настоящих сигнатур и сопровождения активной работы существуют. Однако в проверенных каталогах не найден отдельный `tool-router`/`framework-discovery`/`context-discovery` skill или полный Kiro process/wait API reference. Это ограниченный результат поиска, не утверждение об отсутствии таких средств во всём компьютере или в Kiro. Утверждение исполнителя о жёстком двухминутном пределе **не доказано**. Timeout канала, yield инструмента и exit/kill процесса — разные события; судьба процесса устанавливается по фактической схеме provider и его receipts, а не по названию кнопки.

В прежнем [контракте30](experimets/30-failure-recovery-and-learning-validity.md:7) правило «два одинаковых ответа — сменить диагностику» было слишком широким: нормальный bounded wait может вернуть running без stdout, хотя процесс исправно вычисляет. Finding передан parent. Адресный readback после его правки подтвердил:

- :7–9 исключают настоящий блокирующий/event wait из лимита двух быстрых snapshot/read; повторный wait идёт по тому же handle.
- :16 различает максимум одного ожидания для отзывчивости и общий experiment deadline; timeout/yield не объявляется exit.
- :18 запрещает auto `results-NN`, сокращение обучения под недоказанные две минуты и повторную выдачу команды «теперь синхронно на25минут».

Это закрывает конкретное противоречие с :16 прежней версии, которая уже разрешала ждать живой процесс до назначенного deadline. Прежний [мой evidence](2026-10-21-n63c-rng-learning-evidence.md:48) поддерживал ограничение бесполезной диагностики, но не отделял штатный wait достаточно явно. Его узкий вывод здесь уточнён; он не доказывал невозможность ожидания в Kiro.

## Реальные локальные навыки и предел их применимости

| Проверенный источник | Точное полезное правило | Что источник не доказывает |
|---|---|---|
| [Kiro one-slice adapter](C:/Projects/llm_mvp_core/.kiro/skills/xiigen-code-slice/SKILL.md:10), :10–13,27–37 | Adapter не даёт tool permissions; читать реальные применимые зависимости; разрешать пути/версии на текущем checkout, не выдумывать registry. Inventory не означает загрузку каждого навыка для каждой роли. | Наличие у конкретной Kiro сессии launch/wait API и его параметры. |
| [Kiro always steering](C:/Projects/llm_mvp_core/.kiro/steering/xiigen-continuous-development.md:9), :9–10,22–24 | Сохранять actual permission boundaries; repository text не разрешает paid calls/training/environment changes. | Право исполнять опыт из нынешней исследовательской задачи. |
| [Kiro manual steering](C:/Projects/llm_mvp_core/.kiro/steering/xiigen-luna-through-s02.md:17), :17–20 | Queue не даёт разрешения, manual steering не доказывает выбор модели. | Override модели нашего исследования или будущего исполнителя. |
| [Kiro Luna adapter](C:/Projects/llm_mvp_core/.kiro/skills/xiigen-luna-through-s02/SKILL.md:7), :7–15 | Читать настоящий start/skills/role packet; не подменять principal skills адаптером; активация не разрешает операции. | Tool schema и доказательство runtime recovery. |
| [Kiro skills registry](C:/Projects/llm_mvp_core/.kiro/xiigen-slices/luna/skills.json:3), :3–4,62–77,90–142 | `observed_ref` и inspected ranges — происхождение старого чтения, не текущая полная загрузка; routing/dependencies связаны с ролью. JSON разобран целиком, текст144строки прочитан. | Реестр инструментов. Это registry навыков, не список API процесса. |
| [Phase Preflight](C:/Projects/llm_mvp_core/docs/skills/phase-preflight-SKILL.md:48), :48–69,75–100 | Каждое предположение проверять по реальной сигнатуре/интерфейсу, при несовпадении менять план. | Наличие конкретной функции в Kiro по аналогии с Codex. |
| [Systematic Debugging](C:/Projects/llm_mvp_core/docs/skills/systematic-debugging-SKILL.md:78), :78–85,124–140,167–188 | Прочитать точную ошибку и цепь; сравнить с действующим reference; неизвестное не выдавать за известное; не повторять случайные fixes. | Право воспроизводить эксперимент, добавлять instrumentation/tests в READ_ONLY_RESEARCH. Команды примеров сейчас не исполняются. |
| [Task Slice Golden Rule](C:/Projects/llm_mvp_core/docs/skills/task-slice-golden-rule-SKILL.md:63), :63–69,109–118 | Timeout/cancellation ACK не доказывает termination. Healthy active worker требует supported wait/status/checkpoint; не заменять живую работу второй. | Совпадение worker lifecycle с любым OS-process provider; точные runtime параметры берутся из schema. |
| [Code Execution](C:/Projects/llm_mvp_core/docs/skills/code-execution-SKILL.md:226), :226–242 | Для сложной команды назначать script-file, stdout/stderr/exit и exact evidence; oversized inline retry не лечит tooling failure. | Автоматическое разрешение запускать новый script после уже выданного опыта. Безусловное применение старой фразы :237 «prep/execution» здесь запрещено текущим scope/consumed-attempt rule. |
| [Reconnaissance Gate](C:/Projects/llm_mvp_core/docs/skills/reconnaissance-gate-SKILL.md:173), :173–196 | DESIGN_DOC отличается от implementation и reconciliation; текст навыка не доказывает «работает». | Readiness Kiro runtime; численные review/read floors из иных поручений не создают новые циклы текущего bounded поиска. |
| [Infrastructure Discovery](C:/Projects/llm_mvp_core/docs/skills/infrastructure-discovery-SKILL.md:70), :70–81 | Проверять живые boundaries/signatures, не полагаться на память. | Его Step0 :50–55 с full tests сейчас неприменим: исследовательская фаза не разрешает тесты. Это не подходящий tool-wait reference. |

Проверка наличия `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/.kiro` вернула отсутствие каталога. Адаптеры выше находятся в canonical `C:/Projects/llm_mvp_core/.kiro`, не в planning cwd. Их нельзя называть auto-loaded конкретным внешним исполнителем без отдельного evidence.

## Что именно подтверждено схемами текущего Codex

Источник этого раздела — объявленные инструменты **этой** сессии, а не найденный файл с Kiro API. Для discovery доступны `ALL_TOOLS` с name/description и `tools` в `functions.exec`; по метаданным обнаружены context-mode инструменты. Само нахождение имени не заменяет чтение схемы конкретного вызова. Не следует придумывать `list_tools`, когда среда его не объявляет.

| Контракт текущего инструмента | Семантика |
|---|---|
| `exec_command` | Возвращает output и при продолжающемся процессе `session_id`. `yield_time_ms` — максимум первого ожидания; текущая Windows schema указывает диапазон10000–30000ms. Этот предел не назван длительностью процесса. |
| `write_stdin` | Принимает **тот же** `session_id`; пустой `chars` читает/ожидает продолжающийся процесс. Для пустого poll schema допускает5000–300000ms; текущий рабочий предел отдельного ожидания ≤60000ms уже, чтобы оставаться отзывчивым. Не вводить повторную launch-команду в stdin. |
| `functions.exec` / `functions.wait` | Только ответ `Script running with cell ID ...` даёт `cell_id` для `functions.wait`. Это ID orchestration cell, не OS PID и не `exec_command.session_id`. Повтор `functions.exec` не является resume старой cell. |
| `clock.sleep` | Ждёт интервал, может завершиться по новому вводу; возвращает elapsed. Это timer, не доказательство exit. |
| `collaboration.wait_agent` | Ждёт обновление mailbox. Timeout не означает завершение worker и не разрешает второго worker вопреки лимиту. Это отдельный lifecycle от процесса. |

Из этих контрактов следует лишь поддерживаемый способ ожидания в текущем Codex. Параметры и названия нельзя переносить в Kiro. Здесь не запускался длительный демонстрационный процесс, чтобы «доказать» возможности API.

## Неустановленное в Kiro и правильная граница вывода

Имена из инцидента вроде `execute_pwsh`, `Control Process`, `Read File`, `List Processes` — подписи вызовов, не полная документация. Они не устанавливают ни максимальное ожидание, ни сохранение дочернего процесса после RPC timeout, ни наличие отдельного resume. Независимый reference реальной Kiro schema в проверенном локальном срезе не предъявлен. Нельзя утверждать ни «точно две минуты», ни «точно выдержит синхронные25минут».

Исполнитель в своей среде должен до уже разрешённой выдачи команды прочитать доступные launch/wait/status/file tools и назначить один `tool_wait_binding`: provider/tool, источник schema, тип handle, ongoing/terminal markers, timeout semantics, original deadline. Это выбор технического канала для готового рецепта; не выбор другого опыта/модели/датасета и не просьба к Любе выполнить команду. Если схема не покрывает долгую операцию, сначала проверить другой **уже доступный** provider; отсутствие нужного контракта фиксировать точно, не устанавливать runtime/плагины и не раздувать разрешения самостоятельно.

После ISSUED известный handle сохраняется. Тихий wait возвращается к wait до изначального deadline. Быстрый повторный snapshot без ожидания переключается на штатный wait; неисправный wait/противоречивые каналы — на одну независимую файловую и одну process/exit проверку. Неизвестная судьба остаётся UNKNOWN, не never-started. `results-NN`, перезапуск на25минут или сокращение шагов до120секунд не являются диагностикой.

## Адресный readback новых общих документов

[Контракт31](experimets/31-tool-discovery-and-wait.md) сначала прочитан полностью в64строковой версии, затем после добавления refs — полностью в78строковой версии. Актуальные :9–12 требуют actual provider schema/binding; :20 разделяет IDs; :21–24 допускает повторный настоящий wait и timer fallback для доказанно устойчивого handle; :31–35 разделяет три времени; :41–50 ограничивает Codex schema; :58–68 добавляет5реальных skill refs и явно отделяет worker-supervision от process schema, не выдумывая Kiro API; :72–76 запрещает claimed2min, user-run и auto results-NN. Существенных оставшихся противоречий по этому узкому срезу не найдено. Это review документов, не проверка исполнения Kiro/Codex и не семантический review всех карточек.

Рекомендуемые ссылки для общего contract31 — Kiro adapter :10–13/27–37, registry :3–4/62–77, Phase Preflight :48–69, Systematic Debugging :82–85/128–140, Task Slice :65–67. Они дополняют реальную схему tools; не заменяют её. Parent владеет contract30/31 и массовыми карточными правками; worker их не менял.

## Фактический охват

- Полностью прочитаны текущий contract30 (64строки), предыдущий N63C evidence (62строки), contract31 (сначала64, затем78строк), Systematic Debugging (197строк), четыре Kiro skill/steering файла (42/15/24/20строк), Kiro skills.json (144строки, полный JSON parse).
- Адресно прочитаны Code Execution :180–250, Task Slice :49–140, Phase Preflight :41–100, Infrastructure Discovery :41–82, Reconnaissance :80–210; эти чтения не названы полными загрузками навыков.
- Установленный `C:/Users/lubao/.codex/plugins/cache/context-mode/context-mode/1.0.169/skills/context-mode/SKILL.md` прочитан целиком для маршрута большого анализа. Выполнен один readonly context-mode поиск; его индекс/совпадения не считаются семантическим review всех найденных файлов. Затем точные источники читались адресно, без нового эксперимента. Крупный первоначальный поиск зацепил исторические work/results; его усечённый вывод не использован для claim полного покрытия.
- `C:/Users/lubao/.codex/skills/.system/openai-docs/SKILL.md` прочитан как доступный каталогный источник, но не использован как Kiro reference. Web не применялся по границе этого поручения; текущие Codex утверждения ограничены предоставленной schema.
- Имена/API искались в canonical docs/skills, .kiro/skills, .kiro/steering; точный поиск tool-router/framework-discovery/context-discovery также просмотрел названия локальных навыков и canonical AGENTS/CLAUDE. Полная семантическая загрузка этих огромных governance файлов и всего skills корпуса здесь не заявляется. Секреты и значения конфигурации не печатались.

Единственная запись worker в этом срезе — этот evidence Markdown. Нет новых runtime результатов, доказанной Kiro API-схемы, claim всех skills/all-card review или измерения экономии от context-mode.

## Финальная выборочная проверка применения

После сообщения parent выполнен свежий readback раздела1 contract30 (:1–22), всего contract31 (:1–78), верхних :1–45 карточки [N63-correction-collision.md](experimets/cards/N63-correction-collision.md:6) и всего standalone [SFT-EXP-REUSE-01-executor.md](experimets/prompts/SFT-EXP-REUSE-01-executor.md:6) (:1–33). Усечённые части объединённого вывода прочитаны отдельно; наличие текста не выдано за runtime capability.

Оба образца :6/:8 требуют discovery по реальной schema и повторный нормальный wait того же handle; :14/:16 сохраняют docs-only scope и приоритет нового блока. Дополнительно текущие N63C :29 и embedded prompt :193 прочитаны после concurrent правки parent: прежний широкий лимит read/poll заменён «два быстрых пустых → wait»; штатное silent/running ожидание продолжается. Нет сохранившегося запрета ждать больше двух раз в проверенных конкретных абзацах. N63C остаётся consumed partial; инструкция об ожидании не назначает повтор его запуска.

В этом адресном срезе не обнаружены выдуманный Kiro API, приравнивание yield к exit, перенос60секунд на весь эксперимент или разрешение auto results-NN. Это **ограниченный no-findings по названным документам и двум образцам**, не проверка parent-счётчиков209/155/12 и не181независимый semantic review. Остаток прежний: фактическая schema Kiro исполнителя и runtime-работоспособность его wait здесь не проверены.
