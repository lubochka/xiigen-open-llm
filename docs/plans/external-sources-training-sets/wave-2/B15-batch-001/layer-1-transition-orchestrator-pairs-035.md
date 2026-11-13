# Layer 1 — Transition Orchestrator Pairs (035)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-035-001 — AnonymizationService: IMPLEMENT single transition

A: "Build an AnonymizationService to mask PII fields before export."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true

---

## B15-t1-035-002 — AnonymizationService: IMPLEMENT+SECURITY (two triggers) transition

A: "Build AnonymizationService, protect the raw data endpoint, and encrypt the PII mapping table."
B: TransitionType: feature_addition | SubPhaseIds: ["security", "4.1"] | ContextCarryForward: true
Two SECURITY triggers → one "security" entry in SubPhaseIds (deduplication).

---

## B15-t1-035-003 — AnonymizationService: IMPLEMENT+ARCHITECTURE transition

A: "Build AnonymizationService with an anonymization domain model."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "4.1"] | ContextCarryForward: true

---

## B15-t1-035-004 — AnonymizationService: IMPLEMENT+SECURITY+ARCHITECTURE transition

A: "Build AnonymizationService with an anonymization domain model and protect the raw data endpoint."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "security", "4.1"] | ContextCarryForward: true

---

## B15-t1-035-005 — Deduplication in SubPhaseIds confirmed

SubPhaseIds never contains duplicate entries. Two SECURITY triggers → one "security" entry.
Anti-pattern: SubPhaseIds: ["security", "security", "4.1"] for two SECURITY triggers.
Correct: SubPhaseIds: ["security", "4.1"] — one "security" entry.
