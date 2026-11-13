# Layer 0-B — Router Pairs (045)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-045-001 — Gap: LLM standalone

A: [] | "The LLM (large language model) produces inconsistent outputs on edge cases."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "LLM" = gap vocabulary. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-045-002 — IMPLEMENT: build wrapper around LLM

A: ["IMPLEMENT"] | "Build a wrapper around the LLM that normalizes outputs and handles retries."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "LLM wrapper" = software artifact.

---

## C0B-045-003 — Gap: GPT standalone

A: [] | "GPT (generative pre-trained transformer) requires careful prompt design."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "GPT" = specific LLM model name. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-045-004 — IMPLEMENT: build GPT client wrapper

A: ["IMPLEMENT"] | "Build a GPT client wrapper that enforces token limits and formats responses."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "GPT client wrapper" = implementation artifact.

---

## C0B-045-005 — API_CONTRACT: integrate GPT endpoint

A: ["API_CONTRACT"] | "Integrate the GPT API endpoint with the document summarization service."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" always fires API_CONTRACT. "GPT API endpoint" suppressed.
