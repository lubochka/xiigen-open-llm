# Layer 3 — Teaching Unit Pairs (009)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-009-001 — Teaching: knowledge_edge vs GapHandler — different paths

LESSON: B38 gap vocabulary (rollout, go-live, cutover, etc.) produces knowledge_edge.
B33/B34 AI/guidance gaps produce GapHandler + SCHEDULE_CAPABILITY_LEARNING.
These are different paths and must not be confused.

---

## B38-TU-009-002 — knowledge_edge path (B38)

When:
- Input is project delivery operational vocabulary (rollout, go-live, cutover, runbook, etc.)
- Zero predicates fire
- PredicateSet = []

What happens:
- knowledge_edge state set
- CapabilityGapWarnings populated
- No flow executes
- No GapHandler invoked
- No SCHEDULE_CAPABILITY_LEARNING
- Terminal state returned to caller

Why:
The system is at the edge of its knowledge for project delivery operational vocabulary.
It does not try to learn it (that is GapHandler's job for different types of gaps).
It acknowledges the boundary and returns gap metadata.

---

## B38-TU-009-003 — GapHandler path (B33/B34 style)

When:
- Input describes a capability the AI/guidance system should have but doesn't
- Example: "Guide me through setting up a new AI recommendation engine" (B33)
- The system identifies a specific learnable capability gap

What happens:
- GapHandler invoked
- SCHEDULE_CAPABILITY_LEARNING task created
- System commits to learning the capability
- Not a terminal state — it's a learning trigger

Why:
The gap is in the AI's guidance capabilities, not in the domain vocabulary scope.
The system CAN learn this — it just hasn't yet.

---

## B38-TU-009-004 — Side-by-side comparison

| Property | knowledge_edge (B38) | GapHandler (B33/B34) |
|---|---|---|
| Triggered by | Gap vocabulary, zero predicates | AI capability gap |
| Handler | None (terminal) | GapHandler |
| Downstream action | None | SCHEDULE_CAPABILITY_LEARNING |
| CapabilityGapWarnings | Yes | Yes (different format) |
| Can be retried | No (scope boundary) | Yes (after learning) |
| Represents | Vocabulary outside scope | Missing learnable capability |
| Example | "Execute the rollout" | "Guide me through microservices setup" |

Never use GapHandler for B38 zero-predicate inputs.
Never use knowledge_edge for B33/B34 capability learning gaps.
