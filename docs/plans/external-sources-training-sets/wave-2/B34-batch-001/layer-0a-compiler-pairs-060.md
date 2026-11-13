# Layer 0-A — Compiler Pairs (060)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-060-001 — Round 3 complete guidance vocabulary summary

Round 3 additions (037–060):
- show how to → gap-skills-guidance-train-001
- demonstrate (to/for person) → gap-skills-guidance-train-001
- demonstrate how to → gap-skills-guidance-train-001
- introduce [person] to → gap-skills-guidance-onboard-001 or train-001
- introduce [feature] into → IMPLEMENT (not gap)
- brief [team] on → gap-skills-guidance-mentor-001
- familiarize [person] with → gap-skills-guidance-onboard-001
- run through [X] with [person] → gap-skills-guidance-mentor-001
- review [X] with [person] → gap-skills-guidance-mentor-001
- present [X] to [person] → gap-skills-guidance-train-001
- discuss [X] with [person] → gap-skills-guidance-mentor-001
- consult on/with → gap-skills-guidance-mentor-001
- seek advice on → gap-skills-guidance-mentor-001
- get feedback on → gap-skills-guidance-mentor-001
- sanity check [X] with → gap-skills-guidance-mentor-001
- second opinion → gap-skills-guidance-mentor-001
- rubber duck → gap-skills-guidance-mentor-001
- sounding board → gap-skills-guidance-mentor-001

---

## B34-compiler-060-002 — final rule: all advisory vocabulary → gap, never topic predicate

Advisory vocabulary (consult, seek advice, get feedback, sanity check, second opinion, rubber duck, sounding board) = guidance.
No topic mentioned alongside advisory vocabulary fires its associated predicate alone.
Predicate fires only when requester's direct code action appears in the same request.

---

## B34-compiler-060-003 — final rule: "show how to" and "demonstrate how to" → gap with no exceptions

"show how to [any code verb]" → gap-skills-guidance-train-001
"demonstrate how to [any code verb]" → gap-skills-guidance-train-001
No topic, no code verb in infinitive form overrides this rule.
Exception clause: only when requester performs a DIFFERENT code action in the same request does a predicate fire (as gap warning + code predicate).

---

## B34-compiler-060-004 — final disambiguation: "introduce [person]" vs "introduce [feature]"

Subject type determines routing:
- Person/team/role = gap (onboard-001 or train-001)
- Feature/module/capability/component = IMPLEMENT
Ambiguous cases (e.g., "introduce the concept and the module") → both: gap warning + IMPLEMENT.

---

## B34-compiler-060-005 — B34 complete DomainId and gap ID reference

All B34 guidance gaps use DomainId: software-development.
Three gap IDs:
1. gap-skills-guidance-mentor-001 — ongoing advisory/mentoring relationship
2. gap-skills-guidance-train-001 — formal teaching/training/demonstration
3. gap-skills-guidance-onboard-001 — orientation/introduction/familiarization

No other domain IDs are valid for B34. No other gap IDs exist in B34.
