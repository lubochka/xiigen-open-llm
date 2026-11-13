# Layer 0-C — Knowledge Edge Pairs (028)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-028-001 — Event contract: "event contract" fires API_CONTRACT (all modifiers accepted)

```
source_id: xiigen-b12-0c-028-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the event streaming API contract for the order events topic."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R01: "api" → API_CONTRACT ✓
    Rule B12-R01: "contract" → API_CONTRACT unconditional (modifier "event streaming" irrelevant) ✓
    Rule B12-R04: "define" → DOCUMENT ✓
  Key: The modifier on "contract" is NEVER relevant for API_CONTRACT firing.
  event contract, streaming contract, topic contract, message contract — all fire API_CONTRACT via "contract".

---

## B12-0C-028-002 — Event schema publish: API_CONTRACT fires; IMPLEMENT does NOT

```
source_id: xiigen-b12-0c-028-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Publish the event schema for the payment events API to the event catalog."

B:
  CompiledPredicateIds: ["API_CONTRACT"]
  Rule applications:
    Rule B12-R17c: "publish" → API_CONTRACT (applied to API schema) ✓
    Rule B12-R02: "schema" fires API_CONTRACT because "api" is co-occurrence ✓
    Rule B12-R01: "api" → API_CONTRACT ✓
  Boundary: "publish" → NOT IMPLEMENT in any context. Even "publish the code" → documentation, not feature_addition.
  "publish" is always a release verb in B12.

---

## B12-0C-028-003 — Event producer API: "build" fires IMPLEMENT; DOCUMENT fires from "define"

```
source_id: xiigen-b12-0c-028-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Build the event producer API for the order service and define the event contract schema."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R08: "build" → IMPLEMENT ✓
    Rule B12-R01: "api" → API_CONTRACT ✓
    Rule B12-R01: "event contract" → API_CONTRACT (contract unconditional) ✓
    Rule B12-R02: "schema" fires API_CONTRACT because "api"/"contract" is co-occurrence ✓
    Rule B12-R04: "define" → DOCUMENT ✓
  Key: Three predicates fire from a single request. The construction clause ("build") and the
  specification clause ("define the contract") operate independently — neither suppresses the other.
  All three predicates are additive.
