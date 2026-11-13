# Layer 0-B — Router Pairs (027)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-027-001 — "status report" routing disambiguation

**Three routing variants for "status report":**

Variant 1: "Send the weekly status report."
- Predicates: [] → knowledge_edge → DomainId "project-management"

Variant 2: "Document the status report template."
- Predicates: ["DOCUMENT"] → DocumentationFlow (2.1) → DomainId "software-development"

Variant 3: "Review and define the status report acceptance criteria."
- Predicates: ["QA", "DOCUMENT", "TEST"] → QA + 2.1 + TEST → DomainId "software-development"

---

## B37-0B-027-002 — "Gantt chart" routing: three variants

**Variant 1:** "Maintain the Gantt chart for the programme."
→ knowledge_edge | project-management | SCL emitted

**Variant 2:** "Review the Gantt chart against delivery milestones."
→ QA sub-phase | software-development | gap warnings forwarded

**Variant 3:** "Create a Gantt chart and document it for stakeholders."
→ 4.1 + 2.1 (IMPLEMENT + DOCUMENT) | software-development | gap warnings forwarded

---

## B37-0B-027-003 — "capacity planning" routing: three variants

**Variant 1:** "Run capacity planning for next quarter."
→ knowledge_edge | project-management | SCL emitted

**Variant 2:** "Design the capacity planning structure for the programme."
→ 3.1 (ARCHITECTURE) | software-development | gap-manage-delivery-001 forwarded

**Variant 3:** "Define and document capacity planning criteria for stakeholders."
→ 2.1 (DOCUMENT) | software-development | gap-manage-delivery-001 forwarded

---

## B37-0B-027-004 — "dependency mapping" routing: four variants

**Variant 1:** "Map team dependencies across workstreams."
→ knowledge_edge | project-management | gap-dependencies-team-001 | SCL emitted

**Variant 2:** "Map API dependencies between the auth and payment services."
→ 3.3 + 4.1 (API_CONTRACT + IMPLEMENT) | software-development

**Variant 3:** "Design the dependency mapping architecture."
→ 3.1 (ARCHITECTURE) | software-development | gap-dependencies-team-001 forwarded

**Variant 4:** "Document the dependency map for stakeholders."
→ 2.1 (DOCUMENT) | software-development | gap-dependencies-team-001 forwarded

---

## B37-0B-027-005 — "vendor management" + "SLA tracking" combined routing

**Routing for combined vendor/SLA requests:**

| Input | Predicates | Route | DomainId |
|-------|-----------|-------|---------|
| Manage vendors, track SLAs | [] | knowledge_edge | project-management |
| Review vendor SLA compliance | [QA] | QA sub-phase | software-development |
| Build vendor SLA tracking service | [IMPLEMENT] | 4.1 | software-development |
| Define SLA criteria for stakeholders | [DOCUMENT] | 2.1 | software-development |
| Implement vendor API contract module | [IMPLEMENT, API_CONTRACT] | 4.1 + 3.3 | software-development |
| Review and document SLA breaches | [QA, DOCUMENT] | QA + 2.1 | software-development |
