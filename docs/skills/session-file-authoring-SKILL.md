# Session File Authoring

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

A session file that references another session file, assumes prior context,
or contains implicit state is not a session file — it is a fragment. A fragment
cannot be handed off to a cold session. This skill defines the crystallization
protocol (what makes a session file self-contained), the 8 self-containment
checks (with verification commands), and Gate D (the goal advancement block
that every session file must contain).

---

## When to Invoke

- Before writing any SESSION-EXECUTION, SESSION-BRIEF, or STATE file
- When agent-output-format-SKILL.md SESSION-0 gate requires a new execution file
- When handing off to another session (session-handoff-SKILL.md)
- When a cold-session reader says "I can't understand this without the prior context"

---

## Section 1 — Crystallization Protocol

A session file is "crystallized" when a reader who has never seen this project
before can understand: what the session was trying to accomplish, what state
the project was in when it started, what the session produced, and what the
next session must do.

**Four crystallization requirements:**

1. **No cross-references to prior session files**
   "See SESSION-P4.md for context" is not crystallized. The context must
   be present in this file, not pointed to.

2. **No implicit state**
   "Continuing from last time" is not crystallized. State the relevant prior
   state explicitly in the file header.

3. **No assumed knowledge**
   Abbreviations must be expanded on first use (Rule 9). Capability class names
   must be defined. Interface names must be explained.

4. **No dangling references**
   Every file mentioned by name must either be readable from the project
   directory or explained inline. "See DECISIONS-LOCKED.md" is acceptable
   because that file always exists. "See my earlier analysis" is not.

---

## Section 2 — Eight Self-Containment Checks

Run these checks on every session file before committing or handing off.

```bash
# Check 1: No cross-references to prior session files
grep -n "SESSION-P[0-9]" docs/session-files/SESSION-EXECUTION-Pn.md
# Expected: 0 hits (if any hits: replace with inline context)

# Check 2: No "continuing from" or "as before" phrases
grep -in "continuing from\|as before\|as discussed\|from last time" \
    docs/session-files/SESSION-EXECUTION-Pn.md
# Expected: 0 hits

# Check 3: All abbreviations expanded on first use
# Manual: scan for any uppercase abbreviation not followed by "(full form)"
# Example: "DPO (Direct Preference Optimization)" ✅
# Example: "DPO triple" without prior expansion ❌

# Check 4: File references exist
grep -oP '[\w/\-]+\.(?:md|cs|json)' docs/session-files/SESSION-EXECUTION-Pn.md \
    | while read f; do [ -f "$f" ] || echo "MISSING: $f"; done
# Expected: 0 MISSING lines

# Check 5: Goal advancement block present
grep -n "Goal advancement" docs/session-files/SESSION-EXECUTION-Pn.md
# Expected: 1 hit (Gate D — see Section 3)

# Check 6: Verbatim goal quote present
grep -n "verbatim\|\".*goal.*\"" docs/session-files/SESSION-EXECUTION-Pn.md
# Expected: 1+ hits (at least one quoted goal string)

# Check 7: Test baseline recorded
grep -n "testBaseline\|passed.*0 failed" docs/session-files/SESSION-EXECUTION-Pn.md
# Expected: 1+ hits (pre-session and post-session baselines)

# Check 8: Next session requirements stated
grep -n "Next session\|prerequisite\|blocked by" docs/session-files/SESSION-EXECUTION-Pn.md
# Expected: 1+ hits
```

All 8 checks must pass before the file is handed off. A check that cannot
pass means the file is not crystallized. Fix the file, not the check.

---

## Section 3 — Gate D: Goal Advancement Block

Every session file must contain a Gate D block. Gate D is a binary test:
did this session advance the stated goal?

```markdown
## Goal Advancement

**Stated goal (verbatim):** "[exact quote from session start — never paraphrased]"

**Advancement:** [one sentence with a state-change verb]
Examples of valid advancement sentences:
  - "Phase 5 complete: IncrementalTrainer now produces cumulative ObservationCount."
  - "Gate B passed: 12 new tests added, all green, baseline preserved."
  - "BLOCKED: missing locked decision for ObservationCount semantics; session halted."

**Next session must:** [one sentence stating the concrete starting condition]
```

**State-change verb requirement:** The advancement sentence must contain a verb
that names a state change: "complete," "added," "fixed," "blocked," "produced,"
"removed," "passed." Verbs like "worked on," "investigated," "reviewed" are
not state-change verbs — they describe activity, not outcomes.

**Gate D FAIL conditions:**
- Advancement sentence has no state-change verb → rewrite
- Goal quote is paraphrased, not verbatim → go back to source, quote exactly
- "Next session must" is vague ("continue the work") → name the specific file,
  method, or decision the next session must start from

---

## Section 4 — Session File Header Template

Every session execution file begins with this header:

```markdown
# SESSION-EXECUTION-Pn — [Phase Name]

## Session State (pre-session)
- **Branch:** [git branch name]
- **Test baseline:** [N passed, 0 failed — from dotnet test at session start]
- **Last commit:** [hash + message]
- **Subproject in scope:** [subproject number + name]

## Goal Advancement
**Stated goal (verbatim):** "[from session start or planning document]"
**Advancement:** [state-change sentence — written at session END]
**Next session must:** [specific starting condition — written at session END]

## Session Scope
This session implements: [specific phases or capabilities]
This session does NOT cover: [explicit list of out-of-scope items]
```

The "does NOT cover" section prevents scope creep and prevents the next session
from assuming this session completed something it didn't.

---

## Section 5 — Anti-Patterns

1. **"Session file says 'see SESSION-P4 for background'"** — the background
   must be in this file. A cold reader does not have SESSION-P4. Inline it.

2. **"Goal advancement sentence: 'Made progress on the trainer'"** — "made
   progress" has no state change. What specifically changed? What passes now
   that didn't before?

3. **"Test baseline not recorded"** — without the pre-session baseline, the
   post-session baseline is meaningless. Record both.

4. **"Abbreviations used without expansion"** — Rule 9 applies to session files
   as much as to code comments. The next session reader may not know what
   "CLP" means. Write "ContinuousLearningPipeline (CLP)" on first use.

5. **"'Next session must: continue implementation'"** — this is not actionable.
   "Next session must: implement DomainModelRegistry.UpdateConfidence() starting
   from COMPONENT-INTERFACES.md §3.2" is actionable.

6. **"Gate D written before the session ends"** — the advancement sentence must
   describe what actually happened, not what was planned. Write it last.

---

## Integration

- agent-output-format-SKILL.md defines the three-file rule (REFERENCE/STATE/EXECUTION)
- session-handoff-SKILL.md uses this skill's Gate D block as the handoff summary
- session-output-contract-SKILL.md Section 3 (read-before-write rule) applies to all UPDATE deliverables
- goal-context-persistence-SKILL.md governs the verbatim goal capture that feeds Gate D
- verification-before-completion-SKILL.md runs before any session is declared done; Gate D is its output
