# Layer 1 — Transition Orchestrator Pairs (034)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Salesforce CRM connector orchestration

---

## B17-L1-034-001 — SalesforceConnector orchestration: [IMPL, API, SEC, AUTH]

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SalesforceConnector [parallel with Step 2]
Step 2: DocumentationFlow21 (2.1) — absent (no DOCUMENT predicate)
Step 2: APIContractFlow33 (3.3) — finalize Salesforce REST API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — OAuth connected app token security review [after Step 1, parallel with Step 4]
Step 4: AuthorizationFlow34 (3.4) — OAuth 2.0 flow review [after Step 1, parallel with Step 3]
TransitionType: feature_addition
```

---

## B17-L1-034-002 — Salesforce orchestration: [IMPL] to 4.1

```
Step 1: BackendServiceFlow41 (4.1) — implement Salesforce lead scoring algorithm
TransitionType: feature_addition
```

No integration vocabulary → no APIContractFlow33 selected. Single-step orchestration.

---

## B17-L1-034-003 — Salesforce orchestration: [IMPL, API, TEST]

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SalesforceConnector
Step 2: APIContractFlow33 (3.3) — finalize API contract [after Step 1]
Step 3: TestingFlow51 (5.1) — opportunity sync endpoint tests [after Step 1; hard sequential dependency]
TransitionType: feature_addition
```

---

## B17-L1-034-004 — Salesforce: full 6-predicate orchestration with DOCUMENT + TEST

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SalesforceConnector [parallel with Step 2]
Step 2: DocumentationFlow21 (2.1) — draft Salesforce API documentation [parallel with Step 1]
Step 3: APIContractFlow33 (3.3) — finalize API contract [after Step 1]
Step 4: SecurityAuditFlow34 (3.4) — token security review [after Step 1, parallel with Step 5]
Step 5: AuthorizationFlow34 (3.4) — OAuth review [after Step 1, parallel with Step 4]
Step 6: TestingFlow51 (5.1) — integration tests [after Step 1; hard sequential dependency]
DecisionCount: 6 | UniqueSubPhases: 5
```
