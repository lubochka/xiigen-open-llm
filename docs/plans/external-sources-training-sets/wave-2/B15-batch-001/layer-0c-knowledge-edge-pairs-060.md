# Layer 0-C — Knowledge-Edge Pairs (060)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-060-001 — Complete ARCHITECTURE trigger reference for 049–060 range

| Service | Confirmed ARCHITECTURE triggers |
|---------|--------------------------------|
| EventPublisherService | "event domain model", "event envelope model" |
| EventConsumerService | "event handler model", "consumer domain model" |
| SagaOrchestrator | "saga domain model", "compensation choreography model" + architect verb |
| CompensationService | "compensation domain model", "undo action model" |
| IdempotencyService | "idempotency domain model", "idempotency key model" |
| CacheService | "cache domain model", "cache hierarchy model" |
| RateLimiterService | "rate-limiting domain model", "quota bucket model", "token bucket model" |
| CircuitBreakerService | "circuit breaker domain model", "state transition model" |
| RetryService | "retry domain model", "retry policy model", "backoff strategy model" |
| DeadLetterService | "dead letter domain model", "failure classification model" |
| MonitoringService | "monitoring domain model", "metrics collection model" |

---

## B15-kedge-060-002 — Complete API_CONTRACT trigger reference for 049–060 range

"integrate with [X broker/API]" confirmed for: RabbitMQ, Kafka, Redis, DLQ, Prometheus, Datadog.
"wire up [X]" confirmed for: message broker API, event bus, cache invalidation API, dead letter queue binding.
"[service] contracts" confirmed for: EventPublisher, EventConsumer, Saga, Idempotency, DeadLetter.

---

## B15-kedge-060-003 — Complete SECURITY trigger reference for 049–060 range

SECURITY triggers confirmed across 049–060 services:
- secure: EventConsumerService (050), IdempotencyService (053), CacheService (054), MonitoringService (059)
- protect: RateLimiterService (055), MonitoringService (059)
- encrypt: IdempotencyService (053), CacheService (054)
- authenticate: EventConsumerService (050), IdempotencyService (053)

---

## B15-kedge-060-004 — Pattern names do NOT fire ARCHITECTURE (049–060 confirmation)

Circuit breaker, retry, bulkhead, saga, rate limiter — none fire ARCHITECTURE as standalone terms.
"[X] domain model" or "[X] model" suffix required. Confirmed for all 049–060 services.

---

## B15-kedge-060-005 — QA trigger confirmed for MonitoringService (059)

"QA" keyword fires QA predicate in MonitoringService and all B15 services.
QA is always the last predicate in execution order — absolute ordering constraint.
No service in B15 is exempt from the QA-last rule when QA is present.
