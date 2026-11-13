# Layer 0-B — Router Pairs (009)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-009-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (API versioning)

```
source_id: xiigen-b12-0b-009-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-009-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Document the API versioning strategy for the customer account service and define the v2 endpoint contracts."
  context:
    sub_phase_hint: "3.3"
    domain: "software-development"

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT"
      routes_to: "3.3"
      sub_phase_name: "API Contract Design"
      flow: "APIContractFlow33"
      trigger_terms: ["api", "versioning", "endpoint", "contract"]
      DecisionCount: 1
    - predicate: "DOCUMENT"
      routes_to: "2.1"
      sub_phase_name: "Documentation"
      flow: "DocumentationFlow21"
      trigger_terms: ["document", "define"]
      DecisionCount: 2
  PrimarySubPhase: "3.3"
  SecondarySubPhase: "2.1"
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

Routing note: API versioning documentation uses the standard two-decision routing (3.3 + 2.1).
"versioning" fires API_CONTRACT → 3.3 (Core Rule 21). "document" and "define" fire DOCUMENT → 2.1.
TransitionType: documentation (documenting existing API strategy, not creating new version from scratch).

---

## B12-0B-009-002 — API versioning: when creating new version = feature_addition

```
source_id: xiigen-b12-0b-009-002
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Build the v2 API version of the customer account endpoints with backward compatibility contracts."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"     → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT"  → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"      → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

Contrast with B12-0B-009-001:
  "Document the API versioning strategy" → documentation (no IMPLEMENT) → 2 decisions
  "Build the v2 API version" → feature_addition (IMPLEMENT fires) → 3 decisions

---

## B12-0B-009-003 — Versioning annotation: DOCUMENT routing for annotate verb

```
source_id: xiigen-b12-0b-009-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Annotate the customer account API specification with versioning notes and describe backward-compatibility guarantees."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT"  → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"      → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

Note: "annotate" fires DOCUMENT. "annotate the API specification" = updating existing artifact
→ TransitionType: documentation. Same routing as document/specify/define.

---

## B12-0B-009-004 — API versioning routing: complete pattern table

```
source_id: xiigen-b12-0b-009-004
pair_kind: router_reference
```

API versioning routing patterns in B12:

| Request type | CompiledPredicateIds | Routes | TransitionType | DecisionCount |
|--------------|---------------------|--------|----------------|---------------|
| Document versioning strategy | [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation | 2 |
| Define v2 endpoint contracts | [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation | 2 |
| Annotate spec with versioning | [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | documentation | 2 |
| Build v2 API endpoints | [IMPLEMENT, API_CONTRACT, DOCUMENT] | 4.3 + 3.3 + 2.1 | feature_addition | 3 |
| Write v2 API specification | [IMPLEMENT, API_CONTRACT, DOCUMENT] | 4.3 + 3.3 + 2.1 | feature_addition | 3 |
| Design API versioning architecture | [ARCHITECTURE, API_CONTRACT, DOCUMENT] | 3.1 + 3.3 + 2.1 | documentation | 3 |

Key distinction: "document/define/describe/specify/annotate" versioning → documentation
                "build/write/implement/create" versioning → feature_addition
