# Held-Out and Corruption Pairs (008)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-008-A — Held-out: five-predicate request
A: "Design the order domain model, build the order service, integrate with the payment gateway, secure payment tokens, and write integration tests."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"] | feature_addition

## HC-008-B — Held-out: deduplication
A: "Build the order service, implement the order handler, and create the order repository."
B: Predicates: ["IMPLEMENT"] (three triggers → one predicate) | ImplementationFlow41 | feature_addition

## HC-008-C — Corruption v6-03: deduplication violated
v6 corrupt: Three IMPLEMENT triggers → three ImplementationFlow41 invocations
Correct: Three triggers → one IMPLEMENT predicate → one ImplementationFlow41
Error: Same flow should not run three times; deduplication applies.
