# Plan Execution Feedback

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

After each phase, compare planned assumptions vs actual discoveries.
Tracks how often plans required write-time fixes (assumptions that were wrong
and had to be corrected while writing code).

Execution feedback also preserves user-origin source evidence. Every user
prompt, feedback item, correction, rejection, user question, and assistant
answer that changes scope, gates, artifacts, evidence, or execution order must
be appended to the active plan's User Prompt Source Ledger before the next
phase is planned or reported complete.

Execution feedback must also satisfy the Human-Readable Plan/Execution Log
Gate. Section A cannot be only machine rows: it must state the human-readable
stage, current plan step, what changed, what remains, claims opened/closed, and
why the status is true.

When discovery rate consistently exceeds 120%, the planning process is
under-investing in infrastructure discovery (Gate 0).

---

## Metrics

```
Discovery rate = (write-time fixes actual) / (write-time fixes planned) × 100%
Test delivery rate = (tests delivered) / (tests planned) × 100%
```

A "write-time fix" is any assumption in the plan that turned out to be wrong
and required a correction when actually writing or reading the files.

---

## Interpretation Table

| Discovery Rate | Signal | Required Action for Next Plan |
|---------------|--------|------------------------------|
| ≤ 120% | Normal — planning was accurate | Proceed with standard Gate 0 |
| 121–200% | Gate 0 under-invested | Add explicit file:line verification to every plan element |
| > 200% | Plan assumptions are guesses | Do NOT write code without reading every referenced file first |

---

## Session End Measurement

After each phase, record in STATE-Pn.json:

```json
{
  "phase": "P2",
  "status": "COMPLETE",
  "planExecutionFeedback": {
    "writetimeFixesPlanned": 2,
    "writetimeFixesActual": 5,
    "discoveryRate": "250%",
    "signal": "GATE_0_UNDER_INVESTED",
    "examples": [
      "Assumed IVectorIndex had SearchRange() — it didn't; had to add it",
      "Assumed HybridRetriever constructor took 2 args — it took 3",
      "Assumed DI registration in Program.cs — it was in Module.cs"
    ],
    "testsPlanned": 10,
    "testsDelivered": 8,
    "testDeliveryRate": "80%",
    "userPromptSourceEntriesAdded": 0,
    "unmappedUserPromptSourceEntries": 0,
    "note": "2 integration tests deferred to next phase — infrastructure not ready"
  }
}
```

---

## Project History Tracking

Keep a rolling record across phases to detect patterns:

```markdown
## Plan Execution Feedback History

| Phase | WF Planned | WF Actual | Discovery Rate | Signal |
|-------|-----------|-----------|----------------|--------|
| P1 | 1 | 1 | 100% | Normal |
| P2 | 2 | 5 | 250% | Gate 0 under-invested |
| P3 | 3 | ? | ? | Not yet complete |
```

**Pattern detection:** If 2+ consecutive phases have discovery rate > 120%,
the planning process has a systematic problem. Root cause is almost always:
- Plans referenced file paths without verifying them (Gate 0 not run)
- Plans assumed interface signatures without reading the interface files (Rule 5 skipped)
- Plans assumed DI registration locations without checking

---

## Rules

1. Record WF planned vs actual in STATE-Pn.json after every phase
2. Record every plan-shaping user prompt, feedback item, user question, and
   assistant answer in the active plan's User Prompt Source Ledger
3. If discovery rate > 120% for two consecutive phases: the next phase plan MUST
   include `file:line` citations for every planned element before any code is written
4. If test delivery rate < 80%: investigate whether tests were deferred for a real
   reason or because the plan over-counted (description blocks vs actual test methods)
5. Report the discovery rate and user prompt source ledger deltas in the Section
   A execution summary

---

## Section A Format (Phase End Summary)

When writing the phase completion summary for Luba:

```
## Section A — Phase [N] Execution Summary

Deliverables:
  - [list every file delivered]

Test gate: [N] passed before → [N+M] passed after → +[M] new tests, 0 failures

Write-time fixes: [N] (planned: [M])
  Discovery rate: [N/M × 100%] — [NORMAL | GATE_0_UNDER_INVESTED]
  [If > 120%: list the top 2-3 examples of what was wrong in the plan]
User prompt source entries added: [N]
Unmapped user prompt source entries: [0 required]

Deviations from plan: [none | describe]
Human-readable stage: [stage]
Current plan step: [step]
Claims opened/closed: [opened] / [closed]
What remains and why: [plain language]
```

---

## Anti-Patterns

1. **Not recording write-time fixes** — cannot improve planning without data
2. **Under-counting planned WF** — "I planned for 0 surprises" is itself a planning failure
3. **Accepting > 120% as normal** — two consecutive high-rate phases demand process change
4. **Attributing high discovery rate to "complex task"** — complexity is not an excuse; complex tasks need deeper Gate 0
