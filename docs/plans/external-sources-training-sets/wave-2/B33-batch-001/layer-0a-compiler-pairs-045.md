# Layer 0-A — Compiler Pairs (045)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-045-001 — gap: LLM (large language model) standalone

A: "The LLM (large language model) produces inconsistent outputs on edge cases."
B: [] — "LLM" = gap vocabulary. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-045-002 — IMPLEMENT: build wrapper around LLM

A: "Build a wrapper around the LLM that normalizes outputs and handles retries."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. LLM wrapper = software implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-045-003 — gap: GPT (generative pre-trained transformer) standalone

A: "GPT (generative pre-trained transformer) requires careful prompt design."
B: [] — "GPT" = specific LLM model name. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-045-004 — IMPLEMENT: build GPT client wrapper

A: "Build a GPT client wrapper that enforces token limits and formats responses."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "GPT client wrapper" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-045-005 — API_CONTRACT: integrate GPT endpoint with application

A: "Integrate the GPT API endpoint with the document summarization service."
B: ["API_CONTRACT"] — "integrate" fires API_CONTRACT. "GPT API endpoint" = object context.
CompiledPredicateIds: ["API_CONTRACT"]
