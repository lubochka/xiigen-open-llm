# Layer 0-D — Flow Factory Pairs (057)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: GCS connector

---

## B17-0D-057-001 — GCS: signed URL factory (no SECURITY)

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

Note: "signed URL" alone does not compile SECURITY; SecurityAuditFlow34 NOT selected.

---

## B17-0D-057-002 — GCS: service account token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-057-003 — GCS: OAuth + token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-057-004 — GCS anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for "signed URL" | Signed URL is not a token |
| Selecting SecurityAuditFlow34 for "HMAC key" alone | HMAC key (noun) is not the HMAC validation phrase (Rule 8) |
| Inventing CloudStorageFlow | No domain-specific flows in B17 |
