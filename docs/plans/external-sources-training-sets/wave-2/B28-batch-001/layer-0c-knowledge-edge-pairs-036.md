# Layer 0-C — Knowledge Edge Pairs (036) — Complete Knowledge Edge Reference (Round 2)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E036-001 — Round 2 gate predicate assignment summary

Smoke/Sanity/Regression/Performance/Load/Coverage/StaticAnalysis: all fire TEST
SecurityScan/ContainerScan/DependencyCheck/LicenseCompliance: all fire SECURITY
  Teaching: Test-metric gates → TEST. Security/compliance/vulnerability gates → SECURITY.

---

## E036-002 — Gap vocabulary specific to Round 2

Round 2 gap terms: "trigger", "configure", "enable", "monitor", "push to registry", "merge", "commit", "promoting", "publishing", "CI pipeline" (alone), "pull request" (alone), "schedule" (execution scheduling)
  Teaching: None of these fire any predicate; all produce knowledge_edge output.

---

## E036-003 — "validate" fires TEST in all Round 2 gate contexts

A: "validate SmokeTestSuite" → TEST | "validate PerformanceGate" → TEST | "validate license" → TEST | "validate security findings" → TEST
  Teaching: "validate" fires TEST universally, regardless of what is being validated.

---

## E036-004 — "service" fires IMPLEMENT in all Round 2 gate service names

A: "gate runner service" → IMPLEMENT | "threshold service" → IMPLEMENT | "checker service" → IMPLEMENT | "harness service" → IMPLEMENT
  Teaching: "service" fires IMPLEMENT always, including all Round 2 gate service compound nouns.

---

## E036-005 — "criteria" fires DOCUMENT in all Round 2 gate criteria phrases

A: "smoke test criteria" → DOCUMENT | "performance gate criteria" → DOCUMENT | "security gate criteria" → DOCUMENT | "coverage criteria" → DOCUMENT
  Teaching: "criteria" fires DOCUMENT always, including all Round 2 gate criteria compound nouns.
