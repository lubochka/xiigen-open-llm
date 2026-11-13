# Per-Entity Examination

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

A session that examines 3 of 20 subprojects and synthesizes architectural
conclusions has a 15% evidence base producing a 100% claim scope. The correction
arrives downstream and forces re-examination — but with the sunk cost of the
wrong synthesis already in the plan. This skill installs the 7-step protocol
that makes multi-entity examination rigorous: declare hypotheses before examining
instance 1, examine all N in fixed sequence, synthesize only after all N are
complete.

---

## When to Invoke

- Any ARCHITECT, PLANNING, or INVESTIGATION session examining N≥5 instances
  of the same entity type to answer a system-level question
- Triggered by reconnaissance-gate-SKILL.md domain universe block when N≥5
  unread entities are material to synthesis claims
- Triggered when hypotheses array is empty at the time per-instance examination
  records begin appearing

Entity types this protocol governs in this project: subprojects, capability
classes, interface implementations, test files, xUnit test methods within a class,
skill files, execution logs, phase plans.

---

## Section 1 — The 7-Step Protocol

### Step 1 — Declare entity type and universe size (N)

Before reading any instance file, state:

```
Entity type: [subprojects | capabilities | interfaces | other]
Total universe (N): [integer] — source: [file or grep command that established N]
```

N is counted, not estimated. Record the command that produces N.

### Step 2 — Declare hypotheses (minimum 1, maximum 8) before instance 1

Before opening any instance file:

```
H-[N] — [short name]
Claim:        [one sentence asserting what H-N says about the entity type as a whole]
Confirms if:  [specific observable in which file type if H-N holds]
Refutes if:   [specific observable that would be found if H-N fails]
Scope:        [which architectural question this tests, or "cross-cutting"]
```

**Why before instance 1:** hypotheses formed after reading the first instances
carry confirmation bias from those reads. Pre-declaration removes the feedback
loop. The hypothesis is committed before any evidence exists to shape it.

**Maximum 8:** more than 8 hypotheses create per-instance result records that
are too long to fill consistently. Consolidate before beginning.

### Step 3 — Define the per-instance step sequence (maximum 5 steps)

Before examining instance 1, define the fixed procedure every instance
will go through:

```
Step 0 — Inventory: list files without reading — establishes what exists
Step 1 — [name]: what to read, which hypotheses it tests
Step 2 — [name]: what to read, which hypotheses it tests
...
Step N — Verdict: how H verdicts are assembled from steps 1-N
```

Maximum 5 steps ensures the procedure is executable without consuming a
full context window per instance. The same steps apply to every instance
in the same order.

### Step 4 — Define the result record format before instance 1

No field may be added mid-examination. Every instance's result record
uses the same format:

```
ENTITY_ID:   [identifier for this instance]
TITLE:       [human-readable name]

STEP_0_INVENTORY:
[inventory items from Step 0 — YES / NO / [count]]

H_VERDICTS:
H-1 ([name]): [CONSISTENT | INCONSISTENT | PARTIAL | NOT_TESTABLE] — [one sentence evidence]
H-2 ([name]): [CONSISTENT | INCONSISTENT | PARTIAL | NOT_TESTABLE] — [one sentence evidence]
[... one row per hypothesis]

OPEN_QUESTIONS:
Q[N]: [question] — raised by: [specific finding]
Fix class: [A | B | C | D | E | F from pipeline-bug-taxonomy-SKILL.md]
```

Verdict vocabulary — four values, no others:

```
CONSISTENT    Instance evidence supports the hypothesis. Cite file:line.
INCONSISTENT  Instance evidence contradicts the hypothesis. Cite the finding.
PARTIAL       Holds for part of this instance. Name which part holds and which does not.
NOT_TESTABLE  No implemented code or relevant docs to test this hypothesis.
              NOT_TESTABLE is a finding, not a shortcut.
```

### Step 5 — Examine all N instances in declared sequence

No instance is skipped. No instance receives a reduced procedure. The
sequence is stated before examination begins and does not change.

`NOT_APPLICABLE` (step has no material to examine) and `NOT_TESTABLE`
(hypothesis cannot be tested for this instance) are valid findings.
An instance with these verdicts is a complete instance.

### Step 6 — Running tally after every 10 instances

```
RUNNING TALLY — after instance [N]
H-1 ([name]):  CONSISTENT: [n]  INCONSISTENT: [n]  PARTIAL: [n]  NOT_TESTABLE: [n]
H-2 ([name]):  CONSISTENT: [n]  INCONSISTENT: [n]  PARTIAL: [n]  NOT_TESTABLE: [n]
No conclusions drawn. Tally only.
```

The tally is counts with no analysis language. The synthesis session draws
conclusions from the tally. The examining session does not.

### Step 7 — Synthesis only after all N instances are complete

The examining session produces: result records, running tally, and open
question list. No architectural conclusions. Synthesis is produced in a
separate session after the Freeze Gate (Section 2) passes.

---

## Section 2 — Freeze Gate

The examination is done when all three conditions hold simultaneously:

```
Condition 1 — Completeness
  Every instance in the declared universe has a complete result record.
  Complete means: every mandatory field populated (NOT_APPLICABLE and
  NOT_TESTABLE are populated values, not omissions).
  Verification: count result records. Count instances with all H_VERDICT
  fields populated. Both counts must equal N.

Condition 2 — Convergence
  The second half of the examination (instances N/2+1 through N) produced
  no finding classes absent from the first half (1 through N/2).
  A "finding class" is a structural pattern type, not an instance of a pattern.
  "IVectorIndex returns wrong shape" is a finding class.
  "BruteForceIndex returns wrong shape" is an instance of that class.
  If Condition 2 fails: add H-N+1 for the new class; re-examine from the
  first instance where it appeared; check whether earlier instances need
  re-evaluation for H-N+1.

Condition 3 — Implementation Evidence
  At least one finding per non-NOT_TESTABLE hypothesis cites an
  IMPLEMENTATION or TEST excerpt (from reconnaissance-gate-SKILL.md
  evidence layer definitions).
  If Condition 3 fails: produce a HANDOFF — the specific grep command
  that would confirm or deny each unverified hypothesis.
  The examination is FROZEN until the HANDOFF returns a result.
  FROZEN is a named state. It is not "going in circles."
```

**Examination complete** = all three conditions met.
**Synthesis session authorized** when examination is complete.

---

## Section 3 — Examiner vs Synthesis Session

| Role | Produces | Does NOT produce |
|------|----------|-----------------|
| Examiner | Result records, running tally, open question list | Architectural conclusions, design proposals |
| Synthesis session | Verdict matrix, recommendations from tally | Per-instance re-examination, new instance reads |

---

## Anti-Patterns

1. **"Pattern is obvious after 5 instances — synthesize now"** — five instances
   are not a system-level sample. The pattern may be a cluster, not the system.
   The protocol exists because early-instance patterns fail in the later instances.

2. **"NOT_TESTABLE is a shortcut"** — it is a finding about that instance's
   current implementation state. An ARCHITECT session with many NOT_TESTABLE
   instances learns that many capabilities are at the same early state.

3. **"I'll define the result record format as I go"** — mid-examination format
   changes produce inconsistent records that cannot be aggregated.

4. **"The synthesis session can re-examine instances"** — no. If a result record
   is incomplete, the examining session finishes it. The synthesis session does
   not supplement incomplete records.

5. **"I'll declare hypotheses after I see what the first few show"** — post-hoc
   hypotheses carry the bias of the first instances. Declare before instance 1.

---

## Integration

- reconnaissance-gate-SKILL.md domain universe block (N≥5 unread material entities) is the routing trigger
- claim-verification-SKILL.md verifies RECONCILIATION-layer excerpts found during examination
- gap-to-proposal-SKILL.md governs OPEN_QUESTIONS pre-classification (CONVENTION/ADAPTATION/EXTENSION/NEW_COMPONENT)
- pipeline-bug-taxonomy-SKILL.md provides fix class taxonomy (A-F) for OPEN_QUESTIONS classification
