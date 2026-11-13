# Layer 0-D — Flow Factory Pairs (035)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-035-001 — GapHandler: LLM standalone

A: [] | "The LLM produces inconsistent outputs on edge cases."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "LLM" = gap vocabulary. No code verb. One SCHEDULE_CAPABILITY_LEARNING.

---

## FF-035-002 — IMPLEMENT: build LLM wrapper

A: ["IMPLEMENT"] | "Build a wrapper around the LLM that normalizes outputs and handles retries."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "LLM wrapper" = software artifact.

---

## FF-035-003 — GapHandler: GPT standalone

A: [] | "GPT requires careful prompt design."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "GPT" = specific LLM model name. No code verb.

---

## FF-035-004 — IMPLEMENT: build GPT client wrapper

A: ["IMPLEMENT"] | "Build a GPT client wrapper that enforces token limits and formats responses."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "GPT client wrapper" = implementation artifact.

---

## FF-035-005 — API_CONTRACT: integrate GPT endpoint

A: ["API_CONTRACT"] | "Integrate the GPT API endpoint with the document summarization service."
Factory: [APIContractFlow33] | TransitionType: feature_addition
Teaching: "integrate" fires API_CONTRACT. "GPT API endpoint" suppressed.
