# Layer 0-D — Flow Factory Pairs (006)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-006-001 — IMPLEMENT + API_CONTRACT + DOCUMENT factory: 4.3 + 3.3 + 2.1

**Scenario:** PlaidBankingConnector with interface documentation.

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
ConnectorName: PlaidBankingConnector
TransitionType: feature_addition
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { subPhase: "3.3", flow: "APIContractFlow33" },
  { subPhase: "2.1", flow: "DocumentationFlow21" }
]
DecisionCount: 3
```

---

## B17-0D-006-002 — DocumentationFlow21 characteristics

**DocumentationFlow21:**
- Handles: interface documentation, API reference, integration guides
- Typical output: markdown documentation, API reference docs, sequence diagrams
- Sub-phase 2.1 name: Documentation and Specification
- Triggered by: DOCUMENT predicate

**Documentation in connector context typically includes:**
- Account linking flow documentation
- Transaction retrieval API reference
- Error code reference
- Integration quickstart guide

---

## B17-0D-006-003 — 4.3 and 2.1 can run in parallel

**Execution hint for IMPLEMENT + DOCUMENT combination:**
```
ExecutionHint: {
  "4.3": { order: 1, canParallel: true },
  "2.1": { order: 1, canParallel: true },  // parallel with 4.3
  "3.3": { order: 2, canParallel: false }
}
```

**Implementation (4.3) and documentation (2.1) can proceed in parallel** because documentation of the interface specification doesn't require the implementation to be complete. The interface can be documented from the API contract.

**Note:** This is an execution hint, not a routing rule. The orchestrator makes the final ordering decision.

---

## B17-0D-006-004 — Factory for DOCUMENT: DocumentationFlow21 always

**DocumentationFlow21 is the invariant factory selection for DOCUMENT:**
- DOCUMENT in any predicate combination → DocumentationFlow21 selected for 2.1
- No scenario-specific variant for DOCUMENT in B17
- Status: deterministic_generic (no candidate flow for documentation — it's a generic document generator)

```
DOCUMENT → DocumentationFlow21 (2.1) [deterministic_generic]
```

Contrast with IMPLEMENT → IntegrationConnectorFlow43 which has candidate flows (probationary_accepted / trusted_learned).
