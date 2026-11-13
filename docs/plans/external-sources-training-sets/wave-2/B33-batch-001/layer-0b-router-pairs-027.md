# Layer 0-B — Router Pairs (027)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-027-001 — Gap → GapHandler: "attention mechanism" vocabulary

A: [] | "The attention mechanism is not capturing long-range dependencies."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "attention mechanism" = AI model internal component. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-027-002 — Gap → GapHandler: "transformer" vocabulary

A: [] | "The transformer architecture needs to be adjusted for our use case."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "transformer" = AI model architecture. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-027-003 — Gap → GapHandler: "inference endpoint" without integrate

A: [] | "The inference endpoint serves the model predictions."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "inference endpoint" = AI serving component description. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-027-004 — API_CONTRACT: integrate inference endpoint

A: ["API_CONTRACT"] | "Integrate the inference endpoint with the application backend."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" fires API_CONTRACT. "inference endpoint" = object context.

---

## C0B-027-005 — Gap → GapHandler: "model serving" vocabulary

A: [] | "Model serving latency is too high in production."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "model serving" = AI operational concern. No code verb. Gap ID: gap-ai-ml-model-001.
