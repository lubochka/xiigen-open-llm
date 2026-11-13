# Layer 0-D — Flow Factory Pairs (052)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Supabase connector

---

## B17-0D-052-001 — Supabase: anon key factory (no SECURITY)

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

---

## B17-0D-052-002 — Supabase: JWT factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

Note: SECURITY fires because JWT expansion "JSON Web Token" contains "Token".

---

## B17-0D-052-003 — Supabase: OAuth + token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-052-004 — Supabase anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for "Supabase anon key" | No "token" → SECURITY not compiled |
| Selecting SecurityAuditFlow34 for "RLS policy" | RLS policy is a database feature, not a SECURITY trigger |
| Inventing SupabaseEdgeFunctionFlow | No domain-specific flows in B17 |
