# Layer 2 — Capability-Gap-Extended Pairs (015)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-015-001 — "create the project charter" — IMPLEMENT fires (false positive) + gap warning

A: "Create the project charter for the new platform migration programme."

Predicates: [IMPLEMENT]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Route: BackendServiceFlow41 (4.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-manage-delivery-001
Note: "create" fires IMPLEMENT lexically even for document creation. False positive IMPLEMENT.

---

## B37-L2-015-002 — "circulate the project charter" gap

A: "Circulate the project charter and collect sign-off from all workstream leads."

Predicates: []
GapId: gap-project-managing-manage-delivery-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-work-stream-001, gap-project-managing-accountability-001

---

## B37-L2-015-003 — "share project charter with stakeholders" — DOCUMENT fires + gap warning

A: "Share the project charter with all stakeholders and confirm the delivery timeline."

Predicates: [DOCUMENT]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Route: DocumentationFlow (2.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001

---

## B37-L2-015-004 — "define scope in project charter" — DOCUMENT fires + gap warning

A: "Define the scope and objectives in the project charter before kickoff."

Predicates: [DOCUMENT]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Route: DocumentationFlow (2.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-manage-delivery-001

---

## B37-L2-015-005 — "finalise project charter" gap

A: "Finalise and approve the project charter before the programme kickoff meeting."

Predicates: []
GapId: gap-project-managing-manage-delivery-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
