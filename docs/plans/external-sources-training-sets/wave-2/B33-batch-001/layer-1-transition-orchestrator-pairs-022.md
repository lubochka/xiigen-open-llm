# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-022-001 — v6 corruption: IMPLEMENT+TEST collapsed to IMPLEMENT in AI context

v6 corrupt output: "Implement agent skill router and write integration tests" → Predicates: ["IMPLEMENT"]
Correct output: Predicates: ["IMPLEMENT", "TEST"]
Error type: v6-03 (multi-predicate collapse)
Fix: "implement" → IMPLEMENT; "write integration tests" → TEST. Both fire independently.

---

## TO-022-002 — v6 corruption: ARCHITECTURE+IMPLEMENT+API_CONTRACT collapsed to ARCHITECTURE

v6 corrupt output: "Design multi-agent architecture, build orchestration service, integrate with registry API" → Predicates: ["ARCHITECTURE"]
Correct output: Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"]
Error type: v6-03 (multi-predicate collapse)
Fix: Three code verbs, three predicates. No collapsing permitted.

---

## TO-022-003 — SECURITY overrides gap when prompt injection present

Request: "Improve agent chain-of-thought and prevent prompt injection."
Predicates: ["SECURITY"]
Teaching: "improve chain-of-thought" = gap (silently skipped). "prevent prompt injection" → SECURITY fires. Only SECURITY in result.

---

## TO-022-004 — IMPLEMENT+SECURITY orchestration order

Request: "Build the agent response filter and secure against prompt injection."
Predicates: ["IMPLEMENT", "SECURITY"]
Orchestration: ImplementationFlow41 → SecurityAuditFlow
Teaching: Security audits the built artifact → SECURITY runs after IMPLEMENT.

---

## TO-022-005 — QA is always last even in AI agent context

Request: "Implement the skill router, write tests for routing, and QA sign-off."
Predicates: ["IMPLEMENT", "TEST", "QA"]
Orchestration: ImplementationFlow41 → E2ETestingFlow53 → QAGatingFlow73
Teaching: QA is last — absolute ordering constraint regardless of AI context or predicate count.
