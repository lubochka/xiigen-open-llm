# Layer 2 — Capability Gap Extended Pairs (024)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-024 — Complete B34 capability-gap-extended summary

A: "Mentor the junior developers on clean code and architectural patterns."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Rule demonstrated: mentor = guidance vocabulary → gap always.

A: "Explain the microservices architecture to the new backend team."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Rule demonstrated: explain = always gap, regardless of topic (even architecture topic).

A: "Guide the engineers through writing integration tests for the payment service."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Rule demonstrated: embedded verb rule — "writing tests" fires TEST inside guidance frame; guidance becomes gap warning.

A: "Walk through integrating the SSO (Single Sign-On) provider with our developer portal."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Rule demonstrated: integrate always fires API_CONTRACT even inside guidance frame; walk-through = gap warning.

A: "Teach the team to integrate third-party APIs following our enterprise patterns."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Rule demonstrated: "teach to X" infinitive exception — "teach to integrate" → gap. Infinitive "to integrate" = learning target, not requester's action.

A: "Coach through building the service, advise on trade-offs, and guide through writing the E2E (End-to-End) test suite."
B: Predicates: ["IMPLEMENT", "TEST"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Rule demonstrated: multiple guidance verbs become gap warnings; embedded IMPLEMENT (build) and TEST (writing E2E tests) both fire; GapHandler suppressed.
