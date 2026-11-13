# Layer 0-B — Router Pairs (031)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-031-001 — "capacity planning" + "dependency mapping" combined routing

**Combined requests:**

A: "Run capacity planning and map team dependencies."
→ [] → knowledge_edge | project-management | SCL emitted

B: "Run capacity planning and map API dependencies between services."
→ ["API_CONTRACT", "IMPLEMENT"] → 3.3 + 4.1 | software-development

C: "Design the capacity planning and dependency mapping architecture."
→ ["ARCHITECTURE"] → 3.1 | software-development

D: "Define capacity planning and document dependency criteria for stakeholders."
→ ["DOCUMENT"] → 2.1 | software-development

---

## B37-0B-031-002 — "milestone tracking" + "SLA tracking" combined routing

**Combined requests:**

A: "Track milestones and SLA compliance."
→ [] → knowledge_edge | project-management | SCL emitted

B: "Track milestones, SLA compliance, and budget utilisation."
→ [] → knowledge_edge | project-management | SCL emitted (all three = gap vocabulary)

C: "Review milestone tracking and validate SLA compliance."
→ ["QA"] → QA sub-phase | software-development

D: "Implement milestone and SLA tracking services for the project management platform."
→ ["IMPLEMENT"] → 4.1 | software-development

---

## B37-0B-031-003 — "Gantt chart" + "resource allocation" combined routing

**Combined requests:**

A: "Maintain the Gantt chart and manage resource allocation."
→ [] → knowledge_edge | project-management

B: "Review the Gantt chart and validate resource allocation."
→ ["QA"] → QA sub-phase | software-development

C: "Build the Gantt chart view and implement resource allocation service."
→ ["IMPLEMENT"] → 4.1 | software-development

D: "Design the Gantt chart and resource allocation architecture."
→ ["ARCHITECTURE"] → 3.1 | software-development

---

## B37-0B-031-004 — "critical path" + "budget tracking" combined routing

**Combined requests:**

A: "Identify the critical path and track the budget."
→ [] → knowledge_edge | project-management | SCL emitted

B: "Review the critical path and validate budget spend."
→ ["QA"] → QA sub-phase | software-development

C: "Document the critical path and define budget tracking criteria for stakeholders."
→ ["DOCUMENT"] → 2.1 | software-development

D: "Build the critical path and budget tracking module."
→ ["IMPLEMENT"] → 4.1 | software-development

---

## B37-0B-031-005 — "vendor management" + "progress update" combined routing

**Combined requests:**

A: "Manage vendors and provide progress updates."
→ [] → knowledge_edge | project-management | SCL emitted

B: "Provide progress updates to stakeholders on vendor SLA compliance."
→ ["DOCUMENT"] → 2.1 | software-development (stakeholders fires)

C: "Review vendor performance and document progress update findings."
→ ["QA", "DOCUMENT"] → QA + 2.1 | software-development

D: "Build the vendor performance progress update reporting service."
→ ["IMPLEMENT"] → 4.1 | software-development
