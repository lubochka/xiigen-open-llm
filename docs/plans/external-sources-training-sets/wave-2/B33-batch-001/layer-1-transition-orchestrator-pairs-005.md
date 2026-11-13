# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-005-001 — DOCUMENT single flow in AI context

Request: "Document the agent's tool-calling protocol."
Predicates: ["DOCUMENT"]
Orchestration: DocumentationFlow (single flow)
TransitionType: documentation
Note: "document" fires DOCUMENT; "tool-calling protocol" is AI vocabulary object — does not suppress DOCUMENT.

---

## TO-005-002 — Gap orchestration with GapHandler details

Request: "Improve the agent's chain-of-thought reasoning quality."
Predicates: [] (gap)
Orchestration: GapHandler invoked.
  → Emits: SCHEDULE_CAPABILITY_LEARNING
  → Gap ID logged: gap-ai-agent-llm-001
  → Returns: "Capability not available. Request logged for learning pipeline."
  → DomainId: software-development
TransitionType: none (no predicate, no TransitionType contribution)

---

## TO-005-003 — IMPLEMENT+DOCUMENT parallel-eligible orchestration

Request: "Build the agent response formatter and document its output schema."
Predicates: ["IMPLEMENT", "DOCUMENT"]
Orchestration: ImplementationFlow41 ‖ DocumentationFlow (parallel permitted; neither depends on the other)
TransitionType: feature_addition (IMPLEMENT present)
Note: IMPLEMENT and DOCUMENT may run concurrently when no data dependency exists.

---

## TO-005-004 — ARCHITECTURE single flow (AI system design only)

Request: "Design the architecture for the autonomous reasoning subsystem."
Predicates: ["ARCHITECTURE"]
Orchestration: ArchitectureFlow31 (sub-phase 3.1 — application-level AI system design)
TransitionType: documentation (IMPLEMENT absent)
Note: "autonomous reasoning subsystem" is AI vocabulary but "design" + structural intent → ARCHITECTURE.

---

## TO-005-005 — TEST single flow routing decision

Request: "Write E2E tests for the agent's multi-turn conversation flow."
Predicates: ["TEST"]
Orchestration: E2ETestingFlow53 (E2E/conversation/interaction vocabulary → 5.3, not 5.1)
TransitionType: documentation (no IMPLEMENT)
Note: "multi-turn conversation flow" is AI context but "write E2E tests" is the action — TEST fires, routes to 5.3.
