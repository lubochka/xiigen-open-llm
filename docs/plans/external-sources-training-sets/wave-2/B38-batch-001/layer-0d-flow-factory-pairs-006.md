# Layer 0-D — Flow Factory Pairs (006)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-006-001 — QA + IMPLEMENT factory: ReleaseGateFlow71 + BackendServiceFlow41

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: OrderServiceGoLiveApproval / BuildProvenanceAudit

Factory creates two flows:

Flow 1 (IMPLEMENT):
{
  "flow_id": "BackendServiceFlow41",
  "flow_type": "deterministic_generic",
  "phase": "4.1",
  "predicate": "IMPLEMENT",
  "execution_order": 1,
  "transition_type": "feature_addition"
}

Flow 2 (QA — last):
{
  "flow_id": "ReleaseGateFlow71",
  "flow_type": "deterministic_generic",
  "phase": "7.1",
  "predicate": "QA",
  "execution_order": 2,
  "transition_type": "feature_addition"
}

---

## B38-0D-006-002 — Factory receives IMPLEMENT trigger term for artifact routing

When the factory creates BackendServiceFlow41, it receives the IMPLEMENT trigger term as context:
- "service" → service review mode
- "build" → build artifact review mode
- "pipeline" → pipeline configuration review mode

The flow ID is the same (BackendServiceFlow41) regardless of trigger term.
The trigger term is passed as metadata for N03-N07 artifact generation within the flow.

---

## B38-0D-006-003 — Execution order enforcement

The factory MUST set execution_order such that:
- IMPLEMENT flow has execution_order < QA flow's execution_order
- QA flow always has the highest execution_order (absolute rule 12)

For two flows:
- BackendServiceFlow41: execution_order = 1
- ReleaseGateFlow71: execution_order = 2

Violation: if factory sets ReleaseGateFlow71 execution_order = 1, that is a v6-03 adjacent corruption (ordering violation rather than collapse).

---

## B38-0D-006-004 — [QA, IMPLEMENT] factory TransitionType

TransitionType is set at the scenario level, not per-flow.
The scenario TransitionType = feature_addition because IMPLEMENT is present.
Both flows in the factory inherit the scenario TransitionType.

Factory output summary:
- flows: [BackendServiceFlow41, ReleaseGateFlow71]
- scenario_transition_type: feature_addition
- decision_count: 2
- phase_routing: [4.1, 7.1]
