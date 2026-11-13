# Layer 0-B — Router Pairs (016)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-016-001 — Gap route: "facilitate a workshop" → gap

A: "Facilitate a workshop on microservices decomposition for the platform team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001

---

## B34-router-016-002 — Gap route: "run a workshop" → gap

A: "Run a hands-on workshop where engineers learn to use the observability stack."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "run a workshop where engineers learn" = facilitation + learning intent → gap. "learn to use" is what engineers do, not requester. Requester's action = run = gap.

---

## B34-router-016-003 — IMPLEMENT route: "workshop" + embedded build action by requester

A: "Facilitate the workshop and personally build the demo service we'll use as reference."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: "personally build the demo service" = direct requester action → IMPLEMENT; "facilitate" = gap warning.

---

## B34-router-016-004 — Gap route: "design workshop curriculum" → gap (curriculum noun)

A: "Design the workshop curriculum for the three-day architecture bootcamp."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "design workshop curriculum" = content design → gap. Curriculum/content noun → gap. Structural noun would fire ARCHITECTURE.

---

## B34-router-016-005 — ARCHITECTURE route: "design system architecture" → structural noun

A: "Design the system architecture for the new microservices platform."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31
Evidence: "design" + structural noun "system architecture" → ARCHITECTURE. Contrast with curriculum noun (016-004).
