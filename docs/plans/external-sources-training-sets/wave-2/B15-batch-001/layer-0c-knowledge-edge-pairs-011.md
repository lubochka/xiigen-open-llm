# Layer 0-C — Knowledge Edge Pairs (011)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: knowledge_edge

## KE-011-001 — Canonical B15 request patterns and their predicates

| Pattern | Predicates |
|---------|-----------|
| build [service] | [IMPLEMENT] |
| build + integrate | [IMPLEMENT, API_CONTRACT] |
| build + integrate + write tests | [IMPLEMENT, API_CONTRACT, TEST] |
| build + integrate + secure | [IMPLEMENT, API_CONTRACT, SECURITY] |
| build + write tests + document + QA | [IMPLEMENT, TEST, DOCUMENT, QA] |
| design model + build + integrate + test | [ARCHITECTURE, IMPLEMENT, API_CONTRACT, TEST] |
| refactor [service] | [IMPLEMENT] + refactoring TransitionType |
| build + integrate + secure + test + doc + QA | [IMPLEMENT, API_CONTRACT, SECURITY, TEST, DOCUMENT, QA] |

## KE-011-002 — "bootstrap" is IMPLEMENT, not ARCHITECTURE
"bootstrap the service" → IMPLEMENT (service initialization = code action)
"design the bootstrap configuration architecture" → ARCHITECTURE (design verb + architecture noun)
Rule: "bootstrap" as a verb fires IMPLEMENT. As a noun in an architecture design context, it's part of an ARCHITECTURE request.

## KE-011-003 — Integration tests in B15 route to 5.3
"write integration tests for the order API" → TEST → E2ETestingFlow53 (5.3)
Integration tests verify inter-service communication = interaction/flow context → 5.3.
Unit tests for a single function → 5.1.

## KE-011-004 — "generate" as code generation verb → IMPLEMENT
"generate the order DTOs from the API spec" → IMPLEMENT (code generation = IMPLEMENT)
"generate unit tests for the service" → TEST (generate + test artifact)
Rule: "generate" fires based on what follows: code artifacts → IMPLEMENT; test artifacts → TEST.
