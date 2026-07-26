# Training Data Gap Audit

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

## Full-Task Readiness Supersession Gate

PLAN_READY for planning/execution requires BOTH zero unresolved design/dependency blockers and full original-task coverage proven by minimum 30 sequential mental implementation passes, one at a time, not parallel. A later stricter full-task simulation, reviewer packet, or attachment that reports NOT_READY, zero FULLY coverage, uncovered original-task requirements, or unresolved design/dependency blockers supersedes any earlier narrow ready verdict.

When this happens, downgrade current readiness to PLAN_NOT_READY before any implementation or final ready claim, and propagate the downgrade into active plan artifacts plus the relevant skills/guides/agent files that allowed the stale claim. A plan-only wording patch is incomplete. Readiness-repair work orders must name skills, source attachments, active files, original-task requirements, good/bad examples, pass-count/evidence rules, verification commands, and explicit propagation targets. Attachment-reported simulations are evidence only; do not claim the worker ran those passes unless the worker actually ran or received independently inspectable records for them. Old PLAN_READY_FOR_LUBA_REVIEW may remain only as historical/pre-attachment/narrower-repair text with current PLAN_NOT_READY nearby.

PLAN_NOT_READY after full-task coverage simulation is not a final reporting boundary. When implementable design/dependency blockers remain, immediately convert the blocker inventory (for active-codex Pass06 and later: B-1..B-22 unresolved blocker inventory is mandatory; Missing blocker numbers are trace defects, not permission to drop them; older B1-B21/B5-absent evidence must become explicit trace-repair backlog, not a dropped row) into sequential bounded sub-agent plan-repair passes and continue until the plan reaches readiness or a true external blocker remains after mitigation/decision-prep. A final answer after only downgrading readiness is a protocol violation unless all implementable blockers have been repaired, delegated to active workers, or proven external/non-actionable. Minimum-pass requirements are evidence-bound: if Luba requires 50 passes with sub-agents one-at-a-time, run or receive 50 inspectable sequential sub-agent packets/records before claiming 50; no parallelism and no ledger-only inflation. Governance/skill repair must precede plan repair when the stop came from a governance loophole, but governance repair is not itself a completion boundary.

## What This Skill Prevents

The ContinuousLearningPipeline accumulates training examples (DPO triples or
preference pairs). If examples were generated while a component was incomplete
(STUB state, not ACTIVE), they may encode partial implementation patterns.
Left in the corpus, they teach the model an incomplete behavior as if it were
correct. The fine-tuning corpus permanently contains wrong patterns.

---

## When to Invoke

- When a component transitions from STUB/PARTIAL to ACTIVE (capability unlocked)
- When a capability gap in DomainModelRegistry is resolved
- Before any training run that uses accumulated examples
- After IncrementalTrainer or DpoTrainer is extended with new task coverage

---

## Step 1 — Find the Gap Window

Identify when the capability was STUB and when it became ACTIVE:

```bash
# Check when the component was implemented vs when examples were generated
# From git log:
git log --oneline --all -- Src/12.ContinuousLearning/[ComponentName].cs | tail -5
# Note: date component was committed as "complete" (ACTIVE)

# Find when it was added as a stub:
git log --oneline -- Src/12.ContinuousLearning/[ComponentName].cs | tail -1
# Note: date it was first created (STUB window starts)

# GAP_START = first commit date (stub created)
# GAP_END   = commit date when marked ACTIVE
```

---

## Step 2 — Find Training Examples in the Gap Window

```bash
# Check DPO triples or training records generated during the gap window
# These are stored in the training data directory or STATE files

grep -rn "generatedAt\|capturedAt\|sessionDate" docs/STATE-*.json | \
  grep -E "2026-0[1-4]-[0-9]+" | head -20
# → Filter to examples generated between GAP_START and GAP_END

# Also check any EXECUTION-LOG files for dpoTriplesCaptured
grep -rn "dpoTriplesCaptured" docs/ | grep -v ": 0"
```

---

## Step 3 — Classify Each Training Example

For each example generated during the gap window:

| Class | Condition | Action |
|-------|-----------|--------|
| RECOVERABLE | Example does not use the incomplete component's methods | Keep — capability gap did not affect the example |
| CONTAMINATED | Example uses stub methods that returned incomplete/mock results | Remove — teaches wrong integration pattern |
| CALIBRATION | Example uses the component but result is uncertain without full integration | Tag as not-counting-toward-threshold; regenerate after ACTIVE |

```bash
# Check whether a training example references the stub component
grep -rn "StubProvider\|InMemory\|MockData" \
  [training-example-file] | wc -l
# > 0 → CONTAMINATED
# = 0 → may be RECOVERABLE — check further
```

---

## Step 4 — Execute Remediation

```bash
# Remove contaminated examples
# (locate contaminated training example files and delete or mark invalid)
grep -rn "CONTAMINATED\|gapWindowContamination" docs/training-data/ | head -10

# Tag calibration examples
# In STATE-Pn.json or training manifest:
# "trainingDataQuality": "CALIBRATION",
# "countsTowardThreshold": false,
# "gapWindowContamination": true

# Update training count after cleanup
dotnet test --filter "TrainingData|DpoTriple" --no-build 2>&1 | tail -3
```

---

## Step 5 — Schedule Re-runs

For CALIBRATION and RECOVERABLE examples that need verification:

```bash
# Mark task types for re-generation now that the capability is ACTIVE
# Add to STATE-Pn.json under "scheduledReRuns":
# {
#   "component": "ComponentName",
#   "reason": "GAP_CLOSED",
#   "priority": "HIGH",
#   "scheduledAt": "2026-05-01"
# }
```

---

## Integration with ContinuousLearningPipeline

In the .NET project, the training data audit maps to:

| Step | .NET equivalent |
|------|----------------|
| Gap window | Period between first stub commit and ACTIVE implementation commit |
| Training examples | DPO triples captured in `Src/12.ContinuousLearning/` pipeline |
| Capability state | Component status tracked in `DomainModelRegistry` |
| Remediation | Remove/retag entries in training data store before IncrementalTrainer run |

---

## Anti-Pattern

Skipping this audit before a training run means the model learns from examples
generated when the system was incomplete. The corruption is invisible — the
training run succeeds, the tests pass, but the model has internalized partial
patterns. Audit before every training run that follows a capability gap closure.
