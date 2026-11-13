# Layer 3 — Teaching Unit Pairs (008)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU008-001 — Gap: deploy without QA

A: "Deploy the hotfix to production."
B: [] — "deploy" is operational gap. No QA abbreviation.
Reason: Deployment commands without QA abbreviation → gap. IMPLEMENT does not fire on "deploy".
CompiledPredicateIds: []

---

## TU008-002 — Gap: monitor / SLO / SLA

A: "Monitor the error rate SLO after deployment."
B: [] — operational monitoring vocabulary → gap.
Reason: monitor, SLO, SLA, alert, on-call → operational gap. Not QA activities.
CompiledPredicateIds: []

---

## TU008-003 — Gap: CI/CD pipeline vocabulary

A: "The CI/CD pipeline runs tests on every commit."
B: [] — CI/CD pipeline automation → gap.
Reason: CI/CD, continuous deployment, pipeline → operational/DevOps gap. TEST requires test-writing verb.
CompiledPredicateIds: []

---

## TU008-004 — Gap: hotfix release without QA

A: "Push the hotfix to production immediately."
B: [] — "push to production" is operational gap. No QA abbreviation or design verb.
Reason: Urgency does not change predicate classification. Operational verbs remain gap.
CompiledPredicateIds: []

---

## TU008-005 — Gap: feature flag / rollout

A: "Enable the feature flag for 10% of users."
B: [] — feature flag / rollout → operational gap.
Reason: Feature flag management, rollout percentage → operational vocabulary. No predicate fires.
CompiledPredicateIds: []
