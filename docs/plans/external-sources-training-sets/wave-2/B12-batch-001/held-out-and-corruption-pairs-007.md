# Held-Out and Corruption Pairs (007)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-007 — Held-out: gRPC contract → API_CONTRACT + DOCUMENT (not IMPLEMENT)

```
source_id: xiigen-b12-ho-007
pair_kind: held_out
hold_out_reason: "Tests that gRPC contract specification fires API_CONTRACT not IMPLEMENT"
```

A:
  user_request: "Specify the gRPC service contract for the shipping service and document the protobuf message types."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  Routing: ["3.3", "2.1"]
  Reason:
    "gRPC" → API_CONTRACT fires
    "service contract" → API_CONTRACT fires on "contract" (Rule 2)
    "protobuf" → API_CONTRACT fires (with gRPC co-occurrence — Rule 28)
    "specify" → DOCUMENT fires
    "document" → DOCUMENT fires
    IMPLEMENT does NOT fire ("specify" and "document" are spec verbs, not build verbs)

Pass condition: CompiledPredicateIds = ["API_CONTRACT", "DOCUMENT"] exactly
IMPLEMENT must NOT be present (this is the held-out testing point)

---

## B12-COR-007 — Corruption: gRPC → classified as IMPLEMENT (wrong — should be API_CONTRACT)

```
source_id: xiigen-b12-cor-007
corruption_type: grpc_misclassified_as_implement
pair_kind: corruption
```

A: same request as B12-HO-007

B (corrupt):
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]  ← API_CONTRACT replaced by IMPLEMENT
  TransitionType: "feature_addition"  ← WRONG
  Routing: ["4.3", "2.1"]  ← 3.3 missing
  CorruptReason: "gRPC contract interpreted as implementation task: 'gRPC service contract'
  → classifier treated gRPC as a code implementation domain → IMPLEMENT fires, API_CONTRACT suppressed"

Corruption label: gRPC → IMPLEMENT (wrong — gRPC fires API_CONTRACT, not IMPLEMENT)
Expected correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt output: ["IMPLEMENT", "DOCUMENT"]

Teaching: gRPC is an API protocol (Remote Procedure Call). "gRPC service contract" = API contract
documentation, not code implementation. The gRPC keyword triggers API_CONTRACT, not IMPLEMENT.
"Specifying" or "documenting" a gRPC contract is documentation work (Phase 3.3, not Phase 4.3).
IMPLEMENT only fires when "implement/write/build/create" explicitly appears in the request.
