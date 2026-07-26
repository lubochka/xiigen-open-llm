# Debug Session

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

When a fix attempt fails during execution and the investigation must span
multiple sessions, starting the next session from scratch re-investigates
paths already eliminated. The debug session file persists the investigation
state: every hypothesis, its ELIMINATED/CONFIRMED/INVESTIGATING status,
the exact command that proved or disproved it, and the confirmed API shapes
that must not be re-verified. A session that starts by reading the debug
file instead of the codebase resumes at the exact point debugging stopped.

---

## When to Invoke

- When a fix attempt fails during execution (test fails after applying the fix)
- When a public method signature or return type differs from the plan
- When a C# compiler error cannot be resolved in 2 attempts
- When context-overflow-SKILL.md triggers emergency stop mid-investigation

---

## Section 1 — File Location and Lifecycle

**Location:** `.claude/debug/active-session.md`

**Lifecycle:**
```
1. CREATE the file when a fix attempt fails or an API shape is wrong
2. APPEND to the file after every hypothesis test
3. ARCHIVE the file when the fix is confirmed working:
   rename to .claude/debug/YYYY-MM-DD-[issue-description].md
```

**Load order:** At the start of any session where debugging was in progress,
read `.claude/debug/active-session.md` BEFORE reading any other file.

```bash
# Check if debug session is active
ls .claude/debug/active-session.md
# If exists: read it first — skip the codebase read until hypothesis is current
```

---

## Section 2 — File Format

```markdown
# Debug Session: [issue title]
**Date:** YYYY-MM-DD
**Session:** S[N]
**Phase:** Phase [N] of [plan reference]
**Issue:** [one-line description of what is failing]

## Hypotheses

### H1: [hypothesis title]
**Status:** ELIMINATED / CONFIRMED / INVESTIGATING
**Evidence for:** [what supports this]
**Evidence against:** [what disproves this]
**Elimination command:**
```bash
[command that proved or disproved this — exact command, exact output]
```
**Result:** [exact output — not summarized]

### H2: [next hypothesis]
...

## Confirmed Facts (do NOT re-investigate)
- [symbol]: [type/signature] — [exact type or return value] (line N of [file])
- [API]: is [sync/async], signature: [exact] returns OperationResult<[T]>
- [interface]: injected as [concrete type in DI registration]

## Current Best Theory
[What the investigation currently believes is happening — one paragraph]

## Next Action
[Exact next step to take when this session resumes — specific enough to execute without re-reading the history]
```

---

## Section 3 — Status Values

```
INVESTIGATING: Active hypothesis — not yet tested
CONFIRMED:     Evidence confirms this IS the cause
ELIMINATED:    Evidence proves this is NOT the cause — do not re-investigate
```

An ELIMINATED hypothesis with no supporting evidence record is incomplete.
If you cannot cite the command and output that eliminated it: it has not
been eliminated.

---

## Section 4 — Confirmed Facts Protocol

The "Confirmed Facts" section is the most important part of the file.
Every confirmed fact prevents a future session from repeating work:

```
WRONG:  Hypothesis H3 was ELIMINATED (vague — why? what proved it?)
RIGHT:  H3 ELIMINATED: IncrementalTrainer.Update() returns OperationResult<double>,
        not void. Confirmed by reading IncrementalTrainer.cs line 47.
        Grep: grep -n "public.*Update" src/22.AgentAlgorithms/IncrementalTrainer.cs → line 47
```

Add to Confirmed Facts immediately when any of these are verified:
- A method's exact return type (including the OperationResult<T> generic parameter)
- An interface's concrete implementation in DI registration
- Whether a method is sync or async
- The exact field path for accessing a nested value

---

## Section 5 — Integration with Existing Session Protocol

**At phase-preflight-SKILL.md time:**
Check for active debug session before writing any code:
```bash
ls .claude/debug/active-session.md 2>/dev/null && echo "ACTIVE DEBUG SESSION — read first"
```

**When context-overflow-SKILL.md triggers:**
Before emergency stop — write "Current Best Theory" and "Next Action"
to the active debug session file. These survive context compression.

**When investigation completes:**
Archive the file. Archived debug sessions are reference material for
systematic-debugging-SKILL.md's pattern library.

---

## Anti-Patterns

1. **"Starting investigation from scratch each session"** — the most expensive
   failure mode. ELIMINATED hypotheses are re-investigated. Confirmed API shapes
   are re-verified. Two sessions do the same work. Create the file when debugging
   starts, not after the third failed session.

2. **"ELIMINATED hypothesis with no elimination evidence"** — marking H3
   ELIMINATED without recording what proved it is useless. The next session
   cannot trust the status without the proof.

3. **"Current Best Theory left empty"** — the theory at session end is the
   most valuable recovery point. A resuming session with no theory must
   rebuild the entire investigation chain.

4. **"Archiving before fix is confirmed working"** — archive only after
   `dotnet test` passes with the fix applied and all pre-existing tests
   still pass. A premature archive closes the investigation before it is done.

5. **"Not reading the debug file before reading the codebase"** — the debug
   file changes the read order. If H7 is CONFIRMED, you read the fix site
   first, not the whole subproject. Load order: debug file first.

---

## Integration

- phase-preflight-SKILL.md Gate 0 checks for active debug session before any code write
- code-examination-SKILL.md provides the reading protocol; this skill tracks investigation state
- context-overflow-SKILL.md emergency stop must write "Next Action" to the debug file before stopping
- root-cause-tracing-SKILL.md produces hypotheses; this skill tracks their status
- systematic-debugging-SKILL.md archives provide the historical pattern library
