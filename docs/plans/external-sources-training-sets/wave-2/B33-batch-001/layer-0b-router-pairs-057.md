# Layer 0-B — Router Pairs (057)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-057-001 — Gap: GPT + Llama + BERT vocabulary list

A: [] | "GPT, Llama, and BERT are all transformer-based LLMs with different pretraining objectives."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Comparative description. No code verb. Four AI model names. Gap ID: gap-ai-ml-model-001. One event.

---

## C0B-057-002 — IMPLEMENT: build model-agnostic LLM wrapper

A: ["IMPLEMENT"] | "Build a model-agnostic LLM wrapper that works with GPT, BERT, and Llama backends."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. All AI model names in object position.

---

## C0B-057-003 — Gap: AI agent + LLM orchestration description

A: [] | "AI agents orchestrate LLM calls to complete complex reasoning tasks."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive (orchestrate = AI behavior description here, not imperative). No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-057-004 — IMPLEMENT: build LLM orchestration service for AI agent

A: ["IMPLEMENT"] | "Build an LLM orchestration service that manages AI agent tool calls and memory."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "LLM orchestration service" = implementation artifact.

---

## C0B-057-005 — ARCHITECTURE+IMPLEMENT+TEST: full build cycle round-3

A: ["ARCHITECTURE", "IMPLEMENT", "TEST"] | "Design the multi-agent architecture, implement the LLM gateway, and write tests for the knowledge graph connector."
B: ARCHITECTURE → ArchitectureFlow31; IMPLEMENT → ImplementationFlow41; TEST → TestFlow51.
Teaching: Three code verbs, three flows. Standard order: ARCHITECTURE → IMPLEMENT → TEST.
