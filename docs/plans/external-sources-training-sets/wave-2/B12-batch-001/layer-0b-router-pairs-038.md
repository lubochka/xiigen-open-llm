# Layer 0-B — Router Pairs (038)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-038-001 — CachingAPI: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-038-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the caching API contract for the product catalog REST service response headers."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "rest" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-038-002 — CachingAPI: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-038-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the cache-control headers for the product API endpoints and write the ETag contract."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "implement" + "write" → 4.3 ✓ | "api" + "contract" + "endpoint" → 3.3 ✓

---

## B12-0B-038-003 — CachingAPI: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-038-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Specify the API cache invalidation contract for the order service when product prices change."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "specify" → 2.1 ✓

---

## B12-0B-038-004 — CachingAPI: [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-038-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Design the caching API architecture and document the cache-control header contract for service responses."

B:
  Routes: ["3.1", "3.3", "2.1"]
  PrimarySubPhase: "3.1"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. ArchitectureFlow31 (must execute first)
    2. APIContractFlow33
    3. DocumentationFlow21
    4. QAFlow (always last)
  v6 check:
    "design" → ARCHITECTURE → 3.1 ✓
    "api" + "contract" → API_CONTRACT → 3.3 ✓
    "document" → DOCUMENT → 2.1 ✓
    TransitionType = documentation (no IMPLEMENT) ✓
    DecisionCount = 3 ✓
