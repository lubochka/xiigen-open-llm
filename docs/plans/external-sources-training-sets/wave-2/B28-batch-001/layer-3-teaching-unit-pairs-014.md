# Layer 3 — Teaching Unit Pairs (014)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU014-001 — QA+IMPLEMENT: canary controller

A: "Build the canary release controller and QA the routing logic."
B: ["IMPLEMENT", "QA"] — "build" → IMPLEMENT; "QA" → QA.
Reason: Two predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## TU014-002 — QA+DOCUMENT: canary criteria

A: "Document the canary release criteria and QA sign-off."
B: ["DOCUMENT", "QA"] — "document" → DOCUMENT; "criteria" → DOCUMENT (dedup); "QA sign-off" → QA.
Reason: DOCUMENT fires on both "document" verb and "criteria" noun (deduplicated). TransitionType: documentation.
CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## TU014-003 — Gap: canary percentage adjustment

A: "Increase the canary traffic percentage from 5% to 20%."
B: [] — "increase canary traffic" → gap; "percentage" → gap.
Reason: Canary operational adjustments → gap. knowledge_edge.
CompiledPredicateIds: []

---

## TU014-004 — QA+IMPLEMENT+TEST: full canary implementation

A: "Build the canary controller, write acceptance tests, and QA approve."
B: ["IMPLEMENT", "TEST", "QA"] — "build" → IMPLEMENT; "write acceptance tests" → TEST; "QA approve" → QA.
Reason: Three predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## TU014-005 — TransitionType: QA+IMPLEMENT always feature_addition

A: "Implement the canary traffic split and QA sign-off."
B: ["IMPLEMENT", "QA"] — TransitionType: feature_addition (IMPLEMENT drives it, QA does not contribute).
Reason: QA does not determine TransitionType. IMPLEMENT always drives feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "QA"]
