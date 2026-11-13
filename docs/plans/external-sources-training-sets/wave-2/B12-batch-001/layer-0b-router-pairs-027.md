# Layer 0-B — Router Pairs (027)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-027-001 — WebSocket API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-027-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the WebSocket API contract for the real-time notifications service."

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
    "websocket" + "api" + "contract" → API_CONTRACT → 3.3 ✓
    "define" → DOCUMENT → 2.1 ✓
    DecisionCount = 2 ✓

---

## B12-0B-027-002 — WebSocket API routing: [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-027-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Build the WebSocket connection handler and define the message protocol contract."

B:
  Routes: ["4.3", "3.3", "2.1"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    1. ImplementationFlow43 (parallel with APIContractFlow33)
    2. APIContractFlow33 (parallel with ImplementationFlow43)
    3. DocumentationFlow21 (after parallel flows)
    4. QAFlow (always last)
  v6 check:
    "build" → IMPLEMENT → 4.3 ✓
    "websocket" + "contract" → API_CONTRACT → 3.3 ✓
    "define" → DOCUMENT → 2.1 ✓
    TransitionType = feature_addition ✓
    DecisionCount = 3 ✓

---

## B12-0B-027-003 — WebSocket API routing: [API_CONTRACT] alone → 3.3 only (integrate verb)

```
source_id: xiigen-b12-0b-027-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Integrate the WebSocket endpoint for the chat service with the API gateway."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. QAFlow (always last)
  v6 check:
    "integrate" + "websocket" + "endpoint" + "api" → API_CONTRACT → 3.3 ✓
    No documentation verb → 2.1 NOT included ✓
    No construction verb → 4.3 NOT included ✓
    DecisionCount = 1 ✓

---

## B12-0B-027-004 — WebSocket API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-027-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Document the WebSocket message schema and connection lifecycle contract for the trading platform."

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
    "websocket" + "schema" (with websocket co-occurrence) + "contract" → API_CONTRACT → 3.3 ✓
    "document" → DOCUMENT → 2.1 ✓
    DecisionCount = 2 ✓
