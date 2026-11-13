# Layer 0-C — Knowledge Edge Pairs (029)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-029-001 — Payload: does NOT independently fire API_CONTRACT

```
source_id: xiigen-b12-0c-029-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the export API contract for the reporting service and specify the response payload schema."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R01: "api" → API_CONTRACT ✓
    Rule B12-R01: "contract" → API_CONTRACT unconditional ✓
    Rule B12-R04: "define"/"specify" → DOCUMENT ✓
    Rule B12-R: "response" fires API_CONTRACT in API context (api co-occurrence) ✓
    Rule B12-R: "payload" → does NOT independently fire API_CONTRACT; serves as modifier ✓
  Key edge: "payload" alone NEVER fires API_CONTRACT. "payload contract" fires via "contract".
  "response payload schema" — "response" fires (in API context), "payload" does not independently.

---

## B12-0C-029-002 — Async callback: "endpoint" fires API_CONTRACT unconditionally

```
source_id: xiigen-b12-0c-029-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the async callback API contract for the payment processing service."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R01: "api" → API_CONTRACT ✓
    Rule B12-R01: "contract" → API_CONTRACT unconditional ✓
    Rule B12-R04: "define" → DOCUMENT ✓
  Note: "async callback" is a design pattern, not an API protocol term. API_CONTRACT fires via "api" + "contract".
  Edge: Domain words (async, callback, bulk, export, import, paginated) do NOT independently fire API_CONTRACT.
  API_CONTRACT fires via the protocol terms (api, rest, graphql, grpc, webhook, endpoint, contract, route).

---

## B12-0C-029-003 — Filterable search: "request" fires API_CONTRACT in API context

```
source_id: xiigen-b12-0c-029-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Implement the search endpoint with filter parameters and write the request schema for the products API."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Rule applications:
    Rule B12-R08: "implement"/"write" → IMPLEMENT ✓
    Rule B12-R08: "endpoint" → API_CONTRACT ✓
    Rule B12-R: "request" fires API_CONTRACT because "endpoint" + "api" are present (API context) ✓
    Rule B12-R02: "schema" fires API_CONTRACT because "endpoint"/"api" is co-occurrence ✓
  Key edge: "request" fires API_CONTRACT only when in API context (co-occurring with api/endpoint/rest/graphql etc.).
  "request" alone (e.g., "handle the user request") does NOT fire API_CONTRACT.
  Boundary: request + API context → API_CONTRACT | request alone → no predicate
