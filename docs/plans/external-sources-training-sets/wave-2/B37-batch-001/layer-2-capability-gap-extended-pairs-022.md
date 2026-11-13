# Layer 2 — Capability-Gap-Extended Pairs (022)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-022-001 — "track and document blockers for stakeholders" — DOCUMENT fires + gap warning

A: "Track and document the blockers for all project stakeholders to review."

Predicates: [DOCUMENT]
GapId: gap-project-managing-blockers-001
DomainId: software-development

Route: DocumentationFlow (2.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: gap-project-managing-track-001, gap-project-managing-blockers-001, gap-project-managing-stakeholder-update-001
Note: "document" fires DOCUMENT; "stakeholders" fires DOCUMENT (two triggers, one predicate). "track" and "blockers" are gap vocabulary.

---

## B37-L2-022-002 — "weekly progress report to stakeholders" — DOCUMENT fires + gap warning

A: "Produce the weekly progress report for stakeholders and coordinate action items."

Predicates: [DOCUMENT]
GapId: gap-project-managing-progress-001
DomainId: software-development

Route: DocumentationFlow (2.1)
CapabilityGapWarnings: gap-project-managing-progress-001, gap-project-managing-status-report-001, gap-project-managing-stakeholder-update-001, gap-project-managing-coordinate-001, gap-project-managing-action-items-001

---

## B37-L2-022-003 — "manage blockers, report to programme board" — zero predicates (no stakeholder)

A: "Track blockers, coordinate escalations, and report to the programme board."

Predicates: []
GapId: gap-project-managing-blockers-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-track-001, gap-project-managing-coordinate-001, gap-project-managing-escalation-001, gap-project-managing-status-report-001

---

## B37-L2-022-004 — "risk log + stakeholder notification" — DOCUMENT fires + gap warning

A: "Notify all stakeholders of risk log updates and track open risk mitigations."

Predicates: [DOCUMENT]
GapId: gap-project-managing-raid-log-001
DomainId: software-development

Route: DocumentationFlow (2.1)
CapabilityGapWarnings: gap-project-managing-stakeholder-update-001, gap-project-managing-raid-log-001, gap-project-managing-track-001

---

## B37-L2-022-005 — "change request + stakeholder sign-off" — DOCUMENT fires + gap warning

A: "Escalate the change request to stakeholders for sign-off and track the approval status."

Predicates: [DOCUMENT]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Route: DocumentationFlow (2.1)
CapabilityGapWarnings: gap-project-managing-escalation-001, gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001, gap-project-managing-track-status-001
