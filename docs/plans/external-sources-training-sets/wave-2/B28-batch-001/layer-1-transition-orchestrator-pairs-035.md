# Layer 1 — Transition Orchestrator Pairs (035) — ContainerScanGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B28-transition-035-001 — SECURITY+QA for container scan gate

A: ["SECURITY", "QA"] | SecurityFlow6x(ContainerScanGate) → ReleaseGateFlow71 (QA last)
  TransitionType: documentation
  Teaching: Container scan gate → SECURITY flow → SecurityFlow6x. QA validates last.

## B28-transition-035-002 — IMPLEMENT+SECURITY+QA for container with new build

A: ["IMPLEMENT", "SECURITY", "QA"] | parallel(IMPLEMENT ‖ SECURITY) → QA
  TransitionType: feature_addition
  Teaching: IMPLEMENT and SECURITY can run in parallel. QA last. IMPLEMENT drives feature_addition.

## B28-transition-035-003 — Gap: "configure container registry" → no orchestration

A: [] | "configure the container registry and set image pull policy" → no flows → no orchestration
  Teaching: Container registry configuration is gap. No orchestration.

## B28-transition-035-004 — ARCHITECTURE+IMPLEMENT+SECURITY+QA

A: ["ARCHITECTURE", "IMPLEMENT", "SECURITY", "QA"] | ARCHITECTURE → parallel(IMPLEMENT ‖ SECURITY) → QA
  TransitionType: feature_addition
  Teaching: ARCHITECTURE first. IMPLEMENT and SECURITY in parallel. QA last.

## B28-transition-035-005 — FAIL for critical container image vulnerability

A: ReleaseGateFlow71: FAIL if critical severity CVE found in container base image layers
  Teaching: Critical container vulnerability → FAIL. Cannot be normalized for any release context.
