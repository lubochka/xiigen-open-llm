# Layer 0-A — Compiler Pairs (056)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-056-001 — CircuitBreakerService: IMPLEMENT from "build"

A: "Build a CircuitBreakerService to prevent cascading failures in dependent service calls."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"build" → IMPLEMENT.

---

## B15-compiler-056-002 — CircuitBreakerService: IMPLEMENT+ARCHITECTURE on "circuit breaker model"

A: "Build CircuitBreakerService with a circuit breaker domain model defining state transitions (closed/open/half-open)."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"build" → IMPLEMENT. "circuit breaker domain model" → ARCHITECTURE.

---

## B15-compiler-056-003 — CircuitBreakerService: IMPLEMENT+TEST on "write tests"

A: "Build CircuitBreakerService and write tests for open, half-open, and closed state transitions."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
"build" → IMPLEMENT. "write tests" → TEST.

---

## B15-compiler-056-004 — CircuitBreakerService: refactoring TransitionType

A: "Refactor CircuitBreakerService to add per-dependency threshold configuration."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
"refactor" → IMPLEMENT. TransitionType: refactoring.

---

## B15-compiler-056-005 — "circuit breaker" and "bulkhead" are domain nouns — not ARCHITECTURE triggers alone

"circuit breaker" and "bulkhead" as standalone terms do not fire ARCHITECTURE.
"[domain] domain model" pattern fires ARCHITECTURE. "circuit breaker" alone → IMPLEMENT only.
