# Solution Scope Gate

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

A session that proposes a new infrastructure component when a convention change would
suffice has over-engineered the solution. A session that proposes a convention when
new infrastructure is genuinely required has under-specified it. This skill installs
the correct scope level before any proposal is written.

---

## When to Invoke

- Before proposing any solution to a gap or missing capability
- Before writing a new plan that adds components to the project
- When gap-to-proposal-SKILL identifies a gap that needs resolution
- Before classifying any change as "new interface required"

One run of this skill before proposal = no over-engineered solutions, no
under-specified ones.

---

## Section 1 — The 5-Level Scope Ladder

Every solution proposal occupies exactly one level. Start at Level 1 and
move up only when the lower level is genuinely insufficient.

| Level | Name | What It Produces | When It Applies |
|-------|------|-----------------|-----------------|
| **1** | CONVENTION | Entry in CLAUDE.md, AGENTS.md, or a skill file | Behavior that must be consistent but requires no code |
| **2** | ADAPTATION | Wrapper around existing .NET BCL or NuGet utility | External tool exists; it needs a project-conformant interface |
| **3** | EXTENSION | New method or capability on an existing class/interface | The class already exists; it is missing one thing |
| **4** | NEW COMPONENT | New class, new pipeline stage, new xUnit test fixture | Net-new behavior with no existing attachment point |
| **5** | NEW INTERFACE | New `I`-prefixed interface + concrete class + all downstream wiring | Net-new abstraction boundary required; Level 4 is insufficient |

**Level 5 is the last resort.** Every interface addition requires:
- A new `I`-prefixed interface file
- A concrete implementation
- Registration in the consuming service's constructor
- xUnit tests for the concrete implementation
- DNA compliance check for all touched files

---

## Section 2 — Gap Classification Table

When a gap is identified, classify it on two axes before assigning a scope level.

### Axis 1 — Gap Category

| Category | Meaning |
|----------|---------|
| **TOOL** | A utility is missing (string formatter, file helper, parser) |
| **SKILL** | A skill document is missing or incomplete |
| **ORCHESTRATION** | A coordination step between existing components is missing |
| **ARCHITECTURE** | A structural boundary or abstraction is missing |

### Axis 2 — Fix Class

| Fix Class | Meaning | Max Scope Level |
|-----------|---------|-----------------|
| **CONTENT** | Missing documentation, comment, or convention entry | Level 1 |
| **INTERFACE** | Missing `I`-prefix interface definition | Level 5 |
| **EXTENSION** | Existing class needs a new method | Level 3 |
| **NEW_HANDLER** | New class needed, but no new interface boundary | Level 4 |
| **INFRASTRUCTURE** | New external dependency or storage layer | Level 5 |

Classification record:
```
Gap: [description]
Category:  [TOOL | SKILL | ORCHESTRATION | ARCHITECTURE]
Fix class: [CONTENT | INTERFACE | EXTENSION | NEW_HANDLER | INFRASTRUCTURE]
Assigned scope level: [1-5]
Justification: [one sentence — why this level and not one lower]
```

---

## Section 3 — The 3 Reframe Tests

Before finalizing a scope level, run all 3 reframe tests.
Each test that triggers a downgrade must be recorded.

### Reframe Test 1 — Next Instance Test

> "What happens at the next instance of this same need?"

If the answer is "same convention applies" → Level 1.
If the answer is "same wrapper applies" → Level 2.
If "each instance needs its own class" → Level 4 or 5.

A solution that scales to next instances without code change belongs at Level 1 or 2.

### Reframe Test 2 — Provider vs Capability Test

> "Is this a provider (external tool, service, data source) or a capability
> (algorithm, decision, transformation)?"

Provider → Level 2 (ADAPTATION: wrap the provider behind `IConfiguration`)
Capability → Level 3, 4, or 5 depending on whether attachment point exists.

### Reframe Test 3 — Backward Compatibility Test

> "Does the existing design survive intact inside the new design?"

If yes: the new design is an extension → maximum Level 3.
If no: the new design replaces something → validate with decision-reopening-SKILL
before proceeding to Level 4 or 5.

---

## Section 4 — Scope Gate Record

Produce this record before writing any proposal:

```markdown
## Solution Scope Gate (solution-scope-gate-SKILL)

Gap statement: [one sentence — what is missing]
Category: [TOOL | SKILL | ORCHESTRATION | ARCHITECTURE]
Fix class: [CONTENT | INTERFACE | EXTENSION | NEW_HANDLER | INFRASTRUCTURE]

Reframe tests:
  Test 1 (next instance): [result]
  Test 2 (provider vs capability): [result]
  Test 3 (backward compatibility): [result]

Assigned scope level: [1 | 2 | 3 | 4 | 5]
Level name: [CONVENTION | ADAPTATION | EXTENSION | NEW COMPONENT | NEW INTERFACE]
Justification: [one sentence — why this level and not one lower]

Proposal permitted: YES
```

If any reframe test indicates a lower level is sufficient, the scope is downgraded
before the record is finalized. Do not propose at a higher level than the record permits.

---

## Anti-Patterns

1. **"Start at Level 5 to be safe"** — Level 5 creates wiring debt that outlasts
   the feature. Justify each level from Level 1 up.

2. **"Skip classification, just write the code"** — unclassified gaps produce
   solutions at the wrong scope level. Classification takes 2 minutes. Rework
   takes hours.

3. **"Reframe tests don't apply here"** — they always apply. A test that returns
   "not applicable" is a signal that the gap was not stated precisely enough.

4. **"Convention is too weak for this"** — if a convention entry in CLAUDE.md
   or AGENTS.md would solve the problem, the convention entry is the right answer.
   Code is not stronger than a rule — it is more expensive.

---

## Integration

- gap-to-proposal-SKILL.md calls this skill to assign a scope level to each gap
- prerequisite-chain-SKILL.md checks whether prerequisites exist for Level 4/5 proposals
- decision-reopening-SKILL.md is required before any Level 5 proposal that
  replaces an existing interface (Backward Compatibility Test 3 failure)
- change-propagation-SKILL.md computes blast radius for Level 4/5 proposals
