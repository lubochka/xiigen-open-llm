# Layer 0-A — Compiler Pairs (014)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-014-001 — QA+IMPLEMENT: canary release service

A: "Build the canary release controller that routes 5% of traffic and QA the routing logic."
  "build" → IMPLEMENT | "canary release controller" → IMPLEMENT | "QA" → QA | "routes traffic" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-014-002 — QA+TEST+IMPLEMENT: canary validation

A: "Implement canary traffic splitting, write acceptance tests for error-rate thresholds, and QA the rollout."
  "implement" → IMPLEMENT | "write acceptance tests" → TEST | "QA" → QA | "rollout" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST"]

---

## B28-compiler-014-003 — gap: canary percentage adjustment

A: "Increase canary traffic from 5% to 20% and monitor error rates."
  "increase traffic" → gap | "monitor error rates" → gap | "canary" alone → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-014-004 — QA+DOCUMENT: canary release criteria

A: "Document the canary release criteria and QA sign-off before full rollout."
  "document" → DOCUMENT | "release criteria" → DOCUMENT | "QA sign-off" → QA | "rollout" → gap
  CompiledPredicateIds: ["QA", "DOCUMENT"]

---

## B28-compiler-014-005 — QA+IMPLEMENT+DOCUMENT+TEST: full canary lifecycle

A: "Build the canary pipeline, write integration tests, document the promotion criteria, and QA approve the release."
  "build" → IMPLEMENT | "write integration tests" → TEST | "document" → DOCUMENT | "QA approve" → QA
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"]
