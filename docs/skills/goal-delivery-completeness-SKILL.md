# Goal Delivery Completeness

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
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

## Skill-Load Evidence Packet Gate

- `skill_load_status_required_in_every_subagent_packet`: planning/review/architecture/governance/status/continuation/readiness packets must include `skill_load_status` with found/read/missing for `AGENTS.md` and required skills/guides.
- `missing_skill_load_status_invalidates_packet`: PASS, READY, no-safe-work, completion, hold, continuation, and readiness packets lacking required `skill_load_status` are invalid evidence and become NEEDS_FIX/recovery queue.
- `quoted_or_workorder_skill_list_is_not_load_evidence`: quoted transcript/work-order checklist/prompt list is not evidence that skills were loaded/read.
- Missing, unread, or inaccessible required skills/guides route to governance repair before product work unless Luba explicitly ordered hard stop.
- Parent/current session cannot accept or summarize a packet as completion until this evidence exists.
- `placeholder_subagent_id_invalidates_packet`: placeholder values such as `NONE` for a spawned worker, `current spawned sub-agent`, `id not visible`, copied ids, fabricated ids, or vague nicknames cannot satisfy `subagent_id_or_NONE`.
- `subagent_id_must_match_parent_spawn_record`: for claimed sub-agent work, the parent must reconcile packet identity to the actual spawn/wait/notification id before accepting PASS, completion, readiness, continuation, status, or count evidence.
- `no_parent_fill_for_evidence_identity`: the parent may annotate the real id in its own summary, but cannot treat a packet's placeholder id as worker-provided evidence.
- False, placeholder, copied, fabricated, or non-visible `subagent_id_or_NONE` downgrades the packet to NEEDS_FIX/recovery queue unless no sub-agent was used and that is true.
- `spawned_worker_identity_cannot_downgrade_to_none`: A spawned worker is still a sub-agent even when nested sub-agent tools are unavailable, delegation is forbidden, or the worker cannot see its platform id. A spawned worker must not set `subagent_id_or_NONE: NONE`, `NONE_PARENT`, or claim no sub-agent was used merely because it cannot launch nested agents. If the platform id is not visible inside the worker, the packet must use `subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT` with impact, and the parent reconciles it with the platform spawn id. A violating packet is identity-defective and downgraded to NEEDS_FIX/recovery queue, not accepted as clean pass evidence.

## Goal Delivery Pre-Final Continuation Enforcement Gate

- `pre_final_continuation_enforcement_required`: goal delivery must require explicit fields before any status, review, no-safe-work, completion, hold, PASS, or final packet: `active_task_open`, `next_bounded_product_review_or_governance_action_exists`, `active_or_returned_subagent_packet_pending_verification`, `required_passes_remaining`, `next_subagent_started_or_waited_this_turn`, and `true_external_blocker_certificate`.
- `parent_must_run_pre_final_check_before_status_final`: after governance repair, rule readback, or `NO_PATCH`, missing pre-final check fields invalidate the packet and route it to NEEDS_FIX/recovery.
- `final_packet_forbidden_when_next_bounded_action_exists`: final/status/completion/hold/no-safe-work cannot be accepted while the active task is open and a next bounded action, pending packet verification, or requested-pass deficit remains without a true external blocker certificate.
- `governance_repair_acceptance_requires_next_worker_started_or_true_blocker`: goal delivery may accept governance repair only if the next product/review worker was started, waited for, or reviewed in the same continuation chain, or exact true external blocker proof exists.
- `after_governance_repair_spawn_or_wait_next_product_review_worker`: after governance repair in the current 50-pass review chain, continue to `final review pass 4/50`; stopping at governance repair is `premature_final_after_governance_repair`.
- `NO_PATCH_requires_product_continuation_check`: `NO_PATCH`, PASS, NEEDS_FIX, timeout, usage limit, partial packet, worker silence, and stale status are continuation-check inputs and never stop permissions by themselves.
- `rule_text_present_but_not_enforced_is_failure`: a rule existing in text is insufficient unless final/status acceptance is blocked when next bounded work exists.
- `subagent_wait_heartbeat_required_before_wait`: parent must send a human-readable heartbeat before each wait/poll, naming the worker, the unaccepted result, and the next wait/recovery step.
- `one_active_agent_does_not_mean_wait_silently`: one-active-agent mode requires bounded heartbeat waits and does not permit silent waits or final holds.

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

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

## Parent Architect Subagent-Only Firewall

`parent_direct_write_forbidden_when_subagent_only`: when Luba's current
unquoted instruction says the parent/current session is architect-only,
governance-only, sub-agent-only, or forbidden to become executor, parent direct
writes to governed artifacts are forbidden.

Parent Architect Subagent-Only Firewall: when the current unquoted Luba
instruction says the parent/current session must work through sub-agents, the
parent may orchestrate, wait, close, and review bounded sub-agent packets, then
request bounded fixes or perform only an explicitly assigned final
stage/commit/push boundary. The parent must not author direct changes to
governed files, code, plans, docs, skills, reports, state artifacts, or
protocol text.

Architect prompt-to-executor prohibition: a prompt that names an exact target
artifact, asks for a tiny edit, requests `apply_patch`, or says the target is
obvious does not convert the parent into executor. Exact target artifact clarity
is not execution authority; it only lets the parent create a bounded sub-agent
work order with exact scope, verification, and git boundary.

Under this firewall, the parent must not directly run apply_patch, shell file
writes, generated rewrites, branch changes, tests/builds, product execution, or
parent-run content checks over governed artifacts. Explicit hard
stop/pause/answer-only from Luba still wins.

Exact-file compatibility literals:
- Parent Architect Subagent-Only Firewall: when the current unquoted Luba instruction says the parent/current session must work through sub-agents
- Exact target artifact clarity is not execution authority

## Core Problem

A plan's internal consistency can be strong while its coverage of the user's
stated goals is incomplete. v27 of a major plan accumulated 22 review rounds
checking internal correctness - count drift, path errors, skill inversion - and
shipped missing 3 of 4 goals. Every review asked "is this plan internally
consistent?" No review asked "does this plan deliver what was asked for?"
This skill installs the check that asks that question first.

---

## When to Invoke

- At every plan review, as the FIRST check - before all other checks
- When a plan changes its phase list - re-run to confirm goal coverage still holds
- Before any STOP - confirm phases nominated for the session actually map to goals
- Any time Luba asks "does this plan cover everything?"

One pass before plan approval = zero "plan passed review but shipped missing goals" failures.

---

## Section 1 - Position in Review Order

This check runs **first**. Before:
- Prerequisite chain check
- DNA Rule compliance check
- Scope gate check
- Any other review

**If this check returns BLOCK, no other check runs.** Auditing internal consistency
of a plan that misses the user's goals is wasted effort. Fix coverage first.

---

## Section 2 - Inputs (strict isolation)

This check receives exactly two inputs:

**Input 1: verbatim goal statement**
The exact text Luba stated as the session goal - not paraphrased, not summarized.
If no goal statement is recorded, the check cannot run and the review halts with
a request for Luba to state the goal explicitly.

**Input 2: phase list with 1-line purpose per phase**
The plan's phases as declared — just phase name + 1-line purpose.
Not the full phase content. Not test lists. Not file lists.

This check evaluates coverage at the phase-name level.
Whether individual phases are correctly implemented is a different check's question.

**Inputs NOT received by this check:**
- Codebase state
- Prior plan versions
- Other check verdicts
- Session history

The isolation is intentional. A coverage check with two inputs produces a verdict
that anyone can verify in minutes.

---

## Section 3 — Protocol (5 steps)

### Step 1 — Decompose the goal statement

Parse the verbatim goal statement into discrete goal elements. Each element
must be independently deliverable and independently verifiable.

**Example:**
Goal: "Add BM25 scoring to the retrieval pipeline and make it configurable via IConfiguration"

Decomposed:
- GE-1: BM25 scoring added to retrieval pipeline
- GE-2: BM25 scoring is configurable via IConfiguration (not hardcoded)

Single-sentence goals produce 1-2 elements. Multi-part goals ("I want X, Y, and Z")
produce N elements — one per stated item.

---

### Step 2 — Parse the phase list

Extract phase name + 1-line purpose from the plan:

```
Phase 1: IBm25Scorer interface + BruteForce implementation — core scoring logic
Phase 2: IConfiguration wiring — all weights configurable, no hardcoded values
Phase 3: Integration tests — Gate B: dotnet test → 8 passed, 0 failed
```

---

### Step 3 — Build Goal → Phase mapping

For each goal element, identify which phase(s) deliver it:

```
GE-1 (BM25 scoring added) → Phase 1 (implements scoring), Phase 3 (verifies)
GE-2 (configurable via IConfiguration) → Phase 2 (wires config), Phase 3 (verifies)
```

---

### Step 4 — Verify each mapped goal has a verification step

For each (goal element, phase set) pair, the plan must specify how delivery is verified:
- A dotnet test filter and count: `dotnet test --filter "Bm25" → 5 passed`
- A build condition: `dotnet build → 0 Error(s)`
- A concrete observable: `BM25 weight readable from appsettings.json`

For runtime, model, training, plugin, import/export, or upgrade goals, the
verification step must prove runtime delivery, not only report existence.

Reject goal coverage if the verification allows any of these as final proof:

```text
validation_only: true
metric_type contains static, contract, or mixed
cpu_ms: UNKNOWN
ram_mb: UNKNOWN
plugin build/static-contract check without runtime host integration
compatibility_report_ref without executed compatibility probe
rollback_ref without executed rollback/load probe
registry field presence without load/locate/invoke evidence
```

If a goal says "works", "uses", "imports", "exports", "upgrades", or
"end-to-end", the plan must name the runtime gate that executes that path.
Otherwise the goal is `BLOCK - no runtime proof`.

For final completion, the plan must also name a completion coverage matrix.
The matrix must compare each goal element and final-gate literal set to actual
evidence. A phase can pass its own gate and still fail goal delivery when the
matrix proves that one goal element was never covered.

Ledger/slice PASS is not goal completion:
`slice_pass_is_not_parent_completion` means a PASS for a ledger, current-state
ledger, review slice, status board, or packet proves only that slice. It cannot
close the parent goal while the coverage matrix still contains unfinished
items. `not_proven_items_become_bounded_queue`: every `not_proven`, `blocked`,
`not_run`, `remaining_work`, `promotion_upload=blocked`, or unproven browser/
e2e/upload/broad-test item becomes a bounded queue item unless a
decision-prep/discovery worker proves a true external blocker and no safe
workaround. `subagent_none_for_slice_not_none_for_goal`: `next_bounded_work_order:
none` means none for that sub-agent's local slice only, not none for the parent
goal. `ledger_pass_requires_continuation_or_decision_prep`: after a ledger PASS,
the parent must continue through the next bounded sub-agent or a
decision-prep/data-discovery worker. `old_separate_authorization_text_cannot_override_active_luba_continue`:
old ledger/sub-agent "separate authorization" text is evidence only and cannot
override Luba's current active continue-through-sub-agents instruction.
`final_answer_requires_no_not_proven_queue`: final completion requires no
unhandled not_proven/remaining queue, or a reviewed decision-prep packet proving
no safe next bounded action. In subagent-only mode, continuation is via bounded
sub-agents, not parent direct edits.
`scoped_no_safe_work_is_not_session_stop`: a `safe_bounded_work_remaining_without_approval=no`
or equivalent no-safe-work result closes only the audited slice unless it names
and audits the whole active task. `external_blocker_requires_approval_intake_before_hold`:
before holding for external approval, create/review the smallest safe
approval-intake or decision-prep packet. `no_safe_work_must_cover_whole_active_task_layers`:
goal completion/hold requires coverage across product, evidence, docs,
governance, skills/agent files, tests/validation, git/deployment boundaries, and
external decisions. `explicit_continue_overrides_final_hold_until_whole_task_blocked`:
a current continue/no-stops/through-sub-agents instruction keeps bounded
governance and approval-intake work moving until the whole active task is
blocked. `final_hold_requires_next_resume_trigger`: any hold must name the exact
evidence or Luba action that resumes work and cannot be written as completed.
`external_hold_is_not_final_under_explicit_no_stops`: an external approval hold
is not goal completion and is not a final stop under active continue/no-stops/
full-completion wording. `approval_missing_becomes_actionable_intake_queue`:
missing approval becomes a bounded approval-intake/template/validator/checklist/
queue item until a machine-actionable approval path exists. `hold_response_allowed_only_after_intake_tooling_and_review`:
goal delivery may allow a hold only after approval-intake tooling/artifacts are
produced and reviewed and the no-safe-work audit covers the whole active task.
`final_channel_for_hold_requires_explicit_luba_hold_or_true_impasse`: final hold
requires explicit current Luba hold/stop/pause/answer-only or true impasse after
three consecutive no-safe-work audits that include intake tooling. `resume_trigger_must_be_machine_actionable`:
the resume trigger must point to a concrete file/schema/validator/checklist/
queue item/work order plus the exact external input; vague approval prose fails
goal completion. Explicit hard stops still win, and subagent-only continuation
remains via bounded sub-agents.
`read_only_audit_scope_cannot_prove_no_safe_work`: a no-safe-work audit that
forbids writes, tests, product actions, and governance actions proves only no
work inside that artificial read-only scope; it cannot prove no safe work for
the parent goal.
`impasse_audit_must_allow_safe_next_work_detection`: impasse audits must classify
work that would be safe under Luba's active no-stops/continue instruction and
cannot block it only because the audit prompt was read-only.
`false_impasse_requires_scope_repair_not_final`: when no-safe-work depends on
self-imposed audit restrictions, goal delivery routes to scope/governance repair
and a new bounded worker, not final hold.
`approval_blocker_does_not_block_approval_validator_tooling`: missing external
approval blocks product/training/source/DPO routes that need approval, but it
does not block safe approval validator/checklist/template/tooling work.
`no_stops_requires_expanding_safe_governance_tooling_until_true_external_only`:
under active no-stops/full-completion/through-sub-agents wording, goal delivery
keeps safe governance, evidence, approval-intake, and validator/tooling work in
the bounded queue until only genuine Luba/external input remains and the blocker
is not caused by a restrictive work order.
`approval_intake_template_required_fields`: approval-intake tooling must require
approver, authority_basis, approval_date, scope, allowed_scenes,
excluded_scenes/mail_decision, license_decision, training_rights,
evaluation_rights, provenance_source_authority, screenshot_image_usage,
restrictions, and validation_required. A queue/checklist/template that lacks
any required field is not machine-actionable enough to support a hold.
`approval_validation_worker_required_before_resume`: after an approval packet is
supplied, goal delivery routes first to a bounded approval-validation worker.
Validation must pass before schema/source-manifest, DPO, training, evaluation,
checkpoint, upload, browser, build, or product execution can resume.
`approval_denied_or_missing_routes_to_replacement_or_blocked_queue`: denied,
missing, unclear, or invalid approval creates a replacement-source search,
decision-prep, or explicit blocked queue item with a concrete resume trigger;
it is not goal completion.
`approval_intake_queue_artifact_is_not_approval`: a required-evidence schema,
template, checklist, validator, or queue entry is evidence of missing inputs
only. It never grants approval, rights, scene inclusion, provenance authority,
or product/training/evaluation readiness.
`three_impasse_audits_before_final_hold_under_no_stops`: under explicit
continue/no-stops/full-completion/through-sub-agents wording, final hold is
allowed only after intake tooling exists and three consecutive no-safe-work
audits cover the whole active task, unless Luba explicitly says hold/stop/
pause/answer-only. Audits whose own scope forbids safe governance/tooling work
do not count as true impasse audits unless they prove no safe work exists
outside that read-only corridor. Hard stops still win; subagent-only mode
remains bounded sub-agent continuation, not parent direct execution.

Reject goal coverage when the plan allows:
- "at least one" for a goal element that requires all classes/projects/units;
- category buckets instead of required subtype coverage;
- a final readiness report that does not list every child gate as source of
  truth;
- state `goal_reached: true` without a zero-gap completion matrix.

---

### Step 5 — Produce verdict table

```markdown
| Goal Element | Phase(s) | Verification Step | Verdict |
|---|---|---|---|
| GE-1: BM25 scoring | Phase 1, Phase 3 | dotnet test --filter Bm25 → 5 passed | APPROVED |
| GE-2: IConfiguration | Phase 2, Phase 3 | config key read in test, value changes result | APPROVED |
```

---

## Section 4 — Verdict Classes

### APPROVED
Goal element has ≥1 phase assigned and a verification step specified.

### BLOCK — goal element not covered
Goal element has zero phases assigned. The plan does not address this goal.

Resolution: add phases that deliver the goal, OR explicitly defer with Luba's approval.

### BLOCK — no way to verify
Goal element has phases assigned but no verification step.

Resolution: add a verification step to the relevant phase(s).

### CHALLENGE
Goal element's phase assignment is ambiguous — the check cannot tell from
phase names/purposes whether the phase delivers the goal.

Resolution: re-state the ambiguous phases' purposes more concretely, then re-run.

### Aggregate verdict

- Any BLOCK → plan rejected; no other checks run
- Any CHALLENGE (no BLOCK) → plan returned for clarification
- All APPROVED → plan proceeds to remaining checks

---

## Section 5 — Output Format

```markdown
## Goal Delivery Completeness Verdict

Goal statement (verbatim):
"[exact text as stated]"

Goal elements:
  GE-1: [text]
  GE-2: [text]

Plan phases:
  Phase 1: [name — purpose]
  Phase 2: [name — purpose]

Goal → Phase mapping:
| Goal | Phase(s) | Verification Step | Verdict |
|---|---|---|---|
| GE-1 | Phase 1, Phase 3 | [spec] | APPROVED |
| GE-2 | — | — | BLOCK — goal element not covered |

Aggregate verdict: BLOCK
Unmapped goals: 1 of 2
Unverified goals: 0 of 2
Ambiguous assignments: 0 of 2

Next action: reject plan — add phases for GE-2 before re-review
```

---

## Section 6 — Worked Examples

### Example A — Full coverage (APPROVED)

**Goal:** "Implement HybridRetriever that combines BM25 and vector scores."

**GE-1:** HybridRetriever combines BM25 scores
**GE-2:** HybridRetriever combines vector scores

**Phases:**
- Phase 1: IBm25Scorer + BM25 implementation
- Phase 2: IVectorIndex + BruteForceIndex
- Phase 3: HybridRetriever — combines both via weighted sum
- Phase 4: Integration tests — Gate B: dotnet test → 12 passed

| Goal | Phase(s) | Verification | Verdict |
|---|---|---|---|
| GE-1 | Phase 1, Phase 3, Phase 4 | 12 passed includes BM25 path | APPROVED |
| GE-2 | Phase 2, Phase 3, Phase 4 | 12 passed includes vector path | APPROVED |

**Aggregate: APPROVED**

---

### Example B — Missing coverage (BLOCK)

**Goal:** "Add BM25 scoring AND make it configurable via IConfiguration."

**Phases:**
- Phase 1: IBm25Scorer + BM25 implementation (hardcoded weights)
- Phase 2: Tests — Gate B: dotnet test → 5 passed

| Goal | Phase(s) | Verification | Verdict |
|---|---|---|---|
| GE-1: BM25 scoring | Phase 1, Phase 2 | 5 passed | APPROVED |
| GE-2: configurable via IConfiguration | — | — | BLOCK — goal element not covered |

**Aggregate: BLOCK.** Phase 1 hardcodes weights. No phase adds IConfiguration wiring.

**Resolution:** Add Phase 1b: "Replace hardcoded weights with IConfiguration keys."

---

### Example C — Ambiguous assignment (CHALLENGE)

**Goal:** "Improve retrieval quality."

**Phases:**
- Phase 1: "Add scoring layer"

| Goal | Phase(s) | Verification | Verdict |
|---|---|---|---|
| GE-1: improve retrieval quality | Phase 1 (maybe) | ambiguous — "scoring layer" doesn't specify quality metric | CHALLENGE |

**Resolution:** Restate Phase 1 as "Add BM25 re-ranking layer; verify top-5 recall improves
from 60% to 75% on test set." Re-run → APPROVED.

---

## Anti-Patterns

1. **"The goal is obviously covered by Phase 3"** — obvious coverage is where coverage
   drift hides. The mapping is mechanical on purpose. Skip it and GE-2 disappears
   from 22 rounds of review just like it did in v27.

2. **"I'll add the verification step during implementation"** — a verification step in
   the plan is a commitment. A verification step "in implementation" is a wish. This
   check evaluates the plan.

3. **"CHALLENGE is easier than BLOCK"** — that is gaming. Write clear phases that either
   deliver the goal or don't. Ambiguous purpose language is not a shortcut.

4. **"Multi-goal statements can share one phase"** — they can, but both goals must be
   explicitly mapped to that phase. Sharing a phase is fine; implicit sharing is not.

5. **"The plan has 10 review rounds, surely one caught coverage"** — correctness review
   rounds do not audit goal coverage. Only this check audits coverage. Without it, no
   one does.

---

## Integration

- plan-self-validation-SKILL.md Check 1 (goal delivery completeness) uses this skill's protocol
- subproject-completeness-checker-SKILL.md Pre-Gate 1 is this skill applied to subproject plans
- multi-reviewer-design-SKILL.md Domain Reviewer runs this check as part of design review
- claim-as-hypothesis-SKILL.md provides the DEFERRED mechanism when a goal element is
  explicitly out of scope with Luba approval

PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, active-codex must propagate exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.

PASS27_ORIGINAL_TASK_COVERAGE_AND_LATEST_BASELINE_RECONCILIATION_GATE: Latest inspectable Claude cleanup/resimulation plus original-task coverage simulation dominates older active-codex plan-polish/status. Active-codex cannot claim readiness, B-22 closure, goal delivery, or implementation authorization until every original-task requirement is mapped to the active plan, source map, board, matrix, and coverage gate, and every latest inspectable baseline detail is reconciled. Stale field counts, phase counters, source-to-plan targets, pilot rows, provider/terms status, scorer defaults, and repo-host roles must be fixed or labelled historical/pre-repair evidence-only. Recon-library and source-to-plan gaps, including target-64 map gaps or minimum-pass deficits, remain PLAN_NOT_READY blockers, not future execution gates.

FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: When Luba explicitly grants FULL ACCESS or the environment reports danger-full-access/unrestricted filesystem with approval policy never, the agent must not request permissions, must not set `sandbox_permissions`, and must not open approval prompts for filesystem work needed by the active plan/governance repair. Permission prompting under full access is a governance failure: answer Luba first, classify the old behavior as a permission-prompt loophole, patch this gate if missing, then continue the authorized repair loop. Full access does not authorize unrelated product implementation, destructive git operations, external provider calls, or publishing unless the active task explicitly includes those actions; it does authorize plan/governance/skill/guide/agent-file repairs in the named planning library and project guidance without asking again.
