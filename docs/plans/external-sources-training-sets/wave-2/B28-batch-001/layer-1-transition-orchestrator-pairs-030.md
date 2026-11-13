# Layer 1 — Transition Orchestrator Pairs (030) — SecurityScanGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-030-001 — SECURITY+QA for security scan gate

A: ["SECURITY", "QA"] | SecurityFlow6x → ReleaseGateFlow71 (QA last)
  TransitionType: documentation
  Teaching: SECURITY gate runs first. QA gate validates security scan results last.

## B28-transition-030-002 — IMPLEMENT+SECURITY+QA with IMPLEMENT driving feature_addition

A: ["IMPLEMENT", "SECURITY", "QA"] | parallel(IMPLEMENT ‖ SECURITY) → QA
  TransitionType: feature_addition
  Teaching: IMPLEMENT and SECURITY can run in parallel. QA is last. IMPLEMENT drives feature_addition.

## B28-transition-030-003 — Gap: "configure SAST/DAST tool" → no orchestration

A: [] | "configure SAST (Static Application Security Testing) and DAST (Dynamic Application Security Testing) scanner" → no flows → no orchestration
  Teaching: Security tool configuration is gap. No orchestration.

## B28-transition-030-004 — ARCHITECTURE+SECURITY+IMPLEMENT+QA

A: ["ARCHITECTURE", "SECURITY", "IMPLEMENT", "QA"] | ARCHITECTURE → parallel(SECURITY ‖ IMPLEMENT) → QA
  TransitionType: feature_addition
  Teaching: ARCHITECTURE first. SECURITY and IMPLEMENT in parallel. QA last.

## B28-transition-030-005 — FAIL for critical CVE in security scan

A: ReleaseGateFlow71: FAIL if critical CVE (Common Vulnerability and Exposure) severity found
  Teaching: Critical severity CVE → FAIL. Models must not normalize to PASS_WITH_WARNINGS.
