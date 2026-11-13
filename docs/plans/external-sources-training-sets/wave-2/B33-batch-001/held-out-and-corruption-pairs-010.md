# Held-Out and Corruption Pairs (010)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-010-A — Held-out: "agentic workflow" gap

A: "Set up an agentic workflow for the customer support pipeline."
B: Predicates: [] | GapHandler → gap-ai-agent-adapt-001
Reason: "set up agentic workflow" = AI agentic configuration vocabulary → gap. "set up" ≠ code-action verb.
Phrasing-B: Gap — "agentic workflow" is AI vocabulary; "set up" is configuration vocabulary, not code-action.

---

## HC-010-B — Held-out: ARCHITECTURE + IMPLEMENT + QA full cycle

A: "Architect the agent platform, build the skill engine, and QA the integration."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT", "QA"]
Orchestration: ArchitectureFlow31 → ImplementationFlow41 → QAGatingFlow73
TransitionType: feature_addition | QA sub-phase: 7.3 (feature integration gate)
Phrasing-B: Three-predicate sequence — ARCHITECTURE first, then IMPLEMENT, QA always last.

---

## HC-010-C — Corruption v6-03: Refactor + IMPLEMENT collapsed to refactoring only

v6 corrupt: "Refactor agent skill router and build the new dispatch table" → TransitionType: refactoring (only one flow)
Correct: Predicates: ["IMPLEMENT"] | Orchestration: ImplementationFlow41 | TransitionType: refactoring
Note: TransitionType = refactoring when refactor verb present — but IMPLEMENT predicate still fires; only TransitionType changes.
Fix: Predicate set and orchestration are determined by predicates (IMPLEMENT fires). TransitionType is a separate determination where "refactor" wins.

---

## HC-010-D — Held-out: "agent persona" gap

A: "Configure the agent's persona to match our brand voice."
B: Predicates: [] | GapHandler → gap-ai-agent-adapt-001
Reason: "configure persona" = AI behavioral configuration → gap.
Phrasing-B: Gap — persona configuration is AI identity vocabulary, not code implementation.
