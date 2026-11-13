# Layer 3 — Teaching Unit Pairs (015)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU015-001 — PCI-DSS as acronym is gap; tokenization/encryption fires SECURITY
A: "Audit the payment module for PAN encryption compliance with PCI-DSS." What fires SECURITY?
B: "audit" → SECURITY. "PAN encryption" → SECURITY (encrypt trigger). "tokenization" (if mentioned) → SECURITY.
   "PCI-DSS" alone → gap. The technical actions ("encryption", "tokenization") provide the SECURITY signal.
   Rule: PCI-DSS, SOC2, ISO 27001, GDPR as standalone acronyms are always gap.

## TU015-002 — "cipher" fires SECURITY; "cipher" can appear as cipher suite or algorithm name
A: "Review the TLS cipher suite configuration on all load balancers." Does SECURITY fire?
B: Yes. "cipher suite" → SECURITY (cipher trigger). "TLS" → SECURITY (tls trigger). "review" (security context) → SECURITY.
   CompiledPredicateIds: ["SECURITY"]. Multiple triggers: cipher + TLS.

## TU015-003 — "CVV retention" fires SECURITY via credential/hash context
A: "Audit the CardDataService for CVV retention violations." Does SECURITY fire?
B: Yes. "audit" → SECURITY. "CVV retention" → SECURITY (CVV is a credential/hash stored value).
   "violations" → SECURITY context. CompiledPredicateIds: ["SECURITY"].

## TU015-004 — bcrypt fires SECURITY; SHA-1 fires SECURITY (hash trigger)
A: "Review password storage — confirm bcrypt, reject SHA-1." Does SECURITY fire?
B: Yes. "bcrypt" → SECURITY (hash trigger). "SHA-1" → SECURITY (hash algorithm name). "password storage" → SECURITY context.
   "review" + multiple hash-algorithm names → SECURITY. CompiledPredicateIds: ["SECURITY"].

## TU015-005 — "QA" abbreviation strictness: "QA the cipher suite" vs "quality assurance on the cipher suite"
A-correct: "Audit TLS configuration and QA the cipher suite selections."
   → SECURITY + QA → [6.3, 7.3]
A-gap: "Audit TLS configuration and perform quality assurance on the cipher suite selections."
   → SECURITY only (QA predicate not fired by "quality assurance" spelled out)
   Teaching: QA abbreviation gate is strict. Only "QA" (two capital letters) fires the QA predicate.
