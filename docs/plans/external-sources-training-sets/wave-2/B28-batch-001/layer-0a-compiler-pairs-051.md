# Layer 0-A — Compiler Pairs (051) — ChaosEngineeringGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-051-001 — "chaos engineering" fires TEST

A: "Run chaos engineering experiments on the release candidate."
  "chaos engineering" → TEST (chaos experiments are a form of system testing)
  Teaching: "chaos engineering" in release gate context → TEST.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-051-002 — IMPLEMENT+TEST+QA for chaos gate service

A: "Build the chaos gate service, add chaos injection tests, and QA the resilience results."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "add chaos injection tests" → TEST | "QA" → QA
  Teaching: IMPLEMENT via build+service. TEST via test-adding verb. QA last.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## B28-compiler-051-003 — "fault injection" and "chaos scenario" alone are gap

A: "Configure fault injection scenarios and set the chaos monkey policy."
  "configure" → gap | "fault injection scenarios" → gap | "chaos monkey policy" → gap
  Teaching: Chaos configuration vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-051-004 — TEST+DOCUMENT+QA for chaos gate with criteria

A: "Execute the chaos engineering gate tests, document the resilience criteria, and QA sign-off."
  "execute the chaos engineering gate tests" → TEST | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA sign-off" → QA
  Teaching: Chaos engineering tests → TEST. DOCUMENT via verb and noun. QA last.
  CompiledPredicateIds: ["TEST", "DOCUMENT", "QA"]

---

## B28-compiler-051-005 — "steady state hypothesis" is gap

A: "Define the steady state hypothesis before running chaos experiments."
  "define" → gap (no DOCUMENT trigger without document/criteria) | "steady state hypothesis" → gap
  Teaching: "define" without documentation nouns is gap. Chaos methodology vocabulary is gap.
  CompiledPredicateIds: []
