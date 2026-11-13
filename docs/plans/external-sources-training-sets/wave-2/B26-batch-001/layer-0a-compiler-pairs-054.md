# Layer 0-A — Compiler Pairs (054) — ForensicsAnalysis
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C054-001 — Forensic log audit → SECURITY

A: "Audit authentication logs for evidence of credential stuffing attack patterns over the past 30 days."
  "audit" → SECURITY | "authentication logs" → SECURITY | "credential stuffing" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C054-002 — Forensic scan for injection artifacts → SECURITY

A: "Scan database query logs for SQL injection artifact signatures indicating prior exploitation."
  "scan" → SECURITY | "SQL injection" → SECURITY | "exploitation" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C054-003 — "collect server logs" → gap (no security-action vocabulary)

A: "Collect and archive the application server logs for the past quarter."
  "collect" → gap | "archive logs" → gap (data collection task, no audit/vulnerability vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C054-004 — Forensics + persistence mechanism → SECURITY

A: "Review the compromised server image for backdoor persistence mechanisms and unauthorized SSH (Secure Shell) key additions."
  "review" → SECURITY | "backdoor" → SECURITY (attack vector) | "unauthorized SSH key" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C054-005 — Forensics + DOCUMENT → SECURITY+DOCUMENT

A: "Audit the forensic artifacts from the breach incident and document the attack timeline and affected credentials."
  "audit" → SECURITY | "breach" → SECURITY | "credentials" → SECURITY | "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation
