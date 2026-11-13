# Layer 2 — Capability-Gap-Extended Pairs (014)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-014-001 — "process the change request" gap

A: "Process the change request and coordinate approval from the change advisory board."

Predicates: []
GapId: gap-project-managing-manage-delivery-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-coordinate-001

---

## B37-L2-014-002 — "track open change requests" gap

A: "Track all open change requests and escalate any overdue items to the project manager."

Predicates: []
GapId: gap-project-managing-manage-delivery-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-track-001, gap-project-managing-escalation-001

---

## B37-L2-014-003 — "document change request + stakeholder" — DOCUMENT fires + gap warning

A: "Document the change request with full impact analysis and stakeholder sign-off requirements."

Predicates: [DOCUMENT]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Route: DocumentationFlow (2.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001

---

## B37-L2-014-004 — "implement approved change request" — IMPLEMENT fires + gap warning

A: "Implement the approved change request to add two-factor authentication to the login service."

Predicates: [IMPLEMENT]
GapId: gap-project-managing-manage-delivery-001
DomainId: software-development

Route: BackendServiceFlow41 (4.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-manage-delivery-001

---

## B37-L2-014-005 — "coordinate change request approval" gap (pure)

A: "Coordinate the change request approval process across all affected workstream leads."

Predicates: []
GapId: gap-project-managing-manage-delivery-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-coordinate-001, gap-project-managing-work-stream-001
