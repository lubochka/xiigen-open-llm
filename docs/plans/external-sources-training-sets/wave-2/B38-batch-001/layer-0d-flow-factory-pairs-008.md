# Layer 0-D — Flow Factory Pairs (008)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B38-0D-008-001 — QA + SECURITY factory: ReleaseGateFlow71 + SecurityAuditFlow

PredicateSet: ["QA", "SECURITY"]
Scenario: SecurityAuditRelease (pre-release security audit)

Factory creates two flows:

Flow 1 (SECURITY):
{
  "flow_id": "SecurityAuditFlow",
  "flow_type": "deterministic_generic",
  "phase": "3.4",
  "predicate": "SECURITY",
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

## B38-0D-008-002 — SecurityAuditFlow output fed to ReleaseGateFlow71

SecurityAuditFlow (Phase 3.4) produces:
- security_posture_report.md (overall security assessment)
- vulnerability_summary.md (CVE scan results)
- compliance_check.md (regulatory compliance status)

ReleaseGateFlow71 (Phase 7.1) receives security_posture_report.md as evidence.
The gate decision (approve/reject) is informed by the security findings.

This is why QA is last (ordering rule): the gate needs security results first.

---

## B38-0D-008-003 — Factory output for QA+SECURITY

Factory summary:
- flows: [SecurityAuditFlow, ReleaseGateFlow71]
- scenario_transition_type: no_contribution
- decision_count: 2
- phase_routing: [3.4, 7.1]
- output_files: [security_posture_report.md, vulnerability_summary.md, compliance_check.md, release_gate_decision.md]

---

## B38-0D-008-004 — no_contribution for QA+SECURITY

TransitionType = no_contribution for [QA, SECURITY]:
- SecurityAuditFlow: runs a security review (not code authoring)
- ReleaseGateFlow71: runs a release gate (not code authoring)
- No IMPLEMENT = no feature_addition

This is correct. A pre-release security audit with release gate sign-off does not constitute writing new code. It is a quality and security review process.
