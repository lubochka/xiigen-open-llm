# Layer 3 — Teaching Unit Pairs (023)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU023-001 — QA+IMPLEMENT: emergency hotfix service

A: "Build the emergency hotfix deployment service and QA the patch before go-live."
B: ["IMPLEMENT", "QA"] — "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "QA" → QA; "patch" → gap; "go-live" → gap.
Reason: Two predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## TU023-002 — Gap: hotfix deployment command

A: "Deploy the hotfix to production immediately and restart the affected services."
B: [] — "deploy hotfix" → gap; "production" → gap; "restart services" → gap.
Reason: Hotfix deployment execution → gap. knowledge_edge.
CompiledPredicateIds: []

---

## TU023-003 — QA alone: emergency gate

A: "QA the hotfix patch for the authentication service before emergency cutover."
B: ["QA"] — "QA" → QA; "hotfix patch" → gap; "emergency cutover" → gap.
Reason: QA alone. No TransitionType. Gate-only output.
CompiledPredicateIds: ["QA"]

---

## TU023-004 — QA+IMPLEMENT+TEST+SECURITY: secure hotfix

A: "Build the hotfix, write regression tests, validate the security patch, and QA approve."
B: ["IMPLEMENT", "TEST", "SECURITY", "QA"] — "build" → IMPLEMENT; "write regression tests" → TEST; "validate the security patch" → TEST (via "validate") + SECURITY (via "security patch"); "QA approve" → QA.
Reason: TEST fires twice (deduplicated). Four predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "SECURITY", "QA"]

---

## TU023-005 — "hotfix" alone is gap; "hotfix service" fires IMPLEMENT

A: "Apply the hotfix." vs "Build the hotfix deployment service."
B1: [] — "apply the hotfix" → gap.
B2: ["IMPLEMENT"] — "build" → IMPLEMENT; "service" → IMPLEMENT (dedup).
Reason: "hotfix" alone is gap. "build [noun] service" fires IMPLEMENT via "service" rule.
CompiledPredicateIds (B1): [] | CompiledPredicateIds (B2): ["IMPLEMENT"]
