# Layer 0-B — Router Pairs (004)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-004-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (data contracts routing)

```
source_id: xiigen-b12-0b-004-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-004-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Define the data contracts between the order service and the fulfillment service."
  context:
    sub_phase_hint: "3.3"
    domain: "software-development"

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT"
      routes_to: "3.3"
      sub_phase_name: "API Contract Design"
      flow: "APIContractFlow33"
      trigger_terms: ["contract"]
      DecisionCount: 1
    - predicate: "DOCUMENT"
      routes_to: "2.1"
      sub_phase_name: "Documentation"
      flow: "DocumentationFlow21"
      trigger_terms: ["define"]
      DecisionCount: 2
  PrimarySubPhase: "3.3"
  SecondarySubPhase: "2.1"
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

Routing note: "data contracts" fires API_CONTRACT via "contract" (same routing as webhook contract
from pair B12-0B-003-001). The "data" modifier in "data contracts" does not change routing.
TransitionType: documentation — "define" is a specification verb, not a build verb.

---

## B12-0B-004-002 — [API_CONTRACT, DOCUMENT] → same route for all contract modifier variants

```
source_id: xiigen-b12-0b-004-002
pair_kind: router_reference
```

All "contract" modifier variants produce the same routing:

| Request phrase | CompiledPredicateIds | Routing | TransitionType |
|----------------|---------------------|---------|----------------|
| "data contracts" | [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation |
| "service contracts" | [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation |
| "message contracts" | [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation |
| "event contracts" | [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation |
| "webhook contract" | [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation |
| "API contract" | [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation |
| "REST contract" | [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation |

The word "contract" is a universal router to 3.3 when paired with documentation verbs.

---

## B12-0B-004-003 — Data contract with ARCHITECTURE: three routing decisions

```
source_id: xiigen-b12-0b-004-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "ARCHITECTURE"]
  user_request: "Design the data contract architecture for inter-service communication and define the message formats."

B:
  RoutingDecisions:
    - predicate: "ARCHITECTURE"  → "3.1" (ArchitectureFlow31)
    - predicate: "API_CONTRACT"  → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"      → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]

Note: "design" fires ARCHITECTURE → adds 3.1 routing. "data contract" fires API_CONTRACT → 3.3.
"define" fires DOCUMENT → 2.1. Three routing decisions, FlowOrder: 3.1 first (ARCHITECTURE runs
before API contract documentation).

---

## B12-0B-004-004 — Data contract implementation: IMPLEMENT changes TransitionType

```
source_id: xiigen-b12-0b-004-004
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Build the data contracts layer for the order and fulfillment service integration."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"     → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT"  → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"      → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

Contrast with B12-0B-004-001:
  "Define the data contracts" → documentation, 2 decisions, [3.3, 2.1]
  "Build the data contracts layer" → feature_addition, 3 decisions, [4.3, 3.3, 2.1]
  The verb ("define" vs "build") determines TransitionType.
