# Layer 2 — Capability-Gap-Extended Pairs (017)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-017-001 — "run lessons-learned session" gap

A: "Run the lessons-learned session at the end of the project and collect feedback from all workstream leads."

Predicates: []
GapId: gap-project-managing-manage-delivery-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-work-stream-001

---

## B37-L2-017-002 — "track lessons-learned action items" gap

A: "Track the lessons-learned action items and follow up with owners to ensure completion."

Predicates: []
GapId: gap-project-managing-manage-delivery-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-track-001, gap-project-managing-action-items-001, gap-project-managing-follow-up-001, gap-project-managing-accountability-001

---

## B37-L2-017-003 — "review lessons learned" — QA fires + gap warning

A: "Review the lessons learned from the previous delivery programme and validate the improvement actions."

Predicates: [QA]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Route: ReviewFlow (QA sub-phase)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-action-items-001

---

## B37-L2-017-004 — "document lessons learned + stakeholder" — DOCUMENT fires + gap warning

A: "Document the lessons learned from the delivery programme and circulate to all project stakeholders."

Predicates: [DOCUMENT]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Route: DocumentationFlow (2.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001

---

## B37-L2-017-005 — "define lessons-learned process structure" — DOCUMENT + ARCHITECTURE fire + gap warning

A: "Define the lessons-learned process structure for all future project deliveries."

Predicates: [DOCUMENT, ARCHITECTURE]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Routes: DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-manage-delivery-001
Note: "process structure" fires ARCHITECTURE on "structure" — false positive even in lessons-learned context.
