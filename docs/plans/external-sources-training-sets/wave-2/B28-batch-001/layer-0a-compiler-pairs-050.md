# Layer 0-A — Compiler Pairs (050) — SyntheticMonitoring
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-050-001 — "synthetic monitoring" is gap

A: "Enable synthetic monitoring on the production endpoints."
  "enable" → gap | "synthetic monitoring" → gap | "production endpoints" → gap
  Teaching: Monitoring vocabulary is gap. No predicates fire.
  CompiledPredicateIds: []

---

## B28-compiler-050-002 — IMPLEMENT+TEST+QA for synthetic monitor service

A: "Implement the synthetic monitoring service, write tests for the probe logic, and QA the monitoring results."
  "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "write tests" → TEST | "QA" → QA
  Teaching: IMPLEMENT via verb and service noun. TEST via test-writing. QA last.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## B28-compiler-050-003 — "probe" and "synthetic transaction" are gap

A: "Configure synthetic transaction probes for the checkout flow."
  "configure" → gap | "synthetic transaction probes" → gap | "checkout flow" → gap
  Teaching: Monitoring probe configuration is gap.
  CompiledPredicateIds: []

---

## B28-compiler-050-004 — TEST+QA for synthetic test execution

A: "Run the synthetic test suite against the production endpoint and QA the pass rate."
  "synthetic test suite" → TEST | "run" → gap | "QA" → QA
  Teaching: "synthetic test suite" fires TEST via "test suite" vocabulary. QA last.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-050-005 — "alert" and "alerting" are gap

A: "Set up alerting for synthetic monitoring failures and configure alert thresholds."
  "set up" → gap | "alerting" → gap | "synthetic monitoring failures" → gap | "alert thresholds" → gap
  Teaching: Alert and alerting configuration vocabulary is gap.
  CompiledPredicateIds: []
