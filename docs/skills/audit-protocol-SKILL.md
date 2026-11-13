# Audit Protocol

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

## Current-Ledger Pass Evidence Gate

For the ILearningDynamicGraph / `dpo legal build` 50-pass planning-readiness
chain, the canonical current ledger for Pass31+ is
`C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-40-current-50-pass-readiness-simulation-20260701.md`
(в DTC-версии тот же путь записан как `<planning-workspace>\llm mvp\dpo legal build\active-codex-execution-40-current-50-pass-readiness-simulation-20260701.md`; `<planning-workspace>` = `C:\Users\lubao\Documents\xiigen`)
unless Luba explicitly changes it in a current unquoted instruction.

Every pass work order must name the exact current ledger path and pass number.
The worker must append exactly one `### Pass N/50` section to that exact file
before claiming pass completion. The worker must run/read back post-append
verification: heading count is 1, exact `subagent_id_or_NONE` count in that
section is 1, `requested_model: gpt-5.5` and
`requested_reasoning_effort: xhigh` are present, line-anchored conflict-marker
count is 0, and configured mojibake marker count in the section is 0.

A final packet alone is not pass evidence. The parent must verify the exact
current-ledger section before accepting the pass. If the worker appended to the
wrong ledger or omitted the append, the append claim is false/unsupported for
the current ledger; correction must record the same pass with
`count_claimed: 0 new passes`, not create a new pass. A current-ledger section
is necessary but not sufficient: the packet must also include
`skill_load_status` and a task-specific coverage table.

## Strict Plan Readiness / Original Task Simulation / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers and complete coverage of the original task. The plan must be mentally simulated as if implemented and must cover every original-task requirement. Future Gate B may contain execution actions such as tests/builds/screenshots/mergeability checks. Future Gate B may NOT hide unchosen legal provider, missing input/output contracts, missing dual-path design, unresolved repo ownership, missing external API seam design, missing provenance policy, missing thresholds, missing model/checkpoint lifecycle, or missing source-to-plan mapping. If a future gate hides a design/dependency decision, verdict is PLAN_NOT_READY.

30 sequential simulation/review passes are required for this readiness class: one pass at a time, no parallelism. Each pass records `requested_model: gpt-5.5`, `requested_reasoning_effort: xhigh`, skill focus, original-task requirement checked, plan section checked, mental simulation step, coverage verdict, finding, repair/no-fix reason, and recheck result. A single worker doing 30 internal passes must call them sequential simulation/review passes, not 30 sub-agents or 30 independent sub-agent passes.
Later stricter full-task coverage evidence dominates earlier narrow ready verdicts. If any later sequential simulation, reviewer packet, or attachment reports NOT_READY, zero FULLY coverage, uncovered original-task requirements, or unresolved design/dependency blockers for the same plan, the agent must immediately downgrade current readiness to PLAN_NOT_READY before any implementation or final ready claim. The downgrade must be propagated to active plan artifacts plus the relevant skills/guides/agent files that allowed the stale claim; a plan-only wording patch is incomplete.

PLAN_NOT_READY after full-task coverage simulation is not a final reporting boundary. When implementable design/dependency blockers remain, immediately convert the blocker inventory (for active-codex Pass06 and later: B-1..B-22 unresolved blocker inventory is mandatory; Missing blocker numbers are trace defects, not permission to drop them; older B1-B21/B5-absent evidence must become explicit trace-repair backlog, not a dropped row) into sequential bounded sub-agent plan-repair passes and continue until the plan reaches readiness or a true external blocker remains after mitigation/decision-prep. A final answer after only downgrading readiness is a protocol violation unless all implementable blockers have been repaired, delegated to active workers, or proven external/non-actionable. Minimum-pass requirements are evidence-bound: if Luba requires 50 passes with sub-agents one-at-a-time, run or receive 50 inspectable sequential sub-agent packets/records before claiming 50; no parallelism and no ledger-only inflation. Governance/skill repair must precede plan repair when the stop came from a governance loophole, but governance repair is not itself a completion boundary.

Readiness-repair work orders must name the skills, source attachments, active files, original-task requirements, good/bad examples, pass-count and evidence rules, verification commands, and explicit propagation targets. Attachment-reported simulations are evidence only; do not claim the worker ran those passes unless the worker actually ran or received independently inspectable records for them. Old PLAN_READY_FOR_LUBA_REVIEW may remain only as historical/pre-attachment/narrower-repair text with current PLAN_NOT_READY nearby.

Future gates are execution-continuation checkpoints, not stop permissions. After Luba approves a truly ready plan, gates are executed through bounded sub-agents until the product is complete. If a gate fails, the next bounded sub-agent fixes/mitigates or records a true external blocker. Do not stop/finalize while safe bounded work remains.

Any historical/pre-repair/evidence-only `NOT_READY`, `BLOCKED`, `FAILED`, `READY`, `DONE`, or similar status retained as evidence must be labelled historical/pre-repair/evidence-only, with the current verdict nearby. If a current artifact still makes Luba ask whether stale `NOT_READY` remains, the repair is incomplete.

Good future gate: `Run dotnet tests after implementation; if red, bounded fix worker repairs failing slice.` This is execution, not a design decision. Bad future gate: `Choose legal provider during implementation` or `figure out FLOW-38 input contract later`; these are design blockers, not execution gates. Good historical status: `Attachment 429f reported the pre-repair plan as NOT_READY; current verdict after repair is PLAN_NOT_READY because provider remains unchosen.` Bad historical status: leaving `NOT_READY` in a source map without saying historical/pre-repair and without current verdict next to it.

## Claude Baseline Plan Authority Gate

Execution cannot start until active-codex contains all Claude baseline details, references, and explanations plus only validated Codex deltas.
Claude baseline defines repo scope unless Luba explicitly expands it.
Discovered/copied workdirs are not repo authority.
50-pass plan authority review requires 50 independently inspectable evidence records before claiming completion.
Count-index artifacts are snapshots, not live counters. Before any final/status
answer or audit packet claims a current 50-pass or 20-fix/repair count, audit
must recompute accepted-current count from the latest accepted pass artifacts
and parent acceptance evidence. Refresh or create a count-index artifact at
cadence checkpoints: every 5 accepted main passes, after any change to the
fix/repair classification basis, or when an answer would otherwise present the
latest index as current without a recomputed-current disclosure. If the current
count has advanced since the latest count-index artifact and no cadence artifact
update is due, audit must report both `latest-index snapshot` and `recomputed
accepted-current`. Recovery-only passes, ledger rows, checklists, summaries,
and parent/self checks add 0 unless accepted as independent main pass artifacts
with canonical identity, verdict, skill-load evidence, and parent acceptance. No
50/20 completion claim is valid without 50 accepted main artifacts and 20
accepted fix/repair artifacts.
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

## Skill Load Evidence Gate

- `skill_load_status_required_in_every_subagent_packet`: planning/review/architecture/governance/status/continuation/readiness packets must include `skill_load_status` with found/read/missing for `AGENTS.md` and required skills/guides.
- `missing_skill_load_status_invalidates_packet`: PASS, READY, no-safe-work, completion, hold, continuation, and readiness packets lacking required `skill_load_status` are invalid evidence and become NEEDS_FIX/recovery queue.
- `quoted_or_workorder_skill_list_is_not_load_evidence`: quoted transcript/work-order checklist/prompt list is not evidence that skills were loaded/read.
- Missing, unread, or inaccessible required skills/guides route to governance repair before product work unless Luba explicitly ordered hard stop.
- Parent/current session cannot accept or summarize packet as completion until this evidence exists.

## Sub-Agent Identity Evidence Gate

- `placeholder_subagent_id_invalidates_packet`: placeholder values such as `NONE` for a spawned worker, `current spawned sub-agent`, `id not visible`, copied ids, fabricated ids, or vague nicknames cannot satisfy `subagent_id_or_NONE`.
- `subagent_id_must_match_parent_spawn_record`: for claimed sub-agent work, parent must reconcile packet identity to actual spawn/wait/notification id before accepting audit PASS, completion, readiness, continuation, status, or count evidence.
- `no_parent_fill_for_evidence_identity`: parent may annotate real id in summary but cannot treat a worker placeholder id as worker-provided evidence.
- False, placeholder, copied, fabricated, or non-visible identity evidence downgrades the packet to NEEDS_FIX/recovery queue unless no sub-agent was used and that is true.
- `spawned_worker_identity_cannot_downgrade_to_none`: A spawned worker is still a sub-agent even when nested sub-agent tools are unavailable, delegation is forbidden, or the worker cannot see its platform id. A spawned worker must not set `subagent_id_or_NONE: NONE`, `NONE_PARENT`, or claim no sub-agent was used merely because it cannot launch nested agents. If the platform id is not visible inside the worker, the packet must use `subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT` with impact, and the parent reconciles it with the platform spawn id. A violating packet is identity-defective and downgraded to NEEDS_FIX/recovery queue, not accepted as clean pass evidence.
- `pass109_spawned_worker_identity_pre_return_self_check`: every spawned-worker work order must require `subagent_id_or_NONE: <platform id if visible, otherwise UNKNOWN_IN_SUBAGENT>` plus `subagent_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true`. Before return, the worker must search/read back its packet and any written evidence file for `subagent_id_or_NONE`; if it finds `subagent_id_or_NONE: NONE`, `subagent_id_or_NONE=NONE`, `NONE_PARENT`, `current-thread worker`, or `id not visible`, it must correct the packet before return. Audit must reject an uncorrected spawned-worker packet as identity-defective evidence-only and count it as 0 passes/reviews/cycles.

## Audit Pre-Final Continuation Enforcement Gate

- `pre_final_continuation_enforcement_required`: audit must require pre-final fields before accepting any status, review, no-safe-work, completion, hold, PASS, or final packet: `active_task_open`, `next_bounded_product_review_or_governance_action_exists`, `active_or_returned_subagent_packet_pending_verification`, `required_passes_remaining`, `next_subagent_started_or_waited_this_turn`, and `true_external_blocker_certificate`.
- `parent_must_run_pre_final_check_before_status_final`: an audit/status/final after governance repair, rule readback, or `NO_PATCH` that lacks the pre-final check is invalid evidence and routes to NEEDS_FIX/recovery.
- `final_packet_forbidden_when_next_bounded_action_exists`: audit must reject final/status/completion/hold/no-safe-work if active task is open and next bounded work, pending packet verification, or pass deficit remains without exact true external blocker proof.
- `governance_repair_acceptance_requires_next_worker_started_or_true_blocker`: audit may accept governance repair only with same-chain proof that the next product/review worker was started, waited for, or reviewed, or with a true external blocker certificate.
- `after_governance_repair_spawn_or_wait_next_product_review_worker`: after governance repair in the current 50-pass review chain, audit routes to `final review pass 4/50`; stopping at governance repair is `premature_final_after_governance_repair`.
- `NO_PATCH_requires_product_continuation_check`: `NO_PATCH`, PASS, NEEDS_FIX, timeout, usage limit, partial packet, worker silence, and stale status must be audited as continuation-check inputs, not stop permissions.
- `rule_text_present_but_not_enforced_is_failure`: existing rule text is not audit closure unless the audit blocks final/status acceptance when the check shows next bounded work.
- `subagent_wait_heartbeat_required_before_wait`: each wait/poll must be preceded by a human-readable heartbeat that names the worker, the not-yet-accepted result, and next wait/recovery step.
- `one_active_agent_does_not_mean_wait_silently`: one-active-agent sequencing requires bounded heartbeat waits; silent waits fail audit.

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

An ad-hoc audit varies by reviewer and time available. One reviewer checks
chain arithmetic and skips blast radius. Another checks DNA compliance but skips
API shapes. The result is findings that depend on who ran the audit, not on what
the plan contains. This skill defines a 10-point checklist that every audit runs
against, with severity tags that are consistent and non-negotiable.

---

## When to Invoke

- During any plan review (plan-review-SKILL.md Gate B)
- Before declaring any plan "ready for execution"
- When reviewing another agent's output or plan
- After a multi-phase plan's Phase 0 validation

---

## Section 1 — 10-Point Audit Checklist

Every audit must explicitly address all 10 points. Points not checked must be
listed in a "NOT CHECKED" section with reason.

| # | Check | Governing Skill | Pass Condition |
|---|---|---|---|
| 1 | **Chain arithmetic** | chain-arithmetic-audit-SKILL.md | baseline + Σ(deltas) = final for every metric; no magic number jumps |
| 2 | **Blast radius tagging** | blast-radius-tagger-SKILL.md | every deliverable tagged TEST/GOVERNANCE/PRODUCTION/INTERFACE; INTERFACE changes have rollback plan |
| 3 | **API shape verification** | api-shape-verification-SKILL.md | all imports have verified export shape, parameter types, return types |
| 4 | **Commit atomicity** | blast-radius-tagger-SKILL.md | GOVERNANCE and PRODUCTION files committed before TEST-ONLY; no orphaned partial commits |
| 5 | **Write-time fix verifiability** | — | every write-time fix has a specific grep/dotnet test command that confirms it landed |
| 6 | **Path verification** | — | every file path verified against actual codebase (not assumed from plan context) |
| 7 | **Gate thresholds** | plan-review-SKILL.md | every pass/fail threshold is explicit — "dotnet test → 12 passed", never "~approximately passes" |
| 8 | **Severity assignment** | — | every finding tagged: BLOCKING / WRITE-TIME / INFORMATIONAL |
| 9 | **Severity consistency** | — | no finding tagged BLOCKING may resolve as "minor" in same review |
| 10 | **Coverage declaration** | — | audit output ends with explicit list of what was NOT checked and why |

### Mandatory human-readable architecture audit add-on

When the audit touches architecture, trainable behavior, code review,
documentation, DPO, checkpoints, manifests, KNN, dictionaries, counters,
scorers, selectors, routers, or private decision methods, the audit must also
run the Human-Readable Architecture Artifact Gate and Concrete Repair Blueprint
Gate from `human-readable-architecture-audit-SKILL.md`.

Required audit evidence:

```text
Russian human summary present:
PlantUML or Mermaid UML importable to draw.io:
data-flow simulations and expected branch outcomes present:
no-code algorithm explanation present:
exact code quote + adjacent concrete solution present:
schema-only/table-only/prompt-spec solution absent:
Concrete Repair Blueprint Gate filled:
```

Generic rows such as "create schema", "chosen/rejected/quarantine", "add KNN",
"wrap in ILogicalUnit", or "make typed state" не является решением.

When Luba explicitly asks for iterative audit readability improvement, an audit
cannot pass before minimum 6 cycles, preferably 10. Each cycle records which
skill/gate was applied, what failed, what was fixed, and why the artifact became
more human-readable. A discovered skill/gate gap returns the work to governance
repair before artifact polishing.

### Review/Fix Pass Evidence Add-On

If the audit or work order requires at least N review/fix passes, the audit must
verify N distinct evidence packets. A Review-Fix Ledger is only an index.

Required proof:

```text
requested_pass_floor:
real_pass_count:
evidence_packet_count:
ledger_row_count:
ledger_rows_are_evidence=false:
artifact_version_order_proof:
pass_1_packet_ref:
...
pass_N_packet_ref:
```

Each packet must prove artifact order: the pass was run after the previous
fix/no-fix recheck, and it includes before/after refs, reviewer/sub-agent id,
loaded skills/gates, findings, fix or no-fix justification, and recheck. If the
audit sees N ledger rows but fewer than N valid packets, verdict is
`IN_PROGRESS_NEXT_STEP_KNOWN`, not pass.

### Final Hold / Approval Intake Audit Add-On

Before an audit accepts "no safe work remains", `safe_bounded_work_remaining_without_approval=no`,
`next_bounded_work_order: none`, external approval blocker, or final hold
wording, it must verify these literals:

```text
scoped_no_safe_work_is_not_session_stop
external_blocker_requires_approval_intake_before_hold
no_safe_work_must_cover_whole_active_task_layers
explicit_continue_overrides_final_hold_until_whole_task_blocked
final_hold_requires_next_resume_trigger
external_hold_is_not_final_under_explicit_no_stops
approval_missing_becomes_actionable_intake_queue
hold_response_allowed_only_after_intake_tooling_and_review
final_channel_for_hold_requires_explicit_luba_hold_or_true_impasse
resume_trigger_must_be_machine_actionable
read_only_audit_scope_cannot_prove_no_safe_work
impasse_audit_must_allow_safe_next_work_detection
false_impasse_requires_scope_repair_not_final
approval_blocker_does_not_block_approval_validator_tooling
no_stops_requires_expanding_safe_governance_tooling_until_true_external_only
approval_intake_template_required_fields
approval_validation_worker_required_before_resume
approval_denied_or_missing_routes_to_replacement_or_blocked_queue
approval_intake_queue_artifact_is_not_approval
three_impasse_audits_before_final_hold_under_no_stops
```

Hard fail when a scoped no-safe-work or missing-approval packet is treated as a
parent final stop under active continue/no-stops/full-completion wording.
Missing approval is actionable work until a machine-actionable approval packet
template, schema, validator, checklist, intake queue, or post-approval work
order exists and is reviewed. A hold response is allowed only after that intake
tooling/artifact has been produced and a whole-active-task no-safe-work audit
covers product, evidence, docs, governance, skills/agent files, tests/
validation, git/deployment boundaries, and external decisions. Final-channel
hold requires explicit current Luba hold/stop/pause/answer-only or true impasse
after three consecutive no-safe-work audits that include intake tooling. Any
resume trigger must name a concrete file/schema/validator/checklist/queue item/
work order plus exact external input, not only prose. Explicit hard stops still
win; subagent-only mode still forbids parent direct governed-artifact writes.
`read_only_audit_scope_cannot_prove_no_safe_work` means a no-safe-work audit
whose own work order forbids writes, tests, product actions, and governance
actions proves only the read-only corridor, not whole-task impasse.
`impasse_audit_must_allow_safe_next_work_detection` means the audit must look
for safe work Luba's active no-stops/continue instruction authorizes, not only
work allowed by the narrower audit prompt.
`false_impasse_requires_scope_repair_not_final` means self-imposed audit
restrictions route to scope/governance repair and a new bounded worker, not
final hold.
`approval_blocker_does_not_block_approval_validator_tooling` means missing
external approval blocks approval-dependent product/source/training/DPO routes,
but not safe approval validator, checklist, template, queue, or tooling work.
`no_stops_requires_expanding_safe_governance_tooling_until_true_external_only`
means active no-stops/continue-to-completion wording keeps safe governance,
evidence, approval-intake, and validator/tooling work moving until only genuine
Luba/external input remains and the blocker is not caused by a restrictive work
order.
The intake artifact must satisfy `approval_intake_template_required_fields` by
requiring approver, authority_basis, approval_date, scope, allowed_scenes,
excluded_scenes/mail_decision, license_decision, training_rights,
evaluation_rights, provenance_source_authority, screenshot_image_usage,
restrictions, and validation_required. If an approval packet is supplied,
`approval_validation_worker_required_before_resume` means the next action is a
bounded approval-validation worker, not direct schema/source-manifest, DPO,
training, evaluation, checkpoint, upload, browser, build, or product execution.
`approval_denied_or_missing_routes_to_replacement_or_blocked_queue` sends
denied, missing, unclear, or invalid approval to replacement-source search,
decision-prep, or explicit blocked queue. `approval_intake_queue_artifact_is_not_approval`
means templates, checklists, validators, queue items, and required-evidence
schemas never grant rights or readiness. `three_impasse_audits_before_final_hold_under_no_stops`
requires three consecutive no-safe-work audits after intake tooling exists and
after all active-task layers are covered, unless Luba explicitly says hold/
stop/pause/answer-only. Read-only corridor audits do not count unless they also
prove no safe work exists outside their self-imposed restrictions.

### Audit Sufficiency Boundary Gate

Architecture/trainable audits must not claim broader completion than they
checked. "v1 scope completed" is allowed only with the exact bounded scope.
"Architecture audit sufficient/complete", "достаточно", "готово", or "закрыто"
is forbidden unless the breadth challenge pass is green.

Minimum breadth challenge after focused findings:

```text
confidence claims and confidence floors checked:
export accuracy claims checked:
target-label leakage checked:
runtime routing confidence floors checked:
source/path/provenance leakage checked:
fallback-to-first-entry/default-candidate shortcuts checked:
bounded completion wording used:
```

Parent review must ask and answer:

```text
What would a broader audit likely find that this artifact missed?
```

The answer must name likely missed zones, whether they were checked, and which
claims remain explicitly out-of-scope.

### Delta-Severity Feedback Gate

If the next audit in the same architecture area finds BLOCKING/HIGH issues that
should have been discoverable, the previous audit/review has a retroactive
review-skill failure unless the missed area was explicitly out-of-scope.

Required feedback record:

```text
previous audit/review claim:
later BLOCKING/HIGH issue:
discoverable evidence path:
why previous review missed it:
skill/guide/index/agent rule patched:
new breadth challenge row:
corrected completion wording:
```

Run governance repair before accepting or polishing the next audit artifact.

---

## Section 2 — Severity Definitions

| Tag | Meaning | Required Action |
|---|---|---|
| **BLOCKING** | Execution cannot proceed without fixing this | Stop, fix, re-audit before any Phase 1 work |
| **WRITE-TIME** | Fix during execution, pre-approved | Document as WF-N entry with verification command |
| **INFORMATIONAL** | Noted, no action required | Log only, no gate impact |

### Severity Rules (non-negotiable)

1. Every finding has exactly ONE severity tag
2. A finding tagged BLOCKING that resolves as "minor" is a protocol violation — reclassify or escalate
3. No finding may downgrade from BLOCKING to WRITE-TIME without Luba approval
4. A plan with any BLOCKING finding cannot proceed to Phase 1

---

## Section 3 — Audit Output Format

```markdown
## Audit Report — [Plan Name or Session ID]
Date: [date]
Auditor: [session]
Plan version: [N]

### Section A — Findings

| # | Point | Status | Severity | Finding |
|---|---|---|---|---|
| 1 | Chain arithmetic | PASS | — | baseline 916 + 12 = 928; final declared 928 ✓ |
| 2 | Blast radius | PASS | — | all 4 deliverables tagged |
| 3 | API shape | FAIL | BLOCKING | IBm25Scorer.Score() returns OperationResult<float[]> in plan, but interface declares float — mismatch |
| 4 | Commit atomicity | PASS | — | GOVERNANCE commit before TEST-ONLY commit ✓ |
| 5 | WF verifiability | PASS | — | all 3 write-time fixes have grep verification commands |
| 6 | Path verification | FAIL | WRITE-TIME | src/AgentAlgorithms/Bm25Scorer.cs — file listed but not confirmed to exist |
| 7 | Gate thresholds | PASS | — | all Gate B conditions have exact counts |
| 8 | Severity assignment | PASS | — | 2 BLOCKING, 1 WRITE-TIME, 0 INFORMATIONAL |
| 9 | Severity consistency | PASS | — | no downgrade instances found |
| 10 | Coverage declaration | see below | — | |

### Section B — NOT CHECKED (with reason)

| Point | What was not checked | Reason |
|---|---|---|
| 3 (partial) | IBm25Scorer constructor injection test in 22.AgentAlgorithms.Tests | Test file does not exist yet; cannot verify shape until written |

### Final Gate

BLOCKING findings: 1
→ Plan cannot proceed to Phase 1 until API shape mismatch (Point 3) is resolved.
Resolution required: verify IBm25Scorer.Score() return type in interface vs plan.

WRITE-TIME findings: 1
→ WF-1: verify src/AgentAlgorithms/Bm25Scorer.cs exists before writing to it
   Verification command: ls src/AgentAlgorithms/Bm25Scorer.cs

Verdict: NOT READY — 1 BLOCKING finding requires resolution
```

---

## Anti-Patterns

1. **"The audit found no issues"** — an audit with no INFORMATIONAL findings has
   either 10 points that all genuinely passed, or some points were not checked.
   If no points were checked, the audit is empty. Declare what was checked.

2. **"This issue is minor but I tagged it BLOCKING"** — if a finding is minor,
   tag it INFORMATIONAL. BLOCKING means execution stops. Do not inflate severity.

3. **"The 'NOT CHECKED' section is embarrassing so I skip it"** — an audit without
   a NOT CHECKED section implies all 10 points were checked. If they were not,
   the audit is misrepresenting its coverage. Declare omissions.

4. **"Gate thresholds don't need counts — 'tests pass' is clear enough"** — Point 7
   requires exact counts. "Tests pass" leaves the question "which tests?" and "how many?"
   unanswered. A Gate B that cannot be verified by a third party is not a Gate B.

5. **"BLOCKING finding resolved itself after I re-read"** — a BLOCKING finding that
   was declared and then retracted without a specific code change is a consistency
   violation. Either the finding was wrong (downgrade to INFORMATIONAL with reason)
   or the finding is still valid (remain BLOCKING).

---

## Integration

- plan-review-SKILL.md Gate B invokes this audit protocol
- chain-arithmetic-audit-SKILL.md governs Point 1 (chain arithmetic)
- blast-radius-tagger-SKILL.md governs Points 2 and 4 (blast radius + commit atomicity)
- api-shape-verification-SKILL.md governs Point 3 (API shapes)
- plan-self-validation-SKILL.md Phase 0 check is a prerequisite to a complete audit
- human-readable-architecture-audit-SKILL.md governs Human-Readable,
  Concrete Repair Blueprint, Audit Sufficiency Boundary, and Delta-Severity
  Feedback gates for architecture/trainable/code audits

PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, active-codex must propagate exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.

PASS27_ORIGINAL_TASK_COVERAGE_AND_LATEST_BASELINE_RECONCILIATION_GATE: Audit must downgrade readiness/B-22 closure/implementation authorization when latest inspectable Claude cleanup/resimulation plus original-task coverage simulation has not been reconciled into active-codex. Every original-task requirement must map to the active plan, source map, board, matrix, and coverage gate, and every latest inspectable baseline detail must be reconciled. Stale field counts, phase counters, source-to-plan targets, pilot rows, provider/terms status, scorer defaults, and repo-host roles must be fixed or labelled historical/pre-repair evidence-only. Recon-library and source-to-plan gaps, including target-64 map gaps or minimum-pass deficits, remain PLAN_NOT_READY blockers, not future execution gates.

FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: When Luba explicitly grants FULL ACCESS or the environment reports danger-full-access/unrestricted filesystem with approval policy never, the agent must not request permissions, must not set `sandbox_permissions`, and must not open approval prompts for filesystem work needed by the active plan/governance repair. Permission prompting under full access is a governance failure: answer Luba first, classify the old behavior as a permission-prompt loophole, patch this gate if missing, then continue the authorized repair loop. Full access does not authorize unrelated product implementation, destructive git operations, external provider calls, or publishing unless the active task explicitly includes those actions; it does authorize plan/governance/skill/guide/agent-file repairs in the named planning library and project guidance without asking again.
FULL_ACCESS_EXACT_CLEANUP_NO_APPROVAL_SUBGATE: Under full access or approval policy never, the agent must not use `sandbox_permissions`, approval prompts, or user-permission questions for authorized filesystem/governance cleanup. `approval prompt under full access/approval never is a governance failure`: answer Luba first, classify the old behavior as a permission-prompt loophole, repair the governing rule, verify the repair, and then resume the nearest authorized work. `exact stale-process/temp cleanup` is allowed and required when bounded by exact PIDs/process names/path proof tied to the current work chain, for example stopping exact stale `git.exe` PIDs created by the active temp-copy attempt and deleting the exact `.codex-*` temp path created by that attempt. `Stop-Process is not automatically an approval boundary under full access`; it is a bounded cleanup action when exact PIDs and the cleanup path are tied to the active work chain. If process ownership cannot be proven, record the exact blocker and perform read-only evidence only; still do not open an approval prompt under approval policy never. Parent/direct command cleanup is allowed for exact stale temp/process cleanup only after the subagent cleanup path repeatedly fails or when approval prompts are forbidden by the environment; it must record before/after proof and must not broaden scope.
