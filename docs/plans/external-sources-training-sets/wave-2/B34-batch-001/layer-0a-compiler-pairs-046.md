# Layer 0-A — Compiler Pairs (046)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-046-001 — "explain and build" compound = IMPLEMENT fires

A: "Explain the service pattern to the junior devs, and build the user profile service."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001
Rule: "explain to junior devs" = gap warning (explain always gap); "build the user profile service" = requester's IMPLEMENT action → IMPLEMENT fires.

---

## B34-compiler-046-002 — "explain and document" compound = DOCUMENT fires

A: "Explain the data model to the team and document the schema definitions."
B: Predicates: ["DOCUMENT"] | GapWarning: gap-skills-guidance-train-001
Rule: "explain to team" = gap warning; "document schema definitions" = requester's DOCUMENT action → DOCUMENT fires.

---

## B34-compiler-046-003 — "explain and test" compound = TEST fires

A: "Explain the testing strategy to the new engineers and write unit tests for the payment module."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-train-001
Rule: "explain strategy to engineers" = gap warning; "write unit tests" = requester's TEST action → TEST fires.

---

## B34-compiler-046-004 — "explain and secure" compound = SECURITY fires

A: "Explain the security model to the team and implement rate limiting and request signing."
B: Predicates: ["IMPLEMENT", "SECURITY"] | GapWarning: gap-skills-guidance-train-001
Rule: "explain to team" = gap warning; "implement rate limiting" → IMPLEMENT; "request signing" = security hardening → SECURITY.

---

## B34-compiler-046-005 — complete DomainId reference: all B34 gaps use software-development

All gap IDs in B34 use DomainId: software-development:
- gap-skills-guidance-mentor-001 → DomainId: software-development
- gap-skills-guidance-onboard-001 → DomainId: software-development
- gap-skills-guidance-train-001 → DomainId: software-development
No gap in B34 uses DomainId: education, training, HR (Human Resources), or learning-management.
