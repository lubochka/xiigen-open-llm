# Layer 0-D — Flow Factory Pairs (011)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-011-001 — GapHandler: "chain-of-thought"

A: [] | "Add chain-of-thought prompting to the classifier agent"
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001

---

## FF-011-002 — IMPLEMENT: "implement chain-of-thought reasoning module"

A: ["IMPLEMENT"] | "Implement the chain-of-thought reasoning module as a software service"
Factory: [ImplementationFlow41] | feature_addition
Teaching: Same concept but with code verb → IMPLEMENT fires. Object is AI vocabulary; verb determines predicate.

---

## FF-011-003 — GapHandler: "grounding"

A: [] | "Improve factual grounding of the LLM responses"
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001

---

## FF-011-004 — IMPLEMENT+SECURITY: "build + secure"

A: ["IMPLEMENT", "SECURITY"] | "Build the input sanitization layer for the agent and prevent prompt injection"
Factory: [ImplementationFlow41, SecurityAuditFlow] | feature_addition

---

## FF-011-005 — GapHandler: "temperature and top-p"

A: [] | "Adjust the temperature to 0.3 and top-p to 0.9 for the support agent"
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: Model hyperparameter configuration → gap. No code verb.
