# Layer 0-C — Knowledge Edge Pairs (019)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E019-001 — GAP: "fraud detection check" fires nothing

A:
  user_request: "Do a fraud detection check on the FraudScoreCalculator scoring pipeline."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "do a" — generic action; no predicate
    "fraud detection check" — "check" is gap; "fraud detection" is domain (not SECURITY trigger without security vocabulary)
    Result: zero predicates → knowledge_edge
  Note: "fraud" alone does not fire SECURITY. SECURITY fires on: security, auth, JWT, token, encryption, authentication, authorization, vulnerability, hashing, signing. "Fraud" is a domain noun, not security vocabulary.

---

## E019-002 — EDGE: "run regression tests" does NOT fire TEST

A:
  user_request: "Run regression tests for the FraudScoreCalculator nightly."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "run" — execution verb; NOT TEST
    "regression tests" — object of "run"; does not promote "run" into TEST
    "nightly" — temporal qualifier; gap
    Result: zero predicates → knowledge_edge
  Teaching: "run regression tests" ≠ TEST. Same "run ≠ TEST" boundary applies regardless of what kind of tests are being run. To fire TEST: "Create regression tests..." or "Write regression tests...".
