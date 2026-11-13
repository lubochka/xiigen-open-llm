# Layer 0-D — Flow Factory Pairs (004)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-004-001 — ARCHITECTURE+IMPLEMENT → two flows, feature_addition

A: ["ARCHITECTURE", "IMPLEMENT"] | "Design the agent system and implement the routing service"
Factory: [InfrastructureArchitectureFlow35, ImplementationFlow41] | TransitionType: feature_addition

---

## FF-004-002 — ARCHITECTURE alone → documentation TransitionType

A: ["ARCHITECTURE"] | "Design the multi-agent orchestration architecture"
Factory: [InfrastructureArchitectureFlow35] | TransitionType: documentation (no IMPLEMENT)

---

## FF-004-003 — API_CONTRACT+IMPLEMENT → two flows

A: ["API_CONTRACT", "IMPLEMENT"] | "Integrate the CRM and implement the integration adapter"
Factory: [APIContractFlow33, ImplementationFlow41] | TransitionType: feature_addition

---

## FF-004-004 — IMPLEMENT+TEST+DOCUMENT+QA → four flows

A: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"] | "Build intent module, write tests, document, QA sign-off"
Factory: [ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62, QAGatingFlow73]
TransitionType: feature_addition | QA last

---

## FF-004-005 — GapHandler: AI vocabulary sentence

A: [] | "The agent should use chain-of-thought reasoning for complex queries"
Factory: GapHandler | SCHEDULE_CAPABILITY_LEARNING
Reason: No predicate fires on "should use chain-of-thought reasoning" (AI vocabulary, no code verb).
