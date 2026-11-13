# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B38-L1-005-001 — QA + SECURITY: QA last (absolute); SECURITY before QA

Input: "Audit the security posture of the release and validate compliance before go-live approval."
PredicateSet: ["QA", "SECURITY"]

Orchestrator:
- Two flows: SecurityAuditFlow (3.4) then ReleaseGateFlow71 (7.1)
- SECURITY before QA (QA last rule applies)
- TransitionType: no_contribution

Execution:
1. SecurityAuditFlow executes first
   - Output: security_posture_report.md, compliance_check.md
2. ReleaseGateFlow71 executes second (QA last)
   - Input: security_posture_report.md
   - Output: release_gate_decision.md

---

## B38-L1-005-002 — QA last applies regardless of input order

Input order of terms does not affect execution order.

Input: "Approve the release after the security audit."
→ "approve" (QA) appears first, "security audit" (SECURITY) appears second
→ BUT execution order: SECURITY (3.4) first, QA (7.1) second

Input: "Security audit results reviewed and approved."
→ "security audit" first, "approved" second
→ Same execution order: SECURITY first, QA second

The term order in input is irrelevant to execution order. Execution order is determined by predicate ordering rules, not input text order.

---

## B38-L1-005-003 — no_contribution for QA+SECURITY

SECURITY = security audit (not code authoring)
QA = release gate review (not code authoring)
IMPLEMENT absent → no feature_addition

N09 result:
{
  "predicate_set": ["QA", "SECURITY"],
  "transition_type": "no_contribution",
  "phase_routing": ["3.4", "7.1"],
  "decision_count": 2,
  "output_files": ["security_posture_report.md", "compliance_check.md", "release_gate_decision.md"],
  "file_count": 3,
  "capability_gap_warnings": ["go-live"],
  "result_state": "security_audit_complete_gate_closed"
}

---

## B38-L1-005-004 — QA ordering in all two-predicate combinations

For any [QA, X] combination:
- X executes at order 1
- QA executes at order 2 (last)

[QA, IMPLEMENT]: IMPLEMENT (4.1) → QA (7.1)
[QA, DOCUMENT]: DOCUMENT (2.1) → QA (7.1)
[QA, SECURITY]: SECURITY (3.4) → QA (7.1)

This is the universal two-predicate pattern. QA is always index 1 (0-indexed) or position 2 (1-indexed) in a two-flow sequence.
