# Layer 0-B — Router Pairs (034)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-034-001 — Idempotency API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-034-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the idempotent payment submission API contract with idempotency key requirements."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "idempotent"/"idempotency" + "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-034-002 — Idempotency API routing: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-034-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement idempotency on the order creation endpoint and write the idempotency key schema."

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
    "idempotency" + "endpoint" + "schema" → API_CONTRACT → 3.3 ✓
    TransitionType = feature_addition ✓

---

## B12-0B-034-003 — Versioned Endpoint routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-034-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the versioned API endpoint contract for the customer service v2 release."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "endpoint" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-034-004 — Deprecated Endpoint routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-034-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Document the deprecated v1 API endpoints and their replacement contract in the REST API."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check:
    "api" + "endpoints" + "contract" + "rest" → API_CONTRACT → 3.3 ✓
    "document" → DOCUMENT → 2.1 ✓
    No IMPLEMENT — "document" and "deprecated" are not construction signals ✓
    DecisionCount = 2 ✓
