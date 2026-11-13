# Layer 1 — Transition Orchestrator Pairs (021)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-021-001 — v6 corruption: "adapt the agent" suppressed to IMPLEMENT

v6 corrupt output: "Adapt the agent for Spanish-language queries" → Predicates: ["IMPLEMENT"]
Correct output: "Adapt the agent for Spanish-language queries" → Predicates: [] → GapHandler
Error type: v6-02 (gap suppression)
Fix: "adapt" is AI vocabulary → gap. Gap ID: gap-ai-agent-adapt-001.

---

## TO-021-002 — IMPLEMENT correctly fires for "build the agent"

Request: "Build the agent that handles Spanish-language customer queries."
Predicates: ["IMPLEMENT"]
Orchestration: ImplementationFlow41
Teaching: "build the agent" = code verb + software artifact → IMPLEMENT. This is the correct contrast to "adapt the agent" → gap.

---

## TO-021-003 — Primary verb determines routing in "adapt vs build" sentences

Request: "Adapt the existing agent rather than build a new one."
Predicates: [] → GapHandler
Request: "Build a new agent instead of adapting the existing one."
Predicates: ["IMPLEMENT"]
Teaching: Primary action verb determines routing. Subordinate clause verbs do not fire predicates.

---

## TO-021-004 — "evolve the agent" gap orchestration

Request: "Evolve the agent over time by incorporating user feedback."
Predicates: []
Orchestration: GapHandler → SCHEDULE_CAPABILITY_LEARNING → gap-ai-agent-adapt-001
DomainId: software-development

---

## TO-021-005 — TransitionType summary for adapt/build boundary

| Request | Predicates | TransitionType | Flow |
|---------|-----------|----------------|------|
| adapt the agent | [] | N/A | GapHandler |
| build the agent | ["IMPLEMENT"] | feature_addition | ImplementationFlow41 |
| evolve the agent | [] | N/A | GapHandler |
| build and adapt hybrid | ["IMPLEMENT"] | feature_addition | ImplementationFlow41 |
