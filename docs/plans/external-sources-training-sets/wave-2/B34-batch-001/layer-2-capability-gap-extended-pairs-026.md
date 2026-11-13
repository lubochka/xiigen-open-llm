# Layer 2 — Capability Gap Extended Pairs (026)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-026 — "demonstrate" vocabulary: gap extended execution

A: "Demonstrate the chaos engineering process to the SRE (Site Reliability Engineering) team."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED

A: "Demonstrate how to configure the ingress controller to the platform engineers."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED

A: "Demonstrate the refactoring techniques and refactor the legacy payment controller."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
GapHandler: SUPPRESSED | SCHEDULE_CAPABILITY_LEARNING: NOT EMITTED | Verdict: IMPLEMENT_APPROVED + GAP_WARNING

A: "Demonstrate the testing framework and write 20 unit tests for the invoice generator."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
GapHandler: SUPPRESSED | SCHEDULE_CAPABILITY_LEARNING: NOT EMITTED | Verdict: TEST_PASS + GAP_WARNING
