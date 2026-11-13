# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: orchestrator

---

## B17-L1-008-001 — IMPLEMENT + API_CONTRACT + DOCUMENT + QA: QA last (absolute)

**Scenario:** DocuSignConnector — build, document, and QA.

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  DocumentationFlow21 (2.1),
  QAGatingFlow73 (7.x)
]
```

**Orchestration plan:**
```
Phase 1 (parallel):
  ‖ IntegrationConnectorFlow43 (4.3) — build connector
  ‖ DocumentationFlow21 (2.1) — write documentation
Phase 2:
  APIContractFlow33 (3.3) — finalize API contract
Phase 3 (absolute last):
  QAGatingFlow73 (7.x) — QA gate: validate all outputs
```

---

## B17-L1-008-002 — QA absolute last rule: no exceptions

**Rule 21 at orchestrator level:** QAGatingFlow73 is always the final step. No flow runs after QA.

**This is an absolute constraint, not a preference:**
- If more flows are added later (e.g., TEST added to this scenario), QA still runs last
- QA validates all completed outputs from all preceding flows
- QA cannot validate what hasn't been produced yet — it must be last

**Orchestration violation example:** Running QAGatingFlow73 before DocumentationFlow21 completes would produce incomplete QA results. This is a v6 ordering violation.

---

## B17-L1-008-003 — Orchestration output with QA

```
OrchestrationPlan: {
  sequence: [
    { step: 1, flows: ["IntegrationConnectorFlow43", "DocumentationFlow21"], parallel: true },
    { step: 2, flows: ["APIContractFlow33"], parallel: false },
    { step: 3, flows: ["QAGatingFlow73"], parallel: false, position: "last-absolute" }
  ],
  qaGating: true,
  qaMustBeLastStep: true,
  transitionType: "feature_addition"
}
```

---

## B17-L1-008-004 — QA gating scope in DocuSign context

**QAGatingFlow73 validates:**
1. IntegrationConnectorFlow43 output: DocuSign connector files complete and correct
2. DocumentationFlow21 output: documentation covers all connector methods
3. APIContractFlow33 output: API contract matches connector implementation
4. End-to-end: signing workflow can be initiated, signed, and completed via connector

**QA produces a verdict:** PASS (all validations complete) or FAIL (one or more validations fail)

**If QA fails:** The orchestration does not produce a final commit. The failing flows are identified and re-run. QA gates the entire pipeline.
