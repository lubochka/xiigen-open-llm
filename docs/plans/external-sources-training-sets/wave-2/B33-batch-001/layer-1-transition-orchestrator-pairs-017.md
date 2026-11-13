# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-017-001 — v6 corruption: "SELF_HEAL" invented predicate

v6 corrupt output: "The agent should be self-healing" → Predicates: ["SELF_HEAL"]
Correct output: "The agent should be self-healing" → Predicates: [] → GapHandler
Error type: v6-01 (predicate invention)
Fix: "self-healing" is AI behavior vocabulary → gap. Gap ID: gap-ai-agent-adapt-001.

---

## TO-017-002 — v6 corruption: self-healing suppressed to IMPLEMENT

v6 corrupt output: "Enable self-healing in the agent" → Predicates: ["IMPLEMENT"]
Correct output: "Enable self-healing in the agent" → Predicates: [] → GapHandler
Error type: v6-02 (gap suppression)
Fix: "enable" in AI behavior context does not fire IMPLEMENT. "self-healing" is gap vocabulary.

---

## TO-017-003 — ARCHITECTURE+IMPLEMENT orchestration for self-healing system

Request: "Design the self-healing architecture and implement the fallback routing service."
Predicates: ["ARCHITECTURE", "IMPLEMENT"]
TransitionType: feature_addition
Orchestration: ArchitectureFlow31 → ImplementationFlow41

---

## TO-017-004 — SECURITY overrides self-healing gap in mixed request

Request: "Enable self-healing in the agent and prevent prompt injection at the input layer."
Predicates: ["SECURITY"]
Orchestration: SecurityAuditFlow
Teaching: "enable self-healing" = gap (silently skipped). "prevent prompt injection" → SECURITY fires. Result: ["SECURITY"] only.

---

## TO-017-005 — DomainId for self-healing gap

All "self-healing" gap requests: DomainId: software-development
Gap ID: gap-ai-agent-adapt-001
SCHEDULE_CAPABILITY_LEARNING emitted once.
