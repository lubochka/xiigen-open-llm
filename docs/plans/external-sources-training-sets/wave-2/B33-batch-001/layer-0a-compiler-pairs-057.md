# Layer 0-A — Compiler Pairs (057)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-057-001 — gap: GPT + Llama + BERT vocabulary list

A: "GPT, Llama, and BERT are all transformer-based LLMs with different pretraining objectives."
B: [] — Comparative description. No code verb. Four AI model names. → gap-ai-ml-model-001. One event.
CompiledPredicateIds: []

---

## C0A-057-002 — IMPLEMENT: build model-agnostic LLM wrapper

A: "Build a model-agnostic LLM wrapper that works with GPT, BERT, and Llama backends."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. All AI model names in object position.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-057-003 — gap: AI agent + LLM orchestration

A: "AI agents orchestrate LLM calls to complete complex reasoning tasks."
B: [] — Descriptive. No code verb. Both terms gap. Note: "orchestrate" here is descriptive (AI behavior description), not imperative. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-057-004 — IMPLEMENT: build LLM orchestration service for AI agent

A: "Build an LLM orchestration service that manages AI agent tool calls and memory."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "LLM orchestration service" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-057-005 — ARCHITECTURE+IMPLEMENT+TEST: full build cycle round-3

A: "Design the multi-agent architecture, implement the LLM gateway, and write tests for the knowledge graph connector."
B: ["ARCHITECTURE", "IMPLEMENT", "TEST"] — "design" + "architecture" fires ARCHITECTURE; "implement" fires IMPLEMENT; "write tests" fires TEST. Three predicates, standard order.
CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST"]
