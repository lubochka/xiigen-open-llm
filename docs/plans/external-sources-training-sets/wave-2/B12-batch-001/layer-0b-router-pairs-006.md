# Layer 0-B — Router Pairs (006)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-006-001 — [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1 (three decisions)

```
source_id: xiigen-b12-0b-006-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-006-001"
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  user_request: "Design the REST API architecture for the admin panel and document the route structure."
  context:
    sub_phase_hint: "3.3"
    domain: "software-development"

B:
  RoutingDecisions:
    - predicate: "ARCHITECTURE"
      routes_to: "3.1"
      sub_phase_name: "Architecture (system)"
      flow: "ArchitectureFlow31"
      trigger_terms: ["design", "architecture"]
      DecisionCount: 1
    - predicate: "API_CONTRACT"
      routes_to: "3.3"
      sub_phase_name: "API Contract Design"
      flow: "APIContractFlow33"
      trigger_terms: ["rest", "api", "route"]
      DecisionCount: 2
    - predicate: "DOCUMENT"
      routes_to: "2.1"
      sub_phase_name: "Documentation"
      flow: "DocumentationFlow21"
      trigger_terms: ["document"]
      DecisionCount: 3
  PrimarySubPhase: "3.3"
  SecondarySubPhases: ["3.1", "2.1"]
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]

Routing note: Three predicates → three routing decisions. ARCHITECTURE routes first (system design
runs before API contract documentation). API_CONTRACT routes to 3.3 (B12 primary). DOCUMENT routes
to 2.1. FlowOrder: architecture planning first, then API contract work, then documentation.
TransitionType: documentation (no IMPLEMENT fires).

---

## B12-0B-006-002 — Three-decision routing: FlowOrder explanation

```
source_id: xiigen-b12-0b-006-002
pair_kind: router_reference
```

FlowOrder for [ARCHITECTURE, API_CONTRACT, DOCUMENT] routing:

  1. ArchitectureFlow31 (Phase 3.1)
     - Runs first: system architecture decisions must precede API design
     - Creates: architectural context for API structure
     - Output: service boundary map, component relationships

  2. APIContractFlow33 (Phase 3.3)
     - Runs second: API contract work uses architecture as input
     - Creates: endpoint specifications, route definitions, contract documents
     - Output: API specification files, contract definitions

  3. DocumentationFlow21 (Phase 2.1)
     - Runs third: documentation captures the decisions from both flows above
     - Creates: human-readable documentation from the contract artifacts
     - Output: developer-facing API documentation

This ordering follows the natural dependency: architecture → API contract → documentation.
The orchestrator enforces this order when ARCHITECTURE is present with API_CONTRACT.

---

## B12-0B-006-003 — Three-decision routing: admin panel routes with access control

```
source_id: xiigen-b12-0b-006-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "AUTHORIZATION"]
  user_request: "Design the API route structure for the admin panel and document the access control requirements for each endpoint."

B:
  RoutingDecisions:
    - predicate: "ARCHITECTURE"  → "3.1" (ArchitectureFlow31)
    - predicate: "API_CONTRACT"  → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"      → "2.1" (DocumentationFlow21)
    - predicate: "AUTHORIZATION" → "3.4" (AuthorizationFlow34)
  TotalDecisionCount: 4
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21", "AuthorizationFlow34"]

Note: "access control" fires AUTHORIZATION → adds 3.4 routing. Four predicates → four routing
decisions to four destinations. TransitionType stays documentation (no IMPLEMENT fires).

---

## B12-0B-006-004 — Complete three-predicate routing reference

```
source_id: xiigen-b12-0b-006-004
pair_kind: router_reference
```

Three-predicate routing patterns in B12:

Pattern A: [ARCHITECTURE, API_CONTRACT, DOCUMENT]
  → 3.1 + 3.3 + 2.1
  FlowOrder: [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21]
  Example: "Design the REST API architecture and document the routes"

Pattern B: [IMPLEMENT, API_CONTRACT, DOCUMENT]
  → 4.3 + 3.3 + 2.1
  FlowOrder: [ImplementationFlow43, APIContractFlow33, DocumentationFlow21]
  TransitionType: feature_addition
  Example: "Write the OpenAPI spec and document the endpoints"

Pattern C: [API_CONTRACT, DOCUMENT, SECURITY]
  → 3.3 + 2.1 + 3.4
  FlowOrder: [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34]
  Example: "Document the API contract including authentication requirements"

Pattern D: [API_CONTRACT, DOCUMENT, AUTHORIZATION]
  → 3.3 + 2.1 + 3.4
  FlowOrder: [APIContractFlow33, DocumentationFlow21, AuthorizationFlow34]
  Example: "Specify the API and document access control requirements"
