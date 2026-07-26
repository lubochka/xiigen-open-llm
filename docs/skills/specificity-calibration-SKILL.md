# Specificity Calibration

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

## Specificity Pre-Final Continuation Enforcement Gate

- `pre_final_continuation_enforcement_required`: specificity review must require pre-final fields before status, review, no-safe-work, completion, hold, PASS, or final wording: `active_task_open`, `next_bounded_product_review_or_governance_action_exists`, `active_or_returned_subagent_packet_pending_verification`, `required_passes_remaining`, `next_subagent_started_or_waited_this_turn`, and `true_external_blocker_certificate`.
- `parent_must_run_pre_final_check_before_status_final`: after governance repair, rule readback, or `NO_PATCH`, missing pre-final fields invalidate status/final evidence.
- `final_packet_forbidden_when_next_bounded_action_exists`: final/status/completion/hold/no-safe-work is forbidden while next bounded work, pending packet verification, or pass deficit exists without a true external blocker certificate.
- `governance_repair_acceptance_requires_next_worker_started_or_true_blocker`: governance repair requires same-chain next product/review worker start, wait, or review, or exact true external blocker proof.
- `after_governance_repair_spawn_or_wait_next_product_review_worker`: for the current 50-pass review chain, the required next slice is `final review pass 4/50`; stopping after governance repair is `premature_final_after_governance_repair`.
- `NO_PATCH_requires_product_continuation_check`: `NO_PATCH`, PASS, NEEDS_FIX, timeout, usage limit, partial packet, worker silence, and stale status trigger continuation/recovery, not stop permission.
- `rule_text_present_but_not_enforced_is_failure`: rule text is not enforcement unless acceptance blocks final/status when next bounded action exists.
- `subagent_wait_heartbeat_required_before_wait`: each wait/poll needs a prior human-readable heartbeat naming the worker, unaccepted result, and next wait/recovery step.
- `one_active_agent_does_not_mean_wait_silently`: one-active-agent mode requires bounded heartbeat waits and does not permit silent waiting.

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

## Core Problem

An architect answer with no file:line references, no integer counts, and no verbatim
excerpts is an essay. It provides a frame for thinking but leaves the reader to do the
reconnaissance the architect already has access to. This skill makes concreteness a
mechanically scored property and rejects output below threshold before any STOP gate.

---

## When to Invoke

- Before every STOP gate — self-score the output before presenting it
- Before emitting any architect-level synthesis or plan review
- Before handing any plan to plan-review-SKILL.md (Gate B check includes evidence base)
- When reconnaissance-gate-SKILL.md wide-scope mode applies (doubles thresholds)

One self-score before STOP = zero "your answer is too abstract" correction cycles.

---

## Section 1 — What "Specific" Means

Three senses of specificity are often conflated:

1. **Word count** — more words, longer paragraphs. Not specificity; often the opposite.
2. **Narrow scope** — answering a smaller question. Orthogonal to specificity.
3. **Concreteness** — pointing at specific existing things. This is specificity.

This skill measures #3. Concreteness means the output points at **specific existing
things**: a file at a path, a count with a numerator and denominator, a verbatim string
from somewhere. Abstract output references categories without instances.

**Abstract:** "the handlers appear to be partially wired"
**Concrete:** "3 of 5 handlers in Src/12.ContinuousLearning/ have IAgentMessageBus
  injected; FeedbackHandler.cs:47 has no injection at all (grep: 0 hits for
  IAgentMessageBus in that file)"

---

## Section 2 — The Scoring Formula

Score = count(file:line refs) + count(N-of-M claims) + count(verbatim excerpts)

Each reference counts once regardless of how many times restated.

### Dimension 1 — file:line References

What counts as ONE reference:
- Exact file path + line number: `Src/12.ContinuousLearning/IncrementalTrainer.cs:247`
- Exact file path + method name: `FeedbackHandler.cs::EmitDpoTriple()`
- Exact file path + line range: `DomainModelRegistry.cs:145-152`

What does NOT count:
- Directory only: `Src/12.ContinuousLearning/` — no file, no line
- Category: "the learning pipeline handlers" — no specific entity
- Hypothetical path: "something like `Handlers/Feedback.cs`" — not verified

**Catches:** "referenced the right thing" failures. Without file:line, the output
has pointed at an idea, not an artifact.

### Dimension 2 — N-of-M Integer Claims

What counts as ONE claim:
- Concrete count with numerator and denominator: "10 of 14 handlers wired"
- Concrete count with scope: "388 lines in IncrementalTrainer.cs"
- Before/after counts: "was 4 of 14, is now 14 of 14"
- Zero-result grep: "0 hits for 'IIncrementalTrainer' in test files"

What does NOT count:
- "many" / "several" / "most" — no number
- "roughly half" — no specific count
- Ratios without bases: "50% coverage" with no denominator count

**Catches:** "claim about quantity without doing the count" failures.

### Dimension 3 — Verbatim Quoted Excerpts

What counts as ONE excerpt:
- Direct quote from file content: `"return OperationResult<Unit>.Fail("MISSING_FIELD", ...);"`
- Direct quote from error output: `"CS0246: The type or namespace 'DpoTriple' could not be found"`
- Exact symbol from grep result: `"CurriculumTier: null"`

What does NOT count:
- Paraphrase: "the file says the tier field should not be null" (not verbatim)
- Summary: "the validator requires a non-null tier" (summary, not quote)
- Generic code: `public void Method() {}` — not from a specific source file

**Catches:** "what does the code actually say" failures.

---

## Section 3 — Thresholds per Session Type

| Session type | file:line | N-of-M | verbatim | Total minimum |
|-------------|----------:|-------:|---------:|--------------:|
| EXECUTOR turn output | 5 | 2 | 3 | 10 |
| ARCHITECT answer | 5 | 3 | 3 | 11 |
| PLAN-REVIEW report | 10 | 5 | 5 | 20 |
| DESIGN-REVIEW output | 15 | 8 | 5 | 28 |

Total minimum is the primary gate. Each dimension also requires ≥1 reference (an
output with 11 file:line references and zero N-of-M claims is unbalanced — it passes
total but fails dimension diversity).

**Wide-scope mode doubles all thresholds** (applies when reconnaissance-gate-SKILL.md
has declared wide-scope mode for this session).

---

## Section 4 — Self-Check Protocol

Before any STOP gate, run the count on the output being presented:

```markdown
## Specificity self-check (specificity-calibration-SKILL.md)

Output scored: [session output from this response]

file:line references: [N]
  [list each: "path:line — what this reference shows"]

N-of-M integer claims: [M]
  [list each: "N of M — what is being counted"]

verbatim quoted excerpts: [K]
  [list each: `"quoted string"` — source file or command output]

Total: [N + M + K]
Threshold for [session type]: [threshold]
Dimension diversity: [≥1 in each dimension? YES | NO — which dimension is missing]

Verdict: PASS | FAIL — [reason if FAIL]
```

**If FAIL:** output rejected. Add specifics — run additional reconnaissance if needed,
re-count, retry. Do not present a FAIL output to the user.

**If PASS:** include the self-check block in the output (or log to session state).

---

## Section 5 — Anti-Gaming Rules

1. **Same file:line mentioned multiple times = 1 reference.** `FeedbackHandler.cs:47`
   cited five times counts as 1.

2. **Same count, multiple phrasings = 1 claim.** "10 of 14 wired" in paragraph 1
   and "10 of the 14 have injection" in paragraph 3 count as 1.

3. **Verbatim requires exact characters.** Changing `"CurriculumTier: null"` to
   `CurriculumTier: null` (removing quotes) breaks verbatim status. Copy exactly.

4. **File paths must be real paths in the actual codebase.** A hallucinated path is
   worse than no reference — it is wrong specificity. Verify by reading or globbing.

5. **Counts must be from reconnaissance, not from memory.** "I recall there were about
   14 handlers" is not a count. Count from a Grep or Glob result this session.

6. **Verbatim excerpts must come from files read this session.** Quotes from training
   data memory are plausible strings, not verbatim excerpts.

7. **Placeholder refs score 0.** `[file]:123` signals the session never actually looked.

---

## Section 6 — Worked Example: FAIL vs PASS

**FAIL output:**
```
"The pipeline handlers appear to be partially wired. Some learning signals are missing.
The session identified coverage gaps in the training subsystem."
```
Score: 0 file:line + 0 N-of-M + 0 verbatim = 0 of 11 required for ARCHITECT. FAIL.
The output labels abstractions; it does not point at artifacts.

**PASS output:**
```
"3 of 5 handlers in Src/12.ContinuousLearning/ have IAgentMessageBus injected
(grep: FeedbackHandler.cs:47 — 0 hits for IAgentMessageBus; IncrementalTrainer.cs:89
— import present). DomainModelRegistry.cs:145 has `CurriculumTier: null` on 8 of 14
DpoTriple records. 0 hits for 'TrainAsync' in the test suite (grep across Tests/).
The pipeline runs but produces `DPO_TRIPLE_INVALID: CurriculumTier is null` on
every triple captured."
```
Score: 3 file:line + 3 N-of-M + 3 verbatim = 9 of 11 required. FAIL — one more specific needed.
Adding one more grounded reference brings it to 10+. PASS.

---

## Integration

- reconnaissance-gate-SKILL.md: produces the evidence base the output should cite;
  specificity-calibration-SKILL.md scores the output; both are required in ARCHITECT sessions
- session-mode-declaration-SKILL.md: mode determines the threshold (ARCHITECT = 11, PLAN-REVIEW = 20)
- coverage-completeness-gate-SKILL.md: fires at STOP; specificity-calibration-SKILL.md
  self-check fires first — a FAIL on specificity means coverage claims are unsupported
- claim-verification-SKILL.md: claim verdicts must themselves meet the specificity threshold;
  abstract verdicts ("seems partially verified") fail

PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, active-codex must propagate exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.
