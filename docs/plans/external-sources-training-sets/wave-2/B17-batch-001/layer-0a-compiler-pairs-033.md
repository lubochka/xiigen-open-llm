# Layer 0-A — Compiler Pairs (033)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Zendesk support connector

---

## B17-0A-033-001 — Zendesk: build connector

**Input:** "Build the Zendesk support connector with ticket webhook and token authentication."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |
| token | SECURITY | 6 |
| authentication | SECURITY (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
TransitionType: feature_addition
Integration vocabulary: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-033-002 — Zendesk: integrate

**Input:** "Integrate Zendesk customer support ticketing API."

Triggers: "Integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation

---

## B17-0A-033-003 — Zendesk: build with OAuth

**Input:** "Build Zendesk connector with OAuth 2.0 token for agent authentication."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY (Rule 6; unconditional) |
| authentication | SECURITY (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-033-004 — Zendesk: "authorization" English word trap

**Input:** "Build Zendesk connector with proper authorization for support agents."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| "authorization" (English word) | NO PREDICATE — English word ≠ AUTHORIZATION trigger |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
DecisionCount: 2 | UniqueSubPhases: 2 (4.3, 3.3)
The English word "authorization" does NOT fire AUTHORIZATION. Only "OAuth" fires AUTHORIZATION.
