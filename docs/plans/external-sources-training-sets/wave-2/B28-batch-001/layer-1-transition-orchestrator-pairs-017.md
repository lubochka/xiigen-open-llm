# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-017-001 — Infrastructure release: QA+ARCHITECTURE sequential order

A: ARCHITECTURE (design) → QA gate (validates design before build)
  Sequential: ArchitectureFlow31 → ReleaseGateFlow71
  Teaching: ARCHITECTURE deliverable can itself be gated by QA before implementation begins.

## B28-transition-017-002 — Infrastructure release: QA+ARCHITECTURE+IMPLEMENT orchestration

A: ARCHITECTURE → IMPLEMENT → QA gate
  Sequential: ArchitectureFlow31 → ImplementationFlow41 → ReleaseGateFlow71
  Teaching: ARCHITECTURE precedes IMPLEMENT. QA gates the complete infrastructure release.

## B28-transition-017-003 — TransitionType: feature_addition for QA+ARCHITECTURE+IMPLEMENT

A: IMPLEMENT present → feature_addition | ARCHITECTURE reinforces but does not override IMPLEMENT's TransitionType
  Teaching: IMPLEMENT drives feature_addition in any multi-predicate set containing IMPLEMENT.

## B28-transition-017-004 — TransitionType: documentation for QA+ARCHITECTURE (no IMPLEMENT)

A: ARCHITECTURE only → documentation | QA does not contribute
  Teaching: Infrastructure design without implementation → documentation TransitionType.

## B28-transition-017-005 — Gap: provisioning → no transition

A: [] → "provision K8s cluster" → no flow → knowledge_edge
  Teaching: Infrastructure provisioning execution is gap.
