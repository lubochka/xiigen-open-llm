# Layer 0-C — Knowledge Edge Pairs (025) — SOC2Audit Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E025-001 — SOC2 framework labels that are always gap

SOC2 Type 1 / Type 2 — audit event noun, not security action → gap
SOC2 CC6 (Common Criteria 6) — context label → gap alone
SOC2 trust service criteria — regulatory vocabulary → gap alone
"SOC2 compliant" — adjective modifier → gap
"SOC2 readiness" — readiness/planning → gap

---

## E025-002 — Technical vocabulary within SOC2 audits that fires SECURITY

"authentication controls" in SOC2 CC6 → SECURITY
"encryption keys" in SOC2 CC9 → SECURITY (encryption fires)
"access control matrix" → SECURITY + potential AUTHORIZATION
"audit log integrity" → SECURITY (audit verb on log integrity)
"RBAC boundaries" → SECURITY + AUTHORIZATION (dual-trigger)

---

## E025-003 — SOC2 + "review" disambiguation

"review the SOC2 controls" alone → borderline; depends on what follows
"review authentication controls" in SOC2 context → SECURITY fires
"review the SOC2 evidence package" → gap (administrative review)
Teaching: "review" needs a technical security noun as co-occurrence to fire SECURITY.

---

## E025-004 — SOC2 evidence documentation boundary

"Document SOC2 audit findings for the evidence package" → DOCUMENT fires; SECURITY may also fire on "audit findings"
"Prepare SOC2 report" alone → DOCUMENT only (no security action vocabulary)

---

## E025-005 — Trust state: SOC2 + audit verb

"Audit authentication controls" → deterministic_generic (audit verb + auth noun)
"Review access control matrix" → deterministic_generic (review + access control)
"Look over the SOC2 checklist" → probationary_accepted (weak action verb "look over")
