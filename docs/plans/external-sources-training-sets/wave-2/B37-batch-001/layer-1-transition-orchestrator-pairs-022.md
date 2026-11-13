# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-022-001 — managing + stakeholder: documentation transition

**Input:** "Produce the weekly progress report for stakeholders."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: progress-001, status-report-001, stakeholder-update-001

---

## B37-L1-022-002 — track + escalate + stakeholder: documentation transition

**Input:** "Track delivery blockers and escalate to stakeholders for sign-off."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: track-001, blockers-001, escalation-001, stakeholder-update-001

---

## B37-L1-022-003 — track + escalate without stakeholder: TransitionType unknown

**Input:** "Track delivery blockers and escalate to the programme board."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: track-001, blockers-001, escalation-001, manage-delivery-001

---

## B37-L1-022-004 — coordinate delivery + stakeholder: documentation transition

**Input:** "Coordinate the delivery of stakeholder communications."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: coordinate-001, manage-delivery-001, stakeholder-update-001

---

## B37-L1-022-005 — managing pile-up + stakeholder: single documentation transition

**Input:** "Track blockers, coordinate escalations, follow up on action items, and report to stakeholders."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation (one predicate → one transition)
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Eight gap warnings forwarded (all managing vocabulary)
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Orchestrator note:** Despite eight gap warnings, there is ONE predicate and ONE transition. The richness of gap vocabulary does not add transitions. Only predicate count drives transition count.
