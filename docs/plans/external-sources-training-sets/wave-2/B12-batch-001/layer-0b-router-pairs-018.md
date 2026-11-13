# Layer 0-B — Router Pairs (018)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-018-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (ReportingAPI)

```
source_id: xiigen-b12-0b-018-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Define the OpenAPI spec for the reporting API."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-018-002 — [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4 (authentication token)

```
source_id: xiigen-b12-0b-018-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  user_request: "Specify the reporting API contract including authentication token requirements."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
    - predicate: "SECURITY"     → "3.4" (SecurityAuditFlow34)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

---

## B12-0B-018-003 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1 (scaffold reporting)

```
source_id: xiigen-b12-0b-018-003
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Scaffold the reporting API controller and define the contract for query endpoints."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-018-004 — ReportingAPI: single DOCUMENT routing (no API terms)

```
source_id: xiigen-b12-0b-018-004
transition_type: documentation
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  user_request: "Document the reporting metrics and KPI definitions for the sales dashboard."

B:
  RoutingDecisions:
    - predicate: "DOCUMENT"
      routes_to: "2.1"
      sub_phase_name: "Documentation"
      flow: "DocumentationFlow21"
      DecisionCount: 1
  TotalDecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: ["DocumentationFlow21"]

Note: "reporting metrics" and "KPI definitions" contain no API_CONTRACT triggers.
No "api", "endpoint", "contract", "rest", "graphql", etc. → single DOCUMENT routing only.
