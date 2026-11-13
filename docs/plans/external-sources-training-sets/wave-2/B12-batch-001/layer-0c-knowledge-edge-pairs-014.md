# Layer 0-C — Knowledge Edge Pairs (014)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-014-001 — "database schema" vs "API schema" boundary (InventoryAPI)

```
source_id: xiigen-b12-0c-014-001
rule_id: B12-RULE-SCHEMA-CO-OCCURRENCE
pair_kind: knowledge_edge
```

A:
  Pair A: "Define the database schema for the inventory stock levels table."
  Pair B: "Define the REST API response schema for the inventory service's stock-check endpoint."

B:
  Pair A result: CompiledPredicateIds: ["DOCUMENT"]
    "database" → NOT an API co-occurrence signal
    "schema" → does NOT fire API_CONTRACT (no API co-occurrence present)
    "define" → DOCUMENT fires
    → No API_CONTRACT, no 3.3 routing

  Pair B result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
    "REST" → API co-occurrence satisfied
    "API" → API co-occurrence satisfied
    "schema" → fires API_CONTRACT (REST + API co-occurrence present)
    "endpoint" → API_CONTRACT fires
    "define" → DOCUMENT fires
    → API_CONTRACT fires, 3.3 routing included

  Teaching: The presence of "database" suppresses nothing — "database" simply provides
  no API co-occurrence for "schema". Change "database" to "REST API" and API_CONTRACT fires.

---

## B12-0C-014-002 — "route" fires API_CONTRACT for inventory API

```
source_id: xiigen-b12-0c-014-002
rule_id: B12-RULE-ROUTE
pair_kind: knowledge_edge
```

A:
  CompiledText: "Specify the API routes and request payload format for inventory reservation operations."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "specify" → DOCUMENT fires
    "API" → API_CONTRACT fires
    "routes" → API_CONTRACT fires on "route"
    "request" → API_CONTRACT fires on "request" (in API context)
    "payload" — does NOT independently fire API_CONTRACT (payload is a modifier)
  Result: ["API_CONTRACT", "DOCUMENT"]
  Teaching: "route" is a first-class API_CONTRACT trigger. "routes" = "route" (plural form).

---

## B12-0C-014-003 — Inventory: "request payload" does NOT fire API_CONTRACT independently

```
source_id: xiigen-b12-0c-014-003
rule_id: B12-RULE-PAYLOAD-NOT-INDEPENDENT
pair_kind: knowledge_edge
```

A:
  Hypothesis: "payload" is in the API_CONTRACT lexicon and fires independently.
  Test: "Define the request payload structure for the inventory service."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate analysis:
    "define" → DOCUMENT fires
    "request" — in isolation without API co-occurrence, "request" does not fire API_CONTRACT
    "payload" — NOT in API_CONTRACT trigger lexicon
    No "api", "rest", "endpoint", "contract", etc. present
  Result: ["DOCUMENT"] only — no API_CONTRACT
  Teaching: "payload" alone does not fire API_CONTRACT. "request" alone does not fire API_CONTRACT.
  "request" requires API co-occurrence (api, endpoint, rest, etc.) to fire API_CONTRACT.
  Compare: "Define the API request payload" → "api" provides co-occurrence → API_CONTRACT fires.
