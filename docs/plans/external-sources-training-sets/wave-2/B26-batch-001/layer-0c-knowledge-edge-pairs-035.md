# Layer 0-C — Knowledge Edge Pairs (035) — XSSMitigation Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E035-001 — XSS vocabulary families

Stored XSS: "stored XSS", "persisted XSS" → SECURITY
Reflected XSS: "reflected XSS" → SECURITY
DOM-based XSS: "DOM-based XSS" → SECURITY
XSS vectors: "output encoding", "HTML entity encoding", "sanitization" → SECURITY (sanitization group)
XSS prevention: "CSP as XSS defense" → SECURITY

---

## E035-002 — "protect" verb fires SECURITY

"protect against XSS" → SECURITY ("protect" is in security-action group)
"protect user-generated content" + attack vocabulary → SECURITY
"protect from injection" → SECURITY (protect + injection)
Teaching: "protect" is a security-action verb equivalent to "audit" in strength when paired with attack vocabulary.

---

## E035-003 — Sanitization vocabulary group

"sanitization" → SECURITY (standalone trigger)
"output encoding" → SECURITY
"HTML entity encoding" → SECURITY
"input validation" → borderline; stronger when paired with injection/XSS vocabulary
Teaching: "sanitization" and "output encoding" fire SECURITY independently.

---

## E035-004 — XSS + IMPLEMENT vs SECURITY boundary

"Implement output encoding to fix XSS" → IMPLEMENT + SECURITY (both fire)
"Audit XSS vectors" → SECURITY alone
"Add sanitization middleware" → IMPLEMENT alone (no audit vocabulary)
Teaching: "implement fix" fires IMPLEMENT; "audit/review" fires SECURITY. Both can co-occur.

---

## E035-005 — Trust state for XSS audits

"Audit user-input rendering for stored and reflected XSS vulnerabilities" → deterministic_generic
"Review output encoding for XSS" → deterministic_generic (review + XSS)
"Implement HTML encoding" → knowledge_edge for SECURITY path (IMPLEMENT path, not 6.3)
