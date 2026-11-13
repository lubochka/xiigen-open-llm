# Layer 0-A — Compiler Pairs (032)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Mixpanel product analytics connector

---

## B17-0A-032-001 — Mixpanel: build connector

**Input:** "Build the Mixpanel product analytics connector with event ingestion endpoint."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| endpoint | API_CONTRACT (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary: connector, endpoint → IMPLEMENT routes to 4.3

---

## B17-0A-032-002 — Mixpanel: integrate

**Input:** "Integrate Mixpanel event tracking into the user journey flow."

Triggers: "Integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
"user journey flow" is a domain phrase — no predicate.

---

## B17-0A-032-003 — Mixpanel: connect with token

**Input:** "Connect the Mixpanel external service using project token authentication."

| Trigger | Predicate |
|---|---|
| connect + external service | API_CONTRACT (Rule 22) |
| token | SECURITY (Rule 6; unconditional) |
| authentication | SECURITY (reinforces; fires once) |

CompiledPredicateIds: ["API_CONTRACT", "SECURITY"]
DecisionCount: 2 | UniqueSubPhases: 2 (3.3, 3.4)
No build verb → IMPLEMENT absent.

---

## B17-0A-032-004 — Mixpanel: build with document

**Input:** "Build Mixpanel connector and document the event schema for the analytics contract."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| document | DOCUMENT |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 2.1)
