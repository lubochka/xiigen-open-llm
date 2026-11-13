# Layer 0-C — Knowledge Edge Pairs (020)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E020-001 — GAP: "password policy review" fires nothing

A:
  user_request: "Do a password policy review for the PasswordValidator to check it meets OWASP (Open Web Application Security Project) guidelines."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "do a" — generic action; no predicate
    "review" — reading/analysis verb; no predicate (review ≠ test/verify/assert)
    "check" — NOT TEST; informal inspection; gap
    "OWASP" — domain security framework acronym; does not fire SECURITY predicate in isolation
    Result: zero predicates → knowledge_edge
  Note: "OWASP" is a domain acronym, not SECURITY vocabulary. SECURITY fires on security-action words, not on compliance framework names.

---

## E020-002 — EDGE: "testable" adjective does NOT fire TEST

A:
  user_request: "Ensure the PasswordValidator is testable by extracting its complexity rules into a separate class."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  TransitionType: refactoring
  PhaseId: 4
  SubPhaseId: 4.1
  SourceStatus: ready
  Predicate evidence:
    "testable" — adjective; does NOT fire TEST (only the noun/verb/gerund forms of "test" fire TEST)
    "extracting" — IMPLEMENT fires on "extract" (refactoring action)
    "separate class" — domain architectural term; gap
    Result: ["IMPLEMENT"] — refactoring classification because "extracting" is a refactoring action
  TransitionType: refactoring (extracting code = restructuring, not adding new features)
  Critical boundary: "testable" (adjective) ≠ TEST (predicate). Only test/tests/testing/tested/test-driven fire TEST.
