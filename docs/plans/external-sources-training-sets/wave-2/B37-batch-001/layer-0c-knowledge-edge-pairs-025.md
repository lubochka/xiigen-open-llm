# Layer 0-C — Knowledge-Edge Pairs (025)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-025-001 — "status report" and "progress update" confirmed gap vocabulary

**New Round 2 gap vocabulary (025-036):**

| Token / Phrase | Zero predicates? | Gap ID |
|---------------|-----------------|--------|
| status report | Yes | gap-project-managing-status-report-001 |
| progress update | Yes | gap-project-managing-progress-001 |
| milestone tracking | Yes | gap-project-managing-track-001 + manage-delivery-001 |
| Gantt chart | Yes | gap-project-managing-manage-delivery-001 |
| resource allocation | Yes | gap-project-managing-manage-delivery-001 |
| capacity planning | Yes | gap-project-managing-manage-delivery-001 |
| dependency mapping (no API) | Yes | gap-project-managing-dependencies-team-001 |
| critical path | Yes | gap-project-managing-manage-delivery-001 + track-001 |
| budget tracking | Yes | gap-project-managing-track-001 + manage-delivery-001 |
| vendor management | Yes | gap-project-managing-coordinate-001 + manage-delivery-001 |
| SLA tracking | Yes | gap-project-managing-track-001 + manage-delivery-001 |

All eleven: zero predicates. DomainId "project-management". SCL emitted.

---

## B37-0C-025-002 — "status report" knowledge-edge handling

**Zero-predicate requests for "status report":**

1. "Send the weekly status report." → knowledge_edge | gap-project-managing-status-report-001
2. "Circulate the monthly status report to the team." → knowledge_edge | gap-project-managing-status-report-001
3. "Update the status report with new milestone data." → knowledge_edge | gap-project-managing-status-report-001 + progress-001
4. "Prepare the quarterly status report." → knowledge_edge | gap-project-managing-status-report-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-025-003 — "progress update" knowledge-edge handling

**Zero-predicate requests for "progress update":**

1. "Provide a progress update on the delivery." → knowledge_edge | gap-project-managing-progress-001
2. "Share the progress update with the team leads." → knowledge_edge | gap-project-managing-progress-001
3. "Prepare the weekly progress update." → knowledge_edge | gap-project-managing-progress-001
4. "Gather progress updates from all workstreams." → knowledge_edge | gap-project-managing-progress-001 + work-stream-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-025-004 — "milestone tracking" knowledge-edge handling

**Zero-predicate requests for "milestone tracking":**

1. "Track project milestones." → knowledge_edge | gap-project-managing-track-001 + manage-delivery-001
2. "Update milestone tracking for Q3." → knowledge_edge | same
3. "Monitor milestone progress." → knowledge_edge | gap-project-managing-track-001 + progress-001
4. "Maintain the milestone log." → knowledge_edge | gap-project-managing-track-001 + raid-log-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-025-005 — False positive summary for Round 2 gap vocabulary

**DOCUMENT category:**
- status report + stakeholders/define/document → DOCUMENT fires
- progress update + stakeholders/define/document → DOCUMENT fires
- milestone tracking + define/criteria → DOCUMENT fires

**ARCHITECTURE category:**
- resource allocation structure/design → ARCHITECTURE fires
- capacity planning structure/design → ARCHITECTURE fires
- dependency mapping architecture → ARCHITECTURE fires

**QA category:**
- review milestone tracking → QA fires
- review Gantt chart → QA fires
- validate SLA compliance → QA fires

**IMPLEMENT category:**
- create/build Gantt chart → IMPLEMENT fires
- implement/build SLA tracking service → IMPLEMENT fires
- write resource allocation module → IMPLEMENT fires

**API_CONTRACT category:**
- dependency mapping + API co-occurrence → API_CONTRACT fires
- SLA tracking + API contract → API_CONTRACT fires
