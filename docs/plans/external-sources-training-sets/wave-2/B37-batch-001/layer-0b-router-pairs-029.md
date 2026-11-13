# Layer 0-B — Router Pairs (029)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-029-001 — "SLA tracking" routing with API co-occurrence rule

**Routing variants for SLA-related requests:**

A: "Track SLA compliance." → [] → knowledge_edge | project-management
B: "Implement SLA tracking." → ["IMPLEMENT"] → 4.1 | software-development
C: "Implement SLA tracking service for the vendor REST API contract." → ["IMPLEMENT", "API_CONTRACT"] → 4.1 + 3.3 | software-development
D: "Define and review SLA tracking criteria." → ["DOCUMENT", "QA"] → 2.1 + QA | software-development

Key: "SLA" alone does NOT trigger API_CONTRACT. "SLA" + "API" or "SLA" + "contract" + API co-occurrence token → API_CONTRACT fires.

---

## B37-0B-029-002 — "milestone tracking" routing with "acceptance" test boundary

**Routing variants:**

A: "Track milestones." → [] → knowledge_edge | project-management
B: "Review milestone tracking." → ["QA"] → QA sub-phase | software-development
C: "Define milestone acceptance criteria." → ["DOCUMENT", "TEST"] → 2.1 + TEST sub-phase | software-development

Note: "acceptance" fires TEST; "criteria" fires DOCUMENT. Together → both TEST and DOCUMENT in predicates.

---

## B37-0B-029-003 — "vendor management" routing: contract token boundary

**Routing variants:**

A: "Manage vendor relationships." → [] → knowledge_edge | project-management
B: "Manage vendor contracts." → [] → "contract" alone without API co-occurrence = gap | project-management
C: "Manage the vendor API contract." → ["API_CONTRACT"] → "API" = API co-occurrence → 3.3 | software-development
D: "Document vendor management criteria for stakeholders." → ["DOCUMENT"] → 2.1 | software-development

Key rule: "contract" alone ≠ API_CONTRACT. Must have API co-occurrence token alongside.

---

## B37-0B-029-004 — Multi-step routing: Round 2 vocabulary + two predicates

**Input:** "Review resource allocation, define capacity planning criteria, and document dependency mapping."

Routing derivation:
- "review" → QA fires
- "resource allocation" → gap
- "define" → DOCUMENT fires
- "capacity planning criteria" → "criteria" → DOCUMENT fires
- "document" → DOCUMENT fires
- "dependency mapping" → gap (no API co-occurrence)

Predicates: ["QA", "DOCUMENT"]
Routes: QA sub-phase + DocumentationFlow (2.1)
DomainId: "software-development"
GapIds forwarded: manage-delivery-001, dependencies-team-001

---

## B37-0B-029-005 — "Gantt chart" + "critical path" combined routing

**Combined sentence routing:**

A: "Maintain the Gantt chart and update the critical path."
→ [] → knowledge_edge | project-management | SCL emitted

B: "Review the Gantt chart and validate the critical path acceptance criteria."
→ ["QA", "DOCUMENT", "TEST"] → QA + 2.1 + TEST | software-development

C: "Create a Gantt chart and document the critical path for stakeholders."
→ ["IMPLEMENT", "DOCUMENT"] → 4.1 + 2.1 | software-development

D: "Design the Gantt chart structure and define the critical path dependency architecture."
→ ["ARCHITECTURE", "DOCUMENT"] → 3.1 + 2.1 | software-development
