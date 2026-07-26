# Score-Zero Investigation

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

## Core Problem

A named check that produces a zero score (or consistently fires when it
should not) must be investigated before any resolution is applied. The
most dangerous mistake is removing the named check: the check exists
because an iron rule exists, and removing it to clear the score creates
a silent failure — code that compiles, passes `dotnet test`, and looks
correct, but teaches wrong patterns to every model trained on it. This
skill defines the three fix classes, the six-step investigation protocol,
and the silent failure detection procedure that runs before any resolution
is applied.

---

## When to Invoke

- When score-interpretation-SKILL.md emits machine_constant < 0.85 hard stop
- When a named check consistently fires (score = 0 or near-zero) for 3+ sessions
- When a PRESCRIPTIVE score (> 0.90) suddenly drops to near-zero after a patch
- When meta-arbiter-SKILL.md classifies failure type as PERSISTENT (N ≥ 3 cycles)

---

## Rule 0 — Named Checks Are Never Removed

A named check (CF-N identifier) is never removed until the investigation
completes and a Class B or Class C resolution is explicitly approved.

```
Investigation in progress → named check stays active
Class A resolved         → named check stays (problem was prompt, not check)
Class B resolved         → named check stays (trigger narrowed, not removed)
Class C resolved         → named check may be modified (contract revision required)
```

The only Class C path that touches the check itself still requires a full
contract revision (DECISIONS-LOCKED.md update + cross-capability validation)
before the check is modified. Deletion is never the resolution.

---

## Section 1 — Three Fix Classes

### Class A — Prompt Addition

The model did not know the rule. The check is correct; the context package
is missing instruction.

```
Symptom:   Score = 0 on first occurrence of a capability class
           OR score = 0 on a new iron rule added after context package was created
Diagnosis: The model cannot satisfy the check because the check references
           a rule that is not present in the context package
Fix:       Add a named check + WRONG/CORRECT pair to the context package
           (prompt-patch-authoring-SKILL.md Rule 1)
Named check stays: YES — the gap was in the prompt, not the check
```

### Class B — Check Over-Fires

The check fires on patterns that legitimately satisfy the underlying iron
rule through a different (correct) path.

```
Symptom:   Score = 0 but generated code appears correct
           OR score = 0 only when a specific variant pattern is used
           (e.g., expression-bodied members when the check expects block bodies)
Diagnosis: The trigger pattern in the check is too narrow — it matches
           only one valid form of the correct pattern
Fix:       Narrow the trigger: add an exception clause to the check
           DO NOT remove the check — add the exception
           "CF-12 fires UNLESS the return is an expression body returning
            OperationResult<T> directly"
Named check stays: YES — with the exception clause added
```

### Class C — Contract Iron Rule Wrong

The iron rule encoded in the check itself is incorrect or has been
superseded by a locked decision.

```
Symptom:   Investigation shows the check fires on patterns that are
           explicitly permitted by DECISIONS-LOCKED.md
           OR the iron rule was written for an older version of the architecture
Diagnosis: The contract needs revision, not the code that generates it
Fix:       Requires full contract revision:
             1. Load DECISIONS-LOCKED.md — verify the conflict is real
             2. Propose decision-reopening-SKILL.md challenge
             3. Update the iron rule with the correct constraint
             4. Cross-validate against all capability classes that use this check
             5. Re-score all capability classes affected
Named check stays: YES — modified, never deleted
```

---

## Section 2 — Six-Step Investigation Protocol

```
Step 1: Identify which named check fired
  grep -rn "CF-[0-9]" tests/TrainingData/ -- find the specific check
  Record: CheckId, CapabilityClass, ObservationCount (how many sessions it fired)

Step 2: Trace to the CF-N iron rule
  Load the iron rule definition from the context package
  Confirm: is this a Rule 1-10 DNA rule, a locked decision, or a capability-specific rule?

Step 3: Locate the violation line in generated code
  Run the generator for this capability class
  Identify the exact line that triggers the check
  Record: literal code line, generator ID (A or B for multi-generate)

Step 4: Classify the fix
  □ Is the iron rule present in the context package? NO → Class A
  □ Does the code satisfy the rule through a different valid path? YES → Class B
  □ Does DECISIONS-LOCKED.md permit the code pattern? YES → Class C
  (If none: re-examine Step 2 — the iron rule trace may be wrong)

Step 5: Silent failure simulation (required for all Class B and Class C)
  Silent failure definition: code that compiles, passes dotnet test,
  and looks correct — but encodes a wrong practice that future models
  will learn from the training corpus.

  Simulation:
    Generate 5 outputs with the current check active
    Generate 5 outputs with the proposed exception/modification
    Compare: do any of the modified outputs teach a wrong pattern?
    If yes: the exception is too broad — narrow further before applying.

Step 6: Resolution record
  Record in STATE.json and training data log:
  { "checkId": "CF-12", "fixClass": "B", "sessionId": "...",
    "resolutionApplied": "added expression-body exception",
    "silentFailureSimulation": "PASSED",
    "checkModified": false, "checkDeleted": false }
```

---

## Section 3 — Silent Failure Subtypes

```
Subtype 1: Compiles + passes tests but wrong API
  Example: method returns void instead of OperationResult<T> but all tests
           use the void path so they pass
  Detection: Check that test methods call .IsSuccess or .Errors on the result

Subtype 2: Correct structure but wrong semantics
  Example: OperationResult.Ok() returned with empty payload when the domain
           rule requires a populated payload
  Detection: Check that test assertions verify .Value, not just .IsSuccess

Subtype 3: Correct pattern but configurable value hardcoded
  Example: Confidence threshold 0.75 hardcoded instead of loaded from IConfiguration
  Detection: machine_constant sub-score < 0.85 (the primary trigger for this skill)
```

---

## Anti-Patterns

1. **"Removing the named check to clear the score"** — this is the most
   destructive action possible. The check encodes an iron rule. Removing
   it creates a training corpus that teaches the wrong pattern to every
   future model trained on it.

2. **"Skipping the silent failure simulation for Class B"** — a Class B
   exception that is too broad allows patterns that look correct but encode
   wrong practices. The simulation catches this before it enters the corpus.

3. **"Treating a Class C (contract wrong) as a Class A (prompt missing)"** —
   adding a prompt addition to fix a contract violation makes the context
   package self-contradictory. Diagnose before applying.

4. **"Applying Class B fix without checking cross-capability impact"** —
   an exception clause in CF-12 affects every capability class that uses CF-12.
   Run cross-capability validation before applying the exception.

5. **"Machine_constant investigation skipped because composite score is high"** —
   machine_constant < 0.85 is a hard stop regardless of composite score.
   A 0.88 composite with machine_constant 0.71 is a silent failure waiting
   to corrupt training data.

---

## Integration

- score-interpretation-SKILL.md emits machine_constant < 0.85 hard stop that triggers this skill
- prompt-patch-authoring-SKILL.md Rule 4 routes STRUCTURAL failures here before patching
- meta-arbiter-SKILL.md Step 1 (retrieval gap diagnosis) calls this skill for persistent check failures
- decision-reopening-SKILL.md is required for Class C contract revision
- planning-dpo-authoring-SKILL.md records investigation outcome as VALIDATED triple after resolution
