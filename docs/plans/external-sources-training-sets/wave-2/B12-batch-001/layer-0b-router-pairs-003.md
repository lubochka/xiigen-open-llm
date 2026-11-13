# Layer 0-B — Router Pairs (003)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-003-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1 (webhook contract routing)

```
source_id: xiigen-b12-0b-003-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-003-001"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Specify the webhook payload contract for order status events and document the delivery guarantees."
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

Routing note: "webhook" and "contract" both fire API_CONTRACT → 3.3. "specify" and "document"
fire DOCUMENT → 2.1. Same two-route result as gRPC contract pair (B12-0B-002-001).
Webhooks, gRPC contracts, data contracts all share this routing pattern.

---

## B12-0B-003-002 — [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4 (webhook with auth)

```
source_id: xiigen-b12-0b-003-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  user_request: "Document the webhook contract for shipping notifications including authentication header requirements and payload schema."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
    - predicate: "SECURITY"     → "3.4" (SecurityAuditFlow34)
  TotalDecisionCount: 3
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

Note: "authentication" fires SECURITY → adds 3.4 to routing. TransitionType stays documentation
(no IMPLEMENT fires). SECURITY and API_CONTRACT can coexist in the same request.

---

## B12-0B-003-003 — [API_CONTRACT, DOCUMENT] routing: "integrate" fires API_CONTRACT not IMPLEMENT

```
source_id: xiigen-b12-0b-003-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  user_request: "Describe how to integrate the webhook for refund status events and document the expected response codes."

B:
  RoutingDecisions:
    - predicate: "API_CONTRACT" → "3.3" (APIContractFlow33)
    - predicate: "DOCUMENT"     → "2.1" (DocumentationFlow21)
  TotalDecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]

Routing note: "integrate" fires API_CONTRACT (Core Rule 17) → routes to 3.3, NOT 4.3.
IMPLEMENT does NOT fire on "integrate". No Phase 4.3 routing here.
This is a common routing trap — "integrate webhook" sounds like implementation but is API boundary work.

---

## B12-0B-003-004 — Webhook routing: all three patterns compared

```
source_id: xiigen-b12-0b-003-004
pair_kind: router_reference
```

Webhook routing patterns in B12:

Pattern 1: Webhook contract specification (no security)
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: 3.3 + 2.1
  TransitionType: documentation
  Trigger: "webhook", "contract", "specify/document/define"

Pattern 2: Webhook contract with authentication header
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Routing: 3.3 + 2.1 + 3.4
  TransitionType: documentation
  Trigger: "webhook", "contract", "authentication"

Pattern 3: Webhook integration description
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: 3.3 + 2.1
  TransitionType: documentation
  Trigger: "integrate" (→ API_CONTRACT), "webhook", "document/describe"
  Note: "integrate" → 3.3, NOT 4.3

Pattern 4: Webhook contract implementation (new webhook endpoint)
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  Routing: 4.3 + 3.3 + 2.1
  TransitionType: feature_addition
  Trigger: "implement/build/create", "webhook", "contract"
