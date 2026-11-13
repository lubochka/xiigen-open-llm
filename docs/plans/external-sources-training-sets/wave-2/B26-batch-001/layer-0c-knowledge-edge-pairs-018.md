# Layer 0-C — Knowledge Edge Pairs (018)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E018-001 — "penetration" fires SECURITY; "pen test" (two words) also fires SECURITY

A-fires (compound): "Conduct a pen test of the authentication subsystem."
  "pen test" → SECURITY (penetration testing — two-word form recognized); "authentication" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

A-fires (full): "Review the penetration testing findings for critical vulnerabilities."
  "penetration testing" → SECURITY; "vulnerabilities" → SECURITY; "review" (security context) → SECURITY
  CompiledPredicateIds: ["SECURITY"]

---

## E018-002 — "red team" fires SECURITY; "blue team" also fires SECURITY

A: "Review the red team and blue team findings from the adversarial exercise — focus on credential exfiltration paths."
  "red team" → SECURITY; "blue team" → SECURITY; "credential exfiltration" → SECURITY (credential trigger); "adversarial" → SECURITY context
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "red team", "blue team", "adversarial exercise" are security-domain vocabulary.

---

## E018-003 — "brute-force" fires SECURITY; "high load" is gap

A-fires: "Audit the rate limiting on the /login endpoint for brute-force resistance — what happens after 1000 requests/minute?"
  "audit" → SECURITY; "brute-force" → SECURITY; "rate limiting" → SECURITY context
  CompiledPredicateIds: ["SECURITY"]

A-gap: "The /login endpoint receives high load during peak hours and needs rate limiting."
  "high load" → gap (performance); "rate limiting" alone without security vocabulary → gap
  CompiledPredicateIds: []
  Teaching: "brute-force" fires SECURITY; "high load" is a performance concern and is gap.

---

## E018-004 — "MFA" (Multi-Factor Authentication) is an explicit SECURITY trigger

A: "Audit the MFA bypass paths in the AdminConsole — can an attacker skip TOTP (Time-based One-Time Password) verification via the session fixation route?"
  "audit" → SECURITY; "MFA" → SECURITY (mfa trigger); "TOTP" → SECURITY (authentication mechanism); "session fixation" → SECURITY (session trigger)
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "MFA" is listed as an explicit SECURITY trigger per predicate rules.

---

## E018-005 — "CVE" fires SECURITY; "issue" and "bug" are gap

A-fires: "Audit the codebase for CVE-2024-23917 exposure in the JSON deserialization layer."
  "audit" → SECURITY; "CVE-2024-23917" → SECURITY (CVE identifier = vulnerability)
  CompiledPredicateIds: ["SECURITY"]

A-gap: "There is an issue in the JSON deserialization layer."
  "issue" → gap; "deserialization" alone without vulnerability vocabulary → gap
  CompiledPredicateIds: []
  Teaching: Named CVEs fire SECURITY. Generic "issue" or "bug" vocabulary is gap.
