# Layer 0-B — Router Pairs (017)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-017-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (ProductSearchAPI GraphQL)

```
source_id: xiigen-b12-0b-017-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Specify the GraphQL query contract for the product search API."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-017-002 — [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1 (design GraphQL)

```
source_id: xiigen-b12-0b-017-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  user_request: "Design the GraphQL API for the product search service and define the query schema."

B:
  RoutingDecisions:
    - predicate: "ARCHITECTURE" → "3.1" (ArchitectureFlow31)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]

Note: "design" fires ARCHITECTURE → adds 3.1. "design" + "GraphQL API" → both 3.1 and 3.3
receive routing decisions. Neither suppresses the other.

---

## B12-0B-017-003 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1 (implement resolvers)

```
source_id: xiigen-b12-0b-017-003
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Implement the GraphQL resolvers for product search and create the schema definition."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-017-004 — ProductSearchAPI: GraphQL vs REST routing (both go to 3.3)

```
source_id: xiigen-b12-0b-017-004
pair_kind: router_reference
```

Sub-phase routing is determined by PREDICATES, not by API style.
Both GraphQL and REST requests with API_CONTRACT → 3.3.

GraphQL request: "Specify GraphQL schema for product search"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routes: 3.3 + 2.1

REST request: "Specify REST endpoints for product search"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routes: 3.3 + 2.1

Both produce identical routing. The flow (APIContractFlow33) handles both styles.
The API style (REST vs GraphQL) determines CONTENT inside the flow, not the routing.
