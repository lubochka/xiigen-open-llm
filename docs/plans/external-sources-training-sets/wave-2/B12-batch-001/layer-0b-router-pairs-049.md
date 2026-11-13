# Layer 0-B — Router Pairs (049)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-049-001 — OpenAPISpec: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-049-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the OpenAPI specification for the customer account REST service endpoints."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "openapi" + "rest" + "endpoints" → 3.3 ✓ | "define" + "specification" → 2.1 ✓

---

## B12-0B-049-002 — OpenAPISpec: [IMPLEMENT, API_CONTRACT, DOCUMENT] → 4.3 + 3.3 + 2.1

```
source_id: xiigen-b12-0b-049-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  UserRequest: "Write the OpenAPI specification YAML for the inventory management REST API and document the schemas."

B:
  Routes: ["4.3", "3.3", "2.1"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    parallel(ImplementationFlow43, APIContractFlow33) → DocumentationFlow21 → QAFlow
  v6 check: "write" → 4.3 ✓ | "openapi" + "rest" + "api" → 3.3 ✓ | "specification" + "document" → 2.1 ✓

---

## B12-0B-049-003 — OpenAPISpec: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-049-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Integrate the OpenAPI specification with the API gateway to enable automatic request validation."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "integrate" + "openapi" + "api" → 3.3 ✓ | "specification" → 2.1 ✓

---

## B12-0B-049-004 — OpenAPISpec: [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY] → 3.1 + 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-049-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Design and specify the OpenAPI contract for the billing service including authentication scheme definitions."

B:
  Routes: ["3.1", "3.3", "2.1", "3.4"]
  PrimarySubPhase: "3.1"
  DecisionCount: 4
  TransitionType: "documentation"
  FlowOrder:
    1. ArchitectureFlow31 (must execute first)
    2. APIContractFlow33
    3. parallel(DocumentationFlow21, SecurityAuditFlow34)
    4. QAFlow (always last)
  v6 check:
    "design" → ARCHITECTURE → 3.1 ✓
    "openapi" + "contract" → API_CONTRACT → 3.3 ✓
    "specify" → DOCUMENT → 2.1 ✓
    "authentication" → SECURITY → 3.4 ✓
    TransitionType = documentation (no IMPLEMENT) ✓
    DecisionCount = 4 ✓
