# Layer 0-D — Flow Factory Pairs (009)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-009-001 — Four-predicate factory: QA+IMPLEMENT+DOCUMENT+SECURITY

PredicateSet: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
Scenario: FullReleaseGatePipeline

Factory creates four flows:

Flow 1 (IMPLEMENT):
{
  "flow_id": "BackendServiceFlow41",
  "phase": "4.1",
  "execution_order": 1
}

Flow 2 (DOCUMENT):
{
  "flow_id": "DocumentationFlow",
  "phase": "2.1",
  "execution_order": 2
}

Flow 3 (SECURITY):
{
  "flow_id": "SecurityAuditFlow",
  "phase": "3.4",
  "execution_order": 3
}

Flow 4 (QA — last, absolute):
{
  "flow_id": "ReleaseGateFlow71",
  "phase": "7.1",
  "execution_order": 4
}

scenario_transition_type: feature_addition
decision_count: 4

---

## B38-0D-009-002 — Information flow through four-phase execution

Execution order and data flow:

N01: Input analyzed → ["QA","IMPLEMENT","DOCUMENT","SECURITY"]

Phase 4.1 (IMPLEMENT, order 1):
- Receives: original input
- Produces: service_review.md, build_provenance.md
- Passes to: Phase 2.1

Phase 2.1 (DOCUMENT, order 2):
- Receives: service_review.md (reference for criteria doc)
- Produces: criteria_specification.md, release_requirements.md
- Passes to: Phase 3.4

Phase 3.4 (SECURITY, order 3):
- Receives: service_review.md, criteria_specification.md
- Produces: security_posture_report.md, compliance_check.md
- Passes to: Phase 7.1

Phase 7.1 (QA, order 4 — last):
- Receives: all prior phase outputs
- Produces: release_gate_decision.md, sign_off_register.md
- Final output: complete release gate result

---

## B38-0D-009-003 — Why QA comes last in four-predicate flow

Phase 7.1 (QA) is the approval decision maker.
It needs all evidence:
- Code review (4.1) → quality of implementation
- Documentation (2.1) → criteria and requirements satisfied
- Security audit (3.4) → security posture cleared

Without prior phases, QA would make a gate decision without evidence.
The ordering ensures the gate has maximum information before approving or rejecting.

---

## B38-0D-009-004 — Factory correctness assertion for four predicates

Gate B assertions for four-predicate factory:
1. len(flows) == 4
2. execution_order values are [1, 2, 3, 4] (no gaps, no duplicates)
3. ReleaseGateFlow71 has execution_order == max(execution_orders) == 4
4. decision_count == 4
5. transition_type == feature_addition (IMPLEMENT present)
6. All four phase targets covered: 4.1, 2.1, 3.4, 7.1
