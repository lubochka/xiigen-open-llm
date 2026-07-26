# Session Mode Declaration

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

## Core Problem

A session that drifts from one mode to another mid-stream produces contaminated
output. The contamination is invisible when it happens and costly to unwind later.
This skill makes mode an explicit, declared property with enforceable scope boundaries.

---

## When to Invoke

- At session start, before first synthesis
- When a session's output visibly leaves its declared mode's scope
- When Luba corrects "wrong level of output" — that is a mode drift signal

One explicit declaration at session start = zero slow-drift correction cycles.

---

## Section 1 — The 5 Session Modes

Every session declares exactly one of these modes.

| Mode | Produces | Scope-in | Scope-out |
|------|----------|----------|-----------|
| **ARCHITECT** | decisions, trade-off analyses, interface contracts | goals, interfaces, design invariants, acceptance criteria | file paths, method signatures, test code, implementation diffs |
| **PLANNER** | phase sequences, dependency graphs, EXECUTION files | phases, gates, Gate B conditions, dependencies, scope declarations | new design work, code implementation |
| **REVIEWER** | verdicts, gap reports, plan approval/rejection | plan evaluation, gate checks, DNA compliance, evidence verification | plan authoring, code changes, new design proposals |
| **EXECUTOR** | file changes, test runs, build outputs | file edits, line-precise changes, test runs, build checks | new design decisions, scope changes, architectural trade-offs |
| **INVESTIGATION** | discovery findings, artifact boundaries, capability maps | reading files, grep commands, test runs, verified facts | implementing changes, writing new code |

**The scope boundaries are enforceable gates, not preferences.**

If ARCHITECT mode output contains method signatures or file:line references, the session
has drifted to EXECUTOR. If EXECUTOR mode proposes a new design pattern, that is ARCHITECT
scope. Drift is detected, not tolerated.

---

## Section 2 — Declaration Protocol

At session start:

1. **Declare exactly one mode** — not two, not "mostly X but also Y"
2. **Write 1-2 sentence justification** — why this mode fits the question;
   why the adjacent mode is wrong
3. **Log immutably** — mode cannot silently change mid-session
4. **No output before declaration** — declaration is a prerequisite

Declaration template:
```markdown
## Session Mode (session-mode-declaration-SKILL)

Declared mode: [ARCHITECT | PLANNER | REVIEWER | EXECUTOR | INVESTIGATION]

Justification:
[1-2 sentences. What makes this mode correct for Luba's question?
Why is the adjacent mode wrong?]

Scope-out for this session:
[explicit list of what this session will NOT produce]
```

If Luba's question doesn't fit any single mode cleanly — stop and ask. Do not guess.
Multi-mode sessions are exactly how drift happens.

---

## Section 3 — Drift Detection Signals

Self-check before every STOP. Each mode has specific signals:

### ARCHITECT drifting to EXECUTOR
- Output contains file:line references or method signatures
- Output proposes a specific code change or diff
- Output prescribes an execution order ("first do X, then Y, then Z")

### ARCHITECT drifting to PLANNER
- Output produces a phase sequence or session file
- Output mentions specific Gate B conditions or test counts

### PLANNER drifting to EXECUTOR
- Output contains diffs or file contents
- Output runs or proposes running specific commands

### EXECUTOR drifting to ARCHITECT
- Output proposes a design pattern instead of implementing one
- Output debates the approach instead of executing it

### INVESTIGATION drifting to EXECUTOR
- Output starts modifying files before completing the discovery
- Output writes new code instead of reading existing code

---

## Section 4 — Resolution on Drift

**Detected drift is NOT corrected by continuing in the correct mode.**

Slow-drift correction after 3000 tokens of drifted output doesn't produce a
clean result — it produces a hybrid with the wrong frame baked in.

Resolution steps:
1. Session STOPS immediately when drift is detected, even mid-response
2. Log the drift: what scope-out signal was detected, what output triggered it
3. Luba decides: restart in correct mode (fastest), or change the declaration
   with new justification (if initial declaration was wrong)

A fresh session with explicit mode declaration is faster than correcting mid-course.

---

## Section 5 — Session Mode Record

Record in STATE-Pn.json or session notes:

```json
{
  "sessionMode": {
    "declared": "EXECUTOR",
    "justification": "Luba asked to implement IBm25Scorer interface and BM25Scorer class. The question is about code implementation, not design trade-offs.",
    "scopeOut": ["new design decisions", "architectural trade-offs", "scope changes"],
    "driftEvents": []
  }
}
```

---

## Anti-Patterns

1. **"I'll pick the mode as I go"** — mode declaration is before first synthesis,
   not inferred retrospectively.

2. **"It's multi-mode"** — no session is multi-mode. If the question requires
   two modes, handle one and defer the other to a separate session.

3. **"Implicit mode"** — implicit modes drift. Declare explicitly.

4. **"Mode changed because the question evolved"** — if the question genuinely
   evolved, stop the current session and restart with the new mode.

5. **"Let me correct the drift in the next response"** — corrections leave
   drifted context intact. Restart is faster than correction.
