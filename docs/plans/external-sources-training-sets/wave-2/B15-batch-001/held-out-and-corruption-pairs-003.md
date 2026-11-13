# Held-Out and Corruption Pairs (003)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-003-A — Held-out: "data contracts" fires API_CONTRACT
A: "Implement the order service with clearly defined data contracts for all API payloads."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | APIContractFlow33 + ImplementationFlow41 | feature_addition

## HC-003-B — Held-out: "domain model" fires ARCHITECTURE
A: "Build the payment service using a domain model for transaction state management."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | ArchitectureFlow31 + ImplementationFlow41 | feature_addition

## HC-003-C — Corruption v6-02: "domain model" not caught
v6 corrupt: "Build order service with domain model" → Predicates: ["IMPLEMENT"] only
Correct: ["IMPLEMENT", "ARCHITECTURE"]
Error: v6-02 — "domain model" always fires ARCHITECTURE.
