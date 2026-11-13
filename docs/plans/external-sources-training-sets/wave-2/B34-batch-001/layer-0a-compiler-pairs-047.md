# Layer 0-A — Compiler Pairs (047)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-047-001 — Round 3 vocabulary reference: advisory/feedback frame

Advisory and feedback vocabulary (all → gap):
- consult on/with → gap-skills-guidance-mentor-001
- seek advice on → gap-skills-guidance-mentor-001
- get feedback on → gap-skills-guidance-mentor-001
- sanity check [X] with → gap-skills-guidance-mentor-001
- second opinion → gap-skills-guidance-mentor-001
- rubber duck → gap-skills-guidance-mentor-001
- sounding board → gap-skills-guidance-mentor-001

---

## B34-compiler-047-002 — "consult on" does not fire topic predicate

A: "Consult the architecture team on the right database sharding strategy."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "consult on [architecture topic]" = advisory → gap. Database/architecture topic does not fire ARCHITECTURE predicate.

---

## B34-compiler-047-003 — "sanity check" + test action

A: "Sanity check the test coverage expectations with the QA (Quality Assurance) lead and write the missing integration tests."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "sanity check with QA lead" = gap warning; "write missing integration tests" = requester's TEST action → TEST fires.

---

## B34-compiler-047-004 — "get feedback on" + document action

A: "Get feedback on the current API (Application Programming Interface) docs and update the endpoint reference."
B: Predicates: ["DOCUMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "get feedback on docs" = gap warning; "update the endpoint reference" = requester's DOCUMENT action → DOCUMENT fires.

---

## B34-compiler-047-005 — Round 3 boundary: all advisory vocabulary = gap, never fires topic predicate alone

Advisory vocabulary (consult, seek advice, get feedback, sanity check, second opinion, rubber duck, sounding board) = guidance vocabulary.
Topic in advisory frame = learning/discussion content only.
Predicate fires ONLY when requester's direct action verb appears alongside advisory frame.
