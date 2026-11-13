# Layer 0-D — Flow Factory Pairs (043)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-043-001 — ARCHITECTURE+IMPLEMENT+TEST: full build cycle round-3

A: ["ARCHITECTURE", "IMPLEMENT", "TEST"] | "Design the multi-agent architecture, implement the LLM gateway, and write tests for the knowledge graph connector."
Factory: [ArchitectureFlow31, ImplementationFlow41, TestFlow51] | TransitionType: feature_addition
Ordering: ArchitectureFlow31 → ImplementationFlow41 → TestFlow51.

---

## FF-043-002 — GapHandler: GPT + Llama + BERT vocabulary

A: [] | "GPT, Llama, and BERT are all transformer-based LLMs with different pretraining objectives."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: Comparative description. No code verb. Four AI model names. One event.

---

## FF-043-003 — IMPLEMENT: build model-agnostic LLM wrapper

A: ["IMPLEMENT"] | "Build a model-agnostic LLM wrapper that works with GPT, BERT, and Llama backends."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "model-agnostic LLM wrapper" = software artifact.

---

## FF-043-004 — GapHandler: LLM + training vocabulary bridge

A: [] | "The LLM was pretrained using gradient descent and fine-tuned with RLHF."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: Descriptive. No code verb. Mixed gap terms. → gap-ai-ml-model-001 (dominant).

---

## FF-043-005 — IMPLEMENT+SECURITY: implement LLM client + secure prompt injection

A: ["IMPLEMENT", "SECURITY"] | "Implement the LLM client library and secure it against prompt injection attacks."
Factory: [ImplementationFlow41, SecurityAuditFlow] | TransitionType: feature_addition
Ordering: ImplementationFlow41 → SecurityAuditFlow (IMPLEMENT before SECURITY).
