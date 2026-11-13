# Layer 0-A — Compiler Pairs (026) — ISO27001Review
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C026-001 — ISO 27001 (Information Security Management System) label is always gap

A: "Complete the ISO 27001 gap analysis before the certification audit."
  "ISO 27001" → gap | "gap analysis" → gap | "certification audit" → gap (noun event)
  CompiledPredicateIds: [] → knowledge_edge

---

## C026-002 — ISO 27001 + vulnerability vocabulary → SECURITY

A: "Audit the information asset classification and vulnerability management controls per ISO 27001 Annex A."
  "audit" (verb) → SECURITY | "vulnerability management controls" → SECURITY
  "ISO 27001 Annex A" → gap label providing context
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C026-003 — ISO 27001 + cryptography audit

A: "Review the cryptographic key management procedures and TLS cipher configuration against ISO 27001 A.10."
  "Review" + "cryptographic key management" → SECURITY | "TLS cipher" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C026-004 — ISO 27001 + access control audit with AUTHORIZATION

A: "Audit the identity and access management controls for privilege escalation paths per ISO 27001 A.9."
  "audit" → SECURITY | "privilege escalation" → SECURITY + AUTHORIZATION (dual-predicate trigger)
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C026-005 — ISO 27001 "review security posture" gap boundary

A: "Review the security posture of the system conceptually for ISO 27001 readiness."
  "review security posture" without code → borderline; "conceptually" → leans gap
  "security posture" alone without technical action vocabulary → gap
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "review security posture conceptually" without actionable technical scope → gap.
