# Layer 0-C — Knowledge Edge Pairs (033)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-033-001 — "attention mechanism" vs "architect attention layer" distinction

"attention mechanism" in AI context:
1. "the attention mechanism needs adjustment" → gap (description of model internals)
2. "tune the attention mechanism" → gap ("tune" in AI context = gap, not IMPLEMENT)
3. "implement a custom attention module" → IMPLEMENT (code verb + artifact)
4. "design the attention layer architecture" → ARCHITECTURE (design + structural noun)
Note: "tune" when applied to AI model components = gap (model-level operation, not code operation).

---

## KE-033-002 — "inference endpoint" triple boundary

"inference endpoint" sits at the boundary of three predicates:
1. "the inference endpoint serves predictions" → gap (description, no code verb)
2. "integrate the inference endpoint" → API_CONTRACT (integrate verb)
3. "build the inference endpoint wrapper" → IMPLEMENT (build verb)
4. "secure the inference endpoint" → SECURITY (secure verb)
The predicate is entirely determined by the action verb applied to the endpoint.

---

## KE-033-003 — "RAG pipeline" implementation boundary

"RAG (Retrieval-Augmented Generation) pipeline" as a concept sits at the implementation boundary:
1. "set up a RAG pipeline" → gap ("set up" = AI configuration, not code verb)
2. "implement a RAG pipeline" → IMPLEMENT
3. "design the RAG architecture" → ARCHITECTURE
4. "integrate the RAG service" → API_CONTRACT
The distinction: does the request ask for model-level configuration or code artifact construction?

---

## KE-033-004 — "cross-validation" vs "write cross-validation tests" distinction

"cross-validation" as AI evaluation methodology:
1. "cross-validation shows inconsistent performance" → gap
2. "run cross-validation on the dataset" → gap ("run" in AI context = evaluation operation)
3. "implement a cross-validation framework" → IMPLEMENT
4. "write cross-validation tests" → TEST (write tests form)
"run" in AI evaluation context does not fire IMPLEMENT.

---

## KE-033-005 — Compound AI requests: gap selection when both gap ID categories present

When both gap-ai-ml-model-001 and gap-ai-ml-training-001 vocabulary appear in one request:
Rule: Use gap-ai-ml-model-001 if model-level vocabulary is dominant; gap-ai-ml-training-001 if training vocabulary is dominant.
Example: "tokenize and embed for the neural network training loop" → gap-ai-ml-model-001 (model operations lead).
Example: "run backpropagation and embed results for evaluation" → gap-ai-ml-training-001 (training operation leads).
One event, one gap ID regardless.
