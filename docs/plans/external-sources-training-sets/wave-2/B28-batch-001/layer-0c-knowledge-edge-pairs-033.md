# Layer 0-C — Knowledge Edge Pairs (033) — CodeCoverageGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E033-001 — CodeCoverageGate trigger edge

A: "pass CodeCoverageGate" → TEST | "verify code coverage" → TEST | "code coverage threshold" + pass verb → TEST
  Teaching: CodeCoverageGate fires TEST. "verify" fires TEST in any context.

---

## E033-002 — "enable code coverage reporting" is gap

A: "enable code coverage reporting in the CI pipeline" → [] → knowledge_edge
  Teaching: "enable" + "CI pipeline" = configuration/setup vocabulary; gap.

---

## E033-003 — "write unit tests to increase coverage" fires TEST

A: "write unit tests to increase code coverage" → TEST
  Teaching: "write" + test noun fires TEST. Coverage goal does not change the predicate.

---

## E033-004 — "merge" is gap

A: "before the branch can be merged" → [] → knowledge_edge
  Teaching: "merge" is CI/CD source control execution vocabulary; gap.

---

## E033-005 — Coverage threshold of 80% is gap vocabulary

A: "the threshold of 80%" → [] → knowledge_edge
  Teaching: Numeric thresholds and percentage values are configuration details; gap by themselves.
