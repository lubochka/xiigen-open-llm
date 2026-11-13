# Layer 0-C — Knowledge Edge Pairs (042)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-042-001 — Dataset curation gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| dataset curation (standalone) | No code verb | gap-ai-ml-training-001 |
| dataset curation pipeline (with "build") | Code verb present | IMPLEMENT fires |
| dataset curation output (with "integrate") | "integrate" present | API_CONTRACT fires |

---

## KE-042-002 — Complete gap-ai-ml-training-001 vocabulary list

All terms that trigger gap-ai-ml-training-001 (no code verb required):
neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark (AI evaluation), evaluation metric (F1/BLEU/ROUGE), dataset curation.

Rule: Any number of these terms in a single request → one SCHEDULE_CAPABILITY_LEARNING → gap-ai-ml-training-001.

---

## KE-042-003 — Training vocabulary IMPLEMENT override pattern

Code verb pattern for training vocabulary:
- "build/implement/create/develop a [training vocabulary] [service/module/pipeline/wrapper/runner/harness]" → IMPLEMENT
- The noun after the AI training term must be a software artifact noun.
- "Build a dataset curation pipeline" → IMPLEMENT. (pipeline = software artifact)
- "Build dataset curation" (no artifact noun) → IMPLEMENT (verb still fires).

---

## KE-042-004 — API_CONTRACT: "integrate" always fires regardless of object

Rule: "integrate" fires API_CONTRACT in all contexts.
"integrate the dataset curation output with the training API" → API_CONTRACT.
"integrate the evaluation metric results with the dashboard API" → API_CONTRACT.
"integrate" never requires a software artifact noun to fire API_CONTRACT.

---

## KE-042-005 — TransitionType for training vocabulary requests

Rule: TransitionType determination is independent of AI training vocabulary.
- Request includes "implement" or "build" → TransitionType: feature_addition (unless "refactor" present)
- Request includes "refactor" without new feature → TransitionType: refactoring
- Request is documentation-only → TransitionType: documentation
Training vocabulary does not modify TransitionType. Same hierarchy as all batches.
