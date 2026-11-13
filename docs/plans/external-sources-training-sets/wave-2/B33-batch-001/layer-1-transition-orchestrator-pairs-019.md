# Layer 1 — Transition Orchestrator Pairs (019)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-019-001 — Agent API integration: IMPLEMENT+API_CONTRACT ordering

Request: "Build the agent service and integrate it with the external CRM API."
Predicates: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Orchestration: APIContractFlow33 → ImplementationFlow41
Teaching: Standard ordering — API_CONTRACT before IMPLEMENT. Contract spec informs implementation.

---

## TO-019-002 — v6 corruption: API_CONTRACT collapsed to IMPLEMENT in agent context

v6 corrupt output: "Build the agent and integrate with the CRM" → Predicates: ["IMPLEMENT"]
Correct output: Predicates: ["IMPLEMENT", "API_CONTRACT"]
Error type: v6-03 (multi-predicate collapse)
Fix: "build" → IMPLEMENT; "integrate" → API_CONTRACT. Both fire independently. No collapsing permitted.

---

## TO-019-003 — Three-predicate orchestration: IMPLEMENT+API_CONTRACT+SECURITY

Request: "Implement the agent integration layer, integrate with the payment gateway, secure against prompt injection."
Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
Orchestration: APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow
TransitionType: feature_addition

---

## TO-019-004 — GapHandler: "expose agent as API" is not API_CONTRACT

Request: "We should expose the agent as an API."
Predicates: []
Correct output: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Teaching: "expose as API" is strategic planning language → gap. Only "integrate" fires API_CONTRACT; strategic planning does not.

---

## TO-019-005 — DomainId: agent API gap uses software-development

All "expose agent as API" / "should build agent API" gap requests: DomainId: software-development
Gap ID: gap-ai-agent-skill-001
SCHEDULE_CAPABILITY_LEARNING emitted once.
