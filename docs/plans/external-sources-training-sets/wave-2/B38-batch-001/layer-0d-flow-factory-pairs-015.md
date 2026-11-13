# Layer 0-D — Flow Factory Pairs (015)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B38-0D-015-001 — Factory for HotfixReleaseApproval: QA alone

PredicateSet: ["QA"]
Scenario: HotfixReleaseApproval

Flow 1: ReleaseGateFlow71 (Phase 7.1, QA, order 1)
FlowCount: 1
TransitionType: no_contribution
Gate B required: false

---

## B38-0D-015-002 — Factory for HotfixAuthServiceRelease: QA+IMPLEMENT

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: HotfixAuthServiceRelease

Flow 1: BackendServiceFlow41 (Phase 4.1, IMPLEMENT, order 1)
Flow 2: ReleaseGateFlow71 (Phase 7.1, QA, order 2 — last)
FlowCount: 2
TransitionType: feature_addition
Gate B required: false

---

## B38-0D-015-003 — Factory for HotfixCriteriaDocumentation: QA+DOCUMENT

PredicateSet: ["QA", "DOCUMENT"]
Scenario: HotfixCriteriaDocumentation

Flow 1: DocumentationFlow (Phase 2.1, DOCUMENT, order 1)
Flow 2: ReleaseGateFlow71 (Phase 7.1, QA, order 2 — last)
FlowCount: 2
TransitionType: no_contribution
Gate B required: false

---

## B38-0D-015-004 — Factory: rollback gap — no flows

PredicateSet: []
Scenario: HotfixRollbackGap

Factory output: null
FlowCount: 0
TransitionType: N/A
Result: knowledge_edge
