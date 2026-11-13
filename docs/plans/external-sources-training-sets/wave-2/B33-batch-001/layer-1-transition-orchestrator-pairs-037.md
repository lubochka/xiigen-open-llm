# Layer 1 — Transition Orchestrator Pairs (037)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-037-001 — Orchestration: AI agent + LLM combined gap

Gap request: "AI agents leverage LLMs for reasoning and knowledge graphs for factual grounding."
Predicates: [] | GapHandler → gap-ai-ml-model-001 | One SCHEDULE_CAPABILITY_LEARNING.

---

## TO-037-002 — Orchestration: design AI agent platform + implement tool dispatcher

Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Flow order: ArchitectureFlow31 → ImplementationFlow41 | TransitionType: feature_addition

---

## TO-037-003 — Orchestration: implement AI agent + integrate LLM API + secure + write tests

Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"]
Request: "Implement the AI agent runtime, integrate the LLM API, secure against injection, write tests."
Flow order: APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → TestFlow51
TransitionType: feature_addition

---

## TO-037-004 — v6 corruption: KNOWLEDGE_GRAPH invented predicate (v6-01)

v6 corrupt: "knowledge graph" → "KNOWLEDGE_GRAPH" predicate
Correct: [] → GapHandler → gap-ai-ml-model-001
Error: v6-01. "knowledge graph" is AI vocabulary. No KNOWLEDGE_GRAPH predicate exists.

---

## TO-037-005 — Orchestration: document + QA AI platform

Predicates: ["DOCUMENT", "QA"]
Flow order: DocumentationFlow62 → QAGatingFlow73 | TransitionType: documentation
Rule: QA always last regardless of position in request.
