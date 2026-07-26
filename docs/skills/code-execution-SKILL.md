# code-execution-SKILL.md — Test gate, tier system, Gate B format

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

## Code Execution Pre-Final Continuation Enforcement Gate

- `pre_final_continuation_enforcement_required`: code execution handoff/status must include pre-final fields before any status, review, no-safe-work, completion, hold, PASS, or final packet: `active_task_open`, `next_bounded_product_review_or_governance_action_exists`, `active_or_returned_subagent_packet_pending_verification`, `required_passes_remaining`, `next_subagent_started_or_waited_this_turn`, and `true_external_blocker_certificate`.
- `parent_must_run_pre_final_check_before_status_final`: after governance repair, rule readback, or `NO_PATCH`, parent cannot issue status/final without this check; missing fields route to NEEDS_FIX/recovery.
- `final_packet_forbidden_when_next_bounded_action_exists`: if product/review work remains, final/status/completion/hold/no-safe-work is forbidden unless a true external blocker certificate has exact proof.
- `governance_repair_acceptance_requires_next_worker_started_or_true_blocker`: governance repair is acceptable only when the same chain starts, waits for, or reviews the next product/review worker, or records a true external blocker.
- `after_governance_repair_spawn_or_wait_next_product_review_worker`: for the current 50-pass chain, code/execution status after governance repair must route to `final review pass 4/50`; stopping is `premature_final_after_governance_repair`.
- `NO_PATCH_requires_product_continuation_check`: `NO_PATCH`, PASS, NEEDS_FIX, timeout, usage limit, partial packet, worker silence, and stale status must become continuation/recovery input, not stop permission.
- `rule_text_present_but_not_enforced_is_failure`: rule text is not enforcement unless it blocks final/status when a next bounded action exists.
- `PRODUCT_CODE_CONTINUATION_ONLY`: after governance/skill/rule/agent-file
  repair for a live product/code/QA task, the next accepted continuation must be
  the product/code/QA execution worker or narrowed recovery worker for the
  concrete `audit_union_row_id` or `next_product_code_slice`. Code execution
  must reject replacing that worker with another governance worker, read-only
  scan, status package, `NO_PATCH`, rule readback, or packet-formatting repair.
- `PRODUCT_WORKER_NO_PACKET_ZERO_ACCEPTED_REQUIRES_RECOVERY`: `shutdown`,
  `not_found`, interrupted, no packet, or 0 accepted product evidence from the
  product/code worker is zero accepted product work and requires a narrowed
  product/code recovery worker before final/status.
- `subagent_wait_heartbeat_required_before_wait`: every wait/poll needs a human-readable heartbeat before it starts.
- `one_active_agent_does_not_mean_wait_silently`: one-active-agent mode requires bounded heartbeat waits and does not allow silent wait or final hold.

## Builder-Oriented Execution Work Orders

For product completion tasks, code-execution work orders must be delivery-first.
The default worker role is builder, not auditor. A work order may include review
or file reading, but it must aim at one of:

```text
code_change:
test_change:
runtime_proof:
qa_browser_proof:
true_external_blocker_certificate:
```

If the worker cannot safely edit code/tests, it must return
`NEEDS_FIX_NO_CODE_CHANGE` with the smallest replacement slice:

```text
files_to_modify:
tests_to_add_or_run:
scenario_to_prove:
expected_new_evidence:
why_no_code_change_was_safe:
```

A code-execution packet that only adds a ledger/review/status section is not a
completed execution slice. It is documentation/status progress and must route
to a narrower product worker unless a true external blocker is proven.

Verification literal:
`Code-execution sub-agents must build or return the next code/test/runtime slice.`

## Independent Audit Union Execution Gate

Code execution must consume independent audit maps as a product backlog, not as
status statistics. If Fable, Opus, Claude, benchmark reviewers, or another
strong reviewer reported `MISSING`, `PARTIAL`, `SCAFFOLD_ONLY`,
`OPT_IN_UNWIRED`, edge cases, security/ops holes, trainable-route holes, or
runtime-default-chain failures, the executor work order must name the specific
union-board row it is building or proving.

Required execution fields:

```text
INDEPENDENT_AUDIT_UNION_TO_PRODUCT_BOARD_GATE:
  audit_union_row_id:
  source_audit_tag: BOTH | FABLE_ONLY | OPUS_ONLY | CLAUDE_ONLY | BENCHMARK_ONLY
AUDIT_STATS_ARE_NOT_PRODUCT_PROGRESS_GATE:
  product_progress_kind: code_change | test_change | runtime_proof | qa_browser_proof | true_external_blocker_certificate
EVERY_UNIQUE_AUDIT_HOLE_BECOMES_PRODUCT_SLICE_GATE:
  capability:
  files_or_modules_to_inspect:
  files_or_modules_to_change:
  tests_or_proof_to_add_or_run:
  expected_product_behavior:
  acceptance_evidence:
SINGLE_MODEL_BLIND_SPOT_GATE:
  only_fable_or_only_opus_row_kept_open_until_proven: true
SECURITY_OPS_LIFECYCLE_ARE_PRODUCT_GAPS_GATE:
  security_ops_lifecycle_scope_checked: true
REAL_TRAINER_ROUTE_B_GATE:
  trainer_route_or_checkpoint_lifecycle_scope_checked: true
POST_GOVERNANCE_RETURN_TO_PRODUCT_GATE:
  next_slice_after_governance_repair_named: true
```

A worker that only writes a better audit map, dedup table, or status package
returns documentation/status progress, not product implementation progress. The
next worker must build or prove one bounded row unless a true external blocker
certificate exists.

Current known first product slice after this governance repair: fix/verify the
live-local startup crash caused by `artifact://` fake PNG after stricter
screenshot proof. The fix must preserve fake-proof rejection and use a
deterministic local PNG or bounded non-ready, then run the focused proof.

## Route Choice Belongs To Builder Unless It Is Product Authority

If implementation can proceed through one of several technical routes, the code
executor must choose the smallest safe product-connected route for its bounded
slice. It must not return `ask Luba which route` unless the remaining choice is
a genuine product/business authority decision and no safe local code/test/runtime
slice can move independently.

Required packet fields when route ambiguity appears:

```text
chosen_product_spine:
why_agent_can_choose_locally:
rejected_routes_and_why:
files_changed_or_planned:
tests_added_or_run:
next_runtime_proof:
```

Verification literal:
`Technical route ambiguity is not a Luba stop; choose the smallest safe product spine.`
## v2.0: subproject gate + BaseService reminder added

## When to load
Load for any session that adds, modifies, or reviews code or tests.

## Human-readable plan/report precondition

Do not start implementation from an architecture, trainable, code-review,
documentation, or audit report that fails the Human-Readable Architecture
Artifact Gate. Code execution must reject table-only, shortcode-heavy,
English-only when the operator's language was required, schema-only, or prompt/spec inputs.

For suspicious Dictionary/counter/KNN/scorer/selector/private-method repairs,
the controlling artifact must pass the Concrete Repair Blueprint Gate: exact
classes/records/methods/tests/data rows, affected sets/checkpoints/manifests,
and lifecycle paths must be named. Generic "wrap in ILogicalUnit", "add KNN",
or "chosen/rejected/quarantine" text is not a decision.

Do not start implementation from an audit that claims "Architecture audit
sufficient/complete", "enough", "done", or "closed" unless it passed
the Audit Sufficiency Boundary Gate: exact bounded scope, green breadth
challenge, parent review question answered, and Delta-Severity Feedback Gate
recorded for later same-domain BLOCKING/HIGH misses.

## Sub-Agent-Only Execution Boundary

If Luba says the parent/current session must work through sub-agents, is
architect-only, governance-only, or forbidden to become executor, this skill
does not grant the parent permission to inspect, build, test, run, train, or
repair locally. Convert implementation, governance, skill, report, plan, data,
and code work into bounded sub-agent work orders.

No code executor may receive or keep an unbounded scope. "Do not stop" means
continue the parent-owned bounded phase chain, not one code executor running
until product ready, until the whole plan is closed, or until all remaining work
is done. Each code-execution work order must name the exact deliverable, allowed
files/commands, max active time of 10 minutes or less, required
protocol/evidence report, and `must_stop_after_report=true`; remaining work goes
back to parent review for the next bounded work order.

If a code executor is found running under an unbounded scope and has an id, the
parent must interrupt/close it before accepting more output. If the id is not
available, quarantine its output as evidence-only and replace it with a bounded
worker. A code executor that runs longer than 10 minutes without a
protocol/evidence packet is over-scoped: request or interrupt for the packet,
close if needed, split the scope, and continue. Deprecated `product readiness`
wording must not be used as a code-execution work-order scope or primary
status. Use bounded dual-path scenario coverage packets after code, review, and
test slices.

Every code-execution packet must open with human-readable operator's plain
language: total phases/steps, done/in-progress/remaining, what the phase solves,
what the current step solves, what evidence exists, what is not proven, and the
next bounded action. Machine labels and paths may follow only after this
explanation.

While a sub-agent is running, the parent may not use this skill to read governed
files or code for independent loophole research, repair drafting, or
non-overlapping work. Parent read-only verification is allowed only after a
sub-agent packet returns and only to verify/integrate that packet. Sub-agent
waiting is not a stop; wait/review/orchestrate instead of becoming executor.

Parent Architect Subagent-Only Firewall compatibility: code execution must
never be used as parent local sidecar work under sub-agent-only instructions.
Parent read-only verification is allowed only after a sub-agent packet returns.

## Runtime/Tool Command Construction Gate

UI/runtime/executor work orders must not use huge inline shell scripts. If a
command would exceed 20 nonblank lines, 4000 characters, or include more than
one function/class, the executor must write a scoped `script-file runner` under
the evidence/run folder and execute that file by path. The work order must name
the runner path, expected output, exit-code handling, stdout/stderr files, and
exact evidence filenames.

Windows error 206, oversized inline shell command, or any tool-command
construction failure is a work-order/tooling failure, not a product blocker.
After such a timeout/failure, the next step is preflight-only script-file
prep/execution, not another huge inline command retry.

Node REPL execution must avoid `Node REPL top-level redeclaration` by using
`var`, scoped blocks, or fresh names. A REPL declaration collision is a
work-order/tooling failure, not product evidence and not a product blocker.

## Evidence Text Formatting Gate

Runtime and evidence-writing sub-agents must create every text evidence
artifact in commit-ready format on the first write: UTF-8 without BOM, LF line
endings, and no trailing whitespace. JSON and JSONL evidence must parse before
the packet returns.

Before returning, run `git diff --check -- <changed evidence files>` for
in-repository changes. For generated, untracked, or no-index evidence files,
run an equivalent explicit scan that checks UTF-8 BOM, CRLF, trailing
whitespace, and JSON/JSONL parsing when applicable. The packet must list the
files checked, command or script path, exit code, and result. Repeated
format-only parent repair is a finding against the generating worker, not
normal cleanup.

## Zero failures is a hard gate — Rule 4
dotnet test must report 0 failed before every commit.
Not "only new tests fail." Not "pre-existing failures excluded." Zero.

## One test = one metric (Rule 8)

    // ✅ Assert.Equal(0.895, result.BoundaryAccuracy, precision: 2);
    // ✅ Assert.True(enhanced > baseline, $"expected enhanced > {baseline}, got {enhanced}");
    // ❌ Assert.True(result.IsSuccess);  // too vague — what succeeded?

## Test-before-after rule (Rule 7)
Every placement in a plan has a metric measured before and after.
The test captures both numbers and asserts enhanced >= baseline.

## Tier system for experiments

    Tier 1: 1-10 files    (unit validation)
    Tier 2: 11-100 files  (integration signal)
    Tier 3: 100+ files    (production signal)

FAILED at Tier 2 → escalate to Tier 3 (load experiment-design-SKILL.md).
SCALE_UP at any tier → needs more data, algorithm is correct.

## Subproject completion gate
Before any subproject phase is marked done, verify:

    dotnet build LlmFromScratch.sln                       # zero errors
    dotnet test Tests/LlmFromScratch.Tests.csproj         # zero failures, count >= baseline
    git diff --name-only                                  # only expected files changed

## BaseService rule — every new service class

    public sealed class MyService : BaseService, IMyInterface
    {
        public MyService(ISessionStorage storage) : base(storage) { }
    }
    // ❌ public sealed class MyService : BaseService  // missing interface
    // ❌ public sealed class MyService : IMyInterface  // missing BaseService

## OperationResult<T> rule — every public method

    // ✅ public OperationResult<string> GetName() => ...
    // ✅ public Task<OperationResult<bool>> SaveAsync(...) => ...
    // ❌ public string GetName() => ...   // no OperationResult wrapper
    // ❌ public bool Save() => ...        // bool without context

## Final Between Bounded Slices Code-Execution Gate

`product_readiness_term_deprecated_use_scenario_coverage`: deprecated term: use
dual-path scenario coverage instead. Code/execution packets must not use product
readiness as a working gate, primary status, stop reason, execution target, or
counter.

`dual_path_same_scenario_coverage_gate`: a scenario is accepted only when the
same scenario and same prompt are proven through Path A and Path B. Path A is
the API/boundary path through adapter/API/core request, response, trace,
RequestedSkills, and AvailableSkills when relevant. Path B is the local-agent UI
path in the real Cursor/Claude/Codex application with screenshots, window
inventory, redaction log, and trace/correlation evidence when available.
Required exact phrase for Path B: `local-agent UI path`.
If Path B cannot reach an extension/plugin route because of host limitations,
auth, storage, missing UI route, or inaccessible app surface, report a blocked
Path B host-path blocker with exact screen/state and next repair. Do not revive
the old live-host product-readiness gate.

`no_product_ready_count_primary_status`: reports must say `scenario coverage:
total / accepted / api_only / ui_only / blocked / not_started`, plus per-scenario
Path A and Path B status. Human-facing reports must state total scenarios,
covered by Path A, covered by Path B, blocked Path B with reason, and remaining
steps.

`repo/slice-local no-safe-work is not a final stop`: a code/execution packet may
report no safe work only for its assigned repository, workdir, phase, board row,
or implementation slice. It must not turn that local finding into a whole-plan
hold. `whole-plan no-safe-work requires all participating repos/workstreams
audited`: a whole-plan no-safe-work packet must list the active repo list,
current implementation workdir inventory, and one safe/non-safe result per
repo/workstream. `cross-repo safe work must be exhausted before final hold`.
`F0 Luba-only choices block only decision-dependent product behavior, not
independent scaffolding/docs/tests/mocks/status/reporting/integration harnesses
or other repo work`.

Required exact literals:

```text
repo/slice-local no-safe-work is not a final stop
whole-plan no-safe-work requires all participating repos/workstreams audited
cross-repo safe work must be exhausted before final hold
F0 Luba-only choices block only decision-dependent product behavior, not independent scaffolding/docs/tests/mocks/status/reporting/integration harnesses or other repo work
```

`final_between_bounded_slices_forbidden`: a bounded executor/code slice may
return its packet, but the parent/current session must not convert that return,
or a later commit/push, into a final stop while scenario coverage is incomplete
or a known safe next bounded action exists.

`promise_to_continue_is_not_continuation`: "I will continue", "continue next",
or "next step" is not evidence that continuation happened. A valid packet
must either include the next bounded sub-agent id/packet evidence or state a
real blocker.

`commit/push boundary is not a stop`: after commit/push, continue to the next
known safe bounded slice in commentary.

Pre-final gate: `scenario_coverage_complete?`, `explicit_user_stop?`,
`next_bounded_action_exists?`, `next_subagent_started_or_real_blocker?`.
False/false/true/false means final is forbidden.
