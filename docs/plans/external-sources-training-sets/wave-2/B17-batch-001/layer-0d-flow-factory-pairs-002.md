# Layer 0-D — Flow Factory Pairs (002)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-002-001 — probationary_accepted: 4.3 candidate passes Gate B

**Scenario:** PayPalConnector candidate flow submitted to factory.

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
CandidateFlow: IntegrationConnectorFlow43
CandidateStatus: probationary_accepted
ConnectorName: PayPalConnector
```

**Factory selection logic:**
1. Check CandidateStatus → probationary_accepted (Gate B passed)
2. probationary_accepted → eligible to replace deterministic_generic
3. Select IntegrationConnectorFlow43 as primary flow for 4.3
4. Select APIContractFlow33 as primary flow for 3.3

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "probationary_accepted" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: feature_addition
```

---

## B17-0D-002-002 — probationary_accepted: what Gate B passage means

**Gate B** is the exact passing condition for a candidate flow. For IntegrationConnectorFlow43, Gate B requires:
- Nine-node flow completes without error
- N08 (verdict node) produces PASS
- Generated connector file count meets expected minimum
- All connector interface methods implemented

When Gate B passes → CandidateStatus: probationary_accepted
When Gate B fails → CandidateStatus: probationary_rejected → fallback to deterministic_generic

---

## B17-0D-002-003 — Factory metadata for probationary_accepted

**Factory record for PayPalConnector:**
```
{
  "factoryId": "IntegrationConnectorFlow43-PayPal",
  "candidateStatus": "probationary_accepted",
  "gateB": "PASS",
  "evidenceThreshold": "pending (not yet trusted_learned)",
  "replacesGeneric": true,
  "connectorName": "PayPalConnector",
  "subPhase": "4.3"
}
```

**State:** probationary_accepted is a transitional state. After sufficient evidence (passing Gate B across multiple instances), the status promotes to trusted_learned.

---

## B17-0D-002-004 — Factory selection decision tree (probationary path)

```
Input predicates arrive at factory
  ↓
Any predicate routes to 4.3? → Yes
  ↓
Is there a candidate flow for 4.3?
  ├─ Yes → What is CandidateStatus?
  │   ├─ probationary_accepted → USE candidate flow ✓
  │   ├─ probationary_rejected → FALLBACK to deterministic_generic
  │   └─ trusted_learned → USE candidate flow ✓
  └─ No → USE deterministic_generic
```
