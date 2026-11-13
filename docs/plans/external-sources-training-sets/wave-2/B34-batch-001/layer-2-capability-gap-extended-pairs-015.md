# Layer 2 — Capability Gap Extended Pairs (015)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-015 — "walk through how the API works" — gap (explain = gap)

A: "Walk the onboarding engineers through how the internal API works end to end."
B: Predicates: [] | GapId: gap-skills-guidance-onboard-001 | DomainId: software-development
Analysis: "walk through how the API works" = explain existing behavior → gap. Onboarding context → gap-skills-guidance-onboard-001.

A: "Walk the team through the architecture of the platform we designed last quarter."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "walk through architecture" = explain past decisions → gap. ARCHITECTURE does not fire because requester is explaining, not designing.

A: "Walk through integrating the Stripe payment gateway with our checkout service."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "walk through" = gap warning; "integrating" = embedded API action → API_CONTRACT fires. Integrate always fires in any frame.

A: "Walk through building the notification service and implement it using the observer pattern."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "walk through" = gap warning; "implement it" = requester's direct action → IMPLEMENT fires.

A: "Walk through the CI/CD (Continuous Integration / Continuous Delivery) process with the new hires."
B: Predicates: [] | GapId: gap-skills-guidance-onboard-001 | DomainId: software-development
Analysis: "walk through [process]" = explain process → gap. Onboarding context ("new hires") → gap-skills-guidance-onboard-001.
