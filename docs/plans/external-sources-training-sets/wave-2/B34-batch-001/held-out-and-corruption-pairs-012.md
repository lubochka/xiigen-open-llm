# Held-Out and Corruption Pairs (012)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-012-A — Held-out: "bootcamp" → gap

A: "Run a two-week coding bootcamp for new graduates."
B: Predicates: [] | GapHandler → gap-skills-guidance-onboard-001
Reason: "run a bootcamp" = training event facilitation → gap.

---

## HC-012-B — Held-out: "build a bootcamp platform" → IMPLEMENT

A: "Build a coding bootcamp management platform for tracking cohort progress."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "build a [management] platform" = code verb + software system → IMPLEMENT.
Distinction: running a bootcamp → gap; building a bootcamp platform → IMPLEMENT.

---

## HC-012-C — Held-out: full four-predicate guidance + code combo

A: "Architect the learning platform, build the skill assessment module, write E2E tests, and document the assessment API."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT"]
Orchestration: ArchitectureFlow31 → ImplementationFlow41 → E2ETestingFlow53 ‖ DocumentationFlow
TransitionType: feature_addition

---

## HC-012-D — Corruption v6-03: guidance + code collapsed to guidance only

v6 corrupt: "Mentor the team, build the LMS, document the API" → Predicates: [] → GapHandler
Correct: Predicates: ["IMPLEMENT", "DOCUMENT"] | Two flows execute; GapHandler NOT invoked
Error type: v6-03 (predicate loss). "build" → IMPLEMENT; "document" → DOCUMENT. Guidance "mentor" = gap warning. The two code predicates must fire; GapHandler is suppressed.
