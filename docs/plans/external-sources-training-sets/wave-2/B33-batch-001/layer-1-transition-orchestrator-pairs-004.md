# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-004-001 — IMPLEMENT+TEST sequential orchestration in AI context

Request: "Build the agent skill router and write unit tests for it."
Predicates: ["IMPLEMENT", "TEST"]
Orchestration: ImplementationFlow41 → TestingFlow51 (sequential; TEST waits on IMPLEMENT output)
TransitionType: feature_addition (IMPLEMENT present)
Note: AI object ("agent skill router") does not change orchestration — code verbs determine flow.

---

## TO-004-002 — ARCHITECTURE+IMPLEMENT sequential in AI context

Request: "Design the multi-agent coordination layer and build the message bus."
Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Orchestration: ArchitectureFlow31 → ImplementationFlow41 (sequential)
TransitionType: feature_addition (IMPLEMENT present overrides ARCHITECTURE→documentation)
Note: "design" fires ARCHITECTURE; "build" fires IMPLEMENT. IMPLEMENT TransitionType wins.

---

## TO-004-003 — IMPLEMENT+SECURITY sequential in AI context

Request: "Add input sanitization to the agent gateway and add prompt injection guards."
Predicates: ["IMPLEMENT", "SECURITY"]
Orchestration: ImplementationFlow41 → SecurityAuditFlow (sequential; SECURITY post-validates)
TransitionType: feature_addition (IMPLEMENT present)
Note: "prompt injection guards" → SECURITY; "input sanitization" + "add" → IMPLEMENT.

---

## TO-004-004 — API_CONTRACT single flow in AI context

Request: "Integrate the agent with the OpenAI embeddings API."
Predicates: ["API_CONTRACT"]
Orchestration: APIContractFlow33 (single flow)
TransitionType: documentation (no IMPLEMENT)
Note: "integrate" always fires API_CONTRACT. "embeddings API" is external API object — API_CONTRACT regardless of AI context.

---

## TO-004-005 — Five-predicate orchestration with AI object

Request: "Design the agent architecture, build the skill dispatcher, secure the prompt layer, document the API contract, and QA the release."
Predicates: ["ARCHITECTURE", "IMPLEMENT", "SECURITY", "DOCUMENT", "QA"]
Orchestration: ArchitectureFlow31 → ImplementationFlow41 → SecurityAuditFlow → DocumentationFlow → QAGatingFlow73
TransitionType: feature_addition (IMPLEMENT present)
QA last: absolute — QA validates all preceding flows before verdict.
