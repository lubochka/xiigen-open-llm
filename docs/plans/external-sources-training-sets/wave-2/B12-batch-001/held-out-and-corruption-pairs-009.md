# Held-Out and Corruption Pairs (009)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-009 — Held-out: API versioning → API_CONTRACT + DOCUMENT (not ARCHITECTURE)

```
source_id: xiigen-b12-ho-009
pair_kind: held_out
hold_out_reason: "Tests that API versioning fires API_CONTRACT not ARCHITECTURE"
```

A:
  user_request: "Specify the API versioning approach for the payment service and define the migration guide from v1 to v2."

B (expected):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  Routing: ["3.3", "2.1"]
  Reason:
    "API" → API_CONTRACT fires
    "versioning" → API_CONTRACT fires (Rule 21: versioning in API context → API_CONTRACT)
    "specify" → DOCUMENT fires
    "define" → DOCUMENT fires
    "migration guide" → DOCUMENT fires
    ARCHITECTURE does NOT fire — no "design" verb, no architecture vocabulary
    IMPLEMENT does NOT fire — "specify/define" are spec verbs

Pass condition:
  - CompiledPredicateIds contains "API_CONTRACT"
  - CompiledPredicateIds does NOT contain "ARCHITECTURE" (key test: versioning ≠ architecture)
  - TransitionType = "documentation"

---

## B12-COR-009 — Corruption: versioning → ARCHITECTURE instead of API_CONTRACT (wrong)

```
source_id: xiigen-b12-cor-009
corruption_type: versioning_misclassified_as_architecture
pair_kind: corruption
```

A: same request as B12-HO-009

B (corrupt):
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]  ← ARCHITECTURE wrong, API_CONTRACT missing
  Routing: ["3.1", "2.1"]  ← 3.3 missing, 3.1 wrongly added
  CorruptReason: "API versioning strategy classified as architecture decision: classifier
  treated 'versioning approach' as a system design / architecture choice → ARCHITECTURE fires
  instead of API_CONTRACT"

Corruption label: VERSIONING → ARCHITECTURE (wrong — versioning fires API_CONTRACT)
Expected correct: ["API_CONTRACT", "DOCUMENT"]
Corrupt output: ["ARCHITECTURE", "DOCUMENT"]

Teaching: "API versioning" is an API contract concern, not a system architecture concern.
Core Rule 21: "versioning" and "version" in API context → API_CONTRACT.
API versioning (v1, v2, URL-based, header-based) is about the API contract structure.
ARCHITECTURE fires on "design" and "architecture" vocabulary, not on "versioning" vocabulary.

Contrast:
  "Design the API versioning architecture" → ARCHITECTURE (via "design") + API_CONTRACT (via "api", "versioning")
  "Specify the API versioning approach" → API_CONTRACT only (no "design" → no ARCHITECTURE)
