# Layer 0-B — Router Pairs (035) — XSSMitigation Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R035-001 — XSS audit → SECURITY → 6.3

R035-001 test: "Audit all user-input rendering paths for stored XSS and reflected XSS vulnerabilities."
  SECURITY → 6.3 (audit + XSS + vulnerabilities) | CompiledPredicateIds: ["SECURITY"]

---

## R035-002 — XSS + sanitization review → SECURITY → 6.3

R035-002 test: "Review output encoding and sanitization logic for DOM-based XSS injection points."
  SECURITY → 6.3 (review + sanitization + XSS) | CompiledPredicateIds: ["SECURITY"]

---

## R035-003 — XSS + CSP audit → SECURITY → 6.3

R035-003 test: "Audit CSP configuration as secondary defense against XSS in the admin panel."
  SECURITY → 6.3 (audit + CSP + XSS) | CompiledPredicateIds: ["SECURITY"]

---

## R035-004 — "protect against XSS" → SECURITY → 6.3

R035-004 test: "Protect all user-generated content fields against stored XSS attacks."
  SECURITY → 6.3 ("protect" is security-action verb + XSS) | CompiledPredicateIds: ["SECURITY"]

---

## R035-005 — XSS fix implementation → IMPLEMENT+SECURITY dual routing

R035-005 test: "Implement HTML entity encoding to fix the stored XSS vulnerability."
  IMPLEMENT → 4.1 | SECURITY → 6.3 (vulnerability) | TransitionType: feature_addition
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
