# Layer 0-B — Router Pairs (057)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-057-001 — SDKGeneration: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-057-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the SDK generation contract from the OpenAPI specification for the partner API."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "contract" + "openapi" + "api" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-057-002 — SDKGeneration: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-057-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Build the TypeScript SDK from the REST API contract and write the client wrapper classes."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "build" + "write" → 4.3 ✓ | "rest" + "api" + "contract" → 3.3 ✓

---

## B12-0B-057-003 — SDKGeneration: [API_CONTRACT] → 3.3 only (publish = not IMPLEMENT)

```
source_id: xiigen-b12-0b-057-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Publish the auto-generated SDK to the partner developer portal alongside the API contract."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "publish" + "api" + "contract" → 3.3 ✓
  Note: "publish" is a release verb per B12-R21 — does NOT fire IMPLEMENT

---

## B12-0B-057-004 — SDKGeneration: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-057-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Integrate SDK generation into the CI pipeline and document the API specification versioning contract."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "integrate" + "api" + "contract" → 3.3 ✓ | "specification" + "document" → 2.1 ✓
