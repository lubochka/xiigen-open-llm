# Layer 2 — Capability-Gap-Extended Pairs (016)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-016-001 — "update the decision log" gap

A: "Update the decision log with all key decisions made during the steering committee meeting."

Predicates: []
GapId: gap-project-managing-manage-delivery-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-accountability-001

---

## B37-L2-016-002 — "track decisions and follow up" gap

A: "Track all open decisions in the decision log and follow up with owners on outstanding items."

Predicates: []
GapId: gap-project-managing-manage-delivery-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-track-001, gap-project-managing-follow-up-001, gap-project-managing-accountability-001

---

## B37-L2-016-003 — "document architectural decisions" — DOCUMENT + ARCHITECTURE fire + gap warning

A: "Document all architectural decisions in the decision log with rationale and alternatives."

Predicates: [DOCUMENT, ARCHITECTURE]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Routes: DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-manage-delivery-001

---

## B37-L2-016-004 — "review decision log" — QA fires + gap warning

A: "Review the decision log before the programme board and confirm all decisions have been actioned."

Predicates: [QA]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Route: ReviewFlow (QA sub-phase)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: gap-project-managing-manage-delivery-001, gap-project-managing-action-items-001

---

## B37-L2-016-005 — "define decision-making process" — DOCUMENT fires + gap warning

A: "Define the decision-making process for the programme and log all major decisions."

Predicates: [DOCUMENT]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Route: DocumentationFlow (2.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: gap-project-managing-manage-delivery-001, gap-project-managing-accountability-001
