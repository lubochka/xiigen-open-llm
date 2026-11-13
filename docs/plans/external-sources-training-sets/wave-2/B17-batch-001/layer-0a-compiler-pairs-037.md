# Layer 0-A — Compiler Pairs (037)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Jira connector

---

## B17-0A-037-001 — Jira: predicate firing

**Input:** "Build the Jira issue tracking connector with webhook event handling and API token authentication."

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

## B17-0A-037-002 — Jira: "integrate" alone

**Input:** "Integrate the Jira REST API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — "integrate" never fires IMPLEMENT.

---

## B17-0A-037-003 — Jira: OAuth token

**Input:** "Implement the Jira connector with OAuth 2.0 token-based access."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-037-004 — Jira: proper noun alone

**Input:** "Jira supports custom issue types and workflow transitions."

Triggers: none (proper noun; no build verb; no connector/webhook vocabulary)
CompiledPredicateIds: []
TransitionType: none
"Jira" alone fires nothing.

---

## B17-0A-037-005 — Jira: "connect" with build verb

**Input:** "Build and connect the Jira connector to the project management pipeline."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connect + build verb | API_CONTRACT (Rule 22) |
| connector | API_CONTRACT (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
DecisionCount: 2 | UniqueSubPhases: 2 (4.3, 3.3)
