# Layer 0-A — Compiler Pairs (032)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-032-001 — gap: "benchmark" as AI evaluation vocabulary

A: "Benchmark the model against GPT-4 on the SWE-bench dataset."
B: [] — "benchmark" in AI context = model evaluation activity. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-032-002 — TEST fires: write benchmark tests

A: "Write benchmark tests that measure model throughput and accuracy."
B: ["TEST"] — "write tests" fires TEST. "benchmark" is the test type in object position.
CompiledPredicateIds: ["TEST"]

---

## C0A-032-003 — gap: "evaluation metric" vocabulary

A: "The evaluation metrics (F1, BLEU, ROUGE) need to be reported."
B: [] — "evaluation metrics" = AI model evaluation terms. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-032-004 — TEST fires: create evaluation tests

A: "Create tests that measure F1 score and BLEU metric against the validation set."
B: ["TEST"] — "create tests" fires TEST. "F1 score", "BLEU metric" are object context.
CompiledPredicateIds: ["TEST"]

---

## C0A-032-005 — gap: "dataset curation" vocabulary without code verb

A: "Dataset curation needs to remove low-quality examples."
B: [] — "dataset curation" = AI data preparation activity. No code verb. Gap.
CompiledPredicateIds: []
