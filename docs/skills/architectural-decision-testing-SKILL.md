# Architectural Decision Testing

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

A planning session that proposes something contradicting a locked decision
wastes the session. A decision made once and never re-tested may have been
right at the time but wrong after 10 uses. This skill defines the decision
loading protocol (load before proposing), the three testing timeframes
(immediate / short-term / long-term), and the decision invalidation procedure
(negative knowledge is preserved, not deleted).

---

## When to Invoke

- At the start of every ARCHITECT session — before any proposal is made
- After implementing the 4th capability that uses a given decision
- During any architectural review session after 10+ capabilities implemented
- When a test reveals a decision was applied in the wrong context

---

## Section 1 — Decision Loading (Before Any Proposal)

Load prior decisions before proposing anything. A proposal that contradicts
a locked decision without knowing it exists creates a planning waste.

### Step 1 — Find locked decisions for this domain

```bash
# Read all locked decisions
cat docs/DECISIONS-LOCKED.md

# Find decisions relevant to the specific domain
grep -i "[capability class or subproject name]" docs/DECISIONS-LOCKED.md
```

### Step 2 — Classify each decision

| Status | Definition | Rule |
|---|---|---|
| LOCKED | Implemented, tested, or explicitly closed in prior session | Requires decision-reopening-SKILL.md to change |
| OPEN | Stated as direction but not yet implemented or tested | Can be changed with evidence and rationale in this session |
| PENDING | Flagged as open question, not yet resolved | Must resolve before dependent work proceeds |

### Step 3 — Check every proposal against locked decisions

For every proposed solution in this session:
Does it contradict any LOCKED decision?

**If yes: STOP.** Produce a DECISION_REOPENING challenge before proceeding:

```markdown
## DECISION_REOPENING Challenge

**Locked decision:** [exact text of D-NN from DECISIONS-LOCKED.md]
**New evidence:** [what was found that warrants reconsideration]
**Proposed change:** [what would change if reopened]
**Blast radius:** [which capabilities and interfaces depend on this decision]
**Recommendation:** reopen | confirm locked | defer
```

Do not proceed with a proposal that contradicts a LOCKED decision without
explicit approval to reopen. Do not "work around" the decision.

---

## Section 2 — Immediate Testing (Same Session as the Decision)

Run immediately after the decision is made and the phase plan is written.

### For CAPABILITY_CLASSIFICATION decisions:

```
□ Q1-Q4 applied (capability-implementation-gate-SKILL.md 4-question test recorded)
□ Classification consistent with similar capability classes already implemented
  → grep for similar capability name in docs/DECISIONS-LOCKED.md
  → If different classification for same archetype: document why or reopen
□ If classified as AI_PIPELINE: ai-pipeline-design-SKILL.md risk-tier threshold set
□ If classified as PLAIN_CS: capability is on the bootstrap list
```

### For INTERFACE_INTRODUCTION decisions:

```
□ New interface defined in COMPONENT-INTERFACES.md
□ Interface follows fabric-first pattern (Rule 2): I-prefix, injectable
□ BaseService inheritor uses the interface, not a concrete type
□ Both the interface and at least one implementation exist before any consumer
```

### For DEPENDENCY_DIRECTION decisions:

```
□ No circular dependency introduced (A depends on B, B depends on A = BLOCK)
□ Dependency direction follows the plan's subproject ordering
□ Consumer does not reference the producer's concrete type (only the interface)
```

---

## Section 3 — Short-Term Testing (After First 3 Uses of the Decision)

Run during Phase A of the 4th capability that touches the same decision.

### For CAPABILITY_CLASSIFICATION decisions:

```bash
# Were there implementation failures that a different classification would have prevented?
# Check: did any capability classified as PLAIN_CS end up needing AI pipeline?
grep -n "AI_PIPELINE\|PLAIN_CS" docs/DECISIONS-LOCKED.md

# Check: does the classification match observed quality scores?
# If PLAIN_CS capabilities consistently score < 70%: they may need AI pipeline
# If AI_PIPELINE capabilities consistently score > 95% immediately: they may be PLAIN_CS
```

### For INTERFACE_INTRODUCTION decisions:

```bash
# Did the interface stay stable across 3+ implementors?
# Instability = the interface changed between implementors
grep -rn "IYourInterface" src/ --include="*.cs" | grep -v test
# Expected: multiple implementors, same interface shape

# Are there concrete type injections that bypass the interface?
grep -rn "new ConcreteType" src/ --include="*.cs" | grep -v test
# Expected: 0 hits (fabric-first enforcement — Rule 2)
```

### For ARCHITECTURE_PATTERN decisions:

```bash
# Did the pattern hold across 3+ uses?
# Example: BaseService pattern — are all service classes inheriting correctly?
grep -rn ": BaseService\|BaseService(" src/ --include="*.cs"
# Check: count and verify each is correct (not bypassed)

# Were there any gate B failures attributable to the pattern?
grep -rn "BaseService" docs/DECISIONS-LOCKED.md
# Expected: the decision is reinforced, not challenged
```

---

## Section 4 — Long-Term Testing (After 10+ Capabilities)

Run during any architectural review session.

### For semantic decisions (architecture patterns, interface designs):

```bash
# Trend: are capabilities using this decision converging faster than before?
# If a decision improves quality, capabilities using it should need fewer cycles
grep -n "cycleCount\|actualCycles" docs/DECISIONS-LOCKED.md
# Compare: capabilities before and after the decision

# Are there false positive retrievals?
# Is the decision being applied in contexts where it produces wrong guidance?
# Check: has any capability class that used this decision needed a reversal?
grep -n "INVALIDATED\|reopen" docs/DECISIONS-LOCKED.md | grep -i "[decision domain]"
```

### Decision health indicators:

```
HEALTHY decision:
  □ Same problem solved consistently across 5+ capabilities
  □ No capability needed to reopen the decision to proceed
  □ The decision is referenced, not re-derived

WEAKENING decision:
  □ 2+ capabilities deviated from the decision with justification
  □ The deviation pattern is the same across deviations (signal: decision is too rigid)
  □ Consider: update the decision to include the deviation as a valid case

FAILED decision:
  □ Majority of capabilities needed to work around the decision to succeed
  □ The problem the decision was meant to solve still recurs
  □ Action: invalidate and reopen (Section 5)
```

---

## Section 5 — Decision Invalidation

If testing shows a decision was wrong:

**Step 1 — Record the invalidation in DECISIONS-LOCKED.md:**

```markdown
## D-NN — [Decision Name] [INVALIDATED 2026-MM-DD]

**Status:** INVALIDATED
**Original resolution:** [what was decided]
**Invalidation evidence:** [what testing revealed — specific, not vague]
**Negative teaching point:** [what NOT to do in similar situations]
```

The invalidated decision stays in DECISIONS-LOCKED.md with INVALIDATED status.
Future sessions know it was tried and found wrong. Negative knowledge is valuable —
do not delete it.

**Step 2 — Seed the invalidation as a DPO triple:**

```csharp
new DpoTriple
{
    SessionId = currentSessionId,
    CapabilityClass = "ArchitecturalDecisionTesting",
    Chosen = "Document the invalidation with specific evidence and a negative teaching point",
    Chosen_Source = "architectural-decision-testing-SKILL.md Section 5",
    Rejected = "Delete the invalidated decision to 'clean up' the document",
    Rejected_Source = "incorrect cleanup instinct",
    TeachingPoint = "Negative knowledge (what was tried and failed) is as valuable as positive knowledge"
}
```

**Step 3 — Reopen the decision space:**

Run decision-reopening-SKILL.md to design a replacement decision that accounts
for the invalidation evidence.

---

## Section 6 - Extension Plan Repair Loop

For the orchestration and prompt-to-task extension plan, architect governance
uses a plan repair and repair loop:

1. Issue exactly one sub-agent at a time.
2. Preserve the bounded work order scope and do not chain extra execution.
3. Parent REVIEW_INTERNAL reviews the returned evidence.
4. Any actionable gap becomes the next bounded work order.

Deterministic shortcut evidence cannot be accepted by executor self-review.
Any deterministic path that could replace a trainable decision needs parent
REVIEW_INTERNAL sign-off before the plan or gate may treat it as closed.

---

## Anti-Patterns

1. **"Proposing without loading DECISIONS-LOCKED.md first"** — the proposal may
   contradict a locked decision that was made months ago and is already encoded
   in 5 capability classes. The contradiction wastes the session.

2. **"Short-term testing skipped — decision seems to be working"** — the 3-use
   threshold exists because one use may be the happy path and the next two are
   the edge cases. Run the check at the 4th use.

3. **"Invalidated decision deleted from DECISIONS-LOCKED.md"** — negative knowledge
   is valuable. Future sessions need to know this approach was tried and failed.
   Mark INVALIDATED; do not delete.

4. **"Long-term testing skipped — no architectural review scheduled"** — long-term
   testing doesn't need a formal review session. Run the 3 bash checks during any
   session that touches a capability that uses the decision.

---

## Integration

- decision-reopening-SKILL.md handles the case where a proposal contradicts a locked decision
- docs/DECISIONS-LOCKED.md is the authoritative source for all decisions (D-01..D-NN)
- capability-implementation-gate-SKILL.md Q1-Q4 produces the CAPABILITY_CLASSIFICATION decisions tested here
- confidence-seeding-SKILL.md uses architecture pattern decisions to set initial edge confidence values
- meta-arbiter-SKILL.md Section 4 (recent changes) feeds the regression check in short-term testing
