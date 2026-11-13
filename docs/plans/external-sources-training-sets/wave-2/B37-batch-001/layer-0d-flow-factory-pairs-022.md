# Layer 0-D — Flow Factory Pairs (022)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-022-001 — managing + stakeholder: DocumentationFlow (DOCUMENT)

**Input:** "Produce the weekly progress report for stakeholders and coordinate action items."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap warnings: progress-001, status-report-001, stakeholder-update-001, coordinate-001, action-items-001

---

## B37-0D-022-002 — track + escalate + stakeholders: DocumentationFlow

**Input:** "Track blockers and escalate to stakeholders for sign-off."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- Gap warnings: track-001, blockers-001, escalation-001, stakeholder-update-001

---

## B37-0D-022-003 — track + escalate, no stakeholder: knowledge_edge

**Input:** "Track blockers and escalate to the programme board."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: track-001, blockers-001, escalation-001, manage-delivery-001

---

## B37-0D-022-004 — coordinate + stakeholder: DocumentationFlow

**Input:** "Coordinate the delivery of stakeholder communications."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- Gap warnings: coordinate-001, manage-delivery-001, stakeholder-update-001

---

## B37-0D-022-005 — Managing pile-up + stakeholder: ONE factory (DocumentationFlow)

**Input:** "Track blockers, coordinate escalations, follow up on action items, and report to stakeholders."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- ONE DOCUMENT predicate → ONE factory: DocumentationFlow (2.1)
- Despite rich managing vocabulary, only one predicate fires
- DomainId: "software-development"
- Eight gap warnings forwarded (see 0C-022-003)

**Factory note:** The factory selection is lean — one predicate, one factory. The managing vocabulary generates many gap warnings but does not create additional factories.
