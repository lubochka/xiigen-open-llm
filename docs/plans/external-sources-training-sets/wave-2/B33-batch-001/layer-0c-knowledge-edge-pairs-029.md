# Layer 0-C — Knowledge Edge Pairs (029)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-029-001 — "evaluation metric" disambiguation: AI vs TEST

"evaluation metric" = a quantitative measure of AI model quality (F1, BLEU, ROUGE, accuracy, precision, recall).
Rule: "report evaluation metrics" → gap. "create tests that measure F1 score" → TEST (code verb "create tests" fires TEST). Metric name alone is gap vocabulary.

---

## KE-029-002 — "dataset curation" is AI data preparation vocabulary

"dataset curation" = the process of collecting, filtering, cleaning, and labeling training data for AI models.
Rule: "dataset curation needs to remove low-quality examples" → gap. "implement a dataset curation pipeline" → IMPLEMENT. "dataset curation" as subject of a description → gap.

---

## KE-029-003 — Adjective AI terms do not suppress code verbs

Adjectives formed from AI vocabulary (RAG-powered, transformer-based, embedding-enabled, hallucination-mitigating, AI-driven) do not prevent code verbs from firing.
Examples:
- "Build a RAG-powered search service" → ["IMPLEMENT"] (adjective does not suppress)
- "Design a transformer-based architecture" → ["ARCHITECTURE"] (adjective does not suppress)
- "Secure the embedding-enabled endpoint" → ["SECURITY"] (adjective does not suppress)

---

## KE-029-004 — One gap ID per Round 2 request — selection priority

Round 2 gap ID selection (when multiple terms appear):
1. gap-ai-ml-model-001 — model-level vocabulary dominant: tokenize, embed, RAG, vector search, context window, attention, transformer, inference, serving, hallucination
2. gap-ai-ml-training-001 — training vocabulary dominant: neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation
Apply the most specific matching gap ID for the dominant vocabulary in the request.

---

## KE-029-005 — DomainId for Round 2 gap IDs

Both gap-ai-ml-model-001 and gap-ai-ml-training-001 use DomainId: software-development.
No "ai-development", "ml-development", "data-science", or "model-development" domain exists in the registry.
This is consistent with all B33 gap IDs.
