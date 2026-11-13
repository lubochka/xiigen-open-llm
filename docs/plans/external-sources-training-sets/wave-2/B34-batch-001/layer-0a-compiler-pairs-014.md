# Layer 0-A — Compiler Pairs (014)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-014-001 — "pair with" on testing → gap

A: "Pair with the backend team on their unit testing approach."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "pair with" = collaborative learning vocabulary → gap. "unit testing approach" = topic noun, not test-writing action.

---

## B34-compiler-014-002 — "pair with" + embedded TEST verb

A: "Pair with me to write unit tests for the authentication module."
Analysis: "pair with" = guidance frame; "write unit tests" = direct code action by requester → TEST fires.
B: Predicates: ["TEST"] | TransitionType: documentation
Evidence: "write unit tests" + "module" → TEST; "pair with" = gap warning (TEST fires alongside gap warning).

---

## B34-compiler-014-003 — "pair with" + embedded IMPLEMENT

A: "Pair with the senior engineer and implement the rate-limiting middleware."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "implement" + "middleware" → IMPLEMENT; "pair with" = gap warning; embedded verb rule: IMPLEMENT fires.

---

## B34-compiler-014-004 — "pair with" → gap: no embedded code verb

A: "Pair with the DevOps team to learn the CI/CD (Continuous Integration / Continuous Delivery) pipeline setup."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "pair with … to learn" = learning intent frame → gap. "CI/CD pipeline setup" = topic context, not code-writing action. "learn" reinforces gap classification.

---

## B34-compiler-014-005 — v6 corruption: "pair with" incorrectly fires API_CONTRACT

v6 corrupt: "Pair with the platform team on their API integration" → Predicates: ["API_CONTRACT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "pair with on API integration" = observation/collaboration vocabulary → gap. API_CONTRACT requires "integrate" as requester's direct action verb. "pair with on" ≠ "integrate."
Fix: Predicate = [].
