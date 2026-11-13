# Layer 0-B — Router Pairs (043)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Redis connector

---

## B17-0B-043-001 — Redis: no token routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |

DecisionCount: 2 | UniqueSubPhases: 2

---

## B17-0B-043-002 — Redis: ACL token routing

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

| Predicate | Sub-phase | Flow |
|---|---|---|
| IMPLEMENT + connector | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |

DecisionCount: 3 | UniqueSubPhases: 3
Note: ACL (Access Control List) is a domain acronym. "token" in "ACL authentication token" fires SECURITY.

---

## B17-0B-043-003 — Redis: "integrate" routing

**Predicates:** ["API_CONTRACT"]

DecisionCount: 1 | UniqueSubPhases: 1 | TransitionType: documentation

---

## B17-0B-043-004 — Redis: TLS does not add SECURITY routing

**Predicates from:** "Refactor the Redis connector to use connection pooling and TLS."
Compiled: ["IMPLEMENT", "API_CONTRACT"]

Routing: 4.3 + 3.3 only. TransitionType: refactoring.
SecurityAuditFlow34 not selected — "TLS" alone is not a SECURITY trigger.
