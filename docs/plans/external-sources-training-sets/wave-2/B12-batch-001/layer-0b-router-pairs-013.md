# Layer 0-B — Router Pairs (013)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-013-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (OrderWebhooks)

```
source_id: xiigen-b12-0b-013-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-013-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Specify the order webhook contract for shipment and cancellation events."

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

## B12-0B-013-002 — [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4 (webhook + auth)

```
source_id: xiigen-b12-0b-013-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  user_request: "Define the order webhook endpoint contract including HMAC authentication."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
    - predicate: "SECURITY"     → "3.4" (SecurityAuditFlow34)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

---

## B12-0B-013-003 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1 (build webhook)

```
source_id: xiigen-b12-0b-013-003
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Build the order webhook API and define the event contract."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-013-004 — Routing matrix for OrderWebhooks domain variations

```
source_id: xiigen-b12-0b-013-004
pair_kind: router_reference
```

OrderWebhooks routing patterns:

| Predicates | Routes | DecisionCount | TransitionType |
|-----------|--------|---------------|----------------|
| [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | 2 | documentation |
| [API_CONTRACT, DOCUMENT, SECURITY] | 3.3 + 2.1 + 3.4 | 3 | documentation |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | 4.3 + 3.3 + 2.1 | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] | 4.3 + 3.3 + 2.1 + 3.4 | 4 | feature_addition |

FlowOrder rule: IMPLEMENT (4.3) always first when present; ARCHITECTURE (3.1) before API_CONTRACT (3.3).
SECURITY (3.4) and DOCUMENT (2.1) are parallel after primary flows complete.
