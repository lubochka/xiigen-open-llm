# Held-Out and Corruption Pairs (004)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-004-A — Held-out: refactoring TransitionType
A: "Refactor the order cancellation service to use the command pattern."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring | ImplementationFlow41

## HC-004-B — Held-out: IMPLEMENT+SECURITY
A: "Build the order API and harden it against injection attacks."
B: Predicates: ["IMPLEMENT", "SECURITY"] | ImplementationFlow41 + SecurityAuditFlow | feature_addition

## HC-004-C — Corruption v6-02: refactoring incorrectly labeled feature_addition
v6 corrupt: "Refactor the order pipeline" → TransitionType: feature_addition
Correct: TransitionType: refactoring ("refactor" verb always → refactoring)
Error: TransitionType miscalculation.
