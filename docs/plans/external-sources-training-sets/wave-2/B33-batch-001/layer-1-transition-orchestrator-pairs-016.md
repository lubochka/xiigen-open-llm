# Layer 1 — Transition Orchestrator Pairs (016)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-016-001 — v6 corruption: "PROMPT_ENGINEERING" invented predicate

v6 corrupt output: "Invest in prompt engineering for the agent" → Predicates: ["PROMPT_ENGINEERING"]
Correct output: "Invest in prompt engineering for the agent" → Predicates: [] → GapHandler
Error type: v6-01 (predicate invention)
Fix: "prompt engineering" is AI vocabulary → gap. Gap ID: gap-ai-agent-llm-001.

---

## TO-016-002 — SECURITY fires for prompt injection despite prompt engineering context

v6 possible error: "Prevent prompt injection while engineering prompts" → Predicates: ["PROMPT_ENGINEERING"]
Correct output: Predicates: ["SECURITY"]
Error type: v6-01 (if PROMPT_ENGINEERING invented); or correct if SECURITY fires
Fix: "prompt injection" = security vocabulary → SECURITY fires. "prompt engineering" is gap (silently skipped). Result: ["SECURITY"].

---

## TO-016-003 — TransitionType for prompt template library

Request: "Create the prompt template library and write regression tests."
Predicates: ["IMPLEMENT", "TEST"]
TransitionType: feature_addition
Orchestration: ImplementationFlow41 → E2ETestingFlow53

---

## TO-016-004 — Ordering: IMPLEMENT before DOCUMENT for prompt system

Request: "Build the prompt management system and document the template schema."
Predicates: ["IMPLEMENT", "DOCUMENT"]
Ordering: ImplementationFlow41 → DocumentationFlow62
Teaching: Standard IMPLEMENT-before-DOCUMENT applies.

---

## TO-016-005 — DomainId for prompt engineering gap

All "prompt engineering" gap requests: DomainId: software-development
Gap ID: gap-ai-agent-llm-001
SCHEDULE_CAPABILITY_LEARNING emitted once.
