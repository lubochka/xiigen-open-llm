# Layer 0-D — Flow Factory Pairs (027)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-027-001 — GapHandler: "attention mechanism" vocabulary

A: [] | "The attention mechanism is not capturing long-range dependencies."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "attention mechanism" = AI model internal. No code verb. Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## FF-027-002 — GapHandler: "transformer" vocabulary

A: [] | "The transformer architecture needs to be adjusted for our use case."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "transformer" = AI model architecture. No code verb. Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## FF-027-003 — GapHandler: "inference endpoint" without code verb

A: [] | "The inference endpoint serves the model predictions."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "inference endpoint" = AI serving component description. No code verb. Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## FF-027-004 — API_CONTRACT: integrate inference endpoint

A: ["API_CONTRACT"] | "Integrate the inference endpoint with the application backend."
Factory: [APIContractFlow33]
Teaching: "integrate" fires API_CONTRACT. AI vocabulary = object context.

---

## FF-027-005 — GapHandler: "model serving" + "hallucination mitigation" compound

A: [] | "Model serving latency is too high and hallucination mitigation is insufficient."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: Both "model serving" and "hallucination mitigation" = AI operational vocabulary. No code verb. One event.
