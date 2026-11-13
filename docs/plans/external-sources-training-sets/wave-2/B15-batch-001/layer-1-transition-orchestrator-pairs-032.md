# Layer 1 — Transition Orchestrator Pairs (032)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-032-001 — ComplianceService: IMPLEMENT single transition

A: "Build a ComplianceService to enforce regulatory rules."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true

---

## B15-t1-032-002 — ComplianceService: IMPLEMENT+SECURITY transition

A: "Build ComplianceService and secure all audit log endpoints."
B: TransitionType: feature_addition | SubPhaseIds: ["security", "4.1"] | ContextCarryForward: true

---

## B15-t1-032-003 — ComplianceService: IMPLEMENT+ARCHITECTURE transition

A: "Build ComplianceService with a compliance domain model."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "4.1"] | ContextCarryForward: true

---

## B15-t1-032-004 — ComplianceService: IMPLEMENT+DOCUMENT transition

A: "Build ComplianceService and document the regulatory rule catalog."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1", "2.1"] | ContextCarryForward: true
Note: DOCUMENT (2.1) parallel-eligible with IMPLEMENT (4.1).

---

## B15-t1-032-005 — "enforce" does NOT change SubPhaseIds (confirmed)

"enforce compliance rules" → IMPLEMENT only → SubPhaseIds: ["4.1"].
SECURITY sub-phase ("security") only added when explicit SECURITY trigger verb present.
Anti-pattern: adding "security" to SubPhaseIds because "enforce" is present.
