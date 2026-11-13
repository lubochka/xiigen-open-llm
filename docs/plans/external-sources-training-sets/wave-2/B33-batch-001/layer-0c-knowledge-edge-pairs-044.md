# Layer 0-C — Knowledge Edge Pairs (044)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-044-001 — Compound gap request: backpropagation + gradient descent

Rule: "backpropagation" and "gradient descent" both belong to gap-ai-ml-training-001.
Compound request without code verb → one SCHEDULE_CAPABILITY_LEARNING.
"Backpropagation uses gradient descent to update weights" → [] → gap-ai-ml-training-001 (one event).

---

## KE-044-002 — Implement backpropagation + gradient descent together

When code verb is present with multiple training terms:
"Implement the backpropagation and gradient descent module" → IMPLEMENT (one predicate).
"Implement" fires once regardless of how many training vocabulary items are in the object.
Never emit multiple IMPLEMENT predicates for a single "implement" verb.

---

## KE-044-003 — IMPLEMENT+API_CONTRACT ordering rules

When both IMPLEMENT and API_CONTRACT are present:
Rule: API_CONTRACT fires before IMPLEMENT per canonical ordering rule 2 (API_CONTRACT before IMPLEMENT).
"Build hyperparameter service and integrate with training API" → API_CONTRACT first, then IMPLEMENT.
Wait — canonical ordering places API_CONTRACT before IMPLEMENT in the flow execution sequence, not in detection order.
Detection: both predicates found simultaneously. Execution: API_CONTRACT spec informs IMPLEMENT.

---

## KE-044-004 — QA always executes last, regardless of position in request

Rule: QA (abbreviation) fires QAGatingFlow73. QA always executes last.
"QA the cross-validation pipeline" → QA.
Even if QA appears first in the request: "QA first, then implement" → execution: IMPLEMENT then QA.
QA validates all preceding flows.

---

## KE-044-005 — v6 corruption catalogue for training vocabulary

v6 corruptions to watch for:
- v6-01: "gradient-descent" → "GRADIENT_DESCENT" predicate (invented). Correct: [] → gap.
- v6-01: "backpropagation" → "BACKPROP" predicate (invented). Correct: [] → gap.
- v6-02: "backprop" → IMPLEMENT (incorrectly treating AI training term as code verb). Correct: [] → gap.
- v6-03: "implement optimizer + integrate training API" → ["IMPLEMENT"] only (collapsed). Correct: ["IMPLEMENT", "API_CONTRACT"].
