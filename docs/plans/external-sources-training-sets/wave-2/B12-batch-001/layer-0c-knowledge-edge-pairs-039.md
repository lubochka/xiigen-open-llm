# Layer 0-C — Knowledge Edge Pairs (039)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-039-001 — Rule: "idempotency" and "idempotent" fire API_CONTRACT (B12-R18 extended)

```
source_id: xiigen-b12-0c-039-001
pair_kind: knowledge_edge
```

### Rule B12-R18 (extended): idempotency fires API_CONTRACT

"Idempotent" and "idempotency" are API contract terms. They fire API_CONTRACT unconditionally
when present in a request, because idempotency is a core API design property defined in contracts.

Examples:
  "Define the idempotency API contract" → "idempotency" + "api" + "contract" → API_CONTRACT
  "Connect idempotency enforcement to the REST API routes" → "idempotency" + "connect" + "rest" + "api" + "routes" → API_CONTRACT
  "Implement idempotency key validation for the API endpoint" → "idempotency" + "api" + "endpoint" → API_CONTRACT; + "implement" → IMPLEMENT

Idempotency fires API_CONTRACT because it is always expressed in terms of how the API behaves
under repeated calls — it is inherently an API contract term.

---

## B12-0C-039-002 — Rule: "idempotency-key" header is an API contract artifact, not IMPLEMENT alone

```
source_id: xiigen-b12-0c-039-002
pair_kind: knowledge_edge
```

### Idempotency key as API contract element

The Idempotency-Key request header is an API contract artifact. When a request asks to:
  - "Expose the idempotency guarantee" → API_CONTRACT (expose = release verb)
  - "Define the idempotency key contract" → API_CONTRACT + DOCUMENT
  - "Implement idempotency key validation" → IMPLEMENT + API_CONTRACT

The key distinction:
  Exposing/defining idempotency = API_CONTRACT (documentation/release verb)
  Implementing idempotency key validation = IMPLEMENT + API_CONTRACT (construction verb)

---

## B12-0C-039-003 — Boundary: "connect idempotency layer to REST API" = integration verb → no IMPLEMENT

```
source_id: xiigen-b12-0c-039-003
pair_kind: knowledge_edge
```

### Integration verb boundary with idempotency

"Connect the idempotency enforcement layer to the REST API payment routes" fires:
  API_CONTRACT via: connect + idempotency + rest + api + routes
  Does NOT fire IMPLEMENT: "connect" is an integration verb

"Connect" describes wiring/linking existing components. Even when the action implies infrastructure
changes, "connect" without an explicit construction verb does not fire IMPLEMENT.

Contrast:
  "Connect the idempotency layer to the API" → [API_CONTRACT] only
  "Implement the idempotency layer and connect it to the API" → [IMPLEMENT, API_CONTRACT]
  The difference: "implement" is explicitly present in the second case.
