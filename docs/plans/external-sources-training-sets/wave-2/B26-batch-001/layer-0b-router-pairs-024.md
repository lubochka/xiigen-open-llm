# Layer 0-B — Router Pairs (024) — Complete Predicate-to-Sub-Phase Routing Reference
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R024-001 — SECURITY routing map

SECURITY routes to sub-phase 6.3 in ALL of these contexts:
  - audit/review/scan + security vocabulary → 6.3
  - "authentication", "authorization", "vulnerability", "encryption", "TLS", "certificate" → 6.3
  Exception: "write security tests" / "implement security controls" → SECURITY → 5.2

R024-001 test: "Audit the JWT signing service for algorithm vulnerabilities."
  SECURITY → 6.3 (audit verb + jwt + signing + vulnerabilities)

---

## R024-002 — QA routing map

QA routes to sub-phase 7.3 always.
  "QA" (literal abbreviation) → 7.3
  "quality assurance" (spelled out) → gap, no routing

R024-002 test: "QA the security review findings."
  QA → 7.3 | SECURITY → 6.3 via "security review"
  CompiledPredicateIds: ["QA", "SECURITY"] | SubPhases: [7.3, 6.3]

---

## R024-003 — SECURITY+QA combined routing

SECURITY+QA always produces two independent routing decisions.
Neither suppresses the other. Primary sub-phase is 6.3 (SECURITY takes precedence).

R024-003 test: "QA the penetration test remediation and audit the cipher configuration."
  QA → 7.3 | SECURITY → 6.3 | PrimarySubPhase: 6.3

---

## R024-004 — Gap routing: zero predicates → GapHandler

Zero predicates → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development")
No sub-phase assignment. No flow instantiated.

R024-004 test: "Ensure GDPR compliance for the next audit."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R024-005 — SECURITY+AUTHORIZATION routing reference

SECURITY and AUTHORIZATION are independent predicates — both route to 6.3 in audit context.
"privilege escalation" and "IDOR" fire BOTH.

R024-005 test: "Audit for IDOR vulnerabilities and verify the authorization boundary."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]
  Two routing decisions both landing at 6.3 — not collapsed to one.
