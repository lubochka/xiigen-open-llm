# HOW-TO-USE-SKILLS.md — When to load which skill

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or "почему" in Russian), challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Sub-Agent Work-Order Completeness Gate

Every bounded sub-agent work order must include explicit skill-load guidance and the complete schema below. A generic prompt that only names a target file and broad instruction is invalid. If a sub-agent receives a generic prompt, the parent must interrupt and replace it before accepting work.

Required skill load guidance for DPO/DynamicGraph plan repair:

```text
SK-SESSION: planning-session-startup-SKILL.md + SESSION-START-PROMPT.md + AGENTS.md
SK-INTENT: architect-behavior-classifier-SKILL.md
SK-PLAN: authority-requirement-binding-SKILL.md + reference-inventory-and-source-map-SKILL.md + phase-planning-SKILL.md
SK-REVIEW: plan-review-SKILL.md + goal-delivery-completeness-SKILL.md + claim-verification-SKILL.md + audit-protocol-SKILL.md + specificity-calibration-SKILL.md + output-skepticism-SKILL.md + reconnaissance-gate-SKILL.md
SK-ARCH: trainable-architecture-decision-SKILL.md + no-product-decisions-SKILL.md + output-readability-gate-SKILL.md
SK-DOC/PARTA when plan touches existing code: implementation-doc-uml-SKILL.md + implementation-doc-algorithms-SKILL.md + implementation-doc-code-refs-SKILL.md + implementation-doc-status-SKILL.md + implementation-doc-hierarchy-SKILL.md
```

If a work order lacks this explicit skill block or an equivalent current-task-specific skill block, it is invalid and must be interrupted/replaced.

```text
CURRENT TASK:
NOT THE TASK:
EVIDENCE-ONLY:
required_skills_and_gates:
context_files_to_read:
implementation_or_artifact_details:
original_task_requirements_to_cover:
mental_simulation_required:
good_examples:
bad_examples:
allowed_files:
forbidden_files:
allowed_commands:
forbidden_commands:
verification_required:
return_packet_schema:
claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed:
```

Good sub-agent prompt: names exact skills, exact attachments, exact active files, exact requirements, good/bad examples, pass count, nonparallel rule, verification commands, return schema, and no git authority unless explicitly needed. Bad sub-agent prompt: `Fix active-codex plan readiness` or `Search NOT_READY and fix wording` without skills, task attachments, examples, coverage matrix, and simulation gates.

## Pre-Final Continuation Enforcement Skill Routing

When any loaded skill, guide, parent, or worker is about to emit or accept status, review, no-safe-work, completion, hold, PASS, or final wording, route through these enforceable markers:

- `pre_final_continuation_enforcement_required`: require fields for active task open, next bounded product/review/governance action, pending packet verification, required passes remaining, next worker started/waited this turn, and true external blocker certificate.
- `parent_must_run_pre_final_check_before_status_final`: the parent must run the check before status/final, including after governance repair, rule readback, or `NO_PATCH`.
- `final_packet_forbidden_when_next_bounded_action_exists`: final/status/completion/hold/no-safe-work is forbidden while next bounded work, pending packet verification, or requested-pass deficit remains without exact true external blocker proof.
- `governance_repair_acceptance_requires_next_worker_started_or_true_blocker`: governance repair can be accepted only with same-chain proof that the next product/review worker was started, waited for, or reviewed, or with a true external blocker certificate.
- `after_governance_repair_spawn_or_wait_next_product_review_worker`: for the current 50-pass review chain, route after governance repair to `final review pass 4/50`.
- `NO_PATCH_requires_product_continuation_check`: `NO_PATCH`, PASS, NEEDS_FIX, timeout, usage limit, partial packet, worker silence, and stale status must route to continuation/recovery unless true external blocker proof exists.
- `rule_text_present_but_not_enforced_is_failure`: existing rule text is not enough; acceptance must be blocked when the check finds next work.
- `subagent_wait_heartbeat_required_before_wait`: before every wait/poll, emit a human-readable heartbeat.
- `one_active_agent_does_not_mean_wait_silently`: one-active-agent mode requires bounded heartbeat waits, not silent waiting or final status.

If a previous answer stopped after governance repair while next bounded work existed, classify it as `premature_final_after_governance_repair`, verify the repair readback, then continue the next product/review slice instead of ending at the repair.

## Strict Plan Readiness / Original Task Simulation / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers and complete coverage of the original task. The plan must be mentally simulated as if implemented and must cover every original-task requirement. Future Gate B may contain execution actions such as tests/builds/screenshots/mergeability checks. Future Gate B may NOT hide unchosen legal provider, missing input/output contracts, missing dual-path design, unresolved repo ownership, missing external API seam design, missing provenance policy, missing thresholds, missing model/checkpoint lifecycle, or missing source-to-plan mapping. If a future gate hides a design/dependency decision, verdict is PLAN_NOT_READY.

30 sequential simulation/review passes are required for this readiness class: one pass at a time, no parallelism. Each pass records `requested_model: gpt-5.5`, `requested_reasoning_effort: xhigh`, skill focus, original-task requirement checked, plan section checked, mental simulation step, coverage verdict, finding, repair/no-fix reason, and recheck result. A single worker doing 30 internal passes must call them sequential simulation/review passes, not 30 sub-agents or 30 independent sub-agent passes.
Later stricter full-task coverage evidence dominates earlier narrow ready verdicts. If any later sequential simulation, reviewer packet, or attachment reports NOT_READY, zero FULLY coverage, uncovered original-task requirements, or unresolved design/dependency blockers for the same plan, the agent must immediately downgrade current readiness to PLAN_NOT_READY before any implementation or final ready claim. The downgrade must be propagated to active plan artifacts plus the relevant skills/guides/agent files that allowed the stale claim; a plan-only wording patch is incomplete.

PLAN_NOT_READY after full-task coverage simulation is not a final reporting boundary. When implementable design/dependency blockers remain, immediately convert the blocker inventory (for active-codex Pass06 and later: B-1..B-22 unresolved blocker inventory is mandatory; Missing blocker numbers are trace defects, not permission to drop them; older B1-B21/B5-absent evidence must become explicit trace-repair backlog, not a dropped row) into sequential bounded sub-agent plan-repair passes and continue until the plan reaches readiness or a true external blocker remains after mitigation/decision-prep. A final answer after only downgrading readiness is a protocol violation unless all implementable blockers have been repaired, delegated to active workers, or proven external/non-actionable. Minimum-pass requirements are evidence-bound: if Luba requires 50 passes with sub-agents one-at-a-time, run or receive 50 inspectable sequential sub-agent packets/records before claiming 50; no parallelism and no ledger-only inflation. Governance/skill repair must precede plan repair when the stop came from a governance loophole, but governance repair is not itself a completion boundary.

Readiness-repair work orders must name the skills, source attachments, active files, original-task requirements, good/bad examples, pass-count and evidence rules, verification commands, and explicit propagation targets. Attachment-reported simulations are evidence only; do not claim the worker ran those passes unless the worker actually ran or received independently inspectable records for them. Old PLAN_READY_FOR_LUBA_REVIEW may remain only as historical/pre-attachment/narrower-repair text with current PLAN_NOT_READY nearby.

Future gates are execution-continuation checkpoints, not stop permissions. After Luba approves a truly ready plan, gates are executed through bounded sub-agents until the product is complete. If a gate fails, the next bounded sub-agent fixes/mitigates or records a true external blocker. Do not stop/finalize while safe bounded work remains.

Any historical/pre-repair/evidence-only `NOT_READY`, `BLOCKED`, `FAILED`, `READY`, `DONE`, or similar status retained as evidence must be labelled historical/pre-repair/evidence-only, with the current verdict nearby. If a current artifact still makes Luba ask whether stale `NOT_READY` remains, the repair is incomplete.

Good future gate: `Run dotnet tests after implementation; if red, bounded fix worker repairs failing slice.` This is execution, not a design decision. Bad future gate: `Choose legal provider during implementation` or `figure out FLOW-38 input contract later`; these are design blockers, not execution gates. Good historical status: `Attachment 429f reported the pre-repair plan as NOT_READY; current verdict after repair is PLAN_NOT_READY because provider remains unchosen.` Bad historical status: leaving `NOT_READY` in a source map without saying historical/pre-repair and without current verdict next to it.

## Sub-Agent Work-Order Completeness Gate

Every bounded sub-agent work order must include:

```text
CURRENT TASK:
NOT THE TASK:
EVIDENCE-ONLY:
required_skills_and_gates:
context_files_to_read:
implementation_or_artifact_details:
good_examples:
bad_examples:
allowed_files:
forbidden_files:
allowed_commands:
forbidden_commands:
verification_required:
return_packet_schema:
claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed:
```

A generic prompt that only names a target file and broad instruction is invalid.
If a sub-agent receives a generic prompt, the parent must interrupt and replace
it before accepting work. Good sub-agent prompt: names exact files, exact
skills, exact accepted context, exact implementation/design details, exact good
examples, exact bad examples, exact verification commands, exact return schema,
and no git authority unless explicitly needed. Bad sub-agent prompt: `Fix
active-codex plan readiness` with no skills, context, examples, or verification.

## Strict Plan Readiness / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers. Design/dependency
decisions must be solved inside the plan before implementation. Future Gate B
may contain execution actions such as tests/builds/screenshots/mergeability
checks. Future Gate B may NOT hide unchosen legal provider, missing input/output
contracts, missing dual-path design, unresolved repo ownership, missing external
API seam design, missing provenance policy, missing thresholds, or missing
model/checkpoint lifecycle. If a future gate hides a design/dependency decision,
verdict is PLAN_NOT_READY, not PLAN_READY_FOR_LUBA_REVIEW.

Future gates are execution-continuation checkpoints, not stop permissions. After
Luba approves a truly ready plan, gates are executed through bounded sub-agents
until the product is complete. If a gate fails, the next bounded sub-agent
fixes/mitigates or records a true external blocker. Do not stop/finalize while
safe bounded work remains.

Any historical/pre-repair/evidence-only `NOT_READY`, `BLOCKED`, `FAILED`,
`READY`, `DONE`, or similar status retained as evidence must be labelled
historical/pre-repair/evidence-only, with the current verdict nearby. If a
current artifact still makes Luba ask whether stale `NOT_READY` remains, the
repair is incomplete.

Good future gate: `Run dotnet tests after implementation; if red, bounded fix
worker repairs failing slice.` This is execution, not a design decision. Bad
future gate: `Choose legal provider during implementation` or `figure out
FLOW-38 input contract later`; these are design blockers, not execution gates.
Good historical status: `Attachment 429f reported the pre-repair plan as
NOT_READY; current verdict after repair is PLAN_NOT_READY because provider
remains unchosen.` Bad historical status: leaving `NOT_READY` in a source map
without saying historical/pre-repair and without current verdict next to it.

## Claude Baseline Plan Authority Gate

Execution cannot start until active-codex contains all Claude baseline details, references, and explanations plus only validated Codex deltas.
Claude baseline defines repo scope unless Luba explicitly expands it.
Discovered/copied workdirs are not repo authority.
50-pass plan authority review requires 50 independently inspectable evidence records before claiming completion.
Missed reference propagation is a planning-gate failure, not an execution question.

Application: for Claude/Codex/active-codex planning repair, Claude is the baseline authority; Codex may only add validated non-contradictory deltas/source-map details. Any active-codex open question answered by Claude or Codex evidence must be downgraded from open to resolved/partial with exact source evidence before execution work can be issued.

PASS25_OWNER_CANON_FIELD_COUNT_PROPAGATION_GATE: When the Claude baseline changes an owner-canon field count or schema, active-codex owner copies and all owner/consumer references must propagate the exact new count, fields, gates, and status, or label the old text historical/pre-Pass25 evidence-only. Example: `TeacherProvenance` 18 -> 20 fields with RR6 `service_mode`/`grounding_used`, C17 service-mode/grounding/search ban, and pending provider-terms behavior must be reflected in the active owner plan, source map, board, matrix, gate, ledger, relevant guides/policies, and governance before B-22 closure, readiness, or implementation authorization. Any stale unqualified field-count/schema reference is active-vs-Claude drift and blocks closure.
PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, active-codex must propagate exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.

PASS27_ORIGINAL_TASK_COVERAGE_AND_LATEST_BASELINE_RECONCILIATION_GATE: Latest inspectable Claude cleanup/resimulation plus original-task coverage simulation dominates older active-codex plan-polish/status. Active-codex cannot claim readiness, B-22 closure, or implementation authorization until every original-task requirement is mapped to the active plan, source map, board, matrix, and coverage gate, and every latest inspectable baseline detail is reconciled. Stale field counts, phase counters, source-to-plan targets, pilot rows, provider/terms status, scorer defaults, and repo-host roles must be fixed or labelled historical/pre-repair evidence-only. Recon-library and source-to-plan gaps, including target-64 map gaps or minimum-pass deficits, remain PLAN_NOT_READY blockers, not future execution gates.

FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: When Luba explicitly grants FULL ACCESS or the environment reports danger-full-access/unrestricted filesystem with approval policy never, the agent must not request permissions, must not set `sandbox_permissions`, and must not open approval prompts for filesystem work needed by the active plan/governance repair. Permission prompting under full access is a governance failure: answer Luba first, classify the old behavior as a permission-prompt loophole, patch this gate if missing, then continue the authorized repair loop. Full access does not authorize unrelated product implementation, destructive git operations, external provider calls, or publishing unless the active task explicitly includes those actions; it does authorize plan/governance/skill/guide/agent-file repairs in the named planning library and project guidance without asking again.
PASS21_CLAUDE_REPAIR2_IN_FLIGHT_GATE: If current Luba evidence quotes a Claude-side `claude-*` Repair2, resimulation, or workflow as running, edited, pending, or not yet inspectable, active-codex cannot claim B-22 closure, plan readiness, implementation authorization, or accepted Claude-baseline alignment from that workflow. Record the evidence as `CLAUDE_REPAIR2_RUNNING_OR_UNVERIFIED` until the exact workflow output and changed `claude-*` files are directly inspected. Do not race or overwrite the same authoritative Claude repair slice from active-codex while it is in flight unless Luba explicitly redirects that exact file/slice. After Claude Repair2 returns, the next bounded planning pass must refresh the source map and reconcile active-codex against the fresh Claude baseline before any readiness claim.

## v27.5: governance library organized by layer, with authority-chain binding, exact source-map discipline, review gates, trainable architecture decisions, human-readable architecture audit gates, audit sufficiency/delta-severity gates, DPO/set quarantine, skill injection, testing, documentation sync, and completion evidence rules

---

## Load order: always start here

    Step 1: cat CLAUDE.md + AGENTS.md                            (every session)
    Step 2: read the active plan/state and identify the allowed verification
    Step 3: load session-constitution-SKILL.md                   (every session)
    Step 4: load session-type skill (table below)
    Step 5: load activation triggers if applicable

Project-local override:

```text
AGENTS.md and CLAUDE.md outrank every skill file.
No skill may introduce a user-facing STOP, approval wait, branch switch, or full
dotnet test unless Luba's current instruction or the active plan Gate B requires
it.
```

Luba plan-quality and executor-skill-stack override:

```text
When Luba asks which plan is better, compare by Luba's criteria:
human clarity, readable architecture, architectural reliability, evidence
honesty, requirement fidelity, execution-control quality, and low overclaim
risk.

Newer status tables, file counts, ledgers, count freshness, or claimed pass
counts are delta evidence only. Use valid deltas to patch the stronger plan;
do not call bookkeeping freshness the better plan.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current
task. If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.
```

Skill-local words such as `STOP`, `hard stop`, `must be on main`, `run dotnet
test`, or `wait for answer` are internal checks only. They do not override:

```text
continue while next_action is known;
use the current branch unless Luba explicitly asks to switch;
run only the verification allowed by the active phase;
answer in Luba's language;
commit/push when the phase deliverable requires it.
```

Before any commit/push boundary, failed verification output is a hard fail even
if a later commit command succeeds. Native PowerShell wrappers must check
`$LASTEXITCODE` immediately after `git diff --check`, tests, JSON/JSONL parse
checks, schema checks, and builds, and abort before stage/commit/push on
nonzero. Do not use command chains that can continue into commit/push after a
required verification command failed.

Before any commit/push boundary, commit grouping must use exact
`required_commit_group` paths from the reviewer/validation ledger. Broad `glob`
patterns may only supplement exact staging, never replace it. Required
`validation artifact` files with dot/dash variants must be listed explicitly and
verified in `git diff --cached --name-only`; any missing required artifact is a
hard fail before commit. A follow-up commit for a missed required validation
artifact is a commit-boundary defect to repair, not normal.

Partial progress is not a stop boundary. A skill may package a phase, write a
report, or recommend a checkpoint, but if the active state has
`goal_reached=false`, `stop_allowed=false`, and a non-empty `next_action`, the
executor must continue that next action. Commit/push, report generation,
readiness proof, or a narrower lower-level gate cannot override the active
plan's continuation requirement.

Soft stops are forbidden. Skill wording such as "continue next", "wait for
approval", "status only", "checkpoint complete", or "handoff" must be converted
to continued execution when the state has a known `next_action`. If such wording
causes a user-facing pause, record it as a severe protocol violation with
`-1000`.

Review and architect repair are mandatory internal loop states. After an
executor slice, load the review/check skill named by the active plan and compare
the artifact to the literal gate. If review finds a protocol or plan gap, switch
to architect repair internally, patch the governing file, record the hole in
state, then return to architect-owned work-order mode. Skill-local review STOP wording cannot
become a user-facing pause while `next_action` is known.

Before any round finishes, the executor must hand the result to
ARCHITECT_INTERNAL. This includes final responses, pause reports, blocker
reports, commit/push completion reports, and answers to coverage questions.
Metrics or executor-produced reports are inputs to the architect verdict, not a
replacement for it. Skipping this architect finish review is an
`IMMEDIATE_DISMISSAL_CONDITION`, `score_delta = -1000`, and revokes executor
authority for the round until architect repair explicitly reinstates it.

Architect owns the session during plan review, governance repair, coverage
challenges, and multi-step plan execution. Executor receives only bounded work
orders with named files, commands, forbidden actions, expected artifacts, state
updates, return condition, and evidence packet. Executor has no standing
authority and cannot send final responses. A hidden JSON review is evidence only,
not the visible architect verdict.

Sub-agent-only override:

```text
When Luba declares the parent session ARCHITECT_INTERNAL, governance-only, or
sub-agent-only, no skill may hand that parent session to executor. Architect can
orchestrate sub-agents, spawn/wait/close them, review returned packets, run safe
read-only verification after a sub-agent packet returns, request bounded fixes,
and perform an explicit final commit/push boundary for accepted sub-agent
output. Architect cannot direct-edit governance/protocol/docs/skills/plans/
reports/state files, cannot write production/test code, and cannot run dotnet
tests, builds, runners, or training. Governance, protocol, skill,
documentation, plan, report, code, execution, and training repairs happen only
through bounded sub-agent work orders while the Parent Architect Subagent-Only
Firewall is active.
Spawned bounded sub-agent is already the sub-agent executor. When the work
order gives allowed write scope, the spawned sub-agent directly creates or edits
those scoped artifacts in its workspace and reports evidence. It must not
attempt to invoke `codex.exe`, CLI Codex, another AI, or another sub-agent to
satisfy "through sub-agents" unless the work order explicitly asks it to
delegate. `codex.exe Access is denied is not a blocker` for a spawned sub-agent
artifact task; if scoped files cannot be written for filesystem reasons, the
sub-agent reports the exact path, attempted operation, and filesystem error
instead of demanding parent direct execution. Parent direct execution remains
forbidden, and sub-agent execution is not parent direct execution.
Verification literals: `Spawned bounded sub-agent is already the sub-agent executor`; `codex.exe Access is denied is not a blocker`; `Parent direct execution remains forbidden`; `sub-agent execution is not parent direct execution`.
Those work orders must require the highest-order requested configuration:
`gpt-5.5 Extra High` (`requested_model=gpt-5.5` /
`requested_reasoning_effort=xhigh`).

HOWTO_SUBAGENT_HEARTBEAT_RECOVERY_GATE:
- Skills routing for sub-agent-only work must load and use AGENTS.md plus
  SESSION_SUBAGENT_HEARTBEAT_RECOVERY_GATE before spawning, waiting, status, or
  final reporting.
- Luba never debugs sub-agent/tooling failures; parent owns recovery through a
  bounded retry, smaller work order, explicit interrupt/replace, or true
  external blocker certificate.
- Parent must send a human-readable heartbeat before wait_agent or any long
  wait, and must keep sending human-readable heartbeat updates during the wait.
- wait_agent timeout, empty status, or no-output status means the worker may
  still be running or there is no accepted result yet; it is not a blocker,
  final packet, completion evidence, or permission to stop.
- not_found, interrupted, shutdown, or no returned packet means 0 accepted
  result and routes to the recovery queue.
- Parent cannot use background local work, sidecar drafting, or direct file
  edits as an escape from sub-agent-only.
- When this task class requires `gpt-5.5 xhigh`, the work order must state
  `requested_model=gpt-5.5` and `requested_reasoning_effort=xhigh`; no silent
  downgrade, lower-tier fallback, or unspecified default is allowed.
- Output-readability gate must reject machine-only status and mojibake in
  visible operator-language status; status/final text must be human-readable before acceptance.
  `STATUS_FORMAT_HUMAN_GATE_ALWAYS_REQUIRED` means every visible progress/status/
  review/final/worker packet must use the canonical operator-language status block before machine
  labels, or be rejected as `STATUS_FORMAT_HUMAN_GATE_FAILED`.
- `ROUTINE_PROGRESS_COMPACT_EVIDENCE_GATE`: ordinary commentary heartbeat/
  progress is not a worker/final/audit packet. It starts with a short human
  paragraph or compact board, may omit the full claim/evidence schema, and may
  show only one short `Evidence:` sentence or two short support lines. Full
  claim/evidence schema belongs to worker return packets, acceptance/rejection
  reviews, final summaries, and direct audit/evidence answers.
- Parent must translate worker `claim_scope/evidence_scope` fields into human
  meaning before showing them to Luba. A formal `Фаза...` opening followed by a
  long raw evidence dump still fails the human gate as
  `STATUS_FORMAT_HUMAN_GATE_FAILED`.
- Load and enforce `HYBRID_STATUS_PACKET_DUMP_FORBIDDEN` when status/readability
  is in scope. A routine Luba-facing status cannot be `Фаза/Шаг/Статус` plus a
  long raw `claim_scope/evidence_scope/count_claimed` packet, `subagent_id_or_NONE`,
  `downgrade_needed`, a raw worker packet, or machine inventory as the main body.
  That hybrid status must be rejected as `STATUS_FORMAT_HUMAN_GATE_FAILED`.
  Parent must translate the packet into a short human summary first; routine
  evidence is one short `Evidence:` sentence or 1-2 compact support lines. Full
  schemas are reserved for worker packets, acceptance/rejection reviews, finals,
  or explicit audit/evidence answers.
- If Luba says `не читабельно`, `нечитабельно`, `format lost`, or equivalent,
  route as a status-format challenge: answer first, classify
  `STATUS_FORMAT_HUMAN_GATE_FAILED` or `status_format_governance_gap`, patch or
  verify the governing rule, then resume the nearest authorized product work.
- PRE_FINAL_EXECUTION_CONTINUATION_GATE must be loaded from AGENTS.md before
  final/status/summary after governance or skill repair. If active execution is
  unfinished and a next bounded execution slice exists, the parent must actually
  start or review that sub-agent in the same turn, or record a true external
  blocker certificate; `I will continue`, `now I continue`, and `next I will`
  are not continuation evidence.
- `governance repair is not product continuation evidence`: a governance/skill/
  guide/agent-file repair, verification, or no-change result does not satisfy
  the next product/code/QA execution slice.
- `rule presence is not enforcement`: reporting that AGENTS.md or a skill
  already contains the rule is not continuation evidence. `NO_PATCH from a
  governance worker is not product progress`; it proves only that the governance
  worker changed no governing text. If the parent stops after rule-readback or
  `NO_PATCH`, classify `false_stop_rule_presence_not_enforcement` and continue
  by starting/reviewing the next product/code/QA worker or narrowed recovery
  worker.
- Human status after governance repair must name the current `product slice`,
  `next_product_code_slice`, worker or recovery-worker id, worker state, and
  accepted product packet count; a status that names only the governance rule is
  not enough while product/code work remains.

Phase propagation routing lessons:
- Exact routing literals: `Machine-only statuses are invalid`; `current-state invariants + evidence packet`; `Repeated self-block drift`.
- If a work order or status can be machine-only, load agent-output-format,
  session-output-contract, claim-verification, and plan-review. Machine-only
  statuses are invalid until the visible packet explains phase/step meaning,
  counts, done/in-progress/remaining, review findings, fixes, and next action.
  If Luba challenges report format, classify `status_format_governance_gap`,
  patch or verify the governing rule, and translate any returned sub-agent
  packet before showing it as status.
- For any operator-language plan/status/review summary, load output-readability-gate and
  enforce `human-readable operator-language text must not contain mojibake`; marker-name
  scan plus direct operator-language readback must pass before final/status. Also enforce
  `rendered/readback operator-language text must be human-readable, not only marker-clean` and
  `mojibake scan must include common UTF-8/Windows-1252 corruption markers and human readback of operator-language sections`.
  The original Russian-anchored literals also remain binding: `human-readable Russian text must not contain mojibake`; `rendered/readback Russian must be human-readable, not only marker-clean`; `mojibake scan must include common UTF-8/Windows-1252 corruption markers and human readback of Russian sections`.
- If evidence could be stretched into readiness, load claim-verification and
  implementation-integrity. Enforce: boundary/API evidence != host readiness;
  marker-only != full local desktop pass; trace != savings; schema != training;
  checkpoint candidate != promotion.
- If new evidence artifacts are untracked, review may use current-state
  invariants + evidence packet instead of an impossible git baseline, but must
  still require exact paths, explicit counts/claim=false, and no readiness
  claim.
- If protocol/governance/skill repair is the current slice, load phase-planning
  and plan-review and enforce No stop gates from protocol repair. After repair
  review, continue the next bounded phase unless a real blocker remains.
- If a packet says no safe work remains, load authority-requirement-binding,
  phase-planning, plan-review, claim-verification, and output-skepticism. Enforce
  `repo/slice-local no-safe-work is not a final stop`;
  `whole-plan no-safe-work requires all participating repos/workstreams audited`;
  `cross-repo safe work must be exhausted before final hold`; and `F0
  Luba-only choices block only decision-dependent product behavior, not
  independent scaffolding/docs/tests/mocks/status/reporting/integration
  harnesses or other repo work`. A repo-local packet may close only the audited
  local slice; a whole-plan hold must audit every participating repo/workstream.
  Required exact literals:
  ```text
  repo/slice-local no-safe-work is not a final stop
  whole-plan no-safe-work requires all participating repos/workstreams audited
  cross-repo safe work must be exhausted before final hold
  F0 Luba-only choices block only decision-dependent product behavior, not independent scaffolding/docs/tests/mocks/status/reporting/integration harnesses or other repo work
  ```
- If a sub-agent repeatedly looks for nested agents, local codex CLI, or parent
  direct execution despite scoped write authority, record repeated self-block
  drift as governance drift and repair or avoid it in the next work order.

They must also require a human-readable progress board with total tasks,
completed count and names, current owner/sub-agent, remaining count and names,
current result type, closed claims with reasons, and next bounded action. Any
review work order must name exact files/sections/hosts/claims in scope before a
sub-agent can count as reviewer evidence.
Sequential Execution-Control Gate: for plan creation, plan review, execution
status, or complaints such as `разброс`, `прыжки по фазам`, `где мы`, `сколько
осталось`, `done/in-progress/remaining`, `phase/step/status`, and equivalents,
load SK-PLAN and SK-REVIEW and require a separate `current_execution_pointer`,
an `ordered_phase_queue`, exact `phase.step.status` substep rows, status
vocabulary `done | in-progress | remaining | review-block | blocked`, no phase
jumping, counts matching row statuses, completed/current/remaining phase lists,
remaining action count and names, human-language explanation of what each phase
solves, and a human-readable `requirement -> phase -> evidence` matrix. Other
ledger words such as `complete` or `partial` cannot replace this
execution-control vocabulary. Luba-facing updates must also include the
binding human-readable status template in the operator's language (plain language; Russian for the original author) — a "phase X of Y: <human name>",
"meaning of phase", "step X.Y of X.N: <human step name>", "meaning of step" block (example in the original author's Russian: `Фаза X из Y: <human name>`, `Смысл фазы`, `Шаг X.Y из X.N: <human step name>`, `Смысл шага`),
with rule-file exact verification literals `Фаза N из M: <человеческое название>. Смысл фазы: <что эта фаза решает для проекта/пользователя>.`
and `Шаг N.K из N.T: <человеческое название>. Смысл шага: <что этот шаг доказывает/чинит>.` shown as an example for a Russian-speaking operator;
actual reports must replace N, M, N.K, and N.T with concrete phase/step numbers,
a status line `Статус: DONE | IN_PROGRESS | NEEDS_FIX | REVIEWING | BLOCKED_WITH_REASON`
with a plain-language explanation in the operator's language, total project position, done (example `Сделано`),
in-progress (`В процессе`), numbered remaining (`Осталось`), review-found (`Review нашёл`), what-was-fixed (`Что исправлено`),
next-action (`Следующее действие`), and commit/continuation permission. The bare form
`Фаза N/14, шаг NN.MM/NN.Total, статус X` is invalid for Luba without that
explanation. Technical labels such as `generic_bad_output`,
`writer-level path`, `QSG-28`, `07.06/07.07 NEEDS_FIX`, `packet accepted`,
`boundary pass`, JSON paths, STATE flags, or validator names must be translated
in the same update with human meaning and done/in-progress/remaining counts.
Model/Reasoning Configuration Gate: every sub-agent work order must explicitly
set `requested_model=gpt-5.5` and `requested_reasoning_effort=xhigh` unless
Luba explicitly authorizes a downgrade in the current unquoted instruction. If
visible runtime/request metadata or explicit work-order metadata differs from
those requested values, including medium/high/low/default reasoning instead of
xhigh, emit a user-visible warning, record the downgrade or unavailable xhigh
model in state/log, classify it as a governance violation, and stop before
execution continues. Generic system/developer identity text such as "GPT-5" is
not downgrade evidence by itself. If exact runtime metadata is unavailable but
the work order explicitly requests `gpt-5.5` / `xhigh`, continue and record
`mode_verified_from_work_order=true` unless a concrete contradictory
runtime/request value is visible. A false downgrade stop based only on generic
identity text is a premature stop loophole.
```

Active-session load note: reading this override loads
`Parent Architect Subagent-Only Firewall` for the current session.

If a skill says the architect should "run tests", "execute", "continue as
executor", or "start training", translate that into a bounded sub-agent work
order unless Luba has explicitly made the current session the executor. If no
sub-agent capability is available and execution is required, record a real
blocker instead of using architect to do executor work.

If a skill or generic multi-agent guide says the parent may do local
non-overlapping work while a sub-agent runs, that advice is disabled under a
sub-agent-only instruction. Parent work while waiting is limited to waiting or
polling for the sub-agent result, orchestration, closing/retrying failed agents,
and minimal status checks that do not inspect governed file contents. Read-only
verification and review happen after the sub-agent packet returns. The parent
must not read AGENTS.md, CLAUDE.md, skill indexes, plans, code, reports, or
state for independent loophole research or repair drafting while the assigned
sub-agent is still running. A required sub-agent wait/review is not a
final-response boundary; the parent must wait/review/orchestrate instead of
reporting a pause unless OP-13 proves a real blocker.

Every sub-agent-only review must include an authorship gate: changed files must
come from a bounded sub-agent id and changed-file list. Parent-authored direct
edits after the sub-agent-only instruction fail the review and must be repaired
by a bounded sub-agent.

Every firewall-repair review must also include an exact-file textual gate:
the exact AGENTS/CLAUDE/HOW-TO-USE-SKILLS/skill files named by the work order
must contain `Parent Architect Subagent-Only Firewall`,
`parent local sidecar work ban`, read-only verification only after a sub-agent
packet returns, no local non-overlapping work escape, and
`sub-agent waiting is not a stop`. A broad `rg docs/skills` result is context
only and cannot prove completion.

No skill may choose a lower-tier sub-agent, silent fallback, inherited default,
unspecified model, medium/default reasoning, provider-equivalent substitution,
or weaker reasoning effort for planning, plan review, execution, code review,
execution review, governance repair, or completion review after a
`gpt-5.5 Extra High` highest-order order. Skills must use
`requested_model=gpt-5.5` and `requested_reasoning_effort=xhigh` in generated
sub-agent work orders. If exact runtime metadata is unavailable, the skill must
continue from the explicit work-order request and record
`mode_verified_from_work_order=true` unless a concrete contradictory
runtime/request value is visible. Warning/stop applies only to concrete
runtime/request/work-order mismatch or explicit medium/high/low/default
reasoning, not to generic "GPT-5" system/developer identity text.

Architect must check live code/docs/evidence before answering technical claims.
If sub-agent packets conflict or evidence is ambiguous, architect explains the
conflict to Luba in human language and does not silently choose.

Sub-agent git authority is part of the skill load, not an optional process
detail. Every bounded work order must name `git_authority` as `forbidden`,
`parent_only`, or `allowed_with_exact_quote`, plus exact allowed and forbidden
git commands. Artifact/code/content/data/report/plan/governance work orders
default to `git_authority=forbidden`; parent review or a separate commit-only
work order owns commit/push. A sub-agent that stages, commits, pushes, changes
refs, or emits git directives while git is forbidden has not cleanly completed
the bounded pass. Parent may validate useful content, but must record the
process violation and repair governance before relying on that delegation
pattern again.

TranslationDomain checkpoint writer guard:

```text
Before any training work order, architect assigns exactly one checkpoint path
and one writer sub-agent. The work order authorizes one code slice or one
training chunk only. No second writer may use that checkpoint, and no sub-agent
may chain extra source rollovers, runner starts, checkpoint writes, or
continuation loops unless exact limits are named. If MeetsPilotGate=false or
unknown, the active checkpoint must not be written; the sub-agent returns
evidence and code enforcement becomes the next repair task. Long-running
training sends a heartbeat every 10-15 minutes or after each 29-cycle chunk.
Contaminated checkpoints require rollback to clean checkpoint or Luba-approved
forensic replay before any continuation writer is issued.
```

Binding constitution:
`43.LearningLocalWikiData/CONSTITUTION-EXECUTOR-ARCHITECT-ROUND-CONTROL-v1.md`.
The constitution names this failure as dangerous to project survival and as a
catastrophic-risk condition for the agent's own project authority. Skills must
not soften, reinterpret, or bypass it.

Every multi-step plan must also include a plan-governance orchestration block.
Skills are part of that block, not a separate optional checklist. If the active
plan does not say which skills govern plan creation, plan review, execution,
code review, documentation sync, completion verification, role transitions,
state updates, score penalties, and anti-loophole repair, the executor must add
that missing governance before executing the plan.

Detailed architect reference protocol:
`43.LearningLocalWikiData/PROTOCOL-ARCHITECT-PLAN-SKILL-AGENT-ROLE-REVIEW-v1.md`.
Load it when validating a plan, reviewing a stop/completion claim, comparing
skill behavior to AGENTS/CLAUDE, or repairing role-transition loopholes.

Required skill orchestration by lifecycle stage:

```text
plan creation:
  authority-requirement-binding
  reference-inventory-and-source-map
  planning-session-startup
  codex-planning-quality-GUIDE.md when planning-quality, benchmark-depth, high-risk, multi-root, or focus-retention gates are triggered
  reconnaissance-gate
  human-readable-architecture-audit
  intent-to-plan
  plan-self-validation
  goal-delivery-completeness
  specificity-calibration
  claim-verification

plan review:
  authority-requirement-binding
  reference-inventory-and-source-map
  codex-planning-quality-GUIDE.md when planning-quality, benchmark-depth, high-risk, multi-root, or focus-retention gates are triggered
  human-readable-architecture-audit
  plan-review
  goal-delivery-completeness
  audit-protocol
  claim-verification
  specificity-calibration
  reconnaissance-gate
  output-skepticism

execution:
  code-execution
  code-examination
  change-propagation
  verification-before-completion

code review:
  human-readable-architecture-audit
  generated-code-review
  self-verification
  test-integrity
  dna-compliance-guard

documentation sync:
  human-readable-architecture-audit
  documentation-sync
  document-hierarchy
  skill-graph-sync

completion verification:
  goal-delivery-completeness
  coverage-completeness-gate
  mvp-round-trip-verification
  verification-before-completion

loophole repair:
  authority-requirement-binding
  architect-behavior-classifier
  plan-review
  claim-verification
  change-propagation
```

Plan review is a first-class lifecycle stage, not a docs-sync add-on. Route it
through the SK-REVIEW bundle from `skill-injection-SKILL.md`, including
specificity, reconnaissance, claim verification, direct source read-back,
`reference-inventory-and-source-map-SKILL.md`, source-disjoint evidence for
high-risk/multi-root claims, and a final readiness sweep.
SK-PLAN and SK-REVIEW must keep exact active-artifact discipline: benchmark
plans, quoted transcripts, old plans, and sub-agent packets stay evidence-only
unless Luba reactivates them in the current unquoted instruction. Source/license
status, universal-vs-project-specific reuse boundaries, supplement disposition,
explicit `claude-*`/`codex-*`/`dcm-*` prefix quarantine, named
artifact/library/folder population proof with `evidence_depth_score`, final
Open Decisions Queue, and Whole-Plan Contradiction Sweep must be recorded before
a detail is copied into a plan, skill, guide, dataset, or readiness claim.

If a named skill contains older STOP, wait, branch, or broad-test wording, apply
the project-local override above and continue from the active plan/state.

---

## Session type → skills

| Session type | Load before first output |
|---|---|
| EXECUTOR | dev-safety-SKILL.md + code-execution-SKILL.md |
| ARCHITECT | plan-review-SKILL.md + no-product-decisions-SKILL.md |
| EXPERIMENT | experiment-design-SKILL.md + code-execution-SKILL.md |
| REVIEW | plan-review-SKILL.md |
| TRANSLATION | **translation-domain-mindset-reset-SKILL.md** (FIRST — behavioral anchor) + **translation-domain-reality-check-SKILL.md** (SECOND — technical FAQ) + dev-safety-SKILL.md + code-execution-SKILL.md |

---

## Activation triggers — load when condition is true

| When | Load |
|---|---|
| Any learned/trainable/semantic/adaptive/DPO claim, or any scorer/selector/classifier/router/KNN/dictionary/pattern-matcher mechanism | trainable-architecture-decision-SKILL.md |
| Any high-risk architecture, ILearningMachine/DynamicGraph, multi-root, or cross-source-corpus plan | reconnaissance-gate-SKILL.md + reference-inventory-and-source-map-SKILL.md + codex-planning-quality-GUIDE.md + planning-session-startup-SKILL.md + plan-review-SKILL.md; require the High-Risk Architecture Recon Library Gate, Knowledge Library artifact, source-disjoint evidence, external/source corpus such as `llm mvp` >=20 passes and `llm_mvp_core` docs+code >=40 passes unless current Luba profile is stricter. A generic source map cannot substitute for this gate. |
| Any graph-style or DynamicGraph trainable architecture plan/review/audit | trainable-architecture-decision-SKILL.md + human-readable-architecture-audit-SKILL.md + codex-planning-quality-GUIDE.md + plan-review-SKILL.md; require roles/hierarchy, nodes/edges, input/output contracts, many-to-many outputs, fan-out/fan-in, multi-output edge contracts, async graph execution, deterministic-vs-trainable authority per edge, and hidden deterministic shortcut checks tied to graph edges. |
| Any DPO/distillation teacher output used for training or ranking | trainable-architecture-decision-SKILL.md + planning-session-startup-SKILL.md + plan-review-SKILL.md; require provenance fields `teacher_access_mode`, `hosting_provider`, `model_id`, `model_license`, `provider_terms_checked`, `teacher_output_used_for_training`, `closed_proprietary_model`, and `request_id/model_version` when available. Only allowed hosted open-weight/permissive models after terms/license check may be distillation teachers; Claude/OpenAI/Gemini/Gemini Grounding are prohibited as distillation teachers. `closed_proprietary_model=true` plus `teacher_output_used_for_training=true` is `DATASET_REBUILD_REQUIRED` and plan-review hard fail. |
| Any architecture/trainable/code audit, plan review, code review, documentation, or report that must be readable by Luba as a human architect | human-readable-architecture-audit-SKILL.md + output-readability-gate-SKILL.md |
| Any planning or plan review for frontend, UI, screen, component, layout, visual design, interaction design, or design-regression work | interface-design-craft-SKILL.md + ui-quality-standard-SKILL.md + ui-screen-grammar-SKILL.md + ui-examination-round-SKILL.md + test-integrity-SKILL.md; require UI/UX skill checks plus screenshot/render/design-regression automation evidence when applicable. |
| Luba explicitly asks for iterative audit readability improvement | human-readable-architecture-audit-SKILL.md + plan-review-SKILL.md + audit-protocol-SKILL.md; require minimum 6 cycles, preferably 10, each recording skill/gate applied, failure, fix, and readability improvement |
| Luba asks why planning or plan review is shallow, compares Codex with another model, provides a quoted benchmark, asks for planning-quality/review-depth repair, or challenges focus drift | conversation-intent-and-self-reflection-SKILL.md + codex-planning-quality-GUIDE.md + reference-inventory-and-source-map-SKILL.md + planning-session-startup-SKILL.md + plan-review-SKILL.md + skill-injection-SKILL.md; load SK-REVIEW as the active review bundle and keep benchmark/quoted material evidence-only. |
| Luba or a work order requests N review/fix passes, "at least N" cycles, or "10 review/fix passes completed" | plan-review-SKILL.md + claim-verification-SKILL.md + phase-completion-packager-SKILL.md; add human-readable-architecture-audit-SKILL.md when the artifact is architecture/trainable/code audit. Require N real evidence packets with artifact before/after refs, reviewer/sub-agent id, loaded skills/gates, findings, fix or no-fix justification, and recheck. Ledger rows are only an index. |
| Luba asks for "10 review of execution plan", "10 plan review cycles", or review before continuing execution | plan-review-SKILL.md + multi-reviewer-design-SKILL.md + claim-verification-SKILL.md + phase-completion-packager-SKILL.md + output-readability-gate-SKILL.md; run the 10 Review Cycle Gate and Ten-Review Evidence Gate through real independent sub-agent packets before runtime/code/training/host execution continues. Show visible review records 1..10 with reviewer role/id, requested model/effort, verdict, findings, repair action, affected files/sections, and whether plan/skills changed. |
| Luba asks to continue after governance repair or says the plan/review result must be visible in chat | plan-review-SKILL.md + output-readability-gate-SKILL.md + session-execution-log-SKILL.md + phase-completion-packager-SKILL.md; run the Execution Plan Visibility Gate, Human-Readable Execution Board Gate, and Plan Presentation Gate; present the plan/current stage/10-cycle outcome in the operator's language (human-readable) before execution continues. |
| Luba says "I do not see the plan", "I do not see 10 review", "not clear which stage" (or, in Russian, "не вижу план", "не вижу 10 review"), or equivalent visibility challenge | output-readability-gate-SKILL.md + plan-review-SKILL.md + phase-completion-packager-SKILL.md + session-execution-log-SKILL.md; treat it as visibility/protocol failure to repair before execution resumes, not as a stop gate. Present the plan in chat with done count, current step, remaining count, accepted evidence, open gaps, next action, execution pause state, and visible review records when required. |
| Luba asks for plan creation, plan review, execution status, or complains about `разброс`, `прыжки по фазам`, `где мы`, `сколько осталось`, `done/in-progress/remaining`, `phase/step/status`, or equivalents | planning-session-startup-SKILL.md + phase-planning-SKILL.md + plan-review-SKILL.md + codex-planning-quality-GUIDE.md + phase-completion-packager-SKILL.md; run the Sequential Execution-Control Gate with `current_execution_pointer`, `ordered_phase_queue`, exact `phase.step.status` rows, allowed statuses only, no phase jumping, matching counts, completed/current/remaining lists, remaining action names, phase explanations, and `requirement -> phase -> evidence` matrix. For chat/status to Luba, require the binding status template in the operator's language (example tokens below shown in the original author's Russian) with `Фаза X из Y` / `Фаза N из M`, `Смысл фазы`, `Шаг X.Y из X.N` / `Шаг N.K из N.T`, `Смысл шага`, `Статус: DONE | IN_PROGRESS | NEEDS_FIX | REVIEWING | BLOCKED_WITH_REASON` with a plain-language explanation in the operator's language, total project position, `Сделано`, `В процессе`, numbered `Осталось`, `Review нашёл`, `Что исправлено`, `Следующее действие`, and counts. Rule files must keep the exact literals `Фаза N из M: <человеческое название>. Смысл фазы: <что эта фаза решает для проекта/пользователя>.` and `Шаг N.K из N.T: <человеческое название>. Смысл шага: <что этот шаг доказывает/чинит>.`, while actual reports replace N/M/N.K/N.T with concrete numbers; reject bare `Фаза N/14, шаг NN.MM, статус X`, `QSG-28`, `07.06/07.07 NEEDS_FIX`, `packet accepted`, `boundary pass`, and untranslated labels such as `generic_bad_output` or `writer-level path`. |
| Claude/Cursor/Codex extension plan, runtime execution, or completion claim | plugin-runtime-reality-gate-SKILL.md + client-adaptive-learning-loop-SKILL.md + plan-review-SKILL.md + generated-code-review-SKILL.md + test-integrity-SKILL.md; enforce the Full Extension Definition Gate and Full Extensions Readiness Review Gate for Claude, Cursor, and Codex separately: entrypoint, settings/debug, target-locator/core API, prompt-to-task, task-to-orchestration, task/plan-to-skills, adaptive learning, export/import/backup/share/continue-training, hidden deterministic shortcuts, DPO quality, tests/QA, docs, and live host evidence. |
| Sub-agents claim the plan is fully executed, or a final ready/done completion is about to be reported | phase-completion-packager-SKILL.md + verification-before-completion-SKILL.md + plan-review-SKILL.md + generated-code-review-SKILL.md + output-readability-gate-SKILL.md; run the Final Execution Review Gate with at least 10 independent sub-agent execution reviews. |
| Any code slice review for trainable/adaptive/DPO/semantic/routing/scoring/lookup mechanisms | generated-code-review-SKILL.md + trainable-architecture-decision-SKILL.md + test-integrity-SKILL.md + implementation-integrity-SKILL.md + code-examination-SKILL.md; run the Deterministic-Boundary Code Review Gate and Execution/Code Review Completeness Gate: exact review scope, project standards/tests, `llm_mvp_core` core boundary, thin host adapters, hidden deterministic shortcut sweep, deterministic helper labels, and docs/QA/dashboard impact. |
| Luba says "through sub-agents", "parent is not executor", architect-only, governance-only, or sub-agent-only | planning-session-startup-SKILL.md + skill-injection-SKILL.md + plan-review-SKILL.md + test-integrity-SKILL.md + phase-completion-packager-SKILL.md + architect-behavior-classifier-SKILL.md. Load the Parent Architect Subagent-Only Firewall, forbid parent local sidecar work, require exact current Luba quote, allowed write scope, required skills/guides, allowed verification, quoted-transcript authority quarantine, sub-agent id, changed files, and parent-owned commit/push boundary. Parent may orchestrate, wait, review returned packets, request bounded fixes, and commit/push accepted sub-agent output only when assigned; parent must not direct-edit or perform executor work while architect-only/sub-agent-only is active. |
| Product-readiness evidence is downgraded, blocked, not proven, not ready, needs verification, no-claim, or status-only | plugin-runtime-reality-gate-SKILL.md + verification-before-completion-SKILL.md + claim-verification-SKILL.md + agent-output-format-SKILL.md + plan-review-SKILL.md. Treat the status as not completion; create the next concrete scenario work item unless all product requirements are already proven or Luba explicitly stops. Report total scenarios, fully joined proven, partial, not moved, blocked with next action, exact scenario moved, and separate evidence accounting done from product requirement proven. |
| A later broader audit in the same architecture/trainable/code area finds BLOCKING/HIGH issues, or an audit claims "sufficient/complete/ready/closed" | human-readable-architecture-audit-SKILL.md + audit-protocol-SKILL.md + plan-review-SKILL.md + trainable-architecture-decision-SKILL.md; run Audit Sufficiency Boundary Gate, Delta-Severity Feedback Gate, breadth challenge, and record why the previous review missed it |
| Luba asks whether code hides deterministic logic, or asks how to accept an architecture decision | trainable-architecture-decision-SKILL.md before repair |
| Any learned dictionary/counter/map such as `Dictionary<string, Dictionary<string,int>>`, `_candidatePositionWeights`, `_promptUnderstandingSignals`, `_taskUnderstandingSignals`, `_promptContextSignals`, or `_intermediateChecksSignals` | trainable-architecture-decision-SKILL.md + dpo-triple-format-SKILL.md + code-examination-SKILL.md; require one concrete quote-adjacent `ILogicalUnit` replacement blueprint per field, typed state, exact class/record/method/test names, KNN/nearest method signatures and role, filled positive/rejected/quarantine DPO examples, source license gate, load/cache/repo fallback, backup/export/import/share, continue-training from checkpoint, and affected set rebuild rules. Schema-only rows are rejected. |
| Creating or reviewing DPO rows for trainable unit replacement | dpo-triple-format-SKILL.md + trainable-architecture-decision-SKILL.md; reject rows that train dictionary/candidate-order/source-id shortcuts or use unlicensed/private material for common training; require filled examples that show exact state mutation, KNN/nearest memory update, checkpoint save, and quarantine reason, not only `row schema` or `chosen/rejected`. |

| When | Load |
|---|---|
| Any test fails unexpectedly | mental-debug-SKILL.md |
| Test passes but output value is wrong | mental-debug-SKILL.md (Rule 6: error-silent-swallow) |
| Stub was modified (not just created) to pass a test | test-integrity-SKILL.md (FM-1) |
| Test count went up but coverage feels thin | test-integrity-SKILL.md (FM-3) |
| Plan has no Gate B | plan-review-SKILL.md |
| New, clarified, or strengthened requirement from active task authority | authority-requirement-binding-SKILL.md + plan-review-SKILL.md |
| Architect work order to a downstream sub-agent | authority-requirement-binding-SKILL.md + session-output-contract-SKILL.md |
| Accepted reviewer gate becomes a repair requirement | authority-requirement-binding-SKILL.md + plan-review-SKILL.md |
| Session begins before writing any plan | authority-requirement-binding-SKILL.md + planning-session-startup-SKILL.md (Authority Requirements Ledger supersedes any user-only ledger wording) |
| Choosing between two design approaches | no-product-decisions-SKILL.md |
| New experiment E-N designed | experiment-design-SKILL.md |
| Commit about to happen | dev-safety-SKILL.md |
| New .cs service file created | dev-safety-SKILL.md (Rule 1 + Rule 2 check) + dna-compliance-guard-SKILL.md |
| Subproject phase completing | code-execution-SKILL.md (Gate B check) |
| SCALE_UP result in experiment | experiment-design-SKILL.md (Tier escalation) |
| D-09 max_weight used anywhere | experiment-design-SKILL.md (formula reminder) |
| ProbeNeuralNetwork used | experiment-design-SKILL.md (vocabSize=22 reminder) |
| E20 pipeline order referenced | experiment-design-SKILL.md (StackDetector first) |
| New subproject 12-17 started | code-execution-SKILL.md + plan-review-SKILL.md |
| ISemanticCompiler injected into new class | dev-safety-SKILL.md (Fabric First check) |
| Bug fix declared complete | three-level-verification-SKILL.md (run all 3 levels) |
| Guard added to one service | test-integrity-SKILL.md (FM-4 sibling parity check) |
| OperationResult<T> namespace mismatch suspected | mental-debug-SKILL.md (Rule 4: boundary-data-lifecycle) |
| New service does not inherit BaseService | dna-compliance-guard-SKILL.md (BaseService rule) |
| Writing test that exercises BM25, HNSW, or HybridRetriever | integration-testing-SKILL.md (fabric coverage matrix) |
| New demo stage added to a .Desktop MainForm.cs | ui-testing-SKILL.md (manual checklist + layout standard) |
| SemanticCompilerPipeline or TemplateBeautifier output changed | dna-compliance-guard-SKILL.md (§Generated output audit) + generated-code-review-SKILL.md |
| 17.DynamicDomains phase started | generated-code-review-SKILL.md + integration-testing-SKILL.md |
| About to claim "done" or "passes" | verification-before-completion-SKILL.md (iron law) |
| Before writing ANY code this session | phase-preflight-SKILL.md (assumption registry) |
| Any TranslationDomain work starts | translation-domain-reality-check-SKILL.md (FAQ-1 to FAQ-11) |
| Translation accuracy reported ≥ 0.95 | translation-domain-reality-check-SKILL.md FAQ-3 — check contamination |
| Translation output starts with [DIRECTION:BOOTSTRAP] | translation-domain-reality-check-SKILL.md FAQ-2 — not broken, needs training |
| Reviewing another model's TranslationDomain work | translation-domain-reality-check-SKILL.md FAQ-8 — trace mechanism |
| Adding if/else or dictionary to improve translation | translation-domain-reality-check-SKILL.md FAQ-9 — STOP, forbidden |
| Before any new phase | phase-planning-SKILL.md (9 gates, starting with Requirements Ledger Gate -1) + infrastructure-discovery-SKILL.md (Gate 0) |
| Bug reported or symptom observed | bug-to-tests-SKILL.md (3 failing tests before fix) |
| Error deep in call stack, unclear origin | root-cause-tracing-SKILL.md (trace backward) |
| Generated code looks wrong | retroactive-development-SKILL.md (fix engine not output) |
| Session end — before committing | documentation-sync-SKILL.md + verification-before-completion-SKILL.md |
| Project-local code documentation/wiki flow changes | documentation-sync-SKILL.md + graph-backed-routing-SKILL.md + data-flow-integrity-SKILL.md + negative-example-patching-SKILL.md |
| Context feels low / messages being compressed | context-overflow-SKILL.md (wrap-up or emergency stop) |
| Measuring phase completeness | capability-measurement-SKILL.md (6 classes, scored with evidence) |
| Verifying own generated or modified code | self-verification-SKILL.md (6 rules) |
| Writing session plan or execution file | agent-output-format-SKILL.md (3-file rule + SESSION-0 gate) |
| Session observability / tracking cards | tracker-SKILL.md (IProjectTrackerService, local-file default) |
| Choosing which skills to inject | skill-injection-SKILL.md (max 3, priority DNA>FAB>PLAN/REVIEW>TEST>DOCS; SK-REVIEW is first-class for plan review/user correction) |
| IAgentMessageBus event chain broken | event-driven-debugging-SKILL.md (binary search + 4 break classes) |
| Deciding if a value should be hardcoded or config | freedom-machine-SKILL.md (MACHINE vs FREEDOM classification) |
| About to claim "improved" or report a result | output-skepticism-SKILL.md (3 skeptic questions before reporting) |
| Test failed — unclear which layer is wrong | test-failure-triage-SKILL.md (Class 1-4 classification) |
| End of investigation/analysis session | session-state-crystallization-SKILL.md (≤40-line snapshot) |
| Phase complete — writing git report | phase-git-report-SKILL.md (commit convention + trace link) |
| Handing off investigation findings | investigation-handoff-SKILL.md (discovery/rejection format) |
| Assessing impact before touching a file | blast-radius-SKILL.md (CRITICAL/HIGH/MEDIUM/LOW) |
| Tagging deliverables before writing | blast-radius-tagger-SKILL.md (TEST/GOVERNANCE/PRODUCTION/INTERFACE) |
| Designing which classes belong together | service-boundary-design-SKILL.md (4 boundary tests) |
| Designing message types for IAgentMessageBus | domain-event-design-SKILL.md (naming convention + payload rules) |
| Verifying chain integrity before integration | data-flow-integrity-SKILL.md (producer/consumer schema diff) |
| Writing acceptance tests for a feature | user-journey-testing-SKILL.md (scenarios before implementation) |
| Phase end — measuring plan accuracy | plan-execution-feedback-SKILL.md (discovery rate tracking) |
| Decomposing an ambiguous or compound problem | problem-decomposition-SKILL.md (symptom→root cause, minimum files, executable output) |
| Plan or response depends on a claim about existing code | claim-verification-SKILL.md (VERIFIED/INFERRED/ASSUMED classification) |
| About to write a test that calls a class or interface | api-shape-verification-SKILL.md (verify symbol, signature, return type) |
| New interface or class just added | implementation-integrity-SKILL.md (4 integrity checks + JSON report) |
| Gap discovered — need a new component or interface | gap-to-proposal-SKILL.md (resolution ladder Level 1–4, Luba stop gate) |
| Starting a new phase or session | prerequisite-chain-SKILL.md (prereq list → verification → block protocol) |
| Designing a component with if/else, thresholds, or lookup tables | four-tier-decision-classification-SKILL.md (BOUNDARY/FREEDOM/LEARNABLE/AI-PIPELINE) |
| Test count or metric spans multiple sessions | chain-arithmetic-audit-SKILL.md (verify baseline + Σ deltas = final) |
| Naming a new file, class, interface, or test | naming-conventions-enforcer-SKILL.md (capability names, not sequence numbers) |
| Phase gate just passed — ready to package | phase-completion-packager-SKILL.md (3-artifact procedure + STOP gate) |
| Writing phase completion report | mission-progress-SKILL.md (5 questions: trained/improved/architecture/blocked/decision) |
| Before writing session or EXECUTION files | assumption-registry-SKILL.md (INFRA/BEHAVIORAL/OPERATIONAL assumption table) |
| About to claim completion, improvement, or progress | coverage-completeness-gate-SKILL.md (evidence scope must match claim scope) |
| Any task that changes skills, interfaces, decisions, or docs | change-propagation-SKILL.md (blast radius → definition of done) |
| Assigning a new decision number (D-NN) or BUG number | artifact-numbering-SKILL.md (read live docs, not plan files) |
| Bug reached a deep call site — fixing at one layer isn't enough | defense-in-depth-SKILL.md (4-layer validation: entry/business/env/debug) |
| Multiple fix attempts haven't resolved the issue | systematic-debugging-SKILL.md (4-phase: root cause → pattern → hypothesis → implement) |
| Locked decision being challenged or needs to change | decision-reopening-SKILL.md (4-step test + ADR entry before changing anything) |
| Writing a REFERENCE-PLAN or first-pass phase descriptions | intent-to-plan-SKILL.md (plain language, single responsibility, Gate B per phase) |
| Claiming capability or test count improved | improvement-measurement-SKILL.md (behavioral delta required, not just test count) |
| Component just graduated from STUB to ACTIVE | training-data-gap-audit-SKILL.md (classify gap-window examples before training) |
| Phase just completed — writing the log artifact | session-execution-log-SKILL.md (exact schema: testBaseline/files/gateResults) |
| Session start — anchoring to Luba's stated goal | goal-context-persistence-SKILL.md (verbatim capture + goal reminder at every STOP) |
| Multi-subproject session — establishing scope before work | work-scope-inventory-SKILL.md (denominator before numerator; claim authority table) |
| Session start — deciding architect vs executor vs reviewer mode | session-mode-declaration-SKILL.md (5 modes, drift detection, restart on drift) |
| Phase complete — producing handoff for next planning conversation | session-handoff-SKILL.md (SESSION-BRIEF-Pn.md format; self-contained, prose not file list) |
| Proposing a new component, interface, or convention to fill a gap | solution-scope-gate-SKILL.md (scope ladder Level 1–5, gap classification, 3 reframe tests) |
| Multi-phase plan authored — before Phase 1 executes | plan-self-validation-SKILL.md (Phase 0 gate: 5 checks — requirements ledger, goal delivery, phantom skills, presence matrix, order) |
| Claiming a new capability is fully delivered end-to-end | mvp-round-trip-verification-SKILL.md (8-step round-trip: authored→visible→registered→callable→observable) |
| Starting implementation of a new subproject or capability | subproject-examination-SKILL.md (3-doc output: master plan + missing infrastructure + learning spec) |
| Pipeline run scored below threshold — diagnosing which stage failed | pipeline-trace-reader-SKILL.md (stage-by-stage trace reading; first failing stage = fix target) |
| Capability scored below threshold — iterating toward promotion | quality-improvement-cycle-SKILL.md (score band table; 9-step cycle; max 3 cycles; escalation protocol) |
| Designing expected cycle counts and failure modes before execution | capability-adaptation-map-SKILL.md (per-class cycle budget, expected failures, escalation triggers) |
| Designing a capability that computes scores, weights, or rankings | algorithm-as-service-SKILL.md (5 classes: weighted scoring/decay/recommendation/matching/gamification) |
| ARCHITECT or REVIEWER session — scan output for failure patterns | architect-behavior-classifier-SKILL.md (7 positive habits, FAIL catalog including human-readable architecture audit failures; 3-step doc-first loop) |
| Designing capability lifecycle in ContinuousLearningPipeline | confidence-lifecycle-SKILL.md (5 phases: seed→learn→promote→decay→graduate; risk-tier thresholds) |
| Deciding whether a new capability needs AI pipeline or plain C# | capability-routing-gate-SKILL.md (4-question test: improves-with-data / configurable / traceable / bootstrap) |
| User asserts "X is done / implemented / working" — before planning | claim-as-hypothesis-SKILL.md (capture claim → verify with grep/Read → VERIFIED/DISCONFIRMED/PARTIAL) |
| Checking that a .cs file or interface is actually implemented, not a stub | design-artifact-completeness-SKILL.md (5 checks: exists/populated/specific/matches-spec/reachable) |
| Designing the complete test suite for a new subproject or capability | e2e-test-matrix-SKILL.md (8 scenario categories: happy/idempotency/rate-limit/compensation/time-based/recovery/boundary/security) |
| Architecture decision or interface design needs multi-stakeholder review | multi-reviewer-design-SKILL.md (5 reviewer roles; BLOCK overrides; 3-cycle max; escalation protocol) |
| Multiple capabilities ready to build — choosing build order | feature-prioritization-SKILL.md (3-axis score: value × dependency × cost⁻¹; priority score card) |
| Checking if a subproject plan is ready to implement (all goals covered, artifacts populated) | subproject-completeness-checker-SKILL.md (pre-gate: goal-to-phase mapping + artifact populated; T1–T10 checks) |
| Designing a new capability that will enter ContinuousLearningPipeline | capability-improvement-cycle-SKILL.md (5-stage cycle: gap→design→score→observe→promote) |
| Sequential batch of phases completed — before starting next parallel batch | phase-retrospective-SKILL.md (4 dimensions: cycle accuracy / pattern transfer / test coverage / gate adherence) |
| Verifying that a plan covers all of Luba's stated goals | goal-delivery-completeness-SKILL.md (first arbiter: goal→phase mapping, APPROVED/BLOCK/CHALLENGE per goal element) |
| Writing a new test class — before writing tests | iron-rule-derivation-SKILL.md (derive domain invariants from failure modes; MACHINE vs FREEDOM) |
| Capability runs but confidence never improves | learning-loop-closure-SKILL.md (4-event loop: decide→observe→update→promote; broken event diagnosis) |
| Gate B failed — diagnosing which layer is wrong before fixing | diagnostic-escalation-SKILL.md (4-step diagnosis: test data / wiring / stub / algorithm; escalation triggers) |
| Session begins — before writing any plan | authority-requirement-binding-SKILL.md + planning-session-startup-SKILL.md (Authority Requirements Ledger + 5 questions: authority requirements / mode+scope / baseline / decisions / documents / blockers) |
| Proposal is challenged as "wrong abstraction level" | level-correction-response-SKILL.md (abandon frame; 3 proactive tests: next-instance / provider-vs-capability / old-model-survival) |
| Reading current project capability state before planning | capability-state-reader-SKILL.md (interface registry + test baseline + DomainModelRegistry snapshot; blocker table) |
| Plan review — all 10 points must be audited | audit-protocol-SKILL.md (10-point checklist; BLOCKING/WRITE-TIME/INFORMATIONAL severity; NOT CHECKED declaration) |
| Designing learnable relationships in DomainModelRegistry | graph-entity-schema-SKILL.md (NODE/EDGE/PROPERTY/IMMUTABLE classification; 5-field edge schema; promotion paths) |
| Predicting cycle count for a new capability before planning | cycle-budget-prediction-SKILL.md (formula: baseline + first-occurrence + novelty − clarity + inversion; budget card) |
| Declaring expected failure modes per capability class before implementing | archetype-failure-map-SKILL.md (score bracket actions; per-class failure modes; escalation triggers; adaptation map table) |
| Writing a context package for a pipeline call or AI decision | context-package-design-SKILL.md (5-field structure: INTENT/DOMAIN/CONSTRAINTS/PRIOR_ART/SUCCESS; 8-check verification) |
| Designing a multi-implementation AI decision pipeline | ai-pipeline-design-SKILL.md (4 roles: implementors/arbiters/context/manager; DPO triple; graduation thresholds) |
| Defining an evaluation panel for capability review or convergence | arbiter-panel-design-SKILL.md (8 roles; Role 8 Goal Delivery runs first; minimum panel by archetype) |
| Significant design decision made — recording it for future sessions | decision-capture-SKILL.md (DESIGN_REASONING format; 6 decision types; question field = retrieval key) |
| Classifying a capability as manual C# vs pipeline topology | capability-implementation-gate-SKILL.md (exhaustive bootstrap list; 4-question test; capability routing declaration) |
| Seeding new DomainModelRegistry edges with the right initial confidence | confidence-seeding-SKILL.md (evidence-based formula; update deltas; decay protection window; risk-tier graduation) |
| Designing challenger roles for a multi-model convergence session | challenger-round-design-SKILL.md (per-archetype minimum panel; challenger prompt templates; CONSENSUS/DEFERRED/STALLED) |
| Identifying services that appear in ≥ 3 subprojects — shared infrastructure vs per-subproject | shared-service-design-SKILL.md (appearance count; 3 sharing patterns A/B/C; shared services map; build sequence) |
| Deciding whether a capability should be decomposed into sub-capabilities | capability-decomposition-SKILL.md (5 complexity signals; termination bound depth 3-5; non-overlap verification) |
| Routing questions to the authoritative document and preventing competing documents | document-hierarchy-SKILL.md (5-tier hierarchy; routing table; conflict resolution; 3 pre-document checks) |
| Aggregating multi-arbiter verdicts into an execution decision | escalation-orchestrator-SKILL.md (6 rules: BLOCK/ACCEPT/winner-by-score/upper-judge/human/UNDECIDED) |
| Designing IAgentMessageBus event schemas with compensation and SLA | event-contract-design-SKILL.md (3 correlation fields; PII rule; compensation events; SLA annotation; stack-neutral check) |
| Defining what "done" means before any session work begins | session-output-contract-SKILL.md (exact file list; verification commands; spec gate; does-not-produce section) |
| Running subproject phases in parallel without artifact collisions | parallel-execution-coordinator-SKILL.md (3 questions; artifact pre-allocation; delta gate; CAS lifecycle write) |
| Verifying that a phase knows what downstream components need from its output | pipeline-position-check-SKILL.md (Q0a-Q0d: receives/produces/consumer/consumer-requirement; CONTEXT_INSUFFICIENT signal) |
| Checking CLAUDE.md Rules 1-10 compliance in generated code (isolated — no domain context) | principles-arbiter-SKILL.md (isolation rule; BLOCK/PASS only; Passed list names every rule; self-reinforcement check) |
| After all parallel wave phases merge — validating end-to-end product scenario | product-scope-validation-SKILL.md (5 steps: intent/scenario/trace/business-outcomes/report; PRODUCT-READY/INCOMPLETE/BROKEN_DEPENDENCY) |
| Gap recurring across sessions — finding which level to fix (immediate/structural/architectural) | root-cause-ladder-SKILL.md (3-level structure; cross-gap convergence; rejection response protocol) |
| Designing or reviewing data access that reads or writes ISessionStorage or DomainModelRegistry | scope-isolation-arbiter-SKILL.md (3-tier model: PRIVATE/SHARED/PLATFORM; scope declaration; isolation test pattern) |
| Writing any session execution, brief, or state file | session-file-authoring-SKILL.md (crystallization protocol; 8 self-containment checks; Gate D goal advancement block) |
| Phase complete — validating completion claims before progression | qa-session-type-SKILL.md (5-step protocol; APPROVED/DEFECTS_FOUND; BLOCKING/NON-BLOCKING; training data quality step for DPO phases) |
| Multiple gaps identified — designing the fix sessions after root cause analysis | remediation-session-design-SKILL.md (fix class taxonomy; blast radius; dependency ordering; pre-flight assumptions per group) |
| Design exists and code exists but feature not wired to user-visible surface | materialization-session-type-SKILL.md (3 mandatory inputs; 1-5 task constraint; 4-step process; round-trip verification) |
| New capability designed — reviewing spec before writing any code | capability-design-review-SKILL.md (4 checks: intent/constraints/quality/portability; READY_FOR_GENERATION/NEEDS_REVISION/ESCALATE_TO_PLANNING) |
| AI pipeline score below threshold — diagnosing which layer failed and producing fix proposals | meta-arbiter-SKILL.md (4-step diagnosis: retrieval/assembly/effectiveness/skill; two-call execution; PROMPT_PATCH/SKILL_EDIT/SKILL_ADD proposals; Luba approval required) |
| AI pipeline cycle failed — deciding whether to retry, reduce panel, or escalate to Luba | meta-escalation-router-SKILL.md (decision hierarchy: ACCEPT/HARD_STOP/HALT/STUCK/REGRESSION/algorithmic; escalation briefing 5-question format) |
| Selecting or pruning AI models for a capability's pipeline pool | ai-model-selection-SKILL.md (3-tier model hierarchy; value ratio formula; three-consecutive replacement signal; phase progression; manual fix threshold) |
| Session starting — determining minimum documents, governance chain, and blocking conditions | session-scope-resolution-SKILL.md (dispatch table: EXECUTOR/ARCHITECT/INVESTIGATOR/MAINTAINER/DEBUG; minimum docs per type; governance chains) |
| ARCHITECT session starting or proposal being made — load prior locked decisions first | architectural-decision-testing-SKILL.md (decision loading before proposing; 3 timeframes: immediate/short-term/long-term; invalidation procedure; negative knowledge retention) |
| Phase complete — capturing planning decisions as DPO triples for the learning loop | planning-dpo-authoring-SKILL.md (3 planning signal types: ROUTING_DECISION/CALIBRATION_COMPARISON/ESCALATION_DECISION; GENERATED vs REINFORCEMENT; outcome attribution) |
| DomainModelRegistry has no edges for a new capability or confidence below threshold | graph-extension-protocol-SKILL.md (3 mutation types: ADD_EDGE/PROMOTE_EDGE_TYPE/PROPOSE_NEW_NODE; 3-phase approval; REJECTION_REASON edges; evidence bars) |
| Planning which subprojects to build first — infrastructure vs user-facing, sequential vs parallel | wave-assignment-SKILL.md (N>2 dependents = FOUNDATION; sequential vs parallel rules; gate type per wave; prerequisite chain format) |
| Before designing a new capability or phase for an existing subproject | system-intake-SKILL.md (4 properties: structure/intent/constraints/quality; extraction protocol; domain context package; CONTEXT_INSUFFICIENT signal) |
| AI pipeline score in DETAIL_GAP bracket (0.65–0.84) — applying targeted patch | prompt-patch-authoring-SKILL.md (Rule 0: one bottleneck; Rule 1: NAMED CHECK + WRONG/CORRECT; target zone 0.80–0.90; regression checklist; patch history record) |
| After any AI pipeline cycle produces a score — before deciding next action | score-interpretation-SKILL.md (5 sub-scores; 5 brackets; decision tree; machine_constant < 0.85 hard stop; record interpretation before acting) |
| Named check fires consistently (score 0 or near-zero for 3+ sessions), or machine_constant < 0.85 | score-zero-investigation-SKILL.md (Rule 0: never remove; 3 fix classes A/B/C; silent failure subtypes; 6-step investigation protocol) |
| Before finalizing any topology design, EngineContract, or RAG pattern | self-questioning-SKILL.md (QUESTION YOURSELF ≥3 questions; selfQuestions[]/answers[]/modificationsFromQuestioning[]; SELF-Q-001/002/003 validation) |
| Designing execution structure for a capability class in ContinuousLearningPipeline | ai-pipeline-topology-SKILL.md (9 node handlers; 6 wiring patterns A-F; mandatory elements; Pattern F required for DPO training data) |
| Before modifying any .cs file this session — mapping callers and consumers | code-examination-SKILL.md (5-step: read completely / map callers / map consumers / hypothesis before tests / read tests; diagnostic routing table) |
| Fix attempt failed or investigation spans sessions — persisting debug state | debug-session-SKILL.md (active-session.md format; ELIMINATED/CONFIRMED/INVESTIGATING; Confirmed Facts; lifecycle: create/append/archive) |
| Bug confirmed and needs classification before writing failing tests | pipeline-bug-taxonomy-SKILL.md (6 classes A-F; Q1-Q3 disambiguation rules; 8-field FIX template with worked example) |
| Topology feedback node missing learning signal, or capability confidence not improving | learning-signal-capture-SKILL.md (9 signal types; OUTCOME required; DPO validity gate; curriculum tier table ROUTING=1..SCHEDULED=5; LEARNING-001 check) |
| Designing a capability's verified representation before writing genesis context package | capability-convergence-SKILL.md (proposer + 5 challenger roles; CONTEXT_INSUFFICIENT handling; CapabilityRepresentation verification checklist; genesis derivation) |
| Before writing or reviewing any Windows Forms control, layout, panel, or form | ui-quality-standard-SKILL.md (5 absolute bans; design hierarchy: typography/color/layout; AI Slop Test; checker protocol 4 phases) |
| Before accepting prose output from any generation node — checking for shortcodes in prose | output-readability-gate-SKILL.md (4 prohibited patterns: LETTER-DASH-NUMBER / bare letter+number / ALL_CAPS_UNDERSCORE in prose / version suffix codes; exception: inside code fences) |
| Score in PATTERN_MISSING bracket (0.60–0.79) after capability run — adding negative examples | negative-example-patching-SKILL.md (NEGATIVE EXAMPLE format; 9-step procedure; score decision table; max 3 cycles; DPO triple captured on resolution) |
| Before implementing a new capability class — tracing handler chain for shape mismatches | capability-simulation-SKILL.md (step table: Step/Handler/Input/Output/Actual/Gap; 5 verdicts WORKS/PARTIAL/BREAKS/WRONG/SILENT_FAILURE; 3 depth levels L1/L2/L3; gap format) |
| UI change affects more than one panel, feature, or form state — structured examination round | ui-examination-round-SKILL.md (cell model: Form×Feature×UserRole×State; 5 phases Baseline/Systemic/Per-Cell/Rescore/Convergence; 3 convergence conditions; improvement claim rules) |
| Before any synthesis, plan output, or architectural verdict — building evidence base | reconnaissance-gate-SKILL.md (thresholds per session type; RECON REPORT; evidence layers DESIGN_DOC/IMPLEMENTATION/TEST/RECONCILIATION; domain universe; .NET Tier 0 reads) |
| Session examining N≥5 instances of the same entity type (subprojects, capabilities, interfaces) | per-entity-examination-SKILL.md (7-step protocol; pre-declared hypotheses before instance 1; Freeze Gate: completeness + convergence + implementation evidence) |
| Capability route.handler has hardcoded thresholds — converting to learnable graph-backed routing | graph-backed-routing-SKILL.md (5 refactoring rules; two-step feedback: Pending→SuccessWithinBudget/WastedCycle/EscalationRequired; route.handler still exists as fallback) |
| AI pipeline cycle needs auditability — recording what was sent, received, decided, and changed | cycle-visibility-record-SKILL.md (4-field record: SENT/RECEIVED/DECIDED/CHANGED; per-cycle mandatory; reasoning must cite SENT criterion; NothingChanged requires explicit statement) |
| Missing capability discovered — governed process for extending engine capabilities | self-extension-session-SKILL.md (SELF-EXTENSION session type; 5-step chain: state→gap→STOP→build→integrity→audit→STOP; not autonomous; distinct from MAINTENANCE) |
| Before building a new Windows Forms form — intentional design process, not defaults | interface-design-craft-SKILL.md (Intent First: who/what/feel; domain exploration: concepts/colors/signature; per-component checkpoint; subtle layering; swap test / signature test / squint test) |
| Capability confidence not improving despite runs — auditing actual signal emission in code | learning-path-audit-SKILL.md (5 signal types: DPO_TRIPLE/OUTCOME/ARBITER_VERDICT/MODEL_COMPARISON/DESIGN_REASONING; FIRES?/COMPLETE?/CORRUPTION? per signal; verdict: LEARNING/PARTIAL/GENERATING-NOT-LEARNING/STRUCTURAL-FAILURE) |
| Implementing feedback.handler for DPO capture, or triple count below minimum before promotion | dpo-triple-format-SKILL.md (6 mandatory fields: identity/context/rejected/chosen/teaching-point/curriculum-tier; source node per field; minimum count per capability class; Phase E promotion gate) |
| Topology declares arbiters — registering named checks and verifying replay before Phase B | arbiter-registration-SKILL.md (named check registry; fixture format 8 required fields; arbiter replay gate: fires-on-bad + passes-on-good; score-zero/build-failure/warning severity) |
| Designing a service whose output changes based on time distance from an anchor event | temporal-behavior-SKILL.md (TEMPORAL archetype; 6 phases Baseline→Archive; 3 iron rules IR-TEMPORAL-1/2/3; ISchedulerService for transitions; single parameterized handler) |
| Establishing done criteria at the fleet level before claiming fleet-level improvement | capability-fleet-completion-SKILL.md (5 states: NOT_STARTED/PARTIAL/EXAMINED/BLOCKED/APPROVED; 3 EXAMINED criteria; fleet progress = N_examined/(N_total−N_blocked); work priority protocol) |
| Phase A gate — verifying minimum arbiter type coverage before Phase B can begin | arbiter-type-coverage-SKILL.md (type matrix: scope_isolation/security/iron_rules/domain/ordering_invariant; severity CRITICAL/HIGH/MEDIUM; 4-step assessment; detection commands) |
| Existing capability plan was written before new infrastructure became available | plan-restructure-SKILL.md (delta analysis: UNCHANGED list + CHANGED list; 4 session split criteria; document architecture block before writing files) |
| Writing PHASE-COMPLETE-Pn.md or tracking gap scores toward local model graduation | engine-independence-tracking-SKILL.md (GapScoreRecord; ShadowStatus enum; gap < 5% for 3 runs = Graduated; 80 DPO triples → graduation test; ENGINE PROGRESS section is FIRST) |
| Before designing any new Windows Forms form — declaring the interaction grammar | ui-screen-grammar-SKILL.md (7 grammar types G1–G7: PROGRESS_STRIP/VERDICT_GRID/CARD_LIST/TOPOLOGY_CANVAS/KIOSK/DASHBOARD/SETTINGS_TABS; declare grammar before any controls) |
| Before any STOP gate — self-scoring output for file:line / N-of-M / verbatim concreteness | specificity-calibration-SKILL.md (Score = file:line + N-of-M + verbatim excerpts; thresholds: EXECUTOR=10/ARCHITECT=11/PLAN-REVIEW=20/DESIGN-REVIEW=28; self-check block required; 7 anti-gaming rules) |
| Skill document version bumped or DomainModelRegistry confidence diverged from skill source | skill-graph-sync-SKILL.md (GovernsSeedEdge; 3 drift paths by confidence >0.8/0.5–0.8/<0.5; SkillCompiler two-pass extraction; SkillCompiler is bootstrap not topology) |

---

## Skill index

| Skill file | What it governs | Version |
|---|---|---|
| code-execution-SKILL.md | Test gate, tier system, Gate B format | v2.0 |
| dev-safety-SKILL.md | Build gate, commit discipline, DNA quick check | v2.0 |
| experiment-design-SKILL.md | Experiment structure, SCALE_UP, locked decisions D-03..D-10 | v1.0 |
| mental-debug-SKILL.md | Test failure triage, 8-rule debug protocol with routing table | v3.0 |
| no-product-decisions-SKILL.md | When to stop and ask Luba vs self-resolve | v1.0 |
| plan-review-SKILL.md | FC gates including FC-30 allowed trainable architecture pattern gate, SK-REVIEW first-class plan review, source-map/source-disjoint evidence completeness, proprietary-teacher dataset rebuild hard fail, scenario-counter readiness review, Authority Requirements Ledger + Delta + Replacement traceability, Gate B format, all 10 Rules | v4.3 |
| codex-planning-quality-GUIDE.md | Planning/review quality guide for benchmark-depth, focus retention, source-disjoint evidence, supplement disposition, prefix quarantine, population proof, and final Open Decisions Queue gates | guide |
| reference-inventory-and-source-map-SKILL.md | Mandatory source inventory and source-to-plan/source-to-review map for deep planning and plan review, including source-disjoint evidence for high-risk/multi-root claims | v1.0 |
| test-integrity-SKILL.md | FM-1..FM-9: stub masking, guard stacking, coverage inflation, sibling gaps, UI/UX scope inflation, pseudo-trainable evidence gaps, and sub-agent firewall evidence | v1.2 |
| dna-compliance-guard-SKILL.md | Pre-commit gate for all 10 Rules + BaseService + OperationResult namespace + generated output | v1.0 |
| three-level-verification-SKILL.md | Unit→Integration→Suite levels before declaring a fix done | v1.0 |
| integration-testing-SKILL.md | In-process fabric coverage matrix: BM25, VectorSearch, HybridRetriever, SessionStore, PermissionGate | v1.0 |
| ui-testing-SKILL.md | Windows Forms testing: layout standard, manual checklist, xUnit for library, demo stub pattern | v1.0 |
| session-constitution-SKILL.md | Supreme law: session start/end protocol, critique-response rule, forbidden decisions | v1.0 |
| verification-before-completion-SKILL.md | Iron law: no completion claims without fresh dotnet test evidence; Runtime Evidence Firewall (rejects validation_only / static / contract / UNKNOWN; requires numeric runtime metrics); downgrade/blocker/status-only evidence creates next concrete work; injected via SK-VERIFY | v1.0 |
| bug-to-tests-SKILL.md | BUG-NN format, 3 failing tests before fix, 6-step protocol, zero-regression gate | v1.0 |
| documentation-sync-SKILL.md | File → canonical docs maintenance map, sync checklist, what "synced" means | v1.0 |
| self-verification-SKILL.md | 6 rules: fresh fixtures, categorize change, right level, cascade, non-renderable, predict | v1.0 |
| retroactive-development-SKILL.md | Fix engine not output, propagation protocol, blast radius after engine fix | v1.0 |
| agent-output-format-SKILL.md | Three-file rule (REFERENCE/STATE/EXECUTION), SESSION-0 gate with Requirements Ledger, scenario-counter product-readiness output, and triggered trainable-architecture check | v1.1 |
| phase-preflight-SKILL.md | Assumption registry, 3 fix patterns, escalation format, out-of-scope issue inventory | v1.0 |
| generated-code-review-SKILL.md | 5-layer review: semantic, DNA, silent failure (MACHINE constant test), training data, trainable-vs-stub (Layer 5 numeric-metric test for ILogicalUnit/ILearningMachine) | v1.0 |
| phase-planning-SKILL.md | 9-gate pre-code validation with sub-agent-only routing and scenario-counter readiness planning: requirements ledger -> discovery -> DNA -> fabric -> deps -> decisions -> tests -> docs -> principles | v1.2 |
| infrastructure-discovery-SKILL.md | 10-step discovery: pre-existing failures, artifact boundaries, file reading, plan rewrite with file:line | v1.0 |
| root-cause-tracing-SKILL.md | Trace backward through call chain, defense-in-depth after fix, instrumentation pattern | v1.0 |
| context-overflow-SKILL.md | Three-threshold protocol: >35% continue / ≤35% wrap-up / ≤25% emergency stop + STATE format | v1.0 |
| tracker-SKILL.md | IProjectTrackerService FREEDOM/MACHINE split, local-file default, session usage protocol | v1.0 |
| skill-injection-SKILL.md | 9 injectable blocks incl. SK-REVIEW (first-class plan review), SK-VERIFY (anti-stub), SK-QA (engineering test strategy), SK-ARCH (allowed trainable architecture), and injection of contradiction/open-decision/supplement/prefix/population/source-disjoint gates when triggered | v1.4 |
| capability-measurement-SKILL.md | 6 capability classes, score scale 0-100%, evidence citation, Tier 1/2/3 manifest | v1.0 |
| event-driven-debugging-SKILL.md | Binary search through IAgentMessageBus chain, 4 break classes (producer/subscriber/transform/read) | v1.0 |
| freedom-machine-SKILL.md | MACHINE vs FREEDOM classification, annotation format, hardcoded value detection commands | v1.0 |
| output-skepticism-SKILL.md | 3 skeptic questions (refutation/scope/proxy), format before reporting, worked examples | v1.0 |
| test-failure-triage-SKILL.md | Class 1-4 failure classification: regression/infrastructure/fragility/environment | v1.0 |
| session-state-crystallization-SKILL.md | ≤40-line snapshot format, execution position, blocking prerequisites, artifact boundaries | v1.0 |
| phase-git-report-SKILL.md | Commit convention, bidirectional plan↔commit trace, test delta reporting | v1.0 |
| investigation-handoff-SKILL.md | Discovery/rejected-claim format, STATE-Pn.json continuous write, open questions | v1.0 |
| blast-radius-SKILL.md | CRITICAL/HIGH/MEDIUM/LOW classification, dependent grep commands, protection gates | v1.0 |
| blast-radius-tagger-SKILL.md | File tagging before writing, TEST/GOVERNANCE/PRODUCTION/INTERFACE categories | v1.0 |
| service-boundary-design-SKILL.md | 4 boundary tests (transaction/lifecycle/consumers/domain), method vs class vs subproject | v1.0 |
| domain-event-design-SKILL.md | IAgentMessageBus message naming, payload design rules, cross-component contracts | v1.0 |
| data-flow-integrity-SKILL.md | Chain data flow map, schema diff per link, MISSING/RENAMED/TYPE/NULLABLE gaps | v1.0 |
| user-journey-testing-SKILL.md | Scenario-first testing, guard conditions, stub before implementation, SCENARIO-MANIFEST.md | v1.0 |
| plan-execution-feedback-SKILL.md | Write-time fix tracking, discovery rate, Gate 0 calibration signal | v1.0 |
| problem-decomposition-SKILL.md | Ask "why" 3× to find root cause level, minimum 3-5 files, claim classification, scope response | v1.0 |
| claim-verification-SKILL.md | VERIFIED/INFERRED/ASSUMED table, high-risk claims, downgrade/status-only next-work rule, scenario counters, renumbering verification, phase-preflight integration | v1.0 |
| api-shape-verification-SKILL.md | Symbol exists, method signature, sync/async, property/method, OperationResult wrapping | v1.0 |
| implementation-integrity-SKILL.md | Fix instance + structural guard, 4 checks: docs/tests/DNA/no-new-gaps, JSON integrity report | v1.0 |
| gap-to-proposal-SKILL.md | Resolution ladder: CONVENTION→ADAPTATION→EXTENSION→NEW COMPONENT, Luba approval stop gate | v1.0 |
| prerequisite-chain-SKILL.md | List prerequisites, verification commands, classify PASS/FAIL/BLOCKING, block protocol | v1.0 |
| four-tier-decision-classification-SKILL.md | BOUNDARY CODE / FREEDOM CONFIG / LEARNABLE / AI PIPELINE tiers for every decision point | v1.0 |
| chain-arithmetic-audit-SKILL.md | Verify baseline + Σ(phase deltas) = final for test counts and capability metrics | v1.0 |
| naming-conventions-enforcer-SKILL.md | I-prefix interfaces, capability file names, [Class]Tests pattern, commit message format | v1.0 |
| phase-completion-packager-SKILL.md | 3-artifact procedure: STATE-Pn.json + PHASE-COMPLETE.md + handoff note; STOP gate | v1.0 |
| mission-progress-SKILL.md | 5 mandatory questions: training/capability/architecture/blocked/decision; first in PHASE-COMPLETE | v1.0 |
| assumption-registry-SKILL.md | 3 classes (INFRA/BEHAVIORAL/OPERATIONAL), verification command per assumption, STEP 0 table | v1.0 |
| coverage-completeness-gate-SKILL.md | Claim scope vs evidence scope; downgrade protocol; fires before every STOP | v1.0 |
| change-propagation-SKILL.md | 6 change types with full blast radius; derived views must update in same commit | v1.0 |
| artifact-numbering-SKILL.md | Read live DECISIONS-LOCKED.md + SUBPROJECT-MAP.md before assigning any number | v1.0 |
| defense-in-depth-SKILL.md | 4-layer validation (entry/business/environment/debug); OperationResult.Fail not throw | v1.0 |
| systematic-debugging-SKILL.md | 4-phase protocol; iron law: no fixes without root cause; architectural escalation at 3+ failures | v1.0 |
| decision-reopening-SKILL.md | 4-step test; valid triggers; ADR entry before any change; blast radius after reopening | v1.0 |
| intent-to-plan-SKILL.md | Parse intent clauses; plain language phases; single responsibility; Gate B per phase | v1.0 |
| improvement-measurement-SKILL.md | Unit test layer + behavioral layer required; input→output observable, not just test count | v1.0 |
| training-data-gap-audit-SKILL.md | RECOVERABLE/CONTAMINATED/CALIBRATION classification; remediate before training run | v1.0 |
| session-execution-log-SKILL.md | EXECUTION-LOG-Pn.json schema; written after gate passes; exact counts required | v1.0 |
| goal-context-persistence-SKILL.md | Verbatim goal capture; reminder at every STOP; drift detection checkpoints every 5 turns | v1.0 |
| work-scope-inventory-SKILL.md | Scope tracking record before work; 6-count protocol; claim authority table | v1.0 |
| session-mode-declaration-SKILL.md | 5 modes (ARCHITECT/PLANNER/REVIEWER/EXECUTOR/INVESTIGATION); drift signals; restart on drift | v1.0 |
| session-handoff-SKILL.md | SESSION-BRIEF-Pn.md: project state + what was built (prose) + discoveries + next phase | v1.0 |
| solution-scope-gate-SKILL.md | 5-level scope ladder (CONVENTION→NEW INTERFACE); gap classification table; 3 reframe tests | v1.0 |
| plan-self-validation-SKILL.md | Phase 0 requirement before Phase 1; 5 checks: requirements ledger / goal delivery / phantom skills / presence matrix / order | v1.1 |
| mvp-round-trip-verification-SKILL.md | 8-step round-trip from capability authored to end-to-end result observable; round-trip record schema | v1.0 |
| subproject-examination-SKILL.md | 3-doc output before implementation: master plan + missing infrastructure + learning spec | v1.0 |
| pipeline-trace-reader-SKILL.md | Stage-by-stage trace reading; first-failing-stage fix target; score band → action mapping | v1.0 |
| quality-improvement-cycle-SKILL.md | Score band protocol; 9-step improvement cycle; cycle log; max 3 cycles before escalation | v1.0 |
| capability-adaptation-map-SKILL.md | Per-class expected failure modes; cycle budgets; escalation triggers; adaptation map table format | v1.0 |
| algorithm-as-service-SKILL.md | 5 algorithm classes (weighted/decay/recommendation/matching/gamification); iron rules per class | v1.0 |
| architect-behavior-classifier-SKILL.md | 7 positive habits + 36 failure modes (FAIL-1..FAIL-36); 3-step doc-first loop; pre-STOP checklist | v1.0 |
| confidence-lifecycle-SKILL.md | 5-phase lifecycle (seed/learn/promote/decay/graduate); immutable entries; risk-tier thresholds | v1.0 |
| capability-routing-gate-SKILL.md | 4-question test; bootstrap list; capability routing record; PLAIN_CS challenge protocol | v1.0 |
| claim-as-hypothesis-SKILL.md | User claim → PENDING → verify with grep/Read → VERIFIED/DISCONFIRMED/PARTIAL; BLOCKING/INFORMATIONAL gate | v1.0 |
| design-artifact-completeness-SKILL.md | 5 checks (exists/populated/specific/matches-spec/reachable); stub detection; COMPLETE/INCOMPLETE status | v1.0 |
| e2e-test-matrix-SKILL.md | 8 mandatory scenario categories; time-based test protocol (inject mock clock); N/A requires justification | v1.0 |
| multi-reviewer-design-SKILL.md | 5 reviewer roles; context isolation; BLOCK overrides APPROVED; convergence protocol; 3-cycle escalation | v1.0 |
| feature-prioritization-SKILL.md | 3-axis score (business value × dependency multiplier × infrastructure cost⁻¹); override rule; dependency lock rule | v1.0 |
| subproject-completeness-checker-SKILL.md | Pre-Gate 1 (goal-to-phase mapping) + Pre-Gate 2 (artifact populated); T1–T10 technical checks; NOT_READY blockers | v1.0 |
| capability-improvement-cycle-SKILL.md | 5-stage cycle (gap→design→score→observe→promote); cycle health metrics; escalation after cycle 3 | v1.0 |
| phase-retrospective-SKILL.md | 4 dimensions (cycle accuracy/pattern transfer/test coverage/gate adherence); MAE threshold; next-batch gate | v1.0 |
| goal-delivery-completeness-SKILL.md | First arbiter: goal elements → phase mapping → verification step; APPROVED/BLOCK/CHALLENGE; blocks all other checks | v1.0 |
| iron-rule-derivation-SKILL.md | 4-step derivation (failure modes → invariant → CF rule → MACHINE/FREEDOM); quality test; per-capability iron rule tests | v1.0 |
| learning-loop-closure-SKILL.md | 4-event loop (decide/observe/update/promote); learning plan template; pre-graph discovery rule; broken event diagnosis | v1.0 |
| diagnostic-escalation-SKILL.md | 4-step diagnosis (test data/wiring/stub/algorithm); 3 situations; escalation triggers; proposal format | v1.0 |
| authority-requirement-binding-SKILL.md | Authority-chain requirements ledger; conversation delta; replacement mapping; contradiction sweep; PromptFlowGroundingModel deterministic-shortcut guard | v1.0 |
| planning-session-startup-SKILL.md | Authority Requirements Ledger + 5 pre-plan questions (authority requirements / mode+scope / baseline / decisions / documents / blockers); startup output format; source-disjoint evidence and proprietary-teacher dataset rebuild gates | v1.2 |
| level-correction-response-SKILL.md | 3 proactive tests (next-instance/provider-vs-capability/old-model-survival); abandon-and-rebuild rule | v1.0 |
| capability-state-reader-SKILL.md | 4-step snapshot (interfaces/tests/DomainModelRegistry/blockers); ACTIVE/STUB/MISSING classification | v1.0 |
| audit-protocol-SKILL.md | 10-point audit checklist; BLOCKING/WRITE-TIME/INFORMATIONAL severity; NOT CHECKED section required | v1.0 |
| graph-entity-schema-SKILL.md | NODE/EDGE/PROPERTY/IMMUTABLE classification; 5-field edge schema; confidence seed values; promotion paths | v1.0 |
| cycle-budget-prediction-SKILL.md | Formula: archetypeBaseline + firstOccurrence + noveltySum − clarityDiscount + inversionBonus; budget card; MAE recalibration | v1.0 |
| archetype-failure-map-SKILL.md | Score bracket actions (PASS/NEAR-PASS/PATCH/STRUCTURAL_FAILURE); per-capability-class failure modes; cycle escalation triggers | v1.0 |
| context-package-design-SKILL.md | Instruction file vs context package distinction; 5 fields (INTENT/DOMAIN/CONSTRAINTS/PRIOR_ART/SUCCESS); 8-check verification checklist | v1.0 |
| ai-pipeline-design-SKILL.md | When to use AI vs graph lookup; 4 roles (implementors/arbiters/context/manager); DPO triple structure; graduation path | v1.0 |
| arbiter-panel-design-SKILL.md | 8 roles (Goal Delivery runs first); Role 8 two-input isolation; minimum panel by archetype; upper evaluator expansion | v1.0 |
| decision-capture-SKILL.md | DESIGN_REASONING format; 6 decision types; question field = retrieval key; reasoning = teachable record | v1.0 |
| capability-implementation-gate-SKILL.md | Bootstrap list (exhaustive); 4-question test (Q1-Q4); capability routing declaration; challenge protocol | v1.0 |
| confidence-seeding-SKILL.md | Evidence-based seed formula (0.1 + N×0.04); update deltas by edge type; decay protection window; risk-tier thresholds | v1.0 |
| challenger-round-design-SKILL.md | Per-archetype minimum challenger set; 5 challenger prompt templates; CONSENSUS/DEFERRED/STALLED termination; genuine disagreement | v1.0 |
| shared-service-design-SKILL.md | Count appearances across subprojects; 3 sharing patterns (shared write+read / shared read / event bus); shared services map format | v1.0 |
| capability-decomposition-SKILL.md | LEAF vs EXPAND; 5 complexity signals (intent/inputs/failures/cross-domain/Gate B); termination bound; non-overlap verification | v1.0 |
| document-hierarchy-SKILL.md | 5-tier hierarchy (CLAUDE.md > DECISIONS-LOCKED > COMPONENT-INTERFACES > skills); routing table; conflict resolution; pre-document 3 checks | v1.0 |
| escalation-orchestrator-SKILL.md | 6 rules in order (BLOCK/ACCEPT/score-winner/upper-judge/human/UNDECIDED); DPO triple from Rule 3; DESIGN_FLAW from Rule 6 | v1.0 |
| event-contract-design-SKILL.md | 3 correlation fields (CorrelationId/SessionId/TraceParent); PII rule; compensation events; SLA annotation; stack-neutral check | v1.0 |
| session-output-contract-SKILL.md | 3-part contract (file list/verification commands/spec gate); zeroth deliverable; read-before-write rule; does-not-produce section | v1.0 |
| parallel-execution-coordinator-SKILL.md | 3 questions (parallel mode/artifact ranges/prerequisite); artifact pre-allocation; delta gate; CAS lifecycle write; execution log | v1.0 |
| pipeline-position-check-SKILL.md | Q0a-Q0d (receives/produces/consumer/consumer-requirement); CONTEXT_INSUFFICIENT signal; resumption gate; pipeline contract check | v1.0 |
| principles-arbiter-SKILL.md | Isolation rule (ONLY CLAUDE.md Rules 1-10 + code); BLOCK/PASS verdict; Passed list names every rule; self-reinforcement; growth rule | v1.0 |
| product-scope-validation-SKILL.md | 5-step post-wave validation (intent/scenario/trace/business-outcomes/report); PRODUCT-READY/INCOMPLETE/BROKEN_DEPENDENCY; two-subproject minimum | v1.0 |
| root-cause-ladder-SKILL.md | 3-level structure (immediate/structural/architectural); root cause record format; cross-gap convergence; rejection response; DESIGN_FLAW escalation | v1.0 |
| scope-isolation-arbiter-SKILL.md | 3-tier scoping (PRIVATE/SHARED/PLATFORM); scope declaration required; mixed-scope read rule; ScopeVerdict format; isolation test pattern | v1.0 |
| session-file-authoring-SKILL.md | Crystallization protocol (4 requirements); 8 self-containment checks; Gate D goal advancement block; session file header template | v1.0 |
| qa-session-type-SKILL.md | 5-step QA protocol (verify claims/acceptance criteria/integration/DPO quality/report); APPROVED/DEFECTS_FOUND; BLOCKING/NON-BLOCKING classification | v1.0 |
| remediation-session-design-SKILL.md | Fix class taxonomy (CONTENT/EXTENSION/INFRASTRUCTURE/NEW_CAPABILITY); blast radius; dependency ordering; session group format with Gate B | v1.0 |
| materialization-session-type-SKILL.md | MATERIALIZATION session type; 3 mandatory inputs (design/surface/round-trip); 4-step process (inventory/gap/plan/verify); ≤5 task constraint | v1.0 |
| capability-design-review-SKILL.md | 4 checks (intent completeness/constraint integrity/quality measurability/portability); READY_FOR_GENERATION/NEEDS_REVISION/ESCALATE_TO_PLANNING | v1.0 |
| meta-arbiter-SKILL.md | 3 situations; 4-step diagnosis (retrieval/assembly/effectiveness/skill); two-call execution (diagnosis + blind ranking); proposals require Luba APPROVED | v1.0 |
| meta-escalation-router-SKILL.md | Decision hierarchy (6 steps before escalation); score trajectory classification (CONVERGING/PLATEAU/STUCK/REGRESSION); 5-question escalation briefing; gap analysis format | v1.0 |
| ai-model-selection-SKILL.md | 3-tier model hierarchy; value ratio formula; three-consecutive replacement signal; phase progression (broad→archetype-optimized); manual fix threshold | v1.0 |
| session-scope-resolution-SKILL.md | Dispatch table for 5 session types; minimum documents per type; governance chains in order; blocking conditions; ISSUE INVENTORY requirement | v1.0 |
| architectural-decision-testing-SKILL.md | Decision loading before proposing; 3 testing timeframes (immediate/short-term/long-term); DECISION_REOPENING challenge format; invalidation procedure | v1.0 |
| planning-dpo-authoring-SKILL.md | 3 signal types (ROUTING_DECISION/CALIBRATION_COMPARISON/ESCALATION_DECISION); GENERATED vs REINFORCEMENT; CountsTowardThreshold; outcome attribution (SUCCESS/WASTED/ESCALATION) | v1.0 |
| graph-extension-protocol-SKILL.md | 3 mutation types with evidence bars; 2 prohibited actions (immutable/locked); 3-phase approval (screening/simulation/Luba); REJECTION_REASON edges | v1.0 |
| wave-assignment-SKILL.md | FOUNDATION test (N>2 dependents); sequential vs parallel rules (≥3 priors); gate type per wave (absolute vs delta); wave assignment record; prerequisite chain format | v1.0 |
| system-intake-SKILL.md | 4 extraction properties (structure/intent/constraints/quality); extraction protocol for C#/docs/legacy; domain context package record; minimum viable intake | v1.0 |
| prompt-patch-authoring-SKILL.md | 6 rules (one bottleneck / NAMED CHECK+WRONG/CORRECT pair / 0.80–0.90 target zone / regression checklist / multi-generate verify / patch history); patch format template | v1.0 |
| score-interpretation-SKILL.md | 5 sub-scores (iron_rules/dna_compliance/testability/machine_constant/fail_open); 5 brackets (PRESCRIPTIVE/PASS/DETAIL_GAP/PATTERN_MISSING/STRUCTURAL); decision tree; ScoreInterpretation record | v1.0 |
| score-zero-investigation-SKILL.md | Rule 0 (never remove named check); 3 fix classes (A=prompt addition / B=over-fires+narrow / C=contract revision); silent failure subtypes; 6-step investigation protocol | v1.0 |
| self-questioning-SKILL.md | QUESTION YOURSELF protocol (≥3 questions, 5 standard); output: selfQuestions[]/answers[]/modificationsFromQuestioning[]; SELF-Q-001/002/003 downstream validation; domain-specific question sets | v1.0 |
| ai-pipeline-topology-SKILL.md | 9 node handler types (rag-retrieve/decompose/ai-generate/multi-generate/arbiter-panel/validate/score/route/feedback); 6 wiring patterns A-F; mandatory elements; Pattern F for DPO training data | v1.0 |
| code-examination-SKILL.md | 5-step protocol (read completely / map callers / map consumers / hypothesis before tests / read tests); diagnostic routing table; anti-pattern: tests before implementation | v1.0 |
| debug-session-SKILL.md | Persistent debug session file (.claude/debug/active-session.md); ELIMINATED/CONFIRMED/INVESTIGATING states; Confirmed Facts; lifecycle (create/append/archive); load before codebase | v1.0 |
| pipeline-bug-taxonomy-SKILL.md | 6 bug classes (A=fabric/B=event-bus/C=DNA/D=pipeline-stage/E=cross-subproject/F=contract); Q1-Q3 disambiguation; 8-field FIX template | v1.0 |
| learning-signal-capture-SKILL.md | 9 signal types (OUTCOME/DPO_TRIPLE/CALIBRATION/GAP_SIGNAL/DESIGN_FLAW/PROMPT_PATCH/MODEL_COMPARISON/SHADOW_RUN/ARBITER_VERDICT); LEARNING-001; DPO validity gate; curriculum tier table; DESIGN_REASONING signal | v1.0 |
| capability-convergence-SKILL.md | Proposer + 5 challenger roles; CONTEXT_INSUFFICIENT handling; CapabilityRepresentation verification checklist (no class names / intent = user outcome / constraints have failing checks); genesis derivation | v1.0 |
| ui-quality-standard-SKILL.md | 5 absolute bans (side-stripe / gradient-text / identical-grid / generic-shadow / modal-low-stakes); design hierarchy (typography/color/layout); AI Slop Test; 4-phase checker protocol | v1.0 |
| output-readability-gate-SKILL.md | 4 prohibited patterns in prose (LETTER-DASH-NUMBER / bare letter+number / ALL_CAPS_UNDERSCORE / version suffix codes); .NET project equivalents; 4-checkbox gate check; code fence exception | v1.0 |
| human-readable-architecture-audit-SKILL.md | Human-Readable Architecture Artifact Gate; Concrete Repair Blueprint Gate; Audit Sufficiency Boundary Gate; Delta-Severity Feedback Gate; operator-language summary, PlantUML/draw.io UML, no-code algorithms, quote-adjacent concrete solutions, breadth challenge, minimum 6 readability cycles when requested | v1.1 |
| negative-example-patching-SKILL.md | NEGATIVE EXAMPLE format (WRONG comment before bad code + CORRECT); 9-step procedure; score decision table (≥0.85 PASS / PLATEAU / REGRESSION escalations); max 3 cycles; accumulation rule | v1.0 |
| capability-simulation-SKILL.md | Step table (Step/What/Handler/Input-status/Output-expected/Actual/Gap); 5 verdicts (WORKS/PARTIAL/BREAKS/WRONG/SILENT_FAILURE); SILENT_FAILURE = highest priority; 3 depth levels L1/L2/L3; gap record format | v1.0 |
| ui-examination-round-SKILL.md | Cell model (Form×Feature×UserRole×State); 5-phase round (Baseline/Systemic/Per-Cell/Rescore/Convergence); 3 convergence conditions; improvement claim rules (form/feature-group/fleet/convergence scope) | v1.0 |
| reconnaissance-gate-SKILL.md | Evidence thresholds per session type (EXECUTOR=5/2/3 … ARCHITECT=20/8/10); RECON REPORT format; evidence layers (DESIGN_DOC/IMPLEMENTATION/TEST/RECONCILIATION); domain universe block; .NET Tier 0 reads | v1.0 |
| per-entity-examination-SKILL.md | 7-step protocol (universe declaration / hypotheses before instance 1 / step sequence / result format / all-N examination / tally / synthesis after); Freeze Gate 3 conditions; examiner vs synthesis separation | v1.0 |
| graph-backed-routing-SKILL.md | 5 refactoring rules; rag-retrieve + route.handler + feedback pattern; two-step feedback (Pending→outcomeClass at Phase F); RoutingEdge schema; what NOT to refactor | v1.0 |
| cycle-visibility-record-SKILL.md | 4-field record (SENT/RECEIVED/DECIDED/CHANGED); all 4 required per cycle; DECIDED requires criterion cite; NothingChanged requires reason; C# record + validator | v1.0 |
| self-extension-session-SKILL.md | SELF-EXTENSION session type; 5-step chain (state→proposal→STOP→build→integrity→audit→STOP); gap window training data impact; distinct from MAINTENANCE; not autonomous | v1.0 |
| interface-design-craft-SKILL.md | Intent First (who/what/feel); domain exploration (concepts/color-world/signature/defaults); per-component checkpoint (6 fields); subtle layering; swap/signature/squint tests | v1.0 |
| learning-path-audit-SKILL.md | 5 signal audits (DPO_TRIPLE/OUTCOME/ARBITER_VERDICT/MODEL_COMPARISON/DESIGN_REASONING); FIRES?/COMPLETE?/CORRUPTION? per signal; verdict classification (LEARNING/PARTIAL/GENERATING-NOT-LEARNING/STRUCTURAL-FAILURE) | v1.0 |
| dpo-triple-format-SKILL.md | 6 mandatory field groups; source node per field; PromptSystem required for fine-tuning; minimum triple count per capability class; Phase E DPO-GATE-001 promotion check | v1.0 |
| arbiter-registration-SKILL.md | Named check registry; fixture format (8 required fields); arbiter replay gate (fires-on-bad + passes-on-good); score-zero/build-failure/warning severity; hard rules (silent-skip on unknown CheckId) | v1.0 |
| temporal-behavior-SKILL.md | TEMPORAL sub-type of DATA_PIPELINE; 6 phases (Baseline/Approach/Imminent/Active/Decay/Archive); TemporalContext record; 3 iron rules (config thresholds/anchor from payload/ISchedulerService transitions); single parameterized handler | v1.0 |
| capability-fleet-completion-SKILL.md | Fleet examination tracking; 5 states (NOT_STARTED/PARTIAL/EXAMINED/BLOCKED/APPROVED); 3 EXAMINED criteria (record+context+audit); fleet progress formula; work priority protocol (NOT_STARTED first) | v1.0 |
| arbiter-type-coverage-SKILL.md | Minimum arbiter type matrix; 5 required types with trigger conditions; severity levels (CRITICAL/HIGH/MEDIUM); 4-step coverage assessment; detection commands for arbiter fixture files | v1.0 |
| plan-restructure-SKILL.md | Delta analysis for plans predating new infrastructure; UNCHANGED list (explicit, named); CHANGED list (phase + position + additive/replacement); 4 session split criteria; document architecture block | v1.0 |
| engine-independence-tracking-SKILL.md | Gap score formula (expensive − local per archetype); GapScoreRecord C# record; ShadowStatus enum (PendingLocalModel/Active/Graduated); graduation criteria (3 consecutive runs < 5% + 80 DPO triples + test); ENGINE PROGRESS section first | v1.0 |
| ui-screen-grammar-SKILL.md | 7 Windows Forms grammar types G1–G7; grammar declaration required before any controls; grammar violation signals table; grammar type determines primary control not layout | v1.0 |
| specificity-calibration-SKILL.md | Score = file:line refs + N-of-M integer claims + verbatim excerpts; thresholds by session type; dimension diversity (≥1 each); self-check block format; 7 anti-gaming rules; FAIL output rejected | v1.0 |
| skill-graph-sync-SKILL.md | GovernsSeedEdge links skill docs to DomainModelRegistry; GraphDriftDetector 3 paths (>0.8/0.5–0.8/<0.5 confidence); SkillCompiler two-pass extraction; SkillCompiler is bootstrap infrastructure; SkillGraphResolution record | v1.0 |
| implementation-doc-uml-SKILL.md | UML diagrams for implementation docs: class diagram with note for annotations, legend table, human-language flowcharts, per-connection simulation table (OP-54/FAIL-33). Required at plan start by FC-14. | v1.0 |
| implementation-doc-algorithms-SKILL.md | Algorithm descriptions: human analogy, example values, "what it does NOT do" section. Required by FC-15. | v1.0 |
| implementation-doc-code-refs-SKILL.md | Code reference explanations: WHY/HOW/WHAT per code block, internals of called functions. Required by FC-16. | v1.0 |
| implementation-doc-status-SKILL.md | Implementation status table: status symbols ✅⚠️🔶, concrete "what works / what is stub". Required by FC-17. | v1.0 |
| implementation-doc-hierarchy-SKILL.md | ILogical* hierarchy documentation: levels with input/output types, execution chains. Required by FC-18. | v1.0 |
| trainable-architecture-decision-SKILL.md | Allowed trainable architecture pattern gate: only ILearningMachine, ILogicalOrchestrator, ILogicalFlow, ILogicalUnit, and ILearningMachinePair are architecture levels; scorers/selectors/classifiers/routers/KNN/dictionaries/pattern matchers are internal details only. | v1.0 |

> V20 anti-stub + UML-simulation gate web: AGENTS.md OP-53/OP-54, plan-review-SKILL.md FC-25, architect-behavior-classifier-SKILL.md FAIL-32/FAIL-33, skill-injection-SKILL.md SK-VERIFY.
>
> Plan-testability gate web: AGENTS.md OP-55/OP-56/OP-57, plan-review-SKILL.md FC-26/FC-27/FC-28, architect-behavior-classifier-SKILL.md FAIL-34/FAIL-35/FAIL-36, skill-injection-SKILL.md SK-QA, planning-session-startup-SKILL.md Test Strategy Self-Check, phase-completion-packager-SKILL.md two-PASS gate.

> Allowed trainable architecture gate web: AGENTS.md OP-59, CLAUDE.md
> "Allowed trainable architecture pattern gate", DECISIONS-LOCKED.md
> A-LM-12/A-LM-13, trainable-architecture-decision-SKILL.md, plan-review-SKILL.md
> FC-30, architect-behavior-classifier-SKILL.md FAIL-42/FAIL-43,
> generated-code-review-SKILL.md Layer 5A, implementation-integrity-SKILL.md
> Check 9, test-integrity-SKILL.md FM-8, skill-injection-SKILL.md SK-ARCH.

> Human-readable architecture audit gate web: AGENTS.md OP-60/OP-61, CLAUDE.md
> "Human-readable architecture audit pattern",
> human-readable-architecture-audit-SKILL.md, output-readability-gate-SKILL.md,
> plan-review-SKILL.md FC-31/FC-32, architect-behavior-classifier-SKILL.md
> FAIL-44..FAIL-47, generated-code-review-SKILL.md human-readable and audit
> sufficiency report gates,
> implementation-doc-uml/algo/code-refs skills.

> Architect authority/self-reflection gate web: AGENTS.md OP-58,
> CLAUDE.md "Conversation intent and architect soft-skill pattern",
> conversation-intent-and-self-reflection-SKILL.md, plan-review-SKILL.md
> FC-29, architect-behavior-classifier-SKILL.md FAIL-37..FAIL-41,
> skill-injection-SKILL.md SK-INTENT, and
> ARCHITECT-AUTHORITY-AND-SELF-REFLECTION-REPAIR-PLAN-v1.md.
>
> Load this web before architect planning, plan review, governance/protocol/skill
> repair, sub-agent orchestration, direct Luba questions, settings challenges,
> mixed action+analysis messages, and many-source synthesis tasks.

New skills added by the authority/self-reflection repair:

| Skill | When to load |
| --- | --- |
| conversation-intent-and-self-reflection-SKILL.md | Before architect/review/governance/sub-agent orchestration and any Luba question/challenge/mixed-intent message |
| client-adaptive-learning-loop-SKILL.md | Any plan/review/work order that says a user-side adaptive model learns |
| plugin-runtime-reality-gate-SKILL.md | Claude/Cursor/Codex extension plans, reviews, and completion claims |
| uml-simulation-to-testcase-SKILL.md | Luba-readable UML/data-flow/algorithm plans that must become test cases |
| human-readable-architecture-audit-SKILL.md | Architecture/trainable/code audit reports that must be in the operator's language, human-readable, UML-backed, no-code algorithmic, quote-adjacent concrete, breadth-challenged, and honest about sufficiency |

Current extension execution gate web:

| Gate | Governing files |
| --- | --- |
| Model/Reasoning Configuration Gate | AGENTS.md OP-64, CLAUDE.md extension execution visibility pattern, planning-session-startup-SKILL.md, skill-injection-SKILL.md |
| Model/Effort Visibility Rule | AGENTS.md OP-64, CLAUDE.md extension execution visibility pattern, phase-planning-SKILL.md, output-readability-gate-SKILL.md |
| 10 Review Cycle Gate | AGENTS.md OP-64, plan-review-SKILL.md, phase-completion-packager-SKILL.md, multi-reviewer-design-SKILL.md |
| Ten-Review Evidence Gate | AGENTS.md OP-64, plan-review-SKILL.md, phase-completion-packager-SKILL.md, multi-reviewer-design-SKILL.md, output-readability-gate-SKILL.md |
| Execution Plan Visibility Gate | AGENTS.md OP-64, CLAUDE.md extension execution visibility pattern, output-readability-gate-SKILL.md, session-execution-log-SKILL.md |
| Plan Presentation Gate | AGENTS.md OP-64, CLAUDE.md extension execution visibility pattern, plan-review-SKILL.md, output-readability-gate-SKILL.md |
| Human-Readable Execution Board Gate | AGENTS.md OP-64, CLAUDE.md extension execution visibility pattern, output-readability-gate-SKILL.md, phase-completion-packager-SKILL.md |
| Human Gate for chat logs | AGENTS.md OP-64, CLAUDE.md extension execution visibility pattern, output-readability-gate-SKILL.md, plan-review-SKILL.md, verification-before-completion-SKILL.md, phase-completion-packager-SKILL.md, test-integrity-SKILL.md |
| No Machine Log To User Rule | AGENTS.md OP-64, output-readability-gate-SKILL.md, agent-output-format-SKILL.md |
| Sub-Agent Visibility Ledger | AGENTS.md OP-64, phase-planning-SKILL.md, phase-completion-packager-SKILL.md, agent-output-format-SKILL.md |
| Human-Readable Plan/Execution Log Gate | AGENTS.md OP-64, output-readability-gate-SKILL.md, session-execution-log-SKILL.md, phase-completion-packager-SKILL.md |
| Full Extension Definition Gate | AGENTS.md OP-64, plugin-runtime-reality-gate-SKILL.md, plan-review-SKILL.md, test-integrity-SKILL.md |
| Full Extensions Readiness Review Gate | AGENTS.md OP-64, CLAUDE.md extension execution visibility pattern, plan-review-SKILL.md, implementation-integrity-SKILL.md, test-integrity-SKILL.md |
| Final Execution Review Gate | AGENTS.md OP-64, phase-completion-packager-SKILL.md, verification-before-completion-SKILL.md, architect-behavior-classifier-SKILL.md |
| False Completion / Stop Repair Rule | AGENTS.md OP-64, phase-completion-packager-SKILL.md, verification-before-completion-SKILL.md, plan-review-SKILL.md |
| Deterministic-Boundary Code Review Gate | AGENTS.md OP-64, generated-code-review-SKILL.md, trainable-architecture-decision-SKILL.md, test-integrity-SKILL.md |
| Execution/Code Review Completeness Gate | AGENTS.md OP-64, CLAUDE.md extension execution visibility pattern, plan-review-SKILL.md, implementation-integrity-SKILL.md, generated-code-review-SKILL.md, code-examination-SKILL.md, test-integrity-SKILL.md |
| No-Stop Sequencing Gate | AGENTS.md OP-64, phase-completion-packager-SKILL.md, planning-session-startup-SKILL.md |

---

## Stack reference for xiigen developers

See `docs/skills/REMOVED-FROM-XIIGEN.md` for the full mapping of every xiigen concept to its
live equivalent in this project: IAgentMessageBus (event bus), ISessionStorage (cache),
BM25+VectorSearch+GraphRag (search), PermissionGate (APP_GUARD), Windows Forms UI, MCP, and more.
