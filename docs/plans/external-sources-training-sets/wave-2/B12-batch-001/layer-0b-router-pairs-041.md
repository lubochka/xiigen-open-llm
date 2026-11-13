# Layer 0-B — Router Pairs (041)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-041-001 — DeprecatedEndpoint: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-041-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the deprecated endpoint contract for /api/v1/orders and the sunset timeline."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "endpoint" + "contract" + "api" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-041-002 — DeprecatedEndpoint: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-041-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Update the deprecated API endpoints documentation to include Sunset header and migration guide."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "endpoints" → 3.3 ✓ | "update" (spec update) + "documentation" → 2.1 ✓
  Note: "update" in documentation context → DOCUMENT (not IMPLEMENT) → TransitionType = documentation

---

## B12-0B-041-003 — DeprecatedEndpoint: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-041-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Expose the Sunset header on all deprecated REST API endpoints to signal removal date."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "expose" + "rest" + "api" + "endpoints" → 3.3 ✓

---

## B12-0B-041-004 — DeprecatedEndpoint: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-041-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the HTTP 301 redirect from deprecated /api/v1/users to /api/v2/users endpoint."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "implement" → 4.3 ✓ | "api" + "endpoint" → 3.3 ✓
