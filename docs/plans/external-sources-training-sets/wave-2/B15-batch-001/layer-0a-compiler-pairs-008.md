# Layer 0-A — Compiler Pairs (008)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-008-001 — "implement" caching layer → IMPLEMENT

A: "Implement a Redis-backed caching layer for the order status lookup."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition

---

## B15-compiler-008-002 — ARCHITECTURE+IMPLEMENT: design + build

A: "Design the order processing pipeline architecture and implement the first pipeline stage."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | TransitionType: feature_addition
Evidence: design the architecture → ARCHITECTURE; implement the pipeline stage → IMPLEMENT.

---

## B15-compiler-008-003 — "write" + code → IMPLEMENT

A: "Write the order aggregation query service with filtering, sorting, and pagination."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: write + aggregation query service (code artifact noun) → IMPLEMENT.

---

## B15-compiler-008-004 — IMPLEMENT+API_CONTRACT+SECURITY+TEST: four predicates

A: "Build the checkout API, integrate with Stripe, secure the payment tokens, and write tests."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; integrate → API_CONTRACT; secure tokens → SECURITY; write tests → TEST.

---

## B15-compiler-008-005 — "implement" + QA: IMPLEMENT+QA

A: "Implement the order archival service and QA the archival workflow."
B: Predicates: ["IMPLEMENT", "QA"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; QA abbreviation → QA. QA sub-phase: 7.3 (feature gate).
