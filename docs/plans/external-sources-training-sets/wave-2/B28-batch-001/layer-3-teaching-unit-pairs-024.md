# Layer 3 — Teaching Unit Pairs (024) — Complete Teaching Reference for 7.1
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU024-001 — "service" rule and "criteria" rule summary

A: "Deploy the release service and define the deployment criteria."
B: ["IMPLEMENT", "DOCUMENT"] — "release service" → IMPLEMENT (via "service"); "criteria" → DOCUMENT.
Reason: "service" fires IMPLEMENT always. "criteria" fires DOCUMENT always. "deploy" and "deployment" are gap.
CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]

---

## TU024-002 — QA alone never contributes TransitionType

A: "QA the release before go-live." | "QA sign-off on the deployment." | "QA the canary rollout."
B: ["QA"] for all three. TransitionType: none for all three.
Reason: QA alone → ReleaseGateFlow71 gate only. No TransitionType contribution from QA.
CompiledPredicateIds: ["QA"]

---

## TU024-003 — IMPLEMENT drives feature_addition over any other predicate

A: "Design the release architecture, implement the gate service, document the criteria, and QA approve."
B: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT", "QA"] — TransitionType: feature_addition (IMPLEMENT present).
Reason: IMPLEMENT drives feature_addition regardless of how many other predicates are present.
CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT", "QA"]

---

## TU024-004 — Pure gap vocabulary in release domain

A: "Plan the go-live checklist and schedule the deployment rollout."
B: [] — "go-live checklist" → gap; "schedule deployment rollout" → gap.
Reason: Pure gap → knowledge_edge (not GapHandler, not SCHEDULE_CAPABILITY_LEARNING).
CompiledPredicateIds: []

---

## TU024-005 — "build provenance" fires IMPLEMENT

A: "Record build provenance and QA the artifact integrity."
B: ["IMPLEMENT", "QA"] — "build provenance" → IMPLEMENT (build artifact vocabulary); "QA" → QA.
Reason: "build provenance" is a build artifact term → IMPLEMENT. Not gap.
CompiledPredicateIds: ["IMPLEMENT", "QA"]
