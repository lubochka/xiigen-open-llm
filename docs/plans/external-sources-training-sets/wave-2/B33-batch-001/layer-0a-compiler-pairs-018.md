# Layer 0-A — Compiler Pairs (018)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-018-001 — gap: "reinforcement" learning request

A: "Use reinforcement learning from human feedback (RLHF) to improve agent response quality."
B: [] — "reinforcement learning from human feedback" = AI training vocabulary. "use" does not fire a code predicate. Gap.
CompiledPredicateIds: []

---

## C0A-018-002 — gap: "reinforcement" loop for agent behavior

A: "Set up a reinforcement loop so the agent improves with every customer interaction."
B: [] — "set up a reinforcement loop" = AI training strategy. No code artifact implied; "set up" in AI context → gap.
CompiledPredicateIds: []

---

## C0A-018-003 — IMPLEMENT fires: build reinforcement feedback collector

A: "Build a reinforcement feedback collector that stores user thumbs-up/down ratings for model retraining."
B: ["IMPLEMENT"] — "build" = code verb. "feedback collector" is the software artifact. Reinforcement context does not suppress IMPLEMENT.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-018-004 — IMPLEMENT+TEST fires: implement RLHF reward scorer + tests

A: "Implement the RLHF (Reinforcement Learning from Human Feedback) reward scorer service and write unit tests for scoring edge cases."
B: ["IMPLEMENT", "TEST"] — "implement" → IMPLEMENT; "write unit tests" → TEST. RLHF acronym is AI context; both code verbs fire.
CompiledPredicateIds: ["IMPLEMENT", "TEST"]

---

## C0A-018-005 — gap: "reinforcement" as observation

A: "Without reinforcement, the agent stagnates and fails to improve over time."
B: [] — observation about AI agent limitation. No action verb. "reinforcement" → gap.
CompiledPredicateIds: []
