# Implementation Integrity

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (in any language), challenges an answer, or says a failure came
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

## Fresh Execution / Method / Humility Implementation Hook

Implementation integrity must reject stale or method-skipped completion:
- `REPEATED_CURRENT_COMMAND_IS_FRESH_EXECUTION_GATE`: repeated current command is a fresh execution trigger; old artifacts are baseline evidence, not completion.
- `TOO_FAST_HIGH_PASS_SUSPICION_GATE`: too-fast 50-pass completion is suspicious until per-pass sub-agent evidence is accepted.
- `LARGE_VOLUME_MODEL_HUMILITY_AND_FOCUS_GATE`: large-volume model humility requires bounded slices and focus ledger before completion.
- `EXPLICIT_METHOD_OVERRIDE_GATE`: Luba's method/model/reasoning/count/folders/target/sub-agent route is a deliverable constraint; existing code or package content without that method remains NEEDS_FIX.
- `FINAL_ANSWER_FRESHNESS_GATE`: final/status must answer the latest command and state freshly executed this turn vs only verified.
- `CURRENT_REPEATED_PACKAGE_COMMAND_GOVERNANCE_ADDENDUM`: implementation integrity must reject repeated extension-development package closure by old artifact verification instead of a fresh execution chain; too-fast N-pass/status-package claims without ordered packets/records; missing large-volume model humility focus ledger with exact current task, not-the-task, files, pass/review counts, active worker, remaining actions, and latest-command freshness; parallel workers when Luba flags parallel/too-fast or requires exact-count sub-agent work unless her current unquoted message authorizes parallelism; interrupted or zero-work spawned-worker packets using `NONE` instead of `subagent_id: UNKNOWN_IN_SUBAGENT` when platform id is hidden; parent local sidecar work during waiting; missing human-readable phase counts before execution continuation; and final/status without latest-command, fresh-vs-old execution, actual packets/records, no parent self-review counted, and no product-readiness overclaim.
- `COMMAND_SCOPE_EXACTNESS_GATE`: implementation integrity must reject current-task command violations. Current `allowed_commands` and `forbidden_commands` are binding; read-only convenience commands, alternate git diff modes, external AI/web, `codex.exe`, nested sub-agents, branch/git mutations, or shell variants outside the current allowed list are violations unless Luba's current unquoted instruction explicitly authorizes them. Evidence from an out-of-scope command is quarantine evidence only and cannot support completion.

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

## Implementation Pre-Final Continuation Enforcement Gate

- `pre_final_continuation_enforcement_required`: implementation integrity must require pre-final fields before any status, review, no-safe-work, completion, hold, PASS, or final packet: `active_task_open`, `next_bounded_product_review_or_governance_action_exists`, `active_or_returned_subagent_packet_pending_verification`, `required_passes_remaining`, `next_subagent_started_or_waited_this_turn`, and `true_external_blocker_certificate`.
- `parent_must_run_pre_final_check_before_status_final`: after governance repair, rule readback, or `NO_PATCH`, missing pre-final check fields make status/final invalid evidence.
- `final_packet_forbidden_when_next_bounded_action_exists`: final/status/completion/hold/no-safe-work must be rejected when active task remains open and next bounded work, pending packet verification, or pass deficit exists without a true external blocker certificate.
- `governance_repair_acceptance_requires_next_worker_started_or_true_blocker`: governance repair may be accepted only when same-chain proof shows next product/review worker started, waited, or reviewed, or a true external blocker is recorded.
- `after_governance_repair_spawn_or_wait_next_product_review_worker`: for the current 50-pass review chain, route to `final review pass 4/50`; stopping after governance repair is `premature_final_after_governance_repair`.
- `NO_PATCH_requires_product_continuation_check`: `NO_PATCH`, PASS, NEEDS_FIX, timeout, usage limit, partial packet, worker silence, and stale status route to continuation/recovery unless true external blocker proof exists.
- `rule_text_present_but_not_enforced_is_failure`: rule text without final/status rejection when next bounded action exists is a failure.
- `subagent_wait_heartbeat_required_before_wait`: before each wait/poll, emit a human-readable heartbeat with active worker, unaccepted result, and next wait/recovery step.
- `one_active_agent_does_not_mean_wait_silently`: one-active-agent mode requires bounded heartbeat waits, not silent waiting or final hold.

## The Golden Rule for Closing Gaps

**Fix the instance AND add the structural guard that prevents regression.**

A session that adds a new interface but does not:
- Update docs/COMPONENT-INTERFACES.md to reflect the new interface
- Update docs/SUBPROJECT-MAP.md with the new implementation status
- Update HOW-TO-USE-SKILLS.md with new activation triggers
- Add a DNA check for the new pattern

...has fixed the instance without adding the guard.
The same gap will appear again in the next session.

---

## Check 1 - Documentation Updated

After adding or changing any interface or class:

```bash
# Verify COMPONENT-INTERFACES.md reflects the new interface
grep -n "INewInterface\|NewClass" docs/COMPONENT-INTERFACES.md
# Expected: interface signature block present

# Verify SUBPROJECT-MAP.md reflects current status
grep -n "NN.ProjectName" docs/SUBPROJECT-MAP.md
# Expected: row with current status (Active, not Planned)
```

---

## Check 2 - Tests Cover the New Pattern

After adding a new implementation pattern:

```bash
# Verify tests exist for the new class
grep -rn "class.*NewClass.*Tests\|NewClassTests" Tests/ --include="*.cs"
# Expected: test file present

# Verify tests run and pass
dotnet test --filter "NewClass" --no-build 2>&1 | tail -5
# Expected: Failed: 0, Passed: > 0
```

---

## Check 3 - DNA Rule Applied to the New Code

After adding any production code:

```bash
# Rule 1: No throw in business logic
grep -n "throw new" Src/NN.Project/NewClass.cs
# Expected: 0 matches

# Rule 2: No direct concrete instantiation for fabric types
grep -n "new.*Reader\|new.*Writer\|new.*Storage" Src/NN.Project/NewClass.cs | grep -v "new.*Exception\|new.*Result"
# Expected: 0 matches in business logic

# Rule 3: Every production line has a comment
# Manual check: open file, verify no uncommented production lines
```

---

## Check 4 - No New Gaps Introduced

After any change to existing interfaces or classes:

```bash
# Verify callers still compile
dotnet build LlmFromScratch.sln 2>&1 | tail -3
# Expected: 0 Error(s)

# Verify test baseline unchanged or improved
dotnet test --no-build 2>&1 | tail -5
# Expected: Failed: 0, Total >= [session start baseline]
```

---

## Check 5 - Implementation Reality / Anti-Stub Gate

`CODE_IMPLEMENTATION_CLAIM_GATE` and `Implementation Evidence Map` apply here.

After any implementation that the plan calls trainable, learned, model-backed,
algorithmic, or phase-complete, prove the delivered code is not only a
stub/scaffold/report bundle.

Required evidence:

```text
1. Actual code path:
   - host/API/runner entrypoint -> service/machine -> trainable unit -> trained state
   - file:line refs for the entrypoint, called method, and learned-state read/write
   - implementation location map: repo/project/file/class/method/entrypoint
   - call chain proves the user task reaches the claimed implementation, not only
     a helper, report generator, fixture, or static contract

2. Not a JSON/report substitute:
   - reports may be evidence, but the phase artifact must include production
     code or model assets when the plan asked for them
   - JSON/PowerShell/gate output alone is static evidence, not implementation

3. Not a stub:
   - no `not_ready_*`, `*_no_inference_implemented`, `TODO`, `PLACEHOLDER`,
     `NotImplementedException`, fixed descriptor return, or no-op `TrainBatch`
     on the claimed done path

4. Trainable evidence when learning is claimed:
   - generated-code-review-SKILL.md Layers 1-5 PASS
   - Runtime Evidence Firewall PASS
   - numeric held-out metrics are present (accuracy/exact/loss/reward,
     cpu_ms, ram_mb, row counts)
   - the unit changes decision after training on new pairs and is not exact
     prompt/source replay

5. Skill/guide load evidence:
   - executor packet lists loaded skills, required guides, and injection blocks
   - trainable/algorithm work includes SK-VERIFY

6. Independent verification handoff:
   - the implementing agent names exactly how a separate reviewer/sub-agent can
     verify the code claim: files to read, commands/tests to run, expected
     observable behavior, and falsification conditions
   - if this handoff is missing, implementation integrity may accept only
     "artifact produced", not "task implemented"
```

If any row is missing, implementation integrity is NOT confirmed. The required
state is `IN_PROGRESS_NEXT_STEP_KNOWN`, with `next_action` set to the first
missing reality-evidence row.

Architect read-only claim inspection: an architect/reviewer may read the code
path above to verify the implementation claim directly against source at any
agent-work stage, including work-order review, code review, and claim review.
This is not permission to fix product code directly. If the code does not
implement the claimed task, report the violation to Luba and return a bounded
fix work order with exact files/tests instead of polishing reports.

---

## Check 6 - Real Project Boundary, Not Test-Only Source Linking

When a slice adds a new project, dashboard, API lab, or runtime surface, tests
must reference the real project artifact. A `Compile Include`, linked source
file, generated temporary project, or script-only adapter can be fallback
evidence, but it cannot replace the requested project boundary.

Required evidence:

```text
1. The project file exists in the requested project folder.
2. Tests reference that project through `ProjectReference` or the approved
   runtime/package boundary.
3. The test project does not link production source files from the new project
   with `Compile Include` / `Link` as the main implementation path.
4. A build or equivalent project-boundary command runs against the real project.
5. The state/report says what the evidence proves and what it does not prove.
```

For dashboard/UI work, view-model automation and WPF build evidence may be valid
fallback coverage, but they must be labelled as such. They do not prove full
interactive WPF automation, host runtime, external AI, semantic quality, or
trained-model readiness.

When UI/UX quality is in scope, full interactive UI automation is still not
enough by itself. Implementation integrity also requires a UX automation row
that proves human-readable labels, accessibility names/help text, examples,
error clarity, safe raw/debug affordances, history/copy/export clarity,
layout/bounds, and no internal jargon on the primary path.

After editing any state JSON or report JSON, rerun a JSON parse and `git diff
--check` for the changed state/report files before claiming the gate passed.
Runtime/evidence-writing sub-agents must also prove every changed text evidence
file is UTF-8 without BOM, LF line endings, and has no trailing whitespace;
JSON/JSONL evidence must parse. The packet must include `git diff --check --
<changed evidence files>` or an explicit no-index equivalent for generated or
untracked files.
If `git diff --check`, tests, JSON/JSONL parse, schema checks, builds, or any
required verification prints failure output or exits nonzero before commit/push,
implementation integrity is still failed even if a later commit/push succeeds.
Native PowerShell wrappers must check `$LASTEXITCODE` immediately after each
required verification command and abort before stage/commit/push on nonzero; no
command chain may continue into commit/push after failed verification.
Never chain `git diff --cached --check; git commit; git push` or a similar
semicolon sequence. PowerShell semicolon sequencing can continue after a failed
gate, so split or guard commit/push commands, verify the precheck exit code
before commit, and treat a failed precheck as a fix-loop trigger.
Before any commit, every pre-commit check such as `git diff --check`,
`git diff --cached --check`, JSON parse, text-format scan, screenshot privacy
review, and required test must run as a hard gate. commit is forbidden if a
pre-commit gate prints warnings/errors, even when the shell continues or a later
git command succeeds. do not chain commit/push after a gate command unless exit
code is checked and the wrapper aborts on nonzero before staging, committing, or
pushing. A committed bad evidence packet requires an immediate fix commit and a
governance repair, and the integrity report to Luba must name the exact failed
gate and repaired rule.
Required verification literals: commit is forbidden if a pre-commit gate prints warnings/errors; do not chain commit/push after a gate command unless exit code is checked.

Unbounded executor integrity gate: implementation integrity must fail any code,
runtime, test, governance, or evidence slice produced by one executor/sub-agent
under an unbounded scope such as "act until product ready", "continue until the
whole plan is done", "do everything", or "no stops until ready". If the
executor id is known, parent review must require interrupt/close before more
output is accepted. If the id is unavailable, quarantine the output as
evidence-only and replace it with a bounded worker. This is not a stop for the
parent chat.

Implementation work orders must cover one scenario, slice, repair, or review
packet. Deprecated `product readiness` wording must not be used as an
implementation scope, primary status, status counter, stop reason, execution
target, or impossible full live host UI proof gate. Use bounded `Dual-Path
Same-Scenario Coverage` packets after implementation, test, review, and
packaging slices. A
sub-agent over 10 minutes without a protocol/evidence packet is over-scoped:
request or interrupt for the packet, close if needed, split the work, and
continue with a bounded replacement. Every bounded implementation work order
must set `must_stop_after_report=true`.

OP-67/temp-proof integrity gate: OP-67 mergeability, temp-clone, temp-copy,
candidate-materialization, temp-index, and merge-tree proof work must be split
into atomic slices when the work could exceed 10 minutes. Before any long temp
proof operation, the worker must write and read back a codex-prefixed
evidence/status file or return a heartbeat/status packet naming exact scope,
paths, cleanup responsibility, and next heartbeat point. If early evidence
cannot be written/read back, return `NEEDS_FIX` before starting the long
operation. If a broad OP-67 worker times out or is closed with no packet and no
evidence file, implementation integrity accepts only the narrow recovery claim:
`no-result timeout worker counts as zero OP-67 proof`. The next attempt must
use the ordered slices `inventory-only -> candidate-materialization-only ->
merge-tree-original-only -> merge-tree-main-only -> cleanup-only`, with each
slice writing/readbacking its own codex-prefixed evidence file. Do not run or
accept another all-in-one long OP-67 proof worker until those slices pass.

Candidate-materialization retry integrity gate: after a failed or incomplete
`candidate-materialization-only` slice (early artifact only, pending sections,
timeout/no final packet, or no candidate hash/tree/status), do not run another
broad all-path copy/materialization. The next authorized materialization retry
must be micro-sliced with readback and expected/actual count checks after each
class: tracked governance/evidence, tracked product/tests, untracked includes,
then tracked artifact deletions. The worker must name any temp repo/path before
creation, avoid original `.git` writes and global git config, and record
heartbeat evidence before any long copy step.

Verification literal: `dual-path scenario coverage is the primary status`.

Implementation packets must be readable before they are machine-checkable:
start in the operator's language (plain language; Russian for the original author) with total phases/steps,
done/in-progress/remaining, what the phase solves, what the current step solved,
what evidence exists, what is not proven, and the next bounded action.
`STATUS_FORMAT_HUMAN_GATE_ALWAYS_REQUIRED`: any implementation progress/status/
review/final/worker packet visible to Luba must pass the canonical operator-language human
gate before machine evidence, or implementation integrity fails with
`STATUS_FORMAT_HUMAN_GATE_FAILED`.

Commit group integrity also requires exact staged artifact proof. Build
`required_commit_group` from the reviewer/validation ledger, stage those exact
paths, and use any broad `glob` only as a supplement. Required
`validation artifact` files with dot/dash naming variants must be explicitly
listed and present in `git diff --cached --name-only` before commit. If a
required validation artifact is missing from the staged set, implementation
integrity fails before commit. A follow-up commit for that missed artifact is a
commit-boundary defect to repair, not normal phase cleanup.

---

## Check 7 - UI/Dashboard Completion Firewall

When the phase goal is a UI/dashboard and Luba asked for e2e or QA automation,
implementation integrity is not confirmed while full-interactive UI automation
is open.

Required evidence before final UI/dashboard completion:

```text
1. A real UI automation path exists:
   - WPF/window/control automation, desktop automation, or the exact approved
     host UI path for the product.

2. The automation covers the user journey:
   - select API/item
   - load or enter example/input
   - run through the UI command surface
   - observe result, warnings/errors, raw JSON/debug/copy/export behavior
   - cover negative cases

3. Fallback evidence is scoped honestly:
   - view-model, handler-level, component, static QA, WPF build, script, and
     test-only-adapter evidence are intermediate rows only.

4. State/report compatibility:
   - no final completion claim may coexist with residual full-interactive UI
     automation, UI e2e, or WPF automation gaps.
```

If real UI automation is not feasible in a slice, the correct integrity result
is not failure-by-stop and not success-by-overclaim. Add an honest intermediate
adapter or handler-level artifact, record product-decision/runtime work in an
end queue when needed, keep `goal_reached=false`, and continue independent
plan-derived work.

## Check 8 - UI/UX Automation Firewall

When the phase goal is a UI/dashboard/API lab and Luba asked for UI/UX, QA UI,
or UX automation, implementation integrity is not confirmed by click-path
automation alone.

Required evidence:

```text
1. Human-facing labels:
   - API names, buttons, input, result, history, warnings, errors, and raw/debug
     controls have readable labels.

2. Accessibility metadata:
   - primary controls expose AutomationProperties.Name and HelpText, or the
     equivalent for the UI framework.

3. Guidance and safety:
   - examples say what to type and what output to expect;
   - raw/debug controls explain privacy and evidence boundaries;
   - errors are visible and understandable.

4. Layout and focus:
   - primary panels have stable dimensions/bounds;
   - keyboard or focus path is explicit enough to automate.

5. No primary-path jargon:
   - internal status keys may appear in raw/debug views, but the primary
     user path must not depend on them.
```

If any row is missing, the correct integrity result is
`IN_PROGRESS_NEXT_STEP_KNOWN`, not final UI/UX completion.

---

## Implementation Integrity Report Format

After a capability session, write this report to STATE-Pn.json:

```json
{
  "implementationIntegrityReport": {
    "capability": "IAgentMessageBus",
    "sessionDate": "2026-05-01",
    "checks": {
      "documentationUpdated": {
        "status": "PASS",
        "evidence": "COMPONENT-INTERFACES.md:line 45 - IAgentMessageBus block present"
      },
      "testsPresent": {
        "status": "PASS",
        "evidence": "dotnet test --filter AgentMessageBus -> 8 passed, 0 failed"
      },
      "dnaApplied": {
        "status": "PASS",
        "evidence": "grep throw new → 0 matches; grep direct instantiation → 0 matches"
      },
      "noNewGaps": {
        "status": "PASS",
        "evidence": "dotnet test → 924 passed (baseline was 916, +8 new tests)"
      },
      "implementationReality": {
        "status": "PASS",
        "evidence": "actual code path + anti-stub sweep + SK-VERIFY + numeric metrics"
      }
    },
    "verdict": "INTEGRITY_CONFIRMED"
  }
}
```

---

## Anti-Patterns

1. **Adding a new class without updating COMPONENT-INTERFACES.md** — the doc will not reflect reality at the next session start
2. **Fixing the instance but not adding the structural guard** — the same gap appears again in 3 sessions
3. **"I'll add the doc update in the next session"** — docs are part of the current commit (Rule 6, documentation-sync-SKILL.md)
4. **Counting "implementation complete" without running the DNA checks** — an undocumented class is not complete
5. **Counting "phase complete" from JSON/report/scaffold evidence** - a report proves only the report. If the phase required code or a trainable model, Check 5 must prove the real code path and anti-stub evidence before completion.

## Check 9 - Allowed Trainable Architecture Pattern

If the implementation touches learned, trainable, semantic, adaptive, DPO,
selector, router, classifier, scorer, retrieval, KNN, dictionary, counter, or
pattern-matching logic, verify that the runtime authority is inside one of:

- `ILearningMachine<TDomain>`;
- `ILogicalOrchestrator`;
- `ILogicalFlow<TInput>`;
- `ILogicalUnit<TInput,TOutput>`;
- `ILearningMachinePair<TPersonal,TGlobal>`.

Any scorer, selector, classifier, router, retriever, detector,
KNN/Jaccard/BM25 store, dictionary/counter state, regex/keyword matcher, pattern
matcher, `TryDecide*`, `ChooseBestPattern`, `SelectBestPattern`, or
`CountMatchingSignals` family must be labelled as internal implementation
detail or baseline. It cannot be the architectural completion claim.

Required evidence:

```text
file/class/method | actual mechanism | allowed level | runtime authority |
semantic claim | lifecycle evidence | verdict | required fix
```

If the actual mechanism and the claim differ, implementation integrity fails
until the claim is downgraded or the code is moved behind an allowed contract
with the required lifecycle gates.

## Check 10 - Bounded Sub-Agent Scope Integrity

When implementation or validation evidence comes from a sub-agent, verify that
the agent stayed inside the exact work order before accepting the evidence:

```text
work_order_scope:
exact_allowed_files:
actual_changed_files:
implementation_authorized_for_this_slice:
commit_push_authorized_for_this_exact_file_set:
allowed_git_commands:
forbidden_commands:
parent_will_perform_commit_push_after_review:
commit_only_work_order:
may_derive_next_phase:
must_stop_after_report:
subagent_self_review_is_evidence_only:
parent_review_readback_done:
subagent_notification_checked_for_commit_push_claims:
parent_local_sidecar_work_while_waiting:
parent_read_governed_files_before_packet_return:
read_only_verification_after_packet_return_only:
evidence_text_files_utf8_no_bom_lf_no_trailing_whitespace:
json_evidence_parse_passed:
git_diff_check_or_no_index_equivalent_for_evidence:
format_only_parent_repair_recorded_as_worker_finding:
```

Bounded spawned execution check: Spawned bounded sub-agent is already the
sub-agent executor. If the work order gives allowed write scope, the receiving
sub-agent must directly create or edit the scoped artifacts and report evidence.
It must not invoke `codex.exe`, CLI Codex, another AI, or another sub-agent to
satisfy "through sub-agents" unless the work order explicitly asks it to
delegate. `codex.exe Access is denied is not a blocker` for a spawned
sub-agent artifact task; treat it as proof of an invalid nested-delegation
attempt. If scoped files cannot be written for filesystem reasons, the packet
must name the exact path, attempted operation, and filesystem error instead of
demanding parent direct execution. Parent direct execution remains forbidden,
and sub-agent execution is not parent direct execution.
Verification literals: `Spawned bounded sub-agent is already the sub-agent executor`; `codex.exe Access is denied is not a blocker`; `Parent direct execution remains forbidden`; `sub-agent execution is not parent direct execution`.

Implementation integrity fails when the sub-agent:
- created artifacts for a later phase not named in `work_order_scope`;
- committed or pushed without a commit/push-only work order or exact file-set
  authorization;
- combined artifact/code/content edits with commit/push authority in one work
  order;
- omitted `parent_will_perform_commit_push_after_review=true` from a normal
  artifact/code/content slice;
- ran `git add`, `git commit`, `git push`, `git checkout`, `git reset`,
  `git branch`, `git tag`, `git merge`, `git rebase`, or any git write/ref
  command while `commit_push_authorized_for_this_exact_file_set=false`;
- emitted git directives in its final response without exact git authority;
- used `STATE.json next_action` or the active plan to continue after the return
  condition;
- relied on its own `parent-review` file as if parent review had happened.
- returned a notification that claims commit/push/directives and parent review
  did not explicitly inspect that notification.
- returned text evidence without UTF-8-no-BOM/LF/no-trailing-whitespace proof,
  applicable JSON/JSONL parse evidence, and `git diff --check` or no-index
  equivalent over the exact changed evidence files.
- required repeated parent format-only cleanup for evidence files instead of
  recording the formatting defect as a finding against the generating worker.
- the parent used sub-agent waiting time to read governed files, skill indexes,
  plans, code, reports, or state for independent loophole research, repair
  drafting, or non-overlapping substantive work before the sub-agent packet
  returned.
- tried to satisfy "through sub-agents" by invoking local `codex.exe`, CLI
  Codex, another AI, or another sub-agent when the work order did not explicitly
  authorize delegation.
- treated `codex.exe Access is denied is not a blocker` as a blocker instead
  of continuing direct scoped artifact work or reporting a real filesystem
  write blocker.

The repair is quarantine/validate/record/patch/continue under parent
ARCHITECT_INTERNAL control, not silent acceptance and not default revert.
Parent read-only verification is allowed after the packet returns and only for
verification/integration of that packet.

Parent Architect Subagent-Only Firewall compatibility: implementation integrity
fails when parent local sidecar work happens under sub-agent-only instructions.
Read-only verification is allowed only after a sub-agent packet returns; the no
local non-overlapping work escape is disabled for code, tests, reports, state,
plans, skills, and governance files; sub-agent waiting is not a stop.

Phase propagation integrity gates:
- Exact integrity literals for verification: `boundary/API evidence != host readiness`; `schema != training`; `current-state invariants + evidence packet`.
- Machine-only statuses are invalid. A returned packet must include phase/step
  meaning, counts, done/in-progress/remaining, review findings, fixes, and next
  action before any machine label, JSON path, STATE flag, or validator code.
- Review must not overclaim evidence boundaries: boundary/API evidence != host
  readiness; marker-only != full local desktop pass; trace != savings; schema
  != training; checkpoint candidate != promotion. Accept only the narrow claim
  proven by the packet.
- For untracked new artifacts, accept current-state invariants + evidence
  packet instead of an impossible git baseline only when exact paths, explicit
  counts/claim=false, exact search terms, and no readiness claim are present.
- No stop gates from protocol repair: after a reviewed protocol/governance/
  skill repair, implementation integrity expects continuation to the next
  bounded phase unless GOLD GATE, missing authority, unsafe action, or a real
  blocker remains.
- Repeated self-block drift must be recorded as governance drift and repaired
  or avoided in later work orders. Nested sub-agent/codex CLI searching after
  scoped write authority is an integrity finding, not a blocker.

## Check 11 - Full Extension Definition Gate

When a code/runtime/plugin slice claims Claude, Cursor, or Codex extension
completion, implementation integrity requires host-specific evidence, not only
reports or internal helpers:

```text
host:
real_host_entrypoint:
settings_debug_surface:
target_locator_or_core_api_path_into_llm_mvp_core:
prompt_to_task_path:
task_to_orchestration_path:
task_or_plan_to_skills_path:
local_adaptive_learning_path:
export_import_backup_continue_training_evidence:
qa_live_host_or_explicitly_scoped_handler_evidence:
core_logic_owned_by_llm_mvp_core:
```

JSON, PowerShell, manifests, static validators, dashboard-to-core calls, or
helper-only invocations are not implementation reality for a full extension
unless the real host entrypoint and all rows above are proven. Missing rows keep
the slice at `IN_PROGRESS_NEXT_STEP_KNOWN`.

Full Extensions Readiness Review Gate: integrity review must list Claude,
Cursor, and Codex as separate host rows. For each host, verify the real host
entrypoint/runtime path, settings/debug surface, target-locator or core API path
into `llm_mvp_core`, prompt-to-task path, task-to-orchestration path,
task/plan-to-skills path, local adaptive learning path, local model
backup/export/import/share/continue-training evidence, DPO construction
quality, hidden deterministic shortcut sweep, tests/QA evidence, and docs. Smoke
tests, API-only calls, dashboard-to-core calls, helper invocations, manifests,
PowerShell, JSON, and static validators are narrower evidence and cannot pass
full extension readiness.

## Check 12 - Model/Reasoning Configuration Evidence

Any implementation, review, or execution packet produced by a sub-agent must
state `requested_model=gpt-5.5` and
`requested_reasoning_effort=xhigh`. Medium/high/low/default/lower-tier or
unspecified settings without an explicit current Luba downgrade quote are a
governance violation and cannot support an implementation integrity pass.
Warning/stop is required only for concrete visible runtime/request metadata or
explicit work-order metadata that differs from the requested values. Generic
system/developer identity text such as "GPT-5" is not downgrade evidence by
itself. If exact runtime metadata is unavailable but the work order explicitly
requests `gpt-5.5` / `xhigh`, integrity review continues with
`mode_verified_from_work_order=true` unless a concrete contradictory
runtime/request value is visible. A false downgrade stop is a premature stop
loophole.

## Check 13 - Execution/Code Review Completeness

Before accepting a code, runtime, host, trainable, DPO, or dashboard slice,
implementation integrity must prove the code review covered the actual project
boundary, not only a status label or report.

Required evidence:

```text
exact_review_scope:
project_coding_standards_checked:
allowed_tests_or_gate_b_checked:
llm_mvp_core_core_logic_boundary_checked:
host_adapters_thin_checked:
hidden_deterministic_shortcut_sweep_done:
deterministic_helpers_labelled_eval_only_helper_internal_or_baseline:
deterministic_helpers_not_runtime_trainable_authority:
docs_qa_dashboard_impact_checked_when_applicable:
human_readable_status_counts_present:
```

Integrity fails when:
- the review does not name exact files/sections/hosts/claims inspected;
- core orchestration, skill-selection, DPO, trainable, or semantic logic sits in
  a Claude/Cursor/Codex host adapter instead of `llm_mvp_core`;
- a deterministic scorer, lookup, counter, exact replay, static fixture, or
  fallback is presented as trained/semantic runtime authority;
- eval-only/helper/internal baseline logic is present but not labelled as such;
- docs, QA, dashboard, or user-visible status impact is applicable but omitted.

Missing rows keep the slice at `IN_PROGRESS_NEXT_STEP_KNOWN` with the first
missing review row as the next action.

## Check 14 - Scenario Coverage and Final Between Bounded Slices Integrity

Implementation integrity fails if a report, packet, commit/push summary, or
near-final answer stops between bounded slices while scenario coverage is
incomplete and the active plan has a safe next action.

Required literals:
- `product_readiness_term_deprecated_use_scenario_coverage`
- `dual_path_same_scenario_coverage_gate`
- `no_product_ready_count_primary_status`
- `final_between_bounded_slices_forbidden`
- `final/status/report/commit-push/status-board is not a stop while authorized work remains`
- `status_board_counts_drive_continuation`
- `accidental_final_recovery_requires_subagent_repair_and_resume`
- `promise_to_continue_is_not_continuation`
- `commit/push boundary is not a stop`
- `next_subagent_started_or_real_blocker`
- `Dual-Path Same-Scenario Coverage`
- `scenario coverage`
- `Path A`
- `Path B`

Integrity checks:
- Deprecated term: use dual-path scenario coverage instead. Product readiness is
  not a working gate, primary status, stop reason, execution target, or counter.
- Reject `product-ready count = 0 because full host UI not proven`, or
  equivalent old full-live-host wording, and rewrite it as `Dual-Path
  Same-Scenario Coverage` with per-scenario Path A status, Path B status, exact
  Path B blocker if blocked, next scenario/product fix, and next bounded action.
- Each scenario is accepted only when the same scenario and same prompt pass
  Path A API/boundary and Path B `local-agent UI path` evidence.
- Reports must show `scenario coverage: total / accepted / api_only / ui_only /
  blocked / not_started` plus per-scenario Path A and Path B status. They must
  also state total scenarios, covered by Path A, covered by Path B, blocked
  Path B with reason, and remaining steps.
- If Path B cannot reach an extension/plugin route because of host limitations,
  auth, storage, a missing UI route, or an inaccessible app surface, mark that
  scenario as blocked Path B with exact screen/state and next repair; do not
  restore the old impossible live-host product-readiness gate.
- A promise to continue must be backed by a next sub-agent id, a returned
  packet, a wait/review action, or a real blocker. Otherwise it is a soft stop.
- A commit/push checkpoint cannot close the workflow unless
  scenario_coverage_complete=true or Luba explicitly stopped.
- A final-style summary cannot close the workflow unless Luba explicitly
  stopped/paused/asked for answer-only or all active requirements are complete.
- `status_board_counts_drive_continuation`: if `remaining_count > 0` and
  `next_bounded_action_exists=true`, stopping is an integrity failure; the next
  bounded sub-agent action must be started, waited, reviewed, or repaired.
- Commit/push of an evidence packet or status board is not a completion boundary
  when the status board says remaining groups exist.
- After an accidental final/status stop, the next packet must answer Luba first,
  classify the old answer, repair the exact loophole through bounded
  sub-agents, and resume the previously known bounded action without requiring
  fresh approval unless Luba explicitly stops or a real blocker/unsafe action
  exists.
- Before any final response, inspect: `scenario_coverage_complete?`,
  `explicit_user_stop?`, `active_requirements_complete?`,
  `status_board_remaining_count_zero?`, `next_bounded_action_exists?`,
  `next_subagent_started_or_real_blocker?`. False/false/false/false/true/false
  means the next action is bounded continuation, not final.

## Check 15 - Trainable Contract Reality Sweep (pre-DONE)

Before writing DONE for any trainable/DPO/graph/learning-signal slice,
implementation integrity must run this sweep against the DELIVERED code, not the
plan. Each row is a hard gate; a MISSING/FAIL row keeps the slice at
`IN_PROGRESS_NEXT_STEP_KNOWN` with that row as `next_action`.

```text
1. cross_file_contract_consistency:
   - every consumer-named type/unit resolves at its claimed owner file with the
     SAME name, SAME allowed level (ILogicalUnit vs helper), SAME I/O type;
   - every cross-block delegate owner pointer (_critic/_repair/"canon in block N")
     resolves to an actual instantiated contract+algorithm+status;
   - grep evidence per pointer, not a mention. Unresolved pointer = FAIL.

2. provenance_field_presence_by_actual_count:
   - the provenance/typed-state record is filled for EVERY required field on the
     claimed-done path, and the presence denominator equals the CURRENT locked
     field count (e.g. 20, RR6), not a stale count (18);
   - the locked count is identical across owner declaration, every consumer view,
     every diagram render, and every metric denominator (single-owner numeric
     parity). Any stale variant = FAIL.
   - a non-teacher ACCEPT branch (first-party baseline / self_best / client-local)
     that reaches the presence gate must have a per-field writer for each required
     field OR a sentinel convention distinguishing legally-inapplicable from empty;
     a global presence==1.0 claim with a filler instantiated on only one intake
     leg = FAIL.

3. single_owner_sweep_covers_all_carriers:
   - for a shared type or a locked value, the sweep inspects ALL carriers:
     owner declaration, every consumer reference, every note/UML render, every
     metric denominator, and every intake/producer leg. A carrier omitted from
     the sweep = FAIL.

4. per_level_ablation_with_number:
   - for EACH trainable level in the slice, the ablation (no trained state ->
     not ready / degraded) carries a concrete held-out number, and the held-out
     accuracy function + gold source for THAT unit/domain is instantiated;
     an ablation gate without a per-domain scorer and a number = FAIL.

5. reward_and_intake_join:
   - non-scalar chosen/rejected has an instantiated reward-scalarization function;
   - each declared learning-signal source (self_best / teacher-goal / FLOW-38 /
     seed-corpus) either joins the training loop through a named converter into
     the canonical builder input, or is explicitly marked out-of-scope for this
     slice with the effect on the named outcome stated. A designed ingress whose
     output lands in a deferred/non-activated leg = severed circuit = FAIL.

6. legal_teacher_accept_and_license_coherence:
   - the ACCEPT-assigning branch (allowlist/positive-open-weight) is an
     instantiated identity-match ALGORITHM (normalization, prefix/family, version),
     not a functional spec or a bare allowlist name;
   - a registry invariant binds each allowlist family to its canonical license,
     and the declared license is cross-checked against it (distrust the input
     license field, mirror the identity-from-denylist rule);
   - every ACCEPT-eligible ACTIVE family carries a concrete ProviderConfig
     (id/model_id/license/terms/service_mode) OR is explicitly deferred at every
     mention; a name-only eligible family (e.g. DeepSeek/Kimi/GLM/Mistral) whose
     ACCEPT path cannot be traced = FAIL.

7. seed_dataset_existence:
   - any trained-outcome claim whose training/held-out data depends on a seed
     corpus records EXISTS with a verified path/volume, OR is classified as a
     blocking design/dependency, not a "recommended default". recon-backing is
     not data-backing. MISSING seed corpus for a claimed trained outcome = FAIL.
```

Verification literals:
`recommended default path != verified dataset`;
`reward primitive without reward(side) extractor != covered`;
`designed entrypoint != closed circuit`;
`ablation gate without per-domain scorer+number != admissible`;
`locked count in owner only != single-owner parity`;
`bare allowlist name != instantiated ACCEPT algorithm`;
`declared license != canonical license without cross-check`.

If any Check 15 row is MISSING/FAIL, integrity is NOT confirmed even when build,
tests, DNA, and Check 5 pass.

## Check 16 - Clean-Mergeable-Into-Original-And-Main (pre-DONE / pre-commit)

Development/implementation level of the `clean-mergeable-into-original-and-main
invariant` (AGENTS.md OP-67; CLAUDE.md "Clean-mergeable-into-original-and-main
invariant pattern"; plan-review-SKILL.md FC-43; generated-code-review-SKILL.md
"Clean-Mergeable-Into-Original-And-Main Check"). Before any DONE claim or any
commit on a working/design branch (for example `DesignToCode-clean`), integrity
is NOT confirmed unless the slice still merges cleanly into BOTH the
original/base branch (for example `codex/chat-gpt-5.5`) AND `main`. Literal:
`every commit must merge cleanly into BOTH original and main at any time`.

```text
16. clean_mergeable_into_original_and_main:
    - mergeability_probe_run = true       # git merge-tree (or no-write merge) into <original> AND main
    - mergeability_into_original = clean  # 0 conflicts, else FAIL
    - mergeability_into_main = clean      # 0 conflicts, else FAIL
    - no_add_add_generated_evidence_shared_paths = true  # generated evidence on branch-unique/gitignored paths
    - shared_file_edits_coordinated = true # docs/skills, CLAUDE.md, AGENTS.md, shared contracts: additive/minimal
    - mergeability_evidence_line_present = true
```

Required evidence line:
`mergeability: clean into <original> AND main (git merge-tree, 0 conflicts)`
with the concrete original/base branch name substituted. Literals:
`mergeability check: clean into <original> AND main`;
`no add/add on generated evidence at shared paths`.

If any Check 16 row is MISSING/FAIL — probe not run, nonzero conflicts into
original OR `main`, add/add on generated evidence at shared paths, or a divergent
shared-file edit without coordination — integrity is NOT confirmed and
commit/push stays closed even when build, tests, DNA, Check 5, and Check 15 pass.
