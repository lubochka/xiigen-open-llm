# Layer 0-B — Router Pairs (045)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-045-001 — MultiVersionAPI: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-045-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Specify the multi-version API routing contract for the order management service endpoints."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "routing" + "endpoints" → 3.3 ✓ | "specify" → 2.1 ✓

---

## B12-0B-045-002 — MultiVersionAPI: [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-045-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Build multi-version API support for the payments service and document the version selection contract."

B:
  Routes: ["4.3", "3.3", "2.1"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    parallel(ImplementationFlow43, APIContractFlow33) → DocumentationFlow21 → QAFlow
  v6 check: "build" → 4.3 ✓ | "api" + "contract" → 3.3 ✓ | "document" → 2.1 ✓

---

## B12-0B-045-003 — MultiVersionAPI: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-045-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Wire up multi-version API routing to direct v1 and v2 requests to the correct handlers."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "wire up" + "api" + "routing" → 3.3 ✓ | "wire up" = integration verb (not IMPLEMENT)

---

## B12-0B-045-004 — MultiVersionAPI: [ARCHITECTURE, API_CONTRACT, DOCUMENT] → 3.1 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-045-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Design the multi-version API strategy and define the backward compatibility contract for the user service."

B:
  Routes: ["3.1", "3.3", "2.1"]
  PrimarySubPhase: "3.1"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. ArchitectureFlow31 (must execute first)
    2. APIContractFlow33
    3. DocumentationFlow21
    4. QAFlow
  v6 check: "design" → 3.1 ✓ | "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓
