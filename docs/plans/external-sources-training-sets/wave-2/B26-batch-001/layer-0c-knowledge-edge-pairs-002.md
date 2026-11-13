# Layer 0-C — Knowledge Edge Pairs (002)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E002-001 — "audit" as noun (event) is gap; "audit" as verb fires SECURITY

A: "We have a security audit coming up next quarter — prepare accordingly."
  "audit" (noun event) → gap; "prepare" → gap; "coming up" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "audit" as a noun event is gap. SECURITY fires when "audit" is used as a verb: "audit the service."

---

## E002-002 — "GDPR" is gap; SECURITY does not fire on compliance frameworks

A: "Ensure the DataProcessingService is GDPR (General Data Protection Regulation) compliant."
  "ensure" → gap; "GDPR" → gap; "compliant" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: Compliance frameworks (GDPR, SOC2, ISO 27001, PCI-DSS) are gap. SECURITY fires on security-action vocabulary only.

---

## E002-003 — "security review" fires SECURITY via co-occurrence; "review" alone is gap

A: "Do a security review of the AuthService before going live."
  "security review" → SECURITY via "security" co-occurrence with "review"; "going live" → gap
  vs "Do a code review of the AuthService before merging."
  "code review" → gap (no security co-occurrence)
  CompiledPredicateIds (A): ["SECURITY"] | CompiledPredicateIds (B): []
  Teaching: "review" alone is gap. "security review" fires SECURITY via co-occurrence rule.

---

## E002-004 — "pentest" abbreviation fires SECURITY

A: "Pentest the CheckoutService API endpoints before the release."
  "pentest" → SECURITY (penetration testing abbreviation — SECURITY-action vocabulary); "API endpoints" → API_CONTRACT
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT"]
  Teaching: "pentest" and "penetration test" both fire SECURITY. The abbreviation is recognized.

---

## E002-005 — "vulnerability" alone fires SECURITY (unlike "bug" which is gap)

A: "There is a vulnerability in the LoginService token validation."
  "vulnerability" → SECURITY; no additional action verb needed for vulnerability noun to fire SECURITY
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "vulnerability" as a standalone noun fires SECURITY. Contrast: "bug" alone is gap.
