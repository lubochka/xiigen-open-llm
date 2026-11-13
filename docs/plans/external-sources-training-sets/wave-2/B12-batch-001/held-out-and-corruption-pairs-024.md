# Held-Out and Corruption Pairs (024)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-024 — Held-out: Complete predicate rule stress test — five-predicate request

```
source_id: xiigen-b12-ho-024
pair_kind: held_out
hold_out_reason: "Stress-tests full predicate detection on a request with ARCHITECTURE + API_CONTRACT + SECURITY + IMPLEMENT + DOCUMENT all present"
```

A:
  user_request: "Design and implement a secure REST API contract for the payment gateway, including authentication, OAuth2 endpoints, and write the OpenAPI specification."

B (expected):
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "SECURITY", "IMPLEMENT", "DOCUMENT"]
  Routing: ["3.1", "3.3", "3.4", "3.2", "2.1"]
  TransitionType: "feature_addition"
  DecisionCount: 5
  Trigger summary:
    "design" → ARCHITECTURE
    "REST" + "api" + "contract" + "endpoints" + "OpenAPI" → API_CONTRACT
    "authentication" + "OAuth2" → SECURITY
    "implement" + "write" → IMPLEMENT
    "specification" → DOCUMENT

Held-out evaluation:
  Pass: All five predicates present; DecisionCount = 5; TransitionType = "feature_addition"
  Fail: Any predicate missing or DecisionCount < 5

---

## B12-COR-024 — Corruption: ARCHITECTURE suppressed on five-predicate request

```
source_id: xiigen-b12-cor-024
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-024

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY", "IMPLEMENT", "DOCUMENT"]  ← ARCHITECTURE missing
  DecisionCount: 4  ← wrong (should be 5)
  CorruptReason: "v6-01: classifier dominated by REST/API/contract signals and treated 'design' as description rather than ARCHITECTURE trigger; 'design' always fires ARCHITECTURE"

Correct: ["ARCHITECTURE", "API_CONTRACT", "SECURITY", "IMPLEMENT", "DOCUMENT"], DecisionCount: 5
Corrupt: ["API_CONTRACT", "SECURITY", "IMPLEMENT", "DOCUMENT"], DecisionCount: 4

---

## B12-COR-024B — Corruption: Summary of all v6 corruption types in B12

```
source_id: xiigen-b12-cor-024b
corruption_type: reference
pair_kind: corruption
```

Reference: B12 Corruption Type Summary

v6-01 variants (predicate suppression / wrong predicate):
  - API_CONTRACT suppressed by domain modifier (order, inventory, shipping, etc.)
  - "database schema" wrongly fires API_CONTRACT (schema without API co-occurrence)
  - "publish" wrongly fires IMPLEMENT
  - "update" wrongly fires IMPLEMENT
  - "spec" wrongly fires API_CONTRACT (spec fires DOCUMENT only)
  - "integration" fires API_CONTRACT without API co-occurrence
  - gRPC not recognised as API term
  - "service contract" modifier wrongly suppresses API_CONTRACT
  - "interface" fires ARCHITECTURE instead of API_CONTRACT in API context
  - ARCHITECTURE suppressed when "design" + API terms both present
  - IMPLEMENT suppressed when construction verb present alongside documentation clause
  - SECURITY suppressed when "authentication" appears in API documentation context

v6-02 variants (SECURITY suppression):
  - authentication in API context not triggering SECURITY
  - OAuth2 not recognised as SECURITY trigger

v6-03 variants (DecisionCount collapse):
  - N predicates collapsed to N-1 routing decisions (DOCUMENT routing dropped most commonly)
  - IMPLEMENT and API_CONTRACT collapsed to sequential instead of parallel execution
  - Three-predicate sets collapsed to two routing decisions

Correct baseline for B12: Every predicate that fires produces exactly one routing decision. DecisionCount = len(CompiledPredicateIds).
