# Layer 0-A — Compiler Pairs (024)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-024-001 — Complete guidance vocabulary reference

| Guidance verb | Always gap? | Fires predicate? | Notes |
|--------------|-------------|-----------------|-------|
| mentor | yes | never | gap-skills-guidance-mentor-001 |
| coach | yes | never | gap-skills-guidance-mentor-001 |
| teach | yes | never | gap-skills-guidance-train-001 |
| train (human) | yes | never | gap-skills-guidance-train-001 |
| guide | yes | never | gap-skills-guidance-mentor-001 |
| explain | yes | never | always gap, no matter the topic |
| onboard | yes | never | gap-skills-guidance-onboard-001 |
| shadow | yes | never | gap-skills-guidance-mentor-001 |
| upskill | yes | never | gap-skills-guidance-train-001 |
| advise | yes | never | gap-skills-guidance-mentor-001 |
| facilitate | yes | never | gap-skills-guidance-mentor-001 |
| pair-with | yes | never | gap-skills-guidance-mentor-001 |
| walk-through | yes | never | gap-skills-guidance-mentor-001 |
| workshop (verb) | yes | never | gap-skills-guidance-train-001 |
| review-with | yes | never | gap-skills-guidance-mentor-001 |

---

## B34-compiler-024-002 — Embedded verb rule: complete reference

When a guidance verb frames an embedded code-action verb:
- The guidance verb itself never fires a predicate.
- The embedded code-action verb DOES fire its predicate if the requester performs the action.
- The guidance verb becomes a gap warning (not a full GapHandler invocation).
- GapHandler is invoked only when predicate set = [].

| Frame | Embedded action | Result |
|-------|----------------|--------|
| "guide through writing tests" | write tests → TEST | TEST + gap warning |
| "coach through building service" | build service → IMPLEMENT | IMPLEMENT + gap warning |
| "walk through integrating API" | integrate → API_CONTRACT | API_CONTRACT + gap warning |
| "mentor on architecture" | no code verb | [] + GapHandler |
| "explain the auth flow" | no code verb | [] + GapHandler |

---

## B34-compiler-024-003 — "teach to X" disambiguation

"teach to X" → gap (the infinitive X is what others will learn; requester's action = teach = gap)
"teach to implement" → gap (not IMPLEMENT)
"teach to secure" → gap (not SECURITY)
"teach to integrate" → gap (not API_CONTRACT; "integrate" embedded in "teach to" → gap exception)

Exception: "integrate" normally always fires. But "teach to integrate" = gap because the infinitive is the learning target. The embedded verb rule applies to participial forms ("integrating"), not infinitive "to integrate" in a "teach to" construction.

---

## B34-compiler-024-004 — "explain" is always gap: no exceptions

"explain the architecture" → gap (not ARCHITECTURE)
"explain the security model" → gap (not SECURITY)
"explain how to write tests" → gap (not TEST)
"explain the API contract" → gap (not API_CONTRACT)
"explain and document" → DOCUMENT fires from "document", not from "explain"

Rule: explain = teaching → always gap. The topic after "explain" never activates any predicate.

---

## B34-compiler-024-005 — Complete three-gap-ID assignment rules

| Gap ID | Triggers |
|--------|---------|
| gap-skills-guidance-mentor-001 | mentor, coach, advise, pair-with, shadow, review-with, facilitate, walk-through (general mentoring sense) |
| gap-skills-guidance-onboard-001 | onboard, orientation, buddy-system, ramp-up, walkthrough (new-hire facilitation sense) |
| gap-skills-guidance-train-001 | teach, train (human), explain, demonstrate, upskill, workshop, bootcamp, TDD methodology |

Priority: gap-skills-guidance-mentor-001 is default when verb is ambiguous between mentor and train categories.
DomainId: software-development for all three gap IDs, always.
