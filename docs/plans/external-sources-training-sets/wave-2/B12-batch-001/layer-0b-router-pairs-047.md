# Layer 0-B — Router Pairs (047)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-047-001 — APIDocGenerator: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-047-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the API documentation generation contract for auto-producing reference docs from OpenAPI specs."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "openapi" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-047-002 — APIDocGenerator: [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-047-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Build the API documentation generator that reads OpenAPI specs and writes HTML reference pages."

B:
  Routes: ["4.3", "3.3", "2.1"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    parallel(ImplementationFlow43, APIContractFlow33) → DocumentationFlow21 → QAFlow
  v6 check: "build" + "writes" → 4.3 ✓ | "api" + "openapi" → 3.3 ✓ | "documentation" → 2.1 ✓

---

## B12-0B-047-003 — APIDocGenerator: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-047-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Integrate the API documentation generator with the CI pipeline to publish docs on each release."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "integrate" + "api" → 3.3 ✓ | "documentation" → 2.1 ✓
  Note: "integrate" = API_CONTRACT (not IMPLEMENT)

---

## B12-0B-047-004 — APIDocGenerator: [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-047-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Specify the API documentation contract including authentication flow diagrams and security scheme reference."

B:
  Routes: ["3.3", "2.1", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. parallel(DocumentationFlow21, SecurityAuditFlow34)
    3. QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "specify" → 2.1 ✓ | "authentication" → 3.4 ✓
