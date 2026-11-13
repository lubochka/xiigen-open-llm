# Layer 0-C — Knowledge Edge Pairs (035)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-035-001 — AI compound gap vocabulary: multiple gap terms, one event

Rule: Any number of Round 2 gap vocabulary terms in one request → exactly one SCHEDULE_CAPABILITY_LEARNING, one gap ID.
Example: "tokenize, embed, vector search, RAG, context window, attention mechanism, transformer" → one event, gap-ai-ml-model-001.
Example: "neural network, training loop, gradient descent, backpropagation, hyperparameter" → one event, gap-ai-ml-training-001.
This rule is absolute; no stacking of events per vocabulary term.

---

## KE-035-002 — IMPLEMENT fires regardless of AI noun in object

Examples showing IMPLEMENT fires with any AI noun as object:
- "build a tokenizer" → IMPLEMENT
- "implement an embedding service" → IMPLEMENT
- "create a RAG retrieval component" → IMPLEMENT
- "develop a context window tracker" → IMPLEMENT
- "write code for the attention layer" → IMPLEMENT
- "refactor the inference serving module" → IMPLEMENT
The object being an AI component does not restrict or alter the predicate.

---

## KE-035-003 — SECURITY fires with AI endpoint as object

Examples showing SECURITY fires with AI infrastructure as object:
- "secure the inference endpoint against unauthorized access" → SECURITY
- "harden the model serving layer" → SECURITY
- "prevent prompt injection on the RAG pipeline endpoint" → SECURITY (prompt injection = SECURITY exception)
- "audit the embedding service for data leakage" → SECURITY
AI vocabulary in object position does not suppress SECURITY.

---

## KE-035-004 — DOCUMENT fires with AI vocabulary as object

Examples showing DOCUMENT fires with AI concepts as object:
- "document the context window management policy" → DOCUMENT
- "write docs for the inference endpoint API" → DOCUMENT
- "document the hyperparameter tuning procedure" → DOCUMENT
AI vocabulary as documentation subject does not change the predicate.

---

## KE-035-005 — QA always last regardless of AI vocabulary volume

Rule: QA is always the final flow regardless of how much AI vocabulary appears in the request.
Example: "Architect the transformer platform, implement the RAG pipeline, integrate the embedding API, secure the inference endpoint, test vector search, document context window policy, QA."
→ QA is last after all other flows complete. AI vocabulary does not alter QA position.
