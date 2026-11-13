# Held-Out and Corruption Pairs
# Batch: B12-batch-001
# Scenario: B12 — Development — API contract documentation
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## Held-Out B12B1-HO-001 — gRPC: API_CONTRACT fires

A: "Define the gRPC service contract and generate the protobuf schema for the notification service."
Expected: API_CONTRACT fires on "grpc", "contract", "schema"; DOCUMENT fires on "define".
CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]. Routing: 3.3 + 2.1.

---

## Held-Out B12B1-HO-002 — webhook: API_CONTRACT fires

A: "Specify the webhook payload structure for order status change events."
Expected: API_CONTRACT fires on "webhook"; DOCUMENT fires on "specify".
CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]. Routing: 3.3 + 2.1.

---

## Held-Out B12B1-HO-003 — "route" fires API_CONTRACT

A: "Map the API routes for the admin panel and document the access control requirements."
Expected: API_CONTRACT fires on "api", "route"; DOCUMENT fires on "document"; AUTHORIZATION fires on "access control".
CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "AUTHORIZATION"]. Routing: 3.3 + 2.x + 3.4.

---

## Corruption B12B1-COR-001 — v6-01: API_CONTRACT suppressed on "contract" term

CORRUPT B: "Define the data contracts for the order service" → CompiledPredicateIds: ["DOCUMENT"] only.
Why corrupt: "contract" fires API_CONTRACT regardless of the modifier "data". The corrupt model
suppressed API_CONTRACT because the request seemed like a data modeling task. Correct: ["DOCUMENT", "API_CONTRACT"].

---

## Corruption B12B1-COR-002 — v6-01: SECURITY suppressed in "authentication requirements" request

CORRUPT B: "Document the REST API authentication requirements" → CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"].
Why corrupt: "authentication" fires SECURITY. The corrupt model omitted SECURITY because the dominant
context was API documentation. SECURITY fires literally on "authentication" regardless of context.
Correct: ["API_CONTRACT", "DOCUMENT", "SECURITY"].

---

## Corruption B12B1-COR-003 — v6-03: three routing decisions collapsed to 3.3 only

CORRUPT B: DecisionCount: 1, only 3.3, for [API_CONTRACT, DOCUMENT, SECURITY] input.
Why corrupt: Three predicates must produce three routing decisions. Collapsing to the
"primary" sub-phase 3.3 loses the DOCUMENT (2.4) and SECURITY (3.4) routings.
Correct B: DecisionCount: 3 — 3.3, 2.4, 3.4.
