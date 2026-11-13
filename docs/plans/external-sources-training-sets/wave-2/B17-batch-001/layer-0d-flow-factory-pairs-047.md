# Layer 0-D — Flow Factory Pairs (047)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: PostgreSQL connector

---

## B17-0D-047-001 — PostgreSQL: basic factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

---

## B17-0D-047-002 — PostgreSQL: service account token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-047-003 — PostgreSQL: "integrate" factory

**Predicates:** ["API_CONTRACT"]

Factory output: APIContractFlow33(3.3) only.

---

## B17-0D-047-004 — PostgreSQL: API_CONTRACT + SECURITY factory teaching point

Input: "Implement the PostgreSQL connector; connector exposes an API endpoint and uses a bearer token for row-level security."

Factory reasoning:
1. IMPLEMENT + connector vocabulary → 4.3 → IntegrationConnectorFlow43
2. API_CONTRACT (from "connector", Rule 4; "API endpoint" reinforces but fires once) → 3.3 → APIContractFlow33
3. SECURITY (from "bearer token", Rule 6) → 3.4 → SecurityAuditFlow34

SelectedFlows: [IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)]
Teaching: API_CONTRACT and SECURITY fire from independent triggers (connector vs bearer token) — both produce distinct factory selections.
