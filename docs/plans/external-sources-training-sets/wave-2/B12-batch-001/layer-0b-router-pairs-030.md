# Layer 0-B — Router Pairs (030)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-030-001 — Async Callback API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-030-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the async callback API contract for the payment processing service."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. DocumentationFlow21
    3. QAFlow (always last)
  v6 check: "api" + "contract" → API_CONTRACT → 3.3 ✓ | "define" → DOCUMENT → 2.1 ✓

---

## B12-0B-030-002 — Async Callback API routing: [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-030-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Implement the callback endpoint handler and define the callback request schema."

B:
  Routes: ["4.3", "3.3", "2.1"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    1. parallel(ImplementationFlow43, APIContractFlow33)
    2. DocumentationFlow21
    3. QAFlow (always last)
  v6 check: "implement" → IMPLEMENT → 4.3 ✓ | "endpoint" + "schema" → API_CONTRACT → 3.3 ✓ | "define" → DOCUMENT → 2.1 ✓

---

## B12-0B-030-003 — Paginated List API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-030-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the paginated list endpoint contract for the customer accounts API."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. DocumentationFlow21
    3. QAFlow (always last)
  v6 check: "endpoint" + "contract" + "api" → API_CONTRACT → 3.3 ✓ | "define" → DOCUMENT → 2.1 ✓

---

## B12-0B-030-004 — Paginated List API routing: [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-030-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Build the paginated list endpoint for the products API and document the cursor contract."

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
    "endpoint" + "api" + "contract" → API_CONTRACT → 3.3 ✓
    "document" → DOCUMENT → 2.1 ✓
    TransitionType = feature_addition ✓
    DecisionCount = 3 ✓
