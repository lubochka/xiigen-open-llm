# Layer 0-D — Flow Factory Pairs (005)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-005-001 — IMPLEMENT + API_CONTRACT + TEST factory: 4.3 + 3.3 + 5.1

**Scenario:** SendGridEmailConnector with integration tests.

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
ConnectorName: SendGridEmailConnector
TransitionType: feature_addition
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { subPhase: "3.3", flow: "APIContractFlow33" },
  { subPhase: "5.1", flow: "TestingFlow51" }
]
DecisionCount: 3
```

---

## B17-0D-005-002 — TestingFlow51 characteristics

**TestingFlow51:**
- Handles: unit tests, integration tests for connectors
- Typical output: test files, test fixture setup, mock/stub configurations
- Sub-phase 5.1 name: Unit and Integration Testing
- Triggered by: TEST predicate

**Testing in connector context typically includes:**
- Mock external API responses (Stripe sandbox, Twilio test credentials)
- Test webhook signature validation
- Test error handling (API rate limits, timeouts, authentication failures)
- Integration test against sandbox/staging environment

---

## B17-0D-005-003 — Factory for three predicates: independent selection

**Each predicate selects its flow independently:**
```
IMPLEMENT → IntegrationConnectorFlow43 (4.3) [independent]
API_CONTRACT → APIContractFlow33 (3.3) [independent]
TEST → TestingFlow51 (5.1) [independent]
```

**No predicate blocks or overrides another predicate's flow selection.**

**Exception pattern:** If a predicate has no registered flow for the current scenario, factory falls back to deterministic_generic for that predicate only, while other predicates use their registered flows.

---

## B17-0D-005-004 — Ordering note: tests run after implementation

**Orchestration note (factory output metadata):**
```
ExecutionHint: {
  "4.3": { order: 1, canParallel: false },
  "3.3": { order: 2, canParallel: true },
  "5.1": { order: 3, canParallel: false, dependsOn: "4.3" }
}
```

TEST (5.1) depends on IMPLEMENT (4.3) completing. You cannot run integration tests before the connector is built. This dependency is expressed as execution ordering metadata, not routing metadata.

**The factory selects flows; the orchestrator orders them.**
