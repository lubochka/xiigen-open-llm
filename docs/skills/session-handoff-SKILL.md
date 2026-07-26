# Session Handoff

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

## Purpose

Development in this project has two distinct actors:
- **Claude Code** — executes phases, writes code, runs tests
- **Planning session (new conversation)** — plans next phases, reviews results,
  makes decisions

These two actors share no context. Without a structured handoff, every new
conversation must re-explain the entire project state from scratch.

A session handoff document is that structured bridge. Paste it at the start
of any new planning conversation and the session immediately knows: where things
are, what just happened, and what needs deciding next.

---

## When to Write

After every phase gate passes, as part of the phase-completion-packager-SKILL.md
procedure. Write AFTER the gate passes — never speculatively.

---

## File Location and Name

```
docs/SESSION-BRIEF-P[N].md

Examples:
  docs/SESSION-BRIEF-P5.md
  docs/SESSION-BRIEF-P6.md
```

---

## Required Sections

```markdown
# SESSION BRIEF — Phase [N] → Phase [N+1]
## For: new planning session | Produced by: Claude Code
## Phase completed: [phase_title] | Next: [next_phase_title]
## Date: [completed_at]

---

## PROJECT STATE

Branch:       main
Phase done:   P[N] — [phase_title]
Phase next:   P[N+1] — [next_phase_title]
Tests:        [exit] passing (was [entry] at phase start, +[delta] this phase)
Build:        0 Error(s)
Last commit:  [hash] — phase [N]: [description]

Subproject status (from SUBPROJECT-MAP.md):
  [list active subprojects with current state]

---

## WHAT PHASE [N] BUILT

[3–5 sentences. Plain English. What was implemented and why it matters.
Focus on capability value, not file names or test counts.]

---

## DISCOVERIES — FACTS ESTABLISHED THIS SESSION

Each discovery: fact + verification command + implication + action taken/deferred.

  DISCOVERY-1:
    fact:        [precise factual statement]
    verified_by: grep -n "..." or dotnet test result
    implication: [what this means for future work]
    action:      RESOLVED: [what was done] | DEFERRED: [why and when]

---

## REJECTED CLAIMS

Claims that were wrong during this session. Prevents the same wrong claim
from being accepted in a future session.

  REJECTED-1:
    claim:       [exact wrong statement]
    source:      [who/what made it]
    truth:       [what is actually true]
    verified_by: [command or observation that disproved it]

---

## OPEN ITEMS — NEED YOUR DECISION

[Include only if items genuinely need Luba's input before next phase can start.
Omit this section entirely if nothing needs deciding.]

DECISION 1: [title]
  Context: [one sentence]
  Options:
    A) [option with consequence]
    B) [option with consequence]
  Recommendation: [A or B, and why]

---

## WHAT PHASE [N+1] WILL BUILD

[2–3 sentences: what the next phase covers, what it produces, what the gate looks like.]

Gate: dotnet test → [expected test count] passed, 0 failed

---

## HOW TO USE THIS BRIEF

Paste this file at the start of your planning conversation.
The session will pick up from here without needing full repo context.

To plan Phase [N+1]: say "Plan Phase [N+1] using this brief."
To approve: say "yes" or "proceed to Phase [N+1]."
```

---

## Writing Rules

1. PROJECT STATE section is always present — always current
2. "WHAT PHASE N BUILT" is prose — no file lists, no bullet points; explain the meaning
3. OPEN ITEMS omitted if nothing needs deciding — do not write "(none)"
4. The brief must be self-contained — no references to files the reader would need to open
5. Test count must be exact — the number from `dotnet test` output
6. If no discoveries and no rejected claims in the session — omit those sections

---

## Negative Examples

```
WRONG: Brief that references files
  "See docs/EXECUTION-LOG-P6.json for details."
  → Brief must be self-contained. Reader may not have repo access.

WRONG: "What Phase Built" as a file list
  "## What Phase 6 Built
   - Created IncrementalTrainer.cs
   - Added 35 tests"
  → This is a manifest, not a brief. Write what it MEANS for the project.

WRONG: Including OPEN ITEMS when nothing needs deciding
  "## Open Items
   (none)"
  → Omit the section entirely.
```

---

## Integration

```
Written by:      phase-completion-packager-SKILL.md (as the handoff artifact)
Read by:         Next planning conversation (paste at start)
Feeds from:      session-execution-log-SKILL.md (test counts and file lists)
Complements:     session-state-crystallization-SKILL.md (≤40-line snapshot for
                 within-session continuity vs. this skill's cross-session handoff)
```
