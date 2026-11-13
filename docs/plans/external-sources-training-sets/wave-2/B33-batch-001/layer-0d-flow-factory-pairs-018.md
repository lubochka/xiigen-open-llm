# Layer 0-D — Flow Factory Pairs (018)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-018-001 — GapHandler: "reinforcement learning" strategy

A: [] | "Use RLHF (Reinforcement Learning from Human Feedback) to improve agent response quality."
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: "reinforcement learning" = AI training vocabulary. "use" does not fire a code predicate.

---

## FF-018-002 — GapHandler: "reinforcement loop" for agent

A: [] | "Set up a reinforcement loop so the agent improves with every customer interaction."
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: "set up a reinforcement loop" = AI training strategy. No code artifact in scope.

---

## FF-018-003 — IMPLEMENT: "build reinforcement feedback collector"

A: ["IMPLEMENT"] | "Build a reinforcement feedback collector that stores user ratings."
Factory: [ImplementationFlow41] | TransitionType: feature_addition

---

## FF-018-004 — IMPLEMENT+TEST: implement RLHF reward scorer + tests

A: ["IMPLEMENT", "TEST"] | "Implement the RLHF reward scorer service and write unit tests for scoring."
Factory: [ImplementationFlow41, E2ETestingFlow53] | TransitionType: feature_addition

---

## FF-018-005 — Factory selection summary: reinforcement vocabulary

| Request form | Factory selected | Reason |
|-------------|-----------------|--------|
| RLHF for the agent | GapHandler | AI training vocabulary |
| set up reinforcement loop | GapHandler | AI strategy |
| build reinforcement feedback collector | ImplementationFlow41 | Code verb |
| implement RLHF reward scorer | ImplementationFlow41 | Code verb |
| integrate reinforcement pipeline with feedback API | APIContractFlow33 | "integrate" always |
