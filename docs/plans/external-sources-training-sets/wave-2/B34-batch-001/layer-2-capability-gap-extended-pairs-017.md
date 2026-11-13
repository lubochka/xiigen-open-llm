# Layer 2 — Capability Gap Extended Pairs (017)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-017 — "advise on best practices" — gap

A: "Advise the development team on best practices for API (Application Programming Interface) versioning."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "advise on best practices" = advisory vocabulary → gap. API versioning = topic context, not a design or integration action.

A: "Advise the engineers on how to structure their microservices for maintainability."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "advise on how to structure" = advisory on patterns → gap. "how to" = learning/advisory intent.

A: "Advise on the database design approach and design the relational schema for the user domain."
B: Predicates: ["ARCHITECTURE"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "advise" = gap warning; "design the relational schema" = requester's design action → ARCHITECTURE fires.

A: "Advise on the security posture and harden the payment API endpoints against injection attacks."
B: Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "advise" = gap warning; "harden the payment API endpoints" = requester's security action → SECURITY fires.

A: "Advise the team to integrate the new analytics SDK (Software Development Kit) with the mobile app."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "advise the team to integrate" = recommend others perform action → gap. Requester's action = advise = gap. Team's integration is advised, not performed by requester.
