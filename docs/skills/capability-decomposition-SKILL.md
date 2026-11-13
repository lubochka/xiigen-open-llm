# Capability Decomposition

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

A capability design with 5 input types, 4 independent failure modes, and 3 distinct
responsibilities cannot be implemented as a single unit and tested with a single Gate B.
Without a structured decomposition protocol, the executor makes ad-hoc split decisions —
or fails to split when it should. This skill defines 5 complexity signals that detect
when a capability must be decomposed into sub-capabilities, a termination bound that
prevents infinite recursion, and the verification checks that confirm a decomposition
is genuine (not duplicate).

---

## When to Invoke

- Before writing implementation for any capability with unclear scope
- When phase-planning-SKILL.md Gate 1 (discovery) finds more than 3 distinct responsibilities
- When a capability's Gate B condition covers multiple independent behaviors
- When subproject-examination-SKILL.md finds a sub-capability with 4+ iron rules

---

## Section 1 — LEAF vs EXPAND

The fundamental decision before any capability implementation:

```
LEAF:   This capability's intent is specific enough to implement directly.
        → Proceed to implementation with a single Gate B condition
        
EXPAND: This capability contains multiple distinct responsibilities.
        Each responsibility should become its own sub-capability with its own Gate B.
        → Decompose into sub-capabilities → each sub-capability enters a new design cycle
```

The EXPAND verdict is a judgment, not a rule. The 5 complexity signals below are
what to weigh — not a checklist where all must trigger.

---

## Section 2 — The 5 Complexity Signals

Observable properties that suggest expansion is warranted. Each signal references
a specific design artifact field so the verdict is traceable.

### Signal 1 — Multi-Responsibility Intent (weight: moderate)

```
Field:   capability design "purpose" description
Signal:  The purpose contains more than one distinct action.
Words:   "and", "then", "as well as", "also", "additionally"
Example: "Accept credentials AND validate the token AND determine auth path"
         Three distinct actions — candidate for expansion.
Counter: "Accept credentials" alone — single action, no expansion signal.
```

### Signal 2 — Multiple Independent Input Types (weight: strong)

```
Field:   capability input contract (method parameters or request type)
Signal:  More than 3 distinct input types where no two share a schema.
Example: Inputs: {email, password, ssoToken, inviteCode, sessionId}
         Five inputs — email+password path, SSO path, invite path are
         three independent processing paths → expansion candidate.
Counter: Inputs: {query, sessionId} — two fields, same retrieval context.
         Single processing path → no expansion signal.
```

### Signal 3 — Multiple Independent Failure Modes (weight: strong)

```
Field:   iron rules from iron-rule-derivation-SKILL.md (failure modes)
Signal:  More than 2 failure modes that cannot be detected by the same check.
Example: Failure 1: BM25 score below threshold (scoring check)
         Failure 2: vector index not populated (infrastructure check)
         Failure 3: session context expired (lifecycle check)
         Three independent checks → three independent responsibilities.
Counter: Failure 1: score < 0.70 | Failure 2: score > 0.95
         Both detected by the same scoring function → not independent.
```

### Signal 4 — Cross-Domain Constraints (weight: moderate)

```
Field:   iron rules covering different domain areas
Signal:  Constraints from more than 2 different domain areas.
Domain areas: scoring, storage, session lifecycle, message bus events,
              model training, retrieval, DNA compliance.
Example: Constraint 1: BM25 score normalized before merge (scoring)
         Constraint 2: session context checked before scoring (session lifecycle)
         Constraint 3: ScoreUpdated event published after completion (message bus)
         Three domain areas → expansion candidate.
Counter: All constraints are normalization rules → single domain → no signal.
```

### Signal 5 — Multiple Independent Gate B Conditions (weight: strong when present)

```
Field:   Gate B condition in phase plan
Signal:  The Gate B condition has "AND" connecting independent verifiable assertions.
Example: "dotnet test → ScoreNormalized = true AND EventPublished = true"
         Two independent assertions → two independent Gate B conditions.
         This means two sub-capabilities, not one.
Counter: "dotnet test → 1 passing" — single assertion → no expansion signal.
Note:    Multiple Gate B conditions in one phase is also a Phase planning violation
         (plan-review-SKILL.md FC-4). This signal and that violation resolve together.
```

---

## Section 3 — Termination Bound (MACHINE constraint)

The termination bound prevents infinite decomposition:

```
Default termination depth:  3 levels
Maximum termination depth:  5 levels (above 5 requires Luba approval)
At depth = termination_depth: verdict is always LEAF — no exceptions.
```

Depth counting:
```
Depth 1: top-level capability in the phase plan
Depth 2: sub-capability after one EXPAND decision
Depth 3: sub-sub-capability after two EXPAND decisions (default leaf level)
```

Why these defaults:
- Depth 3 covers: capability → sub-capability → leaf. Three levels is sufficient.
- Any capability requiring more than 3 levels is a design error (scope too large),
  not a decomposition failure.

---

## Section 4 — Decomposition Output Format

When EXPAND is decided, produce this before any implementation begins:

```markdown
## Capability Decomposition — [CapabilityClass]

Verdict: EXPAND
Signal triggered: Signal 3 (3 independent failure modes), Signal 5 (2 Gate B conditions)

Sub-capabilities:
- [CapabilityClass]Scorer — scores input using BM25 + vector hybrid
  Gate B: "dotnet test → NormalizedScore in [0,1]"
  
- [CapabilityClass]Recorder — publishes outcome to IAgentMessageBus
  Gate B: "dotnet test → ScoreUpdated event received by subscriber"

Non-overlap check:
  Scorer does not touch IAgentMessageBus ✓
  Recorder does not compute scores ✓
  (Overlap would mean: removing Recorder breaks Scorer — it does not)
```

---

## Section 5 — Non-Overlap Verification

For every EXPAND verdict, verify sub-capabilities are genuinely distinct:

```
Overlap test: For sub-capabilities A and B, does removing B break A's coverage?
  If yes → A depends on B → they are not distinct → merge them back.
  If no  → they are distinct → keep them separate.

Example failure:
  Sub-capability A: "normalize BM25 scores"
  Sub-capability B: "normalize vector scores"
  Both normalize → one sub-capability "normalize all scores", not two.

Example pass:
  Sub-capability A: "compute normalized score"
  Sub-capability B: "record outcome and update confidence"
  Different responsibilities → distinct.
```

---

## Anti-Patterns

1. **"This capability is complex — EXPAND"** — EXPAND with no signal citation is
   not auditable. Cite at least one of the 5 signals with evidence from the design.

2. **"Termination depth = 7 for this complex capability"** — maximum is 5 without
   Luba approval. A capability needing 7 levels of decomposition is a design error.

3. **"Sub-capability decomposition will be done during implementation"** — the decomposition
   determines the Gate B conditions for each sub-capability. Write it before implementation.

4. **"All 5 signals triggered for every capability I reviewed"** — signals are being used
   as a mandatory checklist rather than weights. Most capabilities will trigger 0-2 signals.
   5/5 triggered means the signals are being misapplied.

---

## Integration

- plan-review-SKILL.md FC-4 (multiple Gate B conditions) triggers this skill
- subproject-completeness-checker-SKILL.md T5 (Gate B measurable) requires single Gate B per capability
- iron-rule-derivation-SKILL.md provides Signal 3 (independent failure modes)
- phase-planning-SKILL.md Gate 1 (discovery) uses Signal 2 (input types) to detect decomposition need
- capability-routing-gate-SKILL.md Q3 (is it traceable?) — each sub-capability after decomposition must be traceable
