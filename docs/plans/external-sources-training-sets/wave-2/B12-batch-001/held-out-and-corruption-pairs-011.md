# Held-Out and Corruption Pairs (011)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-011-v601 — Held-out for v6-01: "event contracts" fires API_CONTRACT

```
source_id: xiigen-b12-ho-011-v601
pair_kind: held_out
corruption_target: v6-01
hold_out_reason: "Primary held-out test for v6-01 corruption — event contracts"
```

A:
  user_request: "Specify the event contracts for the customer lifecycle events published by the CRM service."

B (expected — correct behavior):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  TransitionType: "documentation"

v6-01 detection:
  If output is: CompiledPredicateIds: ["DOCUMENT"]
  → v6-01 corruption detected (API_CONTRACT suppressed on "event" modifier of "contracts")
  → FAIL

---

## B12-HO-011-v602 — Held-out for v6-02: "authentication" fires SECURITY in spec context

```
source_id: xiigen-b12-ho-011-v602
pair_kind: held_out
corruption_target: v6-02
hold_out_reason: "Primary held-out test for v6-02 corruption — auth in spec context"
```

A:
  user_request: "Annotate the REST API specification with authentication and token requirements."

B (expected — correct behavior):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Routing: ["3.3", "2.1", "3.4"]
  TransitionType: "documentation"

v6-02 detection:
  If output is: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  → v6-02 corruption detected (SECURITY suppressed when "authentication" in API spec context)
  → FAIL

---

## B12-HO-011-v603 — Held-out for v6-03: five predicates → five routing decisions

```
source_id: xiigen-b12-ho-011-v603
pair_kind: held_out
corruption_target: v6-03
hold_out_reason: "Primary held-out test for v6-03 corruption — routing collapse"
```

A:
  user_request: "Write the OpenAPI specification for the authentication service, implement the OAuth 2.0 endpoints, and document the security scopes."

B (expected — correct behavior):
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  RoutingDecisions: [4.3, 3.3, 2.1, 3.4, 3.4]
  DecisionCount: 5
  TransitionType: "feature_addition"

v6-03 detection:
  If output is: RoutingDecisions: [3.3] only, DecisionCount: 1
  → v6-03 corruption detected (routing collapse to single 3.3 decision)
  → FAIL

---

## B12-COR-011-summary — All three v6 corruptions for B12 consolidated

```
source_id: xiigen-b12-cor-011-summary
pair_kind: corruption_reference
```

B12 v6 Corruption Detection and Repair Summary:

v6-01: API_CONTRACT suppressed on contract modifier
  Detection: "contract" in request + "API_CONTRACT" NOT in CompiledPredicateIds
  Repair: add "API_CONTRACT" to CompiledPredicateIds
  Rule violated: Core Rule 2 (contract fires always)
  Affected modifiers: data, service, message, event, integration, protocol, response

v6-02: SECURITY suppressed in API documentation context
  Detection: "authentication" or "token" in request + "SECURITY" NOT in CompiledPredicateIds
  Repair: add "SECURITY" to CompiledPredicateIds
  Rule violated: Core Rule 6 (authentication → SECURITY always)
  Affected contexts: OpenAPI spec context, gRPC contract context, webhook doc context

v6-03: Multiple routing decisions collapsed to single 3.3
  Detection: COUNT(CompiledPredicateIds) > COUNT(RoutingDecisions)
  Repair: generate one RoutingDecision per predicate in CompiledPredicateIds
  Rule violated: Core Rule 23 (DecisionCount = predicate count, each routes independently)
  Affected scenario: any multi-predicate request in B12

All three corruptions are addressed by B12-batch-001 training data:
  v6-01: Layer 0-A 004, TU-002, HO-002, HO-010, HO-011
  v6-02: Layer 0-A 007, 011, TU-005, HO-005, HO-011
  v6-03: Layer 0-A 010, Layer 0-B 010, 011, TU-008, HO-011
