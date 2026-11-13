# Product Scope Validation

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

A wave of subproject phases can all pass Gate B individually while the
end-to-end product story is broken. Each subproject tested its own output;
no subproject tested the joint output. This skill defines the 5-step
post-wave validation that answers: "Does this wave advance a real product
scenario that a user of the desktop application can experience end-to-end?"

---

## When to Invoke

- After all phases in a parallel wave complete and merge to main
- Before declaring a wave "production ready" or "feature complete"
- When the user reports that individual subprojects pass but the demo fails
- Before writing the session handoff that closes a multi-subproject batch

---

## Section 1 — Five-Step Validation Protocol

Run all five steps in order. A FAIL at any step produces an INCOMPLETE verdict
and halts — do not skip ahead to produce a partial PRODUCT-READY verdict.

### Step 1 — Retrieve Original Product Intent

```
Source: the goal statement at the top of the session or the planning document
        that initiated this wave. Do not paraphrase. Quote verbatim.

Record:
  intent_statement: "[verbatim quote from session goal]"
  wave_scope:       "[list of subprojects included in this wave]"
```

If the intent statement cannot be found: emit CONTEXT_INSUFFICIENT (see
pipeline-position-check-SKILL.md Section 2 format) before proceeding.

### Step 2 — Define a Product Scenario

Translate the intent into a single concrete user-facing scenario:
a sequence of steps a user takes through the desktop application
that exercises the output of this wave end-to-end.

```
Scenario format:
  User action:     [what the user does in the UI]
  System response: [what the desktop application shows or computes]
  Exercises:       [which subproject outputs are exercised]
```

The scenario must name at least two subproject outputs. If it only names one,
the scenario is not testing cross-subproject integration.

### Step 3 — Trace the Scenario Against Live State

For each step in the scenario:

```
□ Does the required interface exist?     (grep for interface name in .cs files)
□ Is the interface implemented, not stubbed?  (check for STUB marker or TODO body)
□ Does the implementation reach the downstream consumer?
  (trace: producer.Method() → IAgentMessageBus or direct call → consumer.Method())
□ Is there a test that exercises this integration path?
```

A STUB answer at any trace step = INCOMPLETE.
An absent integration test = INCOMPLETE (not FAIL — document the gap).

### Step 4 — Validate Business Outcomes

This step checks product correctness, not code correctness.

For each scenario outcome:

```
WRONG question: "Does the method return OperationResult.Ok()?"
RIGHT question:  "Does the returned value produce a result the user would
                 recognize as correct for their action?"
```

Validation heuristics by archetype:

```
ROUTING archetype:
  Wrong: method returns RouteResult.Ok
  Right: the route chosen is the one a human reader would choose given the input

SCORING archetype:
  Wrong: score is non-null
  Right: the score is in the expected range AND higher scores correspond to
         better candidates (verify with 2 contrasting inputs)

RETRIEVAL archetype:
  Wrong: returns non-empty list
  Right: the top result is the most semantically relevant (spot-check with a
         query where the answer is unambiguous)

LEARNING archetype:
  Wrong: IncrementalTrainer returns Ok()
  Right: confidence on the affected capability class increased after the run
         (check DomainModelRegistry edge value before and after)
```

### Step 5 — Produce Product Validation Report

```markdown
## Product Validation Report — Wave [N]

### Intent Statement
[verbatim from Step 1]

### Scenario Tested
[from Step 2]

### Verdict
PRODUCT-READY | INCOMPLETE | BROKEN_DEPENDENCY

### Evidence
| Trace Step | Status | Evidence |
|---|---|---|
| [subproject A] → [subproject B] | PASS/FAIL/STUB | [grep result or test name] |

### Gaps (INCOMPLETE only)
| Gap | Type | Blocking? |
|---|---|---|
| [missing integration test] | DOCUMENTATION | No |
| [stub in critical path] | IMPLEMENTATION | Yes |

### Business Outcome Check
| Outcome | Correct? | Evidence |
|---|---|---|
| [scenario outcome 1] | YES/NO | [spot-check result] |
```

---

## Section 2 — Verdict Definitions

**PRODUCT-READY:** All trace steps pass, no stubs in critical path, business
outcomes verified with at least two contrasting inputs.

**INCOMPLETE:** Some trace steps fail or stubs exist in the path. The feature
is not ready for user-facing demonstration. List all gaps; classify each as
DOCUMENTATION (missing test, not blocking) or IMPLEMENTATION (blocking).

**BROKEN_DEPENDENCY:** A downstream component does not exist or its interface
has changed since the upstream was written. This is not the same as INCOMPLETE —
it requires a new phase to resolve, not just a gap document.

---

## Section 3 — Common Failure Patterns

```
1. "Wave passed Gate B but demo crashes"
   Cause: Gate B checked the subproject in isolation.
   Fix:   Run Step 3 trace — find the first nil/stub/missing interface in path.

2. "Retrieval returns results but they're wrong"
   Cause: Step 4 business outcome check was skipped.
   Fix:   Run contrasting-input spot-check (right answer vs wrong answer query).

3. "Business outcome correct for one input, wrong for another"
   Cause: Edge case missed in training data or iron rules.
   Fix:   Add the failing case as a DPO triple. Run IncrementalTrainer.

4. "Can't find original intent statement"
   Cause: Goal was never written verbatim. Paraphrased goals drift.
   Fix:   Use goal-context-persistence-SKILL.md next session. This time:
          ask Luba to state the intent before proceeding with Step 1.
```

---

## Anti-Patterns

1. **"All Gate B checks passed — wave is production ready"** — Gate B is per-phase.
   Product scope validation is post-wave. Different gate, different scope.

2. **"Business outcome is 'the method runs without error'"** — this is Step 3
   (code correctness), not Step 4 (product correctness). Run both.

3. **"Only one subproject in the scenario"** — a single-subproject scenario does
   not test integration. Add the downstream consumer or pick a different scenario.

4. **"Verdict is INCOMPLETE but no gaps listed"** — INCOMPLETE without a gap list
   is unusable. Name every gap. Classify each as DOCUMENTATION or IMPLEMENTATION.

5. **"Proceeding to next wave before closing BROKEN_DEPENDENCY gaps"** — a broken
   dependency in the current wave becomes a compounding blocker in the next.

---

## Integration

- phase-retrospective-SKILL.md runs after sequential phases; this skill runs after parallel waves
- pipeline-position-check-SKILL.md Q0d (consumer requirement) feeds Step 4 business outcome check
- mvp-round-trip-verification-SKILL.md Step 8 (end-to-end result observable) covers the same principle
- session-handoff-SKILL.md closes the wave: include the Product Validation Report as the state section
