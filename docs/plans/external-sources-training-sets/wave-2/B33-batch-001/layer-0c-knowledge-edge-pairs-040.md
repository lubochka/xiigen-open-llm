# Layer 0-C — Knowledge Edge Pairs (040)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-040-001 — Regularization gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| regularization (standalone) | No code verb | gap-ai-ml-training-001 |
| regularization wrapper (with "implement") | Code verb present | IMPLEMENT fires |
| regularization service (with "write tests for") | "write tests" present | TEST fires |

---

## KE-040-002 — Cross-validation gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| cross-validation (standalone) | No code verb | gap-ai-ml-training-001 |
| cross-validation runner (with "create") | Code verb present | IMPLEMENT fires |
| cross-validation pipeline (with "QA") | "QA" present | QA fires (QA always last) |

---

## KE-040-003 — "Create" is equivalent to "build" for IMPLEMENT

Rule: "create" fires IMPLEMENT when the object is a software artifact.
"create a cross-validation runner" → IMPLEMENT (software artifact).
"create documentation" → DOCUMENT.
"create tests" → TEST (test-writing form).
The object type determines which predicate fires, not "create" alone.

---

## KE-040-004 — TEST fires only on test-writing form

Rule: TEST fires when the verb phrase is explicitly about writing/creating tests.
"write tests for the regularization service" → TEST.
"test the regularization service" (QA-style verification) does NOT fire TEST — it fires QA or nothing.
Only "write tests", "create tests", "add tests", "implement tests" → TEST.

---

## KE-040-005 — Regularization + cross-validation combined: still one event

Rule: "regularization" and "cross-validation" both map to gap-ai-ml-training-001.
Multiple training vocabulary terms in one gap request → one SCHEDULE_CAPABILITY_LEARNING.
Example: "regularization prevents overfitting, cross-validation measures it" → [] → gap-ai-ml-training-001 (one event).
