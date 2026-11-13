# Held-Out and Corruption Pairs (005)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H005-001 — Held-out: "validate" in business validation fires TEST

Request: "Validate the CartService discount application rules before saving the order."
  Correct: CompiledPredicateIds: ["TEST"]
  Evidence: "validate" always fires TEST (business validation context is NOT an exception)
  TransitionType: feature_addition

---

## H005-002 — Corruption v6-01: "reproduce the bug" fires TEST (invention)

Corrupt output: "reproduce the bug in the CartService" → CompiledPredicateIds: ["TEST"]
Correct output: CompiledPredicateIds: []
  Corruption type: v6-01 (predicate invention — "reproduce" incorrectly fires TEST)
  Fix: "reproduce" is gap. "bug" is gap. Zero predicates. Emit SCHEDULE_CAPABILITY_LEARNING.

---

## H005-003 — Corruption v6-01: "QA" spelled out fires QA (invention)

Corrupt output: "perform quality assurance on the BillingModule" → CompiledPredicateIds: ["QA"]
Correct output: CompiledPredicateIds: []
  Corruption type: v6-01 (predicate invention — "quality assurance" spelled out incorrectly fires QA)
  Fix: QA fires ONLY on the abbreviation "QA". "quality assurance" spelled out is gap.

---

## H005-004 — Held-out: four-predicate request produces four flows

Request: "Write tests for the EncryptionService, implement AES-256 mode, document the key rotation policy, and assert no key is stored in plaintext."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST", "DOCUMENT", "SECURITY"]
  Evidence: "write tests" → TEST+IMPLEMENT; "implement AES-256" → IMPLEMENT+SECURITY; "document" → DOCUMENT; "assert" → TEST; "plaintext" → SECURITY
  SubPhases: [5.1, 4.1, 6.2, 5.2] | ExecutionMode: sequential

---

## H005-005 — Corruption v6-03: three-predicate request collapsed to single predicate

Corrupt output: "write unit tests, implement the method, and verify security" → CompiledPredicateIds: ["IMPLEMENT"]
Correct output: CompiledPredicateIds: ["IMPLEMENT", "TEST", "SECURITY"]
  Corruption type: v6-03 (multi-branch result collapse — collapses all to dominant IMPLEMENT)
  Fix: Each predicate fires independently. Compound requests accumulate predicates.
