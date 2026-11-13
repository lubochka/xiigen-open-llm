# Layer 0-D — Flow Factory Pairs (056)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: AWS S3 connector

---

## B17-0D-056-001 — AWS S3: basic factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

---

## B17-0D-056-002 — AWS S3: access token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-056-003 — AWS S3: STS token + OAuth factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-056-004 — AWS S3 anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for "presigned URL" alone | Presigned URL is not a token; SECURITY not compiled |
| Selecting SecurityAuditFlow34 for "IAM role" alone | IAM role is not a token; SECURITY not compiled |
| Inventing S3BucketFlow | No domain-specific flows in B17 |
