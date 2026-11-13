# Layer 0-A — Compiler Pairs (013)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-013-001 — QA+IMPLEMENT: blue-green service implementation

A: "Build the blue-green deployment service and QA the cutover logic before go-live."
  "build" → IMPLEMENT | "QA" → QA | "cutover" → gap vocabulary | "go-live" → gap vocabulary
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-013-002 — QA+TEST: blue-green smoke validation

A: "QA the blue-green switch and verify the health checks pass on the green environment."
  "QA" → QA | "verify" → TEST | "health checks" → gap | "green environment" → gap
  CompiledPredicateIds: ["QA", "TEST"]

---

## B28-compiler-013-003 — gap: blue-green cutover instruction

A: "Switch traffic to the green environment and decommission the blue stack."
  "switch traffic" → gap | "decommission" → gap | "green environment" → gap | "blue stack" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-013-004 — QA+IMPLEMENT+DOCUMENT: blue-green runbook

A: "Implement the traffic-shifting automation, document the rollback runbook, and get QA sign-off."
  "implement" → IMPLEMENT | "document" → DOCUMENT | "rollback runbook" → DOCUMENT + gap | "QA sign-off" → QA
  CompiledPredicateIds: ["QA", "IMPLEMENT", "DOCUMENT"]

---

## B28-compiler-013-005 — QA alone: blue-green acceptance gate

A: "QA review the blue-green deployment criteria before releasing the PaymentService."
  "QA review" → QA | "deployment criteria" → gap | "releasing" → gap
  CompiledPredicateIds: ["QA"]
