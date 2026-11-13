# Уточнение ожидания на core003: адресная сверка

**Дата:** 2026-10-21. **Policy cutoff:** core `003083feab72157e54184bb66d0d940f48d836fd`, parent `256f603e70032c11b1f4ffabbdd1fefdab642ea5`; planning остаётся `8346dfb697be43041a477886db26c508453fe5ce`. Source/raw cutoff основного механистического аудита остаётся core256. Этот addendum заменяет прежнее общее правило «после двух пустых наблюдений — диагностика»; остальные заключения предыдущего final review этим не отменяются.

## Что прочитано и сохранено

Получены и сохранены exact bytes ровно пяти назначенных файлов: protocol30, новый protocol31, machine execution contract, required-changes и execution index. **5/5 Git blob SHA совпали.** Локальный readback — `evidence/audit_cards/latest003/<repository path>`, receipt — `evidence/audit_cards/latest003/materialization-receipt.json`. Protected материалы и старые snapshot-копии не изменялись; большого corpus audit, runtime, build/test/train/inference не было.

| Файл в core | Git blob SHA |
|---|---|
| `docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md` | `ae662c0e3fb1e9098818685ac3609accd2beff1c` |
| `docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md` | `dc11e97066f37fe88875504f08d9d169a4f30376` |
| `docs/plans/replanning-possible-diffs/2026-10-20-card-reconciliation/0ea-execution-contract.json` | `8834ba83b29c97b4ac3355dd7ef1dffd599b4077` |
| `docs/plans/replanning-possible-diffs/required-changes.md` | `3a25796787d57d484e3677de973947bf906cbf5c` |
| `docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md` | `71bec2bc6b0527cd9193b5a29e5991506b16a4a1` |

## Правильный wait contract

1. До выдачи команды исполнитель читает фактический каталог и схему launch/wait/status своей среды. Записывает provider, точное имя tool, возвращаемый handle, признак running/terminal, смысл timeout/yield, время выдачи и назначенный deadline. Другой host, модель, набор или рецепт не выбирается. [Protocol31:7–14][p31].
2. Launch выдаётся **один раз**. После running/yield выполняется настоящий bounded wait/resume **того же handle**. Нормальный wait, который действительно ожидал и вернул silent/running, можно повторять до terminal state или исходного experiment deadline. Лимита «не больше двух нормальных waits» нет. [Protocol30:7–9,16–18][p30]; [Protocol31:18–25][p31].
3. Два быстрых **неблокирующих** snapshot/read без новой информации означают: заменить быстрый polling настоящим wait. Они не означают, что процесс неисправен, и не назначают автоматически независимую диагностику. [Protocol30:9][p30]; [Protocol31:22][p31].
4. Диагностика назначенных файлов и собственного handle/PID/start/exit требуется, если wait сам неисправен, мгновенно ошибается либо каналы противоречат друг другу. После конечной диагностики живую операцию снова ждут; не запускают entry заново. [Protocol30:11–18][p30].
5. Один wait — не более60 секунд либо меньше по схеме provider, для отзывчивости. Это **не** предел длительности обучения и не основание уменьшить training budget. STOP/NEXT пользователя обрабатывается сразу; обычное ожидание не требует нового разрешения. [Protocol30:16–18,62][p30]; [Protocol31:21–25][p31].
6. `yield_time`, RPC/tool timeout и experiment/phase deadline — разные события. Yield/RPC timeout не доказывает process exit и не разрешает повтор. Только при истечении назначенного experiment/phase deadline применяются правила остановки **доказанно своего** процесса и сохранения partial; если сам provider документированно убил процесс, устанавливается terminal факт. [Protocol31:27–37][p31].

**Готовая формулировка для активной карточки/промпта:**

> До запуска прочитай фактическую схему launch/wait/status своей среды. Выдай entry один раз и сохраняй возвращённый handle. Нормальный bounded wait того же handle при silent/running продолжай до terminal state или первоначального experiment/phase deadline; число таких waits не ограничено двумя. Два быстрых неблокирующих snapshot без новой информации замени настоящим wait. Только при неисправном wait или противоречивых каналах выполни конечную независимую проверку назначенных файлов и своего PID/start/exit, после чего живую операцию продолжай ждать. Yield/RPC timeout не равен exit и не является experiment deadline. Не запускай повтор, не меняй root, не уменьшай TRAIN ради длительности одного tool call. По STOP/NEXT немедленно обработай указание пользователя.

## Очередь, статусы и результаты не изменились в проверенной дельте

Полностью разобраны old/new machine JSON. В 47 queue rows изменилось только добавление `tool_wait_contract`, ровно в 47/47. В 37 future_outputs изменилось только то же поле, в 37/37. IDs, source statuses, переходы, consumed membership и назначенные runtime commands этих строк не изменились. Active next остаётся PA01; затем corrected08 и ремонт открытой N63C постановки, без автоматического повторения/пропуска. **47 исходных ID, 10 consumed, 37 unissued** сохраняются. [Contract:6–7,1251–1260][contract].

Изменены conflict_policy и universal_execution_rules: теперь отдельно названы два nonblocking snapshots, healthy silent wait и wait error. Они не добавляют измерений. Execution index изменил только вводное wait-правило. В required-changes добавлены верхний кандидат ремонта постановки/writer/counters и новое wait-уточнение; старые строки измерений N63C остались, сдвинувшись на четыре строки. N63C candidate теперь расположен в :147–155, прежние margin/token numbers в :153. **Новых учебных чисел или результатов по этим пяти документам нет.** Число802 и отделение complete probe от старого A-only опыта сохраняются на source/raw cutoff256. [Required:3–7,147–155][required].

В новом верхнем candidate слова «учебный эффект и полный A/B не установлены» относятся к дефекту старой постановки/сохранения и не отменяют отдельный partial gain completed probe. Нынешняя документация пакета продолжает отделять эти свидетельства.

## Какие формулировки требовали замены в нашем пакете

На первом readback устаревшее общее условие two-observations→diagnosis оставалось в 04:79, 08:39, SiLU alias card:33/prompt:11, POS card:70/prompt:15 и предыдущем final review:34. Активные card/prompt/registry должны использовать новый условный контракт выше. Раздел08 и прежний review могут сохранить описание core256 только с явной исторической маркировкой и ссылкой на этот superseding addendum.

В POS card фразу «при timeout завершает свой процесс» нужно ограничить **experiment/phase deadline**, чтобы не принять RPC/yield timeout за разрешение kill. Новый policy cutoff003 следует указать отдельно от unchanged source/raw cutoff256; 47/10/37 и combined50 пересчитывать не требуется. Изменения этих авторских файлов выполняет владелец пакета; этот review сам их не меняет.

## Ограничение конкретных названий tools

Protocol31:45–47 корректно различает process `session_id`, functions `cell_id` и worker wait. В нашей текущей среде схемы `exec_command`/`write_stdin`, `functions.wait` и `collaboration.wait_agent` доступны; continuation применяется только после соответствующего возвращённого handle. Их нельзя переносить как Kiro API.

**`clock.sleep` из protocol31:48 не подтверждён callable-каталогом этой проверяющей сессии.** В доступном `ALL_TOOLS` найден `clock__curr_time`, но sleep не найден. Это не утверждение о невозможности такого инструмента у автора или в другой среде. Правильное ограничение для нашего пакета: не hardcode `clock.sleep`, `execute_pwsh` либо выдуманный discovery API; получать реальную схему доступного tool. Уже доступный same-handle wait предпочтителен. При отсутствии штатного wait fallback с timer допустим только при доказанно устойчивом background handle и реально доступном документированном timer, как условно сформулировано в31:23.

## Статус проверки

Дельта003 подтверждена по exact readback. Требуемые адресные изменения перечислены выше; закрытие авторских формулировок фиксируется отдельным коротким readback в этом же файле, без нового полного review. Ни открытие tool schema, ни арифметика JSON не являются runtime-проверкой экспериментального host. Модель проверяющего — default текущей сессии; применён прежний targeted skill scope.

## Закрытие адресных замечаний

После сообщения автора о готовности выполнен только назначенный readback изменённых формулировок. **Все замечания к активному wait-контракту закрыты; обязательных незакрытых исправлений в этом scope нет.**

- POS card:70 и prompt:15 теперь фиксируют `tool_wait_binding`, фактическую discovery/schema, ≤60 секунд на один wait, два быстрых nonblocking snapshot→настоящий wait, неограниченное числом два повторение healthy silent/running wait, конечную диагностику только при неисправности/противоречии и исходный deadline. Termination явно ограничен experiment/phase deadline; RPC/yield timeout им не является. 1 GenerateExamples /2 Train /128 Predict не изменены.
- SiLU alias card:33 и prompt:11 содержат тот же исправленный контракт. Они остаются review alias без самостоятельного host/запуска; отсутствие конкретного tool не заменяется выдуманным именем.
- 04:3/79 и JSON теперь используют policy003, сохраняют source/raw256 и прежний приоритет открытой N63C. JSON содержит `tool_wait_contract` и новый `tool_wait_policy`; 47/10/37 и combined50 сохранены.
- В08:39 и разделе5 явно описано superseding уточнение003; :69 сохраняет ограничение неподтверждённого `clock.sleep`. Предыдущий final review получил banner с исторической границей core256 и ссылкой сюда; старое описание two-observations→diagnosis больше не является текущим правилом.
- README:3, 07:3 и корневой навигационный индекс различают позднюю policy delta и исходные учебные свидетельства. В `data/latest-wait-readback.json` проверены core003, пять файлов и verified=5.

Эта адресная проверка не открывает новый цикл corpus review и не разрешает runtime. Из авторских файлов проверяющий изменил только этот addendum; поправки карточек и навигации внёс владелец пакета.

[p30]: https://github.com/lubochka/llm_mvp_core/blob/003083feab72157e54184bb66d0d940f48d836fd/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md
[p31]: https://github.com/lubochka/llm_mvp_core/blob/003083feab72157e54184bb66d0d940f48d836fd/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md
[contract]: https://github.com/lubochka/llm_mvp_core/blob/003083feab72157e54184bb66d0d940f48d836fd/docs/plans/replanning-possible-diffs/2026-10-20-card-reconciliation/0ea-execution-contract.json
[required]: https://github.com/lubochka/llm_mvp_core/blob/003083feab72157e54184bb66d0d940f48d836fd/docs/plans/replanning-possible-diffs/required-changes.md
