# Layer 0-D — Flow Factory Pairs (020)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-020-001 — GapHandler: "orchestrate agent workflows" — AI context

A: [] | "Orchestrate the agent workflows so tasks hand off between specialist agents."
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001
Reason: "orchestrate" in AI context = operational AI vocabulary → gap.

---

## FF-020-002 — API_CONTRACT: "orchestrate via API" boundary

A: ["API_CONTRACT"] | "Orchestrate the agent calls via the orchestration API to manage routing."
Factory: [APIContractFlow33]
Reason: "via API" pattern → API_CONTRACT fires. "orchestrate" AI context does not suppress.

---

## FF-020-003 — ARCHITECTURE: "architect the orchestration layer"

A: ["ARCHITECTURE"] | "Architect the orchestration layer that coordinates tasks across specialist agents."
Factory: [ArchitectureFlow31] | Sub-phase: 3.1
Reason: "architect" = design verb + "layer" = structural noun → ARCHITECTURE.

---

## FF-020-004 — IMPLEMENT: "build orchestration service"

A: ["IMPLEMENT"] | "Build the agent orchestration service that routes tasks to the appropriate specialist."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "orchestrate" (AI verb) → gap; "build orchestration service" (code verb + software artifact) → IMPLEMENT.

---

## FF-020-005 — Factory selection summary: orchestration vocabulary

| Request form | Factory selected | Reason |
|-------------|-----------------|--------|
| orchestrate agent workflows | GapHandler | AI operational verb |
| orchestrate via API | APIContractFlow33 | "via API" pattern |
| architect the orchestration layer | ArchitectureFlow31 | Design verb + structural noun |
| build orchestration service | ImplementationFlow41 | Code verb |
| orchestrate AI workflows | GapHandler | AI operational verb |
