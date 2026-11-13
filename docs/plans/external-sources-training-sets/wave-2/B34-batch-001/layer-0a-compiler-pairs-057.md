# Layer 0-A — Compiler Pairs (057)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-057-001 — "get feedback" + "then implement feedback" = IMPLEMENT fires

A: "Get feedback on the API (Application Programming Interface) design and implement the suggested improvements."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "get feedback on design" = gap warning; "implement the suggested improvements" = requester's IMPLEMENT action → IMPLEMENT fires. The feedback topic does not suppress the code action.

---

## B34-compiler-057-002 — "sanity check" + "then write tests" = TEST fires

A: "Sanity check the test approach with the tech lead and write the unit tests for the parser."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "sanity check with tech lead" = gap warning; "write unit tests for parser" = requester's TEST action → TEST fires.

---

## B34-compiler-057-003 — "second opinion" + "then harden" = SECURITY fires

A: "Get a second opinion on the encryption approach and harden the data-at-rest storage."
B: Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "second opinion on encryption approach" = gap warning; "harden data-at-rest storage" = requester's SECURITY action → SECURITY fires.

---

## B34-compiler-057-004 — "rubber duck" + "then integrate" = API_CONTRACT fires

A: "Rubber duck the integration design and integrate the notification service with the event bus."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "rubber duck integration design" = gap warning; "integrate notification service with event bus" = requester's direct integrate action → API_CONTRACT fires.

---

## B34-compiler-057-005 — "sounding board" + "then document" = DOCUMENT fires

A: "Use the team as a sounding board for the documentation approach and document the API (Application Programming Interface) reference."
B: Predicates: ["DOCUMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "sounding board for approach" = gap warning; "document the API reference" = requester's DOCUMENT action → DOCUMENT fires.
