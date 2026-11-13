# Layer 0-A — Compiler Pairs (054)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-054-001 — complete B34 guidance vocabulary (all three rounds)

Round 1 (001–012): mentor, coach, teach, train [human], guide, explain, onboard, shadow, upskill, advise, facilitate, pair-with
Round 2 (013–024): walk-through, workshop, review-with, ramp-up, show [guidance], introduce [person], brief [team] on, familiarize with, run through, present to, discuss
Round 2 (025–036): advise, facilitate [extension], guidance+TEST, guidance+IMPLEMENT, guidance+API_CONTRACT, explain boundary
Round 3 (037–060): show how to, demonstrate, introduce [person/feature boundary], brief, familiarize, run through, review with, present, discuss, guidance+UI, guidance+ARCHITECTURE, consult on, seek advice, get feedback, sanity check, second opinion, rubber duck, sounding board

All vocabulary → gap when predicate set = []. Always DomainId: software-development.

---

## B34-compiler-054-002 — "brief" + "familiarize" stacked = single gap

A: "Brief and familiarize the new release engineers with the deployment procedures."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Rule: brief + familiarize = both guidance vocabulary → gap-skills-guidance-onboard-001 (familiarize implies onboarding context).

---

## B34-compiler-054-003 — "sanity check" does not fire topic predicate in any topic

A: "Sanity check the API (Application Programming Interface) contract design with the team lead."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
A: "Sanity check the test coverage with the QA (Quality Assurance) lead."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
A: "Sanity check the security posture with the security team."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: sanity check = advisory → gap always, regardless of topic.

---

## B34-compiler-054-004 — "rubber duck" does not fire topic predicate

A: "Rubber duck the authentication flow logic."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
A: "Rubber duck the architecture decisions."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: rubber duck = thinking-aloud advisory → gap always, regardless of topic.

---

## B34-compiler-054-005 — "sounding board" with architecture topic = gap (not ARCHITECTURE)

A: "Use the senior architects as a sounding board for the service decomposition decisions."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "sounding board" = advisory vocabulary → gap. Architecture topic in this frame does not fire ARCHITECTURE predicate.
