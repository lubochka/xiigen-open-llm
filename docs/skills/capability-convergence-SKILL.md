# Capability Convergence

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

A capability designed by one model in isolation inherits that model's blind
spots. A genesis context package authored without cross-model challenge will
pass the author's assumptions unchallenged and fail when an arbiter with a
different perspective reviews it. Convergence is the process of building the
verified representation of what a capability needs to do through multi-model
consensus: a proposer generates the representation, specialized challengers
question it, and the session does not close until all challenges are resolved
or deferred with explicit justification.

---

## When to Invoke

- Before writing any genesis context package for a new capability class
- Before Phase B generation for any new subproject task type
- When system-intake-SKILL.md is complete and capability design begins
- When a capability design review fails Check 1 (intent completeness)

---

## Section 1 — What a Capability Representation Is

```
CapabilityRepresentation = {structure, intent, constraints, quality}

structure:   The input and output shapes, dependencies, triggers, and
             events emitted. Named shapes with field names and types —
             NOT class names from any existing codebase.

intent:      One sentence describing what a user gains from this capability.
             Intent describes USER OUTCOMES, not system behavior.
             "User identity is created and awaiting verification" — intent.
             "Calls UserService.create() and enqueues event" — implementation.

constraints: CF-N rules that are verifiable and binary. Each constraint must
             have a failing check: what breaks when the constraint is violated?
             If you cannot state what breaks: it is a preference, not a constraint.

quality:     Observable scoring criteria that can be verified externally —
             not test method names, not coverage percentages.
             "Duplicate email within same tenant is rejected with CONFLICT status" — quality.
             "testUserCreationService() passes" — not quality.
```

A CapabilityRepresentation is NOT a service implementation plan. It is the
specification that genesis prompts are derived from — not authored independently.
A representation that mixes implementation details with structural description
is invalid and will cause the genesis stage to generate code that compiles
but violates a constraint the representation should have specified.

---

## Section 2 — Five Challenger Roles

| Role | What it challenges |
|------|-------------------|
| Domain | Intent correctness and iron rule applicability |
| Security | Security properties, failure modes, data exposure |
| Business | Cross-capability consistency and field contracts |
| Completeness | Missing invariants, unspecified failure modes, gaps |
| Principles | CLAUDE.md Rules 1-10 compliance in the representation |

Minimum panel by representation type:

```
Simple capability (ROUTING archetype):    Domain + Completeness
Standard capability (SCORING/RETRIEVAL): Domain + Security + Completeness
Complex capability (ORCHESTRATION):      All five roles
```

Each challenger operates independently. No challenger sees another's
verdict before producing its own. (Blind evaluation — same invariant as
principles-arbiter-SKILL.md isolation rule.)

---

## Section 3 — Convergence Rounds

Each round:
1. Proposer generates the representation (or revision if previous round had challenges)
2. Each challenger produces: `APPROVED` or `CHALLENGE` with specific evidence
3. If any challenger produces `CHALLENGE`: proposer addresses the challenge
4. If challenger cannot verify a constraint without external information:
   emit `CONTEXT_INSUFFICIENT` from pipeline-position-check-SKILL.md

Session closes when: all challengers produce `APPROVED`, or remaining
challenges are marked `DEFERRED_CONSTRAINT` (explicit justification required).

**Maximum rounds:** 5. If not converged by round 5, escalate to Luba.

---

## Section 4 — CONTEXT_INSUFFICIENT Handling

When a challenger cannot verify a constraint without external data:

```
DOWNSTREAM_CONTRACT  → Read COMPONENT-INTERFACES.md for the consuming subproject
BUSINESS_RULE        → Check DECISIONS-LOCKED.md for the relevant D-NN entry
SPEC_CONFLICT        → Document both behaviors; flag for Luba (HUMAN_JUDGMENT)
HUMAN_JUDGMENT       → Mark as DEFERRED_CONSTRAINT; continue with explicit flag
```

A DEFERRED_CONSTRAINT allows the representation to close the session but
marks the constraint as unverified. The downstream phase must resolve
the constraint before Phase B generation begins.

---

## Section 5 — Representation Verification Checklist

Before marking a CapabilityRepresentation as VERIFIED:

```
□ consensusReached = true (not PARTIAL_CONSENSUS or STALLED)
□ structure.inputShape: field names and types — no class names
□ structure.outputShape: field names and types — no framework types
□ intent.purpose: describes user outcome — not system behavior
□ intent.scope: names what this covers AND what it does NOT cover
□ constraints: each has a failing check (what breaks when violated?)
□ quality.scoringCriteria: externally observable (verifiable without code access)
□ No field contains a class name, service name, or framework name
□ Each constraint cites at least one CF-N rule or CLAUDE.md Rule N
□ If any DEFERRED_CONSTRAINT: logged in Constraints.Unknowns (system-intake-SKILL.md)
```

---

## Section 6 — Dependency Ordering

Representations within a subproject have a dependency graph:

```
R1 must be verified before R2 if R2 needs facts from R1.
R1 → R2 → R3 is the correct order.
```

If R3's business challenger discovers R1 is wrong: escalate to the
convergence session owner to reopen R1 with new context. R1 re-verified.
Correction propagates to R2 and R3.

Verified facts from completed representations are immutable within the
session. R3 cannot re-challenge R1 — it can only escalate if R1 is wrong.

---

## Section 7 — Genesis Derivation (After Convergence)

After a CapabilityRepresentation is VERIFIED, the genesis context package
is derived from it — not authored independently:

```
Section 1: intent.purpose + intent.invariants + constraints (capability-neutral)
Section 2: structure.inputShape + structure.outputShape (capability-neutral)
Section 3: quality.scoringCriteria (capability-neutral)
Section 4: Stack-specific implementation notes from COMPONENT-INTERFACES.md
```

One verified representation → one genesis context package for the .NET stack.
No manual authoring per stack variant.

---

## Anti-Patterns

1. **"Proposer generates representation without challenger panel"** — a
   single-model design inherits the model's blind spots. Convergence
   requires at minimum domain + completeness challengers.

2. **"Representation fields contain class names"** — `structure.inputShape = RegisterUserDto`
   is not a shape — it is a class reference. Replace with field names and types.
   A class name in a representation couples it to one codebase.

3. **"intent.purpose describes what the system does internally"** — "calls
   IncrementalTrainer.Update() and writes to DomainModelRegistry" is
   implementation. "Capability confidence is updated based on new
   observations" is intent. Intent must pass the business-analyst test:
   would this make sense to someone with no code access?

4. **"Closing convergence with unresolved CHALLENGE"** — a challenge not
   resolved and not deferred is an invalid representation closure.
   Either address the challenge or explicitly defer it with justification.

5. **"Writing genesis context package before convergence is VERIFIED"** —
   a context package derived from an unverified representation will pass
   score in the first cycle by matching its own assumptions. The violations
   will surface in Phase B when real behavior is observed.

---

## Integration

- system-intake-SKILL.md provides the domain context package that feeds convergence
- capability-design-review-SKILL.md Check 1 (intent completeness) validates the representation
- context-package-design-SKILL.md receives the verified representation as its DOMAIN field
- principles-arbiter-SKILL.md isolation rule applies to the Principles challenger role
- pipeline-position-check-SKILL.md CONTEXT_INSUFFICIENT signal format applies to deferred constraints
- learning-signal-capture-SKILL.md DESIGN_REASONING signal captures the convergence decision at Gate C
