# Layer 0-D — Flow Factory Pairs (048)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-048-001 — GapHandler: requirement statement for AI vocabulary

A: [] | "We need an AI-powered knowledge graph connector."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: Requirement statement. No code verb. "knowledge graph" = gap-ai-ml-model-001.

---

## FF-048-002 — IMPLEMENT: AI-powered + build verb

A: ["IMPLEMENT"] | "Build an AI-powered knowledge graph connector."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "AI-powered" = adjective; does not suppress.

---

## FF-048-003 — IMPLEMENT: build LLM orchestration service

A: ["IMPLEMENT"] | "Build an LLM orchestration service that manages AI agent tool calls and memory."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "LLM orchestration service" = software artifact.

---

## FF-048-004 — GapHandler: LLM + gradient descent mixed (no code verb)

A: [] | "LLM trained with gradient descent for 100 epochs achieves BLEU score of 0.45."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: Descriptive. No code verb. Mixed gap terms. → gap-ai-ml-model-001 (dominant).

---

## FF-048-005 — API_CONTRACT: single integrate suppresses all gap vocabulary

A: ["API_CONTRACT"] | "LLM, GPT, BERT, Llama, neural network, training loop, gradient descent — integrate results with reporting API."
Factory: [APIContractFlow33] | TransitionType: feature_addition
Teaching: "integrate" fires API_CONTRACT. All gap vocabulary from all rounds silently suppressed.
