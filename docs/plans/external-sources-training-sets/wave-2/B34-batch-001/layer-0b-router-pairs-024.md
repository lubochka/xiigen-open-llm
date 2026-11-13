# Layer 0-B — Router Pairs (024)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-024-001 — Complete guidance vocabulary routing reference

All guidance verbs → GapHandler (no predicate fires from guidance verb itself):
mentor → gap-skills-guidance-mentor-001
coach → gap-skills-guidance-mentor-001
advise → gap-skills-guidance-mentor-001
facilitate → gap-skills-guidance-mentor-001
pair-with → gap-skills-guidance-mentor-001
shadow → gap-skills-guidance-mentor-001
walk-through → gap-skills-guidance-mentor-001 (or onboard-001 if new-hire context)
teach → gap-skills-guidance-train-001
train (human) → gap-skills-guidance-train-001
explain → gap-skills-guidance-train-001
upskill → gap-skills-guidance-train-001
workshop (verb/event) → gap-skills-guidance-train-001
onboard → gap-skills-guidance-onboard-001
ramp-up → gap-skills-guidance-onboard-001

---

## B34-router-024-002 — Gap warning vs full gap routing decision

Full gap (GapHandler invoked + SCHEDULE_CAPABILITY_LEARNING emitted):
- Predicate set = []
- All verbs in request are guidance vocabulary

Gap warning only (GapHandler NOT invoked):
- At least one code-action predicate fires
- Guidance vocabulary present as frame/context
- Gap warning appended to flow result, not a separate route

---

## B34-router-024-003 — "teach to integrate" → gap (infinitive exception to "integrate always fires")

A: "Teach the team to integrate third-party services into our platform."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "teach to integrate" = "teach to X" pattern → gap. The infinitive "to integrate" is what others will learn. "integrate" in infinitive form within "teach to X" does not fire API_CONTRACT.
Note: "integrate" always fires API_CONTRACT when used as direct action or participial ("integrating"). "teach to integrate" is the sole exception.

---

## B34-router-024-004 — Summary: when does "integrate" NOT fire API_CONTRACT?

Only in "teach to integrate" / "learn to integrate" / "train to integrate" pattern (infinitive as learning target).
In all other constructions: "integrate", "integrating", "integration of [system]" → API_CONTRACT fires.

---

## B34-router-024-005 — TransitionType decisions for embedded-verb guidance requests

| Predicates fired | TransitionType |
|----------------|----------------|
| [] (gap only) | none |
| [TEST] | documentation |
| [DOCUMENT] | documentation |
| [ARCHITECTURE] | documentation |
| [API_CONTRACT] | documentation |
| [IMPLEMENT] | feature_addition |
| [IMPLEMENT + any] | feature_addition |
| [refactor embedded] | refactoring |
