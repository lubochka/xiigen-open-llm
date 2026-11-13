# Layer 0-A — Compiler Pairs (040)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-040-001 — "brief the team" = gap

A: "Brief the team on the new observability requirements before the sprint."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "brief [team] on [topic]" = knowledge transfer vocabulary → gap. Requester's action = briefing = guidance.

---

## B34-compiler-040-002 — "brief" + implement action

A: "Brief the developers on the new API (Application Programming Interface) spec and then implement the versioning layer."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "brief developers" = gap warning; "implement the versioning layer" = requester's direct IMPLEMENT action → IMPLEMENT fires.

---

## B34-compiler-040-003 — "familiarize with" = gap (onboard vocabulary)

A: "Familiarize the new backend engineers with the event-sourcing (ES) patterns used in the platform."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Rule: "familiarize [person] with [topic]" = onboarding vocabulary → gap-skills-guidance-onboard-001.

---

## B34-compiler-040-004 — "familiarize with" + test action

A: "Familiarize the QA (Quality Assurance) team with the test harness and then write the smoke test suite."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-onboard-001
Rule: "familiarize with test harness" = gap warning; "write the smoke test suite" = requester's TEST action → TEST fires.

---

## B34-compiler-040-005 — "run through" explanation = gap

A: "Run through the incident response playbook with the on-call rotation."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "run through [document/process] with [team]" = explanation/guidance vocabulary → gap.
