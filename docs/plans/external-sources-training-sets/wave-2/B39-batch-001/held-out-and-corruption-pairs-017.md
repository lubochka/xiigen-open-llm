# Held-Out and Corruption Pairs (017)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H017-001 — Held-out: TEST+UI — snapshot tests for component library

A: "Write snapshot tests for the button, input, and dropdown components."
  Expected: ["TEST", "UI"]
  Held-out: "write snapshot tests" → TEST; "button/input/dropdown" → UI.

---

## H017-002 — Held-out: gap — "update snapshots after spacing redesign"

A: "Update all snapshots after the spacing token redesign."
  Expected: []
  Held-out: "update" → gap (not TEST, not IMPLEMENT). "spacing token redesign" → gap. Zero predicates.

---

## H017-003 — Held-out: TEST+UI+IMPLEMENT — build screenshot harness + assertions

A: "Build the screenshot test harness and create snapshot assertions for every modal variant."
  Expected: ["TEST", "UI", "IMPLEMENT"]
  Held-out: "build" → IMPLEMENT; "create snapshot assertions" → TEST; "modal variant" → UI. Three predicates.

---

## C017-001 — Corruption v6-02: "update" → IMPLEMENT (wrong)

A: "Update all snapshots after the spacing token redesign."
  Correct: []
  Corrupt: ["IMPLEMENT"] ← v6-02 ("update" incorrectly treated as IMPLEMENT)
  Teaching: "update" is consistently gap for IMPLEMENT. The model must learn "update" ≠ "build/create/implement".

---

## C017-002 — Corruption v6-03: TEST and UI merged into one sub-phase for snapshot tests

A: "Write snapshot tests for the modal component."
  Correct: TEST → 5.3, UI → 1.5 (two separate sub-phases)
  Corrupt: TEST+UI → 5.3 only (UI routing merged into TEST routing)
  Teaching: UI always routes to 1.5 independently. Even for snapshot tests (which are TEST → 5.3), the UI predicate fires a separate routing decision to 1.5. Never merge.
