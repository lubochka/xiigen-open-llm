# Layer 0-D — Flow Factory Pairs (006)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-006-001 — GapHandler: "token budget"

A: [] | "Optimize the token budget for the LLM calls"
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001

---

## FF-006-002 — GapHandler: "agent persona"

A: [] | "Define the agent persona for our brand voice"
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001

---

## FF-006-003 — IMPLEMENT: "build guardrails"

A: ["IMPLEMENT"] | "Build content safety guardrails for agent output"
Factory: [ImplementationFlow41] | feature_addition

---

## FF-006-004 — IMPLEMENT+SECURITY: content safety + injection prevention

A: ["IMPLEMENT", "SECURITY"] | "Build safety filters and prevent prompt injection"
Factory: [ImplementationFlow41, SecurityAuditFlow] | TransitionType: feature_addition

---

## FF-006-005 — v6-02 corruption: gap → IMPLEMENT (adapt = implement)

A: [] | "Adapt the agent to the healthcare domain"
Correct: GapHandler
Corrupt: [ImplementationFlow41] ← v6-02 ("adapt" incorrectly maps to IMPLEMENT)
Teaching: "adapt" (AI domain adaptation) is gap. IMPLEMENT requires code-writing verbs.
