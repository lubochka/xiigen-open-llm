# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-018-001 — v6 corruption: "REINFORCE" invented predicate

v6 corrupt output: "Use reinforcement learning for the agent" → Predicates: ["REINFORCE"]
Correct output: "Use reinforcement learning for the agent" → Predicates: [] → GapHandler
Error type: v6-01 (predicate invention)
Fix: "reinforcement learning" is AI training vocabulary → gap. Gap ID: gap-ai-agent-llm-001.

---

## TO-018-002 — v6 corruption: RLHF loop suppressed to IMPLEMENT

v6 corrupt output: "Set up a reinforcement loop for the agent" → Predicates: ["IMPLEMENT"]
Correct output: "Set up a reinforcement loop for the agent" → Predicates: [] → GapHandler
Error type: v6-02 (gap suppression)
Fix: "set up a reinforcement loop" = AI training strategy. No code artifact.

---

## TO-018-003 — IMPLEMENT+TEST orchestration for RLHF infrastructure

Request: "Implement the RLHF reward scorer and write unit tests for scoring."
Predicates: ["IMPLEMENT", "TEST"]
TransitionType: feature_addition
Orchestration: ImplementationFlow41 → E2ETestingFlow53

---

## TO-018-004 — API_CONTRACT fires in reinforcement context

Request: "Integrate the reinforcement feedback pipeline with the training API."
Predicates: ["API_CONTRACT"]
Orchestration: APIContractFlow33
Teaching: "integrate" always fires API_CONTRACT. RLHF (Reinforcement Learning from Human Feedback) context does not suppress.

---

## TO-018-005 — DomainId for reinforcement gap

All "reinforcement learning" / "RLHF" gap requests: DomainId: software-development
Gap ID: gap-ai-agent-llm-001
SCHEDULE_CAPABILITY_LEARNING emitted once.
