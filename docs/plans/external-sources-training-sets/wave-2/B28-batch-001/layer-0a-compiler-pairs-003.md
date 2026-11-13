# Layer 0-A — Compiler Pairs (003)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-003-001 — QA: regression gate

A: "Run QA regression gate on the CheckoutService before the hotfix release."
  "QA regression gate" → QA | "run" → gap (alone) | "hotfix release" → gap
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-003-002 — QA+DOCUMENT: QA report

A: "Conduct QA sign-off and document the test results for the release."
  "QA sign-off" → QA | "document the test results" → DOCUMENT
  CompiledPredicateIds: ["QA", "DOCUMENT"]

---

## B28-compiler-003-003 — QA+TEST+IMPLEMENT: automated QA pipeline

A: "Build the QA pipeline, write smoke tests, and get QA approval before the v3 production push."
  "build" → IMPLEMENT | "QA pipeline" → QA + IMPLEMENT | "write smoke tests" → TEST | "QA approval" → QA
  CompiledPredicateIds: ["QA", "TEST", "IMPLEMENT"]

---

## B28-compiler-003-004 — gap: "monitor the release"

A: "Monitor the production release for the first 48 hours after deployment."
  "monitor" → gap | "deployment" → gap | "48 hours" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-003-005 — QA+SECURITY: QA on security controls

A: "QA the security controls on the AuthService and audit the session management implementation."
  "QA" → QA | "audit" → SECURITY | "session management" → SECURITY
  CompiledPredicateIds: ["QA", "SECURITY"]

