# Layer 0-A — Compiler Pairs (042)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Elastic (Elasticsearch) connector

---

## B17-0A-042-001 — Elastic: predicate firing

**Input:** "Build the Elasticsearch connector for document indexing and full-text search."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-042-002 — Elastic: API key does not fire SECURITY

**Input:** "Implement the Elasticsearch connector using the Elastic Cloud API key."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |

Note: "API key" without the word "token" does NOT fire SECURITY (Rule 6 requires "token").
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
DecisionCount: 2 | UniqueSubPhases: 2

---

## B17-0A-042-003 — Elastic: bearer token fires SECURITY

**Input:** "Implement the Elasticsearch connector with bearer token authentication."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-042-004 — Elastic: "integrate" alone

**Input:** "Integrate the Elasticsearch REST API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — indexing domain does not override Rule 1.

---

## B17-0A-042-005 — Elastic: OAuth + token

**Input:** "Build the Elasticsearch connector with OAuth token-based cluster access."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
