# Layer 0-B — Router Pairs (011)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router | focus: v6 corruption routing

---

## B17-0B-011-001 — v6-03 corruption: IMPLEMENT wrongly routes to 4.1 instead of 4.3

**Input:** "Implement the Stripe payment connector for the checkout service."
**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]

**CORRUPTED routing (v6-03):**
```
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.1", flow: "BackendServiceFlow41" },
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" }
]
```
Error: IMPLEMENT sent to 4.1 despite "connector" (integration vocabulary) being present.

**CORRECT routing:**
```
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" }
]
```

**Correction:** "connector" is integration-context vocabulary. Rule 3: IMPLEMENT routes to 4.3 when integration vocabulary co-occurs. "connector" always triggers 4.3 routing for IMPLEMENT.

---

## B17-0B-011-002 — v6-01 corruption: "integrate" generates IMPLEMENT in routing

**Input:** "Integrate the Slack API for notifications."
**CORRUPTED routing (v6-01):**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" }
]
```
Error: IMPLEMENT was invented from "integrate". Router was given a corrupted predicate set.

**CORRECT routing:**
```
CompiledPredicateIds: ["API_CONTRACT"]
Routing: [
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" }
]
DecisionCount: 1
TransitionType: documentation
```

**Note:** The v6-01 corruption happens at the compiler level. If the compiler is correct, the router receives ["API_CONTRACT"] and routes correctly to 3.3. If the compiler is corrupted, the router receives ["IMPLEMENT", "API_CONTRACT"] and generates a wrong 4.3 routing.

---

## B17-0B-011-003 — v6-02 corruption: SECURITY suppressed in routing

**Input:** "Build the Auth0 OAuth connector and implement token refresh."
**CORRUPTED routing (v6-02 — SECURITY suppressed):**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"]
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3" },
  { predicate: "API_CONTRACT", subPhase: "3.3" },
  { predicate: "AUTHORIZATION", subPhase: "3.4" }
]
DecisionCount: 3
```

**CORRECT routing:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3" },
  { predicate: "API_CONTRACT", subPhase: "3.3" },
  { predicate: "SECURITY", subPhase: "3.4", flow: "SecurityAuditFlow34" },
  { predicate: "AUTHORIZATION", subPhase: "3.4", flow: "AuthorizationFlow34" }
]
DecisionCount: 4
```

**Error:** "token" was present → SECURITY must fire. Suppressing SECURITY reduces DecisionCount from 4 to 3 and omits SecurityAuditFlow34 activation.
