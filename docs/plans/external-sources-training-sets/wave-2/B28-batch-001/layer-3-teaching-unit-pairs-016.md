# Layer 3 — Teaching Unit Pairs (016)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU016-001 — QA+DOCUMENT: feature flag criteria

A: "Document the feature flag release criteria and QA sign-off."
B: ["DOCUMENT", "QA"] — "document" → DOCUMENT; "criteria" → DOCUMENT (dedup); "QA sign-off" → QA.
Reason: DOCUMENT fires twice (deduplicated). TransitionType: documentation.
CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## TU016-002 — Gap: feature flag toggle

A: "Enable the recommendation feature flag for 10% of users."
B: [] — "enable feature flag" → gap; "10% of users" → gap.
Reason: Feature flag operational toggle → gap. knowledge_edge.
CompiledPredicateIds: []

---

## TU016-003 — QA+IMPLEMENT: feature flag service

A: "Build the feature flag service and QA the toggle logic."
B: ["IMPLEMENT", "QA"] — "build" → IMPLEMENT; "feature flag service" → IMPLEMENT via "service"; "QA" → QA.
Reason: "service" fires IMPLEMENT. Deduplication yields one IMPLEMENT. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## TU016-004 — QA alone: feature flag gate (no TransitionType)

A: "QA gate the feature flag rollout for the ProductService."
B: ["QA"] — "QA gate" → QA; "rollout" → gap.
Reason: QA alone → no TransitionType. Gate-only output.
CompiledPredicateIds: ["QA"]

---

## TU016-005 — "feature flag" alone is not a predicate trigger

A: "Review the feature flag for the SearchService experiment."
B: [] — "review" alone → gap; "feature flag" → gap; no QA abbreviation.
Reason: "review" alone is gap (not a QA compound noun). "feature flag" alone is gap. No predicates fire.
CompiledPredicateIds: []
