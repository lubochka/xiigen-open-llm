# Layer 0-C — Knowledge Edge Pairs (023)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E023-001 — GAP: "coupon validation check" fires nothing

A:
  user_request: "Do a coupon validation check to make sure expired codes are not accepted."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "do a" — generic action; gap
    "validation check" — "validation" alone in this context is informal process review, not TEST; "check" is gap
    "make sure" — informal assurance; gap
    Result: zero predicates
  Edge: "validation check" as a compound is gap — it is a process-review phrase, not an assert/verify action. Contrast: "validate that expired codes are not accepted" → "validate" fires TEST universally.

---

## E023-002 — EDGE: "run the coupon test suite" does NOT fire TEST

A:
  user_request: "Run the coupon test suite after deploying the new coupon stacking rules."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "run" — NOT TEST; execution verb
    "coupon test suite" — object of "run"; "test suite" is TEST vocabulary but only when it is the object of a TEST-firing verb (write, assert, verify, spec, create, build, develop)
    "after deploying" — temporal qualifier; gap
    Result: zero predicates → knowledge_edge
  Teaching: "run [noun]" never fires a predicate from the noun. The predicate fires from the verb.
  "run the coupon test suite" and "run unit tests" and "run the regression suite" — all produce zero predicates.
