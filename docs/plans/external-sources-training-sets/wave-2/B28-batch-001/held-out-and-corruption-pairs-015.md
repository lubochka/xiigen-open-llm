# Held-Out and Corruption Pairs (015)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H015-001 — Held-out: QA+IMPLEMENT — rollback automation

A: "Build the automated rollback service and QA the trigger before the deployment."
  Expected: ["QA", "IMPLEMENT"]
  Held-out: "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "QA" → QA; "deployment" → gap. feature_addition.

---

## H015-002 — Held-out: gap — rollback execution

A: "Roll back the PaymentService to version 3.1."
  Expected: []
  Held-out: "roll back" is gap vocabulary. Rollback execution → gap. knowledge_edge.

---

## H015-003 — Held-out: QA+IMPLEMENT+TEST — rollback with verify

A: "Implement the rollback triggers, verify state restoration, and QA sign-off."
  Expected: ["QA", "IMPLEMENT", "TEST"]
  Held-out: "implement" → IMPLEMENT; "verify" → TEST (regardless of object); "QA sign-off" → QA. feature_addition.

---

## C015-001 — Corruption: "roll back" incorrectly fires IMPLEMENT

A: "Roll back the service to the previous release."
  Correct: [] — gap
  Corrupt: ["IMPLEMENT"] ← wrong (rollback execution treated as implementation)
  Teaching: "roll back" is gap execution vocabulary. It does not fire IMPLEMENT.

---

## C015-002 — Corruption: FAIL verdict normalized to PASS

A: RollbackService QA gate — restore time exceeds 30s threshold.
  Correct: FAIL — critical defect: restore time 45s > threshold 30s
  Corrupt: PASS ← wrong (FAIL normalized)
  Teaching: QA gate FAIL must not be normalized. Models must report FAIL when gate criteria are unmet.
