# Layer 0-A — Compiler Pairs (029)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-029-001 — guidance + UI embedded

A: "Walk through the accessibility requirements with the frontend team and implement the WCAG (Web Content Accessibility Guidelines) compliant components."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "walk through with the team" = gap warning (gap-skills-guidance-mentor-001); "implement the WCAG compliant components" = requester's direct code action → IMPLEMENT fires.

---

## B34-compiler-029-002 — pure guidance + UI topic → gap

A: "Show the frontend developers how our UI (User Interface) component library is structured and how to use it."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "show how [X] is structured" = teaching vocabulary → gap. "how to use it" = learning content for others → gap. No direct code action by requester.

---

## B34-compiler-029-003 — guidance + ARCHITECTURE embedded

A: "Present the proposed CQRS (Command Query Responsibility Segregation) pattern and design the new event store."
B: Predicates: ["ARCHITECTURE"] | TransitionType: documentation
Evidence: "present the proposed pattern" = gap warning (gap-skills-guidance-train-001); "design the new event store" = requester's direct architecture action → ARCHITECTURE fires.

---

## B34-compiler-029-004 — pure guidance + ARCHITECTURE topic → gap

A: "Brief the team on our CQRS (Command Query Responsibility Segregation) architecture before the sprint planning."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "brief on" = teaching/guidance vocabulary → gap. "before the sprint planning" = temporal context; no code action in request scope.

---

## B34-compiler-029-005 — guidance + both UI and ARCHITECTURE topics → gap (all guidance)

A: "Explain the relationship between our UI component system and the underlying microservices architecture to the new fullstack engineers."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "explain" = always gap, regardless of topic. UI and architecture topics are learning content, not code actions. DomainId: software-development.
