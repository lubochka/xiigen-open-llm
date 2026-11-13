# Layer 0-A — Compiler Pairs (047)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: PostgreSQL connector

---

## B17-0A-047-001 — PostgreSQL: predicate firing

**Input:** "Build the PostgreSQL database connector for transactional reads and writes."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Note: transactional, reads, writes are domain nouns — no predicates.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-047-002 — PostgreSQL: "integrate" alone

**Input:** "Integrate the PostgreSQL JDBC (Java Database Connectivity) driver."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
Note: JDBC (Java Database Connectivity) is a domain acronym — fires no predicate.

---

## B17-0A-047-003 — PostgreSQL: connection token fires SECURITY

**Input:** "Implement the PostgreSQL connector with a service account access token."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-047-004 — PostgreSQL: "query" is not IMPLEMENT

**Input:** "Query the PostgreSQL database for active user sessions."

Triggers: none ("query" is not an IMPLEMENT trigger)
CompiledPredicateIds: []
TransitionType: none

---

## B17-0A-047-005 — PostgreSQL: API_CONTRACT + SECURITY combo emphasis

**Input:** "Implement the PostgreSQL connector; the connector exposes an API endpoint and uses a bearer token for row-level security."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT (Rule 4) |
| API endpoint | API_CONTRACT (reinforces; fires once — "API" alone not trigger; "connector" already fired it) |
| token | SECURITY (Rule 6) |

Teaching: When both "connector" and "API" appear, API_CONTRACT fires once (set deduplication). "bearer token" is a token compound — SECURITY fires unconditionally.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3
