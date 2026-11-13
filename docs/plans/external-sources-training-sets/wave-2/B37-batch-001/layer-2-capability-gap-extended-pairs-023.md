# Layer 2 — Capability-Gap-Extended Pairs (023)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-023-001 — Multiple managing terms + code predicate disambiguation

A: "Build the escalation tracking service and maintain the risk log and decision log."

Predicates: [IMPLEMENT]
GapId: gap-project-managing-escalation-001
DomainId: software-development

Route: BackendServiceFlow41 (4.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: gap-project-managing-escalation-001, gap-project-managing-track-001, gap-project-managing-raid-log-001, gap-project-managing-manage-delivery-001
Note: "build" and "service" fire IMPLEMENT. All managing vocabulary (escalation, tracking, risk log, decision log) becomes gap warnings. DomainId "software-development" because IMPLEMENT fired.

---

## B37-L2-023-002 — Managing + IMPLEMENT + accountability structure

A: "Implement the accountability tracking module and define the governance structure."

Predicates: [IMPLEMENT, DOCUMENT, ARCHITECTURE]
GapId: gap-project-managing-accountability-001
DomainId: software-development

Routes: BackendServiceFlow41 (4.1) + DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
CapabilityGapWarnings: gap-project-managing-accountability-001, gap-project-managing-track-001
Note: "implement" and "module" fire IMPLEMENT; "define" fires DOCUMENT; "structure" fires ARCHITECTURE. Three predicates, three factories.

---

## B37-L2-023-003 — Full managing pile-up, no code predicates

A: "Track the risk log, coordinate change requests, follow up on lessons learned, and escalate blockers."

Predicates: []
GapId: gap-project-managing-track-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-raid-log-001, gap-project-managing-coordinate-001, gap-project-managing-manage-delivery-001, gap-project-managing-follow-up-001, gap-project-managing-escalation-001, gap-project-managing-blockers-001

---

## B37-L2-023-004 — Code predicate with managing framing (disambiguation test)

A: "Create the project tracking service for monitoring change requests and decision log updates."

Predicates: [IMPLEMENT]
GapId: gap-project-managing-track-001
DomainId: software-development

Route: BackendServiceFlow41 (4.1)
CapabilityGapWarnings: gap-project-managing-track-001, gap-project-managing-manage-delivery-001
Note: "create" and "service" fire IMPLEMENT. "project tracking", "change requests", "decision log" are gap vocabulary. Managing framing does not prevent IMPLEMENT from firing.

---

## B37-L2-023-005 — Managing terms + QA predicate disambiguation

A: "Review the accountability structure and escalation protocols for the risk log."

Predicates: [QA, ARCHITECTURE]
GapId: gap-project-managing-accountability-001
DomainId: software-development

Routes: ReviewFlow (QA sub-phase) + ArchitectureFlow31 (3.1)
CapabilityGapWarnings: gap-project-managing-accountability-001, gap-project-managing-escalation-001, gap-project-managing-raid-log-001
Note: "review" fires QA; "structure" fires ARCHITECTURE. Two predicates; two flows. Managing vocabulary (accountability, escalation, risk log) raises gap warnings.
