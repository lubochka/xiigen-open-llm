# Layer 0-B — Router Pairs (023)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-023-001 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1 (build product catalog REST)

```
source_id: xiigen-b12-0b-023-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Build the REST API for the product catalog service and document the endpoint contract."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-023-002 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1 (create GraphQL schema)

```
source_id: xiigen-b12-0b-023-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Create the GraphQL schema and resolver contract for the customer profile service."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-023-003 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (update API doc — documentation)

```
source_id: xiigen-b12-0b-023-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Update the API contract documentation to reflect the new response fields added to the order endpoint."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

Note: "update" does not fire IMPLEMENT → no 4.3 in routing → TransitionType=documentation.

---

## B12-0B-023-004 — IMPLEMENT+API_CONTRACT routing decision summary

```
source_id: xiigen-b12-0b-023-004
pair_kind: router_reference
```

When IMPLEMENT fires with API_CONTRACT, the routing always gains 4.3 at front of FlowOrder:

Without IMPLEMENT:
  [API_CONTRACT, DOCUMENT] → Routes: 3.3 + 2.1
  FlowOrder: [APIContractFlow33, DocumentationFlow21]
  TransitionType: documentation

With IMPLEMENT:
  [IMPLEMENT, API_CONTRACT, DOCUMENT] → Routes: 4.3 + 3.3 + 2.1
  FlowOrder: [ImplementationFlow43, APIContractFlow33, DocumentationFlow21]
  TransitionType: feature_addition

IMPLEMENT always routes to 4.3 (ImplementationFlow43) and always comes first in FlowOrder.
IMPLEMENT always changes TransitionType from "documentation" to "feature_addition".
DecisionCount increases by exactly 1 when IMPLEMENT is added to [API_CONTRACT, DOCUMENT].
