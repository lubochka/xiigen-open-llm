# Held-Out and Corruption Pairs (004)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B12-HO-004 — Held-out: "interface" in API design → fires both ARCHITECTURE and API_CONTRACT

```
source_id: xiigen-b12-ho-004
pair_kind: held_out
hold_out_reason: "Tests interface dual-fire in 'design the ... interface' + API contract context"
```

A:
  user_request: "Design the API interface for the catalog service and specify the REST endpoint contracts."

B (expected):
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  Routing: ["3.1", "3.3", "2.1"]
  TransitionType: "documentation"
  Reason:
    "design" → ARCHITECTURE fires
    "interface" → BOTH (design context = ARCHITECTURE; API context = API_CONTRACT)
    "API" → API_CONTRACT fires
    "REST" → API_CONTRACT fires
    "endpoint" → API_CONTRACT fires
    "contracts" → API_CONTRACT fires (Rule 2)
    "specify" → DOCUMENT fires

Pass condition: CompiledPredicateIds contains BOTH "ARCHITECTURE" AND "API_CONTRACT"

---

## B12-COR-004 — Corruption: ARCHITECTURE suppressed when "interface" in API design context

```
source_id: xiigen-b12-cor-004
corruption_type: architecture_suppressed
pair_kind: corruption
```

A: same request as B12-HO-004
  user_request: "Design the API interface for the catalog service and specify the REST endpoint contracts."

B (corrupt):
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  ← ARCHITECTURE missing
  Routing: ["3.3", "2.1"]  ← 3.1 missing
  CorruptReason: "ARCHITECTURE suppressed: classifier treated 'design' as context for API_CONTRACT
  in API-heavy request → forgot that 'design' always fires ARCHITECTURE independently"

Corruption label: ARCHITECTURE SUPPRESSED WHEN API VOCABULARY DOMINANT
Expected correct: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
Corrupt output: ["API_CONTRACT", "DOCUMENT"]

Teaching: "design" always fires ARCHITECTURE, even in API-heavy requests.
"design" does not get absorbed into API_CONTRACT when API vocabulary is present.
Both predicates fire independently. Neither suppresses the other.

"Design the API interface" = both "design" (→ARCHITECTURE) and "API interface" (→API_CONTRACT).
The "API" context does not cancel the "design" architecture trigger.
