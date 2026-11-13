# Layer 3 — Teaching Unit Pairs (015)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU015-001 — QA+IMPLEMENT: rollback service

A: "Build the automated rollback service and QA the trigger conditions."
B: ["IMPLEMENT", "QA"] — "build" → IMPLEMENT; "QA" → QA.
Reason: Two predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## TU015-002 — Gap: rollback execution

A: "Roll back the deployment to v2.1 and restart the pods."
B: [] — "roll back" → gap; "restart pods" → gap.
Reason: Rollback execution vocabulary → gap. knowledge_edge.
CompiledPredicateIds: []

---

## TU015-003 — QA+IMPLEMENT+TEST: rollback with validation

A: "Implement rollback triggers, verify state restoration, and QA sign-off."
B: ["IMPLEMENT", "TEST", "QA"] — "implement" → IMPLEMENT; "verify" → TEST; "QA sign-off" → QA.
Reason: Three predicates. "verify" fires TEST regardless of what is verified. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## TU015-004 — QA+DOCUMENT: rollback runbook documentation

A: "Document the rollback procedure and QA review the runbook before go-live."
B: ["DOCUMENT", "QA"] — "document" → DOCUMENT; "QA review" → QA; "go-live" → gap.
Reason: "runbook" alone is gap; "document" verb fires DOCUMENT. TransitionType: documentation.
CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## TU015-005 — FAIL is expected: rollback gate

A: "QA gate the rollback procedure for the NotificationService."
B: ["QA"] — gate can produce FAIL if rollback does not restore state correctly.
Reason: QA gate verdicts include FAIL. Models must not default to PASS. QA alone → no TransitionType.
CompiledPredicateIds: ["QA"]
