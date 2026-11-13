# Russian status and execution log template

Этот шаблон нужен для будущих статусов в чате и журналов выполнения. Он делает
видимый отчёт понятным человеку и не позволяет прятать смысл за внутренними
метками. Сам шаблон не доказывает поведение продукта или финальную готовность.
(EN: This template is for future chat/status reports. It makes the status understandable to a human and does not allow meaning to be hidden behind opaque labels. The template itself does not prove product behavior or final readiness.)

Заполненный шаблон ещё не доказывает читабельность. Для примеров рабочего
заказа, пояснений в диаграммах, блоках кода и таблицах, а также для больших
переписываний действует центральный двухслойный договор
`docs/skills/output-readability-gate-SKILL.md`: сначала человеческий смысл и
проверочный фрагмент, затем технические поля как доказательства.

## HUMAN_HEARTBEAT_STATUS_GATE

For long-running agents/tools, every visible update starts with a simple human sentence: what is running, what is known, what is not accepted yet, and what happens next. Never start progress with only `claim_scope`, `evidence_scope`, or other machine fields.

`ROUTINE_PROGRESS_COMPACT_EVIDENCE_GATE`: ordinary commentary heartbeat/progress
updates must be a short human paragraph or compact human board first. They may
omit the full claim/evidence schema. If evidence fields are useful in routine
commentary, keep them to one short `Evidence:` sentence or at most two short
support lines. Full claim/evidence schema belongs to worker return packets,
acceptance/rejection reviews, final summaries, and direct audit/evidence
answers. A report that formally starts with `Фаза...` but then dumps a long raw
claim/evidence block is still `STATUS_FORMAT_HUMAN_GATE_FAILED` when the visible
message is not skimmable.

Для прогресса по коду, продукту или обучению первая фраза говорит по-русски:
зачем это нужно продукту, что уже существует или изменилось сейчас, что
проверяется, и какое следующее полезное действие. Не начинать со списка всего,
чего нет. Счётчики использовать только когда известны осмысленные продуктовые
шаги. Честная граница нужна, но компактная: реальные блокеры видны, отчёт не
превращается в стену отрицаний.

## PRODUCT_LEVEL_HUMAN_ACCEPTANCE_REPORT

Использовать этот блок для кусков кода, продуктового статуса, принятия работы,
отчёта о commit/push или артефакта рядом с обучением. Для обычного короткого
heartbeat-а весь блок не обязателен.

```text
Что пользователь сможет сделать теперь:
<поведение продукта и один пример входа/выхода или сценария>

Как работает алгоритм без кода:
<доменная цель, шаги по порядку, где проходит логика>

Что это НЕ путает с обучением:
<что является проверкой, сохранением и повторным открытием; что меняет обученное состояние; был ли настоящий запуск обучения с метриками>

Ожидалось / фактически проверено:
<ожидаемое поведение; фактические успешные, ошибочные/отрицательные и сохранительные случаи; что тесты не доказали>

Архитектура/поток, если менялись:
<ссылка на живую UML/диаграмму/симуляцию веток или "не менялись">

До первого запуска обучения осталось:
<самые маленькие оставшиеся шаги из текущего кода/плана>

До готовности к измерению и повторному использованию осталось отдельно:
<метрики, проверочный набор, повторное открытие/повторное использование, экспорт/импорт/откат, если они ещё нужны>

Опубликовано сейчас / ещё не опубликовано:
<что уже в опубликованном состоянии, что только локально или ждёт commit/push>
```

Плохой пример commit/status: `reopen-from-saved-DPO; task identity;
repository-owned grouping; 18 fake-case checks; sealed checkpoint publication;
next slice`. Он проваливается, если не объясняет поведение до/после, зачем это
нужно продукту, как работает алгоритм, что реально проверено, и является ли
контрольная точка обучением или только проверкой сохранения и повторного
открытия.

Хороший условный пример для этого продукта, который нельзя выдавать за уже
проверенный факт без доказательства: "Загружены предыдущие веса модели разбора
задач. Модель-проверяющий нашла ошибку на отдельной задаче. Разрешённая
открытая китайская модель получила старые DPO-пары только как примеры хорошего
и плохого ответа, без отложенных экзаменационных задач/ответов, и предложила
исправленную новую DPO-пару из лучшего и худшего плана; ответ платной модели
остаётся только внешним якорем качества, не стороной DPO-пары и не учебной
строкой. Новая пара сохранена с историей до обучения. Существующие веса дообучены только новой парой,
а не переобучены заново по всей базе. Модель-проверяющий повторно оценила
качество на отдельных задачах. Ожидалось: ошибка уйдёт без ухудшения на
контрольных задачах. Фактически: <результат оценки>. Если качество хуже,
следующее действие — полный технический откат к состоянию до последней пары и новая
исправленная пара. Если такой оценки не было, отчёт обязан сказать: обучение и
качество ещё не проверены. Технические проверки могут подтвердить хранение,
загрузку и технический откат, но сами по себе не доказывают качество модели."

`HYBRID_STATUS_PACKET_DUMP_FORBIDDEN`: a formal human board plus a long raw
claim/evidence packet is unreadable hybrid status, not readable progress. Reject
`Фаза/Шаг/Статус` text followed by `claim_scope`, `evidence_scope`,
`count_claimed`, `independent_evidence_count`, `subagent_id_or_NONE`, or
`downgrade_needed` as the main body; reject phase/status prose followed by a raw
worker packet or machine inventory. The parent must summarize the worker packet
in one or two human sentences or a compact board first. Routine progress may add
only one short `Evidence:` sentence or 1-2 compact support lines; full schemas
belong only in worker return packets, acceptance/rejection reviews, final
summaries, or explicit audit/evidence answers.
formal human opening plus raw claim/evidence/inventory/subagent packet dump is
`STATUS_FORMAT_HUMAN_GATE_FAILED` when the raw packet is the main visible
content or longer than 1-2 compact evidence lines.

Parent must translate worker machine fields into human language before showing
them to Luba. Do not quote raw `claim_scope/evidence_scope` blocks as the main
status. For spawned workers, `subagent_id_or_NONE: NONE` is a format defect; the
worker must use `UNKNOWN_IN_SUBAGENT` when it cannot see its id, and the parent
must reconcile the actual platform id before accepting the packet.
`POST_GOVERNANCE_PATCH_RESUME_PRODUCT_EXECUTION`: after a governance human-gate
patch, active product execution must resume through the next authorized product
worker or recovery route; governance repair is not completion.
`PRODUCT_CODE_CONTINUATION_ONLY`: after governance/skill/rule/agent-file repair
for a live product/code/QA task, the visible status must name the next
product/code/QA execution worker or narrowed recovery worker for the concrete
`audit_union_row_id` or `next_product_code_slice`. Another governance worker,
read-only scan, status package, `NO_PATCH`, rule readback, or packet-formatting
repair is not product/code continuation.
`PRODUCT_WORKER_NO_PACKET_ZERO_ACCEPTED_REQUIRES_RECOVERY`: `shutdown`,
`not_found`, interrupted, no packet, or 0 accepted product evidence from the
product/code worker is zero accepted product work and must be shown as a
recovery queue, not as a final blocker.

A wait timeout must be reported as: agent still running / no result accepted yet / next heartbeat or recovery step.

`not_found`, `interrupted`, or `shutdown` must be reported as: 0 accepted result / recovery worker or narrowed work order, not as Luba's blocker.

If Luba asks whether she must debug agents, answer no: the parent owns orchestration recovery.

Keep updates short but not silent.

### NO_FALSE_STOP_AFTER_GOVERNANCE_REPAIR

`false_stop_rule_presence_not_enforcement`: `rule presence is not enforcement`.
After any governance/skill/guide/agent-file repair or `NO_PATCH`, a visible
status/final is invalid while product/code/QA work remains unless it names the
next product slice worker/recovery worker or a true external blocker.
`NO_PATCH from a governance worker is not product progress`.
`governance repair is not product continuation evidence`.
`status/final after governance repair must name the next product slice worker or true external blocker`.
Shutdown, `not_found`, interrupted, no packet, or 0 accepted product evidence
must be explained as recovery for the next product slice, not as a stop.

### PRE_FINAL_CONTINUATION_ENFORCEMENT_STATUS

`pre_final_continuation_enforcement_required`: before visible status/final/hold/no-safe-work/completion, show the continuation check in human language: active task open, next bounded product/review/governance action, pending packet verification, required passes remaining, next worker started or waited this turn, and true external blocker certificate.
`parent_must_run_pre_final_check_before_status_final`.
`final_packet_forbidden_when_next_bounded_action_exists`.
`governance_repair_acceptance_requires_next_worker_started_or_true_blocker`.
`after_governance_repair_spawn_or_wait_next_product_review_worker`: after a governance repair, continue the current explicitly ordered bounded result; do not hard-code an old phase such as a past review pass number into the universal template.
`NO_PATCH_requires_product_continuation_check`.
`rule_text_present_but_not_enforced_is_failure`.
`subagent_wait_heartbeat_required_before_wait`.
`one_active_agent_does_not_mean_wait_silently`.

If governance was repaired or `NO_PATCH` returned and product/review work remains, status must say the next worker was started, waited, or reviewed, or name exact true external blocker proof. Otherwise classify the old answer as `premature_final_after_governance_repair` and continue the next bounded product/review slice.

## POST_GOVERNANCE_PRODUCT_SLICE_STATUS_GATE

After governance, skill, guide, prompt, or agent-file repair, the visible human
status must say what product slice is currently running next. Governance repair
is not completion. `rule presence is not enforcement`, and `NO_PATCH from a
governance worker is not product progress`.

`governance repair is not product continuation evidence`: a governance/skill/
guide/agent-file repair, verification, or no-change result does not satisfy the
next product/code/QA execution slice.

If product/code/QA work remains, the visible human status must answer these
meanings in Russian or the operator's language before any final/status
boundary: which product/code/QA slice continues next; who is running or
recovering it; whether that work is running, accepted, under recovery, reviewed,
or rejected; and how many product packets have actually been accepted. Internal
packets may map those meanings to `next_product_code_slice`,
`current_worker_or_recovery_worker_id`,
`current_worker_or_recovery_worker_status`, and
`accepted_product_packet_count` after the human explanation.

If the parent says the rule already exists or reports `NO_PATCH` and stops
without starting/reviewing the next product/code/QA worker or narrowed recovery
worker, classify `false_stop_rule_presence_not_enforcement`. A product-worker
`shutdown`, `not_found`, interrupted, no packet, or 0 accepted evidence is a
recovery queue, not a true external blocker.
`status/final after governance repair must name the next product slice worker or true external blocker`: when product/code/QA execution remains, status/final after governance repair must name the active product slice and worker/recovery worker, or record a true external blocker; `shutdown`, `not_found`, no packet, interrupted, and 0 accepted evidence are recovery queues, not true external blockers.

## VISIBLE_RUSSIAN_MUST_BE_READABLE_GATE

Visible operator-language text in this template and in operator-language progress/status output must be readable (Russian for the original author). Do not emit mojibake, broken encoding, or machine-looking text where the user needs a human status.

## STATUS_FORMAT_HUMAN_GATE_ALWAYS_REQUIRED

Every progress/status/review/final/worker packet visible to Luba must pass this
human gate before any machine block, raw packet, JSON, ledger row, STATE path,
validator label, or other opaque status. `STATUS_FORMAT_CHALLENGE_REPAIR` is a
special case of the same gate: when Luba challenges the status format, including
`Формат статуса работы не соответствует требованиям` or the misspelled `Формат
статуса работы не соответсвует требованиям`, `не читабельно`, `нечитабельно`,
or `format lost`, answer first, classify `status_format_governance_gap` or
`STATUS_FORMAT_HUMAN_GATE_FAILED`, patch or verify the governing rule, and only
then continue execution. A returned sub-agent packet is not user-visible status
until the parent translates it into this canonical operator-language status block. The canonical template below is shown as an example in the operator's language (original author's Russian):

```text
Фаза X из Y: <человеческое название>. Смысл фазы: <что эта фаза решает для проекта/пользователя>.
Шаг X.Y из X.N: <человеческое название>. Смысл шага: <что этот шаг доказывает/чинит>.
Статус: <простое русское объяснение; внутренний код вроде DONE/NEEDS_FIX можно добавить после смысла>.
Всего: <общее количество фаз/шагов/работы, к которому относится статус>.
Сделано: <что реально завершено, с evidence boundary>.
В процессе: <что сейчас выполняется и кем/чем>.
Осталось:
1. <оставшийся шаг и смысл>.
2. <оставшийся шаг и смысл>.
Review нашёл: <что проверка нашла, или "пока не проверялось">.
Что исправлено: <что изменено, или "пока ничего не исправлялось">.
Следующее действие: <следующее ограниченное действие или "нет" — человеческое название каждого шага, номер строки работ только в скобках после названия; список голых номеров запрещён>.
Если работа ещё осталась: это промежуточный статус, а не остановка; следующий ограниченный шаг указан выше.
Evidence: <optional one short support sentence or 1-2 compact evidence lines for routine progress>.
```

If `remaining_count > 0`, the visible status must express in Russian/operator
language that this is interim status, not a stop, and name the next bounded
action. The English anchor `final/status/report/commit-push/status-board is not
a stop while authorized work remains` may appear only as an internal/reference
anchor after the human sentence. The canonical block above is the user-visible
status; supporting JSON, tables, ledgers, file paths, or claim fields may follow
only as evidence.
For ordinary commentary progress, the support tail is capped at one `Evidence:`
sentence or 1-2 short lines. Use the full claim/evidence schema only for worker
return packets, acceptance/rejection reviews, final summaries, and explicit
audit/evidence answers.

Reject the visible packet as `STATUS_FORMAT_HUMAN_GATE_FAILED` when it is a
machine-only packet, raw `claim_scope` block without the operator-language phase/step/
status board, bare label such as `NEEDS_FIX`, `packet accepted`, `QSG`,
`boundary pass`, a STATE/JSON path, an informal partial board, or a formal
`Фаза...` opening followed by a long raw evidence dump that makes the message
unskimmable.

Bad: `Фаза 9... Статус: REVIEWING... claim_scope: ... evidence_scope: ...
count_claimed: ... subagent_id_or_NONE: ... downgrade_needed: ...`.
Хороший короткий прогресс: один абзац или board на языке оператора, затем одна
короткая строка `Evidence:` при необходимости.

## Status board template

```text
Всего:
<весь результат, который еще нужно закрыть, человеческим языком>

Сделано:
<что реально создано/проверено в этой фазе, с точными ссылками на артефакты>

Сейчас:
<текущая граница доказательств, закрытые/открытые утверждения, счётчики и что не закрыто>

Осталось:
<следующий ограниченный заказ работы и что он не имеет права заявлять>

(English rendering from DTC:)

Total:
<all the work that still needs to be closed, in human language>

Done:
<what was actually created/verified in this phase, with exact artifact refs>

Now:
<current evidence boundary, claim locks, count status, what is not closed>

Remaining:
<next bounded work order and what it is not allowed to claim>
```

## Current count examples

```text
Всего: нужно закрыть шесть направлений доказательствами: живой запуск в
хостах, сравнение расхода токенов на девяти парах, оценку качества по девяти
строкам, проверку обучаемости и жизненного цикла модели по девяти строкам,
проверку документации и десять финальных проверок.
Сделано: подготовка обновления документации опубликована; документационная
правка покрыла разрешённые разделы; проверка документации относится только к
документам.
Сейчас: живой UI/chat путь ещё не закрыт; для токенов, качества, обучаемости и
жизненного цикла принято 0 из 9; финальных проверок принято 0 из 10; утверждения
о готовности остаются закрытыми.
Осталось: запустить следующий независимый трек доказательств по runtime,
токенам, качеству или обучаемости; финальную готовность нельзя заявлять без
реальных доказательств.

(English rendering from DTC:)

Total: close six evidence tracks: live host run, token comparison on nine pairs,
quality scoring on nine rows, learnability/model lifecycle on nine rows,
documentation review, and ten final reviews.
Done: documentation-update preparation is published; the documentation edit
covered the allowed sections; documentation QA proves documents only.
Now: the live UI/chat path is not closed; token, quality, learnability and
lifecycle tracks have 0 of 9 accepted rows; final reviews have 0 of 10 accepted
records; readiness claims remain closed.
Remaining: run the next independent runtime, token, quality, or learnability
evidence track; do not claim final readiness without real evidence.
```

## Claim lock table

| Claim | Current value | Human explanation |
|---|---|---|
| token savings claimed | false | Token pairs are 0/9 now. |
| quality improvement claimed | false | Quality rows ready for actual scoring are 0/9. |
| full semantic quality claimed | false | Scoring and controls are not complete. |
| learnability claimed | false | Trainable DPO rows 0/9 and lifecycle rows 0/9. |
| full runtime readiness claimed | false | Full UI/chat readiness is not closed. |
| common model readiness claimed | false | Common source/material/lifecycle proof is not present. |
| target promotion claimed | false | No target mutation/promotion happened in docs-edit. |
| locator update claimed | false | No locator update happened in docs-edit. |
| final readiness claimed | false | Final reviews counted 0/10. |

## Execution log fields

Эта таблица — техническая справка после человеческого статуса, а не основной
видимый отчёт.

| Field | Fill with |
|---|---|
| Current phase | Work order id and plain explanation. |
| Current sub-agent | Sub-agent id or "bounded docs-edit sub-agent". |
| Changed files | Exact paths only. |
| Evidence artifacts | Exact artifact folder and files. |
| Allowed verification | Exact checks; do not list forbidden commands as run. |
| Forbidden actions preserved | Runtime/eval/training/DPO/checkpoint/model/external AI/git add/commit/push when applicable. |
| Claim locks | Explicit false/true table with human explanation. |
| Next bounded work order | Exact id and scope. |

## Next bounded work order template

Use an exact work order id and scope. The next work order must say what it may prove and what it may not prove.

## Example phase note

```text
Текущая фаза: проверка документации после правок. Она проверяет обновлённые
документы, создаёт доказательства проверки и может исправлять формулировки
только в разрешённых документах. Она не запускает продукт, оценку качества,
обучение или команды модели. Техническое имя фазы:
DOCS-QA-DOCUMENTATION-UPDATE-REVIEW-01_NO_FINAL_READINESS.

Текущий исполнитель: ограниченный проверяющий документации.

Изменённые файлы: <точный список файлов>.

Следующий ограниченный заказ: проверка недостающего живого evidence через
runtime/host-сценарий или более точная восстановительная задача, названная в
текущем состоянии. Техническое имя может быть
RUNTIME-CLAUDE-CURSOR-CODEX-LIVE-EVIDENCE-PLAN-OR-REPAIR-01.
```

## How to avoid opaque labels

Bad:

```text
STATUS: PASS.
```

Good:

```text
Проверка прошла только для покрытия документации: JSON-файлы читаются,
разрешённые изменённые файлы проходят проверку пробелов в diff, а поиск
завышенных заявлений не нашёл положительной заявки на готовность. Это не
доказывает runtime, экономию токенов, улучшение качества, обучение или
финальную готовность.
```

## Внутренний номер никогда не стоит отдельно (`NO_BARE_IDENTIFIER_RULE`, 23.08.26)

Правило для любого статуса, heartbeat-а, коммит-сообщения и строки «Следующее
действие» от исполнителя, рецензента и родителя-архитектора: любой внутренний
номер или код (строка работ W-NNN, разрыв G-NN, решение D-NN, сценарий S-NN,
пункт №NN, фаза P0..P6, гейт QSG, hash, полное имя теста FQN, файл результатов
TRX, имя класса) допустим ТОЛЬКО в скобках сразу после полного человеческого
названия и цели — каждый раз, а не только при первом упоминании. Голый номер,
список номеров, стрелочная цепочка номеров и строка «Ссылки: …» запрещены;
вместо строки «Ссылки» пишется «Где это записано: <человеческое название>
(номер N в файле <название файла>, секция …)». Перед отправкой — «тест
незнакомого читателя» (человек, не знающий ни одного номера, понимает каждую
фразу) и автопроверка grep по
`\bW-[0-9]+\b|\bG-[0-9]+\b|\bD-[0-9]+\b|\bS-[0-9]+\b|\bQSG[0-9]*\b`: каждое
совпадение — в скобках после человеческого названия, иначе отчёт
переписывается. Нарушение = `bare_identifier_to_owner` (-500; повторное после
замечания Лубы -1000).
(EN: An internal identifier never stands alone in owner-facing text — human
name first, number in parentheses after it, every time; bare ids, id lists,
id chains and "References:" lines are violations for executor, reviewer and
parent alike; the "References" line is replaced by "Where this is recorded:
<human name> (number N in file <file name>, section …)".)

Плохо (цитата Лубы 23.08.26):

```text
Ссылки: W-203, W-169.
W-203 полный прогон на HEAD → ре-ревью 2 подозрительных коммитов (W-169) → W-197/W-200/W-201 гигиена.
```

Хорошо:

```text
Дальше исполнитель: (1) прогоняет все тесты проекта на опубликованном коде, чтобы увидеть реальные поломки (строка работ 203); (2) заново проверяет два коммита, где ожидания тестов могли подогнать под код вместо починки кода (строка 169); (3) убирает мусор из рабочего каталога: перестаёт отслеживать папку результатов тестов, чистит временную папку тестов, удаляет временную рабочую копию (строки 197, 200, 201).
Где это записано: план оставшейся работы, раздел кодовых изменений (строки 169, 197, 200, 201, 203 в файле 23.08.26WorkLeft-01-code-changes.md).
```
