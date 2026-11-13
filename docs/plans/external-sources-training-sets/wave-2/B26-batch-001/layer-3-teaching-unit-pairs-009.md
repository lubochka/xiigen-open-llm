# Layer 3 — Teaching Unit Pairs (009)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU009-001 — Teach: "broken access control" fires AUTHORIZATION+SECURITY
  "Audit for broken access control." → ["SECURITY", "AUTHORIZATION"]
  Rule: "broken access control" is simultaneously a security vulnerability and an authorization failure.

## TU009-002 — Teach: "security posture" fires SECURITY
  "Review the security posture of the microservices platform." → ["SECURITY"]
  Rule: "security posture" fires SECURITY via "security" co-occurrence.

## TU009-003 — Teach: "default-allow" alone is gap; "default-allow vulnerability" fires SECURITY
  "The service uses default-allow configuration." → [] (gap — no attack vocabulary)
  "The default-allow configuration is a security vulnerability." → ["SECURITY"]
  Rule: Evaluate the full phrase. "security vulnerability" fires SECURITY even if "default-allow" alone is gap.

## TU009-004 — Teach: "hardcoded" fires SECURITY in credential context
  "Scan for hardcoded API keys and secrets." → ["SECURITY"]
  "The timeout is hardcoded to 30 seconds." → [] (gap — non-credential hardcoding)
  Rule: "hardcoded" fires SECURITY only in credential/secret context (API key, password, token).

## TU009-005 — Teach: "security review report" fires SECURITY+DOCUMENT
  "Document the security review findings." → ["SECURITY", "DOCUMENT"]
  "Write a security review report." → ["SECURITY", "IMPLEMENT", "DOCUMENT"]
  Rule: "security review" fires SECURITY. "document" fires DOCUMENT. "write" fires IMPLEMENT.
