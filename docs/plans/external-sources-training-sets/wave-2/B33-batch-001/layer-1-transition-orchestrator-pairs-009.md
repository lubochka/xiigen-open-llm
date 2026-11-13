# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-009-001 — Observation-only request (zero predicates)

Request: "The agent is not reasoning correctly about ambiguous inputs."
Predicates: [] (gap — observation statement; no action verb firing any predicate)
Orchestration: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Gap ID: gap-ai-agent-llm-001 (reasoning/LLM quality)
Note: Observation statements without code-action verbs always yield zero predicates.

---

## TO-009-002 — IMPLEMENT with AI framework object

Request: "Build a LangChain-style skill chaining module."
Analysis: "build" → IMPLEMENT; "LangChain-style" is AI framework reference in adjective position — does not fire ARCHITECTURE
Predicates: ["IMPLEMENT"]
Orchestration: ImplementationFlow41
TransitionType: feature_addition
Note: Framework names as adjectives do not trigger gaps or additional predicates — only the verb matters.

---

## TO-009-003 — SECURITY single flow in AI context

Request: "Validate and sanitize all inputs entering the agent context window."
Predicates: ["SECURITY"]
Orchestration: SecurityAuditFlow (single flow)
TransitionType: documentation (no IMPLEMENT; validate/sanitize in security context without build verb)
Note: If "implement validation layer" were present → IMPLEMENT → feature_addition.

---

## TO-009-004 — AUTHORIZATION in AI context

Request: "Add role-based access control to the agent admin endpoints."
Predicates: ["AUTHORIZATION"] (add+access control → AUTHORIZATION; admin endpoints = protected resource)
Orchestration: AuthorizationFlow
TransitionType: documentation (IMPLEMENT absent; "add" in authorization context — context-dependent)
Note: If "add" clearly means software implementation → IMPLEMENT+AUTHORIZATION → feature_addition.

---

## TO-009-005 — Gap with explicit learning intent

Request: "Teach the agent to handle legal domain queries."
Analysis: "teach" is AI training vocabulary → gap; "handle" without code verb → gap
Predicates: [] (gap)
Orchestration: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Gap ID: gap-ai-agent-skill-001 (domain skill gap)
DomainId: software-development
Note: "teach the agent" is capability-acquisition vocabulary — always gap regardless of domain object.
