# Layer 0-C — Knowledge Edge Pairs (004)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E004-001 — SECURITY sub-phase: audit/review → 6.3; assert/write tests → 5.2

A: "Audit the AuthService for token forgery vulnerabilities." → SECURITY → 6.3
B: "Write tests asserting the AuthService blocks forged tokens." → SECURITY → 5.2
  Teaching: Same domain (token forgery), different action verb → different sub-phase routing.

---

## E004-002 — "risk" vocabulary is gap

A: "Assess the security risks for the new DataIngestionPipeline."
  "assess" → gap; "security risks" → "security" present but "risks" alone is gap; no security-action verb
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "risk assessment" and "security risks" without attack/vulnerability vocabulary are gap. Risk management vocabulary ≠ SECURITY predicate.

---

## E004-003 — "encrypt" fires SECURITY in any context

A: "Encrypt the PII (Personally Identifiable Information) fields in the UserProfile before database storage."
  "encrypt" → SECURITY; "PII" → domain gap but reinforces SECURITY context; no IMPLEMENT from "encrypt" alone
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "encrypt" fires SECURITY. It is a security-action verb in all contexts.

---

## E004-004 — "penetration testing" and "security testing" both fire SECURITY

A: "Commission penetration testing of the CustomerPortal."
  "commission" → gap; "penetration testing" → SECURITY
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "penetration testing" fires SECURITY. "commission" is gap for IMPLEMENT. Single predicate.

---

## E004-005 — "secret" in "secret key" fires SECURITY; "secret" in "trade secret" does not

A: "Rotate the secret keys used for session signing in the AuthService."
  "rotate" → gap (not in IMPLEMENT lexicon without action verb); "secret keys" → SECURITY (cryptographic secrets); "session signing" → SECURITY
  CompiledPredicateIds: ["SECURITY"]
  Teaching: Cryptographic "secret key" fires SECURITY. Business "trade secret" would be gap. Context matters for "secret".
