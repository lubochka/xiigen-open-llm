# Layer 0-C — Knowledge Edge Pairs (046)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-046-001 — BERT gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| BERT (standalone) | No code verb | gap-ai-ml-model-001 |
| BERT inference client (with "build") | Code verb present | IMPLEMENT fires |
| BERT endpoint (with "integrate") | "integrate" present | API_CONTRACT fires |
| BERT embeddings (with "implement") | Code verb present | IMPLEMENT fires |

---

## KE-046-002 — Llama gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| Llama (standalone) | No code verb | gap-ai-ml-model-001 |
| Llama API client (with "implement") | Code verb present | IMPLEMENT fires |
| Llama serving architecture (with "design") | "design" + structural noun | ARCHITECTURE fires |
| Llama endpoint (with "integrate") | "integrate" present | API_CONTRACT fires |

---

## KE-046-003 — BERT vs. Llama: both gap-ai-ml-model-001, both IMPLEMENT when wrapped

Rule: BERT and Llama are both specific AI model families → gap-ai-ml-model-001.
"BERT inference client" and "Llama API client" are both software artifacts → IMPLEMENT when code verb present.
Neither BERT nor Llama has a special predicate. They are always object context.

---

## KE-046-004 — ARCHITECTURE+IMPLEMENT pattern for LLM serving layer

Pattern: "Design the [model name] serving architecture and implement the [software artifact]" → ARCHITECTURE + IMPLEMENT.
"Design the Llama serving architecture and implement the request routing layer" → [ARCHITECTURE, IMPLEMENT].
Canonical ordering: ARCHITECTURE first (must complete before IMPLEMENT starts).

---

## KE-046-005 — Model name acronyms are always gap unless preceded by code verb

Rule: GPT, BERT, LLM, RAG (when used as a model pattern), RLHF (reinforcement learning from human feedback) — all model/method names → gap.
v6 corruption risk: treating model names as predicates (e.g., "BERT" → "BERT_ENCODE" predicate). Correct: no such predicate exists.
All model names → gap-ai-ml-model-001 or gap-ai-ml-training-001 depending on category.
