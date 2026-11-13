# Layer 0-B — Router Pairs (032)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-032-001 — "status report" + "progress update" combined routing

**Combined requests:**

A: "Circulate the status report and progress update to the team."
→ [] → knowledge_edge | project-management | SCL emitted

B: "Circulate the status report and progress update to stakeholders."
→ ["DOCUMENT"] → 2.1 | software-development (stakeholders fires)

C: "Define the status report and progress update templates."
→ ["DOCUMENT"] → 2.1 | software-development

D: "Build the status report and progress update service."
→ ["IMPLEMENT"] → 4.1 | software-development

---

## B37-0B-032-002 — Five parallel-eligible factory combinations for Round 2

**Parallel-eligible factory pairs in Round 2 B37 routing:**

1. [DOCUMENT, ARCHITECTURE] → DocumentationFlow (2.1) + ArchitectureFlow31 (3.1) — parallel eligible
   Example: "Define the Gantt chart structure for stakeholders."

2. [API_CONTRACT, IMPLEMENT] → APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1) — parallel eligible
   Example: "Implement SLA tracking with API contract for vendor services."

3. [DOCUMENT, IMPLEMENT] → DocumentationFlow (2.1) + BackendServiceFlow41 (4.1) — parallel eligible
   Example: "Create a Gantt chart and document it."

4. [QA, DOCUMENT] → ReviewFlow + DocumentationFlow — ordered (QA typically before or after DOCUMENT)
   Example: "Review and document the critical path."

5. [DOCUMENT, ARCHITECTURE, IMPLEMENT] → all three — parallel eligible
   Example: "Define, design, and build the resource allocation service."

---

## B37-0B-032-003 — Routing when B37 base vocabulary mixes with Round 2 vocabulary

**Base vocabulary (002-024) + Round 2 vocabulary (025-036) combined:**

A: "Track progress, coordinate workstreams, and report status milestones."
→ [] → knowledge_edge | project-management (all base + Round 2 gap vocabulary)

B: "Track progress, coordinate workstreams, and send status milestone report to stakeholders."
→ ["DOCUMENT"] → 2.1 | software-development (stakeholders fires)

C: "Escalate risk log items and track SLA compliance."
→ [] → knowledge_edge | project-management (escalation + risk log + SLA = all gap)

D: "Review the risk log, validate SLA compliance, and define milestone criteria."
→ ["QA", "DOCUMENT"] → QA + 2.1 | software-development

---

## B37-0B-032-004 — "dependency mapping" API boundary routing confirmation

**Dependency co-occurrence routing matrix:**

| Dependency context | API co-occurrence? | Predicates | Route |
|-------------------|-------------------|-----------|-------|
| team/project/workstream | No | [] | knowledge_edge |
| team/project/workstream | No | [] | knowledge_edge |
| service dependencies | No API token | [IMPLEMENT] ("service") | 4.1 |
| API dependencies | Yes (api) | [API_CONTRACT] | 3.3 |
| REST API dependencies + service | Yes (REST, api, service) | [API_CONTRACT, IMPLEMENT] | 3.3 + 4.1 |
| contract dependencies | No API token | [] | knowledge_edge |
| integration dependencies | Yes (integration) | [API_CONTRACT] | 3.3 |
| webhook dependencies | Yes (webhook) | [API_CONTRACT] | 3.3 |

---

## B37-0B-032-005 — Round 2 router certification: 10 questions

Q1: "Run capacity planning." → Route? A: knowledge_edge | project-management
Q2: "Design capacity planning structure." → Route? A: 3.1 (ARCHITECTURE) | software-development
Q3: "Track milestones and SLA compliance." → Route? A: knowledge_edge | project-management
Q4: "Review SLA compliance." → Route? A: QA sub-phase | software-development
Q5: "Map team dependencies." → Route? A: knowledge_edge | project-management
Q6: "Map REST API dependencies between services." → Route? A: 3.3 + 4.1 | software-development
Q7: "Update the Gantt chart." → Route? A: knowledge_edge | project-management
Q8: "Create a Gantt chart." → Route? A: 4.1 (IMPLEMENT) | software-development
Q9: "Report budget status to programme board." → Route? A: knowledge_edge (report ≠ DOCUMENT) | project-management
Q10: "Report budget status to stakeholders." → Route? A: 2.1 (DOCUMENT) | software-development
