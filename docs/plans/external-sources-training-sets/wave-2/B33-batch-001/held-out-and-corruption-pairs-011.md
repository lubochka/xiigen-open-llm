# Held-Out and Corruption Pairs (011)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-011-A — Held-out: "self-directed" gap

A: "Make the agent self-directed so it can plan its own tasks."
B: Predicates: [] | GapHandler → gap-ai-agent-adapt-001
Reason: "make self-directed" = AI autonomous behavior vocabulary → gap.
Phrasing-B: Gap — "self-directed" is AI behavioral vocabulary; "make" in this context ≠ code-action verb.

---

## HC-011-B — Held-out: "Build task planner module"

A: "Build a task planner module for the agent's self-direction feature."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "build" = code verb; "task planner module" = software component.
Phrasing-B: IMPLEMENT — "build a [noun] module" fires IMPLEMENT. "self-direction" in description does not affect predicate.

---

## HC-011-C — Held-out: "agent grounding" with implicit code verb ambiguity

A: "Ground the agent's responses using our verified knowledge base."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001
Reason: "ground" (AI context) = LLM quality vocabulary → gap. Not a code-action verb.
Phrasing-B: Gap — "ground responses" is AI alignment vocabulary. "Build a grounding verification module" → IMPLEMENT.

---

## HC-011-D — Corruption v6-01: LLM predicate invented

v6 corrupt: "Improve the agent's LLM reasoning" → Predicates: ["LLM"] | LLMOptimizationFlow
Correct: Predicates: [] → GapHandler → gap-ai-agent-llm-001
Error type: v6-01 — LLM is not a predicate. No LLM predicate or flow exists in the nine-predicate lexicon.
Fix: All LLM/reasoning vocabulary → gap. No predicate invented.
