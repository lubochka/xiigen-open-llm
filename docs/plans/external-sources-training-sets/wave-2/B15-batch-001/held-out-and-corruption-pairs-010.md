# Held-Out and Corruption Pairs (010)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-010-A — Held-out: "extend" → IMPLEMENT
A: "Extend the order service to handle split fulfillment across multiple warehouses."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | feature_addition

## HC-010-B — Held-out: IMPLEMENT+SECURITY+AUTHORIZATION+QA four predicates
A: "Build the order admin service, secure sensitive data, add admin roles, and QA the access flow."
B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "QA"] | feature_addition | QA last

## HC-010-C — Corruption v6-02: "wire up" not caught as API_CONTRACT
v6 corrupt: "Build order service and wire up the payment API" → ["IMPLEMENT"] only
Correct: ["IMPLEMENT", "API_CONTRACT"] — "wire up" fires API_CONTRACT.
Error: v6-02 — "wire up [external API]" = API_CONTRACT trigger.
