# Layer 0-D — Flow Factory Pairs (010)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-010-001 — GapHandler: observation about agent

A: [] | "The agent is making incorrect decisions on financial queries"
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001
Reason: Observation statement → gap. No predicate-triggering verb.

---

## FF-010-002 — IMPLEMENT fires: "build the tool registry"

A: ["IMPLEMENT"] | "Build the tool registry for the agent's function-calling capabilities"
Factory: [ImplementationFlow41] | feature_addition

---

## FF-010-003 — API_CONTRACT: "define the tool schema"

A: ["API_CONTRACT"] | "Define the function-calling schema for the agent tool registry"
Factory: [APIContractFlow33] | TransitionType: documentation

---

## FF-010-004 — IMPLEMENT+API_CONTRACT: "build + define schema"

A: ["IMPLEMENT", "API_CONTRACT"] | "Build the tool registry and define its API schema"
Factory: [ImplementationFlow41, APIContractFlow33] | TransitionType: feature_addition

---

## FF-010-005 — GapHandler: "agent knowledge"

A: [] | "Update the agent's knowledge base with new product information"
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001
Reason: Content/knowledge update to AI agent → gap. No code-writing verb.
