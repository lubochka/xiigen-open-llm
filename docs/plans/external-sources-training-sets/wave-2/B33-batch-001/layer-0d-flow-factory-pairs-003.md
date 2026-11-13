# Layer 0-D — Flow Factory Pairs (003)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-003-001 — IMPLEMENT+TEST → two flows

A: ["IMPLEMENT", "TEST"] | "Build intent classifier and write unit tests"
Factory: [ImplementationFlow41, UnitTestingFlow51] | TransitionType: feature_addition

---

## FF-003-002 — IMPLEMENT+DOCUMENT → two flows

A: ["IMPLEMENT", "DOCUMENT"] | "Build the agent API and document the schema"
Factory: [ImplementationFlow41, DocumentationFlow62] | TransitionType: feature_addition

---

## FF-003-003 — IMPLEMENT+SECURITY → two flows

A: ["IMPLEMENT", "SECURITY"] | "Implement agent API with prompt injection prevention"
Factory: [ImplementationFlow41, SecurityAuditFlow] | TransitionType: feature_addition

---

## FF-003-004 — IMPLEMENT+TEST+DOCUMENT → three flows

A: ["IMPLEMENT", "TEST", "DOCUMENT"] | "Build classifier, write tests, document taxonomy"
Factory: [ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62] | TransitionType: feature_addition

---

## FF-003-005 — v6-01 corruption: AI_AGENT or LLM invented

A: ["IMPLEMENT"] | "Implement the LLM-powered routing service"
Correct: [ImplementationFlow41]
Corrupt: ["IMPLEMENT", "LLM"] ← v6-01 (LLM is not a predicate)
Teaching: LLM, AI_AGENT, AUTONOMOUS, RAG are not predicates. "LLM-powered" is object vocabulary. IMPLEMENT fires from the code verb.
