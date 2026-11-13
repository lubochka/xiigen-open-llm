# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-020-001 — v6 corruption: "orchestrate" incorrectly fires ARCHITECTURE

v6 corrupt output: "Orchestrate the agent workflows" → Predicates: ["ARCHITECTURE"]
Correct output: "Orchestrate the agent workflows" → Predicates: [] → GapHandler
Error type: v6-02 (gap suppression — "orchestrate" incorrectly mapped to ARCHITECTURE)
Fix: "orchestrate" = AI operational vocabulary → gap. ARCHITECTURE requires design verbs (architect, design) + structural noun.

---

## TO-020-002 — ARCHITECTURE correctly fires for "architect the orchestration layer"

Request: "Architect the orchestration layer that coordinates tasks."
Predicates: ["ARCHITECTURE"]
Orchestration: ArchitectureFlow31 (3.1)
Teaching: "architect" (design verb) + "layer" (structural noun) → ARCHITECTURE. This is the correct result; not a corruption.

---

## TO-020-003 — API_CONTRACT fires for "via API" orchestration pattern

Request: "Orchestrate agent calls via the orchestration API."
Predicates: ["API_CONTRACT"]
Orchestration: APIContractFlow33
Teaching: "via API" pattern → API_CONTRACT fires even with AI orchestration vocabulary present.

---

## TO-020-004 — TransitionType: feature_addition for orchestration service build

Request: "Build the agent orchestration service that routes tasks."
Predicates: ["IMPLEMENT"]
TransitionType: feature_addition
Orchestration: ImplementationFlow41

---

## TO-020-005 — DomainId for orchestration gap

All "orchestrate agent workflows" gap requests: DomainId: software-development
Gap ID: gap-ai-agent-adapt-001
SCHEDULE_CAPABILITY_LEARNING emitted once.
