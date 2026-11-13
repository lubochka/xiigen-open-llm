# Materialization Session Type

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

Many sessions start with the design already complete and the code already written,
but nothing connects them to the user-visible surface. Treating these sessions as
PLANNING sessions produces 10-15 turns of re-design work for decisions already made.
Treating them as EXECUTOR sessions skips the inventory and round-trip verification
that makes the wiring safe. MATERIALIZATION is a distinct session type for this
specific work shape: existing design + existing code + wiring gap to user-visible surface.

---

## When to Invoke

- At session start, when reconnaissance finds existing design artifacts for the work in scope
- When the user's request includes: "make this visible," "wire up," "connect X to Y,"
  "surface this to the UI," "make this work end-to-end"
- When design-artifact-completeness-SKILL.md reports PASS on checks 1-4 but the
  feature is not yet observable through the desktop application
- When claim-as-hypothesis-SKILL.md verifies "design exists" as VERIFIED

**Default rule:** If design exists (per design-artifact-completeness-SKILL.md) AND
the work touches user-visible surfaces, default session type is MATERIALIZATION.
Overriding to PLANNING or GENERATION requires written justification.

---

## Section 1 — MATERIALIZATION vs Other Session Types

| Type | Purpose | Plan shape |
|---|---|---|
| PLANNING | New design decisions to make | 10-15 turns, arbiter rounds, gate reviews |
| GENERATION | New code to produce (no design yet) | Phase-by-phase with Gate B per phase |
| EXECUTOR | Execute a plan authored by another session | Follows existing plan, no plan-authoring |
| MATERIALIZATION | Wire existing design + code to user-visible surface | 1-5 tasks, specific wiring changes |

MATERIALIZATION is not EXECUTOR with extra steps. EXECUTOR executes someone else's plan.
MATERIALIZATION authors its own 1-5 task plan before executing.

---

## Section 2 — Three Mandatory Inputs

A MATERIALIZATION session cannot start without all three.

### Input 1 — Design source pointer

Path to the existing design artifact(s) the session is materializing:
- Interface declaration in [src/SubprojectN/ICapabilityClass.cs](../COMPONENT-INTERFACES.md)
- Architecture decision in [docs/DECISIONS-LOCKED.md](../DECISIONS-LOCKED.md)
- Component specification in [docs/COMPONENT-INTERFACES.md](../COMPONENT-INTERFACES.md)

If the path doesn't exist or design-artifact-completeness-SKILL.md reports INCOMPLETE:
restart as GENERATION (populate the design first).

### Input 2 — Target user-visible surface pointer

Path to the UI component, demo stage, or observable output the session wires to:
- A Windows Forms demo stage in MainForm.cs
- A new xUnit integration test that exercises the end-to-end path
- A new capability visible through the desktop application UI

If the target surface does not exist: the session is MATERIALIZATION + GENERATION
combined — that is two sessions. Split them.

### Input 3 — Round-trip step pointer

Which step(s) of the mvp-round-trip-verification-SKILL.md 8-step round-trip does
this session advance? Example: "This session advances step 3 (capability callable
through IHybridRetriever) and step 4 (result observable in demo)."

Without this pointer, the session has no acceptance test — success is undefined.

---

## Section 3 — Four-Step MATERIALIZATION Process

### Step 1 — INVENTORY

Run claim-as-hypothesis-SKILL.md on every design claim + design-artifact-completeness-SKILL.md
on every subject artifact. Produce a list:

```
Artifacts present and complete (usable as inputs):
  □ [interface path] — Check 5: reachable ✅
  ...

Artifacts present but incomplete (blocking):
  □ [implementation path] — Check 3: specific ❌ (still a stub)
  ...

Artifacts referenced but missing (blocking):
  □ [expected path] — file not found
  ...
```

Session cannot proceed to Step 2 until every blocking gap is either resolved
(create the missing artifact) or explicitly scoped out with a written justification.

### Step 2 — IDENTIFY WIRING GAP

What specifically sits between the complete design and the user-visible surface?
Use concrete gaps with file:line references, not abstract descriptions:

```
✅ GOOD gap statement:
  "WeightedScoringService.Score() is implemented at src/22/WeightedScoringService.cs
   but MainForm.cs demo stage 3 has no button that calls it — grep shows 0 hits
   for 'WeightedScoringService' in MainForm.cs"

❌ BAD gap statement:
  "The scoring service is not connected to the UI"
```

### Step 3 — MINIMAL WIRING PLAN (1-5 tasks, hard upper bound)

Each task:
- Names 1-3 specific files
- Specifies the concrete change ("add Score() call at MainForm.cs:247 in the
  existing demo button handler")
- Has a verification step (usually a round-trip check or specific test)

```
Example 3-task plan:
  Task 1: Wire WeightedScoringService into demo Stage 3
    Files: MainForm.cs (add button handler + call), WeightedScoringService.cs (verify interface)
    Change: add ScoreButton_Click handler at MainForm.cs:~250
    Verify: dotnet test --filter "Demo_Stage3_ScoringButton_ProducesScore"

  Task 2: Add integration test for full scoring path
    Files: tests/IntegrationTests/ScoringPathTests.cs (new)
    Change: new test: input → WeightedScoringService → OperationResult<Score> → display
    Verify: dotnet test --filter "ScoringPath_EndToEnd" returns 1 passed

  Task 3: Add demo data for Stage 3
    Files: src/22/DemoData/ScoringDemoData.cs (new)
    Change: static fixture with 3 pre-built scoring scenarios
    Verify: demo loads without null reference exception
```

If the work genuinely needs > 5 tasks: either split into multiple MATERIALIZATION sessions
(one per round-trip step) or identify hidden GENERATION work and pull it into its own session.

### Step 4 — ROUND-TRIP VERIFICATION

Execute the mvp-round-trip-verification-SKILL.md per-step verification commands for the
round-trip steps declared in Input 3.

The session's acceptance test is not "tests pass" — it is "the round-trip step declared
at Input 3 now shows PASS on its verification command."

---

## Section 4 — Output Shape Constraints

| Constraint | Value |
|---|---|
| Task count | 1–5 (hard upper bound) |
| Files changed per task | 1–3 typical; 5 maximum |
| New design decisions | 0 (if > 0, session is misclassified) |
| New capability archetypes | 0 |
| Arbiter panel rounds for plan review | 0 (plan review is not needed for wiring) |
| Round-trip step advances | ≥ 1 required |

---

## Anti-Patterns

1. **"Let's treat this as PLANNING for safety — more review is better"** — more review
   of the wrong session type adds design noise to wiring work. 10 turns of re-designing
   what's already decided is not safety; it is waste.

2. **"Design exists but I want to revise it while I'm here"** — design revision is PLANNING
   or GENERATION. MATERIALIZATION sessions do not re-design. If revision is needed,
   that is a separate session with a separate session type.

3. **"The task list is 7 tasks but each is small"** — the constraint is 5 tasks,
   not 5 large tasks. 7 tasks means hidden GENERATION work is in the task list,
   or the session needs to be split into two.

4. **"Round-trip pointer is optional — we'll know when it's done"** — without a
   round-trip pointer, success is undefined. The session produces output that "looks
   done" but cannot be verified as delivered.

5. **"Skipped Step 1 INVENTORY — design obviously exists"** — design artifacts that
   "obviously exist" sometimes contain stubs in check 3 (specific content) or check 5
   (reachable from calling code). Run the inventory; the check takes 5 minutes and
   prevents a session wasted on wiring to a stub.

---

## Integration

- session-mode-declaration-SKILL.md includes MATERIALIZATION as one of the 5 mode values
- design-artifact-completeness-SKILL.md runs against every artifact in Step 1 INVENTORY
- claim-as-hypothesis-SKILL.md verifies user design claims during Step 1 INVENTORY
- mvp-round-trip-verification-SKILL.md provides the acceptance test at Step 4
- goal-delivery-completeness-SKILL.md verifies task-to-goal mapping; for MATERIALIZATION
  sessions, "delivers goal" means "round-trip step declared in Input 3 passes verification"
