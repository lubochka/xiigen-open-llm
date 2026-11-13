# Layer 0-A — Compiler Pairs (016)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-016-001 — QA+DOCUMENT: feature flag gate criteria

A: "Document the feature flag release criteria and QA sign-off on the gate conditions."
  "document" → DOCUMENT | "release criteria" → DOCUMENT | "QA sign-off" → QA | "gate conditions" → gap
  CompiledPredicateIds: ["QA", "DOCUMENT"]

---

## B28-compiler-016-002 — QA+IMPLEMENT: feature flag service

A: "Build the feature flag service and QA the enable/disable toggle logic."
  "build" → IMPLEMENT | "feature flag service" → IMPLEMENT | "QA" → QA | "toggle logic" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-016-003 — gap: feature flag toggle instruction

A: "Enable the dark-mode feature flag for 10% of users in production."
  "enable feature flag" → gap | "production" → gap | "10% of users" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-016-004 — QA+IMPLEMENT+TEST: feature flag validation

A: "Implement the feature flag evaluation logic, write tests for the flag resolution, and QA approve the service."
  "implement" → IMPLEMENT | "write tests" → TEST | "QA approve" → QA | "flag resolution" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST"]

---

## B28-compiler-016-005 — QA alone: feature flag acceptance gate

A: "QA gate the feature flag rollout for the RecommendationService before production."
  "QA gate" → QA | "rollout" → gap | "production" → gap | "RecommendationService" → gap
  CompiledPredicateIds: ["QA"]
