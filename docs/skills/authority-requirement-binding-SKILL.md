# Authority Requirement Binding

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

## Strict Plan Readiness / Original Task Simulation / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers and complete coverage of the original task. The plan must be mentally simulated as if implemented and must cover every original-task requirement. Future Gate B may contain execution actions such as tests/builds/screenshots/mergeability checks. Future Gate B may NOT hide unchosen legal provider, missing input/output contracts, missing dual-path design, unresolved repo ownership, missing external API seam design, missing provenance policy, missing thresholds, missing model/checkpoint lifecycle, or missing source-to-plan mapping. If a future gate hides a design/dependency decision, verdict is PLAN_NOT_READY.

30 sequential simulation/review passes are required for this readiness class: one pass at a time, no parallelism. Each pass records `requested_model: gpt-5.5`, `requested_reasoning_effort: xhigh`, skill focus, original-task requirement checked, plan section checked, mental simulation step, coverage verdict, finding, repair/no-fix reason, and recheck result. A single worker doing 30 internal passes must call them sequential simulation/review passes, not 30 sub-agents or 30 independent sub-agent passes.

For ILearningDynamicGraph/DPO readiness repair, `B-1..B-22 unresolved blocker inventory is mandatory`. Missing blocker numbers are trace defects, not permission to drop them. Older B1-B21 or B5-absent evidence must become explicit historical/trace-repair backlog, including `MISSING_NUMBER_NEEDS_TRACE_REPAIR` for grouped, absent, renamed, or ambiguous source evidence.

`PLAN_NOT_READY after full-task coverage simulation is not a final reporting boundary`. `PLAN_NOT_READY` is a bounded repair queue, not a stop, while safe plan/governance repair remains. A final answer after only downgrading readiness is invalid unless all implementable blockers have been repaired, delegated, or proven external/non-actionable.

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

PASS24_LOCKED_BASELINE_NO_DOWNGRADE_GATE: When the Claude baseline labels a decision `LOCKED`, `RESOLVED(Luba YYYY-MM-DD)`, or equivalent Luba-resolved wording, active-codex copies must carry that decision as locked/resolved. They must not downgrade it to `PROPOSED_NOT_LOCKED`, an open question, pending default, or residual-for-Luba row unless a newer explicit current Luba override is cited. Such a downgrade is active-vs-Claude drift and must be repaired in the source map, active plan, and ledger before readiness, blocker closure, or implementation authorization claims.

PASS25_OWNER_CANON_FIELD_COUNT_PROPAGATION_GATE: When the Claude baseline changes an owner-canon field count or schema, active-codex owner copies and all owner/consumer references must propagate the exact new count, fields, gates, and status, or label the old text historical/pre-Pass25 evidence-only. Example: `TeacherProvenance` 18 -> 20 fields with RR6 `service_mode`/`grounding_used`, C17 service-mode/grounding/search ban, and pending provider-terms behavior must be reflected in the active owner plan, source map, board, matrix, gate, ledger, relevant guides/policies, and governance before B-22 closure, readiness, or implementation authorization. Any stale unqualified field-count/schema reference is active-vs-Claude drift and blocks closure.
PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, active-codex must propagate exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.

PASS27_ORIGINAL_TASK_COVERAGE_AND_LATEST_BASELINE_RECONCILIATION_GATE: Latest inspectable Claude cleanup/resimulation plus original-task coverage simulation dominates older active-codex plan-polish/status. Active-codex cannot claim readiness, B-22 closure, or implementation authorization until every original-task requirement is mapped to the active plan, source map, board, matrix, and coverage gate, and every latest inspectable baseline detail is reconciled. Stale field counts, phase counters, source-to-plan targets, pilot rows, provider/terms status, scorer defaults, and repo-host roles must be fixed or labelled historical/pre-repair evidence-only. Recon-library and source-to-plan gaps, including target-64 map gaps or minimum-pass deficits, remain PLAN_NOT_READY blockers, not future execution gates.

FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: When Luba explicitly grants FULL ACCESS or the environment reports danger-full-access/unrestricted filesystem with approval policy never, the agent must not request permissions, must not set `sandbox_permissions`, and must not open approval prompts for filesystem work needed by the active plan/governance repair. Permission prompting under full access is a governance failure: answer Luba first, classify the old behavior as a permission-prompt loophole, patch this gate if missing, then continue the authorized repair loop. Full access does not authorize unrelated product implementation, destructive git operations, external provider calls, or publishing unless the active task explicitly includes those actions; it does authorize plan/governance/skill/guide/agent-file repairs in the named planning library and project guidance without asking again.
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

Agents often preserve the shape of an older plan after the task authority adds
or strengthens a requirement. That turns explicit requirements into wishlist
items and lets downstream agents silently narrow the task. This skill makes
every upstream requirement a binding acceptance criterion before plan prose,
work orders, review verdicts, or execution.

---

## When to Invoke

- Before drafting, reviewing, approving, executing, or repairing any plan
- After any user/operator/product-owner correction
- Before an architect issues a bounded work order to a sub-agent
- After an accepted reviewer finding becomes a repair requirement
- When a plan uses trainable units, learned routing, learned grounding, model
  assets, import/export, checkpoints, manifests, or marketplace/locator loading

---

## Section 1 - Active Task Authority

Identify every active authority source:

```text
human_operator
product_owner
architect_work_order
approved_active_plan
accepted_reviewer_gate
```

Every explicit requirement from those sources is binding unless the same source
explicitly marks it optional, exploratory, or preference-only.

Downstream agents must not downgrade, ignore, reinterpret, or replace an
upstream requirement with a more convenient narrower task.

---

## Section 2 - Required Ledgers

Before plan prose or review verdict, produce:

```text
Authority Requirements Ledger
Conversation Delta Ledger
Replacement Mapping Register
Contradiction Sweep
```

Each Authority Requirements Ledger row must contain:

```text
requirement_id
source_authority
source_quote_or_work_order_text
normalized_requirement
requirement_type = must_do | must_not_do | scope | sequence | artifact | gate | definition | replacement
binding_level = required | optional_by_source
old_assumption_invalidated
forbidden_or_superseded_mechanism
required_replacement_capability
plan_search_terms
plan_section
gate_or_test
required_artifact
contradiction_scan_result
status = mapped | unmapped | deletion_only | contradicted
next_action
```

The Conversation Delta Ledger records only requirements added, clarified, or
strengthened since the previous review. The delta must name the old assumption
that is no longer valid.

The Replacement Mapping Register is required whenever an authority forbids,
rejects, replaces, or supersedes a mechanism.

The Contradiction Sweep searches the whole plan, not only the edited section.

---

## Section 3 - Replacement Rule

Removing an old mechanism is not coverage for a required replacement.

If the authority requires a trainable replacement, the plan must name:

```text
trainable_unit
trained_state
checkpoint_path_or_checkpoint_asset
manifest
export_path
import_path
locator_or_marketplace_entry
fresh_load_probe
continue_training_probe
ablation_or_leakage_gate
```

A deterministic mechanism cannot satisfy a trainable replacement requirement
unless the active authority explicitly allows that deterministic mechanism in
the current conversation or work order.

---

## Section 4 - Prompt Flow Grounding Example

If the authority requires `PromptFlowGroundingModel`, then production prompt
grounding must be handled by that trainable unit or an explicitly named
trainable equivalent.

These are not coverage for `PromptFlowGroundingModel`:

```text
PromptFlowIndex removal
FlowReferenceDetector runtime routing
explicit FlowIndex production routing
direct dictionary retrieval
exact-prompt lookup
regex-only flow selection
KnownFlowRetrieval as deterministic production path
```

Allowed deterministic helper scope:

```text
corpus labeling
build-time audit
fixture generation
evaluation setup
```

Forbidden production contradiction sweep terms:

```text
PromptFlowIndex
FlowReferenceDetector
FlowIndex
KnownFlowRetrieval
deterministic retrieval
exact prompt lookup
regex-only flow selection
_flowIndex.TryGetValue
```

If any term remains in a production routing path while the ledger requires
trainable grounding, the plan is contradicted.

---

## Section 5 - Review Verdict Rules

Approve only when:

```text
all binding requirements are mapped
all replacement requirements name replacement capabilities
all trainable replacements have model asset gates
contradiction sweep has no production contradictions
completion matrix has one row per ledger row
```

Reject when:

```text
requirements are treated as suggestions
only the closest existing architecture is covered
old mechanism is deleted without required replacement
deterministic production shortcut replaces a required trainable unit
sub-agent work order omits architect requirements
reviewer gate findings are not converted into downstream requirements
```

---

## Output Format

```markdown
## Authority Requirement Binding Check

Active authorities:
- [authority]: [source]

Conversation Delta Ledger:
| ID | Source | Requirement | Old assumption invalidated | Required replacement | Status |
|---|---|---|---|---|---|

Replacement Mapping Register:
| Superseded mechanism | Required replacement | Plan section | Gate/artifact | Status |
|---|---|---|---|---|

Contradiction Sweep:
| Search term | Production occurrence | Allowed helper occurrence | Verdict |
|---|---|---|---|

Verdict:
APPROVE | REJECT

Next action:
[first unmapped/deletion-only/contradicted row repair]
```

---

## Anti-Patterns

1. Treating an explicit upstream requirement as a wishlist item.
2. Saying "removed X" when the requirement was "replace X with Y."
3. Checking only the edited plan section instead of searching the whole plan.
4. Letting a sub-agent execute a narrower version of the architect work order.
5. Counting deterministic lookup, regex, or dictionary routing as a trained
   model when the authority required a trainable unit.
