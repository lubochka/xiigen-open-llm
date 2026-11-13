# Layer 3 — Teaching Unit Pairs (013)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU013-001 — QA+IMPLEMENT: blue-green service + gate

A: "Build the blue-green deployment service and QA the cutover logic."
B: ["IMPLEMENT", "QA"] — "build" → IMPLEMENT; "QA" → QA.
Reason: Two predicates. "cutover logic" is gap vocabulary. "build" fires IMPLEMENT. QA fires on abbreviation. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## TU013-002 — QA alone: no contribution

A: "QA review the blue-green deployment criteria before releasing."
B: ["QA"] — "QA review" → QA; "deployment criteria" → QA fires on QA, not on "deployment" or "criteria" alone here.
Reason: Wait — "deployment criteria" fires both DOCUMENT (via "criteria") and the overall is gap context for "deployment". Re-analysis: "QA review" → QA; "criteria" → DOCUMENT. Two predicates.
Correction: B: ["QA", "DOCUMENT"] — "QA review" → QA; "criteria" → DOCUMENT.
CompiledPredicateIds: ["QA", "DOCUMENT"]

---

## TU013-003 — Gap: "switch traffic to green" is pure gap

A: "Switch traffic to the green environment and monitor error rates."
B: [] — "switch traffic" → gap; "monitor error rates" → gap.
Reason: Blue-green operational commands and monitoring vocabulary → gap. knowledge_edge output.
CompiledPredicateIds: []

---

## TU013-004 — QA+IMPLEMENT+DOCUMENT: runbook + service + gate

A: "Implement the blue-green service, document the rollback runbook, and get QA sign-off."
B: ["IMPLEMENT", "DOCUMENT", "QA"] — "implement" → IMPLEMENT; "document" → DOCUMENT; "QA sign-off" → QA.
Reason: Three predicates. "rollback runbook" fires DOCUMENT via "document" verb. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "QA"]

---

## TU013-005 — QA alone vs QA+DOCUMENT: "criteria" disambiguator

A: "QA gate the deployment." vs "QA gate the deployment criteria."
B1: ["QA"] — no "criteria" noun; "deployment" → gap.
B2: ["QA", "DOCUMENT"] — "criteria" fires DOCUMENT.
Reason: Adding "criteria" to the request fires DOCUMENT independently of QA.
CompiledPredicateIds (B1): ["QA"] | CompiledPredicateIds (B2): ["QA", "DOCUMENT"]
