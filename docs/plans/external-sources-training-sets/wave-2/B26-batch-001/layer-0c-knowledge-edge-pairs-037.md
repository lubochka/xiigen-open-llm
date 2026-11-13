# Layer 0-C — Knowledge Edge Pairs (037) — CSRFProtection Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E037-001 — CSRF fires SECURITY; "configure CSRF middleware" is gap

CSRF (Cross-Site Request Forgery) token → SECURITY (attack vector solo noun)
"CSRF protection" → SECURITY
"configure CSRF middleware" → gap (configuration without audit/vulnerability vocabulary)
"add CSRF header" → gap (add action without audit verb)

---

## E037-002 — SameSite cookie attribute disambiguation

"SameSite=Strict" alone → gap (configuration attribute, no security-action verb)
"audit SameSite cookie configuration for CSRF exposure" → SECURITY (audit verb + CSRF)
"review SameSite attribute" → SECURITY (review + security-adjacent vocabulary)
Teaching: SameSite is a cookie attribute; it fires SECURITY only with an action verb.

---

## E037-003 — Double-submit cookie pattern boundary

"implement double-submit cookie" → IMPLEMENT (not SECURITY alone — implementation task)
"implement double-submit cookie and audit existing CSRF exposure" → IMPLEMENT+SECURITY
Teaching: Implementation requests fire IMPLEMENT. SECURITY fires independently on audit/review vocabulary.

---

## E037-004 — State-changing endpoint vocabulary

"state-changing endpoints" → context indicator, does not fire SECURITY alone
"audit state-changing endpoints for CSRF" → SECURITY (audit verb + CSRF trigger)
"review POST endpoints" → gap (no security vocabulary present)

---

## E037-005 — Trust-state calibration for CSRF

CSRF alone + audit verb → deterministic_generic (high)
"SameSite" alone + review → probationary_accepted (single weak trigger)
CSRF + SameSite + missing token validation → deterministic_generic (three triggers)
