# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: orchestrator

---

## B17-L1-004-001 — IMPLEMENT + API_CONTRACT + TEST sequential: 4.3 then 5.1

**Scenario:** SendGridEmailConnector with integration tests.

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  TestingFlow51 (5.1)
]
```

**Orchestration plan:**
```
Phase 1: IntegrationConnectorFlow43 (4.3)
  — Build SendGrid connector (MUST complete before tests can run)
Phase 2 (parallel):
  ‖ APIContractFlow33 (3.3) — API contract
  ‖ TestingFlow51 (5.1) — write integration tests
  Note: Tests can be written while implementation is reviewed, but tests
  run against the completed connector. Test writing can be concurrent with 3.3.
```

**Alternative strict ordering:**
```
Phase 1: IntegrationConnectorFlow43 (4.3) — build
Phase 2: APIContractFlow33 (3.3) — document (parallel option)
Phase 3: TestingFlow51 (5.1) — test (strictly after build)
```

---

## B17-L1-004-002 — Why TEST has a sequential dependency on IMPLEMENT (4.3)

**Dependency rule:** You cannot run integration tests for a connector that doesn't exist yet.

**TestingFlow51 depends on IntegrationConnectorFlow43 completing** in order to:
- Import the connector class in test files
- Call connector methods in test assertions
- Set up connector fixtures with real implementation

**Test writing vs test running:**
- Test writing (scaffolding) CAN be concurrent with implementation
- Test running (execution) MUST be after implementation

**The orchestrator encodes the dependency:** TestingFlow51 has `dependsOn: "IntegrationConnectorFlow43"`.

---

## B17-L1-004-003 — Sequential orchestration output

```
OrchestrationPlan: {
  sequence: [
    { step: 1, flows: ["IntegrationConnectorFlow43"], parallel: false },
    { step: 2, flows: ["APIContractFlow33", "TestingFlow51"], parallel: true,
      note: "test writing concurrent with contract, test execution deferred" }
  ],
  dependencies: [
    { flow: "TestingFlow51", dependsOn: "IntegrationConnectorFlow43" }
  ],
  totalSteps: 2,
  transitionType: "feature_addition"
}
```

---

## B17-L1-004-004 — Contrast: DOCUMENT vs TEST in parallel eligibility

**DocumentationFlow21 parallel eligibility:**
- Can fully run concurrent with IntegrationConnectorFlow43
- Documentation is spec-driven, not implementation-driven
- No dependency on 4.3 completing

**TestingFlow51 parallel eligibility:**
- Test writing: concurrent (scaffold from spec)
- Test execution: depends on 4.3 completing
- Partial parallelism only

**Rule of thumb:** DOCUMENT is more parallelizable than TEST in integration connector scenarios.
