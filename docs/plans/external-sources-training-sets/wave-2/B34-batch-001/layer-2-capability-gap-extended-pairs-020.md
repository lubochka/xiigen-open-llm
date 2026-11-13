# Layer 2 — Capability Gap Extended Pairs (020)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-020 — "explain and build the auth service" — IMPLEMENT fires (build fires); explain is gap

A: "Explain the authentication flow and build the JWT (JSON Web Token) validation service."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "explain" = gap warning (train-001); "build the JWT validation service" = requester's direct action → IMPLEMENT fires. Explain does not contribute a predicate.

A: "Explain the design and implement the repository pattern for the product catalog domain."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "explain" = gap warning; "implement the repository pattern" → IMPLEMENT fires.

A: "Explain and then integrate the OAuth2 (Open Authorization 2.0) provider with the user service."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "explain" = gap warning; "integrate the OAuth2 provider" → API_CONTRACT fires. Explain does not fire API_CONTRACT on its own.

A: "Explain the approach, build the event sourcing framework, and write the tests."
B: Predicates: ["IMPLEMENT", "TEST"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "explain" = gap warning; "build the event sourcing framework" → IMPLEMENT; "write the tests" → TEST.

A: "Explain the architecture decisions and document them in the engineering handbook."
B: Predicates: ["DOCUMENT"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "explain" = gap warning (train-001); "document them in the engineering handbook" = direct documentation action → DOCUMENT fires. Explain does not fire DOCUMENT.
