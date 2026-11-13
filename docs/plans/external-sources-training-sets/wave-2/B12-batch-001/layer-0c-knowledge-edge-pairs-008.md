# Layer 0-C — Knowledge Edge Pairs (008)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-008-001 — Rule: TransitionType for API work — feature_addition vs documentation

```
source_id: xiigen-b12-0c-008-001
rule_id: B12-RULE-TRANSITIONTYPE-API
pair_kind: knowledge_edge
```

### The Rule

TransitionType: feature_addition — when a NEW artifact is being created
TransitionType: documentation — when an EXISTING artifact is being updated or described

The primary determinant: does IMPLEMENT fire in CompiledPredicateIds?
  IMPLEMENT fires → feature_addition
  IMPLEMENT does not fire → documentation (default)

Secondary determinant (for API_CONTRACT-only cases):
  "new endpoint/contract being created for the first time" → feature_addition
    (even without explicit IMPLEMENT, if creation context is unambiguous)
  "documenting/updating existing API or contract" → documentation

Core Rule 12: TransitionType → feature_addition when new artifact created; documentation when updated
Core Rule 13: API_CONTRACT alone → documentation (unless new endpoint/contract being created → feature_addition)
Core Rule 14: API_CONTRACT + IMPLEMENT → feature_addition

---

## B12-0C-008-002 — TransitionType determination examples

```
source_id: xiigen-b12-0c-008-002
rule_id: B12-RULE-TRANSITIONTYPE-API
pair_kind: knowledge_edge
```

Examples by TransitionType:

feature_addition (IMPLEMENT fires):
  "Write the OpenAPI spec for the cart service" → IMPLEMENT (write) → feature_addition
  "Implement the gRPC contract for the shipping service" → IMPLEMENT → feature_addition
  "Build the webhook endpoint for order events" → IMPLEMENT (build) → feature_addition
  "Create the data contract for the fulfillment integration" → IMPLEMENT (create) → feature_addition

documentation (IMPLEMENT does NOT fire):
  "Document the REST API endpoints" → DOCUMENT only → documentation
  "Specify the gRPC service contract" → DOCUMENT only → documentation
  "Define the webhook payload contract" → DOCUMENT only → documentation
  "Annotate the OpenAPI specification" → DOCUMENT only → documentation
  "Describe the GraphQL schema" → DOCUMENT only → documentation
  "Design the REST API architecture and document routes" → ARCHITECTURE + API_CONTRACT + DOCUMENT, no IMPLEMENT → documentation

Borderline: "write spec" (Core Rule 10)
  "Write the API specification" → context determines:
    If creating a new spec file → IMPLEMENT fires → feature_addition
    If the spec already exists and "write" means "write up" (describe) → DOCUMENT → documentation
  In B12 training data, "write the [format] spec/specification" typically = IMPLEMENT fires

---

## B12-0C-008-003 — "integrate" → API_CONTRACT (NOT IMPLEMENT) for TransitionType

```
source_id: xiigen-b12-0c-008-003
rule_id: B12-RULE-INTEGRATE-NOT-IMPLEMENT
pair_kind: knowledge_edge
```

Core Rule 17: "integrate" → API_CONTRACT (NOT IMPLEMENT)

"Integrate the webhook for order status events" → API_CONTRACT (integrate = API interaction verb)
NOT → IMPLEMENT (integrate is NOT a build/create verb in API context)

TransitionType impact:
  "Integrate the webhook" → API_CONTRACT fires, IMPLEMENT does NOT fire → TransitionType: documentation
  "Implement the webhook integration" → IMPLEMENT fires, API_CONTRACT fires → TransitionType: feature_addition

The distinction: "integrate" describes using/connecting an API (API boundary work = API_CONTRACT).
"implement" or "build" describes creating code (construction = IMPLEMENT).

Examples:
  "Integrate the payment gateway API" → API_CONTRACT (documentation)
  "Implement the payment gateway API integration" → IMPLEMENT + API_CONTRACT (feature_addition)
  "Describe how to integrate the event bus webhook" → API_CONTRACT + DOCUMENT (documentation)
  "Write the code to integrate the event bus webhook" → IMPLEMENT + API_CONTRACT + DOCUMENT (feature_addition)

---

## B12-0C-008-004 — TransitionType summary table for common B12 verb patterns

```
source_id: xiigen-b12-0c-008-004
rule_id: B12-RULE-TRANSITIONTYPE-API
pair_kind: knowledge_edge
```

| Verb/action | IMPLEMENT fires? | TransitionType | Note |
|-------------|-----------------|----------------|------|
| write [spec/contract/schema] | YES | feature_addition | creates new artifact |
| implement | YES | feature_addition | always |
| build | YES | feature_addition | always |
| create | YES | feature_addition | code artifact context |
| develop | YES | feature_addition | code development context |
| document | NO | documentation | documentation verb |
| define | NO | documentation | specification verb |
| specify | NO | documentation | specification verb |
| describe | NO | documentation | description verb |
| annotate | NO | documentation | annotation verb |
| design | NO | documentation | design verb (ARCHITECTURE, not IMPLEMENT) |
| integrate | NO | documentation | API_CONTRACT, not IMPLEMENT (Rule 17) |
| review | NO | documentation | review verb |
| update | NO | documentation | usually updating existing |
| add [to existing spec] | NO | documentation | adding to existing artifact |
| version | depends | depends | "create v2" → feature_addition; "document versioning" → documentation |
