# Layer 2 — Capability Gap Extended Pairs (014)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-014 — "pair with the team on testing" — gap and TEST fire

A: "Pair with the backend team on their testing approach."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "pair with on testing approach" = collaborative learning → gap. "testing approach" = advisory topic, no write/create verb.

A: "Pair with me to write unit tests for the order processing module."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "pair with" = gap warning; "write unit tests" = direct action → TEST fires. Embedded verb rule: TEST fires.

A: "Pair with the team on the integration work."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "pair with on the integration work" = collaborative advisory → gap. "integration work" = noun phrase; no "integrate" action verb present. Contrast: "pair with to integrate" → API_CONTRACT.

A: "Pair with the infrastructure team to integrate our service with the observability platform."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "pair with" = gap warning; "integrate our service with the observability platform" → API_CONTRACT. Integrate always fires in any frame.

A: "Pair with the senior developer to implement the rate limiter and write the test suite."
B: Predicates: ["IMPLEMENT", "TEST"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "pair with" = gap warning; "implement the rate limiter" → IMPLEMENT; "write the test suite" → TEST. Both code predicates fire.
