# Layer 0-B — Router Pairs (033)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-033-001 — Rate Limiting API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-033-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the rate limiting API contract for the public API gateway."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-033-002 — Rate Limiting API routing: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-033-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement rate limiting on the REST API endpoints and write the rate limit response schema."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder:
    1. parallel(ImplementationFlow43, APIContractFlow33)
    2. QAFlow (always last)
  v6 check:
    "implement"/"write" → IMPLEMENT → 4.3 ✓
    "rest" + "api" + "endpoints" + "response" + "schema" → API_CONTRACT → 3.3 ✓
    TransitionType = feature_addition ✓

---

## B12-0B-033-003 — Caching API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-033-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the caching layer API contract for the product catalog service."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-033-004 — Caching API routing: [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-033-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Build the caching middleware for the product API and document the cache-control contract."

B:
  Routes: ["4.3", "3.3", "2.1"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    1. parallel(ImplementationFlow43, APIContractFlow33)
    2. DocumentationFlow21
    3. QAFlow (always last)
  v6 check:
    "build" → IMPLEMENT → 4.3 ✓
    "api" + "contract" → API_CONTRACT → 3.3 ✓
    "document" → DOCUMENT → 2.1 ✓
    TransitionType = feature_addition ✓
    DecisionCount = 3 ✓
