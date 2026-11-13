# Layer 0-A — Compiler Pairs (034) — StaticAnalysisGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-034-001 — "StaticAnalysisGate" vocabulary

A: "The StaticAnalysisGate must pass before the branch can be deployed."
  "StaticAnalysisGate" → TEST (static analysis = code quality test gate) | "deployed" → gap
  Teaching: StaticAnalysisGate is a test gate artifact; fires TEST. "deployed" is gap.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-034-002 — "run static analysis" + "QA sign-off"

A: "Run static analysis on the codebase and QA sign-off before merge."
  "run static analysis" → TEST (analysis = test-type scan) | "QA sign-off" → QA
  Teaching: "static analysis" fires TEST; "QA sign-off" fires QA.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-034-003 — "implement" linting rule + "document criteria"

A: "Implement the custom linting rule service and document the analysis criteria."
  "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup)
  Teaching: IMPLEMENT fires twice (deduplicated); DOCUMENT fires twice (deduplicated).
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]

---

## B28-compiler-034-004 — "secure" + "validate static analysis"

A: "Secure the analysis tool configuration and validate static analysis findings."
  "secure" → SECURITY | "validate" → TEST
  Teaching: "secure" fires SECURITY; "validate" fires TEST always.
  CompiledPredicateIds: ["SECURITY", "TEST"]

---

## B28-compiler-034-005 — Pure gap: "configure static analysis"

A: "Configure the StaticAnalysisGate to run on every pull request."
  "configure" → gap | "pull request" → gap
  Teaching: "configure" is setup/configuration vocabulary; gap. "pull request" is CI/CD vocabulary; gap.
  CompiledPredicateIds: [] → gap (knowledge_edge)
