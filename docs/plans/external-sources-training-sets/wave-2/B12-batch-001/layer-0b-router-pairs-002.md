# Layer 0-B — Router Pairs (002)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-002-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (gRPC contract routing)

```
source_id: xiigen-b12-0b-002-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-002-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Define the gRPC service contract and protobuf schema for the user service."
  context:
    sub_phase_hint: "3.3"
    domain: "software-development"

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
  SecondarySubPhase: "2.1"
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

Routing note: API_CONTRACT routes first to 3.3 (primary sub-phase for B12).
DOCUMENT routes to 2.1 (documentation sub-phase). Two routing decisions, two flows.
TransitionType: documentation — no IMPLEMENT fires.

---

## B12-0B-002-002 — [API_CONTRACT, DOCUMENT] routing: gRPC variant with interface term

```
source_id: xiigen-b12-0b-002-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Define the gRPC interface for the billing service, specify the request and response message types."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT" → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

Note: "interface" in gRPC context → API_CONTRACT, not ARCHITECTURE (no "design" verb present).
This is why ARCHITECTURE does not appear in CompiledPredicateIds and 3.1 is not in routing.

---

## B12-0B-002-003 — [API_CONTRACT, DOCUMENT] routing with IMPLEMENT: feature_addition change

```
source_id: xiigen-b12-0b-002-003
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Implement the gRPC service contract for the shipping service and write the protobuf schema."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

Contrast with B12-0B-002-001:
  Without IMPLEMENT: DecisionCount=2, TransitionType=documentation, FlowOrder=[3.3, 2.1]
  With IMPLEMENT:    DecisionCount=3, TransitionType=feature_addition, FlowOrder=[4.3, 3.3, 2.1]

---

## B12-0B-002-004 — Complete routing matrix for two-predicate API_CONTRACT+DOCUMENT cases

```
source_id: xiigen-b12-0b-002-004
pair_kind: router_reference
```

Two-predicate [API_CONTRACT, DOCUMENT] routing is the most common B12 pattern.

Standard routing result:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  → Route 1: API_CONTRACT → 3.3 (APIContractFlow33)
  → Route 2: DOCUMENT → 2.1 (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: documentation
  FlowOrder: [APIContractFlow33, DocumentationFlow21]

This routing applies to ALL of the following request patterns:
  - gRPC service contract definition
  - Webhook payload contract specification
  - Data contract definition
  - Service contract specification
  - API versioning documentation
  - Endpoint contract documentation
  - OpenAPI spec documentation (without "write" verb)
  - GraphQL schema specification (without "design" verb)

The routing only changes when additional predicates fire:
  + ARCHITECTURE → adds 3.1 route, FlowOrder gains ArchitectureFlow31 at front
  + IMPLEMENT → adds 4.3 route, FlowOrder gains ImplementationFlow43 at front, TransitionType→feature_addition
  + SECURITY → adds 3.4 route, FlowOrder gains SecurityAuditFlow34
  + AUTHORIZATION → adds 3.4 route (separate decision), FlowOrder gains AuthorizationFlow34
