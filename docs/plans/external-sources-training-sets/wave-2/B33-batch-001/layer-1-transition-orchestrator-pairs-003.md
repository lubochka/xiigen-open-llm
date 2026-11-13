# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-003-001 — TransitionType determination in AI context

IMPLEMENT (AI feature) → feature_addition (same as any software feature)
ARCHITECTURE (AI system design only) → documentation
ARCHITECTURE+IMPLEMENT → feature_addition
Refactor verb → refactoring
QA → no TransitionType contribution

---

## TO-003-002 — GapHandler orchestration details

On gap (zero predicates):
1. GapHandler captures request
2. Emits SCHEDULE_CAPABILITY_LEARNING event
3. Logs to gap registry: gap-ai-agent-adapt-001 / gap-ai-agent-skill-001 / gap-ai-agent-llm-001
4. Returns: "Capability not available. Request logged for learning pipeline."
5. DomainId: software-development

---

## TO-003-003 — Mixed gap and predicate requests

A: "Adapt the agent to handle finance queries and implement the finance intent classifier."
Analysis: "adapt" → gap; "implement" → IMPLEMENT. Since IMPLEMENT fires, predicates = ["IMPLEMENT"].
Orchestration: ImplementationFlow41. GapHandler NOT invoked (at least one predicate fired).

---

## TO-003-004 — SECURITY in AI context: prompt injection handling

A: ["SECURITY"] | "Add prompt injection prevention to the agent input layer"
Orchestration: SecurityAuditFlow. TransitionType: documentation (IMPLEMENT absent if "add" is in security context without implementation intent). 
Note: If "add" here means software implementation → IMPLEMENT+SECURITY. Context-dependent.

---

## TO-003-005 — API_CONTRACT orchestration in AI context

A: ["API_CONTRACT"] | "Integrate the AI agent with the Salesforce CRM API"
Orchestration: APIContractFlow33. TransitionType: documentation (no IMPLEMENT).
Note: "integrate" fires API_CONTRACT always. Even in AI integration context.
