# Held-Out and Corruption Pairs (021)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-021-A — Held-out: "adapt vs build" boundary cases

A (gap): "Adapt the agent for Spanish-language queries." → Predicates: [] → GapHandler → gap-ai-agent-adapt-001
A (IMPLEMENT): "Build the agent for Spanish-language queries." → Predicates: ["IMPLEMENT"]
Key: Same object. "adapt" → gap. "build" → IMPLEMENT. Verb determines routing.

---

## HC-021-B — Held-out: primary verb in compound sentence

A: "Adapt the agent rather than build a new one."
B: Predicates: [] | GapHandler → gap-ai-agent-adapt-001
Reason: "adapt" is primary. "build" is in negative subordinate clause. Primary verb determines routing.

A: "Build a new agent instead of adapting the existing one."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41.
Reason: "build" is primary. "adapting" is in "instead of" clause — does not fire.

---

## HC-021-C — Corruption v6-02: "adapt" mapped to IMPLEMENT

v6 corrupt: "Adapt the agent for e-commerce domain" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-ai-agent-adapt-001
Error: v6-02. "adapt" is AI vocabulary. No code artifact implied. Gap.

---

## HC-021-D — Held-out: "evolve the agent" with multiple gap terms

A: "Evolve the agent over time with reinforcement learning and self-healing capabilities."
B: Predicates: [] | GapHandler → gap-ai-agent-adapt-001
Reason: "evolve" = primary AI vocabulary. "reinforcement learning" and "self-healing" add AI vocabulary without code verbs. One gap ID; one event.
Phrasing-B: Gap — stacked AI vocabulary produces one gap event regardless of count.
