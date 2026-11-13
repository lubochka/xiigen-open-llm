# Layer 1 — Transition Orchestrator Pairs (036)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Complete orchestration reference for connectors 025-036

---

## B17-L1-036-001 — Complete orchestration pattern matrix for connectors 025-036

| Predicate set | Total flows | Steps | Parallel in Step 1 | QA gate |
|---|---|---|---|---|
| [IMPL, API] | 2 | 2 (parallel) | API contract parallel | No |
| [IMPL, API, SEC] | 3 | 3 | API contract parallel Step 1 | No |
| [IMPL, API, SEC, AUTH] | 4 | 3-4 | API or DOC parallel Step 1; SEC+AUTH parallel after | No |
| [IMPL, API, TEST] | 3 | 3 | None (TEST after IMPL) | No |
| [IMPL, API, DOC] | 3 | 2-3 | DOC parallel Step 1 | No |
| [IMPL, API, TEST, DOC] | 4 | 4 | DOC parallel Step 1; TEST after Step 1 | No |
| [IMPL, API, TEST, QA] | 4 | 4 | None | Yes (Step 4) |
| [IMPL, API, SEC, AUTH, TEST, DOC] | 6 | 6 | DOC parallel Step 1 | No |

---

## B17-L1-036-002 — TransitionType summary for orchestrator 025-036

| Input pattern | TransitionType |
|---|---|
| Build/implement/create + connector vocabulary | feature_addition |
| Refactor/restructure/rewrite + connector | refactoring |
| Integrate/wire up alone | documentation |
| Document alone | documentation |
| No triggering predicates | none |

---

## B17-L1-036-003 — Critical orchestration constraints (025-036)

1. TestingFlow51 (5.1) ALWAYS after IntegrationConnectorFlow43 (4.3) — hard sequential dependency
2. DocumentationFlow21 (2.1) CAN run in parallel with IntegrationConnectorFlow43 (spec-driven)
3. QAGatingFlow73 (7.x) ALWAYS last-absolute — starts only when ALL other flows complete
4. SecurityAuditFlow34 (3.4) and AuthorizationFlow34 (3.4) can run in parallel with each other after 4.3
5. API_CONTRACT alone (no IMPLEMENT) → no IntegrationConnectorFlow43, no 4.3 sub-phase
