# Held-Out and Corruption Pairs (006)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-006-A — Held-out: "orchestrate" disambiguation

A: "Orchestrate the agent's multi-step reasoning pipeline."
B: Predicates: [] | GapHandler → gap-ai-agent-adapt-001
Reason: "orchestrate" = AI vocabulary → gap. "orchestrate" does NOT fire ARCHITECTURE.
Phrasing-B: Gap — "orchestrate" is AI agentic vocabulary, not a design verb. No predicate fires.

---

## HC-006-B — Held-out: "Design the orchestration layer"

A: "Design the agent orchestration layer architecture."
B: Predicates: ["ARCHITECTURE"] | ArchitectureFlow31 (sub-phase 3.1) | TransitionType: documentation
Reason: "design" = design verb; "orchestration layer" = structural noun; ARCHITECTURE fires.
Phrasing-B: ARCHITECTURE — "design the [noun] layer" fires ARCHITECTURE. "orchestrate" alone is gap.

---

## HC-006-C — Held-out: "RAG pipeline" with code verb

A: "Implement the RAG retrieval module."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "implement" = code verb; "RAG retrieval module" = software component. Code verb overrides.
Phrasing-B: IMPLEMENT — "implement the [AI noun] module" is a code-action task. RAG object does not fire any other predicate.

---

## HC-006-D — Corruption v6-02: "orchestrate" incorrectly maps to ARCHITECTURE

v6 corrupt: "Orchestrate the agent workflows" → Predicates: ["ARCHITECTURE"]
Correct: Predicates: [] → GapHandler
Error type: v6-02 — "orchestrate" incorrectly treated as ARCHITECTURE trigger. It is AI vocabulary → gap.
