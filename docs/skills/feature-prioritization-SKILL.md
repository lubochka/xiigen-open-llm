# Feature Prioritization

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

When multiple subprojects or capabilities are buildable at the same time, choosing
the wrong one to build first delays user-visible value and wastes infrastructure
investment. This skill defines a 3-axis score that makes the ordering decision
explicit and auditable, and records any override so priority decisions are not
invisible.

---

## When to Invoke

- When a plan has 2+ candidate subprojects/capabilities to sequence
- When Luba asks "what should we build next?"
- When a new subproject is proposed and dependencies are not yet mapped
- When a session-start reconnaissance finds multiple incomplete phases

One score card per session = zero "we built the wrong thing first" retrospectives.

---

## Section 1 — The 3-Axis Score

### Axis 1 — Business Value Unlock (1–5)

**What it measures:** does completing this subproject let users DO something new?

| Score | Meaning |
|---|---|
| 5 | Users can take a new action that was impossible before |
| 4 | Substantially improves a current action (speed, quality, reach) |
| 3 | Makes the system more complete, but no new user action is unlocked |
| 2 | Operational improvement (monitoring, diagnostics) |
| 1 | Backend-only: infrastructure, pipeline plumbing, no user-visible change |

### Axis 2 — Dependency Multiplier (1–3)

**What it measures:** how many other subprojects are blocked until this one ships?

| Score | Meaning |
|---|---|
| 3 | 3 or more subprojects cannot start until this ships |
| 2 | 1–2 subprojects are blocked |
| 1 | Nothing else is blocked by this |

### Axis 3 — Infrastructure Cost (1–3, INVERTED — lower cost = higher score)

**What it measures:** how much new fabric must be built before this capability can be implemented?

| Score | Meaning |
|---|---|
| 3 | All required interfaces already exist (IAgentMessageBus, ISessionStorage, IVectorIndex, IBm25Scorer, ISemanticCompiler29, etc.) |
| 2 | One new interface required |
| 1 | Two or more new fabric interfaces required |

**INVERTED because:** high infrastructure cost means the capability cannot be built
until that infrastructure exists — building the infrastructure first is the only
correct sequence, and scoring it as low priority captures that friction correctly.

### Priority Score Formula

```
Priority Score = Axis1 × Axis2 × Axis3
```

Maximum possible: 5 × 3 × 3 = 45
Minimum possible: 1 × 1 × 1 = 1

---

## Section 2 — Scoring Example

| Capability | Axis 1 | Axis 2 | Axis 3 | Score | Rationale |
|---|---|---|---|---|---|
| Search pipeline (IVectorIndex + IBm25Scorer) | 5 | 3 | 3 | 45 | Users can search; 3 downstream caps depend on it; all interfaces exist |
| Feedback capture endpoint | 4 | 2 | 3 | 24 | Improves search; 2 caps blocked; interfaces exist |
| DomainModelRegistry persistence | 3 | 1 | 2 | 6 | More complete; nothing blocked; 1 new interface needed |
| ContinuousLearningPipeline Phase 2 | 3 | 1 | 1 | 3 | Backend-only improvement; nothing blocked; 2+ new fabric pieces needed |

**Build order:** Search pipeline → Feedback capture → DomainModelRegistry → ContinuousLearningPipeline Phase 2

---

## Section 3 — Score Card Format

Before executing a plan with multiple sequenced capabilities, produce a score card:

```markdown
## Feature Priority Score Card — [Session Date]

| Capability | Axis1 (value) | Axis2 (deps) | Axis3 (cost⁻¹) | Score | Verdict |
|---|---|---|---|---|---|
| [name] | [1-5] | [1-3] | [1-3] | [A1×A2×A3] | BUILD_NEXT / QUEUE / BLOCKED |

Selected build order: [ranked list]
Override: [NONE / Decision D-XX — reason]
```

**Verdict definitions:**
- **BUILD_NEXT** — highest score in current set; no blocking prerequisites
- **QUEUE** — lower score; prerequisites met; sequence after BUILD_NEXT
- **BLOCKED** — hard dependency on an unshipped capability; score is irrelevant until unblocked

---

## Section 4 — Override Rule

A priority score can be overridden when:
- Luba explicitly names a different capability as the priority
- A stakeholder commitment requires a specific delivery order

**Override is not free.** Every override must be recorded as a locked decision in
`docs/DECISIONS-LOCKED.md`:

```markdown
D-XX — Priority Override: [capability] promoted over [score-winner]
Reason: [stakeholder reason / commitment]
Score at override time: [score-winner]=N, [promoted]=M
Recorded: [date]
```

**What override cannot do:** override a hard dependency lock. If Capability B requires
Capability A's output, B cannot be built first regardless of score or stakeholder priority.
The dependency graph is invariant. Only the ordering of non-blocked capabilities is flexible.

---

## Section 5 — Dependency Lock Rule

Before scoring, classify each candidate:

```markdown
Dependency check: [CapabilityName]
  Requires: [list of capabilities/interfaces that must exist first]
  Status of each: [SHIPPED / IN_PROGRESS / MISSING]
  Can start now: YES / NO — blocked by [name]
```

Capabilities with any MISSING prerequisite are marked BLOCKED and removed from
the priority score calculation. They are re-evaluated after their prerequisites ship.

---

## Anti-Patterns

1. **"We built what was easiest"** — low infrastructure cost (Axis 3 = 3) alone does not
   justify building first if there is no user value (Axis 1 = 1). Score all three axes.

2. **"We built what felt most important"** — feeling is not a score. Produce the card
   before building, not after.

3. **"Override is fine, we don't need to record it"** — an unrecorded override is an
   invisible architectural decision. It will cause confusion in future sessions.
   Record every override in docs/DECISIONS-LOCKED.md.

4. **"Dependency locks are guidelines"** — they are not. If Capability B reads output
   from Capability A's ISessionStorage key, B cannot run before A writes it. This is
   a code constraint, not a preference.

---

## Integration

- prerequisite-chain-SKILL.md classifies each dependency as PASS/FAIL/BLOCKING
- solution-scope-gate-SKILL.md classifies each capability's fix class before scoring
- plan-self-validation-SKILL.md Check 1 verifies that the selected build order delivers the stated goals
- docs/DECISIONS-LOCKED.md receives every priority override as a locked decision
