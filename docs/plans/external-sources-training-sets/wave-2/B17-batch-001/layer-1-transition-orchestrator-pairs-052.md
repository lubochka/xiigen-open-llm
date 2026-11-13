# Layer 1 — Transition Orchestrator Pairs (052)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Supabase connector

---

## B17-L1-052-001 — Supabase: orchestration for [IMPL, API]

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SupabaseConnector
Step 2: APIContractFlow33 (3.3) — Supabase REST API contract [after Step 1]
```

---

## B17-L1-052-002 — Supabase: JWT orchestration

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SupabaseConnector
Step 2: APIContractFlow33 (3.3) — API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — JWT token audit [after Step 1]
```

---

## B17-L1-052-003 — Supabase: OAuth + token orchestration

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4), AuthorizationFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build SupabaseConnector
Step 2: APIContractFlow33 (3.3)
Step 3: SecurityAuditFlow34 (3.4) [parallel with Step 4]
Step 4: AuthorizationFlow34 (3.4) [parallel with Step 3]
```

---

## B17-L1-052-004 — Supabase: "integrate" orchestration

**Flows:** APIContractFlow33(3.3)

```
Step 1: APIContractFlow33 (3.3) — document Supabase Realtime API contract
```

TransitionType: documentation
