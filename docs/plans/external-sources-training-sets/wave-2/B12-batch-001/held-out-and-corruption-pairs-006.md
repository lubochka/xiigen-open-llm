# Held-Out and Corruption Pairs (006)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-006 — Held-out: API_CONTRACT + DOCUMENT → documentation TransitionType

```
source_id: xiigen-b12-ho-006
pair_kind: held_out
hold_out_reason: "Tests correct TransitionType assignment for documentation-only API contract work"
```

A:
  user_request: "Define the event-driven API contract for the order service and document the message schemas."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  Routing: ["3.3", "2.1"]
  Reason:
    "API" → API_CONTRACT fires
    "contract" → API_CONTRACT fires (Rule 2)
    "define" → DOCUMENT fires
    "document" → DOCUMENT fires
    "message schemas" → DOCUMENT (schema with "API contract" co-occurrence → API_CONTRACT also)
    IMPLEMENT does NOT fire → TransitionType = documentation

Pass condition: TransitionType = "documentation"
The absence of "write/implement/build/create" verbs keeps TransitionType as documentation.

---

## B12-COR-006 — Corruption: classified as feature_addition when no IMPLEMENT fires

```
source_id: xiigen-b12-cor-006
corruption_type: wrong_transition_type
pair_kind: corruption
```

A: same request as B12-HO-006

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  (correctly detected)
  TransitionType: "feature_addition"  ← WRONG
  CorruptReason: "TransitionType incorrectly set to feature_addition because API contract
  definition felt like 'creating' something new (new API contract document)"

Corruption label: FEATURE_ADDITION WITHOUT IMPLEMENT
Expected correct TransitionType: "documentation"
Corrupt TransitionType: "feature_addition"

Teaching: TransitionType = feature_addition REQUIRES IMPLEMENT predicate.
"Define the API contract" → "define" is a specification verb → DOCUMENT fires, IMPLEMENT does NOT.
Producing a contract document (documentation artifact) is documentation, not feature_addition.
The distinction: feature_addition creates CODE artifacts; documentation creates SPECIFICATION artifacts.

Note: In some edge cases, creating a new specification file could be considered feature_addition
(new artifact where none existed). But the B12 rule is clear: IMPLEMENT fires on construction verbs.
"Define" is not a construction verb → documentation TransitionType.
