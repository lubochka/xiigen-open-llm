# Layer 1 — Transition Orchestrator Pairs (029)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-029-001 — Orchestration: benchmark + evaluation metric gap

Gap request: "Benchmark the model using F1 and ROUGE evaluation metrics."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-training-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-029-002 — Orchestration: build benchmark harness + document results

Predicates: ["IMPLEMENT", "DOCUMENT"]
Request: "Build a benchmark harness that measures inference latency and document the evaluation criteria."
Flow order: ImplementationFlow41 → DocumentationFlow62
TransitionType: feature_addition

---

## TO-029-003 — Orchestration: dataset curation gap

Gap request: "Dataset curation involves filtering and deduplication before training."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-training-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-029-004 — Orchestration: build dataset curation pipeline + integrate training API

Predicates: ["IMPLEMENT", "API_CONTRACT"]
Request: "Build the dataset curation pipeline and integrate the result with the training API endpoint."
Flow order: APIContractFlow33 → ImplementationFlow41
TransitionType: feature_addition

---

## TO-029-005 — v6 corruption: dataset-curation → IMPLEMENT without code verb (v6-02)

v6 corrupt: "dataset curation" → IMPLEMENT (treating AI data term as code verb)
Correct: [] → GapHandler → gap-ai-ml-training-001
Error: v6-02. "dataset curation" is AI vocabulary. No IMPLEMENT without explicit code verb.
