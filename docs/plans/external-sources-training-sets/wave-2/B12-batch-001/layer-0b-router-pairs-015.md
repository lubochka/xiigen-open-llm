# Layer 0-B — Router Pairs (015)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-015-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (UserManagementAPI basic)

```
source_id: xiigen-b12-0b-015-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-015-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Specify the user management API contract including CRUD operations."

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

## B12-0B-015-002 — [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4 (authentication)

```
source_id: xiigen-b12-0b-015-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  user_request: "Document the user authentication API endpoints and token response schema."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
    - predicate: "SECURITY"     → "3.4" (SecurityAuditFlow34)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

---

## B12-0B-015-003 — [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1 ("design the API")

```
source_id: xiigen-b12-0b-015-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  user_request: "Design the user account management API and specify the endpoint contract."

B:
  RoutingDecisions:
    - predicate: "ARCHITECTURE" → "3.1" (ArchitectureFlow31)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["ArchitectureFlow31", "APIContractFlow33", "DocumentationFlow21"]

Note: ARCHITECTURE routes to 3.1 BEFORE API_CONTRACT routes to 3.3. FlowOrder rule:
ARCHITECTURE (3.1) precedes API_CONTRACT (3.3) in execution sequence.

---

## B12-0B-015-004 — UserManagementAPI full routing reference

```
source_id: xiigen-b12-0b-015-004
pair_kind: router_reference
```

UserManagementAPI routing patterns sorted by predicate count:

1 predicate:
  [API_CONTRACT] → 3.3 only, DecisionCount=1, documentation

2 predicates:
  [API_CONTRACT, DOCUMENT] → 3.3 + 2.1, DecisionCount=2, documentation

3 predicates:
  [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4, DecisionCount=3, documentation
  [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1, DecisionCount=3, documentation
  [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1, DecisionCount=3, feature_addition

FlowOrder priority: ARCHITECTURE > IMPLEMENT > API_CONTRACT > DOCUMENT/SECURITY (parallel last)
