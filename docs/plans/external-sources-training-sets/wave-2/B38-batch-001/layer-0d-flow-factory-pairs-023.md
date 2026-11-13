# Layer 0-D — Flow Factory Pairs (023)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B38-0D-023-001 — Factory for ReleaseSignOffCollection: QA alone (canonical)

PredicateSet: ["QA"]
Scenario: ReleaseSignOffCollection

Flow 1: ReleaseGateFlow71 (Phase 7.1, QA, order 1)
FlowCount: 1
TransitionType: no_contribution
Gate B required: false
Entry state: release_sign_off_pending
Exit state: release_sign_off_complete

---

## B38-0D-023-002 — Factory for AnalyticsServiceSignOff: QA+IMPLEMENT

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: AnalyticsServiceSignOff

Flow 1: BackendServiceFlow41 (Phase 4.1, IMPLEMENT, order 1)
Flow 2: ReleaseGateFlow71 (Phase 7.1, QA, order 2 — last)
FlowCount: 2
TransitionType: feature_addition
Gate B required: false

---

## B38-0D-023-003 — Factory for SignOffChecklist: QA with gap warning

PredicateSet: ["QA"]
Scenario: SignOffChecklist

Flow 1: ReleaseGateFlow71 (Phase 7.1, QA, order 1)
FlowCount: 1
TransitionType: no_contribution
CapabilityGapWarnings: ["checklist"] — warning only; "checklist" does not create a flow
Gate B required: false

---

## B38-0D-023-004 — Factory for SignOffDocumentation: QA+DOCUMENT

PredicateSet: ["QA", "DOCUMENT"]
Scenario: SignOffDocumentation

Flow 1: DocumentationFlow (Phase 2.1, DOCUMENT, order 1)
Flow 2: ReleaseGateFlow71 (Phase 7.1, QA, order 2 — last)
FlowCount: 2
TransitionType: no_contribution
Gate B required: false

---

## B38-0D-023-005 — Factory for GoLiveSignOff: QA with three gap warnings

PredicateSet: ["QA"]
Scenario: GoLiveSignOff

Flow 1: ReleaseGateFlow71 (Phase 7.1, QA, order 1)
FlowCount: 1
TransitionType: no_contribution
CapabilityGapWarnings: ["go-live", "checklist", "launch"] — three warnings, zero additional flows

This is the canonical v6-01 factory demonstration:
The factory receives PredicateSet: ["QA"] with three gap warnings.
It creates exactly one flow (ReleaseGateFlow71).
It does NOT create flows for gap vocabulary items.
