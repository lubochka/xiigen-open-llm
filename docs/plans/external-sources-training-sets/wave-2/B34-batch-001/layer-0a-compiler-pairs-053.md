# Layer 0-A — Compiler Pairs (053)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-053-001 — "review with" vs "review for" disambiguation (extended)

A: "Review the codebase with the team and document the technical debt findings."
B: Predicates: ["DOCUMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "review with team" = gap warning; "document the findings" = requester's DOCUMENT action → DOCUMENT fires.

---

## B34-compiler-053-002 — "review and fix" → code action determines predicate

A: "Review the authentication flow with the security team and fix the session fixation vulnerability."
B: Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "review with security team" = gap warning; "fix session fixation vulnerability" = requester's SECURITY action → SECURITY fires.

---

## B34-compiler-053-003 — "run through" + API_CONTRACT embedded

A: "Run through the integration checklist with the team and integrate the third-party analytics SDK (Software Development Kit)."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "run through checklist with team" = gap warning; "integrate third-party analytics SDK" = requester's direct integrate action → API_CONTRACT fires.

---

## B34-compiler-053-004 — "present" + multiple code actions

A: "Present the migration plan to stakeholders, implement the database schema changes, and document the rollback procedure."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | GapWarning: gap-skills-guidance-train-001
Rule: "present to stakeholders" = gap warning; "implement schema changes" → IMPLEMENT; "document rollback" → DOCUMENT. TransitionType: feature_addition.

---

## B34-compiler-053-005 — "discuss" + implement + test = both predicates fire

A: "Discuss the feature requirements with the product team, implement the user notification system, and write E2E (End-to-End) tests."
B: Predicates: ["IMPLEMENT", "TEST"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "discuss with product team" = gap warning; "implement notification system" → IMPLEMENT; "write E2E tests" → TEST. TransitionType: feature_addition.
