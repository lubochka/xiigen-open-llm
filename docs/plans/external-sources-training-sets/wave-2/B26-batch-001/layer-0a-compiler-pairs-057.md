# Layer 0-A — Compiler Pairs (057) — CloudSecurityPosture
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C057-001 — Cloud security posture audit → SECURITY

A: "Audit IAM (Identity and Access Management) role permissions and S3 (Simple Storage Service) bucket policies for public exposure vulnerabilities."
  "audit" → SECURITY | "IAM" → AUTHORIZATION (also) | "public exposure vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C057-002 — Cloud misconfiguration scan → SECURITY

A: "Scan the AWS (Amazon Web Services) environment for security group misconfigurations allowing unrestricted inbound access."
  "scan" → SECURITY | "misconfigurations" in security context → SECURITY | "unrestricted inbound access" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C057-003 — "review cloud costs" → gap

A: "Review the AWS cloud spend and identify cost optimization opportunities."
  "review" → gap (non-security review) | "cloud spend" → gap (cost context, no security vocabulary)
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "review" fires SECURITY only when the review object contains security-action vocabulary.

---

## C057-004 — Cloud key rotation audit → SECURITY

A: "Audit AWS KMS (Key Management Service) key rotation schedules and IAM key age compliance."
  "audit" → SECURITY | "key rotation" → SECURITY (cryptographic group) | "IAM" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C057-005 — Cloud posture + DOCUMENT → SECURITY+DOCUMENT

A: "Audit the cloud security posture for exposed storage buckets and vulnerable security groups, then document all findings in a remediation register."
  "audit" → SECURITY | "exposed storage buckets" → SECURITY | "vulnerable" → SECURITY | "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation
