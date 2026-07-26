# Diagnostic Escalation

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

## Core Problem

When a capability fails its Gate B, there are only four possible root causes:
wrong test data, missing implementation, broken wiring, or incorrect algorithm.
Patching the wrong layer wastes cycles — fixing a test when the wiring is broken,
or rewriting the algorithm when a test expectation was wrong. This skill defines
a 4-step diagnosis protocol that identifies the root cause layer before any fix
is proposed, and distinguishes fixes that require Luba's approval from those
that can proceed autonomously.

---

## When to Invoke

- Any Gate B failure (dotnet test produces a count below the declared passing threshold)
- A capability's Stage 3 cycle 2 still failing the same category as cycle 1
- A capability that regressed (score dropped since last session)
- Before proposing any architectural change to a failing capability

One diagnosis before fixing = zero "patched the wrong layer" wasted cycles.

---

## Section 1 — When to Fire

**Situation 1 — Single failure:**
One failing test after a change. Diagnosis runs; one fix proposed.

**Situation 2 — Repeated failure pattern:**
Same test category (same Category N from e2e-test-matrix-SKILL.md) has failed
in 3+ consecutive cycles. Diagnosis runs across all cycles; up to 3 fix proposals
ranked by expected impact.

**Situation 3 — Regression:**
Test count dropped since the previous passing baseline. Diagnosis runs regression
check first — identify what changed since the last green state.

**Does NOT fire when:**
- All Gate B conditions pass
- A test fails on a capability's very first cycle (no regression possible;
  Situation 1 applies, no regression check)

---

## Section 2 — The 4-Step Diagnosis Protocol

Run steps in order. Stop at the first step that identifies the failure layer.
Do not run all 4 steps and pick the most interesting — the first match is the root cause.

### Step 1 — Check test data

**Question:** Is the test failing because of wrong input data, not wrong behavior?

```
Check:
  Are hardcoded test values appropriate for this capability?
  Does the test exercise the declared scenario or a different one?
  Does the test fixture (Arrange section) match the contract?

If test data is wrong:
  → Failure layer: TEST_DATA
  → Fix: correct Arrange section in the test
  → STOP diagnosis here — do not patch the implementation for a test data error

If test data is correct:
  → Proceed to Step 2
```

---

### Step 2 — Check wiring

**Question:** Is the capability reachable by the test? Is the DI (Dependency Injection) registration present?

```
Check:
  grep -n "[InterfaceName]" [Project]/Program.cs
  dotnet build [Project] → 0 Error(s)

If registration missing:
  → Failure layer: WIRING
  → Fix: add DI registration in Program.cs
  → STOP diagnosis here — an unreachable capability cannot produce correct output

If project reference missing:
  → Failure layer: WIRING
  → Fix: add project reference in .csproj
  → STOP diagnosis here

If wiring is correct:
  → Proceed to Step 3
```

---

### Step 3 — Check implementation completeness

**Question:** Does the implementation have real content, or is it a stub?

```
Check:
  grep -n "NotImplementedException\|throw new\|TODO\|FIXME" [implementation].cs

If NotImplementedException found:
  → Failure layer: STUB
  → Fix: implement the method (Stage 2 incomplete)
  → STOP diagnosis here

If method is empty:
  → Failure layer: STUB
  → STOP diagnosis here

If implementation has real content:
  → Proceed to Step 4
```

---

### Step 4 — Check algorithm correctness

**Question:** Is the implementation's logic producing the wrong result?

```
Evidence:
  What does the test expect (Assert section)?
  What is the capability actually producing?
  Which iron rule does the test verify?

If the algorithm contradicts an iron rule:
  → Two sub-cases:
    A) The iron rule was derived correctly → ALGORITHM_ERROR: fix the implementation
    B) The iron rule itself is wrong → IRON_RULE_ERROR: re-derive via iron-rule-derivation-SKILL.md

  Distinguish A from B:
    Apply the iron-rule-derivation-SKILL.md quality test to the rule:
    □ Can it be violated without an exception?
    □ Is it stated without implementation detail?
    □ Does it have a testable failure consequence?
    □ Does it trace to a specific failure mode?
    If ANY check fails → Sub-case B → re-derive the rule first
    If ALL checks pass → Sub-case A → fix the implementation

If regression was detected (Situation 3):
  → Check what changed since last passing baseline (git diff)
  → If a recent change introduced the regression: regression fix has highest priority
  → Revert or correct the specific change, not the algorithm generally
```

---

## Section 3 — Proposal Format

Every fix proposal follows this format:

```markdown
## Diagnostic Escalation Proposal — [CapabilityName]

Situation: [1 — single / 2 — pattern / 3 — regression]
Failure layer (from diagnosis): [TEST_DATA / WIRING / STUB / ALGORITHM_ERROR / IRON_RULE_ERROR]
Diagnosis step that identified it: [Step N]

Evidence:
  Test name: [exact failing test name]
  Expected: [what Assert expects]
  Actual: [what was produced or observed]
  Diagnosis evidence: [specific grep result or diff that identified the layer]

Fix proposal:
  Type: [TEST_CORRECTION / WIRING_FIX / STUB_IMPLEMENTATION / ALGORITHM_FIX / RULE_REDERIVATION]
  Target: [exact file:line]
  Change: [specific change — not "fix the bug"]

Expected result after fix: dotnet test --filter "[filter]" → [N] passed, 0 failed
Regression risk: LOW / MEDIUM / HIGH
  HIGH = change affects >2 other test categories or touches a shared interface
```

---

## Section 4 — Escalation Triggers

Some failures must be escalated to Luba before any fix is applied:

```
Escalate immediately when:
  □ Fix would change a DNA Rule (Rule 1–10) — no DNA rule changes without Luba approval
  □ Fix would change a locked decision (docs/DECISIONS-LOCKED.md) — cannot override locked decisions
  □ Fix would change an interface (IXxx) — interface changes break all implementations
  □ Same failure has persisted for 3+ cycles with different fix attempts — architectural problem
  □ Regression check finds a change that was explicitly reviewed and approved — re-opening approved work

Do NOT escalate:
  □ TEST_DATA fixes (wrong Arrange values)
  □ WIRING fixes (missing DI registration)
  □ STUB implementations (filling in a NotImplementedException)
  □ ALGORITHM_FIX that affects only the failing test's scenario
```

---

## Section 5 — Escalation Briefing Format

When escalation is required, provide Luba with a complete briefing:

```markdown
## Diagnostic Escalation to Luba

WHY: [one sentence — specific reason for escalation]
TRAJECTORY: [cycle 1: failed category X / cycle 2: failed same category / cycle 3: same]
BEST STATE: [what was the last passing baseline and what changed since]

Gap analysis:
  Test: [name]
  Violation: [what iron rule or contract is violated]
  Root cause: [the specific structural issue, not just the symptom]
  Likely fix: [what structural change would resolve it]
  Fix type: [INTERFACE_CHANGE / DNA_RULE_CHANGE / LOCKED_DECISION_CHANGE]

Options:
  [A] [concrete action — cost, likely outcome]
  [B] [concrete alternative — different tradeoff]
  [C] Accept current state with acknowledged gap: [what is the known gap]
```

---

## Anti-Patterns

1. **"Fixing the algorithm when the wiring is broken"** — an unreachable capability
   cannot produce correct output regardless of algorithm quality. Steps 1–3 must
   pass before Step 4 is relevant.

2. **"Patching the test to match the wrong output"** — a test that is adjusted to
   match wrong behavior is not a passing test, it is a hidden failure. Distinguish
   TEST_DATA errors (wrong input) from ALGORITHM_ERROR (wrong output for correct input).

3. **"Three cycles of the same failure, try a fourth"** — three cycles without
   improvement is an architectural problem. Escalate. A fourth cycle without
   structural change produces the same result.

4. **"The iron rule must be wrong because the test fails"** — the test failing
   means either the implementation is wrong OR the rule is wrong. Apply the
   iron-rule-derivation-SKILL.md quality test before deciding.

5. **"Regression fix applied before checking what changed"** — a regression fix
   that guesses the root cause may break the passing tests while "fixing" the
   failing one. Always read the diff first (Step 4 regression check).

---

## Integration

- capability-improvement-cycle-SKILL.md Stage 3 (PATCH) invokes this skill when cycle ≥ 2 still fails
- iron-rule-derivation-SKILL.md Step 3 quality test is Step 4's Sub-case B check
- design-artifact-completeness-SKILL.md Checks 4–5 cover Step 2 (wiring) and Check 2 covers Step 3 (stub)
- multi-reviewer-design-SKILL.md escalation report format is used when escalation trigger fires
