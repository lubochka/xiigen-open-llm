# Layer 0-C — Knowledge Edge Pairs (045)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-045-001 — LLM (large language model) gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| LLM (standalone) | No code verb | gap-ai-ml-model-001 |
| LLM wrapper (with "build") | Code verb present | IMPLEMENT fires |
| LLM inference endpoint (with "integrate") | "integrate" present | API_CONTRACT fires |
| LLM endpoint (with "secure") | "secure" present | SECURITY fires |

---

## KE-045-002 — GPT (generative pre-trained transformer) gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| GPT (standalone) | No code verb | gap-ai-ml-model-001 |
| GPT client wrapper (with "build") | Code verb present | IMPLEMENT fires |
| GPT API endpoint (with "integrate") | "integrate" present | API_CONTRACT fires |
| GPT (with "document") | "document" present | DOCUMENT fires |

---

## KE-045-003 — LLM vs. GPT: both map to gap-ai-ml-model-001

Rule: Both "LLM" and "GPT" map to gap-ai-ml-model-001.
"GPT" is a specific LLM instance; it does not have a separate gap ID.
"The GPT LLM produces outputs via the inference endpoint" → [] → gap-ai-ml-model-001 (one event).
Multiple LLM model names in one request → one SCHEDULE_CAPABILITY_LEARNING.

---

## KE-045-004 — LLM wrapper is IMPLEMENT, not API_CONTRACT

Edge case: "Build a wrapper around the LLM" → IMPLEMENT (build = code verb; wrapper = software artifact).
Compare: "Integrate the LLM API" → API_CONTRACT ("integrate" fires API_CONTRACT).
The distinction: building a wrapper around a model is IMPLEMENT; connecting to it via integration is API_CONTRACT.

---

## KE-045-005 — Round-3 model vocabulary: gap-ai-ml-model-001 dominates

Rule: When both gap-ai-ml-model-001 (LLM, GPT, BERT, etc.) and gap-ai-ml-training-001 (gradient descent, etc.) terms appear in the same gap request (no code verb), gap-ai-ml-model-001 is the selected gap ID.
Rationale: Model-level vocabulary is more specific to the user's operational concern than training vocabulary.
Example: "LLM trained with gradient descent" → gap-ai-ml-model-001 (model dominant).
