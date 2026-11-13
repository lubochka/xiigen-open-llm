# Layer 0-A — Compiler Pairs (016)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C016-001 — SECURITY+QA: threat model review

A: "Review the threat model for the authentication subsystem — validate STRIDE (Spoofing, Tampering, Repudiation, Information Disclosure, Denial of Service, Elevation of Privilege) coverage — and QA all mitigations."
  "review" (threat model context) → SECURITY; "threat model" → SECURITY (vulnerability/security context); "authentication" → SECURITY; "STRIDE" → SECURITY (threat modeling framework = security action context); "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3

---

## C016-002 — SECURITY: attack surface analysis

A: "Audit the external-facing attack surface — enumerate all ingress points, authentication mechanisms, and potential injection entry points."
  "audit" → SECURITY; "attack surface" → SECURITY; "authentication mechanisms" → SECURITY; "injection entry points" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C016-003 — SECURITY+DOCUMENT: threat model document update

A: "Review the existing threat model for privilege escalation paths and document all newly discovered attack vectors."
  "review" (security context) → SECURITY; "threat model" → SECURITY; "privilege escalation" → SECURITY+AUTHORIZATION; "document" → DOCUMENT; "attack vectors" → SECURITY
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C016-004 — Gap: "architecture diagram" without security context is gap

A: "Update the system architecture diagram to reflect the new microservice boundaries."
  "update" → borderline IMPLEMENT? No: "update... architecture diagram" → no predicate; "microservice boundaries" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Gap: Architecture diagrams without security/vulnerability co-occurrence are gap.

---

## C016-005 — SECURITY+QA: threat modeling session output review + QA

A: "Audit the threat modeling session findings for IDOR (Insecure Direct Object Reference) and mass assignment vulnerabilities, and QA the remediation plan."
  "audit" → SECURITY; "IDOR" → SECURITY (vulnerability); "mass assignment vulnerabilities" → SECURITY; "QA" → QA; "remediation plan" → no predicate
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
