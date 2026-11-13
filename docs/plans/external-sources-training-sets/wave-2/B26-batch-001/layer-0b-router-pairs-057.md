# Layer 0-B — Router Pairs (057) — CloudSecurityPosture Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R057-001 — Cloud IAM audit → 6.3 dual

R057-001 test: "Audit IAM role permissions and S3 bucket policies for public exposure vulnerabilities."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | Dual predicates, same sub-phase
  TrustState: deterministic_generic

---

## R057-002 — Cloud misconfiguration scan → 6.3

R057-002 test: "Scan the AWS environment for security group misconfigurations allowing unrestricted inbound access."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R057-003 — "review cloud costs" → GapHandler

R057-003 test: "Review the AWS cloud spend and identify cost optimization opportunities."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Teaching: "review" fires SECURITY only when the review object contains security vocabulary.

---

## R057-004 — Key rotation + IAM audit → 6.3 dual

R057-004 test: "Audit AWS KMS key rotation schedules and IAM key age compliance."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | Dual predicates

---

## R057-005 — Cloud posture + DOCUMENT → 6.3 + 6.2

R057-005 test: "Audit exposed storage buckets and vulnerable security groups, then document all findings."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation
