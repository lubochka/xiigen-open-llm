# Layer 1 — Transition Orchestrator Pairs (032) — LicenseComplianceGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-032-001 — SECURITY+QA for license compliance gate

A: ["SECURITY", "QA"] | SecurityFlow6x(LicenseComplianceGate) → ReleaseGateFlow71 (QA last)
  TransitionType: documentation
  Teaching: License compliance fires SECURITY. QA validates last.

## B28-transition-032-002 — DOCUMENT+SECURITY+QA orchestration

A: ["DOCUMENT", "SECURITY", "QA"] | parallel(DOCUMENT ‖ SECURITY) → QA
  TransitionType: documentation
  Teaching: DOCUMENT and SECURITY can run in parallel. QA last.

## B28-transition-032-003 — Gap: "update license file" → no orchestration

A: [] | "update the LICENSE file and add copyright headers" → no flows → no orchestration
  Teaching: License file management is gap. No orchestration.

## B28-transition-032-004 — IMPLEMENT+SECURITY+DOCUMENT+QA

A: ["IMPLEMENT", "SECURITY", "DOCUMENT", "QA"] | parallel(IMPLEMENT ‖ SECURITY ‖ DOCUMENT) → QA
  TransitionType: feature_addition
  Teaching: Three parallel flows, then QA last. IMPLEMENT drives feature_addition.

## B28-transition-032-005 — FAIL for copyleft license violation

A: ReleaseGateFlow71: FAIL if copyleft license detected in proprietary codebase
  Teaching: Copyleft license violation → FAIL. License compliance violations cannot be normalized.
