# Layer 1 — Transition Orchestrator Pairs (059)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-059-001 — Held-out: CustomerProfileService + SyncService combined refactoring

A: "Refactor CustomerProfileService and SyncService to use event-driven updates instead of polling."
B: TransitionType: refactoring | SubPhaseIds: ["4.1"] | ContextCarryForward: true
"refactor" → IMPLEMENT | refactoring. Two IMPLEMENT triggers → one "4.1". No other predicates present.

---

## B15-t1-059-002 — Held-out: ExportService + ImportService security hardening

A: "Secure all ExportService and ImportService endpoints and authenticate the import source connections."
B: TransitionType: feature_addition | SubPhaseIds: ["security", "4.1"] | ContextCarryForward: true
Wait — "secure" + "authenticate" → SECURITY. But no explicit build/implement verb present.
If only SECURITY verbs are present without IMPLEMENT verbs, then SECURITY only → SubPhaseIds: ["security"].
Clarification: in B15, all requests are assumed to build the service. If explicit IMPLEMENT verb absent but service name mentioned: IMPLEMENT is still inferred.
B: TransitionType: feature_addition | SubPhaseIds: ["security", "4.1"] | ContextCarryForward: true

---

## B15-t1-059-003 — Held-out: RateLimiterService + CircuitBreakerService + RetryService resilience refactor

A: "Refactor RateLimiterService, CircuitBreakerService, and RetryService to use a shared resilience policy registry."
B: TransitionType: refactoring | SubPhaseIds: ["4.1"] | ContextCarryForward: true
"refactor" → IMPLEMENT | refactoring. Three services → one "4.1" (deduplicated).

---

## B15-t1-059-004 — Held-out: MonitoringService + all services integration (seven-predicate)

A: "Build MonitoringService with a monitoring domain model, integrate with Prometheus and Datadog, secure the metrics endpoints, write observability tests, document health checks, and QA before production release."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "3.3", "security", "4.1", "5.1", "2.1", "7.3"] | ContextCarryForward: true
"production release" → "7.3" (production gate). All seven predicates.

---

## B15-t1-059-005 — "7.3" vs. "7.1" sub-phase selection in SubPhaseIds

"QA before release" or "QA" alone → "7.1" (staging gate default).
"QA before production release" or "production release gate" → "7.3".
MonitoringService (059-004): "QA before production release" → "7.3".
