# Held-Out and Corruption Pairs (011)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H011-001 — Held-out: QA — "QA owner"

A: "Who is the QA owner for the inventory module this sprint?"
  Expected: ["QA"]
  Held-out: "QA owner" is a QA compound noun → fires QA predicate. Interrogative fires QA normally.

---

## H011-002 — Held-out: QA+DOCUMENT — document + QA gate

A: "Write the release notes and QA sign-off before shipping."
  Expected: ["DOCUMENT", "QA"]
  Held-out: "write the release notes" → DOCUMENT; "QA sign-off" → QA. TransitionType: documentation (IMPLEMENT absent).

---

## H011-003 — Held-out: gap — "incident post-mortem"

A: "Run the post-mortem for last night's incident and assign action items."
  Expected: []
  Held-out: Post-mortem / incident review without QA abbreviation → gap. Operational incident management vocabulary.

---

## C011-001 — Corruption v6-01: QA_GATE invented

A: "Pass the QA gate before releasing."
  Correct: ["QA"]
  Corrupt: ["QA", "QA_GATE"] ← v6-01 (QA_GATE is not a valid predicate)
  Teaching: QA_GATE is not a predicate. "QA gate" fires QA only via the abbreviation present in the compound noun.

---

## C011-002 — Corruption: "QA testing" → TEST+QA (wrong — "testing" inside compound)

A: "Complete QA testing before the release."
  Correct: ["QA"]
  Corrupt: ["TEST", "QA"] ← wrong ("testing" inside "QA testing" compound noun counted as separate TEST trigger)
  Teaching: "QA testing" is one compound noun → one QA predicate. "testing" is not an independent TEST trigger when it is part of a QA compound noun. Contrast: "write tests AND QA sign-off" → two separate clauses → TEST+QA.
