# Layer 0-C — Knowledge Edge Pairs (008)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: knowledge_edge

## KE-008-001 — IMPLEMENT predicate = sub-phase 4.1 default for B15
All IMPLEMENT predicates in B15 route to sub-phase 4.1 (Backend Services and Business Logic).
No other IMPLEMENT sub-phase applies to this batch.

## KE-008-002 — "add" + software artifact → IMPLEMENT
"add order validation to the submission handler" → IMPLEMENT (add + code artifact)
"add retry logic to the payment processor" → IMPLEMENT (add + code artifact)
"add a circuit breaker to the external API client" → IMPLEMENT (add + code artifact)

## KE-008-003 — "write" disambiguation in B15
"write the order state machine" → IMPLEMENT (write + code artifact)
"write unit tests for the state machine" → TEST (write + test artifact)
"write the API documentation" → DOCUMENT (write + documentation artifact)
Rule: "write" fires based on what follows: code → IMPLEMENT; test → TEST; docs → DOCUMENT.

## KE-008-004 — "create" disambiguation in B15
"create the order repository" → IMPLEMENT (create + software component)
"create unit tests" → TEST (create + test artifact)
"create the API specification" → DOCUMENT (create + documentation artifact)
Same disambiguation as "write" — noun type determines predicate.
