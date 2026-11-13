# Layer 0-B — Router Pairs (043)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-043-001 — OAuthScopeAPI: [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-043-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Define the OAuth scope API contract for the customer data access endpoints."

B:
  Routes: ["3.3", "2.1", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. parallel(DocumentationFlow21, SecurityAuditFlow34)
    3. QAFlow
  v6 check: "api" + "contract" + "endpoints" → 3.3 ✓ | "define" → 2.1 ✓ | "oauth" → 3.4 ✓

---

## B12-0B-043-002 — OAuthScopeAPI: [IMPLEMENT, API_CONTRACT, SECURITY] → 4.3 + 3.3 + 3.4

```
source_id: xiigen-b12-0b-043-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
  UserRequest: "Implement the OAuth scope validation middleware for the partner REST API endpoints."

B:
  Routes: ["4.3", "3.3", "3.4"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    parallel(ImplementationFlow43, APIContractFlow33, SecurityAuditFlow34) → QAFlow
  v6 check: "implement" → 4.3 ✓ | "rest" + "api" + "endpoints" → 3.3 ✓ | "oauth" → 3.4 ✓

---

## B12-0B-043-003 — OAuthScopeAPI: [API_CONTRACT, SECURITY] → 3.3 + 3.4

```
source_id: xiigen-b12-0b-043-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY"]
  UserRequest: "Connect the OAuth scope enforcement to the API gateway for all protected endpoints."

B:
  Routes: ["3.3", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: parallel(APIContractFlow33, SecurityAuditFlow34) → QAFlow
  v6 check: "connect" + "api" + "endpoints" → 3.3 ✓ | "oauth" → 3.4 ✓

---

## B12-0B-043-004 — OAuthScopeAPI: [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-043-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Specify the OAuth token scope requirements for each endpoint in the OpenAPI specification."

B:
  Routes: ["3.3", "2.1", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. parallel(DocumentationFlow21, SecurityAuditFlow34)
    3. QAFlow
  v6 check:
    "endpoint" + "openapi" → API_CONTRACT → 3.3 ✓
    "specify" + "specification" → DOCUMENT → 2.1 ✓
    "oauth" → SECURITY → 3.4 ✓
    TransitionType = documentation (no IMPLEMENT) ✓
