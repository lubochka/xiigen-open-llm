# Layer 0-D — Flow Factory Pairs (053)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Vercel connector

---

## B17-0D-053-001 — Vercel: webhook factory (no token)

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

---

## B17-0D-053-002 — Vercel: bearer token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-053-003 — Vercel: HMAC + test factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- TestingFlow51(5.1)

---

## B17-0D-053-004 — Vercel anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for "deploy to Vercel" | "deploy" fires no IMPLEMENT |
| Inventing DeploymentPipelineFlow | No domain-specific flows in B17 |
| Selecting SecurityAuditFlow34 for "verify webhook events" alone | No HMAC phrase or token → SECURITY not compiled |
