# Layer 0-C — Knowledge-Edge Pairs (030)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-030-001 — "connect to the OpenExchangeRates API" fires API_CONTRACT

"connect to the OpenExchangeRates API" → API_CONTRACT fires on "connect" verb.
Named external system (OpenExchangeRates) confirms context but is not required for API_CONTRACT to fire.

---

## B15-kedge-030-002 — "document the rate-caching strategy" fires DOCUMENT

"document the rate-caching strategy" → DOCUMENT fires on "document" verb.
"rate-caching strategy" is the topic of the documentation, not a separate predicate.

---

## B15-kedge-030-003 — "extend" fires IMPLEMENT as feature_addition (not refactoring)

"Extend the ExchangeRateService to support historical rate lookups."
→ "extend" → IMPLEMENT fires. TransitionType: feature_addition.
"extend" does NOT produce TransitionType: refactoring. Only "refactor" verb produces refactoring.

---

## B15-kedge-030-004 — "in-memory" and "distributed cache" do NOT fire predicates

"Refactor ExchangeRateService to use a distributed cache instead of in-memory."
→ Only "refactor" fires IMPLEMENT (with TransitionType: refactoring).
"distributed cache" and "in-memory" are implementation approach nouns — no predicate fires.

---

## B15-kedge-030-005 — SECURITY verb test: "authenticate" fires SECURITY

"authenticate the rate-fetch requests" → SECURITY fires on "authenticate".
"rate-fetch requests" is the target, not a trigger.
Complete SECURITY verb list: secure / harden / protect / encrypt / authenticate.
