# Layer 0-B — Router Pairs (034)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-034-001 — "show" + integrate → API_CONTRACT routing

A: "Show the team the integration pattern and integrate the new data pipeline."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "show the integration pattern" = gap warning; "integrate the new data pipeline" → API_CONTRACT fires (integrate always fires).

---

## B34-router-034-002 — "brief" + three predicates routing

A: "Brief the team, then architect the solution, integrate with the existing services, and implement the new module."
Router decision: Predicates = ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT"] → ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "brief" = gap warning; three code actions fire in priority order. TransitionType: feature_addition.

---

## B34-router-034-003 — guidance + gap ID priority rule

When multiple guidance verbs present, gap ID priority:
1. gap-skills-guidance-mentor-001 if any mentor/coach/advise/walk-through/review-with/discuss/brief present.
2. gap-skills-guidance-onboard-001 if new hire/ramp-up/familiarize context but no mentor verbs.
3. gap-skills-guidance-train-001 if only show/demonstrate/present/introduce/explain verbs.

---

## B34-router-034-004 — "run through" + DOCUMENT + IMPLEMENT routing

A: "Run through the feature spec, implement the inventory service, and document the API endpoints."
Router decision: Predicates = ["IMPLEMENT", "DOCUMENT"] → ImplementationFlow41 ‖ DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "run through spec" = gap warning; "implement" → IMPLEMENT; "document" → DOCUMENT. IMPLEMENT dominates TransitionType: feature_addition.

---

## B34-router-034-005 — "familiarize" only → single gap

A: "Familiarize yourself with the project codebase before starting the sprint."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001
Evidence: "familiarize with codebase" = onboarding vocabulary even for self-directed onboarding → gap-skills-guidance-onboard-001. No code action in scope.
