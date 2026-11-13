# Layer 0-A — Compiler Pairs (046)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: MongoDB connector

---

## B17-0A-046-001 — MongoDB: predicate firing

**Input:** "Build the MongoDB database connector for document storage and aggregation queries."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Note: document storage, aggregation, queries are domain nouns — no predicates.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-046-002 — MongoDB: "integrate" alone

**Input:** "Integrate the MongoDB Atlas Data API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — database domain has no exception to Rule 1.

---

## B17-0A-046-003 — MongoDB: access token fires SECURITY

**Input:** "Implement the MongoDB connector with Atlas access token for cluster authentication."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-046-004 — MongoDB: "store" is not IMPLEMENT

**Input:** "Store user documents in MongoDB and retrieve them by user ID."

Triggers: none ("store" and "retrieve" are not IMPLEMENT triggers)
CompiledPredicateIds: []
TransitionType: none

---

## B17-0A-046-005 — MongoDB: OAuth + token

**Input:** "Build the MongoDB Atlas connector with OAuth 2.0 service account token."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
