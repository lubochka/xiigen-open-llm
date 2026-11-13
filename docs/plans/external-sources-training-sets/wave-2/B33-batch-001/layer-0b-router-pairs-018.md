# Layer 0-B — Router Pairs (018)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-018-001 — Gap → GapHandler: "reinforcement learning" strategy

A: [] | "Use RLHF (Reinforcement Learning from Human Feedback) to improve agent response quality."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "reinforcement learning" = AI training vocabulary. "use" in AI context does not fire a code verb. Gap ID: gap-ai-agent-llm-001.

---

## C0B-018-002 — Gap → GapHandler: "reinforcement loop" for agent

A: [] | "Set up a reinforcement loop so the agent improves with every customer interaction."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "set up a reinforcement loop" = AI training strategy. No code artifact. Gap ID: gap-ai-agent-llm-001.

---

## C0B-018-003 — IMPLEMENT: "build reinforcement feedback collector"

A: ["IMPLEMENT"] | "Build a reinforcement feedback collector that stores user ratings for model retraining."
B: IMPLEMENT → ImplementationFlow41. TransitionType: feature_addition.
Reason: "build" fires IMPLEMENT. "feedback collector" is the software artifact.

---

## C0B-018-004 — IMPLEMENT+TEST: implement RLHF reward scorer + tests

A: ["IMPLEMENT", "TEST"] | "Implement the RLHF reward scorer service and write unit tests for scoring edge cases."
B: IMPLEMENT → 4.1; TEST → E2ETestingFlow53 (5.1). TransitionType: feature_addition.

---

## C0B-018-005 — Gap disambiguation: "reinforcement" vs "implement reinforcement infrastructure"

"reinforcement learning for the agent" → GapHandler (AI vocabulary → gap)
"implement the reinforcement training infrastructure" → ImplementationFlow41 (code verb fires)
Teaching: "reinforcement" and "reinforcement learning" are AI vocabulary. Only code verbs applied to software infrastructure fire IMPLEMENT.
