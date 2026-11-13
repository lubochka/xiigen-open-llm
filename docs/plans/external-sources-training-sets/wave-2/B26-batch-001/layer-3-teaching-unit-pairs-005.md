# Layer 3 — Teaching Unit Pairs (005)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU005-001 — Teach: "authentication" fires SECURITY; "authorization" fires AUTHORIZATION
  "Audit the authentication layer." → ["SECURITY"]
  "Audit the authorization layer." → ["SECURITY", "AUTHORIZATION"]
  "Audit both authentication and authorization." → ["SECURITY", "AUTHORIZATION"]
  Rule: "authentication" is a SECURITY trigger; "authorization" fires AUTHORIZATION (and often SECURITY in audit context).

## TU005-002 — Teach: "allowlist"/"blocklist" fire SECURITY
  "Implement an allowlist for permitted file types." → ["SECURITY", "IMPLEMENT"]
  "Add the IP to the blocklist." → ["SECURITY", "IMPLEMENT"]
  Rule: Access control list vocabulary (allowlist, blocklist, whitelist, blacklist) fires SECURITY.

## TU005-003 — Teach: "OWASP" fires SECURITY
  "Review against the OWASP Top 10." → ["SECURITY"] | "Implement OWASP API Security mitigations." → ["SECURITY", "IMPLEMENT"]
  Rule: OWASP as a security framework reference fires SECURITY in any context.

## TU005-004 — Teach: "zero-trust" fires SECURITY in architecture context
  "Design a zero-trust architecture." → ["SECURITY", "ARCHITECTURE"]
  Rule: "zero-trust" fires SECURITY. Combined with "design/review" → also fires ARCHITECTURE.

## TU005-005 — Teach: "data masking" and "data exfiltration" fire SECURITY
  "Audit for data exfiltration risks." → ["SECURITY"] | "Implement data masking for PII exports." → ["SECURITY", "IMPLEMENT"]
  Rule: Data protection vocabulary (masking, exfiltration, exposure, leakage) fires SECURITY.
