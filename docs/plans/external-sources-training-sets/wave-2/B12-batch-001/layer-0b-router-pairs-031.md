# Layer 0-B — Router Pairs (031)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-031-001 — Filterable Search API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-031-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the filterable search API contract for the product discovery service."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-031-002 — Filterable Search API routing: [IMPLEMENT, API_CONTRACT] → 4.3 + 3.3

```
source_id: xiigen-b12-0b-031-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  UserRequest: "Implement the search endpoint with filter parameters and write the request schema for the products API."

B:
  Routes: ["4.3", "3.3"]
  PrimarySubPhase: "4.3"
  DecisionCount: 2
  TransitionType: "feature_addition"
  FlowOrder:
    1. parallel(ImplementationFlow43, APIContractFlow33)
    2. QAFlow (always last)
  v6 check: "implement"/"write" → IMPLEMENT → 4.3 ✓ | "endpoint"/"schema"/"api" → API_CONTRACT → 3.3 ✓

---

## B12-0B-031-003 — Audit Trail API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-031-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the audit trail API contract for the compliance service and specify the event log schema."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "schema" (with api co-occurrence) → 3.3 ✓ | "define"/"specify" → 2.1 ✓

---

## B12-0B-031-004 — Audit Trail API routing: [API_CONTRACT, SECURITY] → 3.3 + 3.4

```
source_id: xiigen-b12-0b-031-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY"]
  UserRequest: "Expose the audit trail API endpoint with authentication requirements for the compliance team."

B:
  Routes: ["3.3", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33 (parallel with SecurityAuditFlow34)
    2. SecurityAuditFlow34 (parallel with APIContractFlow33)
    3. QAFlow (always last)
  v6 check:
    "expose" + "api" + "endpoint" → API_CONTRACT → 3.3 ✓
    "authentication" → SECURITY (Core Rule 6: unconditional) → 3.4 ✓
    No IMPLEMENT → 4.3 NOT included ✓
    No DOCUMENT → 2.1 NOT included ✓
    DecisionCount = 2 ✓

Anti-pattern check:
  WRONG: Routes: ["3.3"], DecisionCount: 1
  Reason: "authentication" always fires SECURITY → 3.4 must be included.
  v6-02 corruption: treating "authentication" in API context as not-SECURITY.
