# Layer 0-A — Compiler Pairs (016)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-016-001 — "workshop" facilitation → gap

A: "Run a workshop on clean code principles for the engineering team."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "run a workshop" = event facilitation vocabulary → gap. "clean code principles" = educational topic, not code action.

---

## B34-compiler-016-002 — "facilitate a workshop" → gap

A: "Facilitate a workshop on test-driven development for the backend engineers."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "facilitate" = facilitation vocabulary → gap. "test-driven development (TDD)" = methodology topic → gap (TDD methodology alone is gap; writing tests would be TEST).

---

## B34-compiler-016-003 — "workshop" + embedded IMPLEMENT

A: "Hold a workshop where participants will build and deploy a sample microservice."
Analysis: "hold a workshop" = facilitation frame; "build and deploy a sample microservice" = embedded direct actions → IMPLEMENT fires.
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "build" + "microservice" → IMPLEMENT; "hold a workshop" = gap warning; embedded verb rule applies.

---

## B34-compiler-016-004 — "design a workshop" → gap (curriculum noun)

A: "Design a two-day workshop curriculum for new engineers on our tech stack."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "design a workshop curriculum" = curriculum design → gap (content/curriculum noun, not structural noun). "design [curriculum]" → gap; "design [system/architecture]" → ARCHITECTURE. Noun type determines outcome.

---

## B34-compiler-016-005 — v6 corruption: "workshop on architecture" fires ARCHITECTURE

v6 corrupt: "Run a workshop on software architecture for senior developers" → Predicates: ["ARCHITECTURE"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-02. "run a workshop on architecture" = teaching/facilitation → gap. The topic of the workshop does not determine the predicate. Requester's action = run a workshop = gap.
Fix: Predicate = [].
