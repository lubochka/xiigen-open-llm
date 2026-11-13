# Layer 0-C — Knowledge Edge Pairs (030) — SecurityScanGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E030-001 — SecurityScanGate trigger edge

A: "run SecurityScanGate" → SECURITY | "security scan" + execution verb → SECURITY
  Teaching: SecurityScanGate fires SECURITY. Not TEST. Security scan vocabulary → SECURITY.

---

## E030-002 — "merging" is gap

A: "before merging to main" → [] → knowledge_edge
  Teaching: "merging" is CI/CD source control vocabulary; gap.

---

## E030-003 — "secure" verb fires SECURITY always

A: "secure the authentication endpoints" → SECURITY | "secure the container" → SECURITY
  Teaching: "secure" verb fires SECURITY regardless of what is being secured.

---

## E030-004 — "commit" is gap

A: "trigger SecurityScanGate on each commit" → [] → knowledge_edge
  Teaching: "commit" is source control execution vocabulary; gap.

---

## E030-005 — SecurityScanGate vs StaticAnalysisGate: both fire TEST except SecurityScan

A: "SecurityScanGate" → SECURITY | "StaticAnalysisGate" → TEST | "RegressionGate" → TEST | "CodeCoverageGate" → TEST
  Teaching: Only SecurityScanGate and ContainerScanGate fire SECURITY. All other gates fire TEST.
