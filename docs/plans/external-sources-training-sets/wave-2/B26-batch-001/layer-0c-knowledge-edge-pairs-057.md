# Layer 0-C — Knowledge Edge Pairs (057) — CloudSecurityPosture Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E057-001 — Cloud security vocabulary

"public exposure vulnerabilities" → SECURITY (vulnerability vocabulary)
"IAM role permissions" in audit → SECURITY + AUTHORIZATION (dual)
"security group misconfiguration" → SECURITY (misconfiguration in security context)
"cloud cost optimization" → gap (financial/operational context)

---

## E057-002 — "review" with non-security object is gap

"review cloud costs" → gap (cost context)
"review cloud architecture" → gap (architecture review without security vocabulary)
"review cloud security group misconfigurations" → SECURITY (review + security + misconfiguration)
Teaching: "review" fires SECURITY only when the object contains security-action vocabulary.

---

## E057-003 — KMS key rotation vocabulary

"KMS key rotation schedules" → SECURITY (key rotation = crypto group)
"IAM key age compliance" → SECURITY + AUTHORIZATION (IAM = authorization + key = crypto)
"key rotation" alone → SECURITY (solo noun in cryptographic group)

---

## E057-004 — S3 bucket exposure boundary

"S3 bucket policies for public exposure" → SECURITY (public exposure = vulnerability vocabulary)
"S3 bucket replication policy" → gap (data management policy without security vocabulary)
"audit S3 bucket public access" → SECURITY (audit + public access = exposure context)

---

## E057-005 — Trust state calibration

IAM audit + public exposure + key rotation → deterministic_generic (high, dual predicate)
"review cloud costs" → gap (zero predicates)
Security group misconfiguration + unrestricted inbound → deterministic_generic
