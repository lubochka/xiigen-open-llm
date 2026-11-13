# Held-Out and Corruption Pairs (022)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-022 — Held-out: "update" does not fire IMPLEMENT for API versioning

```
source_id: xiigen-b12-ho-022
pair_kind: held_out
hold_out_reason: "Tests that 'update' does not fire IMPLEMENT; only build/implement/create/scaffold/write/develop do"
```

A:
  user_request: "Update the API versioning contract to include sunset date headers for deprecated endpoints."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Routing: ["3.3", "2.1"]
  TransitionType: "documentation"
  Reason: "api" + "contract" + "endpoints" → API_CONTRACT; implied "define" → DOCUMENT; "update" does NOT fire IMPLEMENT

Held-out evaluation:
  Pass: CompiledPredicateIds does NOT contain "IMPLEMENT"; TransitionType = "documentation"
  Fail (v6-01): "update" wrongly fires IMPLEMENT → TransitionType becomes "feature_addition"

---

## B12-COR-022 — Corruption: "update" wrongly fires IMPLEMENT on API versioning contract

```
source_id: xiigen-b12-cor-022
corruption_type: v6-01
pair_kind: corruption
```

A: same request as B12-HO-022

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]  ← IMPLEMENT wrong
  TransitionType: "feature_addition"  ← wrong (should be "documentation")
  CorruptReason: "v6-01: classifier treated 'update' as a construction verb and fired IMPLEMENT; 'update' is a modification verb — it is NOT an IMPLEMENT trigger"

Correct: ["API_CONTRACT", "DOCUMENT"], TransitionType: "documentation"
Corrupt: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"], TransitionType: "feature_addition"

---

## B12-COR-022B — Corruption: "design" fires API_CONTRACT only, ARCHITECTURE suppressed on versioning

```
source_id: xiigen-b12-cor-022b
corruption_type: v6-01
pair_kind: corruption
```

A:
  user_request: "Design the API versioning strategy and document the deprecation contract."

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  ← ARCHITECTURE missing
  CorruptReason: "v6-01: classifier saw 'API' + 'versioning' + 'contract' and prioritised API_CONTRACT, not recognising that 'design' independently fires ARCHITECTURE"

Correct: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
Corrupt: ["API_CONTRACT", "DOCUMENT"]
Rule: "design" fires ARCHITECTURE unconditionally. It co-exists with API_CONTRACT — neither suppresses the other.
