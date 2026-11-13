# Layer 0-A — Compiler Pairs (002)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-002-001 — "coach" alone → gap

A: "Coach the team on writing testable code."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001 | DomainId: software-development
Evidence: "coach" = guidance vocabulary; "testable code" = not a predicate token without test-writing verb.

---

## B34-compiler-002-002 — "teach" + "write" → DOCUMENT (teach is gap; write fires)

A: "Teach the team and write a guide on clean code practices."
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "teach" → gap; "write a guide" → DOCUMENT (write + documentation artifact noun). DOCUMENT fires.
Gap warning: gap-skills-guidance-mentor-001 (teach vocabulary logged as capability warning).

---

## B34-compiler-002-003 — "onboard" alone → gap

A: "Onboard the new engineers to the codebase."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001 | DomainId: software-development
Evidence: "onboard" = guidance vocabulary; "new engineers" = not a predicate token; "codebase" = not a predicate token alone.

---

## B34-compiler-002-004 — "implement" + onboarding object → IMPLEMENT

A: "Implement the onboarding checklist application for new engineers."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "implement" = code verb; "checklist application" = software component. Code verb overrides guidance object.
Gap warning: gap-skills-guidance-onboard-001 (onboarding context noted).

---

## B34-compiler-002-005 — "explain" → gap (never fires DOCUMENT or SECURITY)

A: "Explain the authentication flow to the team."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "explain" = teaching vocabulary; does NOT fire DOCUMENT (document/write verbs required); does NOT fire SECURITY (security-implementing action required).
