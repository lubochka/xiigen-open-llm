# Decision Reopening Protocol

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

## Purpose

Locked decisions exist to prevent a recurring failure mode: every session
re-derives the same conclusions from first principles, wastes time, and sometimes
arrives at different answers due to context differences. But architecture must
evolve. This skill defines the narrow gate through which a decision legitimately
changes.

---

## When to Invoke

Any time a session or response challenges a locked decision:
- "I think we should reconsider D-NN"
- "This decision doesn't work for the new context"
- "The interface contract needs to change because..."
- Any suggestion that contradicts a D-series entry in DECISIONS-LOCKED.md

---

## Four-Step Test

A decision may only be reopened if it passes all four steps.

### Step 1 — Is this decision in DECISIONS-LOCKED.md?

```bash
grep -n "D-[0-9][0-9]" docs/DECISIONS-LOCKED.md | head -20
```

If NOT in DECISIONS-LOCKED.md: not a locked decision — proceed freely.
If IN the document: locked. Apply steps 2–4.

---

### Step 2 — What is the trigger for reopening?

**Valid triggers (any one is sufficient):**
```
□ New information that did not exist when the decision was made
  Example: "D-09 assumes float32 vector dimensions, but the new
   embedding model outputs float16 — the storage schema must change"

□ The decision causes a concrete failure in the current plan
  Example: "D-04 says ISessionStorage always uses local file storage,
   but the planned desktop app needs in-memory storage for testing —
   there is no way to inject an in-memory provider"

□ The decision's rationale has been invalidated by subsequent decisions
  Example: "D-06 chose BruteForceIndex as the default, but D-11 added
   HNSW — BruteForce is now the wrong default for large corpora"
```

**Invalid triggers (do not reopen):**
```
✗ "I just think there's a better way"
✗ "This would be simpler if we changed it"
✗ "The other approach is more familiar"
✗ Re-examining without new information
```

---

### Step 3 — Write the ADR entry BEFORE changing anything

Append to the Decision Change Log section of DECISIONS-LOCKED.md:

```markdown
## Decision Change Log

### D-NN Amendment — [today's date]
**Changed:** [what is changing]
**Rationale:** [the valid trigger from step 2 — exact new information or failure]
**Impact:** [which files/subprojects/interfaces need updating]
**Previous decision text:** [quote the original]
**New decision text:** [the replacement]
```

The ADR entry must exist as a written record **BEFORE** any file is edited
or any implementation is changed.

---

### Step 4 — Scope the impact and update all affected documents

After writing the ADR entry:
1. List every document that references this decision
2. Update each one — no partial updates (change-propagation-SKILL.md)
3. Run `dotnet build` and `dotnet test` after all updates

```bash
# Find all references to the decision number
grep -rn "D-NN\|decisionNN" docs/ Src/ Tests/ --include="*.md" --include="*.cs" --include="*.json"
# Update each file found
```

---

## Positive Example

```
Trigger: "D-04 says ISessionStorage always uses FileSystemStorage, but we
  need an InMemorySessionStorage for unit tests — we cannot inject it because
  the DI registration always creates FileSystemStorage"

Step 1: D-04 is in DECISIONS-LOCKED.md → locked decision

Step 2: Valid trigger — concrete failure in current plan. Unit tests cannot
  inject an in-memory implementation without changing the DI registration.

Step 3: Write ADR entry:
  "D-04 amended: ISessionStorage implementations are registered by the
   caller (not hardcoded). Program.cs registers FileSystemStorage in
   production. Test fixtures register InMemorySessionStorage. Both implement
   ISessionStorage. DI contract unchanged; binding is environment-specific."

Step 4: Update DECISIONS-LOCKED.md D-04. Update docs/COMPONENT-INTERFACES.md
  to note InMemorySessionStorage as a test implementation. Update Program.cs
  DI binding comment.
```

---

## Negative Example

```
Trigger: "I think we should consider using a different vector similarity metric"

WRONG:
  → Re-debating D-09 without new information
  → No concrete failure cited
  → No ADR entry written
  → Proposing a change that contradicts the locked decision in a comment
```

---

## Integration

```
Prerequisite:   claim-verification-SKILL.md — verify D-NN exists before claiming it's locked
After reopening: blast-radius-SKILL.md — assess impact scope before updating files
After updating:  change-propagation-SKILL.md — update all derived views in same commit
                 documentation-sync-SKILL.md — sync COMPONENT-INTERFACES + SUBPROJECT-MAP
```
