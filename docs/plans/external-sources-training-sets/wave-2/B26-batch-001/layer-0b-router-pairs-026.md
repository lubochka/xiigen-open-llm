# Layer 0-B — Router Pairs (026) — ISO27001Review Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R026-001 — ISO 27001 gap → GapHandler

R026-001 test: "Complete the ISO 27001 gap analysis before the certification audit."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R026-002 — ISO 27001 + TLS audit → SECURITY → 6.3

R026-002 test: "Review TLS cipher suites and certificate validity against ISO 27001 A.10."
  SECURITY → 6.3 | "ISO 27001 A.10" → gap label
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## R026-003 — ISO 27001 + AUTHORIZATION (privilege escalation)

R026-003 test: "Audit IAM (Identity and Access Management) controls for privilege escalation per ISO 27001 A.9."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 (privilege escalation = dual-trigger)
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]

---

## R026-004 — ISO 27001 + DOCUMENT

R026-004 test: "Review the cryptographic procedures and document all deviations for the ISO 27001 ISMS (Information Security Management System) evidence package."
  SECURITY → 6.3 (review + cryptographic) | DOCUMENT → 6.2 (document all deviations)
  TransitionType: documentation | CompiledPredicateIds: ["SECURITY", "DOCUMENT"]

---

## R026-005 — "review security posture conceptually" boundary

R026-005 test: "Review the overall security posture of the application conceptually for ISO 27001 readiness."
  "review security posture conceptually" → gap (no technical action) | CompiledPredicateIds: []
  GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Teaching: Conceptual posture review without technical scope → gap.
