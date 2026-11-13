# Layer 0-D — Flow Factory Pairs (033)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-033-001 — GapHandler: benchmark standalone

A: [] | "Benchmark the model on the GLUE evaluation suite."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: "benchmark" = AI evaluation goal (not test-writing). No code verb.

---

## FF-033-002 — IMPLEMENT: build benchmark harness

A: ["IMPLEMENT"] | "Build a benchmark harness that calls the model inference API and measures latency."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "benchmark harness" = software artifact.

---

## FF-033-003 — GapHandler: evaluation metric without code verb

A: [] | "The evaluation metric for this task is F1 score on the held-out test set."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: "evaluation metric" = AI quality measurement. No code verb.

---

## FF-033-004 — IMPLEMENT: develop evaluation metric calculator

A: ["IMPLEMENT"] | "Develop an evaluation metric calculator that computes F1, BLEU, and ROUGE scores."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "develop" fires IMPLEMENT. "evaluation metric calculator" = implementation artifact.

---

## FF-033-005 — DOCUMENT: document evaluation metric selection

A: ["DOCUMENT"] | "Document the evaluation metric selection criteria for the model quality dashboard."
Factory: [DocumentationFlow62] | TransitionType: documentation
Teaching: "document" fires DOCUMENT. "evaluation metric selection criteria" = object context.
