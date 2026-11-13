# Layer 0-B — Router Pairs (019)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-019-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (NotificationAPI webhook)

```
source_id: xiigen-b12-0b-019-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Define the notification webhook contract for push notification delivery events."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-019-002 — [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4 (notification auth)

```
source_id: xiigen-b12-0b-019-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  user_request: "Document the notification API authentication scheme and webhook security requirements."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
    - predicate: "SECURITY"     → "3.4" (SecurityAuditFlow34)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

---

## B12-0B-019-003 — [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1 (build notification REST)

```
source_id: xiigen-b12-0b-019-003
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  user_request: "Build the REST API for the notification service and write the endpoint contract."

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"    → "4.3" (ImplementationFlow43)
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21"]

---

## B12-0B-019-004 — NotificationAPI: webhook + gRPC routing comparison (both → 3.3)

```
source_id: xiigen-b12-0b-019-004
pair_kind: router_reference
```

Both webhook and gRPC notification contracts route to 3.3:

Webhook notification: "Define notification webhook contract"
  Triggers: webhook → API_CONTRACT, contract → API_CONTRACT
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routes: 3.3 + 2.1

gRPC notification: "Define gRPC service contract for notification delivery"
  Triggers: grpc → API_CONTRACT, contract → API_CONTRACT
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routes: 3.3 + 2.1

Both route identically. The trigger mechanism differs (webhook vs grpc) but the
resulting predicate (API_CONTRACT) is the same and routes to the same sub-phase (3.3).
