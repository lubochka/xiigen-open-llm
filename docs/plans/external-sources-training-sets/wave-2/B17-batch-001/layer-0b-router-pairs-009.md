# Layer 0-B — Router Pairs (009)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router

---

## B17-0B-009-001 — Five predicates: [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST] → 5 decisions

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]
**ConnectorName:** Auth0OAuthConnector

**Routing decisions:**

| # | Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|---|
| 1 | IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| 2 | API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |
| 3 | SECURITY | 3.4 | Security and Compliance | SecurityAuditFlow34 |
| 4 | AUTHORIZATION | 3.4 | Security and Compliance | AuthorizationFlow34 |
| 5 | TEST | 5.1 | Unit and Integration Testing | TestingFlow51 |

**DecisionCount:** 5
**Unique sub-phases:** 4 (4.3, 3.3, 3.4, 5.1)
**TransitionType:** feature_addition

---

## B17-0B-009-002 — Five-decision routing output

```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" },
  { predicate: "SECURITY", subPhase: "3.4", flow: "SecurityAuditFlow34" },
  { predicate: "AUTHORIZATION", subPhase: "3.4", flow: "AuthorizationFlow34" },
  { predicate: "TEST", subPhase: "5.1", flow: "TestingFlow51" }
]
DecisionCount: 5
UniqueSubPhases: ["4.3", "3.3", "3.4", "5.1"]
TransitionType: feature_addition
```

---

## B17-0B-009-003 — Five predicates: DecisionCount = 5, unique sub-phases = 4

**Important distinction:**
- DecisionCount: 5 (one per predicate fired)
- Unique sub-phases: 4 (3.4 appears twice — for SECURITY and AUTHORIZATION)

Both counts are useful for different purposes:
- DecisionCount tracks orchestration breadth (how many predicate handlers activate)
- Unique sub-phases tracks phase coverage (how many distinct work zones activate)

**Neither replaces the other. Both are reported.**

---

## B17-0B-009-004 — Validation checklist for five-predicate routing

**Gate B: all five must be correct:**
1. IMPLEMENT → 4.3 (not 4.1, not 3.3)
2. API_CONTRACT → 3.3 (not 4.3)
3. SECURITY → 3.4 (SecurityAuditFlow34)
4. AUTHORIZATION → 3.4 (AuthorizationFlow34, separate from SECURITY)
5. TEST → 5.1 (not 4.3, not 5.3)

**Common errors in five-predicate routing:**
- Error A: IMPLEMENT → 4.1 (v6-03: ignores integration vocabulary)
- Error B: API_CONTRACT → 4.3 (wrong: API_CONTRACT alone never → 4.3)
- Error C: SECURITY + AUTHORIZATION counted as one decision (v6-02)
- Error D: TEST → 4.3 (wrong: TEST has dedicated 5.1)
