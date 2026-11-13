# Layer 0-B — Router Pairs (040)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-040-001 — VersionedEndpoint: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-040-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the versioned endpoint contract for the customer profile REST API v2."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "endpoint" + "contract" + "rest" + "api" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-040-002 — VersionedEndpoint: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-040-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Build the versioned endpoint /api/v2/users and implement the version negotiation contract."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "build" + "implement" → 4.3 ✓ | "endpoint" + "api" + "contract" → 3.3 ✓

---

## B12-0B-040-003 — VersionedEndpoint: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-040-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Wire up the versioned API routing so /api/v1 and /api/v2 both resolve correctly."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "wire up" + "api" → 3.3 ✓ | "wire up" = integration verb (not IMPLEMENT)

---

## B12-0B-040-004 — VersionedEndpoint: [ARCHITECTURE, API_CONTRACT] → 3.1 + 3.3

```
source_id: xiigen-b12-0b-040-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT"]
  UserRequest: "Design the versioned API strategy with header-based and URI-based version negotiation."

B:
  Routes: ["3.1", "3.3"]
  PrimarySubPhase: "3.1"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder:
    1. ArchitectureFlow31 (must execute first)
    2. APIContractFlow33
    3. QAFlow (always last)
  v6 check:
    "design" → ARCHITECTURE → 3.1 ✓
    "api" → API_CONTRACT → 3.3 ✓
    TransitionType = documentation (no IMPLEMENT) ✓
    DecisionCount = 2 ✓
