# Layer 0-C — Knowledge Edge Pairs (027)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-027-001 — "neural network" is AI model type vocabulary

"neural network" = a class of machine learning models inspired by biological neural structures.
Rule: "the neural network needs more layers" → gap. "build neural network training infrastructure" → IMPLEMENT. "design neural network architecture" → ARCHITECTURE.

---

## KE-027-002 — "training loop" is AI model training vocabulary

"training loop" = the iterative process of forward pass, loss computation, and weight update during model training.
Rule: "the training loop runs for 100 epochs" → gap. "build the training loop executor" → IMPLEMENT. "training loop" is never an API_CONTRACT or SECURITY trigger.

---

## KE-027-003 — "gradient descent" is AI optimization vocabulary

"gradient descent" = the algorithm for minimizing loss by updating weights in the direction of steepest descent.
Rule: "gradient descent is not converging" → gap. "implement gradient descent optimizer" → IMPLEMENT. Part of gap-ai-ml-training-001.

---

## KE-027-004 — "backpropagation" is AI training vocabulary

"backpropagation" = the algorithm for computing gradients through a neural network using the chain rule.
Rule: "backpropagation is failing" → gap. "implement a backpropagation engine" → IMPLEMENT. Always gap without code verb.

---

## KE-027-005 — New gap IDs for Round 2 AI/ML vocabulary

Two new gap IDs introduced for Round 2 vocabulary:
- gap-ai-ml-model-001: tokenize, embed, vector search, RAG, context window, attention mechanism, transformer, inference endpoint, model serving, hallucination mitigation
- gap-ai-ml-training-001: neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark (AI), evaluation metric, dataset curation
DomainId for both: software-development (same as all B33 gap IDs).
