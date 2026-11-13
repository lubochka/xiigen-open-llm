# Layer 2 — Capability Gap Extended Pairs (018)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-018 — "coach through writing unit tests" — TEST fires + gap warning

A: "Coach the junior developers through writing unit tests for the inventory management module."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "coach through" = gap warning; "writing unit tests" = embedded direct action → TEST fires. Embedded verb rule applies.

A: "Coach the backend team through writing end-to-end tests for the user registration flow."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "coach through" = gap warning; "writing end-to-end tests" = embedded action → TEST fires.

A: "Coach the engineers on test coverage goals for the next sprint."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "coach on test coverage goals" = advisory vocabulary → gap. No write/create/assert verb → no TEST fires.

A: "Coach through writing tests and implementing the corresponding feature together."
B: Predicates: ["TEST", "IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "coach through" = gap warning; "writing tests" → TEST; "implementing the feature" → IMPLEMENT. Both embedded actions fire.

A: "Coach the team on the TDD (Test-Driven Development) methodology and philosophy."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "coach on TDD methodology" = methodology advisory → gap. TDD methodology alone (without write-test verb) → gap. Gap ID = train-001 (methodology = training).
