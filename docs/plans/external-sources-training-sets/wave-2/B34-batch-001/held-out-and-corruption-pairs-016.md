# Held-Out and Corruption Pairs (016)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-016-A — Held-out: "facilitate workshop on architecture" → gap

A: "Facilitate a two-day workshop on software architecture principles for the platform team."
B: Predicates: [] | GapHandler → gap-skills-guidance-train-001
Reason: Workshop facilitation = gap. Architecture topic does not fire ARCHITECTURE.

---

## HC-016-B — Held-out: "design workshop curriculum" → gap (curriculum noun)

A: "Design the three-day architecture workshop curriculum for senior engineers."
B: Predicates: [] | GapHandler → gap-skills-guidance-train-001
Reason: "design + curriculum" = content design noun → gap (not ARCHITECTURE).

---

## HC-016-C — Held-out: "design system architecture" → ARCHITECTURE (structural noun)

A: "Design the domain service architecture for the new financial platform."
B: Predicates: ["ARCHITECTURE"] | ArchitectureFlow31 | TransitionType: documentation
Reason: "design + system/architecture" = structural noun → ARCHITECTURE fires.
Distinction: curriculum noun → gap; structural noun → ARCHITECTURE.

---

## HC-016-D — Corruption v6-02: "workshop on architecture" fires ARCHITECTURE

v6 corrupt: "Run a workshop on distributed systems architecture" → Predicates: ["ARCHITECTURE"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-02. Topic of workshop ≠ predicate. Requester's action = run a workshop = gap. Architecture as workshop topic does not fire ARCHITECTURE.
