# Held-Out and Corruption Pairs (018)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-018-A — Held-out: "reinforcement" boundary cases

A (gap): "Use RLHF (Reinforcement Learning from Human Feedback) to improve agent quality." → Predicates: [] → GapHandler → gap-ai-agent-llm-001
A (IMPLEMENT): "Build the reinforcement feedback collector service." → Predicates: ["IMPLEMENT"]
A (API_CONTRACT): "Integrate the reinforcement pipeline with the feedback API." → Predicates: ["API_CONTRACT"]
Key distinction: AI training vocabulary → gap. Code verb + software artifact → IMPLEMENT. "integrate" → API_CONTRACT.

---

## HC-018-B — Held-out: "reward function" disambiguation

A (gap): "Design the reward function for agent behavior optimization." → Predicates: [] → GapHandler → gap-ai-agent-llm-001
Reason: "reward function" = AI training design artifact. "design" here modifies an AI artifact (reward function), not a structural software noun.
A (ARCHITECTURE): "Design the reward scoring architecture." → Predicates: ["ARCHITECTURE"]
Reason: "design" + "architecture" (structural noun) → ARCHITECTURE fires.
Distinction: "reward function" is an AI artifact → gap. "reward scoring architecture" is structural → ARCHITECTURE.

---

## HC-018-C — Corruption v6-01: "RLHF" invented predicate

v6 corrupt: "Set up RLHF (Reinforcement Learning from Human Feedback) for the agent" → Predicates: ["RLHF"]
Correct: Predicates: [] → GapHandler → gap-ai-agent-llm-001
Error: v6-01. RLHF is not a predicate. It is AI vocabulary → gap.

---

## HC-018-D — Held-out: "reinforcement" + DOCUMENT

A: "Document the reinforcement learning methodology for the agent team."
B: Predicates: ["DOCUMENT"] | GapHandler suppressed; gap-ai-agent-llm-001 silently skipped.
Reason: "document" fires DOCUMENT. "reinforcement learning" is documentation subject.
Phrasing-B: DOCUMENT fires regardless of AI vocabulary in object.
