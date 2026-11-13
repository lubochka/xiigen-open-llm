# Layer 0-A — Compiler Pairs (049) — PostDeploymentVerification
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-049-001 — "post-deployment verification" fires TEST

A: "Run post-deployment verification on the production environment."
  "post-deployment verification" → TEST ("verification" in test context fires TEST)
  Teaching: "verification" in deployment gate context → TEST.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-049-002 — IMPLEMENT+TEST+QA for verification harness

A: "Build the post-deployment verification harness service, write tests for each verification step, and QA the verification results."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "write tests" → TEST | "verification" → TEST (dedup) | "QA" → QA
  Teaching: IMPLEMENT via build+service. TEST via test-writing verb. QA last.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## B28-compiler-049-003 — "smoke test post-deploy" fires TEST

A: "Run the smoke test suite post-deploy and verify the payment endpoint."
  "smoke test" → TEST | "post-deploy" → gap | "verify" → TEST (dedup)
  Teaching: "smoke test" fires TEST. "post-deploy" is gap. "verify" fires TEST.
  CompiledPredicateIds: ["TEST"]

---

## B28-compiler-049-004 — "deployment" alone is gap even in verification context

A: "Check the deployment health and monitor the error rate."
  "check" → gap | "deployment health" → gap | "monitor" → gap | "error rate" → gap
  Teaching: All post-deployment monitoring vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-049-005 — TEST+DOCUMENT+QA for verification with criteria

A: "Run the post-deployment verification suite, document the verification criteria, and QA sign-off on the results."
  "run the post-deployment verification suite" → TEST | "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA sign-off" → QA
  Teaching: TEST via verification suite. DOCUMENT via verb and noun. QA last.
  CompiledPredicateIds: ["TEST", "DOCUMENT", "QA"]
