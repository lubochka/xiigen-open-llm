# Layer 0-A — Compiler Pairs (052) — CapacityVerification
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-052-001 — "capacity verification" fires TEST

A: "Run capacity verification for the new release before go-live."
  "capacity verification" → TEST ("verification" in gate context fires TEST)
  Teaching: "capacity verification" → TEST. "go-live" is gap.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-052-002 — IMPLEMENT+TEST+QA for capacity checker

A: "Build the capacity verification service, write load tests for peak traffic, and QA the capacity results."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "write load tests" → TEST | "QA" → QA
  Teaching: Three-predicate set. "load tests" explicitly names test type → TEST.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## B28-compiler-052-003 — "scale" and "scaling" are gap

A: "Configure auto-scaling policies and verify scaling behavior under load."
  "configure" → gap | "auto-scaling policies" → gap | "verify scaling behavior" → TEST ("verify" fires TEST)
  Teaching: "configure scaling" is gap but "verify" fires TEST.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-052-004 — "capacity plan" alone is gap

A: "Review the capacity plan and adjust resource quotas."
  "review" → gap | "capacity plan" → gap | "adjust resource quotas" → gap
  Teaching: Capacity planning review vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-052-005 — TEST+DOCUMENT+QA for capacity gate with criteria

A: "Execute the capacity verification gate, document the throughput criteria, and QA the gate results."
  "execute the capacity verification gate" → TEST | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA" → QA
  Teaching: Verification execution → TEST. DOCUMENT via verb and noun. QA last.
  CompiledPredicateIds: ["TEST", "DOCUMENT", "QA"]
