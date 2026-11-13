# Held-Out and Corruption Pairs (008)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-008-A — Held-out: "LLM hallucination" gap

A: "Reduce LLM hallucination in the agent's responses."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001
Reason: "reduce hallucination" = AI quality improvement vocabulary → gap.
Phrasing-B: Gap — hallucination reduction is AI behavioral vocabulary. "Build a hallucination detection module" → IMPLEMENT.

---

## HC-008-B — Held-out: "vector embeddings" gap

A: "Use vector embeddings to improve the agent's memory retrieval."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001
Reason: "use vector embeddings" = AI/ML technique vocabulary → gap. "use" is not a code-action verb.
Phrasing-B: Gap — "use [AI technique]" is capability usage vocabulary, not software implementation.

---

## HC-008-C — Held-out: "Build embedding store"

A: "Build a vector embedding store for the agent knowledge base."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "build" = code verb; "vector embedding store" = software component (store). Code verb overrides.
Phrasing-B: IMPLEMENT — "build a [noun] store" fires IMPLEMENT even with AI vocabulary object.

---

## HC-008-D — Corruption v6-02: "use vector embeddings" incorrectly fires IMPLEMENT

v6 corrupt: "Use vector embeddings for retrieval" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler
Error type: v6-02 — "use" is not a code-action verb. Gap suppression error.
Fix: "use [technique]" is gap. "Build/implement [artifact using technique]" fires IMPLEMENT.
