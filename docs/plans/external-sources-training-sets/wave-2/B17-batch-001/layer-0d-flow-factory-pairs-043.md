# Layer 0-D — Flow Factory Pairs (043)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Redis connector

---

## B17-0D-043-001 — Redis: no token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

---

## B17-0D-043-002 — Redis: ACL token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-043-003 — Redis: "integrate" factory

**Predicates:** ["API_CONTRACT"]

Factory output: APIContractFlow33(3.3) only.

---

## B17-0D-043-004 — Redis anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for "Redis TLS connection" | TLS is not a SECURITY trigger; SecurityAuditFlow34 not selected |
| Inventing CacheFlow for Redis | No domain-specific flow; always deterministic_generic |
| Selecting 4.1 for "build Redis connector" | "connector" vocabulary routes IMPLEMENT to 4.3 |
