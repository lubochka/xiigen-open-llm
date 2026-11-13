# Layer 0-B — Router Pairs (030)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-030-001 — "budget tracking" + "status" routing: confirming "status" zero-predicate

**Input A:** "Report budget tracking status to the programme board."
- "Report" → NOT DOCUMENT (Rule 3); "budget tracking" → gap; "status" → zero predicates (Rule 4); "programme board" → gap
- Predicates: [] → knowledge_edge | project-management

**Input B:** "Report budget tracking status to stakeholders."
- "Report" → gap; "stakeholders" → DOCUMENT fires
- Predicates: ["DOCUMENT"] → 2.1 | software-development

Contrast: "programme board" does NOT fire DOCUMENT; "stakeholders" always fires DOCUMENT.

---

## B37-0B-030-002 — "progress update" in gap_interruption context

**Scenario:** User is in active DocumentationFlow (2.1), then issues:
"Track progress on milestones and update the Gantt chart."

Router analysis:
- "Track" → NOT TEST → gap
- "progress on milestones" → gap
- "update" → gap
- "Gantt chart" → gap
- Zero predicates

TransitionType: gap_interruption
ContextCarryForward: true (DocumentationFlow context preserved)
Route: knowledge_edge
DomainId: "project-management"
SCL: emitted
After gap handling: DocumentationFlow resumable

---

## B37-0B-030-003 — "resource allocation" in gap_interruption context

**Scenario:** User is in active BackendServiceFlow41 (4.1), then issues:
"Coordinate resource allocation and flag capacity blockers."

Router analysis:
- Zero predicates across all tokens
- TransitionType: gap_interruption
- ContextCarryForward: true (BackendServiceFlow41 context preserved)
- Route: knowledge_edge
- DomainId: "project-management"
- SCL: emitted

---

## B37-0B-030-004 — "vendor management" + "SLA tracking" multi-route

**Input:** "Build the vendor management platform, implement SLA tracking, and document the API contract for the integration service."

Token scan:
- "Build" → IMPLEMENT fires
- "vendor management platform" → gap
- "implement" → IMPLEMENT fires
- "SLA tracking" → gap
- "document" → DOCUMENT fires
- "API contract" → "API" = API co-occurrence → API_CONTRACT fires
- "integration service" → "service" → IMPLEMENT fires; "integration" also API co-occurrence

Predicates: ["IMPLEMENT", "DOCUMENT", "API_CONTRACT"]
Routes: 4.1 + 2.1 + 3.3 (all parallel eligible)
DomainId: "software-development"
GapIds forwarded: coordinate-001, track-001, manage-delivery-001

---

## B37-0B-030-005 — Complete Round 2 router knowledge_edge confirmation

**Knowledge_edge is the ONLY route for all pure Round 2 zero-predicate requests:**

All eleven Round 2 PM vocabulary families route to knowledge_edge when zero predicates fire:
status report, progress update, milestone tracking, Gantt chart maintenance, resource allocation, capacity planning, dependency mapping (no API), critical path, budget tracking, vendor management, SLA tracking.

No new flow factories exist. No "StatusReportFlow", "MilestoneFlow", "GanttFlow", "ResourceAllocationFlow", "CapacityPlanningFlow", "DependencyMappingFlow", "CriticalPathFlow", "BudgetTrackingFlow", "VendorManagementFlow", or "SLATrackingFlow".

Any output claiming one of these factories exists = v6-01 corruption.
