# Layer 0-C — Knowledge Edge Pairs (041)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-041-001 — Rule: "deprecated" is a lifecycle label; it does NOT suppress API_CONTRACT

```
source_id: xiigen-b12-0c-041-001
pair_kind: knowledge_edge
```

### Deprecated endpoint and Core Rule 2

"Define the deprecated endpoint contract" fires API_CONTRACT via "endpoint" and "contract".
"Deprecated" is a lifecycle label on the endpoint. It does NOT:
  - Suppress API_CONTRACT
  - Fire IMPLEMENT (no construction work implied)
  - Fire DOCUMENT alone (only when a documentation verb is present)

Core Rule 2 applies: "contract" fires API_CONTRACT regardless of any modifier.
"Data contract", "deprecated contract", "service contract", "event contract" — all fire API_CONTRACT
because "contract" is an unconditional trigger.

---

## B12-0C-041-002 — Rule: "update the deprecated API documentation" — "update" fires DOCUMENT (not IMPLEMENT)

```
source_id: xiigen-b12-0c-041-002
pair_kind: knowledge_edge
```

### Update as documentation verb

"Update the deprecated API endpoints documentation" fires:
  DOCUMENT: "update" in a documentation context fires DOCUMENT
  API_CONTRACT: "api" + "endpoints"
  NOT IMPLEMENT: "update" here = modifying a specification/document, not building code

B12-R22 (update distinction):
  "Update the spec" → DOCUMENT (updating a specification)
  "Update the endpoint handler" (with code context) → could be IMPLEMENT
  "Update the documentation" → DOCUMENT

The presence of "documentation" as the object of "update" confirms DOCUMENT, not IMPLEMENT.

---

## B12-0C-041-003 — Boundary: implementing a redirect from deprecated to new API = IMPLEMENT

```
source_id: xiigen-b12-0c-041-003
pair_kind: knowledge_edge
```

### Deprecated endpoint redirect = IMPLEMENT case

"Implement the HTTP 301 redirect from deprecated /api/v1/users to /api/v2/users endpoint" fires:
  IMPLEMENT: "implement" is explicit
  API_CONTRACT: "api" + "endpoint"
  TransitionType: feature_addition

This is the only deprecated endpoint case that fires IMPLEMENT. The explicit "implement" verb
is the trigger. "Deprecated", "redirect", "301" are not IMPLEMENT triggers.

Compare:
  "Define the deprecated endpoint contract" → API_CONTRACT + DOCUMENT (no IMPLEMENT)
  "Expose the Sunset header on deprecated endpoints" → API_CONTRACT only (no IMPLEMENT)
  "Implement the redirect from deprecated endpoint" → IMPLEMENT + API_CONTRACT (explicit implement)
