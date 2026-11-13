# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B12-L1-018-001 — ReportingAPI: documentation, two flows

```
source_id: xiigen-b12-l1-018-001
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Define the OpenAPI spec for the reporting API."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    ExecutionPlan:
      Phase1: APIContractFlow33 → reporting API OpenAPI contract
      Phase2: DocumentationFlow21 → reporting API documentation
    QACheckpoint: after Phase2

---

## B12-L1-018-002 — ReportingAPI: DOCUMENT only orchestration (no API terms)

```
source_id: xiigen-b12-l1-018-002
transition_type: documentation
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  TransitionType: "documentation"
  user_request: "Document the reporting metrics and KPI definitions for the dashboard."

B:
  OrchestratorPlan:
    TransitionType: "documentation"
    TotalFlows: 1
    ExecutionPlan:
      Phase1: DocumentationFlow21 → reporting metrics documentation
    QACheckpoint: after Phase1

Note: No APIContractFlow33 → no 3.3 sub-phase execution.
Single-flow orchestration for single-predicate [DOCUMENT] case.

---

## B12-L1-018-003 — ReportingAPI IMPLEMENT + SECURITY: four flows

```
source_id: xiigen-b12-l1-018-003
transition_type: feature_addition
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  TransitionType: "feature_addition"
  user_request: "Scaffold the reporting API and specify the authenticated endpoint contract."

B:
  OrchestratorPlan:
    TransitionType: "feature_addition"
    TotalFlows: 4
    ExecutionPlan:
      Phase1_Parallel:
        - ImplementationFlow43 → reporting API controller scaffold
        - APIContractFlow33 → reporting endpoint contract spec
      Phase2_Parallel:
        - DocumentationFlow21 → reporting API documentation
        - SecurityAuditFlow34 → authentication security review
    QACheckpoint: after Phase2

---

## B12-L1-018-004 — ReportingAPI: orchestrator QA position rule

```
source_id: xiigen-b12-l1-018-004
pair_kind: transition_orchestrator
```

QA checkpoint placement rule for B12:

QA is ALWAYS placed LAST in the execution plan, after ALL flows complete.

For any number of flows:
  [APIContractFlow33, DocumentationFlow21] → QA after both
  [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34] → QA after all three
  [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34] → QA after all four

v6 ordering violation: placing QA between flows → CORRUPT
  Example CORRUPT: QA → DocumentationFlow21 → SecurityAuditFlow34
  Correct: DocumentationFlow21 → SecurityAuditFlow34 → QA

Documentation-before-security is also a v6 ordering violation:
  CORRUPT: APIContractFlow33 → SecurityAuditFlow34 → DocumentationFlow21 → QA
  Correct: APIContractFlow33 → parallel(DocumentationFlow21, SecurityAuditFlow34) → QA
