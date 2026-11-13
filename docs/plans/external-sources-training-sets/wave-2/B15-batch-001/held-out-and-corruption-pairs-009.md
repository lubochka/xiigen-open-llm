# Held-Out and Corruption Pairs (009)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-009-A — Held-out: ARCHITECTURE 3.5 + IMPLEMENT
A: "Design the infrastructure for the order processing cluster and implement the first service node."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | ArchitectureFlow35 (3.5) + ImplementationFlow41 | feature_addition

## HC-009-B — Held-out: IMPLEMENT+API_CONTRACT+DOCUMENT three flows
A: "Build the shipping integration service, wire up the FedEx API, and document the integration spec."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"] | APIContractFlow33 → ImplementationFlow41 ‖ DocumentationFlow | feature_addition

## HC-009-C — Corruption: QA placed before IMPLEMENT
v6 corrupt: ["IMPLEMENT", "QA"] → QAGatingFlow73 → ImplementationFlow41 (QA first)
Correct: ImplementationFlow41 → QAGatingFlow73 (QA always last)
Error: absolute ordering constraint violated.
