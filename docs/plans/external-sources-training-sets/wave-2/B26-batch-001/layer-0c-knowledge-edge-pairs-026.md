# Layer 0-C — Knowledge Edge Pairs (026) — ISO27001Review Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E026-001 — ISO 27001 labels that are always gap

"ISO 27001 gap analysis" — gap analysis = planning/admin noun → gap
"ISMS" (Information Security Management System) — framework label → gap alone
"ISO 27001 certification" — certification event → gap
"Annex A" — control reference label → gap alone (needs action verb + security noun)
"ISO 27001 compliance" — compliance noun → gap

---

## E026-002 — ISO 27001 Annex A areas that fire SECURITY when audited

A.9 (Access Control) → SECURITY + AUTHORIZATION when "privilege escalation" or "RBAC" mentioned
A.10 (Cryptography) → SECURITY when "TLS", "cipher", "key management" mentioned
A.12 (Operations Security) → SECURITY when "vulnerability management" mentioned
A.14 (Secure Development) → SECURITY when "injection", "XSS", "code signing" mentioned

---

## E026-003 — "review security posture conceptually" gap boundary

"Review the security posture conceptually for ISO 27001 readiness" → gap
"Review TLS configuration for ISO 27001 A.10" → SECURITY (specific technical noun)
Teaching: "security posture" alone is not a technical security noun. Must have specific tech vocabulary.

---

## E026-004 — ISO 27001 + DOCUMENT boundary

"Audit findings and document them for ISO 27001 evidence" → SECURITY + DOCUMENT
"Document the ISO 27001 policy" → DOCUMENT only (policy documentation, no security action)

---

## E026-005 — Trust state for ISO 27001 audits

"Audit TLS cipher suites against ISO 27001 A.10" → deterministic_generic
"Check the ISO 27001 controls" → probationary_accepted (weak verb "check")
"Ensure ISO 27001 compliance" → knowledge_edge (zero predicates)
