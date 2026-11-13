# Layer 0-B — Router Pairs (028)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-028-001 — "resource allocation" routing disambiguation

**Four routing variants:**

A: "Allocate resources for the delivery workstream." → Predicates: [] → knowledge_edge | project-management
B: "Review resource allocation across teams." → Predicates: ["QA"] → QA sub-phase | software-development
C: "Implement the resource allocation service." → Predicates: ["IMPLEMENT"] → 4.1 | software-development
D: "Design the resource allocation architecture for the workforce module." → Predicates: ["ARCHITECTURE", "IMPLEMENT"] → 3.1 + 4.1 | software-development

---

## B37-0B-028-002 — "critical path" routing disambiguation

**Four routing variants:**

A: "Identify the critical path." → [] → knowledge_edge | project-management
B: "Review the critical path for programme risks." → ["QA"] → QA sub-phase | software-development
C: "Define the critical path acceptance criteria." → ["DOCUMENT", "TEST"] → 2.1 + TEST | software-development
D: "Document the critical path for stakeholders and design the dependency architecture." → ["DOCUMENT", "ARCHITECTURE"] → 2.1 + 3.1 | software-development

---

## B37-0B-028-003 — "budget tracking" routing disambiguation

**Four routing variants:**

A: "Monitor budget tracking." → [] → knowledge_edge | project-management
B: "Review budget tracking and validate spend against plan." → ["QA"] → QA sub-phase | software-development
C: "Build the budget tracking module." → ["IMPLEMENT"] → 4.1 | software-development
D: "Define budget tracking criteria and document the reporting structure for stakeholders." → ["DOCUMENT", "ARCHITECTURE"] → 2.1 + 3.1 | software-development

---

## B37-0B-028-004 — "progress update" routing disambiguation

**Four routing variants:**

A: "Provide a progress update." → [] → knowledge_edge | project-management
B: "Send a progress update to stakeholders." → ["DOCUMENT"] → 2.1 | software-development
C: "Define the progress update template and review reporting criteria." → ["DOCUMENT", "QA"] → 2.1 + QA | software-development
D: "Build the progress update service and document the API for stakeholders." → ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"] → 4.1 + 3.3 + 2.1 | software-development

---

## B37-0B-028-005 — Gap warning forwarding: Round 2 vocabulary full reference

**Round 2 gap IDs forwarded when predicates fire:**

| Round 2 vocabulary present | Gap ID forwarded |
|---------------------------|-----------------|
| status report | gap-project-managing-status-report-001 |
| progress update | gap-project-managing-progress-001 |
| milestone tracking | gap-project-managing-track-001 + manage-delivery-001 |
| Gantt chart | gap-project-managing-manage-delivery-001 |
| resource allocation | gap-project-managing-manage-delivery-001 |
| capacity planning | gap-project-managing-manage-delivery-001 |
| dependency mapping (no API) | gap-project-managing-dependencies-team-001 |
| critical path | gap-project-managing-manage-delivery-001 + track-001 |
| budget tracking | gap-project-managing-track-001 + manage-delivery-001 |
| vendor management | gap-project-managing-coordinate-001 + manage-delivery-001 |
| SLA tracking | gap-project-managing-track-001 + manage-delivery-001 |
