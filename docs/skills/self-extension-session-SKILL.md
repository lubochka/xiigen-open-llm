# Self-Extension Session

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

When a capability gap is identified — a missing interface, an absent provider,
a named check that does not exist — the natural response is to fill it
immediately during the current session. That shortcut bypasses the gates that
prevent filling a gap with the wrong solution. A SELF-EXTENSION session is a
governed session type specifically for closing capability gaps: it reads the
current state, classifies the gap, proposes a resolution, requires explicit
approval before building, verifies closure, and audits the training data
affected by the gap window.

---

## When to Invoke

1. capability-state-reader-SKILL.md detects a MISSING capability that blocks
   a planned phase
2. gap-to-proposal-SKILL.md classifies a gap requiring a new interface,
   provider, or named check
3. A topology phase discovers a missing dependency during infrastructure
   discovery (infrastructure-discovery-SKILL.md Gate 0)
4. A Phase E promotion gate fails because a required learning signal is absent

**Not initiated autonomously.** The detection is systematic; initiating
a SELF-EXTENSION session requires explicit direction from Luba.

---

## Section 1 — The Governance Chain

```
START

  Step 1: capability-state-reader-SKILL.md
    Read current capability manifest — what is ACTIVE, STUB, MISSING

  Step 2: gap-to-proposal-SKILL.md
    Classify the gap (what layer is wrong)
    Produce a resolution proposal (Level 1–4 from solution-scope-gate-SKILL.md)

  ⛔ STOP — present proposal — await Luba explicit "yes" before building

  Step 3: Execute build work
    Fabric interface only → no new session required; treat as MAINTENANCE
    New capability class required → use PLANNING session + EXECUTOR session
    Config / appsettings update → no new session required; treat as MAINTENANCE

  Step 4: implementation-integrity-SKILL.md
    Verify closure: 4 checks (docs / tests / DNA / no new gaps)
    Produce JSON integrity report

  Step 5: training-data-gap-audit-SKILL.md
    Audit capability runs during the gap window
    Classify affected DPO triples (RECOVERABLE / CONTAMINATED / CALIBRATION)
    Remediate before next training run

  ⛔ STOP — present integrity report + audit results — await Luba sign-off

END
```

---

## Section 2 — Session Type vs Other Types

| Session type | Scope | Gates |
|---|---|---|
| EXECUTOR | Write code for a known plan | DNA check, Gate B per phase |
| PLANNING | Design new capability | Plan gates, self-questioning |
| ARCHITECT | Structural analysis and synthesis | Reconnaissance gate, convergence |
| REVIEW | Audit a plan or code | Plan review, audit protocol |
| DEBUG | Fix a confirmed bug | Bug taxonomy, 3 failing tests |
| **SELF-EXTENSION** | **Close a capability gap** | **Steps 1→2→STOP→3→4→5→STOP** |

The critical distinction from MAINTENANCE: MAINTENANCE applies when the
fix is known and scoped. SELF-EXTENSION applies when the gap is identified
but the resolution is unknown — the proposal step determines the resolution.
Treating SELF-EXTENSION as MAINTENANCE skips the proposal step and the
first STOP gate.

---

## Section 3 — What SELF-EXTENSION Is Not

```
❌ Building a new fabric interface mid-EXECUTOR session without a proposal
   → A missing interface discovered during execution is a gap.
   → Stop the executor session. Write the gap. Start a SELF-EXTENSION session.

❌ Adding a named check to validate.handler without checking if it changes
   existing scores
   → A named check that fires on existing capabilities changes their scores.
   → Step 4 integrity check must verify no regression.

❌ Treating all MISSING capabilities as gaps requiring SELF-EXTENSION
   → A MISSING capability that was never needed is not a gap.
   → A MISSING capability that blocks a current phase is a gap.
   → The trigger is "blocks a phase" not "appears in the MISSING list."

❌ Closing a SELF-EXTENSION session before Step 5 (training data audit)
   → The gap window produced runs where the learning signal was absent or wrong.
   → Those triples must be classified before the next training run.
```

---

## Section 4 — Gap Window and Training Data Audit

A gap window is the period between when the gap was introduced and when it
was closed. Capability runs during the gap window may have produced:

```
RECOVERABLE   The DPO triple can be updated with the correct signal now that
              the gap is closed. The context package was correct; the signal
              handler was missing. Re-run the capability with the fixed handler
              and update the triple.

CONTAMINATED  The DPO triple contains wrong data that cannot be corrected by
              re-running. The context package itself was wrong (e.g., the named
              check was absent, so the rejected output contained a violation that
              was not flagged). These triples must be removed from the training
              index — they teach the model to accept violations.

CALIBRATION   The triple is valid but the confidence score is based on incorrect
              data. The triple remains but the DomainModelRegistry edge confidence
              seed must be recalculated.
```

Classification follows training-data-gap-audit-SKILL.md. The classification
result is reported at the Step 5 STOP gate.

---

## Anti-Patterns

1. **"I'll fix it inline and note it at the end"** — inline fixes without
   the proposal step bypass the Stop gate and Luba approval. The Stop gate
   exists to prevent wrong-level fixes (new interface where a config change
   sufficed, etc.).

2. **"The gap window was short — audit is probably fine"** — the training
   data audit is not optional. A gap window of 1 run that produced 3 DPO
   triples with null prompts corrupts those 3 triples regardless of window
   length.

3. **"SELF-EXTENSION doesn't need reconnaissance"** — reconnaissance-gate-SKILL.md
   applies: read the current capability state (Step 1) before proposing
   (Step 2). A proposal without evidence of the current state is an essay.

---

## Integration

- capability-state-reader-SKILL.md provides the Step 1 snapshot
- gap-to-proposal-SKILL.md classifies the gap and determines resolution level
- implementation-integrity-SKILL.md provides the Step 4 closure verification
- training-data-gap-audit-SKILL.md provides the Step 5 triple classification
- solution-scope-gate-SKILL.md governs the proposal level (Level 1–4) in Step 2
- session-mode-declaration-SKILL.md: SELF-EXTENSION is a distinct session type;
  declare it before Step 1 begins
