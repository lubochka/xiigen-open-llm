# Layer 0-C — Knowledge Edge Pairs (003)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E003-001 — "secure" as adjective does NOT fire SECURITY

A: "Make the API more secure."
  "make" → gap; "more secure" → gap (adjective comparison, not security-action)
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "secure" as an adjective (more secure, fully secure, secure design) is gap. SECURITY fires on security-action vocabulary (audit, scan, inject, vulnerability, enforce, sanitize, encrypt).

---

## E003-002 — "hardening" fires SECURITY when action verb is present

A: "Apply security hardening to the ProductionDatabase configuration."
  "apply" → IMPLEMENT; "security hardening" → SECURITY; "configuration" → gap
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Teaching: "security hardening" fires SECURITY via "security" co-occurrence. "hardening" alone might be borderline — test: "apply hardening" without "security" → borderline gap.

---

## E003-003 — "quality assurance" spelled out does NOT fire QA

A: "Perform quality assurance on the PaymentService before the release."
  "perform" → gap; "quality assurance" → gap (QA fires only on abbreviation "QA"); "release" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "quality assurance" spelled out is gap for QA predicate. QA abbreviation must be literal.

---

## E003-004 — "injection" alone fires SECURITY (attack vector noun)

A: "The SearchService is vulnerable to injection attacks through the product name parameter."
  "injection attacks" → SECURITY; "vulnerable" → gap (adjective); no action verb needed for attack-vector noun
  CompiledPredicateIds: ["SECURITY"]
  Teaching: Security attack-vector nouns (injection, XSS, CSRF, SSRF, IDOR) fire SECURITY without requiring an action verb.

---

## E003-005 — "QA" in "QA engineer" does NOT fire QA predicate

A: "Assign a QA engineer to review the ShippingService before the deploy."
  "QA engineer" → QA fires on "QA" abbreviation; "review" → gap (no security co-occurrence); "deploy" → gap
  CompiledPredicateIds: ["QA"]
  Teaching: "QA" in compound noun "QA engineer" still fires QA predicate. The abbreviation fires regardless of compound form.
