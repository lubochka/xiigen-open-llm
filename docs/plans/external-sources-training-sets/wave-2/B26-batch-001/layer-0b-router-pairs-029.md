# Layer 0-B — Router Pairs (029) — NISTFramework Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R029-001 — NIST CSF management → GapHandler

R029-001 test: "Align the security program to the NIST CSF identify and protect functions."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R029-002 — NIST SP 800-53 + privilege escalation → SECURITY+AUTHORIZATION

R029-002 test: "Audit NIST SP 800-53 AC-2 controls for privilege escalation paths."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 (privilege escalation = dual-trigger)
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]

---

## R029-003 — NIST + cryptographic review → SECURITY → 6.3

R029-003 test: "Review cryptographic module implementations against NIST FIPS 140-2."
  SECURITY → 6.3 (review + cryptographic) | CompiledPredicateIds: ["SECURITY"]

---

## R029-004 — NIST Identify / risk vocabulary → GapHandler

R029-004 test: "Perform asset inventory and risk assessment as part of NIST CSF Identify function."
  "risk assessment" → gap | "asset inventory" → gap | CompiledPredicateIds: []
  GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R029-005 — NIST Detect + authentication scan → SECURITY → 6.3

R029-005 test: "Scan endpoints for anomalous authentication events per NIST CSF Detect function."
  SECURITY → 6.3 (scan + authentication) | CompiledPredicateIds: ["SECURITY"]
