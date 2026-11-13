# Layer 2 — Capability Gap Extended Pairs (016)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-016 — "facilitate a workshop on architecture" — gap

A: "Facilitate a workshop on software architecture principles for the engineering team."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "facilitate a workshop on architecture" = educational facilitation → gap. Topic (architecture) does not fire ARCHITECTURE. Requester's action = facilitate = gap.

A: "Run a hands-on architecture workshop where the team designs the new service boundaries."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "run a workshop where the team designs" = facilitation + third-party action → gap. Team's design action does not fire ARCHITECTURE for requester. Requester's action = run a workshop = gap.

A: "Facilitate a workshop and then design the system architecture based on the outcomes."
B: Predicates: ["ARCHITECTURE"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "facilitate" = gap warning; "design the system architecture" = requester's direct action → ARCHITECTURE fires.

A: "Design a two-day workshop curriculum for the microservices migration program."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "design workshop curriculum" = content/curriculum noun → gap. Not ARCHITECTURE. Curriculum is not a structural noun.

A: "Facilitate the integration of the workshop scheduling tool with our HR (Human Resources) system."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "facilitate" = gap warning; "integration of X with Y" = API coupling action → API_CONTRACT fires. Integrate always fires.
