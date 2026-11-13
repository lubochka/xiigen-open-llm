# Layer 0-D — Flow Factory Pairs (021)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-021-001 — GapHandler: "adapt the agent"

A: [] | "Adapt the agent to handle Spanish-language customer queries."
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001
Reason: "adapt" = AI adaptation vocabulary → gap.

---

## FF-021-002 — IMPLEMENT: "build the agent"

A: ["IMPLEMENT"] | "Build the agent that handles Spanish-language customer queries."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: Same object, different verb. "adapt" → GapHandler. "build" → ImplementationFlow41.

---

## FF-021-003 — GapHandler: primary action is "adapt"

A: [] | "Adapt the existing agent for a new vertical rather than build a new one."
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001
Reason: "adapt" is the primary action. "build" is in a negative subordinate clause → does not fire.

---

## FF-021-004 — IMPLEMENT: primary action is "build"

A: ["IMPLEMENT"] | "Instead of adapting the agent, build a vertical-specific agent from scratch."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: Primary verb determines routing. "build" fires IMPLEMENT; "adapting" in negative clause does not.

---

## FF-021-005 — Factory selection summary: adapt vs build boundary

| Request form | Factory selected | Reason |
|-------------|-----------------|--------|
| adapt the agent | GapHandler | AI adaptation verb |
| build the agent | ImplementationFlow41 | Code verb |
| adapt the existing agent (primary) | GapHandler | Primary verb determines routing |
| build instead of adapting (primary) | ImplementationFlow41 | Primary verb determines routing |
| evolve the agent | GapHandler | AI evolution verb |
