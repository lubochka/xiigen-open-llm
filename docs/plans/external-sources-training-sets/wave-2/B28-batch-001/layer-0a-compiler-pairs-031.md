# Layer 0-A — Compiler Pairs (031) — DependencyCheckGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-031-001 — "DependencyCheckGate" vocabulary

A: "All releases must pass the DependencyCheckGate before promotion."
  "DependencyCheckGate" → SECURITY (dependency check = security vulnerability scan) | "promotion" → gap
  Teaching: DependencyCheckGate fires SECURITY (third-party dependency vulnerability check). "promotion" is gap.
  CompiledPredicateIds: ["SECURITY"]

---

## B28-compiler-031-002 — "check dependencies" + "QA sign-off"

A: "Check all third-party dependencies for vulnerabilities and QA sign-off."
  "check dependencies" → SECURITY (vulnerability check) | "QA sign-off" → QA (abbreviation compound)
  Teaching: Dependency vulnerability check fires SECURITY; "QA sign-off" fires QA.
  CompiledPredicateIds: ["SECURITY", "QA"]

---

## B28-compiler-031-003 — "implement" dependency check + "document criteria"

A: "Implement the dependency checker service and document the criteria for blocking CVEs."
  "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup)
  Teaching: IMPLEMENT fires twice (deduplicated); DOCUMENT fires twice (deduplicated).
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]

---

## B28-compiler-031-004 — "secure" + "validate dependencies"

A: "Secure the build pipeline and validate all dependency versions before release."
  "secure" → SECURITY | "validate" → TEST (validate fires TEST in any context)
  Teaching: "secure" fires SECURITY; "validate" always fires TEST.
  CompiledPredicateIds: ["SECURITY", "TEST"]

---

## B28-compiler-031-005 — Pure gap: "update dependencies"

A: "Update all npm dependencies to the latest version before the rollout."
  "update" → gap | "rollout" → gap
  Teaching: "update" alone is a maintenance task vocabulary word; "rollout" is gap. No predicates fire.
  CompiledPredicateIds: [] → gap (knowledge_edge)
