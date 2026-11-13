# Layer 0-A — Compiler Pairs (010)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-010-001 — "upskill" → gap

A: "Upskill the backend team on event-driven architecture patterns."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "upskill" = training vocabulary; "event-driven architecture patterns" = methodology, not artifact → gap.
Note: "Design an event-driven architecture" → ARCHITECTURE (design verb + structural artifact). "Upskill on" → gap.

---

## B34-compiler-010-002 — "demo" → gap

A: "Demo the new deployment pipeline to the team."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "demo" = demonstration/presentation vocabulary; "deployment pipeline" object does not fire QA → gap.

---

## B34-compiler-010-003 — "write" + runbook → DOCUMENT

A: "Write a runbook for the on-call engineers covering common failure modes."
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "write a runbook" → DOCUMENT (write + operational documentation artifact).

---

## B34-compiler-010-004 — "build" + documentation system → IMPLEMENT

A: "Build the developer documentation portal with search and versioning."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "build" = code verb; "documentation portal" = software system. IMPLEMENT fires despite documentation context.

---

## B34-compiler-010-005 — "guide" as action verb → gap

A: "Guide the team through the incident response process."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "guide through" = mentoring/facilitation vocabulary → gap. "incident response" alone does not fire SECURITY.
