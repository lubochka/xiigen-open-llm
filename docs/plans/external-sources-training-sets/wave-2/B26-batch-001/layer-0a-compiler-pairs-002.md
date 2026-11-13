# Layer 0-A — Compiler Pairs (002)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C002-001 — SECURITY via XSS injection check

A: "Scan the CheckoutService for XSS (Cross-Site Scripting) vulnerabilities in the product description field."
  "scan" → SECURITY (security scan action); "XSS" → SECURITY; "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C002-002 — SECURITY + IMPLEMENT via enforce + sanitize

A: "Enforce input sanitization on all API endpoints and implement the XSS filter in the ProductSearchService."
  "enforce" → SECURITY; "sanitization" → SECURITY; "API endpoints" → API_CONTRACT; "implement" → IMPLEMENT
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "IMPLEMENT"] | SubPhase: 6.3/4.1

---

## C002-003 — SECURITY + QA via "audit" + "QA review"

A: "Audit the AdminService permissions model and QA the role-assignment workflow."
  "audit" → SECURITY; "permissions" → AUTHORIZATION; "QA" → QA
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "QA"] | SubPhase: 6.3

---

## C002-004 — Gap: "compliance" is gap; SECURITY does not fire on "GDPR"

A: "Ensure the UserDataService is GDPR compliant and meets data retention requirements."
  "ensure" → gap; "GDPR" → gap; "compliant" → gap; "data retention" → gap
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development"

---

## C002-005 — SECURITY + DOCUMENT via "security review report"

A: "Conduct a security review of the PaymentModule and document all findings."
  "security review" → SECURITY; "conduct" → borderline (audit-adjacent); "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2
