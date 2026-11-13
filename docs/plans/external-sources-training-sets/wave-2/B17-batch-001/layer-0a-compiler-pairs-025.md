# Layer 0-A — Compiler Pairs (025)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Twilio SMS connector

---

## B17-0A-025-001 — Twilio SMS: predicate firing

**Input:** "Build the Twilio SMS connector with webhook delivery status and authentication token."

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

## B17-0A-025-002 — Twilio: "integrate" alone

**Input:** "Integrate the Twilio Messaging API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — "integrate" never fires IMPLEMENT.

---

## B17-0A-025-003 — Twilio: wire up

**Input:** "Wire up the Twilio SMS endpoints."

Triggers: "Wire up" → API_CONTRACT (Rule 22)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
"Wire up" = API_CONTRACT exactly like "integrate" — never IMPLEMENT.

---

## B17-0A-025-004 — Twilio: connect with OAuth

**Input:** "Connect and build the Twilio connector with OAuth token handling."

| Trigger | Predicate |
|---|---|
| connect + external service | API_CONTRACT (Rule 22) |
| build | IMPLEMENT |
| connector | API_CONTRACT (reinforces; fires once) |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
