# Held-Out and Corruption Pairs (011)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-011-A — Held-out: IMPLEMENT+TEST: unit test routing
A: "Implement the order discount calculator and write unit tests for all discount scenarios."
B: Predicates: ["IMPLEMENT", "TEST"] | ImplementationFlow41 → UnitTestingFlow51 (5.1 — unit + calculator) | feature_addition

## HC-011-B — Held-out: IMPLEMENT+TEST: E2E test routing
A: "Build the checkout API and write E2E tests for the complete purchase flow."
B: Predicates: ["IMPLEMENT", "TEST"] | ImplementationFlow41 → E2ETestingFlow53 (5.3 — E2E + purchase flow) | feature_addition

## HC-011-C — Corruption v6-02: TEST before IMPLEMENT in routing
v6 corrupt: ["IMPLEMENT", "TEST"] → UnitTestingFlow51 → ImplementationFlow41 (wrong order)
Correct: ImplementationFlow41 → UnitTestingFlow51 (TEST after IMPLEMENT)
Error: Ordering violation. Tests run on implemented code; IMPLEMENT must precede TEST.
