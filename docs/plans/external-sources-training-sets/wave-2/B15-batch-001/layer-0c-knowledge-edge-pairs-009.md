# Layer 0-C — Knowledge Edge Pairs (009)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: knowledge_edge

## KE-009-001 — "define" noun disambiguation
"define the order schema" → could be IMPLEMENT (code) or DOCUMENT (spec)
"define the data contract" → API_CONTRACT (contract keyword fires)
"define the domain model" → ARCHITECTURE (model keyword fires)
"define the service interface" → IMPLEMENT (code artifact — interface is a software component)
Rule: "define" routes based on the noun that follows.

## KE-009-002 — "implement" + "integrate" common B15 pattern
"implement the order service and integrate with the payment gateway" → IMPLEMENT + API_CONTRACT
"implement the fulfillment service and integrate with the warehouse API" → IMPLEMENT + API_CONTRACT
This is the canonical two-predicate B15 pattern. Always fire both.

## KE-009-003 — "secure" always fires SECURITY in B15
"secure the payment tokens" → SECURITY
"secure the order data at rest" → SECURITY
"secure the API against unauthorized access" → SECURITY
"harden the order endpoint against injection" → SECURITY

## KE-009-004 — Complete B15 ordering in seven-predicate case
ARCHITECTURE → API_CONTRACT → IMPLEMENT → SECURITY → TEST ‖ DOCUMENT → QA
This is the maximum pipeline for B15. QA is absolute last.
