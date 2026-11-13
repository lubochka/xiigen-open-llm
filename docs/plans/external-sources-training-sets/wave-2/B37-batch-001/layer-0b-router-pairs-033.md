# Layer 0-B — Router Pairs (033)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-033-001 — "budget tracking" routing with "define" and "criteria"

**Routing for budget + define/criteria combinations:**

A: "Track the budget and report to the programme board." → [] → knowledge_edge
B: "Define budget tracking criteria." → ["DOCUMENT"] → 2.1 (define + criteria both fire DOCUMENT)
C: "Define and validate budget tracking criteria." → ["DOCUMENT", "QA"] → 2.1 + QA (define → DOCUMENT; validate → QA)
D: "Build the budget tracking module with define criteria API." → ["IMPLEMENT", "DOCUMENT", "API_CONTRACT"] → 4.1 + 2.1 + 3.3

---

## B37-0B-033-002 — "vendor management" routing with "contract" boundary

**Contract token routing boundary:**

A: "Manage vendor contracts." → [] → knowledge_edge (contract alone ≠ API_CONTRACT)
B: "Manage the vendor REST API contract." → ["API_CONTRACT"] → 3.3 (REST + API = co-occurrence)
C: "Manage the vendor OpenAPI integration contract." → ["API_CONTRACT"] → 3.3 (OpenAPI + integration = co-occurrence)
D: "Manage vendor contracts and build the contract service." → ["IMPLEMENT"] → 4.1 (service fires IMPLEMENT; contract alone still gap)

---

## B37-0B-033-003 — "resource allocation" + "milestone tracking" combined routing

**A:** "Allocate resources and track milestones for the delivery workstream."
→ [] → knowledge_edge | project-management | SCL emitted

**B:** "Review resource allocation against milestone targets."
→ ["QA"] → QA sub-phase | software-development

**C:** "Build the resource allocation and milestone tracking service."
→ ["IMPLEMENT"] → 4.1 | software-development

**D:** "Define resource allocation criteria and document milestone tracking for stakeholders."
→ ["DOCUMENT"] → 2.1 | software-development

---

## B37-0B-033-004 — "progress update" + "escalation" combined routing

**A:** "Provide progress updates and escalate blockers to the programme board."
→ [] → knowledge_edge | project-management | SCL emitted (all gap vocabulary)

**B:** "Send progress updates and escalation reports to stakeholders."
→ ["DOCUMENT"] → 2.1 | software-development (stakeholders fires)

**C:** "Review progress updates and validate escalation procedures."
→ ["QA"] → QA sub-phase | software-development

**D:** "Document the escalation process and define progress update criteria."
→ ["DOCUMENT"] → 2.1 | software-development (document + define + criteria)

---

## B37-0B-033-005 — Round 2 absent factory confirmation

**Non-existent factories for Round 2 vocabulary:**
- StatusReportFlow, StatusReportingFlow, WeeklyReportFlow
- ProgressUpdateFlow, MilestoneProgressFlow
- MilestoneTrackingFlow, MilestoneManagementFlow
- GanttChartFlow, GanttManagementFlow
- ResourceAllocationFlow, CapacityPlanningFlow
- DependencyMappingFlow, DependencyManagementFlow
- CriticalPathFlow, CriticalPathAnalysisFlow
- BudgetTrackingFlow, BudgetManagementFlow
- VendorManagementFlow, VendorOnboardingFlow
- SLATrackingFlow, SLAMonitoringFlow, SLAComplianceFlow

None of these exist. Any output claiming one = v6-01 corruption.

B37 routing for Round 2 vocabulary: knowledge_edge when zero predicates; existing registered flows when predicates fire.
