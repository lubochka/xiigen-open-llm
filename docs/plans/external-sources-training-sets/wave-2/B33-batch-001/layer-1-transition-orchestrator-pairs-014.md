# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-014-001 — v6 corruption: "FEW_SHOT" invented predicate

v6 corrupt output: "Use few-shot learning for the agent" → Predicates: ["FEW_SHOT"]
Correct output: "Use few-shot learning for the agent" → Predicates: [] → GapHandler
Error type: v6-01 (predicate invention)
Fix: "few-shot learning" is AI vocabulary → gap. Gap ID: gap-ai-agent-llm-001.

---

## TO-014-002 — v6 corruption: "add few-shot examples to prompt" suppressed to IMPLEMENT

v6 corrupt output: "Add few-shot examples to the system prompt" → Predicates: ["IMPLEMENT"]
Correct output: "Add few-shot examples to the system prompt" → Predicates: [] → GapHandler
Error type: v6-02 (gap suppression)
Fix: "add to system prompt" = AI prompt configuration. Not a code artifact. Gap.

---

## TO-014-003 — TransitionType for few-shot retrieval service

Request: "Build the few-shot example retrieval service."
Predicates: ["IMPLEMENT"]
TransitionType: feature_addition
Orchestration: ImplementationFlow41

---

## TO-014-004 — Ordering rule: IMPLEMENT before TEST for few-shot

Request: "Implement the few-shot retrieval service and write unit tests."
Predicates: ["IMPLEMENT", "TEST"]
Ordering: ImplementationFlow41 → E2ETestingFlow53
Teaching: Standard ordering (IMPLEMENT before TEST) applies in all contexts including AI.

---

## TO-014-005 — DomainId for few-shot gap

All "few-shot learning" gap requests: DomainId: software-development
Gap ID: gap-ai-agent-llm-001
SCHEDULE_CAPABILITY_LEARNING emitted once per gap request.
