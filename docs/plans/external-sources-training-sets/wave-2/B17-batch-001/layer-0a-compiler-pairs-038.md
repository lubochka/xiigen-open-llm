# Layer 0-A — Compiler Pairs (038)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Slack connector

---

## B17-0A-038-001 — Slack: predicate firing

**Input:** "Build the Slack notification connector with incoming webhook support and bot token authentication."

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

## B17-0A-038-002 — Slack: "integrate" alone

**Input:** "Integrate the Slack Incoming Webhooks API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — "integrate" is API_CONTRACT ONLY regardless of connector domain.

---

## B17-0A-038-003 — Slack: slash command with OAuth

**Input:** "Implement Slack slash command connector with OAuth 2.0 token exchange."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-038-004 — Slack: "signing secret" does not fire SECURITY alone

**Input:** "Build the Slack connector using the signing secret for request validation."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |

Note: "signing secret" is not in the SECURITY trigger list (Rule 6/8 require "token" or "authenticate"/"validate signature"). HMAC validation phrase fires SECURITY; "signing secret" alone does NOT.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
DecisionCount: 2 | UniqueSubPhases: 2

---

## B17-0A-038-005 — Slack: HMAC phrase fires SECURITY

**Input:** "Build the Slack connector and validate the HMAC-SHA256 signature on each event."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| validate + HMAC-SHA256 signature | SECURITY (Rule 8) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
