# Layer 3 — Teaching Unit Pairs
# Batch: B12-batch-001
# Scenario: B12 — Development — API contract documentation
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B12B1-WDA-teach-001 — "interface" fires both API_CONTRACT and ARCHITECTURE

```
source_id: xiigen-b12-teach-001
pair_kind: teaching_unit
```

A: "Design the service interface and define the API contract." Which predicates fire?

B: ARCHITECTURE fires on "design" and "interface" (system design context). API_CONTRACT fires
on "interface" (API context) and "api" and "contract". DOCUMENT fires on "define".
Result: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]. The "interface" term fires both
ARCHITECTURE and API_CONTRACT when both design-context and API-context signals are present.

---

## Pair B12B1-WDA-teach-002 — feature_addition vs documentation TransitionType for API work

```
source_id: xiigen-b12-teach-002
pair_kind: teaching_unit
```

A: "Write the OpenAPI spec for the new orders endpoint" vs "Update the OpenAPI spec to add
the new pagination parameter." Both fire API_CONTRACT. Do they have the same TransitionType?

B: No. "Write the OpenAPI spec for the new endpoint" → TransitionType: feature_addition
(new artifact being created). "Update the OpenAPI spec to add pagination" → TransitionType:
documentation (existing artifact being updated). The TransitionType reflects whether the
work creates something new vs modifies something that already exists.

---

## Pair B12B1-WDA-teach-003 — "schema" needs API co-occurrence to fire API_CONTRACT

```
source_id: xiigen-b12-teach-003
pair_kind: teaching_unit
```

A: "Define the database schema for the orders table." Does API_CONTRACT fire on "schema"?

B: No. "schema" fires API_CONTRACT only with API co-occurrence (api, rest, graphql, openapi,
endpoint, swagger, webhook). "Database schema" has no API co-occurrence. The term "schema" alone
does not fire API_CONTRACT. "database" is not an API domain signal. Result: DOCUMENT fires on
"define"; "schema" produces no predicate. CompiledPredicateIds: ["DOCUMENT"].

---

## Pair B12B1-WDA-teach-004 — SECURITY fires on "authentication" in API context

```
source_id: xiigen-b12-teach-004
pair_kind: teaching_unit
```

A: "Document the API authentication requirements." How many predicates fire?

B: Three predicates: DOCUMENT fires on "document"; API_CONTRACT fires on "api"; SECURITY fires
on "authentication". CompiledPredicateIds: ["DOCUMENT", "API_CONTRACT", "SECURITY"]. Three routing
decisions produced: 2.x (DOCUMENT), 3.3 (API_CONTRACT), 3.4 (SECURITY). Authentication
requirements are simultaneously a documentation concern, an API contract concern, and a
security design concern.

---

## Pair B12B1-WDA-teach-005 — sub-phase 3.3 in Phase 3 context

```
source_id: xiigen-b12-teach-005
pair_kind: teaching_unit
```

A: "What are the Phase 3 sub-phases and what predicates own them?"

B:
  Phase3SubPhases:
    - "3.1 — System Architecture Design — ARCHITECTURE"
    - "3.2 — Component and Module Design — ARCHITECTURE + IMPLEMENT"
    - "3.3 — API Contract Design — API_CONTRACT"
    - "3.4 — Security and Compliance Design — SECURITY + AUTHORIZATION"
    - "3.5 — Infrastructure and Deployment Design — ARCHITECTURE (cloud/infra context)"
  Note: "3.3 is exclusively API_CONTRACT territory. A request with API_CONTRACT predicate
  routes to 3.3. If ARCHITECTURE also fires (e.g., 'design the API architecture'), both
  3.3 and 3.1 receive routing decisions."
