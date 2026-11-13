# Layer 2 — Capability Gap Extended Pairs (025)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-025 — "show how to" vocabulary: gap extended execution

A: "Show the backend team how to structure domain events in the event sourcing service."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED
Gap payload: {gap_id: "gap-skills-guidance-train-001", domainId: "software-development", request: "Show the backend team how to structure domain events..."}
Verdict: GAP_LOGGED

A: "Show the new interns how to submit their first pull request."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Verdict: GAP_LOGGED

A: "Show the QA (Quality Assurance) team how to write parametrized tests."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Verdict: GAP_LOGGED

A: "Show the team how to build the service, then implement the service yourself."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
GapHandler: SUPPRESSED | SCHEDULE_CAPABILITY_LEARNING: NOT EMITTED
Verdict: IMPLEMENT_APPROVED + GAP_WARNING
