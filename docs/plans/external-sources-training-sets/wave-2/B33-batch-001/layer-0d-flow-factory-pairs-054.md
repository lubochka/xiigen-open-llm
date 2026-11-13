# Layer 0-D — Flow Factory Pairs (054)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-054-001 — IMPLEMENT: refactor embedding client for performance

A: ["IMPLEMENT"] | "Refactor the embedding model client to use connection pooling and reduce round-trip latency."
Factory: [ImplementationFlow41] | TransitionType: refactoring
Teaching: "refactor" fires IMPLEMENT. TransitionType = refactoring.

---

## FF-054-002 — SECURITY: secure knowledge graph endpoint

A: ["SECURITY"] | "Secure the knowledge graph API endpoint against unauthorized entity writes."
Factory: [SecurityAuditFlow] | TransitionType: N/A (security audit)
Teaching: "secure" fires SECURITY. "knowledge graph API endpoint" = object.

---

## FF-054-003 — IMPLEMENT+SECURITY: build + secure LLM gateway

A: ["IMPLEMENT", "SECURITY"] | "Build the LLM API gateway and secure it against prompt injection and rate abuse."
Factory: [ImplementationFlow41, SecurityAuditFlow] | TransitionType: feature_addition
Ordering: ImplementationFlow41 → SecurityAuditFlow (IMPLEMENT before SECURITY).

---

## FF-054-004 — ARCHITECTURE+API_CONTRACT+IMPLEMENT+TEST: four-predicate AI system

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "TEST"] | "Design the AI agent architecture, integrate the LLM API, implement the tool dispatcher, write tests for agent routing."
Factory: [ArchitectureFlow31, APIContractFlow33, ImplementationFlow41, TestFlow51] | TransitionType: feature_addition
Ordering: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → TestFlow51.

---

## FF-054-005 — GapHandler: AI agent + LLM (no code verb)

A: [] | "AI agents leverage LLMs for reasoning and knowledge graphs for factual grounding."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: Descriptive. No code verb. Three gap terms. One event.
