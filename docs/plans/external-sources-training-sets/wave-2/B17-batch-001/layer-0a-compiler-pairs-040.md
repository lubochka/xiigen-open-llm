# Layer 0-A — Compiler Pairs (040)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Datadog connector

---

## B17-0A-040-001 — Datadog: predicate firing

**Input:** "Build the Datadog metrics connector to push application telemetry via the API."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Note: "API" alone does not fire API_CONTRACT — "connector" is the trigger. "API" here is contextual.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-040-002 — Datadog: API key vs token

**Input:** "Implement the Datadog connector using the Datadog API key for authentication."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |

Note: "API key" does NOT fire SECURITY (Rule 6 requires "token" or authenticate/HMAC phrases). "API key" without the word "token" is a non-trigger.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
DecisionCount: 2 | UniqueSubPhases: 2 (4.3, 3.3)

---

## B17-0A-040-003 — Datadog: agent token fires SECURITY

**Input:** "Implement the Datadog connector with the Datadog Agent authentication token."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-040-004 — Datadog: "integrate" alone

**Input:** "Integrate the Datadog Metrics API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — no exception for monitoring/observability domain.

---

## B17-0A-040-005 — Datadog: refactor

**Input:** "Refactor the Datadog metrics connector to use the v2 API."

| Trigger | Predicate |
|---|---|
| Refactor | IMPLEMENT (Rule 11) |
| connector | API_CONTRACT |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: refactoring
DecisionCount: 2 | UniqueSubPhases: 2
