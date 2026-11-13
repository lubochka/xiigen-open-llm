# Layer 1 — Transition Orchestrator Pairs (034)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-034-001 — Orchestration: AI agent gap

Gap request: "The AI agent autonomously selects tools to complete multi-step tasks."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-model-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-034-002 — Orchestration: build AI agent runtime + secure

Predicates: ["IMPLEMENT", "SECURITY"]
Request: "Build the AI agent runtime that dispatches tool calls and secure the communication endpoints against injection."
Flow order: ImplementationFlow41 → SecurityAuditFlow
TransitionType: feature_addition
Rule: IMPLEMENT before SECURITY (canonical rule 4).

---

## TO-034-003 — Orchestration: multi-agent gap

Gap request: "Multi-agent coordination requires a shared message bus and conflict resolution."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-model-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-034-004 — Orchestration: design multi-agent architecture

Predicates: ["ARCHITECTURE"]
Request: "Design the multi-agent coordination architecture with shared state management."
Flow: ArchitectureFlow31 | TransitionType: feature_addition
Rule: "design" + "architecture" fires ARCHITECTURE.

---

## TO-034-005 — v6 corruption: AI_AGENT invented predicate (v6-01)

v6 corrupt: "AI agent" → "AI_AGENT" predicate
Correct: [] → GapHandler → gap-ai-ml-model-001
Error: v6-01. "AI agent" is AI vocabulary. No AI_AGENT predicate exists.
