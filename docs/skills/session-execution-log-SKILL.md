# Session Execution Log

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

Every phase produces a structured execution log. This log is the single source
of truth for what happened in a phase: what files changed, what tests moved,
what was built, and what decisions were made. Future sessions read it.
Audit trails reference it.

---

## When to Write

After every phase gate passes — never before. If the gate fails (tests regress,
build errors), the execution log is NOT written. A log only exists for a phase
that completed successfully.

---

## File Location and Name

```
docs/EXECUTION-LOG-P[N].json

Examples:
  docs/EXECUTION-LOG-P5.json
  docs/EXECUTION-LOG-P6.json
```

---

## Schema

```json
{
  "phaseNumber": "P6",
  "phaseName": "IncrementalTrainer — LoRA fine-tuning via LoraApplicator + DpoTrainer",
  "completedAt": "2026-05-01",
  "durationMinutes": 52,

  "humanReadablePlanExecutionLog": {
    "stage": "Phase 6 completion",
    "totalMajorPlanBlocksOrCurrentPhaseTasks": "8 current-phase tasks",
    "completedCountAndNames": "6/8: read, implement, targeted tests, docs sync, gate check, package",
    "currentPlanStep": "STEP 4 - package trainable evidence",
    "currentActiveTaskAndOwner": "Package evidence - parent architect",
    "remainingCountAndNames": "2/8: parent review, commit/push boundary",
    "currentResultType": "artifact",
    "whatChanged": "LoRA trainer and tests were added",
    "whatRemains": "Next phase starts HnswIndex integration",
    "claimsOpened": [],
    "claimsClosed": ["Phase 6 Gate B"],
    "claimsStillClosedAndWhy": ["Final product readiness remains closed because later phases are not executed"],
    "whyThisStatusIsTrue": "Build and allowed phase gate passed with exact evidence",
    "nextRequiredAction": "Parent review then commit/push boundary"
  },

  "testBaseline": {
    "entry": 881,
    "exit": 916,
    "delta": 35,
    "command": "dotnet test LlmFromScratch.sln --no-build"
  },

  "buildStatus": {
    "errors": 0,
    "command": "dotnet build LlmFromScratch.sln"
  },

  "filesChanged": [
    {
      "path": "Src/12.ContinuousLearning/IncrementalTrainer.cs",
      "type": "created",
      "linesAfter": 187,
      "summary": "LoRA fine-tuning trainer using LoraApplicator + DpoTrainer"
    },
    {
      "path": "Tests/IncrementalTrainerTests.cs",
      "type": "created",
      "linesAfter": 312,
      "summary": "35 tests: unit tests for LoRA params, integration test for round-trip"
    }
  ],

  "artifactsRegistered": {
    "decisionsLocked": [],
    "interfacesAdded": ["IIncrementalTrainer"],
    "subprojectsExtended": ["12.ContinuousLearning"]
  },

  "gateResults": [
    { "check": "dotnet build LlmFromScratch.sln → 0 Error(s)", "passed": true, "value": "0 Error(s)" },
    { "check": "dotnet test → Failed: 0", "passed": true, "value": "916 passed, 0 failed" },
    { "check": "DNA Rule 1 — no throw in business logic", "passed": true },
    { "check": "DNA Rule 2 — no direct instantiation", "passed": true },
    { "check": "COMPONENT-INTERFACES.md updated", "passed": true }
  ],

  "gatePassed": true
}
```

---

## Writing Rules

1. Write AFTER the gate passes — never speculatively
2. All number fields must be exact — no estimates or "approximately"
3. `delta` must equal `exit - entry` — if negative, the gate should have failed
4. `filesChanged` must list every file touched during the phase
5. `completedAt` is the timestamp of the gate passing, not phase start
6. `gatePassed` is always `true` — a log only exists for a passed phase
7. Human-Readable Plan/Execution Log Gate: every log must include `stage`,
   total major plan blocks and/or current-phase tasks, completed count and
   names, `currentPlanStep`, current active task and owner/sub-agent, remaining
   count and names, current result type, `whatChanged`, `whatRemains`,
   `claimsOpened`, `claimsClosed`, claims still closed and why,
   `whyThisStatusIsTrue`, and `nextRequiredAction` in language a human can read
   without decoding machine status fields
8. If the phase follows governance repair, the log must reference the
   Execution Plan Visibility Gate chat packet and the 10 Review Cycle Gate
   outcome before runtime/code/training/host execution continues
9. Human gate for chat logs: if Luba cannot tell from the log what is done,
   what is running, what remains, whether the current result is code, artifact,
   review, policy, or evidence-only, which claims remain closed, and what
   bounded action comes next, the log fails and triggers governance/skill
   repair before product continuation.

---

## Positive Example

A phase P6 log produced after IncrementalTrainer implementation:
- `testBaseline.entry: 881` — confirmed from dotnet test before Phase 6 started
- `testBaseline.exit: 916` — confirmed from dotnet test after Phase 6 gate passed
- `testBaseline.delta: 35` — 916 - 881 = 35 ✅
- `buildStatus.errors: 0` — confirmed from dotnet build output
- `filesChanged` — lists every .cs file created or modified
- `gateResults` — every check from the EXECUTION file listed with pass/fail

---

## Negative Examples

```
WRONG: Writing the log before the gate runs
  → Log exists but tests actually regressed — the log is a lie

WRONG: Estimated test counts
  "testBaseline.exit": "approximately 916"
  → Must be the actual number from dotnet test output

WRONG: Missing filesChanged
  "filesChanged": []  when 4 files were actually modified
  → Every touched file must appear in filesChanged

WRONG: Machine-only primary status
  Gate rows exist, but there is no human-readable stage, current plan step,
  total/done/current/remaining counts and names, current result type, what
  changed, what remains, claims opened/closed, or why.
  → The log may be parseable, but it fails the Human-Readable Plan/Execution
    Log Gate.

WRONG: delta is negative
  "entry": 916, "exit": 905, "delta": -11
  → Negative delta = regression = gate should have failed
  → If this log exists, the phase was incorrectly declared complete
```

---

## Integration

```
Written by:      phase-completion-packager-SKILL.md (after gate passes)
Read by:         Next session's prerequisite-chain-SKILL.md (to verify test baseline)
                 chain-arithmetic-audit-SKILL.md (to verify test count chain)
                 mission-progress-SKILL.md (for ENGINE PROGRESS reporting)
Cross-checks:    implementation-integrity-SKILL.md (gateResults must include doc sync)
```
