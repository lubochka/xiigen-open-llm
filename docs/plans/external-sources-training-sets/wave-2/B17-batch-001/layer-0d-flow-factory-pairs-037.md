# Layer 0-D — Flow Factory Pairs (037)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Jira connector

---

## B17-0D-037-001 — Jira: factory selection for [IMPL, API, SEC]

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IMPLEMENT + connector vocabulary → 4.3 → IntegrationConnectorFlow43 [deterministic_generic]
- API_CONTRACT → 3.3 → APIContractFlow33 [deterministic_generic]
- SECURITY → 3.4 → SecurityAuditFlow34 [deterministic_generic]

SelectedFlows: [IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)]

---

## B17-0D-037-002 — Jira: "integrate" factory output

**Predicates:** ["API_CONTRACT"]

Factory output: APIContractFlow33(3.3) only.
IntegrationConnectorFlow43 NOT selected — IMPLEMENT absent.

---

## B17-0D-037-003 — Jira: OAuth + token factory selection

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

Note: Both SecurityAuditFlow34 and AuthorizationFlow34 are selected at 3.4 — two flows, one sub-phase.

---

## B17-0D-037-004 — Jira anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for "integrate Jira" | "integrate" → API_CONTRACT only → APIContractFlow33; no 4.3 |
| Selecting AuthorizationFlow34 for Jira OAuth 1.0a only | OAuth 1.0a still contains "OAuth" → AUTHORIZATION fires |
| Selecting BackendServiceFlow41 when "connector" is present | "connector" vocabulary → always 4.3, not 4.1 |
