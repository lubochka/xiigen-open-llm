# Layer 0-D — Flow Factory Pairs (055)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Cloudflare connector

---

## B17-0D-055-001 — Cloudflare: API token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-055-002 — Cloudflare: OAuth + token + document factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "DOCUMENT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)
- DocumentationFlow21(2.1)

---

## B17-0D-055-003 — Cloudflare: "integrate" factory

**Predicates:** ["API_CONTRACT"]

Factory output: APIContractFlow33(3.3) only.

---

## B17-0D-055-004 — Cloudflare anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for "Cloudflare API key" | API key is not a token; SECURITY not compiled |
| Inventing EdgeWorkerFlow | No domain-specific flows in B17 |
| Selecting 4.1 for "build Cloudflare Workers connector" | connector vocabulary → 4.3 |
