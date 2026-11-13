# Held-Out and Corruption Pairs (006)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-006-A — Held-out: "TDD" abbreviation alone → gap

A: "Introduce TDD (Test-Driven Development) to the engineering team."
B: Predicates: [] | GapHandler → gap-skills-guidance-train-001
Reason: "introduce TDD" = teaching a methodology → gap. TDD abbreviation alone ≠ TEST trigger.
Phrasing-B: Gap — methodology introduction is guidance vocabulary. TEST fires on test-writing verbs, not TDD/BDD abbreviations.

---

## HC-006-B — Held-out: "write tests TDD style" → TEST fires

A: "Write tests TDD style for the new checkout service."
B: Predicates: ["TEST"] | UnitTestingFlow51 or E2ETestingFlow53 | TransitionType: documentation
Reason: "write tests" = test-writing verb → TEST fires. "TDD style" = descriptor; test-writing verb is the trigger.
Phrasing-B: TEST fires — "write tests" is the predicate trigger; TDD is just the methodology label.

---

## HC-006-C — Corruption v6-02: "introduce TDD" fires TEST

v6 corrupt: "Introduce TDD practices to the team" → Predicates: ["TEST"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-02. "introduce TDD" = teaching vocabulary → gap. TDD abbreviation ≠ TEST predicate trigger.

---

## HC-006-D — Held-out: "run workshops" → gap

A: "Run weekly workshops on Go concurrency patterns."
B: Predicates: [] | GapHandler → gap-skills-guidance-train-001
Reason: "run workshops" = facilitation/training event vocabulary → gap.
