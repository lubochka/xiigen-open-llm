# Reconnaissance Gate

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the equivalent in the operator's language), challenges an answer, or says a failure came
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

## Reconnaissance Pre-Final Continuation Enforcement Gate

- `pre_final_continuation_enforcement_required`: reconnaissance/no-safe-work review must require pre-final fields before status, review, no-safe-work, completion, hold, PASS, or final wording: `active_task_open`, `next_bounded_product_review_or_governance_action_exists`, `active_or_returned_subagent_packet_pending_verification`, `required_passes_remaining`, `next_subagent_started_or_waited_this_turn`, and `true_external_blocker_certificate`.
- `parent_must_run_pre_final_check_before_status_final`: after governance repair, rule readback, or `NO_PATCH`, missing pre-final fields invalidate status/final evidence.
- `final_packet_forbidden_when_next_bounded_action_exists`: final/status/completion/hold/no-safe-work is forbidden while next bounded work, pending packet verification, or pass deficit exists without exact true external blocker proof.
- `governance_repair_acceptance_requires_next_worker_started_or_true_blocker`: governance repair can be accepted only with same-chain proof of next product/review worker start, wait, or review, or a true external blocker certificate.
- `after_governance_repair_spawn_or_wait_next_product_review_worker`: for the current 50-pass review chain, route to `final review pass 4/50`; stopping after governance repair is `premature_final_after_governance_repair`.
- `NO_PATCH_requires_product_continuation_check`: `NO_PATCH`, PASS, NEEDS_FIX, timeout, usage limit, partial packet, worker silence, and stale status trigger continuation/recovery, not stop permission.
- `rule_text_present_but_not_enforced_is_failure`: rule text is not enforcement unless final/status acceptance is blocked when next bounded action exists.
- `subagent_wait_heartbeat_required_before_wait`: every wait/poll requires a prior human-readable heartbeat naming the worker, unaccepted result, and next wait/recovery step.
- `one_active_agent_does_not_mean_wait_silently`: one-active-agent mode requires bounded heartbeat waits and does not permit silent waiting or final hold.

## Strict Plan Readiness / Original Task Simulation / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers and complete coverage of the original task. The plan must be mentally simulated as if implemented and must cover every original-task requirement. Future Gate B may contain execution actions such as tests/builds/screenshots/mergeability checks. Future Gate B may NOT hide unchosen legal provider, missing input/output contracts, missing dual-path design, unresolved repo ownership, missing external API seam design, missing provenance policy, missing thresholds, missing model/checkpoint lifecycle, or missing source-to-plan mapping. If a future gate hides a design/dependency decision, verdict is PLAN_NOT_READY.

30 sequential simulation/review passes are required for this readiness class: one pass at a time, no parallelism. Each pass records `requested_model: gpt-5.5`, `requested_reasoning_effort: xhigh`, skill focus, original-task requirement checked, plan section checked, mental simulation step, coverage verdict, finding, repair/no-fix reason, and recheck result. A single worker doing 30 internal passes must call them sequential simulation/review passes, not 30 sub-agents or 30 independent sub-agent passes.
Later stricter full-task coverage evidence dominates earlier narrow ready verdicts. If any later sequential simulation, reviewer packet, or attachment reports NOT_READY, zero FULLY coverage, uncovered original-task requirements, or unresolved design/dependency blockers for the same plan, the agent must immediately downgrade current readiness to PLAN_NOT_READY before any implementation or final ready claim. The downgrade must be propagated to active plan artifacts plus the relevant skills/guides/agent files that allowed the stale claim; a plan-only wording patch is incomplete.

PLAN_NOT_READY after full-task coverage simulation is not a final reporting boundary. When implementable design/dependency blockers remain, immediately convert the blocker inventory (for active-codex attachment 0ce0: B1-B21, preserving absent B5 if applicable) into sequential bounded sub-agent plan-repair passes and continue until the plan reaches readiness or a true external blocker remains after mitigation/decision-prep. A final answer after only downgrading readiness is a protocol violation unless all implementable blockers have been repaired, delegated to active workers, or proven external/non-actionable. Minimum-pass requirements are evidence-bound: if Luba requires 50 passes with sub-agents one-at-a-time, run or receive 50 inspectable sequential sub-agent packets/records before claiming 50; no parallelism and no ledger-only inflation. Governance/skill repair must precede plan repair when the stop came from a governance loophole, but governance repair is not itself a completion boundary.

Readiness-repair work orders must name the skills, source attachments, active files, original-task requirements, good/bad examples, pass-count and evidence rules, verification commands, and explicit propagation targets. Attachment-reported simulations are evidence only; do not claim the worker ran those passes unless the worker actually ran or received independently inspectable records for them. Old PLAN_READY_FOR_LUBA_REVIEW may remain only as historical/pre-attachment/narrower-repair text with current PLAN_NOT_READY nearby.

Future gates are execution-continuation checkpoints, not stop permissions. After Luba approves a truly ready plan, gates are executed through bounded sub-agents until the product is complete. If a gate fails, the next bounded sub-agent fixes/mitigates or records a true external blocker. Do not stop/finalize while safe bounded work remains.

Any historical/pre-repair/evidence-only `NOT_READY`, `BLOCKED`, `FAILED`, `READY`, `DONE`, or similar status retained as evidence must be labelled historical/pre-repair/evidence-only, with the current verdict nearby. If a current artifact still makes Luba ask whether stale `NOT_READY` remains, the repair is incomplete.

Good future gate: `Run dotnet tests after implementation; if red, bounded fix worker repairs failing slice.` This is execution, not a design decision. Bad future gate: `Choose legal provider during implementation` or `figure out FLOW-38 input contract later`; these are design blockers, not execution gates. Good historical status: `Attachment 429f reported the pre-repair plan as NOT_READY; current verdict after repair is PLAN_NOT_READY because provider remains unchosen.` Bad historical status: leaving `NOT_READY` in a source map without saying historical/pre-repair and without current verdict next to it.

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

## Core Problem

A plan, review, or architect-level answer produced before the session read
the files is an essay, not an analysis. Without file reads, grep counts, and
verbatim excerpts from the actual code, synthesis claims are memory-retrieval
dressed as evidence. This skill installs the mandatory evidence threshold
that must be met before any synthesis output is produced.

---

## Subagent Firewall Sentinels

Preserve these exact proof strings from `AGENTS.md` as authority:
- `Parent Architect Subagent-Only Firewall`
- `parent local sidecar work ban`
- `read-only verification after a sub-agent packet returns`
- `sub-agent waiting is not a stop`
- `no local non-overlapping work escape`

---

## When to Invoke

- At session start, BEFORE any plan output or architectural synthesis
- Before any plan-review verdict
- BEFORE writing any phase plan or execution document
- When reviewing a completion claim against actual codebase state

---

## Section 1 — Thresholds per Session Type

No synthesis output may be produced until the reconnaissance threshold
is met AND the RECON REPORT is written.

| Session type | File reads | Grep counts | Verbatim excerpts |
|---|---|---|---|
| EXECUTOR | 5 | 2 | 3 |
| PLANNING | 10 | 3 | 5 |
| REVIEW | 15 | 5 | 8 |
| ARCHITECT | 20 | 8 | 10 |

**Wide-scope mode** (user says "see the whole picture" / "load the real state" /
uploads ≥5 files): double all thresholds.

**Counts are orthogonal.** Meeting the file-read count without meeting the grep
count fails the threshold. Each dimension catches a different failure class:

- File reads catch "that file doesn't exist at that path"
- Grep counts catch "how many of X are there and how many are populated"
- Verbatim excerpts catch "the code actually says Y, not what I remembered"

---

## Section 2 — Evidence Layer Tags

Every verbatim excerpt in the RECON REPORT carries one of four evidence layer
tags. A synthesis claim that asserts something "works" or "exists as implemented"
must trace to at least one IMPLEMENTATION or TEST excerpt.

```
DESIGN_DOC     File describes intended behavior — not evidence that the behavior
               exists in running code.
               Includes: ARCHITECTURE.md, CLAUDE.md, docs/, plan documents,
               skill files, AGENTS.md, DECISIONS-LOCKED.md.
               A "works" claim backed only by DESIGN_DOC is NOT verified.

IMPLEMENTATION File is source code — evidence of what the system actually does.
               Includes: *.cs, *.csproj, Program.cs, appsettings.json.
               A "works" claim requires at least one IMPLEMENTATION excerpt.

TEST           File is a test — evidence of what has been verified.
               Includes: *Tests.cs, *Test.cs, fixture files.
               A "passes" claim requires at least one TEST excerpt.

RECONCILIATION File is a state record that may be stale.
               Includes: SUBPROJECT-MAP.md, EXECUTION-LOG files, session state files.
               RECONCILIATION excerpts are hypotheses until confirmed by IMPLEMENTATION.
```

---

## Section 3 — RECON REPORT Template

Before first synthesis output, produce a RECON REPORT:

```markdown
# RECON REPORT — Session [date/ID]

Session type: [EXECUTOR | PLANNING | REVIEW | ARCHITECT]
Wide-scope: [YES | NO]
Threshold: [N] file reads / [M] grep counts / [K] excerpts
Actual: [n] file reads / [m] grep counts / [k] excerpts
Threshold met: [PASS | FAIL — short of threshold in dimension X]

## Domain Universe (ARCHITECT sessions only)
Entity type being examined: [subprojects | capabilities | interfaces | tests | other]
Total universe (N): [integer] — source: [command that established this count]
Examined this session: [n]
Coverage fraction: [n/N = X%]

Unread and material:
- [entity] — [why it could change a synthesis claim if read]
(If all material entities examined: "none — all material entities examined.")

Unread and not yet known to be material: [count]
Resolution: [deferred to next session | out of scope | examined if claim challenged]

## File Inventory
- [filepath]: [line count] lines — [one-line summary]
- ...

## Grep Findings
- Pattern "[pattern]" in [scope]: [N] hits
  - [file:line] [excerpt]
  - ...

## Verbatim Excerpts
- [filepath:line]: "[exact text]"
  Evidence layer: [DESIGN_DOC | IMPLEMENTATION | TEST | RECONCILIATION]
- ...

## Hypotheses (N≥5 entity examination only)
H1: [claim] — Confirms if: [observable] — Refutes if: [observable]
(declare ALL hypotheses before examining instance 1)
```

---

## Section 4 — Gate Enforcement

### Before first synthesis output

```
□ RECON REPORT exists
□ RECON REPORT meets threshold (all three dimensions)
□ Synthesis output cites RECON REPORT entries
```

### Before any "works" or "exists" claim

```
□ At least one IMPLEMENTATION or TEST excerpt backs the claim
□ If only DESIGN_DOC evidence exists: reword the claim to assert intent,
  not existence ("was designed to do X" not "does X")
```

### For ARCHITECT sessions: before system-level claims

```
□ Domain universe block present (coverage fraction declared)
□ Universal quantifiers ("no subproject", "every interface", "all tests") require
  either: ≥50% coverage in domain universe, or explicit scope qualifier:
  "among the [n] [entities] examined..."
```

### For multi-entity sessions (N≥5 instances): before examining instance 1

```
□ Hypotheses declared in RECON REPORT with Confirms-if / Refutes-if
□ per-entity-examination-SKILL.md activated
```

---

## Section 5 — .NET Project Tier 0 Reads

At the start of every ARCHITECT session in this project, read these files
before any other file. Tier 0 reads count toward the Section 1 threshold.

```
1. CLAUDE.md                    — 10 DNA Rules; BaseService pattern; OperationResult namespaces
2. AGENTS.md                    — OP-1..OP-7 agent rules
3. docs/DECISIONS-LOCKED.md     — locked decisions D-01..D-12; grep for latest D-NN
4. docs/SUBPROJECT-MAP.md       — current subproject status; grep for ACTIVE/STUB/MISSING
5. docs/COMPONENT-INTERFACES.md — interface registry; fabric contracts
```

Total Tier 0 contribution: 5 file reads + 2 grep counts (DECISIONS-LOCKED count,
SUBPROJECT-MAP ACTIVE count).

Tier 0 is not a substitute for full reconnaissance. It covers the governance
anchors. Flow/capability-specific reads follow Tier 0.

---

## Section 5A — High-Risk Architecture Recon Library Gate

For high-risk architecture plans, ILearningMachine/DynamicGraph plans,
cross-repository designs, source-corpus learning plans, or any plan where a
wrong foundation would cause broad downstream work, the normal RECON REPORT is
only the startup layer. Before architecture plan prose, build a Knowledge
Library from quantified source-corpus passes.

Default pass floor unless current Luba profile is stricter:

```text
external/source corpus such as `llm mvp`: >=20 focused passes
`llm_mvp_core` docs+code: >=40 focused passes
```

A focused pass means one bounded question, named roots, exact files/line ranges,
and a decision the pass can support or reject. Generic source maps, folder
summaries, or "read many docs" claims do not satisfy this gate.

Knowledge Library artifact schema:

```text
path:
type:
summary:
why_to_use:
when_to_use:
relevance_to_target_plan:
document_locations:
implementation_locations:
evidence_line_range:
decision_supported:
source_disjoint_evidence: independent_roots | duplicate_derived | single_source
confidence_effect:
held_out_roots_or_sources:
status: accepted | rejected | deferred | evidence_only
coverage_critic:
```

Gate enforcement:

```text
High-Risk Architecture Recon Library Gate required=true/false
required_pass_floor:
actual_external_source_corpus_passes:
actual_llm_mvp_core_docs_code_passes:
stricter_current_luba_profile_applied=true/false
Knowledge Library rows complete=true/false
source_disjoint_evidence complete=true/false when high-risk/multi-root/source-corpus planning is in scope
named artifact/library/folder population proof present=true/false
population proof exact_path/file_list_or_index/non_empty_proof/cutoff_snapshot=true/false
evidence_depth_score=targeted | recon-library | full-pass | legal-verified
moving_target_late_file_queue complete=true/false
generic_source_map_used_as_substitute=false
architecture_plan_prose_allowed=true only when pass floor and Knowledge Library pass
```

Population proof is required when the task names a library, folder, corpus, or
artifact as evidence. It must use the exact path, file list or index, non-empty
proof, cutoff snapshot, and late-file queue; broad folder grep or row presence
alone is not completion evidence.

Source-disjoint evidence is required for high-risk, multi-root, cross-corpus, or
broad architecture planning. Each relevant Knowledge Library row must say
whether the support comes from independent roots/sources, duplicate-derived
material, or a single source; how that affects confidence; and which roots or
sources remain held out. Do not treat repeated copies, summaries, or old packets
as independent evidence.

If the floor is not met, the output may be a recon plan or blocker, not
architecture plan prose.

---

## Anti-Patterns

1. **"I already know this codebase"** — codebase state changes between sessions.
   The RECON REPORT is a current-session snapshot, not a memory retrieval.

2. **"Project knowledge gives me the counts"** — the RECON REPORT requires actual
   file reads and grep command output, not recalled summaries.

3. **"I'll count later when I need it"** — counts go in the RECON REPORT before
   synthesis, not on-demand during synthesis.

4. **"DESIGN_DOC evidence proves implementation"** — a CLAUDE.md rule describes
   intent. A .cs file shows what exists. A "the pattern is implemented" claim
   requires IMPLEMENTATION evidence.

5. **"Threshold met means the picture is complete"** — the threshold confirms
   minimum evidence was gathered. Coverage fraction is declared separately.
   Threshold PASS + 10% coverage = threshold met at 10% coverage. Both facts
   must appear in the RECON REPORT.

---

## Integration

- per-entity-examination-SKILL.md activates when domain universe shows N≥5 unread material entities
- planning-session-startup-SKILL.md question 2 (baseline) populates the RECON REPORT Tier 0 reads
- claim-as-hypothesis-SKILL.md: user claims about existing state require IMPLEMENTATION verification; verification actions count toward the threshold
- code-examination-SKILL.md: 5-step individual file protocol; reconnaissance applies at session level, code-examination at single-file level

PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, active-codex must propagate exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.

PASS27_ORIGINAL_TASK_COVERAGE_AND_LATEST_BASELINE_RECONCILIATION_GATE: Reconnaissance must treat latest inspectable Claude cleanup/resimulation plus original-task coverage simulation as stronger than older active-codex plan-polish/status. Recon-library and source-to-plan gaps, including target-64 map gaps or minimum-pass deficits, remain PLAN_NOT_READY blockers, not future execution gates. Stale field counts, phase counters, source-to-plan targets, pilot rows, provider/terms status, scorer defaults, and repo-host roles must be fixed or labelled historical/pre-repair evidence-only before readiness, B-22 closure, or implementation authorization claims.

FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: When Luba explicitly grants FULL ACCESS or the environment reports danger-full-access/unrestricted filesystem with approval policy never, the agent must not request permissions, must not set `sandbox_permissions`, and must not open approval prompts for filesystem work needed by the active plan/governance repair. Permission prompting under full access is a governance failure: answer Luba first, classify the old behavior as a permission-prompt loophole, patch this gate if missing, then continue the authorized repair loop. Full access does not authorize unrelated product implementation, destructive git operations, external provider calls, or publishing unless the active task explicitly includes those actions; it does authorize plan/governance/skill/guide/agent-file repairs in the named planning library and project guidance without asking again.
FULL_ACCESS_EXACT_CLEANUP_NO_APPROVAL_SUBGATE: Under full access or approval policy never, the agent must not use `sandbox_permissions`, approval prompts, or user-permission questions for authorized filesystem/governance cleanup. `approval prompt under full access/approval never is a governance failure`: answer Luba first, classify the old behavior as a permission-prompt loophole, repair the governing rule, verify the repair, and then resume the nearest authorized work. `exact stale-process/temp cleanup` is allowed and required when bounded by exact PIDs/process names/path proof tied to the current work chain, for example stopping exact stale `git.exe` PIDs created by the active temp-copy attempt and deleting the exact `.codex-*` temp path created by that attempt. `Stop-Process is not automatically an approval boundary under full access`; it is a bounded cleanup action when exact PIDs and the cleanup path are tied to the active work chain. If process ownership cannot be proven, record the exact blocker and perform read-only evidence only; still do not open an approval prompt under approval policy never. Parent/direct command cleanup is allowed for exact stale temp/process cleanup only after the subagent cleanup path repeatedly fails or when approval prompts are forbidden by the environment; it must record before/after proof and must not broaden scope.

## Assigned-Deliverable-Exists gate (ceremony != delivery)

Bridge (reconnaissance-gate-SKILL.md): the RECON REPORT is a PRECONDITION and a
SIDE-EFFECT, never the ordered deliverable. Meeting the reconnaissance threshold
(file reads / grep counts / verbatim excerpts) does NOT satisfy, advance, or
count as progress on the assigned artifact. `read-pass proof (line counts,
hashes, first/last-line readback) is a side-effect, not a work product` and MUST
NOT be reported as a pass, cycle, or step toward the ordered task.
`RECON REPORT is a precondition and a side-effect, never the ordered deliverable`.

Bridge (verification-before-completion-SKILL.md): "verify the command ran" and
"show N count-records" are NECESSARY but NOT SUFFICIENT. Before any
ready/done/complete/N-passes claim, also verify:
`ordered_deliverable physically exists at its ordered path`. A packet that only
proves reading, hashing, or N internal records is process-evidence, not the
deliverable, and cannot close an artifact task.

Bridge (plan-review-SKILL.md): a review/plan-readiness pass counts toward any
requested floor ONLY if that pass's evidence packet advances the ORDERED
deliverable. A pass whose whole content is a read-pass, a hash, a pass-ledger
row, or a governance/rule edit that is not the ordered deliverable counts as
ZERO toward the floor.

Deliverable-Exists checklist (run before every PASS/READY/complete/N-passes
claim and whenever the human signals wasted time / impatience):

    ordered_deliverable = <exact artifact the human asked for, with its path>
    what_this_step_physically_produced = <ordered_artifact | only_process_evidence>
    ordered_deliverable_exists_at_path = true/false   # readback-verified, non-empty
    process_evidence_reported_as_progress = false     # read-pass/hash/ledger/rule-edit is NOT progress
    governance_or_rule_edit_is_the_ordered_deliverable = true/false
    target_path_verified_correct = true/false         # right repo/file, not a stray copy

Hard fail (return to producing the ordered artifact, do not claim a pass) when:
- `ordered_deliverable_exists_at_path=false` but a PASS/READY/complete/N-passes
  claim is made;
- `what_this_step_physically_produced=only_process_evidence` and it is reported
  as progress on the ordered task;
- a pass-ledger, results-ledger, or `independent-pass-results-*` file is offered
  as the ordered artifact when the ordered artifact is something else (e.g. a
  branch comparison, a report, code);
- a governance/skill/rule edit is treated as the ordered deliverable while
  `governance_or_rule_edit_is_the_ordered_deliverable=false`;
- `target_path_verified_correct=false` (wrong repo/copy, e.g. an
  `llm_mvp_core1`-style stray path) — verify the target before writing.

Convenient-interpretation lock: if the ordered artifact already exists in
chat/draft, PERSIST it as the ordered artifact at its path. Do NOT re-classify a
finished deliverable as "not yet proven" and launch more read-pass / N-pass
process on top of it. An honest downgrade lowers the CONFIDENCE of a produced
result; it never dissolves the obligation to persist that produced deliverable.

Frustration pivot: when the human says time/tokens/nerves were wasted or "didn't
solve a single task", that is a hard signal that ceremony replaced delivery.
Stop process-about-process immediately and produce the ordered artifact next.

Literals for exact-file verification:
`Assigned-Deliverable-Exists gate`;
`RECON REPORT is a precondition and a side-effect, never the ordered deliverable`;
`read-pass proof (line counts, hashes, first/last-line readback) is a side-effect, not a work product`;
`ordered_deliverable physically exists at its ordered path`;
`a pass counts toward the floor only if it advances the ordered deliverable`;
`a pass-ledger is a process record not the ordered artifact`;
`honest downgrade lowers confidence not the obligation to persist`;
`verify the target path before writing (right repo, not a stray copy)`.
