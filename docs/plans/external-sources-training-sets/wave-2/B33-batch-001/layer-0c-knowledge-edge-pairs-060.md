# Layer 0-C — Knowledge Edge Pairs (060)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-060-001 — Complete B33 AI vocabulary gap trigger list (final reference)

Complete list of all AI/ML gap vocabulary terms across all B33 rounds:

**gap-ai-ml-model-001 triggers:**
tokenize, embed (AI context), vector search, RAG (retrieval-augmented generation) pipeline, context window, attention mechanism, transformer (AI architecture), inference endpoint (standalone), model serving (standalone), latency optimization (AI context), hallucination mitigation, LLM (large language model), GPT (generative pre-trained transformer), BERT (bidirectional encoder representations from transformers), Llama (AI model family), embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent.

**gap-ai-ml-training-001 triggers:**
neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark (AI evaluation), evaluation metric (F1/BLEU/ROUGE), dataset curation.

---

## KE-060-002 — Code verbs that override all AI vocabulary (complete reference)

| Code verb | Predicate | Notes |
|-----------|-----------|-------|
| build, implement, create, develop, write (code), refactor | IMPLEMENT | Fires on software artifacts |
| design, architect + structural noun | ARCHITECTURE | Structural noun required |
| integrate, connect, wire, interface with | API_CONTRACT | Always fires; no restrictions |
| secure, harden + injection context | SECURITY | "prompt injection" alone sufficient |
| write tests, create tests, add tests | TEST | Test-writing form required |
| document, write docs | DOCUMENT | Documentation verb |
| QA (abbreviation) | QA | Always last; absolute |

---

## KE-060-003 — DomainId: software-development — absolute rule for all B33 gaps

Rule: All B33 gap IDs (gap-ai-ml-model-001, gap-ai-ml-training-001, gap-ai-agent-adapt-001, gap-ai-agent-skill-001, gap-ai-agent-llm-001) use DomainId: software-development.
No other domain exists. Never: ai-development, ml-development, llm-development, agent-development, data-science.

---

## KE-060-004 — Single code verb suppresses all B33 gap vocabulary (final rule)

Rule: One code predicate firing → all gap vocabulary from all rounds in the same request is silently skipped.
No SCHEDULE_CAPABILITY_LEARNING emitted. No gap warning required (for round-2 and round-3 terms).
Exception: gap-ai-agent-* terms may emit GapWarning (noted in earlier files).

---

## KE-060-005 — v6 corruption catalogue: complete B33 summary

v6-01 (invented predicates): FINE_TUNE, FEW_SHOT, REASONING, SELF_HEAL, REINFORCE, AI_AGENT, LLM_CALL, VECTOR_SEARCH, BERT_ENCODE, GRADIENT_DESCENT, BACKPROP, NEURAL_NET → all invented, none valid. Correct output: [] → gap.
v6-02 (gap treated as code verb): adapt→IMPLEMENT, orchestrate→ARCHITECTURE, fine-tune→IMPLEMENT, semantic-search→IMPLEMENT → all incorrect. Correct: [] → gap.
v6-03 (multi-predicate collapse): "build + integrate" → ["IMPLEMENT"] only → incorrect. Correct: ["IMPLEMENT", "API_CONTRACT"].
