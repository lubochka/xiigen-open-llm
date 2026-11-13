# Layer 1 — Transition Orchestrator Pairs (060)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Complete orchestration reference for connectors 049-060

---

## B17-L1-060-001 — Complete orchestration pattern matrix for connectors 049-060

| Predicate set | Total flows | Step count | Parallel opportunities | QA gate |
|---|---|---|---|---|
| [IMPL, API] | 2 | 2 | None | No |
| [IMPL, API, SEC] | 3 | 3 | Steps 2+3 after Step 1 | No |
| [IMPL, API, SEC, AUTH] | 4 | 4 | Steps 3+4 parallel after Step 1 | No |
| [IMPL, API, SEC, AUTH, DOC] | 5 | 5 | DOC parallel Step 1; SEC+AUTH parallel | No |
| [IMPL, API, SEC, AUTH, TEST, DOC] | 6 | 6 | DOC parallel Step 1; SEC+AUTH parallel | No |
| [IMPL, API, SEC, AUTH, TEST, DOC, QA] | 7 | 7 | DOC parallel Step 1; SEC+AUTH parallel; QA last | Yes |

---

## B17-L1-060-002 — TransitionType summary for 049-060

| Input pattern | TransitionType |
|---|---|
| build/implement/create + connector vocabulary | feature_addition |
| refactor/restructure/rewrite + connector | refactoring |
| integrate/wire up alone | documentation |
| document alone | documentation |
| no triggering predicates | none |

---

## B17-L1-060-003 — Critical orchestration constraints (049-060)

1. TestingFlow51 (5.1) ALWAYS after IntegrationConnectorFlow43 (4.3) — hard sequential dependency
2. DocumentationFlow21 (2.1) CAN run parallel with IntegrationConnectorFlow43 — spec-driven
3. QAGatingFlow73 (7.x) ALWAYS last-absolute — starts only when ALL other flows complete
4. SecurityAuditFlow34 and AuthorizationFlow34 can run in parallel with each other after 4.3
5. API_CONTRACT alone (no IMPLEMENT) → single-step orchestration; no IntegrationConnectorFlow43
6. OIDC/SAML/PKCE/RBAC do NOT add AuthorizationFlow34 — only OAuth triggers it
