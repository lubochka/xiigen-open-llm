# Layer 2 — Capability Gap Extended Pairs (023)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-023 — Guidance vs instruction disambiguation

A: "Guide the team through the deployment checklist." (passive observation)
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "guide through checklist" = lead through documentation = guidance/explanation → gap. No deploy/build action by requester.

A: "Perform the deployment following the deployment checklist." (requester acts directly)
B: Predicates: ["IMPLEMENT"] | DomainId: software-development
Analysis: "perform the deployment" = direct execution action → IMPLEMENT (deployment = build/deliver sub-type). No guidance frame.

A: "Mentor the team on clean architecture and help implement the domain layer together."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "mentor on" = gap warning; "help implement the domain layer together" = requester participates in implementation → IMPLEMENT fires. "help implement" = requester's co-action.

A: "Advise the developers and let them implement it themselves."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "advise" = gap; "let them implement" = third-party action (directive to let others act). Requester's action = advise = gap.

A: "Coach the team through the testing process by writing the test framework setup yourself."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "coach through" = gap warning; "writing the test framework setup yourself" = requester's direct action → TEST fires. "yourself" confirms requester performs the action.
