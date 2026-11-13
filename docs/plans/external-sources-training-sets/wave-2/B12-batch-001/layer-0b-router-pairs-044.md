# Layer 0-B — Router Pairs (044)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-044-001 — TenantScopedAPI: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-044-001
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
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-044-002 — TenantScopedAPI: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-044-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement tenant isolation in the REST API endpoints using the X-Tenant-ID header."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder: parallel(ImplementationFlow43, APIContractFlow33) → QAFlow
  v6 check: "implement" → 4.3 ✓ | "rest" + "api" + "endpoints" → 3.3 ✓

---

## B12-0B-044-003 — TenantScopedAPI: [API_CONTRACT] → 3.3 only

```
source_id: xiigen-b12-0b-044-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT"]
  UserRequest: "Expose the tenant-scoped API contract to external partners via the developer portal."

B:
  Routes: ["3.3"]
  PrimarySubPhase: "3.3"
  DecisionCount: 1
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → QAFlow
  v6 check: "expose" + "api" + "contract" → 3.3 ✓

---

## B12-0B-044-004 — TenantScopedAPI: [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-044-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Define the tenant-scoped API contract with authentication and tenant-level authorization policies."

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
