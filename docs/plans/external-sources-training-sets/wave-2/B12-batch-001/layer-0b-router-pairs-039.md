# Layer 0-B — Router Pairs (039)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-039-001 — IdempotencyAPI: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-039-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the idempotency API contract for the payment processing REST endpoints."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "idempotency" + "api" + "contract" + "rest" + "endpoint" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-039-002 — IdempotencyAPI: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-039-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the idempotency key validation for the order creation API endpoint."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "implement" → 4.3 ✓ | "idempotency" + "api" + "endpoint" → 3.3 ✓

---

## B12-0B-039-003 — IdempotencyAPI: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-039-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Expose the idempotency guarantee for the payment submission endpoint in the API contract."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "expose" + "idempotency" + "endpoint" + "api" + "contract" → 3.3 ✓

---

## B12-0B-039-004 — IdempotencyAPI: [API_CONTRACT] → 3.3 only (connect verb)

```
source_id: xiigen-b12-0b-039-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Connect the idempotency enforcement layer to the REST API payment routes."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "connect" + "idempotency" + "rest" + "api" + "routes" → 3.3 ✓
  Note: "connect" is an integration verb → API_CONTRACT only (not IMPLEMENT)
