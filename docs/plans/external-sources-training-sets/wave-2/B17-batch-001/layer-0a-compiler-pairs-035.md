# Layer 0-A — Compiler Pairs (035)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: GitHub source control connector

---

## B17-0A-035-001 — GitHub: build connector with webhook and HMAC

**Input:** "Build the GitHub webhook connector with HMAC-SHA256 signature validation and token authentication."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |
| validate signature | SECURITY | 8 |
| token | SECURITY (reinforces; fires once) | 6 |
| authentication | SECURITY (reinforces; fires once) | 8 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
TransitionType: feature_addition
Integration vocabulary: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-035-002 — GitHub: integrate

**Input:** "Integrate GitHub source control API for commit tracking."

Triggers: "Integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation

---

## B17-0A-035-003 — GitHub: build with OAuth

**Input:** "Build GitHub connector with OAuth app token for repository access."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-035-004 — GitHub: wire up and add tests

**Input:** "Wire up the GitHub webhook endpoint and add tests for push and PR events."

| Trigger | Predicate |
|---|---|
| Wire up | API_CONTRACT (Rule 22) |
| webhook | API_CONTRACT (reinforces; fires once) |
| endpoint | API_CONTRACT (reinforces; fires once) |
| add tests | TEST |

CompiledPredicateIds: ["API_CONTRACT", "TEST"]
No build verb → IMPLEMENT absent.
DecisionCount: 2 | UniqueSubPhases: 2 (3.3, 5.1)
