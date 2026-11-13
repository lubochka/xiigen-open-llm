# Layer 1 — Transition Orchestrator Pairs (030)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-030-001 — Orchestration: LLM gap

Gap request: "The LLM produces inconsistent outputs on edge cases."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-model-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-030-002 — Orchestration: build LLM wrapper + integrate

Predicates: ["IMPLEMENT", "API_CONTRACT"]
Request: "Build a wrapper around the LLM and integrate it with the document processing API."
Flow order: APIContractFlow33 → ImplementationFlow41
TransitionType: feature_addition

---

## TO-030-003 — v6 corruption: LLM → LLM_CALL invented predicate (v6-01)

v6 corrupt: "LLM" → "LLM_CALL" predicate
Correct: [] → GapHandler → gap-ai-ml-model-001
Error: v6-01. "LLM" is AI vocabulary. No LLM_CALL predicate exists.

---

## TO-030-004 — Orchestration: GPT gap

Gap request: "GPT requires careful prompt design for reliable outputs."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-model-001
Orchestration: No flow. One gap event. DomainId: software-development.

---

## TO-030-005 — Orchestration: secure LLM endpoint + implement client

Predicates: ["IMPLEMENT", "SECURITY"]
Request: "Implement the LLM client library and secure the inference endpoint against prompt injection."
Flow order: ImplementationFlow41 → SecurityAuditFlow
TransitionType: feature_addition
Rule: IMPLEMENT before SECURITY (canonical rule 4).
