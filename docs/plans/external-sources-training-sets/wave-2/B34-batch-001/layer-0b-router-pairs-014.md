# Layer 0-B — Router Pairs (014)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-014-001 — Gap route: "pair with" → gap

A: "Pair with the senior architect on the new platform design."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-014-002 — TEST route: "pair with" + write tests embedded

A: "Pair with me to write the contract tests for the billing API."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51
Evidence: "write contract tests" = TEST trigger; "pair with" = gap warning alongside TEST.

---

## B34-router-014-003 — API_CONTRACT route: "pair with" + integrate embedded

A: "Pair with the platform team to integrate our service with their event bus."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33
Evidence: "integrate" always fires API_CONTRACT; "pair with" = gap warning.

---

## B34-router-014-004 — Gap route: "pair with … to learn"

A: "Pair with the backend team to learn how they handle distributed transactions."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "pair with … to learn" = learning intent → gap. "distributed transactions" = topic noun, no code-action verb.

---

## B34-router-014-005 — IMPLEMENT+TEST route: "pair with" + multiple embedded actions

A: "Pair with the team to implement the feature flag system and write tests for the rollout logic."
Router decision: Predicates = ["IMPLEMENT", "TEST"] → ImplementationFlow41 → UnitTestingFlow51
Evidence: "implement" + "feature flag system" → IMPLEMENT; "write tests" → TEST; "pair with" = gap warning; both flows execute.
