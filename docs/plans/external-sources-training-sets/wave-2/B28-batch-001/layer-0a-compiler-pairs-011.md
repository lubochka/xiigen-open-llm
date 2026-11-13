# Layer 0-A — Compiler Pairs (011)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-011-001 — QA: "QA testing" fires QA

A: "Perform QA testing on the NotificationService before the weekly release."
  "QA testing" → QA | "perform" → gap (alone)
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-011-002 — QA+ARCHITECTURE: QA framework design

A: "Design the QA testing framework architecture and define the release gate criteria."
  "design" → ARCHITECTURE | "QA testing framework architecture" → QA + ARCHITECTURE | "QA" → QA | "release gate criteria" → QA
  CompiledPredicateIds: ["QA", "ARCHITECTURE"]

---

## B28-compiler-011-003 — gap: "rollout"

A: "Rollout the v3.0 feature flag to 10% of users."
  "rollout" → gap | "feature flag" → gap | "10% of users" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-011-004 — QA+SECURITY: QA the security release

A: "QA the security patch for the CVE (Common Vulnerabilities and Exposures) fix before emergency production push."
  "QA" → QA | "security patch" → SECURITY | "CVE fix" → SECURITY
  CompiledPredicateIds: ["QA", "SECURITY"]

---

## B28-compiler-011-005 — QA+TEST+IMPLEMENT: build + test + QA

A: "Write smoke tests, implement the QA reporting module, and get QA sign-off before v2.5 launch."
  "write smoke tests" → TEST | "implement the QA reporting module" → IMPLEMENT + QA | "QA sign-off" → QA
  CompiledPredicateIds: ["QA", "TEST", "IMPLEMENT"]

