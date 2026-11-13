# Root Cause Ladder

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

Every gap has three possible explanations that sit at different levels:
the immediate fix, the structural practice that let it recur, and the
architectural decision that made it possible in the first place. A session
that only applies the immediate fix will see the same gap re-emerge. A session
that addresses the wrong level will over-engineer a small symptom or
under-solve a deep pattern. This skill defines the three-level structure,
the convergence protocol (multiple gaps sharing a root), and the rejection
response protocol.

---

## When to Invoke

- After Gate B fails — before deciding what to fix
- When the same category of error appears in more than one phase
- When a remediation was applied in a prior session but the problem recurred
- When diagnostic-escalation-SKILL.md produces an architectural escalation signal

---

## Section 1 — The Three-Level Structure

Every gap must be classified at all three levels before any fix is written.

### Level 1 — Immediate Fix

The specific code or configuration change that makes this instance stop failing.

```
Format:
  What failed:  [exact method or assertion that failed]
  Why it failed: [proximate cause — the concrete missing or wrong thing]
  Fix:          [specific change: file + line + what to change]
  Test:         [the test that will be green after this fix and not before]
```

Level 1 is necessary but not sufficient. A Level-1-only response means the
same gap will recur in the next phase.

### Level 2 — Structural Fix

The skill, rule, or practice that, if present, would have caught Level 1 before
it reached Gate B.

```
Format:
  Missing practice: [what should have been checked but wasn't]
  Skill or rule:    [which skill addresses this — or "NEW SKILL NEEDED"]
  Action:           [update skill / add iron rule / add pre-phase check]
```

Level 2 examples:
- "api-shape-verification-SKILL.md was not loaded before test was written"
- "No iron rule for 'WeightedScorer must not produce negative weights'"
- "phase-planning-SKILL.md Gate 3 (fabric check) was not applied"

### Level 3 — Architectural Decision

The design decision or document that, if correct, would have prevented the
structural gap at Level 2 from occurring.

```
Format:
  Root cause:   [the missing or incorrect design artifact or decision]
  Document:     [which document should capture this — DECISIONS-LOCKED.md, COMPONENT-INTERFACES.md, etc.]
  Action:       [write D-NN entry / update interface spec / new architecture pattern]
```

Level 3 examples:
- "No locked decision defining when ISessionStorage is required vs optional"
- "COMPONENT-INTERFACES.md does not specify the return type contract for HybridRetriever"
- "DECISIONS-LOCKED.md has no entry for ObservationCount semantics (cumulative vs delta)"

---

## Section 2 — Root Cause Record Format

After classifying a gap at all three levels, produce one record:

```markdown
### Root Cause Record — [Gap Name]

**Level 1 — Immediate Fix**
- Failed:   [method/assertion]
- Why:      [proximate cause]
- Fix:      [file:line change]
- Test:     [test name that verifies fix]

**Level 2 — Structural Fix**
- Gap:      [missing practice or skill]
- Action:   [skill update / new iron rule / pre-phase check]

**Level 3 — Architectural Decision**
- Root:     [missing or incorrect design artifact]
- Document: [where to record it]
- Action:   [new D-NN entry or interface spec update]
```

Do not proceed to Level 1 fix work until the record is complete.
A record missing Level 3 means the architectural root was not identified.

---

## Section 3 — Cross-Gap Convergence Protocol

When multiple gaps appear in the same session, check for shared Level 3 roots
before fixing each individually.

```
Step 1 — List all gaps (Level 3 only):
  Gap 1: [Level 3 root cause]
  Gap 2: [Level 3 root cause]
  Gap 3: [Level 3 root cause]

Step 2 — Group by shared root:
  If Gap 1 and Gap 3 share the same Level 3 root:
    → One Level 3 fix closes both
    → Priority: Level 3 fix first, then Level 1 fixes for both

Step 3 — Report shared root count:
  "3 gaps found. 2 share Level 3 root: missing locked decision for
   ObservationCount semantics. Fixing D-NN entry closes gaps 1 and 3.
   Gap 2 has independent Level 3 root."
```

A shared Level 3 root is always fixed before the individual Level 1 fixes.
Fixing Level 1 without the shared Level 3 fix means the next session will
produce the same pattern.

---

## Section 4 — Rejection Response Protocol

When a proposed fix is challenged by another reviewer or by Luba:

```
The challenged fix is at Level [1/2/3].
The challenge claims the fix is at the wrong level.

Response protocol:
  Step 1: State which level the challenger is operating at.
  Step 2: Agree or disagree at each level explicitly.
  Step 3: If the challenge is correct — restate the fix at the right level.
  Step 4: If the challenge is incorrect — show the Level 3 root cause record
          and explain why it confirms the current level.

NEVER: abandon a valid Level 2 or Level 3 fix because it "feels over-engineered"
       for the immediate symptom. The symptom is Level 1. The structural and
       architectural fixes operate at different scopes.
```

Example rejection response:

```
Challenge: "This is a one-line fix — adding a D-NN entry is overkill."

Response:
  Level 1 fix (one line): correct — I will apply it.
  Level 3 fix (D-NN entry): also required.

  Reason: this is the second session where ObservationCount was ambiguous.
  The missing D-NN entry is the Level 3 root. Without it, a third session
  will ask the same question. The D-NN entry costs 3 lines. Not applying it
  costs a future session.
```

---

## Section 5 — Escalation to DESIGN_FLAW

When Level 3 analysis produces a root that contradicts an existing locked
decision (D-NN), escalate via decision-reopening-SKILL.md rather than
writing a competing Level 3 fix.

```
Signal: DESIGN_FLAW (from escalation-orchestrator-SKILL.md Rule 6)
Trigger: Level 3 root contradicts D-01..D-12

Action:
  1. Apply Level 1 fix only (immediate — stops the bleed)
  2. Do NOT apply Level 3 fix
  3. Emit: "DESIGN_FLAW: Level 3 root [X] contradicts D-NN [Y].
           Routing to decision-reopening-SKILL.md."
  4. Decision-reopening-SKILL.md runs the 4-step test before any change
```

---

## Anti-Patterns

1. **"Level 1 fix applied — issue resolved"** — Level 1 stops the bleeding.
   It does not prevent recurrence. A Level 2 or 3 root that is not fixed
   will produce the same symptom in the next phase.

2. **"Level 3 analysis skipped — too slow"** — Level 3 is 3–5 lines.
   The cost of a missing D-NN entry that causes a future re-investigation is
   one full session. The math is simple.

3. **"Rejected the Level 3 fix to avoid scope creep"** — scope creep is adding
   features. Writing a D-NN entry is not a feature — it is documentation of
   a decision that has already been made.

4. **"Each gap fixed independently without convergence check"** — 3 gaps with
   a shared Level 3 root require 3 Level 1 fixes + 1 Level 3 fix. Without the
   convergence check: 3 Level 1 fixes applied, Level 3 root unclosed, same
   3 gaps in the next session.

---

## Integration

- diagnostic-escalation-SKILL.md produces architectural escalation signals → root-cause-ladder Level 3
- decision-reopening-SKILL.md handles Level 3 fixes that contradict existing locked decisions
- escalation-orchestrator-SKILL.md Rule 6 (UNDECIDED = DESIGN_FLAW) triggers Level 3 escalation
- planning-session-startup-SKILL.md Q5 (blockers) is where Level 3 roots from prior sessions surface
- phase-retrospective-SKILL.md Dimension 2 (pattern transfer) tracks whether Level 2 fixes were adopted
