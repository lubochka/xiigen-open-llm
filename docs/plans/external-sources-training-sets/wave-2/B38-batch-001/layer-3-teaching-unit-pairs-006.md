# Layer 3 — Teaching Unit Pairs (006)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-006-001 — Teaching: Complete B38 gap vocabulary list

LESSON: These terms are B38 gap vocabulary. They produce CapabilityGapWarnings but do not fire any predicate.

B38 GAP VOCABULARY:
| Term | Gap ID | What it represents |
|---|---|---|
| go-live | gap-project-delivery-go-live-001 | Production go-live event |
| checklist | gap-project-delivery-checklist-001 | Release checklist |
| rollout | gap-project-delivery-rollout-golive-001 | Staged rollout operation |
| cutover | gap-project-delivery-cutover-001 | System cutover activity |
| runbook | gap-project-delivery-runbook-001 | Operational runbook |
| launch | gap-project-delivery-launch-001 | Product/feature launch |
| on-call | (operational) | On-call rotation |
| monitor | (operational — rule 7) | Monitoring/observability |
| execute (execution context) | (operational — rule 8) | Running operations |
| deployment (operational context) | (operational — rule 4) | Deploying software |
| rollback | (operational) | Rolling back a deployment |

---

## B38-TU-006-002 — Teaching: Gap vocabulary behavior rules

Rule A: Gap vocabulary alone → knowledge_edge
"Execute the rollout" → [] → knowledge_edge

Rule B: Gap vocabulary + other predicates → CapabilityGapWarning only
"Review the go-live plan" → ["QA"] with gap warning on "go-live"

Rule C: Gap vocabulary does NOT suppress predicates
"Approve the launch plan" → ["QA"] (approve fires QA; launch is gap warning)
(NOT: launch suppresses QA — this would be v6-01 corruption)

Rule D: Multiple gap terms still produce single knowledge_edge (not multiple states)
"Execute rollout via runbook" → [] → one knowledge_edge state with multiple gap warnings

---

## B38-TU-006-003 — Teaching: Gap vocabulary that surprises learners

"checklist" surprises: sounds like it could fire DOCUMENT
→ It does NOT. Checklist is gap vocabulary, not a DOCUMENT trigger.
→ Only "criteria" and "document/documentation" fire DOCUMENT in B38.

"monitor" surprises: sounds like it could be quality-adjacent (QA)
→ It does NOT. Rule 7 is explicit: monitor is not in any predicate.

"launch" surprises: sounds like "build" (building and launching)
→ It does NOT fire IMPLEMENT. Launch is operational gap vocabulary.
→ Only "build", "service", "pipeline" fire IMPLEMENT from operational-adjacent words.

"execute" surprises: sounds like it could trigger IMPLEMENT (executing code)
→ It does NOT. Rule 8 explicit: IMPLEMENT = code authoring, not running.
