# Layer 3 — Teaching Unit Pairs (008)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-008-001 — Teaching: TransitionType — no_contribution vs feature_addition

LESSON: TransitionType is determined by whether IMPLEMENT is in the predicate set.

RULE:
- IMPLEMENT in predicate set → TransitionType = feature_addition
- IMPLEMENT NOT in predicate set → TransitionType = no_contribution (or N/A for knowledge_edge)

NO_CONTRIBUTION scenarios (QA-dominant, no code authoring):
- Sign-off collection
- Approval-only flows
- Criteria validation (QA + DOCUMENT)
- Security audit for release (QA + SECURITY)
- Post-release validation

FEATURE_ADDITION scenarios (code in scope):
- Service release gate (QA + IMPLEMENT from "service")
- Build artifact audit (QA + IMPLEMENT from "build")
- CI/CD pipeline review (QA + IMPLEMENT from "pipeline")
- Full release gate (QA + IMPLEMENT + DOCUMENT + SECURITY)

---

## B38-TU-008-002 — Teaching: Why "service" changes TransitionType

"Sign off on the release" → ["QA"] → no_contribution
"Sign off on the service release" → ["QA","IMPLEMENT"] → feature_addition

The single word "service" elevates the TransitionType from no_contribution to feature_addition.

This is by design: when a service is mentioned, code is in scope. The IMPLEMENT predicate fires to ensure the code review phase runs. The TransitionType reflects that code is part of the scope.

---

## B38-TU-008-003 — Teaching: DOCUMENT does not create feature_addition

"Validate the release criteria and sign off" → ["QA","DOCUMENT"] → no_contribution
(criteria fires DOCUMENT; IMPLEMENT absent; TransitionType remains no_contribution)

DOCUMENT produces documentation, not code. Documentation is not a "feature" in the TransitionType model.

If the same request included a service:
"Validate the service release criteria and sign off" → ["QA","IMPLEMENT","DOCUMENT"] → feature_addition
(now IMPLEMENT is present from "service")

---

## B38-TU-008-004 — Teaching: TransitionType summary table

| Has IMPLEMENT? | Has QA? | TransitionType |
|---|---|---|
| No | No (empty) | N/A (knowledge_edge) |
| No | Yes | no_contribution |
| Yes | No | feature_addition |
| Yes | Yes | feature_addition |

The presence of IMPLEMENT in the predicate set is the sole decider for feature_addition vs no_contribution (when not knowledge_edge).
