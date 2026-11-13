# Layer 0-D — Flow Factory Pairs (007)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-007-001 — QA + DOCUMENT factory: ReleaseGateFlow71 + DocumentationFlow

PredicateSet: ["QA", "DOCUMENT"]
Scenario: ReleaseGateReview / DeploymentReadinessCheck

Factory creates two flows:

Flow 1 (DOCUMENT):
{
  "flow_id": "DocumentationFlow",
  "flow_type": "deterministic_generic",
  "phase": "2.1",
  "predicate": "DOCUMENT",
  "execution_order": 1,
  "transition_type": "no_contribution"
}

Flow 2 (QA — last):
{
  "flow_id": "ReleaseGateFlow71",
  "flow_type": "deterministic_generic",
  "phase": "7.1",
  "predicate": "QA",
  "execution_order": 2,
  "transition_type": "no_contribution"
}

---

## B38-0D-007-002 — DocumentationFlow artifact generation

DocumentationFlow (Phase 2.1) for criteria-based DOCUMENT:

Input context: "release criteria" / "acceptance criteria" / "exit criteria"
Output artifacts:
- criteria_specification.md (the criteria document)
- requirements_trace.md (traceability matrix)

These artifacts are passed to ReleaseGateFlow71 as input for the gate decision.
ReleaseGateFlow71 references criteria_specification.md when producing the gate report.

---

## B38-0D-007-003 — Factory output for QA+DOCUMENT

Factory summary:
- flows: [DocumentationFlow, ReleaseGateFlow71]
- scenario_transition_type: no_contribution
- decision_count: 2
- phase_routing: [2.1, 7.1]
- output_files: [criteria_specification.md, requirements_trace.md, release_gate_report.md]

Note: no_contribution is the scenario TransitionType even though documentation is produced. DOCUMENT phase output counts as documentation, not as a code feature_addition.

---

## B38-0D-007-004 — [QA, DOCUMENT] factory does not invoke BackendServiceFlow41

The absence of IMPLEMENT in ["QA", "DOCUMENT"] means:
- BackendServiceFlow41 is NOT created
- Phase 4.1 receives NO routing decision
- No service/build/pipeline review is performed
- Only DocumentationFlow (2.1) and ReleaseGateFlow71 (7.1) are in the factory output

This is the expected, correct behavior. The factory creates exactly as many flows as there are predicates.
