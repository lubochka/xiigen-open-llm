# Layer 1 — Transition Orchestrator Pairs (036) — Complete QA Orchestrator Reference (Round 2)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-036-001 — QA alone: one gate, no TransitionType, no preceding flows

A: ["QA"] | ReleaseGateFlow71 only | no TransitionType | no preceding flow steps
  Teaching: QA alone → single gate. Nothing precedes QA in this case.

## B28-transition-036-002 — QA+IMPLEMENT: feature_addition, strictly sequential after IMPLEMENT

A: ["IMPLEMENT", "QA"] | IMPLEMENT → QA | feature_addition
  Teaching: IMPLEMENT must complete before QA gate. No parallelism between IMPLEMENT and QA.

## B28-transition-036-003 — Five-predicate complete orchestration sequence

A: ["ARCHITECTURE", "IMPLEMENT", "SECURITY", "TEST", "QA"] | ARCHITECTURE → parallel(IMPLEMENT ‖ SECURITY ‖ TEST) → QA
  TransitionType: feature_addition
  Teaching: ARCHITECTURE first. Three parallel flows. QA strictly last.

## B28-transition-036-004 — Gap produces no orchestration in Round 2 contexts

A: [] | any Round 2 gap vocabulary → no flows → no orchestration
  Teaching: Gap → knowledge_edge. No orchestration steps executed. No TransitionType.

## B28-transition-036-005 — FAIL and PASS_WITH_WARNINGS are valid orchestration outcomes

A: ReleaseGateFlow71 outcome options: PASS | PASS_WITH_WARNINGS | FAIL
  Teaching: All three verdict types are valid. Models must not normalize FAIL to PASS in any Round 2 context.
