# Layer 0-A — Compiler Pairs (035)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-035-001 — complete "show how to" vocabulary reference

| Pattern | Predicate result | Gap ID |
|---------|-----------------|--------|
| "show how to [infinitive]" | [] | gap-skills-guidance-train-001 |
| "show [person] how [existing X] works" | [] | gap-skills-guidance-train-001 |
| "show and implement" | [IMPLEMENT] + gap warning | gap-skills-guidance-train-001 |
| "show and write tests" | [TEST] + gap warning | gap-skills-guidance-train-001 |
| "show and secure" | [SECURITY] + gap warning | gap-skills-guidance-train-001 |

---

## B34-compiler-035-002 — "introduce" vocabulary full reference

| Pattern | Predicate result | Gap ID |
|---------|-----------------|--------|
| "introduce [person/team] to [topic]" | [] | gap-skills-guidance-train-001 |
| "introduce [new hire] to" | [] | gap-skills-guidance-onboard-001 |
| "introduce [feature/module] into [system]" | [IMPLEMENT] | — (software insertion) |
| "introduce [change] to [codebase]" | [IMPLEMENT] | — (software insertion) |
| "introduce and then build" | [IMPLEMENT] + gap warning | gap-skills-guidance-train-001 |

---

## B34-compiler-035-003 — "brief the team on" complete reference

| Pattern | Predicate result | Gap ID |
|---------|-----------------|--------|
| "brief [team] on [any topic]" | [] | gap-skills-guidance-mentor-001 |
| "brief and implement" | [IMPLEMENT] + gap warning | gap-skills-guidance-mentor-001 |
| "brief and document" | [DOCUMENT] + gap warning | gap-skills-guidance-mentor-001 |
| "brief and harden/secure" | [SECURITY] + gap warning | gap-skills-guidance-mentor-001 |

---

## B34-compiler-035-004 — guidance + ARCHITECTURE compound scenario

A: "Brief the team on the new data pipeline architecture and design the streaming topology."
B: Predicates: ["ARCHITECTURE"] | TransitionType: documentation
Evidence: "brief on" = gap warning (gap-skills-guidance-mentor-001); "design the streaming topology" = requester's direct architecture action → ARCHITECTURE fires.

---

## B34-compiler-035-005 — "discuss" + TEST embedded

A: "Discuss the test coverage gaps with the QA (Quality Assurance) lead and write the missing integration tests."
B: Predicates: ["TEST"] | TransitionType: documentation
Evidence: "discuss with [person]" = gap warning (gap-skills-guidance-mentor-001); "write the missing integration tests" = requester's direct test action → TEST fires.
