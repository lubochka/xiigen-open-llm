# Layer 0-A — Compiler Pairs (010)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: compiler

---

## P010-001 — TEST+IMPLEMENT: code + tests together

A: "Develop the WishlistService and write unit tests for add, remove, and merge operations."
  "develop" → IMPLEMENT; "write" → IMPLEMENT; "unit tests" → TEST; "WishlistService" — no predicate
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]

---

## P010-002 — TEST: "test" + negative path focus

A: "Test that the PromotionService throws a validation failure when a promotion code has expired."
  "test" → TEST; "validation failure" — domain result; "throws" — implementation detail; no predicate
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Note: "throws" describes runtime behavior but does not fire a predicate.

---

## P010-003 — TEST+DOCUMENT: spec document

A: "Write the test specification document for the ReturnPolicyService covering all return window rules."
  "write" → IMPLEMENT; "test specification" → TEST via "test"; "document" is part of "specification document" — does DOCUMENT fire?
  "specification document" → DOCUMENT fires on "specification" (specification = written spec = DOCUMENT);
  "test specification" also → TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "DOCUMENT"] | SubPhases: [4.1, 5.1, 6.2]

---

## P010-004 — TEST only: "test" + performance-adjacent

A: "Test that the ProductSearchService returns results within 200 milliseconds for a dataset of 100,000 products."
  "test" → TEST; "200 milliseconds" — performance metric; gap; "100,000 products" — scale; gap
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Gap warnings: performance threshold testing vocabulary (latency, throughput, milliseconds) is gap.

---

## P010-005 — TEST+IMPLEMENT+SECURITY: full auth unit test task

A: "Implement OAuth token validation tests and write test cases that verify the service rejects tampered JWTs."
  "implement" → IMPLEMENT; "OAuth token validation tests" → TEST via "tests"; "validation" → TEST;
  "write" → IMPLEMENT; "test cases" → TEST; "verify" → TEST; "tampered JWTs" → SECURITY (JWT + tampered);
  "OAuth" → SECURITY (OAuth is auth protocol)
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "SECURITY"] | SubPhases: [4.1, 5.1, 5.2]
