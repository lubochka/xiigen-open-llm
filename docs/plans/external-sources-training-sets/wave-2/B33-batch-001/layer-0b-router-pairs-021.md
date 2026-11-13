# Layer 0-B — Router Pairs (021)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-021-001 — Gap → GapHandler: "adapt the agent"

A: [] | "Adapt the agent to handle Spanish-language customer queries."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "adapt" = AI adaptation vocabulary → gap. Gap ID: gap-ai-agent-adapt-001.

---

## C0B-021-002 — IMPLEMENT fires: "build the agent"

A: ["IMPLEMENT"] | "Build the agent that handles Spanish-language customer queries."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build the agent" = code verb + software artifact → IMPLEMENT. Same object (agent, Spanish-language) produces different routing depending on the verb.

---

## C0B-021-003 — Gap → GapHandler: "adapt" as primary action

A: [] | "Adapt the existing agent for a new vertical rather than build a new one."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "adapt" is the primary action. "build" is in a negative subordinate clause ("rather than"). Primary verb determines routing: "adapt" → gap. Gap ID: gap-ai-agent-adapt-001.

---

## C0B-021-004 — IMPLEMENT fires: "build" as explicit primary action

A: ["IMPLEMENT"] | "Instead of adapting the agent, build a dedicated vertical-specific agent from scratch."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" is the primary action verb. "adapting" appears in a negative subordinate clause and does not fire a predicate. IMPLEMENT fires on the primary verb only.

---

## C0B-021-005 — Gap → GapHandler: "evolve the agent"

A: [] | "Evolve the agent over time by incorporating user feedback into its behavior."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "evolve the agent" = AI capability evolution vocabulary → gap. Gap ID: gap-ai-agent-adapt-001.
