# Layer 0-C — Knowledge Edge Pairs (002)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: knowledge_edge

## KE-002-001 — "contract" fires API_CONTRACT regardless of modifier
"data contracts" → API_CONTRACT | "service contracts" → API_CONTRACT | "API contract" → API_CONTRACT
Rule: The word "contract" in any compound noun fires API_CONTRACT. The modifier ("data", "service", "API") does not matter.

## KE-002-002 — "domain model" fires ARCHITECTURE
"domain model" → ARCHITECTURE | "domain entity model" → ARCHITECTURE | "bounded context" → ARCHITECTURE
Rule: "model" in domain modeling context fires ARCHITECTURE. Even when embedded in an IMPLEMENT request.

## KE-002-003 — "scaffold" and "bootstrap" are IMPLEMENT triggers
"scaffold the service" → IMPLEMENT | "bootstrap the application" → IMPLEMENT
These are project initialization verbs that fire IMPLEMENT the same as build/create.

## KE-002-004 — "extend" fires IMPLEMENT
"extend the order service" → IMPLEMENT | "add feature to existing service" → IMPLEMENT
Rule: Extending existing software functionality = IMPLEMENT.
