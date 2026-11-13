# Layer 0-B — Router Pairs (058)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-058-001 — MockServerAPI: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-058-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the mock server API contract for the payment gateway to enable frontend development in parallel."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-058-002 — MockServerAPI: [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-058-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Build the mock server from the OpenAPI specification and implement the stub response handlers."

B:
  Routes: ["4.3", "3.3", "2.1"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    parallel(ImplementationFlow43, APIContractFlow33) → DocumentationFlow21 → QAFlow
  v6 check: "build" + "implement" → 4.3 ✓ | "openapi" → 3.3 ✓ | "specification" → 2.1 ✓
  Note: "specification" fires DOCUMENT independently even when IMPLEMENT is present

---

## B12-0B-058-003 — MockServerAPI: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-058-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Wire the mock server into the integration test environment for API contract validation."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "wire" + "api" + "contract" → 3.3 ✓

---

## B12-0B-058-004 — MockServerAPI: [API_CONTRACT, SECURITY] → 3.3 + 3.4

```
source_id: xiigen-b12-0b-058-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY"]
  UserRequest: "Expose the mock API endpoints with token authentication for the staging test environment."

B:
  Routes: ["3.3", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: parallel(APIContractFlow33, SecurityAuditFlow34) → QAFlow
  v6 check: "expose" + "api" + "endpoints" → 3.3 ✓ | "authentication" → 3.4 ✓
