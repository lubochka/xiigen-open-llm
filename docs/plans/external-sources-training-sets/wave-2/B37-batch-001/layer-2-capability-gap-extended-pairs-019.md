# Layer 2 — Capability-Gap-Extended Pairs (019)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-019-001 — "send stakeholder update" — DOCUMENT fires (stakeholder boundary)

A: "Send a weekly status update to all project stakeholders confirming delivery is on track."

Predicates: [DOCUMENT]
GapId: gap-project-managing-stakeholder-update-001
DomainId: software-development

Route: DocumentationFlow (2.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: gap-project-managing-status-report-001, gap-project-managing-track-status-001, gap-project-managing-stakeholder-update-001, gap-project-managing-manage-delivery-001
Note: DOCUMENT fires on "stakeholders". Notification intent not addressed by DocumentationFlow. v6-02 would suppress DOCUMENT — incorrect.

---

## B37-L2-019-002 — "escalate to stakeholders" — DOCUMENT fires (stakeholder boundary)

A: "Escalate the delivery blocker to senior stakeholders and coordinate an emergency response."

Predicates: [DOCUMENT]
GapId: gap-project-managing-stakeholder-update-001
DomainId: software-development

Route: DocumentationFlow (2.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: gap-project-managing-escalation-001, gap-project-managing-blockers-001, gap-project-managing-coordinate-001, gap-project-managing-stakeholder-update-001

---

## B37-L2-019-003 — "send update to programme board" — zero predicates (no stakeholder)

A: "Send a status update to the programme board confirming the delivery is on track."

Predicates: []
GapId: gap-project-managing-status-report-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-track-status-001, gap-project-managing-manage-delivery-001
Note: "programme board" ≠ stakeholder. Replacing "stakeholders" with "programme board" removes DOCUMENT and produces zero predicates.

---

## B37-L2-019-004 — "notify stakeholders of risk log" — DOCUMENT fires + risk gap

A: "Notify all stakeholders of risk log updates and coordinate risk mitigation responses."

Predicates: [DOCUMENT]
GapId: gap-project-managing-stakeholder-update-001
DomainId: software-development

Route: DocumentationFlow (2.1)
CapabilityGapWarnings: gap-project-managing-stakeholder-update-001, gap-project-managing-raid-log-001, gap-project-managing-coordinate-001

---

## B37-L2-019-005 — v6-02 boundary: stakeholder DOCUMENT must not be suppressed

A: "Send a weekly status update to stakeholders." 

Predicates: [DOCUMENT]
GapId: gap-project-managing-stakeholder-update-001
DomainId: software-development

Route: DocumentationFlow (2.1)
v6-02 corrupt version: Predicates: [] — DOCUMENT suppressed ("This is communication, not documentation")
Correct: Predicates: [DOCUMENT] — Rule 6 absolute. "stakeholder" fires DOCUMENT. No suppression permitted.
