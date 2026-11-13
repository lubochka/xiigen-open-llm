# Layer 1 — Transition Orchestrator Pairs (042)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-042-001 — Orchestration: dataset curation + training loop + evaluation metric

Gap request: "Dataset curation feeds the training loop; evaluation metrics guide model selection."
Predicates: [] | GapHandler → gap-ai-ml-training-001 | One event (pure round-2 terms).

---

## TO-042-002 — Orchestration: build curation pipeline + integrate + write tests

Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"]
Request: "Build the dataset curation pipeline, integrate with the training API, write tests for data quality."
Flow order: APIContractFlow33 → ImplementationFlow41 → TestFlow51 | TransitionType: feature_addition

---

## TO-042-003 — Orchestration: document + QA training pipeline

Predicates: ["DOCUMENT", "QA"]
Request: "Document the dataset curation pipeline and QA before production rollout."
Flow order: DocumentationFlow62 → QAGatingFlow73 | TransitionType: documentation
Rule: QA always last.

---

## TO-042-004 — v6 corruption: DATASET_CURATION invented predicate (v6-01)

v6 corrupt: "dataset curation" → "DATASET_CURATION" predicate
Correct: [] → GapHandler → gap-ai-ml-training-001
Error: v6-01. "dataset curation" is AI vocabulary. No predicate by that name exists.

---

## TO-042-005 — Orchestration: implement + secure + test + document full cycle

Predicates: ["IMPLEMENT", "SECURITY", "TEST", "DOCUMENT"]
Request: "Implement the model training service, secure endpoints, write integration tests, document the training API."
Flow order: ImplementationFlow41 → SecurityAuditFlow → TestFlow51 → DocumentationFlow62
TransitionType: feature_addition
