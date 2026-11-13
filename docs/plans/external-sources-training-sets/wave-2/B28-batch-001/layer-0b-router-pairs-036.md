# Layer 0-B — Router Pairs (036) — Complete QA Router Reference (Round 2)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-036-001 — QA alone → ReleaseGateFlow71 only, no TransitionType

A: ["QA"] | ReleaseGateFlow71 only | no TransitionType
  Teaching: QA alone → gate only. No contribution flow. No TransitionType.

## B28-router-036-002 — QA+TEST: feature_addition absent; documentation absent; TEST drives documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: TEST without IMPLEMENT → documentation. QA does not override.

## B28-router-036-003 — QA+IMPLEMENT: feature_addition from IMPLEMENT

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA is last gate.

## B28-router-036-004 — "ensure quality" → gap → knowledge_edge (not QA)

A: [] | "ensure quality of release" → GapFlow | knowledge_edge
  Teaching: "ensure quality" fires no predicate. Pure gap → knowledge_edge.

## B28-router-036-005 — Five-predicate Round 2 flow: ARCHITECTURE+IMPLEMENT+TEST+SECURITY+QA

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "SECURITY", "QA"] | [ArchitectureFlow31, ImplementationFlow41, TestingFlow, SecurityFlow6x, ReleaseGateFlow71] | feature_addition
  Sequential: ARCHITECTURE → parallel(IMPLEMENT ‖ SECURITY) → TEST → QA (last)
  Teaching: IMPLEMENT drives feature_addition. QA always last.
