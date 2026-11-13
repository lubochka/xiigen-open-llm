# Layer 3 — Teaching Unit Pairs (007)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU007-001 — Teach: SECURITY in unit test context routes to 5.2, not 5.1

Misconception: When writing unit tests for a security feature, SECURITY routes to 6.3 (audit context).
Correction: SECURITY sub-phase routing is context-dependent:
  Writing tests that assert security properties → 5.2 (security testing)
  Auditing/reviewing existing security controls → 6.3 (security audit)
  Unit-level assertion of security behavior → 5.2
Rule: SECURITY in a "write/assert/verify" context → 5.2. SECURITY in "audit/review/scan" context → 6.3.

---

## TU007-002 — Teach: "test plan" fires TEST via "test"; "create" fires IMPLEMENT

Misconception: "create a test plan" should fire TEST only because a test plan is a planning artifact, not production code.
Correction: "create" fires IMPLEMENT (creating something = feature_addition). "test plan" fires TEST via "test".
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
Rule: The action verb ("create") determines IMPLEMENT; the object noun ("test plan") determines TEST. Both fire.

---

## TU007-003 — Teach: "regression" alone is gap; "regression test" fires TEST

Misconception: "there was a regression" or "catch the regression" fires TEST.
Correction: "regression" alone is gap. TEST fires only when "test" is present: "regression test" or "regression tests".
  "Catch the regression in the PricingService." → [] (gap)
  "Write a regression test for the PricingService." → ["IMPLEMENT", "TEST"]
Rule: Compound noun "regression test" fires TEST via the "test" component. The modifier "regression" alone is insufficient.

---

## TU007-004 — Teach: "test double" is a canonical TEST trigger (canonical = listed in TEST lexicon)

Misconception: "test double" is a testing pattern term and might be gap like "mock" or "spy" alone.
Correction: "test double" is a canonical TEST predicate trigger — it is explicitly in the TEST lexicon.
  "mock" alone → gap
  "spy" alone → gap
  "stub" alone → gap
  "test double" → TEST fires
  "test spy" → TEST fires via "test"
  "test stub" → TEST fires via "test"
Rule: The word "test" in any compound fires TEST. "mock/spy/stub" alone (without "test") are gap.

---

## TU007-005 — Teach: SCHEDULE_CAPABILITY_LEARNING is emitted for all zero-predicate requests

Misconception: Gap requests are silently ignored or returned as errors.
Correction: Every zero-predicate request emits a SCHEDULE_CAPABILITY_LEARNING event to the AutonomousLearningPipeline (subproject 16). The gap is logged with its DomainId for future training.
  DomainId for B20 gaps: "software-development"
  The gap is not an error — it is training signal.
Rule: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING) is mandatory for all CompiledPredicateIds: [] results.
