# Layer 0-B — Router Pairs (055)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-055-001 — RateLimiterService: IMPLEMENT routing

A: "Create a RateLimiterService to throttle API calls per client token."
B: Flow: ImplementationFlow41 | DecisionCount: 1

---

## B15-router-055-002 — RateLimiterService: IMPLEMENT+SECURITY routing

A: "Create RateLimiterService and protect the rate limit configuration endpoint."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-055-003 — RateLimiterService: IMPLEMENT+ARCHITECTURE routing

A: "Create RateLimiterService with a rate-limiting domain model covering quota buckets."
B: Flow: ArchitectureFlow31 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-055-004 — RateLimiterService: IMPLEMENT+TEST routing

A: "Create RateLimiterService and write tests for sliding window and token bucket algorithms."
B: Flow: ImplementationFlow41 → TestingFlow51 | DecisionCount: 2

---

## B15-router-055-005 — RateLimiterService: IMPLEMENT+ARCHITECTURE+SECURITY routing

A: "Create RateLimiterService with a rate-limiting domain model and protect the configuration endpoint."
B: Flow: ArchitectureFlow31 → SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 3
Ordering: 3.1 → security → 4.1
