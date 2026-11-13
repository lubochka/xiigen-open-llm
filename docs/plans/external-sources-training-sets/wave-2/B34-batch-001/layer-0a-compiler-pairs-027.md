# Layer 0-A — Compiler Pairs (027)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-027-001 — "brief the team on" → gap

A: "Brief the team on the new security patching procedures before the sprint."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "brief the team on" = information-sharing/guidance vocabulary → gap. Communicating knowledge to a group = teaching action.

---

## B34-compiler-027-002 — "familiarize with" → gap

A: "Familiarize the new developers with our deployment pipeline and rollback process."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Evidence: "familiarize with" = onboarding/orientation vocabulary → gap-skills-guidance-onboard-001. New developers context confirms onboarding sense.

---

## B34-compiler-027-003 — "run through" → gap

A: "Run through the authentication flow with the frontend team before implementation."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "run through" = walkthrough/explanation vocabulary → gap. "before implementation" = future action context; no code verb by requester in current scope.

---

## B34-compiler-027-004 — "run through" vs actual action

A: "Run through the deployment checklist and deploy to staging."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "run through the checklist" = preparation action → gap warning (gap-skills-guidance-mentor-001); "deploy to staging" = requester's direct deployment action → IMPLEMENT fires. Embedded verb rule.

---

## B34-compiler-027-005 — "brief" + embedded security action

A: "Brief the team on threat modeling and harden the authentication endpoints."
B: Predicates: ["SECURITY"] | TransitionType: documentation
Evidence: "brief the team on threat modeling" = gap warning (gap-skills-guidance-mentor-001); "harden the authentication endpoints" = requester's direct security action → SECURITY fires.
