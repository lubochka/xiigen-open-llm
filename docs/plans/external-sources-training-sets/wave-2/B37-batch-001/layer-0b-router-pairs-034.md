# Layer 0-B — Router Pairs (034)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-034-001 — "Gantt chart" + "critical path" + "dependency mapping" combined

**Three-way PM combination routing:**

A: "Maintain the Gantt chart, update the critical path, and map team dependencies."
→ [] → knowledge_edge | project-management | SCL emitted

B: "Review the Gantt chart, validate the critical path, and document dependency mapping."
→ ["QA", "DOCUMENT"] → QA + 2.1 | software-development

C: "Design the Gantt chart structure, map API dependencies, and build the critical path service."
→ ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT"] → 3.1 + 3.3 + 4.1 | software-development

---

## B37-0B-034-002 — "capacity planning" + "resource allocation" + "dependency mapping" combined

**Three-way PM combination routing:**

A: "Run capacity planning, allocate resources, and map team dependencies."
→ [] → knowledge_edge | project-management | SCL emitted

B: "Define capacity planning structure, document resource allocation, and map API dependencies."
→ ["DOCUMENT", "ARCHITECTURE", "API_CONTRACT"] → 2.1 + 3.1 + 3.3 | software-development

C: "Implement the capacity planning, resource allocation, and dependency management service."
→ ["IMPLEMENT"] → 4.1 | software-development

---

## B37-0B-034-003 — "status report" + "milestone tracking" + "SLA tracking" combined

**Three-way PM combination routing:**

A: "Send status report, track milestones, and monitor SLA compliance."
→ [] → knowledge_edge | project-management | SCL emitted

B: "Send status report to stakeholders and review milestone SLA tracking."
→ ["DOCUMENT", "QA"] → 2.1 + QA | software-development

C: "Build the status report, milestone tracking, and SLA monitoring service."
→ ["IMPLEMENT"] → 4.1 | software-development

---

## B37-0B-034-004 — "vendor management" + "budget tracking" + "critical path" combined

**Three-way PM combination routing:**

A: "Manage vendors, track budget, and identify the critical path."
→ [] → knowledge_edge | project-management | SCL emitted

B: "Review vendor SLA, validate budget spend, and document critical path criteria."
→ ["QA", "DOCUMENT"] → QA + 2.1 | software-development

C: "Build the vendor management, budget tracking, and critical path module."
→ ["IMPLEMENT"] → 4.1 | software-development

---

## B37-0B-034-005 — Parallel route eligibility for Round 2 three-predicate combinations

**Parallel-eligible three-predicate combinations:**

1. ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"] → 2.1 + 3.1 + 4.1 — all parallel eligible
   Example: "Define, design, and build the SLA tracking service."

2. ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"] → 3.3 + 4.1 + 2.1 — all parallel eligible
   Example: "Document the API contract and implement the dependency mapping service."

3. ["QA", "DOCUMENT", "IMPLEMENT"] — QA first or last; DOCUMENT and IMPLEMENT parallel
   Example: "Review, document, and implement the resource allocation module."

4. ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT"] → 3.1 + 3.3 + 4.1 — all parallel eligible
   Example: "Design, specify API, and build the capacity planning system."
