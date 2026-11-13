# Layer 0-A — Compiler Pairs (031)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Segment analytics connector

---

## B17-0A-031-001 — Segment: build connector

**Input:** "Build the Segment analytics connector with event tracking and batch upload endpoint."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| endpoint | API_CONTRACT (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary: connector, endpoint → IMPLEMENT routes to 4.3

---

## B17-0A-031-002 — Segment: integrate

**Input:** "Integrate Segment customer data platform."

Triggers: "Integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
"customer data platform" is a domain phrase — no predicate.

---

## B17-0A-031-003 — Segment: build with write key token

**Input:** "Build the Segment connector using write key token authentication."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6; unconditional) |
| authentication | SECURITY (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-031-004 — Segment: refactor with tests

**Input:** "Refactor the Segment connector and write unit tests for track and identify methods."

| Trigger | Predicate |
|---|---|
| Refactor | IMPLEMENT (TransitionType=refactoring) |
| connector | API_CONTRACT |
| write unit tests | TEST |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
TransitionType: refactoring
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 5.1)
