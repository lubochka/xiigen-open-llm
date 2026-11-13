# Layer 0-C — Knowledge Edge Pairs (030)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-030-001 — Round 2 AI/ML gap vocabulary reference table

| Term | Gap category | Gap ID |
|------|-------------|--------|
| tokenize (AI input) | Model-level operation | gap-ai-ml-model-001 |
| embed (AI vector conversion) | Model-level operation | gap-ai-ml-model-001 |
| vector search | AI retrieval method | gap-ai-ml-model-001 |
| RAG pipeline (standalone goal) | AI system pattern | gap-ai-ml-model-001 |
| context window | AI model property | gap-ai-ml-model-001 |
| attention mechanism | AI model component | gap-ai-ml-model-001 |
| transformer (AI architecture) | AI model type | gap-ai-ml-model-001 |
| inference endpoint (standalone) | AI serving description | gap-ai-ml-model-001 |
| model serving (standalone) | AI operational concern | gap-ai-ml-model-001 |
| hallucination mitigation | AI quality concern | gap-ai-ml-model-001 |
| neural network | AI model type | gap-ai-ml-training-001 |
| training loop | AI training operation | gap-ai-ml-training-001 |
| gradient descent | AI optimization algorithm | gap-ai-ml-training-001 |
| backpropagation | AI training mechanism | gap-ai-ml-training-001 |
| hyperparameter | AI training parameter | gap-ai-ml-training-001 |
| overfitting | AI quality problem | gap-ai-ml-training-001 |
| regularization (standalone) | AI training technique | gap-ai-ml-training-001 |
| cross-validation | AI evaluation method | gap-ai-ml-training-001 |
| benchmark (AI evaluation) | AI evaluation activity | gap-ai-ml-training-001 |
| evaluation metric (F1/BLEU/ROUGE) | AI quality measurement | gap-ai-ml-training-001 |
| dataset curation | AI data preparation | gap-ai-ml-training-001 |

---

## KE-030-002 — Code verb override table for Round 2 AI vocabulary

| Code verb | Predicate | Works with AI objects |
|-----------|-----------|----------------------|
| build, implement, create, develop, write code, refactor | IMPLEMENT | Yes — code verb wins |
| design, architect + structural noun | ARCHITECTURE | Yes — code verb wins |
| integrate, connect, wire, interface with | API_CONTRACT | Yes — always fires |
| secure, harden + AI endpoint | SECURITY | Yes — code verb wins |
| write tests, create tests, add tests | TEST | Yes — metric names in object |
| document, write docs | DOCUMENT | Yes — code verb wins |
| QA | QA | Yes — always last |
