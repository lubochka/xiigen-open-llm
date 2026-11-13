# Layer 0-B — Router Pairs (042)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-042-001 — APIKeyManagement: [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-042-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Define the API key management contract including authentication, key rotation, and revocation policies."

B:
  Routes: ["3.3", "2.1", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. parallel(DocumentationFlow21, SecurityAuditFlow34)
    3. QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓ | "authentication" → 3.4 ✓

---

## B12-0B-042-002 — APIKeyManagement: [IMPLEMENT, API_CONTRACT, SECURITY] → 4.3 + 3.3 + 3.4

```
source_id: xiigen-b12-0b-042-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
  UserRequest: "Implement the API key generation endpoint with authentication checks and write the key issuance contract."

B:
  Routes: ["4.3", "3.3", "3.4"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    parallel(ImplementationFlow43, APIContractFlow33, SecurityAuditFlow34) → QAFlow
  v6 check:
    "implement" + "write" → IMPLEMENT → 4.3 ✓
    "api" + "endpoint" + "contract" → API_CONTRACT → 3.3 ✓
    "authentication" → SECURITY (unconditional) → 3.4 ✓
    TransitionType = feature_addition (IMPLEMENT present) ✓
    DecisionCount = 3 ✓

---

## B12-0B-042-003 — APIKeyManagement: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-042-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Expose the API key management endpoints in the partner portal dashboard."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "expose" + "api" + "endpoints" → 3.3 ✓

---

## B12-0B-042-004 — APIKeyManagement: [API_CONTRACT, SECURITY] → 3.3 + 3.4

```
source_id: xiigen-b12-0b-042-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY"]
  UserRequest: "Connect the API key authentication layer to all service REST endpoints."

B:
  Routes: ["3.3", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder:
    parallel(APIContractFlow33, SecurityAuditFlow34) → QAFlow
  v6 check:
    "connect" + "api" + "rest" + "endpoints" → API_CONTRACT → 3.3 ✓
    "authentication" → SECURITY (Core Rule 6) → 3.4 ✓
    TransitionType = documentation (no IMPLEMENT) ✓
    DecisionCount = 2 ✓
