# Layer 0-A — Compiler Pairs (058) — SLAVerification
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-058-001 — "SLA verification" fires TEST

A: "Run SLA (Service Level Agreement) verification before the production release."
  "SLA verification" → TEST (verification in release gate context → TEST)
  Teaching: "SLA verification" → TEST. "SLA" alone or "SLO (Service Level Objective)" alone is gap.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-058-002 — "SLA" alone and "SLO" alone are gap

A: "Check the SLA compliance and monitor SLO adherence."
  "SLA compliance" → gap | "monitor SLO adherence" → gap
  Teaching: "SLA" and "SLO" alone without verification verb are gap.
  CompiledPredicateIds: []

---

## B28-compiler-058-003 — IMPLEMENT+TEST+QA for SLA checker service

A: "Build the SLA verification service, write tests for SLA threshold checks, and QA the compliance report."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "write tests" → TEST | "QA" → QA
  Teaching: Three-predicate set. "SLA verification service" → IMPLEMENT via service rule.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## B28-compiler-058-004 — "uptime" and "error budget" are gap

A: "Calculate uptime and track the error budget burn rate."
  "calculate uptime" → gap | "error budget burn rate" → gap
  Teaching: SLA metrics vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-058-005 — TEST+DOCUMENT+QA for SLA gate with criteria

A: "Execute SLA verification gate tests, document the SLA compliance criteria, and QA sign-off."
  "execute SLA verification gate tests" → TEST | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA sign-off" → QA
  Teaching: Three-predicate set. QA absolute last.
  CompiledPredicateIds: ["TEST", "DOCUMENT", "QA"]
