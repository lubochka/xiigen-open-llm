# Layer 1 — Transition Orchestrator Pairs (044)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-044-001 — RateLimiterService: IMPLEMENT single transition

A: "Create a RateLimiterService to throttle API calls per client token."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true

---

## B15-t1-044-002 — RateLimiterService: IMPLEMENT+SECURITY transition

A: "Create RateLimiterService and protect the rate limit configuration endpoint."
B: TransitionType: feature_addition | SubPhaseIds: ["security", "4.1"] | ContextCarryForward: true

---

## B15-t1-044-003 — RateLimiterService: IMPLEMENT+ARCHITECTURE transition

A: "Create RateLimiterService with a rate-limiting domain model covering quota buckets."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "4.1"] | ContextCarryForward: true

---

## B15-t1-044-004 — RateLimiterService: IMPLEMENT+TEST transition

A: "Create RateLimiterService and write tests for sliding window and token bucket algorithms."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1", "5.1"] | ContextCarryForward: true

---

## B15-t1-044-005 — RateLimiterService: IMPLEMENT+ARCHITECTURE+SECURITY transition

A: "Create RateLimiterService with a rate-limiting domain model and protect the configuration endpoint."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "security", "4.1"] | ContextCarryForward: true
