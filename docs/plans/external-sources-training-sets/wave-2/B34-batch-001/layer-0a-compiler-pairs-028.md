# Layer 0-A — Compiler Pairs (028)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-028-001 — "review with" → gap

A: "Review the API design patterns with the junior engineers so they understand the conventions."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "review with [person]" = collaborative explanation/teaching vocabulary → gap. "so they understand" = learning intent confirms gap.

---

## B34-compiler-028-002 — "review with" vs "review and fix" disambiguation

A: "Review the authentication module with the team and fix the identified vulnerabilities."
B: Predicates: ["SECURITY"] | TransitionType: documentation
Evidence: "review with the team" = gap warning (gap-skills-guidance-mentor-001); "fix the identified vulnerabilities" = requester's direct security action → SECURITY fires.

---

## B34-compiler-028-003 — "present" → gap

A: "Present the system architecture to the stakeholders and engineering board."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "present" = demonstration/explanation vocabulary → gap. Presenting information to an audience = teaching/communication action, not a code action.

---

## B34-compiler-028-004 — "present" + embedded ARCHITECTURE action

A: "Present and then design the new event-driven architecture for the platform."
B: Predicates: ["ARCHITECTURE"] | TransitionType: documentation
Evidence: "present" = gap warning (gap-skills-guidance-train-001); "design the new event-driven architecture" = requester's direct architecture action → ARCHITECTURE fires.

---

## B34-compiler-028-005 — "discuss" → gap

A: "Discuss the trade-offs between monolith and microservices with the engineering leads."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "discuss" = collaborative dialogue/guidance vocabulary → gap. Discussing technical topics = knowledge-sharing action, not a code-action.
