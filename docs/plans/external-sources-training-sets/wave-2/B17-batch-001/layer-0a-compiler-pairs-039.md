# Layer 0-A — Compiler Pairs (039)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: PagerDuty connector

---

## B17-0A-039-001 — PagerDuty: predicate firing

**Input:** "Build the PagerDuty incident connector with webhook event routing and API token authentication."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |
| token | SECURITY | 6 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
TransitionType: feature_addition
Integration vocabulary present: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-039-002 — PagerDuty: "integrate" alone

**Input:** "Integrate the PagerDuty Events API v2."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — Rule 1 is unconditional.

---

## B17-0A-039-003 — PagerDuty: OAuth token

**Input:** "Implement the PagerDuty connector with OAuth 2.0 token for user-level access."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-039-004 — PagerDuty: domain nouns alone

**Input:** "PagerDuty incident escalation policy triggers on-call alerts."

Triggers: none (proper noun; domain nouns: incident, escalation, policy, alert — none fire predicates)
CompiledPredicateIds: []
TransitionType: none

---

## B17-0A-039-005 — PagerDuty: test + document

**Input:** "Build the PagerDuty connector, write integration tests, and document the alert API contract."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| write integration tests | TEST |
| document | DOCUMENT |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
DecisionCount: 4 | UniqueSubPhases: 4 (4.3, 3.3, 5.1, 2.1)
TransitionType: feature_addition
