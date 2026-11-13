# Layer 0-A — Compiler Pairs (026)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: SendGrid transactional email connector

---

## B17-0A-026-001 — SendGrid: build connector

**Input:** "Build the SendGrid transactional email connector with webhook event tracking."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-026-002 — SendGrid: integrate

**Input:** "Integrate SendGrid email delivery API."

Triggers: "Integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
"Integrate" = API_CONTRACT only. No IMPLEMENT despite "delivery" or "email".

---

## B17-0A-026-003 — SendGrid: build with token auth

**Input:** "Build the SendGrid connector using an API key token for authentication."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6; unconditional) |
| authentication | SECURITY (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
"API key token" — "token" fires SECURITY regardless of the surrounding phrase.

---

## B17-0A-026-004 — SendGrid: document and test

**Input:** "Build SendGrid email connector, write integration tests, and document the email API contract."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| write integration tests | TEST |
| document | DOCUMENT |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
DecisionCount: 4 | UniqueSubPhases: 4 (4.3, 3.3, 5.1, 2.1)
