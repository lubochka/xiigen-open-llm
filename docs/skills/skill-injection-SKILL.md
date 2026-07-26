# Skill Injection

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the same in the operator's language, e.g. "почему"), challenges an answer, or says a failure came
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

## What This Skill Does

Governs how skill files are selected and injected as context into generation and planning steps.
Without skill injection, generated code may violate DNA rules or miss required patterns.

## Luba Plan-Quality Criteria / No Executor Without Skills

When Luba asks which plan is better, route the task through SK-INTENT, SK-PLAN,
SK-REVIEW, and SK-ARCH when trainable/DPO/learned/adaptive/semantic
architecture is involved. "Better plan" means better by Luba's criteria:
human clarity, readable architecture, architectural reliability, evidence
honesty, requirement fidelity, execution-control quality, and low overclaim
risk. Recency, file count, ledger size, or fresher status tables are only delta
evidence and cannot by themselves win the comparison.

If one plan is stronger on Luba's criteria and another plan has newer deltas,
the newer plan is a patch/source-map input, not automatically the baseline.
Executor-only conversation with Luba is forbidden until SK-INTENT, SK-PLAN, and
SK-REVIEW are loaded and acknowledged for the current task.

In brief for the architect: when planning and reviewing, you cannot stop at
short labels like `SK-PLAN` or `SK-REVIEW`. The work order and the return
packet must name, in plain language, which files/sections are being checked,
which sources were read, where the boundary of the current artifact is, and
why this does not permit direct execution by the parent session.

Authority requirement binding is mandatory whenever SK-PLAN is selected.
SK-PLAN includes `authority-requirement-binding-SKILL.md`,
`reference-inventory-and-source-map-SKILL.md`, and `phase-planning-SKILL.md`.
It must be selected for new or changed upstream requirements, user corrections,
architect work orders, accepted reviewer gates, plan review, and phase
boundaries.

SK-REVIEW is a first-class sibling of SK-PLAN for plan review, user
correction, benchmark-depth comparison, and shallow-review repair. SK-REVIEW =
`plan-review-SKILL.md` + `goal-delivery-completeness-SKILL.md` +
`claim-verification-SKILL.md` + `audit-protocol-SKILL.md` +
`specificity-calibration-SKILL.md` + `output-skepticism-SKILL.md` +
`reconnaissance-gate-SKILL.md` +
`reference-inventory-and-source-map-SKILL.md`. Add
`human-readable-architecture-audit-SKILL.md` for architecture/trainable/code/audit plans. Add
`multi-reviewer-design-SKILL.md` for major or multi-phase plans.

Sequential execution-control routing: SK-PLAN and SK-REVIEW must be selected
for plan creation, plan review, execution status, phase/step status, or user
complaints such as operator-language phrases like `разброс`, `прыжки по фазам`, `где мы`, `сколько осталось`,
`done/in-progress/remaining`, `phase/step/status`, and equivalents. The loaded
skills must enforce `current_execution_pointer`, `ordered_phase_queue`, exact
`phase.step.status` rows, status vocabulary `done | in-progress | remaining |
review-block | blocked`, no phase jumping, matching counts, explicit completed
phase list, current phase/step, remaining phase list, remaining action
names/count, phase explanations, and a `requirement -> phase -> evidence`
matrix. For Luba-facing updates they must also enforce the binding
human-readable status template: `Phase N/14: <human name>`, `what the phase solves`,
`Step NN.MM/NN.Total: <human step name>`, `what the step solves`, `Now` with a
plain-language translation of the status in the operator's language, `what was found`, `what was fixed`,
`what remains`, plus phase done/remaining and total done/remaining counts.
The bare form `Phase N/14, step NN.MM/NN.Total, status X` is invalid without
that explanation. Technical labels such as `generic_bad_output`,
`writer-level path`, JSON paths, STATE flags, or validator names must be
translated in the same update.

SK-PLAN must carry the Authority Requirements Ledger, Conversation Delta
Ledger, Reference Inventory, Source Map, Replacement Mapping Register, and
Contradiction Sweep. For high-risk, large, multi-phase, benchmark-depth, or
multi-root plans it must also carry the Whole-Plan Contradiction Sweep, final
Open Decisions Queue, supplement disposition, explicit `claude-*`/`codex-*`/
`dcm-*` prefix quarantine, named artifact/library/folder population proof with
`evidence_depth_score`, and source-disjoint evidence
(`independent_roots`/`duplicate_derived`/`single_source`, confidence effect,
held-out roots/sources). A planning call without those artifacts is missing the
requirement-binding/source-map block even if it loaded
`phase-planning-SKILL.md`.

---

## Injectable Skill Blocks: Core Candidates and Conditional Add-Ons

| Block ID | Skill File | Content | Token Budget |
|----------|-----------|---------|-------------|
| SK-DNA | dna-compliance-guard-SKILL.md | 10 DNA rules + grep checks | ≤ 800 tokens |
| SK-FAB | integration-testing-SKILL.md + fabric interfaces | Fabric coverage + interface contracts | ≤ 800 tokens |
| SK-PLAN | authority-requirement-binding-SKILL.md + reference-inventory-and-source-map-SKILL.md + phase-planning-SKILL.md | 9 gates pre-code, source inventory/map, Whole-Plan Contradiction Sweep, Open Decisions Queue, supplement disposition, prefix quarantine, population proof with evidence_depth_score, and source-disjoint evidence when triggered | ≤ 1000 tokens |
| SK-REVIEW | plan-review-SKILL.md + goal-delivery-completeness-SKILL.md + claim-verification-SKILL.md + audit-protocol-SKILL.md + specificity-calibration-SKILL.md + output-skepticism-SKILL.md + reconnaissance-gate-SKILL.md + reference-inventory-and-source-map-SKILL.md | First-class plan review, correction, benchmark-depth, specificity, source-backed claim verification, exact active-artifact/source-map discipline, final readiness sweep, source-disjoint evidence, prefix quarantine, population proof, supplements, and final Open Decisions Queue | ≤ 1800 tokens |
| SK-TEST | three-level-verification-SKILL.md | Unit + integration + suite | ≤ 800 tokens |
| SK-DOCS | documentation-sync-SKILL.md | File → canonical docs map | ≤ 800 tokens |
| SK-VERIFY | generated-code-review-SKILL.md + verification-before-completion-SKILL.md | 5-layer generated-code review (semantic + DNA + silent-failure + generalization + trainable-vs-stub) PLUS the Runtime Evidence Firewall: reject validation_only/static/contract-only/UNKNOWN; require numeric runtime metrics (accuracy/loss/reward, cpu_ms, ram_mb) | ≤ 1000 tokens |
| SK-QA | three-level-verification-SKILL.md + integration-testing-SKILL.md + AGENTS.md OP-55/OP-56/OP-57 | Engineering test strategy: unit/integration/e2e-host/QA-automation/negative + untrained-honesty + trained-evidence + forbidden fake-pass; forces the work order to declare test layers and untrained/trained expectations | ≤ 1000 tokens |
| SK-ARCH | trainable-architecture-decision-SKILL.md | Allowed trainable architecture gate: map learned/scorer/selector/router/classifier/KNN/dictionary/pattern-matcher mechanisms to ILearningMachine, ILogicalOrchestrator, ILogicalFlow, ILogicalUnit, or ILearningMachinePair; helpers may be classified only as internal details inside one of those levels | ≤ 1000 tokens |

**Hard limit: maximum 3 core candidate skill blocks per generation call.**

**Exception — SK-VERIFY is mandatory-additional.** Whenever the work touches a trainable `ILogicalUnit`/`ILearningMachine`, OR makes any "phase done"/completion claim on an algorithm or model, OR reviews generated code, SK-VERIFY is injected IN ADDITION to the cap and is NEVER dropped. Mandatory-additional blocks are exempt from the top-3 cap; total injected blocks may exceed 3 by the number of triggered mandatory blocks. Skipping SK-VERIFY for trainable work is the V20 failure mode (json/stub passed as phase-done).

**Exception — SK-QA is mandatory-additional.** Whenever a work order writes or changes code, a trainable unit, a host/extension entrypoint, OR makes any runtime/e2e claim, SK-QA is injected IN ADDITION to the cap (alongside SK-VERIFY) and is NEVER dropped. SK-TEST covers unit+integration+suite; SK-QA additionally forces e2e/host, QA automation, negative, and the two-PASS untrained/trained expectations (OP-55/OP-56/OP-57). A code/host/trainable work order without SK-QA has no declared test strategy — the testability-gap failure.

**Exception — Dual-Path Host Verification is mandatory-additional.** Whenever a
plan, review, work order, or completion claim touches extension/runtime/product
readiness, inject SK-QA, SK-VERIFY, and `plugin-runtime-reality-gate-SKILL.md`,
then require the Dual-Path Host Verification Rule. The packet must include
Automated boundary path and Local
desktop path rows. JSON-only, adapter/core-only, test-only hook,
process/window-list, or synthetic helper UI cannot replace Local desktop proof;
screenshot-only cannot replace Automated boundary proof. A blocked Local
desktop path downgrades the claim and is not a stop gate.

**Exception - SK-ARCH is mandatory-additional.** Whenever work mentions learned,
semantic, trainable, adaptive, DPO, selector, router, classifier, scorer, KNN,
dictionary, counter, pattern matching, `TryDecide*`, `ChooseBestPattern`,
`SelectBestPattern`, or `CountMatchingSignals`, SK-ARCH is injected IN ADDITION
to the cap and is NEVER dropped. It runs before repair and before accepting a
plan, review, code slice, dataset, or completion claim.

For learned dictionaries/counters/maps, SK-ARCH must include the
Dictionary-to-`ILogicalUnit` replacement matrix from
`trainable-architecture-decision-SKILL.md`: one row per field/method with
replacement unit, typed state, KNN/nearest role, DPO sources, license gate,
pair-building rule, user/cache load first, repo/common fallback second,
backup/export/import/share, continue-training from checkpoint, affected sets,
and tests. A sub-agent packet that says only "wrapped in ILogicalUnit" is
invalid.

Rationale: injecting more than 3 core candidate blocks truncates the core instructions in the context window.

---

## Activation Rules (MACHINE — not configurable)

```
SK-VERIFY: trainable ILogicalUnit/ILearningMachine work  OR  any phase-done/completion claim on an algorithm/model  OR  generated-code review  → ALWAYS (priority 0, exempt from the 3-block cap)
SK-QA:    code/contract change  OR  trainable unit work  OR  host/extension entrypoint  OR  any runtime/e2e claim  → ALWAYS (mandatory-additional, exempt from the 3-block cap)
SK-ARCH: learned/semantic/trainable/adaptive/DPO claim OR scorer/selector/router/classifier/KNN/dictionary/pattern matcher OR Luba deterministic-shortcut question -> ALWAYS (priority 0, exempt from the 3-block cap)
SK-DNA:  new class being added  OR  dna_compliance check score < 0.7
SK-FAB:  new interface being added  OR  new DI dependency chain
SK-PLAN: iteration ≤ 2  OR  phase boundary (phase transition)
SK-REVIEW: plan review OR user correction OR shallow planning/review challenge OR benchmark/depth comparison OR final readiness sweep -> enforce Whole-Plan Contradiction Sweep, final Open Decisions Queue, supplement disposition, explicit `claude-*`/`codex-*`/`dcm-*` quarantine, population proof with `evidence_depth_score`, and source-disjoint evidence when applicable
SK-PLAN + SK-REVIEW: plan creation OR execution status OR complaints about phase order/current location/remaining work OR `phase.step.status`/`done in-progress remaining` vocabulary -> enforce Sequential Execution-Control Gate
SK-TEST: test coverage incomplete  OR  bug-fix (always needs test first)
SK-DOCS: last step of session  OR  canonical doc sync triggered
```

---

## Selection Algorithm

```csharp
// Maximum 3 core candidate blocks per injection — priority order when > 3 core candidates
// Priority: DNA > FAB > PLAN/REVIEW > TEST > DOCS

static IEnumerable<string> SelectSkillsForContext(GenerationContext ctx)
{
    // Mandatory-additional blocks are not part of the top-3 competition.
    // Append all triggered mandatory-additional blocks after selecting core:
    // SK-VERIFY, SK-QA, SK-ARCH, SK-INTENT, and SK-DPO when their triggers apply.

    // Collect candidates based on activation rules
    var candidates = new List<(int priority, string skillId)>();

    if (ctx.IsNewClass || ctx.DnaComplianceScore < 0.7)
        candidates.Add((1, "SK-DNA")); // Highest priority — DNA violations corrupt output

    if (ctx.IsNewInterface || ctx.HasNewDependencies)
        candidates.Add((2, "SK-FAB")); // Second — fabric violations are hard to detect later

    if (ctx.Iteration <= 2 || ctx.IsPhaseTransition)
        candidates.Add((3, "SK-PLAN")); // Third — prevents artifact collisions

    if (ctx.IsPlanReview || ctx.IsUserCorrection || ctx.IsPlanningQualityChallenge)
        candidates.Add((3, "SK-REVIEW")); // Sibling to PLAN — prevents shallow review

    if (!ctx.TestCoverageComplete || ctx.IsBugFix)
        candidates.Add((4, "SK-TEST")); // Fourth — coverage gaps are recoverable

    if (ctx.IsLastStep || ctx.DocSyncTriggered)
        candidates.Add((5, "SK-DOCS")); // Lowest — doc sync can run after generation

    // Compute top 3 core candidates by priority.
    var core = candidates
        .OrderBy(c => c.priority)
        .Take(3)
        .Select(c => c.skillId);

    // Compute all triggered mandatory-additional blocks, exempt from the cap.
    var mandatoryAdditional = TriggeredMandatoryAdditional(ctx);

    // Return deduped merge: capped core + all triggered mandatory-additional.
    return core
        .Concat(mandatoryAdditional)
        .Distinct();
}
```

---

## When to Inject Per Task Type

| Task | Inject |
|------|--------|
| New class from scratch | SK-DNA + SK-FAB + SK-TEST |
| Bug fix | SK-TEST + SK-DNA |
| Phase planning | SK-PLAN + SK-DNA |
| Plan review or user correction | SK-REVIEW + SK-INTENT + SK-PLAN |
| Architect work order to sub-agent | SK-PLAN + SK-FAB |
| Session end | SK-DOCS |
| Interface change | SK-FAB + SK-DNA |
| Generated code (SemanticCompiler / trainable unit) | SK-DNA + SK-TEST + SK-VERIFY |
| Trainable ILogicalUnit/ILearningMachine implementation | SK-DNA + SK-FAB + SK-VERIFY + SK-ARCH |
| Learned selector/scorer/router/classifier/KNN/dictionary/pattern matcher review | SK-ARCH + SK-VERIFY + SK-DNA |
| Learned dictionary/counter/map replacement plan or implementation | SK-ARCH + SK-DPO + SK-QA + SK-VERIFY |
| Architect work order to sub-agent (trainable/algorithm) | SK-PLAN + SK-VERIFY + SK-ARCH (+ SK-FAB, + SK-QA/+SK-DPO when triggered) |
| Phase-done / completion claim on an algorithm or model | SK-VERIFY (always) |
| Host/extension entrypoint (Claude/Cursor/Codex) | SK-QA + SK-VERIFY |
| Any code/host/trainable work order (test strategy required) | SK-QA (always) |

---

## Work-order skill + guide binding (AGENTS.md OP-53, OP-55)

For learned/scorer/selector/router/classifier/KNN/dictionary/pattern-matcher
work, the work order must enumerate SK-ARCH and require an allowed-hierarchy
classification in the returned evidence packet. A packet without that
classification is invalid even when SK-VERIFY and tests pass.

A bounded sub-agent work order MUST enumerate (a) the exact skills it must load and apply, (b) the required guides, and (c) the required injection blocks (e.g., SK-VERIFY for trainable/algorithm/phase-done work). The returned evidence packet MUST confirm which skills, guides, and blocks were loaded and applied. A trainable-unit packet without SK-VERIFY loaded and its Layer-5 numeric metrics is an invalid packet — the architect rejects it. This closes the V20 gap where executors never received the anti-stub rulebook.

For any trainable/DPO/graph/learning-signal work order, SK-VERIFY binding
additionally requires the returned packet to carry the applicable
generated-code-review Layer 5B-5F rows AND implementation-integrity Check 15
rows, not only Layer-5 numbers. The work order must enumerate, and the packet
must confirm, whichever of these apply to the slice:

```text
reward_scalarization_for_non_scalar_output (Layer 5B): PASS | N/A - reward(side) extractor file:line
winner_vs_losers_cardinality (Layer 5C): PASS | N/A - N, pair rule, acceptance-signal independence
determinism_primitive_named (Layer 5D): PASS | N/A - named hash/PRNG/sort primitive; no String.GetHashCode
seam_sourced_extractor_present (Layer 5E): PASS | N/A - per-field parser file:line + unknown-format branch
cross_file_single_owner_consistency (Layer 5F / Check 15.1): PASS - owner name/level/IO match, no dangling pointer
provenance_presence_by_current_locked_count (Check 15.2): PASS - denominator == current locked field count on all carriers
per_level_ablation_with_number (Check 15.4): PASS - per-domain scorer+gold + ablation number for each trainable level
reward_and_intake_join (Check 15.5): PASS - each signal source joins the loop or is out-of-scope
legal_teacher_accept_and_license_coherence (Check 15.6): PASS | N/A - ACCEPT algorithm + family->canonical_license cross-check + per-family ProviderConfig-or-deferred
seed_dataset_existence (Check 15.7): PASS | BLOCKING_DEPENDENCY_RECORDED - verified path/volume, recon-backing != data-backing
```

A trainable packet that returns Layer-5 numbers but omits the applicable
Layer 5B-5F / Check 15 rows is invalid; the architect rejects it and sets the
next action to the first missing row. A code/host/trainable work order MUST additionally enumerate SK-QA, the required_testing_guides, expected_test_layers, and the untrained/trained behavior expectations (AGENTS.md OP-34, OP-55, OP-56, OP-57); a packet that returns no test_layer_results for the declared layers is invalid. Cross-reference: AGENTS.md OP-53, OP-55, OP-56, OP-57; plan-review-SKILL.md FC-25, FC-26, FC-27, FC-28; architect-behavior-classifier-SKILL.md FAIL-32, FAIL-34, FAIL-35, FAIL-36; CLAUDE.md "Executor skill injection + trainable completion wiring pattern", "Plan testability gate pattern", "Untrained honesty vs trained readiness pattern".

---

## Bounded work-order boundary injection

Every bounded sub-agent work order must include the boundary-control fields
alongside the skill list:

```text
CURRENT TASK:
NOT THE TASK:
EVIDENCE-ONLY:
requested_artifact_type
allowed_write_scope
exact_current_luba_quote
active_task_sentence_source
quoted_transcripts_state_old_plans_are_evidence_only=true
required_skills_guides_and_injection_blocks
Benchmark Startup Packet present=true when benchmark/depth/focus repair is in scope
Benchmark Delta Report required=true when benchmark evidence is used
current_task_focus_retention_ledger required=true when benchmark evidence, quoted transcripts, old plans, or sub-agent packets are present
current_task_focus_retention_ledger accepted_benchmark_item/current_task_mapping/not_the_task_exclusion_check/evidence_only_source/decision_effect required=true when benchmark deltas are accepted
Existing Artifact Inventory / Conflict Policy required=true when old plans, foreign artifacts, or benchmark artifacts are present
Benchmark Authority Ledger required=true when benchmark depth patterns affect a guide/plan/review
accepted_rejected_deferred_enrichment_ledger required=true when Benchmark Delta Report is required
compatibility_conflict_non_conflict_table required=true before accepting benchmark additions
human_readable_status_required=true
human_readable_progress_board_required=true
human_readable_progress_board_fields = total_major_plan_blocks_or_current_phase_tasks, completed_count_and_names, current_active_task_and_owner, remaining_count_and_names, current_result_type, claims_still_closed_and_why, next_bounded_action
human_readable_phase_step_status_template_required=true
human_readable_phase_step_status_fields = phase_number_and_human_name, phase_plain_language_problem_solved, step_number_and_human_name, step_plain_language_problem_solved, plain_language_status_translation, what_found, what_fixed, what_remains, phase_done_remaining_counts, total_done_remaining_counts
bare_phase_step_status_for_luba_forbidden=true
technical_labels_require_same_update_translation=true
machine_labels_secondary_only=true
exact_review_scope_required=true
review_scope_must_name_files_sections_hosts_and_claims=true
code_review_completeness_scope_required_for_code_slices=true
Model/Reasoning Configuration Gate
requested_model = gpt-5.5
requested_reasoning_effort = xhigh
runtime_model_or_unavailable = <visible runtime/request metadata or unavailable>
runtime_reasoning_effort_or_unavailable = <visible runtime/request metadata or unavailable>
mode_verified_from_work_order = true only when exact runtime metadata is unavailable and no concrete contradictory runtime/request value is visible
downgrade_authority = explicit current Luba quote only
downgrade_warning_required = true
state_log_record_required = true
allowed_verification
Scenario prep NEEDS_FIX blocks executor work order:
  prep_review_status = PASS required before UI/runtime/product scenario execution
  prep_review_status = NEEDS_FIX blocks executor run command
  plan not closed/readable/exact is repair loop
  human-readable operator-language scenario prep required before execution
  exact phase/step meaning required
  exact app/window target required
  login/session preflight required
  exact clicks/typing/expected visible state required for UI scenarios
  screenshot/video/evidence capture moments required
  two-monitor/window-overlap fallback required
  no secrets captured required
  Automated Path A exact commands or exact callable script entrypoints required
  Automated Path A expected output and exact evidence filenames required
  vague commands forbidden: "test host UI", "run scenario", "run UI", "continue scenario"
  independent prep review PASS required for the same scenario
Dual-Path Host Verification when extension/runtime/product readiness is in scope:
  Automated boundary path: adapter/API/core request-response; model logic; DPO/training; token measurement; quality scoring; backup/export/import/share/continue-training/rollback; checkpoint/locator gates
  Local desktop path: agent on local computer in real installed host; launch/activate real app; visible marker; screenshot/video/log capture; saved evidence path; exact blocker if UI control is impossible
  JSON-only/adapter-core-only/test-only/process-window-list/synthetic-helper-UI cannot replace Local desktop proof: true
  screenshot-only cannot replace Automated boundary proof: true
  blocked Local desktop path downgrades claim and is not a stop gate: true
  human report fields: phase.step, Automated boundary status, Local desktop status, evidence, remaining, downgraded claims
forbidden_broad_verification
commit_push_boundary_owner = parent_after_review | separate_commit_only_work_order | allowed_with_exact_quote
work_order_scope
exact_allowed_files
High-Risk Architecture Recon Library Gate required=true when the plan is high-risk architecture, ILearningMachine/DynamicGraph, or cross-repo/source-corpus design
Knowledge Library artifact required=true when High-Risk Architecture Recon Library Gate is required
Graph/DynamicGraph Architecture Reconstruction required=true for graph-like trainable systems
DPO/distillation Legal Teacher Provenance Gate required=true when teacher outputs may train or rank a model
implementation_authorized
plan_only_deliverable
plan_delivered_to_luba
luba_acceptance_quote
execution_authorized_now
git_authority = forbidden | parent_only | allowed_with_exact_quote
commit_push_authorized
allowed_git_commands
forbidden_commands
parent_will_perform_commit_push_after_review=true for normal artifact/code/content slices
may_derive_next_phase=false unless explicitly granted
must_stop_after_report=true
parent_review_required_after_return=true
parent_direct_write_forbidden_when_subagent_only=true when Luba restricts parent/current session to architect-only, sub-agent-only, or non-executor
subagent_authorship_required_for_governed_artifacts=true when repair/execution is requested through subagents
returned_packet_must_name_subagent_id=true
returned_packet_must_list_changed_files=true
parent_direct_apply_patch_or_shell_write_forbidden=true when parent_direct_write_forbidden_when_subagent_only applies
direct_repair_reflex_must_be_converted_to_subagent_work_order=true
parent_self_review_does_not_count_as_subagent_pass=true
parent_local_sidecar_work_forbidden_while_waiting=true
parent_read_governed_files_before_packet_return_forbidden=true
parent_read_only_verification_allowed_only_after_packet_return=true
firewall_repair_exact_file_verification_required=true
```

For plan/review/governance/skill/documentation deliverables,
implementation after the deliverable is forbidden when `luba_acceptance_quote`
is `NONE`, old, quoted, or pre-delivery; `execution_authorized_now` may be true
only after a fresh post-delivery Luba acceptance quote explicitly accepts the
artifact and starts implementation.

If a bounded sub-agent work order omits `requested_model=gpt-5.5` or
`requested_reasoning_effort=xhigh`, executor/reviewer authority is not granted.
If visible runtime/request metadata or explicit work-order metadata differs from
those requested values, including medium/high/low/default reasoning instead of
xhigh, and there is no explicit current Luba downgrade quote, the returned
packet must name the run as a governance violation, emit a user-visible warning,
record state/log evidence, reload the model/reasoning repair rules, and stop
before product execution continues.

Generic system/developer identity text such as "GPT-5" is not downgrade
evidence by itself. If exact runtime metadata is unavailable but the work order
explicitly requests `gpt-5.5` / `xhigh`, continue and record
`mode_verified_from_work_order=true` unless a concrete contradictory
runtime/request value is visible. A stop based only on generic identity text is
a false downgrade stop and premature stop loophole.

When the work order delegates a review/fix pass, it must also inject this packet
schema:

```text
delegated_review_fix_pass_packet:
  requested_pass_floor:
  pass_number:
  reviewer_or_subagent_id:
  artifact_before_ref:
  artifact_after_ref:
  loaded_skills:
  loaded_gates:
  inspected_scope:
  exact_review_scope:
  human_readable_status:
  progress_counts:
    total:
    completed_count_and_names:
    current_active_task_and_owner:
    remaining_count_and_names:
    current_result_type:
    claims_still_closed_and_why:
    next_bounded_action:
  findings:
  fix_refs_or_no_fix_justification:
  recheck_evidence:
  requested_model: gpt-5.5
  requested_reasoning_effort: xhigh
  runtime_model_or_unavailable:
  runtime_reasoning_effort_or_unavailable:
  mode_verified_from_work_order:
  downgrade_warning_or_NONE:
  state_log_record_ref:
  parent_review_required_after_return=true:
```

The sub-agent may update a Review-Fix Ledger only as an index to the packet.
It must not claim that N passes are complete from N ledger rows unless N valid
packets exist. Missing packets make the returned evidence invalid.

When the delegated pass is one of the 10 Review Cycle Gate passes, the packet
must additionally name the exact plan section reviewed and whether the pass
proves that Claude, Cursor, and Codex each become full extensions. When the
delegated pass is one of the Final Execution Review Gate passes, the packet
must additionally prove or fail the rows for three full extensions, no hidden
deterministic mechanisms, `llm_mvp_core` core ownership, local trainable user
model lifecycle, DPO quality, tests/e2e/QA, docs, and live runtime evidence.

When the delegated pass reviews a code slice, the work order and packet must
name the exact code-review scope and explicitly check project coding standards,
allowed tests, `llm_mvp_core/core` ownership for core logic, thin host adapters,
hidden deterministic shortcut sweep, deterministic helper labels
(eval-only/helper/internal baseline when present), and docs/QA/dashboard impact
when applicable.

A sub-agent packet that covers files, phases, commits, pushes, runners, model
assets, or state transitions outside `work_order_scope` is invalid evidence
even if all skill blocks were loaded. A sub-agent-created `parent-review` file
is evidence only; it does not satisfy parent review until the parent architect
reads the artifacts and creates or accepts the review after executor authority
expires.

When `parent_direct_write_forbidden_when_subagent_only=true`, the work order
must state that parent direct `apply_patch`, shell file-writing, generated
rewrites, or equivalent file-writing tools are forbidden for governed artifact
authorship. Governance, skill, protocol, content, plan, report, data, and code
repairs requested through sub-agents must be authored by the bounded sub-agent.
The returned packet must identify the sub-agent id and changed files so parent
review and completion packaging can fail any parent-authored substitute.
The work order must also state that a parent "quick direct repair" instinct is
a role-firewall failure, not a shortcut. If review/fix passes are requested,
only real packets from the named sub-agent or reviewer count; parent
self-review under sub-agent-only is zero delegated passes.
The work order must also state that the parent cannot do local sidecar research
while waiting: no reading governed files, skill indexes, plans, code, reports,
or state for loophole analysis, repair drafting, or non-overlapping substantive
work before the sub-agent packet returns. Parent read-only verification is
after-return verification only and must be tied to the returned packet.

Parent Architect Subagent-Only Firewall compatibility: injected work orders
must preserve exact phrases: `parent local sidecar work ban`,
`read-only verification after a sub-agent packet returns`,
`sub-agent waiting is not a stop`, and `no local non-overlapping work escape`.

When `commit_push_authorized=false`, SK-PLAN/SK-VERIFY review must confirm that
`git_authority` is `forbidden` or `parent_only`, and `forbidden_commands`
explicitly blocks `git add`, `git commit`, `git push`, `git checkout`,
`git reset`, `git branch`, `git tag`, `git merge`, `git rebase`, and any git
write/ref command. Read-only git commands may appear only under
`allowed_git_commands`. A packet that ran git write/ref commands without this
authority is invalid and must be quarantined pending parent validation.

For artifact-producing, code-producing, data-producing, documentation-producing,
report-producing, plan-producing, and governance-producing slices, inject
`git_authority=forbidden` and `allowed_git_commands=NONE` by default. The
sub-agent returns evidence and stops; parent review performs repository checks
and then performs the commit/push boundary or launches a separate commit-only
work order.

Commit/push authority is injected only into a commit-only work order. That work
order must forbid artifact/code/content edits, list the exact file set to stage,
list the exact commit message, and forbid next-phase derivation. If a sub-agent
emits git directives without exact authority, SK-PLAN/SK-VERIFY review fails the
packet even if the artifacts themselves are valid.

Every returned sub-agent packet must include:

```text
git_authority_seen
git_commands_run
git_write_or_ref_commands_run
git_directives_emitted
```

The claim "sub-agent completed bounded pass" is false if the packet or
notification shows forbidden git use, out-of-scope files, or collapsed required
passes without parent approval. Parent may independently accept useful artifact
content after readback, but must record the process violation and repair
governance before relying on the same delegation pattern again.

### Context boundary for bounded sub-agents

For artifact-producing, code-producing, data-producing, documentation-producing,
report-producing, plan-producing, and governance-producing work orders, inject a
clean/minimal task context by default. Do not use a full-history fork unless the
current human instruction explicitly requires it and the work order explains why
the inherited history cannot override the boundary.

Required field:

```text
subagent_context_mode = clean_minimal | full_history_exception
```

`clean_minimal` means the agent receives the current work order, exact files,
required skills/guides, and no old continuation/commit/push authority.

`full_history_exception` is allowed only when explicitly justified. In that
case, the work order must say:

```text
old_history_authority = evidence_only
state_next_action_authority = false
prior_continue_or_commit_authority = false
work_order_fields_are_only_authority = true
```

If a full-history agent commits, pushes, stages files, derives the next phase,
or treats old state/plan history as authority despite these fields, the packet
fails the boundary review. The artifact can be accepted only after independent
parent validation, and the next bounded write work order must use
`subagent_context_mode=clean_minimal`.

---

## Context Hygiene

Each generation/planning step receives a CLEAN context — skill blocks are injected fresh,
not accumulated from previous steps.

Do not carry over skill content from step to step. Re-inject the relevant blocks at each step.

Old effectiveness data informs which blocks to select, but the blocks themselves are always
injected fresh.

---

## Effectiveness Tracking

After each session, note which skills were active and whether DNA/test gates passed:

```json
{
  "skillsActive": ["SK-DNA", "SK-FAB"],
  "dnaCheckPassed": true,
  "testGatePassed": true,
  "testCountDelta": 8,
  "notes": "SK-PLAN not injected — iteration 3, not phase transition"
}
```

This feeds back into the activation rules: if SK-DNA is consistently effective, keep its
priority at 1.

---

## SK-INTENT mandatory-additional block

`SK-INTENT` means:

- `conversation-intent-and-self-reflection-SKILL.md`
- `architect-behavior-classifier-SKILL.md`

It is mandatory-additional and exempt from the normal skill-block cap whenever
the task is architect planning, plan review, governance/protocol/skill repair,
sub-agent orchestration, or any current Luba message that asks why, what
allowed, where the gap is, whether the plan was written, why the output was
shallow, whether settings/skills are missing, or says to return to analysis.

It is also mandatory when the message is mixed:

- small action plus analysis;
- add/write/fix plus "return to analysis";
- many files/guides/prompts/plans plus a request to synthesize or plan.

Required SK-INTENT packet before action:

```text
conversation_intent_review:
  primary_intent:
  direct_question_present:
  selected_mode:
  first_response_required:
  forbidden_actions:
  settings_gap_hypothesis_required:
  action_as_relief_risk:
  safe_to_write_or_patch:
  entropy_detected:
  order_making_needed:
  attention_budget_required:
  primary_work:
  action_budget_percent:
  analysis_budget_percent:
  smallest_sufficient_action:
  must_return_to_analysis_before_final:
```

If SK-INTENT fires and is absent from a work order or review packet, that packet
is invalid. If `primary_work=analysis`, a permitted edit is only a preservation
step; the response must return to analysis before it closes.

---

## SK-DPO mandatory-additional block

`SK-DPO` means:

- `dpo-triple-format-SKILL.md`
- `planning-dpo-authoring-SKILL.md`
- `DPO-KNOWLEDGE-MAP-v1.md`
- the domain-specific DPO guide named by that map.

It is mandatory-additional and exempt from the normal skill-block cap whenever
the task mentions DPO, trainable units, adaptive user learning, model upgrade,
training data, rejected examples, pair registries, or model readiness.

Required SK-DPO packet:

```text
dpo_knowledge_binding:
  domain:
  loaded_dpo_skills:
  loaded_domain_guides:
  pair_schema:
  rejected_taxonomy:
  registry_fields:
  train_ready_gate:
  held_out_or_fresh_load_gate:
```

If a trainable/adaptive work order does not list SK-DPO and the domain guide, it
is invalid even when code compiles or a report says passed.

---

## Anti-Patterns

1. **Injecting every available block every step** — select a small core set plus triggered mandatory-additional blocks; injecting everything causes context overflow and truncates core instructions
2. **Not injecting SK-DNA for new classes** — DNA violations in generated code are hard to find later
3. **Carrying skill context between steps** — accumulation pollutes later steps with stale guidance
4. **Skipping injection because "I know the rules"** — systematic injection prevents drift
5. **Omitting SK-VERIFY for trainable-unit or phase-done work** — lets json/stub deliverables pass as phase-done with only build+contract+DNA evidence (the V20 failure). SK-VERIFY is mandatory-additional, never dropped.
6. **Omitting SK-QA for code/host/trainable work** — lets a plan/slice ship with no declared test strategy (no e2e/host, no untrained/trained split). This is the testability-gap failure (OP-55). SK-QA is mandatory-additional, never dropped.
