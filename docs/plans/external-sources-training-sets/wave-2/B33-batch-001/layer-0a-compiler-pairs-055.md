# Layer 0-A — Compiler Pairs (055)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-055-001 — gap: AI agent + neural network combined

A: "AI agents use neural networks for decision-making and action selection."
B: [] — Descriptive. No code verb. Both gap terms. → gap-ai-ml-model-001. One event.
CompiledPredicateIds: []

---

## C0A-055-002 — IMPLEMENT: build AI agent with neural network backbone

A: "Build an AI agent that uses a neural network backbone for intent classification."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. Both AI terms in object position.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-055-003 — gap: LLM + training vocabulary bridge

A: "The LLM was pretrained on a large dataset using gradient descent and fine-tuned with RLHF."
B: [] — Descriptive history. No code verb. Multiple gap terms. → gap-ai-ml-model-001. One event.
CompiledPredicateIds: []

---

## C0A-055-004 — IMPLEMENT+SECURITY: implement LLM client + secure prompt injection

A: "Implement the LLM client library and secure it against prompt injection attacks."
B: ["IMPLEMENT", "SECURITY"] — "implement" fires IMPLEMENT; "secure" + "prompt injection" fires SECURITY. Two predicates.
CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]

---

## C0A-055-005 — DOCUMENT+QA: document AI stack + QA

A: "Document the LLM integration design and QA the vector database connector."
B: ["DOCUMENT", "QA"] — "document" fires DOCUMENT; "QA" fires QA (last). Two predicates.
CompiledPredicateIds: ["DOCUMENT", "QA"]
