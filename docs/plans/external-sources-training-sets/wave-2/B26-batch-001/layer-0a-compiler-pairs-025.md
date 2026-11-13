# Layer 0-A — Compiler Pairs (025) — SOC2Audit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C025-001 — SOC2 (System and Organization Controls 2) audit: gap vs SECURITY

SOC2 label alone is gap. Technical security actions co-occurring with SOC2 audit fire SECURITY.

A: "Prepare the SOC2 Type 2 report for the upcoming audit committee review."
  "SOC2" → gap (regulatory label) | "upcoming audit" → gap (noun event)
  CompiledPredicateIds: [] → knowledge_edge

---

## C025-002 — SOC2 + audit verb → SECURITY fires

A: "Audit the authentication controls against SOC2 CC6 (Common Criteria 6) requirements."
  "audit" (verb) → SECURITY fires | "authentication controls" → SECURITY
  "SOC2 CC6" as context label → does not additionally fire; audit verb carries SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C025-003 — SOC2 + encryption vocabulary

A: "Review all encryption keys and certificate rotation schedules for SOC2 CC9 readiness."
  "Review" + "encryption keys" → SECURITY | "certificate rotation" → SECURITY
  "SOC2 CC9" → gap (context label only)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C025-004 — SOC2 + document → SECURITY+DOCUMENT

A: "Audit the access control matrix and document findings for the SOC2 evidence package."
  "audit" → SECURITY | "access control matrix" → SECURITY
  "document findings" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation

---

## C025-005 — SOC2 compliance gap: "ensure compliance" pattern

A: "Ensure the application is SOC2 compliant before the external audit."
  "ensure" → gap | "SOC2 compliant" → gap (compliance label) | "external audit" → gap (noun event)
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "ensure compliance" is always gap regardless of framework name.
