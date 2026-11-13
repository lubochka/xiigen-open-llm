# Layer 0-B — Router Pairs (035)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-035-001 — API Key Management routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-035-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the API key management contract for the developer portal."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-035-002 — API Key Management routing: [IMPLEMENT, API_CONTRACT, SECURITY] → 4.3 + 3.3 + 3.4

```
source_id: xiigen-b12-0b-035-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
  UserRequest: "Implement API key authentication for the partner integration endpoints and write the key validation schema."

B:
  Routes: ["4.3", "3.3", "3.4"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    1. parallel(ImplementationFlow43, APIContractFlow33, SecurityAuditFlow34)
    2. QAFlow (always last)
  v6 check:
    "implement"/"write" → IMPLEMENT → 4.3 ✓
    "api" + "endpoints" + "schema" → API_CONTRACT → 3.3 ✓
    "authentication" → SECURITY (Core Rule 6: unconditional) → 3.4 ✓
    TransitionType = feature_addition ✓
    DecisionCount = 3 ✓

Anti-pattern check:
  WRONG: Routes: ["4.3", "3.3"], DecisionCount: 2
  Reason: "authentication" always fires SECURITY → 3.4 must be included.
  The v6-02 error is treating authentication in an API implementation context as non-SECURITY.

---

## B12-0B-035-003 — OAuth Scope API routing: [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-035-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Define the OAuth scope contract for the customer data API and document the authentication requirements."

B:
  Routes: ["3.3", "2.1", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. parallel(DocumentationFlow21, SecurityAuditFlow34)
    3. QAFlow (always last)
  v6 check:
    "oauth" + "contract" + "api" → API_CONTRACT → 3.3 ✓
    "define"/"document" → DOCUMENT → 2.1 ✓
    "oauth" + "authentication" → SECURITY → 3.4 ✓
    TransitionType = documentation (no IMPLEMENT) ✓
    DecisionCount = 3 ✓

---

## B12-0B-035-004 — Tenant-Scoped API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-035-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the tenant-scoped API contract for the multi-tenant SaaS platform."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓ | DecisionCount = 2 ✓
