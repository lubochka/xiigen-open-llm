# Layer 0-C — Knowledge Edge Pairs (037)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-037-001 — Neural network gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| neural network (standalone) | No code verb | gap-ai-ml-training-001 |
| neural network (with "build/implement/create") | Code verb present | IMPLEMENT fires; gap skipped |
| neural network architecture (with "design") | "design" + structural noun | ARCHITECTURE fires |
| trained neural network (with "integrate") | "integrate" present | API_CONTRACT fires |

---

## KE-037-002 — Training loop gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| training loop (standalone) | No code verb | gap-ai-ml-training-001 |
| training loop (with "implement/build") | Code verb present | IMPLEMENT fires; gap skipped |
| training loop orchestration (with "design") | "design" + structural noun | ARCHITECTURE fires |

---

## KE-037-003 — Neural network vs. training loop distinction

Rule: Both "neural network" and "training loop" map to gap-ai-ml-training-001.
When both appear in a single request without a code verb → one SCHEDULE_CAPABILITY_LEARNING event, one gap ID.
Example: "Neural network training loop needs optimization" → [] → gap-ai-ml-training-001 (one event).

---

## KE-037-004 — Code verb override for neural network + training loop

When any code verb appears:
- "build neural network" → IMPLEMENT (neural network = object)
- "integrate trained neural network" → API_CONTRACT
- "design training loop architecture" → ARCHITECTURE
- "write tests for neural network" → TEST
Code verb always determines outcome; AI training vocabulary never determines predicate.

---

## KE-037-005 — DomainId: software-development for all training gap terms

Rule: neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation → all gap-ai-ml-training-001 → DomainId: software-development.
No "ml-engineering", "data-science", or "ai-training" domain exists. Always software-development.
