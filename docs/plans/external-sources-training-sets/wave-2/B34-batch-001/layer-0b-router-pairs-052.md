# Layer 0-B — Router Pairs (052)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-052-001 — v6 corruption routing: incorrect flow selected for advisory vocabulary

v6 corrupt routing: "Consult with architect on design" → ArchitectureFlow31
Correct routing: → GapHandler → gap-skills-guidance-mentor-001
Error: v6-02 topic-predicate confusion. Advisory vocabulary never selects topic-associated flow alone.

---

## B34-router-052-002 — v6 corruption routing: gap warning lost

v6 corrupt routing: "Rubber duck design, then implement service" → ImplementationFlow41 (no gap warning)
Correct routing: → ImplementationFlow41 + CapabilityGapWarning: gap-skills-guidance-mentor-001
Error: v6-03 predicate loss. Advisory vocabulary alongside code action = gap warning must be appended.

---

## B34-router-052-003 — v6 corruption routing: SCHEDULE_CAPABILITY_LEARNING emitted multiple times

v6 corrupt: "Consult, seek advice, and rubber duck the design" → SCHEDULE_CAPABILITY_LEARNING × 3
Correct: → GapHandler → gap-skills-guidance-mentor-001 + SCHEDULE_CAPABILITY_LEARNING × 1
Error: v6-04 duplicate emission. Multiple guidance verbs = single SCHEDULE_CAPABILITY_LEARNING emission only.

---

## B34-router-052-004 — v6 corruption routing: GapHandler invoked alongside code predicate

v6 corrupt: "Explain design to team and implement service" → GapHandler + ImplementationFlow41 (both invoked)
Correct: → ImplementationFlow41 + CapabilityGapWarning: gap-skills-guidance-train-001 (GapHandler suppressed)
Error: v6-05 dual invocation. GapHandler SUPPRESSED when any predicate fires. Gap = warning only.

---

## B34-router-052-005 — routing correctness checkpoint (Round 3 complete)

Correct B34 routing invariants:
1. GapHandler invoked ONLY when predicate set = [].
2. Code predicates and GapHandler never fire simultaneously.
3. Advisory vocabulary never selects topic-associated flow alone.
4. SCHEDULE_CAPABILITY_LEARNING emitted exactly once per full-gap request.
5. CapabilityGapWarning appended (not GapHandler invoked) when code predicates fire alongside guidance vocabulary.
