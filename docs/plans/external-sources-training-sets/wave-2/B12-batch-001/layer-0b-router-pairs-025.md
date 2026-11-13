# Layer 0-B — Router Pairs (025)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-025-001 — GraphQL API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-025-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the GraphQL API schema for the product catalog service."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33 (primary B12 flow)
    2. DocumentationFlow21 (documentation post-processing)
    3. QAFlow (always last)
  v6 check:
    "graphql" → API_CONTRACT present → 3.3 included ✓
    "define" → DOCUMENT present → 2.1 included ✓
    DecisionCount = 2 → matches len(["API_CONTRACT", "DOCUMENT"]) = 2 ✓

---

## B12-0B-025-002 — GraphQL API routing: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-025-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the GraphQL schema for the product catalog and write the query resolvers."

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
    "implement" → IMPLEMENT present → 4.3 included ✓
    "graphql" → API_CONTRACT present → 3.3 included ✓
    TransitionType = feature_addition (IMPLEMENT fires) ✓
    Parallel execution: IMPLEMENT ∥ API_CONTRACT ✓

---

## B12-0B-025-003 — GraphQL API routing: [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-025-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Design the GraphQL API for the product catalog service and specify the query types."

B:
  Routes: ["3.1", "3.3", "2.1"]
  PrimarySubPhase: "3.1"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. ArchitectureFlow31 (must execute first — informs API contract decisions)
    2. APIContractFlow33 (uses architecture decisions)
    3. DocumentationFlow21 (documentation post-processing)
    4. QAFlow (always last)
  v6 check:
    "design" → ARCHITECTURE present → 3.1 included ✓
    "graphql" → API_CONTRACT present → 3.3 included ✓
    "specify" → DOCUMENT present → 2.1 included ✓
    FlowOrder: ARCHITECTURE first (tier 0) ✓
    TransitionType = documentation (no IMPLEMENT) ✓

---

## B12-0B-025-004 — GraphQL API routing: [API_CONTRACT] alone → 3.3 only

```
source_id: xiigen-b12-0b-025-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Wire up the GraphQL endpoint for the storefront to the product catalog service."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. QAFlow (always last)
  v6 check:
    "wire up" + "graphql" + "endpoint" → API_CONTRACT present → 3.3 included ✓
    No DOCUMENT verb → 2.1 NOT included ✓
    No IMPLEMENT verb → 4.3 NOT included ✓
    DecisionCount = 1 → matches len(["API_CONTRACT"]) = 1 ✓
