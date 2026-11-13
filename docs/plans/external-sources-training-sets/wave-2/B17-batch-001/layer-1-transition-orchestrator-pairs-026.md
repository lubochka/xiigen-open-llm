# Layer 1 — Transition Orchestrator Pairs (026)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: SendGrid email connector orchestration

---

## B17-L1-026-001 — SendGridEmailConnector orchestration: [IMPL, API]

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SendGridEmailConnector [parallel with APIContractFlow33]
Step 2: APIContractFlow33 (3.3) — finalize SendGrid email API contract [parallel with Step 1]
TransitionType: feature_addition
```

---

## B17-L1-026-002 — SendGrid orchestration: [IMPL, API, TEST, DOC]

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SendGridEmailConnector [parallel with Step 2]
Step 2: DocumentationFlow21 (2.1) — draft email API documentation [parallel with Step 1]
Step 3: APIContractFlow33 (3.3) — finalize API contract [after Step 1]
Step 4: TestingFlow51 (5.1) — integration tests for send, bounce, suppression [after Step 1; hard dependency]

DocumentationFlow21 runs in parallel with IntegrationConnectorFlow43 (spec-driven)
TestingFlow51 runs after IntegrationConnectorFlow43 (connector code must exist first)
```

---

## B17-L1-026-003 — SendGrid orchestration: refactor case

```
Step 1: IntegrationConnectorFlow43 (4.3) — refactor SendGridEmailConnector to v3 API
Step 2: APIContractFlow33 (3.3) — update API contract [after Step 1]
TransitionType: refactoring
```

TransitionType=refactoring. N08 for refactoring checks backward compatibility of refactored connector.

---

## B17-L1-026-004 — SendGrid: DocumentationFlow21 parallelism rule

DocumentationFlow21 is spec-driven. It does NOT require connector code to exist.
It CAN run in parallel with IntegrationConnectorFlow43 (Step 1).
This is the exception to the sequential dependency rule — only TestingFlow51 has the hard sequential dependency.
