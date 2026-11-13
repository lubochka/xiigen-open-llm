# Layer 2 — Capability Gap Extended Pairs (013)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-013 — "shadow me on the deployment" — gap

A: "Shadow me on the next production deployment so you understand our release process."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "shadow" = observation/learning vocabulary → gap. "production deployment" = context noun; no deploy/build code verb by requester. Requester's action = shadow = gap.

A: "Shadow the senior SRE (Site Reliability Engineer) on the on-call rotation to learn incident response."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "shadow the senior SRE" = guided observation → gap. Learning intent confirmed by "to learn incident response."

A: "Shadow the architect while she designs the new event-driven system."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: Third-party action: "she designs" = architect's action. Requester's action = shadow = gap. Embedded verb rule: only requester's actions fire predicates.

A: "Shadow the lead engineer and then implement the same caching pattern in the product service."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "shadow" = gap warning; "implement the same caching pattern" = requester's direct action → IMPLEMENT fires. Embedded verb rule: code action fires.

A: "Shadow the QA (Quality Assurance) team and write regression tests for the payment flow."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "shadow" = gap warning; "write regression tests" = requester's direct action → TEST fires.
