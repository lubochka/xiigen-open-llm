# Layer 0-A — Compiler Pairs (041) — ManualIntervention
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-041-001 — "manual intervention" is gap

A: "Pause for manual intervention before the production cutover."
  "pause" → gap | "manual intervention" → gap | "production cutover" → gap
  Teaching: Manual intervention pipeline vocabulary is pure gap.
  CompiledPredicateIds: []

---

## B28-compiler-041-002 — IMPLEMENT+TEST for manual intervention handler

A: "Implement the manual intervention handler and write tests for the pause/resume workflow."
  "implement" → IMPLEMENT | "handler" → gap (not "service") | "write tests" → TEST
  Teaching: "handler" alone does not fire IMPLEMENT; "implement" verb does. TEST via test-writing verb.
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]

---

## B28-compiler-041-003 — DOCUMENT+QA for manual intervention runbook

A: "Document the manual intervention runbook criteria and QA the escalation steps."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "runbook" → gap | "QA" → QA
  Teaching: "criteria" fires DOCUMENT independently. "runbook" is gap. QA last.
  CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## B28-compiler-041-004 — "pause pipeline" and "resume pipeline" are gap

A: "Configure pipeline pause and resume for manual intervention windows."
  "configure" → gap | "pause pipeline" → gap | "resume pipeline" → gap | "intervention windows" → gap
  Teaching: All pipeline pause/resume vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-041-005 — ARCHITECTURE+IMPLEMENT+QA for intervention framework

A: "Design the manual intervention architecture, build the pause state machine service, and QA the recovery paths."
  "design" → ARCHITECTURE | "architecture" → ARCHITECTURE (dedup) | "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "QA" → QA
  Teaching: Three-predicate set. QA validates after ARCHITECTURE and IMPLEMENT.
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "QA"]
