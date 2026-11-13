# Layer 0-D — Flow Factory Pairs (007)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-007-001 — TEST (5.1) for AI service unit tests

A: ["TEST"] | "Write unit tests for the intent classifier service"
Factory: [UnitTestingFlow51] | Sub-phase 5.1 (unit, service logic) | TransitionType: documentation

---

## FF-007-002 — TEST (5.3) for AI integration tests

A: ["TEST"] | "Write integration tests for the agent API endpoints"
Factory: [E2ETestingFlow53] | Sub-phase 5.3 (integration/API context) | TransitionType: documentation

---

## FF-007-003 — DOCUMENT for AI taxonomy

A: ["DOCUMENT"] | "Document the intent taxonomy for the classifier"
Factory: [DocumentationFlow62] | TransitionType: documentation

---

## FF-007-004 — ARCHITECTURE+IMPLEMENT: full AI system build

A: ["ARCHITECTURE", "IMPLEMENT"] | "Design and build the multi-agent routing system"
Factory: [InfrastructureArchitectureFlow35, ImplementationFlow41] | TransitionType: feature_addition

---

## FF-007-005 — GapHandler: "model evaluation"

A: [] | "Evaluate the agent model on our benchmark dataset"
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: "evaluate" (ML sense) is gap. Does not fire TEST (no test-writing verb).
