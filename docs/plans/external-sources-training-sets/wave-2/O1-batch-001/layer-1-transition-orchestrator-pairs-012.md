# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-012-001 — Complete orchestration rules for O1 (summary reference)

Reference pair — complete orchestration rule set for O1 scenario.

Predicate-to-flow mapping:
  SECURITY → SecurityVerification34Flow (3.4) [trusted_learned]
  AUTHORIZATION → SecurityVerification34Flow (3.4) [trusted_learned]
  SECURITY+AUTHORIZATION → SecurityVerification34Flow (3.4) [single branch, two evidence predicates]
  API_CONTRACT → ApiContract33Flow (3.3) [deterministic_generic]
  IMPLEMENT → ImplementationFlow41 (4.1) [deterministic_generic]
  TEST → generic nine-node (5.1) [UnitTestDesign51FlowAdapter rejected]
  DOCUMENT → RequirementsAuthoringFlow21 (2.1) [deterministic_generic]
  QA → QAGatingFlow73 (7.x) [deterministic_generic, always last]

Execution ordering:
  IMPLEMENT (Phase 1) → [SECURITY+AUTH ‖ API_CONTRACT] (Phase 2) → TEST (Phase 3) → QA (final)
  DOCUMENT is parallel with any phase unless it depends on another branch's output.

---

## TO-012-002 — Common O1 orchestration patterns

Reference pair — patterns with flow diagrams.

Pattern A — Single security:
  SecurityVerification34Flow

Pattern B — Security + test:
  SecurityVerification34Flow → generic-5.1

Pattern C — Implementation + security:
  ImplementationFlow41 → SecurityVerification34Flow

Pattern D — Full feature (canonical O1):
  ImplementationFlow41 → (SecurityVerification34Flow ‖ ApiContract33Flow) → generic-5.1

Pattern E — Documentation:
  SecurityVerification34Flow ‖ RequirementsAuthoringFlow21

Pattern F — Full scenario with QA:
  ImplementationFlow41 → (SecurityVerification34Flow ‖ ApiContract33Flow) → generic-5.1 → QAGatingFlow73

---

## TO-012-003 — v6 corruption detection in orchestration

Reference pair — v6 errors at the orchestration layer.

v6-01 (invented flows):
  Wrong: Flows contains "JwtAuthFlow", "TenantIsolationFlow", etc.
  Detect: any flow name not in the O1 registry

v6-02 (gap suppression in orchestration):
  Wrong: "login issues" (with jwt/auth/token) → orchestration skipped → knowledge_edge
  Detect: knowledge_edge when SECURITY lexicon tokens are present in the prompt

v6-03 (wrong routing in orchestration):
  Wrong: TEST branch placed before SECURITY branch
  Wrong: QA branch placed before other branches
  Wrong: parallel execution when sequential dependency exists
  Detect: verify branch ordering against canonical rules

---

## TO-012-004 — SelectedLogicalTransition validation checklist

Reference pair — output validation for TransitionOrchestratorUnit.

Validation checklist:
  [ ] Every predicate that fired has a corresponding flow branch
  [ ] SECURITY+AUTHORIZATION share one branch (not two separate 3.4 branches)
  [ ] QA branch, if present, is last in execution sequence
  [ ] No invented flow names
  [ ] TransitionType matches IMPLEMENT/TEST/DOCUMENT/none rule
  [ ] KnowledgeEdge is false when any predicate fired
  [ ] KnowledgeEdge is true when zero predicates fired
  [ ] ExecutionMode (parallel/sequential) follows dependency rules
  [ ] Each branch has correct TrustState from factory registry
