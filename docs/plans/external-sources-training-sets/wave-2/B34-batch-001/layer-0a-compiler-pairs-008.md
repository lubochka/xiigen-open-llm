# Layer 0-A — Compiler Pairs (008)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-008-001 — "learn" → gap

A: "Help the team learn how to write scalable microservices."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "learn" = learning vocabulary; "help the team" = facilitation; neither fires any predicate.

---

## B34-compiler-008-002 — "self-paced" course → gap

A: "Create a self-paced course on our internal tooling."
B: Predicates: ["DOCUMENT"] (course = documentation artifact type; create + content artifact → DOCUMENT)
Evidence: "create a course" → DOCUMENT (create + curriculum/documentation artifact). NOT IMPLEMENT unless course is a software system.
Note: "create a course platform" → IMPLEMENT; "create a course" → DOCUMENT.

---

## B34-compiler-008-003 — "build" + test → IMPLEMENT+TEST

A: "Build the unit test framework for the junior team and write sample tests."
B: Predicates: ["IMPLEMENT", "TEST"]
Evidence: "build the [framework]" → IMPLEMENT; "write sample tests" → TEST. Both fire.
TransitionType: feature_addition (IMPLEMENT present).
Gap warning: None — both are predicate-firing requests.

---

## B34-compiler-008-004 — "refactor" in training context → IMPLEMENT (refactoring)

A: "Teach the team refactoring techniques by refactoring the payment service together."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
Evidence: "teach" → gap; "refactoring the payment service" → IMPLEMENT; "refactor" verb → TransitionType: refactoring.

---

## B34-compiler-008-005 — "introduce" concept → gap

A: "Introduce domain-driven design principles to the architecture team."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "introduce" = teaching vocabulary; "domain-driven design" = methodology, not an artifact → gap.
Note: "Design the domain model" → ARCHITECTURE (design verb + structural noun). "Introduce DDD" → gap.
