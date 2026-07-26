# Output Skepticism

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

## Output Pre-Final Continuation Enforcement Gate

- `pre_final_continuation_enforcement_required`: output skepticism must require pre-final fields before status, review, no-safe-work, completion, hold, PASS, or final wording: `active_task_open`, `next_bounded_product_review_or_governance_action_exists`, `active_or_returned_subagent_packet_pending_verification`, `required_passes_remaining`, `next_subagent_started_or_waited_this_turn`, and `true_external_blocker_certificate`.
- `parent_must_run_pre_final_check_before_status_final`: after governance repair, rule readback, or `NO_PATCH`, status/final without pre-final fields is unsupported and routes to NEEDS_FIX/recovery.
- `final_packet_forbidden_when_next_bounded_action_exists`: final/status/completion/hold/no-safe-work is forbidden while next bounded work, pending packet verification, or pass deficit exists without true external blocker proof.
- `governance_repair_acceptance_requires_next_worker_started_or_true_blocker`: governance repair can be accepted only when same-chain proof shows the next product/review worker started, waited, or was reviewed, or a true external blocker exists.
- `after_governance_repair_spawn_or_wait_next_product_review_worker`: after governance repair in the current review chain, continue to the next current unquoted product/review/governance work-order slice; stopping is `premature_final_after_governance_repair`.
- `NO_PATCH_requires_product_continuation_check`: `NO_PATCH`, PASS, NEEDS_FIX, timeout, usage limit, partial packet, worker silence, and stale status are continuation/recovery inputs, not stop permissions.
- `rule_text_present_but_not_enforced_is_failure`: rule presence is not enforcement unless final/status acceptance is blocked when next bounded action exists.
- `subagent_wait_heartbeat_required_before_wait`: before every wait/poll, require a human-readable heartbeat naming the worker, unaccepted result, and next wait/recovery step.
- `one_active_agent_does_not_mean_wait_silently`: one-active-agent mode requires bounded heartbeat waits and cannot justify silent waiting or final hold.

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

## What This Skill Prevents

Reporting a result without asking whether the evidence actually supports the claim.
Claiming "tests pass" from a partial run. Claiming "fixed" when only one test was checked.
Claiming "improved" when the metric used could improve while the problem stays the same.
Claiming "50 cycles" when only 50 ledger rows exist. Claiming "sub-agent did
it" when the parent did it.

---

## When to Invoke

Any response that contains a result claim. Trigger phrases:
- improvement / reduced / fixed / resolved / addressed
- percentage changes or comparisons
- completion claims: done / complete / finished / passes
- progress claims: improving / significant progress / major improvement
- count claims: N cycles / N reviews / N passes / N files / N checks
- actor claims: sub-agent did it / reviewer did it / tests ran / browser checked

If the response contains none of these: run light-mode check — "No result claims — skepticism N/A."

---

## The Three Skeptic Questions

Answer all three before sending. Each answer must be a specific statement with evidence,
not a reassurance. "I believe so" is not an answer.

### Question 1 — Refutation Evidence

"What observable evidence would prove this result wrong? Have I looked for that evidence?"

**For test pass claims:**
```
Refutation evidence:
  Evidence that would refute: dotnet test shows failed count > 0
  Did I look: YES — [paste the last 5 lines of dotnet test output including Failed: 0]
  OR: NO — I did not run dotnet test in this message → cannot claim tests pass
```

**For bug fix claims:**
```
Refutation evidence:
  Evidence that would refute: the original symptom still reproduces
  Did I look: YES — ran the specific test that exercises the symptom: [test name]
              Output: [paste result]
  OR: NO — did not re-run the symptom test → downgrade to "fix implemented, not verified"
```

**For DNA compliance claims:**
```
Refutation evidence:
  Evidence that would refute: grep finds violations in changed files
  Did I look: YES — ran grep -rn "throw new" [changed-files] → 0 matches
  OR: NO — did not grep → cannot claim DNA compliance
```

### Question 2 — Scope Validity

"Does the evidence I have cover the scope I am claiming?"

```
Scope validity:
  Scope claimed: [one file / one class / all tests / full suite]
  Evidence scope: [what was actually checked — filter used, files read]
  Coverage sufficient: YES / NO
  Count claimed: [N or N/A]
  Independent evidence records: [N or lower actual count]
  Actor claimed: [parent / sub-agent / tool / unknown]
  Actor evidence: [sub-agent id, command output, file diff, or NONE]
  If NO: downgrade claim to match actual evidence scope
```

Examples:
```
"Tests pass" without --filter → scope: full suite, evidence: full suite → PASS
"Tests pass" with --filter "TokenizerTests" → scope: one class, evidence: that class → PASS if claim is "Tokenizer tests pass"
"All tests pass" with --filter "TokenizerTests" → scope mismatch → FAIL → revise to "Tokenizer tests pass"
```

### Question 3 — Proxy Check

"Am I measuring a proxy that could improve without the underlying thing improving?"

Common proxies in this project:

| Proxy metric | Underlying thing | Can proxy improve without underlying? |
|-------------|-----------------|--------------------------------------|
| dotnet build: 0 errors | Tests pass | YES — build checks syntax, not behavior |
| grep: 0 violations | Code is correct | YES — grep checks patterns, not logic |
| Single test pass | All related tests pass | YES — test isolation may hide regressions |
| Test count increased | Test quality improved | YES — tests may be shallow assertions |
| Ledger row count | Review/fix cycles completed | YES - rows can be generated without independent review/fix passes |
| Parent self-review | Sub-agent review completed | YES - actor is different and independence is not proven |
| Summary says command ran | Command actually ran | YES - only command output/exit code proves execution |

```
Proxy check:
  Metric measured: [what command was run and what it showed]
  Underlying goal: [what was actually supposed to change]
  Are they the same: YES / NO — [one sentence connecting them]
  If NO: state explicitly what was and was not verified
```

Additional high-risk proxies:

| Proxy metric | Underlying thing | Can proxy improve without underlying? |
|-------------|-----------------|--------------------------------------|
| `passed: true` in a JSON report | Runtime capability works end-to-end | YES - report may be static, contract, mixed, or validation-only |
| `compatibility_report_ref` exists | Upgrade compatibility was executed | YES - the ref may point to a generic runner report |
| `rollback_ref` exists | Rollback was loaded and verified | YES - the ref may only be metadata |
| Plugin build passes | Plugin can use current model runtime | YES - build does not prove host integration |
| CPU/RAM `UNKNOWN` | Resource report complete | YES - UNKNOWN is missing data, not a metric |

For end-to-end, training, plugin, or upgrade claims, the proxy check must
explicitly answer:

```text
Does the evidence include runtime execution, not only generated reports?
Would the current runner/generator code reproduce the persisted report?
Does it reject validation_only: true?
Does it reject static/contract/mixed metric types for runtime claims?
Does it include numeric CPU/RAM/time where those metrics are required?
Does it prove rollback/load and compatibility probes actually ran?
Does plugin readiness avoid MOCK readiness for a full readiness claim?
If plugin orchestration is claimed, was the orchestrator prompt suite executed
through the plugin-facing entrypoint rather than only runtime/readiness loading?
Does "plugin-facing entrypoint" name a real VS Code command, panel, chat
participant, or Visual Studio command/tool-window path, rather than only an
internal helper?
Do prompt expected results include the orchestration graph: unit A-to-B pairs,
unit input/output surfaces, fan-out arrays, arbiter review, and reprocessing
paths where the prompt requires flow, generation, validation, or variants?
Does every validation project group have a documentation/RAG/plugin-scenario
matrix row that says where to look and what to do when a source is missing?
```

For full-plan completion claims, the proxy check must also answer:

```text
Does a completion coverage matrix exist?
Does it map every literal final-gate requirement to an artifact?
Does it compare exact expected sets to actual sets?
Does it reject bucket coverage when subtype coverage is required?
Does it reject "at least one" when the plan requires "all" or "every"?
Does the final readiness source_of_truth include every child gate the plan names?
Does the state file agree with the matrix and live git boundary?
Do state, self-review, matrix, and readiness metadata agree with current HEAD
instead of an older pushed commit or stale pending next action?
Does the matrix verify the current state invariant, rather than passing because
it describes the future final stop state?
If Luba challenged a prior completion claim, was a new-attempt score reset and
live-source recheck recorded in state?
```

If any answer is NO, the claim must be downgraded to the exact narrow evidence
available, and execution must continue from the first matrix gap.

---

## Output Skepticism Format (Before Reporting)

Run internally before sending any message with a result claim:

```
OUTPUT SKEPTICISM CHECK:

Q1 — Refutation evidence:
  [What would refute this + whether I looked + exact evidence]

Q2 — Scope validity:
  [Scope claimed] vs [scope of evidence] → PASS / FAIL

Q3 — Proxy check:
  [Metric measured] → [underlying goal] — same / different
  If different: explicitly state what was not verified

Overall verdict:
  ALL PASS → report claim with evidence
  ANY FAIL → revise claim to match evidence before reporting
```

For count or sub-agent claims, add:

```text
COUNT / ACTOR HONESTY CHECK:
  count_claimed:
  independent_evidence_count:
  ledger_rows_or_summary_only:
  subagent_id_or_NONE:
  allowed_wording:
```

If `independent_evidence_count < count_claimed`, or `subagent_id_or_NONE=NONE`
for a sub-agent claim, the response must say the narrow truth before anything
else.

---

## Worked Examples

### Example A — PASS

Claim: "BruteForceIndex tests pass after the zero-vector fix."

```
Q1 — Refutation evidence:
  Evidence that would refute: dotnet test shows Failed > 0
  Did I look: YES — ran: dotnet test --filter "BruteForceIndexTests" --no-build
  Output last 3 lines:
    Passed!  - Failed:     0, Passed:   12, Skipped:   0
  Verdict: PASS

Q2 — Scope validity:
  Scope claimed: BruteForceIndex tests (all 12)
  Evidence scope: --filter "BruteForceIndexTests" → 12 tests run
  Coverage: 12 of 12 BruteForceIndex tests — MATCH
  Verdict: PASS

Q3 — Proxy check:
  Metric: dotnet test pass count
  Underlying goal: BruteForceIndex handles zero vectors correctly
  Connection: the zero-vector test is in the 12 passing tests by name
  Verdict: PASS

Overall: ALL PASS → report: "BruteForceIndex tests pass (12/12). Zero-vector fix verified."
```

### Example B — FAIL → Revised

Claim: "The project is now bug-free."

```
Q1 — Refutation evidence:
  Evidence that would refute: any test shows Failed > 0
  Did I look: YES — ran dotnet test --filter "BruteForceIndexTests"
  But: did not run the full suite
  Verdict: FAIL (insufficient scope)

Q2 — Scope validity:
  Scope claimed: project (all tests)
  Evidence scope: BruteForceIndexTests only (12 of 916 tests)
  Coverage: 12/916 = 1.3% — far below full-project claim
  Verdict: FAIL

Q3: Not needed — Q2 already fails.

Overall: FAIL → revised claim:
  "BruteForceIndex tests pass (12/12). Full suite not run in this message.
  Cannot claim project-wide status without running dotnet test on the full solution."
```

---

## Anti-Patterns

1. **"I answered the questions in my head"** — answers must appear in the response; unwritten answers are unverifiable
2. **Listing the questions without answering them** — every question requires a specific, falsifiable answer
3. **"Q1 answer: I believe the fix is correct"** — belief is not evidence; name the observable that would refute it
4. **"Q3 answer: the metric and goal are related"** — relatedness is not sufficiency; answer whether the proxy could improve without the goal improving
5. **Skipping because "I just ran the command"** — if the output is not in the message, it cannot be verified

PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, active-codex must propagate exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.
