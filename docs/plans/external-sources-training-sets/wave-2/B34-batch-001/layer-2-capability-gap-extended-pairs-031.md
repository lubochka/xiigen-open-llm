# Layer 2 — Capability Gap Extended Pairs (031)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-031 — "second opinion", "rubber duck", "sounding board": extended execution

A: "Get a second opinion on the multi-tenant isolation approach from the platform architects."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED

A: "Rubber duck the state machine transitions for the order fulfillment workflow."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED

A: "Use the architecture guild as a sounding board for the event mesh topology design."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED

A: "Second opinion on design, rubber duck edge cases, sounding board with guild — then architect the final topology."
B: Predicates: ["ARCHITECTURE"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: SUPPRESSED | SCHEDULE_CAPABILITY_LEARNING: NOT EMITTED | Verdict: ARCHITECTURE_APPROVED + GAP_WARNING
Rule demonstrated: three advisory verbs → single gap warning; ARCHITECTURE fires once.
