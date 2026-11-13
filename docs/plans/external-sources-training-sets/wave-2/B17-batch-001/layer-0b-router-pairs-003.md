# Layer 0-B — Router Pairs (003)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router

---

## B17-0B-003-001 — [IMPLEMENT, API_CONTRACT, SECURITY] → 4.3 + 3.3 + 3.4

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**ConnectorName:** GitHubWebhookConnector

**Routing decisions:**

| Predicate | Sub-phase | FlowId |
|---|---|---|
| IMPLEMENT | 4.3 | IntegrationConnectorFlow43 |
| API_CONTRACT | 3.3 | APIContractFlow33 |
| SECURITY | 3.4 | SecurityAuditFlow34 |

**DecisionCount:** 3
**TransitionType:** feature_addition

---

## B17-0B-003-002 — SECURITY routing: always 3.4, regardless of context

**Rule:** SECURITY always routes to sub-phase 3.4 (Security and Compliance).
This is true whether SECURITY fires from "token", "authentication", "secure", or "validate signature".

**Sub-phase 3.4 name:** Security and Compliance
**FlowId:** SecurityAuditFlow34

**Contrast with:** AUTHORIZATION also routes to 3.4. Two predicates, same destination, two routing decisions.

---

## B17-0B-003-003 — Three-sub-phase scenario: 4.3 + 3.3 + 3.4

**Scenario:** "Build the GitHub webhook listener and implement signature validation to secure incoming payloads."

**Complete routing output:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" },
  { predicate: "SECURITY", subPhase: "3.4", flow: "SecurityAuditFlow34" }
]
DecisionCount: 3
TransitionType: feature_addition
```

---

## B17-0B-003-004 — Three-decision routing validation: check all three

**Gate B for three-predicate routing:**
- IMPLEMENT must route to 4.3 (not 4.1) — because webhook vocabulary present
- API_CONTRACT must route to 3.3 (not 4.3)
- SECURITY must route to 3.4

**All three must be correct. If any one is wrong, the routing fails Gate B.**

**Nearest wrong:** SECURITY → 4.3 (wrong: SECURITY routes to 3.4 always, even in integration scenario)
