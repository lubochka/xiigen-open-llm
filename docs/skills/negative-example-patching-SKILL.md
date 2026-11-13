# Negative Example Patching

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

## Core Problem

A context package that only shows what correct code looks like does not
teach a model to avoid the most common failure mode. If a model generates
code that violates CF-N (Capability Framework Rule N) ninety percent of
the time, a POSITIVE EXAMPLE in the context package teaches the other
ten percent — the model already avoids the correct pattern and is still
producing the wrong one. A NEGATIVE EXAMPLE labels the exact wrong output
and explains why it is wrong in the same breath. This skill defines the
nine-step patching procedure, the score decision table, and the cycle
limit that prevents runaway patching loops.

---

## When to Invoke

- When a capability scores 0.60–0.79 (PATTERN_MISSING bracket from score-interpretation-SKILL.md)
- When a specific Capability Framework rule repeatedly fails across multiple runs
- When prompt-patch-authoring-SKILL.md Rule 4 routes to "section rewrite" but the failing check is a single pattern
- When the same wrong pattern appears in the DPO (Direct Preference Optimization) rejected output two or more times

---

## Section 1 — NEGATIVE EXAMPLE Format

Every negative example follows this exact structure:

```
// ← WRONG: [reason — one sentence, specific to this context]
[bad code line or block]
// CORRECT: [exact passing pattern — one line if possible]
```

Example for Rule 1 (OperationResult<T> everywhere) violation:

```csharp
// ← WRONG: method throws instead of returning OperationResult — violates Rule 1
public void ProcessCapability(string capabilityId)
{
    throw new InvalidOperationException("capability not found");
}
// CORRECT: return OperationResult<Unit>.Fail("capability not found", "CAPABILITY_NOT_FOUND")
```

Example for Rule 2 (Fabric First) violation:

```csharp
// ← WRONG: concrete type instantiated in constructor — violates Rule 2 (Fabric First)
public IncrementalTrainer()
{
    _vectorIndex = new BruteForceIndex();
}
// CORRECT: inject IVectorIndex via constructor parameter
```

The WRONG line comes BEFORE the bad code so the model sees the label
before reading the pattern. The CORRECT line is a one-line remediation
that does NOT need to be a complete class — just the specific fix.

---

## Section 2 — Nine-Step Patching Procedure

```
Step 1: Confirm score band
  □ Run the capability pipeline → read the score report
  □ Confirm iron_rules sub-score or dna_compliance sub-score is in 0.60–0.79
  □ If score is below 0.60: ESCALATE — do not patch; use score-zero-investigation-SKILL.md

Step 2: Read failing Capability Framework rules
  □ Open the score report and list every failing CF-N rule
  □ For each failing rule: identify the exact wrong pattern in the generated output

Step 3: Generate NEGATIVE EXAMPLE
  □ Write one WRONG/CORRECT pair per failing rule using the format in Section 1
  □ The WRONG comment must name the specific CF-N rule (e.g., "violates CF-3 / Rule 1")
  □ The bad code must be the actual pattern from the generated output — not a hypothetical

Step 4: Read current context package version
  □ Open the context package file and record its current version number
  □ Note the existing NEGATIVE EXAMPLES section (or create it if absent)

Step 5: Compute new version
  □ Minor bump only: if current version is 3.2, new version is 3.3
  □ Major bump (N.0) is reserved for genesis rewrites (score < 0.50)

Step 6: Update context package
  □ Add the NEGATIVE EXAMPLE(S) to the context package's NEGATIVE EXAMPLES section
  □ Update the version header to the new version number
  □ Do NOT remove any existing NEGATIVE EXAMPLES — accumulate

Step 7: Verify version bump
  □ Read the updated file — confirm version header matches the computed new version
  □ Confirm the NEGATIVE EXAMPLE appears under the correct CF-N rule section

Step 8: Re-run pipeline
  □ Execute the capability pipeline with the updated context package
  □ Record the new score and sub-scores

Step 9: Compare and decide (see score decision table)
```

---

## Section 3 — Score Decision Table

| Result after re-run | Action |
|---------------------|--------|
| New score ≥ 0.85 | PASS — close patch cycle; DPO (Direct Preference Optimization) triple captured automatically |
| New score ≥ 0.70 | MINIMAL — apply one more targeted patch; begin cycle 2 |
| New score within ±0.02 of previous | PLATEAU — escalate to prompt-patch-authoring-SKILL.md Rule 4; negative examples cannot fix this |
| New score worse than previous | REGRESSION — escalate immediately; revert the context package update |
| New score < 0.60 | ESCALATE — negative example patching cannot address structural failure; use score-zero-investigation-SKILL.md |

---

## Section 4 — Cycle Limit

Maximum 3 patching cycles per capability before escalation:

```
Cycle 1: add NEGATIVE EXAMPLE → re-run → check decision table
Cycle 2: add second NEGATIVE EXAMPLE (different failing rule) → re-run → check decision table
Cycle 3: add third NEGATIVE EXAMPLE → re-run → if not ≥ 0.85 → ESCALATE
```

At cycle 3 with score still below 0.85: the context package has a structural
problem that negative examples cannot fix. Escalate to capability-convergence-SKILL.md
for a full representation review.

A DPO (Direct Preference Optimization) triple is captured automatically when
the capability first reaches ≥ 0.85 after patching:
- `rejected`: the output that produced the pre-patch score
- `chosen`: the output that produced the post-patch score ≥ 0.85
- `TeachingPoint`: derived from the WRONG comment of the resolved NEGATIVE EXAMPLE

---

## Section 5 — Accumulation Rule

```
Never remove a NEGATIVE EXAMPLE from a context package.
Each example adds a resolved failure mode to the model's training surface.
Removing one removes the constraint that prevents that failure from returning.
```

Negative examples accumulate across patch cycles. A context package at
version 3.5 may have five NEGATIVE EXAMPLES — each corresponding to a
previously resolved CF-N failure. All five remain.

---

## Anti-Patterns

1. **"I wrote a generic NEGATIVE EXAMPLE not taken from the actual output"** —
   a hypothetical bad pattern does not match the specific failure mode.
   Extract the wrong pattern from the actual generated output.

2. **"I patched three cycles and the score is still 0.72"** — cycle limit
   reached. This is a structural problem. Escalate to capability-convergence-SKILL.md.

3. **"I removed an old NEGATIVE EXAMPLE because it seemed outdated"** —
   the accumulation rule prohibits removal. The old failure mode can return
   if the constraint is removed.

4. **"Score improved from 0.65 to 0.83 — close enough"** — 0.83 is
   DETAIL_GAP bracket, not PASS. One more targeted patch to cross 0.85.

5. **"I applied the patch but didn't bump the version"** — context package
   versioning is required for the learning pipeline to detect package changes.
   Always bump minor version at patch time.

---

## Integration

- score-interpretation-SKILL.md routes PATTERN_MISSING (0.60–0.79) to this skill
- prompt-patch-authoring-SKILL.md Rule 4 routes section rewrites here when pattern is single-rule
- learning-signal-capture-SKILL.md captures DPO_TRIPLE automatically on cycle resolution
- score-zero-investigation-SKILL.md handles scores below 0.60 and REGRESSION escalations
- capability-convergence-SKILL.md receives cycle-3 escalations
