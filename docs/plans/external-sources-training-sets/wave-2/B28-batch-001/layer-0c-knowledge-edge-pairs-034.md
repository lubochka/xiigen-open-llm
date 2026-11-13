# Layer 0-C — Knowledge Edge Pairs (034) — StaticAnalysisGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E034-001 — StaticAnalysisGate trigger edge

A: "pass StaticAnalysisGate" → TEST | "run static analysis" → TEST
  Teaching: Static analysis gate fires TEST (code quality test gate, not security scan).

---

## E034-002 — "configure StaticAnalysisGate" is gap

A: "configure the StaticAnalysisGate to run on every pull request" → [] → knowledge_edge
  Teaching: "configure" is setup vocabulary; gap. "pull request" confirms CI/CD gap context.

---

## E034-003 — "deployed" is gap even adjacent to gate name

A: "before the branch can be deployed" → [] → knowledge_edge
  Teaching: "deployed" is pipeline execution vocabulary; gap regardless of context.

---

## E034-004 — "implement custom linting rule service" fires IMPLEMENT (twice, deduplicated)

A: "implement the custom linting rule service" → IMPLEMENT (implement verb + service noun — deduplicated)
  Teaching: "service" fires IMPLEMENT always, including "linting rule service".

---

## E034-005 — StaticAnalysisGate fires TEST, not SECURITY

A: "StaticAnalysisGate" → TEST | NOT SECURITY
  Teaching: Static analysis is a code quality gate (TEST). Container scans and security scans are SECURITY. Distinction is important.
