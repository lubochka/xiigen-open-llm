# Layer 0-C — Knowledge Edge Pairs (058)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-058-001 — Complete training vocabulary rules summary

All gap-ai-ml-training-001 terms and their code-verb override patterns:

| Term | Gap (no verb) | IMPLEMENT verb | API_CONTRACT |
|------|--------------|----------------|--------------|
| neural network | gap | build/implement | integrate (with API) |
| training loop | gap | implement | integrate (with API) |
| gradient descent | gap | implement | integrate (with API) |
| backpropagation | gap | implement/write | integrate (with API) |
| hyperparameter | gap | build | integrate |
| overfitting | gap | build (detector) | — |
| regularization | gap | implement | — |
| cross-validation | gap | create/build | — |
| benchmark | gap | build (harness) | — |
| evaluation metric | gap | develop | — |
| dataset curation | gap | build | integrate |

---

## KE-058-002 — Complete round-3 model vocabulary rules summary

All gap-ai-ml-model-001 round-3 terms and override patterns:

| Term | Gap (no verb) | IMPLEMENT verb | API_CONTRACT |
|------|--------------|----------------|--------------|
| LLM | gap | build (wrapper) | integrate |
| GPT | gap | build (client) | integrate |
| BERT | gap | build (client) | integrate |
| Llama | gap | implement (client) | integrate |
| embedding model | gap | build (client) | integrate |
| vector database | gap | implement (connector) | integrate |
| semantic search | gap | build (service) | integrate |
| knowledge graph | gap | implement (adapter) | integrate |
| AI agent | gap | build (runtime) | — |
| multi-agent | gap | implement (broker) | — |

---

## KE-058-003 — Gap-ai-ml-model-001 dominates gap-ai-ml-training-001 when combined

Rule: In mixed gap requests (no code verb) containing both round-3 model and round-2 training terms:
→ gap-ai-ml-model-001 is selected.
Example: "LLM trained with gradient descent" → gap-ai-ml-model-001.

---

## KE-058-004 — API_CONTRACT fires on "integrate" for vector database

Rule: "Integrate the vector database write API with the semantic search indexing service" → API_CONTRACT.
"Vector database write API" is an API endpoint → "integrate" fires API_CONTRACT.
Both AI terms (vector database, semantic search) in object position.

---

## KE-058-005 — Single code verb suppresses all gap vocabulary in request

Rule (final restatement): One code verb present → all gap vocabulary in that request is silently skipped.
"LLM, GPT, BERT, neural network, training loop, gradient descent — integrate results with reporting API" → API_CONTRACT only.
All gap terms from all rounds silently skipped. No SCHEDULE_CAPABILITY_LEARNING emitted.
