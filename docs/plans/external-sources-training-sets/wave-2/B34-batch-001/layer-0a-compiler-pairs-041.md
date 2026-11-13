# Layer 0-A — Compiler Pairs (041)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-041-001 — "review with" = gap (mentoring)

A: "Review the design patterns with the senior architects before finalizing."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "review [X] with [person]" = collaborative mentoring vocabulary → gap. No code action by requester.

---

## B34-compiler-041-002 — "review with" + security fix

A: "Review the authentication design with the security team and fix the token expiry vulnerability."
B: Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "review with security team" = gap warning; "fix the token expiry vulnerability" = requester's security action → SECURITY fires.

---

## B34-compiler-041-003 — "present" = always gap (regardless of topic)

A: "Present the refactored service architecture to the engineering leadership."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: present = guidance vocabulary → gap always. Architecture topic does not trigger ARCHITECTURE predicate when verb is present.

---

## B34-compiler-041-004 — "present" + architect action

A: "Present the current architecture to the team and then architect the new microservices decomposition."
B: Predicates: ["ARCHITECTURE"] | GapWarning: gap-skills-guidance-train-001
Rule: "present to team" = gap warning; "architect the new microservices decomposition" = requester's ARCHITECTURE action → ARCHITECTURE fires.

---

## B34-compiler-041-005 — "discuss" = guidance vocabulary → gap

A: "Discuss the API (Application Programming Interface) design principles with the frontend and backend teams."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: discuss = collaborative knowledge-transfer vocabulary → gap. Topic (API design) is the learning content, not a predicate trigger.
