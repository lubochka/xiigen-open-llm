# Layer 0-B — Router Pairs (031)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-031-001 — Gap → GapHandler: "benchmark" as AI evaluation

A: [] | "Benchmark the model against GPT-4 on the SWE-bench dataset."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "benchmark" in AI context = model evaluation activity. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-031-002 — TEST: write benchmark tests

A: ["TEST"] | "Write benchmark tests that measure model throughput and accuracy."
B: TEST → E2ETestingFlow53.
Teaching: "write tests" fires TEST. "benchmark" = test type in object position.

---

## C0B-031-003 — Gap → GapHandler: "evaluation metric" vocabulary

A: [] | "The evaluation metrics (F1, BLEU, ROUGE) need to be reported."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "evaluation metrics" = AI model evaluation terms. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-031-004 — TEST: create evaluation tests

A: ["TEST"] | "Create tests that measure F1 score and BLEU metric against the validation set."
B: TEST → E2ETestingFlow53.
Teaching: "create tests" fires TEST. AI metric vocabulary = object context.

---

## C0B-031-005 — Gap → GapHandler: "dataset curation" vocabulary

A: [] | "Dataset curation needs to remove low-quality examples."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "dataset curation" = AI data preparation activity. No code verb. Gap ID: gap-ai-ml-training-001.
