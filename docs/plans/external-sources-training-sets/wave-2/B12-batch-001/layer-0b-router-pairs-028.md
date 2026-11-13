# Layer 0-B — Router Pairs (028)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-028-001 — Event Streaming API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-028-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the event streaming API contract for the order events topic."

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
    "api" + "contract" (event contract) → API_CONTRACT → 3.3 ✓
    "define" → DOCUMENT → 2.1 ✓
    DecisionCount = 2 ✓

---

## B12-0B-028-002 — Event Streaming API routing: [API_CONTRACT] alone → 3.3 only (publish schema)

```
source_id: xiigen-b12-0b-028-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Publish the event schema for the payment events API to the event catalog."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. QAFlow (always last)
  v6 check:
    "publish" + "schema" (with api co-occurrence) + "api" → API_CONTRACT → 3.3 ✓
    "publish" ≠ IMPLEMENT → 4.3 NOT included ✓
    No documentation verb → 2.1 NOT included ✓
    DecisionCount = 1 ✓

---

## B12-0B-028-003 — Event Streaming API routing: [API_CONTRACT] alone → 3.3 only (wire up)

```
source_id: xiigen-b12-0b-028-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Wire up the event streaming endpoint for the inventory service to the Kafka topic contract."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. QAFlow (always last)
  v6 check:
    "wire up" + "endpoint" + "contract" → API_CONTRACT → 3.3 ✓
    No documentation verb → 2.1 NOT included ✓
    No construction verb → 4.3 NOT included ✓
    DecisionCount = 1 ✓

---

## B12-0B-028-004 — Event Streaming API routing: [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-028-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Build the event producer API for the order service and define the event contract schema."

B:
  Routes: ["4.3", "3.3", "2.1"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    1. ImplementationFlow43 (parallel with APIContractFlow33)
    2. APIContractFlow33 (parallel with ImplementationFlow43)
    3. DocumentationFlow21
    4. QAFlow (always last)
  v6 check:
    "build" → IMPLEMENT → 4.3 ✓
    "api" + "event contract" + "schema" (with api co-occurrence) → API_CONTRACT → 3.3 ✓
    "define" → DOCUMENT → 2.1 ✓
    TransitionType = feature_addition ✓
    DecisionCount = 3 ✓
