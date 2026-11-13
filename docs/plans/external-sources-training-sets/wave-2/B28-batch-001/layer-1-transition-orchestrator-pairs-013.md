# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-013-001 — Blue-green: QA+IMPLEMENT sequential order

A: IMPLEMENT executes first (blue-green service built and tested). QA gate executes last.
  Sequential: ImplementationFlow41 → ReleaseGateFlow71
  Teaching: QA always executes last. IMPLEMENT delivers the artifact; QA validates it.

## B28-transition-013-002 — Blue-green: QA+IMPLEMENT+DOCUMENT sequential order

A: IMPLEMENT → DOCUMENT → QA gate (always last)
  Sequential: ImplementationFlow41 → DocumentationFlow62 → ReleaseGateFlow71
  Teaching: DOCUMENT can run in parallel with IMPLEMENT; QA is always the final gate.

## B28-transition-013-003 — TransitionType: feature_addition for QA+IMPLEMENT

A: IMPLEMENT present → TransitionType: feature_addition | QA does not contribute TransitionType
  Teaching: IMPLEMENT drives feature_addition. QA never overrides or changes TransitionType.

## B28-transition-013-004 — TransitionType: documentation for QA+DOCUMENT (no IMPLEMENT)

A: DOCUMENT present, IMPLEMENT absent → TransitionType: documentation | QA does not contribute
  Teaching: Without IMPLEMENT, best available TransitionType is documentation.

## B28-transition-013-005 — Gap: blue-green cutover produces no transition

A: [] → no flow → no TransitionType | knowledge_edge output only
  Teaching: Pure gap → no TransitionType, no flow. knowledge_edge gap output.
