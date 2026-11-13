# Layer 2 — Capability Gap Extended Pairs (027)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-027 — "introduce" disambiguation: extended execution

A: "Introduce the junior developers to the event-driven architecture principles."
B: Predicates: [] | GapId: gap-skills-guidance-onboard-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED

A: "Introduce server-side rendering into the Next.js (Next.js framework) application."
B: Predicates: ["IMPLEMENT"] | DomainId: software-development
GapHandler: NOT INVOKED | SCHEDULE_CAPABILITY_LEARNING: NOT EMITTED | Verdict: IMPLEMENT_APPROVED
Rule demonstrated: introduce [feature into system] = IMPLEMENT; introduce [person to topic] = gap.

A: "Introduce the new engineers to GraphQL (Graph Query Language) and introduce GraphQL support into the API (Application Programming Interface) gateway."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-onboard-001 | DomainId: software-development
GapHandler: SUPPRESSED | Verdict: IMPLEMENT_APPROVED + GAP_WARNING
Rule demonstrated: both forms in same request → IMPLEMENT fires; introduce [person] = gap warning.

A: "Introduce the platform team to the new deployment tooling."
B: Predicates: [] | GapId: gap-skills-guidance-onboard-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED
