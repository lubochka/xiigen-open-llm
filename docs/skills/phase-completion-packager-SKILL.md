# Phase Completion Packager

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

## Fresh Execution / Method / Humility Packaging Hook

Packaging must not turn old packages into fresh completion:
- `REPEATED_CURRENT_COMMAND_IS_FRESH_EXECUTION_GATE`: repeated current command is a fresh execution trigger; old artifacts are baseline evidence, not completion.
- `TOO_FAST_HIGH_PASS_SUSPICION_GATE`: too-fast 50-pass completion is suspicious until per-pass sub-agent evidence is accepted; package summaries need ordered evidence for N-pass or large-copy work.
- `LARGE_VOLUME_MODEL_HUMILITY_AND_FOCUS_GATE`: large-volume model humility requires bounded slices and focus ledger before completion.
- `EXPLICIT_METHOD_OVERRIDE_GATE`: Luba's method/model/reasoning/count/folders/target/sub-agent route is binding; a package with matching content but wrong method remains NEEDS_FIX.
- `FINAL_ANSWER_FRESHNESS_GATE`: package/final text must say freshly executed this turn vs only verified and answer the latest command.
- `CURRENT_REPEATED_PACKAGE_COMMAND_GOVERNANCE_ADDENDUM`: package closure must reject repeated extension-development package closure by old artifact verification instead of a fresh execution chain; too-fast N-pass/status-package closure without ordered packets/records; missing large-volume model humility focus ledger with exact current task, not-the-task, files, pass/review counts, active worker, remaining actions, and latest-command freshness; parallel workers when Luba flags parallel/too-fast or requires exact-count sub-agent work unless her current unquoted message authorizes parallelism; interrupted or zero-work spawned-worker packets using `NONE` instead of `subagent_id: UNKNOWN_IN_SUBAGENT` when platform id is hidden; parent local sidecar work during waiting; missing human-readable phase counts before execution continuation; and final/status without latest-command, fresh-vs-old execution, actual packets/records, no parent self-review counted, and no product-readiness overclaim.
- `COMMAND_SCOPE_EXACTNESS_GATE`: packaging must reject current-task command violations. Current `allowed_commands` and `forbidden_commands` are binding; read-only convenience commands, alternate git diff modes, external AI/web, `codex.exe`, nested sub-agents, branch/git mutations, or shell variants outside the current allowed list are violations unless Luba's current unquoted instruction explicitly authorizes them. Evidence from an out-of-scope command is quarantine evidence only and cannot support completion.
- `CODE_IMPLEMENTATION_TRACE_PACKAGE_GATE`: phase packaging must not package a
  code/product/runtime/model task as implemented unless the packet includes an
  `Implementation Evidence Map` and an `Independent Verification Work Order`.
  Package output must separate `artifact/status/documentation progress` from
  `code implementation progress`. If the code trace is missing, the package may
  say only that artifacts were produced and must list the next code-verification
  or implementation slice.

## Status Package Is Not Product Progress Gate

Completion packages must separate two ledgers:

```text
documentation_status_progress:
product_implementation_progress:
```

`documentation_status_progress` may include plans, ledgers, reviews, packages,
governance repairs, skill repairs, and status summaries.

`product_implementation_progress` may include only fresh product evidence:
code changes, test changes, passing focused/full tests, runtime/browser/QA
proof, build proof, persisted data/checkpoint proof, or a proven true external
blocker after mitigation.

A package that contains only documentation/status progress must say
`product_implementation_progress: 0` and must name the next implementation
slice when product work remains. It must not psychologically or structurally
close the phase just because the package is well formatted.

Required package fields when any product gap remains:

```text
next_implementation_slice:
files_to_modify:
tests_to_add_or_run:
runtime_or_user_scenario_to_prove:
owner_subagent_or_recovery_worker:
```

Verification literal:
`Status package is not product progress unless it changes product evidence.`

## Independent Audit Union Packaging Gate

A phase package that summarizes Fable, Opus, Claude, benchmark, or other
independent audit findings must not treat the map, counts, or dedup table as
delivery. The package must separate audit coverage from product implementation.

Required package fields when independent audit findings remain:

```text
INDEPENDENT_AUDIT_UNION_TO_PRODUCT_BOARD_GATE:
  audit_union_board_ref:
  source_tags_preserved: BOTH | FABLE_ONLY | OPUS_ONLY | CLAUDE_ONLY | BENCHMARK_ONLY
  union_backlog_wins_over_intersection_only_closure: true
AUDIT_STATS_ARE_NOT_PRODUCT_PROGRESS_GATE:
  documentation_status_progress:
  product_implementation_progress:
  audit_counts_are_evidence_coverage_only: true
EVERY_UNIQUE_AUDIT_HOLE_BECOMES_PRODUCT_SLICE_GATE:
  required_row_fields: capability, files_or_modules_to_inspect, files_or_modules_to_change, tests_or_proof_to_add_or_run, expected_product_behavior, source_audit_tag, owner_worker, acceptance_evidence
SINGLE_MODEL_BLIND_SPOT_GATE:
  only_fable_or_only_opus_rows_open_or_proven: true
SECURITY_OPS_LIFECYCLE_ARE_PRODUCT_GAPS_GATE:
  security_ops_lifecycle_rows_packaged_as_product_gaps: true
REAL_TRAINER_ROUTE_B_GATE:
  trainer_route_b_checkpoint_lifecycle_row_present: true
POST_GOVERNANCE_RETURN_TO_PRODUCT_GATE:
  next_product_slice_after_governance_repair:
```

Counts such as 177 unique holes, 88 both, 53 Fable-only, 36 Opus-only, 117/133
Fable findings, or 127/121 Opus findings belong in
`documentation_status_progress`. They become `product_implementation_progress`
only after the package includes fresh code change, test change, runtime/browser
proof, build/test proof, persisted data/checkpoint proof, or a true external
blocker certificate for the specific row.

After governance-only repair, the package must say product progress is 0 and
name the next product slice: live-local startup crash caused by `artifact://`
fake PNG after stricter screenshot proof; preserve fake-proof rejection and use
deterministic local PNG or bounded non-ready, then run focused proof.

`PRODUCT_CODE_CONTINUATION_ONLY`: after governance/skill/rule/agent-file repair
for a live product/code/QA task, the package must not close with another
governance worker, read-only scan, status package, `NO_PATCH`, rule readback, or
packet-formatting repair as continuation evidence. It must name the product/code
QA execution worker or narrowed recovery worker for the concrete
`audit_union_row_id` or `next_product_code_slice`, or record a true external
blocker certificate. `PRODUCT_WORKER_NO_PACKET_ZERO_ACCEPTED_REQUIRES_RECOVERY`:
`shutdown`, `not_found`, interrupted, no packet, or 0 accepted product evidence
from the product/code worker is zero accepted product work and requires a
narrowed product/code recovery worker before final/status.

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

## Purpose

When a phase gate passes, this skill defines the exact procedure: what three
artifacts to produce, in what order, what each must contain, and whether to
continue or stop according to the active plan. Continuous plans continue instead
of turning the package into a hidden pause.

---

## Gate Model — Read STATE-Pn.json First

Before running any gate check, read the phase mode from STATE-Pn.json:

```
parallelWork: null  → SEQUENTIAL mode (one subproject at a time)
                      Gate: dotnet test total ≥ {absolute_baseline}

parallelWork: N     → PARALLEL mode (multiple subprojects in one session)
                      Gate: dotnet test --filter "[SubprojectName]" delta ≥ {expected_delta}
                      Absolute baseline verified after merge — not here.
```

Never apply the absolute gate in parallel mode. The absolute baseline will
have moved as other work was merged. Count only your delta.

## Continuous Plan Exception

Some Luba-directed plans explicitly forbid stopping between internal roles or
phase boundaries while the full goal remains open. In those plans, the package is
a checkpoint, not a stop gate.

Before writing any STOP line, check the active plan and state:

```text
goal_reached == true
next_action is empty
completion coverage matrix exists when the plan requires it
completion coverage matrix has zero gaps
Luba did not ask to continue / do not stop
```

If any condition fails, do not write a STOP instruction. Write
`IN_PROGRESS_NEXT_STEP_KNOWN`, update state with the next plan-derived action,
and continue.

For sub-agent-only governance repair, the repair itself is not permission to
stop when Luba's requested outcome remains open and the next action is known.
After the role-firewall repair is packaged, the parent must continue the
requested plan repair, plan review, or governance review through bounded
sub-agents unless Luba's latest unquoted instruction explicitly says to stop or
answer only.

Why Question Root-Cause Repair Trigger packaging: when the package follows a
"why/why did you stop/why did this happen/pochemu/почему" failure-cause
question, why is not a stop. The package must prove the response answered first
in human language, diagnosed the exact missing/weak/contradictory rule, did
repair skills/guides/agent files before continuation through the active role
boundary, and will explain human language then continue the nearest safe
already-authorized work. The human-readable packet must say what was wrong,
which files/rules changed, and what resumes next. Protocol repair complete is
not a completion boundary, and sub-agent waiting for the repair is not a stop.

No-Stop Sequencing Gate: when the current governance repair exists to unblock
three full Claude/Cursor/Codex extensions, the package must say the next
required action is 10 sub-agent plan review cycles before QSG/runtime/extension
execution continues. A governance patch alone is not a final stop.

No-unbounded-executor packaging gate: "do not stop" means package the completed
bounded slice, report what remains, and continue with the next parent-issued
bounded work order. It must not be reported as permission to keep one
executor/sub-agent running until product ready, until the whole plan is closed,
or until all remaining work is gone. A completion packet must include the exact
deliverable, max active time of 10 minutes or less, protocol/evidence report,
and next bounded action for parent review.

If packaging discovers an executor/sub-agent that was launched with an
unbounded "until product ready / until plan complete / do everything" scope,
the package must reject that execution shape. When the executor id is known,
the parent must interrupt/close it before accepting more output. When the id is
unknown, package all outputs from that executor as quarantine/evidence-only and
continue with a bounded replacement work order. This is not a stop for the chat.

A sub-agent that runs more than 10 minutes without a protocol/evidence packet
is over-scoped. The package must request or interrupt for the packet, close if
needed, split the remaining work, and state the next bounded action.
Deprecated `product readiness` wording must not be used as a completion packet
scope, primary status, status counter, stop reason, execution target, or
impossible full live host UI proof gate. Use bounded `Dual-Path Same-Scenario
Coverage` packets after accepted slices and reviews.

Verification literal: `dual-path scenario coverage is the primary status`.
Required replacement literals: `Dual-path same-scenario coverage is product readiness evidence`;
`full live host UI path is not the readiness definition unless explicitly re-approved by Luba`;
`NOT READY is a bounded sub-agent work queue, not a stop`;
`same scenario Path A plus Path B`;
`human-readable phase counts before execution continuation`.
If a package says NOT READY, not proven, blocked, needs fix, or full live host
UI path not proven, the package must include the next bounded sub-agent task or
a true external blocker with the mitigation/discovery attempt. It must not be a
final stop while authorized work remains.

The user-visible completion packet must start in Russian/plain human language:
total phases/steps, done/in-progress/remaining, what the phase solves, what the
current step solved, what evidence exists, what is not proven, and the next
bounded action. Machine labels, packet ids, STATE flags, and paths may follow
only after this human explanation.

---

## When to Invoke

After every phase gate passes — specifically after:
```
□ dotnet build LlmFromScratch.sln → 0 Error(s)
□ Gate passes (absolute or delta per parallelWork flag)
□ All phase-specific checks from the EXECUTION file pass
□ DNA compliance checks pass (dna-compliance-guard-SKILL.md)
□ TRAINABLE-PHASE GATE passes when the phase delivers a trainable unit (see below)
```

Never invoke if any gate check fails.

---

## Trainable-phase gate (anti-stub) — REQUIRED before status=DONE for any trainable/algorithm phase

Build + test-count + DNA is NOT sufficient to mark a trainable `ILogicalUnit`/`ILearningMachine` phase DONE. A json/stub unit passes all three. Before writing `status: DONE` in STATE for such a phase, REQUIRE all of:

1. **generated-code-review record** (generated-code-review-SKILL.md, Layers 1–5) = PASS, including Layer 5 (Trainable vs Stub) with numeric metrics.
2. **Runtime Evidence Firewall** (verification-before-completion-SKILL.md) = PASS: reject if evidence contains `validation_only: true`, `metric_type` static/contract/mixed, or `cpu_ms`/`ram_mb`/`accuracy` = UNKNOWN. Build/contract evidence proves only build/contract readiness.
3. **Numeric runtime metrics**, reproducible from the runner: accuracy/exact/loss/reward, cpu_ms, ram_mb, row counts; the unit must change its decision after training on new pairs (not exact-memory replay).

If any of the three is missing, the phase is NOT done: write `IN_PROGRESS_NEXT_STEP_KNOWN`, set `next_action` to the missing evidence, and continue. A trainable phase marked DONE on build+contract+DNA only is the V20 failure (`-1000`, loophole `trainable_stub_accepted_as_phase_done`). Cross-reference: AGENTS.md OP-53; architect-behavior-classifier-SKILL.md FAIL-32; plan-review-SKILL.md FC-25; injected via SK-VERIFY.

### Trainable Contract Reality rows - REQUIRED alongside the numeric metrics

Numeric metrics are necessary but not sufficient. Before `status: DONE` for any
trainable/DPO/graph/learning-signal phase, the package must also carry
`implementation-integrity-SKILL.md` Check 15 (Trainable Contract Reality Sweep)
= PASS, exposed as explicit rows so a reader can see each closed:

```text
cross_file_contract_consistency = PASS   (owner name/level/IO match; no dangling owner pointer)
provenance_field_presence_by_actual_count = PASS   (denominator == current locked count on ALL carriers)
single_owner_sweep_covers_all_carriers = PASS   (owner + consumers + diagrams + metric denominators + intake legs)
per_level_ablation_with_number = PASS   (each trainable level: per-domain scorer+gold instantiated, ablation carries a number)
reward_and_intake_join = PASS   (non-scalar output has reward-scalarization; each signal source joins the loop or is marked out-of-scope)
legal_teacher_accept_and_license_coherence = PASS | N/A   (ACCEPT match algorithm + family->canonical_license cross-check + ProviderConfig or explicit-deferred per active family)
seed_dataset_existence = PASS | BLOCKING_DEPENDENCY_RECORDED   (seed corpus EXISTS with verified path/volume, else recorded as a blocking design/dependency)
```

Any row MISSING/FAIL forces `IN_PROGRESS_NEXT_STEP_KNOWN` with that row as
`next_action`. `recommended default path != verified dataset`;
`reward primitive without reward(side) extractor != covered`;
`bare allowlist name != instantiated ACCEPT algorithm`;
`locked count in owner only != single-owner parity`. A phase packaged DONE with
numeric metrics but a stale provenance denominator, a dangling owner pointer, a
name-only ACCEPT family, or a seed corpus assumed as "recommended default" is a
false completion, not a limitation.

---

## Two-PASS status gate (OP-57) — which PASS may you write?

A trainable/algorithm phase has TWO distinct legitimate PASS states. Never write
a bare `status: DONE` for trainable work; write which PASS it is, and record it in
STATE (e.g. `passState`):

```text
PASS_AS_HONEST_UNTRAINED_SCAFFOLD
    Allowed when: contract/skeleton exists AND the OP-56 untrained-honesty tests
    pass (no semantic claim, honest not_ready, no deterministic answer dressed as
    smart, no fake DPO/checkpoint/metrics, honest host degrade).
    This is a real milestone. It is NOT a working feature. It must NOT satisfy a
    parent completion row that asks for the trained capability.

PASS_AS_TRAINED_FEATURE
    Allowed only with the full OP-57 evidence set: checkpoint + held-out numeric
    metrics + ablation (no trained state -> not ready) + fresh-load probe +
    continue-training probe + e2e through the real host entrypoint.
```

Writing PASS_AS_TRAINED_FEATURE (or bare DONE) on a path that only meets the
scaffold bar is the masquerade class: `-1000`, loophole
`two_pass_conflation_untrained_reported_as_trained`. Cross-reference: AGENTS.md
OP-56/OP-57; CLAUDE.md "Untrained honesty vs trained readiness pattern";
plan-review-SKILL.md FC-27/FC-28; architect-behavior-classifier-SKILL.md
FAIL-35/FAIL-36.

---

## Implementation Reality Gate - REQUIRED before status=DONE for code/runtime/plugin phases

Before writing `status: DONE` for any phase that claims real code, host
entrypoint, extension runtime, model asset readiness, trainable behavior, or
algorithmic implementation, require `implementation-integrity-SKILL.md` Check 5
PASS.

The completion package must include `implementationReality` with:
- actual code path file:line refs from entrypoint/runner to service/machine;
- artifact-class match proving JSON/PowerShell/reports are not substitutes;
- anti-stub sweep for TODO/FIXME/PLACEHOLDER/not_ready/no_inference/no-op paths;
- loaded skills/guides/injection blocks, including SK-VERIFY when applicable;
- runtime evidence for runtime/plugin claims;
- numeric held-out metrics for learned/trainable claims.

If missing, do not write DONE. Write `IN_PROGRESS_NEXT_STEP_KNOWN` and set
`next_action` to the first missing implementation-reality evidence row.

---

## UI/Dashboard completion gate - REQUIRED before final UI/dashboard DONE

For UI/dashboard phases where Luba asked for e2e or QA automation, the phase or
full-plan package must reject completion if full-interactive UI automation is
still open.

Required rows:

```text
full_interactive_ui_automation:
  PASS only when the running UI window/control/host entrypoint was driven and
  observed for the requested user journey and negative cases.

fallback_scope:
  View-model, handler-level, component, static QA, WPF build, script parity, or
  test-only adapter evidence is INTERMEDIATE only.

residual_gap_check:
  PASS only when state/report has no open full-interactive UI automation,
  UI e2e, WPF automation, or equivalent residual gap.

dashboard_scenario_ledger:
  PASS only when every new API/dashboard behavior introduced by the phase has a
  named automated scenario with human API name, example input, expected visible
  output, raw/debug expectation, negative case, and evidence scope.
```

If the real UI automation path is not feasible in the current slice, package the
narrow fallback artifact honestly, move product-decision/runtime-only work to
the end queue when needed, write `IN_PROGRESS_NEXT_STEP_KNOWN`, and continue any
independent plan-derived work. Do not write DONE, TASK_FULLY_COMPLETE, ready,
accepted, or promoted.

Dashboard QA automation proves the dashboard/API surface only. It must not be
used as proof of model semantic quality, trained-feature readiness, or
Claude/Cursor/Codex host runtime.

## Host Session Auth Preflight Packaging Gate

For any phase package that includes real host UI proof for Cursor, Claude,
Codex, or another signed-in host, packaging must include Host Session Auth Preflight and Browser Login Recovery before accepting the UI path. If Cursor opens anonymously, the host is logged out, the wrong account is visible, or
session state is unknown, the package cannot count the UI scenario yet. The
next action is browser login recovery through the host's normal sign-in flow,
then the Dual-Path Same-Scenario UI Proof: same scenario twice, automated path
and real host UI path.

Required package rows:

```text
host_session_auth_preflight:
  status: SIGNED_IN_READY | LOGIN_RECOVERY_REQUIRED | HUMAN_LOGIN_REQUIRED | AUTH_BLOCKED_WITH_EVIDENCE
  visible_signed_in_or_not_signed_in_proof:
  no secrets captured:
  browser login recovery run_or_required:
  correlated_following_ui_scenario:
  human_status_says_cursor_signed_in_anonymous_or_needs_browser_login:
```

The package must never include passwords, tokens, cookies, or secrets. If human
credential entry is required, package `HUMAN_LOGIN_REQUIRED`, keep readiness
claims closed, and resume only after the user completes login.

## No-Blind-Spot UI Scenario Script Packaging Gate

Completion packaging must reject any UI scenario package that lacks a
No-Blind-Spot UI Scenario Script and a 50-Pass Scenario Clarity Review ledger.
agent judgment is not a scenario step. The package must show exact
clicks/typing/expected UI state, the required qa/testing skills, exact
auth/session and browser login recovery branches, evidence capture moments,
Path A comparison, pass/fail/blocked meanings, and next smallest action for
each common blocked branch. A statement that a scenario was "reviewed" is not
enough; the human-readable ledger must contain 50 explicit checks or passes.

Plan-Not-Ready Is Repair Loop, Not Stop: if the script or ledger is missing,
plan not ready is not a stop. Write `IN_PROGRESS_NEXT_STEP_KNOWN`, do not execute an unready plan, and return to architecture/planning repair loop through
bounded subagents when required. The package must name exact clicks/typing/expected UI state before execution. Do not use "do not stop" to run an unready
scenario, and do not let protocol/skill repair substitute for repairing the
exact active scenario artifact.

## UI/UX completion gate - REQUIRED when UX is in scope

For UI/dashboard/API lab phases where Luba asked for UI/UX or QA UI/UX
automation, add this row to the completion package:

```text
ux_automation:
  PASS only when the running UI surface proves human-readable labels,
  accessible names/help text, examples, error clarity, safe raw/debug wording,
  history/copy/export clarity, stable layout/bounds, and no internal jargon on
  the primary path.
```

Do not substitute:
- click-only WPF/browser automation;
- catalog metadata alone;
- docs alone;
- a QA script that checks only file presence.

Missing UX automation keeps the package at `IN_PROGRESS_NEXT_STEP_KNOWN` even
when the functional UI automation row passed.

## Review/fix cycle gate

When Luba or the active plan requires several, multiple, or repeated review/fix
cycles, completion requires the exact requested floor. The project default is
two independent review passes only when Luba did not name a higher number. If
Luba says "minimum 4", "at least 4", or equivalent, the floor is four.

Before commit/push for repository-changing work when the floor is four:

```text
pre_commit_push_review_floor = 4
code_review_cycle_1 = PASS | ZERO_FINDINGS, with fixes/recheck evidence
code_review_cycle_2 = PASS | ZERO_FINDINGS, with fixes/recheck evidence
code_review_cycle_3 = PASS | ZERO_FINDINGS, with fixes/recheck evidence
code_review_cycle_4 = PASS | ZERO_FINDINGS, with fixes/recheck evidence
unfixed_findings_open = false
documentation_drift_open = false
```

Commit/push packaging must also prove verification did not fail before the git
boundary. Any printed failure from `git diff --check`, tests, JSON/JSONL parse,
schema checks, builds, or another required verification keeps the package
failed even if a later commit command succeeds. Native PowerShell wrappers must
check `$LASTEXITCODE` immediately after each required verification command and
abort before stage/commit/push on nonzero; command chains that continue into
commit/push after failed verification are forbidden.
Never chain `git diff --cached --check; git commit; git push` or any similar
semicolon sequence. PowerShell semicolon sequencing can continue after a failed
gate, so commit/push commands must be split or explicitly guarded, the precheck
exit code must be verified before commit, and a failed precheck starts a fix
loop rather than a commit boundary.
Before any commit, every pre-commit check such as `git diff --check`,
`git diff --cached --check`, JSON parse, text-format scan, screenshot privacy
review, and required test must run as a hard gate. commit is forbidden if a
pre-commit gate prints warnings/errors, even when a shell chain continues.
do not chain commit/push after a gate command unless exit code is checked and
the wrapper aborts on nonzero before staging, committing, or pushing. A
committed bad evidence packet requires an immediate fix commit and governance
repair, and the package report to Luba must name the exact failed gate and the
rule repair.
Required verification literals: commit is forbidden if a pre-commit gate prints warnings/errors; do not chain commit/push after a gate command unless exit code is checked.

Commit group packaging must also prove exact staging:

```text
required_commit_group = [exact reviewer/validation ledger artifact paths]
glob_used_only_as_supplement = true
required_validation_artifacts_include_dot_dash_variants = true
staged_files_command = git diff --cached --name-only
required_commit_group_missing_from_staged = []
```

Do not rely on a broad `glob` such as `quality-semantic-gates-24-*` as the
only staging rule when a required `validation artifact` may use dot naming such
as `quality-semantic-gates-24.validation.json`. If any required path is absent
from `git diff --cached --name-only`, the package hard-fails before commit. A
follow-up commit for a missed required validation artifact is a commit-boundary
defect to repair, not a normal second commit.

Before final ready/completion for Claude/Cursor/Codex extension execution,
governance-repaired execution, or any Luba-requested full-outcome plan, the
floor is 10:

```text
Final Execution Review Gate
final_execution_review_floor = 10
execution_review_cycle_1 = PASS | ZERO_FINDINGS, with scope and evidence
execution_review_cycle_2 = PASS | ZERO_FINDINGS, with scope and evidence
execution_review_cycle_3 = PASS | ZERO_FINDINGS, with scope and evidence
execution_review_cycle_4 = PASS | ZERO_FINDINGS, with scope and evidence
execution_review_cycle_5 = PASS | ZERO_FINDINGS, with scope and evidence
execution_review_cycle_6 = PASS | ZERO_FINDINGS, with scope and evidence
execution_review_cycle_7 = PASS | ZERO_FINDINGS, with scope and evidence
execution_review_cycle_8 = PASS | ZERO_FINDINGS, with scope and evidence
execution_review_cycle_9 = PASS | ZERO_FINDINGS, with scope and evidence
execution_review_cycle_10 = PASS | ZERO_FINDINGS, with scope and evidence
documentation_fix_ledger_present = true
documentation_drift_open = false
three_full_extensions_proven = true
hidden_deterministic_mechanisms_absent = true
llm_mvp_core_core_logic_ownership_proven = true
local_trainable_user_model_lifecycle_proven = true
dpo_quality_proven = true
dpo_source_quality_proven = true
tests_e2e_qa_docs_live_runtime_evidence_proven = true
code_review_completeness_for_code_slices_proven = true
```

One final summary review is not enough. If any pass finds an issue and a fix is
applied, the next pass must run after that fix. Missing review-cycle records
force `IN_PROGRESS_NEXT_STEP_KNOWN`.
A non-PASS execution review does not count toward the 10-review floor. If a
review finds a premature-completion loophole in skills/guides/agent files, the
governing file must be repaired and rechecked before that pass can count.

### Review/fix evidence packaging rule

When packaging a phase that required N review/fix cycles, the package must
separate real evidence from ledger indexing:

```text
requested_review_fix_floor = N
real_pass_count = <valid packets only>
evidence_packet_count = <inspectable packet refs>
ledger_row_count = <summary rows>
ledger_rows_are_evidence = false
artifact_version_order_proof = present/absent
```

Each counted packet must include reviewer/sub-agent id, artifact before/after
refs, loaded skills/gates, findings, fix refs or no-fix justification, and
recheck evidence. If `real_pass_count`, `evidence_packet_count`, and
`requested_review_fix_floor` do not match, or if the package relies on
`ledger_row_count` to close the floor, write `IN_PROGRESS_NEXT_STEP_KNOWN` with
the next missing review/fix pass as the next action.
When the requested floor is 10, the package must show 10 real packets against
the right target outcome. Parent self-review under sub-agent-only, direct parent
repair, summaries, and machine-readable rows are not sub-agent review/fix
passes.

For the 10 Review Cycle Gate before execution, each packet must name
reviewer/sub-agent id, `requested_model=gpt-5.5`,
`requested_reasoning_effort=xhigh`, visible runtime/request metadata or
`runtime_metadata_unavailable`, `mode_verified_from_work_order=true` when
runtime metadata is unavailable and no concrete contradiction is visible, exact
plan section reviewed, findings, fixes or no-fix rationale, before/after refs,
loaded skills/gates, recheck evidence, and whether it proves Claude, Cursor,
and Codex will each become full extensions. A missing requested model/reasoning
row, silent downgrade, concrete runtime/request mismatch, or explicit
medium/high/low/default run without current Luba approval makes the packet
invalid and requires a warning plus state/log governance-violation record.
Generic system/developer identity text such as "GPT-5" is not downgrade
evidence by itself; stopping only on that generic identity is a false downgrade
stop and premature stop loophole.

Ten-Review Evidence Gate packaging: when Luba asked for 10 reviews, the package
must expose records 1..10 in human-readable form. Each record must include
reviewer role/id, requested model/effort, verdict, findings, repair action,
files/sections affected, whether it changed the plan/skills, and recheck
result. A JSON array, state row, ledger row, or sentence saying "10 reviews
completed" is supporting context only and cannot close the floor.

## Target Outcome Completion Packaging Rule

When Luba names a target outcome, the completion package must close that exact
outcome, not a narrower honest packet produced by the implementation or review.

Required package rows:

```text
literal_luba_target_outcome = <exact current quote>
observable_end_state = <product/model/runtime state that must be true>
final_evidence_proves_target_outcome = true/false
not_proven_target_items = [...]
not_proven_items_have_current_luba_scope_narrowing = true/false
review_floor_target_matches_luba_outcome = true/false
```

Completion hard-fails `DONE`, `complete`, `ready`, `TASK_FULLY_COMPLETE`, and
any equivalent closure when final evidence leaves any target-outcome row
`NOT_PROVEN` unless Luba's current unquoted message explicitly narrows the
scope after seeing the delivered artifact.

"NOT_PROVEN and not claimed" is truthful packet wording, but it is not PASS
when the binding requirement was to prove or deliver that target outcome. In
that case write:

```text
status = IN_PROGRESS_NEXT_STEP_KNOWN
false_completion_loophole = target_outcome_replaced_by_honest_narrow_packet
next_action = repair_plan_to_cover_target_outcome_then_rerun_required_reviews
```

Review-count packaging must also verify the target of the reviews:

```text
requested_review_target = <literal Luba outcome>
actual_review_target = <what reviewers actually evaluated>
reviews_of_wrong_target_count_as_plan_sufficiency_reviews = false
```

If `actual_review_target` is a narrower packet, limitation matrix, or
"truthfulness of NOT_PROVEN" packet, then N reviews do not satisfy N reviews of
plan sufficiency. The next action is to repair the plan/review target and rerun
the requested review/fix floor against Luba's named outcome.

## Human-Readable Plan/Execution Log Gate

Every completion or phase packet must include a human-readable section for
Luba before or alongside machine evidence:

```text
stage:
total_major_plan_blocks_or_current_phase_tasks:
completed_count_and_names:
current_plan_step:
current_active_task_and_owner:
remaining_count_and_names:
current_result_type: code | artifact | review | policy | evidence-only | commit boundary
what_changed:
what_remains:
claims_opened:
claims_closed:
claims_still_closed_and_why:
why_this_status_is_true:
next_required_action:
```

Machine-only JSON, state rows, ledger rows, or hidden logs cannot be the
primary status. If execution will continue after governance repair, parent must
also satisfy the Execution Plan Visibility Gate by presenting the plan and
10-cycle review outcome in human-readable Russian in chat before runtime/code/
training/host execution continues.

`STATUS_FORMAT_HUMAN_GATE_ALWAYS_REQUIRED`: every visible progress/status/
review/final/worker packet must pass the canonical human gate in
`docs/RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE.md` before machine evidence, or
the package is `STATUS_FORMAT_HUMAN_GATE_FAILED`. A returned sub-agent packet is
supporting evidence only until the parent translates it into that human-gate
format.

Human-Readable Execution Board Gate:

```text
Всего: <total major plan blocks and/or current-phase tasks>
Сделано: <completed count>/<total>; <completed names>
Сейчас выполняется: <current task and owner/sub-agent>
Осталось: <remaining count>; <remaining names>
Тип текущего результата: <code | artifact | review | policy | evidence-only | commit boundary>
Принятые доказательства: <accepted evidence in human words>
Закрытые claims и почему: <claim names + human reason they remain closed>
Открытые разрывы: <open gaps>
Следующее действие: <next bounded action>
Execution paused for plan-review visibility: yes/no
```

The board is mandatory for long execution plans and governance-repaired
execution. Internal labels such as `QSG-28 accepted`, JSON paths, or STATE flags
must be translated into what was checked, what passed or failed, what was
fixed, and what remains.

Parent-facing human-first status packaging rule:
- `human-first status`: every Luba-facing phase/completion/progress package must
  start with human meaning: total tasks/scenarios when known, done count,
  current phase/step meaning, what is being checked or fixed, what remains, and
  whether the claim is scoped/evidence-only or accepted dual-path scenario
  coverage.
- `machine labels after human meaning`: `ROW19_*`, `ROW20_*`, packet names, tool
  labels, JSON/state rows, validator names, and generic words such as
  "execution", "packaging", or "review" may appear only after the human
  explanation as internal references.
- `parent is orchestrating not executing`: when parent is architect/governance/
  review/sub-agent-only, package wording must say "sub-agent is checking/writing
  X" and "parent is reviewing/orchestrating"; it must not sound like the parent
  is executing product work.
- `translate sub-agent packets before reporting`: raw sub-agent packet language
  cannot be the whole visible package; translate it into meaning, result,
  remaining gap, and next bounded action first.
- Bad -> good incident example:
  Bad: "Launching Row20 prep review. `ROW20_PREP_REVIEW...` execution/packaging."
  Good: "Всего 20 рядов проверки; сделано 19; sub-agent проверяет подготовку
  Row20 и закрытые claims; parent only orchestrates/reviews; scenario coverage
  remains scoped/not proven. Internal ref: `ROW20_PREP_REVIEW`."

Every Luba-facing phase package, progress update, review status, near-final,
or final packet must also include the binding phase/step status template below.
The template augments the no-jump `phase.step.status` pointer; it does not
replace it.

```text
Фаза N/14: <человеческое имя фазы>
Что решает фаза: <одно простое русское предложение>
Шаг NN.MM/NN.Total: <человеческое имя шага>
Что решает шаг: <одно простое русское предложение>
Сейчас: <человеческий перевод DONE/IN_PROGRESS/NEEDS_FIX/REMAINING/BLOCKED; технический label только в скобках>
Что найдено: <что review/status проверка увидела, или "пока не проверялось">
Что исправлено: <что fix изменил, или "пока ничего не менялось">
Что осталось: <оставшаяся работа и boundary простыми словами>
Счетчики фазы: сделано <X>/<Y>, осталось <Y-X>/<Y>
Счетчики всего плана: сделано <A>/<B>, осталось <B-A>/<B>
```

`Фаза N/14, шаг NN.MM/NN.Total, статус X` alone is a packaging failure when
the packet is for Luba. Technical labels such as `generic_bad_output`,
`writer-level path`, `QSG-28`, STATE flags, JSON paths, validator names,
`NEEDS_FIX`, `DONE`, or `PASS` must be translated in the same update into what
review found, what fix did, and what remains.

Sequential Execution-Control Gate:

Completion and phase packages for multi-phase plans must include
`current_execution_pointer` separately from artifact targeting, plus an
`ordered_phase_queue` whose row ids use exact `phase.step.status` form.

Allowed execution-control statuses are exactly:

```text
done | in-progress | remaining | review-block | blocked
```

The package may mark only the first unfinished row as the current executable
target. It must fail packaging when a later phase is active while an earlier row
is `remaining`, `review-block`, or `blocked`.

Required package fields:
- total phases;
- total substeps;
- counts for every allowed execution-control status;
- completed phase list;
- current phase and step;
- remaining phase list;
- remaining action count and names;
- one human-language sentence per phase explaining what it solves;
- human-readable `requirement -> phase -> evidence` matrix.

Counts must equal `ordered_phase_queue` row statuses. Legacy state labels such
as `DONE`, `PASS`, `complete`, `partial`, or `IN_PROGRESS_NEXT_STEP_KNOWN` may
remain as secondary state/evidence labels only; they cannot replace the
execution-control vocabulary.

Plan Presentation Gate: if Luba says she cannot see the plan, cannot see 10
reviews, or cannot understand the stage, package status as
`IN_PROGRESS_NEXT_STEP_KNOWN` with the next action to present the plan and the
review evidence in chat. This is a visibility/protocol failure to repair before
execution resumes, not a final stop by itself.

Sub-Agent Visibility Ledger: package every sub-agent item with agent id or
nickname, requested model, requested effort, task, status, key result, exact
changed files or reviewed sections, and what parent review accepted or
rejected.

Human gate for chat logs: a completion package fails readability if Luba cannot
tell from the visible status what is done, what is running, what remains, what
kind of result this is, which claims are still closed and why, and what bounded
action comes next. The package must set the next action to governance/skill
repair before product continuation, not to STOP, while the plan-derived next
action is known.

False Completion / Stop Repair Rule: if the package says done/ready/complete
while the full requested goal is not proven, the package must downgrade to
`IN_PROGRESS_NEXT_STEP_KNOWN`, name the false completion loophole, repair
plan/skills/gates, present the human-readable board and review records, and
continue the required review/fix sequence unless Luba explicitly asks to stop
or answer only.

## Full Extension Definition Gate

Completion may not claim a full extension unless Claude, Cursor, and Codex each
have evidence for real host entrypoint, settings/debug, target-locator/core API
path into `llm_mvp_core`, prompt-to-task, task-to-orchestration,
task/plan-to-skills, local adaptive learning, export/import/backup/
continue-training, and QA with live host evidence or explicitly scoped handler
evidence. Any host with only JSON, PowerShell, manifest, report, validator,
dashboard-to-core, or helper-call evidence remains `IN_PROGRESS_NEXT_STEP_KNOWN`.

Full Extensions Readiness Review Gate: completion review must list Claude,
Cursor, and Codex separately and check real host entrypoint/runtime path,
`llm_mvp_core` ownership, hidden deterministic shortcuts, local model
backup/export/import/share/continue-training, DPO/source quality, tests/QA,
docs, live host evidence, and code-review completeness for any code slices.
Smoke, API, dashboard, helper, or static-contract proof alone cannot close full
extension readiness.

## Sub-agent pool saturation gate

Completed sub-agent pool saturation is infrastructure cleanup. It never expands
the parent architect's write authority. If the active plan requires bounded
sub-agents and the pool is full, close, archive, reuse, or wait for completed
agents according to available tools. If capacity cannot be safely restored,
record the infrastructure issue and continue only governance/review work inside
the allowed scope. Do not package parent local production execution as an
accepted substitute.

Sub-agent error is recovery queue, not stop. Packaging must not treat usage
limit, capacity exhaustion, timeout, tool error, transport failure, or failed
tool invocation as product readiness, product stop, completion, or a completed
review/fix cycle. A valid package records exact failing agent id/nickname,
exact error, work-order id, time, failed scope, close/quarantine action, and the
next smaller bounded recovery agent if tools allow. If the exact path is
externally blocked, the package names the next independent safe bounded work
item; if no sub-agent can be spawned at all, it reports the external blocker and
preserves uncommitted evidence. An errored agent without a returned evidence
packet counts as zero cycles/reviews/passes. The human-readable package must
show phase/step counts, done, in-progress, remaining, and what remains blocked.

## Bounded sub-agent boundary completion gate

Before packaging or accepting any sub-agent slice, check that the returned
packet stayed inside the assigned work order:

```text
work_order_scope_matched=true
exact_allowed_files_only=true
may_derive_next_phase=false or explicitly granted
must_stop_after_report=true
git_authority=forbidden|parent_only|allowed_with_exact_quote
commit_push_authorized=false unless commit/push-only or exact file-set grant
allowed_git_commands_present=true
forbidden_commands_present=true
git_write_commands_forbidden_when_commit_push_false=true
artifact_or_code_subagent_allowed_git_commands_NONE_by_default=true
parent_will_perform_commit_push_after_review_present=true
commit_push_authority_only_in_commit_only_work_order=true
parent_direct_write_forbidden_when_subagent_only=true/false
direct_repair_reflex_detected=false
claimed_subagent_only_repair=true/false
subagent_id_for_claimed_repair=<id or NONE>
changed_files_for_claimed_repair=[...]
parent_authored_files_in_claimed_subagent_only_repair=[]
parent_self_review_counted_as_subagent_pass=false
parent_local_sidecar_work_while_waiting=false
parent_read_governed_files_before_packet_return=false
parent_read_only_verification_after_packet_return_only=true
firewall_exact_files_verified_for_required_phrases=true/false
broad_rg_used_as_firewall_completion_evidence=false
subagent_notification_inspected_for_commit_push_claims=true
subagent_packet_reports_git_commands_run=true
subagent_packet_reports_git_write_or_ref_commands_run=true
subagent_packet_reports_git_directives_emitted=true
subagent_commit_push_performed_only_when_authorized=true
parent_review_created_or_accepted_by_parent_after_reading_artifacts=true
state_reconciliation_flags_match_status=true
evidence_text_format_gate_present=true
changed_text_evidence_utf8_no_bom_lf_no_trailing_whitespace=true
json_evidence_parse_passed=true
git_diff_check_or_no_index_equivalent_passed=true
format_repair_repeat_finding_recorded_if_needed=true
```

If a sub-agent created later-phase artifacts, committed, pushed, or wrote a
`parent-review` file beyond its authority, do not mark the slice as cleanly
complete. The completion package must:
- quarantine and validate already-created artifacts rather than reverting by
  default;
- record the overrun violation and exact work-order field that failed;
- patch AGENTS/CLAUDE/skills before any further feature/training/model work;
- set the next action to parent-owned review or commit-boundary repair.

If Luba required repair or execution through sub-agents, the package must prove
authorship, not only file content. A claimed sub-agent-only repair must name the
sub-agent id and exact changed files. If any listed file was authored by parent
direct `apply_patch`, shell file-writing, generated rewrite, or equivalent
file-writing tools while `parent_direct_write_forbidden_when_subagent_only`
applied, the completion status is not clean:

```text
status = IN_PROGRESS_NEXT_STEP_KNOWN
false_completion_loophole = architect_parent_direct_edits_under_subagent_only_instruction
next_action = quarantine_or_validate_parent_authored_files_then_issue_bounded_subagent_repair
```

The parent may still perform read-only verification after the sub-agent packet
returns, request sub-agent fixes, and handle an explicitly assigned final
stage/commit/push boundary. The parent may not use sub-agent waiting time to
read governed files for independent loophole research, repair drafting, or
non-overlapping substantive work.
If the package omits the sub-agent id, omits changed files, detects the direct
repair reflex, detects parent local sidecar work before packet return, or
counts parent self-review/direct repair as a sub-agent pass, the same
`IN_PROGRESS_NEXT_STEP_KNOWN` status applies. The next action is a bounded
sub-agent repair or the next required sub-agent review/fix pass.

For firewall repairs, the package must prove exact-file coverage. It must list
each exact AGENTS/CLAUDE/HOW-TO-USE-SKILLS/skill file named by the work order
and show targeted textual verification for the parent local sidecar work ban,
`read-only verification after a sub-agent packet returns`, no local
non-overlapping work escape, and sub-agent waiting is not a stop. Folder-wide grep output is
supporting context only and cannot close the package.

Parent Architect Subagent-Only Firewall compatibility: completion packaging
must preserve the rule that read-only verification is allowed only after a
sub-agent packet returns, the no local non-overlapping work escape is disabled,
and sub-agent waiting is not a stop.

If `commit_push_authorized=false`, any `git add`, `git commit`, `git push`,
`git checkout`, `git reset`, `git branch`, `git tag`, `git merge`,
`git rebase`, or git write/ref command in the sub-agent transcript is an
authority violation. Already-pushed artifacts become
`quarantine_pending_parent_validation`; parent may accept them only after
independent validation and a recorded governance repair. Sub-agent final reports
must not include git directives unless the work order explicitly authorized the
git action.

If `git_authority=forbidden` or `git_authority=parent_only`, the same rule
applies even when the phase as a whole eventually requires commit/push. The
sub-agent's bounded-pass completion claim is false when it used forbidden git;
only the artifact content can be considered after independent parent readback.

If a runtime or evidence-writing sub-agent returns JSON, MD, TXT, JSONL, or
other text evidence without proving UTF-8 without BOM, LF line endings, no
trailing whitespace, JSON/JSONL parse when applicable, and `git diff --check`
or an explicit no-index equivalent for the exact changed evidence files, do not
package the slice as clean. Repeated format-only parent repair is a worker
finding that must be recorded before the next bounded work order.

Artifact-producing, code-producing, documentation-producing, data-producing,
report-producing, plan-producing, and governance-producing sub-agents must not
package their own commit/push boundary. Their returned packet may say the
artifact is ready for parent review, but not that the phase is cleanly
committed/pushed unless the current work order was commit-only. Parent review
must inspect the sub-agent notification for commit/push claims and directives;
do not rely only on repository state.

Sub-agent self-review and `parent-review` artifacts are evidence only. They do
not replace parent ARCHITECT_INTERNAL review.

---

## Architect soft-skill closure gate

For architect, plan, review, governance, protocol, and skill-repair phases, the
completion package must include:

```text
conversation_intent_review_present:
settings_gap_hypothesis_present_when_required:
soft_note_closure_check:
  must_address_open_notes:
  unreviewed_notes:
  reviewed_watch_context_later_notes:
unresolved_thinking_check_present_when_required:
attention_budget_check_present_when_required:
order_making_review_present_when_required:
```

Completion is blocked when:

- a direct question was not answered first;
- `settings_gap_hypothesis` was required but absent;
- any `must_address` soft note remains open;
- notes were not reviewed at all;
- `action_as_relief_risk=true` and no `unresolved_thinking_check` exists;
- a mixed request required an attention budget but the response did not return
  to analysis;
- many sources were required and no source map/rule synthesis exists.

This gate does not authorize product code, tests, training, host probes, commit,
or push. It only prevents a governance/plan/review phase from claiming closure
while architect soft-skill gaps remain.

---

## Gate Check — Two Models

### Sequential mode (parallelWork: null)

```bash
# Full solution test run
dotnet test LlmFromScratch.sln --no-build 2>&1 | tail -5
# Assert: total passed ≥ {absolute_baseline} and Failed: 0
```

### Parallel mode (parallelWork: N)

```bash
# Check only this subproject's tests
dotnet test LlmFromScratch.sln --no-build --filter "FullyQualifiedName~SubprojectName" 2>&1 | tail -5
# Assert: all {expected_delta} new tests pass, Failed: 0
# (Do NOT assert absolute total — post-merge CI handles that)

# Build always runs regardless of mode
dotnet build LlmFromScratch.sln 2>&1 | tail -3
# Assert: 0 Error(s)
```

---

## Procedure — Three Artifacts in Order

### Artifact 1: STATE-Pn.json update (machine-readable)

Update STATE-Pn.json with the phase completion data:

```json
{
  "phase": "P6",
  "phaseName": "IncrementalTrainer — LoRA fine-tuning via LoraApplicator + DpoTrainer",
  "status": "DONE",
  "completedAt": "2026-05-01",
  "parallelWork": null,
  "testChain": {
    "sessionStart": 881,
    "sessionEnd": 916,
    "delta": 35,
    "chainVerified": true
  },
  "gateResults": {
    "build": "PASS — 0 Error(s)",
    "testGate": "PASS — 916 passed (+35 from 881), 0 failed",
    "dnaCompliance": "PASS",
    "trainablePhaseGate": "N/A (not a trainable phase) | PASS — generated-code-review Layers1-5 PASS; runtime accuracy=.., loss=.., cpu_ms=.., ram_mb=.."
  },
  "filesChanged": [
    "Src/12.ContinuousLearning/IncrementalTrainer.cs",
    "Tests/IncrementalTrainerTests.cs"
  ]
}
```

### Artifact 2: PHASE-COMPLETE-Pn.md (human-readable — for Luba)

```markdown
# Phase [N] Complete — [phase_title]
## [completed_at]
## Mode: SEQUENTIAL | PARALLEL

## What Was Built
[2–4 sentences in plain English: what the phase produced and why it matters.
Not a list of files — the meaning of the work.]

## Gate Results
| Check | Result | Value |
|-------|--------|-------|
| Build | ✅ PASS | 0 Error(s) |
| Test gate | ✅ PASS | 916 passed (+35 from 881), 0 failed |
| DNA compliance | ✅ PASS | Rule 1-4 verified |
| [phase-specific checks] | ✅ PASS | ... |

## Files Changed
[List of files created/modified with one-line summary each.]

## What's Next
Phase [next]: [next_phase_title]

## STOP OR CONTINUE — stop only when the active plan has no next action
```

**Rules for PHASE-COMPLETE.md:**
- "What Was Built" is prose — not bullets, not file lists
- Gate results table is exact — no approximations
- Parallel mode: show delta count, not absolute (prevents confusion)
- Last line is STOP only when the active plan permits a stop. In continuous
  plans, the last line is `IN_PROGRESS_NEXT_STEP_KNOWN` with the next phase.

### Artifact 3: Session handoff note (for next session)

Brief section in PHASE-COMPLETE-Pn.md or separate SESSION-BRIEF.md:

```markdown
## Session Handoff

**Branch:** main
**Last commit:** [hash] — phase N: description
**Test baseline at end:** [N] passed, 0 failed
**Next phase entry point:** [file:line or class to start from]
**Open items:** [only if Luba decision needed]
```

---

## Invocation in EXECUTION Files

Every EXECUTION file ends with this section:

```markdown
## PHASE GATE

# Build check (always):
dotnet build LlmFromScratch.sln 2>&1 | tail -3

# Test check (mode determines which):
# Sequential: dotnet test LlmFromScratch.sln --no-build 2>&1 | tail -5
# Parallel:   dotnet test --filter "FullyQualifiedName~SubprojectName" --no-build 2>&1 | tail -5

[phase-specific gate checks]

## PHASE COMPLETION PACKAGE
# Run only after ALL gate checks above pass

# 1. Update STATE-Pn.json
# 2. Write PHASE-COMPLETE-Pn.md
# 3. Append session handoff note
# 4. git commit + push (phase-git-report-SKILL.md)

## STOP OR CONTINUE
Use STOP only when the active plan allows a stop and the state has no next
action. If the active plan requires continuous execution, write
IN_PROGRESS_NEXT_STEP_KNOWN and continue to Phase [next].
```

---

## Anti-Patterns

1. **Writing the completion package before gate passes** — gate must pass first. Always.
2. **Stopping on a checkpoint in a continuous plan** — if the plan says continue
   while the full goal remains open, package the checkpoint and keep going.
3. **Showing absolute test count in parallel mode** — show the delta, not the total.
4. **"What Was Built" as a file list** — prose that explains the meaning of the work.
5. **Missing phaseName in STATE-Pn.json** — every phase needs both number and name.
6. **Marking a trainable phase DONE on build+contract+DNA alone** — a json/stub unit (status `not_ready_*`/`*_no_inference_implemented`) passes build, contract tests, and DNA. Trainable phases require the Trainable-phase gate above: generated-code-review Layer 5 + Runtime Evidence Firewall + numeric metrics. This is the exact V20 failure.
7. **Marking a code/runtime/plugin phase DONE from JSON/PowerShell/static evidence** — if the phase promised real code, host entrypoint, runtime invocation, or model asset readiness, the Implementation Reality Gate must pass before DONE.
8. **Marking a UI/dashboard phase DONE while full-interactive UI automation is open** — view-model, handler-level, WPF build, static QA, script parity, or test-only adapter evidence is intermediate only when e2e/QA automation was requested.
9. **Collapsing several review/fix cycles into one summary review** — the exact requested review/fix floor applies; if Luba named four cycles, two passes are not enough.
10. **Using completed sub-agent pool saturation as parent execution permission** — close/reuse/wait for completed agents; do not turn infrastructure saturation into parent local production edits.
11. **Marking UI/UX DONE from click-only automation** — a test that clicks the UI but never checks the human-facing surface is UI automation, not UX automation.
12. **Scenario coverage, not product-ready count** —
    `product_readiness_term_deprecated_use_scenario_coverage`.
    Deprecated term: use dual-path scenario coverage instead. The completion
    package must not use product readiness as a working gate, primary status,
    stop reason, execution target, or counter.
    It must reject `product-ready count = 0 because full host UI not proven`,
    or equivalent old full-live-host wording, and rewrite it as `Dual-Path
    Same-Scenario Coverage`.

    `dual_path_same_scenario_coverage_gate`: a scenario is accepted only when
    the same scenario and same prompt are proven through Path A and Path B.
    Path A is API/boundary path through adapter/API/core request, response,
    trace, RequestedSkills, and AvailableSkills when relevant. Path B is
    local-agent UI path in the real Cursor/Claude/Codex application with
    screenshots, window inventory, redaction log, and trace/correlation evidence
    when available.

    `no_product_ready_count_primary_status`: packages must say `scenario
    coverage: total / accepted / api_only / ui_only / blocked / not_started`,
    plus per-scenario Path A and Path B status. Human-facing packages must
    state total scenarios, covered by Path A, covered by Path B, blocked Path B
    with reason, and remaining steps. If Path B cannot reach an
    extension/plugin route because of host limitations, auth, storage, missing
    UI route, or inaccessible app surface, package a host-path blocker with
    exact screen/state, blocker reason, and next scenario/product fix; do not
    restore the old live-host product-readiness gate.

13. **Final between bounded slices** — `final_between_bounded_slices_forbidden`.
    When scenario coverage is incomplete or the plan has a known safe next
    bounded action, the completion package must not end in a final/status stop
    after commit/push. `commit/push boundary is not a stop`: it is a checkpoint
    before the next bounded sub-agent slice.
    `final/status/report/commit-push/status-board is not a stop while authorized
    work remains`: a final-style summary is allowed only when Luba explicitly
    stops/pauses/asks for answer-only or all active requirements are complete.
    Otherwise package interim status and continue the next bounded sub-agent
    work order immediately.
    Required literal:
    `final/status/report/commit-push/status-board is not a stop while authorized work remains`.
    `status_board_counts_drive_continuation`: if `remaining_count > 0` and
    `next_bounded_action_exists=true`, do not stop and do not use `final`; start,
    wait for, review, or repair the next bounded sub-agent action. Commit/push
    of an evidence packet or status board is not a completion boundary when
    remaining groups exist.

    `promise_to_continue_is_not_continuation`: "I will continue", "continue
    next", or "следующий шаг" fails packaging unless the next bounded sub-agent
    was actually started/waited/reviewed or a real blocker exists.
    `accidental_final_recovery_requires_subagent_repair_and_resume`: after an
    accidental final/status stop, the next package must answer Luba first,
    classify the old answer, repair the exact loophole through bounded
    sub-agents, and resume the previously known bounded action without requiring
    fresh approval unless Luba explicitly stops or a real blocker/unsafe action
    exists.

    Pre-final questions required:
    `scenario_coverage_complete?`, `explicit_user_stop?`,
    `active_requirements_complete?`, `status_board_remaining_count_zero?`,
    `next_bounded_action_exists?`, `next_subagent_started_or_real_blocker?`. If
    the answers are false, false, false, false, true, false, the package must return
    `IN_PROGRESS_NEXT_STEP_KNOWN` and name the next sub-agent action instead of
    producing a final response.
