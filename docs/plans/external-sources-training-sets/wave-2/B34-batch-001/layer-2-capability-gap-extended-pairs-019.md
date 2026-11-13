# Layer 2 — Capability Gap Extended Pairs (019)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-019 — "guide integration with the payment API" — API_CONTRACT fires + gap warning

A: "Guide the payments team through integrating the payment API into the checkout workflow."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "guide through" = gap warning; "integrating the payment API" = embedded API coupling action → API_CONTRACT fires. Integrate always fires in any frame.

A: "Walk the mobile team through integrating push notifications using the Firebase API."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "walk through" = gap warning; "integrating push notifications" = API action → API_CONTRACT fires.

A: "Facilitate the integration of the new analytics provider with the web application."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "facilitate" = gap warning; "integration of provider with app" = API coupling → API_CONTRACT fires.

A: "Show the engineers how to integrate the Twilio SMS (Short Message Service) API for two-factor authentication."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "show how to" = guidance frame; embedded "integrate the Twilio SMS API" → API_CONTRACT fires. "show" = gap warning (train-001).

A: "Teach the team to integrate third-party services following our enterprise patterns."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "teach to integrate" = "teach to X" infinitive exception → gap. Integrate blocked from firing in direct teach-to pattern. Gap-skills-guidance-train-001.
