# Artifact Numbering

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (in any language), challenges an answer, or says a failure came
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

## The Core Rule

**NEVER use artifact numbers from a plan file, a session summary, or memory.**
Numbers in plans decay. Live canonical documents are the only authoritative source.

At the start of every session that assigns new numbers:
```bash
# Read live canonical docs — do not use any cached or plan-file numbers
grep -n "D-[0-9][0-9]" docs/DECISIONS-LOCKED.md | tail -5
grep -n "subproject\|NN\." docs/SUBPROJECT-MAP.md | tail -10
grep -n "BUG-[0-9][0-9]" docs/STATE-*.json | tail -5
```

---

## When to Invoke

- When assigning a new decision number (D-NN in DECISIONS-LOCKED.md)
- When assigning a new BUG number (BUG-NN in bug-to-tests-SKILL.md)
- When assigning a new subproject number (NN.SubprojectName)
- When assigning a new phase number (P-N in STATE-Pn.json)
- Before any STATE-Pn.json is saved with artifact numbers

---

## Numbering Protocol

### Step 1: Read live numbers (MANDATORY before assigning)

```bash
# From DECISIONS-LOCKED.md — get highest D-NN in use
grep -n "D-[0-9][0-9]" docs/DECISIONS-LOCKED.md | grep "^##\|^### D-" | tail -5

# From SUBPROJECT-MAP.md — get highest subproject number
grep -n "^| [0-9][0-9]\." docs/SUBPROJECT-MAP.md | tail -5

# From STATE files — get highest BUG-NN in use
grep -rn "BUG-[0-9][0-9]" docs/ | tail -5
```

Record in STATE-Pn.json before assigning new numbers:
```json
{
  "artifactNumbersRead": {
    "readAt": "session-start",
    "highestDecisionNumber": "D-12 (from DECISIONS-LOCKED.md)",
    "highestSubprojectNumber": "24 (from SUBPROJECT-MAP.md)",
    "highestBugNumber": "BUG-03 (from docs/STATE-P5.json)"
  }
}
```

### Step 2: Cross-check before claiming a number

Before assigning D-13, verify D-13 is not already in use:
```bash
grep -n "D-13" docs/DECISIONS-LOCKED.md
# Expected: 0 matches — safe to assign
```

### Step 3: Assign sequentially

- Decision numbers: next sequential integer after the highest D-NN in use
- BUG numbers: next sequential integer after the highest BUG-NN in use
- Subproject numbers: next sequential integer after the highest subproject number
- Never skip numbers — gaps create audit confusion
- Never reuse a number from a deleted or abandoned artifact

### Step 4: Record the assignment in STATE-Pn.json immediately

```json
{
  "numbersAssignedThisSession": {
    "decisions": ["D-13"],
    "bugs": [],
    "subprojects": []
  }
}
```

This prevents a second session from claiming the same number before the
first session's commit is pushed.

---

## Collision Prevention

| Collision type | Prevention |
|---|---|
| Two sessions assign same D-NN | Both check STATE-Pn.json at session start; higher session uses next available |
| Plan file says D-13 but live DECISIONS-LOCKED.md already has D-13 | Live doc wins; plan file number is stale |
| BUG-04 assigned but not yet committed | Reserve in STATE-Pn.json; do not reuse until commit confirmed |

---

## Numbers at Plan Creation (REFERENCE ONLY — DO NOT USE)

Numbers recorded in REFERENCE-PLAN or EXECUTION files at plan creation time
may be stale by the time the session runs. Always read live docs before assigning.

```
D-12 (plan creation) → READ DOCS before assigning next D-NN
BUG-03 (plan creation) → READ DOCS before assigning next BUG-NN
```

---

## Anti-Patterns

1. **"The plan says the next decision is D-13."** Plan numbers decay. Read live docs.
2. **"I'll assign the number and update DECISIONS-LOCKED.md later."** Assign only after
   reading live. Record in STATE-Pn.json immediately.
3. **"This decision number is unused — I'll reclaim it."** Never reuse a number.
   Use next sequential.
4. **"Only one session runs at a time."** Multiple sessions may run in different
   worktrees. STATE-Pn.json reservation prevents collision.
