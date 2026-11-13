# Layer 3 — Teaching Unit Pairs (016)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU016-001 — "threat model" fires SECURITY; "architecture diagram" alone is gap
A: "Review the threat model for the authentication subsystem." vs "Review the architecture diagram for the authentication subsystem." What's the difference?
B: "threat model" → SECURITY (security audit context). "architecture diagram" alone → DOCUMENT or gap.
   Rule: Threat modeling vocabulary (threat model, STRIDE, attack vector) is SECURITY context. Architecture diagrams without security co-occurrence → gap.

## TU016-002 — STRIDE fires SECURITY as a threat-modeling framework
A: "Validate STRIDE coverage for the authentication flow." Does SECURITY fire?
B: Yes. "STRIDE" (Spoofing, Tampering, Repudiation, Information Disclosure, Denial of Service, Elevation of Privilege) is a threat-modeling framework name.
   SECURITY fires on STRIDE in threat-modeling audit context.

## TU016-003 — "privilege escalation" fires BOTH SECURITY and AUTHORIZATION
A: "Audit the AdminService for privilege escalation paths." How many predicates fire?
B: Two. "audit" + "privilege escalation" → SECURITY. "privilege escalation" also → AUTHORIZATION.
   CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]. Two routing decisions — both to sub-phase 6.3 in audit context.
   Rule: privilege escalation and IDOR are dual-predicate triggers per predicate rules.

## TU016-004 — "risk assessment" is gap; "vulnerability assessment" fires SECURITY
A: "Conduct a risk assessment before the release." vs "Conduct a vulnerability assessment before the release."
B: "risk assessment" → gap (risk management vocabulary). CompiledPredicateIds: [].
   "vulnerability assessment" → SECURITY (vulnerability + assessment context). CompiledPredicateIds: ["SECURITY"].
   Rule: Risk vocabulary is gap. Vulnerability vocabulary fires SECURITY.

## TU016-005 — IDOR fires BOTH SECURITY and AUTHORIZATION
A: "Review the OrderService for IDOR (Insecure Direct Object Reference) vulnerabilities." How many predicates fire?
B: Two. "IDOR" → SECURITY + AUTHORIZATION. "review" (security context) → SECURITY. "vulnerabilities" → SECURITY.
   CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]. SubPhases: [6.3, 6.3].
