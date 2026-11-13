# Layer 0-C — Knowledge-Edge Pairs (056)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-056-001 — CircuitBreakerService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "circuit breaker domain model" → ARCHITECTURE (full pattern: domain model present)
- "state transition model" (open/closed/half-open) → ARCHITECTURE
- "bounded context" (resilience bounded context) → ARCHITECTURE

---

## B15-kedge-056-002 — CircuitBreakerService: "circuit breaker" alone does NOT fire ARCHITECTURE

"circuit breaker" as a standalone term → does not fire ARCHITECTURE.
"Build a circuit breaker" → IMPLEMENT only.
"Build CircuitBreakerService with a circuit breaker domain model" → IMPLEMENT + ARCHITECTURE.
The "[X] domain model" or explicit model suffix is required.

---

## B15-kedge-056-003 — CircuitBreakerService: TEST trigger

"write tests for open, half-open, and closed state transitions" → TEST.
"test failure threshold detection" → TEST.
"write integration tests for circuit breaker with dependent services" → TEST.

---

## B15-kedge-056-004 — CircuitBreakerService: refactoring — knowledge edge

"refactor" → TransitionType: refactoring. Same ImplementationFlow41 flow.
"Refactor CircuitBreakerService to add per-dependency threshold configuration" → IMPLEMENT | refactoring.

---

## B15-kedge-056-005 — "bulkhead" and "retry" do NOT fire ARCHITECTURE alone

"bulkhead pattern", "retry logic", "circuit breaker" as standalone terms → no ARCHITECTURE.
Pattern names (bulkhead, retry, circuit breaker) are domain nouns when used without "domain model" or "model" suffix.
ARCHITECTURE fires from explicit "[X] domain model", "[X] model", "bounded context", or "architect" verb.
