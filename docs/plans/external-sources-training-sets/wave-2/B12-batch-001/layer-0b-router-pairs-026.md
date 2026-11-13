# Layer 0-B — Router Pairs (026)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-026-001 — gRPC Service routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-026-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the gRPC service contract for the billing service and specify the RPC methods."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. DocumentationFlow21
    3. QAFlow (always last)
  v6 check:
    "grpc" → API_CONTRACT present → 3.3 included ✓
    "define"/"specify" → DOCUMENT present → 2.1 included ✓
    DecisionCount = 2 ✓

---

## B12-0B-026-002 — gRPC Service routing: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-026-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the gRPC service for the billing module and create the protobuf message types."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder:
    1. ImplementationFlow43 (parallel with APIContractFlow33)
    2. APIContractFlow33 (parallel with ImplementationFlow43)
    3. QAFlow (always last)
  v6 check:
    "implement"/"create" → IMPLEMENT → 4.3 ✓
    "grpc" → API_CONTRACT → 3.3 ✓
    TransitionType = feature_addition ✓

---

## B12-0B-026-003 — gRPC Service routing: [API_CONTRACT] alone → 3.3 only (connect verb)

```
source_id: xiigen-b12-0b-026-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Connect the gRPC billing service to the payment orchestrator service."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. QAFlow (always last)
  v6 check:
    "connect" + "grpc" → API_CONTRACT → 3.3 ✓
    No documentation verb → 2.1 NOT included ✓
    No construction verb → 4.3 NOT included ✓
    DecisionCount = 1 ✓

Anti-pattern check:
  WRONG: Routes: ["3.3", "2.1"], DecisionCount: 2
  Reason: No DOCUMENT verb present in the request. "connect" is not a documentation verb.
  CORRECT: Routes: ["3.3"], DecisionCount: 1

---

## B12-0B-026-004 — gRPC Service routing: [API_CONTRACT] alone → 3.3 only (publish verb)

```
source_id: xiigen-b12-0b-026-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Publish the gRPC endpoint for the billing service to the internal service registry."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. QAFlow (always last)
  v6 check:
    "publish endpoint" + "grpc" → API_CONTRACT → 3.3 ✓
    No documentation verb → 2.1 NOT included ✓
    "publish" ≠ IMPLEMENT → 4.3 NOT included ✓
    DecisionCount = 1 ✓

Teaching note: "publish" is a release verb. It fires API_CONTRACT but not IMPLEMENT.
The anti-pattern is treating "publish" as a construction verb and adding 4.3 to routes.
