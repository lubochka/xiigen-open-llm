# Layer 0-B — Router Pairs (016)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-016-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (ShippingRatesAPI)

```
source_id: xiigen-b12-0b-016-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Specify the REST endpoint contract for the shipping rate calculation service."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-016-002 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1 (write Swagger)

```
source_id: xiigen-b12-0b-016-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Write the Swagger documentation for the shipping rates API."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-016-003 — [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1 (design interface)

```
source_id: xiigen-b12-0b-016-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  user_request: "Design the shipping rates service interface and define the API contract for carrier integrations."

B:
  RoutingDecisions:
    - predicate: "ARCHITECTURE" → "3.1" (ArchitectureFlow31)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-016-004 — [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4 (carrier auth)

```
source_id: xiigen-b12-0b-016-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  user_request: "Specify the carrier webhook contract and authentication requirements."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
    - predicate: "SECURITY"     → "3.4" (SecurityAuditFlow34)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

Note: "authentication" unconditionally fires SECURITY → always adds 3.4 routing decision.
DecisionCount must be 3, not 2. v6-02 corruption: SECURITY suppressed → DecisionCount wrongly 2.
