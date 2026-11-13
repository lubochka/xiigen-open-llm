# Layer 0-B — Router Pairs (036)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-036-001 — Multi-Version API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-036-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the multi-version API contract strategy for the payments REST service."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "rest" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-036-002 — Backward Compat API routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-036-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the backward compatible API contract for the user service upgrade from v1 to v2."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-036-003 — API Doc Generator routing: [API_CONTRACT, DOCUMENT] → 3.3 + 2.1

```
source_id: xiigen-b12-0b-036-003
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  UserRequest: "Define the API documentation contract for auto-generating the SDK reference from the REST endpoints."

B:
  Routes: ["3.3", "2.1"]
  PrimarySubPhase: "3.3"
  DecisionCount: 2
  TransitionType: "documentation"
  FlowOrder: APIContractFlow33 → DocumentationFlow21 → QAFlow
  v6 check: "api" + "contract" + "rest" + "endpoints" → 3.3 ✓ | "define" → 2.1 ✓

---

## B12-0B-036-004 — API_CONTRACT + SECURITY routing: [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-036-004
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Define the API contract and security requirements for the payment gateway authentication endpoints."

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
    "api" + "contract" + "endpoints" → API_CONTRACT → 3.3 ✓
    "define" → DOCUMENT → 2.1 ✓
    "authentication" → SECURITY (Core Rule 6: unconditional) → 3.4 ✓
    TransitionType = documentation (no IMPLEMENT) ✓
    DecisionCount = 3 ✓

Round 2 routing summary (files 025–036):
  All 12 files demonstrate routing correctness for new API topic areas.
  Key patterns:
    - API_CONTRACT alone → 3.3 only (integration verbs: wire up, connect, publish, expose)
    - API_CONTRACT + DOCUMENT → 3.3 + 2.1
    - API_CONTRACT + SECURITY → 3.3 + 3.4 (authentication always fires SECURITY)
    - IMPLEMENT + API_CONTRACT → 4.3 + 3.3 (feature_addition)
    - IMPLEMENT + API_CONTRACT + DOCUMENT → 4.3 + 3.3 + 2.1
    - API_CONTRACT + DOCUMENT + SECURITY → 3.3 + 2.1 + 3.4
