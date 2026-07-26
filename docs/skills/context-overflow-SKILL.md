# Context Overflow

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
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

## Overview

When a session runs out of context, in-progress work may be lost. This skill governs
what to do at three threshold levels so nothing important is lost.

---

## The Three Thresholds

### Threshold 1: > 35% Context Remaining — Continue Normally

Continue executing the plan. No special action required.

### Threshold 2: ≤ 35% Context — Wrap-Up Mode

Switch to wrap-up mode:

```
1. Finish the current step completely (do not start a new step)
2. Save STATE-Pn.json with status: "PAUSED_CONTEXT_LIMIT"
3. Record exactly which step was in progress
4. Run dotnet test and record the current count
5. Write the "Resume From" note (see format below)
6. Commit whatever is complete (even partial phase progress)
7. Do NOT start the next step
```

Resume From note format:
```
RESUME FROM: [timestamp]
  Last completed step: STEP N — [description]
  Current test count: [N] passed, 0 failed
  Next step: STEP N+1 — [description, file, Gate B]
  In-progress work: [describe anything done but not committed]
  Blockers: [any unresolved issues]
```

### Threshold 3: ≤ 25% Context — Emergency Stop

Stop immediately. Save and commit regardless of step completion status.

```
1. Stop mid-step if necessary
2. Save STATE-Pn.json:
   {
     "status": "INCOMPLETE_CONTEXT_OVERFLOW",
     "pausedAt": "[timestamp]",
     "lastCompletedStep": "STEP N",
     "partialWork": "[describe what was done but not finished]",
     "testCount": "[N] from last run",
     "nextStep": "STEP N+1"
   }
3. git add [all changed files]
4. git commit -m "WIP: context limit — phase N partial, step N+1 pending"
5. git push origin main
6. Report to Luba with the STATE file path
```

---

## How to Estimate Context Remaining

The session has no direct token counter visible to Claude Code. Use these proxies:

| Signal | Likely Threshold |
|--------|-----------------|
| Can still see early session messages in context | > 35% |
| System reminder shows compressed earlier messages | ≈ 35% |
| Large blocks of conversation appear summarized | ≤ 25% |
| Responses feel slower or shorter | Possible limit approaching |

When uncertain: assume lower estimate and wrap up.

---

## STATE-Pn.json at Context Wrap-Up

```json
{
  "phase": "P3",
  "status": "PAUSED_CONTEXT_LIMIT",
  "pausedAt": "2026-05-01T14:30:00Z",
  "testBaselineAtStart": 916,
  "testCountNow": 924,
  "completedDeliverables": [
    "Src/09.VectorSearch/BruteForceIndex.cs",
    "Tests/VectorSearch/BruteForceIndexTests.cs"
  ],
  "lastCompletedStep": "STEP 2 — BruteForceIndex unit tests",
  "nextStep": "STEP 3 — HnswIndex implementation",
  "partialWork": "none",
  "blockers": "none"
}
```

---

## Session Resumption Protocol

At the start of the next session, when resuming from context wrap-up:

```
1. git branch --show-current → confirm on main
2. dotnet build LlmFromScratch.sln → 0 errors
3. dotnet test --no-build → record new baseline
4. Read STATE-Pn.json → find lastCompletedStep and nextStep
5. Read all files referenced in completedDeliverables (Rule 5)
6. Continue from nextStep
```

Do not re-do completed steps. The STATE file is the source of truth about what was done.

---

## Anti-Patterns

1. **Trying to finish the phase before the context limit** — rushing causes mistakes; wrap cleanly
2. **Not saving STATE** — the next session has no way to know where to resume
3. **Not committing at wrap-up** — uncommitted work can be lost
4. **Over-estimating context remaining** — when uncertain, use the emergency stop protocol
