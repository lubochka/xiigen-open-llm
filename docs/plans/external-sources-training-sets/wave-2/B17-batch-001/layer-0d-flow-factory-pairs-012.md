# Layer 0-D — Flow Factory Pairs (012)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-012-001 — Complete B17 factory map table (all predicate combos → factory → TransitionType)

| CompiledPredicateIds | Selected Flows | DecisionCount | TransitionType |
|---|---|---|---|
| [API_CONTRACT] | APIContractFlow33(3.3) | 1 | documentation |
| [IMPLEMENT, API_CONTRACT] | IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3) | 2 | feature_addition |
| [IMPLEMENT, API_CONTRACT] + refactor | IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3) | 2 | refactoring |
| [IMPLEMENT, API_CONTRACT, SECURITY] | +SecurityAuditFlow34(3.4) | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, AUTHORIZATION] | +AuthorizationFlow34(3.4) | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] | +SecurityAuditFlow34(3.4), AuthorizationFlow34(3.4) | 4 | feature_addition |
| [IMPLEMENT, API_CONTRACT, TEST] | +TestingFlow51(5.1) | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | +DocumentationFlow21(2.1) | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, QA] | +DocumentationFlow21(2.1), QAGatingFlow73(7.x) | 4 | feature_addition |
| [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST] | all 5 flows | 5 | feature_addition |
| [QA] | QAGatingFlow73(7.x) | 1 | no contribution |
| [TEST] | TestingFlow51(5.1) | 1 | no contribution |
| [DOCUMENT] | DocumentationFlow21(2.1) | 1 | documentation |

---

## B17-0D-012-002 — Flow ID reference table

| FlowId | Sub-phase | Status in B17 |
|---|---|---|
| IntegrationConnectorFlow43 | 4.3 | probationary_accepted / trusted_learned |
| APIContractFlow33 | 3.3 | deterministic_generic |
| SecurityAuditFlow34 | 3.4 | deterministic_generic |
| AuthorizationFlow34 | 3.4 | deterministic_generic |
| TestingFlow51 | 5.1 | deterministic_generic |
| DocumentationFlow21 | 2.1 | deterministic_generic |
| QAGatingFlow73 | 7.x | deterministic_generic |
| BackendServiceFlow41 | 4.1 | NOT selected in B17 integration context |

---

## B17-0D-012-003 — Factory invariants for B17

1. IntegrationConnectorFlow43 is ONLY selected when IMPLEMENT is in CompiledPredicateIds AND integration vocabulary is present
2. APIContractFlow33 is selected whenever API_CONTRACT is in CompiledPredicateIds (always)
3. SecurityAuditFlow34 and AuthorizationFlow34 are BOTH selected when BOTH SECURITY and AUTHORIZATION fire
4. QAGatingFlow73 always carries position: "last" metadata
5. BackendServiceFlow41 is NEVER selected in B17 when integration vocabulary is present

---

## B17-0D-012-004 — probationary_accepted vs trusted_learned vs deterministic_generic

| Status | Meaning | Gate B required per invocation |
|---|---|---|
| deterministic_generic | Standard pre-approved flow, no learning component | No |
| probationary_accepted | Candidate flow, Gate B passed once, eligible to use | Yes (per invocation) |
| trusted_learned | Candidate flow, Gate B passed evidence-threshold times | No (pre-validated) |
| probationary_rejected | Candidate flow failed Gate B → fallback used | N/A (fallback active) |
