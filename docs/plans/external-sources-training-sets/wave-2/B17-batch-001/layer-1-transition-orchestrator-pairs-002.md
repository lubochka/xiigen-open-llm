# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: orchestrator

---

## B17-L1-002-001 — Four sub-phases: IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION

**Scenario:** OAuthIdentityConnector (Auth0) — four predicates, four orchestrated flows.

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  SecurityAuditFlow34 (3.4),
  AuthorizationFlow34 (3.4)
]
TransitionType: feature_addition
```

**Orchestration plan:**
```
Phase 1: IntegrationConnectorFlow43 (4.3)
  — Build the Auth0 connector (prerequisite for all other flows)
Phase 2 (parallel):
  ‖ SecurityAuditFlow34 (3.4) — security review
  ‖ AuthorizationFlow34 (3.4) — OAuth protocol design
  ‖ APIContractFlow33 (3.3) — API contract specification
Phase 3: [none — QA absent]
```

---

## B17-L1-002-002 — Orchestration ordering rationale

**Why 4.3 runs first:**
IntegrationConnectorFlow43 builds the connector. The security and authorization reviews and API contract work can reference the connector interface but don't depend on it being complete.

**Why 3.4 and 3.3 can be parallel:**
SecurityAuditFlow34 and AuthorizationFlow34 work on different aspects (security checklist vs OAuth protocol). APIContractFlow33 documents the interface. All three can proceed concurrently.

**No test flow present** → no sequential dependency after Phase 2.

---

## B17-L1-002-003 — Orchestration output structure

```
OrchestrationPlan: {
  sequence: [
    { step: 1, flows: ["IntegrationConnectorFlow43"], parallel: false },
    { step: 2, flows: ["SecurityAuditFlow34", "AuthorizationFlow34", "APIContractFlow33"], parallel: true },
  ],
  totalSteps: 2,
  transitionType: "feature_addition",
  qaGating: false
}
```

---

## B17-L1-002-004 — Four-sub-phase orchestration: key timing properties

**Critical path:** IntegrationConnectorFlow43 → [parallel batch] → done
**Minimum serial steps:** 2
**Parallelization benefit:** Phase 2 runs three flows simultaneously rather than sequentially

**QA absent:** No QAGatingFlow73 in this scenario → orchestration ends after Phase 2. If QA were added, it would be Phase 3 (absolute last).
