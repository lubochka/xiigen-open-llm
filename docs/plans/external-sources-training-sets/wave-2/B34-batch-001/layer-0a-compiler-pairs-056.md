# Layer 0-A — Compiler Pairs (056)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-056-001 — purpose-clause guidance + multiple code verbs

A: "As part of knowledge transfer, implement the service health check endpoints, write the integration tests, and document the health check API (Application Programming Interface)."
B: Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "as part of knowledge transfer" = purpose clause → gap warning; implement + test + document = three code predicates all fire. TransitionType: feature_addition.

---

## B34-compiler-056-002 — "to help onboard" purpose clause + implement action

A: "To help onboard the new team, build a working example of the service template."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-onboard-001
Rule: "to help onboard" = purpose clause → gap warning (onboard-001); "build working example" = requester's IMPLEMENT action → IMPLEMENT fires.

---

## B34-compiler-056-003 — guidance vocabulary in conditional = still gap

A: "If the team needs it, explain the retry pattern before implementing the circuit breaker."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001
Rule: "explain the retry pattern" (even in conditional) = gap warning; "implementing the circuit breaker" = requester's IMPLEMENT action → IMPLEMENT fires.

---

## B34-compiler-056-004 — guidance framing at end of request = still gap warning

A: "Implement the feature flag service and then walk the team through how it works."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "implement the feature flag service" → IMPLEMENT fires; "walk the team through" (at end) = gap warning. Position does not affect outcome.

---

## B34-compiler-056-005 — nested guidance + guidance = single gap (no nesting rules)

A: "Mentor the senior developers on how to coach the junior developers."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "mentor on how to coach" = guidance nested inside guidance → single gap. No predicate fires. SCHEDULE_CAPABILITY_LEARNING emitted once.
