# Layer 0-A — Compiler Pairs (023)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-023-001 — Multiple guidance verbs + code verbs: full analysis

A: "Coach the team through building the auth service, walk through integrating it with the SSO provider, and guide them through writing tests."
Analysis: Three guidance frames with three embedded code actions.
- "building the auth service" → IMPLEMENT
- "integrating it with the SSO provider" → API_CONTRACT
- "writing tests" → TEST
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"] | TransitionType: feature_addition
Evidence: All three code verbs fire; all three guidance frames become gap warnings. GapHandler NOT invoked.

---

## B34-compiler-023-002 — Guidance verbs only: multiple, no embedded code actions

A: "Mentor, coach, and advise the new backend engineers on clean architecture practices."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "mentor" + "coach" + "advise" = all guidance vocabulary, no embedded code-action verbs. Predicate set = []. Full GapHandler invocation.

---

## B34-compiler-023-003 — Mixed: guidance + DOCUMENT

A: "Shadow the tech writer and document the developer onboarding checklist."
Analysis: "shadow" = gap frame; "document the developer onboarding checklist" = direct action → DOCUMENT fires.
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "document" + documentation artifact → DOCUMENT; "shadow" = gap warning.

---

## B34-compiler-023-004 — Mixed: guidance + ARCHITECTURE + IMPLEMENT

A: "Advise on the approach, design the platform architecture, and build the core service layer."
Analysis: "advise" = gap frame; "design the platform architecture" → ARCHITECTURE; "build the core service layer" → IMPLEMENT.
B: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | TransitionType: feature_addition
Evidence: Two code verbs fire; "advise" = gap warning alongside both predicates.

---

## B34-compiler-023-005 — Mixed: explain (always gap) + IMPLEMENT

A: "Explain the patterns involved, then implement the event sourcing framework for the order domain."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "explain" = always gap (gap warning); "implement" + "event sourcing framework" → IMPLEMENT fires. "explain" never contributes a predicate even alongside IMPLEMENT.
