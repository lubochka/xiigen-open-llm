# Layer 3 — Teaching Unit Pairs (005)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: teaching_unit

## TU-005 — "scaffold" and "bootstrap" are IMPLEMENT triggers

Rule: Scaffolding and bootstrapping are software initialization actions = IMPLEMENT.

"scaffold the order service" → IMPLEMENT (creates initial service structure)
"bootstrap the application with DI" → IMPLEMENT (sets up application foundation)
"scaffold from template" → IMPLEMENT (code generation/initialization)

These are equivalent to "build" or "create" — they produce a software artifact.

They do NOT fire: ARCHITECTURE (no design verb + structural noun); API_CONTRACT (no integration); DOCUMENT (no documentation artifact).

Context note: In B15, scaffold and bootstrap are always IMPLEMENT regardless of what they're applied to (service, microservice, module, handler).
