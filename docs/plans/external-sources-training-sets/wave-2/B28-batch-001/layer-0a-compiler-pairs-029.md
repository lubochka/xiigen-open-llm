# Layer 0-A — Compiler Pairs (029) — LoadTestGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-029-001 — "LoadTestGate" vocabulary

A: "All services must clear the LoadTestGate before production cutover."
  "LoadTestGate" → TEST (load test = test artifact) | "cutover" → gap | "production" → gap
  Teaching: LoadTestGate fires TEST. "cutover" and "production" alone are gap vocabulary.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-029-002 — "run load tests" + "QA gate"

A: "Run load tests at 10k RPS (requests per second) and QA gate the deployment."
  "run load tests" → TEST | "QA gate" → QA (abbreviation compound) | "deployment" → gap
  Teaching: "load tests" fires TEST; "QA gate" fires QA on abbreviation; "deployment" alone is gap.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-029-003 — "build load test harness"

A: "Build the load test harness and implement the autoscaling service."
  "build" → IMPLEMENT | "load test harness" → TEST | "implement" → IMPLEMENT (dedup) | "service" → IMPLEMENT (dedup)
  Teaching: "build" fires IMPLEMENT; "load test harness" fires TEST; all IMPLEMENT triggers deduplicated.
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]

---

## B28-compiler-029-004 — "document load test criteria"

A: "Document the load test criteria and secure the endpoint under load."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "secure" → SECURITY
  Teaching: DOCUMENT fires twice (deduplicated). "secure" verb fires SECURITY.
  CompiledPredicateIds: ["DOCUMENT", "SECURITY"]

---

## B28-compiler-029-005 — Pure gap: "schedule load test run"

A: "Schedule the LoadTestGate run at 2 AM before the cutover window."
  "schedule" → gap | "cutover window" → gap
  Teaching: "schedule" is execution planning vocabulary; pure gap.
  CompiledPredicateIds: [] → gap (knowledge_edge)
