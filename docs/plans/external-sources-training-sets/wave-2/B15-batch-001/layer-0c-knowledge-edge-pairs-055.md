# Layer 0-C — Knowledge-Edge Pairs (055)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-055-001 — RateLimiterService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "rate-limiting domain model" → ARCHITECTURE
- "quota bucket model" → ARCHITECTURE
- "token bucket model" → ARCHITECTURE
- "bounded context" (rate limiting bounded context) → ARCHITECTURE

---

## B15-kedge-055-002 — RateLimiterService: SECURITY triggers

Confirmed SECURITY triggers:
- "protect the rate limit configuration endpoint" → SECURITY
- "secure the quota enforcement API" → SECURITY
- "authenticate rate limit override requests" → SECURITY

---

## B15-kedge-055-003 — RateLimiterService: TEST trigger

"write tests for sliding window and token bucket algorithms" → TEST.
"test rate limit quota enforcement" → TEST.
"write unit tests for rate limit calculation" → TEST.

---

## B15-kedge-055-004 — "throttle" and "limit" as verbs fire IMPLEMENT (confirmed)

"throttle" → IMPLEMENT. "limit" → IMPLEMENT (in code artifact creation context).
"Throttle API calls per client token" → IMPLEMENT. "Limit requests to 100 per minute" → IMPLEMENT.
"limit" as a noun ("the rate limit") does not fire predicates.

---

## B15-kedge-055-005 — RateLimiterService: "sliding window" and "token bucket" are domain nouns

"sliding window algorithm", "token bucket algorithm", "leaky bucket algorithm" are domain nouns.
None fire ARCHITECTURE alone. "[X] domain model" pattern is required for ARCHITECTURE.
"token bucket model" → ARCHITECTURE (model suffix present).
"token bucket algorithm" → no ARCHITECTURE (algorithm suffix, not model/domain model).
