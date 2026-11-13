# Layer 0-D — Flow Factory Pairs (005)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-005-001 — GapHandler: "fine-tune"

A: [] | "Fine-tune the LLM on our support ticket data"
Factory: GapHandler | SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-agent-llm-001

---

## FF-005-002 — GapHandler: "orchestrate agents"

A: [] | "Orchestrate the AI agents to handle complex queries"
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001

---

## FF-005-003 — GapHandler: "adapt behavior"

A: [] | "Adapt the agent behavior for the French market"
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001

---

## FF-005-004 — GapHandler: "RAG accuracy"

A: [] | "Improve RAG retrieval accuracy to 90%"
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001

---

## FF-005-005 — IMPLEMENT fires on "build RAG"

A: ["IMPLEMENT"] | "Build the RAG retrieval service"
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" overrides "RAG" as AI vocabulary. Code verb determines predicate.
