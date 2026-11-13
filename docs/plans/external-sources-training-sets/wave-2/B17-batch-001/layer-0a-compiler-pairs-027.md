# Layer 0-A — Compiler Pairs (027)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Plaid banking connector

---

## B17-0A-027-001 — Plaid: build connector with OAuth

**Input:** "Build the Plaid banking connector with OAuth token exchange and link token handling."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| OAuth | AUTHORIZATION | 7 |
| token (two occurrences) | SECURITY | 6 (fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
TransitionType: feature_addition
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
"token" fires SECURITY once regardless of count in input.

---

## B17-0A-027-002 — Plaid: integrate

**Input:** "Integrate Plaid into the payment flow."

Triggers: "Integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
No IMPLEMENT. "payment flow" is a domain phrase — not integration vocabulary.

---

## B17-0A-027-003 — Plaid: implement with authentication

**Input:** "Implement Plaid link token creation and bank account authentication."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| token | SECURITY |
| authentication | SECURITY (reinforces; fires once) |

Integration vocabulary check: no "connector", "webhook", "API", "external service", "gateway", "SDK" → IMPLEMENT routes to 4.1
CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
DecisionCount: 2 | UniqueSubPhases: 2 (4.1, 3.4)
"Plaid" proper noun does not supply integration vocabulary.

---

## B17-0A-027-004 — Plaid: build with document

**Input:** "Build the Plaid banking API connector and document the integration contract."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| API | API_CONTRACT (reinforces; fires once) |
| document | DOCUMENT |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 2.1)
