# Layer 0-B — Router Pairs (040)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-040-001 — ComplianceService: IMPLEMENT routing

A: "Build a ComplianceService to enforce regulatory rules across all service transactions."
B: Flow: ImplementationFlow41 | DecisionCount: 1

---

## B15-router-040-002 — ComplianceService: IMPLEMENT+SECURITY routing

A: "Build ComplianceService and secure all audit log endpoints."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-040-003 — ComplianceService: IMPLEMENT+ARCHITECTURE routing

A: "Build ComplianceService with a compliance domain model for rule enforcement."
B: Flow: ArchitectureFlow31 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-040-004 — ComplianceService: IMPLEMENT+DOCUMENT routing

A: "Build ComplianceService and document the regulatory rule catalog."
B: Flow: ImplementationFlow41 ‖ DocumentationFlow21 | DecisionCount: 2 | Parallel-eligible

---

## B15-router-040-005 — DOCUMENT parallel-eligible with IMPLEMENT routing

DocumentationFlow21 (2.1) is parallel-eligible with ImplementationFlow41 (4.1).
Both can execute concurrently. Router emits both in DecisionCount and notes parallel eligibility.
Anti-pattern: routing DOCUMENT before ARCHITECTURE or treating DOCUMENT as a sequential blocker.
