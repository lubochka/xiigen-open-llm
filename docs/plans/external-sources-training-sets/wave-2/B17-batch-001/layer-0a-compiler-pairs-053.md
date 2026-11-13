# Layer 0-A — Compiler Pairs (053)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Vercel connector

---

## B17-0A-053-001 — Vercel: predicate firing

**Input:** "Build the Vercel deployment connector with webhook event handling for deployment status updates."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary present: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-053-002 — Vercel: bearer token fires SECURITY

**Input:** "Implement the Vercel connector with bearer token authentication for the Deployments API."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-053-003 — Vercel: "integrate" alone

**Input:** "Integrate the Vercel REST API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — deployment/CDN (Content Delivery Network) domain has no exception.

---

## B17-0A-053-004 — Vercel: "deploy" is not IMPLEMENT

**Input:** "Deploy the application to Vercel and configure environment variables."

Triggers: none ("deploy" and "configure" are not IMPLEMENT triggers)
CompiledPredicateIds: []
TransitionType: none
Teaching: "deploy", "configure", "provision" do not fire IMPLEMENT.

---

## B17-0A-053-005 — Vercel: webhook + HMAC + test

**Input:** "Build the Vercel connector, validate the HMAC-SHA256 webhook signature, and write deployment tests."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| webhook | API_CONTRACT (reinforces; fires once) |
| validate HMAC-SHA256 webhook signature | SECURITY (Rule 8) |
| write + tests | TEST |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"]
DecisionCount: 4 | UniqueSubPhases: 4 (4.3, 3.3, 3.4, 5.1)
