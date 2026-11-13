# Layer 0-B — Router Pairs (011)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-011-v6-01 — v6-03 corruption: three predicates collapsed to single 3.3 routing decision

```
source_id: xiigen-b12-0b-011-v601
corruption_type: v6-03
pair_kind: router_corruption
```

Request: "Write the OpenAPI specification for the authentication service, implement the OAuth 2.0
token endpoint, and document the security requirements."

Correct routing:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  RoutingDecisions (5 total):
    - IMPLEMENT → 4.3
    - API_CONTRACT → 3.3
    - DOCUMENT → 2.1
    - SECURITY → 3.4
    - AUTHORIZATION → 3.4
  DecisionCount: 5
  FlowOrder: [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34]

v6-03 corrupt routing:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  (predicates correctly detected)
  RoutingDecisions (1 total — COLLAPSED):
    - API_CONTRACT → 3.3 (all other predicates absorbed into this single decision)
  DecisionCount: 1
  FlowOrder: [APIContractFlow33]

Why v6-03 is wrong:
  Each predicate independently routes to its destination.
  API_CONTRACT being the "primary" predicate for B12 does not mean other predicates are suppressed.
  Five predicates → five routing decisions.
  The router must generate one RoutingDecision entry per predicate, not collapse to the primary.

Detection pattern: If CompiledPredicateIds has N>1 predicates but RoutingDecisions has only 1 entry,
this is a v6-03 routing collapse corruption.

---

## B12-0B-011-v6-02 — v6-01 routing effect: wrong sub-phase from suppressed API_CONTRACT

```
source_id: xiigen-b12-0b-011-v602
corruption_type: v6-01
pair_kind: router_corruption
```

Request: "Define the data contracts between the order service and the fulfillment service."

Correct routing:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  RoutingDecisions:
    - API_CONTRACT → 3.3
    - DOCUMENT → 2.1
  DecisionCount: 2
  PrimarySubPhase: 3.3

v6-01 corrupt routing:
  CompiledPredicateIds: ["DOCUMENT"]  (API_CONTRACT incorrectly suppressed)
  RoutingDecisions:
    - DOCUMENT → 2.1 (only)
  DecisionCount: 1
  PrimarySubPhase: 2.1 (WRONG — should be 3.3)

Why this matters: The v6-01 corruption (suppressing API_CONTRACT on "contract") directly
causes routing to miss Phase 3.3 entirely. The request lands in Phase 2.1 (documentation)
instead of Phase 3.3 (API contract design). This is a sub-phase routing failure.

---

## B12-0B-011-v6-03 — v6-02 routing effect: missing 3.4 from suppressed SECURITY

```
source_id: xiigen-b12-0b-011-v603
corruption_type: v6-02
pair_kind: router_corruption
```

Request: "Write the OpenAPI spec for the cart service and include authentication requirements."

Correct routing:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  RoutingDecisions:
    - IMPLEMENT → 4.3
    - API_CONTRACT → 3.3
    - DOCUMENT → 2.1
    - SECURITY → 3.4
  DecisionCount: 4

v6-02 corrupt routing:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]  (SECURITY suppressed)
  RoutingDecisions:
    - IMPLEMENT → 4.3
    - API_CONTRACT → 3.3
    - DOCUMENT → 2.1
  DecisionCount: 3

Why this matters: The v6-02 corruption (suppressing SECURITY for "authentication" in API docs)
causes 3.4 routing to be missed entirely. Security review of the API is not scheduled.
Authentication concerns in the specification are not flagged for security audit.

---

## B12-0B-011-summary — B12 v6 corruption routing impact table

```
source_id: xiigen-b12-0b-011-summary
pair_kind: router_reference
```

| v6 Version | Corruption | Routing impact | Sub-phase missed |
|------------|------------|----------------|------------------|
| v6-01 | API_CONTRACT suppressed on "contract" modifier | 3.3 missing from routing | 3.3 (API Contract) |
| v6-02 | SECURITY suppressed in API doc context | 3.4 missing from routing | 3.4 (Security) |
| v6-03 | Multiple routing decisions collapsed to 3.3 | Only 3.3 routing decision present | 4.3, 2.1, 3.4 all missed |

Detection signals:
  v6-01: Request has "contract" but RoutingDecisions lacks 3.3 entry
  v6-02: Request has "authentication/token" but RoutingDecisions lacks 3.4 entry
  v6-03: CompiledPredicateIds has N>1 but RoutingDecisions has 1 entry pointing to 3.3
