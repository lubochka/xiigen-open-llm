# Layer 0-D — Flow Factory Pairs (005)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-005-001 — v6-03 corruption: ARCHITECTURE suppressed

A: ["ARCHITECTURE", "IMPLEMENT"]
B: CORRUPT: FlowFactory.Create("IMPLEMENT") only → ImplementationFlow41 (ARCHITECTURE suppressed) → v6-03
  Correct: FlowFactory.CreateMulti(["ARCHITECTURE","IMPLEMENT"]) → [InfrastructureArchitectureFlow35, ImplementationFlow41]
  Teaching: v6-03 collapses multi-predicate routing to a single flow. ARCHITECTURE must never be suppressed by IMPLEMENT or any other predicate.

---

## B14-factory-005-002 — v6-01 corruption: DEVOPS invented

A: ["ARCHITECTURE"] | cloud infra context
B: CORRUPT: FlowFactory.Create("DEVOPS") invented → v6-01
  Correct: FlowFactory.Create("ARCHITECTURE") → InfrastructureArchitectureFlow35
  Teaching: v6-01 invents predicates. DEVOPS is not a valid predicate in the lexicon.

---

## B14-factory-005-003 — ARCHITECTURE alone with documentation TransitionType

A: ["ARCHITECTURE"] | "design" verb only, no "implement"
B: InfrastructureArchitectureFlow35 | TransitionType: documentation
  Teaching: ARCHITECTURE without IMPLEMENT → TransitionType: documentation. The design artifact is a documentation output.

---

## B14-factory-005-004 — ARCHITECTURE with "refactor" verb → TransitionType: refactoring

A: ["ARCHITECTURE"] | "refactor the architecture" verb
B: InfrastructureArchitectureFlow35 | TransitionType: refactoring
  Teaching: "refactor" verb overrides documentation TransitionType for ARCHITECTURE.

---

## B14-factory-005-005 — gap: no flow created

A: []
B: FlowFactory.CreateGap() → GapFlow | No InfrastructureArchitectureFlow35
  DomainId: "software-development" | SCHEDULE_CAPABILITY_LEARNING emitted to subproject 16 (AutonomousLearningPipeline (16))

