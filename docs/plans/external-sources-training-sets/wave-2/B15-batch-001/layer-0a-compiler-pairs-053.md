# Layer 0-A — Compiler Pairs (053)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-053-001 — IdempotencyService: IMPLEMENT from "build"

A: "Build an IdempotencyService to prevent duplicate processing of retried requests."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"build" → IMPLEMENT.

---

## B15-compiler-053-002 — IdempotencyService: IMPLEMENT+SECURITY on "secure"

A: "Build IdempotencyService and secure the idempotency key lookup endpoint."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
"build" → IMPLEMENT. "secure" → SECURITY.

---

## B15-compiler-053-003 — IdempotencyService: IMPLEMENT+API_CONTRACT on "contracts"

A: "Build IdempotencyService and define the idempotency key API contracts."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
"build" → IMPLEMENT. "API contracts" → API_CONTRACT.

---

## B15-compiler-053-004 — IdempotencyService: IMPLEMENT+TEST on "write tests"

A: "Build IdempotencyService and write tests for duplicate request detection."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
"build" → IMPLEMENT. "write tests" → TEST.

---

## B15-compiler-053-005 — "idempotency" is a domain noun — does not fire predicates

"idempotency", "idempotent", "deduplication key" are domain nouns.
None fire predicates. IMPLEMENT fires only from action verbs.
v6-01 risk: treating "idempotency" as a trigger and returning [] is incorrect.
