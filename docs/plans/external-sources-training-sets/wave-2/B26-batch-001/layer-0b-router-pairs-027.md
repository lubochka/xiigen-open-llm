# Layer 0-B — Router Pairs (027) — HIPAACompliance Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R027-001 — HIPAA gap → GapHandler

R027-001 test: "Ensure the patient portal is HIPAA compliant before go-live."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R027-002 — HIPAA + encryption audit → SECURITY → 6.3

R027-002 test: "Audit the encryption of PHI at rest and in transit for HIPAA compliance."
  SECURITY → 6.3 (audit + encryption) | CompiledPredicateIds: ["SECURITY"]

---

## R027-003 — HIPAA + MFA + session routing

R027-003 test: "Review MFA configuration and session management for HIPAA-protected systems."
  SECURITY → 6.3 (review + MFA + session) | CompiledPredicateIds: ["SECURITY"]

---

## R027-004 — HIPAA + DOCUMENT (legal documentation)

R027-004 test: "Prepare Business Associate Agreement documentation for our cloud vendor under HIPAA."
  DOCUMENT → 6.2 | No SECURITY trigger (administrative document, no technical security action)
  CompiledPredicateIds: ["DOCUMENT"] | SubPhase: 6.2

---

## R027-005 — HIPAA + AUTHORIZATION (access control audit)

R027-005 test: "Audit access controls to PHI data and verify RBAC boundaries for HIPAA minimum necessary rule."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 (RBAC fires both)
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]
