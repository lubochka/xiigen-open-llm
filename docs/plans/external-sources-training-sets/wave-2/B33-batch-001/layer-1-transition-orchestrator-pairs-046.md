# Layer 1 — Transition Orchestrator Pairs (046)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-046-001 — Orchestration: AI agent + LLM + vector database all-gap

Gap request: "AI agents use LLMs to query a vector database for retrieval-augmented generation."
Predicates: [] | GapHandler → gap-ai-ml-model-001 | One event.

---

## TO-046-002 — Orchestration: full platform build + all flows

Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
Request: "Design the AI agent platform architecture, integrate the LLM and vector database APIs, implement the dispatcher, secure the endpoints, write integration tests, document the system, QA."
Flow order: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → TestFlow51 → DocumentationFlow62 → QAGatingFlow73
TransitionType: feature_addition

---

## TO-046-003 — Orchestration: single "integrate" suppresses all gap vocabulary

Predicates: ["API_CONTRACT"]
Request: "LLM, GPT, BERT, Llama, embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent — integrate results with reporting API."
Flow: APIContractFlow33 | All gap vocabulary silently suppressed.

---

## TO-046-004 — Orchestration: DomainId is always software-development

Rule verified for all B33 rounds: DomainId: software-development.
gap-ai-ml-model-001: software-development
gap-ai-ml-training-001: software-development
gap-ai-agent-adapt-001: software-development
gap-ai-agent-skill-001: software-development
gap-ai-agent-llm-001: software-development

---

## TO-046-005 — Orchestration: TransitionType summary for all B33 scenarios

| Request type | TransitionType |
|-------------|---------------|
| build/implement new feature | feature_addition |
| refactor existing code | refactoring |
| documentation only | documentation |
| gap request (GapHandler) | N/A |
| security/QA/TEST only | N/A (sub-phase) |
