# Layer 0-B — Router Pairs (005)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-005-001 — [DOCUMENT] only → 2.1 only (database schema, no API_CONTRACT)

```
source_id: xiigen-b12-0b-005-001
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-005-001"
  CompiledPredicateIds: ["DOCUMENT"]
  user_request: "Define the database schema for the inventory table."
  context:
    sub_phase_hint: "2.1"
    domain: "software-development"

B:
  RoutingDecisions:
    - predicate: "DOCUMENT"
      routes_to: "2.1"
      sub_phase_name: "Documentation"
      flow: "DocumentationFlow21"
      trigger_terms: ["define"]
      DecisionCount: 1
  PrimarySubPhase: "2.1"
  SecondarySubPhase: null
  TotalDecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: ["DocumentationFlow21"]

Routing note: Only DOCUMENT fires. API_CONTRACT does NOT fire because "schema" has no API
co-occurrence. "database schema" → "schema" requires API co-occurrence to fire API_CONTRACT.
Without API_CONTRACT, there is no 3.3 routing decision.
This request routes entirely to Phase 2 (documentation), not Phase 3 (development planning).

---

## B12-0B-005-002 — [DOCUMENT] routing: all non-API schema patterns go to 2.1 only

```
source_id: xiigen-b12-0b-005-002
pair_kind: router_reference
```

Non-API schema patterns that route to 2.1 only (no API_CONTRACT, no 3.3 routing):

| Request phrase | Routing | Reason schema does NOT fire API_CONTRACT |
|----------------|---------|------------------------------------------|
| "database schema for inventory table" | 2.1 only | "database" is not an API co-occurrence |
| "validation schema for order form" | 2.1 only | "validation" is not an API co-occurrence |
| "JSON schema for configuration file" | 2.1 only | "configuration" is not API co-occurrence (unless API config doc) |
| "table schema for user profiles" | 2.1 only | "table" is not an API co-occurrence |
| "data schema for ML model input" | 2.1 only | "ML model" is not API co-occurrence |

Contrast — API-context schema patterns that DO fire API_CONTRACT → 3.3:

| Request phrase | Routing | Reason schema DOES fire API_CONTRACT |
|----------------|---------|--------------------------------------|
| "REST API schema for endpoint" | 3.3 + 2.1 | "REST API" is API co-occurrence |
| "GraphQL schema for queries" | 3.3 + 2.1 | "GraphQL" is API co-occurrence |
| "OpenAPI schema definition" | 3.3 + 2.1 | "OpenAPI" is API co-occurrence |
| "webhook payload schema" | 3.3 + 2.1 | "webhook" is API co-occurrence |
| "gRPC message schema" | 3.3 + 2.1 | "gRPC" is API co-occurrence |

---

## B12-0B-005-003 — [DOCUMENT] routing: single-predicate documentation scenarios

```
source_id: xiigen-b12-0b-005-003
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  user_request: "Describe the data transformation rules for the inventory feed processor."

B:
  RoutingDecisions:
    - predicate: "DOCUMENT" → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: ["DocumentationFlow21"]

Note: No API vocabulary → no API_CONTRACT fires. No design vocabulary → no ARCHITECTURE fires.
Single documentation predicate → single routing decision to Phase 2.

---

## B12-0B-005-004 — DOCUMENT routes to 2.4 when secondary documentation context

```
source_id: xiigen-b12-0b-005-004
pair_kind: router_reference
```

DocumentationFlow routing:
  Primary routing: DOCUMENT → 2.1 (standard documentation sub-phase)
  Alternate routing: DOCUMENT → 2.4 (when secondary documentation context signals present)

Conditions for 2.4 routing:
  - When DOCUMENT is secondary predicate (another predicate is primary)
  - When documentation is about updating an existing specification

In most B12 cases: DOCUMENT routes to 2.1 (primary documentation entry point).

In cases where API_CONTRACT is primary and DOCUMENT is secondary (annotating existing contract):
  FlowOrder: [APIContractFlow33, DocumentationFlow21] — DocumentationFlow21 is the standard
  secondary routing even when documentation is updating existing artifact.

The B12 routing does not use 2.4 as a separate routing destination within the standard
[API_CONTRACT, DOCUMENT] two-predicate pattern. All DOCUMENT routings in B12 go to 2.1
as the documentation sub-phase destination.
