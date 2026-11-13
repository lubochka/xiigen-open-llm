# Layer 0-B — Router Pairs (014)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-014-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (InventoryAPI standard)

```
source_id: xiigen-b12-0b-014-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-014-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Document the inventory API endpoints for stock level queries."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT"
      routes_to: "3.3"
      sub_phase_name: "API Contract Design"
      flow: "APIContractFlow33"
      DecisionCount: 1
    - predicate: "DOCUMENT"
      routes_to: "2.1"
      sub_phase_name: "Documentation"
      flow: "DocumentationFlow21"
      DecisionCount: 2
  PrimarySubPhase: "3.3"
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-014-002 — [DOCUMENT] only → 2.1 only (database schema — no API_CONTRACT)

```
source_id: xiigen-b12-0b-014-002
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  user_request: "Define the database schema for the inventory stock levels table."

B:
  RoutingDecisions:
    - predicate: "DOCUMENT"
      routes_to: "2.1"
      sub_phase_name: "Documentation"
      flow: "DocumentationFlow21"
      DecisionCount: 1
  PrimarySubPhase: "2.1"
  TotalDecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: ["DocumentationFlow21"]

Routing note: "database schema" → no API_CONTRACT fires → no 3.3 routing.
Only DOCUMENT fires → single routing decision to 2.1. This is the canonical negative
case: "schema" without API co-occurrence never reaches sub-phase 3.3.

---

## B12-0B-014-003 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1 (write OpenAPI)

```
source_id: xiigen-b12-0b-014-003
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Write the OpenAPI specification for the inventory management API."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-014-004 — InventoryAPI: contrast [API_CONTRACT, DOCUMENT] vs [DOCUMENT] routing

```
source_id: xiigen-b12-0b-014-004
pair_kind: router_reference
```

Key routing distinction for InventoryAPI domain:

Case A: "Document the inventory API endpoints"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routes: 3.3 + 2.1
  DecisionCount: 2

Case B: "Define the database schema for the inventory table"
  CompiledPredicateIds: ["DOCUMENT"]
  Routes: 2.1 only
  DecisionCount: 1

The presence of API_CONTRACT determines whether 3.3 is included.
"API endpoints" → API_CONTRACT fires → 3.3 included.
"database schema" → no API co-occurrence → no API_CONTRACT → no 3.3.

v6-03 corruption target: if Case A produces DecisionCount=1 (3.3 only) → CORRUPT.
Correct: DecisionCount=2 (both 3.3 and 2.1).
