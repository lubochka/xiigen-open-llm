# Layer 0-A — Compiler Pairs (059)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: HashiCorp Vault connector

---

## B17-0A-059-001 — HashiCorpVault: predicate firing

**Input:** "Build the HashiCorp Vault secrets connector with dynamic secret lease and token renewal."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| token | SECURITY | 6 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
TransitionType: feature_addition
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-059-002 — HashiCorpVault: "integrate" alone

**Input:** "Integrate the HashiCorp Vault API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — secrets management domain has no exception.

---

## B17-0A-059-003 — HashiCorpVault: OAuth + token

**Input:** "Implement the Vault connector with OAuth 2.0 token for AppRole authentication."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3

---

## B17-0A-059-004 — HashiCorpVault: authenticate fires SECURITY

**Input:** "Build the Vault connector and authenticate using the AppRole secret ID."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| authenticate | SECURITY (Rule 8) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-059-005 — HashiCorpVault: "retrieve secrets" is not IMPLEMENT

**Input:** "Retrieve secrets from HashiCorp Vault and inject them into the application environment."

Triggers: none ("retrieve" and "inject" are not IMPLEMENT triggers)
CompiledPredicateIds: []
TransitionType: none
Teaching: operations on Vault data ("retrieve", "inject", "rotate", "renew") do not fire IMPLEMENT. The build verb must appear.
