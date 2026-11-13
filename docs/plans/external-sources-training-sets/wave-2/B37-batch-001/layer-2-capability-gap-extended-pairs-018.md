# Layer 2 — Capability-Gap-Extended Pairs (018)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-018-001 — "define accountability structure" — ARCHITECTURE fires + gap warning

A: "Define the accountability structure for the project governance model."

Predicates: [DOCUMENT, ARCHITECTURE]
GapId: gap-project-managing-accountability-001
DomainId: software-development

Routes: DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-accountability-001
Note: "accountability structure" fires ARCHITECTURE on "structure". Governance intent not addressed by ArchitectureFlow31. "define" fires DOCUMENT.

---

## B37-L2-018-002 — "establish accountability" gap (no structure)

A: "Establish clear accountability across all workstream leads for delivery milestones."

Predicates: []
GapId: gap-project-managing-accountability-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-L2-018-003 — "coordinate accountability + escalation" gap

A: "Coordinate accountability and escalation protocols across all programme workstreams."

Predicates: []
GapId: gap-project-managing-escalation-accountability-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-coordinate-001, gap-project-managing-accountability-001, gap-project-managing-escalation-001, gap-project-managing-work-stream-001

---

## B37-L2-018-004 — "map accountability structure" — ARCHITECTURE fires + gap warning

A: "Map the accountability structure and escalation hierarchy for the programme."

Predicates: [ARCHITECTURE]
GapId: gap-project-managing-accountability-001
DomainId: software-development

Route: ArchitectureFlow31 (3.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: gap-project-managing-accountability-001, gap-project-managing-escalation-001

---

## B37-L2-018-005 — "build RACI matrix" — IMPLEMENT fires + gap warning

A: "Build the RACI matrix to clarify accountability for all project activities."

Predicates: [IMPLEMENT]
GapId: gap-project-managing-accountability-001
DomainId: software-development

Route: BackendServiceFlow41 (4.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-accountability-001
Note: "build" fires IMPLEMENT on lexical token. RACI matrix is a project artefact but IMPLEMENT fires regardless of intent.
