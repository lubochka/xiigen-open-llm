# Layer 0-B — Router Pairs (029)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-029-001 — Bulk Import API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-029-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the bulk import API contract for the product catalog ingestion service."

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
    "api" + "contract" → API_CONTRACT → 3.3 ✓
    "define" → DOCUMENT → 2.1 ✓
    DecisionCount = 2 ✓

---

## B12-0B-029-002 — Bulk Import API routing: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-029-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the bulk import endpoint for the product catalog and create the request schema."

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
    "endpoint" → API_CONTRACT → 3.3 ✓
    TransitionType = feature_addition ✓
    No DOCUMENT → 2.1 NOT included ✓

---

## B12-0B-029-003 — Export API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-029-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the export API contract for the reporting service and specify the response payload schema."

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
    "api" + "contract" + "response" (in api context) → API_CONTRACT → 3.3 ✓
    "define"/"specify" → DOCUMENT → 2.1 ✓
    DecisionCount = 2 ✓

---

## B12-0B-029-004 — Export API routing: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-029-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Build the data export endpoint for the analytics API and write the response contract."

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
    "build"/"write" → IMPLEMENT → 4.3 ✓
    "endpoint" + "api" + "response contract" → API_CONTRACT → 3.3 ✓
    TransitionType = feature_addition ✓
    DecisionCount = 2 ✓
