# Layer 2 — Capability-Gap-Extended Pairs (021)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-021-001 — "team dependencies blocking progress" gap

A: "Identify and resolve the team dependencies that are blocking delivery progress."

Predicates: []
GapId: gap-project-managing-dependencies-team-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-blockers-001, gap-project-managing-progress-001, gap-project-managing-manage-delivery-001

---

## B37-L2-021-002 — "project dependencies no API" gap

A: "Map all project dependencies and escalate any that are at risk to the programme director."

Predicates: []
GapId: gap-project-managing-dependencies-team-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-escalation-001, gap-project-managing-manage-delivery-001

---

## B37-L2-021-003 — "API dependencies" — API_CONTRACT fires (contrast)

A: "Document the API dependencies between the order management service and the inventory endpoint."

Predicates: [DOCUMENT, API_CONTRACT, IMPLEMENT]
GapId: (none — all covered by predicates)
DomainId: software-development

Routes: DocumentationFlow (2.1) + APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)
Note: "document" fires DOCUMENT; "API" + "endpoint" co-occurrence fires API_CONTRACT; "service" fires IMPLEMENT. No gap vocabulary — all tokens covered.

---

## B37-L2-021-004 — "dependencies" with document but no API: DOCUMENT + gap

A: "Document the cross-workstream dependencies and track progress against the dependency resolution plan."

Predicates: [DOCUMENT]
GapId: gap-project-managing-dependencies-team-001
DomainId: software-development

Route: DocumentationFlow (2.1)
CapabilityGapWarnings: gap-project-managing-dependencies-team-001, gap-project-managing-work-stream-001, gap-project-managing-track-progress-001

---

## B37-L2-021-005 — "service dependencies" — IMPLEMENT fires, gap warning (not API_CONTRACT)

A: "Resolve all service-to-service dependencies before the go-live date."

Predicates: [IMPLEMENT]
GapId: gap-project-managing-dependencies-team-001
DomainId: software-development

Route: BackendServiceFlow41 (4.1)
CapabilityGapWarning: gap-project-managing-dependencies-team-001
Note: "service" fires IMPLEMENT twice. "service" is NOT an API co-occurrence token. API_CONTRACT does NOT fire. The dependency gap warning is raised.
