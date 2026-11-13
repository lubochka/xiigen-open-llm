# Layer 0-B — Router Pairs (044)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-044-001 — Gap: backpropagation + gradient descent compound

A: [] | "Backpropagation uses gradient descent to update weights in each layer."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: No code verb. Two gap-ai-ml-training-001 terms. One event.

---

## C0B-044-002 — IMPLEMENT: implement backpropagation + gradient descent module

A: ["IMPLEMENT"] | "Implement the backpropagation and gradient descent module for the custom optimizer."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. Both AI training terms in object position.

---

## C0B-044-003 — Gap: hyperparameter + regularization combined

A: [] | "Hyperparameter selection affects regularization strength and model generalization."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Both terms gap-ai-ml-training-001. One event.

---

## C0B-044-004 — IMPLEMENT+API_CONTRACT: build hyperparameter service and integrate

A: ["IMPLEMENT", "API_CONTRACT"] | "Build the hyperparameter configuration service and integrate it with the training orchestrator API."
B: IMPLEMENT → ImplementationFlow41; API_CONTRACT → APIContractFlow33.
Teaching: "build" fires IMPLEMENT; "integrate" fires API_CONTRACT. Two flows, standard order.

---

## C0B-044-005 — QA: QA cross-validation pipeline

A: ["QA"] | "QA the cross-validation pipeline before promoting to production."
B: QA → QAGatingFlow73 (last, absolute).
Teaching: "QA" abbreviation fires QA. QA always last regardless of position in request.
