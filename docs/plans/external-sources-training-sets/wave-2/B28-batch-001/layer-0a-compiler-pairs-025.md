# Layer 0-A — Compiler Pairs (025) — SmokeTestSuite
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-025-001 — "SmokeTestSuite" vocabulary

A: "Run the SmokeTestSuite before deploying to production."
  "SmokeTestSuite" → TEST (test-execution vocabulary) | "deploying" → gap
  Teaching: SmokeTestSuite is a test artifact; "run" + test noun fires TEST. "deploying" alone is gap.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-025-002 — "smoke test" vs "QA sign-off"

A: "Execute the smoke test suite and QA sign-off on the results."
  "smoke test suite" → TEST (execute + test noun) | "QA sign-off" → QA (abbreviation compound)
  Teaching: "smoke test" fires TEST; "QA sign-off" fires QA. Two predicates.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-025-003 — "build" + smoke test

A: "Build the smoke test harness and run it against the release candidate."
  "build" → IMPLEMENT (build verb fires IMPLEMENT) | "smoke test harness" → TEST (test noun)
  "release candidate" → gap
  Teaching: "build" fires IMPLEMENT; "smoke test harness" fires TEST.
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]

---

## B28-compiler-025-004 — "document the smoke test criteria"

A: "Document the smoke test criteria and QA approve the suite."
  "document" → DOCUMENT (document verb) | "criteria" → DOCUMENT (criteria rule — dedup) | "QA approve" → QA
  Teaching: Both "document" verb and "criteria" noun fire DOCUMENT; deduplicated to one. QA fires on abbreviation.
  CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## B28-compiler-025-005 — Pure gap: "trigger smoke tests"

A: "Trigger the smoke test pipeline after the deployment completes."
  "trigger" → gap | "deployment" → gap | "completes" → gap
  Teaching: "trigger" is execution vocabulary; alone it is gap. "deployment" is gap. No predicates fire.
  CompiledPredicateIds: [] → gap (knowledge_edge)
