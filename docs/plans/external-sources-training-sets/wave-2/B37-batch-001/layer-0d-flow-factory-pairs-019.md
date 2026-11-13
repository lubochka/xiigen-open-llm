# Layer 0-D — Flow Factory Pairs (019)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-019-001 — stakeholder update: DocumentationFlow (false positive)

**Input:** "Send a status update to all project stakeholders confirming delivery is on track."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT (from "stakeholders") → DocumentationFlow (2.1)
- DomainId: "software-development"
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-status-report-001
- gap-project-managing-track-status-001
- gap-project-managing-stakeholder-update-001
- gap-project-managing-manage-delivery-001

**Factory note:** DocumentationFlow is selected but does NOT address stakeholder notification. The managing intent is captured in gap warnings only.

---

## B37-0D-019-002 — escalate to stakeholders: DocumentationFlow

**Input:** "Escalate the delivery blocker to senior stakeholders."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- Gap warnings: escalation-001, blockers-001, stakeholder-update-001, coordinate-001

---

## B37-0D-019-003 — no stakeholder: knowledge_edge

**Input:** "Send a status update to the programme board confirming delivery is on track."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-status-report-001, gap-project-managing-track-status-001, gap-project-managing-manage-delivery-001

---

## B37-0D-019-004 — stakeholder with requirements: DocumentationFlow

**Input:** "Gather requirements from stakeholders and track progress."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- Gap warnings: stakeholder-update-001, track-progress-001, manage-delivery-001

---

## B37-0D-019-005 — StakeholderNotificationFlow does NOT exist (v6-01 boundary)

**Confirmed absent from factory registry:**
- StakeholderNotificationFlow — does not exist
- StakeholderUpdateFlow — does not exist
- CommunicationFlow — does not exist
- NotificationFlow — does not exist

For stakeholder-containing requests: DocumentationFlow is selected (from DOCUMENT predicate). The factory does not match the intent. This is the B37 false positive — by design.

v6-02 corruption: suppressing DocumentationFlow because "this is communication, not documentation" — WRONG. DocumentationFlow is selected by predicate, not intent.
