# Layer 0-A — Compiler Pairs (060)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-060-001 — Complete predicate reference for 049–060 range

All IMPLEMENT trigger verbs confirmed for 049–060 services:
- EventPublisherService: build/publish/emit → IMPLEMENT
- EventConsumerService: implement/subscribe/consume → IMPLEMENT
- SagaOrchestrator: build/orchestrate → IMPLEMENT
- CompensationService: create/rollback/undo → IMPLEMENT
- IdempotencyService: build → IMPLEMENT
- CacheService: implement → IMPLEMENT
- RateLimiterService: create/throttle/limit → IMPLEMENT
- CircuitBreakerService: build → IMPLEMENT
- RetryService: implement → IMPLEMENT
- DeadLetterService: create/capture/replay → IMPLEMENT
- MonitoringService: build → IMPLEMENT

---

## B15-compiler-060-002 — TransitionType reference for 049–060 range

All services: feature_addition when IMPLEMENT trigger is not "refactor".
TransitionType: refactoring when "refactor" is present — confirmed for CacheService (054), CircuitBreakerService (056), CompensationService (052).

---

## B15-compiler-060-003 — ARCHITECTURE trigger reference for 049–060 range

"[domain] domain model" fires ARCHITECTURE across all services:
- "event domain model" (049), "saga domain model" (051), "compensation domain model" (052)
- "rate-limiting domain model" (055), "circuit breaker domain model" (056), "retry domain model" (057)

---

## B15-compiler-060-004 — API_CONTRACT trigger reference for 049–060 range

"integrate with [X]" fires API_CONTRACT across all broker/store integration requests.
"define [X] contracts" and "wire up [X] API" also fire API_CONTRACT.
Confirmed: EventPublisherService (049), EventConsumerService (050), SagaOrchestrator (051), CacheService (054), DeadLetterService (058), MonitoringService (059).

---

## B15-compiler-060-005 — Complete non-trigger domain noun reference for 049–060

Non-trigger nouns: "event", "saga", "compensation", "idempotency", "cache", "rate limit", "circuit breaker", "retry", "dead letter", "DLQ", "monitoring", "metrics", "health check" — none fire predicates when used as nouns alone. IMPLEMENT fires only from action verbs.
