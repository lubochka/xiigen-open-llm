# Layer 0-C — Knowledge Edge Pairs (051) — BugBountyTriage Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E051-001 — Bug bounty action vocabulary boundary

"triage bug bounty XSS submission" → SECURITY (triage + XSS)
"respond to bug bounty researcher" → gap (communication task)
"pay bug bounty reward" → gap (financial transaction)
"assess severity of bug bounty report" → SECURITY (assess in vulnerability context)

---

## E051-002 — Stored XSS vs reflected XSS

Both "stored XSS" and "reflected XSS" fire SECURITY as attack-vector vocabulary
"XSS in comment field" → SECURITY (XSS = solo-noun trigger)
"display user comments" → gap (rendering feature, no XSS context)
Teaching: XSS always fires SECURITY regardless of stored/reflected/DOM variant.

---

## E051-003 — IDOR in bug bounty context

"IDOR in invoice download endpoint" → SECURITY + AUTHORIZATION (dual-predicate)
"download invoice by ID" → gap (feature access pattern, no IDOR/audit vocabulary)
"audit invoice endpoint for IDOR" → SECURITY + AUTHORIZATION (dual)
Teaching: IDOR always fires both predicates.

---

## E051-004 — Bug bounty triage + fix stacking

"triage + implement fix" in one request → SECURITY + IMPLEMENT
"triage SSRF, implement URL allowlist" → SECURITY (triage) + IMPLEMENT (implement allowlist)
TransitionType: feature_addition (IMPLEMENT present)

---

## E051-005 — Trust state calibration

Bug bounty XSS triage → deterministic_generic
IDOR triage + AUTHORIZATION → deterministic_generic (dual predicate)
"respond to researcher" → gap (zero predicates)
