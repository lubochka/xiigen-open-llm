# Layer 0-B — Router Pairs (022)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-022-001 — managing + stakeholder report: [DOCUMENT] → 2.1

**Input:** "Produce the weekly progress report for stakeholders and coordinate action items."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate (from "stakeholders") → DocumentationFlow (2.1)
- DomainId: "software-development"
- Route: 2.1
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-progress-001
- gap-project-managing-status-report-001
- gap-project-managing-stakeholder-update-001
- gap-project-managing-coordinate-001
- gap-project-managing-action-items-001

---

## B37-0B-022-002 — track + escalate + stakeholders: [DOCUMENT] → 2.1

**Input:** "Track delivery blockers and escalate critical items to stakeholders for sign-off."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate → DocumentationFlow (2.1)
- DomainId: "software-development"

**Gap warnings forwarded:**
- gap-project-managing-track-001
- gap-project-managing-blockers-001
- gap-project-managing-escalation-001
- gap-project-managing-stakeholder-update-001

---

## B37-0B-022-003 — track + escalate, no stakeholder: [] → knowledge_edge

**Input:** "Track delivery blockers and escalate critical items to the programme board."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-track-001, gap-project-managing-blockers-001, gap-project-managing-escalation-001, gap-project-managing-manage-delivery-001

**Routing contrast:** "programme board" does not fire DOCUMENT. Replacing "stakeholders" with "programme board" changes routing from 2.1 to knowledge_edge.

---

## B37-0B-022-004 — coordinate + stakeholder: [DOCUMENT] → 2.1

**Input:** "Coordinate the delivery of stakeholder communications for the programme."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate → DocumentationFlow (2.1)
- DomainId: "software-development"

**Gap warnings forwarded:**
- gap-project-managing-coordinate-001
- gap-project-managing-manage-delivery-001
- gap-project-managing-stakeholder-update-001

---

## B37-0B-022-005 — full managing pile-up + stakeholder: [DOCUMENT] → 2.1

**Input:** "Track blockers, coordinate escalations, follow up on action items, and report status to stakeholders."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate (from "stakeholders") → DocumentationFlow (2.1)
- DomainId: "software-development"
- Route: 2.1 only (single predicate despite rich gap vocabulary)

**Gap warnings forwarded:**
- gap-project-managing-track-001
- gap-project-managing-blockers-001
- gap-project-managing-coordinate-001
- gap-project-managing-escalation-001
- gap-project-managing-follow-up-001
- gap-project-managing-action-items-001
- gap-project-managing-status-report-001
- gap-project-managing-stakeholder-update-001
