# Layer 0-B — Router Pairs (019)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-019-001 — stakeholder update: [DOCUMENT] → 2.1 (DOCUMENT always fires)

**Input:** "Send a status update to all project stakeholders confirming the delivery is on track."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate (from "stakeholders") → DocumentationFlow (2.1)
- DomainId: "software-development"
- Route: 2.1
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-status-report-001
- gap-project-managing-track-status-001
- gap-project-managing-stakeholder-update-001
- gap-project-managing-manage-delivery-001

---

## B37-0B-019-002 — stakeholder escalation: [DOCUMENT] → 2.1

**Input:** "Escalate the delivery blocker to senior stakeholders and coordinate an emergency response."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate → DocumentationFlow (2.1)
- DomainId: "software-development"

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-blockers-001
- gap-project-managing-stakeholder-update-001
- gap-project-managing-coordinate-001

---

## B37-0B-019-003 — multiple stakeholder tokens: [DOCUMENT] → 2.1 (one route)

**Input:** "Notify all stakeholders of the schedule change and prepare a stakeholder communication plan."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate → DocumentationFlow (2.1)
- ONE route despite two "stakeholder" occurrences firing DOCUMENT
- DomainId: "software-development"

**Gap warnings forwarded:**
- gap-project-managing-stakeholder-update-001

---

## B37-0B-019-004 — no stakeholder → zero predicates: [] → knowledge_edge

**Input:** "Send a status update to the programme board confirming the delivery is on track."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-status-report-001, gap-project-managing-track-status-001, gap-project-managing-manage-delivery-001

**Key boundary:** "programme board" ≠ "stakeholders". Routing depends entirely on the token "stakeholder" being present.

---

## B37-0B-019-005 — stakeholder with requirements: [DOCUMENT] → 2.1

**Input:** "Gather requirements from stakeholders and track progress against the agreed delivery timeline."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate (from "requirements" + "stakeholders") → DocumentationFlow (2.1)
- DomainId: "software-development"

**Gap warnings forwarded:**
- gap-project-managing-stakeholder-update-001
- gap-project-managing-track-progress-001
- gap-project-managing-manage-delivery-001
