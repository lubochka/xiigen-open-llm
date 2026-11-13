# Layer 3 — Teaching Unit Pairs (004)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: teaching_unit

## TU-004 — "domain model" fires ARCHITECTURE

Rule: "model" in a domain modeling context fires ARCHITECTURE, even within an IMPLEMENT request.

Examples that fire ARCHITECTURE:
- "domain model" → ARCHITECTURE
- "domain entity model" → ARCHITECTURE
- "bounded context" → ARCHITECTURE
- "aggregate root" (design context) → ARCHITECTURE

Examples that do NOT fire ARCHITECTURE:
- "data model" (schema/database context) → may fire IMPLEMENT or DOCUMENT
- "view model" (UI context) → IMPLEMENT
- "ML model" (machine learning context) → gap (B33)

Common v6-02 error: "build order service with domain model" → ["IMPLEMENT"] only
Correct: ["IMPLEMENT", "ARCHITECTURE"] — "domain model" always fires ARCHITECTURE.
