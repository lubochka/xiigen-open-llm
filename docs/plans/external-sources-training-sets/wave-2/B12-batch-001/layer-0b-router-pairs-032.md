# Layer 0-B — Router Pairs (032)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-032-001 — Health Check API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-032-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the health check endpoint contract for the payment service."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "endpoint" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-032-002 — Health Check API routing: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-032-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the health check route for the user service and create the response schema."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder:
    1. parallel(ImplementationFlow43, APIContractFlow33)
    2. QAFlow (always last)
  v6 check:
    "implement"/"create" → IMPLEMENT → 4.3 ✓
    "route" → API_CONTRACT (unconditional) → 3.3 ✓
    TransitionType = feature_addition ✓
    DecisionCount = 2 ✓

---

## B12-0B-032-003 — Health Check API routing: [API_CONTRACT] → 3.3 only (connect verb)

```
source_id: xiigen-b12-0b-032-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Connect the health check API endpoint to the monitoring system."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check:
    "connect" + "api" + "endpoint" → API_CONTRACT → 3.3 ✓
    No documentation verb → 2.1 NOT included ✓
    No construction verb → 4.3 NOT included ✓

---

## B12-0B-032-004 — Routing anti-pattern: DecisionCount collapse warning (v6-03)

```
source_id: xiigen-b12-0b-032-004
pair_kind: router_reference
```

### v6-03 pattern: DecisionCount collapse

The most common v6-03 corruption: collapsing multiple routing decisions into one.

| Request type | Correct DecisionCount | v6-03 wrong DecisionCount |
|-------------|----------------------|--------------------------|
| [API_CONTRACT, DOCUMENT] | 2 | 1 (drops 2.1) |
| [API_CONTRACT, SECURITY] | 2 | 1 (drops 3.4) |
| [IMPLEMENT, API_CONTRACT] | 2 | 1 (drops 3.3) |
| [API_CONTRACT, DOCUMENT, SECURITY] | 3 | 2 or 1 |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | 3 | 2 or 1 |

Rule: DecisionCount MUST equal len(CompiledPredicateIds). Each predicate produces exactly one routing decision.

SECURITY + AUTHORIZATION: both route to 3.4 but still count as TWO separate decisions (DecisionCount = 2 for these two predicates, not 1).
