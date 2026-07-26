# Remediation Session Design

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

Raw gaps do not map to sessions. A list of 12 gaps may collapse to 3 root causes
(via root-cause-ladder-SKILL.md), and those 3 root causes may group into 2 sessions
with a clear dependency between them. Designing sessions from raw gaps produces
12 sessions instead of 2. This skill takes root-cause-ladder output as input and
produces an ordered session plan: groups of fixes, each with a session type,
blast radius classification, dependency chain, and pre-flight assumptions.

---

## When to Invoke

- After root-cause-ladder-SKILL.md produces K root causes from N gaps
- Before scheduling remediation work (to avoid scheduling 12 sessions when 2 suffice)
- When a prior remediation session failed to close the gap (re-plan from root causes)
- As the planning output when a gate review produces multiple BLOCK verdicts

---

## Prerequisite

Run root-cause-ladder-SKILL.md first. This skill starts from its output:
K root causes, each with a dependency order.

```
Expected input format from root-cause-ladder-SKILL.md:
  Root cause 1: [description] — covers gaps [N, M, P]
  Root cause 2: [description] — covers gaps [Q, R]
  Dependency: Root cause 1 must be fixed before root cause 2
```

If you have raw gaps but have not run root-cause-ladder-SKILL.md:
run it first. Do not group raw gaps into sessions — collapse to root causes first.

---

## Section 1 — Fix Class Taxonomy

Apply this taxonomy to each root cause before assigning a session type.

| Fix class | Definition |
|---|---|
| CONTENT | Change to existing logic — no new interfaces, no new registrations |
| EXTENSION | Add to existing structure — new method on existing class, new edge in registry |
| INFRASTRUCTURE | New interface or new service registration required |
| NEW_CAPABILITY | New class + interface + tests (full capability) |

**Blast radius by fix class:**

```
CONTENT:        ≤ 3 files, ≤ 50 lines → LOW blast radius → may batch with other CONTENT fixes
CONTENT:        ≤ 3 files, 50–150 lines → MEDIUM → own session if functionally unrelated
EXTENSION:      ≤ 5 files, ≤ 150 lines, no new interfaces → MEDIUM → may batch if blast radius contained
EXTENSION:      > 5 files OR blast radius HIGH → own session
INFRASTRUCTURE: always own session (new interface = blast radius always HIGH)
NEW_CAPABILITY: always own session + plan-review-SKILL.md Gate before execution
```

**Hard rule:** > 10 files changed in one session → always own isolated session.
A 10-file grep-and-replace in a maintenance session is a test-failure risk.

---

## Section 2 — Session Type Assignment

| Fix class | Session type | Rule |
|---|---|---|
| CONTENT (LOW) | MAINTENANCE (batched) | ≤ 3 files, ≤ 50 lines → combine CONTENT fixes |
| CONTENT (MEDIUM) | MAINTENANCE (isolated) | functionally unrelated → own session |
| EXTENSION (MEDIUM) | MAINTENANCE (isolated) | contained blast radius → may batch |
| EXTENSION (HIGH) | MAINTENANCE (isolated) | > 5 files → own session |
| INFRASTRUCTURE | MAINTENANCE + gate review | plan-review-SKILL.md FC check required |
| NEW_CAPABILITY | EXECUTOR session | own session + Gate B declaration |

---

## Section 3 — Session Group Format

After classifying each root cause, produce an ordered session plan:

```markdown
## Remediation Session Plan

### Group A — [session type]: [one-line description]
Fix class:    [CONTENT / EXTENSION / INFRASTRUCTURE / NEW_CAPABILITY]
Files:        [list of files to change]
Lines:        ~[N] estimated
Blast radius: LOW / MEDIUM / HIGH
Depends on:   [nothing / Group X]
Blocks:       [Group Y / nothing]

Fixes:
  A1: [root cause 1 description] — [file:what to change]
  A2: [root cause 2 description] — [file:what to change]

Pre-flight assumptions (assumption-registry-SKILL.md):
  INFRA-1: [assumption] | verify: [bash command] | expected: [result]
  BEHAV-1: [assumption] | verify: [bash command] | expected: [result]

Gate B:
  [exact passing condition as assertion, count, or comparison]

### Group B — [session type]: [description]
Depends on:   Group A complete
Blocks:       Group C
...
```

---

## Section 4 — Dependency Ordering Rules

Order groups by:

1. **Dependency chain from root-cause-ladder-SKILL.md** — Level 3 fixes (architectural
   decisions) before Level 2 fixes (structural/skill updates) before Level 1 fixes
   (immediate code changes)
2. **Within same dependency level:** INFRASTRUCTURE before EXTENSION before CONTENT
3. **Infrastructure fixes before capability fixes** — capabilities inject what
   infrastructure registers; registration must exist before injection

```
Example ordering:
  Group A: INFRASTRUCTURE — adds INewInterface registration (Level 3, must be first)
  Group B: EXTENSION — wires INewInterface into existing service (Level 2, depends on A)
  Group C: CONTENT — fixes logic in wired service (Level 1, depends on B)
```

---

## Section 5 — Worked Example

**Input from root-cause-ladder-SKILL.md (3 root causes):**

```
Root cause 1: IncrementalTrainer.ObservationCount returns delta not cumulative
  → covers: gaps 1, 3 (DomainModelRegistry decay wrong, confidence under-protected)
  Level: CONTENT fix
Root cause 2: ISessionStorage not injected in WeightedScoringService
  → covers: gap 2 (NullReferenceException in scoring)
  Level: EXTENSION fix (adds ctor parameter + BaseService inheritance)
Root cause 3: Missing D-NN entry for ObservationCount semantics
  → covers: gap 4 (same question re-asked in every session)
  Level: ARCHITECTURAL (write to DECISIONS-LOCKED.md)
Dependency: Root cause 3 must be recorded before 1 and 2 are fixed
  (the fix encodes a decision; the decision must be locked first)
```

**Output:**

```markdown
### Group A — MAINTENANCE: Record D-NN decision for ObservationCount semantics
Fix class:    CONTENT (documentation)
Files:        docs/DECISIONS-LOCKED.md
Lines:        ~10
Blast radius: LOW
Depends on:   nothing
Blocks:       Group B, Group C

Fixes:
  A1: Write D-13 entry — ObservationCount in UpdatedModelWeights is CUMULATIVE
      (total since capability first registered, not delta for this run)

Pre-flight:
  INFRA-1: docs/DECISIONS-LOCKED.md last entry is D-12 | grep "^## D-12" docs/DECISIONS-LOCKED.md | expected: 1 hit

Gate B: grep -n "D-13" docs/DECISIONS-LOCKED.md returns 1 hit

### Group B — MAINTENANCE: Fix ObservationCount to return cumulative
Fix class:    CONTENT
Files:        src/22.AgentAlgorithms/IncrementalTrainer.cs
Lines:        ~15
Blast radius: LOW
Depends on:   Group A (D-13 locked)
Blocks:       nothing

Fixes:
  B1: IncrementalTrainer.Train() — sum stored count + new count before writing
      UpdatedModelWeights.ObservationCount

Gate B: dotnet test --filter "IncrementalTrainer_ObservationCount_IsCumulative" returns 1 passed

### Group C — MAINTENANCE: Fix WeightedScoringService BaseService inheritance
Fix class:    EXTENSION
Files:        src/22.AgentAlgorithms/WeightedScoringService.cs
Lines:        ~20
Blast radius: LOW (only one class)
Depends on:   nothing (independent of A and B)

Fixes:
  C1: WeightedScoringService inherits BaseService(ISessionStorage storage)
      → removes direct ISessionStorage field, uses base storage instead

Gate B: dotnet test --filter "WeightedScoringService" returns ≥ N passed, 0 failed
```

---

## Anti-Patterns

1. **"Grouping raw gaps into sessions without running root-cause-ladder-SKILL.md first"**
   — 12 gaps may collapse to 3 root causes. Grouping 12 gaps = 12 sessions.
   Collapsing to root causes = 3 sessions.

2. **"Assigning session type before estimating file count"** — file count determines
   blast radius. Blast radius determines session type. Guessing type skips estimation.

3. **"Batching high-blast-radius fixes 'for efficiency'"** — a 15-file change
   in one session is a test-failure risk. The extra session is cheaper.

4. **"Starting Group B before Group A completes"** — dependency chains are not
   optional. Group A locked the decision that Group B encodes in code.

5. **"Gate B omitted from a group"** — every group must have a Gate B.
   "Works correctly" is not a Gate B (plan-review-SKILL.md FC-4).

---

## Integration

- root-cause-ladder-SKILL.md provides the K root causes this skill starts from
- solution-scope-gate-SKILL.md Level 1–5 classifies fix class per gap
- assumption-registry-SKILL.md populates the pre-flight assumptions per group
- phase-preflight-SKILL.md verifies pre-flight assumptions before each group starts
- blast-radius-SKILL.md classifies HIGH/MEDIUM/LOW for each group
