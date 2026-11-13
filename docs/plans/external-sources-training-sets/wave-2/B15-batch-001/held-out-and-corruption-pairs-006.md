# Held-Out and Corruption Pairs (006)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-006-A — Held-out: QA before release → 7.1
A: "Implement the order history service and QA before release to staging."
B: Predicates: ["IMPLEMENT", "QA"] | ImplementationFlow41 → ReleaseGateFlow71 (7.1)

## HC-006-B — Held-out: QA feature gate → 7.3
A: "Implement the bulk order processor and QA sign-off before the feature ships."
B: Predicates: ["IMPLEMENT", "QA"] | ImplementationFlow41 → QAGatingFlow73 (7.3)

## HC-006-C — Corruption v6-02: "data contract" not firing API_CONTRACT
v6 corrupt: "Build the order service and define the data contract" → ["IMPLEMENT"] only
Correct: ["IMPLEMENT", "API_CONTRACT"] — "data contract" → API_CONTRACT fires.
