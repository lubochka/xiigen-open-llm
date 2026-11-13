# Held-Out and Corruption Pairs (007)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H007-001 — Held-out: "acceptance test" fires TEST; "sprint" vocabulary is gap

Request: "Write acceptance tests for the ProductBundling sprint stories."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "write" → IMPLEMENT; "acceptance tests" → TEST; "sprint stories" is gap (project vocabulary)
  TransitionType: feature_addition

---

## H007-002 — Corruption v6-01: "run" fires TEST (invention for wrong reason)

Corrupt output: "run the unit tests" → CompiledPredicateIds: ["TEST"] — correct result but wrong reason documented
Correct reasoning: TEST fires on "unit tests" (the object noun), NOT on "run". "run" is gap.
  Training target: the model must learn the correct attribution — TEST fires on the noun, not the verb.

---

## H007-003 — Held-out: "verify the fix" = TEST only; "fix" does NOT contribute IMPLEMENT

Request: "Verify that the fix for the PaymentAdapter race condition is correct."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "verify" → TEST; "fix" is gap for IMPLEMENT; "race condition" is domain gap
  TransitionType: feature_addition

---

## H007-004 — Corruption v6-01: "happy path" fires TEST (invention)

Corrupt output: "cover the happy path for the CheckoutService" → CompiledPredicateIds: ["TEST"]
Correct output: CompiledPredicateIds: []
  Corruption type: v6-01 (predicate invention — "happy path" incorrectly fires TEST)
  Fix: "happy path" is gap testing vocabulary. "cover" is gap. Zero predicates. Emit SCHEDULE_CAPABILITY_LEARNING.

---

## H007-005 — Held-out: "test suite" fires TEST; "review" is gap; correct result on gap verb

Request: "Review the unit test suite for the InventoryService before the release."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "review" is gap; "unit test suite" → TEST via "test" (compound: "unit" modifier, "test" trigger, "suite" extension)
  "release" is gap (release vocabulary)
