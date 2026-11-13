# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-002-001 — Gap: no orchestration

A: [] | "Adapt the agent to handle new query types"
Orchestration: GapHandler → SCHEDULE_CAPABILITY_LEARNING → subproject 16. No flows, no TransitionType.

---

## TO-002-002 — IMPLEMENT: single flow

A: ["IMPLEMENT"] | "Build the intent routing service"
Orchestration: ImplementationFlow41 only. TransitionType: feature_addition.

---

## TO-002-003 — IMPLEMENT+TEST: sequential

A: ["IMPLEMENT", "TEST"] | "Build and test the classifier"
Orchestration: ImplementationFlow41 → UnitTestingFlow51 (IMPLEMENT before TEST — tests require implementation).

---

## TO-002-004 — ARCHITECTURE+IMPLEMENT: sequential

A: ["ARCHITECTURE", "IMPLEMENT"] | "Design and implement the agent system"
Orchestration: InfrastructureArchitectureFlow35 → ImplementationFlow41. ARCHITECTURE before IMPLEMENT.

---

## TO-002-005 — IMPLEMENT+TEST+DOCUMENT+QA: four flows

A: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"] | "Build, test, document, QA sign-off"
Orchestration: IMPLEMENT → TEST (parallel with DOCUMENT possible) → QA (always last).
TransitionType: feature_addition | QA validates all preceding work.
