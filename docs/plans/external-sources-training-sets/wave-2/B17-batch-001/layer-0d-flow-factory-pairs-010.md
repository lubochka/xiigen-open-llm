# Layer 0-D — Flow Factory Pairs (010)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-010-001 — v6 factory corruption: wrong sub-phase selection (4.1 instead of 4.3)

**Scenario:** StripeConnector — v6-03 corruption at factory level.

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
ConnectorName: StripeConnector
```

**CORRUPTED factory output (v6-03):**
```
SelectedFlows: [
  { subPhase: "4.1", flow: "BackendServiceFlow41" },  // WRONG
  { subPhase: "3.3", flow: "APIContractFlow33" }
]
```
Error: Factory selected BackendServiceFlow41 (4.1) instead of IntegrationConnectorFlow43 (4.3).

**CORRECT factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43" },  // CORRECT
  { subPhase: "3.3", flow: "APIContractFlow33" }
]
```

---

## B17-0D-010-002 — Root cause of v6-03 factory corruption

**Cause:** The factory routing table was not updated to include integration vocabulary as a 4.3 signal. When "connector" appears, the factory looked up IMPLEMENT and found only the default 4.1 mapping.

**Fix:** The factory routing table must include:
```
IMPLEMENT + {connector, webhook, API, integration, gateway, SDK} → 4.3
IMPLEMENT + {no integration vocabulary} → 4.1
```

The context-aware routing must be applied at the factory level, not just at the compiler level.

---

## B17-0D-010-003 — Impact of v6-03 on downstream execution

**When factory selects 4.1 instead of 4.3:**
- Nine-node flow runs BackendServiceFlow41 instead of IntegrationConnectorFlow43
- N02 (domain statement) uses generic service context, not connector context
- N06 (implementation plan) generates service class template, not connector class template
- N09 (output) produces a service file, not a connector file
- Result: wrong file type generated, wrong interface implemented

**The error propagates through all nine nodes.** Factory selection error is a high-impact corruption.

---

## B17-0D-010-004 — Factory Gate B test for v6-03 prevention

**Gate B check for factory:**
- Input: CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"], ConnectorName: StripeConnector
- Expected: SelectedFlows contains IntegrationConnectorFlow43 at subPhase 4.3
- NOT expected: BackendServiceFlow41 at subPhase 4.1

**Assertion:**
```
SelectedFlows.Any(f => f.subPhase == "4.3" && f.flow == "IntegrationConnectorFlow43") == true
SelectedFlows.Any(f => f.subPhase == "4.1") == false  // 4.1 must NOT appear
```
