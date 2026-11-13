# Layer 0-B — Router Pairs (008)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-008-001 — [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1 (event bus interface)

```
source_id: xiigen-b12-0b-008-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-008-001"
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  user_request: "Design the service interface for the event bus and specify the API contract."
  context:
    sub_phase_hint: "3.3"
    domain: "software-development"

B:
  RoutingDecisions:
    - predicate: "ARCHITECTURE"
      routes_to: "3.1"
      sub_phase_name: "Architecture (system)"
      flow: "ArchitectureFlow31"
      trigger_terms: ["design", "interface (system context)"]
      DecisionCount: 1
    - predicate: "API_CONTRACT"
      routes_to: "3.3"
      sub_phase_name: "API Contract Design"
      flow: "APIContractFlow33"
      trigger_terms: ["interface (API context)", "api", "contract"]
      DecisionCount: 2
    - predicate: "DOCUMENT"
      routes_to: "2.1"
      sub_phase_name: "Documentation"
      flow: "DocumentationFlow21"
      trigger_terms: ["specify"]
      DecisionCount: 3
  PrimarySubPhase: "3.3"
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]

Routing note: "interface" fires BOTH ARCHITECTURE and API_CONTRACT — but this produces only
two routing decisions (to 3.1 and 3.3 respectively), not two API_CONTRACT decisions.
"interface" = one term that contributes to two different predicates simultaneously.
DecisionCount = 3 (three predicates: ARCHITECTURE, API_CONTRACT, DOCUMENT).

---

## B12-0B-008-002 — "interface" disambiguation in routing: same term, two routing contributions

```
source_id: xiigen-b12-0b-008-002
pair_kind: router_reference
```

How "interface" affects routing:

  Signal "interface" with "design" (no API vocab): → ARCHITECTURE only → 3.1 only
  Signal "interface" with API vocab (no "design"):  → API_CONTRACT only → 3.3 only
  Signal "interface" with BOTH:                     → ARCHITECTURE (→3.1) + API_CONTRACT (→3.3)

The term "interface" itself does not create two separate predicate entries.
It contributes to the predicate it contextually matches:
  - In ARCHITECTURE context: counted as one ARCHITECTURE trigger
  - In API context: counted as one API_CONTRACT trigger
  - In BOTH contexts simultaneously: counted as one trigger for EACH → two predicates fire

Routing outcome for "design the service interface ... API contract":
  Three predicates: [ARCHITECTURE, API_CONTRACT, DOCUMENT]
  Three routing decisions: [3.1, 3.3, 2.1]
  FlowOrder: [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21]

---

## B12-0B-008-003 — Event bus API with messaging architecture: same three-decision routing

```
source_id: xiigen-b12-0b-008-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  user_request: "Design the messaging interface for the event bus service and document the API contract for publishing and consuming events."

B:
  RoutingDecisions:
    - predicate: "ARCHITECTURE"  → "3.1" (ArchitectureFlow31)
    - predicate: "API_CONTRACT"  → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"      → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-008-004 — Event bus with security: extends to four decisions

```
source_id: xiigen-b12-0b-008-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  user_request: "Design the event bus API interface, document the contract, and specify authentication requirements for producers and consumers."

B:
  RoutingDecisions:
    - predicate: "ARCHITECTURE"  → "3.1" (ArchitectureFlow31)
    - predicate: "API_CONTRACT"  → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"      → "2.1" (DocumentationFlow21)
    - predicate: "SECURITY"      → "3.4" (SecurityAuditFlow34)
  TotalDecisionCount: 4
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

Note: "authentication" fires SECURITY → adds fourth routing decision to 3.4.
TransitionType remains documentation (no IMPLEMENT fires despite "design" and "specify").
FlowOrder: architecture → API contract → documentation → security review.
