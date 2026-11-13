# Russian status and execution log template

Этот template нужен для будущих chat/status reports. Он делает статус понятным человеку и не позволяет прятать смысл за opaque labels. Сам template не доказывает product behavior or final readiness.

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
`after_governance_repair_spawn_or_wait_next_product_review_worker`: for this chain, the next slice is `final review pass 4/50`.
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

If product/code/QA work remains, fill these fields in the human status before any
final/status boundary:

```text
next_product_code_slice:
current_worker_or_recovery_worker_id:
current_worker_or_recovery_worker_status: running | accepted | recovery_started | reviewed | rejected
accepted_product_packet_count:
```

If the parent says the rule already exists or reports `NO_PATCH` and stops
without starting/reviewing the next product/code/QA worker or narrowed recovery
worker, classify `false_stop_rule_presence_not_enforcement`. A product-worker
`shutdown`, `not_found`, interrupted, no packet, or 0 accepted evidence is a
recovery queue, not a true external blocker.
`status/final after governance repair must name the next product slice worker or true external blocker`: when product/code/QA execution remains, status/final after governance repair must name the active product slice and worker/recovery worker, or record a true external blocker; `shutdown`, `not_found`, no packet, interrupted, and 0 accepted evidence are recovery queues, not true external blockers.

## VISIBLE_RUSSIAN_MUST_BE_READABLE_GATE

Visible Russian in this template and in Russian progress/status output must be readable Russian. Do not emit mojibake, broken encoding, or machine-looking text where the user needs a human status.

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
until the parent translates it into this canonical Russian block.

```text
Фаза X из Y: <человеческое название>. Смысл фазы: <что эта фаза решает для проекта/пользователя>.
Шаг X.Y из X.N: <человеческое название>. Смысл шага: <что этот шаг доказывает/чинит>.
Статус: DONE | IN_PROGRESS | NEEDS_FIX | REVIEWING | BLOCKED_WITH_REASON — <простое русское объяснение>.
Всего: <общее количество фаз/шагов/работы, к которому относится статус>.
Сделано: <что реально завершено, с evidence boundary>.
В процессе: <что сейчас выполняется и кем/чем>.
Осталось:
1. <оставшийся шаг и смысл>.
2. <оставшийся шаг и смысл>.
Review нашёл: <что проверка нашла, или "пока не проверялось">.
Что исправлено: <что изменено, или "пока ничего не исправлялось">.
Следующее действие: <следующее bounded действие или NONE>.
final/status/report/commit-push/status-board is not a stop while authorized work remains: <required when remaining_count > 0>.
Evidence: <optional one short support sentence or 1-2 compact evidence lines for routine progress>.
```

If `remaining_count > 0`, the status must explicitly say the line
`final/status/report/commit-push/status-board is not a stop while authorized
work remains`. The canonical block above is the user-visible status; supporting
JSON, tables, ledgers, file paths, or claim fields may follow only as evidence.
For ordinary commentary progress, the support tail is capped at one `Evidence:`
sentence or 1-2 short lines. Use the full claim/evidence schema only for worker
return packets, acceptance/rejection reviews, final summaries, and explicit
audit/evidence answers.

Reject the visible packet as `STATUS_FORMAT_HUMAN_GATE_FAILED` when it is a
machine-only packet, raw `claim_scope` block without the Russian phase/step/
status board, bare label such as `NEEDS_FIX`, `packet accepted`, `QSG`,
`boundary pass`, a STATE/JSON path, an informal partial board, or a formal
`Фаза...` opening followed by a long raw evidence dump that makes the message
unskimmable.

Bad: `Фаза 9... Статус: REVIEWING... claim_scope: ... evidence_scope: ...
count_claimed: ... subagent_id_or_NONE: ... downgrade_needed: ...`.
Good routine progress: one Russian paragraph or board, then one short
`Evidence:` sentence.

## Status board template

```text
Всего:
<весь результат, который еще нужно закрыть, человеческим языком>

Сделано:
<что реально создано/проверено в этой фазе, с exact artifact refs>

Сейчас:
<текущая граница evidence, claim locks, count status, что не закрыто>

Осталось:
<следующий bounded work order и что он не имеет права заявлять>
```

## Current count examples

```text
Всего: нужно довести 6 треков до packet-backed evidence: runtime Claude/Cursor/Codex, token before/after 9 пар, quality scoring 9 строк, learnability/DPO/lifecycle 9 строк, docs/QA и 10 final reviews.
Сделано: docs update prep packet committed/pushed; docs-edit phase applied documentation coverage; docs QA review checks documentation only.
Сейчас: full UI/chat readiness не закрыта; token complete pairs 0/9; quality rows ready for actual scoring 0/9; trainable DPO rows 0/9; actual lifecycle rows 0/9; final readiness reviews 0/10; claim locks false.
Осталось: next independent runtime/token/quality/learnability evidence track; no final readiness claim until real evidence exists.
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
Current phase: DOCS-QA-DOCUMENTATION-UPDATE-REVIEW-01_NO_FINAL_READINESS. Это фаза только docs QA review: она проверяет updated docs, создает review evidence artifacts and may fix docs wording inside the allowed docs scope. Она не запускает runtime/eval/training/model commands.

Current sub-agent: bounded docs QA review sub-agent.

Changed files: <exact file list>.

Next bounded work order: RUNTIME-CLAUDE-CURSOR-CODEX-LIVE-EVIDENCE-PLAN-OR-REPAIR-01, or a more precise missing-evidence work order named by state.
```

## How to avoid opaque labels

Bad:

```text
STATUS: PASS.
```

Good:

```text
Checks passed for documentation coverage only: JSON files parse, changed allowed paths pass git diff --check, and no-overclaim scan found no positive readiness claim. This does not prove runtime, token savings, quality improvement, learning or final readiness.
```
