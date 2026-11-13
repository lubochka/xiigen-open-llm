# Held-Out and Corruption Pairs (023)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H023-001 — Held-out: QA+IMPLEMENT — hotfix service

A: "Build the emergency hotfix deployment service and QA the patch before go-live."
  Expected: ["QA", "IMPLEMENT"]
  Held-out: "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "QA" → QA; "patch" → gap; "go-live" → gap. feature_addition.

---

## H023-002 — Held-out: gap — hotfix deployment

A: "Deploy the authentication hotfix to production immediately."
  Expected: []
  Held-out: "deploy hotfix" → gap; "production" → gap. knowledge_edge.

---

## H023-003 — Held-out: QA+IMPLEMENT+TEST+SECURITY — secure hotfix

A: "Build the hotfix, write regression tests, validate the security patch, and QA approve."
  Expected: ["QA", "IMPLEMENT", "TEST", "SECURITY"]
  Held-out: "build" → IMPLEMENT; "write regression tests" → TEST; "validate" → TEST (dedup) + "security patch" → SECURITY; "QA approve" → QA. feature_addition.

---

## C023-001 — Corruption: "emergency" fires an invented predicate

A: "Handle the emergency deployment request."
  Correct: [] — gap
  Corrupt: ["EMERGENCY"] ← invented predicate (wrong)
  Teaching: "emergency" alone is gap vocabulary. No EMERGENCY predicate exists.

---

## C023-002 — Corruption: hotfix FAIL verdict normalized to PASS

A: Hotfix QA gate — regression test fails for authentication logout flow.
  Correct: FAIL — critical defect: authentication logout regression
  Corrupt: PASS ← wrong (FAIL normalized for "emergency" context)
  Teaching: Emergency/hotfix context does not override FAIL verdict. QA gate FAIL must be reported even for emergency releases.
