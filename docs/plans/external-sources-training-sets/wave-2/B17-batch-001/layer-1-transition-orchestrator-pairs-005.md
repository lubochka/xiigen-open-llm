# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: orchestrator

---

## B17-L1-005-001 — IMPLEMENT + API_CONTRACT + DOCUMENT parallel: 4.3 ‖ 2.1

**Scenario:** PlaidBankingConnector with documentation.

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  DocumentationFlow21 (2.1)
]
```

**Orchestration plan:**
```
Phase 1 (parallel):
  ‖ IntegrationConnectorFlow43 (4.3) — build Plaid connector
  ‖ DocumentationFlow21 (2.1) — document interface specification
Phase 2:
  APIContractFlow33 (3.3) — finalize API contract (after implementation informs contract)
```

---

## B17-L1-005-002 — Why 4.3 and 2.1 can run in parallel

**Documentation (2.1) is specification-driven, not implementation-driven:**
- The account linking and transaction retrieval interfaces are defined in the Plaid API specification
- Documentation can be written from the spec before the code is complete
- DocumentationFlow21 does not import or depend on the connector code

**Contrast with TestingFlow51:**
- Tests reference the connector class directly (code dependency)
- Documents reference the API specification (spec dependency, no code dependency)

**Therefore:** 4.3 ‖ 2.1 is a valid parallel execution, but 4.3 → 5.1 is sequential.

---

## B17-L1-005-003 — Parallel orchestration output

```
OrchestrationPlan: {
  sequence: [
    {
      step: 1,
      flows: ["IntegrationConnectorFlow43", "DocumentationFlow21"],
      parallel: true
    },
    {
      step: 2,
      flows: ["APIContractFlow33"],
      parallel: false
    }
  ],
  dependencies: [],  // No dependencies for step 1 flows
  totalSteps: 2,
  transitionType: "feature_addition"
}
```

---

## B17-L1-005-004 — Practical benefit of 4.3 ‖ 2.1 parallelism

**Without parallelism:**
- Step 1: Build connector (N hours)
- Step 2: Write documentation (M hours)
- Step 3: Finalize contract (P hours)
- Total: N + M + P hours

**With 4.3 ‖ 2.1 parallelism:**
- Step 1: Build connector AND write documentation simultaneously (max(N, M) hours)
- Step 2: Finalize contract (P hours)
- Total: max(N, M) + P hours

**Time saved:** min(N, M) hours — whichever of build or documentation is shorter is "free" in the parallel model.
