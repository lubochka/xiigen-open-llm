# Held-Out and Corruption Pairs (020)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H020-001 — Held-out: QA alone — A/B testing gate

A: "QA review the A/B testing release for the SearchService before go-live."
  Expected: ["QA"]
  Held-out: "QA review" → QA; "A/B testing" → gap; "go-live" → gap. QA alone → no TransitionType.

---

## H020-002 — Held-out: gap — A/B configuration

A: "Configure the A/B test with a 50/50 split for the checkout flow."
  Expected: []
  Held-out: "configure A/B test" → gap; "50/50 split" → gap. knowledge_edge.

---

## H020-003 — Held-out: QA+TEST+DOCUMENT — A/B validation with criteria

A: "Verify the A/B variant assignment, document the statistical significance criteria, and QA sign-off."
  Expected: ["QA", "TEST", "DOCUMENT"]
  Held-out: "verify" → TEST; "document" → DOCUMENT; "criteria" → DOCUMENT (dedup); "QA sign-off" → QA. documentation (no IMPLEMENT).

---

## C020-001 — Corruption: "A/B" fires an invented predicate

A: "Run the A/B test for the checkout experiment."
  Correct: [] — gap
  Corrupt: ["AB_TESTING"] ← invented predicate (wrong)
  Teaching: "A/B" is gap vocabulary. No AB_TESTING predicate exists.

---

## C020-002 — Corruption: PASS_WITH_WARNINGS normalized to PASS for A/B gate

A: A/B gate with cross-service metric leakage (non-critical warning).
  Correct: PASS_WITH_WARNINGS
  Corrupt: PASS ← wrong (warning suppressed)
  Teaching: PASS_WITH_WARNINGS must not be normalized to PASS. Non-critical issues must be reported.
