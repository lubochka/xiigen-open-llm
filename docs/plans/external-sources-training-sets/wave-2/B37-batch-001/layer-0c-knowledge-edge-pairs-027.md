# Layer 0-C — Knowledge-Edge Pairs (027)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-027-001 — "critical path" knowledge-edge handling

**Zero-predicate requests for "critical path":**

1. "Maintain the critical path." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Update the critical path analysis." → knowledge_edge | gap-project-managing-manage-delivery-001
3. "Track critical path milestones." → knowledge_edge | gap-project-managing-track-001 + manage-delivery-001
4. "Resolve critical path blockers." → knowledge_edge | gap-project-managing-blockers-001 + manage-delivery-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-027-002 — "budget tracking" knowledge-edge handling

**Zero-predicate requests for "budget tracking":**

1. "Track the programme budget." → knowledge_edge | gap-project-managing-track-001 + manage-delivery-001
2. "Monitor budget consumption." → knowledge_edge | gap-project-managing-manage-delivery-001
3. "Manage budget tracking across workstreams." → knowledge_edge | gap-project-managing-manage-delivery-001 + work-stream-001
4. "Escalate budget overruns to the programme board." → knowledge_edge | gap-project-managing-escalation-001 + manage-delivery-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-027-003 — "vendor management" knowledge-edge handling

**Zero-predicate requests for "vendor management":**

1. "Coordinate vendor management." → knowledge_edge | gap-project-managing-coordinate-001
2. "Track vendor deliverables." → knowledge_edge | gap-project-managing-track-001 + manage-delivery-001
3. "Manage SLA compliance with vendors." → knowledge_edge | gap-project-managing-manage-delivery-001
4. "Escalate vendor issues to the steering committee." → knowledge_edge | gap-project-managing-escalation-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-027-004 — "SLA tracking" knowledge-edge handling

**Zero-predicate requests for "SLA tracking":**

1. "Monitor SLA compliance." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Track SLA breaches." → knowledge_edge | gap-project-managing-track-001 + manage-delivery-001
3. "Report SLA status to the programme board." → knowledge_edge | gap-project-managing-status-report-001 + manage-delivery-001
4. "Coordinate SLA resolution." → knowledge_edge | gap-project-managing-coordinate-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-027-005 — Round 2 (025-036) knowledge-edge complete reference

**All Round 2 vocabulary → knowledge_edge when used alone:**
status report, progress update, milestone tracking, Gantt chart, resource allocation, capacity planning, dependency mapping (no API), critical path, budget tracking, vendor management, SLA tracking

**SCL emitted for all.** DomainId "project-management" for all.

**Exception triggers (SCL NOT emitted):**
review, validate, define, document, structure, design, stakeholder, build, create, implement, service, module, API + dependencies
