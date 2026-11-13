# Reference Inventory And Source Map Skill

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
Missed reference propagation is a planning-gate failure, not an execution question.

Application: for Claude/Codex/active-codex planning repair, Claude is the baseline authority; Codex may only add validated non-contradictory deltas/source-map details. Any active-codex open question answered by Claude or Codex evidence must be downgraded from open to resolved/partial with exact source evidence before execution work can be issued.

PASS24_LOCKED_BASELINE_NO_DOWNGRADE_GATE: When the Claude baseline labels a decision `LOCKED`, `RESOLVED(Luba YYYY-MM-DD)`, or equivalent Luba-resolved wording, active-codex copies must carry that decision as locked/resolved. They must not downgrade it to `PROPOSED_NOT_LOCKED`, an open question, pending default, or residual-for-Luba row unless a newer explicit current Luba override is cited. Such a downgrade is active-vs-Claude drift and must be repaired in the source map, active plan, and ledger before readiness, blocker closure, or implementation authorization claims.

PASS25_OWNER_CANON_FIELD_COUNT_PROPAGATION_GATE: When the Claude baseline changes an owner-canon field count or schema, source maps must inventory the new owner count/schema and every active owner/consumer reference that must be updated. Active copies must propagate the exact new count, fields, gates, and status, or label old text historical/pre-Pass25 evidence-only. Example: `TeacherProvenance` 18 -> 20 fields with RR6 `service_mode`/`grounding_used`, C17 service-mode/grounding/search ban, and pending provider-terms behavior must appear in the active owner plan, source map, board, matrix, gate, ledger, relevant guides/policies, and governance before B-22 closure, readiness, or implementation authorization. Any stale unqualified field-count/schema reference is active-vs-Claude drift and blocks closure.

PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, source maps must inventory the new board-owner counts, pilot identity, pilot row range, status, blocked count, and every active owner/consumer reference that must be updated. Active copies must propagate the exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.

PASS27_ORIGINAL_TASK_COVERAGE_AND_LATEST_BASELINE_RECONCILIATION_GATE: Source maps must treat latest inspectable Claude cleanup/resimulation plus original-task coverage simulation as dominating older active-codex plan-polish/status. Active-codex cannot claim readiness, B-22 closure, or implementation authorization until every original-task requirement is mapped to active source-map rows and the active plan, board, matrix, and coverage gate, and every latest inspectable baseline detail is reconciled. Stale field counts, phase counters, source-to-plan targets, pilot rows, provider/terms status, scorer defaults, and repo-host roles must be fixed or labelled historical/pre-repair evidence-only. Recon-library and source-to-plan gaps, including target-64 map gaps or minimum-pass deficits, remain PLAN_NOT_READY blockers, not future execution gates.

FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: When Luba explicitly grants FULL ACCESS or the environment reports danger-full-access/unrestricted filesystem with approval policy never, the agent must not request permissions, must not set `sandbox_permissions`, and must not open approval prompts for filesystem work needed by the active plan/governance repair. Permission prompting under full access is a governance failure: answer Luba first, classify the old behavior as a permission-prompt loophole, patch this gate if missing, then continue the authorized repair loop. Full access does not authorize unrelated product implementation, destructive git operations, external provider calls, or publishing unless the active task explicitly includes those actions; it does authorize plan/governance/skill/guide/agent-file repairs in the named planning library and project guidance without asking again.
PASS14_REPO_BRANCH_WORKDIR_AUTHORITY_GATE: For implementation repo source maps, authority must come from the latest updated branch or most advanced branch explicitly selected from each implementation repo's own plan/repo inventory, not from copied workdir names, stale branch memory, or a planning workspace path. The implementation working name `DPOLegalLearning` must be recorded as a work branch/workdir convention under `C:\Projects` only after plan readiness and fresh Luba implementation approval. `<planning-workspace>` is planning library/workspace evidence, not implementation repo authority. Before every future implementation commit and push, the executor must refresh/check the original authority branch and validate mergeability/no-conflict back into that original branch. PR creation and merge are forbidden unless Luba later explicitly asks. Planning passes must not claim branch freshness, workdir creation, mergeability, commit, push, PR, merge, or implementation authorization unless those actions actually ran in the authorized future implementation scope.
PASS21_CLAUDE_REPAIR2_IN_FLIGHT_GATE: A quoted Claude-side `claude-*` Repair2, resimulation, or workflow that is running, edited, pending, or not yet inspectable is inventory evidence only. Source maps must label it `CLAUDE_REPAIR2_RUNNING_OR_UNVERIFIED`, must not treat it as the accepted Claude baseline, and must add a required reconciliation row: inspect exact workflow output and changed `claude-*` files, then refresh active-codex before B-22 closure or readiness.

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

## When to load

Load this skill before deep planning, plan review, benchmark-depth comparison,
shallow-planning repair, shallow-review repair, architecture review, or any
artifact that makes claims about existing files, guides, code, plans, tests, or
governance rules.

## Purpose

Force every planning or review claim to trace back to exact read sources. A
broad statement such as "the repo/docs say" is not evidence until it is mapped
to a source id, exact path, line range, and the plan or review claim it
supports.

## Subagent Firewall Sentinels

Preserve these exact proof strings from `AGENTS.md` as authority:
- `Parent Architect Subagent-Only Firewall`
- `parent local sidecar work ban`
- `read-only verification after a sub-agent packet returns`
- `sub-agent waiting is not a stop`
- `no local non-overlapping work escape`

Человеческое правило (RU): карта источников нужна не для красивой таблицы, а
чтобы Luba видела, откуда взят каждый вывод. Чужой пример, старый план,
benchmark, sub-agent packet или broad folder search могут быть только
доказательным контекстом. Они не становятся текущей задачей, текущим
артефактом, лицензией на копирование или product-ready доказательством без
прямой текущей команды Luba и прямого чтения источника.

## Required Source Inventory

Create a source inventory before plan prose or before approving a review:

```text
Reference Inventory:
  source_id:
  path:
  line_range:
  source_layer: current_luba_quote | work_order | plan | guide | skill | code | test | doc | benchmark_evidence | sub_agent_packet
  claim_supported:
  plan_section:
  decision_supported:
  active_artifact_scope: exact_current_target | evidence_only | out_of_scope
  source_license_status: project_owned | third_party | unknown | not_applicable
  reuse_boundary: universal_pattern | project_specific | benchmark_only | evidence_only
  source_disjoint_evidence: independent_roots | duplicate_derived | single_source
  confidence_effect:
  held_out_roots_or_sources:
  status: used | rejected | unavailable | evidence_only
  why_used_or_rejected:
```

Rules:
- Every read source must map to a claim, plan section, review finding, or
  explicit rejection row.
- Broad repo, folder, guide-family, or document-family references cannot satisfy
  a plan or review claim without exact `source_id`, `path`, and `line_range`.
- Benchmark examples, quoted transcripts, old plans, state files, and
  sub-agent packets may support `benchmark_evidence` or `evidence_only` rows
  only; they cannot create active requirements unless Luba reactivates them in
  the current unquoted instruction.
- Every source row must distinguish universal reusable pattern from
  project-specific detail. Project-specific paths, client names, tenant data,
  private prompts, screenshots, raw design files, and licensed third-party
  material cannot be copied into a universal guide unless the row records the
  license/source boundary and the reusable pattern is restated generically.
- A source with `source_license_status=unknown` cannot support implementation,
  publication, dataset, or product-readiness claims. It may only support a
  recon task, an evidence-only note, or a rejected/deferred review row.
- For high-risk, multi-root, cross-corpus, or broad architecture planning, every
  source row must state whether evidence is from independent roots/sources,
  duplicate-derived material, or a single source. Record how this changes
  confidence and which roots or sources remain held out. This is a general
  planning/review confidence gate, not a DPO/training-only rule.
- The exact current artifact must stay explicit. A map for one plan, guide, or
  sub-agent packet cannot silently repair, approve, stage, commit, or replace a
  different artifact.
- If a source was read but does not affect the plan or review, mark
  `status=rejected` and explain why. Silent unused reads fail the map.
- If a source is needed but unavailable, mark `status=unavailable`, name the
  blocked claim, and either remove the claim or queue an explicit recon task.

## Source-To-Plan / Source-To-Review Map

After the inventory, create the map:

```text
Source Map:
  source_id:
  claim_supported:
  plan_section_or_review_finding:
  decision_supported:
  exact_evidence_line_range:
  active_artifact_scope:
  source_license_status:
  reuse_boundary:
  source_disjoint_evidence:
  confidence_effect:
  held_out_roots_or_sources:
  status: mapped | rejected | missing | contradicted
  why_used_or_rejected:
```

Review must check:
- every binding requirement has at least one mapped current-authority source;
- every code/doc/governance claim has an exact source id and line range;
- every review finding has PASS/FAIL, exact line or absence evidence, impacted
  requirement, required fix, and re-review trigger;
- every rejected or evidence-only source stays out of active target scope;
- every license/source row and universal-vs-project-specific boundary is
  explicit before a detail is reused outside its original project context;
- high-risk or multi-root claims record source disjointness, confidence impact,
  and held-out roots/sources before the claim is accepted;
- no completion/readiness claim depends on a broad folder search, memory, or a
  sub-agent summary without direct source read-back.
- no plan/governance review claims product evidence or implementation
  readiness unless the exact mapped artifact contains runtime/test evidence
  required by its Gate B.

Target-count reconciliation rule:
- Any source-map target count such as `target 64`, `64/64`, `N rows`, or `all
  substeps` requires enumerable records before a completion claim.
- Summary prose, ledger rows, checklist rows, file counts, category rows, and
  support-group rows are not count completion evidence.
- Source-to-Plan row counts must use exact row-id matching, not loose `^| Ф`
  scans. Canonical row ids use `Ф<phase>.<subphase>.<substep>`, for example
  `Ф1.1.1`; in Markdown tables the counting regex is
  `^\|\s*Ф\d+\.\d+\.\d+\s*\|`. Group, support, and summary table labels must
  not start with `Ф` unless they are actual canonical row ids. A non-row table
  line starting with row-like text such as `Ф1.1` or `Ф10` is a trace defect
  that must be renamed or explicitly excluded before any count claim.
- Count verification must include three visible outputs: exact canonical row
  count, status histogram, and every noncanonical offending table line beginning
  with `| Ф` or `|Ф` that fails the canonical row-id regex. Do not accept a
  source-map pass when the offender list is missing, truncated, or only reported
  as a bare zero.
- Each counted row must identify row id/category, active owner, baseline
  source, accepted delta status if any, downstream consumers, current
  acceptance state, and next repair route.
- Allowed current states include `MAPPED_VERIFIED`, `MAPPED_ACCEPTED_DELTA`,
  `PARTIAL_NEEDS_OWNER_REVIEW`, `MISSING_ACTIVE_COUNTERPART`,
  `STALE_HISTORICAL_ONLY`, `CLAUDE_REPAIR2_UNVERIFIED`, and
  `NEEDS_CODE_REF_INVENTORY`.
- If the full denominator is not enumerated and checked, the source map must
  state the exact narrower count or state `64/64 not claimed` / `N/N not
  claimed`; B-22 or readiness cannot close from target-count prose alone.
- Stale current counters in owner sections must be rewritten in place. A later
  disclaimer, historical label, or downstream override does not neutralize an
  active contradictory owner sentence that still says an old denominator,
  remaining-row count, status, pilot count, field count, or readiness state.
- Stale current counters in consumer sections such as execution boards,
  requirements matrices, full-task gates, library indexes, and continuation
  ledgers must also be rewritten in place. A correct owner roster plus a later
  disclaimer does not neutralize a current contradictory consumer table, list,
  heading/body pair, or status counter.

If any row is `missing` or `contradicted`, the plan or review is not ready.

## High-Risk Architecture Recon Library Hook

When `reconnaissance-gate-SKILL.md` requires the High-Risk Architecture Recon
Library Gate, the Reference Inventory and Source Map must link to the Knowledge
Library artifact instead of replacing it. A generic source inventory is not a
substitute for the quantified library passes.

Required additional rows:

```text
Knowledge Library:
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
  status:
  coverage_critic:
```

## Named Artifact / Library / Folder Population Proof

When a claim depends on a named artifact, evidence library, or folder, add a
population proof row before completion:

```text
Population Proof:
  exact_path:
  file_list_or_index:
  non_empty_proof:
  cutoff_snapshot:
  late_file_queue:
  evidence_depth_score: targeted | recon-library | full-pass | legal-verified
```

Reject completion when the proof is a broad folder grep, remembered path,
summary-only packet, or empty directory. Accepted late files must be merged into
the current target artifact or rejected/deferred with reason and line/section
read-back.

Review must fail when:
- high-risk architecture plan prose appears before the required pass floors;
- Knowledge Library rows lack `coverage_critic` or `decision_supported`;
- high-risk or multi-root Knowledge Library rows omit
  `source_disjoint_evidence`, `confidence_effect`, or
  `held_out_roots_or_sources`;
- a broad source map, folder grep, or old packet is used as the library;
- a required artifact/library/folder lacks exact path, file list or index,
  non-empty proof, cutoff snapshot, or evidence depth score;
- source rows do not distinguish document locations from implementation
  locations for decisions that depend on both.

## Source Addressability And N-Pass Library Floor Gate

When the current task names source folders, planning materials, repositories, or
API/client/platform roots, the source map must include one row per named source
with:

```text
short_name:
exact_physical_path:
source_type:
inventory_or_navigation_row:
target_plan_consumer:
status: mapped | missing | removed_by_current_luba_decision
```

Shortname-only references are not enough. A material such as
`Planning sessions/session-start-extracted` with no exact path, no inventory row,
and no navigation target is a plan-coverage hole.

Do not normalize, shorten, or guess repository/folder names from a short label.
If the task names `xiigen-platform-dot-net-angular`, the active source map must
carry that exact path after a `Test-Path`/existence check; `xiigen-platform` is a
different path, not an acceptable alias. If an old plan row contains the shorter
path, mark it historical/wrong or repair it before readiness.

For explicit pass-floor requirements such as `>=20` or `>=40` focused passes,
the source map must name:

```text
target_library_artifact:
pass_floor:
corpus_partition:
per_pass_record_template:
acceptance_rule:
floor_narrowing_luba_quote_or_NONE:
```

If `floor_narrowing_luba_quote_or_NONE` is `NONE`, the pass floor remains
binding and cannot be downgraded by a worker, skill, or older overlay.
