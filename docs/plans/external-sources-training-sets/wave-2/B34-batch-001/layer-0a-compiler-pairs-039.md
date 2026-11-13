# Layer 0-A — Compiler Pairs (039)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-039-001 — "introduce" as onboarding vocabulary

A: "Introduce the new hires to our development workflow and toolchain."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Rule: "introduce [person] to [process]" = onboarding vocabulary → gap-skills-guidance-onboard-001.

---

## B34-compiler-039-002 — "introduce [feature] into" = IMPLEMENT, not gap

A: "Introduce rate limiting into the API (Application Programming Interface) gateway."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Rule: "introduce [feature] into [system]" = feature addition vocabulary → IMPLEMENT fires. No gap. Subject is a software component, not a person.

---

## B34-compiler-039-003 — "introduce" disambiguation boundary

A: "Introduce the junior engineers to the codebase and then introduce error handling into the core module."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-onboard-001
Rule: "introduce [engineers] to [codebase]" = gap warning (onboard-001); "introduce [error handling] into [module]" = IMPLEMENT fires. Mixed sentence.

---

## B34-compiler-039-004 — "introduce" + person + learning topic = gap

A: "Introduce the team to domain-driven design (DDD) principles."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: "introduce [team] to [concept/principles]" = teaching vocabulary → gap-skills-guidance-train-001 (conceptual introduction, not onboarding).

---

## B34-compiler-039-005 — "introduce" in presentation context = gap

A: "Introduce the architectural changes to the stakeholders and present the migration roadmap."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: "introduce to stakeholders" + "present to stakeholders" = both guidance/presentation vocabulary → gap. No code action by requester.
