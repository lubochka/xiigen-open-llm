# Layer 0-C — Knowledge Edge Pairs (026)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-026-001 — "attention mechanism" is AI model internal vocabulary

"attention mechanism" = the mathematical component within a transformer model that weights token importance. Model internals.
Rule: "tune the attention mechanism" → gap. "implement custom attention" → IMPLEMENT. Not a software design concept.

---

## KE-026-002 — "transformer" is AI model architecture vocabulary

"transformer" = a class of neural network architectures using attention. Not a software design pattern in this context.
Rule: "adjust the transformer architecture" → gap. "build a transformer wrapper" → IMPLEMENT. "transformer" as adjective (transformer-based) → gap vocabulary in object, code verb still fires.

---

## KE-026-003 — "inference endpoint" boundary: gap vs API_CONTRACT vs IMPLEMENT

"inference endpoint" = an HTTP (HyperText Transfer Protocol) endpoint that serves AI model predictions.
Rule: "the inference endpoint serves predictions" → gap. "integrate the inference endpoint" → API_CONTRACT. "build the inference serving layer" → IMPLEMENT.

---

## KE-026-004 — "model serving" is AI operational vocabulary

"model serving" = the infrastructure pattern for deploying and running AI models in production.
Rule: "model serving latency is high" → gap. "optimize model serving" alone → gap. "refactor the serving pipeline" → IMPLEMENT. "secure the serving endpoint" → SECURITY.

---

## KE-026-005 — "hallucination mitigation" is AI quality vocabulary

"hallucination mitigation" = techniques to reduce incorrect or fabricated AI model outputs.
Rule: "apply hallucination mitigation" → gap. "build a hallucination guard" → IMPLEMENT. No dedicated predicate for hallucination; it is always gap unless a code verb precedes.
