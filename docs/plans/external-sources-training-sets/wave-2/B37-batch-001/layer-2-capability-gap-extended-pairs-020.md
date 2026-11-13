# Layer 2 — Capability-Gap-Extended Pairs (020)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-020-001 — "API dependencies between services" — API_CONTRACT fires

A: "Map all API dependencies between the payment service and the user authentication endpoint."

Predicates: [API_CONTRACT, IMPLEMENT]
GapId: (none — all vocabulary covered by predicates)
DomainId: software-development

Routes: APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: none
Note: "API" and "endpoint" are API co-occurrence tokens. API_CONTRACT fires. "service" fires IMPLEMENT. No gap vocabulary present.

---

## B37-L2-020-002 — "integration dependencies" — API_CONTRACT fires

A: "Identify all integration dependencies between the notification service and the messaging module."

Predicates: [API_CONTRACT, IMPLEMENT]
GapId: (none)
DomainId: software-development

Routes: APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)
Note: "integration" is API co-occurrence token. "service" and "module" fire IMPLEMENT.

---

## B37-L2-020-003 — "team dependencies blocking progress" — zero predicates (gap)

A: "Resolve all team dependencies blocking delivery progress before the milestone."

Predicates: []
GapId: gap-project-managing-dependencies-team-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-blockers-001, gap-project-managing-progress-001, gap-project-managing-manage-delivery-001

---

## B37-L2-020-004 — "cross-workstream dependencies" — zero predicates (gap)

A: "Map cross-workstream dependencies and coordinate resolution with workstream leads."

Predicates: []
GapId: gap-project-managing-dependencies-team-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-work-stream-001, gap-project-managing-coordinate-001

---

## B37-L2-020-005 — "service dependencies" boundary — IMPLEMENT fires (service), not API_CONTRACT

A: "Resolve service dependencies between the authentication module and the payment module."

Predicates: [IMPLEMENT]
GapId: gap-project-managing-dependencies-team-001
DomainId: software-development

Route: BackendServiceFlow41 (4.1)
CapabilityGapWarning: gap-project-managing-dependencies-team-001
Note: "service" fires IMPLEMENT but is NOT an API co-occurrence token. API_CONTRACT does NOT fire. Dependencies gap warning raised because no API co-occurrence present.
