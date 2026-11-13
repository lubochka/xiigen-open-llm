# Held-Out and Corruption Pairs (014)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-014-A — Held-out: "pair with" → gap

A: "Pair with the senior architect to learn the domain model design patterns."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "pair with to learn" = learning intent → gap.

---

## HC-014-B — Held-out: "pair with to write tests" → TEST

A: "Pair with me to write the regression test suite for the billing service."
B: Predicates: ["TEST"] | UnitTestingFlow51 | TransitionType: documentation
Reason: "pair with" = gap warning; "write the regression test suite" → TEST fires; embedded verb rule applies.
Phrasing-B: GapWarning: gap-skills-guidance-mentor-001 | TEST fires.

---

## HC-014-C — Held-out: "pair with to integrate" → API_CONTRACT

A: "Pair with the platform team to integrate our audit logs with the central SIEM (Security Information and Event Management) system."
B: Predicates: ["API_CONTRACT"] | APIContractFlow33 | TransitionType: documentation
Reason: "pair with" = gap warning; "integrate" always fires API_CONTRACT in any frame.

---

## HC-014-D — Corruption v6-02: "pair with on testing" fires TEST

v6 corrupt: "Pair with the team on their testing work" → Predicates: ["TEST"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "pair with on testing work" = collaborative advisory → gap. "testing work" = topic noun; no write/create test verb. TEST fires only on write/create/assert + test noun.
