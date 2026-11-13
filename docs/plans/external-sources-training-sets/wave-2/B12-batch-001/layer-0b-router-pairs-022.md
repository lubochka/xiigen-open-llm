# Layer 0-B — Router Pairs (022)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-022-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (API versioning documentation)

```
source_id: xiigen-b12-0b-022-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Document the versioned API contract changes between v1 and v2."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-022-002 — [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1 (design versioning)

```
source_id: xiigen-b12-0b-022-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  user_request: "Design the API versioning strategy and specify the OpenAPI version migration contract."

B:
  RoutingDecisions:
    - predicate: "ARCHITECTURE" → "3.1" (ArchitectureFlow31)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-022-003 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1 (implement versioning)

```
source_id: xiigen-b12-0b-022-003
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Implement API version negotiation and write the response contract for version-specific endpoints."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-022-004 — Versioning routing: "update" Swagger vs "implement" version negotiation

```
source_id: xiigen-b12-0b-022-004
pair_kind: router_reference
```

Versioning TransitionType distinction:

"Update the Swagger spec to mark v1 endpoints as deprecated"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: documentation (update = maintenance verb, not IMPLEMENT)
  Routes: 3.3 + 2.1
  DecisionCount: 2

"Implement API version negotiation and write the response contract"
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  TransitionType: feature_addition (implement + write = IMPLEMENT verbs)
  Routes: 4.3 + 3.3 + 2.1
  DecisionCount: 3

The verb determines TransitionType and whether 4.3 (ImplementationFlow43) is in FlowOrder.
